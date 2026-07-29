# Phase 1 — Data Model: NovaLeave MVP — Vacation Requests & Balances

**Date**: 2026-07-28 (revised 2026-07-28)
**Feature**: 001-leave-management-mvp
**Approved decisions**: see `research.md`

---

## Domain Traceability Matrix

| Concept | Type | Responsibility | Protected Rules | Traceability |
|---------|------|---------------|-----------------|--------------|
| `VacationRequest` | Aggregate root | Full lifecycle: state, dates, working-day total | States (5), overlap, next-day minimum, Mon–Fri calculation, ownership | FR-001, FR-005–007, FR-011–012, BR-001–035, CON-001–012, AUTHZ-001–007, AUD-001–002 |
| `VacationBalance` | Aggregate root | One global balance per User; non-negative invariant | AccruedDays >= 0; DeductedDays >= 0; ReservedDays >= 0; Available >= 0; no HR adjustment | BR-017–018, BR-030–035, CON-003, CON-007–011, FR-004, FR-014, AUD-007 |
| `BalanceMovement` | Immutable entity (child of VacationBalance) | Append-only history of every balance effect | Immutable after creation; linked by RequestId | BR-030–035, CON-007–011, AUD-007; Instruction §5.11 |
| `AuditRecord` | Immutable entity | Append-only business audit with all required fields | Immutable; complete fields; sensitive reasons never in Data column | AUD-001–010, CON-006–011, SEC-005–006 |
| `ApplicationUser` | Infrastructure entity (extends IdentityUser) | Active/Inactive status; roles; canResolveRequests; EmploymentStartDate | Active status checked pre-transition; Domain User does NOT inherit IdentityUser | FR-001, FR-006, AUTHZ-001–016, Constitution §4.4, §7.1 |
| `LeaveType` | Domain enum/constant | Request type validation | Vacation only in MVP; no persisted lookup or mutable aggregate | VAL-002, VAL-003, spec.md PD-001, research.md CR-09 |
| `TimeProvider` | Built-in .NET service | Single system business date; testable clock | No DateTime.Now in Domain/Application; no per-user timezone; no duplicate clock abstraction | Constitution §VI; BR-002–003; research.md CR-08 |

### VacationRequest Lifecycle

| From | To | Actor | Balance Effect | Audit |
|------|----|-------|---------------|-------|
| (new) | Pending | Active User | +Reservation | AuditRecord(Create) + BalanceMovement(Reservation) |
| Pending | Pending (edit) | Active User (Owner) | Adjust reservation | AuditRecord(Edit) + BalanceMovement adjustment |
| Pending | Approved | Active Approver (not owner) | Reservation → Deduction | AuditRecord(Approve) + BalanceMovement(Deduction) |
| Pending | Rejected | Active Approver (not owner) | Release reservation | AuditRecord(Reject) + BalanceMovement(Release) |
| Pending | CancelledByTimeout | System | Release reservation | AuditRecord(Timeout, actor=System) + BalanceMovement(Release) |
| Approved | CancelledByApprover | Active Approver (not owner, pre-start only) | Restore deduction | AuditRecord(Deactivate) + BalanceMovement(Restoration) |

**Terminal states**: Rejected, CancelledByTimeout, CancelledByApprover; Approved is final except valid pre-start deactivation.

### VacationBalance Lifecycle

| Event | Effect | Idempotency |
|-------|--------|------------|
| Accrual | AccruedDays += 1 | Unique constraint on (UserId, AccrualPeriod) |
| Reserve | ReservedDays += N | On request creation |
| Release | ReservedDays -= N | On rejection or timeout |
| Deduct | ReservedDays -= N; DeductedDays += N | On approval |
| Restore | DeductedDays -= N | On pre-start deactivation |

---

## Module Responsibilities

| Module | Responsibility | Owned Concepts / Use Cases | Dependencies |
|--------|---------------|---------------------------|--------------|
| Identity & Access | Authentication, sessions, role claims, active-status, security-stamp refresh, demo seeding | UC-01, UC-02; ApplicationUser; authorization policies | ASP.NET Core Identity; Infrastructure |
| Vacation Requests | Full lifecycle: create, edit, approve, reject, timeout, deactivate | UC-04–06, UC-09–13, UC-16; VacationRequest | Balance & Accrual module; Audit |
| Balances & Accrual | Global balance, movements, monthly accrual | UC-07, UC-17, UC-20; VacationBalance, BalanceMovement | None (owns aggregates) |
| HR Queries & Capability | Organization-wide read views, canResolveRequests management | UC-18–22; AuditRecord queries | Vacation Requests (read), Identity & Access |
| Audit & Observability | Append-only AuditRecords; security events via Serilog; correlation middleware | All UCs (AUD-001–010) | All modules (write side) |
| Frontend Presentation | MVC controllers, Razor views, ViewModels, design tokens, accessibility | All UCs | Application use cases only (no direct DbContext) |

