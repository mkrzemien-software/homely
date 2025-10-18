# Podsumowanie wymagań projektowych - Aplikacja do zarządzania terminami

Na podstawie przeprowadzonej analizy przygotowałem kompleksowe podsumowanie, które posłuży jako fundament do stworzenia pełnego dokumentu PRD.

---

## 1. PODSTAWOWE INFORMACJE O PRODUKCIE

**Nazwa projektu:** Aplikacja webowa do zarządzania terminami serwisów i wizyt

**Grupa docelowa:** Domownicy zarządzający gospodarstwem domowym

**Problem do rozwiązania:** Brak centralnego systemu do zarządzania terminami serwisów urządzeń domowych i wizyt dla całej rodziny, co prowadzi do przegapionych terminów i nieefektywnego zarządzania obowiązkami domowymi.

---

## 2. ZAKRES MVP

### Kategorie w pierwszej wersji:
- Przeglądy techniczne
- Wywóz śmieci
- Wizyty medyczne domowników

### Kluczowe funkcjonalności MVP:
1. **Zarządzanie urządzeniami/wizytami**
   - Dodawanie, edytowanie, usuwanie pozycji
   - Przypisywanie do członków gospodarstwa
   - Ustawianie interwałów czasowych

2. **System użytkowników i gospodarstw**
   - Role: Administrator, Domownik, Dashboard (tylko odczyt)
   - Zarządzanie członkami gospodarstwa domowego
   - Limity dla wersji darmowej: 5 urządzeń, 3 osoby

3. **Potwierdzanie i zarządzanie terminami**
   - Potwierdzenie wykonania
   - Przełożenie terminu
   - Usunięcie/edycja terminu

---

## 3. KLUCZOWE ŚCIEŻKI UŻYTKOWNIKA

### Momenty krytyczne dla sukcesu:
1. **Onboarding** - Dodanie pierwszego urządzenia
2. **First value** - Dodanie pierwszego terminu
3. **Retention** - Łatwość potwierdzenia/przełożenia/edycji terminu

---

## 4. MODEL BIZNESOWY

### Wersja darmowa (Freemium):
- Limit: 5 urządzeń
- Limit: 3 osoby w gospodarstwie
- Podstawowy widok terminów

### Wersja premium (Subskrypcja):
- Nielimitowana liczba urządzeń
- Nielimitowana liczba osób
- System powiadomień email z konfiguracją częstotliwości
- Zarządzanie dokumentacją (nielimitowany storage)
- Historia serwisów
- Raporty kosztów
- Ulepszone wykresy
- Zaawansowane analizy

---

## 5. METRYKI SUKCESU

**Główny KPI:**
- Liczba aktywnych użytkowników po 30 dniach od rejestracji

**Dodatkowe metryki do monitorowania:**
- Średnia liczba dodanych urządzeń/wizyt na użytkownika
- Retention rate (użytkownicy wracający po 3 miesiącach)
- Conversion rate (free → premium)
- Średni czas do dodania pierwszego urządzenia (onboarding success)

---

## 6. KLUCZOWE WYMAGANIA FUNKCJONALNE

### System ról i uprawnień:
- **Administrator:** Pełne uprawnienia (dodawanie użytkowników, wszystkich urządzeń, zarządzanie subskrypcją)
- **Domownik:** Odczyt wszystkich, edycja przypisanych do siebie
- **Dashboard:** widok stworzony dla monitora zawieszonego na ścianie

---

## 7. WYMAGANIA TECHNICZNE I BEZPIECZEŃSTWO

### Zgodność z przepisami:
- RODO compliance
- Szyfrowanie danych (AES-256, TLS 1.3)
- Możliwość eksportu danych użytkownika
- Opcja całkowitego usunięcia konta

### Szczególna ochrona danych medycznych:
- Dodatkowa warstwa zabezpieczeń
- Rozważenie 2FA dla wrażliwych informacji

---

## 8. WYŁĄCZENIA Z ZAKRESU MVP

**Nie będzie w pierwszej wersji:**
- System powiadomień email (zostanie dodany w kolejnych iteracjach)
- Zarządzanie dokumentacją (upload i storage plików)
- Integracje z zewnętrznymi kalendarzami (Google Calendar, Outlook)
- OCR do rozpoznawania dat z dokumentów
- Machine Learning do przewidywania terminów
- Aplikacje mobilne (native iOS/Android)

---