# Database Migration Runbook

## Scope

This runbook governs manual validation of EF Core SQL Server migrations. It does not define automated migration deployment.

## Pre-Migration Checks

- Confirm source revision and migration files under the approved Infrastructure project.
- Confirm backup evidence is current.
- Review migration SQL for destructive operations.
- Confirm configuration and connection string handling expose no secrets in logs or evidence.

## Validation Procedure

1. Generate or inspect the migration script using environment-compatible tooling.
2. Apply the migration to a local or test database.
3. Verify schema version and required indexes, constraints, and rowversion columns.
4. Run integration tests that cover persistence, concurrency, balances, and audit records.
5. Run invariant checks for balances, lifecycle state, idempotency, and duplicate movements.
6. Record commands, results, failures, reviewer, and exceptions.

## Rollback Planning

Rollback steps are environment-specific and must be documented before production use. If rollback is not validated, record it as an accepted operational risk before acceptance.
