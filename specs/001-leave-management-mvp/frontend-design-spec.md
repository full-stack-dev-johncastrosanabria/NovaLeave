# Frontend Design Specification — NovaLeave MVP

**Related Feature**: `001-leave-management-mvp`
**Version**: 1.3.0
**Date**: 2026-07-23
**Status**: Draft for review

## 1. Purpose

This document defines the mandatory frontend design rules for NovaLeave MVP, including visual language, UX/UI standards, responsive behavior, accessibility, colors, shadows, transitions, and animations.
It complements `spec.md` and aligns with `.specify/memory/constitution.md` v6.0.0 under the MVC, Razor Views, Bootstrap, security, and accessibility baseline.

## 2. Scope

This specification applies to all user-facing screens for `User`, `Approver`, and `HR` (displayed as `RRHH`), including:

* Authentication views.
* Vacation request creation and editing.
* Request history and status views.
* Approver resolution views.
* HR read-only views (request list, request detail, organizational calendar, balances, balance movements, audit).
* Approver-capability management views.
* Balance summary views.
* Basic vacation calendar.
* Shared components such as navigation, buttons, forms, cards, badges, tables, alerts, dropdowns, and modals.

## 3. UX/UI Principles

1. **Clarity first**: prioritize status, actions, balance impact, and outcomes.
2. **Consistency**: the same component must preserve the same appearance and behavior.
3. **Server-authoritative UX**: no business result is final before server confirmation.
4. **Accessibility by default**: keyboard, contrast, screen readers, and focus are mandatory.
5. **Responsive by default**: every workflow must work on mobile, tablet, and desktop.
6. **Feedback and trust**: every action must provide loading, success, validation, conflict, or error feedback.
7. **Restrained motion**: animation must explain interaction or state change, never decorate.

## 4. Design Tokens

### 4.1 Color Palette

NovaLeave must use dark corporate blues, white surfaces, and cool neutral grays.
Gradients are prohibited in backgrounds, buttons, borders, text, cards, navigation, and loading indicators.

#### Primary Colors

| Token                 |       Hex | Use                                                    |
| --------------------- | --------: | ------------------------------------------------------ |
| `--nl-color-navy-900` | `#102A43` | Navigation, sidebar, main headers                      |
| `--nl-color-navy-800` | `#163A59` | Navigation hover and dark secondary surfaces           |
| `--nl-color-blue-700` | `#174A70` | Links, secondary actions, active icons                 |
| `--nl-color-blue-600` | `#1E5A85` | Primary buttons and selected controls                  |
| `--nl-color-blue-100` | `#E8F2F8` | Selected rows, active filters, information backgrounds |
| `--nl-color-blue-050` | `#F1F7FB` | Subtle information sections and calendar highlights    |

#### Neutral Colors

| Token                       |       Hex | Use                               |
| --------------------------- | --------: | --------------------------------- |
| `--nl-color-background`     | `#F6F8FB` | Main application background       |
| `--nl-color-surface`        | `#FFFFFF` | Cards, forms, tables, dialogs     |
| `--nl-color-border`         | `#D8E1E8` | Inputs, tables, component borders |
| `--nl-color-border-subtle`  | `#E9EEF2` | Internal separators               |
| `--nl-color-text-primary`   | `#17212B` | Titles, body text, important data |
| `--nl-color-text-secondary` | `#52606D` | Metadata and descriptions         |
| `--nl-color-text-muted`     | `#6B7785` | Helper text and timestamps        |
| `--nl-color-text-disabled`  | `#98A2AD` | Disabled controls                 |

#### Semantic State Colors

| State                                                                                          | Text/Icon | Background |    Border | Label                     |
| ---------------------------------------------------------------------------------------------- | --------: | ---------: | --------: | ------------------------- |
| `Pending`                                                                                      | `#875A00` |  `#FFF5D6` | `#E8C76A` | Pendiente                 |
| `Approved`                                                                                     | `#17653A` |  `#E8F5ED` | `#9BCBAD` | Aprobada                  |
| `Rejected`                                                                                     | `#9F2730` |  `#FCEBED` | `#E3A2A7` | Rechazada                 |
| `CancelledByTimeout`                                                                           | `#52606D` |  `#EEF2F5` | `#C7D0D8` | Cancelada automáticamente |
| `CancelledByApprover`                                                                          | `#35546B` |  `#EAF1F5` | `#AFC3D0` | Cancelada por aprobador   |
| Information                                                                                    | `#174A70` |  `#E8F2F8` | `#A8CBDF` | Información               |
| Warning                                                                                        | `#875A00` |  `#FFF5D6` | `#E8C76A` | Advertencia               |
| Error                                                                                          | `#9F2730` |  `#FCEBED` | `#E3A2A7` | Error                     |
| Status must always include readable text and may include an icon. Color alone is insufficient. |           |            |           |                           |

