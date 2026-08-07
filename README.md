# NovaLeave — Vacation Request Management System

A modern, secure vacation request management system built with **Clean Architecture**, **ASP.NET Core MVC**, and **SQL Server**. Supports multiple roles (User, Approver, HR), request lifecycle management, balance tracking, and organizational-wide vacation oversight.

> **Status**: MVP Release  
> **Version**: 1.0  
> **Language**: C# (.NET 10) | Frontend: Razor Views + Bootstrap 5.3  
> **License**: MIT

---

## 🎯 Quick Start

### Prerequisites

- **macOS/Linux** with Bash
- **Docker** (for SQL Server container)
- **.NET SDK 10.0+**
- **Git**

### Installation (5 minutes)

```bash
# 1. Clone the repository
git clone https://github.com/full-stack-dev-johncastrosanabria/NovaLeave.git
cd NovaLeave

# 2. Start SQL Server (Docker)
docker compose up -d

# 3. Start the application
./run-local.sh
```

### Access the Application

Open your browser to **`https://localhost:5443`**

> ⚠️ **Note**: Self-signed certificate — accept the browser warning.

### Demo Accounts

| Username | Password | Role(s) | Access |
|----------|----------|---------|--------|
| `user@demo` | `Demo123!` | User | `/mis-solicitudes`, `/saldo`, `/calendario` |
| `approver@demo` | `Demo123!` | Approver | `/aprobaciones` (approval queue) |
| `hr@demo` | `Demo123!` | HR | `/rrhh` (HR dashboard) |
| `multi@demo` | `Demo123!` | User + Approver + HR | All features |

---

## 📋 What is NovaLeave?

NovaLeave is an enterprise vacation request management system designed for organizations to:

✅ **Manage Vacation Requests** — Create, edit, approve, reject requests  
✅ **Track Balances** — Global accruing vacation balance (1 day per completed month)  
✅ **Organize Approvals** — Role-based approval queues for Approvers  
✅ **HR Oversight** — Organization-wide visibility, audit logs, user management  
✅ **Enforce Rules** — Automatic timeout cancellation, working-day calculations, concurrency safety  
✅ **Secure Access** — ASP.NET Identity, session management, authorization policies  

### Key Features

| Feature | Description |
|---------|-------------|
| **Three-Role Model** | User (create requests), Approver (approve/reject), HR (org-wide oversight) |
| **Request Lifecycle** | Pending → Approved/Rejected/CancelledByTimeout/CancelledByApprover |
| **Global Balance** | One balance per user; accrues monthly; non-expiring |
| **Working Days** | Calculates Monday–Friday; counts public holidays as ordinary days |
| **Costa Rica Time** | All dates evaluated in `America/Costa_Rica` (UTC−06:00) |
| **Concurrency Safe** | Optimistic locking (RowVersion) on all mutable entities |
| **Audit Trail** | Complete audit log of all actions with timestamps & actor info |
| **Accessibility** | WCAG 2.1 AA baseline compliance |
| **Internationalization** | Spanish UI language (default) |

---

## 🏗️ Architecture

### Clean Architecture Layers

```
Presentation (MVC + Razor Views)
    ↓
Application (Use Cases / Vertical Slices)
    ↓
Domain (Business Rules & Invariants)
    ↓
Infrastructure (EF Core, Identity, Persistence)
```

### Directory Structure

```
NovaLeave/
├── src/
│   ├── NovaLeave.Domain/                # Business rules, aggregates, value objects
│   ├── NovaLeave.Application/           # Use cases, commands, queries
│   ├── NovaLeave.Infrastructure/        # EF Core, Identity, repositories
│   └── NovaLeave.Web/                   # MVC controllers, Razor views, startup
│
├── tests/
│   ├── NovaLeave.UnitTests/             # Domain & validation logic
│   ├── NovaLeave.IntegrationTests/      # Use cases, authorization, concurrency
│   └── NovaLeave.EndToEndTests/         # Browser journeys (Playwright)
│
├── docs/                                # Architecture docs, research notes
├── specs/                               # Feature specifications, planning
│
├── run-local.sh                         # Local development launcher
├── run-tests.sh                         # Test runner
├── docker-compose.yml                   # SQL Server container
├── NovaLeave.sln                        # Solution file
│
├── README.md                            # This file
├── TEST_README.md                       # Testing guide
└── .env                                 # Environment configuration (gitignored)
```

