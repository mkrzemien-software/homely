import { Component, inject, signal, Input, Output, EventEmitter, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

// PrimeNG Components
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { InputTextarea } from 'primeng/inputtextarea';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';

// Models
import { Task, Priority, getPriorityLabel, getPrioritySeverity, formatInterval } from '../../models/task.model';

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
    DropdownModule,
    CalendarModule,
    InputTextarea,
    ToastModule,
    DividerModule,
    TagModule
  ],
  providers: [MessageService],
  templateUrl: './create-event-from-task-dialog.component.html',
  styleUrl: './create-event-from-task-dialog.component.scss'
})
export class CreateEventFromTaskDialogComponent implements OnInit {
  @Input() visible = false;
  @Input() task: Task | null = null;
  @Input() householdId: string | null = null;

  @Output() onClose = new EventEmitter<void>();
  @Output() onCreated = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);
  // TODO: Inject EventsService when implemented
  // private eventsService = inject(EventsService);

  createEventForm: FormGroup;
  isLoading = signal<boolean>(false);

  // TODO: Load household members from API
  householdMembers = signal<any[]>([]);

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
    // TODO: Replace with actual household members
    return this.householdMembers().map(member => ({
      label: `${member.firstName} ${member.lastName}`,
      value: member.id
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
    this.createEventForm = this.fb.group({
      assignedTo: [null, [Validators.required]],
      dueDate: [null, [Validators.required]],
      priority: [Priority.MEDIUM, [Validators.required]],
      notes: ['']
    });
  }

  ngOnInit(): void {
    // Initialize form with task priority
    if (this.task) {
      this.createEventForm.patchValue({
        priority: this.task.priority
      });
    }

    // TODO: Load household members
    // this.loadHouseholdMembers();
  }

  /**
   * TODO: Load household members from API
   */
  private loadHouseholdMembers(): void {
    // Placeholder for API call
    // this.householdMembersService.getMembers(this.householdId).subscribe({
    //   next: (members) => {
    //     this.householdMembers.set(members);
    //   },
    //   error: (error) => {
    //     console.error('Error loading household members:', error);
    //   }
    // });
  }

  /**
   * Submit form
   */
  onSubmit(): void {
    if (this.createEventForm.invalid || !this.task) {
      this.createEventForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.createEventForm.value;

    // TODO: Replace with actual API call to EventsService
    /*
    const createEventDto = {
      taskId: this.task.id,
      assignedTo: formValue.assignedTo,
      dueDate: formValue.dueDate,
      priority: formValue.priority,
      notes: formValue.notes?.trim() || undefined
    };

    this.eventsService.createEvent(createEventDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sukces',
          detail: 'Wydarzenie zostało utworzone pomyślnie'
        });
        this.onCreated.emit();
        this.closeDialog();
      },
      error: (error) => {
        const errorMessage = error.error?.error || 'Nie udało się utworzyć wydarzenia';
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
    */

    // Placeholder implementation
    setTimeout(() => {
      this.messageService.add({
        severity: 'info',
        summary: 'TODO',
        detail: 'Tworzenie wydarzeń zostanie zaimplementowane po utworzeniu Events API'
      });
      this.isLoading.set(false);
      this.onCreated.emit();
      this.closeDialog();
    }, 1000);
  }

  /**
   * Close dialog and reset form
   */
  closeDialog(): void {
    this.createEventForm.reset({
      priority: this.task?.priority || Priority.MEDIUM
    });
    this.onClose.emit();
  }
}
