import { Client } from 'pg';

/**
 * E2E Authentication and Test Data Service
 * Manages test household creation and user assignments for E2E tests
 */

// Database connection configuration
const DB_CONFIG = {
  host: process.env['E2E_DB_HOST'] || '127.0.0.1',
  port: parseInt(process.env['E2E_DB_PORT'] || '54011'),
  database: process.env['E2E_DB_NAME'] || 'postgres',
  user: process.env['E2E_DB_USER'] || 'postgres',
  password: process.env['E2E_DB_PASSWORD'] || 'postgres',
  ssl: false,
  connectionTimeoutMillis: 10000,
  query_timeout: 30000,
};

// Test users configuration (must match global-setup.ts)
export const TEST_USERS = {
  admin: { email: 'admin@e2e.homely.com', role: 'admin' },
  member: { email: 'member@e2e.homely.com', role: 'member' },
  dashboard: { email: 'dashboard@e2e.homely.com', role: 'dashboard' },
};

// Test household configuration
export const TEST_HOUSEHOLD = {
  name: 'E2E Test Household',
  address: 'Test Address 123, E2E City',
  planTypeId: 1, // Free plan
};

/**
 * Creates a database client connection
 */
async function createClient(): Promise<Client> {
  const client = new Client(DB_CONFIG);
  await client.connect();
  return client;
}

/**
 * Creates test household and assigns all test users to it
 * This should be called after database cleanup to set up fresh test data
 *
 * @returns The created household ID
 */
export async function setupTestHouseholdWithUsers(): Promise<string> {
  const client = await createClient();

  try {
    console.log('🏠 Setting up test household with users...');

    // Start transaction
    await client.query('BEGIN');

    // Step 1: Create household
    const createHouseholdQuery = `
      INSERT INTO households (name, address, plan_type_id, subscription_status)
      VALUES ($1, $2, $3, 'free')
      RETURNING id
    `;

    const householdResult = await client.query(createHouseholdQuery, [
      TEST_HOUSEHOLD.name,
      TEST_HOUSEHOLD.address,
      TEST_HOUSEHOLD.planTypeId,
    ]);

    const householdId = householdResult.rows[0].id;
    console.log(`  ✅ Created household: ${TEST_HOUSEHOLD.name} (id: ${householdId})`);

    // Step 2: Get user IDs from auth.users
    const users = Object.values(TEST_USERS);
    const userIdMap: Record<string, string> = {};

    for (const user of users) {
      const getUserQuery = 'SELECT id FROM auth.users WHERE email = $1';
      const userResult = await client.query(getUserQuery, [user.email]);

      if (userResult.rows.length === 0) {
        throw new Error(`User ${user.email} not found in auth.users. Run global-setup first.`);
      }

      userIdMap[user.email] = userResult.rows[0].id;
    }

    // Step 3: Assign all users to household with their roles
    for (const user of users) {
      const userId = userIdMap[user.email];

      const assignUserQuery = `
        INSERT INTO household_members (household_id, user_id, role, joined_at)
        VALUES ($1, $2, $3, NOW())
        RETURNING id
      `;

      await client.query(assignUserQuery, [householdId, userId, user.role]);
      console.log(`  ✅ Assigned ${user.email} to household as ${user.role}`);
    }

    // Step 4: Initialize plan usage for the household
    const initPlanUsageQuery = `
      INSERT INTO plan_usage (household_id, usage_type, current_value, max_value, usage_date)
      VALUES
        ($1, 'household_members', 3, 3, CURRENT_DATE),
        ($1, 'tasks', 0, 5, CURRENT_DATE)
      ON CONFLICT (household_id, usage_type, usage_date)
      DO UPDATE SET
        current_value = EXCLUDED.current_value,
        max_value = EXCLUDED.max_value,
        updated_at = NOW()
    `;

    await client.query(initPlanUsageQuery, [householdId]);
    console.log('  ✅ Initialized plan usage tracking');

    // Commit transaction
    await client.query('COMMIT');

    console.log(`✅ Test household setup completed (household_id: ${householdId})\n`);
    return householdId;
  } catch (error) {
    // Rollback on error
    await client.query('ROLLBACK');
    console.error('❌ Failed to setup test household:', error);
    throw error;
  } finally {
    await client.end();
  }
}

/**
 * Gets the test household ID (if it exists)
 * @returns The household ID or null if not found
 */
export async function getTestHouseholdId(): Promise<string | null> {
  const client = await createClient();

  try {
    const query = 'SELECT id FROM households WHERE name = $1 AND deleted_at IS NULL';
    const result = await client.query(query, [TEST_HOUSEHOLD.name]);

    if (result.rows.length === 0) {
      return null;
    }

    return result.rows[0].id;
  } finally {
    await client.end();
  }
}

/**
 * Verifies that all test users are assigned to the test household
 * Useful for debugging test setup issues
 *
 * @returns true if all users are assigned, false otherwise
 */
export async function verifyTestUsersAssigned(): Promise<boolean> {
  const client = await createClient();

  try {
    const householdId = await getTestHouseholdId();

    if (!householdId) {
      console.error('❌ Test household not found');
      return false;
    }

    const users = Object.values(TEST_USERS);

    for (const user of users) {
      const query = `
        SELECT hm.id, hm.role
        FROM household_members hm
        JOIN auth.users u ON hm.user_id = u.id
        WHERE u.email = $1 AND hm.household_id = $2 AND hm.deleted_at IS NULL
      `;

      const result = await client.query(query, [user.email, householdId]);

      if (result.rows.length === 0) {
        console.error(`❌ User ${user.email} not assigned to household`);
        return false;
      }

      const assignedRole = result.rows[0].role;
      if (assignedRole !== user.role) {
        console.error(
          `❌ User ${user.email} has incorrect role. Expected: ${user.role}, Got: ${assignedRole}`
        );
        return false;
      }
    }

    console.log('✅ All test users are correctly assigned to household');
    return true;
  } finally {
    await client.end();
  }
}
