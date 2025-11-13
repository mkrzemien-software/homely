# Architektura UI dla Homely - Aplikacja do Zarządzania Terminami Domowymi

## 1. Przegląd struktury UI

Aplikacja Homely to responsywna aplikacja webowa zbudowana w Angular 20 z PrimeNG, przeznaczona do zarządzania zadaniami domowymi i wydarzeniami. System opiera się na szablonach zadań (Tasks), z których użytkownicy tworzą konkretne wydarzenia (Events) w kalendarzu. Architektura UI opiera się na modelu freemium z role-based access control, obsługując trzy typy użytkowników: Administrator, Domownik i Dashboard (tylko odczyt).

### Kluczowe założenia architektoniczne:
- **Stack technologiczny**: Angular 20 + PrimeNG + CSS Grid/Flexbox
- **Responsywność**: Desktop-first z optymalizacją dla tablet i mobile browser
- **Autoryzacja**: JWT Bearer Token z role-based guards
- **Nawigacja**: Dynamiczna, oparta na rolach z lazy loading
- **Dostępność**: WCAG 2.1 compliance z keyboard navigation i screen reader support
- **Model biznesowy**: Freemium z jasnym rozróżnieniem funkcjonalności premium

## 2. Lista widoków

### 2.1 Authentication Views

#### Login View
- **Ścieżka**: `/auth/login`
- **Cel**: Uwierzytelnienie użytkownika w systemie
- **Kluczowe informacje**: Formularz logowania, opcja "zapamiętaj mnie", linki do rejestracji i resetowania hasła
- **Komponenty**: 
  - LoginForm (email, hasło, walidacja)
  - AuthLinks (rejestracja, reset hasła)
  - ThemeToggle
- **UX/Dostępność**: Focus management, keyboard navigation, clear error messages
- **Bezpieczeństwo**: Rate limiting, CSRF protection, input sanitization

#### Register View
- **Ścieżka**: `/auth/register`
- **Cel**: Rejestracja nowego użytkownika z zgodami RODO
- **Kluczowe informacje**: Formularz rejestracji, informacje o przetwarzaniu danych, zgody
- **Komponenty**:
  - RegisterForm (email, hasło, imię, nazwisko)
  - GDPRConsent
  - PasswordStrengthIndicator
- **UX/Dostępność**: Progressive enhancement, clear validation feedback
- **Bezpieczeństwo**: Password strength validation, email verification flow

#### Reset Password Views
- **Ścieżki**: `/auth/forgot-password`, `/auth/reset-password/:token`
- **Cel**: Bezpieczne resetowanie hasła użytkownika
- **Kluczowe informacje**: Formularz z email, potwierdzenie wysłania, formularz nowego hasła
- **Komponenty**:
  - ForgotPasswordForm
  - ResetPasswordForm
  - SuccessMessage
- **UX/Dostępność**: Clear success/error states, progress indication
- **Bezpieczeństwo**: Token expiration handling, secure password requirements

### 2.2 Main Application Views

#### Dashboard Główny
- **Ścieżka**: `/dashboard`
- **Cel**: Centralny hub z nawigacją kafelkową i przeglądem wydarzeń
- **Kluczowe informacje**: 
  - **Kafelki nawigacyjne** (duże przyciski z ikonami) do głównych widoków:
    - 📅 Wydarzenia - lista nadchodzących wydarzeń (7 dni)
    - 📋 Zadania - zarządzanie szablonami zadań
    - 🏷️ Kategorie - widok kategorii i podkategorii z zadaniami
    - ⚙️ Ustawienia - szybki dostęp do konfiguracji gospodarstwa
  - **Zintegrowany kalendarz** - dostępny jako widget/modal z poziomu dashboardu
  - Wydarzenia na najbliższe 7 dni z oznaczeniem pilności
  - Statystyki wykorzystania limitu zadań (wersja darmowa)
  - Szybkie akcje (potwierdź, przełóż, edytuj, anuluj)
  - Przełącznik gospodarstw (jeśli dostęp do wielu)
- **Komponenty**:
  - NavigationTiles (kafelki do widoków: wydarzenia/zadania/kategorie/ustawienia)
  - CalendarWidget (zintegrowany mini kalendarz z wydarzeniami)
  - UpcomingEventsList z color-coded urgency
  - QuickActionButtons (potwierdź, przełóż, anuluj, szczegóły)
  - UsageStatistics (progress bars dla limitów: zadania, członkowie, storage)
  - HouseholdSwitcher
- **UX/Dostępność**: Auto-refresh co 5 minut, keyboard shortcuts, ARIA live regions, tile-based navigation
- **Bezpieczeństwo**: Role-based event visibility, permission checks na akcjach

**Uwaga**: Kalendarz jest zintegrowany z dashboardem jako widget/modal, nie posiada osobnego widoku.

#### Widok Wydarzeń
- **Ścieżka**: `/events`
- **Cel**: Lista nadchodzących wydarzeń (konkretnych terminów) z możliwością wykonywania akcji
- **Kluczowe informacje**:
  - Lista wydarzeń chronologicznie (7 dni domyślnie)
  - Wyróżnienie kolorystyczne (przekroczony/dzisiaj/nadchodzący)
  - Informacja o powiązanym zadaniu
  - Szybkie akcje na każdym wydarzeniu
  - Filtry (osoba odpowiedzialna, kategoria, priorytet, status)
  - Licznik wydarzeń według statusu
- **Komponenty**:
  - EventsList z filtering
  - EventCard z quick actions
  - EventFilters
  - EventActionButtons (potwierdź, przełóż, edytuj, anuluj)
  - RelatedTaskInfo
- **UX/Dostępność**: Focus management, keyboard shortcuts dla akcji
- **Bezpieczeństwo**: Permission checks per event based on assignment
- **Dostęp przez**: Sidebar (📅 Wydarzenia) lub kafelek na dashboardzie

