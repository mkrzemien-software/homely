import { Component, inject, signal, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
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
import { CategoryType, UpdateCategoryTypeDto } from '../../models/category.model';

@Component({
  selector: 'app-edit-category-type-dialog',
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
  templateUrl: './edit-category-type-dialog.component.html',
  styleUrls: ['./edit-category-type-dialog.component.scss']
})
export class EditCategoryTypeDialogComponent implements OnInit, OnChanges {
  @Input({ required: true }) householdId!: string;
  @Input() categoryType: CategoryType | null = null;
  @Output() categoryTypeUpdated = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private messageService = inject(MessageService);

  editCategoryTypeForm: FormGroup;
  isLoading = signal<boolean>(false);

  constructor() {
    this.editCategoryTypeForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      sortOrder: [0, [Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    // Initialization logic if needed
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['categoryType'] && this.categoryType) {
      this.editCategoryTypeForm.patchValue({
        name: this.categoryType.name,
        description: this.categoryType.description,
        sortOrder: this.categoryType.sortOrder
      });
    }
  }

  onSubmit(): void {
    if (this.editCategoryTypeForm.invalid || !this.categoryType) {
      this.editCategoryTypeForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.editCategoryTypeForm.value;

    const updateDto: UpdateCategoryTypeDto = {
      name: formValue.name,
      description: formValue.description,
      sortOrder: formValue.sortOrder
    };

    this.categoryService.updateCategoryType(this.householdId, this.categoryType.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sukces',
          detail: 'Kategoria została zaktualizowana pomyślnie'
        });
        this.categoryTypeUpdated.emit();
        this.closeDialog();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Błąd',
          detail: error.message || 'Nie udało się zaktualizować kategorii'
        });
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  closeDialog(): void {
    this.editCategoryTypeForm.reset();
    this.dialogClosed.emit();
  }
}
