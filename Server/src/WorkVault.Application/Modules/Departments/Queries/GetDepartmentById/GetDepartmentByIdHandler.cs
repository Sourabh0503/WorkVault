using MediatR;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Departments.Queries.GetDepartmentById;

public class GetDepartmentByIdHandler(IDepartmentRepository repository)
    : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>
{
    public async Task<DepartmentDto?> Handle(
        GetDepartmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var department = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (department is null) return null;

        return new DepartmentDto(
            Id: department.Id,
            Name: department.Name,
            Description: department.Description,
            HeadEmployeeId: department.HeadEmployeeId,
            ParentDepartmentId: department.ParentDepartmentId,
            ParentDepartmentName: department.ParentDepartment?.Name,
            CreatedAt: department.CreatedAt
        );
    }
}
