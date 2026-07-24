# NovaLeave MVP — Implementation Prompt for Google AI Studio

## Context Summary

You are tasked with implementing **NovaLeave**, a vacation request management system built as an ASP.NET Core MVC application with Razor Views, Bootstrap 5.3.x, and ASP.NET Core Identity. The system follows Clean Architecture with four layers: Domain, Application, Infrastructure, and Presentation.

### Technology Stack (MANDATORY)
- **Runtime**: .NET 10 / C#
- **Presentation**: ASP.NET Core MVC with Controllers + Razor Views
- **UI Framework**: Bootstrap 5.3.x (pinned, no CDN without CSP + SRI)
- **Persistence**: Entity Framework Core + SQL Server
- **Validation**: FluentValidation (server-side authoritative) + Bootstrap client-side hints
- **Authentication**: ASP.NET Core Identity with cookie authentication
- **Authorization**: Policy-based + resource-based; roles as claims used by policies but NEVER replace ownership/hierarchy checks
- **Testing**: xUnit, WebApplicationFactory (integration), Playwright (E2E)
- **Observability**: Serilog, structured logging, correlation IDs, audit trails

### Architecture Rules (Constitution v6.0.0)
1. **Clean Architecture**: Dependencies point inward (Domain ← Application ← Infrastructure ← Presentation)
2. **Rich Domain**: Business rules in entities/aggregates; Application coordinates only
3. **Single Source of Truth**: Each rule in exactly one place (Domain for business, Application for orchestration)
4. **Test-First**: Domain invariants = unit tests; critical flows = integration tests; key journeys = E2E
5. **Security by Design**: Server-authoritative everything; deny-by-default; fail-closed; antiforgery on all mutations
6. **No Emojis**: Never in user-facing text (navigation, buttons, cards, statuses, alerts, modals, login, Approver/HR views)
7. **No Gradients**: Bootstrap variables/utilities only; extensions in `wwwroot/css`
8. **WCAG 2.1 AA**: Semantic HTML, keyboard navigation, focus visible, contrast ≥4.5:1, ARIA live for updates
9. **Reduced Motion**: Respect `prefers-reduced-motion` (animations ≤1ms)
10. **Spanish UI**: User-visible text in Spanish; technical identifiers in English

---

## Specifications (Authoritative Sources — Read in This Order)

### 1. Constitution v6.0.0 (`.specify/memory/constitution.md`)
Governance, architecture, security, roles, lifecycle invariants, testing, operations.
- **Actors**: `User`, `Approver`, `HR` (combinable); all have `Active`/`Inactive` status
- **HR**: Org-wide read access (requests, calendar, balances, audit) + Approver capability management (activate/deactivate `canResolveRequests` for existing Approvers with reason, confirmation, row version, optimistic concurrency, audit). HR MUST NOT: approve/reject/deactivate requests, modify balances, assign/remove roles.
- **Mutual Exclusivity**: Requesting and approving are mutually exclusive per resource
- **Lifecycle**: `Pending` → `Approved`/`Rejected`/`CancelledByTimeout`; `Approved` → `CancelledByApprover` (pre-start only)
- **Balance**: Global, accrues 1 day/completed month, non-expiring; Pending reserves; Approval deducts; Deactivation restores
- **Working Days**: Mon–Fri inclusive; weekends excluded; holidays counted as working days (no holiday calendar in MVP)
- **Two Input Modes**: Date range OR start date + working days count; server-normalized to one authoritative range + total

### 2. Feature Spec v2.0 (`specs/001-leave-management-mvp/spec.md`)
Complete EARS requirements, acceptance scenarios, traceability matrix.
- **Key Requirements** (prototype review):
  - FR-023: Projected available balance after approval (server-calculated: `available - requestedDays`)
  - FR-024: Calendar-to-detail navigation per role authorization
  - FR-025: Expose Newly Created Pending Requests to Eligible Approvers (new — creation-to-queue visibility)
  - BR-036: Projected balance calculation formula
  - BR-037: Deny approval when projected balance negative
  - BR-038: Revalidate projected balance on approval POST
  - CON-012: Mutually exclusive approve/reject transitions (UI + server)

