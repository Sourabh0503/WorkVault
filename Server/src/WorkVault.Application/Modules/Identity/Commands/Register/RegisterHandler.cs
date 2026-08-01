using MediatR;
using Microsoft.Extensions.Configuration;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Messaging;
using WorkVault.Domain.Modules.Employees;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Constants;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.Register;

/// <summary>
/// Handles company registration with admin user creation.
/// </summary>
/// <remarks>
/// Registration is now invite-based (mirrors the employee onboarding flow):
/// 1. Enforce one company per email — reject if the email is already in use.
/// 2. Create the Company (tenant).
/// 3. Create the admin User with no password (IsActive = false).
/// 4. Create an Employee record for the admin (Status = Pending).
/// 5. Create an InviteToken (48h expiry).
/// 6. Save everything in a single transaction.
/// 7. Publish the invite email with the set-password link.
///
/// The admin activates and logs in by following the emailed link
/// (handled by <c>SetPasswordHandler</c>) — no tokens are issued here.
/// </remarks>
public class RegisterHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository,
    IEmployeeRepository employeeRepository,
    IInviteTokenRepository inviteTokenRepository,
    IEmailPublisher emailPublisher,
    IConfiguration configuration,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // 1. One company per email — an admin can own only one company.
        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
            throw new ConflictException("User already exists with this email.");

        // ---- Create the Company ----
        var company = new Company
        {
            Name = request.CompanyName,
            Domain = request.Domain,
            Industry = request.Industry,
            GstNumber = request.GstNumber,
            Timezone = "Asia/Kolkata"
        };
        await companyRepository.AddAsync(company, cancellationToken);
        var companyGuid = company.Id;

        // ---- Create the admin User — no password, inactive until invite accepted ----
        var user = new User
        {
            CompanyId = companyGuid,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = string.Empty,   // set later via the invite/set-password flow
            IsActive = false,              // activated when password is set
            RoleId = SystemRoles.CompanyAdmin
        };
        await userRepository.AddAsync(user, cancellationToken);

        // ---- Also create an Employee record for the admin ----
        // They're a real person at the company — should appear in the directory.
        // Sparse by default: no department/designation/manager yet. Pending until
        // they accept the invite (SetPasswordHandler flips this to Active).
        var year = DateTime.UtcNow.Year;
        var seq = await employeeRepository.AllocateNextCodeNumberAsync(user.CompanyId, year, cancellationToken);
        var employeeCode = $"EMP-{year}-{seq:D4}";

        var employee = new Employee
        {
            CompanyId = companyGuid,
            UserId = user.Id,
            EmployeeCode = employeeCode,
            JoinDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = EmployeeStatus.Pending   // activated when the invite is accepted
        };
        await employeeRepository.AddAsync(employee, cancellationToken);

        // ---- Invite token (48h expiry from entity defaults) ----
        var inviteToken = new InviteToken
        {
            UserId = user.Id
            // Token and ExpiresAt have defaults on the entity
        };
        await inviteTokenRepository.AddAsync(inviteToken, cancellationToken);

        // ---- Single atomic save ----
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ---- Send the confirmation email with the set-password link ----
        // Points back into the registration wizard (step 3), not the employee
        // invite page — the admin finishes signup where they started.
        var frontendUrl = configuration["AppSettings:FrontendUrl"];
        var inviteLink = $"{frontendUrl}/register/{inviteToken.Token}";
        await emailPublisher.PublishAsync(new EmailMessage(
            To: user.Email,
            Subject: $"Verify your email to activate {company.Name} on WorkVault",
            Body: EmailTemplate.VerifyEmail(
                firstName: user.FirstName,
                companyName: company.Name,
                ctaUrl: inviteLink,
                expiryText: "This link expires in 48 hours. If you didn't create this account, you can safely ignore this email."),
            Type: EmailType.Registration
        ), cancellationToken);

        return new RegisterResponse(companyGuid, user.Id, user.Email);
    }
}
