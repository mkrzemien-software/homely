import { Component, inject, signal, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

// PrimeNG Components
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageService } from 'primeng/api';

// Services
import { CategoryService } from '../../services/category.service';

// Models
import { CategoryType, CreateCategoryDto } from '../../models/category.model';

@Component({
  selector: 'app-create-category-dialog',
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
  ],
  templateUrl: './create-category-dialog.component.html',
  styleUrls: ['./create-category-dialog.component.scss']
})
export class CreateCategoryDialogComponent implements OnInit {
  @Output() categoryCreated = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private messageService = inject(MessageService);

  createCategoryForm: FormGroup;
  isLoading = signal<boolean>(false);
  categoryTypes = signal<CategoryType[]>([]);

  constructor() {
    this.createCategoryForm = this.fb.group({
      categoryTypeId: [null, [Validators.required]],
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      sortOrder: [0, [Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadCategoryTypes();
  }

  private loadCategoryTypes(): void {
    this.categoryService.getCategoryTypes().subscribe({
      next: (types) => {
        this.categoryTypes.set(types);
      },
      error: (error) => {
        console.error('Error loading category types:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Błąd',
          detail: 'Nie udało się załadować kategorii'
        });
      }
    });
  }

  onSubmit(): void {
    if (this.createCategoryForm.invalid) {
      this.createCategoryForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.createCategoryForm.value;

    const createDto: CreateCategoryDto = {
      categoryTypeId: formValue.categoryTypeId,
      name: formValue.name,
      description: formValue.description,
      sortOrder: formValue.sortOrder
    };

    this.categoryService.createCategory(createDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sukces',
          detail: 'Podkategoria została utworzona pomyślnie'
        });
        this.categoryCreated.emit();
        this.closeDialog();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Błąd',
          detail: error.message || 'Nie udało się utworzyć podkategorii'
        });
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  closeDialog(): void {
    this.createCategoryForm.reset();
    this.dialogClosed.emit();
  }
}
