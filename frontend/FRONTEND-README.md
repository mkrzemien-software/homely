# Homely Frontend - Theme System

System zarządzania stylami dla aplikacji Homely z obsługą jasnego i ciemnego motywu.

## 🚀 Uruchomienie

```bash
# Instalacja zależności
npm install

# Uruchomienie serwera deweloperskiego
npm start

# Aplikacja dostępna na http://localhost:4200
```

## 🎨 System Motywów

### Architektura

System stylów oparty jest na CSS Custom Properties (zmienne CSS), co umożliwia:
- **Centralne zarządzanie stylami** - wszystkie zmienne w jednym miejscu
- **Dynamiczną zmianę motywów** - bez przeładowania strony
- **Łatwą rozbudowę** - dodawanie nowych motywów lub kolorów
- **Spójność** - te same wartości używane w całej aplikacji

### Struktura Plików

```
frontend/src/
├── styles/
│   ├── _variables.scss     # Podstawowe zmienne SCSS (kolory, odstępy, czcionki)
│   └── _themes.scss         # CSS Custom Properties dla motywów (light/dark)
├── styles.scss              # Główny plik stylów (import wszystkich styli)
└── app/
    ├── core/
    │   └── services/
    │       └── theme.service.ts        # Serwis zarządzania motywami
    └── shared/
        └── components/
            └── theme-toggle/           # Komponent przełącznika motywów
                ├── theme-toggle.component.ts
                ├── theme-toggle.component.html
                └── theme-toggle.component.scss
```

### Dostępne Motywy

1. **Light** (jasny) - domyślny motyw
2. **Dark** (ciemny) - motyw ciemny
3. **Auto** - automatyczne dostosowanie do preferencji systemowych

### Zmienne CSS

Wszystkie zmienne dostępne w aplikacji:

#### Kolory Brandowe
```css
--color-primary      /* Główny kolor (indigo) */
--color-secondary    /* Kolor drugorzędny (purple) */
--color-success      /* Sukces (green) */
--color-warning      /* Ostrzeżenie (amber) */
--color-danger       /* Błąd (red) */
--color-info         /* Informacja (blue) */
```

#### Kolory Tła
```css
--bg-primary         /* Główne tło */
--bg-secondary       /* Drugorzędne tło */
--bg-tertiary        /* Trzecie tło */
--surface            /* Powierzchnia komponentów */
--surface-hover      /* Powierzchnia po najechaniu */
```

#### Kolory Tekstu
```css
--text-primary       /* Główny tekst */
--text-secondary     /* Drugorzędny tekst */
--text-tertiary      /* Trzeci tekst */
--text-disabled      /* Wyłączony tekst */
--text-on-primary    /* Tekst na primary (biały) */
```

#### Kolory Kategorii (z PRD)
```css
--category-technical /* Przeglądy techniczne (blue) */
--category-waste     /* Wywóz śmieci (green) */
--category-medical   /* Wizyty medyczne (red) */
```

#### Kolory Priorytetów
```css
--priority-low       /* Niski priorytet */
--priority-medium    /* Średni priorytet */
--priority-high      /* Wysoki priorytet */
```

#### Kolory Statusów
```css
--status-pending     /* Oczekujący */
--status-completed   /* Ukończony */
--status-overdue     /* Przeterminowany */
--status-cancelled   /* Anulowany */
```

#### Odstępy
```css
--spacing-xs         /* 4px */
--spacing-sm         /* 8px */
--spacing-md         /* 16px */
--spacing-lg         /* 24px */
--spacing-xl         /* 32px */
--spacing-2xl        /* 48px */
--spacing-3xl        /* 64px */
```

#### Zaokrąglenia
```css
--radius-sm          /* 4px */
--radius-md          /* 8px */
--radius-lg          /* 12px */
--radius-xl          /* 16px */
--radius-full        /* 9999px (pełne koło) */
```

#### Cienie
```css
--shadow-sm          /* Mały cień */
--shadow-md          /* Średni cień */
--shadow-lg          /* Duży cień */
--shadow-xl          /* Bardzo duży cień */
```

#### Typografia
```css
--font-family        /* Główna czcionka */
--font-family-mono   /* Czcionka monospace */

--font-size-xs       /* 12px */
--font-size-sm       /* 14px */
--font-size-base     /* 16px */
--font-size-lg       /* 18px */
--font-size-xl       /* 20px */
--font-size-2xl      /* 24px */
--font-size-3xl      /* 30px */
--font-size-4xl      /* 36px */

--font-weight-normal    /* 400 */
--font-weight-medium    /* 500 */
--font-weight-semibold  /* 600 */
--font-weight-bold      /* 700 */
```

#### Przejścia
```css
--transition-fast    /* 150ms */
--transition-normal  /* 300ms */
--transition-slow    /* 500ms */
```

## 🔧 Użycie w Komponentach

### Serwis Motywów

```typescript
import { ThemeService } from './core/services/theme.service';

export class MyComponent {
  private themeService = inject(ThemeService);

  // Pobranie aktualnego motywu
  currentTheme = this.themeService.appliedTheme();  // 'light' lub 'dark'

  // Zmiana motywu
  setLight() {
    this.themeService.setTheme('light');
  }

  setDark() {
    this.themeService.setTheme('dark');
  }

  setAuto() {
    this.themeService.setTheme('auto');
  }

  // Przełączanie między jasnym a ciemnym
  toggle() {
    this.themeService.toggleTheme();
  }

  // Sprawdzenie czy ciemny motyw
  isDark = this.themeService.isDarkMode();
}
```

