# Schemat Bazy Danych PostgreSQL - Homely MVP

## Struktura implementacji

### 1. Tabele główne (Core Tables)

- `user_profiles` - dodatkowe dane użytkownika (nadbudowa nad Supabase auth.users)
- `households` - gospodarstwa domowe z typem planu
- `household_members` - tabela łącznikowa użytkownik-gospodarstwo z rolami
- `category_types` - typy kategorii (przeglądy techniczne, wywóz śmieci, wizyty medyczne)
- `categories` - kategorie z przypisaniem do typu
- `items` - urządzenia i wizyty w gospodarstwie
- `tasks` - terminy/spotkania jako osobne rekordy

### 2. Tabele historii i śledzenia

- `tasks_history` - historia wykonanych terminów (premium)
- `plan_usage` - śledzenie wykorzystania limitów planów

### 3. Indeksy wydajnościowe

- Indeksy dla dashboard queries
- Indeksy dla filtrowania i sortowania

### 6. Views i funkcje pomocnicze

- View dla dashboard z nadchodzącymi terminami
- Funkcje do obsługi soft delete

## Kluczowe cechy implementacji

- Wykorzystanie Supabase auth.users jako podstawa autentykacji
- Soft delete z polem `deleted_at`
- Interwały jako wartości liczbowe (years, months, weeks, days)
- Śledzenie limitów planu free/premium