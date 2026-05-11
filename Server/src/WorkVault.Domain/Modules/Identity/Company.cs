using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Identity;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string Industry { get; set; } = string.Empty;
    public string Timezone { get; set; } = "Asia/Kolkata";
    public string? GstNumber { get; set; }
    public bool IsActive { get; set; } = true;
}