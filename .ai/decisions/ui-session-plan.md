Podsumowanie Decyzji Architektonicznych UI - Aplikacja Homely MVP
1. Główne Wymagania Dotyczące Architektury UI
Stack Technologiczny
Frontend: Angular 20 z PrimeNG jako główna biblioteka komponentów
Styling: CSS Grid/Flexbox z responsywnymi breakpointami
Integracja: REST API (.NET 8 backend) z Supabase jako bazą danych
Kluczowe Założenia Projektowe
Aplikacja webowa responsywna (desktop, tablet, mobile browser)
Wsparcie dla przeglądarek: Chrome, Firefox, Safari, Edge (2 ostatnie wersje)
Model biznesowy freemium z jasnym rozróżnieniem funkcjonalności
Nacisk na prostotę i intuicyjność interfejsu
2. Kluczowe Widoki, Ekrany i Przepływy Użytkownika
2.1 System Nawigacji
Dynamiczna nawigacja oparta na rolach z Angular guards
Routing z lazy loading dla każdej głównej sekcji
Breadcrumb navigation (PrimeNG Breadcrumb) dla orientacji użytkownika
Context-aware back navigation z consistent action buttons placement
Deep linking dla wszystkich głównych widoków z Angular Router
2.2 Główne Widoki Aplikacji
Dashboard Główny
Lista nadchodzących terminów (7 dni)
Wyróżnienie terminów przekroczonych (czerwony), dzisiejszych (pomarańczowy), przyszłych (zielony)
Szybkie akcje: potwierdź, przełóż, edytuj
Statystyki wykorzystania limitu i liczby urządzeń
Household switcher w header aplikacji dla użytkowników z dostępem do wielu gospodarstw
Widok Kalendarza Miesięcznego
PrimeNG FullCalendar z custom event rendering
Kolorowe oznaczenia kategorii z legend component
Click handlers prowadzące do task details modal
Navigation controls dla poprzedni/następny miesiąc
Licznik terminów w danym dniu (jeśli więcej niż 3)
Lista Urządzeń/Wizyt
EditableDataTable z inline editing (komponent wielokrotnego użytku)
Grupowanie po kategoriach
Sortowanie i filtrowanie (kategoria, osoba odpowiedzialna, priorytet)
ActionButton z permission checking (komponent wielokrotnego użytku)
Widok Dashboard dla Monitora na Ścianie
Uproszczony interfejs z dużą czcionką (minimum 24px)
Tylko najbliższe 5 terminów
Auto-refresh co 5 minut
Wysokie kontrasty kolorystyczne
Responsywność dla różnych rozmiarów monitorów
Brak możliwości edycji (tylko odczyt)
2.3 Role i Uprawnienia w UI
Administrator: Dedykowana zakładka z widokiem gospodarstw domowych, domowników i subskrypcji
Domownik: Dostęp do odczytu i edycji przypisanych pozycji
Dashboard: Tylko widok odczytu z uproszczonym interfejsem
2.4 Proces Onboardingu
Uproszczony do tooltips z podpowiedzią jak dodać gospodarstwo domowe i urządzenie
Brak złożonych stepperów czy guided tours
Możliwość pominięcia onboardingu
Checkbox "Nie pokazuj ponownie"
3. Strategia Integracji z API i Zarządzania Stanem
3.1 Zarządzanie Stanem Autoryzacji
Angular Service z BehaviorSubject do zarządzania stanem auth/household
HTTP interceptory do automatycznego dodawania tokenów JWT
Obsługa wygasłych sesji z automatycznym przekierowaniem do logowania
Wylogowanie ze wszystkich urządzeń jako opcja bezpieczeństwa
3.2 Integracja z REST API
Wykorzystanie wszystkich endpointów zgodnie z planem API
Paginacja dla list (domyślnie 20 elementów, maksymalnie 100)
Filtrowanie i sortowanie po stronie serwera
Soft delete pattern z zachowaniem spójności danych
3.3 Obsługa Błędów API
Globalny error interceptor do przechwytywania błędów HTTP
Toast notification service (PrimeNG Toast) do wyświetlania komunikatów
Loading spinner service do zarządzania stanami ładowania
User-friendly error messages mapping dla różnych HTTP status codes
Retry button dla nieudanych akcji
Graceful degradation przy częściowych problemach
4. Responsywność, Dostępność i Bezpieczeństwo
4.1 Responsywność
PrimeNG DataTable z responsywnymi trybami (stacked, scroll)
CSS Grid/Flexbox breakpoints dla różnych urządzeń
Optymalizacja dla desktop, tablet i mobile browser
Specjalna optymalizacja dla widoku Dashboard (monitor na ścianie)
4.2 Komponenty Wielokrotnego Użytku
EditableDataTable z inline editing
ActionButton z permission checking
ConfirmDialog dla operacji delete (komponent PrimeNG)
DateTimePicker dla terminów
Theme Toggle Component już zaimplementowany
Wszystko oparte na PrimeNG z custom business logic overlay
4.3 Bezpieczeństwo w UI
JWT Bearer Token w nagłówkach Authorization
Role-based access control na poziomie komponentów i routing guards
Angular Guards do ochrony tras przed nieautoryzowanym dostępem
Walidacja po stronie klienta (JavaScript) z duplikacją walidacji po stronie serwera
Sanityzacja danych (ochrona przed XSS)
4.4 Funkcje Premium w UI
Prosty baner z informacją o limitach i linkiem do upgrade
Brak zaawansowanych progress barów czy notyfikacji o limitach
Komunikaty przy osiągnięciu limitów freemium (5 urządzeń, 3 osoby)
Wydzielone sekcje dla funkcji premium (historia, raporty, analizy)
5. Zarządzanie Tematami i Personalizacja
5.1 System Tematów
Theme Service już zaimplementowany (core/services/theme.service.ts)
Theme Toggle Component dostępny (shared/components/theme-toggle/)
Style w _themes.scss i _variables.scss
Wsparcie dla dark mode
5.2 Personalizacja
Możliwość przypisywania kolorów/ikon członkom rodziny
Kolorowe oznaczenia kategorii w kalendarzu
Personalizacja dashboard według preferencji użytkownika
6. Nierozwiązane Kwestie i Obszary Wymagające Dalszego Wyjaśnienia
6.1 Implementacja Funkcji Premium
Szczegółowe specyfikacje dla raportów kosztów i analiz predykcyjnych
Design wizualizacji danych (wykresy, timeline, heatmapa)
Implementacja wykresów Gantta z przeciąganiem terminów
6.2 Integracje Przyszłościowe
Przygotowanie architektury pod przyszłe powiadomienia email
Struktura do zarządzania dokumentacją (upload, podgląd, kategoryzacja)
Extensibility dla dodatkowych kategorii (rośliny, ubezpieczenia, zwierzęta)
6.3 Optymalizacja Wydajności
Strategia cache'owania dla często używanych danych
Lazy loading dla komponentów premium
Optymalizacja zapytań API (batch requests, debouncing)
6.4 Dostępność (A11y)
Implementacja ARIA labels dla komponentów custom
Keyboard navigation dla wszystkich interakcji
Screen reader compatibility
Color contrast compliance (WCAG 2.1)
6.5 Testowanie
Strategia testów jednostkowych dla komponentów Angular
E2E testy dla kluczowych przepływów użytkownika
Testy responsywności na różnych urządzeniach
Testy bezpieczeństwa (XSS, CSRF protection)
7. Następne Kroki w Implementacji
Setup projektu Angular 20 z PrimeNG i podstawową strukturą
Implementacja systemu autoryzacji z JWT i role-based guards
Stworzenie komponentów wielokrotnego użytku (EditableDataTable, ActionButton, etc.)
Implementacja głównych widoków (Dashboard, Calendar, Items List)
Integracja z REST API według established patterns
Implementacja widoku Dashboard dla monitora
Testowanie responsywności i dostępności
Implementacja funkcji premium (historia, raporty)
To podsumowanie stanowi kompletną podstawę do rozpoczęcia implementacji architektury UI dla aplikacji Homely MVP, z jasnym rozdziałem odpowiedzialności między komponenty i serwisy Angular.