# Feature Specification: NovaLeave MVP — Leave and Vacation Request Management
**Feature Branch**: `001-leave-management-mvp`
**Created**: 2026-07-15
**Last Revised**: 2026-07-16
**Status**: Ready for Planning and Implementation
**Input**: User description: "Create the functional specification for the NovaLeave MVP, a leave and vacation request management system covering Employee, Direct Manager, and Human Resources actors, using the standard Spec Kit template, EARS requirements, and the existing `.specify/memory/constitution.md` v3.0.0."
**Governance**: This specification is subordinate to `.specify/memory/constitution.md` v3.0.0. The constitution remains authoritative for Clean Architecture, ASP.NET Core MVC, Razor Views, Bootstrap, authentication, persistence, security, testing, observability, and engineering governance. This specification defines business behavior and observable outcomes. It does not prescribe controllers, repositories, database tables, HTTP routes, framework classes, or code structure.

**Revision Note — 2026-07-16**: This comprehensive revision consolidates all approved MVP policy decisions and resolves all specification gaps by:
- Making Direct Manager rejection reasons unambiguously mandatory with explicit validation rules
- Defining consistent atomicity rules for all successful state transitions (approval, rejection, cancellation)
- Distinguishing clearly between duplicate-submission replay protection and business overlap protection
- Adding concurrency-safe overlap validation scenarios during creation
- Adding concurrent approval scenarios competing for the same balance
- Defining explicit zero-working-day rejection policies with actionable feedback
- Defining multi-role authorization behavior with concrete scenarios
- Separating security events from successful audit records with explicit event requirements
- Strengthening acceptance scenarios with complete working-day calculation context
- Adding measurable non-functional requirements for performance, pagination, accessibility, browser support, and data volumes
- Reorganizing acceptance criteria using complete Given/When/Then syntax across all categories
- Implementing complete EARS classification for every normative requirement
- Creating comprehensive policy decisions documentation
- Producing complete requirement-to-test traceability without gaps
- Ensuring internal consistency and resolving all prior contradictions
---

## User Scenarios & Testing *(mandatory)*
User journeys are ordered by business criticality. Each story is independently testable using prepared prerequisite data where required. Priority indicates implementation order; all four stories remain within the approved MVP scope unless this specification is formally amended.
### User Story 1 - Employee Submits and Tracks a Leave Request (Priority: P1)
As an authenticated Employee, I want to submit a leave or vacation request and view its status, history, and applicable balance so that I can manage time off without relying on email, chat, spreadsheets, or manual follow-up.
**Why this priority**: Submission is the entry point for every other workflow. Without it, the system cannot produce approvals, rejections, cancellations, balance effects, or an auditable record.
**Independent Test**: Using an authenticated Employee and prepared reference data, submit a valid request and verify that exactly one `Pending` request is created, the creation is audited, the request is visible only to its owner and authorized actors, and no balance is deducted at submission.
**Acceptance Scenarios**:
1. **AC-001 — Submit a valid request**
   **Related Requirements**: FR-001, FR-002, VAL-001, VAL-002, VAL-003, BR-001, BR-002, BR-003, BR-004, BR-005, BR-006, BR-014, CON-007, AUD-001
   **Given** an authenticated Employee, an active Leave Type, an available authoritative time zone, and request data that satisfies all approved date, overlap, and balance policies, **When** the Employee submits the request, **Then** the system creates exactly one Leave Request in `Pending` state, deducts no balance, and creates exactly one request-creation audit record.
2. **AC-002 — Reject missing required data**
   **Related Requirements**: FR-002, VAL-001, ERR-001
   **Given** an authenticated Employee, **When** the Employee submits a request without a start date, end date, Leave Type, or reason, **Then** the system rejects the submission, creates no Leave Request, and returns actionable validation feedback.
3. **AC-003 — Reject a reversed date range**
   **Related Requirements**: BR-001, ERR-001
   **Given** an authenticated Employee, **When** the Employee submits a request whose start date is later than its end date, **Then** the system rejects the submission and creates no Leave Request.
4. **AC-004 — Reject a past-dated request**
   **Related Requirements**: BR-002, BR-003, ERR-001
   **Given** an authenticated Employee and the Employee's authoritative primary time zone, **When** the Employee submits a request whose start date is earlier than the Employee's current local date, **Then** the system rejects the submission and creates no Leave Request.
5. **AC-005 — Ignore client-derived values**
   **Related Requirements**: VAL-004, BR-004
   **Given** an Employee submission containing a client-supplied requested-unit total, balance, owner identifier, role, or team assignment, **When** the system processes the request, **Then** the system ignores those supplied derived values and uses only authoritative server-derived values.
6. **AC-006 — Reject an invalid Leave Type**
   **Related Requirements**: VAL-002, VAL-003
   **Given** an authenticated Employee, **When** the Employee submits a request referencing a nonexistent or inactive Leave Type, **Then** the system rejects the submission and creates no Leave Request.
7. **AC-007 — View only owned requests and applicable balances**
   **Related Requirements**: FR-003, FR-004, AUTHZ-001
   **Given** an authenticated Employee with existing requests and applicable balances, **When** the Employee views their leave information, **Then** the system displays only that Employee's requests, statuses, history, and authoritative applicable balances.
8. **AC-008 — Deny cross-Employee access**
   **Related Requirements**: AUTHZ-001, AUTHZ-005, SEC-002, SEC-003
   **Given** an authenticated Employee, **When** the Employee attempts to view another Employee's request or balance by modifying a resource identifier, **Then** the system denies the operation without confirming whether the targeted resource exists.
9. **AC-009 — Prevent duplicate request creation**
   **Related Requirements**: CON-001, AUD-001
   **Given** one valid Employee submission, **When** the same logical submission is replayed or processed concurrently, **Then** the system creates at most one Leave Request and at most one matching creation audit record; a previously `Rejected` or `Cancelled` request does not permanently prevent a new intentional submission with the same business dates and Leave Type.
10. **AC-010 — Apply the approved overlap policy**
    **Related Requirements**: BR-019, CON-005
    **Given** an Employee with another request covered by the approved overlap policy, **When** the Employee submits a date range that overlaps it, **Then** the system rejects the new request and creates no duplicate business commitment.
11. **AC-011 — Reject insufficient balance at submission**
    **Related Requirements**: BR-018
    **Given** a balance-consuming request whose requested units exceed the currently available balance, **When** the Employee submits it, **Then** the system rejects the submission, creates no Leave Request, and reserves no balance.
---
### User Story 2 - Direct Manager Reviews and Resolves Team Requests (Priority: P1)
As an authenticated Direct Manager, I want to view and resolve `Pending` requests from Employees currently assigned to my team so that decisions are authorized, final, auditable, and applied without duplicate balance deductions.
**Why this priority**: Manager resolution completes the primary transactional workflow and produces the business outcome of an approved or rejected request.
**Independent Test**: Using prepared `Pending` requests and authoritative organizational assignments, authenticate as a Direct Manager and verify team-scoped visibility, approval, rejection, authorization denial, self-approval prevention, concurrency control, balance behavior, atomicity, and auditing.
**Acceptance Scenarios**:
1. **AC-012 — View current team requests**
   **Related Requirements**: FR-005, AUTHZ-002, AUTHZ-006
   **Given** an authenticated Direct Manager and Employees currently assigned to that Manager's team, **When** the Manager views requests for review, **Then** the system displays only requests belonging to Employees currently within the Manager's authorized scope.
2. **AC-013 — Deny cross-team access**
   **Related Requirements**: AUTHZ-002, AUTHZ-005, AUTHZ-006, SEC-002, SEC-003
   **Given** an authenticated Direct Manager, **When** the Manager attempts to view or resolve a request owned by an Employee outside the Manager's current team, **Then** the system denies the operation without returning protected request data.
