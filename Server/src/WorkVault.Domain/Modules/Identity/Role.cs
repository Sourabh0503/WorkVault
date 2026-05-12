using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Identity;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; } = true;
}