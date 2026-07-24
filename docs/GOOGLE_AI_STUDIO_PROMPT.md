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
- **Key Requirements** (prototype review + latest updates):
  - FR-023: Projected available balance after approval (server-calculated: `available - requestedDays`)
  - FR-024: Calendar-to-detail navigation per role authorization
  - FR-025: Expose Newly Created Pending Requests to Eligible Approvers (creation-to-queue visibility)
  - FR-026: Request Excess Vacation Days with Justification and HR Escalation
  - FR-027: HR Resolve Escalated Request
  - FR-028: Apply Accrual Recovery to Negative Balance
  - FR-029: View Accrual Recovery Plan
  - BR-036: Projected balance calculation formula (uses `authoritativeAvailableExcludingCurrentRequest`)
  - BR-037: Deny approval when projected balance negative
  - BR-038: Revalidate projected balance on approval POST
  - BR-039: Allow negative balance via HR-escalated approval
  - BR-043: Restrict PendingEscalated transitions
  - BR-044: HR Resolution of EscalatedToHR
  - BR-045: Approver Action on PendingAuthorizedExcess
  - BR-046: ApprovedEscalated Creates Negative Balance Carry-Forward
  - BR-047: Accrual Recovery Reduces Negative Balance Carry-Forward
  - BR-048: Project Available Balance with Recovery Plan
  - BR-049: Prohibit New Reservations While Negative Carry-Forward Exists
  - CON-012: Mutually exclusive approve/reject transitions (UI + server)

### 3. Frontend Design Spec v1.3.0 (`specs/001-leave-management-mvp/frontend-design-spec.md`)
Design tokens, components, motion, accessibility, interaction rules.
- **§6.6**: Global Header & Layout — brand, context switcher "Mis roles", user menu with **Cerrar sesión**, login account switcher
- **§13**: Branded login screen (NovaLeave logo, navy-900/blue-600, white card, accessible, no emojis/gradients)
- **§13.1**: Demo account switcher (`user@demo`, `approver@demo`, `hr@demo`, `multi@demo` with masked emails)
- **§14**: Context selector "Mis roles" (dropdown/listbox, options: Mi espacio / Aprobaciones / RRHH; hidden when <2 roles; no session/claims modification)
- **§15**: Navigation labels — `Mi historial` (was `Mi saldo`), `Historial` (was `Historial y Desactivación`), context button `Mis roles`
- **§16**: Balance card terminology — `Acumulado total`, `Pendientes`, `Días gozados`, `Disponible` (removed: `Devengado`, `Reservado`, `Deducido`)
- **§17**: Projected balance in Approver views — `Disponible actual`, `Días solicitados`, `Disponible después de aprobar`; warning + disable Approve when negative
- **§18**: Two input modes only — "Fecha inicio + Fecha fin" / "Fecha inicio + Cantidad de días" (no "Modo A/B")
- **§18.1**: Excess Justification Field (conditional, 10–500 chars, creates `PendingEscalated` state)
- **§19**: Weekend exclusion — Mon–Fri only; Fri+1→Mon; weekend-only = 0 days rejected
- **§20**: Approver UI improvements — hierarchy, spacing, badges, confirmation dialogs, loading, empty states, responsive
- **§20.1**: Escalated Requests (`PendingEscalated`) — `Exceso de saldo` badge, `Escalar a RRHH` action, Rechazar available, Approve disabled
- **§20.2**: Authorized Excess (`PendingAuthorizedExcess`) — `Exceso autorizado` badge, standard Aprobar/Rechazar enabled, mutually exclusive
- **§21**: HR UI improvements — read-only badges, tables/pagination, calendar with names, capability modal (reason 10–500, confirmation, row version)
- **§21**: Escalated request resolution — `Autorizar exceso` / `Rechazar exceso` with reason + confirmation; Approve → `PendingAuthorizedExcess`, Reject → `Pending` with reduced days
- **§22**: Calendar-to-detail navigation — keyboard accessible (Enter/Space), per-role targets, anonymized calendars hide links
- **§23**: Mutually exclusive approve/reject — UI disables conflicting action on submit; server revalidates with optimistic concurrency
- **§24**: Create-to-Queue Presentation Feedback — User creation success toast; Approver queue visibility on refresh/open without manual sync
- **§25**: Traditional month-view calendar — grid, weekday headers, day cells, weekend exclusion, event bars, keyboard nav, responsive, reduced motion

