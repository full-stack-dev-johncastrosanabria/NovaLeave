# Frontend Design Specification — NovaLeave MVP

**Related Feature**: `001-leave-management-mvp`
**Version**: 1.9.1
**Date**: 2026-08-05
**Status**: Ready for Planning. Design tokens, components, accessibility baseline, and responsive rules approved.

## 1. Purpose

This document defines the mandatory frontend design rules for NovaLeave MVP, including visual language, UX/UI standards, responsive behavior, accessibility, colors, shadows, transitions, and animations.
It complements `spec.md` and aligns with `.specify/memory/constitution.md` v7.0.0 under the MVC, Razor Views, Bootstrap, security, and accessibility baseline.

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
* **Every visual change MUST update this specification in the same change set.** The update must identify affected routes, visual hierarchy, component behavior, responsive behavior, accessibility considerations, and testable acceptance criteria. A visual implementation without its corresponding specification update is incomplete.

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

### 13.1 Demo User List

When demo seeding is enabled outside Production, the login screen displays a
compact list titled `Usuarios disponibles` containing only these identifiers:

- `user@demo`
- `approver@demo`
- `hr@demo`
- `multi@demo`

The list must not display passwords, roles, active status, context descriptions,
credential instructions, or technical seeding information. It is informational
only and does not authenticate or prefill credentials automatically. Each user
identifier remains selectable as text and readable by assistive technology.

In Production, the entire demo-user list remains hidden. The standard email and
password fields, validation, lockout, and authentication behavior are unchanged.

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

### 16.1 Balance Visual Hierarchy

The four concepts must not receive equal visual weight because `Acumulado total`
is a historical figure and can be confused with usable balance. The presentation
must establish the following hierarchy without changing the authoritative values:

1. **`Disponible` is the primary figure.** It appears first, uses the largest
   numeric treatment, and includes plain-language guidance that it is the saldo
   that can actually be used for new requests.
2. **`Acumulado total`, `Pendientes`, and `Días gozados` are explanatory
   components.** They use smaller, independent cards or clearly separated
   regions and must not visually compete with `Disponible`.
3. The formula `Disponible = Acumulado total − Pendientes − Días gozados` may
   be displayed as supporting context, but it must remain compact and must not
   become the dominant page heading.

#### `/saldo` — Mi historial

- Use one spacious primary balance card containing `Disponible`, its value, a
  short explanation, and the `Crear solicitud` action.
- On desktop, the primary card may include a separate compact calculation panel.
- Place the three component figures below the primary card as independent cards
  with at least `16px` between them; each includes an icon, value, and one short
  explanatory sentence.
- Separate the balance summary from `Historial de movimientos` by at least
  `24px`; the movement table remains a distinct surface.
- On viewports below `md`, the primary card, formula, and component cards stack
  vertically without horizontal overflow or loss of labels.

#### `/mis-solicitudes/crear` — Balance context

- Present `Disponible` as the primary figure and keep the projected value inside
  the same visual context so the before/after relationship is immediate.
- Display the three component values as supporting information with the same
  approved terminology.
- The projected balance remains informational; server validation remains
  authoritative.

#### Acceptance criteria

- A user can identify the usable balance without reading the component cards.
- `Acumulado total` is explicitly described as historical and is never presented
  as the amount available to request.
- All four approved labels remain visible in the summary.
- The project stylesheet URL is content-versioned so updated markup cannot be
  rendered with stale balance styles from the browser cache.

### 16.2 User Request Detail Visual Hierarchy

The owned-request detail at `/mis-solicitudes/{id}` must let the user identify
the request state, requested period, and available action before reading the
supporting information.

- Start with a spacious summary surface containing the visible page context,
  shared textual status badge, date range, a short status explanation, and the
  conditional `Editar solicitud` action when the authoritative status is
  `Pending`.
- Present `Período solicitado`, `Días hábiles`, and `Fecha de solicitud` as
  three compact facts with icons, labels, and values. Icons are decorative and
  labels remain visible, so meaning never depends on iconography or colour.
- Place `Motivo de la solicitud` in its own readable surface and preserve line
  breaks without exposing technical identifiers or concurrency data.
