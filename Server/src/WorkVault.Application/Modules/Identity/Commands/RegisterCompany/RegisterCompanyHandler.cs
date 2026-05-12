using MediatR;
using WorkVault.Domain.Modules.Identity;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.RegisterCompany;

public class RegisterCompanyHandler : IRequestHandler<RegisterCompanyCommand, Guid>
{
    private readonly ICompanyRepository _repository;

    public RegisterCompanyHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        RegisterCompanyCommand request,
        CancellationToken cancellationToken)
    {
        var company = new Company
        {
            Name = request.Name,
            Domain = request.Domain,
            Industry = request.Industry,
            GstNumber = request.GstNumber,
            Timezone = request.Timezone
        };

        await _repository.AddAsync(company, cancellationToken);
        return company.Id;
    }
}