### 3. Frontend Design Spec v1.3.0 (`specs/001-leave-management-mvp/frontend-design-spec.md`)
Design tokens, components, motion, accessibility, interaction rules.
- **§6.6**: Global Header & Layout — brand, context switcher "Mis roles", user menu with **Cerrar sesión**, login account switcher
- **§13**: Branded login screen (NovaLeave logo, navy-900/blue-600, white card, accessible, no emojis/gradients)
- **§14**: Context selector "Mis roles" (dropdown/listbox, options: Mi espacio / Aprobaciones / RRHH; hidden when <2 roles; no session/claims modification)
- **§15**: Navigation labels — `Mi historial` (was `Mi saldo`), `Historial` (was `Historial y Desactivación`), context button `Mis roles`
- **§16**: Balance card terminology — `Acumulado total`, `Pendientes`, `Días gozados`, `Disponible` (removed: `Devengado`, `Reservado`, `Deducido`)
- **§17**: Projected balance in Approver views — `Disponible actual`, `Días solicitados`, `Disponible después de aprobar`; warning + disable Approve when negative
- **§18**: Two input modes only — "Fecha inicio + Fecha fin" / "Fecha inicio + Cantidad de días" (no "Modo A/B")
- **§19**: Weekend exclusion — Mon–Fri only; Fri+1→Mon; weekend-only = 0 days rejected
- **§20**: Approver UI improvements — hierarchy, spacing, badges, confirmation dialogs, loading, empty states, responsive
- **§21**: HR UI improvements — read-only badges, tables/pagination, calendar with names, capability modal (reason 10–500, confirmation, row version)
- **§22**: Calendar-to-detail navigation — keyboard accessible (Enter/Space), per-role targets, anonymized calendars hide links
- **§23**: Mutually exclusive approve/reject — UI disables conflicting action on submit; server revalidates with optimistic concurrency
- **§24**: Create-to-Queue Presentation Feedback — User creation success toast; Approver queue visibility on refresh/open without manual sync
- **§25**: Traditional month-view calendar — grid, weekday headers, day cells, weekend exclusion, event bars, keyboard nav, responsive, reduced motion
Route mapping, view inventory, navigation, accessibility checklists, acceptance criteria RBFV-001 through RBFV-034.
- **Routes**: `/mis-solicitudes*`, `/saldo` (labeled `Mi historial`), `/calendario` (shared), `/aprobaciones*`, `/rrhh*`
- **Context Switcher**: "Mis roles" — visible only with 2+ roles; options per held roles
- **View Accessibility**: Per-view checklists (§8.1–8.15) including projected balance columns, keyboard calendar nav, read-only HR indicators
- **Authorization Policies**: `RequireActiveUser`, `RequireActiveApprover`, `RequireActiveHR`, `RequireApproverEligible`, `RequireApproverNotOwner`, `RequirePreStartDeactivation`, `RequireHRForApproverManagement`
- **New Acceptance Criterion**: RBFV-034 — New eligible Pending request appears in `/aprobaciones` exactly once without manual sync; Approver queue excludes only owning Approver and ineligible states, never team/hierarchy/department/assigned-approver

---

## Open Questions (DO NOT IMPLEMENT Until Resolved)
- **OQ-002**: Completed-month semantics for accrual (calendar vs anniversary boundaries)
- **OQ-003**: Inactive User operations (create/edit requests?)
- **OQ-004**: Mandatory reason for Approver deactivation of approved request
- **OQ-005**: Calendar scope (own only vs anonymized org-wide for User)

---

## Implementation Task: Create the Complete NovaLeave Solution

### Phase 0: Solution Structure
Create the standard Clean Architecture folder structure:
```
src/
  NovaLeave.Domain/
  NovaLeave.Application/
  NovaLeave.Infrastructure/
  NovaLeave.Presentation.Web/
    Controllers/
    Views/
      Shared/
    ViewModels/
    Filters/
    TagHelpers/
    Areas/
    wwwroot/
      css/
      js/
      lib/
tests/
  NovaLeave.UnitTests/
  NovaLeave.IntegrationTests/
  NovaLeave.EndToEndTests/
docs/
  adr/
  diagrams/
docker/
.github/
```

