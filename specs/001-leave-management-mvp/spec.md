# Feature Specification: NovaLeave — Leave Management MVP

**Feature Branch**: `001-leave-management-mvp`

**Created**: 2026-07-13

**Status**: Draft

**Input**: User description: "I want to build an enterprise-grade Leave Management System called NovaLeave. NovaLeave is a web application that allows organizations to manage employee leave requests through a structured approval workflow. The goal is to replace manual email and spreadsheet processes with a centralized application where employees, managers, and Human Resources can efficiently manage leave requests while enforcing business rules and maintaining a complete audit trail. Primary actors: Employee, Manager, Human Resources Administrator. [Full capability list, business rules, lifecycle, leave types, and future roadmap as provided by the user.]"

## Business Goals

- Replace manual, error-prone email- and spreadsheet-based leave tracking with a single centralized system of record.
- Give employees self-service visibility into their leave balance, request history, and request status.
- Give managers a fast, structured way to review and decide on their direct reports' leave requests.
- Give HR Administrators full organization-wide oversight, search, auditability, and reporting over leave activity.
- Guarantee that leave-related business rules (balance integrity, no overlapping requests, immutability of decisions, complete audit trail) are enforced consistently, not dependent on individual diligence.
- Establish an extensible foundation that can grow into future capabilities (notifications, calendar integration, a public API, Microsoft Entra ID authentication, multi-company support, Azure deployment, Power BI reporting) without a redesign.

## User Personas

- **Employee ("Alex")** — An individual contributor who wants to check their remaining leave balance and submit a request in minutes, without emailing HR or waiting on a spreadsheet update.
- **Manager ("Jordan")** — A people manager who needs a fast, reliable way to see which of their direct reports have pending requests and to approve or reject them with context, without disrupting team coverage.
- **HR Administrator ("Morgan")** — The person accountable for organization-wide leave policy, data accuracy, and compliance; needs to see every request, search history, audit every decision, and produce reports on demand.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Employee Submits a Leave Request (Priority: P1)

An employee checks their available leave balance and submits a new leave request by choosing a leave type, start date, end date, and an optional reason, so their time off is officially recorded and routed for approval instead of being tracked by email.

**Why this priority**: This is the core value proposition of NovaLeave — it directly replaces the manual process the system is built to eliminate. Without this, there is no product.

**Independent Test**: Can be fully tested by signing in as an employee, submitting a valid leave request, and confirming it appears in the employee's request history with status "Pending." Delivers value on its own even before any manager reviews it, because the request is now centrally recorded.

**Acceptance Scenarios**:

1. **Given** an employee with a positive Vacation balance, **When** they submit a request for 3 days of Vacation within their available balance, with a valid future start and end date, **Then** the request is created with status "Pending" and is visible in their request history.
2. **Given** an employee viewing the request form, **When** they leave the reason field blank, **Then** the request is still accepted, because the reason is optional.
3. **Given** an employee with an existing Approved or Pending request for August 10–12, **When** they attempt to submit a new request that overlaps any of those dates, **Then** the system rejects the submission and explains that it overlaps an existing request.
4. **Given** an employee submitting a request, **When** the selected start date is in the past, **Then** the system rejects the submission and explains that requests cannot start in the past.
5. **Given** an employee submitting a request, **When** the selected end date is earlier than the start date, **Then** the system rejects the submission and explains the date ordering error.
6. **Given** an employee whose available balance for the selected leave type is less than the requested duration, **When** they submit the request, **Then** the system rejects the submission and explains the balance shortfall.

---

### User Story 2 - Manager Reviews and Decides on Direct Reports' Requests (Priority: P2)

A manager views the leave requests submitted by their direct reports and approves or rejects each pending request, optionally adding a comment to explain the decision, so employees get a timely answer and team coverage stays intact.

**Why this priority**: Approval is the second half of the core workflow — a request that can never be decided on delivers no value. This is the first priority after submission because it is the next mandatory step in the lifecycle.

**Independent Test**: Can be fully tested by signing in as a manager with at least one direct report who has a pending request, approving one request and rejecting another with a comment, and confirming both decisions are reflected in the requests' status and history.

**Acceptance Scenarios**:

1. **Given** a manager viewing their team's requests, **When** they open the list, **Then** they see only requests submitted by their own direct reports, not other employees.
2. **Given** a pending request from a direct report, **When** the manager approves it, **Then** the request status changes to "Approved," the deciding manager and timestamp are recorded, and the employee's leave balance is reduced accordingly.
3. **Given** a pending request from a direct report, **When** the manager rejects it with a comment, **Then** the request status changes to "Rejected," the comment is stored and visible to the employee, and no balance is deducted.
4. **Given** a manager who has their own pending leave request, **When** they view their team's request list, **Then** their own request does not appear as something they can approve or reject.
5. **Given** a request that has already been Approved, **When** anyone attempts to change its dates or leave type, **Then** the system prevents the modification because approved requests are immutable.

