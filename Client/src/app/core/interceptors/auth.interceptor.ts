import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * Attaches the JWT access token to every outgoing HTTP request as a Bearer token.
 * Runs automatically before each request leaves the app.
 * 
 * Skipped for auth endpoints (login, register, refresh) — they don't need a token,
 * and attaching an expired one would cause weird errors.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  // Don't attach token to auth endpoints — they either issue new tokens
  // or don't need one at all
  const isAuthEndpoint = req.url.includes('/auth/login')
    || req.url.includes('/auth/register')
    || req.url.includes('/auth/refresh');

  if (!token || isAuthEndpoint) {
    return next(req);
  }

  // Clone the request and add the Authorization header.
  // We clone because HttpRequest is immutable — you can't modify it directly.
  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authReq);
};