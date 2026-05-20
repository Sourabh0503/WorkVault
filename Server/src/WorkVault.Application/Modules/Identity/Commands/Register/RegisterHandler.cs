using MediatR;
using Microsoft.Extensions.Options;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Settings;
using WorkVault.Application.Modules.Identity.Commands.RegisterCompany;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Constants;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.Register;

/// <summary>
/// Handles company registration with admin user creation.
/// </summary>
/// <remarks>
/// Registration flow:
/// 1. Check email uniqueness (across all companies)
/// 2. Create Company via RegisterCompanyCommand
/// 3. Create User with CompanyAdmin role
/// 4. Hash password with BCrypt
/// 5. Generate JWT access and refresh tokens
/// 6. Save all changes atomically
///
/// This is a public endpoint - no authentication required.
/// </remarks>
public class RegisterHandler(
    IMediator mediator,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email , cancellationToken);
        if (existingUser != null)
            throw new ConflictException("User already exists with this email.");
            
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
            ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays)
        };
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(companyGuid, accessToken, refreshTokenString);
    }

}