---

### User Story 3 - Employee Tracks and Cancels Requests (Priority: P3)

An employee views the history and current status of all their past and pending leave requests, and cancels a request that is still pending if their plans change.

**Why this priority**: This closes the loop for the employee experience and handles the common real-world case of changed plans, but the system already delivers its core value (Stories 1–2) without it.

**Independent Test**: Can be fully tested by signing in as an employee with a mix of pending, approved, and rejected requests, confirming all are visible with correct status, and cancelling a pending request to confirm it moves to "Cancelled" and is excluded from balance calculations.

**Acceptance Scenarios**:

1. **Given** an employee with requests in multiple statuses, **When** they view their request history, **Then** every request is listed with its leave type, dates, current status, and any manager comment.
2. **Given** a Pending request, **When** the employee cancels it, **Then** its status changes to "Cancelled" and it can never return to "Pending."
3. **Given** an Approved request, **When** the employee attempts to cancel it, **Then** the system prevents the cancellation, consistent with approved requests being immutable in this release.

---

### User Story 4 - HR Administrator Oversees All Leave Activity (Priority: P4)

An HR Administrator views every leave request across the organization, searches historical requests by employee, leave type, status, or date range, views any employee's leave balance, and reviews the complete audit trail behind any approval decision.

**Why this priority**: HR oversight and auditability are essential for an enterprise system of record and for compliance, but the day-to-day submit/approve workflow (Stories 1–2) can function and deliver value without HR actively using every capability from day one.

**Independent Test**: Can be fully tested by signing in as an HR Administrator, searching for a specific employee's historical requests across departments, and viewing the full status-change audit trail for one request.

**Acceptance Scenarios**:

1. **Given** an HR Administrator, **When** they open the request list, **Then** they see requests from every employee in the organization, regardless of reporting line.
2. **Given** an HR Administrator, **When** they search by employee name, leave type, status, and a date range, **Then** only matching historical requests are returned.
3. **Given** any leave request, **When** an HR Administrator opens its audit trail, **Then** they see every status transition with the acting user and timestamp, from submission through to its current state.
4. **Given** an HR Administrator, **When** they view an employee's profile, **Then** they see that employee's current leave balance for every leave type.

---

### User Story 5 - HR Administrator Manages Leave Types and Reports (Priority: P5)

An HR Administrator configures the organization's leave types and generates reports summarizing leave usage across employees, departments, leave types, and time periods.

**Why this priority**: Configuration and reporting are administrative capabilities that make the system manageable long-term, but the first four launch-critical stories can operate using the pre-configured leave types without this story being complete.

**Independent Test**: Can be fully tested by signing in as an HR Administrator, editing an existing leave type, and generating a usage report for a specific date range, confirming the output reflects actual request data.

**Acceptance Scenarios**:

1. **Given** an HR Administrator, **When** they view the list of leave types, **Then** they see Vacation, Personal Leave, Medical Leave, and Unpaid Leave, pre-configured and ready to use.
2. **Given** an HR Administrator, **When** they add a new leave type or edit an existing one, **Then** the change is reflected the next time any user selects a leave type.
3. **Given** an HR Administrator, **When** they generate a leave usage report for a chosen date range, **Then** the report summarizes requests by employee, leave type, and status within that range.

---

### Edge Cases

- What happens when an employee submits a request that exactly abuts (but does not overlap) an existing approved request (e.g., existing request ends the day before the new one starts)? The system MUST allow it, since the ranges do not overlap.
- What happens when a manager who is also an HR Administrator views a request from their own direct report? Their manager-approval restriction on their own requests still applies; their HR-level read access to all requests is unaffected.
- What happens when an employee has no manager assigned? The request MUST be routed to HR for decision, since every submitted request must have a valid decision path.
- What happens when a leave type is deactivated by HR while an employee has a pending request of that type? The existing request MUST remain valid and decidable; only new requests are prevented from using the deactivated type.
- What happens when two requests for the same employee are submitted for overlapping dates in rapid succession? The system MUST reject the second one at submission time, evaluated against all existing Pending and Approved requests.
- What happens when a leave request would reduce the balance to exactly zero? The system MUST allow it; only requests that would take the balance below zero are rejected.
- What happens when a manager attempts to approve a request that a colleague manager already rejected? The system MUST prevent further decisions on a request once it has left the Pending status.
- How does the system handle a request that spans a leave type with no remaining balance and a leave type with unlimited balance (e.g., Unpaid Leave)? Unpaid Leave requests are not constrained by a balance check.

