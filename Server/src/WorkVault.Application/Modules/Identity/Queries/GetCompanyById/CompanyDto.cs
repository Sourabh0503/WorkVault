namespace WorkVault.Application.Modules.Identity.Queries.GetCompanyById;

/// <summary>Read model describing a company/tenant profile.</summary>
/// <param name="Id">Company id.</param>
/// <param name="Name">Display name.</param>
/// <param name="Domain">Email domain, if set.</param>
/// <param name="Industry">Industry classification.</param>
/// <param name="GstNumber">GST registration number, if any.</param>
/// <param name="Logo">Logo URL, if any.</param>
/// <param name="Timezone">IANA timezone.</param>
/// <param name="IsActive">Whether the company is active.</param>
/// <param name="CreatedAt">When the company was created (UTC).</param>
public record CompanyDto(
    Guid Id,
    string Name,
    string? Domain,
    string Industry,
    string? GstNumber,
    string? Logo,
    string Timezone,
    bool IsActive,
    DateTime CreatedAt
);