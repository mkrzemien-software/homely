import { Component, inject, signal, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

// PrimeNG Components
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { MessageService } from 'primeng/api';

// Services
import { SystemHouseholdsService } from '../../../../../core/services/system-households.service';

@Component({
  selector: 'app-create-household-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
  ],
  templateUrl: './create-household-dialog.component.html',
  styleUrls: ['./create-household-dialog.component.scss']
})
export class CreateHouseholdDialogComponent {
  @Output() householdCreated = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private systemHouseholdsService = inject(SystemHouseholdsService);
  private messageService = inject(MessageService);

  createHouseholdForm: FormGroup;
  isLoading = signal<boolean>(false);

  constructor() {
    this.createHouseholdForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      address: ['', [Validators.maxLength(255)]]
    });
  }

  onSubmit(): void {
    if (this.createHouseholdForm.invalid) {
      this.createHouseholdForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const { name, address } = this.createHouseholdForm.value;

    this.systemHouseholdsService.createHousehold({ name, address }).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sukces',
          detail: 'Gospodarstwo domowe zostało utworzone pomyślnie'
        });
        this.householdCreated.emit();
        this.closeDialog();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Błąd',
          detail: error.message || 'Nie udało się utworzyć gospodarstwa domowego'
        });
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  closeDialog(): void {
    this.createHouseholdForm.reset();
    this.dialogClosed.emit();
  }
}
