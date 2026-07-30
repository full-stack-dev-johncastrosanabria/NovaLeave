# DR-003 - Manual Quality Gate Without CI/CD

**Status**: Accepted  
**Date**: 2026-07-30  
**Related**: Constitution v7.0.0; Manual Quality and Security Gate; `docs/operations/manual-quality-gate.md`

## Context

NovaLeave MVP is delivered without CI/CD, automated deployment pipelines,
automated release pipelines, continuous deployment, or continuous delivery
configuration. The project still requires reproducible quality, security,
testing, validation, and evidence before merge, handoff, release, or acceptance.

The prior constitution required an automated CI gate. That conflicted with the
approved MVP delivery model.

## Decision

NovaLeave MVP uses a mandatory Manual Quality and Security Gate instead of
CI/CD. Automated pipeline configuration is not required and must not be created
for the MVP.

The manual gate is blocking. Failed required checks prevent merge, handoff,
release, or acceptance. Unavailable tooling is recorded as `NOT EXECUTED`, never
as `PASS`.

## Alternatives Considered

1. **GitHub Actions CI**: Rejected because CI/CD is outside the MVP delivery
   model.
2. **Azure DevOps or another managed pipeline**: Rejected for the same reason
   and to avoid introducing unsupported delivery infrastructure.
3. **No formal gate**: Rejected because absence of CI/CD does not waive quality
   or security obligations.

## Consequences

- Developers and reviewers must manually execute and document all applicable
  gate checks.
- Evidence must include commands, results, failures, exceptions, reviewer
  identity, and date.
- Tooling gaps are visible as `NOT EXECUTED` rather than hidden as success.
- Reintroducing CI/CD later requires an approved decision and any required
  constitutional amendment.

## Manual Gate Procedure

Before merge, handoff, release, or acceptance, execute and record:

1. Restore dependencies.
2. Compile the complete solution.
3. Verify formatting and `.editorconfig` compliance.
4. Run configured analyzers, including nullable analysis where configured.
5. Run all unit tests.
6. Run all integration tests.
7. Run all applicable E2E tests.
8. Produce and inspect test coverage.
9. Perform static security analysis when compatible tooling is available.
10. Scan dependencies for known vulnerabilities.
11. Review third-party dependency licenses.
12. Validate database migrations.
13. Validate modified Mermaid diagrams when applicable tooling is available.
14. Review configuration and secrets handling.
15. Record commands, results, failures, exceptions, and reviewer identity.

## Required Evidence

- Date, branch, commit SHA, and reviewer identity.
- Exact commands executed.
- Exit codes or explicit result statuses.
- Coverage summary and inspection notes.
- Security, dependency, and license scan results, or `NOT EXECUTED` with reason.
- Migration and Mermaid validation results, or `NOT EXECUTED` with reason.
- Approved exceptions with rationale, mitigation, owner, and expiration or
  removal condition.

## Conditions for Reconsidering CI/CD

CI/CD may be reconsidered if the project adopts automated delivery governance,
requires repeatable team-scale release validation, or receives an approved
production operations requirement for automated pipelines. Reconsideration must
be recorded in a new ADR and, if constitutional delivery obligations change, a
constitution amendment.