## Business Rules

- **BR-001**: An employee's leave balance for a given leave type can never become negative as a result of an approved request.
- **BR-002**: A leave request cannot overlap the date range of any existing Pending or Approved request belonging to the same employee.
- **BR-003**: A leave request's start date cannot be in the past at the time of submission.
- **BR-004**: A leave request's start date must not be after its end date.
- **BR-005**: Once a request is Approved, its dates, leave type, and reason cannot be modified.
- **BR-006**: Once a request is Cancelled, it can never return to Pending.
- **BR-007**: Every change to a request's status must be recorded with the acting user and a timestamp, forming a permanent audit trail.
- **BR-008**: A manager cannot approve or reject their own leave request.
- **BR-009**: HR Administrators have read access to every leave request in the organization, regardless of reporting line.
- **BR-010**: All leave request dates and audit timestamps are recorded and reasoned about in UTC, so that decisions and reports are consistent regardless of the time zone of the person viewing them.

## Requirements *(mandatory)*

### Functional Requirements

**Authentication & Access**

- **FR-001**: System MUST allow employees, managers, and HR Administrators to sign in securely before accessing any leave data.
- **FR-002**: System MUST restrict each user's visibility of leave requests according to their role: employees see only their own requests, managers additionally see their direct reports' requests, and HR Administrators see every request in the organization.

**Employee Capabilities**

- **FR-003**: System MUST allow an employee to view their current available leave balance, broken down by leave type.
- **FR-004**: System MUST allow an employee to submit a leave request specifying a leave type, start date, end date, and an optional reason.
- **FR-005**: System MUST validate every submitted leave request against Business Rules BR-001 through BR-004 and reject any request that violates one, with an explanation of which rule was violated.
- **FR-006**: System MUST allow an employee to view the history and current status of every leave request they have submitted.
- **FR-007**: System MUST allow an employee to cancel a leave request that is currently Pending.
- **FR-008**: System MUST prevent an employee from modifying or cancelling a request once it is Approved or Rejected.

**Manager Capabilities**

- **FR-009**: System MUST allow a manager to view all leave requests, pending and historical, submitted by their direct reports.
- **FR-010**: System MUST allow a manager to approve or reject a Pending request submitted by one of their direct reports.
- **FR-011**: System MUST allow a manager to attach a comment when approving or rejecting a request, and that comment MUST be visible to the requesting employee.
- **FR-012**: System MUST prevent a manager from approving or rejecting their own leave request (BR-008), regardless of who else could act on it.

**HR Administrator Capabilities**

- **FR-013**: System MUST allow an HR Administrator to view every leave request across the organization, regardless of reporting line (BR-009).
- **FR-014**: System MUST allow an HR Administrator to search historical leave requests by employee, leave type, status, and date range.
- **FR-015**: System MUST allow an HR Administrator to view the leave balance of any employee.
- **FR-016**: System MUST allow an HR Administrator to create, edit, and deactivate leave types.
- **FR-017**: System MUST allow an HR Administrator to generate reports summarizing leave usage by employee, department, leave type, and time period.
- **FR-018**: System MUST allow an HR Administrator to review the complete audit trail of status changes and approval decisions for any leave request.

**Lifecycle & Audit**

- **FR-019**: System MUST record every leave request status transition with a timestamp and the acting user, forming a complete, permanent audit trail (BR-007).
- **FR-020**: System MUST enforce the leave request lifecycle so that a request can only move Pending → Approved, Pending → Rejected, or Pending → Cancelled, and Approved/Rejected/Cancelled are terminal states for this release.
- **FR-021**: System MUST record and reason about all leave request and audit dates/timestamps in UTC (BR-010).

### Non-Functional Requirements

