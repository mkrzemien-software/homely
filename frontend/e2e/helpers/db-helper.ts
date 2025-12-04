import { Client } from 'pg';

/**
 * Database helper configuration
 */
const DB_CONFIG = {
  // Use 127.0.0.1 instead of localhost to avoid IPv6 resolution issues on Windows
  host: process.env.E2E_DB_HOST || '127.0.0.1',
  port: parseInt(process.env.E2E_DB_PORT || '54011'),
  database: process.env.E2E_DB_NAME || 'postgres',
  user: process.env.E2E_DB_USER || 'postgres',
  password: process.env.E2E_DB_PASSWORD || 'postgres',
  // Disable SSL for local development (not needed for localhost connections)
  ssl: false,
  // Connection timeout in milliseconds
  connectionTimeoutMillis: 10000,
  // Query timeout in milliseconds
  query_timeout: 30000,
};

/**
 * Tables that should be truncated before each test
 * Order matters for foreign key constraints
 *
 * NOT included:
 * - plan_types: Reference data (plan definitions)
 * - user_profiles: Test users (created in global-setup)
 * - auth.users: Authentication users (managed separately)
 */
const TABLES_TO_TRUNCATE = [
  'tasks_history',
  'events',
  'tasks',
  'categories',
  'category_types',
  'plan_usage',
  'household_members',
  'households',
];

/**
 * Creates a database client connection
 */
async function createClient(): Promise<Client> {
  const client = new Client(DB_CONFIG);
  await client.connect();
  return client;
}

/**
 * Executes a function with retry logic
 * @param fn Function to execute
 * @param maxRetries Maximum number of retries
 * @param delayMs Delay between retries in milliseconds
 */
async function withRetry<T>(
  fn: () => Promise<T>,
  maxRetries: number = 3,
  delayMs: number = 1000
): Promise<T> {
  let lastError: Error | undefined;

  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error as Error;
      console.warn(`Attempt ${attempt}/${maxRetries} failed: ${lastError.message}`);

      if (attempt < maxRetries) {
        console.log(`Retrying in ${delayMs}ms...`);
        await new Promise(resolve => setTimeout(resolve, delayMs));
      }
    }
  }

  throw new Error(
    `Failed after ${maxRetries} attempts. Last error: ${lastError?.message}`
  );
}

/**
 * Truncates all business tables in the database except auth.users and plan_types
 * Uses CASCADE to handle foreign key constraints
 *
 * @throws Error if database connection or truncate operation fails
 */
export async function truncateAllTables(): Promise<void> {
  await withRetry(async () => {
    const client = await createClient();

    try {
      console.log('🧹 Starting database cleanup...');

      // Start transaction for atomic operation
      await client.query('BEGIN');

      // Truncate all business tables
      const truncateQuery = `
        TRUNCATE TABLE ${TABLES_TO_TRUNCATE.join(', ')} CASCADE
      `;

      console.log(`Truncating tables: ${TABLES_TO_TRUNCATE.join(', ')}`);
      await client.query(truncateQuery);

      // Commit transaction
      await client.query('COMMIT');

      console.log('✅ Database cleanup completed successfully');
    } catch (error) {
      // Rollback on error
      await client.query('ROLLBACK');
      console.error('❌ Database cleanup failed:', error);
      throw error;
    } finally {
      await client.end();
    }
  });
}

/**
 * Resets all sequences to 1
 * Optional helper for deterministic test data
 *
 * @throws Error if database connection or reset operation fails
 */
export async function resetSequences(): Promise<void> {
  await withRetry(async () => {
    const client = await createClient();

    try {
      console.log('🔄 Resetting sequences...');

      // Get all sequences in the public schema
      const sequencesQuery = `
        SELECT sequence_name
        FROM information_schema.sequences
        WHERE sequence_schema = 'public'
      `;

      const result = await client.query(sequencesQuery);
      const sequences = result.rows.map(row => row.sequence_name);

      if (sequences.length === 0) {
        console.log('No sequences found to reset');
        return;
      }

      // Reset each sequence to 1
      for (const sequence of sequences) {
        await client.query(`ALTER SEQUENCE ${sequence} RESTART WITH 1`);
        console.log(`  Reset sequence: ${sequence}`);
      }

      console.log('✅ Sequences reset completed successfully');
    } catch (error) {
      console.error('❌ Sequences reset failed:', error);
      throw error;
    } finally {
      await client.end();
    }
  });
}

/**
 * Verifies database connection is available
 * Useful for healthchecks in global setup
 *
 * @returns true if connection successful, false otherwise
 */
export async function isDatabaseAvailable(): Promise<boolean> {
  try {
    const client = await createClient();
    await client.query('SELECT 1');
    await client.end();
    return true;
  } catch (error) {
    console.error('Database is not available:', error);
    return false;
  }
}

/**
 * Gets the count of records in each business table
 * Useful for debugging test data
 *
 * @returns Object with table names as keys and record counts as values
 */
export async function getTableCounts(): Promise<Record<string, number>> {
  const client = await createClient();
  const counts: Record<string, number> = {};

  try {
    for (const table of TABLES_TO_TRUNCATE) {
      const result = await client.query(`SELECT COUNT(*) as count FROM ${table}`);
      counts[table] = parseInt(result.rows[0].count);
    }

    return counts;
  } catch (error) {
    console.error('Failed to get table counts:', error);
    throw error;
  } finally {
    await client.end();
  }
}
