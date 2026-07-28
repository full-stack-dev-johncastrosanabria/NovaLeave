# NovaLeave MVP — Use Case Specification

**Version:** 2.0.0 (compact Markdown edition)  
**Language:** English; approved UI labels and routes remain in Spanish  
**Applies to:** NovaLeave Constitution v6.0.0, Feature Specification 001, Role-Based Frontend Views 002  
**Target path in repository:** `/docs/NovaLeave_Use_Cases_MVP_EN.md`

## 1. Scope

This document contains the 22 observable MVP use cases for NovaLeave. It focuses only on use-case behavior and the controls required to implement and test each flow.

### Actors

- **User:** Creates, views, and edits owned `Pending` vacation requests; views own balance, history, and calendar.
- **Approver:** When `Active` and `canResolveRequests=true`, approves, rejects, or deactivates eligible requests that they do not own.
- **HR:** Organization-wide read-only access plus limited management of `canResolveRequests` for identities that already hold the `Approver` role.
- **System:** Executes timeout cancellation and monthly accrual.

### Official request states

- `Pending`
- `Approved`
- `Rejected`
- `CancelledByTimeout`
- `CancelledByApprover`

### Official transitions

- `Pending -> Approved`
- `Pending -> Rejected`
- `Pending -> CancelledByTimeout`
- `Approved -> CancelledByApprover` before the vacation period starts

---

# 2. Shared and User Use Cases

## UC-01 — Authenticate

| Field | Value |
|---|---|
| Primary actor | User, Approver, or HR |
| Secondary actor | ASP.NET Core Identity |
| Priority | P1 |
| Type | Interaction |
| Route | `/Identity/Account/Login` |

### Objective

Establish a secure authenticated session and redirect the identity to an authorized context.

### Trigger

The person opens or submits the login screen.

### Preconditions

- The application and ASP.NET Core Identity are available.
- The identity exists and is `Active`.
- A prior session is absent, invalid, or intentionally replaced.

### Main flow

1. The system displays accessible username/email and password fields.
2. The person enters credentials. A demo-account selector may prefill the email but must not authenticate automatically.
3. The system validates antiforgery, credentials, `Active` status, lockout policy, and session state.
4. The system creates a secure authentication cookie and resolves authorized roles.
5. The system redirects to the correct initial dashboard or role context.

### Alternate and exception flows

- Invalid credentials: deny access without revealing whether the account exists and record the failure.
- `Inactive` identity: deny protected access.
- Expired or reused session: require reauthentication and record the event.
- Locked account: apply the Identity policy and show a non-sensitive message.

### Postconditions

- A valid authenticated session exists, or access is denied without information leakage.
- Credentials, secrets, and internal details are not exposed.
- Relevant authentication events are logged.

### Controls and references

- Secure `HttpOnly` cookie; `Secure` in production; appropriate `SameSite` policy.
- Antiforgery, lockout, non-enumerable messages, and structured logging.
- References: `SEC-001`, `SEC-008`, `CFG-002`, `AC-030`, `AC-031`, `AC-055`, `AC-056`, `RBFV-033`, `RBFV-018`, `RBFV-027`.

---

## UC-02 — Switch Role Context

| Field | Value |
|---|---|
| Primary actor | Identity holding at least two roles |
| Secondary actor | Authorization and navigation system |
| Priority | P2 |
| Type | Interaction |
| Entry point | Header — `Mis roles` |

### Objective

Switch visible navigation between `Mi espacio`, `Aprobaciones`, and `RRHH` without changing identity or authorization.

### Trigger

The person selects another authorized role context.

### Preconditions

- The identity is authenticated and `Active`.
- The identity holds at least two roles among `User`, `Approver`, and `HR`.

### Main flow

1. The system displays the accessible `Mis roles` selector.
2. The person selects an available context.
3. The system verifies that the selected role exists in current authoritative claims.
4. The system navigates to the root of the selected context and updates visible navigation.
5. Every subsequent resource request re-evaluates authorization by action and resource.

### Alternate and exception flows

- Missing or stale role claim: deny the context change and preserve the current context.
- Expired session: redirect to authentication.
- Single-role identity: hide the selector.

### Postconditions

- Identity, session, roles, claims, ownership, and permissions remain unchanged.
- The active visual context corresponds to an actually authorized role.

### Controls and references

- The role selector is a UI convenience, not a security control.
- References: `RBFV-009`, `RBFV-010`, `RBFV-020`, `RBFV-021`.

---

## UC-03 — View Own Vacation Requests

| Field | Value |
|---|---|
| Primary actor | User |
| Priority | P1 |
| Type | Query |
| Route | `/mis-solicitudes` |

### Objective

Display only the authenticated User's vacation requests, statuses, and authorized actions.

### Trigger

The User opens `Mis solicitudes`.

### Preconditions

- The User is authenticated and `Active`.

### Main flow

1. The system identifies the User from authoritative session data.
2. The system queries only requests whose owner matches the identity.
3. The system displays status, dates, working-day count, and permitted actions.
4. The User may open the detail of an owned request.

### Alternate and exception flows

- No requests: display an empty state and a create-request action.
- Manipulated identifier or cross-user attempt: deny access without confirming resource existence.
- Expired session or `Inactive` User: deny access.

### Postconditions

- No information belonging to another User is disclosed.
- No request, balance, or business-audit state is changed.

### Controls and references

- Server-side owner filtering, pagination, and resource authorization.
- References: `FR-003`, `AUTHZ-001`, `AUTHZ-005`, `SEC-002`, `SEC-003`, `AC-007`, `AC-008`, `RBFV-001`, `RBFV-002`, `RBFV-011`.

---

## UC-04 — Create a Vacation Request

