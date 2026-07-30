using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WorkVault.Application.Modules.Identity.Commands.ForgotPassword;
using WorkVault.Application.Modules.Identity.Commands.Login;
using WorkVault.Application.Modules.Identity.Commands.Logout;
using WorkVault.Application.Modules.Identity.Commands.Register;
using WorkVault.Application.Modules.Identity.Commands.RefreshTokens;
using WorkVault.Application.Modules.Identity.Commands.ResendConfirmation;
using WorkVault.Application.Modules.Identity.Commands.ResetPassword;
using WorkVault.Application.Modules.Identity.Commands.SetPassword;
using WorkVault.Application.Modules.Identity.Queries.GetCurrentUser;
using WorkVault.Application.Modules.Identity.Queries.ValidateInvite;
using WorkVault.Application.Modules.Identity.Queries.ValidatePasswordReset;

namespace WorkVault.API.Controllers;

/// <summary>
/// Handles all authentication-related endpoints.
/// </summary>
/// <remarks>
/// Endpoints:
/// - POST /register - Register new company with admin user
/// - POST /login - Authenticate with email/password
/// - POST /refresh - Get new tokens using refresh token
/// - POST /logout - Revoke refresh token (requires auth)
/// - GET /invite/{token} - Validate invite token
/// - POST /set-password - Accept invite and set password
///
/// Rate limiting:
/// - Login/Register/SetPassword: 5 requests/minute per IP (brute force protection)
/// - Refresh: 10 requests/minute per IP
/// - Logout: 200 requests/minute per user (authenticated policy)
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new company with an admin user.
    /// </summary>
    /// <param name="command">Registration details including company info and admin credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Company ID and JWT tokens for immediate login.</returns>
    /// <response code="200">Registration successful, returns tokens.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="409">Email already exists.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="command">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT access token and refresh token.</returns>
    /// <response code="200">Login successful, returns tokens.</response>
    /// <response code="401">Invalid credentials.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command, cancellationToken);

        if (response is null)
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(response);
    }

    /// <summary>
    /// Revokes the refresh token, logging the user out from this device.
    /// Returns 204 even if the token is invalid — never leak token state.
    /// </summary>
    /// <response code="204">Logout successful (always returns success).</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Returns the authenticated user's own profile (account + employee details).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current user's profile.</returns>
    /// <response code="200">Profile returned.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet("me")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return result is null ? Unauthorized() : Ok(result);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Implements token rotation - old refresh token is revoked.
    /// </summary>
    /// <param name="command">The refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New access token and refresh token.</returns>
    /// <response code="200">Refresh successful, returns new tokens.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command, cancellationToken);

        if (response is null)
            return Unauthorized(new { message = "Invalid or expired refresh token" });

        return Ok(response);
    }

    /// <summary>
    /// Validates an invite token and returns employee info for the
    /// set-password page. Public endpoint — invitee has no JWT yet.
    /// </summary>
    /// <param name="token">The invite token GUID from the email link.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Employee info (email, name) for the set-password form.</returns>
    /// <response code="200">Token valid, returns employee info.</response>
    /// <response code="404">Token invalid, expired, or already used.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpGet("invite/{token:guid}")]
    [AllowAnonymous]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> ValidateInvite(
        Guid token,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ValidateInviteQuery(token), cancellationToken);

        if (result is null)
            return NotFound(new
            {
                type = "InvalidInvite",
                title = "Invite link is invalid, expired, or already used",
                status = 404
            });

        return Ok(result);
    }

    /// <summary>
    /// Employee accepts their invite and sets their password.
    /// On success, returns JWT tokens — they're auto-logged-in.
    /// </summary>
    /// <param name="command">Invite token and new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT tokens for immediate login.</returns>
    /// <response code="200">Password set, returns tokens.</response>
    /// <response code="400">Validation failed (password requirements).</response>
    /// <response code="404">Token invalid, expired, or already used.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpPost("set-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> SetPassword(
        [FromBody] SetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (result is null)
            return NotFound(new
            {
                type = "InvalidInvite",
                title = "Invite link is invalid, expired, or already used",
                status = 404
            });

        return Ok(result);
    }
    
    /// <summary>
    /// Resend the account-confirmation (set-password) email to a self-registered
    /// company admin who never activated. Always returns 204 — it only actually
    /// sends when the email belongs to an inactive CompanyAdmin.
    /// </summary>
    /// <response code="204">Request accepted (email sent only if applicable).</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpPost("resend-confirmation")]
    [AllowAnonymous]
    [EnableRateLimiting("forgot-password")]
    public async Task<IActionResult> ResendConfirmation(
        [FromBody] ResendConfirmationCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Request a password reset link. Always returns 200 to prevent
    /// email enumeration attacks — the response is identical whether
    /// or not the email is registered.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    /// Complete the password reset with a valid token.
    /// On success, revokes all sessions — user must log in again with the new password.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    /// Check if a reset token is valid before showing the password form.
    /// Returns user info if token is valid, 404 if expired/used/invalid.
    /// </summary>
    [HttpGet("reset-token/{token:guid}")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ValidateResetToken(
        Guid token,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ValidatePasswordResetQuery(token), cancellationToken);
        if (result is null ) return NotFound();
        return Ok(result);
    }
}
