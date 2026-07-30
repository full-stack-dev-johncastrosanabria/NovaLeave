# TASK-072 - Implement edit Pending request command with full revalidation, reservation adjustment, rowversion, and audit

## Type

Infrastructure

## Epic

EPIC-003 - User Request Management

## Objective

Deliver the outcome defined by canonical task T072: [US1] Implement edit Pending request command with full revalidation, reservation adjustment, rowversion, and audit in `src/NovaLeave.Application/Requests/EditPendingRequest/`.

## Description

This task is a derived execution view of T072 from specs/001-leave-management-mvp/tasks.md. Implement or produce only the work explicitly described by the canonical task and the approved source artifacts.

## Context

The task exists to make canonical backlog item T072 executable without changing the approved scope. Original canonical text: [US1] Implement edit Pending request command with full revalidation, reservation adjustment, rowversion, and audit in `src/NovaLeave.Application/Requests/EditPendingRequest/`.

## Related Requirements

FR-001, FR-002, FR-003, FR-004, FR-005, FR-012, FR-015, FR-025, VAL-001, VAL-004, VAL-005, VAL-009, AUTHZ-001, SEC-002, SEC-003, AUD-001, AUD-002

## Related Use Cases

UC-01, UC-02, UC-03, UC-04, UC-05, UC-06, UC-07, UC-08

## Related Business Rules

BR-001, BR-002, BR-004, BR-005, BR-018, BR-019, BR-024, BR-025, BR-030, BR-031, BR-034; docs/use-cases.md UC-01 through UC-08

## Scope

- Preserve the original canonical task identifier T072.
- Deliver only: Implement edit Pending request command with full revalidation, reservation adjustment, rowversion, and audit.
- Expected files or folders:
- `src/NovaLeave.Application/Requests/EditPendingRequest/`
- Validate against the authoritative source sections listed in Technical Notes.
- Add or run applicable tests when the canonical task is a testing or implementation task.

## Out of Scope

- Any requirement, route, integration, component, state, transition, role, or behavior not present in the approved artifacts.
- Public API, JWT, OpenAPI, email, outbox, Redis, message queues, microservices, Teams, departments, managers, organizational hierarchy, delegation, escalation, external holiday calendar, per-user time zones, and User Pending cancellation.
- Modifying official specifications, contracts, plan, data model, quickstart, Constitution, or canonical `tasks.md` as part of this derived task file.

## Acceptance Criteria

- [ ] The deliverable described by T072 exists in the planned path or documented artifact.
- [ ] Required test or evidence demonstrates the deliverable matches the approved source artifacts.
- [ ] Traceability to T072 and the listed requirements/use cases is preserved.
- [ ] No excluded functionality or invented identifier is introduced.

## Dependencies

TASK-058, TASK-063

## Risks

No additional risks were identified beyond the general project risks.

## Technical Notes

- Planned path: `src/NovaLeave.Application/Requests/EditPendingRequest/`
- Original task: T072 from specs/001-leave-management-mvp/tasks.md.
- Source files and sections: `.specify/memory/constitution.md` v6.0.1; `specs/001-leave-management-mvp/spec.md`; `docs/use-cases.md`; `specs/001-leave-management-mvp/contracts/uc-contracts.md`; `specs/001-leave-management-mvp/plan.md`; applicable frontend/design/data-model/research sections by epic.
- Architectural restrictions: Clean Architecture; MVC and Razor Views; Bootstrap 5.3; EF Core and SQL Server; ASP.NET Core Identity cookie authentication; FluentValidation; Serilog; xUnit/WebApplicationFactory/approved E2E testing; built-in .NET `TimeProvider` only.
- Configuration: `NovaLeave:PendingRequestTimeoutDays`, `NovaLeave:SessionTimeoutMinutes`, `NovaLeave:SeedDemoUsers`, and accrual cadence remain explicitly configured where applicable; no defaults are invented.
- Security, concurrency, and audit: apply deny-by-default authorization, resource authorization, row-version concurrency, atomic persistence, redaction, and audit rules when applicable.

## Priority

P1

## Complexity

M

## Estimate

3 Story Points

## Definition of Done

- [ ] Deliverable implemented or document produced
- [ ] Peer review completed
- [ ] Applicable unit tests passed
- [ ] Applicable integration tests passed
- [ ] Acceptance criteria verified
- [ ] Security and authorization verified when applicable
- [ ] Documentation and traceability updated
- [ ] No known regressions
