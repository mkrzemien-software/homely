# Plan Testów - Homely

## 1. Wprowadzenie i Cel Testów

### 1.1 Opis Projektu
Homely to aplikacja webowa do zarządzania terminami serwisów urządzeń domowych i wizyt medycznych dla całej rodziny. Aplikacja implementuje model biznesowy freemium z trzema planami subskrypcyjnymi (Darmowy, Premium, Rodzinny) oraz oferuje zaawansowane funkcje zarządzania gospodarstwem domowym.

### 1.2 Stack Technologiczny
- **Frontend**: Angular 20 z PrimeNG, standalone components, Signals
- **Backend**: ASP.NET Core z .NET 8, Entity Framework Core
- **Baza danych**: Supabase (PostgreSQL) z Row Level Security
- **Autentykacja**: Supabase Auth z JWT tokens
- **CI/CD**: GitHub Actions
- **Hosting**: AWS z Docker

### 1.3 Cele Testowania
1. Zapewnienie poprawności kluczowych funkcjonalności biznesowych (zarządzanie zadaniami, wydarzeniami, gospodarstwami)
2. Weryfikacja bezpieczeństwa i izolacji danych między gospodarstwami (RLS policies)
3. Walidacja limitów planu freemium i funkcji premium
4. Potwierdzenie zgodności z wymogami RODO
5. Weryfikacja wydajności i skalowalności systemu
6. Zapewnienie wysokiej jakości kodu i pokrycia testami (minimum 80% dla kodu krytycznego)

## 2. Zakres Testów

### 2.1 W Zakresie Testów

**Funkcjonalności MVP**:
- ✅ Rejestracja i autentykacja użytkowników (Supabase Auth)
- ✅ Tworzenie i zarządzanie gospodarstwami domowymi
- ✅ Zarządzanie członkami gospodarstwa (role: Administrator, Domownik, Dashboard)
- ✅ CRUD dla zadań (Tasks) - szablony z interwałami
- ✅ CRUD dla wydarzeń (Events) - konkretne wystąpienia z datami
- ✅ Automatyczne generowanie kolejnych wydarzeń po completion
- ✅ System ról i uprawnień (RBAC)
- ✅ Plan usage tracking (PlanUsageService) i limity freemium
- ✅ Soft delete pattern dla wszystkich głównych encji
- ✅ Row Level Security (RLS) - izolacja danych między gospodarstwami
- ✅ Dashboard z kalendarzem tygodniowym i miesięcznym
- ✅ Widoki: Wydarzenia, Zadania, Kategorie
- ✅ Filtrowanie i sortowanie

**Funkcjonalności Premium (MVP)**:
- ✅ Historia wydarzeń (events_history)
- ✅ Raporty kosztów
- ✅ Zaawansowane analizy

**Bezpieczeństwo**:
- ✅ Szyfrowanie danych (AES-256, TLS 1.3)
- ✅ JWT token validation i expiration (30 dni)
- ✅ RLS policies dla wszystkich tabel
- ✅ RODO compliance (eksport danych, usuwanie konta)
- ✅ Input validation i sanitization (XSS, SQL injection prevention)

**Wydajność**:
- ✅ Query performance (eager loading, AsNoTracking)
- ✅ N+1 query detection
- ✅ API response times
- ✅ Database indexing

**Integracje**:
- ✅ Supabase Auth integration
- ✅ Email notifications (post-MVP, ale testy kontraktowe)
- ✅ Payment gateway integration (Stripe/PayPal - post-MVP)

### 2.2 Poza Zakresem Testów (Post-MVP)

- ❌ Aplikacje mobilne native (iOS/Android)
- ❌ Powiadomienia push
- ❌ Integracje z zewnętrznymi kalendarzami (Google Calendar, Outlook)
- ❌ OCR dokumentów
- ❌ Funkcje społecznościowe
- ❌ Machine Learning / AI features
- ❌ Aplikacje trzecich stron przez API publiczne

### 2.3 Założenia i Ograniczenia

**Założenia**:
- Testy będą uruchamiane w środowiskach: local, CI/CD, staging
- Dostęp do instancji Supabase testowej
- Konfiguracja GitHub Actions dla automatyzacji
- Testcontainers dla izolowanych testów bazodanowych

**Ograniczenia**:
- Ograniczony budżet czasu w fazie MVP - priorytetyzacja testów krytycznych
- Brak dedykowanego zespołu QA - deweloperzy piszą testy
- Brak środowiska pre-production w fazie MVP

## 3. Strategia Testowania

### 3.1 Podejście Ogólne

Projekt wykorzystuje **podejście Test-Driven Development (TDD) dla komponentów krytycznych** oraz **Risk-Based Testing** do priorytetyzacji obszarów testowych.

**Piramida Testów**:
```
           /\
          /E2E\         (10% - Playwright)
         /------\
        /  API  \       (20% - Integration Tests)
       /----------\
      /   Unit     \    (70% - xUnit, Jasmine)
     /--------------\
```

**Strategia**:
- **70% Unit Tests** - szybkie, izolowane, wysokie pokrycie logiki biznesowej
- **20% Integration/API Tests** - weryfikacja współpracy komponentów
- **10% E2E Tests** - krytyczne user journeys (happy paths + error scenarios)

### 3.2 Shift-Left Testing

- Testy pisane równocześnie z kodem (TDD dla services)
- Static code analysis w CI/CD (SonarQube, Snyk)
- Code review z wymogiem pokrycia testowego
- Pre-commit hooks dla lint i formatowania

### 3.3 Continuous Testing

- Wszystkie testy unit i integration uruchamiane w GitHub Actions przy każdym PR
- E2E tests uruchamiane nocne (daily) i przed release
- Performance tests tygodniowo
- Security scans w CI/CD pipeline

### 3.4 Test Data Management

- **Fixtures** - przygotowane zestawy danych testowych (households, users, tasks)
- **Bogus** - generowanie losowych danych dla testów parametryzowanych
- **Respawn** - czyszczenie bazy danych między testami
- **Testcontainers** - izolowane kontenery PostgreSQL dla testów integracyjnych
- **Seedy** - dane inicjalne dla środowisk testowych (staging)

## 4. Typy Testów

### 4.1 Testy Jednostkowe (Unit Tests)

**Backend (.NET 8)**:
- **Framework**: xUnit
- **Biblioteki pomocnicze**: FluentAssertions, Moq, Bogus
- **Zakres**:
  - Services: `EventService`, `TaskService`, `PlanUsageService`, `HouseholdMemberService`
  - Domain models: business logic w encjach
  - Validators: input validation logic
  - Extensions i utilities
