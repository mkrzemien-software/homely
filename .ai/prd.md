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
- Zarządzanie wszystkimi urządzeniami i terminami
- Zarządzanie subskrypcją
- Pełny dostęp do dokumentacji

Domownik:
- Odczyt wszystkich urządzeń i terminów w gospodarstwie
- Edycja i zarządzanie urządzeniami/terminami przypisanymi do siebie
- Upload dokumentacji dla przypisanych pozycji
- Potwierdzanie i przełożenie własnych terminów

Dashboard (tylko odczyt):
- Widok terminów bez możliwości edycji
- Optymalizacja dla monitora na ścianie
- Uproszczony interfejs z kluczowymi informacjami

#### 3.1.3 Zarządzanie gospodarstwem
- Limit wersji darmowej: 3 osoby w gospodarstwie
- Brak limitu w wersji premium
- Możliwość przypisywania kolorów/ikon członkom rodziny dla lepszej wizualizacji

### 3.2 Zarządzanie urządzeniami i wizytami

#### 3.2.1 Dodawanie pozycji
Dla każdej kategorii (przeglądy techniczne, wywóz śmieci, wizyty medyczne):
- Nazwa urządzenia/typu wizyty
- Kategoria
- Przypisanie do członka gospodarstwa (odpowiedzialny)
- Interwał czasowy (dni/tygodnie/miesiące)
- Data ostatniego serwisu/wizyty
- Notatki dodatkowe
- Priorytet (niski/średni/wysoki)

#### 3.2.2 Edycja i usuwanie
- Możliwość modyfikacji wszystkich pól
- Opcja archiwizacji zamiast usunięcia (zachowanie historii)
- Potwierdzenie przed ostatecznym usunięciem

#### 3.2.3 Limity wersji darmowej
- Maksymalnie 5 urządzeń/wizyt łącznie
- Komunikat o limicie przy próbie dodania kolejnej pozycji
- Propozycja upgrade'u do wersji premium

### 3.3 System terminów

#### 3.3.1 Generowanie terminów
- Automatyczne wyliczanie kolejnych terminów na podstawie interwału
- Wyświetlanie nadchodzących terminów na dashboard
- Oznaczenie przekroczonych terminów

#### 3.3.2 Potwierdzanie wykonania
- Możliwość potwierdzenia wykonania serwisu/wizyty
- Opcjonalne dodanie notatki o wykonaniu
- Upload zdjęcia/dokumentu potwierdzającego
- Automatyczne ustawienie nowego terminu

#### 3.3.3 Przełożenie terminu
- Możliwość przesunięcia terminu o określoną liczbę dni
- Wymagane uzasadnienie/notatka
- Historia przełożeń (dostępna w premium)

#### 3.3.4 Usunięcie terminu
- Możliwość usunięcia pojedynczego terminu
- Zachowanie urządzenia/wizyty w systemie
- Wygenerowanie nowego terminu na podstawie daty usunięcia lub zachowanie oryginalnego harmonogramu

### 3.4 Widoki i nawigacja

#### 3.4.1 Dashboard główny
- Lista nadchodzących terminów (7 dni)
- Wyróżnienie terminów przekroczonych
- Szybkie akcje: potwierdź, przełóż, edytuj
- Statystyki: liczba urządzeń, wykorzystanie limitu

#### 3.4.2 Widok kalendarza
- Miesięczny widok terminów
- Kolorowe oznaczenia kategorii
- Możliwość kliknięcia w termin i wykonania akcji

#### 3.4.3 Lista urządzeń/wizyt
- Wszystkie pozycje pogrupowane po kategorii
- Sortowanie: po dacie, nazwie, priorytecie
- Filtrowanie po kategorii, osobie odpowiedzialnej
- Szybka edycja inline

#### 3.4.4 Widok Dashboard (monitor)
- Uproszczony, czytelny interfejs
- Duża czcionka
- Wyświetlanie tylko najbliższych 5 terminów
- Auto-refresh co 5 minut

#### 3.4.5 Panel Administratora
Specjalne widoki dostępne tylko dla użytkowników z rolą Administrator:

