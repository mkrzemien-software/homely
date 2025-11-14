# Dokument wymagań produktu (PRD) - Homely

## 1. Przegląd produktu

### 1.1 Nazwa produktu
Aplikacja webowa do zarządzania terminami serwisów i wizyt

### 1.2 Wizja produktu
Stworzenie centralnego systemu webowego, który umożliwi domownikom efektywne zarządzanie terminami serwisów urządzeń domowych oraz wizytami dla całej rodziny, eliminując problem przegapionych terminów i nieefektywnego zarządzania obowiązkami domowymi.

### 1.3 Grupa docelowa
Domownicy zarządzający gospodarstwem domowym, którzy potrzebują uporządkować i monitorować różnorodne terminy związane z utrzymaniem domu oraz wizytami członków rodziny.

### 1.4 Cele biznesowe
- Zbudowanie bazy aktywnych użytkowników poprzez model freemium
- Osiągnięcie 30% retention rate po 3 miesiącach od rejestracji
- Konwersja minimum 5% użytkowników darmowych na subskrypcję premium w pierwszym roku
- Stworzenie skalowalnej platformy gotowej do rozszerzenia o dodatkowe kategorie

### 1.5 Zakres MVP
Pierwsza wersja produktu obejmuje trzy podstawowe kategorie:
- Przeglądy techniczne
- Wywóz śmieci
- Wizyty medyczne domowników

## 2. Problem użytkownika

### 2.1 Opis problemu
Domownicy borykają się z chaotycznym zarządzaniem wieloma terminami związanymi z utrzymaniem gospodarstwa domowego. Brak centralnego systemu prowadzi do:
- Przegapionych terminów wymiany filtrów, przeglądów technicznych i wizyt medycznych
- Zagubionej dokumentacji (faktury, instrukcje, gwarancje)
- Braku przejrzystości, kto w gospodarstwie jest odpowiedzialny za które zadania
- Trudności w koordynacji obowiązków między członkami rodziny
- Stresu i potencjalnych konsekwencji zdrowotnych lub finansowych wynikających z zaniedbanych terminów

### 2.2 Obecne rozwiązania i ich ograniczenia
Użytkownicy obecnie korzystają z:
- Notatek papierowych - łatwo gubią się, brak powiadomień
- Kalendarzy ogólnego przeznaczenia - nie są dostosowane do specyfiki gospodarstwa domowego
- Aplikacji przypominających - brak kontekstu, historii i dokumentacji
- Pamięci - zawodna, szczególnie przy wielu równoczesnych obowiązkach

### 2.3 Wartość dla użytkownika
Aplikacja rozwiązuje te problemy poprzez:
- Centralizację wszystkich terminów w jednym miejscu
- Jasny podział odpowiedzialności między domownikami
- Możliwość monitorowania historii serwisów i kosztów

## 3. Wymagania funkcjonalne

### 3.1 System użytkowników i gospodarstw domowych

#### 3.1.1 Rejestracja i uwierzytelnianie
- Rejestracja użytkownika przez email i hasło
- Logowanie z walidacją danych
- Możliwość resetowania hasła
- Zgodność z RODO przy zbieraniu danych osobowych

#### 3.1.2 Role i uprawnienia

System Developer (Super Admin):
- Zarządzanie wszystkimi gospodarstwami w systemie
- Tworzenie nowych gospodarstw i przypisywanie administratorów
- Dostęp do globalnych statystyk i metryk platformy
- Zarządzanie subskrypcjami wszystkich użytkowników
- Administracja systemowa i wsparcie techniczne
- Dostęp do logów systemowych i diagnostyki
- Zarządzanie kategoriami globalnymi i ustawieniami platformy

Administrator:
- Tworzenie i zarządzanie gospodarstwem domowym
- Dodawanie i usuwanie członków gospodarstwa
- Zarządzanie wszystkimi zadaniami i wydarzeniami
- Zarządzanie subskrypcją
- Pełny dostęp do dokumentacji

Domownik:
- Odczyt wszystkich zadań i wydarzeń w gospodarstwie
- Edycja i zarządzanie zadaniami przypisanymi do siebie
- Tworzenie wydarzeń dla zadań
- Upload dokumentacji dla przypisanych pozycji
- Potwierdzanie i przełożenie własnych wydarzeń

Dashboard (tylko odczyt):
- Widok terminów bez możliwości edycji
- Optymalizacja dla monitora na ścianie
- Uproszczony interfejs z kluczowymi informacjami

#### 3.1.3 Zarządzanie gospodarstwem
- Limit wersji darmowej: 3 osoby w gospodarstwie
- Brak limitu w wersji premium
- Możliwość przypisywania kolorów/ikon członkom rodziny dla lepszej wizualizacji

### 3.2 Zarządzanie zadaniami i wydarzeniami

#### 3.2.1 Zadania (Tasks) - Szablony
Zadanie to szablon definiujący powtarzalną aktywność powiązaną z podkategorią:
- Nazwa zadania (np. "Serwis", "Przegląd")
- Podkategoria (np. "Toyota" w kategorii "Samochody")
- Interwał czasowy (lata/miesiące/tygodnie/dni) - opcjonalny
- Priorytet (niski/średni/wysoki)
- Notatki dodatkowe

**Workflow Zadań:**
- Zadanie definiuje "co" i "jak często" (jeśli ma interwał)
- Jedno zadanie może generować wiele wydarzeń w czasie
- Przykład: Zadanie "Serwis" dla podkategorii "Toyota" z interwałem 12 miesięcy

#### 3.2.2 Wydarzenia (Events) - Konkretne wystąpienia
Wydarzenie to konkretne zaplanowane wystąpienie zadania z przypisaną datą:
- Referencja do zadania (taskId)
- Przypisanie do członka gospodarstwa (odpowiedzialny)
- Data terminu (dueDate)
- Status (pending/completed/postponed/cancelled)
- Notatki o wykonaniu
- Data wykonania (completionDate)

**Workflow Wydarzeń:**
- Wydarzenia tworzone są **ręcznie** przez użytkownika dla konkretnego zadania
- Pierwsze wydarzenie dla zadania użytkownik tworzy sam
- Gdy wydarzenie jest oznaczone jako "completed" i zadanie ma interwał → system **automatycznie** generuje następne wydarzenie
- Jeśli zadanie nie ma interwału (one-time), nie generuje kolejnych wydarzeń

#### 3.2.3 Edycja i usuwanie
**Zadania:**
- Możliwość modyfikacji wszystkich pól zadania
- Zmiana interwału wpływa tylko na przyszłe wydarzenia
- Opcja archiwizacji zamiast usunięcia (zachowanie historii)
- Potwierdzenie przed usunięciem (ostrzeżenie o powiązanych wydarzeniach)

**Wydarzenia:**
- Możliwość edycji daty, osoby odpowiedzialnej, notatek
- Możliwość anulowania pojedynczego wydarzenia
- Nie wpływa na szablon zadania

#### 3.2.4 Limity wersji darmowej
- Maksymalnie 5 zadań (Tasks) łącznie
- Brak limitu na liczbę wydarzeń (Events)
- Komunikat o limicie przy próbie dodania kolejnego zadania
- Propozycja upgrade'u do wersji premium

### 3.3 System wydarzeń

#### 3.3.1 Tworzenie wydarzeń
- Użytkownik ręcznie tworzy pierwsze wydarzenie dla zadania
- Wybór daty terminu (dueDate)
- Przypisanie do członka gospodarstwa
- Dziedziczenie priorytetu z zadania (możliwość zmiany)

#### 3.3.2 Automatyczne generowanie kolejnych wydarzeń
- Gdy wydarzenie jest oznaczone jako "completed" i zadanie ma interwał
- System automatycznie tworzy następne wydarzenie
- Data następnego wydarzenia = completionDate + interval
- Zachowanie przypisania do tej samej osoby (możliwość zmiany)

#### 3.3.3 Potwierdzanie wykonania
- Możliwość potwierdzenia wykonania wydarzenia
- Opcjonalne dodanie notatki o wykonaniu
- Upload zdjęcia/dokumentu potwierdzającego
- Automatyczne wygenerowanie następnego wydarzenia (jeśli zadanie ma interwał)

#### 3.3.4 Przełożenie wydarzenia
- Możliwość przesunięcia daty wydarzenia o określoną liczbę dni
- Wymagane uzasadnienie/notatka
- Historia przełożeń (dostępna w premium)
- Nie wpływa na szablon zadania

#### 3.3.5 Anulowanie wydarzenia
- Możliwość anulowania pojedynczego wydarzenia
- Zachowanie zadania w systemie
- Nie generuje automatycznie nowego wydarzenia
- Historia anulowanych wydarzeń (premium)

### 3.4 Widoki i nawigacja

#### 3.4.0 Główne menu nawigacyjne (Sidebar)
Aplikacja wykorzystuje wysuwane menu z lewej strony, które zawiera:

**Sekcja 1: Widoki Gospodarstwa** (dostępne dla wszystkich użytkowników w kontekście aktualnie otwartego gospodarstwa):
- 📊 Dashboard - główny widok z kafelkami nawigacyjnymi (Kalendarz, Wydarzenia, Zadania, Kategorie) + interaktywny kalendarz tygodniowy + lista wydarzeń
- 📅 Kalendarz - widok miesięczny kalendarz z zaznaczonymi wydarzeniami (Admin, Domownik)
- 📋 Wydarzenia - lista wszystkich wydarzeń z filtrowaniem (Admin, Domownik)
- 📝 Zadania - zarządzanie szablonami zadań (Admin, Domownik)
- 🏷️ Kategorie - widok kategorii i podkategorii (Admin, Domownik)
- 👥 Gospodarstwo - zarządzanie członkami i ustawieniami (tylko Administrator)
- 📈 Historia - archiwum wykonanych wydarzeń (tylko Premium)
- 📊 Raporty - zestawienia kosztów (tylko Premium)
- 🔬 Analizy - zaawansowane analizy predykcyjne (tylko Premium)
- ⚙️ Ustawienia - konfiguracja profilu i preferencji
- ❓ Pomoc - FAQ i wsparcie

