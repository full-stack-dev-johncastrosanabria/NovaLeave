# Manual Quality and Security Gate

## Scope

This gate replaces CI/CD for the NovaLeave MVP. Automated pipeline configuration is not required and must not be created for MVP acceptance.

All applicable checks are blocking before merge, handoff, release, or acceptance. A failed required check blocks acceptance. An unavailable tool must be recorded as `NOT EXECUTED`, never as `PASS`.

## Required Checks

Record the command, result, failure details, exception status, reviewer identity, and date for each item:

- Restore dependencies.
- Compile the complete solution.
- Verify formatting.
- Run configured analyzers.
- Run all unit tests.
- Run all integration tests.
- Run all applicable E2E tests.
- Produce and inspect test coverage.
- Perform static security analysis when compatible tooling is available.
- Scan dependencies for known vulnerabilities.
- Review third-party dependency licenses.
- Validate database migrations.
- Validate Mermaid diagrams when applicable tooling is available.
- Review configuration and secrets handling.

## Evidence Format

| Check | Command | Result | Evidence Location | Reviewer | Notes |
| --- | --- | --- | --- | --- | --- |
| Restore dependencies | TBD by environment | PENDING | TBD | TBD |  |
| Compile solution | TBD by environment | PENDING | TBD | TBD |  |
| Formatting | TBD by environment | PENDING | TBD | TBD |  |
| Analyzers | TBD by environment | PENDING | TBD | TBD |  |
| Unit tests | TBD by environment | PENDING | TBD | TBD |  |
| Integration tests | TBD by environment | PENDING | TBD | TBD |  |
| E2E tests | TBD by environment | PENDING | TBD | TBD |  |
| Coverage | TBD by environment | PENDING | TBD | TBD |  |
| Static security analysis | Tool-specific | PENDING or NOT EXECUTED | TBD | TBD |  |
| Dependency vulnerability scan | Tool-specific | PENDING or NOT EXECUTED | TBD | TBD |  |
| License review | Tool-specific or manual | PENDING or NOT EXECUTED | TBD | TBD |  |
| Migration validation | Environment-specific | PENDING | TBD | TBD |  |
| Mermaid validation | Tool-specific | PENDING or NOT EXECUTED | TBD | TBD |  |
| Configuration and secrets review | Manual | PENDING | TBD | TBD |  |

## Exceptions

Accepted exceptions require written rationale, approver identity, mitigation, owner, and expiration or review date. Exceptions do not waive unrelated checks.
