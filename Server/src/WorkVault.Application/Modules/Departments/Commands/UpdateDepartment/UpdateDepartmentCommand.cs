using MediatR;

namespace WorkVault.Application.Modules.Departments.Commands.UpdateDepartment;

public record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentDepartmentId,
    Guid? HeadEmployeeId
) : IRequest<UpdateDepartmentResult>;

public record UpdateDepartmentResult(Guid Id, string Name);
