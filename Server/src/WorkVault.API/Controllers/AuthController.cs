using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkVault.Application.Modules.Identity.Commands.Login;
using WorkVault.Application.Modules.Identity.Commands.Register;
using WorkVault.Application.Modules.Identity.Commands.RefreshTokens;
using WorkVault.Application.Modules.Identity.Queries.ValidateInvite;

namespace WorkVault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
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
                title = "Invite link is invalid or expired",
                status = 404
            });

        return Ok(result);
    }
}
