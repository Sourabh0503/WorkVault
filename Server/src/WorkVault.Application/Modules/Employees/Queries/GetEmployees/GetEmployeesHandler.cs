using MediatR;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Models;

namespace WorkVault.Application.Modules.Employees.Queries.GetEmployees;

/// <summary>
/// Handles retrieval of paginated employee list with filters.
/// </summary>
public class GetEmployeesHandler(IEmployeeRepository repository)
    : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeListDto>>
{
    public async Task<PagedResult<EmployeeListDto>> Handle(
        GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        // Clamp page size to max 100
        var pageSize = Math.Min(request.PageSize, 100);
        var pageNumber = Math.Max(request.PageNumber, 1);

        var (employees, totalCount) = await repository.GetAllAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            departmentId: request.DepartmentId,
            status: request.Status,
            managerId: request.ManagerId,
            search: request.Search,
            cancellationToken: cancellationToken);

        var items = employees.Select(e => new EmployeeListDto(
            Id: e.Id,
            EmployeeCode: e.EmployeeCode,
            FullName: $"{e.User?.FirstName} {e.User?.LastName}".Trim(),
            Email: e.User?.Email ?? string.Empty,
            DepartmentName: e.Department?.Name,
            DesignationTitle: e.Designation?.Title,
            Status: e.Status,
            JoinDate: e.JoinDate
        )).ToList();

        return new PagedResult<EmployeeListDto>(
            Items: items,
            TotalCount: totalCount,
            PageNumber: pageNumber,
            PageSize: pageSize);
    }
}
