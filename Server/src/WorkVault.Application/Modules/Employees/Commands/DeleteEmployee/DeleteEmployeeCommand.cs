using MediatR;

namespace WorkVault.Application.Modules.Employees.Commands.DeleteEmployee;

/// <summary>Soft-deletes an employee and deactivates their linked login account.</summary>
/// <param name="Id">The employee to delete.</param>
public record DeleteEmployeeCommand(Guid Id) : IRequest;