> One modular monolith. No microservices. Modules enforce non-overlapping responsibilities via project dependencies.

---

## Persistence Entities

### VacationRequest

| Field | Type | Notes |
|-------|------|-------|
| Id | `Guid` (PK) | |
| OwnerId | `string` | FK → ApplicationUser.Id (stable Identity linkage) |
| StartDate | `DateOnly` | Authoritative, server-derived, inclusive |
| EndDate | `DateOnly` | Authoritative, server-derived, inclusive |
| WorkingDays | `int` | Server-calculated Mon–Fri count; holidays counted |
| Reason | `string` | Normalized 10–500 chars; **sensitive — never in logs or audit Data** |
| Status | `RequestStatus` enum | Pending / Approved / Rejected / CancelledByTimeout / CancelledByApprover |
| RejectionReason | `string?` | Normalized 10–500 chars; **sensitive — never in logs or audit Data** |
| RowVersion | `byte[]` | `rowversion` — optimistic concurrency on mutable state |
| CreatedAtUtc | `DateTime` | UTC |
| UpdatedAtUtc | `DateTime` | UTC |

**Removed from Conjunto 1**: `ReservationMovementId`, `DeductionMovementId` (movements linked by `RequestId`, no back-reference needed — research.md CR-13); `StartBusinessDateUtc` (derived at runtime via `TimeProvider`, not cached — research.md CR-14); `LeaveTypeId` FK (leave type is a constant; no FK lookup needed — research.md CR-09).

Indexes: `IX_VacationRequest_OwnerId_Status`, `IX_VacationRequest_StartDate_EndDate`

### VacationBalance

| Field | Type | Notes |
|-------|------|-------|
| Id | `Guid` (PK) | |
| UserId | `string` | FK → ApplicationUser.Id; unique per user |
| AccruedDays | `int` | >= 0; total accruals |
| DeductedDays | `int` | >= 0; permanent deductions |
| ReservedDays | `int` | >= 0; sum of active Pending reservations |
| RowVersion | `byte[]` | `rowversion` |
| ModifiedAtUtc | `DateTime` | UTC |

`Available = AccruedDays − DeductedDays − ReservedDays` (>= 0, invariant enforced in Domain).  
HR has read-only access. No HR adjustment or generic balance editing.

### BalanceMovement (Immutable)

| Field | Type | Notes |
|-------|------|-------|
| Id | `Guid` (PK) | |
| BalanceId | `Guid` | FK → VacationBalance |
| RequestId | `Guid?` | FK → VacationRequest (null for pure accrual) |
| Type | `MovementType` enum | Accrual / Reservation / Release / Deduction / Restoration |
| Amount | `int` | Positive whole days |
| ActorId | `string` | User / Approver / System identity |
| EffectiveAtUtc | `DateTime` | UTC |
| CreatedAtUtc | `DateTime` | UTC |

**No `Reason` field** — sensitive free-text reasons must not appear in BalanceMovement (Instruction §5.12; SEC-005, SEC-006).  
**No `RowVersion`** — immutable; never modified after creation.

Indexes: `IX_BalanceMovement_BalanceId_EffectiveAt`, `IX_BalanceMovement_RequestId`

### AuditRecord (Immutable)

| Field | Type | Notes |
|-------|------|-------|
| Id | `Guid` (PK) | |
| TimestampUtc | `DateTime` | Required; UTC |
| ActorId | `string` | Required |
| ActorRole | `string` | Required |
| Action | `string` | Required |
| EntityType | `string` | Required |
| EntityId | `Guid` | Required |
| Result | `string` | Required |
| CorrelationId | `Guid` | Required |
| RequestId | `Guid` | Required |
| Data | `string?` | JSON snapshot; **sensitive fields (Reason, RejectionReason) always redacted** |

HR access to Reason or RejectionReason creates a dedicated audit event (SEC-009, AUD-009) containing: HR actor, request identifier, accessed field name, result, CorrelationId, RequestId, timestamp — **never the reason content**.

**No `RowVersion`** — immutable; never modified after creation.

### ApplicationUser (Infrastructure — extends IdentityUser)

