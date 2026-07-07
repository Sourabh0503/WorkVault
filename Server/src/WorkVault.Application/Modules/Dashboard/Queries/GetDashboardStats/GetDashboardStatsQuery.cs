using MediatR;

namespace WorkVault.Application.Modules.Dashboard.Queries.GetDashboardStats;

/// <summary>Requests headline aggregate counts for the current company's dashboard.</summary>
public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

/// <summary>Aggregate counts shown on the dashboard.</summary>
/// <param name="TotalEmployees">All employees in the company (any status).</param>
/// <param name="ActiveEmployees">Employees with Active status.</param>
/// <param name="PendingInvites">Employees still in Pending status (invite not accepted).</param>
/// <param name="TotalDepartments">Number of departments.</param>
public record DashboardStatsDto(
    int TotalEmployees,
    int ActiveEmployees,
    int PendingInvites,
    int TotalDepartments
);