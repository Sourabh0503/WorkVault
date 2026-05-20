using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Identity;

/// <summary>
/// Represents a tenant/organization in the multi-tenant system.
/// Each company is an isolated tenant with its own users, employees, and data.
/// </summary>
/// <remarks>
/// Special note: For Company entities, the <see cref="BaseEntity.Id"/> IS the tenant ID.
/// The <see cref="BaseEntity.CompanyId"/> field equals <see cref="BaseEntity.Id"/> for companies.
/// This is handled specially in AppDbContext's query filters.
/// </remarks>
public class Company : BaseEntity
{
    /// <summary>Legal or display name of the company.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Company's domain (e.g., "acme.com") - used for branding/identification.</summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>URL to the company's logo image. Nullable.</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Industry sector (e.g., "Technology", "Healthcare", "Manufacturing").</summary>
    public string Industry { get; set; } = string.Empty;

    /// <summary>
    /// IANA timezone identifier for the company's primary location.
    /// Defaults to "Asia/Kolkata" (IST) as the platform targets Indian SMEs.
    /// </summary>
    public string Timezone { get; set; } = "Asia/Kolkata";

    /// <summary>Indian GST registration number. Nullable for companies without GST.</summary>
    public string? GstNumber { get; set; }

    /// <summary>
    /// Whether the company account is active.
    /// Inactive companies cannot log in or access the platform.
    /// </summary>
    public bool IsActive { get; set; } = true;
}