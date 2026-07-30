# Incident Response Runbook

## Scope

This runbook covers manual incident response for the NovaLeave MVP. It does not define automated paging, managed monitoring services, or deployment automation.

## Responsibilities

- Implementer or operator detects and records the incident.
- Reviewer or product owner approves any exception that affects acceptance.
- Database owner performs or validates restore actions when needed.

## Response Sequence

1. Record incident time, reporter, affected environment, and source revision.
2. Classify impact: authentication, authorization, request lifecycle, balance, scheduled job, database, configuration, or observability.
3. Preserve evidence from logs, metrics, traces, health checks, audit records, and database state.
4. Check for sensitive data exposure in captured evidence before sharing.
5. Stop unsafe manual operation if it risks further data mutation.
6. Use the backup and restore runbook when database recovery is required.
7. Use the database migration runbook when schema state is involved.
8. Validate recovery with health checks, targeted tests, and invariant checks.
9. Record final status, root cause notes, remediation tasks, and approvals.

## Disaster Recovery

Recovery sequence:

1. Confirm source revision and configuration required for the environment.
2. Restore database from an approved backup if required.
3. Reapply or validate application configuration and secrets.
4. Validate migrations, scheduled jobs, and health checks.
5. Run critical route smoke checks and invariant monitoring checks.
6. Record evidence and unresolved limitations.

No new RPO or RTO is introduced here. If the approved targets cannot be validated in a selected environment, record the gap as an operational decision before acceptance.
