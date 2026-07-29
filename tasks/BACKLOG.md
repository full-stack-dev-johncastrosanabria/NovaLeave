# Product Backlog

## Executive Summary

NovaLeave MVP is a server-rendered ASP.NET Core MVC vacation request management system derived from the official Spec Kit planning artifacts. `specs/001-leave-management-mvp/tasks.md` remains the canonical task source. `/tasks` is only a derived execution view for navigation, traceability, dependency review, and execution readiness.

## Project Scope

The MVP includes one leave type, `Vacation`; combinable `User`, `Approver`, and `HR` roles; the automatic `System` actor; UC-01 through UC-22; five request states; and four official transitions. HR has organization-wide read access and limited `canResolveRequests` management for identities that already hold `Approver`.

## System Objectives

- Allow active Users to authenticate, create, view, and edit owned Pending vacation requests.
- Allow active Approvers with `canResolveRequests=true` to resolve eligible non-owned requests.
- Allow the System to cancel timed-out Pending requests and run idempotent monthly accrual.
- Allow HR to read organization-wide request, calendar, balance, and audit data and manage Approver resolution capability within approved limits.
- Preserve balance integrity, auditability, authorization, concurrency safety, accessibility, and Clean Architecture.

## Sources of Authority

- `.specify/memory/constitution.md`
- `specs/001-leave-management-mvp/spec.md`
- `docs/use-cases.md`
- `specs/001-leave-management-mvp/contracts/uc-contracts.md`
- `specs/001-leave-management-mvp/frontend-design-spec.md`
- `specs/002-role-based-frontend-views/spec.md`
- `specs/001-leave-management-mvp/plan.md`
- `specs/001-leave-management-mvp/research.md`
- `specs/001-leave-management-mvp/data-model.md`
- `specs/001-leave-management-mvp/quickstart.md`
- `specs/001-leave-management-mvp/tasks.md`
- `specs/001-leave-management-mvp/checklists/requirements.md`
- `specs/001-leave-management-mvp/diagrams/clean-architecture.md`
- `specs/001-leave-management-mvp/diagrams/core-data-relationships.md`
- `specs/001-leave-management-mvp/diagrams/request-lifecycle.md`
- `AGENTS.md`

## Actors

- `User`: creates, views, and edits owned `Pending` vacation requests and views own balance/history/calendar.
- `Approver`: resolves eligible non-owned requests when active and enabled.
- `HR`: organization-wide read-only access plus limited Approver capability management.
- `System`: timeout cancellation and monthly accrual.

## Modules

- Identity and Access
- Vacation Requests
- Balances and Accrual
- HR Queries and Capability
- Audit and Observability
- Frontend Presentation
- Quality, Security, and Release

## Epics

- EPIC-001 - Foundation (EPIC-001-foundation): TASK-001 through TASK-019
- EPIC-002 - Domain and Persistence (EPIC-002-domain-and-persistence): TASK-020 through TASK-058
- EPIC-003 - User Request Management (EPIC-003-user-request-management): TASK-059 through TASK-077
- EPIC-004 - Approver Resolution (EPIC-004-approver-resolution): TASK-078 through TASK-093
- EPIC-005 - Automated Timeout (EPIC-005-automated-timeout): TASK-094 through TASK-100
- EPIC-006 - Approved Request Deactivation (EPIC-006-approved-request-deactivation): TASK-101 through TASK-107
- EPIC-007 - Balance and Accrual (EPIC-007-balance-and-accrual): TASK-108 through TASK-116
- EPIC-008 - Calendars (EPIC-008-calendars): TASK-117 through TASK-122
- EPIC-009 - HR Read Only (EPIC-009-hr-read-only): TASK-123 through TASK-136
- EPIC-010 - Approver Capability Management (EPIC-010-approver-capability-management): TASK-137 through TASK-145
- EPIC-011 - Quality, Security, and Release (EPIC-011-quality-security-and-release): TASK-146 through TASK-153

## Features

- Authentication and role context switching.
- User request list, create, edit, detail, balance/history, and personal calendar.
- Approver queue, detail, approval, rejection, resolution history, and calendar.
- Timeout cancellation, pre-start deactivation, monthly accrual, HR read-only views, HR capability management, and cross-cutting validation.

