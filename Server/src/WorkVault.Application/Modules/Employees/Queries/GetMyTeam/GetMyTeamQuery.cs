using MediatR;

namespace WorkVault.Application.Modules.Employees.Queries.GetMyTeam;

/// <summary>
/// Returns the signed-in user's team. Parameterless — "my team" is derived from the
/// current user's own department (resolved via <c>ICurrentUserService</c>).
/// </summary>
public record GetMyTeamQuery : IRequest<MyTeamResult>;

/// <summary>The current user's department and its members.</summary>
/// <param name="DepartmentName">Name of the user's department (null if none assigned).</param>
/// <param name="Members">Everyone in that department, including the user.</param>
public record MyTeamResult(
    string? DepartmentName,
    IReadOnlyList<TeamMemberDto> Members
);

/// <summary>A single team member row.</summary>
/// <param name="Id">Employee id.</param>
/// <param name="EmployeeCode">Employee code.</param>
/// <param name="FullName">Full name.</param>
/// <param name="Email">Work email.</param>
/// <param name="Phone">Contact phone, if any.</param>
/// <param name="PhotoUrl">Profile photo URL, if any.</param>
/// <param name="DesignationTitle">Designation/title, if any.</param>
/// <param name="IsMe">True for the signed-in user, so the UI can highlight "you".</param>
public record TeamMemberDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string Email,
    string? Phone,
    string? PhotoUrl,
    string? DesignationTitle,
    bool IsMe
);