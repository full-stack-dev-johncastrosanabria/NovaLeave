# Deployment Runbook

## Authority

This runbook documents manual deployment readiness for the MVP. It does not create CI/CD, deployment automation, release automation, or production infrastructure.

## Preconditions

- Constitution v7.0.0 Manual Quality and Security Gate evidence is complete.
- Required configuration values are provided by the target environment.
- Database migration plan has been validated.
- Backup and restore procedure has current evidence.

## Manual Sequence

1. Confirm the exact source revision and approved task scope.
2. Restore dependencies and compile the solution.
3. Validate configuration and secrets handling for the target environment.
4. Apply database migration steps using environment-specific commands.
5. Start the application using environment-approved hosting.
6. Verify liveness, readiness, and database connectivity health checks.
7. Run smoke checks for critical User, Approver, and HR routes.
8. Record commands, results, reviewer, and exceptions.

## Load and Concurrency Readiness

Use a local or test environment; production deployment is not required for implementation readiness.

- Representative dataset: active Users, active Approvers with and without `canResolveRequests`, HR users, Pending requests, Approved requests, balance movements, and audit records.
- Concurrent request scenario: multiple Users create and edit Pending vacation requests against available balances.
- Approval contention scenario: multiple eligible Approvers attempt to resolve the same Pending request.
- Scheduled job scenario: accrual and timeout jobs execute with duplicate-run attempts.
- Expected measurements: request count, latency, error count, concurrency conflict count, accrual success/failure, timeout success/failure, and invariant violation count.
- Evidence: dataset description, command or procedure, timestamp, measured results, failures, and reviewer identity.

## MVP Limitations

Numeric RPO and RTO targets are governed by the constitution and must be validated against the selected environment before production acceptance. Environment-specific commands remain configurable.
