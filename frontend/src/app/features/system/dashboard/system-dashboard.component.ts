import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';

// Custom Components
import { NavigationTilesComponent } from './components/navigation-tiles/navigation-tiles.component';

// Models
import { NavigationTile, DEFAULT_NAVIGATION_TILES } from './models/navigation-tile.model';

/**
 * SystemDashboardComponent
 *
 * Main dashboard for System Developer (Super Admin) role.
 * Provides quick navigation to main system administration sections:
 * - 🏢 Households Management
 * - 👤 Users Management
 * - 💳 Subscriptions Monitoring (Post-MVP)
 * - 🔧 System Administration (Post-MVP)
 * - 🎧 Support Tools (Post-MVP)
 *
 * Features:
 * - Large navigation tiles with icons and descriptions
 * - Responsive grid layout (2-3 columns)
 * - Hover effects and animations
 * - Badge indicators for alerts/notifications
 * - Statistics display on tiles
 * - Disabled state for Post-MVP features
 * - Keyboard navigation support
 *
 * Route: /system/dashboard
 * Access: System Developer role only
 *
 * @example
 * // Route configuration
 * {
 *   path: 'system/dashboard',
 *   loadComponent: () => import('./system-dashboard.component').then(m => m.SystemDashboardComponent),
 *   canActivate: [systemDeveloperGuard]
 * }
 */
@Component({
  selector: 'app-system-dashboard',
  imports: [
    CommonModule,
    CardModule,
    DividerModule,
    NavigationTilesComponent
  ],
  templateUrl: './system-dashboard.component.html',
  styleUrl: './system-dashboard.component.scss'
})
export class SystemDashboardComponent {
  private router = inject(Router);

  /**
   * Navigation tiles to display
   * Using DEFAULT_NAVIGATION_TILES from model
   */
  navigationTiles = signal<NavigationTile[]>(DEFAULT_NAVIGATION_TILES);

  /**
   * Page title
   */
  readonly pageTitle = 'System Dashboard';

  /**
   * Page subtitle/description
   */
  readonly pageSubtitle = 'Zarządzanie platformą Homely - wybierz sekcję aby rozpocząć';

  /**
   * Handle tile click event
   * Navigate to the selected tile's route
   *
   * @param tile - The clicked navigation tile
   */
  onTileClick(tile: NavigationTile): void {
    if (tile.enabled !== false) {
      this.router.navigate([tile.route]);
    }
  }

  /**
   * Refresh dashboard data
   * This method can be used to reload statistics and counts
   * Currently loads default tiles, but can be extended to fetch from API
   */
  refreshDashboard(): void {
    // In the future, this can fetch updated statistics from API
    // For now, we use the default configuration
    this.navigationTiles.set(DEFAULT_NAVIGATION_TILES);
  }

  /**
   * Get enabled tiles count
   * Useful for displaying statistics
   */
  getEnabledTilesCount(): number {
    return this.navigationTiles().filter(tile => tile.enabled !== false).length;
  }

  /**
   * Get total tiles count
   */
  getTotalTilesCount(): number {
    return this.navigationTiles().length;
  }
}
