# Quickstart — NovaLeave MVP (Developer Guide)

**Date**: 2026-07-28 (revised 2026-08-03)
**Feature**: 001-leave-management-mvp

> **Status**: Implementation for UC-01 through UC-22 exists in `src/` and
> `tests/`. Phase 11 validation evidence is recorded in
> `phase-11-validation-evidence.md`.

---

## Prerequisites

| Prerequisite | Status | Notes |
|---|---|---|
| .NET 10 SDK | **Required** | Install before any development begins |
| SQL Server (local or container) | **Required** | Needed for integration tests and local development |
| Bootstrap assets | **Present** | Managed under `src/NovaLeave.Web/wwwroot/lib/bootstrap/` |
| Docker | **Required on macOS / Linux** | Hosts the local SQL Server; Windows hosts may use LocalDB instead (see below) |

---

## Repository Structure

The implemented repository structure is:

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

The team runs a mix of macOS (Apple Silicon) and Windows hosts. Windows developers may use
SQL Server LocalDB and skip this section — the integration-test fixture falls back to LocalDB.
LocalDB does not exist on macOS or Linux, so use the committed `docker-compose.yml`:

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

---

## Setup Commands

### 1. Restore and build
```
dotnet restore
dotnet build
```

### 2. Apply EF Core migrations
```
dotnet ef database update \
  --project src/NovaLeave.Infrastructure \
  --startup-project src/NovaLeave.Web
```

### 3. Run the web application
```
dotnet run --project src/NovaLeave.Web
```
Then browse to `https://localhost:5001` and log in at `/Identity/Account/Login`.

### 4. Run unit tests
```
dotnet test tests/NovaLeave.UnitTests
```

### 5. Run integration tests
```
dotnet test tests/NovaLeave.IntegrationTests
```
Integration tests use a real SQL Server database (connection string required). Testcontainers MAY be used when the test objective requires it; it is not mandatory.

On macOS and Linux the fixture's LocalDB fallback is unavailable, so point it at the Docker
container by exporting the connection string first:

```bash
export NOVALEAVE_TEST_SQLSERVER="Server=localhost,14333;Database=NovaLeave_Test;User Id=sa;Password=<your MSSQL_SA_PASSWORD>;TrustServerCertificate=True;MultipleActiveResultSets=true;Pooling=false"
```

### 6. Run E2E tests
```
dotnet test tests/NovaLeave.EndToEndTests
```
Current E2E checks are smoke/static accessibility checks in the test project and
do not require a separately running browser session.

### 7. Manual Quality and Security Gate

NovaLeave MVP does not use CI/CD or automated deployment pipelines. Before merge, handoff, release, or acceptance, execute and document the Manual Quality and Security Gate in `docs/operations/manual-quality-gate.md`.

If Bash, Python, Mermaid validation, static security analysis, dependency scanning, or license-review tooling is unavailable, record the affected check as `NOT EXECUTED`. Do not report an unavailable official validator as `PASS`; PowerShell or manual equivalents are equivalent checks only.

---

## Demo Identities (CFG-003)

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
- The system business date and every user-facing timestamp use `America/Costa_Rica` (UTC−06:00); database timestamps remain UTC and there are no per-user time zones.
- All mutable aggregates use `RowVersion` (`rowversion` SQL Server type, `byte[]` in EF Core) for optimistic concurrency.
- Background services (timeout cancellation, monthly accrual) are idempotent and bounded. They can run repeatedly without side effects.
- Security events are recorded as Serilog structured log entries. No `SecurityEvent` database table.
- No MediatR, AutoMapper, or custom pipeline behaviors.

---

## Create a Migration

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

## Useful Commands

- Create migration:
  - `dotnet ef migrations add <Name> --project src/NovaLeave.Infrastructure --startup-project src/NovaLeave.Web`

---

## Contact

For PO questions (timeout X, accrual schedule), see `specs/001-leave-management-mvp/research.md` and contact the product owner.