- **NFR-001 (Security)**: Access to leave data MUST be enforced by role at every access point; a user MUST NOT be able to view or act on data outside their permitted scope (own requests, direct reports' requests, or organization-wide, per role) by any means, including direct navigation or shared links.
- **NFR-002 (Auditability)**: Every business-relevant action (submission, approval, rejection, cancellation) MUST be traceable to a specific user and timestamp, and audit records MUST be immutable once created.
- **NFR-003 (Availability)**: The system MUST be available during the organization's business hours at a reliability level appropriate for a system that has replaced a manual HR process employees depend on.
- **NFR-004 (Scalability)**: The system MUST support the organization's entire workforce submitting and reviewing requests concurrently during peak periods (e.g., year-end leave planning) without failed submissions or perceptible slowdown.
- **NFR-005 (Usability)**: The core workflows — submitting a request, approving or rejecting a request, and viewing a balance — MUST be completable by a first-time user without training or a help desk call.
- **NFR-006 (Data Integrity)**: The leave balance displayed to a user MUST always be consistent with the sum of that user's approved, non-cancelled requests recorded in the audit trail; there must be no discrepancy between the two.
- **NFR-007 (Extensibility)**: The system MUST be able to accommodate the future capabilities listed in the Assumptions and Out of Scope sections without requiring a fundamental redesign of the data model or workflow.
- **NFR-008 (Privacy)**: Leave request reasons (particularly for Medical Leave) are sensitive personal data and MUST be visible only to the requesting employee, their reviewing manager, and HR Administrators.

### Key Entities

- **Employee**: A person who can submit leave requests. Key attributes: name, role (Employee, Manager, and/or HR Administrator — a person may hold more than one role), reporting manager, employment status. An employee with one or more direct reports acts as a Manager for those requests.
- **LeaveRequest**: A single request for time off. Key attributes: requesting employee, leave type, start date, end date, optional reason, current status (Pending, Approved, Rejected, Cancelled), submission timestamp, deciding manager (once decided), decision comment.
- **LeaveBalance**: An employee's remaining allowance for a specific leave type. Key attributes: employee, leave type, available amount.
- **LeaveType**: A category of leave that can be requested. Key attributes: name (e.g., Vacation, Personal Leave, Medical Leave, Unpaid Leave), whether it is balance-tracked (Unpaid Leave is not constrained by a balance), active/inactive status.
- **AuditRecord**: A permanent record of a single status transition or decision on a LeaveRequest. Key attributes: related request, previous status, new status, acting user, timestamp, optional comment.
- **Report**: An on-demand summary of leave usage data over a chosen date range, grouped by employee, department, and/or leave type.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An employee can submit a new, valid leave request in under 2 minutes from sign-in.
- **SC-002**: A manager can review and decide on a pending request (approve or reject with comment) in under 1 minute per request.
- **SC-003**: 100% of leave request status changes are traceable to a specific acting user and timestamp in the audit trail.
- **SC-004**: Zero leave balances are ever observed to go negative across all approved requests.
- **SC-005**: An HR Administrator can locate any specific historical leave request via search in under 30 seconds.
- **SC-006**: 100% of leave requests in the organization are tracked in NovaLeave rather than by email or spreadsheet within the first month of launch.
- **SC-007**: At least 90% of employees and managers successfully complete their primary task (submit a request, or approve/reject one) on their first attempt without needing help desk support.
- **SC-008**: The system handles the organization's entire workforce submitting requests during a peak period (e.g., the two weeks around a year-end deadline) with zero failed submissions attributable to system load.

## Assumptions

- Leave balances are tracked in whole days for this release; half-day or hourly granularity is not required.
- Leave balances are established and maintained by HR Administrators (e.g., initial allotment, periodic accrual) rather than being automatically calculated by the system in this release.
- Each employee has exactly one direct manager at a time; multi-level or delegated approval chains are not required for this release.
- The four listed leave types (Vacation, Personal Leave, Medical Leave, Unpaid Leave) are pre-configured at launch; HR Administrators can add further types using the leave type management capability.
- The "Draft" lifecycle state mentioned as optional is not required for this release — employees submit requests directly into the Pending status.
- Users are provisioned into the system by an administrator; self-service account registration is not required for this release.
- An employee with no assigned manager is routed to HR Administrators for a decision, as noted in Edge Cases.
- The specific technical mechanism for secure sign-in and role enforcement is a delivery decision governed by the project's engineering constitution, not a specification concern.

## Constraints

- The first release (MVP) must exclude all capabilities explicitly listed under Out of Scope below; these are acknowledged future directions, not omissions.
- Every business rule listed (BR-001 through BR-010) MUST be enforced consistently regardless of which part of the system a request or decision originates from.
- The system MUST be designed so that the future capabilities listed under Out of Scope can be added later without redesigning the core leave request workflow or data model.

## Out of Scope

The following are explicitly deferred beyond this first release and MUST NOT be assumed to exist in the MVP:

- Email notifications for request submission, decision, or reminders.
- Calendar integration (e.g., syncing approved leave to Outlook or Google Calendar).
- A public REST API for third-party or external integration.
- Microsoft Entra ID or other external single sign-on authentication.
- Multi-company or multi-tenant support.
- Azure-specific deployment automation.
- Power BI or other external business-intelligence tool integration.
- Modifying or cancelling a request that has already been Approved.
- Half-day, hourly, or other sub-day leave granularity.
- Multi-level or delegated approval workflows.
- Background jobs (e.g., automated balance accrual or scheduled reports).
