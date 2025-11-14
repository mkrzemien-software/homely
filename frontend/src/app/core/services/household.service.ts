import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, catchError, of, map } from 'rxjs';

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
 * Household data from API
 */
export interface HouseholdDto {
  id: string;
  name: string;
  address?: string;
  planTypeId: number;
  planTypeName: string;
  subscriptionStatus: string;
  subscriptionStartDate?: string;
  subscriptionEndDate?: string;
  memberCount: number;
  createdAt: string;
}

/**
 * Household response (wrapped in ApiResponse)
 */
export interface HouseholdResponse extends ApiResponse<HouseholdDto> {}

/**
 * Households list response (wrapped in ApiResponse)
 */
export interface HouseholdsResponse extends ApiResponse<HouseholdDto[]> {}

/**
 * Household member data
 */
export interface HouseholdMember {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: 'admin' | 'member' | 'dashboard';
}

@Injectable({
  providedIn: 'root'
})
export class HouseholdService {
  private readonly API_URL = 'http://localhost:5000/api/households'; // Backend API base URL

  private http = inject(HttpClient);

  /**
   * Get household by ID
   */
  getById(householdId: string): Observable<HouseholdResponse> {
    return this.http.get<HouseholdResponse>(`${this.API_URL}/${householdId}`).pipe(
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Get all households for the current user
   */
  getMyHouseholds(): Observable<HouseholdsResponse> {
    return this.http.get<HouseholdsResponse>(`${this.API_URL}/my`).pipe(
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.handleError(error));
      })
    );
  }

  /**
   * Get household members
   */
  getHouseholdMembers(householdId: string): Observable<HouseholdMember[]> {
    return this.http.get<ApiResponse<HouseholdMember[]>>(`${this.API_URL}/${householdId}/members`).pipe(
      map(response => response.data || []),
      catchError((error: HttpErrorResponse) => {
        console.error('Error loading household members:', error);
        // Return empty array on error to prevent application crash
        return of([]);
      })
    );
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse): Error {
    let errorMessage = 'An error occurred while fetching household data';

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
            errorMessage = 'Invalid request';
            break;
          case 401:
            errorMessage = 'Unauthorized';
            break;
          case 403:
            errorMessage = 'Access forbidden';
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
    }

    return new Error(errorMessage);
  }
}
