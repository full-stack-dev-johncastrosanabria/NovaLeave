# Role-Based Frontend Views Specification — NovaLeave MVP

**Related Feature**: `001-leave-management-mvp` (complementary specification; not an independent feature and no separate implementation plan)
**Version**: 2.1.0
**Date**: 2026-08-04
**Status**: Audited. Normative for role-based presentation; implementation deviations are recorded in §21 and are not legitimized by this document.
**Constitution**: `.specify/memory/constitution.md` v7.0.0

---

## 1. Purpose

This specification defines the role-based route, view, navigation, composition,
and interaction contracts for the NovaLeave MVP. It documents the implemented
Presentation surface for `User`, `Approver`, and `HR` while preserving the
requirements of the Constitution, the functional MVP specification, and the
approved frontend design specification.

This document MUST be used as the view-level guide for maintaining existing
screens and extending the approved MVP. It MUST NOT be used to introduce or
alter business rules, permissions, roles, routes, lifecycle states, or server
calculations.

## 2. Scope

This specification covers:

- Route-to-role mapping and role-context navigation.
- The shared application shell and authenticated header.
- The implemented view inventory for `User`, `Approver`, and `HR`.
- Login, access-denied, and branded unexpected-error presentation.
- Tables, filters, pagination, forms, summary cards, badges, alerts, modals,
  empty states, calendars, and feedback patterns.
- Reusable Razor partials and shared CSS/JavaScript conventions.
- Responsive and accessibility obligations at view level.
- Presentation-to-business boundary rules for future views.

This specification does not define:

- Domain rules, balance formulas, authorization invariants, lifecycle
  transitions, or validation policy; those remain in
  `specs/001-leave-management-mvp/spec.md` and the Constitution.
- Design-token values, typography scales, breakpoints, spacing, radii, shadows,
  or motion durations; implementations MUST reference
  `specs/001-leave-management-mvp/frontend-design-spec.md` instead of duplicating
  those values here.
- Database, infrastructure, API, deployment, or background-job behavior.

## 3. Authority and Evidence

### 3.1 Authority Order

Conflicts MUST be resolved in this order:

1. `.specify/memory/constitution.md` v7.0.0.
2. `specs/001-leave-management-mvp/spec.md`.
3. `specs/001-leave-management-mvp/frontend-design-spec.md` v1.3.0.
4. This specification.
5. Approved ADRs.
6. Code and tests as implementation evidence.

Code and tests MUST NOT override a higher-authority requirement. A conflicting
implementation is an `IMPLEMENTATION_DEVIATION`, not a new requirement.

### 3.2 Audit Evidence

This revision was derived from the current controllers, ViewModels, Razor
Views, Identity Razor Pages, shared partials, `site.css`, `site.js`, Presentation
unit tests, MVC integration tests, route/security tests, E2E smoke tests, recent
frontend commits, and the current working-tree diff.

No frontend-specific ADR changes the presentation contracts in this document.
The existing ADRs address runtime configuration, generated-artifact review, and
the manual quality gate.

### 3.3 Finding Classification

| Classification | Meaning in this audit |
|---|---|
| `ALREADY_DOCUMENTED` | The prior spec and implementation agree with higher-authority sources. |
| `DOCUMENTATION_GAP` | Valid implemented composition or interaction existed but was not described precisely. |
| `STALE_SPEC` | The prior spec described a different or unsupported presentation and has been corrected here. |
| `IMPLEMENTATION_DEVIATION` | Current code or markup conflicts with a higher-authority rule and remains listed in §21. |
| `OUT_OF_SCOPE` | The finding belongs to business, infrastructure, or future functionality and is not converted into a frontend requirement. |

## 4. Roles and Contexts

| Technical role | Spanish UI context | Context root | Presentation responsibility |
|---|---|---|---|
| `User` | `Mi espacio` | `/mis-solicitudes` | Personal requests, personal balance/history, and personal calendar. |
| `Approver` | `Aprobaciones` | `/aprobaciones` | Eligible request resolution, resolution history, and authorized Approver calendar. |
| `HR` | `RRHH` | `/rrhh` | Organization-wide read views and Approver capability management. |

An identity MAY hold more than one role. The active UI context determines the
visible navigation and target route only. It MUST NOT mutate identity, session,
roles, claims, ownership, active status, `canResolveRequests`, or authorization.

### 4.1 Context Switcher — `Mis roles`

- The switcher MUST be visible only when at least two authorized contexts are
  available to the identity.
- It MUST list only contexts the identity may enter: `Mi espacio`,
  `Aprobaciones`, and/or `RRHH`.
