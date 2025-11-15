import { Component, signal, computed, output, input } from '@angular/core';
import { CommonModule } from '@angular/common';

// PrimeNG Components
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { AvatarModule } from 'primeng/avatar';
import { BadgeModule } from 'primeng/badge';

// Models
import { DashboardEvent } from '../../models/dashboard.model';
import { getDashboardUrgencySeverity, getDashboardUrgencyLabel } from '../../models/dashboard.model';
import { getEventStatusLabel } from '../../../events/models/event.model';

/**
 * DashboardEventsListComponent
 *
 * Displays scrollable list of events under the week calendar.
 * Shows event details in a table format with color-coded urgency.
 *
 * Based on UI Plan (ui-plan.md line 938-946):
 * - Scrollowalna lista pod kalendarzem
 * - Każde wydarzenie jako wiersz z nazwą zadania, kategorią, datą, osobą odpowiedzialną
 * - Color-coded urgency (primary/warning/danger)
 * - Kliknięcie w wydarzenie → otwiera EventDetailsDialog
 * - Ikona statusu wydarzenia (pending/completed)
 *
 * Features:
 * - PrimeNG Table with events data
 * - Color-coded urgency tags (overdue/today/upcoming)
 * - Click event to view details
 * - Status indicators
 * - Responsive design
 * - Empty state when no events
 * - Accessible (keyboard navigation, ARIA labels)
 *
 * @example
 * <app-dashboard-events-list
 *   [events]="upcomingEvents()"
 *   (eventClick)="onEventClick($event)">
 * </app-dashboard-events-list>
 */
@Component({
  selector: 'app-dashboard-events-list',
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    TagModule,
    TooltipModule,
    AvatarModule,
    BadgeModule
  ],
  templateUrl: './dashboard-events-list.component.html',
  styleUrl: './dashboard-events-list.component.scss'
})
export class DashboardEventsListComponent {
  /**
   * Events to display (input)
   */
  events = input<DashboardEvent[]>([]);

  /**
   * Loading state (input)
   */
  loading = input<boolean>(false);

  /**
   * Output event when event is clicked
   */
  eventClick = output<DashboardEvent>();

  /**
   * Output event when action button is clicked
   * (e.g., complete, postpone)
   */
  actionClick = output<{ event: DashboardEvent; action: string }>();

  /**
   * Sorted events by due date
   */
  sortedEvents = computed(() => {
    const eventsArray = [...this.events()];
    return eventsArray.sort((a, b) => {
      const dateA = new Date(a.dueDate).getTime();
      const dateB = new Date(b.dueDate).getTime();
      return dateA - dateB;
    });
  });

  /**
   * Events count
   */
  eventsCount = computed(() => this.events().length);

  /**
   * Handle row click
   */
  onEventClick(event: DashboardEvent): void {
    this.eventClick.emit(event);
  }

  /**
   * Handle action button click
   */
  onActionClick(event: DashboardEvent, action: string, $event: Event): void {
    $event.stopPropagation(); // Prevent row click
    this.actionClick.emit({ event, action });
  }

  /**
   * Get urgency severity for PrimeNG tag
   */
  getUrgencySeverity(event: DashboardEvent): 'danger' | 'warn' | 'info' {
    return getDashboardUrgencySeverity(event.urgencyStatus);
  }

  /**
   * Get urgency label
   */
  getUrgencyLabel(event: DashboardEvent): string {
    return getDashboardUrgencyLabel(event.urgencyStatus);
  }

  /**
   * Get status label
   */
  getStatusLabel(event: DashboardEvent): string {
    return getEventStatusLabel(event.status);
  }

  /**
   * Get status severity
   */
  getStatusSeverity(event: DashboardEvent): 'success' | 'info' | 'warn' | 'danger' {
    const severities: Record<string, 'success' | 'info' | 'warn' | 'danger'> = {
      pending: 'info',
      completed: 'success',
      postponed: 'warn',
      cancelled: 'danger'
    };
    return severities[event.status] || 'info';
  }

  /**
   * Format date for display
   */
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const eventDate = new Date(date);
    eventDate.setHours(0, 0, 0, 0);

    const diffTime = eventDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays === 0) {
      return 'Dzisiaj';
    } else if (diffDays === 1) {
      return 'Jutro';
    } else if (diffDays === -1) {
      return 'Wczoraj';
    } else if (diffDays > 1 && diffDays <= 7) {
      return `Za ${diffDays} dni`;
    } else if (diffDays < -1 && diffDays >= -7) {
      return `${Math.abs(diffDays)} dni temu`;
    }

    return date.toLocaleDateString('pl-PL', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    });
  }

  /**
   * Get full date display
   */
  getFullDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('pl-PL', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    });
  }

  /**
   * Get assigned user initials
   */
  getUserInitials(event: DashboardEvent): string {
    const first = event.assignedTo.firstName.charAt(0).toUpperCase();
    const last = event.assignedTo.lastName.charAt(0).toUpperCase();
    return `${first}${last}`;
  }

  /**
   * Get assigned user full name
   */
  getUserFullName(event: DashboardEvent): string {
    return `${event.assignedTo.firstName} ${event.assignedTo.lastName}`;
  }

  /**
   * Get category path (Type > Category)
   */
  getCategoryPath(event: DashboardEvent): string {
    return `${event.task.category.categoryType.name} > ${event.task.category.name}`;
  }

  /**
   * Get priority severity
   */
  getPrioritySeverity(priority: string): 'danger' | 'warn' | 'info' {
    const severities: Record<string, 'danger' | 'warn' | 'info'> = {
      high: 'danger',
      medium: 'warn',
      low: 'info'
    };
    return severities[priority] || 'info';
  }

  /**
   * Get priority label
   */
  getPriorityLabel(priority: string): string {
    const labels: Record<string, string> = {
      high: 'Wysoki',
      medium: 'Średni',
      low: 'Niski'
    };
    return labels[priority] || priority;
  }
}
