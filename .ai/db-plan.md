# Schemat Bazy Danych PostgreSQL - Homely MVP

## 1. Lista tabel z kolumnami, typami danych i ograniczeniami

### 1.1. Tabela autentykacji (zarządzana przez Supabase)

#### auth.users
Tabela użytkowników automatycznie tworzona i zarządzana przez Supabase Auth.
**UWAGA: Ta tabela jest tworzona automatycznie przez Supabase - nie tworzymy jej ręcznie.**

```sql
 Struktura tabeli auth.users (zarządzana przez Supabase)
 CREATE TABLE auth.users (
     id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
     instance_id UUID,
     aud VARCHAR(255),
     role VARCHAR(255),
     email VARCHAR(255) UNIQUE,
     encrypted_password VARCHAR(255),
     email_confirmed_at TIMESTAMP WITH TIME ZONE,
     invited_at TIMESTAMP WITH TIME ZONE,
     confirmation_token VARCHAR(255),
     confirmation_sent_at TIMESTAMP WITH TIME ZONE,
     recovery_token VARCHAR(255),
     recovery_sent_at TIMESTAMP WITH TIME ZONE,
     email_change_token_new VARCHAR(255),
     email_change VARCHAR(255),
     email_change_sent_at TIMESTAMP WITH TIME ZONE,
     last_sign_in_at TIMESTAMP WITH TIME ZONE,
     raw_app_meta_data JSONB,
     raw_user_meta_data JSONB,
     is_super_admin BOOLEAN,
     created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
     updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
     phone TEXT UNIQUE,
     phone_confirmed_at TIMESTAMP WITH TIME ZONE,
     phone_change TEXT,
     phone_change_token VARCHAR(255),
     phone_change_sent_at TIMESTAMP WITH TIME ZONE,
     confirmed_at TIMESTAMP WITH TIME ZONE GENERATED ALWAYS AS (LEAST(email_confirmed_at, phone_confirmed_at)) STORED,
     email_change_token_current VARCHAR(255),
     email_change_confirm_status SMALLINT,
     banned_until TIMESTAMP WITH TIME ZONE,
     reauthentication_token VARCHAR(255),
     reauthentication_sent_at TIMESTAMP WITH TIME ZONE,
     is_sso_user BOOLEAN DEFAULT FALSE,
     deleted_at TIMESTAMP WITH TIME ZONE
 );
```

### 1.2. Tabele główne (Core Tables)

#### user_profiles
Nadbudowa nad Supabase auth.users z dodatkowymi danymi użytkownika.

```sql
CREATE TABLE user_profiles (
    id UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    avatar_url TEXT,
    last_active_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE
);
```

#### plan_types
Typy planów subskrypcyjnych.

```sql
CREATE TABLE plan_types (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    max_household_members INTEGER,
    max_items INTEGER,
    max_storage_mb INTEGER,
    price_monthly DECIMAL(10,2),
    price_yearly DECIMAL(10,2),
    features JSONB DEFAULT '[]',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE
);
```

#### households
Gospodarstwa domowe z typem planu.

```sql
CREATE TABLE households (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    address TEXT,
    plan_type_id INTEGER REFERENCES plan_types(id) DEFAULT 1,
    subscription_status VARCHAR(20) DEFAULT 'free' CHECK (subscription_status IN ('free', 'active', 'cancelled', 'expired')),
    subscription_start_date DATE,
    subscription_end_date DATE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE
);
```

#### household_members
Tabela łącznikowa użytkownik-gospodarstwo z rolami.

```sql
CREATE TABLE household_members (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    household_id UUID REFERENCES households(id) ON DELETE CASCADE,
    user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
    role VARCHAR(20) NOT NULL CHECK (role IN ('admin', 'member', 'dashboard')),
    invited_by UUID REFERENCES auth.users(id),
    invitation_token VARCHAR(100) UNIQUE,
    invitation_expires_at TIMESTAMP WITH TIME ZONE,
    joined_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE
);
```

#### category_types
Typy kategorii (przeglądy techniczne, wywóz śmieci, wizyty medyczne).

```sql
CREATE TABLE category_types (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    sort_order INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE
);
```

#### categories
Kategorie z przypisaniem do typu.

