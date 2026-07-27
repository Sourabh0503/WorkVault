using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WorkVault.Application.Modules.Employees.Commands.CreateEmployee;
using WorkVault.Application.Modules.Employees.Commands.DeleteEmployee;
using WorkVault.Application.Modules.Employees.Commands.ResendInvite;
using WorkVault.Application.Modules.Employees.Queries.GetDepartmentMembers;
using WorkVault.Application.Modules.Employees.Queries.GetEmployeeById;
using WorkVault.Application.Modules.Employees.Commands.UpdateEmployee;
using WorkVault.Application.Modules.Employees.Queries.GetEmployees;
using WorkVault.Application.Modules.Employees.Queries.GetMyTeam;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.API.Controllers;

/// <summary>
/// Handles employee management operations.
/// </summary>
/// <remarks>
/// Most endpoints require HR or CompanyAdmin role.
/// The my-team endpoint is available to any authenticated user
/// (scoped to their own department).
/// Rate limited to 200 requests/minute per authenticated user.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
public class EmployeesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets a paginated list of employees with optional filters.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based). Default: 1</param>
    /// <param name="pageSize">Items per page. Default: 20, Max: 100</param>
    /// <param name="departmentId">Filter by department ID.</param>
    /// <param name="status">Filter by status (0=Pending, 1=Active, 2=OnNotice, 3=Suspended, 4=Offboarded).</param>
    /// <param name="managerId">Filter by manager ID (get direct reports).</param>
    /// <param name="search">Search by name, email, or employee code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of employees.</returns>
    /// <response code="200">List of employees.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpGet]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] EmployeeStatus? status = null,
        [FromQuery] Guid? managerId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmployeesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            DepartmentId = departmentId,
            Status = status,
            ManagerId = managerId,
            Search = search
        };
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets an employee by ID.
    /// </summary>
    /// <param name="id">The employee's ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Employee details.</returns>
    /// <response code="200">Employee found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Employee not found.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Updates an existing employee's details.
    /// </summary>
    /// <remarks>
    /// Updates employee personal info, organizational placement, and lifecycle status.
    /// Email cannot be changed through this endpoint.
    /// </remarks>
    /// <param name="id">The employee's ID.</param>
    /// <param name="command">Updated employee details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated employee ID and code.</returns>
    /// <response code="200">Employee updated successfully.</response>
    /// <response code="400">Validation failed or ID mismatch.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized (requires HR or CompanyAdmin).</response>
    /// <response code="404">Employee not found.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("URL ID does not match command ID.");

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

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
    
    /// <summary>
    /// Deletes a pending employee (invite never accepted) and voids their invite.
    /// </summary>
    /// <remarks>
    /// Only <c>Pending</c> employees can be deleted — onboarded employees are audit
    /// records and must be suspended/offboarded instead. Data is never physically
    /// removed (soft delete). Also blocked (400) if the employee still has direct reports.
    /// </remarks>
    /// <param name="id">The employee's ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Employee deleted successfully.</response>
    /// <response code="400">Employee is not pending, or still has direct reports.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized (requires HR or CompanyAdmin).</response>
    /// <response code="404">Employee not found.</response>
    /// <response code="429">Too many requests - rate limit exceeded.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteEmployeeCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Lists the Active employees in a department (any role).
    /// </summary>
    /// <remarks>
    /// Populates the reporting-manager dropdown on the employee create/edit form —
    /// any active member of the department can be a reporting manager.
    /// </remarks>
    /// <param name="departmentId">The department whose members to list.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The department's members.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized (requires HR or CompanyAdmin).</response>
    [HttpGet("department-members")]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> GetDepartmentMembers(
        [FromQuery] Guid departmentId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDepartmentMembersQuery(departmentId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the members of the current user's own department.
    /// Available to any authenticated user — the team is scoped to
    /// their own department automatically (no cross-department access).
    /// </summary>
    [HttpGet("my-team")]
    [Authorize]
    public async Task<IActionResult> GetMyTeam(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyTeamQuery(), cancellationToken);
        return Ok(result);
    }
}