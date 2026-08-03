# Tasks: NovaLeave MVP — Vacation Request Management

**Input**: Design documents from `specs/001-leave-management-mvp/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `quickstart.md`, `contracts/uc-contracts.md`, `docs/use-cases.md`, `frontend-design-spec.md`, `specs/002-role-based-frontend-views/spec.md`

**Tests**: Required by Constitution v7.0.0 test-first governance. Write the listed tests first and verify they fail before implementation.

**Organization**: Tasks are grouped by independently testable MVP increments. Story labels map to the restored approved user stories and UC contracts; each task includes exact planned paths.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the planned solution skeleton and baseline tooling without implementing feature behavior.

- [X] T001 Create solution file `NovaLeave.sln` and planned project directories `src/NovaLeave.Domain/`, `src/NovaLeave.Application/`, `src/NovaLeave.Infrastructure/`, `src/NovaLeave.Web/`, `tests/NovaLeave.UnitTests/`, `tests/NovaLeave.IntegrationTests/`, and `tests/NovaLeave.EndToEndTests/`
- [X] T002 Initialize .NET 10 projects and references matching Clean Architecture in `NovaLeave.sln`
- [X] T003 [P] Add shared build settings, nullable reference types, analyzers, and formatting configuration in `Directory.Build.props` and `.editorconfig`
- [X] T004 [P] Configure test project packages for xUnit, WebApplicationFactory, EF Core SQL Server integration tests, and approved E2E tooling in `tests/NovaLeave.UnitTests/NovaLeave.UnitTests.csproj`, `tests/NovaLeave.IntegrationTests/NovaLeave.IntegrationTests.csproj`, and `tests/NovaLeave.EndToEndTests/NovaLeave.EndToEndTests.csproj`
- [X] T005 [P] Configure MVC and Razor Views package references in `src/NovaLeave.Web/NovaLeave.Web.csproj`
- [X] T006 [P] Configure ASP.NET Core Identity UI package references in `src/NovaLeave.Web/NovaLeave.Web.csproj`
- [X] T007 [P] Configure FluentValidation package references in `src/NovaLeave.Application/NovaLeave.Application.csproj` and `src/NovaLeave.Web/NovaLeave.Web.csproj`
- [X] T008 [P] Configure EF Core SQL Server package references in `src/NovaLeave.Infrastructure/NovaLeave.Infrastructure.csproj`
- [X] T009 [P] Configure Serilog package references in `src/NovaLeave.Infrastructure/NovaLeave.Infrastructure.csproj` and `src/NovaLeave.Web/NovaLeave.Web.csproj`
- [X] T010 [P] Configure Bootstrap 5.3 asset management in `src/NovaLeave.Web/libman.json` or `src/NovaLeave.Web/package.json`
- [X] T011 Add application settings placeholders with no invented defaults in `src/NovaLeave.Web/appsettings.json`
- [X] T012 Verify `AGENTS.md` Spec Kit section references `specs/001-leave-management-mvp/plan.md`; update only if the reference is missing or stale

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core architecture, domain model, persistence, authorization, validation, audit, and test harness shared by all use cases.

**CRITICAL**: No user-story implementation starts until this phase is complete.

### Tests First

- [X] T013 [P] Add Clean Architecture dependency tests in `tests/NovaLeave.UnitTests/Architecture/CleanArchitectureTests.cs`
- [X] T014 [P] Add request lifecycle invariant tests for five states (`Pending`, `Approved`, `Rejected`, `CancelledByTimeout`, `CancelledByApprover`) and four official transitions (`Pending -> Approved`, `Pending -> Rejected`, `Pending -> CancelledByTimeout`, `Approved -> CancelledByApprover`) in `tests/NovaLeave.UnitTests/Domain/VacationRequestLifecycleTests.cs`
- [X] T015 [P] Add working-day calculation tests for Mon-Fri, weekend exclusion, and holiday-as-working-day behavior in `tests/NovaLeave.UnitTests/Domain/WorkingDaysCalculatorTests.cs`
- [X] T016 [P] Add balance invariant tests for non-negative accrued/reserved/deducted/available totals in `tests/NovaLeave.UnitTests/Domain/VacationBalanceTests.cs`
- [X] T017 [P] Add authorization policy tests for User, Approver, HR, Active/Inactive, self-resolution denial, and `canResolveRequests` in `tests/NovaLeave.IntegrationTests/Security/AuthorizationPolicyTests.cs`
- [X] T018 [P] Add audit schema and redaction tests in `tests/NovaLeave.UnitTests/Domain/AuditRecordTests.cs`
- [X] T019 [P] Add configuration validation tests for `NovaLeave:PendingRequestTimeoutDays`, `NovaLeave:SessionTimeoutMinutes`, and accrual scheduler cadence in `tests/NovaLeave.UnitTests/Configuration/NovaLeaveOptionsTests.cs`

### Implementation

- [X] T020 Create request status enum with exactly five states in `src/NovaLeave.Domain/Enums/RequestStatus.cs`
- [X] T021 Create movement type enum in `src/NovaLeave.Domain/Enums/MovementType.cs`
- [X] T022 Create Vacation-only leave type enum or constant in `src/NovaLeave.Domain/Enums/LeaveType.cs`
- [X] T023 Create DateRange value object in `src/NovaLeave.Domain/ValueObjects/DateRange.cs`
- [X] T024 Create WorkingDayCount value object in `src/NovaLeave.Domain/ValueObjects/WorkingDayCount.cs`
- [X] T025 Create AccrualPeriod value object in `src/NovaLeave.Domain/ValueObjects/AccrualPeriod.cs`
- [X] T026 Create WorkingDaysCalculator domain service using built-in .NET `TimeProvider` inputs where current date is needed in `src/NovaLeave.Domain/Services/WorkingDaysCalculator.cs`
- [X] T027 Create OverlapPolicy domain service in `src/NovaLeave.Domain/Services/OverlapPolicy.cs`
- [X] T028 Create VacationRequest aggregate with states `Pending`, `Approved`, `Rejected`, `CancelledByTimeout`, and `CancelledByApprover`, and only the official transitions `Pending -> Approved`, `Pending -> Rejected`, `Pending -> CancelledByTimeout`, and `Approved -> CancelledByApprover` in `src/NovaLeave.Domain/Entities/VacationRequest.cs`
- [X] T029 Create VacationBalance aggregate with non-negative balance invariants in `src/NovaLeave.Domain/Entities/VacationBalance.cs`
- [X] T030 Create immutable BalanceMovement entity in `src/NovaLeave.Domain/Entities/BalanceMovement.cs`
- [X] T031 Create immutable AuditRecord entity with redaction-ready Data field in `src/NovaLeave.Domain/Entities/AuditRecord.cs`
- [X] T032 Create Application DbContext abstraction in `src/NovaLeave.Application/Common/Interfaces/IApplicationDbContext.cs`
- [X] T033 Create Application authorization facade abstraction in `src/NovaLeave.Application/Common/Interfaces/IAuthorizationServiceFacade.cs`
- [X] T034 Create Application audit writer abstraction in `src/NovaLeave.Application/Common/Interfaces/IAuditWriter.cs`
- [X] T035 Create Application current-user abstraction in `src/NovaLeave.Application/Common/Interfaces/ICurrentUser.cs`
- [X] T036 Create Application result and error types for MVC/Razor outcomes in `src/NovaLeave.Application/Common/Results/Result.cs` and `src/NovaLeave.Application/Common/Errors/ErrorCodes.cs`
- [X] T037 Create typed configuration model in `src/NovaLeave.Application/Configuration/NovaLeaveOptions.cs`
- [X] T038 Create Infrastructure Identity user extension in `src/NovaLeave.Infrastructure/Identity/ApplicationUser.cs`
- [X] T039 Create EF Core DbContext shell in `src/NovaLeave.Infrastructure/Persistence/NovaLeaveDbContext.cs`
- [X] T040 Create VacationRequest EF Core mapping in `src/NovaLeave.Infrastructure/Persistence/Configurations/VacationRequestConfiguration.cs`
- [X] T041 Create VacationBalance EF Core mapping in `src/NovaLeave.Infrastructure/Persistence/Configurations/VacationBalanceConfiguration.cs`
- [X] T042 Create BalanceMovement EF Core mapping in `src/NovaLeave.Infrastructure/Persistence/Configurations/BalanceMovementConfiguration.cs`
- [X] T043 Create AuditRecord EF Core mapping in `src/NovaLeave.Infrastructure/Persistence/Configurations/AuditRecordConfiguration.cs`
- [X] T044 Create ApplicationUser EF Core mapping in `src/NovaLeave.Infrastructure/Persistence/Configurations/ApplicationUserConfiguration.cs`
- [X] T045 Create initial EF Core migration plan implementation in `src/NovaLeave.Infrastructure/Persistence/Migrations/`
- [X] T046 Register ASP.NET Core Identity in `src/NovaLeave.Infrastructure/DependencyInjection.cs`
- [X] T047 Register EF Core SQL Server DbContext in `src/NovaLeave.Infrastructure/DependencyInjection.cs`
- [X] T048 Register Serilog request logging and structured logging enrichment in `src/NovaLeave.Web/Program.cs`
- [X] T049 Register built-in .NET `TimeProvider` directly in `src/NovaLeave.Web/Program.cs`
- [X] T050 Register `NovaLeaveOptions` validation with startup failure for missing/invalid values in `src/NovaLeave.Web/Program.cs`
- [X] T051 Create MVC filters for antiforgery, deny-by-default authorization, overposting prevention, ModelState validation feedback, safe error pages, and 403/404/409 responses in `src/NovaLeave.Web/Filters/`
- [X] T052 Create shared Razor layout shell in `src/NovaLeave.Web/Views/Shared/_Layout.cshtml`
- [X] T053 Create shared status badge partial in `src/NovaLeave.Web/Views/Shared/_StatusBadge.cshtml`
- [X] T054 Create shared toast partial in `src/NovaLeave.Web/Views/Shared/_Toast.cshtml`
- [X] T055 Create shared validation summary partial in `src/NovaLeave.Web/Views/Shared/_ValidationSummary.cshtml`
- [X] T056 Create shared calendar partial shell in `src/NovaLeave.Web/Views/Shared/_Calendar.cshtml`
- [X] T057 Create integration test factory in `tests/NovaLeave.IntegrationTests/Support/NovaLeaveWebApplicationFactory.cs`
- [X] T058 Create SQL Server test fixture in `tests/NovaLeave.IntegrationTests/Support/SqlServerFixture.cs`

**Checkpoint**: Foundation ready; user-story phases can begin.

---

## Phase 3: User Story 1 - User Submits and Manages a Vacation Request (Priority: P1)

**Covers**: UC-01 through UC-08.

**Independent Test**: Authenticate as an active User, create valid requests using both input modes, edit an owned Pending request, view own request detail, balance/history, and personal calendar; verify authorization, validation, reservation, audit, and owner-only visibility.

### Tests for User Story 1

- [X] T059 [P] [US1] Add UC-01 authentication integration tests in `tests/NovaLeave.IntegrationTests/UseCases/UC01AuthenticateTests.cs`
- [X] T060 [P] [US1] Add UC-02 role context switching integration tests in `tests/NovaLeave.IntegrationTests/UseCases/UC02SwitchRoleContextTests.cs`
- [X] T061 [P] [US1] Add UC-03 own request list integration tests in `tests/NovaLeave.IntegrationTests/UseCases/UC03ViewOwnRequestsTests.cs`
- [X] T062 [P] [US1] Add UC-04 create request validation, reservation, audit, and overlap tests in `tests/NovaLeave.IntegrationTests/UseCases/UC04CreateVacationRequestTests.cs`
- [X] T063 [P] [US1] Add UC-05 Pending edit revalidation and reservation-adjustment tests in `tests/NovaLeave.IntegrationTests/UseCases/UC05EditPendingRequestTests.cs`
- [X] T064 [P] [US1] Add UC-06 owned detail authorization tests in `tests/NovaLeave.IntegrationTests/UseCases/UC06ViewOwnedRequestDetailTests.cs`
- [X] T065 [P] [US1] Add UC-07 balance and movement history tests in `tests/NovaLeave.IntegrationTests/UseCases/UC07ViewOwnBalanceTests.cs`
- [X] T066 [P] [US1] Add UC-08 personal calendar tests in `tests/NovaLeave.IntegrationTests/UseCases/UC08PersonalCalendarTests.cs`
- [X] T067 [P] [US1] Add User MVC accessibility and route smoke tests in `tests/NovaLeave.EndToEndTests/User/UserJourneySmokeTests.cs`

### Implementation for User Story 1

- [X] T068 [P] [US1] Implement User request query use cases in `src/NovaLeave.Application/Requests/Queries/`
- [X] T069 [P] [US1] Implement User balance query use cases in `src/NovaLeave.Application/Balances/Queries/`
- [X] T070 [P] [US1] Implement personal calendar query use case in `src/NovaLeave.Application/Calendars/GetPersonalCalendar/`
- [X] T071 [US1] Implement create vacation request command with reservation and audit transaction in `src/NovaLeave.Application/Requests/CreateVacationRequest/`
- [X] T072 [US1] Implement edit Pending request command with full revalidation, reservation adjustment, rowversion, and audit in `src/NovaLeave.Application/Requests/EditPendingRequest/`
- [X] T073 [US1] Implement FluentValidation validators for User request commands in `src/NovaLeave.Application/Requests/Validation/`
- [X] T074 [US1] Implement resource-oriented MVC controllers for `/mis-solicitudes`, `/mis-solicitudes/crear`, `/mis-solicitudes/{id}`, `/saldo`, and User `/calendario` actions in `src/NovaLeave.Web/Controllers/MisSolicitudesController.cs` and `src/NovaLeave.Web/Controllers/CalendarioController.cs`
- [X] T075 [US1] Implement dedicated User ViewModels in `src/NovaLeave.Web/ViewModels/MisSolicitudes/` and `src/NovaLeave.Web/ViewModels/Calendario/`
- [X] T076 [US1] Implement User Razor views with approved Spanish labels in `src/NovaLeave.Web/Views/MisSolicitudes/` and `src/NovaLeave.Web/Views/Calendario/`
- [X] T077 [US1] Wire User navigation and role context switcher in `src/NovaLeave.Web/Views/Shared/_Layout.cshtml`

**Checkpoint**: UC-01 through UC-08 independently pass.

---

## Phase 4: User Story 2 - Approver Resolves a Vacation Request (Priority: P1)

**Covers**: UC-09, UC-10, UC-11, UC-12, UC-14, UC-15.

**Independent Test**: Authenticate as an active Approver with `canResolveRequests=true`, view all eligible non-owned Pending requests, approve/reject globally, deny self-resolution and inactive/capability-disabled resolution, and verify transaction/audit/concurrency behavior.

### Tests for User Story 2

- [X] T078 [P] [US2] Add UC-09 eligible queue tests without team/department/hierarchy filters in `tests/NovaLeave.IntegrationTests/UseCases/UC09ApproverQueueTests.cs`
- [X] T079 [P] [US2] Add UC-10 resolution detail tests for BR-036/BR-037 projected-balance calculation/display and disabled Approver denial in `tests/NovaLeave.IntegrationTests/UseCases/UC10ApproverDetailTests.cs`
- [X] T080 [P] [US2] Add UC-11 approval transaction, audit, BR-037 negative projected-balance rejection, and BR-038 approval POST revalidation tests in `tests/NovaLeave.IntegrationTests/UseCases/UC11ApproveRequestTests.cs`
- [X] T081 [P] [US2] Add UC-12 rejection reason, release, and audit tests in `tests/NovaLeave.IntegrationTests/UseCases/UC12RejectRequestTests.cs`
- [X] T082 [P] [US2] Add UC-14 resolution history tests in `tests/NovaLeave.IntegrationTests/UseCases/UC14ResolutionHistoryTests.cs`
- [X] T083 [P] [US2] Add UC-15 Approver calendar tests in `tests/NovaLeave.IntegrationTests/UseCases/UC15ApproverCalendarTests.cs`
- [X] T084 [P] [US2] Add approval/rejection concurrency race tests in `tests/NovaLeave.IntegrationTests/Concurrency/ApprovalConcurrencyTests.cs`
- [X] T085 [P] [US2] Add Approver MVC E2E smoke tests in `tests/NovaLeave.EndToEndTests/Approver/ApproverJourneySmokeTests.cs`

### Implementation for User Story 2

- [X] T086 [P] [US2] Implement Approver queue and detail queries with `canResolveRequests=true` eligibility and BR-036/BR-037 server-derived projected-balance fields in `src/NovaLeave.Application/Approvals/Queries/`
- [X] T087 [P] [US2] Implement Approver history and calendar queries in `src/NovaLeave.Application/Approvals/History/` and `src/NovaLeave.Application/Calendars/GetApproverCalendar/`
- [X] T088 [US2] Implement approval command converting reservation to deduction atomically with `canResolveRequests=true` revalidation, BR-037 negative projected-balance rejection, and BR-038 stale/concurrent projected-balance revalidation in `src/NovaLeave.Application/Approvals/ApproveRequest/`
- [X] T089 [US2] Implement rejection command releasing reservation atomically in `src/NovaLeave.Application/Approvals/RejectRequest/`
- [X] T090 [US2] Implement Approver authorization resource checks requiring authenticated active `Approver`, `canResolveRequests=true`, non-ownership, eligible request state, and Application-layer execution-time revalidation in `src/NovaLeave.Application/Authorization/ApproverPolicies.cs`
- [X] T091 [US2] Implement resource-oriented Approver MVC controller actions for `/aprobaciones`, `/aprobaciones/{id}`, `/aprobaciones/{id}/aprobar`, `/aprobaciones/{id}/rechazar`, `/aprobaciones/historial`, and shared calendar route `/calendario` in `src/NovaLeave.Web/Controllers/AprobacionesController.cs` and `src/NovaLeave.Web/Controllers/CalendarioController.cs`
- [X] T092 [US2] Implement dedicated Approver ViewModels in `src/NovaLeave.Web/ViewModels/Aprobaciones/` and `src/NovaLeave.Web/ViewModels/Calendario/`
- [X] T093 [US2] Implement Approver Razor views with mutual approve/reject submission protections in `src/NovaLeave.Web/Views/Aprobaciones/`

**Checkpoint**: UC-09 through UC-12, UC-14, and UC-15 independently pass.

---

## Phase 5: User Story 3 - System Cancels an Unresolved Request by Timeout (Priority: P2)

**Covers**: UC-16.

**Independent Test**: With a Pending request older than configured `NovaLeave:PendingRequestTimeoutDays`, run timeout processing and verify `CancelledByTimeout`, reservation release, single system-actor audit, idempotency, and approval-race behavior.

### Tests for User Story 3

- [X] T094 [P] [US3] Add UC-16 timeout transition tests in `tests/NovaLeave.IntegrationTests/UseCases/UC16TimeoutCancellationTests.cs`
- [X] T095 [P] [US3] Add timeout idempotency tests in `tests/NovaLeave.IntegrationTests/Idempotency/TimeoutIdempotencyTests.cs`
- [X] T096 [P] [US3] Add timeout versus approval concurrency tests in `tests/NovaLeave.IntegrationTests/Concurrency/TimeoutApprovalRaceTests.cs`

### Implementation for User Story 3

- [X] T097 [US3] Implement timeout cancellation command in `src/NovaLeave.Application/System/CancelTimedOutRequests/`
- [X] T098 [US3] Implement system actor audit writer support in `src/NovaLeave.Application/Audit/SystemAuditWriter.cs`
- [X] T099 [US3] Implement configured timeout hosted-job adapter without default cadence in `src/NovaLeave.Infrastructure/Scheduling/PendingRequestTimeoutJob.cs`
- [X] T100 [US3] Register timeout job only when required configuration is present and valid in `src/NovaLeave.Infrastructure/DependencyInjection.cs`

**Checkpoint**: UC-16 independently passes.

---

## Phase 6: User Story 4 - Approver Deactivates an Approved Request Before It Begins (Priority: P2)

**Covers**: UC-13.

**Independent Test**: With a future Approved request, an active non-owner Approver deactivates it before start and verifies `CancelledByApprover`, balance restoration, audit, denial after start, denial of partial deactivation, and duplicate/dead-row concurrency behavior.

### Tests for User Story 4

- [X] T101 [P] [US4] Add UC-13 pre-start deactivation tests in `tests/NovaLeave.IntegrationTests/UseCases/UC13DeactivateApprovedRequestTests.cs`
- [X] T102 [P] [US4] Add post-start and partial-deactivation denial tests in `tests/NovaLeave.IntegrationTests/UseCases/UC13DeactivateDenialTests.cs`
- [X] T103 [P] [US4] Add duplicate deactivation concurrency tests in `tests/NovaLeave.IntegrationTests/Concurrency/DeactivationConcurrencyTests.cs`

### Implementation for User Story 4

- [X] T104 [US4] Implement pre-start deactivation command with restoration transaction in `src/NovaLeave.Application/Approvals/DeactivateApprovedRequest/`
- [X] T105 [US4] Extend Approver detail command surface for deactivation eligibility in `src/NovaLeave.Application/Approvals/Queries/GetApprovalDetail/`
- [X] T106 [US4] Add Approver POST route `/aprobaciones/{id}/desactivar` in `src/NovaLeave.Web/Controllers/AprobacionesController.cs`
- [X] T107 [US4] Add deactivation confirmation UI and denial messages in `src/NovaLeave.Web/Views/Aprobaciones/Detail.cshtml`

**Checkpoint**: UC-13 independently passes.

---

## Phase 7: User Story 5 - User Balance and Monthly Accrual (Priority: P2)

**Covers**: UC-07 and UC-17.

**Independent Test**: Run accrual for Users with `EmploymentStartDate` examples A and B; verify one whole day per fully completed calendar month, first partial month skipped, no proration, no expiry, idempotency by `(UserId, AccrualPeriod)`, catch-up once per eligible period, and correct balance history.

### Tests for User Story 5

- [X] T108 [P] [US5] Add OQ-002 accrual examples A and B tests in `tests/NovaLeave.UnitTests/Domain/MonthlyAccrualPolicyTests.cs`
- [X] T109 [P] [US5] Add UC-17 accrual integration tests in `tests/NovaLeave.IntegrationTests/UseCases/UC17MonthlyAccrualTests.cs`
- [X] T110 [P] [US5] Add duplicate accrual idempotency tests in `tests/NovaLeave.IntegrationTests/Idempotency/AccrualIdempotencyTests.cs`
- [X] T111 [P] [US5] Add catch-up accrual tests in `tests/NovaLeave.IntegrationTests/UseCases/UC17AccrualCatchUpTests.cs`

### Implementation for User Story 5

- [X] T112 [US5] Implement monthly accrual policy in `src/NovaLeave.Domain/Services/MonthlyAccrualPolicy.cs`
- [X] T113 [US5] Implement accrual command with unique `(UserId, AccrualPeriod)` idempotency in `src/NovaLeave.Application/System/ExecuteMonthlyAccrual/`
- [X] T114 [US5] Implement accrual scheduler adapter with DR-001-configured cadence in `src/NovaLeave.Infrastructure/Scheduling/MonthlyAccrualJob.cs`
- [X] T115 [US5] Add EF Core unique index for accrual idempotency in `src/NovaLeave.Infrastructure/Persistence/Configurations/BalanceMovementConfiguration.cs`
- [X] T116 [US5] Ensure balance history displays accrual, reservation, release, deduction, and restoration movements in `src/NovaLeave.Web/Views/MisSolicitudes/Balance.cshtml`

**Checkpoint**: UC-07 and UC-17 balance/accrual behavior independently pass.

---

## Phase 8: User Story 6 - Basic Vacation Calendar (Priority: P3)

**Covers**: UC-08, UC-15, and UC-19 shared calendar rendering.

**Independent Test**: Render authorized User and Approver `/calendario` views plus dedicated HR `/rrhh/calendario` view with correct event scope, colors, accessible keyboard activation, and authorized detail links only; verify HR receives no `/calendario` behavior and no role inherits another role's data scope.

### Tests for User Story 6

- [ ] T117 [P] [US6] Add shared calendar ViewModel unit tests in `tests/NovaLeave.UnitTests/Presentation/CalendarViewModelTests.cs`
- [ ] T118 [P] [US6] Add calendar authorization integration tests for User personal `/calendario`, eligible Approver anonymized `/calendario`, HR-only `/rrhh/calendario`, HR denial on `/calendario`, and disabled Approver denial in `tests/NovaLeave.IntegrationTests/UseCases/CalendarAuthorizationTests.cs`
- [ ] T119 [P] [US6] Add calendar accessibility E2E smoke tests in `tests/NovaLeave.EndToEndTests/Calendar/CalendarAccessibilityTests.cs`

### Implementation for User Story 6

- [ ] T120 [P] [US6] Implement shared calendar query models in `src/NovaLeave.Application/Calendars/CalendarModels.cs`
- [ ] T121 [US6] Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links in `src/NovaLeave.Web/Views/Shared/_Calendar.cshtml`
- [ ] T122 [US6] Integrate shared calendar partial into User and Approver `/calendario` views and the dedicated read-only HR `/rrhh/calendario` view without sharing route data scope under `src/NovaLeave.Web/Views/`

**Checkpoint**: Calendar behavior for UC-08, UC-15, and UC-19 independently passes.

---

## Phase 9: User Story 7 - HR Read-Only Organization-Wide Views (Priority: P2)

**Covers**: UC-18, UC-19, UC-20, UC-21 and AC-HR-001 through AC-HR-007, AC-HR-009 through AC-HR-012.

**Independent Test**: Authenticate as active HR and verify organization-wide read access to requests, calendar, balances, movements, and audit; verify no approve/reject/deactivate/balance-edit/role-assignment action is available or authorized.

### Tests for User Story 7

- [ ] T123 [P] [US7] Add UC-18 HR request list/detail tests in `tests/NovaLeave.IntegrationTests/UseCases/UC18HRRequestsTests.cs`
- [ ] T124 [P] [US7] Add UC-19 HR calendar tests for dedicated `/rrhh/calendario`, all vacation requests across the complete organization, requester/status visibility, read-only detail links, and forbidden mutations in `tests/NovaLeave.IntegrationTests/UseCases/UC19HRCalendarTests.cs`
- [ ] T125 [P] [US7] Add UC-20 HR balance and movement read-only tests in `tests/NovaLeave.IntegrationTests/UseCases/UC20HRBalancesTests.cs`
- [ ] T126 [P] [US7] Add UC-21 HR audit access and sensitive-reason read audit tests in `tests/NovaLeave.IntegrationTests/UseCases/UC21HRAuditTests.cs`
- [ ] T127 [P] [US7] Add HR forbidden mutation tests for approve/reject/deactivate/balance/role operations in `tests/NovaLeave.IntegrationTests/Security/HRRestrictionTests.cs`
- [ ] T128 [P] [US7] Add HR E2E read-only smoke tests in `tests/NovaLeave.EndToEndTests/HR/HRReadOnlySmokeTests.cs`

### Implementation for User Story 7

- [ ] T129 [P] [US7] Implement HR request read queries in `src/NovaLeave.Application/HR/Requests/`
- [ ] T130 [P] [US7] Implement HR calendar query returning all vacation requests across the complete organization with requester names, statuses, working-day counts, and no mutation capability in `src/NovaLeave.Application/HR/Calendar/`
- [ ] T131 [P] [US7] Implement HR balance and movement read queries in `src/NovaLeave.Application/HR/Balances/`
- [ ] T132 [P] [US7] Implement HR audit read queries with sensitive-reason access auditing in `src/NovaLeave.Application/HR/Audit/`
- [ ] T133 [US7] Implement HR authorization policies in `src/NovaLeave.Application/Authorization/HRPolicies.cs`
- [ ] T134 [US7] Implement resource-oriented HR MVC controller actions for `/rrhh/solicitudes`, dedicated read-only `/rrhh/calendario`, `/rrhh/saldos`, and `/rrhh/auditoria`; do not expose HR calendar behavior through `/calendario` in `src/NovaLeave.Web/Controllers/RRHHController.cs`
- [ ] T135 [US7] Implement dedicated HR read-only ViewModels in `src/NovaLeave.Web/ViewModels/RRHH/`
- [ ] T136 [US7] Implement HR read-only Razor views with no resolution or balance modification actions in `src/NovaLeave.Web/Views/RRHH/`

**Checkpoint**: UC-18 through UC-21 independently pass.

---

## Phase 10: User Story 8 - HR Manages Approver Resolution Capability (Priority: P2)

**Covers**: UC-22 and AC-HR-007 through AC-HR-009.

**Independent Test**: Authenticate as active HR, list Approver identities, toggle `canResolveRequests` only for identities that already hold Approver role with reason, confirmation, rowversion, audit, and authorization-state refresh; verify non-Approver, missing reason, stale rowversion, inactive HR, role assignment/removal, and request resolution are denied.

### Tests for User Story 8

- [ ] T137 [P] [US8] Add UC-22 Approver capability list tests in `tests/NovaLeave.IntegrationTests/UseCases/UC22ApproverCapabilityListTests.cs`
- [ ] T138 [P] [US8] Add UC-22 capability toggle validation, rowversion, and audit tests in `tests/NovaLeave.IntegrationTests/UseCases/UC22ApproverCapabilityToggleTests.cs`
- [ ] T139 [P] [US8] Add inactive HR and non-Approver target denial tests in `tests/NovaLeave.IntegrationTests/Security/HRCapabilityAuthorizationTests.cs`
- [ ] T140 [P] [US8] Add HR capability E2E smoke tests in `tests/NovaLeave.EndToEndTests/HR/HRCapabilitySmokeTests.cs`

### Implementation for User Story 8

- [ ] T141 [P] [US8] Implement Approver capability list query in `src/NovaLeave.Application/HR/ApproverCapabilities/ListApproverCapabilities/`
- [ ] T142 [US8] Implement `canResolveRequests` toggle command with reason, confirmation, rowversion, role revalidation, and audit in `src/NovaLeave.Application/HR/ApproverCapabilities/ToggleApproverCapability/`
- [ ] T143 [US8] Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad` in `src/NovaLeave.Web/Controllers/RRHHController.cs`
- [ ] T144 [US8] Implement capability management ViewModels in `src/NovaLeave.Web/ViewModels/RRHH/ApproverCapabilities/`
- [ ] T145 [US8] Implement capability list, modal confirmation, validation, concurrency, and toast UI in `src/NovaLeave.Web/Views/RRHH/ApproverCapabilities/`

