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

public class SetPasswordHandler(
    IInviteTokenRepository inviteTokenRepository,
    IEmployeeRepository employeeRepository,
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

        return new SetPasswordResult(
            UserId: user.Id,
            CompanyId: user.CompanyId,
            AccessToken: accessToken,
            RefreshToken: refreshTokenString);
    }
}