### 4.2 Typography

Font stack:

```css
font-family: "Inter", "Roboto", system-ui, -apple-system, "Segoe UI", sans-serif;
```

| Element    |   Size | Weight |
| ---------- | -----: | -----: |
| H1         | `32px` |  `700` |
| H2         | `24px` |  `600` |
| H3         | `20px` |  `600` |
| Body       | `16px` |  `400` |
| Small text | `14px` |  `400` |
| Labels     | `14px` |  `500` |
| Badges     | `12px` |  `600` |

### 4.3 Spacing, Radius, and Touch Targets

* Base spacing unit: `4px`.
* Allowed scale: `4, 8, 12, 16, 24, 32, 48, 64`.
* Small radius: `6px`.
* Medium radius: `8px`.
* Large radius: `12px`.
* Minimum interactive target: `44x44px`.
* Pill shapes are allowed only for badges, filters, and status labels.

### 4.4 Shadows and Elevation

Borders are the primary method for separating surfaces.

```css
.nl-card {
  border: 1px solid #E1E7EC;
  box-shadow: 0 1px 2px rgba(16, 42, 67, 0.08);
}
.nl-overlay {
  box-shadow: 0 16px 40px rgba(16, 42, 67, 0.16);
}
```

Colored glows, blue halos, decorative blur, frosted-glass effects, translucent floating panels, neon borders, and multiple decorative shadows are prohibited.

### 4.5 Motion Tokens

```css
:root {
  --nl-motion-fast: 120ms;
  --nl-motion-standard: 180ms;
  --nl-motion-deliberate: 240ms;
  --nl-ease-standard: cubic-bezier(0.2, 0, 0, 1);
  --nl-ease-enter: cubic-bezier(0, 0, 0.2, 1);
  --nl-ease-exit: cubic-bezier(0.4, 0, 1, 1);
}
```

## 5. Responsive Rules

### 5.1 Breakpoints

* `xs`: `0–575px`
* `sm`: `576–767px`
* `md`: `768–991px`
* `lg`: `992–1199px`
* `xl`: `1200px+`

### 5.2 Layout Behavior

* Use fluid layouts and mobile-first enhancement.
* Use a 12-column grid on tablet and desktop.
* Use a single-column priority layout on mobile.
* Navigation must collapse into an accessible mobile menu.
* Critical actions must remain visible and reachable.
* Normal workflows must not require horizontal page scrolling.
* Sticky elements must not cover content, errors, or focused controls.

### 5.3 Content Priority

Mobile views must show:

1. Status.
2. Dates.
3. Working-day total.
4. Balance impact.
5. Available actions.

## 6. Component Rules

### 6.1 Buttons

```css
.nl-button-primary {
  background-color: #1E5A85;
  color: #FFFFFF;
  border: 1px solid #1E5A85;
}
.nl-button-primary:hover {
  background-color: #174A70;
  border-color: #174A70;
}
.nl-button-secondary {
  background-color: #FFFFFF;
  color: #174A70;
  border: 1px solid #A8B8C5;
}
.nl-button-danger {
  background-color: #FFFFFF;
  color: #9F2730;
  border: 1px solid #C96970;
}
```

A solid red button may be used only in the final confirmation of a destructive action.
Buttons must not grow on hover. Active buttons may move downward by no more than `1px`. Loading must preserve width and prevent duplicate submission.

```css
.nl-button {
  transition:
    background-color var(--nl-motion-fast) var(--nl-ease-standard),
    border-color var(--nl-motion-fast) var(--nl-ease-standard),
    color var(--nl-motion-fast) var(--nl-ease-standard),
    box-shadow var(--nl-motion-fast) var(--nl-ease-standard);
}
.nl-button:active:not(:disabled) {
  transform: translateY(1px);
}
```

### 6.2 Forms and Inputs

* Every input must include a visible label.
* Validation must include explicit text.
* User-entered values must remain after server validation fails.
* Both request input modes must be clearly separated.
* Hidden inactive-mode values must not be submitted.
* Focus must move to the first error or validation summary when appropriate.

