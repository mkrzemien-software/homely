import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
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
    path: 'dashboard',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
    title: 'Dashboard - Homely'
    // TODO: Create DashboardComponent and add auth guard
  },
  {
    path: '**',
    redirectTo: '/auth/login'
  }
];