### Phase 1: Domain Layer (NovaLeave.Domain)
**Entities & Value Objects:**
- `User` (Id, IdentityId, Roles[], ActiveStatus, EmploymentStartDate, Balance, NegativeBalanceCarryForward)
- `VacationRequest` (Id, OwnerId, InputMode, StartDate, EndDate, WorkingDaysTotal, Reason, ExcessJustification, State, Version, Reservation, ApprovalMetadata, RejectionMetadata, TimeoutMetadata, DeactivationMetadata, EscalationMetadata, HRAuthorizationMetadata)
- `VacationBalance` (UserId, AccruedDays, DeductedDays, ReservedDays, AvailableDays computed, NegativeBalanceCarryForward)
- `LeaveType` (Id, Name, IsActive, ConsumesBalance) — MVP only `Vacation`
- `RequestState` enum: `Pending`, `PendingEscalated`, `EscalatedToHR`, `PendingAuthorizedExcess`, `Approved`, `ApprovedEscalated`, `Rejected`, `CancelledByTimeout`, `CancelledByApprover`
- `AuditRecord` (immutable)
- `SecurityEvent` (immutable)
- `SystemParameter` (TimeoutDays, SessionLifetime)

**Domain Services:**
- `WorkingDayCalculator` — authoritative Mon–Fri count, weekend exclusion, holiday inclusion (no calendar)
- `BalanceService` — accrual (1 day/completed month), reservation, deduction, restoration, available calculation, **negative balance recovery from accruals**
- `OverlapValidator` — inclusive range intersection against Pending/Approved same owner
- `StateTransitionValidator` — validates all transitions per invariants (BR-006, BR-007, BR-027, BR-028, BR-043, BR-044, BR-045)

**Domain Events:** `RequestCreated`, `RequestEdited`, `RequestApproved`, `RequestRejected`, `RequestCancelledByTimeout`, `RequestCancelledByApprover`, `BalanceAccrued`, `BalanceDeducted`, `BalanceRestored`, `ApproverCapabilityToggled`, `RequestEscalatedToHR`, `HRAuthorizedExcess`, `HRRejectedExcess`, `AccrualRecoveredNegativeBalance`

### Phase 2: Application Layer (NovaLeave.Application)
**Vertical Slices (feature folders):**
```
LeaveRequests/
  Create/
    CreateVacationRequestCommand
    CreateVacationRequestHandler
    CreateVacationRequestValidator
    CreateVacationRequestResponse
  Edit/
    EditVacationRequestCommand
    EditVacationRequestHandler
    EditVacationRequestValidator
  Approve/
    ApproveVacationRequestCommand
    ApproveVacationRequestHandler
    ApproveVacationRequestValidator  // includes projected balance revalidation (BR-038)
  Reject/
    RejectVacationRequestCommand
    RejectVacationRequestHandler
    RejectVacationRequestValidator
  Deactivate/
    DeactivateApprovedRequestCommand
    DeactivateApprovedRequestHandler
    DeactivateApprovedRequestValidator
  Escalate/
    EscalateRequestToHRCommand
    EscalateRequestToHRHandler
    EscalateRequestToHRValidator
  HRAuthorize/
    HRAuthorizeExcessCommand
    HRAuthorizeExcessHandler
    HRAuthorizeExcessValidator
  HRReject/
    HRRejectExcessCommand
    HRRejectExcessHandler
    HRRejectExcessValidator
  Get/
    GetMyRequestsQuery
    GetRequestDetailQuery
    GetPendingRequestsForApproverQuery
    GetRequestDetailForApproverQuery
    GetRequestDetailForHRQuery
LeaveBalances/
  GetMyBalanceQuery
  GetMyBalanceHandler
  GetAllBalancesQuery (HR)
  GetBalanceMovementsQuery (HR)
  GetAccrualRecoveryPlanQuery (HR + User)
HR/
  GetApproversListQuery
  ToggleApproverCapabilityCommand
  ToggleApproverCapabilityHandler
  ToggleApproverCapabilityValidator
  GetAuditLogQuery
System/
  RunTimeoutCancellationCommand
  RunMonthlyAccrualCommand
  RunAccrualRecoveryCommand
```

