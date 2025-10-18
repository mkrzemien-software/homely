-- Migration: Create Database Indexes for Performance Optimization
-- Description: Creates strategic indexes for core queries, dashboard performance, and data access patterns
-- Tables: All core tables with performance-critical indexes
-- Author: Homely MVP Database Schema
-- Date: 2025-10-12

-- ============================================================================
-- PERFORMANCE INDEXES FOR CORE TABLES
-- ============================================================================

-- ============================================================================
-- 1. PLAN TYPES INDEXES
-- ============================================================================
-- Index for active plan filtering - used when presenting available plans
create index idx_plan_types_active on plan_types(is_active);

-- Index for soft delete filtering
create index idx_plan_types_deleted_at on plan_types(deleted_at);

comment on index idx_plan_types_active is 'Filters active plans available for subscription';
comment on index idx_plan_types_deleted_at is 'Optimizes soft delete filtering for plan types';

-- ============================================================================
-- 2. HOUSEHOLDS INDEXES
-- ============================================================================
-- Index for plan type lookup - used for subscription management queries
create index idx_households_plan_type on households(plan_type_id);

-- Index for soft delete filtering
create index idx_households_deleted_at on households(deleted_at);

comment on index idx_households_plan_type is 'Optimizes plan type lookups for subscription management';
comment on index idx_households_deleted_at is 'Optimizes soft delete filtering for households';

-- ============================================================================
-- 3. HOUSEHOLD MEMBERS INDEXES
-- ============================================================================
-- Critical index for household membership lookups - used in most RLS policies
create index idx_household_members_household on household_members(household_id) where deleted_at is null;

-- Index for user membership lookups - used when finding user's households
create index idx_household_members_user on household_members(user_id) where deleted_at is null;

-- Composite index for role-based access control queries
create index idx_household_members_role on household_members(household_id, role) where deleted_at is null;

-- Unique constraint to prevent duplicate active memberships
create unique index idx_household_members_unique_active on household_members(household_id, user_id) where deleted_at is null;

comment on index idx_household_members_household is 'Critical for RLS policies - finds members of a household';
comment on index idx_household_members_user is 'Finds all households a user belongs to';
comment on index idx_household_members_role is 'Optimizes role-based access control queries';
comment on index idx_household_members_unique_active is 'Prevents duplicate active memberships';

-- ============================================================================
-- 4. CATEGORY TYPES INDEXES
-- ============================================================================
-- Index for active category types
create index idx_category_types_active on category_types(is_active);

-- Index for display ordering
create index idx_category_types_sort_order on category_types(sort_order);

-- Index for soft delete filtering
create index idx_category_types_deleted_at on category_types(deleted_at);

comment on index idx_category_types_active is 'Filters active category types for display';
comment on index idx_category_types_sort_order is 'Optimizes category type ordering queries';

-- ============================================================================
-- 5. CATEGORIES INDEXES
-- ============================================================================
-- Index for category type relationships
create index idx_categories_type on categories(category_type_id);

-- Index for active categories filtering
create index idx_categories_active on categories(is_active) where deleted_at is null;

-- Index for soft delete filtering
create index idx_categories_deleted_at on categories(deleted_at);

-- Unique constraint for category names within category types (full constraint for ON CONFLICT support)
alter table categories add constraint categories_unique_name unique (category_type_id, name);

comment on index idx_categories_type is 'Optimizes category type to categories relationships';
comment on index idx_categories_active is 'Filters active categories for item assignment';

-- ============================================================================
-- 6. ITEMS INDEXES
-- ============================================================================
-- Critical index for household items - used extensively in dashboard queries
create index idx_items_household on items(household_id) where deleted_at is null;

-- Index for category-based filtering and reporting
create index idx_items_category on items(category_id) where deleted_at is null;

-- Index for tracking item creators
create index idx_items_created_by on items(created_by) where deleted_at is null;

-- Index for active items filtering
create index idx_items_active on items(is_active) where deleted_at is null;

comment on index idx_items_household is 'Critical for dashboard queries - finds all household items';
comment on index idx_items_category is 'Optimizes category-based item filtering and reports';
comment on index idx_items_created_by is 'Tracks item creation for audit purposes';
comment on index idx_items_active is 'Filters active items in household views';

-- ============================================================================
-- 7. TASKS INDEXES - CRITICAL FOR DASHBOARD PERFORMANCE
-- ============================================================================
-- Most important index - due date sorting for dashboard upcoming tasks
create index idx_tasks_due_date on tasks(due_date) where deleted_at is null;

-- Composite index for household task filtering by status - critical for dashboard
create index idx_tasks_household_status on tasks(household_id, status) where deleted_at is null;

-- Index for task assignment queries
create index idx_tasks_assigned_to on tasks(assigned_to) where deleted_at is null;

-- Composite index for status and due date - optimizes dashboard urgency queries
create index idx_tasks_status_due on tasks(status, due_date) where deleted_at is null;

-- Index for item-to-tasks relationships
create index idx_tasks_item on tasks(item_id) where deleted_at is null;

comment on index idx_tasks_due_date is 'CRITICAL: Optimizes dashboard upcoming tasks sorting by due date';
comment on index idx_tasks_household_status is 'CRITICAL: Dashboard household task filtering by status';
comment on index idx_tasks_assigned_to is 'Optimizes user-assigned task queries';
comment on index idx_tasks_status_due is 'CRITICAL: Dashboard urgency calculations (overdue, today, upcoming)';
comment on index idx_tasks_item is 'Links tasks back to their parent items';

-- ============================================================================
-- 8. TASKS HISTORY INDEXES
-- ============================================================================
-- Index for household history queries (premium feature)
create index idx_tasks_history_household on tasks_history(household_id);

-- Index for completion date analytics
create index idx_tasks_history_completion_date on tasks_history(completion_date);

-- Index for item history tracking
create index idx_tasks_history_item on tasks_history(item_id);

comment on index idx_tasks_history_household is 'Premium feature: household maintenance history analytics';
comment on index idx_tasks_history_completion_date is 'Time-based analytics for completion patterns';
comment on index idx_tasks_history_item is 'Item-specific maintenance history tracking';

-- ============================================================================
-- 9. PLAN USAGE INDEXES
-- ============================================================================
-- Composite index for usage tracking queries
create index idx_plan_usage_household_type on plan_usage(household_id, usage_type);

-- Index for date-based usage analytics
create index idx_plan_usage_date on plan_usage(usage_date);

comment on index idx_plan_usage_household_type is 'Optimizes plan limit checking queries';
comment on index idx_plan_usage_date is 'Enables usage analytics over time';

-- ============================================================================
-- END OF PERFORMANCE INDEXES MIGRATION
-- ============================================================================


