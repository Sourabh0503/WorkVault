using MediatR;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.RegisterCompany;

public class RegisterCompanyHandler(ICompanyRepository repository , IUnitOfWork unitOfWork): IRequestHandler<RegisterCompanyCommand, Guid>{

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

        await repository.AddAsync(company, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return company.Id;
    }
}