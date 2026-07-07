using MediatR;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Departments.Queries.GetDepartmentById;

/// <summary>Loads a department by id and maps it to a <see cref="DepartmentDto"/> (null if missing).</summary>
public class GetDepartmentByIdHandler(IDepartmentRepository repository)
    : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>
{
    /// <summary>Returns the department detail, or null when no department matches the id.</summary>
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
