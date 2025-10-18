-- Migration: Create Views and Triggers
-- Description: Creates optimized views for dashboard queries and automated triggers for business logic
-- Objects: dashboard_upcoming_tasks view, user profile creation trigger, plan data insertion
-- Author: Homely MVP Database Schema
-- Date: 2025-10-12

-- ============================================================================
-- DASHBOARD VIEWS FOR OPTIMIZED QUERIES
-- ============================================================================

-- ============================================================================
-- 1. DASHBOARD UPCOMING TASKS VIEW
-- ============================================================================
-- Optimized view for dashboard showing upcoming maintenance tasks with urgency classification
create view dashboard_upcoming_tasks as
select 
    t.id,
    t.due_date,
    t.title,
    t.description,
    t.status,
    t.priority,
    t.assigned_to,
    i.name as item_name,
    i.description as item_description,
    c.name as category_name,
    ct.name as category_type_name,
    -- User names would need to be fetched from auth.users if needed
    h.id as household_id,
    h.name as household_name,
    -- Urgency classification for dashboard prioritization
    case 
        when t.due_date < current_date then 'overdue'
        when t.due_date = current_date then 'today'
        when t.due_date <= current_date + interval '7 days' then 'this_week'
        when t.due_date <= current_date + interval '30 days' then 'this_month'
        else 'upcoming'
    end as urgency_status,
    -- Days until due (negative for overdue)
    t.due_date - current_date as days_until_due,
    -- Combined priority score for sorting (urgency + priority)
    case 
        when t.due_date < current_date then 1000 + case t.priority when 'high' then 3 when 'medium' then 2 else 1 end
        when t.due_date = current_date then 500 + case t.priority when 'high' then 3 when 'medium' then 2 else 1 end
        when t.due_date <= current_date + interval '7 days' then 100 + case t.priority when 'high' then 3 when 'medium' then 2 else 1 end
        else case t.priority when 'high' then 3 when 'medium' then 2 else 1 end
    end as priority_score
from tasks t
join items i on t.item_id = i.id
join categories c on i.category_id = c.id
join category_types ct on c.category_type_id = ct.id
join households h on t.household_id = h.id
-- User data can be fetched from auth.users if needed
where t.deleted_at is null 
    and t.status = 'pending'
    and i.deleted_at is null
    and i.is_active = true
    and h.deleted_at is null
order by priority_score desc, t.due_date asc, t.priority desc;

comment on view dashboard_upcoming_tasks is 'Optimized dashboard view showing pending tasks with urgency classification and priority scoring';

-- ============================================================================
-- AUTOMATED TRIGGERS FOR BUSINESS LOGIC
-- ============================================================================

-- ============================================================================
-- 2. AUTOMATIC TASK HISTORY CREATION TRIGGER
-- ============================================================================
-- Automatically creates history record when task is completed (premium feature)
create or replace function create_task_history()
returns trigger as $$
begin
    -- Only create history when task is marked as completed
    if new.status = 'completed' and old.status != 'completed' then
        insert into tasks_history (
            task_id,
            item_id,
            household_id,
            assigned_to,
            completed_by,
            due_date,
            completion_date,
            title,
            completion_notes
        )
        values (
            new.id,
            new.item_id,
            new.household_id,
            new.assigned_to,
            auth.uid(), -- user who marked it complete
            new.due_date,
            coalesce(new.completion_date, current_date),
            new.title,
            new.completion_notes
        );
    end if;
    return new;
end;
$$ language plpgsql security definer;

-- Create trigger on tasks table
create trigger create_task_history_trigger
    after update on tasks
    for each row
    execute function create_task_history();

comment on function create_task_history() is 'Premium feature: Automatically records task completion history';
comment on trigger create_task_history_trigger on tasks is 'Creates history record when task is completed';

-- ============================================================================
-- 3. AUTOMATIC RECURRING TASK CREATION TRIGGER
-- ============================================================================
-- Automatically creates next recurring task when current task is completed
create or replace function create_recurring_task()
returns trigger as $$
declare
    next_due_date date;
    item_intervals record;
begin
    -- Only process if task is marked completed and is recurring
    if new.status = 'completed' and old.status != 'completed' and new.is_recurring = true then
        
        -- Get the interval values from the associated item
        select years_value, months_value, weeks_value, days_value
        into item_intervals
        from items
        where id = new.item_id;
        
        -- Calculate next due date based on completion date
        next_due_date := coalesce(new.completion_date, current_date);
        
        -- Add intervals to calculate next due date
        if item_intervals.years_value > 0 then
            next_due_date := next_due_date + (item_intervals.years_value || ' years')::interval;
        end if;
        
        if item_intervals.months_value > 0 then
            next_due_date := next_due_date + (item_intervals.months_value || ' months')::interval;
        end if;
        
        if item_intervals.weeks_value > 0 then
            next_due_date := next_due_date + (item_intervals.weeks_value || ' weeks')::interval;
        end if;
        
        if item_intervals.days_value > 0 then
            next_due_date := next_due_date + (item_intervals.days_value || ' days')::interval;
        end if;
        
        -- Create the next recurring task
        insert into tasks (
            item_id,
            household_id,
            assigned_to,
            due_date,
            title,
            description,
            priority,
            is_recurring,
            created_by
        )
        values (
            new.item_id,
            new.household_id,
            new.assigned_to,
            next_due_date,
            new.title,
            new.description,
            new.priority,
            true,
            new.created_by
        );
        
        -- Update the item's last_date
        update items 
        set last_date = coalesce(new.completion_date, current_date),
            updated_at = now()
        where id = new.item_id;
        
    end if;
    
    return new;
