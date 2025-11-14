-- Migration: Fix tasks table foreign keys
-- Description: Updates created_by foreign key to reference user_profiles instead of auth.users
-- Author: Homely Database Schema
-- Date: 2025-11-14

-- ============================================================================
-- STEP 1: Drop existing foreign key constraint
-- ============================================================================
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'tasks_created_by_fkey' AND table_name = 'tasks'
    ) THEN
        ALTER TABLE tasks DROP CONSTRAINT tasks_created_by_fkey;
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'tasks_created_by_fkey1' AND table_name = 'tasks'
    ) THEN
        ALTER TABLE tasks DROP CONSTRAINT tasks_created_by_fkey1;
    END IF;
END $$;

-- ============================================================================
-- STEP 2: Add new foreign key constraint to user_profiles
-- ============================================================================
ALTER TABLE tasks ADD CONSTRAINT tasks_created_by_fkey
    FOREIGN KEY (created_by) REFERENCES user_profiles(user_id) ON DELETE RESTRICT;

COMMENT ON CONSTRAINT tasks_created_by_fkey ON tasks IS 'Foreign key to user_profiles for task creator';

-- ============================================================================
-- STEP 3: Update events table foreign keys if needed
-- ============================================================================
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'events_created_by_fkey' AND table_name = 'events'
    ) THEN
        ALTER TABLE events DROP CONSTRAINT events_created_by_fkey;
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'events_created_by_fkey1' AND table_name = 'events'
    ) THEN
        ALTER TABLE events DROP CONSTRAINT events_created_by_fkey1;
    END IF;
END $$;

ALTER TABLE events ADD CONSTRAINT events_created_by_fkey
    FOREIGN KEY (created_by) REFERENCES user_profiles(user_id) ON DELETE RESTRICT;

-- Also fix assigned_to foreign key in events
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'events_assigned_to_fkey' AND table_name = 'events'
    ) THEN
        ALTER TABLE events DROP CONSTRAINT events_assigned_to_fkey;
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'events_assigned_to_fkey1' AND table_name = 'events'
    ) THEN
        ALTER TABLE events DROP CONSTRAINT events_assigned_to_fkey1;
    END IF;
END $$;

ALTER TABLE events ADD CONSTRAINT events_assigned_to_fkey
    FOREIGN KEY (assigned_to) REFERENCES user_profiles(user_id) ON DELETE SET NULL;

COMMENT ON CONSTRAINT events_created_by_fkey ON events IS 'Foreign key to user_profiles for event creator';
COMMENT ON CONSTRAINT events_assigned_to_fkey ON events IS 'Foreign key to user_profiles for assigned user';

-- ============================================================================
-- END OF MIGRATION
-- ============================================================================