| Field | Value |
|---|---|
| Primary actor | User |
| Secondary actors | Approver queue, VacationBalance, Audit |
| Priority | P1 |
| Type | Command |
| Route | `/mis-solicitudes/crear` |

### Objective

Create one valid `Vacation` request in `Pending`, reserve its authoritative working days, and record the operation.

### Trigger

The User submits the new-request form.

### Preconditions

- The User is authenticated and `Active`.
- Sufficient available balance exists.
- No owned `Pending` or `Approved` request overlaps the requested period.

### Main flow

1. The User selects exactly one input mode:
   - Start date + end date, or
   - Start date + number of working days.
2. The User enters the required data and a normalized reason of 10–500 characters.
3. The server normalizes the selected mode and calculates dates and Monday-to-Friday working days authoritatively.
4. The system validates the next-day minimum, range order, `Vacation` type, non-zero working days, overlap, available balance, and all untrusted client-derived values.
5. In one atomic transaction, the system creates the `Pending` request, reserves the days, creates the balance movement, and writes the audit record.
6. The request becomes visible exactly once in every eligible active Approver queue, excluding the owner.
7. The system applies Post/Redirect/Get and displays confirmation.

### Alternate and exception flows

- Missing data, mixed modes, or invalid reason: reject and preserve non-sensitive input.
- Start date today or in the past, reversed range, or weekend-only range: reject.
- Insufficient balance: reject without creating a request or reservation.
- Overlap with the same User's `Pending` or `Approved` request: reject.
- Same date range belonging to another User: permit if all other rules pass.
- Duplicate or concurrent submission: create at most one logical request.
- Audit or transaction failure: roll back the request, movement, reservation, and audit effects.
- `Inactive` User or expired session: deny the command.

### Postconditions

- The request is `Pending`.
- Days are reserved, not permanently deducted.
- Available balance remains non-negative.
- Exactly one creation audit record exists.

### Controls and references

- Server-side calculation, antiforgery, dedicated input model, idempotency, optimistic concurrency, and atomic transaction.
- References: `FR-001`, `FR-002`, `FR-012`, `FR-025`; `VAL-001`, `VAL-004`, `VAL-005`, `VAL-009`; `BR-001`, `BR-002`, `BR-004`, `BR-005`, `BR-018`, `BR-019`, `BR-024`, `BR-030`, `BR-034`; `CON-001`, `CON-007`, `CON-008`; `AUD-001`; `AC-001–005`, `AC-009–011`, `AC-035`, `AC-040–043`, `AC-058`, `AC-059`, `RBFV-028`, `RBFV-029`, `RBFV-034`.

---

## UC-05 — Edit an Owned Pending Request

| Field | Value |
|---|---|
| Primary actor | User |
| Secondary actors | VacationBalance, Audit |
| Priority | P1 |
| Type | Command |
| Route | `/mis-solicitudes/{id}/editar` |

### Objective

Edit an owned request while it remains `Pending`, recalculate its authoritative days, and adjust the reservation atomically.

### Trigger

The owner submits the edit form.

### Preconditions

- The User is authenticated and `Active`.
- The request exists, belongs to the User, and is `Pending`.
- The form contains the expected concurrency version.

### Main flow

1. The system loads current request data and presents both approved input modes.
2. The User changes dates, working-day quantity, or reason.
3. The server recalculates the range and working days.
4. The system revalidates ownership, `Active` status, state, next-day minimum, overlap, available balance, and `RowVersion`.
5. In one transaction, the system updates the request, adjusts the reservation and balance movement, and writes the edit audit record.
6. The system redirects to the detail or list with confirmation.

### Alternate and exception flows

- Request no longer `Pending`: reject with a conflict response.
- User is not the owner: deny without revealing resource existence.
- Invalid dates, overlap, or insufficient balance: preserve the previous request and reservation.
- Stale `RowVersion`: return a conflict and require refresh.
- Persistence or audit failure: roll back every change.

### Postconditions

- The request remains `Pending`.
- The reservation matches the edited authoritative working-day count.
- Exactly one successful edit audit record exists.

### Controls and references

- Owner authorization, full revalidation, optimistic concurrency, antiforgery, and atomicity.
- References: `FR-005`, `BR-025`, `CON-004`, `CON-008`, `CON-011`, `AUD-002`, `ERR-002`, `ERR-004`, `AC-045`, `AC-046`, `RBFV-028`, `RBFV-029`.

---

## UC-06 — View an Owned Request Detail

| Field | Value |
|---|---|
| Primary actor | User |
| Priority | P1 |
| Type | Query |
| Route | `/mis-solicitudes/{id}` |

### Objective

Display the complete authorized detail and visible traceability of an owned request.

### Trigger

The User opens an authorized row or calendar event.

### Preconditions

- The User is authenticated and `Active`.
- The request belongs to the User.

### Main flow

1. The system validates ownership and authorization.
2. The system loads the request detail and permitted audit events.
3. The system displays status, range, working days, reason, balance effect, and transitions.
4. The `Editar` action is displayed only while the request is `Pending`.

### Alternate and exception flows

- Foreign request or manipulated identifier: deny without confirming existence.
- Missing or inaccessible request: return a non-revealing response.
- Expired session: require authentication.

### Postconditions

- Request state and balance remain unchanged.
- Only owner-authorized data is displayed.

### Controls and references

- Resource authorization, sensitive-data redaction, and accessible navigation.
- References: `FR-003`, `FR-024`, `AUTHZ-001`, `SEC-002`, `SEC-005`, `RBFV-026`, `RBFV-001`, `RBFV-011`.

---

## UC-07 — View Own Balance and History

| Field | Value |
|---|---|
| Primary actor | User |
| Priority | P2 |
| Type | Query |
| Route | `/saldo` |

### Objective

Display the User's accrued total, pending days, used days, available balance, and balance movements.

### Trigger

The User opens `Mi historial`.