### 6.3 Cards, Tables, and Calendar

* Cards use white surfaces, subtle borders, and minimal shadows.
* Cards must not scale or float on hover.
* Table row hover may use `#F1F7FB`.
* Selected rows may use `#E8F2F8`.
* Rows must not move or scale.
* Calendar month changes may use a short crossfade.
* Calendar events must not enlarge on hover.
* Essential calendar information must not depend only on hover.

### 6.4 Alerts, Toasts, and Modals

* Alerts use semantic colors and plain language.
* Toasts may enter with opacity and no more than `8px` horizontal movement.
* Toast transition must not exceed `200ms`.
* No more than three toasts should be visible.
* Critical failures must not rely only on a toast.
* Modals may use opacity and vertical movement from `8px` to `0`.
* Modal duration must not exceed `240ms`.
* Scaling, bounce, and rotation are prohibited.
* Rejection and deactivation dialogs must explain status and balance consequences.

### 6.5 Loading States

Gradient shimmer is prohibited.

```css
@keyframes nl-skeleton-pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.55; }
}
.nl-skeleton {
  background-color: #E4EAF0;
  animation: nl-skeleton-pulse 1.2s ease-in-out infinite;
}
```

## 6.6 Global Header & Layout

The application uses a single shared `_Layout.cshtml` with a semantic `<header>` element that persists across all authenticated views.

### 6.6.1 Header Structure

| Zone | Content | Specification |
|------|---------|---------------|
| **Brand (left)** | NovaLeave logo/mark | SVG logo using `--nl-color-navy-900` / `--nl-color-blue-600`; link to context-appropriate root (`/mis-solicitudes`, `/aprobaciones`, `/rrhh`); accessible name `NovaLeave` |
| **Context Switcher (center-left)** | `Mis roles` dropdown | Visible only when identity holds ≥2 roles (User, Approver, HR); options: `Mi espacio`, `Aprobaciones`, `RRHH`; switches visible nav + route prefix only; **no session/claims modification** (§13) |
| **User Menu (right)** | Avatar/initials + dropdown | Button: `aria-haspopup="menu"`, `aria-expanded`, `aria-label="Menú de usuario"`; shows user email/name; dropdown `role="menu"` with keyboard navigation (Arrow keys, Esc, Enter/Space) |
| **User Menu Items** | 1. `Perfil` (future) — placeholder, disabled with `aria-disabled="true"`<br>2. Divider (`<hr role="separator">`)<br>3. `Cerrar sesión` — POST to `/Identity/Account/Logout` with antiforgery | `Cerrar sesión` is the only functional item in MVP; uses `<form method="post">` + button styled as menu item; confirms via hidden POST (no modal required); preserves `ReturnUrl` |

### 6.6.3 Responsive Header Behavior

| Breakpoint | Behavior |
|------------|----------|
| `xs`–`sm` (`<768px`) | Brand left; hamburger button (right) opens off-canvas drawer containing: Context Switcher (if applicable), User Menu |
| `md`–`xl` (`≥768px`) | Full horizontal header: Brand | Context Switcher | User Menu |

### 6.6.4 Reduced Motion

- Drawer slide: `240ms` ease; disabled under `prefers-reduced-motion` (instant)
- Dropdown fade: `120ms`; disabled under reduced motion

---

## 8. Motion and Interaction Effects

| Interaction                                                                                  |    Duration |
| -------------------------------------------------------------------------------------------- | ----------: |
| Hover and focus                                                                              |     `120ms` |
| Button and input state                                                                       |     `120ms` |
| Status transition                                                                            | `160–180ms` |
| Dropdown                                                                                     |     `160ms` |
| Card or row entry                                                                            |     `180ms` |
| Toast                                                                                        |     `200ms` |
| Modal                                                                                        |     `220ms` |
| Sidebar or panel                                                                             |     `240ms` |
| No frequent interaction may exceed `300ms`.                                                  |             |
| Initial content may use opacity from `0` to `1` and vertical movement of no more than `4px`. |             |

```css
@keyframes nl-card-enter {
  from {
    opacity: 0;
    transform: translateY(4px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
```

Animations must not replay on every scroll.
When a request changes state:

1. Disable conflicting actions.
2. Show processing feedback.
3. Wait for server confirmation.
4. Update the badge with a short crossfade.
5. Refresh balance information.
6. Show success or failure feedback.
   Confetti, celebration effects, animated counters, shaking fields, flashing messages, and permanent optimistic state changes are prohibited.

