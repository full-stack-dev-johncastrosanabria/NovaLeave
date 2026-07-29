# TASK-143 - Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad`

## Type

Backend

## Epic

EPIC-010 - Approver Capability Management

## Objective

Deliver the outcome defined by canonical task T143: Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad`.

## Description

This task is a derived execution view of $(@{Id=143; OfficialId=T143; TaskId=TASK-143; Raw=[US8] Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad` in `src/NovaLeave.Presentation.Web/Controllers/HR/ApproverCapabilitiesController.cs`; Name=Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad`; IsParallel=False; US=US8; Paths=System.Object[]}.OfficialId) from specs/001-leave-management-mvp/tasks.md. Implement or produce only the work explicitly described by the canonical task and the approved source artifacts.

## Context

The task exists to make canonical backlog item $(@{Id=143; OfficialId=T143; TaskId=TASK-143; Raw=[US8] Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad` in `src/NovaLeave.Presentation.Web/Controllers/HR/ApproverCapabilitiesController.cs`; Name=Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad`; IsParallel=False; US=US8; Paths=System.Object[]}.OfficialId) executable without changing the approved scope. Original canonical text: $(@{Id=143; OfficialId=T143; TaskId=TASK-143; Raw=[US8] Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad` in `src/NovaLeave.Presentation.Web/Controllers/HR/ApproverCapabilitiesController.cs`; Name=Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad`; IsParallel=False; US=US8; Paths=System.Object[]}.Raw).

## Related Requirements

FR-021, FR-022, AUTHZ-012, AUTHZ-015, AUTHZ-016, AUD-009, AUD-010, SEC-001, SEC-002

## Related Use Cases

UC-22

## Related Business Rules

Constitution v6.0.0 Section 4.3; docs/use-cases.md UC-22; AC-HR-007 through AC-HR-009

## Scope

- Preserve the original canonical task identifier $(@{Id=143; OfficialId=T143; TaskId=TASK-143; Raw=[US8] Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad` in `src/NovaLeave.Presentation.Web/Controllers/HR/ApproverCapabilitiesController.cs`; Name=Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad`; IsParallel=False; US=US8; Paths=System.Object[]}.OfficialId).
- Deliver only: Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad`.
- Expected files or folders:
- `/rrhh/aprobadores`
- `/rrhh/aprobadores/{id}/capacidad`
- `src/NovaLeave.Presentation.Web/Controllers/HR/ApproverCapabilitiesController.cs`
- Validate against the authoritative source sections listed in Technical Notes.
- Add or run applicable tests when the canonical task is a testing or implementation task.

## Out of Scope

- Any requirement, route, integration, component, state, transition, role, or behavior not present in the approved artifacts.
- Public API, JWT, OpenAPI, email, outbox, Redis, message queues, microservices, Teams, departments, managers, organizational hierarchy, delegation, escalation, external holiday calendar, per-user time zones, and User Pending cancellation.
- Modifying official specifications, contracts, plan, data model, quickstart, Constitution, or canonical `tasks.md` as part of this derived task file.

## Acceptance Criteria

- [ ] The deliverable described by $(@{Id=143; OfficialId=T143; TaskId=TASK-143; Raw=[US8] Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad` in `src/NovaLeave.Presentation.Web/Controllers/HR/ApproverCapabilitiesController.cs`; Name=Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad`; IsParallel=False; US=US8; Paths=System.Object[]}.OfficialId) exists in the planned path or documented artifact.
- [ ] Required test or evidence demonstrates the deliverable matches the approved source artifacts.
- [ ] Traceability to $(@{Id=143; OfficialId=T143; TaskId=TASK-143; Raw=[US8] Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad` in `src/NovaLeave.Presentation.Web/Controllers/HR/ApproverCapabilitiesController.cs`; Name=Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad`; IsParallel=False; US=US8; Paths=System.Object[]}.OfficialId) and the listed requirements/use cases is preserved.
- [ ] No excluded functionality or invented identifier is introduced.

## Dependencies

TASK-058, TASK-136, TASK-137, TASK-138, TASK-139, TASK-140

## Risks

No additional risks were identified beyond the general project risks.

## Technical Notes

- Planned path: `/rrhh/aprobadores`, `/rrhh/aprobadores/{id}/capacidad`, `src/NovaLeave.Presentation.Web/Controllers/HR/ApproverCapabilitiesController.cs`
- Original task: $(@{Id=143; OfficialId=T143; TaskId=TASK-143; Raw=[US8] Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad` in `src/NovaLeave.Presentation.Web/Controllers/HR/ApproverCapabilitiesController.cs`; Name=Implement HR Approver capability MVC controller routes `/rrhh/aprobadores` and `/rrhh/aprobadores/{id}/capacidad`; IsParallel=False; US=US8; Paths=System.Object[]}.OfficialId) from specs/001-leave-management-mvp/tasks.md.
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