#### Widok Zadań (Templates)
- **Ścieżka**: `/tasks`
- **Cel**: Zarządzanie szablonami zadań, z których tworzone są wydarzenia
- **Kluczowe informacje**:
  - Lista wszystkich zadań w gospodarstwie
  - Wyświetlanie: nazwa, podkategoria, interwał, priorytet
  - Przycisk "Utwórz wydarzenie" przy każdym zadaniu
  - Filtry (podkategoria, priorytet, z/bez interwału)
  - Sortowanie (nazwa, kategoria, priorytet)
- **Komponenty**:
  - TasksList (szablon Tasks)
  - TaskCard z action "Utwórz wydarzenie"
  - TaskFilters
  - CreateEventFromTaskDialog
  - TaskForm (dodawanie/edycja)
- **UX/Dostępność**: Clear task-event relationship indicators
- **Bezpieczeństwo**: Role-based task management
- **Dostęp przez**: Sidebar (📋 Zadania) lub kafelek na dashboardzie

#### Widok Kategorii
- **Ścieżka**: `/categories`
- **Cel**: Zarządzanie kategoriami (podkategoriami) i typami kategorii, z widokiem przypisanych zadań
- **Kluczowe informacje**:
  - Lista wszystkich kategorii (podkategorii) pogrupowanych po typach
  - **Zarządzanie kategoriami**:
    - Możliwość dodawania nowych kategorii (CreateCategoryDialogComponent)
    - Możliwość edycji istniejących kategorii (EditCategoryDialogComponent)
    - Możliwość usuwania kategorii (soft delete)
    - Przycisk "Dodaj kategorię" w toolbar
  - **Zarządzanie typami kategorii** (Admin/System Developer):
    - Możliwość dodawania nowych typów kategorii (CreateCategoryTypeDialogComponent)
    - Możliwość edycji istniejących typów kategorii (EditCategoryTypeDialogComponent)
    - Ikona ołówka obok nazwy typu kategorii w widoku accordion
    - Przycisk "Dodaj typ kategorii" w toolbar obok "Dodaj kategorię"
  - Filtrowanie kategorii po typie
  - Wyszukiwanie kategorii po nazwie
  - Licznik zadań przypisanych do każdej kategorii (podkategorii)
  - Wyświetlanie w dwóch trybach: pogrupowane lub lista
  - Szybki dostęp do akcji (edytuj, usuń) dla każdej kategorii
  - Możliwość dodawania zadań bezpośrednio do kategorii
- **Komponenty**:
  - CategoriesListComponent (główny komponent widoku)
  - **Dialogi kategorii**:
    - CreateCategoryDialogComponent (dialog dodawania kategorii)
    - EditCategoryDialogComponent (dialog edycji kategorii)
  - **Dialogi typów kategorii**:
    - CreateCategoryTypeDialogComponent (dialog dodawania typu kategorii)
    - EditCategoryTypeDialogComponent (dialog edycji typu kategorii)
  - CategoryGroupedView (widok pogrupowany z zadaniami)
  - CategoryAccordion (accordion dla typów kategorii z ikonami edycji)
  - CategoryCard (karta pojedynczej podkategorii z licznikiem zadań)
  - Filters i search bar
  - TasksPerCategoryList
- **UX/Dostępność**:
  - Accordion navigation dla widoku pogrupowanego
  - Przełączanie między widokiem pogrupowanym a listą
  - Lazy loading dla dużych kategorii
  - Toast notifications dla operacji CRUD (kategorie i typy)
  - Confirmation dialogs dla operacji usuwania
  - Ikony edycji przy typach kategorii nie rozwijają/zwijają accordion (stopPropagation)
  - Dwa przyciski w toolbar: "Dodaj typ kategorii" i "Dodaj kategorię"
- **Bezpieczeństwo**:
  - Role-based visibility (Admin/System Developer)
  - Permission checks dla operacji CRUD (kategorie i typy)
  - Input validation i sanitization w formularzach
  - Walidacja: maksymalna długość nazwy (100 znaków) i opisu (500 znaków)
- **Dostęp przez**: Sidebar (🏷️ Kategorie) lub kafelek na dashboardzie

#### Formularz Zadania
- **Ścieżki**: `/tasks/add`, `/tasks/:id/edit`
- **Cel**: Dodawanie nowych i edycja istniejących szablonów zadań
- **Kluczowe informacje**:
  - Formularz z wszystkimi wymaganymi polami
  - Wybór podkategorii
  - Konfiguracja interwału (opcjonalny)
  - Ustawienie priorytetu i notatek
- **Komponenty**:
  - TaskForm z progressive disclosure
  - IntervalConfiguratorComponent
  - SubcategorySelector
  - PrioritySelector
- **UX/Dostępność**: Form validation feedback, save/cancel actions, preview kolejnego wydarzenia
- **Bezpieczeństwo**: Input validation, permission checks, freemium limits (5 zadań)

#### Szczegóły Wydarzenia
- **Ścieżka**: `/events/:id`
- **Cel**: Wyświetlenie szczegółów wydarzenia z możliwością wykonania akcji
- **Kluczowe informacje**:
  - Pełne informacje o wydarzeniu i powiązanym zadaniu
  - Historia poprzednich wydarzeń tego zadania (premium)
  - Akcje (potwierdź, przełóż, edytuj, anuluj)
  - Informacje o automatycznym generowaniu następnego wydarzenia
- **Komponenty**:
  - EventDetails
  - EventHistory (premium - poprzednie wydarzenia tego zadania)
  - EventActionButtons
  - RelatedTaskInfo
  - NextEventPreview (jeśli zadanie ma interwał)