### Preconditions

- The User is authenticated and `Active`.

### Main flow

1. The system loads the User's single global vacation balance.
2. The system calculates available balance as accrued minus deducted minus reserved.
3. The system displays `Acumulado total`, `Pendientes`, `Días gozados`, and `Disponible`.
4. The system lists accrual, reservation, release, deduction, and restoration movements.

### Alternate and exception flows

- Authoritative data unavailable: fail safely and never use client values.
- Attempt to query another User's identifier: deny.

### Postconditions

- No balance is modified.
- Displayed available balance is never below zero.

### Controls and references

- Authoritative calculation, owner-only query, and sensitive-data redaction.
- References: `FR-004`, `BR-012`, `BR-017`, `BR-031`, `AC-007`, `AC-044`, `RBFV-024`, `RBFV-018`.

---

## UC-08 — View the Personal Vacation Calendar

| Field | Value |
|---|---|
| Primary actor | User |
| Priority | P3 |
| Type | Query |
| Route | `/calendario` |

### Objective

Display only the User's own `Approved` vacation periods in an accessible monthly calendar.

### Trigger

The User opens the calendar or changes month.

### Preconditions

- The User is authenticated and `Active`.

### Main flow

1. The system resolves the requested month.
2. The system queries only `Approved` periods owned by the User.
3. The calendar renders Monday through Sunday, visually distinguishes weekends, and does not create vacation events on Saturday or Sunday.
4. The calendar supports keyboard navigation and event activation.
5. Activating an event navigates to `/mis-solicitudes/{id}`.

### Alternate and exception flows

- No `Approved` periods: display an empty state.
- Unauthorized or manipulated event: omit the link and deny direct access.
- Query failure: display a safe message and retry option.

### Postconditions

- No request is modified.
- No other User's identity, availability, or requests are disclosed.

### Controls and references

- Owner filtering, resource authorization, and WCAG 2.1 AA behavior.
- References: `FR-015`, `FR-024`, `AUTHZ-001`, `AUTHZ-017`, `AUTHZ-019`, `SEC-002`, `AC-057`, `RBFV-026`, `RBFV-018`, `RBFV-019`, `RBFV-029`.

---

# 3. Approver Use Cases

## UC-09 — View the Eligible Pending Request Queue

| Field | Value |
|---|---|
| Primary actor | Approver |
| Priority | P1 |
| Type | Query |
| Route | `/aprobaciones` |

### Objective

Display all organization-wide `Pending` requests the Approver is eligible to resolve.

### Trigger

The Approver opens `Pendientes`.

### Preconditions

- The Approver is authenticated, `Active`, and has `canResolveRequests=true`.

### Main flow

1. The system validates the current role and capability.
2. The system queries organization-wide `Pending` requests.
3. The system excludes requests owned by the Approver.
4. The system displays requester, dates, working days, current balance, and projected balance.
5. The Approver may open the resolution detail.

### Alternate and exception flows

- `Inactive` Approver or capability disabled: deny access.
- No eligible requests: display an empty state.
- Newly created eligible request: display exactly once when the queue is loaded or refreshed.

### Postconditions

- No request state or balance is changed.

### Controls and references

- Global authorization without team, department, or hierarchy scope; strict owner exclusion.
- References: `FR-025`, `AUTHZ-002`, `AUTHZ-003`, `AUTHZ-009`, `AC-047`, `AC-058`, `RBFV-003`, `RBFV-034`, `RBFV-007`, `RBFV-012`, `RBFV-014`, `RBFV-025`, `RBFV-031`.

---

## UC-10 — View Request Detail for Resolution

| Field | Value |
|---|---|
| Primary actor | Approver |
| Priority | P1 |
| Type | Query |
| Route | `/aprobaciones/{id}` |

### Objective

Review an eligible request and its authoritative projected balance before resolution.

### Trigger

The Approver selects a request from the queue or an authorized calendar event.

### Preconditions

- The Approver is authenticated, `Active`, enabled, and authorized.
- The request is not owned by the Approver.

### Main flow

1. The system revalidates role, `Active`, `canResolveRequests`, non-ownership, and request state.
2. The system calculates current available balance while excluding the current request's own reservation from the approval projection.
3. The system displays `Días solicitados` and `Disponible después de aprobar`.
4. The system rechecks overlap and warns if authoritative data changed since the prior view.
5. The system displays `Aprobar` and `Rechazar` for `Pending`, or `Desactivar` for future `Approved` requests.

### Alternate and exception flows

- Negative projected balance: display a warning and prevent approval in the UI; the POST must still revalidate.
- Request already resolved or version changed: display a conflict and require refresh.
- Own request or ineligible actor: deny access.

### Postconditions

- No state or balance changes occur until a command is confirmed.

### Controls and references

- All displayed values are server-derived; UI values are informational, not authoritative.
- References: `FR-023`, `BR-036`, `BR-037`, `AUTHZ-002`, `AUTHZ-003`, `RBFV-025`, `RBFV-026`, `RBFV-030`, `RBFV-028`, `RBFV-031`.

---

## UC-11 — Approve a Pending Request

| Field | Value |
|---|---|
| Primary actor | Approver |
| Secondary actors | User owner, VacationBalance, Audit |
| Priority | P1 |
| Type | Command |
| Route | `/aprobaciones/{id}/aprobar` |

### Objective

Transition an eligible `Pending` request to `Approved` and convert its reservation into a permanent deduction exactly once.

### Trigger

The Approver confirms `Aprobar`.

### Preconditions

- Approver is authenticated, `Active`, has `canResolveRequests=true`, and is not the owner.
- Request is `Pending` with a current `RowVersion`.
- Sufficient balance exists and no prohibited overlap is present.

### Main flow