**Cross-Cutting:**
- `ITimeProvider` abstraction for testable time
- `ICurrentUserService` for identity/role/active-status
- FluentValidation pipeline with `ValidateAsync`
- Optimistic concurrency via row version on all mutations
- Audit logging decorator/behavior

### Phase 3: Infrastructure Layer (NovaLeave.Infrastructure)
- **EF Core**: `NovaLeaveDbContext` with configurations for all entities
- **Migrations**: Versioned, no `EnsureCreated()`
- **Repositories**: Implement Application contracts (not generic repository pattern unless justified)
- **Identity**: `ApplicationUser` extends `IdentityUser`; link to Domain `User` via stable `IdentityId`
- **Services**: `TimeProvider` implementation, email sender (stub), background job scheduler (timeout/accrual)
- **Audit/Security Event Persistence**: Immutable tables, redacted payloads
- **Demo User Seeding** (opt-in, `NovaLeave:SeedDemoUsers=true`): `IDbInitializer` / `IHostedService` that seeds 4 identities at startup:
  - `demo.user@novaleave.test` — User, Active
  - `demo.approver@novaleave.test` — Approver (canResolveRequests=true), Active
  - `demo.rrhh@novaleave.test` — HR, Active
  - `demo.combo@novaleave.test` — User + Approver (canResolveRequests=true) + HR, Active
  - All: password `Demo123!`, employment start date = 12 months prior (ensures ≥12 accrued days), `Active=true`

### Phase 4: Presentation Layer (NovaLeave.Presentation.Web)

#### 4.1 Controllers (Thin — HTTP → Application → ViewModel/Redirect)
```
MisSolicitudesController      [RequireActiveUser]
  GET    /mis-solicitudes           → MyRequestsViewModel
  GET    /mis-solicitudes/crear     → CreateRequestViewModel (dual-mode form)
  POST   /mis-solicitudes/crear     → CreateVacationRequestCommand
  GET    /mis-solicitudes/{id}      → RequestDetailViewModel (owner)
  GET    /mis-solicitudes/{id}/editar → EditRequestViewModel
  POST   /mis-solicitudes/{id}/editar → EditVacationRequestCommand

SaldoController               [RequireActiveUser]
  GET    /saldo                     → BalanceViewModel (labels: Acumulado total, Pendientes, Días gozados, Disponible)

CalendarioController          [RequireActiveUser|Approver|HR]
  GET    /calendario               → CalendarViewModel (role-aware events)

AprobacionesController        [RequireActiveApprover]
  GET    /aprobaciones             → PendingRequestsViewModel (with projected balance columns)
  GET    /aprobaciones/{id}        → ApproverRequestDetailViewModel (projected balance card, **actions vary by state**: Pending=Approve/Reject/Deactivate; PendingEscalated=Escalar a RRHH/Rechazar; PendingAuthorizedExcess=Approve/Reject)
  POST   /aprobaciones/{id}/aprobar   → ApproveVacationRequestCommand (mutually exclusive, revalidate projected)
  POST   /aprobaciones/{id}/rechazar  → RejectVacationRequestCommand (reason required 10-500)
  POST   /aprobaciones/{id}/desactivar → DeactivateApprovedRequestCommand (pre-start only)
  POST   /aprobaciones/{id}/escalar   → EscalateRequestToHRCommand (reason required 10-500, confirmation)
  GET    /aprobaciones/historial   → ApproverHistoryViewModel

RRHHController                [RequireActiveHR]
  GET    /rrhh                     → HRDashboardViewModel
  GET    /rrhh/solicitudes         → HRRequestListViewModel (filterable, paginated)
  GET    /rrhh/solicitudes/{id}    → HRRequestDetailViewModel (read-only, projected balance; **actions for EscalatedToHR: Autorizar exceso / Rechazar exceso**)
  POST   /rrhh/solicitudes/{id}/autorizar-exceso → HRAuthorizeExcessCommand (reason 10-500, confirmation, row version)
  POST   /rrhh/solicitudes/{id}/rechazar-exceso → HRRejectExcessCommand (reason 10-500, confirmation)
  GET    /rrhh/calendario          → HRCalendarViewModel (org-wide, names visible)
  GET    /rrhh/saldos              → HRBalanceListViewModel (cards: Acumulado total, Pendientes, Días gozados, Disponible; **NegativeBalanceCarryForward visible**)
  GET    /rrhh/saldos/{userId}     → HRBalanceMovementsViewModel (**AccrualRecovery movements shown**)
  GET    /rrhh/auditoria           → HRAuditLogViewModel
  GET    /rrhh/aprobadores         → HRApproverListViewModel
  GET    /rrhh/aprobadores/{id}/capacidad → ApproverCapabilityViewModel (GET shows modal data)
  POST   /rrhh/aprobadores/{id}/capacidad → ToggleApproverCapabilityCommand (reason, confirmation, row version)
```

