# Role-Based Frontend Views Specification — NovaLeave MVP

**Related Feature**: `002-role-based-frontend-views`
**Version**: 2.0.0
**Date**: 2026-07-23
**Status**: Draft for review
**Constitution**: `.specify/memory/constitution.md` v6.0.0

---

## 1. Purpose

This specification defines the mandatory view and navigation structure for NovaLeave MVP based on the three application roles (`User`, `Approver`, `HR`) plus the automatic system actor. It complements `specs/001-leave-management-mvp/spec.md` (business behavior) and `specs/001-leave-management-mvp/frontend-design-spec.md` v1.3.0 (design tokens, components, accessibility baseline).

All routes, views, and navigation elements described here MUST conform to the Constitution v6.0.0, the approved frontend design specification v1.3.0, and the authority order in Constitution §15.1.

---

## 2. Scope

This specification covers:

- Route-to-role mapping and exclusive access rules
- Required views per role
- Shared views accessible by multiple roles
- Navigation structure per role
- View-level accessibility requirements
- Security and authorization boundaries at the routing level

This specification does NOT cover:

- Business rules (see `specs/001-leave-management-mvp/spec.md`)
- Design tokens, colors, spacing, motion (see `frontend-design-spec.md`)
- API contracts (no approved API exists for the MVP)

---

## 3. Authority Order

Per Constitution §15.1:

1. **Constitution v6.0.0** — architecture, security, roles, cross-cutting invariants, engineering quality, governance
2. **Approved feature specifications** — business behavior and acceptance criteria within constitutional boundaries
3. **Approved frontend design specification v1.3.0** — design tokens, responsive behavior, accessibility, components, motion, interaction patterns
4. **Approved ADRs** — authorized architectural exceptions
5. **Code and tests** — implement approved requirements but do not override them
6. **Prototypes, screen-construction guides, OKF, Mermaid, Graphify** — derived or reference artifacts; cannot introduce requirements, roles, or permissions

---

## 4. Role and Context Definitions

| Technical Role | Spanish UI Context | Route Prefix | Description |
|----------------|-------------------|--------------|-------------|
| `User` | `Mi espacio` | `/mis-solicitudes` | Personal vacation workflows |
| `Approver` | `Aprobaciones` | `/aprobaciones` | Request resolution workflows |
| `HR` | `RRHH` | `/rrhh` | Organization-wide read access and approver-capability management |
| System | — | — | Automatic timeout cancellation (`CancelledByTimeout`) |

> **Note**: The technical identifier is always `User` / `Approver` / `HR` in code, policies, and documentation. The Spanish context label (`Mi espacio` / `Aprobaciones` / `RRHH`) appears only in user-facing navigation and breadcrumbs.

### 4.1 Role Combinations

A single identity MAY hold any valid combination of `User`, `Approver`, and `HR` roles. When multiple roles are present:

- All authorized navigation contexts MUST be available via a context switcher in the header
- Switching context MUST NOT modify identity, session, roles, claims, permissions, ownership, or resource authorization
- The active context determines which navigation bar and route prefix are visible
- Resource authorization is ALWAYS re-evaluated per request; context is a UI convenience only

### 4.2 Context Selector — "Mis roles"

When an identity has at least two authorized contexts:

- **Visible label**: `Mis roles` (accessible name and visible concept)
- **Control type**: Accessible dropdown/button group (`<select>` or `<button aria-haspopup="listbox">` + listbox)
- **Options**: `Mi espacio`, `Aprobaciones`, `RRHH` (Spanish labels matching role contexts)
- **Behavior**: Switching context updates the visible navigation and route prefix only; **must not** modify identity, session, roles, claims, permissions, ownership, or resource authorization
- **Display condition**: Hidden when the identity has fewer than two authorized contexts
- **Active context indication**: The selected context may be indicated inside the control (e.g., as the button label or selected option)
- **Keyboard accessible**: Full arrow-key navigation, `Esc` to close, focus management on open/close
- **No emojis** or icons as sole indicators

---

## 5. Route-to-Role Mapping

### 5.1 Exclusive Routes

