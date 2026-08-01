using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Identity;

/// <summary>
/// EF Core data access for <see cref="User"/>. Writes stage changes only; the handler
/// commits via <c>IUnitOfWork</c>.
/// </summary>
public class UserRepository(AppDbContext context) : IUserRepository
{
    /// <summary>Stages a new user for insertion.</summary>
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await context.Users.AddAsync(user, cancellationToken);
    }

    /// <summary>Loads a user by id with the Role eager-loaded.</summary>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>Loads a user by email with the Role eager-loaded (used at login).</summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        // Emails are stored normalized (trimmed + lowercased), so normalize the
        // lookup too — this makes login and the uniqueness checks case-insensitive.
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();

        return await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }
}