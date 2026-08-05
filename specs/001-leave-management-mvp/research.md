# Phase 0 — Research: NovaLeave MVP — Vacation Request Management

**Date**: 2026-07-28 (revised 2026-07-28)
**Feature**: 001-leave-management-mvp
**Purpose**: Resolve open questions, record design decisions, and document conflict resolutions before Phase 1 design.

## Conflict Resolutions (Authority Order Applied)

### CR-01: Timeout Default Value
**Source conflict**: Conjunto 1 research.md recommended a 7-day default. Instruction §5.6 prohibits inventing defaults.
**Authority applied**: Instruction §5.6 over Conjunto 1 assumption.
**Resolution**: `NovaLeave:PendingRequestTimeoutDays` is a required typed configuration value (required configuration; official value 14 per DR-001). No default is seeded or invented in code. The application validates presence, positive value, and non-zero at startup (`ValidateOnStart`). Timeout processing is idempotent regardless of configured value.
**Affected**: CFG-001, AC-048, UC-16.

### CR-02: Accrual Timezone Reference
**Source conflict**: Conjunto 1 mentioned "accrual keyed to calendar month ending in user's employment timezone." Constitution §5 and Instruction §5.4 prohibit per-user timezone behavior.
**Authority applied**: Constitution v7.0.0 invariant 4; Instruction §5.4.
**Resolution**: No per-user timezone logic. The Product Owner clarification of 2026-08-05 fixes the single system business date to `America/Costa_Rica` (UTC−06:00). Accrual periods, next-day validation, timeout dates, calendar “today”, and pre-start deactivation use that business date. Timestamps remain persisted in UTC and are converted only for user-facing presentation. `TimeProvider` (built-in .NET 10) remains the sole clock abstraction.
**Affected**: FR-014, BR-032, BR-033, AC-054, UC-17.

### CR-03: Accrual Job Cadence
**Source conflict**: Conjunto 1 asked for schedule/time-of-day confirmation.
**Resolution**: Scheduler cadence and time-of-day are operational configuration (required configuration; daily 00:05 UTC per DR-001). The job is idempotent per `(UserId, AccrualPeriod)` unique constraint. Catch-up behavior is defined: multiple runs per month apply at most one accrual per user per period. No default cadence is invented.
**Affected**: FR-014, UC-17.

### CR-04: Holiday Rule
**Source conflict**: Conjunto 1 listed holidays as an open question.
**Authority applied**: spec.md BR-004, AC-035, EC-005; Instruction §5.5.
**Resolution**: Count Monday through Friday inclusively. Exclude Saturday and Sunday. Count public holidays as ordinary working days. No holiday calendar. Reject a range containing zero working days. Open question is fully resolved.
**Affected**: BR-004, AC-035, AC-043, EC-005, EC-006.

### CR-05: Demo Identities
**Source conflict**: Conjunto 1 treated demo seeding as an open question.
**Authority applied**: spec.md CFG-003; frontend-design-spec.md §13.1; Instruction §5.8.
**Resolution**: Demo identities are confirmed and must be treated as such, not as open questions:
- `user@demo` — User role, Active — password `Demo123!`
- `approver@demo` — Approver role, Active, `canResolveRequests=true` — password `Demo123!`
- `hr@demo` — HR role, Active — password `Demo123!`
- `multi@demo` — User + Approver + HR, Active — password `Demo123!`
Seeded via `NovaLeave:SeedDemoUsers=true`. **Must be disabled in Production.**
**Affected**: CFG-003, AC-060, RBFV-033, UC-01.

### CR-06: Audit Retention
**Source conflict**: Conjunto 1 recommended 2-year retention.
**Authority applied**: Constitution §13 over Conjunto 1 assumption.
**Resolution**: Default retention for business and audit records is **seven years** unless another approved law or policy applies. No archival or deletion jobs are invented beyond this period.
**Affected**: AUD-005, Constitution §13.

### CR-07: SecurityEvent Storage
**Source conflict**: Conjunto 1 planned a `SecurityEvent` Domain entity and database table.
**Authority applied**: Instruction §5.1.
**Resolution**: Security events required by AUD-004 are satisfied by Serilog structured logging. No `SecurityEvent` Domain entity or database table is created. Immutable `AuditRecord` records satisfy AUD-001 through AUD-010 for business events. If persistent security-event storage is required in the future, an approved requirement and ADR must precede implementation.
**Affected**: AUD-004, SEC-001, SEC-002.

