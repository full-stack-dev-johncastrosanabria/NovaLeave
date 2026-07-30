# TASK-013 - Add Clean Architecture dependency tests

## Type

Testing

## Epic

EPIC-001 - Foundation

## Objective

Deliver the outcome defined by canonical task T013: [P] Add Clean Architecture dependency tests in `tests/NovaLeave.UnitTests/Architecture/CleanArchitectureTests.cs`.

## Description

This task is a derived execution view of T013 from specs/001-leave-management-mvp/tasks.md. Implement or produce only the work explicitly described by the canonical task and the approved source artifacts.

## Context

The task exists to make canonical backlog item T013 executable without changing the approved scope. Original canonical text: [P] Add Clean Architecture dependency tests in `tests/NovaLeave.UnitTests/Architecture/CleanArchitectureTests.cs`.

## Related Requirements

N/A Ã¢â‚¬â€ foundational task derived from specs/001-leave-management-mvp/tasks.md Phase 1-2 and plan.md Technical Context

## Related Use Cases

N/A Ã¢â‚¬â€ cross-cutting task

## Related Business Rules

Constitution v6.0.1 Sections 1-3, 9, and 15.1; plan.md Technical Context and Project Structure

## Scope

- Preserve the original canonical task identifier T013.
- Deliver only: Add Clean Architecture dependency tests.
- Expected files or folders:
- `tests/NovaLeave.UnitTests/Architecture/CleanArchitectureTests.cs`
- Validate against the authoritative source sections listed in Technical Notes.
- Add or run applicable tests when the canonical task is a testing or implementation task.

## Out of Scope

- Any requirement, route, integration, component, state, transition, role, or behavior not present in the approved artifacts.
- Public API, JWT, OpenAPI, email, outbox, Redis, message queues, microservices, Teams, departments, managers, organizational hierarchy, delegation, escalation, external holiday calendar, per-user time zones, and User Pending cancellation.
- Modifying official specifications, contracts, plan, data model, quickstart, Constitution, or canonical `tasks.md` as part of this derived task file.

## Acceptance Criteria

- [ ] The deliverable described by T013 exists in the planned path or documented artifact.
- [ ] Required test or evidence demonstrates the deliverable matches the approved source artifacts.
- [ ] Traceability to T013 and the listed requirements/use cases is preserved.
- [ ] No excluded functionality or invented identifier is introduced.

## Dependencies

TASK-011, TASK-012

## Risks

No additional risks were identified beyond the general project risks.

## Technical Notes

- Planned path: `tests/NovaLeave.UnitTests/Architecture/CleanArchitectureTests.cs`
- Original task: T013 from specs/001-leave-management-mvp/tasks.md.
- Source files and sections: `.specify/memory/constitution.md` v6.0.1; `specs/001-leave-management-mvp/spec.md`; `docs/use-cases.md`; `specs/001-leave-management-mvp/contracts/uc-contracts.md`; `specs/001-leave-management-mvp/plan.md`; applicable frontend/design/data-model/research sections by epic.
- Architectural restrictions: Clean Architecture; MVC and Razor Views; Bootstrap 5.3; EF Core and SQL Server; ASP.NET Core Identity cookie authentication; FluentValidation; Serilog; xUnit/WebApplicationFactory/approved E2E testing; built-in .NET `TimeProvider` only.
- Configuration: `NovaLeave:PendingRequestTimeoutDays`, `NovaLeave:SessionTimeoutMinutes`, `NovaLeave:SeedDemoUsers`, and accrual cadence remain explicitly configured where applicable; no defaults are invented.
- Security, concurrency, and audit: apply deny-by-default authorization, resource authorization, row-version concurrency, atomic persistence, redaction, and audit rules when applicable.

## Priority

P1

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