| Route Pattern | Methods | Authorized Role(s) | Description |
|---------------|---------|-------------------|-------------|
| `/mis-solicitudes` | GET | `User` (Active) | List own requests |
| `/mis-solicitudes/crear` | GET, POST | `User` (Active) | Create new request |
| `/mis-solicitudes/{id}/editar` | GET, POST | `User` (Active, Owner) | Edit own `Pending` request |
| `/mis-solicitudes/{id}` | GET | `User` (Active, Owner) | View own request detail |
| `/aprobaciones` | GET | `Approver` (Active) | List eligible pending requests |
| `/aprobaciones/{id}` | GET | `Approver` (Active, Eligible) | View request detail for resolution |
| `/aprobaciones/{id}/aprobar` | POST | `Approver` (Active, Eligible, Not Owner) | Approve request |
| `/aprobaciones/{id}/rechazar` | POST | `Approver` (Active, Eligible, Not Owner) | Reject request |
| `/aprobaciones/{id}/desactivar` | POST | `Approver` (Active, Eligible, Not Owner) | Deactivate approved request (pre-start) |
| `/aprobaciones/{id}/escalar` | POST | `Approver` (Active, Eligible, Not Owner) | Escalate `PendingEscalated` request to HR |
| `/rrhh` | GET | `HR` (Active) | HR dashboard |
| `/rrhh/solicitudes` | GET | `HR` (Active) | Read-only request list (org-wide) |
| `/rrhh/solicitudes/{id}` | GET | `HR` (Active) | Read-only request detail |
| `/rrhh/solicitudes/{id}/autorizar-exceso` | POST | `HR` (Active) | Authorize excess days for `EscalatedToHR` request |
| `/rrhh/solicitudes/{id}/rechazar-exceso` | POST | `HR` (Active) | Reject excess, reduce to available balance |
| `/rrhh/calendario` | GET | `HR` (Active) | Organizational calendar |
| `/rrhh/saldos` | GET | `HR` (Active) | Read-only balances list |
| `/rrhh/saldos/{userId}` | GET | `HR` (Active) | Read-only balance detail and movements |
| `/rrhh/auditoria` | GET | `HR` (Active) | Relevant audit information |
| `/rrhh/aprobadores` | GET | `HR` (Active) | Approver capability list |
| `/rrhh/aprobadores/{id}/capacidad` | GET, POST | `HR` (Active) | Toggle `canResolveRequests` for Approver |

### 5.2 Shared Routes

| Route Pattern | Methods | Authorized Role(s) | Description |
|---------------|---------|-------------------|-------------|
| `/` | GET | `User` (Active) \| `Approver` (Active) \| `HR` (Active) | Dashboard redirect based on roles |
| `/saldo` | GET | `User` (Active) | View own global balance (labeled **Mi historial** in navigation) |
| `/calendario` | GET | `User` (Active) \| `Approver` (Active) \| `HR` (Active) | Basic vacation calendar |
| `/acceso-denegado` | GET | Any (including unauthenticated) | Branded 403 page |
| `/error` | GET | Any | Branded error page |

### 5.3 Authentication Routes (Shared)

| Route Pattern | Methods | Description |
|---------------|---------|-------------|
| `/Identity/Account/Login` | GET, POST | ASP.NET Core Identity login |
| `/Identity/Account/Logout` | POST | Secure logout |
| `/Identity/Account/AccessDenied` | GET | Redirect target for failed authorization |

> All authentication routes use the default Identity UI area. No custom routes are added.

---

## 6. Required Views Per Role

### 6.1 User Context (`Mi espacio`)

| View | Route | Purpose | Key Components |
|------|-------|---------|----------------|
| **Mis Solicitudes** | `/mis-solicitudes` | List own requests with status badges, date ranges, working-day totals, balance impact | Table, status badges, empty state, create button |
| **Crear Solicitud** | `/mis-solicitudes/crear` | Submit new vacation request using either input mode | Dual-mode form (date-range / start+days), validation summary, balance preview, submit |
| **Editar Solicitud** | `/mis-solicitudes/{id}/editar` | Edit owned `Pending` request with full revalidation | Pre-filled dual-mode form, revalidation, version concurrency token |
| **Detalle de Solicitud** | `/mis-solicitudes/{id}` | Read-only detail with full audit trail | Status badge, date range, working days, reason, balance impact, audit timeline |
| **Mi historial** | `/saldo` | Display accrued, reserved, deducted, available days with history | Summary cards, balance movements, request-related movements, accrual note |
| **Calendario Básico** | `/calendario` | Month-view calendar showing own approved periods | Month grid, event markers, legend |