## MVP

The MVP implements UC-01 through UC-22 and canonical tasks T001 through T153 only. `specs/001-leave-management-mvp/tasks.md` remains the canonical task source. `/tasks` is only a derived execution view.

## Out of MVP

Public API, JWT, OpenAPI, email, outbox, Redis, message queues, microservices, Teams, departments, managers, organizational hierarchy, delegation, escalation, external holiday calendar, per-user time zones, User Pending cancellation, persisted LeaveType lookup tables, and unapproved leave types are outside the MVP.

## Module Dependencies

Foundation and Domain/Persistence must complete before user-story execution. User and Approver P1 flows depend on shared request, balance, audit, identity, authorization, configuration, and test infrastructure. Timeout, deactivation, accrual, HR, and calendar increments depend on the same foundation plus relevant workflow fixtures.

## Risks

- Incorrect balances, unauthorized approvals, lost audit records, PII leakage, concurrency races, stale documentation, HR capability management abuse, and overengineering.
- Required configuration values remain pending and must not receive invented defaults.
- Large canonical tasks may need internal decomposition before execution while preserving original identifiers.

## Pending Configuration

- `ConnectionStrings:DefaultConnection`
- `NovaLeave:PendingRequestTimeoutDays`
- `NovaLeave:SessionTimeoutMinutes`
- `NovaLeave:SeedDemoUsers`
- Accrual scheduler cadence

## Functional Requirements

Functional requirement identifiers are preserved from the approved artifacts and mapped in `coverage-report.md`.

## Non-Functional Requirements

Clean Architecture, MVC/Razor/Bootstrap, Identity cookies, EF Core SQL Server, FluentValidation, Serilog, xUnit/WebApplicationFactory/approved E2E tests, accessibility, observability, startup configuration validation, and TimeProvider-only time dependency.

## Business Rules

Business rules are preserved through the `BR-XXX` identifiers and normative source sections mapped in `coverage-report.md`.

## Use Cases

- UC-01
- UC-02
- UC-03
- UC-04
- UC-05
- UC-06
- UC-07
- UC-08
- UC-09
- UC-10
- UC-11
- UC-12
- UC-13
- UC-14
- UC-15
- UC-16
- UC-17
- UC-18
- UC-19
- UC-20
- UC-21
- UC-22

## Identified Architecture

Clean Architecture with `NovaLeave.Domain`, `NovaLeave.Application`, `NovaLeave.Infrastructure`, and `NovaLeave.Presentation.Web`; Application organized by vertical slice; MVC/Razor Views; Bootstrap 5.3; EF Core SQL Server; Identity; Serilog; and built-in .NET `TimeProvider`.

## Integrations

Approved integrations are limited to framework and platform integrations: ASP.NET Core Identity, SQL Server via EF Core, Bootstrap assets, Serilog logging, and test tooling. External product integrations are out of MVP.

## Recommended Implementation Order

1. EPIC-001 Foundation.
2. EPIC-002 Domain and Persistence.
3. EPIC-003 User Request Management.
4. EPIC-004 Approver Resolution.
5. EPIC-005 Automated Timeout.
6. EPIC-006 Approved Request Deactivation.
7. EPIC-007 Balance and Accrual.
8. EPIC-009 HR Read Only.
9. EPIC-010 Approver Capability Management.
10. EPIC-008 Calendars.
11. EPIC-011 Quality, Security, and Release.

## Incremental Roadmap

- Increment 1: Foundation and Domain/Persistence.
- Increment 2: User P1 workflows.
- Increment 3: Approver P1 workflows.
- Increment 4: System timeout, deactivation, accrual, and HR P2 workflows.
- Increment 5: Calendar P3 refinements and final release validation.

Sprint numbers, if later assigned, are provisional only. Final sprint allocation depends on team capacity and velocity.

## Traceability Matrix