#### 4.2 Views (Razor) — Per Frontend Design Spec v1.3.0
**Shared:**
- `_Layout.cshtml` — header with context switcher "Mis roles" (§14), navigation per role (§15), antiforgery, CSP
- `_ValidationScriptsPartial.cshtml` — unobtrusive + custom JS for dual-mode switching, calendar nav
- `_StatusBadge.cshtml` — semantic badges with text (no color-only)
- `_BalanceCards.cshtml` — four cards with exact labels (§16)
- `_ProjectedBalanceCard.cshtml` — Approver/HR detail (§17)
- `_Calendar.cshtml` — **traditional month grid, weekend columns muted, event bars split at weekends, keyboard navigation per §25**
- `_ConfirmationModal.cshtml` — accessible modal, focus trap, aria-modal
- `_Toast.cshtml` — accessible toast (≤200ms, aria-live)

**User Context (`Mi espacio`):**
- `MisSolicitudes/Index.cshtml` — table with **server-side pagination** (page size selector, first/prev/next/last, `aria-label` controls, total count via `aria-live="polite"`), empty state, create button
- `MisSolicitudes/Create.cshtml` — dual-mode radio group (§18), date inputs, working-days preview (aria-live), balance impact, submit; **when requested days > available: conditional `Justificación de exceso` field (10–500 chars, validation per §18.1), creates `PendingEscalated`**
- `MisSolicitudes/Edit.cshtml` — pre-filled dual-mode, version token, revalidation
- `MisSolicitudes/Detail.cshtml` — status badge, dates, days, reason, balance impact, audit timeline, edit link (Pending+owner only); **for `PendingEscalated`: shows `Exceso de saldo` badge, excess count, justification**
- `Saldo/Index.cshtml` — four summary cards (§16), movements timeline, reservation breakdown (dl); **when `NegativeBalanceCarryForward > 0`: recovery plan card showing months to zero**

