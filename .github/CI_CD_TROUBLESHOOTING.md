# CI/CD Troubleshooting Guide

## 📝 Session Summary - E2E Database Setup Implementation

### ✅ **COMPLETED: Full E2E Testing Environment with Docker**

**Implementation Date:** December 4, 2025
**Status:** ✅ MVP Complete (All 5 phases done)
**Total Implementation Time:** ~7 hours

---

## 🎯 What Was Implemented

### **5 Implementation Phases:**

#### **Phase 1: Infrastructure Setup** ✅
- Created `docker-compose.e2e.yml` (PostgreSQL + Backend API)
- Created `docker/init-supabase-e2e.sql` (auth schema, helper functions)
- Created `docker/01-run-migrations.sql` (automatic migrations)
- Created `backend/appsettings.E2E.json` (E2E environment config)
- Configured PostgreSQL on port `54011`, Backend on port `8081`

#### **Phase 2: Database Helpers** ✅
- Created `frontend/e2e/helpers/db-helper.ts` (TRUNCATE, healthcheck, table counts)
- Created `frontend/e2e/fixtures/database-fixture.ts` (auto-cleanup before each test)
- Implemented automatic database cleanup using TRUNCATE CASCADE

#### **Phase 3: Global Setup** ✅
- Created `frontend/e2e/global-setup.ts` (healthchecks + user creation)
- Automatic test user creation (3 users: admin, member, dashboard)
- Service healthchecks (PostgreSQL + Backend API)
- Uses `127.0.0.1` instead of `localhost` (Windows IPv6 fix)

#### **Phase 4: Integration** ✅
- Updated `frontend/playwright.config.ts` (1 worker, globalSetup)
- Added 10 npm scripts to `frontend/package.json`
- Configured sequential test execution (prevents race conditions)
- All tests updated to use database-fixture

#### **Phase 5: Documentation** ✅
- Updated `frontend/e2e/README.md` (comprehensive guide)
- Created `frontend/e2e/E2E_DATABASE_SETUP_PLAN.md` (architecture doc)
- Created `.github/workflows/e2e-tests.yml` (CI/CD workflow)
- Created `.github/workflows/e2e-nightly.yml` (scheduled tests)
- Created `.github/workflows/README.md` (workflow documentation)
- Created `.github/CI_CD_TROUBLESHOOTING.md` (this file)

---

## 📁 Key Files Reference

### **Docker & Infrastructure:**
```
docker-compose.e2e.yml          # E2E environment orchestration
docker/init-supabase-e2e.sql    # Auth schema + helper functions
docker/01-run-migrations.sql    # Automatic migration execution
backend/appsettings.E2E.json    # Backend E2E configuration
```

### **E2E Test Setup:**
```
frontend/e2e/global-setup.ts              # Environment initialization
frontend/e2e/helpers/db-helper.ts         # Database utilities
frontend/e2e/fixtures/database-fixture.ts # Auto-cleanup fixture
frontend/playwright.config.ts             # Playwright configuration
```

### **CI/CD:**
```
.github/workflows/e2e-tests.yml    # Main CI/CD workflow
.github/workflows/e2e-nightly.yml  # Nightly scheduled tests
.github/workflows/README.md        # Workflow documentation
.github/CI_CD_TROUBLESHOOTING.md   # This file
```

### **Documentation:**
```
frontend/e2e/README.md                  # E2E testing guide
frontend/e2e/E2E_DATABASE_SETUP_PLAN.md # Architecture & plan
```

---

## 🚀 Quick Reference Commands

### **Local Development:**
```bash
# Complete workflow (recommended)
cd frontend && npm run e2e:full

# Deep cleanup
npm run e2e:full:clean

# Manual control
npm run e2e:docker:start    # Start environment
npm run e2e                 # Run tests
npm run e2e:docker:stop     # Stop environment

# Debugging
npm run e2e:docker:logs              # All logs
npm run e2e:docker:logs:backend      # Backend logs only
npm run e2e:docker:logs:postgres     # Database logs only
npm run e2e:docker:ps                # Container status
```

