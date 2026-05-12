using MediatR;
using WorkVault.Domain.Modules.Identity.Interfaces;

namespace WorkVault.Application.Modules.Identity.Queries.GetCompanyById;

public class GetCompanyByIdHandler(ICompanyRepository repository) : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{

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