### 4. Role-Based Frontend Views Spec v2.0 (`specs/002-role-based-frontend-views/spec.md`)
Route-to-role mapping, views, navigation, accessibility checklists, acceptance criteria RBFV-001 through RBFV-034.
- **Routes**: `/mis-solicitudes*`, `/saldo` (labeled `Mi historial`), `/calendario` (shared), `/aprobaciones*`, `/rrhh*`
- **Context Switcher**: "Mis roles" — visible only with 2+ roles; options per held roles
- **View Accessibility**: Per-view checklists (§8.1–8.17) including projected balance columns, keyboard calendar nav, read-only HR indicators
- **Authorization Policies**: `RequireActiveUser`, `RequireActiveApprover`, `RequireActiveHR`, `RequireApproverEligible`, `RequireApproverNotOwner`, `RequirePreStartDeactivation`, `RequireHRForApproverManagement`
- **Server-side pagination** with accessible controls (page size selector, first/prev/next/last, aria-label, aria-live totals) on 7 list views
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
  GET    /rrhh/solicitudes         → HRRequestsViewModel (filterable, paginated)
  GET    /rrhh/solicitudes/{id}    → HRRequestDetailViewModel (read-only; for EscalatedToHR: shows Autorizar exceso / Rechazar exceso)
  POST   /rrhh/solicitudes/{id}/autorizar-exceso → HRAuthorizeExcessCommand
  POST   /rrhh/solicitudes/{id}/rechazar-exceso → HRRejectExcessCommand
  GET    /rrhh/calendario          → HRCalendarViewModel
  GET    /rrhh/saldos              → HRBalancesViewModel
  GET    /rrhh/saldos/{userId}     → HRBalanceMovementsViewModel
  GET    /rrhh/auditoria           → HRAuditLogViewModel
  GET    /rrhh/aprobadores         → HRApproversListViewModel
  GET+POST /rrhh/aprobadores/{id}/capacidad → ToggleApproverCapability (modal, reason 10-500, confirmation, row version)