**Sekcja 2: Widoki Systemowe** (widoczna tylko dla użytkowników z rolą System Developer):
- 🖥️ System Dashboard - główny panel administracyjny platformy
- 🏢 Gospodarstwa - zarządzanie wszystkimi gospodarstwami w systemie
- 👤 Użytkownicy - administracja wszystkich kont użytkowników
- 💳 Subskrypcje - monitoring płatności i metryk finansowych
- 🔧 Administracja - zarządzanie infrastrukturą i konfiguracją
- 🎧 Wsparcie - narzędzia do obsługi użytkowników i troubleshooting
- 📈 Metryki Systemu - globalne statystyki i KPI platformy
- ⚙️ Konfiguracja Systemu - ustawienia globalne platformy

**Zachowanie menu**:
- Desktop (>1024px): Persistent sidebar zawsze widoczny, możliwość zwinięcia do ikon
- Tablet (768-1024px): Collapsible sidebar, domyślnie zwinięty
- Mobile (<768px): Hamburger menu z pełnoekranowym overlay
- Pozycje menu dynamicznie filtrowane na podstawie roli użytkownika i subskrypcji
- Wyraźne wizualne oddzielenie sekcji (separator, nagłówki sekcji)
- Active state indicator dla aktualnie wybranego widoku

#### 3.4.1 Dashboard główny
- **Kafelki nawigacyjne** (duże przyciski z ikonami) przekierowujące do głównych widoków:
  - 📅 Kalendarz → `/calendar` - widok miesięczny kalendarz
  - 📋 Wydarzenia → `/events` - lista wszystkich wydarzeń z filtrowaniem
  - 📝 Zadania → `/tasks` - zarządzanie szablonami zadań
  - 🏷️ Kategorie → `/categories` - widok kategorii i podkategorii z zadaniami
  - Layout: 2x2 kafelki desktop, pionowo na mobile
- **Interaktywny kalendarz tygodniowy** (PN-ND):
  - Wydarzenia wyświetlane bezpośrednio w dniach tygodnia
  - Całodniowe wydarzenia na górze każdego dnia
  - Pozostałe wydarzenia jako bary uporządkowane według godzin
  - Możliwość kliknięcia w dzień dla szczegółów
  - Nawigacja poprzedni/następny tydzień
  - Dzisiejszy dzień wyróżniony ramką
  - Responsive: dni spadają pionowo na mobile/tablet
- **Lista wydarzeń** (pod kalendarzem):
  - Scrollowalna lista kontynuująca wydarzenia
  - Dropdown wyboru zakresu: 7 dni (domyślnie), 14 dni, miesiąc
  - Color-coded urgency (primary/warning/danger)
  - Przekroczone terminy ze specjalnym wyróżnieniem
  - Kliknięcie w wydarzenie otwiera dialog ze szczegółami i akcjami
- **Toolbar** (prawy górny róg):
  - Przycisk dodawania nowego wydarzenia/zadania
  - Filtry (osoba odpowiedzialna, kategoria, priorytet)
  - Statystyki wykorzystania limitu (progress bars - Post-MVP)
- **Przełącznik gospodarstw**: w menu nawigacyjnym (sidebar)

#### 3.4.2 Widok Kalendarza Miesięcznego
- **Ścieżka**: `/calendar`
- **Kalendarz miesięczny** w siatce (7x5/6 wierszy dla dni)
  - Wydarzenia zaznaczone w dniach (kropki, ikony, kolory)
  - Dzisiejszy dzień wyróżniony ramką
  - Nawigacja między miesiącami (strzałki, przycisk "dzisiaj")
  - Color-coding wydarzeń (primary/warning/danger)
- **Po kliknięciu w dzień**:
  - Pod kalendarzem pojawia się lista wydarzeń tego dnia
  - Lista podobna jak w Dashboard (scrollowalna, color-coded)
  - Kliknięcie w wydarzenie otwiera dialog ze szczegółami i akcjami (EventDetailsDialog)
- **Po kliknięciu w pusty dzień**:
  - Otwiera się formularz dodawania wydarzenia z pre-wypełnioną datą
- **Toolbar** (prawy górny róg):
  - Przycisk dodawania nowego wydarzenia
  - Filtry (osoba odpowiedzialna, kategoria, priorytet)
  - Przycisk powrotu do dzisiejszego dnia
- **Responsywność**:
  - Desktop: Pełna siatka kalendarza, wydarzenia widoczne w dniach
  - Mobile/Tablet: Kompaktowa siatka, wydarzenia jako licznik, kliknięcie pokazuje listę
- **Dostęp przez**: Sidebar (📅 Kalendarz), kafelek na dashboardzie, bottom navigation (mobile)

#### 3.4.3 Widok Wydarzeń
- **Ścieżka**: `/events`
- **Pełna lista wszystkich wydarzeń** z filtrowaniem (podobna do `/tasks`)
- Lista wydarzeń w formie tabeli/kart (podobnie jak widok zadań)
- Wyróżnienie kolorystyczne (przekroczony/dzisiaj/nadchodzący)
- Informacja o powiązanym zadaniu
- Kliknięcie w wydarzenie otwiera dialog ze szczegółami i akcjami (EventDetailsDialog)
- Filtry (osoba odpowiedzialna, kategoria, priorytet, status, zakres dat)
- Sortowanie (data, nazwa, priorytet, status)
- Licznik wydarzeń według statusu
- Wyszukiwanie po nazwie/opisie
- Szybkie akcje na każdym wydarzeniu: potwierdź, przełóż, edytuj, anuluj
- **Dostęp przez**: Sidebar (📋 Wydarzenia) lub kafelek na dashboardzie

#### 3.4.4 Widok Zadań
- **Ścieżka**: `/tasks`
- Lista wszystkich szablonów zadań w gospodarstwie
- Wyświetlanie: nazwa, podkategoria, interwał, priorytet
- Przycisk "Utwórz wydarzenie" przy każdym zadaniu
- Filtry (podkategoria, priorytet, z/bez interwału)
- Sortowanie (nazwa, kategoria, priorytet)
- Szybka edycja zadań
- Możliwość dodania nowego zadania
- **Dostęp przez**: Sidebar (📝 Zadania) lub kafelek na dashboardzie

#### 3.4.5 Widok Dashboard (monitor)
- Uproszczony, czytelny interfejs
- Duża czcionka
- Wyświetlanie tylko najbliższych 5 wydarzeń
- Auto-refresh co 5 minut

#### 3.4.5 Panel Administratora
Specjalne widoki dostępne tylko dla użytkowników z rolą Administrator:

##### 3.4.5.1 Zarządzanie gospodarstwem domowym
- Edycja nazwy i adresu gospodarstwa
- Przegląd statystyk gospodarstwa (liczba członków, zadań, wydarzeń)
- Historia zmian w gospodarstwie (audit log)
- Ustawienia domyślne dla nowych zadań
- Archiwum usuniętych pozycji z możliwością przywrócenia

##### 3.4.5.2 Zarządzanie członkami gospodarstwa
- Lista wszystkich członków z rolami i statusami
- Formularz dodawania nowych członków z wysyłką zaproszeń
- Edycja ról i uprawnień istniejących członków
- Historia aktywności członków (ostatnie logowanie, akcje)
- Usuwanie członków z reassignment ich wydarzeń
- Zarządzanie zaproszeniami (pending, expired, resend)

##### 3.4.5.3 Centralne zarządzanie zadaniami i wydarzeniami
- Widok globalny wszystkich zadań w gospodarstwie
- Możliwość edycji zadań wszystkich kategorii
- Widok wszystkich wydarzeń z możliwością filtrowania
- Masowe operacje (zmiana interwałów, reassignment odpowiedzialnych)
- Przegląd konfliktów wydarzeń i ich rozwiązywanie
- Konfiguracja priorytetów i kategorii
- Import/export danych zadań (CSV)

##### 3.4.5.4 Zarządzanie subskrypcją i limitami
- Przegląd aktualnego planu i wykorzystania limitów
- Historia płatności i faktur
- Upgrade/downgrade planu subskrypcji
- Zarządzanie metodami płatności
- Ustawienia automatycznej odnowy
- Przegląd kosztów gospodarstwa (tylko premium)

##### 3.4.5.5 Ustawienia systemowe gospodarstwa
- Konfiguracja stref czasowych i formatów dat
- Ustawienia powiadomień dla całego gospodarstwa
- Zarządzanie kategoriami i priorytetami
- Backup i przywracanie danych
- Integracje z zewnętrznymi systemami
- Logi systemowe i diagnostyka

##### 3.4.5.6 Raporty i analizy administratora
- Dashboard z kluczowymi metrykami gospodarstwa
- Raporty wykorzystania funkcji przez członków
- Analiza efektywności zarządzania wydarzeniami
- Statystyki potwierdzanych vs przegapionych wydarzeń
- Przegląd najczęściej używanych zadań/kategorii
- Export raportów dla zewnętrznych systemów księgowych

#### 3.4.6 Panel System Developer (Super Admin)
Widoki dostępne wyłącznie dla twórców oprogramowania i administratorów systemu:

##### 3.4.6.0 Dashboard systemu
- Główny panel administracyjny z kluczowymi metrykami platformy
- **Kafelki nawigacyjne** (duże przyciski z ikonami) do głównych sekcji systemowych:
  - 🏢 Gospodarstwa - przejście do `/system/households`
  - 👤 Użytkownicy - przejście do `/system/users`
  - 💳 Subskrypcje - przejście do `/system/subscriptions`
  - 🔧 Administracja - przejście do `/system/administration`
  - 🎧 Wsparcie - przejście do `/system/support`
- **(Post-MVP)** Kluczowe metryki systemu (uptime, performance, error rate, response time)
- **(Post-MVP)** Przegląd aktywności gospodarstw (nowe, aktywne, nieaktywne)
- **(Post-MVP)** Panel alertów systemowych i incydentów wymagających uwagi
- **(Post-MVP)** Szybkie statystyki biznesowe (nowi użytkownicy, MRR, churn rate)
- **(Post-MVP)** Wykresy trendu wzrostu użytkowników i przychodów
- **(Post-MVP)** Real-time monitoring statusu systemu

##### 3.4.6.1 Zarządzanie gospodarstwami
- Lista wszystkich gospodarstw w systemie z podstawowymi statystykami
- Wyszukiwanie i filtrowanie gospodarstw (po nazwie, dacie utworzenia, planie)
- Tworzenie nowych gospodarstw z przypisaniem administratorów
- Edycja podstawowych danych gospodarstw (nazwa, adres, ustawienia)
- Usuwanie i archiwizowanie nieaktywnych gospodarstw
- Przenoszenie członków między gospodarstwami
- Historia zmian i operacji na gospodarstwach

