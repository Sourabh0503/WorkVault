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
- Validation: FluentValidation (configured, not yet implemented)

---

## Solution Structure

```
Server/src/
├── WorkVault.API/                    # Entry point, controllers, middleware
│   ├── Controllers/
│   │   ├── AuthController.cs         # POST /api/auth/register, login, refresh
│   │   └── CompaniesController.cs    # POST /api/companies (SuperAdmin only), GET by ID
│   ├── Program.cs                    # App configuration, DI, middleware pipeline
│   └── appsettings.Development.json  # Connection string, JWT settings
│
├── WorkVault.Application/            # Business logic, CQRS handlers, DTOs
│   ├── Common/
│   │   ├── IJwtTokenService.cs       # Token generation interface
│   │   └── JwtSettings.cs            # JWT configuration POCO
│   └── Modules/
│       └── Identity/
│           ├── Commands/
│           │   ├── Login/
│           │   │   ├── LoginCommand.cs
│           │   │   └── LoginHandler.cs
│           │   ├── Register/
│           │   │   ├── RegisterCommand.cs    # Company + Admin registration
│           │   │   └── RegisterHandler.cs
│           │   ├── RegisterCompany/
│           │   │   ├── RegisterCompanyCommand.cs
│           │   │   └── RegisterCompanyHandler.cs
│           │   └── RefreshTokens/
│           │       ├── RefreshTokenCommand.cs
│           │       └── RefreshTokenHandler.cs
│           ├── Queries/
│           │   └── GetCompanyById/
│           │       ├── GetCompanyByIdQuery.cs
│           │       └── GetCompanyByIdHandler.cs
│           └── DTOs/
│               └── CompanyDto.cs
│
├── WorkVault.Domain/                 # Entities, enums, repository interfaces
│   └── Modules/
│       └── Identity/
│           ├── Company.cs            # Tenant entity
│           ├── User.cs               # User with RoleId, CompanyId
│           ├── Role.cs               # System roles
│           ├── RefreshToken.cs       # JWT refresh tokens (not BaseEntity)
│           └── Interfaces/
│               ├── ICompanyRepository.cs
│               ├── IUserRepository.cs
│               └── IRefreshTokenRepository.cs
│
├── WorkVault.Infrastructure/         # EF Core, repositories, external services
│   ├── Auth/
│   │   └── JwtTokenService.cs        # JWT + refresh token generation
│   ├── Modules/
│   │   └── Identity/
│   │       ├── CompanyRepository.cs
│   │       ├── UserRepository.cs
│   │       └── RefreshTokenRepository.cs
│   ├── Persistence/
│   │   ├── AppDbContext.cs           # DbContext with soft delete filters, role seeding
│   │   └── Migrations/
│   │       └── *_InitialCreate.cs    # Single migration with all tables
│   └── DependencyInjection.cs        # Infrastructure DI registration
│
└── WorkVault.SharedKernel/           # Shared base classes, constants, interfaces
    ├── BaseEntity.cs                 # Id, CompanyId, CreatedAt, UpdatedAt, CreatedBy, IsDeleted
    ├── Constants/
    │   └── SystemRoles.cs            # RoleType enum, role GUIDs, role name constants
    └── Interfaces/
        └── IRepository.cs            # Generic repository interface
```

---

## Database Schema

### Tables

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
├── ExpiresAt (DateTime)
└── IsRevoked (bool)
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
// BaseEntity.cs
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

### 3. CQRS with MediatR
- **Commands**: Write operations (Register, Login, RefreshToken)
- **Queries**: Read operations (GetCompanyById)
- All handlers in `Application/Modules/{Module}/Commands|Queries/`

