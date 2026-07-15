<!--
Sync Impact Report
==================
Consolidation scope:
- Base document: constitution.md (NovaLeave v2.2.0).
- Inputs reviewed: Constitution grupo 4, Grupo 5, Constitution Grupo 3,
  Constitution Grupo 1, and Constitution Grupo 02.
- Note: Constitution grupo 4 is substantially equivalent to the base document.

Version change: 2.2.0 -> 2.3.0
Change type: MINOR
Reason: consolidation adds material governance, UX, persistence, testability,
and development-workflow requirements without redefining the existing core
architecture, authentication model, or approved OWASP baseline.

Preserved decisions:
- .NET 10, ASP.NET Core Web API, C#, EF Core, SQL Server, FluentValidation,
  Serilog, JWT Bearer, Docker, GitHub Actions, and Azure-ready deployment.
- Clean Architecture with Domain, Application, Infrastructure, Presentation.
- Vertical Slice organization in Application.
- CQRS preferred but not mandatory; MediatR optional.
- OWASP Top 10:2025 primary baseline: A01, A06, and A09.
- Graphify, OKF, and Mermaid are derived development/documentation tools,
  never runtime dependencies or authoritative sources.

Consolidated additions:
- Explicit actor and MVP authorization matrix.
- HR read-only scope for the MVP; future adjustment workflows require an
  approved specification and separate authorization/audit controls.
- TimeProvider/IClock requirement for deterministic date rules.
- Final-state and cancellation rules, balance deduction on approval, and
  stronger persistence/concurrency requirements.
- Data-access discipline: projections, AsNoTracking, pagination, indexing,
  versioned migrations, and prohibition of EnsureCreated in production.
- Accessibility/localization requirements for current or future user interfaces.
- Human review requirements for AI-generated code.
- ADR, regression-test, code-complexity, and pull-request sizing guidance.
- Explicit list of business-policy questions that belong in feature specs and
  MUST NOT be invented by code, AI agents, diagrams, or this constitution.

Conflicts resolved:
- Presentation: retained Web API + future React; MVC/Razor-specific controls are
  conditional when browser-form or cookie-based surfaces are introduced.
- Authentication: retained JWT Bearer; Cookie Authentication/Identity is not
  adopted by this amendment.
- Physical structure: retained the existing multi-project src/ layout rather
  than the single-project MVC proposal.
- HR permissions: read-only for MVP; no balance adjustment or state transition
  is allowed unless a later approved specification adds a controlled workflow.
- Coverage: behavioral coverage of every invariant is mandatory; 80% line
  coverage is a target for critical Domain/Application modules, not a substitute
  for scenario coverage.
- Patterns: Repository/UoW, factories, specifications, state machines, and
  MediatR are used only when they solve an actual problem; no pattern is
  mandatory merely for architectural purity.
- Rules appearing in only one proposal (for example one pending request at a
  time or a medical-leave balance exception) are not made constitutional rules;
  they require an approved feature specification.

Templates requiring review:
- .specify/templates/spec-template.md
- .specify/templates/plan-template.md
- .specify/templates/tasks-template.md
- Any Spec Kit commands or skills that contain a Constitution Check.
-->

# NovaLeave — Consolidated Constitution

**Version:** 2.3.0  
**Ratified:** 2026-07-13  
**Last Amended:** 2026-07-14  
**Status:** Binding

## 1. Purpose, Scope, and Normative Language

This constitution establishes the engineering, security, quality, operations,
and governance rules for **NovaLeave**, a system for managing leave and vacation
requests. It applies to specifications, plans, ADRs, tasks, code, tests,
migrations, documentation, diagrams, pipelines, pull requests, and changes
produced by humans or AI tools.

In this document:

- **MUST / MUST NOT** indicates a non-negotiable obligation.
- **SHOULD / SHOULD NOT** indicates a strong recommendation; any deviation
  requires justification.
- **MAY** indicates a permitted option, not an obligation.

