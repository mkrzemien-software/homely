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
  UpdateCategoryTypeDto
} from '../models/category.model';

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
   * TODO: Move to environment configuration
   */
  private readonly API_URL = 'http://localhost:5000/api';

  /**
   * Cache for category types
   */
  private categoryTypesCache$ = new BehaviorSubject<CategoryType[]>([]);

  /**
   * Cache for categories
   */
  private categoriesCache$ = new BehaviorSubject<Category[]>([]);

  /**
   * Get all category types
   *
   * @returns Observable of category types array
   */
  getCategoryTypes(): Observable<CategoryType[]> {
    // Return cached if available
    if (this.categoryTypesCache$.value.length > 0) {
      return this.categoryTypesCache$.asObservable();
    }

    return this.http.get<CategoryTypesResponse>(`${this.API_URL}/category-types`)
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
   */
  getCachedCategoryTypes(): Observable<CategoryType[]> {
    if (this.categoryTypesCache$.value.length === 0) {
      this.getCategoryTypes().subscribe();
    }
    return this.categoryTypesCache$.asObservable();
  }

  /**
   * Get all categories
   *
   * @param categoryTypeId - Optional filter by category type
   * @returns Observable of categories array
   */
  getCategories(categoryTypeId?: number): Observable<Category[]> {
    // Build query params
    let params = new HttpParams();
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
   */
  getCachedCategories(): Observable<Category[]> {
    if (this.categoriesCache$.value.length === 0) {
      this.getCategories().subscribe();
    }
    return this.categoriesCache$.asObservable();
  }

  /**
   * Get a single category by ID
   *
   * @param categoryId - The category ID
   * @returns Observable of category
   */
  getCategory(categoryId: number): Observable<Category> {
    return this.http.get<Category>(`${this.API_URL}/categories/${categoryId}`)
      .pipe(
        catchError(error => {
          console.error('Error fetching category:', error);
          throw error;
        })
      );
  }

  /**
   * Create a new category
   *
   * @param createDto - Category creation data
   * @returns Observable of created category
   */
  createCategory(createDto: CreateCategoryDto): Observable<Category> {
    return this.http.post<Category>(`${this.API_URL}/categories`, createDto)
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
   * Update an existing category
   *
   * @param categoryId - The category ID
   * @param updateDto - Update data
   * @returns Observable of updated category
   */
  updateCategory(categoryId: number, updateDto: UpdateCategoryDto): Observable<Category> {
    return this.http.put<Category>(`${this.API_URL}/categories/${categoryId}`, updateDto)
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
   * Delete a category
   *
   * @param categoryId - The category ID
   * @returns Observable of success status
   */
  deleteCategory(categoryId: number): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(`${this.API_URL}/categories/${categoryId}`)
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
   * Get a single category type by ID
   *
   * @param categoryTypeId - The category type ID
   * @returns Observable of category type
   */
  getCategoryType(categoryTypeId: number): Observable<CategoryType> {
    return this.http.get<CategoryType>(`${this.API_URL}/category-types/${categoryTypeId}`)
      .pipe(
        catchError(error => {
          console.error('Error fetching category type:', error);
          throw error;
        })
      );
  }

  /**
   * Create a new category type
   *
   * @param createDto - Category type creation data
   * @returns Observable of created category type
   */
  createCategoryType(createDto: CreateCategoryTypeDto): Observable<CategoryType> {
    return this.http.post<CategoryType>(`${this.API_URL}/category-types`, createDto)
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
   * Update an existing category type
   *
   * @param categoryTypeId - The category type ID
   * @param updateDto - Update data
   * @returns Observable of updated category type
   */
  updateCategoryType(categoryTypeId: number, updateDto: UpdateCategoryTypeDto): Observable<CategoryType> {
    return this.http.put<CategoryType>(`${this.API_URL}/category-types/${categoryTypeId}`, updateDto)
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
   * Delete a category type
   *
   * @param categoryTypeId - The category type ID
   * @returns Observable of success status
   */
  deleteCategoryType(categoryTypeId: number): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(`${this.API_URL}/category-types/${categoryTypeId}`)
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
   * Clear all caches
   */
  clearAllCaches(): void {
    this.categoryTypesCache$.next([]);
    this.categoriesCache$.next([]);
  }
}
