-- Migration: Add household_id to categories and category_types for household isolation
-- Description: Adds household_id column to both tables, updates RLS policies, and migrates existing data
-- Author: Homely Development Team
-- Date: 2025-12-11

-- ============================================================================
-- STEP 1: Add household_id columns (nullable initially for data migration)
-- ============================================================================

-- Add household_id to category_types
ALTER TABLE category_types
ADD COLUMN household_id uuid REFERENCES households(id) ON DELETE CASCADE;

-- Add household_id to categories
ALTER TABLE categories
ADD COLUMN household_id uuid REFERENCES households(id) ON DELETE CASCADE;

COMMENT ON COLUMN category_types.household_id IS 'Household that owns this category type - enables multi-tenant isolation';
COMMENT ON COLUMN categories.household_id IS 'Household that owns this category - enables multi-tenant isolation';

-- ============================================================================
-- STEP 2: Migrate existing data to first household
-- ============================================================================

DO $$
DECLARE
    first_household_id uuid;
BEGIN
    -- Get the first household ID (by created_at)
    SELECT id INTO first_household_id
    FROM households
    WHERE deleted_at IS NULL
    ORDER BY created_at ASC
    LIMIT 1;

    -- Only proceed if we found a household
    IF first_household_id IS NOT NULL THEN
        -- Assign all existing category_types to first household
        UPDATE category_types
        SET household_id = first_household_id
        WHERE household_id IS NULL;

        -- Assign all existing categories to first household
        UPDATE categories
        SET household_id = first_household_id
        WHERE household_id IS NULL;

        RAISE NOTICE 'Migrated existing categories to household: %', first_household_id;
    ELSE
        -- No households exist - delete orphaned seed data
        -- (categories should be created per-household going forward)
        RAISE NOTICE 'No households found - removing orphaned seed data';
        
        DELETE FROM categories WHERE household_id IS NULL;
        DELETE FROM category_types WHERE household_id IS NULL;
    END IF;
END $$;

-- ============================================================================
-- STEP 3: Make household_id NOT NULL
-- ============================================================================

ALTER TABLE category_types
ALTER COLUMN household_id SET NOT NULL;

ALTER TABLE categories
ALTER COLUMN household_id SET NOT NULL;

-- ============================================================================
-- STEP 4: Add indexes for household filtering
-- ============================================================================

-- Index for category_types household filtering
CREATE INDEX idx_category_types_household ON category_types(household_id)
WHERE deleted_at IS NULL;

-- Index for categories household filtering
CREATE INDEX idx_categories_household ON categories(household_id)
WHERE deleted_at IS NULL;

COMMENT ON INDEX idx_category_types_household IS 'Optimizes household-specific category type queries with RLS';
COMMENT ON INDEX idx_categories_household IS 'Optimizes household-specific category queries with RLS';

-- ============================================================================
-- STEP 5: Update unique constraints
-- ============================================================================

-- Drop old unique constraint for category_types name
ALTER TABLE category_types DROP CONSTRAINT IF EXISTS category_types_name_key;

-- Add new unique constraint: household + name
CREATE UNIQUE INDEX idx_category_types_household_name_unique
ON category_types(household_id, name)
WHERE deleted_at IS NULL;

COMMENT ON INDEX idx_category_types_household_name_unique IS 'Ensures unique category type names within each household';

-- Drop old unique constraint for categories
ALTER TABLE categories DROP CONSTRAINT IF EXISTS categories_unique_name;

-- Add new unique constraint: household + category_type + name
CREATE UNIQUE INDEX idx_categories_household_type_name_unique
ON categories(household_id, category_type_id, name)
WHERE deleted_at IS NULL;

COMMENT ON INDEX idx_categories_household_type_name_unique IS 'Ensures unique category names within category type and household';

-- ============================================================================
-- STEP 6: Update RLS policies for category_types
-- ============================================================================

-- Drop old public policies
DROP POLICY IF EXISTS "anyone_can_select_active_category_types" ON category_types;

-- SELECT: Household members can view their household's category types
CREATE POLICY "household_members_can_select_category_types" ON category_types
    FOR SELECT TO authenticated
    USING (
        household_id IN (
            SELECT household_id FROM household_members
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

-- INSERT: Household members can create category types
CREATE POLICY "household_members_can_insert_category_types" ON category_types
    FOR INSERT TO authenticated
    WITH CHECK (
        household_id IN (
            SELECT household_id FROM household_members
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

-- UPDATE: Household members can update their household's category types
CREATE POLICY "household_members_can_update_category_types" ON category_types
    FOR UPDATE TO authenticated
    USING (
        household_id IN (
            SELECT household_id FROM household_members
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    )
    WITH CHECK (
        household_id IN (
            SELECT household_id FROM household_members
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

-- DELETE: Household members can delete their household's category types
CREATE POLICY "household_members_can_delete_category_types" ON category_types
    FOR DELETE TO authenticated
    USING (
        household_id IN (
            SELECT household_id FROM household_members
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

COMMENT ON POLICY "household_members_can_select_category_types" ON category_types IS 'Members can view household category types';
COMMENT ON POLICY "household_members_can_insert_category_types" ON category_types IS 'Members can create category types for their household';
COMMENT ON POLICY "household_members_can_update_category_types" ON category_types IS 'Members can update household category types';
COMMENT ON POLICY "household_members_can_delete_category_types" ON category_types IS 'Members can delete household category types';

-- ============================================================================
-- STEP 7: Update RLS policies for categories
-- ============================================================================

-- Drop old public policies
DROP POLICY IF EXISTS "anyone_can_select_active_categories" ON categories;

-- SELECT: Household members can view their household's categories
CREATE POLICY "household_members_can_select_categories" ON categories
    FOR SELECT TO authenticated
    USING (
        household_id IN (
            SELECT household_id FROM household_members
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

-- INSERT: Household members can create categories
CREATE POLICY "household_members_can_insert_categories" ON categories
    FOR INSERT TO authenticated
    WITH CHECK (
        household_id IN (
            SELECT household_id FROM household_members
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

-- UPDATE: Household members can update their household's categories
CREATE POLICY "household_members_can_update_categories" ON categories
    FOR UPDATE TO authenticated
    USING (
        household_id IN (
            SELECT household_id FROM household_members
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    )
    WITH CHECK (
        household_id IN (
            SELECT household_id FROM household_members
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

-- DELETE: Household members can delete their household's categories
CREATE POLICY "household_members_can_delete_categories" ON categories
    FOR DELETE TO authenticated
    USING (
        household_id IN (
            SELECT household_id FROM household_members
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

COMMENT ON POLICY "household_members_can_select_categories" ON categories IS 'Members can view household categories';
COMMENT ON POLICY "household_members_can_insert_categories" ON categories IS 'Members can create categories for their household';
COMMENT ON POLICY "household_members_can_update_categories" ON categories IS 'Members can update household categories';
COMMENT ON POLICY "household_members_can_delete_categories" ON categories IS 'Members can delete household categories';

-- ============================================================================
-- END OF MIGRATION
-- ============================================================================
