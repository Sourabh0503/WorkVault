using MediatR;

namespace WorkVault.Application.Modules.Departments.Commands.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : IRequest;