```sql
CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    category_type_id INTEGER REFERENCES category_types(id),
    name VARCHAR(100) NOT NULL,
    description TEXT,
    sort_order INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE
);
```

#### items
Urządzenia i wizyty w gospodarstwie.

```sql
CREATE TABLE items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    household_id UUID REFERENCES households(id) ON DELETE CASCADE,
    category_id INTEGER REFERENCES categories(id),
    name VARCHAR(100) NOT NULL,
    description TEXT,
    years_value INTEGER,
    months_value INTEGER,
    weeks_value INTEGER,
    days_value INTEGER,
    last_date DATE,
    priority VARCHAR(10) DEFAULT 'medium' CHECK (priority IN ('low', 'medium', 'high')),
    notes TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_by UUID REFERENCES auth.users(id) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE
);
```

#### tasks
Terminy/spotkania jako osobne rekordy.

```sql
CREATE TABLE tasks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    item_id UUID REFERENCES items(id) ON DELETE CASCADE,
    household_id UUID REFERENCES households(id) ON DELETE CASCADE,
    assigned_to UUID REFERENCES auth.users(id),
    due_date DATE NOT NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending', 'completed', 'postponed', 'cancelled')),
    priority VARCHAR(10) DEFAULT 'medium' CHECK (priority IN ('low', 'medium', 'high')),
    completion_date DATE,
    completion_notes TEXT,
    postponed_from_date DATE,
    postpone_reason TEXT,
    is_recurring BOOLEAN DEFAULT TRUE,
    created_by UUID REFERENCES auth.users(id) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE
);
```

### 1.3. Tabele historii i śledzenia

#### tasks_history
Historia wykonanych terminów (premium).

```sql
CREATE TABLE tasks_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_id UUID REFERENCES tasks(id) ON DELETE CASCADE,
    item_id UUID REFERENCES items(id) ON DELETE CASCADE,
    household_id UUID REFERENCES households(id) ON DELETE CASCADE,
    assigned_to UUID REFERENCES auth.users(id),
    completed_by UUID REFERENCES auth.users(id),
    due_date DATE NOT NULL,
    completion_date DATE NOT NULL,
    title VARCHAR(200) NOT NULL,
    completion_notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE
);
```

#### plan_usage
Śledzenie wykorzystania limitów planów.

```sql
CREATE TABLE plan_usage (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    household_id UUID REFERENCES households(id) ON DELETE CASCADE,
    usage_type VARCHAR(50) NOT NULL,
    current_value INTEGER DEFAULT 0,
    max_value INTEGER,
    usage_date DATE DEFAULT CURRENT_DATE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(household_id, usage_type, usage_date)
);
```


## 2. Relacje między tabelami

### Relacje jeden-do-wielu (1:N)
- `plan_types` → `households` (1:N)
- `auth.users` → `user_profiles` (1:1)
- `auth.users` → `household_members` (1:N)
- `households` → `household_members` (1:N)
- `households` → `items` (1:N)
- `households` → `tasks` (1:N)
- `category_types` → `categories` (1:N)
- `categories` → `items` (1:N)
- `items` → `tasks` (1:N)

### Relacje wiele-do-wielu (M:N)
- `auth.users` ↔ `households` (przez `household_members`)

## 3. Indeksy

### Indeksy wydajnościowe dla dashboard queries