- **UX/Dostępność**: Clear action hierarchy, confirmation dialogs
- **Bezpieczeństwo**: Event ownership validation, role-based actions

### 2.3 Household Management Views

#### Lista Gospodarstw
- **Ścieżka**: `/households`
- **Cel**: Przegląd dostępnych gospodarstw domowych (dla użytkowników z dostępem do wielu)
- **Kluczowe informacje**:
  - Lista gospodarstw z rolami użytkownika
  - Statystyki każdego gospodarstwa
  - Opcje tworzenia nowego gospodarstwa
- **Komponenty**:
  - HouseholdGrid
  - CreateHouseholdButton
  - HouseholdStats
- **UX/Dostępność**: Card-based layout, clear role indicators
- **Bezpieczeństwo**: Display only accessible households

#### Zarządzanie Gospodarstwem
- **Ścieżka**: `/households/:id/manage`
- **Cel**: Administracja gospodarstwa (tylko dla administratorów)
- **Kluczowe informacje**:
  - Lista członków z rolami
  - Statystyki wykorzystania planu
  - Ustawienia gospodarstwa
  - Zarządzanie subskrypcją
- **Komponenty**:
  - MembersList z role management
  - PlanUsageIndicator
  - HouseholdSettings
  - SubscriptionManager
- **UX/Dostępność**: Clear member hierarchy, role change confirmations
- **Bezpieczeństwo**: Admin-only access, at least one admin requirement

#### Zapraszanie Członków
- **Ścieżka**: `/households/:id/invite`
- **Cel**: Zapraszanie nowych członków do gospodarstwa
- **Kluczowe informacje**:
  - Formularz zaproszenia z email i rolą
  - Informacje o limitach planu
  - Status wysłanych zaproszeń
- **Komponenty**:
  - InviteForm
  - PlanLimitWarning
  - PendingInvitations
- **UX/Dostępność**: Clear limit indicators, email validation
- **Bezpieczeństwo**: Member limit enforcement, email validation

### 2.4 Premium Views

#### Historia Zadań
- **Ścieżka**: `/premium/history`
- **Cel**: Przegląd wykonanych zadań z możliwością analizy (tylko premium)
- **Kluczowe informacje**:
  - Lista wszystkich ukończonych zadań
  - Filtry (data, kategoria, osoba)
  - Statystyki wykonania
- **Komponenty**:
  - TaskHistoryTable
  - HistoryFilters
  - CompletionStats
  - ExportButton
- **UX/Dostępność**: Advanced filtering, export options
- **Bezpieczeństwo**: Premium subscription validation

#### Raporty Kosztów
- **Ścieżka**: `/premium/reports`
- **Cel**: Analiza kosztów związanych z serwisami (tylko premium)
- **Kluczowe informacje**:
  - Zestawienia kosztów według okresów
  - Wykresy wydatków
  - TOP najdroższe urządzenia
- **Komponenty**:
  - CostReports
  - ExpenseCharts
  - CostBreakdown
- **UX/Dostępność**: Interactive charts, data export
- **Bezpieczeństwo**: Premium access control, data privacy

#### Zaawansowane Analizy
- **Ścieżka**: `/premium/analytics`
- **Cel**: Predykcyjne analizy i zaawansowane visualizations (tylko premium)
- **Kluczowe informacje**:
  - Prognozy wydatków
  - Heatmapy terminów
  - Timeline z wykresem Gantta
- **Komponenty**:
  - PredictiveAnalytics
  - TaskHeatmap
  - GanttTimeline
- **UX/Dostępność**: Interactive visualizations, accessibility for charts
- **Bezpieczeństwo**: Premium feature gates

### 2.5 Special Views

#### Dashboard dla Monitora
- **Ścieżka**: `/monitor` lub `/dashboard?mode=monitor`
- **Cel**: Uproszczony widok dla monitora na ścianie
- **Kluczowe informacje**:
  - Tylko 5 najbliższych terminów
  - Duża, czytelna czcionka (minimum 24px)
  - Aktualna data i godzina
- **Komponenty**:
  - MonitorDashboard
  - LargeTaskDisplay
  - AutoRefreshIndicator
- **UX/Dostępność**: High contrast, large fonts, auto-refresh
- **Bezpieczeństwo**: Read-only mode, no sensitive data

#### Onboarding
- **Ścieżka**: `/onboarding`
- **Cel**: Wprowadzenie nowych użytkowników do aplikacji
- **Kluczowe informacje**:
  - Krótkie wyjaśnienie aplikacji
  - Tworzenie pierwszego gospodarstwa
  - Dodanie pierwszego urządzenia
- **Komponenty**:
  - WelcomeStep
  - HouseholdCreationStep
  - FirstItemStep
  - SkipOption
- **UX/Dostępność**: Progressive steps, skip options, tooltips
- **Bezpieczeństwo**: Secure initial setup

#### Upgrade do Premium
- **Ścieżka**: `/upgrade`
- **Cel**: Prezentacja planów premium i proces zakupu
- **Kluczowe informacje**:
  - Porównanie planów Free vs Premium
  - Lista funkcji premium
  - Cennik i opcje płatności
- **Komponenty**:
  - PlanComparison
  - FeaturesList
  - PaymentIntegration
- **UX/Dostępność**: Clear value proposition, accessible pricing table
- **Bezpieczeństwo**: Secure payment processing

### 2.6 User Management Views

#### Ustawienia Profilu
- **Ścieżka**: `/settings/profile`
- **Cel**: Zarządzanie danymi osobowymi użytkownika
- **Kluczowe informacje**:
  - Edycja danych osobowych
  - Zmiana hasła
  - Ustawienia prywatności
- **Komponenty**:
  - ProfileForm
  - PasswordChangeForm
  - PrivacySettings
  - AccountDeletion
- **UX/Dostępność**: Clear form sections, secure password change
- **Bezpieczeństwo**: Email verification for changes, GDPR compliance

