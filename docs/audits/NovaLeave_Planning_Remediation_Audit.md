# NovaLeave Planning Artifact Audit

**Status**: Current recovery note. Derived, not authoritative.
**Scope**: Documentation and planning artifacts only.

## Result

All approved planning artifacts outside the constitution have been normalized to the confirmed MVP baseline. The repository has no `src/` and no `tests/`; therefore implementation is `NOT STARTED`, tests are `PLANNED, NOT IMPLEMENTED`, security controls are `PLANNED, NOT VERIFIED`, and executable verification is `NOT VERIFIABLE`.
Generated `specs/001-leave-management-mvp/tasks.md` now exists as planning output only. It contains 123 unchecked implementation tasks and does not constitute implementation evidence.

## Corrected Conflicts

- Lifecycle normalized to five states and four official transitions.
- OQ-001 normalized to: RESOLVED - User cancellation of a Pending request is OUT OF MVP SCOPE.
- OQ-002 normalized to completed-calendar-month accrual from `EmploymentStartDate` with the two approved examples.
- UC-01 through UC-22 mappings rebuilt without task IDs as evidence.
- HR permissions limited to organization-wide read access and `canResolveRequests` management for existing Approvers.
- Removed active MVP dependence on teams, departments, managers, hierarchy, delegation, escalation, public APIs, JWT, OpenAPI, email, Outbox, queues, Redis, and microservices.
- `Vacation` simplified to a Domain enum or constant; no persisted `LeaveType` lookup remains planned.
- Configuration defaults and cron schedules were not invented.

## Requirement Totals

| Prefix | Count |
|---|---:|
| UC | 22 |
| FR | 25 |
| VAL | 9 |
| BR | 38 |
| AUTHZ | 19 |
| SEC | 9 |
| AUD | 10 |
| CON | 12 |
| CFG | 4 |
| ERR | 6 |
| RBFV | 34 |
| AC | 60 |
| AC-HR | 12 |
| SC | 12 |
| OQ | 5 |
| PD | 16 |

## Remaining NEEDS CONFIGURATION

- `NovaLeave:PendingRequestTimeoutDays`
- `NovaLeave:SessionTimeoutMinutes`
- accrual scheduler cadence

## Constitution Check

Design-level result: PASS against `.specify/memory/constitution.md` v6.0.0 after restoring the approved constitution from `origin/main`. This is a documentation/planning check only; implementation is `NOT STARTED`, tests are `PLANNED, NOT IMPLEMENTED`, security controls are `PLANNED, NOT VERIFIED`, and executable validation is `NOT VERIFIABLE`.

## Verdict

TASK-READY

TASKS GATE: PASSED BY MANUAL SPEC KIT REVIEW; official bash wrappers were unavailable because `/bin/bash` is not installed in this environment.
