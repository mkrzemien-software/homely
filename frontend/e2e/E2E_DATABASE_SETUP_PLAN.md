# Plan Implementacji Dedykowanej Bazy Danych dla Testów E2E

---

## 📌 Cel

Utworzenie dedykowanego środowiska bazodanowego dla testów E2E przy użyciu Docker Compose, które zapewni:
- Pełną izolację testów od środowiska deweloperskiego
- Deterministyczne środowisko testowe
- Łatwe uruchamianie i czyszczenie bazy danych
- Automatyczne zarządzanie cyklem życia środowiska testowego

---

## 🎯 Wymagania Funkcjonalne

### Wybrane Rozwiązania

| Aspekt | Wybrane Rozwiązanie |
|--------|-------------------|
| **Baza danych** | Osobna instancja Supabase lokalnie w Docker |
| **Reset bazy** | Przed każdym testem (TRUNCATE CASCADE) |
| **Dane testowe** | Mix: seed użytkowników + fixtures dla danych |
| **Credentials** | Docker Compose z predefiniowaną konfiguracją |
| **Lifecycle** | Docker Compose dla całego stack'u E2E |
| **Backend** | Backend w Docker łączący się z bazą E2E |
| **Zakres MVP** | Setup infrastruktury + Global setup i database helpers |

---

## 🏗️ Architektura

```
┌─────────────────────────────────────────────────────────────┐
│                    Docker Compose E2E                       │
│                                                             │
│  ┌──────────────┐      ┌──────────────┐     ┌──────────┐  │
│  │   Supabase   │◄─────┤  Backend API │◄────┤ Playwright│  │
│  │   Stack      │      │  (Port 8081) │     │  Tests    │  │
│  │              │      │              │     │           │  │
│  │ - Kong:54010 │      │ ASPNETCORE_  │     │ localhost │  │
│  │ - DB:54011   │      │ ENVIRONMENT= │     │ :4200     │  │
│  │ - Auth:54012 │      │ E2E          │     │           │  │
│  │ - REST:54013 │      │              │     │           │  │
│  │ - Realtime   │      │              │     │           │  │
│  │ - Storage    │      │              │     │           │  │
│  └──────────────┘      └──────────────┘     └──────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Porty

| Serwis | Port | Opis |
|--------|------|------|
| Supabase Kong (API Gateway) | 54010 | Główny endpoint API |
| PostgreSQL | 54011 | Baza danych |
| Supabase Auth | 54012 | Autentykacja |
| Supabase REST | 54013 | REST API |
| Supabase Realtime | 54014 | WebSocket realtime |
| Supabase Storage | 54015 | File storage |
| Supabase Meta | 54016 | Metadata API |
| Backend API | 8081 | ASP.NET Core API |
| Frontend | 4200 | Angular dev server (poza Docker) |

---

## 📁 Struktura Plików

```
homely/
├── backend/
│   ├── Dockerfile (✅ już istnieje)
│   └── HomelyApi/
│       └── Homely.API/
│           └── appsettings.E2E.json (🆕 do utworzenia)
│
├── database/
│   └── supabase/
│       ├── config.toml (✅ istniejący, współdzielony)
│       └── migrations/ (✅ istniejące, współdzielone)
│
├── frontend/
│   ├── e2e/
│   │   ├── global-setup.ts (🆕 do utworzenia)
│   │   ├── helpers/
│   │   │   └── db-helper.ts (🆕 do utworzenia)
│   │   ├── fixtures/
│   │   │   ├── test-data.ts (✅ istniejący)
│   │   │   └── database-fixture.ts (🆕 do utworzenia)
│   │   └── E2E_DATABASE_SETUP_PLAN.md (✅ ten plik)
│   │
│   ├── playwright.config.ts (🔄 do aktualizacji)
│   └── package.json (🔄 do aktualizacji - npm scripts)
│
├── docker-compose.e2e.yml (🆕 do utworzenia)
└── .env.e2e.example (🆕 do utworzenia)
```

---

## 🔧 Szczegóły Implementacji

### 1. docker-compose.e2e.yml

**Cel**: Orkiestracja całego środowiska E2E

**Serwisy**:
- `postgres-e2e`: PostgreSQL 17
- `kong-e2e`: Supabase Kong Gateway
- `auth-e2e`: Supabase Auth
- `rest-e2e`: Supabase REST API (PostgREST)
- `realtime-e2e`: Supabase Realtime
- `storage-e2e`: Supabase Storage
- `meta-e2e`: Supabase Meta
- `backend-e2e`: ASP.NET Core API

**Kluczowe cechy**:
- Wszystkie serwisy w dedykowanej sieci `e2e-network`
- Healthchecks dla każdego serwisu
- Depends_on z condition: service_healthy
- Volumes dla persist danych PostgreSQL
- Environment variables inline (nie potrzebny .env dla Docker)

**Migracje**:
- Montowanie `./database/supabase/migrations:/docker-entrypoint-initdb.d` dla automatycznych migracji
- Alternatywnie: uruchamianie przez `supabase db push` w global-setup

---

### 2. appsettings.E2E.json

**Lokalizacja**: `backend/HomelyApi/Homely.API/appsettings.E2E.json`

**Zawartość**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres-e2e;Port=5432;Database=postgres;Username=postgres;Password=postgres"
  },
  "Supabase": {
    "Url": "http://kong-e2e:8000",
    "Key": "[SERVICE_ROLE_KEY_FROM_SUPABASE]",
    "ServiceRoleKey": "[SERVICE_ROLE_KEY_FROM_SUPABASE]"
  },
  "JwtSettings": {
    "ValidIssuer": "http://kong-e2e:8000/auth/v1",
    "ValidAudience": "authenticated",
    "Secret": "[JWT_SECRET_FROM_SUPABASE]"
  },
  "Environment": {
    "Name": "E2E",
    "Description": "E2E Testing Environment"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

**Uwagi**:
- Nazwy hostów to nazwy serwisów z docker-compose (np. `postgres-e2e`, `kong-e2e`)
- Keys i secrets będą generowane przez Supabase podczas startu

---

### 3. global-setup.ts

**Lokalizacja**: `frontend/e2e/global-setup.ts`

**Odpowiedzialności**:
1. Sprawdzenie czy Docker Compose działa (healthcheck)
2. Opcjonalnie: uruchomienie `docker-compose up` jeśli nie działa
3. Oczekiwanie na healthchecks wszystkich serwisów
4. Uruchomienie migracji Supabase (jeśli nie były auto-applied)
5. Utworzenie 3 użytkowników testowych przez Supabase Auth API

**Użytkownicy do utworzenia**:
```typescript
const TEST_USERS = [
  { email: 'admin@e2e.homely.com', password: 'Test123!@#', role: 'admin' },
  { email: 'member@e2e.homely.com', password: 'Test123!@#', role: 'member' },
  { email: 'dashboard@e2e.homely.com', password: 'Test123!@#', role: 'dashboard' }
];
```

**API Call**:
```typescript
POST http://localhost:54010/auth/v1/signup
Content-Type: application/json
apikey: [ANON_KEY]