- Place `Seguimiento` in a secondary surface with current status and last update.
  When a rejection reason exists, show it in a distinct semantic error region
  titled `Motivo del rechazo`.
- The screen must render only values already projected by the authoritative
  request-detail query. It must not calculate or imply balance impact in Razor.
- On viewports below `md`, the summary content, actions, fact cards, and lower
  content stack vertically without horizontal overflow. Primary and secondary
  actions become full-width where needed.
- Use semantic headings, a definition list for labeled facts, the shared status
  badge, and visible focus styles. Status meaning must be conveyed by icon and
  text, never colour alone.

#### Acceptance criteria

- A user can identify the status, requested period, and requested working days
  from the first visual group.
- `Editar solicitud` is visible only for an owned `Pending` request.
- The request reason and any rejection reason have explicit descriptive titles.
- Creation and update dates use a consistent user-facing date format.
- The page remains readable at `320px` without horizontal scrolling.

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

### 18.1 Insufficient Balance Feedback

When the selected period requires more working days than the user's current
`Disponible`, the create-request screen must replace the generic validation
banner for this condition with a focused modal titled `No tienes saldo
suficiente`.

- The modal explains in Spanish that the selected period exceeds the usable
  balance and preserves every value entered in the form.
- It displays `Disponible`, `Días solicitados`, and `Días faltantes` as labeled
  values. These figures clarify the rejection but never replace authoritative
  server validation.
- The primary action is `Ajustar período`: it closes the modal and moves focus
  to the editable date or day-count field for the selected input mode.
- A secondary `Cerrar` action closes the modal without clearing the form.
- Client-side detection may open the modal before submission as immediate UX
  feedback. The server must still enforce the invariant and cause the same modal
  to open when it rejects a request for insufficient balance.
- The dialog uses an accessible name and description, traps focus while modal,
  closes with `Escape`, returns focus to the correction target, and does not
  depend on colour or animation to communicate the problem.
- At widths down to `320px`, the comparison values and actions stack without
  horizontal overflow.

#### Acceptance criteria

- A user who exceeds the available balance sees no raw English domain message.
- The user can identify the available, requested, and missing days within the
  modal without consulting the balance cards behind it.
- Closing the dialog preserves dates, day count, input mode, and reason.
- `Ajustar período` places keyboard focus on the field needed to reduce the
  request.
- Submitting the same invalid payload without client scripting is rejected by
  the server and returns the modal in its open-on-load state.

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

## 20. Request List and Approver UI/UX Improvements

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

### 20.1 Approver Queue — `/aprobaciones`

- Start with a descriptive page-intro surface containing the `Aprobaciones`
  context, the purpose of the queue, and an explicit `Ver historial` action.
- Show three compact summary values before the table: requests pending review,
  total working days represented, and requests whose projected balance would be
  insufficient.
- Place the results in a titled surface named `Solicitudes por revisar`, with a
  visible result count and an explicit `Revisar solicitud` action per row.
- Identify each requester by their user-facing display name in the queue and in
  accessible action labels. Internal identifiers such as UUIDs must remain
  hidden from approvers unless no directory entry exists as a defensive fallback.
- Combine start and end dates under `Período solicitado`; use the user-facing
  `dd/MM/yyyy` format consistently.
- Present balance impact as an ordered flow: `Disponible actual` → `Después de
  aprobar`. `Disponible actual` must exclude the current request reservation;
  the projected value must subtract `Días solicitados`. These values must never
  be swapped or relabeled for visual convenience.

### 20.2 Approver Detail — `/aprobaciones/{id}`

- Use a summary-first hero containing requester, textual status badge, period,
  and requested working days.
- Present the server-authoritative balance impact in one dedicated three-step
  surface: `Disponible actual`, `Días solicitados`, and `Después de aprobar`.
- Place overlap or insufficient-balance warnings immediately below the impact
  they qualify, with icon and text rather than colour alone.
- Name the action surface `Decisión sobre la solicitud`. Approval and rejection
  must be separate options with short consequence explanations; the rejection
  reason remains visibly labeled and states that it is shared with the requester.
- Disable approval visually when the authoritative projected balance is
  negative; server revalidation remains mandatory.

### 20.3 User Request List — `/mis-solicitudes`

- Start with a `Mi espacio` page-intro surface explaining the list and containing
  the primary `Nueva solicitud` action.
