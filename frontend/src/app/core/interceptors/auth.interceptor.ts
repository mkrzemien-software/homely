import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError, switchMap } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * HTTP Interceptor that adds JWT token to all requests and handles token expiration
 *
 * This interceptor:
 * - Automatically adds the Authorization header with Bearer token to all outgoing HTTP requests
 * - Handles 401 (Unauthorized) errors by attempting to refresh the token
 * - Logs out the user if token refresh fails or token has expired
 *
 * @example
 * // Registered in app.config.ts
 * provideHttpClient(
 *   withInterceptors([authInterceptor])
 * )
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  // If token exists, clone the request and add Authorization header
  let clonedRequest = req;
  if (token) {
    clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(clonedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized errors (token expired or invalid)
      if (error.status === 401) {
        // Skip token refresh for login and refresh endpoints to avoid infinite loops
        const isAuthEndpoint = req.url.includes('/auth/login') ||
                               req.url.includes('/auth/refresh');

        if (!isAuthEndpoint && authService.getRefreshToken()) {
          // Attempt to refresh the token
          return authService.refreshAccessToken().pipe(
            switchMap(() => {
              // Retry the original request with the new token
              const newToken = authService.getToken();
              const retryRequest = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${newToken}`
                }
              });
              return next(retryRequest);
            }),
            catchError((refreshError) => {
              // Refresh failed - token has expired, log out the user
              console.error('Token refresh failed, logging out user');
              authService.logout();
              return throwError(() => refreshError);
            })
          );
        } else {
          // No refresh token available or auth endpoint failed, log out the user
          console.error('Token expired or invalid, logging out user');
          authService.logout();
        }
      }

      // For other errors, just pass them through
      return throwError(() => error);
    })
  );
};
