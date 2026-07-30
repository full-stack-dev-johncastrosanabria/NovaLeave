# Google AI Studio Prompt: NovaLeave Functional Prototype

Copy and paste the full prompt below into Google AI Studio.

---

## 1. Role and Working Mode

Act as a Senior Product Designer, Senior UX/UI Designer, Frontend Architect, Business Analyst, Accessibility Specialist, Functional Prototype Engineer, and QA Engineer.

Build a functional, highly realistic NovaLeave prototype from this complete supplied specification. Review the full specification before generating. Preserve traceability from requirements to screens, routes, workflows, rules, and demo scenarios. Ask questions instead of assuming any material behavior. Do not add speculative scope, actors, roles, states, routes, transitions, integrations, or features. Do not weaken the authorization, HR read-only, balance, audit, or concurrency rules.

## 2. Project Context

NovaLeave is an MVP vacation request management system. Its business objective is to replace spreadsheet/manual leave tracking with an auditable, role-aware workflow for requesting, resolving, tracking, and reviewing vacation requests.

The approved MVP scope is:

- One leave type only: `Vacation`.
- Three combinable human roles: `User`, `Approver`, and `HR`, plus the automatic `System` actor.
- One global vacation balance per User.
- Monthly accrual of one whole day per completed month.
- Pending vacation requests reserve balance.
- Approval converts reservation into permanent deduction.
- Rejection and timeout release reservation.
- Valid pre-start deactivation of an approved request restores deducted balance.
- HR has organization-wide read access and limited Approver capability management.

The approved production architecture target is ASP.NET Core MVC with Razor Views, Bootstrap 5.3, ASP.NET Core Identity, EF Core, SQL Server, Clean Architecture, Application use-case services, Domain rules, Infrastructure persistence, and Presentation controllers/views.

For this prototype, use Google AI Studio supported technology and a centralized mock backend/service layer if a production backend is unavailable. The prototype is not the final ASP.NET Core implementation and must not claim production compliance. It must preserve approved system behavior as closely as reasonably possible.

## 3. Prototype Objective

Generate a functional, fully navigable, responsive, accessible prototype suitable for stakeholder validation and live demonstration.

The prototype must:

- Demonstrate UC-01 through UC-22.
- Include every approved user-visible function.
- Support User, Approver, HR, System behavior, and valid multi-role context switching.
- Use realistic seeded data.
- Demonstrate successful, invalid, unauthorized, stale, and concurrent scenarios.
- Preserve the approved business rules, route scopes, validations, audit behavior, and exclusions.
- Use a centralized service/state layer that revalidates authorization and business rules at execution time.

## 4. Explicit Non-Goals and Exclusions

Do not implement, display, imply, or stub as active MVP functionality:

- Personal Leave.
- Medical Leave.
- Leave types other than Vacation.
- Half-day requests.
- Hourly requests.
- Attachments.
- Holiday-calendar integrations or holiday exclusion from working-day calculations.
- Per-user time zones.
- Teams.
- Departments as approval scope.
- Managers, direct managers, reporting hierarchy, approval hierarchy.
- Delegation.
- Escalation.
- SSO.
- Automatic password recovery.
- Public APIs.
- Payroll integration.
- Google Calendar integration.
- Outlook integration.
- External enterprise integrations.
- Advanced reports, analytics, KPIs, AI simulations, advanced AI functionality, or conversational bots.
- User cancellation of Pending requests.
- Partial cancellation.
- Post-start deactivation.
- Editing Approved request dates.
- HR approval, rejection, cancellation, deactivation, request editing, balance modification, role assignment, or role removal.

Departments may appear only as seed-data attributes for HR organization-wide visibility and filtering; they must not create approval scope, hierarchy, or authorization rules.

## 5. Actors, Roles, and Combinations

Actors:

- `User`: authenticated active identity that creates, views, edits owned Pending vacation requests, views own balance/history, and views own calendar scope.
- `Approver`: authenticated active identity with `Approver` role and `canResolveRequests=true`; may access approval queues/details and resolve eligible non-owned requests.
- `HR`: authenticated active identity with organization-wide read-only access plus limited management of `canResolveRequests` for identities that already hold `Approver`.
- `System`: automatic background actor for timeout cancellation and monthly accrual.

Approved role combinations:

- A single identity may hold any combination of `User`, `Approver`, and `HR`.
- The UI context switcher is labeled `Mis roles` and appears only when the identity has more than one active context.
- Switching visual context changes navigation and route context only. It must not modify identity, claims, role membership, active status, or actual authorization.
- The service layer must always authorize by current identity, active status, role, capability, ownership, request state, and row version, regardless of what the UI shows.

## 6. User Capabilities

User functionality:

- Authenticate through `/Identity/Account/Login`.
- Use a User dashboard or redirect to `Mi espacio`.
- View own vacation requests at `/mis-solicitudes`.
- Create a Vacation request at `/mis-solicitudes/crear`.
- Use exactly two request input modes: `Fecha inicio + Fecha fin` and `Fecha inicio + Cantidad de días`.
- Edit an owned `Pending` request at `/mis-solicitudes/{id}/editar`.
- View owned request detail at `/mis-solicitudes/{id}`.
- View own global balance and movement history at `/saldo`.
- View personal calendar at `/calendario`.
- Receive Spanish validation, conflict, success, loading, empty, access-denied, and error feedback.

User scope:

- User request list, detail, balance, history, and calendar are owner-filtered.
- User calendar shows only the authenticated User's authorized personal scope, specifically own approved vacation periods.
- User cannot approve, reject, deactivate, cancel, resolve, edit approved dates, view another User's private data, or modify balances directly.

## 7. Approver Capabilities and Authorization

An eligible Approver requires all conditions:

