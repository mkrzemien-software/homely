# E2E Tests with Playwright

This directory contains End-to-End (E2E) tests for the Homely application using Playwright with a dedicated Docker-based test database.

## 🚀 Quick Start

**Run complete E2E test suite:**
```bash
cd frontend
npm run e2e:full
```

This single command:
- ✅ Starts Docker containers (PostgreSQL + Backend API)
- ✅ Waits for services to be healthy
- ✅ Creates test users automatically
- ✅ Runs all E2E tests with clean database
- ✅ Stops and removes containers

## 📋 E2E Database Environment

### Architecture

Tests run against a **dedicated isolated environment** with:
- **PostgreSQL 17** (Supabase-compatible) on port `54011`
- **Backend API** (.NET 9) on port `8081`
- **Automatic database cleanup** before each test
- **Test users** created in global setup

### Benefits

✅ **Isolated** - Separate from development database
✅ **Deterministic** - Clean state for every test
✅ **Automated** - Docker Compose manages everything
✅ **Fast** - Sequential execution prevents race conditions

### Test Users

Three users are automatically created:

| Email | Password | Role |
|-------|----------|------|
| `admin@e2e.homely.com` | `Test123!@#` | admin |
| `member@e2e.homely.com` | `Test123!@#` | member |
| `dashboard@e2e.homely.com` | `Test123!@#` | dashboard |

## Structure

```
e2e/
├── page-objects/       # Page Object Model classes
│   ├── base.page.ts       # Base page with common functionality
│   ├── login.page.ts      # Login page object
│   └── category.page.ts   # Category management page object
├── fixtures/           # Test data and fixtures
│   └── test-data.ts       # Centralized test data
└── *.spec.ts           # Test specification files
```

## Page Object Model

We follow the Page Object Model (POM) pattern for maintainable and reusable tests:

- **Base Page**: Common functionality across all pages
- **Login Page**: Authentication interactions
- **Category Page**: Category and subcategory management

## Test Data Selectors

All tests use `data-testid` attributes for resilient test-oriented selectors:

```typescript
// Good - using data-testid
await page.getByTestId('login-email');

// Avoid - CSS selectors are fragile
await page.locator('.email-input');
```

## Running Tests

### Prerequisites

1. **Docker Desktop** must be installed and running
2. Install Node.js dependencies:
   ```bash
   npm install
   ```
3. Install Playwright browsers (if not already done):
   ```bash
   npx playwright install chromium
   ```

### Test Commands

#### Complete Workflow (Recommended)

```bash
# Full workflow: Start → Test → Stop containers
npm run e2e:full

# Full workflow with deep cleanup (removes volumes)
npm run e2e:full:clean
```

#### Manual Docker Management

```bash
# Start E2E environment
npm run e2e:docker:start

# Run tests only (environment must be running)
npm run e2e

# Stop environment
npm run e2e:docker:stop

# Clean environment (removes volumes)
npm run e2e:docker:clean

# Restart environment
npm run e2e:docker:restart

# View container status
npm run e2e:docker:ps

# View all logs
npm run e2e:docker:logs

# View backend logs only
npm run e2e:docker:logs:backend

# View database logs only
npm run e2e:docker:logs:postgres
```

#### Test Execution Modes

```bash
# Run all E2E tests (headless)
npm run e2e

# Run tests with browser visible
npm run e2e:headed

# Run tests in UI mode (interactive)
npm run e2e:ui

# Debug tests step-by-step
npm run e2e:debug

# View test report
npm run e2e:report
```

### Running Specific Tests

```bash
# Run a specific test file
npx playwright test category-management.spec.ts

# Run tests matching a pattern
npx playwright test --grep "category"
```

## Test Structure

All tests follow the **Arrange-Act-Assert** (AAA) pattern:

```typescript
test('should perform action', async ({ page }) => {
  // ARRANGE - Set up test data and initial state
  const testData = { ... };

  // ACT - Execute the action being tested
  await somePage.performAction(testData);

  // ASSERT - Verify the expected outcome
  expect(result).toBe(expected);
});
```

## Configuration

Test configuration is in `playwright.config.ts`:

- **Browser**: Chromium only (Desktop Chrome)
- **Frontend URL**: http://localhost:4200 (with E2E environment config)
- **Backend API**: http://127.0.0.1:8081 (configured in `environment.e2e.ts`)
- **Database**: 127.0.0.1:54011
- **Workers**: 1 (sequential execution for database isolation)
- **Global Setup**: Automatic environment initialization
- **Database Cleanup**: Before each test via fixture
- **Timeout**: 30 seconds per test
- **Retries**: 2 retries in CI, 0 locally
- **Screenshots**: On failure only
- **Videos**: Retained on failure
- **Traces**: On first retry

### Environment Configuration