- `Aprobaciones` MUST NOT be offered when the Approver context is not eligible,
  including `canResolveRequests=false`.
- Selecting an option MUST navigate to that context root.
- The current context MUST be communicated with text; an icon or color MUST NOT
  be its sole indicator.
- The control MUST support keyboard use and focus management according to the
  frontend design specification.
- `/calendario?context=User` and `/calendario?context=Approver` MUST preserve the
  selected shared-calendar context for multi-role identities.

## 5. Route and View Map

### 5.1 User Routes

| Route | Method | View | Required actor |
|---|---|---|---|
| `/mis-solicitudes` | GET | `MisSolicitudes/Index` | Active `User`. |
| `/mis-solicitudes/crear` | GET, POST | `MisSolicitudes/Create` | Active `User`. |
| `/mis-solicitudes/{id}` | GET | `MisSolicitudes/Detail` | Active owning `User`. |
| `/mis-solicitudes/{id}/editar` | GET, POST | `MisSolicitudes/Edit` | Active owning `User`; mutation remains subject to approved state rules. |
| `/saldo` | GET | `MisSolicitudes/Balance` | Active `User`. |
| `/calendario?context=User` | GET | `Calendario/Index` | Active `User`. |

### 5.2 Approver Routes

| Route | Method | View or outcome | Required actor |
|---|---|---|---|
| `/aprobaciones` | GET | `Aprobaciones/Index` | Active eligible `Approver` with `canResolveRequests=true`. |
| `/aprobaciones/{id}` | GET | `Aprobaciones/Detail` | Active eligible `Approver`, not owner. |
| `/aprobaciones/{id}/aprobar` | POST | Redirect or redisplayed detail | Active eligible `Approver`, not owner. |
| `/aprobaciones/{id}/rechazar` | POST | Redirect or redisplayed detail | Active eligible `Approver`, not owner. |
| `/aprobaciones/{id}/desactivar` | POST | Redirect or redisplayed detail | Active eligible `Approver`, not owner; approved pre-start request only. |
| `/aprobaciones/historial` | GET | `Aprobaciones/History` | Active eligible `Approver`. |
| `/calendario?context=Approver` | GET | `Calendario/Approver` | Active eligible `Approver`. |

### 5.3 HR Routes

| Route | Method | View | Required actor |
|---|---|---|---|
| `/rrhh` | GET | `RRHH/Index` | Active `HR`. |
| `/rrhh/solicitudes` | GET | `RRHH/Solicitudes` | Active `HR`; read-only request data. |
| `/rrhh/solicitudes/{id}` | GET | `RRHH/SolicitudDetalle` | Active `HR`; read-only request detail. |
| `/rrhh/calendario` | GET | `RRHH/Calendario` | Active `HR`; organization calendar. |
| `/rrhh/saldos` | GET | `RRHH/Saldos` | Active `HR`; read-only balances. |
| `/rrhh/saldos/{userId}` | GET | `RRHH/Movimientos` | Active `HR`; read-only movements. |
| `/rrhh/auditoria` | GET | `RRHH/Auditoria` | Active `HR`; relevant redacted audit data. |
| `/rrhh/aprobadores` | GET | `RRHH/ApproverCapabilities/Index` | Active `HR`. |
| `/rrhh/aprobadores/{id}/capacidad` | GET, POST | `RRHH/ApproverCapabilities/Capability` | Active `HR`; approved capability-management operation only. |

HR MUST use `/rrhh/calendario`. `/calendario` MUST NOT provide or expose the HR
organization-wide calendar.

### 5.4 Shared and Identity Routes

| Route | Method | Presentation behavior |
|---|---|---|
| `/` | GET | Redirects to `/mis-solicitudes`, `/aprobaciones`, or `/rrhh` according to the identity's authorized role order; otherwise redirects to access denied. |
| `/Identity/Account/Login` | GET, POST | Identity login Razor Page. |
| `/Identity/Account/Logout` | POST | Identity logout; MUST retain antiforgery protection. |
| `/Identity/Account/AccessDenied` | GET | Branded access-denied Razor Page. |
| Central exception view | internal error result | `Views/Shared/Error.cshtml` with a correlation/reference identifier and no implementation details. |

The prior aliases `/acceso-denegado` and `/error` are not implemented public
routes and are removed from the route contract.

## 6. Shared Application Shell

Authenticated MVC views MUST use `Views/Shared/_Layout.cshtml`. The shell
contains:

1. A semantic `<header>` with the NovaLeave brand linked to the active context
   root.
