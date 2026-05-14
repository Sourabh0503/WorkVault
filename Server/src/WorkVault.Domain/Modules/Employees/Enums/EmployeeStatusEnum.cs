namespace WorkVault.Domain.Modules.Employees.Enums;

/// <summary>
/// Employee lifecycle states per the product plan.
/// </summary>
public enum EmployeeStatus
{
    Pending = 0,     // Just created — invite sent but not accepted
    Active = 1,      // Normal working employee
    OnNotice = 2,    // Serving notice period (resigned)
    Suspended = 3,   // Temporarily deactivated by HR
    Offboarded = 4   // Exited the company (record preserved)
}