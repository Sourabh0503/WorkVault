import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, filter, Observable, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Attaches the JWT to every outgoing request AND handles 401 responses
 * by refreshing the token silently and retrying the request.
 *
 * User never notices tokens expiring — as long as their refresh token
 * is still valid (7 days), they stay logged in.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isAuthEndpoint =
    req.url.includes('/auth/login') ||
    req.url.includes('/auth/register') ||
    req.url.includes('/auth/refresh') ||
    req.url.includes('/auth/forgot-password') ||
    req.url.includes('/auth/reset-password') ||
    req.url.includes('/auth/set-password') ||
    req.url.includes('/auth/reset-token') ||
    req.url.includes('/auth/invite');

  // Attach token if we have one and this isn't an auth endpoint
  const authReq = attachToken(req, authService, isAuthEndpoint);

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only handle 401 on non-auth endpoints. Auth endpoints (login etc.)
      // returning 401 are just failed credentials — bubble that up normally.
      if (error.status !== 401 || isAuthEndpoint) {
        return throwError(() => error);
      }

      // 401 on a normal API call → try to refresh, then retry
      return handleRefresh(req, next, authService, router);
    }),
  );
};

/**
 * Clones the request and adds the Bearer token if applicable.
 */
function attachToken(
  req: HttpRequest<unknown>,
  authService: AuthService,
  isAuthEndpoint: boolean,
): HttpRequest<unknown> {
  const token = authService.getAccessToken();

  if (!token || isAuthEndpoint) {
    return req;
  }

  return req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });
}

/**
 * Refresh the token and retry the original request.
 * If a refresh is already in progress, wait for it to finish
 * instead of firing a duplicate refresh.
 */
function handleRefresh(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router,
) : Observable<HttpEvent<unknown>> {
  // Another request already started refreshing → wait for it to finish
  if (authService.isRefreshing()) {
    return authService.refreshComplete$.pipe(
      filter((inProgress) => !inProgress), // wait until refresh is done
      take(1), // only take the first "done" signal
      switchMap(() => next(attachToken(req, authService, false))),
    );
  }

  // We're the first — perform the refresh
  return authService.refresh().pipe(
    switchMap(() => next(attachToken(req, authService, false))),
    catchError((err) => {
      // Refresh failed too — session is fully dead. Log the user out.
      authService.logout();
      router.navigate(['/login']);
      return throwError(() => err);
    }),
  );
}
