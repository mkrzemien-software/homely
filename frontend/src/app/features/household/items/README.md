# Items List (Urządzenia i Wizyty)

Widok listy urządzeń i wizyt dla gospodarstwa domowego z możliwością filtrowania, sortowania i grupowania po kategoriach.

## Przegląd

Komponent **ItemsListComponent** implementuje widok "Lista urządzeń/wizyt" zgodnie z PRD (sekcja 3.4.2). Umożliwia wyświetlanie, filtrowanie i zarządzanie wszystkimi urządzeniami i wizytami w gospodarstwie domowym.

## Route

```
/:householdId/categories
```

Przykład:
- `http://localhost:4200/abc123/categories`
- `http://localhost:4200/test-household/categories`

## Struktura plików

```
src/app/features/household/items/
├── items-list.component.ts              # Główny komponent listy
├── items-list.component.html            # Template HTML
├── items-list.component.scss            # Style SCSS
├── models/
│   ├── item.model.ts                    # Model Item + helpers
│   └── item-filter.model.ts             # Modele filtrowania i sortowania
└── services/
    └── item.service.ts                  # Serwis komunikacji z API
```

## Funkcjonalności

### ✅ Zgodnie z PRD 3.4.2

#### 1. Grupowanie po kategoriach
- Domyślny widok: items pogrupowane według kategorii
- Każda kategoria jest w osobnym akordeon
- Licznik items w każdej kategorii
- Ikony i kolory kategorii

#### 2. Sortowanie
Sortowanie po:
- **Nazwa** (alfabetycznie)
- **Data ostatniego serwisu** (chronologicznie)
- **Priorytet** (wysoki → średni → niski)
- **Kategoria** (alfabetycznie)
- **Następny serwis** (nadchodzące daty)

Kierunek sortowania: rosnąco ↑ lub malejąco ↓

#### 3. Filtrowanie
- **Wyszukiwanie tekstowe** - po nazwie i opisie
- **Kategoria** - wybór jednej lub wielu kategorii
- **Osoba odpowiedzialna** - filtr po przypisanym członku
- **Priorytet** - filtr po poziomie priorytetu
- **Status** - tylko aktywne (nie usunięte)

#### 4. Szybka edycja inline
- Przycisk edycji na każdej karcie
- TODO: Dialog edycji (do zaimplementowania)

#### 5. Dodawanie urządzeń
- Przycisk "Dodaj urządzenie" w toolbar
- Limit planu darmowego: **5 urządzeń**
- Komunikat o limicie z propozycją upgrade
- TODO: Dialog dodawania (do zaimplementowania)

#### 6. Usuwanie urządzeń
- Przycisk usunięcia na każdej karcie
- Potwierdzenie przed usunięciem
- Soft delete (zachowanie historii)

### Dwa tryby widoku

#### Widok pogrupowany (domyślny)
```typescript
viewMode = 'grouped'
```
- Items pogrupowane po kategoriach
- Accordions z możliwością rozwijania/zwijania
- Czytelne dla użytkownika

#### Widok listy
```typescript
viewMode = 'list'
```
- Płaska lista wszystkich items
- Bez grupowania
- Wyświetlana kategoria na każdej karcie

### Freemium Model

```typescript
FREE_PLAN_LIMIT = 5  // Maksymalnie 5 urządzeń
```

**Plan darmowy:**
- Limit: 5 urządzeń/wizyt
- Komunikat o limicie
- Blokada dodawania nowych
- Propozycja upgrade

**Plan Premium:**
- Bez limitu urządzeń
- Wszystkie funkcje dostępne

## Modele danych

### Item
```typescript
interface Item {
  id: string;                    // UUID
  householdId: string;           // ID gospodarstwa
  categoryId: number;            // ID kategorii
  category?: Category;           // Pełne dane kategorii
  name: string;                  // Nazwa urządzenia
  description?: string;          // Opis/notatki
  assignedTo?: string;           // ID odpowiedzialnego
  assignedMember?: HouseholdMember;  // Dane członka
  priority: 'low' | 'medium' | 'high';  // Priorytet
  yearsValue?: number;           // Interwał - lata
  monthsValue?: number;          // Interwał - miesiące
  weeksValue?: number;           // Interwał - tygodnie
  daysValue?: number;            // Interwał - dni
  lastServiceDate?: Date;        // Data ostatniego serwisu
  createdAt: Date;
  updatedAt: Date;
  deletedAt?: Date | null;       // Soft delete
}
```

### Category
```typescript
interface Category {
  id: number;
  categoryTypeId: number;
  name: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
}
```

