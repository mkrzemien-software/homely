import { Injectable, signal, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError, tap, catchError } from 'rxjs';

/**
 * Subscription status types
 */
export enum SubscriptionStatus {
  FREE = 'free',
  ACTIVE = 'active',
  CANCELLED = 'cancelled',
  EXPIRED = 'expired'
}

/**
 * System household interface
 */
export interface SystemHousehold {
  id: string;
  name: string;
  address?: string;
  planTypeId: number;
  planName: string;
  subscriptionStatus: SubscriptionStatus;
  subscriptionStartDate?: Date;
  subscriptionEndDate?: Date;
  memberCount: number;
  itemCount: number;
  taskCount: number;
  createdAt: Date;
  updatedAt: Date;
  isDeleted: boolean;
}

/**
 * Household member summary
 */
export interface HouseholdMemberSummary {
  id: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  joinedAt: Date;
  lastActiveAt?: Date;
}

/**
 * Detailed household information
 */
export interface SystemHouseholdDetails {
  id: string;
  name: string;
  address?: string;
  planTypeId: number;
  planName: string;
  subscriptionStatus: SubscriptionStatus;
  subscriptionStartDate?: Date;
  subscriptionEndDate?: Date;
  createdAt: Date;
  updatedAt: Date;
  memberCount: number;
  itemCount: number;
  taskCount: number;
  completedTasksCount: number;
  lastActivityAt?: Date;
  maxMembers?: number;
  maxItems?: number;
  members: HouseholdMemberSummary[];
}

/**
 * Household search filters
 */
export interface HouseholdSearchFilters {
  searchTerm?: string;
  planTypeId?: number;
  subscriptionStatus?: SubscriptionStatus;
  hasActiveMembers?: boolean;
  page?: number;
  pageSize?: number;
}

/**
 * Paginated households response
 */
export interface PaginatedHouseholds {
  households: SystemHousehold[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Create household request
 */
export interface CreateHouseholdRequest {
  name: string;
  address?: string;
}

/**
 * Update household request
 */
export interface UpdateHouseholdRequest {
  name?: string;
  address?: string;
  planTypeId?: number;
  subscriptionStatus?: SubscriptionStatus;
}

/**
 * Household statistics
 */
export interface HouseholdStats {
  totalHouseholds: number;
  activeHouseholds: number;
  freeHouseholds: number;
  premiumHouseholds: number;
  totalMembers: number;
  totalItems: number;
  totalTasks: number;
}

/**
 * Assign admin request
 */
export interface AssignAdminRequest {
  householdId: string;
  userId: string;
}

@Injectable({
  providedIn: 'root'
})
export class SystemHouseholdsService {
  private readonly API_URL = 'http://localhost:5000/api/system/households';

  private http = inject(HttpClient);

  // Signals for reactive state management
  households = signal<SystemHousehold[]>([]);
  selectedHousehold = signal<SystemHouseholdDetails | null>(null);
  householdStats = signal<HouseholdStats | null>(null);
  isLoading = signal<boolean>(false);
  totalHouseholds = signal<number>(0);

  /**
   * Search households globally with filters
   */
  searchHouseholds(filters: HouseholdSearchFilters): Observable<PaginatedHouseholds> {
    this.isLoading.set(true);

    let params = new HttpParams();

    if (filters.searchTerm) {
      params = params.set('searchTerm', filters.searchTerm);
    }
    if (filters.planTypeId !== undefined) {
      params = params.set('planTypeId', filters.planTypeId.toString());
    }
    if (filters.subscriptionStatus) {
      params = params.set('subscriptionStatus', filters.subscriptionStatus);
    }
    if (filters.hasActiveMembers !== undefined) {
      params = params.set('hasActiveMembers', filters.hasActiveMembers.toString());
    }
    if (filters.page !== undefined) {
      params = params.set('page', filters.page.toString());
    }
    if (filters.pageSize !== undefined) {
      params = params.set('pageSize', filters.pageSize.toString());
    }

    return this.http.get<PaginatedHouseholds>(this.API_URL, { params }).pipe(
      tap((response) => {
        this.households.set(response.households);
        this.totalHouseholds.set(response.total);
        this.isLoading.set(false);
      }),
      catchError((error: HttpErrorResponse) => {
        this.isLoading.set(false);
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Get detailed household information
   */
  getHouseholdDetails(householdId: string): Observable<SystemHouseholdDetails> {
    this.isLoading.set(true);

    return this.http.get<SystemHouseholdDetails>(`${this.API_URL}/${householdId}`).pipe(
      tap((household) => {
        this.selectedHousehold.set(household);
        this.isLoading.set(false);
      }),
      catchError((error: HttpErrorResponse) => {
        this.isLoading.set(false);
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Get overall system household statistics
   */
  getHouseholdStats(): Observable<HouseholdStats> {
    return this.http.get<HouseholdStats>(`${this.API_URL}/stats`).pipe(
      tap((stats) => {
        this.householdStats.set(stats);
      }),
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Create new household
   */
  createHousehold(request: CreateHouseholdRequest): Observable<SystemHousehold> {
    return this.http.post<SystemHousehold>(this.API_URL, request).pipe(
      tap((household) => {
        // Add to the list
        const households = this.households();
        this.households.set([household, ...households]);
        this.totalHouseholds.set(this.totalHouseholds() + 1);
      }),
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Update household information
   */
  updateHousehold(householdId: string, request: UpdateHouseholdRequest): Observable<SystemHousehold> {
    return this.http.put<SystemHousehold>(`${this.API_URL}/${householdId}`, request).pipe(
      tap((updatedHousehold) => {
        // Update the household in the list
        const households = this.households();
        const index = households.findIndex(h => h.id === updatedHousehold.id);
        if (index !== -1) {
          households[index] = updatedHousehold;
          this.households.set([...households]);
        }

        // Update selected household if it's the same
        const selected = this.selectedHousehold();
        if (selected?.id === updatedHousehold.id) {
          this.selectedHousehold.set({
            ...selected,
            ...updatedHousehold
          });
        }
      }),
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Delete household (soft delete)
   */
  deleteHousehold(householdId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(
      `${this.API_URL}/${householdId}`
    ).pipe(
      tap(() => {
        // Remove from the list
        const households = this.households();
        this.households.set(households.filter(h => h.id !== householdId));
        this.totalHouseholds.set(this.totalHouseholds() - 1);

        // Clear selected if it was deleted
        if (this.selectedHousehold()?.id === householdId) {
          this.selectedHousehold.set(null);
        }
      }),
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Clear selected household
   */
  clearSelectedHousehold(): void {
    this.selectedHousehold.set(null);
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse): Error {
    let errorMessage = 'An error occurred while processing your request';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      switch (error.status) {
        case 400:
          errorMessage = 'Invalid request parameters';
          break;
        case 401:
          errorMessage = 'Unauthorized access';
          break;
        case 403:
          errorMessage = 'Insufficient permissions';
          break;
        case 404:
          errorMessage = 'Household not found';
          break;
        case 500:
          errorMessage = 'Server error. Please try again later';
          break;
        default:
          errorMessage = error.error?.message || errorMessage;
      }
    }

    return new Error(errorMessage);
  }
}
