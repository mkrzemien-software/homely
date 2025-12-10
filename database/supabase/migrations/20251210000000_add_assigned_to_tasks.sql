-- Migration: Add assigned_to column to tasks table
-- Description: Adds optional assigned_to field to task templates for default event assignment
-- Author: Homely MVP Database Schema
-- Date: 2025-12-10

-- Add assigned_to column to tasks table
alter table tasks
add column assigned_to uuid references auth.users(id);

comment on column tasks.assigned_to is 'Default user assignment for events generated from this task template (optional)';
