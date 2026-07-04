using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WorkVault.Application.Modules.Designations.Commands.CreateDesignation;
using WorkVault.Application.Modules.Designations.Commands.DeleteDesignation;
using WorkVault.Application.Modules.Designations.Commands.UpdateDesignation;
using WorkVault.Application.Modules.Designations.Queries.GetDesignationById;
using WorkVault.Application.Modules.Designations.Queries.GetDesignations;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.API.Controllers;

/// <summary>
/// Handles designation (job title) management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
public class DesignationsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets all designations, optionally filtered by department.
    /// </summary>
    /// <param name="departmentId">Optional department ID to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? departmentId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDesignationsQuery(departmentId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a designation by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDesignationByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Creates a new designation.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Create(
        [FromBody] CreateDesignationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing designation.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDesignationCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("URL ID does not match command ID.");

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a designation (soft delete).
    /// </summary>
    /// <remarks>
    /// Cannot delete a designation that has employees assigned.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteDesignationCommand(id), cancellationToken);
        return NoContent();
    }
}
