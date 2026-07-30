# NovaLeave Coverage Report

**Repository**: `full-stack-dev-johncastrosanabria/NovaLeave`  
**Branch**: `abraham-villalobos`  
**Baseline commit**: `c2a3282995e72c9ef44a574e2c9a4c58cbf3302f`  
**Audit date**: 2026-07-30  
**Canonical task source**: `specs/001-leave-management-mvp/tasks.md`  
**Generated task tree**: `tasks/EPIC-000-general-system/TASK-001.md` through `TASK-153.md`  
**Decision-record authority**: `docs/adr/`

## Methodology

Coverage means an active identifier or normative behavior is semantically planned by an explicit canonical task and, where the behavior is security-, authorization-, concurrency-, transaction-, audit-, validation-, or user-facing, by an explicit test task. Identifier mentions alone do not count as coverage.

Generated artifacts are checked only after canonical sources are corrected. `tasks.md` remains authoritative; `TASK-*.md`, `BACKLOG.md`, and this report are derived.

## Inventories

| Category | Definition | Active inventory |
|---|---|---:|
| Functional requirements | Active `FR-XXX` requirements in `spec.md`; `FR-008` and `FR-010` are retired/reserved and excluded | 23 |
| Business rules | Active `BR-XXX` rules in `spec.md`, including BR-036 through BR-038; retired BR-010 and BR-022 excluded | 36 |
| Standard acceptance criteria | Active `AC-XXX` criteria in `spec.md` | 47 |
| HR acceptance criteria | `AC-HR-XXX` criteria in `docs/use-cases.md` | 12 |
| RBFV criteria | `RBFV-XXX` criteria in `specs/002-role-based-frontend-views/spec.md` | 34 |
| Use cases | UC-01 through UC-22 in `docs/use-cases.md` and UC contracts | 22 |
| Security requirements | `SEC-001` through `SEC-009` | 9 |
| Authorization requirements | Active `AUTHZ-XXX`; `AUTHZ-004` is retired/revised and excluded | 18 |
| Validation requirements | Active `VAL-XXX`; `VAL-008` is not active | 8 |
| Audit requirements | `AUD-001` through `AUD-010` | 10 |
| Concurrency requirements | `CON-001` through `CON-012` | 12 |
| Frontend requirements | RBFV criteria plus frontend design sections | 34 RBFV criteria + design sections |
| Canonical tasks | `T001` through `T153` in `specs/001-leave-management-mvp/tasks.md` | 153 |
| Generated task files | `TASK-001.md` through `TASK-153.md` | 153 |
| Required tests | Canonical tasks whose task text explicitly adds tests covering unit, integration, E2E, security, accessibility, audit, concurrency, idempotency, and traceability | 52 |
| Explicit exclusions | MVP out-of-scope items from `spec.md`, `docs/use-cases.md`, `tasks.md`, and generated task out-of-scope sections | 28 |

## Coverage Results

| Category | Active total | Covered | Partial | Missing | Conflict | Coverage | Notes |
|---|---:|---:|---:|---:|---:|---:|---|
| Functional requirements | 23 | 23 | 0 | 0 | 0 | 100% | FR-023 is covered by T079/T080/T086/T088 after projected-balance remediation |
| Business rules | 36 | 36 | 0 | 0 | 0 | 100% | BR-036/BR-037 covered by T079/T086; BR-037/BR-038 covered by T080/T088 |
| Standard acceptance criteria | 47 | 47 | 0 | 0 | 0 | 100% | AC-012, AC-018, AC-020 covered by approval transaction, rejection, stale-operation, and projected-balance tests |
| HR acceptance criteria | 12 | 12 | 0 | 0 | 0 | 100% | AC-HR-003 maps to dedicated `/rrhh/calendario` with all organization-wide vacation requests |
| RBFV criteria | 34 | 34 | 0 | 0 | 0 | 100% | RBFV-003/RBFV-012/RBFV-032 resolved for capability and HR route behavior |
| Use cases | 22 | 22 | 0 | 0 | 0 | 100% | UC-19 route ambiguity resolved; UC-09 through UC-13 capability gate resolved |
| Security requirements | 9 | 9 | 0 | 0 | 0 | 100% | HR forbidden mutations and disabled-Approver access are planned by explicit tests |
| Authorization requirements | 18 | 18 | 0 | 0 | 0 | 100% | `canResolveRequests=true` is explicit for Approver queue/detail/resolution; HR `/calendario` denied |
| Validation requirements | 8 | 8 | 0 | 0 | 0 | 100% | Server-derived values, input modes, reasons, and stale operations remain covered |
| Audit requirements | 10 | 10 | 0 | 0 | 0 | 100% | State changes, HR sensitive access, and capability toggles covered |
| Concurrency requirements | 12 | 12 | 0 | 0 | 0 | 100% | BR-038 approval POST revalidation is explicitly tied to stale/concurrent projected-balance behavior |
| Frontend requirements | 34 | 34 | 0 | 0 | 0 | 100% | Route, navigation, read-only HR views, calendar accessibility, and projected-balance UI all covered |
| Canonical tasks | 153 | 153 | 0 | 0 | 0 | 100% structural | Task count unchanged |
| Generated task files | 153 | 153 | 0 | 0 | 0 | 100% structural | One-to-one `T001-T153` to `TASK-001-TASK-153` preserved |
| Required tests | 52 | 52 | 0 | 0 | 0 | 100% planned | Counted from canonical tasks that explicitly add tests; no source/tests exist yet |
| Explicit exclusions | 28 | 28 | 0 | 0 | 0 | 100% classified | No task introduces excluded APIs, email, hierarchy, HR request resolution, HR balance edits, or User Pending cancellation |

## Key Traceability Repairs

| Behavior | Canonical sources | Canonical tasks | Generated tasks |
|---|---|---|---|
| Approver capability required for queue/detail/resolution | Constitution v6.0.1, UC-09 through UC-13, UC contracts, RBFV policies | T017, T079, T080, T086, T088, T090 | TASK-017, TASK-079, TASK-080, TASK-086, TASK-088, TASK-090 |
| HR calendar is `/rrhh/calendario` only | FR-017, UC-19, AC-HR-003, RBFV-032, UC-19 contract | T118, T122, T124, T130, T134 | TASK-118, TASK-122, TASK-124, TASK-130, TASK-134 |
| HR calendar shows all organization-wide vacation requests | FR-017, UC-19, AC-HR-003, UC-19 contract | T124, T130, T134 | TASK-124, TASK-130, TASK-134 |
| Projected-balance display and query-time evaluation | FR-023, BR-036, BR-037, UC-10 | T079, T086 | TASK-079, TASK-086 |
| Approval POST projected-balance revalidation | BR-037, BR-038, UC-11, CON-004, CON-006 | T080, T088 | TASK-080, TASK-088 |
| Decision-record governance | Constitution §14, DR-001, DR-002 | T019, T099, T114 | TASK-019, TASK-099, TASK-114 |

## Remaining Partial Or Unresolved Items

None in the active planning package.

Implementation is not present, so this report does not claim executable code coverage or passing application tests.

## Validation Status

Validation commands and results are recorded in `docs/audits/NovaLeave_Post_Remediation_Audit.md`. This report was updated before the final validation pass and supersedes the stale 2026-07-29 coverage metadata.
