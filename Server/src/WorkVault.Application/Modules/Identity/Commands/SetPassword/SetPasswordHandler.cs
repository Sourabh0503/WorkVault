using MediatR;
using Microsoft.Extensions.Options;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Settings;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.SetPassword;

/// <summary>
/// Handles employee invite acceptance and password setup.
/// </summary>
/// <remarks>
/// This is the final step in the employee onboarding flow:
/// 1. Validate invite token (not used, not expired)
/// 2. Validate user exists and is not already active
/// 3. Hash and save password
/// 4. Activate user (IsActive = true)
/// 5. Activate employee (Status = Active)
/// 6. Mark invite token as used
/// 7. Generate JWT tokens (auto-login after setup)
///
/// Returns null for invalid tokens (controller returns 404).
/// This is a public endpoint - the invite token is the credential.
/// </remarks>
public class SetPasswordHandler(
    IInviteTokenRepository inviteTokenRepository,
    IEmployeeRepository employeeRepository,
    ICompanyRepository companyRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<SetPasswordCommand, SetPasswordResult?>
{
    public async Task<SetPasswordResult?> Handle(
        SetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Look up invite (User + Role both included)
        var invite = await inviteTokenRepository.GetByTokenAsync(
            request.Token, cancellationToken);

        if (invite is null || !invite.IsValid)
            return null;

        var user = invite.User;
        if (user is null || user.IsActive)
            return null;

        // 2. Hash password & activate user
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.IsActive = true;
        user.LastLogin = DateTime.UtcNow;

        // 3. Activate the linked Employee record
        var employee = await employeeRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (employee is not null)
            employee.Status = EmployeeStatus.Active;

        // 4. Mark invite as used
        invite.UsedAt = DateTime.UtcNow;

        // 5. Generate JWT
        var accessToken = jwtTokenService.GenerateAccessToken(user, user.Role!.Name);
        var refreshTokenString = jwtTokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays)
        };
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        // 6. Single SaveChanges
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Resolve the tenant display name for UI branding. The invited user always has a
        // valid CompanyId (FK), and this public endpoint runs with a permissive tenant
        // filter — so a null here means a corrupt tenant reference. Fail loud (500).
        var company = await companyRepository.GetByIdAsync(user.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Company '{user.CompanyId}' not found for user '{user.Id}' during invite acceptance.");

        return new SetPasswordResult(
            UserId: user.Id,
            CompanyId: user.CompanyId,
            CompanyName: company.Name,
            AccessToken: accessToken,
            RefreshToken: refreshTokenString);
    }
}