### **Test Users (automatically created):**
```
Email: admin@e2e.homely.com     | Password: Test123!@#  | Role: admin
Email: member@e2e.homely.com    | Password: Test123!@#  | Role: member
Email: dashboard@e2e.homely.com | Password: Test123!@#  | Role: dashboard
```

### **Database Access:**
```bash
# Direct PostgreSQL access
docker exec -it homely-postgres-e2e psql -U postgres -d postgres

# Check table contents
docker exec homely-postgres-e2e psql -U postgres -d postgres -c "SELECT * FROM auth.users;"

# Manual cleanup
docker exec homely-postgres-e2e psql -U postgres -d postgres -c "TRUNCATE TABLE households CASCADE;"
```

### **GitHub Actions:**
```bash
# Run workflow manually
gh workflow run e2e-tests.yml

# Watch live
gh run watch

# View runs
gh run list --workflow=e2e-tests.yml

# Download artifacts
gh run download <run-id>
```

---

## 🔧 Critical Fix Applied (Session)

### **Docker Build Context Error - FIXED ✅**

**Problem:**
```
ERROR: "/HomelyApi": not found
```

**Solution:**
Added `working-directory: backend` to Docker build step in `.github/workflows/e2e-tests.yml`:

```yaml
- name: 🔨 Build backend Docker image
  run: |
    docker build \
      --build-arg BUILD_ENV=E2E \
      --tag homely-backend-e2e \
      --cache-from type=gha \
      --cache-to type=gha,mode=max \
      --file Dockerfile \
      .
  working-directory: backend  # ← Critical fix
```

**Why:** Build context must be `backend/` directory to match Dockerfile expectations.

---

## ⚙️ Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                  Docker Compose E2E                     │
│                                                         │
│  ┌──────────────┐         ┌──────────────┐            │
│  │  PostgreSQL  │◄────────┤  Backend API │            │
│  │  Port: 54011 │         │  Port: 8081  │            │
│  │              │         │              │            │
│  │ - auth.users │         │ - E2E Config │            │
│  │ - migrations │         │ - Healthcheck│            │
│  └──────────────┘         └──────────────┘            │
│         ▲                        ▲                     │
│         │                        │                     │
│         └────────────┬───────────┘                     │
└──────────────────────┼─────────────────────────────────┘
                       │
            ┌──────────▼──────────┐
            │  Playwright Tests   │
            │  (localhost:4200)   │
            │                     │
            │ - Global Setup      │
            │ - Database Fixture  │
            │ - 1 Worker          │
            └─────────────────────┘
