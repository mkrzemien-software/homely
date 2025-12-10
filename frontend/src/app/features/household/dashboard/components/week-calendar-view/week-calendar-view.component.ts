import { Component, signal, computed, output, input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

// PrimeNG Components
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TooltipModule } from 'primeng/tooltip';
import { BadgeModule } from 'primeng/badge';

// Models
import { DashboardEvent } from '../../models/dashboard.model';
import { getDashboardUrgencySeverity, getActualUrgencyStatus } from '../../models/dashboard.model';

/**
 * Week day interface
 */
export interface WeekDay {
  /**
   * Date object
   */
  date: Date;

  /**
   * Day number (1-31)
   */
  dayNumber: number;

  /**
   * Day name (Polish)
   */
  dayName: string;

  /**
   * Short day name (Pon, Wt, Śr, etc.)
   */
  dayNameShort: string;

  /**
   * Is today
   */
  isToday: boolean;

  /**
   * Events for this day
   */
  events: DashboardEvent[];
}

/**
 * WeekCalendarViewComponent
 *
 * Displays interactive weekly calendar (Monday-Sunday) with events as bars.
 * Used in dashboard to show upcoming events for the current week.
 *
 * Based on UI Plan (ui-plan.md line 922-936):
 * - Wyświetlanie 7 dni tygodnia (PN-ND) w układzie poziomym (desktop)
 * - Wydarzenia bezpośrednio w dniach jako bary
 * - Całodniowe wydarzenia na górze
 * - Wydarzenia z godziną jako bary według godzin
 * - Dzisiejszy dzień wyróżniony ramką
 * - Możliwość kliknięcia w wydarzenie
 * - Nawigacja poprzedni/następny tydzień
 * - Responsive: dni spadają pionowo na mobile/tablet
 *
 * Features:
 * - 7-day week view (Monday to Sunday)
 * - Color-coded events (overdue/today/upcoming)
 * - Click event to view details
 * - Week navigation (previous/next)
 * - Today indicator
 * - Responsive design
 * - Accessible (keyboard navigation, ARIA labels)
 *
 * @example
 * <app-week-calendar-view
 *   [events]="upcomingEvents()"
 *   (eventClick)="onEventClick($event)">
 * </app-week-calendar-view>
 */
@Component({
  selector: 'app-week-calendar-view',
  imports: [CommonModule, RouterLink, ButtonModule, CardModule, TooltipModule, BadgeModule],
  templateUrl: './week-calendar-view.component.html',
  styleUrl: './week-calendar-view.component.scss'
})
export class WeekCalendarViewComponent implements OnInit {
  /**
   * Events to display (input)
   */
  events = input<DashboardEvent[]>([]);

  /**
   * Household ID (input)
   */
  householdId = input<string>('');

  /**
   * Output event when event is clicked
   */
  eventClick = output<DashboardEvent>();

  /**
   * Output event when week changes
   * Emits start date of new week
   */
  weekChange = output<Date>();

  /**
   * Output event when show completed events toggle changes
   */
  showCompletedEventsChange = output<boolean>();

  /**
   * Current week start date (Monday)
   */
  currentWeekStart = signal<Date>(this.getWeekStart(new Date()));

  /**
   * Show completed events in calendar
   */
  showCompletedEvents = input<boolean>(false);

  /**
   * Week days array (Monday to Sunday)
   */
  weekDays = computed<WeekDay[]>(() => {
    const startDate = this.currentWeekStart();
    const days: WeekDay[] = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // Generate 7 days starting from Monday
    for (let i = 0; i < 7; i++) {
      const date = new Date(startDate);
      date.setDate(date.getDate() + i);
      date.setHours(0, 0, 0, 0);

      // Filter events for this day
      const dayEvents = this.events().filter(event => {
        const eventDate = new Date(event.dueDate);
        eventDate.setHours(0, 0, 0, 0);
        return eventDate.getTime() === date.getTime();
      });

      days.push({
        date: new Date(date),
        dayNumber: date.getDate(),
        dayName: this.getDayName(date.getDay()),
        dayNameShort: this.getDayNameShort(date.getDay()),
        isToday: date.getTime() === today.getTime(),
        events: dayEvents
      });
    }

    return days;
  });

