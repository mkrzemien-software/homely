import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { AccordionModule } from 'primeng/accordion';
import { DialogModule } from 'primeng/dialog';

// Models
import {
  Category,
  CategoryType,
  CategoriesByType,
  getCategoryTypeIcon,
  getCategoryTypeColor
} from './models/category.model';

// Services
import { CategoryService } from './services/category.service';

// Components
import { CreateCategoryDialogComponent } from './components/create-category-dialog/create-category-dialog.component';

/**
 * CategoriesListComponent
 *
 * Main component for displaying and managing categories.
 * Based on API plan - GET /categories and GET /category-types
 *
 * Features:
 * - Display all categories grouped by category type
 * - Sorting: by name, sort order
 * - Filtering: by category type, search text
 * - Quick inline edit
 * - Add new category
 * - Delete category
 *
 * Route: /:householdId/categories
 *
 * @example
 * // Route configuration
 * {
 *   path: ':householdId/categories',
 *   loadComponent: () => import('./categories-list.component').then(m => m.CategoriesListComponent)
 * }
 */
@Component({
  selector: 'app-categories-list',
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    DropdownModule,
    InputTextModule,
    MultiSelectModule,
    DividerModule,
    MessageModule,
    SkeletonModule,
    TagModule,
    TooltipModule,
    AccordionModule,
    DialogModule,
    CreateCategoryDialogComponent
  ],
  templateUrl: './categories-list.component.html',
  styleUrl: './categories-list.component.scss'
})
export class CategoriesListComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private categoryService = inject(CategoryService);

  /**
   * Household ID from route
   */
  householdId = signal<string>('');

  /**
   * All categories (raw from API)
   */
  allCategories = signal<Category[]>([]);

  /**
   * All category types (raw from API)
   */
  allCategoryTypes = signal<CategoryType[]>([]);

  /**
   * Selected category type filter
   */
  selectedCategoryTypeId = signal<number | undefined>(undefined);

  /**
   * Loading state
   */
  isLoading = signal<boolean>(true);

  /**
   * Error message
   */
  errorMessage = signal<string | null>(null);

  /**
   * View mode: 'grouped' or 'list'
   */
  viewMode = signal<'grouped' | 'list'>('grouped');

  /**
   * Search text
   */
  searchText = signal<string>('');

  /**
   * Create category dialog visibility
   */
  createDialogVisible = signal<boolean>(false);

  /**
   * Filtered categories
   */
  filteredCategories = computed(() => {
    const categories = this.allCategories();
    const searchText = this.searchText().toLowerCase();
    const categoryTypeId = this.selectedCategoryTypeId();

    let filtered = categories;

    // Filter by category type
    if (categoryTypeId !== undefined) {
      filtered = filtered.filter(c => c.categoryTypeId === categoryTypeId);
    }

    // Filter by search text
    if (searchText) {
      filtered = filtered.filter(c =>
        c.name.toLowerCase().includes(searchText) ||
        c.description?.toLowerCase().includes(searchText)
      );
    }

    // Sort by sortOrder, then by name
    return filtered.sort((a, b) => {
      if (a.sortOrder !== b.sortOrder) {
        return a.sortOrder - b.sortOrder;
      }
      return a.name.localeCompare(b.name);
    });
  });

  /**
   * Categories grouped by type
   */
  categoriesByType = computed<CategoriesByType[]>(() => {
    const categories = this.filteredCategories();
    const categoryTypes = this.allCategoryTypes();

    // Group categories by type
    const grouped = new Map<number, Category[]>();
    categories.forEach(category => {
      if (!grouped.has(category.categoryTypeId)) {
        grouped.set(category.categoryTypeId, []);
      }
      grouped.get(category.categoryTypeId)!.push(category);
    });

    // Build result array
    const result: CategoriesByType[] = [];
    grouped.forEach((typeCategories, categoryTypeId) => {
      const categoryType = categoryTypes.find(ct => ct.id === categoryTypeId);
      if (categoryType) {
        result.push({
          categoryTypeId,
          categoryTypeName: categoryType.name,
          categoryTypeDescription: categoryType.description,
          categories: typeCategories,
          categoryCount: typeCategories.length
        });
      }
    });

    // Sort by sortOrder
    return result.sort((a, b) => {
      const typeA = categoryTypes.find(ct => ct.id === a.categoryTypeId);
      const typeB = categoryTypes.find(ct => ct.id === b.categoryTypeId);
      if (typeA && typeB) {
        return typeA.sortOrder - typeB.sortOrder;
      }
      return 0;
    });
  });

  /**
   * Total categories count
   */
  totalCategoriesCount = computed(() => this.allCategories().length);

  /**
   * Filtered categories count
   */
  filteredCategoriesCount = computed(() => this.filteredCategories().length);

  /**
   * Category type options for dropdown
   */
  categoryTypeOptions = computed(() => {
    const types = this.allCategoryTypes();
    return [
      { label: 'Wszystkie typy', value: undefined },
      ...types.map(type => ({ label: type.name, value: type.id }))
    ];
  });

  /**
   * Helper functions exposed to template
   */
  readonly getCategoryTypeIcon = getCategoryTypeIcon;
  readonly getCategoryTypeColor = getCategoryTypeColor;

  ngOnInit(): void {
    // Get household ID from route
    this.route.params.subscribe(params => {
      const id = params['householdId'];
      if (id) {
        this.householdId.set(id);
        this.loadCategories();
        this.loadCategoryTypes();
      }
    });
  }

  /**
   * Load categories from API
   */
  private loadCategories(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.categoryService.getCategories().subscribe({
      next: (categories) => {
        this.allCategories.set(categories);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.errorMessage.set('Nie udało się załadować listy kategorii. Spróbuj ponownie później.');
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Load category types from API
   */
  private loadCategoryTypes(): void {
    this.categoryService.getCategoryTypes().subscribe({
      next: (categoryTypes) => {
        this.allCategoryTypes.set(categoryTypes);
      },
      error: (error) => {
        console.error('Error loading category types:', error);
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
   * Handle category type filter change
   */
  onCategoryTypeFilterChange(categoryTypeId: number | undefined): void {
    this.selectedCategoryTypeId.set(categoryTypeId);
  }

  /**
   * Toggle view mode
   */
  toggleViewMode(): void {
    this.viewMode.set(this.viewMode() === 'grouped' ? 'list' : 'grouped');
  }

  /**
   * Open add new category dialog
   */
  addNewCategory(): void {
    this.createDialogVisible.set(true);
  }

  /**
   * Hide create category dialog
   */
  hideCreateDialog(): void {
    this.createDialogVisible.set(false);
  }

  /**
   * Handle category created event
   */
  onCategoryCreated(): void {
    this.hideCreateDialog();
    this.refresh();
  }

  /**
   * Edit category
   */
  editCategory(category: Category): void {
    // TODO: Open edit dialog
    console.log('Edit category:', category);
  }

  /**
   * Delete category
   */
  deleteCategory(category: Category): void {
    if (confirm(`Czy na pewno chcesz usunąć kategorię "${category.name}"?`)) {
      this.categoryService.deleteCategory(category.id).subscribe({
        next: () => {
          // Remove from local state
          const categories = this.allCategories();
          this.allCategories.set(categories.filter(c => c.id !== category.id));
        },
        error: (error) => {
          console.error('Error deleting category:', error);
          alert('Nie udało się usunąć kategorii. Spróbuj ponownie.');
        }
      });
    }
  }

  /**
   * Refresh categories list
   */
  refresh(): void {
    this.loadCategories();
    this.loadCategoryTypes();
  }
}