2. One role-specific primary navigation set at a time.
3. The `Mis roles` context switcher when multiple eligible contexts exist.
4. A user menu with identity/context text and a POST `Cerrar sesión` action.
5. A semantic `<main>` containing the rendered view and shared toast region.
6. The shared confirmation modal and pinned Bootstrap bundle.

The brand, navigation, context switcher, user menu, focus treatment, mobile
navigation, and visual tokens MUST follow `frontend-design-spec.md`; this
document does not redefine their measurements or colors.

### 6.1 Primary Navigation

| Context | Required labels and destinations |
|---|---|
| `User` | `Mis solicitudes` → `/mis-solicitudes`; `Crear solicitud` → `/mis-solicitudes/crear`; `Mi historial` → `/saldo`; `Mi calendario` → `/calendario?context=User`. |
| `Approver` | `Aprobaciones` → `/aprobaciones`; `Historial` → `/aprobaciones/historial`; `Calendario` → `/calendario?context=Approver`. |
| `HR` | `Panel RRHH` → `/rrhh`; `Solicitudes` → `/rrhh/solicitudes`; `Calendario global` → `/rrhh/calendario`; `Saldos` → `/rrhh/saldos`; `Aprobadores` → `/rrhh/aprobadores`; `Auditoría` → `/rrhh/auditoria`. |

Only navigation for the active context MUST be rendered. Hiding navigation is
not authorization; direct requests MUST still be authorized server-side.

### 6.2 Page Composition

Views SHOULD compose the current shared visual hierarchy:

- Page header with one `<h1>`, concise Spanish description, optional approved
  SVG icon, and a context-appropriate action or read-only indicator.
- Summary-card grid when authoritative totals are available.
- One or more white `.nl-card` surfaces for the principal content.
- Section headings, filters, tables/forms/calendars, and an explicit empty or
  error state inside the content surface.

Decorative icons MUST use the shared SVG icon partial or an approved equivalent,
MUST be hidden from assistive technology when adjacent text supplies the name,
and MUST NOT replace visible labels.

## 7. View Inventory by Role

### 7.1 User — `Mi espacio`

| View | Current structure and normative presentation |
|---|---|
| `MisSolicitudes/Index` | Page header with create action; four balance cards; client-visible status filter; own-request table with status badge, date range, working days, creation date, detail action, and conditional edit action; explicit empty state. |
| `MisSolicitudes/Create` | Constrained form card; two-mode segmented selector; start/end or start/days input; informational calculated-days/end/balance panel; reason counter; validation summary; cancel and submit actions. |
| `MisSolicitudes/Edit` | Pre-populated dedicated ViewModel and row-version token; MUST present the same two mutually exclusive modes as Create and preserve input on validation/conflict. |
| `MisSolicitudes/Detail` | Read-only status/date/reason composition; edit action only for an owned `Pending` request. |
| `MisSolicitudes/Balance` | Four balance cards and a chronological movement table with request links where authorized; explicit empty state. |
| `Calendario/Index` | Personal month view through the shared calendar partial and authorized owner-detail links. |

### 7.2 Approver — `Aprobaciones`

| View | Current structure and normative presentation |
|---|---|
| `Aprobaciones/Index` | Queue count; pending-request table with requester, dates, requested days, authoritative available value, projected post-approval value, and review link; explicit empty state. |
| `Aprobaciones/Detail` | Request identity and date information; projected-balance summary; overlap warning when applicable; mutually exclusive approve/reject controls; conditional pre-start deactivation; validation/conflict feedback and confirmation. |
| `Aprobaciones/History` | Resolution table with timestamp, action, request/detail link, requester, and status; explicit empty state. |
| `Calendario/Approver` | Authorized Approver month view through the shared calendar partial, respecting anonymization and detail-link authorization. |

The Approver UI MUST display requester identity where the approved view contract
authorizes it. It MUST NOT display the actor as anonymous when the requester name
is available and required by the approved queue/detail design.

### 7.3 HR — `RRHH`

| View | Current structure and normative presentation |
|---|---|
| `RRHH/Index` | Organization metric cards and quick-link module cards for calendar, Approver capability management, and audit. |
| `RRHH/Solicitudes` | Read-only request table, status filter, server-side paging state, detail links, and empty state. |
| `RRHH/SolicitudDetalle` | Read-only requester, dates, days, status, authorized reasons, and audit trail; no resolution actions. |
| `RRHH/Calendario` | Dedicated organization calendar with requester identity and authorized detail links; visible read-only marker. |
| `RRHH/Saldos` | Read-only organization balance table with the four approved balance concepts and movement links. |
| `RRHH/Movimientos` | Selected-user balance summary and chronological movements with request links where available. |
| `RRHH/Auditoria` | Relevant, redacted audit records with paging state. |
| `ApproverCapabilities/Index` | Approver identity, active status, capability status, and link to the controlled capability form. |
| `ApproverCapabilities/Capability` | Current capability summary; dedicated input ViewModel; reason, confirmation, row version, and explicit modal confirmation. |

