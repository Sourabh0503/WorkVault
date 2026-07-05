using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WorkVault.Application.Modules.Departments.Commands.CreateDepartment;
using WorkVault.Application.Modules.Departments.Commands.DeleteDepartment;
using WorkVault.Application.Modules.Departments.Commands.UpdateDepartment;
using WorkVault.Application.Modules.Departments.Queries.GetDepartmentById;
using WorkVault.Application.Modules.Departments.Queries.GetDepartments;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.API.Controllers;

/// <summary>
/// Handles department management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
public class DepartmentsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets all departments for the current company.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDepartmentsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a department by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDepartmentByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Creates a new department.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Create(
        [FromBody] CreateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing department.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("URL ID does not match command ID.");

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a department (soft delete).
    /// </summary>
    /// <remarks>
    /// Cannot delete a department that has employees assigned.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteDepartmentCommand(id), cancellationToken);
        return NoContent();
    }
}