### 6.2 Approver Context (`Aprobaciones`)

| View | Route | Purpose | Key Components |
|------|-------|---------|----------------|
| **Pendientes de Resolución** | `/aprobaciones` | List all eligible `Pending` requests across organization | Table with requester, dates, working days, **balance available, projected balance (Disponible actual, Días solicitados, Disponible después de aprobar)**, overlap warnings, action buttons |
| **Detalle para Resolución** | `/aprobaciones/{id}` | Detail view with resolution actions | Full request info, **balance revalidation with projected balance**, overlap revalidation, approve/reject/deactivate buttons, rejection reason textarea; for `PendingEscalated`: shows `Exceso de saldo` badge, `Disponible actual`, `Días solicitados`, `Exceso`, `Justificación del solicitante`, **Escalar a RRHH** action (primary), **Rechazar** (secondary), Approve disabled |
| **Historial de Resoluciones** | `/aprobaciones/historial` | List of requests resolved by this approver | Table with date, action, requester, status, audit link |
| **Calendario Básico** | `/calendario` | Month-view calendar showing approved periods (org-wide, anonymized per policy) | Month grid, event markers, legend |

### 6.3 HR Context (`RRHH`)

| View | Route | Purpose | Key Components |
|------|-------|---------|----------------|
| **Dashboard RRHH** | `/rrhh` | Organization-wide summary cards | Total pending, active approvers, balance summary, quick links |
| **Solicitudes (Solo Lectura)** | `/rrhh/solicitudes` | Filterable, paginated list of all requests | Table with requester, dates, status, working days, reservation, deduction |
| **Detalle de Solicitud (Solo Lectura)** | `/rrhh/solicitudes/{id}` | Full request detail including audit trail | Same as Approver detail but read-only; no resolution actions; for `EscalatedToHR`: shows `Autorizar exceso` and `Rechazar exceso` actions |
| **Autorizar Exceso RRHH** | `/rrhh/solicitudes/{id}/autorizar-exceso` | HR authorizes excess days | Modal with authorized excess count, reason (10–500 chars), confirmation, row version; transitions to `PendingAuthorizedExcess` |
| **Rechazar Exceso RRHH** | `/rrhh/solicitudes/{id}/rechazar-exceso` | HR rejects excess, reduces to available balance | Modal with reason (10–500 chars), confirmation; transitions to `Pending` with requested days = available balance at submission |
| **Calendario Organizacional** | `/rrhh/calendario` | Month-view calendar showing all approved periods | Month grid, event markers, legend, filter by department/team if applicable |
| **Saldos (Solo Lectura)** | `/rrhh/saldos` | List all users with balance summary | Table with user, **Acumulado total, Pendientes, Días gozados, Disponible** |
| **Movimientos de Saldo (Solo Lectura)** | `/rrhh/saldos/{userId}` | Balance history for user | Timeline of accruals, reservations, deductions, restorations |
| **Auditoría Relevante** | `/rrhh/auditoria` | Filterable audit log | Table with timestamp, actor, role, action, entity, result |
| **Gestión de Aprobadores** | `/rrhh/aprobadores` | List all identities with `Approver` role and `canResolveRequests` | Table with name, email, active status, canResolveRequests, toggle action |
| **Activar/Desactivar Capacidad** | `/rrhh/aprobadores/{id}/capacidad` | Modal with reason, confirmation, row version | Confirmation modal, reason textarea (required), row version check, audit |

### 6.4 Shared Views

| View | Route | Purpose | Key Components |
|------|-------|---------|----------------|
| **Dashboard** | `/` | Role-aware landing page | Context switcher (if multi-role), quick actions, summary cards |
| **Acceso Denegado** | `/acceso-denegado` | Branded 403 with context-aware message | Friendly message, return link, contact hint |
| **Error** | `/error` | Branded error page | Correlation ID, support reference |

---

## 7. Navigation Structure

### 7.1 User Context Navigation (`Mi espacio`)

