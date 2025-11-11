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

// Models
import { Item, ItemsByCategory, getPriorityLabel, getPriorityColor, formatInterval } from './models/item.model';
import {
  ItemFilter,
  ItemSort,
  DEFAULT_ITEM_FILTER,
  DEFAULT_ITEM_SORT,
  SORT_FIELD_LABELS,
  applyItemFilters,
  applyItemSort,
  groupItemsByCategory
} from './models/item-filter.model';

// Services
import { ItemService } from './services/item.service';

/**
 * ItemsListComponent
 *
 * Main component for displaying and managing items (devices and visits) list.
 * Based on PRD section 3.4.2 - Lista urządzeń/wizyt
 *
 * Features:
 * - Display all items grouped by category
 * - Sorting: by date, name, priority
 * - Filtering: by category, assigned person
 * - Quick inline edit
 * - Add new item
 * - Delete item
 * - Freemium limits (5 items max for free plan)
 *
 * Route: /:householdId/categories
 *
 * @example
 * // Route configuration
 * {
 *   path: ':householdId/categories',
 *   loadComponent: () => import('./items-list.component').then(m => m.ItemsListComponent)
 * }
 */
@Component({
  selector: 'app-items-list',
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
    AccordionModule
  ],
  templateUrl: './items-list.component.html',
  styleUrl: './items-list.component.scss'
})
export class ItemsListComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private itemService = inject(ItemService);

  /**
   * Household ID from route
   */
  householdId = signal<string>('');

  /**
   * All items (raw from API)
   */
  allItems = signal<Item[]>([]);

  /**
   * Current filter configuration
   */
  currentFilter = signal<ItemFilter>({ ...DEFAULT_ITEM_FILTER });

  /**
   * Current sort configuration
   */
  currentSort = signal<ItemSort>({ ...DEFAULT_ITEM_SORT });

  /**
   * Available categories for filtering
   */
  availableCategories = signal<{ id: number; name: string }[]>([]);

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
   * Filtered and sorted items
   */
  filteredItems = computed(() => {
    const items = this.allItems();
    const filter = { ...this.currentFilter(), searchText: this.searchText() };
    const sort = this.currentSort();

    // Apply filters
    let filtered = applyItemFilters(items, filter);

    // Apply sort
    filtered = applyItemSort(filtered, sort);

    return filtered;
  });

  /**
   * Items grouped by category
   */
  itemsByCategory = computed<ItemsByCategory[]>(() => {
    const items = this.filteredItems();
    const grouped = groupItemsByCategory(items);
    const categories = this.availableCategories();

    const result: ItemsByCategory[] = [];

    grouped.forEach((categoryItems, categoryId) => {
      const category = categories.find(c => c.id === categoryId);
      if (category) {
        result.push({
          categoryId,
          categoryName: category.name,
          categoryColor: this.getCategoryColor(categoryId),
          categoryIcon: this.getCategoryIcon(categoryId),
          items: categoryItems,
          itemCount: categoryItems.length
        });
      }
    });

    // Sort by category name
    return result.sort((a, b) => a.categoryName.localeCompare(b.categoryName));
  });

  /**
   * Total items count
   */
  totalItemsCount = computed(() => this.allItems().length);

  /**
   * Filtered items count
   */
  filteredItemsCount = computed(() => this.filteredItems().length);

  /**
   * Check if free plan limit reached (5 items)
   */
  isFreePlanLimitReached = computed(() => {
    // TODO: Check actual plan from household data
    const FREE_PLAN_LIMIT = 5;
    return this.totalItemsCount() >= FREE_PLAN_LIMIT;
  });

  /**
   * Sort options for dropdown
   */
  readonly sortOptions = Object.entries(SORT_FIELD_LABELS).map(([value, label]) => ({
    label,
    value
  }));

  /**
   * Helper functions exposed to template
   */
  readonly getPriorityLabel = getPriorityLabel;
  readonly getPriorityColor = getPriorityColor;
  readonly formatInterval = formatInterval;

  ngOnInit(): void {
    // Get household ID from route
    this.route.params.subscribe(params => {
      const id = params['householdId'];
      if (id) {
        this.householdId.set(id);
        this.loadItems(id);
        this.loadCategories();
      }
    });
  }

  /**
   * Load items from API
   */
  private loadItems(householdId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.itemService.getHouseholdItems(householdId).subscribe({
      next: (items) => {
        this.allItems.set(items);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading items:', error);
        this.errorMessage.set('Nie udało się załadować listy urządzeń. Spróbuj ponownie później.');
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Load categories
   */
  private loadCategories(): void {
    this.itemService.getCategories().subscribe({
      next: (categories) => {
        this.availableCategories.set(
          categories.map(c => ({ id: c.id, name: c.name }))
        );
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  /**
   * Handle sort change
   */
  onSortChange(field: string): void {
    const currentSort = this.currentSort();

    if (currentSort.field === field) {
      // Toggle direction
      this.currentSort.set({
        field: field as any,
        direction: currentSort.direction === 'asc' ? 'desc' : 'asc'
      });
    } else {
      // New field, default to asc
      this.currentSort.set({
        field: field as any,
        direction: 'asc'
      });
    }
  }

  /**
   * Handle search
   */
  onSearch(text: string): void {
    this.searchText.set(text);
  }

  /**
   * Toggle view mode
   */
  toggleViewMode(): void {
    this.viewMode.set(this.viewMode() === 'grouped' ? 'list' : 'grouped');
  }

  /**
   * Navigate to add new item
   */
  addNewItem(): void {
    if (this.isFreePlanLimitReached()) {
      // TODO: Show upgrade dialog
      alert('Osiągnięto limit 5 urządzeń w planie darmowym. Przejdź na plan Premium aby dodać więcej.');
      return;
    }

    // TODO: Open add item dialog
    console.log('Add new item');
  }

  /**
   * Edit item
   */
  editItem(item: Item): void {
    // TODO: Open edit dialog
    console.log('Edit item:', item);
  }

  /**
   * Delete item
   */
  deleteItem(item: Item): void {
    if (confirm(`Czy na pewno chcesz usunąć "${item.name}"?`)) {
      this.itemService.deleteItem(item.id).subscribe({
        next: () => {
          // Remove from local state
          const items = this.allItems();
          this.allItems.set(items.filter(i => i.id !== item.id));
        },
        error: (error) => {
          console.error('Error deleting item:', error);
          alert('Nie udało się usunąć urządzenia. Spróbuj ponownie.');
        }
      });
    }
  }

  /**
   * Refresh items list
   */
  refresh(): void {
    const householdId = this.householdId();
    if (householdId) {
      this.loadItems(householdId);
    }
  }

  /**
   * Get category color
   */
  private getCategoryColor(categoryId: number): string {
    // Map category IDs to colors
    // TODO: Get from backend or configuration
    const colorMap: Record<number, string> = {
      1: 'primary',    // Technical inspections
      2: 'success',    // Waste collection
      3: 'danger'      // Medical visits
    };
    return colorMap[categoryId] || 'secondary';
  }

  /**
   * Get category icon
   */
  private getCategoryIcon(categoryId: number): string {
    // Map category IDs to icons
    const iconMap: Record<number, string> = {
      1: 'pi-cog',           // Technical inspections
      2: 'pi-trash',         // Waste collection
      3: 'pi-heart-fill'     // Medical visits
    };
    return iconMap[categoryId] || 'pi-tag';
  }
}