##### 3.4.5.1 Zarządzanie gospodarstwem domowym
- Edycja nazwy i adresu gospodarstwa
- Przegląd statystyk gospodarstwa (liczba członków, urządzeń, terminów)
- Historia zmian w gospodarstwie (audit log)
- Ustawienia domyślne dla nowych urządzeń/wizyt
- Archiwum usuniętych pozycji z możliwością przywrócenia

##### 3.4.5.2 Zarządzanie członkami gospodarstwa
- Lista wszystkich członków z rolami i statusami
- Formularz dodawania nowych członków z wysyłką zaproszeń
- Edycja ról i uprawnień istniejących członków
- Historia aktywności członków (ostatnie logowanie, akcje)
- Usuwanie członków z reassignment ich terminów
- Zarządzanie zaproszeniami (pending, expired, resend)

##### 3.4.5.3 Centralne zarządzanie terminami i urządzeniami
- Widok globalny wszystkich urządzeń/wizyt w gospodarstwie
- Możliwość edycji terminów wszystkich członków
- Masowe operacje (zmiana interwałów, reassignment odpowiedzialnych)
- Przegląd konfliktów terminów i ich rozwiązywanie
- Konfiguracja priorytetów i kategorii
- Import/export danych urządzeń (CSV)

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
- Analiza efektywności zarządzania terminami
- Statystyki potwierdzanych vs przegapionych terminów
- Przegląd najczęściej używanych urządzeń/kategorii
- Export raportów dla zewnętrznych systemów księgowych

#### 3.4.6 Panel System Developer (Super Admin)
Widoki dostępne wyłącznie dla twórców oprogramowania i administratorów systemu:

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

US-050: Masowe zarządzanie urządzeniami
Jako administrator
Chcę wykonywać operacje na wielu urządzeniach jednocześnie
Aby efektywnie zarządzać dużym gospodarstwem

Kryteria akceptacji:
- Selekcja wielu urządzeń (checkbox)
- Masowa zmiana osoby odpowiedzialnej
- Masowa aktualizacja interwałów
- Masowe dodanie etykiet/kategorii
- Masowe archiwizowanie/usuwanie
- Potwierdzenie przed wykonaniem operacji masowej
- Preview zmian przed aplikacją

US-051: Zarządzanie konfliktami terminów
Jako administrator
Chcę identyfikować i rozwiązywać konflikty terminów
Aby zapewnić płynne funkcjonowanie gospodarstwa

Kryteria akceptacji:
- Widok wszystkich konfliktów terminów (ten sam dzień, ta sama osoba)
- Automatyczne wykrywanie nakładających się terminów
- Sugestie rozwiązania (przesunięcie, zmiana odpowiedzialnego)
- Możliwość ręcznej zmiany terminów w konflikcie
- Powiadomienia członków o zmianach
- Historia rozwiązanych konfliktów

### 5.2.2 Panel administratora - subskrypcja i finanse

US-052: Przegląd wykorzystania planu
Jako administrator
Chcę monitorować wykorzystanie limitów mojego planu
Aby planować ewentualny upgrade

Kryteria akceptacji:
- Dashborad z wykorzystaniem: członkowie, urządzenia, storage
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
Aby optymalizować zarządzanie terminami

Kryteria akceptacji:
- Raport potwierdzanych vs przegapionych terminów
- Analiza najbardziej problematycznych urządzeń
- Statystyki aktywności członków
- Średnie czasy reakcji na terminy
- Identyfikacja wzorców sezonowych
- Rekomendacje optymalizacji

US-056: Dashboard analityczny administratora
Jako administrator
Chcę mieć szybki dostęp do kluczowych metryk
Aby na bieżąco monitorować stan gospodarstwa

Kryteria akceptacji:
- Widget z nadchodzącymi terminami krytycznymi
- Mierniki wykorzystania planu
- Alerty o problemach (przekroczone terminy, konflikty)
- Statystyki aktywności w czasie rzeczywistym
- Szybkie akcje (dodaj członka, wyślij przypomnienie)
- Personalizacja widżetów na dashboardzie

