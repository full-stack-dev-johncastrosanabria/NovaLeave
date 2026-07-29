# TASK-125 - Add UC-20 HR balance and movement read-only tests

## Type

Testing

## Epic

EPIC-009 - HR Read Only

## Objective

Deliver the outcome defined by canonical task T125: Add UC-20 HR balance and movement read-only tests.

## Description

This task is a derived execution view of $(@{Id=125; OfficialId=T125; TaskId=TASK-125; Raw=[P] [US7] Add UC-20 HR balance and movement read-only tests in `tests/NovaLeave.IntegrationTests/UseCases/UC20HRBalancesTests.cs`; Name=Add UC-20 HR balance and movement read-only tests; IsParallel=True; US=US7; Paths=System.Object[]}.OfficialId) from specs/001-leave-management-mvp/tasks.md. Implement or produce only the work explicitly described by the canonical task and the approved source artifacts.

## Context

The task exists to make canonical backlog item $(@{Id=125; OfficialId=T125; TaskId=TASK-125; Raw=[P] [US7] Add UC-20 HR balance and movement read-only tests in `tests/NovaLeave.IntegrationTests/UseCases/UC20HRBalancesTests.cs`; Name=Add UC-20 HR balance and movement read-only tests; IsParallel=True; US=US7; Paths=System.Object[]}.OfficialId) executable without changing the approved scope. Original canonical text: $(@{Id=125; OfficialId=T125; TaskId=TASK-125; Raw=[P] [US7] Add UC-20 HR balance and movement read-only tests in `tests/NovaLeave.IntegrationTests/UseCases/UC20HRBalancesTests.cs`; Name=Add UC-20 HR balance and movement read-only tests; IsParallel=True; US=US7; Paths=System.Object[]}.Raw).

## Related Requirements

FR-009, FR-016, FR-017, FR-018, FR-019, FR-020, AUTHZ-011, AUTHZ-013, AUTHZ-014, SEC-005, SEC-006, SEC-009, AUD-003, AUD-005, AUD-006

## Related Use Cases

UC-18, UC-19, UC-20, UC-21

## Related Business Rules

Constitution v6.0.0 Section 4.3; docs/use-cases.md UC-18 through UC-21; AC-HR-001 through AC-HR-007 and AC-HR-009 through AC-HR-012

## Scope

- Preserve the original canonical task identifier $(@{Id=125; OfficialId=T125; TaskId=TASK-125; Raw=[P] [US7] Add UC-20 HR balance and movement read-only tests in `tests/NovaLeave.IntegrationTests/UseCases/UC20HRBalancesTests.cs`; Name=Add UC-20 HR balance and movement read-only tests; IsParallel=True; US=US7; Paths=System.Object[]}.OfficialId).
- Deliver only: Add UC-20 HR balance and movement read-only tests.
- Expected files or folders:
- `tests/NovaLeave.IntegrationTests/UseCases/UC20HRBalancesTests.cs`
- Validate against the authoritative source sections listed in Technical Notes.
- Add or run applicable tests when the canonical task is a testing or implementation task.

## Out of Scope

- Any requirement, route, integration, component, state, transition, role, or behavior not present in the approved artifacts.
- Public API, JWT, OpenAPI, email, outbox, Redis, message queues, microservices, Teams, departments, managers, organizational hierarchy, delegation, escalation, external holiday calendar, per-user time zones, and User Pending cancellation.
- Modifying official specifications, contracts, plan, data model, quickstart, Constitution, or canonical `tasks.md` as part of this derived task file.

## Acceptance Criteria

- [ ] The deliverable described by $(@{Id=125; OfficialId=T125; TaskId=TASK-125; Raw=[P] [US7] Add UC-20 HR balance and movement read-only tests in `tests/NovaLeave.IntegrationTests/UseCases/UC20HRBalancesTests.cs`; Name=Add UC-20 HR balance and movement read-only tests; IsParallel=True; US=US7; Paths=System.Object[]}.OfficialId) exists in the planned path or documented artifact.
- [ ] Required test or evidence demonstrates the deliverable matches the approved source artifacts.
- [ ] Traceability to $(@{Id=125; OfficialId=T125; TaskId=TASK-125; Raw=[P] [US7] Add UC-20 HR balance and movement read-only tests in `tests/NovaLeave.IntegrationTests/UseCases/UC20HRBalancesTests.cs`; Name=Add UC-20 HR balance and movement read-only tests; IsParallel=True; US=US7; Paths=System.Object[]}.OfficialId) and the listed requirements/use cases is preserved.
- [ ] No excluded functionality or invented identifier is introduced.

## Dependencies

TASK-058

## Risks

No additional risks were identified beyond the general project risks.

## Technical Notes

- Planned path: `tests/NovaLeave.IntegrationTests/UseCases/UC20HRBalancesTests.cs`
- Original task: $(@{Id=125; OfficialId=T125; TaskId=TASK-125; Raw=[P] [US7] Add UC-20 HR balance and movement read-only tests in `tests/NovaLeave.IntegrationTests/UseCases/UC20HRBalancesTests.cs`; Name=Add UC-20 HR balance and movement read-only tests; IsParallel=True; US=US7; Paths=System.Object[]}.OfficialId) from specs/001-leave-management-mvp/tasks.md.
- Source files and sections: `.specify/memory/constitution.md` v6.0.0; `specs/001-leave-management-mvp/spec.md`; `docs/use-cases.md`; `specs/001-leave-management-mvp/contracts/uc-contracts.md`; `specs/001-leave-management-mvp/plan.md`; applicable frontend/design/data-model/research sections by epic.
- Architectural restrictions: Clean Architecture; MVC and Razor Views; Bootstrap 5.3; EF Core and SQL Server; ASP.NET Core Identity cookie authentication; FluentValidation; Serilog; xUnit/WebApplicationFactory/approved E2E testing; built-in .NET `TimeProvider` only.
- Configuration: `NovaLeave:PendingRequestTimeoutDays`, `NovaLeave:SessionTimeoutMinutes`, `NovaLeave:SeedDemoUsers`, and accrual cadence remain explicitly configured where applicable; no defaults are invented.
- Security, concurrency, and audit: apply deny-by-default authorization, resource authorization, row-version concurrency, atomic persistence, redaction, and audit rules when applicable.

## Priority

P2

## Complexity

S

## Estimate

2 Story Points

## Definition of Done

- [ ] Deliverable implemented or document produced
- [ ] Peer review completed
- [ ] Applicable unit tests passed
- [ ] Applicable integration tests passed
- [ ] Acceptance criteria verified
- [ ] Security and authorization verified when applicable
- [ ] Documentation and traceability updated
- [ ] No known regressions
