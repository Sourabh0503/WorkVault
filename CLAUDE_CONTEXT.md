# WorkVault - Project Context for AI Assistants

> Use this file to understand the complete codebase structure, architecture, and implementation details.

---

## Project Overview

**WorkVault** is a multi-tenant SaaS platform for HR, assets, attendance, and workplace operations built for Indian SMEs with 50-500 employees.

**Tech Stack:**
- Backend: .NET 9 Web API
- Database: PostgreSQL 16
- ORM: Entity Framework Core 9
- Architecture: Clean Architecture + Modular Monolith
- Auth: JWT + Refresh Tokens + BCrypt
- CQRS: MediatR
- Validation: FluentValidation with MediatR pipeline behavior
- Persistence: Unit of Work pattern

---

## Solution Structure

```
Server/src/
├── WorkVault.API/                    # Entry point, controllers, middleware
│   ├── Controllers/
│   │   ├── AuthController.cs         # POST /api/auth/register, login, refresh
│   │   ├── CompaniesController.cs    # POST /api/companies (SuperAdmin only), GET by ID
│   │   └── EmployeesController.cs    # POST /api/employees (HR/CompanyAdmin)
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Services/
│   │   └── CurrentUserService.cs     # JWT claims extraction
│   ├── Program.cs                    # App configuration, DI, middleware pipeline
│   └── appsettings.Development.json  # Connection string, JWT settings
│
├── WorkVault.Application/            # Business logic, CQRS handlers, DTOs
│   ├── Common/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs # MediatR pipeline for FluentValidation
│   │   ├── Interfaces/
│   │   │   ├── ICurrentUserService.cs
│   │   │   └── IJwtTokenService.cs
│   │   └── Settings/
│   │       └── JwtSettings.cs
│   └── Modules/
│       ├── Identity/
│       │   ├── Commands/
│       │   │   ├── Login/
│       │   │   │   ├── LoginCommand.cs
│       │   │   │   ├── LoginCommandValidator.cs
│       │   │   │   └── LoginHandler.cs
│       │   │   ├── Register/
│       │   │   │   ├── RegisterCommand.cs
│       │   │   │   ├── RegisterCommandValidator.cs
│       │   │   │   └── RegisterHandler.cs
│       │   │   ├── RegisterCompany/
│       │   │   │   ├── RegisterCompanyCommand.cs
│       │   │   │   └── RegisterCompanyHandler.cs
│       │   │   └── RefreshTokens/
│       │   │       ├── RefreshTokenCommand.cs
│       │   │       ├── RefreshTokenCommandValidator.cs
│       │   │       └── RefreshTokenHandler.cs
│       │   ├── Queries/
│       │   │   └── GetCompanyById/
│       │   └── DTOs/
│       │       └── CompanyDto.cs
│       └── Employees/
│           └── Commands/
│               └── CreateEmployee/
│                   ├── CreateEmployeeCommand.cs
│                   ├── CreateEmployeeCommandValidator.cs
│                   └── CreateEmployeeHandler.cs
│
├── WorkVault.Domain/                 # Entities, enums, repository interfaces
│   └── Modules/
│       ├── Identity/
│       │   ├── Company.cs
│       │   ├── User.cs
│       │   ├── Role.cs
│       │   ├── RefreshToken.cs       # NOT BaseEntity
│       │   ├── InviteToken.cs        # Employee invitation tokens
│       │   └── Interfaces/
│       │       ├── ICompanyRepository.cs
│       │       ├── IUserRepository.cs
│       │       ├── IRefreshTokenRepository.cs
│       │       └── IInviteTokenRepository.cs
│       └── Employees/
│           ├── Employee.cs
│           ├── Department.cs
│           ├── Designation.cs
│           ├── Enums/
│           │   └── EmployeeStatusEnum.cs
│           └── Interfaces/
│               └── IEmployeeRepository.cs
│
├── WorkVault.Infrastructure/         # EF Core, repositories, external services
│   ├── Auth/
│   │   └── JwtTokenService.cs
│   ├── Modules/
│   │   ├── Identity/
│   │   │   ├── CompanyRepository.cs
│   │   │   ├── UserRepository.cs
│   │   │   ├── RefreshTokenRepository.cs
│   │   │   └── InviteTokenRepository.cs
│   │   └── Employees/
│   │       └── EmployeeRepository.cs
│   ├── Persistence/
│   │   ├── AppDbContext.cs           # DbContext with filters, seeding, audit
│   │   ├── UnitOfWork.cs             # IUnitOfWork implementation
│   │   └── Migrations/
│   └── DependencyInjection.cs
│
└── WorkVault.SharedKernel/           # Shared base classes, constants, interfaces
    ├── BaseEntity.cs                 # Id, CompanyId, CreatedAt, UpdatedAt, CreatedBy, IsDeleted
    ├── Constants/
    │   └── SystemRoles.cs            # Role GUIDs and name constants
    └── Interfaces/
        ├── IRepository.cs            # Generic repository interface
        └── IUnitOfWork.cs            # SaveChangesAsync coordinator
```