#### Ustawienia Konta
- **Ścieżka**: `/settings/account`
- **Cel**: Zarządzanie ustawieniami konta i bezpieczeństwa
- **Kluczowe informacje**:
  - Historia logowań
  - Aktywne sesje
  - Ustawienia bezpieczeństwa
- **Komponenty**:
  - LoginHistory
  - ActiveSessions
  - SecuritySettings
  - DataExport
- **UX/Dostępność**: Clear security indicators, session management
- **Bezpieczeństwo**: Session management, data export compliance

### 2.7 System Developer Views (Super Admin)

#### Dashboard Systemu
- **Ścieżka**: `/system/dashboard`
- **Cel**: Główny panel administracyjny dla zarządzania całą platformą
- **Kluczowe informacje**:
  - Kafelki nawigacyjne do głównych sekcji systemowych
  - **(Post-MVP)** Kluczowe metryki systemu (uptime, performance, errors)
  - **(Post-MVP)** Przegląd aktywności gospodarstw
  - **(Post-MVP)** Alerty systemowe i incydenty
  - **(Post-MVP)** Szybkie statystyki (nowi użytkownicy, revenue, churn)
- **Komponenty**:
  - NavigationTiles (duże przyciski z ikonami do sekcji systemowych):
    - 🏢 Gospodarstwa (`/system/households`)
    - 👤 Użytkownicy (`/system/users`)
    - 💳 Subskrypcje (`/system/subscriptions`)
    - 🔧 Administracja (`/system/administration`)
    - 🎧 Wsparcie (`/system/support`)
  - **(Post-MVP)** SystemMetricsDashboard
  - **(Post-MVP)** AlertsPanel  
  - **(Post-MVP)** QuickStats
  - **(Post-MVP)** SystemHealthIndicator
- **UX/Dostępność**: Quick navigation via tiles; **(Post-MVP)** High-level overview, drill-down capabilities
- **Bezpieczeństwo**: Super admin role verification, audit logging

#### Zarządzanie Gospodarstwami
- **Ścieżka**: `/system/households`
- **Cel**: Przegląd i zarządzanie wszystkimi gospodarstwami w systemie
- **Kluczowe informacje**:
  - Lista wszystkich gospodarstw z podstawowymi statystykami
  - Wyszukiwanie i filtry (plan, aktywność, problemy)
  - Możliwość tworzenia nowych gospodarstw
  - Zarządzanie administratorami gospodarstw
- **Komponenty**:
  - HouseholdsDataTable z advanced filtering
  - CreateHouseholdDialog
  - HouseholdStatsCards
  - AdminAssignmentForm
- **UX/Dostępność**: Advanced search, bulk operations, export funkcje
- **Bezpieczeństwo**: Audit trail dla wszystkich operacji

#### Zarządzanie Użytkownikami Globalnie
- **Ścieżka**: `/system/users`
- **Cel**: Administracja wszystkich kont użytkowników w systemie
- **Kluczowe informacje**:
  - Wyszukiwanie użytkowników w całym systemie
  - Historia aktywności i logowań
  - Zarządzanie rolami i uprawnieniami
  - Resetowanie haseł i odblokowywanie kont
- **Komponenty**:
  - GlobalUserSearch
  - UserDetailsPanel
  - RoleManagementForm
  - AccountActionsToolbar
- **UX/Dostępność**: Advanced search, user impersonation capability
- **Bezpieczeństwo**: Strong authentication for sensitive operations

#### Monitoring Subskrypcji
- **Ścieżka**: `/system/subscriptions`
- **Cel**: Przegląd wszystkich subskrypcji i metryk finansowych
- **Kluczowe informacje**:
  - Dashboard z MRR, churn rate, conversion metrics
  - Lista aktywnych subskrypcji z datami odnowienia
  - Zarządzanie promocjami i kodami rabatowymi
  - Obsługa problemów płatności i refundów
- **Komponenty**:
  - RevenueMetricsDashboard
  - SubscriptionsTable
  - PaymentIssuesPanel
  - PromoCodeManager
- **UX/Dostępność**: Financial data visualization, export capabilities
- **Bezpieczeństwo**: Financial data protection, PCI compliance

#### Administracja Systemowa
- **Ścieżka**: `/system/administration`
- **Cel**: Monitoring techniczny i zarządzanie infrastrukturą
- **Kluczowe informacje**:
  - Monitoring wydajności (CPU, memoria, database)
  - Logi aplikacji z filtrowaniem i wyszukiwaniem
  - Zarządzanie backup'ami i restore
  - Konfiguracja feature flags i deployment
- **Komponenty**:
  - SystemMonitoringDashboard
  - LogViewer z advanced search
  - BackupManager
  - FeatureFlagsPanel
- **UX/Dostępność**: Technical interface, real-time monitoring
- **Bezpieczeństwo**: Secure system access, operation logging

#### Wsparcie Techniczne
- **Ścieżka**: `/system/support`
- **Cel**: Narzędzia do udzielania wsparcia użytkownikom
- **Kluczowe informacje**:
  - System ticketów z historią konwersacji
  - Możliwość impersonacji użytkownika
  - Narzędzia diagnostyczne i troubleshooting
  - Baza wiedzy dla zespołu wsparcia
- **Komponenty**:
  - SupportTicketsPanel
  - UserImpersonationTool
  - DiagnosticTools
  - KnowledgeBaseEditor
- **UX/Dostępność**: Efficient support workflows, quick user lookup
- **Bezpieczeństwo**: Impersonation audit trail, secure data access

### 2.8 Support & Error Views

#### Pomoc/FAQ
- **Ścieżka**: `/help`
- **Cel**: Self-service support dla użytkowników
- **Kluczowe informacje**:
  - FAQ z wyszukiwaniem
  - Tutorial wideo
  - Formularz kontaktowy