1. The system validates antiforgery.
2. The system revalidates identity, role, capability, non-ownership, state, and `RowVersion`.
3. The system recalculates working days, authoritative available balance, and projected balance.
4. The system revalidates overlap within the same operation.
5. In one transaction, the system transitions the request to `Approved`, converts the reservation into a deduction, creates the balance movement, and writes the audit record.
6. The system redirects with confirmation and refreshes the queue.

### Alternate and exception flows

- Insufficient balance or negative projection: reject and preserve `Pending`.
- Own request, `Inactive` Approver, or disabled capability: deny and record a security event.
- Stale version or concurrent resolution: exactly one operation wins; return a conflict to the loser.
- Audit or persistence failure: roll back state, balance, movement, and audit.

### Postconditions

- The request is `Approved`.
- The deduction is applied exactly once.
- Balance remains non-negative.

### Controls and references

- Antiforgery, optimistic concurrency, mutually exclusive actions, idempotency, and atomic transaction.
- References: `FR-006`; `BR-008`, `BR-013`, `BR-015`, `BR-016`, `BR-020`, `BR-037`, `BR-038`; `AUTHZ-002`, `AUTHZ-003`, `AUTHZ-007`; `CON-002`, `CON-003`, `CON-005`, `CON-006`, `CON-012`; `AUD-002`, `AUD-007`; `AC-012`, `AC-017–020`, `RBFV-014`, `RBFV-025`, `RBFV-030`, `RBFV-031`.

---

## UC-12 — Reject a Pending Request

| Field | Value |
|---|---|
| Primary actor | Approver |
| Secondary actors | User owner, VacationBalance, Audit |
| Priority | P1 |
| Type | Command |
| Route | `/aprobaciones/{id}/rechazar` |

### Objective

Transition a `Pending` request to `Rejected`, release its reservation, and record a valid rejection reason.

### Trigger

The Approver confirms `Rechazar` with a reason.

### Preconditions

- Approver is authenticated, `Active`, enabled, and not the owner.
- Request is `Pending` with a current `RowVersion`.
- Rejection reason contains 10–500 normalized characters.

### Main flow

1. The system validates antiforgery, reason, role, and eligibility.
2. The system revalidates request state, non-ownership, and `RowVersion`.
3. In one transaction, the system transitions to `Rejected`, releases the reservation, stores the normalized reason, creates the balance movement, and writes the audit record.
4. The system redirects with confirmation.

### Alternate and exception flows

- Empty or invalid-length reason: reject the command and preserve `Pending`.
- Own request or `Inactive` Approver: deny and record the event.
- Concurrent resolution: permit exactly one transition.
- Persistence or audit failure: apply no partial changes.

### Postconditions

- The request is terminal in `Rejected`.
- No permanent deduction exists.
- The reservation is released exactly once.

### Controls and references

- Normalized plain text, redaction, antiforgery, optimistic concurrency, and atomic transaction.
- References: `FR-007`, `VAL-007`, `BR-009`, `BR-014`, `BR-021`, `BR-023`, `CON-002`, `CON-012`, `AUD-002`, `AC-016`, `AC-036`, `RBFV-030`, `RBFV-028`, `RBFV-031`.

---

## UC-13 — Deactivate an Approved Request Before Its Start Date

| Field | Value |
|---|---|
| Primary actor | Approver |
| Secondary actors | User owner, VacationBalance, Audit |
| Priority | P2 |
| Type | Command |
| Route | `/aprobaciones/{id}/desactivar` |

### Objective

Cancel an entire future `Approved` request and restore the original deduction atomically.

### Trigger

The Approver confirms `Desactivar`.

### Preconditions

- Approver is authenticated, `Active`, enabled, and not the owner.
- Request is `Approved`.
- The vacation start date is after the current business date.
- The expected concurrency version is supplied.

### Main flow

1. The system revalidates role, capability, non-ownership, state, business date, and `RowVersion`.
2. The system confirms that deactivation applies to the complete request.
3. In one transaction, the system transitions to `CancelledByApprover`, restores the deducted days, creates the balance movement, and writes the audit record.
4. The system displays confirmation and the updated balance.

### Alternate and exception flows

- Vacation period already started: reject and preserve `Approved` and the deduction.
- Partial-deactivation attempt: reject.
- Own request or `Inactive` Approver: deny.
- Duplicate or concurrent command: restore the balance at most once.
- Audit or persistence failure: roll back every effect.

### Postconditions

- The request is terminal in `CancelledByApprover`.
- The original deduction is restored exactly once.

### Controls and references

- Strict temporal boundary, full-request action, antiforgery, optimistic concurrency, atomicity, and idempotency.
- No human-entered deactivation reason is mandatory in the MVP.
- References: `FR-011`, `BR-027`, `BR-028`, `BR-035`, `AUTHZ-002`, `AUTHZ-003`, `CON-003`, `CON-010`, `AUD-002`, `AUD-007`, `AC-050–053`, `RBFV-014`, `RBFV-025`, `RBFV-030`, `RBFV-031`.

---

## UC-14 — View Resolution History

| Field | Value |
|---|---|
| Primary actor | Approver |
| Priority | P3 |
| Type | Query |
| Route | `/aprobaciones/historial` |

### Objective

Display requests previously approved, rejected, or deactivated by the authenticated Approver.

### Trigger

The Approver opens `Historial`.

### Preconditions

- The Approver is authenticated and `Active`.

### Main flow

1. The system identifies the Approver.
2. The system queries resolutions attributed to that actor.
3. The system displays date, action, requester, resulting state, and an authorized detail link.
4. The Approver may filter, sort, and paginate results.

### Alternate and exception flows

- No resolutions: display an empty state.
- Unauthorized or inaccessible detail: deny access.

### Postconditions

- No business data is modified.

### Controls and references