---

## Database Schema

### Identity Module

```
Companies
├── Id (PK, Guid)
├── Name, Domain, Industry, Timezone, LogoUrl, GstNumber
├── IsActive
└── [BaseEntity fields]: CompanyId, CreatedAt, UpdatedAt, CreatedBy, IsDeleted

Roles (5 seeded system roles)
├── Id (PK, Guid)
├── Name (SuperAdmin, CompanyAdmin, HR, Manager, Employee)
├── IsSystemRole
└── [BaseEntity fields]

Users
├── Id (PK, Guid)
├── Email, PasswordHash, FirstName, LastName
├── RoleId (FK → Roles)
├── IsActive, LastLogin
├── [BaseEntity fields]
└── Unique Index: (Email, CompanyId)

RefreshTokens (NOT BaseEntity)
├── Id (PK, Guid)
├── UserId (FK → Users, cascade delete)
├── Token (string)
├── ExpiresAt, IsRevoked

InviteTokens
├── Id (PK, Guid)
├── UserId (FK → Users)
├── Token (Guid) - random token for URL
├── ExpiresAt (48h default)
├── UsedAt (null = unused)
└── [BaseEntity fields]
```

### Employees Module

```
Employees
├── Id (PK, Guid)
├── EmployeeCode (e.g., EMP-2025-0042)
├── UserId (FK → Users)
├── Phone, PhotoUrl, DateOfBirth
├── DepartmentId (FK → Departments)
├── DesignationId (FK → Designations)
├── ManagerId (FK → Employees, self-ref)
├── JoinDate, ResignationDate, LastWorkingDay
├── Status (EmployeeStatus enum)
└── [BaseEntity fields]

Departments
├── Id (PK, Guid)
├── Name, Description
├── HeadEmployeeId (FK → Employees)
├── ParentDepartmentId (FK → Departments, self-ref)
└── [BaseEntity fields]

Designations
├── Id (PK, Guid)
├── Title (e.g., "Senior Software Engineer")
├── Level (1-5 seniority)
├── DepartmentId (FK → Departments)
└── [BaseEntity fields]
```

### EmployeeStatus Enum

```csharp
Pending = 0,     // Invite sent, not accepted
Active = 1,      // Normal working employee
OnNotice = 2,    // Serving notice period
Suspended = 3,   // Temporarily deactivated
Offboarded = 4   // Exited company
```

### System Role IDs (Seeded)

```csharp
SuperAdmin   = 11111111-1111-1111-1111-111111111111
CompanyAdmin = 22222222-2222-2222-2222-222222222222
HR           = 33333333-3333-3333-3333-333333333333
Manager      = 44444444-4444-4444-4444-444444444444
Employee     = 55555555-5555-5555-5555-555555555555
```

---

## Key Design Patterns

