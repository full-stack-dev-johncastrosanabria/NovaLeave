# Feature Specification: NovaLeave MVP — Vacation Request Management
**Feature Branch**: `001-leave-management-mvp`
**Created**: 2026-07-15
**Last Refactored**: 2026-07-23
**Status**: Ready for Planning. The core `User`/`Approver`/`HR` vacation workflow may proceed on the recorded decisions. The two prior Constitution conflicts (Approver deactivation of an approved request; time-zone removal) are **resolved** by the Constitution v4.0.0 amendment (2026-07-16). HR role added by Constitution v6.0.0 amendment (2026-07-23). Four business questions (OQ-002–OQ-005) remain open and should be settled before building the affected areas; OQ-001 (owner cancellation) is resolved as out of MVP scope by v4.0.0.
**Input**: Authoritative Product Owner decisions (2026-07-16) that supersede the prior Employee/Direct-Manager/HR, multi-leave-type model. The MVP is now a single-leave-type (`Vacation`) system with three application roles (`User`, `Approver`, `HR`), a global accruing balance with Pending reservations, Pending editing, automatic timeout cancellation, and Approver deactivation of an approved request before it begins. HR has organization-wide read access and approver-capability management per Constitution v6.0.0 §4.3.
**Governance**: This specification is subordinate to `.specify/memory/constitution.md` v6.0.0. The constitution remains authoritative for Clean Architecture, ASP.NET Core MVC, Razor Views, Bootstrap, ASP.NET Core Identity, persistence, security, testing, observability, and engineering governance. This specification defines business behavior and observable outcomes. It does not prescribe controllers, repositories, database tables, HTTP routes, framework classes, or code structure.
**Revision Note**: This revision replaces the previous business model in its entirety. Superseded and removed: the Employee/Direct-Manager/HR actor set (legacy three-role model with teams/hierarchies); Teams, organizational scope, primary/delegate/alternate managers, delegation, and escalation; `Personal Leave` and `Medical Leave`; per–Leave-Type balances and static seeded balances; holiday calendars; employee-specific time zones; the "Pending does not reserve balance" policy; the "`Approved` is always immutable / only three final states" rule; and "editing a Pending request is outside the MVP." Newly added: a global accruing vacation balance with Pending reservations; two request input modes; Pending editing with full revalidation; automatic timeout cancellation (`CancelledByTimeout`); Approver deactivation of an approved request before its start date (`CancelledByApprover`) with atomic balance restoration; active/inactive user status; session expiration; a basic visual vacation calendar; and the `HR` role with organization-wide read access to requests, calendar, balances, audit, and approver-capability management (activate/deactivate `canResolveRequests` for existing Approvers). Historical concepts are named here only to explain the revision; they are no longer normative policy.
**Constitution Alignment Note**: The two behaviors that previously conflicted with Constitution v3.0.0 — Approver deactivation of an `Approved` request (former invariants 8–9) and removal of time zones (former invariant 4) — are **reconciled by the Constitution v4.0.0 amendment (2026-07-16)**, which redefines the actor model (`User`/`Approver`) and lifecycle invariants 3, 4, 6–10, and 12 to match these Product Owner decisions. The Constitution v6.0.0 amendment (2026-07-23) adds `HR` as a third combinable role with read-only organization-wide access and approver-capability management, amending §4 (Actors), §5 (Lifecycle), §7.1 (Auth), §7.4 (Security Tests), §11 (MVC/Frontend Governance), §15.1 (Authority Order), §16.4 (Amendments), and §17 (Risks). Owner cancellation of a `Pending` request is deferred to a future approved specification by v4.0.0 invariant 7. This specification is aligned with Constitution v6.0.0; no unresolved Constitution conflict remains.
---

## Clarifications

### Session 2026-07-16 (Product Owner decisions applied)
- Q: Which leave types are in the MVP? → A: **Only `Vacation`.** `Personal Leave` and `Medical Leave` are out of scope. The domain remains parametric so more types can be added later.
- Q: Which application roles exist? → A: **`User`, `Approver`, and `HR`.** No teams, managers, delegates, escalation. Any active Approver may resolve any eligible request except their own. HR has organization-wide read access to requests, calendar, balances, audit, and approver-capability management (activate/deactivate `canResolveRequests` for existing Approvers with reason, confirmation, row version, audit) per Constitution v6.0.0 §4.3.
- Q: How is balance modeled? → A: **One global balance per User**, accruing **one day per completed month**, non-expiring; Pending requests **reserve** availability; approval makes the reservation a permanent deduction.
- Q: May a Pending request be edited? → A: **Yes**, with full atomic revalidation. Approved-request date editing remains out of scope.
- Q: What happens to an unresolved Pending request? → A: It is **automatically cancelled after a configurable `X` days** as `CancelledByTimeout`, releasing its reservation.
- Q: May an approved request be undone? → A: **Yes — an active Approver may deactivate it only before the vacation period begins** (`CancelledByApprover`), atomically restoring the deducted days. Partial deactivation and post-start deactivation are not allowed. *(Permitted under Constitution v4.0.0, invariants 7–9.)*
- Q: May a User independently cancel their own Pending request? → A: **Unresolved (OQ-001).** The Product Owner did not confirm it; Constitution invariant 7 permits it but supplies no details. Not invented here.

## User Scenarios & Testing *(mandatory)*
User journeys are ordered by business criticality. Each story is independently testable using prepared prerequisite data where required. Priority indicates implementation order.

### User Story 1 - User Submits and Manages a Vacation Request (Priority: P1)
As an authenticated active User, I want to submit a vacation request in either input mode, view its status and my balance, and edit it while it is still `Pending`, so that I can manage my own time off without spreadsheets or manual follow-up.
**Why this priority**: Submission is the entry point for every other workflow and the source of every reservation, approval, rejection, timeout, and audit record.
**Independent Test**: Authenticate as an active User, submit a valid request in each input mode, and verify that exactly one `Pending` request is created with a server-authoritative date range and working-day total, that its days are reserved against the global balance, that the creation is audited atomically, and that the request and balance are visible only to the owner and authorized actors.
**Acceptance Scenarios**:
1. **AC-001 — Submit a valid vacation request**
   **Related Requirements**: FR-001, FR-002, BR-001, BR-002, BR-004, BR-005, BR-018, BR-030, CON-007, AUD-001
   **Given** an authenticated active User and request data satisfying all date, overlap, and available-balance rules, **When** the User submits the request, **Then** the system creates exactly one `Vacation Request` in `Pending` state, reserves its authoritative working-day total against the User's global balance, and creates exactly one immutable creation audit record as one atomic operation.
2. **AC-041 — Submit using the date-range input mode**
   **Related Requirements**: FR-012, VAL-009, BR-004
   **Given** an authenticated active User, **When** the User submits a start date and an end date, **Then** the system normalizes the input into one authoritative date range and one authoritative working-day total and creates the request from those server-derived values.
3. **AC-042 — Submit using the start-date-plus-days input mode**
   **Related Requirements**: FR-012, VAL-009, BR-004
   **Given** an authenticated active User, **When** the User submits a start date and a number of working days, **Then** the system computes the authoritative end date and working-day total server-side and creates the request from those values.
4. **AC-002 — Reject missing core data**
   **Related Requirements**: FR-002, VAL-001, ERR-001
   **Given** an authenticated active User, **When** the User submits a request missing the start date, both end-date and day-count inputs, or the reason, **Then** the system rejects the submission, creates no request, and returns actionable validation feedback.
5. **AC-003 — Reject a reversed date range**
   **Related Requirements**: BR-001, ERR-001
   **Given** an authenticated active User, **When** the submitted start date is later than the resolved end date, **Then** the system rejects the submission and creates no request.
6. **AC-004 — Reject a start date that is not after today**
   **Related Requirements**: BR-002, ERR-001
   **Given** an authenticated active User, **When** the resolved start date is the current business date or earlier, **Then** the system rejects the submission and creates no request (the earliest allowed start is the following calendar day).
7. **AC-043 — Reject a zero-working-day request**
   **Related Requirements**: BR-024, VAL-001, ERR-001
   **Given** an authenticated active User, **When** the resolved range contains zero working days (for example a Saturday-to-Sunday range), **Then** the system rejects the submission, creates no request, and returns actionable validation feedback.
8. **AC-035 — Exclude weekends and count holidays as working days**
   **Related Requirements**: BR-004
   **Given** an authenticated active User and a request whose inclusive range spans one or more weekends and one or more public holidays, **When** the User submits it, **Then** the system calculates the authoritative working-day total as the count of Monday-through-Friday dates in the range, excluding Saturdays and Sundays and including every holiday date, and stores that total with the request.
9. **AC-005 — Ignore client-derived business values**
   **Related Requirements**: VAL-004
   **Given** a submission carrying a client-supplied working-day total, end date, day count, balance, owner, role, or status, **When** the system processes it, **Then** the system disregards those values and uses only server-derived authoritative values.
10. **AC-011 — Reject a request exceeding available balance after reservations**
    **Related Requirements**: BR-018, BR-031, BR-034
    **Given** an authenticated active User whose available balance (accrued minus permanently deducted minus days reserved by other active Pending requests) is fewer working days than the new request, **When** the User submits it, **Then** the system rejects the submission, creates no request, and reserves nothing.
11. **AC-010 — Reject an overlapping request**
    **Related Requirements**: BR-019, CON-008
    **Given** an authenticated active User with a `Pending` or `Approved` request, **When** the User submits a request whose inclusive calendar range intersects it, **Then** the system rejects the new request; adjacent non-intersecting ranges are permitted.
12. **AC-040 — Reject concurrent mutually-overlapping submissions**
    **Related Requirements**: BR-019, CON-008
    **Given** an authenticated active User with no existing requests, **When** two submissions whose ranges overlap each other are processed concurrently, **Then** the system creates at most one and rejects the other for prohibited overlap.
13. **AC-009 — Prevent duplicate request creation**
    **Related Requirements**: CON-001, AUD-001
    **Given** one valid submission, **When** it is replayed, retried, or processed concurrently, **Then** the system creates at most one request and at most one matching creation audit record.
14. **AC-007 — View only owned requests and balance**
    **Related Requirements**: FR-003, FR-004, AUTHZ-001
    **Given** an authenticated User with existing requests and a balance, **When** the User views their vacation information, **Then** the system displays only that User's requests, statuses, history, and global balance (accrued, reserved, deducted, and available).
15. **AC-008 — Deny cross-User access**
    **Related Requirements**: AUTHZ-001, AUTHZ-005, SEC-002, SEC-003
    **Given** an authenticated User, **When** the User attempts to view or modify another User's request or balance by manipulating an identifier, **Then** the system denies the operation without confirming whether the target exists.
16. **AC-045 — Edit an owned Pending request with revalidation**
    **Related Requirements**: FR-005, BR-025, CON-011, AUD-002
    **Given** an owned `Pending` request, **When** the owner edits its dates or input values and all revalidation checks pass, **Then** the system atomically re-normalizes the authoritative range and total, re-reserves the corrected balance, records the edit in an audit record, and keeps the request `Pending`.
17. **AC-046 — Reject a Pending edit that fails revalidation**
    **Related Requirements**: FR-005, BR-025, CON-011, ERR-002
    **Given** an owned `Pending` request, **When** the owner submits an edit that violates ownership, date validity, next-day minimum, overlap, available balance, or concurrency version, **Then** the system rejects the edit, preserves the prior request and reservation, and creates no successful-edit audit record.

