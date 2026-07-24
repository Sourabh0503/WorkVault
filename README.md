<div align="center">

# 🏢 WorkVault

### Your Entire Workplace, Secured in One Place

[![.NET](https://img.shields.io/badge/.NET_9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular_17+-DD0031?style=for-the-badge&logo=angular&logoColor=white)](https://angular.io/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL_16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![Render](https://img.shields.io/badge/Render-Live-46E3B7?style=for-the-badge&logo=render&logoColor=white)](https://render.com/)

<br/>

![Status](https://img.shields.io/badge/Status-Live_on_Render-brightgreen?style=flat-square)
![CI](https://img.shields.io/badge/CI-GitHub_Actions-2088FF?style=flat-square&logo=githubactions&logoColor=white)
![Tests](https://img.shields.io/badge/Tests-Testcontainers-success?style=flat-square)
![Architecture](https://img.shields.io/badge/Architecture-Modular_Monolith-blue?style=flat-square)
![License](https://img.shields.io/badge/License-Proprietary-red?style=flat-square)

<br/>

**A multi-tenant SaaS platform for HR, assets, attendance, and workplace operations.**
<br/>
Built for Indian SMEs with 50–500 employees. Competing with Keka, DarwinBox & GreytHR.

<br/>

[Getting Started](#-getting-started) •
[Architecture](#-architecture) •
[Modules](#-modules) •
[Roadmap](#-roadmap)

</div>

---

<br/>

## 🌐 Live

| Service | URL |
|---|---|
| 🖥️ **App (Angular)** | https://workvault.onrender.com |
| ⚙️ **API (.NET)** | https://workvault-api.onrender.com |
| ❤️ **Health check** | https://workvault-api.onrender.com/health |

Deployed on **Render** (Docker API + static frontend + managed PostgreSQL). Every push runs the **GitHub Actions** pipeline — build, integration tests (real Postgres via Testcontainers), and a Docker image build — and deploys are **gated behind green tests**.

---

<br/>

## ⚡ The Big Idea

> **AI-powered setup in minutes.** A new company admin registers, picks their industry, and gets departments, designations, asset types, and leave policies auto-generated. No competitor does this.

<br/>

## 🧬 Tech Stack

<table>
<tr>
<td align="center" width="140">
<img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/dotnetcore/dotnetcore-original.svg" width="40" height="40" alt=".NET"/>
<br/><b>.NET 9</b>
<br/><sub>Web API</sub>
</td>
<td align="center" width="140">
<img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/angularjs/angularjs-original.svg" width="40" height="40" alt="Angular"/>
<br/><b>Angular 17+</b>
<br/><sub>Frontend</sub>
</td>
<td align="center" width="140">
<img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/postgresql/postgresql-original.svg" width="40" height="40" alt="PostgreSQL"/>
<br/><b>PostgreSQL 16</b>
<br/><sub>Database</sub>
</td>
<td align="center" width="140">
<img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/docker/docker-original.svg" width="40" height="40" alt="Docker"/>
<br/><b>Docker</b>
<br/><sub>Containers</sub>
</td>
<td align="center" width="140">
<img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/docker/docker-plain.svg" width="40" height="40" alt="Render"/>
<br/><b>Render</b>
<br/><sub>Cloud (live)</sub>
</td>
</tr>
</table>

| Layer | Technology | Purpose |
|:---:|---|---|
| 🏗️ | **Modular Monolith + Clean Architecture** | Scalable without microservice complexity |
| 📨 | **MediatR (CQRS)** | Separated commands & queries |
| 🔐 | **JWT + Refresh Tokens + BCrypt** | Stateless, secure authentication |
| 📦 | **Entity Framework Core 9** | Type-safe ORM with global tenant filters |

<br/>

## 🏛️ Architecture

```
WorkVault/
│
├── 🎯 src/WorkVault.API              → Controllers, middleware, entry point
├── 📋 src/WorkVault.Application       → CQRS commands/queries, DTOs, validators
├── 🧩 src/WorkVault.Domain           → Entities, enums, repository interfaces
├── ⚙️ src/WorkVault.Infrastructure    → EF Core, repositories, auth, services
└── 🔗 src/WorkVault.SharedKernel     → BaseEntity, interfaces, constants
```

### 🔑 Design Principles

| Principle | How |
|---|---|
| **Multi-tenancy** | Every table has `CompanyId`. EF Core global filters ensure zero cross-tenant data leaks. |
| **Soft deletes** | Nothing is ever hard deleted. `IsDeleted` flag with automatic query filtering. |
| **CQRS** | Commands (writes) and Queries (reads) separated via MediatR handlers. |
| **Modular Monolith** | Feature folders (`Identity/`, `Employees/`, `Assets/`) — can split to microservices later. |

<br/>

## 🔄 CI/CD & Testing

Pipeline in `.github/workflows/ci.yml`, runs on every push and PR to `master`:

| Job | What it does |
|---|---|
| **server** | Restores/builds the solution and runs integration tests |
| **client** | `npm ci` + production Angular build |
| **docker** | Builds the API image (Dockerfile smoke test) |
| **deploy** | Test-gated — fires Render deploy hooks **only after** the three jobs pass, on `master` pushes |

**Integration tests** ([`Server/tests/WorkVault.IntegrationTests`](Server/tests/WorkVault.IntegrationTests)) spin up a **real PostgreSQL container** via [Testcontainers](https://testcontainers.com/) and prove multi-tenant isolation end to end — Company A cannot read or write Company B's data (404 via global query filters). The app runs EF Core migrations on startup and exposes `/health` for Render's checks.

<br/>

## 📦 Modules

| Module | Description | Status |
|:---:|---|:---:|
| 🔐 Identity | Company registration, JWT auth, roles, multi-tenancy | 🟡 Building |
| 👥 Employees | Profiles, lifecycle, ID card + QR, invite flow | ⬜ Next |
| 💻 Assets | Asset register, assignment, service requests | ⬜ Planned |
| ⏰ Attendance | Clock in/out, leave, timesheets, shifts | ⬜ Planned |
| 🏢 Bookings | Meeting rooms, desk booking, visitor mgmt | ⬜ Planned |
| 📊 Analytics | Dashboards, reports, audit log | ⬜ Planned |
| 📁 Documents | Employee document vault, compliance | ⬜ Planned |
| 🤖 AI Features | Smart onboarding, HR chatbot, anomaly detection | ⬜ Future |

<br/>

## 👥 Roles & Access

```
🔴 Super Admin    →  Platform-wide: manage all companies, billing
🟠 Company Admin  →  Own company: manage HR users, billing, settings
🟡 HR Manager     →  Own company: employees, assets, leaves, reports
🟢 Manager        →  Own team: approve leaves, view team data
🔵 Employee       →  Self only: own profile, request assets, clock in/out
```

<br/>

## 🗄️ Database Design

All entities inherit from `BaseEntity` — multi-tenancy and audit baked in from day one:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }      // 🔒 Tenant isolation
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public bool IsDeleted { get; set; }       // 🗑️ Soft delete
}
```

**Current tables:** `Companies` · `Users` · `Roles` (5 seeded system roles) · `RefreshTokens`

<br/>

## 🚀 Getting Started

### Prerequisites

| Tool | Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 9.0+ |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Latest |
| [Node.js](https://nodejs.org/) | 18+ *(for Angular, coming soon)* |

### 1️⃣ Clone

```bash
git clone https://github.com/Sourabh0503/WorkVault.git
cd WorkVault
```

### 2️⃣ Start Database

```bash
docker run --name workvault-db \
  -e POSTGRES_USER=workvault \
  -e POSTGRES_PASSWORD=workvault123 \
  -e POSTGRES_DB=workvaultdb \
  -p 5432:5432 \
  -d postgres:16
```

### 3️⃣ Configure

Create `src/WorkVault.API/appsettings.Development.json`:

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

### 4️⃣ Migrate & Run

```bash
dotnet ef database update \
  --project src/WorkVault.Infrastructure \
  --startup-project src/WorkVault.API

dotnet run --project src/WorkVault.API
```

### 5️⃣ Open Swagger

```
http://localhost:5080/swagger
```

<br/>

## 🔌 API Endpoints

### Authentication (`/api/auth`)

| Method | Endpoint | Description |
|:---:|---|---|
| POST | `/api/auth/register` | Register new company + admin user |
| POST | `/api/auth/login` | Login with email & password |
| POST | `/api/auth/refresh` | Refresh access token |

### Companies (`/api/companies`)

| Method | Endpoint | Description |
|:---:|---|---|
| POST | `/api/companies` | Create a new company |
| GET | `/api/companies/{id}` | Get company by ID |

<details>
<summary><b>Example: Register Company + Admin</b></summary>

```bash
POST /api/auth/register
```

```json
{
  "companyName": "WorkVault",
  "domain": "workvault.com",
  "industry": "Technology",
  "timezone": "Asia/Kolkata",
  "gstNumber": "22AAAAA0000A1Z5",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@workvault.com",
  "password": "SecureP@ss123"
}
```

**Response:**
```json
{
  "companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2..."
}
```
</details>

<details>
<summary><b>Example: Login</b></summary>

```bash
POST /api/auth/login
```

```json
{
  "email": "john@workvault.com",
  "password": "SecureP@ss123"
}
```

**Response:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2..."
}
```
</details>

<details>
<summary><b>Example: Refresh Token</b></summary>

```bash
POST /api/auth/refresh
```

```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2..."
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "bmV3IHJlZnJlc2ggdG9rZW4..."
}
```
</details>

<br/>

## 🗺️ Roadmap

### Phase 1 — Identity & Employees `In Progress`

- [x] Solution scaffolding — Clean Architecture
- [x] Multi-tenant `BaseEntity` with `CompanyId`
- [x] Company entity + POST/GET endpoints (CQRS)
- [x] Role entity with 5 seeded system roles
- [x] User entity with email uniqueness per tenant
- [x] JWT token generation service
- [x] Refresh token rotation flow
- [x] Register / Login / Refresh endpoints
- [x] `CompanyId` global query filter (tenant isolation)
- [x] Employee / Department / Designation CRUD + invite flow
- [x] Angular frontend — auth, dashboard, employees, departments
- [x] Integration tests (Testcontainers) proving tenant isolation
- [x] CI/CD (GitHub Actions) + live deploy on Render
- [ ] `DELETE /api/employees/{id}` (soft delete) — only missing endpoint
- [ ] Employee ID card with QR code
- [ ] Designation management UI + create-form dropdowns

### Phase 2 — Asset Management `Planned`

- [ ] Asset types & register
- [ ] Assignment with digital acknowledgement
- [ ] Service & new asset requests

### Phase 3–7 `Future`

- [ ] Attendance, leave, timesheets
- [ ] Meeting rooms, desk & parking booking
- [ ] Analytics dashboards & reporting
- [ ] Document vault & compliance (DPDP Act)
- [ ] AI-powered features & integrations

<br/>

## 💰 Planned Pricing

| Plan | Price | Employees |
|:---:|:---:|:---:|
| 🆓 Free | ₹0/mo | Up to 30 |
| 🟢 Starter | ₹2,999/mo | Up to 100 |
| 🔵 Growth | ₹5,999/mo | Up to 200 |
| 🟣 Enterprise | Custom | 200+ |

<br/>

---

<div align="center">

**Built with ❤️ as a learning project & future SaaS product**

<br/>

[![workvault.co.in](https://img.shields.io/badge/🌐_workvault.co.in-Visit-4361ee?style=for-the-badge)](https://workvault.co.in)

</div>