- Actor attribution, pagination, filtering, redaction, and resource authorization.
- References: `RBFV-023`, Role-Based Frontend Views §6.2 and §8.10.

---

## UC-15 — View the Approver Calendar

| Field | Value |
|---|---|
| Primary actor | Approver |
| Priority | P3 |
| Type | Query |
| Route | `/calendario` |

### Objective

Display organization-wide `Approved` periods in anonymized form and expose detail links only when authorized.

### Trigger

The Approver opens the calendar.

### Preconditions

- The Approver is authenticated and `Active`.

### Main flow

1. The system queries `Approved` periods.
2. The system renders ranges without requester names.
3. The calendar supports accessible monthly and keyboard navigation.
4. A detail link is added only when the Approver is eligible for that resource.

### Alternate and exception flows

- Event not eligible for detail: display it anonymized without a link.
- No events in month: display an empty state.

### Postconditions

- No request is modified and no unauthorized identity data is exposed.

### Controls and references

- Anonymization, resource authorization, and accessibility.
- References: `FR-024`, `AUTHZ-017`, `AUTHZ-018`, `AUTHZ-019`, `RBFV-026`, `RBFV-018`, `RBFV-019`, `RBFV-029`, `RBFV-033`.

---

# 4. Automatic System Use Cases

## UC-16 — Cancel an Unresolved Pending Request by Timeout

| Field | Value |
|---|---|
| Primary actor | System |
| Secondary actors | User owner, VacationBalance, Audit |
| Priority | P2 |
| Type | Automatic |
| Entry point | Configurable automatic process |

### Objective

Prevent unresolved requests from reserving balance indefinitely.

### Trigger

A `Pending` request reaches at least configurable timeout `X` and the process runs.

### Preconditions

- The request is `Pending`.
- Its age satisfies configured timeout `X`.

### Main flow

1. The process selects eligible requests in bounded batches.
2. The process revalidates request state, version, and timeout boundary.
3. In one transaction, the process transitions to `CancelledByTimeout`, releases the reservation, creates the balance movement, and writes an audit record using the System actor.
4. The User can view a standard timeout explanation.

### Alternate and exception flows

- Concurrent approval or rejection: exactly one transition wins.
- Repeated process execution: apply no additional effect.
- Audit or persistence failure: commit no partial effect.

### Postconditions

- The request is terminal in `CancelledByTimeout`.
- The reservation is released exactly once.

### Controls and references

- Idempotency, optimistic concurrency, bounded queries, testable time abstraction, and atomic transaction.
- References: `FR-013`, `BR-026`, `CON-002`, `CON-009`, `AUD-002`, `AUD-008`, `CFG-001`, `ERR-006`, `AC-048`, `AC-049`, `RBFV-018`.

---

## UC-17 — Execute Monthly Vacation Accrual

| Field | Value |
|---|---|
| Primary actor | System |
| Secondary actors | User, VacationBalance, Audit |
| Priority | P2 |
| Type | Automatic |
| Entry point | Automatic process |

### Objective

Add exactly one whole day to the User's global balance for each completed month, without expiration or proration.

### Trigger

The system determines that a User completed a month according to an approved rule.

### Preconditions

- The User exists and is eligible for the completed-month event.
- Accrual for the same User and period has not already been applied.

### Main flow

1. The process identifies eligible Users.
2. The process validates idempotency for the User and accrual period.
3. The process increments accrued balance by exactly one whole day.
4. In one transaction, the process writes the balance movement and audit record.
5. The process recalculates available balance using existing deductions and reservations.

### Alternate and exception flows

- Repeated execution for the same User and period: do not duplicate accrual.
- Persistence or audit failure: apply no partial increment.
- No approved definition of “completed month”: do not invent calendar-month or employment-anniversary semantics; treat the trigger calculation as blocked.

### Postconditions

- Accrued balance increases by one day, or remains unchanged when the operation is duplicate or blocked.
- Accrued days do not expire and are not prorated.
- Available balance remains non-negative.

### Controls and references

- Idempotency key by User and period, `TimeProvider`/`IClock`, and atomic transaction.
- References: `FR-014`, `BR-032`, `BR-033`, `AC-054`, `RBFV-018`. **OQ-002 RESOLVED (2026-07-27): calendar month boundary from EmploymentStartDate**.

---

# 5. HR Use Cases

## UC-18 — View Organization-Wide Vacation Requests

| Field | Value |
|---|---|
| Primary actor | HR |
| Secondary actor | Sensitive-access audit |
| Priority | P2 |
| Type | Query |
| Routes | `/rrhh/solicitudes`, `/rrhh/solicitudes/{id}` |

### Objective

Provide organization-wide read-only access to vacation requests and their details.

### Trigger

HR opens the request list or a request detail.

### Preconditions

- HR is authenticated and `Active`.

### Main flow

1. The system validates `RequireActiveHR`.
2. The system displays a filterable, paginated organization-wide request list.
3. HR may open any request detail in read-only mode.
4. The system displays authorized audit and projected-balance information without approve, reject, or deactivate actions.
5. When HR accesses a sensitive reason, the system records the access.

### Alternate and exception flows

- `Inactive` HR: deny access.
- Direct invocation of a resolution route: return `403` and change no data.
- Sensitive data not required for the view: omit or redact it.

### Postconditions

- No request or balance is changed.
- Required sensitive-data accesses are audited.

### Controls and references

- Server-enforced read-only behavior, pagination, redaction, and PII-access auditing.
- References: `FR-009`, `FR-016`, `AUTHZ-011`, `AUTHZ-013`, `SEC-005`, `SEC-009`, `RBFV-005`, `RBFV-006`, `RBFV-016`, `RBFV-018`.

---

## UC-19 — View the Organizational Vacation Calendar

| Field | Value |
|---|---|
| Primary actor | HR |
| Priority | P3 |
| Type | Query |
| Route | `/rrhh/calendario` |

