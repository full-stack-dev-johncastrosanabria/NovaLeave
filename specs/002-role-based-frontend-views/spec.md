# Role-Based Frontend Views Specification — NovaLeave MVP

**Related Feature**: `001-leave-management-mvp` (complementary specification; not an independent feature and no separate implementation plan)
**Version**: 2.1.0
**Date**: 2026-08-04
**Status**: Audited against the committed `branch-john` frontend at `136d65e`. Approved requirements remain normative; implementation deviations and incomplete behaviors are recorded separately and are not legitimized by this specification.
**Constitution**: `.specify/memory/constitution.md` v7.0.0

---

## 1. Purpose

This specification defines the mandatory view and navigation structure for NovaLeave MVP based on the three application roles (`User`, `Approver`, `HR`) plus the automatic system actor. It complements `specs/001-leave-management-mvp/spec.md` (business behavior) and `specs/001-leave-management-mvp/frontend-design-spec.md` v1.3.0 (design tokens, components, accessibility baseline).

It also records the presentation structure verifiably implemented in the committed `branch-john` snapshot. Implementation evidence is descriptive only: it MAY confirm an approved rule, but it MUST NOT override a higher-authority rule or turn an incomplete, experimental, or non-conforming implementation into a normative requirement.

All routes, views, and navigation elements described here MUST conform to the Constitution v7.0.0, the approved frontend design specification v1.3.0, and the authority order in Constitution §15.1.

---

## 2. Scope

This specification covers:

- Route-to-role mapping and exclusive access rules
- Required views per role
- Shared views accessible by multiple roles
- Navigation structure per role
- The verified `branch-john` application shell, shared presentation components, and repeated view patterns
- The conformance status of visible behaviors implemented in `branch-john`
- View-level accessibility requirements
- Security and authorization boundaries at the routing level
- Mandatory extension rules for future role-based views

This specification does NOT cover:

- Business rules (see `specs/001-leave-management-mvp/spec.md`)
- Design tokens, colors, spacing, motion (see `frontend-design-spec.md`)
- API contracts (no approved API exists for the MVP)
- Design values copied from implementation; all colors, typography, spacing, breakpoints, motion, shadows, radii, and dimensions remain governed by `frontend-design-spec.md`
- Remediation of implementation deviations identified by this audit

---

## 3. Authority Order

Per Constitution §15.1:

1. **Constitution v7.0.0** — architecture, security, roles, cross-cutting invariants, engineering quality, governance
2. **Approved feature specifications** — business behavior and acceptance criteria within constitutional boundaries
3. **Approved frontend design specification v1.3.0** — design tokens, responsive behavior, accessibility, components, motion, interaction patterns
4. **Approved ADRs** — authorized architectural exceptions
5. **Code and tests** — implement approved requirements but do not override them
6. **Prototypes, screen-construction guides, OKF, Mermaid, Graphify** — derived or reference artifacts; cannot introduce requirements, roles, or permissions

### 3.1 Complementary Authority Boundary

This specification complements `specs/001-leave-management-mvp/spec.md`. It is
authoritative only for frontend routes, navigation, role-based view behavior,
accessibility, and presentation.

It must not redefine domain rules, lifecycle states, balance semantics, backend
authorization invariants, or MVP scope. In any conflict, the Constitution and
the primary MVP specification prevail.

### 3.2 Audit Evidence and Classification

The `branch-john` audit uses committed files under `src/NovaLeave.Web/`, related Presentation/integration/E2E tests, and the branch's own commit history as implementation evidence. Uncommitted working-tree changes are excluded from the evidence baseline.

Findings use these classifications:

