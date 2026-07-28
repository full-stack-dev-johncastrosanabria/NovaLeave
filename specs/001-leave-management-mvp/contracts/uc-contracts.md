# UC Contracts — NovaLeave MVP

**Feature**: 001-leave-management-mvp
**Date**: 2026-07-28
**Covers**: UC-01 through UC-22

Rules:
- Every route traces to an approved use case.
- GET never changes state.
- Mutations use POST + antiforgery.
- Input models are dedicated; server-derived values are never trusted from client.
- Views never receive Domain or persistence entities.
- Controllers never access DbContext or concrete repositories.

---

## UC-01 — Authenticate

| Field | Value |
|---|---|
| Entry point | GET `/Identity/Account/Login` (form) |
| Mutation | POST `/Identity/Account/Login` |
| Input fields | email, password, rememberMe, antiforgery token |
| Output fields | error summary, demo accounts list (when demo users enabled) |
| Application contract | Authenticate user by credentials → authentication result |
| Authorization | Anonymous (Identity manages internally) |
| Transaction / Audit | Identity login event; audit record for authentication failure |
| Concurrency | Identity lockout policy; rate limiting on login endpoint |
| Notes | `/Identity/Account/AccessDenied` for 403; `/Identity/Account/Logout` POST for logout |

---

## UC-02 — Switch Role Context

| Field | Value |
|---|---|
| Entry point | Header "Mis roles" dropdown (client-side navigation) |
| Mutation | None — navigation only; no POST |
| Application contract | None — authorization re-evaluated on each subsequent request |
| Authorization | Authenticated; active; ≥ 2 roles |
| Transaction / Audit | None; context switching is a UI convenience, not a security control |
| Notes | Switching navigates to root of selected context (`/mis-solicitudes`, `/aprobaciones`, `/rrhh`). Identity, claims, and permissions unchanged. |

---

## UC-03 — View Own Vacation Requests

| Field | Value |
|---|---|
| Entry point | GET `/mis-solicitudes` |
| Application contract | Query own requests with pagination → paged request summaries |
| Output fields | request items, pagination controls, balance summary |
| Authorization | Active User; server-side owner filtering |
| Transaction / Audit | Read-only; no state change |
| Notes | Pagination required; server-side owner filter; no other User's data disclosed |

---

## UC-04 — Create a Vacation Request

| Field | Value |
|---|---|
| Entry point | GET `/mis-solicitudes/crear` (form) |
| Mutation | POST `/mis-solicitudes/crear` |
| Input fields | inputMode (dateRange | startPlusDays), startDate, endDate (optional), workingDays (optional), reason, antiforgery token |
| Application contract | Create vacation request → creation command with userId, input mode, dates, day count, reason → creation result |
| Authorization | Active User; owner = authenticated identity (server-set) |
| Transaction / Audit | Single DB transaction: VacationRequest (Pending) + BalanceMovement (Reservation) + AuditRecord (Create) |
| Concurrency | Optimistic concurrency on balance; unique constraint on (OwnerId, StartDate, EndDate) |
| Notes | Server calculates WorkingDays, EndDate, DateRange; client-supplied values ignored. PRG after success. |

---

## UC-05 — Edit an Owned Pending Request

| Field | Value |
|---|---|
| Entry point | GET `/mis-solicitudes/{id}/editar` (form) |
| Mutation | POST `/mis-solicitudes/{id}/editar` |
| Input fields | id, inputMode, startDate, endDate (optional), workingDays (optional), reason, rowVersion, antiforgery token |
| Application contract | Edit command with userId, requestId, input mode, dates, day count, reason, concurrency token → edit result |
| Authorization | Active User + request owner + request must be Pending |
| Transaction / Audit | Single DB transaction: VacationRequest update + reservation adjustment + BalanceMovement + AuditRecord (Edit) |
| Concurrency | Optimistic concurrency on request and balance |
| Notes | Full revalidation on server. Stale concurrency token → 409 + refresh prompt. |

---

## UC-06 — View an Owned Request Detail

| Field | Value |
|---|---|
| Entry point | GET `/mis-solicitudes/{id}` |
| Application contract | Query request detail by userId and requestId → request detail |
| Output fields | status, dates, days, reason (visible to owner), balance effect, audit trail |
| Authorization | Active User + request owner |
| Transaction / Audit | Read-only; no state change |
| Notes | Reason visible to owner only; redacted in all non-owner contexts |

---

## UC-07 — View Own Balance and History

