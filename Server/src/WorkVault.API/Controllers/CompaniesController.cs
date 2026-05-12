using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkVault.Application.Modules.Identity.Commands.RegisterCompany;
using WorkVault.Application.Modules.Identity.Queries.GetCompanyById;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = SystemRoles.SuperAdminRole)]
    public async Task<IActionResult> Register(
        RegisterCompanyCommand command,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Ok(new { id });
    }
    
    
    [HttpGet("{id:guid}")]
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