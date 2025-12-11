import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';

// Models
import {
  Task,
  Priority,
  hasInterval,
  formatInterval,
  getPriorityLabel,
  getPrioritySeverity
} from './models/task.model';

// Services
import { TasksService } from './services/tasks.service';
import { CategoryService } from '../categories/services/category.service';
import { Category } from '../categories/models/category.model';

// Components
import { CreateTaskDialogComponent } from './components/create-task-dialog/create-task-dialog.component';
import { EditTaskDialogComponent } from './components/edit-task-dialog/edit-task-dialog.component';
import { CreateEventFromTaskDialogComponent } from './components/create-event-from-task-dialog/create-event-from-task-dialog.component';

/**
 * TasksListComponent
 *
 * Main component for displaying and managing task templates.
 * Based on API plan - GET /tasks, POST /tasks, PUT /tasks/{id}, DELETE /tasks/{id}
 *
 * Features:
 * - Display all tasks for household
 * - Sorting: by name, category, priority, creation date
 * - Filtering: by category, priority, with/without interval
 * - Create new task
 * - Edit task
 * - Delete task
 * - Create event from task
 * - Pagination support
 *
 * Route: /:householdId/tasks
 *
 * @example
 * // Route configuration
 * {
 *   path: ':householdId/tasks',
 *   loadComponent: () => import('./tasks-list.component').then(m => m.TasksListComponent)
 * }
 */
