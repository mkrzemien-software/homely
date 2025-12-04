import { chromium, FullConfig } from '@playwright/test';
import bcrypt from 'bcryptjs';
import { Pool } from 'pg';
import { isDatabaseAvailable } from './helpers/db-helper';

// Test users configuration
const TEST_USERS = [
  { email: 'admin@e2e.homely.com', password: 'Test123!@#', role: 'admin' },
  { email: 'member@e2e.homely.com', password: 'Test123!@#', role: 'member' },
  { email: 'dashboard@e2e.homely.com', password: 'Test123!@#', role: 'dashboard' }
];

// Database connection configuration
const DB_CONFIG = {
  host: '127.0.0.1',
  port: 54011,
  database: 'postgres',
  user: 'postgres',
  password: 'postgres',
};

// API configuration
// Note: Use 127.0.0.1 instead of localhost to avoid IPv6 issues on Windows
const BACKEND_API_URL = 'http://127.0.0.1:8081';
const HEALTHCHECK_TIMEOUT = 60000; // 60 seconds
const HEALTHCHECK_INTERVAL = 2000; // 2 seconds

/**
 * Wait for a service to be healthy with retry logic
 */
async function waitForHealthcheck(
  checkFn: () => Promise<boolean>,
  serviceName: string,
  timeout: number = HEALTHCHECK_TIMEOUT
): Promise<void> {
  const startTime = Date.now();
  let attempts = 0;

  while (Date.now() - startTime < timeout) {
    attempts++;
    try {
      const isHealthy = await checkFn();
      if (isHealthy) {
        console.log(`✅ ${serviceName} is healthy (attempt ${attempts})`);
        return;
      }
    } catch (error) {
      // Continue trying
    }

    console.log(`⏳ Waiting for ${serviceName}... (attempt ${attempts})`);
    await new Promise(resolve => setTimeout(resolve, HEALTHCHECK_INTERVAL));
  }

  throw new Error(`❌ ${serviceName} failed to become healthy after ${timeout}ms`);
}

/**
 * Check if backend API is healthy
 */
async function checkBackendHealth(): Promise<boolean> {
  try {
    const response = await fetch(`${BACKEND_API_URL}/health`, {
      method: 'GET',
      signal: AbortSignal.timeout(5000),
    });
    return response.ok;
  } catch (error) {
    return false;
  }
}

/**
 * Check if database is available
 */
async function checkDatabaseHealth(): Promise<boolean> {
  try {
    return await isDatabaseAvailable();
  } catch (error) {
    return false;
  }
}

/**
 * Create test users in auth.users table
 * Uses bcrypt to hash passwords (Supabase-compatible)
 */
async function createTestUsers(): Promise<void> {
  const pool = new Pool(DB_CONFIG);

  try {
    console.log('\n📝 Creating test users...');

    for (const user of TEST_USERS) {
      try {
        // Check if user already exists
        const existingUser = await pool.query(
          'SELECT id FROM auth.users WHERE email = $1',
          [user.email]
        );

        if (existingUser.rows.length > 0) {
          console.log(`  ℹ️  User ${user.email} already exists (skipping)`);
          continue;
        }

        // Hash password with bcrypt (10 rounds, Supabase default)
        const hashedPassword = await bcrypt.hash(user.password, 10);

        // Insert user into auth.users
        // Note: Simplified auth.users table for E2E testing (see init-supabase-e2e.sql)
        const insertQuery = `
          INSERT INTO auth.users (
            email,
            encrypted_password,
            email_confirmed_at,
            created_at,
            updated_at,
            raw_user_meta_data,
            raw_app_meta_data
          ) VALUES (
            $1,
            $2,
            NOW(),
            NOW(),
            NOW(),
            jsonb_build_object('role', $3::text),
            '{}'::jsonb
          )
          RETURNING id, email;
        `;

        const result = await pool.query(insertQuery, [user.email, hashedPassword, user.role]);
        console.log(`  ✅ Created user: ${result.rows[0].email} (id: ${result.rows[0].id})`);
      } catch (error) {
        console.error(`  ❌ Failed to create user ${user.email}:`, error);
        throw error;
      }
    }

    console.log('✅ Test users setup completed\n');
  } finally {
    await pool.end();
  }
}

/**
 * Global setup function - runs once before all tests
 */
async function globalSetup(config: FullConfig) {
  console.log('\n🚀 E2E Global Setup Started\n');
  console.log('Configuration:');
  console.log(`  Database: ${DB_CONFIG.host}:${DB_CONFIG.port}`);
  console.log(`  Backend API: ${BACKEND_API_URL}`);
  console.log(`  Healthcheck timeout: ${HEALTHCHECK_TIMEOUT}ms`);
  console.log(`  Workers: ${config.workers} (sequential execution to maintain database isolation)`);
  console.log();

  try {
    // Step 1: Check database availability
    console.log('1️⃣ Checking database availability...');
    await waitForHealthcheck(
      checkDatabaseHealth,
      'PostgreSQL Database',
      HEALTHCHECK_TIMEOUT
    );

    // Step 2: Check backend API availability
    console.log('\n2️⃣ Checking backend API availability...');
    await waitForHealthcheck(
      checkBackendHealth,
      'Backend API',
      HEALTHCHECK_TIMEOUT
    );

    // Step 3: Create test users
    console.log('\n3️⃣ Setting up test users...');
    await createTestUsers();

    console.log('✅ E2E Global Setup Completed Successfully\n');
  } catch (error) {
    console.error('\n❌ E2E Global Setup Failed:', error);
    console.error('\nTroubleshooting:');
    console.error('  1. Ensure Docker Compose is running: npm run e2e:docker:start');
    console.error('  2. Check Docker logs: npm run e2e:docker:logs');
    console.error('  3. Verify services are healthy: docker-compose -f ../docker-compose.e2e.yml ps\n');
    throw error;
  }
}

export default globalSetup;
