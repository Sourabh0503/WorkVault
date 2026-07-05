using MediatR;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Employees.Queries.GetMyTeam;

public class GetMyTeamHandler(
    IEmployeeRepository employeeRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMyTeamQuery, MyTeamResult>
{
    public async Task<MyTeamResult> Handle(
        GetMyTeamQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;

        // No authenticated user — shouldn't happen (endpoint is authorized),
        // but guard anyway.
        if (userId is null)
            return new MyTeamResult(null, []);

        // 1. Find the current user's own employee record
        var me = await employeeRepository.GetByUserIdAsync(userId.Value, cancellationToken);

        // 2. No employee record, or no department assigned → empty team
        if (me is null || me.DepartmentId is null)
            return new MyTeamResult(null, []);

        // 3. Get everyone in the same department
        var members = await employeeRepository.GetByDepartmentAsync(
            me.DepartmentId.Value, cancellationToken);

        // 4. Map to DTOs, flagging which one is "me"
        var memberDtos = members
            .Select(e => new TeamMemberDto(
                Id: e.Id,
                EmployeeCode: e.EmployeeCode,
                FullName: $"{e.User?.FirstName} {e.User?.LastName}".Trim(),
                Email: e.User?.Email ?? string.Empty,
                Phone: e.Phone,
                PhotoUrl: e.PhotoUrl,
                DesignationTitle: e.Designation?.Title,
                IsMe: e.Id == me.Id))
            .ToList();

        return new MyTeamResult(
            DepartmentName: me.Department?.Name,
            Members: memberDtos);
    }
}