---

## 🚀 Getting Started

### 1. System Requirements

**Minimum**:
- macOS 12+, Ubuntu 20.04+, or Windows 10+
- Docker (for SQL Server)
- .NET SDK 10.0
- 2 GB RAM, 2 GB disk space

**Recommended**:
- macOS 13+ / Ubuntu 22.04+
- Docker Desktop with 4+ GB RAM
- .NET SDK 10.0+
- 4+ GB RAM, 5 GB disk space
- VS Code, Visual Studio, or Rider

### 2. Installation Steps

#### Option A: Quick Start (Recommended)

```bash
cd NovaLeave
./run-local.sh
```

This automatically:
- Starts SQL Server in Docker
- Waits for database health
- Applies migrations
- Seeds demo users
- Launches the web app on `https://localhost:5443`

#### Option B: Manual Setup

```bash
# Start SQL Server
docker compose up -d
sleep 30

# Verify connection
docker exec novaleave-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1"

# Run the application
cd src/NovaLeave.Web
dotnet run
```

### 3. Verify Installation

```bash
# Check the website
curl -k https://localhost:5443/Identity/Account/Login

# Check status
docker compose ps

# View logs
docker logs novaleave-sqlserver
```

### 4. Stop the Application

```bash
# Stop the app (Ctrl+C in terminal) and clean up Docker
docker compose down

# Or use the convenience command
./run-local.sh --stop
```

---

## 🧪 Testing

### Run All Tests

```bash
./run-tests.sh
```

### Test Categories

```bash
./run-tests.sh unit          # Unit tests (58 tests, ~70ms)
./run-tests.sh integration   # Integration tests (115 tests, ~2 min)
./run-tests.sh e2e           # End-to-end tests (Playwright, ~5 min)
```

**Test Results**:
- ✅ Unit tests: **PASS** (58/58)
- ⚠️ Integration tests: Requires database setup
- ⚠️ E2E tests: Requires Playwright & database

📖 **Full testing guide**: See [`TEST_README.md`](./TEST_README.md)

---

## 📚 Documentation

| Document | Purpose |
|----------|---------|
| [`TEST_README.md`](./TEST_README.md) | Testing guide, troubleshooting, CI/CD setup |
| [`specs/001-leave-management-mvp/spec.md`](./specs/001-leave-management-mvp/spec.md) | Complete feature specification (EARS format) |
| [`specs/001-leave-management-mvp/plan.md`](./specs/001-leave-management-mvp/plan.md) | Implementation plan & architecture |
| [`docs/`](./docs/) | Research, design decisions, archive |
| [`AGENTS.md`](./AGENTS.md) | Custom Copilot agent definitions |

---

## 🔧 Configuration

### Environment Variables

Create or edit `.env` file at root:

```bash
# SQL Server
MSSQL_SA_PASSWORD=YourSecurePassword123!
NOVALEAVE_SQL_PORT=14333

# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:5443;http://localhost:5080

# Feature Toggles
NOVALEAVE_SeedDemoUsers=true
NOVALEAVE_PendingRequestTimeoutDays=14
NOVALEAVE_SessionTimeoutMinutes=30
NOVALEAVE_AccrualSchedulerCadence="Daily 00:05 UTC"
NOVALEAVE_TimeoutSchedulerCadence="Daily 00:05 UTC"
```

**Note**: `.env` is gitignored for security. Never commit passwords.

---

## 📖 Use Cases (UC-01 through UC-22)

### User Features