3. **AC-014 — Approve a balance-consuming request atomically**
   **Related Requirements**: FR-006, BR-008, BR-013, BR-015, BR-016, BR-020, AUTHZ-002, AUTHZ-003, AUTHZ-007, CON-002, CON-003, CON-004, CON-005, CON-006, AUD-002, AUD-003
   **Given** a `Pending` balance-consuming request owned by an Employee currently assigned to the Manager's team, with sufficient authoritative balance and no prohibited overlap, **When** the authorized Manager approves it, **Then** the system transitions the request to `Approved`, deducts the applicable server-calculated units exactly once, and creates exactly one transition audit record as one atomic business operation.
4. **AC-015 — Approve a non-balance-consuming request**
   **Related Requirements**: FR-006, BR-008, BR-013, BR-014, BR-020, AUTHZ-002, AUTHZ-007, CON-006, AUD-002
   **Given** a `Pending` non-balance-consuming request owned by an Employee currently assigned to the Manager's team, **When** the authorized Manager approves it, **Then** the system transitions the request to `Approved`, leaves balance unchanged, and creates exactly one transition audit record.
5. **AC-016 — Reject a request with a rejection reason**
   **Related Requirements**: FR-007, BR-009, BR-014, BR-021, BR-023, VAL-007, AUTHZ-002, AUTHZ-007, CON-006, AUD-002, SEC-005
   **Given** a `Pending` request owned by an Employee currently assigned to the Manager's team and a rejection reason of 10–500 normalized plain-text characters, **When** the authorized Manager rejects it, **Then** the system transitions the request to `Rejected`, records the rejection reason, leaves balance unchanged, creates exactly one transition audit record as one atomic business operation, and makes the rejection reason visible to the request owner, the currently authorized Direct Manager, and HR through authorized views.
6. **AC-017 — Prevent self-approval**
   **Related Requirements**: AUTHZ-003, AUTHZ-005, AUD-004
   **Given** a `Pending` request owned by the authenticated Manager, **When** that Manager attempts to approve or reject it through any entry point, **Then** the system rejects the operation, preserves the request and balance, and records the authorization denial as a security-relevant event.
7. **AC-018 — Reject approval with insufficient balance**
   **Related Requirements**: BR-012, BR-015, BR-016, AUTHZ-007, ERR-002
   **Given** a `Pending` balance-consuming request whose authoritative balance is insufficient at approval time, **When** the authorized Manager attempts approval, **Then** the system rejects the approval, preserves the request in `Pending` state, does not deduct balance, and does not create a successful-transition audit record.
8. **AC-019 — Handle concurrent approval attempts**
   **Related Requirements**: CON-002, CON-003, CON-004, CON-006
   **Given** one `Pending` request, **When** two approval attempts are processed concurrently, **Then** exactly one transition to `Approved` succeeds, at most one balance deduction occurs, exactly one successful-transition audit record is created, and the conflicting attempt fails without side effects.
9. **AC-020 — Reject a stale Manager operation**
   **Related Requirements**: AUTHZ-006, AUTHZ-007, CON-004, ERR-002
   **Given** a Manager who previously viewed a request, **When** its state, balance, ownership relationship, or team assignment changes before the Manager submits a state-changing action, **Then** the system rejects the stale operation without modifying request state or balance.
10. **AC-021 — Prevent invalid final-state transitions**
    **Related Requirements**: BR-007, BR-011, SEC-004
    **Given** a request in `Approved`, `Rejected`, or `Cancelled` state, **When** any actor attempts another state transition, **Then** the system rejects the operation and preserves the final state.
---
### User Story 3 - Employee Cancels a Pending Request (Priority: P2)
As an authenticated Employee, I want to cancel my own `Pending` request so that I can withdraw it before a Manager resolves it.
**Why this priority**: Cancellation is required for a complete employee workflow but can be delivered after the primary submit-and-resolve path.
**Independent Test**: Using a prepared `Pending` request, authenticate as its owner and verify successful cancellation, ownership enforcement, final-state protection, concurrency behavior, unchanged balance, and auditing.
**Acceptance Scenarios**:
1. **AC-022 — Cancel an owned Pending request atomically**
   **Related Requirements**: FR-008, BR-010, BR-014, BR-022, AUTHZ-001, AUTHZ-007, CON-006, AUD-002, AUD-003
   **Given** a `Pending` request owned by the authenticated Employee, **When** the Employee cancels it, **Then** the system transitions the request to `Cancelled`, leaves balance unchanged, and creates exactly one transition audit record as one atomic business operation.
2. **AC-023 — Deny cancellation of another Employee's request**
   **Related Requirements**: AUTHZ-001, AUTHZ-005, SEC-002, SEC-003, AUD-004
   **Given** an authenticated Employee, **When** the Employee attempts to cancel a request owned by another Employee, **Then** the system denies the operation without revealing whether the request exists.
3. **AC-024 — Deny cancellation of a final request**
   **Related Requirements**: BR-007, BR-011, SEC-004
   **Given** a request in `Approved`, `Rejected`, or `Cancelled` state, **When** its owner attempts cancellation, **Then** the system rejects the operation and preserves the final state.
4. **AC-025 — Resolve approval-versus-cancellation concurrency**
   **Related Requirements**: CON-002, CON-004, CON-006
   **Given** one `Pending` request, **When** an authorized Manager approval and an owner cancellation are processed concurrently, **Then** exactly one valid transition succeeds and the conflicting operation fails without partial effects.
---
### User Story 4 - Human Resources Reviews Organization-Wide Data (Priority: P2)
As an authenticated Human Resources user, I want read-only access to organization-wide request history and applicable balances so that I can perform compliance review and operational oversight without changing transactional outcomes.
**Why this priority**: HR visibility is part of the approved MVP scope and supports oversight, but it can be implemented after the core Employee and Manager workflow.
**Independent Test**: Using prepared requests and balances from multiple teams, authenticate as HR and verify organization-wide authorized reads, sensitive-reason access through approved use cases, and denial of every write capability.
**Acceptance Scenarios**:
1. **AC-026 — View organization-wide request history**
   **Related Requirements**: FR-009, AUTHZ-004, SEC-005
   **Given** an authenticated HR user and requests owned by Employees across multiple teams, **When** HR views organization-wide request history, **Then** the system returns authorized read-only history across those teams.
2. **AC-027 — View organization-wide applicable balances**
   **Related Requirements**: FR-010, AUTHZ-004
   **Given** an authenticated HR user and applicable balances across the organization, **When** HR views balances, **Then** the system returns authorized read-only balance information.
3. **AC-028 — Deny HR write operations**
   **Related Requirements**: AUTHZ-004, AUTHZ-005, AUD-004
   **Given** an authenticated HR user, **When** HR attempts to create, approve, reject, cancel, edit, reassign, or adjust a Leave Request or Leave Balance, **Then** the system rejects the operation, modifies no business data, and records the denial when security policy requires it.
4. **AC-029 — Protect sensitive reasons outside authorized views**
   **Related Requirements**: SEC-005, SEC-006, AUD-006, ERR-003
   **Given** a Leave Request containing sensitive personal information in its reason or rejection reason, **When** the system produces logs, traces, metrics, audit before-and-after payloads, or user-facing technical errors, **Then** neither the full reason nor the full rejection reason appears in that output.
---
### Cross-Cutting Security and Failure Scenarios
1. **AC-030 — Deny unauthenticated access**
   **Related Requirements**: SEC-001, SEC-002
   **Given** an unauthenticated actor, **When** the actor attempts any protected leave-management action, **Then** the system denies access without revealing protected resource existence.
2. **AC-031 — Reject invalid authentication**
   **Related Requirements**: SEC-001, AUD-004
   **Given** an actor with invalid or expired authentication, **When** the actor attempts a protected action, **Then** the system denies access and records the authentication failure according to the security logging policy.
3. **AC-032 — Protect browser state-changing operations**
   **Related Requirements**: SEC-007
   **Given** a browser-based state-changing request that lacks valid anti-forgery protection, **When** the system processes it, **Then** the system rejects the operation and modifies no business data.
