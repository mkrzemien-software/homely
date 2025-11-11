import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, catchError, of } from 'rxjs';
import { Item, CreateItemDto, UpdateItemDto, Category } from '../models/item.model';
import { ItemFilter } from '../models/item-filter.model';

/**
 * ItemService
 *
 * Service for managing items (devices and visits) in a household.
 * Communicates with the backend API.
 *
 * Based on PRD section 3.2 - Zarządzanie urządzeniami i wizytami
 *
 * @example
 * const itemService = inject(ItemService);
 * itemService.getHouseholdItems(householdId).subscribe(items => {
 *   console.log('Items:', items);
 * });
 */
@Injectable({
  providedIn: 'root'
})
export class ItemService {
  private http = inject(HttpClient);

  /**
   * Base API URL
   * TODO: Move to environment configuration
   */
  private readonly API_URL = 'http://localhost:5000/api';

  /**
   * Cache for items by household
   */
  private itemsCache = new Map<string, BehaviorSubject<Item[]>>();

  /**
   * Cache for categories
   */
  private categoriesCache$ = new BehaviorSubject<Category[]>([]);

  /**
   * Get all items for a household
   *
   * @param householdId - The household ID
   * @param filter - Optional filter criteria
   * @returns Observable of items array
   */
  getHouseholdItems(householdId: string, filter?: ItemFilter): Observable<Item[]> {
    // Check cache first
    if (!this.itemsCache.has(householdId)) {
      this.itemsCache.set(householdId, new BehaviorSubject<Item[]>([]));
    }

    // Build query params
    let params = new HttpParams();
    if (filter?.categoryIds && filter.categoryIds.length > 0) {
      params = params.append('categoryIds', filter.categoryIds.join(','));
    }
    if (filter?.assignedUserIds && filter.assignedUserIds.length > 0) {
      params = params.append('assignedUserIds', filter.assignedUserIds.join(','));
    }
    if (filter?.activeOnly !== undefined) {
      params = params.append('activeOnly', filter.activeOnly);
    }

    // Fetch from API
    return this.http.get<Item[]>(`${this.API_URL}/households/${householdId}/items`, { params })
      .pipe(
        tap(items => {
          // Update cache
          this.itemsCache.get(householdId)?.next(items);
        }),
        catchError(error => {
          console.error('Error fetching household items:', error);
          // Return cached data on error
          return of(this.itemsCache.get(householdId)?.value || []);
        })
      );
  }

  /**
   * Get cached items for a household
   *
   * @param householdId - The household ID
   * @returns Observable of cached items
   */
  getCachedItems(householdId: string): Observable<Item[]> {
    if (!this.itemsCache.has(householdId)) {
      this.itemsCache.set(householdId, new BehaviorSubject<Item[]>([]));
      // Trigger fetch
      this.getHouseholdItems(householdId).subscribe();
    }
    return this.itemsCache.get(householdId)!.asObservable();
  }

  /**
   * Get a single item by ID
   *
   * @param itemId - The item ID
   * @returns Observable of item
   */
  getItem(itemId: string): Observable<Item> {
    return this.http.get<Item>(`${this.API_URL}/items/${itemId}`)
      .pipe(
        catchError(error => {
          console.error('Error fetching item:', error);
          throw error;
        })
      );
  }

  /**
   * Create a new item
   *
   * @param createDto - Item creation data
   * @returns Observable of created item
   */
  createItem(createDto: CreateItemDto): Observable<Item> {
    return this.http.post<Item>(`${this.API_URL}/items`, createDto)
      .pipe(
        tap(item => {
          // Update cache
          this.addItemToCache(item);
        }),
        catchError(error => {
          console.error('Error creating item:', error);
          throw error;
        })
      );
  }

  /**
   * Update an existing item
   *
   * @param itemId - The item ID
   * @param updateDto - Update data
   * @returns Observable of updated item
   */
  updateItem(itemId: string, updateDto: UpdateItemDto): Observable<Item> {
    return this.http.put<Item>(`${this.API_URL}/items/${itemId}`, updateDto)
      .pipe(
        tap(item => {
          // Update cache
          this.updateItemInCache(item);
        }),
        catchError(error => {
          console.error('Error updating item:', error);
          throw error;
        })
      );
  }

  /**
   * Delete an item (soft delete)
   *
   * @param itemId - The item ID
   * @returns Observable of success status
   */
  deleteItem(itemId: string): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(`${this.API_URL}/items/${itemId}`)
      .pipe(
        tap(() => {
          // Remove from cache
          this.removeItemFromCache(itemId);
        }),
        catchError(error => {
          console.error('Error deleting item:', error);
          throw error;
        })
      );
  }

  /**
   * Get all categories
   *
   * @returns Observable of categories array
   */
  getCategories(): Observable<Category[]> {
    // Return cached if available
    if (this.categoriesCache$.value.length > 0) {
      return this.categoriesCache$.asObservable();
    }

    return this.http.get<Category[]>(`${this.API_URL}/categories/active`)
      .pipe(
        tap(categories => {
          this.categoriesCache$.next(categories);
        }),
        catchError(error => {
          console.error('Error fetching categories:', error);
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
   * Clear cache for a household
   *
   * @param householdId - The household ID
   */
  clearCache(householdId: string): void {
    this.itemsCache.delete(householdId);
  }

  /**
   * Clear all caches
   */
  clearAllCaches(): void {
    this.itemsCache.clear();
    this.categoriesCache$.next([]);
  }

  /**
   * Add item to cache
   */
  private addItemToCache(item: Item): void {
    const cache = this.itemsCache.get(item.householdId);
    if (cache) {
      const currentItems = cache.value;
      cache.next([...currentItems, item]);
    }
  }

  /**
   * Update item in cache
   */
  private updateItemInCache(updatedItem: Item): void {
    const cache = this.itemsCache.get(updatedItem.householdId);
    if (cache) {
      const currentItems = cache.value;
      const index = currentItems.findIndex(item => item.id === updatedItem.id);
      if (index !== -1) {
        const newItems = [...currentItems];
        newItems[index] = updatedItem;
        cache.next(newItems);
      }
    }
  }

  /**
   * Remove item from cache
   */
  private removeItemFromCache(itemId: string): void {
    this.itemsCache.forEach(cache => {
      const currentItems = cache.value;
      const filtered = currentItems.filter(item => item.id !== itemId);
      if (filtered.length !== currentItems.length) {
        cache.next(filtered);
      }
    });
  }
}
