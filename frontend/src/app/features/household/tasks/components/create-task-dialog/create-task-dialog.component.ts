import { Component, inject, signal, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, computed, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

// PrimeNG Components
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { TreeSelectModule } from 'primeng/treeselect';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { MessageService, TreeNode } from 'primeng/api';
import { DividerModule } from 'primeng/divider';
import { DatePickerModule } from 'primeng/datepicker';

// Services
import { TasksService } from '../../services/tasks.service';
import { AuthService } from '../../../../../core/services/auth.service';
import { HouseholdService, HouseholdMember } from '../../../../../core/services/household.service';

// Models
import { Category } from '../../../categories/models/category.model';
import { Priority, CreateTaskDto, getPriorityLabel } from '../../models/task.model';

/**
 * CreateTaskDialogComponent
 *
 * Dialog for creating new task templates.
 * Features:
 * - Category selection (subcategory dropdown)
 * - Task name and description
 * - Priority selection
 * - Interval configuration (years, months, weeks, days)
 * - Notes field
 * - Form validation
 * - Toast notifications
 */
@Component({
  selector: 'app-create-task-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    SelectModule,
    TreeSelectModule,
    InputNumberModule,
    ToastModule,
    DividerModule,
    DatePickerModule
  ],
  providers: [MessageService],
  templateUrl: './create-task-dialog.component.html',
  styleUrl: './create-task-dialog.component.scss'
})
export class CreateTaskDialogComponent implements OnInit, OnChanges {
  @Input() visible = false;
  @Input() householdId: string | null = null;
  categories = input<Category[]>([]);

  @Output() onClose = new EventEmitter<void>();
  @Output() onCreated = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private tasksService = inject(TasksService);
  private authService = inject(AuthService);
  private householdService = inject(HouseholdService);
  private messageService = inject(MessageService);

  createTaskForm: FormGroup;
  isLoading = signal<boolean>(false);
  householdMembers = signal<HouseholdMember[]>([]);
  minStartDate = new Date();

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
   * Category tree nodes for tree select
   * Groups categories by category type
   */
  categoryTreeNodes = computed<TreeNode[]>(() => {
    // Group categories by category type
    const grouped = this.categories().reduce((acc, cat) => {
      const typeId = cat.categoryTypeId;
      const typeName = cat.categoryTypeName || 'Inne';

      if (!acc[typeId]) {
        acc[typeId] = {
          typeId,
          typeName,
          categories: []
        };
      }
      acc[typeId].categories.push(cat);
      return acc;
    }, {} as Record<string, { typeId: string; typeName: string; categories: Category[] }>);

    // Convert to TreeNode structure
    return Object.values(grouped).map(group => ({
      label: group.typeName,
      data: null,
      selectable: false, // Category types are not selectable
      children: group.categories.map(cat => ({
        label: cat.name,
        data: cat.id, // Store category ID in data field
        selectable: true
      }))
    }));
  });

  constructor() {
    this.createTaskForm = this.fb.group({
      categoryId: [null, [Validators.required]],
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: [''],
      priority: [Priority.MEDIUM, [Validators.required]],
      assignedTo: [null], // Optional field for default user assignment
      startDate: [null],
      yearsValue: [0, [Validators.min(0)]],
      monthsValue: [0, [Validators.min(0)]],
      weeksValue: [0, [Validators.min(0)]],
      daysValue: [0, [Validators.min(0)]],
      notes: ['']
    });
  }

  ngOnInit(): void {
    // Load household members when household ID is available
    if (this.householdId) {
      this.loadHouseholdMembers();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Reload members when dialog becomes visible or household changes
    if ((changes['visible'] && this.visible) || changes['householdId']) {
      if (this.householdId) {
        this.loadHouseholdMembers();
      }
    }
  }

  /**
   * Load household members for assignment dropdown
   */
  private loadHouseholdMembers(): void {
    if (!this.householdId) return;

    this.householdService.getHouseholdMembers(this.householdId).subscribe({
      next: (members) => {
        this.householdMembers.set(members);
      },
      error: (error) => {
        console.error('Error loading household members:', error);
        // Don't show error to user - assignment is optional
      }
    });
  }

  /**
   * Check if interval has at least one non-zero value
   */
  hasValidInterval(): boolean {
    const formValue = this.createTaskForm.value;
    return (
      formValue.yearsValue > 0 ||
      formValue.monthsValue > 0 ||
      formValue.weeksValue > 0 ||
      formValue.daysValue > 0
    );
  }

  /**
   * Submit form
   */
  onSubmit(): void {
    if (this.createTaskForm.invalid) {
      this.createTaskForm.markAllAsTouched();
      return;
    }

    if (!this.householdId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Błąd',
        detail: 'Brak ID gospodarstwa'
      });
      return;
    }

    // Get current user ID from auth service
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser || !currentUser.id) {
      this.messageService.add({
        severity: 'error',
        summary: 'Błąd',
        detail: 'Musisz być zalogowany aby utworzyć zadanie'
      });
      return;
    }

    this.isLoading.set(true);
    const formValue = this.createTaskForm.value;

    // Extract category ID from TreeNode
    // TreeSelect stores the TreeNode object, we need the data field which contains the category ID
    const categoryId = formValue.categoryId?.data || formValue.categoryId;

    const createDto: CreateTaskDto = {
      householdId: this.householdId,
      categoryId: categoryId,
      name: formValue.name.trim(),
      description: formValue.description?.trim() || undefined,
      priority: formValue.priority,
      assignedTo: formValue.assignedTo || undefined,
      startDate: formValue.startDate ? this.formatDateOnly(formValue.startDate) : undefined,
      yearsValue: formValue.yearsValue || undefined,
      monthsValue: formValue.monthsValue || undefined,
      weeksValue: formValue.weeksValue || undefined,
      daysValue: formValue.daysValue || undefined,
      notes: formValue.notes?.trim() || undefined,
      createdBy: currentUser.id
    };

    this.tasksService.createTask(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sukces',
          detail: 'Zadanie zostało utworzone pomyślnie'
        });
        this.onCreated.emit();
        this.closeDialog();
      },
      error: (error) => {
        const errorMessage = error.error?.error || 'Nie udało się utworzyć zadania';
        this.messageService.add({
          severity: 'error',
          summary: 'Błąd',
          detail: errorMessage
        });
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Format date as YYYY-MM-DD string (local time, without timezone conversion)
   */
  private formatDateOnly(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  /**
   * Close dialog and reset form
   */
  closeDialog(): void {
    this.createTaskForm.reset({
      priority: Priority.MEDIUM,
      assignedTo: null,
      startDate: null,
      yearsValue: 0,
      monthsValue: 0,
      weeksValue: 0,
      daysValue: 0
    });
    this.onClose.emit();
  }
}