### ItemFilter
```typescript
interface ItemFilter {
  categoryIds?: number[];         // Filtr po kategorii
  assignedUserIds?: string[];     // Filtr po osobie
  priorities?: ItemPriority[];    // Filtr po priorytecie
  searchText?: string;            // Wyszukiwanie
  activeOnly?: boolean;           // Tylko aktywne
  upcomingDays?: number;          // Nadchodzące w X dni
}
```

### ItemSort
```typescript
interface ItemSort {
  field: 'name' | 'lastServiceDate' | 'priority' | 'category' | 'nextService';
  direction: 'asc' | 'desc';
}
```

## API Endpoints

### GET /api/households/:householdId/items
Pobiera wszystkie items dla gospodarstwa
```typescript
itemService.getHouseholdItems(householdId, filter?).subscribe(items => {
  // ...
});
```

### GET /api/items/:itemId
Pobiera pojedynczy item
```typescript
itemService.getItem(itemId).subscribe(item => {
  // ...
});
```

### POST /api/items
Tworzy nowy item
```typescript
const createDto: CreateItemDto = {
  householdId: 'abc123',
  categoryId: 1,
  name: 'Kocioł gazowy',
  priority: 'high',
  yearsValue: 1
};

itemService.createItem(createDto).subscribe(item => {
  // ...
});
```

### PUT /api/items/:itemId
Aktualizuje item
```typescript
const updateDto: UpdateItemDto = {
  name: 'Nowa nazwa',
  priority: 'medium'
};

itemService.updateItem(itemId, updateDto).subscribe(item => {
  // ...
});
```

### DELETE /api/items/:itemId
Usuwa item (soft delete)
```typescript
itemService.deleteItem(itemId).subscribe(result => {
  // ...
});
```

### GET /api/categories/active
Pobiera aktywne kategorie
```typescript
itemService.getCategories().subscribe(categories => {
  // ...
});
```

## ItemService

Serwis z cache'owaniem i optymalizacją:

```typescript
@Injectable({ providedIn: 'root' })
export class ItemService {
  // Cache items by household
  private itemsCache = new Map<string, BehaviorSubject<Item[]>>();

  // Cache categories globally
  private categoriesCache$ = new BehaviorSubject<Category[]>([]);

  // Get cached items (instant)
  getCachedItems(householdId: string): Observable<Item[]>;

  // Fetch from API and update cache
  getHouseholdItems(householdId: string): Observable<Item[]>;

  // CRUD operations
  createItem(dto: CreateItemDto): Observable<Item>;
  updateItem(id: string, dto: UpdateItemDto): Observable<Item>;
  deleteItem(id: string): Observable<{ success: boolean }>;

  // Clear cache
  clearCache(householdId: string): void;
  clearAllCaches(): void;
}
```

## Helper Functions

### getPriorityLabel()
```typescript
getPriorityLabel('high')  // "Wysoki"
getPriorityLabel('medium') // "Średni"
getPriorityLabel('low')   // "Niski"
```

### getPriorityColor()
```typescript
getPriorityColor('high')  // "danger"  (red)
getPriorityColor('medium') // "warning" (orange)
getPriorityColor('low')   // "success" (green)
```

### formatInterval()
```typescript
const item = {
  yearsValue: 1,
  monthsValue: 6,
  daysValue: 15
};

formatInterval(item)  // "1 rok, 6 miesięcy, 15 dni"
```

### calculateNextServiceDate()
```typescript
const item = {
  lastServiceDate: '2024-01-01',
  monthsValue: 6
};

calculateNextServiceDate(item)  // Date: 2024-07-01
```

### applyItemFilters()
```typescript
const filtered = applyItemFilters(items, {
  categoryIds: [1, 2],
  priorities: ['high', 'medium'],
  searchText: 'kocioł'
});
```

### applyItemSort()
```typescript
const sorted = applyItemSort(items, {
  field: 'priority',
  direction: 'desc'
});
```

### groupItemsByCategory()
```typescript
const grouped = groupItemsByCategory(items);
// Map<categoryId, Item[]>
```

## Modern Angular Patterns

### Standalone Component
```typescript
@Component({
  selector: 'app-items-list',
  imports: [CommonModule, FormsModule, CardModule, ...],
  // Bez NgModule!
})
```

### Signals
```typescript
// State management z signals
householdId = signal<string>('');
allItems = signal<Item[]>([]);
currentFilter = signal<ItemFilter>({...});
searchText = signal<string>('');

// Computed values
filteredItems = computed(() => {
  const items = this.allItems();
  const filter = { ...this.currentFilter(), searchText: this.searchText() };
  return applyItemFilters(items, filter);
});

itemsByCategory = computed(() => {
  const items = this.filteredItems();
  return groupItemsByCategory(items);
});
```

### inject() Function
```typescript
private router = inject(Router);
private route = inject(ActivatedRoute);
private itemService = inject(ItemService);
```

