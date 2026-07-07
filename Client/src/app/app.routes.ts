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
      {
        path: 'forgot-password',
        loadComponent: () =>
          import('./features/auth/forgot-password/forgot-password').then((m) => m.ForgotPassword),
      },
      {
        path: 'reset-password',
        loadComponent: () =>
          import('./features/auth/reset-password/reset-password').then((m) => m.ResetPassword),
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
        data: { title: 'Dashboard' },
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'employees',
        data: { title: 'Employees' },
        loadComponent: () =>
          import('./features/employees/employee-list/employee-list').then((m) => m.EmployeeList),
      },
      {
        path: 'employees/new',
        data: { title: 'Add Employee' },
        loadComponent: () =>
          import('./features/employees/employee-create/employee-create').then(
            (m) => m.EmployeeCreate,
          ),
      },
      {
        path: 'employees/:id',
        data: { title: 'Employee' },
        loadComponent: () =>
          import('./features/employees/employee-detail-page/employee-detail-page').then(
            (m) => m.EmployeeDetailPage,
          ),
      },
      {
        path: 'my-team',
        data: { title: 'My Team' },
        loadComponent: () => import('./features/employees/my-team/my-team').then((m) => m.MyTeam),
      },
      {
        path: 'departments',
        data: { title: 'Departments' },
        loadComponent: () =>
          import('./features/departments/department-list/department-list').then(
            (m) => m.DepartmentList,
          ),
      },
      // Future: employees, departments, etc go here
    ],
  },

  {
    path: '**',
    redirectTo: '',
  },
];
