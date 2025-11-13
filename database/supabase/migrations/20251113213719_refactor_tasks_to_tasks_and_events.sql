-- Migration: Refactor tasks table into tasks (templates) and events (occurrences)
-- Description: Splits the tasks table into two:
--   - tasks: Templates defining "what" and "how often"
--   - events: Concrete scheduled occurrences with due dates
-- Author: Homely Database Schema
-- Date: 2025-11-13

-- ============================================================================
-- STEP 1: Rename current tasks table to events
-- ============================================================================
ALTER TABLE tasks RENAME TO events;

-- ============================================================================
-- STEP 2: Rename columns in events table to match EventEntity
-- ============================================================================
-- The item_id column will become task_id after we create the new tasks table
ALTER TABLE events RENAME COLUMN item_id TO task_id;

-- Make task_id nullable since events can exist without a task template
ALTER TABLE events ALTER COLUMN task_id DROP NOT NULL;

-- ============================================================================
-- STEP 3: Create new tasks table (templates)
-- ============================================================================
CREATE TABLE tasks (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    household_id uuid REFERENCES households(id) ON DELETE CASCADE NOT NULL,
    category_id integer REFERENCES categories(id),
    name varchar(100) NOT NULL,
    description text,
    years_value integer,
    months_value integer,
    weeks_value integer,
    days_value integer,
    last_date date,
    priority varchar(10) DEFAULT 'medium' CHECK (priority IN ('low', 'medium', 'high')),
    notes text,
    is_active boolean DEFAULT true,
    created_by uuid REFERENCES auth.users(id) NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    updated_at timestamp with time zone DEFAULT now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for tasks
ALTER TABLE tasks ENABLE ROW LEVEL SECURITY;

COMMENT ON TABLE tasks IS 'Task templates defining what needs to be done and how often (intervals)';
COMMENT ON COLUMN tasks.category_id IS 'References categories.id for task categorization';
COMMENT ON COLUMN tasks.years_value IS 'Recurrence interval in years';
COMMENT ON COLUMN tasks.months_value IS 'Recurrence interval in months';
COMMENT ON COLUMN tasks.weeks_value IS 'Recurrence interval in weeks';
COMMENT ON COLUMN tasks.days_value IS 'Recurrence interval in days';
COMMENT ON COLUMN tasks.last_date IS 'Date when this task was last completed';
COMMENT ON COLUMN tasks.is_active IS 'Whether this task template is active';

-- ============================================================================
-- STEP 4: Update events table foreign key to reference tasks
-- ============================================================================
-- Drop the old foreign key constraint (it was pointing to items)
-- Note: The constraint name might vary, this is the expected name
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'events_task_id_fkey' AND table_name = 'events'
    ) THEN
        ALTER TABLE events DROP CONSTRAINT events_task_id_fkey;
    END IF;
END $$;

-- Add new foreign key constraint pointing to tasks table
ALTER TABLE events ADD CONSTRAINT events_task_id_fkey
    FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE SET NULL;

-- ============================================================================
-- STEP 5: Update RLS policies for new table structure
-- ============================================================================
-- Drop old tasks RLS policies (now on events table)
DROP POLICY IF EXISTS "tasks_select_policy" ON events;
DROP POLICY IF EXISTS "tasks_insert_policy" ON events;
DROP POLICY IF EXISTS "tasks_update_policy" ON events;
DROP POLICY IF EXISTS "tasks_delete_policy" ON events;

-- Create new events RLS policies
CREATE POLICY "events_select_policy" ON events
    FOR SELECT
    USING (
        household_id IN (
            SELECT household_id
            FROM household_members
            WHERE user_id = auth.uid()
            AND deleted_at IS NULL
        )
        AND deleted_at IS NULL
    );

CREATE POLICY "events_insert_policy" ON events
    FOR INSERT
    WITH CHECK (
        household_id IN (
            SELECT household_id
            FROM household_members
            WHERE user_id = auth.uid()
            AND role IN ('admin', 'member')
            AND deleted_at IS NULL
        )
    );