### 5.2.4 Panel System Developer - zarządzanie systemem

US-057: Przegląd wszystkich gospodarstw w systemie
Jako System Developer
Chcę widzieć listę wszystkich gospodarstw w systemie
Aby monitorować aktywność platformy i udzielać wsparcia

Kryteria akceptacji:
- Lista gospodarstw z podstawowymi statystykami (liczba członków, urządzeń, ostatnia aktywność)
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

### 5.3 Zarządzanie urządzeniami i wizytami

US-009: Dodawanie urządzenia/wizyty
Jako domownik
Chcę dodać urządzenie lub wizytę do systemu
Aby móc śledzić terminy związane z tym elementem

Kryteria akceptacji:
- Formularz zawiera: nazwę, kategorię (dropdown), osobę odpowiedzialną, interwał (liczba + jednostka), datę ostatniego serwisu, priorytet, notatki
- Walidacja wszystkich wymaganych pól
- W wersji darmowej limit 5 pozycji łącznie
- Komunikat o dodaniu z wyliczonym pierwszym terminem
- Automatyczne przekierowanie do listy urządzeń
- Możliwość natychmiastowego dodania kolejnego

US-010: Edycja urządzenia/wizyty
Jako domownik
Chcę edytować dane urządzenia/wizyty
Aby aktualizować informacje w systemie

Kryteria akceptacji:
- Dostęp do edycji tylko dla przypisanych pozycji (lub administrator)
- Formularz z wypełnionymi aktualnymi danymi
- Możliwość zmiany wszystkich pól
- Zmiana interwału nie wpływa na historię, tylko przyszłe terminy
- Potwierdzenie zapisania zmian
- Opcja anulowania bez zapisywania

US-011: Usuwanie urządzenia/wizyty
Jako domownik
Chcę usunąć urządzenie/wizytę z systemu
Aby pozbyć się nieaktualnych pozycji

Kryteria akceptacji:
- Przycisk usunięcia przy każdej pozycji
- Dialog potwierdzenia z opcjami: "Usuń całkowicie" lub "Archiwizuj"
- Usunięcie całkowite usuwa wszystkie terminy i dokumenty
- Archiwizacja zachowuje historię (funkcja premium)
- Zwolnienie miejsca w limicie (wersja darmowa)
- Komunikat potwierdzający

US-012: Filtrowanie i sortowanie listy
Jako użytkownik
Chcę filtrować i sortować listę urządzeń/wizyt
Aby szybko znaleźć interesującą mnie pozycję

Kryteria akceptacji:
- Filtry: wszystkie/kategoria/osoba odpowiedzialna/priorytet
- Sortowanie: po nazwie/dacie najbliższego terminu/priorytecie/dacie dodania
- Możliwość łączenia filtrów
- Licznik wyświetlonych pozycji
- Resetowanie filtrów jednym przyciskiem
- Zapisanie wybranych filtrów w sesji

### 5.4 Zarządzanie terminami

US-013: Wyświetlanie nadchodzących terminów
Jako użytkownik
Chcę widzieć nadchodzące terminy na dashboardzie
Aby być na bieżąco z obowiązkami

Kryteria akceptacji:
- Lista terminów na najbliższe 7 dni
- Sortowanie chronologiczne
- Wyróżnienie kolorystyczne: przekroczony (czerwony), dzisiaj (pomarańczowy), nadchodzące (zielony)
- Wyświetlanie: nazwa, kategoria, osoba odpowiedzialna, data
- Szybkie akcje: potwierdź, przełóż, edytuj
- Odświeżanie w czasie rzeczywistym

US-014: Potwierdzanie wykonania terminu
Jako domownik
Chcę potwierdzić wykonanie serwisu/wizyty
Aby system wygenerował kolejny termin

Kryteria akceptacji:
- Przycisk "Potwierdź wykonanie" przy terminie
- Opcjonalne pole na notatkę o wykonaniu
- Możliwość załączenia zdjęcia/dokumentu
- Automatyczne wyliczenie kolejnego terminu na podstawie interwału
- Przesunięcie terminu do historii (premium) lub oznaczenie jako wykonany
- Powiadomienie innych członków gospodarstwa o wykonaniu