## 9. Reduced Motion and Accessibility

```css
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    scroll-behavior: auto !important;
    animation-duration: 1ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 1ms !important;
  }
}
```

Requirements:

* WCAG 2.1 AA contrast.
* Full keyboard accessibility.
* Visible focus for every interactive element.
* Associated labels for form controls.
* Text-based validation feedback.
* ARIA live regions for important updates.
* Minimum touch target of `44x44px`.
* Status must not depend only on color.
* Loading indicators must include accessible text.
* All workflows must remain understandable when motion is disabled.

## 10. Interaction and Content Rules

* Essential functionality must not depend on hover.
* Destructive actions require explicit confirmation.
* Status wording must match `spec.md`.
* PRG flows must preserve UX continuity.
* Validation failures must preserve entered values.
* Conflict messages must instruct the user to refresh.
* Loading states must prevent repeated submissions.
* Server confirmation is required before permanent visual updates.
* Default UI language is Spanish.
* Labels and messages must use plain, direct wording.
* Technical enum names must not be primary user-facing labels.
* Buttons should use precise action verbs.
* **No emojis may appear in any user-facing text, including navigation, buttons, cards, statuses, empty states, alerts, modals, login, Approver views, or HR views.** Approved icon libraries or Bootstrap-compatible SVG icons may be used when accessible. Icons must be marked `aria-hidden="true"` when nearby text already provides the accessible name.

## 11. Frontend Quality Gates

A frontend change is acceptable only if:

1. It uses the approved palette, typography, spacing, radii, and shadows.
2. It contains no gradients or prohibited effects.
3. It is responsive across all defined breakpoints.
4. It passes keyboard and focus visibility checks.
5. It provides explicit loading, validation, success, conflict, and error feedback.
6. It waits for server confirmation before permanent visual updates.
7. It respects reduced-motion preferences.
8. It preserves semantic status consistency.
9. Its animations have a functional purpose.
10. No animation or transition exceeds `300ms`.

## 12. Governance

* This document defines presentation and interaction rules for the MVP.
* Business behavior remains authoritative in `spec.md`.
* Architecture and engineering governance remain authoritative in `.specify/memory/constitution.md`.
* Security, accessibility, and correctness take precedence over visual preferences.
* New colors, effects, or animation patterns require explicit design review.

---

## 13. Login Screen (ASP.NET Core Identity)

The login screen must use the existing ASP.NET Core Identity flow with the following presentation requirements:

- **Branding**: NovaLeave logo/mark using design tokens (`--nl-color-navy-900`, `--nl-color-blue-600`).
- **Layout**: Centered card on `--nl-color-background` with white surface (`--nl-color-surface`), subtle border (`--nl-color-border`), and minimal shadow (`nl-card`).
- **Fields**: Email/username and password, each with visible `<label>` elements associated via `for`/`id`.
- **Validation**: Client-side (HTML5 + Bootstrap validation styles) and server-side validation summary; preserve entered email on failure.
- **Error feedback**: Authentication failures displayed in a styled alert (`--nl-color-error` tokens) without revealing account existence.
- **Accessibility**: Full keyboard navigation, visible focus rings, `autocomplete` attributes (`username`, `current-password`), `aria-describedby` linking errors to inputs.
- **Responsive**: Single-column centered card on mobile; constrained max-width on desktop.
- **Security**: Preserves antiforgery token, secure cookie settings, lockout, and session behavior configured in Identity.
- **No emojis, no gradients**.

### 13.1 Demo Account Switcher (Account Selector)

The login screen includes an **account switcher** (`Cuenta` dropdown) below the email field to select a pre-seeded demo identity before authenticating.

- **Seeded demo identities** (created via EF Core migration seed data, `NovaLeave:SeedDemoUsers=true` opt-in config flag, disabled in Production):
  - `user@demo` — `User` role, Active — password `Demo123!`
  - `approver@demo` — `Approver` role, Active — password `Demo123!`
  - `hr@demo` — `HR` role, Active — password `Demo123!`
  - `multi@demo` — `User`, `Approver`, `HR` roles, Active — password `Demo123!`
- **UI**: Dropdown/listbox labeled `Cuenta` below email field; options show display name + masked email (e.g., `Usuario Demo — u***@demo`)
- **Behavior**: Selecting an identity pre-fills the email field; **does not** authenticate automatically
- **Accessibility**: Full keyboard support, `aria-label="Seleccionar cuenta de demostración"`, `aria-describedby` linking to helper text "Seleccione una cuenta preconfigurada para acceso rápido"
- **No emojis, no gradients**; uses design tokens per §4
- **Security**: Demo accounts are indistinguishable from real accounts at login; no special treatment post-authentication

