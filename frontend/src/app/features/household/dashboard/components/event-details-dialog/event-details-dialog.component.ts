import { Component, signal, computed, output, input, effect, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// PrimeNG Components
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { AvatarModule } from 'primeng/avatar';
import { Calendar, CalendarModule } from 'primeng/calendar';
import { InputTextarea } from 'primeng/inputtextarea';

// Models
import { DashboardEvent } from '../../models/dashboard.model';
import { getDashboardUrgencySeverity, getDashboardUrgencyLabel } from '../../models/dashboard.model';
import { getEventStatusLabel, getEventStatusSeverity } from '../../../events/models/event.model';

/**
 * Event action type
 */
export type EventAction = 'complete' | 'postpone' | 'edit' | 'cancel' | 'close';

/**
 * EventDetailsDialogComponent
 *
 * Dialog component for displaying detailed information about an event.
 * Allows users to view event details and perform actions.
 *
 * Based on UI Plan (ui-plan.md):
 * - Wyświetlanie szczegółów wydarzenia
 * - Akcje: oznacz jako wykonane, przełóż, edytuj, anuluj
 * - Informacje: nazwa zadania, kategoria, termin, osoba, priorytet, status, notatki
 *
 * Features:
 * - PrimeNG Dialog with event details
 * - Action buttons (complete, postpone, edit, cancel)
 * - Responsive design
 * - Accessible (keyboard navigation, ARIA labels)
 * - Close on escape key
 *
 * @example
 * <app-event-details-dialog
 *   [(visible)]="dialogVisible"
 *   [event]="selectedEvent()"
 *   (action)="onEventAction($event)">
 * </app-event-details-dialog>
 */
@Component({
  selector: 'app-event-details-dialog',
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    TagModule,
    DividerModule,
    AvatarModule,
    CalendarModule,
    InputTextarea
  ],
  templateUrl: './event-details-dialog.component.html',
  styleUrl: './event-details-dialog.component.scss'
})
export class EventDetailsDialogComponent {
  /**
   * Dialog visibility (two-way binding)
   */
  visible = input<boolean>(false);
  visibleChange = output<boolean>();

  /**
   * Event to display (input)
   */
  event = input<DashboardEvent | null>(null);

  /**
   * Output event when action is performed
   */
  action = output<{ action: EventAction; event: DashboardEvent; data?: any }>();

  /**
   * Reference to the calendar component
   */
  @ViewChild(Calendar) calendar?: Calendar;

  /**
   * Internal dialog visibility state
   */
  dialogVisible = signal<boolean>(false);

  /**
   * Postpone mode (show date picker)
   */
  postponeMode = signal<boolean>(false);

  /**
   * Complete mode (show completion notes form)
   */
  completeMode = signal<boolean>(false);

  /**
   * Cancel mode (show cancel reason form)
   */
  cancelMode = signal<boolean>(false);

  /**
   * New due date for postponement
   */
  newDueDate = signal<Date | null>(null);

  /**
   * Postpone reason
   */
  postponeReason = signal<string>('');

  /**
   * Completion notes
   */
  completionNotes = signal<string>('');

  /**
   * Completion date
   */
  completionDate = signal<Date>(new Date());

  /**
   * Cancel reason
   */
  cancelReason = signal<string>('');

  /**
   * Minimum date for postpone (today)
   */
  readonly minDate = new Date();

  /**
   * Dialog header title
   */
  dialogTitle = computed(() => {
    const ev = this.event();
    return ev ? ev.task.name : 'Szczegóły wydarzenia';
  });

  /**
   * Can complete event (pending and postponed events)
   */
  canComplete = computed(() => {
    const ev = this.event();
    return ev?.status === 'pending' || ev?.status === 'postponed';
  });

  /**
   * Can postpone event (only pending events)
   */
  canPostpone = computed(() => {
    const ev = this.event();
    return ev?.status === 'pending' || ev?.status === 'postponed';
  });

  constructor() {
    // Sync visible input with internal state
    effect(() => {
      this.dialogVisible.set(this.visible());
    });

    // Reset all modes and forms when dialog closes
    effect(() => {
      if (!this.dialogVisible()) {
        // Close calendar overlay if open before resetting
        this.closeCalendarOverlay();

        this.postponeMode.set(false);
        this.completeMode.set(false);
        this.cancelMode.set(false);
        this.newDueDate.set(null);
        this.postponeReason.set('');
        this.completionNotes.set('');
        this.completionDate.set(new Date());
        this.cancelReason.set('');
      }
    });
  }

  /**
   * Handle dialog visibility change
   */
  onVisibleChange(visible: boolean): void {
    this.dialogVisible.set(visible);
    this.visibleChange.emit(visible);
  }

  /**
   * Close dialog
   */
  closeDialog(): void {
    this.closeCalendarOverlay();
    this.onVisibleChange(false);
    this.action.emit({ action: 'close', event: this.event()! });
  }

