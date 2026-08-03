# Phase 12 Operations Evidence

**Date**: 2026-08-03  
**Branch**: `001-phase-12-operations`  
**Scope**: Canonical tasks T154-T178 only.  
**Reviewer identity**: Local implementer evidence; reviewer sign-off pending PR review.

## Preconditions

- Phase 11 merge evidence: `origin/001-phase-10-us8` contains merge `6d3a837`
  from PR #43.
- Latest Phase 11 full-suite evidence is available in
  `specs/001-leave-management-mvp/phase-11-validation-evidence.md`.
- `main` and `origin/main` remain at the same SHA but do not contain Phases
  1-11. Phase 12 was based on the active feature base containing Phase 11.
- `.specify/extensions.yml`: absent.

## Initial Expected Failures

- `dotnet test tests\NovaLeave.IntegrationTests\NovaLeave.IntegrationTests.csproj --no-restore --filter FullyQualifiedName~Observability`: expected initial failure.
- Result: compile failed because `NovaLeave.Application.Observability` and
  `NovaLeave.Infrastructure.Observability` did not exist yet.
- After first implementation pass, the same focused command failed 2/10:
  redaction did not handle quoted JSON keys, and one invariant test was checking
  EF tracking state rather than database mutation.

## Focused Observability Validation

- `dotnet test tests\NovaLeave.IntegrationTests\NovaLeave.IntegrationTests.csproj --no-restore --filter FullyQualifiedName~Observability`: PASS, 10 passed.
- Covered:
  - Correlation ID propagation and generation.
  - Sensitive-data redaction.
  - Metrics counters and latency histogram.
  - Tracing spans with correlation-safe redacted tags.
  - Liveness and readiness/database health responses.
  - Read-only invariant monitoring.
  - Health/error response redaction.

## Phase 11 Evidence Reused

- Full suite: 58 unit, 103 integration, 12 E2E passed.
- Coverage from Phase 11 integration report:
  - `NovaLeave.Application`: 87.33% line coverage.
  - `NovaLeave.Domain`: 93.05% line coverage.
- Phase 12 added observability-scope production code only. Focused functional
  tests cover the new behavior. Full integration coverage is not rerun solely
  for Phase 12 observability additions.

## Manual Gate Results

| Check | Command | Result | Notes |
| --- | --- | --- | --- |
| Restore | Not repeated | NOT EXECUTED | No project/package files changed. |
| Build | `dotnet build NovaLeave.sln --no-restore` | PASS | 0 warnings, 0 errors. |
| Unit tests | `dotnet test tests\NovaLeave.UnitTests\NovaLeave.UnitTests.csproj --no-build --no-restore` | PASS | 58 passed, 0 failed, 0 skipped. |
| E2E tests | `dotnet test tests\NovaLeave.EndToEndTests\NovaLeave.EndToEndTests.csproj --no-build --no-restore` | PASS | 12 passed, 0 failed, 0 skipped. |
| Full integration rerun | `dotnet test tests\NovaLeave.IntegrationTests\NovaLeave.IntegrationTests.csproj --no-build --no-restore` | NOT COMPLETED | Windows runner/process hang; command timed out and orphaned `dotnet` processes were stopped. No focused Phase 12 test failed. |
| Full solution runner | `dotnet test NovaLeave.sln --no-build` | NOT COMPLETED | Windows runner/process hang; command timed out and orphaned `dotnet` processes were stopped. |
| Format/analyzers | `dotnet format NovaLeave.sln --verify-no-changes --no-restore` | NOT EXECUTED | User-directed fast gate after runner instability limited final validation to build, unit, E2E, focused Observability, `git diff --check`, and diff review. |
| Diff whitespace | `git diff --check` | PASS | No whitespace errors; CRLF conversion warnings only. |
| Focused observability | See focused command above | PASS | 10 tests. |
| Dependency vulnerabilities | Existing dotnet command | NOT EXECUTED | Not rerun in Phase 12 fast gate; Phase 11 evidence recorded PASS and no package files changed. |
| Migration validation | Existing integration test | NOT EXECUTED | Not rerun in Phase 12 fast gate; Phase 11 evidence recorded PASS and no migration files changed. |
| License review | No configured license tool found | NOT EXECUTED | Manual review limited to no package changes. |
| Mermaid validation | No changed Mermaid diagrams | NOT EXECUTED | No configured validator needed. |
| Static security | No configured static security command found | NOT EXECUTED | Dependency scan and manual secrets review used. |
| Coverage refresh | `dotnet test tests\NovaLeave.IntegrationTests\NovaLeave.IntegrationTests.csproj --no-build --no-restore --filter FullyQualifiedName~Observability --collect:"XPlat Code Coverage" --results-directory TestResults\Phase12ObservabilityCoverage` | PASS | 10 passed; report generated at `TestResults\Phase12ObservabilityCoverage\17d2d30d-53d0-49fe-bf8f-5e78b5a2fd9c\coverage.cobertura.xml`. |

## Security and Redaction

- Automated redaction tests verify logs/traces/metrics/audit-like payloads and
  health/error responses do not expose passwords, tokens, connection strings,
  secrets, request reasons, or rejection reasons.
- Manual repository scan found no new CI/CD files and no new production secrets.
- Test fixtures contain local SQL Server test connection strings and approved
  synthetic sensitive strings used only as redaction test inputs.

## Load and Concurrency Readiness

- Local/test verification uses existing concurrency and idempotency tests rather
  than a new load framework.
- Attempted combined readiness command:
  `dotnet test tests\NovaLeave.IntegrationTests\NovaLeave.IntegrationTests.csproj --no-build --no-restore --filter "FullyQualifiedName~Concurrency|FullyQualifiedName~Idempotency|FullyQualifiedName~Observability"`.
- Result: NOT COMPLETED due Windows runner/process hang. Orphaned `dotnet`
  processes were stopped. No confirmed Phase 12 functional regression was found.
- This evidence does not claim production throughput or RTO/RPO validation.

## Task Synchronization

- Canonical `specs/001-leave-management-mvp/tasks.md` is the source of truth.
- Derived `/tasks` files remain one-to-one from TASK-001 through TASK-178.
- Safe synchronization review: T001-T178 numbering remains intact in canonical
  tasks, and derived `/tasks` files for TASK-001 through TASK-178 remain present.
- Official task-tree validator: NOT EXECUTED in the fast gate.

## Remaining Findings

- No CRITICAL/HIGH finding is open from focused observability validation.
- Production alert delivery channel, exact thresholds, backup command syntax,
  restore timing, and load targets remain environment-specific operational
  decisions, documented as configurable runbook items.