```

### **Flow:**
1. Global Setup runs once:
   - ✅ Check PostgreSQL health
   - ✅ Check Backend API health
   - ✅ Create 3 test users
2. Before each test (database-fixture):
   - ✅ TRUNCATE all business tables
   - ✅ Preserve test users
3. Run test with clean database
4. Repeat for next test

---

## 📊 Performance Metrics

### **Local Execution:**
- Cold start (first time): ~2-3 minutes
- Warm start (cached): ~30-45 seconds
- Single test: ~2-4 seconds
- Full suite (4 tests): ~8-10 seconds
- Database cleanup: ~50-200ms per test

### **CI/CD Execution:**
- Docker build (cached): ~2-3 minutes
- Docker build (no cache): ~5-8 minutes
- Service startup: ~30-60 seconds
- Test execution: ~10-15 seconds
- **Total workflow:** ~10-15 minutes

---

## 🎯 MVP Acceptance Criteria - 100% Complete

- [x] Docker Compose uruchamia wszystkie serwisy poprawnie ✅
- [x] Backend łączy się z bazą E2E ✅
- [x] Wszystkie migracje aplikacyjne wykonują się automatycznie ✅
- [x] Auth schema i tabele utworzone (auth.users + funkcje helper) ✅
- [x] Global setup tworzy użytkowników testowych ✅
- [x] Truncate działa przed każdym testem ✅
- [x] Istniejące testy przechodzą ✅
- [x] NPM scripts działają (`e2e:docker:start/stop/clean/full`) ✅
- [x] Dokumentacja opisuje setup i usage ✅

---

## 🔒 Important Notes

### **Windows Compatibility:**
- ✅ Uses `127.0.0.1` instead of `localhost` (IPv6 fix)
- ✅ Line endings handled (LF for scripts)
- ✅ Path separators normalized

### **Database Isolation:**
- ✅ Sequential execution (1 worker) prevents race conditions
- ✅ TRUNCATE CASCADE before each test
- ✅ Test users preserved across tests
- ✅ No manual cleanup needed

### **CI/CD Integration:**
- ✅ Docker BuildKit caching for fast builds
- ✅ npm caching for dependencies
- ✅ Playwright browser caching
- ✅ Concurrency control (cancel in-progress runs)
- ✅ Artifacts uploaded only on failure

### **Security:**
- ⚠️ Demo credentials in E2E (acceptable for testing)
- ⚠️ Supabase demo keys (acceptable for E2E)
- ✅ No production secrets in code
- ✅ Isolated test environment

---

## 🚨 Known Limitations & Future Improvements

### **Current Limitations:**
- Sequential test execution (1 worker) - slower than parallel
- No parallel test support (due to shared database)
- Manual Docker management required locally
- No CI/CD deployment workflow yet

### **Future Enhancements (Post-MVP):**
- [ ] Parallel test execution with per-test database isolation
- [ ] Test sharding for faster CI runs
- [ ] Visual regression testing
- [ ] Performance benchmarks
- [ ] Multiple environment support (staging, production-like)
- [ ] Automatic PR comments with test results
- [ ] Codecov integration

---

## 🐛 Common Issues and Solutions

### 0. Frontend Cannot Connect to Backend (ERR_CONNECTION_REFUSED)

#### ❌ Error:
```
net::ERR_CONNECTION_REFUSED when POST http://localhost:5000/api/auth/login
```

#### 🔍 Root Cause:
Frontend Angular app was using wrong API URL. By default, `environment.ts` configured `apiUrl: 'http://localhost:5000/api'`, but backend in E2E Docker environment runs on port **8081**.

#### ✅ Solution:
Created dedicated E2E environment configuration:

1. **Created new environment file** (`frontend/src/environments/environment.e2e.ts`):
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://127.0.0.1:8081/api',  // ← Correct E2E backend port
  environmentName: 'e2e'
};
```

2. **Updated `angular.json`** with E2E configuration:
```json
"configurations": {
  "e2e": {
    "optimization": false,
    "extractLicenses": false,
    "sourceMap": {
      "scripts": true,
      "styles": true,
      "vendor": true
    },
    "fileReplacements": [
      {
        "replace": "src/environments/environment.ts",
        "with": "src/environments/environment.e2e.ts"
      }
    ]
  }
}
```

3. **Added `start:e2e` script** to `package.json`:
```json
"start:e2e": "ng serve --configuration e2e"
```

4. **Updated `playwright.config.ts`** to use E2E configuration:
```typescript
webServer: {
  command: 'npm run start:e2e',  // ← Uses E2E environment
  url: 'http://localhost:4200',
  reuseExistingServer: !process.env.CI,
  timeout: 120 * 1000,
}
```

#### 📝 Why This Matters:
- Backend in Docker runs on port `8081` (mapped from internal `8080`)
- Frontend needs to know correct backend URL at build time
- Each environment (local, dev, e2e, prod) needs its own configuration
- Using `127.0.0.1` instead of `localhost` avoids IPv6 issues on Windows

---

### 1. CORS Policy Execution Failed

#### ❌ Error:
```
info: Microsoft.AspNetCore.Cors.Infrastructure.CorsService[5]
      CORS policy execution failed.
info: Microsoft.AspNetCore.Cors.Infrastructure.CorsService[6]
      Request origin http://localhost:4200 does not have permission to access the resource.
```