##### 3.4.6.2 Zarządzanie użytkownikami globalnie  
- Lista wszystkich użytkowników w systemie
- Wyszukiwanie użytkowników po email, imieniu, gospodarstwie
- Zmiana ról użytkowników w ramach ich gospodarstw
- Przypisywanie użytkowników do gospodarstw
- Resetowanie haseł i odblokowanie kont
- Historia aktywności użytkowników (logowania, akcje)
- Usuwanie kont i dane RODO compliance

##### 3.4.6.3 Monitorowanie subskrypcji i płatności
- Dashboard wszystkich subskrypcji w systemie
- Przegląd przychodów i metryk finansowych
- Zarządzanie promocjami i kodami rabatowymi
- Ręczna modyfikacja planów subskrypcji
- Obsługa sporów i refundacji
- Analiza churn rate i conversion metrics
- Export danych finansowych dla księgowości

##### 3.4.6.4 Administracja systemowa
- Monitorowanie wydajności i uptime systemu
- Przegląd logów systemowych i błędów
- Zarządzanie backup'ami i disaster recovery
- Konfiguracja globalnych ustawień platformy
- Zarządzanie kategoriami i szabłonami systemowymi
- Aktualizacje systemu i maintenance mode
- Monitoring bezpieczeństwa i incident response

##### 3.4.6.5 Analizy i metryki globalne
- Dashboard z kluczowymi KPI całej platformy
- Analizy wzrostu użytkowników i retention
- Statystyki wykorzystania funkcji na poziomie systemu  
- Raporty wydajności i cost optimization
- A/B testing results i feature adoption
- Przewidywania trendu i capacity planning
- Export danych dla business intelligence

##### 3.4.6.6 Wsparcie techniczne
- System ticketów i obsługa użytkowników
- Narzędzia diagnostyczne i troubleshooting
- Impersonacja użytkowników (z audit trail)
- Masowe operacje i data migration tools
- Feature flags i rollout management  
- Monitoring alertów i incident management
- Dokumentacja techniczna i runbooks

### 3.5 Bezpieczeństwo i prywatność

#### 3.5.1 Szyfrowanie danych
- Szyfrowanie danych w spoczynku: AES-256
- Połączenia: TLS 1.3
- Hasła: haszowanie z użyciem bcrypt/Argon2

#### 3.5.2 Zgodność z RODO
- Informacja o przetwarzaniu danych przy rejestracji
- Zgoda na przetwarzanie danych
- Możliwość eksportu wszystkich danych użytkownika
- Całkowite usunięcie konta i danych na żądanie

#### 3.5.3 Ochrona danych medycznych
- Szczególna ochrona wizyt medycznych
- Rozważenie implementacji 2FA dla dostępu do danych medycznych
- Dodatkowe logowanie dostępu do danych wrażliwych

#### 3.5.4 Sesje użytkowników
- Automatyczne wylogowanie po 30 dniach nieaktywności
- Możliwość wylogowania ze wszystkich urządzeń
- Historia aktywności logowania

## 4. Granice produktu

### 4.1 Co jest w zakresie MVP

#### 4.1.1 Funkcjonalności podstawowe
- Trzy kategorie: przeglądy techniczne, wywóz śmieci, wizyty medyczne
- System użytkowników z trzema rolami
- Zarządzanie urządzeniami i wizytami
- Automatyczne generowanie terminów
- Podstawowy dashboard i kalendarz
- Model biznesowy freemium z określonymi limitami

#### 4.1.2 Obsługiwane platformy
- Aplikacja webowa responsywna (desktop, tablet, mobile browser)
- Przeglądarki: Chrome, Firefox, Safari, Edge (2 ostatnie wersje)

### 4.2 Co nie jest w zakresie MVP

#### 4.2.1 System powiadomień
- Powiadomienia email przed terminami
- Digest tygodniowy z nadchodzącymi terminami
- Konfiguracja częstotliwości powiadomień
- Powiadomienia o przekroczonych terminach

#### 4.2.2 Zarządzanie dokumentacją
- Upload plików (PDF, JPG, PNG)
- Przechowywanie dokumentów w kontekście urządzeń/wizyt
- Kategoryzacja dokumentów (faktura, instrukcja, gwarancja)
- Przeglądanie i pobieranie dokumentów
- Limity storage (100 MB dla wersji darmowej)

#### 4.2.3 Funkcje premium
- Historia serwisów z archiwum wykonanych terminów
- Raporty kosztów z zestawieniami miesięcznymi/rocznymi
- Zaawansowane analizy i przewidywania
- Ulepszone wykresy i wizualizacje

#### 4.2.4 Integracje zewnętrzne
- Synchronizacja z Google Calendar, Outlook, iCal
- Integracje z systemami smart home
- API dla zewnętrznych aplikacji
- Webhooks

#### 4.2.5 Zaawansowane funkcje AI/ML
- OCR do automatycznego rozpoznawania dat z dokumentów
- Machine Learning do przewidywania optymalnych terminów
- Chatbot do obsługi użytkownika
- Automatyczne kategoryzowanie dokumentów

#### 4.2.6 Aplikacje mobilne native
- Aplikacja iOS
- Aplikacja Android
- Powiadomienia push (dostępne tylko w native apps)

#### 4.2.7 Funkcje społecznościowe
- Udostępnianie gospodarstwa między niezależnymi użytkownikami
- Forum/społeczność użytkowników
- System rekomendacji serwisantów
- Oceny i opinie

#### 4.2.8 Integracje płatności
- Automatyczne księgowanie kosztów z konta bankowego
- Integracja z systemami księgowymi
- Faktury automatyczne

#### 4.2.9 Dodatkowe kategorie
Wszystkie kategorie poza trzema wymienionymi w MVP będą dodane w przyszłych wersjach

### 4.3 Przyszłe rozszerzenia (post-MVP)
Planowane w kolejnych iteracjach:
- System powiadomień email z konfiguracją częstotliwości
- Zarządzanie dokumentacją z upload i przechowywaniem plików
- Historia serwisów i raporty kosztów (funkcje premium)
- Rozszerzenie kategorii (rośliny, ubezpieczenia, płatności cykliczne, zwierzęta)
- Aplikacje mobilne native z powiadomieniami push
- Integracje z kalendarzami zewnętrznymi
- OCR dokumentów
- System rekomendacji terminów
- Marketplace serwisantów
- Udostępnianie dostępu sąsiadom (np. podlewanie roślin)

## 5. Historyjki użytkowników

### 5.0 Nawigacja i interfejs użytkownika

US-000: Nawigacja przez sidebar
Jako użytkownik aplikacji
Chcę używać wysuwnego menu z lewej strony
Aby szybko przechodzić między różnymi widokami aplikacji

Kryteria akceptacji:
- Sidebar zawiera dwie sekcje: Widoki Gospodarstwa i Widoki Systemowe (jeśli System Developer)
- Sekcja 1 wyświetla widoki dla aktualnie otwartego gospodarstwa
- Pozycje menu są dynamicznie filtrowane na podstawie roli i subskrypcji użytkownika
- Wizualne oznaczenie aktywnego widoku
- Desktop: persistent sidebar z możliwością zwinięcia do ikon
- Tablet: collapsible sidebar, domyślnie zwinięty
- Mobile: hamburger menu z pełnoekranowym overlay
- Smooth transitions przy collapse/expand
- Keyboard navigation (Tab, Enter, Arrow keys)
- Tooltips dla ikon w trybie zwiniętym

US-000a: Sekcja widoków gospodarstwa w sidebar
Jako użytkownik z dostępem do gospodarstwa
Chcę widzieć menu z widokami dla mojego gospodarstwa
Aby łatwo nawigować między funkcjami gospodarstwa

Kryteria akceptacji:
- Nagłówek sekcji z nazwą aktualnego gospodarstwa
- Lista widoków: Dashboard (z kafelkami + kalendarz tygodniowy), Kalendarz (miesięczny), Wydarzenia (lista), Zadania, Kategorie, etc.
- Ukrycie pozycji "Gospodarstwo" dla użytkowników nie będących administratorami
- Oznaczenie funkcji premium (badge/icon) dla użytkowników bez subskrypcji
- Zmiana gospodarstwa odświeża zawartość sekcji
- Separator wizualny pomiędzy widokami gospodarstwa a widokami systemowymi

US-000b: Sekcja widoków systemowych w sidebar
Jako System Developer
Chcę widzieć dodatkową sekcję menu z widokami administracyjnymi
Aby szybko przechodzić do narzędzi zarządzania platformą

Kryteria akceptacji:
- Sekcja widoczna tylko dla użytkowników z rolą System Developer
- Nagłówek sekcji "Administracja Systemu"
- Lista widoków systemowych: System Dashboard, Gospodarstwa, Użytkownicy, etc.
- Wizualne oddzielenie od sekcji gospodarstwa (separator, inny kolor tła)
- Badge indicators dla alertów/powiadomień systemowych
- Możliwość dostępu zarówno do widoków gospodarstwa jak i systemowych

US-000c: Przełączanie gospodarstw z sidebar
Jako użytkownik z dostępem do wielu gospodarstw
Chcę przełączać się między gospodarstwami z poziomu sidebar
Aby szybko zarządzać różnymi gospodarstwami

Kryteria akceptacji:
- Dropdown z listą gospodarstw w headerze sekcji gospodarstwa
- Wyświetlanie roli w każdym gospodarstwie
- Quick stats per household (liczba zadań, pilne terminy)
- Zmiana gospodarstwa aktualizuje zawartość sekcji 1 sidebar
- Zmiana gospodarstwa przekierowuje na dashboard wybranego gospodarstwa
- Zapamiętanie ostatnio wybranego gospodarstwa

### 5.1 Rejestracja i uwierzytelnianie

US-001: Rejestracja nowego użytkownika
Jako nowy użytkownik
Chcę zarejestrować się w aplikacji
Aby móc zarządzać terminami w moim gospodarstwie domowym

Kryteria akceptacji:
- Formularz rejestracji zawiera pola: email, hasło, powtórz hasło, imię
- Walidacja formatu email
- Hasło musi zawierać minimum 8 znaków, w tym cyfrę i znak specjalny
- Po rejestracji użytkownik otrzymuje email weryfikacyjny
- Wyświetlenie zgody na przetwarzanie danych zgodnie z RODO
- Po weryfikacji użytkownik jest przekierowany do onboardingu

