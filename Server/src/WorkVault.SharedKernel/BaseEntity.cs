namespace WorkVault.SharedKernel;

/// <summary>
/// Abstract base class for all domain entities in the system.
/// Provides multi-tenancy, soft delete, and audit trail capabilities.
/// </summary>
/// <remarks>
/// All entities inheriting from this class automatically get:
/// - Tenant isolation via <see cref="CompanyId"/> (enforced by global query filters)
/// - Soft delete support via <see cref="IsDeleted"/> (never hard delete data)
/// - Audit fields populated automatically by <see cref="Infrastructure.Persistence.AppDbContext"/>
/// </remarks>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary key. Uses Guid v7 (time-sortable) for better index performance
    /// and distributed system compatibility.
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant identifier for multi-tenancy isolation.
    /// All queries are automatically filtered by this value via EF Core global filters.
    /// Auto-populated from JWT claims during SaveChangesAsync if not set.
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// UTC timestamp when the entity was created.
    /// Auto-populated by AppDbContext.SaveChangesAsync.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp when the entity was last modified.
    /// Auto-updated by AppDbContext.SaveChangesAsync on every change.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID who created this entity.
    /// Auto-populated from JWT claims during SaveChangesAsync.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Soft delete flag. When true, entity is excluded from all queries
    /// via global query filter. Data is never physically deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}