**Checkpoint**: UC-22 independently passes.

---

## Phase 11: Polish & Cross-Cutting Validation

**Purpose**: Full regression, security, accessibility, traceability, and documentation checks after all selected user stories are complete.

- [ ] T146 [P] Add full traceability smoke test asserting UC-01 through UC-22 routes and authorization policies in `tests/NovaLeave.IntegrationTests/Traceability/UseCaseRouteTraceabilityTests.cs`
- [ ] T147 [P] Add lifecycle transition matrix regression tests in `tests/NovaLeave.UnitTests/Domain/RequestStateTransitionMatrixTests.cs`
- [ ] T148 [P] Add audit completeness regression tests for all successful state changes and Pending edits in `tests/NovaLeave.IntegrationTests/Audit/AuditCompletenessTests.cs`
- [ ] T149 [P] Add security regression tests for CSRF, IDOR, forced browsing, overposting, and session timeout in `tests/NovaLeave.IntegrationTests/Security/SecurityRegressionTests.cs`
- [ ] T150 [P] Add accessibility regression tests for User, Approver, and HR critical pages in `tests/NovaLeave.EndToEndTests/Accessibility/AccessibilityRegressionTests.cs`
- [ ] T151 Validate `specs/001-leave-management-mvp/quickstart.md` against the implemented application and update only if commands or paths changed
- [ ] T152 Run complete test suite from `NovaLeave.sln` and record evidence in implementation handoff notes
- [ ] T153 Review documentation references and ensure no generated artifact claims implementation evidence without inspected source in `specs/001-leave-management-mvp/`

