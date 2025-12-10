import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ButtonModule } from 'primeng/button';

// Custom Components
import { WeekCalendarViewComponent } from './components/week-calendar-view/week-calendar-view.component';
import { DashboardEventsListComponent } from './components/dashboard-events-list/dashboard-events-list.component';
import { EventDetailsDialogComponent, EventAction } from './components/event-details-dialog/event-details-dialog.component';
import { EventFiltersToolbarComponent, EventFilters } from './components/event-filters-toolbar/event-filters-toolbar.component';

// Services
import { DashboardService } from './services/dashboard.service';
import { EventsService } from '../events/services/events.service';

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
    ButtonModule,
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
  private eventsService = inject(EventsService);

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
  readonly pageSubtitle = 'Witaj w panelu zarządzania gospodarstwem';

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
   * Dashboard statistics from API
   */
  dashboardStats = signal<{
    pending: number;
    overdue: number;
    completedThisMonth: number;
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
   * Show completed events in calendar
   */
  showCompletedEvents = signal<boolean>(true);

  /**
   * Current week start date (for calendar view)
   */
  currentWeekStartDate = signal<Date | null>(null);

  /**
   * Filtered events based on current filters (for events list only)
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
   * Fetches real statistics from API
   *
   * @param householdId - The household ID
   */
  private loadHouseholdData(householdId: string): void {
    console.log('Loading household statistics for:', householdId);

    // Fetch statistics from API
    this.dashboardService
      .getStatistics(householdId)
      .subscribe({
        next: (response) => {
          // Set statistics from events data
          this.dashboardStats.set({
            pending: response.events.pending,
            overdue: response.events.overdue,
            completedThisMonth: response.events.completedThisMonth
          });
          console.log('Loaded statistics:', this.dashboardStats());
        },
        error: (error) => {
          console.error('Error loading statistics:', error);
          // Set default values on error
          this.dashboardStats.set({
            pending: 0,
            overdue: 0,
            completedThisMonth: 0
          });
        }
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

    // Initialize current week start date if not set
    if (!this.currentWeekStartDate()) {
      this.currentWeekStartDate.set(this.getWeekStart(new Date()));
    }

    // Get current week start date and format it for API
    const weekStartDate = this.currentWeekStartDate();
    const startDate = weekStartDate
      ? this.formatDateLocal(weekStartDate)
      : undefined;

    this.dashboardService
      .getUpcomingEvents({
        days: this.selectedDays(),
        householdId,
        startDate: startDate,
        includeCompleted: this.showCompletedEvents()
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

    // Store current week start date
    this.currentWeekStartDate.set(startDate);

    const householdId = this.householdId();
    if (!householdId) {
      return;
    }

    // Format date to ISO 8601 (YYYY-MM-DD) using LOCAL timezone, not UTC
    const formattedStartDate = this.formatDateLocal(startDate);
    console.log('Formatted startDate for API:', formattedStartDate);

    // Fetch events for the new week (7 days from startDate)
    this.eventsLoading.set(true);

    this.dashboardService
      .getUpcomingEvents({
        days: 7,
        householdId,
        startDate: formattedStartDate,
        includeCompleted: this.showCompletedEvents()
      })
      .subscribe({
        next: (response) => {
          this.upcomingEvents.set(response.data);
          this.eventsSummary.set(response.summary);
          this.eventsLoading.set(false);
        },
        error: (error) => {
          console.error('Error loading events for new week:', error);
          this.eventsLoading.set(false);
        }
      });
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
        this.completeEvent(action.event, action.data);
        break;
      case 'postpone':
        this.postponeEvent(action.event, action.data);
        break;
      case 'edit':
        this.editEvent(action.event);
        break;
      case 'cancel':
        this.cancelEvent(action.event, action.data);
        break;
      case 'close':
        // Dialog closed
        break;
    }
  }

  /**
   * Complete event
   */
  private completeEvent(event: DashboardEvent, data?: { completionNotes: string; completionDate: string }): void {
    console.log('Completing event:', event, data);

    this.eventsService.completeEvent(event.id, {
      completionDate: data?.completionDate || this.formatDateLocal(new Date()),
      notes: data?.completionNotes || ''
    }).subscribe({
      next: (completedEvent) => {
        console.log('Event completed successfully:', completedEvent);
        // Invalidate cache and reload events to show updated list
        const id = this.householdId();
        if (id) {
          this.dashboardService.invalidateCache(id);
          this.loadUpcomingEvents(id);
          this.loadHouseholdData(id); // Reload statistics
        }
      },
      error: (error) => {
        console.error('Error completing event:', error);
        // TODO: Show error message to user
      }
    });
  }

  /**
   * Postpone event
   */
  private postponeEvent(event: DashboardEvent, data: { newDueDate: string; reason: string }): void {
    console.log('Postponing event:', event, data);

    this.eventsService.postponeEvent(event.id, {
      newDueDate: data.newDueDate,
      reason: data.reason
    }).subscribe({
      next: (postponedEvent) => {
        console.log('Event postponed successfully:', postponedEvent);
        // Invalidate cache and reload events to show updated list
        const id = this.householdId();
        if (id) {
          this.dashboardService.invalidateCache(id);
          this.loadUpcomingEvents(id);
          this.loadHouseholdData(id); // Reload statistics
        }
      },
      error: (error) => {
        console.error('Error postponing event:', error);
        // TODO: Show error message to user
      }
    });
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
   * Cancel event (changes status to 'cancelled')
   */
  private cancelEvent(event: DashboardEvent, data?: { reason: string }): void {
    console.log('Cancelling event:', event, data);

    if (!data?.reason) {
      console.error('Cancel reason is required');
      return;
    }

    this.eventsService.cancelEvent(event.id, {
      reason: data.reason
    }).subscribe({
      next: (cancelledEvent) => {
        console.log('Event cancelled successfully:', cancelledEvent);
        // Invalidate cache and reload events to show updated list
        const id = this.householdId();
        if (id) {
          this.dashboardService.invalidateCache(id);
          this.loadUpcomingEvents(id);
          this.loadHouseholdData(id); // Reload statistics
        }
      },
      error: (error) => {
        console.error('Error cancelling event:', error);
        // TODO: Show error message to user
      }
    });
  }

  /**
   * Handle filters change
   */
  onFiltersChange(filters: EventFilters): void {
    this.currentFilters.set(filters);
  }

  /**
   * Handle show completed events change from week calendar view
   */
  onShowCompletedEventsChange(showCompleted: boolean): void {
    this.showCompletedEvents.set(showCompleted);
    const id = this.householdId();
    if (id) {
      this.eventsLoading.set(true);

      // Use current week start date if available, otherwise use default
      const weekStartDate = this.currentWeekStartDate();
      const startDate = weekStartDate
        ? this.formatDateLocal(weekStartDate)
        : undefined;

      this.dashboardService
        .getUpcomingEvents({
          days: this.selectedDays(),
          householdId: id,
          startDate: startDate,
          includeCompleted: this.showCompletedEvents()
        })
        .subscribe({
          next: (response) => {
            this.upcomingEvents.set(response.data);
            this.eventsSummary.set(response.summary);
            this.eventsLoading.set(false);
          },
          error: (error) => {
            console.error('Error loading events:', error);
            this.eventsLoading.set(false);
          }
        });
    }
  }

  /**
   * Get week start date (Monday) for given date
   * Helper method to calculate the Monday of the week containing the given date
   * Week starts on Monday (ISO 8601)
   */
  private getWeekStart(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    // Convert Sunday (0) to 7 for easier calculation
    const dayOfWeek = day === 0 ? 7 : day;
    // Calculate days to subtract to get to Monday (day 1)
    const daysFromMonday = dayOfWeek - 1;
    d.setDate(d.getDate() - daysFromMonday);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  /**
   * Format date to ISO 8601 string (YYYY-MM-DD) using local timezone
   * This avoids timezone conversion issues with toISOString()
   */
  private formatDateLocal(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