- **Pokrycie**: minimum 80% dla services i domain logic
- **Przykładowe przypadki**:
  - `EventService.CompleteEventAsync()` - weryfikacja automatycznego generowania następnego wydarzenia
  - `PlanUsageService.WouldExceedLimitAsync()` - walidacja limitów planu
  - `TaskService.CreateTaskAsync()` - sprawdzenie soft delete filter
  - Domain model validation (required fields, format email, etc.)

**Frontend (Angular 20)**:
- **Framework**: Jasmine + Karma (lub migracja do Jest)
- **Biblioteki pomocnicze**: Angular Testing Library, ng-mocks, MSW
- **Zakres**:
  - Components: logika komponentów (bez testowania HTML)
  - Services: API communication, state management (Signals)
  - Pipes: transformacje danych
  - Guards: autoryzacja i nawigacja
  - Validators: custom form validators
- **Pokrycie**: minimum 70%
- **Przykładowe przypadki**:
  - `EventListComponent` - sortowanie i filtrowanie wydarzeń
  - `DashboardComponent` - agregacja danych z kalendarza
  - `AuthGuard` - sprawdzenie przekierowania dla niezalogowanych
  - `EventService` (frontend) - mockowanie HTTP calls

### 4.2 Testy Integracyjne (Integration Tests)

**Backend API Tests**:
- **Framework**: xUnit + WebApplicationFactory
- **Biblioteki pomocnicze**: Testcontainers.PostgreSQL, Respawn, FluentAssertions
- **Zakres**:
  - API endpoints (`/tasks`, `/events`, `/households`, `/auth`)
  - Repository + Database interactions
  - EF Core migrations compatibility z Supabase schema
  - Middleware (authentication, error handling, validation)
- **Pokrycie**: wszystkie kontrolery API
- **Przykładowe przypadki**:
  - `POST /tasks` - utworzenie zadania i weryfikacja w bazie
  - `POST /events/{id}/complete` - completion flow z automatycznym utworzeniem następnego wydarzenia
  - `POST /households/{id}/members` - dodanie członka z walidacją limitu planu
  - `GET /dashboard/upcoming-events?days=7` - agregacja danych z wieloma tabelami
  - Error handling: 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found

**Database Integration Tests**:
- **Framework**: xUnit + Testcontainers.PostgreSQL
- **Zakres**:
  - RLS policies - **manualne testy podstawowe** + integration tests przez API
  - Constraints i foreign keys
  - Indexes performance
  - **Uwaga**: Logika biznesowa w .NET services (nie w database triggers)
- **Przykładowe przypadki**:
  - Integration test: User A próbuje GET /tasks dla household B → 403 Forbidden (RLS)
  - Soft delete: zapytania filtrują rekordy z `deleted_at IS NOT NULL`
  - Constraint test: nie można dodać wydarzenia bez zadania (foreign key)

**❌ Pominięte w MVP**: pgTAP (dedykowane testy PostgreSQL)

**Frontend Integration Tests**:
- **Framework**: Angular Testing Library
- **Zakres**:
  - Component + Service + HTTP interaction
  - Routing i navigation flows
  - Forms z walidacją
- **Przykładowe przypadki**:
  - Formularz tworzenia zadania - walidacja, submit, error handling
  - Lista wydarzeń - fetch data, display, pagination

### 4.3 Testy End-to-End (E2E)

**Framework**: Playwright (nowoczesna alternatywa dla Protractor)
**Zakres**: Krytyczne user journeys (happy paths + główne error scenarios)
**Pokrycie**: 5-10 kluczowych scenariuszy

**Przykładowe scenariusze**:

**P0 (Must Have)**:
1. **Rejestracja i onboarding nowego użytkownika**:
   - Rejestracja → weryfikacja email → onboarding → utworzenie gospodarstwa → dodanie pierwszego zadania → utworzenie wydarzenia
2. **Happy path - zarządzanie wydarzeniem**:
   - Logowanie → dashboard → utworzenie zadania z interwałem → utworzenie wydarzenia → potwierdzenie completion → weryfikacja automatycznego wygenerowania następnego wydarzenia
3. **Multi-user collaboration**:
   - Admin dodaje członka → członek loguje się → widzi przypisane wydarzenie → potwierdza wykonanie
4. **Freemium limit enforcement**:
   - Użytkownik darmowy tworzy 5 zadań → próba utworzenia 6. zadania → komunikat o limicie → upgrade do premium → ponowna próba → sukces

**P1 (Should Have)**:
5. **Dashboard navigation flow**:
   - Nawigacja przez kafelki → kalendarz miesięczny → lista wydarzeń → widok zadań → widok kategorii
6. **Role-based access control**:
   - Domownik próbuje edytować gospodarstwo → brak dostępu → Administrator edytuje → sukces
7. **Error handling**:
   - Brak połączenia z internetem → komunikat → retry → sukces

**Konfiguracja Playwright**:
- Testy na Chrome, Firefox, Safari (WebKit)
- Responsive testing (desktop, tablet, mobile viewports)
- Screenshots i video recording dla failed tests
- Parallel execution w CI/CD

### 4.4 Testy API/Contract Tests

**❌ Pominięte w MVP**: Contract Testing (Pact)

**Uzasadnienie**:
- MVP z jednym zespołem (frontend + backend) - integration tests API wystarczą
- Pact ma sens gdy API się ustabilizuje i jest więcej konsumentów
- Rozważyć post-MVP gdy API będzie używane przez aplikacje mobilne

**Alternatywa w MVP**:
- Integration tests API z WebApplicationFactory
- Swagger/OpenAPI schema validation
- TypeScript types generowane z backendu (shared types)

### 4.5 Testy Bezpieczeństwa (Security Tests)

**Narzędzia MVP**: Snyk (dependency scanning w CI/CD)

**❌ Pominięte w MVP**: OWASP ZAP, SonarQube (post-MVP lub przed production)

**Zakres**:

**Authentication & Authorization**:
- JWT token validation (expired, invalid, missing)
- RLS policies - data isolation tests
- Role-based access control (Admin vs Domownik vs Dashboard)
- Session management (30 dni expiration)

**Input Validation**:
- XSS prevention (sanitization testów)
- SQL Injection prevention (parametrized queries)
- CSRF protection
- File upload validation (gdy funkcja będzie dostępna)

**Data Protection**:
- Szyfrowanie w spoczynku (AES-256)
- TLS 1.3 enforcement
- Password hashing (bcrypt/Argon2)
- RODO compliance (export danych, right to be forgotten)

