# System Users Management View

Widok zarządzania użytkownikami systemowymi (`/system/users`) dla roli **System Developer**.

## Przegląd

Ten widok umożliwia administratorom systemowym (System Developers) kompleksowe zarządzanie wszystkimi użytkownikami w systemie Homely. Implementacja opiera się na specyfikacji z `.ai/prd.md` i `.ai/ui-plan.md`.

## Struktura Komponentów

### Główny Komponent

**`SystemUsersComponent`** (`system-users.component.ts`)
- Główny kontener zarządzający stanem i orkiestrujący podkomponenty
- Odpowiedzialny za lazy loading, routing i integrację z API
- Zarządza listą użytkowników, paginacją i filtrowaniem
- Obsługuje sidebar z szczegółami użytkownika

### Podkomponenty

#### 1. **GlobalUserSearchComponent** (`components/global-user-search/`)
**Cel**: Zaawansowane wyszukiwanie i filtrowanie użytkowników

**Funkcje**:
- Wyszukiwanie po email, imieniu, nazwisku lub gospodarstwie
- Filtrowanie po roli (Admin, Member, Dashboard, System Developer)
- Filtrowanie po statusie (Active, Inactive, Locked, Pending)
- Debounced search (400ms) dla lepszej wydajności
- Przycisk resetowania filtrów

**Interakcje**:
- Automatyczne wyszukiwanie przy zmianie filtrów
- Emit search event z aktualnymi filtrami do rodzica

---

#### 2. **UserDetailsPanelComponent** (`components/user-details-panel/`)
**Cel**: Wyświetlanie szczegółowych informacji o użytkowniku

**Funkcje**:
- Awatar użytkownika z inicjałami
- Pełne dane konta (ID, email, rola, status)
- Informacje o gospodarstwie
- Historia ostatniego logowania
- Daty utworzenia i aktualizacji
- Top 10 ostatnich aktywności użytkownika

**Komponenty UI**:
- PrimeNG Card z customowym headerem
- PrimeNG Tags dla roli i statusu z kolorami
- PrimeNG Table dla historii aktywności
- Loading skeleton podczas ładowania danych

---

#### 3. **RoleManagementFormComponent** (`components/role-management-form/`)
**Cel**: Zarządzanie rolami użytkowników w gospodarstwach

**Funkcje**:
- Dropdown z wszystkimi dostępnymi rolami
- Opisy ról przy wyborze (tooltips)
- Walidacja przed zapisem
- Ostrzeżenie o wpływie zmiany roli
- Potwierdzenie sukcesu/błędu operacji

**Interakcje**:
- Disabled state jeśli rola nie uległa zmianie
- Emit roleUpdated event po pomyślnej zmianie
- Auto-clear success message po 3 sekundach
- Cancel button resetuje formularz

---

#### 4. **AccountActionsToolbarComponent** (`components/account-actions-toolbar/`)
**Cel**: Akcje administracyjne na kontach użytkowników

**Funkcje**:
- Reset hasła z wysyłką email
- Odblokowanie konta (tylko dla locked accounts)
- Confirmation dialogs przed akcjami
- Toast notifications o wyniku operacji

**Bezpieczeństwo**:
- Confirmation dialogs dla wszystkich destruktywnych akcji
- Disabled state podczas wykonywania akcji
- Audit trail w backend (TODO)

---

## Service Layer

### **SystemUsersService** (`core/services/system-users.service.ts`)

**Funkcje API**:
- `searchUsers(filters)` - Wyszukiwanie z paginacją i filtrami
- `getUserDetails(userId)` - Szczegóły pojedynczego użytkownika
- `getUserActivity(userId, limit)` - Historia aktywności
- `resetUserPassword(request)` - Reset hasła
- `unlockUserAccount(userId)` - Odblokowanie konta
- `updateUserRole(request)` - Zmiana roli
- `moveUserToHousehold(userId, householdId)` - Przeniesienie do innego gospodarstwa

**Signals (Reactive State)**:
- `users` - Lista użytkowników
- `selectedUser` - Aktualnie wybrany użytkownik
- `isLoading` - Stan ładowania
- `totalUsers` - Całkowita liczba użytkowników

**Interfaces**:
```typescript
export enum UserRole {
  ADMIN = 'admin',
  MEMBER = 'member',
  DASHBOARD = 'dashboard',
  SYSTEM_DEVELOPER = 'system_developer'
}

export enum UserAccountStatus {
  ACTIVE = 'active',
  INACTIVE = 'inactive',
  LOCKED = 'locked',
  PENDING = 'pending'
}

export interface SystemUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  status: UserAccountStatus;
  householdId: string;
  householdName: string;
  lastLogin: Date | null;
  createdAt: Date;
  updatedAt: Date;
}
```

