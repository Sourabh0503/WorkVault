using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Identity;

/// <summary>
/// EF Core data access for <see cref="Company"/> (tenants). Writes stage changes only;
/// the handler commits via <c>IUnitOfWork</c>.
/// </summary>
public class CompanyRepository (AppDbContext context) : ICompanyRepository
{
    /// <summary>Stages a new company for insertion.</summary>
    public async Task AddAsync(Company company, CancellationToken cancellationToken)
    {
        await context.Companies.AddAsync(company, cancellationToken);
    }

    /// <summary>Loads a company by id.</summary>
    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Companies
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>Loads a company by its email domain.</summary>
    public async Task<Company?> GetByDomainAsync(string domain, CancellationToken cancellationToken)
    {
        return await context.Companies
            .FirstOrDefaultAsync(c => c.Domain == domain, cancellationToken);
    }
}