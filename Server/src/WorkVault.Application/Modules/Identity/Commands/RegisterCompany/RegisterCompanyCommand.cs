using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.RegisterCompany;

public record RegisterCompanyCommand(
    string Name,
    string Domain,
    string Industry,
    string? GstNumber,
    string Timezone = "Asia/Kolkata"
) : IRequest<Guid>;