When requirements conflict, **correctness and security are entry conditions**:
neither may be sacrificed for speed, convenience, or architectural aesthetics.
Once both are satisfied, maintainability, simplicity, performance, and developer
experience are prioritized, in that order. Every decision MUST support a real
business outcome and maintainable operations.

## 2. Core Principles

### I. Clean Architecture and Inward-Pointing Dependencies

The codebase MUST be organized into four layers:

1. **Domain**: entities, value objects, aggregates, domain events, and
   invariants. It does not depend on frameworks, persistence, or presentation.
2. **Application**: use cases, contracts, input validation, and orchestration.
   It depends only on Domain and its own abstractions.
3. **Infrastructure**: EF Core, SQL Server, repositories/adapters, external
   services, and technical observability. It implements internal contracts.
4. **Presentation**: ASP.NET Core Web API and any future clients. It translates
   HTTP into use cases and contains no business rules.

Dependencies MUST point inward. Domain MUST NOT reference Application,
Infrastructure, or Presentation. Application MUST NOT reference Infrastructure
implementations. Presentation MUST NOT directly access `DbContext`, concrete
repositories, or business rules.

### II. Simplicity Before Speculative Abstraction

The simplest solution that correctly satisfies the current requirement MUST be
preferred over a generic solution designed for hypothetical scenarios. An
interface, pattern, library, configuration option, or additional layer is
introduced only when it provides a demonstrable benefit in testability,
maintainability, security, performance, or decoupling.

Repository, Unit of Work, factories, specifications, state machines, MediatR,
and other patterns MAY be used, but MUST NOT be imposed merely to appear
architecturally pure. EF Core `DbContext` MAY act as the Unit of Work when that
is sufficient.

### III. SOLID and a Single Source of Truth

The design MUST apply SOLID pragmatically. Every business rule MUST exist in one
authoritative location and be reused by every flow. Balance calculations,
overlap checks, transitions, permissions, or policies MUST NOT be duplicated
across controllers, handlers, services, clients, or queries.

### IV. Rich Domain and Business-Rule Isolation

Invariants MUST be protected by entities, value objects, aggregates, or domain
services. Application coordinates; Domain decides. Creating or transitioning an
invalid aggregate MUST be impossible or fail explicitly and predictably.

Validation is separated as follows:

- **Input validation**: format, required fields, length, and structure;
  implemented with FluentValidation.
- **Business rule**: balance, overlap, state, approval authority, and date
  policies; implemented in Domain.
- **Authorization**: identity, role, ownership, and organizational relationship;
  enforced in Presentation and revalidated in Application for sensitive
  operations.

### V. Explicit Use Cases and Pragmatic CQRS

Every business action MUST be represented by an explicit use case. CQRS is the
preferred style for separating commands and queries, but MediatR is NOT
mandatory. An application service or native handler is valid when it is simpler
and preserves equivalent boundaries and testability.

Within `Application/`, code MUST be organized by feature or vertical slice, not
in global `Commands`, `Handlers`, or `Validators` folders without documented
justification.

### VI. Dependency Injection and Testable Time

All dependencies MUST be constructor-injected through the native ASP.NET Core
container. Service locators, mutable static dependencies, and direct creation of
Infrastructure implementations from Domain or Application are prohibited.

Domain and Application MUST NOT directly use `DateTime.Now` or
`DateTime.UtcNow`. Every time-dependent rule MUST use `TimeProvider`, `IClock`,
or an equivalent abstraction to support deterministic testing.

### VII. Test-First Discipline for Critical Rules

Every feature MUST have acceptance criteria before implementation begins. Every
domain invariant MUST have positive and negative unit tests. Critical workflows
MUST have integration tests that exercise Presentation, Application, and
Infrastructure.

A change to a critical rule, authorization boundary, balance calculation, state
transition, or audit behavior MUST NOT be merged without corresponding tests.

### VIII. Asynchronous Execution and Observability by Default

