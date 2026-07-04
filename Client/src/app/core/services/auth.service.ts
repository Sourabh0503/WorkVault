import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LoginRequest,
  LoginResponse,
  RefreshTokenResponse,
  RegisterRequest,
  RegisterResponse
} from '../models/auth.models';

// Storage keys — using constants prevents typos across the codebase
const STORAGE_ACCESS = 'wv_access_token';
const STORAGE_REFRESH = 'wv_refresh_token';
const STORAGE_USER = 'wv_user';

interface StoredUser {
  userId: string;
  companyId: string;
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

  // Derived: is the user logged in?
  isAuthenticated = computed(() => this._user() !== null);

  private apiUrl = environment.apiUrl;

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
      .pipe(tap(response => this.persistSession(response)));
  }

  /**
   * POST /api/auth/register
   */
  register(data: RegisterRequest): Observable<RegisterResponse> {
    return this.http
      .post<RegisterResponse>(`${this.apiUrl}/auth/register`, data)
      .pipe(
        tap(response => {
          // Register returns companyId + tokens but no userId.
          // We don't know the userId until the /me endpoint is added.
          // For now, store what we have.
          this.setTokens(response.accessToken, response.refreshToken);
          const user: StoredUser = { userId: '', companyId: response.companyId };
          localStorage.setItem(STORAGE_USER, JSON.stringify(user));
          this._user.set(user);
        })
      );
  }

  /**
   * POST /api/auth/refresh
   * Called by the HTTP interceptor when the access token expires.
   */
  refresh(): Observable<RefreshTokenResponse> {
    const refreshToken = this.getRefreshToken();
    return this.http
      .post<RefreshTokenResponse>(`${this.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(
        tap(response => {
          this.setTokens(response.accessToken, response.refreshToken);
        })
      );
  }

  /**
   * POST /api/auth/logout
   * Revokes the refresh token on the server, then clears local state.
   */
  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      // Fire-and-forget — even if it fails, we still clear local state
      this.http
        .post(`${this.apiUrl}/auth/logout`, { refreshToken })
        .subscribe({ error: () => {} });
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
      companyId: response.companyId
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
}