- **Komponenty**:
  - FAQSearch
  - VideoTutorials
  - ContactForm
- **UX/Dostępność**: Searchable content, accessible videos
- **Bezpieczeństwo**: Rate limiting dla contact form

#### Error Views
- **Ścieżki**: `/error/404`, `/error/403`, `/error/500`
- **Cel**: Graceful error handling z helpful actions
- **Kluczowe informacje**:
  - User-friendly error messages
  - Navigation back to safety
  - Report issue option
- **Komponenty**:
  - ErrorMessage
  - NavigationSuggestions
  - ReportIssue
- **UX/Dostępność**: Clear error explanation, alternative paths
- **Bezpieczeństwo**: No sensitive information leakage

## 3. Mapa podróży użytkownika

### 3.1 Nowy Użytkownik (First-time Experience)

```
Landing Page → Register → Email Verification → 
Login → Onboarding (Create Household) → 
Add First Item → Dashboard → Explore Features
```

**Kluczowe punkty**:
- Szybka rejestracja z minimalnymi wymaganiami
- Weryfikacja email dla bezpieczeństwa
- Guided onboarding z możliwością pominięcia
- Immediate value przez dodanie pierwszego urządzenia

### 3.2 Codzienny Workflow (Daily Usage)

```
Login → Dashboard (Review Upcoming Tasks) → 
Quick Actions (Confirm/Postpone) → 
[Optional: Sidebar Navigation to Tasks/Categories/Items] → 
[Optional: Switch Tile Views (Tasks/Categories/Settings)] → 
[Optional: Open Integrated Calendar] → 
[Optional: Items Management] → Logout
```

**Kluczowe punkty**:
- Fast access do najważniejszych informacji
- One-click actions dla common tasks
- Sidebar navigation dla szybkiego dostępu do wszystkich widoków
- Tile-based navigation na dashboardzie dla alternatywnego sposobu nawigacji
- Optional deeper management functions

### 3.3 Administrator Workflow

```
Login → Dashboard → [Sidebar Navigation] → 
Household Management (via Sidebar) → 
Member Management → Items/Tasks Management → 
Subscription Management → Settings
```

**Kluczowe punkty**:
- Full control nad gospodarstwem
- Sidebar z rozszerzoną sekcją widoków gospodarstwa (w tym opcja "Gospodarstwo")
- Easy member onboarding
- Clear subscription status i upgrade paths
- Wszystkie funkcje dostępne bezpośrednio z sidebar

### 3.4 Premium User Workflow

```
Login → Dashboard → [Regular Usage] → 
Premium Features (History/Reports/Analytics) → 
Advanced Management
```

**Kluczowe punkty**:
- Seamless integration premium features
- Value-demonstrating analytics
- Advanced planning capabilities

### 3.5 System Developer Workflow

```
Login → System Dashboard → 
[Sidebar with 2 sections visible] → 
  Section 1: Household Views (if accessing specific household) →
  Section 2: System Views (primary workflow) → 
[Monitor Alerts/Issues] → 
System Administration (Users/Households/Subscriptions via Sidebar) → 
Support Tools → Technical Monitoring
```

**Kluczowe punkty**:
- System-wide oversight and control
- **Dual sidebar sections**: Sekcja gospodarstwa + Sekcja systemowa
- Możliwość dostępu zarówno do funkcji gospodarstwa jak i administracji systemu
- Proactive monitoring and issue resolution  
- Platform administration and user support
- Technical operations and maintenance
- Szybki dostęp do wszystkich narzędzi administracyjnych przez sidebar

## 4. Układ i struktura nawigacji

### 4.1 Primary Navigation (Role-Based)

#### Sidebar Navigation (Desktop/Tablet)

Aplikacja wykorzystuje **wysuwane menu z lewej strony** podzielone na sekcje kontekstowe:

**Sekcja 1: Widoki Gospodarstwa**
Pierwsza sekcja menu zawiera widoki związane z aktualnie otwartym gospodarstwem domowym. Dostępne opcje zależą od roli użytkownika i statusu subskrypcji:

```
=== GOSPODARSTWO: [Nazwa gospodarstwa] ===

📊 Dashboard (wszystkie role)
   └─ z kafelkami nawigacyjnymi i zintegrowanym kalendarzem
📋 Zadania (Admin, Domownik)
   └─ lista nadchodzących terminów (7 dni)
🏷️ Kategorie (Admin, Domownik)
   └─ widok urządzeń/wizyt pogrupowanych po kategoriach
🏠 Urządzenia/Wizyty (Admin, Domownik)
   └─ pełna lista z możliwością zarządzania
👥 Gospodarstwo (Admin only)
   └─ zarządzanie członkami i ustawieniami
📈 Historia (Premium only)
   └─ archiwum wykonanych zadań
📊 Raporty (Premium only)
   └─ zestawienia kosztów
🔬 Analizy (Premium only)
   └─ zaawansowane analizy predykcyjne
⚙️ Ustawienia (Admin, Domownik)
   └─ konfiguracja profilu i preferencji
❓ Pomoc (wszystkie role)
   └─ FAQ i wsparcie
```

**Sekcja 2: Widoki Systemowe** 
Druga sekcja menu (widoczna **tylko dla użytkowników z rolą System Developer**) zawiera widoki administracyjne całej platformy:

```
=== ADMINISTRACJA SYSTEMU ===

🖥️ System Dashboard
   └─ główny panel administracyjny platformy
🏢 Gospodarstwa
   └─ zarządzanie wszystkimi gospodarstwami w systemie
👤 Użytkownicy
   └─ administracja wszystkich kont użytkowników
💳 Subskrypcje
   └─ monitoring płatności i metryk finansowych
🔧 Administracja
   └─ zarządzanie infrastrukturą i konfiguracją
🎧 Wsparcie
   └─ narzędzia do obsługi użytkowników i troubleshooting
📈 Metryki Systemu
   └─ globalne statystyki i KPI platformy
⚙️ Konfiguracja Systemu
   └─ ustawienia globalne platformy
```

