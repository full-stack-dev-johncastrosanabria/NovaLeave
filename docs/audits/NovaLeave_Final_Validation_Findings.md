# NovaLeave Final Validation Findings

**Audit date**: 2026-07-30  
**Repository**: `full-stack-dev-johncastrosanabria/NovaLeave`  
**Branch**: `abraham-villalobos`  
**Baseline SHA**: `c2a3282995e72c9ef44a574e2c9a4c58cbf3302f`  
**Scope**: Final validation, governance/documentation correction, task synchronization, coverage recalculation, audit readiness, commit, and push preparation.

## Validator Inventory

| Validator | Command | Purpose | Runtime | Mandatory | Available | Status |
|---|---|---|---|---|---|---|
| Git state inspection | `git rev-parse --show-toplevel`; `git branch --show-current`; `git rev-parse HEAD`; `git status --short`; `git diff --stat`; `git diff --name-status`; `git diff`; `git diff --cached`; `git remote -v`; `git branch -vv` | Baseline, branch, diff, staged state, remote/upstream status | Git | Yes | Yes | PASSED |
| Task tree validator | `python tools\validate_tasks_tree.py`; `python3 tools\validate_tasks_tree.py`; `py -3 tools\validate_tasks_tree.py` | DR-002 task-tree invariant validation | Python 3 | Yes | No | NOT EXECUTED |
| Task tree equivalent | PowerShell equivalent of `tools/validate_tasks_tree.py` assertions | Verify canonical/generated round-trip, FR traceability floor, template artifacts, BOM | PowerShell 5.1 | Yes, as safe equivalent | Yes | PASSED after correction |
| Spec Kit prerequisites | `bash .specify/scripts/bash/check-prerequisites.sh --json --require-tasks --include-tasks` | Verify active feature documents and task availability | Bash | Yes when Bash is available | No | NOT EXECUTED |
| Validator discovery | `rg --files`; `rg -n "validate|validation|lint|test|check-prerequisites|coverage|..."` | Discover repository validators and validation instructions | ripgrep | Yes | Yes | PASSED |
| Runtime inventory | `git --version`; `python --version`; `python3 --version`; `py -3 --version`; `bash --version`; `pwsh --version`; `powershell -Command "(Get-Host).Version"`; `rg --version`; `dotnet --version`; `node --version`; `npm --version` | Determine executable validation environment | Shell/runtime commands | Yes | Partial | PASSED/NOT EXECUTED by runtime |
| Inventory extraction | PowerShell regex extraction from primary sources | Recalculate FR, BR, AC, AC-HR, RBFV, UC, SEC, AUTHZ, VAL, AUD, CON, task, generated-task, and test-task counts | PowerShell 5.1 | Yes | Yes | PASSED |
| Dependency validation | PowerShell canonical task dependency scan | Detect missing dependency references and cycles | PowerShell 5.1 | Yes | Yes | PASSED |
| Markdown relative-link validation | PowerShell Markdown link resolver | Detect broken active relative links | PowerShell 5.1 | Yes | Yes | PASSED |
| Case-sensitive path validation | Targeted `Test-Path` checks for ADR and diagram references | Validate remediated path casing | PowerShell 5.1 | Yes | Yes | PASSED |
| Governance scans | `rg` for `docs/decisions`, ADR paths, Constitution version, Sync Impact Report | Detect stale active decision-record references and governance conflicts | ripgrep | Yes | Yes | PASSED after correction |
| Route/AuthZ/traceability scans | `rg` for `/rrhh/calendario`, `/calendario`, `canResolveRequests`, `FR-023`, `BR-036`-`BR-038` | Detect semantic contradictions and traceability gaps | ripgrep | Yes | Yes | PASSED |
| Template and BOM scans | `rg` plus PowerShell byte-prefix scan | Detect generated placeholders and UTF-8 BOMs | ripgrep/PowerShell | Yes | Yes | PASSED after correction |

## New Findings

### FV-001

| Field | Value |
|---|---|
| Severity | MEDIUM |
| Status | RESOLVED |
| Evidence | `tools/validate_tasks_tree.py` still referenced `docs/decisions/` in its docstring, artifact scan, and allowed-citation path after the approved ADR migration to `docs/adr/`. Its active-FR extraction depended on a mojibake em dash sequence, which would undercount active FRs. |
| Root cause | The validator was not updated when active decision records moved from `docs/decisions/` to `docs/adr/`; the FR parser had a stale encoding-sensitive pattern. |
| Affected paths and identifiers | `tools/validate_tasks_tree.py`; DR-002; active FR traceability floor. |
| Correction | Updated the validator to scan `docs/adr/*.md`, allow `docs/adr/DR-002-generated-artifact-review-rule.md`, update documentation references, and extract active FR IDs using the stable bold `FR-XXX` identifier pattern. |
| Revalidation result | Python runtime unavailable, so the corrected validator could not execute directly. A PowerShell equivalent of the validator assertions passed: 153 TASK files, 153 canonical tasks, 23 active FRs all referenced, no template artifacts, no BOM. |

### FV-002

