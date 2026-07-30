# NovaLeave Operations

## Authority

These runbooks are active operational documentation for the NovaLeave MVP. They implement the manual operations obligations in Constitution v7.0.0, the primary MVP specification, and ADR DR-003.

The constitution, approved specifications, and ADRs remain higher authority. These documents must not introduce product behavior, roles, routes, lifecycle states, balance semantics, or deployment automation.

## Manual Gate

Before merge, handoff, release, or acceptance, the Manual Quality and Security Gate in `manual-quality-gate.md` is blocking. CI/CD and automated deployment pipelines are outside the MVP delivery model.

## Runbooks

- `manual-quality-gate.md`: required manual validation evidence.
- `deployment-runbook.md`: configurable deployment and readiness procedure.
- `incident-response-runbook.md`: incident triage and recovery coordination.
- `backup-and-restore-runbook.md`: backup, restore, verification, and evidence.
- `database-migration-runbook.md`: migration validation and rollback planning.
- `scheduled-jobs-runbook.md`: accrual and timeout job operation.
- `observability-runbook.md`: logs, metrics, traces, health checks, alerts, invariant monitoring, and redaction inspection.

## Ownership

The implementer records command evidence. The reviewer verifies evidence, exceptions, and traceability before acceptance. Any exception must include rationale, approval, owner, mitigation, and expiration or review date.
