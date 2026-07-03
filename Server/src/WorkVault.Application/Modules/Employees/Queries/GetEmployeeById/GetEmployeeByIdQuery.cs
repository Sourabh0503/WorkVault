using MediatR;

namespace WorkVault.Application.Modules.Employees.Queries.GetEmployeeById;

/// <summary>
/// Query to retrieve a single employee by ID.
/// </summary>
public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto?>;