### 1. Multi-Tenancy
Every entity inherits from `BaseEntity` which includes `CompanyId`. EF Core global query filters ensure tenant isolation.

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }      // Tenant isolation
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public bool IsDeleted { get; set; }       // Soft delete
}
```

### 2. Soft Deletes
Global query filter applied to all `BaseEntity` types:
```csharp
modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
```

### 3. Automatic Audit Fields
`AppDbContext.SaveChangesAsync` automatically populates:
- **Added entities**: `CreatedAt`, `UpdatedAt`, `CreatedBy` (from current user)
- **Modified entities**: `UpdatedAt`

### 4. Unit of Work Pattern
Handlers inject `IUnitOfWork` to coordinate saves across multiple repositories:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

**Why?** Multi-step operations (User + Employee + InviteToken) commit atomically.

### 5. CQRS with MediatR
- **Commands**: Write operations (Register, Login, CreateEmployee)
- **Queries**: Read operations (GetCompanyById)
- All handlers in `Application/Modules/{Module}/Commands|Queries/`

### 6. Role-Based Authorization
```csharp
[Authorize(Roles = SystemRoles.SuperAdminRole)]
[Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]

// Available role constants:
SystemRoles.SuperAdminRole    // "SuperAdmin"
SystemRoles.CompanyAdminRole  // "CompanyAdmin"
SystemRoles.HRRole            // "HR"
SystemRoles.ManagerRole       // "Manager"
SystemRoles.EmployeeRole      // "Employee"

// Role IDs for database operations:
SystemRoles.SuperAdmin        // Guid
SystemRoles.CompanyAdmin      // Guid
```

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | Public | Register company + admin user |
| POST | `/api/auth/login` | Public | Login, returns tokens |
| POST | `/api/auth/refresh` | Public | Refresh access token |
| POST | `/api/companies` | SuperAdmin | Create company directly |
| GET | `/api/companies/{id}` | Authorized | Get company by ID |
| POST | `/api/employees` | HR, CompanyAdmin | Create employee + send invite |

---

## Authentication Flow

### 1. Register (Company + Admin)
```
POST /api/auth/register
→ RegisterHandler
  1. Creates Company via RegisterCompanyCommand
  2. Creates User (CompanyAdmin role, BCrypt hashed password)
  3. Generates JWT access token (configurable minutes)
  4. Generates refresh token (configurable days)
  5. Saves via IUnitOfWork.SaveChangesAsync
← Returns: { companyId, accessToken, refreshToken }
```

### 2. Login
```
POST /api/auth/login
→ LoginHandler
  1. Find user by email (includes Role)
  2. Verify password with BCrypt
  3. Generate JWT access token
  4. Generate and save refresh token
  5. Update LastLogin
  6. Saves via IUnitOfWork.SaveChangesAsync
← Returns: { userId, companyId, accessToken, refreshToken }
```

### 3. Refresh Token
```
POST /api/auth/refresh
→ RefreshTokenHandler
  1. Find token in DB (includes User.Role)
  2. Check if expired or revoked
  3. Revoke old token (rotation)
  4. Generate new access + refresh tokens
  5. Saves via IUnitOfWork.SaveChangesAsync
← Returns: { accessToken, refreshToken }
```

### JWT Claims
```csharp
ClaimTypes.NameIdentifier → User.Id
ClaimTypes.Email → User.Email
"CompanyId" → User.CompanyId
ClaimTypes.Role → Role.Name
"FirstName" → User.FirstName
"LastName" → User.LastName
```

---

## Employee Invite Flow

### Create Employee (HR adds new employee)
```
POST /api/employees
→ CreateEmployeeHandler
  1. Check email not already used in company
  2. Create User (no password, IsActive=false, Role=Employee)
  3. Generate EmployeeCode: EMP-{year}-{sequence:D4}
  4. Create Employee record linked to User
  5. Create InviteToken (48h expiry)
  6. Single SaveChangesAsync (atomic transaction)
  7. Log invite link (email stub)
← Returns: { employeeId, employeeCode, inviteLink }
```

### Set Password (Employee accepts invite) - TODO
```
POST /api/auth/set-password?token={inviteToken}
→ SetPasswordHandler
  1. Find InviteToken by token guid
  2. Validate: not used, not expired
  3. Set User.PasswordHash, User.IsActive = true
  4. Mark InviteToken.UsedAt
  5. Update Employee.Status = Active
