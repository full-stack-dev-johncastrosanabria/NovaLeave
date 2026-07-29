# TASK-090 - Implement Approver authorization resource checks

## Type

Security

## Epic

EPIC-004 - Approver Resolution

## Objective

Deliver the outcome defined by canonical task T090: Implement Approver authorization resource checks.

## Description

This task is a derived execution view of $(@{Id=90; OfficialId=T090; TaskId=TASK-090; Raw=[US2] Implement Approver authorization resource checks in `src/NovaLeave.Application/Authorization/ApproverPolicies.cs`; Name=Implement Approver authorization resource checks; IsParallel=False; US=US2; Paths=System.Object[]}.OfficialId) from specs/001-leave-management-mvp/tasks.md. Implement or produce only the work explicitly described by the canonical task and the approved source artifacts.

## Context

The task exists to make canonical backlog item $(@{Id=90; OfficialId=T090; TaskId=TASK-090; Raw=[US2] Implement Approver authorization resource checks in `src/NovaLeave.Application/Authorization/ApproverPolicies.cs`; Name=Implement Approver authorization resource checks; IsParallel=False; US=US2; Paths=System.Object[]}.OfficialId) executable without changing the approved scope. Original canonical text: $(@{Id=90; OfficialId=T090; TaskId=TASK-090; Raw=[US2] Implement Approver authorization resource checks in `src/NovaLeave.Application/Authorization/ApproverPolicies.cs`; Name=Implement Approver authorization resource checks; IsParallel=False; US=US2; Paths=System.Object[]}.Raw).

## Related Requirements

FR-006, FR-007, FR-025, AUTHZ-002, AUTHZ-003, AUTHZ-007, AUTHZ-009, AUTHZ-010, SEC-004, SEC-005, AUD-002

## Related Use Cases

UC-09, UC-10, UC-11, UC-12, UC-14, UC-15

## Related Business Rules

BR-006, BR-007, BR-008, BR-009, BR-011, BR-012, BR-013, BR-014, BR-015, BR-016, BR-019, BR-020, BR-021, BR-023; docs/use-cases.md UC-09 through UC-15

## Scope

- Preserve the original canonical task identifier $(@{Id=90; OfficialId=T090; TaskId=TASK-090; Raw=[US2] Implement Approver authorization resource checks in `src/NovaLeave.Application/Authorization/ApproverPolicies.cs`; Name=Implement Approver authorization resource checks; IsParallel=False; US=US2; Paths=System.Object[]}.OfficialId).
- Deliver only: Implement Approver authorization resource checks.
- Expected files or folders:
- `src/NovaLeave.Application/Authorization/ApproverPolicies.cs`
- Validate against the authoritative source sections listed in Technical Notes.
- Add or run applicable tests when the canonical task is a testing or implementation task.

## Out of Scope

- Any requirement, route, integration, component, state, transition, role, or behavior not present in the approved artifacts.
- Public API, JWT, OpenAPI, email, outbox, Redis, message queues, microservices, Teams, departments, managers, organizational hierarchy, delegation, escalation, external holiday calendar, per-user time zones, and User Pending cancellation.
- Modifying official specifications, contracts, plan, data model, quickstart, Constitution, or canonical `tasks.md` as part of this derived task file.

## Acceptance Criteria

- [ ] The deliverable described by $(@{Id=90; OfficialId=T090; TaskId=TASK-090; Raw=[US2] Implement Approver authorization resource checks in `src/NovaLeave.Application/Authorization/ApproverPolicies.cs`; Name=Implement Approver authorization resource checks; IsParallel=False; US=US2; Paths=System.Object[]}.OfficialId) exists in the planned path or documented artifact.
- [ ] Required test or evidence demonstrates the deliverable matches the approved source artifacts.
- [ ] Traceability to $(@{Id=90; OfficialId=T090; TaskId=TASK-090; Raw=[US2] Implement Approver authorization resource checks in `src/NovaLeave.Application/Authorization/ApproverPolicies.cs`; Name=Implement Approver authorization resource checks; IsParallel=False; US=US2; Paths=System.Object[]}.OfficialId) and the listed requirements/use cases is preserved.
- [ ] No excluded functionality or invented identifier is introduced.

## Dependencies

TASK-058, TASK-082

## Risks

No additional risks were identified beyond the general project risks.

## Technical Notes

- Planned path: `src/NovaLeave.Application/Authorization/ApproverPolicies.cs`
- Original task: $(@{Id=90; OfficialId=T090; TaskId=TASK-090; Raw=[US2] Implement Approver authorization resource checks in `src/NovaLeave.Application/Authorization/ApproverPolicies.cs`; Name=Implement Approver authorization resource checks; IsParallel=False; US=US2; Paths=System.Object[]}.OfficialId) from specs/001-leave-management-mvp/tasks.md.
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