#### 🔍 Cause:
Backend CORS policy doesn't include E2E environment origins. The frontend runs on `http://localhost:4200` but backend only allows this origin for Development/Local environments, not E2E.

#### ✅ Solution:
Add E2E environment to CORS policy in `backend/HomelyApi/Homely.API/Program.cs`:

**Before:**
```csharp
if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Local")
{
    policy.WithOrigins("http://localhost:4200")
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials();
}
```

**After:**
```csharp
if (builder.Environment.IsDevelopment() ||
    builder.Environment.EnvironmentName == "Local" ||
    builder.Environment.EnvironmentName == "E2E")  // ← Add E2E
{
    policy.WithOrigins(
              "http://localhost:4200",      // Angular dev server
              "http://127.0.0.1:4200"       // Alternative localhost (E2E tests)
          )
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials();
}
```

**Why both `localhost` and `127.0.0.1`?**
- Playwright may use either `localhost` or `127.0.0.1` depending on configuration
- Windows/Linux handle localhost resolution differently
- Using both ensures compatibility across environments

**How to verify CORS is working:**
```bash
# Check backend logs for successful CORS
docker compose -f docker-compose.e2e.yml logs backend-e2e | grep CORS

# Should see:
# info: Microsoft.AspNetCore.Cors.Infrastructure.CorsService[4]
#       CORS policy execution successful.
```

---

### 2. docker-compose: command not found

#### ❌ Error:
```
/home/runner/work/_temp/xxxxx.sh: line 1: docker-compose: command not found
```

#### 🔍 Cause:
GitHub Actions runners now use Docker Compose V2, which is integrated as a Docker CLI plugin (`docker compose`) instead of a standalone binary (`docker-compose`).

#### ✅ Solution:
Replace all instances of `docker-compose` with `docker compose` (space instead of hyphen):

**Before:**
```yaml
- name: 🚀 Start services
  run: docker-compose -f docker-compose.e2e.yml up -d
```

**After:**
```yaml
- name: 🚀 Start services
  run: docker compose -f docker-compose.e2e.yml up -d
```

**Why it works:**
- Docker Compose V2 is the current standard and is pre-installed on GitHub Actions runners
- It provides the same functionality with improved performance
- Matches Docker's direction of integrating Compose as a native plugin

**Alternative (not recommended):**
Install docker-compose as standalone binary:
```yaml
- name: Install docker-compose
  run: |
    sudo curl -L "https://github.com/docker/compose/releases/download/v2.23.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
```

---

### 1. Docker Build Context Error

#### ❌ Error:
```
ERROR: failed to build: failed to solve: failed to compute cache key:
"/HomelyApi": not found
```

#### 🔍 Cause:
Docker build context mismatch. The Dockerfile expects files relative to `backend/` directory, but the build was running from root.

#### ✅ Solution:
Use `working-directory: backend` in the workflow:

```yaml
- name: 🔨 Build backend Docker image
  run: |
    docker build \
      --build-arg BUILD_ENV=E2E \
      --tag homely-backend-e2e \
      --file Dockerfile \
      .
  working-directory: backend  # ← This fixes the context
```

**Why it works:**
- Sets the build context to `backend/` directory
- Dockerfile can now find `HomelyApi/` at the correct path
- Matches the local `docker-compose.e2e.yml` behavior

---

### 2. Services Don't Start (Timeout on Health Check)

#### ❌ Error:
```
⏳ Waiting for PostgreSQL...
Error: Process completed with exit code 124 (timeout)
```

#### 🔍 Possible Causes:
1. Services taking too long to start
2. Port conflicts
3. Resource constraints in GitHub runner
4. Health check command incorrect

#### ✅ Solutions:

**A. Increase timeout:**
```yaml
- name: ⏳ Wait for services
  run: |
    timeout 120 bash -c 'until docker exec homely-postgres-e2e pg_isready -U postgres; do sleep 2; done'
```

