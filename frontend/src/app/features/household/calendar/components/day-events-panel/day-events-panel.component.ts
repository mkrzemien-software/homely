import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

// PrimeNG Components
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';

// Models
import { Event, getEventUrgency, getUrgencySeverity, formatEventDate } from '../../../events/models/event.model';

/**
 * DayEventsPanelComponent
 *
 * Panel displaying events for a selected day in the calendar view.
 * Shown below the calendar when user clicks on a day.
 *
 * Based on PRD section 3.4.2 and UI Plan:
 * - Lista wydarzeń wybranego dnia pod kalendarzem
 * - Scrollowalna lista, color-coded
 * - Kliknięcie w wydarzenie otwiera EventDetailsDialog
 *
 * Features:
 * - List of events for the selected day
 * - Color-coded urgency (overdue/today/upcoming)
 * - Scrollable if many events
 * - Empty state when no events
 * - Close button
 *
 * @example
 * <app-day-events-panel
 *   [date]="selectedDate"
 *   [events]="eventsForDay"
 *   (eventClick)="onEventClick($event)"
 *   (close)="onClosePanel()">
 * </app-day-events-panel>
 */
@Component({
  selector: 'app-day-events-panel',
  imports: [
    CommonModule,
    PanelModule,
    ButtonModule,
    TagModule,
    DividerModule
  ],
  templateUrl: './day-events-panel.component.html',
  styleUrl: './day-events-panel.component.scss'
})
export class DayEventsPanelComponent {
  /**
   * Selected date
   */
  date = input.required<Date>();

  /**
   * Events for the selected day
   */
  events = input<Event[]>([]);

  /**
   * Event clicked
   */
  eventClick = output<Event>();

  /**
   * Close panel
   */
  close = output<void>();

  /**
   * Formatted date for display
   */
  formattedDate = computed(() => {
    const date = this.date();
    return date.toLocaleDateString('pl-PL', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    });
  });

  /**
   * Check if there are any events
   */
  hasEvents = computed(() => this.events().length > 0);

  /**
   * Handle event click
   */
  onEventClick(event: Event): void {
    this.eventClick.emit(event);
  }

  /**
   * Handle close button click
   */
  onClose(): void {
    this.close.emit();
  }

  /**
   * Get urgency severity for an event
   */
  getUrgencySeverity(event: Event): 'danger' | 'warn' | 'info' {
    const urgency = getEventUrgency(event);
    return getUrgencySeverity(urgency);
  }

  /**
   * Get urgency label
   */
  getUrgencyLabel(event: Event): string {
    const urgency: 'overdue' | 'today' | 'upcoming' = getEventUrgency(event);
    const labels: Record<'overdue' | 'today' | 'upcoming', string> = {
      overdue: 'Przekroczony',
      today: 'Dzisiaj',
      upcoming: 'Nadchodzące'
    };
    return labels[urgency];
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
   * Format event date
   */
  formatDate(dateString: string): string {
    return formatEventDate(dateString);
  }
}
