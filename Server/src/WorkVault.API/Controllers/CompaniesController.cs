using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WorkVault.Application.Modules.Identity.Commands.RegisterCompany;
using WorkVault.Application.Modules.Identity.Queries.GetCompanyById;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.API.Controllers;

/// <summary>
/// Handles company/tenant management operations.
/// </summary>
/// <remarks>
/// All endpoints require authentication.
/// POST (create) requires SuperAdmin role.
/// Rate limited to 200 requests/minute per authenticated user.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
public class CompaniesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a new company/tenant (SuperAdmin only).
    /// </summary>
    /// <remarks>
    /// This is for platform admins to create companies directly.
    /// Regular users should use /api/auth/register instead.
    /// </remarks>
    /// <param name="command">Company details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new company's ID.</returns>
    /// <response code="200">Company created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized (requires SuperAdmin).</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpPost]
    [Authorize(Roles = SystemRoles.SuperAdminRole)]
    public async Task<IActionResult> Register(
        RegisterCompanyCommand command,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Ok(new { id });
    }

    /// <summary>
    /// Gets a company by its ID.
    /// </summary>
    /// <param name="id">The company's ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Company details.</returns>
    /// <response code="200">Company found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Company not found or not accessible.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var company = await mediator.Send(
            new GetCompanyByIdQuery(id), cancellationToken);

        if (company is null) return NotFound();

        return Ok(company);
    }
}