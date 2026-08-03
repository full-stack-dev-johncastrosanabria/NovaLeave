# Phase 11 Validation Evidence

**Date**: 2026-08-03
**Branch**: `001-phase-11-validation`
**Scope**: Canonical tasks T146-T153 only.

## Preconditions

- Phase 10 is merged into `origin/001-phase-06-us4` through PR #38.
- `main` and `origin/main` both resolved to `205e1263adb19a7b2a453f2315df95f9e146faaf`.
- Phase 11 branch was created from `origin/001-phase-06-us4` because that is the active feature base containing Phases 1-10.
- `.specify/extensions.yml`: absent.
- Checklist `requirements.md`: 14 total, 14 complete, 0 incomplete.

## Official Wrapper

- `.specify/scripts/bash/check-prerequisites.sh --json --require-tasks --include-tasks`: `NOT EXECUTED`.
- Reason: `/bin/bash` is unavailable in this Windows environment.

## Initial Phase 11 Failures

- New T146-T149 integration/security/audit tests initially found:
  - Session cookie lifetime was the ASP.NET Core Identity default of 14 days instead of the approved 30 minutes.
  - Audit completeness test expected the edit audit action under a generic name; inspected implementation records the concrete action `EditPending`.
- New T150 accessibility regression initially found:
  - Approver detail action buttons lacked explicit `aria-label` attributes.
  - The first status regression assertion looked for English status literals instead of the implemented Spanish/dynamic status labels.

## Fixes Applied

- `src/NovaLeave.Web/Program.cs`: applies `NovaLeave:SessionTimeoutMinutes` to the Identity application cookie and enables sliding expiration.
- `src/NovaLeave.Web/Views/Aprobaciones/Detail.cshtml`: adds explicit accessible labels to approve, reject, and deactivate actions.
- Tests were aligned to authoritative implementation names and Spanish/dynamic UI text without weakening business or security requirements.

## Targeted Validation

- `dotnet build`: PASS.
- `dotnet test tests\NovaLeave.UnitTests\NovaLeave.UnitTests.csproj --filter FullyQualifiedName~RequestStateTransitionMatrixTests`: 21 passed.
- `dotnet test tests\NovaLeave.IntegrationTests\NovaLeave.IntegrationTests.csproj --filter "FullyQualifiedName~UseCaseRouteTraceabilityTests|FullyQualifiedName~AuditCompletenessTests|FullyQualifiedName~SecurityRegressionTests"`: 10 passed.
- `dotnet test tests\NovaLeave.EndToEndTests\NovaLeave.EndToEndTests.csproj --filter FullyQualifiedName~AccessibilityRegressionTests`: 7 passed.
- After strengthening route-template traceability, `dotnet test tests\NovaLeave.IntegrationTests\NovaLeave.IntegrationTests.csproj --filter FullyQualifiedName~UseCaseRouteTraceabilityTests`: 5 passed.

## Full Regression Gate

- `dotnet restore`: PASS.
- `dotnet build`: PASS, 0 warnings, 0 errors.
- `dotnet test`: PASS.
  - Unit: 58 passed, 0 failed, 0 skipped.
  - Integration: 103 passed, 0 failed, 0 skipped.
  - E2E: 12 passed, 0 failed, 0 skipped.

## Coverage

- `dotnet test tests\NovaLeave.UnitTests\NovaLeave.UnitTests.csproj --collect:"XPlat Code Coverage" --results-directory TestResults\Phase11Coverage`: PASS, 58 tests.
- `dotnet test tests\NovaLeave.IntegrationTests\NovaLeave.IntegrationTests.csproj --collect:"XPlat Code Coverage" --results-directory TestResults\Phase11IntegrationCoverage`: PASS, 102 tests.
- Coverage from `TestResults\Phase11IntegrationCoverage\4c98a2a3-e126-41d5-8e99-2cbed1877256\coverage.cobertura.xml`:
  - `NovaLeave.Application`: 87.33% line coverage.
  - `NovaLeave.Domain`: 93.05% line coverage.
- Coverage refresh for the one later-added route-template traceability assertion was not repeated across the whole integration project. The full regression gate passed after that assertion was added, and the focused traceability class passed separately.

## Additional Checks

- `dotnet list package --vulnerable --include-transitive`: PASS, no vulnerable packages reported by current NuGet sources.
- `dotnet test tests\NovaLeave.IntegrationTests\NovaLeave.IntegrationTests.csproj --filter FullyQualifiedName~InitialMigrationValidationTests`: PASS, 1 test.
- `git diff --check`: PASS, no whitespace errors; Git reported CRLF conversion warnings only.
- `dotnet format --verify-no-changes`: PARTIAL. The global check reports pre-existing end-of-line normalization differences across files from earlier phases. Phase 11 `.cs` files were formatted with `dotnet format --include ...` and build/tests pass.

## Documentation Review

- `quickstart.md` was updated from planned/no-code language to current implemented commands and paths.
- Active references reviewed: `spec.md`, `plan.md`, `tasks.md`, `contracts/uc-contracts.md`, `quickstart.md`, `data-model.md`, `research.md`.
- Generated `/tasks` artifacts were inspected as derived execution views only; they do not override canonical `tasks.md`.

## Remaining Findings

- No CRITICAL/HIGH defect remains from Phase 11 targeted validation.
- Bash prerequisite wrapper, license review, Docker validation, Mermaid rendering, and external browser accessibility tooling are unavailable or not configured in this environment unless later Phase 12 tooling is added; record unavailable checks as `NOT EXECUTED`.
