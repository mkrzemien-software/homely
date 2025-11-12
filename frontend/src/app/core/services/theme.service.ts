import { Injectable, signal, effect } from '@angular/core';

export type Theme = 'light' | 'dark' | 'auto';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_STORAGE_KEY = 'homely-theme';

  // Signal for current theme preference
  themePreference = signal<Theme>('auto');

  // Signal for actual applied theme (resolved from 'auto')
  appliedTheme = signal<'light' | 'dark'>('light');

  constructor() {
    // Load theme preference from localStorage
    this.loadThemePreference();

    // Apply theme on initialization
    this.applyTheme();

    // Watch for theme preference changes
    effect(() => {
      const preference = this.themePreference();
      this.saveThemePreference(preference);
      this.applyTheme();
    });

    // Listen for system theme changes
    this.watchSystemTheme();
  }

  /**
   * Set theme preference
   */
  setTheme(theme: Theme): void {
    this.themePreference.set(theme);
  }

  /**
   * Toggle between light and dark themes
   */
  toggleTheme(): void {
    const currentApplied = this.appliedTheme();
    const newTheme: Theme = currentApplied === 'light' ? 'dark' : 'light';
    this.setTheme(newTheme);
  }

  /**
   * Apply theme to document
   * Uses PrimeNG convention: .p-dark class for dark mode
   */
  private applyTheme(): void {
    const preference = this.themePreference();
    const resolvedTheme = this.resolveTheme(preference);

    // Update applied theme signal
    this.appliedTheme.set(resolvedTheme);

    // Apply to document using PrimeNG convention
    const root = document.documentElement;

    // Remove existing theme classes/attributes
    root.classList.remove('p-dark');
    root.removeAttribute('data-theme');

    // Add dark mode class if needed (PrimeNG convention)
    if (resolvedTheme === 'dark') {
      root.classList.add('p-dark');
    }

    // Also set data-theme attribute for backward compatibility
    root.setAttribute('data-theme', resolvedTheme);

    // Update meta theme-color for mobile browsers
    this.updateMetaThemeColor(resolvedTheme);
  }

  /**
   * Resolve theme based on preference
   */
  private resolveTheme(preference: Theme): 'light' | 'dark' {
    if (preference === 'auto') {
      return this.getSystemTheme();
    }
    return preference;
  }

  /**
   * Get system theme preference
   */
  private getSystemTheme(): 'light' | 'dark' {
    if (typeof window !== 'undefined' && window.matchMedia) {
      return window.matchMedia('(prefers-color-scheme: dark)').matches
        ? 'dark'
        : 'light';
    }
    return 'light';
  }

  /**
   * Watch for system theme changes
   */
  private watchSystemTheme(): void {
    if (typeof window !== 'undefined' && window.matchMedia) {
      const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

      mediaQuery.addEventListener('change', (e) => {
        if (this.themePreference() === 'auto') {
          const newTheme = e.matches ? 'dark' : 'light';
          this.appliedTheme.set(newTheme);

          const root = document.documentElement;
          root.classList.remove('p-dark');
          root.removeAttribute('data-theme');

          if (newTheme === 'dark') {
            root.classList.add('p-dark');
          }
          root.setAttribute('data-theme', newTheme);

          this.updateMetaThemeColor(newTheme);
        }
      });
    }
  }

  /**
   * Update meta theme-color for mobile browsers
   */
  private updateMetaThemeColor(theme: 'light' | 'dark'): void {
    const metaThemeColor = document.querySelector('meta[name="theme-color"]');
    const color = theme === 'dark' ? '#0f172a' : '#ffffff';

    if (metaThemeColor) {
      metaThemeColor.setAttribute('content', color);
    } else {
      const meta = document.createElement('meta');
      meta.name = 'theme-color';
      meta.content = color;
      document.head.appendChild(meta);
    }
  }

  /**
   * Load theme preference from localStorage
   */
  private loadThemePreference(): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      const stored = localStorage.getItem(this.THEME_STORAGE_KEY);
      if (stored && (stored === 'light' || stored === 'dark' || stored === 'auto')) {
        this.themePreference.set(stored as Theme);
      }
    }
  }

  /**
   * Save theme preference to localStorage
   */
  private saveThemePreference(theme: Theme): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.setItem(this.THEME_STORAGE_KEY, theme);
    }
  }

  /**
   * Get current theme as observable value
   */
  getCurrentTheme(): 'light' | 'dark' {
    return this.appliedTheme();
  }

  /**
   * Get theme preference
   */
  getThemePreference(): Theme {
    return this.themePreference();
  }

  /**
   * Check if dark mode is active
   */
  isDarkMode(): boolean {
    return this.appliedTheme() === 'dark';
  }

  /**
   * Check if light mode is active
   */
  isLightMode(): boolean {
    return this.appliedTheme() === 'light';
  }
}
