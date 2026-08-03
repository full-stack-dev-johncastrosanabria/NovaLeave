# Manual Quality and Security Gate

## Scope

This gate replaces CI/CD for the NovaLeave MVP. Automated pipeline
configuration, automated deployment, continuous delivery, and release automation
must not be created for MVP acceptance.

All applicable checks are blocking before merge, handoff, release, or
acceptance. A failed required check blocks acceptance. An unavailable tool must
be recorded as `NOT EXECUTED`, never as `PASS`.

## Procedure

1. Record date, branch, commit SHA, implementer, reviewer, environment, and
   database target.
2. Execute the configured commands for restore, build, format, analyzers, unit
   tests, integration tests, E2E tests, migration validation, and vulnerability
   scanning.
3. Reuse prior valid coverage only when allowed by the active phase rules and
   record the source report. Otherwise run focused coverage for changed code.
4. Inspect configuration and evidence for secrets before attaching or sharing.
5. Record failures exactly, including command, exit code, affected check, and
   remediation.
6. Record unavailable optional tooling as `NOT EXECUTED` with reason.
7. Record accepted exceptions with approval, owner, mitigation, and review date.

## Evidence Checklist

| Check | Command | Required Result | Evidence Required |
| --- | --- | --- | --- |
| Restore dependencies | `dotnet restore` when packages or project files changed | PASS or NOT EXECUTED when unchanged and current | Command/result or reason skipped |
| Compile solution | `dotnet build NovaLeave.sln --no-restore` | PASS | Exit result and warning count |
| Formatting/analyzers | `dotnet format NovaLeave.sln --verify-no-changes --no-restore` | PASS, or documented repository-wide pre-existing issue | Command/result and scope |
| Unit tests | `dotnet test NovaLeave.sln --no-build` or project-specific command | PASS | Counts |
| Integration tests | Included in full solution command | PASS | Counts |
| E2E tests | Included when available | PASS or NOT EXECUTED if unavailable | Counts or reason |
| Coverage | Latest valid report or focused coverage | PASS, PARTIAL, or NOT EXECUTED | Report path and Domain/Application percentages |
| Static security | Existing configured tool only | PASS or NOT EXECUTED | Tool/result or reason |
| Dependency vulnerabilities | `dotnet list package --vulnerable --include-transitive` | PASS or documented advisory | Command/result |
| License review | Existing configured tool or manual review | PASS or NOT EXECUTED | Tool/result or reason |
| Migration validation | Existing migration validation test or environment command | PASS | Command/result |
| Mermaid validation | Existing configured validator only | PASS or NOT EXECUTED | Changed diagrams and result |
| Configuration/secrets | Manual inspection | PASS | Scope and findings |
| Task synchronization | Canonical and derived task validation | PASS or NOT EXECUTED for missing validator | Command/result |

## NOT EXECUTED Handling

Use `NOT EXECUTED` only when a tool is unavailable, not configured, out of scope,
or intentionally skipped by an approved phase rule. The evidence must state why
the skipped check does not weaken required functionality.

## Exceptions

Accepted exceptions require written rationale, approver identity, mitigation,
owner, and expiration or review date. Exceptions do not waive unrelated checks.