All I/O MUST use `async`/`await` end to end. `.Result`, `.Wait()`, and
sync-over-async wrappers are prohibited in application code.

Structured logging, correlation, metrics, tracing, health checks, and auditing
are baseline requirements, not optional enhancements.

### IX. Security by Design

The server never trusts the client. Authentication, authorization, ownership,
hierarchical relationships, requested-day calculations, balances, and state
transitions MUST be revalidated server-side.

NovaLeave adopts the following OWASP Top 10:2025 categories as its primary
baseline:

- **A01 — Broken Access Control**.
- **A06 — Insecure Design**.
- **A09 — Security Logging and Alerting Failures**.

Other applicable security controls remain mandatory as cross-cutting practices,
even when they are not among the three prioritized categories.

### X. Architecture Serves the Business

Every technical decision MUST be traceable to correctness, security,
maintainability, performance, compliance, or business value. When two rules
appear to compete, the chosen alternative is the one that preserves invariants,
reduces risk, and is simpler to maintain.

## 3. Structure and Technology Decisions

### 3.1 Standard Structure

```text
src/
  Domain/
  Application/
  Infrastructure/
  Presentation/
tests/
  UnitTests/
  IntegrationTests/
  EndToEndTests/          # when applicable
docs/
  adr/
  diagrams/
  runbooks/
docker/
.github/
```

Any additional top-level folder requires justification in the pull request.

Example vertical organization:

```text
Application/
  LeaveRequests/
    Create/
    Approve/
    Reject/
    Cancel/
  LeaveBalances/
    GetByEmployee/
```

Each slice contains its request/command/query, handler or service, validator,
DTO, mapping, and related tests.

### 3.2 Approved Stack

- **Runtime:** .NET 10 and C#.
- **Backend:** ASP.NET Core Web API.
- **Persistence:** Entity Framework Core and SQL Server.
- **Validation:** FluentValidation.
- **Logging:** Serilog over `ILogger<T>`.
- **Authentication:** JWT Bearer.
- **Authorization:** roles, policies, and resource-based authorization.
- **Future frontend:** React + TypeScript.
- **Infrastructure:** Docker, GitHub Actions, and Azure-ready deployment.
- **Testing:** xUnit; mocks/fakes only where they provide useful isolation.

Changing an approved decision requires an ADR and a constitutional amendment
when the change affects a constitutional rule.

### 3.3 Conventions

- Code, types, properties, methods, and namespaces: English.
- Documentation and user-visible messages: Spanish, unless an approved
  localization decision states otherwise.
- Types: `PascalCase`; variables and parameters: `camelCase`; private fields:
  `_camelCase`.
- Interfaces: `I` prefix.
- Asynchronous methods: `Async` suffix.
- DTO, Request, Response, Command, Query, Handler, Validator, Repository,
  Controller, and ViewModel types MUST use the appropriate suffix.
- REST endpoints: plural resources and resource-oriented routes, for example
  `POST /api/leave-requests/{id}/approve`.
- Table names: singular, unless another convention is documented and applied
  consistently.

## 4. MVP Actors and Authorization

### 4.1 Employee

An Employee MAY create requests and view their own requests, statuses, history,
and balance. An Employee MUST NOT view another employee's data or modify a
resolved request.

### 4.2 Direct Manager

A Direct Manager MAY view and resolve requests only for employees assigned to
their team at the time of the operation. A Direct Manager MUST NOT approve or
reject their own request.

### 4.3 Human Resources

In the MVP, HR has organization-wide **read-only** access to history and
balances through explicitly authorized use cases. HR MUST NOT create, approve,
reject, cancel, or adjust requests or balances.

A future HR correction or retroactive-adjustment workflow requires an
independent specification, dedicated authorization, dual control where
appropriate, and immutable auditing. This constitution does not presume that
such a workflow exists.

### 4.4 Users with Multiple Roles