| Field | Type | Notes |
|-------|------|-------|
| Id | `string` | ASP.NET Identity PK; stable linkage key |
| IsActive | `bool` | Business active/inactive status |
| CanResolveRequests | `bool` | Approver capability; HR-managed via UC-22 |
| EmploymentStartDate | `DateOnly` | For monthly accrual calculation |
| RowVersion | `byte[]` | `rowversion` — for canResolveRequests optimistic concurrency (UC-22) |

> Domain business `User` concept does **NOT** inherit from `IdentityUser`. `ApplicationUser` (Infrastructure) extends `IdentityUser` and is the stable linkage point.

### LeaveType (Domain enum/constant — not persisted)

No lookup table, seed row, DbSet, or migration task is planned. Domain validates via `LeaveType.Vacation` constant.

---

## Value Objects

| Name | Fields | Domain Rule |
|------|--------|------------|
| `DateRange` | Start: DateOnly, End: DateOnly | Start <= End; both valid dates |
| `WorkingDays` | Count: int | Count > 0; server-calculated only; never trusted from client |
| `NormalizedText` | Value: string | 10–500 chars after trim; no whitespace-only; markup is plain text |

---

## Concurrency and Idempotency Matrix

| Race / Scenario | Protection | Winner | Loser Outcome | Transaction Boundary | Test Layer |
|----------------|-----------|--------|----------------|---------------------|-----------|
| Duplicate request creation (retry/replay) | Unique index on (OwnerId, StartDate, EndDate) where Status != terminal | First committed | DbUpdateException → 409 | Single tx per request | Integration + Concurrency |
| Concurrent overlapping submissions (same user) | Overlap check within same tx as reservation (serializable segment or lock) | First committed | Overlap rejection → 409 | Atomic with CON-008 | Integration + Concurrency |
| Concurrent approval + rejection | RowVersion on VacationRequest | First committed | DbUpdateConcurrencyException → refresh prompt | Single tx per operation | Integration + Concurrency |
| Approval vs. timeout at boundary | RowVersion; timeout checks Status=Pending inside tx | Human approval | Timeout yields; no-op | Single tx per operation | Concurrency |
| Duplicate timeout execution | Idempotent: Status=Pending check before transition + RowVersion | First run | Silent no-op | Per-request tx | Idempotency |
| Duplicate monthly accrual | Unique constraint on (UserId, AccrualPeriod) in BalanceMovement | First committed | DbException → skip | Per-user accrual tx | Idempotency |
| Duplicate pre-start deactivation | RowVersion + Status=Approved check | First committed | 409 Conflict | Single tx | Integration |
| Stale Pending edit | RowVersion on VacationRequest | Current version | DbUpdateConcurrencyException → 409 | Single tx | Integration |
| Stale HR capability update | RowVersion on ApplicationUser | Current version | DbUpdateConcurrencyException → 409 | Single tx | Integration |
| Audit or persistence failure | All effects in one DB transaction | All-or-nothing | Rollback entire operation | Single tx | Failure injection |

---

## EF Core Notes

- `rowversion` mapped via `.IsRowVersion()` on `byte[]` property.
- Explicit transactions (`IDbContextTransaction`) for multi-entity atomic operations.
- `AsNoTracking()` + projection for all read queries.
- Server-side pagination (`Skip`/`Take` with `Where` filter) for all queries that may return > 50 rows.
- No EF Core InMemory provider for integration tests claiming relational behavior (Constitution §9.1).
- N+1 patterns prohibited; use `Include` and `Select` deliberately.
- Table names: singular (per Constitution §3.3).

---

## Typed Configuration (NovaLeaveOptions)

| Property | Type | Validation | Environment |
|----------|------|-----------|-------------|
| `PendingRequestTimeoutDays` | `int` | Required; > 0 | All environments; official value 14 (DR-001) |
| `SeedDemoUsers` | `bool` | — | Development/Staging only; `false` in Production |
| `SessionTimeoutMinutes` | `int` | Required; > 0 | All environments; official value 30 (DR-001) |

Validated at startup via `services.AddOptions<NovaLeaveOptions>().ValidateDataAnnotations().ValidateOnStart()`.

No `SystemParameter` database entity. No seeded default timeout value. (research.md CR-10, CR-11)

---

## Diagrams

- Clean Architecture dependency diagram: `specs/001-leave-management-mvp/Diagrams/clean-architecture.md`
- Core data relationships diagram: `specs/001-leave-management-mvp/Diagrams/core-data-relationships.md`
- Request lifecycle state machine: `specs/001-leave-management-mvp/Diagrams/request-lifecycle.md`
