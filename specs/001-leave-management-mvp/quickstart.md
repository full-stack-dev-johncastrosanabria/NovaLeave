# Quickstart — NovaLeave MVP (Developer Guide)

**Date**: 2026-07-28 (revised 2026-07-28)
**Feature**: 001-leave-management-mvp

> **Status**: No production code has been written yet. This guide distinguishes **existing** items from **planned** items. Do not treat planned commands as if they already work.

---

## Prerequisites

| Prerequisite | Status | Notes |
|---|---|---|
| .NET 10 SDK | **Required** | Install before any development begins |
| SQL Server (local or container) | **Required** (planned) | Needed for integration tests and local development |
| Node.js / npm or LibMan | **Planned** | Bootstrap 5.3.x asset management |
| Docker | **Planned** | Containerized development; E2E Playwright tests |

---

## Repository Structure (Planned — Not Yet Created)

No `src/` or `tests/` directories exist in the repository today. When implementation begins, the planned structure is:

```text
src/
  NovaLeave.Domain/
  NovaLeave.Application/
  NovaLeave.Infrastructure/
  NovaLeave.Web/
tests/
  NovaLeave.UnitTests/
  NovaLeave.IntegrationTests/
  NovaLeave.EndToEndTests/
docs/
  adr/
  diagrams/
  operations/
docker/
```

---

## Environment Configuration

These values must be set before the application can run. **No value is hard-coded in application code**; the official values below were decided in [DR-001](../../docs/adr/DR-001-runtime-configuration-values.md) (2026-07-29).

| Key | Description | Official value (DR-001) | Required In |
|-----|-------------|-------------------------|-------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | environment-specific | All non-test environments |
| `NovaLeave:PendingRequestTimeoutDays` | Days before a Pending request is cancelled (positive int) | **14** | All environments |
| `NovaLeave:SessionTimeoutMinutes` | Authenticated session lifetime (positive int) | **30** | All environments |
| `NovaLeave:SeedDemoUsers` | `true` to seed demo identities at startup | `true` in Dev/Staging | Development / Staging **only** — **must be `false` or absent in Production** |
| Accrual & timeout job cadence | Scheduler frequency for the idempotent accrual and timeout scans | **Daily 00:05 UTC** | All environments |

Configuration validated at startup. Application will not start with missing or invalid `PendingRequestTimeoutDays` or `SessionTimeoutMinutes`. Development/Staging may carry the DR-001 values in `appsettings.Development.json`; Production supplies them through its configuration/secret store.

---

## Local database with Docker (macOS / Linux)

The team runs a mix of macOS (Apple Silicon) and Windows hosts. Windows developers can
use SQL Server LocalDB and skip this section entirely — the integration-test fixture
defaults to LocalDB. On macOS and Linux, LocalDB does not exist, so use the committed
`docker-compose.yml`:

```bash
cp .env.example .env          # then set MSSQL_SA_PASSWORD to a value of your own
docker compose up -d          # starts SQL on ${NOVALEAVE_SQL_PORT}, default 14333
```

| Setting | Notes |
|---|---|
| `MSSQL_SA_PASSWORD` | Local development only. `.env` is gitignored — never commit it. |
| `NOVALEAVE_SQL_PORT` | Defaults to **14333**, not 1433, so it cannot collide with another local SQL instance. |
| `NOVALEAVE_SQL_IMAGE` | `azure-sql-edge` on Apple Silicon (arm64-native); `mssql/server:2022-latest` on amd64. |

Stop with `docker compose down` (keeps data) or `docker compose down -v` (deletes the volume).

**Applying migrations:** `Microsoft.EntityFrameworkCore.Design` is referenced by
`NovaLeave.Infrastructure`, not by `NovaLeave.Web`, so `dotnet ef` must be pointed at
Infrastructure as the startup project. The integration tests do not need this — they
migrate the database themselves.

**Verified on 2026-08-03** (Apple M3, Azure SQL Edge): full suite green —
37 unit + 92 integration + 5 E2E = **134 passed, 0 failed**.

---

## Planned Setup Commands

All commands below are **planned** — they will work once implementation begins.

### 1. Restore and build
```
dotnet restore
dotnet build
```

### 2. Apply EF Core migrations (planned)
```
dotnet ef database update \
  --project src/NovaLeave.Infrastructure \
  --startup-project src/NovaLeave.Web
```

### 3. Run the web application (planned)
```
dotnet run --project src/NovaLeave.Web
```
Then browse to `https://localhost:5001` and log in at `/Identity/Account/Login`.

### 4. Run unit tests (planned)
```
dotnet test tests/NovaLeave.UnitTests
```

### 5. Run integration tests (requires SQL Server)
```
dotnet test tests/NovaLeave.IntegrationTests
```
Integration tests use a real SQL Server database and apply the EF Core migrations
themselves (`Database.MigrateAsync()` in `Support/IntegrationTestDatabase.cs`), so no
manual `dotnet ef database update` is needed before running them. Testcontainers MAY be
used when the test objective requires it; it is not mandatory.

**Where the connection string comes from** — `Support/SqlServerFixture.cs` reads the
`NOVALEAVE_TEST_SQLSERVER` environment variable and falls back to
`(localdb)\MSSQLLocalDB` when it is unset:

