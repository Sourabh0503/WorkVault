using MediatR;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Departments.Queries.GetDepartments;

public class GetDepartmentsHandler(IDepartmentRepository repository)
    : IRequestHandler<GetDepartmentsQuery, IReadOnlyList<DepartmentListDto>>
{
    public async Task<IReadOnlyList<DepartmentListDto>> Handle(
        GetDepartmentsQuery request,
        CancellationToken cancellationToken)
    {
        var departments = await repository.GetAllAsync(cancellationToken);

        return departments.Select(d => new DepartmentListDto(
            Id: d.Id,
            Name: d.Name,
            Description: d.Description,
            ParentDepartmentId: d.ParentDepartmentId,
            ParentDepartmentName: d.ParentDepartment?.Name,
            HeadEmployeeId: d.HeadEmployeeId,
            HeadEmployeeName: d.HeadEmployee != null
                ? $"{d.HeadEmployee.User?.FirstName} {d.HeadEmployee.User?.LastName}".Trim()
                : null,
            MemberCount: d.Employees?.Count ?? 0
        )).ToList();
    }
}