### Objective

Display all organization-wide `Approved` periods with requester identity and read-only detail navigation.

### Trigger

HR opens the calendar or changes month.

### Preconditions

- HR is authenticated and `Active`.

### Main flow

1. The system queries every `Approved` period for the requested month.
2. The system displays requester name, date range, and working-day count.
3. The calendar supports keyboard navigation and event activation.
4. Activating an event navigates to `/rrhh/solicitudes/{id}`.

### Alternate and exception flows

- No events in month: display an empty state.
- Query failure: display a safe retry message.
- `Inactive` HR: deny access.

### Postconditions

- No business data is modified.

### Controls and references

- HR authorization, accessible calendar behavior, and read-only detail.
- References: `FR-017`, `FR-024`, `AUTHZ-011`, `AUTHZ-017`, `AUTHZ-019`, `RBFV-026`, `RBFV-018`, `RBFV-019`, `RBFV-029`, `RBFV-032`.

---

## UC-20 — View Vacation Balances and Movements

| Field | Value |
|---|---|
| Primary actor | HR |
| Priority | P2 |
| Type | Query |
| Routes | `/rrhh/saldos`, `/rrhh/saldos/{userId}` |

### Objective

Display any User's balance summary and movement history without permitting modification.

### Trigger

HR opens `Saldos` or selects a User.

### Preconditions

- HR is authenticated and `Active`.

### Main flow

1. The system loads a paginated User list with accrued, pending, used, and available values.
2. HR selects a User.
3. The system displays accrual, reservation, release, deduction, and restoration movements.
4. The system exposes no edit action or generic balance-update endpoint.

### Alternate and exception flows

- Direct route or overposting attempt to modify a balance: deny and record the event.
- Missing User or unavailable authoritative data: return a safe response without internal details.

### Postconditions

- No vacation balance is modified.

### Controls and references

- Read-only authorization, pagination, dedicated query models, deny-by-default, and no generic balance update.
- References: `FR-018`, `FR-019`, `AUTHZ-011`, `AUTHZ-014`, `RBFV-007`, `RBFV-024`, `RBFV-014`, `RBFV-018`.

---

## UC-21 — View Relevant Audit Records

| Field | Value |
|---|---|
| Primary actor | HR |
| Priority | P2 |
| Type | Query |
| Route | `/rrhh/auditoria` |

### Objective

Display relevant business and security audit events with sensitive content redacted.

### Trigger

HR opens `Auditoría` and applies filters.

### Preconditions

- HR is authenticated and `Active`.

### Main flow

1. The system validates HR access.
2. The system queries events by authorized filters such as date, actor, action, entity, or result.
3. The system displays minimum traceability fields: timestamp, actor, role, action, entity, and result.
4. The system redacts complete reasons, credentials, tokens, secrets, and sensitive payloads.

### Alternate and exception flows

- Invalid filter: return actionable validation.
- `Inactive` HR: deny access.
- Unauthorized sensitive field: omit or redact it.

### Postconditions

- Audit records remain immutable.

### Controls and references

- Immutability, redaction, pagination, and HR authorization.
- References: `FR-020`, `AUTHZ-011`, `AUD-003`, `AUD-005`, `AUD-006`, `SEC-006`, `RBFV-016`, `RBFV-018`.

---

## UC-22 — Manage Approver Resolution Capability

| Field | Value |
|---|---|
| Primary actor | HR |
| Secondary actors | Target Approver, Audit |
| Priority | P2 |
| Type | Limited administrative command |
| Routes | `/rrhh/aprobadores`, `/rrhh/aprobadores/{id}/capacidad` |

### Objective

Enable or disable `canResolveRequests` only for an identity that already holds the `Approver` role, without assigning or removing roles.

### Trigger

HR confirms a capability change with a reason.

### Preconditions

- HR is authenticated and `Active`.
- The target identity already holds the `Approver` role.
- An explicit reason, confirmation, and expected `RowVersion` are provided.

### Main flow

1. The system lists identities holding `Approver` and their current `canResolveRequests` value.
2. HR selects enable or disable.
3. The system requests an explicit reason and confirmation.
4. The system revalidates active HR authorization, the target's current `Approver` role, and expected `RowVersion`.
5. In one transaction, the system changes the capability and writes an audit record containing the redacted before and after values.
6. The system displays confirmation.

### Alternate and exception flows

- Target no longer holds `Approver`: reject with a safe client error and make no change.
- Missing reason or confirmation: reject.
- Stale `RowVersion`: return `409` and make no change.
- `Inactive` HR: deny.
- Attempt to assign or remove roles: deny and record a security event.
- Audit or persistence failure: commit neither the capability change nor a partial audit record.

### Postconditions

- `canResolveRequests` reflects the new value only after a successful atomic commit.
- The target's roles remain unchanged.
- Exactly one successful audit record exists, or a relevant failure/security event is recorded.

### Controls and references

- Explicit reason, confirmation, resource authorization, role revalidation, optimistic concurrency, deny-by-default, atomic transaction, and authorization-state refresh when required.
- References: `FR-021`, `FR-022`, `AUTHZ-012`, `AUTHZ-015`, `AUTHZ-016`, `AUD-009`, `AUD-010`, `RBFV-013`, `RBFV-015`, `RBFV-016`, `RBFV-017`, `RBFV-018`.

---

# 5. HR Acceptance Criteria (AC-HR)

These acceptance criteria correspond to the HR use cases (UC-18 through UC-22) and are referenced in the MVP Feature Specification traceability matrix.

## AC-HR-001 — HR Views Organization-Wide Request List

| Field | Value |
|---|---|
| Related Use Case | UC-18 |
| Related Requirements | FR-009, AUTHZ-011, SEC-005, SEC-009, RBFV-005, RBFV-006 |
| Priority | P2 |

