using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Employees.Queries.GetEmployeeById;

/// <summary>
/// Handles retrieval of a single employee by ID.
/// </summary>
public class GetEmployeeByIdHandler(IEmployeeRepository repository)
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto?>
{
    public async Task<EmployeeDto?> Handle(
        GetEmployeeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (employee is null) throw new NotFoundException($"Employee {request.Id} not found.");;

        return new EmployeeDto(
            Id: employee.Id,
            EmployeeCode: employee.EmployeeCode,
            Email: employee.User?.Email ?? string.Empty,
            FirstName: employee.User?.FirstName ?? string.Empty,
            LastName: employee.User?.LastName ?? string.Empty,
            Phone: employee.Phone,
            PhotoUrl: employee.PhotoUrl,
            DateOfBirth: employee.DateOfBirth,
            JoinDate: employee.JoinDate,
            ResignationDate: employee.ResignationDate,
            LastWorkingDay: employee.LastWorkingDay,
            Status: employee.Status,
            Department: employee.Department is not null
                ? new DepartmentInfo(employee.Department.Id, employee.Department.Name)
                : null,
            Designation: employee.Designation is not null
                ? new DesignationInfo(
                    employee.Designation.Id,
                    employee.Designation.Title,
                    employee.Designation.Level)
                : null,
            Manager: employee.Manager is not null
                ? new ManagerInfo(
                    employee.Manager.Id,
                    employee.Manager.EmployeeCode,
                    $"{employee.Manager.User?.FirstName} {employee.Manager.User?.LastName}".Trim())
                : null,
            Role: new RoleInfo(
                employee.User?.RoleId ?? Guid.Empty,
                employee.User?.Role?.Name ?? string.Empty),
            CreatedAt: employee.CreatedAt
        );
    }
}
