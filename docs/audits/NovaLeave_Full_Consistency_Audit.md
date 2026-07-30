# NovaLeave Full Consistency Audit

## 1. Audit metadata

| Field | Value |
|---|---|
| Report title | NovaLeave Full Consistency Audit |
| Repository | `full-stack-dev-johncastrosanabria/NovaLeave` |
| Branch analyzed | `abraham-villalobos` |
| Commit SHA analyzed | `c2a3282995e72c9ef44a574e2c9a4c58cbf3302f` |
| Audit date | 2026-07-29 |
| Auditor role | Enterprise Solution Architect, Senior Business Analyst, Requirements Engineer, UML Reviewer, QA Lead, Software Engineering Auditor |
| Audit scope | Current governance, specifications, use cases, contracts, plans, diagrams, task artifacts, generated task files, coverage reports, validation scripts, and active documentation. Source implementation was not present. |
| Files created or modified | `docs/audits/NovaLeave_Full_Consistency_Audit.md` only |

## 2. Executive verdict

`NOT READY`

The planning package is close to implementation-ready, but it is not fully consistent. The most important blockers to readiness are not missing source code, because implementation is intentionally not started; they are semantic documentation defects that can drive incorrect implementation: the role-based frontend authorization policies omit `canResolveRequests` where use cases and contracts require it; the HR calendar route is ambiguous between shared `/calendario` and HR-specific `/rrhh/calendario`; and the generated coverage report is stale for active business rules `BR-036` through `BR-038`. No open question blocks the product model itself, but documentary corrections are required before implementation can proceed without assumptions.

## 3. Scope and sources reviewed

### Files actually reviewed

| Path | Status | Purpose / authority notes |
|---|---|---|
| `.specify/memory/constitution.md` | Active, normative | Binding constitution v6.0.0; top authority. |
| `AGENTS.md` | Active, informational/instructional | Points to `specs/001-leave-management-mvp/plan.md`. |
| `specs/001-leave-management-mvp/spec.md` | Active, normative | Primary MVP business specification. |
| `specs/001-leave-management-mvp/frontend-design-spec.md` | Active, normative for UI design | Frontend tokens, accessibility, labels, motion, layout. |
| `specs/001-leave-management-mvp/plan.md` | Active, planning authority | Architecture and implementation plan. |
| `specs/001-leave-management-mvp/research.md` | Active, decision support | Conflict resolutions CR-01 through CR-16. |
| `specs/001-leave-management-mvp/data-model.md` | Active, design authority | Domain, persistence, concurrency, lifecycle model. |
| `specs/001-leave-management-mvp/quickstart.md` | Active, informational/planning | Existing vs planned setup guidance. |
| `specs/001-leave-management-mvp/tasks.md` | Active, canonical task source | Canonical `T001` through `T153`. |
| `specs/001-leave-management-mvp/contracts/uc-contracts.md` | Active, normative contract | UC-01 through UC-22 route/application contracts. |
| `specs/001-leave-management-mvp/checklists/requirements.md` | Active, quality checklist | Spec quality evidence. |
| `specs/001-leave-management-mvp/diagrams/clean-architecture.md` | Active, derived diagram | Architecture diagram. |
| `specs/001-leave-management-mvp/diagrams/core-data-relationships.md` | Active, derived diagram | Data relationship diagram. |
| `specs/001-leave-management-mvp/diagrams/request-lifecycle.md` | Active, derived diagram | State machine. |
| `specs/002-role-based-frontend-views/spec.md` | Active, complementary normative spec | Route-to-role mapping, view inventory, frontend authorization. |
| `docs/use-cases.md` | Active, normative use cases | UC-01 through UC-22 plus AC-HR-001 through AC-HR-012. |
| `docs/decisions/README.md` | Active, decision index | Indexes accepted DR-001 and DR-002. |
| `docs/decisions/DR-001-runtime-configuration-values.md` | Active, approved decision | Runtime configuration values. |
| `docs/decisions/DR-002-generated-artifact-review-rule.md` | Active, approved decision | Generated task artifact validation rule. |
| `docs/audits/NovaLeave_Quick_Audit.md` | Historical/informational | Prior audit, not normative. |
| `docs/audits/NovaLeave_Planning_Remediation_Audit.md` | Historical/informational | Prior audit, not normative. |
| `tasks/BACKLOG.md` | Active, generated/derived | Backlog view derived from canonical `tasks.md`. |
| `tasks/coverage-report.md` | Active, generated/derived | Coverage report; not accepted as primary evidence. |
| `tasks/EPIC-000-general-system/EPIC-000.md` | Active, generated/derived | Single physical epic index. |
| `tasks/EPIC-000-general-system/TASK-001.md` through `TASK-153.md` | Active, generated/derived | Execution views of canonical tasks. |
| `tools/validate_tasks_tree.py` | Active validation script | Structural validator; could not execute because Python is unavailable. |

### Directories reviewed

| Directory | Result |
|---|---|
| `specs/001-leave-management-mvp/` | Reviewed for requirements, design, plan, tasks, contracts, diagrams, checklist. |
| `specs/002-role-based-frontend-views/` | Reviewed for complementary frontend-role requirements. |
| `docs/decisions/` | Reviewed as approved decision records, despite constitution path issue. |
| `docs/audits/` | Reviewed as historical/informational audit context. |
| `docs/archive/` | Excluded as archived; checked only for status context. |
| `tasks/EPIC-000-general-system/` | Reviewed structurally and by representative semantic samples. |
| `tools/` | Reviewed validation script. |

### Files excluded

| Path | Reason |
|---|---|
| `docs/archive/*` | Archived content is non-normative unless detecting stale divergence. |
| Generic templates not present in active paths | Not active project authority. |
| Source code and tests | No `src/`, `tests/`, `.sln`, `.csproj`, or `.cs` implementation artifacts are present per `plan.md` repository assessment and file inventory. |

### Validation tools executed

| Command | Result |
|---|---|
| `rg --files` | PASS; inventory produced active files and 153 `TASK-*.md` files. |
| `rg -n ...` identifier extraction | PASS; independently extracted active identifiers and evidence lines. |
| `Get-ChildItem tasks\EPIC-000-general-system\TASK-*.md` count | PASS; 153 generated task files found. |
| `rg` template artifact scan | PARTIAL; no unexpected template artifacts found outside allowed DR-002 citation and coverage-report citation. |
| `git branch --show-current` | PASS; branch `abraham-villalobos`. |
| `git rev-parse HEAD` | PASS after escalation; SHA `c2a3282995e72c9ef44a574e2c9a4c58cbf3302f`. |

### Validation tools that could not be executed

