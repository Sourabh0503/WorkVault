import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Route guard — restricts a route to the roles listed in `route.data['roles']`.
 * Runs after `authGuard` (checks role only, not authentication). Users whose role
 * isn't allowed are redirected to the dashboard (reachable by everyone), so e.g. a
 * regular Employee can't land on the HR-only Employees list (which 403s server-side).
 *
 * Usage: `canActivate: [authGuard, roleGuard], data: { roles: ['HR', 'CompanyAdmin'] }`
 */
export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const allowed = (route.data?.['roles'] as string[] | undefined) ?? [];
  if (allowed.length === 0 || allowed.includes(authService.role())) {
    return true;
  }

  router.navigate(['/dashboard']);
  return false;
};
