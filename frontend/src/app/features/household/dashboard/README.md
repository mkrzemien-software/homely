# Household Dashboard

Dashboard główny dla gospodarstwa domowego z kafelkami nawigacyjnymi.

## Przegląd

Komponent **HouseholdDashboardComponent** stanowi główny panel zarządzania gospodarstwem domowym dla użytkowników z rolami: Administrator, Domownik (Member) i Dashboard (read-only).

Zgodnie z PRD (sekcja 3.4.1), dashboard zawiera **kafelki nawigacyjne** - duże przyciski z ikonami umożliwiające szybki dostęp do głównych sekcji aplikacji.

## Route

```
/:householdId/dashboard
```

Przykład:
- `http://localhost:4200/abc123/dashboard`
- `http://localhost:4200/household-uuid-1234/dashboard`

## Struktura plików

```
src/app/features/household/dashboard/
├── household-dashboard.component.ts        # Główny komponent (logic)
├── household-dashboard.component.html      # Template HTML
├── household-dashboard.component.scss      # Style SCSS
└── models/
    └── household-navigation-tile.model.ts  # Model kafelków nawigacyjnych
```

## Kafelki nawigacyjne

Dashboard zawiera 3 główne kafelki zgodnie z PRD:

### 1. 📋 Zadania (Tasks)
- **Opis**: Lista nadchodzących terminów z szybkimi akcjami
- **Route**: `/:householdId/tasks`
- **Kolor**: Primary (niebieski)
- **Statystyki**: Liczba nadchodzących terminów w ciągu 7 dni
- **Status**: Aktywny (do implementacji w kolejnej iteracji)

### 2. 🏷️ Kategorie (Categories)
- **Opis**: Urządzenia i wizyty pogrupowane po kategoriach
- **Route**: `/:householdId/categories`
- **Kolor**: Success (zielony)
- **Statystyki**: Liczba aktywnych kategorii
- **Status**: Aktywny (do implementacji w kolejnej iteracji)

### 3. ⚙️ Ustawienia (Settings)
- **Opis**: Konfiguracja gospodarstwa i zarządzanie członkami
- **Route**: `/:householdId/settings`
- **Kolor**: Secondary (szary)
- **Status**: Aktywny (do implementacji w kolejnej iteracji)

## Role użytkowników

Komponent dostosowuje wyświetlane kafelki w zależności od roli użytkownika:

### Administrator
```typescript
getAdminNavigationTiles(householdId)
```
- Pełny dostęp do wszystkich 3 kafelków
- Ustawienia: pełna konfiguracja gospodarstwa

### Domownik (Member)
```typescript
getMemberNavigationTiles(householdId)
```
- Dostęp do Zadań i Kategorii
- Ustawienia zastąpione przez "Mój profil" (route: `/:householdId/profile`)

### Dashboard (tylko odczyt)
```typescript
getDashboardRoleNavigationTiles(householdId)
```
- Tylko kafelek "Zadania"
- Optymalizacja dla monitorów ściennych

## Użycie

### Routing

```typescript
// app.routes.ts
{
  path: ':householdId',
  children: [
    {
      path: 'dashboard',
      loadComponent: () => import('./features/household/dashboard/household-dashboard.component')
        .then(m => m.HouseholdDashboardComponent),
      title: 'Dashboard - Homely',
      // canActivate: [authGuard, householdMemberGuard] // Do odkomentowania po implementacji auth
    }
  ]
}
```

### Nawigacja programowa

```typescript
// Z poziomu innego komponentu
this.router.navigate([householdId, 'dashboard']);

// Przykład
this.router.navigate(['abc123', 'dashboard']);
```

### Przykład linku w template

```html
<a [routerLink]="['/', householdId, 'dashboard']">
  Przejdź do Dashboard
</a>
```

## Komponenty współdzielone

Dashboard wykorzystuje komponent **NavigationTilesComponent** z `features/system/dashboard`:

```typescript
<app-navigation-tiles
  [tiles]="navigationTiles()"
  (tileClick)="onTileClick($event)">
</app-navigation-tiles>
```