- Authenticated.
- Account is active.
- Holds `Approver`.
- `canResolveRequests=true`.
- Is not the owner of the target request where resource-specific authorization applies.
- Request is in a state eligible for the operation.
- Authorization is revalidated in the prototype service layer at execution time.

Approver functionality:

- View eligible Pending request queue at `/aprobaciones`.
- View eligible request detail at `/aprobaciones/{id}`.
- Approve a Pending non-owned request at `/aprobaciones/{id}/aprobar`.
- Reject a Pending non-owned request at `/aprobaciones/{id}/rechazar` with a mandatory normalized rejection reason of 10-500 characters.
- View resolution history at `/aprobaciones/historial`.
- View the Approver calendar at `/calendario`, with approved organization-wide vacation periods anonymized according to the active spec.
- Deactivate an Approved non-owned request only before its vacation period begins at `/aprobaciones/{id}/desactivar`.

Approver restrictions:

- Disabled Approvers (`canResolveRequests=false`) immediately lose queue, detail, calendar-as-Approver, and resolution access.
- Inactive Approvers cannot access protected approval views or perform resolution operations.
- Approvers cannot resolve their own requests.
- UI hiding/disabling is only a usability hint and must never be treated as authorization.

## 8. HR Capabilities and Restrictions

Use the dedicated HR global calendar route only:

`/rrhh/calendario`

HR capabilities:

- `/rrhh`: HR dashboard.
- `/rrhh/solicitudes`: organization-wide read-only request list.
- `/rrhh/solicitudes/{id}`: read-only request detail with audit trail.
- `/rrhh/calendario`: global read-only organization-wide calendar of all vacation requests across all users and departments.
- `/rrhh/saldos`: read-only balance summary for all users.
- `/rrhh/saldos/{userId}`: read-only movement timeline for a user.
- `/rrhh/auditoria`: read-only relevant business/security audit log with redacted sensitive payloads.
- `/rrhh/aprobadores`: list identities holding `Approver` and their `canResolveRequests` status.
- `/rrhh/aprobadores/{id}/capacidad`: enable or disable `canResolveRequests` only for an identity that already has `Approver`, with reason, confirmation, row version, transaction-style persistence, security-stamp/session refresh behavior, and audit.

HR restrictions:

- HR must not approve requests.
- HR must not reject requests.
- HR must not cancel requests.
- HR must not deactivate requests.
- HR must not edit request data.
- HR must not modify balances.
- HR must not assign roles.
- HR must not remove roles.
- HR must not perform request-resolution operations.
- HR must not receive global calendar behavior through `/calendario`.

The shared route `/calendario` remains limited to User and eligible Approver contexts only.

## 9. Approved Route Inventory

| Route | Allowed role | Capability | Data scope | Purpose | Actions | Forbidden actions | Unauthorized result |
|---|---|---|---|---|---|---|---|
| `/` | Active User, Approver, or HR | Authenticated + active | Redirect by active context | Landing/dashboard redirect | Navigate | Protected data mutation | Login or 403 |
| `/Identity/Account/Login` | Anonymous | N/A | Demo login form | Authentication | Sign in | Business mutation | Validation message |
| `/Identity/Account/Logout` | Authenticated | Valid anti-forgery equivalent | Current session | Logout | Sign out | Business mutation | Login |
| `/Identity/Account/AccessDenied` | Any | N/A | None | 403 display | Navigate back | Protected data | Access denied |
| `/mis-solicitudes` | User | Active User | Own requests only | Request list/history | View/filter/page | Cross-user access | 403/404 without existence disclosure |
| `/mis-solicitudes/crear` | User | Active User | Own new request | Create form | Submit Vacation request | Non-Vacation, partial-day, owner override | 400 validation/403 |
| `/mis-solicitudes/{id}` | User | Active User + owner | Owned request | Detail | View own reason, state, audit summary | Cross-user detail | 403/404 |
| `/mis-solicitudes/{id}/editar` | User | Active User + owner + Pending | Owned Pending request | Edit form | Edit Pending dates/input mode/reason | Edit Approved/terminal/cross-user | 403/409/400 |
| `/saldo` | User | Active User | Own balance | Balance summary/history | View | Balance edit | 403 |
| `/calendario` | User | Active User | Own approved vacation periods | Personal calendar | View, navigate to own detail | HR global behavior, cross-user data | 403/404 |
| `/calendario` | Approver | Active Approver + `canResolveRequests=true` | Approved org-wide periods, anonymized | Approver calendar | View, navigate only when resource-eligible | HR names/global status, unauthorized detail | 403/no link |
| `/aprobaciones` | Approver | Active + `canResolveRequests=true` | Eligible non-owned Pending requests, global | Approval queue | View | Self-resolution, disabled access | 403 |
| `/aprobaciones/{id}` | Approver | Active + `canResolveRequests=true` + eligible | Eligible non-owned request | Resolution detail | View projected balance, warnings, actions | Owned/ineligible/disabled access | 403/404/409 |
| `/aprobaciones/{id}/aprobar` | Approver | Active + `canResolveRequests=true` + not owner + Pending | Target request | Approve | Approve with revalidation | Negative projected balance, stale, invalid state | 403/409/400 |
| `/aprobaciones/{id}/rechazar` | Approver | Active + `canResolveRequests=true` + not owner + Pending | Target request | Reject | Reject with reason | Missing reason, stale, invalid state | 403/409/400 |
| `/aprobaciones/{id}/desactivar` | Approver | Active + `canResolveRequests=true` + not owner + Approved + pre-start | Target request | Pre-start deactivation | Deactivate entire request | Partial/post-start/stale | 403/409/400 |
| `/aprobaciones/historial` | Approver | Active + `canResolveRequests=true` | Resolutions by this Approver | History | View/filter | Mutation | 403 |
| `/rrhh` | HR | Active HR | Organization summary | HR dashboard | View | Request resolution/balance edit/role assignment | 403 |
| `/rrhh/solicitudes` | HR | Active HR | All requests organization-wide | Read-only list | View/filter/page | Approve/reject/deactivate/edit | 403 |
| `/rrhh/solicitudes/{id}` | HR | Active HR | Any request + audit trail | Read-only detail | View; audit sensitive reason access | Mutate request | 403 |
| `/rrhh/calendario` | HR | Active HR | All vacation requests across all users/departments | HR global calendar | View, navigate to read-only HR detail | Mutation, `/calendario` global behavior | 403 |
| `/rrhh/saldos` | HR | Active HR | All user balances | Read-only balances | View/filter/page | Balance modification | 403 |
| `/rrhh/saldos/{userId}` | HR | Active HR | Target user's movements | Read-only balance timeline | View | Balance modification | 403 |
| `/rrhh/auditoria` | HR | Active HR | Relevant audit records | Audit log | View/filter | Modify/delete audit | 403 |
| `/rrhh/aprobadores` | HR | Active HR | Identities with Approver role | Capability list | View/open toggle modal | Assign/remove roles | 403/400 |
| `/rrhh/aprobadores/{id}/capacidad` | HR | Active HR + target has Approver role | Target Approver capability | Toggle capability | Enable/disable `canResolveRequests` with reason/confirmation/row version | Toggle non-Approver, role assignment/removal, request resolution | 400/403/409 |

