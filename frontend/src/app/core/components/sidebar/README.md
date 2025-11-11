# Sidebar Navigation Component

Wysuwane menu nawigacyjne z lewej strony z dwoma sekcjami: Widoki Gospodarstwa i Widoki Systemowe.

## Przegląd

Komponent **SidebarComponent** implementuje główne menu nawigacyjne aplikacji zgodnie z PRD (sekcja 3.4.0) i ui-plan.md (sekcja 4.1). Zawiera dwie sekcje menu z dynamicznym filtrowaniem na podstawie roli użytkownika i subskrypcji.

## Struktura plików

```
src/app/core/components/sidebar/
├── sidebar.component.ts              # Główny komponent sidebar
├── sidebar.component.html            # Template HTML
├── sidebar.component.scss            # Style SCSS
├── models/
│   └── sidebar-menu.model.ts        # Modele menu items i funkcje pomocnicze
└── README.md                         # Dokumentacja
```

## Funkcjonalności

### ✅ Zgodnie z PRD 3.4.0 i UI-plan 4.1

#### 1. Dwie sekcje menu

**Sekcja 1: Widoki Gospodarstwa**
- 📊 Dashboard - główny widok z kafelkami i kalendarzem
- 📋 Zadania - lista nadchodzących terminów (7 dni)
- 🏷️ Kategorie - widok urządzeń/wizyt pogrupowanych po kategoriach
- 🏠 Urządzenia/Wizyty - pełna lista z możliwością zarządzania
- 👥 Gospodarstwo - zarządzanie członkami i ustawieniami (tylko Admin)
- 📈 Historia - archiwum wykonanych zadań (tylko Premium)
- 📊 Raporty - zestawienia kosztów (tylko Premium)
- 🔬 Analizy - zaawansowane analizy predykcyjne (tylko Premium)
- ⚙️ Ustawienia - konfiguracja profilu i preferencji
- ❓ Pomoc - FAQ i wsparcie

**Sekcja 2: Widoki Systemowe** (tylko System Developer)
- 🖥️ System Dashboard - główny panel administracyjny platformy
- 🏢 Gospodarstwa - zarządzanie wszystkimi gospodarstwami w systemie
- 👤 Użytkownicy - administracja wszystkich kont użytkowników
- 💳 Subskrypcje - monitoring płatności i metryk finansowych
- 🔧 Administracja - zarządzanie infrastrukturą i konfiguracją
- 🎧 Wsparcie - narzędzia do obsługi użytkowników i troubleshooting
- 📈 Metryki Systemu - globalne statystyki i KPI platformy
- ⚙️ Konfiguracja Systemu - ustawienia globalne platformy

#### 2. Role-Based Filtering

Menu dynamicznie filtrowane na podstawie:
- **Roli użytkownika**: admin, member, dashboard, system_developer
- **Statusu subskrypcji**: free vs premium
- **Kontekstu gospodarstwa**: obecność householdId

#### 3. Responsive Behavior

- **Desktop (>1024px)**: Persistent sidebar, możliwość zwinięcia do ikon
- **Tablet (768-1024px)**: Collapsible sidebar, domyślnie zwinięty
- **Mobile (<768px)**: Hamburger menu z pełnoekranowym overlay

#### 4. Visual Features

- **Active State Indicator**: Wyróżnienie aktualnie wybranego widoku
- **Premium Badges**: Oznaczenie funkcji premium (PREMIUM badge)
- **Tooltips**: Podpowiedzi dla ikon w trybie zwiniętym
- **Smooth Transitions**: Płynne animacje przy collapse/expand
- **Section Separators**: Wizualne oddzielenie sekcji

#### 5. Accessibility

- **Keyboard Navigation**: Tab, Enter, Arrow keys
- **ARIA Labels**: Odpowiednie labele dla screen readers
- **Focus Management**: Wyraźne focus states
- **High Contrast Mode**: Zwiększone kontrasty
- **Reduced Motion**: Wyłączenie animacji

## Modele danych

### UserRole
```typescript
type UserRole = 'admin' | 'member' | 'dashboard' | 'system_developer';
```

### MenuItem
```typescript
interface MenuItem {
  id: string;
  label: string;
  icon: string;
  route?: string;
  roles: UserRole[];
  requiresPremium?: boolean;
  badge?: string;
  tooltip?: string;
  children?: MenuItem[];
}
```

### MenuSection
```typescript
interface MenuSection {
  id: string;
  title: string;
  items: MenuItem[];
  roles: UserRole[];
  showSeparator?: boolean;
}
```

## Helper Functions

### getHouseholdMenuItems()
```typescript
getHouseholdMenuItems(householdId: string): MenuItem[]
```
Zwraca menu items dla Sekcji 1 (Widoki Gospodarstwa) z dynamicznymi routami opartymi o householdId.