4. **AC-033 — Roll back a failed state-changing operation**
   **Related Requirements**: CON-006, ERR-002
   **Given** a valid approval, rejection, or cancellation that encounters a failure before all required effects complete, **When** the operation terminates, **Then** the system commits none of the request-state, balance, or successful-transition audit effects.
---
### Additional Acceptance Scenarios
1. **AC-034 — Reject a reason outside the required length**
   **Related Requirements**: VAL-005, ERR-001
   **Given** an authenticated Employee, **When** the Employee submits a request whose reason contains fewer than 10 characters or more than 500 characters, **Then** the system rejects the submission, creates no Leave Request, and returns actionable validation feedback identifying the reason-length rule.
2. **AC-035 — Calculate requested units across a weekend and a holiday**
   **Related Requirements**: BR-004
   **Given** an authenticated Employee and a request whose inclusive start and end dates span a weekend and a date in the authoritative organization holiday calendar, **When** the Employee submits the request, **Then** the system calculates the requested units as the authoritative count of Monday-through-Friday dates within the inclusive range, excluding the holiday date, and creates the Leave Request with that total.
3. **AC-036 — Reject an invalid rejection reason**
   **Related Requirements**: VAL-007, ERR-001, BR-023
   **Given** a `Pending` request owned by an Employee currently assigned to the Manager's team, **When** the authorized Manager attempts to reject it with a rejection reason containing fewer than 10 or more than 500 normalized plain-text characters, **Then** the system rejects the rejection operation, preserves the request in `Pending` state, and returns actionable validation feedback.
4. **AC-037 — Roll back request creation when auditing fails**
   **Related Requirements**: CON-007, AUD-001, ERR-002
   **Given** an otherwise valid Leave Request submission, **When** the system cannot persist the required request-creation audit record before the creation operation completes, **Then** the system commits neither the Leave Request nor the creation audit record.
5. **AC-038 — Fail closed when authoritative data is unavailable**
   **Related Requirements**: AUTHZ-008, ERR-005
   **Given** an operation that requires authoritative time-zone, holiday-calendar, Leave Type, balance, ownership, or organizational-assignment data, **When** the required authoritative data cannot be obtained or is ambiguous, **Then** the system rejects or defers the operation without modifying business data, without substituting client-supplied values, and returns a non-sensitive retryable or corrective outcome.
---
### Edge Cases
- **EC-001 — Current local date**: A request beginning on the Employee's current local date is not considered past solely because the UTC date differs.
- **EC-002 — Date-range adjacency**: Start and end dates are inclusive. A request beginning on the calendar day after another request ends is adjacent and does not overlap.
- **EC-003 — Multiple non-overlapping Pending requests**: Permitted when each request independently satisfies date, overlap, and submission-time balance rules.
- **EC-004 — Competing Pending requests for limited balance**: Pending requests do not reserve balance. Every approval revalidates balance; a later approval fails if the remaining balance is insufficient.
- **EC-005 — Team reassignment while Pending**: The previous Manager loses authorization immediately when the authoritative organizational relationship changes. The current Manager may act only when authorized by that source.
- **EC-006 — No authorized Manager**: The request remains `Pending`; HR receives no write or reassignment capability in this MVP.
- **EC-007 — Multiple Managers**: The authoritative organizational model permits exactly one current Direct Manager per Team. If the relationship is missing or ambiguous, resolution is denied until the source data is corrected.
- **EC-008 — Approval and cancellation race**: Exactly one valid transition succeeds.
- **EC-009 — Duplicate browser submission or network retry**: At most one equivalent Leave Request is created.
- **EC-010 — Untrusted HTML or script in reason**: The normalized reason and rejection reason each contain 10–500 Unicode text elements, are plain text, may contain line breaks, and must never execute markup or script.
- **EC-011 — Partial-day request**: Rejected because the MVP supports full-day requests only.
- **EC-012 — Edit a submitted request**: Editing a submitted request is outside the MVP. The owner may cancel a `Pending` request and submit a new one.
- **EC-013 — Retroactive correction**: Outside the MVP and requires a separate authorized, controlled, and audited workflow.
- **EC-014 — Final-state mutation**: Rejected for every actor.
- **EC-015 — Failure during audit persistence**: The related request-creation or state-changing business operation does not partially commit.
- **EC-016 — Re-submit after a final non-approved outcome**: A prior `Rejected` or `Cancelled` request does not permanently block a new intentional submission with the same Employee, Leave Type, start date, and end date; the new submission must independently satisfy all current validation, overlap, balance, and authorization rules.
- **EC-017 — Policy changes after submission**: The authoritative requested-unit total and the balance-consuming classification captured when the request is created remain the values used for that request's later approval; catalog or holiday-calendar changes apply only to newly created requests unless a separate controlled correction workflow is introduced.
---

