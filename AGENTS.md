<!-- SPECKIT START -->
Before implementation, read `.specify/memory/constitution.md` first and follow
this authority hierarchy:

1. Constitution.
2. `specs/001-leave-management-mvp/spec.md`.
3. Clarifications/research and feature design artifacts in
   `specs/001-leave-management-mvp/`.
4. `specs/001-leave-management-mvp/plan.md`.
5. Canonical tasks in `specs/001-leave-management-mvp/tasks.md`.
6. Derived execution artifacts under `/tasks`.

Use `spec.md`, `plan.md`, and the canonical `tasks.md` for implementation
scope. Root `/tasks` files are generated execution/reporting views; regenerate
or synchronize them after canonical task changes and never let them override the
canonical task source.

Resolve conflicts at the highest-authority source. Never invent missing product
decisions, implement undocumented behavior, change approved business rules, or
load archived documents as active context. Preserve test-first ordering, run
consistency validation before implementation, and execute tasks according to
their dependencies.

Every visual change must update
`specs/001-leave-management-mvp/frontend-design-spec.md` in the same change set.
Document the affected routes, visual hierarchy, responsive behavior,
accessibility considerations, and testable acceptance criteria. A visual
implementation without this specification update is incomplete.
<!-- SPECKIT END -->