  /**
   * Close calendar overlay to prevent animation errors
   */
  private closeCalendarOverlay(): void {
    try {
      if (this.calendar?.overlayVisible) {
        this.calendar.overlayVisible = false;
        // Force hide the overlay element
        if (this.calendar.overlay) {
          this.calendar.hideOverlay();
        }
      }
    } catch (error) {
      // Ignore errors during cleanup
      console.debug('Calendar overlay cleanup:', error);
    }
  }

  /**
   * Show complete form
   */
  showCompleteForm(): void {
    this.completeMode.set(true);
    this.completionDate.set(new Date()); // Default to today
  }

  /**
   * Cancel complete
   */
  cancelComplete(): void {
    this.completeMode.set(false);
    this.completionNotes.set('');
    this.completionDate.set(new Date());
  }

  /**
   * Confirm complete
   */
  confirmComplete(): void {
    const ev = this.event();
    const notes = this.completionNotes();
    const date = this.completionDate();

    if (ev && date) {
      this.action.emit({
        action: 'complete',
        event: ev,
        data: {
          completionNotes: notes,
          completionDate: date.toISOString().split('T')[0] // YYYY-MM-DD format
        }
      });
      this.closeDialog();
    }
  }

  /**
   * Show postpone form
   */
  showPostponeForm(): void {
    this.postponeMode.set(true);
    // Set default new date to current event's due date
    const ev = this.event();
    if (ev) {
      // Parse date in local timezone to avoid timezone issues
      // dueDate is in ISO 8601 format, e.g., "2025-01-15" or "2025-01-15T10:00:00Z"
      const datePart = ev.dueDate.split('T')[0]; // Take only the date part (YYYY-MM-DD)
      const [year, month, day] = datePart.split('-').map(Number);
      const currentDueDate = new Date(year, month - 1, day);
      this.newDueDate.set(currentDueDate);
    }
  }

  /**
   * Cancel postpone
   */
  cancelPostpone(): void {
    this.closeCalendarOverlay();
    this.postponeMode.set(false);
    this.newDueDate.set(null);
    this.postponeReason.set('');
  }

  /**
   * Confirm postpone
   */
  confirmPostpone(): void {
    const ev = this.event();
    const newDate = this.newDueDate();
    const reason = this.postponeReason();

    if (ev && newDate) {
      // Close calendar overlay before proceeding
      this.closeCalendarOverlay();

      // Format date in local timezone to avoid timezone issues
      const year = newDate.getFullYear();
      const month = String(newDate.getMonth() + 1).padStart(2, '0');
      const day = String(newDate.getDate()).padStart(2, '0');
      const formattedDate = `${year}-${month}-${day}`;

      this.action.emit({
        action: 'postpone',
        event: ev,
        data: {
          newDueDate: formattedDate,
          reason
        }
      });
      this.closeDialog();
    }
  }

  /**
   * Handle edit action
   */
  onEdit(): void {
    const ev = this.event();
    if (ev) {
      this.action.emit({ action: 'edit', event: ev });
      this.closeDialog();
    }
  }

  /**
   * Show cancel event form
   */
  showCancelForm(): void {
    this.cancelMode.set(true);
  }

  /**
   * Cancel cancel event
   */
  cancelCancelEvent(): void {
    this.cancelMode.set(false);
    this.cancelReason.set('');
  }

  /**
   * Confirm cancel event
   */
  confirmCancelEvent(): void {
    const ev = this.event();
    const reason = this.cancelReason();

    console.log('confirmCancelEvent - reason:', reason, 'trimmed:', reason.trim());

    if (ev && reason.trim()) {
      this.action.emit({
        action: 'cancel',
        event: ev,
        data: {
          reason: reason.trim() // Send trimmed version
        }
      });
      this.closeDialog();
    }
  }

  /**
   * Get urgency severity
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
    return getEventStatusSeverity(event.status);
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

  /**
   * Format date for display
   */
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('pl-PL', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    });
  }

  /**
   * Get user initials
   */
  getUserInitials(event: DashboardEvent): string {
    const first = event.assignedTo.firstName.charAt(0).toUpperCase();
    const last = event.assignedTo.lastName.charAt(0).toUpperCase();
    return `${first}${last}`;
  }

  /**
   * Get user full name
   */
  getUserFullName(event: DashboardEvent): string {
    return `${event.assignedTo.firstName} ${event.assignedTo.lastName}`;
  }

  /**
   * Get category path
   */
  getCategoryPath(event: DashboardEvent): string {
    return `${event.task.category.categoryType.name} > ${event.task.category.name}`;
  }

  /**
   * Check if postpone form is valid
   */
  isPostponeFormValid(): boolean {
    return this.newDueDate() !== null && this.postponeReason().trim().length > 0;
  }

  /**
   * Check if complete form is valid
   */
  isCompleteFormValid(): boolean {
    return this.completionDate() !== null;
  }

  /**
   * Check if cancel form is valid
   */
  isCancelFormValid(): boolean {
    return this.cancelReason().trim().length > 0;
  }

  /**
   * Format completion date (always today)
   */
  formatCompletionDate(): string {
    const today = new Date();
    return today.toLocaleDateString('pl-PL', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    });
  }
}
