# STALE - NOT AUTHORITATIVE

This prompt artifact is retained only as historical working material. It is not an approved source for requirements, planning, implementation status, security verification, routes, tasks, diagrams, or architecture.

Use these authoritative artifacts instead:

1. `.specify/memory/constitution.md`
2. `specs/001-leave-management-mvp/spec.md`
3. `docs/use-cases.md`
4. `specs/001-leave-management-mvp/frontend-design-spec.md`
5. `specs/002-role-based-frontend-views/spec.md`
6. `specs/001-leave-management-mvp/plan.md`
7. `specs/001-leave-management-mvp/research.md`
8. `specs/001-leave-management-mvp/data-model.md`
9. `specs/001-leave-management-mvp/quickstart.md`
10. `specs/001-leave-management-mvp/contracts/uc-contracts.md`

Current MVP baseline:

- Only `Vacation`.
- Roles: `User`, `Approver`, `HR`, plus automatic `System`.
- Exactly five states and four official transitions.
- OQ-001: RESOLVED - User cancellation of a Pending request is OUT OF MVP SCOPE.
- OQ-002: one whole vacation day per fully completed calendar month from `EmploymentStartDate`; first partial month does not accrue; no proration; no expiry; idempotent by `(UserId, AccrualPeriod)`.
- No public API, JWT, OpenAPI, email, Outbox, queues, Redis, or microservices in the MVP.
- Implementation is `NOT STARTED`.
