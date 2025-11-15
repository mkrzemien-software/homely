import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, catchError, of } from 'rxjs';
import {
  Event,
  EventsResponse,
  CreateEventDto,
  UpdateEventDto,
  CompleteEventDto,
  PostponeEventDto,
  CancelEventDto,
  EventsQueryParams
} from '../models/event.model';

/**
 * EventsService
 *
 * Service for managing events (concrete task occurrences).
 * Communicates with the backend API.
 *
 * Based on API plan:
 * - GET /events
 * - POST /events
 * - GET /events/{id}
 * - PUT /events/{id}
 * - DELETE /events/{id}
 * - POST /events/{id}/complete
 * - POST /events/{id}/postpone
 * - POST /events/{id}/cancel
 *
 * @example
 * const eventsService = inject(EventsService);
 * eventsService.getEvents({ householdId: '123' }).subscribe(response => {
 *   console.log('Events:', response.data);
 * });
 */
@Injectable({
  providedIn: 'root'
})
export class EventsService {
  private http = inject(HttpClient);

  /**
   * Base API URL
   * TODO: Move to environment configuration
   */
  private readonly API_URL = 'http://localhost:5000/api';

  /**
   * Cache for events
   * Note: Cache is per household, so we may need to invalidate on household switch
   */
  private eventsCache$ = new BehaviorSubject<Event[]>([]);

  /**
   * Current pagination info
   */
  private currentPagination$ = new BehaviorSubject<EventsResponse['pagination'] | null>(null);

  /**
   * Get events with optional filters and pagination
   *
   * @param queryParams - Query parameters for filtering and pagination
   * @returns Observable of events response with pagination
   */
  getEvents(queryParams?: EventsQueryParams): Observable<EventsResponse> {
    // Build query params
    let params = new HttpParams();

    if (queryParams) {
      if (queryParams.householdId) {
        params = params.append('householdId', queryParams.householdId);
      }
      if (queryParams.taskId) {
        params = params.append('taskId', queryParams.taskId);
      }
      if (queryParams.assignedToId) {
        params = params.append('assignedToId', queryParams.assignedToId);
      }
      if (queryParams.categoryId !== undefined) {
        params = params.append('categoryId', queryParams.categoryId.toString());
      }
      if (queryParams.priority) {
        params = params.append('priority', queryParams.priority);
      }
      if (queryParams.status) {
        params = params.append('status', queryParams.status);
      }
      if (queryParams.startDate) {
        params = params.append('startDate', queryParams.startDate);
      }
      if (queryParams.endDate) {
        params = params.append('endDate', queryParams.endDate);
      }
      if (queryParams.isOverdue !== undefined) {
        params = params.append('isOverdue', queryParams.isOverdue.toString());
      }
      if (queryParams.sortBy) {
        params = params.append('sortBy', queryParams.sortBy);
      }
      if (queryParams.sortOrder) {
        params = params.append('sortOrder', queryParams.sortOrder);
      }
      if (queryParams.page) {
        params = params.append('page', queryParams.page.toString());
      }
      if (queryParams.limit) {
        params = params.append('limit', queryParams.limit.toString());
      }
    }

    return this.http.get<EventsResponse>(`${this.API_URL}/events`, { params })
      .pipe(
        tap(response => {
          // Update cache with events from current page
          this.eventsCache$.next(response.data);
          this.currentPagination$.next(response.pagination);
        }),
        catchError(error => {
          console.error('Error fetching events:', error);
          // Return cached data on error
          return of({
            data: this.eventsCache$.value,
            pagination: this.currentPagination$.value || {
              currentPage: 1,
              totalPages: 1,
              totalItems: this.eventsCache$.value.length,
              itemsPerPage: 20
            }
          });
        })
      );
  }

  /**
   * Get cached events
   */
  getCachedEvents(): Observable<Event[]> {
    return this.eventsCache$.asObservable();
  }

  /**
   * Get cached pagination info
   */
  getCachedPagination(): Observable<EventsResponse['pagination'] | null> {
    return this.currentPagination$.asObservable();
  }

  /**
   * Get a single event by ID
   *
   * @param eventId - The event ID
   * @returns Observable of event
   */
  getEvent(eventId: string): Observable<Event> {
    return this.http.get<Event>(`${this.API_URL}/events/${eventId}`)
      .pipe(
        catchError(error => {
          console.error('Error fetching event:', error);
          throw error;
        })
      );
  }

