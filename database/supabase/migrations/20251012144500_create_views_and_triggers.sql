-- Migration: Create Views and Insert Default Data
-- Description: Creates optimized views for dashboard queries and inserts default plan types and categories
-- Objects: dashboard_upcoming_tasks view, plan types, category types, categories
-- Author: Homely MVP Database Schema
-- Date: 2025-10-12
-- Updated: 2025-11-26 - Removed database triggers, business logic moved to .NET code

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
-- BUSINESS LOGIC NOW HANDLED IN .NET CODE
-- ============================================================================
-- All business logic previously handled by database triggers is now implemented
-- in the .NET application layer for better control, testability, and maintainability:
--
-- 1. Event History Creation (Premium Feature):
--    - Handled in EventService.CompleteEventAsync()
--    - Creates tasks_history entry only for premium households
--
-- 2. Recurring Event Creation:
--    - Handled in EventService.CompleteEventAsync()
--    - Creates next event based on task template interval
--
-- 3. Plan Usage Tracking:
--    - Handled in PlanUsageService
--    - UpdateTasksUsageAsync() tracks task count
--    - UpdateMembersUsageAsync() tracks member count
--    - Integrated in TaskService and HouseholdMemberService

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
-- END OF VIEWS MIGRATION
-- ============================================================================