## 14. Context Selector — "Mis roles"

When an identity holds at least two authorized contexts (User, Approver, HR):

- **Visible label**: `Mis roles` (accessible name and visible concept).
- **Control type**: Accessible dropdown/button group (`<select>` or `<button aria-haspopup="listbox">` + listbox).
- **Options**: `Mi espacio`, `Aprobaciones`, `RRHH` (Spanish labels matching role contexts).
- **Behavior**: Switching context updates the visible navigation and route prefix only; **must not** modify identity, session, roles, claims, or permissions.
- **Display only when applicable**: Hidden when the identity has fewer than two authorized contexts.
- **Active context indication**: The selected context may be indicated inside the control (e.g., as the button label or selected option).
- **Keyboard accessible**: Full arrow-key navigation, `Esc` to close, focus management on open/close.
- **No emojis or icons as sole indicators**.

## 15. Navigation Labels

| Route / View | Updated Label |
|--------------|---------------|
| `/mis-solicitudes` (User history) | `Mi historial` (was `Mi saldo`) |
| `/aprobaciones` history tab | `Historial` (was `Historial y Desactivación`) |
| Context selector button | `Mis roles` (was `Contexto: Mi espacio`) |

**Notes**:
- `Mi historial` view **must retain** balance summary, balance movements, and request-related movements/history with clear dates, concepts, amounts, and resulting balance.
- `Historial` (Approver) retains approved pre-start deactivation functionality; deactivation remains available only from an eligible request or its detail, not from the navigation label.

## 16. Balance Card Terminology

The balance summary must display exactly these four concepts using the Spanish labels below, mapped to existing authoritative values:

| Display Label | Authoritative Source |
|---------------|----------------------|
| `Acumulado total` | Accrued days (global balance) |
| `Pendientes` | Days reserved by active `Pending` requests |
| `Días gozados` | Permanently deducted days (approved requests) |
| `Disponible` | Accrued − Reserved − Deducted (available balance) |

**Removed labels**: `Devengado`, `Reservado`, `Deducido` (when used as visible card titles for these four concepts).

The underlying balance formula and domain model remain unchanged; only the presentation labels are updated.

## 17. Projected Balance in Approver Views

In the Approver request list (`/aprobaciones`) and request detail (`/aprobaciones/{id}`):

- **Labels (Spanish)**:
  - `Disponible actual` — authoritative available balance **excluding the current request's reserved days** (i.e., accrued days − permanently deducted days − days reserved by other active Pending requests, not including the request being evaluated)
  - `Días solicitados` — authoritative working-day total of this request
  - `Disponible después de aprobar` — projected available balance = `Disponible actual` − `Días solicitados`
- **Calculation**: Server-side only using authoritative values (`authoritativeAvailableExcludingCurrentRequest - authoritativeRequestedDays`). Client preview must match server result but never replace server validation.
- **Warning state**: When projected balance is negative or invalid, display a visible warning (semantic warning colors per Section 4.1) and **disable the Approve action**.
- **Revalidation on POST**: When the approval POST is processed, the server must revalidate the projected balance; if negative, reject with a conflict outcome.

## 18. Request Input Modes

The create/edit request form must expose **only two input modes** via an accessible radio group or segmented control:

1. `Fecha inicio + Fecha fin` (date range)
2. `Fecha inicio + Cantidad de días` (start date + working days count)

**Requirements**:
- No `Modo A` / `Modo B` prefixes in user-facing labels.
- Only fields belonging to the selected mode are rendered and submitted.
- Both modes normalize server-side to one authoritative date range and one authoritative working-day total.
- Weekend exclusion (Mon–Fri only) applies in both modes; holidays counted as working days per MVP rule.

### 18.1 Excess Justification Field (Conditional)

When the server detects that the requested working-day total exceeds the User's available balance at submission time:

- **Additional field required**: `Justificación de exceso` (textarea, 10–500 characters, trimmed, whitespace-only rejected, markup treated as plain text).
- **Accessible label**: `Justificación por solicitar días que exceden su saldo disponible (10–500 caracteres)`.
- **Client hint**: Inline text "Este campo es obligatorio cuando los días solicitados superan su saldo disponible" displayed below the field conditionally.
- **Visual distinction**: Field rendered with a subtle warning accent (semantic warning token from Section 4.1) but no emojis.
- **Server behavior**: Request is created in `PendingEscalated` state; Approver cannot approve/reject directly — must escalate to HR.

