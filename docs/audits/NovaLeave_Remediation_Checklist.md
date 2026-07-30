# NovaLeave Remediation Checklist

**Repository**: `full-stack-dev-johncastrosanabria/NovaLeave`  
**Branch**: `abraham-villalobos`  
**Baseline SHA**: `c2a3282995e72c9ef44a574e2c9a4c58cbf3302f`  
**Created**: 2026-07-30

| Finding ID | Severity | Verification status | Affected authoritative sources | Required decision | Required correction | Dependent generated artifacts | Validation method |
|---|---|---|---|---|---|---|---|
| F-HIGH-001 | HIGH | CONFIRMED | Constitution, RBFV spec, use cases, UC contracts, canonical tasks | None; rule supplied in remediation request | Require authenticated active Approver with `canResolveRequests=true`, non-owner, eligible state, and execution-time Application revalidation for approval queues, details, and resolution operations | TASK-017, TASK-079, TASK-080, TASK-086, TASK-088, TASK-090, BACKLOG, coverage report | Search for policy wording, disabled-Approver tests, `canResolveRequests=true` in affected tasks/contracts |
| F-HIGH-002 | HIGH | CONFIRMED | RBFV spec, spec 001, use cases, UC contracts, plan, canonical tasks | User selected `/rrhh/calendario` only | Make HR calendar a dedicated read-only global route for all vacation requests; keep `/calendario` limited to User and eligible Approver scopes; deny HR `/calendario` behavior | TASK-118, TASK-122, TASK-124, TASK-130, TASK-134, BACKLOG, coverage report | Search route table/contracts/tasks; verify no HR authorization on `/calendario`; link route to UC-19/AC-HR-003 |
| F-MED-001 | MEDIUM | CONFIRMED | Spec 001, use cases, canonical tasks, coverage report | None | Recalculate active BR count as 36 and explicitly trace BR-036 through BR-038 | TASK-079, TASK-080, TASK-086, TASK-088, coverage report | Identifier extraction and generated task grep for BR-036/BR-037/BR-038 |
| F-MED-002 | MEDIUM | CONFIRMED | Spec 001, UC-10, UC-11, canonical tasks | None | Distinguish projected-balance display/query behavior from approval POST revalidation, negative rejection, stale/concurrent conflict, and transaction behavior | TASK-079, TASK-080, TASK-086, TASK-088, coverage report | Task semantic review and grep for BR-037/BR-038 in approval test/command tasks |
| F-MED-003 | MEDIUM | CONFIRMED | Constitution, DR index, DR-001, DR-002, plan, research, quickstart, canonical tasks | User selected `docs/adr/` | Move accepted decision records to `docs/adr/`, update references, and remove competing normative copies from `docs/decisions/` | TASK-019, TASK-099, TASK-114, coverage report | File inventory, `docs/decisions` reference scan, relative-link scan |
| F-LOW-001 | LOW | CONFIRMED | plan, data-model | None | Normalize active diagram links to `specs/001-leave-management-mvp/diagrams/` | coverage report | Case-sensitive path validation and `Diagrams/` grep |
| F-LOW-002 | LOW | CONFIRMED | coverage report | None | Rebuild `tasks/coverage-report.md` only after canonical documents and tasks are corrected | coverage report | Compare active identifier inventory to report totals |
| F-LOW-003 | LOW | CONFIRMED | AGENTS.md, canonical tasks, TASK-012 | None | Treat T012 as verification-only because AGENTS.md already points to `plan.md` | TASK-012, BACKLOG | Search AGENTS.md and TASK-012 canonical text |
| F-INFO-001 | INFO | NO LONGER APPLICABLE | Historical audits only | None | No active correction required; superseded by post-remediation audit | Post-remediation audit | Confirm historical files are not used as active authority |
| F-INFO-002 | INFO | ALREADY RESOLVED | Active task tree | None | No task-count change required; T001-T153 and TASK-001-TASK-153 remain one-to-one | coverage report | Task count and round-trip validation |