{
  "email": "admin@e2e.homely.com",
  "password": "Test123!@#"
}
```

**Uwagi**:
- Musi być idempotentny (sprawdzać czy użytkownicy już istnieją)
- Timeout dla healthchecks: 60 sekund
- Logowanie postępu do konsoli

---

### 4. db-helper.ts

**Lokalizacja**: `frontend/e2e/helpers/db-helper.ts`

**Funkcje**:

#### `truncateAllTables()`
```typescript
/**
 * Truncates all tables in the database except auth.users
 * Uses CASCADE to handle foreign key constraints
 */
async function truncateAllTables(): Promise<void>
```

**Implementacja**:
1. Połączenie z PostgreSQL przez `pg` library lub Supabase client
2. Pobranie listy wszystkich tabel z `information_schema.tables`
3. Wykluczenie tabel systemowych (`auth.*`, `storage.*`, `pg_*`, `information_schema`)
4. Wykonanie `TRUNCATE TABLE table1, table2, ... CASCADE`

**Tabele do wyczyszczenia**:
- `households`
- `household_members`
- `categories`
- `category_types`
- `tasks`
- `events`
- `events_history`
- `plan_usage`

**NIE czyścić**:
- `auth.users` (użytkownicy są seed'owani w global-setup)
- `plan_types` (dane referencyjne)

#### `resetSequences()` (opcjonalne)
```typescript
/**
 * Resets all sequences to 1
 */
