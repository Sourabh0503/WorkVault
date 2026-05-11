using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Identity;

namespace WorkVault.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    public DbSet<Company> Companies => Set<Company>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // global soft delete filter — applies to ALL entities
        modelBuilder.Entity<Company>()
            .HasQueryFilter(c => !c.IsDeleted);
    }
}