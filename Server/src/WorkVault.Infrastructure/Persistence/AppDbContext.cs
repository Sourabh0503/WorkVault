using Microsoft.EntityFrameworkCore;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Domain.Modules.Employees;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Performance;
using WorkVault.SharedKernel;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext with multi-tenancy and audit support.
/// </summary>
/// <remarks>
/// Key features:
///
/// 1. **Multi-tenancy via global query filters**: All BaseEntity queries are
///    automatically filtered by CompanyId from the current user's JWT claims.
///    SuperAdmins bypass this filter.
///
/// 2. **Soft delete**: All BaseEntity queries exclude IsDeleted=true records.
///
/// 3. **Automatic audit fields**: SaveChangesAsync populates CreatedAt, UpdatedAt,
///    CreatedBy, and CompanyId automatically.
///
/// 4. **Role seeding**: The 5 system roles are seeded with fixed GUIDs on migration.
///
/// Special cases:
/// - Company: Uses Id as the tenant ID (not CompanyId field)
/// - RefreshToken: Not a BaseEntity, filtered via User.IsDeleted
/// </remarks>
public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUserService currentUserService) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<InviteToken> InviteTokens => Set<InviteToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<EmployeeCodeCounter> EmployeeCodeCounters => Set<EmployeeCodeCounter>();
    public DbSet<Review> Reviews => Set<Review>();

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
            (currentUserService.IsSuperAdmin ||
             currentUserService.CompanyId == null ||
             c.Id == currentUserService.CompanyId));

        // Roles are special: system roles are seeded with CompanyId = Guid.Empty and must
        // stay visible to every tenant. Otherwise a user's required Role gets filtered out
        // and the User (or Employee) row is dropped via the inner join — e.g. GET /auth/me
        // returning null. Allow system roles plus any tenant-owned roles.
        modelBuilder.Entity<Role>().HasQueryFilter(r =>
            !r.IsDeleted &&
            (r.CompanyId == Guid.Empty ||
             currentUserService.IsSuperAdmin ||
             currentUserService.CompanyId == null ||
             r.CompanyId == currentUserService.CompanyId));

        // ---- Department ↔ Employee relationships (two separate FKs) ----

        // A department HAS MANY employees (via Employee.DepartmentId)
        modelBuilder.Entity<Department>()
            .HasMany(d => d.Employees)
            .WithOne(e => e.Department)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull); // if dept deleted, employees keep existing with null dept

        // A department HAS ONE head employee (via Department.HeadEmployeeId)
        modelBuilder.Entity<Department>()
            .HasOne(d => d.HeadEmployee)
            .WithMany() // Employee has no "departments I head" collection
            .HasForeignKey(d => d.HeadEmployeeId)
            .OnDelete(DeleteBehavior.SetNull); // if head employee deleted, dept keeps existing, head becomes null

        SeedRolesData(modelBuilder);
        
        // Email unique only among non-deleted users — a soft-deleted (cancelled-invite)
        // user frees up their email for reuse.
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Email, u.CompanyId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        
        modelBuilder.Entity<Employee>()
            .HasIndex(e => new {e.EmployeeCode , e.CompanyId})
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(rt => !rt.User.IsDeleted);

        // ---- Performance reviews ----
        // Soft-delete + tenant query filters are applied automatically (Review is a BaseEntity).
        modelBuilder.Entity<Review>(b =>
        {
            // Many reviews per employee. No collection nav on Employee (keeps it decoupled);
            // reviews are historical and preserved, so restrict rather than cascade.
            b.HasOne(r => r.Employee)
                .WithMany()
                .HasForeignKey(r => r.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rating 0–5 with one decimal; salary as money.
            b.Property(r => r.Rating).HasPrecision(3, 2);
            b.Property(r => r.NewSalary).HasPrecision(18, 2);
            b.Property(r => r.ReviewName).HasMaxLength(200);

            // List/charts always query by employee, newest first.
            b.HasIndex(r => new { r.EmployeeId, r.ReviewDate });
        });

        // Employee-code counter: composite key, no tenant filter/soft-delete (not a BaseEntity).
        modelBuilder.Entity<EmployeeCodeCounter>(b =>
        {
            b.ToTable("EmployeeCodeCounters");
            b.HasKey(c => new { c.CompanyId, c.Year });
        });
    }

    private void SeedRolesData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = SystemRoles.SuperAdmin, Name = SystemRoles.SuperAdminRole, IsSystemRole = true,
                CompanyId = Guid.Empty, CreatedAt = seedDate, UpdatedAt = seedDate
            },
            new Role
            {
                Id = SystemRoles.CompanyAdmin, Name = SystemRoles.CompanyAdminRole, IsSystemRole = true,
                CompanyId = Guid.Empty, CreatedAt = seedDate, UpdatedAt = seedDate
            },
            new Role
            {
                Id = SystemRoles.HR, Name = SystemRoles.HRRole, IsSystemRole = true, CompanyId = Guid.Empty,
                CreatedAt = seedDate, UpdatedAt = seedDate
            },
            new Role
            {
                Id = SystemRoles.Manager, Name = SystemRoles.ManagerRole, IsSystemRole = true, CompanyId = Guid.Empty,
                CreatedAt = seedDate, UpdatedAt = seedDate
            },
            new Role
            {
                Id = SystemRoles.Employee, Name = SystemRoles.EmployeeRole, IsSystemRole = true, CompanyId = Guid.Empty,
                CreatedAt = seedDate, UpdatedAt = seedDate
            }
        );
    }

    private void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder)
        where T : BaseEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted && (currentUserService.IsSuperAdmin ||
                                                                      currentUserService.CompanyId == null ||
                                                                      e.CompanyId == currentUserService.CompanyId));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentUserId = currentUserService.UserId ?? Guid.Empty;
        var currentCompanyId = currentUserService.CompanyId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.CreatedBy = currentUserId;
                    if (entry.Entity.CompanyId == Guid.Empty && currentCompanyId.HasValue)
                        entry.Entity.CompanyId = currentCompanyId.Value;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}