All HR request, calendar, balance, movement, and audit views MUST visibly state
`Solo lectura` and MUST render no Approve, Reject, Deactivate, balance-edit, or
role-assignment action. Capability management is the sole approved HR mutation
and MUST be described as such rather than as general administration.

## 8. Reusable Presentation Components

The following shared artifacts are the canonical reuse points:

| Artifact | Responsibility |
|---|---|
| `_Layout.cshtml` | Shell, role navigation, context switcher, user menu, logout, shared assets, toast host, and confirmation modal host. |
| `_Icon.cshtml` plus the layout SVG sprite | Consistent decorative line icons without emoji text. |
| `_StatusBadge.cshtml` | Spanish status label plus semantic class; color is never the only state signal. |
| `_Calendar.cshtml` | Shared month grid, navigation, authorized event links, overflow indicator, event list, and calendar empty state. |
| `_ValidationSummary.cshtml` | Focusable, assertive validation summary with explicit failure heading. |
| `_ConfirmationModal.cshtml` | Reusable Bootstrap confirmation surface driven by form data attributes. |
| `_Toast.cshtml` | Shared polite success feedback rendered from `TempData`. |
| `_EmptyStateIcon.cshtml` | Decorative accessible-hidden SVG for empty states. |
| `Error.cshtml` | Safe unexpected-error presentation with correlation reference. |

New views MUST reuse these artifacts where their responsibility matches. A new
partial, View Component, or Tag Helper MAY be added only when repeated markup or
behavior has a cohesive responsibility not covered by an existing component.

## 9. Tables, Filters, Pagination, and Search

- Tables MUST use semantic captions and scoped column headers.
- Desktop tabular views SHOULD use `.nl-table`; views that require a mobile card
  fallback SHOULD also use the shared responsive-table convention and
  `data-label` values.
- A filter MUST have a visible or accessible label and MUST preserve its current
  value after navigation or server response.
- Client-side filtering MAY be used only for an already-bounded rendered result,
  such as the current personal request status filter. It MUST NOT replace
  server-side filtering or pagination for potentially large organization lists.
- Organization lists that may exceed approved limits MUST use server-side
  pagination. Paging links MUST preserve active filters and expose accessible
  names and current-page information.
- Sorting, searching, date-range filtering, page-size selection, and full
  first/previous/next/last controls MUST be implemented only where supported by
  the corresponding query contract. The spec MUST NOT claim they exist merely
  because a table is present.
- Empty results MUST render an explicit Spanish empty state rather than an empty
  table body.

## 10. Forms and Validation Presentation

- Every mutation MUST bind a dedicated input ViewModel and use server-side
  validation as authoritative.
- Razor MUST NOT bind Domain or EF entities directly.
- Every control MUST have an associated visible label or equivalent accessible
  name, and field errors MUST be rendered adjacent to their controls when
  available.
- `_ValidationSummary` MUST present operation-level failures, including safe
  validation and conflict messages. Focus SHOULD move to that summary or the
  first invalid field.
- User-entered non-sensitive values MUST be preserved when a form is redisplayed.
- Create and Edit MUST expose exactly the two approved request input modes. Only
  the active mode's fields MUST be enabled and submitted.
- Working-day, end-date, character-count, and balance previews are informational.
  They MUST be labelled for assistive technology and MUST NOT be accepted as
  authoritative server values.
- Row-version values MAY be carried in hidden inputs for optimistic concurrency;
  they MUST NOT be presented as business data.
- Forms MUST prevent duplicate submission, preserve button width while loading,
  expose a processing state, and wait for server confirmation before permanent
  UI updates.
- Unsafe requests MUST retain antiforgery protection.

## 11. Summary Cards and Status Representation

Balance summaries MUST use these exact labels and authoritative mappings:

| Label | Meaning |
|---|---|
| `Acumulado total` | Accrued days. |
| `Pendientes` | Days reserved by active `Pending` requests. |
| `Días gozados` | Permanently deducted days. |
| `Disponible` | Authoritative available balance. |

Summary cards MUST provide a text label, value, and concise helper text where
useful. They MUST NOT introduce a new balance formula.