### User Story 2 - Approver Resolves a Vacation Request (Priority: P1)
As an authenticated active Approver, I want to approve or reject any eligible `Pending` vacation request that is not my own, so that decisions are authorized, auditable, and applied without duplicate balance effects.
**Why this priority**: Approver resolution completes the primary transaction and converts a reservation into a permanent deduction or releases it.
**Independent Test**: With prepared `Pending` requests, authenticate as an active Approver and verify global (non-scoped) approval and rejection, self-resolution denial, inactive-Approver denial, balance conversion, concurrency control, atomicity, and auditing.
**Acceptance Scenarios**:
1. **AC-012 — Any active Approver approves an eligible request atomically**
   **Related Requirements**: FR-006, BR-008, BR-013, BR-015, BR-016, BR-020, AUTHZ-002, AUTHZ-003, AUTHZ-007, CON-002, CON-003, CON-005, CON-006, AUD-002
   **Given** a `Pending` request not owned by the acting Approver, sufficient available balance, and no prohibited overlap, **When** any active Approver approves it, **Then** the system transitions it to `Approved`, converts its reservation into a permanent deduction of the authoritative working-day total exactly once, and creates exactly one transition audit record as one atomic operation.
2. **AC-016 — Reject a request with a required reason**
   **Related Requirements**: FR-007, BR-009, BR-014, BR-021, BR-023, VAL-007, AUTHZ-002, AUTHZ-003, AUTHZ-007, CON-006, AUD-002, SEC-005
   **Given** a `Pending` request not owned by the acting Approver and a rejection reason of 10–500 normalized plain-text characters, **When** an active Approver rejects it, **Then** the system transitions it to `Rejected`, records the reason, releases its reservation with no permanent deduction, and creates exactly one transition audit record atomically.
3. **AC-036 — Reject an invalid rejection reason**
   **Related Requirements**: VAL-007, BR-023, ERR-001
   **Given** a `Pending` request not owned by the acting Approver, **When** an Approver attempts to reject it with a reason outside 10–500 normalized characters, **Then** the system rejects the rejection, preserves the request as `Pending`, and returns actionable validation feedback.
4. **AC-017 — Prevent self-resolution**
   **Related Requirements**: AUTHZ-003, AUTHZ-005, AUD-004
   **Given** a `Pending` request owned by the acting Approver, **When** that Approver attempts to approve, reject, or deactivate it, **Then** the system denies the operation, preserves the request and balance, and records a security event.
5. **AC-047 — Deny resolution by an inactive Approver**
   **Related Requirements**: AUTHZ-009, AUTHZ-010, AUD-004
   **Given** an Approver whose status is `Inactive`, **When** they attempt to approve, reject, or deactivate any request, **Then** the system denies the operation, changes no business data, and records a security event.
6. **AC-018 — Reject an approval that would exceed available balance**
   **Related Requirements**: BR-012, BR-015, BR-016, AUTHZ-007, ERR-002
   **Given** a `Pending` request whose authoritative available balance is insufficient at approval time, **When** an active Approver attempts approval, **Then** the system rejects it, preserves the request as `Pending`, produces no permanent deduction, and creates no successful-transition audit record.
7. **AC-019 — Handle concurrent approval attempts**
   **Related Requirements**: CON-002, CON-003, CON-006
   **Given** one `Pending` request, **When** two approval attempts run concurrently, **Then** exactly one transition to `Approved` succeeds, at most one permanent deduction occurs, exactly one audit record is created, and the conflicting attempt fails without side effects.
8. **AC-020 — Reject a stale Approver operation**
   **Related Requirements**: AUTHZ-007, CON-004, ERR-002, ERR-004
   **Given** an Approver who previously viewed a request, **When** its state, balance, or version changes before the Approver commits a resolution, **Then** the system rejects the stale operation without modifying request state or balance and returns a non-sensitive conflict outcome.
9. **AC-021 — Reject a transition on a terminal request**
   **Related Requirements**: BR-007, BR-011, SEC-004
   **Given** a request in `Rejected`, `CancelledByTimeout`, or `CancelledByApprover` state, or an `Approved` request whose vacation period has begun, **When** any actor attempts a further transition, **Then** the system rejects the operation and preserves the current state.
10. **AC-058 — Newly created request appears in the Approver queue**
    **Related Requirements**: FR-025, AUTHZ-002, AUTHZ-003, CON-008, AUD-001
    **Given** User A creates a valid vacation request, **When** the request creation transaction completes successfully, **Then** the request is stored as `Pending` and appears exactly once in the `Aprobaciones` queue for active Approver B, **And** Approver B can open its detail, **And** no team, hierarchy, department, or assigned-approver filter prevents visibility, **And** the request is not resolvable by its owner.
11. **AC-059 — Allow overlapping dates for different users**
    **Related Requirements**: BR-019, AUTHZ-002, CON-008
    **Given** User A has a `Pending` or `Approved` request for a date range, **When** User B submits a request that partially or fully overlaps that range, **Then** User B's request is allowed when User B independently satisfies all date, balance, and validation rules.
12. **AC-060 — Demo users available on login screen**
    **Related Requirements**: FR-001, CFG-003, SEC-001
    **Given** the application is seeded with demo identities (`user@demo`, `approver@demo`, `hr@demo`, all password `Demo123!`), **When** the login page is rendered, **Then** a visual account switcher (`Cuenta` dropdown) displays these identities with masked emails, **And** selecting one pre-fills the email field without authenticating, **And** submitting valid credentials for a demo identity grants access to the corresponding role context(s).

### User Story 3 - System Cancels an Unresolved Request by Timeout (Priority: P2)
As the system operator, I want a `Pending` request that is not resolved within a configurable number of days to be cancelled automatically, so that stale requests do not hold reservations indefinitely.
**Why this priority**: Timeout keeps reservations and availability accurate when no Approver acts, and it is required for a complete lifecycle.
**Independent Test**: With a `Pending` request older than the configured limit, run the timeout operation and verify a `CancelledByTimeout` outcome, reservation release, system-actor audit, idempotency, and correct behavior when racing a resolution.
**Acceptance Scenarios**:
1. **AC-048 — Automatically cancel an unresolved request**
   **Related Requirements**: FR-013, BR-026, CFG-001, CON-009, AUD-002, AUD-008
   **Given** a `Pending` request that has been unresolved for at least the configured timeout `X` days, **When** the automatic timeout operation runs, **Then** the system transitions it to `CancelledByTimeout`, releases its reservation with no permanent deduction, and creates exactly one audit record identifying the system as the actor and recording the configured timeout rule.
2. **AC-049 — Resolve timeout-versus-approval concurrency**
   **Related Requirements**: CON-002, CON-009
   **Given** one `Pending` request at its timeout boundary, **When** an approval (or rejection) and the timeout operation are processed concurrently, **Then** exactly one transition succeeds and the other fails without side effects; a repeated timeout operation is idempotent.

### User Story 4 - Approver Deactivates an Approved Request Before It Begins (Priority: P2)
As an authenticated active Approver, I want to deactivate an approved vacation request before its vacation period starts, so that plans that change can be corrected while restoring the User's balance. *(Permitted under Constitution v4.0.0, invariants 7–9.)*
**Why this priority**: Pre-start deactivation is an approved MVP correction path with a real balance effect, but it is secondary to the create-and-resolve flow.
**Independent Test**: With an `Approved` request whose start date is in the future, authenticate as an active Approver and verify a distinct `CancelledByApprover` outcome, atomic balance restoration, denial after the period begins, and denial of partial deactivation.
**Acceptance Scenarios**:
1. **AC-050 — Deactivate an approved request before its start date**
   **Related Requirements**: FR-011, BR-027, BR-035, AUTHZ-002, AUTHZ-003, AUTHZ-007, CON-010, AUD-002, AUD-007
   **Given** an `Approved` request whose start date has not yet arrived, **When** an active Approver who does not own it deactivates it, **Then** the system transitions it to `CancelledByApprover`, restores the previously deducted working-day total to the owner's balance, and creates exactly one transition audit record as one atomic operation.
2. **AC-051 — Deny deactivation after the period begins**
   **Related Requirements**: BR-027, SEC-004, ERR-002
   **Given** an `Approved` request whose vacation period has already begun, **When** an Approver attempts to deactivate it, **Then** the system rejects the operation and preserves the `Approved` state and the deduction.
3. **AC-052 — Deny partial deactivation**
   **Related Requirements**: BR-028, ERR-001
   **Given** an `Approved` request, **When** an Approver attempts to deactivate only part of its date range or day count, **Then** the system rejects the operation; deactivation applies to the whole request only.
4. **AC-053 — Restore balance atomically or not at all**
   **Related Requirements**: BR-035, CON-010, ERR-002
   **Given** a valid pre-start deactivation, **When** any part of the state transition, balance restoration, or audit write fails, **Then** the system commits none of them and preserves the `Approved` state and the deduction.

### User Story 5 - User Balance and Monthly Accrual (Priority: P2)
As an authenticated User, I want to see my global vacation balance and to accrue one day per completed month, so that my available days are accurate and non-expiring.
**Why this priority**: Accurate accrual and availability underpin every submission and approval decision.
**Independent Test**: With a User whose service spans one or more completed months, run accrual and verify one whole day is added per completed month, that days never expire, and that available balance reflects reservations and deductions.
**Acceptance Scenarios**:
1. **AC-054 — Accrue one day after a completed month**
   **Related Requirements**: FR-014, BR-032, BR-033
   **Given** a User who has completed a whole month of service, **When** accrual runs, **Then** the system adds exactly one whole vacation day to the User's global balance, applies no fractional or prorated accrual, and never expires accrued days.
2. **AC-044 — Available balance reflects reservations and deductions**
   **Related Requirements**: BR-031, FR-004
   **Given** a User with accrued days, active Pending reservations, and prior permanent deductions, **When** the User views their balance, **Then** the system presents available balance as accrued minus permanently deducted minus reserved, never below zero.