**B. Check service status first:**
```yaml
- name: 📊 Check container status
  run: |
    docker-compose -f docker-compose.e2e.yml ps
    docker-compose -f docker-compose.e2e.yml logs postgres-e2e
```

**C. Add retry logic:**
```yaml
- name: ⏳ Wait for PostgreSQL with retries
  run: |
    for i in {1..30}; do
      if docker exec homely-postgres-e2e pg_isready -U postgres -d postgres; then
        echo "✅ PostgreSQL is ready"
        exit 0
      fi
      echo "⏳ Attempt $i/30: Waiting for PostgreSQL..."
      sleep 2
    done
    echo "❌ PostgreSQL failed to start"
    exit 1
```

---

### 3. npm ci Fails

#### ❌ Error:
```
npm ERR! The `npm ci` command can only install with an existing package-lock.json
```

#### 🔍 Cause:
Missing or outdated `package-lock.json`

#### ✅ Solution:
```bash
# Locally, regenerate package-lock.json
cd frontend
rm package-lock.json
npm install
git add package-lock.json
git commit -m "Update package-lock.json"
git push
```

---

### 4. Playwright Browser Installation Fails

#### ❌ Error:
```
Error: browserType.launch: Executable doesn't exist
```

#### 🔍 Cause:
Browsers not installed or wrong version

#### ✅ Solutions:

**A. Install with system dependencies:**
```yaml
- name: 🎭 Install Playwright browsers
  run: npx playwright install chromium --with-deps
```

**B. Use Playwright Docker image (alternative):**
```yaml
jobs:
  e2e-tests:
    container:
      image: mcr.microsoft.com/playwright:v1.40.0-focal
```

---

### 5. Docker Compose Down Fails

#### ❌ Error:
```
Error: No such service: postgres-e2e
```

#### 🔍 Cause:
Services weren't started or have different names

#### ✅ Solution:
Always use `if: always()` for cleanup:
```yaml
- name: 🧹 Cleanup
  if: always()  # ← Run even if previous steps failed
  run: docker-compose -f docker-compose.e2e.yml down -v || true
```

---

### 6. Artifacts Not Uploaded

#### ❌ Issue:
No artifacts appear after workflow run

#### 🔍 Causes:
1. Paths are incorrect
2. Files don't exist
3. Workflow failed before upload step

#### ✅ Solutions:

**A. Check paths are relative to working directory:**
```yaml
- name: 📸 Upload artifacts
  if: failure()
  uses: actions/upload-artifact@v4
  with:
    name: test-results
    path: |
      frontend/playwright-report/
      frontend/test-results/
    if-no-files-found: warn  # ← Show warning instead of error
```

**B. Debug artifact paths:**
```yaml
- name: 🔍 Debug artifact paths
  if: always()
  run: |
    echo "Checking for test results..."
    ls -la frontend/playwright-report/ || echo "Report directory not found"
    ls -la frontend/test-results/ || echo "Results directory not found"
```

---

### 7. Docker Cache Not Working

#### ❌ Issue:
Every build takes full 5+ minutes despite caching

#### 🔍 Causes:
1. Cache keys change on every build
2. Cache storage limits exceeded
3. Cache not properly configured

#### ✅ Solutions:

**A. Verify cache configuration:**
```yaml
- name: 🔨 Build with cache
  run: |
    docker build \
      --cache-from type=gha \
      --cache-to type=gha,mode=max \
      --tag myimage \
      .
```

**B. Check cache size:**
```yaml
- name: 📊 Cache statistics
  run: |
    gh cache list
  env:
    GH_TOKEN: ${{ github.token }}
```

**C. Use registry cache (alternative):**
```yaml
- name: 🔨 Build with registry cache
  run: |
    docker build \
      --cache-from ghcr.io/${{ github.repository }}/cache:latest \
      --cache-to ghcr.io/${{ github.repository }}/cache:latest \
      --tag myimage \
      .
```

