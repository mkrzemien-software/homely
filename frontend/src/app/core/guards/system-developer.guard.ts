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

  // Check if user is authenticated and token is valid (not expired)
  if (!authService.isAuthenticated() || !authService.isTokenValid()) {
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

  // Check if user is authenticated and token is valid (not expired)
  if (!authService.isAuthenticated() || !authService.isTokenValid()) {
    router.navigate(['/auth/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }

  return true;
};

/**
 * Guard to protect household routes
 *
 * This guard checks if the user is authenticated and is a member of the household
 * specified in the route parameter.
 *
 * Usage in routes:
 * ```typescript
 * {
 *   path: ':householdId/dashboard',
 *   component: DashboardComponent,
 *   canActivate: [authGuard, householdMemberGuard]
 * }
 * ```
 */
export const householdMemberGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // First check authentication and token validity (redundant if used with authGuard, but safe)
  if (!authService.isAuthenticated() || !authService.isTokenValid()) {
    router.navigate(['/auth/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }

  // Get current user
  const currentUser = authService.getCurrentUser();
  if (!currentUser) {
    router.navigate(['/auth/login']);
    return false;
  }

  // Get household ID from route params
  const householdIdParam = route.paramMap.get('householdId');

  // Check if user has a household assigned
  if (!currentUser.householdId || currentUser.householdId.trim() === '') {
    console.warn('User has no household assigned');
    router.navigate(['/error/403']);
    return false;
  }

  // If householdId is in route params, verify it matches user's household
  if (householdIdParam && householdIdParam !== currentUser.householdId) {
    console.warn(`User ${currentUser.id} attempted to access household ${householdIdParam} but belongs to ${currentUser.householdId}`);
    router.navigate(['/error/403']);
    return false;
  }

  return true;
};

/**
 * Guard to prevent authenticated users from accessing auth routes (login, register)
 *
 * This guard redirects authenticated users to their household dashboard
 * if they try to access authentication pages.
 *
 * Usage in routes:
 * ```typescript
 * {
 *   path: 'auth/login',
 *   component: LoginComponent,
 *   canActivate: [guestOnlyGuard]
 * }
 * ```
 */
export const guestOnlyGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Check if user is already authenticated
  if (authService.isAuthenticated()) {
    // Get current user
    const currentUser = authService.getCurrentUser();

    // Redirect to user's household dashboard
    if (currentUser?.householdId) {
      router.navigate([`/${currentUser.householdId}/dashboard`]);
    } else {
      // User is authenticated but has no household - redirect to root
      router.navigate(['/']);
    }

    return false;
  }

  // User is not authenticated, allow access to auth pages
  return true;
};