**Approver Context (`Aprobaciones`):**
- `Aprobaciones/Index.cshtml` — table with requester, dates, days, **Disponible actual** (authoritative available **excluding current request's reservation**), **Días solicitados**, **Disponible después de aprobar** (= Disponible actual − Días solicitados), overlap warning, **state badges: `Pendiente` / `Exceso de saldo` / `Exceso autorizado`**, actions; **server-side pagination** (page size selector, first/prev/next/last, `aria-label`, total via `aria-live="polite"`); responsive cards on mobile
- `Aprobaciones/Detail.cshtml` — full detail **actions vary by state**:
  - `Pending`/`PendingAuthorizedExcess`: projected balance card, approve/reject/deactivate (pre-start)
  - `PendingEscalated`: `Exceso de saldo` badge, `Disponible actual`, `Días solicitados`, `Exceso`, `Justificación del solicitante`, **Escalar a RRHH** (primary, modal with reason 10–500 + confirmation), **Rechazar** (secondary, standard rejection), Approve disabled
- `Aprobaciones/Historial.cshtml` — resolved requests table with **server-side pagination** (page size selector, first/prev/next/last, `aria-label`, total via `aria-live="polite"`)

**HR Context (`RRHH`):**
- `RRHH/Index.cshtml` — dashboard summary cards
- `RRHH/Solicitudes.cshtml` — filterable **server-side paginated table** (page size selector, first/prev/next/last, `aria-label`, total via `aria-live="polite"`), row click → detail
- `RRHH/Detalle.cshtml` — **actions vary by state**:
  - Standard requests: read-only mirror of Approver detail, `Solo lectura` badge, no action buttons
  - `EscalatedToHR`: `Exceso de saldo` badge, `Disponible actual`, `Días solicitados`, `Exceso`, `Justificación del solicitante`, **Autorizar exceso** (primary, modal with authorized excess count, reason 10–500, confirmation, row version → transitions to `PendingAuthorizedExcess`), **Rechazar exceso** (secondary, modal with reason 10–500, confirmation → transitions to `Pending` with requested days = available balance), Approve/Reject/Deactivate hidden
- `RRHH/Calendario.cshtml` — org calendar with requester names, keyboard nav to `/rrhh/solicitudes/{id}`
- `RRHH/Saldos.cshtml` — user table with four balance cards per user, **server-side pagination** (page size selector, first/prev/next/last, `aria-label`, total via `aria-live="polite"`); **shows `NegativeBalanceCarryForward` badge and recovery timeline**
- `RRHH/Movimientos.cshtml` — timeline with date, concept, amount, resulting balance; **includes `AccrualRecovery` movements reducing carry-forward**
- `RRHH/Auditoria.cshtml` — filterable audit table with **server-side pagination** (page size selector, first/prev/next/last, `aria-label`, total via `aria-live="polite"`), redacted payloads, sortable headers
- `RRHH/Aprobadores.cshtml` — table with toggle `canResolveRequests`, **server-side pagination** (page size selector, first/prev/next/last, `aria-label`, total via `aria-live="polite"`), modal with reason (10-500), confirmation, row version check
- `RRHH/Capacidad.cshtml` — capability toggle form (GET loads modal, POST executes)

**Auth:**
- `Identity/Account/Login.cshtml` — branded per §13 (NovaLeave logo, navy-900/blue-600, white card, accessible labels, autocomplete, preserved email on failure, no emojis/gradients)
- **Login screen account switcher (`Cuenta` dropdown)** — displays seeded demo identities (`demo.user@novaleave.test`, `demo.approver@novaleave.test`, `demo.rrhh@novaleave.test`, `demo.combo@novaleave.test`, password `Demo123!`); selecting one pre-fills email without authenticating; opt-in via config `NovaLeave:SeedDemoUsers=true` (disabled in Production)

#### 4.3 Client-Side (wwwroot/js)
- `context-switcher.js` — "Mis roles" dropdown, no session modification
- `dual-mode-form.js` — radio group shows/hides fields, preserves values
- `calendar-keyboard-nav.js` — arrow keys, Enter/Space activation, role-aware target URLs
- `projected-balance.js` — UI preview (informational only), matches server calc
- `confirmation-modal.js` — focus trap, aria-modal, Esc close, return focus
- `loading-buttons.js` — disable + spinner, width preserved, prevent double-submit
- `toast.js` — accessible toasts, aria-live polite
- `char-counter.js` — rejection reason live count

#### 4.4 Styles (wwwroot/css)
- `site.css` — extends Bootstrap variables: `--nl-color-navy-900`, `--nl-color-blue-600`, `--nl-color-background`, `--nl-color-surface`, `--nl-color-border`, `--nl-color-error`, `--nl-color-warning`, `--nl-color-success`, spacing scale (4/8/12/16/24), radius, shadows (`nl-card`)
- Reduced motion media query per §8
- No gradients, no emojis

### Phase 5: Testing (Mandatory)

#### Unit Tests (NovaLeave.UnitTests)
- Domain: WorkingDayCalculator (Fri+1=Mon, weekend exclusion, holiday inclusion, zero-day rejection), BalanceService (accrual, reservation, deduction, restoration, available calc, negative prevention), OverlapValidator (intersection, adjacency, terminal exclusion), StateTransitionValidator (all valid/invalid transitions)
- **Calendar**: CalendarViewModel builder (month grid generation, weekend column handling, event bar splitting across weekends, max 3 visible + overflow), keyboard nav logic (weekday skip, month wrapping)
- Application: Validators (command/query), handlers (success/failure/concurrency)

#### Integration Tests (NovaLeave.IntegrationTests)
- WebApplicationFactory with real EF Core + SQL Server (Testcontainers or local)
- Full MVC pipeline: routing, model binding, validation, authorization, antiforgery, rendered responses
- Scenarios: AC-001 through AC-059, AC-HR-001 through AC-HR-009, all concurrency/concurrency tests

#### E2E Tests (NovaLeave.EndToEndTests)
- Playwright critical journeys:
  1. User: login → create request (both modes) → view balance → edit pending → calendar nav
  2. Approver: login → list pending → view detail with projected balance → approve (success) → reject (with reason) → deactivate pre-start → escalate excess to HR → approve authorized excess
  3. HR: login → dashboard → request list → detail (read-only) → calendar → balances → movements → audit → approver capability toggle (reason, confirmation, concurrency conflict) → authorize/reject excess days → view accrual recovery plan
  4. Security: cross-user IDOR, self-resolution, inactive approver, expired session, antiforgery missing, HR attempting resolution/balance edit/role assignment
  5. Excess flow: user submits 10-day request with 5 available balance + justification → approver sees Exceso de saldo badge, escalates with reason → HR authorizes excess with reason → approver approves authorized excess → user balance shows negative with carry-forward → monthly accrual reduces carry-forward → user cannot create new requests until carry-forward zero

#### Accessibility Tests
- axe-core automated on every view
- Manual keyboard audit per §8 checklists
- Reduced motion verification (DevTools emulation)

---

## Acceptance Criteria (Must All Pass)

### Constitution Compliance
- [ ] Clean Architecture layering verified (no inward dependency violations)
- [ ] Domain contains all business rules; Application coordinates only
- [ ] Server-authoritative calculations (working days, balance, projected balance, overlap)
- [ ] Optimistic concurrency on all mutations (row version)
- [ ] Audit on every creation, edit, transition, capability toggle, timeout
- [ ] Security events on denials, inactive actors, self-resolution, HR violations
- [ ] Redaction of reasons in logs/traces/audit payloads
- [ ] Antiforgery on all browser mutations
- [ ] Session expiration + expired-session rejection
- [ ] Deny-by-default policies per role/resource/state

### Feature Spec (spec.md)
- [ ] All FR, VAL, BR, AUTHZ, SEC, CON, AUD, ERR, CFG requirements implemented
- [ ] All acceptance scenarios (AC-001 through AC-HR-013 + AC-058, AC-059, AC-060, **AC-061 through AC-068**) passing
- [ ] Projected balance: server-calculated `available - requestedDays` (FR-023, BR-036, BR-037, BR-038)
- [ ] Calendar-to-detail nav per role authorization (FR-024, AUTHZ-017, AUTHZ-018, AUTHZ-019)
- [ ] Mutually exclusive approve/reject (CON-012, UI + server)
- [ ] Creation-to-queue visibility: new eligible Pending request appears in `/aprobaciones` exactly once without manual sync (FR-025, AC-058)
- [ ] User-scoped overlap: different users may overlap; same user rejected (BR-019, AC-059)
- [ ] **Excess request flow**: User submits request exceeding balance with justification → created as `PendingEscalated` → Approver sees `Exceso de saldo` badge, escalates to HR with reason → HR authorizes/rejects excess → Approver approves authorized excess → negative balance with `NegativeBalanceCarryForward` → monthly accrual reduces carry-forward → new requests blocked until carry-forward zero (FR-026, FR-027, FR-028, FR-029, BR-043–BR-049, AC-061–AC-068)

### Frontend Design Spec (frontend-design-spec.md)
- [ ] Login screen branded per §13
- [ ] Context selector "Mis roles" per §14
- [ ] Navigation labels per §15 (Mi historial, Historial, Mis roles)
- [ ] Balance cards per §16 (exact four labels)
- [ ] Projected balance in Approver/HR per §17
- [ ] Two input modes per §18 (no "Modo A/B")
- [ ] Weekend exclusion per §19 (server + UI preview)
- [ ] Approver UI per §20 (hierarchy, spacing, badges, confirmations, loading, empty, responsive)
- [ ] HR UI per §21 (read-only badges, no actions, pagination, calendar names, capability modal)
- [ ] Calendar keyboard nav per §22
- [ ] Mutually exclusive actions per §23
- [ ] Create-to-Queue feedback per §24
- [ ] **Traditional month-view calendar per §25 (grid, weekday headers, day cells, weekend exclusion, event bars, keyboard nav, responsive, reduced motion)**
- [ ] No emojis anywhere (§10.3 last bullet)
- [ ] No gradients (§4.3, §10.2)
- [ ] Reduced motion respected (§9, §10.7)
- [ ] WCAG 2.1 AA per §9, §11.5
- [ ] Spanish UI text throughout

### Role-Based Views Spec (spec 002)
- [ ] All routes implemented with correct policies
- [ ] All views per role exist (§6.1–6.3)
- [ ] Navigation per role per §7.1–7.3
- [ ] All 33 accessibility checklists pass (§8.1–8.15)
- [ ] All 34 acceptance criteria RBFV-001 through RBFV-034 pass

---

## Quality Gates (CI Must Pass)
1. `dotnet restore` + `dotnet build` (no warnings as errors in Domain/Application)
2. `dotnet format --verify-no-changes` + `.editorconfig`
3. Roslyn analyzers + nullable analysis
4. Unit tests (Domain/Application) — coverage ≥80% on critical modules
5. Integration tests (MVC + EF Core) — all AC scenarios
6. E2E tests (Playwright) — critical journeys
7. SAST/SCA/dependency scanning
8. License verification
9. Migration validation (if schema changed)
10. Browser asset vulnerability scan
11. Mermaid diagram validation (if docs changed)

---

## Deliverables
1. Complete `src/` solution with all four projects compiling
2. Complete `tests/` with all three test projects
3. Initial EF Core migration (`InitialCreate`)
4. `docker-compose.yml` for SQL Server + app
5. `.github/workflows/ci.yml` with all quality gates
6. README with architecture overview, run instructions, test commands
7. `docs/adr/` for any architectural decisions made during implementation

---

## Important Reminders
- **Do not invent requirements** — only what's in the three specs + Constitution
- **Do not resolve open questions** (OQ-002–OQ-005) — leave as TODOs with comments referencing the OQ
- **Server is the authority** — client preview is informational only
- **Spanish for users, English for code** — ViewModels, DTOs, entities in English; .resx or View-level Spanish strings
- **No MediatR unless justified** — native handlers/services preferred
- **No generic repositories** — implement specific contracts
- **ViewModels per view** — never pass Domain entities to Views
- **Post/Redirect/Get** on all successful mutations
- **Row version on every mutation command** — optimistic concurrency
- **Audit record in same transaction** as business effect

---

## Reference Files (for your context):
- `.specify/memory/constitution.md` (v6.0.0)
- `specs/001-leave-management-mvp/spec.md`
- `specs/001-leave-management-mvp/frontend-design-spec.md`
- `specs/002-role-based-frontend-views/spec.md`

Implement the complete solution. Start with Phase 0-1 (structure + Domain), then proceed iteratively through Application → Infrastructure → Presentation → Tests. Run quality gates after each phase.