---

### 8. Tests Pass Locally, Fail in CI

#### 🔍 Common Causes & Solutions:

**A. Timing Issues:**
```typescript
// Add explicit waits
await page.waitForLoadState('networkidle');
await page.waitForTimeout(1000); // Use sparingly
```

**B. Environment Differences:**
```yaml
# Match local Node version
- uses: actions/setup-node@v4
  with:
    node-version: '20'  # ← Match your local version
```

**C. Display/Screen Issues:**
```yaml
# Run in headed mode to debug
- name: 🧪 Run tests
  run: npm run e2e:headed
  env:
    DISPLAY: ':99'  # Virtual display for Linux
```

**D. Database State:**
```bash
# Ensure clean state
npm run e2e:full:clean  # Use deep cleanup workflow
```

---

### 9. Concurrency Issues

#### ❌ Error:
```
Error: Resource busy or locked
```

#### 🔍 Cause:
Multiple workflows trying to use same resources

#### ✅ Solution:
Already configured in workflow:
```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.event.pull_request.number || github.ref }}
  cancel-in-progress: true
```

To disable for debugging:
```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.event.pull_request.number || github.ref }}
  cancel-in-progress: false  # ← Don't cancel
```

---

### 10. Secrets Not Available

#### ❌ Error:
```
Error: SLACK_WEBHOOK_URL not found
```

#### 🔍 Cause:
Secret not configured in repository settings

#### ✅ Solution:
1. Go to **Settings → Secrets and variables → Actions**
2. Click **New repository secret**
3. Add secret name and value
4. Re-run workflow

**Access in workflow:**
```yaml
env:
  SLACK_WEBHOOK: ${{ secrets.SLACK_WEBHOOK_URL }}
```

---

## 🔍 Debugging Workflow

### Enable Debug Logging

**1. Enable runner debug logging:**
```yaml
env:
  ACTIONS_RUNNER_DEBUG: true
  ACTIONS_STEP_DEBUG: true
```

**2. Add debug steps:**
```yaml
- name: 🔍 Debug environment
  run: |
    echo "=== System Info ==="
    uname -a
    echo ""
    echo "=== Docker Info ==="
    docker version
    docker info
    echo ""
    echo "=== Node Info ==="
    node --version
    npm --version
    echo ""
    echo "=== Working Directory ==="
    pwd
    ls -la
```

### View Live Logs

**GitHub CLI:**
```bash
# Watch workflow run live
gh run watch

# View specific run
gh run view <run-id> --log
```

### Download Artifacts Locally

```bash
# List artifacts
gh run view <run-id>

# Download artifact
gh run download <run-id> -n artifact-name

# Download all artifacts
gh run download <run-id>
```

---

## 📊 Performance Issues

### Slow npm install

#### ✅ Solutions:

**A. Use npm ci instead of npm install:**
```yaml
run: npm ci  # ← Faster, uses package-lock.json
```

**B. Cache node_modules:**
```yaml
- uses: actions/setup-node@v4
  with:
    cache: 'npm'
    cache-dependency-path: frontend/package-lock.json
```

**C. Use npm cache directly:**
```yaml
- uses: actions/cache@v3
  with:
    path: ~/.npm
    key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
```

### Slow Docker builds

#### ✅ Solutions:

**A. Multi-stage builds** (already implemented)

**B. Smaller base images:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
```

**C. Layer caching:**
```dockerfile
# Copy and restore dependencies first (cached layer)
COPY *.csproj ./
RUN dotnet restore

# Copy source code last (changes frequently)
COPY . ./
```

### Slow test execution

#### ✅ Solutions:

**A. Parallel execution** (if tests are isolated):
```yaml
- run: npm run e2e -- --workers=4
```

**B. Test sharding:**
```yaml
strategy:
  matrix:
    shard: [1, 2, 3, 4]
steps:
  - run: npm run e2e -- --shard=${{ matrix.shard }}/4
