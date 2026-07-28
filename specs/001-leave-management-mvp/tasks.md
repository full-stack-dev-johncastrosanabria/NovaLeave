# Tasks: NovaLeave MVP — Vacation Request Management

**Feature**: `001-leave-management-mvp` | **Branch**: `abraham-villalobos`  
**Input**: `specs/001-leave-management-mvp/` (plan.md, spec.md, data-model.md, research.md, quickstart.md, contracts/uc-contracts.md)  
**Constitution**: `.specify/memory/constitution.md` v6.0.0 — Tests are MANDATORY per Principle VII

## Format: `[ID] [P?] [Story?] Description — file path`

- **[P]**: Parallelizable (different files, no incomplete-task dependencies)
- **[US#]**: User story this task belongs to
- Test tasks use path prefix `tests/`; source tasks use `src/`

---

## Phase 1: Setup — Solution and Project Initialization

**Purpose**: Create the .NET 10 solution skeleton with Clean Architecture layers, CI pipeline, and project configuration. No user story work until this phase completes.

- [ ] T001 Create .NET 10 solution file and four projects (Domain, Application, Infrastructure, Presentation.Web) — `NovaLeave.sln`
- [ ] T002 [P] Create `NovaLeave.UnitTests` xUnit project and add `NovaLeave.Domain` + `NovaLeave.Application` project references — `tests/NovaLeave.UnitTests/NovaLeave.UnitTests.csproj`
- [ ] T003 [P] Create `NovaLeave.IntegrationTests` xUnit project with `WebApplicationFactory` reference and add all src project references — `tests/NovaLeave.IntegrationTests/NovaLeave.IntegrationTests.csproj`
- [ ] T004 [P] Create `NovaLeave.EndToEndTests` xUnit project with Playwright reference — `tests/NovaLeave.EndToEndTests/NovaLeave.EndToEndTests.csproj`
- [ ] T005 [P] Add NuGet packages to `NovaLeave.Domain`: no external packages (pure Domain); enable `<Nullable>enable</Nullable>` and `<WarningsAsErrors>true</WarningsAsErrors>` — `src/NovaLeave.Domain/NovaLeave.Domain.csproj`
- [ ] T006 [P] Add NuGet packages to `NovaLeave.Application`: `FluentValidation 11`, `Microsoft.Extensions.Options`; enable Nullable + WarningsAsErrors — `src/NovaLeave.Application/NovaLeave.Application.csproj`
- [ ] T007 [P] Add NuGet packages to `NovaLeave.Infrastructure`: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Serilog.AspNetCore` — `src/NovaLeave.Infrastructure/NovaLeave.Infrastructure.csproj`
- [ ] T008 [P] Add NuGet packages to `NovaLeave.Presentation.Web`: `FluentValidation.AspNetCore` (for DI only), `Serilog.AspNetCore`; add Bootstrap 5.3.x via LibMan to `wwwroot/lib/bootstrap/` — `src/NovaLeave.Presentation.Web/NovaLeave.Presentation.Web.csproj`
- [ ] T009 Add `.editorconfig` with tab/space settings and Roslyn analyzer rules; add `Directory.Build.props` setting default `<Nullable>enable</Nullable>` across all projects — `.editorconfig`, `Directory.Build.props`
- [ ] T010 Create GitHub Actions CI workflow: restore, build, format check, unit tests, integration tests, SAST scan, coverage — `.github/workflows/ci.yml`

**Checkpoint**: `dotnet build` succeeds; solution compiles across all 7 projects.

---

## Phase 2: Foundational — Domain, Persistence, Identity, and Cross-Cutting Infrastructure

**Purpose**: Core building blocks that ALL user stories depend on. No user story work may begin until this phase completes.

**⚠️ CRITICAL**: Complete and verify every task in this phase before proceeding to Phase 3.

### Domain Model

- [ ] T011 Create `RequestStatus` enum (Pending, Approved, Rejected, CancelledByTimeout, CancelledByApprover) and `MovementType` enum (Accrual, Reservation, Release, Deduction, Restoration) — `src/NovaLeave.Domain/Enums/RequestStatus.cs`, `src/NovaLeave.Domain/Enums/MovementType.cs`
- [ ] T012 [P] Create `LeaveType` enum/constant (`Vacation`) and `DateRange` value object (Start: DateOnly, End: DateOnly; invariant: Start <= End) — `src/NovaLeave.Domain/Enums/LeaveType.cs`, `src/NovaLeave.Domain/ValueObjects/DateRange.cs`
- [ ] T013 [P] Create `WorkingDays` value object (Count: int > 0; server-calculated only; no client-trust) and `NormalizedText` value object (10–500 chars after trim; whitespace-only rejected) — `src/NovaLeave.Domain/ValueObjects/WorkingDays.cs`, `src/NovaLeave.Domain/ValueObjects/NormalizedText.cs`
- [ ] T014 Create `WorkingDaysCalculator` domain service: count Monday–Friday inclusive in a DateRange; include all public holidays as normal working days; return 0 when range is weekend-only; support both input modes (BR-004, AC-035, AC-043, EC-005, EC-006) — `src/NovaLeave.Domain/Services/WorkingDaysCalculator.cs`
- [ ] T015 [P] Create unit tests for `WorkingDaysCalculator`: Mon–Fri count, weekend exclusion, holiday-is-working-day, zero-day range rejection, both input modes, boundary dates (BR-004, VAL-001) — `tests/NovaLeave.UnitTests/Domain/WorkingDaysCalculatorTests.cs`
- [ ] T016 Create `BalanceMovement` immutable entity (Id, BalanceId, RequestId?, Type, Amount, ActorId, EffectiveAtUtc, CreatedAtUtc; no Reason field; no RowVersion; append-only semantics — Instruction §5.12) — `src/NovaLeave.Domain/Entities/BalanceMovement.cs`
- [ ] T017 Create `AuditRecord` immutable entity with all 10 required fields (TimestampUtc, ActorId, ActorRole, Action, EntityType, EntityId, Result, CorrelationId, RequestId, Data?); no RowVersion; Data never contains Reason or RejectionReason (AUD-003, SEC-005, SEC-006) — `src/NovaLeave.Domain/Entities/AuditRecord.cs`
- [ ] T018 Create `DomainException` base exception class — `src/NovaLeave.Domain/Exceptions/DomainException.cs`
- [ ] T019 Create `VacationBalance` aggregate root with properties (Id, UserId, AccruedDays, DeductedDays, ReservedDays, RowVersion, ModifiedAtUtc) and domain methods: `Reserve(int days)`, `Release(int days)`, `Deduct(int days)`, `Restore(int days)`, `Accrue(int days)`; enforce Available = Accrued − Deducted − Reserved >= 0; throw `DomainException` on invariant violation (BR-017, BR-030–034, Constitution §5 invariant 1) — `src/NovaLeave.Domain/Entities/VacationBalance.cs`
- [ ] T020 Create unit tests for `VacationBalance`: non-negative invariant on all operations, reserve/release/deduct/restore semantics, boundary amounts, double-reserve scenario, concurrent-simulation scenarios (BR-017, BR-030–034, CON-003) — `tests/NovaLeave.UnitTests/Domain/VacationBalanceTests.cs`
- [ ] T021 Create `VacationRequest` aggregate root with all fields (Id, OwnerId, StartDate, EndDate, WorkingDays, Reason, Status, RejectionReason?, RowVersion, CreatedAtUtc, UpdatedAtUtc); include domain methods: `Create(...)`, `Edit(...)`, `Approve()`, `Reject(reason)`, `CancelByTimeout()`, `CancelByApprover(systemDate)`; enforce lifecycle invariants (BR-001, BR-002, BR-007, BR-008, BR-009, BR-011, BR-025, BR-027, BR-028, Constitution §5 invariants 2–9) — `src/NovaLeave.Domain/Entities/VacationRequest.cs`
- [ ] T022 Create unit tests for `VacationRequest`: state transitions (all valid and all invalid), reversed date, next-day minimum, zero-day rejection, terminal-state mutation rejection, pre-start deactivation boundary, post-start deactivation denial, partial-deactivation rejection (BR-001, BR-002, BR-007, BR-011, BR-027, BR-028, Constitution §5 invariants) — `tests/NovaLeave.UnitTests/Domain/VacationRequestTests.cs`
- [ ] T023 Create `AccrualService` domain service: given `EmploymentStartDate` and `referenceDate`, compute which completed calendar months are eligible; first partial month excluded (OQ-002; BR-032, BR-033) — `src/NovaLeave.Domain/Services/AccrualService.cs`
- [ ] T024 [P] Create unit tests for `AccrualService`: completed-month boundary, first-partial-month exclusion, same-month idempotency, multiple-months-catchup, leap-year boundaries (BR-032, BR-033, OQ-002) — `tests/NovaLeave.UnitTests/Domain/AccrualServiceTests.cs`

### Infrastructure — Persistence and Identity

- [ ] T025 Create `ApplicationUser` extending `IdentityUser<string>` with: `IsActive` (bool), `CanResolveRequests` (bool), `EmploymentStartDate` (DateOnly), `RowVersion` (byte[]); note: Domain User concept is NOT a class inheriting IdentityUser — `src/NovaLeave.Infrastructure/Identity/ApplicationUser.cs`
- [ ] T026 Create `NovaLeaveDbContext` extending `IdentityDbContext<ApplicationUser>` with DbSets for VacationRequest, VacationBalance, BalanceMovement, AuditRecord, LeaveType; register it in DI with SQL Server provider — `src/NovaLeave.Infrastructure/Persistence/NovaLeaveDbContext.cs`
- [ ] T027 [P] Create EF Core configuration for `VacationRequest`: map all fields; configure `rowversion` on RowVersion byte[]; add indexes `IX_VacationRequest_OwnerId_Status` and `IX_VacationRequest_StartDate_EndDate` — `src/NovaLeave.Infrastructure/Persistence/Configurations/VacationRequestConfiguration.cs`
- [ ] T028 [P] Create EF Core configuration for `VacationBalance`: map all fields; configure `rowversion`; add unique index on UserId — `src/NovaLeave.Infrastructure/Persistence/Configurations/VacationBalanceConfiguration.cs`
- [ ] T029 [P] Create EF Core configuration for `BalanceMovement`: map all fields; no RowVersion; add indexes `IX_BalanceMovement_BalanceId_EffectiveAt` and `IX_BalanceMovement_RequestId`; configure immutability (no update operations) — `src/NovaLeave.Infrastructure/Persistence/Configurations/BalanceMovementConfiguration.cs`
- [ ] T030 [P] Create EF Core configuration for `AuditRecord`: map all fields; no RowVersion; configure immutability; protect from Update/Delete via interceptor or convention — `src/NovaLeave.Infrastructure/Persistence/Configurations/AuditRecordConfiguration.cs`
- [ ] T031 [P] Create EF Core configuration for `LeaveType`: map Id, Name, IsActive; configure as seed-only (HasData for Vacation row); configure `ApplicationUser` with RowVersion and custom properties — `src/NovaLeave.Infrastructure/Persistence/Configurations/LeaveTypeConfiguration.cs`, `src/NovaLeave.Infrastructure/Persistence/Configurations/ApplicationUserConfiguration.cs`
- [ ] T032 Create and run `InitialCreate` EF Core migration: all tables, indexes, RowVersion columns, LeaveType seed row (Vacation, active); no SystemParameter table, no OutboxMessages, no SecurityEvents table — `src/NovaLeave.Infrastructure/Persistence/Migrations/`
- [ ] T033 Create `DemoUserSeeder` as `IHostedService`: idempotent; seeds 4 demo users (`user@demo`, `approver@demo`, `hr@demo`, `multi@demo`) with password `Demo123!`, roles, canResolveRequests, EmploymentStartDate = 12 months prior; executes only when `NovaLeave:SeedDemoUsers=true`; disabled in Production (CFG-003) — `src/NovaLeave.Infrastructure/Persistence/Seeding/DemoUserSeeder.cs`

### Application Layer — Typed Configuration and Abstractions

- [ ] T034 Create `NovaLeaveOptions` with properties `PendingRequestTimeoutDays` (int, required > 0), `SessionTimeoutMinutes` (int, required > 0), `SeedDemoUsers` (bool); register in `Program.cs` with `ValidateDataAnnotations().ValidateOnStart()` (CFG-001, CFG-002, research.md CR-10) — `src/NovaLeave.Application/Common/Options/NovaLeaveOptions.cs`
- [ ] T035 [P] Create `IAuditWriter` application abstraction for writing immutable `AuditRecord`s; implement in Infrastructure as `EfCoreAuditWriter`; register via DI — `src/NovaLeave.Application/Common/Abstractions/IAuditWriter.cs`, `src/NovaLeave.Infrastructure/Persistence/EfCoreAuditWriter.cs`

### Presentation — Identity, Auth Policies, Middleware, Layout

- [ ] T036 Configure ASP.NET Core Identity in `Program.cs`: cookie auth; `HttpOnly`, `Secure` in production, appropriate `SameSite`; session expiry via `NovaLeave:SessionTimeoutMinutes`; security-stamp validation interval; lockout policy; password policy; deny-by-default authorization policy (Constitution §7.1, SEC-001, CFG-002) — `src/NovaLeave.Presentation.Web/Program.cs`
- [ ] T037 Define authorization policies: `RequireActiveUser`, `RequireActiveApprover`, `RequireActiveHR`, `RequireRequestOwner`, `RequireApproverEligible`, `RequireApproverNotOwner`, `RequirePreStartDeactivation`, `RequireHRForApproverManagement`; apply `[Authorize(Policy = "RequireActiveUser")]` etc. at controller class level (RBFV spec §9.1, AUTHZ-001–016) — `src/NovaLeave.Presentation.Web/Program.cs`
- [ ] T038 [P] Add Serilog correlation middleware: inject `correlation_id` and `request_id` into every request; enrich all Serilog log entries; exclude sensitive payloads from request logs (Constitution §8, AUD-003, SEC-005) — `src/NovaLeave.Presentation.Web/Middleware/CorrelationMiddleware.cs`
- [ ] T039 [P] Register `TimeProvider` as singleton DI service in `Program.cs`; verify no `DateTime.Now` or `DateTime.UtcNow` usage in Domain or Application projects (Constitution §VI, research.md CR-08) — `src/NovaLeave.Presentation.Web/Program.cs`
- [ ] T040 [P] Create `ValidationMappingFilter` action filter: invokes FluentValidation explicitly via `IValidator<T>`, maps results to `ModelState`; register globally; do NOT use deprecated sync auto-validation pipeline (Constitution §7.2) — `src/NovaLeave.Presentation.Web/Filters/ValidationMappingFilter.cs`
- [ ] T041 Create `_Layout.cshtml` with: semantic `<header>` (brand logo, "Mis roles" context switcher for multi-role identities, user menu with "Cerrar sesión" POST); responsive hamburger at < 768px; `prefers-reduced-motion` CSS; design token CSS variables from frontend-design-spec.md §4; no gradients, no emojis — `src/NovaLeave.Presentation.Web/Views/Shared/_Layout.cshtml`, `src/NovaLeave.Presentation.Web/wwwroot/css/site.css`
- [ ] T042 [P] Create branded error pages: `acceso-denegado` (403, `/acceso-denegado`), `Error` (500, `/error` with correlation ID); configure centralized exception middleware; no stack traces in user-facing output (Constitution §11.4, ERR-003) — `src/NovaLeave.Presentation.Web/Views/Shared/Error.cshtml`, `src/NovaLeave.Presentation.Web/Views/Shared/AccesoDenegado.cshtml`
- [ ] T043 [P] Create Identity UI Area override for login page: branding with design tokens, accessible email/password fields, "Cuenta" demo-account switcher dropdown (pre-fills email only; does not authenticate), no emojis, WCAG 2.1 AA (frontend-design-spec.md §13, RBFV §8.17, CFG-003) — `src/NovaLeave.Presentation.Web/Areas/Identity/Pages/Account/Login.cshtml`
- [ ] T044 Create `DatabaseHealthCheck` and register with `/health` endpoint (no PII, no internal config exposed); configure globally with `AutoValidateAntiforgeryToken` attribute and explicit PRG conventions (Constitution §12.3, §7.2) — `src/NovaLeave.Infrastructure/HealthChecks/DatabaseHealthCheck.cs`

**Checkpoint**: `dotnet build` passes; migrations apply; login page renders; all 4 demo users seed correctly when `SeedDemoUsers=true`; health endpoint returns healthy. Unit tests T015, T020, T022, T024 all pass.

---

## Phase 3: User Story 1 — User Submits and Manages a Vacation Request (Priority: P1) 🎯 MVP

**Goal**: An authenticated active User can submit a vacation request in either input mode, view their requests and balance impact, and edit a Pending request — all server-authoritative with atomic audit.

**Independent Test**: Authenticate as `user@demo`; submit a valid request in both input modes; verify one Pending request and one reservation appear; verify balance reflects reservation; edit the request; verify atomic audit records.

### Tests for User Story 1 — Domain and Application (Write FIRST, verify they FAIL)

- [ ] T045 [P] [US1] Unit tests for `CreateVacationRequestCommand` handler: valid creation, missing data (VAL-001), reversed range (BR-001), same-day start (BR-002, EC-001), zero working days (BR-024, EC-006), insufficient balance (BR-018), overlap rejection (BR-019, AC-010), client-value ignored (VAL-004), duplicate submission (CON-001, AC-009) — `tests/NovaLeave.UnitTests/Application/CreateRequestHandlerTests.cs`
- [ ] T046 [P] [US1] Unit tests for `EditVacationRequestCommand` handler: valid edit, stale RowVersion (CON-004), non-Pending status rejection, non-owner rejection, insufficient balance after edit, zero-day edit (BR-025, CON-011, AC-045, AC-046) — `tests/NovaLeave.UnitTests/Application/EditRequestHandlerTests.cs`
- [ ] T047 [P] [US1] Integration tests for request creation: atomic tx (request + reservation + movement + audit), audit failure rollback (AC-037, CON-007), concurrent overlapping submissions (AC-040, CON-008), inactive-user denial (AUTHZ-009, AC-047), antiforgery, overposting (VAL-004) — `tests/NovaLeave.IntegrationTests/LeaveRequests/CreateRequestIntegrationTests.cs`
- [ ] T048 [P] [US1] Integration tests for request edit: atomic tx, RowVersion conflict, non-owner denial (IDOR), non-Pending status rejection, reservation adjustment correctness (CON-011, AC-045, AC-046) — `tests/NovaLeave.IntegrationTests/LeaveRequests/EditRequestIntegrationTests.cs`

### Implementation for User Story 1

- [ ] T049 [US1] Implement `CreateVacationRequestCommand` + `CreateVacationRequestHandler`: resolve input mode, calculate authoritative StartDate/EndDate/WorkingDays via `WorkingDaysCalculator`; validate via `CreateVacationRequestValidator` (FluentValidation); check active status, overlap, balance; execute single DB tx: VacationRequest (Pending) + VacationBalance.Reserve + BalanceMovement(Reservation) + AuditRecord(Create); return typed result — `src/NovaLeave.Application/LeaveRequests/Create/CreateVacationRequestCommand.cs`, `src/NovaLeave.Application/LeaveRequests/Create/CreateVacationRequestHandler.cs`, `src/NovaLeave.Application/LeaveRequests/Create/CreateVacationRequestValidator.cs`
- [ ] T050 [US1] Implement `EditVacationRequestCommand` + `EditVacationRequestHandler`: revalidate ownership, active status, Pending state, RowVersion; recalculate authoritative values; check overlap excluding self; adjust reservation delta; execute single DB tx: request update + reservation adjustment + BalanceMovement + AuditRecord(Edit) — `src/NovaLeave.Application/LeaveRequests/Edit/EditVacationRequestCommand.cs`, `src/NovaLeave.Application/LeaveRequests/Edit/EditVacationRequestHandler.cs`, `src/NovaLeave.Application/LeaveRequests/Edit/EditVacationRequestValidator.cs`
- [ ] T051 [P] [US1] Implement `GetMyRequestsQuery` + handler: paginated, owner-filtered, `AsNoTracking`; map to `RequestSummaryDto` — `src/NovaLeave.Application/LeaveRequests/Queries/GetMyRequestsQuery.cs`
- [ ] T052 [P] [US1] Implement `GetRequestDetailQuery` + handler: owner-validated; map to `RequestDetailDto`; audit trail visible; reason visible to owner only — `src/NovaLeave.Application/LeaveRequests/Queries/GetRequestDetailQuery.cs`
- [ ] T053 [US1] Create `MisSolicitudesController` with actions: `Index` (GET), `Create` (GET/POST), `Edit` (GET/POST), `Detail` (GET); apply `[Authorize(Policy = "RequireActiveUser")]` at class; use `[AutoValidateAntiforgeryToken]`; PRG after mutations; call Application handlers only — `src/NovaLeave.Presentation.Web/Controllers/MisSolicitudesController.cs`
- [ ] T054 [P] [US1] Create `RequestListViewModel`, `RequestCreateViewModel` (inputMode, startDate, endDate?, workingDays?, reason; server-derived fields excluded), `RequestEditViewModel` (add rowVersion), `RequestDetailViewModel`; no business logic in ViewModels (Constitution §11.1) — `src/NovaLeave.Presentation.Web/ViewModels/MisSolicitudes/`
- [ ] T055 [US1] Create `Mis Solicitudes` Razor views: `Index.cshtml` (table, status badges, pagination — RBFV §8.2), `Create.cshtml` (dual-mode form — RBFV §8.3), `Edit.cshtml` (pre-filled dual-mode, rowVersion hidden field — RBFV §8.3), `Detail.cshtml` (read-only, audit timeline — RBFV §8.4); all use design tokens; no emojis; WCAG 2.1 AA labels and live regions — `src/NovaLeave.Presentation.Web/Views/MisSolicitudes/`
- [ ] T056 [P] [US1] Create shared partial `_StatusBadge.cshtml` rendering semantic status badges with text labels and design-token colors (status never conveyed by color alone — frontend-design-spec.md §4.1, Constitution §11.2) — `src/NovaLeave.Presentation.Web/Views/Shared/_StatusBadge.cshtml`
- [ ] T057 [P] [US1] Create shared partial `_ValidationSummary.cshtml` and `_Toast.cshtml` (opacity entry ≤ 200ms; no gradients; accessible) — `src/NovaLeave.Presentation.Web/Views/Shared/_ValidationSummary.cshtml`, `src/NovaLeave.Presentation.Web/Views/Shared/_Toast.cshtml`
- [ ] T058 [P] [US1] Add User context navigation items to `_Layout.cshtml`: "Mis solicitudes" → `/mis-solicitudes`, "Mi historial" → `/saldo`, "Calendario" → `/calendario` (RBFV §7.1) — `src/NovaLeave.Presentation.Web/Views/Shared/_Layout.cshtml`

**Checkpoint**: User Story 1 fully functional. `user@demo` can log in, create requests in both input modes, view their list, edit a Pending request, and view the detail with audit trail. All T045–T048 tests pass.

---

## Phase 4: User Story 2 — Approver Resolves a Vacation Request (Priority: P1)

**Goal**: Any authenticated active Approver (with `canResolveRequests=true`) can approve or reject any eligible Pending request that is not their own — globally scoped, with balance conversion, atomic audit, and correct concurrency control.

**Independent Test**: Authenticate as `approver@demo`; load the pending queue; approve one request; verify balance deducted exactly once, audit record created, queue updated. Reject another with a reason; verify release. Attempt self-resolution → denied with security event.

### Tests for User Story 2

- [ ] T059 [P] [US2] Unit tests for `ApproveRequestCommand` handler: valid approval, self-resolution denial (AC-017), inactive-Approver denial (AC-047), insufficient balance (AC-018), stale RowVersion (AC-020), terminal state rejection (AC-021), concurrent dual-approval (AC-019, CON-002) — `tests/NovaLeave.UnitTests/Application/ApproveRequestHandlerTests.cs`
- [ ] T060 [P] [US2] Unit tests for `RejectRequestCommand` handler: valid rejection with reason, invalid reason length (VAL-007, AC-036), self-resolution denial, release-not-deduction semantics (EC-004) — `tests/NovaLeave.UnitTests/Application/RejectRequestHandlerTests.cs`
- [ ] T061 [P] [US2] Integration tests for approval: atomic tx (transition + deduction + movement + audit), duplicate approval idempotency (CON-003), overlap recheck inside tx (CON-005), rollback on audit failure (CON-006, AC-033), self-resolution → security log (AC-017, AUD-004) — `tests/NovaLeave.IntegrationTests/LeaveRequests/ApproveRequestIntegrationTests.cs`
- [ ] T062 [P] [US2] Integration tests for rejection: atomic tx, reservation release, reason stored and redacted from logs (SEC-005, AC-029), duplicate rejection idempotency (CON-012) — `tests/NovaLeave.IntegrationTests/LeaveRequests/RejectRequestIntegrationTests.cs`
- [ ] T063 [P] [US2] Integration tests for authorization: unauthenticated access denial (AC-030), IDOR cross-User (AC-008), forced-browsing to approve own request (AUTHZ-003), inactive Approver (AC-047), expired session reuse (AC-056), antiforgery failure (AC-032) — `tests/NovaLeave.IntegrationTests/LeaveRequests/AuthorizationTests.cs`

### Implementation for User Story 2

- [ ] T064 [US2] Implement `ApproveRequestCommand` + `ApproveRequestHandler`: revalidate actor, active status, `canResolveRequests`, non-ownership, Pending state, RowVersion; recalculate available balance and overlap within same tx; execute single DB tx: Pending→Approved + VacationBalance.Deduct + BalanceMovement(Deduction) + AuditRecord(Approve); log security event via Serilog on denial — `src/NovaLeave.Application/LeaveRequests/Approve/ApproveRequestCommand.cs`, `src/NovaLeave.Application/LeaveRequests/Approve/ApproveRequestHandler.cs`
- [ ] T065 [US2] Implement `RejectRequestCommand` + `RejectRequestHandler` + `RejectRequestValidator`: validate reason (10–500 normalized chars); execute single DB tx: Pending→Rejected + VacationBalance.Release + BalanceMovement(Release) + AuditRecord(Reject); RejectionReason stored on request; never written to Serilog (SEC-005) — `src/NovaLeave.Application/LeaveRequests/Reject/RejectRequestCommand.cs`, `src/NovaLeave.Application/LeaveRequests/Reject/RejectRequestHandler.cs`, `src/NovaLeave.Application/LeaveRequests/Reject/RejectRequestValidator.cs`
- [ ] T066 [P] [US2] Implement `GetApproverQueueQuery` + handler: org-wide Pending requests, exclude Approver's own, include projected balance (available − requested days) as informational display; paginated, `AsNoTracking` — `src/NovaLeave.Application/LeaveRequests/Queries/GetApproverQueueQuery.cs`
- [ ] T067 [P] [US2] Implement `GetApproverRequestDetailQuery` + handler: revalidate eligibility; recalculate projected balance server-side; check overlap warning; return full detail DTO — `src/NovaLeave.Application/LeaveRequests/Queries/GetApproverRequestDetailQuery.cs`
- [ ] T068 [P] [US2] Implement `GetApproverHistoryQuery` + handler: resolutions attributed to actor; filterable; paginated — `src/NovaLeave.Application/LeaveRequests/Queries/GetApproverHistoryQuery.cs`
- [ ] T069 [US2] Create `AprobacionesController` with actions: `Index` (GET), `Detail` (GET), `Aprobar` (POST), `Rechazar` (POST), `Historial` (GET); `[Authorize(Policy = "RequireActiveApprover")]`; `[AutoValidateAntiforgeryToken]`; PRG after mutations — `src/NovaLeave.Presentation.Web/Controllers/AprobacionesController.cs`
- [ ] T070 [P] [US2] Create `ApproverQueueViewModel` (includes projected balance columns: Disponible actual, Días solicitados, Disponible después de aprobar — RBFV §8.7), `ApproverDetailViewModel` (balance revalidation banner), `ApproverHistoryViewModel`; no business logic in ViewModels — `src/NovaLeave.Presentation.Web/ViewModels/Aprobaciones/`
- [ ] T071 [US2] Create Aprobaciones Razor views: `Index.cshtml` (paginated table with projected balance, RBFV §8.7), `Detail.cshtml` (full detail, projected balance card, rejection reason textarea with char counter 10–500, approve/reject buttons, RBFV §8.8), `Historial.cshtml` (paginated history, RBFV §8.10); WCAG 2.1 AA; no emojis; design tokens; `_ModalConfirm.cshtml` shared partial for destructive confirmations — `src/NovaLeave.Presentation.Web/Views/Aprobaciones/`, `src/NovaLeave.Presentation.Web/Views/Shared/_ModalConfirm.cshtml`
- [ ] T072 [P] [US2] Add Approver context navigation to `_Layout.cshtml`: "Pendientes" → `/aprobaciones`, "Historial" → `/aprobaciones/historial`, "Calendario" → `/calendario` (RBFV §7.2) — `src/NovaLeave.Presentation.Web/Views/Shared/_Layout.cshtml`
- [ ] T073 [P] [US2] Implement `AuditTests`: verify every successful Create, Approve, Reject generates exactly one immutable AuditRecord with all 10 required fields; verify no Reason content in AuditRecord.Data (AUD-001–003, AUD-006, SEC-005) — `tests/NovaLeave.IntegrationTests/LeaveRequests/AuditTests.cs`

**Checkpoint**: User Story 2 fully functional. `approver@demo` can view the queue, approve/reject requests, see history. Concurrent dual-approval test yields exactly one transition. All T059–T063 and T073 tests pass.

---

## Phase 5: User Story 3 — System Cancels Unresolved Requests by Timeout (Priority: P2)

**Goal**: A background process automatically cancels Pending requests older than `NovaLeave:PendingRequestTimeoutDays` days, releases reservations idempotently, and attributes the action to the System actor.

**Independent Test**: Seed a Pending request older than the configured timeout; run the timeout service; verify CancelledByTimeout state, reservation released, audit actor = "System"; run again → no additional effect.

### Tests for User Story 3

- [ ] T074 [P] [US3] Integration tests for timeout cancellation: eligible request → CancelledByTimeout + release + System-actor audit (AC-048, AUD-008), re-run idempotency (AC-049, CON-009), concurrent approval-vs-timeout → exactly one wins (EC-007, CON-002), missing configuration fails at startup (CFG-001) — `tests/NovaLeave.IntegrationTests/LeaveRequests/TimeoutCancellationIntegrationTests.cs`

### Implementation for User Story 3

- [ ] T075 [US3] Implement `TimeoutPendingRequestsJob` application use case: query Pending requests where `CreatedAtUtc < now − PendingRequestTimeoutDays`; process in bounded batches; for each: revalidate Status=Pending within tx, check RowVersion; execute single DB tx: Pending→CancelledByTimeout + VacationBalance.Release + BalanceMovement(Release) + AuditRecord(Timeout, ActorId="System"); yield to concurrent resolutions (CON-009, AUD-008, CFG-001) — `src/NovaLeave.Application/LeaveRequests/Timeout/TimeoutPendingRequestsJob.cs`
- [ ] T076 [US3] Implement `TimeoutCancellationHostedService` as `IHostedService`: invoke `TimeoutPendingRequestsJob` on configurable cadence (NEEDS CONFIGURATION); catch-and-log exceptions per batch without stopping service; inject `TimeProvider` for testable scheduling — `src/NovaLeave.Infrastructure/BackgroundJobs/TimeoutCancellationHostedService.cs`
- [ ] T077 [P] [US3] Add E2E test: timeout workflow browser journey — seed old Pending request; verify after timeout run that request detail shows "Cancelada automáticamente" status and no balance deduction remains — `tests/NovaLeave.EndToEndTests/CriticalJourneys/TimeoutCancellationTests.cs`

**Checkpoint**: User Story 3 fully functional. Timeout job processes eligible requests exactly once, is idempotent on re-run, and yields correctly to concurrent approvals. T074 passes.

---

## Phase 6: User Story 4 — Approver Deactivates an Approved Request Before Its Start Date (Priority: P2)

**Goal**: An active Approver can cancel the entire future approved request, atomically restoring the owner's balance, strictly before the vacation period begins.

**Independent Test**: Seed an Approved request with a future start date; authenticate as `approver@demo`; deactivate; verify CancelledByApprover, balance restored exactly once, audit record present. Attempt after start date → denied.

### Tests for User Story 4

- [ ] T078 [P] [US4] Integration tests for deactivation: valid pre-start deactivation → atomic tx (CancelledByApprover + Restoration + AuditRecord, AC-050, CON-010), post-start denial (AC-051, BR-027), partial-deactivation denial (AC-052, BR-028), duplicate deactivation idempotency (CON-003), rollback on failure (AC-053), self-deactivation denial (AC-017), inactive Approver denial (AC-047) — `tests/NovaLeave.IntegrationTests/LeaveRequests/DeactivateRequestIntegrationTests.cs`

### Implementation for User Story 4

- [ ] T079 [US4] Implement `DeactivateRequestCommand` + `DeactivateRequestHandler`: revalidate actor, active status, `canResolveRequests`, non-ownership, Status=Approved, `StartDate > TimeProvider.Today`, RowVersion; execute single DB tx: Approved→CancelledByApprover + VacationBalance.Restore + BalanceMovement(Restoration) + AuditRecord(Deactivate) — `src/NovaLeave.Application/LeaveRequests/Deactivate/DeactivateRequestCommand.cs`, `src/NovaLeave.Application/LeaveRequests/Deactivate/DeactivateRequestHandler.cs`
- [ ] T080 [US4] Add `Desactivar` POST action to `AprobacionesController`; add `Desactivar` button to `Detail.cshtml` (visible only when Status=Approved and StartDate > today; requires confirmation modal; PRG on success) — `src/NovaLeave.Presentation.Web/Controllers/AprobacionesController.cs`, `src/NovaLeave.Presentation.Web/Views/Aprobaciones/Detail.cshtml`
- [ ] T081 [P] [US4] E2E test: pre-start deactivation journey — approve a request; verify balance deducted; deactivate before start; verify balance restored; verify status badge shows "Cancelada por aprobador" — `tests/NovaLeave.EndToEndTests/CriticalJourneys/DeactivateApprovedTests.cs`

**Checkpoint**: User Story 4 fully functional. Pre-start deactivation works atomically; post-start deactivation is denied. T078 passes.

---

## Phase 7: User Story 5 — User Balance and Monthly Accrual (Priority: P2)

**Goal**: Users see their accurate global balance (Acumulado total, Pendientes, Días gozados, Disponible) and accrue exactly one whole day per completed calendar month idempotently.

**Independent Test**: Seed a User with `EmploymentStartDate` 3 months ago; run accrual; verify 3 days accrued; run again → no change; view balance page → correct values displayed.

### Tests for User Story 5

- [ ] T082 [P] [US5] Integration tests for monthly accrual: one day per completed month, no proration, no expiry, idempotency via unique (UserId, AccrualPeriod) constraint, catch-up for multiple months, failure rollback (AC-054, BR-032, BR-033, CON-007 analog) — `tests/NovaLeave.IntegrationTests/LeaveRequests/MonthlyAccrualIntegrationTests.cs`
- [ ] T083 [P] [US5] Integration tests for balance display: available = accrued − deducted − reserved (never below 0), reservation reflected after create, release after reject/timeout, deduction after approve, restoration after deactivation (AC-044, BR-031, SC-009) — `tests/NovaLeave.IntegrationTests/LeaveRequests/BalanceInvariantTests.cs`

### Implementation for User Story 5

- [ ] T084 [US5] Implement `AccrueMonthlyBalancesJob` application use case: query users eligible for one or more outstanding completed months; use `AccrualService` to determine eligible periods; for each user+period not already accrued: insert unique BalanceMovement(Accrual) + VacationBalance.Accrue + AuditRecord(Accrual); unique constraint on (UserId, AccrualPeriod) prevents duplicates (BR-032, BR-033, FR-014, OQ-002) — `src/NovaLeave.Application/LeaveBalances/AccrueMonthlyBalancesJob.cs`
- [ ] T085 [US5] Implement `MonthlyAccrualHostedService` as `IHostedService`: invoke `AccrueMonthlyBalancesJob` on configurable cadence (NEEDS CONFIGURATION); catch-and-log exceptions without stopping service — `src/NovaLeave.Infrastructure/BackgroundJobs/MonthlyAccrualHostedService.cs`
- [ ] T086 [P] [US5] Implement `GetMyBalanceQuery` + handler: load VacationBalance by userId; compute available balance; load BalanceMovements timeline (paginated); `AsNoTracking`; map to `BalanceDetailDto` — `src/NovaLeave.Application/LeaveBalances/GetMyBalanceQuery.cs`
- [ ] T087 [US5] Create `SaldoController` with `Index` GET action; `[Authorize(Policy = "RequireActiveUser")]`; display balance summary and movement timeline — `src/NovaLeave.Presentation.Web/Controllers/SaldoController.cs`
- [ ] T088 [P] [US5] Create `BalanceViewModel` with Spanish labels (Acumulado total, Pendientes, Días gozados, Disponible — RBFV §8.5, frontend-design-spec.md §16); create `Saldo/Index.cshtml` with 4 summary cards, balance movements timeline with definition list; accessible (RBFV §8.5) — `src/NovaLeave.Presentation.Web/ViewModels/Saldo/BalanceViewModel.cs`, `src/NovaLeave.Presentation.Web/Views/Saldo/Index.cshtml`

**Checkpoint**: User Story 5 fully functional. Accrual runs idempotently; balance page shows correct values reflecting all reservations, deductions, and accruals. T082 and T083 pass.

---

## Phase 8: User Story 6 — HR Organization-Wide Access and Capability Management (Priority: P2)

**Goal**: An active HR identity has read-only organization-wide access to requests, calendar, balances, and audit; and can enable/disable `canResolveRequests` on existing Approvers with reason, confirmation, RowVersion, and full audit.

**Independent Test**: Authenticate as `hr@demo`; view request list, balance list, audit log (all read-only, no resolution buttons); toggle Approver capability with reason; verify AuditRecord(ToggleCapability) created; attempt to approve a request → 403.

### Tests for User Story 6

- [ ] T089 [P] [US6] Integration tests for HR read-only access: list requests (all users), request detail (no resolution actions), balances, movements, audit log — paginated, filtered; verify read-only enforcement: no approve/reject/deactivate actions available (FR-009, FR-016–020, AUTHZ-011) — `tests/NovaLeave.IntegrationTests/HR/HRReadOnlyAccessTests.cs`
- [ ] T090 [P] [US6] Integration tests for HR capability toggle: valid toggle (reason 10–500 chars, confirmation, RowVersion, target has Approver role → AuditRecord with before/after, no reason content in record), stale RowVersion → 409, missing reason → 400, missing confirmation → 400, target lacks Approver → 400, failed toggle → AuditRecord(ToggleCapabilityFailed) via Serilog, inactive HR → 403 (UC-22, AUD-009, AUD-010, AUTHZ-012–016) — `tests/NovaLeave.IntegrationTests/HR/ApproverCapabilityToggleTests.cs`
- [ ] T091 [P] [US6] Security tests for HR prohibitions: HR attempt to approve (AUTHZ-013), HR attempt to reject (AUTHZ-014), HR attempt to deactivate (AUTHZ-015), HR attempt to modify balance (AUTHZ-014), HR attempt to assign role → all 403 with security event logged (Constitution §7.4) — `tests/NovaLeave.IntegrationTests/HR/HRSecurityProhibitionsTests.cs`
- [ ] T092 [P] [US6] Security tests for HR sensitive-data access: verify that when HR accesses request detail with Reason/RejectionReason, AuditRecord(HRSensitiveAccess) is created with field name only (not content); verify reasons never appear in logs (SEC-009, AUD-009, SEC-005, AC-HR-009) — `tests/NovaLeave.IntegrationTests/HR/HRSensitiveAccessAuditTests.cs`

### Implementation for User Story 6

- [ ] T093 [P] [US6] Implement HR read-only queries: `GetHRRequestListQuery`, `GetHRRequestDetailQuery` (with sensitive-access audit trigger), `GetHRCalendarQuery`, `GetHRBalancesQuery`, `GetHRBalanceMovementsQuery`, `GetHRAuditLogQuery`, `GetApproversListQuery`; all paginated, `AsNoTracking`; Reason/RejectionReason returned only to HR via authorized DTO; trigger AuditRecord(HRSensitiveAccess) when field is returned (SEC-009, AUD-009) — `src/NovaLeave.Application/HR/`
- [ ] T094 [US6] Implement `ToggleApproverCapabilityCommand` + `ToggleApproverCapabilityHandler` + `ToggleApproverCapabilityValidator`: validate HR actor active, target has Approver role, reason 10–500 chars, confirmation flag set, RowVersion matches; execute single DB tx: update `ApplicationUser.CanResolveRequests` + AuditRecord(ToggleCapability, before/after values, reason excluded); trigger security-stamp refresh; log `AuditRecord(ToggleCapabilityFailed)` via Serilog on any failure (FR-022, AUTHZ-012, AUD-009, AUD-010) — `src/NovaLeave.Application/HR/ToggleApproverCapabilityCommand.cs`, `src/NovaLeave.Application/HR/ToggleApproverCapabilityHandler.cs`, `src/NovaLeave.Application/HR/ToggleApproverCapabilityValidator.cs`
- [ ] T095 [US6] Create `RRHHController` with all HR actions: `Dashboard`, `Solicitudes/Index`, `Solicitudes/Detail`, `Calendario`, `Saldos/Index`, `Saldos/Detail`, `Auditoria/Index`, `Aprobadores/Index`, `Aprobadores/Capacidad` (GET + POST); `[Authorize(Policy = "RequireActiveHR")]`; no approve/reject/deactivate actions; read-only — `src/NovaLeave.Presentation.Web/Controllers/RRHHController.cs`
- [ ] T096 [P] [US6] Create HR ViewModels: `HRDashboardViewModel`, `HRRequestListViewModel`, `HRRequestDetailViewModel`, `HRCalendarViewModel`, `HRBalancesViewModel`, `HRBalanceDetailViewModel`, `HRAuditViewModel`, `HRApproversViewModel`, `HRApproverCapabilityViewModel` (includes reason textarea, confirmation checkbox, rowVersion hidden field, antiforgery); no resolution actions in any HR ViewModel — `src/NovaLeave.Presentation.Web/ViewModels/RRHH/`
- [ ] T097 [US6] Create HR Razor views: `RRHH/Dashboard.cshtml`, `Solicitudes/Index.cshtml + Detail.cshtml`, `Calendario/Index.cshtml`, `Saldos/Index.cshtml + Detail.cshtml` (balance labels: Acumulado total, Pendientes, Días gozados, Disponible), `Auditoria/Index.cshtml` (redacted payload column), `Aprobadores/Index.cshtml`, `Aprobadores/Capacidad.cshtml` (modal with reason, confirmation, rowVersion); server-side pagination on all lists (RBFV §8.11–8.16); WCAG 2.1 AA; no emojis — `src/NovaLeave.Presentation.Web/Views/RRHH/`
- [ ] T098 [P] [US6] Add HR context navigation to `_Layout.cshtml`: "Solicitudes", "Calendario", "Saldos", "Auditoría", "Aprobadores" → respective routes (RBFV §7.3) — `src/NovaLeave.Presentation.Web/Views/Shared/_Layout.cshtml`
- [ ] T099 [P] [US6] E2E test for HR views: login as `hr@demo`; navigate all HR views; verify no resolution buttons; attempt forced-browse to approve URL → 403; toggle Approver capability → success confirmation; verify audit log shows toggle event — `tests/NovaLeave.EndToEndTests/CriticalJourneys/HRViewsTests.cs`

**Checkpoint**: User Story 6 fully functional. HR can access all read-only views; capability toggle works with full concurrency and audit. T089–T092 pass.

---

## Phase 9: User Story 7 — Basic Vacation Calendar (Priority: P3)

**Goal**: Authenticated Users see their own Approved periods in an accessible monthly calendar; Approvers see anonymized org-wide periods; navigation to request detail works via keyboard and mouse.

**Independent Test**: Authenticate as `user@demo`; verify calendar shows own Approved periods with accessible aria-labels; weekends visually distinct, no events on weekends; keyboard navigation works; event activation navigates to request detail.

### Tests for User Story 7

- [ ] T100 [P] [US7] Integration tests for calendar: User calendar shows only own Approved periods (no other Users', UC-08), Approver calendar shows anonymized org-wide periods (UC-15), HR calendar shows named org-wide (UC-19); unauthorized event navigation denied; expired session → re-auth (AC-057, FR-015, FR-024) — `tests/NovaLeave.IntegrationTests/Calendar/CalendarAccessTests.cs`

### Implementation for User Story 7

- [ ] T101 [US7] Implement `GetCalendarQuery` + handler: parameter `(userId, role, year, month)`; User role → own Approved only; Approver → org-wide Approved anonymized; HR → org-wide Approved with requester names; map to `CalendarDto` with event links per role — `src/NovaLeave.Application/Calendar/GetCalendarQuery.cs`
- [ ] T102 [US7] Create `CalendarioController` with `Index` GET action; authorized for `RequireActiveUser` OR `RequireActiveApprover` (shared route `/calendario`); dispatch query based on active role context — `src/NovaLeave.Presentation.Web/Controllers/CalendarioController.cs`
- [ ] T103 [P] [US7] Create `CalendarViewModel` and `Calendario/Index.cshtml`: month grid as `<table>` with `scope="col"` day headers Mon–Sun; weekend columns visually distinct (muted background); Approved-period events as `<span>` with `aria-label`; keyboard navigation (arrow keys, Home/End, PageUp/PageDown); no events on weekend cells; `aria-live` region for month change; event activation via Enter/Space navigates to authorized detail (RBFV §8.6, §8.9, frontend-design-spec.md §6.3) — `src/NovaLeave.Presentation.Web/ViewModels/CalendarViewModel.cs`, `src/NovaLeave.Presentation.Web/Views/Calendario/Index.cshtml`, `src/NovaLeave.Presentation.Web/wwwroot/js/calendar.js`
- [ ] T104 [P] [US7] E2E test for calendar: keyboard navigation between months, event activation, weekend non-focusable, month-change announcement via aria-live — `tests/NovaLeave.EndToEndTests/CriticalJourneys/CalendarNavigationTests.cs`

**Checkpoint**: User Story 7 fully functional. Calendar renders correctly per role; keyboard navigation works per RBFV §8.6. T100 and T104 pass.

---

## Phase 10: Polish and Cross-Cutting Concerns

**Purpose**: Context switcher, shared accessible components, accessibility validation, security hardening, CI finalization, and documentation.

- [ ] T105 [P] Implement "Mis roles" context switcher in `_Layout.cshtml` and `_ContextSwitcher.cshtml`: accessible dropdown visible only when identity has ≥ 2 roles; options Mi espacio / Aprobaciones / RRHH; switching navigates to context root and updates nav only — no session/claims/identity modification; full keyboard support (arrows, Esc, Enter/Space); no emojis (RBFV §4.2, frontend-design-spec.md §14, UC-02) — `src/NovaLeave.Presentation.Web/Views/Shared/_ContextSwitcher.cshtml`
- [ ] T106 [P] E2E test: context-switcher journey — log in as `multi@demo`; switch contexts; verify navigation changes; verify identity/session unchanged; verify resource authorization re-evaluated per request — `tests/NovaLeave.EndToEndTests/CriticalJourneys/ContextSwitcherTests.cs`
- [ ] T107 [P] Implement `_UserMenu.cshtml` shared partial: avatar/initials, `aria-haspopup="menu"`, keyboard-navigable dropdown with "Cerrar sesión" POST form + antiforgery; "Perfil" placeholder with `aria-disabled="true"`; no emojis (frontend-design-spec.md §6.6) — `src/NovaLeave.Presentation.Web/Views/Shared/_UserMenu.cshtml`
- [ ] T108 [P] Apply reduced-motion CSS: `@media (prefers-reduced-motion: reduce)` block disabling all animations to 1ms; verify all transitions < 300ms and comply with motion tokens from frontend-design-spec.md §4.5 and §8 — `src/NovaLeave.Presentation.Web/wwwroot/css/site.css`
- [ ] T109 [P] Implement server-side pagination shared component: page-size selector (10/25/50), first/prev/next/last with ellipsis, accessible `aria-label` on each control, total count via `aria-live="polite"` (RBFV §8.2, §8.7, §8.10–8.16) — `src/NovaLeave.Presentation.Web/Views/Shared/_Pagination.cshtml`
- [ ] T110 [P] Run accessibility smoke tests with Playwright + axe-core: verify WCAG 2.1 AA compliance on all main views (login, create request, approver queue, HR request list, calendar); check visible focus rings, contrast, label associations, landmark structure — `tests/NovaLeave.EndToEndTests/CriticalJourneys/AccessibilitySmokeTests.cs`
- [ ] T111 [P] Add security HTTP headers to middleware pipeline: CSP, `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`; configure HSTS in production; configure rate limiting on `/Identity/Account/Login`; disable Swagger UI in production (Constitution §7.2) — `src/NovaLeave.Presentation.Web/Program.cs`
- [ ] T112 [P] Write concurrency integration tests: approval-vs-approval (one winner — AC-019, CON-002), concurrent overlapping submissions (AC-040, CON-008), approval-vs-timeout (AC-049, CON-009), edit-vs-approval RowVersion collision (CON-004), HR capability stale-version (UC-22, CON-004 analog) — `tests/NovaLeave.IntegrationTests/LeaveRequests/ConcurrencyTests.cs`
- [ ] T113 [P] Write redaction integration tests: verify Reason and RejectionReason never appear in Serilog output, AuditRecord.Data, BalanceMovement, or technical error responses; verify HR reason-access audit records contain only field name (not content) (SEC-005, SEC-006, AUD-006, AC-029, SC-007) — `tests/NovaLeave.IntegrationTests/LeaveRequests/RedactionTests.cs`
- [ ] T114 [P] Write audit immutability tests: attempt update and delete on AuditRecord and BalanceMovement via DbContext → verify operations blocked; verify no application code path physically deletes audit records (AUD-005, Constitution §6) — `tests/NovaLeave.IntegrationTests/LeaveRequests/AuditImmutabilityTests.cs`
- [ ] T115 Write `docs/adr/ADR-001-Architecture.md` (Clean Architecture decision), `ADR-002-Auth.md` (Identity cookie auth), `ADR-003-Concurrency.md` (RowVersion + one-winner transactions), `ADR-004-AccrualSemantics.md` (OQ-002 resolution) — `docs/adr/`
- [ ] T116 [P] Finalize CI: verify coverage >= 80% for Domain and Application; integrate SAST (Roslyn analyzers, `dotnet-security-audit`); add migration validation step; pin Bootstrap version in CI vulnerability scan — `.github/workflows/ci.yml`
- [ ] T117 Run `quickstart.md` validation: execute every planned command from `specs/001-leave-management-mvp/quickstart.md`; update any paths that changed during implementation; confirm demo seeding works end-to-end — `specs/001-leave-management-mvp/quickstart.md`

**Checkpoint**: All user stories independently testable. Accessibility smoke passes. Concurrency, redaction, and audit-immutability tests pass. CI pipeline green. Coverage >= 80% in Domain + Application.

---

## Dependencies & Execution Order

### Phase Dependencies

| Phase | Depends On | Can Parallelize Within Phase? |
|-------|-----------|-------------------------------|
| Phase 1 — Setup | None | Most tasks [P] |
| Phase 2 — Foundational | Phase 1 | Domain tasks [P] in pairs; Infrastructure after Domain |
| Phase 3 — US1 | Phase 2 | Tests [P]; Models+Services [P]; Controller→Views sequentially |
| Phase 4 — US2 | Phase 2 + Phase 3 (queue depends on US1 data) | Tests [P]; Queries [P] |
| Phase 5 — US3 | Phase 2 + Phase 3 | Single-thread; background job |
| Phase 6 — US4 | Phase 4 (needs approved requests) | Tests [P] |
| Phase 7 — US5 | Phase 2 | Tests [P]; Queries [P] |
| Phase 8 — US6 | Phase 2 + Phase 3 + Phase 7 (needs balance data) | Tests [P]; Queries [P] |
| Phase 9 — US7 | Phase 3 + Phase 4 (needs approved requests to display) | Single-phase |
| Phase 10 — Polish | Phase 3–9 | All tasks [P] |

### User Story Dependencies

- **US1 (P1)**: Start after Foundational only — independent
- **US2 (P1)**: Start after Foundational; integrates with US1 data (requires created requests)
- **US3 (P2)**: Start after Foundational; requires US1 (requests must exist to time out)
- **US4 (P2)**: Start after Foundational; requires US2 (requests must be Approved)
- **US5 (P2)**: Start after Foundational; partially integrates with US1 (balance display completes the story)
- **US6 (P2)**: Start after Foundational; reads US1+US5 data; independent of US2/US3/US4
- **US7 (P3)**: Start after US1+US2 (needs Approved requests to display)

---

## Parallel Execution Examples

### Phase 2 Parallelism (within Foundational)
```
Parallel group A (Domain):
  Task: T011 — Enums
  Task: T012 — DateRange + LeaveType
  Task: T013 — WorkingDays + NormalizedText

Sequential after A:
  T014 WorkingDaysCalculator (depends on T012, T013)
  T015 WorkingDaysCalculator tests [P with T016]
  T016 BalanceMovement entity [P with T015]
  T017 AuditRecord entity [P]
  T018 DomainException [P]
  T019 VacationBalance (depends on T016)
  T020 VacationBalance tests [P with T021]
  T021 VacationRequest (depends on T012, T013, T014)
  T022 VacationRequest tests [P with T023]
  T023 AccrualService [P with T022]
  T024 AccrualService tests

Parallel group B (Infrastructure — after domain complete):
  T025 ApplicationUser
  T026 DbContext
  T027, T028, T029, T030, T031 EF Configurations [all P]
  T032 Migration (depends on T027–T031)
  T033 DemoUserSeeder (depends on T032)
  T034, T035 Options + IAuditWriter [P]

Parallel group C (Presentation foundation):
  T036–T044 — mostly [P] within group
```

### Phase 3 Parallelism (US1)
```
Parallel: T045 + T046 + T047 + T048 (test writing)
Sequential: T049 CreateCommand → T050 EditCommand
Parallel: T051 + T052 (read queries)
Sequential: T053 Controller → T054 [P] ViewModels → T055 Views
Parallel: T056 + T057 + T058 (shared partials, nav)
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: US1 (User creates and manages requests)
4. Complete Phase 4: US2 (Approver resolves requests)
5. **STOP AND VALIDATE**: Full Create→Approve/Reject lifecycle works end-to-end
6. Deploy or demo

### Incremental Delivery

| Sprint | Deliverable | Business Value |
|--------|------------|----------------|
| 1 | Setup + Foundational | Infrastructure ready |
| 2 | US1 | Users can submit and manage requests |
| 3 | US2 | Approvers can resolve requests |
| 4 | US3 + US4 | Automated timeout; pre-start deactivation |
| 5 | US5 + US6 | Balance visibility; HR read-only access |
| 6 | US7 + Polish | Calendar; accessibility; security hardening |

---

## Notes

- `[P]` = different files, no dependency on incomplete tasks in same phase — safe to run in parallel
- `[US#]` = user story traceability for independent implementation and testing
- Every Domain business rule has positive, negative, and boundary unit tests (Constitution Principle VII)
- Every critical endpoint has integration tests (Constitution Principle VII)
- `TimeProvider` used throughout; no `DateTime.Now` in Domain or Application
- All POST actions use `[AutoValidateAntiforgeryToken]`; all mutations follow PRG pattern
- Sensitive `Reason` and `RejectionReason` fields: never in logs, AuditRecord.Data, BalanceMovement, or error responses
- `NovaLeave:PendingRequestTimeoutDays` and `NovaLeave:SessionTimeoutMinutes`: NEEDS CONFIGURATION before running
