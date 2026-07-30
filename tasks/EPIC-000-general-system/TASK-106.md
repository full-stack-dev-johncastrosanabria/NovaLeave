# TASK-106 - Add Approver POST route `/aprobaciones/{id}/desactivar`

## Type

Backend

## Epic

EPIC-006 - Approved Request Deactivation

## Objective

Deliver the outcome defined by canonical task T106: [US4] Add Approver POST route `/aprobaciones/{id}/desactivar` in `src/NovaLeave.Presentation.Web/Controllers/Approver/ApprovalsController.cs`.

## Description

This task is a derived execution view of T106 from specs/001-leave-management-mvp/tasks.md. Implement or produce only the work explicitly described by the canonical task and the approved source artifacts.

## Context

The task exists to make canonical backlog item T106 executable without changing the approved scope. Original canonical text: [US4] Add Approver POST route `/aprobaciones/{id}/desactivar` in `src/NovaLeave.Presentation.Web/Controllers/Approver/ApprovalsController.cs`.

## Related Requirements

FR-011, BR-027, BR-035, CON-010, AUD-002, AUD-007, ERR-002

## Related Use Cases

UC-13

## Related Business Rules

BR-027, BR-028, BR-035; docs/use-cases.md UC-13

## Scope

- Preserve the original canonical task identifier T106.
- Deliver only: Add Approver POST route `/aprobaciones/{id}/desactivar`.
- Expected files or folders:
- `/aprobaciones/{id}/desactivar`
- `src/NovaLeave.Presentation.Web/Controllers/Approver/ApprovalsController.cs`
- Validate against the authoritative source sections listed in Technical Notes.
- Add or run applicable tests when the canonical task is a testing or implementation task.

## Out of Scope

- Any requirement, route, integration, component, state, transition, role, or behavior not present in the approved artifacts.
- Public API, JWT, OpenAPI, email, outbox, Redis, message queues, microservices, Teams, departments, managers, organizational hierarchy, delegation, escalation, external holiday calendar, per-user time zones, and User Pending cancellation.
- Modifying official specifications, contracts, plan, data model, quickstart, Constitution, or canonical `tasks.md` as part of this derived task file.

## Acceptance Criteria

- [ ] The deliverable described by T106 exists in the planned path or documented artifact.
- [ ] Required test or evidence demonstrates the deliverable matches the approved source artifacts.
- [ ] Traceability to T106 and the listed requirements/use cases is preserved.
- [ ] No excluded functionality or invented identifier is introduced.

## Dependencies

TASK-093, TASK-101, TASK-102, TASK-103

## Risks

No additional risks were identified beyond the general project risks.

## Technical Notes

- Planned path: `/aprobaciones/{id}/desactivar`, `src/NovaLeave.Presentation.Web/Controllers/Approver/ApprovalsController.cs`
- Original task: T106 from specs/001-leave-management-mvp/tasks.md.
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
