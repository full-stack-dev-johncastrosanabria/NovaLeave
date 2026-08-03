# Scheduled Jobs Runbook

## Jobs

- Monthly accrual job.
- Pending request timeout job.

## Required Configuration

Job cadence values are explicit configuration. Missing or invalid values must fail startup or prevent job registration according to the approved plan and ADRs.

Approved DR-001 values:

- Monthly accrual job: `Daily 00:05 UTC`.
- Pending request timeout job: `Daily 00:05 UTC`.
- Pending timeout threshold: `14` days.

## Operation Procedure

1. Confirm environment configuration and source revision.
2. Verify job registration state.
3. Execute or observe the job in a local or test environment.
4. Confirm idempotency by repeating the job where safe.
5. Inspect structured logs, metrics, traces, and audit records.
6. Confirm no duplicate balance movement is created for one operation.
7. Record success, failure, skipped execution, reviewer, and evidence.

## Verification Matrix

| Job | Verification |
| --- | --- |
| Monthly accrual | Eligible users accrue one day per fully completed calendar month; duplicate runs do not duplicate movements |
| Timeout cancellation | Pending requests older than the configured threshold transition once and release reservations |
| Both jobs | Structured logs, metrics, traces, audit records, idempotency, and invariant checks remain clean |

## Evidence Requirements

Record source SHA, system date/reference date, configuration values, executed
command or observation method, affected user/request counts, idempotency result,
audit result, metrics/log result, failures, and reviewer identity.

## Failure Handling

Accrual job failure, timeout job failure, repeated idempotency failures, and invariant violations are alertable conditions. Operators must preserve evidence and investigate before retrying in production-like data.