Status presentation MUST use the Spanish labels approved in
`frontend-design-spec.md`, the corresponding semantic tokens, and readable
text. `Pending`, `Approved`, `Rejected`, `CancelledByTimeout`, and
`CancelledByApprover` MUST remain visually and textually distinguishable.

## 12. Confirmations, Feedback, and View States

- Approve, Reject, pre-start Deactivate, and capability changes MUST require an
  explicit accessible confirmation before submission.
- Reject MUST retain its required reason input. Capability changes MUST retain
  reason, explicit confirmation, and row-version input. No new reason
  requirement may be invented for request deactivation.
- Shared confirmation behavior SHOULD use `_ConfirmationModal` and
  `data-confirm-*` attributes. A specialized modal MAY be retained when the form
  content itself is part of the confirmation, as in capability management.
- Success MAY use the shared toast. Validation, authorization, conflict, and
  critical failures MUST have persistent inline feedback and MUST NOT rely only
  on a toast.
- Conflict text MUST tell the actor to refresh current information and retry.
- Loading states MUST include accessible text and prevent duplicate submission.
- Every list or calendar MUST define an explicit empty state. Error and forbidden
  states MUST use safe Spanish messaging without revealing protected resources
  or implementation details.

## 13. Calendar Contract

All three role contexts MUST reuse `_Calendar.cshtml` and a dedicated wrapper
ViewModel containing the shared `CalendarViewModel`.

- The grid MUST be a traditional Monday-to-Sunday month view with semantic day
  headers, weekday focus targets, muted weekends, and no weekend events.
- Only authoritative approved periods MUST be rendered, as required by
  `frontend-design-spec.md` §25.2.
- User events MUST show the owner's approved periods and MAY link to
  `/mis-solicitudes/{id}` when authorized.
- Approver events MUST preserve the approved anonymized scope. They MUST NOT
  expose requester identity or a detail link when the actor is not authorized
  for that detail.
- HR events MUST show the requester name and MAY link to
  `/rrhh/solicitudes/{id}`.
- Events MUST show readable text including working-day count; approved calendar
  events use the approved calendar treatment from the design spec.
- No essential event information may depend only on hover.
- Month controls, day cells, events, overflow, current day, focus, keyboard
  behavior, mobile rendering, and reduced motion MUST follow
  `frontend-design-spec.md` §25 without redefining its values here.

## 14. Responsive Behavior

- The shell, page header, summary grids, module grids, tables, forms, dialogs,
  and calendars MUST work across the breakpoints defined by the frontend design
  specification.
- The authenticated header MUST use the approved mobile navigation pattern.
- Summary and module grids MUST collapse without hiding required data or actions.
- Responsive tables MAY become labelled card rows on smaller screens; status,
  dates, working days, balance impact, and authorized actions remain the content
  priority.
- Normal forms MUST become single-column where needed.
- The calendar MUST retain the approved mobile month-view behavior; replacing
  the month grid with a different presentation requires design review.
- Required actions and focused controls MUST remain visible and reachable
  without page-level horizontal overflow.

## 15. Accessibility and Keyboard Interaction

Every view MUST meet WCAG 2.1 AA and the frontend design specification:

- Semantic landmarks and one purpose-matching `<h1>`.
- Logical heading hierarchy.
- Visible focus for every interactive element.
- Accessible labels for forms, filters, menus, modal controls, and calendar
  navigation.
- Textual status and validation feedback; color and icons are never sole cues.
- Semantic table captions and headers.
- Accessible modal naming, focus containment, close behavior, and focus return.
- `aria-live` treatment for meaningful validation, calculation, loading, month,
  and result updates.
- Keyboard operation for dropdowns, forms, confirmation dialogs, and calendar
  events; calendar key behavior MUST follow the design spec.
- Decorative SVGs MUST use `aria-hidden="true"`; meaningful standalone icons
  require an accessible name.
- Reduced-motion preferences MUST disable non-essential motion as defined in the
  design spec.
- Interactive target dimensions MUST use the approved minimum.

## 16. Login, Access Denied, and Unexpected Errors

### 16.1 Login

The Identity login page MUST use the shared NovaLeave visual system, visible
email/password labels, appropriate autocomplete attributes, preserved email,
server and client validation, antiforgery, keyboard access, and responsive card
layout. When demo seeding is enabled outside Production, the `Cuenta de
demostración` selector MAY prefill the email but MUST NOT authenticate and MUST
mask email display according to the design spec.

### 16.2 Access Denied

`/Identity/Account/AccessDenied` MUST provide a branded Spanish denial without
confirming protected-resource existence. It SHOULD provide a safe path back to
an authorized context.

