import { Component, inject, signal, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

// PrimeNG Components
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';

// Models
import { Task, Priority, getPriorityLabel, getPrioritySeverity, formatInterval, hasInterval } from '../../models/task.model';
import { Category, CategoryType } from '../../../categories/models/category.model';

// Services
import { EventsService } from '../../../events/services/events.service';
import { HouseholdService } from '../../../../../core/services/household.service';
import { CreateEventDto } from '../../../events/models/event.model';
import { AuthService, UserProfile } from '../../../../../core/services/auth.service';
import { TasksService } from '../../services/tasks.service';
import { CategoryService } from '../../../categories/services/category.service';

/**
 * CreateEventFromTaskDialogComponent
 *
 * Dialog for creating an event (scheduled occurrence) from a task template.
 * Features:
 * - Display task information (name, category, interval)
 * - Assign to household member (dropdown)
 * - Set due date (datepicker)
 * - Inherit priority from task (with option to change)
 * - Add notes
 * - Form validation
 * - Toast notifications
 *
 * NOTE: This component requires Events API and models to be implemented.
 * Currently contains placeholders for future implementation.
 */
@Component({
  selector: 'app-create-event-from-task-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    SelectModule,
    DatePickerModule,
    TextareaModule,
    ToastModule,
    DividerModule,
    TagModule
  ],
  providers: [MessageService],
  templateUrl: './create-event-from-task-dialog.component.html',
  styleUrl: './create-event-from-task-dialog.component.scss'
})
export class CreateEventFromTaskDialogComponent implements OnInit, OnChanges {
  @Input() visible = false;
  @Input() task: Task | null = null;
  @Input() householdId: string | null = null;
  @Input() initialDate: Date | null = null;