| Tool / command | Result |
|---|---|
| `bash .specify/scripts/bash/check-prerequisites.sh --json --require-tasks --include-tasks` | FAILED; `/bin/bash` unavailable in this Windows environment. |
| `python tools\validate_tasks_tree.py` | FAILED; `python` resolves to Microsoft Store alias and is not installed. |
| `py -3 tools\validate_tasks_tree.py` | FAILED; `py` launcher is not installed. |

## 4. Authority hierarchy used

The repository defines an authority order in `.specify/memory/constitution.md` §15.1:

1. Current constitution.
2. Approved feature specifications.
3. Approved frontend design specification for visual and interaction rules.
4. Approved ADRs and plans.
5. Executable code and tests.
6. OKF, Mermaid, and Graphify as derived artifacts.

This audit used that repository-defined hierarchy first. Where the user request supplied more detailed task-artifact ordering, it was applied under the constitution for generated artifacts:

1. Constitution and accepted decisions.
2. Active specifications.
3. Active use cases and contracts.
4. Business rules and acceptance criteria.
5. Plan, research, data model, frontend design, diagrams.
6. Canonical `specs/001-leave-management-mvp/tasks.md`.
7. Generated `tasks/EPIC-*/TASK-*.md`.
8. Backlog, coverage reports, previous audits.

Exception recorded: `.specify/memory/constitution.md` §14 requires relevant architectural decisions under `docs/adr/`, while the repository’s accepted decision records are under `docs/decisions/`. The user request explicitly included `docs/decisions/`, and `docs/decisions/README.md` marks DR-001 and DR-002 as Accepted, so they were treated as approved decisions while the path mismatch is reported as finding F-LOW-001.

## 5. Canonical system summary

### Verified facts

| Area | Canonical summary | Evidence |
|---|---|---|
| MVP scope | Server-rendered ASP.NET Core MVC vacation request system for single leave type `Vacation`, roles `User`, `Approver`, `HR`, global balance, Pending reservations, timeout cancellation, monthly accrual, pre-start deactivation, HR read-only views and approver capability management. | `.specify/memory/constitution.md` §§3.2, 4, 5; `specs/001-leave-management-mvp/spec.md` lines 5-9; `plan.md` Summary. |
| Out of scope | `Personal Leave`, `Medical Leave`, half-day/hourly requests, attachments, holiday calendars, per-user time zones, teams, hierarchy, delegation, escalation, SSO, automatic password recovery, integrations, User Pending cancellation, partial cancellation, approved-date editing. | `specs/001-leave-management-mvp/spec.md` Explicitly Out of MVP Scope; `docs/use-cases.md` §6. |
| Actors | `User`, `Approver`, `HR`, automatic `System`. | `.specify/memory/constitution.md` §4; `docs/use-cases.md` §1 Actors. |
| Role combinations | A person may hold any valid combination of `User`, `Approver`, `HR`; context selector is UI convenience only. | `.specify/memory/constitution.md` §4.5; `specs/002-role-based-frontend-views/spec.md` §§4.1-4.2. |
| User responsibilities | Create vacation requests, view own requests/status/history/balance/calendar, edit own `Pending` requests only. | `.specify/memory/constitution.md` §4.1; `docs/use-cases.md` UC-03 through UC-08. |
| Approver responsibilities | Active Approver with capability resolves eligible non-owned requests; approve, reject, pre-start deactivate. | `.specify/memory/constitution.md` §4.2; `docs/use-cases.md` §1 and UC-09 through UC-15; `uc-contracts.md` UC-11 authorization. |
| HR responsibilities | Organization-wide read access plus `canResolveRequests` management for existing Approvers; no request resolution, balance modification, or role assignment/removal. | `.specify/memory/constitution.md` §4.3; `docs/use-cases.md` UC-18 through UC-22 and AC-HR-010 through AC-HR-012. |
| States | `Pending`, `Approved`, `Rejected`, `CancelledByTimeout`, `CancelledByApprover`. | `.specify/memory/constitution.md` §5; `docs/use-cases.md` §1; `data-model.md` VacationRequest Lifecycle. |
| Transitions | `Pending -> Approved`, `Pending -> Rejected`, `Pending -> CancelledByTimeout`, `Approved -> CancelledByApprover` before start; creation and edit are operations, not official transitions. | `.specify/memory/constitution.md` §5; `specs/001-leave-management-mvp/diagrams/request-lifecycle.md` lines 5-10. |
| Business rules | 23 active FRs, 36 active BRs including projected-balance `BR-036` through `BR-038`, 47 standard ACs, 12 HR ACs, 34 RBFV criteria. | `spec.md` FR/BR/AC identifier extraction; `docs/use-cases.md` AC-HR sections; `specs/002.../spec.md` RBFV table. |
| Validation | Server authoritative date, input-mode, reason, balance, overlap, concurrency, and stale operation validation; client-derived values ignored. | `spec.md` VAL-001 through VAL-009; `docs/use-cases.md` UC-04, UC-05, UC-11, UC-22. |
| Balance | One global balance; accrued, reserved, deducted, available non-negative; Pending reserves, approval deducts, rejection/timeout releases, pre-start deactivation restores. | `.specify/memory/constitution.md` §5; `data-model.md` VacationBalance Lifecycle. |
| Concurrency | RowVersion on mutable aggregates, idempotent timeout/accrual, one-winner transitions, failure rollback. | `.specify/memory/constitution.md` §6; `data-model.md` Concurrency and Idempotency Matrix. |
| Transactions | Critical operations are all-or-nothing with audit and balance effects. | `plan.md` Atomic Operations Matrix; `uc-contracts.md` UC-04, UC-11, UC-13, UC-16, UC-17, UC-22. |
| Audit | Immutable audit for transitions and critical changes; security events via Serilog; HR reason access audited without reason content. | `.specify/memory/constitution.md` §§8, 7.3; `data-model.md` AuditRecord; `docs/use-cases.md` AC-HR-009. |
| Security | Deny-by-default, server-side authorization, anti-forgery, IDOR/forced browsing/overposting/session tests, no UI-only authorization. | `.specify/memory/constitution.md` §§7, 11.1; `specs/002.../spec.md` §9.3. |
| UI/UX | Spanish UI labels, Bootstrap 5.3, WCAG 2.1 AA, no gradients/emojis, accessible calendars, role-based navigation. | `.specify/memory/constitution.md` §§3.3, 11; `frontend-design-spec.md`; `specs/002.../spec.md`. |
| Use cases | UC-01 through UC-22 active. | `docs/use-cases.md` headings; `uc-contracts.md` Covers UC-01 through UC-22. |
| Expected tests | Unit, integration, E2E, security, accessibility, concurrency, idempotency, audit, traceability tests planned in T013 through T153. | `specs/001-leave-management-mvp/tasks.md` T013-T153. |