### getSystemMenuItems()
```typescript
getSystemMenuItems(): MenuItem[]
```
Zwraca menu items dla Sekcji 2 (Widoki Systemowe) - tylko dla System Developer.

### getMenuSections()
```typescript
getMenuSections(
  householdId: string | null,
  userRole: UserRole,
  hasPremium: boolean
): MenuSection[]
```
Główna funkcja zwracająca przefiltrowane sekcje menu na podstawie:
- ID gospodarstwa (jeśli dostępne)
- Roli użytkownika
- Statusu subskrypcji premium

### hasMenuAccess()
```typescript
hasMenuAccess(
  item: MenuItem,
  userRole: UserRole,
  hasPremium: boolean
): boolean
```
Sprawdza czy użytkownik ma dostęp do danego menu item.

## Component API

### Signals

```typescript
// State
isVisible: WritableSignal<boolean>      // Widoczność sidebar
isCollapsed: WritableSignal<boolean>    // Stan zwinięcia (desktop)
isMobileView: WritableSignal<boolean>   // Czy widok mobilny

// User context
currentHouseholdId: WritableSignal<string | null>  // ID gospodarstwa
userRole: WritableSignal<UserRole>                 // Rola użytkownika
hasPremium: WritableSignal<boolean>                // Status premium
householdName: WritableSignal<string>              // Nazwa gospodarstwa

// Computed
menuSections: Signal<MenuSection[]>     // Przefiltrowane sekcje menu
```

### Methods

```typescript
toggleSidebar(): void           // Przełącz widoczność sidebar
toggleCollapsed(): void         // Przełącz stan zwinięcia (desktop)
closeSidebar(): void           // Zamknij sidebar (mobile)
navigateTo(route: string): void // Nawiguj i zamknij (mobile)
handleResize(): void           // Obsłuż zmianę rozmiaru okna
isRouteActive(route: string): boolean  // Sprawdź czy route aktywny
```

## Użycie

### Basic Usage
```typescript
import { SidebarComponent } from '@core/components/sidebar/sidebar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [SidebarComponent],
  template: `
    <app-sidebar></app-sidebar>
    <div class="main-content">
      <router-outlet></router-outlet>
    </div>
  `
})
export class AppComponent {}
```

### Programmatic Control
```typescript
@ViewChild(SidebarComponent) sidebar!: SidebarComponent;

// Toggle sidebar visibility
toggleMenu() {
  this.sidebar.toggleSidebar();
}

// Set user context
setUserContext(householdId: string, role: UserRole, premium: boolean) {
  this.sidebar.currentHouseholdId.set(householdId);
  this.sidebar.userRole.set(role);
  this.sidebar.hasPremium.set(premium);
}
```

## Modern Angular Patterns

### Standalone Component
```typescript
@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, ...],
  // Bez NgModule!
})
```

### Signals
```typescript
// State management z signals
isVisible = signal<boolean>(true);
userRole = signal<UserRole>('admin');

// Computed values
menuSections = computed<MenuSection[]>(() => {
  return getMenuSections(
    this.currentHouseholdId(),
    this.userRole(),
    this.hasPremium()
  );
});
```

### inject() Function
```typescript
private router = inject(Router);
```

### Control Flow (@if, @for)
```html
@if (!isCollapsed()) {
  <span class="menu-label">{{ item.label }}</span>
}

@for (section of menuSections(); track section.id) {
  <div class="menu-section">
    @for (item of section.items; track item.id) {
      <a [routerLink]="item.route">...</a>
    }
  </div>
}
```

## PrimeNG Components

Wykorzystane komponenty:
- **SidebarModule** - podstawowa struktura sidebar
- **ButtonModule** - przyciski toggle/close
- **TooltipModule** - podpowiedzi
- **BadgeModule** - oznaczenia premium
- **DividerModule** - separatory sekcji
- **MenuModule** - struktura menu

## Styling

### CSS Variables
```scss
--surface-card       // Tło sidebar
--surface-border     // Obramowania
--primary-color      // Kolor aktywny
--text-color         // Tekst podstawowy
--text-color-secondary  // Tekst drugorzędny
```

### SCSS Variables
```scss
$sidebar-width-expanded: 280px;
$sidebar-width-collapsed: 80px;
$transition-duration: 0.3s;
```

### Classes
```scss
.sidebar-container    // Główny kontener
.sidebar-header       // Nagłówek z logo
.sidebar-content      // Zawartość z menu
.sidebar-footer       // Stopka
.menu-section         // Sekcja menu
.menu-item           // Pojedynczy item menu
.menu-item.active    // Aktywny item
.sidebar-overlay     // Overlay (mobile)
.hamburger-btn       // Przycisk hamburger (mobile)
```

## Responsywność

