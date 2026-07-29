# STALE - NOT AUTHORITATIVE

This historical question log is superseded by the approved MVP artifacts. It must not be used as a requirement source, planning blocker, implementation guide, or task source.

Current resolved questions:

- OQ-001: RESOLVED - User cancellation of a Pending request is OUT OF MVP SCOPE.
- OQ-002: one whole vacation day per fully completed calendar month from `EmploymentStartDate`; first partial calendar month does not accrue; no proration; accrued days do not expire; idempotent by `(UserId, AccrualPeriod)`; scheduler catch-up may process missed eligible periods exactly once.
- OQ-003: inactive identities cannot execute protected workflows.
- OQ-004: mandatory reason for Approver deactivation is out of MVP scope.
- OQ-005: User calendar is owner-only; HR calendar is organization-wide read-only.

Normative examples for OQ-002:

- `EmploymentStartDate = 2026-03-15`: March is partial and does not accrue; April accrual becomes eligible on `2026-05-01`; `AccrualPeriod = 2026-04`.
- `EmploymentStartDate = 2026-03-01`: March is fully completed; March accrual becomes eligible on `2026-04-01`; `AccrualPeriod = 2026-03`.
