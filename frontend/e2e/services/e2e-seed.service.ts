import { Client } from 'pg';

/**
 * E2E Seed Service
 * Seeds reference data from migrations for E2E testing
 * Mirrors the data from: database/supabase/migrations/20251012144500_create_views_and_triggers.sql
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

/**
 * Creates a database client connection
 */
async function createClient(): Promise<Client> {
  const client = new Client(DB_CONFIG);
  await client.connect();
  return client;
}

/**
 * Seeds category types from migrations
 * Data source: 20251012144500_create_views_and_triggers.sql (lines 93-97)
 */
export async function seedCategoryTypes(): Promise<void> {
  const client = await createClient();

  try {
    console.log('🌱 Seeding category types...');

    const seedQuery = `
      INSERT INTO category_types (name, description, sort_order, is_active) VALUES
      ('Przeglądy techniczne', 'Przeglądy techniczne pojazdów i urządzeń', 1, true),
      ('Wywóz śmieci', 'Harmonogram wywozu śmieci', 2, true),
      ('Wizyty medyczne', 'Wizyty zdrowotne członków gospodarstwa', 3, true)
      ON CONFLICT (name) DO NOTHING
    `;

    const result = await client.query(seedQuery);
    console.log(`  ✅ Seeded ${result.rowCount || 3} category types`);
  } catch (error) {
    console.error('❌ Failed to seed category types:', error);
    throw error;
  } finally {
    await client.end();
  }
}

/**
 * Seeds categories from migrations
 * Data source: 20251012144500_create_views_and_triggers.sql (lines 101-112)
 */
export async function seedCategories(): Promise<void> {
  const client = await createClient();

  try {
    console.log('🌱 Seeding categories...');

    const seedQuery = `
      INSERT INTO categories (category_type_id, name, description, sort_order, is_active) VALUES
      -- Przeglądy techniczne
      ((SELECT id FROM category_types WHERE name = 'Przeglądy techniczne'), 'Przegląd samochodu', 'Obowiązkowy przegląd techniczny pojazdu', 1, true),
      ((SELECT id FROM category_types WHERE name = 'Przeglądy techniczne'), 'Przegląd kotła', 'Przegląd kotła grzewczego', 2, true),
      -- Wywóz śmieci
      ((SELECT id FROM category_types WHERE name = 'Wywóz śmieci'), 'Śmieci zmieszane', 'Wywóz śmieci zmieszanych', 1, true),
      ((SELECT id FROM category_types WHERE name = 'Wywóz śmieci'), 'Odpady segregowane', 'Wywóz odpadów segregowanych', 2, true),
      -- Wizyty medyczne
      ((SELECT id FROM category_types WHERE name = 'Wizyty medyczne'), 'Lekarz rodzinny', 'Wizyty u lekarza pierwszego kontaktu', 1, true),
      ((SELECT id FROM category_types WHERE name = 'Wizyty medyczne'), 'Dentysta', 'Wizyty dentystyczne', 2, true),
      ((SELECT id FROM category_types WHERE name = 'Wizyty medyczne'), 'Badania kontrolne', 'Regularne badania profilaktyczne', 3, true)
      ON CONFLICT (category_type_id, name) DO NOTHING
    `;

    const result = await client.query(seedQuery);
    console.log(`  ✅ Seeded ${result.rowCount || 7} categories`);
  } catch (error) {
    console.error('❌ Failed to seed categories:', error);
    throw error;
  } finally {
    await client.end();
  }
}

/**
 * Seeds all reference data in the correct order
 * Call this function after truncating tables to restore reference data
 */
export async function seedAllReferenceData(): Promise<void> {
  console.log('🌱 Seeding all reference data from migrations...');

  try {
    // Order matters: category_types must be seeded before categories
    await seedCategoryTypes();
    await seedCategories();

    console.log('✅ All reference data seeded successfully\n');
  } catch (error) {
    console.error('❌ Failed to seed reference data:', error);
    throw error;
  }
}

/**
 * Verifies that reference data exists
 * Useful for debugging seed issues
 */
export async function verifyReferenceData(): Promise<boolean> {
  const client = await createClient();

  try {
    // Check category types
    const categoryTypesResult = await client.query(
      'SELECT COUNT(*) as count FROM category_types WHERE deleted_at IS NULL'
    );
    const categoryTypesCount = parseInt(categoryTypesResult.rows[0].count);

    // Check categories
    const categoriesResult = await client.query(
      'SELECT COUNT(*) as count FROM categories WHERE deleted_at IS NULL'
    );
    const categoriesCount = parseInt(categoriesResult.rows[0].count);

    console.log(`📊 Reference data counts:`);
    console.log(`  - Category Types: ${categoryTypesCount} (expected: 3)`);
    console.log(`  - Categories: ${categoriesCount} (expected: 7)`);

    const isValid = categoryTypesCount === 3 && categoriesCount === 7;

    if (isValid) {
      console.log('✅ Reference data verification passed');
    } else {
      console.error('❌ Reference data verification failed');
    }

    return isValid;
  } catch (error) {
    console.error('❌ Failed to verify reference data:', error);
    return false;
  } finally {
    await client.end();
  }
}