async function resetSequences(): Promise<void>
```

**Uwagi**:
- Connection string: `postgresql://postgres:postgres@localhost:54011/postgres`
- Error handling z retry mechanism (3 próby)
- Logowanie operacji do konsoli

---

### 5. database-fixture.ts

**Lokalizacja**: `frontend/e2e/fixtures/database-fixture.ts`

**Cel**: Playwright fixture do automatycznego czyszczenia bazy

**Implementacja**:
```typescript
import { test as base } from '@playwright/test';
import { truncateAllTables } from '../helpers/db-helper';

export const test = base.extend({
  cleanDatabase: [async ({}, use) => {
    // Setup: Clean database before test
    await truncateAllTables();

    // Run the test
    await use();

    // Teardown: optionally clean after test (not needed)
  }, { auto: true }]
});

export { expect } from '@playwright/test';
```

**Użycie w testach**:
```typescript
import { test, expect } from './fixtures/database-fixture';

test('should create category', async ({ page, cleanDatabase }) => {
  // cleanDatabase automatically runs before this test
  // ...
});
```

---

### 6. playwright.config.ts - Aktualizacje

**Zmiany**:

```typescript
import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',

  // Add global setup
  globalSetup: require.resolve('./e2e/global-setup.ts'),

  // Update base URL to use backend in Docker
  use: {
    baseURL: 'http://localhost:4200',

    // Add environment variables for tests
    extraHTTPHeaders: {
      'X-Test-Environment': 'E2E'
    }
  },

  // Update webServer config (frontend still runs locally)
  webServer: {
    command: 'npm start',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },

  // Existing config...
});
```

**Environment dla testów**:
```typescript
// Dostępne w testach przez process.env
process.env.E2E_API_URL = 'http://localhost:8081';
process.env.E2E_SUPABASE_URL = 'http://localhost:54010';
process.env.E2E_DB_URL = 'postgresql://postgres:postgres@localhost:54011/postgres';
```

---

### 7. NPM Scripts

**Lokalizacja**: `frontend/package.json`

**Nowe komendy**:
```json
{
  "scripts": {
    "e2e:docker:start": "docker-compose -f ../docker-compose.e2e.yml up -d",
    "e2e:docker:stop": "docker-compose -f ../docker-compose.e2e.yml down",
    "e2e:docker:clean": "docker-compose -f ../docker-compose.e2e.yml down -v",
    "e2e:docker:logs": "docker-compose -f ../docker-compose.e2e.yml logs -f",
    "e2e:docker:logs:backend": "docker-compose -f ../docker-compose.e2e.yml logs -f backend-e2e",
    "e2e:docker:restart": "npm run e2e:docker:stop && npm run e2e:docker:start",
    "e2e": "playwright test",
    "e2e:headed": "playwright test --headed",
    "e2e:ui": "playwright test --ui",
    "e2e:debug": "playwright test --debug"
  }
}
```

**Workflow**:
```bash
# 1. Start Docker stack
npm run e2e:docker:start

# 2. Wait for healthchecks (handled by global-setup)

# 3. Run tests
npm run e2e

# 4. Stop Docker stack
npm run e2e:docker:stop

# Clean everything (including volumes)
npm run e2e:docker:clean
```

---

## 🔄 Workflow Developera

### Pierwsze uruchomienie

```bash
# 1. Zbuduj obrazy Docker (jeśli trzeba)
cd /c/Users/mkrzemien/mk-projects/homely
docker-compose -f docker-compose.e2e.yml build

# 2. Uruchom stack E2E
npm run e2e:docker:start

# 3. Sprawdź logi (opcjonalnie)
npm run e2e:docker:logs

# 4. Uruchom testy (global-setup automatycznie przygotuje środowisko)
cd frontend
npm run e2e

# 5. Zatrzymaj stack
npm run e2e:docker:stop
```

### Codzienne użycie

```bash
# NAJSZYBCIEJ: Pełny workflow (start → testy → stop)
cd frontend
npm run e2e:full

# Lub pełny workflow z czyszczeniem volumes
npm run e2e:full:clean

# Lub tylko testy (jeśli stack już działa)
npm run e2e

# Debug pojedynczego testu
npm run e2e:debug -- category-management.spec.ts
```

### Troubleshooting