## 10. Complete Screen Inventory

For every screen, use Spanish user-facing content, consistent status labels, accessible forms/tables/dialogs, loading states, empty states, validation summaries, and safe error messaging.

| Screen | Route | Authorized role | Purpose | Information shown | Actions | Forbidden actions | Rules/validation | Related use cases | Related AC |
|---|---|---|---|---|---|---|---|---|---|
| Login | `/Identity/Account/Login` | Anonymous | Authenticate and select demo account | NovaLeave brand, email/password, demo users | Sign in | Business mutation | Failed login audit; no stack traces | UC-01 | AC-030, AC-031, AC-060 |
| Role context switcher | Header `Mis roles` | Multi-role active identity | Switch visual context | Active role options | Navigate to context root | Change claims/roles | Context switch is navigation only | UC-02 | RBFV-009, RBFV-010, RBFV-020, RBFV-021 |
| User dashboard/list | `/mis-solicitudes` | User | View owned requests | Own requests, status, pagination, balance summary | Filter/page/open/create | Cross-user data | Owner filter | UC-03 | AC-007, AC-008, RBFV-001 |
| Create vacation request | `/mis-solicitudes/crear` | User | Submit Vacation request | Two input modes, reason, preview | Create | Attachments, non-Vacation, partial days | Required fields, next-day, overlap, balance, server-derived values | UC-04 | AC-001-005, AC-010, AC-011, AC-035, AC-040-043 |
| Edit Pending request | `/mis-solicitudes/{id}/editar` | User owner | Edit owned Pending request | Existing values, row version | Save | Edit Approved/terminal/cross-user | Pending-only, full revalidation, conflict | UC-05 | AC-045, AC-046 |
| User request detail | `/mis-solicitudes/{id}` | User owner | View own detail | Status, dates, days, own reason, balance effect, audit | View | Resolve/edit non-Pending | Owner authorization | UC-06 | AC-007, AC-008 |
| Balance/history | `/saldo` | User | View own global balance | `Acumulado total`, `Pendientes`, `Días gozados`, `Disponible`, movements | View | Edit balance | Non-negative formula | UC-07 | AC-044 |
| User calendar | `/calendario` | User | Personal calendar | Own approved periods | Open own detail | HR global scope | Keyboard navigation, owner links only | UC-08 | AC-057, RBFV-026, RBFV-029 |
| Approver queue | `/aprobaciones` | Eligible Approver | View eligible Pending requests | Requester, dates, days, current/projected balance | Open detail | Disabled/self access | Capability + non-owner; projected balance server-derived | UC-09 | AC-012, AC-017, AC-047, AC-058, RBFV-003, RBFV-012 |
| Approver detail | `/aprobaciones/{id}` | Eligible Approver | Review request for resolution | Full detail, projected balance, overlap warning, actions | Approve/reject/deactivate where eligible | Own/ineligible/disabled | Revalidate role/capability/state | UC-10 | RBFV-025, RBFV-030 |
| Approval workflow | `/aprobaciones/{id}/aprobar` | Eligible Approver | Approve Pending request | Confirmation, projected balance | Approve | Negative/stale/self | BR-036-BR-038, transaction rollback | UC-11 | AC-012, AC-018-020 |
| Rejection workflow | `/aprobaciones/{id}/rechazar` | Eligible Approver | Reject Pending request | Reason textarea | Reject | Empty/short/long reason | Reason 10-500 chars, release reservation | UC-12 | AC-016, AC-036 |
| Pre-start deactivation | `/aprobaciones/{id}/desactivar` | Eligible Approver | Deactivate future Approved request | Confirmation, balance restoration | Deactivate entire request | Partial/post-start | Pre-start only, atomic restore | UC-13 | AC-050-053 |
| Resolution history | `/aprobaciones/historial` | Eligible Approver | View own resolution history | Date, action, requester, status, detail link | Filter/open | Mutation | Capability required | UC-14 | RBFV-023 |
| Approver calendar | `/calendario` | Eligible Approver | Anonymized approved periods | Month grid, anonymized events | Open detail only if eligible | Names when not authorized | No unauthorized links | UC-15 | RBFV-026 |
| HR dashboard | `/rrhh` | HR | Organization summary | Pending count, active Approvers, balances, quick links | Navigate | Mutation | Active HR only | UC-18-UC-22 | RBFV-005, RBFV-032 |
| HR request list | `/rrhh/solicitudes` | HR | Read-only org request list | Requester, department, dates, status, days, reservation, deduction | Filter/page/open | Resolve/edit | Read-only badge | UC-18 | AC-HR-001 |
| HR request detail | `/rrhh/solicitudes/{id}` | HR | Read-only any request | Full detail, reason, audit trail | View | Approve/reject/deactivate/edit | Sensitive reason access audit | UC-18 | AC-HR-002, AC-HR-009, AC-HR-010 |
| HR global calendar | `/rrhh/calendario` | HR | Global org calendar | All requests, departments, requester names, statuses, days | Navigate to read-only detail | Mutation; shared `/calendario` behavior | HR-only route | UC-19 | AC-HR-003, AC-HR-010 |
| HR balances | `/rrhh/saldos` | HR | Read-only all balances | Users, four balance labels | Filter/page/open | Modify balance | Read-only | UC-20 | AC-HR-004, AC-HR-011 |
| HR balance movements | `/rrhh/saldos/{userId}` | HR | Read-only movement timeline | Accruals, reservations, releases, deductions, restorations | View linked request | Modify balance | Read-only | UC-20 | AC-HR-005, AC-HR-011 |
| HR audit log | `/rrhh/auditoria` | HR | Relevant audit records | Timestamp, actor, role, action, entity, result, redacted data | Filter/page | Edit/delete audit | Immutable/read-only | UC-21 | AC-HR-006 |
| HR Approver capability list | `/rrhh/aprobadores` | HR | Manage resolution capability | Approver identities, active, `canResolveRequests`, row version | Open modal | Assign/remove roles | Target must have Approver role | UC-22 | AC-HR-007, AC-HR-012 |
| Capability toggle modal | `/rrhh/aprobadores/{id}/capacidad` | HR | Toggle `canResolveRequests` | Target, current value, reason, confirmation | Enable/disable | Toggle non-Approver, no reason, stale version | Reason 10-500, row version, audit, refresh eligibility | UC-22 | AC-HR-008, AC-HR-009, AC-HR-012 |
| Access denied | `/Identity/Account/AccessDenied` or `/acceso-denegado` | Any denied actor | Explain 403 | Safe message, return link | Navigate | Leak existence/sensitive data | Deny-by-default | Cross-cutting | RBFV-002, RBFV-004, RBFV-006-RBFV-008 |
| Not found | Safe 404 | Any | Resource not available | Safe message | Navigate | Existence disclosure | 404/403 ambiguity safe | Cross-cutting | SEC-002, SEC-003 |
| Validation error | Same form/view | Actor for attempted action | Correct invalid input | Summary + field errors | Retry | Partial mutation | Preserve state | Cross-cutting | ERR-001 |
| Concurrency conflict | Same/refreshed view | Actor for attempted action | Resolve stale operation | Conflict alert, refresh action | Refresh | Partial mutation | Row version conflict | Cross-cutting | ERR-004, CON-004 |
| Empty/loading states | Each list/calendar/table | Authorized actor | Demo realistic waiting/no-data | Skeleton/spinner/empty copy | Navigate/filter | Misleading success | Accessible status | All list/calendar UCs | RBFV-031, RBFV-032 |

