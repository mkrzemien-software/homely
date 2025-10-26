# Architektura UI dla Homely - Aplikacja do Zarządzania Terminami Domowymi

## 1. Przegląd struktury UI

Aplikacja Homely to responsywna aplikacja webowa zbudowana w Angular 20 z PrimeNG, przeznaczona do zarządzania terminami serwisów urządzeń domowych i wizyt. Architektura UI opiera się na modelu freemium z role-based access control, obsługując trzy typy użytkowników: Administrator, Domownik i Dashboard (tylko odczyt).

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
- **Cel**: Centralny przegląd nadchodzących terminów i kluczowych statystyk gospodarstwa
- **Kluczowe informacje**: 
  - Terminy na najbliższe 7 dni z oznaczeniem pilności
  - Statystyki wykorzystania limitu (wersja darmowa)
  - Szybkie akcje (potwierdź, przełóż, edytuj)
  - Przełącznik gospodarstw (jeśli dostęp do wielu)
- **Komponenty**:
  - UpcomingTasksList z color-coded urgency
  - QuickActionButtons (potwierdź, przełóż, szczegóły)
  - UsageStatistics (progress bars dla limitów freemium)
  - HouseholdSwitcher
- **UX/Dostępność**: Auto-refresh co 5 minut, keyboard shortcuts, ARIA live regions
- **Bezpieczeństwo**: Role-based task visibility, permission checks na akcjach

#### Widok Kalendarza
- **Ścieżka**: `/calendar`
- **Cel**: Wizualizacja terminów w układzie miesięcznym z możliwością szybkich akcji
- **Kluczowe informacje**:
  - Kalendarz miesięczny z kolorowymi oznaczeniami kategorii
  - Legenda kategorii i priorytetów
  - Szczegóły terminów po kliknięciu
  - Nawigacja między miesiącami
- **Komponenty**:
  - PrimeNG FullCalendar z custom event rendering
  - CalendarLegend
  - TaskDetailsModal
  - MonthNavigation
- **UX/Dostępność**: Keyboard navigation przez daty, screen reader announcements
- **Bezpieczeństwo**: Filtrowanie wydarzeń based on user permissions

#### Lista Urządzeń/Wizyt
- **Ścieżka**: `/items`
- **Cel**: Kompleksowe zarządzanie wszystkimi urządzeniami i wizytami w gospodarstwie
- **Kluczowe informacje**:
  - Lista wszystkich urządzeń/wizyt z możliwością inline editing
  - Filtry (kategoria, osoba odpowiedzialna, priorytet)
  - Sortowanie (nazwa, data następnego terminu, priorytet)
  - Informacje o następnym terminie i statusie
- **Komponenty**:
  - EditableDataTable z role-based edit permissions
  - ItemFilters (kategoria, osoba, priorytet, status)
  - AddItemButton
  - BulkActions (dla administratorów)
- **UX/Dostępność**: Tabela z proper headers, sortable columns, paginacja
- **Bezpieczeństwo**: Edit permissions per item, admin-only bulk operations

#### Formularz Urządzenia/Wizyty
- **Ścieżki**: `/items/add`, `/items/:id/edit`
- **Cel**: Dodawanie nowych i edycja istniejących urządzeń/wizyt
- **Kluczowe informacje**:
  - Formularz z wszystkimi wymaganymi polami
  - Kalkulator następnego terminu
  - Walidacja interwałów czasowych
- **Komponenty**:
  - ItemForm z progressive disclosure
  - IntervalCalculator
  - CategorySelector
  - AssignmentSelector
- **UX/Dostępność**: Form validation feedback, save/cancel actions
- **Bezpieczeństwo**: Input validation, permission checks, freemium limits

#### Szczegóły Zadania
- **Ścieżka**: `/tasks/:id`
- **Cel**: Wyświetlenie szczegółów zadania z możliwością wykonania akcji
- **Kluczowe informacje**:
  - Pełne informacje o zadaniu i związanym urządzeniu
  - Historia poprzednich wykonań (premium)
  - Akcje (potwierdź, przełóż, edytuj, usuń)
- **Komponenty**:
  - TaskDetails
  - TaskHistory (premium)
  - TaskActionButtons
  - RelatedItemInfo
- **UX/Dostępność**: Clear action hierarchy, confirmation dialogs
- **Bezpieczeństwo**: Task ownership validation, role-based actions

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

### 2.7 Support & Error Views

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
[Optional: Calendar View for Planning] → 
[Optional: Items Management] → Logout
```

**Kluczowe punkty**:
- Fast access do najważniejszych informacji
- One-click actions dla common tasks
- Optional deeper management functions

### 3.3 Administrator Workflow

```
Login → Dashboard → Household Management → 
Member Management → Items/Tasks Management → 
Subscription Management → Settings
```

**Kluczowe punkty**:
- Full control nad gospodarstwem
- Easy member onboarding
- Clear subscription status i upgrade paths

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

## 4. Układ i struktura nawigacji

### 4.1 Primary Navigation (Role-Based)

#### Sidebar Navigation (Desktop/Tablet)
```
📊 Dashboard (wszystkie role)
📅 Kalendarz (wszystkie role)  
🏠 Urządzenia/Wizyty (Admin, Domownik)
👥 Gospodarstwo (Admin only)
📈 Historia (Premium only)
📊 Raporty (Premium only) 
🔬 Analizy (Premium only)
⚙️ Ustawienia (Admin, Domownik)
❓ Pomoc (wszystkie role)
```

#### Bottom Navigation (Mobile)
```
[Dashboard] [Kalendarz] [Urządzenia] [Menu]
```

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
  '/dashboard': ['admin', 'member', 'dashboard'],
  '/calendar': ['admin', 'member', 'dashboard'],
  '/items': ['admin', 'member'],
  '/households/*/manage': ['admin'],
  '/premium/*': ['premium_subscription'],
  '/monitor': ['dashboard'] // special monitor mode
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
- **Features**: Sidebar toggle, theme switching, breadcrumbs
- **Reusability**: Base layout dla wszystkich authenticated views

#### MonitorLayout  
- **Cel**: Uproszczony layout dla monitor dashboard
- **Features**: Full-screen mode, auto-refresh, minimal UI
- **Reusability**: Specjalistyczny layout dla display monitors

### 5.2 Data Display Components

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
- **Cel**: Custom rendering wydarzeń w kalendarzu
- **Features**:
  - Category color coding
  - Priority indicators
  - Hover details
- **Reusability**: Main calendar, mini calendars

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
- **Reusability**: Dashboard, task lists, calendar

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

Każdy komponent uwzględnia:
- **Accessibility**: ARIA labels, keyboard navigation, screen reader support
- **Responsiveness**: Mobile-first design z progressive enhancement
- **Theming**: Support dla light/dark modes
- **Internationalization**: Prepared dla przyszłych tłumaczeń
- **Performance**: Lazy loading, virtual scrolling gdzie needed
- **Security**: Input sanitization, XSS protection, role-based rendering