| Field | Value |
|---|---|
| Entry point | GET `/saldo` |
| Application contract | Query balance by userId → balance detail (accrued, deducted, reserved, available, movements) |
| Output fields | Acumulado total, Pendientes, Días gozados, Disponible; movement timeline |
| Authorization | Active User; server-side owner filter |
| Transaction / Audit | Read-only |
| Notes | Available = AccruedDays − DeductedDays − ReservedDays (server-computed, never below 0) |

---

## UC-08 — View the Personal Vacation Calendar

| Field | Value |
|---|---|
| Entry point | GET `/calendario` |
| Application contract | Query calendar by userId, role, year, month → calendar data |
| Output fields | month grid, own Approved events only for User context |
| Authorization | Active User |
| Transaction / Audit | Read-only |
| Notes | Shows only authenticated User's own Approved periods. Event activation → `/mis-solicitudes/{id}` |

---

## UC-09 — View the Eligible Pending Request Queue

| Field | Value |
|---|---|
| Entry point | GET `/aprobaciones` |
| Application contract | Query approver queue by approverId, page, pageSize → paged pending request summaries |
| Output fields | requester, dates, working days, available balance, projected balance after approval |
| Authorization | Active Approver; excludes requests owned by Approver |
| Transaction / Audit | Read-only |
| Notes | Organization-wide; no team/department/hierarchy scope. Projected balance is informational only. |

---

## UC-10 — View Request Detail for Resolution

| Field | Value |
|---|---|
| Entry point | GET `/aprobaciones/{id}` |
| Application contract | Query approver request detail by approverId and requestId → approver request detail |
| Output fields | full detail, projected balance, overlap warning, action buttons |
| Authorization | Active Approver + approver eligible (not owner, Pending/eligible Approved) |
| Transaction / Audit | Read-only |
| Notes | All displayed values are server-derived. UI-level balance warning does not replace server revalidation on POST. |

---

## UC-11 — Approve a Pending Request

| Field | Value |
|---|---|
| Mutation | POST `/aprobaciones/{id}/aprobar` |
| Input fields | requestId, rowVersion, antiforgery token |
| Application contract | Approve command with approverId, requestId, concurrency token → approve result |
| Authorization | Active Approver + not owner + canResolveRequests=true |
| Transaction / Audit | Single DB transaction: Pending→Approved + Reservation→Deduction + BalanceMovement (Deduction) + AuditRecord (Approve) |
| Concurrency | Optimistic concurrency on request and balance; overlap recheck within same transaction |
| Notes | Denial of own request + denial of Inactive Approver → security event. PRG after success. |

---

## UC-12 — Reject a Pending Request

| Field | Value |
|---|---|
| Mutation | POST `/aprobaciones/{id}/rechazar` |
| Input fields | requestId, rejectionReason, rowVersion, antiforgery token |
| Application contract | Reject command with approverId, requestId, rejectionReason, concurrency token → reject result |
| Authorization | Active Approver + not owner |
| Transaction / Audit | Single DB transaction: Pending→Rejected + Release + BalanceMovement (Release) + AuditRecord (Reject) |
| Concurrency | Optimistic concurrency on request |
| Notes | RejectionReason normalized 10–500 chars; stored on request; never in logs or audit Data. |

---

## UC-13 — Deactivate an Approved Request Before Its Start Date

| Field | Value |
|---|---|
| Mutation | POST `/aprobaciones/{id}/desactivar` |
| Input fields | requestId, rowVersion, antiforgery token |
| Application contract | Deactivate command with approverId, requestId, concurrency token → deactivate result |
| Authorization | Active Approver + not owner + pre-start deactivation eligible (Status=Approved + StartDate > today) |
| Transaction / Audit | Single DB transaction: Approved→CancelledByApprover + Restoration + BalanceMovement (Restoration) + AuditRecord (Deactivate) |
| Concurrency | Optimistic concurrency on request and balance |
| Notes | Start-date boundary evaluated using system time at execution time (not cached field). No human reason required in MVP. |

---

## UC-14 — View Resolution History

| Field | Value |
|---|---|
| Entry point | GET `/aprobaciones/historial` |
| Application contract | Query approver history by approverId, page, pageSize, filters → paged resolution history |
| Output fields | date, action, requester, status, detail link |
| Authorization | Active Approver |
| Transaction / Audit | Read-only |
| Notes | Filterable by date range, action type, requester. Links to `/aprobaciones/{id}`. |

---

## UC-15 — View the Approver Calendar

| Field | Value |
|---|---|
| Entry point | GET `/calendario` |
| Application contract | Query calendar by userId, role=Approver, year, month → calendar data |
| Output fields | month grid, event markers (org-wide Approved periods, anonymized) |
| Authorization | Active Approver |
| Transaction / Audit | Read-only |
| Notes | Events anonymized. Detail link only when Approver is eligible for that resource. |