### Justified inferences

| Inference | Basis |
|---|---|
| The system is not implementation-ready until documentary defects are corrected, even though all use cases have planned tasks. | Constitution §15.1 says derived artifacts cannot override authoritative sources; current frontend auth and route conflicts can cause divergent implementation. |
| Coverage report coverage is overstated for business rules because it counts 35 rules while active spec text defines 36 active `BR-XXX` identifiers. | `spec.md` defines `BR-036`, `BR-037`, `BR-038`; `tasks/coverage-report.md` says `BR-001 through BR-035` and total 35. |

### Open questions

No implementation-blocking product decision remains open. The report includes two documentary clarification questions because active artifacts conflict on route and governance path wording.

## 6. Findings summary

| Severity | Count | Blocking | Human decision required |
|---|---:|---:|---:|
| BLOCKER | 0 | 0 | 0 |
| CRITICAL | 0 | 0 | 0 |
| HIGH | 2 | 2 | 1 |
| MEDIUM | 3 | 0 | 1 |
| LOW | 3 | 0 | 0 |
| INFO | 2 | 0 | 0 |

## 7. Findings by severity

### BLOCKER

No BLOCKER findings.

### CRITICAL

No CRITICAL findings.

### HIGH

#### F-HIGH-001

| Field | Value |
|---|---|
| Severity | HIGH |
| Status | OPEN |
| Title | Role-based frontend authorization omits `canResolveRequests` where active use cases require it |
| Category | Authorization conflict |
| Affected files | `specs/002-role-based-frontend-views/spec.md`; `docs/use-cases.md`; `specs/001-leave-management-mvp/contracts/uc-contracts.md`; `specs/001-leave-management-mvp/tasks.md` |
| Exact sections or identifiers | RBFV §5.1, §9.1, §9.2, RBFV-003/RBFV-012; UC-09, UC-10, UC-11, UC-13; UC contracts UC-11; T017, T090 |
| Related requirements | AUTHZ-002, AUTHZ-009, AUTHZ-010, AUTHZ-012 |
| Related business rules | Not directly BR; authorization rule conflict |
| Related acceptance criteria | RBFV-003, RBFV-012, AC-047 |
| Related use cases | UC-09, UC-10, UC-11, UC-13, UC-22 |
| Related tasks | T017, T090, T137-T145 |
| Evidence | `docs/use-cases.md` §1 states an Approver resolves only when `Active` and `canResolveRequests=true`; UC-09 precondition requires `canResolveRequests=true`; UC-10 main flow revalidates capability; UC-11 precondition requires capability. `uc-contracts.md` UC-11 authorization says `Active Approver + not owner + canResolveRequests=true`. `specs/002.../spec.md` §9.1 defines `RequireActiveApprover` as only authenticated + active + Approver claim, and `RequireApproverEligible` as Active Approver + not owner + request in eligible state. |
| Description | The frontend routing policy definitions and RBFV route table can be read as allowing disabled Approvers to enter `/aprobaciones` and possibly detail routes if they are active and hold the role. Use cases and contracts require capability checks for queue/detail/resolution. |
| Impact | A developer could implement route authorization without `canResolveRequests`, creating an authorization bypass for disabled Approvers at least on read routes and increasing risk on resolution routes. |
| Recommended documentary correction | Amend RBFV §5.1, §9.1, §9.2, RBFV-003, and RBFV-012 to explicitly include `canResolveRequests=true` for Approver queue, detail, and resolution policies where required; preserve Application-layer revalidation in T090. |
| Human decision required | NO |

#### F-HIGH-002

| Field | Value |
|---|---|
| Severity | HIGH |
| Status | OPEN |
| Title | HR calendar route is ambiguous between shared `/calendario` and HR-specific `/rrhh/calendario` |
| Category | Route / permission conflict |
| Affected files | `specs/002-role-based-frontend-views/spec.md`; `docs/use-cases.md`; `specs/001-leave-management-mvp/contracts/uc-contracts.md`; `specs/001-leave-management-mvp/tasks.md`; `specs/001-leave-management-mvp/plan.md` |
| Exact sections or identifiers | RBFV §5.2 shared routes, §6.3 HR context, §7.3 HR navigation; UC-19; contract UC-19; T122, T130, T134; plan project structure `CalendarioController` |
| Related requirements | FR-017, FR-024, AUTHZ-011, AUTHZ-017, AUTHZ-019 |
| Related business rules | None |
| Related acceptance criteria | AC-HR-003, RBFV-026, RBFV-032 |
| Related use cases | UC-08, UC-15, UC-19 |
| Related tasks | T118, T120-T122, T124, T130, T134 |
| Evidence | `specs/002.../spec.md` §5.2 lists `/calendario` as authorized for `User`, `Approver`, and `HR`, while §6.3 and §7.3 define HR calendar as `/rrhh/calendario`. `docs/use-cases.md` UC-19 and `uc-contracts.md` UC-19 use `/rrhh/calendario`. `tasks.md` T122 says integrate User, Approver, and HR calendar Razor views under shared views, while T134 separately implements `/rrhh/calendario`. `plan.md` project structure says `CalendarioController.cs` supports User + Approver + HR via role context. |
| Description | Active artifacts do not clearly decide whether HR may access an organizational calendar through `/calendario`, only `/rrhh/calendario`, or both. Because User `/calendario` is own-only and Approver `/calendario` is anonymized, this ambiguity affects data exposure and navigation. |
| Impact | Implementation may expose requester names or organization-wide calendar data on a shared route not intended for HR, or may deny a route listed as allowed in RBFV. This affects security, QA route coverage, and frontend navigation consistency. |
| Recommended documentary correction | Choose one canonical HR calendar entry point and update RBFV §5.2/§6.3/§7.3, UC-19, contracts, plan structure, and T122/T134 wording accordingly. |
| Human decision required | YES |
| Open question | Should active HR access the organizational calendar only at `/rrhh/calendario`, or is `/calendario` also an approved HR route? |

### MEDIUM

#### F-MED-001

