using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkVault.Application.Modules.Identity.Commands.Login;
using WorkVault.Application.Modules.Identity.Commands.Logout;
using WorkVault.Application.Modules.Identity.Commands.Register;
using WorkVault.Application.Modules.Identity.Commands.RefreshTokens;
using WorkVault.Application.Modules.Identity.Commands.SetPassword;
using WorkVault.Application.Modules.Identity.Queries.ValidateInvite;

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
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
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
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("refresh")]
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
    [HttpGet("invite/{token:guid}")]
    [AllowAnonymous]
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
    [HttpPost("set-password")]
    [AllowAnonymous]
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
}
