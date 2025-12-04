import { test as base } from '@playwright/test';
import { truncateAllTables } from '../helpers/db-helper';

/**
 * Extended Playwright test with automatic database cleanup
 *
 * Usage in tests:
 * ```typescript
 * import { test, expect } from './fixtures/database-fixture';
 *
 * test('should create category', async ({ page, cleanDatabase }) => {
 *   // cleanDatabase automatically runs before this test
 *   // Database is clean and ready for test data
 * });
 * ```
 */
export const test = base.extend({
  /**
   * Automatically cleans the database before each test
   * This fixture runs automatically due to { auto: true }
   */
  cleanDatabase: [
    async ({}, use) => {
      // Setup: Clean database before test
      console.log('🔧 [Fixture] Cleaning database before test...');
      await truncateAllTables();

      // Run the test
      await use();

      // Teardown: Nothing needed - we clean before next test
      // This saves time as we don't need to clean twice
    },
    { auto: true },
  ],
});

// Re-export expect from Playwright
export { expect } from '@playwright/test';
