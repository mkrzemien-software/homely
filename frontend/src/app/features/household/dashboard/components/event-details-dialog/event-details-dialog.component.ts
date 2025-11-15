import { Component, signal, computed, output, input, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// PrimeNG Components
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { AvatarModule } from 'primeng/avatar';
import { CalendarModule } from 'primeng/calendar';
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
   * Internal dialog visibility state
   */
  dialogVisible = signal<boolean>(false);

  /**
   * Postpone mode (show date picker)
   */
  postponeMode = signal<boolean>(false);

  /**
   * New due date for postponement
   */
  newDueDate = signal<Date | null>(null);

  /**
   * Postpone reason
   */
  postponeReason = signal<string>('');

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
   * Can complete event (only pending events)
   */
  canComplete = computed(() => {
    const ev = this.event();
    return ev?.status === 'pending';
  });

  /**
   * Can postpone event (only pending events)
   */
  canPostpone = computed(() => {
    const ev = this.event();
    return ev?.status === 'pending';
  });

  constructor() {
    // Sync visible input with internal state
    effect(() => {
      this.dialogVisible.set(this.visible());
    });

    // Reset postpone mode when dialog closes
    effect(() => {
      if (!this.dialogVisible()) {
        this.postponeMode.set(false);
        this.newDueDate.set(null);
        this.postponeReason.set('');
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
    this.onVisibleChange(false);
    this.action.emit({ action: 'close', event: this.event()! });
  }

  /**
   * Handle complete action
   */
  onComplete(): void {
    const ev = this.event();
    if (ev) {
      this.action.emit({ action: 'complete', event: ev });
      this.closeDialog();
    }
  }

  /**
   * Show postpone form
   */
  showPostponeForm(): void {
    this.postponeMode.set(true);
    // Set default new date to tomorrow
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.newDueDate.set(tomorrow);
  }

  /**
   * Cancel postpone
   */
  cancelPostpone(): void {
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
      this.action.emit({
        action: 'postpone',
        event: ev,
        data: {
          newDueDate: newDate.toISOString(),
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
   * Handle cancel event action
   */
  onCancelEvent(): void {
    const ev = this.event();
    if (ev) {
      this.action.emit({ action: 'cancel', event: ev });
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
}