### 16.3 Unexpected Error

The centralized error view MUST provide safe Spanish guidance and a correlation
reference. It MUST NOT display stack traces, database detail, secrets, or
sensitive request content.

## 17. ViewModels and Presentation Boundaries

- Every Razor View MUST receive a dedicated Presentation ViewModel or a shared
  Presentation contract intentionally wrapped by one.
- ViewModels MAY contain display-ready values, current filters, paging state,
  safe URLs, and hidden concurrency tokens.
- Controllers MAY select a view, map Application results, set safe status codes,
  and preserve validation feedback. They MUST NOT calculate balances, working
  days, overlap, permissions, or lifecycle transitions.
- Razor MAY conditionally render an action only from an authoritative
  server-provided capability or status. It MUST NOT independently decide that an
  actor is authorized.
- Client calculations are previews only. Hidden fields, query strings, CSS
  state, disabled buttons, and context selection are untrusted.

## 18. Frontend Extension Rules

Future NovaLeave screens and changes:

1. MUST reuse the shared layout, role navigation, context switcher, icon system,
   feedback components, and established view patterns before creating a new
   abstraction.
2. MUST NOT use inline styles.
3. MUST NOT add inline scripts or duplicate CSS/JavaScript per view; shared
   behavior belongs under the controlled `wwwroot/css` and `wwwroot/js` assets.
4. MUST use the pinned Bootstrap 5.3.x conventions and the shared NovaLeave
   component classes.
5. MUST keep user-facing navigation, labels, validation, and feedback in Spanish;
   technical identifiers remain in English.
6. MUST use dedicated ViewModels and MUST NOT expose Domain or EF entities
   directly to Razor.
7. MUST keep authorization, sensitive-data visibility, and business decisions
   outside purely visual logic.
8. MUST preserve established patterns for tables, filters, pagination, forms,
   badges, alerts, confirmations, modals, loading, and empty states.
9. MUST meet WCAG 2.1 AA, include responsive behavior, support keyboard use, and
   retain visible focus.
10. MUST NOT use color, motion, or an icon as the only indicator of status or
    meaning.
11. MUST NOT create roles, permissions, routes, lifecycle states, balance rules,
    or other business behavior from the frontend.
12. MUST NOT render an action the actor cannot execute; hiding it remains UX,
    not authorization.
13. MUST NOT trust client-calculated dates, working days, balances, projections,
    ownership, status, role, or capability.
14. MUST preserve the separation of `User`, `Approver`, and `HR` contexts,
    including privacy and detail-link boundaries.
15. MUST use `/rrhh/calendario` for HR and MUST NOT reuse `/calendario` as the
    HR organization calendar.
16. MUST reference the values in `frontend-design-spec.md` for palette,
    typography, spacing, radius, shadow, motion, target size, and breakpoints;
    it MUST NOT invent or duplicate those values here.
17. MUST remain compatible with approved contracts, use cases, authorization
    policies, and route traceability tests.
18. SHOULD extend an existing partial or shared module when the responsibility
    is already present; a new component MAY be introduced for a genuinely new,
    repeated presentation concern.

## 19. Security and Authorization Presentation Rules

- Protected controllers MUST retain their approved active-role policies.
- Approver queue, detail, history, resolution, and calendar context require an
  eligible active Approver with `canResolveRequests=true`.
- Owner and resource eligibility MUST be revalidated server-side.
- HR MUST NOT receive request-resolution, balance-edit, or role-assignment UI.
- The capability-management form MUST target only an existing Approver and MUST
  preserve reason, confirmation, concurrency, and audit behavior from the
  authoritative specifications.
- Unauthorized detail links MUST not be rendered, and forced browsing MUST fail
  closed.
- Request reasons and rejection reasons are sensitive. Their rendering MUST be
  limited to an authorized owner, eligible Approver use case, or authorized HR
  read use case.

## 20. Traceability Matrix

