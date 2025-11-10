import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { RippleModule } from 'primeng/ripple';

// Models
import { NavigationTile } from '../../models/navigation-tile.model';

/**
 * NavigationTilesComponent
 *
 * Displays a grid of navigation tiles for quick access to main system sections.
 * Each tile is a large, clickable card with:
 * - Icon
 * - Title and description
 * - Optional badge for alerts/notifications
 * - Optional statistics
 * - Hover effects and animations
 *
 * Features:
 * - Responsive grid layout (2-3 columns based on screen size)
 * - Keyboard navigation support (tab, enter)
 * - Disabled state for Post-MVP features
 * - Badge indicators for alerts
 * - Hover animations
 *
 * @example
 * <app-navigation-tiles
 *   [tiles]="navigationTiles"
 *   (tileClick)="onTileClick($event)">
 * </app-navigation-tiles>
 */
@Component({
  selector: 'app-navigation-tiles',
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    BadgeModule,
    TooltipModule,
    RippleModule
  ],
  templateUrl: './navigation-tiles.component.html',
  styleUrl: './navigation-tiles.component.scss'
})
export class NavigationTilesComponent {
  /**
   * Array of navigation tiles to display
   */
  tiles = input.required<NavigationTile[]>();

  /**
   * Event emitted when a tile is clicked
   */
  tileClick = output<NavigationTile>();

  /**
   * Handle tile click event
   * Emits the clicked tile if it's enabled
   */
  onTileClick(tile: NavigationTile): void {
    if (tile.enabled !== false) {
      this.tileClick.emit(tile);
    }
  }

  /**
   * Handle keyboard navigation (Enter key)
   */
  onKeyDown(event: KeyboardEvent, tile: NavigationTile): void {
    if (event.key === 'Enter' && tile.enabled !== false) {
      this.onTileClick(tile);
    }
  }

  /**
   * Get CSS classes for tile based on its state and color
   */
  getTileClasses(tile: NavigationTile): string {
    const classes = ['navigation-tile'];

    // Add color class
    classes.push(`tile-${tile.color}`);

    // Add disabled class if tile is not enabled
    if (tile.enabled === false) {
      classes.push('tile-disabled');
    }

    return classes.join(' ');
  }

  /**
   * Get icon classes with color
   */
  getIconClasses(tile: NavigationTile): string {
    return `${tile.icon} tile-icon tile-icon-${tile.color}`;
  }

  /**
   * Check if tile should show badge
   */
  shouldShowBadge(tile: NavigationTile): boolean {
    return tile.badgeCount !== undefined && tile.badgeCount > 0;
  }

  /**
   * Check if tile should show stats
   */
  shouldShowStats(tile: NavigationTile): boolean {
    return tile.stats !== undefined;
  }

  /**
   * Get trend icon for stats
   */
  getTrendIcon(trend?: 'up' | 'down' | 'neutral'): string {
    switch (trend) {
      case 'up':
        return 'pi pi-arrow-up';
      case 'down':
        return 'pi pi-arrow-down';
      case 'neutral':
      default:
        return 'pi pi-minus';
    }
  }

  /**
   * Get trend color class
   */
  getTrendColorClass(trend?: 'up' | 'down' | 'neutral'): string {
    switch (trend) {
      case 'up':
        return 'trend-up';
      case 'down':
        return 'trend-down';
      case 'neutral':
      default:
        return 'trend-neutral';
    }
  }

  /**
   * Format badge count (max 99+)
   */
  formatBadgeCount(count: number): string {
    return count > 99 ? '99+' : count.toString();
  }
}
