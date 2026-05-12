using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Identity.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}