| Context | Route/view | Primary use case evidence | Principal RBFV criteria |
|---|---|---|---|
| Shared | `/Identity/Account/Login` | UC01 | RBFV-027, RBFV-033 |
| Shared | Context switcher in `_Layout` | UC02 | RBFV-009, RBFV-010, RBFV-020, RBFV-021 |
| User | `/mis-solicitudes` | UC03 | RBFV-001, RBFV-024, RBFV-031 |
| User | `/mis-solicitudes/crear` | UC04 | RBFV-028, RBFV-029, RBFV-034 |
| User | `/mis-solicitudes/{id}/editar` | UC05 | RBFV-028, RBFV-029, RBFV-030 |
| User | `/mis-solicitudes/{id}` | UC06 | RBFV-001, RBFV-026 |
| User | `/saldo` | UC07 | RBFV-022, RBFV-024 |
| User | `/calendario?context=User` | UC08 | RBFV-026, RBFV-029 |
| Approver | `/aprobaciones` | UC09 | RBFV-003, RBFV-025, RBFV-031, RBFV-034 |
| Approver | `/aprobaciones/{id}` | UC10 | RBFV-025, RBFV-030, RBFV-031 |
| Approver | Approve/Reject POST | UC11, UC12 | RBFV-006, RBFV-014, RBFV-030 |
| Approver | Deactivate POST | UC13 | RBFV-014, RBFV-030 |
| Approver | `/aprobaciones/historial` | UC14 | RBFV-023 |
| Approver | `/calendario?context=Approver` | UC15 | RBFV-012, RBFV-026 |
| HR | `/rrhh/solicitudes[/{id}]` | UC18 | RBFV-005, RBFV-006, RBFV-032 |
| HR | `/rrhh/calendario` | UC19 | RBFV-026, RBFV-032 |
| HR | `/rrhh/saldos[/{userId}]` | UC20 | RBFV-007, RBFV-024, RBFV-032 |
| HR | `/rrhh/auditoria` | UC21 | RBFV-032 |
| HR | `/rrhh/aprobadores[/{id}/capacidad]` | UC22 | RBFV-008, RBFV-013, RBFV-015, RBFV-016, RBFV-017, RBFV-032 |
| Cross-role | Route authorization | Route traceability and security tests | RBFV-002, RBFV-004, RBFV-006–RBFV-008, RBFV-011–RBFV-017 |
| Cross-view | Shared accessibility/components | Presentation unit and E2E accessibility tests | RBFV-018, RBFV-019, RBFV-027 |

### 20.1 RBFV Acceptance Criteria

The identifiers consumed by the primary MVP traceability matrix remain stable:

| ID | Normative outcome |
|---|---|
| RBFV-001 | Active User can render owned request list/detail; cross-user data is absent. |
| RBFV-002 | User access to Approver routes is denied. |
| RBFV-003 | Eligible active Approver can render the queue and authorized detail. |
| RBFV-004 | An Approver without an active User context cannot create a User request. |
| RBFV-005 | Active HR can render organization request list/detail as read-only. |
| RBFV-006 | HR request-resolution attempts are denied and no resolution action is rendered. |
| RBFV-007 | HR balance mutation is denied and no balance-edit action is rendered. |
| RBFV-008 | HR role assignment/removal is denied and no such action is rendered. |
| RBFV-009 | Multi-role context switching changes navigation/route only. |
| RBFV-010 | Triple-role eligible identity sees all three context options. |
| RBFV-011 | Inactive User protected access is denied. |
| RBFV-012 | Inactive or capability-disabled Approver protected access is denied. |
| RBFV-013 | Inactive HR capability mutation is denied. |
| RBFV-014 | Approver self-resolution is denied and its action is absent. |
| RBFV-015 | Capability toggle for a non-Approver is rejected. |
| RBFV-016 | Capability toggle without valid reason/confirmation is rejected with feedback. |
| RBFV-017 | Stale capability toggle returns a safe conflict outcome. |
| RBFV-018 | Views satisfy the approved accessibility contract. |
| RBFV-019 | Reduced-motion preference suppresses non-essential animation. |
| RBFV-020 | `Mis roles` is shown only for two or more eligible contexts. |
| RBFV-021 | Context switching does not change identity, claims, or authorization. |
| RBFV-022 | `/saldo` is labelled `Mi historial` in navigation. |
| RBFV-023 | Approver history navigation is labelled `Historial`. |
| RBFV-024 | Balance cards use the four exact approved Spanish labels. |
| RBFV-025 | Approver queue/detail present the three authoritative projected-balance values and warn/disable approval when invalid. |
| RBFV-026 | Calendar activation reaches only the authorized role-specific detail route. |
| RBFV-027 | User-facing text contains no emoji; icons remain accessible. |
| RBFV-028 | Create/Edit expose only the two approved mutually exclusive input modes. |
| RBFV-029 | Working-day previews match the approved weekend policy but remain informational. |
| RBFV-030 | Conflicting resolution controls disable during submit; server confirmation remains authoritative. |
| RBFV-031 | User/Approver views use the approved hierarchy, cards, statuses, feedback, and empty states. |
| RBFV-032 | HR views preserve read-only boundaries, dedicated calendar route, pagination where required, and controlled capability management. |
| RBFV-033 | Login follows Identity, branding, accessibility, responsive, masking, and no-gradient/no-emoji rules. |
| RBFV-034 | Successful creation feedback is visible and the eligible Approver queue shows the committed request exactly once on normal refresh/navigation. |