end;
$$ language plpgsql security definer;

-- Create trigger on tasks table
create trigger create_recurring_task_trigger
    after update on tasks
    for each row
    execute function create_recurring_task();

comment on function create_recurring_task() is 'Automatically creates next recurring task when current task is completed';
comment on trigger create_recurring_task_trigger on tasks is 'Generates recurring tasks based on item maintenance intervals';

-- ============================================================================
-- 4. PLAN USAGE TRACKING TRIGGER
-- ============================================================================
-- Automatically updates plan usage statistics when items/members are added
create or replace function update_plan_usage()
returns trigger as $$
declare
    household_uuid uuid;
    usage_count integer;
begin
    -- Determine household_id based on table
    if tg_table_name = 'items' then
        household_uuid := coalesce(new.household_id, old.household_id);
        
        -- Count current active items for household
        select count(*) into usage_count
        from items
        where household_id = household_uuid 
            and deleted_at is null 
            and is_active = true;
            
        -- Update or insert plan usage record
        insert into plan_usage (household_id, usage_type, current_value, usage_date)
        values (household_uuid, 'items', usage_count, current_date)
        on conflict (household_id, usage_type, usage_date)
        do update set 
            current_value = excluded.current_value,
            updated_at = now();
            
    elsif tg_table_name = 'household_members' then
        household_uuid := coalesce(new.household_id, old.household_id);
        
        -- Count current active members for household
        select count(*) into usage_count
        from household_members
        where household_id = household_uuid 
            and deleted_at is null;
            
        -- Update or insert plan usage record
        insert into plan_usage (household_id, usage_type, current_value, usage_date)
        values (household_uuid, 'household_members', usage_count, current_date)
        on conflict (household_id, usage_type, usage_date)
        do update set 
            current_value = excluded.current_value,
            updated_at = now();
    end if;
    
    return coalesce(new, old);
end;
$$ language plpgsql security definer;

-- Create triggers for plan usage tracking
create trigger update_items_plan_usage_trigger
    after insert or update or delete on items
    for each row
    execute function update_plan_usage();

create trigger update_members_plan_usage_trigger
    after insert or update or delete on household_members
    for each row
    execute function update_plan_usage();

comment on function update_plan_usage() is 'Automatically tracks plan usage statistics for subscription limits';

-- ============================================================================
-- 5. INSERT DEFAULT PLAN TYPES AND CATEGORY DATA
-- ============================================================================
-- Wstawienie domyślnych typów planów dla modelu freemium
insert into plan_types (id, name, description, max_household_members, max_items, price_monthly, price_yearly, features, is_active) values
(1, 'Darmowy', 'Podstawowe zarządzanie gospodarstwem domowym', 3, 5, 0.00, 0.00, '["podstawowe_zadania", "widok_kalendarza"]', true),
(2, 'Premium', 'Pełne zarządzanie gospodarstwem z analityką', 10, 100, 9.99, 99.99, '["podstawowe_zadania", "widok_kalendarza", "powiadomienia_email", "dokumenty", "analityka", "historia", "wsparcie_priorytetowe"]', true),
(3, 'Rodzinny', 'Kompletne rozwiązanie dla rodziny', null, null, 19.99, 199.99, '["podstawowe_zadania", "widok_kalendarza", "powiadomienia_email", "dokumenty", "analityka", "historia", "wsparcie_priorytetowe", "nieograniczeni_czlonkowie", "nieograniczone_przedmioty"]', true)
on conflict (id) do nothing;

-- Wstawienie domyślnych typów kategorii (tylko MVP)
insert into category_types (name, description, sort_order, is_active) values
('Przeglądy techniczne', 'Przeglądy techniczne pojazdów i urządzeń', 1, true),
('Wywóz śmieci', 'Harmonogram wywozu śmieci', 2, true),
('Wizyty medyczne', 'Wizyty zdrowotne członków gospodarstwa', 3, true)
on conflict (name) do nothing;

-- Wstawienie domyślnych kategorii (tylko MVP)
insert into categories (category_type_id, name, description, sort_order, is_active) values
-- Przeglądy techniczne
((select id from category_types where name = 'Przeglądy techniczne'), 'Przegląd samochodu', 'Obowiązkowy przegląd techniczny pojazdu', 1, true),
((select id from category_types where name = 'Przeglądy techniczne'), 'Przegląd kotła', 'Przegląd kotła grzewczego', 2, true),
-- Wywóz śmieci  
((select id from category_types where name = 'Wywóz śmieci'), 'Śmieci zmieszane', 'Wywóz śmieci zmieszanych', 1, true),
((select id from category_types where name = 'Wywóz śmieci'), 'Odpady segregowane', 'Wywóz odpadów segregowanych', 2, true),
-- Wizyty medyczne
((select id from category_types where name = 'Wizyty medyczne'), 'Lekarz rodzinny', 'Wizyty u lekarza pierwszego kontaktu', 1, true),
((select id from category_types where name = 'Wizyty medyczne'), 'Dentysta', 'Wizyty dentystyczne', 2, true),
((select id from category_types where name = 'Wizyty medyczne'), 'Badania kontrolne', 'Regularne badania profilaktyczne', 3, true)
on conflict (category_type_id, name) do nothing;

comment on view dashboard_upcoming_tasks is 'Main dashboard view with all pending tasks and urgency classification';

-- ============================================================================
-- END OF VIEWS AND TRIGGERS MIGRATION
-- ============================================================================


