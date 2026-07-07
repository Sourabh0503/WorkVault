using MediatR;

namespace WorkVault.Application.Modules.Identity.Queries.GetCompanyById;

/// <summary>Fetches a single company's profile by id. Returns null if not found.</summary>
/// <param name="Id">The company (tenant) id.</param>
public record GetCompanyByIdQuery(Guid Id) : IRequest<CompanyDto?>;