| Field | Value |
|---|---|
| Severity | MEDIUM |
| Status | OPEN |
| Title | Generated coverage report is stale for active projected-balance business rules |
| Category | Generated artifact divergence |
| Affected files | `specs/001-leave-management-mvp/spec.md`; `docs/use-cases.md`; `tasks/coverage-report.md`; `tasks/EPIC-000-general-system/TASK-079.md`; `tasks/EPIC-000-general-system/TASK-086.md` |
| Exact sections or identifiers | BR-036, BR-037, BR-038; UC-10, UC-11; T079, T086 |
| Related requirements | FR-023 |
| Related business rules | BR-036, BR-037, BR-038 |
| Related acceptance criteria | RBFV-025, AC-012, AC-018, AC-020 |
| Related use cases | UC-10, UC-11 |
| Related tasks | T079, T080, T086, T088 |
| Evidence | `spec.md` defines `BR-036`, `BR-037`, and `BR-038`. `docs/use-cases.md` UC-10 references BR-036 and BR-037; UC-11 references BR-037 and BR-038. `tasks/coverage-report.md` Results says "Identified business rules: 35" and Business Rule Coverage says `BR-001 through BR-035`. TASK-079 and TASK-086 include FR-023 and projected-balance semantics but do not include BR-036/037/038 identifiers. |
| Description | The active business-rule inventory is 36 rules after excluding retired BR-010 and BR-022, not 35. The generated coverage report is therefore stale and overstates its recalculated accuracy. |
| Impact | QA and implementation tracking may miss negative projected-balance validation/revalidation coverage, especially server-side approval POST revalidation. |
| Recommended documentary correction | Regenerate or correct `tasks/coverage-report.md` and update generated task technical notes to include BR-036, BR-037, and BR-038 where semantically covered. |
| Human decision required | NO |

#### F-MED-002

| Field | Value |
|---|---|
| Severity | MEDIUM |
| Status | OPEN |
| Title | Task coverage for projected-balance rules is partial by semantic evidence |
| Category | Traceability gap |
| Affected files | `specs/001-leave-management-mvp/tasks.md`; `tasks/EPIC-000-general-system/TASK-079.md`; `tasks/EPIC-000-general-system/TASK-086.md`; representative approval tasks T080/T088 |
| Exact sections or identifiers | T079, T080, T086, T088; BR-036, BR-037, BR-038 |
| Related requirements | FR-023 |
| Related business rules | BR-036, BR-037, BR-038 |
| Related acceptance criteria | AC-012, AC-018, AC-020, RBFV-025 |
| Related use cases | UC-10, UC-11 |
| Related tasks | T079, T080, T086, T088 |
| Evidence | T079 covers "UC-10 resolution detail tests with projected balance"; T086 covers Approver queue/detail queries; no generated task-file evidence found for explicit `BR-036`, `BR-037`, or `BR-038`. UC-11 requires approval POST projected-balance revalidation. |
| Description | Query/display coverage exists, but the task set does not explicitly bind BR-037/BR-038 to approval command tests and implementation. |
| Impact | A developer could implement display-only projected balance and miss server-side conflict handling for negative projected balance at approval time. |
| Recommended documentary correction | Add explicit BR-037/BR-038 references and wording to approval test/command tasks T080 and T088, while preserving T079/T086 for display/query behavior. |
| Human decision required | NO |

#### F-MED-003

| Field | Value |
|---|---|
| Severity | MEDIUM |
| Status | OPEN |
| Title | Decision-record directory conflicts with constitutional ADR path |
| Category | Governance / authority ambiguity |
| Affected files | `.specify/memory/constitution.md`; `docs/decisions/README.md`; `docs/decisions/DR-001-runtime-configuration-values.md`; `docs/decisions/DR-002-generated-artifact-review-rule.md`; `specs/001-leave-management-mvp/plan.md` |
| Exact sections or identifiers | Constitution §14; Constitution §15.1; DR-001; DR-002 |
| Related requirements | CFG-001, CFG-002, CFG-003 |
| Related business rules | None |
| Related acceptance criteria | AC-048, AC-055, AC-060 |
| Related use cases | UC-01, UC-16, UC-17 |
| Related tasks | T019, T099, T114 |
| Evidence | Constitution §14 states relevant architectural decisions MUST be recorded under `docs/adr/`. The repository contains accepted decisions under `docs/decisions/`; `docs/decisions/README.md` indexes DR-001 and DR-002 as Accepted. `plan.md` project structure still lists `docs/adr/` and sample `ADR-*.md`, while research and tasks reference `docs/decisions/DR-001...`. |
| Description | The repository uses `docs/decisions/` as active accepted decision authority, but the constitution still mandates `docs/adr/`. |
| Impact | Future governance checks may reject valid decisions or duplicate decisions across two directories. |
| Recommended documentary correction | Either amend the constitution to recognize `docs/decisions/` as the ADR/decision path, or move/rename decision records through a governed change. |
| Human decision required | YES |
| Open question | Should accepted architectural decisions for NovaLeave live under `docs/adr/` as the constitution currently requires, or is `docs/decisions/` the approved replacement path? |

### LOW

#### F-LOW-001

| Field | Value |
|---|---|
| Severity | LOW |
| Status | OPEN |
| Title | Diagram links use `Diagrams/` but actual directory is `diagrams/` |
| Category | Broken relative link / portability |
| Affected files | `specs/001-leave-management-mvp/plan.md`; `specs/001-leave-management-mvp/data-model.md`; `specs/001-leave-management-mvp/diagrams/` |
| Exact sections or identifiers | `plan.md` Clean-Architecture Dependency Diagram; `data-model.md` Diagrams |
| Related requirements | Architecture documentation requirements |
| Related business rules | None |
| Related acceptance criteria | None |
| Related use cases | None |
| Related tasks | T153 |
| Evidence | `plan.md` references `specs/001-leave-management-mvp/Diagrams/clean-architecture.md`; `data-model.md` references `Diagrams/...`; actual files are under `specs/001-leave-management-mvp/diagrams/`. |
| Description | Case mismatch is tolerated on Windows but breaks links on case-sensitive systems and contradicts exact path traceability. |
| Impact | Documentation navigation and tooling may fail outside Windows. |
| Recommended documentary correction | Normalize links to `specs/001-leave-management-mvp/diagrams/...`. |
| Human decision required | NO |

#### F-LOW-002

| Field | Value |
|---|---|
| Severity | LOW |
| Status | OPEN |
| Title | Coverage report contains stale branch/working-tree metadata |
| Category | Generated artifact staleness |
| Affected files | `tasks/coverage-report.md` |
| Exact sections or identifiers | Metadata; Findings |
| Related requirements | Traceability reporting |
| Related business rules | None |
| Related acceptance criteria | None |
| Related use cases | None |
| Related tasks | T153 |
| Evidence | Coverage report metadata says commit `fff4b94...` and mentions re-validation on `branch-john`, while this audit analyzed branch `abraham-villalobos` at `c2a328...`. Its findings mention prior deleted/untracked files that are not this audit's baseline. |
| Description | The generated report is not synchronized with the currently analyzed commit. |
| Impact | Consumers may rely on outdated counts or stale worktree claims. |
| Recommended documentary correction | Regenerate coverage report after correcting semantic findings and include current branch/SHA. |
| Human decision required | NO |