US-002: Logowanie użytkownika
Jako zarejestrowany użytkownik
Chcę zalogować się do aplikacji
Aby uzyskać dostęp do moich terminów

Kryteria akceptacji:
- Formularz logowania zawiera pola: email, hasło
- Przycisk "Zapamiętaj mnie" do dłuższej sesji
- Link "Zapomniałem hasła"
- Komunikat o błędnych danych logowania
- Po poprawnym logowaniu przekierowanie do dashboardu
- Sesja wygasa po 30 dniach nieaktywności

US-003: Resetowanie hasła
Jako użytkownik, który zapomniał hasła
Chcę móc zresetować hasło
Aby odzyskać dostęp do konta

Kryteria akceptacji:
- Formularz z polem email
- Wysłanie linku resetującego na email
- Link ważny przez 24 godziny
- Formularz ustawienia nowego hasła z walidacją
- Potwierdzenie zmiany hasła emailem
- Automatyczne wylogowanie ze wszystkich urządzeń po zmianie

US-004: Wylogowanie
Jako zalogowany użytkownik
Chcę móc się wylogować
Aby zabezpieczyć dostęp do mojego konta

Kryteria akceptacji:
- Przycisk "Wyloguj" widoczny w nawigacji
- Opcja "Wyloguj ze wszystkich urządzeń"
- Potwierdzenie wylogowania
- Przekierowanie na stronę logowania
- Wyczyszczenie sesji

### 5.2 Zarządzanie gospodarstwem domowym

US-005: Tworzenie gospodarstwa domowego
Jako nowy administrator
Chcę utworzyć gospodarstwo domowe
Aby móc zarządzać terminami dla mojej rodziny

Kryteria akceptacji:
- Formularz z nazwą gospodarstwa
- Możliwość dodania adresu (opcjonalne)
- Automatyczne przypisanie roli Administratora do twórcy
- Komunikat powitalny
- Przekierowanie do dodania pierwszego urządzenia (onboarding)

US-006: Dodawanie członka gospodarstwa
Jako administrator
Chcę dodać członka do mojego gospodarstwa
Aby mógł zarządzać przypisanymi do niego terminami

Kryteria akceptacji:
- Formularz z polami: imię, email, rola (Administrator/Domownik/Dashboard)
- Wysłanie zaproszenia emailem
- Link aktywacyjny ważny 7 dni
- W wersji darmowej limit 3 osoby
- Komunikat o osiągnięciu limitu z propozycją upgrade
- Lista członków gospodarstwa z możliwością edycji ról

US-007: Usuwanie członka gospodarstwa
Jako administrator
Chcę usunąć członka z gospodarstwa
Aby zarządzać składem mojej rodziny w systemie

Kryteria akceptacji:
- Przycisk usunięcia przy każdym członku (oprócz siebie)
- Potwierdzenie akcji z ostrzeżeniem o konsekwencjach
- Przypisanie terminów usuniętego członka do administratora
- Powiadomienie email do usuniętego użytkownika
- Użytkownik nie ma już dostępu do tego gospodarstwa

US-008: Zmiana roli członka
Jako administrator
Chcę zmienić rolę członka gospodarstwa
Aby dostosować uprawnienia do potrzeb

Kryteria akceptacji:
- Dropdown z dostępnymi rolami przy każdym członku
- Natychmiastowa zmiana po wyborze
- Powiadomienie email do użytkownika o zmianie roli
- Niemożność zmiany roli własnej (wymaga innego administratora)
- Wymagany przynajmniej jeden administrator w gospodarstwie

### 5.2.1 Panel administratora - zarządzanie gospodarstwem

US-046: Przegląd statystyk gospodarstwa
Jako administrator
Chcę widzieć kluczowe statystyki mojego gospodarstwa
Aby monitorować aktywność i wykorzystanie systemu

Kryteria akceptacji:
- Dashboard ze statystykami: liczba członków, urządzeń, terminów w miesiącu
- Wykresy aktywności członków w czasie
- Wykorzystanie limitów planu (urządzenia, członkowie, storage)
- Lista ostatnich akcji w gospodarstwie (audit log)
- Porównanie z poprzednim miesiącem
- Export statystyk do CSV/PDF

US-047: Konfiguracja ustawień gospodarstwa
Jako administrator
Chcę konfigurować globalne ustawienia dla całego gospodarstwa
Aby dostosować system do naszych potrzeb

Kryteria akceptacji:
- Edycja nazwy i adresu gospodarstwa
- Ustawienia domyślnych interwałów dla nowych urządzeń
- Konfiguracja kategorii i priorytetów
- Ustawienia stref czasowych i formatów dat
- Domyślne ustawienia powiadomień dla nowych członków
- Język interfejsu dla gospodarstwa

US-048: Zarządzanie zaproszeniami
Jako administrator
Chcę zarządzać wysłanymi zaproszeniami do gospodarstwa
Aby kontrolować proces dołączania nowych członków

Kryteria akceptacji:
- Lista wszystkich wysłanych zaproszeń z statusami
- Opcje: pending, accepted, expired, cancelled
- Możliwość ponownego wysłania zaproszenia
- Anulowanie niewykorzystanych zaproszeń
- Historia zaproszeń z datami i akcjami
- Ustawienie czasu wygaśnięcia zaproszeń

US-049: Historia aktywności członków
Jako administrator
Chcę monitorować aktywność członków gospodarstwa
Aby ocenić zaangażowanie i zapewnić bezpieczeństwo

Kryteria akceptacji:
- Lista członków z ostatnim logowaniem i aktywnością
- Historia akcji każdego członka (dodane urządzenia, potwierdzone terminy)
- Statystyki wykonanych vs przegapionych terminów na członka
- Identyfikacja nieaktywnych członków (brak logowania >30 dni)
- Możliwość wysłania przypomnienia do nieaktywnych
- Eksport danych aktywności

US-050: Masowe zarządzanie zadaniami i wydarzeniami
Jako administrator
Chcę wykonywać operacje na wielu zadaniach lub wydarzeniach jednocześnie
Aby efektywnie zarządzać dużym gospodarstwem

Kryteria akceptacji:
- Selekcja wielu zadań lub wydarzeń (checkbox)
- Masowa zmiana osoby odpowiedzialnej (wydarzenia)
- Masowa aktualizacja interwałów (zadania)
- Masowa zmiana podkategorii
- Masowe archiwizowanie/usuwanie
- Potwierdzenie przed wykonaniem operacji masowej
- Preview zmian przed aplikacją

US-051: Zarządzanie konfliktami wydarzeń
Jako administrator
Chcę identyfikować i rozwiązywać konflikty wydarzeń
Aby zapewnić płynne funkcjonowanie gospodarstwa

Kryteria akceptacji:
- Widok wszystkich konfliktów wydarzeń (ten sam dzień, ta sama osoba)
- Automatyczne wykrywanie nakładających się wydarzeń
- Sugestie rozwiązania (przesunięcie, zmiana odpowiedzialnego)
- Możliwość ręcznej zmiany daty wydarzenia w konflikcie
- Powiadomienia członków o zmianach
- Historia rozwiązanych konfliktów

### 5.2.2 Panel administratora - subskrypcja i finanse

US-052: Przegląd wykorzystania planu
Jako administrator
Chcę monitorować wykorzystanie limitów mojego planu
Aby planować ewentualny upgrade

Kryteria akceptacji:
- Dashboard z wykorzystaniem: członkowie, zadania, storage
- Wykresy trendu wykorzystania w czasie
- Prognozy osiągnięcia limitów
- Porównanie planów (current vs available upgrades)
- Kalkulacja oszczędności przy rocznej płatności
- Historia zmian planu

US-053: Zarządzanie metodami płatności
Jako administrator z subskrypcją premium
Chcę zarządzać metodami płatności
Aby kontrolować finansowanie subskrypcji

Kryteria akceptacji:
- Lista wszystkich dodanych kart/kont
- Dodawanie nowej metody płatności
- Usuwanie starych metod płatności
- Ustawianie domyślnej metody
- Testowanie ważności karty
- Powiadomienia o zbliżającym się wygaśnięciu karty

US-054: Historia płatności i faktury
Jako administrator z subskrypcją premium
Chcę przeglądać historię płatności i pobierać faktury
Aby prowadzić rozliczenia finansowe

Kryteria akceptacji:
- Lista wszystkich płatności z datami i kwotami
- Status płatności (successful, failed, pending, refunded)
- Pobieranie faktur w formacie PDF
- Wysyłka faktur emailem na dodatkowy adres
- Filtrowanie po okresach i statusach
- Export zestawienia dla księgowości

### 5.2.3 Panel administratora - analizy i raporty

US-055: Raporty efektywności gospodarstwa
Jako administrator
Chcę generować raporty efektywności mojego gospodarstwa
Aby optymalizować zarządzanie wydarzeniami

Kryteria akceptacji:
- Raport potwierdzanych vs przegapionych wydarzeń
- Analiza najczęściej niewykonywanych zadań
- Statystyki aktywności członków
- Średnie czasy reakcji na wydarzenia
- Identyfikacja wzorców sezonowych
- Rekomendacje optymalizacji

US-056: Dashboard analityczny administratora
Jako administrator
Chcę mieć szybki dostęp do kluczowych metryk
Aby na bieżąco monitorować stan gospodarstwa

Kryteria akceptacji:
- Widget z nadchodzącymi wydarzeniami krytycznymi
- Mierniki wykorzystania planu (liczba zadań, członków, storage)
- Alerty o problemach (przekroczone wydarzenia, konflikty)
- Statystyki aktywności w czasie rzeczywistym
- Szybkie akcje (dodaj członka, dodaj zadanie, wyślij przypomnienie)
- Personalizacja widżetów na dashboardzie

### 5.2.4 Panel System Developer - zarządzanie systemem

US-057: Przegląd wszystkich gospodarstw w systemie
Jako System Developer
Chcę widzieć listę wszystkich gospodarstw w systemie
Aby monitorować aktywność platformy i udzielać wsparcia