A user MAY hold more than one role. Authorization is evaluated per action and
resource; holding the manager role does not remove restrictions that apply when
the same user acts as an employee.

## 5. Invariants and Lifecycle

The following rules MUST hold regardless of client or endpoint:

1. The applicable balance can never become negative.
2. The start date cannot be later than the end date.
3. Requests for past dates are not allowed in the normal workflow.
4. “Past” is determined using the employee's primary time zone; timestamps are
   stored in UTC.
5. Overlapping requests are prohibited according to the active policy.
6. A request starts in `Pending`.
7. Permitted MVP transitions:
   - `Pending -> Approved`, by the authorized direct manager.
   - `Pending -> Rejected`, by the authorized direct manager.
   - `Pending -> Cancelled`, by the employee who owns the request.
8. `Approved`, `Rejected`, and `Cancelled` are final states in the MVP.
9. A resolved request cannot be edited or returned to `Pending`.
10. Balance is deducted only when approving a request whose leave type consumes
    balance; creating or rejecting a request does not deduct balance.
11. Every transition generates an audit record.
12. Identity, ownership, manager-employee relationship, and balance are
    revalidated immediately before a state-changing operation.
13. The requested number of days MUST be calculated server-side according to an
    approved policy; a client-calculated value is never accepted as truth.

### 5.1 Rules That Belong in Feature Specifications

The constitution does NOT invent policies that have not been approved. Each
feature specification MUST resolve, when applicable:

- Calendar days versus working days and holidays.
- Leave types and which ones consume balance.
- Medical or legal exceptions.
- Whether more than one pending request is permitted.
- Half days, hourly requests, or partial ranges.
- Accrual, expiration, and carryover of balances.
- Delegation when the direct manager changes.
- Retroactive corrections and their payroll impact.

Until an approved specification exists, code, AI, Graphify, OKF, and diagrams
MUST NOT assume an answer.

## 6. Persistence, Integrity, and Concurrency

- EF Core is the approved ORM and SQL Server is the production database.
- Data access is confined to Infrastructure.
- Schema changes MUST use versioned migrations. `EnsureCreated()` is prohibited
  in production.
- Approval, balance deduction, and auditing MUST execute within the same atomic
  transaction.
- State and balance operations MUST use optimistic concurrency (`rowversion` or
  equivalent) and handle conflicts predictably.
- Overlap verification MUST be atomic with the operation that confirms the
  request to prevent race conditions.
- Repeatable operations or external integrations SHOULD use idempotency keys or
  equivalent mechanisms.
- Requests and audit records MUST NOT be physically deleted through normal
  operations. Soft delete or immutable retention is used according to the
  record type and data policy.
- Deletion required by law or policy MUST follow an approved, auditable process
  compatible with retention obligations.
- Read queries SHOULD use projection and `AsNoTracking()`.
- N+1 access patterns are prohibited. `Include` and `Select` are used
  deliberately.
- Lists that may exceed 50 records MUST use server-side pagination.
- Foreign keys and frequently filtered columns MUST have appropriate indexes.
- No potentially large query may call `ToListAsync()` without a limit,
  pagination, or documented justification.

## 7. Security and Privacy

### 7.1 Authentication and Authorization

- JWT validation MUST verify signature, issuer, audience, expiration, and
  required claims.
- The default policy is deny-by-default.
- Every non-public endpoint MUST declare authorization.
- Application MUST verify ownership and organizational relationships; hiding a
  frontend button is not a security control.
- IDs, roles, manager IDs, employee IDs, and balances supplied by the client are
  untrusted.
- An authorization failure MUST fail closed and avoid revealing whether an
  inaccessible resource exists.

### 7.2 Input, Web, and Secure Configuration

- All external input MUST be validated using allowlists, bounds, and formats.
- EF Core or parameterized SQL is mandatory; concatenating user input into SQL
  is prohibited.
- User content is encoded or sanitized before rendering; raw HTML MUST NOT be
  used with untrusted data.
