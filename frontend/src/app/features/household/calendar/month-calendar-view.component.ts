import { Component, signal, computed, inject, effect, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

// FullCalendar
import { FullCalendarModule, FullCalendarComponent } from '@fullcalendar/angular';
import { CalendarOptions, DateSelectArg, EventClickArg, EventInput } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import plLocale from '@fullcalendar/core/locales/pl';

// PrimeNG Components
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';

// Services
import { EventsService } from '../events/services/events.service';

// Models
import { Event, EventsQueryParams, Priority, EventStatus } from '../events/models/event.model';
import { DashboardEvent } from '../dashboard/models/dashboard.model';

// Components
import { EventDetailsDialogComponent, EventAction } from '../dashboard/components/event-details-dialog/event-details-dialog.component';
import { DayEventsPanelComponent } from './components/day-events-panel/day-events-panel.component';
import { CreateEventFromTaskDialogComponent } from '../tasks/components/create-event-from-task-dialog/create-event-from-task-dialog.component';

/**
 * MonthCalendarViewComponent
 *
 * Main calendar view component displaying events in a monthly grid.
 * Based on PRD section 3.4.2 and UI Plan.
 *
 * Features:
 * - Monthly calendar grid (7x5/6 rows for days)
 * - Events marked in days (dots, icons, colors)
 * - Today highlighted with border
 * - Navigation between months (arrows, "today" button)
 * - Color-coding events (primary/warning/danger)
 * - Click on day -> show events list below calendar
 * - Click on empty day -> open event creation form
 * - Click on event -> open EventDetailsDialog
 * - Toolbar (top right): add event button, filters, today button
 * - Responsive: full grid on desktop, compact on mobile/tablet
 *
 * @example
 * Navigate to /:householdId/calendar
 */
@Component({
  selector: 'app-month-calendar-view',
  imports: [
    CommonModule,
    FullCalendarModule,
    ButtonModule,
    ToolbarModule,
    EventDetailsDialogComponent,
    DayEventsPanelComponent,
    CreateEventFromTaskDialogComponent
  ],
  templateUrl: './month-calendar-view.component.html',
  styleUrl: './month-calendar-view.component.scss'
})
export class MonthCalendarViewComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private eventsService = inject(EventsService);
  private destroy$ = new Subject<void>();

  /**
   * Reference to FullCalendar component
   */
  @ViewChild('calendar') calendarComponent!: FullCalendarComponent;

  /**
   * Household ID from route
   */
  householdId = signal<string>('');

  /**
   * Current month being displayed
   */
  currentDate = signal<Date>(new Date());

  /**
   * All events for the current month
   */
  events = signal<Event[]>([]);

  /**
   * Selected date (when user clicks on a day)
   */
  selectedDate = signal<Date | null>(null);

  /**
   * Events for the selected day
   */
  selectedDayEvents = computed(() => {
    const date = this.selectedDate();
    if (!date) return [];

    const dateStr = this.formatDateToISO(date);
    return this.events().filter(event => {
      const eventDate = new Date(event.dueDate);
      return this.isSameDay(eventDate, date);
    });
  });

  /**
   * Selected event for details dialog
   */
  selectedEvent = signal<DashboardEvent | null>(null);

  /**
   * Event details dialog visibility
   */
  showEventDetailsDialog = signal<boolean>(false);

  /**
   * Create event dialog visibility
   */
  showCreateEventDialog = signal<boolean>(false);

  /**
   * Pre-filled date for create event dialog
   */
  createEventDate = signal<Date | null>(null);

  /**
   * Loading state
   */
  loading = signal<boolean>(false);

  /**
   * FullCalendar options
   */
  calendarOptions = computed<CalendarOptions>(() => ({
    plugins: [dayGridPlugin, interactionPlugin],
    initialView: 'dayGridMonth',
    locale: plLocale,
    headerToolbar: false, // We'll create custom toolbar
    events: this.getFullCalendarEvents(),
    selectable: true,
    selectMirror: true,
    dayMaxEvents: 3, // Show max 3 events per day, rest as "+X more"
    eventClick: this.handleEventClick.bind(this),
    select: this.handleDateSelect.bind(this),
    dateClick: this.handleDateClick.bind(this),
    datesSet: this.handleDatesSet.bind(this),
    height: 'auto',
    eventClassNames: (arg) => {
      const event = this.events().find(e => e.id === arg.event.id);
      if (!event) return [];

      const classes: string[] = [];

      // Add urgency class
      if (event.isOverdue) {
        classes.push('event-overdue');
      } else if (this.isToday(new Date(event.dueDate))) {
        classes.push('event-today');
      } else {
        classes.push('event-upcoming');
      }

      // Add priority class
      classes.push(`event-priority-${event.priority}`);

      return classes;
    },
    eventContent: (arg) => {
      // Custom event rendering
      const event = this.events().find(e => e.id === arg.event.id);
      if (!event) return { html: arg.event.title };

      // Simple dot indicator for events
      return {
        html: `<div class="fc-event-dot"></div><div class="fc-event-title">${arg.event.title}</div>`
      };
    }
  }));

  constructor() {
    // Load events when household or current date changes
    effect(() => {
      const household = this.householdId();
      const currentDate = this.currentDate(); // Track currentDate changes
      if (household) {
        this.loadEventsForCurrentMonth();
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    // Get household ID from route
    this.route.parent?.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const id = params.get('householdId');
        if (id) {
          this.householdId.set(id);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load events for the current month
   */
  private loadEventsForCurrentMonth(): void {
    this.loading.set(true);

    const currentDate = this.currentDate();
    const startDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1);
    const endDate = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 0);

    const queryParams: EventsQueryParams = {
      householdId: this.householdId(),
      startDate: this.formatDateToISO(startDate),
      endDate: this.formatDateToISO(endDate),
      sortBy: 'dueDate',
      sortOrder: 'asc'
    };

    this.eventsService.getEvents(queryParams)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.events.set(response.data);
          this.loading.set(false);
        },
        error: (error) => {
          console.error('Error loading events:', error);
          this.loading.set(false);
        }
      });
  }

  /**
   * Convert events to FullCalendar format
   */
  private getFullCalendarEvents(): EventInput[] {
    return this.events().map(event => ({
      id: event.id,
      title: event.taskName,
      start: event.dueDate,
      allDay: true,
      backgroundColor: this.getEventColor(event),
      borderColor: this.getEventColor(event),
      extendedProps: {
        event: event
      }
    }));
  }

  /**
   * Get event color based on urgency and priority
   */
  private getEventColor(event: Event): string {
    // Overdue events are always red
    if (event.isOverdue && event.status === 'pending') {
      return '#ef4444'; // Red
    }

    // Today events are orange
    if (this.isToday(new Date(event.dueDate)) && event.status === 'pending') {
      return '#f59e0b'; // Orange
    }

    // Upcoming events by priority
    switch (event.priority) {
      case 'high':
        return '#f59e0b'; // Orange
      case 'medium':
        return '#3b82f6'; // Blue
      case 'low':
        return '#10b981'; // Green
      default:
        return '#6b7280'; // Gray
    }
  }

  /**
   * Handle event click
   */
  private handleEventClick(clickInfo: EventClickArg): void {
    const eventId = clickInfo.event.id;
    const event = this.events().find(e => e.id === eventId);

    if (event) {
      // Convert Event to DashboardEvent for the dialog
      const dashboardEvent = this.convertToDashboardEvent(event);
      this.selectedEvent.set(dashboardEvent);
      this.showEventDetailsDialog.set(true);
    }
  }

  /**
   * Handle date select (user dragged to select range)
   */
  private handleDateSelect(selectInfo: DateSelectArg): void {
    // For now, just select the start date
    this.handleDateClick({ date: selectInfo.start, dateStr: selectInfo.startStr } as any);
  }

  /**
   * Handle date click (user clicked on a day)
   */
  private handleDateClick(info: { date: Date; dateStr: string }): void {
    const clickedDate = info.date;
    this.selectedDate.set(clickedDate);

    // If no events on this day, show create event dialog
    const eventsOnDay = this.selectedDayEvents();
    if (eventsOnDay.length === 0) {
      this.createEventDate.set(clickedDate);
      this.showCreateEventDialog.set(true);
    }
  }

  /**
   * Handle dates set (month changed)
   */
  private handleDatesSet(dateInfo: any): void {
    const newDate = dateInfo.view.currentStart;
    this.currentDate.set(newDate);
  }

  /**
   * Navigate to previous month
   */
  goToPreviousMonth(): void {
    const calendarApi = this.calendarComponent.getApi();
    calendarApi.prev();
  }

  /**
   * Navigate to next month
   */
  goToNextMonth(): void {
    const calendarApi = this.calendarComponent.getApi();
    calendarApi.next();
  }

  /**
   * Go to today
   */
  goToToday(): void {
    const calendarApi = this.calendarComponent.getApi();
    calendarApi.today();
    this.selectedDate.set(null);
  }

  /**
   * Handle event action from dialog
   */
  onEventAction(actionData: { action: EventAction; event: DashboardEvent; data?: any }): void {
    const { action, event, data } = actionData;

    switch (action) {
      case 'complete':
        this.completeEvent(event.id, data);
        break;
      case 'postpone':
        this.postponeEvent(event.id, data);
        break;
      case 'cancel':
        this.cancelEvent(event.id, data);
        break;
      case 'edit':
        // Navigate to edit event page or open edit dialog
        console.log('Edit event:', event);
        break;
      case 'close':
        this.showEventDetailsDialog.set(false);
        break;
    }
  }

  /**
   * Complete an event
   */
  private completeEvent(eventId: string, data: any): void {
    this.eventsService.completeEvent(eventId, data)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadEventsForCurrentMonth();
        },
        error: (error) => {
          console.error('Error completing event:', error);
        }
      });
  }

  /**
   * Postpone an event
   */
  private postponeEvent(eventId: string, data: any): void {
    this.eventsService.postponeEvent(eventId, data)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadEventsForCurrentMonth();
        },
        error: (error) => {
          console.error('Error postponing event:', error);
        }
      });
  }

  /**
   * Cancel an event
   */
  private cancelEvent(eventId: string, data: any): void {
    this.eventsService.cancelEvent(eventId, data)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadEventsForCurrentMonth();
        },
        error: (error) => {
          console.error('Error cancelling event:', error);
        }
      });
  }

  /**
   * Handle event created from dialog
   */
  onEventCreated(): void {
    this.showCreateEventDialog.set(false);
    this.loadEventsForCurrentMonth();
  }

  /**
   * Handle event click from day panel
   */
  onDayPanelEventClick(event: Event): void {
    const dashboardEvent = this.convertToDashboardEvent(event);
    this.selectedEvent.set(dashboardEvent);
    this.showEventDetailsDialog.set(true);
  }

  /**
   * Close day events panel
   */
  closeDayEventsPanel(): void {
    this.selectedDate.set(null);
  }

  /**
   * Convert Event to DashboardEvent
   */
  private convertToDashboardEvent(event: Event): DashboardEvent {
    return {
      id: event.id,
      dueDate: event.dueDate,
      urgencyStatus: event.isOverdue ? 'overdue' : (this.isToday(new Date(event.dueDate)) ? 'today' : 'upcoming'),
      task: {
        name: event.taskName,
        category: {
          name: event.categoryName || 'Bez kategorii',
          categoryType: {
            name: event.categoryTypeName || 'Ogólne'
          }
        }
      },
      assignedTo: {
        firstName: event.assignedToFirstName || 'Nieprzypisany',
        lastName: event.assignedToLastName || ''
      },
      priority: event.priority,
      status: event.status as any
    };
  }

  /**
   * Check if date is today
   */
  private isToday(date: Date): boolean {
    const today = new Date();
    return this.isSameDay(date, today);
  }

  /**
   * Check if two dates are the same day
   */
  private isSameDay(date1: Date, date2: Date): boolean {
    return date1.getFullYear() === date2.getFullYear() &&
      date1.getMonth() === date2.getMonth() &&
      date1.getDate() === date2.getDate();
  }

  /**
   * Format date to ISO string (YYYY-MM-DD)
   */
  private formatDateToISO(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  /**
   * Get current month and year for display
   */
  getCurrentMonthDisplay(): string {
    const date = this.currentDate();
    return date.toLocaleDateString('pl-PL', { month: 'long', year: 'numeric' });
  }
}
