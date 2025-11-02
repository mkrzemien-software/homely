-- Migration: Create User Profiles Table
-- Description: Creates user_profiles table for extended user information beyond Supabase auth.users
-- Tables: user_profiles with indexes and RLS policies
-- Author: Homely MVP Database Schema
-- Date: 2025-11-02

-- ============================================================================
-- 1. USER PROFILES TABLE
-- ============================================================================
-- Extended user information table that complements auth.users
-- Stores additional profile data needed for the application

create table user_profiles (
    -- Primary key that references auth.users(id)
    user_id uuid primary key references auth.users(id) on delete cascade,

    -- Basic user information
    first_name varchar(50) not null,
    last_name varchar(50) not null,
    avatar_url varchar(500),
    phone_number varchar(20),

    -- Localization preferences
    preferred_language varchar(5) default 'pl' not null,
    timezone varchar(50) default 'Europe/Warsaw' not null,

    -- Activity tracking
    last_active_at timestamp with time zone,

    -- Timestamps
    created_at timestamp with time zone default now() not null,
    updated_at timestamp with time zone default now() not null,
    deleted_at timestamp with time zone
);

-- Enable RLS for user profiles
alter table user_profiles enable row level security;

-- Table and column comments
comment on table user_profiles is 'Extended user profile information beyond Supabase auth.users';
comment on column user_profiles.user_id is 'Primary key referencing auth.users(id) in Supabase';
comment on column user_profiles.first_name is 'User first name (required)';
comment on column user_profiles.last_name is 'User last name (required)';
comment on column user_profiles.avatar_url is 'URL to user avatar image stored in Supabase Storage';
comment on column user_profiles.phone_number is 'User phone number for notifications';
comment on column user_profiles.preferred_language is 'User preferred language (ISO 639-1 code, default: pl)';
comment on column user_profiles.timezone is 'User timezone for date/time display (default: Europe/Warsaw)';
comment on column user_profiles.last_active_at is 'Last time user was active in the system';
comment on column user_profiles.created_at is 'Profile creation timestamp';
comment on column user_profiles.updated_at is 'Profile last update timestamp';
comment on column user_profiles.deleted_at is 'Soft delete timestamp - null means active profile';

-- ============================================================================
-- 2. INDEXES FOR USER PROFILES
-- ============================================================================

-- Index for soft delete filtering
create index idx_user_profiles_deleted_at on user_profiles(deleted_at);

-- Index for name search/sorting
create index idx_user_profiles_names on user_profiles(first_name, last_name) where deleted_at is null;

-- Index for last activity tracking
create index idx_user_profiles_last_active on user_profiles(last_active_at) where deleted_at is null;

-- Index comments
comment on index idx_user_profiles_deleted_at is 'Optimizes soft delete filtering for user profiles';
comment on index idx_user_profiles_names is 'Optimizes user name search and sorting queries';
comment on index idx_user_profiles_last_active is 'Tracks user activity for analytics and session management';

-- ============================================================================
-- 3. ROW LEVEL SECURITY POLICIES
-- ============================================================================

-- Select policy: Users can view their own profile and profiles of household members
create policy "users_can_select_own_and_household_profiles" on user_profiles
    for select to authenticated
    using (
        user_id = auth.uid()
        or
        user_id in (
            select hm.user_id
            from household_members hm
            where hm.household_id in (
                select household_id
                from household_members
                where user_id = auth.uid() and deleted_at is null
            )
            and hm.deleted_at is null
        )
    );

-- Insert policy: Users can only create their own profile
create policy "users_can_insert_own_profile" on user_profiles
    for insert to authenticated
    with check (user_id = auth.uid());

-- Update policy: Users can only update their own profile
create policy "users_can_update_own_profile" on user_profiles
    for update to authenticated
    using (user_id = auth.uid())
    with check (user_id = auth.uid());

-- Delete policy: Users can soft delete their own profile
create policy "users_can_delete_own_profile" on user_profiles
    for delete to authenticated
    using (user_id = auth.uid());

-- Policy comments
comment on policy "users_can_select_own_and_household_profiles" on user_profiles is 'Users can view their own profile and profiles of household members';
comment on policy "users_can_insert_own_profile" on user_profiles is 'Users can only create their own profile';
comment on policy "users_can_update_own_profile" on user_profiles is 'Users can only update their own profile information';
comment on policy "users_can_delete_own_profile" on user_profiles is 'Users can soft delete (mark as deleted) their own profile';

-- ============================================================================
-- 4. TRIGGERS FOR AUTOMATIC TIMESTAMP UPDATES
-- ============================================================================

-- Function to update updated_at timestamp
create or replace function update_user_profiles_updated_at()
returns trigger as $$
begin
    new.updated_at = now();
    return new;
end;
$$ language plpgsql;

-- Trigger to automatically update updated_at on row update
create trigger trigger_user_profiles_updated_at
    before update on user_profiles
    for each row
    execute function update_user_profiles_updated_at();

comment on function update_user_profiles_updated_at() is 'Automatically updates updated_at timestamp on user profile changes';
comment on trigger trigger_user_profiles_updated_at on user_profiles is 'Triggers automatic updated_at timestamp update';

-- ============================================================================
-- END OF USER PROFILES MIGRATION
-- ============================================================================