- Production CORS uses explicit origins; wildcard origins with credentials are
  prohibited.
- TLS 1.2+ and HSTS are mandatory in production.
- Secrets and keys are stored outside the repository, preferably in a managed
  secret store such as Azure Key Vault.
- Production MUST use appropriate security headers: CSP,
  `X-Content-Type-Options`, framing protection, and `Referrer-Policy`.
- Swagger UI, diagnostics, and detailed errors are not publicly exposed in
  production without explicit controls.
- Sensitive endpoints MUST apply rate limiting proportional to risk.
- If cookie authentication or Razor forms are introduced, every mutation MUST
  include anti-CSRF protection. This conditional rule does not replace JWT in
  the current architecture.

### 7.3 Sensitive Data

A request reason may contain personal or medical information and is classified
as Sensitive/PII. It may be visible only to the owner, the authorized direct
manager, and HR through approved use cases. Logs, traces, metrics, and diagrams
MUST NOT contain the complete reason or other unnecessary sensitive data.

### 7.4 OWASP A01, A06, and A09

Every security specification or critical workflow MUST document:

- authorized actors and protected assets;
- trust boundaries and sensitive data;
- permitted and prohibited transitions;
- successful flows, failure flows, and abuse cases;
- concurrency, replay, and duplication risks;
- audit events and security acceptance criteria.

Tests MUST cover anonymous access, invalid/expired tokens, incorrect roles,
cross-employee access, cross-team access, IDOR, privilege escalation,
self-approval, duplicate transitions, replay, and direct access that bypasses
the frontend.

## 8. Auditing, Logging, and Observability

- Serilog and `ILogger<T>` MUST produce structured logs.
- Every request MUST carry propagated `correlation_id` and `request_id` values.
- Request logging MUST include method, path, status, and duration without
  sensitive payloads.
- Every transition or critical change MUST record at least:
  `timestamp_utc`, `actor_id`, `actor_role`, `action`, `entity_type`,
  `entity_id`, `result`, `correlation_id`, and `request_id`.
- Where applicable, `before` and `after` values are recorded with sensitive
  fields redacted.
- Audit records MUST be protected against unauthorized modification or deletion.
- Successful and failed authentication, denials, creation, approval, rejection,
  cancellation, invalid-transition attempts, and sensitive administrative
  changes are logged.
- Logs MUST NOT contain passwords, hashes, tokens, keys, secrets, full payloads,
  or complete medical reasons.
- Health checks MUST cover the application and database without exposing PII or
  internal configuration.
- Metrics and tracing MUST instrument creation, approval, rejection, balance
  queries, errors, and latency.
- Production MUST define alerts for repeated authentication failures, anomalous
  denials, suspected IDOR, 5xx errors, latency, and high-risk administrative
  changes.

## 9. Testing and Definition of Done

### 9.1 Test Pyramid

- **Unit:** Domain and isolated use cases; no I/O.
- **Integration:** EF Core/SQL and a realistic HTTP pipeline.
- **End-to-end:** highest-risk or highest-value journeys, especially approval,
  authorization, and payroll-impacting flows.

SQLite, Testcontainers, or an isolated SQL instance MAY be used according to the
test's objective. EF Core InMemory MUST NOT be used to claim relational behavior
that the provider does not reproduce.

### 9.2 Mandatory Requirements

- Every critical rule has positive, negative, and boundary cases.
- Every bug fix includes a regression test that fails before the fix.
- Tests are deterministic and independent of execution order and the real clock.
- Tests do not call real external services.
- Concurrency changes include race-condition and double-application tests.
- Security changes include authorization, abuse-case, and redaction tests.
- Audit changes verify event creation and event structure.

The line-coverage target for critical Domain and Application modules is
**>= 80%**. Coverage does not replace the obligation to test every invariant and
every rejection scenario.

### 9.3 CI Gate

Before merge, CI MUST execute and pass:

