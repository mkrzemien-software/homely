# E2E Tests with Playwright

This directory contains End-to-End (E2E) tests for the Homely application using Playwright.

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

1. Install dependencies:
   ```bash
   npm install
   ```

2. Install Playwright browsers (if not already done):
   ```bash
   npx playwright install chromium
   ```

### Test Commands

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
- **Base URL**: http://localhost:4200
- **Timeout**: 30 seconds per test
- **Retries**: 2 retries in CI, 0 locally
- **Screenshots**: On failure only
- **Videos**: Retained on failure
- **Traces**: On first retry

## Writing New Tests

1. Create page objects in `page-objects/` for new pages
2. Add test data to `fixtures/test-data.ts`
3. Create test file `*.spec.ts` following the AAA pattern
4. Use `data-testid` selectors for all interactions
5. Add appropriate assertions with Playwright's `expect`

### Example Test Template

```typescript
import { test, expect } from '@playwright/test';
import { MyPage } from './page-objects/my.page';

test.describe('Feature Name', () => {
  let myPage: MyPage;

  test.beforeEach(async ({ page }) => {
    myPage = new MyPage(page);
    await myPage.navigateToPage();
  });

  test('should do something', async ({ page }) => {
    // Arrange
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

### Tests fail locally but pass in CI

- Check Node.js and Playwright versions match CI
- Clear browser cache: `npx playwright install --force`

### Flaky tests

- Add explicit waits: `await page.waitForLoadState('networkidle')`
- Use retry logic in playwright.config.ts
- Check for race conditions in async operations

### Browser not found

- Run: `npx playwright install chromium`

## Resources

- [Playwright Documentation](https://playwright.dev)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Test Plan](./.ai/test-plan.md)
- [Testing Setup Tasks](./.ai/testing-setup-tasks.md)