#### F-LOW-003

| Field | Value |
|---|---|
| Severity | LOW |
| Status | OPEN |
| Title | Canonical documentation task T012 targets `AGENTS.md`, which already matches the requested reference |
| Category | Potential redundant task |
| Affected files | `AGENTS.md`; `specs/001-leave-management-mvp/tasks.md`; `tasks/EPIC-000-general-system/TASK-012.md` |
| Exact sections or identifiers | T012, TASK-012 |
| Related requirements | Repository guidance |
| Related business rules | None |
| Related acceptance criteria | None |
| Related use cases | None |
| Related tasks | T012 |
| Evidence | `AGENTS.md` instructs reading `specs/001-leave-management-mvp/plan.md`. T012 says update `AGENTS.md` Spec Kit section to reference the same path. |
| Description | T012 may be already satisfied at planning time and may become a no-op. |
| Impact | Minor execution confusion; not a scope or correctness defect. |
| Recommended documentary correction | Mark T012 as verification-only or remove it in a governed task update if canonical tasks are regenerated. |
| Human decision required | NO |

### INFO

#### F-INFO-001

| Field | Value |
|---|---|
| Severity | INFO |
| Status | VERIFIED |
| Title | No source implementation exists yet |
| Category | Repository state |
| Affected files | `plan.md`; repository root |
| Exact sections or identifiers | `plan.md` Repository Assessment |
| Evidence | `plan.md` states no `src/`, `tests/`, `.sln`, `.csproj`, or `.cs` files exist; `rg --files` inventory found documentation/tasks only. |
| Description | Implementation, runtime tests, and security controls are planned, not implemented or verified. |
| Impact | Audit scope is planning/documentation readiness only. |
| Human decision required | NO |

#### F-INFO-002

| Field | Value |
|---|---|
| Severity | INFO |
| Status | VERIFIED |
| Title | Generated task tree is structurally present |
| Category | Structural validation |
| Affected files | `specs/001-leave-management-mvp/tasks.md`; `tasks/EPIC-000-general-system/` |
| Exact sections or identifiers | T001-T153; TASK-001-TASK-153 |
| Evidence | `Get-ChildItem tasks\EPIC-000-general-system\TASK-*.md` found 153 files; canonical `tasks.md` lists T001 through T153; generated files sampled embed canonical text and include acceptance criteria/dependencies/technical notes. |
| Description | Structural task generation appears complete, but semantic validation findings remain. |
| Impact | Structural completeness does not prove business/authorization correctness. |
| Human decision required | NO |

## 8. Responsibility-conflict matrix

| Responsibility | Expected actor or layer | Actual actor or layer found | Sources | Status | Impact |
|---|---|---|---|---|---|
| Domain invariants | Domain | Domain tasks T020-T031, data model | Constitution §§2, 5; data-model Domain Traceability; tasks T020-T031 | OK | Clear ownership. |
| Input validation | Application using FluentValidation; Domain for invariants | T073 and validation specs | Constitution §2.IV; tasks T007, T073 | OK | Clear split. |
| Server-side authorization | Presentation policies plus Application revalidation | Mostly correct; RBFV omits `canResolveRequests` in policy definitions | RBFV §9; use cases UC-09/10/11; T017/T090 | CONFLICT | Disabled Approver ambiguity. |
| UI authorization | UI may hide buttons but cannot authorize | RBFV §9.3 says hidden UI is not authorization | RBFV §9.3; Constitution §7 | OK | Clear prohibition. |
| HR request resolution prohibition | Application/server authorization | HR prohibition present in constitution, use cases, RBFV, tasks | Constitution §4.3; AC-HR-010; T127/T133/T136 | OK | Covered. |
| HR calendar data exposure | HR-specific `/rrhh/calendario`; possibly shared `/calendario` | Conflicting route statements | RBFV §5.2/§6.3; UC-19; contracts UC-19 | CONFLICT | Potential route/data exposure ambiguity. |
| Balance calculations | Domain/Application query models; never client | Domain/services and Application queries planned | Constitution §2.III/IV; data-model; T026/T029/T069/T086 | OK | Clear. |
| Projected balance approval revalidation | Application approval command | Query/display tasks explicit; approval task coverage not explicit | BR-036/037/038; UC-11; T080/T088 | PARTIAL | Risk of display-only implementation. |
| Transactions | Application use cases + Infrastructure EF transaction | Planned in tasks and contracts | UC contracts; plan Atomic Operations; tasks T071/T088/T089/T104/T113/T142 | OK | Clear. |
| Auditing | Application abstraction, immutable AuditRecord, Serilog for security | Planned across domain/app/infra | Constitution §8; data-model; T018/T031/T034/T132/T148 | OK | Clear. |
| Persistence | Infrastructure EF Core | Planned in T039-T047 | Constitution §2.I; data-model | OK | Clear. |
| Presentation | Controllers thin, Razor ViewModels, Bootstrap | Planned in T051-T056, T074-T077, T091-T093, T134-T145 | Constitution §11; frontend specs | OK except route conflict | Mostly clear. |

## 9. Cross-document consistency matrix

| Concept | Source A | Source B | Result | Evidence |
|---|---|---|---|---|
| Authority order | Constitution §15.1 | RBFV §3 | OK | Both place constitution first and derived artifacts below approved specs. |
| Roles | Constitution §4 | spec.md status/input and docs/use-cases §1 | OK | User, Approver, HR, System consistently present. |
| Role combinations | Constitution §4.5 | RBFV §4.1 | OK | Any valid combination allowed; context switcher does not change auth. |
| Approver capability | docs/use-cases UC-09/10/11 | RBFV §9.1 policies | CONFLICT | `canResolveRequests` required in use cases but omitted in RBFV policy definitions. |
| HR route `/rrhh/calendario` | UC-19 and contracts UC-19 | RBFV §5.2 shared `/calendario` allows HR | CONFLICT | Both routes appear active for HR. |
| States/transitions | Constitution §5 | request-lifecycle diagram | OK | Diagram lists only official four transitions, creation/edit labeled non-transition. |
| Owner Pending cancellation | spec.md OQ-001 / Explicitly Out of Scope | tasks.md quality gates | OK | No task introduces User Pending cancellation. |
| Projected balance rules | spec.md BR-036/037/038 | coverage-report business rules | CONFLICT | Coverage report still says BR-001 through BR-035 and total 35. |
| ADR path | Constitution §14 | docs/decisions/README.md | AMBIGUOUS | Constitution says `docs/adr/`; repository uses `docs/decisions/`. |
| Diagram paths | actual `diagrams/` directory | plan/data-model links `Diagrams/` | AMBIGUOUS | Case mismatch affects case-sensitive environments. |