## 11. All 22 Use Cases as Executable Prototype Workflows

Implement each use case as a working workflow in the prototype service layer and UI:

| UC | Name | Actor | Preconditions | Trigger | Main flow | Alternates/exceptions | Resulting state | Balance effect | Audit effect | Required demo scenario |
|---|---|---|---|---|---|---|---|---|---|---|
| UC-01 | Authenticate | Anonymous/User | Demo account exists | Login submit | Validate credentials, active status, session | Bad credentials, inactive, expired | Authenticated context | None | Failed auth security event | Successful User, Approver, HR, combo login; failed login |
| UC-02 | Switch Role Context | Multi-role identity | Authenticated, active, >=2 roles | Select `Mis roles` option | Navigate to context root | Missing/inactive role denied | No domain state change | None | None unless denied | Demo combo account switching User/Approver/HR |
| UC-03 | View Own Vacation Requests | User | Active User | Open `/mis-solicitudes` | List owner-filtered requests | Empty list, pagination, cross-ID denied | No change | None | Optional read trace | Owner sees only own requests |
| UC-04 | Create Vacation Request | User | Active User, valid balance | Submit create form | Validate, calculate days, create Pending, reserve, audit | Missing data, invalid dates, overlap, insufficient balance, duplicate | `Pending` | Reservation +N | Create audit | Valid range and start+days; invalid cases |
| UC-05 | Edit Owned Pending Request | User owner | Owned Pending request | Submit edit | Revalidate ownership/state/dates/overlap/balance/version, adjust reservation | Not owner, not Pending, stale, invalid | Remains `Pending` | Reservation adjusted | Edit audit on success | Edit succeeds and stale edit fails |
| UC-06 | View Owned Request Detail | User owner | Owned request | Open detail | Show status, dates, days, reason, balance effect, audit summary | Cross-user ID denied | No change | None | None | Owner detail; unauthorized forced browsing |
| UC-07 | View Own Balance and History | User | Active User | Open `/saldo` | Show balance cards and movement timeline | Empty movements | No change | None | None | Show accrued, pending, deducted, available |
| UC-08 | View Personal Vacation Calendar | User | Active User | Open `/calendario` | Month grid of own approved periods | Unauthorized event link omitted/denied | No change | None | None | Calendar event opens own detail |
| UC-09 | View Eligible Pending Queue | Approver | Active + `canResolveRequests=true` | Open `/aprobaciones` | List eligible non-owned Pending requests, current/projected balance | Disabled/inactive Approver denied; own requests excluded | No change | None | Security event on denial | Eligible queue and disabled Approver denial |
| UC-10 | View Request Detail for Resolution | Approver | Eligible Approver, non-owner | Open `/aprobaciones/{id}` | Revalidate, show detail/projected balance/actions | Negative projected balance warning disables approve; unauthorized denied | No change | None | Security event on denial | Positive and negative projected balance details |
| UC-11 | Approve Pending Request | Approver | Eligible non-owner Pending request | Approve confirm | Revalidate state/role/capability/owner/overlap/projected balance/version, approve atomically | Self, inactive, disabled, stale, negative projected balance, conflict | `Approved` on success | Reservation -> deduction | Transition + balance audit | Success and stale/negative conflict |
| UC-12 | Reject Pending Request | Approver | Eligible non-owner Pending request | Reject with reason | Validate reason, revalidate auth/state/version, reject atomically | Missing/invalid reason, stale, self, disabled | `Rejected` on success | Release reservation | Transition audit; reason stored not logged | Valid reject and invalid reason |
| UC-13 | Deactivate Approved Before Start | Approver | Eligible non-owner Approved future request | Deactivate confirm | Revalidate pre-start/state/version, transition atomically | Post-start, partial, stale, self, disabled | `CancelledByApprover` | Restore deduction | Transition + restoration audit | Future approved succeeds; started approved fails |
| UC-14 | View Resolution History | Approver | Active + capability | Open `/aprobaciones/historial` | Show actions resolved by Approver | Empty/filter states; disabled denied | No change | None | None | Filter by action/date/requester |
| UC-15 | View Approver Calendar | Approver | Active + capability | Open `/calendario` | Show anonymized approved periods | Unauthorized detail link omitted | No change | None | None | Anonymized event and eligible detail navigation |
| UC-16 | Cancel Unresolved Pending by Timeout | System | Pending older than X=14 days | Run timeout service/demo action | Revalidate Pending and boundary, cancel idempotently | Already resolved no-op; race loses safely | `CancelledByTimeout` | Release reservation | System audit | Timeout run and approval race |
| UC-17 | Execute Monthly Accrual | System | Completed month from employment start | Run accrual service/demo action | Add one whole day once per completed month | Duplicate accrual skipped; partial month excluded | No request state change | Accrued +1 | Accrual audit/movement | First accrual and duplicate idempotency |
| UC-18 | View Organization-Wide Requests | HR | Active HR | Open `/rrhh/solicitudes` or detail | Show all requests read-only, including audit/projected-balance info | Unauthorized/inactive denied; mutations forbidden | No change | None | HR sensitive reason access audit | HR list/detail with no actions |
| UC-19 | View Organizational Calendar | HR | Active HR | Open `/rrhh/calendario` | Show all vacation requests across users/departments with names/status/days | HR `/calendario` denied; mutations forbidden | No change | None | Optional read audit | Global HR calendar only at `/rrhh/calendario` |
| UC-20 | View Balances and Movements | HR | Active HR | Open `/rrhh/saldos` or detail | Show all balances and timelines read-only | Balance edit attempt forbidden | No change | None | Security event on forbidden edit | HR balance list and movement detail |
| UC-21 | View Relevant Audit Records | HR | Active HR | Open `/rrhh/auditoria` | Filter business/security audit records with redacted data | Unauthorized denied | No change | None | HR audit access read | Audit log filters and redaction |
| UC-22 | Manage Approver Resolution Capability | HR | Active HR, target has Approver | Toggle with reason/confirmation/row version | Validate target/reason/confirmation/version, update capability atomically, refresh authorization | Non-Approver target, inactive HR, missing reason, stale version, role assignment attempt | No request state change | None | Capability audit or failed-toggle security event | Disable Approver and show immediate denial |

