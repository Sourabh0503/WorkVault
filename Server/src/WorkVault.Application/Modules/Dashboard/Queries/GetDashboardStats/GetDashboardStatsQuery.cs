using MediatR;

namespace WorkVault.Application.Modules.Dashboard.Queries.GetDashboardStats;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public record DashboardStatsDto(
    int TotalEmployees,
    int ActiveEmployees,
    int PendingInvites,
    int TotalDepartments
);