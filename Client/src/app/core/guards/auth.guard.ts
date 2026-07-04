import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Route guard — blocks access to protected pages when the user isn't logged in.
 * 
 * Angular calls this function before the route activates. Return true to allow,
 * return false (or a redirect) to block.
 * 
 * Applied to routes in app.routes.ts via: canActivate: [authGuard]
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // Not logged in — redirect to login
  router.navigate(['/login']);
  return false;
};