```mermaid
flowchart LR
    A[Dashboard /] --> B[Mis Solicitudes /mis-solicitudes]
    A --> C[Mi Historial /saldo]
    A --> D[Calendario /calendario]
    B --> E[Crear /mis-solicitudes/crear]
    B --> F[Detalle /mis-solicitudes/{id}]
    F --> G[Editar /mis-solicitudes/{id}/editar]
```

**Primary Nav Items (header/sidebar):**
1. **Mis solicitudes** → `/mis-solicitudes`
2. **Mi historial** → `/saldo`
3. **Calendario** → `/calendario`

**Context Switcher** (visible only when identity has 2+ roles): Labeled **"Mis roles"** — dropdown with available contexts (`Mi espacio`, `Aprobaciones`, `RRHH`); hidden when <2 roles; switching updates route prefix and navigation only; does not modify identity, session, roles, claims, or permissions.

### 7.2 Approver Context Navigation (`Aprobaciones`)

```mermaid
flowchart LR
    A[Dashboard /] --> B[Pendientes /aprobaciones]
    A --> C[Historial /aprobaciones/historial]
    A --> D[Calendario /calendario]
    B --> E[Detalle /aprobaciones/{id}]
    E --> F1[Aprobar POST]
    E --> F2[Rechazar POST]
    E --> F3[Desactivar POST]
```

**Primary Nav Items (header/sidebar):**
1. **Pendientes** → `/aprobaciones`
2. **Historial** → `/aprobaciones/historial`
3. **Calendario** → `/calendario`

**Context Switcher** (visible only when identity has 2+ roles): Labeled **"Mis roles"** — dropdown with available contexts (`Mi espacio`, `Aprobaciones`, `RRHH`); hidden when <2 roles; switching updates route prefix and navigation only; does not modify identity, session, roles, claims, or permissions.

### 7.3 HR Context Navigation (`RRHH`)

```mermaid
flowchart LR
    A[Dashboard /rrhh] --> B[Solicitudes /rrhh/solicitudes]
    A --> C[Calendario /rrhh/calendario]
    A --> D[Saldos /rrhh/saldos]
    A --> E[Auditoría /rrhh/auditoria]
    A --> F[Aprobadores /rrhh/aprobadores]
    B --> G[Detalle /rrhh/solicitudes/{id}]
    D --> H[Movimientos /rrhh/saldos/{userId}]
    F --> I[Capacidad /rrhh/aprobadores/{id}/capacidad]
```

**Primary Nav Items (header/sidebar):**
1. **Solicitudes** → `/rrhh/solicitudes`
2. **Calendario** → `/rrhh/calendario`
3. **Saldos** → `/rrhh/saldos`
4. **Auditoría** → `/rrhh/auditoria`
5. **Aprobadores** → `/rrhh/aprobadores`

**Context Switcher** (visible only when identity has 2+ roles): Labeled **"Mis roles"** — dropdown with available contexts (`Mi espacio`, `Aprobaciones`, `RRHH`); hidden when <2 roles; switching updates route prefix and navigation only; does not modify identity, session, roles, claims, or permissions.

---

## 8. View-Level Accessibility Checklist

Every view MUST satisfy the baseline from `frontend-design-spec.md` §8 plus the following per-view requirements.

### 8.1 Common to All Views

- [ ] Semantic HTML5 landmarks (`<header>`, `<main>`, `<nav>`, `<footer>`)
- [ ] Single `<h1>` matching view purpose
- [ ] Heading hierarchy (h1 → h2 → h3) without gaps
- [ ] Visible focus outline on all interactive elements (per design token `--nl-color-blue-600`, 2px offset)
- [ ] Minimum 44×44px touch targets on all buttons, links, form controls
- [ ] `prefers-reduced-motion: reduce` respected (animations ≤ 1ms)
- [ ] Color contrast ≥ 4.5:1 for text, ≥ 3:1 for UI components
- [ ] Status badges include text label; color is not sole conveyor
- [ ] Form inputs have associated `<label>` or `aria-label`
- [ ] Validation errors announced via `aria-live="polite"` region
- [ ] Tables have `<caption>`, `<th scope="col">`, and row `<th scope="row">` where applicable
- [ ] No horizontal scrolling at 320px viewport width

### 8.2 Mis Solicitudes (List)