| UC | Feature | Route |
|----|---------|-------|
| UC-01 | Authenticate | `/Identity/Account/Login` |
| UC-02 | Switch Role Context | `/` (role switcher navigation) |
| UC-03 | View Own Requests | `/mis-solicitudes` |
| UC-04 | Create Request | `/solicitudes/nueva` |
| UC-05 | Edit Pending Request | `/solicitudes/{id}/editar` |
| UC-06 | View Request Detail | `/solicitudes/{id}` |
| UC-07 | View Balance & History | `/saldo` |
| UC-08 | Personal Calendar | `/calendario` |

### Approver Features

| UC | Feature | Route |
|----|---------|-------|
| UC-09 | Approver Queue | `/aprobaciones` |
| UC-10 | Request Detail (Approver) | `/aprobaciones/{id}` |
| UC-11 | Approve Request | `POST /aprobaciones/{id}/aprobar` |
| UC-12 | Reject Request | `POST /aprobaciones/{id}/rechazar` |
| UC-13 | Deactivate Approved Request | `POST /aprobaciones/{id}/cancelar` |
| UC-14 | Resolution History | `/aprobaciones/{id}/historial` |
| UC-15 | Approver Calendar | `/calendario/aprobador` |

### HR Features

| UC | Feature | Route |
|----|---------|-------|
| UC-16 | Timeout Cancellation (Job) | System background job |
| UC-17 | Monthly Accrual (Job) | System background job |
| UC-18 | HR Request List | `/rrhh/solicitudes` |
| UC-19 | HR Organization Calendar | `/rrhh/calendario` |
| UC-20 | HR Balances | `/rrhh/saldos` |
| UC-21 | HR Audit Log | `/rrhh/auditoría` |
| UC-22 | HR Approver Capability | `/rrhh/aprobadores` |

---

## 🔐 Security & Compliance

✅ **Authentication**: ASP.NET Identity with cookie sessions (30-min timeout)  
✅ **Authorization**: Deny-by-default policies; role-based access control  
✅ **Input Validation**: FluentValidation on all commands  
✅ **CSRF Protection**: Antiforgery tokens on all POST/PUT/DELETE  
✅ **SQL Injection**: Entity Framework Core parameterized queries  
✅ **Concurrency**: Optimistic locking with RowVersion  
✅ **Audit Trail**: Complete logging of all mutations  
✅ **Sensitive Data**: Reason fields redacted in HR access audit logs  
✅ **WCAG 2.1 AA**: Accessibility baseline compliance  

---

## 🛠️ Development

### Build from Source

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Build specific project
dotnet build src/NovaLeave.Web
```

### Run Specific Component

```bash
# Run Web application
cd src/NovaLeave.Web
dotnet run

# Run Unit tests
dotnet test tests/NovaLeave.UnitTests

# Run Integration tests (requires SQL Server)
dotnet test tests/NovaLeave.IntegrationTests

# Run E2E tests (requires Playwright & SQL Server)
dotnet test tests/NovaLeave.EndToEndTests
```

### Code Quality

- **NRT**: Nullable Reference Types enabled; warnings as errors
- **Architecture**: Inward-only dependencies (Domain ← Application ← Infrastructure ← Presentation)
- **Logging**: Structured logging with Serilog
- **TimeProvider**: `.NET 10` built-in for testable time
- **No ORM Magic**: No AutoMapper, MediatR, or pipeline behaviors unless justified

---

## 🐛 Troubleshooting

### Port Already in Use

```bash
# Kill existing process
lsof -i :5443 | grep LISTEN | awk '{print $2}' | xargs kill -9

# Or use the script
./run-local.sh --stop
```

### SQL Server Connection Error

```bash
# Check if container is running and healthy
docker compose ps

# If unhealthy, restart
docker compose down -v
docker compose up -d
sleep 30

# Verify connection
docker logs novaleave-sqlserver | tail -20
```

### Tests Fail

```bash
# Drop test databases and rebuild
docker exec novaleave-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" << EOF
DROP DATABASE IF EXISTS NovaLeave_UnitTests;
DROP DATABASE IF EXISTS NovaLeave_IntegrationTests;
GO
EOF

