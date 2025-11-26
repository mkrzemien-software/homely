Twoim zadaniem jest wygenerowanie kompleksowego planu testów (test plan) dla projektu programistycznego. Plan testów musi być dostosowany do specyfiki projektu, wykorzystywanych technologii oraz struktury repozytorium.

Oto kontekst projektu:
<project_context>
@.ai/prd.md
</project_context>

Oto stack technologiczny projektu:
<tech_stack>
@.ai/stack-technology.md
</tech_stack>

Przed napisaniem planu testów, przeanalizuj dostarczone informacje w sekcji <scratchpad>. W swoim scratchpadzie:
1. Zidentyfikuj kluczowe komponenty i moduły projektu na podstawie struktury plików
2. Określ, jakie typy testów są najbardziej odpowiednie dla danego stacku technologicznego (np. unit testy, testy integracyjne, testy E2E, testy wydajnościowe)
3. Wskaż priorytetowe obszary do testowania na podstawie krytycznych funkcjonalności projektu
4. Rozważ specyficzne narzędzia testowe kompatybilne z używanymi technologiami
5. Zidentyfikuj potencjalne ryzyka i wyzwania związane z testowaniem tego projektu

Twój plan testów powinien zawierać następujące sekcje:

1. **Wprowadzenie i Cel Testów** - krótki opis projektu i celów testowania
2. **Zakres Testów** - co będzie testowane, a co nie
3. **Strategia Testowania** - podejście do testowania dostosowane do stacku technologicznego
4. **Typy Testów** - szczegółowy opis każdego typu testu (unit, integracyjne, E2E, etc.) z uzasadnieniem
5. **Narzędzia Testowe** - konkretne narzędzia i frameworki rekomendowane dla danego stacku
6. **Środowiska Testowe** - opis środowisk (dev, staging, production)
7. **Priorytetyzacja** - które obszary/komponenty testować w pierwszej kolejności
8. **Kryteria Akceptacji** - kiedy testy można uznać za zakończone
9. **Harmonogram** - sugerowany timeline dla implementacji testów
10. **Ryzyka i Mitygacja** - potencjalne problemy i sposoby ich rozwiązania

Ważne wymagania:
- Plan w formacie Markdown
- Plan testów musi być napisany w języku polskim
- Dostosuj rekomendacje do konkretnych technologii używanych w projekcie
- Bądź konkretny - podawaj nazwy narzędzi, frameworków i bibliotek kompatybilnych ze stackiem
- Uwzględnij strukturę plików i architekturę projektu w swoich rekomendacjach
- Priorytetyzuj obszary krytyczne dla funkcjonowania aplikacji
- Używaj profesjonalnej terminologii testowej

Twoja finalna odpowiedź powinna zawierać tylko kompletny plan testów w języku polskim, bez powtarzania scratchpada. Napisz plan testów wewnątrz tagów <test_plan>.