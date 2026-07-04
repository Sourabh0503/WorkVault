using MediatR;

namespace WorkVault.Application.Modules.Designations.Queries.GetDesignations;

public record GetDesignationsQuery(Guid? DepartmentId = null) : IRequest<IReadOnlyList<DesignationListDto>>;

public record DesignationListDto(
    Guid Id,
    string Title,
    int Level,
    Guid DepartmentId,
    string DepartmentName
);
