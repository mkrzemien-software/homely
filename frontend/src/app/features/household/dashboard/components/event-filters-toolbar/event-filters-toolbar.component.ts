import { Component, signal, output, input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// PrimeNG Components
import { ToolbarModule } from 'primeng/toolbar';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { BadgeModule } from 'primeng/badge';

// Custom Components
import { DateRangeSelectorComponent } from '../date-range-selector/date-range-selector.component';

/**
 * Event filters interface
 */
export interface EventFilters {
  /**
   * Filter by assigned user ID
   */
  assignedUserId?: string | null;

  /**
   * Filter by category ID (UUID)
   */
  categoryId?: string | null;

  /**
   * Filter by priority
   */
  priority?: 'high' | 'medium' | 'low' | null;

  /**
   * Filter by status
   */
  status?: 'pending' | 'completed' | 'postponed' | 'cancelled' | null;
}

/**
 * Filter option interface
 */
export interface FilterOption {
  label: string;
  value: any;
}

/**
 * EventFiltersToolbarComponent
 *
 * Toolbar component with filters for events list.
 * Allows filtering by assigned user, category, priority, and status.
 *
 * Based on UI Plan (ui-plan.md):
 * - Toolbar nad listą wydarzeń
 * - Filtry: osoba odpowiedzialna, kategoria, priorytet, status
 * - Przycisk reset filtrów
 * - Badge pokazujący liczbę aktywnych filtrów
 *
 * Features:
 * - PrimeNG Toolbar with filter dropdowns
 * - Active filters count badge
 * - Reset filters button
 * - Responsive design (filters collapse on mobile)
 * - Accessible (keyboard navigation, ARIA labels)
 *
 * @example
 * <app-event-filters-toolbar
 *   [users]="householdMembers()"
 *   [categories]="categories()"
 *   (filtersChange)="onFiltersChange($event)">
 * </app-event-filters-toolbar>
 */
@Component({
  selector: 'app-event-filters-toolbar',
  imports: [
    CommonModule,
    FormsModule,
    ToolbarModule,
    DropdownModule,
    ButtonModule,
    BadgeModule,
    DateRangeSelectorComponent
  ],
  templateUrl: './event-filters-toolbar.component.html',
  styleUrl: './event-filters-toolbar.component.scss'
})
export class EventFiltersToolbarComponent implements OnInit {
  /**
   * Available users for filtering (input)
   */
  users = input<FilterOption[]>([]);

  /**
   * Available categories for filtering (input)
   */
  categories = input<FilterOption[]>([]);

  /**
   * Selected date range (days)
   */
  selectedDays = input<7 | 14 | 30>(7);

  /**
   * Output event when filters change
   */
  filtersChange = output<EventFilters>();

  /**
   * Output event when date range changes
   */
  rangeChange = output<7 | 14 | 30>();

  /**
   * Selected assigned user
   */
  selectedUser = signal<string | null>(null);

  /**
   * Selected category (UUID)
   */
  selectedCategory = signal<string | null>(null);

  /**
   * Selected priority
   */
  selectedPriority = signal<'high' | 'medium' | 'low' | null>(null);

  /**
   * Selected status
   */
  selectedStatus = signal<'pending' | 'completed' | 'postponed' | 'cancelled' | null>(null);

  /**
   * Priority options
   */
  readonly priorityOptions: FilterOption[] = [
    { label: 'Wysoki', value: 'high' },
    { label: 'Średni', value: 'medium' },
    { label: 'Niski', value: 'low' }
  ];

  /**
   * Status options
   */
  readonly statusOptions: FilterOption[] = [
    { label: 'Oczekujące', value: 'pending' },
    { label: 'Wykonane', value: 'completed' },
    { label: 'Przełożone', value: 'postponed' },
    { label: 'Anulowane', value: 'cancelled' }
  ];

  /**
   * Active filters count
   */
  activeFiltersCount = signal<number>(0);

  ngOnInit(): void {
    this.updateActiveFiltersCount();
  }

  /**
   * Handle user filter change
   */
  onUserChange(event: any): void {
    this.selectedUser.set(event.value);
    this.emitFiltersChange();
  }

  /**
   * Handle category filter change
   */
  onCategoryChange(event: any): void {
    this.selectedCategory.set(event.value);
    this.emitFiltersChange();
  }

  /**
   * Handle priority filter change
   */
  onPriorityChange(event: any): void {
    this.selectedPriority.set(event.value);
    this.emitFiltersChange();
  }

  /**
   * Handle status filter change
   */
  onStatusChange(event: any): void {
    this.selectedStatus.set(event.value);
    this.emitFiltersChange();
  }

  /**
   * Handle date range change
   */
  onDateRangeChange(days: 7 | 14 | 30): void {
    this.rangeChange.emit(days);
  }

  /**
   * Reset all filters
   */
  resetFilters(): void {
    this.selectedUser.set(null);
    this.selectedCategory.set(null);
    this.selectedPriority.set(null);
    this.selectedStatus.set(null);
    this.emitFiltersChange();
  }

  /**
   * Emit filters change event
   */
  private emitFiltersChange(): void {
    const filters: EventFilters = {
      assignedUserId: this.selectedUser(),
      categoryId: this.selectedCategory(),
      priority: this.selectedPriority(),
      status: this.selectedStatus()
    };

    this.filtersChange.emit(filters);
    this.updateActiveFiltersCount();
  }

  /**
   * Update active filters count
   */
  private updateActiveFiltersCount(): void {
    let count = 0;

    if (this.selectedUser()) count++;
    if (this.selectedCategory()) count++;
    if (this.selectedPriority()) count++;
    if (this.selectedStatus()) count++;

    this.activeFiltersCount.set(count);
  }

  /**
   * Check if any filters are active
   */
  hasActiveFilters(): boolean {
    return this.activeFiltersCount() > 0;
  }
}