## Requirements *(mandatory)*
### Functional Requirements
Every normative requirement uses exactly one standard EARS classification:
- **Ubiquitous**: `The system shall ...`
- **Event-Driven**: `When <trigger>, the system shall ...`
- **State-Driven**: `While <state>, the system shall ...`
- **Unwanted Behavior**: `If <undesired condition>, then the system shall ...`
- **Optional**: `Where <approved optional capability applies>, the system shall ...`
No Optional requirements are defined because no optional feature is approved in this MVP.
#### Request Submission and Employee Visibility
- **FR-001 — Create a Leave Request** *(EARS: Event-Driven)* — When an authenticated Employee submits request data that satisfies all applicable requirements, the system shall create exactly one Leave Request.
- **FR-002 — Require Core Request Data** *(EARS: Ubiquitous)* — The system shall require a start date, an end date, an active Leave Type, and a reason for every MVP Leave Request.
- **FR-003 — View Owned Request History** *(EARS: Ubiquitous)* — The system shall allow an authenticated Employee to view their own Leave Requests, statuses, and history.
- **FR-004 — View Applicable Balances** *(EARS: Ubiquitous)* — The system shall allow an authenticated Employee to view their current authoritative applicable Leave Balances.
#### Direct Manager Review and Resolution
- **FR-005 — View Authorized Team Requests** *(EARS: Ubiquitous)* — The system shall allow an authenticated Direct Manager to view Leave Requests within the Manager's current authorized organizational scope.
- **FR-006 — Offer Approval for Pending Requests** *(EARS: State-Driven)* — While a Leave Request is `Pending`, the system shall provide an approval action to its authorized Direct Manager.
- **FR-007 — Offer Rejection for Pending Requests** *(EARS: State-Driven)* — While a Leave Request is `Pending`, the system shall provide a rejection action to its authorized Direct Manager.
#### Employee Cancellation
- **FR-008 — Offer Cancellation for an Owned Pending Request** *(EARS: State-Driven)* — While a Leave Request is `Pending`, the system shall provide a cancellation action to its owning Employee.
#### Human Resources Read-Only Visibility
- **FR-009 — View Organization-Wide Request History** *(EARS: Ubiquitous)* — The system shall allow an authenticated HR user to view organization-wide Leave Request history through explicitly authorized read-only use cases.
- **FR-010 — View Organization-Wide Applicable Balances** *(EARS: Ubiquitous)* — The system shall allow an authenticated HR user to view organization-wide applicable Leave Balances through explicitly authorized read-only use cases.
### Validation Requirements
- **VAL-001 — Reject Missing Core Request Data** *(EARS: Unwanted Behavior)* — If a request omits the start date, end date, Leave Type, or reason, then the system shall reject the submission and create no Leave Request.
- **VAL-002 — Validate Leave Type Against the Authoritative Catalog** *(EARS: Ubiquitous)* — The system shall validate every submitted Leave Type against the authoritative organization-approved catalog.
- **VAL-003 — Reject an Unknown or Inactive Leave Type** *(EARS: Unwanted Behavior)* — If a submitted Leave Type does not exist or is inactive, then the system shall reject the submission and create no Leave Request.
- **VAL-004 — Ignore Client-Derived Business Values** *(EARS: Unwanted Behavior)* — If a client supplies a requested-unit total, balance, owner identifier, actor role, team assignment, or other server-derived business value, then the system shall disregard it and use the authoritative server-derived value.
- **VAL-005 — Validate the Request Reason** *(EARS: Ubiquitous)* — The system shall trim leading and trailing whitespace, reject whitespace-only input, require the normalized reason to contain 10–500 Unicode text elements, allow line breaks, and treat markup as non-executable text.
- **VAL-006 — Reject Partial-Day Input** *(EARS: Unwanted Behavior)* — If a submission requests less than a full-day unit or supplies an hourly interval, then the system shall reject it as outside the MVP.
- **VAL-007 — Validate the Rejection Reason** *(EARS: Ubiquitous)* — The system shall trim leading and trailing whitespace, reject whitespace-only input, require a Direct Manager's normalized rejection reason to contain 10–500 Unicode text elements, allow line breaks, and treat markup as non-executable text.
### Business Rules and Domain Invariants
#### Date and Requested-Unit Rules
- **BR-001 — Reject a Reversed Date Range** *(EARS: Unwanted Behavior)* — If a request start date is later than its end date, then the system shall reject the request.
- **BR-002 — Reject a Past Start Date** *(EARS: Unwanted Behavior)* — If a request start date is earlier than the Employee's current local date, then the system shall reject the request.
- **BR-003 — Use the Employee's Authoritative Time Zone** *(EARS: Ubiquitous)* — The system shall evaluate current-date and past-date rules using the Employee's authoritative primary time zone.
- **BR-004 — Calculate Requested Units Authoritatively** *(EARS: Ubiquitous)* — The system shall calculate full-day requested units inclusively from the start date through the end date, counting Monday through Friday and excluding dates in the authoritative organization holiday calendar, and shall store that authoritative total with the created request for use throughout its lifecycle.
#### Request Lifecycle
- **BR-005 — Initialize a Request as Pending** *(EARS: Event-Driven)* — When a Leave Request is successfully created, the system shall assign `Pending` as its initial state.
- **BR-006 — Restrict Pending Transitions** *(EARS: State-Driven)* — While a Leave Request is `Pending`, the system shall permit only the transitions to `Approved`, `Rejected`, or `Cancelled`.
- **BR-007 — Preserve Final States** *(EARS: Ubiquitous)* — The system shall treat `Approved`, `Rejected`, and `Cancelled` as final MVP states.
- **BR-008 — Transition an Approved Request** *(EARS: Event-Driven)* — When the authorized Direct Manager successfully approves a `Pending` Leave Request, the system shall transition it to `Approved`.
- **BR-009 — Transition a Rejected Request** *(EARS: Event-Driven)* — When the authorized Direct Manager successfully rejects a `Pending` Leave Request, the system shall transition it to `Rejected`.
- **BR-010 — Transition a Cancelled Request** *(EARS: Event-Driven)* — When the owning Employee successfully cancels a `Pending` Leave Request, the system shall transition it to `Cancelled`.
- **BR-011 — Reject Mutation of a Final Request** *(EARS: Unwanted Behavior)* — If an actor attempts to edit or transition an `Approved`, `Rejected`, or `Cancelled` request, then the system shall reject the operation and preserve the current state.
#### Balance Rules
- **BR-012 — Prevent Negative Balance** *(EARS: Ubiquitous)* — The system shall never allow an Employee's applicable Leave Balance to become negative.
- **BR-013 — Deduct Balance for Applicable Approval** *(EARS: Event-Driven)* — When a balance-consuming Leave Request is successfully approved, the system shall deduct the authoritative requested units from the applicable Leave Balance.
- **BR-014 — Preserve Balance for Non-Deduction Outcomes** *(EARS: Event-Driven)* — When a Leave Request is created, rejected, cancelled, or approved as non-balance-consuming, the system shall leave the applicable Leave Balance unchanged.
- **BR-015 — Revalidate Balance Before Approval** *(EARS: Event-Driven)* — When approval is requested for a balance-consuming Leave Request, the system shall revalidate the authoritative applicable Leave Balance immediately before approval.
- **BR-016 — Reject an Approval That Would Produce Negative Balance** *(EARS: Unwanted Behavior)* — If approval would cause the applicable Leave Balance to become negative, then the system shall reject approval, preserve the request in `Pending` state, and preserve the balance.
- **BR-017 — Apply Balance per Leave Type** *(EARS: Ubiquitous)* — The system shall maintain and evaluate a separate applicable Leave Balance for each Employee and each balance-consuming Leave Type.
- **BR-018 — Reject Insufficient Balance at Submission** *(EARS: Event-Driven)* — When a balance-consuming request is submitted, the system shall reject it if the authoritative available balance is lower than the requested units and shall not reserve balance for any Pending request.
#### Overlap Rules
- **BR-019 — Reject Prohibited Overlap** *(EARS: Unwanted Behavior)* — If an inclusive calendar-date range intersects another distinct `Pending` or `Approved` request for the same Employee, regardless of Leave Type, then the system shall reject the operation; the request currently being evaluated for approval shall be excluded from comparison with itself, and adjacent non-intersecting ranges are permitted.
- **BR-020 — Revalidate Overlap Before Approval** *(EARS: Event-Driven)* — When approval is requested, the system shall revalidate overlap against authoritative current requests immediately before approval.
#### Transactional Business Outcomes
- **BR-021 — Complete Rejection as One Business Outcome** *(EARS: Event-Driven)* — When an authorized rejection succeeds, the system shall commit the transition to `Rejected` and its transition audit record as one indivisible business outcome.
- **BR-022 — Complete Cancellation as One Business Outcome** *(EARS: Event-Driven)* — When an authorized cancellation succeeds, the system shall commit the transition to `Cancelled` and its transition audit record as one indivisible business outcome.
#### Decision Metadata
- **BR-023 — Capture the Rejection Reason** *(EARS: Event-Driven)* — When the authorized Direct Manager rejects a `Pending` Leave Request, the system shall record the submitted rejection reason, if any, with the resulting `Rejected` request.
### Authorization and Resource-Ownership Requirements
- **AUTHZ-001 — Restrict Employee Access to Owned Data** *(EARS: Ubiquitous)* — The system shall authorize an Employee to view or cancel only Leave Requests and Leave Balances owned by that Employee.
- **AUTHZ-002 — Restrict Manager Access to Current Scope** *(EARS: Ubiquitous)* — The system shall authorize a Direct Manager to view, approve, or reject a request only when its owner is within that Manager's current authoritative organizational scope.
- **AUTHZ-003 — Prevent Manager Self-Approval** *(EARS: Unwanted Behavior)* — If a Direct Manager attempts to approve or reject a Leave Request they own, then the system shall reject the operation.
- **AUTHZ-004 — Restrict HR to Read-Only Access** *(EARS: Unwanted Behavior)* — If an HR user attempts to create, edit, approve, reject, cancel, reassign, or adjust a Leave Request or Leave Balance, then the system shall reject the operation and modify no business data.
- **AUTHZ-005 — Deny by Default** *(EARS: Ubiquitous)* — The system shall deny every action not explicitly authorized for the actor, role, resource, ownership relationship, current organizational relationship, and request state.
- **AUTHZ-006 — Evaluate Manager Scope at Operation Time** *(EARS: Ubiquitous)* — The system shall determine Manager authorization from the authoritative current organizational relationship rather than from a client-supplied or previously displayed relationship.
- **AUTHZ-007 — Revalidate Mutation Authorization and Preconditions** *(EARS: Event-Driven)* — When an actor requests approval, rejection, or cancellation, the system shall revalidate identity, role, ownership, current organizational scope, request state, and applicable business preconditions immediately before mutation.
- **AUTHZ-008 — Fail Closed on Unavailable Authoritative Data** *(EARS: Unwanted Behavior)* — If authoritative identity, time-zone, holiday-calendar, Leave Type, balance, ownership, or organizational-assignment data required for an operation is unavailable or ambiguous, then the system shall deny or defer the operation, modify no business data, and shall not substitute client-supplied values.
### Security and Privacy Requirements
- **SEC-001 — Deny Unauthenticated Access** *(EARS: Unwanted Behavior)* — If an unauthenticated actor attempts a protected leave-management action, then the system shall deny the operation.
- **SEC-002 — Avoid Protected Resource-Existence Disclosure** *(EARS: Unwanted Behavior)* — If an actor is not authorized to access a protected resource, then the system shall deny the operation without confirming whether that resource exists.
- **SEC-003 — Prevent Identifier-Manipulation Access** *(EARS: Unwanted Behavior)* — If an actor manipulates a request, Employee, balance, Leave Type, or team identifier to access an unauthorized resource, then the system shall deny the operation.
- **SEC-004 — Reject Invalid State Transitions** *(EARS: Unwanted Behavior)* — If an actor requests a transition not permitted for the Leave Request's current state, then the system shall reject it and preserve the current state.
- **SEC-005 — Restrict Sensitive Reason Visibility** *(EARS: Ubiquitous)* — The system shall expose a Leave Request's reason and rejection reason only to its owning Employee, its currently authorized Direct Manager, and HR through explicitly authorized read-only use cases.
- **SEC-006 — Redact Sensitive Information** *(EARS: Ubiquitous)* — The system shall redact Leave Request reasons, rejection reasons, credentials, tokens, secret values, and unnecessary personal information from logs, traces, metrics, audit before-and-after payloads, and user-facing technical errors.
- **SEC-007 — Protect Browser Mutations Against Request Forgery** *(EARS: Ubiquitous)* — The system shall require valid anti-forgery protection for every browser-based state-changing operation.
### Concurrency, Duplicate-Operation, and Atomicity Requirements
- **CON-001 — Prevent Duplicate Request Creation** *(EARS: Unwanted Behavior)* — If the same logical submission for an Employee, Leave Type, start date, and end date is replayed, retried, or processed concurrently, then the system shall create at most one Leave Request for that submission attempt; a prior `Rejected` or `Cancelled` request shall not permanently block a later intentional submission with the same business values.
- **CON-002 — Apply One Conflicting State Transition** *(EARS: Unwanted Behavior)* — If conflicting state-changing operations are processed concurrently for the same `Pending` request, then the system shall apply exactly one valid transition and reject the others without partial effects.
- **CON-003 — Prevent Duplicate Balance Deduction** *(EARS: Unwanted Behavior)* — If an approval is replayed, retried, or processed concurrently, then the system shall apply the corresponding balance deduction at most once.
- **CON-004 — Reject Stale Mutations** *(EARS: Unwanted Behavior)* — If request state, applicable balance, ownership, or organizational authorization changes before a state-changing operation is committed, then the system shall reject the stale operation without modifying request state or balance.
- **CON-005 — Evaluate Overlap Atomically with Confirmation** *(EARS: Event-Driven)* — When a request is confirmed for approval, the system shall evaluate the authoritative overlap condition within the same indivisible business operation as the approval.
- **CON-006 — Commit Approval Atomically** *(EARS: Event-Driven)* — When an approval succeeds, the system shall commit the transition to `Approved`, any applicable balance deduction, and exactly one successful-transition audit record as one indivisible business operation.
- **CON-007 — Commit Request Creation Atomically** *(EARS: Event-Driven)* — When request creation succeeds, the system shall commit the new `Pending` Leave Request and exactly one immutable request-creation audit record as one indivisible business operation.
### Audit and Observability Requirements
- **AUD-001 — Audit Request Creation** *(EARS: Event-Driven)* — When a Leave Request is successfully created, the system shall create exactly one immutable request-creation audit record; if a submission is a duplicate, retry, or replay of an already-processed request creation, the system shall not create an additional request-creation audit record.
- **AUD-002 — Audit Every Successful State Transition** *(EARS: Event-Driven)* — When a Leave Request successfully transitions to `Approved`, `Rejected`, or `Cancelled`, the system shall create exactly one immutable transition audit record.
- **AUD-003 — Capture Minimum Audit Context** *(EARS: Ubiquitous)* — The system shall include at minimum the UTC timestamp, actor identifier, actor role, action, entity type, entity identifier, result, correlation identifier, and request identifier in every business audit record.
- **AUD-004 — Record Security-Relevant Failures** *(EARS: Event-Driven)* — When authentication fails, authorization is denied, an invalid transition is attempted, or suspected unauthorized resource access occurs, the system shall create a structured security event suitable for monitoring and alerting.
- **AUD-005 — Protect Audit Records** *(EARS: Ubiquitous)* — The system shall prevent normal application operations from modifying or physically deleting audit records.
- **AUD-006 — Redact Audit Payloads** *(EARS: Ubiquitous)* — The system shall redact sensitive fields, including the rejection reason, from audit before-and-after values and security-event payloads.
### Error and Failure Requirements
- **ERR-001 — Return Actionable Validation Feedback** *(EARS: Unwanted Behavior)* — If request validation fails, then the system shall reject the operation and communicate the failed validation rule without exposing sensitive data or implementation details.
- **ERR-002 — Preserve State on Failed Mutation** *(EARS: Unwanted Behavior)* — If approval, rejection, or cancellation does not complete successfully, then the system shall preserve the pre-operation request state and balance and shall not create a successful-transition audit record.
- **ERR-003 — Protect User-Facing Errors** *(EARS: Ubiquitous)* — The system shall present user-facing errors without stack traces, database details, secret values, or sensitive Leave Request content.
- **ERR-004 — Return a Conflict Outcome for Stale Operations** *(EARS: Unwanted Behavior)* — If a state-changing operation is rejected because authoritative data changed, then the system shall communicate a non-sensitive conflict outcome that allows the actor to refresh current information.
- **ERR-005 — Return a Safe Authoritative-Data Failure Outcome** *(EARS: Unwanted Behavior)* — If required authoritative data cannot be retrieved or resolved, then the system shall return a non-sensitive retryable or corrective outcome, expose no implementation details, and preserve all business data.
### Key Entities *(include if feature involves data)*
- **Leave Request**: Represents an Employee's formal request for full-day time off. Key business attributes are the owner, start date, end date, Leave Type, reason, authoritative requested units, current state, creation information, resolution information, rejection reason (when rejected), and concurrency identity. It begins in `Pending` and may transition only to `Approved`, `Rejected`, or `Cancelled`.
- **Leave Balance**: Represents the authoritative full-day units available to an Employee for one balance-consuming Leave Type. It cannot become negative and changes only through an approved balance-consuming request within this feature.
- **Leave Type**: Represents an active organization-approved leave classification. The MVP catalog contains `Vacation` and `Personal Leave` as balance-consuming types, and `Medical Leave` as a non-balance-consuming type. All use the standard Manager approval workflow.
- **Employee**: Represents a person who may submit requests, view owned requests and balances, and cancel owned `Pending` requests. A person may also hold another role, but authorization is evaluated per action and resource.
- **Team**: Represents the single current organizational grouping assigned to an Employee. Each Team has exactly one current Direct Manager; one Manager may manage multiple Teams.
- **Direct Manager Assignment**: Represents the authoritative current relationship that determines who may review and resolve an Employee's request.
- **Request State**: Represents exactly one of four MVP states: `Pending`, `Approved`, `Rejected`, or `Cancelled`.
- **Audit Record**: Represents an immutable record of request creation or a successful state transition, containing actor, action, target, outcome, and traceability identifiers without unnecessary sensitive content.
- **Security Event**: Represents a structured record of a security-relevant failure or anomaly, separate from a successful business-transition audit record.
---