Kryteria akceptacji:
- Lista gospodarstw z podstawowymi statystykami (liczba członków, zadań, wydarzeń, ostatnia aktywność)
- Wyszukiwanie po nazwie gospodarstwa lub email administratora
- Filtrowanie po planie subskrypcji, dacie utworzenia, statusie aktywności
- Sortowanie po różnych kryteriach
- Paginacja dla dużych ilości danych
- Export listy do CSV/Excel
- Oznaczenie gospodarstw wymagających uwagi (nieaktywne, problemy płatności)

US-058: Tworzenie gospodarstwa dla użytkownika
Jako System Developer
Chcę móc tworzyć nowe gospodarstwa i przypisywać administratorów
Aby wspierać użytkowników w procesie onboardingu lub migracji

Kryteria akceptacji:
- Formularz tworzenia gospodarstwa z polami: nazwa, adres, plan subskrypcji
- Wyszukiwanie i dodawanie administratora po email
- Opcja utworzenia nowego konta administratora
- Wysłanie zaproszenia z hasłem tymczasowym
- Konfiguracja początkowych ustawień gospodarstwa
- Powiadomienie email do nowego administratora
- Audit log operacji tworzenia

US-059: Zarządzanie użytkownikami globalnie
Jako System Developer
Chcę zarządzać kontami użytkowników w całym systemie
Aby rozwiązywać problemy i udzielać wsparcia technicznego

Kryteria akceptacji:
- Wyszukiwanie użytkowników po email, imieniu, gospodarstwie
- Wyświetlanie szczegółów konta: role, ostatnia aktywność, subskrypcja
- Resetowanie hasła użytkownika z powiadomieniem email
- Odblokowywanie zablokowanych kont
- Zmiana roli użytkownika w ramach gospodarstwa
- Przenoszenie użytkownika do innego gospodarstwa
- Historia wszystkich akcji na koncie użytkownika

US-060: Monitorowanie subskrypcji i płatności
Jako System Developer
Chcę monitorować wszystkie subskrypcje i płatności w systemie
Aby zapewnić stabilność finansową platformy

Kryteria akceptacji:
- Dashboard z kluczowymi metrykami: MRR, churn rate, conversion rate
- Lista wszystkich aktywnych subskrypcji z datami odnowienia
- Przegląd nieudanych płatności z możliwością retry
- Zarządzanie promocjami i kodami rabatowymi
- Ręczna modyfikacja planu subskrypcji dla użytkownika
- Historia wszystkich transakcji z możliwością refund
- Alerty o problemach płatności

US-061: Administracja systemowa i monitoring
Jako System Developer
Chcę monitorować wydajność i stabilność systemu
Aby zapewnić wysoką jakość usługi

Kryteria akceptacji:
- Dashboard z metrykami systemu: uptime, response time, error rate
- Przegląd logów aplikacji z filtrowaniem po poziomie i dacie
- Monitoring wykorzystania zasobów (CPU, memoria, storage)
- Zarządzanie backup'ami z możliwością restore
- Konfiguracja alertów o problemach systemowych
- Maintenance mode z komunikatem dla użytkowników
- Feature flags do kontrolowanego rollout nowych funkcji

US-062: Wsparcie techniczne i troubleshooting
Jako System Developer
Chcę mieć narzędzia do diagnozowania i rozwiązywania problemów użytkowników
Aby szybko udzielać skutecznego wsparcia

Kryteria akceptacji:
- Impersonacja użytkownika z pełnym audit trail
- Narzędzia diagnostyczne do analizy problemów
- System ticketów wsparcia z historią konwersacji
- Szablony odpowiedzi dla typowych problemów
- Możliwość ręcznej synchronizacji danych użytkownika
- Narzędzia do masowych operacji (bulk updates)
- Dokumentacja troubleshooting dla zespołu wsparcia

### 5.2.5 Zarządzanie kategoriami i typami kategorii

US-063: Przeglądanie i zarządzanie kategoriami
Jako Administrator lub System Developer
Chcę przeglądać, dodawać, edytować i usuwać kategorie
Aby dostosować system do potrzeb gospodarstwa

Kryteria akceptacji:
- Dostęp do widoku kategorii przez sidebar (🏷️ Kategorie)
- Lista wszystkich kategorii pogrupowanych po typach
- Filtrowanie kategorii po typie i wyszukiwanie po nazwie
- Możliwość dodania nowej kategorii z przypisaniem do typu
- Możliwość edycji istniejącej kategorii (nazwa, opis, typ, kolejność)
- Możliwość usunięcia kategorii (soft delete)
- Sortowanie kategorii według kolejności (sortOrder) i nazwy
- Wyświetlanie liczby itemów przypisanych do każdej kategorii

US-064: Zarządzanie typami kategorii (System Developer)
Jako System Developer
Chcę zarządzać typami kategorii na poziomie systemu
Aby rozszerzać funkcjonalność platformy o nowe obszary

Kryteria akceptacji:
- Dostęp do zarządzania typami kategorii w widoku Kategorie
- Przycisk "Dodaj typ kategorii" w toolbar obok "Dodaj kategorię"
- Dialog tworzenia nowego typu kategorii z polami: nazwa, opis, kolejność sortowania
- Dialog edycji istniejącego typu (nazwa, opis, kolejność) otwierany przez ikonę ołówka
- Możliwość usunięcia typu kategorii (soft delete)
- Walidacja: nie można usunąć typu jeśli ma przypisane kategorie
- Możliwość sortowania typów według kolejności
- Automatyczne generowanie ikon i kolorów dla nowych typów
- Powiadomienia toast o sukcesie/błędzie operacji
- Automatyczne odświeżenie listy kategorii po zmianach

US-065: Inline editing kategorii
Jako Administrator
Chcę szybko edytować podstawowe informacje o kategorii
Aby efektywnie zarządzać kategoriami bez otwierania dialogów

Kryteria akceptacji:
- Możliwość edycji nazwy i opisu kategorii bezpośrednio na liście
- Kliknięcie na nazwę kategorii otwiera pole edycji
- Enter zapisuje zmiany, Escape anuluje
- Wizualna informacja o zapisywaniu zmian
- Powiadomienie o pomyślnym zapisaniu lub błędzie
- Możliwość przeciągania kategorii między typami (drag & drop)

### 5.3 Zarządzanie zadaniami i wydarzeniami

#### 5.3.1 Zadania (Tasks)

US-009: Dodawanie zadania
Jako domownik
Chcę dodać szablon zadania do systemu
Aby móc tworzyć z niego konkretne wydarzenia

Kryteria akceptacji:
- Formularz zawiera: nazwę zadania, podkategorię (dropdown), interwał (opcjonalny: lata/miesiące/tygodnie/dni), priorytet, notatki
- Walidacja wszystkich wymaganych pól
- W wersji darmowej limit 5 zadań łącznie
- Komunikat o dodaniu zadania
- Automatyczne przekierowanie do listy zadań
- Możliwość natychmiastowego utworzenia wydarzenia dla tego zadania

US-010: Edycja zadania
Jako domownik
Chcę edytować dane zadania
Aby aktualizować szablon w systemie

Kryteria akceptacji:
- Dostęp do edycji dla wszystkich członków gospodarstwa
- Formularz z wypełnionymi aktualnymi danymi
- Możliwość zmiany wszystkich pól
- Zmiana interwału wpływa tylko na nowo tworzone wydarzenia
- Potwierdzenie zapisania zmian
- Opcja anulowania bez zapisywania

US-011: Usuwanie zadania
Jako domownik
Chcę usunąć zadanie z systemu
Aby pozbyć się nieaktualnych szablonów

Kryteria akceptacji:
- Przycisk usunięcia przy każdym zadaniu
- Dialog potwierdzenia z ostrzeżeniem o powiązanych wydarzeniach
- Dialog z opcjami: "Usuń całkowicie" lub "Archiwizuj"
- Usunięcie całkowite usuwa zadanie ale zachowuje powiązane wydarzenia (jako orphaned)
- Archiwizacja zachowuje historię (funkcja premium)
- Zwolnienie miejsca w limicie (wersja darmowa)
- Komunikat potwierdzający

US-012: Filtrowanie i sortowanie listy zadań
Jako użytkownik
Chcę filtrować i sortować listę zadań
Aby szybko znaleźć interesującą mnie pozycję

Kryteria akceptacji:
- Filtry: wszystkie/podkategoria/priorytet/z interwałem/bez interwału
- Sortowanie: po nazwie/podkategorii/priorytecie/dacie dodania
- Możliwość łączenia filtrów
- Licznik wyświetlonych pozycji
- Resetowanie filtrów jednym przyciskiem
- Zapisanie wybranych filtrów w sesji

#### 5.3.2 Wydarzenia (Events)

US-013a: Tworzenie wydarzenia z zadania
Jako domownik
Chcę utworzyć konkretne wydarzenie z szablonu zadania
Aby zaplanować termin wykonania

Kryteria akceptacji:
- Przycisk "Utwórz wydarzenie" przy każdym zadaniu
- Formularz z polami: data terminu, osoba odpowiedzialna, priorytet (dziedziczony z zadania)
- Walidacja daty (nie może być w przeszłości)
- Przypisanie do członka gospodarstwa
- Komunikat potwierdzający utworzenie
- Przekierowanie do widoku wydarzeń lub kalendarza

US-013b: Edycja wydarzenia
Jako domownik
Chcę edytować konkretne wydarzenie
Aby dostosować termin lub osobę odpowiedzialną

Kryteria akceptacji:
- Dostęp do edycji tylko dla przypisanych wydarzeń (lub administrator)
- Możliwość zmiany daty, osoby odpowiedzialnej, notatek
- Nie można zmienić powiązanego zadania
- Potwierdzenie zapisania zmian
- Opcja anulowania bez zapisywania

### 5.4 Widoki aplikacji

US-012a: Widok Dashboard główny (kafelki + kalendarz tygodniowy + lista wydarzeń)
Jako użytkownik
Chcę mieć centralny dashboard z nawigacją kafelkową i widokiem kalendarza tygodniowego
Aby szybko przechodzić do głównych funkcji i widzieć nadchodzące wydarzenia

Kryteria akceptacji:
- **Kafelki nawigacyjne** (2x2 na desktop, pionowo na mobile):
  - 📅 Kalendarz → `/calendar` (widok miesięczny)
  - 📋 Wydarzenia → `/events` (lista wszystkich)
  - 📝 Zadania → `/tasks` (szablony zadań)
  - 🏷️ Kategorie → `/categories` (organizacja)