---

## Phase 12: Observability, Manual Quality Gates, and Operational Readiness

**Purpose**: Close observability, production-invariant monitoring, operational readiness, and manual quality/security gate obligations without introducing CI/CD or changing approved product behavior.

### Governance and Manual Gate

- [ ] T154 [P] Document the Manual Quality and Security Gate procedure in `docs/operations/manual-quality-gate.md` per Constitution v7.0.0 and `docs/adr/DR-003-manual-quality-gate-no-ci-cd.md`
- [ ] T155 [P] Create the operations documentation index and ownership model in `docs/operations/README.md`
- [ ] T156 [P] Add the manual gate evidence checklist in `docs/operations/manual-quality-gate.md` covering commands, results, failures, exceptions, reviewer identity, and `NOT EXECUTED` handling

### Observability Implementation and Verification

- [ ] T157 [P] Add structured logging and correlation verification tests in `tests/NovaLeave.IntegrationTests/Observability/CorrelationLoggingTests.cs`
- [ ] T158 Implement request correlation ID generation or propagation, safe error correlation display, and structured log enrichment in `src/NovaLeave.Web/Program.cs`, `src/NovaLeave.Web/Middleware/`, and `src/NovaLeave.Infrastructure/Observability/`
- [ ] T159 [P] Add metrics instrumentation tests for request count, latency, error count, vacation request creation, approval, rejection, timeout cancellation, accrual execution, concurrency conflicts, and invariant violations in `tests/NovaLeave.IntegrationTests/Observability/MetricsTests.cs`
- [ ] T160 Implement application metrics instrumentation without paid external services in `src/NovaLeave.Infrastructure/Observability/` and application use cases
- [ ] T161 [P] Add tracing and redaction tests covering request, application operation, persistence action, and error handling spans in `tests/NovaLeave.IntegrationTests/Observability/TracingRedactionTests.cs`
- [ ] T162 Implement application tracing with correlation-safe spans and no sensitive reasons, secrets, tokens, or unapproved personal data in `src/NovaLeave.Infrastructure/Observability/`
- [ ] T163 [P] Add health-check endpoint tests for liveness, readiness, database connectivity, and secret-free responses in `tests/NovaLeave.IntegrationTests/Observability/HealthCheckTests.cs`
- [ ] T164 Implement liveness, readiness, and database connectivity health checks with safe responses in `src/NovaLeave.Web/Program.cs` and `src/NovaLeave.Infrastructure/HealthChecks/`

