# Observability Runbook

## Scope

NovaLeave MVP observability uses technology compatible with the approved .NET architecture and must not require paid external services.

## Signals

- Structured logs with request correlation ID.
- Metrics for request count, request latency, error count, vacation request creation success/failure, approval success/failure, rejection success/failure, timeout cancellation count, accrual execution success/failure, concurrency conflict count, and business-invariant violation count.
- Application tracing for incoming request, application operation, persistence action, and error handling.
- Health checks for application liveness, readiness, and database connectivity.

## Invariant Monitoring

Evaluate invariants during relevant application operations, scheduled jobs, and operational verification:

- Available balance never becomes negative.
- Reserved balance matches active Pending requests.
- Deducted balance matches applicable Approved requests.
- Final-state requests are not mutated.
- Self-approval never succeeds.
- HR never performs request-resolution operations.
- Disabled Approvers cannot access or execute approval workflows.
- Accrual is idempotent.
- Timeout processing is idempotent.
- Audit records exist for required successful mutations.
- No duplicate balance movement is created for one operation.

Invariant checks emit structured events and metrics. Any violation is alertable and requires investigation. Checks must not silently mutate production data or perform self-healing.

## Alert Definitions

| Condition | Evidence | Response |
| --- | --- | --- |
| Repeated application errors | Error count and correlated logs | Inspect request path, recent changes, and safe error responses |
| Database health failure | Readiness/database health check failure | Validate configuration, connectivity, migration state, and backup status |
| Accrual job failure | Accrual failure metric/event | Inspect job config, idempotency key, balance movements, and audit records |
| Timeout job failure | Timeout failure metric/event | Inspect job config, Pending request age, releases, and audit records |
| Persistent concurrency failures | Concurrency conflict count | Inspect contention scenario and rowversion handling |
| Failed invariant checks | Business-invariant violation count/event | Preserve data snapshot and investigate before retrying mutations |
| Abnormal authn/authz denial patterns | Denial metrics/events | Inspect role state, `canResolveRequests`, forced browsing, and IDOR evidence |

Thresholds, ownership, and delivery channel are environment-configured. If no managed monitoring platform is available, operators must review recorded metrics/logs manually and document evidence.

## Redaction Inspection

Inspect logs, traces, metrics dimensions and labels, error payloads, audit payloads, and health-check responses. They must not expose passwords, authentication tokens, connection strings, secrets, full rejection reasons where prohibited, sensitive free-text request content, internal exception details in user-facing responses, or unapproved personal data.

Automated assertions are required where feasible. Manual inspection evidence is required for outputs that cannot be fully asserted automatically.