## 10. Gap analysis

### Requirements

- No missing active FR found. Active FR inventory is 23: FR-001 through FR-007, FR-009, FR-011 through FR-025. FR-008 and FR-010 are retired.

### Business rules

- Gap: BR-036/BR-037/BR-038 are active but not counted in `tasks/coverage-report.md` and not explicitly tagged in relevant generated tasks. Severity MEDIUM. Evidence: F-MED-001.

### Acceptance criteria

- Standard ACs and AC-HR are present, but the generated report total of 72 was not reproduced from primary identifiers: 47 `AC-XXX` in `spec.md` plus 12 `AC-HR-XXX` in `docs/use-cases.md` equals 59 acceptance criteria before RBFV criteria. If RBFV criteria are counted separately, total is 93. This requires coverage-report correction.

### Use cases

- UC-01 through UC-22 are present in `docs/use-cases.md` and `uc-contracts.md`. No missing use case found.

### Flows

- Main, alternate, and exception flows exist for all UC-01 through UC-22. No missing exception-flow class found, but F-HIGH-001 affects Approver capability flow authorization.

### States

- Five states are consistently defined. No undefined or retired active state found.

### Validations

- Most validations are traceable. Projected negative-balance validation at approval POST needs explicit task traceability (F-MED-002).

### Authorization

- Gap: RBFV policy definitions omit `canResolveRequests` in Approver route policies (F-HIGH-001).
- Gap: HR calendar route ambiguity affects authorization and data scope (F-HIGH-002).

### Security

- Security requirements are broadly defined; task coverage includes HR forbidden mutation, CSRF, IDOR, forced browsing, overposting, session timeout. Semantic route/policy conflicts remain.

### Data integrity

- No data model contradiction found for balances, movements, audit, or rowversion. Coverage of projected-balance deduction path is partial by task evidence.

### Concurrency

- Concurrency matrix is complete in `data-model.md`; tasks exist for creation, approval, timeout, deactivation, accrual, HR capability. No circular concurrency rule found.

### Transactions

- Atomic operations matrix and tasks cover major write operations. No missing transactional boundary found, except projected-balance revalidation task explicitness.

### Auditing

- Audit requirements are defined. HR reason access audit is covered in AC-HR-009 and T126/T132.

### Error handling

- ERR-001 through ERR-006 are traced in `spec.md`. Safe outcomes are planned in T036/T051 and use cases.

### Accessibility

- RBFV and frontend design specs define WCAG, keyboard, focus, pagination, calendar behavior. T119/T150 cover tests.

### Tests

- Planned tests exist before implementation tasks. Not executable yet because no source/test projects exist.

### Tasks

- 153 canonical and generated tasks exist. Semantic defects: projected-balance BR task traceability partial; HR calendar route ambiguity; RBFV policy conflict.

### Decisions

- DR-001 and DR-002 are accepted, but decision-record directory conflicts with constitutional path.

### Terminology

- `HR` technical role and `RRHH` Spanish context are explained in RBFV §4. No unresolved actor terminology conflict found.

### Generated artifacts

- `tasks/coverage-report.md` is stale for branch/SHA and business-rule count. Generated task files appear structurally present.

## 11. Traceability matrix

| Authority source | Requirement | Business rule | Acceptance criterion | Use case | Contract | Layer or component | Test | TXXX | TASK-XXX | Status |
|---|---|---|---|---|---|---|---|---|---|---|
| Constitution §§4-5; spec.md | FR-001/FR-002/FR-012/FR-025 | BR-001/002/004/005/018/019/024/030/034 | AC-001-005, AC-009-011, AC-035, AC-040-043, AC-058/059 | UC-04 | UC-04 contract | Domain/Application/Presentation | UC04CreateVacationRequestTests | T062, T071, T073, T074 | TASK-062, TASK-071, TASK-073, TASK-074 | OK |
| Constitution §4.1; spec.md | FR-003/FR-004 | BR-012/017/031 | AC-007/008/044 | UC-03, UC-07 | UC-03, UC-07 contracts | Application queries, User MVC | UC03, UC07 tests | T061, T065, T068, T069 | TASK-061, TASK-065, TASK-068, TASK-069 | OK |
| spec.md | FR-005 | BR-025 | AC-045/046 | UC-05 | UC-05 contract | Application command, User MVC | UC05EditPendingRequestTests | T063, T072 | TASK-063, TASK-072 | OK |
| Constitution §4.2; spec.md | FR-006/FR-007 | BR-008/009/013/014/015/016/020/021/023 | AC-012/016/017/018/019/020/021 | UC-11, UC-12 | UC-11, UC-12 contracts | Application approval/rejection | UC11/UC12 tests | T080, T081, T088, T089, T090 | TASK-080, TASK-081, TASK-088, TASK-089, TASK-090 | PARTIAL: capability and projected-balance explicitness gaps |
| spec.md | FR-023 | BR-036/037/038 | RBFV-025, AC-012/018/020 | UC-10, UC-11 | UC-10, UC-11 contracts | Approver queries and approval command | UC10 detail, UC11 approval | T079, T080, T086, T088 | TASK-079, TASK-080, TASK-086, TASK-088 | PARTIAL |
| Constitution §5; spec.md | FR-011 | BR-027/028/035 | AC-050-053 | UC-13 | UC-13 contract | Application deactivation command | UC13 tests | T101-T107 | TASK-101-TASK-107 | OK |
| DR-001; spec.md | FR-013, CFG-001 | BR-026 | AC-048/049 | UC-16 | UC-16 contract | Background job/Application/System audit | UC16 timeout/idempotency/concurrency tests | T094-T100 | TASK-094-TASK-100 | OK |
| spec.md OQ-002; DR-001 | FR-014 | BR-032/033 | AC-054 | UC-17 | UC-17 contract | Domain policy/Application job/Infrastructure schedule | Accrual tests | T108-T116 | TASK-108-TASK-116 | OK |
| spec.md/RBFV | FR-015/017/024 | N/A | AC-057, AC-HR-003, RBFV-026 | UC-08/15/19 | UC-08/15/19 contracts | Calendar models, shared partial, HR views | CalendarAuthorizationTests | T117-T124, T130, T134 | TASK-117-TASK-124, TASK-130, TASK-134 | CONFLICT: HR route ambiguity |
| Constitution §4.3; docs/use-cases | FR-009/016-020 | N/A | AC-HR-001-006, AC-HR-009-012 | UC-18-21 | UC-18-21 contracts | HR read queries/controllers/views | HR tests | T123-T136 | TASK-123-TASK-136 | OK |
| Constitution §4.3; docs/use-cases | FR-021/022 | N/A | AC-HR-007/008 | UC-22 | UC-22 contract | HR capability query/command/controller | Capability tests | T137-T145 | TASK-137-TASK-145 | OK |
| Constitution §§7-9, 11 | SEC/AUTHZ/CON/AUD/ERR | N/A | RBFV, AC-HR security ACs | Cross-cutting | UC contracts | Filters, policies, tests | SecurityRegressionTests, Traceability tests | T017, T051, T127, T146-T150 | TASK-017, TASK-051, TASK-127, TASK-146-TASK-150 | PARTIAL: RBFV policy wording conflict |

