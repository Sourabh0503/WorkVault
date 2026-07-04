using MediatR;

namespace WorkVault.Application.Modules.Designations.Queries.GetDesignationById;

public record GetDesignationByIdQuery(Guid Id) : IRequest<DesignationDto?>;

public record DesignationDto(
    Guid Id,
    string Title,
    int Level,
    Guid DepartmentId,
    string DepartmentName,
    DateTime CreatedAt
);