---

## Routing i Guards

### Route Configuration
```typescript
{
  path: 'system',
  children: [
    {
      path: 'users',
      loadComponent: () => import('./features/system/users/system-users.component'),
      title: 'User Management - Homely System',
      canActivate: [systemDeveloperGuard] // TODO: Uncomment when auth is ready
    }
  ]
}
```

### Guards
**`systemDeveloperGuard`** (`core/guards/system-developer.guard.ts`)
- Weryfikuje czy użytkownik jest zalogowany
- Sprawdza czy ma rolę System Developer (TODO: implement role check)
- Przekierowuje do `/auth/login` jeśli niezalogowany
- Przekierowuje do `/error/403` jeśli brak uprawnień

---

## Integracja API

Wszystkie endpointy API są zdefiniowane w `SystemUsersService`:

### Base URL
```
http://localhost:5000/api/system/users
```

### Endpoints

#### GET `/api/system/users`
**Query Params**:
- `searchTerm` (string) - Wyszukiwanie po email/nazwie
- `role` (UserRole) - Filtr roli
- `status` (UserAccountStatus) - Filtr statusu
- `householdId` (string) - Filtr gospodarstwa
- `page` (number) - Numer strony
- `pageSize` (number) - Rozmiar strony

**Response**:
```json
{
  "users": [SystemUser[]],
  "total": number,
  "page": number,
  "pageSize": number,
  "totalPages": number
}
```

#### GET `/api/system/users/:userId`
**Response**: `SystemUser`

#### GET `/api/system/users/:userId/activity`
**Query Params**: `limit` (number)
**Response**: `UserActivity[]`

#### POST `/api/system/users/:userId/reset-password`
**Body**:
```json
{ "sendEmail": boolean }
```
**Response**:
```json
{ "success": boolean, "message": string }
```

#### POST `/api/system/users/:userId/unlock`
**Response**:
```json
{ "success": boolean, "message": string }
```

#### PUT `/api/system/users/:userId/role`
**Body**:
```json
{
  "householdId": string,
  "role": UserRole
}
```
**Response**: `SystemUser` (updated)

#### POST `/api/system/users/:userId/move`
**Body**:
```json
{ "newHouseholdId": string }
```
**Response**: `SystemUser` (updated)

---

## User Interactions

### Główne Flow

1. **Wyszukiwanie użytkowników**
   - Użytkownik wpisuje frazę w search box
   - System automatycznie wyszukuje po 400ms (debounce)
   - Tabela aktualizuje się z wynikami

2. **Filtrowanie**
   - Wybór roli z dropdown
   - Wybór statusu z dropdown
   - Reset wszystkich filtrów przyciskiem "Reset"

3. **Paginacja**
   - Zmiana strony w PrimeNG Paginator
   - Wybór liczby wyników na stronę (10, 20, 50, 100)

4. **Wyświetlanie szczegółów**
   - Kliknięcie przycisku "View Details" (ikona oka)
   - Otwarcie sidebaru z prawej strony
   - Automatyczne ładowanie szczegółów i historii aktywności

5. **Zmiana roli**
   - W sidebarze wybór nowej roli z dropdown
   - Przycisk "Update Role" aktywny tylko gdy rola się zmieniła
   - Confirmation w formie warning message
   - Toast notification o sukcesie/błędzie

6. **Reset hasła**
   - Przycisk "Reset Password" w toolbar
   - Confirmation dialog
   - Wysyłka email do użytkownika
   - Toast notification o wyniku

7. **Odblokowanie konta**
   - Przycisk "Unlock Account" widoczny tylko dla locked accounts
   - Confirmation dialog
   - Toast notification o wyniku

---

## Styling i Responsywność

### Desktop (>1024px)
- Grid layout dla search form (2fr 1fr 1fr auto)
- Tabela z pełnymi kolumnami
- Sidebar 50rem szerokości

### Tablet (768px - 1024px)
- Grid layout 1fr 1fr dla search form
- Kompaktowa tabela
- Sidebar pełnej szerokości

### Mobile (<768px)
- Stack layout dla wszystkich elementów
- Single column search form
- Kompaktowa tabela ze zmniejszonym padding
- Sidebar pełnoekranowy

### Dark/Light Mode
Wszystkie komponenty używają CSS variables z systemu tematów:
- `--surface-card` - Tło kart
- `--text-color` - Kolor tekstu
- `--surface-border` - Granice
- `--primary-color` - Kolor primary
- etc.

---

## Obsługa Błędów

