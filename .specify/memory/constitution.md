<!--
Sync Impact Report
==================
Version change: 1.0.0 → 2.0.0

Modified principles:
- I. Clean Architecture → renamed "API" layer to "Presentation" throughout;
  clarified Presentation currently exposes an ASP.NET Core Web API but is
  intentionally designed to also host Blazor, SignalR, gRPC, Workers, or
  other clients in the future; reinforced inward-only dependencies and
  "no business logic in Presentation."
- II. SOLID & Simplicity → split: the general-purpose "prefer simple,
  non-clever solutions" guidan
 ce moved to the new Principle II below;
  this principle is retained (renumbered III) and narrowed to SOLID +
  single source of truth for business logic.
- IV. CQRS with MediatR (renamed "V. CQRS — Preferred, Not Mandated") →
  redefined: MediatR is now OPTIONAL rather than mandatory; native
  ASP.NET Core patterns are explicitly permitted; this is a
  backward-incompatible redefinition of a prior MUST rule, hence the
  MAJOR version bump.
- VII. Asynchronous Execution & Observability (renumbered VIII) → trimmed
  to the async/observability baseline; expanded operational detail moved
  to the new dedicated "Observability" section.
- VIII. Security by Design (renumbered IX) → retained, now points to the
  expanded "Security" section for concrete controls.

Added principles:
- II. Simplicity Over Unnecessary Abstraction (NEW)
- X. Architecture Exists to Support the Business (NEW, capstone principle)

