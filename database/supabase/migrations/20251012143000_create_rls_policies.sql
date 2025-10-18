-- Migration: Create Row Level Security Policies
-- Description: Implements comprehensive RLS policies for data isolation and security
-- Tables: All tables with granular access control policies for authenticated and anonymous users
-- Author: Homely MVP Database Schema  
-- Date: 2025-10-12

-- ============================================================================
-- ROW LEVEL SECURITY POLICIES
-- ============================================================================
-- Implements data isolation between households and role-based access control
-- Each policy is granular (separate for select, insert, update, delete) for security clarity

-- ============================================================================
-- 1. PLAN TYPES POLICIES (PUBLIC READ-ONLY)
-- ============================================================================
-- Plan types are publicly readable for pricing pages, but not modifiable by users

-- Select policy: Anyone can view active plan types
create policy "anyone_can_select_active_plan_types" on plan_types
    for select to anon, authenticated
    using (is_active = true and deleted_at is null);

-- No insert/update/delete policies for regular users - admin only operations

comment on policy "anyone_can_select_active_plan_types" on plan_types is 'Public access to view available subscription plans';

-- ============================================================================
-- 2. HOUSEHOLDS POLICIES
-- ============================================================================
-- Household access restricted to members only, with admin-only modifications

-- Select policy: Household members can view household data
create policy "household_members_can_select_household" on households
    for select to authenticated
    using (
        id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

-- Insert policy: Authenticated users can create new households (becomes admin)
create policy "authenticated_users_can_insert_household" on households
    for insert to authenticated
    with check (true);

-- Update policy: Only household admins can update household data
create policy "household_admins_can_update_household" on households
    for update to authenticated
    using (
        id in (
            select household_id from household_members 
            where user_id = auth.uid() and role = 'admin' and deleted_at is null
        )
    )
    with check (
        id in (
            select household_id from household_members 
            where user_id = auth.uid() and role = 'admin' and deleted_at is null
        )
    );

-- Delete policy: Only household admins can delete households
create policy "household_admins_can_delete_household" on households
    for delete to authenticated
    using (
        id in (
            select household_id from household_members 
            where user_id = auth.uid() and role = 'admin' and deleted_at is null
        )
    );

comment on policy "household_members_can_select_household" on households is 'Only household members can view household information';
comment on policy "household_admins_can_update_household" on households is 'Only admins can modify household settings and subscription';

-- ============================================================================
-- 3. HOUSEHOLD MEMBERS POLICIES
-- ============================================================================
-- Members can view other members, admins can manage membership

-- Select policy: Household members can view all members of their households
create policy "household_members_can_select_members" on household_members
    for select to authenticated
    using (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

-- Insert policy: Household admins can invite new members
create policy "household_admins_can_insert_members" on household_members
    for insert to authenticated
    with check (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and role = 'admin' and deleted_at is null
        )
    );

-- Update policy: Household admins can update member roles and status
create policy "household_admins_can_update_members" on household_members
    for update to authenticated
    using (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and role = 'admin' and deleted_at is null
        )
    )
    with check (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and role = 'admin' and deleted_at is null
        )
    );

-- Delete policy: Household admins can remove members, users can remove themselves
create policy "household_members_can_delete_members" on household_members
    for delete to authenticated
    using (
        -- Admin can remove any member OR user can remove themselves
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and role = 'admin' and deleted_at is null
        )
        or user_id = auth.uid()
    );

comment on policy "household_members_can_select_members" on household_members is 'Members can view all household members';
comment on policy "household_admins_can_insert_members" on household_members is 'Only admins can invite new members';
comment on policy "household_members_can_delete_members" on household_members is 'Admins can remove members, users can leave households';

-- ============================================================================
-- 4. CATEGORY TYPES POLICIES (PUBLIC READ-ONLY)
-- ============================================================================
-- Category types are publicly readable for item categorization

-- Select policy: Anyone can view active category types
create policy "anyone_can_select_active_category_types" on category_types
    for select to anon, authenticated
    using (is_active = true and deleted_at is null);

comment on policy "anyone_can_select_active_category_types" on category_types is 'Public access to view item category types';

-- ============================================================================
-- 5. CATEGORIES POLICIES (PUBLIC READ-ONLY)
-- ============================================================================
-- Categories are publicly readable for item classification

-- Select policy: Anyone can view active categories
create policy "anyone_can_select_active_categories" on categories
    for select to anon, authenticated
    using (is_active = true and deleted_at is null);

comment on policy "anyone_can_select_active_categories" on categories is 'Public access to view item categories';

-- ============================================================================
-- 6. ITEMS POLICIES
-- ============================================================================
-- Items are restricted to household members with full CRUD access

-- Select policy: Household members can view household items
create policy "household_members_can_select_items" on items
    for select to authenticated
    using (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

-- Insert policy: Household members can add new items
create policy "household_members_can_insert_items" on items
    for insert to authenticated
    with check (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

-- Update policy: Household members can update items
create policy "household_members_can_update_items" on items
    for update to authenticated
    using (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    )
    with check (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

-- Delete policy: Household members can delete items
create policy "household_members_can_delete_items" on items
    for delete to authenticated
    using (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

comment on policy "household_members_can_select_items" on items is 'Members can view all household items and equipment';
comment on policy "household_members_can_insert_items" on items is 'Members can add new items to household inventory';

-- ============================================================================
-- 7. TASKS POLICIES
-- ============================================================================
-- Tasks are restricted to household members with full CRUD access

-- Select policy: Household members can view household tasks
create policy "household_members_can_select_tasks" on tasks
    for select to authenticated
    using (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

-- Insert policy: Household members can create new tasks
create policy "household_members_can_insert_tasks" on tasks
    for insert to authenticated
    with check (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

-- Update policy: Household members can update tasks
create policy "household_members_can_update_tasks" on tasks
    for update to authenticated
    using (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    )
    with check (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

-- Delete policy: Household members can delete tasks
create policy "household_members_can_delete_tasks" on tasks
    for delete to authenticated
    using (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

comment on policy "household_members_can_select_tasks" on tasks is 'Members can view all household maintenance tasks';
comment on policy "household_members_can_insert_tasks" on tasks is 'Members can create new maintenance tasks';

-- ============================================================================
-- 8. TASKS HISTORY POLICIES (PREMIUM FEATURE)
-- ============================================================================
-- Task history access for household members (premium analytics)

-- Select policy: Household members can view task history
create policy "household_members_can_select_tasks_history" on tasks_history
    for select to authenticated
    using (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

-- Insert policy: System can create history records (via triggers)
create policy "system_can_insert_tasks_history" on tasks_history
    for insert to authenticated
    with check (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

comment on policy "household_members_can_select_tasks_history" on tasks_history is 'Premium feature: Members can view maintenance history analytics';

-- ============================================================================
-- 9. PLAN USAGE POLICIES
-- ============================================================================
-- Plan usage tracking restricted to household members

-- Select policy: Household members can view usage statistics
create policy "household_members_can_select_plan_usage" on plan_usage
    for select to authenticated
    using (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

-- Insert/Update policies: System can manage usage tracking
create policy "system_can_manage_plan_usage" on plan_usage
    for all to authenticated
    using (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    )
    with check (
        household_id in (
            select household_id from household_members 
            where user_id = auth.uid() and deleted_at is null
        )
    );

comment on policy "household_members_can_select_plan_usage" on plan_usage is 'Members can view household plan usage statistics';

-- ============================================================================
-- END OF RLS POLICIES MIGRATION
-- ============================================================================


