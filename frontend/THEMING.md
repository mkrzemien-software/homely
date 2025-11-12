# PrimeNG Theming System - Homely

## Przegląd

Projekt Homely używa oficjalnego systemu themingu PrimeNG z presetu **Aura** i kolorem głównym **Green**.

## Konfiguracja

### Preset i Kolor

- **Motyw**: Aura (nowoczesny, minimalistyczny design)
- **Kolor główny**: Green (#10b981)
- **Dark mode**: Obsługa przez klasę `.p-dark`

### Plik konfiguracyjny

Konfiguracja znajduje się w `src/app/app.config.ts`:

```typescript
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';
import { definePreset } from '@primeuix/themes';

const HomelyPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{green.50}',
      // ... mapowanie green palette na primary
    }
  }
});

export const appConfig: ApplicationConfig = {
  providers: [
    providePrimeNG({
      theme: {
        preset: HomelyPreset,
        options: {
          darkModeSelector: '.p-dark'
        }
      }
    })
  ]
};
```

## PrimeNG Design Tokens

### Kolory

#### Primary Colors
```css
--p-primary-color          /* Główny kolor aplikacji (green.600) */
--p-primary-hover-color    /* Hover state (green.700) */
--p-primary-active-color   /* Active state (green.800) */
```

#### Surface Colors
```css
--p-surface-0              /* Najjaśniejszy */
--p-surface-50
--p-surface-100
...
--p-surface-950            /* Najciemniejszy */
--p-content-background     /* Główne tło */
```

#### Text Colors
```css
--p-text-color             /* Główny kolor tekstu */
--p-text-secondary-color   /* Drugorzędny tekst */
--p-text-muted-color       /* Wyciszony tekst */
```

### Custom Business Tokens

Zachowane specyficzne dla biznesu:

```css
:root {
  /* Category Colors (z PRD) */
  --category-technical: #3b82f6;
  --category-waste: #10b981;
  --category-medical: #ef4444;

  /* Priority Colors */
  --priority-low: #6b7280;
  --priority-medium: #f59e0b;
  --priority-high: #ef4444;

  /* Status Colors */
  --status-pending: #f59e0b;
  --status-completed: #10b981;
  --status-overdue: #ef4444;
  --status-cancelled: #6b7280;
}
```

## Dark Mode

### Implementacja

Dark mode jest obsługiwany przez klasę `.p-dark` na elemencie `<html>`:

```typescript
// ThemeService
if (theme === 'dark') {
  document.documentElement.classList.add('p-dark');
} else {
  document.documentElement.classList.remove('p-dark');
}
```

### ThemeService API

```typescript
interface ThemeService {
  // Signals
  themePreference: WritableSignal<'light' | 'dark' | 'auto'>
  appliedTheme: Signal<'light' | 'dark'>

  // Methods
  setTheme(theme: Theme): void
  toggleTheme(): void
  getCurrentTheme(): 'light' | 'dark'
  isDarkMode(): boolean
  isLightMode(): boolean
}
```

## Komponenty PrimeNG

Wszystkie komponenty PrimeNG automatycznie używają design tokens:

```html
<!-- Przyciski automatycznie używają primary color -->
<p-button label="Click me"></p-button>

<!-- Formularze używają surface i border tokens -->
<input pInputText [(ngModel)]="value" />

<!-- Dialogi używają surface i text tokens -->
<p-dialog header="Title">
  Content
</p-dialog>
```

## Customizacja

### Nadpisywanie tokenów

Aby zmienić konkretne tokeny, dodaj do `styles.scss`:

```scss
:root {
  // Nadpisz domyślne wartości
  --p-primary-color: #custom-color;
}

.p-dark {
  // Nadpisz dla dark mode
  --p-primary-color: #custom-color-dark;
}
```

### Customizacja komponentów

```scss
// Customizacja konkretnego komponentu
.p-button {
  border-radius: 10px !important;
  padding: 10px !important;
}
```

## Migracja z customowego systemu

### Co zostało zmienione

1. **Usunięte pliki**:
   - `src/styles/_variables.scss`
   - `src/styles/_themes.scss`

2. **Zmodyfikowane pliki**:
   - `src/app/app.config.ts` - dodano `providePrimeNG`
   - `src/app/core/services/theme.service.ts` - użycie `.p-dark` class
   - `src/styles.scss` - używa PrimeNG design tokens
   - `package.json` - zmieniono `@primeng/themes` na `@primeuix/themes`

### Mapowanie starych zmiennych

| Stare zmienne | Nowe tokeny PrimeNG |
|--------------|-------------------|
| `--color-primary` | `--p-primary-color` |
| `--surface-card` | `--p-surface-0` |
| `--text-primary` | `--p-text-color` |
| `--text-secondary` | `--p-text-secondary-color` |
| `--border-color` | `--p-surface-border` |
| `--bg-primary` | `--p-content-background` |

### Backward Compatibility

Niektóre komponenty mogą używać starych zmiennych. W `styles.scss` są zachowane aliasy:

```scss
// Custom business tokens - zachowane
--category-technical
--category-waste
--priority-*
--status-*
```

## Best Practices

### ✅ Rekomendowane

1. **Używaj PrimeNG design tokens** zamiast hardcoded colors
2. **Używaj semantic tokens** (primary, success, danger) zamiast palette tokens
3. **Testuj w obu trybach** (light i dark)
4. **Używaj PrimeNG komponentów** zamiast custom CSS

### ❌ Unikaj

1. ~~Hardcoded colors w SCSS~~
2. ~~Custom variables dla rzeczy pokrytych przez PrimeNG~~
3. ~~Własne implementacje dark mode~~
4. ~~Nadpisywanie wszystkiego przez `!important`~~

## Przykłady

### Użycie w komponentach

```typescript
@Component({
  template: `
    <div class="card">
      <h2>{{ title }}</h2>
      <p-button label="Primary" />
      <p-button label="Secondary" severity="secondary" />
    </div>
  `,
  styles: [`
    .card {
      background: var(--p-surface-0);
      border: 1px solid var(--p-surface-border);
      padding: 1rem;
      border-radius: var(--p-border-radius);
      color: var(--p-text-color);
    }

    h2 {
      color: var(--p-primary-color);
    }
  `]
})
```

### Responsive Design

```scss
.my-component {
  background: var(--p-surface-0);

  @media (max-width: 768px) {
    padding: 0.5rem;
  }

  // Dark mode
  .p-dark & {
    // Automatycznie zmienia się przez tokeny
    // Nie musisz nic robić!
  }
}
```

## Zasoby

- [PrimeNG Theming Documentation](https://primeng.org/theming)
- [PrimeNG Configuration](https://primeng.org/configuration)
- [Aura Preset Documentation](https://primeng.org/theming#aura)
- [@primeuix/themes npm](https://www.npmjs.com/package/@primeuix/themes)

## Troubleshooting

### Problem: Kolory nie zmieniają się w dark mode

**Rozwiązanie**: Sprawdź czy używasz PrimeNG design tokens (`--p-*`) zamiast custom variables.

### Problem: Kompilacja SCSS failuje

**Rozwiązanie**: Upewnij się że:
1. `@primeuix/themes` jest zainstalowany
2. `providePrimeNG` jest w `app.config.ts`
3. Nie importujesz starych `_variables.scss` lub `_themes.scss`

### Problem: Bundle size jest za duży

**Rozwiązanie**: PrimeNG theming dodaje ~100KB do bundle. To jest normalne i expected.

## License

MIT