**Właściwości Sidebar:**
- **Separatory**: Wyraźne wizualne oddzielenie sekcji (linia + nagłówek sekcji)
- **Dynamic Rendering**: Pozycje menu filtrowane na podstawie roli użytkownika i subskrypcji
- **Active State**: Indicator dla aktualnie wybranego widoku
- **Collapse/Expand**: 
  - Desktop (>1024px): Persistent sidebar z możliwością zwinięcia do ikon
  - Tablet (768-1024px): Collapsible sidebar, domyślnie zwinięty
  - Mobile (<768px): Hamburger menu z pełnoekranowym overlay
- **Context Switching**: Zmiana gospodarstwa dynamicznie odświeża Sekcję 1

#### Bottom Navigation (Mobile)
```
[Dashboard] [Zadania] [Kategorie] [Menu]
```
**Uwaga**: 
- Kalendarz dostępny z poziomu Dashboard jako widget/modal
- Przycisk Menu otwiera pełny sidebar z obiema sekcjami (jeśli użytkownik ma uprawnienia)

### 4.2 Secondary Navigation

#### Header Bar
- **Left**: Logo, breadcrumb navigation
- **Center**: Household switcher (jeśli wiele gospodarstw)
- **Right**: User menu, theme toggle, notifications

#### User Dropdown Menu
```
👤 Profil
⚙️ Ustawienia konta
🏠 Zmień gospodarstwo
🌓 Tryb ciemny/jasny  
🚪 Wyloguj
```

### 4.3 Navigation Guards i Permissions

```typescript
interface NavigationRules {
  // ===== SEKCJA 1: WIDOKI GOSPODARSTWA =====
  // Dostępne w kontekście aktualnie otwartego gospodarstwa
  
  '/dashboard': ['admin', 'member', 'dashboard'], // includes integrated calendar
  '/tasks': ['admin', 'member'], // lista zadań (7 dni)
  '/categories': ['admin', 'member'], // widok pogrupowany po kategoriach
  '/items': ['admin', 'member'], // pełna lista urządzeń/wizyt
  '/households/:id/manage': ['admin'], // zarządzanie gospodarstwem
  
  // Premium features (wymagają subskrypcji)
  '/premium/history': ['admin', 'member', 'premium_subscription'],
  '/premium/reports': ['admin', 'member', 'premium_subscription'],
  '/premium/analytics': ['admin', 'member', 'premium_subscription'],
  
  '/settings/profile': ['admin', 'member'], // ustawienia profilu
  '/settings/account': ['admin', 'member'], // ustawienia konta
  '/help': ['admin', 'member', 'dashboard'], // pomoc i FAQ
  
  '/monitor': ['dashboard'], // special monitor mode (read-only)
  
  // ===== SEKCJA 2: WIDOKI SYSTEMOWE =====
  // Dostępne tylko dla System Developer (Super Admin)
  
  '/system/*': ['system_developer'], // guard dla całej sekcji systemowej
  '/system/dashboard': ['system_developer'], // główny panel systemu
  '/system/households': ['system_developer'], // wszystkie gospodarstwa
  '/system/users': ['system_developer'], // globalna administracja użytkowników
  '/system/subscriptions': ['system_developer'], // monitoring subskrypcji
  '/system/administration': ['system_developer'], // administracja techniczna
  '/system/support': ['system_developer'], // wsparcie techniczne
  '/system/metrics': ['system_developer'], // metryki systemu
  '/system/configuration': ['system_developer'], // konfiguracja globalna
  
  // ===== SHARED ROUTES =====
  // Dostępne dla wielu typów użytkowników
  
  '/upgrade': ['admin', 'member'], // upgrade do premium
  '/onboarding': ['admin', 'member'], // first-time setup
}

// Sidebar menu items visibility rules
interface SidebarSectionRules {
  section1_household: {
    visible_for: ['admin', 'member', 'dashboard'],
    context: 'current_household_id_required',
    items: {
      dashboard: ['admin', 'member', 'dashboard'],
      tasks: ['admin', 'member'],
      categories: ['admin', 'member'],
      items: ['admin', 'member'],
      household_manage: ['admin'],
      history: ['admin', 'member', 'premium_subscription'],
      reports: ['admin', 'member', 'premium_subscription'],
      analytics: ['admin', 'member', 'premium_subscription'],
      settings: ['admin', 'member'],
      help: ['admin', 'member', 'dashboard']
    }
  },
  
  section2_system: {
    visible_for: ['system_developer'],
    context: 'global_system_access',
    items: {
      system_dashboard: ['system_developer'],
      households: ['system_developer'],
      users: ['system_developer'],
      subscriptions: ['system_developer'],
      administration: ['system_developer'],
      support: ['system_developer'],
      metrics: ['system_developer'],
      configuration: ['system_developer']
    }
  }
}
```

### 4.4 Responsive Navigation Strategy

- **Desktop (>1024px)**: Persistent sidebar z full menu
- **Tablet (768-1024px)**: Collapsible sidebar
- **Mobile (<768px)**: Bottom navigation + hamburger menu
- **Monitor Mode**: Minimal navigation, focus na content

## 5. Kluczowe komponenty

### 5.1 Layout Components

#### AppLayout
- **Cel**: Main layout wrapper z responsive navigation
- **Features**: Sidebar integration, theme switching, breadcrumbs, header bar
- **Reusability**: Base layout dla wszystkich authenticated views