```sql
-- User profiles
CREATE INDEX idx_user_profiles_deleted_at ON user_profiles(deleted_at);
CREATE INDEX idx_user_profiles_last_active ON user_profiles(last_active_at) WHERE deleted_at IS NULL;

-- Households
CREATE INDEX idx_households_plan_type ON households(plan_type_id);
CREATE INDEX idx_households_deleted_at ON households(deleted_at);

-- Plan types
CREATE INDEX idx_plan_types_active ON plan_types(is_active);
CREATE INDEX idx_plan_types_deleted_at ON plan_types(deleted_at);

-- Household members
CREATE INDEX idx_household_members_household ON household_members(household_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_household_members_user ON household_members(user_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_household_members_role ON household_members(household_id, role) WHERE deleted_at IS NULL;
CREATE UNIQUE INDEX idx_household_members_unique_active ON household_members(household_id, user_id) WHERE deleted_at IS NULL;

-- Categories
CREATE INDEX idx_categories_type ON categories(category_type_id);
CREATE INDEX idx_categories_active ON categories(is_active) WHERE deleted_at IS NULL;
CREATE INDEX idx_categories_deleted_at ON categories(deleted_at);
CREATE UNIQUE INDEX idx_categories_unique_name ON categories(category_type_id, name) WHERE deleted_at IS NULL;

-- Category types
CREATE INDEX idx_category_types_active ON category_types(is_active);
CREATE INDEX idx_category_types_sort_order ON category_types(sort_order);
CREATE INDEX idx_category_types_deleted_at ON category_types(deleted_at);

-- Items
CREATE INDEX idx_items_household ON items(household_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_items_category ON items(category_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_items_created_by ON items(created_by) WHERE deleted_at IS NULL;
CREATE INDEX idx_items_active ON items(is_active) WHERE deleted_at IS NULL;

-- Tasks - kluczowe dla dashboard
CREATE INDEX idx_tasks_due_date ON tasks(due_date) WHERE deleted_at IS NULL;
CREATE INDEX idx_tasks_household_status ON tasks(household_id, status) WHERE deleted_at IS NULL;
CREATE INDEX idx_tasks_assigned_to ON tasks(assigned_to) WHERE deleted_at IS NULL;
CREATE INDEX idx_tasks_status_due ON tasks(status, due_date) WHERE deleted_at IS NULL;
CREATE INDEX idx_tasks_item ON tasks(item_id) WHERE deleted_at IS NULL;

-- Tasks history
CREATE INDEX idx_tasks_history_household ON tasks_history(household_id);
CREATE INDEX idx_tasks_history_completion_date ON tasks_history(completion_date);
CREATE INDEX idx_tasks_history_item ON tasks_history(item_id);

-- Plan usage
CREATE INDEX idx_plan_usage_household_type ON plan_usage(household_id, usage_type);
CREATE INDEX idx_plan_usage_date ON plan_usage(usage_date);
```

## 4. Zasady PostgreSQL (RLS - Row Level Security)

### Podstawowe zasady bezpieczeństwa

```sql
-- Enable RLS na głównych tabelach
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE households ENABLE ROW LEVEL SECURITY;
ALTER TABLE household_members ENABLE ROW LEVEL SECURITY;
ALTER TABLE items ENABLE ROW LEVEL SECURITY;
ALTER TABLE tasks ENABLE ROW LEVEL SECURITY;
ALTER TABLE tasks_history ENABLE ROW LEVEL SECURITY;

-- User profiles - użytkownik może edytować tylko swój profil
CREATE POLICY "Users can view and edit their own profile" ON user_profiles
    FOR ALL USING (auth.uid() = id);

-- Households - dostęp tylko dla członków gospodarstwa
CREATE POLICY "Household members can view household" ON households
    FOR SELECT USING (
        id IN (
            SELECT household_id FROM household_members 
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

CREATE POLICY "Household admins can update household" ON households
    FOR UPDATE USING (
        id IN (
            SELECT household_id FROM household_members 
            WHERE user_id = auth.uid() AND role = 'admin' AND deleted_at IS NULL
        )
    );

-- Household members - członkowie mogą widzieć innych członków
CREATE POLICY "Household members can view members" ON household_members
    FOR SELECT USING (
        household_id IN (
            SELECT household_id FROM household_members 
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

-- Items - dostęp dla członków gospodarstwa
CREATE POLICY "Household members can view items" ON items
    FOR SELECT USING (
        household_id IN (
            SELECT household_id FROM household_members 
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

CREATE POLICY "Household members can manage items" ON items
    FOR ALL USING (
        household_id IN (
            SELECT household_id FROM household_members 
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

-- Tasks - dostęp dla członków gospodarstwa
CREATE POLICY "Household members can view tasks" ON tasks
    FOR SELECT USING (
        household_id IN (
            SELECT household_id FROM household_members 
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );

CREATE POLICY "Household members can manage tasks" ON tasks
    FOR ALL USING (
        household_id IN (
            SELECT household_id FROM household_members 
            WHERE user_id = auth.uid() AND deleted_at IS NULL
        )
    );
```

