using MediatR;

namespace WorkVault.Application.Modules.Identity.Queries.GetCompanyById;

public record GetCompanyByIdQuery(Guid Id) : IRequest<CompanyDto?>;