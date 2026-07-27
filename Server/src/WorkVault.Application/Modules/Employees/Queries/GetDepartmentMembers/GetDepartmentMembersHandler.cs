using MediatR;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Employees.Queries.GetDepartmentMembers;

/// <summary>Loads the department's Active employees and projects them to reporting-manager options.</summary>
public class GetDepartmentMembersHandler(IEmployeeRepository repository)
    : IRequestHandler<GetDepartmentMembersQuery, IReadOnlyList<DepartmentMemberDto>>
{
    public async Task<IReadOnlyList<DepartmentMemberDto>> Handle(
        GetDepartmentMembersQuery request,
        CancellationToken cancellationToken)
    {
        var members = await repository.GetDepartmentMembersAsync(request.DepartmentId, cancellationToken);

        return members.Select(m => new DepartmentMemberDto(
            Id: m.Id,
            FullName: $"{m.User?.FirstName} {m.User?.LastName}".Trim(),
            EmployeeCode: m.EmployeeCode
        )).ToList();
    }
}