### Invariant Monitoring and Redaction

- [ ] T165 [P] Add invariant-monitoring tests for non-negative available balance, reservation and deduction consistency, terminal-state immutability, self-approval denial, HR resolution denial, disabled Approver denial, accrual idempotency, timeout idempotency, audit completeness, and duplicate movement prevention in `tests/NovaLeave.IntegrationTests/Observability/BusinessInvariantMonitoringTests.cs`
- [ ] T166 Implement read-only business invariant evaluation that emits metrics and structured events without self-healing or silent production data mutation in `src/NovaLeave.Application/Observability/` and `src/NovaLeave.Infrastructure/Observability/`
- [ ] T167 [P] Add observability redaction inspection tests for logs, traces, metrics labels, error payloads, audit payloads, and health-check responses in `tests/NovaLeave.IntegrationTests/Observability/ObservabilityRedactionTests.cs`
- [ ] T168 Document the manual observability redaction inspection procedure in `docs/operations/observability-runbook.md`

### Alerts and Operations

- [ ] T169 [P] Document alert definitions, thresholds, owners, and response procedures for repeated application errors, database health failure, accrual job failure, timeout job failure, persistent concurrency failures, failed invariant checks, and abnormal authentication or authorization denial patterns in `docs/operations/observability-runbook.md`
- [ ] T170 Implement alertable structured events and metrics for operational alert conditions without binding to a paid provider in `src/NovaLeave.Infrastructure/Observability/`
- [ ] T171 [P] Create deployment and incident response runbooks in `docs/operations/deployment-runbook.md` and `docs/operations/incident-response-runbook.md` without CI/CD or deployment automation
- [ ] T172 [P] Create backup and restore runbook in `docs/operations/backup-and-restore-runbook.md` covering assumptions, backup, restore, verification, responsible role, evidence, limitations, and configurable environment-specific commands
- [ ] T173 [P] Create database migration and scheduled jobs runbooks in `docs/operations/database-migration-runbook.md` and `docs/operations/scheduled-jobs-runbook.md`
- [ ] T174 [P] Document load and concurrency readiness procedure with representative dataset, concurrent request scenario, approval contention, accrual and timeout job behavior, expected measurements, and result recording in `docs/operations/observability-runbook.md`
- [ ] T175 Execute load and concurrency readiness verification in a local or test environment and record evidence without requiring production deployment
- [ ] T176 Execute the final Manual Quality and Security Gate and record restore, build, format, analyzer, test, coverage, security, dependency, license, migration, Mermaid, configuration, and secrets-handling results or `NOT EXECUTED` statuses in implementation handoff evidence
- [ ] T177 Validate no CI/CD pipeline configuration, automated deployment automation, or false official validator success claim exists in active docs or repository configuration
- [ ] T178 Synchronize canonical and derived task artifacts, rerun available task-tree validation or safe equivalent, and record results in handoff notes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 Setup**: no dependencies.
- **Phase 2 Foundational**: depends on Phase 1 and blocks all user-story phases.
- **Phase 3 US1** and **Phase 4 US2**: P1 increments; both depend on Phase 2. US2 can start in parallel once shared request/balance entities exist, but final approval tests require request creation fixtures from US1 support builders.
- **Phase 5 US3**, **Phase 6 US4**, **Phase 7 US5**, **Phase 9 US7**, and **Phase 10 US8**: P2 increments; depend on Phase 2 and relevant shared fixtures.
- **Phase 8 US6**: P3 calendar increment; depends on query models from User, Approver, and HR views selected for implementation.
- **Phase 11 Polish**: depends on all selected user-story phases.
- **Phase 12 Observability, Manual Quality Gates, and Operational Readiness**: depends on Phase 11 and blocks implementation handoff, release, or acceptance.