#### Sidebar
- **Cel**: Główne wysuwane menu nawigacyjne z lewej strony
- **Features**:
  - **Sekcja 1: Widoki Gospodarstwa** - dynamicznie renderowana na podstawie aktualnie wybranego gospodarstwa
    - Lista widoków związanych z gospodarstwem (Dashboard, Zadania, Kategorie, Urządzenia, etc.)
    - Nagłówek sekcji z nazwą gospodarstwa
    - Filtrowanie pozycji na podstawie roli użytkownika (Admin/Domownik/Dashboard)
    - Oznaczenie funkcji premium (badge/icon)
  - **Sekcja 2: Widoki Systemowe** - widoczna tylko dla System Developer
    - Lista widoków administracyjnych platformy
    - Nagłówek sekcji "Administracja Systemu"
    - Separator wizualny między sekcjami
  - **Responsive behavior**:
    - Desktop (>1024px): Persistent sidebar, możliwość zwinięcia do ikon
    - Tablet (768-1024px): Collapsible sidebar, domyślnie zwinięty
    - Mobile (<768px): Hamburger menu z pełnoekranowym overlay
  - **Active state indicator** dla aktualnie wybranego widoku
  - **Household switcher** w headerze sekcji 1 (jeśli użytkownik ma dostęp do wielu gospodarstw)
  - **Tooltips** dla ikon w trybie zwiniętym
  - **Badge indicators** dla powiadomień/alertów
  - **Smooth transitions** przy collapse/expand
  - **Keyboard navigation** (Tab, Enter, Arrow keys)
- **Reusability**: Wszystkie authenticated views (oprócz monitor mode)
- **Security**: Role-based rendering, permission checks per menu item

#### MonitorLayout  
- **Cel**: Uproszczony layout dla monitor dashboard
- **Features**: Full-screen mode, auto-refresh, minimal UI, no sidebar
- **Reusability**: Specjalistyczny layout dla display monitors

### 5.2 Data Display Components

#### NavigationTiles (Dashboard)
- **Cel**: Kafelki nawigacyjne do przełączania widoków na dashboardzie
- **Features**:
  - Duże, klikalne kafelki z ikonami (Zadania/Kategorie/Ustawienia)
  - Responsive grid layout (2-3 kolumny w zależności od rozmiaru ekranu)
  - Active state indicator dla wybranego widoku
  - Hover effects z subtle animations
  - Keyboard navigation support (tab, enter)
- **Reusability**: Dashboard główny
- **Security**: Dynamic rendering based on user permissions

#### CalendarWidget
- **Cel**: Zintegrowany kalendarz w dashboardzie
- **Features**:
  - Kompaktowy widok miesięczny lub modal z pełnym widokiem
  - Kolorowe oznaczenia kategorii
  - Click na dzień pokazuje szczegóły terminów
  - Nawigacja między miesiącami
  - Responsive design (collapse na mobile)
- **Reusability**: Dashboard główny, modals
- **Security**: Filtrowanie wydarzeń based on user permissions

#### CategoryGroupedView
- **Cel**: Widok urządzeń/wizyt pogrupowanych po kategoriach
- **Features**:
  - Grupowanie itemów po kategoriach z możliwością collapse/expand
  - Licznik itemów w każdej kategorii
  - Szybki dostęp do akcji na itemach
  - Sortowanie wewnątrz grup
- **Reusability**: Dashboard (widok Kategorie)
- **Security**: Role-based item visibility

#### EditableDataTable
- **Cel**: Tabela z inline editing capabilities
- **Features**: 
  - Role-based edit permissions
  - Sortowanie i filtrowanie
  - Bulk actions dla administratorów
  - Responsive stacked mode
- **Reusability**: Items list, members list, tasks history

#### TaskCard
- **Cel**: Wyświetlanie pojedynczego zadania z quick actions
- **Features**:
  - Color-coded urgency (red/orange/green)
  - Quick action buttons
  - Responsive design
- **Reusability**: Dashboard, calendar popups

#### CalendarEventRenderer
- **Cel**: Custom rendering wydarzeń w zintegrowanym kalendarzu
- **Features**:
  - Category color coding
  - Priority indicators
  - Hover details
- **Reusability**: CalendarWidget, mini calendars w dashboardzie

### 5.3 Form Components

#### ItemForm
- **Cel**: Dodawanie/edycja urządzeń i wizyt
- **Features**:
  - Progressive disclosure
  - Interval calculator
  - Real-time validation
  - Auto-save drafts
- **Reusability**: Add item, edit item, bulk edit

#### TaskActionForm
- **Cel**: Formularz dla akcji na zadaniach (postpone, complete)
- **Features**:
  - Conditional fields based on action
  - Date/time pickers
  - Notes i attachments
- **Reusability**: Task completion, postponement, editing

#### CreateCategoryDialogComponent
- **Cel**: Formularz dodawania nowych kategorii
- **Features**:
  - Wybór typu kategorii z dropdown
  - Walidacja wymaganych pól (nazwa, typ)
  - Opcjonalne pola (opis, sortOrder)
  - Real-time validation feedback
  - Loading states podczas zapisywania
  - Toast notifications dla sukcesu/błędu
- **Reusability**: Categories list view, admin panels
- **Security**: Input sanitization, validation on frontend and backend

#### EditCategoryDialogComponent
- **Cel**: Formularz edycji istniejących kategorii
- **Features**:
  - Pre-populated form z danymi kategorii
  - Możliwość zmiany typu kategorii
  - Walidacja zmian
  - Change detection dla dirty forms
  - Loading states i error handling
  - Toast notifications
- **Reusability**: Categories list view, inline editing
- **Security**: Permission checks, input validation

### 5.4 Interactive Components

#### ActionButton
- **Cel**: Smart button z role-based permissions
- **Features**:
  - Permission checking
  - Loading states
  - Confirmation dialogs
  - Keyboard accessibility
