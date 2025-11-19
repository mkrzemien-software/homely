import { Injectable, signal, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError, tap, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';

/**
 * User role types in the system
 */
export enum UserRole {
  ADMIN = 'admin',
  MEMBER = 'member',
  DASHBOARD = 'dashboard',
  SYSTEM_DEVELOPER = 'system_developer'
}

/**
 * User account status
 */
export enum UserAccountStatus {
  ACTIVE = 'active',
  INACTIVE = 'inactive',
  LOCKED = 'locked',
  PENDING = 'pending'
}

/**
 * System user interface
 */
export interface SystemUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  status: UserAccountStatus;
  householdId: string;
  householdName: string;
  lastLogin: Date | null;
  createdAt: Date;
  updatedAt: Date;
}

/**
 * User activity log entry
 */
export interface UserActivity {
  id: string;
  userId: string;
  action: string;
  description: string;
  timestamp: Date;
  ipAddress?: string;
  userAgent?: string;
}

/**
 * User search filters
 */
export interface UserSearchFilters {
  searchTerm?: string;
  role?: UserRole;
  status?: UserAccountStatus;
  householdId?: string;
  page?: number;
  pageSize?: number;
}

/**
 * Paginated user response
 */
export interface PaginatedUsers {
  users: SystemUser[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Password reset request
 */
export interface PasswordResetRequest {
  userId: string;
  sendEmail: boolean;
}

/**
 * Role update request
 */
export interface RoleUpdateRequest {
  userId: string;
  householdId: string;
  newRole: UserRole;
}

/**
 * Create user request
 */
export interface CreateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  householdId?: string;
  role?: UserRole;
}

@Injectable({
  providedIn: 'root'
})
export class SystemUsersService {
  private readonly API_URL = `${environment.apiUrl}/system/users`;

  private http = inject(HttpClient);

  // Signals for reactive state management
  users = signal<SystemUser[]>([]);
  selectedUser = signal<SystemUser | null>(null);
  isLoading = signal<boolean>(false);
  totalUsers = signal<number>(0);

  /**
   * Search users globally across all households
   */
  searchUsers(filters: UserSearchFilters): Observable<PaginatedUsers> {
    this.isLoading.set(true);

    let params = new HttpParams();

    if (filters.searchTerm) {
      params = params.set('searchTerm', filters.searchTerm);
    }
    if (filters.role) {
      params = params.set('role', filters.role);
    }
    if (filters.status) {
      params = params.set('status', filters.status);
    }
    if (filters.householdId) {
      params = params.set('householdId', filters.householdId);
    }
    if (filters.page !== undefined) {
      params = params.set('page', filters.page.toString());
    }
    if (filters.pageSize !== undefined) {
      params = params.set('pageSize', filters.pageSize.toString());
    }

    return this.http.get<PaginatedUsers>(this.API_URL, { params }).pipe(
      tap((response) => {
        this.users.set(response.users);
        this.totalUsers.set(response.total);
        this.isLoading.set(false);
      }),
      catchError((error: HttpErrorResponse) => {
        this.isLoading.set(false);
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Get detailed user information
   */
  getUserDetails(userId: string): Observable<SystemUser> {
    this.isLoading.set(true);

    return this.http.get<SystemUser>(`${this.API_URL}/${userId}`).pipe(
      tap((user) => {
        this.selectedUser.set(user);
        this.isLoading.set(false);
      }),
      catchError((error: HttpErrorResponse) => {
        this.isLoading.set(false);
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Get user activity history
   */
  getUserActivity(userId: string, limit: number = 50): Observable<UserActivity[]> {
    return this.http.get<UserActivity[]>(`${this.API_URL}/${userId}/activity`, {
      params: { limit: limit.toString() }
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Reset user password
   */
  resetUserPassword(request: PasswordResetRequest): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/${request.userId}/reset-password`,
      { sendEmail: request.sendEmail }
    ).pipe(
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Unlock user account
   */
  unlockUserAccount(userId: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/${userId}/unlock`,
      {}
    ).pipe(
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Update user role in household
   */
  updateUserRole(request: RoleUpdateRequest): Observable<SystemUser> {
    return this.http.put<SystemUser>(
      `${this.API_URL}/${request.userId}/role`,
      { householdId: request.householdId, role: request.newRole }
    ).pipe(
      tap((updatedUser) => {
        // Update the user in the list
        const users = this.users();
        const index = users.findIndex(u => u.id === updatedUser.id);
        if (index !== -1) {
          users[index] = updatedUser;
          this.users.set([...users]);
        }

        // Update selected user if it's the same
        if (this.selectedUser()?.id === updatedUser.id) {
          this.selectedUser.set(updatedUser);
        }
      }),
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Move user to different household
   */
  moveUserToHousehold(userId: string, newHouseholdId: string): Observable<SystemUser> {
    return this.http.post<SystemUser>(
      `${this.API_URL}/${userId}/move`,
      { newHouseholdId }
    ).pipe(
      tap((updatedUser) => {
        const users = this.users();
        const index = users.findIndex(u => u.id === updatedUser.id);
        if (index !== -1) {
          users[index] = updatedUser;
          this.users.set([...users]);
        }

        if (this.selectedUser()?.id === updatedUser.id) {
          this.selectedUser.set(updatedUser);
        }
      }),
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Create new user (system admin only)
   */
  createUser(request: CreateUserRequest): Observable<SystemUser> {
    return this.http.post<SystemUser>(this.API_URL, request).pipe(
      tap((newUser) => {
        // Add the new user to the list
        const users = this.users();
        this.users.set([newUser, ...users]);
        this.totalUsers.set(this.totalUsers() + 1);
      }),
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Clear selected user
   */
  clearSelectedUser(): void {
    this.selectedUser.set(null);
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
          errorMessage = 'User not found';
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
