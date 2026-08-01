namespace WorkVault.Domain.Modules.Employees;

/// <summary>
/// Per-tenant, per-year counter used to generate collision-free employee codes
/// (<c>EMP-{year}-{seq}</c>).
/// </summary>
/// <remarks>
/// Allocated atomically (a single <c>INSERT … ON CONFLICT … RETURNING</c>), so
/// concurrent "add employee" requests never receive the same number, and soft-deletes
/// never lower the next value (numbers are monotonic and never reused).
///
/// Deliberately NOT a <see cref="WorkVault.SharedKernel.BaseEntity"/>: no soft-delete,
/// no audit stamps, no tenant query filter — it's keyed by (CompanyId, Year) and read
/// via raw SQL, not change tracking.
/// </remarks>
public class EmployeeCodeCounter
{
    /// <summary>Tenant this counter belongs to. Part of the composite key.</summary>
    public Guid CompanyId { get; set; }

    /// <summary>Calendar year the codes are scoped to. Part of the composite key.</summary>
    public int Year { get; set; }

    /// <summary>Highest sequence number allocated so far for this company + year.</summary>
    public int LastValue { get; set; }
}
