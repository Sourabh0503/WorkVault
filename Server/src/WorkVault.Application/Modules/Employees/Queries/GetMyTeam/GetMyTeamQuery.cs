using MediatR;

namespace WorkVault.Application.Modules.Employees.Queries.GetMyTeam;

// No parameters — "my team" is derived from the current user's own department.
public record GetMyTeamQuery : IRequest<MyTeamResult>;

public record MyTeamResult(
    string? DepartmentName,
    IReadOnlyList<TeamMemberDto> Members
);

public record TeamMemberDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string Email,
    string? Phone,
    string? PhotoUrl,
    string? DesignationTitle,
    bool IsMe   // flag so the frontend can highlight "you"
);