Added sections:
- Project Structure
- Vertical Slice Architecture
- Naming Conventions
- Technology Decisions (absorbs prior "Technology Stack & API Standards")
- Security (expanded from principle-level detail)
- Observability (expanded from principle-level detail)
- Code Quality
- Git Workflow
- Domain Invariants
- Testing Philosophy (absorbs testing-gate detail from prior "Development
  Workflow & Quality Gates")
- Future Evolution

Removed sections:
- Technology Stack & API Standards → superseded by "Technology Decisions"
- Development Workflow & Quality Gates → split into "Git Workflow" (process/
  review bullets) and "Testing Philosophy" (testing-gate bullets); the
  complexity-justification bullet moved into Governance.

Templates requiring updates:
- ✅ .specify/templates/tasks-template.md — already updated in v1.0.0 to make
  tests mandatory; principle renumbering (VI → VII) does not change its
  meaning, no further edit required.
- ✅ .specify/templates/plan-template.md — Constitution Check gate remains
  generic ("[Gates determined based on constitution file]"); no edit
  required, gates are derived at plan time from this file, including the
  new Project Structure / Vertical Slice Architecture sections.
- ✅ .specify/templates/spec-template.md — no change needed; acceptance
  scenario format still aligns with the Testing Philosophy section.
- ✅ Command/skill files (.claude/skills/speckit-*) — reviewed, no
  agent-specific references requiring genericization.

Follow-up TODOs: None. All placeholders resolved.
-->

# NovaLeave Constitution

## Core Principles

### I. Clean Architecture

The codebase MUST be organized into four layers — **Domain**, **Application**,
**Infrastructure**, and **Presentation** — with dependencies pointing strictly
inward. Domain MUST NOT reference Application, Infrastructure, or
Presentation. Application MUST depend only on Domain (via
abstractions/interfaces). Infrastructure implements interfaces defined by
Domain/Application and MUST NOT be referenced by them. Presentation composes
Application use cases and MUST contain no business logic.

The Presentation layer currently exposes an ASP.NET Core Web API, but it is
intentionally designed as a boundary that MAY host additional presentation
technologies over time — Blazor, SignalR, gRPC, background Workers, or other
clients — without requiring changes to Domain, Application, or
Infrastructure. Business logic MUST NEVER exist in the Presentation layer,
regardless of which presentation technology hosts it.

**Rationale**: Enforcing inward-only dependencies keeps business logic
framework-agnostic, testable in isolation, and able to evolve (e.g., swapping
EF Core, SQL Server, or adding a second front door like gRPC) without
touching Domain or Application code.

### II. Simplicity Over Unnecessary Abstraction

The simplest solution that correctly satisfies today's requirement MUST be
preferred over a more general, more "architecturally pure," or more
speculatively extensible one. Patterns, layers, interfaces, and abstractions
MUST be introduced only to solve a problem that actually exists in the
codebase — not one that might exist someday. An interface with a single
implementation "just in case," a generic framework built ahead of its second
use case, or a configuration knob nobody asked for are overengineering, not
craftsmanship, and MUST be avoided. Code MUST be understandable by another
engineer reading it six months from now without additional explanation;
where a choice exists between a clever, terse solution and a plain, explicit
one, the plain solution wins. Readability is optimized for before cleverness.

**Rationale**: NovaLeave is a long-lived enterprise system maintained by a
rotating set of engineers. Complexity that isn't paying for itself today
becomes a permanent tax on every future change — premature abstraction is
far harder to remove than to add later once a real second use case appears.

### III. SOLID & Single Source of Truth

All classes and modules MUST follow SOLID principles (Single Responsibility,
Open/Closed, Liskov Substitution, Interface Segregation, Dependency
Inversion). Business logic MUST NOT be duplicated across handlers, features,
or layers — a rule expressed once in the Domain (or a designated Application
service) is reused everywhere else, never copy-pasted. SOLID and
deduplication are applied as means to maintainability, not as ends in
themselves: a pattern MUST NOT be introduced solely to satisfy a principle
in the abstract when a simpler design already satisfies it in practice (see
Principle II).

**Rationale**: SOLID keeps the codebase change-tolerant as the leave-
management domain grows (new leave types, approval chains, policies); a
single source of truth for business rules directly reduces defect rates and
onboarding cost.

### IV. Domain-Driven Design & Business Rule Isolation

Business entities (e.g., Employee, LeaveRequest, LeaveBalance, LeavePolicy)
MUST be modeled using DDD building blocks — entities, value objects,
aggregates, and domain events — with invariants enforced by the entities
themselves, not by external callers. **All business rules MUST reside
exclusively in the Domain layer.** Application layer handlers MUST
orchestrate domain objects and infrastructure calls; they MUST NOT
re-implement or duplicate business rules (e.g., leave-balance calculations,
overlap checks, approval-eligibility rules — see the Domain Invariants
section for concrete examples).

When domain entities or aggregates are introduced or materially changed, a
UML class diagram or equivalent visual model describing entities, value
objects, aggregates, key invariants, and relationships SHOULD be produced
and committed to `docs/diagrams` or `.specify/diagrams`. Where diagram
generation tools are available (PlantUML, Mermaid or model-first generators),
use them and include generated artifacts in the PR. Diagrams are required
for all entities when available and MUST be updated as part of the same PR
that modifies the domain model to aid design reviews and onboarding.

**Rationale**: Centralizing rules in Domain guarantees a single, testable
source of truth for what constitutes a valid leave request, balance, or
approval — preventing rule drift between Presentation, Application, and
Infrastructure code paths.

### V. CQRS — Preferred, Not Mandated

CQRS (separating commands that mutate state from queries that read state) is
the preferred style for organizing Application-layer use cases. MediatR MAY
be used to implement CQRS but is **NOT mandatory** — a feature MAY instead be
implemented with native ASP.NET Core patterns (a plain application service
class, a minimal API handler, or a controller action calling an Application
service directly) when that is simpler and equally testable. When MediatR is
used, its pipeline behaviors are the preferred mechanism for cross-cutting
concerns (validation, logging, transactions); when it is not, framework-
native filters/middleware are an acceptable equivalent. Libraries and
abstractions MUST NOT be introduced without a clear, articulable benefit over
what the framework already provides.

**Rationale**: CQRS's separation of concerns is valuable regardless of
tooling. Mandating a specific library for every use case, including trivial
ones, contradicts Principle II; keeping the architecture compatible with
native ASP.NET Core patterns avoids library lock-in and keeps the barrier to
entry low for new features.

### VI. Dependency Injection & Input Validation

All dependencies (repositories, domain services, infrastructure clients)
MUST be provided through the built-in ASP.NET Core DI container — static
access, service locators, and `new`-ing up infrastructure dependencies inside
Application/Domain code are prohibited. Every request/command/query with
external input MUST be validated using FluentValidation validators executed
through a validation pipeline (MediatR behavior or the equivalent native
filter/middleware per Principle V), not ad-hoc `if` checks inside handlers or
controllers.

**Rationale**: Constructor-injected dependencies keep components substitutable
and unit-testable. Centralized FluentValidation keeps validation rules
declarative, consistent, and separate from business/domain rules.

### VII. Test-First Discipline (NON-NEGOTIABLE)

Every feature MUST have documented acceptance criteria agreed upon **before**
implementation begins. Every business rule implemented in the Domain layer
MUST have unit test coverage proving the rule in isolation. Every critical
API endpoint (authentication, leave request submission/approval/cancellation,
balance queries, and any endpoint touching payroll-relevant data) MUST have
integration tests exercising the full request pipeline. Code implementing a
business rule or critical endpoint MUST NOT be merged without its
corresponding tests (see the Testing Philosophy section for how this is
enforced day to day).

**Rationale**: A leave-management system directly affects pay and compliance;
untested business rules (e.g., accrual, carry-over, overlapping-request
checks) risk incorrect balances or unauthorized approvals. Acceptance
criteria defined up front prevent scope drift and give tests a concrete
target.

### VIII. Asynchronous Execution & Observability

All I/O-bound operations (database access via EF Core, external calls, file
access) MUST use asynchronous programming (`async`/`await`) end-to-end — no
blocking calls (`.Result`, `.Wait()`, synchronous-over-asynchronous wrappers)
in Application, Infrastructure, or Presentation code. The application MUST be
observable by default — structured logging, correlation, and health signals
are baseline requirements, not optional polish (see the Observability
section for the concrete mechanisms required).

**Rationale**: Async I/O is required for scalable request handling under
concurrent load (e.g., end-of-year leave submission spikes). Observability
is the primary tool for diagnosing production issues without attaching a
debugger.

### IX. Security by Design

Authentication MUST use JWT bearer tokens. Authorization MUST be role-based,
and policy-based where role alone is insufficient, enforced declaratively on
every protected endpoint and, where relevant, on the command/query handler
itself — authorization MUST NOT rely solely on UI-level restrictions.
Security controls are enforced on the server; the client is never trusted
(see the Security section for the full set of required controls).

**Rationale**: Leave data (medical/personal leave reasons, manager approvals)
is sensitive; authentication and authorization must be enforced consistently
at the API and application boundary, not assumed from client behavior.

### X. Architecture Exists to Support the Business

Every architectural decision, pattern, and abstraction in this codebase MUST
be traceable to a real business need — correctness, maintainability,
security, performance, or compliance for NovaLeave's leave-management domain.
Pragmatism takes precedence over dogma: business value over unnecessary
architecture, readability over cleverness, simplicity over complexity, and
long-term maintainability over short-term elegance. When principles in this
document appear to conflict, the conflict is resolved in favor of the
business outcome and long-term maintainability, not architectural purity for
its own sake.

**Rationale**: A constitution that optimizes for its own rules over the
product it governs has failed at its job. This principle is the tie-breaker
when the letter of another principle and the spirit of the project diverge.

## Project Structure

Every feature MUST fit within the following standard solution layout:

```text
src/
    Domain/
    Application/
    Infrastructure/
    Presentation/
tests/
docs/
docker/
.github/
```

Introducing a new top-level folder outside this layout requires the same
justification as any other architectural deviation (Principle II and the
Governance complexity-justification rule).

## Vertical Slice Architecture

Within `Application/`, code MUST be organized by feature/use case, not by
technical artifact type:

```text
Application/
    LeaveRequests/
        Create/
        Approve/
        Reject/
```

Each feature folder owns its own Command (or equivalent per Principle V),
Query, Handler, Validator, DTOs, mapping, and — where applicable — tests.
Global technical folders that span multiple features (`Commands/`, `Queries/`,
`Handlers/`, `Validators/`) MUST NOT be used unless there is a compelling,
documented reason.

**Rationale**: colocating everything a feature needs makes the blast radius
of a change visible at a glance and lets a feature be understood, modified,
or removed without touring the entire Application layer.

## Naming Conventions

- Types (classes, records, interfaces, enums): PascalCase.
- Local variables and parameters: camelCase.
- Asynchronous methods MUST carry an `Async` suffix.
- Use-case types are named by role and feature: `Command`, `Query`, `Handler`,
  `Validator`, `Request`, `Response`, and `Dto` suffixes identify what a type
  is (e.g., `ApproveLeaveRequestCommand`, `ApproveLeaveRequestHandler`,
  `ApproveLeaveRequestValidator`).
- REST endpoints MUST follow resource-oriented naming (e.g.,
  `POST /api/leave-requests/{id}/approve`, not
  `POST /api/ApproveLeaveRequest`) — plural nouns for collections, HTTP verbs
  for actions.

## Technology Decisions

Technology choices below are the project's defaults and MUST remain
consistent unless there is a justified architectural reason to change them
(Principle X).

**Backend**: .NET 10 · ASP.NET Core Web API · C# · Entity Framework Core ·
SQL Server · FluentValidation · Serilog.

**Architecture**: Clean Architecture · CQRS (Principle V) · Vertical Slice
Architecture · Dependency Injection.

**Authentication**: JWT Bearer Authentication · Role-Based Authorization.

**Frontend (future)**: React · TypeScript.

**Infrastructure**: Docker · GitHub Actions · Azure-ready deployment.

**API standards**:
- API versioning: public APIs MUST publish OpenAPI (Swagger) definitions and follow semantic versioning for breaking changes. Backward-incompatible API changes require a deprecation plan and a migration window (recommended minimum: 90 days) with communicated client upgrade guidance.

## Non-Functional Requirements (NFRs)

The system MUST meet the following non-functional expectations. These are baseline requirements and MUST be testable and enforced in CI/CD and operational runbooks.

- Performance: typical read/query endpoints SHOULD achieve p95 latency < 300ms under expected production load for the service tier; complex queries may be slower but documented. Performance tests for critical endpoints MUST be included before release.
- Throughput: capacity planning targets (RPS/concurrent users) MUST be documented per release and validated via load tests for major changes.
- Availability / SLA: target availability for production is 99.9% (monthly), with RTO (Recovery Time Objective) <= 1 hour for critical workflows and RPO (Recovery Point Objective) <= 15 minutes for transactional data. These targets MUST be reviewed and approved by product/ops.
- Backup & Recovery: automated backups for primary databases MUST run daily with tested restore procedures. Backups and restore runbooks MUST be documented and exercised at least annually.
- Scalability: the system MUST support horizontal scaling of stateless components and document stateful scaling strategies for databases.
- Rate limiting: public-facing endpoints MUST enforce rate limits. Default policy: 100 requests/min per client for non-privileged APIs, adjustable by environment and endpoint sensitivity; critical endpoints (auth, payroll) MAY use stricter limits.
- Security & Compliance: cryptographic defaults and key management are in the Security section. Non-functional security controls (encryption, key rotation, auditability) MUST be tested in pre-production.
- Observability & Alerting: metrics, traces, and logs (see Observability) MUST be collected, retained per policy, and connected to alerting rules that notify on-call for degraded conditions.
- Data retention: default retention windows and tenant-specific rules MUST be defined in Data Governance and enforced by tooling where possible.

These NFRs are normative: any exception requires explicit justification in a PR and a documented mitigation plan. Devices, integrations, or third-party services that affect these NFRs must be considered in the plan and validated in staging prior to production rollout.
- Entity Framework Core is the only supported ORM; SQL Server is the primary
  and only supported production database. Repository/DbContext access is
  confined to the Infrastructure layer.
- FluentValidation is the only supported validation library.
- All API error responses MUST use RFC 7807 Problem Details
  (`application/problem+json`) — validation failures, domain-rule
  violations, and unhandled exceptions all resolve to this single,
  predictable error shape.
- Public API endpoints, DTOs, and Application-layer contracts intended for
  external consumption SHOULD carry XML documentation comments sufficient to
  generate accurate OpenAPI/Swagger documentation.

## Security

- Authentication: JWT Bearer tokens.
- Authorization: role-based by default; policy-based authorization where a
  role alone cannot express the rule (e.g., "manager of this employee").
- Rate limiting: public-facing endpoints MUST apply rate limiting (see Non-Functional Requirements). Abuse protection and throttling MUST be part of the security review for any public endpoint.
- CORS: explicit origin allowlists only in production; no wildcard origins.
- Secrets & Keys: all secrets and signing keys MUST be stored in a managed secret store (e.g., Azure Key Vault). Key/certificate rotation MUST be supported and exercised; rotation procedures MUST be documented.
- Encryption: data in transit MUST use TLS 1.2+; sensitive data at rest MUST be encrypted using platform-supported encryption (transparent data encryption for SQL Server or equivalent). Field-level encryption MUST be used for sensitive PII where required by law or policy.
- Audit logging: every change to business data or status transitions MUST emit an audit record with the following minimum fields: timestamp (UTC), actor_id, actor_role, action, entity_type, entity_id, before (redacted if sensitive), after (redacted if sensitive), correlation_id, request_id. Audit logs MUST be stored in an append-only store or use write-once retention (where available). Retention policy: audit records MUST be retained per Data Governance (default retention: 7 years) and be searchable by authorized roles.
- Diagram/document access: architecture or domain diagrams that contain sensitive business rules or PII MUST be stored in a controlled location (e.g., `docs/diagrams`) and access-limited per repository/team policies; if diagrams embed PII they MUST be redacted or access-restricted.
- Least privilege: access to infrastructure, data, and deployment environments MUST follow the Principle of Least Privilege and be reviewed periodically.
- Security testing: static analysis, dependency vulnerability scans, and automated security tests (SCA, SAST) MUST run in CI. High/critical findings MUST block merges until addressed or mitigated with an approved exception and mitigation plan.

## Observability

- Structured logging via Serilog is mandatory for all layers that can fail or make a decision worth auditing. Logs MUST include structured fields for correlation_id, request_id, actor_id (if available), and environment.
- Every request MUST carry a correlation ID, propagated through logs and, if present, downstream calls, so a single request can be traced end-to-end.
- Request logging (method, path, status, duration) MUST be enabled for all Presentation entry points; sensitive payloads must be redacted from logs.
- Metrics and tracing: key business metrics (requests, errors, approvals, critical queue lengths) and distributed tracing MUST be emitted for critical workflows. Traces must be sampleable and include span and trace IDs correlated to logs.
- Alerting & SLOs: SLOs for key user-facing flows and critical endpoints (e.g., leave submission, approval, balance query) MUST be documented. Alerting rules (e.g., error rate, latency, queue depth) MUST notify on-call and include runbook links.
- Retention & access controls: logs and traces retention windows MUST be defined in Data Governance. Access to raw logs and audit trails MUST be limited to authorized roles and be auditable.
- A global exception-handling middleware MUST translate unhandled exceptions into RFC 7807 Problem Details responses and log the underlying exception without leaking sensitive data.
- Health checks MUST be exposed for the application and its critical dependencies (database, external services). Health endpoints MAY expose basic status but MUST NOT return PII or internal configuration.
- Logging MUST NEVER expose sensitive information (passwords, tokens, full payroll/medical leave details) — log identifiers and outcomes, not payloads.

## Data Governance

- Data classification: define data classes (Public, Internal, Sensitive/PII, Regulated) and document examples for each class in `docs/data-governance.md`.
- Data retention & deletion: the default retention for business records and audit logs is 7 years unless legal or regulatory requirements specify otherwise. Data subject requests (export, deletion, rectification) MUST be supported via documented processes.
- Data minimization & anonymization: store the minimal personal data necessary for business operations. Where historical analytics are required, consider anonymization or pseudonymization.
- Export & portability: provide mechanisms to export an employee's data in a machine-readable format within a documented SLA for requests (e.g., 30 days).
- Access & approvals: accessing Sensitive/PII data outside normal workflows (e.g., ad-hoc debugging) MUST require justification, approval, and be logged.
- Data residency & compliance: if deployment spans regions with different data residency requirements, flows that create or store regulated data MUST honor regional policies and be documented in release plans.
- Data governance ownership: product/ops/security MUST jointly own data retention and access policies; changes MUST be reflected in `docs/data-governance.md`.

## Code Quality

- Nullable Reference Types MUST be enabled solution-wide.
- Warnings MUST be treated as errors in build configuration.
- An `.editorconfig` MUST define and enforce consistent formatting.
- Roslyn analyzers MUST be enabled and their findings addressed, not
  suppressed without justification.
- Methods MUST stay small and focused on a single responsibility.
- Dead code and commented-out code MUST NOT be committed.
- Business logic MUST NOT be duplicated (Principle III).

## Git Workflow

- Commits MUST follow the Conventional Commits format.
- Pull requests MUST be small and scoped to one feature or fix.
- One feature per branch.
- A pull request MUST compile and MUST pass all automated tests before
  merge.
- Architecture compliance (Clean Architecture layering, Vertical Slice
  organization, business-rule placement) MUST be part of every code review,
  not treated as a style nitpick.

## Domain Invariants

The following are Domain invariants — rules enforced by the Domain layer
itself — not client-side or UI-level validations, and MUST hold regardless
of which Presentation technology issues the request:

- Leave balance can never become negative.
- Approved requests cannot be modified.
- Cancelled requests cannot return to Pending.
- Managers cannot approve their own requests.
- HR administrators can review any request.
- Every status transition MUST generate an audit record.
- Overlapping leave requests are prohibited.
- Requests cannot be created for past dates. "Past" is evaluated against the employee's primary timezone at 00:00 (local date); the system MUST store all datetimes in UTC and translate display/validation using the employee's timezone. Requests for dates strictly earlier than the current local date are disallowed via the standard UI/API. Retroactive requests or corrections are allowed only via a designated HR adjustment workflow that records the reason, approver, and an immutable audit record.
- A request's start date must not be after its end date.
- Business rules belong exclusively to the Domain layer (Principle IV).

## Edge Cases & Concurrency

- Concurrency control: updates that change balances or status MUST be protected by transactional semantics (database transactions or unit-of-work) and optimistic concurrency controls (row versioning) where appropriate to avoid double-applies and lost updates. Compensating actions and idempotency keys SHOULD be used for external integrations.
- Overlap & race conditions: overlap checks MUST be atomic with balance deductions/approvals to prevent race conditions when multiple requests are created or approved concurrently.
- Time zones & DST: dates and times MUST be stored in UTC; employee local timezone metadata MUST be used for display and validation. The system MUST handle DST transitions, leap days, and cross-midnight requests. Partial-day/half-day requests MUST be supported and represented by a time range or fraction with clear semantics documented.
- Manager changes & delegation: if a manager changes or leaves while a request is pending, delegation rules MUST be defined (e.g., delegate to new manager, escalate to HR) and captured in the audit trail. Approvals once completed SHOULD remain effective unless reversed by a defined HR process.
- Retroactive adjustments & payroll corrections: retroactive changes that affect payroll MUST use a controlled HR adjustment workflow, require approvals, and generate audit records and, where relevant, downstream compensation/notification workflows.
- Bulk imports/migrations: imports of historic leave data MUST include conflict resolution rules (e.g., prefer newer authoritative source, map managers) and be tested in staging. Import processes MUST emit audit events for each migrated record.

## Testing Philosophy

- Business rules require unit tests that validate behavior, not implementation details — tests MUST survive a refactor that preserves behavior. Every domain rule implemented in code MUST have unit tests covering the positive and negative cases.
- Critical workflows (see Principle VII) require integration tests exercising the full request pipeline (Presentation → Application → Infrastructure). End-to-end tests that include database and messaging interactions are REQUIRED for payroll-relevant flows.
- Acceptance criteria MUST exist and be agreed upon before implementation begins, and MUST map directly to automated tests when possible.
- Automated tests are part of the Definition of Done: a pull request introducing or modifying a Domain business rule MUST include unit tests for that rule, and one introducing or modifying a critical endpoint MUST include integration tests. A PR failing this gate MUST NOT be merged.
- CI gating: unit and integration tests for changed features MUST run in CI; high-severity test regressions MUST block merges. Security scans, static analysis, and license checks MUST run in CI and block on high/critical findings.
- Performance & load testing: changes that affect critical paths (submission, approval, balance calculation) MUST include performance benchmarks. Major releases MUST include load tests validating capacity planning targets.
- UML & documentation acceptance: when domain entities or aggregates are added or materially changed, a UML class diagram (PlantUML or Mermaid) or equivalent visual model MUST be produced and committed to `docs/diagrams` or `.specify/diagrams`. The PR that changes the model MUST include the updated diagram and a short summary of the model change as part of its description. Where diagram-generation tooling is available, diagrams SHOULD be generated automatically.

Acceptance criteria checklist (minimum) for a Domain change PR:
- Unit tests covering the rule(s) added/changed (pass in CI).
- Integration test(s) for any changed critical endpoint (pass in CI).
- Updated UML diagram(s) in `docs/diagrams` or `.specify/diagrams` when entities/aggregates change.
- OpenAPI/Swagger updates if public API surface changes.
- Security scan results (no high/critical findings) or an approved mitigation plan.
- Performance benchmark if the change affects a critical path.

These items are the minimum; product owners and reviewers may add additional acceptance checks as needed for regulatory, payroll, or customer-impacting changes.
## Business Risks & Mitigations

- Payroll & compliance exposure: incorrect balances or missing audit trails can cause legal and payroll errors. Mitigation: require automated tests, audit logs, and integration tests for payroll flows; require pre-production validation with realistic data.
- Vendor lock-in: reliance on EF Core + SQL Server may complicate migrations. Mitigation: abstract data access behind repository/DbContext boundaries, maintain migration and export tools, and evaluate cloud-native alternatives when necessary.
- Observability gaps: insufficient alerts or visibility may delay detection of failures. Mitigation: define SLOs, instrument critical paths, and test alerting during drills.
- Documentation & onboarding risk: incomplete diagrams or stale docs increase onboarding time and design errors. Mitigation: require updated UML diagrams in PRs when domain models change; maintain `docs/diagrams`.
- Sensitive data leakage in docs: architectural diagrams may expose PII or business rules. Mitigation: store sensitive diagrams in controlled locations with access controls and redaction policies.

## Future Evolution

The architecture MUST support future expansion without premature
implementation of capabilities not yet needed. Potential future capabilities
include: email notifications, calendar integrations, Microsoft Entra ID
authentication, multi-company (multi-tenant) support, public APIs,
background jobs, Azure deployment, and Power BI integration. The possibility
of future extensibility MUST NOT justify unnecessary complexity today
(Principle II) — build for the feature in front of you, in a way that does
not preclude these directions.

## Governance

- Operational runbooks: runbooks for common operational tasks (restores, failovers, scaling, critical-dependency outages) MUST be maintained under `docs/runbooks/` and linked from alerting rules. Runbooks MUST include step-by-step recovery steps, verification checks, and communication templates.
- Incident response & on-call: an incident response plan and on-call rotation MUST exist. Severity definitions (SEV0/SEV1/SEV2) and responder responsibilities MUST be documented; the playbook must include escalation paths, customer communication templates, and post-incident review requirements.
- Backups & DR exercises: backup and restore procedures MUST be documented and exercised at least annually; DR exercises MUST include timeboxed recovery verification against RTO/RPO targets.
- Change approvals: changes that materially affect architecture, data handling, or SLAs MUST be approved by product and ops and documented in the PR description.
- Audit & compliance reviews: regular audits of RBAC, secrets access, and data retention policies MUST occur (frequency defined in `docs/data-governance.md`).

This constitution supersedes all other engineering conventions, style guides, and prior informal practices for NovaLeave. All pull requests and code reviews MUST verify compliance with the principles above; the Git Workflow and Testing Philosophy sections define the day-to-day enforcement mechanism.

This constitution supersedes all other engineering conventions, style
guides, and prior informal practices for NovaLeave. All pull requests and
code reviews MUST verify compliance with the principles above; the Git
Workflow and Testing Philosophy sections define the day-to-day enforcement
mechanism.

**Amendment procedure**: Amendments are proposed via PR modifying this file,
must include an updated Sync Impact Report (prepended as an HTML comment),
and require review/approval before merge. Dependent templates
(`plan-template.md`, `spec-template.md`, `tasks-template.md`, and Spec Kit
command/skill files) MUST be checked for consistency as part of the same PR.

**Versioning policy**: This constitution follows semantic versioning:
- **MAJOR** — backward-incompatible governance changes or removal/
  redefinition of an existing principle.
- **MINOR** — a new principle or materially expanded section is added.
- **PATCH** — clarifications, wording, or non-semantic refinements.

**Complexity justification**: Any deviation from these principles (e.g., a
business rule temporarily placed outside Domain, a blocking I/O call, a
folder structure outside Project Structure) MUST be explicitly justified in
the PR description and tracked for follow-up remediation; silent deviations
are not permitted.

**Compliance review**: Every plan produced via `/speckit-plan` MUST pass the
Constitution Check gate against the current version of this file before
Phase 0 research begins, and MUST be re-checked after Phase 1 design.

**Version**: 2.0.0 | **Ratified**: 2026-07-13 | **Last Amended**: 2026-07-13