## 12. Request Lifecycle

Use only these request states and Spanish labels:

- `Pending`: `Pendiente`
- `Approved`: `Aprobada`
- `Rejected`: `Rechazada`
- `CancelledByTimeout`: `Cancelada automáticamente`
- `CancelledByApprover`: `Cancelada por aprobador`

Approved transitions only:

- New request -> `Pending` by active User creation. Creation is not a lifecycle transition from an existing request.
- `Pending -> Pending` for an owned Pending edit. Editing is not resolution and must revalidate atomically.
- `Pending -> Approved` by eligible Approver.
- `Pending -> Rejected` by eligible Approver with mandatory reason.
- `Pending -> CancelledByTimeout` by System after configured timeout.
- `Approved -> CancelledByApprover` by eligible Approver before vacation start only.

Prevent every invalid transition in both UI and service layer.

## 13. Balance Behavior

Implement one global non-expiring vacation balance per User:

- `Acumulado total`: accrued whole days.
- `Pendientes`: days reserved by active Pending requests.
- `Días gozados`: permanently deducted days from approved requests.
- `Disponible`: accrued - deducted - reserved.

Rules:

- All quantities are non-negative whole working days.
- Pending creation reserves requested working days.
- Pending edit adjusts reservation.
- Approval converts reservation into permanent deduction.
- Rejection releases reservation and creates no deduction.
- Timeout releases reservation and creates no deduction.
- Pre-start deactivation restores previously deducted days.
- Monthly accrual adds exactly one day per completed month from employment start; duplicate accrual is idempotent.

Seed realistic data that visibly demonstrates every balance effect.

## 14. Projected Balance

Implement FR-023 and BR-036 through BR-038 explicitly:

- Calculate projected available balance as `authoritativeAvailableExcludingCurrent - authoritativeRequestedDays`.
- `authoritativeAvailableExcludingCurrent` means accrued minus permanently deducted minus days reserved by other active Pending requests, excluding the current request's own reservation.
- Display in Approver list/detail:
  - `Disponible actual`
  - `Días solicitados`
  - `Disponible después de aprobar`
- The value is informational only and never trusted from the client.
- At query time, calculate from current mock repository data.
- At approval POST-equivalent time, revalidate projected balance immediately before commit.
- If projected balance is negative, reject approval, preserve request as `Pending`, create no deduction, and display a Spanish conflict/warning.
- Demonstrate stale projected-balance data by changing another Pending reservation or balance between detail load and approval.
- Simulate transaction-style rollback: failed approval leaves request, balance, movement, and audit unchanged.