| Field | Value |
|---|---|
| Severity | MEDIUM |
| Status | RESOLVED |
| Evidence | PowerShell equivalent of `tools/validate_tasks_tree.py` reported BOMs in `tasks/BACKLOG.md` and `tasks/EPIC-000-general-system/EPIC-000.md`, plus generated TASK objective mismatches for T012, T086, T088, T090, T118, T122, and T124. |
| Root cause | Generated task synchronization had updated selected generated files semantically but did not preserve exact canonical objective round-trip text for all affected tasks. Two generated/index artifacts were written with UTF-8 BOM. |
| Affected paths and identifiers | `tasks/BACKLOG.md`; `tasks/EPIC-000-general-system/EPIC-000.md`; `TASK-012`; `TASK-086`; `TASK-088`; `TASK-090`; `TASK-118`; `TASK-122`; `TASK-124`. |
| Correction | Rewrote generated TASK objective lines from canonical `specs/001-leave-management-mvp/tasks.md`; rewrote generated/index task artifacts as UTF-8 without BOM; corrected a duplicated TASK-090 title phrase. |
| Revalidation result | PASSED: 153 TASK files round-trip against 153 canonical tasks; 23 active FRs all referenced; no template artifacts; no BOM. |

### FV-003

| Field | Value |
|---|---|
| Severity | LOW |
| Status | RESOLVED |
| Evidence | Canonical task extraction found 52 explicit `Add ... tests` tasks, while `tasks/coverage-report.md` reported 51 required tests and used a coverage table that did not include Partial/Missing/Conflict columns required by the final validation workflow. |
| Root cause | Coverage metadata was recalculated before the final validation pass and retained the previous required-test count/table shape. |
| Affected paths and identifiers | `tasks/coverage-report.md`; `docs/audits/NovaLeave_Post_Remediation_Audit.md`; required-test inventory. |
| Correction | Updated required tests from 51 to 52 and replaced the coverage results table with `Active total`, `Covered`, `Partial`, `Missing`, `Conflict`, and `Coverage` columns. Updated the post-remediation audit count and linked this final findings report. |
| Revalidation result | PASSED: canonical explicit test-task extraction returns 52; coverage report now records 52 required tests and the requested coverage table format. |

## Remaining Issues

No repository defects remain open.

Unavailable validator runtimes remain an environment limitation:

- Python is unavailable; `python`, `python3`, and `py -3` cannot execute `tools/validate_tasks_tree.py`.
- Bash is unavailable; `.specify/scripts/bash/check-prerequisites.sh` cannot execute.

Equivalent PowerShell validations passed where practical. Commit and push require explicit approval to proceed without the unavailable Python and Bash validators.

## Commands Executed

- `git rev-parse --show-toplevel`
- `git branch --show-current`
- `git rev-parse HEAD`
- `git status --short`
- `git diff --stat`
- `git diff --name-status`
- `git diff`
- `git diff --cached`
- `git remote -v`
- `git branch -vv`
- `rg --files tools .specify .github specs tasks docs AGENTS.md README.md package.json package-lock.json pnpm-lock.yaml yarn.lock Makefile makefile`
- `rg -n "validate|validation|lint|test|check-prerequisites|coverage|..."`
- `git --version`
- `python --version`
- `python3 --version`
- `py -3 --version`
- `bash --version`
- `pwsh --version`
- `powershell -NoProfile -Command "(Get-Host).Version"`
- `rg --version`
- `dotnet --version`
- `node --version`
- `npm --version`
- `python tools\validate_tasks_tree.py`
- `python3 tools\validate_tasks_tree.py`
- `py -3 tools\validate_tasks_tree.py`
- `bash .specify/scripts/bash/check-prerequisites.sh --json --require-tasks --include-tasks`
- PowerShell task-tree equivalent validation
- PowerShell dependency validation
- PowerShell Markdown relative-link validation
- PowerShell case-sensitive ADR/diagram path validation
- PowerShell inventory extraction
- `rg` governance, route, authorization, projected-balance, template, and casing scans

## Commands Not Executed And Why

No discovered validator was silently skipped.

The Python task-tree validator and Bash Spec Kit prerequisite validator were attempted and are recorded as `NOT EXECUTED` because their runtimes are unavailable. No installation was performed.

No `dotnet test`, `npm test`, `npm run lint`, or CI workflow command was applicable because the current repository contains no `.sln`, project source tree, package manifest, or workflow job defining such commands. The MVP is still in planning-package form and implementation source code has not begun.

## Revalidation Results

- Task-tree equivalent: PASSED.
- Canonical task count: 153.
- Generated task count: 153.
- Canonical/generated round-trip: PASSED.
- FR traceability floor: PASSED, 23 active FRs all referenced by generated TASK files.
- Duplicate task IDs: PASSED, 0 duplicates.
- Missing task IDs: PASSED, 0 missing from T001-T153.
- Extra generated task files: PASSED, 0 extra.
- Dependency references: PASSED, 0 missing.
- Dependency cycles: PASSED, 0 cycles.
- Relative links: PASSED, 0 broken active Markdown links.
- ADR path validation: PASSED, active governance uses `docs/adr/`.
- Diagram path casing: PASSED for `specs/001-leave-management-mvp/diagrams/`.
- Template-artifact scan: PASSED for generated artifacts.
- BOM scan: PASSED after correction.
- Authorization consistency: PASSED.
- HR route/read-only consistency: PASSED.
- Projected-balance traceability: PASSED.
- Coverage recalculation: PASSED.

## Final Readiness Conclusion

**READY WITH APPROVED VALIDATION LIMITATION REQUIRED**

The repository planning package has no unresolved BLOCKER, CRITICAL, HIGH, MEDIUM, or LOW repository defects after correction and revalidation. The only gate remaining before commit/push is explicit user approval to proceed without executing the unavailable Python and Bash validators in this environment, relying on the documented PowerShell equivalent checks instead.