## 21. Audit Findings and Unresolved Implementation Deviations

The following findings MUST NOT be treated as approved behavior. They remain
implementation work outside this documentation-only change.

### 21.1 `DOCUMENTATION_GAP` Resolved Here

- The exact shared partial inventory and its responsibilities are now defined.
- Page-header, metric-card, module-card, table-card, form-preview, empty-state,
  toast, validation-popover, icon-sprite, and data-attribute interaction patterns
  are documented.
- The current distinction between bounded client-side filtering and server-side
  organization pagination is documented.
- The real Identity access-denied route and centralized internal error view
  replace stale public-route aliases.
- The current HR dashboard and dedicated capability form are included in the
  view map.

### 21.2 `STALE_SPEC` Corrected

- The spec no longer claims that every table already has sorting, search,
  date-range filtering, page-size selection, and complete pagination controls.
- The spec no longer describes a generic rendered Dashboard view at `/`; the
  implemented root behavior is a role-aware redirect.
- The spec no longer treats `/acceso-denegado` or `/error` as implemented public
  routes.
- Reuse rules now reference the partials and client hooks that actually exist.

### 21.3 `IMPLEMENTATION_DEVIATION`

1. The User navigation currently renders `Mi Saldo`; the approved label is
   `Mi historial` (RBFV-022 and frontend design spec §15).
2. The responsive header currently uses a Bootstrap collapse rather than the
   approved off-canvas mobile drawer, and the user menu omits the approved
   disabled `Perfil` placeholder.
3. `MisSolicitudes/Create.cshtml` contains duplicated inline request-preview
   JavaScript while a shared implementation exists in `site.js`; future and
   corrected views MUST use shared assets.
4. The Edit view uses a select and simultaneously renders both mode-specific
   inputs instead of the same mutually exclusive two-mode presentation required
   for Create/Edit.
5. Current uncommitted calendar query/view changes render non-approved statuses
   and requester names in scopes where `frontend-design-spec.md` §25 requires
   approved periods only and Approver anonymization.
6. The shared calendar exposes keyboard data hooks but `site.js` does not
   implement the specified arrow/Home/End/PageUp/PageDown interaction. On the
   smallest breakpoint it hides the grid and shows a list instead of the
   approved mobile month-grid behavior.
7. The Approver detail and history currently render `Usuario solicitante`
   instead of the authorized requester name required by the approved visual
   hierarchy.
8. The Approver detail does not visibly implement the required invalid projected
   balance warning plus disabled Approve control.
9. Several lists lack the complete server-side filters, sorting, accessible
   pagination controls, or mobile fallback required by the frontend design spec,
   especially Approver history/queue and HR balances/audit/capabilities.
10. The shared status badge maps both cancellation states to one neutral class
    instead of preserving their distinct approved semantic treatments.
11. The login demo-account option currently displays the full email rather than
    the masked presentation required by the frontend design spec.
12. Some current CSS overrides use radius and interactive-height values outside
    the approved design-token contracts. The design spec remains authoritative;
    these values are not adopted here.

### 21.4 `OUT_OF_SCOPE`

- Changes to business calculations, authorization, state transitions, routes,
  data contracts, controller behavior, CSS, JavaScript, Razor, tests, or demo
  data are outside this documentation-only audit.
- No deviation listed above was corrected or converted into a new business
  requirement by this revision.

## 22. Version History

| Version | Date | Change |
|---|---|---|
| 1.0.0 | 2026-07-23 | Initial role-based view draft. |
| 2.0.0 | 2026-07-23 | Added `HR`/`RRHH` context. |
| 2.0.1 | 2026-07-30 | Clarified Approver capability eligibility and HR-only calendar route. |
| 2.1.0 | 2026-08-04 | Audited current Presentation implementation; documented shared components and final view composition; corrected stale route/view claims; added `Frontend Extension Rules`; recorded unresolved deviations without legitimizing them. |

---

**Constitution Alignment**: This specification is subordinate to
`.specify/memory/constitution.md` v7.0.0,
`specs/001-leave-management-mvp/spec.md`, and
`specs/001-leave-management-mvp/frontend-design-spec.md` v1.3.0. Any conflict
MUST be resolved at the higher-authority source; implementation evidence alone
MUST NOT amend approved product or design behavior.