| UC | Related TASK files |
|---|---|
| UC-01 | TASK-059 through TASK-077; foundational TASK-001 through TASK-058 |
| UC-02 | TASK-059 through TASK-077; foundational TASK-001 through TASK-058 |
| UC-03 | TASK-059 through TASK-077; foundational TASK-001 through TASK-058 |
| UC-04 | TASK-059 through TASK-077; foundational TASK-001 through TASK-058 |
| UC-05 | TASK-059 through TASK-077; foundational TASK-001 through TASK-058 |
| UC-06 | TASK-059 through TASK-077; foundational TASK-001 through TASK-058 |
| UC-07 | TASK-059 through TASK-077; foundational TASK-001 through TASK-058 |
| UC-08 | TASK-059 through TASK-077; foundational TASK-001 through TASK-058 |
| UC-09 | TASK-078 through TASK-093; foundational TASK-001 through TASK-058 |
| UC-10 | TASK-078 through TASK-093; foundational TASK-001 through TASK-058 |
| UC-11 | TASK-078 through TASK-093; foundational TASK-001 through TASK-058 |
| UC-12 | TASK-078 through TASK-093; foundational TASK-001 through TASK-058 |
| UC-13 | TASK-101 through TASK-107; foundational TASK-001 through TASK-058 |
| UC-14 | TASK-078 through TASK-093; foundational TASK-001 through TASK-058 |
| UC-15 | TASK-078 through TASK-093; foundational TASK-001 through TASK-058 |
| UC-16 | TASK-094 through TASK-100; foundational TASK-001 through TASK-058 |
| UC-17 | TASK-108 through TASK-116; foundational TASK-001 through TASK-058 |
| UC-18 | TASK-123 through TASK-136; calendar TASK-117 through TASK-122 where applicable; foundational TASK-001 through TASK-058 |
| UC-19 | TASK-123 through TASK-136; calendar TASK-117 through TASK-122 where applicable; foundational TASK-001 through TASK-058 |
| UC-20 | TASK-123 through TASK-136; calendar TASK-117 through TASK-122 where applicable; foundational TASK-001 through TASK-058 |
| UC-21 | TASK-123 through TASK-136; calendar TASK-117 through TASK-122 where applicable; foundational TASK-001 through TASK-058 |
| UC-22 | TASK-137 through TASK-145; foundational TASK-001 through TASK-058 |

## Maintenance Conventions

Requirement changes must be made first in the official artifacts and then synchronized into `/tasks`. Do not edit `/tasks` as the source of truth. Preserve official identifiers exactly. Keep task files one-to-one with T001 through T153.

## Generated Task Files

