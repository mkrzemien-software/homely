import { Component, inject, signal, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

// PrimeNG Components
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

// Services
import { CategoryService } from '../../services/category.service';

// Models
import { Category, CategoryType, UpdateCategoryDto } from '../../models/category.model';

@Component({
  selector: 'app-edit-category-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    SelectModule,
    InputNumberModule,
    ToastModule,
  ],
  templateUrl: './edit-category-dialog.component.html',
  styleUrls: ['./edit-category-dialog.component.scss']
})
export class EditCategoryDialogComponent implements OnChanges {
  @Input() category: Category | null = null;
  @Input() categoryTypes: CategoryType[] = [];
  @Output() categoryUpdated = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private messageService = inject(MessageService);

  editCategoryForm: FormGroup;
  isLoading = signal<boolean>(false);

  constructor() {
    this.editCategoryForm = this.fb.group({
      categoryTypeId: [null, [Validators.required]],
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      sortOrder: [0, [Validators.min(0)]],
      isActive: [true]
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['category'] && this.category) {
      this.editCategoryForm.patchValue({
        categoryTypeId: this.category.categoryTypeId,
        name: this.category.name,
        description: this.category.description,
        sortOrder: this.category.sortOrder,
        isActive: this.category.isActive
      });
    }
  }

  onSubmit(): void {
    if (this.editCategoryForm.invalid || !this.category) {
      this.editCategoryForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.editCategoryForm.value;

    const updateDto: UpdateCategoryDto = {
      categoryTypeId: formValue.categoryTypeId,
      name: formValue.name,
      description: formValue.description,
      sortOrder: formValue.sortOrder,
      isActive: formValue.isActive
    };

    this.categoryService.updateCategory(this.category.id, updateDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sukces',
          detail: 'Podkategoria została zaktualizowana pomyślnie'
        });
        this.categoryUpdated.emit();
        this.closeDialog();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Błąd',
          detail: error.message || 'Nie udało się zaktualizować podkategorii'
        });
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  closeDialog(): void {
    this.editCategoryForm.reset();
    this.dialogClosed.emit();
  }
}