- **Interaktywny kalendarz tygodniowy** (PN-ND):
  - Wydarzenia wyświetlane bezpośrednio w dniach jako bary
  - Całodniowe wydarzenia na górze każdego dnia
  - Nawigacja poprzedni/następny tydzień
  - Dzisiejszy dzień wyróżniony ramką
  - Responsive: dni spadają pionowo na mobile/tablet
- **Lista wydarzeń** (pod kalendarzem):
  - Dropdown wyboru zakresu: 7/14/30 dni
  - Color-coded urgency (primary/warning/danger)
  - Przekroczone terminy ze specjalnym wyróżnieniem
  - Kliknięcie w wydarzenie otwiera EventDetailsDialog z akcjami
- **Toolbar**: przycisk dodawania, filtry, statystyki wykorzystania limitu (Post-MVP)

US-012b: Widok Kalendarza Miesięcznego
Jako użytkownik
Chcę widzieć wydarzenia w widoku miesięcznym kalendarza
Aby mieć lepszy przegląd harmonogramu

Kryteria akceptacji:
- Dostęp przez sidebar (📅 Kalendarz), kafelek na dashboardzie, bottom nav (mobile)
- Siatka 7x5/6 (dni × tygodnie)
- Wydarzenia zaznaczone w dniach (kropki, ikony, liczniki)
- Dzisiejszy dzień wyróżniony ramką
- Nawigacja między miesiącami (strzałki, przycisk "dzisiaj")
- Kliknięcie w dzień → lista wydarzeń pod kalendarzem
- Kliknięcie w pusty dzień → formularz dodawania wydarzenia z pre-wypełnioną datą
- Toolbar: przycisk dodawania, filtry, przycisk powrotu do dzisiaj
- Responsive: kompaktowa siatka na mobile z licznikami

US-012c: Widok Wydarzeń (pełna lista wszystkich wydarzeń)
Jako użytkownik
Chcę mieć pełną listę wszystkich wydarzeń z filtrowaniem
Aby zarządzać wszystkimi terminami w jednym miejscu

Kryteria akceptacji:
- Dostęp przez sidebar (📋 Wydarzenia) lub kafelek na dashboardzie
- Lista wydarzeń w formie tabeli/kart
- Wyróżnienie kolorystyczne: przekroczony (czerwony), dzisiaj (pomarańczowy), nadchodzące (zielony)
- Informacja o powiązanym zadaniu
- Kliknięcie w wydarzenie otwiera EventDetailsDialog z akcjami
- Filtry: osoba odpowiedzialna, kategoria, priorytet, status, zakres dat
- Sortowanie: data, nazwa, priorytet, status
- Licznik wydarzeń według statusu
- Wyszukiwanie po nazwie/opisie
- Szybkie akcje: potwierdź, przełóż, edytuj, anuluj

US-012d: Widok Zadań (szablony)
Jako użytkownik
Chcę widzieć wszystkie moje szablony zadań
Aby zarządzać powtarzalnymi aktywnościami

Kryteria akceptacji:
- Dostęp przez sidebar (📝 Zadania) lub kafelek na dashboardzie
- Lista wszystkich zadań w gospodarstwie
- Wyświetlanie podkategorii, interwału, priorytetu
- Przycisk "Utwórz wydarzenie" przy każdym zadaniu
- Filtry: podkategoria, priorytet, z/bez interwału
- Sortowanie: nazwa, kategoria, priorytet, data utworzenia
- Licznik zadań według podkategorii
- Szybki dostęp do edycji zadań
- Możliwość dodania nowego zadania

US-012e: Widok Kategorii (struktura organizacyjna)
Jako użytkownik
Chcę widzieć hierarchię kategorii i podkategorii z przypisanymi zadaniami
Aby łatwiej zarządzać podobnymi elementami razem

Kryteria akceptacji:
- Dostęp przez sidebar (🏷️ Kategorie) lub kafelek na dashboardzie
- Grupy typów kategorii z możliwością collapse/expand
- Podkategorie w ramach każdego typu
- Licznik zadań w każdej podkategorii
- Możliwość dodania nowego zadania bezpośrednio do podkategorii
- Szybki dostęp do edycji zadań
- Sortowanie wewnątrz kategorii (nazwa, priorytet)
- Accordion navigation z lazy loading dla dużych kategorii

US-013: Wyświetlanie nadchodzących wydarzeń na dashboardzie
Jako użytkownik
Chcę widzieć nadchodzące wydarzenia na dashboardzie
Aby być na bieżąco z obowiązkami

Kryteria akceptacji:
- Lista wydarzeń na najbliższe 7 dni
- Sortowanie chronologiczne
- Wyróżnienie kolorystyczne: przekroczony (czerwony), dzisiaj (pomarańczowy), nadchodzące (zielony)
- Wyświetlanie: nazwa zadania, kategoria, osoba odpowiedzialna, data
- Szybkie akcje: potwierdź, przełóż, edytuj, anuluj
- Odświeżanie w czasie rzeczywistym
- Link do pełnego widoku wydarzeń

US-014: Potwierdzanie wykonania wydarzenia
Jako domownik
Chcę potwierdzić wykonanie wydarzenia
Aby system automatycznie wygenerował kolejne (jeśli zadanie ma interwał)

Kryteria akceptacji:
- Przycisk "Potwierdź wykonanie" przy wydarzeniu
- Opcjonalne pole na notatkę o wykonaniu
- Pole na datę faktycznego wykonania (domyślnie dzisiaj)
- Możliwość załączenia zdjęcia/dokumentu
- Jeśli zadanie ma interwał → automatyczne utworzenie następnego wydarzenia
- Data następnego wydarzenia = completionDate + interwał zadania
- Przesunięcie wydarzenia do historii (premium) lub oznaczenie jako completed
- Powiadomienie innych członków gospodarstwa o wykonaniu

US-015: Przełożenie wydarzenia
Jako domownik
Chcę przełożyć wydarzenie na inny dzień
Aby dostosować harmonogram do sytuacji

Kryteria akceptacji:
- Przycisk "Przełóż" przy wydarzeniu
- Kalendarz do wyboru nowej daty lub pole z liczbą dni przesunięcia
- Wymagane pole z uzasadnieniem/notatką
- Historia przełożeń widoczna w wersji premium
- Potwierdzenie nowej daty
- Nie wpływa na szablon zadania
- Email do wszystkich członków o przesunięciu

US-016: Edycja wydarzenia
Jako domownik
Chcę edytować pojedyncze wydarzenie
Aby dostosować konkretny termin

Kryteria akceptacji:
- Możliwość zmiany daty
- Możliwość zmiany osoby odpowiedzialnej
- Możliwość zmiany priorytetu
- Możliwość dodania notatki
- Nie można zmienić powiązanego zadania
- Nie wpływa na inne wydarzenia tego samego zadania
- Potwierdzenie zmian
- Powiadomienie o zmianie

US-017: Anulowanie wydarzenia
Jako domownik
Chcę anulować pojedyncze wydarzenie
Aby pominąć jednorazowo wykonanie

Kryteria akceptacji:
- Przycisk "Anuluj wydarzenie" przy wydarzeniu
- Dialog potwierdzenia z polem na powód anulowania
- Status wydarzenia zmieniony na "cancelled"
- Zadanie pozostaje w systemie
- Nie generuje automatycznie nowego wydarzenia
- Historia anulowanych wydarzeń (premium)
- Email potwierdzający do członków gospodarstwa

US-018: Widok kalendarza miesięcznego
Jako użytkownik
Chcę widzieć wydarzenia w formie kalendarza
Aby mieć lepszy przegląd harmonogramu

Kryteria akceptacji:
- Standardowy widok kalendarzowy z dniami miesiąca
- Oznaczenia kolorowe według kategorii zadań
- Możliwość przejścia do poprzedniego/następnego miesiąca
- Kliknięcie w wydarzenie otwiera szczegóły i akcje
- Licznik wydarzeń w danym dniu jeśli więcej niż 3
- Legenda kolorów kategorii
- Możliwość przeciągania wydarzeń między dniami (drag & drop)

### 5.5 Widoki specjalne

US-027: Widok Dashboard (monitor na ścianie)
Jako użytkownik z monitorem na ścianie
Chcę mieć uproszczony widok terminów
Aby szybko sprawdzić najbliższe obowiązki

Kryteria akceptacji:
- Dostęp przez oddzielny URL lub przełącznik trybu
- Tylko najbliższe 5 terminów
- Duża, czytelna czcionka (minimum 24px)
- Wysokie kontrasty kolorystyczne
- Brak możliwości edycji (tylko odczyt)
- Auto-refresh co 5 minut
- Wyświetlanie aktualnej daty i godziny
- Responsywność dla różnych rozmiarów monitorów

US-028: Onboarding nowego użytkownika
Jako nowy użytkownik
Chcę być przeprowadzony przez proces pierwszego użycia
Aby szybko zacząć korzystać z aplikacji

Kryteria akceptacji:
- Powitalna wiadomość z krótkim wyjaśnieniem aplikacji
- Krok 1: Stwórz gospodarstwo domowe
- Krok 2: Dodaj pierwsze zadanie (szablon)
- Krok 3: Utwórz pierwsze wydarzenie
- Możliwość pominięcia onboardingu
- Wskazówki kontekstowe (tooltips) przy pierwszym użyciu funkcji
- Checkbox "Nie pokazuj ponownie"

### 5.6 Funkcje premium

US-029: Historia wydarzeń
Jako użytkownik premium
Chcę przeglądać historię wszystkich wykonanych wydarzeń
Aby analizować częstotliwość i koszty

Kryteria akceptacji:
- Zakładka "Historia" w menu (tylko premium)
- Lista wszystkich zakończonych wydarzeń (completed/cancelled)
- Filtry: data, kategoria, zadanie, osoba
- Wyświetlanie: data wykonania, notatki, załączone dokumenty, status
- Export do CSV lub PDF
- Statystyki: liczba wykonanych wydarzeń w okresie, średni czas realizacji
- Wykresy trendu częstotliwości wykonania zadań

US-030: Dodawanie kosztów do wydarzeń
Jako użytkownik premium
Chcę dodawać koszty do wykonanych wydarzeń
Aby śledzić wydatki gospodarstwa