## 12. Recalculated coverage results

Formula: `Coverage percentage = Covered / Total * 100`. `Covered` requires semantic task support from canonical/generator task text, not identifier appearance alone. `Partial`, `Conflict`, and `Ambiguous` are not counted as covered.

| Category | Total | Covered | Partial | Missing | Conflict | Ambiguous | Coverage percentage |
|---|---:|---:|---:|---:|---:|---:|---:|
| Functional requirements | 23 | 22 | 1 | 0 | 0 | 0 | 95.7% |
| Business rules | 36 | 33 | 3 | 0 | 0 | 0 | 91.7% |
| Standard acceptance criteria | 47 | 44 | 3 | 0 | 0 | 0 | 93.6% |
| HR acceptance criteria | 12 | 12 | 0 | 0 | 0 | 0 | 100% |
| RBFV criteria | 34 | 31 | 0 | 0 | 2 | 1 | 91.2% |
| Use cases | 22 | 21 | 0 | 0 | 1 | 0 | 95.5% |
| Security requirements | 9 | 9 | 0 | 0 | 0 | 0 | 100% |
| Authorization requirements | 19 | 16 | 1 | 0 | 2 | 0 | 84.2% |
| Validation requirements | 8 | 8 | 0 | 0 | 0 | 0 | 100% |
| Audit requirements | 10 | 10 | 0 | 0 | 0 | 0 | 100% |
| Concurrency requirements | 12 | 11 | 1 | 0 | 0 | 0 | 91.7% |
| Frontend requirements | 34 RBFV + frontend sections | 31 | 0 | 0 | 2 | 1 | 91.2% |
| Canonical tasks | 153 | 153 | 0 | 0 | 0 | 0 | 100% structural |
| Generated task files | 153 | 153 | 0 | 0 | 0 | 0 | 100% structural |
| Required tests | 51 planned test tasks | 49 | 2 | 0 | 0 | 0 | 96.1% |
| Explicit exclusions | 28 reported | 28 | 0 | 0 | 0 | 0 | 100% |

## 13. Comparison with existing coverage reports

| Existing report / artifact | Existing claim | Recalculated result | Discrepancy |
|---|---|---|---|
| `tasks/coverage-report.md` | Functional requirements 23/23, 100% | 22 covered, 1 partial (FR-023 due BR-036/037/038 explicit task traceability) | Existing report counts identifier/task presence as enough. |
| `tasks/coverage-report.md` | Business rules 35/35, 100%; BR-001 through BR-035 | 36 active BRs; 33 covered, 3 partial | Report misses BR-036 through BR-038. |
| `tasks/coverage-report.md` | Acceptance criteria 72/72, 100% | 47 standard AC + 12 AC-HR + 34 RBFV = separate inventories; not 72 from primary identifiers | Report category definition is unclear or stale. |
| `tasks/coverage-report.md` | Use cases 22/22, 100% | 21 covered, 1 conflict (UC-19 route ambiguity) | UC-19 is not cleanly covered until HR calendar route is resolved. |
| `tasks/coverage-report.md` | Canonical task count 153; generated task count 153 | 153 and 153 | Matches structurally. |
| `tasks/BACKLOG.md` | Canonical `tasks.md` remains source; `/tasks` derived view | Confirmed | Matches. |
| Previous audit reports | Historical readiness/remediation context | Not normative; current findings supersede prior conclusions where current files differ | Previous audit content not treated as authority. |
| `tools/validate_tasks_tree.py` | Would validate no artifacts/BOM, 153 round-trip, active FR references | Could not execute; manual structural checks partially reproduced | Tool requires Python not available. |

## 14. Task audit

### Structural errors

- No missing generated task files found: 153 generated files for T001-T153.
- Python validator could not be executed, so full automated round-trip is NOT VERIFIED.

### Semantic errors

- Projected-balance BR-036/037/038 not explicitly bound to approval command tasks (F-MED-001, F-MED-002).
- RBFV authorization policy conflict affects tasks that would implement route authorization (F-HIGH-001).
- HR calendar route ambiguity affects calendar tasks (F-HIGH-002).

### Incorrect dependencies

- No circular dependency was identified from available task metadata. Representative generated files include dependency sections.
- T122 depends on User/Approver/calendar tests but HR calendar implementation also appears in T130/T134; this is acceptable only after HR route is clarified.

### Circular dependencies

- None identified by document review. Automated validation not executed.

### Missing tasks

- No missing `TXXX`/`TASK-XXX` structural task found.
- Missing semantic explicitness for BR-037/BR-038 in approval-command tasks.

### Duplicate tasks

- No duplicate `TASK-XXX` file found structurally.
- Calendar implementation is split between shared partial T121/T122 and HR T130/T134; not duplicate if route ownership is clarified.

### Orphaned tasks

- No orphan generated task identified structurally.

### Redundant tasks

- T012 may be redundant because AGENTS.md already references plan.md (F-LOW-003).

### Out-of-scope tasks

- No task found introducing APIs, JWT, email, outbox, Redis, teams, hierarchy, User Pending cancellation, holiday calendars, or HR request resolution.

### Incorrect layer assignments

- No direct layer assignment error found in tasks. T090/T133 place authorization policies in Application; RBFV still needs policy wording correction.

### Responsibility conflicts

- RBFV route policies vs Application/use-case capability revalidation conflict (F-HIGH-001).

### Insufficiently defined tasks

- T080/T088 insufficiently explicit for BR-037/BR-038.
- T122/T130/T134 insufficiently clear until HR calendar route is resolved.

### Invalid file paths

- Task paths are planned paths and internally consistent. Documentation diagram links have case mismatch (F-LOW-001).

### Missing completion conditions

- Representative generated tasks include acceptance criteria. No broad missing completion-condition pattern found.

### Missing tests

- No missing test category found; tests are planned, not implemented. Projected-balance approval tests need explicit BR links.