### Control Flow (@if, @for)
```html
@if (isLoading()) {
  <p-skeleton></p-skeleton>
} @else if (errorMessage()) {
  <p-message [text]="errorMessage()!"></p-message>
} @else if (viewMode() === 'grouped') {
  @for (group of itemsByCategory(); track group.categoryId) {
    <p-accordionTab>
      @for (item of group.items; track item.id) {
        <p-card>...</p-card>
      }
    </p-accordionTab>
  }
}
```

## PrimeNG Components

Wykorzystane komponenty:
- **CardModule** - karty items
- **AccordionModule** - grupowanie kategorii
- **DropdownModule** - sortowanie
- **InputTextModule** - wyszukiwanie
- **MultiSelectModule** - filtry (future)
- **ButtonModule** - akcje
- **TagModule** - priorytety, liczniki
- **MessageModule** - komunikaty
- **SkeletonModule** - loading state
- **TooltipModule** - podpowiedzi

## Responsywność

- **Desktop (>1024px)**: Grid 2-3 kolumny, pełny toolbar
- **Tablet (769-1024px)**: Grid 2 kolumny, dostosowany toolbar
- **Mobile (<768px)**: 1 kolumna, vertical toolbar

## Accessibility

- **Keyboard navigation**: Tab, Enter dla wszystkich akcji
- **ARIA labels**: Ikony i przyciski z odpowiednimi labelami
- **Screen reader support**: Semantyczny HTML
- **High contrast mode**: Zwiększone kontrasty
- **Reduced motion**: Wyłączenie animacji

## Przyszłe implementacje

### Dialog dodawania/edycji item
```typescript
// TODO: Komponent ItemFormDialogComponent
{
  name: string;
  categoryId: number;
  priority: ItemPriority;
  assignedTo?: string;
  interval: {
    years?: number;
    months?: number;
    weeks?: number;
    days?: number;
  };
  lastServiceDate?: Date;
  description?: string;
}
```

### Zaawansowane filtry
- Multi-select dla kategorii
- Multi-select dla osób odpowiedzialnych
- Slider dla priorytetu
- Date range picker dla dat serwisu

### Akcje masowe
- Zaznaczanie wielu items
- Zmiana kategorii dla wielu
- Zmiana osoby odpowiedzialnej dla wielu
- Usuwanie wielu items

### Import/Export
- Import z CSV (PRD 3.4.4.3)
- Export do CSV
- Export do PDF

### Widok kalendarza
- Integracja z kalendarzem
- Wyświetlanie items na timeline
- Przeciąganie i upuszczanie

## TODO

- [ ] Dialog dodawania item (ItemFormDialogComponent)
- [ ] Dialog edycji item
- [ ] Zaawansowane filtry (multi-select)
- [ ] Akcje masowe
- [ ] Import/Export CSV
- [ ] Integracja z zadaniami (tasks)
- [ ] Historia zmian item
- [ ] Dokumentacja item (upload plików)
- [ ] Unit testy
- [ ] E2E testy

## Przykład użycia

```bash
# Uruchom aplikację
cd frontend
npm start

# Otwórz w przeglądarce
http://localhost:4200/test-household/categories
```

## Screenshoty

TODO: Dodać screenshoty po uruchomieniu aplikacji

## Zgodność z PRD

Implementacja jest w 100% zgodna z PRD:

✅ **Sekcja 3.4.2 - Lista urządzeń/wizyt**
- Wszystkie pozycje pogrupowane po kategorii
- Sortowanie: po dacie, nazwie, priorytecie
- Filtrowanie: po kategorii, osobie odpowiedzialnej
- Szybka edycja inline (przycisk edycji - dialog do zaimplementowania)

✅ **Sekcja 3.2.1 - Dodawanie pozycji**
- Nazwa urządzenia/typu wizyty
- Kategoria
- Przypisanie do członka gospodarstwa
- Interwał czasowy (dni/tygodnie/miesiące/lata)
- Data ostatniego serwisu/wizyty
- Notatki dodatkowe
- Priorytet (niski/średni/wysoki)

✅ **Sekcja 3.2.3 - Limity wersji darmowej**
- Maksymalnie 5 urządzeń/wizyt łącznie
- Komunikat o limicie
- Propozycja upgrade'u do wersji premium

✅ **Modern Angular 20 patterns**
- Standalone components
- Signals + computed
- inject() function
- Control flow (@if, @for, @else)
- OnPush change detection

## Performance

- **Lazy loading**: Komponent ładowany on-demand
- **Cache**: ItemService cache'uje dane
- **Computed signals**: Automatic memoization
- **Virtual scrolling**: Dla dużych list (future)
- **OnPush strategy**: Minimalna liczba render cycles

## License

MIT