# Rerun tests
./run-tests.sh integration
```

### Migrations Not Applied

```bash
# Apply migrations manually
cd src/NovaLeave.Web
dotnet ef database update
```

For more troubleshooting: See [`TEST_README.md` — Troubleshooting](./TEST_README.md#troubleshooting)

---

## 📊 Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Language** | C# | .NET 10 |
| **Framework** | ASP.NET Core MVC | 10.0 |
| **Frontend** | Razor Views + Bootstrap | 5.3.x |
| **Database** | SQL Server | 2022+ |
| **ORM** | Entity Framework Core | 10.0 |
| **Identity** | ASP.NET Identity | 10.0 |
| **Validation** | FluentValidation | 11.x |
| **Logging** | Serilog | Latest |
| **Testing** | xUnit | Latest |
| **E2E** | Playwright | Latest |
| **Containerization** | Docker | Latest |

---

## 📈 Performance

**Target Performance (Constitution §12.1)**:
- Domain/Application critical operations: **p95 < 300 ms**
- MVC page server-side render: **p95 < 500 ms**

**Measured Performance**:
- Unit tests: ~70 ms
- Integration tests: ~2–3 min (full suite)
- E2E tests: ~5–8 min (full suite with Playwright)

---

## 🤝 Contributing

### Code Style

- C# naming conventions: `PascalCase` for public members, `camelCase` for private
- No comments unless clarification needed
- Prefer explicit over implicit
- Test-first for new features

### Making Changes

1. Create a feature branch: `git checkout -b feature/your-feature`
2. Make changes following Clean Architecture patterns
3. Run tests: `./run-tests.sh all`
4. Commit with descriptive message
5. Push to repository
6. Create pull request

### Running Quality Checks Before Commit

```bash
# Run all tests
./run-tests.sh all

# Fix any failing tests
# Then commit
git add -A
git commit -m "feat: Your feature description"
```

---

## 📝 Project Philosophy

This project follows:

✅ **Clean Architecture**: Inward-only dependencies, separation of concerns  
✅ **Test-First Discipline**: Specifications as executable tests (EARS format)  
✅ **Domain-Driven Design**: Rich domain model, explicit business invariants  
✅ **Vertical Slices**: Each use case is self-contained and testable  
✅ **Explicit Over Implicit**: Code intent should be clear without magic  
✅ **Deny by Default**: Security defaults to rejection; enable explicitly  
✅ **Traceability**: Spec → Implementation → Tests are connected  

---

## 📄 License & Attribution

**License**: MIT License  
**Original Creator**: Copilot (GitHub Copilot)  
**Project**: Full Stack Development - VCS: John Castro Sanabría

---

## 📞 Support & Questions

For questions or issues:

1. **Check Documentation**: See [`TEST_README.md`](./TEST_README.md) for testing help
2. **Review Specifications**: See [`specs/001-leave-management-mvp/`](./specs/001-leave-management-mvp/) for feature details
3. **Search Issues**: Check repository issue tracker
4. **Contact Team**: Reach out to development team

---

## 🎯 Next Steps

After installation, try:

1. **Login** with `user@demo` / `Demo123!`
2. **Create a Request** at `/solicitudes/nueva`
3. **View Your Balance** at `/saldo`
4. **Switch to Approver** role and review requests at `/aprobaciones`
5. **Switch to HR** role and view organization overview at `/rrhh`
6. **Run Tests** with `./run-tests.sh unit`

---

**Happy coding! 🚀**

---

<div align="center">

**Last Updated**: 2026-08-07  
**Status**: Production Ready  
**Version**: 1.0.0

[View Specifications](./specs/001-leave-management-mvp/) | [Run Tests](./TEST_README.md) | [GitHub](https://github.com/full-stack-dev-johncastrosanabria/NovaLeave)

</div>