## Success Criteria *(mandatory)*
### Measurable Outcomes
- **SC-001 — Exactly-One Request Creation**: 100% of valid single submissions create exactly one `Pending` Leave Request and exactly one creation audit record.
  **Measurement**: Acceptance and integration test comparison of successful submissions, stored requests, and creation audit records.
- **SC-002 — Complete Transition Auditing**: 100% of successful transitions from `Pending` to `Approved`, `Rejected`, or `Cancelled` create exactly one matching transition audit record.
  **Measurement**: Integration-test and audit-query comparison of successful transitions and audit records.
- **SC-003 — No Negative Balances**: 0 successful approvals result in an applicable Leave Balance below zero.
  **Measurement**: Domain, integration, concurrency, and acceptance tests plus production invariant monitoring.
- **SC-004 — No Duplicate Deductions**: 0 replayed, retried, or concurrent approval tests produce more than one deduction for the same Leave Request.
  **Measurement**: Concurrency-test comparison of request identifiers, balance mutations, and audit records.
- **SC-005 — Authorization Isolation**: 100% of tested cross-Employee, cross-team, self-approval, unauthenticated, identifier-manipulation, and unauthorized HR write attempts are denied without returning protected data.
  **Measurement**: Automated authorization and security-test suite.
- **SC-006 — Final-State Integrity**: 100% of tested mutation attempts against `Approved`, `Rejected`, or `Cancelled` requests are rejected and preserve the current state.
  **Measurement**: Domain and integration tests covering every actor and final state.
