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

// Services
import { TasksService } from '../../services/tasks.service';

// Models
import { Category } from '../../../items/models/category.model';
import { Task, Priority, UpdateTaskDto, getPriorityLabel } from '../../models/task.model';

/**
 * EditTaskDialogComponent
 *
 * Dialog for editing existing task templates.
 * Features:
 * - Pre-populated form with task data
 * - Category selection (subcategory dropdown)
 * - Task name and description
 * - Priority selection
 * - Interval configuration (years, months, weeks, days)
 * - Notes field
 * - Form validation
 * - Toast notifications
 */
@Component({
  selector: 'app-edit-task-dialog',
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
    DividerModule
  ],
  providers: [MessageService],
  templateUrl: './edit-task-dialog.component.html',
  styleUrl: './edit-task-dialog.component.scss'
})
export class EditTaskDialogComponent implements OnInit, OnChanges {
  @Input() visible = false;
  @Input() task: Task | null = null;
  categories = input<Category[]>([]);

  @Output() onClose = new EventEmitter<void>();
  @Output() onUpdated = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private tasksService = inject(TasksService);
  private messageService = inject(MessageService);

  editTaskForm: FormGroup;
  isLoading = signal<boolean>(false);

  /**
   * Priority options for dropdown
   */
  priorityOptions = [
    { label: getPriorityLabel(Priority.LOW), value: Priority.LOW },
    { label: getPriorityLabel(Priority.MEDIUM), value: Priority.MEDIUM },
    { label: getPriorityLabel(Priority.HIGH), value: Priority.HIGH }
  ];

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
    }, {} as Record<number, { typeId: number; typeName: string; categories: Category[] }>);

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
    this.editTaskForm = this.fb.group({
      categoryId: [null, [Validators.required]],
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: [''],
      priority: [Priority.MEDIUM, [Validators.required]],
      yearsValue: [0, [Validators.min(0)]],
      monthsValue: [0, [Validators.min(0)]],
      weeksValue: [0, [Validators.min(0)]],
      daysValue: [0, [Validators.min(0)]],
      notes: ['']
    });
  }

  ngOnInit(): void {
    if (this.task) {
      this.populateForm();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['task'] && this.task) {
      this.populateForm();
    }
  }

  /**
   * Populate form with task data
   */
  private populateForm(): void {
    if (!this.task) return;

    // Find the TreeNode for the selected category
    const categoryId = this.task.category.id;
    let selectedNode: TreeNode | null = null;

    // Search through tree nodes to find the matching category
    for (const parentNode of this.categoryTreeNodes()) {
      if (parentNode.children) {
        const found = parentNode.children.find(child => child.data === categoryId);
        if (found) {
          selectedNode = found;
          break;
        }
      }
    }

    this.editTaskForm.patchValue({
      categoryId: selectedNode,
      name: this.task.name,
      description: this.task.description || '',
      priority: this.task.priority,
      yearsValue: this.task.interval?.years || 0,
      monthsValue: this.task.interval?.months || 0,
      weeksValue: this.task.interval?.weeks || 0,
      daysValue: this.task.interval?.days || 0,
      notes: this.task.notes || ''
    });
  }

  /**
   * Submit form
   */
  onSubmit(): void {
    if (this.editTaskForm.invalid || !this.task) {
      this.editTaskForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.editTaskForm.value;

    // Extract category ID from TreeNode
    // TreeSelect stores the TreeNode object, we need the data field which contains the category ID
    const categoryId = formValue.categoryId?.data || formValue.categoryId;

    const updateDto: UpdateTaskDto = {
      categoryId: categoryId,
      name: formValue.name.trim(),
      description: formValue.description?.trim() || undefined,
      priority: formValue.priority,
      yearsValue: formValue.yearsValue || undefined,
      monthsValue: formValue.monthsValue || undefined,
      weeksValue: formValue.weeksValue || undefined,
      daysValue: formValue.daysValue || undefined,
      notes: formValue.notes?.trim() || undefined
    };

    this.tasksService.updateTask(this.task.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sukces',
          detail: 'Zadanie zostało zaktualizowane pomyślnie'
        });
        this.onUpdated.emit();
        this.closeDialog();
      },
      error: (error) => {
        const errorMessage = error.error?.error || 'Nie udało się zaktualizować zadania';
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
   * Close dialog
   */
  closeDialog(): void {
    this.editTaskForm.reset({
      priority: Priority.MEDIUM,
      yearsValue: 0,
      monthsValue: 0,
      weeksValue: 0,
      daysValue: 0
    });
    this.onClose.emit();
  }
}