```bash
# Pełny reset środowiska
npm run e2e:docker:clean
npm run e2e:docker:start

# Sprawdzenie statusu kontenerów
docker-compose -f docker-compose.e2e.yml ps

# Logi z konkretnego serwisu
npm run e2e:docker:logs:backend

# Połączenie do bazy bezpośrednio
psql postgresql://postgres:postgres@localhost:54011/postgres

# Restart pojedynczego serwisu
docker-compose -f docker-compose.e2e.yml restart backend-e2e
```

---

## 📝 Szczegóły Techniczne

### Supabase w Docker

**Obrazy Docker** (oficjalne od Supabase):
- `supabase/postgres:17` - PostgreSQL z Supabase extensions
- `kong:3.0` - API Gateway
- `supabase/gotrue:v2` - Auth service
- `postgrest/postgrest:v12` - REST API
- `supabase/realtime:v2` - Realtime service
- `supabase/storage-api:v1` - Storage service
- `supabase/postgres-meta:v0.80` - Meta API

**Konfiguracja**:
- JWT Secret musi być taki sam dla wszystkich serwisów
- Anon Key i Service Role Key generowane przez Supabase
- RLS policies działają tak samo jak w lokalnym Supabase CLI

### Backend w Docker

**Build**:
```bash
docker build \
  --build-arg BUILD_ENV=E2E \
  -t homely-backend-e2e \
  -f backend/Dockerfile \
  .
```

**Environment**:
- `ASPNETCORE_ENVIRONMENT=E2E` - ładuje appsettings.E2E.json
- `ASPNETCORE_URLS=http://+:8080` - wewnętrzny port
- Port mapping: `8081:8080` (host:container)

### Database Cleanup Strategy

**TRUNCATE CASCADE**:
```sql
-- Przykład implementacji w db-helper.ts
TRUNCATE TABLE
  households,
  household_members,
  categories,
  category_types,
  tasks,
  events,
  events_history,
  plan_usage
CASCADE;
```

**Zalety**:
- Szybkie (10-50ms dla pustych tabel)
- Zachowuje strukturę tabel
- CASCADE automatycznie obsługuje foreign keys

**Wady**:
- NIE resetuje sequences (ID może rosnąć)
- Nie czyści `auth.users` (celowo)

**Alternatywy**:
- DELETE FROM: wolniejsze, ale resetuje sequences jeśli dodać `ALTER SEQUENCE ... RESTART`
- DROP DATABASE: bardzo wolne (~5s), ale całkowicie czyste

### Dane Testowe

**Seed (global-setup)**:
- 3 użytkowników w `auth.users`
- Mogą być dodane podstawowe `plan_types` jeśli nie są w migracjach

**Fixtures (w testach)**:
- Każdy test tworzy swoje category_types, categories, tasks, events
- Używa `generateUniqueName()` z timestamp dla unikalności

**Izolacja**:
- Każdy test zaczyna z czystymi tabelami biznesowymi
- Tylko użytkownicy są współdzieleni między testami

---

## ⚠️ Uwagi i Ograniczenia

### 1. Performance
- Pierwsze uruchomienie może trwać 30-60s (pull obrazów)
- Każdy test dodaje ~50-200ms na TRUNCATE
- Global setup dodaje ~2-3s (healthchecks + user creation)

### 2. Porty
- Porty 54010-54016 muszą być wolne
- Port 8081 dla backend E2E musi być wolny
- Konflikt z lokalnym Supabase CLI (porty 54000-54005)

### 3. Resources
- Docker musi mieć przydzielone minimum:
  - 4GB RAM
  - 2 CPU cores
- ~15 kontenerów będzie uruchomionych jednocześnie

### 4. Windows/WSL
- Paths w docker-compose muszą być w formacie Unix
- Volume mounts mogą być wolniejsze na Windows
- Użyj Docker Desktop z WSL2 backend dla lepszej wydajności

### 5. CI/CD
- W GitHub Actions trzeba będzie:
  - Zainstalować Docker Compose
  - Uruchomić `docker-compose up -d`
  - Poczekać na healthchecks
  - Uruchomić testy
  - Zalogować artefakty (logs, screenshots)

---

## ✅ Kryteria Akceptacji

