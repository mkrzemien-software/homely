import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

// Custom Components
import { NavigationTilesComponent } from '../../system/dashboard/components/navigation-tiles/navigation-tiles.component';
import { WeekCalendarViewComponent } from './components/week-calendar-view/week-calendar-view.component';
import { DashboardEventsListComponent } from './components/dashboard-events-list/dashboard-events-list.component';
import { EventDetailsDialogComponent, EventAction } from './components/event-details-dialog/event-details-dialog.component';
import { EventFiltersToolbarComponent, EventFilters } from './components/event-filters-toolbar/event-filters-toolbar.component';

// Services
import { DashboardService } from './services/dashboard.service';

// Models
import { NavigationTile } from '../../system/dashboard/models/navigation-tile.model';
import {
  getHouseholdNavigationTiles,
  getAdminNavigationTiles,
  getMemberNavigationTiles,
  getDashboardRoleNavigationTiles
} from './models/household-navigation-tile.model';
import { DashboardEvent, DashboardUpcomingEventsResponse } from './models/dashboard.model';

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
    ProgressSpinnerModule,
    NavigationTilesComponent,
    WeekCalendarViewComponent,
    DashboardEventsListComponent,
    EventDetailsDialogComponent,
    EventFiltersToolbarComponent
  ],
  templateUrl: './household-dashboard.component.html',
  styleUrl: './household-dashboard.component.scss'
})
export class HouseholdDashboardComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private dashboardService = inject(DashboardService);

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

  /**
   * Selected date range (days)
   */
  selectedDays = signal<7 | 14 | 30>(7);

  /**
   * Upcoming events data
   */
  upcomingEvents = signal<DashboardEvent[]>([]);

  /**
   * Events summary
   */
  eventsSummary = signal<{ overdue: number; today: number; thisWeek: number } | null>(null);

  /**
   * Dashboard statistics
   */
  dashboardStats = signal<{
    totalEvents: number;
    completedEvents: number;
    totalItems: number;
    totalMembers: number;
  } | null>(null);

  /**
   * Loading state for events
   */
  eventsLoading = signal<boolean>(false);

  /**
   * Selected event for details dialog
   */
  selectedEvent = signal<DashboardEvent | null>(null);

  /**
   * Event details dialog visibility
   */
  eventDialogVisible = signal<boolean>(false);

  /**
   * Current filters
   */
  currentFilters = signal<EventFilters>({});

  /**
   * Filtered events based on current filters
   */
  filteredEvents = computed(() => {
    const events = this.upcomingEvents();
    const filters = this.currentFilters();

    return events.filter(event => {
      if (filters.assignedUserId && event.assignedTo.firstName !== filters.assignedUserId) {
        return false;
      }
      // Note: event.task.category is an object, need to compare category ID
      // For now, skipping category filter until we have category ID in the event model
      // if (filters.categoryId && event.task.category.id !== filters.categoryId) {
      //   return false;
      // }
      if (filters.priority && event.priority !== filters.priority) {
        return false;
      }
      if (filters.status && event.status !== filters.status) {
        return false;
      }
      return true;
    });
  });

  ngOnInit(): void {
    // Get household ID from route
    this.route.params.subscribe(params => {
      const id = params['householdId'];
      if (id) {
        this.householdId.set(id);
        this.loadHouseholdData(id);
        this.loadUpcomingEvents(id);
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

    // TODO: Replace with real data from API
    // For now, setting mock statistics
    this.dashboardStats.set({
      totalEvents: 24,
      completedEvents: 18,
      totalItems: 12,
      totalMembers: 4
    });
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

  /**
   * Load upcoming events from API
   */
  private loadUpcomingEvents(householdId: string): void {
    this.eventsLoading.set(true);

    this.dashboardService
      .getUpcomingEvents({
        days: this.selectedDays(),
        householdId
      })
      .subscribe({
        next: (response: DashboardUpcomingEventsResponse) => {
          this.upcomingEvents.set(response.data);
          this.eventsSummary.set(response.summary);
          this.eventsLoading.set(false);
        },
        error: (error) => {
          console.error('Error loading upcoming events:', error);
          this.eventsLoading.set(false);
        }
      });
  }

  /**
   * Handle date range change
   */
  onDateRangeChange(days: 7 | 14 | 30): void {
    this.selectedDays.set(days);
    const id = this.householdId();
    if (id) {
      this.loadUpcomingEvents(id);
    }
  }

  /**
   * Handle week change in calendar
   */
  onWeekChange(startDate: Date): void {
    console.log('Week changed to:', startDate);
    // Optionally reload events for new week
  }

  /**
   * Handle event click
   */
  onEventClick(event: DashboardEvent): void {
    this.selectedEvent.set(event);
    this.eventDialogVisible.set(true);
  }

  /**
   * Handle event action from dialog
   */
  onEventAction(action: { action: EventAction; event: DashboardEvent; data?: any }): void {
    console.log('Event action:', action);

    switch (action.action) {
      case 'complete':
        this.completeEvent(action.event);
        break;
      case 'postpone':
        this.postponeEvent(action.event, action.data);
        break;
      case 'edit':
        this.editEvent(action.event);
        break;
      case 'cancel':
        this.cancelEvent(action.event);
        break;
      case 'close':
        // Dialog closed
        break;
    }
  }

  /**
   * Complete event
   */
  private completeEvent(event: DashboardEvent): void {
    console.log('Completing event:', event);
    // TODO: Implement API call to complete event
    // For now, just reload events
    const id = this.householdId();
    if (id) {
      this.loadUpcomingEvents(id);
    }
  }

  /**
   * Postpone event
   */
  private postponeEvent(event: DashboardEvent, data: { newDueDate: string; reason: string }): void {
    console.log('Postponing event:', event, data);
    // TODO: Implement API call to postpone event
    const id = this.householdId();
    if (id) {
      this.loadUpcomingEvents(id);
    }
  }

  /**
   * Edit event
   */
  private editEvent(event: DashboardEvent): void {
    console.log('Editing event:', event);
    // TODO: Navigate to edit event page or open edit dialog
    this.router.navigate([`/${this.householdId()}/events/${event.id}/edit`]);
  }

  /**
   * Cancel event
   */
  private cancelEvent(event: DashboardEvent): void {
    console.log('Cancelling event:', event);
    // TODO: Implement API call to cancel event
    const id = this.householdId();
    if (id) {
      this.loadUpcomingEvents(id);
    }
  }

  /**
   * Handle filters change
   */
  onFiltersChange(filters: EventFilters): void {
    this.currentFilters.set(filters);
  }

  /**
   * Handle action click from events list
   */
  onActionClick(action: { event: DashboardEvent; action: string }): void {
    console.log('Action click:', action);

    if (action.action === 'complete') {
      this.completeEvent(action.event);
    }
  }
}