1. restore and build;
2. formatting and `.editorconfig` checks;
3. Roslyn analyzers and nullable analysis;
4. applicable unit and integration tests;
5. coverage measurement;
6. SAST, SCA, and vulnerable-dependency scanning;
7. license verification where applicable;
8. migration and modified-Mermaid validation when tooling exists.

High/Critical findings block merge unless a formal exception includes a
mitigation, accountable owner, and expiration date.

## 10. Code Quality and Maintainability

- Nullable Reference Types MUST be enabled.
- Warnings MUST be treated as errors in Domain and Application; elsewhere, any
  exclusion requires justification.
- Methods MUST be cohesive and have a single responsibility.
- Dead code, commented-out code, temporary debugging, and `TODO` comments
  without a linked issue are prohibited.
- Cyclomatic complexity SHOULD be <= 10, and methods SHOULD remain below 40
  lines; exceptions are documented in the pull request.
- A pull request SHOULD remain below 400 net lines and cover one purpose.
  Migrations or generated code may be excluded from the measurement, but not
  from review.
- Public interfaces and contracts MUST include sufficient XML documentation.
- Comments explain intent, risk, or rationale; they do not repeat the code.

## 11. API, User Experience, and Accessibility

- All HTTP errors MUST use RFC 7807 Problem Details.
- Validation errors are actionable; technical details and stack traces are not
  shown to users.
- Public APIs MUST publish OpenAPI definitions and be versioned. A breaking
  change requires a deprecation and migration plan.
- Presentation/Application DTOs are used at boundaries; Domain entities are not
  exposed directly.
- The current or future frontend MUST apply the same capability matrix as the
  server, but it is never considered a security control.
- User interfaces SHOULD meet WCAG 2.1 AA requirements: contrast, labels,
  keyboard navigation, focus handling, and semantic HTML.
- User-visible text SHOULD be centralized for localization and consistent
  terminology.
- State-changing actions MUST provide clear and unambiguous feedback.

## 12. Performance, Availability, and Operations

### 12.1 Performance

- Read endpoints and critical flows SHOULD achieve p95 < 300 ms under expected
  load.
- Complex operations may reach p95 < 500 ms when documented and measured.
- Changes that affect critical paths MUST include a proportional benchmark or
  load test.
- RPS and concurrency targets MUST be documented for each production release.

### 12.2 Scalability

- Presentation must be stateless and suitable for horizontal scaling.
- Long-running operations MUST execute outside the synchronous request through
  approved jobs or queues when such mechanisms are introduced.
- Redis, messaging, or other infrastructure is not added until a real use case
  and approved decision exist.

### 12.3 Production Operational Targets

- Availability target: 99.9% monthly.
- RTO target: <= 1 hour for critical workflows.
- RPO target: <= 15 minutes for transactional data.
- Automated backups: at least daily.
- Restore and disaster recovery: documented and exercised at least annually.
- Runbooks: stored under `docs/runbooks/` and linked from critical alerts.

These values are production targets. During the MVP, every unimplemented
requirement MUST be recorded as a gap, risk, and evolution plan.

## 13. Data and Retention

- Minimum data classes are Public, Internal, Sensitive/PII, and Regulated.
- NovaLeave stores only the data necessary for the process.
- The default retention period for business and audit records is seven years,
  unless another approved law or policy applies.
- Extraordinary access to PII for debugging requires justification, approval,
  and auditing.
- Export, rectification, and regulated deletion MUST have documented processes.
- Historical analytics SHOULD use anonymization or pseudonymization.
- Rules are documented in `docs/data-governance.md`.

## 14. Documentation, ADRs, and Diagrams as Code

- Relevant architectural decisions MUST be recorded under `docs/adr/`.
- Rule changes MUST update specifications, tests, and documentation in the same
  pull request.
- Material entity or aggregate changes MUST update a `classDiagram` or
  equivalent model under `docs/diagrams/` or `.specify/diagrams/`.
