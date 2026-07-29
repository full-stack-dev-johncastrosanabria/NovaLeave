# TASK-121 - Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links

## Type

Frontend

## Epic

EPIC-008 - Calendars

## Objective

Deliver the outcome defined by canonical task T121: Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links.

## Description

This task is a derived execution view of $(@{Id=121; OfficialId=T121; TaskId=TASK-121; Raw=[US6] Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links in `src/NovaLeave.Presentation.Web/Views/Shared/_Calendar.cshtml`; Name=Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links; IsParallel=False; US=US6; Paths=System.Object[]}.OfficialId) from specs/001-leave-management-mvp/tasks.md. Implement or produce only the work explicitly described by the canonical task and the approved source artifacts.

## Context

The task exists to make canonical backlog item $(@{Id=121; OfficialId=T121; TaskId=TASK-121; Raw=[US6] Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links in `src/NovaLeave.Presentation.Web/Views/Shared/_Calendar.cshtml`; Name=Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links; IsParallel=False; US=US6; Paths=System.Object[]}.OfficialId) executable without changing the approved scope. Original canonical text: $(@{Id=121; OfficialId=T121; TaskId=TASK-121; Raw=[US6] Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links in `src/NovaLeave.Presentation.Web/Views/Shared/_Calendar.cshtml`; Name=Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links; IsParallel=False; US=US6; Paths=System.Object[]}.Raw).

## Related Requirements

FR-015, FR-017, FR-024, AUTHZ-017, AUTHZ-019, RBFV-026, RBFV-029

## Related Use Cases

UC-08, UC-15, UC-19

## Related Business Rules

frontend-design-spec.md Sections 22 and 25; specs/002-role-based-frontend-views/spec.md Sections 8.6, 8.9, and 8.13

## Scope

- Preserve the original canonical task identifier $(@{Id=121; OfficialId=T121; TaskId=TASK-121; Raw=[US6] Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links in `src/NovaLeave.Presentation.Web/Views/Shared/_Calendar.cshtml`; Name=Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links; IsParallel=False; US=US6; Paths=System.Object[]}.OfficialId).
- Deliver only: Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links.
- Expected files or folders:
- `_Calendar.cshtml`
- `src/NovaLeave.Presentation.Web/Views/Shared/_Calendar.cshtml`
- Validate against the authoritative source sections listed in Technical Notes.
- Add or run applicable tests when the canonical task is a testing or implementation task.

## Out of Scope

- Any requirement, route, integration, component, state, transition, role, or behavior not present in the approved artifacts.
- Public API, JWT, OpenAPI, email, outbox, Redis, message queues, microservices, Teams, departments, managers, organizational hierarchy, delegation, escalation, external holiday calendar, per-user time zones, and User Pending cancellation.
- Modifying official specifications, contracts, plan, data model, quickstart, Constitution, or canonical `tasks.md` as part of this derived task file.

## Acceptance Criteria

- [ ] The deliverable described by $(@{Id=121; OfficialId=T121; TaskId=TASK-121; Raw=[US6] Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links in `src/NovaLeave.Presentation.Web/Views/Shared/_Calendar.cshtml`; Name=Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links; IsParallel=False; US=US6; Paths=System.Object[]}.OfficialId) exists in the planned path or documented artifact.
- [ ] Required test or evidence demonstrates the deliverable matches the approved source artifacts.
- [ ] Traceability to $(@{Id=121; OfficialId=T121; TaskId=TASK-121; Raw=[US6] Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links in `src/NovaLeave.Presentation.Web/Views/Shared/_Calendar.cshtml`; Name=Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links; IsParallel=False; US=US6; Paths=System.Object[]}.OfficialId) and the listed requirements/use cases is preserved.
- [ ] No excluded functionality or invented identifier is introduced.

## Dependencies

TASK-077, TASK-093, TASK-117, TASK-118, TASK-119

## Risks

No additional risks were identified beyond the general project risks.

## Technical Notes

- Planned path: `_Calendar.cshtml`, `src/NovaLeave.Presentation.Web/Views/Shared/_Calendar.cshtml`
- Original task: $(@{Id=121; OfficialId=T121; TaskId=TASK-121; Raw=[US6] Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links in `src/NovaLeave.Presentation.Web/Views/Shared/_Calendar.cshtml`; Name=Implement shared `_Calendar.cshtml` partial with Bootstrap 5.3, status colors, keyboard support, and authorized links; IsParallel=False; US=US6; Paths=System.Object[]}.OfficialId) from specs/001-leave-management-mvp/tasks.md.
- Source files and sections: `.specify/memory/constitution.md` v6.0.0; `specs/001-leave-management-mvp/spec.md`; `docs/use-cases.md`; `specs/001-leave-management-mvp/contracts/uc-contracts.md`; `specs/001-leave-management-mvp/plan.md`; applicable frontend/design/data-model/research sections by epic.
- Architectural restrictions: Clean Architecture; MVC and Razor Views; Bootstrap 5.3; EF Core and SQL Server; ASP.NET Core Identity cookie authentication; FluentValidation; Serilog; xUnit/WebApplicationFactory/approved E2E testing; built-in .NET `TimeProvider` only.
- Configuration: `NovaLeave:PendingRequestTimeoutDays`, `NovaLeave:SessionTimeoutMinutes`, `NovaLeave:SeedDemoUsers`, and accrual cadence remain explicitly configured where applicable; no defaults are invented.
- Security, concurrency, and audit: apply deny-by-default authorization, resource authorization, row-version concurrency, atomic persistence, redaction, and audit rules when applicable.

## Priority

P3

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