## 19. Weekend Exclusion in Working-Day Calculation

- **Date-range mode**: Count Monday–Friday inclusively; exclude Saturdays and Sundays.
- **Start-date-plus-days mode**: Advance through calendar dates; count only Monday–Friday; produce authoritative end date server-side.
- **UI preview**: Must match server calculation but is informational only.
- **Boundary tests required** (add to test plan):
  - Friday + 1 working day → Monday
  - Range spanning one weekend
  - Weekend-only range (zero working days → rejected)
  - Start date on Saturday or Sunday (rejected per next-day minimum)
  - Holidays remain counted as working days (no holiday calendar in MVP)

## 20. Approver UI/UX Improvements

**List (`/aprobaciones`) and Detail (`/aprobaciones/{id}`)**:

- **Visual hierarchy**: Clear heading, requester name, date range, requested days, status badge, projected balance prominence.
- **Spacing**: Consistent 12px/16px/24px scale; white cards (`nl-card`) with subtle borders.
- **Status badges**: Semantic colors per Section 4.1; readable text; icons decorative with `aria-hidden="true"`.
- **Actions**: Primary (Approve) and secondary (Reject) buttons clearly distinguished; destructive Reject uses danger style only in final confirmation.
- **Confirmation dialogs**: Accessible modal (`role="dialog"`, `aria-modal="true"`, focus trap); explain status and balance consequences; Reject requires reason input (10–500 chars) before enabling confirm.
- **Loading states**: Button disables, spinner, width preserved; prevent duplicate submission.
- **Feedback**: Toast (≤200ms) for success; inline alert for validation/conflict/error; critical failures not toast-only.
- **Responsive**: Mobile card layout showing status, dates, days, balance impact, actions; table on ≥`md`.
- **Empty states**: "No hay solicitudes pendientes" / "Sin solicitudes elegibles" with illustration (SVG, `aria-hidden`).
- **Reduced motion**: Respects `prefers-reduced-motion` (Section 8).

### 20.1 Escalated Requests (`PendingEscalated` State)

When a request exceeds the User's available balance at submission:

- **List badge**: `Exceso de saldo` (semantic warning token, distinct from `Pendiente`).
- **Detail view**: Displays `Disponible actual` (available balance at submission), `Días solicitados` (total requested), `Exceso` (requested − available), and the User's `Justificación de exceso` with label `Justificación del solicitante`.
- **Actions**:
  - **Escalar a RRHH** (primary action): Opens accessible modal requiring escalation reason (10–500 chars) and explicit confirmation; on submit transitions to `EscalatedToHR`, locks Approver actions, records audit.
  - **Rechazar** (secondary): Standard rejection flow with reason (10–500 chars); releases any reserved days; transitions to `Rejected`.
  - **Approve**: Disabled/hidden for `PendingEscalated` — cannot approve directly.
- **Projected balance card**: Shows `Disponible actual`, `Días solicitados`, `Exceso`, and `Disponible después de aprobar (negativo)` = `Disponible actual` − `Días solicitados` (negative value with warning style).
- **No emojis** in badges or buttons.

### 20.2 Authorized Excess Requests (`PendingAuthorizedExcess` State)

After HR authorizes the excess days:

- **List badge**: `Exceso autorizado` (semantic info token).
- **Detail view**: Shows HR authorization record (authorizer, date, authorized excess count, authorization reason) with label `Autorización RRHH`.
- **Projected balance card**: `Disponible actual` (available at submission), `Días solicitados`, `Exceso autorizado`, `Disponible después de aprobar (negativo)` = negative value (warning style).
- **Actions**: Standard **Aprobar** and **Rechazar** enabled; Approve deducts total days creating negative balance with `NegativeBalanceCarryForward`; Reject transitions to `Rejected` and releases reservation.
- **Mutually exclusive**: Approve/Reject mutually exclusive per §23.

## 21. HR UI/UX Improvements

**Applies to**: Request list, Request detail, Organizational calendar, Balances list, Balance movements, Audit log, Approver capability list, **Escalated request resolution**.