- Workflows and states MUST use Mermaid where appropriate:
  - `flowchart` for processes and decisions;
  - `stateDiagram-v2` for lifecycles;
  - `sequenceDiagram` for interactions;
  - `classDiagram` for domain models;
  - `erDiagram` for persistence models.
- Diagrams MUST NOT invent actors, tables, relationships, states, or rules.

## 15. Graphify, OKF, Mermaid, and AI

### 15.1 Authority Order

1. Current constitution.
2. Approved feature specifications.
3. Approved ADRs and plans.
4. Executable code and tests.
5. OKF, Mermaid, and Graphify as derived artifacts.

A derived artifact never replaces an authoritative source. A conflict MUST be
reported and resolved explicitly.

### 15.2 Graphify

Graphify MAY support dependency and impact analysis. Its output is derived
context and MUST be verified against files, specifications, and tests. A stale
or unavailable graph cannot block build, test, execution, or deployment.

### 15.3 Open Knowledge Format

OKF MAY summarize and link sources while preserving provenance and clearly
distinguishing facts, decisions, assumptions, questions, and stale content. It
MUST NOT introduce new requirements or permissions.

### 15.4 Mermaid

Mermaid is the preferred format for version-controlled diagrams. A rendering
failure does not affect the application, but modified syntax SHOULD be validated
before merge.

### 15.5 AI-Assisted Development

- All AI-generated code receives human review equivalent to any other change.
- AI MUST verify authoritative sources before modifying rules.
- AI MUST NOT silently resolve contradictions or invent requirements.
- Every dependency proposed by AI requires security, licensing, and maintenance
  evaluation.
- Prompts, outputs, graphs, and knowledge pages MUST NOT contain secrets or real
  PII.
- Use of external providers for semantic analysis of sensitive information
  requires organizational approval.

## 16. Git, Pull Requests, and Governance

### 16.1 Git Workflow

- One feature or fix per branch and pull request.
- Conventional Commits is mandatory.
- Direct pushes to `main` are prohibited.
- Every pull request requires review and a green CI pipeline.
- Applicable architecture, rules, security, migrations, tests, documentation,
  and diagrams are part of code review.

### 16.2 Minimum Pull Request Checklist

- Scope and acceptance criteria identified.
- Constitution Check completed.
- Tests added and passing.
- Authorization and abuse cases reviewed where applicable.
- Migration and rollback strategy documented where applicable.
- Auditing and log redaction verified.
- OpenAPI, ADR, README, and diagrams updated where applicable.
- No unresolved High/Critical findings.
- Performance impact measured for critical paths.

### 16.3 Exceptions

Every deviation MUST include in the pull request:

- affected rule;
- technical or business reason;
- risk;
- mitigation;
- accountable owner;
- expiration date or removal condition.

Silent deviations are prohibited.

### 16.4 Amendments and Versioning

Amendments are made through a pull request, with a Sync Impact Report at the
beginning of the document and a review of dependent templates and skills.

- **MAJOR:** removes or incompatibly redefines a rule.
- **MINOR:** adds a principle or materially expands obligations.
- **PATCH:** clarifies wording without changing normative meaning.

Every `/speckit-plan` plan MUST run a Constitution Check before Phase 0 and
repeat it after the Phase 1 design.

## 17. Risks and Evolution

Primary risks include incorrect balances, unauthorized approvals, lost audit
records, PII leakage, concurrency races, stale documentation, and
overengineering. Mandatory mitigations are domain invariants, resource-based
authorization, transactions, row versioning, tests, auditing, redaction,
updated diagrams, and simplicity.

The architecture MAY evolve toward notifications, calendars, Microsoft Entra
ID, multi-company support, jobs, public APIs, Power BI, or other integrations,
but no future possibility justifies implementing complexity before an approved
need exists.

---

**Version:** 2.3.0 | **Ratified:** 2026-07-13 | **Last Amended:** 2026-07-14