### CR-08: Duplicate Time Abstractions
**Source conflict**: Conjunto 1 planned both `ITimeProvider` and `ISystemDateProvider`.
**Authority applied**: Instruction §5.4; Constitution §VI.
**Resolution**: One abstraction only: `TimeProvider` (built into .NET 8+, available natively in .NET 10). All time-dependent rules use `TimeProvider`. No `ISystemDateProvider`, no `IClock`, no duplicate.
**Affected**: All time-dependent domain rules; BR-002, BR-003, BR-026, BR-027, UC-16, UC-17.

### CR-09: LeaveType as Persisted Aggregate
**Source conflict**: Conjunto 1 planned `LeaveType` as a full Domain entity and mutable aggregate.
**Authority applied**: Instruction §5.10; spec.md VAL-002, VAL-003.
**Resolution**: Only `Vacation` exists in the MVP. LeaveType is represented as a read-only enum/constant in Domain (`LeaveType.Vacation`). There is no mutable LeaveType aggregate, lookup table, seed row, DbSet, or migration task because no approved source requires persisted leave-type reference data.
**Affected**: VAL-002, VAL-003, spec.md PD-001.

### CR-10: SystemParameter as Domain Entity
**Source conflict**: Conjunto 1 planned `SystemParameter` as a Domain entity and database table with seeded value 7 for `PendingTimeoutDays`.
**Authority applied**: Instruction §5.6, §5.10; CFG-001.
**Resolution**: Timeout duration is typed configuration (`IOptions<NovaLeaveOptions>`), not a database entity. No `SystemParameter` table. Configuration is validated at startup. The seeded default of 7 days is removed.
**Affected**: CFG-001, AC-048, UC-16.

### CR-11: Domain Events and IDomainEventDispatcher
**Source conflict**: Conjunto 1 planned Domain event classes and `IDomainEventDispatcher`.
**Authority applied**: Instruction §5.10; Constitution Principle II (simplicity before abstraction).
**Resolution**: Domain events and event dispatching are not introduced. Application use cases coordinate all effects (transition + movement + audit + balance) within a single DB transaction. No event sourcing, no outbox, no message queue in MVP.
**Affected**: Plan.md project structure.

### CR-12: MediatR and Pipeline Behaviors
**Source conflict**: Conjunto 1 planned `ValidationBehavior`, `AuthorizationBehavior`, `ConcurrencyBehavior` in Application.
**Authority applied**: Instruction §5.3; Constitution Principle II.
**Resolution**: MediatR is not introduced. Application use cases are explicit service classes registered via DI. FluentValidation validators invoked explicitly via `IValidator<T>` in handlers. No pipeline behaviors.
**Affected**: Plan.md project structure.

### CR-12A: Manual Quality Gate and No CI/CD
**Source conflict**: Earlier constitutional wording required automated CI/CD gates, but the authoritative project decision excludes CI/CD from the NovaLeave MVP delivery model.
**Authority applied**: Constitution v7.0.0 §9.3; `docs/adr/DR-003-manual-quality-gate-no-ci-cd.md`.
**Resolution**: NovaLeave MVP uses a mandatory Manual Quality and Security Gate. No GitHub Actions, Azure DevOps, Jenkins, continuous delivery, continuous deployment, automated release, or automated deployment pipeline configuration is planned for MVP implementation. Manual gate evidence is blocking before merge, handoff, release, or acceptance. Unavailable tooling is recorded as `NOT EXECUTED`, never as `PASS`; manual or PowerShell equivalents are not official Spec Kit wrapper success.
**Affected**: Constitution §9.3, plan.md, quickstart.md, tasks.md, docs/operations/.

### CR-13: ReservationMovementId / DeductionMovementId on VacationRequest
**Source conflict**: Conjunto 1 planned these FK fields on `VacationRequest`.
**Authority applied**: Instruction §5.10.
**Resolution**: These fields are removed. `BalanceMovement` records are linked to requests via `RequestId`. A movement linked through `RequestId` is sufficient; no back-reference needed on `VacationRequest`.
**Affected**: data-model.md.