- `ALREADY_DOCUMENTED`: the approved requirement and the verified implementation agree.
- `DOCUMENTATION_GAP`: valid committed behavior exists and is added descriptively or normatively within this specification's authority.
- `STALE_SPEC`: wording described a prior presentation and is corrected without changing higher-authority behavior.
- `IMPLEMENTATION_DEVIATION`: committed code conflicts with a higher-authority source; the approved rule is retained.
- `OUT_OF_SCOPE`: the finding belongs to another authoritative artifact.
- `INCOMPLETE_IMPLEMENTATION`: only part of an approved behavior exists; the complete behavior is not claimed as implemented.
- `BRANCH_ONLY_EXPERIMENT`: branch evidence is insufficient to make the behavior normative.

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
| `/aprobaciones` | GET | `Approver` (Active, `canResolveRequests=true`) | List eligible pending requests |
| `/aprobaciones/{id}` | GET | `Approver` (Active, `canResolveRequests=true`, Eligible) | View request detail for resolution |
| `/aprobaciones/{id}/aprobar` | POST | `Approver` (Active, `canResolveRequests=true`, Eligible, Not Owner) | Approve request |
| `/aprobaciones/{id}/rechazar` | POST | `Approver` (Active, `canResolveRequests=true`, Eligible, Not Owner) | Reject request |
| `/aprobaciones/{id}/desactivar` | POST | `Approver` (Active, `canResolveRequests=true`, Eligible, Not Owner) | Deactivate approved request (pre-start) |
| `/aprobaciones/historial` | GET | `Approver` (Active, `canResolveRequests=true`) | View own resolution history |
| `/rrhh` | GET | `HR` (Active) | HR dashboard |
| `/rrhh/solicitudes` | GET | `HR` (Active) | Read-only request list (org-wide) |
| `/rrhh/solicitudes/{id}` | GET | `HR` (Active) | Read-only request detail |
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
| `/calendario` | GET | `User` (Active) \| `Approver` (Active, `canResolveRequests=true`) | Basic vacation calendar for User personal scope and Approver anonymized scope only; HR MUST use `/rrhh/calendario` |
| `/acceso-denegado` | GET | Any (including unauthenticated) | Branded 403 page |
| `/error` | GET | Any | Branded error page |

`branch-john` currently implements `/` as a role-aware redirect, with priority
`User` → `/mis-solicitudes`, `Approver` → `/aprobaciones`, and `HR` → `/rrhh`.
It does not render a shared dashboard at `/`. The approved branded
`/acceso-denegado` and `/error` views are not both present as dedicated routes
in the committed Presentation tree; this is recorded as incomplete
implementation in §11.2 and MUST NOT be interpreted as route removal.

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
| **Detalle para Resolución** | `/aprobaciones/{id}` | Detail view with resolution actions | Full request info, **balance revalidation with projected balance**, overlap revalidation, approve/reject/deactivate buttons, rejection reason textarea |
| **Historial de Resoluciones** | `/aprobaciones/historial` | List of requests resolved by this approver | Table with date, action, requester, status, audit link |
| **Calendario Básico** | `/calendario` | Month-view calendar showing approved periods (org-wide, anonymized per policy) for Approvers with `canResolveRequests=true` | Month grid, event markers, legend |

### 6.3 HR Context (`RRHH`)

| View | Route | Purpose | Key Components |
|------|-------|---------|----------------|
| **Dashboard RRHH** | `/rrhh` | Organization-wide summary cards | Total pending, active approvers, balance summary, quick links |
| **Solicitudes (Solo Lectura)** | `/rrhh/solicitudes` | Filterable, paginated list of all requests | Table with requester, dates, status, working days, reservation, deduction |
| **Detalle de Solicitud (Solo Lectura)** | `/rrhh/solicitudes/{id}` | Full request detail including audit trail | Same as Approver detail but read-only; no resolution actions |
| **Calendario Organizacional** | `/rrhh/calendario` | Dedicated read-only HR calendar route showing all vacation requests organization-wide; HR MUST NOT use `/calendario` | Month grid, event markers, legend, filter by requester name if applicable |
| **Saldos (Solo Lectura)** | `/rrhh/saldos` | List all users with balance summary | Table with user, **Acumulado total, Pendientes, Días gozados, Disponible** |
| **Movimientos de Saldo (Solo Lectura)** | `/rrhh/saldos/{userId}` | Balance history for user | Timeline of accruals, reservations, deductions, restorations |
| **Auditoría Relevante** | `/rrhh/auditoria` | Filterable audit log | Table with timestamp, actor, role, action, entity, result |
| **Gestión de Aprobadores** | `/rrhh/aprobadores` | List all identities with `Approver` role and `canResolveRequests` | Table with name, email, active status, canResolveRequests, toggle action |
| **Activar/Desactivar Capacidad** | `/rrhh/aprobadores/{id}/capacidad` | Modal with reason, confirmation, row version | Confirmation modal, reason textarea (required), row version check, audit |

### 6.4 Shared Views

| View | Route | Purpose | Key Components |
|------|-------|---------|----------------|
| **Role-Aware Landing Redirect** | `/` | Redirect to the first authorized role entry point; no dashboard markup is rendered at this route in `branch-john` | Server-side role-aware redirect; destination shell supplies context switcher when applicable |
| **Acceso Denegado** | `/acceso-denegado` | Branded 403 with context-aware message | Friendly message, return link, contact hint |
| **Error** | `/error` | Branded error page | Correlation ID, support reference |

