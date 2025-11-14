import { Injectable, signal, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, tap, catchError } from 'rxjs';
import { Router } from '@angular/router';

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
 * User data from API
 */
export interface UserDto {
  id: string;
  email: string;
  name: string;
  emailConfirmed: boolean;
  createdAt: string;
  householdId: string;
  role: string;
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
  householdId: string;
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = 'http://localhost:5000/api/auth'; // Backend API base URL
  private readonly TOKEN_KEY = 'homely-auth-token';
  private readonly REFRESH_TOKEN_KEY = 'homely-refresh-token';
  private readonly USER_KEY = 'homely-user';

  private http = inject(HttpClient);
  private router = inject(Router);

  // Signals for reactive state management
  isAuthenticated = signal<boolean>(false);
  currentUser = signal<UserProfile | null>(null);
  isLoading = signal<boolean>(false);

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

        // Store tokens
        this.storeTokens(response.data.accessToken, response.data.refreshToken);

        // Store user data
        this.storeUser(response.data.user);

        // Update authentication state
        this.isAuthenticated.set(true);
        this.currentUser.set(response.data.user);
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
      try {
        const user = JSON.parse(userJson) as UserProfile;
        this.isAuthenticated.set(true);
        this.currentUser.set(user);
      } catch (error) {
        // Invalid stored data, clear it
        this.clearAuthData();
      }
    }
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