- [ ] Table rows keyboard-navigable; `Enter`/`Space` opens detail
- [ ] Status column uses semantic badges with text
- [ ] Empty state has descriptive text and create action link
- [ ] Sortable columns announce sort direction via `aria-sort`
- [ ] **Server-side pagination** with accessible controls: page size selector (10/25/50), first/prev/next/last with ellipsis, `aria-label` for each control, total count announced via `aria-live="polite"`

### 8.3 Crear / Editar Solicitud

- [ ] Dual-mode selector announced as radio group (`role="radiogroup"`)
- [ ] Date inputs use `type="date"` with accessible label
- [ ] Working-days preview updates via `aria-live="polite"`
- [ ] Balance impact shown before submit with clear wording
- [ ] Submit button disabled during submission; loading state announced
- [ ] Revalidation errors focus first invalid field

### 8.4 Detalle de Solicitud (User)

- [ ] Audit timeline as `<ol>` with `datetime` attributes
- [ ] Balance impact card uses semantic state colors with text
- [ ] Edit link only visible when status=`Pending` and user=owner

### 8.5 Mi Historial

- [ ] Four summary cards: Acumulado total, Pendientes, Días gozados, Disponible
- [ ] Available balance emphasized (larger type, primary color)
- [ ] Balance movements timeline with date, concept, amount, resulting balance
- [ ] Request-related movements/history already available
- [ ] Reservation breakdown as definition list `<dl>`

### 8.6 Calendario Básico (User)

- [ ] Month grid as `<table>` with `scope="col"` day headers (Mon–Sun)
- [ ] **Weekend columns (Sat/Sun) visually distinct**: muted background (`--nl-color-border-subtle`), no event rendering
- [ ] Approved periods as `<span class="nl-calendar-event">` with `aria-label` (format: `Solicitud propia, del {start} al {end}, {días} días`)
- [ ] Keyboard navigation between days (arrow keys: Left/Right/Up/Down move by day/week)
- [ ] `Home`/`End` jump to first/last day of month; `PageUp`/`PageDown` navigate months
- [ ] Current day highlighted with `2px solid --nl-color-blue-600` border, not color alone
- [ ] Focus indicator on day cell: `--nl-overlay` shadow + outline
- [ ] **Events keyboard-activatable**: `Enter`/`Space` on day with event → navigate to `/mis-solicitudes/{id}`
- [ ] No events rendered on weekend cells; weekend days non-focusable for event navigation
- [ ] Month navigation: accessible `<button>` prev/next with `aria-label` "Mes anterior"/"Mes siguiente"
- [ ] `aria-live="polite"` region announces month change

### 8.7 Pendientes de Resolución (Approver)

- [ ] Table with requester name, dates, working days, balance available
- [ ] **Projected balance columns: Disponible actual, Días solicitados, Disponible después de aprobar**
- [ ] Overlap warning column with icon + text
- [ ] Action buttons (Aprobar / Rechazar / Desactivar) in same row
- [ ] Row `data-request-id` for traceability
- [ ] **Server-side pagination** with accessible controls: page size selector (10/25/50), first/prev/next/last with ellipsis, `aria-label` for each control, total count announced via `aria-live="polite"`

### 8.8 Detalle para Resolución (Approver)

- [ ] Full request detail mirrored from User detail
- [ ] **Projected balance card: Disponible actual, Días solicitados, Disponible después de aprobar (server-calculated)**
- [ ] Revalidation banner if balance/overlap changed since list load
- [ ] Rejection reason `textarea` with `aria-describedby` linking to char-count hint
- [ ] Charounter live region (10–500 chars)
- [ ] Approve/Reject/Deactivate buttons use `aria-pressed` during async call
- [ ] Deactivate button only visible when status=`Approved` and start date > today
- [ ] **Approve button disabled when projected balance is negative; warning displayed**

### 8.9 Calendario Básico (Approver)

- [ ] Same keyboard/structure requirements as User calendar (§8.6)
- [ ] **Weekend columns (Sat/Sun) visually distinct**: muted background, no event rendering
- [ ] Events anonymized per policy (show date ranges only, no requester names)
- [ ] **Calendar events keyboard-activatable (Enter/Space) → navigate to `/aprobaciones/{id}`**
- [ ] Month navigation buttons accessible with `aria-label`
- [ ] `aria-live="polite"` region announces month change

### 8.10 Historial de Resoluciones (Approver)

