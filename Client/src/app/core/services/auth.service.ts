import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { BehaviorSubject, finalize, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  InviteInfo,
  LoginRequest,
  LoginResponse,
  RefreshTokenResponse,
  RegisterRequest,
  RegisterResponse,
  ResetPasswordRequest,
  ResetTokenInfo,
  SetPasswordRequest,
  SetPasswordResponse,
} from '../models/auth.models';
import { CurrentUserProfile } from '../models/profile.models';

// Shape of the claims inside the access token.
// The role claim uses the full .NET claim URI.
interface JwtPayload {
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string;
  CompanyId: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': string;
}

// Storage keys — using constants prevents typos across the codebase
const STORAGE_ACCESS = 'wv_access_token';
const STORAGE_REFRESH = 'wv_refresh_token';
const STORAGE_USER = 'wv_user';

interface StoredUser {
  userId: string;
  companyId: string;
  role: string;
  // Human-readable tenant name, supplied by login/register/set-password.
  companyName?: string;
  // The user's first name, for greetings/sidebar/avatar. Sourced from /me.
  firstName?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  // ---- Reactive state (Angular Signals) ----
  // Any component that reads these will re-render when the values change.
  private _user = signal<StoredUser | null>(this.loadUserFromStorage());

  // Public read-only view of the user
  user = this._user.asReadonly();

  // Current user's role — used for showing/hiding UI by permission
  role = computed(() => this._user()?.role ?? '');

  // Human-readable tenant name for branding — seeded by login, refreshed from /me.
  companyName = computed(() => this._user()?.companyName ?? '');

  // The user's first name — sourced from /me (kept fresh across HR edits), '' until loaded.
  firstName = computed(() => this._user()?.firstName ?? '');

  // Full current-user profile from /me — populated by loadProfile(), shared so pages don't
  // each re-fetch /me. Null until the first load resolves.
  private _profile = signal<CurrentUserProfile | null>(null);
  profile = this._profile.asReadonly();

  // Derived: is the user logged in?
  isAuthenticated = computed(() => this._user() !== null);

  private apiUrl = environment.apiUrl;

  // Tracks in-progress refresh so multiple 401s share one refresh call
  private refreshInProgress$ = new BehaviorSubject<boolean>(false);
  isRefreshing = () => this.refreshInProgress$.value;
  refreshComplete$ = this.refreshInProgress$.asObservable();

  // =========================================================================
  // API calls
  // =========================================================================