- **Visual standards**: Same tokens, spacing, cards, tables, shadows as rest of app.
- **Read-only indicators**: Visible badge/label `Solo lectura` on all HR views; no resolution actions (Approve/Reject/Deactivate) rendered or actionable **except** for `EscalatedToHR` requests (see below).
- **Tables**: Server-side pagination, sortable columns, filter toolbar; responsive card fallback on mobile.
- **Calendar**: Month view with approved periods; requester names visible (HR-authorized); keyboard-navigable events.
- **Balances/Movements**: Summary cards (`Acumulado total`, `Pendientes`, `Días gozados`, `Disponible`); movement timeline with date, concept, amount, resulting balance; **when `NegativeBalanceCarryForward > 0`, show recovery plan card with months to zero**.
- **Capability management**: List shows Approver identity, `canResolveRequests` toggle; toggle requires reason (10–500 chars), confirmation modal, row version (optimistic concurrency), success/error/conflict toasts.
- **States**: Empty, loading, forbidden (403), conflict (stale version), error — all with accessible messaging.
- **Escalated request resolution** (`/rrhh/solicitudes/{id}` for `EscalatedToHR` state):
  - **Detail view**: Mirrors Approver detail with `Exceso de saldo` badge, `Disponible actual`, `Días solicitados`, `Exceso`, `Justificación del solicitante`, and `Autorización RRHH` section (empty pending resolution).
  - **Actions**: Two primary actions — **Autorizar exceso** and **Rechazar exceso**; both require reason (10–500 chars) and explicit confirmation modal; **Approve/Reject/Deactivate not shown**.
  - **Autorizar exceso**: On submit, transitions to `PendingAuthorizedExcess`, records authorized excess count and HR reason, unlocks Approver actions, creates authorization audit event.
  - **Rechazar exceso**: On submit, transitions to `Pending` with `Días solicitados` reduced to available balance at submission time, `Exceso` set to zero, notifies Approver.
  - **Negative balance carry-forward visibility**: In balance list and movements, shows `NegativeBalanceCarryForward` as a distinct concept with recovery timeline.

## 22. Calendar-to-Detail Navigation

- **Calendar events** (User, Approver, HR views) must be keyboard-activatable (`Enter`/`Space`) and have an accessible name (e.g., `Solicitud de {requester}, del {start} al {end}, {días} días`).
- **Navigation target** by role:
  - User owner → `/mis-solicitudes/{id}` (own detail)
  - Eligible Approver → `/aprobaciones/{id}` (approver detail)
  - HR → `/rrhh/solicitudes/{id}` (read-only HR detail)
- **Authorization**: Navigation link/button only rendered when actor is authorized for target view; if calendar is anonymized for a role, no detail link is added.
- **No emojis** in event tooltips or labels.

## 23. Mutually Exclusive Approve/Reject Actions

- **UI level**: After one action (Approve or Reject) is submitted, **disable all conflicting resolution buttons**; show a single processing state; prevent duplicate submission; require explicit confirmation; Reject requires valid reason before enable.
- **Server level**: POST with antiforgery; revalidate role, active status, ownership, eligibility, state, balance, overlap, row version; apply optimistic concurrency; allow exactly one valid transition; reject stale/duplicate competing transitions without side effects; return clear conflict result.
- **JavaScript disabling is UX only** — never treated as authorization or concurrency control.

## 24. Create-to-Queue Presentation Feedback

- **User creation success**: After a valid request is created, the redirected User view (`/mis-solicitudes` or `/saldo`) displays a success toast/alert confirming the request was submitted.
- **Approver queue visibility**: When any Approver subsequently opens or refreshes `/aprobaciones`, the newly created eligible `Pending` request appears exactly once without requiring manual synchronization, cache clearing, or application restart. No client-side polling or push is mandated; the requirement is that standard server-rendered navigation reflects the committed state.

---

## 25. Vacation Calendar — Traditional Month View (User, Approver, HR)

The calendar is a **traditional month-view grid** with weekday headers and day cells, adhering to the MVP rule: **weekends excluded** (non-working days are visually distinguished and not counted as working days).

### 24.1 Structure & Layout

| Element | Specification |
|---------|---------------|
| **Grid** | 7 columns (Mon–Sun), 4–6 rows per month |
| **Weekday header row** | Fixed at top; labels: `Lun`, `Mar`, `Mié`, `Jue`, `Vie`, `Sáb`, `Dom` (Spanish abbreviations, `<abbr>` with full name) |
| **Day cells** | Square-ish aspect ratio on desktop (≥`96px`), full-width on mobile (`xs`); minimum `44x44px` touch target |
| **Today highlight** | Subtle ring (`--nl-color-blue-600`, `2px` outline) without changing background |
| **Weekend columns** | `Sáb`/`Dom` visually muted: background `#F5F6F8`, text `--nl-color-text-muted`; **no events rendered** (weekends carry no approved periods per MVP) |
| **Month navigation** | Header with month/year label, `Previous`/`Next` buttons (icon + text), `Today` button returns to current month |
| **Year jump** | Optional dropdown on `lg`+`xl` for direct year selection |

