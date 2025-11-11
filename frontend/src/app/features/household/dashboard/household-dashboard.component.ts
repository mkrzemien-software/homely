import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';

// Custom Components
import { NavigationTilesComponent } from '../../system/dashboard/components/navigation-tiles/navigation-tiles.component';

// Models
import { NavigationTile } from '../../system/dashboard/models/navigation-tile.model';
import {
  getHouseholdNavigationTiles,
  getAdminNavigationTiles,
  getMemberNavigationTiles,
  getDashboardRoleNavigationTiles
} from './models/household-navigation-tile.model';

/**
 * HouseholdDashboardComponent
 *
 * Main dashboard for household members (Administrator, Member, Dashboard roles).
 * Provides quick navigation to main household sections:
 * - 📋 Zadania (Tasks) - upcoming tasks with quick actions
 * - 🏷️ Kategorie (Categories) - devices/visits grouped by categories
 * - ⚙️ Ustawienia (Settings) - household configuration
 *
 * Features:
 * - Large navigation tiles with icons and descriptions
 * - Role-based tile visibility (Administrator, Member, Dashboard)
 * - Responsive grid layout (2-3 columns)
 * - Household context from route parameter
 * - Statistics display on tiles
 * - Hover effects and animations
 * - Keyboard navigation support
 *
 * Route: /:householdId/dashboard
 * Access: All authenticated household members
 *
 * Based on PRD section 3.4.1 - Dashboard główny
 *
 * @example
 * // Route configuration
 * {
 *   path: ':householdId/dashboard',
 *   loadComponent: () => import('./household-dashboard.component').then(m => m.HouseholdDashboardComponent),
 *   canActivate: [authGuard]
 * }
 */
@Component({
  selector: 'app-household-dashboard',
  imports: [
    CommonModule,
    CardModule,
    DividerModule,
    MessageModule,
    NavigationTilesComponent
  ],
  templateUrl: './household-dashboard.component.html',
  styleUrl: './household-dashboard.component.scss'
})
export class HouseholdDashboardComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  /**
   * Household ID from route parameter
   */
  householdId = signal<string>('');

  /**
   * Current user role in the household
   * TODO: Get from authentication service
   * For now, defaulting to 'admin'
   */
  userRole = signal<'admin' | 'member' | 'dashboard'>('admin');

  /**
   * Navigation tiles to display based on role
   */
  navigationTiles = computed<NavigationTile[]>(() => {
    const id = this.householdId();
    const role = this.userRole();

    if (!id) {
      return [];
    }

    switch (role) {
      case 'admin':
        return getAdminNavigationTiles(id);
      case 'member':
        return getMemberNavigationTiles(id);
      case 'dashboard':
        return getDashboardRoleNavigationTiles(id);
      default:
        return getHouseholdNavigationTiles(id);
    }
  });

  /**
   * Page title with household context
   */
  readonly pageTitle = computed(() => {
    const id = this.householdId();
    return id ? `Dashboard - Gospodarstwo ${id}` : 'Dashboard';
  });

  /**
   * Page subtitle/description
   */
  readonly pageSubtitle = 'Witaj w panelu zarządzania gospodarstwem - wybierz sekcję aby rozpocząć';

  /**
   * Whether household ID is loaded
   */
  readonly isLoaded = computed(() => this.householdId() !== '');

  ngOnInit(): void {
    // Get household ID from route
    this.route.params.subscribe(params => {
      const id = params['householdId'];
      if (id) {
        this.householdId.set(id);
        this.loadHouseholdData(id);
      }
    });

    // TODO: Get user role from authentication service
    // For now, using default 'admin' role
  }

  /**
   * Load household data and statistics
   * TODO: Implement API call to fetch household data
   *
   * @param householdId - The household ID
   */
  private loadHouseholdData(householdId: string): void {
    // TODO: Implement API call
    // this.householdService.getHousehold(householdId).subscribe(...)
    // For now, just log the household ID
    console.log('Loading household data for:', householdId);

    // TODO: Load statistics for tiles (upcoming tasks count, etc.)
    // this.loadTaskStatistics(householdId);
    // this.loadCategoryStatistics(householdId);
  }

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
   * Reload statistics and counts
   */
  refreshDashboard(): void {
    const id = this.householdId();
    if (id) {
      this.loadHouseholdData(id);
    }
  }

  /**
   * Get enabled tiles count
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