## 15. Validation Rules

Apply these validations in the centralized service layer and reflect them in the UI with clear Spanish messages:

- Required start date.
- Exactly one input mode: date range OR start date + working-day count.
- Required end date for range mode.
- Required whole day count for start+days mode.
- Vacation only.
- Start date must be after the current business date.
- Start date must not be Saturday or Sunday.
- End date must not precede start date.
- Working-day count must be greater than zero.
- Count Monday-Friday inclusively; holidays count as working days.
- No same-User overlap with Pending or Approved requests; adjacent ranges allowed.
- Request must not exceed available balance.
- Owner is server-set; ignore client-supplied owner, status, balance, working-day total, role, or derived dates.
- Request reason must be normalized 10-500 Unicode text elements, no whitespace-only, markup treated as plain text.
- Pending-only editing.
- Row-version/stale-data validation on edits and resolution.
- Approver must be active, hold Approver, have `canResolveRequests=true`, not own target, and target must be eligible.
- Rejection reason must be normalized 10-500 Unicode text elements.
- HR capability toggle reason must be normalized 10-500 Unicode text elements and require explicit confirmation.
- HR capability target must already hold Approver.
- Inactive accounts fail protected operations.
- Expired sessions require re-authentication.

Use concise Spanish feedback such as:

- `Completa los campos obligatorios.`
- `La fecha de inicio debe ser posterior a hoy.`
- `La solicitud debe contener al menos un día hábil.`
- `La solicitud se cruza con otra solicitud pendiente o aprobada.`
- `No hay saldo disponible suficiente.`
- `La información cambió. Actualiza la solicitud e intenta nuevamente.`
- `No tienes autorización para realizar esta acción.`

## 16. Authorization and Security

Implement deny-by-default behavior in the prototype service layer:

- Simulate authentication, active/inactive status, role authorization, capability authorization, ownership, request state, and row-version checks.
- Direct navigation to forbidden routes must fail with 403/access denied or safe 404 without resource-existence disclosure.
- Simulate IDOR prevention and forced-browsing prevention.
- Simulate anti-forgery-equivalent protection for POST-equivalent actions with operation tokens or guarded service commands.
- Simulate overposting resistance by ignoring or rejecting manipulated client fields.
- Redact sensitive reason/rejection reason from audit payloads and technical errors.
- Do not show stack traces, database details, credentials, tokens, or secret values.
- Session expiration should deny protected access and require login.

## 17. Concurrency and Transactional Scenarios

Include demo controls or scripted scenario buttons that safely simulate:

- Two Approvers resolving the same Pending request: only one approve/reject wins; loser receives conflict; no duplicate balance effect.
- Approval with stale projected-balance data: revalidation fails if projected balance becomes negative.
- Timeout cancellation competing with approval: exactly one operation succeeds; the other fails/no-ops without partial state.
- Pre-start deactivation competing with another operation: one valid operation succeeds; stale duplicate fails.
- HR disabling `canResolveRequests` during an active Approver session: Approver immediately loses queue/detail/resolution access.
- Monthly accrual idempotency: duplicate execution for the same User and period adds at most one day.
- Duplicate request creation/replay and overlapping submissions: at most one logical request succeeds.

Failed operations must not leave partial request state, balance movement, or audit changes.

## 18. Audit Behavior

Simulate immutable audit entries with:

- UTC timestamp.
- Actor ID.
- Actor role.
- Action.
- Entity type.
- Entity ID.
- Result.
- Correlation ID.
- Request ID where applicable.
- Previous state and new state for transitions.
- Safe metadata.
- Approved reason field names only where sensitive content is redacted.

Audit required events:

- Request creation.
- Pending edit.
- Approval.
- Rejection.
- Timeout cancellation by System.
- Pre-start deactivation.
- Balance deduction/restoration within the same transition audit.
- Authentication failures.
- Expired session reuse.
- Authorization denials.
- Inactive or capability-disabled Approver protected access/resolution attempts.
- HR sensitive reason access.
- Successful and failed HR capability toggles.

Do not expose unauthorized sensitive content in audit views. HR audit view may show that a sensitive field was accessed, not the sensitive value in audit payloads.

## 19. Seeded Data and Demo Accounts

Use realistic simulated data for multiple users and departments. Departments are display/filter metadata only, not authorization scope.

Demo accounts:

| Email | Roles | Active status | `canResolveRequests` | Purpose |
|---|---|---:|---:|---|
| `demo.user@novaleave.test` | User | Active | false | Standard employee workflows |
| `demo.approver@novaleave.test` | Approver | Active | true | Approval workflows |
| `demo.rrhh@novaleave.test` | HR | Active | false | HR read-only and capability management |
| `demo.combo@novaleave.test` | User, Approver, HR | Active | true | Multi-role context switching |
| `demo.disabledapprover@novaleave.test` | Approver | Active | false | Capability-denial scenario |
| `demo.inactive@novaleave.test` | User or Approver | Inactive | false | Inactive-denial scenario |

Seed requests:

- Pending owned by User A, eligible for Approver B.
- Pending owned by Approver B, visible to HR but excluded from Approver B resolution.
- Pending with positive projected balance.
- Pending with negative projected balance after simulated stale balance change.
- Approved future request eligible for pre-start deactivation.
- Approved started/current request where deactivation is denied.
- Rejected request with redacted rejection reason in non-owner audit.
- CancelledByTimeout request.
- CancelledByApprover request.
- User with empty request list.
- Enough request rows for pagination/filter demos.

Seed balances and movements:

- Accruals.
- Reservations.
- Releases.
- Deductions.
- Restorations.
- Users with positive, low, and zero available balance.