- [ ] Table with columns: date, action (Aprobó/Rechazó/Desactivó), requester, status, audit link
- [ ] **Server-side pagination** with accessible controls: page size selector (10/25/50), first/prev/next/last with ellipsis, `aria-label` for each control, total count announced via `aria-live="polite"`
- [ ] Sortable columns (date, action, requester) with `aria-sort` announcement
- [ ] Filterable by date range, action type, requester
- [ ] Row click navigates to read-only detail (`/aprobaciones/{id}`)
- [ ] Empty state: "No hay resoluciones realizadas" with link to pending queue

### 8.11 Solicitudes RRHH (List)

- [ ] Filterable by status, date range, requester
- [ ] Server-side pagination with accessible controls
- [ ] Status badges with text labels
- [ ] Row click navigates to read-only detail

### 8.12 Detalle de Solicitud RRHH

- [ ] Same content as Approver detail, read-only
- [ ] Audit trail fully visible
- [ ] No resolution action buttons present
- [ ] **Projected balance displayed read-only: Disponible actual, Días solicitados, Disponible después de aprobar**

### 8.13 Calendario Organizacional RRHH

- [ ] Same keyboard/structure requirements as User calendar (§8.6)
- [ ] **Weekend columns (Sat/Sun) visually distinct**: muted background, no event rendering
- [ ] Events show requester name (HR-authorized) + working-day count (e.g., `Juan Pérez — 5 días`)
- [ ] Multiple events per day stack vertically with `+N más` overflow indicator
- [ ] Filter toolbar: department/team `<select>` (accessible label), `Buscar` input (debounced 300ms)
- [ ] **Calendar events keyboard-activatable (Enter/Space) → navigate to `/rrhh/solicitudes/{id}`**
- [ ] Month navigation buttons accessible with `aria-label`
- [ ] `aria-live="polite"` region announces month change and filter results count
- [ ] Tooltip on hover/focus: requester, date range, working days, status `Aprobada`

### 8.14 Saldos y Movimientos RRHH

- [ ] Sortable, filterable table
- [ ] Balance movements as timeline with semantic indicators
- [ ] **Balance summary cards use labels: Acumulado total, Pendientes, Días gozados, Disponible**
- [ ] **Server-side pagination** for balances table: page size selector, first/prev/next/last with ellipsis, `aria-label` controls, total count via `aria-live="polite"`

### 8.15 Gestión de Aprobadores RRHH

- [ ] Table with toggle buttons for `canResolveRequests`
- [ ] Toggle opens modal with required reason, confirmation, row version
- [ ] Inactive approvers clearly marked
- [ ] Audit trail link for each capability change
- [ ] **Server-side pagination** for approvers table: page size selector, first/prev/next/last with ellipsis, `aria-label` controls, total count via `aria-live="polite"`

### 8.16 Auditoría Relevante RRHH

- [ ] Filterable table: date range, actor, action, entity, result
- [ ] Server-side pagination with accessible controls: page size selector, first/prev/next/last, `aria-label`, total via `aria-live="polite"`
- [ ] Semantic column headers with `scope="col"`; row actions via keyboard
- [ ] Redacted sensitive data in payload column (no reasons/tokens/secrets exposed)
- [ ] Sortable columns with `aria-sort` announcement
- [ ] Responsive: card fallback on mobile with timestamp + actor + action + result

### 8.17 Login Screen (ASP.NET Core Identity)

- [ ] NovaLeave branding with design tokens (navy-900, blue-600)
- [ ] White surface card with subtle border and shadow
- [ ] Email/username and password fields with visible `<label>` elements
- [ ] Client-side and server-side validation with alert feedback
- [ ] Preserved email on validation failure
- [ ] Accessible: full keyboard nav, visible focus, autocomplete attrs, aria-describedby for errors
- [ ] Responsive: centered card mobile, constrained desktop
- [ ] No emojis, no gradients
- [ ] Antiforgery, secure cookies, lockout, session behavior preserved

---

## 9. Security and Authorization at the Routing Layer

### 9.1 Policy Definitions

