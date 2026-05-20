using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WorkVault.Application.Modules.Employees.Commands.CreateEmployee;
using WorkVault.Application.Modules.Employees.Commands.ResendInvite;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.API.Controllers;

/// <summary>
/// Handles employee management operations.
/// </summary>
/// <remarks>
/// All endpoints require authentication and HR or CompanyAdmin role.
/// Rate limited to 200 requests/minute per authenticated user.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
public class EmployeesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a new employee with user account and invite token.
    /// </summary>
    /// <remarks>
    /// Creates a User + Employee + InviteToken in one atomic transaction.
    /// The invite link is returned for HR to send to the new employee.
    /// </remarks>
    /// <param name="command">Employee details including name, email, department.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Employee ID, code, and invite link.</returns>
    /// <response code="201">Employee created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized (requires HR or CompanyAdmin).</response>
    /// <response code="409">Email already exists in this company.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = result.EmployeeId }, result);
    }
    
    /// <summary>
    /// Resends the invite to a Pending employee.
    /// </summary>
    /// <remarks>
    /// Generates a new invite token (48h expiry) and invalidates any existing
    /// active invite for the same user. Use when employee didn't receive or
    /// lost the original invite.
    /// </remarks>
    /// <param name="id">The employee's ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New invite link.</returns>
    /// <response code="200">Invite resent successfully.</response>
    /// <response code="400">Employee is not in Pending status.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized (requires HR or CompanyAdmin).</response>
    /// <response code="404">Employee not found.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpPost("{id:guid}/resend-invite")]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> ResendInvite(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ResendInviteCommand(id), cancellationToken);
        return Ok(result);
    }
}