## 20. UI and UX Requirements

Follow the final frontend design specification:

- Professional corporate design, not a generic AI dashboard.
- Dark and light blues, white surfaces, cool neutral grays, and approved turquoise accents only when useful.
- No gradients.
- No emojis in application UI.
- Bootstrap-compatible component language.
- Clear visual hierarchy with status, action, balance impact, and outcome prioritized.
- Consistent 12px/16px/24px spacing rhythm.
- White cards and surfaces with subtle borders.
- Accessible status badges with text plus color; color alone is insufficient.
- Responsive layouts: mobile cards, desktop tables, usable tablet views.
- Spanish user-facing labels and messages.
- Use approved labels: `Acumulado total`, `Pendientes`, `Días gozados`, `Disponible`.
- Removed visible balance-card labels: do not use `Devengado`, `Reservado`, or `Deducido` as card titles.
- Request form mode labels exactly: `Fecha inicio + Fecha fin` and `Fecha inicio + Cantidad de días`.
- Use loading, success, validation, conflict, empty, and forbidden states throughout.

## 21. Accessibility

Aim for WCAG 2.1 AA-oriented behavior:

- Full keyboard navigation.
- Visible focus indicators.
- Semantic headings, landmarks, labels, and form associations.
- Accessible validation summaries linked to invalid fields.
- Sufficient color contrast.
- Screen-reader status feedback for async operations.
- Accessible tables with captions or contextual headings.
- Accessible dialogs with `role="dialog"`, `aria-modal="true"`, focus trap, initial focus, and return focus.
- Calendar events keyboard-activatable with Enter/Space.
- Calendar event accessible names containing requester when authorized, date range, and day count.
- Reduced motion support for non-essential animations.
- Touch targets at least 44x44px.

## 22. Prototype Architecture

Organize the prototype internally into:

- Presentation components/pages.
- Application-style use-case services.
- Domain-style rules and lifecycle functions.
- Mock repository/infrastructure layer.
- Seed-data layer.
- Authorization service.
- Audit service.
- Clock/time service with configurable business date.
- Scenario service for concurrency/stale-data demonstrations.

Do not place all business logic in UI components. UI components call service commands/queries. Service commands revalidate authorization, state, version, projected balance, overlap, and transaction-style invariants.

Use immutable or copy-on-write updates for command execution so failed operations roll back cleanly.

## 23. Functional Completion Criteria

The prototype is complete only if:

- All approved roles can be demonstrated.
- UC-01 through UC-22 are navigable and executable.
- All five valid request states appear.
- All valid transitions work.
- Invalid transitions fail.
- Balance behavior is visible.
- Projected balance is calculated, displayed, and enforced.
- Approver capability is enforced immediately.
- HR global access works only through `/rrhh/calendario`.
- HR cannot resolve requests or mutate request/balance/role data.
- Ownership checks work.
- Concurrency and stale-data scenarios can be demonstrated.
- Validation and error states are available.
- Responsive behavior works.
- Accessibility requirements are represented.
- No excluded functionality was introduced.

## 24. Traceability Matrix

Include a traceability matrix in the prototype output using this format:

| Prototype screen or flow | Route | Actor | Requirement | Business rule | Acceptance criterion | Use case | Verification status |
|---|---|---|---|---|---|---|---|
| Login | `/Identity/Account/Login` | Anonymous/User/Approver/HR | SEC-001, CFG-003 | N/A | AC-030, AC-031, AC-060 | UC-01 | Implemented |
| Create request | `/mis-solicitudes/crear` | User | FR-001, FR-002, FR-012 | BR-001, BR-002, BR-004, BR-018, BR-030 | AC-001-005, AC-010, AC-011, AC-035, AC-041-043 | UC-04 | Implemented |
| Approve request | `/aprobaciones/{id}/aprobar` | Approver | FR-006, FR-023 | BR-008, BR-013, BR-015, BR-016, BR-036, BR-037, BR-038 | AC-012, AC-018-020 | UC-11 | Implemented |
| HR calendar | `/rrhh/calendario` | HR | FR-017, AUTHZ-011, AUTHZ-017 | N/A | AC-HR-003, AC-HR-010, RBFV-032 | UC-19 | Implemented |

Every UC-01 through UC-22 must appear. Every active user-visible requirement must map to a prototype screen or workflow. If anything is omitted, mark it explicitly as omitted and explain why; do not claim full compliance.

## 25. Required Google AI Studio Output

Provide:

1. The functional prototype.
2. Screen inventory.
3. Route and authorization matrix.
4. Implemented use-case list.
5. Simulated backend-behavior list.
6. Validation inventory.
7. Demo-user and seeded-scenario list.
8. Traceability matrix.
9. Prototype-environment limitations.
10. Open questions that prevent faithful implementation.

Do not claim full compliance if any approved behavior is omitted. Do not create a marketing landing page. The first screen after login should be the usable role-aware application experience.

## Source Appendix

### Active Actors and Roles

- User
- Approver
- HR
- System

### Active Role Combinations

- User only.
- Approver only.
- HR only.
- User + Approver.
- User + HR.
- Approver + HR.
- User + Approver + HR.

### Five Active States

- Pending
- Approved
- Rejected
- CancelledByTimeout
- CancelledByApprover

### Approved Routes

- `/`
- `/Identity/Account/Login`
- `/Identity/Account/Logout`
- `/Identity/Account/AccessDenied`
- `/acceso-denegado`
- `/mis-solicitudes`
- `/mis-solicitudes/crear`
- `/mis-solicitudes/{id}`
- `/mis-solicitudes/{id}/editar`
- `/saldo`
- `/calendario` for User personal scope and eligible Approver anonymized scope only
- `/aprobaciones`
- `/aprobaciones/{id}`
- `/aprobaciones/{id}/aprobar`
- `/aprobaciones/{id}/rechazar`
- `/aprobaciones/{id}/desactivar`
- `/aprobaciones/historial`
- `/rrhh`
- `/rrhh/solicitudes`
- `/rrhh/solicitudes/{id}`
- `/rrhh/calendario`
- `/rrhh/saldos`
- `/rrhh/saldos/{userId}`
- `/rrhh/auditoria`
- `/rrhh/aprobadores`
- `/rrhh/aprobadores/{id}/capacidad`