- TASK-001 - EPIC-001 - Foundation: Create solution file `NovaLeave.sln` and planned project directories `src/NovaLeave.Domain/`, `src/NovaLeave.Application/`, `src/NovaLeave.Infrastructure/`, `src/NovaLeave.Presentation.Web/`, `tests/NovaLeave.UnitTests/`, `tests/NovaLeave.IntegrationTests/`, and `tests/NovaLeave.EndToEndTests/`
- TASK-002 - EPIC-001 - Foundation: Initialize .NET 10 projects and references matching Clean Architecture
- TASK-003 - EPIC-001 - Foundation: Add shared build settings, nullable reference types, analyzers, and formatting configuration
- TASK-004 - EPIC-001 - Foundation: Configure test project packages for xUnit, WebApplicationFactory, EF Core SQL Server integration tests, and approved E2E tooling
- TASK-005 - EPIC-001 - Foundation: Configure MVC and Razor Views package references
- TASK-006 - EPIC-001 - Foundation: Configure ASP.NET Core Identity UI package references
- TASK-007 - EPIC-001 - Foundation: Configure FluentValidation package references
- TASK-008 - EPIC-001 - Foundation: Configure EF Core SQL Server package references
- TASK-009 - EPIC-001 - Foundation: Configure Serilog package references
- TASK-010 - EPIC-001 - Foundation: Configure Bootstrap 5.3 asset management
- TASK-011 - EPIC-001 - Foundation: Add application settings placeholders with no invented defaults
- TASK-012 - EPIC-001 - Foundation: Update `AGENTS.md` Spec Kit section to reference `specs/001-leave-management-mvp/plan.md`
- TASK-013 - EPIC-001 - Foundation: Add Clean Architecture dependency tests
- TASK-014 - EPIC-001 - Foundation: Add request lifecycle invariant tests for five states (`Pending`, `Approved`, `Rejected`, `CancelledByTimeout`, `CancelledByApprover`) and four official transitions (`Pending -> Approved`, `Pending -> Rejected`, `Pending -> CancelledByTimeout`, `Approved -> CancelledByApprover`)
- TASK-015 - EPIC-001 - Foundation: Add working-day calculation tests for Mon-Fri, weekend exclusion, and holiday-as-working-day behavior
- TASK-016 - EPIC-001 - Foundation: Add balance invariant tests for non-negative accrued/reserved/deducted/available totals
- TASK-017 - EPIC-001 - Foundation: Add authorization policy tests for User, Approver, HR, Active/Inactive, self-resolution denial, and `canResolveRequests`
- TASK-018 - EPIC-001 - Foundation: Add audit schema and redaction tests
- TASK-019 - EPIC-001 - Foundation: Add configuration validation tests for `NovaLeave:PendingRequestTimeoutDays`, `NovaLeave:SessionTimeoutMinutes`, and accrual scheduler cadence
- TASK-020 - EPIC-002 - Domain and Persistence: Create request status enum with exactly five states
- TASK-021 - EPIC-002 - Domain and Persistence: Create movement type enum
- TASK-022 - EPIC-002 - Domain and Persistence: Create Vacation-only leave type enum or constant
- TASK-023 - EPIC-002 - Domain and Persistence: Create DateRange value object
- TASK-024 - EPIC-002 - Domain and Persistence: Create WorkingDayCount value object
- TASK-025 - EPIC-002 - Domain and Persistence: Create AccrualPeriod value object
- TASK-026 - EPIC-002 - Domain and Persistence: Create WorkingDaysCalculator domain service using built-in .NET `TimeProvider` inputs where current date is needed
- TASK-027 - EPIC-002 - Domain and Persistence: Create OverlapPolicy domain service
- TASK-028 - EPIC-002 - Domain and Persistence: Create VacationRequest aggregate with states `Pending`, `Approved`, `Rejected`, `CancelledByTimeout`, and `CancelledByApprover`, and only the official transitions `Pending -> Approved`, `Pending -> Rejected`, `Pending -> CancelledByTimeout`, and `Approved -> CancelledByApprover`
- TASK-029 - EPIC-002 - Domain and Persistence: Create VacationBalance aggregate with non-negative balance invariants
- TASK-030 - EPIC-002 - Domain and Persistence: Create immutable BalanceMovement entity
- TASK-031 - EPIC-002 - Domain and Persistence: Create immutable AuditRecord entity with redaction-ready Data field
- TASK-032 - EPIC-002 - Domain and Persistence: Create Application DbContext abstraction
- TASK-033 - EPIC-002 - Domain and Persistence: Create Application authorization facade abstraction
- TASK-034 - EPIC-002 - Domain and Persistence: Create Application audit writer abstraction
- TASK-035 - EPIC-002 - Domain and Persistence: Create Application current-user abstraction
- TASK-036 - EPIC-002 - Domain and Persistence: Create Application result and error types for MVC/Razor outcomes
- TASK-037 - EPIC-002 - Domain and Persistence: Create typed configuration model
- TASK-038 - EPIC-002 - Domain and Persistence: Create Infrastructure Identity user extension
- TASK-039 - EPIC-002 - Domain and Persistence: Create EF Core DbContext shell
- TASK-040 - EPIC-002 - Domain and Persistence: Create VacationRequest EF Core mapping
- TASK-041 - EPIC-002 - Domain and Persistence: Create VacationBalance EF Core mapping
- TASK-042 - EPIC-002 - Domain and Persistence: Create BalanceMovement EF Core mapping
- TASK-043 - EPIC-002 - Domain and Persistence: Create AuditRecord EF Core mapping
- TASK-044 - EPIC-002 - Domain and Persistence: Create ApplicationUser EF Core mapping
- TASK-045 - EPIC-002 - Domain and Persistence: Create initial EF Core migration plan implementation
- TASK-046 - EPIC-002 - Domain and Persistence: Register ASP.NET Core Identity
- TASK-047 - EPIC-002 - Domain and Persistence: Register EF Core SQL Server DbContext
- TASK-048 - EPIC-002 - Domain and Persistence: Register Serilog request logging and structured logging enrichment
- TASK-049 - EPIC-002 - Domain and Persistence: Register built-in .NET `TimeProvider` directly
- TASK-050 - EPIC-002 - Domain and Persistence: Register `NovaLeaveOptions` validation with startup failure for missing/invalid values
- TASK-051 - EPIC-002 - Domain and Persistence: Create MVC filters for antiforgery, deny-by-default authorization, overposting prevention, ModelState validation feedback, safe error pages, and 403/404/409 responses
- TASK-052 - EPIC-002 - Domain and Persistence: Create shared Razor layout shell
- TASK-053 - EPIC-002 - Domain and Persistence: Create shared status badge partial
- TASK-054 - EPIC-002 - Domain and Persistence: Create shared toast partial
- TASK-055 - EPIC-002 - Domain and Persistence: Create shared validation summary partial
- TASK-056 - EPIC-002 - Domain and Persistence: Create shared calendar partial shell
- TASK-057 - EPIC-002 - Domain and Persistence: Create integration test factory
- TASK-058 - EPIC-002 - Domain and Persistence: Create SQL Server test fixture
- TASK-059 - EPIC-003 - User Request Management: Add UC-01 authentication integration tests
- TASK-060 - EPIC-003 - User Request Management: Add UC-02 role context switching integration tests
- TASK-061 - EPIC-003 - User Request Management: Add UC-03 own request list integration tests
- TASK-062 - EPIC-003 - User Request Management: Add UC-04 create request validation, reservation, audit, and overlap tests
- TASK-063 - EPIC-003 - User Request Management: Add UC-05 Pending edit revalidation and reservation-adjustment tests
- TASK-064 - EPIC-003 - User Request Management: Add UC-06 owned detail authorization tests
- TASK-065 - EPIC-003 - User Request Management: Add UC-07 balance and movement history tests
- TASK-066 - EPIC-003 - User Request Management: Add UC-08 personal calendar tests
- TASK-067 - EPIC-003 - User Request Management: Add User MVC accessibility and route smoke tests
- TASK-068 - EPIC-003 - User Request Management: Implement User request query use cases
- TASK-069 - EPIC-003 - User Request Management: Implement User balance query use cases
- TASK-070 - EPIC-003 - User Request Management: Implement personal calendar query use case
- TASK-071 - EPIC-003 - User Request Management: Implement create vacation request command with reservation and audit transaction
- TASK-072 - EPIC-003 - User Request Management: Implement edit Pending request command with full revalidation, reservation adjustment, rowversion, and audit
- TASK-073 - EPIC-003 - User Request Management: Implement FluentValidation validators for User request commands
- TASK-074 - EPIC-003 - User Request Management: Implement User MVC controllers for `/mis-solicitudes`, `/mis-solicitudes/crear`, `/mis-solicitudes/{id}`, `/saldo`, and `/calendario`
- TASK-075 - EPIC-003 - User Request Management: Implement dedicated User ViewModels
- TASK-076 - EPIC-003 - User Request Management: Implement User Razor views with approved Spanish labels
- TASK-077 - EPIC-003 - User Request Management: Wire User navigation and role context switcher
- TASK-078 - EPIC-004 - Approver Resolution: Add UC-09 eligible queue tests without team/department/hierarchy filters
- TASK-079 - EPIC-004 - Approver Resolution: Add UC-10 resolution detail tests with projected balance
- TASK-080 - EPIC-004 - Approver Resolution: Add UC-11 approval transaction and audit tests
- TASK-081 - EPIC-004 - Approver Resolution: Add UC-12 rejection reason, release, and audit tests
- TASK-082 - EPIC-004 - Approver Resolution: Add UC-14 resolution history tests
- TASK-083 - EPIC-004 - Approver Resolution: Add UC-15 Approver calendar tests
- TASK-084 - EPIC-004 - Approver Resolution: Add approval/rejection concurrency race tests
- TASK-085 - EPIC-004 - Approver Resolution: Add Approver MVC E2E smoke tests
- TASK-086 - EPIC-004 - Approver Resolution: Implement Approver queue and detail queries
- TASK-087 - EPIC-004 - Approver Resolution: Implement Approver history and calendar queries
- TASK-088 - EPIC-004 - Approver Resolution: Implement approval command converting reservation to deduction atomically
- TASK-089 - EPIC-004 - Approver Resolution: Implement rejection command releasing reservation atomically
- TASK-090 - EPIC-004 - Approver Resolution: Implement Approver authorization resource checks
- TASK-091 - EPIC-004 - Approver Resolution: Implement Approver MVC controllers for `/aprobaciones`, `/aprobaciones/{id}`, `/aprobaciones/{id}/aprobar`, `/aprobaciones/{id}/rechazar`, `/aprobaciones/historial`, and shared calendar route `/calendario`
- TASK-092 - EPIC-004 - Approver Resolution: Implement dedicated Approver ViewModels
- TASK-093 - EPIC-004 - Approver Resolution: Implement Approver Razor views with mutual approve/reject submission protections
- TASK-094 - EPIC-005 - Automated Timeout: Add UC-16 timeout transition tests
- TASK-095 - EPIC-005 - Automated Timeout: Add timeout idempotency tests
- TASK-096 - EPIC-005 - Automated Timeout: Add timeout versus approval concurrency tests
- TASK-097 - EPIC-005 - Automated Timeout: Implement timeout cancellation command
- TASK-098 - EPIC-005 - Automated Timeout: Implement system actor audit writer support
- TASK-099 - EPIC-005 - Automated Timeout: Implement configured timeout hosted-job adapter without default cadence
- TASK-100 - EPIC-005 - Automated Timeout: Register timeout job only when required configuration is present and valid
- TASK-101 - EPIC-006 - Approved Request Deactivation: Add UC-13 pre-start deactivation tests
- TASK-102 - EPIC-006 - Approved Request Deactivation: Add post-start and partial-deactivation denial tests
- TASK-103 - EPIC-006 - Approved Request Deactivation: Add duplicate deactivation concurrency tests
- TASK-104 - EPIC-006 - Approved Request Deactivation: Implement pre-start deactivation command with restoration transaction
- TASK-105 - EPIC-006 - Approved Request Deactivation: Extend Approver detail command surface for deactivation eligibility
- TASK-106 - EPIC-006 - Approved Request Deactivation: Add Approver POST route `/aprobaciones/{id}/desactivar`
- TASK-107 - EPIC-006 - Approved Request Deactivation: Add deactivation confirmation UI and denial messages
- TASK-108 - EPIC-007 - Balance and Accrual: Add OQ-002 accrual examples A and B tests
- TASK-109 - EPIC-007 - Balance and Accrual: Add UC-17 accrual integration tests
- TASK-110 - EPIC-007 - Balance and Accrual: Add duplicate accrual idempotency tests
- TASK-111 - EPIC-007 - Balance and Accrual: Add catch-up accrual tests
- TASK-112 - EPIC-007 - Balance and Accrual: Implement monthly accrual policy
- TASK-113 - EPIC-007 - Balance and Accrual: Implement accrual command with unique `(UserId, AccrualPeriod)` idempotency
- TASK-114 - EPIC-007 - Balance and Accrual: Implement accrual scheduler adapter with DR-001-configured cadence
- TASK-115 - EPIC-007 - Balance and Accrual: Add EF Core unique index for accrual idempotency
- TASK-116 - EPIC-007 - Balance and Accrual: Ensure balance history displays accrual, reservation, release, deduction, and restoration movements
- TASK-117 - EPIC-008 - Calendars: Add shared calendar ViewModel unit tests
- TASK-118 - EPIC-008 - Calendars: Add calendar authorization integration tests
- TASK-119 - EPIC-008 - Calendars: Add calendar accessibility E2E smoke tests
- TASK-120 - EPIC-008 - Calendars: Implement shared calendar query models
- TASK-121 - EPIC-008 - Calendars: Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links
- TASK-122 - EPIC-008 - Calendars: Integrate shared calendar partial into User, Approver, and HR calendar Razor views under `src/NovaLeave.Presentation.Web/Views/`
- TASK-123 - EPIC-009 - HR Read Only: Add UC-18 HR request list/detail tests
- TASK-124 - EPIC-009 - HR Read Only: Add UC-19 HR calendar tests
- TASK-125 - EPIC-009 - HR Read Only: Add UC-20 HR balance and movement read-only tests
- TASK-126 - EPIC-009 - HR Read Only: Add UC-21 HR audit access and sensitive-reason read audit tests
- TASK-127 - EPIC-009 - HR Read Only: Add HR forbidden mutation tests for approve/reject/deactivate/balance/role operations
- TASK-128 - EPIC-009 - HR Read Only: Add HR E2E read-only smoke tests
- TASK-129 - EPIC-009 - HR Read Only: Implement HR request read queries
- TASK-130 - EPIC-009 - HR Read Only: Implement HR calendar query
- TASK-131 - EPIC-009 - HR Read Only: Implement HR balance and movement read queries
- TASK-132 - EPIC-009 - HR Read Only: Implement HR audit read queries with sensitive-reason access auditing
- TASK-133 - EPIC-009 - HR Read Only: Implement HR authorization policies
- TASK-134 - EPIC-009 - HR Read Only: Implement HR MVC controllers for `/rrhh/solicitudes`, `/rrhh/calendario`, `/rrhh/saldos`, and `/rrhh/auditoria`
- TASK-135 - EPIC-009 - HR Read Only: Implement dedicated HR read-only ViewModels
- TASK-136 - EPIC-009 - HR Read Only: Implement HR read-only Razor views with no resolution or balance modification actions
- TASK-137 - EPIC-010 - Approver Capability Management: Add UC-22 Approver capability list tests
- TASK-138 - EPIC-010 - Approver Capability Management: Add UC-22 capability toggle validation, rowversion, and audit tests
- TASK-139 - EPIC-010 - Approver Capability Management: Add inactive HR and non-Approver target denial tests
- TASK-140 - EPIC-010 - Approver Capability Management: Add HR capability E2E smoke tests
- TASK-141 - EPIC-010 - Approver Capability Management: Implement Approver capability list query
- TASK-142 - EPIC-010 - Approver Capability Management: Implement `canResolveRequests` toggle command with reason, confirmation, rowversion, role revalidation, and audit
- TASK-143 - EPIC-010 - Approver Capability Management: Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad`
- TASK-144 - EPIC-010 - Approver Capability Management: Implement capability management ViewModels
- TASK-145 - EPIC-010 - Approver Capability Management: Implement capability list, modal confirmation, validation, concurrency, and toast UI
- TASK-146 - EPIC-011 - Quality, Security, and Release: Add full traceability smoke test asserting UC-01 through UC-22 routes and authorization policies
- TASK-147 - EPIC-011 - Quality, Security, and Release: Add lifecycle transition matrix regression tests
- TASK-148 - EPIC-011 - Quality, Security, and Release: Add audit completeness regression tests for all successful state changes and Pending edits
- TASK-149 - EPIC-011 - Quality, Security, and Release: Add security regression tests for CSRF, IDOR, forced browsing, overposting, and session timeout
- TASK-150 - EPIC-011 - Quality, Security, and Release: Add accessibility regression tests for User, Approver, and HR critical pages
- TASK-151 - EPIC-011 - Quality, Security, and Release: Validate `specs/001-leave-management-mvp/quickstart.md` against the implemented application and update only if commands or paths changed
- TASK-152 - EPIC-011 - Quality, Security, and Release: Run complete test suite from `NovaLeave.sln` and record evidence in implementation handoff notes
- TASK-153 - EPIC-011 - Quality, Security, and Release: Review documentation references and ensure no generated artifact claims implementation evidence without inspected source