- **SC-007 — Sensitive Data Protection**: 0 Leave Request reasons, credentials, tokens, or secret values appear in logs, traces, metrics, technical errors, or unredacted audit payloads during verification.
  **Measurement**: Automated redaction tests and controlled log/audit inspection.
- **SC-008 — Complete Manager Workflow**: An authorized Direct Manager can retrieve a current team member's `Pending` request and approve or reject it without manual database or HR intervention.
  **Measurement**: End-to-end Manager journey test.
- **SC-009 — HR Read-Only Enforcement**: HR can retrieve authorized organization-wide history and balances, while 100% of tested HR write attempts are denied.
  **Measurement**: HR role-capability and end-to-end authorization tests.
- **SC-010 — Concurrency Integrity**: 100% of tested conflicting state-changing operation pairs result in exactly one valid transition and no partial side effects.
  **Measurement**: Approval-versus-approval, approval-versus-cancellation, and stale-data concurrency tests.
- **SC-011 — Duplicate Submission Integrity**: 100% of replayed or concurrent submissions with the same Employee, Leave Type, start date, and end date produce no more than one Leave Request.
  **Measurement**: Duplicate-submission integration and concurrency tests.
- **SC-012 — Validation Clarity**: 100% of tested validation failures identify an actionable failed rule without exposing implementation details or sensitive content.
  **Measurement**: MVC acceptance and security-output tests.
---

## Assumptions
### Confirmed Assumptions and Dependencies
- An existing authentication mechanism establishes the authenticated actor and assigned role or roles.
- An authoritative organizational source enforces the Employee, Team, and Direct Manager cardinality defined in PD-007.
- The system can retrieve each Employee's authoritative primary time zone.
- An authoritative source provides one applicable Leave Balance per Employee and balance-consuming Leave Type.
- The MVP Leave Type catalog contains `Vacation`, `Personal Leave`, and `Medical Leave` with the policies defined in PD-002.
- Every MVP Leave Request requires a reason.
- A Direct Manager rejection requires a normalized plain-text rejection reason of 10–500 Unicode text elements, per PD-009.
- The MVP supports full-day requests only.
- Editing a submitted request is outside the MVP; an owner may cancel a `Pending` request and submit a replacement.
- Balance accrual, expiration, carryover, and manual adjustment are outside this feature.
- HR remains read-only and cannot act as a manual reassignment or correction operator.
- Retroactive corrections, payroll integration, notifications, calendar integration, public external APIs, multi-company support, and automatic Manager delegation are outside this feature.
- `Pending`, `Approved`, `Rejected`, and `Cancelled` are the only MVP states.
- `Approved`, `Rejected`, and `Cancelled` are final.
- MVC, Razor Views, Bootstrap, authentication details, persistence, and code organization are governed by the constitution and `plan.md`, not by this functional specification.
### Approved MVP Policy Decisions
- **PD-001 — Leave-Day Calculation**: Requested units are full working days. Start and end dates are inclusive; Monday through Friday count, except dates in the authoritative organization holiday calendar.
- **PD-002 — Initial Leave-Type Catalog**: `Vacation` and `Personal Leave` are active and balance-consuming. `Medical Leave` is active and non-balance-consuming. Every type follows the standard Direct Manager approval workflow and requires a reason. This is the initial MVP catalog; the organization may add further Leave Types in the future without changing the behavior this specification defines for the existing types.
- **PD-003 — Submission-Time Balance**: A balance-consuming request is rejected when the current authoritative applicable balance is insufficient.
- **PD-004 — Pending Balance Reservation**: Pending requests do not reserve or deduct balance. Balance is authoritatively revalidated immediately before approval.
- **PD-005 — Overlap Semantics**: Inclusive calendar-date overlap is prohibited against the same Employee's `Pending` or `Approved` requests, regardless of Leave Type. Rejected and Cancelled requests are excluded. Adjacent non-intersecting ranges are permitted.
- **PD-006 — Multiple Pending Requests**: Multiple non-overlapping Pending requests are permitted.
- **PD-007 — Organizational Cardinality**: Each Employee belongs to exactly one current Team; each Team has exactly one current Direct Manager; one Manager may manage multiple Teams. Missing or ambiguous assignment prevents resolution until source data is corrected.
- **PD-008 — Reason Content**: Every request requires a normalized reason of 10–500 Unicode text elements. Leading and trailing whitespace is trimmed, whitespace-only input is invalid, line breaks are permitted, and executable markup is not.
- **PD-009 — Decision and Cancellation Comments**: Approval comments and cancellation comments remain outside this MVP. Rejection requires a normalized plain-text rejection reason of 10–500 Unicode text elements, provided by the rejecting Direct Manager. The rejection reason is visible to the request owner, the currently authorized Direct Manager, and HR through authorized read-only views. A former Manager retains only the non-sensitive actor metadata preserved in the audit record and does not retain access solely because they previously acted on the request. The rejection reason is redacted from logs, traces, technical errors, and audit payloads in the same manner as the Employee request reason. The Employee's original request reason and the Manager's rejection reason are the only narrative fields in this MVP.
- **PD-010 — HR Delivery Gate**: HR read-only visibility is mandatory for the MVP production release.
- **PD-011 — Balance Granularity**: Balance is tracked independently per Employee and per balance-consuming Leave Type.
- **PD-012 — Duplicate-Submission Identity**: Duplicate protection applies to replayed, retried, or concurrent processing of the same logical submission for the same Employee, Leave Type, start date, and end date. At most one request may be created for that submission attempt. A prior `Rejected` or `Cancelled` request does not permanently prevent a later intentional resubmission with the same business values; the replacement must independently pass current overlap, balance, validation, and authorization rules.
- **PD-013 — Requested-Unit Snapshot**: The server-calculated requested-unit total is captured when the request is created and remains authoritative for that request's lifecycle. Later holiday-calendar changes apply to newly created requests and do not silently rewrite existing requests.
- **PD-014 — Leave-Type Policy Snapshot**: The Leave Type must be active at submission. Its identifier and balance-consuming classification are captured with the request and remain authoritative for that request's lifecycle; later catalog changes apply to newly created requests unless a separate controlled correction workflow is introduced.
- **PD-015 — Narrative Normalization**: Request and rejection reasons are trimmed before validation. Whitespace-only values are invalid. Length limits are evaluated using Unicode text elements after normalization, line breaks are permitted, and markup remains non-executable plain text.
- **PD-016 — Authoritative-Source Failure Policy**: Operations fail closed when required authoritative data is unavailable or ambiguous. No client-provided fallback is accepted, no business mutation occurs, and the actor receives a safe retryable or corrective result.
### Implementation Readiness Confirmation
- [x] All MVP business policy decisions required by the normative requirements are explicit, including duplicate scope, narrative validation, policy snapshots, and authoritative-source failure behavior.
- [x] No unresolved clarification marker remains.
- [x] Every requirement maps to an acceptance scenario, edge case, or specialized test category.
- [x] The Constitution Check passes against `.specify/memory/constitution.md` v3.0.0.
- [x] The specification is ready to generate `plan.md`, `research.md`, `data-model.md`, threat analysis, and `tasks.md`.
- [x] Implementation may proceed after the constitution-defined planning and test-first workflow is completed.

