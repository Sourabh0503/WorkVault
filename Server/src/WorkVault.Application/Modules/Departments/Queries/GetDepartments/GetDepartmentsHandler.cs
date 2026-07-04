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
            ParentDepartmentName: d.ParentDepartment?.Name
        )).ToList();
    }
}
