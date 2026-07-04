import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/landing/landing').then((m) => m.Landing),
    canActivate: [guestGuard],
  },

  // Auth pages — wrapped by AuthLayout
  {
    path: '',
    canActivate: [guestGuard],
    loadComponent: () => import('./layouts/auth-layout/auth-layout').then((m) => m.AuthLayout),
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
      },
      {
        path: 'set-password',
        loadComponent: () =>
          import('./features/auth/set-password/set-password').then((m) => m.SetPassword),
      },
    ],
  },

  // Protected pages (require login)
  // Protected pages — wrapped by MainLayout
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layouts/main-layout/main-layout').then((m) => m.MainLayout),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
      },
      // Future: employees, departments, etc go here
    ],
  },

  {
    path: '**',
    redirectTo: '',
  },
];
