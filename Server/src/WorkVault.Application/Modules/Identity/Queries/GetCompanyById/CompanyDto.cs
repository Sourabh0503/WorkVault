namespace WorkVault.Application.Modules.Identity.Queries.GetCompanyById;

public record CompanyDto(
    Guid Id,
    string Name,
    string Domain,
    string Industry,
    string? GstNumber,
    string? Logo,
    string Timezone,
    bool IsActive,
    DateTime CreatedAt
);