  /**
   * Current month and year label
   */
  currentMonthLabel = computed(() => {
    const date = this.currentWeekStart();
    return date.toLocaleDateString('pl-PL', { month: 'long', year: 'numeric' });
  });

  /**
   * Week range label (e.g., "6-12 stycznia")
   */
  weekRangeLabel = computed(() => {
    const days = this.weekDays();
    if (days.length === 0) return '';

    const firstDay = days[0];
    const lastDay = days[days.length - 1];

    const firstMonth = firstDay.date.toLocaleDateString('pl-PL', { month: 'long' });
    const lastMonth = lastDay.date.toLocaleDateString('pl-PL', { month: 'long' });

    if (firstMonth === lastMonth) {
      return `${firstDay.dayNumber}-${lastDay.dayNumber} ${firstMonth}`;
    } else {
      return `${firstDay.dayNumber} ${firstMonth} - ${lastDay.dayNumber} ${lastMonth}`;
    }
  });

  ngOnInit(): void {
    // Initialize with current week
    const today = new Date();
    this.currentWeekStart.set(this.getWeekStart(today));
  }

  /**
   * Navigate to previous week
   */
  previousWeek(): void {
    const current = this.currentWeekStart();
    const previous = new Date(current);
    previous.setDate(previous.getDate() - 7);
    this.currentWeekStart.set(previous);
    this.weekChange.emit(previous);
  }

  /**
   * Navigate to next week
   */
  nextWeek(): void {
    const current = this.currentWeekStart();
    const next = new Date(current);
    next.setDate(next.getDate() + 7);
    this.currentWeekStart.set(next);
    this.weekChange.emit(next);
  }

  /**
   * Navigate to current week (today)
   */
  goToToday(): void {
    const today = new Date();
    const weekStart = this.getWeekStart(today);
    this.currentWeekStart.set(weekStart);
    this.weekChange.emit(weekStart);
  }

  /**
   * Handle event click
   */
  onEventClick(event: DashboardEvent): void {
    this.eventClick.emit(event);
  }

  /**
   * Get severity class for event urgency (recalculated based on current date)
   */
  getEventSeverity(event: DashboardEvent): 'danger' | 'warn' | 'info' {
    const actualUrgency = getActualUrgencyStatus(event);
    return getDashboardUrgencySeverity(actualUrgency);
  }

  /**
   * Get week start date (Monday) for given date
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
   * Get Polish day name
   */
  private getDayName(dayIndex: number): string {
    const days = ['Niedziela', 'Poniedziałek', 'Wtorek', 'Środa', 'Czwartek', 'Piątek', 'Sobota'];
    return days[dayIndex];
  }

  /**
   * Get short Polish day name
   */
  private getDayNameShort(dayIndex: number): string {
    const days = ['Nd', 'Pn', 'Wt', 'Śr', 'Cz', 'Pt', 'Sb'];
    return days[dayIndex];
  }

  /**
   * Check if week contains today
   */
  isCurrentWeek(): boolean {
    const days = this.weekDays();
    return days.some(day => day.isToday);
  }

  /**
   * Get event time display
   * Returns time if event has specific time, otherwise returns "Cały dzień"
   */
  getEventTimeDisplay(event: DashboardEvent): string {
    // For now, all events are displayed as all-day
    // In future, we could parse time from dueDate if available
    return 'Cały dzień';
  }

  /**
   * Get event tooltip text
   */
  getEventTooltip(event: DashboardEvent): string {
    return `${event.task.name}\n${event.task.category.categoryType.name} > ${event.task.category.name}\nOdpowiedzialny: ${event.assignedTo.firstName} ${event.assignedTo.lastName}`;
  }

  /**
   * Toggle show completed events
   */
  toggleShowCompletedEvents(): void {
    this.showCompletedEventsChange.emit(!this.showCompletedEvents());
  }
}
