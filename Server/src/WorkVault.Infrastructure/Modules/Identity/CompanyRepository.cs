using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Identity;

public class CompanyRepository (AppDbContext context) : ICompanyRepository
{
    public async Task AddAsync(Company company, CancellationToken cancellationToken)
    {
        await context.Companies.AddAsync(company, cancellationToken);
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Companies
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Company?> GetByDomainAsync(string domain, CancellationToken cancellationToken)
    {
        return await context.Companies
            .FirstOrDefaultAsync(c => c.Domain == domain, cancellationToken);
    }
}