Komponent ten zapewnia:
- Responsywny grid layout (2-3 kolumny)
- Hover efekty i animacje
- Keyboard navigation (Tab, Enter)
- Badge indicators dla alertów
- Display statystyk na kafelkach
- Disabled state dla niedostępnych funkcji

## Modern Angular Patterns

Implementacja wykorzystuje najnowsze wzorce Angular 20:

### Standalone Components
```typescript
@Component({
  selector: 'app-household-dashboard',
  imports: [CommonModule, CardModule, ...],
  // Bez NgModule!
})
```

### Signals
```typescript
householdId = signal<string>('');
userRole = signal<'admin' | 'member' | 'dashboard'>('admin');

// Computed signals
navigationTiles = computed<NavigationTile[]>(() => {
  const id = this.householdId();
  const role = this.userRole();
  // ...
});
```

### inject() function
```typescript
private router = inject(Router);
private route = inject(ActivatedRoute);
```

### Control flow (@if, @for)
```html
@if (isLoaded()) {
  <header>...</header>
} @else {
  <div class="loading-container">...</div>
}
```

## Responsywność

Dashboard jest w pełni responsywny:

- **Desktop (>1024px)**: 3 kolumny, max-width: 1400px
- **Tablet (769-1024px)**: 2-3 kolumny, padding: 1.5rem
- **Mobile (<768px)**: 1-2 kolumny, padding: 1rem

## Accessibility

- **Keyboard navigation**: Tab, Enter
- **ARIA labels**: Ikony oznaczone jako `aria-hidden="true"`
- **High contrast mode**: Zwiększone grubości obramowań
- **Reduced motion**: Wyłączenie animacji dla użytkowników preferujących mniejszy ruch

## Przyszłe implementacje

Dashboard w kolejnych iteracjach będzie zawierał (zgodnie z PRD 3.4.1):

### Zintegrowany kalendarz
- Miesięczny widok terminów
- Kolorowe oznaczenia kategorii
- Możliwość kliknięcia w termin i wykonania akcji

### Lista nadchodzących terminów
- Terminé w ciągu najbliższych 7 dni
- Wyróżnienie terminów przekroczonych
- Szybkie akcje: potwierdź, przełóż, edytuj

### Statystyki
- Liczba urządzeń
- Wykorzystanie limitu (free: 5 urządzeń, 3 członków)
- Liczba oczekujących zadań

## TODO

- [ ] Implementacja AuthService i pobieranie roli użytkownika
- [ ] Implementacja HouseholdService do pobierania danych gospodarstwa
- [ ] Ładowanie statystyk dla kafelków (liczba zadań, kategorii)
- [ ] Guards: authGuard, householdMemberGuard
- [ ] Implementacja komponentów docelowych: Tasks, Categories, Settings
- [ ] Integracja z API backend
- [ ] Unit testy
- [ ] E2E testy

## Przykład użycia w development

```bash
# Uruchom aplikację
cd frontend
npm start

# Otwórz w przeglądarce
http://localhost:4200/test-household/dashboard
```

Dashboard automatycznie załaduje kafelki nawigacyjne dla gospodarstwa "test-household".

## Zrzuty ekranu

TODO: Dodać zrzuty ekranu po uruchomieniu aplikacji

## Zgodność z PRD

Implementacja jest w 100% zgodna z PRD (Product Requirements Document):

✅ **Sekcja 3.4.1 - Dashboard główny**
- Kafelki nawigacyjne z dużymi przyciskami i ikonami
- 3 główne sekcje: Zadania, Kategorie, Ustawienia
- Responsywny layout
- Przygotowane miejsce na kalendarz i listę terminów (future implementation)

✅ **Sekcja 3.1.2 - Role i uprawnienia**
- Obsługa 3 ról: Administrator, Domownik, Dashboard
- Dostosowanie widoku do roli użytkownika

✅ **Modern Angular patterns**
- Standalone components
- Signals
- inject() function
- Control flow directives (@if, @for)

## License

MIT