### Must Have (MVP) - ✅ 100% COMPLETE
- [x] Docker Compose uruchamia wszystkie serwisy poprawnie (PostgreSQL + Backend) ✅
- [x] Backend łączy się z bazą E2E ✅
- [x] Wszystkie migracje aplikacyjne wykonują się automatycznie ✅
- [x] Auth schema i tabele utworzone (auth.users + funkcje helper) ✅
- [x] Global setup tworzy użytkowników testowych ✅
- [x] Truncate działa przed każdym testem ✅
- [x] Istniejące testy przechodzą ✅
- [x] NPM scripts działają (`e2e:docker:start/stop/clean/full`) ✅
- [x] Dokumentacja opisuje setup i usage ✅

### Should Have (Post-MVP)
- [ ] Migracja wszystkich testów do nowego setup'u
- [ ] CI/CD workflow w GitHub Actions
- [ ] Monitoring i alerty dla failed tests
- [ ] Performance optimization (caching, parallel tests)

### Could Have (Future)
- [ ] Multiple environments (E2E-staging, E2E-production-like)
- [ ] Test data builders dla complex scenarios
- [ ] Visual regression testing
- [ ] API contract testing

---

## 📚 Referencje

### Dokumentacja
- [Supabase Local Development](https://supabase.com/docs/guides/local-development)
- [Supabase Docker Setup](https://supabase.com/docs/guides/self-hosting/docker)
- [Playwright Global Setup](https://playwright.dev/docs/test-global-setup-teardown)
- [Docker Compose Healthcheck](https://docs.docker.com/compose/compose-file/compose-file-v3/#healthcheck)

### Przykłady
- [Supabase Docker Compose Template](https://github.com/supabase/supabase/blob/master/docker/docker-compose.yml)
- [Playwright Database Fixtures](https://playwright.dev/docs/test-fixtures)

---

## 📅 Timeline Implementacji

### Faza 1: Setup Infrastruktury
1. Utworzenie `docker-compose.e2e.yml`
2. Utworzenie `appsettings.E2E.json`
3. Test uruchomienia stacku
4. Debugging połączeń między serwisami

### Faza 2: Database Helpers
1. Implementacja `db-helper.ts` z TRUNCATE
2. Implementacja `database-fixture.ts`
3. Testy manualne czyszczenia bazy

### Faza 3: Global Setup
1. Implementacja `global-setup.ts`
2. Healthcheck logic
3. User creation przez Auth API

### Faza 4: Integracja 
1. Aktualizacja `playwright.config.ts`
2. Dodanie npm scripts
4. Testy E2E

### Faza 5: Dokumentacja 
1. README.md z instrukcjami
2. Troubleshooting guide
3. Update CLAUDE.md


---

## 🎬 Następne Kroki

**Status: ✅ WSZYSTKIE FAZY ZAKOŃCZONE (1-5) | MVP COMPLETE 🎉**

### ✅ Faza 1: Setup Infrastruktury (COMPLETED)

1. ✅ Utworzenie `docker-compose.e2e.yml` (uproszczona wersja: PostgreSQL + Backend)
2. ✅ Utworzenie `appsettings.E2E.json` dla środowiska E2E
3. ✅ Utworzenie `docker/init-supabase-e2e.sql` (auth schema + role + helper functions)
4. ✅ Utworzenie `docker/01-run-migrations.sql` (automatyczne wykonywanie migracji)
5. ✅ Zaktualizowanie `Program.cs` (włączenie Swagger dla E2E)
6. ✅ Przetestowanie uruchomienia stacku i migracji
7. ✅ Debugging połączeń (listen_addresses, line endings)

**Rezultat:**
- PostgreSQL E2E (port 54011) z automatycznymi migracjami ✅
- Backend API (port 8081) ze Swaggerem ✅
- Wszystkie tabele aplikacyjne utworzone (households, categories, tasks, events, etc.) ✅
- Funkcje auth.uid(), auth.role(), auth.email() dla RLS policies ✅

### ✅ Faza 2: Database Helpers (COMPLETED)

4. ✅ Implementować `frontend/e2e/helpers/db-helper.ts` (TRUNCATE helper)
5. ✅ Implementować `frontend/e2e/fixtures/database-fixture.ts` (Playwright fixture)
6. ✅ Testy manualne czyszczenia bazy

**Rezultat:**
- Utworzono `frontend/e2e/helpers/db-helper.ts` z funkcjami:
  - `truncateAllTables()` - czyszczenie wszystkich tabel biznesowych
  - `resetSequences()` - opcjonalny reset sekwencji
  - `isDatabaseAvailable()` - healthcheck
  - `getTableCounts()` - debugging helper
- Utworzono `frontend/e2e/fixtures/database-fixture.ts` - auto-fixture dla Playwright
- Testy manualne przeszły pomyślnie ✅
- **Ważne**: Użycie `127.0.0.1` zamiast `localhost` aby uniknąć problemów IPv6 na Windows

### ✅ Faza 3: Global Setup (COMPLETED)

7. ✅ Implementować `frontend/e2e/global-setup.ts` (healthchecks + user creation via SQL)
8. ✅ Utworzenie 3 użytkowników testowych bezpośrednio w auth.users

**Rezultat:**
- Utworzono `frontend/e2e/global-setup.ts` z funkcjami:
  - `waitForHealthcheck()` - retry logic dla healthchecków
  - `checkDatabaseHealth()` - sprawdzenie dostępności PostgreSQL
  - `checkBackendHealth()` - sprawdzenie dostępności Backend API
  - `createTestUsers()` - tworzenie 3 użytkowników testowych w auth.users
  - `globalSetup()` - główna funkcja uruchamiana przed wszystkimi testami
- Zaktualizowano `playwright.config.ts` z globalSetup
- Użytkownicy testowi:
  - admin@e2e.homely.com (role: admin)
  - member@e2e.homely.com (role: member)
  - dashboard@e2e.homely.com (role: dashboard)
- Idempotentne tworzenie użytkowników (sprawdza czy już istnieją)
- **Ważne**: Użycie `127.0.0.1` zamiast `localhost` dla backend API aby uniknąć problemów IPv6 na Windows

### ✅ Faza 4: Integracja (COMPLETED)

9. ✅ Aktualizować `frontend/playwright.config.ts` (global setup, base URLs)
10. ✅ Dodać npm scripts do `frontend/package.json` (e2e:docker:*)
11. ✅ Upewnić się, że testy E2E korzystają z nowego setupu

**Rezultat:**
- Dodano npm scripts do `package.json`:
  - **Workflow scripts:**
    - `e2e:full` - pełny workflow: start → testy → stop kontenerów
    - `e2e:full:clean` - pełny workflow z czyszczeniem volumes przed i po
  - **Docker management:**
    - `e2e:docker:start` - uruchomienie Docker Compose
    - `e2e:docker:stop` - zatrzymanie Docker Compose
    - `e2e:docker:clean` - zatrzymanie i usunięcie volumes
    - `e2e:docker:logs` - podgląd logów wszystkich serwisów
    - `e2e:docker:logs:backend` - podgląd logów backend
    - `e2e:docker:logs:postgres` - podgląd logów PostgreSQL
    - `e2e:docker:restart` - restart całego stacku
    - `e2e:docker:ps` - status kontenerów
- Przetestowano wszystkie npm scripts (działają poprawnie)
- Zaktualizowano `category-management.spec.ts` do używania database-fixture
- Skonfigurowano `playwright.config.ts`:
  - `workers: 1` - sekwencyjne wykonywanie testów
  - `fullyParallel: false` - zapobiega race conditions
  - Dodano komunikat w global-setup o izolacji bazy danych
- Wszystkie testy E2E (4/4) przechodzą z automatycznym czyszczeniem bazy

### ✅ Faza 5: Dokumentacja (COMPLETED)

12. ✅ Zaktualizować README z instrukcjami

**Rezultat:**
- Zaktualizowano `frontend/e2e/README.md` z kompleksową dokumentacją:
  - 🚀 Quick Start - jedna komenda do uruchomienia wszystkich testów
  - 📋 E2E Database Environment - architektura i korzyści
  - 🔧 Running Tests - wszystkie npm scripts z opisem
  - ⚙️ Configuration - ustawienia Playwright i database fixture
  - 📝 Writing New Tests - template z database-fixture
  - 🐛 Troubleshooting - Docker, database, i test execution issues
  - 📚 Resources - linki do dokumentacji i plików projektu
- Dodano sekcję o test users (3 użytkowników z hasłami)
- Zadokumentowano wszystkie npm scripts workflow
- Dodano troubleshooting dla Windows-specific issues (IPv6, line endings)
- Zaktualizowano przykłady kodu do używania database-fixture