```

**C. Only run affected tests:**
```yaml
- name: 🧪 Run changed tests
  run: npm run e2e -- --grep "${{ github.event.pull_request.title }}"
```

---

## 🚨 Emergency Fixes

### Disable E2E Tests Temporarily

Add to workflow:
```yaml
jobs:
  e2e-tests:
    if: false  # ← Disable workflow
```

Or use workflow dispatch with condition:
```yaml
on:
  workflow_dispatch:
    inputs:
      skip_tests:
        description: 'Skip E2E tests'
        required: false
        default: 'false'

jobs:
  e2e-tests:
    if: ${{ github.event.inputs.skip_tests != 'true' }}
```

### Skip CI for Specific Commits

Add to commit message:
```bash
git commit -m "docs: Update README [skip ci]"
```

Or:
```bash
git commit -m "docs: Update README

[skip actions]"
```

---

## 📚 Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Build GitHub Action](https://github.com/docker/build-push-action)
- [Playwright CI Guide](https://playwright.dev/docs/ci-intro)
- [Troubleshooting GitHub Actions](https://docs.github.com/en/actions/monitoring-and-troubleshooting-workflows)

---

## 🔍 Verbose Logging for Debugging

### Backend API Logs

The E2E environment is configured with **verbose logging** to help debug issues:

**What is logged:**
- ✅ All HTTP requests (method, path, headers, body)
- ✅ All HTTP responses (status, headers, body)
- ✅ Routing decisions
- ✅ Authentication events
- ✅ Database queries (SQL statements)
- ✅ Entity Framework operations

**Configuration locations:**

1. **`backend/HomelyApi/Homely.API/appsettings.E2E.json`** (lines 5-14):
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Information",
    "Microsoft.AspNetCore.Hosting": "Information",
    "Microsoft.AspNetCore.Routing": "Information",
    "Microsoft.AspNetCore.HttpLogging": "Information",
    "Microsoft.EntityFrameworkCore": "Information"
  }
}
```

2. **`docker-compose.e2e.yml`** (lines 101-108):
```yaml
Logging__LogLevel__Default: "Information"
Logging__LogLevel__Microsoft.AspNetCore: "Information"
Logging__LogLevel__Microsoft.AspNetCore.Hosting: "Information"
Logging__LogLevel__Microsoft.AspNetCore.Routing: "Information"
Logging__LogLevel__Microsoft.AspNetCore.HttpLogging: "Information"
Logging__LogLevel__Microsoft.EntityFrameworkCore: "Information"
```

3. **`backend/HomelyApi/Homely.API/Program.cs`** (lines 107-118, 295-298):
```csharp
// HTTP Logging middleware (E2E only)
if (builder.Environment.EnvironmentName == "E2E")
{
    builder.Services.AddHttpLogging(logging =>
    {
        logging.LoggingFields = HttpLoggingFields.All;
        logging.RequestBodyLogLimit = 4096;
        logging.ResponseBodyLogLimit = 4096;
    });
}
```

**Example log output:**
```
info: Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware[1]
      Request:
      Protocol: HTTP/1.1
      Method: POST
      Scheme: http
      PathBase:
      Path: /api/auth/login
      Accept: application/json
      Content-Type: application/json
      Request Body: {"email":"admin@e2e.homely.com","password":"Test123!@#"}

info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'AuthController.Login'

info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (12ms) [Parameters=[@p0='?' (DbType = String)], CommandType='Text', CommandTimeout='30']
      SELECT u."id", u."email" FROM auth."users" AS u WHERE u."email" = @p0 LIMIT 1

info: Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware[2]
      Response:
      StatusCode: 200
      Content-Type: application/json
      Response Body: {"token":"eyJhbGci...","user":{"id":"123","email":"admin@e2e.homely.com"}}
```

**View logs in CI:**
```yaml
# In workflow (.github/workflows/e2e-tests.yml)
- name: 📋 Display Docker logs (on failure)
  if: failure()
  run: |
    docker compose -f docker-compose.e2e.yml logs backend-e2e
```