## 5. Views

### View dla dashboard z nadchodzącymi terminami

```sql
CREATE VIEW dashboard_upcoming_tasks AS
SELECT 
    t.id,
    t.due_date,
    t.title,
    t.status,
    t.priority,
    i.name as item_name,
    c.name as category_name,
    ct.name as category_type_name,
    up.first_name as assigned_to_first_name,
    up.last_name as assigned_to_last_name,
    h.id as household_id,
    CASE 
        WHEN t.due_date < CURRENT_DATE THEN 'overdue'
        WHEN t.due_date = CURRENT_DATE THEN 'today'
        ELSE 'upcoming'
    END as urgency_status
FROM tasks t
JOIN items i ON t.item_id = i.id
JOIN categories c ON i.category_id = c.id
JOIN category_types ct ON c.category_type_id = ct.id
JOIN households h ON t.household_id = h.id
LEFT JOIN user_profiles up ON t.assigned_to = up.id
WHERE t.deleted_at IS NULL 
    AND t.status = 'pending'
    AND i.deleted_at IS NULL
    AND h.deleted_at IS NULL
ORDER BY t.due_date ASC, t.priority DESC;
```

## 6. Dodatkowe uwagi i wyjaśnienia decyzji projektowych

### 6.1. Wykorzystanie Supabase Authentication

**Ważne:** W Supabase tabela `auth.users` jest automatycznie tworzona i zarządzana przez system autentykacji. **Nie tworzymy jej ręcznie** - Supabase robi to za nas podczas inicjalizacji projektu.

#### Jak działa integracja:
1. **Rejestracja/Logowanie**: Supabase automatycznie zarządza procesem autentykacji i tworzy rekordy w `auth.users`
2. **Rozszerzenie danych**: Tabela `user_profiles` łączy się z `auth.users` poprzez `id` i przechowuje dodatkowe dane użytkownika
3. **Bezpieczeństwo**: Funkcja `auth.uid()` zwraca ID zalogowanego użytkownika dla RLS policies
4. **Automatyzacja**: Triggery mogą automatycznie tworzyć profil użytkownika po rejestracji:

```sql
-- Trigger do automatycznego tworzenia profilu użytkownika
CREATE OR REPLACE FUNCTION create_user_profile()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO user_profiles (id, first_name, last_name)
    VALUES (
        NEW.id, 
        COALESCE(NEW.raw_user_meta_data->>'first_name', 'User'),
        COALESCE(NEW.raw_user_meta_data->>'last_name', '')
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER create_user_profile_trigger
    AFTER INSERT ON auth.users
    FOR EACH ROW
    EXECUTE FUNCTION create_user_profile();
```

- Schemat wykorzystuje wbudowany system autentykacji Supabase (`auth.users`)
- Tabela `user_profiles` stanowi rozszerzenie danych użytkownika
- RLS automatycznie korzysta z `auth.uid()` do identyfikacji zalogowanego użytkownika

### 6.2. Soft Delete Pattern
- Wszystkie główne tabele implementują soft delete poprzez kolumnę `deleted_at`
- Indeksy uwzględniają warunek `WHERE deleted_at IS NULL` dla wydajności
- Views automatycznie filtrują usunięte rekordy

### 6.3. Model Freemium
- Tabela `plan_types` definiuje limity dla różnych planów
- Tabela `plan_usage` śledzi aktualne wykorzystanie limitów
- Funkcje premium są dostępne poprzez warunki w aplikacji

### 6.4. Wydajność
- Strategiczne indeksy dla najczęstszych zapytań dashboard
- Composite indeksy dla złożonych filtrów
- View `dashboard_upcoming_tasks` optymalizuje główne zapytania

### 6.5. Bezpieczeństwo
- Row Level Security zapewnia izolację danych między gospodarstwami
- Granularne uprawnienia oparte na rolach w gospodarstwie
- Automatic policy enforcement na poziomie bazy danych

### 6.6. Skalowalność
- UUID jako klucze główne zapewniają globalną unikalność
- JSONB dla elastycznych danych (features w plan_types)
- Trigger-based automation dla rutynowych operacji
