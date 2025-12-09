import { Injectable, signal, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, tap, catchError } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

/**
 * Login credentials interface
 */
export interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
}

/**
 * API Response wrapper
 */
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  errorMessage?: string;
  errors?: string[];
  statusCode: number;
}

/**
 * Login response data from API
 */
export interface LoginResponseData {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: UserDto;
}

/**
 * Login response (wrapped in ApiResponse)
 */
export interface LoginResponse extends ApiResponse<LoginResponseData> {}

/**
 * User household interface
 */
export interface UserHousehold {
  householdId: string;
  householdName: string;
  role: string;
  joinedAt: string;
}

/**
 * User data from API
 */
export interface UserDto {
  id: string;
  email: string;
  name: string;
  emailConfirmed: boolean;
  createdAt: string;
  /** @deprecated Use households instead */
  householdId: string;
  /** @deprecated Use households instead */
  role: string;
  households: UserHousehold[];
}

/**
 * User profile interface
 */
export interface UserProfile {
  id: string;
  email: string;
  name: string;
  emailConfirmed: boolean;
  createdAt: string;
  /** @deprecated Use households instead */
  householdId: string;
  /** @deprecated Use households instead */
  role: string;
  households: UserHousehold[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = `${environment.apiUrl}/auth`;
  private readonly TOKEN_KEY = 'homely-auth-token';
  private readonly REFRESH_TOKEN_KEY = 'homely-refresh-token';
  private readonly USER_KEY = 'homely-user';
  private readonly ACTIVE_HOUSEHOLD_KEY = 'homely-active-household';

  private http = inject(HttpClient);
  private router = inject(Router);

  // Signals for reactive state management
  isAuthenticated = signal<boolean>(false);
  currentUser = signal<UserProfile | null>(null);
  isLoading = signal<boolean>(false);
  activeHouseholdId = signal<string | null>(null);
  userHouseholds = signal<UserHousehold[]>([]);

  constructor() {
    // Check if user is already authenticated on service initialization
    this.checkAuthStatus();
  }

  /**
   * Login user with credentials
   */
  login(credentials: LoginCredentials): Observable<LoginResponse> {
    this.isLoading.set(true);

    return this.http.post<LoginResponse>(`${this.API_URL}/login`, credentials).pipe(
      tap((response) => {
        console.log(response);

        if (!response.success || !response.data) {
          throw new Error(response.errorMessage || 'Login failed');
        }

        // Store reference to response data for use in microtask
        const responseData = response.data;

        // IMPORTANT: Store tokens and user data synchronously first
        // This ensures the token is available in localStorage before any
        // reactive effects are triggered by signal updates
        this.storeTokens(responseData.accessToken, responseData.refreshToken);
        this.storeUser(responseData.user);

        // Set active household (use stored preference or first household)
        const storedActiveHousehold = this.getActiveHouseholdId();
        const userHouseholds = responseData.user.households || [];
        const activeHousehold = storedActiveHousehold && userHouseholds.some(h => h.householdId === storedActiveHousehold)
          ? storedActiveHousehold
          : userHouseholds[0]?.householdId || null;

        if (activeHousehold) {
          this.storeActiveHousehold(activeHousehold);
        }

        // Use queueMicrotask to defer signal updates until after current execution
        // This ensures localStorage operations complete before effects fire
        queueMicrotask(() => {
          this.isAuthenticated.set(true);
          this.currentUser.set(responseData.user);
          this.userHouseholds.set(userHouseholds);
          this.activeHouseholdId.set(activeHousehold);
        });
      }),
      catchError((error: HttpErrorResponse) => {
        this.isLoading.set(false);
        return throwError(() => this.handleError(error));
      }),
      tap(() => {
        this.isLoading.set(false);
      })
    );
  }

  /**
   * Logout user
   */
  logout(): void {
    // Clear tokens and user data
    this.clearAuthData();

    // Update state
    this.isAuthenticated.set(false);
    this.currentUser.set(null);
    this.activeHouseholdId.set(null);
    this.userHouseholds.set([]);

    // Navigate to login
    this.router.navigate(['/auth/login']);
  }

  /**
   * Get stored access token
   */
  getToken(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem(this.TOKEN_KEY);
    }
    return null;
  }