  /**
   * POST /api/auth/login
   * On success, stores tokens and updates the user signal.
   */
  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(tap((response) => this.persistSession(response)));
  }

  /** GET /api/auth/me — the signed-in user's full profile (account + employee details). */
  getMe(): Observable<CurrentUserProfile> {
    return this.http.get<CurrentUserProfile>(`${this.apiUrl}/auth/me`);
  }

  /**
   * Refreshes the cached display profile (first name, company name) from /me.
   * Called on app startup and after auth, so these fields reflect the source of
   * truth rather than a stale login snapshot. No-op when not authenticated.
   */
  loadProfile(): void {
    if (!this.getAccessToken()) return;
    this.getMe().subscribe({
      next: (profile) => {
        this._profile.set(profile);
        const current = this._user();
        if (!current) return;
        const updated: StoredUser = {
          ...current,
          firstName: profile.firstName,
          companyName: profile.companyName,
        };
        localStorage.setItem(STORAGE_USER, JSON.stringify(updated));
        this._user.set(updated);
      },
      error: () => {}, // interceptor handles 401; a transient failure just keeps cached values
    });
  }

  /**
   * POST /api/auth/register
   */
  register(data: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/auth/register`, data).pipe(
      tap((response) => {
        // Register returns companyId + tokens but no userId.
        // We don't know the userId until the /me endpoint is added.
        // For now, store what we have.
        this.setTokens(response.accessToken, response.refreshToken);
        const user: StoredUser = {
          userId: response.userId,
          companyId: response.companyId,
          role: this.extractRole(response.accessToken),
          // The register response omits companyName, but we have it from the
          // request the user just submitted.
          companyName: data.companyName,
        };
        localStorage.setItem(STORAGE_USER, JSON.stringify(user));
        this._user.set(user);
      }),
    );
  }

  /**
   * POST /api/auth/refresh
   * Called by the HTTP interceptor when the access token expires.
   */
  refresh(): Observable<RefreshTokenResponse> {
    this.refreshInProgress$.next(true);
    const refreshToken = this.getRefreshToken();
    return this.http
      .post<RefreshTokenResponse>(`${this.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(
        tap((response) => {
          this.setTokens(response.accessToken, response.refreshToken);
        }),
        finalize(() => {
          this.refreshInProgress$.next(false);
        })
      );
  }

  /**
   * GET /api/auth/invite/{token}
   * Validates the invite before showing the set-password form.
   * Returns null via 404 if the token is invalid/expired/used.
   */
  validateInvite(token: string): Observable<InviteInfo> {
    return this.http.get<InviteInfo>(`${this.apiUrl}/auth/invite/${token}`);
  }

  /**
   * GET /api/auth/reset-token/{token}
   * Checks if a reset token is valid before showing the password form.
   * Returns user info on valid, 404 on invalid/expired/used.
   */
  validateResetToken(token: string): Observable<ResetTokenInfo> {
    return this.http.get<ResetTokenInfo>(`${this.apiUrl}/auth/reset-token/${token}`);
  }
  /**
   * POST /api/auth/set-password
   * Sets the password, activates the user, and auto-logs them in.
   */
  setPassword(data: SetPasswordRequest): Observable<SetPasswordResponse> {
    return this.http
      .post<SetPasswordResponse>(`${this.apiUrl}/auth/set-password`, data)
      .pipe(tap((response) => this.persistSession(response)));
  }

  /**
   * POST /api/auth/forgot-password
   * Requests a password reset link. Always returns 200 regardless of
   * whether the email is registered — the frontend can't tell either way.
   */
  forgotPassword(email: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/auth/forgot-password`, { email });
  }

  /**
   * POST /api/auth/reset-password
   * Completes the reset with a valid token. Does NOT auto-login —
   * user must log in again with the new password.
   */
  resetPassword(data: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/auth/reset-password`, data);
  }

  /**
   * POST /api/auth/logout
   * Revokes the refresh token on the server, then clears local state.
   */
  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      // Fire-and-forget — even if it fails, we still clear local state
      this.http.post(`${this.apiUrl}/auth/logout`, { refreshToken }).subscribe({ error: () => {} });
    }
    this.clearSession();
    this.router.navigate(['/login']);
  }

  // =========================================================================
  // Token accessors — used by the interceptor
  // =========================================================================

  getAccessToken(): string | null {
    return localStorage.getItem(STORAGE_ACCESS);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(STORAGE_REFRESH);
  }

  // =========================================================================
  // Internal helpers
  // =========================================================================

  private persistSession(response: LoginResponse): void {
    this.setTokens(response.accessToken, response.refreshToken);
    const user: StoredUser = {
      userId: response.userId,
      companyId: response.companyId,
      role: this.extractRole(response.accessToken),
      // companyName + firstName are populated by loadProfile() from /auth/me
      // (the source of truth) once the authenticated shell loads.
    };
    localStorage.setItem(STORAGE_USER, JSON.stringify(user));
    this._user.set(user);
  }

  private setTokens(access: string, refresh: string): void {
    localStorage.setItem(STORAGE_ACCESS, access);
    localStorage.setItem(STORAGE_REFRESH, refresh);
  }

  private clearSession(): void {
    localStorage.removeItem(STORAGE_ACCESS);
    localStorage.removeItem(STORAGE_REFRESH);
    localStorage.removeItem(STORAGE_USER);
    this._user.set(null);
    this._profile.set(null);
  }

  private loadUserFromStorage(): StoredUser | null {
    const raw = localStorage.getItem(STORAGE_USER);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as StoredUser;
    } catch {
      return null;
    }
  }

  private extractRole(accessToken: string): string {
    try {
      const payload = jwtDecode<JwtPayload>(accessToken);
      return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? '';
    } catch {
      return '';
    }
  }
}
