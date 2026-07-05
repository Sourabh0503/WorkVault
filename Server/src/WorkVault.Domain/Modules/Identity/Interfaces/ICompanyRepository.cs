using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Identity.Interfaces;

/// <summary>
/// Repository interface for <see cref="Company"/> entity operations.
/// </summary>
public interface ICompanyRepository : IRepository<Company>
{
    /// <summary>
    /// Finds a company by its domain name.
    /// </summary>
    /// <param name="domain">The company domain (e.g., "workvault.com").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The company if found, or null.</returns>
    /// <remarks>
    /// Used for domain-based login flows and duplicate prevention.
    /// </remarks>
    Task<Company?> GetByDomainAsync(string domain, CancellationToken cancellationToken);
}