**Penetration Testing** (przed production):
- OWASP Top 10 vulnerability scanning
- Dependency scanning (Snyk)
- Static Application Security Testing (SAST) - SonarQube

**Przykładowe przypadki testowe**:
- Test RLS: User z household A próbuje GET /tasks?householdId=B → 403 Forbidden
- Test JWT: Request bez tokenu → 401 Unauthorized
- Test JWT expired: Request z wygasłym tokenem → 401 + refresh flow
- Test XSS: Wpisanie `<script>alert('XSS')</script>` w nazwę zadania → sanitization
- Test RODO: Użytkownik żąda eksportu wszystkich danych → JSON z wszystkimi danymi osobowymi

### 4.6 Testy Wydajnościowe (Performance Tests)

**Narzędzia**: k6 (load testing), Lighthouse (frontend performance)

**Zakres**:

**Backend Performance**:
- Load testing API endpoints
- Database query performance (N+1 detection)
- Response times pod obciążeniem
- Scalability testing (wzrost liczby gospodarstw i użytkowników)

**Frontend Performance**:
- Time to Interactive (TTI) < 2s dla dashboard
- Lighthouse score > 90
- Bundle size optimization
- Lazy loading verification

**Metryki docelowe**:
- API response time: p95 < 200ms, p99 < 500ms
- Database queries: < 100ms dla 90% queries
- Concurrent users: 100 users bez degradacji
- Throughput: 1000 requests/min

**Przykładowe scenariusze k6**:
```javascript
// Load test dla GET /dashboard/upcoming-events
- Ramp up: 0 → 100 users w 2 min
- Plateau: 100 users przez 5 min
- Ramp down: 100 → 0 users w 2 min
- Oczekiwania: p95 < 200ms, error rate < 1%
```

**Profiling**:
- EF Core query logging (detect N+1)
- Database slow query log analysis (PostgreSQL `pg_stat_statements`)

**❌ Pominięte w MVP**: Application Insights, New Relic (monitoring post-MVP)

### 4.7 Testy Dostępności (Accessibility Tests)

**❌ Pominięte w MVP**: Accessibility Testing (post-MVP)

**Uzasadnienie**:
- Skupienie na funkcjonalności w MVP
- PrimeNG ma wbudowane podstawowe wsparcie a11y
- Planowane post-MVP: axe-core, Lighthouse, WCAG 2.1 AA compliance

**Alternatywa w MVP**:
- Manualna weryfikacja keyboard navigation (podstawowa)
- Code review: semantic HTML, ARIA gdzie potrzebne

### 4.8 Testy Regresji (Regression Tests)

**Strategia**:
- Automated regression suite uruchamiana przed każdym release
- Składa się z podzbioru testów unit, integration, E2E (krytyczne scenariusze)
- Uruchamiana w GitHub Actions przy merge do `main`/`master`

**Zakres**:
- Wszystkie testy P0 z sekcji E2E
- Kluczowe API integration tests
- Critical path unit tests (EventService completion logic, PlanUsageService limits)

### 4.9 Testy Smoke (Smoke Tests)

**Cel**: Szybka weryfikacja, czy aplikacja jest "żywa" po deployment

**Zakres**:
- Health check endpoint: `GET /health` → 200 OK
- Database connectivity check
- Supabase Auth availability
- Frontend build loads successfully
- Critical API endpoints (login, dashboard)

**Czas wykonania**: < 2 minuty

**Uruchamianie**: Po każdym deployment do staging/production

## 5. Narzędzia Testowe

### 5.1 Backend (.NET 9)

| Narzędzie | Przeznaczenie | Wersja |
|-----------|---------------|--------|
| **xUnit** | Framework testowy | Latest |
| **FluentAssertions** | Czytelne asercje | Latest |
| **Moq** | Mockowanie zależności | 4.x |
| **WebApplicationFactory** | Integration testing API | Built-in ASP.NET Core |
| **Testcontainers.PostgreSQL** | Kontenery bazy danych w testach | Latest |
| **Bogus** | Generowanie danych testowych | Latest |
| **Respawn** | Database cleanup między testami | Latest |
| **Coverlet** | Code coverage | Latest |
| **ReportGenerator** | Wizualizacja code coverage (lokalne HTML) | Latest |

**❌ Pominięte w MVP**: SpecFlow (BDD testing)

**Instalacja (przykład .csproj)**:
```xml
<PackageReference Include="xunit" Version="2.6.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.6.0" />
<PackageReference Include="Bogus" Version="35.0.0" />
<PackageReference Include="Respawn" Version="6.2.0" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```

### 5.2 Frontend (Angular 20)

| Narzędzie | Przeznaczenie | Wersja |
|-----------|---------------|--------|
| **Jasmine** | Framework testowy (domyślny Angular) | Latest |
| **Karma** | Test runner | Latest |
| **Angular Testing Library** | Component testing | Latest |
| **ng-mocks** | Mockowanie Angular dependencies | Latest |
| **MSW (Mock Service Worker)** | API mocking | Latest |
| **Playwright** | E2E testing | Latest |

**❌ Pominięte w MVP**: Jest, axe-core, Lighthouse CI (accessibility post-MVP)

**Instalacja (Angular 20)**:
```bash
# E2E z Playwright
npm install -D @playwright/test

# Testing utilities
npm install -D @testing-library/angular ng-mocks msw
```

### 5.3 Database i Infrastructure

| Narzędzie | Przeznaczenie |
|-----------|---------------|
| **Testcontainers.PostgreSQL** | Izolowane kontenery bazy danych (z sekcji 5.1) |
| **Supabase CLI** | Database migrations i local testing |
| **Docker Compose** | Local environment orchestration |
| **k6** | Load/performance testing |

**❌ Pominięte w MVP**: pgTAP (testy RLS manualne), Postman/Newman (integration tests w .NET wystarczą)

### 5.4 CI/CD i Code Quality

| Narzędzie | Przeznaczenie |
|-----------|---------------|
| **GitHub Actions** | CI/CD pipeline automation (GitHub-hosted runners: ubuntu-latest) |
| **Snyk** | Dependency vulnerability scanning |
| **act** | Local GitHub Actions testing (opcjonalnie) |

**❌ Pominięte w MVP**: SonarQube, OWASP ZAP, Codecov/Coveralls (coverage tylko lokalnie)

### 5.5 Monitoring i Reporting