| Policy | Requirement |
|--------|-------------|
| `RequireActiveUser` | Authenticated, `Active` status, `User` claim present |
| `RequireActiveApprover` | Authenticated, `Active` status, `Approver` claim present |
| `RequireActiveHR` | Authenticated, `Active` status, `HR` claim present |
| `RequireRequestOwner` | Resource owner matches current identity |
| `RequireApproverEligible` | Active Approver, not owner, request in eligible state |
| `RequireApproverNotOwner` | Active Approver, not owner of target request |
| `RequirePreStartDeactivation` | Request `Approved` + start date > system business date |
| `RequireHRForApproverManagement` | Active HR, target has Approver role |

### 9.2 Controller Authorization

```csharp
// User context controllers
[Authorize(Policy = "RequireActiveUser")]
public class MisSolicitudesController : Controller { }

[Authorize(Policy = "RequireActiveUser")]
public class SaldoController : Controller { }

// Approver context controllers
[Authorize(Policy = "RequireActiveApprover")]
public class AprobacionesController : Controller { }

// HR context controllers
[Authorize(Policy = "RequireActiveHR")]
public class RRHHController : Controller { }

// Shared
[Authorize(Policy = "RequireActiveUser", "RequireActiveApprover", "RequireActiveHR")]
public class CalendarioController : Controller { }
```

### 9.3 Resource Authorization (Per-Action)

All mutation endpoints MUST revalidate in the Application layer:

- Identity is Active
- Role matches required role for action
- Ownership/eligibility for target resource
- Request state allows the transition
- Concurrency token matches (optimistic lock)

**HR-specific prohibitions enforced in Application layer:**

- HR MUST NOT approve, reject, or deactivate any request
- HR MUST NOT modify vacation balances
- HR MUST NOT assign or remove roles
- HR may only toggle `canResolveRequests` for identities that already have `Approver` role

Hidden nav, disabled buttons, or client-side checks are NOT authorization controls.

---

## 10. Shared Components Referenced

The following components are defined in `frontend-design-spec.md` and MUST be used consistently:

- `.nl-button-primary`, `.nl-button-secondary`, `.nl-button-danger`
- `.nl-card`, `.nl-overlay`
- Status badges: `.nl-badge-pending`, `.nl-badge-approved`, `.nl-badge-rejected`, `.nl-badge-timeout`, `.nl-badge-cancelled-approver`
- `.nl-skeleton` for loading
- `.nl-table`, `.nl-form`, `.nl-input`, `.nl-select`, `.nl-textarea`
- Toast/alert components with semantic colors
- Modal confirmation for destructive actions

---

## 11. Acceptance Criteria

