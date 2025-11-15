-- Migration: Remove title, description, and is_recurring columns from events table
-- Events should not have their own title - they display the task's name
-- Events should not have description field - use notes instead
-- Events should not have is_recurring field - check task's interval instead

-- Drop columns from events table
ALTER TABLE events
  DROP COLUMN IF EXISTS title,
  DROP COLUMN IF EXISTS description,
  DROP COLUMN IF EXISTS is_recurring;

-- Add notes column if it doesn't exist (for event-specific notes)
ALTER TABLE events
  ADD COLUMN IF NOT EXISTS notes TEXT;

-- Add comment to clarify that events don't have titles
COMMENT ON TABLE events IS 'Events (task occurrences) - concrete scheduled instances of tasks. Events display the task name as their title and do not have their own title field.';
