# NovaLeave Operations

## Authority

These runbooks are active operational documentation for the NovaLeave MVP. They
implement Constitution v7.0.0, ADR DR-001, and ADR DR-003 obligations without
adding CI/CD, deployment automation, release automation, external schedulers,
queues, Redis, email, or paid observability requirements.

The constitution, approved specifications, and ADRs remain higher authority.
These documents must not introduce product behavior, roles, routes, lifecycle
states, balance semantics, or hidden operational repair behavior.

## Ownership Model

| Responsibility | Owner | Evidence |
| --- | --- | --- |
| Execute implementation validation | Implementer | Commands and results in the phase evidence file |
| Review gate completeness | Reviewer | Manual Quality and Security Gate sign-off |
| Approve exceptions | Product owner or delegated reviewer | Written exception with owner, mitigation, and review date |
| Operate database backup/restore | Environment database owner | Backup, restore, and verification evidence |
| Investigate alerts/incidents | Operator plus reviewer when acceptance is affected | Incident record and linked logs/metrics/traces |

## Required Runbooks

- `manual-quality-gate.md`: blocking manual validation procedure and evidence
  checklist.
- `deployment-runbook.md`: manual deployment readiness and smoke checks.
- `incident-response-runbook.md`: incident triage, evidence preservation, and
  recovery coordination.
- `backup-and-restore-runbook.md`: SQL Server backup, restore, verification,
  responsible role, limitations, and evidence.
- `database-migration-runbook.md`: migration validation and rollback planning.
- `scheduled-jobs-runbook.md`: monthly accrual and timeout job operation.
- `observability-runbook.md`: correlation, logs, metrics, traces, health,
  alerts, redaction, invariants, and load/concurrency readiness.
- `phase-12-operations-evidence.md`: Phase 12 implementation evidence.

## Acceptance Rules

- The Manual Quality and Security Gate is blocking before merge, handoff,
  release, or acceptance.
- Unavailable tooling is recorded as `NOT EXECUTED`, never as `PASS`.
- CI/CD and automated deployment pipeline configuration remain outside the MVP.
- Operational evidence must not include secrets, connection strings, tokens,
  passwords, sensitive reasons, or internal exception details.
