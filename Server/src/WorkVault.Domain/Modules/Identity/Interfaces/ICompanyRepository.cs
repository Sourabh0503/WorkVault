using WorkVault.SharedKernel.Interfaces;
using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Identity.Interfaces;

public interface ICompanyRepository : IRepository<Company>
{
    // company specific methods later
    Task<Company?> GetByDomainAsync(string domain, CancellationToken cancellationToken);
}