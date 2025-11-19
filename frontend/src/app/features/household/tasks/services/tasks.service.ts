import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, catchError, of, map } from 'rxjs';
import {
  Task,
  TasksResponse,
  CreateTaskDto,
  UpdateTaskDto,
  TasksQueryParams
} from '../models/task.model';
import { environment } from '../../../../../environments/environment';

/**
 * TasksService
 *
 * Service for managing task templates.
 * Communicates with the backend API.
 *
 * Based on API plan:
 * - GET /tasks
 * - POST /tasks
 * - GET /tasks/{id}
 * - PUT /tasks/{id}
 * - DELETE /tasks/{id}
 *
 * @example
 * const tasksService = inject(TasksService);
 * tasksService.getTasks({ householdId: '123' }).subscribe(response => {
 *   console.log('Tasks:', response.data);
 * });
 */
@Injectable({
  providedIn: 'root'
})
export class TasksService {
  private http = inject(HttpClient);

  /**
   * Base API URL
   */
  private readonly API_URL = environment.apiUrl;

  /**
   * Cache for tasks
   * Note: Cache is per household, so we may need to invalidate on household switch
   */
  private tasksCache$ = new BehaviorSubject<Task[]>([]);

  /**
   * Current pagination info
   */
  private currentPagination$ = new BehaviorSubject<TasksResponse['pagination'] | null>(null);

  /**
   * Get tasks with optional filters and pagination
   *
   * @param queryParams - Query parameters for filtering and pagination
   * @returns Observable of tasks response with pagination
   */
  getTasks(queryParams?: TasksQueryParams): Observable<TasksResponse> {
    // Build query params
    let params = new HttpParams();

    if (queryParams) {
      if (queryParams.householdId) {
        params = params.append('householdId', queryParams.householdId);
      }
      if (queryParams.categoryId !== undefined) {
        params = params.append('categoryId', queryParams.categoryId.toString());
      }
      if (queryParams.priority) {
        params = params.append('priority', queryParams.priority);
      }
      if (queryParams.hasInterval !== undefined) {
        params = params.append('hasInterval', queryParams.hasInterval.toString());
      }
      if (queryParams.isActive !== undefined) {
        params = params.append('isActive', queryParams.isActive.toString());
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

    return this.http.get<TasksResponse>(`${this.API_URL}/tasks`, { params })
      .pipe(
        tap(response => {
          // Update cache with tasks from current page
          this.tasksCache$.next(response.data);
          this.currentPagination$.next(response.pagination);
        }),
        catchError(error => {
          console.error('Error fetching tasks:', error);
          // Return cached data on error
          return of({
            data: this.tasksCache$.value,
            pagination: this.currentPagination$.value || {
              currentPage: 1,
              totalPages: 1,
              totalItems: this.tasksCache$.value.length,
              itemsPerPage: 20
            }
          });
        })
      );
  }

  /**
   * Get cached tasks
   */
  getCachedTasks(): Observable<Task[]> {
    return this.tasksCache$.asObservable();
  }

  /**
   * Get cached pagination info
   */
  getCachedPagination(): Observable<TasksResponse['pagination'] | null> {
    return this.currentPagination$.asObservable();
  }

  /**
   * Get a single task by ID
   *
   * @param taskId - The task ID
   * @returns Observable of task
   */
  getTask(taskId: string): Observable<Task> {
    return this.http.get<Task>(`${this.API_URL}/tasks/${taskId}`)
      .pipe(
        catchError(error => {
          console.error('Error fetching task:', error);
          throw error;
        })
      );
  }

  /**
   * Create a new task
   *
   * @param createDto - Task creation data
   * @returns Observable of created task
   */
  createTask(createDto: CreateTaskDto): Observable<Task> {
    return this.http.post<Task>(`${this.API_URL}/tasks`, createDto)
      .pipe(
        tap(task => {
          // Update cache - add new task
          const current = this.tasksCache$.value;
          this.tasksCache$.next([task, ...current]);
        }),
        catchError(error => {
          console.error('Error creating task:', error);
          throw error;
        })
      );
  }

  /**
   * Update an existing task
   *
   * @param taskId - The task ID
   * @param updateDto - Update data
   * @returns Observable of updated task
   */
  updateTask(taskId: string, updateDto: UpdateTaskDto): Observable<Task> {
    return this.http.put<Task>(`${this.API_URL}/tasks/${taskId}`, updateDto)
      .pipe(
        tap(task => {
          // Update cache
          const current = this.tasksCache$.value;
          const index = current.findIndex(t => t.id === taskId);
          if (index !== -1) {
            const updated = [...current];
            updated[index] = task;
            this.tasksCache$.next(updated);
          }
        }),
        catchError(error => {
          console.error('Error updating task:', error);
          throw error;
        })
      );
  }

  /**
   * Delete a task (soft delete)
   *
   * @param taskId - The task ID
   * @returns Observable of success message
   */
  deleteTask(taskId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.API_URL}/tasks/${taskId}`)
      .pipe(
        tap(() => {
          // Remove from cache
          const current = this.tasksCache$.value;
          this.tasksCache$.next(current.filter(t => t.id !== taskId));
        }),
        catchError(error => {
          console.error('Error deleting task:', error);
          throw error;
        })
      );
  }

  /**
   * Clear cache
   * Useful when switching households or after major data changes
   */
  clearCache(): void {
    this.tasksCache$.next([]);
    this.currentPagination$.next(null);
  }
}