← Returns: { accessToken, refreshToken }
```

---

## Dependency Injection

### Infrastructure/DependencyInjection.cs
```csharp
services.AddDbContext<AppDbContext>(...);
services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
services.AddScoped<IJwtTokenService, JwtTokenService>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ICompanyRepository, CompanyRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
services.AddScoped<IInviteTokenRepository, InviteTokenRepository>();
```

### Application/DependencyInjection.cs
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
services.AddValidatorsFromAssembly(assembly);
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

---

## Configuration

### appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=workvaultdb;Username=workvault;Password=workvault123"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-at-least-32-characters-long",
    "Issuer": "WorkVault",
    "Audience": "WorkVault",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

---

## Common Commands

```bash
# Run the API
cd Server/src/WorkVault.API
dotnet run

# Add migration
dotnet ef migrations add MigrationName --project ../WorkVault.Infrastructure --output-dir Persistence/Migrations

# Update database
dotnet ef database update

# Drop database
dotnet ef database drop --force

# Check pending changes
dotnet ef migrations has-pending-model-changes --project ../WorkVault.Infrastructure
```

---

## Validation Rules

### Password Requirements (RegisterCommandValidator)
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character

### Validation Pipeline
All MediatR commands pass through `ValidationBehavior` which:
1. Finds all validators for the request type
2. Runs validators in parallel
3. Throws `ValidationException` if any failures
4. `ExceptionHandlingMiddleware` returns 400 with grouped errors

---

## Current Status

### Completed
- [x] Clean Architecture setup
- [x] Multi-tenant BaseEntity with CompanyId
- [x] Company, User, Role entities
- [x] JWT token generation service
- [x] Refresh token rotation flow
- [x] Register / Login / Refresh endpoints
- [x] Role-based authorization
- [x] Soft delete with global filters
- [x] CompanyId global query filter (tenant isolation)
- [x] Input validation with FluentValidation
- [x] Global exception handling middleware
- [x] MediatR validation pipeline behavior
- [x] Unit of Work pattern
- [x] Employee, Department, Designation entities
- [x] Employee creation with invite token
- [x] EmployeesController (POST)

### Pending
- [ ] SetPassword endpoint (accept invite)
- [ ] Employee CRUD (update, list, get by id)
- [ ] Department CRUD
- [ ] Designation CRUD
- [ ] Employee ID card with QR code
- [ ] Angular frontend

---

## Key Files to Read First

1. `SharedKernel/BaseEntity.cs` - Base class for all entities
2. `SharedKernel/Interfaces/IUnitOfWork.cs` - Unit of Work interface
3. `SharedKernel/Constants/SystemRoles.cs` - Role constants
4. `Infrastructure/Persistence/AppDbContext.cs` - DB context, filters, seeding, audit
5. `Application/Modules/Identity/Commands/Register/RegisterHandler.cs` - Registration flow
6. `Application/Modules/Employees/Commands/CreateEmployee/CreateEmployeeHandler.cs` - Employee creation
7. `Infrastructure/Auth/JwtTokenService.cs` - Token generation
8. `API/Controllers/AuthController.cs` - Auth endpoints
9. `Application/Common/Interfaces/ICurrentUserService.cs` - CurrentUser interface
10. `Application/Common/Behaviors/ValidationBehavior.cs` - Validation pipeline

---

## Notes for AI Assistants

1. **Always use `SystemRoles` constants** for role names and IDs
2. **All entities extend `BaseEntity`** except `RefreshToken`
3. **CQRS pattern**: Commands for writes, Queries for reads
4. **Repository pattern**: Interfaces in Domain, implementations in Infrastructure
5. **Unit of Work**: Inject `IUnitOfWork`, call `SaveChangesAsync` once at end of handler
6. **Soft deletes**: Set `IsDeleted = true`, never hard delete
7. **Multi-tenancy**: CompanyId auto-set by SaveChangesAsync from JWT claims
8. **JWT**: Access token = configurable minutes, Refresh token = configurable days with rotation
9. **Audit fields**: Auto-populated by `SaveChangesAsync` - no manual setting needed
10. **Validators**: Create `{Command}Validator.cs` alongside command files - auto-registered
11. **Employee Code**: Format `EMP-{year}-{sequence:D4}`, auto-generated
12. **Invite Flow**: User created inactive → InviteToken sent → SetPassword activates
