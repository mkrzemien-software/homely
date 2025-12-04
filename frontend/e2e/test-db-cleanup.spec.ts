import { test, expect } from './fixtures/database-fixture';
import { getTableCounts } from './helpers/db-helper';

/**
 * Test suite to verify database cleanup functionality
 * This test validates that the database-fixture correctly cleans tables before each test
 */
test.describe('Database Cleanup', () => {
  test('should clean all tables before test', async () => {
    // The database should be clean at the start of this test
    // because the cleanDatabase fixture runs automatically

    const counts = await getTableCounts();

    console.log('Table counts after cleanup:', counts);

    // Verify all business tables are empty
    expect(counts.households).toBe(0);
    expect(counts.household_members).toBe(0);
    expect(counts.categories).toBe(0);
    expect(counts.category_types).toBe(0);
    expect(counts.tasks).toBe(0);
    expect(counts.events).toBe(0);
    expect(counts.tasks_history).toBe(0);
    expect(counts.plan_usage).toBe(0);
  });

  test('should clean tables between tests', async () => {
    // This test verifies that even if previous test left data,
    // it would be cleaned before this test runs

    const counts = await getTableCounts();

    // All tables should still be empty in the second test
    expect(counts.households).toBe(0);
    expect(counts.household_members).toBe(0);
    expect(counts.tasks).toBe(0);
  });
});