### 6.5 Verified `branch-john` Presentation Structure

The following structure is committed and observable in `branch-john`. It is an
implementation profile, not a replacement for any unmet normative requirement
elsewhere in this specification.

#### 6.5.1 Authenticated Application Shell

- Authenticated views reuse `Views/Shared/_Layout.cshtml`.
- The shell contains a persistent navigation rail on large viewports, a sticky
  page header, a page title derived from `ViewData["Title"]`, role-context
  navigation, the current identity, session-state text, and a POST logout action.
- A skip link targets the main content region. Active navigation uses visible
  styling plus `aria-current`; color is not the only active-page cue.
- On smaller viewports, a menu button opens the navigation rail as an overlay.
  The shared JavaScript closes it through the backdrop or `Escape` and keeps
  `aria-expanded` synchronized.
- Unauthenticated views use the same layout file but render in a centered,
  single-column content container without the authenticated shell.
- Breadcrumbs are not implemented in the committed branch and MUST NOT be
  described as an existing shared component.

#### 6.5.2 Header and Context Interaction

- The header displays the current page title and a direct logout control.
- Multi-role identities receive a shared dropdown whose accessible name includes
  `Mis roles` and the active context. The selected item has a check icon,
  `aria-current`, and visually hidden `contexto actual` text.
- The context switcher links to the context landing route. It changes route and
  visible navigation only; it does not mutate identity, session, roles, claims,
  capability, ownership, or authorization.
- Shared calendar navigation preserves context explicitly with
  `/calendario?context=User` or `/calendario?context=Approver`. HR navigation
  uses only `/rrhh/calendario`.
- The approved user-menu structure in `frontend-design-spec.md` §6.6 is not
  complete in `branch-john`; the current header exposes logout directly.

#### 6.5.3 Repeated View Composition

- Operational lists use responsive wrappers around compact semantic tables with
  captions, scoped headers, right-aligned numeric columns, and explicit action
  links.
- Summary information uses responsive grids of `.nl-card` and `.nl-stat`
  surfaces. Numeric values use `.nl-num` or tabular-number styling.
- Empty list states use `.nl-empty`, a decorative icon, plain Spanish text, and
  a next action when one is authorized.
- Forms use visible labels, Razor Tag Helpers where a typed form ViewModel is
  available, server validation summaries, help text, antiforgery protection for
  mutations, and explicit submit/cancel actions.
- Status presentation SHOULD flow through `_StatusBadge.cshtml`; raw enum output
  in committed views is a deviation, not an alternative convention.
- Table pagination, filters, searches, sorting controls, reusable breadcrumbs,
  and generic modal components are not consistently implemented. Their presence
  in handlers or query parameters alone MUST NOT be documented as completed UI.

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

The `RRHH` context calendar navigation MUST point only to `/rrhh/calendario`. The shared `/calendario` route has no HR behavior and MUST NOT expose HR global-calendar data.

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
1. **Panel RRHH** → `/rrhh`
2. **Solicitudes** → `/rrhh/solicitudes`
3. **Calendario** → `/rrhh/calendario`
4. **Saldos** → `/rrhh/saldos`
5. **Auditoría** → `/rrhh/auditoria`
6. **Aprobadores** → `/rrhh/aprobadores`

`Panel RRHH` is active only on the exact `/rrhh` route. Nested routes activate
their specific navigation item so two sidebar options are never marked current.

**Context Switcher** (visible only when identity has 2+ roles): Labeled **"Mis roles"** — dropdown with available contexts (`Mi espacio`, `Aprobaciones`, `RRHH`); hidden when <2 roles; switching updates route prefix and navigation only; does not modify identity, session, roles, claims, or permissions.

---

### 7.4 Verified Screen Patterns by Context

#### User

- `/mis-solicitudes` displays a compact count summary, an authorized
  `Nueva solicitud` action, a responsive request table, shared status badges,
  and a first-use empty state.
- `/mis-solicitudes/crear` displays authoritative balance context in summary
  cards, the two approved input-mode labels, date/day fields, reason help and
  character count, an informational client preview, a server validation summary,
  and a duplicate-submission loading state. Every derived value remains
  informational and MUST be revalidated server-side.
