using MediatR;
using WorkVault.Domain.Modules.Identity.Interfaces;

namespace WorkVault.Application.Modules.Identity.Queries.GetCompanyById;

/// <summary>Loads a company by id and maps it to a <see cref="CompanyDto"/> (null if missing).</summary>
public class GetCompanyByIdHandler(ICompanyRepository repository) : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{
    /// <summary>Returns the company profile, or null when no company matches the id.</summary>
    public async Task<CompanyDto?> Handle(
        GetCompanyByIdQuery request,
        CancellationToken cancellationToken)
    {
        var company = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (company is null) return null;

        return new CompanyDto(
            company.Id,
            company.Name,
            company.Domain,
            company.Industry,
            company.GstNumber,
            company.LogoUrl,
            company.Timezone,
            company.IsActive,
            company.CreatedAt
        );
    }
}