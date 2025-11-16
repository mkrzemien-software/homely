import { Routes } from '@angular/router';
import { authGuard, householdMemberGuard, systemDeveloperGuard, guestOnlyGuard } from './core/guards/system-developer.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/auth/login',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    children: [
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
      },
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
        title: 'Login - Homely',
        // canActivate: [guestOnlyGuard]
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
        title: 'Register - Homely',
        // canActivate: [guestOnlyGuard]
        // TODO: Create RegisterComponent
      },
      {
        path: 'forgot-password',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
        title: 'Forgot Password - Homely',
        // canActivate: [guestOnlyGuard]
        // TODO: Create ForgotPasswordComponent
      },
      {
        path: 'reset-password/:token',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
        title: 'Reset Password - Homely',
        // canActivate: [guestOnlyGuard]
        // TODO: Create ResetPasswordComponent
      }
    ]
  },
  {
    path: 'system',
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/system/dashboard/system-dashboard.component').then(m => m.SystemDashboardComponent),
        title: 'System Dashboard - Homely',
        // canActivate: [systemDeveloperGuard]
      },
      {
        path: 'users',
        loadComponent: () => import('./features/system/users/system-users.component').then(m => m.SystemUsersComponent),
        title: 'User Management - Homely System',
        // canActivate: [systemDeveloperGuard]
      },
      {
        path: 'households',
        loadComponent: () => import('./features/system/households/system-households.component').then(m => m.SystemHouseholdsComponent),
        title: 'Household Management - Homely System',
        // canActivate: [systemDeveloperGuard]
      }
    ]
  },
  {
    path: ':householdId',
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/household/dashboard/household-dashboard.component').then(m => m.HouseholdDashboardComponent),
        title: 'Dashboard - Homely',
        // canActivate: [authGuard, householdMemberGuard]
      },
      {
        path: 'tasks',
        loadComponent: () => import('./features/household/tasks/tasks-list.component').then(m => m.TasksListComponent),
        title: 'Zadania - Homely',
        // canActivate: [authGuard, householdMemberGuard]
      },
      {
        path: 'events',
        loadComponent: () => import('./features/household/events/events-view.component').then(m => m.EventsViewComponent),
        title: 'Wydarzenia - Homely',
        // canActivate: [authGuard, householdMemberGuard]
      },
      {
        path: 'calendar',
        loadComponent: () => import('./features/household/calendar/month-calendar-view.component').then(m => m.MonthCalendarViewComponent),
        title: 'Kalendarz - Homely',
        // canActivate: [authGuard, householdMemberGuard]
      },
      {
        path: 'categories',
        loadComponent: () => import('./features/household/items/categories-list.component').then(m => m.CategoriesListComponent),
        title: 'Kategorie - Homely',
        // canActivate: [authGuard, householdMemberGuard]
      },
      {
        path: 'settings',
        redirectTo: 'dashboard', // TODO: Create SettingsComponent
        pathMatch: 'full'
      },
      {
        path: 'profile',
        redirectTo: 'dashboard', // TODO: Create ProfileComponent (for member role)
        pathMatch: 'full'
      }
    ]
  },
  {
    path: 'error',
    children: [
      {
        path: '403',
        loadComponent: () => import('./features/error/forbidden/forbidden.component').then(m => m.ForbiddenComponent),
        title: '403 - Brak dostępu'
      },
      {
        path: '404',
        loadComponent: () => import('./features/error/not-found/not-found.component').then(m => m.NotFoundComponent),
        title: '404 - Nie znaleziono'
      }
    ]
  },
  {
    path: '**',
    redirectTo: '/error/404'
  }
];