  @Output() onClose = new EventEmitter<void>();
  @Output() onCreated = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);
  private householdService = inject(HouseholdService);
  private eventsService = inject(EventsService);
  private authService = inject(AuthService);
  private tasksService = inject(TasksService);
  private categoryService = inject(CategoryService);

  createEventForm: FormGroup;
  isLoading = signal<boolean>(false);
  currentUser = signal<UserProfile | null>(null);

  /**
   * Household members loaded from API
   */
  householdMembers = signal<Array<{ id: string; userId: string; firstName: string; lastName: string; email: string; role: string }>>([]);

  /**
   * All tasks loaded from API (when no task provided)
   */
  allTasks = signal<Task[]>([]);

  /**
   * All category types loaded from API
   */
  allCategoryTypes = signal<CategoryType[]>([]);

  /**
   * All categories loaded from API
   */
  allCategories = signal<Category[]>([]);

  /**
   * Selected category type ID
   */
  selectedCategoryTypeId = signal<number | null>(null);

  /**
   * Selected category ID
   */
  selectedCategoryId = signal<number | null>(null);

  /**
   * Priority options for dropdown
   */
  priorityOptions = [
    { label: getPriorityLabel(Priority.LOW), value: Priority.LOW },
    { label: getPriorityLabel(Priority.MEDIUM), value: Priority.MEDIUM },
    { label: getPriorityLabel(Priority.HIGH), value: Priority.HIGH }
  ];

  /**
   * Household member options for dropdown
   */
  memberOptions = computed(() => {
    return this.householdMembers().map(member => ({
      label: `${member.firstName} ${member.lastName}`,
      value: member.userId
    }));
  });

  /**
   * Category type options for dropdown
   */
  categoryTypeOptions = computed(() => {
    return this.allCategoryTypes().map(categoryType => ({
      label: categoryType.name,
      value: categoryType.id
    }));
  });

  /**
   * Category options filtered by selected category type
   */
  categoryOptions = computed(() => {
    const selectedTypeId = this.selectedCategoryTypeId();
    if (!selectedTypeId) return [];

    return this.allCategories()
      .filter(category => category.categoryTypeId === selectedTypeId)
      .map(category => ({
        label: category.name,
        value: category.id
      }));
  });

  /**
   * One-time tasks (no interval) filtered by selected category
   */
  oneTimeTasks = computed(() => {
    const selectedCategoryId = this.selectedCategoryId();
    if (!selectedCategoryId) return [];

    return this.allTasks().filter(task =>
      task.category.id === selectedCategoryId && !hasInterval(task)
    );
  });

  /**
   * Task options for dropdown (filtered by category and only one-time tasks)
   */
  taskOptions = computed(() => {
    return this.oneTimeTasks().map(task => ({
      label: task.name,
      value: task.id
    }));
  });

  /**
   * Minimum date for due date picker (today)
   */
  minDate = new Date();

  /**
   * Helper functions
   */
  readonly formatInterval = formatInterval;
  readonly getPriorityLabel = getPriorityLabel;
  readonly getPrioritySeverity = getPrioritySeverity;

  constructor() {
    // Initialize form - taskId will be added conditionally in ngOnInit
    this.createEventForm = this.fb.group({
      assignedTo: [null, [Validators.required]],
      dueDate: [null, [Validators.required]],
      priority: [Priority.MEDIUM, [Validators.required]],
      notes: ['']
    });
  }

  ngOnInit(): void {
    // Load household members
    this.loadHouseholdMembers();
    this.currentUser.set(this.authService.getCurrentUser() || null);

    // Initial setup
    this.setupForm();

    // Load category types and categories (needed for cascading dropdowns)
    this.loadCategoryTypes();
    this.loadCategories();
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Handle changes to @Input() properties
    if (changes['task'] || changes['initialDate']) {
      this.setupForm();
    }
  }

  private setupForm(): void {
    // Add or remove controls based on whether task is provided
    if (!this.task) {
      // No task provided - need cascading selection: CategoryType → Category → Task
      if (!this.createEventForm.get('categoryTypeId')) {
        this.createEventForm.addControl('categoryTypeId', this.fb.control(null, [Validators.required]));
      }
      if (!this.createEventForm.get('categoryId')) {
        this.createEventForm.addControl('categoryId', this.fb.control(null, [Validators.required]));
      }
      if (!this.createEventForm.get('taskId')) {
        this.createEventForm.addControl('taskId', this.fb.control(null, [Validators.required]));
      }
      // Load available tasks
      this.loadAvailableTasks();
    } else {
      // Task is provided - remove cascading selection controls
      if (this.createEventForm.get('categoryTypeId')) {
        this.createEventForm.removeControl('categoryTypeId');
      }
      if (this.createEventForm.get('categoryId')) {
        this.createEventForm.removeControl('categoryId');
      }
      if (this.createEventForm.get('taskId')) {
        this.createEventForm.removeControl('taskId');
      }
    }

    // Initialize form with task priority and initial date
    const patchData: any = {};

    if (this.task) {
      patchData.priority = this.task.priority;
    }

    if (this.initialDate) {
      patchData.dueDate = this.initialDate;
    }

    if (Object.keys(patchData).length > 0) {
      this.createEventForm.patchValue(patchData);
    }

    // Reset selections when setting up form
    this.selectedCategoryTypeId.set(null);
    this.selectedCategoryId.set(null);
  }

  /**
   * Load household members from API
   */
  private loadHouseholdMembers(): void {
    if (!this.householdId) {
      console.warn('Cannot load household members: householdId is null');
      return;
    }

    console.log('loadHouseholdMembers');

    this.householdService.getHouseholdMembers(this.householdId).subscribe({
      next: (members) => {
        this.householdMembers.set(members || []);
        console.log('Loaded household members:', members);
      },
      error: (error) => {
        console.error('Error loading household members:', error);
        this.householdMembers.set([]); // Set empty array on error
        this.messageService.add({
          severity: 'error',
          summary: 'Błąd',
          detail: 'Nie udało się załadować listy członków gospodarstwa'
        });
      }
    });
  }

  /**
   * Load available tasks from API (all tasks for the household)
   */
  private loadAvailableTasks(): void {
    if (!this.householdId) {
      console.warn('Cannot load tasks: householdId is null');
      return;
    }

    console.log('loadAvailableTasks');

    this.tasksService.getTasks({ householdId: this.householdId }).subscribe({
      next: (response) => {
        this.allTasks.set(response.data || []);
        console.log('Loaded available tasks:', response.data);
      },
      error: (error) => {
        console.error('Error loading tasks:', error);
        this.allTasks.set([]);
        this.messageService.add({
          severity: 'error',
          summary: 'Błąd',
          detail: 'Nie udało się załadować listy zadań'
        });
      }
    });
  }

  /**
   * Load category types from API
   */
  private loadCategoryTypes(): void {
    if (!this.householdId) {
      this.allCategoryTypes.set([]);
      return;
    }

    this.categoryService.getCategoryTypes(this.householdId).subscribe({
      next: (categoryTypes) => {
        this.allCategoryTypes.set(categoryTypes || []);
        console.log('Loaded category types:', categoryTypes);
      },
      error: (error) => {
        console.error('Error loading category types:', error);
        this.allCategoryTypes.set([]);
        this.messageService.add({
          severity: 'error',
          summary: 'Błąd',
          detail: 'Nie udało się załadować typów kategorii'
        });
      }
    });
  }

  /**
   * Load categories from API
   */
  private loadCategories(): void {
    if (!this.householdId) {
      this.allCategories.set([]);
      return;
    }

    this.categoryService.getCategories(this.householdId).subscribe({
      next: (categories) => {
        this.allCategories.set(categories || []);
        console.log('Loaded categories:', categories);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.allCategories.set([]);
        this.messageService.add({
          severity: 'error',
          summary: 'Błąd',
          detail: 'Nie udało się załadować kategorii'
        });
      }
    });
  }

  /**
   * Handle category type selection change
   */
  onCategoryTypeChange(categoryTypeId: number | null): void {
    this.selectedCategoryTypeId.set(categoryTypeId);
    // Reset category and task selections
    this.selectedCategoryId.set(null);
    this.createEventForm.patchValue({
      categoryId: null,
      taskId: null
    });
  }

  /**
   * Handle category selection change
   */
  onCategoryChange(categoryId: number | null): void {
    this.selectedCategoryId.set(categoryId);
    // Reset task selection
    this.createEventForm.patchValue({
      taskId: null
    });
  }

  /**
   * Submit form
   */
  onSubmit(): void {
    if (this.createEventForm.invalid) {
      this.createEventForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.createEventForm.value;

    // Get taskId from either the task prop or the form
    const taskId = this.task ? this.task.id : formValue.taskId;

    if (!taskId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Błąd',
        detail: 'Nie wybrano zadania'
      });
      this.isLoading.set(false);
      return;
    }

    // Build CreateEventDto
    // Format dueDate as YYYY-MM-DD (DateOnly format for backend)
    const dueDate = formValue.dueDate;
    const dueDateString = `${dueDate.getFullYear()}-${String(dueDate.getMonth() + 1).padStart(2, '0')}-${String(dueDate.getDate()).padStart(2, '0')}`;

    const createDto: CreateEventDto = {
      householdId: this.householdId!,
      taskId: taskId,
      assignedTo: formValue.assignedTo || undefined,
      dueDate: dueDateString,
      notes: formValue.notes?.trim() || undefined,
      priority: formValue.priority,
      createdBy: this.currentUser()?.id || ''
    };

    // Call service to create event
    this.eventsService.createEvent(createDto).subscribe({
      next: (newEvent) => {
        console.log('Event created:', newEvent);
        this.messageService.add({
          severity: 'success',
          summary: 'Sukces',
          detail: 'Wydarzenie zostało utworzone pomyślnie'
        });
        this.onCreated.emit();
        this.closeDialog();
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error creating event:', error);
        const errorMessage = error.error?.error || 'Nie udało się utworzyć wydarzenia. Spróbuj ponownie.';
        alert(errorMessage);
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Close dialog and reset form
   */
  closeDialog(): void {
    this.createEventForm.reset({
      priority: this.task?.priority || Priority.MEDIUM
    });
    // Reset cascading selection signals
    this.selectedCategoryTypeId.set(null);
    this.selectedCategoryId.set(null);
    this.onClose.emit();
  }

  /**
   * Helper method to debug form validation errors
   */
  getFormValidationErrors(): any {
    const errors: any = {};
    Object.keys(this.createEventForm.controls).forEach(key => {
      const control = this.createEventForm.get(key);
      if (control && control.invalid) {
        errors[key] = control.errors;
      }
    });
    return errors;
  }
}
