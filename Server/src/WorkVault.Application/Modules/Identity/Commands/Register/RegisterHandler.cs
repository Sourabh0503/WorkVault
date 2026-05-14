using MediatR;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Modules.Identity.Commands.RegisterCompany;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.Application.Modules.Identity.Commands.Register;

public class RegisterHandler(
    IMediator mediator,
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var companyRegisterCommand = new RegisterCompanyCommand(
            request.CompanyName, request.Domain, request.Industry, request.GstNumber);
        var companyGuid = await mediator.Send(companyRegisterCommand, cancellationToken);

        var user = new User
        {
            CompanyId = companyGuid,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = SystemRoles.CompanyAdmin
        };
        await userRepository.AddAsync(user, cancellationToken);

        // Generate tokens
        var accessToken = jwtTokenService.GenerateAccessToken(user, SystemRoles.CompanyAdminRole);
        var refreshTokenString = jwtTokenService.GenerateRefreshToken();

        // Save refresh token to database
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        return new RegisterResponse(companyGuid, accessToken, refreshTokenString);
    }

}