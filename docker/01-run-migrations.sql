-- Automatic migration runner for E2E environment
-- This SQL script includes all migrations from database/supabase/migrations/

-- Log start
DO $$
BEGIN
  RAISE NOTICE '========================================';
  RAISE NOTICE 'Running Supabase migrations via SQL...';
  RAISE NOTICE '========================================';
END $$;

-- Migration 1: Create core tables
\i /docker-entrypoint-initdb.d/migrations/20251012140000_create_core_tables.sql

-- Migration 2: Create indexes
\i /docker-entrypoint-initdb.d/migrations/20251012141500_create_indexes.sql

-- Migration 3: Create RLS policies
\i /docker-entrypoint-initdb.d/migrations/20251012143000_create_rls_policies.sql

-- Migration 4: Create views and triggers
\i /docker-entrypoint-initdb.d/migrations/20251012144500_create_views_and_triggers.sql

-- Migration 5: Create user profiles table
\i /docker-entrypoint-initdb.d/migrations/20251102181017_create_user_profiles_table.sql

-- Log completion
DO $$
BEGIN
  RAISE NOTICE '========================================';
  RAISE NOTICE 'Migration execution completed';
  RAISE NOTICE '========================================';
END $$;