### Scenario
Given an active HR identity, when HR navigates to `/rrhh/solicitudes`, then the system displays a filterable, paginated list of all vacation requests across the organization with requester, dates, status, working days, reservation, and deduction.

### Acceptance Conditions
- List includes all requests regardless of owner, team, or department
- Filters: status, date range, requester name/email
- Server-side pagination (page size 10/25/50)
- No resolution actions (approve/reject/deactivate) visible or accessible
- Sensitive reasons redacted in list view; access to reason audited per SEC-009
- Inactive HR receives 403

---

## AC-HR-002 — HR Views Read-Only Request Detail

| Field | Value |
|---|---|
| Related Use Case | UC-18 (detail) |
| Related Requirements | FR-016, AUTHZ-011, SEC-005, SEC-009, RBFV-005, RBFV-006 |
| Priority | P2 |

### Scenario
Given an active HR identity and any request ID, when HR opens `/rrhh/solicitudes/{id}`, then the system displays full request detail including complete audit trail in read-only mode.

### Acceptance Conditions
- All fields visible: owner, dates, working days, reason, state, reservation, deduction, rejection reason, timeout metadata, deactivation metadata
- Full audit timeline with actor, role, action, timestamp, result
- No resolution action buttons present
- Reason and rejection reason displayed (HR-authorized access, audited per SEC-009)
- Direct invocation of resolution endpoints returns 403
- Inactive HR receives 403

---

## AC-HR-003 — HR Views Organizational Calendar

| Field | Value |
|---|---|
| Related Use Case | UC-19 |
| Related Requirements | FR-017, FR-024, AUTHZ-011, AUTHZ-017, AUTHZ-019, RBFV-026 |
| Priority | P3 |

### Scenario
Given an active HR identity, when HR opens `/rrhh/calendario`, then the system displays a month-view calendar of all approved vacation periods organization-wide with requester names and working-day counts.

### Acceptance Conditions
- All approved periods for requested month rendered
- Event label format: "Requester Name — N días"
- Multiple events per day stack with "+N más" overflow
- Keyboard navigation: arrow keys move by day/week, Home/End jump to month boundaries, PageUp/PageDown change month
- Event activation (Enter/Space) navigates to `/rrhh/solicitudes/{id}`
- Filter toolbar: department/team select (accessible label), search input (debounced 300ms)
- Weekend columns visually distinct (muted background, no events)
- `aria-live="polite"` announces month change and filter results count
- Tooltip on hover/focus: requester, date range, working days, status "Aprobada"
- Inactive HR receives 403

---

## AC-HR-004 — HR Views All User Balances Summary

| Field | Value |
|---|---|
| Related Use Case | UC-20 |
| Related Requirements | FR-018, AUTHZ-011, AUTHZ-014, RBFV-007, RBFV-024 |
| Priority | P2 |

### Scenario
Given an active HR identity, when HR opens `/rrhh/saldos`, then the system displays a paginated list of all users with balance summary: Acumulado total, Pendientes, Días gozados, Disponible.

### Acceptance Conditions
- Four summary values per user matching exact labels
- Server-side pagination (page size 10/25/50)
- Sortable by user name, any balance column
- No edit actions, no balance modification endpoints accessible
- Clicking user navigates to `/rrhh/saldos/{userId}` for movements
- Inactive HR receives 403
- Available balance never negative

---

## AC-HR-005 — HR Views Balance Movements for a User

| Field | Value |
|---|---|
| Related Use Case | UC-20 (detail) |
| Related Requirements | FR-019, AUTHZ-011, AUTHZ-014, RBFV-007, RBFV-024 |
| Priority | P2 |

### Scenario
Given an active HR identity and a target user ID, when HR opens `/rrhh/saldos/{userId}`, then the system displays a timeline of all balance movements: accruals, reservations, releases, deductions, restorations.

### Acceptance Conditions
- Timeline ordered chronologically (newest first or oldest first, configurable)
- Each entry: date, concept (Devengo/Reserva/Liberación/Deducción/Restauración), amount, resulting balance
- Request-linked movements show request ID and link to `/rrhh/solicitudes/{id}`
- No modification actions available
- Direct balance-edit endpoint returns 403
- Inactive HR receives 403

---

## AC-HR-006 — HR Views Filterable Audit Log

| Field | Value |
|---|---|
| Related Use Case | UC-21 |
| Related Requirements | FR-020, AUTHZ-011, AUD-003, AUD-005, AUD-006, SEC-006, RBFV-016 |
| Priority | P2 |

### Scenario
Given an active HR identity, when HR opens `/rrhh/auditoria` and applies filters, then the system displays relevant business and security audit events with minimum traceability fields and redacted sensitive payloads.

### Acceptance Conditions
- Filters: date range, actor, role, action, entity type, result
- Server-side pagination with accessible controls
- Columns: timestamp (UTC), actor, role, action, entity, entity ID, result
- Sensitive fields (reasons, rejection reasons, credentials, tokens, secrets) redacted in payload columns
- Sortable columns with `aria-sort` announcement
- Responsive: card fallback on mobile with timestamp + actor + action + result
- Audit records immutable — no delete/modify actions
- Inactive HR receives 403

---

## AC-HR-007 — HR Views Approver Capability List

| Field | Value |
|---|---|
| Related Use Case | UC-22 (list) |
| Related Requirements | FR-021, AUTHZ-012, AUTHZ-015, RBFV-013, RBFV-015 |
| Priority | P2 |

### Scenario
Given an active HR identity, when HR opens `/rrhh/aprobadores`, then the system displays all identities holding the `Approver` role with their `canResolveRequests` status.

