using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkVault.Application.Modules.Identity.Commands.Login;
using WorkVault.Application.Modules.Identity.Commands.Register;
using WorkVault.Application.Modules.Identity.Commands.RefreshTokens;

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
}