| Platform | What to do |
|---|---|
| **Windows** | Nothing. The LocalDB fallback works out of the box. |
| **macOS / Linux** | LocalDB does not exist on these platforms. Start the container (see [Local database with Docker](#local-database-with-docker-macos--linux)) and export `NOVALEAVE_TEST_SQLSERVER` before running the tests, otherwise every database-backed test fails with a connection error. |

```bash
# macOS / Linux
set -a; . ./.env; set +a
export NOVALEAVE_TEST_SQLSERVER="Server=localhost,${NOVALEAVE_SQL_PORT};Database=NovaLeave_Test;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;MultipleActiveResultSets=true;Pooling=false;Connect Timeout=60"
dotnet test tests/NovaLeave.IntegrationTests
```

### 6. Run E2E tests (planned — requires running application and demo seed data)
```
dotnet test tests/NovaLeave.EndToEndTests
```
Requires `NovaLeave:SeedDemoUsers=true` in the test environment and a running application instance.

### 7. Manual Quality and Security Gate (planned — blocking)

NovaLeave MVP does not use CI/CD or automated deployment pipelines. Before merge, handoff, release, or acceptance, execute and document the Manual Quality and Security Gate in `docs/operations/manual-quality-gate.md`.

If Bash, Python, Mermaid validation, static security analysis, dependency scanning, or license-review tooling is unavailable, record the affected check as `NOT EXECUTED`. Do not report an unavailable official validator as `PASS`; PowerShell or manual equivalents are equivalent checks only.

---

## Demo Identities (Planned — CFG-003)

When `NovaLeave:SeedDemoUsers=true`, the application seeds these identities at startup:

| Email | Role(s) | `canResolveRequests` | Password | Status |
|-------|---------|---------------------|----------|--------|
| `user@demo` | User | N/A | `Demo123!` | Active |
| `approver@demo` | Approver | `true` | `Demo123!` | Active |
| `hr@demo` | HR | N/A | `Demo123!` | Active |
| `multi@demo` | User + Approver + HR | `true` | `Demo123!` | Active |

Employment start date: 12 months before seed date (ensures ≥ 12 accrued days).

The login screen includes a "Cuenta" dropdown that pre-fills the email field. It does not authenticate automatically.

---

## Key Routes

| Route | Method | Description |
|-------|--------|-------------|
| `/Identity/Account/Login` | GET, POST | ASP.NET Core Identity login |
| `/Identity/Account/Logout` | POST | Secure logout |
| `/Identity/Account/AccessDenied` | GET | Branded 403 page |
| `/mis-solicitudes` | GET | User — own requests |
| `/mis-solicitudes/crear` | GET, POST | User — create request |
| `/mis-solicitudes/{id}/editar` | GET, POST | User — edit Pending request |
| `/mis-solicitudes/{id}` | GET | User — request detail |
| `/saldo` | GET | User — balance and history |
| `/calendario` | GET | Calendar (User / Approver) |
| `/aprobaciones` | GET | Approver — pending queue |
| `/aprobaciones/{id}` | GET | Approver — resolution detail |
| `/aprobaciones/{id}/aprobar` | POST | Approve request |
| `/aprobaciones/{id}/rechazar` | POST | Reject request |
| `/aprobaciones/{id}/desactivar` | POST | Deactivate approved request |
| `/aprobaciones/historial` | GET | Approver — resolution history |
| `/rrhh` | GET | HR — dashboard |
| `/rrhh/solicitudes` | GET | HR — org-wide request list |
| `/rrhh/solicitudes/{id}` | GET | HR — request detail |
| `/rrhh/calendario` | GET | HR — dedicated read-only organizational calendar for all vacation requests |
| `/rrhh/saldos` | GET | HR — balances list |
| `/rrhh/saldos/{userId}` | GET | HR — balance movements |
| `/rrhh/auditoria` | GET | HR — audit log |
| `/rrhh/aprobadores` | GET | HR — approver list |
| `/rrhh/aprobadores/{id}/capacidad` | GET, POST | HR — toggle canResolveRequests |

Full UC-to-route mapping: `specs/001-leave-management-mvp/contracts/uc-contracts.md`

---

## Architecture Notes

- `TimeProvider` (.NET 10 built-in) is the sole time abstraction. All time-dependent Domain and Application rules use it. No `DateTime.Now` or `DateTime.UtcNow` in Domain or Application.
- All mutable aggregates use `RowVersion` (`rowversion` SQL Server type, `byte[]` in EF Core) for optimistic concurrency.
- Background services (timeout cancellation, monthly accrual) are idempotent and bounded. They can run repeatedly without side effects.
- Security events are recorded as Serilog structured log entries. No `SecurityEvent` database table.
- No MediatR, AutoMapper, or custom pipeline behaviors.

---

## Create a Migration (Planned)

```
dotnet ef migrations add <MigrationName> \
  --project src/NovaLeave.Infrastructure \
  --startup-project src/NovaLeave.Web
```

---

## Contact / References

- Approved sources: `specs/001-leave-management-mvp/spec.md`, `frontend-design-spec.md`, `specs/002-role-based-frontend-views/spec.md`, `.specify/memory/constitution.md`, `docs/use-cases.md`
- Design decisions and conflict resolutions: `specs/001-leave-management-mvp/research.md`
- Data model: `specs/001-leave-management-mvp/data-model.md`
- UC contracts: `specs/001-leave-management-mvp/contracts/uc-contracts.md`

---

## Development Notes

- Use the built-in .NET `TimeProvider` directly for tests; do not introduce `IClock`, `ITimeProvider`, `ISystemDateProvider`, or a wrapper service.
- Keep Domain project dependency-free of EF Core and ASP.NET types.
- Use `RowVersion` in forms as a hidden field for concurrency.
- Use structured logging (`ILogger<T>`) and enrich with `CorrelationId` middleware.

---

## Useful Commands (Planned)

- Create migration:
  - `dotnet ef migrations add <Name> --project src/NovaLeave.Infrastructure --startup-project src/NovaLeave.Web`
- Build container image:
  - `docker build -t novelave:web -f docker/Dockerfile .`

---

## Contact

For PO questions (timeout X, accrual schedule), see `specs/001-leave-management-mvp/research.md` and contact the product owner.
