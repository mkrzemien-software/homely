import { test as base, ConsoleMessage } from '@playwright/test';
import { truncateAllTables } from '../helpers/db-helper';
import { setupTestHouseholdWithUsers } from '../services/e2e-auth.service';
import * as fs from 'fs';
import * as path from 'path';

/**
 * Extended Playwright test with automatic database cleanup and console log capture
 *
 * Usage in tests:
 * ```typescript
 * import { test, expect } from './fixtures/database-fixture';
 *
 * test('should create category', async ({ page, cleanDatabase }) => {
 *   // cleanDatabase automatically runs before this test
 *   // Database is clean and ready for test data
 *   // Browser console logs are automatically captured
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

      // Create test household with all test users assigned
      console.log('🔧 [Fixture] Setting up test household and users...');
      await setupTestHouseholdWithUsers();

      // Run the test
      await use();

      // Teardown: Nothing needed - we clean before next test
      // This saves time as we don't need to clean twice
    },
    { auto: true },
  ],

  /**
   * Automatically captures browser console logs and saves them to a file
   * This fixture runs automatically due to { auto: true }
   */
  page: async ({ page }, use, testInfo) => {
    const consoleLogs: Array<{
      timestamp: string;
      type: string;
      text: string;
      location?: string;
    }> = [];

    // Setup: Listen to console events
    page.on('console', (msg: ConsoleMessage) => {
      const logEntry = {
        timestamp: new Date().toISOString(),
        type: msg.type(),
        text: msg.text(),
        location: msg.location() ? `${msg.location().url}:${msg.location().lineNumber}` : undefined,
      };
      consoleLogs.push(logEntry);

      // Also log to Node console for real-time debugging
      const prefix = {
        error: '❌',
        warning: '⚠️',
        info: 'ℹ️',
        log: '📝',
      }[msg.type()] || '📄';

      console.log(`${prefix} [Browser Console] ${msg.type()}: ${msg.text()}`);
    });

    // Listen to page errors
    page.on('pageerror', (error) => {
      const logEntry = {
        timestamp: new Date().toISOString(),
        type: 'pageerror',
        text: error.message,
        location: error.stack,
      };
      consoleLogs.push(logEntry);
      console.log(`🔥 [Page Error] ${error.message}`);
    });

    // Run the test
    await use(page);

    // Teardown: Save console logs to file
    if (consoleLogs.length > 0) {
      const logsDir = path.join(testInfo.project.outputDir, 'console-logs');
      fs.mkdirSync(logsDir, { recursive: true });

      const sanitizedTitle = testInfo.title.replace(/[^a-z0-9]/gi, '_').toLowerCase();
      const logFileName = `${sanitizedTitle}-${Date.now()}.json`;
      const logFilePath = path.join(logsDir, logFileName);

      const logData = {
        test: {
          title: testInfo.title,
          file: testInfo.file,
          status: testInfo.status,
          duration: testInfo.duration,
        },
        logs: consoleLogs,
      };

      fs.writeFileSync(logFilePath, JSON.stringify(logData, null, 2));
      console.log(`📋 [Console Logs] Saved to: ${logFilePath}`);

      // Attach to test report
      await testInfo.attach('console-logs', {
        path: logFilePath,
        contentType: 'application/json',
      });
    }
  },
});

// Re-export expect from Playwright
export { expect } from '@playwright/test';
