# DR-001 — Runtime Configuration Values

**Status**: Accepted
**Date**: 2026-07-29
**Deciders**: Product owner proxy (repository owner), recorded during the A-grade remediation session
**Related**: CFG-001, CFG-002 (spec 001 §Configuration Requirements); `quickstart.md` Environment Configuration; `research.md`; canonical tasks T019, T099, T114; audits AUD-001–AUD-003 (docs/audits)

## Context

The planning audits correctly removed a previously invented 7-day timeout default and marked
`PendingRequestTimeoutDays`, `SessionTimeoutMinutes`, and the accrual scheduler cadence as
`NEEDS CONFIGURATION` with no invented values (see `docs/audits/`). That preserved governance
integrity but left three startup blockers: the application is specified to fail fast when the two
required values are missing, and the accrual job could not be scheduled at all.

This record resolves the blockers with **explicitly decided product values** — not invented
defaults. The fail-fast validation architecture is unchanged.

## Decision

| Key | Official value | Notes |
|-----|----------------|-------|
| `NovaLeave:PendingRequestTimeoutDays` | **14** | Two-week approval SLA. A `Pending` request unresolved for 14 days transitions to `CancelledByTimeout` and releases its reservation. |
| `NovaLeave:SessionTimeoutMinutes` | **30** | Standard idle timeout; aligns with the constitution's session-lifetime configuration and testing requirements (§7.1). |
| Accrual job cadence | **Daily at 00:05 UTC** | Engineering decision. The business rule (1 day per completed month) is unchanged; the job is an idempotent scan protected by the unique `(UserId, AccrualPeriod)` constraint, so a daily run is self-healing — a missed execution is corrected on the next run. |
| Timeout job cadence | **Daily at 00:05 UTC** | Engineering decision, same self-healing rationale. The business threshold is the 14-day value above; scan frequency only affects detection latency (≤ 24 h), acceptable for a days-granularity rule. |

## Consequences

- Configuration remains **required and validated at startup** in every environment; no value is
  hard-coded in application code. These are the official values to place in environment
  configuration (Development/Staging may ship them in `appsettings.Development.json`; Production
  supplies them through its secret/configuration store).
- `NEEDS CONFIGURATION` markers that referred to these three items are resolved and now point to
  this record. Changing a value later is a configuration change, not a specification change.
- The scheduler adapters (canonical T099, T114) read cadence from configuration with no code
  default, exactly as planned; this record supplies the value the configuration carries.