### CR-14: StartBusinessDateUtc Cached Field
**Source conflict**: Conjunto 1 planned `StartBusinessDateUtc` as a cached field on `VacationRequest`.
**Authority applied**: Instruction §5.10.
**Resolution**: The pre-start deactivation boundary is evaluated at runtime using `TimeProvider.GetUtcNow()` against `VacationRequest.StartDate`. No cached field needed.
**Affected**: data-model.md, UC-13.

### CR-15: Custom AccountController
**Source conflict**: Conjunto 1 planned an `AccountController.cs` for Identity routes.
**Authority applied**: Instruction §5.9.
**Resolution**: ASP.NET Core Identity default UI routes are used. No custom `AccountController`. Login routes remain at `/Identity/Account/Login`, `/Identity/Account/Logout`, `/Identity/Account/AccessDenied`. No override unless the repository already contains a reviewed Identity UI override that preserves these routes (repository evidence: none).
**Affected**: UC-01 entry point; plan.md project structure.

### CR-16: AutoMapper
**Source conflict**: Conjunto 1 planned `Mapping/*.cs` folders across Application slices, implying AutoMapper.
**Authority applied**: Instruction §5.3; Constitution Principle II.
**Resolution**: AutoMapper is not introduced unless justified by implementation need. Manual mapping within vertical slices is preferred.
**Affected**: Plan.md project structure.

## Resolved Decisions (No Conflict)

| Decision | Value | Authority |
|---|---|---|
| Concurrency token format | `rowversion` (SQL Server binary(8)) → `byte[]` with `.IsRowVersion()` | Constitution §6 |
| Overlap scope | Same User's Pending/Approved only; different Users may overlap | spec.md BR-019, PD-008 |
| Balance negative prevention | Server-side rejection; UI may show warning but server is authoritative | spec.md BR-031; AUTHZ-007 |
| Identity linkage | `ApplicationUser.Id` (string) stored as `UserId` on Domain User concept | Instruction §5.9 |
| RowVersion scope | Applied to `VacationRequest`, `VacationBalance`, `ApplicationUser` (for canResolveRequests); NOT on immutable `BalanceMovement` or `AuditRecord` | Instruction §5.13 |
| Accrual semantics (OQ-002, resolved 2026-07-27) | One whole vacation day per fully completed calendar month from `EmploymentStartDate`; first partial calendar month does not accrue; no proration; accrued days do not expire; idempotent by `(UserId, AccrualPeriod)`; catch-up processes each eligible period exactly once | spec.md OQ-002 |

## Configuration (values decided — DR-001, 2026-07-29)

No value is hard-coded in application code; startup validation remains fail-fast. Official values per [`docs/adr/DR-001-runtime-configuration-values.md`](../../docs/adr/DR-001-runtime-configuration-values.md):

| Key | Description | Official value | Note |
|-----|-------------|----------------|------|
| `NovaLeave:PendingRequestTimeoutDays` | Days before an unresolved Pending request is cancelled (int, > 0) | **14** | CFG-001; decided, not invented |
| `NovaLeave:SeedDemoUsers` | `true` in Development/Staging; absent or `false` in Production | env-specific | CFG-003 |
| `NovaLeave:SessionTimeoutMinutes` | Authenticated session lifetime | **30** | CFG-002 |
| Accrual & timeout job cadence | Scheduler frequency and execution time (idempotent scans) | **Daily 00:05 UTC** | DR-001 |

## Design Constraints

- `TimeProvider` for all time-dependent rules; no `DateTime.Now` or `DateTime.UtcNow` in Domain or Application.
- One centralized `America/Costa_Rica` conversion policy derives business dates and presentation times; no per-user time-zone or duplicate clock abstraction.
- EF Core optimistic concurrency with `rowversion` on mutable aggregates.
- `BalanceMovement` and `AuditRecord` are immutable; created within the same DB transaction as the state change.
- Background services (timeout + accrual) are idempotent and bounded per batch.
- All read queries use `AsNoTracking()` + projection.
- Server-side pagination required for any list that may exceed 50 records (Constitution §6).
- Antiforgery on all POST actions; PRG pattern after successful mutations.

## Risks

- Approval vs. timeout concurrency at boundary window — mitigated by RowVersion and one-winner DB semantics (CON-002, CON-009).
- Stale authorization after `canResolveRequests` toggle — mitigated by security-stamp refresh and per-request re-evaluation (AUTHZ-012).
- Missing required configuration at startup — mitigated by `IOptions` validation with `ValidateOnStart = true`.

