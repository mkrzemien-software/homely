Podsumowanie podjętych decyzji architektonicznych UI
1. Nawigacja główna aplikacji:
Dynamiczna nawigacja oparta na rolach z Angular guards
Routing z lazy loading dla każdej sekcji
Administrator otrzymuje dedykowaną zakładkę z widokiem gospodarstw domowych, domowników i subskrypcji
2. Responsywność aplikacji:
Wykorzystanie PrimeNG DataTable z responsywnymi trybami (stacked, scroll)
CSS Grid/Flexbox breakpoints dla różnych urządzeń
3. Proces onboardingu:
Uproszczony do tooltips z podpowiedzią jak dodać gospodarstwo domowe i urządzenie
Brak złożonych stepperów czy guided tours
4. Specjalny widok Dashboard dla monitora:
Nie implementować dodatkowych funkcjonalności dla tego przypadku użycia
5. Zarządzanie stanem autoryzacji:
Angular Service z BehaviorSubject do zarządzania stanem auth/household
Household switcher w header aplikacji
HTTP interceptory do automatycznego dodawania tokenów i obsługi wygasłych sesji
6. Prezentacja limitów freemium:
Prosty baner z informacją o limitach i linkiem do upgrade
Brak zaawansowanych progress barów czy notyfikacji
7. Komponenty wielokrotnego użytku:
EditableDataTable z inline editing
ActionButton z permission checking
ConfirmDialog dla operacji delete
DateTimePicker dla terminów
Wszystko oparte na PrimeNG z custom business logic overlay
8. Obsługa błędów API:
Globalny error interceptor
Toast notification service (PrimeNG Toast)
Loading spinner service
User-friendly error messages mapping dla HTTP status codes
9. Nawigacja między widokami:
Breadcrumb navigation (PrimeNG Breadcrumb)
Consistent action buttons placement
Context-aware back navigation
Deep linking dla wszystkich głównych widoków z Angular Router
10. Widok kalendarza miesięcznego:
PrimeNG FullCalendar z custom event rendering
Kolorowe oznaczenia kategorii
Click handlers prowadzące do task details modal
Legend component dla kategorii
Navigation controls dla poprzedni/następny miesiąc