Kryteria akceptacji:
- Pole "Koszt" przy potwierdzaniu wykonania wydarzenia
- Waluta zgodna z ustawieniami użytkownika
- Możliwość dodania kosztów później (edycja historii)
- Kategoryzacja kosztów: części, robocizna, przejazd, inne
- Załączanie faktury jako dokumentu
- Suma kosztów widoczna przy zadaniu (zsumowane ze wszystkich wydarzeń)

US-031: Raporty kosztów
Jako użytkownik premium
Chcę generować raporty wydatków
Aby kontrolować budżet gospodarstwa

Kryteria akceptacji:
- Zakładka "Raporty" w menu (tylko premium)
- Wybór okresu: miesiąc, kwartał, rok, własny zakres
- Suma wszystkich kosztów w okresie
- Rozbicie po kategoriach i podkategoriach
- Rozbicie po zadaniach (TOP 5 najdroższe)
- Wykres słupkowy wydatków miesięcznych
- Porównanie rok do roku
- Export raportu do PDF

US-032: Zaawansowane analizy
Jako użytkownik premium
Chcę mieć dostęp do analiz predykcyjnych
Aby planować przyszłe wydatki i obciążenie

Kryteria akceptacji:
- Zakładka "Analizy" w menu (tylko premium)
- Prognoza wydatków na najbliższe 6 miesięcy na podstawie historii
- Identyfikacja zadań najczęściej przekraczających termin
- Sugestie optymalizacji kosztów i harmonogramu
- Heatmapa intensywności wydarzeń w czasie
- Analiza obciążenia poszczególnych domowników
- Wykresy trendów długoterminowych

US-033: Ulepszone wykresy
Jako użytkownik premium
Chcę wizualizować wydarzenia na osi czasu
Aby lepiej planować harmonogram

Kryteria akceptacji:
- Wykres Gantta z wydarzeniami na najbliższe 3 miesiące
- Timeline z możliwością przewijania
- Grupowanie po kategoriach, zadaniach lub osobach
- Zoom in/out dla różnego poziomu szczegółowości
- Przeciąganie wydarzeń na wykresie (drag and drop reschedule)
- Export wykresu do obrazu

### 5.7 Subskrypcja i płatności

US-034: Upgrade do wersji premium
Jako użytkownik darmowy
Chcę zakupić subskrypcję premium
Aby uzyskać dostęp do zaawansowanych funkcji

Kryteria akceptacji:
- Przycisk "Upgrade" widoczny w menu i przy limitach
- Strona z porównaniem planów (Free vs Premium)
- Lista wszystkich funkcji premium
- Cena subskrypcji (miesięczna/roczna)
- Integracja z systemem płatności (Stripe/PayPal)
- Potwierdzenie zakupu emailem
- Natychmiastowy dostęp do funkcji premium

US-035: Zarządzanie subskrypcją
Jako użytkownik premium
Chcę zarządzać moją subskrypcją
Aby kontrolować płatności i plan

Kryteria akceptacji:
- Zakładka "Subskrypcja" w ustawieniach
- Wyświetlanie aktualnego planu i daty odnowienia
- Możliwość zmiany metody płatności
- Możliwość zmiany z miesięcznej na roczną i odwrotnie
- Historia płatności z fakturami
- Opcja anulowania subskrypcji
- Informacja o dostępie do premium do końca opłaconego okresu

US-036: Anulowanie subskrypcji
Jako użytkownik premium
Chcę anulować subskrypcję
Aby przestać być obciążany płatnościami

Kryteria akceptacji:
- Przycisk "Anuluj subskrypcję" w ustawieniach
- Dialog z potwierdzeniem i powodem anulowania (opcjonalne)
- Informacja o utracie dostępu do funkcji premium
- Dostęp do premium zachowany do końca opłaconego okresu
- Email potwierdzający anulowanie
- Możliwość ponownej aktywacji w dowolnym momencie

### 5.8 Ustawienia i profil

US-037: Edycja profilu użytkownika
Jako użytkownik
Chcę edytować swój profil
Aby aktualizować dane osobowe

Kryteria akceptacji:
- Formularz z polami: imię, email, telefon (opcjonalnie)
- Możliwość zmiany hasła (z potwierdzeniem starego)
- Upload zdjęcia profilowego
- Zmiana języka interfejsu (jeśli dostępne tłumaczenia)
- Strefa czasowa
- Format daty
- Walidacja zmian
- Potwierdzenie emailem przy zmianie adresu

US-038: Ustawienia prywatności
Jako użytkownik
Chcę zarządzać ustawieniami prywatności
Aby kontrolować moje dane

Kryteria akceptacji:
- Zakładka "Prywatność" w ustawieniach
- Możliwość włączenia 2FA dla wrażliwych danych
- Historia logowań (ostatnie 10)
- Aktywne sesje z możliwością wylogowania
- Zgody na przetwarzanie danych (RODO)
- Export wszystkich danych (JSON/CSV)
- Przycisk "Usuń konto trwale"

US-039: Usunięcie konta
Jako użytkownik
Chcę trwale usunąć konto
Aby moje dane zostały usunięte z systemu

Kryteria akceptacji:
- Przycisk "Usuń konto" w ustawieniach prywatności
- Dialog z ostrzeżeniem o nieodwracalności akcji
- Wymagane potwierdzenie hasłem
- Checkbox "Rozumiem konsekwencje"
- Usunięcie wszystkich danych: profil, gospodarstwo, urządzenia, terminy, dokumenty
- Email potwierdzający usunięcie
- Możliwość anulowania w ciągu 7 dni (grace period)

US-040: Pomoc i FAQ
Jako użytkownik
Chcę mieć dostęp do pomocy
Aby nauczyć się korzystać z aplikacji

Kryteria akceptacji:
- Link "Pomoc" w menu
- Strona FAQ z najczęstszymi pytaniami
- Kategorie: Pierwsze kroki, Zarządzanie terminami, Powiadomienia, Dokumenty, Subskrypcja, Bezpieczeństwo
- Wyszukiwanie w FAQ
- Tutorial wideo (osadzony YouTube)
- Formularz kontaktowy do supportu
- Czas odpowiedzi: 24h w dni robocze

### 5.9 Scenariusze skrajne i obsługa błędów

US-041: Obsługa limitów wersji darmowej
Jako użytkownik darmowy osiągający limit
Chcę być jasno poinformowany o ograniczeniach
Aby zdecydować czy chcę upgrade

Kryteria akceptacji:
- Komunikat przy próbie dodania 6. zadania
- Komunikat przy próbie dodania 4. osoby
- Komunikat przy przekroczeniu 100 MB storage
- Każdy komunikat zawiera link do upgrade
- Możliwość usunięcia istniejących zadań aby zwolnić miejsce
- Licznik wykorzystania limitu widoczny w dashboardzie
- Powiadomienie email przy 90% wykorzystania limitu

US-042: Obsługa błędów połączenia
Jako użytkownik
Chcę być informowany o problemach technicznych
Aby wiedzieć że problem nie jest po mojej stronie

Kryteria akceptacji:
- Komunikat przy braku połączenia z internetem
- Retry button dla nieudanych akcji
- Zapisanie zmian lokalnie i synchronizacja po przywróceniu połączenia
- Informacja o tymczasowych problemach serwera
- Strona błędu 500 z przyciskiem "Zgłoś problem"
- Graceful degradation przy częściowych problemach

US-043: Obsługa konfliktów danych
Jako użytkownik
Chcę rozwiązywać konflikty przy jednoczesnej edycji
Aby nie stracić wprowadzonych zmian

Kryteria akceptacji:
- Wykrywanie konfliktów (inny użytkownik edytował tą samą pozycję)
- Dialog z pokazaniem obu wersji
- Opcje: przyjmij zmiany innych, zachowaj moje, scal ręcznie
- Możliwość podglądu różnic
- Zapisanie historii konfliktu (audit log dla premium)
- Powiadomienie o konflikcie do obu użytkowników

US-044: Walidacja danych wejściowych
Jako system
Chcę walidować wszystkie dane wprowadzane przez użytkownika
Aby zapewnić integralność danych

Kryteria akceptacji:
- Walidacja po stronie klienta (JavaScript) i serwera
- Komunikaty o błędach przy każdym polu formularza
- Blokada submit przy niewypełnionych wymaganych polach
- Sanityzacja danych (ochrona przed XSS)
- Limity długości tekstu (nazwa: 100 znaków, notatka: 500 znaków)
- Walidacja formatów (email, telefon, data)
- Komunikaty w języku użytkownika

US-045: Obsługa sesji wygasłej
Jako użytkownik z wygasłą sesją
Chcę móc kontynuować pracę po ponownym zalogowaniu
Aby nie stracić wprowadzonych danych

Kryteria akceptacji:
- Wykrywanie wygasłej sesji przy próbie akcji
- Modal z formularzem logowania
- Zachowanie kontekstu (strona, wypełniony formularz)
- Wykonanie akcji po ponownym zalogowaniu
- Możliwość porzucenia akcji
- Auto-save dla długich formularzy

## 6. Metryki sukcesu

### 6.1 Główne KPI

6.1.1 Liczba aktywnych użytkowników po 30 dniach od rejestracji
Definicja: Użytkownik aktywny to taki, który zalogował się przynajmniej raz w ciągu ostatnich 30 dni
Cel: 60% użytkowników aktywnych po 30 dniach
Pomiar: Tracking logowań w bazie danych
Częstotliwość: Tygodniowa

### 6.2 Metryki onboardingu

6.2.1 Czas do dodania pierwszego zadania
Definicja: Czas od rejestracji do dodania pierwszego zadania (szablonu)
Cel: 80% użytkowników dodaje pierwsze zadanie w ciągu 10 minut od rejestracji
Pomiar: Timestamp rejestracji vs timestamp dodania pierwszego zadania
Częstotliwość: Dzienna

6.2.2 Completion rate onboardingu
Definicja: Procent użytkowników, którzy ukończyli cały proces onboardingu
Cel: 70% completion rate
Pomiar: Tracking kolejnych kroków onboardingu
Częstotliwość: Dzienna

### 6.3 Metryki zaangażowania

6.3.1 Średnia liczba dodanych zadań na użytkownika
Definicja: Średnia liczba zadań (szablonów) dodanych przez aktywnego użytkownika
Cel: 3.5 zadań w pierwszym miesiącu
Pomiar: Suma zadań podzielona przez liczbę użytkowników
Częstotliwość: Miesięczna