- Show `Total de solicitudes`, `En revisión`, and `Aprobadas` as compact summary
  values that support, rather than replace, the request list.
- Place requests in a titled `Historial de solicitudes` surface with a visible
  count, combined `Período solicitado`, working days, shared textual status
  badge, and explicit `Ver detalle` action.

### 20.4 Responsive and accessibility acceptance

- At widths below `md`, each request table row becomes a labeled card; no value
  may depend on remembering a hidden desktop column heading.
- The three summary values and balance-impact steps stack without horizontal
  overflow at `320px`.
- Page regions use semantic headings; decorative icons use `aria-hidden`; action
  labels describe their destination; status and balance warnings do not depend
  on colour alone.
- A user or approver can identify the period, days, status, and next action from
  one visual group without scanning unrelated page regions.

## 21. HR UI/UX Improvements

**Applies to**: Request list, Request detail, Organizational calendar, Balances list, Balance movements, Audit log, Approver capability list.

- **Visual standards**: Same tokens, spacing, cards, tables, shadows as rest of app.
- **Authorization clarity**: Do not render badges or repeated labels stating `Solo lectura`. HR restrictions remain enforced by authorization and by the absence of unapproved resolution or balance-editing actions.
- **Tables**: Server-side pagination, sortable columns, filter toolbar; responsive card fallback on mobile.
- **Calendar**: Month view with approved periods; requester names visible (HR-authorized); keyboard-navigable events.
- **Balances/Movements**: Summary cards (`Acumulado total`, `Pendientes`, `Días gozados`, `Disponible`); movement timeline with date, concept, amount, resulting balance.
- **Capability management**: List shows Approver identity, `canResolveRequests` toggle; toggle requires reason (10–500 chars), confirmation modal, row version (optimistic concurrency), success/error/conflict toasts.
- **States**: Empty, loading, forbidden (403), conflict (stale version), error — all with accessible messaging.

### 21.1 HR Information Hierarchy and Plain Language

- Every HR read view starts with a compact context panel that uses a descriptive
  subject label and one sentence explaining the data shown. It must not repeat
  `Solo lectura` or displace primary data below the initial viewport on standard
  desktop sizes.
- The HR landing page title is `Resumen de vacaciones`. Its main sections use
  task-oriented titles: `Solicitudes que requieren atención`, `Estado de las
  solicitudes`, `Últimos cambios registrados`, and `Consultas frecuentes`.
- Summary indicators use descriptive labels: `Solicitudes por resolver`,
  `Solicitudes registradas`, `Personas con saldo`, and `Saldo disponible total`.
- The HR sidebar starts with **`Panel RRHH`**, linking to `/rrhh`, followed by
  `Solicitudes`, `Calendario`, `Saldos`, `Auditoría`, and `Aprobadores`.
- `Panel RRHH` is marked as the current navigation item only on the exact
  `/rrhh` route; child routes mark their own corresponding item instead.
- Data tables are placed inside a titled surface and show total record count,
  descriptive empty state, explicit column labels, and action labels such as
  `Ver detalle` or `Ver historial` instead of generic `Ver`.
- Request status uses the shared textual status badge; raw enum values must not
  be the primary presentation.
- HR balance list and movement detail follow the hierarchy in §16.1:
  `Disponible` is primary, and the other three concepts explain its composition.
- Approver-capability screens explain the business effect as “puede resolver
  solicitudes”. Technical concurrency tokens such as `RowVersion` remain in the
  submitted form when required but are not displayed as user-facing content.
- Capability changes remain distinct from read-only HR views and must clearly
  state that the Approver role itself is not assigned or removed.

#### Acceptance criteria

- An HR user can state the purpose of each list/detail view from its introductory
  panel without seeing repeated authorization disclaimers.
- No HR view displays the phrase `Solo lectura`.
- An HR user can return to the organizational summary from every HR route using
  the first sidebar option, `Panel RRHH`.
- No HR balance view uses `Reservado` or `Deducido` as visible summary titles.
- Empty HR lists explain what data is absent instead of displaying a blank table.
- Internal field names and concurrency tokens are not visible as primary labels.

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
| `Enter` / `Space` | Activate focused event → navigate to detail (§22) |
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
