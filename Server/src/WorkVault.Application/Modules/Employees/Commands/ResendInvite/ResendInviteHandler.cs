using MediatR;
using Microsoft.Extensions.Logging;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Employees.Commands.ResendInvite;

public class ResendInviteHandler(
    IEmployeeRepository employeeRepository,
    IInviteTokenRepository inviteTokenRepository,
    IUnitOfWork unitOfWork,
    ILogger<ResendInviteHandler> logger)
    : IRequestHandler<ResendInviteCommand, ResendInviteResult>
{
    public async Task<ResendInviteResult> Handle(
        ResendInviteCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find employee (tenant filter scopes to current company)
        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            throw new NotFoundException("Employee not found.");

        if (employee.User is null)
            throw new InvalidOperationException("Employee has no linked user account.");

        // 2. Verify they're still Pending (can't resend to active employees)
        if (employee.Status != EmployeeStatus.Pending)
            throw new BusinessRuleException(
                $"Cannot resend invite — employee status is '{employee.Status}', not Pending.");

        // 3. Revoke any existing active invite (mark as used)
        var existingInvite = await inviteTokenRepository
            .GetActiveTokenForUserAsync(employee.UserId, cancellationToken);

        if (existingInvite is not null)
            existingInvite.UsedAt = DateTime.UtcNow;

        // 4. Create new invite token (defaults: 48h expiry, fresh Guid)
        var newInvite = new InviteToken
        {
            UserId = employee.UserId
        };
        await inviteTokenRepository.AddAsync(newInvite, cancellationToken);

        // 5. Save — both updates and new insert in one transaction
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Log link (later: send real email)
        var inviteLink = $"http://localhost:5012/auth/set-password?token={newInvite.Token}";
        logger.LogInformation(
            "Invite resent for {Email}. Link: {InviteLink}",
            employee.User.Email, inviteLink);

        return new ResendInviteResult(
            EmployeeId: employee.Id,
            Email: employee.User.Email,
            InviteLink: inviteLink);
    }
}