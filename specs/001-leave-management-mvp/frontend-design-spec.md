# Frontend Design Specification — NovaLeave MVP

**Related Feature**: `001-leave-management-mvp`
**Version**: 1.1.0
**Date**: 2026-07-22
**Status**: Draft for review

## 1. Purpose

This document defines the mandatory frontend design rules for NovaLeave MVP, including visual language, UX/UI standards, responsive behavior, accessibility, colors, shadows, transitions, and animations.
It complements `spec.md` and aligns with `.specify/memory/constitution.md` under the MVC, Razor Views, Bootstrap, security, and accessibility baseline.

## 2. Scope

This specification applies to all user-facing screens for `User` and `Approver`, including:

* Authentication views.
* Vacation request creation and editing.
* Request history and status views.
* Approver resolution views.
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

## 7. Motion and Interaction Effects

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

## 8. Reduced Motion and Accessibility

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

## 9. Interaction and Content Rules

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

## 10. Frontend Quality Gates

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

## 11. Governance

* This document defines presentation and interaction rules for the MVP.
* Business behavior remains authoritative in `spec.md`.
* Architecture and engineering governance remain authoritative in `.specify/memory/constitution.md`.
* Security, accessibility, and correctness take precedence over visual preferences.
* New colors, effects, or animation patterns require explicit design review.
