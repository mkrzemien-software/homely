import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'auth',
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
        title: 'Login - Homely'
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
        title: 'Register - Homely'
        // TODO: Create RegisterComponent
      },
      {
        path: 'forgot-password',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
        title: 'Forgot Password - Homely'
        // TODO: Create ForgotPasswordComponent
      },
      {
        path: 'reset-password/:token',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
        title: 'Reset Password - Homely'
        // TODO: Create ResetPasswordComponent
      }
    ]
  },
  {
    path: 'system',
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/system/dashboard/system-dashboard.component').then(m => m.SystemDashboardComponent),
        title: 'System Dashboard - Homely',
        // TODO: Uncomment when authentication is fully implemented
        // canActivate: [systemDeveloperGuard]
      },
      {
        path: 'users',
        loadComponent: () => import('./features/system/users/system-users.component').then(m => m.SystemUsersComponent),
        title: 'User Management - Homely System',
        // TODO: Uncomment when authentication is fully implemented
        // canActivate: [systemDeveloperGuard]
      },
      {
        path: 'households',
        loadComponent: () => import('./features/system/households/system-households.component').then(m => m.SystemHouseholdsComponent),
        title: 'Household Management - Homely System',
        // TODO: Uncomment when authentication is fully implemented
        // canActivate: [systemDeveloperGuard]
      }
    ]
  },
  {
    path: ':householdId',
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/household/dashboard/household-dashboard.component').then(m => m.HouseholdDashboardComponent),
        title: 'Dashboard - Homely',
        // TODO: Uncomment when authentication is fully implemented
        // canActivate: [authGuard, householdMemberGuard]
      },
      {
        path: 'tasks',
        redirectTo: 'dashboard', // TODO: Create TasksComponent
        pathMatch: 'full'
      },
      {
        path: 'categories',
        loadComponent: () => import('./features/household/items/items-list.component').then(m => m.ItemsListComponent),
        title: 'Urządzenia i Wizyty - Homely',
        // TODO: Uncomment when authentication is fully implemented
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
  }
];
