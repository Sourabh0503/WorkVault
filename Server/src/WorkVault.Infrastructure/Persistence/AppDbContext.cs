using Microsoft.EntityFrameworkCore;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Domain.Modules.Identity;
using WorkVault.SharedKernel;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options,
    ICurrentUserService currentUserService) : DbContext(options)
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply soft delete filter to EVERY entity that extends BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter),
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(this, [modelBuilder]);
            }
        }
        
        // Company is special: its Id is the tenant ID (not CompanyId).
        // Override the generic filter for Company entity.
        modelBuilder.Entity<Company>().HasQueryFilter(c =>
            !c.IsDeleted &&
            (_currentUserService.IsSuperAdmin ||
             _currentUserService.CompanyId == null ||
             c.Id == _currentUserService.CompanyId));
        
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = SystemRoles.SuperAdmin, Name = SystemRoles.SuperAdminRole, IsSystemRole = true, CompanyId = Guid.Empty, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = SystemRoles.CompanyAdmin, Name = SystemRoles.CompanyAdminRole, IsSystemRole = true, CompanyId = Guid.Empty, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = SystemRoles.HR, Name = SystemRoles.HRRole, IsSystemRole = true, CompanyId = Guid.Empty, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = SystemRoles.Manager, Name = SystemRoles.ManagerRole, IsSystemRole = true, CompanyId = Guid.Empty, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = SystemRoles.Employee, Name = SystemRoles.EmployeeRole, IsSystemRole = true, CompanyId = Guid.Empty, CreatedAt = seedDate, UpdatedAt = seedDate }
        );
        
        modelBuilder.Entity<User>().HasIndex(u => new { u.Email, u.CompanyId }).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(rt => !rt.User.IsDeleted);
    }

    private void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder)
        where T : BaseEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted && (_currentUserService.IsSuperAdmin ||
                                                                      _currentUserService.CompanyId == null ||
                                                                      e.CompanyId == _currentUserService.CompanyId));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentUserId = _currentUserService.UserId ?? Guid.Empty;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.CreatedBy = currentUserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}