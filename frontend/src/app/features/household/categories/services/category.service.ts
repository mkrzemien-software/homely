import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, catchError, of, map } from 'rxjs';
import {
  Category,
  CategoryType,
  CategoriesResponse,
  CategoryTypesResponse,
  CreateCategoryDto,
  UpdateCategoryDto,
  CreateCategoryTypeDto,
  UpdateCategoryTypeDto,
  CategorySortOrderItem,
  UpdateCategoriesSortOrderDto
} from '../models/category.model';
import { environment } from '../../../../../environments/environment';

/**
 * CategoryService
 *
 * Service for managing categories and category types.
 * Communicates with the backend API.
 *
 * Based on API plan:
 * - GET /category-types
 * - GET /categories
 *
 * @example
 * const categoryService = inject(CategoryService);
 * categoryService.getCategories().subscribe(categories => {
 *   console.log('Categories:', categories);
 * });
 */
@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private http = inject(HttpClient);

  /**
   * Base API URL
   */
  private readonly API_URL = environment.apiUrl;

  /**
   * Cache for category types
   */
  private categoryTypesCache$ = new BehaviorSubject<CategoryType[]>([]);

  /**
   * Cache for categories
   */
  private categoriesCache$ = new BehaviorSubject<Category[]>([]);

  /**
   * Get all category types for a household
   *
   * @param householdId - The household ID
   * @returns Observable of category types array
   */
  getCategoryTypes(householdId: string): Observable<CategoryType[]> {
    // Return cached if available
    if (this.categoryTypesCache$.value.length > 0) {
      return this.categoryTypesCache$.asObservable();
    }

    const params = new HttpParams().append('householdId', householdId);

    return this.http.get<CategoryTypesResponse>(`${this.API_URL}/category-types`, { params })
      .pipe(
        map(response => response.data),
        tap(data => {
          this.categoryTypesCache$.next(data);
        }),
        catchError(error => {
          console.error('Error fetching category types:', error);
          return of([]);
        })
      );
  }

  /**
   * Get cached category types
   *
   * @param householdId - The household ID
   */
  getCachedCategoryTypes(householdId: string): Observable<CategoryType[]> {
    if (this.categoryTypesCache$.value.length === 0) {
      this.getCategoryTypes(householdId).subscribe();
    }
    return this.categoryTypesCache$.asObservable();
  }

  /**
   * Get all categories for a household
   *
   * @param householdId - The household ID
   * @param categoryTypeId - Optional filter by category type (UUID)
   * @returns Observable of categories array
   */
  getCategories(householdId: string, categoryTypeId?: string): Observable<Category[]> {
    // Build query params
    let params = new HttpParams().append('householdId', householdId);
    if (categoryTypeId !== undefined) {
      params = params.append('categoryTypeId', categoryTypeId.toString());
    }

    return this.http.get<CategoriesResponse>(`${this.API_URL}/categories`, { params })
      .pipe(
        map(response => response.data),
        tap(data => {
          // Update cache only if no filter was applied
          if (categoryTypeId === undefined) {
            this.categoriesCache$.next(data);
          }
        }),
        catchError(error => {
          console.error('Error fetching categories:', error);
          // Return cached data on error if no filter
          if (categoryTypeId === undefined) {
            return of(this.categoriesCache$.value);
          }
          return of([]);
        })
      );
  }

  /**
   * Get cached categories
   *
   * @param householdId - The household ID
   */
  getCachedCategories(householdId: string): Observable<Category[]> {
    if (this.categoriesCache$.value.length === 0) {
      this.getCategories(householdId).subscribe();
    }
    return this.categoriesCache$.asObservable();
  }

  /**
   * Get a single category by ID for a household
   *
   * @param householdId - The household ID
   * @param categoryId - The category ID (UUID)
   * @returns Observable of category
   */
  getCategory(householdId: string, categoryId: string): Observable<Category> {
    const params = new HttpParams().append('householdId', householdId);

    return this.http.get<Category>(`${this.API_URL}/categories/${categoryId}`, { params })
      .pipe(
        catchError(error => {
          console.error('Error fetching category:', error);
          throw error;
        })
      );
  }

  /**
   * Create a new category for a household
   *
   * @param householdId - The household ID
   * @param createDto - Category creation data
   * @returns Observable of created category
   */
  createCategory(householdId: string, createDto: CreateCategoryDto): Observable<Category> {
    const params = new HttpParams().append('householdId', householdId);

    return this.http.post<Category>(`${this.API_URL}/categories`, createDto, { params })
      .pipe(
        tap(category => {
          // Update cache
          const current = this.categoriesCache$.value;
          this.categoriesCache$.next([...current, category]);
        }),
        catchError(error => {
          console.error('Error creating category:', error);
          throw error;
        })
      );
  }

  /**
   * Update an existing category for a household
   *
   * @param householdId - The household ID
   * @param categoryId - The category ID (UUID)
   * @param updateDto - Update data
   * @returns Observable of updated category
   */
  updateCategory(householdId: string, categoryId: string, updateDto: UpdateCategoryDto): Observable<Category> {
    const params = new HttpParams().append('householdId', householdId);

    return this.http.put<Category>(`${this.API_URL}/categories/${categoryId}`, updateDto, { params })
      .pipe(
        tap(category => {
          // Update cache
          const current = this.categoriesCache$.value;
          const index = current.findIndex(c => c.id === categoryId);
          if (index !== -1) {
            const updated = [...current];
            updated[index] = category;
            this.categoriesCache$.next(updated);
          }
        }),
        catchError(error => {
          console.error('Error updating category:', error);
          throw error;
        })
      );
  }

  /**
   * Delete a category for a household
   *
   * @param householdId - The household ID
   * @param categoryId - The category ID (UUID)
   * @returns Observable of success status
   */
  deleteCategory(householdId: string, categoryId: string): Observable<{ success: boolean }> {
    const params = new HttpParams().append('householdId', householdId);

    return this.http.delete<{ success: boolean }>(`${this.API_URL}/categories/${categoryId}`, { params })
      .pipe(
        tap(() => {
          // Remove from cache
          const current = this.categoriesCache$.value;
          this.categoriesCache$.next(current.filter(c => c.id !== categoryId));
        }),
        catchError(error => {
          console.error('Error deleting category:', error);
          throw error;
        })
      );
  }

  /**
   * Get a single category type by ID for a household
   *
   * @param householdId - The household ID
   * @param categoryTypeId - The category type ID (UUID)
   * @returns Observable of category type
   */
  getCategoryType(householdId: string, categoryTypeId: string): Observable<CategoryType> {
    const params = new HttpParams().append('householdId', householdId);

    return this.http.get<CategoryType>(`${this.API_URL}/category-types/${categoryTypeId}`, { params })
      .pipe(
        catchError(error => {
          console.error('Error fetching category type:', error);
          throw error;
        })
      );
  }

  /**
   * Create a new category type for a household
   *
   * @param householdId - The household ID
   * @param createDto - Category type creation data
   * @returns Observable of created category type
   */
  createCategoryType(householdId: string, createDto: CreateCategoryTypeDto): Observable<CategoryType> {
    const params = new HttpParams().append('householdId', householdId);

    return this.http.post<CategoryType>(`${this.API_URL}/category-types`, createDto, { params })
      .pipe(
        tap(categoryType => {
          // Update cache
          const current = this.categoryTypesCache$.value;
          this.categoryTypesCache$.next([...current, categoryType]);
        }),
        catchError(error => {
          console.error('Error creating category type:', error);
          throw error;
        })
      );
  }

  /**
   * Update an existing category type for a household
   *
   * @param householdId - The household ID
   * @param categoryTypeId - The category type ID (UUID)
   * @param updateDto - Update data
   * @returns Observable of updated category type
   */
  updateCategoryType(householdId: string, categoryTypeId: string, updateDto: UpdateCategoryTypeDto): Observable<CategoryType> {
    const params = new HttpParams().append('householdId', householdId);

    return this.http.put<CategoryType>(`${this.API_URL}/category-types/${categoryTypeId}`, updateDto, { params })
      .pipe(
        tap(categoryType => {
          // Update cache
          const current = this.categoryTypesCache$.value;
          const index = current.findIndex(ct => ct.id === categoryTypeId);
          if (index !== -1) {
            const updated = [...current];
            updated[index] = categoryType;
            this.categoryTypesCache$.next(updated);
          }
        }),
        catchError(error => {
          console.error('Error updating category type:', error);
          throw error;
        })
      );
  }

  /**
   * Delete a category type for a household
   *
   * @param householdId - The household ID
   * @param categoryTypeId - The category type ID (UUID)
   * @returns Observable of success status
   */
  deleteCategoryType(householdId: string, categoryTypeId: string): Observable<{ success: boolean }> {
    const params = new HttpParams().append('householdId', householdId);

    return this.http.delete<{ success: boolean }>(`${this.API_URL}/category-types/${categoryTypeId}`, { params })
      .pipe(
        tap(() => {
          // Remove from cache
          const current = this.categoryTypesCache$.value;
          this.categoryTypesCache$.next(current.filter(ct => ct.id !== categoryTypeId));
        }),
        catchError(error => {
          console.error('Error deleting category type:', error);
          throw error;
        })
      );
  }

  /**
   * Update sort order for multiple categories for a household
   *
   * @param householdId - The household ID
   * @param updates - Array of category ID and sort order updates
   * @returns Observable of success status
   */
  updateCategoriesOrder(householdId: string, updates: CategorySortOrderItem[]): Observable<{ success: boolean }> {
    const dto: UpdateCategoriesSortOrderDto = { items: updates };
    const params = new HttpParams().append('householdId', householdId);

    return this.http.patch<{ success: boolean }>(`${this.API_URL}/categories/sort-order`, dto, { params })
      .pipe(
        tap(() => {
          // Update cache with new sort orders
          const current = this.categoriesCache$.value;
          const updated = current.map(category => {
            const update = updates.find(u => u.id === category.id);
            if (update) {
              return { ...category, sortOrder: update.sortOrder };
            }
            return category;
          });
          this.categoriesCache$.next(updated);
        }),
        catchError(error => {
          console.error('Error updating categories order:', error);
          throw error;
        })
      );
  }

  /**
   * Clear all caches
   */
  clearAllCaches(): void {
    this.categoryTypesCache$.next([]);
    this.categoriesCache$.next([]);
  }
}