6.3.2 Średnia liczba potwierdzeń wydarzeń tygodniowo
Definicja: Ile wydarzeń jest potwierdzanych przez użytkownika w tygodniu
Cel: 80% wydarzeń potwierdzonych w czasie (przed lub w dniu terminu)
Pomiar: Timestamp potwierdzenia vs dueDate wydarzenia
Częstotliwość: Tygodniowa

6.3.3 Częstotliwość logowań
Definicja: Ile razy w miesiącu użytkownik loguje się do aplikacji
Cel: Minimum 4 logowania/miesiąc na aktywnego użytkownika
Pomiar: Counting unique session starts
Częstotliwość: Miesięczna

### 6.4 Metryki retencji

6.4.1 Retention rate po 3 miesiącach
Definicja: Procent użytkowników, którzy wracają po 3 miesiącach od rejestracji
Cel: 40% retention rate
Pomiar: Cohort analysis - użytkownicy zalogowani w 90. dniu od rejestracji
Częstotliwość: Miesięczna

6.4.2 Churn rate
Definicja: Procent użytkowników, którzy przestali korzystać z aplikacji
Cel: Poniżej 10% miesięcznego churn
Pomiar: Użytkownicy bez logowania przez 60 dni
Częstotliwość: Miesięczna

### 6.5 Metryki konwersji

6.5.1 Conversion rate (free → premium)
Definicja: Procent użytkowników darmowych, którzy wykupili subskrypcję
Cel: 5% w pierwszym roku
Pomiar: Liczba subskrypcji / liczba wszystkich użytkowników
Częstotliwość: Miesięczna

6.5.2 Czas do konwersji
Definicja: Średni czas od rejestracji do zakupu premium
Cel: 45 dni
Pomiar: Timestamp rejestracji vs timestamp zakupu
Częstotliwość: Miesięczna

6.5.3 Trigger konwersji
Definicja: Moment/event, który najbardziej wpływa na decyzję o upgrade
Cel: Identyfikacja top 3 triggerów
Pomiar: Tracking ostatniej akcji przed upgrade (np. osiągnięcie limitu, próba użycia funkcji premium)
Częstotliwość: Miesięczna

### 6.6 Metryki użytkowania funkcji

6.6.1 Adoption rate poszczególnych funkcji
Definicja: Procent użytkowników korzystających z danej funkcji
Cel: 
- Dodawanie zadań: 90%
- Tworzenie wydarzeń: 85%
- Potwierdzanie wydarzeń: 75%
- Edycja wydarzeń: 50%
Pomiar: Liczba użytkowników używających funkcji / wszyscy użytkownicy
Częstotliwość: Miesięczna

6.6.2 Liczba członków gospodarstwa
Definicja: Średnia liczba osób w gospodarstwie domowym
Cel: 2.5 osoby w free, 4+ w premium
Pomiar: Suma członków / liczba gospodarstw
Częstotliwość: Miesięczna

### 6.7 Metryki jakości

6.7.1 Liczba zgłoszeń błędów
Definicja: Ile błędów jest zgłaszanych przez użytkowników tygodniowo
Cel: Poniżej 5 unique bugs tygodniowo po pierwszym miesiącu
Pomiar: Tickety w systemie support
Częstotliwość: Tygodniowa

6.7.2 Średni czas rozwiązania problemu
Definicja: Czas od zgłoszenia do rozwiązania problemu
Cel: 24h dla critical, 3 dni dla medium, 7 dni dla low
Pomiar: Timestamp utworzenia vs zamknięcia ticketu
Częstotliwość: Tygodniowa

6.7.3 Customer Satisfaction Score (CSAT)
Definicja: Ocena satysfakcji użytkowników ze wsparcia
Cel: 4.5/5.0 średnia ocena
Pomiar: Survey po rozwiązaniu problemu
Częstotliwość: Ciągła

6.7.4 Net Promoter Score (NPS)
Definicja: Skłonność użytkowników do rekomendacji aplikacji
Cel: NPS > 30 w pierwszym roku
Pomiar: Ankieta co 3 miesiące "Jak prawdopodobne że polecisz aplikację?"
Częstotliwość: Kwartalna

### 6.8 Metryki panelu administratora

6.8.1 Adoption rate funkcji administratora
Definicja: Procent administratorów korzystających z zaawansowanych funkcji panelu
Cel: 
- Zarządzanie członkami: 95%
- Przegląd statystyk: 80%
- Masowe operacje: 60%
- Raporty: 40%
Pomiar: Tracking użycia funkcji w panelu administratora
Częstotliwość: Miesięczna

6.8.2 Efektywność zarządzania gospodarstwem
Definicja: Średnia liczba członków i zadań zarządzanych przez administratora
Cel: 
- Free plan: 3 członków, 5 zadań
- Premium: 6 członków, 20 zadań
Pomiar: Średnie wartości na gospodarstwo z rolą administratora
Częstotliwość: Miesięczna

6.8.3 Czas rozwiązywania konfliktów wydarzeń
Definicja: Średni czas od wykrycia konfliktu do jego rozwiązania
Cel: < 24 godziny dla konfliktów krytycznych
Pomiar: Timestamp wykrycia vs rozwiązania konfliktu
Częstotliwość: Tygodniowa

6.8.4 Wykorzystanie funkcji masowych
Definicja: Procent administratorów używających operacji masowych
Cel: 50% administratorów z >10 zadaniami używa funkcji masowych
Pomiar: Tracking użycia bulk operations
Częstotliwość: Miesięczna

6.8.5 Retention administratorów vs domowników
Definicja: Porównanie retention rate między rolami
Cel: Administratorzy 60% retention vs 40% domownicy (3 miesiące)
Pomiar: Cohort analysis pogrupowany po rolach
Częstotliwość: Miesięczna

### 6.9 Metryki systemu dla System Developer

6.9.1 Wzrost liczby gospodarstw
Definicja: Liczba nowych gospodarstw utworzonych w okresie
Cel: 
- Miesiąc 1-3: 50 nowych gospodarstw/miesiąc
- Miesiąc 4-12: 200 nowych gospodarstw/miesiąc
Pomiar: Count nowych gospodarstw w okresie
Częstotliwość: Tygodniowa

6.9.2 Aktywność gospodarstw
Definicja: Procent gospodarstw aktywnych w ostatnich 30 dniach
Cel: 85% gospodarstw aktywnych
Pomiar: Gospodarstwa z aktywnością (logowanie, akcje) w ostatnich 30 dniach
Częstotliwość: Tygodniowa

6.9.3 Wykorzystanie zasobów systemu
Definicja: Obciążenie serwerów i baz danych
Cel: 
- CPU utilization < 70%
- Memory usage < 80%
- Database response time < 100ms
Pomiar: Monitoring infrastruktury
Częstotliwość: Ciągła

6.9.4 Revenue metrics (MRR/ARR)
Definicja: Miesięczne i roczne przychody z subskrypcji
Cel:
- MRR: $10,000 w pierwszym roku
- Churn rate < 5% miesięcznie
- Average Revenue Per User (ARPU): $15/miesiąc
Pomiar: Stripe/PayPal analytics + własne kalkulacje
Częstotliwość: Miesięczna

6.9.5 System reliability metrics
Definicja: Metryki niezawodności i wydajności systemu
Cel:
- Uptime: 99.9%
- Mean Time to Recovery (MTTR): < 2 godziny
- Alert response time: < 15 minut
Pomiar: Monitoring tools (Datadog, New Relic)
Częstotliwość: Ciągła

6.9.6 Support effectiveness
Definicja: Efektywność wsparcia technicznego
Cel:
- First Response Time: < 2 godziny
- Resolution Time: < 24 godziny (critical), < 3 dni (medium)
- Customer Satisfaction Score: > 4.5/5
Pomiar: Ticketing system analytics
Częstotliwość: Tygodniowa

6.9.7 Platform adoption metrics
Definicja: Adopcja funkcji na poziomie całej platformy
Cel:
- Feature adoption rate: > 60% dla nowych funkcji w 3 miesiące
- API usage growth: 20% miesięcznie
- Mobile vs desktop usage ratio tracking
Pomiar: Feature usage analytics + API metrics
Częstotliwość: Miesięczna

### 6.10 Metryki techniczne

6.10.1 Uptime
Definicja: Procent czasu, w którym aplikacja jest dostępna
Cel: 99.5% uptime
Pomiar: Monitoring serwera
Częstotliwość: Ciągła

6.10.2 Średni czas ładowania strony
Definicja: Time to Interactive dla kluczowych stron
Cel: < 2 sekundy dla dashboard, < 3 sekundy dla innych stron
Pomiar: Real User Monitoring (RUM)
Częstotliwość: Ciągła

6.10.3 Error rate
Definicja: Procent requestów kończących się błędem 5xx
Cel: < 0.1% error rate
Pomiar: Server logs
Częstotliwość: Ciągła

### 6.11 Metodologia pomiaru

Wszystkie metryki będą mierzone za pomocą:
- Google Analytics 4 dla behawioralnych metryk użytkownika
- Własne eventy w bazie danych dla biznesowych KPI
- Stripe/PayPal dashboard dla metryk płatności
- Monitoring serwera dla metryk technicznych

Raporty będą generowane:
- Daily dashboard dla kluczowych metryk (logowania, rejestracje, aktywność)
- Weekly report dla metryk engagement i funkcjonalności
- Monthly report dla metryk biznesowych i retention
- Quarterly review dla strategicznych decyzji produktowych

### 6.12 Kryteria sukcesu MVP

MVP zostanie uznane za sukces jeśli po 3 miesiącach od uruchomienia:
- Osiągniemy 1000 zarejestrowanych użytkowników
- 40% retention rate po 3 miesiącach
- 60% aktywnych użytkowników po 30 dniach
- Minimum 3% conversion rate free → premium
- NPS > 20
- < 0.5% error rate
- Średnio 3+ zadań na aktywnego użytkownika
- Średnio 5+ wydarzeń utworzonych na aktywnego użytkownika

Jeśli te cele zostaną osiągnięte, będziemy kontynuować rozwój produktu zgodnie z planem post-MVP.


### 7. Pomysł na użycie AI

OCR dokumentów - Automatyczne wyciąganie kluczowych informacji (data, kwota, nazwa usługi, treść instrukcji dla konkretnej czynności).