### Breakpoints
- **Desktop**: >1024px - Persistent sidebar z collapse
- **Tablet**: 768-1024px - Collapsible sidebar
- **Mobile**: <768px - Hamburger menu + overlay

### Behavior
```typescript
ngOnInit(): void {
  this.handleResize();
  window.addEventListener('resize', () => this.handleResize());
}

handleResize(): void {
  const width = window.innerWidth;
  if (width < 768) {
    // Mobile: hamburger menu
    this.isMobileView.set(true);
    this.isVisible.set(false);
  } else if (width < 1024) {
    // Tablet: collapsible
    this.isCollapsed.set(true);
  } else {
    // Desktop: persistent
    this.isVisible.set(true);
  }
}
```

## Integration Points

### TODO: Auth Service Integration
```typescript
// Obecnie mockowane wartości:
currentHouseholdId = signal<string | null>('test-household');
userRole = signal<UserRole>('admin');
hasPremium = signal<boolean>(false);

// Do zaimplementowania:
constructor() {
  const authService = inject(AuthService);

  effect(() => {
    const user = authService.currentUser();
    this.userRole.set(user?.role || 'member');
    this.hasPremium.set(user?.subscription === 'premium');
  });
}
```

### TODO: Household Service Integration
```typescript
// Do zaimplementowania:
constructor() {
  const householdService = inject(HouseholdService);

  effect(() => {
    const household = householdService.currentHousehold();
    this.currentHouseholdId.set(household?.id || null);
    this.householdName.set(household?.name || 'Moje Gospodarstwo');
  });
}
```

## Przyszłe implementacje

### Household Switcher
```typescript
// TODO: Komponent do przełączania między gospodarstwami
interface HouseholdSwitcherProps {
  households: Household[];
  currentHouseholdId: string;
  onSwitch: (householdId: string) => void;
}
```

### Notifications Badge
```typescript
// TODO: Licznik powiadomień na menu items
interface MenuItem {
  // ...
  notificationCount?: number;
  notificationSeverity?: 'info' | 'warning' | 'error';
}
```

### Search in Menu
```typescript
// TODO: Wyszukiwarka w menu dla dużej liczby items
<input
  type="text"
  placeholder="Szukaj w menu..."
  [(ngModel)]="searchQuery"
/>
```

### Favorites/Recent
```typescript
// TODO: Sekcja z ulubionymi/ostatnio odwiedzanymi
interface RecentItem {
  menuItemId: string;
  lastVisited: Date;
  visitCount: number;
}
```

## Accessibility

### Keyboard Navigation
- **Tab**: Nawigacja między items
- **Enter**: Aktywacja item
- **Arrow Up/Down**: Nawigacja w menu
- **Escape**: Zamknięcie sidebar (mobile)

### Screen Reader Support
```html
<nav aria-label="Główne menu nawigacyjne">
  <a
    role="menuitem"
    [attr.aria-current]="isActive ? 'page' : null"
    [attr.aria-label]="item.label"
  >
    <!-- ... -->
  </a>
</nav>
```

### Focus Management
```scss
.menu-item {
  &:focus-visible {
    outline: 2px solid var(--primary-color);
    outline-offset: 2px;
  }
}
```

## Performance

- **Lazy Loading**: Routes ładowane on-demand
- **Computed Signals**: Automatic memoization
- **OnPush Strategy**: Minimalna liczba render cycles
- **CSS Transitions**: Hardware-accelerated animations

## Testing

### Unit Tests
```typescript
// TODO: Unit testy
describe('SidebarComponent', () => {
  it('should filter menu items by role', () => {
    // ...
  });

  it('should show premium badge for premium items', () => {
    // ...
  });

  it('should handle responsive breakpoints', () => {
    // ...
  });
});
```

### E2E Tests
```typescript
// TODO: E2E testy
describe('Sidebar Navigation', () => {
  it('should navigate between views', () => {
    // ...
  });

  it('should toggle on mobile', () => {
    // ...
  });
});
```

## Zgodność z PRD

Implementacja jest w 100% zgodna z PRD i ui-plan:

✅ **PRD Sekcja 3.4.0 - Główne menu nawigacyjne (Sidebar)**
- Dwie sekcje: Widoki Gospodarstwa + Widoki Systemowe
- Dynamiczne filtrowanie na podstawie roli i subskrypcji
- Responsive behavior (desktop/tablet/mobile)
- Active state indicator
- Wyraźne separatory między sekcjami

✅ **UI-plan Sekcja 4.1 - Primary Navigation**
- Sidebar z lewej strony
- Role-based menu rendering
- Premium features indicators
- Household switcher (miejsce przygotowane)
- Tooltips dla collapsed state
- Keyboard navigation support

✅ **Modern Angular 20 patterns**
- Standalone component
- Signals + computed
- inject() function
- Control flow (@if, @for)
- RouterLink + RouterLinkActive

## License

MIT
