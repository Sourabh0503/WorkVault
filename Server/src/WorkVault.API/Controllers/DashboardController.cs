using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WorkVault.Application.Modules.Dashboard.Queries.GetDashboardStats;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.API.Controllers;

/// <summary>
/// Dashboard aggregates for the current company.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
public class DashboardController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Company-wide stats: employee/department counts, pending invites.
    /// HR and CompanyAdmin only.
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var stats = await mediator.Send(new GetDashboardStatsQuery(), cancellationToken);
        return Ok(stats);
    }
}