The E2E tests use a dedicated Angular environment file (`src/environments/environment.e2e.ts`):
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://127.0.0.1:8081/api',
  environmentName: 'e2e'
};
```

This ensures that the frontend communicates with the correct backend API running in Docker on port 8081.

### Database Fixture

All tests automatically use the `database-fixture` which:
- ✅ Truncates all business tables before each test
- ✅ Preserves test users (created in global setup)
- ✅ Ensures deterministic test environment

Import the fixture in your tests:
```typescript
import { test, expect } from './fixtures/database-fixture';
```

### Global Setup

The `global-setup.ts` runs once before all tests:
1. Checks database availability
2. Checks backend API health
3. Creates test users in `auth.users`
4. Reports configuration (workers, timeouts, URLs)

## Writing New Tests

1. Create page objects in `page-objects/` for new pages
2. Add test data to `fixtures/test-data.ts`
3. Create test file `*.spec.ts` following the AAA pattern
4. Use `data-testid` selectors for all interactions
5. Add appropriate assertions with Playwright's `expect`

### Example Test Template

```typescript
// IMPORTANT: Import from database-fixture, not @playwright/test
import { test, expect } from './fixtures/database-fixture';
import { MyPage } from './page-objects/my.page';

test.describe('Feature Name', () => {
  let myPage: MyPage;

  test.beforeEach(async ({ page }) => {
    // Database is automatically cleaned before this runs
    myPage = new MyPage(page);
    await myPage.navigateToPage();
  });

  test('should do something', async ({ page }) => {
    // Arrange - database is clean, use test users
    const testData = { ... };

    // Act
    await myPage.performAction(testData);

    // Assert
    const result = await myPage.getResult();
    expect(result).toBe(expected);
  });
});
```

## Debugging

### Visual Debugging

Use Playwright's UI mode for step-by-step debugging:

```bash
npm run e2e:ui
```

### Trace Viewer

After a test failure, view the trace:

```bash
npx playwright show-trace trace.zip
```

### Screenshots

Failed tests automatically capture screenshots in `test-results/`

## Best Practices

1. **Use Page Objects** - Keep tests clean by abstracting page interactions
2. **Use data-testid** - Prefer `getByTestId()` over CSS selectors
3. **Wait for Elements** - Playwright auto-waits, but use explicit waits when needed
4. **Isolate Tests** - Each test should be independent and idempotent
5. **Use Fixtures** - Centralize test data in `fixtures/test-data.ts`
6. **Follow AAA** - Structure tests with Arrange-Act-Assert pattern
7. **Test Cleanup** - Use `afterEach` for proper teardown
8. **Screenshots** - Use visual comparison for UI regression testing

## CI/CD Integration

The E2E tests are configured to run in GitHub Actions:

- Tests run on every pull request
- Uses headless mode
- Retries failed tests 2 times
- Uploads artifacts (screenshots, videos, traces) on failure

## Troubleshooting

### Docker Environment Issues

#### Containers won't start
```bash
# Check Docker is running
docker ps

# View detailed logs
npm run e2e:docker:logs

# Full cleanup and restart
npm run e2e:docker:clean
npm run e2e:docker:start
```

#### Port already in use
If ports `54011` or `8081` are occupied:
```bash
# Stop conflicting services
docker-compose -f ../docker-compose.e2e.yml down

# Or use different ports by editing docker-compose.e2e.yml
```

#### Database connection fails
```bash
# Verify PostgreSQL is healthy
npm run e2e:docker:ps

# Check database logs
npm run e2e:docker:logs:postgres

# Manually verify database
docker exec homely-postgres-e2e psql -U postgres -d postgres -c "SELECT 1;"
```

#### Backend API not responding
```bash
# Check backend status
npm run e2e:docker:ps

# View backend logs
npm run e2e:docker:logs:backend

# Manually test health endpoint
curl http://127.0.0.1:8081/health
```

### Database Issues

#### Tests see data from previous runs
- Database cleanup happens automatically via fixture
- If issues persist:
  ```bash
  npm run e2e:full:clean  # Deep cleanup with volumes
  ```

#### Test users not found
- Users are created in global-setup.ts
- Check logs for user creation errors:
  ```bash
  npm run e2e:docker:logs:postgres | grep "admin@e2e.homely.com"
  ```

### Test Execution Issues

#### Tests fail locally but pass in CI
- Check Node.js and Playwright versions match CI
- Clear browser cache: `npx playwright install --force`
- Ensure Docker environment is clean

#### Flaky tests
- Tests run sequentially (1 worker) to prevent race conditions
- Add explicit waits: `await page.waitForLoadState('networkidle')`
- Check for timing issues in test code

#### Browser not found
```bash
npx playwright install chromium
```

### Windows-Specific Issues

#### IPv6 Connection Problems
The setup uses `127.0.0.1` instead of `localhost` to avoid IPv6 issues on Windows. If you see connection errors, verify the configuration uses `127.0.0.1`.

#### Line Ending Issues
Ensure `.sh` scripts use LF line endings (not CRLF):
```bash
git config core.autocrlf false
```

## Resources

### Documentation
- [Playwright Documentation](https://playwright.dev)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [E2E Database Setup Plan](./E2E_DATABASE_SETUP_PLAN.md) - Complete architecture documentation

### Project Files
- `global-setup.ts` - Environment initialization and user creation
- `database-fixture.ts` - Automatic database cleanup fixture
- `db-helper.ts` - Database utility functions
- `playwright.config.ts` - Test configuration
- `../docker-compose.e2e.yml` - Docker environment definition