- **Reusability**: Wszystkie CRUD operations

#### QuickActionToolbar
- **Cel**: Grupa quick actions dla zadań
- **Features**:
  - Context-aware actions
  - Bulk operations support
  - Responsive button grouping
- **Reusability**: Dashboard (wszystkie widoki kafelkowe), task lists, calendar widget

#### ConfirmDialog
- **Cel**: Reusable confirmation dialogs
- **Features**:
  - Customizable content
  - Action severity indicators
  - Keyboard navigation
- **Reusability**: Delete operations, destructive actions

### 5.5 Navigation Components

#### HouseholdSwitcher
- **Cel**: Przełączanie między gospodarstwami
- **Features**:
  - Dropdown z list of accessible households
  - Role indicators
  - Quick stats per household
- **Reusability**: Header, dashboard

#### Breadcrumb
- **Cel**: Hierarchical navigation indicator
- **Features**:
  - Auto-generated based on route
  - Clickable segments
  - Responsive collapsing
- **Reusability**: All internal pages

### 5.6 Utility Components

#### LoadingSpinner
- **Cel**: Consistent loading states
- **Features**:
  - Multiple sizes
  - Overlay modes
  - Accessibility announcements
- **Reusability**: Wszystkie loading states

#### Toast
- **Cel**: User notifications i feedback
- **Features**:
  - Multiple types (success, error, warning, info)
  - Auto-dismiss
  - Action buttons
- **Reusability**: API responses, form submissions

#### UpgradePrompt
- **Cel**: Freemium upgrade notifications
- **Features**:
  - Context-aware messaging
  - Feature comparisons
  - Dismissible
- **Reusability**: Limit warnings, premium feature gates

### 5.7 Premium Components

#### ReportChart
- **Cel**: Wykresy dla premium analytics
- **Features**:
  - Multiple chart types
  - Interactive tooltips
  - Export functionality
  - Accessibility dla screen readers
- **Reusability**: Cost reports, analytics dashboard

#### AnalyticsDashboard
- **Cel**: Premium analytics overview
- **Features**:
  - Predictive insights
  - Trend analysis
  - Customizable widgets
- **Reusability**: Analytics page, premium dashboard

### 5.8 System Developer Components (Super Admin)

#### NavigationTiles
- **Cel**: Szybka nawigacja do głównych sekcji systemu z dashboardu
- **Features**:
  - Duże, klikalne kafelki z ikonami i opisami
  - Responsive grid layout (2-3 kolumny w zależności od rozmiaru ekranu)
  - Hover effects z subtle animations
  - Badge indicators dla alertów/powiadomień na kafelkach
  - Liczniki pokazujące aktualne statystyki każdej sekcji
  - Keyboard navigation support (tab, enter)
- **Reusability**: System dashboard, admin quick access panels
- **Security**: Dynamic rendering based on super admin permissions

#### SystemMetricsDashboard
- **Cel**: Monitoring kluczowych metryk systemowych
- **Features**:
  - Real-time system health indicators
  - Performance metrics visualization
  - Alert panels z system notifications
  - Drill-down capabilities do szczegółów
- **Reusability**: System dashboard, monitoring views
- **Security**: Super admin access validation, audit logging

#### HouseholdsDataTable
- **Cel**: Zarządzanie wszystkimi gospodarstwami w systemie
- **Features**:
  - Advanced filtering i search functionality
  - Bulk operations na wielu gospodarstwach
  - Inline editing podstawowych informacji
  - Export capabilities (CSV, Excel)
  - Pagination dla dużych zbiorów danych
- **Reusability**: System households view
- **Security**: Permission-based actions, operation audit trail

#### GlobalUserSearch
- **Cel**: Wyszukiwanie i zarządzanie użytkownikami globalnie
- **Features**:
  - Cross-household user search
  - Advanced filters (role, activity, subscription)
  - User impersonation capabilities
  - Account management actions
- **Reusability**: System users view, support tools
- **Security**: Strong authentication for sensitive operations

#### RevenueMetricsDashboard
- **Cel**: Monitoring finansowy i subskrypcje
- **Features**:
  - MRR/ARR tracking with trend analysis
  - Churn rate i conversion metrics
  - Payment issues dashboard
  - Subscription lifecycle management
- **Reusability**: System subscriptions view, financial reports
- **Security**: PCI compliance, financial data protection

#### UserImpersonationTool
- **Cel**: Bezpieczna impersonacja użytkowników dla wsparcia
- **Features**:
  - Secure user session switching
  - Complete audit trail logging
  - Time-limited impersonation sessions
  - Clear indicators podczas impersonacji
- **Reusability**: Support tools, troubleshooting
- **Security**: Multi-factor authentication required, comprehensive logging

#### SystemMonitoringDashboard
- **Cel**: Monitoring techniczny infrastruktury
- **Features**:
  - Real-time performance monitoring
  - Resource utilization tracking
  - Alert management system
  - Historical trend analysis
- **Reusability**: System administration view
- **Security**: Technical access control, operational security

#### SupportTicketsPanel
- **Cel**: Zarządzanie ticketami wsparcia technicznego
- **Features**:
  - Ticket lifecycle management
  - Priority i category filtering
  - Response templates i knowledge base integration
  - SLA tracking i escalation rules
- **Reusability**: Support dashboard, customer service tools
- **Security**: Customer data protection, access controls

Każdy komponent uwzględnia:
- **Accessibility**: ARIA labels, keyboard navigation, screen reader support
- **Responsiveness**: Mobile-first design z progressive enhancement
- **Theming**: Support dla light/dark modes
- **Internationalization**: Prepared dla przyszłych tłumaczeń
- **Performance**: Lazy loading, virtual scrolling gdzie needed
- **Security**: Input sanitization, XSS protection, role-based rendering