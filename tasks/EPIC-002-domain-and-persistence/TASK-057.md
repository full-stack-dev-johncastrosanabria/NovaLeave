# TASK-057 - Create integration test factory

## Type

Testing

## Epic

EPIC-002 - Domain and Persistence

## Objective

Deliver the outcome defined by canonical task T057: Create integration test factory in `tests/NovaLeave.IntegrationTests/Support/NovaLeaveWebApplicationFactory.cs`.

## Description

This task is a derived execution view of T057 from specs/001-leave-management-mvp/tasks.md. Implement or produce only the work explicitly described by the canonical task and the approved source artifacts.

## Context

The task exists to make canonical backlog item T057 executable without changing the approved scope. Original canonical text: Create integration test factory in `tests/NovaLeave.IntegrationTests/Support/NovaLeaveWebApplicationFactory.cs`.

## Related Requirements

VAL-002, VAL-003, BR-001, BR-002, BR-004, BR-017, BR-018, BR-030, BR-031, AUD-001, AUD-002, CON-001, CON-002, CFG-001, CFG-002

## Related Use Cases

N/A â€” cross-cutting task

## Related Business Rules

Constitution v6.0.0 Sections 4, 5, 6, 7, 8, 9, 11; data-model.md Domain Traceability Matrix and Concurrency and Idempotency Matrix; research.md CR-01 through CR-16

## Scope

- Preserve the original canonical task identifier T057.
- Deliver only: Create integration test factory.
- Expected files or folders:
- `tests/NovaLeave.IntegrationTests/Support/NovaLeaveWebApplicationFactory.cs`
- Validate against the authoritative source sections listed in Technical Notes.
- Add or run applicable tests when the canonical task is a testing or implementation task.

## Out of Scope

- Any requirement, route, integration, component, state, transition, role, or behavior not present in the approved artifacts.
- Public API, JWT, OpenAPI, email, outbox, Redis, message queues, microservices, Teams, departments, managers, organizational hierarchy, delegation, escalation, external holiday calendar, per-user time zones, and User Pending cancellation.
- Modifying official specifications, contracts, plan, data model, quickstart, Constitution, or canonical `tasks.md` as part of this derived task file.

## Acceptance Criteria

- [ ] The deliverable described by T057 exists in the planned path or documented artifact.
- [ ] Required test or evidence demonstrates the deliverable matches the approved source artifacts.
- [ ] Traceability to T057 and the listed requirements/use cases is preserved.
- [ ] No excluded functionality or invented identifier is introduced.

## Dependencies

TASK-013, TASK-014, TASK-015, TASK-016, TASK-017, TASK-018, TASK-019

## Risks

No additional risks were identified beyond the general project risks.

## Technical Notes

- Planned path: `tests/NovaLeave.IntegrationTests/Support/NovaLeaveWebApplicationFactory.cs`
- Original task: T057 from specs/001-leave-management-mvp/tasks.md.
- Source files and sections: `.specify/memory/constitution.md` v6.0.0; `specs/001-leave-management-mvp/spec.md`; `docs/use-cases.md`; `specs/001-leave-management-mvp/contracts/uc-contracts.md`; `specs/001-leave-management-mvp/plan.md`; applicable frontend/design/data-model/research sections by epic.
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
