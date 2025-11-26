-- Migration: Create Views and Triggers
-- Description: Creates optimized views for dashboard queries and automated triggers for business logic
-- Objects: dashboard_upcoming_tasks view, user profile creation trigger, plan data insertion
-- Author: Homely MVP Database Schema
-- Date: 2025-10-12

-- ============================================================================
-- DASHBOARD VIEWS FOR OPTIMIZED QUERIES
-- ============================================================================

-- ============================================================================
-- 1. DASHBOARD UPCOMING EVENTS VIEW
-- ============================================================================
-- Optimized view for dashboard showing upcoming events with urgency classification
create view dashboard_upcoming_tasks as
select
    e.id,
    e.due_date,
    t.name as title,
    t.description,
    e.status,
    e.priority,
    e.assigned_to,
    t.name as task_name,
    t.description as task_description,
    c.name as category_name,
    ct.name as category_type_name,
    -- User names would need to be fetched from auth.users if needed
    h.id as household_id,
    h.name as household_name,
    -- Urgency classification for dashboard prioritization
    case
        when e.due_date < current_date then 'overdue'
        when e.due_date = current_date then 'today'
        when e.due_date <= current_date + interval '7 days' then 'this_week'
        when e.due_date <= current_date + interval '30 days' then 'this_month'
        else 'upcoming'
    end as urgency_status,
    -- Days until due (negative for overdue)
    e.due_date - current_date as days_until_due,
    -- Combined priority score for sorting (urgency + priority)
    case
        when e.due_date < current_date then 1000 + case e.priority when 'high' then 3 when 'medium' then 2 else 1 end
        when e.due_date = current_date then 500 + case e.priority when 'high' then 3 when 'medium' then 2 else 1 end
        when e.due_date <= current_date + interval '7 days' then 100 + case e.priority when 'high' then 3 when 'medium' then 2 else 1 end
        else case e.priority when 'high' then 3 when 'medium' then 2 else 1 end
    end as priority_score
from events e
left join tasks t on e.task_id = t.id
left join categories c on t.category_id = c.id
left join category_types ct on c.category_type_id = ct.id
join households h on e.household_id = h.id
-- User data can be fetched from auth.users if needed
where e.deleted_at is null
    and e.status = 'pending'
    and (t.id is null or (t.deleted_at is null and t.is_active = true))
    and h.deleted_at is null
order by priority_score desc, e.due_date asc, e.priority desc;

comment on view dashboard_upcoming_tasks is 'Optimized dashboard view showing pending events with urgency classification and priority scoring';

-- ============================================================================
-- AUTOMATED TRIGGERS FOR BUSINESS LOGIC
-- ============================================================================

-- ============================================================================
-- 2. AUTOMATIC EVENT HISTORY CREATION TRIGGER
-- ============================================================================
-- Automatically creates history record when event is completed (premium feature)
create or replace function create_event_history()
returns trigger as $$
declare
    task_name_var varchar(100);
begin
    -- Only create history when event is marked as completed
    if new.status = 'completed' and old.status != 'completed' then
        -- Get task name if event is linked to a task
        if new.task_id is not null then
            select name into task_name_var from tasks where id = new.task_id;
        else
            task_name_var := 'One-off event';
        end if;

        insert into tasks_history (
            event_id,
            task_id,
            household_id,
            assigned_to,
            completed_by,
            due_date,
            completion_date,
            task_name,
            completion_notes
        )
        values (
            new.id,
            new.task_id,
            new.household_id,
            new.assigned_to,
            auth.uid(), -- user who marked it complete
            new.due_date,
            coalesce(new.completion_date, current_date),
            task_name_var,
            new.completion_notes
        );
    end if;
    return new;
end;
$$ language plpgsql security definer;

-- Create trigger on events table
create trigger create_event_history_trigger
    after update on events
    for each row
    execute function create_event_history();

comment on function create_event_history() is 'Premium feature: Automatically records event completion history';
comment on trigger create_event_history_trigger on events is 'Creates history record when event is completed';

-- ============================================================================
-- 3. AUTOMATIC RECURRING EVENT CREATION
-- ============================================================================
-- NOTE: Recurring event creation is handled in application code, not via database trigger
-- This ensures better control, error handling, and business logic flexibility

-- ============================================================================
-- 4. PLAN USAGE TRACKING TRIGGER
-- ============================================================================
-- Automatically updates plan usage statistics when tasks/members are added
create or replace function update_plan_usage()
returns trigger as $$
declare
    household_uuid uuid;
    usage_count integer;
begin
    -- Determine household_id based on table
    if tg_table_name = 'tasks' then
        household_uuid := coalesce(new.household_id, old.household_id);

        -- Count current active tasks for household
        select count(*) into usage_count
        from tasks
        where household_id = household_uuid
            and deleted_at is null
            and is_active = true;

        -- Update or insert plan usage record
        insert into plan_usage (household_id, usage_type, current_value, usage_date)
        values (household_uuid, 'tasks', usage_count, current_date)
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
create trigger update_tasks_plan_usage_trigger
    after insert or update or delete on tasks
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
insert into plan_types (id, name, description, max_household_members, max_tasks, price_monthly, price_yearly, features, is_active) values
(1, 'Darmowy', 'Podstawowe zarządzanie gospodarstwem domowym', 3, 5, 0.00, 0.00, '["podstawowe_zadania", "widok_kalendarza"]', true),
(2, 'Premium', 'Pełne zarządzanie gospodarstwem z analityką', 10, 100, 9.99, 99.99, '["podstawowe_zadania", "widok_kalendarza", "powiadomienia_email", "dokumenty", "analityka", "historia", "wsparcie_priorytetowe"]', true),
(3, 'Rodzinny', 'Kompletne rozwiązanie dla rodziny', null, null, 19.99, 199.99, '["podstawowe_zadania", "widok_kalendarza", "powiadomienia_email", "dokumenty", "analityka", "historia", "wsparcie_priorytetowe", "nieograniczeni_czlonkowie", "nieograniczone_zadania"]', true)
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

comment on view dashboard_upcoming_tasks is 'Main dashboard view with all pending events and urgency classification';

-- ============================================================================
-- END OF VIEWS AND TRIGGERS MIGRATION
-- ============================================================================


