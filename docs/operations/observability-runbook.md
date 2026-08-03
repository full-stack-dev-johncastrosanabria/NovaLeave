# Observability Runbook

## Scope

NovaLeave MVP observability uses technology compatible with the approved .NET architecture and must not require paid external services.

## Signals

- Structured logs with `correlation_id` and `request_id`.
- Metrics for request count, request latency, error count, vacation request
  creation success/failure, approval success/failure, rejection success/failure,
  timeout cancellation count, accrual execution success/failure, concurrency
  conflict count, business-invariant violation count, and alertable events.
- Application tracing for incoming request, application operation, persistence
  action context, and error handling context.
- Health checks:
  - `/health/live`: application process liveness.
  - `/health/ready`: readiness including database connectivity.

## Correlation

Incoming requests may provide `X-Correlation-ID`. If absent, the application
generates one and returns it in the same response header. Operators must use this
value to join request logs, traces, health observations, and user-facing safe
error responses. Correlation values must not contain secrets or free-text
business reasons.

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

Thresholds, ownership, and delivery channel are environment-configured. If no
managed monitoring platform is available, operators must review recorded
metrics/logs manually and document evidence.

## Redaction Inspection

Inspect logs, traces, metrics dimensions and labels, error payloads, audit
payloads, and health-check responses. They must not expose passwords,
authentication tokens, connection strings, secrets, full rejection reasons where
prohibited, sensitive free-text request content, internal exception details in
user-facing responses, or unapproved personal data.

Procedure:

1. Trigger representative User, Approver, HR, scheduled-job, health, and error
   paths in a local or test environment.
2. Capture logs, traces, metrics labels, audit payloads, health responses, and
   user-facing errors.
3. Search captured output for passwords, tokens, connection strings, secrets,
   request reasons, rejection reasons, stack traces, and unapproved personal
   data.
4. Record PASS, FAIL, or NOT EXECUTED per output type.
5. Treat any unredacted sensitive value as a blocking finding.

Automated assertions are required where feasible. Manual inspection evidence is
required for outputs that cannot be fully asserted automatically.

## Load and Concurrency Readiness

Use a local or test environment; production deployment is not required.

- Representative dataset: active Users, active Approvers with and without
  `canResolveRequests`, active HR, Pending requests, Approved requests, balance
  movements, audit records, and users eligible for monthly accrual.
- Concurrent request scenario: multiple Users create or edit Pending vacation
  requests against available balances.
- Approval contention scenario: multiple eligible Approvers attempt to resolve
  the same Pending request.
- Scheduled job scenario: monthly accrual and timeout processing are executed
  more than once to verify idempotency.
- Expected measurements: request count, latency, error count, concurrency
  conflict count, accrual success/failure, timeout success/failure, and
  invariant violation count.
- Evidence: dataset description, commands or procedures, timestamp, measured
  results, failures, reviewer identity, and accepted limitations.
