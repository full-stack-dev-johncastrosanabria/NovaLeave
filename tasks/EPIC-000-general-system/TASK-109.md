# TASK-109 - Add UC-17 accrual integration tests

## Type

Testing

## Epic

EPIC-007 - Balance and Accrual

## Objective

Deliver the outcome defined by canonical task T109: [P] [US5] Add UC-17 accrual integration tests in `tests/NovaLeave.IntegrationTests/UseCases/UC17MonthlyAccrualTests.cs`.

## Description

This task is a derived execution view of T109 from specs/001-leave-management-mvp/tasks.md. Implement or produce only the work explicitly described by the canonical task and the approved source artifacts.

## Context

The task exists to make canonical backlog item T109 executable without changing the approved scope. Original canonical text: [P] [US5] Add UC-17 accrual integration tests in `tests/NovaLeave.IntegrationTests/UseCases/UC17MonthlyAccrualTests.cs`.

## Related Requirements

FR-004, FR-014, BR-017, BR-032, BR-033, CON-003, AUD-007

## Related Use Cases

UC-07, UC-17

## Related Business Rules

BR-017, BR-030, BR-031, BR-032, BR-033; spec.md OQ-002; docs/use-cases.md UC-17

## Scope

- Preserve the original canonical task identifier T109.
- Deliver only: Add UC-17 accrual integration tests.
- Expected files or folders:
- `tests/NovaLeave.IntegrationTests/UseCases/UC17MonthlyAccrualTests.cs`
- Validate against the authoritative source sections listed in Technical Notes.
- Add or run applicable tests when the canonical task is a testing or implementation task.

## Out of Scope

- Any requirement, route, integration, component, state, transition, role, or behavior not present in the approved artifacts.
- Public API, JWT, OpenAPI, email, outbox, Redis, message queues, microservices, Teams, departments, managers, organizational hierarchy, delegation, escalation, external holiday calendar, per-user time zones, and User Pending cancellation.
- Modifying official specifications, contracts, plan, data model, quickstart, Constitution, or canonical `tasks.md` as part of this derived task file.

## Acceptance Criteria

- [ ] The deliverable described by T109 exists in the planned path or documented artifact.
- [ ] Required test or evidence demonstrates the deliverable matches the approved source artifacts.
- [ ] Traceability to T109 and the listed requirements/use cases is preserved.
- [ ] No excluded functionality or invented identifier is introduced.

## Dependencies

TASK-058

## Risks

No additional risks were identified beyond the general project risks.

## Technical Notes

- Planned path: `tests/NovaLeave.IntegrationTests/UseCases/UC17MonthlyAccrualTests.cs`
- Original task: T109 from specs/001-leave-management-mvp/tasks.md.
- Source files and sections: `.specify/memory/constitution.md` v6.0.1; `specs/001-leave-management-mvp/spec.md`; `docs/use-cases.md`; `specs/001-leave-management-mvp/contracts/uc-contracts.md`; `specs/001-leave-management-mvp/plan.md`; applicable frontend/design/data-model/research sections by epic.
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