- `/mis-solicitudes/{id}` uses a summary-first detail with the shared status
  badge, period, working-day and creation-date facts, separate reason and
  follow-up surfaces, rejection context when applicable, and a conditional edit
  action for `Pending`. The fuller approved audit and balance presentation is
  incomplete and no balance impact is inferred in Razor.
- `/mis-solicitudes/{id}/editar` exists as a typed form with a hidden row-version
  token, but it does not yet reuse the complete create-form interaction pattern.
- `/saldo` uses summary cards and a movement table/empty state. Approved visible
  balance terminology remains authoritative even where committed labels differ.

#### Approver

- `/aprobaciones` uses a responsive queue table with requester, period,
  requested days, available balance, projected balance, a visible insufficient-
  balance cue, and a `Resolver` link.
- `/aprobaciones/{id}` separates request information and resolution actions into
  responsive cards. It submits row-version tokens and antiforgery tokens for
  approval, rejection, and deactivation. Approved confirmation, conflict,
  loading, and projected-balance presentation requirements remain binding where
  the current implementation is partial.
- `/aprobaciones/historial` is currently a basic history table. Approved
  filtering, sorting, pagination, empty state, and responsive behavior are not
  complete.

#### HR

- `/rrhh` is an implemented read-only overview composed of organizational
  summary cards, recent requests, a status distribution, recent audit activity,
  and quick links to the HR work areas.
- HR request, balance, movement, audit, and approver-capability pages use typed
  table/detail/form views. The committed request, balance, and audit queries may
  be paged, but the views expose only a page summary rather than the complete
  approved pagination controls.
- `/rrhh/aprobadores/{id}/capacidad` uses a Bootstrap-compatible confirmation
  modal, explicit confirmation field, required reason input, and hidden
  row-version submission. The row version MUST remain a technical concurrency
  input and SHOULD NOT be exposed as visible user content.
- HR read-only presentation MUST remain visually explicit on every applicable
  HR view; the existing isolated labels do not satisfy that requirement across
  the entire context.

#### Calendar

- User, Approver, and HR calendar views reuse `_Calendar.cshtml`.
- The shared partial renders a Monday-to-Sunday semantic grid, visibly muted
  weekend cells, today indication, at most two visible event chips per day with
  a `+N más` overflow count, an authorized event list, and an empty-month state.
- User events link to owned detail; HR events link to HR read-only detail.
  Approver events are anonymized in the current branch.
- Grid roles, grid-cell roles, focusable weekday cells, event accessible names,
  and data hooks are present. Complete arrow-key navigation, month controls,
  focus management, and the approved role-specific navigation behavior MUST NOT
  be claimed as implemented until executable behavior and tests verify them.

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
- [ ] Filter toolbar: requester name `<input>` (accessible label), `Buscar` input (debounced 300ms)
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
| `RequireActiveApprover` | Authenticated, `Active` status, `Approver` claim present, `canResolveRequests=true` |
| `RequireActiveHR` | Authenticated, `Active` status, `HR` claim present |
| `RequireRequestOwner` | Resource owner matches current identity |
| `RequireApproverEligible` | Active Approver with `canResolveRequests=true`, not owner, request in eligible state |
| `RequireApproverNotOwner` | Active Approver with `canResolveRequests=true`, not owner of target request |
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

