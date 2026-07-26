import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { roleGuard } from './core/guards/role.guard';

// Routes restricted to org managers. Employee/Manager land on the dashboard instead.
const HR_ADMIN = ['HR', 'CompanyAdmin'];

/**
 * Application routes, grouped into three shells:
 * - Public landing (`guestGuard`).
 * - Auth pages under `AuthLayout` (`guestGuard`): login, register, set/forgot/reset password.
 * - Protected pages under `MainLayout` (`authGuard`): dashboard, employees (+ new/:id),
 *   my-team, departments. Each carries a `data.title` used by the topbar.
 * Unknown paths redirect to the landing page.
 */
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
        canActivate: [roleGuard],
        data: { title: 'Employees', roles: HR_ADMIN },
        loadComponent: () =>
          import('./features/employees/employee-list/employee-list').then((m) => m.EmployeeList),
      },
      {
        path: 'employees/new',
        canActivate: [roleGuard],
        data: { title: 'Add Employee', roles: HR_ADMIN },
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
        canActivate: [roleGuard],
        data: { title: 'Departments', roles: HR_ADMIN },
        loadComponent: () =>
          import('./features/departments/department-list/department-list').then(
            (m) => m.DepartmentList,
          ),
      },
      {
        path: 'designations',
        canActivate: [roleGuard],
        data: { title: 'Designations', roles: HR_ADMIN },
        loadComponent: () =>
          import('./features/designations/designation-list/designation-list').then(
            (m) => m.DesignationList,
          ),
      },
      {
        path: 'profile',
        data: { title: 'My Profile' },
        loadComponent: () => import('./features/profile/profile').then((m) => m.Profile),
      },
      // Future: employees, departments, etc go here
    ],
  },

  {
    path: '**',
    redirectTo: '',
  },
];
