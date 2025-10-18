import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService, Theme } from '../../../core/services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './theme-toggle.component.html',
  styleUrl: './theme-toggle.component.scss'
})
export class ThemeToggleComponent {
  private themeService = inject(ThemeService);

  // Expose theme service signals
  themePreference = this.themeService.themePreference;
  appliedTheme = this.themeService.appliedTheme;

  /**
   * Set specific theme
   */
  setTheme(theme: Theme): void {
    this.themeService.setTheme(theme);
  }

  /**
   * Quick toggle between light and dark
   */
  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  /**
   * Check if theme is active
   */
  isThemeActive(theme: Theme): boolean {
    return this.themePreference() === theme;
  }
}