  /**
   * Get stored refresh token
   */
  getRefreshToken(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem(this.REFRESH_TOKEN_KEY);
    }
    return null;
  }

  /**
   * Check if user is authenticated
   */
  isLoggedIn(): boolean {
    return this.isAuthenticated();
  }

  /**
   * Decode JWT token and extract payload
   */
  private decodeToken(token: string): any {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) {
        return null;
      }

      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  /**
   * Check if token is expired
   */
  private isTokenExpired(token: string): boolean {
    const decoded = this.decodeToken(token);

    if (!decoded || !decoded.exp) {
      return true;
    }

    // exp is in seconds, Date.now() is in milliseconds
    const expirationDate = decoded.exp * 1000;
    const now = Date.now();

    return now >= expirationDate;
  }

  /**
   * Check if token is valid (exists and not expired)
   */
  isTokenValid(): boolean {
    const token = this.getToken();

    if (!token) {
      return false;
    }

    return !this.isTokenExpired(token);
  }

  /**
   * Get current user profile
   */
  getCurrentUser(): UserProfile | null {
    return this.currentUser();
  }

  /**
   * Store tokens in localStorage
   */
  private storeTokens(token: string, refreshToken?: string): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.setItem(this.TOKEN_KEY, token);
      if (refreshToken) {
        localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
      }
    }
  }

  /**
   * Store user data in localStorage
   */
  private storeUser(user: UserProfile): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    }
  }

  /**
   * Clear all authentication data from localStorage
   */
  private clearAuthData(): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.removeItem(this.TOKEN_KEY);
      localStorage.removeItem(this.REFRESH_TOKEN_KEY);
      localStorage.removeItem(this.USER_KEY);
      localStorage.removeItem(this.ACTIVE_HOUSEHOLD_KEY);
    }
  }

  /**
   * Check authentication status on service initialization
   */
  private checkAuthStatus(): void {
    const token = this.getToken();
    const userJson = typeof window !== 'undefined' && window.localStorage
      ? localStorage.getItem(this.USER_KEY)
      : null;

    if (token && userJson) {
      // Check if token is expired
      if (this.isTokenExpired(token)) {
        console.warn('Token has expired, clearing authentication data');
        this.clearAuthData();
        this.isAuthenticated.set(false);
        this.currentUser.set(null);
        this.activeHouseholdId.set(null);
        this.userHouseholds.set([]);
        return;
      }

      try {
        const user = JSON.parse(userJson) as UserProfile;
        const activeHousehold = this.getActiveHouseholdId();
        const households = user.households || [];

        this.isAuthenticated.set(true);
        this.currentUser.set(user);
        this.userHouseholds.set(households);
        this.activeHouseholdId.set(activeHousehold);
      } catch (error) {
        // Invalid stored data, clear it
        console.error('Error parsing user data:', error);
        this.clearAuthData();
      }
    }
  }

  /**
   * Get active household ID
   */
  getActiveHouseholdId(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem(this.ACTIVE_HOUSEHOLD_KEY);
    }
    return null;
  }

  /**
   * Store active household ID in localStorage
   */
  private storeActiveHousehold(householdId: string): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.setItem(this.ACTIVE_HOUSEHOLD_KEY, householdId);
    }
  }

  /**
   * Set active household (for switching between user's households)
   */
  setActiveHousehold(householdId: string): void {
    // Validate that user belongs to this household
    const households = this.userHouseholds();
    const household = households.find(h => h.householdId === householdId);

    if (!household) {
      console.error(`User does not belong to household: ${householdId}`);
      return;
    }

    // Update active household
    this.activeHouseholdId.set(householdId);
    this.storeActiveHousehold(householdId);

    // Force navigation to refresh dashboard
    // Navigate to blank route first, then to new household dashboard
    // This ensures the dashboard component reloads with new household data
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigate([`/${householdId}/dashboard`]);
    });
  }

  /**
   * Get current active household details
   */
  getActiveHousehold(): UserHousehold | null {
    const activeId = this.activeHouseholdId();
    if (!activeId) return null;

    const households = this.userHouseholds();
    return households.find(h => h.householdId === activeId) || null;
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse): Error {
    let errorMessage = 'An error occurred during authentication';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error - check for ApiResponse structure
      if (error.error?.errorMessage) {
        // Backend returned ApiResponse with errorMessage
        errorMessage = error.error.errorMessage;
      } else {
        // Fallback to status code based messages
        switch (error.status) {
          case 400:
            errorMessage = 'Invalid email or password';
            break;
          case 401:
            errorMessage = 'Invalid credentials';
            break;
          case 403:
            errorMessage = 'Access forbidden';
            break;
          case 404:
            errorMessage = 'Service not found';
            break;
          case 429:
            errorMessage = 'Too many login attempts. Please try again later';
            break;
          case 500:
            errorMessage = 'Server error. Please try again later';
            break;
          default:
            errorMessage = error.error?.message || errorMessage;
        }
      }
    }

    return new Error(errorMessage);
  }

  /**
   * Refresh access token using refresh token
   */
  refreshAccessToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http.post<LoginResponse>(`${this.API_URL}/refresh`, { refreshToken }).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this.storeTokens(response.data.accessToken, response.data.refreshToken);
        }
      }),
      catchError((error: HttpErrorResponse) => {
        this.logout();
        return throwError(() => this.handleError(error));
      })
    );
  }
}
