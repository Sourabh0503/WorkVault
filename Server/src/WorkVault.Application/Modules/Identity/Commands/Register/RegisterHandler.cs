using MediatR;
using Microsoft.Extensions.Options;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Settings;
using WorkVault.Application.Modules.Identity.Commands.RegisterCompany;
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
/// The admin is also created as an Employee record so they appear
/// in the employee directory with sensible defaults.
/// </summary>
public class RegisterHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository,
    IEmployeeRepository employeeRepository,
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

        // ---- Create the admin User ----
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

        // ---- Also create an Employee record for the admin ----
        // They're a real person at the company — should appear in the directory.
        // Sparse by default: no department/designation/manager yet. They can
        // fill those in later via the employee edit page.
        var year = DateTime.UtcNow.Year;
        var countSoFar = await employeeRepository.GetCountForYearAsync(user.CompanyId,year, cancellationToken);
        var employeeCode = $"EMP-{year}-{(countSoFar + 1):D4}";

        var employee = new Employee
        {
            CompanyId = companyGuid,
            UserId = user.Id,
            EmployeeCode = employeeCode,
            JoinDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = EmployeeStatus.Active   // admin is already active, no invite needed
        };
        await employeeRepository.AddAsync(employee, cancellationToken);

        // ---- Tokens ----
        var accessToken = jwtTokenService.GenerateAccessToken(user, SystemRoles.CompanyAdminRole);
        var refreshTokenString = jwtTokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays)
        };
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        // ---- Single atomic save ----
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(companyGuid, user.Id, accessToken, refreshTokenString);
    }
}