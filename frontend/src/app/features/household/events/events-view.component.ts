import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { DatePickerModule } from 'primeng/datepicker';

// Models
import {
  Event,
  EventStatus,
  getEventStatusLabel,
  getEventStatusSeverity,
  getEventUrgency,
  getUrgencySeverity,
  formatEventDate,
  isEventOverdue
} from './models/event.model';

import { Priority, getPriorityLabel, getPrioritySeverity, Task } from '../tasks/models/task.model';
import { CreateEventDto } from './models/event.model';

// Services
import { EventsService } from './services/events.service';
import { CategoryService } from '../items/services/category.service';
import { HouseholdService } from '../../../core/services/household.service';
import { TasksService } from '../tasks/services/tasks.service';
import { Category } from '../items/models/category.model';

// Components (to be created)
// import { EventDetailsDialogComponent } from './components/event-details-dialog/event-details-dialog.component';
// import { StatusCountersComponent } from './components/status-counters/status-counters.component';

/**
 * EventsViewComponent
 *
 * Main component for displaying and managing events (task occurrences).
 * Based on API plan - GET /events, POST /events, PUT /events/{id}, DELETE /events/{id}
 * Plus action endpoints: complete, postpone, cancel
 *
 * Features:
 * - Display all events for household
 * - Sorting: by date, status, priority
 * - Filtering: by assignee, category, priority, status, date range
 * - Search by name/description
 * - Status counters
 * - Quick actions: complete, postpone, edit, cancel
 * - Pagination support
 *
 * Route: /:householdId/events
 *
 * @example
 * // Route configuration
 * {
 *   path: ':householdId/events',
 *   loadComponent: () => import('./events-view.component').then(m => m.EventsViewComponent)
 * }
 */