US-015: Przełożenie terminu
Jako domownik
Chcę przełożyć termin na inny dzień
Aby dostosować harmonogram do sytuacji

Kryteria akceptacji:
- Przycisk "Przełóż" przy terminie
- Kalendarz do wyboru nowej daty lub pole z liczbą dni przesunięcia
- Wymagane pole z uzasadnieniem/notatką
- Historia przełożeń widoczna w wersji premium
- Potwierdzenie nowej daty
- Email do wszystkich członków o przesunięciu

US-016: Edycja pojedynczego terminu
Jako domownik
Chcę edytować pojedynczy termin bez zmiany ustawień urządzenia
Aby dostosować konkretny termin

Kryteria akceptacji:
- Możliwość zmiany daty
- Możliwość zmiany osoby odpowiedzialnej
- Możliwość dodania notatki
- Nie wpływa na interwał ani przyszłe terminy
- Potwierdzenie zmian
- Powiadomienie o zmianie

US-017: Usuwanie pojedynczego terminu
Jako domownik
Chcę usunąć pojedynczy termin
Aby pominąć jednorazowo serwis/wizytę

Kryteria akceptacji:
- Przycisk "Usuń termin" przy terminie
- Dialog potwierdzenia
- Opcje: "Usuń i zachowaj harmonogram" lub "Usuń i wygeneruj nowy od dzisiaj"
- Urządzenie pozostaje w systemie
- Historia usunięcia (premium)
- Email potwierdzający

US-018: Widok kalendarza miesięcznego
Jako użytkownik
Chcę widzieć terminy w formie kalendarza
Aby mieć lepszy przegląd harmonogramu

Kryteria akceptacji:
- Standardowy widok kalendarzowy z dniami miesiąca
- Oznaczenia kolorowe według kategorii
- Możliwość przejścia do poprzedniego/następnego miesiąca
- Kliknięcie w termin otwiera szczegóły i akcje
- Licznik terminów w danym dniu jeśli więcej niż 3
- Legenda kolorów kategorii

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
- Krok 2: Dodaj pierwsze urządzenie/wizytę
- Możliwość pominięcia onboardingu
- Wskazówki kontekstowe (tooltips) przy pierwszym użyciu funkcji
- Checkbox "Nie pokazuj ponownie"

### 5.6 Funkcje premium

US-029: Historia serwisów
Jako użytkownik premium
Chcę przeglądać historię wszystkich wykonanych serwisów
Aby analizować częstotliwość i koszty

Kryteria akceptacji:
- Zakładka "Historia" w menu (tylko premium)
- Lista wszystkich potwierdzonych terminów
- Filtry: data, kategoria, urządzenie, osoba
- Wyświetlanie: data wykonania, notatki, załączone dokumenty
- Export do CSV lub PDF
- Statystyki: liczba serwisów w okresie, średni interwał
- Wykresy trendu częstotliwości

US-030: Dodawanie kosztów do serwisów
Jako użytkownik premium
Chcę dodawać koszty do wykonanych serwisów
Aby śledzić wydatki gospodarstwa

Kryteria akceptacji:
- Pole "Koszt" przy potwierdzaniu wykonania terminu
- Waluta zgodna z ustawieniami użytkownika
- Możliwość dodania kosztów później (edycja historii)
- Kategoryzacja kosztów: części, robocizna, przejazd, inne
- Załączanie faktury jako dokumentu
- Suma kosztów widoczna przy urządzeniu

US-031: Raporty kosztów
Jako użytkownik premium
Chcę generować raporty wydatków
Aby kontrolować budżet gospodarstwa

Kryteria akceptacji:
- Zakładka "Raporty" w menu (tylko premium)
- Wybór okresu: miesiąc, kwartał, rok, własny zakres
- Suma wszystkich kosztów w okresie
- Rozbicie po kategoriach
- Rozbicie po urządzeniach (TOP 5 najdroższe)
- Wykres słupkowy wydatków miesięcznych
- Porównanie rok do roku
- Export raportu do PDF