### User Story Dependencies

- **US1**: can be delivered first as MVP user request management.
- **US2**: depends on shared request/balance/audit foundations; integrates with US1 request fixtures.
- **US3**: depends on Pending request lifecycle and configured timeout options.
- **US4**: depends on Approved request lifecycle and Approver authorization.
- **US5**: depends on balance movements and `EmploymentStartDate` on `ApplicationUser`.
- **US6**: depends on calendar query scopes for implemented roles.
- **US7**: depends on HR authorization and read query foundations.
- **US8**: depends on Identity role membership and HR authorization.

### Parallel Opportunities

- T003 through T010 can run in parallel after T001/T002.
- T013 through T019 can run in parallel.
- Test tasks inside each user-story phase can run in parallel.
- Query implementation tasks marked `[P]` can run in parallel when they touch different feature folders.
- US3, US4, US5, US7, and US8 can be assigned in parallel after Phase 2 when fixtures and authorization foundations exist.

---

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1) and validate UC-01 through UC-08.
3. Complete Phase 4 (US2) and validate UC-09 through UC-12, UC-14, and UC-15.
4. Add P2 system, deactivation, accrual, and HR increments.
5. Add P3 calendar refinements and cross-cutting validation.

### Quality Gates

- Every test task must be written before its corresponding implementation task and initially fail.
- No task may introduce APIs, JWT, OpenAPI, email, Outbox, queues, Redis, microservices, teams, departments, managers, hierarchy, delegation, escalation, User Pending cancellation, holiday calendars, per-user timezones, HR request resolution, HR balance modification, or HR role assignment/removal.
- No task may introduce GitHub Actions, Azure DevOps, Jenkins, CI/CD, automated deployment pipelines, automated release pipelines, continuous delivery, continuous deployment, or production deployment automation for the MVP.
- No task may add a persisted LeaveType lookup table; `Vacation` remains a Domain enum/constant.
- Configuration values `NovaLeave:PendingRequestTimeoutDays`, `NovaLeave:SessionTimeoutMinutes`, and accrual scheduler cadence remain required configuration with no invented defaults; official values are decided in `docs/adr/DR-001-runtime-configuration-values.md` (14 days, 30 minutes, daily 00:05 UTC).
- The Manual Quality and Security Gate is blocking; unavailable tools must be recorded as `NOT EXECUTED`, never as `PASS`, and accepted exceptions require written rationale and approval.
