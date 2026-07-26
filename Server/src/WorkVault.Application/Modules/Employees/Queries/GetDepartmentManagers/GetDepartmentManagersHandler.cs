using MediatR;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Employees.Queries.GetDepartmentManagers;

/// <summary>Loads the department's Active manager-role employees and projects them to dropdown options.</summary>
public class GetDepartmentManagersHandler(IEmployeeRepository repository)
    : IRequestHandler<GetDepartmentManagersQuery, IReadOnlyList<ManagerOptionDto>>
{
    public async Task<IReadOnlyList<ManagerOptionDto>> Handle(
        GetDepartmentManagersQuery request,
        CancellationToken cancellationToken)
    {
        var managers = await repository.GetManagersByDepartmentAsync(request.DepartmentId, cancellationToken);

        return managers.Select(m => new ManagerOptionDto(
            Id: m.Id,
            FullName: $"{m.User?.FirstName} {m.User?.LastName}".Trim(),
            EmployeeCode: m.EmployeeCode
        )).ToList();
    }
}