```

#### 4.2 Views & Shared Layout
- **`_Layout.cshtml`**: Semantic `<header>` with brand, "Mis roles" context switcher (conditional), user menu with "Cerrar sesión" (POST to `/Identity/Account/Logout`)
- **Login** (`/Identity/Account/Login`): Brand card, demo account switcher (`Cuenta` dropdown with masked emails), accessible, no emojis/gradients
- **Traditional month-view calendar** (`_Calendar.cshtml` partial shared): 7-col grid, Mon–Sun headers, weekend columns muted/no events, keyboard nav (arrow keys skip weekends), event bars for Approved only, role-aware detail navigation
- **Server-side pagination** on 7 list views: `Mis Solicitudes`, `Pendientes`, `Historial`, `Solicitudes RRHH`, `Saldos RRHH`, `Aprobadores RRHH`, `Auditoría RRHH` — page size selector (10/25/50), first/prev/next/last with ellipsis, `aria-label`, `aria-live="polite"` totals
- **Balance cards** (User + HR): `Acumulado total`, `Pendientes`, `Días gozados`, `Disponible`; recovery plan card when `NegativeBalanceCarryForward > 0`
- **Projected balance cards** (Approver list/detail, HR detail): `Disponible actual` (excludes current request), `Días solicitados`, `Disponible después de aprobar` — warning + disable Approve when negative
- **Status badges**: Semantic colors with text; `Exceso de saldo` (warning), `Exceso autorizado` (info), `Solo lectura` (HR views)
- **Mutually exclusive approve/reject**: UI disables conflicting action on POST; server revalidates with row version
- **Confirmation modals**: Accessible (`role="dialog"`, `aria-modal="true"`, focus trap) for Reject, Deactivate, Escalate, HR capability toggle, HR excess resolution
- **Toast/alert system**: Semantic colors, loading states prevent duplicate submission, PRG on success

#### 4.3 ViewModels (Per View — Never Domain Entities)
- `MyRequestsViewModel`, `CreateRequestViewModel` (dual mode, excess justification conditional), `EditRequestViewModel`, `RequestDetailViewModel`, `BalanceViewModel`, `CalendarViewModel`, `PendingRequestsViewModel` (with projected), `ApproverRequestDetailViewModel`, `ApproverHistoryViewModel`, `HRDashboardViewModel`, `HRRequestsViewModel`, `HRRequestDetailViewModel`, `HRCalendarViewModel`, `HRBalancesViewModel`, `HRBalanceMovementsViewModel`, `HRAuditLogViewModel`, `HRApproversListViewModel`, `ToggleApproverCapabilityViewModel`

---

## Phase 5: Testing & Quality Gates

### 5.1 Test Organization
```
tests/
  NovaLeave.UnitTests/
    Domain/
      WorkingDayCalculatorTests.cs
      BalanceServiceTests.cs (incl. negative carry-forward + accrual recovery)
      OverlapValidatorTests.cs
      StateTransitionValidatorTests.cs
      UserTests.cs
      VacationRequestTests.cs
    Application/
      CreateVacationRequestHandlerTests.cs
      ApproveVacationRequestHandlerTests.cs (incl. BR-038 revalidation)
      EscalateRequestToHRHandlerTests.cs
      HRAuthorizeExcessHandlerTests.cs
      ToggleApproverCapabilityHandlerTests.cs (concurrency + audit)
  NovaLeave.IntegrationTests/
    Controllers/
      MisSolicitudesControllerTests.cs
      AprobacionesControllerTests.cs
      RRHHControllerTests.cs
    Infrastructure/
      NovaLeaveDbContextTests.cs
      MigrationTests.cs
      DemoUserSeederTests.cs
    Application/
      AccrualRecoveryIntegrationTests.cs
      TimeoutCancellationIntegrationTests.cs
      ExcessRequestWorkflowIntegrationTests.cs
  NovaLeave.EndToEndTests/
    UserJourneyTests.cs (login → create → view balance → edit → calendar)
    ApproverJourneyTests.cs (queue → detail → approve/reject/escalate → history)
    HRJourneyTests.cs (dashboard → requests → escalated resolution → balances → approvers → audit)
    MultiRoleContextSwitchTests.cs
    AccessibilityTests.cs (axe-core)
    CalendarKeyboardNavTests.cs
```

### 5.2 CI Gate (`.github/workflows/ci.yml`)
Before merge, CI MUST execute and pass:
1. Restore and build
2. Formatting and `.editorconfig` checks
3. Roslyn analyzers and nullable analysis
4. Applicable unit and integration tests
5. Coverage measurement (Domain/Application ≥ 80%)
6. SAST, SCA, vulnerable-dependency scanning
7. License verification
8. Migration and modified-Mermaid validation
9. Browser asset vulnerability checks + critical MVC E2E tests when affected

High/Critical findings block merge unless formal exception with mitigation, owner, expiration.

---

## Deliverables
1. Complete `src/` solution with all four projects compiling
2. Complete `tests/` with all three test projects
3. Initial EF Core migration (`InitialCreate`)
4. `docker-compose.yml` for SQL Server + app
5. `.github/workflows/ci.yml` with all quality gates
6. README with architecture overview, run instructions, test commands
7. `docs/adr/` for any architectural decisions made during implementation
8. **This prompt** saved as `docs/GOOGLE_AI_STUDIO_PROMPT.md`

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