### Acceptance Conditions
- Table columns: name, email, active status, `canResolveRequests` (toggle)
- Only identities with `Approver` role listed
- Inactive approvers clearly marked
- Server-side pagination with accessible controls
- Toggle opens modal (see AC-HR-008)
- Audit trail link per capability change
- Inactive HR receives 403
- No role assignment/removal actions present

---

## AC-HR-008 — HR Toggles Approver Capability

| Field | Value |
|---|---|
| Related Use Case | UC-22 (action) |
| Related Requirements | FR-022, AUTHZ-012, AUTHZ-015, AUTHZ-016, AUD-009, AUD-010, RBFV-013, RBFV-015, RBFV-016, RBFV-017 |
| Priority | P2 |

### Scenario
Given an active HR identity and a target identity with `Approver` role, when HR confirms enable/disable of `canResolveRequests` with explicit reason, confirmation, and valid row version, then the system atomically updates the capability and writes an audit record.

### Acceptance Conditions
- Modal requires: reason (10–500 chars normalized), explicit confirmation checkbox, current row version
- On success: capability updated, audit record with actor HR, target approver, prior/new value, reason, row version, timestamp
- On missing reason: 400 with validation message
- On missing confirmation: 400 with validation message
- On stale row version: 409 Conflict, no change
- On target lacking Approver role: 400 with safe message
- On inactive HR: 403
- On audit/persistence failure: atomic rollback, no partial commit
- Security event logged for: missing reason, missing confirmation, stale version, non-Approver target, inactive HR, role-assignment attempt

---

## AC-HR-009 — HR Access to Sensitive Reasons Audited

| Field | Value |
|---|---|
| Related Use Case | UC-18, UC-18 (detail), UC-21 |
| Related Requirements | SEC-009, AUD-003, AUD-006, SEC-005 |
| Priority | P2 |

### Scenario
Given an active HR identity, when HR views a request reason or rejection reason through authorized read-only use cases (UC-18, UC-21), then the system creates a structured audit event recording HR actor, accessed request, reason field accessed, and timestamp.

### Acceptance Conditions
- Audit event created for each distinct reason-field access (request reason, rejection reason)
- Event fields: timestamp_utc, actor_id, actor_role=HR, action=ReasonAccessed, entity_type=VacationRequest, entity_id, result=Success, correlation_id, request_id, accessed_field (Reason|RejectionReason)
- Reason content NOT included in audit event (redacted per AUD-006)
- Logs/traces/metrics do not contain reason content (SEC-006)
- Inactive HR: access denied, security event logged

---

## AC-HR-010 — HR Cannot Approve/Reject/Deactivate Requests

| Field | Value |
|---|---|
| Related Use Case | UC-18, UC-19, UC-20, UC-21, UC-22 |
| Related Requirements | AUTHZ-013, SEC-001, SEC-002, RBFV-006 |
| Priority | P2 (Security) |

### Scenario
Given an active HR identity, when HR attempts to invoke any request resolution endpoint (approve, reject, deactivate), then the system returns 403 Forbidden and changes no business data.

### Acceptance Conditions
- POST `/aprobaciones/{id}/aprobar` → 403
- POST `/aprobaciones/{id}/rechazar` → 403
- POST `/aprobaciones/{id}/desactivar` → 403
- Direct navigation to resolution views → 403
- Security event logged per attempt with actor, target, action, result=Denied
- UI shows no resolution action buttons in HR views

---

## AC-HR-011 — HR Cannot Modify Vacation Balances

| Field | Value |
|---|---|
| Related Use Case | UC-20, UC-20 (detail) |
| Related Requirements | AUTHZ-014, SEC-001, SEC-002, RBFV-007 |
| Priority | P2 (Security) |

### Scenario
Given an active HR identity, when HR attempts to modify any vacation balance via any endpoint, then the system returns 403/400 and changes no balance data.

### Acceptance Conditions
- No generic balance-update endpoint exposed
- Overposting attempt (extra form fields) → ignored, 400
- Direct API call to balance modification → 403
- Security event logged with actor, target user, action, result=Denied
- UI shows no edit controls on balance views

---

## AC-HR-012 — HR Cannot Assign or Remove Roles

| Field | Value |
|---|---|
| Related Use Case | UC-22 |
| Related Requirements | AUTHZ-015, SEC-001, SEC-002, RBFV-008 |
| Priority | P2 (Security) |

### Scenario
Given an active HR identity, when HR attempts to assign or remove any role (User, Approver, HR) for any identity, then the system returns 403 and makes no role changes.

### Acceptance Conditions
- No role-management endpoint exposed to HR
- Capability toggle (AC-HR-008) only affects `canResolveRequests`, not role membership
- Attempt to toggle capability for non-Approver → 400 (validated in AC-HR-008)
- Security event logged for any role-assignment/removal attempt
- UI: no role management controls in HR context

---

# 6. Explicit MVP Exclusions

The following are not part of these use cases:

- User cancellation of a `Pending` request.
- Negative balances or requests exceeding available balance.
- HR approval, rejection, or deactivation of requests.
- HR balance modification.
- HR role assignment or removal.
- Partial cancellation or post-start deactivation.
- Editing `Approved` dates.
- Additional leave types, half days, hourly leave, or holiday exclusion.
- Teams, departments, managers, delegation, hierarchy, or escalation.
- SSO, public APIs, payroll integration, or external calendar integration.

# 7. Open Question (Resolved)

- **OQ-002 — RESOLVED (2026-07-27):** Completed-month semantics for accrual defined as **calendar month** (un mes calendario sin importar el mes). A "completed month" means: for each full calendar month (e.g., January 1-31, February 1-28/29, etc.) that has fully elapsed since the User's EmploymentStartDate, add 1 day. The first partial month is not counted. Partial months at the end are not counted. Implemented in `BalanceService.AccrueOneDay()` and Monthly Accrual job.

