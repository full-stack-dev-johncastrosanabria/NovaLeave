# Backup and Restore Runbook

## Assumptions

- SQL Server is the approved persistence technology.
- Backup storage, retention, encryption, and exact commands are environment-specific.
- This runbook does not create infrastructure or promise capabilities unavailable in the target environment.

## Backup Procedure

1. Identify environment, database name, source revision, and responsible role.
2. Confirm no unsafe migration or manual data operation is in progress.
3. Execute the environment-approved SQL Server backup command.
4. Store the backup according to environment-approved retention and access controls.
5. Record backup timestamp, command, operator, storage location reference, and result.

## Configurable Command Placeholders

Exact command syntax depends on the selected SQL Server hosting model. Record
the environment-approved command without exposing secrets.

| Environment Type | Backup Command Reference | Restore Command Reference |
| --- | --- | --- |
| Local SQL Server | Environment-defined `BACKUP DATABASE` command | Environment-defined `RESTORE DATABASE` command |
| Azure SQL | Environment-defined portal, CLI, or managed backup operation | Environment-defined point-in-time restore operation |
| Managed DBA process | Ticket or change reference | Ticket or change reference |

## Restore Procedure

1. Select the backup by timestamp and environment.
2. Confirm restore approval and expected target database.
3. Restore using environment-approved SQL Server commands.
4. Apply any required migration validation from `database-migration-runbook.md`.
5. Restart or reconnect the application as required by the environment.

## Restore Verification

- Verify database connectivity health check.
- Verify expected schema version.
- Run balance and request lifecycle invariant checks.
- Validate critical User, Approver, and HR read paths.
- Record failures, exceptions, reviewer, and evidence location.

## Evidence Requirements

- Environment and database identifier.
- Responsible operator and reviewer.
- Backup timestamp and storage reference.
- Restore target and approval.
- Health/readiness result.
- Migration validation result.
- Invariant monitoring result.
- Known limitations and accepted risks.

## Recovery Limitations

Actual recovery capability depends on configured backup frequency, retention, storage durability, and environment access. Unvalidated capabilities must be recorded as operational risks.
