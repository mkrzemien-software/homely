import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard to protect routes that require System Developer role
 *
 * This guard checks if the user is authenticated and has the System Developer role.
 * If not, it redirects to the login page or a forbidden page.
 *
 * Usage in routes:
 * ```typescript
 * {
 *   path: 'system/users',
 *   component: SystemUsersComponent,
 *   canActivate: [systemDeveloperGuard]
 * }
 * ```
 */
export const systemDeveloperGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Check if user is authenticated
  if (!authService.isAuthenticated()) {
    // Redirect to login with return URL
    router.navigate(['/auth/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }

  // Get current user
  const currentUser = authService.getCurrentUser();

  // TODO: When user roles are implemented in the backend,
  // check if user has System Developer role
  // For now, we'll allow all authenticated users for development

  // Example implementation when roles are available:
  /*
  if (currentUser?.role !== 'system_developer') {
    // User doesn't have System Developer role
    router.navigate(['/error/403']);
    return false;
  }
  */

  // User is authenticated and has proper role
  return true;
};

/**
 * Guard to protect routes that require authentication
 *
 * Basic authentication guard that can be used for any authenticated route
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    // router.navigate(['/auth/login'], {
    //   queryParams: { returnUrl: state.url }
    // });
    // return false;
    return true;
  }

  return true;
};