@Component({
  selector: 'app-events-view',
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    SelectModule,
    InputTextModule,
    TextareaModule,
    IconFieldModule,
    InputIconModule,
    DividerModule,
    MessageModule,
    SkeletonModule,
    TagModule,
    TooltipModule,
    DialogModule,
    PaginatorModule,
    DatePickerModule
    // EventDetailsDialogComponent,
    // StatusCountersComponent
  ],
  templateUrl: './events-view.component.html',
  styleUrl: './events-view.component.scss'
})
export class EventsViewComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private eventsService = inject(EventsService);
  private categoryService = inject(CategoryService);
  private householdService = inject(HouseholdService);
  private tasksService = inject(TasksService);

  /**
   * Household ID from route
   */
  householdId = signal<string>('');

  /**
   * All events (current page)
   */
  events = signal<Event[]>([]);

  /**
   * All categories for filter dropdown
   */
  allCategories = signal<Category[]>([]);

  /**
   * Household members for assignee filter
   */
  householdMembers = signal<Array<{ id: string; userId: string; firstName: string; lastName: string }>>([]);

  /**
   * Selected assignee filter
   */
  selectedAssigneeId = signal<string | undefined>(undefined);

  /**
   * Selected category filter
   */
  selectedCategoryId = signal<number | undefined>(undefined);

  /**
   * Selected priority filter
   */
  selectedPriority = signal<Priority | undefined>(undefined);

  /**
   * Selected status filter
   */
  selectedStatus = signal<EventStatus | undefined>(undefined);

  /**
   * Date range filter - start date
   */
  startDate = signal<Date | undefined>(undefined);

  /**
   * Date range filter - end date
   */
  endDate = signal<Date | undefined>(undefined);

  /**
   * Filter overdue events only
   */
  showOverdueOnly = signal<boolean>(false);

  /**
   * Search text
   */
  searchText = signal<string>('');

  /**
   * Sort by field
   */
  sortBy = signal<'dueDate' | 'status' | 'priority' | 'createdAt'>('dueDate');

  /**
   * Sort order
   */
  sortOrder = signal<'asc' | 'desc'>('asc');

  /**
   * Loading state
   */
  isLoading = signal<boolean>(true);

  /**
   * Error message
   */
  errorMessage = signal<string | null>(null);

  /**
   * Pagination
   */
  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  totalItems = signal<number>(0);
  itemsPerPage = signal<number>(20);

  /**
   * Event details dialog visibility
   */
  eventDetailsDialogVisible = signal<boolean>(false);

  /**
   * Selected event for details/actions
   */
  selectedEvent = signal<Event | null>(null);

  /**
   * Create event dialog visibility
   */
  createEventDialogVisible = signal<boolean>(false);

  /**
   * All tasks for task selection dropdown
   */
  allTasks = signal<Task[]>([]);

  /**
   * Create event form - selected task ID (required)
   */
  newEventTaskId = signal<string | undefined>(undefined);

  /**
   * Create event form - due date
   */
  newEventDueDate = signal<Date | undefined>(undefined);

  /**
   * Create event form - priority (optional, defaults to task priority)
   */
  newEventPriority = signal<Priority | undefined>(undefined);

  /**
   * Create event form - assigned member ID
   */
  newEventAssignedToId = signal<string | undefined>(undefined);

  /**
   * Create event form - notes
   */
  newEventNotes = signal<string>('');

  /**
   * Current user ID (for createdBy field)
   * TODO: Get from auth service when implemented
   */
  currentUserId = signal<string>('1'); // Mock value

  /**
   * Filtered events (client-side filtering by search text)
   */
  filteredEvents = computed(() => {
    const events = this.events() || [];
    const searchText = this.searchText().toLowerCase().trim();

    if (!searchText) {
      return events;
    }

    return events.filter(event => {
      // Search in task name
      if (event.taskName?.toLowerCase().includes(searchText)) {
        return true;
      }

      // Search in notes
      if (event.notes?.toLowerCase().includes(searchText)) {
        return true;
      }

      // Search in user name
      const userName = this.getUserNameForEvent(event).toLowerCase();
      if (userName.includes(searchText)) {
        return true;
      }

      // Search in category name
      const categoryName = this.getCategoryNameForEvent(event).toLowerCase();
      if (categoryName.includes(searchText)) {
        return true;
      }

      return false;
    });
  });

  /**
   * Total filtered events count
   */
  filteredEventsCount = computed(() => {
    const filtered = this.filteredEvents() || [];
    return filtered.length;
  });

  /**
   * Status counters
   */
  statusCounters = computed(() => {
    const events = this.events() || [];
    return {
      total: events.length,
      pending: events.filter(e => e.status === EventStatus.PENDING).length,
      completed: events.filter(e => e.status === EventStatus.COMPLETED).length,
      postponed: events.filter(e => e.status === EventStatus.POSTPONED).length,
      cancelled: events.filter(e => e.status === EventStatus.CANCELLED).length,
      overdue: events.filter(e => isEventOverdue(e)).length
    };
  });

  /**
   * Assignee filter options
   */
  assigneeOptions = computed(() => {
    const members = this.householdMembers();
    return [
      { label: 'Wszyscy użytkownicy', value: undefined },
      ...members.map(m => ({
        label: `${m.firstName} ${m.lastName}`,
        value: m.id
      }))
    ];
  });

  /**
   * Assignee dropdown options (without "all users" option, for create form)
   */
  assigneeDropdownOptions = computed(() => {
    const members = this.householdMembers();
    return members.map(m => ({
      label: `${m.firstName} ${m.lastName}`,
      value: m.userId
    }));
  });

  /**
   * Task dropdown options for create event form
   */
  taskDropdownOptions = computed(() => {
    const tasks = this.allTasks();
    return tasks.map(task => ({
      label: task.name,
      value: task.id
    }));
  });

  /**
   * Category filter options
   */
  categoryOptions = computed(() => {
    const categories = this.allCategories();
    return [
      { label: 'Wszystkie kategorie', value: undefined },
      ...categories.map(cat => ({ label: cat.name, value: cat.id }))
    ];
  });

  /**
   * Priority filter options
   */
  priorityOptions = [
    { label: 'Wszystkie priorytety', value: undefined },
    { label: getPriorityLabel(Priority.LOW), value: Priority.LOW },
    { label: getPriorityLabel(Priority.MEDIUM), value: Priority.MEDIUM },
    { label: getPriorityLabel(Priority.HIGH), value: Priority.HIGH }
  ];

  /**
   * Status filter options
   */
  statusOptions = [
    { label: 'Wszystkie statusy', value: undefined },
    { label: getEventStatusLabel(EventStatus.PENDING), value: EventStatus.PENDING },
    { label: getEventStatusLabel(EventStatus.COMPLETED), value: EventStatus.COMPLETED },
    { label: getEventStatusLabel(EventStatus.POSTPONED), value: EventStatus.POSTPONED },
    { label: getEventStatusLabel(EventStatus.CANCELLED), value: EventStatus.CANCELLED }
  ];

  /**
   * Sort options
   */
  sortOptions = [
    { label: 'Data terminu', value: 'dueDate' },
    { label: 'Status', value: 'status' },
    { label: 'Priorytet', value: 'priority' },
    { label: 'Data utworzenia', value: 'createdAt' }
  ];

  /**
   * Helper functions exposed to template
   */
  readonly getEventStatusLabel = getEventStatusLabel;
  readonly getEventStatusSeverity = getEventStatusSeverity;
  readonly getPriorityLabel = getPriorityLabel;
  readonly getPrioritySeverity = getPrioritySeverity;
  readonly getEventUrgency = getEventUrgency;
  readonly getUrgencySeverity = getUrgencySeverity;
  readonly formatEventDate = formatEventDate;
  readonly isEventOverdue = isEventOverdue;

  /**
   * Get user name for event (by assignedTo ID)
   */
  getUserNameForEvent(event: Event): string {
    const member = this.householdMembers().find(m => m.userId === event.assignedTo);
    return member ? `${member.firstName} ${member.lastName}` : 'Nieznany użytkownik';
  }

  /**
   * Get creator name for event (by createdBy ID)
   */
  getCreatorNameForEvent(event: Event): string {
    const member = this.householdMembers().find(m => m.userId === event.createdBy);
    return member ? `${member.firstName} ${member.lastName}` : 'Nieznany użytkownik';
  }

  /**
   * Get category name for event (from task)
   * Note: API doesn't return category info in event, so we need to get it from tasks
   */
  getCategoryNameForEvent(event: Event): string {
    const task = this.allTasks().find(t => t.id === event.taskId);
    return task?.category?.name || 'Brak kategorii';
  }

  /**
   * Get task description for event (from task)
   */
  getTaskDescriptionForEvent(event: Event): string | null {
    const task = this.allTasks().find(t => t.id === event.taskId);
    return task?.description || null;
  }

  ngOnInit(): void {
    // Get household ID from route
    this.route.params.subscribe(params => {
      const id = params['householdId'];
      if (id) {
        this.householdId.set(id);
        this.loadCategories();
        this.loadHouseholdMembers();
        this.loadTasks();
        this.loadEvents();
      }
    });
  }

  /**
   * Load categories from API
   */
  private loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (categories) => {
        this.allCategories.set(categories || []);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.allCategories.set([]); // Set empty array on error
      }
    });
  }

  /**
   * Load household members for assignee filter
   */
  private loadHouseholdMembers(): void {
    this.householdService.getHouseholdMembers(this.householdId()).subscribe({
      next: (members) => {
        this.householdMembers.set(members || []);

        // Set default assignee to current user (first member for now)
        // TODO: Get from auth service when implemented
        if (members && members.length > 0) {
          this.currentUserId.set(members[0].userId);
          this.newEventAssignedToId.set(members[0].userId);
        }
      },
      error: (error) => {
        console.error('Error loading household members:', error);
        this.householdMembers.set([]); // Set empty array on error
      }
    });
  }

  /**
   * Load tasks for task selection dropdown
   */
  private loadTasks(): void {
    this.tasksService.getTasks({ householdId: this.householdId(), isActive: true }).subscribe({
      next: (response) => {
        this.allTasks.set(response?.data || []);
      },
      error: (error) => {
        console.error('Error loading tasks:', error);
        this.allTasks.set([]); // Set empty array on error
      }
    });
  }

  /**
   * Load events from API
   */
  private loadEvents(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const queryParams = {
      householdId: this.householdId(),
      assignedToId: this.selectedAssigneeId(),
      categoryId: this.selectedCategoryId(),
      priority: this.selectedPriority(),
      status: this.selectedStatus(),
      startDate: this.startDate()?.toISOString(),
      endDate: this.endDate()?.toISOString(),
      isOverdue: this.showOverdueOnly() ? true : undefined,
      sortBy: this.sortBy(),
      sortOrder: this.sortOrder(),
      page: this.currentPage(),
      limit: this.itemsPerPage()
    };

    this.eventsService.getEvents(queryParams).subscribe({
      next: (response) => {
        // Set events data with fallback to empty array
        this.events.set(response?.data || []);

        // Set pagination with fallback values
        if (response?.pagination) {
          this.currentPage.set(response.pagination.currentPage || 1);
          this.totalPages.set(response.pagination.totalPages || 1);
          this.totalItems.set(response.pagination.totalItems || 0);
          this.itemsPerPage.set(response.pagination.itemsPerPage || 20);
        } else {
          // No pagination in response, use defaults
          this.currentPage.set(1);
          this.totalPages.set(1);
          this.totalItems.set(response?.data?.length || 0);
          this.itemsPerPage.set(20);
        }

        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading events:', error);
        this.errorMessage.set('Nie udało się załadować listy wydarzeń. Spróbuj ponownie później.');
        this.events.set([]); // Set empty array on error
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Handle search
   */
  onSearch(text: string): void {
    this.searchText.set(text);
  }

  /**
   * Handle assignee filter change
   */
  onAssigneeFilterChange(assigneeId: string | undefined): void {
    this.selectedAssigneeId.set(assigneeId);
    this.currentPage.set(1);
    this.loadEvents();
  }

  /**
   * Handle category filter change
   */
  onCategoryFilterChange(categoryId: number | undefined): void {
    this.selectedCategoryId.set(categoryId);
    this.currentPage.set(1);
    this.loadEvents();
  }

  /**
   * Handle priority filter change
   */
  onPriorityFilterChange(priority: Priority | undefined): void {
    this.selectedPriority.set(priority);
    this.currentPage.set(1);
    this.loadEvents();
  }

  /**
   * Handle status filter change
   */
  onStatusFilterChange(status: EventStatus | undefined): void {
    this.selectedStatus.set(status);
    this.currentPage.set(1);
    this.loadEvents();
  }

  /**
   * Handle date range filter change
   */
  onDateRangeChange(): void {
    this.currentPage.set(1);
    this.loadEvents();
  }

  /**
   * Toggle overdue filter
   */
  toggleOverdueFilter(): void {
    this.showOverdueOnly.set(!this.showOverdueOnly());
    this.currentPage.set(1);
    this.loadEvents();
  }

  /**
   * Handle sort change
   */
  onSortChange(sortBy: 'dueDate' | 'status' | 'priority' | 'createdAt'): void {
    // If same field, toggle order
    if (this.sortBy() === sortBy) {
      this.sortOrder.set(this.sortOrder() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(sortBy);
      this.sortOrder.set('asc');
    }
    this.currentPage.set(1);
    this.loadEvents();
  }

  /**
   * Handle page change
   */
  onPageChange(event: PaginatorState): void {
    if (event.page !== undefined && event.rows !== undefined) {
      this.currentPage.set(event.page + 1); // PrimeNG paginator is 0-indexed
      this.itemsPerPage.set(event.rows);
      this.loadEvents();
    }
  }

  /**
   * Open event details dialog
   */
  openEventDetails(event: Event): void {
    this.selectedEvent.set(event);
    this.eventDetailsDialogVisible.set(true);
  }

  /**
   * Hide event details dialog
   */
  hideEventDetailsDialog(): void {
    this.eventDetailsDialogVisible.set(false);
    this.selectedEvent.set(null);
  }

  /**
   * Complete event
   */
  completeEvent(event: Event): void {
    if (confirm(`Czy na pewno chcesz oznaczyć wydarzenie "${event.taskName}" jako wykonane?`)) {
      this.eventsService.completeEvent(event.id, {
        completionDate: new Date().toISOString()
      }).subscribe({
        next: (response) => {
          console.log('Event completed:', response);
          this.refresh();
        },
        error: (error) => {
          console.error('Error completing event:', error);
          const errorMessage = error.error?.error || 'Nie udało się oznaczyć wydarzenia jako wykonane. Spróbuj ponownie.';
          alert(errorMessage);
        }
      });
    }
  }

  /**
   * Postpone event (simplified version - opens dialog in real implementation)
   */
  postponeEvent(event: Event): void {
    const days = prompt('O ile dni przełożyć wydarzenie?', '7');
    if (days) {
      const daysNumber = parseInt(days, 10);
      if (isNaN(daysNumber) || daysNumber <= 0) {
        alert('Nieprawidłowa liczba dni.');
        return;
      }

      const newDueDate = new Date(event.dueDate);
      newDueDate.setDate(newDueDate.getDate() + daysNumber);

      const reason = prompt('Podaj powód przełożenia:', '');
      if (!reason || reason.trim() === '') {
        alert('Powód przełożenia jest wymagany.');
        return;
      }

      this.eventsService.postponeEvent(event.id, {
        newDueDate: newDueDate.toISOString(),
        reason: reason.trim()
      }).subscribe({
        next: (updatedEvent) => {
          console.log('Event postponed:', updatedEvent);
          this.refresh();
        },
        error: (error) => {
          console.error('Error postponing event:', error);
          const errorMessage = error.error?.error || 'Nie udało się przełożyć wydarzenia. Spróbuj ponownie.';
          alert(errorMessage);
        }
      });
    }
  }

  /**
   * Cancel event
   */
  cancelEvent(event: Event): void {
    const reason = prompt('Podaj powód anulowania:', '');
    if (reason && reason.trim() !== '') {
      this.eventsService.cancelEvent(event.id, {
        reason: reason.trim()
      }).subscribe({
        next: (updatedEvent) => {
          console.log('Event cancelled:', updatedEvent);
          this.refresh();
        },
        error: (error) => {
          console.error('Error cancelling event:', error);
          const errorMessage = error.error?.error || 'Nie udało się anulować wydarzenia. Spróbuj ponownie.';
          alert(errorMessage);
        }
      });
    }
  }

  /**
   * Delete event
   */
  deleteEvent(event: Event): void {
    if (confirm(`Czy na pewno chcesz usunąć wydarzenie "${event.taskName}"?`)) {
      this.eventsService.deleteEvent(event.id).subscribe({
        next: () => {
          // Remove from local state
          const events = this.events();
          this.events.set(events.filter(e => e.id !== event.id));
          this.totalItems.set(this.totalItems() - 1);
        },
        error: (error) => {
          console.error('Error deleting event:', error);
          const errorMessage = error.error?.error || 'Nie udało się usunąć wydarzenia. Spróbuj ponownie.';
          alert(errorMessage);
        }
      });
    }
  }

  /**
   * Refresh events list
   */
  refresh(): void {
    this.loadEvents();
  }

  /**
   * Reset all filters
   */
  resetFilters(): void {
    this.selectedAssigneeId.set(undefined);
    this.selectedCategoryId.set(undefined);
    this.selectedPriority.set(undefined);
    this.selectedStatus.set(undefined);
    this.startDate.set(undefined);
    this.endDate.set(undefined);
    this.showOverdueOnly.set(false);
    this.searchText.set('');
    this.sortBy.set('dueDate');
    this.sortOrder.set('asc');
    this.currentPage.set(1);
    this.loadEvents();
  }

  /**
   * Open create event dialog
   */
  openCreateEventDialog(): void {
    // Reset form
    this.newEventTaskId.set(undefined);
    this.newEventDueDate.set(undefined);
    this.newEventPriority.set(undefined);
    this.newEventNotes.set('');

    // Set default assignee to current user
    this.newEventAssignedToId.set(this.currentUserId());

    // Open dialog
    this.createEventDialogVisible.set(true);
  }

  /**
   * Close create event dialog
   */
  closeCreateEventDialog(): void {
    this.createEventDialogVisible.set(false);
  }

  /**
   * Submit create event form
   */
  submitCreateEvent(): void {
    // Validate required fields
    if (!this.newEventTaskId()) {
      alert('Proszę wybrać zadanie.');
      return;
    }

    if (!this.newEventDueDate()) {
      alert('Proszę wybrać datę wykonania.');
      return;
    }

    // Get selected task to determine priority if not set
    const selectedTask = this.allTasks().find(t => t.id === this.newEventTaskId());
    const priority = this.newEventPriority() || selectedTask?.priority || Priority.MEDIUM;

    // Build CreateEventDto
    // Format dueDate as YYYY-MM-DD (DateOnly format for backend)
    const dueDate = this.newEventDueDate()!;
    const dueDateString = `${dueDate.getFullYear()}-${String(dueDate.getMonth() + 1).padStart(2, '0')}-${String(dueDate.getDate()).padStart(2, '0')}`;

    const createDto: CreateEventDto = {
      householdId: this.householdId(),
      taskId: this.newEventTaskId()!,
      assignedTo: this.newEventAssignedToId() || undefined,
      dueDate: dueDateString,
      notes: this.newEventNotes() || undefined,
      priority: priority,
      createdBy: this.currentUserId()
    };

    // Call service to create event
    this.eventsService.createEvent(createDto).subscribe({
      next: (newEvent) => {
        console.log('Event created:', newEvent);
        // Close dialog
        this.closeCreateEventDialog();
        // Refresh events list
        this.refresh();
      },
      error: (error) => {
        console.error('Error creating event:', error);
        const errorMessage = error.error?.error || 'Nie udało się utworzyć wydarzenia. Spróbuj ponownie.';
        alert(errorMessage);
      }
    });
  }
}
