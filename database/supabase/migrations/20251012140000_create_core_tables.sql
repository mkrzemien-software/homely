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
    max_items integer,
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
create table category_types (
    id serial primary key,
    name varchar(100) not null unique,
    description text,
    sort_order integer default 0,
    is_active boolean default true,
    created_at timestamp with time zone default now(),
    updated_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for category types
alter table category_types enable row level security;

comment on table category_types is 'High-level category groupings for organizing item categories';
comment on column category_types.sort_order is 'Display order for category types';

-- ============================================================================
-- 5. CATEGORIES TABLE
-- ============================================================================
-- Specific categories within category types
create table categories (
    id serial primary key,
    category_type_id integer references category_types(id),
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

comment on table categories is 'Specific categories within category types for item classification';
comment on column categories.category_type_id is 'References category_types.id for hierarchical organization';

-- ============================================================================
-- 6. ITEMS TABLE
-- ============================================================================
-- Household items/appliances/visits that need maintenance tracking
create table items (
    id uuid primary key default gen_random_uuid(),
    household_id uuid references households(id) on delete cascade,
    category_id integer references categories(id),
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
    created_by uuid references auth.users(id) not null,
    created_at timestamp with time zone default now(),
    updated_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for items
alter table items enable row level security;

comment on table items is 'Household items, appliances, and visits that require maintenance tracking';
comment on column items.years_value is 'Maintenance interval in years';
comment on column items.months_value is 'Maintenance interval in months';
comment on column items.weeks_value is 'Maintenance interval in weeks';
comment on column items.days_value is 'Maintenance interval in days';
comment on column items.last_date is 'Date of last maintenance/service';
comment on column items.priority is 'Item priority level: low, medium, high';

-- ============================================================================
-- 7. TASKS TABLE
-- ============================================================================
-- Individual maintenance tasks/appointments for items
create table tasks (
    id uuid primary key default gen_random_uuid(),
    item_id uuid references items(id) on delete cascade,
    household_id uuid references households(id) on delete cascade,
    assigned_to uuid references auth.users(id),
    due_date date not null,
    title varchar(200) not null,
    description text,
    status varchar(20) default 'pending' check (status in ('pending', 'completed', 'postponed', 'cancelled')),
    priority varchar(10) default 'medium' check (priority in ('low', 'medium', 'high')),
    completion_date date,
    completion_notes text,
    postponed_from_date date,
    postpone_reason text,
    is_recurring boolean default true,
    created_by uuid references auth.users(id) not null,
    created_at timestamp with time zone default now(),
    updated_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for tasks
alter table tasks enable row level security;

comment on table tasks is 'Individual maintenance tasks and appointments with scheduling and tracking';
comment on column tasks.status is 'Task status: pending, completed, postponed, cancelled';
comment on column tasks.is_recurring is 'Whether task should automatically regenerate when completed';
comment on column tasks.postponed_from_date is 'Original due date if task was postponed';

-- ============================================================================
-- 8. TASKS HISTORY TABLE
-- ============================================================================
-- Historical record of completed tasks (premium feature)
create table tasks_history (
    id uuid primary key default gen_random_uuid(),
    task_id uuid references tasks(id) on delete cascade,
    item_id uuid references items(id) on delete cascade,
    household_id uuid references households(id) on delete cascade,
    assigned_to uuid references auth.users(id),
    completed_by uuid references auth.users(id),
    due_date date not null,
    completion_date date not null,
    title varchar(200) not null,
    completion_notes text,
    created_at timestamp with time zone default now(),
    deleted_at timestamp with time zone
);

-- Enable RLS for tasks history
alter table tasks_history enable row level security;

comment on table tasks_history is 'Historical record of completed tasks for premium plan analytics';
comment on column tasks_history.completed_by is 'User who marked the task as completed';

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