| ID | Scenario | Expected Result |
|----|----------|-----------------|
| RBFV-001 | Active User navigates to `/mis-solicitudes` | 200 OK, list view rendered |
| RBFV-002 | Active User navigates to `/aprobaciones` | 403 Forbidden → `/acceso-denegado` |
| RBFV-003 | Active Approver navigates to `/aprobaciones` | 200 OK, list view rendered |
| RBFV-004 | Active Approver navigates to `/mis-solicitudes/crear` | 403 Forbidden |
| RBFV-005 | Active HR navigates to `/rrhh/solicitudes` | 200 OK, read-only list rendered |
| RBFV-006 | Active HR attempts `/aprobaciones/{id}/aprobar` | 403 Forbidden |
| RBFV-007 | Active HR attempts balance edit endpoint | 403 Forbidden |
| RBFV-008 | Active HR attempts role assignment/removal | 403 Forbidden |
| RBFV-009 | Dual-role identity switches context | Header updates, route prefix changes, no session modification |
| RBFV-010 | Triple-role identity sees all three contexts in switcher | Dropdown shows `Mi espacio`, `Aprobaciones`, `RRHH` |
| RBFV-011 | Inactive User attempts `/mis-solicitudes` | 403 Forbidden (policy `RequireActiveUser` fails) |
| RBFV-012 | Inactive Approver attempts `/aprobaciones/{id}/aprobar` | 403 Forbidden (policy `RequireActiveApprover` fails) |
| RBFV-013 | Inactive HR attempts `/rrhh/aprobadores/{id}/capacidad` | 403 Forbidden (policy `RequireActiveHR` fails) |
| RBFV-014 | Approver attempts to approve own request | 403 Forbidden (policy `RequireApproverNotOwner` fails) |
| RBFV-015 | HR toggles `canResolveRequests` for non-Approver | 400 Bad Request (validation fails) |
| RBFV-016 | HR toggles `canResolveRequests` without reason | 400 Bad Request (validation fails) |
| RBFV-017 | HR toggles `canResolveRequests` with stale row version | 409 Conflict (optimistic concurrency) |
| RBFV-018 | All views pass accessibility checklist (§8) | Axecore/PASS, manual keyboard audit PASS |
| RBFV-019 | Reduced-motion preference disables all non-essential animation | Verified via DevTools emulation |
| RBFV-020 | Multi-role identity sees context switcher labeled "Mis roles" | Dropdown/button shows "Mis roles" with options: Mi espacio, Aprobaciones, RRHH; hidden when <2 roles |
| RBFV-021 | Context switcher labeled "Mis roles" — switching does not modify session/roles/claims | Route prefix changes, navigation updates, identity/claims unchanged |
| RBFV-022 | User navigation shows "Mi historial" (not "Mi saldo") at `/saldo` | Label in nav/sidebar is "Mi historial"; route `/saldo` unchanged |
| RBFV-023 | Approver navigation shows "Historial" (not "Historial y Desactivación") | Nav item reads "Historial" at `/aprobaciones/historial` |
| RBFV-024 | Balance cards show "Acumulado total", "Pendientes", "Días gozados", "Disponible" | Four cards with exact labels; no "Devengado", "Reservado", "Deducido" as card titles |
| RBFV-025 | Approver list/detail shows projected balance: Disponible actual, Días solicitados, Disponible después de aprobar | Three values displayed; server-calculated; warning + disable Approve when negative |
| RBFV-026 | Calendar event keyboard activation opens authorized detail by role | User→/mis-solicitudes/{id}, Approver→/aprobaciones/{id}, HR→/rrhh/solicitudes/{id}; no link if unauthorized |
| RBFV-027 | No emoji text in any view (navigation, buttons, cards, statuses, alerts, modals, login) | Visual grep for emoji ranges returns zero matches in rendered views |
| RBFV-028 | Request form shows only two input modes with generic labels | Radio group: "Fecha inicio + Fecha fin" and "Fecha inicio + Cantidad de días"; no "Modo A/B" |
| RBFV-029 | Weekend exclusion in working-day calculation (server + UI preview match) | Fri+1→Mon, range spanning weekend, weekend-only rejected, Sat/Sun start rejected; holidays counted |
| RBFV-030 | Approve/Reject are mutually exclusive — UI disables conflicting action on submit, server revalidates | After one POST, other button disabled; stale/duplicate POST returns conflict; reason required for Reject |
| RBFV-031 | Approver views: visual hierarchy, spacing, white cards, status badges, loading, empty states | Manual review against frontend-design-spec.md §20 |
| RBFV-032 | HR views: read-only indicators, no resolution actions, pagination, calendar names, capability modal | Manual review against frontend-design-spec.md §21 |
| RBFV-033 | Login screen uses NovaLeave branding, Identity flow, accessible, responsive, no emojis/gradients | Manual review against frontend-design-spec.md §12 |
| RBFV-034 | New eligible Pending request appears in `/aprobaciones` exactly once without manual sync; Approver queue excludes only owning Approver and ineligible states, never team/hierarchy/department/assigned-approver | After successful creation, redirect User shows success; next Approver open/refresh shows the request once; no organizational filter applied |

---

## 12. Out of Scope for This Specification

- Business validation rules (see `specs/001-leave-management-mvp/spec.md`)
- Design token values (see `frontend-design-spec.md` v1.3.0)
- Database schema, EF Core mappings, migrations
- Application use cases, handlers, validators
- Infrastructure, deployment, CI/CD
- API endpoints (no approved API for MVP)

---

## 13. Version History

| Version | Date | Author | Change |
|---------|------|--------|--------|
| 1.0.0 | 2026-07-23 | — | Initial draft aligned with Constitution v5.0.0 |
| 2.0.0 | 2026-07-23 | — | Added HR/RRHH context; updated to Constitution v6.0.0; removed stale HR-exclusion warnings |

---

**Constitution Alignment**: This specification is subordinate to `.specify/memory/constitution.md` v6.0.0 and `specs/001-leave-management-mvp/frontend-design-spec.md` v1.3.0. Any conflict MUST be resolved by amending the authoritative source.