### 4. Role-Based Authorization
```csharp
// Use const strings for [Authorize] attributes
[Authorize(Roles = SystemRoles.SuperAdminRole)]

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

## Authentication Flow

### 1. Register (Company + Admin)
```
POST /api/auth/register
→ RegisterCommand
→ RegisterHandler
  1. Creates Company via RegisterCompanyCommand
  2. Creates User (CompanyAdmin role, BCrypt hashed password)
  3. Generates JWT access token (15 min)
  4. Generates refresh token (7 days)
  5. Saves RefreshToken to database
← Returns: { companyId, accessToken, refreshToken }
```

### 2. Login
```
POST /api/auth/login
→ LoginCommand { email, password }
→ LoginHandler
  1. Find user by email (includes Role)
  2. Verify password with BCrypt
  3. Generate JWT access token
  4. Generate and save refresh token
  5. Update LastLogin
← Returns: { userId, companyId, accessToken, refreshToken }
```

### 3. Refresh Token
```
POST /api/auth/refresh
→ RefreshTokenCommand { refreshToken }
→ RefreshTokenHandler
  1. Find token in DB (includes User.Role)
  2. Check if expired or revoked
  3. Revoke old token (rotation)
  4. Generate new access + refresh tokens
  5. Save new refresh token
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

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | Public | Register company + admin user |
| POST | `/api/auth/login` | Public | Login, returns tokens |
| POST | `/api/auth/refresh` | Public | Refresh access token |
| POST | `/api/companies` | SuperAdmin | Create company directly |
| GET | `/api/companies/{id}` | Public | Get company by ID |

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

## Dependency Injection

### Infrastructure/DependencyInjection.cs
```csharp
services.AddDbContext<AppDbContext>(...);
services.Configure<JwtSettings>(...);
services.AddScoped<IJwtTokenService, JwtTokenService>();
services.AddScoped<ICompanyRepository, CompanyRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
```

### Application (via MediatR)
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));
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

## Current Status (Phase 1)

### Completed
- [x] Clean Architecture setup
- [x] Multi-tenant BaseEntity with CompanyId
- [x] Company, User, Role entities
- [x] JWT token generation service
- [x] Refresh token rotation flow
- [x] Register / Login / Refresh endpoints
- [x] Role-based authorization with enums
- [x] Soft delete with global filters
- [x] CompanyId global query filter (tenant isolation in queries)
- [x] Input validation with FluentValidation
- [x] Global exception handling middleware
- [x] MediatR validation pipeline behavior
- [x] Common/ folder structure: Interfaces/, Settings/, Behaviors/

### Pending
- [ ] Employee CRUD + invite flow
- [ ] Employee ID card with QR code
- [ ] Angular frontend

---

## Key Files to Read First

1. `SharedKernel/BaseEntity.cs` - Base class for all entities
2. `SharedKernel/Constants/SystemRoles.cs` - Role enum and constants
3. `Infrastructure/Persistence/AppDbContext.cs` - DB context, filters, seeding
4. `Application/Modules/Identity/Commands/Register/RegisterHandler.cs` - Main registration flow
5. `Infrastructure/Auth/JwtTokenService.cs` - Token generation
6. `API/Controllers/AuthController.cs` - Auth endpoints
7. `Application/Common/Interfaces/ICurrentUserService.cs` - CurrentUser Interface
8. `Application/Common/Behaviors/ValidationBehavior.cs` - Validation pipeline
9. `API/Services/CurrentUserService.cs` - CurrentUser Implementation using JWT
10. `API/Middleware/ExceptionHandlingMiddleware.cs` - Exception middleware

---

## Notes for AI Assistants

1. **Always use `SystemRoles` constants** for role names and IDs
2. **All entities extend `BaseEntity`** except `RefreshToken`
3. **CQRS pattern**: Commands for writes, Queries for reads
4. **Repository pattern**: Interfaces in Domain, implementations in Infrastructure
5. **Soft deletes**: Set `IsDeleted = true`, never hard delete
6. **Multi-tenancy**: Always include `CompanyId` in operations
7. **JWT**: Access token = 15 min, Refresh token = 7 days with rotation