CREATE POLICY "events_update_policy" ON events
    FOR UPDATE
    USING (
        household_id IN (
            SELECT household_id
            FROM household_members
            WHERE user_id = auth.uid()
            AND role IN ('admin', 'member')
            AND deleted_at IS NULL
        )
    );

CREATE POLICY "events_delete_policy" ON events
    FOR DELETE
    USING (
        household_id IN (
            SELECT household_id
            FROM household_members
            WHERE user_id = auth.uid()
            AND role = 'admin'
            AND deleted_at IS NULL
        )
    );

-- Create RLS policies for new tasks table
CREATE POLICY "tasks_select_policy" ON tasks
    FOR SELECT
    USING (
        household_id IN (
            SELECT household_id
            FROM household_members
            WHERE user_id = auth.uid()
            AND deleted_at IS NULL
        )
        AND deleted_at IS NULL
    );

CREATE POLICY "tasks_insert_policy" ON tasks
    FOR INSERT
    WITH CHECK (
        household_id IN (
            SELECT household_id
            FROM household_members
            WHERE user_id = auth.uid()
            AND role IN ('admin', 'member')
            AND deleted_at IS NULL
        )
    );

CREATE POLICY "tasks_update_policy" ON tasks
    FOR UPDATE
    USING (
        household_id IN (
            SELECT household_id
            FROM household_members
            WHERE user_id = auth.uid()
            AND role IN ('admin', 'member')
            AND deleted_at IS NULL
        )
    );

CREATE POLICY "tasks_delete_policy" ON tasks
    FOR DELETE
    USING (
        household_id IN (
            SELECT household_id
            FROM household_members
            WHERE user_id = auth.uid()
            AND role = 'admin'
            AND deleted_at IS NULL
        )
    );

-- ============================================================================
-- STEP 6: Create indexes for performance
-- ============================================================================
CREATE INDEX idx_tasks_household_id ON tasks(household_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_tasks_category_id ON tasks(category_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_tasks_is_active ON tasks(is_active) WHERE deleted_at IS NULL;
CREATE INDEX idx_tasks_created_by ON tasks(created_by) WHERE deleted_at IS NULL;

-- Update events indexes (some might already exist with old names)
DROP INDEX IF EXISTS idx_tasks_household_id;
DROP INDEX IF EXISTS idx_tasks_item_id;
DROP INDEX IF EXISTS idx_tasks_status;
DROP INDEX IF EXISTS idx_tasks_due_date;

CREATE INDEX idx_events_household_id ON events(household_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_events_task_id ON events(task_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_events_status ON events(status) WHERE deleted_at IS NULL;
CREATE INDEX idx_events_due_date ON events(due_date) WHERE deleted_at IS NULL;
CREATE INDEX idx_events_assigned_to ON events(assigned_to) WHERE deleted_at IS NULL;

-- ============================================================================
-- STEP 7: Update tasks_history table foreign keys
-- ============================================================================
-- tasks_history.task_id should reference events table (the completed event)
-- tasks_history.item_id should reference tasks table (the task template)

-- Drop old foreign key constraints
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'tasks_history_task_id_fkey' AND table_name = 'tasks_history'
    ) THEN
        ALTER TABLE tasks_history DROP CONSTRAINT tasks_history_task_id_fkey;
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'tasks_history_item_id_fkey' AND table_name = 'tasks_history'
    ) THEN
        ALTER TABLE tasks_history DROP CONSTRAINT tasks_history_item_id_fkey;
    END IF;
END $$;

-- Add new foreign key constraints
-- task_id references events (the completed event occurrence)
ALTER TABLE tasks_history ADD CONSTRAINT tasks_history_task_id_fkey
    FOREIGN KEY (task_id) REFERENCES events(id) ON DELETE CASCADE;

-- item_id references tasks (the task template)
ALTER TABLE tasks_history ADD CONSTRAINT tasks_history_item_id_fkey
    FOREIGN KEY (item_id) REFERENCES tasks(id) ON DELETE CASCADE;

-- ============================================================================
-- END OF MIGRATION
-- ============================================================================
