import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';
import { definePreset } from '@primeuix/themes';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

// Custom Homely preset based on Aura with green primary color
const HomelyPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{green.50}',
      100: '{green.100}',
      200: '{green.200}',
      300: '{green.300}',
      400: '{green.400}',
      500: '{green.500}',
      600: '{green.600}',
      700: '{green.700}',
      800: '{green.800}',
      900: '{green.900}',
      950: '{green.950}'
    },
    colorScheme: {
      light: {
        primary: {
          color: '{green.600}',
          contrastColor: '#ffffff',
          hoverColor: '{green.700}',
          activeColor: '{green.800}'
        },
        highlight: {
          background: '{green.950}',
          focusBackground: '{green.700}',
          color: '#ffffff',
          focusColor: '#ffffff'
        },
        surface: {
          0: '#ffffff',
          50: '{slate.50}',
          100: '{slate.100}',
          200: '{slate.200}',
          300: '{slate.300}',
          400: '{slate.400}',
          500: '{slate.500}',
          600: '{slate.600}',
          700: '{slate.700}',
          800: '{slate.800}',
          900: '{slate.900}',
          950: '{slate.950}'
        },
        text: {
          color: '{slate.700}',
          hoverColor: '{slate.800}',
          mutedColor: '{slate.500}',
          highlightColor: '{green.600}'
        },
        content: {
          background: '#ffffff',
          hoverBackground: '{slate.100}',
          borderColor: '{slate.200}',
          color: '{slate.700}',
          hoverColor: '{slate.800}'
        },
        formField: {
          background: '#ffffff',
          borderColor: '{slate.300}',
          hoverBorderColor: '{slate.400}',
          focusBorderColor: '{green.600}',
          invalidBorderColor: '{red.500}',
          color: '{slate.700}',
          placeholderColor: '{slate.400}'
        },
        mask: {
          background: 'rgba(0, 0, 0, 0.4)',
          color: '{slate.700}'
        },
        overlay: {
          modal: {
            background: '#ffffff',
            borderColor: '{slate.200}',
            color: '{slate.700}',
            shadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)'
          },
          popover: {
            background: '#ffffff',
            borderColor: '{slate.200}',
            color: '{slate.700}',
            shadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)'
          }
        }
      },
      dark: {
        primary: {
          color: '{green.500}',
          contrastColor: '{zinc.900}',
          hoverColor: '{green.400}',
          activeColor: '{green.300}'
        },
        highlight: {
          background: 'rgba(34, 197, 94, 0.16)',
          focusBackground: 'rgba(34, 197, 94, 0.24)',
          color: 'rgba(255, 255, 255, 0.87)',
          focusColor: 'rgba(255, 255, 255, 0.87)'
        },
        surface: {
          0: '#18181b',         // Główne tło - zinc.900 (ciemny szary zamiast czarnego)
          50: '#27272a',        // Lekko jaśniejsze - zinc.800
          100: '#3f3f46',       // Karty i komponenty - zinc.700
          200: '#52525b',       // Subtelne odcienie - zinc.600
          300: '#71717a',       // zinc.500
          400: '#a1a1aa',       // zinc.400
          500: '#d4d4d8',       // zinc.300
          600: '#e4e4e7',       // zinc.200
          700: '#f4f4f5',       // zinc.100
          800: '#fafafa',       // zinc.50
          900: '#ffffff',       // Biały
          950: '#ffffff'        // Biały
        },
        text: {
          color: 'rgba(255, 255, 255, 0.87)',
          hoverColor: '#ffffff',
          mutedColor: 'rgba(255, 255, 255, 0.60)',
          highlightColor: '{green.400}'
        },
        content: {
          background: '#18181b',     // Tło kontentów (zinc.900)
          hoverBackground: '#27272a', // Subtelny hover (zinc.800)
          borderColor: '#3f3f46',     // Delikatne ramki (zinc.700)
          color: 'rgba(255, 255, 255, 0.87)',
          hoverColor: '#ffffff'
        },
        formField: {
          background: '#27272a',      // Lekko jaśniejsze od tła (zinc.800)
          borderColor: '#3f3f46',     // Subtelne ramki (zinc.700)
          hoverBorderColor: '#52525b', // zinc.600
          focusBorderColor: '{green.500}',
          invalidBorderColor: '{red.400}',
          color: 'rgba(255, 255, 255, 0.87)',
          placeholderColor: 'rgba(255, 255, 255, 0.40)'
        },
        mask: {
          background: 'rgba(0, 0, 0, 0.6)',
          color: 'rgba(255, 255, 255, 0.87)'
        },
        overlay: {
          modal: {
            background: '#27272a',    // Dialog background (zinc.800)
            borderColor: '#3f3f46',   // Dialog border (zinc.700)
            color: 'rgba(255, 255, 255, 0.87)',
            shadow: '0 20px 25px -5px rgba(0, 0, 0, 0.5), 0 10px 10px -5px rgba(0, 0, 0, 0.3)'
          },
          popover: {
            background: '#27272a',
            borderColor: '#3f3f46',
            color: 'rgba(255, 255, 255, 0.87)',
            shadow: '0 4px 6px -1px rgba(0, 0, 0, 0.3), 0 2px 4px -1px rgba(0, 0, 0, 0.2)'
          }
        }
      }
    }
  }
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    provideAnimations(),
    providePrimeNG({
      theme: {
        preset: HomelyPreset,
        options: {
          darkModeSelector: '.p-dark',
          cssLayer: false
        }
      }
    })
  ]
};
