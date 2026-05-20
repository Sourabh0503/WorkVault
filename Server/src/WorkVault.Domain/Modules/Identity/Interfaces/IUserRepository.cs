using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Identity.Interfaces;

/// <summary>
/// Repository interface for <see cref="User"/> entity operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Finds a user by their email address within the current tenant.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user with Role included, or null if not found.</returns>
    /// <remarks>
    /// Tenant isolation is enforced via global query filter.
    /// The same email can exist in different companies.
    /// </remarks>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}