### Authorization gaps

- F-HIGH-001.

### Concurrency gaps

- BR-038/approval revalidation task explicitness is partial.

### Transaction gaps

- No transaction boundary missing, but BR-038 must be explicitly tied to approval command transaction.

### Audit gaps

- No audit task gap found; HR reason audit covered by T126/T132 and AC-HR-009.

### Security gaps

- F-HIGH-001 and F-HIGH-002.

### Differences between canonical and generated tasks

- Generated files sampled embed canonical task text and preserve identifiers. Full round-trip automated validation is NOT VERIFIED because Python is unavailable.

## 15. Differences between task artifacts

| Artifact pair | Difference |
|---|---|
| `specs/001-leave-management-mvp/tasks.md` vs generated `TASK-*.md` | Generated files add metadata, source IDs, acceptance criteria, dependencies, and technical notes. Sampled files preserve canonical text. |
| `specs/001-leave-management-mvp/tasks.md` vs `tasks/BACKLOG.md` | Backlog is a derived index and explicitly states canonical `tasks.md` remains source. |
| `specs/001-leave-management-mvp/tasks.md` vs `tasks/coverage-report.md` | Coverage report claims 100% coverage but misses active BR-036/037/038 count and has stale branch/SHA metadata. |
| Generated `TASK-*.md` vs `tasks/coverage-report.md` | TASK-079/TASK-086 include FR-023/projected-balance semantics, while coverage report business-rule inventory excludes BR-036/037/038. |
| `tasks/BACKLOG.md` vs generated files | Backlog says all TASK files physically live in `EPIC-000-general-system`; file inventory confirms. |

## 16. Open questions

| Question ID | Priority | Exact question | Reason it cannot be resolved | Affected files | Affected identifiers | Impact | Required decision owner |
|---|---|---|---|---|---|---|---|
| OQ-AUD-001 | HIGH | Should active HR access the organizational calendar only at `/rrhh/calendario`, or is `/calendario` also an approved HR route? | Active artifacts conflict: RBFV shared route allows HR on `/calendario`, but UC-19/contracts define `/rrhh/calendario`. | `specs/002-role-based-frontend-views/spec.md`; `docs/use-cases.md`; `uc-contracts.md`; `tasks.md`; `plan.md` | UC-19, FR-017, FR-024, AUTHZ-011, AUTHZ-017, RBFV-026, T122, T130, T134 | Affects route implementation, data exposure, navigation, and tests. | Product Owner / Security / Architect |
| OQ-AUD-002 | MEDIUM | Should accepted architectural decisions for NovaLeave live under `docs/adr/` as the constitution currently requires, or is `docs/decisions/` the approved replacement path? | Constitution §14 mandates `docs/adr/`, but accepted DR files exist under `docs/decisions/` and active artifacts reference them. | `.specify/memory/constitution.md`; `docs/decisions/*`; `plan.md`; `research.md` | DR-001, DR-002, CFG-001, CFG-002, T019, T099, T114 | Affects governance, traceability, future decision placement. | Architect |

## 17. Recommended correction order

1. Correct the authorization conflict: update RBFV Approver policy definitions and route criteria to include `canResolveRequests=true` wherever use cases/contracts require it.
2. Resolve the HR calendar route decision and update all affected specifications, contracts, plan, and tasks.
3. Correct projected-balance traceability: update coverage report and task technical notes for BR-036, BR-037, and BR-038, especially approval POST revalidation.
4. Resolve the decision-record directory governance issue (`docs/adr/` vs `docs/decisions/`).
5. Normalize diagram link casing to `diagrams/`.
6. Regenerate or amend `tasks/coverage-report.md` with current branch/SHA and recalculated category definitions.
7. Re-run `tools/validate_tasks_tree.py` once Python is available.
8. Treat T012 as verification-only or regenerate tasks if it is no longer needed.

## 18. Final readiness assessment

| Question | Answer |
|---|---|
| Are the specifications consistent with the Constitution? | Mostly yes, except decision-record path governance ambiguity and RBFV policy wording must be corrected. |
| Are the active specifications consistent with each other? | No. RBFV conflicts with use cases/contracts on `canResolveRequests` policy wording and HR calendar route. |
| Are the use cases consistent with the specifications? | Mostly yes; UC-19 route conflicts with RBFV shared-route table. |
| Are the contracts consistent with the use cases? | Yes for core behavior; both require HR calendar `/rrhh/calendario` and Approver `canResolveRequests`. |
| Are roles and responsibilities consistently defined? | Mostly yes; Approver capability is inconsistent in RBFV policies. |
| Are state transitions consistently defined? | Yes. |
| Are business rules completely traceable? | No. BR-036, BR-037, BR-038 have partial generated task/coverage traceability. |
| Are acceptance criteria complete and testable? | Mostly yes; generated report category totals are stale/unclear. |
| Are the tasks consistent with the Constitution? | Mostly yes; semantic corrections required before execution. |
| Are the tasks consistent with all specifications? | No, because tasks inherit HR calendar ambiguity and projected-balance traceability gaps. |
| Do the tasks cover all 22 active use cases? | Structurally yes; semantically UC-19 is conflicted until route decision is resolved. |
| Do the tasks cover all active requirements? | Mostly; FR-023 is partial due BR-036/037/038 task explicitness. |
| Do the tasks cover all active acceptance criteria? | Mostly; route and projected-balance criteria need correction. |
| Do the tasks cover all active business rules? | No; BR-036/037/038 are partial. |
| Do the tasks introduce unauthorized scope? | No unauthorized scope found. |
| Are there responsibility conflicts? | Yes: RBFV route authorization vs Application/use-case capability rules. |
| Are there implementation gaps? | Documentation-level gaps remain; no implementation exists yet. |
| Are there test gaps? | Planned test tasks exist, but projected-balance approval revalidation must be explicit. |
| Are there unresolved decisions? | Yes, two documentary governance/route decisions. |
| Can implementation begin without making assumptions? | No. Implementation would require assumptions about HR calendar routing and Approver capability enforcement in route policies. |

## 19. Final verdict

`NOT READY`

NovaLeave’s core business model is well specified and structurally traceable, with 153 canonical tasks and 153 generated task files. However, implementation should not begin until the HIGH and MEDIUM documentary defects are corrected. The key evidence is: RBFV route policies omit `canResolveRequests` despite use cases/contracts requiring it; HR calendar route authority conflicts between `/calendario` and `/rrhh/calendario`; and the coverage report excludes active `BR-036` through `BR-038` while claiming 100% business-rule coverage. Structural validation was not treated as semantic validation, and the Python validator could not be executed in this environment.
