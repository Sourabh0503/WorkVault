// =============================================================================
// Auth API types — must match the C# DTOs in the backend
// =============================================================================
// If the backend changes a shape, update here too. TypeScript will then show
// compile errors everywhere that field is used — safer than runtime crashes.

/**
 * POST /api/auth/login — request body
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * POST /api/auth/login — successful response
 * Matches LoginResponse from LoginHandler.cs
 */
export interface LoginResponse {
  userId: string;
  companyId: string;
  accessToken: string;
  refreshToken: string;
}

/**
 * POST /api/auth/register — request body
 * Matches RegisterCommand.cs
 */
export interface RegisterRequest {
  companyName: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  domain?: string;
  industry?: string;
  gstNumber?: string;
}

/**
 * POST /api/auth/register — successful response
 */
export interface RegisterResponse {
  companyId: string;
  userId: string;
  accessToken: string;
  refreshToken: string;
}

/**
 * POST /api/auth/refresh — request body
 */
export interface RefreshTokenRequest {
  refreshToken: string;
}

/**
 * POST /api/auth/refresh — successful response
 */
export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
}

/**
 * Standard error shape from ExceptionHandlingMiddleware.
 * Different exception types produce slightly different shapes but always have these fields.
 */
export interface ApiError {
  type: string;
  title: string;
  status: number;
  errors?: Record<string, string[]>; // present for ValidationFailure
}

/**
 * GET /api/auth/invite/{token} — success response
 * Matches ValidateInviteResult from ValidateInviteHandler.cs
 */
export interface InviteInfo {
  email: string;
  firstName: string;
  lastName: string;
  companyName: string;
}

/**
 * POST /api/auth/set-password — request body
 */
export interface SetPasswordRequest {
  token: string;
  password: string;
}

/**
 * POST /api/auth/set-password — success response
 * Matches SetPasswordResult from SetPasswordHandler.cs
 * (Same shape as LoginResponse — user is auto-logged-in)
 */
export interface SetPasswordResponse {
  userId: string;
  companyId: string;
  accessToken: string;
  refreshToken: string;
}

/**
 * POST /api/auth/forgot-password — request body
 */
export interface ForgotPasswordRequest {
  email: string;
}

/**
 * POST /api/auth/reset-password — request body
 */
export interface ResetPasswordRequest {
  token: string;
  password: string;
}

/**
 * GET /api/auth/reset-token/{token} — success response
 * Matches ValidatePasswordResetResult from ValidatePasswordResetHandler.cs
 */
export interface ResetTokenInfo {
  email: string;
  firstName: string;
  companyName: string;
}