@Component({
  selector: 'app-tasks-list',
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    SelectModule,
    InputTextModule,
    DividerModule,
    MessageModule,
    SkeletonModule,
    TagModule,
    TooltipModule,
    DialogModule,
    PaginatorModule,
    CreateTaskDialogComponent,
    EditTaskDialogComponent,
    CreateEventFromTaskDialogComponent
  ],
  templateUrl: './tasks-list.component.html',
  styleUrl: './tasks-list.component.scss'
})
export class TasksListComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private tasksService = inject(TasksService);
  private categoryService = inject(CategoryService);

  /**
   * Household ID from route
   */
  householdId = signal<string>('');

  /**
   * All tasks (current page)
   */
  tasks = signal<Task[]>([]);

  /**
   * All categories for filter dropdown
   */
  allCategories = signal<Category[]>([]);

  /**
   * Selected category filter
   */
  selectedCategoryId = signal<number | undefined>(undefined);

  /**
   * Selected priority filter
   */
  selectedPriority = signal<Priority | undefined>(undefined);

  /**
   * Selected interval filter
   */
  selectedIntervalFilter = signal<'all' | 'with' | 'without'>('all');

  /**
   * Search text
   */
  searchText = signal<string>('');

  /**
   * Sort by field
   */
  sortBy = signal<'name' | 'priority' | 'createdAt'>('createdAt');

  /**
   * Sort order
   */
  sortOrder = signal<'asc' | 'desc'>('desc');

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
   * Create task dialog visibility
   */
  createDialogVisible = signal<boolean>(false);

  /**
   * Edit task dialog visibility
   */
  editDialogVisible = signal<boolean>(false);

  /**
   * Task to edit
   */
  taskToEdit = signal<Task | null>(null);

  /**
   * Create event dialog visibility
   */
  createEventDialogVisible = signal<boolean>(false);

  /**
   * Task to create event from
   */
  taskForEvent = signal<Task | null>(null);

  /**
   * Filtered tasks (client-side filtering by search text)
   */
  filteredTasks = computed(() => {
    const tasks = this.tasks();
    const searchText = this.searchText().toLowerCase().trim();

    if (!searchText) {
      return tasks;
    }

    return tasks.filter(task =>
      task.name.toLowerCase().includes(searchText) ||
      task.description?.toLowerCase().includes(searchText) ||
      task.category.name.toLowerCase().includes(searchText)
    );
  });

  /**
   * Total filtered tasks count
   */
  filteredTasksCount = computed(() => this.filteredTasks().length);

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
   * Interval filter options
   */
  intervalOptions = [
    { label: 'Wszystkie zadania', value: 'all' },
    { label: 'Z interwałem', value: 'with' },
    { label: 'Jednorazowe', value: 'without' }
  ];

  /**
   * Sort options
   */
  sortOptions = [
    { label: 'Data utworzenia', value: 'createdAt' },
    { label: 'Nazwa', value: 'name' },
    { label: 'Priorytet', value: 'priority' }
  ];

  /**
   * Helper functions exposed to template
   */
  readonly hasInterval = hasInterval;
  readonly formatInterval = formatInterval;
  readonly getPriorityLabel = getPriorityLabel;
  readonly getPrioritySeverity = getPrioritySeverity;

  ngOnInit(): void {
    // Get household ID from route
    this.route.params.subscribe(params => {
      const id = params['householdId'];
      if (id) {
        this.householdId.set(id);
        this.loadCategories();
        this.loadTasks();
      }
    });
  }

  /**
   * Load categories from API
   */
  private loadCategories(): void {
    const householdId = this.householdId();
    if (!householdId) {
      this.allCategories.set([]);
      return;
    }

    this.categoryService.getCategories(householdId).subscribe({
      next: (categories) => {
        this.allCategories.set(categories);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  /**
   * Load tasks from API
   */
  private loadTasks(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const queryParams = {
      householdId: this.householdId(),
      categoryId: this.selectedCategoryId(),
      priority: this.selectedPriority(),
      hasInterval: this.selectedIntervalFilter() === 'all'
        ? undefined
        : this.selectedIntervalFilter() === 'with',
      isActive: true,
      sortBy: this.sortBy(),
      sortOrder: this.sortOrder(),
      page: this.currentPage(),
      limit: this.itemsPerPage()
    };

    this.tasksService.getTasks(queryParams).subscribe({
      next: (response) => {
        this.tasks.set(response.data);
        this.currentPage.set(response.pagination.currentPage);
        this.totalPages.set(response.pagination.totalPages);
        this.totalItems.set(response.pagination.totalItems);
        this.itemsPerPage.set(response.pagination.itemsPerPage);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading tasks:', error);
        this.errorMessage.set('Nie udało się załadować listy zadań. Spróbuj ponownie później.');
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
   * Handle category filter change
   */
  onCategoryFilterChange(categoryId: number | undefined): void {
    this.selectedCategoryId.set(categoryId);
    this.currentPage.set(1); // Reset to first page
    this.loadTasks();
  }

  /**
   * Handle priority filter change
   */
  onPriorityFilterChange(priority: Priority | undefined): void {
    this.selectedPriority.set(priority);
    this.currentPage.set(1); // Reset to first page
    this.loadTasks();
  }

  /**
   * Handle interval filter change
   */
  onIntervalFilterChange(filter: 'all' | 'with' | 'without'): void {
    this.selectedIntervalFilter.set(filter);
    this.currentPage.set(1); // Reset to first page
    this.loadTasks();
  }

  /**
   * Handle sort change
   */
  onSortChange(sortBy: 'name' | 'priority' | 'createdAt'): void {
    // If same field, toggle order
    if (this.sortBy() === sortBy) {
      this.sortOrder.set(this.sortOrder() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(sortBy);
      this.sortOrder.set('asc');
    }
    this.currentPage.set(1); // Reset to first page
    this.loadTasks();
  }

  /**
   * Handle page change
   */
  onPageChange(event: PaginatorState): void {
    if (event.page !== undefined && event.rows !== undefined) {
      this.currentPage.set(event.page + 1); // PrimeNG paginator is 0-indexed
      this.itemsPerPage.set(event.rows);
      this.loadTasks();
    }
  }

  /**
   * Open add new task dialog
   */
  addNewTask(): void {
    this.createDialogVisible.set(true);
  }

  /**
   * Hide create task dialog
   */
  hideCreateDialog(): void {
    this.createDialogVisible.set(false);
  }

  /**
   * Handle task created event
   */
  onTaskCreated(): void {
    this.hideCreateDialog();
    this.refresh();
  }

  /**
   * Edit task
   */
  editTask(task: Task): void {
    this.taskToEdit.set(task);
    this.editDialogVisible.set(true);
  }

  /**
   * Hide edit task dialog
   */
  hideEditDialog(): void {
    this.editDialogVisible.set(false);
    this.taskToEdit.set(null);
  }

  /**
   * Handle task updated event
   */
  onTaskUpdated(): void {
    this.hideEditDialog();
    this.refresh();
  }

  /**
   * Delete task
   */
  deleteTask(task: Task): void {
    if (confirm(`Czy na pewno chcesz usunąć zadanie "${task.name}"?`)) {
      this.tasksService.deleteTask(task.id).subscribe({
        next: () => {
          // Remove from local state
          const tasks = this.tasks();
          this.tasks.set(tasks.filter(t => t.id !== task.id));
          this.totalItems.set(this.totalItems() - 1);
        },
        error: (error) => {
          console.error('Error deleting task:', error);
          const errorMessage = error.error?.error || 'Nie udało się usunąć zadania. Spróbuj ponownie.';
          alert(errorMessage);
        }
      });
    }
  }

  /**
   * Open create event from task dialog
   */
  createEventFromTask(task: Task): void {
    this.taskForEvent.set(task);
    this.createEventDialogVisible.set(true);
  }

  /**
   * Hide create event dialog
   */
  hideCreateEventDialog(): void {
    this.createEventDialogVisible.set(false);
    this.taskForEvent.set(null);
  }

  /**
   * Handle event created from task
   */
  onEventCreated(): void {
    this.hideCreateEventDialog();
    // Success message is shown by the dialog itself
  }

  /**
   * Refresh tasks list
   */
  refresh(): void {
    this.loadTasks();
  }

  /**
   * Reset all filters
   */
  resetFilters(): void {
    this.selectedCategoryId.set(undefined);
    this.selectedPriority.set(undefined);
    this.selectedIntervalFilter.set('all');
    this.searchText.set('');
    this.sortBy.set('createdAt');
    this.sortOrder.set('desc');
    this.currentPage.set(1);
    this.loadTasks();
  }
}
