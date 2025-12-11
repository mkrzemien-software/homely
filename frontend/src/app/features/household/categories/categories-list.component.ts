import { Component, inject, signal, computed, OnInit, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { AccordionModule } from 'primeng/accordion';
import { DialogModule } from 'primeng/dialog';

// Angular CDK
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

// Models
import {
  Category,
  CategoryType,
  CategoriesByType,
  getCategoryTypeColor
} from './models/category.model';

// Services
import { CategoryService } from './services/category.service';

// Components
import { CreateCategoryDialogComponent } from './components/create-category-dialog/create-category-dialog.component';
import { EditCategoryDialogComponent } from './components/edit-category-dialog/edit-category-dialog.component';
import { CreateCategoryTypeDialogComponent } from './components/create-category-type-dialog/create-category-type-dialog.component';
import { EditCategoryTypeDialogComponent } from './components/edit-category-type-dialog/edit-category-type-dialog.component';

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
    SelectModule,
    InputTextModule,
    MultiSelectModule,
    DividerModule,
    MessageModule,
    SkeletonModule,
    TagModule,
    TooltipModule,
    AccordionModule,
    DialogModule,
    DragDropModule,
    CreateCategoryDialogComponent,
    EditCategoryDialogComponent,
    CreateCategoryTypeDialogComponent,
    EditCategoryTypeDialogComponent
  ],
  templateUrl: './categories-list.component.html',
  styleUrl: './categories-list.component.scss'
})
export class CategoriesListComponent implements OnInit, AfterViewInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private categoryService = inject(CategoryService);
  private cdr = inject(ChangeDetectorRef);

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
  selectedCategoryTypeId = signal<string | undefined>(undefined);

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
   * Preselected category type ID for create dialog
   */
  preselectedCategoryTypeId = signal<string | null>(null);

  /**
   * Create category type dialog visibility
   */
  createCategoryTypeDialogVisible = signal<boolean>(false);

  /**
   * Edit category dialog visibility
   */
  editDialogVisible = signal<boolean>(false);

  /**
   * Category to edit
   */
  categoryToEdit = signal<Category | null>(null);

  /**
   * Edit category type dialog visibility
   */
  editCategoryTypeDialogVisible = signal<boolean>(false);

  /**
   * Category type to edit
   */
  categoryTypeToEdit = signal<CategoryType | null>(null);

  /**
   * Accordion active indexes - writable signal to avoid change detection issues
   */
  accordionActiveIndexes = signal<number[]>([]);

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
    const grouped = new Map<string, Category[]>();
    categories.forEach(category => {
      if (!grouped.has(category.categoryTypeId)) {
        grouped.set(category.categoryTypeId, []);
      }
      grouped.get(category.categoryTypeId)!.push(category);
    });

    // Build result array - INCLUDE ALL CATEGORY TYPES (even those without categories)
    const result: CategoriesByType[] = categoryTypes.map(categoryType => {
      const typeCategories = grouped.get(categoryType.id) || [];
      return {
        categoryTypeId: categoryType.id,
        categoryTypeName: categoryType.name,
        categoryTypeDescription: categoryType.description,
        categories: typeCategories,
        categoryCount: typeCategories.length
      };
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
      { label: 'Wszystkie kategorie', value: undefined },
      ...types.map(type => ({ label: type.name, value: type.id }))
    ];
  });

  /**
   * Helper functions exposed to template
   */
  readonly getCategoryTypeColor = getCategoryTypeColor;

  ngOnInit(): void {
    // Get household ID from route
    this.route.params.subscribe(params => {
      const id = params['householdId'];
      if (id) {
        this.householdId.set(id);
        // Clear cache when household changes to avoid showing data from previous household
        this.categoryService.clearAllCaches();
        this.loadCategories();
        this.loadCategoryTypes();
      }
    });
  }

  ngAfterViewInit(): void {
    // Accordion indexes will be set after data is loaded
  }

  /**
   * Load categories from API
   */
  private loadCategories(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const householdId = this.householdId();
    if (!householdId) {
      this.errorMessage.set('Brak ID gospodarstwa domowego');
      this.isLoading.set(false);
      return;
    }

    this.categoryService.getCategories(householdId).subscribe({
      next: (categories) => {
        this.allCategories.set(categories);
        this.isLoading.set(false);
        // Update accordion after data is loaded
        this.updateAccordionActiveIndexes();
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.errorMessage.set('Nie udało się załadować listy podkategorii. Spróbuj ponownie później.');
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Load category types from API
   */
  private loadCategoryTypes(): void {
    const householdId = this.householdId();
    if (!householdId) {
      return;
    }

    this.categoryService.getCategoryTypes(householdId).subscribe({
      next: (categoryTypes) => {
        this.allCategoryTypes.set(categoryTypes);
        // Update accordion after data is loaded
        this.updateAccordionActiveIndexes();
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
  onCategoryTypeFilterChange(categoryTypeId: string | undefined): void {
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
   * @param categoryTypeId Optional category type ID to preselect
   */
  addNewCategory(categoryTypeId?: string): void {
    this.preselectedCategoryTypeId.set(categoryTypeId ?? null);
    this.createDialogVisible.set(true);
  }

  /**
   * Hide create category dialog
   */
  hideCreateDialog(): void {
    this.createDialogVisible.set(false);
    this.preselectedCategoryTypeId.set(null);
  }

  /**
   * Handle category created event
   */
  onCategoryCreated(): void {
    this.hideCreateDialog();
    this.refresh();
  }

  /**
   * Open add new category type dialog
   */
  addNewCategoryType(): void {
    this.createCategoryTypeDialogVisible.set(true);
  }

  /**
   * Hide create category type dialog
   */
  hideCreateCategoryTypeDialog(): void {
    this.createCategoryTypeDialogVisible.set(false);
  }

  /**
   * Handle category type created event
   */
  onCategoryTypeCreated(): void {
    this.hideCreateCategoryTypeDialog();
    this.refresh();
  }

  /**
   * Edit category
   */
  editCategory(category: Category): void {
    this.categoryToEdit.set(category);
    this.editDialogVisible.set(true);
  }

  /**
   * Hide edit category dialog
   */
  hideEditDialog(): void {
    this.editDialogVisible.set(false);
    this.categoryToEdit.set(null);
  }

  /**
   * Handle category updated event
   */
  onCategoryUpdated(): void {
    this.hideEditDialog();
    this.refresh();
  }

  /**
   * Delete category
   */
  deleteCategory(category: Category): void {
    const householdId = this.householdId();
    if (!householdId) {
      return;
    }

    if (confirm(`Czy na pewno chcesz usunąć podkategorię "${category.name}"?`)) {
      this.categoryService.deleteCategory(householdId, category.id).subscribe({
        next: () => {
          // Remove from local state
          const categories = this.allCategories();
          this.allCategories.set(categories.filter(c => c.id !== category.id));
        },
        error: (error) => {
          console.error('Error deleting category:', error);
          alert('Nie udało się usunąć podkategorii. Spróbuj ponownie.');
        }
      });
    }
  }

  /**
   * Edit category type
   */
  editCategoryType(categoryTypeId: string): void {
    const categoryType = this.allCategoryTypes().find(ct => ct.id === categoryTypeId);
    if (categoryType) {
      this.categoryTypeToEdit.set(categoryType);
      this.editCategoryTypeDialogVisible.set(true);
    }
  }

  /**
   * Hide edit category type dialog
   */
  hideEditCategoryTypeDialog(): void {
    this.editCategoryTypeDialogVisible.set(false);
    this.categoryTypeToEdit.set(null);
  }

  /**
   * Handle category type updated event
   */
  onCategoryTypeUpdated(): void {
    this.hideEditCategoryTypeDialog();
    this.refresh();
  }

  /**
   * Refresh categories list
   */
  refresh(): void {
    this.loadCategories();
    this.loadCategoryTypes();
    // Update accordion state after refresh
    setTimeout(() => this.updateAccordionActiveIndexes());
  }

  /**
   * Update accordion active indexes to expand all panels
   */
  private updateAccordionActiveIndexes(): void {
    // Use setTimeout to ensure this runs after change detection
    setTimeout(() => {
      const typeCount = this.categoriesByType().length;
      const indexes = Array.from({ length: typeCount }, (_, i) => i);
      this.accordionActiveIndexes.set(indexes);
      // Run change detection in the next tick to avoid ExpressionChangedAfterItHasBeenCheckedError
      setTimeout(() => {
        this.cdr.detectChanges();
      }, 0);
    }, 0);
  }

  /**
   * Handle drop event for drag & drop reordering
   * Only works in grouped view - subcategories can only be reordered within the same category
   */
  onCategoryDrop(event: CdkDragDrop<Category[]>, categoryTypeId: string): void {
    // Get the categories for this category type
    const categoryGroup = this.categoriesByType().find(
      group => group.categoryTypeId === categoryTypeId
    );

    if (!categoryGroup) {
      return;
    }

    // Create a copy of the categories array
    const categories = [...categoryGroup.categories];

    // Move the item in the array
    moveItemInArray(categories, event.previousIndex, event.currentIndex);

    // Recalculate sortOrder for all categories in this group
    const updates = categories.map((category, index) => ({
      id: category.id,
      sortOrder: index
    }));

    // Optimistically update local state
    const allCategories = this.allCategories();
    const updatedCategories = allCategories.map(cat => {
      const update = updates.find(u => u.id === cat.id);
      if (update) {
        return { ...cat, sortOrder: update.sortOrder };
      }
      return cat;
    });
    this.allCategories.set(updatedCategories);

    // Call API to persist the changes
    const householdId = this.householdId();
    if (!householdId) {
      // Rollback on error
      this.loadCategories();
      return;
    }

    this.categoryService.updateCategoriesOrder(householdId, updates).subscribe({
      next: () => {
        // Success - state already updated optimistically
      },
      error: (error) => {
        console.error('Error updating category order:', error);
        // Rollback on error
        this.loadCategories();
        alert('Nie udało się zaktualizować kolejności podkategorii. Spróbuj ponownie.');
      }
    });
  }
}