### Użycie w Stylach SCSS

```scss
.my-component {
  // Użycie zmiennych CSS
  background: var(--surface);
  color: var(--text-primary);
  padding: var(--spacing-md);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);

  &:hover {
    background: var(--surface-hover);
  }
}

.my-button {
  background: var(--color-primary);
  color: var(--text-on-primary);
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-medium);
}

.my-card {
  border: 1px solid var(--border-color);

  &--technical {
    border-left: 4px solid var(--category-technical);
  }

  &--priority-high {
    background: rgba(var(--color-danger), 0.1);
  }
}
```

### Komponent Przełącznika Motywów

```html
<!-- Użycie gotowego komponentu -->
<app-theme-toggle></app-theme-toggle>
```

## 📱 Strona Demo

Strona demo dostępna po uruchomieniu aplikacji prezentuje:
- **Przełącznik motywów** - Light / Dark / Auto
- **Paletę kolorów** - wszystkie kolory brandowe, kategorie, statusy
- **Typografię** - nagłówki, tekst, rozmiary
- **Przyciski** - różne warianty i rozmiary
- **Karty** - przykłady kart z różnymi kategoriami
- **Formularze** - pola tekstowe, select, textarea, checkbox
- **Odznaki** - statusy i priorytety
- **Cienie** - różne poziomy cieni

## 🎯 Best Practices

### 1. Zawsze używaj zmiennych CSS
```scss
// ✅ DOBRZE
.component {
  color: var(--text-primary);
  background: var(--surface);
}

// ❌ ŹLE
.component {
  color: #111827;
  background: #ffffff;
}
```

### 2. Używaj semantic naming
```scss
// ✅ DOBRZE - znaczenie
.error-message {
  color: var(--color-danger);
}

// ❌ ŹLE - konkretny kolor
.error-message {
  color: var(--red-500);
}
```

### 3. Dodawaj nowe kolory do _variables.scss
```scss
// W _variables.scss dodaj nową zmienną
$new-feature-color: #abc123;

// W _themes.scss dodaj do obu motywów
:root {
  --new-feature: #{$new-feature-color};
}
```

### 4. Testuj oba motywy
Zawsze sprawdzaj komponenty w obu motywach (light i dark), aby upewnić się, że są czytelne i estetyczne.

## 🔄 Automatyczne Funkcje

### Zapisywanie preferencji
Wybór motywu jest automatycznie zapisywany w `localStorage` i przywracany przy następnym odwiedzeniu.

### Detekcja motywu systemowego
Gdy wybrano "Auto", aplikacja automatycznie:
- Wykrywa motyw systemowy użytkownika
- Reaguje na zmiany motywu systemowego w czasie rzeczywistym
- Aktualizuje `meta theme-color` dla przeglądarek mobilnych

### Płynne przejścia
Wszystkie elementy mają płynne przejścia między motywami (300ms).

## 📚 Zgodność z PRD

System motywów implementuje wszystkie wymagania z PRD:
- ✅ Kolory kategorii (Technical, Waste, Medical)
- ✅ Kolory priorytetów (Low, Medium, High)
- ✅ Kolory statusów (Pending, Completed, Overdue, Cancelled)
- ✅ System typografii
- ✅ Responsive design
- ✅ Accessibility (kontrast, focus states)

## 🛠 Rozwój

### Dodawanie nowego motywu

1. Dodaj kolory w `_variables.scss`:
```scss
$new-theme-bg: #xyz;
$new-theme-text: #abc;
```

2. Dodaj wariant w `_themes.scss`:
```scss
[data-theme='new-theme'] {
  --bg-primary: #{$new-theme-bg};
  --text-primary: #{$new-theme-text};
}
```

3. Zaktualizuj typ w `theme.service.ts`:
```typescript
export type Theme = 'light' | 'dark' | 'new-theme' | 'auto';
```

### Dodawanie nowej zmiennej

1. W `_variables.scss`:
```scss
$my-new-variable: value;
```

2. W `_themes.scss`:
```scss
:root {
  --my-new-variable: #{$my-new-variable};
}

// Opcjonalnie inna wartość dla dark
[data-theme='dark'] {
  --my-new-variable: #{$dark-variant};
}
```

## 🐛 Troubleshooting

### Motyw nie zmienia się
- Sprawdź czy `ThemeService` jest zaimportowany w komponencie
- Sprawdź czy zmienne CSS są poprawnie zdefiniowane
- Otwórz DevTools i sprawdź czy `data-theme` jest ustawiony na elemencie `<html>`

### Kolory nie działają
- Upewnij się że używasz `var(--variable-name)` zamiast wartości bezpośrednich
- Sprawdź czy zmienna jest zdefiniowana w `_themes.scss`
- Sprawdź czy importujesz `styles.scss` w `angular.json`

### Przejścia są za szybkie/wolne
Zmień wartość w `_variables.scss`:
```scss
$transition-normal: 500ms cubic-bezier(0.4, 0, 0.2, 1); // wolniejsze
```

## 📞 Wsparcie

W przypadku pytań lub problemów:
- Sprawdź dokumentację Angular: https://angular.dev
- Sprawdź dokumentację PrimeNG: https://primeng.org
- Zobacz przykłady w `app.component.html` (strona demo)