  /**
   * Create a new event
   *
   * @param createDto - Event creation data
   * @returns Observable of created event
   */
  createEvent(createDto: CreateEventDto): Observable<Event> {
    return this.http.post<Event>(`${this.API_URL}/events`, createDto)
      .pipe(
        tap(event => {
          // Update cache - add new event
          const current = this.eventsCache$.value || [];
          this.eventsCache$.next([event, ...current]);
        }),
        catchError(error => {
          console.error('Error creating event:', error);
          throw error;
        })
      );
  }

  /**
   * Update an existing event
   *
   * @param eventId - The event ID
   * @param updateDto - Update data
   * @returns Observable of updated event
   */
  updateEvent(eventId: string, updateDto: UpdateEventDto): Observable<Event> {
    return this.http.put<Event>(`${this.API_URL}/events/${eventId}`, updateDto)
      .pipe(
        tap(event => {
          // Update cache
          const current = this.eventsCache$.value;
          const index = current.findIndex(e => e.id === eventId);
          if (index !== -1) {
            const updated = [...current];
            updated[index] = event;
            this.eventsCache$.next(updated);
          }
        }),
        catchError(error => {
          console.error('Error updating event:', error);
          throw error;
        })
      );
  }

  /**
   * Delete an event (soft delete)
   *
   * @param eventId - The event ID
   * @returns Observable of success message
   */
  deleteEvent(eventId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.API_URL}/events/${eventId}`)
      .pipe(
        tap(() => {
          // Remove from cache
          const current = this.eventsCache$.value;
          this.eventsCache$.next(current.filter(e => e.id !== eventId));
        }),
        catchError(error => {
          console.error('Error deleting event:', error);
          throw error;
        })
      );
  }

  /**
   * Complete an event
   * Automatically generates next event if task has interval
   *
   * @param eventId - The event ID
   * @param completeDto - Completion data
   * @returns Observable of completed event and optionally next generated event
   */
  completeEvent(eventId: string, completeDto: CompleteEventDto): Observable<{
    completedEvent: Event;
    nextEvent?: Event
  }> {
    return this.http.post<{ completedEvent: Event; nextEvent?: Event }>(
      `${this.API_URL}/events/${eventId}/complete`,
      completeDto
    ).pipe(
      tap(response => {
        // Update cache - update completed event
        const current = this.eventsCache$.value;
        const index = current.findIndex(e => e.id === eventId);
        if (index !== -1) {
          const updated = [...current];
          updated[index] = response.completedEvent;

          // Add next event if generated
          if (response.nextEvent) {
            updated.push(response.nextEvent);
          }

          this.eventsCache$.next(updated);
        }
      }),
      catchError(error => {
        console.error('Error completing event:', error);
        throw error;
      })
    );
  }

  /**
   * Postpone an event
   *
   * @param eventId - The event ID
   * @param postponeDto - Postponement data (new date and reason)
   * @returns Observable of postponed event
   */
  postponeEvent(eventId: string, postponeDto: PostponeEventDto): Observable<Event> {
    return this.http.post<Event>(
      `${this.API_URL}/events/${eventId}/postpone`,
      postponeDto
    ).pipe(
      tap(event => {
        // Update cache
        const current = this.eventsCache$.value;
        const index = current.findIndex(e => e.id === eventId);
        if (index !== -1) {
          const updated = [...current];
          updated[index] = event;
          this.eventsCache$.next(updated);
        }
      }),
      catchError(error => {
        console.error('Error postponing event:', error);
        throw error;
      })
    );
  }

  /**
   * Cancel an event
   *
   * @param eventId - The event ID
   * @param cancelDto - Cancellation data (reason)
   * @returns Observable of cancelled event
   */
  cancelEvent(eventId: string, cancelDto: CancelEventDto): Observable<Event> {
    return this.http.post<Event>(
      `${this.API_URL}/events/${eventId}/cancel`,
      cancelDto
    ).pipe(
      tap(event => {
        // Update cache
        const current = this.eventsCache$.value;
        const index = current.findIndex(e => e.id === eventId);
        if (index !== -1) {
          const updated = [...current];
          updated[index] = event;
          this.eventsCache$.next(updated);
        }
      }),
      catchError(error => {
        console.error('Error cancelling event:', error);
        throw error;
      })
    );
  }

  /**
   * Clear cache
   * Useful when switching households or after major data changes
   */
  clearCache(): void {
    this.eventsCache$.next([]);
    this.currentPagination$.next(null);
  }
}