### 24.2 Event Rendering (Approved Periods Only)

| Aspect | Rule |
|--------|------|
| **Source** | Only `Approved` requests (authoritative server data) |
| **Visual** | Horizontal bar spanning inclusive start–end dates within the month |
| **Color** | `--nl-color-blue-600` background, white text; border `--nl-color-blue-700` |
| **Label** | Requester name (User/Approver: own name; HR: requester name), working-day count (e.g., `5 días`) |
| **Overlap** | Multiple events in same day → stack vertically with `2px` gap; max 3 visible, `+N más` indicator for overflow |
| **Tooltip** | On hover/focus: full date range, working days, requester, status badge `Aprobada` |
| **Click/Enter/Space** | Navigates to authorized detail view per role (§21) |

### 24.3 Weekend Exclusion — Visual & Behavioral

| Requirement | Implementation |
|-------------|----------------|
| **Non-working days** | Saturday/Sunday columns rendered but **never receive event bars** |
| **Range spanning weekend** | Event bar splits: `Fri` segment ends Fri, `Mon` segment starts Mon (visual gap over weekend) |
| **Zero-working-day ranges** | Ranges falling entirely on weekend → **rejected at creation** (server validation per §18); calendar never renders them |
| **Keyboard navigation** | Arrow keys skip weekend cells (Left on Fri → Mon same week; Right on Mon → Fri prev week) or jump to next/prev weekday — **configurable per §24.5** |

### 24.4 Responsive Behavior

| Breakpoint | Behavior |
|------------|----------|
| `xs` (`<576px`) | Horizontal scroll on month grid; weekday headers sticky left; day cells minimum `44px` width; events as colored dots with count badge, tap → bottom sheet detail |
| `sm`–`md` | Full grid; cells `72px` min; event labels truncated with ellipsis |
| `lg`–`xl` | Full grid; cells `96px` min; full event labels; year dropdown visible |

### 24.5 Keyboard Navigation (WCAG 2.1 AA)

| Key | Action |
|-----|--------|
| `ArrowLeft` / `ArrowRight` | Move focus to previous/next **weekday** (skip Sat/Sun) |
| `ArrowUp` / `ArrowDown` | Move focus to same weekday previous/next week |
| `Home` / `End` | First/last weekday of current month |
| `PageUp` / `PageDown` | Previous/next month (focus preserved on same weekday) |
| `Enter` / `Space` | Activate focused event → navigate to detail (§21) |
| `Escape` | Close any open tooltip/sheet; return focus to grid |
| `Tab` / `Shift+Tab` | Move between month nav, `Today` button, grid, events |
| **Screen reader** | Each event: `role="button"` + `aria-label="Solicitud de {requester}, del {start} al {end}, {días} días hábiles, Aprobada"` |

### 24.6 Reduced Motion

- Month transition: crossfade `120ms` (disabled under `prefers-reduced-motion`)
- Event hover/focus: no scale/transform; only outline/color change
- Tooltip: fade `80ms` (disabled under reduced motion)

### 24.7 Empty & Loading States

| State | UI |
|-------|-----|
| **Loading** | Skeleton grid (`nl-skeleton` per day cell) |
| **No approved periods** | Centered illustration (SVG, `aria-hidden`), text "No hay periodos aprobados en este mes" |
| **Error** | Inline alert (semantic error colors), `Reintentar` button |

### 24.8 Shared Calendar Partial

All three roles reuse a single Razor partial `_Calendar.cshtml` (model: `CalendarViewModel` with `Month`, `Events[]`, `CurrentUserRole`, `CanNavigateToDetail`):

```csharp
// CalendarViewModel (shared)
public record CalendarViewModel(
    YearMonth CurrentMonth,
    IReadOnlyList<CalendarEvent> Events,
    RoleContext CurrentRole, // User | Approver | HR
    bool CanNavigateToDetail
);
```

Role-specific controllers (`CalendarioController`, `RRHHController`) populate the view model and pass the correct detail route template.

---