| Narzędzie | Przeznaczenie |
|-----------|---------------|
| **xUnit/Jasmine Reports** | Wbudowane raporty testowe (XML, console) |
| **ReportGenerator** | .NET code coverage reports (lokalne HTML) |
| **Playwright Trace Viewer** | E2E test debugging |

**❌ Pominięte w MVP**: Allure Framework, Application Insights, Sentry (monitoring post-MVP)

## 6. Środowiska Testowe

### 6.1 Local Development

**Opis**: Środowisko developerskie na lokalnych maszynach

**Komponenty**:
- Backend: `dotnet run` (https://localhost:8080)
- Frontend: `ng serve` (http://localhost:4200)
- Database: Supabase local instance (`npx supabase start`) lub remote dev instance
- Testcontainers: Automatyczne kontenery PostgreSQL dla testów integracyjnych

**Konfiguracja**:
- Pliki `.env.local` z kluczami Supabase dev
- User secrets w .NET dla API keys
- Mock data z Bogus

**Testy uruchamiane**:
- Unit tests (`dotnet test`, `ng test`)
- Integration tests z Testcontainers
- Smoke tests

### 6.2 CI/CD (GitHub Actions)

**Opis**: Środowisko automatyczne w GitHub Actions

**Workflow**:
```yaml
name: CI/CD Pipeline

on:
  pull_request:
  push:
    branches: [main, master, develop]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - Checkout code
      - Setup .NET 8
      - Restore dependencies
      - Run unit tests
      - Run integration tests (Testcontainers)
      - Upload code coverage to Codecov

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - Checkout code
      - Setup Node.js 20
      - Install dependencies (npm ci)
      - Run unit tests (ng test --watch=false)
      - Run lint (ng lint)
      - Build production (ng build --configuration production)

  e2e-tests:
    runs-on: ubuntu-latest
    needs: [backend-tests, frontend-tests]
    steps:
      - Checkout code
      - Setup environment (Docker Compose)
      - Run Playwright tests
      - Upload artifacts (screenshots, videos)

  security-scan:
    runs-on: ubuntu-latest
    steps:
      - Snyk vulnerability scan
      - OWASP ZAP scan (staging)
      - SonarQube analysis
```

**Testy uruchamiane**:
- Wszystkie unit tests (backend + frontend)
- Integration tests
- E2E tests (krytyczne scenariusze)
- Security scans
- Code quality analysis

**Metryki sukcesu**:
- Code coverage > 80% (backend services)
- Code coverage > 70% (frontend components)
- 0 critical vulnerabilities
- SonarQube Quality Gate: PASSED

### 6.3 Staging

**Opis**: Środowisko pre-production identyczne z production

**Komponenty**:
- Backend: Deployed na AWS (Docker container)
- Frontend: Deployed na AWS S3 + CloudFront
- Database: Supabase staging project
- Email: Testowy SMTP (Mailtrap)

**Konfiguracja**:
- Dane zbliżone do produkcyjnych (anonymized production data)
- Integracje z testowymi API (Stripe test mode)
- Monitorowanie (Application Insights, Sentry)

**Testy uruchamiane**:
- E2E tests (full suite)
- Performance tests (k6 load testing)
- Security tests (OWASP ZAP)
- User Acceptance Testing (UAT)
- Smoke tests po deployment

**Deployment**:
- Automatyczny deploy z branch `develop`
- Manual approval required dla production release

### 6.4 Production

**Opis**: Środowisko produkcyjne dla użytkowników końcowych

**Komponenty**:
- Backend: AWS ECS/Fargate z auto-scaling
- Frontend: AWS S3 + CloudFront CDN
- Database: Supabase production (backups, replication)
- Email: SendGrid/AWS SES

**Testy uruchamiane**:
- Smoke tests immediately after deployment
- Synthetic monitoring (Pingdom, Uptime Robot)
- Real User Monitoring (RUM)

**Rollback strategy**:
- Immediate rollback jeśli smoke tests fail
- Blue-Green deployment dla zero-downtime

## 7. Priorytetyzacja

### 7.1 Kryteria Priorytetyzacji

Priorytetyzacja oparta na **Risk-Based Testing**:

| Priorytet | Kryteria | Test Coverage | Przykłady |
|-----------|----------|---------------|-----------|
| **P0 - Krytyczne** | Blocker dla MVP, bezpieczeństwo, data loss risk | 95%+ | Autentykacja, RLS policies, Event completion logic, Plan usage limits |
| **P1 - Wysokie** | Kluczowe funkcjonalności MVP | 85%+ | CRUD Tasks/Events, Dashboard, Role permissions |
| **P2 - Średnie** | Ważne dla UX, ale nie blocker | 70%+ | Filtrowanie, Sortowanie, Responsive UI |
| **P3 - Niskie** | Nice-to-have, edge cases | 50%+ | System Developer panel, Advanced analytics |

### 7.2 Obszary Priorytetowe

#### P0 - Krytyczne (Must Have)

**1. Autentykacja i Autoryzacja**
- **Testy**: Unit + Integration + Security
- **Obszary**:
  - Rejestracja użytkownika przez Supabase Auth
  - Logowanie i JWT token generation
  - Token validation i expiration (30 dni)
  - Password reset flow
  - RLS policies - izolacja danych między gospodarstwami
- **Uzasadnienie**: Brak bezpieczeństwa = brak zaufania użytkowników

**2. Event Completion Workflow**
- **Testy**: Unit (EventService) + Integration API
- **Obszary**:
  - `EventService.CompleteEventAsync()` - automatyczne generowanie następnego wydarzenia
  - Kalkulacja next due date: completion_date + interval
  - Premium feature: zapis do `events_history`
- **Uzasadnienie**: Core value proposition aplikacji

**3. Plan Usage Tracking i Limity Freemium**
- **Testy**: Unit (PlanUsageService) + Integration
- **Obszary**:
  - `WouldExceedLimitAsync()` - walidacja przed operacją
  - `UpdateTasksUsageAsync()`, `UpdateMembersUsageAsync()` - aktualizacja usage
  - Limit enforcement: 5 tasks, 3 members (free plan)
  - Komunikaty o przekroczeniu limitu
- **Uzasadnienie**: Model biznesowy freemium - kluczowy dla monetyzacji

**4. Data Isolation (RLS)**
- **Testy**: Database Integration + Security
- **Obszary**:
  - RLS policies dla wszystkich tabel
  - User A nie widzi danych z household B
  - Multi-tenant architecture correctness
- **Uzasadnienie**: Krytyczne dla RODO i trust

**5. Soft Delete Pattern**
- **Testy**: Unit + Integration
- **Obszary**:
  - Wszystkie DELETE operations ustawiają `deleted_at` timestamp
  - Wszystkie SELECT queries filtrują `deleted_at IS NULL`
  - Admin może przywrócić soft-deleted items
- **Uzasadnienie**: Ochrona przed data loss, RODO compliance

#### P1 - Wysokie (Should Have)

**6. CRUD dla Tasks i Events**
- **Testy**: Unit + Integration API
- **Obszary**:
  - Tworzenie zadania z walidacją
  - Edycja zadania (zmiana interwału nie wpływa na istniejące wydarzenia)
  - Usuwanie zadania (soft delete)
  - Tworzenie wydarzenia z referencją do zadania
  - Edycja wydarzenia (data, osoba odpowiedzialna)
  - Anulowanie wydarzenia (status = cancelled)
- **Uzasadnienie**: Core functionality MVP

**7. Dashboard i Kalendarz**
- **Testy**: Unit (components) + E2E
- **Obszary**:
  - Dashboard z kalendarzem tygodniowym
  - Kalendarz miesięczny
  - Lista wydarzeń z filtrowaniem
  - Color-coding (overdue, today, upcoming)
- **Uzasadnienie**: Primary user interface

**8. Role-Based Access Control**
- **Testy**: Unit + Integration + E2E
- **Obszary**:
  - Administrator: full access
  - Domownik: edycja tylko przypisanych wydarzeń
  - Dashboard: read-only
  - System Developer: global access
- **Uzasadnienie**: Multi-user collaboration feature

**9. Zarządzanie Gospodarstwem**
- **Testy**: Unit + Integration
- **Obszary**:
  - Tworzenie gospodarstwa
  - Dodawanie członków z walidacją limitu
  - Usuwanie członków (reassignment wydarzeń)
  - Zmiana ról
- **Uzasadnienie**: Multi-tenant core feature

#### P2 - Średnie (Could Have)

**10. Filtry i Sortowanie**
- **Testy**: Unit + Integration
- **Obszary**:
  - Filtrowanie wydarzeń (osoba, kategoria, priorytet, status, daty)
  - Sortowanie (data, nazwa, priorytet)
  - Wyszukiwanie po nazwie
- **Uzasadnienie**: UX improvement, ale nie blocker

**11. Responsive UI**
- **Testy**: E2E (Playwright multi-viewport)
- **Obszary**:
  - Desktop (>1024px): sidebar persistent
  - Tablet (768-1024px): sidebar collapsible
  - Mobile (<768px): hamburger menu
  - Kalendarz responsive (days stack vertically)
- **Uzasadnienie**: Mobile usage expected

**12. Onboarding Flow**
- **Testy**: E2E
- **Obszary**:
  - Krok 1: Stwórz gospodarstwo
  - Krok 2: Dodaj pierwsze zadanie
  - Krok 3: Utwórz pierwsze wydarzenie
  - Skip option
- **Uzasadnienie**: User activation, ale można pominąć w MVP

#### P3 - Niskie (Won't Have w MVP)

**13. System Developer Panel**
- **Testy**: Integration + E2E (basic smoke tests tylko)
- **Uzasadnienie**: Admin functionality, niski impact na użytkowników końcowych

**14. Advanced Analytics (Premium)**
- **Testy**: Unit + Integration (basic)
- **Uzasadnienie**: Premium feature, post-MVP priority

**15. Export do CSV/PDF**
- **Testy**: Integration
- **Uzasadnienie**: Nice-to-have feature

### 7.3 Harmonogram Implementacji Testów

**Tydzień 1-2: Fundamenty**
- Setup projektu testowego (xUnit, Jasmine, Playwright)
- Konfiguracja Testcontainers
- Implementacja testów P0: Autentykacja (10 test cases)
- Implementacja testów P0: RLS policies (15 test cases)

**Tydzień 3-4: Core Logic**
- Testy P0: EventService (20 test cases)
- Testy P0: PlanUsageService (15 test cases)
- Testy P0: Soft delete pattern (10 test cases)
- Integration tests: API endpoints P0 (25 test cases)

**Tydzień 5-6: CRUD i Dashboard**
- Testy P1: TaskService, HouseholdMemberService (30 test cases)
- Testy P1: Dashboard components (15 test cases)
- Testy P1: RBAC integration tests (20 test cases)
- E2E: Critical user journeys P0 (5 scenariuszy)

**Tydzień 7-8: Finalizacja MVP**
- Testy P2: Filtrowanie, sortowanie (15 test cases)
- Testy P2: Responsive UI E2E (10 test cases)
- Security tests: OWASP Top 10 scanning
- Performance tests: k6 load testing (5 scenariuszy)
- Code coverage analysis i gap filling

**Tydzień 9+: Maintenance i Expansion**
- Regression suite optimization
- CI/CD pipeline tuning
- Documentation
- P3 tests (jeśli czas pozwoli)

## 8. Kryteria Akceptacji

### 8.1 Kryteria Zakończenia Testów (Definition of Done)

Testy można uznać za zakończone, gdy:

**Code Coverage**:
- ✅ Backend services: ≥ 80% code coverage (line coverage)
- ✅ Frontend components: ≥ 70% code coverage
- ✅ Krytyczne services (EventService, PlanUsageService): ≥ 95% coverage
- ✅ API controllers: 100% endpoint coverage (minimum smoke tests)

**Test Execution**:
- ✅ Wszystkie testy P0 i P1 przechodzą (0 failed tests)
- ✅ Testy P2: < 5% failure rate (edge cases)
- ✅ E2E tests: 100% pass rate dla krytycznych scenariuszy
- ✅ Regression suite: 100% pass rate

**Security**:
- ✅ 0 critical vulnerabilities (Snyk, OWASP ZAP)
- ✅ 0 high severity issues (SonarQube)
- ✅ RLS policies tests: 100% pass rate
- ✅ Authentication tests: 100% pass rate

**Performance**:
- ✅ API response time: p95 < 200ms
- ✅ Dashboard load time: < 2s (Lighthouse)
- ✅ Load test: 100 concurrent users bez error rate > 1%

**Documentation**:
- ✅ Test plan zatwierdzony przez zespół
- ✅ Test cases udokumentowane (README w projekcie testowym)
- ✅ Known issues / limitations udokumentowane

**CI/CD**:
- ✅ GitHub Actions pipeline green (all checks pass)
- ✅ Automated deployment do staging działa
- ✅ Smoke tests po deployment przechodzą

### 8.2 Entry Criteria (Warunki Wstępne)

Zanim rozpoczną się testy, wymagane jest:

- ✅ Implementacja funkcjonalności gotowa (feature complete)
- ✅ Code review zakończony i zaakceptowany
- ✅ Unit tests napisane przez developera
- ✅ API documentation (Swagger) aktualna
- ✅ Test environment gotowy (staging)
- ✅ Test data fixtures przygotowane

### 8.3 Exit Criteria (Warunki Wyjścia)

Testy są zakończone gdy:

- ✅ Wszystkie kryteria z sekcji 8.1 spełnione
- ✅ Bug triage zakończony:
  - 0 Critical bugs
  - 0 High bugs (lub zaakceptowane jako known issues)
  - < 5 Medium bugs (z planem fix w następnej iteracji)
- ✅ Test report wygenerowany i zatwierdzony
- ✅ Regression suite zaktualizowana
- ✅ Go/No-Go decision podjęta dla release

### 8.4 Metryki Sukcesu

**Metryki jakości testów**:
- **Test Pass Rate**: > 98% (testy stabilne, nie flaky)
- **Code Coverage Delta**: każdy PR musi utrzymać lub zwiększyć coverage
- **Bug Detection Rate**: > 80% bugów znalezionych przed production
- **Test Execution Time**: Full suite < 15 minut (CI/CD)

**Metryki biznesowe** (związane z testowaniem):
- **Mean Time to Detection (MTTD)**: < 24h od wdrożenia buga
- **Mean Time to Resolution (MTTR)**: < 48h dla critical bugs
- **Deployment Frequency**: > 1x/tydzień (dzięki automated testing)
- **Change Failure Rate**: < 5% (dzięki regression tests)

## 9. Harmonogram

### 9.1 Timeline Implementacji Testów

**Założenia**:
- MVP development trwa 12 tygodni
- Testing rozpoczyna się od Tygodnia 1 (równolegle z development - TDD)
- 1 dedykowany tester + deweloperzy piszą testy

| Tydzień | Faza | Aktywności Testowe | Deliverables |
|---------|------|-------------------|--------------|
| **1-2** | Setup + Auth | - Setup xUnit, Jasmine, Playwright<br>- Testcontainers config<br>- Testy autentykacji (P0)<br>- Testy RLS policies (P0) | - Test project structure<br>- CI/CD pipeline (basic)<br>- 25 test cases (P0) |
| **3-4** | Core Logic | - EventService tests (P0)<br>- PlanUsageService tests (P0)<br>- Soft delete tests (P0)<br>- API integration tests (P0) | - 70 test cases (P0)<br>- Code coverage > 80% services<br>- Integration test suite |
| **5-6** | CRUD + UI | - TaskService, HouseholdService (P1)<br>- Dashboard component tests (P1)<br>- RBAC tests (P1)<br>- E2E critical paths (P0) | - 65 test cases (P1)<br>- 5 E2E scenarios<br>- Frontend coverage > 70% |
| **7-8** | Polish | - Filtry, sortowanie (P2)<br>- Responsive UI tests (P2)<br>- Security scanning (OWASP, Snyk)<br>- Performance tests (k6) | - 25 test cases (P2)<br>- Security report<br>- Performance baseline<br>- Regression suite |
| **9-10** | Stabilization | - Bug fixing<br>- Flaky tests stabilization<br>- Code coverage gap analysis<br>- Documentation | - Test report<br>- Coverage report (>80%)<br>- Test documentation |
| **11-12** | UAT + Release Prep | - User Acceptance Testing<br>- Production smoke tests<br>- Deployment runbook<br>- Final regression suite | - UAT sign-off<br>- Release checklist<br>- Go/No-Go decision |

### 9.2 Milestones

**Milestone 1 (Tydzień 4): Core Testing Foundation**
- ✅ Test infrastructure ready (Testcontainers, CI/CD)
- ✅ P0 tests implemented (95 test cases)
- ✅ Code coverage backend services > 80%
- **Gate**: Code review of test architecture

**Milestone 2 (Tydzień 8): Feature Complete Testing**
- ✅ P0 + P1 tests complete (160 test cases)
- ✅ E2E critical paths (5 scenarios)
- ✅ Security scan passed (0 critical issues)
- ✅ Performance baseline established
- **Gate**: QA sign-off, no critical bugs

**Milestone 3 (Tydzień 12): Release Ready**
- ✅ All P0, P1, P2 tests passing
- ✅ Regression suite automated
- ✅ UAT completed
- ✅ Production deployment tested (staging)
- **Gate**: Go/No-Go decision for production release

### 9.3 Continuous Activities (Ongoing)

**Podczas całego projektu**:
- **Daily**: Unit tests run by developers (pre-commit)
- **Per PR**: CI/CD pipeline (unit + integration tests)
- **Weekly**:
  - Regression suite execution (full)
  - Performance tests (k6)
  - Security scans (Snyk, SonarQube)
- **Bi-weekly**:
  - Test metrics review (coverage, pass rate)
  - Flaky test cleanup
- **Monthly**:
  - Test strategy review
  - Tool evaluation (nowe wersje narzędzi)

### 9.4 Post-MVP Testing (Maintenance)

**Po release MVP** (ongoing):
- Regression tests before każdego release (biweekly/monthly cadence)
- Monitoring real user issues → regression tests updates
- Performance monitoring → performance test updates
- New features → TDD approach (tests first)
- Quarterly security audits (penetration testing)

## 10. Ryzyka i Mitygacja

### 10.1 Ryzyka Techniczne

| # | Ryzyko | Prawdopodobieństwo | Impact | Mitygacja | Owner |
|---|--------|-------------------|--------|-----------|-------|
| **R1** | **RLS policies trudne do przetestowania** - Supabase RLS wymaga specjalnego contextu użytkownika | Wysokie | Wysoki | - Dedykowane testy z Testcontainers PostgreSQL<br>- Helper functions do symulacji różnych user contexts<br>- Testy z `SET LOCAL role` w PostgreSQL<br>- Code review RLS policies przez security expert | Backend Lead |
| **R2** | **Flaky E2E tests** - testy Playwright mogą być niestabilne (timing issues, async) | Średnie | Średni | - Użycie Playwright auto-waiting mechanisms<br>- Explicit waits dla network requests<br>- Retry logic dla transient failures<br>- Isolation: każdy test z czystą bazą (Respawn)<br>- Screenshot/video recording dla debugging | QA Engineer |
| **R3** | **Testcontainers wolne w CI/CD** - podnoszenie kontenerów PostgreSQL może wydłużyć pipeline | Średnie | Średni | - Caching Docker images w GitHub Actions<br>- Parallel test execution<br>- Selective test runs (tylko dla zmian w DB schema)<br>- Rozważyć shared test database dla non-conflicting tests | DevOps |
| **R4** | **Synchronizacja EF migrations z Supabase schema** - ryzyko desynchronizacji | Wysokie | Wysoki | - Integration tests weryfikujące schema compatibility<br>- Automated migration testing w CI/CD<br>- Supabase CLI w pipeline (npx supabase db diff)<br>- Manual review: EF migration vs Supabase migration | Backend Lead |
| **R5** | **N+1 query problem** - EF Core może generować nieoptymalne queries | Średnie | Średni | - Code review guidelines (mandatory .Include())<br>- Performance tests z k6 (query count monitoring)<br>- EF Core logging w dev environment<br>- Database query profiler (pg_stat_statements) | Backend Lead |
| **R6** | **Soft delete pattern - easy to forget filters** | Wysokie | Średni | - Global query filter w EF Core: `HasQueryFilter(e => e.DeletedAt == null)`<br>- Unit tests weryfikujące soft delete w każdym repository<br>- Code review checklist<br>- Linting rules (custom analyzer) | Backend Lead |
| **R7** | **JWT token expiration issues** (30 dni) | Niskie | Średni | - Unit tests dla token expiration scenarios<br>- Integration tests z expired tokens<br>- Monitoring real user sessions (Application Insights)<br>- Refresh token flow testowany | Backend Lead |
| **R8** | **Premium feature flags testing** - conditional logic (free vs premium) | Średnie | Średni | - Parametrized tests (xUnit Theory) dla różnych planów<br>- Helper fixtures dla każdego typu planu<br>- E2E tests dla upgrade flow<br>- Database seedy z różnymi planami | QA Engineer |

### 10.2 Ryzyka Organizacyjne

| # | Ryzyko | Prawdopodobieństwo | Impact | Mitygacja | Owner |
|---|--------|-------------------|--------|-----------|-------|
| **R9** | **Ograniczony budżet czasu - MVP w 12 tygodni** | Wysokie | Wysoki | - Priorytetyzacja P0 > P1 > P2<br>- Automatyzacja gdzie możliwe (CI/CD)<br>- TDD: testy pisane podczas development, nie po<br>- Realistyczne scope (nie wszystkie edge cases w MVP) | Project Manager |
| **R10** | **Brak dedykowanego QA team** - deweloperzy piszą testy | Wysokie | Średni | - Code review testów przez senior devs<br>- Pair testing dla krytycznych obszarów<br>- Test guidelines i best practices document<br>- 1 dedykowany tester (part-time) dla E2E | Tech Lead |
| **R11** | **Zmiana requirements** - PRD może ewoluować | Średnie | Średni | - Regression tests aktualizowane przy zmianach<br>- Test plan review co 2 tygodnie<br>- Flexible test architecture (BDD SpecFlow) dla zmian | Product Owner |
| **R12** | **Niska adopcja testów przez team** | Niskie | Wysoki | - Szkolenia z xUnit, Playwright, TDD<br>- Showcase: demo wartości automated tests<br>- Mandatory code coverage minimum (80%)<br>- Gamification: leaderboard code coverage | Tech Lead |
| **R13** | **Test debt accumulation** - pominięcie testów dla speed | Średnie | Wysoki | - Definition of Done: code + tests<br>- PR nie może być merge bez testów<br>- Technical debt tracking (SonarQube)<br>- Dedicated "test cleanup" sprints co 2 miesiące | Tech Lead |

### 10.3 Ryzyka Infrastrukturalne

| # | Ryzyko | Prawdopodobieństwo | Impact | Mitygacja | Owner |
|---|--------|-------------------|--------|-----------|-------|
| **R14** | **Supabase downtime** - external dependency | Niskie | Wysoki | - Fallback: Testcontainers dla local/CI testing<br>- Supabase uptime SLA monitoring<br>- Backup test environment (local PostgreSQL)<br>- Circuit breaker pattern w integration tests | DevOps |
| **R15** | **GitHub Actions quota limits** (free tier) | Średnie | Średni | - Optimize pipeline (parallel jobs, caching)<br>- Selective test runs (nie full suite dla każdego commita)<br>- Self-hosted runners (jeśli budget pozwoli)<br>- Monitor usage dashboard | DevOps |
| **R16** | **AWS deployment issues** - staging environment down | Niskie | Średni | - Smoke tests with retry logic<br>- Rollback automation<br>- Health checks przed deployment<br>- Monitoring alerts (PagerDuty) | DevOps |
| **R17** | **Test data privacy** - staging może mieć real user data | Niskie | Wysoki | - NIGDY production data w staging<br>- Anonymized data lub synthetic data (Bogus)<br>- RODO compliance check<br>- Access controls (tylko authorized personnel) | Security Lead |

### 10.4 Ryzyka Bezpieczeństwa

| # | Ryzyko | Prawdopodobieństwo | Impact | Mitygacja | Owner |
|---|--------|-------------------|--------|-----------|-------|
| **R18** | **RLS bypass vulnerability** | Niskie | Krytyczny | - Penetration testing przed production<br>- Code review: każda RLS policy przez 2 osoby<br>- Automated RLS tests w CI/CD (mandatory)<br>- Security audit (external) | Security Lead |
| **R19** | **Secrets exposure** - API keys w kodzie/testach | Średnie | Wysoki | - GitHub secret scanning enabled<br>- .gitignore dla .env files<br>- User secrets w .NET<br>- Pre-commit hooks (detect-secrets) | DevOps |
| **R20** | **Dependency vulnerabilities** | Średnie | Średni | - Snyk scan w CI/CD (blocking)<br>- Dependabot alerts<br>- Quarterly dependency updates<br>- OWASP Top 10 checklist | Security Lead |

### 10.5 Plan Mitygacji - Action Items

**Natychmiastowe (Tydzień 1)**:
- [ ] Setup Testcontainers dla RLS testing (R1)
- [ ] Create EF + Supabase migration verification test (R4)
- [ ] Configure GitHub Actions caching (R15)
- [ ] Setup pre-commit hooks (git-secrets) (R19)
- [ ] Enable GitHub secret scanning (R19)

**Krótkoterminowe (Tydzień 1-4)**:
- [ ] Write soft delete global query filter (R6)
- [ ] Create test data fixtures dla różnych planów (R8)
- [ ] Setup SonarQube quality gates (R13)
- [ ] Implement Respawn for database cleanup (R2)
- [ ] Configure Snyk in CI/CD (R20)

**Średnioterminowe (Tydzień 5-8)**:
- [ ] Conduct RLS security review session (R18)
- [ ] Create test best practices documentation (R10)
- [ ] Setup Application Insights monitoring (R7)
- [ ] Implement circuit breaker for Supabase (R14)
- [ ] Configure PagerDuty alerts (R16)

**Długoterminowe (Post-MVP)**:
- [ ] External security audit (R18)
- [ ] Evaluate self-hosted GitHub runners (R15)
- [ ] Quarterly penetration testing (R18, R20)
- [ ] Test cleanup sprints scheduling (R13)

### 10.6 Risk Review Cadence

- **Weekly**: Review high-priority risks (R1, R4, R6, R9) - status check
- **Bi-weekly**: Full risk register review - update probabilities i mitigation status
- **Monthly**: Risk retrospective - new risks identification
- **Quarterly**: External audit results review - update security risks

### 10.7 Escalation Path

**Jeśli ryzyko się zmaterializuje**:
1. **Low Impact**: Team Lead resolve (dokumentacja w JIRA)
2. **Medium Impact**: Project Manager + Tech Lead (mitigation plan w 24h)
3. **High/Critical Impact**: Immediate escalation do Product Owner + CTO (emergency meeting)

**Kryteria eskalacji**:
- Critical bugs found < 1 tydzień przed release → Go/No-Go decision
- Security vulnerability (CVSS > 7.0) → Immediate fix required
- Code coverage drop < 75% → Blocking merge
- Test execution time > 30 min → Pipeline optimization sprint

---

## 11. Finalne Narzędzia Testowe - Podsumowanie MVP

### 11.1 Stack Technologiczny Testów

**Backend (.NET 9)**:
```
✅ xUnit - framework testowy
✅ FluentAssertions - czytelne asercje
✅ Moq - mockowanie zależności
✅ WebApplicationFactory - integration testing API
✅ Testcontainers.PostgreSQL - izolowane kontenery bazy danych
✅ Bogus - generowanie danych testowych
✅ Respawn - czyszczenie bazy między testami
✅ Coverlet - code coverage collection
✅ ReportGenerator - lokalne HTML raporty coverage

❌ Pominięte: SpecFlow (BDD)
```

**Frontend (Angular 20)**:
```
✅ Jasmine + Karma - framework testowy (default Angular)
✅ Angular Testing Library - component testing
✅ ng-mocks - mockowanie Angular dependencies
✅ MSW (Mock Service Worker) - API mocking
✅ Playwright - E2E testing

❌ Pominięte: Jest, axe-core, Lighthouse CI
```

**Database & Infrastructure**:
```
✅ Testcontainers.PostgreSQL - integration tests
✅ Supabase CLI - migrations
✅ Docker Compose - local environment
✅ k6 - performance testing

❌ Pominięte: pgTAP (testy RLS manualne)
```

**CI/CD & Security**:
```
✅ GitHub Actions - GitHub-hosted runners (ubuntu-latest)
✅ Snyk - dependency vulnerability scanning

❌ Pominięte: SonarQube, OWASP ZAP, Codecov
```

**Monitoring & Reporting**:
```
✅ xUnit/Jasmine wbudowane raporty
✅ ReportGenerator - coverage HTML (lokalnie)
✅ Playwright Trace Viewer - E2E debugging

❌ Pominięte: Allure, Application Insights, Sentry
```

### 11.2 Pominięte w MVP (Post-MVP)

| Kategoria | Narzędzia | Uzasadnienie |
|-----------|-----------|--------------|
| **BDD** | SpecFlow | MVP z developerami - xUnit wystarczy |
| **Contract Testing** | Pact | Jeden zespół - integration tests wystarczą |
| **Security** | SonarQube, OWASP ZAP | Snyk wystarczy dla MVP |
| **Accessibility** | axe-core, Lighthouse | Skupienie na funkcjonalności |
| **Database** | pgTAP | Testcontainers + manualne testy RLS |
| **Monitoring** | Application Insights, Sentry | Tylko logi w MVP |
| **Reporting** | Allure, Codecov | Lokalne raporty wystarczą |

### 11.3 Harmonogram Wdrożenia Narzędzi

**Tydzień 1-2** (Setup):
- [ ] Konfiguracja xUnit projektu testowego (.NET 9)
- [ ] Instalacja FluentAssertions, Moq, Bogus
- [ ] Setup Testcontainers.PostgreSQL
- [ ] Setup Respawn dla database cleanup
- [ ] Konfiguracja Coverlet + ReportGenerator
- [ ] Setup Jasmine/Karma dla Angular (domyślne)
- [ ] Instalacja Playwright dla E2E
- [ ] Konfiguracja GitHub Actions (basic pipeline)

**Tydzień 3-4** (First Tests):
- [ ] Pierwsze testy unit (xUnit + FluentAssertions)
- [ ] Pierwsze integration tests (WebApplicationFactory)
- [ ] Setup Snyk w GitHub Actions
- [ ] Pierwsze testy frontend (Jasmine)

**Tydzień 5-6** (E2E & Performance):
- [ ] Pierwsze E2E testy (Playwright)
- [ ] Setup k6 dla performance tests (basic)
- [ ] Manualne testy RLS (dokumentacja)

**Tydzień 7+** (Optimization):
- [ ] CI/CD pipeline tuning
- [ ] Test coverage monitoring
- [ ] Regression suite stabilization

---

## Podsumowanie

Ten plan testów zapewnia kompleksowe pokrycie funkcjonalności aplikacji Homely, z naciskiem na:

1. **Bezpieczeństwo** - RLS policies, autentykacja, RODO compliance
2. **Krytyczna logika biznesowa** - EventService completion workflow, PlanUsageService limits
3. **Automatyzacja** - CI/CD z GitHub Actions, regression suite
4. **Priorytetyzacja** - P0 (95% coverage) > P1 (85%) > P2 (70%)
5. **Risk mitigation** - 20 zidentyfikowanych ryzyk z konkretną mitygacją

**Kluczowe metryki sukcesu**:
- ✅ 80%+ code coverage (backend services)
- ✅ 70%+ code coverage (frontend)
- ✅ 0 critical security vulnerabilities
- ✅ < 15 min full test suite execution time
- ✅ 98%+ test pass rate (stable tests)

Plan jest elastyczny i będzie aktualizowany co 2 tygodnie na podstawie postępów w rozwoju MVP i feedbacku zespołu.