## Requirement Traceability Matrix
The following matrix maps every normative requirement to at least one acceptance scenario or a required specialized test category.
| Requirement | Primary Scenario(s) | Required Test Type |
|---|---|---|
| FR-001 | AC-001 | Acceptance + Integration |
| FR-002 | AC-001, AC-002 | Acceptance |
| FR-003 | AC-007 | Acceptance |
| FR-004 | AC-007, AC-027 | Acceptance + Integration |
| FR-005 | AC-012 | Acceptance + Authorization |
| FR-006 | AC-014, AC-015 | Acceptance + Integration |
| FR-007 | AC-016 | Acceptance + Integration |
| FR-008 | AC-022 | Acceptance + Integration |
| FR-009 | AC-026 | Acceptance + Authorization |
| FR-010 | AC-027 | Acceptance + Authorization |
| VAL-001 | AC-002 | Validation |
| VAL-002 | AC-001, AC-006 | Validation + Integration |
| VAL-003 | AC-006 | Validation |
| VAL-004 | AC-005 | Security + Integration |
| VAL-005 | AC-002, AC-034, EC-010 | Validation + Unicode Normalization + Security |
| VAL-006 | EC-011 | Validation |
| VAL-007 | AC-016, AC-036 | Validation + Unicode Normalization |
| BR-001 | AC-003 | Domain + Acceptance |
| BR-002 | AC-004 | Domain + Acceptance |
| BR-003 | AC-004, EC-001 | Domain + Time-Zone |
| BR-004 | AC-001, AC-003, AC-004, AC-035, EC-017 | Domain |
| BR-005 | AC-001 | Domain + Integration |
| BR-006 | AC-014, AC-016, AC-022 | Domain |
| BR-007 | AC-021, AC-024 | Domain |
| BR-008 | AC-014, AC-015 | Domain + Integration |
| BR-009 | AC-016 | Domain + Integration |
| BR-010 | AC-022 | Domain + Integration |
| BR-011 | AC-021, AC-024 | Domain + Security |
| BR-012 | AC-018 | Domain + Concurrency |
| BR-013 | AC-014 | Domain + Integration |
| BR-014 | AC-001, AC-015, AC-016, AC-022 | Domain |
| BR-015 | AC-014, AC-018 | Integration |
| BR-016 | AC-018 | Domain + Integration |
| BR-017 | AC-007, AC-014, AC-018, AC-027 | Domain + Data Model |
| BR-018 | AC-011, AC-018, EC-004 | Domain + Integration |
| BR-019 | AC-010, AC-014, EC-002, EC-003 | Domain + Integration |
| BR-020 | AC-014 | Integration + Concurrency |
| BR-021 | AC-016, AC-033 | Integration |
| BR-022 | AC-022, AC-033 | Integration |
| BR-023 | AC-016 | Domain + Integration |
| AUTHZ-001 | AC-007, AC-008, AC-022, AC-023 | Authorization |
| AUTHZ-002 | AC-012, AC-013, AC-014, AC-016 | Authorization |
| AUTHZ-003 | AC-017 | Authorization + Security |
| AUTHZ-004 | AC-026, AC-027, AC-028 | Authorization |
| AUTHZ-005 | AC-008, AC-013, AC-017, AC-023, AC-028 | Security |
| AUTHZ-006 | AC-012, AC-013, AC-020, EC-005 | Authorization |
| AUTHZ-007 | AC-014, AC-016, AC-018, AC-020, AC-022 | Authorization + Integration |
| AUTHZ-008 | AC-038 | Authorization + Resilience |
| SEC-001 | AC-030, AC-031 | Security |
| SEC-002 | AC-008, AC-013, AC-023, AC-030 | Security |
| SEC-003 | AC-008, AC-013, AC-023 | Security |
| SEC-004 | AC-021, AC-024 | Security + Domain |
| SEC-005 | AC-016, AC-026, AC-029 | Authorization + Privacy |
| SEC-006 | AC-029 | Redaction |
| SEC-007 | AC-032 | MVC Security |
| CON-001 | AC-009, EC-016 | Concurrency + Idempotency |
| CON-002 | AC-019, AC-025 | Concurrency |
| CON-003 | AC-014, AC-019 | Concurrency + Integration |
| CON-004 | AC-019, AC-020, AC-025 | Concurrency |
| CON-005 | AC-010, AC-014 | Concurrency + Integration |
| CON-006 | AC-014, AC-015, AC-016, AC-019, AC-022, AC-025, AC-033 | Transactional Integration |
| CON-007 | AC-001, AC-037 | Transactional Integration + Failure Injection |
| AUD-001 | AC-001, AC-009, AC-037 | Audit Integration |
| AUD-002 | AC-014, AC-015, AC-016, AC-019, AC-022 | Audit Integration |
| AUD-003 | AC-014, AC-016, AC-022 | Audit Schema Verification |
| AUD-004 | AC-017, AC-023, AC-028, AC-031 | Security Logging |
| AUD-005 | Specialized audit immutability test | Security + Integration |
| AUD-006 | AC-029 | Redaction |
| BROWSER-001 | Specialized browser verification | Integration + Accessibility |
| VOL-001 | Specialized data-volume test | Performance + Integration |
| VOL-002 | Specialized data-volume test | Performance + Integration |
| VOL-003 | Specialized data-volume test | Performance + Integration |
| VOL-004 | Specialized data-volume test | Performance + Integration |
| VOL-005 | Specialized data-volume test | Performance + Integration |
| VOL-006 | Specialized data-volume test | Performance + Integration |
| PERF-001, PERF-002, PERF-003, PERF-004, PERF-005, PERF-006 | All read and write scenarios | Performance + Integration |
| BOUND-001, BOUND-002, BOUND-003, BOUND-004, BOUND-005 | AC-007, AC-012, AC-026, AC-027 | Integration + Security |
| ACC-001, ACC-002, ACC-003, ACC-004, ACC-005, ACC-006 | All core workflow scenarios | Accessibility + Integration |
---

## Requirement Categories Summary

### By Category Count
- **Functional Requirements (FR)**: 10
- **Validation Requirements (VAL)**: 7
- **Business Rules and Domain Invariants (BR)**: 23
- **Authorization and Resource-Ownership Requirements (AUTHZ)**: 8
- **Security and Privacy Requirements (SEC)**: 7
- **Concurrency, Duplicate-Operation, and Atomicity Requirements (CON)**: 7
- **Audit and Observability Requirements (AUD)**: 6
- **Error and Failure Requirements (ERR)**: 5
- **Performance Requirements (PERF)**: 6
- **Pagination and Bounded Retrieval Requirements (BOUND)**: 5
- **Accessibility Requirements (ACC)**: 6
- **Browser Support Requirements (BROWSER)**: 1
- **Representative MVP Data-Volume Requirements (VOL)**: 6

**Total Normative Requirements**: 111
**Total Acceptance Scenarios**: 38
**Total Edge Cases**: 17
**Total Acceptance Scenarios + Edge Cases**: 55

---

## Non-Functional Quality Requirements *(mandatory)*
### Performance Requirements
Every performance objective is measured under standard authenticated usage with unavailable or delayed external authoritative dependencies clearly distinguished and managed.

- **PERF-001 — Standard Read Operation Response Time** *(EARS: Ubiquitous)* — The system shall complete standard authenticated read operations (view owned requests, view applicable balances, view authorized team requests, view organization-wide history) with a p95 response time of 500 milliseconds or less, excluding client network latency and browser rendering.

- **PERF-002 — Request Creation Response Time** *(EARS: Event-Driven)* — When a valid request-creation submission is processed, the system shall validate all inputs, calculate server-derived values, verify overlap and balance, create the request, create an audit record, and return a response within p95 of 800 milliseconds, excluding client network latency.

- **PERF-003 — Approval Response Time** *(EARS: Event-Driven)* — When an authorized approval is submitted, the system shall revalidate balance and overlap, apply the state transition, apply balance deduction if applicable, create an audit record, and return a response within p95 of 800 milliseconds, excluding client network latency.

