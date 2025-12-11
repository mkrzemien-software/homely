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
 * Seeds category types for a specific household
 * Note: Categories are now multi-tenant (household-specific)
 * @param householdId - The household UUID to seed categories for
 */
export async function seedCategoryTypes(householdId: string): Promise<void> {
  const client = await createClient();

  try {
    console.log('🌱 Seeding category types for household:', householdId);

    const seedQuery = `
      INSERT INTO category_types (household_id, name, description, sort_order, is_active) VALUES
      ($1, 'Przeglądy techniczne', 'Przeglądy techniczne pojazdów i urządzeń', 1, true),
      ($1, 'Wywóz śmieci', 'Harmonogram wywozu śmieci', 2, true),
      ($1, 'Wizyty medyczne', 'Wizyty zdrowotne członków gospodarstwa', 3, true)
      ON CONFLICT DO NOTHING
    `;

    const result = await client.query(seedQuery, [householdId]);
    console.log(`  ✅ Seeded ${result.rowCount || 3} category types`);
  } catch (error) {
    console.error('❌ Failed to seed category types:', error);
    throw error;
  } finally {
    await client.end();
  }
}

/**
 * Seeds categories for a specific household
 * Note: Categories are now multi-tenant (household-specific)
 * @param householdId - The household UUID to seed categories for
 */
export async function seedCategories(householdId: string): Promise<void> {
  const client = await createClient();

  try {
    console.log('🌱 Seeding categories for household:', householdId);

    const seedQuery = `
      INSERT INTO categories (household_id, category_type_id, name, description, sort_order, is_active) VALUES
      -- Przeglądy techniczne
      ($1, (SELECT id FROM category_types WHERE household_id = $1 AND name = 'Przeglądy techniczne'), 'Przegląd samochodu', 'Obowiązkowy przegląd techniczny pojazdu', 1, true),
      ($1, (SELECT id FROM category_types WHERE household_id = $1 AND name = 'Przeglądy techniczne'), 'Przegląd kotła', 'Przegląd kotła grzewczego', 2, true),
      -- Wywóz śmieci
      ($1, (SELECT id FROM category_types WHERE household_id = $1 AND name = 'Wywóz śmieci'), 'Śmieci zmieszane', 'Wywóz śmieci zmieszanych', 1, true),
      ($1, (SELECT id FROM category_types WHERE household_id = $1 AND name = 'Wywóz śmieci'), 'Odpady segregowane', 'Wywóz odpadów segregowanych', 2, true),
      -- Wizyty medyczne
      ($1, (SELECT id FROM category_types WHERE household_id = $1 AND name = 'Wizyty medyczne'), 'Lekarz rodzinny', 'Wizyty u lekarza pierwszego kontaktu', 1, true),
      ($1, (SELECT id FROM category_types WHERE household_id = $1 AND name = 'Wizyty medyczne'), 'Dentysta', 'Wizyty dentystyczne', 2, true),
      ($1, (SELECT id FROM category_types WHERE household_id = $1 AND name = 'Wizyty medyczne'), 'Badania kontrolne', 'Regularne badania profilaktyczne', 3, true)
      ON CONFLICT DO NOTHING
    `;

    const result = await client.query(seedQuery, [householdId]);
    console.log(`  ✅ Seeded ${result.rowCount || 7} categories`);
  } catch (error) {
    console.error('❌ Failed to seed categories:', error);
    throw error;
  } finally {
    await client.end();
  }
}

/**
 * Seeds all reference data for a specific household in the correct order
 * Call this function after creating a household to seed reference data
 * @param householdId - The household UUID to seed data for
 */
export async function seedAllReferenceData(householdId: string): Promise<void> {
  console.log('🌱 Seeding all reference data for household:', householdId);

  try {
    // Order matters: category_types must be seeded before categories
    await seedCategoryTypes(householdId);
    await seedCategories(householdId);

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