### Active FR Identifiers

FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-007, FR-009, FR-011, FR-012, FR-013, FR-014, FR-015, FR-016, FR-017, FR-018, FR-019, FR-020, FR-021, FR-022, FR-023, FR-024, FR-025.

Retired/reserved and not active: FR-008, FR-010.

### Active BR Identifiers

BR-001, BR-002, BR-003, BR-004, BR-005, BR-006, BR-007, BR-008, BR-009, BR-011, BR-012, BR-013, BR-014, BR-015, BR-016, BR-017, BR-018, BR-019, BR-020, BR-021, BR-023, BR-024, BR-025, BR-026, BR-027, BR-028, BR-029, BR-030, BR-031, BR-032, BR-033, BR-034, BR-035, BR-036, BR-037, BR-038.

Retired and not active: BR-010, BR-022.

### Acceptance-Criteria Families

- Standard acceptance criteria: AC-001 through AC-060 where active in the specification.
- HR acceptance criteria: AC-HR-001 through AC-HR-012.
- RBFV criteria: RBFV-001 through RBFV-034.
- Security, authorization, validation, audit, concurrency, configuration, and error criteria must be represented through workflows, service checks, and demo scenarios.

### UC-01 Through UC-22

- UC-01 Authenticate.
- UC-02 Switch Role Context.
- UC-03 View Own Vacation Requests.
- UC-04 Create a Vacation Request.
- UC-05 Edit an Owned Pending Request.
- UC-06 View an Owned Request Detail.
- UC-07 View Own Balance and History.
- UC-08 View the Personal Vacation Calendar.
- UC-09 View the Eligible Pending Request Queue.
- UC-10 View Request Detail for Resolution.
- UC-11 Approve a Pending Request.
- UC-12 Reject a Pending Request.
- UC-13 Deactivate an Approved Request Before Its Start Date.
- UC-14 View Resolution History.
- UC-15 View the Approver Calendar.
- UC-16 Cancel an Unresolved Pending Request by Timeout.
- UC-17 Execute Monthly Vacation Accrual.
- UC-18 View Organization-Wide Vacation Requests.
- UC-19 View the Organizational Vacation Calendar.
- UC-20 View Vacation Balances and Movements.
- UC-21 View Relevant Audit Records.
- UC-22 Manage Approver Resolution Capability.

### Main Authorization Rules

- Deny by default.
- Protected operations require authenticated active identity.
- User access is owner-only.
- Approver access requires active account, Approver role, `canResolveRequests=true`, non-ownership where applicable, eligible request state, and service-layer revalidation at execution time.
- HR read access is organization-wide but read-only.
- HR may toggle `canResolveRequests` only for identities that already hold Approver.
- HR cannot assign/remove roles and cannot resolve requests.
- Calendar route scopes are separate:
  - User `/calendario`: personal authorized scope.
  - Approver `/calendario`: approved anonymized scope for eligible Approvers.
  - HR `/rrhh/calendario`: global organization-wide read-only scope.

### Explicit Exclusions

Personal Leave, Medical Leave, non-Vacation leave types, hourly requests, half-day requests, attachments, holiday-calendar integration, per-user time zones, teams, approval hierarchy, delegation, escalation, SSO, automatic password recovery, public APIs, payroll integration, external calendar integration, external enterprise integrations, advanced reports, analytics, KPIs, AI simulations, conversational bots, User cancellation of Pending requests, partial cancellation, post-start deactivation, editing Approved dates, HR request resolution, HR balance modification, HR role assignment, and HR role removal.

### Required Demo Scenarios

- Successful User login and failed login.
- Multi-role context switching without permission mutation.
- Create Vacation request in both input modes.
- Missing data, reversed date range, past/today start, weekend-only range, overlap, and insufficient-balance failures.
- Pending edit success and stale edit conflict.
- Owner-only request detail and forced-browsing denial.
- Balance cards and movement timeline.
- User calendar personal scope.
- Approver queue with global eligible non-owned Pending requests.
- Disabled Approver immediate denial after HR capability toggle.
- Approver detail with positive and negative projected balance.
- Approval success and negative projected-balance rejection.
- Rejection success and invalid rejection reason.
- Concurrent approval conflict.
- Timeout cancellation and timeout-versus-approval race.
- Pre-start deactivation success, post-start denial, partial-deactivation denial.
- Monthly accrual and duplicate accrual idempotency.
- HR request list/detail read-only behavior.
- HR global calendar only at `/rrhh/calendario`.
- HR balance list/movements read-only behavior.
- HR audit log with redaction.
- HR capability enable/disable success, missing reason failure, stale row-version conflict, non-Approver target failure.
- HR forbidden approve/reject/deactivate/balance/role actions.

## Final Self-Check Before Returning

Before producing the prototype, verify:

- UC-01 through UC-22 are included.
- User, Approver, HR, and System are included.
- All five request states are included.
- `/rrhh/calendario` is the only HR global-calendar route.
- HR request resolution is prohibited.
- `canResolveRequests=true` is required for Approver approval queues, details, calendar-as-Approver, and resolution operations.
- FR-023 and BR-036 through BR-038 are included.
- Concurrency scenarios are included.
- Audit behavior is included.
- Explicit exclusions are included.
- Accessibility is included.
- Traceability is included.
- No placeholder remains.
- No feature was invented.
- The result is a functional prototype, not static screens.