### HTTP Errors
Service obsługuje wszystkie HTTP error codes:
- `400` - Invalid request parameters
- `401` - Unauthorized access
- `403` - Insufficient permissions
- `404` - User not found
- `500` - Server error

### Error Display
- Toast notifications dla akcji (reset password, unlock)
- Message components w formularzach (role change)
- Console.error dla development debugging

---

## Accessibility (WCAG 2.1)

### Keyboard Navigation
- Wszystkie interaktywne elementy dostępne przez Tab
- Enter/Space dla akcji
- Escape zamyka sidebar i dialogs

### Screen Readers
- ARIA labels na inputach i buttonach
- ARIA live regions dla dynamicznych zmian
- Semantic HTML (proper headings hierarchy)

### Focus Management
- Focus visible z outline
- Focus trap w dialogach
- Auto-focus na ważnych elementach

---

## Bezpieczeństwo

### Frontend
- Input sanitization (Angular domyślnie)
- XSS protection
- CSRF tokens (TODO: implement in backend)
- Role-based rendering
- Disabled states podczas operacji

### Backend (TODO)
- JWT authentication
- Role-based authorization
- Rate limiting
- Audit logging
- Password reset token expiration

---

## TODO / Future Enhancements

1. **Authentication**
   - [ ] Implement full role checking in guard
   - [ ] Add JWT refresh token mechanism

2. **Features**
   - [ ] User impersonation tool
   - [ ] Bulk operations (mass role changes)
   - [ ] Export users to CSV/Excel
   - [ ] Advanced filters (date ranges, custom queries)
   - [ ] User creation from System Developer panel

3. **UX Improvements**
   - [ ] Infinite scroll zamiast paginacji
   - [ ] Real-time updates (WebSocket)
   - [ ] Keyboard shortcuts (/, Ctrl+K dla search)
   - [ ] Recent searches saved in localStorage

4. **Performance**
   - [ ] Virtual scrolling dla dużych list
   - [ ] Optimistic UI updates
   - [ ] Cache search results

5. **Testing**
   - [ ] Unit tests dla komponentów
   - [ ] Integration tests dla service
   - [ ] E2E tests dla głównych flow

---

## Struktura Plików

```
frontend/src/app/features/system/users/
├── system-users.component.ts         # Główny komponent
├── system-users.component.html       # Template
├── system-users.component.scss       # Style
├── README.md                         # Ta dokumentacja
└── components/
    ├── global-user-search/
    │   ├── global-user-search.component.ts
    │   ├── global-user-search.component.html
    │   └── global-user-search.component.scss
    ├── user-details-panel/
    │   ├── user-details-panel.component.ts
    │   ├── user-details-panel.component.html
    │   └── user-details-panel.component.scss
    ├── role-management-form/
    │   ├── role-management-form.component.ts
    │   ├── role-management-form.component.html
    │   └── role-management-form.component.scss
    └── account-actions-toolbar/
        ├── account-actions-toolbar.component.ts
        ├── account-actions-toolbar.component.html
        └── account-actions-toolbar.component.scss
```

---

## Współpraca z Backend

Backend API powinno implementować następujące endpointy zgodnie z powyższą specyfikacją. Przykładowy controller w ASP.NET Core:

```csharp
[ApiController]
[Route("api/system/users")]
[Authorize(Roles = "SystemDeveloper")]
public class SystemUsersController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedUsers>> SearchUsers(
        [FromQuery] UserSearchFilters filters) { ... }

    [HttpGet("{userId}")]
    public async Task<ActionResult<SystemUser>> GetUserDetails(string userId) { ... }

    [HttpGet("{userId}/activity")]
    public async Task<ActionResult<List<UserActivity>>> GetUserActivity(
        string userId, [FromQuery] int limit = 50) { ... }

    [HttpPost("{userId}/reset-password")]
    public async Task<ActionResult> ResetPassword(
        string userId, [FromBody] PasswordResetRequest request) { ... }

    [HttpPost("{userId}/unlock")]
    public async Task<ActionResult> UnlockAccount(string userId) { ... }

    [HttpPut("{userId}/role")]
    public async Task<ActionResult<SystemUser>> UpdateRole(
        string userId, [FromBody] RoleUpdateRequest request) { ... }

    [HttpPost("{userId}/move")]
    public async Task<ActionResult<SystemUser>> MoveToHousehold(
        string userId, [FromBody] MoveHouseholdRequest request) { ... }
}
```

---

## Kontakt / Support

Dla pytań dotyczących implementacji:
- Sprawdź PRD: `.ai/prd.md`
- Sprawdź UI Plan: `.ai/ui-plan.md`
- Sprawdź kod komponentów z komentarzami
