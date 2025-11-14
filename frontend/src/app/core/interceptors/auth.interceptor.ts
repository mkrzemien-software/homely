import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * HTTP Interceptor that adds JWT token to all requests
 *
 * This interceptor automatically adds the Authorization header with Bearer token
 * to all outgoing HTTP requests if a token is available in AuthService.
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
  if (token) {
    const clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(clonedRequest);
  }

  // If no token, proceed with original request
  return next(req);
};
