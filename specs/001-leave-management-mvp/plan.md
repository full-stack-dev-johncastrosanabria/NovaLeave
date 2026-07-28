# Implementation Plan: NovaLeave MVP — Vacation Request Management

**Branch**: `001-leave-management-mvp` | **Date**: 2026-07-28 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-leave-management-mvp/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

NovaLeave MVP is a vacation request management system built on Clean Architecture with ASP.NET Core MVC, Razor Views, Bootstrap 5.3, Entity Framework Core, and SQL Server. The system implements a single leave type (`Vacation`) with three combinable roles (`User`, `Approver`, `HR`), a global accruing balance (1 day per completed calendar month, non-expiring), Pending reservations, and a complete request lifecycle (`Pending` → `Approved`/`Rejected`/`CancelledByTimeout`; `Approved` → `CancelledByApprover` pre-start). HR has organization-wide read-only access plus limited approver-capability management (`canResolveRequests` toggle). The implementation follows test-first discipline with Domain invariants, Application use cases (vertical slices), Infrastructure (EF Core + Identity), and Presentation (MVC + Razor) layers.

## Technical Context

**Language/Version**: .NET 10 (C# version: SDK default; do not hardcode a specific C# version)

**Primary Dependencies**:
- ASP.NET Core 10 (MVC, Razor Views, Identity, Identity.UI)
- Entity Framework Core 10 (SQL Server provider)
- FluentValidation 11 (input validation)
- Serilog (structured logging)
- Bootstrap 5.3.x (pinned, via LibMan or npm)
- xUnit / WebApplicationFactory / Testcontainers / Playwright (testing)

**Storage**: SQL Server (production); SQL Server or real SQL instance for integration tests; EF Core migrations (versioned). Tables: VacationRequests, VacationBalances, BalanceMovements, LeaveTypes (seed only: Vacation), AuditRecords, AspNetUsers (extended with IsActive, CanResolveRequests, EmploymentStartDate). No OutboxMessages, no SystemParameters table, no SecurityEvents table (security events via Serilog structured logging — research.md CR-07, CR-10, CR-11).

**Testing**:
- Unit: xUnit (Domain invariants, working-day calculations, balance rules, concurrency scenarios)
- Integration: WebApplicationFactory + real SQL Server (Application use cases, authz, concurrency, audit, antiforgery). Testcontainers MAY be used when the test objective requires it; it is not mandatory.
- E2E: Playwright (critical browser journeys: login, create, approve, reject, timeout, deactivate, HR views, calendar nav, a11y smoke)

**Target Platform**: Docker, Azure-ready; SQL Server (Azure SQL or on-prem)

**Project Type**: Web application (ASP.NET Core MVC server-rendered)

**Performance Goals**:
- Domain/Application critical ops: p95 < 300 ms (Constitution §12.1)
- MVC pages server-side: p95 < 500 ms (Constitution §12.1)
- Batch job performance targets: document and measure per-release; no speculative targets invented

- Nullable Reference Types enabled; warnings as errors in Domain/Application
- Clean Architecture: Domain ← Application ← Infrastructure ← Presentation (inward deps only)
- No MediatR, no AutoMapper, no pipeline behaviors unless justified by repository evidence (Instruction §5.3)
- `TimeProvider` (.NET 10 built-in) for all time-dependent rules (no DateTime.Now in Domain/Application)
- Optimistic concurrency (RowVersion) on all mutable aggregates; NOT on immutable records
- WCAG 2.1 AA accessibility baseline
- Spanish UI language (Constitution §3.3)
- No SSO, no automatic password recovery, no API surface in MVP

**Scale/Scope**:
- Production user count: NEEDS CONFIGURATION (not specified in approved sources)
- Approved use cases: UC-01 through UC-22 (22 total)
- Background jobs: timeout cancellation + monthly accrual (2 total)
- Domain aggregates: VacationRequest, VacationBalance (2 aggregate roots)

## Initial Constitution Check (Conjunto 1 — Before Corrections)

*Evaluates existing Conjunto 1 artifacts before applying corrections. A proposal does not count as implemented.*

| Constitutional Area | Status | Notes |
|---|---|---|
| Clean Architecture (inward deps) | PARTIAL | Diagram had Application→Infrastructure dependency (prohibited). Project structure had prohibited patterns. |
| MVC, Razor Views, Bootstrap | PASS | Specified correctly |
| ASP.NET Core Identity & sessions | PARTIAL | Custom AccountController planned without justification; Identity routes should remain at `/Identity/Account/...` |
| Roles & authorization (User/Approver/HR) | PASS | Three roles, deny-by-default, HR restrictions correctly specified |
| Lifecycle invariants (v6.0.0) | PASS | Transitions match amended invariants |
| Balance integrity | PARTIAL | HR balance adjustment excluded, but Adjustment movement type was implied |
| Date and overlap rules | PASS | Weekends excluded, holidays counted, no per-user timezone |
| Security baseline | PARTIAL | SecurityEvent entity planned as DB table (research.md CR-07 — structured logs sufficient) |
| Auditability | PARTIAL | Sensitive reasons at risk of appearing in BalanceMovement.Reason field |
| Concurrency | PASS | RowVersion on mutable entities; idempotent jobs |
| Test-first & traceability | PARTIAL | Only 3 of 22 UC contracts defined |
| Localization | PASS | Spanish default correct |
| Scope discipline | FAIL | OutboxMessages (post-MVP email), IEmailSender, EmailSenderStub, SecurityEvent DB entity, ISystemDateProvider+ITimeProvider duplicate, SystemParameter DB entity, domain events, IDomainEventDispatcher, AutoMapper, MediatR behaviors — all speculative/excluded |
| Invented scale/numbers | FAIL | "C# 13" hardcoded; "10k users," "50 screens," "8 domain events," "< 30s for 10k users," "7 entities," "~15 use cases," "7-day default" all invented |
| Domain model correctness | FAIL | ReservationMovementId/DeductionMovementId on VacationRequest; StartBusinessDateUtc cached; SystemParameter as DB entity; LeaveType as mutable aggregate |
| Quickstart evidence | FAIL | References non-existent files, Testcontainers as mandatory, invented docker-compose files |
| Retention | FAIL | research.md recommended 2 years; Constitution §13 requires 7 years |

**Initial Constitution Result**: PARTIAL/FAIL — Conjunto 1 contained speculative scope, invented values, architectural violations, and incorrect retention. Corrections required before readiness.

---

## Repository Assessment — UC-01 through UC-22

**Inspection date**: 2026-07-28  
**Evidence**: Repository root contains only `.agents/`, `.claude/`, `.git/`, `.github/`, `.gitignore/`, `.specify/`, `.vs/`, `.vscode/`, `LICENSE`, `docs/`, `specs/`. No `src/` or `tests/` directory exists. No `.sln`, `.csproj`, or `.cs` files found.

| UC | Status | Repository Evidence | Missing / Inconsistent Work |
|---|---|---|---|
| UC-01 Authenticate | Missing | No src/ directory; no Identity configuration; no project files | All implementation: Identity, login view, cookie config, session expiry, antiforgery |
| UC-02 Switch Role Context | Missing | No src/ directory | Context switcher component, navigation structure, role claims |
| UC-03 View Own Requests | Missing | No src/ directory | MisSolicitudesController, GetMyRequestsQuery, RequestListViewModel |
| UC-04 Create Request | Missing | No src/ directory | CreateVacationRequestCommand, validation, reservation logic, atomic tx |
| UC-05 Edit Pending Request | Missing | No src/ directory | EditVacationRequestCommand, RowVersion form field, revalidation |
| UC-06 View Request Detail | Missing | No src/ directory | GetRequestDetailQuery, RequestDetailViewModel, audit trail display |
| UC-07 View Balance & History | Missing | No src/ directory | GetMyBalanceQuery, BalanceViewModel, movement timeline |
| UC-08 Personal Calendar | Missing | No src/ directory | GetCalendarQuery, CalendarViewModel, keyboard nav, WCAG |
| UC-09 Approver Queue | Missing | No src/ directory | GetApproverQueueQuery, ApproverQueueViewModel, projected balance |
| UC-10 Resolution Detail | Missing | No src/ directory | GetApproverRequestDetailQuery, overlap recheck, balance warning |
| UC-11 Approve Request | Missing | No src/ directory | ApproveRequestCommand, overlap recheck in tx, deduction, audit |
| UC-12 Reject Request | Missing | No src/ directory | RejectRequestCommand, rejection reason validation, release |
| UC-13 Deactivate Request | Missing | No src/ directory | DeactivateRequestCommand, pre-start boundary check, restoration |
| UC-14 Resolution History | Missing | No src/ directory | GetApproverHistoryQuery, history table, pagination |
| UC-15 Approver Calendar | Missing | No src/ directory | CalendarQuery for Approver, anonymized event display |
| UC-16 Timeout Cancellation | Missing | No src/ directory | TimeoutCancellationService hosted service, idempotent batch, config |
| UC-17 Monthly Accrual | Missing | No src/ directory | MonthlyAccrualService hosted service, (UserId,AccrualPeriod) idempotency |
| UC-18 HR Request List | Missing | No src/ directory | RRHHController, GetHRRequestListQuery, pagination, HR reason audit |
| UC-19 HR Org Calendar | Missing | No src/ directory | GetHRCalendarQuery, org-wide events, requester names |
| UC-20 HR Balances | Missing | No src/ directory | GetHRBalancesQuery, balance movements view |
| UC-21 HR Audit Log | Missing | No src/ directory | GetHRAuditLogQuery, redaction, immutability |
| UC-22 HR Capability Toggle | Missing | No src/ directory | ToggleApproverCapabilityCommand, RowVersion, reason, confirmation |

**Summary**: 22 Missing, 0 Partial, 0 Complete, 0 Inconsistent, 0 Blocked.

---

## Post-Phase-1 Constitution Check (After Corrections)

*Applied after correcting plan.md, research.md, data-model.md, quickstart.md, contracts/, and clean-architecture.md.*

| Constitutional Area | Status | Evidence |
|---|---|---|
| Architecture (4 layers, inward deps) | PASS | Clean-architecture.md corrected; Application→Infrastructure dependency removed; prohibited deps documented |
| Simplicity before abstraction | PASS | MediatR, AutoMapper, behaviors, domain events, IDomainEventDispatcher, repositories — all removed from plan |
| MVC, Razor Views, Bootstrap | PASS | All views use dedicated ViewModels; no Domain/EF entities in views |
| ASP.NET Core Identity & sessions | PASS | Default Identity routes; no custom AccountController; Domain User linked via ApplicationUser.Id |
| Roles & authorization | PASS | RequireActiveUser/Approver/HR policies; resource authz; deny-by-default; HR restrictions enforced |
| Lifecycle invariants | PASS | 5 states; 6 transitions; terminal states; pre-start deactivation; no User Pending cancellation |
| Balance integrity | PASS | Non-negative invariant; reserve/deduct/release/restore; no HR adjustment; no Adjustment movement type |
| Date and overlap rules | PASS | Mon–Fri; holidays counted; no per-user timezone; next-day minimum; zero-day rejection |
| Security baseline | PASS | Antiforgery; IDOR denial; self-resolution denial; session expiry; security events via Serilog |
| Auditability | PASS | AuditRecord with all 10 required fields; sensitive reasons never in Data; HR access audited (field name only) |
| Concurrency | PASS | RowVersion on mutable aggregates; concurrency matrix covers 10 races; idempotent jobs |
| Test-first & traceability | PASS | UC-01–22 contracts defined; every UC has Application contract and authorization policy |
| Localization | PASS | Spanish labels from approved specs used throughout |
| Scope exclusions | PASS | Email, queues, Redis, microservices, JWT, OpenAPI, external calendar, holiday calendar, per-user timezone — all absent |
| Retention | PASS | 7 years per Constitution §13; 2-year recommendation removed |
| Invented values | PASS | C# version removed; scale numbers removed; 7-day timeout default removed; all marked NEEDS CONFIGURATION |
| Domain model | PASS | LeaveType = read-only seed; SystemParameter = typed configuration; ReservationMovementId/DeductionMovementId removed; StartBusinessDateUtc removed; no Reason in BalanceMovement |
| Quickstart evidence | PASS | Existing vs planned clearly distinguished; no non-existent files referenced |
| Artifacts consistency | PASS | Same entity names, routes, modules, config keys across all artifacts |
| Contracts | PASS | 22 UC contracts defined in contracts/uc-contracts.md |

**Post-Phase-1 Constitution Result**: PASS — All corrections applied; no failed constitutional requirement remains.

---

## Project Structure

### Documentation (this feature)

```text
specs/001-leave-management-mvp/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
src/
  NovaLeave.Domain/
    Entities/
      VacationRequest.cs          # Aggregate root; lifecycle, state, dates, working days
      VacationBalance.cs          # Aggregate root; accrued/deducted/reserved/available
      BalanceMovement.cs          # Immutable; append-only movement record
      AuditRecord.cs              # Immutable; append-only business audit record
    ValueObjects/
      DateRange.cs                # (StartDate, EndDate) — inclusive
      WorkingDays.cs              # Positive int; server-calculated only
      NormalizedText.cs           # 10–500 chars after trim
    Enums/
      RequestStatus.cs            # Pending/Approved/Rejected/CancelledByTimeout/CancelledByApprover
      MovementType.cs             # Accrual/Reservation/Release/Deduction/Restoration
      LeaveType.cs                # Vacation (only MVP type; enum/constant, not persisted aggregate)
    Services/
      WorkingDaysCalculator.cs    # Mon–Fri count; holidays counted; zero-day rejection
      AccrualService.cs           # Completed-month rule from EmploymentStartDate
    Exceptions/
      DomainException.cs
  NovaLeave.Application/
    LeaveRequests/
      Create/
        CreateVacationRequestCommand.cs
        CreateVacationRequestHandler.cs
        CreateVacationRequestValidator.cs
      Edit/
        EditVacationRequestCommand.cs
        EditVacationRequestHandler.cs
        EditVacationRequestValidator.cs
      Approve/
        ApproveRequestCommand.cs
        ApproveRequestHandler.cs
      Reject/
        RejectRequestCommand.cs
        RejectRequestHandler.cs
        RejectRequestValidator.cs
      Deactivate/
        DeactivateRequestCommand.cs
        DeactivateRequestHandler.cs
      Timeout/
        TimeoutPendingRequestsJob.cs   # Background job use case
      Queries/
        GetMyRequestsQuery.cs
        GetRequestDetailQuery.cs
        GetApproverQueueQuery.cs
        GetApproverRequestDetailQuery.cs
        GetApproverHistoryQuery.cs
    LeaveBalances/
      GetMyBalanceQuery.cs
      AccrueMonthlyBalancesJob.cs      # Background job use case
    Calendar/
      GetCalendarQuery.cs              # Supports UC-08 (User) and UC-15 (Approver) via role context
    HR/
      GetHRRequestListQuery.cs
      GetHRRequestDetailQuery.cs
      GetHRCalendarQuery.cs            # Supports UC-19 (HR organizational calendar)
      GetHRBalancesQuery.cs
      GetHRBalanceMovementsQuery.cs
      GetHRAuditLogQuery.cs
      GetApproversListQuery.cs
      ToggleApproverCapabilityCommand.cs
      ToggleApproverCapabilityHandler.cs
      ToggleApproverCapabilityValidator.cs
    Common/
      Abstractions/
        IVacationRequestService.cs     # Application-owned abstraction (if needed)
        IAuditWriter.cs                # Application-owned abstraction for audit persistence
      DTOs/
        RequestSummaryDto.cs
        RequestDetailDto.cs
        BalanceDetailDto.cs
        CalendarDto.cs
        *(other DTOs per slice)
  NovaLeave.Infrastructure/
    Persistence/
      NovaLeaveDbContext.cs
      Configurations/
        VacationRequestConfiguration.cs
        VacationBalanceConfiguration.cs
        BalanceMovementConfiguration.cs
        AuditRecordConfiguration.cs
        LeaveTypeConfiguration.cs      # Seed: one row (Vacation)
        ApplicationUserConfiguration.cs
      Migrations/
        *(versioned migrations — not yet created)
      Seeding/
        DemoUserSeeder.cs              # IHostedService; opt-in via NovaLeave:SeedDemoUsers=true
    Identity/
      ApplicationUser.cs              # Extends IdentityUser; adds IsActive, CanResolveRequests, EmploymentStartDate
    Services/
      ClockService.cs                 # Wraps TimeProvider for DI
    BackgroundJobs/
      TimeoutCancellationHostedService.cs
      MonthlyAccrualHostedService.cs
    HealthChecks/
      DatabaseHealthCheck.cs
  NovaLeave.Presentation.Web/
    Controllers/
      MisSolicitudesController.cs     # User context (RequireActiveUser)
      SaldoController.cs              # User balance (RequireActiveUser)
      CalendarioController.cs         # Calendar — User + Approver + HR via role context
      AprobacionesController.cs       # Approver context (RequireActiveApprover)
      RRHHController.cs               # HR context (RequireActiveHR)
    Areas/
      Identity/                        # ASP.NET Core Identity UI overrides (login page branding only)
    Views/
      Shared/
        _Layout.cshtml
        _StatusBadge.cshtml
        _Toast.cshtml
        _ModalConfirm.cshtml
        _ContextSwitcher.cshtml
        _UserMenu.cshtml
        _ValidationSummary.cshtml
      MisSolicitudes/
        Index.cshtml
        Create.cshtml
        Edit.cshtml
        Detail.cshtml
      Saldo/
        Index.cshtml
      Calendario/
        Index.cshtml
      Aprobaciones/
        Index.cshtml
        Detail.cshtml
        Historial.cshtml
      RRHH/
        Dashboard.cshtml
        Solicitudes/Index.cshtml, Detail.cshtml
        Calendario/Index.cshtml
        Saldos/Index.cshtml, Detail.cshtml
        Auditoria/Index.cshtml
        Aprobadores/Index.cshtml, Capacidad.cshtml
    ViewModels/
      *(dedicated ViewModels per view — no Domain or EF entities in views)
    Filters/
      ValidationMappingFilter.cs      # Maps FluentValidation results to ModelState
    TagHelpers/
      *(shared tag helpers)
    wwwroot/
      css/site.css
      js/app.js
      lib/bootstrap/ (pinned 5.3.x)
    Program.cs                        # Composition root; registers Infrastructure; validates IOptions
tests/
  NovaLeave.UnitTests/
    Domain/
      WorkingDaysCalculatorTests.cs
      VacationRequestTests.cs
      VacationBalanceTests.cs
      AccrualServiceTests.cs
  NovaLeave.IntegrationTests/
    LeaveRequests/
      CreateRequestIntegrationTests.cs
      ApproveRequestIntegrationTests.cs
      RejectRequestIntegrationTests.cs
      DeactivateRequestIntegrationTests.cs
      TimeoutCancellationIntegrationTests.cs
      MonthlyAccrualIntegrationTests.cs
      ConcurrencyTests.cs
      AuthorizationTests.cs
      AuditTests.cs
    HR/
      HRReadOnlyAccessTests.cs
      ApproverCapabilityToggleTests.cs
    Common/
      TestFixture.cs                  # WebApplicationFactory + real SQL Server
      TestExtensions.cs
  NovaLeave.EndToEndTests/
    CriticalJourneys/
      LoginTests.cs
      ApproverApproveRejectTests.cs
      TimeoutCancellationTests.cs
      DeactivateApprovedTests.cs
      HRViewsTests.cs
      CalendarNavigationTests.cs
      ContextSwitcherTests.cs
      AccessibilitySmokeTests.cs
    Fixtures/
      PlaywrightFixture.cs
docs/
  adr/
    ADR-001-Architecture.md
    ADR-002-Auth.md
    ADR-003-Concurrency.md
    ADR-004-AccrualSemantics.md
  diagrams/
    domain-class-diagram.mermaid
    er-diagram.mermaid
    request-lifecycle.mermaid
  runbooks/
    deploy.md
    rollback.md
    migration.md
    seeding.md
    disaster-recovery.md
docker/
  Dockerfile
  docker-compose.yml
  docker-compose.override.yml (dev)
.github/
  workflows/
    ci.yml
    cd.yml

**Structure Decision**: Clean Architecture with 4 projects (Domain, Application, Infrastructure, Presentation.Web) + 3 test projects (Unit, Integration, E2E), matching Constitution §3.1 exactly. Vertical slices in Application per feature area. No API project (MVP is MVC-only per Constitution §3.2).

## Complexity Tracking

> No Constitution violations requiring justification. All patterns align with constitutional mandates.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | N/A | N/A |

## Implementation Phases (short)

1. Repository assessment & Constitution Check (done)
2. Architecture and dependency corrections (clean-architecture enforcement, DI, time provider)
3. Domain model and persistence foundations (entities, rowversion, indexes, migrations)
4. Identity, roles, active status, authorization policies, `Mis roles` UI
5. Transactions, concurrency, audit, redaction, observability (Serilog)
6. User workflows (UC-03..UC-08)
7. Approver workflows (UC-09..UC-15)
8. Automatic workflows (UC-16, UC-17) — background hosted services
9. HR workflows (UC-18..UC-22)
10. Shared MVC frontend, accessibility, responsive behavior
11. Migrations, seeded demo identities, diagrams, quickstart, CI
12. Final traceability, post-design Constitution Check, and handoff to implementation

## Atomic Operations Matrix (compact)

| Operation | Atomic effects | Concurrency / idempotency control | Failure result |
|---|---:|---|---|
| Request creation | Request (Pending) + Reservation + BalanceMovement + Audit | RowVersion on balance/request, unique insertion, DB transaction, idempotency key | Rollback all, return validation/conflict |
| Pending edit | Request update + Reservation adjustment + Movement + Audit | RowVersion, optimistic concurrency, same transaction | Rollback all, conflict/validation |
| Approval | Transition to Approved + Reservation→Deduction + Movement + Audit | RowVersion, overlap recheck inside tx, one-winner via DB constraint | Rollback, conflict or insufficient-balance rejection |
| Rejection | Transition to Rejected + Release reservation + Movement + Audit | RowVersion, single transaction | Rollback, conflict |
| Timeout | Transition to CancelledByTimeout + Release reservation + Movement + Audit (System actor) | Idempotent processing, rowversion check, bounded batches | No-op for already-processed, retry safe |
| Deactivation | Transition to CancelledByApprover + Restoration + Movement + Audit | RowVersion, start-date pre-check inside tx | Rollback, conflict |
| Monthly accrual | Accrual + Movement + Audit | Idempotency key per user+period, transaction | No duplicate accrual, rollback on failure |
| HR capability change | Toggle canResolveRequests + RowVersion + Audit | RowVersion optimistic concurrency, validation target has Approver role | 409 conflict or 400 validation, rollback |

## Security & Authorization (summary)

- Deny-by-default policies (`RequireActiveUser`, `RequireActiveApprover`, `RequireActiveHR`).
- Resource-based checks in Application layer: ownership, active status, role, `canResolveRequests`, RowVersion.
- Anti-forgery on all POSTs, explicit PRG flows for forms.
- Sensitive `reason` fields redacted in logs and audited when HR reads them (AUD-009).
- HR read access to request reasons creates a dedicated audit event recording field name, not content.

## MVC / Application Contracts

Full UC-to-route-to-contract mapping for all UC-01 through UC-22 is defined in:

`specs/001-leave-management-mvp/contracts/uc-contracts.md`

**Compact summary of key rules** (Constitution §11.1):
- Every route traces to an approved use case; no orphan routes.
- GET never changes state.
- Mutations use POST + antiforgery (AutoValidateAntiforgeryToken applied globally).
- Input ViewModels are dedicated per action; server-derived values (WorkingDays, OwnerId, Status, Balance) are never trusted from client (VAL-004).
- Views receive only dedicated ViewModels; no Domain or EF entities.
- Controllers invoke Application use cases only; no direct DbContext or concrete repository access.

## Clean-Architecture Dependency Diagram

See: `specs/001-leave-management-mvp/Diagrams/clean-architecture.md`

> Note: Conjunto 1 had an incorrect dependency (`Application -->|depends on| Infrastructure`). This is corrected in the diagram — Infrastructure implements Application abstractions, not the reverse.

## Artifacts Checklist

- [x] All 5 approved sources read (constitution.md, spec.md, frontend-design-spec.md, use-cases.md, 002 spec.md)
- [x] research.md complete — 16 conflict resolutions, all open questions resolved
- [x] data-model.md complete — domain traceability matrix, module matrix, entity definitions, concurrency matrix
- [x] quickstart.md complete — existing vs planned clearly distinguished
- [x] contracts/uc-contracts.md created — UC-01 through UC-22 all covered
- [x] Diagrams/clean-architecture.md corrected — Application→Infrastructure dependency removed
- [x] Initial Constitution Check documented
- [x] Post-Phase-1 Constitution Check: PASS
- [x] `002-role-based-frontend-views` treated as complementary only; no separate plan created

---

## Final Checklist

### Sources and Scope

| Check | Status | Notes |
|---|---|---|
| All approved sources were read | PASS | constitution.md, spec.md, frontend-design-spec.md, use-cases.md (docs/), 002 spec.md |
| Only one feature plan exists | PASS | One plan under 001-leave-management-mvp/ only |
| Feature 002 treated as complementary only | PASS | No separate plan, no artifacts under 002/ |
| No excluded or future functionality introduced | PASS | Email, queues, Redis, microservices, JWT, OpenAPI, holiday calendar absent |
| No invented business or configuration value | PASS | Timeout, session timeout, user count all marked NEEDS CONFIGURATION |

### Repository Evidence

| Check | Status | Notes |
|---|---|---|
| UC-01 through UC-22 classified | PASS | All 22 classified as Missing (no src/ directory) |
| Every classification includes repository evidence | PASS | Evidence: no src/, no tests/, no .sln or .csproj files |
| Existing code and missing work clearly distinguished | PASS | Quickstart distinguishes existing vs planned |
| Dependency findings are evidence-based | PASS | "No implementation exists yet" — verified by file search |

### Domain and Modules

| Check | Status | Notes |
|---|---|---|
| Every Domain concept has approved traceability | PASS | Traceability matrix in data-model.md |
| Aggregates have clear lifecycles and responsibilities | PASS | VacationRequest and VacationBalance lifecycles defined |
| No speculative entity, event, repository, or abstraction remains | PASS | SecurityEvent, SystemParameter, domain events, IDomainEventDispatcher, IEmailSender, repositories removed |
| Modules are cohesive and non-overlapping | PASS | 6 modules with distinct responsibilities |
| Domain contains no framework or persistence dependency | PASS | Domain project structure shows no EF Core or ASP.NET references |

### Contracts and Architecture

| Check | Status | Notes |
|---|---|---|
| Every UC has an MVC/Application contract or justified N/A | PASS | UC-01–22 in contracts/uc-contracts.md |
| Routes use correct approved paths | PASS | Identity routes, /mis-solicitudes, /aprobaciones, /rrhh all per RBFV spec |
| GET performs no mutation | PASS | All state changes use POST |
| Mutations use POST and antiforgery | PASS | AutoValidateAntiforgeryToken planned globally |
| ViewModels contain no business or authorization logic | PASS | Enforced by architecture rules |
| Controllers and Views do not access persistence directly | PASS | Controllers call Application use cases only |
| The Mermaid dependency diagram exists and is valid | PASS | Diagrams/clean-architecture.md corrected |

### Business Correctness

| Check | Status | Notes |
|---|---|---|
| Official states and transitions used exclusively | PASS | 5 states, 6 transitions per Constitution v6.0.0 §5 |
| Working days exclude weekends and count holidays | PASS | BR-004, AC-035, research.md CR-04 |
| Accrual uses completed calendar months and is idempotent | PASS | OQ-002 resolved; unique (UserId, AccrualPeriod) constraint |
| No per-user timezone logic exists | PASS | Constitution invariant 4; research.md CR-02 |
| No negative balances or manual HR adjustments exist | PASS | Domain invariant enforced; no Adjustment movement type |
| HR restrictions remain enforced | PASS | RequireActiveHR; no approve/reject/deactivate/balance-edit |
| User Pending cancellation not introduced | PASS | OQ-001 deferred per Constitution v4.0.0 invariant 7 |

### Transactions, Security, and Audit

| Check | Status | Notes |
|---|---|---|
| All eight atomic operations defined | PASS | Atomic Operations Matrix complete |
| Concurrency and idempotency races covered | PASS | 10-race matrix in data-model.md |
| Audit records include all mandatory fields | PASS | 10 required fields in AuditRecord definition |
| Sensitive reasons are redacted | PASS | AuditRecord.Data never contains Reason/RejectionReason |
| HR reason access audited without storing reason content | PASS | SEC-009, AUD-009 — field name audited, not content |
| Identity, antiforgery, overposting, IDOR, forced browsing planned | PASS | Security & Authorization section; RBFV spec references |

### Testing and Delivery

| Check | Status | Notes |
|---|---|---|
| Every active requirement has a verification method | PASS | UC contracts reference acceptance scenarios |
| Domain rules have positive, negative, and boundary tests | PASS | Test file plan in project structure |
| Transactions and relational behavior have integration tests | PASS | IntegrationTests/ planned per UC |
| MVC authorization, validation, binding, antiforgery have tests | PASS | AuthorizationTests.cs in test plan |
| Concurrency and idempotency have executable test plans | PASS | ConcurrencyTests.cs planned |
| Critical browser workflows have E2E coverage | PASS | EndToEndTests/CriticalJourneys/ planned |
| Accessibility includes automated and manual validation | PASS | AccessibilitySmokeTests.cs planned; WCAG 2.1 AA |
| Quickstart reflects actual or clearly planned repository paths | PASS | Quickstart clearly labels existing vs planned |
| Post-Phase-1 Constitution Check passes | PASS | See Post-Phase-1 section above |
| No production code or tasks.md generated | PASS | No src/ code created; no tasks.md created |

---

## Plan Summary

- **Constitution Check (Initial)**: PARTIAL/FAIL — Conjunto 1 contained speculative scope, invented values, and architectural violations
- **Constitution Check (Post-Phase-1)**: PASS — All corrections applied
- **Repository Assessment**: 22 Missing (no implementation exists)
- **NEEDS CONFIGURATION**: `PendingRequestTimeoutDays`, `SessionTimeoutMinutes`, accrual cadence
- **NEEDS CLARIFICATION**: None — all open questions resolved or documented as out of MVP scope
- **Feature 002 treatment**: Complementary specification only; no independent plan created
- **Status**: READY FOR /speckit-tasks