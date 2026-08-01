using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkVault.Application.Modules.Departments.Commands.CreateDepartment;
using WorkVault.Application.Modules.Departments.Queries.GetDepartmentById;
using WorkVault.Application.Modules.Departments.Queries.GetDepartments;
using WorkVault.Application.Modules.Employees.Commands.CreateEmployee;
using WorkVault.Application.Modules.Identity.Commands.Register;
using WorkVault.Application.Modules.Identity.Commands.SetPassword;
using WorkVault.Infrastructure.Persistence;
using WorkVault.SharedKernel.Constants;
using Xunit;

namespace WorkVault.IntegrationTests;

public class TenantIsolationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TenantIsolationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompanyRegistration_ShouldBeAtomicAndIsolated()
    {
        // Arrange & Act - Register two separate companies
        var companyAResponse = await RegisterCompanyAsync("Company Alpha", "alpha.com", "admin@alpha.com");
        var companyBResponse = await RegisterCompanyAsync("Company Beta", "beta.com", "admin@beta.com");

        // Assert
        Assert.NotNull(companyAResponse);
        Assert.NotNull(companyBResponse);
        Assert.NotEqual(Guid.Empty, companyAResponse.CompanyId);
        Assert.NotEqual(Guid.Empty, companyBResponse.CompanyId);
        Assert.NotEqual(companyAResponse.CompanyId, companyBResponse.CompanyId);
        Assert.NotEqual(companyAResponse.UserId, companyBResponse.UserId);
        Assert.False(string.IsNullOrWhiteSpace(companyAResponse.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(companyBResponse.AccessToken));
    }

    [Fact]
    public async Task TenantIsolation_ShouldPreventCrossTenantReadAccess()
    {
        // 1. Register Company A & Company B
        var tenantA = await RegisterCompanyAsync("Tenant A Corp", "tenanta.com", "owner@tenanta.com");
        var tenantB = await RegisterCompanyAsync("Tenant B Corp", "tenantb.com", "owner@tenantb.com");

        // 2. Create a Department in Company A (authorized as Company A)
        var createDeptACommand = new CreateDepartmentCommand("Alpha Engineering", "Eng Dept A", null, null);
        var deptAResult = await CreateDepartmentAsync(tenantA.AccessToken, createDeptACommand);
        Assert.NotNull(deptAResult);

        // 3. Create a Department in Company B (authorized as Company B)
        var createDeptBCommand = new CreateDepartmentCommand("Beta Marketing", "Marketing Dept B", null, null);
        var deptBResult = await CreateDepartmentAsync(tenantB.AccessToken, createDeptBCommand);
        Assert.NotNull(deptBResult);

        // 4. Test Read Isolation: Company A lists all departments
        // Assert: Company A lists departments and only gets Alpha's department, NOT Beta's.
        var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/departments");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tenantA.AccessToken);
        
        var listResponse = await _client.SendAsync(listRequest);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        
        var departmentsA = await listResponse.Content.ReadFromJsonAsync<List<DepartmentDto>>();
        Assert.NotNull(departmentsA);
        Assert.Contains(departmentsA, d => d.Name == "Alpha Engineering");
        Assert.DoesNotContain(departmentsA, d => d.Name == "Beta Marketing");

        // 5. Test Read Isolation: Company A tries to query Company B's specific department by ID
        // Assert: Company A gets 404 (due to global company ID query filters preventing search)
        var getByIdRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/departments/{deptBResult.Id}");
        getByIdRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tenantA.AccessToken);
        
        var getByIdResponse = await _client.SendAsync(getByIdRequest);
        Assert.Equal(HttpStatusCode.NotFound, getByIdResponse.StatusCode);
    }

    [Fact]
    public async Task TenantIsolation_ShouldPreventCrossTenantWriteAccess()
    {
        // 1. Register Company A & Company B
        var tenantA = await RegisterCompanyAsync("Write Tenant A", "writea.com", "admin@writea.com");
        var tenantB = await RegisterCompanyAsync("Write Tenant B", "writeb.com", "admin@writeb.com");

        // 2. Create a Department in Company B
        var createDeptBCommand = new CreateDepartmentCommand("Beta Operations", "Ops Dept B", null, null);
        var deptBResult = await CreateDepartmentAsync(tenantB.AccessToken, createDeptBCommand);
        Assert.NotNull(deptBResult);

        // 3. Test Write Isolation: Company A tries to create an employee but associate them 
        //    with Company B's department.
        // Assert: Company A gets a 404 (due to read-side check of the FK returning null in Company A's context)
        var createEmployeeRequest = new CreateEmployeeCommand(
            Email: "newhire@writea.com",
            FirstName: "Jane",
            LastName: "Doe",
            Phone: "9876543210",
            JoinDate: DateOnly.FromDateTime(DateTime.UtcNow),
            DepartmentId: deptBResult.Id, // Belonging to Company B
            DesignationId: null,
            ManagerId: null,
            RoleId: SystemRoles.Employee
        );

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/employees")
        {
            Content = JsonContent.Create(createEmployeeRequest)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tenantA.AccessToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EmployeeCodes_AreUnique_UnderConcurrentCreates()
    {
        // 1. One tenant + an activated admin token.
        var tenant = await RegisterCompanyAsync("Concurrent Co", "concurrent.com", "admin@concurrent.com");

        // 2. Fire many "add employee" requests in parallel (the multi-HR race).
        const int count = 15;
        var tasks = Enumerable.Range(0, count).Select(async i =>
        {
            var command = new CreateEmployeeCommand(
                Email: $"emp{i}@concurrent.com",
                FirstName: "Emp",
                LastName: $"{i}",
                Phone: "9876543210",
                JoinDate: DateOnly.FromDateTime(DateTime.UtcNow),
                DepartmentId: null,
                DesignationId: null,
                ManagerId: null,
                RoleId: SystemRoles.Employee);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/employees")
            {
                Content = JsonContent.Create(command)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tenant.AccessToken);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode(); // no 500 from a duplicate-code collision
            var result = await response.Content.ReadFromJsonAsync<CreateEmployeeResult>();
            return result!.EmployeeCode;
        });

        var codes = await Task.WhenAll(tasks);

        // 3. Every create succeeded and every code is distinct (atomic counter held).
        Assert.Equal(count, codes.Length);
        Assert.Equal(count, codes.Distinct().Count());
    }

    // ---- Helper Methods ----

    /// <summary>
    /// Registers a company + admin, then completes the invite/set-password step so the
    /// caller gets an authenticated admin. Registration itself no longer auto-logs-in
    /// (no password collected), so the access token comes from set-password.
    /// </summary>
    private async Task<RegisteredTenant> RegisterCompanyAsync(string companyName, string domain, string email)
    {
        var command = new RegisterCommand(
            CompanyName: companyName,
            Domain: domain,
            Industry: "Technology",
            GstNumber: "22AAAAA0000A1Z5",
            FirstName: "Admin",
            LastName: "User",
            Email: email
        );

        var response = await _client.PostAsJsonAsync("/api/auth/register", command);
        response.EnsureSuccessStatusCode();

        var registered = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(registered);

        // Grab the invite token created for the new admin straight from the DB
        // (the API doesn't expose it — it's emailed), then set the password to activate.
        Guid inviteToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            inviteToken = await db.InviteTokens
                .IgnoreQueryFilters()
                .Where(t => t.UserId == registered!.UserId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => t.Token)
                .FirstAsync();
        }

        var setPasswordResponse = await _client.PostAsJsonAsync(
            "/api/auth/set-password",
            new SetPasswordCommand(inviteToken, "SecureP@ss123!"));
        setPasswordResponse.EnsureSuccessStatusCode();

        var session = await setPasswordResponse.Content.ReadFromJsonAsync<SetPasswordResult>();
        Assert.NotNull(session);

        return new RegisteredTenant(registered!.CompanyId, registered.UserId, session!.AccessToken);
    }

    /// <summary>An activated admin: tenant identifiers plus a usable access token.</summary>
    private sealed record RegisteredTenant(Guid CompanyId, Guid UserId, string AccessToken);

    private async Task<CreateDepartmentResult> CreateDepartmentAsync(string accessToken, CreateDepartmentCommand command)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/departments")
        {
            Content = JsonContent.Create(command)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateDepartmentResult>();
        return result!;
    }
}
