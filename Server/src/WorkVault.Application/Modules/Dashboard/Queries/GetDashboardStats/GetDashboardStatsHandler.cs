using MediatR;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsHandler(
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository)
    : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> Handle(
        GetDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        var statusCounts = await employeeRepository.GetStatusCountsAsync(cancellationToken);
        var departmentCount = await departmentRepository.CountAsync(cancellationToken);

        var total = statusCounts.Values.Sum();
        var active = statusCounts.GetValueOrDefault(EmployeeStatus.Active, 0);
        var pending = statusCounts.GetValueOrDefault(EmployeeStatus.Pending, 0);

        return new DashboardStatsDto(
            TotalEmployees: total,
            ActiveEmployees: active,
            PendingInvites: pending,
            TotalDepartments: departmentCount
        );
    }
}