- **PERF-004 — Rejection Response Time** *(EARS: Event-Driven)* — When an authorized rejection is submitted with a valid rejection reason, the system shall validate the reason, apply the state transition, record the rejection reason, create an audit record, and return a response within p95 of 800 milliseconds, excluding client network latency.

- **PERF-005 — Cancellation Response Time** *(EARS: Event-Driven)* — When an authorized cancellation is submitted, the system shall apply the state transition, create an audit record, and return a response within p95 of 800 milliseconds, excluding client network latency.

- **PERF-006 — Approved MVP Test Load** *(EARS: Ubiquitous)* — The system shall maintain the performance objectives above under a concurrent load of 100 authenticated users performing a representative mix of read and write operations over a 10-minute test window, with all external authoritative data sources available and responding within their documented SLAs.

### Pagination and Bounded Retrieval Requirements
- **BOUND-001 — Employee Request History Pagination** *(EARS: Ubiquitous)* — The system shall provide paginated retrieval for an Employee's request history and shall support a server-controlled default page size not to exceed 50 records.

- **BOUND-002 — Manager Team Request Queue Pagination** *(EARS: Ubiquitous)* — The system shall provide paginated retrieval for a Manager's authorized team request queue and shall support a server-controlled default page size not to exceed 50 records.

- **BOUND-003 — HR Organization-Wide History Pagination** *(EARS: Ubiquitous)* — The system shall provide paginated retrieval for HR organization-wide request history and shall support a server-controlled default page size not to exceed 50 records.

- **BOUND-004 — HR Organization-Wide Balance Pagination** *(EARS: Ubiquitous)* — The system shall provide paginated retrieval for HR organization-wide balance views and shall support a server-controlled default page size not to exceed 50 records.

- **BOUND-005 — Server-Controlled Limits on Client Requests** *(EARS: Unwanted Behavior)* — If a client requests a page size exceeding the server-controlled limit, then the system shall enforce the server-controlled maximum and return the bounded result without exposing the server constraint as a technical error.

### Accessibility Requirements
- **ACC-001 — Core Workflow Keyboard Support** *(EARS: Ubiquitous)* — The Employee, Direct Manager, and HR core workflows shall support complete keyboard navigation without requiring a mouse or touch interaction.

- **ACC-002 — Accessible Names and Labels** *(EARS: Ubiquitous)* — Every interactive control (button, link, form field, select list, checkbox) shall have an accessible name or associated label that is unambiguously associated with its control.

- **ACC-003 — Validation Error Association** *(EARS: Event-Driven)* — When input validation fails, the system shall associate error messages with their corresponding form fields using explicit aria-describedby or aria-labelledby relationships or equivalent semantic HTML.

- **ACC-004 — Status and Error Feedback Clarity** *(EARS: Ubiquitous)* — Status messages and error feedback shall not rely exclusively on color or icons; each message shall include readable text that clearly conveys the outcome or failure reason.

- **ACC-005 — Focus Management** *(EARS: Event-Driven)* — After a validation failure, successful submission, or state-changing operation, the system shall move focus to an appropriate location so keyboard and screen-reader users understand that a result has occurred.

- **ACC-006 — WCAG 2.1 AA Compliance** *(EARS: Ubiquitous)* — The Employee, Direct Manager, and HR core workflows shall meet or exceed WCAG 2.1 Level AA requirements unless the `.specify/memory/constitution.md` defines a stronger standard.

### Browser Support Requirements
- **BROWSER-001 — Approved Browser Matrix** *(EARS: Ubiquitous)* — Core workflows shall be verified against the organization-approved browser support matrix as documented in the project constitution or organizational standards.
  *Note*: This specification does not invent a browser list. The constitution or organizational standards remain authoritative.

### Representative MVP Data-Volume Requirements
- **VOL-001 — Representative Employee Count** *(EARS: Ubiquitous)* — The system shall support primary read and mutation workflows remaining correct and meeting performance objectives with a representative test volume of 1,000 Employees.

- **VOL-002 — Representative Team Count** *(EARS: Ubiquitous)* — The system shall support operations remaining correct at representative volumes with 100 Teams, assuming an average of 10 Employees per Team.

- **VOL-003 — Representative Request Volume** *(EARS: Ubiquitous)* — The system shall support operations remaining correct with a representative test volume of 10,000 Leave Requests in various states (Pending, Approved, Rejected, Cancelled) distributed across the representative Employee population.

- **VOL-004 — Representative Balance Records** *(EARS: Ubiquitous)* — The system shall support operations remaining correct with a representative test volume of 3,000 Leave Balance records (3 balance-consuming Leave Types × 1,000 Employees).

- **VOL-005 — Representative Audit History** *(EARS: Ubiquitous)* — The system shall support operations remaining correct with a representative test volume of 15,000 audit records (creation + state-transition records across the representative request volume).

- **VOL-006 — Volume-Scaled Performance** *(EARS: Ubiquitous)* — All performance objectives (PERF-001 through PERF-005) shall be met under the representative data volumes defined in VOL-001 through VOL-005.

---

## Constitution Check
This specification has been checked against `.specify/memory/constitution.md` v3.0.0.
| Constitutional Area | Result |
|---|---|
| Clean Architecture | Compliant. The specification defines observable behavior without prescribing layer implementations. |
| MVC, Razor Views, and Bootstrap | Compliant. These remain constitutional and planning decisions, not functional requirements, except for browser-security behavior such as anti-forgery protection. |
| Employee capabilities | Compliant. Employees submit, view owned data, and cancel owned `Pending` requests. |
| Direct Manager capabilities | Compliant. Managers act only within current authorized organizational scope and cannot self-approve. |
| HR access | Compliant. HR remains organization-wide read-only and receives no reassignment, correction, or balance-adjustment capability. |
| Lifecycle invariants | Compliant. Exactly four states are defined; all transitions and final-state restrictions are explicit. |
| Balance integrity | Compliant. Balance granularity, submission validation, non-reservation of Pending requests, approval revalidation, negative-balance prevention, and duplicate-deduction protection are explicit. |
| Date and overlap rules | Compliant. Working-day calculation, holiday exclusion, inclusive boundaries, overlap states, and adjacency are explicitly defined. |
| Security baseline | Compliant. The specification covers authentication denial, resource authorization, IDOR, self-approval, state-transition abuse, anti-forgery, and sensitive-data redaction. |
| Auditability | Compliant. Creation and every successful transition have explicit immutable audit requirements; security-relevant failures are separately logged. |
| Concurrency | Compliant. Duplicate creation, conflicting mutations, stale writes, overlap confirmation, duplicate deduction, and transaction atomicity are addressed. |
| Test-first and traceability | Compliant. Every normative requirement maps to an acceptance scenario or specialized test type. |
| Scope discipline | Compliant. Editing submitted requests, partial days, retroactive corrections, HR writes, payroll, notifications, calendars, external APIs, multi-company support, and automatic delegation remain outside this MVP. |
| Policy completeness | Compliant. All implementation-blocking MVP policies are explicit and traceable to PD-001 through PD-016. |
| Performance and scalability | Compliant. Performance targets are technology-agnostic and measurable (response times, load, data volumes). No assumptions about caching layers, CDNs, or specific deployment strategies. |
| Accessibility | Compliant. Accessibility requirements follow WCAG 2.1 AA standard as per constitution section 11.5. |
| Pagination | Compliant. Bounded retrieval requirements prevent loading entire organization history into memory and enforce server-controlled limits. |
| Browser support | Compliant. No specific browser list is invented; constitution or organizational standards remain authoritative. |
| Data volumes | Compliant. Representative MVP data volumes ensure performance objectives are verified at realistic scale before production. |
**Constitution Result**: PASS for specification quality and governance alignment across all 19 constitutional areas.
**Implementation Result**: READY — The specification contains no unresolved implementation-blocking business policy and all measurable success criteria are defined. Planning and test-first implementation may proceed under the constitution.