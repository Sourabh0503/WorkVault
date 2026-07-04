using MediatR;

namespace WorkVault.Application.Modules.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(
    string Name,
    string? Description,
    Guid? ParentDepartmentId,
    Guid? HeadEmployeeId
) : IRequest<CreateDepartmentResult>;

public record CreateDepartmentResult(Guid Id, string Name);
