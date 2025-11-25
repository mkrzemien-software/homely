import { Component, inject, signal, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

// PrimeNG Components
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

// Services
import { CategoryService } from '../../services/category.service';

// Models
import { CreateCategoryTypeDto } from '../../models/category.model';

@Component({
  selector: 'app-create-category-type-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    InputNumberModule,
    ToastModule,
  ],
  providers: [MessageService],
  templateUrl: './create-category-type-dialog.component.html',
  styleUrls: ['./create-category-type-dialog.component.scss']
})
export class CreateCategoryTypeDialogComponent implements OnInit {
  @Output() categoryTypeCreated = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private messageService = inject(MessageService);

  createCategoryTypeForm: FormGroup;
  isLoading = signal<boolean>(false);

  constructor() {
    this.createCategoryTypeForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      sortOrder: [0, [Validators.min(0)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    // Initialization logic if needed
  }

  onSubmit(): void {
    if (this.createCategoryTypeForm.invalid) {
      this.createCategoryTypeForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.createCategoryTypeForm.value;

    const createDto: CreateCategoryTypeDto = {
      name: formValue.name,
      description: formValue.description,
      sortOrder: formValue.sortOrder,
      isActive: formValue.isActive
    };

    this.categoryService.createCategoryType(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sukces',
          detail: 'Nowa kategoria została utworzona pomyślnie'
        });
        this.categoryTypeCreated.emit();
        this.closeDialog();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Błąd',
          detail: error.message || 'Nie udało się utworzyć kategorii'
        });
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  closeDialog(): void {
    this.createCategoryTypeForm.reset({
      name: '',
      description: '',
      sortOrder: 0,
      isActive: true
    });
    this.dialogClosed.emit();
  }
}
