using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkVault.Application.Modules.Employees.Commands.CreateEmployee;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// HR adds a new employee. Creates a User + Employee + InviteToken
    /// in one transaction, and returns the invite link for the new employee
    /// to set their password.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = result.EmployeeId }, result);
    }
}