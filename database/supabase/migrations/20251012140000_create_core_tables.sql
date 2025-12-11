-- Migration: Create Core Tables for Homely MVP
-- Description: Creates main application tables for MVP including households, items, and tasks
-- Tables: plan_types, households, household_members, category_types, categories, items, tasks, tasks_history, plan_usage
-- Author: Homely MVP Database Schema
-- Date: 2025-10-12

-- ============================================================================
-- 1. PLAN TYPES TABLE
-- ============================================================================
-- Subscription plan definitions with limits and pricing
create table plan_types (
    id serial primary key,
    name varchar(50) not null unique,
    description text,
    max_household_members integer,
    max_tasks integer,
    price_monthly decimal(10,2),
    price_yearly decimal(10,2),
    features jsonb default '[]',
    is_active boolean default true,
    created_at timestamp with time zone default now(),
    updated_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for plan types
alter table plan_types enable row level security;

comment on table plan_types is 'Subscription plan definitions with features and limits';
comment on column plan_types.features is 'JSON array of available features for this plan';
comment on column plan_types.is_active is 'Whether this plan is available for new subscriptions';

-- ============================================================================
-- 2. HOUSEHOLDS TABLE
-- ============================================================================
-- Main household entity with subscription information
create table households (
    id uuid primary key default gen_random_uuid(),
    name varchar(100) not null,
    address text,
    plan_type_id integer references plan_types(id) default 1,
    subscription_status varchar(20) default 'free' check (subscription_status in ('free', 'active', 'cancelled', 'expired')),
    subscription_start_date date,
    subscription_end_date date,
    created_at timestamp with time zone default now(),
    updated_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for households
alter table households enable row level security;

comment on table households is 'Main household entities with subscription and plan information';
comment on column households.subscription_status is 'Current subscription status: free, active, cancelled, expired';
comment on column households.plan_type_id is 'References plan_types.id, defaults to free plan (id=1)';

-- ============================================================================
-- 3. HOUSEHOLD MEMBERS TABLE
-- ============================================================================
-- Junction table for users-households relationship with roles
create table household_members (
    id uuid primary key default gen_random_uuid(),
    household_id uuid references households(id) on delete cascade,
    user_id uuid references auth.users(id) on delete cascade,
    role varchar(20) not null check (role in ('admin', 'member', 'dashboard')),
    invited_by uuid references auth.users(id),
    invitation_token varchar(100) unique,
    invitation_expires_at timestamp with time zone,
    joined_at timestamp with time zone default now(),
    created_at timestamp with time zone default now(),
    updated_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for household members
alter table household_members enable row level security;

comment on table household_members is 'Many-to-many relationship between users and households with role-based access';
comment on column household_members.role is 'User role in household: admin (full access), member (standard access), dashboard (view only)';
comment on column household_members.invitation_token is 'Unique token for household invitations';
comment on column household_members.invitation_expires_at is 'Expiration time for pending invitations';

-- ============================================================================
-- 4. CATEGORY TYPES TABLE
-- ============================================================================
-- High-level category groupings (maintenance, medical visits, waste management)
-- Multi-tenant: Each household has its own category types
create table category_types (
    id uuid primary key default gen_random_uuid(),
    household_id uuid references households(id) on delete cascade not null,
    name varchar(100) not null,
    description text,
    sort_order integer default 0,
    is_active boolean default true,
    created_at timestamp with time zone default now(),
    updated_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for category types
alter table category_types enable row level security;

comment on table category_types is 'High-level category groupings for organizing item categories (multi-tenant per household)';
comment on column category_types.household_id is 'Household that owns this category type - enables multi-tenant isolation';
comment on column category_types.sort_order is 'Display order for category types';

-- ============================================================================
-- 5. CATEGORIES TABLE
-- ============================================================================
-- Specific categories within category types
-- Multi-tenant: Each household has its own categories
create table categories (
    id uuid primary key default gen_random_uuid(),
    household_id uuid references households(id) on delete cascade not null,
    category_type_id uuid references category_types(id) on delete set null,
    name varchar(100) not null,
    description text,
    sort_order integer default 0,
    is_active boolean default true,
    created_at timestamp with time zone default now(),
    updated_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for categories
alter table categories enable row level security;

comment on table categories is 'Specific categories within category types for item classification (multi-tenant per household)';
comment on column categories.household_id is 'Household that owns this category - enables multi-tenant isolation';
comment on column categories.category_type_id is 'References category_types.id (UUID) for hierarchical organization';

-- ============================================================================
-- 6. TASKS TABLE
-- ============================================================================
-- Task templates defining what needs to be done and how often
create table tasks (
    id uuid primary key default gen_random_uuid(),
    household_id uuid references households(id) on delete cascade not null,
    category_id uuid references categories(id) on delete set null,
    name varchar(100) not null,
    description text,
    years_value integer,
    months_value integer,
    weeks_value integer,
    days_value integer,
    last_date date,
    priority varchar(10) default 'medium' check (priority in ('low', 'medium', 'high')),
    notes text,
    is_active boolean default true,
    assigned_to uuid references auth.users(id),
    created_by uuid references auth.users(id) not null,
    created_at timestamp with time zone default now(),
    updated_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for tasks
alter table tasks enable row level security;

comment on table tasks is 'Task templates defining what needs to be done and how often (intervals)';
comment on column tasks.category_id is 'References categories.id (UUID) for task categorization';
comment on column tasks.years_value is 'Recurrence interval in years';
comment on column tasks.months_value is 'Recurrence interval in months';
comment on column tasks.weeks_value is 'Recurrence interval in weeks';
comment on column tasks.days_value is 'Recurrence interval in days';
comment on column tasks.last_date is 'Date when this task was last completed';
comment on column tasks.priority is 'Task priority level: low, medium, high';
comment on column tasks.assigned_to is 'Default user assignment for events generated from this task template (optional)';
comment on column tasks.is_active is 'Whether this task template is active';

-- ============================================================================
-- 7. EVENTS TABLE
-- ============================================================================
-- Concrete scheduled occurrences of tasks (individual appointments/due dates)
create table events (
    id uuid primary key default gen_random_uuid(),
    task_id uuid references tasks(id) on delete set null,
    household_id uuid references households(id) on delete cascade not null,
    assigned_to uuid references auth.users(id),
    due_date date not null,
    status varchar(20) default 'pending' check (status in ('pending', 'completed', 'postponed', 'cancelled')),
    priority varchar(10) default 'medium' check (priority in ('low', 'medium', 'high')),
    completion_date date,
    completion_notes text,
    postponed_from_date date,
    postpone_reason text,
    notes text,
    created_by uuid references auth.users(id) not null,
    created_at timestamp with time zone default now(),
    updated_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for events
alter table events enable row level security;

comment on table events is 'Events (task occurrences) - concrete scheduled instances of tasks. Events display the task name as their title and do not have their own title field.';
comment on column events.task_id is 'References tasks.id - the task template this event is based on (nullable for one-off events)';
comment on column events.status is 'Event status: pending, completed, postponed, cancelled';
comment on column events.postponed_from_date is 'Original due date if event was postponed';
comment on column events.notes is 'Event-specific notes (separate from task template notes)';

-- ============================================================================
-- 8. TASKS HISTORY TABLE
-- ============================================================================
-- Historical record of completed events (premium feature)
create table tasks_history (
    id uuid primary key default gen_random_uuid(),
    event_id uuid references events(id) on delete cascade,
    task_id uuid references tasks(id) on delete cascade,
    household_id uuid references households(id) on delete cascade,
    assigned_to uuid references auth.users(id),
    completed_by uuid references auth.users(id),
    due_date date not null,
    completion_date date not null,
    task_name varchar(100) not null,
    completion_notes text,
    created_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for tasks history
alter table tasks_history enable row level security;

comment on table tasks_history is 'Historical record of completed events for premium plan analytics';
comment on column tasks_history.event_id is 'References events.id - the completed event occurrence';
comment on column tasks_history.task_id is 'References tasks.id - the task template this event was based on';
comment on column tasks_history.completed_by is 'User who marked the event as completed';
comment on column tasks_history.task_name is 'Snapshot of task name at completion time';

-- ============================================================================
-- 9. PLAN USAGE TABLE
-- ============================================================================
-- Tracking of plan limits usage
create table plan_usage (
    id uuid primary key default gen_random_uuid(),
    household_id uuid references households(id) on delete cascade,
    usage_type varchar(50) not null,
    current_value integer default 0,
    max_value integer,
    usage_date date default current_date,
    created_at timestamp with time zone default now(),
    updated_at timestamp with time zone default now(),
    unique(household_id, usage_type, usage_date)
);

-- Enable RLS for plan usage
alter table plan_usage enable row level security;

comment on table plan_usage is 'Tracking current usage against plan limits';
comment on column plan_usage.usage_type is 'Type of usage being tracked (items, members, storage_mb)';
comment on column plan_usage.current_value is 'Current usage count for this usage type';
comment on column plan_usage.max_value is 'Maximum allowed value from plan limits';

-- ============================================================================
-- END OF CORE TABLES MIGRATION
-- ============================================================================