---

## UC-16 — Cancel an Unresolved Pending Request by Timeout

| Field | Value |
|---|---|
| Entry point | Background hosted service (timeout cancellation service) |
| Application contract | Timeout job with system date, batch size → no HTTP route |
| Authorization | System actor only; not triggered by any HTTP request |
| Transaction / Audit | Per-request transaction: Pending→CancelledByTimeout + Release + BalanceMovement (Release) + AuditRecord (Timeout, actorId=System) |
| Concurrency | Optimistic concurrency on request; idempotent (skip if not Pending); yield to concurrent human resolution |
| Configuration | `NovaLeave:PendingRequestTimeoutDays` (NEEDS CONFIGURATION); cadence configurable |
| Notes | Bounded batch processing. Fully idempotent. System actor recorded in AuditRecord. |

---

## UC-17 — Execute Monthly Vacation Accrual

| Field | Value |
|---|---|
| Entry point | Background hosted service (monthly accrual service) |
| Application contract | Accrual job with reference date → no HTTP route |
| Authorization | System actor only |
| Transaction / Audit | Per-user transaction: AccruedDays += 1 + BalanceMovement (Accrual) + AuditRecord (Accrual) |
| Concurrency | Unique constraint on (UserId, AccrualPeriod) — idempotent |
| Configuration | Cadence: NEEDS CONFIGURATION |
| Notes | Accrual period = completed calendar month from EmploymentStartDate. First partial month excluded. |

---

## UC-18 — View Organization-Wide Vacation Requests (HR)

| Field | Value |
|---|---|
| Entry points | GET `/rrhh/solicitudes`, GET `/rrhh/solicitudes/{id}` |
| Application contracts | Query HR request list with filters, page → paged HR request summaries; Query HR request detail by requestId → HR request detail |
| Output fields | read-only list and detail including audit trail |
| Authorization | Active HR |
| Transaction / Audit | Read-only; HR access to Reason/RejectionReason → AuditRecord (HRSensitiveAccess) |
| Notes | Sensitive reasons audited (field name, not content). No approve/reject/deactivate actions. |

---

## UC-19 — View the Organizational Vacation Calendar (HR)

| Field | Value |
|---|---|
| Entry point | GET `/rrhh/calendario` |
| Application contract | Query HR calendar by year, month → HR calendar data (with requester names) |
| Output fields | month grid, event markers, requester names visible |
| Authorization | Active HR |
| Transaction / Audit | Read-only |
| Notes | Requester names visible to HR (authorized). Event activation → `/rrhh/solicitudes/{id}`. |

---

## UC-20 — View Vacation Balances and Movements (HR)

| Field | Value |
|---|---|
| Entry points | GET `/rrhh/saldos`, GET `/rrhh/saldos/{userId}` |
| Application contracts | Query HR balances with page → paged balance summaries; Query HR balance movements by userId → balance movements |
| Output fields | Acumulado total, Pendientes, Días gozados, Disponible; movement timeline |
| Authorization | Active HR |
| Transaction / Audit | Read-only; no balance modification endpoint |
| Notes | No generic balance-update endpoint. Any overposting attempt → 403 + security event. |

---

## UC-21 — View Relevant Audit Records (HR)

| Field | Value |
|---|---|
| Entry point | GET `/rrhh/auditoria` |
| Application contract | Query HR audit log with filters, page → paged audit records |
| Output fields | timestamp, actor, role, action, entity, result; Data column redacted |
| Authorization | Active HR |
| Transaction / Audit | Read-only |
| Notes | Sensitive fields (Reason, RejectionReason) always redacted. Immutable records. |

---

## UC-22 — Manage Approver Resolution Capability (HR)

| Field | Value |
|---|---|
| Entry points | GET `/rrhh/aprobadores`, GET `/rrhh/aprobadores/{id}/capacidad`, POST `/rrhh/aprobadores/{id}/capacidad` |
| Input fields | targetId, desiredState, reason, confirmation, rowVersion, antiforgery token |
| Application contracts | Query approvers list → approvers list; Toggle command with hrId, targetId, enable, reason, confirmed, concurrency token → toggle result |
| Authorization | Active HR + HR for approver management (target must hold Approver role) |
| Transaction / Audit | Single DB transaction: CanResolveRequests update + AuditRecord (ToggleCapability, before/after values without reason content) |
| Concurrency | Optimistic concurrency on identity |
| Notes | Reason normalized 10–500 chars; explicit confirmation required; security-stamp refresh after capability change. HR cannot assign/remove roles. Failed toggle → AuditRecord (ToggleCapabilityFailed). |