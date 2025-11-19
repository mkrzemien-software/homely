import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, catchError, of, map } from 'rxjs';
import {
  DashboardUpcomingEventsResponse,
  DashboardUpcomingEventsParams,
  DashboardStatisticsResponse,
  DashboardEvent
} from '../models/dashboard.model';
import { environment } from '../../../../../environments/environment';

/**
 * DashboardService
 *
 * Service for managing dashboard data.
 * Communicates with the backend API dashboard endpoints.
 *
 * Based on API plan:
 * - GET /dashboard/upcoming-events
 * - GET /dashboard/statistics
 *
 * Features:
 * - Fetches upcoming events for weekly calendar and event list
 * - Supports configurable date ranges (7, 14, 30 days)
 * - Provides household statistics for dashboard widgets
 * - Caching mechanism for better performance
 * - Automatic cache invalidation on household switch
 *
 * @example
 * const dashboardService = inject(DashboardService);
 * dashboardService.getUpcomingEvents({ days: 7, householdId: '123' }).subscribe(response => {
 *   console.log('Events:', response.data);
 *   console.log('Summary:', response.summary);
 * });
 */
@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);

  /**
   * Base API URL
   */
  private readonly API_URL = environment.apiUrl;

  /**
   * Cache for upcoming events
   * Key: combination of days and householdId
   */
  private upcomingEventsCache$ = new BehaviorSubject<Map<string, DashboardUpcomingEventsResponse>>(
    new Map()
  );

  /**
   * Cache for statistics
   * Key: householdId
   */
  private statisticsCache$ = new BehaviorSubject<Map<string, DashboardStatisticsResponse>>(
    new Map()
  );

  /**
   * Loading state for upcoming events
   */
  private loadingUpcomingEvents$ = new BehaviorSubject<boolean>(false);

  /**
   * Loading state for statistics
   */
  private loadingStatistics$ = new BehaviorSubject<boolean>(false);

  /**
   * Get upcoming events for dashboard
   * Supports configurable date ranges: 7, 14, or 30 days
   *
   * @param params - Query parameters (days, householdId, startDate)
   * @returns Observable of upcoming events response with summary
   */
  getUpcomingEvents(
    params?: DashboardUpcomingEventsParams
  ): Observable<DashboardUpcomingEventsResponse> {
    // Build query params
    let httpParams = new HttpParams();

    if (params) {
      if (params.days) {
        httpParams = httpParams.append('days', params.days.toString());
      }
      if (params.householdId) {
        httpParams = httpParams.append('householdId', params.householdId);
      }
      if (params.startDate) {
        httpParams = httpParams.append('startDate', params.startDate);
      }
    }

    // Generate cache key
    const cacheKey = this.generateCacheKey(params);

    // Check cache first
    const cached = this.upcomingEventsCache$.value.get(cacheKey);
    if (cached) {
      console.log('Returning cached upcoming events for:', cacheKey);
      return of(cached);
    }

    // Set loading state
    this.loadingUpcomingEvents$.next(true);

    return this.http
      .get<{ success: boolean; data: DashboardUpcomingEventsResponse }>(`${this.API_URL}/dashboard/upcoming-events`, {
        params: httpParams
      })
      .pipe(
        map(apiResponse => apiResponse.data), // Unwrap ApiResponseDto
        tap(response => {
          // Update cache
          const cache = this.upcomingEventsCache$.value;
          cache.set(cacheKey, response);
          this.upcomingEventsCache$.next(cache);

          // Clear loading state
          this.loadingUpcomingEvents$.next(false);

          console.log('Fetched upcoming events:', response);
        }),
        catchError(error => {
          console.error('Error fetching upcoming events:', error);

          // Clear loading state
          this.loadingUpcomingEvents$.next(false);

          // Return empty response on error
          return of({
            data: [],
            summary: {
              overdue: 0,
              today: 0,
              thisWeek: 0
            }
          });
        })
      );
  }

  /**
   * Get household statistics for dashboard
   *
   * @param householdId - Optional household ID filter
   * @returns Observable of dashboard statistics
   */
  getStatistics(householdId?: string): Observable<DashboardStatisticsResponse> {
    // Build query params
    let httpParams = new HttpParams();

    if (householdId) {
      httpParams = httpParams.append('householdId', householdId);
    }

    // Check cache first
    const cacheKey = householdId || 'default';
    const cached = this.statisticsCache$.value.get(cacheKey);
    if (cached) {
      console.log('Returning cached statistics for:', cacheKey);
      return of(cached);
    }

    // Set loading state
    this.loadingStatistics$.next(true);

    return this.http
      .get<{ success: boolean; data: DashboardStatisticsResponse }>(`${this.API_URL}/dashboard/statistics`, {
        params: httpParams
      })
      .pipe(
        map(apiResponse => apiResponse.data), // Unwrap ApiResponseDto
        tap(response => {
          // Update cache
          const cache = this.statisticsCache$.value;
          cache.set(cacheKey, response);
          this.statisticsCache$.next(cache);

          // Clear loading state
          this.loadingStatistics$.next(false);

          console.log('Fetched statistics:', response);
        }),
        catchError(error => {
          console.error('Error fetching statistics:', error);

          // Clear loading state
          this.loadingStatistics$.next(false);

          // Return empty response on error
          return of({
            tasks: {
              total: 0,
              byCategory: []
            },
            events: {
              pending: 0,
              overdue: 0,
              completedThisMonth: 0
            },
            planUsage: {
              tasksUsed: 0,
              tasksLimit: 5,
              membersUsed: 0,
              membersLimit: 3
            }
          });
        })
      );
  }

  /**
   * Get loading state for upcoming events
   */
  getUpcomingEventsLoadingState(): Observable<boolean> {
    return this.loadingUpcomingEvents$.asObservable();
  }

  /**
   * Get loading state for statistics
   */
  getStatisticsLoadingState(): Observable<boolean> {
    return this.loadingStatistics$.asObservable();
  }

  /**
   * Invalidate cache for specific household
   * Called when switching households or after data changes
   *
   * @param householdId - Optional household ID to invalidate (if not provided, clears all)
   */
  invalidateCache(householdId?: string): void {
    if (householdId) {
      // Invalidate only specific household caches
      const eventsCache = this.upcomingEventsCache$.value;
      const statsCache = this.statisticsCache$.value;

      // Remove all cache entries containing this household ID
      for (const key of eventsCache.keys()) {
        if (key.includes(householdId)) {
          eventsCache.delete(key);
        }
      }

      statsCache.delete(householdId);

      this.upcomingEventsCache$.next(eventsCache);
      this.statisticsCache$.next(statsCache);

      console.log('Invalidated cache for household:', householdId);
    } else {
      // Clear all caches
      this.upcomingEventsCache$.next(new Map());
      this.statisticsCache$.next(new Map());

      console.log('Cleared all dashboard caches');
    }
  }

  /**
   * Refresh upcoming events
   * Forces a fresh fetch from API, bypassing cache
   *
   * @param params - Query parameters
   * @returns Observable of upcoming events response
   */
  refreshUpcomingEvents(
    params?: DashboardUpcomingEventsParams
  ): Observable<DashboardUpcomingEventsResponse> {
    // Invalidate specific cache entry
    const cacheKey = this.generateCacheKey(params);
    const cache = this.upcomingEventsCache$.value;
    cache.delete(cacheKey);
    this.upcomingEventsCache$.next(cache);

    // Fetch fresh data
    return this.getUpcomingEvents(params);
  }

  /**
   * Refresh statistics
   * Forces a fresh fetch from API, bypassing cache
   *
   * @param householdId - Optional household ID
   * @returns Observable of statistics response
   */
  refreshStatistics(householdId?: string): Observable<DashboardStatisticsResponse> {
    // Invalidate specific cache entry
    const cacheKey = householdId || 'default';
    const cache = this.statisticsCache$.value;
    cache.delete(cacheKey);
    this.statisticsCache$.next(cache);

    // Fetch fresh data
    return this.getStatistics(householdId);
  }

  /**
   * Generate cache key from parameters
   * Format: "days:{days}|household:{id}|start:{date}"
   *
   * @param params - Query parameters
   * @returns Cache key string
   */
  private generateCacheKey(params?: DashboardUpcomingEventsParams): string {
    const days = params?.days || 7;
    const householdId = params?.householdId || 'default';
    const startDate = params?.startDate || 'today';

    return `days:${days}|household:${householdId}|start:${startDate}`;
  }
}