### User Story 6 - Basic Vacation Calendar (Priority: P3)
As an authenticated User, I want a basic visual vacation calendar, so that I can see vacation periods at a glance.
**Why this priority**: The calendar improves usability but is not required for the transactional core.
**Independent Test**: Authenticate as a User and verify that the basic visual calendar renders the authorized vacation periods.
**Acceptance Scenarios**:
1. **AC-057 — Access the basic vacation calendar**
   **Related Requirements**: FR-015, AUTHZ-001, SEC-002
   **Given** an authenticated User, **When** the User opens the vacation calendar, **Then** the system presents a basic visual calendar of the authorized vacation periods. *(Whether it shows only the User's own requests or broader anonymized availability is unresolved — see OQ-005.)*

### User Story 7 - Excess Vacation Request with HR Escalation (Priority: P2)
As an authenticated active User, I want to request more vacation days than my available balance by providing a justification, so that my Approver can escalate the request to HR for authorization or approve it with a negative balance carried forward from future accruals.
**Why this priority**: Real-world scenarios require exceptions; HR escalation provides governed flexibility without bypassing balance rules.
**Independent Test**: With a User whose available balance is 5 days, submit a 10-day request with justification; verify Approver sees escalation option, HR can authorize the excess, and balance reflects negative days with future accruals applied to the deficit.
**Acceptance Scenarios**:
1. **AC-061 — Submit request exceeding available balance with justification**
   **Related Requirements**: FR-026, VAL-010, BR-040, BR-041
   **Given** an authenticated active User with 5 available days, **When** the User submits a 10-day request with a justification of 10–500 characters, **Then** the system creates the request as `PendingExcess` (not `Pending`), reserving only the available 5 days, marking the remaining 5 as `ExcessDays`, and requiring Approver escalation to HR.
2. **AC-062 — Approver views excess request with escalation option**
   **Related Requirements**: FR-027, BR-042, AUTHZ-002
   **Given** a `PendingExcess` request, **When** an active Approver views it, **Then** the system displays the request with a prominent `Exceso de saldo` badge, shows `Disponible actual` (available days), `Días solicitados` (total requested), `Exceso` (requested minus available), and provides an `Escalar a RRHH` action alongside standard Approve/Reject.
3. **AC-063 — Approver escalates excess request to HR**
   **Related Requirements**: FR-028, BR-043, BR-044, AUD-009, CON-012
   **Given** a `PendingExcess` request, **When** an active Approver escalates it to HR with a mandatory reason (10–500 chars) and confirmation, **Then** the system transitions it to `EscalatedToHR`, locks it from further Approver action, records an escalation audit event, and notifies HR.
4. **AC-064 — HR authorizes excess days**
   **Related Requirements**: FR-029, BR-045, BR-046, AUTHZ-011, AUTHZ-012, AUD-009
   **Given** an `EscalatedToHR` request, **When** an active HR identity authorizes the excess days with a mandatory reason (10–500 chars) and confirmation, **Then** the system transitions it to `PendingAuthorizedExcess`, records the authorized excess count, restores the Approver's ability to approve/reject, and creates an authorization audit event.
5. **AC-065 — Approver approves authorized excess request**
   **Related Requirements**: FR-006, BR-013, BR-047, BR-048, CON-003, AUD-002
   **Given** a `PendingAuthorizedExcess` request with HR-authorized excess days, **When** an active Approver approves it, **Then** the system transitions it to `Approved`, deducts total requested days from balance (producing a negative available balance equal to the authorized excess), sets `NegativeBalanceCarryForward` equal to the authorized excess, and creates an approval audit record.
6. **AC-066 — Future accrual reduces negative balance carry-forward**
   **Related Requirements**: FR-014, BR-032, BR-049
   **Given** a User with `NegativeBalanceCarryForward > 0`, **When** monthly accrual runs, **Then** the system applies each accrued day first to reduce `NegativeBalanceCarryForward`, then increases available balance only after the carry-forward reaches zero.
7. **AC-067 — HR rejects excess authorization**
   **Related Requirements**: FR-030, BR-045, AUD-009
   **Given** an `EscalatedToHR` request, **When** an active HR identity rejects the excess authorization with a mandatory reason (10–500 chars), **Then** the system transitions it to `Pending` (standard request for available days only), adjusts `ExcessDays` to zero, reduces `Días solicitados` to the available balance at submission time, and notifies the Approver.
8. **AC-068 — Approver rejects excess request without escalation**
   **Related Requirements**: FR-007, BR-014, AUD-002
   **Given** a `PendingExcess` request, **When** an active Approver rejects it with a reason (10–500 chars), **Then** the system transitions it to `Rejected`, releases any reserved days, produces no permanent deduction, and creates a rejection audit record.

### Cross-Cutting Security and Failure Scenarios
1. **AC-030 — Deny unauthenticated access**
   **Related Requirements**: SEC-001, SEC-002
   **Given** an unauthenticated actor, **When** they attempt any protected action, **Then** the system denies access without revealing protected resource existence.
2. **AC-031 — Record authentication failures**
   **Related Requirements**: SEC-001, AUD-004
   **Given** an actor with invalid credentials, **When** they attempt a protected action, **Then** the system denies access and records the authentication failure.
3. **AC-055 — Expire authenticated sessions**
   **Related Requirements**: SEC-008, CFG-002
   **Given** an authenticated session that has reached its configured expiration, **When** the actor attempts a protected action, **Then** the system denies access and requires re-authentication.
4. **AC-056 — Reject reuse of an expired session**
   **Related Requirements**: SEC-008, AUD-004
   **Given** an expired or invalidated session artifact, **When** it is presented again, **Then** the system rejects it, changes no business data, and records the event.
5. **AC-032 — Protect browser state-changing operations**
   **Related Requirements**: SEC-007
   **Given** a browser state-changing request lacking valid anti-forgery protection, **When** the system processes it, **Then** the system rejects it and modifies no business data.
6. **AC-029 — Protect sensitive reasons outside authorized views**
   **Related Requirements**: SEC-005, SEC-006, AUD-006, ERR-003
   **Given** a request whose reason or rejection reason contains sensitive personal information, **When** the system emits logs, traces, metrics, audit before/after payloads, or user-facing technical errors, **Then** the full reason and rejection reason never appear in that output.
7. **AC-033 — Roll back a failed state-changing operation**
   **Related Requirements**: CON-006, CON-010, CON-011, ERR-002
   **Given** a valid approval, rejection, timeout, deactivation, or edit that fails before all effects complete, **When** the operation terminates, **Then** the system commits none of the state, balance, or successful-transition audit effects.
8. **AC-037 — Roll back creation when auditing fails**
   **Related Requirements**: CON-007, AUD-001, ERR-002
   **Given** an otherwise valid submission, **When** the required creation audit record cannot be persisted before the operation completes, **Then** the system commits neither the request, its reservation, nor the audit record.
9. **AC-038 — Fail closed when authoritative data is unavailable**
   **Related Requirements**: AUTHZ-008, ERR-005
   **Given** an operation requiring authoritative identity, active-status, leave-type, balance, ownership, or request-state data, **When** that data cannot be obtained or is ambiguous, **Then** the system rejects or defers the operation, changes no business data, substitutes no client-supplied value, and returns a non-sensitive retryable or corrective outcome.

### Edge Cases
- **EC-001 — Next-day boundary**: The earliest valid start date is the following calendar day; a same-day start is rejected.
- **EC-002 — Date-range adjacency**: Start and end dates are inclusive. A request beginning the calendar day after another ends is adjacent and does not overlap.
- **EC-003 — Multiple non-overlapping Pending requests**: Permitted only while their combined reserved days plus the new request do not exceed the User's global balance.
- **EC-004 — Reservation release**: When a Pending request is rejected or timed out, its reservation is released; because no permanent deduction occurred, this is a release, not a return of deducted days.
- **EC-005 — Holiday inside a range**: A public holiday inside the range is counted as a normal working day; the MVP has no holiday calendar.
- **EC-006 — Weekend-only range**: A range consisting solely of Saturday and/or Sunday yields zero working days and is rejected.
- **EC-007 — Timeout at the boundary racing a resolution**: Exactly one transition succeeds; the timeout operation is idempotent and concurrency-safe.
- **EC-008 — Deactivation exactly at start**: Deactivation is permitted strictly before the vacation period begins; once it has begun, deactivation is denied.
- **EC-009 — Duplicate submission or network retry**: At most one equivalent request is created.
- **EC-010 — Untrusted HTML or script in a reason**: The normalized reason and rejection reason are plain text (10–500 characters), may contain line breaks, and never execute markup or script.
- **EC-011 — Partial-day or hourly request**: Rejected; the MVP supports full working days only.
- **EC-012 — Editing an Approved request**: Editing the dates of an `Approved` request is out of scope; only pre-start deactivation is available.
- **EC-013 — Input-mode consistency**: Exactly one input mode is used per submission; the server derives and stores one authoritative range and one authoritative working-day total regardless of mode.
- **EC-014 — Terminal-state mutation**: A transition on `Rejected`, `CancelledByTimeout`, `CancelledByApprover`, or a started `Approved` request is rejected for every actor.
- **EC-015 — Failure during audit persistence**: The related business operation does not partially commit.
- **EC-016 — Re-submit after a non-approved outcome**: A prior `Rejected`, `CancelledByTimeout`, or `CancelledByApprover` request does not block a new intentional submission that independently satisfies all current rules; such prior requests do not reserve balance or block dates.
- **EC-017 — Inactive Approver**: An inactive Approver cannot approve, reject, or deactivate any request.
- **EC-018 — Inactive User**: Whether an inactive User may create or edit requests is unresolved (OQ-003); only the inactive-Approver resolution rule is defined.
---

## Requirements *(mandatory)*
### Functional Requirements
Every normative requirement uses exactly one standard EARS classification: **Ubiquitous** (`The system shall …`), **Event-Driven** (`When <trigger>, the system shall …`), **State-Driven** (`While <state>, the system shall …`), **Unwanted Behavior** (`If <condition>, then the system shall …`), or **Optional** (`Where <approved capability applies>, the system shall …`). No Optional requirements are defined.

*Retired identifiers (behavior removed with the prior model): FR-008 is reserved pending the owner-cancellation decision (OQ-001) and is intentionally not reused. FR-009 and FR-010 (HR read-only visibility) are **active** under Constitution v6.0.0 §4.3. AUTHZ-004 (HR read-only restriction) is **revised** — HR has authorized read access but MUST NOT approve, reject, deactivate, modify balances, or assign/remove roles.*

#### User Capabilities
- **FR-001 — Create a Vacation Request** *(Event-Driven)* — When an authenticated active User submits request data that satisfies all applicable requirements, the system shall create exactly one `Vacation Request`.
- **FR-002 — Require Core Request Data** *(Ubiquitous)* — The system shall require, for every request, a start date, exactly one of an end date or a number of working days (per the selected input mode), and a reason.
- **FR-003 — View Owned Requests** *(Ubiquitous)* — The system shall allow an authenticated User to view their own requests, statuses, and history.
- **FR-004 — View Own Global Balance** *(Ubiquitous)* — The system shall allow an authenticated User to view their own global vacation balance, including accrued, reserved, permanently deducted, and available working days.
- **FR-005 — Edit an Owned Pending Request** *(State-Driven)* — While a request owned by the User is `Pending`, the system shall allow the owner to edit it, subject to full revalidation (BR-025).
- **FR-012 — Support Both Input Modes** *(Ubiquitous)* — The system shall accept a request as either a start date and end date, or a start date and a number of working days, and shall normalize either into one authoritative date range and one authoritative working-day total before creating or updating the request.
- **FR-015 — Provide a Basic Vacation Calendar** *(Ubiquitous)* — The system shall provide a basic visual vacation calendar of the authorized vacation periods to an authenticated User.
- **FR-023 — Calculate and Return Projected Post-Approval Balance** *(Event-Driven)* — When an Approver views a `Pending` request eligible for approval, the system shall calculate and return the projected available balance after approval, computed as authoritative available balance minus authoritative requested working days, for display as an informational value only.
- **FR-024 — Navigate from Calendar Event to Authorized Request Detail** *(Event-Driven)* — When an authorized user activates a calendar event or its accessible dialog, the system shall navigate to the corresponding request detail view according to the active role and authorization: User owner → own request detail; eligible Approver → Approver request detail; HR → read-only HR request detail. Navigation shall be denied if the actor lacks authorization for the target view.
- **FR-025 — Expose Newly Created Pending Requests to Eligible Approvers** *(Event-Driven)* — When a valid vacation-request creation transaction completes, the system shall make the resulting Pending request available exactly once in the approval queue of every active eligible Approver other than its owner, without requiring manual synchronization or application restart.
- **FR-026 — Request Excess Vacation Days with Justification and HR Escalation** *(Event-Driven)* — When an authenticated active User submits a request whose authoritative working-day total exceeds their available balance, the system shall require a mandatory justification (10–500 characters), create the request in a distinct `PendingEscalated` state, and route it to active HR identities for resolution; an Approver may not approve or reject a `PendingEscalated` request, and HR may resolve it by approving (producing a negative balance offset by future accruals) or rejecting.

#### Approver Capabilities
- **FR-006 — Approve a Pending Request** *(State-Driven)* — While a request is `Pending`, the system shall provide an approval action to any active Approver who does not own it.
- **FR-007 — Reject a Pending Request** *(State-Driven)* — While a request is `Pending`, the system shall provide a rejection action, requiring a rejection reason, to any active Approver who does not own it.
- **FR-011 — Deactivate an Approved Request Before Start** *(State-Driven)* — While a request is `Approved` and its vacation period has not begun, the system shall provide a deactivation action to any active Approver who does not own it, producing a distinct `CancelledByApprover` outcome. *(Permitted under Constitution v4.0.0.)*

#### System Capabilities
- **FR-013 — Automatically Cancel Unresolved Requests by Timeout** *(Event-Driven)* — When a request has remained `Pending` for at least the configured timeout `X` days without resolution, the system shall cancel it as `CancelledByTimeout`.
- **FR-014 — Accrue Vacation Monthly** *(Event-Driven)* — When a User completes a whole month of service, the system shall add exactly one whole vacation day to that User's global balance.

#### HR Capabilities *(Constitution v6.0.0 §4.3)*
- **FR-009 — HR Read-Only Request List** *(State-Driven)* — While authenticated as an active HR identity, the system shall provide a read-only, filterable, paginated list of all vacation requests across the organization, including requester, dates, status, working days, reservation, and deduction.
- **FR-016 — HR Read-Only Request Detail** *(State-Driven)* — While authenticated as an active HR identity, the system shall provide a read-only detail view of any vacation request including its full audit trail, without resolution actions.
- **FR-017 — HR Organizational Calendar** *(State-Driven)* — While authenticated as an active HR identity, the system shall provide a month-view calendar showing all approved vacation periods organization-wide, with requester names visible (authorized for HR).
- **FR-018 — HR Read-Only Balances List** *(State-Driven)* — While authenticated as an active HR identity, the system shall provide a list of all users with balance summary (accrued, reserved, deducted, available).
- **FR-019 — HR Read-Only Balance Movements** *(State-Driven)* — While authenticated as an active HR identity, the system shall provide a timeline of balance movements for any user (accruals, reservations, deductions, restorations).
- **FR-020 — HR Relevant Audit Log** *(State-Driven)* — While authenticated as an active HR identity, the system shall provide a filterable audit log with timestamp, actor, role, action, entity, and result.
- **FR-021 — HR Approver Capability List** *(State-Driven)* — While authenticated as an active HR identity, the system shall provide a list of all identities with the `Approver` role and their `canResolveRequests` status.
- **FR-022 — HR Toggle Approver Capability** *(Event-Driven)* — When an active HR identity requests to activate or deactivate `canResolveRequests` for an identity that already has the `Approver` role, the system shall require an explicit reason, confirmation, validation of target's Approver role, expected row version, optimistic concurrency handling, transaction-safe persistence, and audit logging.
- **FR-027 — HR Resolve Escalated Request** *(State-Driven)* — While a request is in `PendingEscalated` state, the system shall provide approve and reject actions to any active HR identity, requiring a resolution reason (10–500 characters); approval transitions to `ApprovedEscalated` (producing negative balance offset by future accruals), rejection transitions to `Rejected`.
- **FR-028 — Apply Accrual Recovery to Negative Balance** *(Event-Driven)* — When a User with a negative available balance (due to `ApprovedEscalated` request) completes a whole month of service, the system shall apply the accrued day first to reduce the negative balance, recording the recovery in the balance movement timeline; only after the negative balance reaches zero shall subsequent accruals increase the positive available balance.
- **FR-029 — View Accrual Recovery Plan** *(Ubiquitous)* — When a User has a negative available balance, the system shall display the scheduled recovery plan showing months remaining until balance reaches zero based on current accrual rate, and shall project this recovery into the balance display for any new request creation.

### Validation Requirements
- **VAL-001 — Reject Missing Core Request Data** *(Unwanted Behavior)* — If a request omits the start date, both the end-date and day-count inputs, or the reason, then the system shall reject the submission and create no request.
- **VAL-002 — Validate the Leave Type** *(Ubiquitous)* — The system shall validate that every request references an active leave type; in the MVP the only active type is `Vacation`.
- **VAL-003 — Reject an Unknown or Inactive Leave Type** *(Unwanted Behavior)* — If a request references a nonexistent or inactive leave type, then the system shall reject the submission and create no request.
- **VAL-004 — Ignore Client-Derived Business Values** *(Unwanted Behavior)* — If a client supplies a working-day total, end date, day count, balance, owner identifier, role, status, or other server-derived value, then the system shall disregard it and use the authoritative server-derived value.
- **VAL-005 — Validate the Request Reason** *(Ubiquitous)* — The system shall trim leading and trailing whitespace, reject whitespace-only input, require the normalized reason to contain 10–500 Unicode text elements, allow line breaks, and treat markup as non-executable text.
- **VAL-006 — Reject Partial-Day Input** *(Unwanted Behavior)* — If a submission requests less than a full working day or supplies an hourly interval, then the system shall reject it as outside the MVP.
- **VAL-007 — Validate the Rejection Reason** *(Ubiquitous)* — The system shall trim leading and trailing whitespace, reject whitespace-only input, require an Approver's normalized rejection reason to contain 10–500 Unicode text elements, allow line breaks, and treat markup as non-executable text.
- **VAL-009 — Normalize and Validate the Input Mode** *(Ubiquitous)* — The system shall accept exactly one input mode per request, reject a submission that mixes or omits the required mode inputs, and derive one authoritative date range and one authoritative working-day total server-side.
- **VAL-010 — Validate Excess Justification** *(Ubiquitous)* — When a request's authoritative working-day total exceeds the User's available balance, the system shall require a separate justification field (distinct from the request reason) of 10–500 Unicode characters, trimmed, whitespace-only rejected, markup treated as plain text; the request shall be created in `PendingEscalated` state with the excess days recorded.

### Business Rules and Domain Invariants
*Retired identifiers: BR-003 is repurposed (time-zone dependence removed); BR-010 and BR-022 (owner-cancellation transition and its atomicity) are retired pending OQ-001 and not reused.*

#### Date and Working-Day Rules
- **BR-001 — Reject a Reversed Date Range** *(Unwanted Behavior)* — If a request's resolved start date is later than its resolved end date, then the system shall reject the request.
- **BR-002 — Require a Next-Day Minimum Start** *(Unwanted Behavior)* — If a request's resolved start date is the current business date or earlier, then the system shall reject the request; the earliest valid start date is the following calendar day.
- **BR-003 — Evaluate Dates Without Time-Zone Dependence** *(Ubiquitous)* — The system shall evaluate date rules against a single system business date and shall not apply per-user time-zone behavior. *(Aligned with Constitution v4.0.0 invariant 4.)*
- **BR-004 — Calculate Working Days Authoritatively** *(Ubiquitous)* — The system shall calculate the authoritative working-day total as a whole number, inclusively from the start date through the end date, counting Monday through Friday, excluding Saturdays and Sundays, and counting public holidays as ordinary working days; it shall store that total with the request for use throughout its lifecycle.
- **BR-024 — Reject a Zero-Working-Day Request** *(Unwanted Behavior)* — If a request's resolved range contains zero working days, then the system shall reject it and create no request.

#### Request Lifecycle
- **BR-005 — Initialize a Request as Pending** *(Event-Driven)* — When a request is successfully created, the system shall assign `Pending` as its initial state for requests within available balance, or `PendingEscalated` for requests exceeding available balance (requiring excess justification).
- **BR-006 — Restrict Pending Transitions** *(State-Driven)* — While a request is `Pending`, the system shall permit only editing (remaining `Pending`) or transition to `Approved`, `Rejected`, or `CancelledByTimeout`.
- **BR-007 — Define Terminal Outcomes** *(Ubiquitous)* — The system shall treat `Rejected`, `CancelledByTimeout`, and `CancelledByApprover` as terminal, and shall treat `Approved` as terminal except for a valid pre-start deactivation (BR-027).
- **BR-008 — Transition an Approved Request** *(Event-Driven)* — When an active Approver who does not own the request successfully approves a `Pending` request, the system shall transition it to `Approved`.
- **BR-009 — Transition a Rejected Request** *(Event-Driven)* — When an active Approver who does not own the request successfully rejects a `Pending` request, the system shall transition it to `Rejected`.
- **BR-011 — Reject Mutation of a Terminal Request** *(Unwanted Behavior)* — If an actor attempts to edit or transition a `Rejected`, `CancelledByTimeout`, or `CancelledByApprover` request, or an `Approved` request whose vacation period has begun, then the system shall reject the operation and preserve the current state.
- **BR-025 — Revalidate a Pending Edit Atomically** *(Event-Driven)* — When the owner edits a `Pending` request, the system shall, within one atomic operation, revalidate ownership, actor active status, date validity, next-day minimum, working-day calculation, overlap, reserved and available balance, and the request version, and shall keep the request `Pending` on success or reject it with no change on failure.
- **BR-026 — Transition a Timed-Out Request** *(Event-Driven)* — When a `Pending` request reaches the configured timeout, the system shall transition it to `CancelledByTimeout`.
- **BR-027 — Deactivate an Approved Request Before Start** *(Event-Driven)* — When an active Approver who does not own the request deactivates an `Approved` request whose vacation period has not begun, the system shall transition it to `CancelledByApprover`; the system shall reject deactivation once the period has begun. *(Permitted under Constitution v4.0.0.)*
- **BR-028 — Prohibit Partial Deactivation** *(Unwanted Behavior)* — If an actor attempts to deactivate only part of an `Approved` request's range or day count, then the system shall reject the operation; deactivation applies to the whole request only.
- **BR-029 — Prohibit Editing Approved Dates** *(Unwanted Behavior)* — If an actor attempts to edit the dates of an `Approved` request, then the system shall reject the operation.
- **BR-043 — Restrict PendingEscalated Transitions** *(State-Driven)* — While a request is `PendingEscalated`, the system shall permit only escalation to HR by an Approver (transition to `EscalatedToHR`) or rejection by an Approver (transition to `Rejected`); Approve action shall be prohibited on `PendingEscalated`.
- **BR-044 — HR Resolution of EscalatedToHR** *(State-Driven)* — While a request is `EscalatedToHR`, the system shall permit approval (transition to `ApprovedEscalated`) or rejection (transition to `Pending` with requested days reduced to available balance at submission time) by an active HR identity, with mandatory reason (10–500 chars) and confirmation.
- **BR-045 — Approver Action on PendingAuthorizedExcess** *(State-Driven)* — While a request is `PendingAuthorizedExcess` (after HR authorizes excess), the system shall permit Approve (transition to `Approved` with negative balance) or Reject (transition to `Rejected`) by any active Approver not owning the request.
- **BR-046 — ApprovedEscalated Creates Negative Balance Carry-Forward** *(Event-Driven)* — When a request transitions to `ApprovedEscalated`, the system shall deduct the total requested days from the User's balance (producing an available balance equal to the negative of the authorized excess), set `NegativeBalanceCarryForward` equal to the authorized excess, and record the deduction atomically with the transition.
- **BR-047 — Accrual Recovery Reduces Negative Balance Carry-Forward** *(Event-Driven)* — When monthly accrual runs for a User with `NegativeBalanceCarryForward > 0`, the system shall apply the accrued day to reduce `NegativeBalanceCarryForward` by one, recording a `BalanceRecovered` movement; when `NegativeBalanceCarryForward` reaches zero, subsequent accruals increase available balance normally.
- **BR-048 — Project Available Balance with Recovery Plan** *(Ubiquitous)* — When a User has `NegativeBalanceCarryForward > 0`, the system shall display the projected available balance as `available - reserved - NegativeBalanceCarryForward` and show the estimated months to recovery (`ceil(NegativeBalanceCarryForward / 1)` assuming one accrual per completed month).
- **BR-049 — Prohibit New Reservations While Negative Carry-Forward Exists** *(Unwanted Behavior)* — If a User's `NegativeBalanceCarryForward > 0`, the system shall reject any new or edited request that would require a positive reservation (i.e., any request when available balance <= 0) until `NegativeBalanceCarryForward` reaches zero.

#### Balance Rules
- **BR-012 — Prevent Negative Balance** *(Ubiquitous)* — The system shall never allow a User's global vacation balance, available balance, or reservation total to become negative; all balance quantities are non-negative whole working days.
- **BR-013 — Convert Reservation to Deduction on Approval** *(Event-Driven)* — When a request is successfully approved, the system shall convert its Pending reservation into a permanent deduction of its authoritative working-day total from the owner's global balance, exactly once.
- **BR-014 — Release Reservation Without Permanent Deduction** *(Event-Driven)* — When a request is rejected or cancelled by timeout, the system shall release its reservation and produce no permanent deduction.
- **BR-015 — Revalidate Available Balance Before Approval** *(Event-Driven)* — When approval is requested, the system shall revalidate the authoritative available balance immediately before approval.
- **BR-016 — Reject an Approval That Would Produce Negative Balance** *(Unwanted Behavior)* — If approval would drive the available or global balance below zero, then the system shall reject approval, preserve the request as `Pending`, and preserve the balance.
- **BR-017 — Maintain One Global Balance Per User** *(Ubiquitous)* — The system shall maintain exactly one global vacation balance per User and shall not partition balance by leave type.
- **BR-018 — Reject a Request Exceeding Available Balance** *(Event-Driven)* — When a request is submitted or edited, the system shall reject it if its authoritative working-day total exceeds the available balance, where available balance is the global balance reduced by days reserved by the User's other active Pending requests.
- **BR-030 — Reserve Balance for a Pending Request** *(Event-Driven)* — When a request is created or edited into `Pending`, the system shall reserve its authoritative working-day total against the owner's balance; the reservation blocks that availability without producing a permanent deduction.
- **BR-031 — Define Available Balance Authoritatively** *(Ubiquitous)* — The system shall compute available balance as accrued days minus permanently deducted days minus days reserved by active Pending requests.
- **BR-032 — Accrue One Day Per Completed Month** *(Event-Driven)* — When a User completes a whole month of service, the system shall accrue exactly one whole vacation day and shall not apply fractional or prorated monthly accrual.
- **BR-033 — Do Not Expire Accrued Days** *(Ubiquitous)* — The system shall not expire accrued vacation days; they accumulate indefinitely until permanently deducted by an approval.
- **BR-034 — Bound Request Size** *(Ubiquitous)* — The system shall require a minimum request of one working day and a maximum request equal to the User's currently available balance.
- **BR-035 — Restore Balance on Valid Deactivation** *(Event-Driven)* — When an `Approved` request is validly deactivated before its start date, the system shall restore its previously deducted working-day total to the owner's global balance as part of the same atomic operation as the state transition and audit record.
- **BR-036 — Calculate Projected Available Balance** *(Ubiquitous)* — The system shall compute the projected available balance after approval as `authoritativeAvailableExcludingCurrent - authoritativeRequestedDays`, using the authoritative available balance **excluding the current request's reservation** (accrued minus permanently deducted minus reserved by other active Pending requests) and the authoritative requested working-day total server-side; this value is informational only and must never be trusted from the client.
- **BR-037 — Deny Approval When Projected Balance Is Negative** *(Unwanted Behavior)* — If the projected available balance after approval would be negative (using `authoritativeAvailableExcludingCurrent - authoritativeRequestedDays`), the system shall reject the approval, preserve the request as `Pending`, produce no permanent deduction, and return a clear warning indicating insufficient available balance for the requested days.
- **BR-038 — Revalidate Projected Balance on Approval Submission** *(Event-Driven)* — When an approval POST is processed, the system shall revalidate the projected available balance immediately before committing the transition using `authoritativeAvailableExcludingCurrent - authoritativeRequestedDays`; if the revalidated projected balance is negative, the system shall reject the approval with a conflict outcome.
- **BR-039 — Allow Negative Balance via HR-Escalated Approval** *(Event-Driven)* — When a request in `PendingEscalated` state is approved by an active HR identity, the system shall permit the available balance to become negative by the excess amount (requested days minus available balance at approval time), record the negative offset as a permanent deduction, and track the deficit as a scheduled recovery to be satisfied by future monthly accruals for that User; the request transitions to `ApprovedEscalated`.
- **BR-040 — Recover Negative Balance from Future Accruals** *(Ubiquitous)* — When a User has a negative available balance due to an `ApprovedEscalated` request, the system shall apply each subsequent monthly accrual day first to reduce the negative balance until it reaches zero, then resume normal accrual to positive available balance; the projected balance on any new request must account for the accrual-scheduled recovery plan.
- **BR-041 — Prohibit New Requests While Negative Balance Exists** *(Unwanted Behavior)* — If a User's available balance is negative (due to an `ApprovedEscalated` request), the system shall reject any new vacation request submission or edit that would require a positive reservation until the scheduled accrual recovery has brought the available balance to zero or above.
- **BR-042 — Require Justification for Excess Request** *(Ubiquitous)* — When a request's authoritative working-day total exceeds the User's available balance at submission time, the system shall require a non-empty justification of 10–500 normalized Unicode characters, trimmed, whitespace-only rejected, markup treated as plain text; the request shall be created in `PendingEscalated` state.

#### Overlap Rules
- **BR-019 — Reject Prohibited Overlap** *(Unwanted Behavior)* — If a request's inclusive calendar-date range intersects another `Pending` or `Approved` request owned by the same User, then the system shall reject it; the request being evaluated is excluded from comparison with itself, adjacent non-intersecting ranges are permitted, and `Rejected`, `CancelledByTimeout`, and `CancelledByApprover` requests do not block dates.
- **BR-020 — Revalidate Overlap Before Approval** *(Event-Driven)* — When approval is requested, the system shall revalidate overlap against authoritative current requests immediately before approval.

#### Transactional Business Outcomes
- **BR-021 — Complete Rejection as One Outcome** *(Event-Driven)* — When an authorized rejection succeeds, the system shall commit the transition to `Rejected`, the reservation release, and the audit record as one indivisible outcome.
- **BR-023 — Capture the Rejection Reason** *(Event-Driven)* — When an active Approver rejects a `Pending` request, the system shall record the submitted rejection reason, which is required per VAL-007, with the resulting `Rejected` request.

### Authorization and Resource-Ownership Requirements
*Retired identifier: AUTHZ-004 (HR read-only restriction) is retired with the HR role.*
- **AUTHZ-001 — Restrict User Access to Owned Data** *(Ubiquitous)* — The system shall authorize a User to view or edit only requests and the balance owned by that User.
- **AUTHZ-002 — Authorize Any Active Approver Globally** *(Ubiquitous)* — The system shall authorize any active Approver to approve, reject, or deactivate any eligible request in the system, without team, hierarchy, or organizational scope.
- **AUTHZ-003 — Prevent Self-Resolution** *(Unwanted Behavior)* — If an Approver attempts to approve, reject, or deactivate a request they own, then the system shall reject the operation.
- **AUTHZ-005 — Deny by Default** *(Ubiquitous)* — The system shall deny every action not explicitly authorized for the actor, role, active status, resource, ownership relationship, and request state.
- **AUTHZ-006 — Evaluate Authorization at Operation Time** *(Ubiquitous)* — The system shall determine authorization from authoritative current identity, role, active status, ownership, and request state rather than from client-supplied or previously displayed values.
- **AUTHZ-007 — Revalidate Mutation Authorization and Preconditions** *(Event-Driven)* — When an actor requests creation, edit, approval, rejection, or deactivation, the system shall revalidate identity, active status, role, ownership, request state, and applicable business preconditions immediately before the mutation.
- **AUTHZ-008 — Fail Closed on Unavailable Authoritative Data** *(Unwanted Behavior)* — If authoritative identity, active-status, leave-type, balance, ownership, or request-state data required for an operation is unavailable or ambiguous, then the system shall deny or defer the operation, modify no business data, and substitute no client-supplied value.
- **AUTHZ-009 — Deny Resolution by an Inactive Approver** *(Unwanted Behavior)* — If an Approver whose status is `Inactive` attempts to approve, reject, or deactivate a request, then the system shall reject the operation and modify no business data.
- **AUTHZ-010 — Require Active Authenticated Identity for Protected Operations** *(Ubiquitous)* — The system shall require a current authenticated identity with `Active` status, role authorization, resource authorization, and a valid request state for every protected operation.
- **AUTHZ-011 — Authorize Active HR for Read Access** *(Ubiquitous)* — The system shall authorize any active HR identity to read all vacation requests, organizational calendar information, vacation balances and movements, and relevant audit information across the organization.
- **AUTHZ-012 — Authorize Active HR for Approver Capability Management** *(Event-Driven)* — When an active HR identity requests to activate or deactivate `canResolveRequests` for an identity that already has the `Approver` role, the system shall authorize the operation subject to explicit reason, confirmation, target Approver role validation, expected row version, optimistic concurrency, transaction-safe persistence, and audit logging.
- **AUTHZ-013 — Prohibit HR Request Resolution** *(Unwanted Behavior)* — If an HR identity attempts to approve, reject, or deactivate any standard `Pending` vacation request, then the system shall reject the operation and modify no business data.
- **AUTHZ-020 — Authorize HR Resolution of Escalated Requests** *(Ubiquitous)* — When a request is in `PendingEscalated` state (exceeds available balance with justification), the system shall authorize active HR identities to approve (transition to `ApprovedEscalated` with negative balance carry-forward) or reject (transition to `Rejected`) the request, requiring a resolution reason (10–500 chars) and confirmation.
- **AUTHZ-021 — Prohibit Approver Resolution of Escalated Requests** *(Unwanted Behavior)* — If an Approver attempts to approve or reject a `PendingEscalated` request, the system shall reject the operation; Approvers may only escalate to HR via the `Escalar a RRHH` action.
- **AUTHZ-022 — Authorize Approver Resolution of Authorized Excess** *(Ubiquitous)* — When a request is in `PendingAuthorizedExcess` state (HR has authorized the excess days), the system shall authorize any active Approver (except the owner) to approve (transition to `Approved` with negative balance) or reject (transition to `Rejected`).
- **AUTHZ-023 — Deny New Requests During Negative Carry-Forward** *(Unwanted Behavior)* — If a User has `NegativeBalanceCarryForward > 0` from an `ApprovedEscalated` request, the system shall deny creation or edit of any new vacation request until the accrual recovery plan brings `NegativeBalanceCarryForward` to zero.
- **AUTHZ-014 — Prohibit HR Balance Modification** *(Unwanted Behavior)* — If an HR identity attempts to modify any vacation balance directly, then the system shall reject the operation and modify no business data.
- **AUTHZ-015 — Prohibit HR Role Assignment** *(Unwanted Behavior)* — If an HR identity attempts to assign or remove any role, then the system shall reject the operation and modify no business data.
- **AUTHZ-016 — Require Active HR for Capability Management** *(Unwanted Behavior)* — If an identity with `Inactive` status and the `HR` role attempts to toggle `canResolveRequests`, then the system shall reject the operation and modify no business data.
- **AUTHZ-017 — Authorize Calendar Navigation per Role** *(Ubiquitous)* — The system shall authorize navigation from a calendar event to a request detail only when the actor holds the necessary role and resource authorization: User owners may navigate to their own request detail; eligible Approvers may navigate to the Approver request detail; active HR may navigate to the read-only HR request detail.
- **AUTHZ-018 — Preserve Calendar Anonymization on Unauthorized Navigation** *(Unwanted Behavior)* — If the calendar view is anonymized for a role, the system shall not expose a navigation link or action to an unauthorized request detail for that role.
- **AUTHZ-019 — Require Keyboard-Accessible Calendar Navigation** *(Ubiquitous)* — Calendar events and their dialogs shall support keyboard activation and provide an accessible name for navigation to the authorized request detail.

### Security and Privacy Requirements
- **SEC-001 — Deny Unauthenticated Access** *(Unwanted Behavior)* — If an unauthenticated actor attempts a protected action, then the system shall deny the operation.
- **SEC-002 — Avoid Protected Resource-Existence Disclosure** *(Unwanted Behavior)* — If an actor is not authorized for a protected resource, then the system shall deny the operation without confirming whether it exists.
- **SEC-003 — Prevent Identifier-Manipulation Access** *(Unwanted Behavior)* — If an actor manipulates a request, User, or balance identifier to reach an unauthorized resource, then the system shall deny the operation.
- **SEC-004 — Reject Invalid State Transitions** *(Unwanted Behavior)* — If an actor requests a transition not permitted for the request's current state, then the system shall reject it and preserve the current state.
- **SEC-005 — Restrict Sensitive Reason Visibility** *(Ubiquitous)* — The system shall expose a request's reason and rejection reason only to its owning User, to an authorized active Approver acting on it through an authorized use case, and to an authorized active HR identity through an authorized read-only use case.
- **SEC-006 — Redact Sensitive Information** *(Ubiquitous)* — The system shall redact request reasons, rejection reasons, credentials, tokens, secret values, and unnecessary personal information from logs, traces, metrics, audit before-and-after payloads, and user-facing technical errors.
- **SEC-007 — Protect Browser Mutations Against Request Forgery** *(Ubiquitous)* — The system shall require valid anti-forgery protection for every browser-based state-changing operation.
- **SEC-008 — Expire Sessions and Reject Reuse** *(Unwanted Behavior)* — If an authenticated session has expired or been invalidated, then the system shall deny protected access using it, require re-authentication, and not honor the expired session artifact.
- **SEC-009 — Audit HR Access to Sensitive Reasons** *(Event-Driven)* — When an active HR identity accesses a request reason or rejection reason through an authorized read-only use case, the system shall create a structured audit event recording the HR actor, the accessed request, the reason field accessed, and the timestamp.

### Concurrency, Duplicate-Operation, and Atomicity Requirements
- **CON-001 — Prevent Duplicate Request Creation** *(Unwanted Behavior)* — If the same logical submission for a User, start date, and resolved range is replayed, retried, or processed concurrently, then the system shall create at most one request; a prior terminal request shall not permanently block a later intentional submission with the same values.
- **CON-002 — Apply One Conflicting State Transition** *(Unwanted Behavior)* — If conflicting state-changing operations (approval, rejection, timeout, deactivation) are processed concurrently for the same request, then the system shall apply exactly one valid transition and reject the others without partial effects.
- **CON-003 — Prevent Duplicate Balance Effect** *(Unwanted Behavior)* — If an approval or deactivation is replayed, retried, or processed concurrently, then the system shall apply the corresponding permanent deduction or restoration at most once.
- **CON-004 — Reject Stale Mutations** *(Unwanted Behavior)* — If request state, balance, ownership, active status, or request version changes before a state-changing operation is committed, then the system shall reject the stale operation without modifying request state or balance.
- **CON-005 — Evaluate Overlap Atomically With Confirmation** *(Event-Driven)* — When a request is confirmed for approval, the system shall evaluate the authoritative overlap condition within the same indivisible operation as the approval.
- **CON-006 — Commit Approval Atomically** *(Event-Driven)* — When an approval succeeds, the system shall commit the transition to `Approved`, the reservation-to-deduction conversion, and exactly one audit record as one indivisible operation.
- **CON-007 — Commit Request Creation Atomically** *(Event-Driven)* — When request creation succeeds, the system shall commit the new `Pending` request, its balance reservation, and exactly one immutable creation audit record as one indivisible operation.
- **CON-008 — Evaluate Submission and Edit Overlap and Reservation Atomically** *(Event-Driven)* — When a request is created or edited, the system shall evaluate overlap and reserve available balance within the same indivisible operation, so that concurrent operations cannot both create mutually overlapping requests or over-reserve the balance.
- **CON-009 — Make Timeout Idempotent and Concurrency-Safe** *(Event-Driven)* — When the timeout operation runs, the system shall transition each eligible request at most once, remain safe under repetition, and yield to any concurrently committed resolution.
- **CON-010 — Commit Deactivation Atomically** *(Event-Driven)* — When a valid deactivation succeeds, the system shall commit the transition to `CancelledByApprover`, the balance restoration, and exactly one audit record as one indivisible operation.
- **CON-011 — Commit Pending Edit Atomically** *(Event-Driven)* — When a Pending edit succeeds, the system shall commit the revalidated values, the corrected reservation, and exactly one audit record as one indivisible operation.
- **CON-012 — Enforce Mutually Exclusive Approve and Reject Transitions** *(Unwanted Behavior)* — The system shall treat approve and reject as mutually exclusive transitions for a given request; after one action is submitted, all conflicting resolution actions shall be disabled server-side, the request state shall be revalidated on POST, and exactly one valid transition shall be permitted with optimistic concurrency control; stale or duplicate competing transitions shall be rejected without side effects.

### Audit and Observability Requirements
- **AUD-001 — Audit Request Creation** *(Event-Driven)* — When a request is successfully created, the system shall create exactly one immutable creation audit record; a duplicate, retry, or replay of an already-processed creation shall not create an additional record.
- **AUD-002 — Audit Every Successful State Change** *(Event-Driven)* — When a request successfully transitions to `Approved`, `Rejected`, `CancelledByTimeout`, or `CancelledByApprover`, or is edited while `Pending`, the system shall create exactly one immutable audit record.
- **AUD-003 — Capture Minimum Audit Context** *(Ubiquitous)* — The system shall include at minimum the UTC timestamp, actor identifier, actor role, action, entity type, entity identifier, result, correlation identifier, and request identifier in every business audit record.
- **AUD-004 — Record Security-Relevant Failures** *(Event-Driven)* — When authentication fails, an expired session is used, authorization is denied, an inactive Approver attempts a resolution, an invalid transition is attempted, or suspected unauthorized resource access occurs, the system shall create a structured security event suitable for monitoring and alerting.
- **AUD-005 — Protect Audit Records** *(Ubiquitous)* — The system shall prevent normal application operations from modifying or physically deleting audit records.
- **AUD-006 — Redact Audit Payloads** *(Ubiquitous)* — The system shall redact sensitive fields, including request and rejection reasons, from audit before-and-after values and security-event payloads.
- **AUD-007 — Audit Balance Deduction and Restoration** *(Event-Driven)* — When a balance is permanently deducted on approval or restored on deactivation, the system shall record that balance effect within the same audit record as its transition.
- **AUD-008 — Attribute Timeout to the System Actor** *(Event-Driven)* — When a request is cancelled by timeout, the system shall record the system as the actor and record the configured timeout rule that triggered the transition, and shall not require a human rejection reason.
- **AUD-009 — Audit HR Approver Capability Toggle** *(Event-Driven)* — When an active HR identity successfully activates or deactivates `canResolveRequests` for an Approver, the system shall create exactly one immutable audit record with the HR actor identifier, target Approver identifier, prior and new capability value, explicit reason, row version, and timestamp.
- **AUD-010 — Audit Failed HR Capability Toggle** *(Event-Driven)* — When an HR capability toggle is rejected due to missing reason, missing confirmation, missing/invalid row version (optimistic concurrency conflict), target lacking Approver role, or inactive HR actor, the system shall create a structured security event recording the HR actor, target (if applicable), failure reason, and timestamp.
- **AUD-011 — Audit Request Escalation to HR** *(Event-Driven)* — When an Approver escalates a `PendingEscalated` request to HR, the system shall create an immutable audit record with the Approver actor, target request, action `Escalated`, excess days count, escalation reason, and timestamp.
- **AUD-012 — Audit HR Resolution of Escalated Request** *(Event-Driven)* — When an active HR identity resolves an `EscalatedToHR` request (approve or reject), the system shall create an immutable audit record with the HR actor, target request, action `AuthorizedExcess` or `RejectedExcess`, authorized excess days (if approved), resolution reason, row version, and timestamp.
- **AUD-013 — Audit Accrual Recovery Movement** *(Event-Driven)* — When monthly accrual reduces a User's `NegativeBalanceCarryForward`, the system shall create a balance movement audit record with action `AccrualRecovery`, amount applied to carry-forward, resulting `NegativeBalanceCarryForward`, and timestamp.

### Error and Failure Requirements
- **ERR-001 — Return Actionable Validation Feedback** *(Unwanted Behavior)* — If validation fails, then the system shall reject the operation and communicate the failed rule without exposing sensitive data or implementation details.
- **ERR-002 — Preserve State on Failed Mutation** *(Unwanted Behavior)* — If a creation, edit, approval, rejection, timeout, or deactivation does not complete successfully, then the system shall preserve the pre-operation state, reservation, and balance and shall create no successful-transition audit record.
- **ERR-003 — Protect User-Facing Errors** *(Ubiquitous)* — The system shall present user-facing errors without stack traces, database details, secret values, or sensitive request content.
- **ERR-004 — Return a Conflict Outcome for Stale Operations** *(Unwanted Behavior)* — If a state-changing operation is rejected because authoritative data changed, then the system shall communicate a non-sensitive conflict outcome that allows the actor to refresh current information.
- **ERR-005 — Return a Safe Authoritative-Data Failure Outcome** *(Unwanted Behavior)* — If required authoritative data cannot be retrieved or resolved, then the system shall return a non-sensitive retryable or corrective outcome, expose no implementation details, and preserve all business data.
- **ERR-006 — Display a System Timeout Explanation** *(Event-Driven)* — When a request is cancelled by timeout, the system may present a standard system-generated timeout explanation to the request owner without a human reason.

### Configuration Requirements
- **CFG-001 — Configure the Timeout Limit** *(Ubiquitous)* — The system shall treat the Pending-resolution timeout `X` (in days) as a configurable system parameter that can change without altering the domain workflow.
- **CFG-002 — Configure Session Lifetime** *(Ubiquitous)* — The system shall treat authenticated-session lifetime and expiration behavior as configurable, consistent with the constitutional security baseline.
- **CFG-003 — Provision Initial Demo Identities** *(Ubiquitous)* — The system shall provide a configurable, idempotent mechanism to seed initial demo users at application startup (e.g., via `IHostedService` or `IDbInitializer`) with the following roles and statuses for manual verification and E2E testing:
  - `demo.user@novaleave.test` — roles: `User`, `Approver` (Active); HR: Inactive
  - `demo.approver@novaleave.test` — role: `Approver` (Active, `canResolveRequests=true`); User: Inactive; HR: Inactive
  - `demo.rrhh@novaleave.test` — role: `HR` (Active); User: Inactive; Approver: Inactive
  - `demo.combo@novaleave.test` — roles: `User` (Active), `Approver` (Active, `canResolveRequests=true`), `HR` (Active) — for multi-role context switching
  - All identities: `Active=true`, default password configurable, employment start date = 12 months before seed date (ensures accrued balance ≥ 12 days), initial `canResolveRequests` per role assignment above
  - Seeding is opt-in via configuration flag (e.g., `NovaLeave:SeedDemoUsers=true`) and MUST NOT run in Production environments

### Key Entities *(include if feature involves data)*
- **User**: A person authenticated through ASP.NET Core Identity, linked to the business user by a stable identifier. Holds one or more of the roles `User`, `Approver`, and `HR`, an `Active`/`Inactive` status, service or employment-start information sufficient to determine completed months for accrual, and exactly one global vacation balance.
- **HR**: An identity with the `HR` role and `Active`/`Inactive` status. Authorized for organization-wide read access to vacation requests, organizational calendar information, vacation balances and movements, relevant audit information, and Approver capability status (`canResolveRequests`). May activate or deactivate `canResolveRequests` only for an identity that already has the `Approver` role, requiring explicit reason, confirmation, expected row version, optimistic concurrency handling, transaction-safe persistence, and audit logging. MUST NOT approve, reject, deactivate requests; modify vacation balances; or assign/remove roles. *(Constitution v6.0.0 §4.3)*
- **Vacation Request**: A User's request for full working days of vacation. Attributes: owner, input mode, authoritative start date, authoritative end date, authoritative working-day total, reason, state, reservation effect, approval metadata, rejection metadata (including rejection reason), timeout metadata, deactivation metadata, and a version/concurrency identity. It begins `Pending` and transitions per the lifecycle rules.
- **Vacation Balance**: The global, non-expiring balance for one User. Attributes: accrued days, permanently deducted days, days reserved by active Pending requests, and available days (accrued − deducted − reserved). All quantities are non-negative whole working days.
- **Leave Type**: A parametric leave classification. In the MVP the only active type is `Vacation`; the model remains extensible so additional types can be introduced later without redesigning the request lifecycle. Balance is global and not partitioned by type.
- **Request State**: Exactly one of `Pending`, `PendingEscalated`, `EscalatedToHR`, `PendingAuthorizedExcess`, `Approved`, `ApprovedEscalated`, `Rejected`, `CancelledByTimeout`, or `CancelledByApprover`.
- **System Parameter**: A configurable business value that may change without altering the domain workflow, including the timeout limit `X` and session-lifetime settings.
- **Audit Record**: An immutable record of creation, editing, or a successful transition, capturing actor, actor role, action, target, result, balance effect where applicable, and traceability identifiers without unnecessary sensitive content.
- **Security Event**: A structured record of a security-relevant failure or anomaly, separate from a successful business audit record.

*Removed from the domain: Team, Direct Manager Assignment, HR-specific entities, and any per–Leave-Type balance.*
---

## Success Criteria *(mandatory)*
- **SC-001 — Exactly-One Request Creation**: 100% of valid single submissions create exactly one `Pending` request, one reservation, and one creation audit record. **Measurement**: acceptance and integration tests.
- **SC-002 — Complete Transition Auditing**: 100% of successful transitions (`Approved`, `Rejected`, `CancelledByTimeout`, `CancelledByApprover`) and Pending edits create exactly one matching audit record. **Measurement**: audit-query integration tests.
- **SC-003 — No Negative Balances**: 0 operations drive global, available, or reserved balance below zero. **Measurement**: domain, integration, and concurrency tests plus production invariant monitoring.
- **SC-004 — No Duplicate Balance Effects**: 0 replayed or concurrent approvals or deactivations produce more than one deduction or restoration for the same request. **Measurement**: concurrency tests.
- **SC-005 — Authorization Isolation**: 100% of tested cross-User (IDOR), self-resolution, inactive-Approver, unauthenticated, and expired-session attempts are denied without returning protected data. **Measurement**: authorization/security test suite.
- **SC-006 — Terminal-State Integrity**: 100% of tested mutation attempts against terminal requests (and started `Approved` requests) are rejected and preserve state. **Measurement**: domain and integration tests.
- **SC-007 — Sensitive Data Protection**: 0 request or rejection reasons, credentials, tokens, or secrets appear in logs, traces, metrics, technical errors, or unredacted audit payloads. **Measurement**: redaction tests and controlled inspection.
- **SC-008 — Complete Approver Workflow**: Any active Approver can retrieve and approve or reject any eligible `Pending` request that is not their own, without manual intervention. **Measurement**: end-to-end Approver journey test.
- **SC-009 — Reservation Accuracy**: 100% of tested submission/edit/reject/timeout/approval sequences leave reserved and available balance consistent with accrued and deducted totals. **Measurement**: balance-invariant integration tests.
- **SC-010 — Concurrency Integrity**: 100% of tested conflicting operation pairs (approval-vs-approval, approval-vs-timeout, edit-vs-approval, deactivation-vs-timeout) yield exactly one valid transition with no partial effects. **Measurement**: concurrency tests.
- **SC-011 — Timeout Correctness**: 100% of requests unresolved beyond `X` days are cancelled as `CancelledByTimeout` with reservation released and system-actor audit, idempotently. **Measurement**: timeout integration and idempotency tests.
- **SC-012 — Deactivation Correctness**: 100% of valid pre-start deactivations restore the deducted balance atomically, and 100% of post-start or partial deactivation attempts are denied. **Measurement**: deactivation integration tests.
- **SC-013 — Accrual Correctness**: 100% of completed months add exactly one whole day, with no proration and no expiry. **Measurement**: accrual domain and integration tests.
- **SC-014 — Validation Clarity**: 100% of tested validation failures identify an actionable failed rule without exposing implementation details or sensitive content. **Measurement**: MVC acceptance and security-output tests.
---

## Assumptions
### Confirmed Assumptions and Dependencies
- ASP.NET Core Identity provides authentication, application users, and the `User`/`Approver` roles, consistent with the constitution; SSO and automatic password recovery are out of scope.
- Each authenticated identity is linked to exactly one business User with an `Active`/`Inactive` status.
- The system can determine each User's completed months of service for accrual from authoritative service or employment-start information.
- Each User has exactly one global vacation balance; there is no per-leave-type balance and no static seeded entitlement as the balance source — balance results from accrual minus deductions, with Pending reservations reducing availability.
- The only active MVP leave type is `Vacation`; the domain remains parametric for future types.
- Every request requires a reason; every rejection requires a rejection reason (10–500 normalized characters).
- The MVP supports full working days only; weekends are excluded and holidays are counted as working days; there is no holiday calendar and no time-zone behavior.
- The Pending-resolution timeout `X` and session lifetime are configurable system parameters.
- User-facing language and localization follow the constitution (§3.3, Spanish by default) and are not redefined by this specification.
- MVC, Razor Views, Bootstrap, ASP.NET Core Identity configuration, persistence, and code organization are governed by the constitution and `plan.md`, not by this functional specification.
- Frontend design, visual language, UX/UI, and responsive rules for this feature are defined in the companion document `frontend-design-spec.md` in the same feature directory.

### Delivery and UX-Validation Constraint (non-normative)
- Before implementing the affected user interfaces, the team shall create visual prototypes and validate the prototypes, workflows, terminology, and usability with stakeholders. Stitch and Banani may be used as prototyping tools. This is a delivery and UX-validation constraint, not a runtime business requirement, and does not alter any EARS requirement above.

### Approved Product Decisions (2026-07-16)
- **PD-001 — Single Leave Type**: `Vacation` is the only active MVP type; the domain stays parametric and extensible. `Personal Leave` and `Medical Leave` are out of scope.
- **PD-002 — Three Roles**: The application roles are `User`, `Approver`, and `HR`. No teams, managers, delegates, escalation. Any active Approver may resolve any eligible request except their own. HR has organization-wide read access to requests, calendar, balances, audit, and approver-capability management (activate/deactivate `canResolveRequests` for existing Approvers with reason, confirmation, row version, audit) per Constitution v6.0.0 §4.3.
- **PD-003 — Global Approver Authority**: Any active Approver may resolve any eligible request; no one may resolve their own request.
- **PD-004 — Active/Inactive Status**: Users, Approvers, and HR have `Active`/`Inactive` status; an inactive Approver cannot resolve requests; an inactive HR cannot perform approver-capability management; an inactive User cannot submit or edit requests.
- **PD-005 — Global Accruing Balance**: One global balance per User; +1 whole day per completed month; no proration; no expiry; accumulates indefinitely; never negative.
- **PD-006 — Pending Reservation**: Pending requests reserve availability; available balance accounts for other Pending reservations; approval converts a reservation into a permanent deduction; rejection and timeout release it (no deduction occurred, so this is a release, not a return of deducted days).
- **PD-007 — Full-Day Calendar Rules**: Inclusive calendar dates and whole working days; Saturdays and Sundays excluded; holidays counted as working days; no holiday calendar; no time zones; earliest start is the following calendar day; zero-working-day ranges are rejected.
- **PD-008 — Overlap**: Overlap is prohibited against the same User's `Pending` or `Approved` requests; terminal requests do not block dates; adjacent ranges are valid.
- **PD-009 — Two Input Modes**: Date range, or start date plus number of working days; both normalized to one authoritative range and total server-side; client-derived values are never trusted.
- **PD-010 — Pending Editing**: Pending requests are editable with full atomic revalidation; approved-request date editing is out of scope.
- **PD-011 — Timeout**: A `Pending` request unresolved for a configurable `X` days is cancelled as `CancelledByTimeout`, releasing its reservation; the operation is idempotent and concurrency-safe; the system is the actor; no human reason is required.
- **PD-012 — Pre-Start Deactivation**: An active Approver may deactivate an `Approved` request only before its period begins, producing a distinct `CancelledByApprover` outcome and atomically restoring the deducted balance; partial deactivation and post-start deactivation are prohibited. *(Permitted under Constitution v4.0.0.)*
- **PD-013 — Rejection Reason**: Rejection requires a normalized 10–500-character reason; request reason is likewise required; these are the only narrative fields and are sensitive/redacted.
- **PD-014 — Authentication**: ASP.NET Core Identity with cookie authentication per the constitution; sessions expire and expired sessions cannot be reused; SSO and automatic password recovery are out of scope.
- **PD-015 — Calendar**: The MVP includes a basic visual vacation calendar.
- **PD-016 — Configuration Discipline**: Only explicitly approved values (timeout `X`, session lifetime) are configurable; not every business rule becomes configuration.

### Open Questions (unresolved — do not implement until answered)
- **OQ-001 — Owner cancellation of a Pending request** *(Resolved 2026-07-16)*: Deferred — a separate User-initiated cancellation of a `Pending` request is **out of MVP scope**. Constitution v4.0.0 invariant 7 explicitly defers it to a future approved specification, so no Constitution conflict remains and nothing is invented here. It may be added later by an approved specification. *(Reserved identifier: FR-008.)*
- **OQ-002 — Completed-month semantics for accrual**: The exact rule for what constitutes a "completed month" from a User's employment-start date (calendar month boundaries vs. anniversary-day boundaries, partial first month) is undefined.
- **OQ-003 — Inactive User operations**: Whether an `Inactive` User may create or edit requests is undefined; only the inactive-Approver resolution restriction is decided.
- **OQ-004 — Mandatory reason for deactivation**: Whether Approver deactivation of an approved request requires a human-entered reason is undefined.
- **OQ-005 — Calendar scope**: Whether the basic vacation calendar shows only the User's own requests or broader anonymized availability is undefined.

### Explicitly Out of MVP Scope
`Personal Leave`; `Medical Leave`; half-day requests; hourly requests; attachments; holiday calendars; time-zone handling; teams and reporting hierarchies; manager delegation; approval escalation; SSO; automatic password recovery; Google Calendar integration; Outlook integration; payroll integration; other external integrations; advanced reports; data analytics; AI simulations; advanced AI functionality; conversational bots; partial cancellation; approved-request date editing; cancellation after the approved period begins; and any additional complexity not explicitly approved for the MVP.

### Future Direction (not MVP requirements)
Additional leave types; vacation-pattern analytics; KPIs; AI-assisted reporting; conversational bots; predictive or simulation capabilities; and external calendar, payroll, or enterprise integrations may be considered in later phases but must not become MVP requirements.

### Implementation Readiness Confirmation
- [x] The `User`/`Approver`/`HR`, `Vacation`-only business model is fully specified with EARS requirements and complete acceptance-scenario traceability.
- [x] The two prior Constitution conflicts (Approver deactivation; time-zone removal) are resolved by the Constitution v4.0.0 amendment (2026-07-16); the HR role addition is resolved by the Constitution v6.0.0 amendment (2026-07-23); no unresolved Constitution conflict remains.
- [ ] Four business questions remain open (OQ-002–OQ-005) and must be resolved before building the affected areas (completed-month semantics, inactive-User operations, deactivation reason, calendar scope). OQ-001 (owner cancellation) is resolved as out of MVP scope by v4.0.0.
- [x] Every normative requirement maps to at least one acceptance scenario, edge case, or specialized test category.
- [x] The core create-resolve-reserve-accrue workflow may proceed on the recorded decisions under Constitution v6.0.0, following the constitution-defined planning and test-first workflow.

## Requirement Traceability Matrix
Every normative requirement maps to at least one acceptance scenario, edge case, or specialized test.
| Requirement | Primary Scenario(s) | Required Test Type |
|---|---|---|
| FR-001 | AC-001 | Acceptance + Integration |
| FR-002 | AC-001, AC-002 | Acceptance |
| FR-003 | AC-007 | Acceptance |
| FR-004 | AC-007, AC-044 | Acceptance + Integration |
| FR-005 | AC-045, AC-046 | Acceptance + Integration |
| FR-006 | AC-012 | Acceptance + Authorization |
| FR-007 | AC-016 | Acceptance + Integration |
| FR-011 | AC-050, AC-051 | Acceptance + Integration |
| FR-012 | AC-041, AC-042 | Acceptance |
| FR-013 | AC-048 | Integration + Concurrency |
| FR-014 | AC-054 | Domain + Integration |
| FR-015 | AC-057 | Acceptance |
| FR-009 | AC-HR-001 | Acceptance + Authorization |
| FR-016 | AC-HR-002 | Acceptance + Authorization |
| FR-017 | AC-HR-003 | Acceptance + Authorization |
| FR-018 | AC-HR-004 | Acceptance + Authorization |
| FR-019 | AC-HR-005 | Acceptance + Authorization |
| FR-020 | AC-HR-006 | Acceptance + Authorization |
| FR-021 | AC-HR-007 | Acceptance + Authorization |
| FR-022 | AC-HR-008 | Integration + Concurrency + Security |
| FR-025 | AC-058 | Integration + Authorization |
| VAL-001 | AC-002, AC-043 | Validation |
| VAL-002 | AC-001 | Validation |
| VAL-003 | AC-038 | Validation |
| VAL-004 | AC-005 | Security + Integration |
| VAL-005 | AC-002, EC-010 | Validation + Unicode Normalization |
| VAL-006 | EC-011 | Validation |
| VAL-007 | AC-016, AC-036 | Validation + Unicode Normalization |
| VAL-009 | AC-041, AC-042, EC-013 | Validation + Integration |
| BR-001 | AC-003 | Domain + Acceptance |
| BR-002 | AC-004, EC-001 | Domain + Acceptance |
| BR-003 | AC-004 | Domain |
| BR-004 | AC-035, AC-041, AC-042 | Domain |
| BR-005 | AC-001 | Domain + Integration |
| BR-006 | AC-012, AC-016, AC-048 | Domain |
| BR-007 | AC-021 | Domain |
| BR-008 | AC-012 | Domain + Integration |
| BR-009 | AC-016 | Domain + Integration |
| BR-011 | AC-021, EC-014 | Domain + Security |
| BR-012 | AC-018, AC-044 | Domain + Concurrency |
| BR-013 | AC-012 | Domain + Integration |
| BR-014 | AC-016, AC-048 | Domain |
| BR-015 | AC-012, AC-018 | Integration |
| BR-016 | AC-018 | Domain + Integration |
| BR-017 | AC-007, AC-044 | Domain + Data Model |
| BR-018 | AC-011 | Domain + Integration |
| BR-019 | AC-010, AC-040, AC-058, AC-059, EC-002 | Domain + Integration |
| BR-020 | AC-012 | Integration + Concurrency |
| BR-021 | AC-016, AC-033 | Integration |
| BR-023 | AC-016 | Domain + Integration |
| BR-024 | AC-043, EC-006 | Domain + Validation |
| BR-025 | AC-045, AC-046, AC-059 | Integration + Concurrency |
| BR-026 | AC-048 | Domain + Integration |
| BR-027 | AC-050, AC-051 | Domain + Integration |
| BR-028 | AC-052 | Domain |
| BR-029 | EC-012 | Domain |
| BR-030 | AC-001, AC-011 | Domain + Integration |
| BR-031 | AC-044, AC-011 | Domain + Data Model |
| BR-032 | AC-054 | Domain |
| BR-033 | AC-054 | Domain |
| BR-034 | AC-011 | Domain |
| BR-035 | AC-050, AC-053 | Domain + Integration |
| AUTHZ-001 | AC-007, AC-008 | Authorization |
| AUTHZ-002 | AC-012, AC-050, AC-058 | Authorization |
| AUTHZ-003 | AC-017, AC-058 | Authorization + Security |
| AUTHZ-005 | AC-008, AC-017, AC-047 | Security |
| AUTHZ-006 | AC-012, AC-020 | Authorization |
| AUTHZ-007 | AC-012, AC-018, AC-020, AC-046, AC-050 | Authorization + Integration |
| AUTHZ-008 | AC-038 | Authorization + Resilience |
| AUTHZ-009 | AC-047 | Authorization + Security |
| AUTHZ-010 | AC-047, AC-055 | Authorization |
| AUTHZ-011 | AC-HR-001, AC-HR-002, AC-HR-003, AC-HR-004, AC-HR-005, AC-HR-006 | Authorization |
| AUTHZ-012 | AC-HR-008 | Authorization + Integration |
| AUTHZ-013 | AC-HR-010 | Security |
| AUTHZ-014 | AC-HR-011 | Security |
| AUTHZ-015 | AC-HR-012 | Security |
| AUTHZ-016 | AC-HR-008 | Authorization + Security |
| SEC-001 | AC-030, AC-031 | Security |
| SEC-002 | AC-008, AC-030, AC-057 | Security |
| SEC-003 | AC-008 | Security |
| SEC-004 | AC-021, AC-051 | Security + Domain |
| SEC-005 | AC-016, AC-029 | Authorization + Privacy |
| SEC-006 | AC-029 | Redaction |
| SEC-007 | AC-032 | MVC Security |
| SEC-008 | AC-055, AC-056 | Security |
| SEC-009 | AC-HR-009 | Security Logging |
| CON-001 | AC-009, EC-016 | Concurrency + Idempotency |
| CON-002 | AC-019, AC-049 | Concurrency |
| CON-003 | AC-019, AC-053 | Concurrency + Integration |
| CON-004 | AC-020 | Concurrency |
| CON-005 | AC-012 | Concurrency + Integration |
| CON-006 | AC-012, AC-033 | Transactional Integration |
| CON-007 | AC-001, AC-037 | Transactional Integration + Failure Injection |
| CON-008 | AC-010, AC-040, AC-058, AC-059 | Concurrency + Integration |
| CON-009 | AC-048, AC-049 | Concurrency + Idempotency |
| CON-010 | AC-050, AC-053 | Transactional Integration |
| CON-011 | AC-045, AC-046, AC-059 | Transactional Integration |
| AUD-001 | AC-001, AC-009, AC-037 | Audit Integration |
| AUD-002 | AC-012, AC-016, AC-045, AC-048, AC-050 | Audit Integration |
| AUD-003 | AC-012, AC-048 | Audit Schema Verification |
| AUD-004 | AC-017, AC-031, AC-047, AC-056 | Security Logging |
| AUD-005 | Specialized audit immutability test | Security + Integration |
| AUD-006 | AC-029 | Redaction |
| AUD-007 | AC-012, AC-050 | Audit Integration |
| AUD-008 | AC-048 | Audit Schema Verification |
| AUD-009 | AC-HR-008 | Audit Integration |
| AUD-010 | AC-HR-009 | Security Logging |
| ERR-001 | AC-002, AC-003, AC-004, AC-043, AC-052 | Acceptance |
| ERR-002 | AC-018, AC-046, AC-051, AC-053 | Failure Injection + Integration |
| ERR-003 | AC-029 | Security Output |
| ERR-004 | AC-020 | Acceptance + Integration |
| ERR-005 | AC-038 | Resilience + Failure Injection |
| ERR-006 | AC-048 | Acceptance |
| CFG-001 | AC-048 | Configuration + Integration |
| CFG-002 | AC-055 | Configuration + Security |
| CFG-003 | AC-060 | Configuration + Integration |
---

## Constitution Check
This specification has been checked against `.specify/memory/constitution.md` v6.0.0.

### Conflicts Resolved by the Constitution v4.0.0 Amendment (2026-07-16)
The three items previously reported against Constitution v3.0.0 are now resolved by the v4.0.0 amendment, which adopted the Product Owner model (per §16.4, a MAJOR amendment with a Sync Impact Report):
1. **Approved-request deactivation** — v4.0.0 amends invariant 8 (`Approved` is final *except a valid pre-start deactivation*) and invariant 9 (a resolved request may undergo a bounded, audited pre-start deactivation), and adds `Approved → CancelledByApprover` to invariant 7. FR-011 / BR-027 / BR-035 now comply.
2. **Removal of time zones** — v4.0.0 amends invariant 4 to evaluate dates against a single system business date without per-user time zones (UTC storage retained). BR-003 now complies.
3. **Owner cancellation** — v4.0.0 invariant 7 explicitly defers User-initiated cancellation of a `Pending` request to a future approved specification, so the Constitution no longer mandates it. Recorded as OQ-001 (out of MVP scope); nothing invented.

### HR Role Addition via Constitution v6.0.0 Amendment (2026-07-23)
The Constitution v6.0.0 amendment (MAJOR, per §16.4 with Sync Impact Report) adds `HR` as a third combinable application role with organization-wide read access and approver-capability management. This specification has been amended to align:
- §4 (Actors) expanded to three roles with HR read-only access and prohibitions
- §5 (Lifecycle) amended to include HR active-status revalidation
- §7.1 (Auth) and §7.4 (Security Tests) amended with HR policies and test cases
- §11 (MVC/Frontend Governance) amended to reference role-based frontend specs
- §15.1 (Authority Order), §16.4 (Amendments), §17 (Risks) updated accordingly

| Constitutional Area | Result |
|---|---|
| Clean Architecture | Compliant. Behavior only; no layer/implementation prescribed. |
| MVC, Razor Views, Bootstrap | Compliant. Presentation remains a constitutional/planning concern; anti-forgery and server-authoritative validation are honored. |
| ASP.NET Core Identity & sessions | Compliant. Identity with cookie auth; configured session expiration and expired-session rejection (SEC-008, CFG-002). SSO and automatic password recovery are out of scope; Identity password policies remain governed by the constitution. |
| Roles & authorization | Compliant. Three roles (`User`, `Approver`, `HR`); deny-by-default; ownership and active-status checks; global (non-scoped) Approver authority; HR read-only with capability management per §4.3. §7.4's test cases include HR resolution/balance/role/capability violation attempts. |
| Lifecycle invariants | Compliant under v6.0.0. The revised lifecycle (`CancelledByTimeout`, `CancelledByApprover`, pre-start deactivation, Pending editing) matches amended invariants 3, 4, 6–10, and 12; HR inactive status revalidation added. |
| Balance integrity | Compliant. One global non-negative balance; monthly accrual (constitution §5.1 reserves accrual policy to the spec); Pending reservations; approval deduction; deactivation restoration; duplicate-effect protection; atomic with audit (§6). |
| Date and overlap rules | Compliant under v6.0.0 (amended invariants 3 and 4). Inclusive whole working days; weekends excluded; holidays counted; next-day minimum; zero-day rejection; overlap and adjacency defined; single business date, no time zones. |
| Security baseline | Compliant. Authentication denial, IDOR fail-closed, self-resolution denial, invalid-transition rejection, anti-forgery, redaction, session expiration, HR reason-access audit, HR capability-toggle audit are specified; server never trusts client-derived values. |
| Auditability | Compliant. Creation, editing, and every successful transition (including timeout and deactivation) and balance effects are immutably audited with the constitution's minimum fields; security events are separate; HR capability toggle audited (AUD-009, AUD-010). |
| Concurrency | Compliant. Optimistic-concurrency-style stale rejection, one-winner transitions, duplicate-effect prevention, atomic overlap-with-confirmation, idempotent timeout, HR capability optimistic concurrency (row version) are specified (§6). |
| Test-first and traceability | Compliant. Every requirement maps to a scenario or specialized test; new HR requirements (FR-009, FR-016–022, AUTHZ-011–016, SEC-009, AUD-009–010) have traceability pending acceptance scenarios. |
| Localization | Compliant. User-facing language follows the constitution (§3.3, Spanish default); the prior English-only assumption is removed. |
| Scope discipline | Compliant. Out-of-scope items are enumerated; only approved values are configurable; HR role and workflows are IN scope per Constitution v6.0.0. |
**Constitution Result**: PASS — the specification aligns with Constitution v6.0.0; the two prior lifecycle conflicts are resolved by the v4.0.0 amendment; the HR role addition is resolved by the v6.0.0 amendment; no unresolved Constitution conflict remains.
**Implementation Result**: READY WITH OPEN QUESTIONS — the core create-resolve-reserve-accrue workflow with HR read-only and capability management is fully specified and Constitution-aligned, and may enter planning and test-first design. Four product questions (OQ-002–OQ-005) should be resolved before building the areas they affect (completed-month accrual semantics, inactive-User operations, deactivation reason, calendar scope); OQ-001 is resolved as out of MVP scope.