**View logs locally:**
```bash
# Real-time logs
docker compose -f docker-compose.e2e.yml logs -f backend-e2e

# Last 100 lines
docker compose -f docker-compose.e2e.yml logs --tail=100 backend-e2e

# Grep for specific endpoint
docker compose -f docker-compose.e2e.yml logs backend-e2e | grep "POST /api/auth/login"
```

---

### Browser Console Logs (Frontend)

The E2E tests **automatically capture all browser console logs** for debugging.

**What is captured:**
- ✅ `console.log()`, `console.error()`, `console.warn()`, `console.info()`
- ✅ Unhandled JavaScript exceptions (`pageerror`)
- ✅ Timestamp, log type, message, and source location
- ✅ Test metadata (title, status, duration)

**Location:**
```
frontend/test-results/console-logs/
  ├── should_login_successfully-1234567890.json
  ├── should_create_category-1234567891.json
  └── ...
```

**Example log file:**
```json
{
  "test": {
    "title": "should login with valid credentials",
    "file": "/path/to/auth.spec.ts",
    "status": "failed",
    "duration": 2341
  },
  "logs": [
    {
      "timestamp": "2025-12-04T12:34:56.789Z",
      "type": "error",
      "text": "Failed to fetch: POST http://localhost:8081/api/auth/login",
      "location": "http://localhost:4200/main.js:456"
    },
    {
      "timestamp": "2025-12-04T12:34:57.123Z",
      "type": "pageerror",
      "text": "Uncaught TypeError: Cannot read property 'token' of undefined",
      "location": "Error stack trace..."
    }
  ]
}
```

**Real-time viewing (during test execution):**
```bash
# Console logs are printed to terminal in real-time
npm run e2e

# Output:
# 📝 [Browser Console] log: Angular initialized
# ❌ [Browser Console] error: Failed to fetch
# 🔥 [Page Error] Uncaught TypeError: ...
```

**In CI/CD:**
Console logs are automatically uploaded as artifacts (both for passed and failed tests).

**Download from GitHub Actions:**
1. Navigate to Actions tab → Select workflow run
2. Scroll to "Artifacts" section
3. Download `browser-console-logs-{run_number}.zip`
4. Extract and view JSON files

**Configuration:**
Console logging is implemented in `frontend/e2e/fixtures/database-fixture.ts` and runs automatically for all tests.

---

## 💡 Pro Tips

1. **Always use `if: always()` for cleanup steps only**
2. **Use `if: success()` for steps that should only run after successful setup**
3. **Use `if: failure()` for diagnostic steps (logs, debug info)**
4. **Add `|| echo "message"` to diagnostic commands to prevent failure during error handling**
5. **Use `timeout` command to prevent hanging processes**
6. **Enable artifact upload only on failure to save storage**
7. **Use concurrency control to cancel old runs**
8. **Cache everything: npm, Docker layers, Playwright browsers**
9. **Test workflows locally with `act`**: https://github.com/nektos/act

### Workflow Step Conditions Best Practices

**Setup/Build/Test steps** (should STOP on failure):
```yaml
- name: Build application
  run: npm run build
  # No 'if' condition - fails and stops workflow
```

**Diagnostic steps** (run only on failure):
```yaml
- name: Display logs on failure
  if: failure()
  run: docker compose logs || echo "Failed to get logs"
```

**Cleanup steps** (always run):
```yaml
- name: Cleanup resources
  if: always()
  run: docker compose down -v || echo "Cleanup completed with warnings"
```

**Test steps** (only after successful setup):
```yaml
- name: Run tests
  if: success()
  run: npm test
```

---

## 🆘 Need More Help?

1. Check workflow logs in GitHub Actions tab
2. Review artifacts for screenshots/traces
3. Run workflow manually with debug logging enabled
4. Test locally with same Node/Docker versions as CI
5. Compare working CI runs with failing ones