// Shared User/Approver calendar only; HR uses RRHHController at /rrhh/calendario.
[Authorize(Policy = "RequireActiveUserOrEligibleApprover")]
public class CalendarioController : Controller { }
```

### 9.3 Resource Authorization (Per-Action)

All mutation endpoints MUST revalidate in the Application layer:

- Identity is Active
- Role matches required role for action
- `canResolveRequests=true` for Approver queue, detail, and resolution actions
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

### 10.1 Approved Component Conventions

The component conventions defined in `frontend-design-spec.md` MUST be used
consistently. Their token values and fixed visual measurements MUST be referenced
from that document and MUST NOT be redefined here. These include buttons,
surfaces, tables, forms, status badges, skeletons, alerts, toasts, overlays, and
confirmation modals.

### 10.2 Reusable Components Verified in `branch-john`

| Component or Pattern | Evidence | Verified Responsibility | Extension Rule |
|----------------------|----------|-------------------------|----------------|
| Application shell | `Views/Shared/_Layout.cshtml` | Authenticated navigation rail, sticky header, page title, context switcher, logout, skip link, responsive sidebar | Future authenticated views MUST reuse it |
| Calendar partial | `Views/Shared/_Calendar.cshtml` | Shared role-aware calendar grid, event chips/list, authorized detail links, empty month | Calendar views MUST reuse it and controllers MUST supply the authorized role context |
| Status badge partial | `Views/Shared/_StatusBadge.cshtml` | Icon, visible Spanish label, semantic badge class, canonical `data-status` | Request states SHOULD use this partial rather than raw enum text |
| Toast partial | `Views/Shared/_Toast.cshtml` | Non-critical status feedback with polite live-region semantics | Success feedback SHOULD reuse it where toast feedback is appropriate |
| Validation summary partial | `Views/Shared/_ValidationSummary.cshtml` | Shared server-side validation summary | Forms SHOULD reuse it instead of duplicating summary markup |
| Card/stat patterns | `.nl-card`, `.nl-card-header`, `.nl-stat`, `.nl-stat-label`, `.nl-stat-value`, `.nl-stat-hint` | Compact summaries and dashboard groupings | Existing compliant patterns SHOULD be reused |
| Table/empty patterns | `.nl-table`, `.nl-empty`, `.table-responsive` | Responsive operational lists and first-use/no-result states | Existing compliant patterns SHOULD be reused |
| Calendar/status patterns | `.nl-calendar`, `.nl-event`, `.nl-status`, `.nl-num` | Calendar layout, compact events, icon-plus-text status, stable numeric display | Existing compliant patterns SHOULD be reused |
| Shared JavaScript module | `wwwroot/js/novaleave.js` | Responsive sidebar and create-request interaction initialization | New shared behavior MUST extend the approved module structure and MUST NOT duplicate initialization in views |
| Capability confirmation | `Views/RRHH/ApproverCapabilities/Capability.cshtml` | Bootstrap-compatible confirmation modal for HR capability change | The interaction pattern SHOULD be reused only for actions with equivalent approved confirmation semantics |

No reusable breadcrumb, pagination, filter-toolbar, search, or generic modal
partial is present in the committed branch. No View Component or project-owned
Tag Helper implements these patterns. Future work MUST NOT claim reuse of a
component that does not exist.

## 11. `branch-john` Conformance Audit

### 11.1 Confirmed and Documentary Findings

| Classification | Finding | Evidence | Documentation Outcome |
|----------------|---------|----------|-----------------------|
| `ALREADY_DOCUMENTED` | Role-separated routes, HR-only `/rrhh/calendario`, and server-side authorization boundaries match the approved role model | Controllers, authorization tests, §§4–9 | Retained |
| `ALREADY_DOCUMENTED` | The multi-role switcher changes route/navigation without changing identity or claims | `_Layout.cshtml`, `UC02SwitchRoleContextTests` | Retained and clarified |
| `DOCUMENTATION_GAP` | Authenticated screens share a navigation rail, sticky header, skip link, page title, session identity, direct logout, and responsive overlay navigation | `_Layout.cshtml`, `novaleave.js` | Added in §§6.5–7.4 |
| `DOCUMENTATION_GAP` | `branch-john` contains reusable calendar, status-badge, toast, validation-summary, card/stat, table, and empty-state patterns | `Views/Shared/`, `novaleave.css` | Added in §10.2 |
| `DOCUMENTATION_GAP` | HR has a committed overview with organization metrics, recent requests, status distribution, audit activity, and quick links | `Views/RRHH/Index.cshtml`, `RRHHDashboardViewModel` | Added in §7.4 |
| `STALE_SPEC` | `/` was described as a shared dashboard even though the committed route is a role-aware redirect | `Program.cs` | Corrected in §§5.2 and 6.4 |
| `STALE_SPEC` | The component list mixed approved design conventions with components not actually present as shared project artifacts | `Views/Shared/`, `novaleave.css` | Separated into approved conventions and verified components |
| `BRANCH_ONLY_EXPERIMENT` | htmx is vendored and loaded globally, but no `hx-*` behavior is present in committed Razor views | `libman.json`, `_Layout.cshtml`, no matching Razor usage | Not made normative |
| `OUT_OF_SCOPE` | Business calculations, lifecycle transitions, balances, permissions, and audit rules | Primary MVP specification and Constitution | Referenced, not duplicated |

### 11.2 Implementation Deviations and Incomplete Behaviors

The following findings do not amend approved requirements. They MUST remain
visible until separately remediated through an authorized implementation task.

| Classification | Finding | Higher-Authority Rule | Evidence in `branch-john` |
|----------------|---------|-----------------------|---------------------------|
| `IMPLEMENTATION_DEVIATION` | The branch replaces approved visual tokens and typography with a separate Fira Sans/Fira Code, blue/neutral token set and compact fixed values | `frontend-design-spec.md` §§4–5, 11 | `wwwroot/css/novaleave.css`, self-hosted Fira assets |
| `IMPLEMENTATION_DEVIATION` | CoreUI is loaded as the primary stylesheet and runtime without a frontend ADR; Bootstrap 5.3.x is the approved UI framework | Constitution §3.2 and §11.2; no related ADR exists | `libman.json`, `_Layout.cshtml` |
| `IMPLEMENTATION_DEVIATION` | Inline CSS and inline JavaScript/confirmation behavior remain in Razor views | Constitution §11.2; shared asset governance | Login, User balance, Approver list/detail, HR dashboard |
| `IMPLEMENTATION_DEVIATION` | Visible balance labels use `Acumulado`, `Reservado`, and `Deducido` in some User/HR views instead of the approved `Acumulado total`, `Pendientes`, and `Días gozados` terminology | `frontend-design-spec.md` §16; RBFV-024 | User balance and HR balance/movement views |
| `IMPLEMENTATION_DEVIATION` | Some request and movement states render raw technical enum values, and the shared badge uses shortened cancellation labels | Constitution §3.3 and §11.5; `frontend-design-spec.md` §§4.1 and 10 | User detail, Approver history, HR tables, `_StatusBadge.cshtml` |
| `IMPLEMENTATION_DEVIATION` | Approver calendar tests and rendering deliberately omit detail links for anonymized events, while primary FR-024 requires navigation for an authorized eligible Approver | MVP `FR-024`; `frontend-design-spec.md` §22 | `_Calendar.cshtml`, `CalendarAuthorizationTests` |
| `IMPLEMENTATION_DEVIATION` | A technical `RowVersion` value is displayed as visible HR content instead of remaining a hidden concurrency input | Plain-language presentation and technical-identifier boundaries | HR capability form |
| `DOCUMENTATION_UPDATED` | Login demo guidance is intentionally limited to the four user identifiers; roles, passwords, statuses, and credential instructions are excluded | `frontend-design-spec.md` §§13–13.1; RBFV-033 | Login Razor Page and authentication tests |
| `INCOMPLETE_IMPLEMENTATION` | The approved header user menu is absent; logout is exposed directly in the header | `frontend-design-spec.md` §6.6 | `_Layout.cshtml` |
| `INCOMPLETE_IMPLEMENTATION` | User detail lacks the approved audit/balance presentation; edit does not reuse the full dual-mode create pattern | §§6.1, 8.3–8.4 | User detail/edit views |
| `INCOMPLETE_IMPLEMENTATION` | Approver confirmation modals, mutually-exclusive loading behavior, full projected-balance labels, conflict feedback, and accessible rejection character count are incomplete | `frontend-design-spec.md` §§17, 20, 23; RBFV-025, RBFV-030 | Approver detail/list views |
| `INCOMPLETE_IMPLEMENTATION` | HR filter toolbars, searches, sorting, responsive card fallbacks, and complete pagination controls are not rendered across the HR context | MVP FR-009; `frontend-design-spec.md` §21; RBFV-032 | HR list/detail views and controllers |
| `INCOMPLETE_IMPLEMENTATION` | Calendar markup exposes accessibility hooks but lacks verified arrow-key/month navigation behavior and complete approved controls; static tests do not prove keyboard behavior | `frontend-design-spec.md` §25; RBFV-018, RBFV-026 | `_Calendar.cshtml`, `novaleave.js`, calendar tests |
| `INCOMPLETE_IMPLEMENTATION` | Dedicated branded `/acceso-denegado` and `/error` experiences with return/support/correlation guidance are not complete | Constitution §11.4; §§5.2 and 6.4 | Identity AccessDenied page; no dedicated Error view in committed tree |
| `INCOMPLETE_IMPLEMENTATION` | Several list views have no explicit empty state, filter/search UI, or reusable pagination component even where approved | §§8.10–8.16 | Approver history and HR operational views |

### 11.3 View, Route, Role, and Use-Case Traceability

| Context | View or Interaction | Route | Approved Use Case(s) | Primary Shared Pattern |
|---------|---------------------|-------|----------------------|------------------------|
| Shared | Login | `/Identity/Account/Login` | UC-01 | Unauthenticated layout, typed Identity PageModel, validation feedback |
| Shared | Context switching | Role landing routes; explicit calendar query context | UC-02 | `_Layout.cshtml` context switcher |
| User | Request list | `/mis-solicitudes` | UC-03 | `.nl-table`, `_StatusBadge`, `.nl-empty` |
| User | Create request | `/mis-solicitudes/crear` | UC-04 | `.nl-card`, `.nl-stat`, typed form, shared JavaScript module |
| User | Edit Pending request | `/mis-solicitudes/{id}/editar` | UC-05 | Typed form, validation, row version |
| User | Request detail | `/mis-solicitudes/{id}` | UC-06 | Definition-list detail, conditional action |
| User | Balance and movements | `/saldo` | UC-07 | `.nl-stat`, `.nl-table`, `.nl-empty` |
| User | Personal calendar | `/calendario?context=User` | UC-08 | `_Calendar.cshtml` |
| Approver | Pending queue | `/aprobaciones` | UC-09 | `.nl-table`, balance warning, `.nl-empty` |
| Approver | Resolution detail | `/aprobaciones/{id}` | UC-10–UC-13 | Responsive cards, protected forms, warning/feedback patterns |
| Approver | Resolution history | `/aprobaciones/historial` | UC-14 | `.nl-table` |
| Approver | Anonymized calendar | `/calendario?context=Approver` | UC-15 | `_Calendar.cshtml` |
| HR | Request list and detail | `/rrhh/solicitudes`, `/rrhh/solicitudes/{id}` | UC-18 | `.nl-table`, read-only detail |
| HR | Organizational calendar | `/rrhh/calendario` | UC-19 | `_Calendar.cshtml`, descriptive context panel |
| HR | Balances and movements | `/rrhh/saldos`, `/rrhh/saldos/{userId}` | UC-20 | `.nl-table`, read-only detail |
| HR | Relevant audit | `/rrhh/auditoria` | UC-21 | `.nl-table` |
| HR | Approver capability | `/rrhh/aprobadores`, `/rrhh/aprobadores/{id}/capacidad` | UC-22 | `.nl-table`, typed form, confirmation modal, `_Toast` |

---

## 12. Acceptance Criteria

| ID | Scenario | Expected Result |
|----|----------|-----------------|
| RBFV-001 | Active User navigates to `/mis-solicitudes` | 200 OK, list view rendered |
| RBFV-002 | Active User navigates to `/aprobaciones` | 403 Forbidden → `/acceso-denegado` |
| RBFV-003 | Active Approver with `canResolveRequests=true` navigates to `/aprobaciones` | 200 OK, list view rendered |
| RBFV-004 | Active Approver navigates to `/mis-solicitudes/crear` | 403 Forbidden |
| RBFV-005 | Active HR navigates to `/rrhh/solicitudes` | 200 OK, read-only list rendered |
| RBFV-006 | Active HR attempts `/aprobaciones/{id}/aprobar` | 403 Forbidden |
| RBFV-007 | Active HR attempts balance edit endpoint | 403 Forbidden |
| RBFV-008 | Active HR attempts role assignment/removal | 403 Forbidden |
| RBFV-009 | Dual-role identity switches context | Header updates, route prefix changes, no session modification |
| RBFV-010 | Triple-role identity sees all three contexts in switcher | Dropdown shows `Mi espacio`, `Aprobaciones`, `RRHH` |
| RBFV-011 | Inactive User attempts `/mis-solicitudes` | 403 Forbidden (policy `RequireActiveUser` fails) |
| RBFV-012 | Inactive Approver or Approver with `canResolveRequests=false` attempts `/aprobaciones`, `/aprobaciones/{id}`, or a resolution POST | 403 Forbidden (policy `RequireActiveApprover` or resource eligibility fails) |
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
| RBFV-032 | HR views: descriptive context, no resolution actions, pagination, calendar names at `/rrhh/calendario`, capability modal; HR access to `/calendario` is forbidden | Manual review against frontend-design-spec.md §21 and route authorization tests |
| RBFV-033 | Login screen uses NovaLeave branding, Identity flow, accessible, responsive, no emojis/gradients | Manual review against frontend-design-spec.md §13 |
| RBFV-034 | New eligible Pending request appears in `/aprobaciones` exactly once without manual sync; Approver queue excludes only owning Approver and ineligible states, never team/hierarchy/department/assigned-approver | After successful creation, redirect User shows success; next Approver open/refresh shows the request once; no organizational filter applied |

---

## 13. Out of Scope for This Specification

- Business validation rules (see `specs/001-leave-management-mvp/spec.md`)
- Design token values (see `frontend-design-spec.md` v1.3.0)
- Database schema, EF Core mappings, migrations
- Application use cases, handlers, validators
- Infrastructure, deployment, CI/CD
- API endpoints (no approved API for MVP)

---

## Frontend Extension Rules

These rules are mandatory for future screens derived from the approved
NovaLeave frontend and the conforming patterns verified in `branch-john`.

### Shell, Assets, and Reuse

- Future views MUST reuse the existing application layout and navigation shell.
- Existing shared components MUST be reused before creating view-specific alternatives.
- Views MUST NOT contain inline CSS.
- Views MUST NOT contain duplicated JavaScript initialization.
- Shared CSS MUST remain under the approved `wwwroot/css` structure.
- Shared JavaScript MUST remain under the approved `wwwroot/js` structure.
- Bootstrap 5.3.x MUST be used through the shared project conventions.
- Bootstrap versions MUST NOT be changed by this documentation task or by
  extensions governed by this specification without separate approval.
- Existing table patterns SHOULD be reused.
- Existing form patterns SHOULD be reused.
- Existing badge patterns SHOULD be reused.
- Existing alert patterns SHOULD be reused.
- Existing modal patterns SHOULD be reused.
- Existing empty-state patterns SHOULD be reused.
- A branch-only dependency or unused experiment MUST NOT become a required
  frontend convention without approval under the Constitution and, when
  applicable, an ADR.

### Language, Models, and Presentation Boundaries

- Visible UI content MUST remain in Spanish.
- Technical identifiers MUST remain in English.
- Every Razor view MUST use a dedicated ViewModel appropriate to its presentation purpose.
- Domain entities and EF entities MUST NOT be exposed directly to Razor.
- Business rules MUST be referenced from the primary MVP specification rather than duplicated.
- Design tokens and fixed visual values MUST be referenced from `frontend-design-spec.md` rather than redefined.
- Frontend calculations MAY provide informational previews only. Client-calculated
  balances, roles, ownership, statuses, or working-day totals MUST NOT be trusted.
- Sensitive data MUST NOT be exposed by presentation decisions.

### Authorization, Roles, and Routes

- Authorization MUST be enforced server-side.
- Visual hiding MUST NOT be treated as authorization.
- Unauthorized actions MUST NOT be displayed as executable controls.
- Frontend code MUST NOT create new roles.
- Frontend code MUST NOT create new permissions.
- Frontend code MUST NOT create new request states.
- Frontend code MUST NOT create unapproved routes.
- The contexts `User`, `Approver`, and `HR` MUST remain visually and navigationally distinct.
- Role switching MUST NOT modify identity, claims, session, permissions, ownership, or authorization.
- HR MUST use `/rrhh/calendario`.
- `/calendario` MUST NOT be reused as the global HR calendar.
- Future views MUST remain aligned with approved use cases and contracts.

### Responsive and Accessible Interaction

- New views MUST define responsive behavior using the approved rules in `frontend-design-spec.md`.
- New views MUST preserve WCAG 2.1 AA requirements.
- Keyboard navigation MUST remain supported.
- Focus indicators MUST remain visible.
- Color MUST NOT be the only status indicator.
- Required actions and feedback MUST remain reachable at every approved viewport.
- Loading, success, warning, validation, conflict, empty, and error states MUST
  use the appropriate shared pattern and MUST remain understandable without
  motion, color, or icons alone.

---

## 14. Version History

| Version | Date | Author | Change |
|---------|------|--------|--------|
| 1.0.0 | 2026-07-23 | — | Initial draft aligned with Constitution v5.0.0 |
| 2.0.0 | 2026-07-23 | — | Added HR/RRHH context; updated to Constitution v6.0.0; removed stale HR-exclusion warnings |
| 2.0.1 | 2026-07-30 | — | Clarified Approver `canResolveRequests` eligibility and HR-only `/rrhh/calendario` route |
| 2.1.0 | 2026-08-04 | — | Audited committed `branch-john` Presentation; documented the verified shell, role-specific view structure, reusable components, extension rules, and separate non-legitimizing deviation/incomplete-implementation findings |

---

**Constitution Alignment**: This specification is subordinate to `.specify/memory/constitution.md` v7.0.0 and `specs/001-leave-management-mvp/frontend-design-spec.md` v1.3.0. Any conflict MUST be resolved by amending the authoritative source.
