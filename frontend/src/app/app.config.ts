import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';
import { definePreset } from '@primeuix/themes';

import { routes } from './app.routes';

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
        }
      },
      dark: {
        primary: {
          color: '{green.500}',
          contrastColor: '{surface.900}',
          hoverColor: '{green.400}',
          activeColor: '{green.300}'
        },
        highlight: {
          background: '{green.400}',
          focusBackground: '{green.300}',
          color: '{surface.900}',
          focusColor: '{surface.900}'
        }
      }
    }
  }
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
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