US-032: Zaawansowane analizy
Jako użytkownik premium
Chcę mieć dostęp do analiz predykcyjnych
Aby planować przyszłe wydatki

Kryteria akceptacji:
- Zakładka "Analizy" w menu (tylko premium)
- Prognoza wydatków na najbliższe 6 miesięcy
- Identyfikacja najbardziej awaryjnych urządzeń
- Sugestie optymalizacji kosztów
- Heatmapa intensywności terminów
- Analiza obciążenia poszczególnych domowników
- Wykresy trendów długoterminowych

US-033: Ulepszone wykresy
Jako użytkownik premium
Chcę wizualizować terminy na osi czasu
Aby lepiej planować harmonogram

Kryteria akceptacji:
- Wykres Gantta z terminami na najbliższe 3 miesiące
- Timeline z możliwością przewijania
- Grupowanie po kategoriach lub osobach
- Zoom in/out dla różnego poziomu szczegółowości
- Przeciąganie terminów na wykresie (drag and drop reschedule)
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
- Komunikat przy próbie dodania 6. urządzenia
- Komunikat przy próbie dodania 4. osoby
- Komunikat przy przekroczeniu 100 MB storage
- Każdy komunikat zawiera link do upgrade
- Możliwość usunięcia istniejących pozycji aby zwolnić miejsce
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

6.2.1 Czas do dodania pierwszego urządzenia
Definicja: Czas od rejestracji do dodania pierwszego urządzenia/wizyty
Cel: 80% użytkowników dodaje pierwsze urządzenie w ciągu 10 minut od rejestracji
Pomiar: Timestamp rejestracji vs timestamp dodania pierwszego urządzenia
Częstotliwość: Dzienna

6.2.2 Completion rate onboardingu
Definicja: Procent użytkowników, którzy ukończyli cały proces onboardingu
Cel: 70% completion rate
Pomiar: Tracking kolejnych kroków onboardingu
Częstotliwość: Dzienna

### 6.3 Metryki zaangażowania

6.3.1 Średnia liczba dodanych urządzeń/wizyt na użytkownika
Definicja: Średnia liczba pozycji dodanych przez aktywnego użytkownika
Cel: 3.5 pozycji w pierwszym miesiącu
Pomiar: Suma urządzeń/wizyt podzielona przez liczbę użytkowników
Częstotliwość: Miesięczna

6.3.2 Średnia liczba potwierdzeń terminów tygodniowo
Definicja: Ile terminów jest potwierdzanych przez użytkownika w tygodniu
Cel: 80% terminów potwierdzonych w czasie (przed lub w dniu terminu)
Pomiar: Timestamp potwierdzenia vs data terminu
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
- Dodawanie urządzeń: 90%
- Potwierdzanie terminów: 75%
- Edycja terminów: 50%
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
Definicja: Średnia liczba członków i urządzeń zarządzanych przez administratora
Cel: 
- Free plan: 3 członków, 5 urządzeń
- Premium: 6 członków, 20 urządzeń
Pomiar: Średnie wartości na gospodarstwo z rolą administratora
Częstotliwość: Miesięczna

6.8.3 Czas rozwiązywania konfliktów terminów
Definicja: Średni czas od wykrycia konfliktu do jego rozwiązania
Cel: < 24 godziny dla konfliktów krytycznych
Pomiar: Timestamp wykrycia vs rozwiązania konfliktu
Częstotliwość: Tygodniowa

6.8.4 Wykorzystanie funkcji masowych
Definicja: Procent administratorów używających operacji masowych
Cel: 50% administratorów z >10 urządzeniami używa funkcji masowych
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
- Średnio 3+ urządzeń/wizyt na aktywnego użytkownika

Jeśli te cele zostaną osiągnięte, będziemy kontynuować rozwój produktu zgodnie z planem post-MVP.


### 7. Pomysł na użycie AI

OCR dokumentów - Automatyczne wyciąganie kluczowych informacji (data, kwota, nazwa usługi, treść instrukcji dla konkretnej czynności).