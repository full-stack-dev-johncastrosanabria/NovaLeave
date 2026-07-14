<!--
Sync Impact Report
==================
Version change: 2.1.0 → 2.2.0

Added sections:
- Knowledge, Context, and Diagram Tooling (new section after Data Governance)
  with comprehensive governance rules for:
  - Authoritative sources and authority order (constitution → specs → ADRs/plans
    → source code/tests → derived artifacts)
  - Graphify governance (code analysis, derived context, stale-graph prevention,
    no runtime dependency)
  - Open Knowledge Format governance (curated knowledge layer with source
    provenance, no silent introduction of new rules)
  - Mermaid governance (preferred text-based diagram format with specific
    diagram types per purpose)
  - AI context-efficiency rules (focused retrieval, source verification)
  - Security and privacy rules (no credentials, sensitive information handling)
  - Derived-artifact consistency rules (contradictions must be reported)
  - Tooling scope (development/documentation tools only, not runtime
    dependencies)

Updated sections:
- Governance → added amendment procedure for rules governing Graphify, OKF, or
  Mermaid; clarified that use of these tools requires justified value and
  remains proportional to change size/risk.

Preserved sections:
- All existing principles (I–X) remain unchanged.
- All existing architecture, naming, technology, security, NFR, and operational
  sections unchanged.
- Core Principles, Clean Architecture, CQRS, DDD, SOLID, observability, OWASP
  baseline, testing, and all prior amendments remain in effect.
- Security, Data Governance, Code Quality, Git Workflow, Domain Invariants,
  Edge Cases, Testing Philosophy, Business Risks, and Future Evolution
  unchanged.

Architecture & Application:
- No runtime dependencies added (Graphify, OKF, Mermaid are tooling only).
- No MVP functionality changed.
- No business rules added or modified.
- No technology stack decisions affected.
- No application feature scope changes.

Templates & Skills:
- ⚠ .specify/templates/spec-template.md — review for consistency with new
  governance rules (optional; no mandatory changes).
- ⚠ .specify/templates/plan-template.md — review for consistency with new
  governance rules (optional; no mandatory changes).
- ⚠ .specify/templates/tasks-template.md — review for consistency with new
  governance rules (optional; no mandatory changes).
- ✅ Spec Kit command/skill files (.claude/skills/speckit-*) — reviewed, no
  updates needed.

Follow-up TODOs: None. All governance rules for Graphify, OKF, and Mermaid
integrated. Version bumped to 2.2.0 per semantic versioning (MINOR: new
expanded governance section for derived tooling artifacts added).
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
Security controls are enforced on the server; the client is never trusted.

NovaLeave adopts the following OWASP Top 10:2025 categories as its primary
application-security baseline:
- **A01:2025 — Broken Access Control**: access control is enforced
  server-side at both the Presentation and Application boundaries; frontend
  controls are never treated as security controls.
- **A06:2025 — Insecure Design**: security controls are designed during
  specification and planning, with threat modeling and security acceptance
  criteria established before implementation.
- **A09:2025 — Security Logging and Alerting Failures**: all security-relevant
  and business-critical events produce structured audit logs, with alerting
  configured to detect and notify on suspicious activity.

These categories are mandatory because NovaLeave processes sensitive employee
information (medical/personal leave reasons), enforces manager-to-employee
relationships with approval authority, modifies leave balances that affect
payroll, and records approval decisions that create compliance obligations.
See the OWASP Top 10:2025 Security Baseline section for the detailed control
requirements for each category.

**Rationale**: Leave data is sensitive and affects payroll and compliance;
authentication and authorization must be enforced consistently at the API and
application boundary, not assumed from client behavior. Systematic security
design and audit logging are essential for correctness and compliance in a
payroll-relevant system.

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

## OWASP Top 10:2025 Security Baseline

### A01:2025 — Broken Access Control

**Core Requirements**:
- Access control MUST be enforced server-side at both the Presentation and Application boundaries.
- Frontend controls, hidden routes, disabled buttons, client-supplied roles, or client-supplied ownership identifiers MUST NOT be treated as security controls.
- Authorization MUST follow deny-by-default behavior.

**Authorization Validation**:
Every protected use case MUST validate:
- The authenticated user's identity.
- The authenticated user's role.
- Ownership of the requested resource.
- The user's organizational relationship to the resource.
- Any resource-specific policy required by the business workflow.

**Minimum Authorization Rules**:
- Employees MAY access only their own leave requests and balances.
- Managers MAY view, approve, or reject requests only for employees currently assigned to them.
- HR users MAY access organization-wide information only through explicitly authorized HR use cases.
- No user MAY approve or reject their own leave request.

**Client Input Trust Boundaries**:
- Request IDs, employee IDs, manager IDs, roles, and ownership information supplied by a client MUST NOT be trusted without server-side verification.
- Authorization MUST be revalidated immediately before every state-changing operation.

**JWT Validation**:
- JWT issuer, audience, signature, expiration, and required claims MUST be validated.

**Error Handling**:
- Authorization failures MUST fail closed.
- Authorization errors MUST NOT reveal whether an inaccessible resource exists.

**Testing Requirements**:
Automated tests MUST cover:
- Anonymous access (no token, expired token, invalid token).
- Incorrect-role access (user with wrong role attempting resource access).
- Cross-user data access (one employee attempting to access another's requests).
- Cross-manager access (manager attempting to access requests outside their team).
- Direct-object-reference manipulation (guessing or altering resource IDs).
- Privilege escalation attempts (user self-assigning elevated roles).
- Attempts to approve one's own request.
- Direct API access that bypasses the frontend.
- Expired, invalid, or incorrectly issued JWTs.

### A06:2025 — Insecure Design

**Design-Time Security**:
Security controls MUST be designed during specification and planning, not added only after implementation.

**Security-Sensitive Feature Specifications**:
Every security-sensitive feature specification MUST document:
- Authorized actors (who can perform the action).
- Protected assets (what data/state is at risk).
- Sensitive data involved (PII, financial data, etc.).
- Trust boundaries (which components are trusted; where does the system accept external input).
- Allowed state transitions (valid workflow paths).
- Prohibited state transitions (invalid or dangerous paths).
- Successful flows (normal operation).
- Failure flows (error handling).
- Abuse and misuse cases (intentional and unintentional misuse scenarios).
- Concurrency risks (race conditions, double-applies).
- Replay or duplicate-operation risks (preventing repeated submission of the same request).
- Required audit events (what must be logged).
- Security acceptance criteria (testable security requirements).

**Threat Modeling**:
Threat modeling MUST be performed for at least:
- Authentication (login, token refresh).
- Leave request creation (data entry, validation).
- Leave approval (authorization, state transition).
- Leave rejection (authorization, state transition).
- Leave balance changes (calculations, concurrency).
- Administrative HR operations (bulk imports, corrections, role management).
- Any retroactive adjustment workflow (historical data changes).
- Any future payroll-related integration (if implemented).

**Domain & Application Behavior Prevention**:
Domain and Application behavior MUST prevent:
- Negative leave balances.
- Unauthorized approval or rejection.
- Duplicate approval or rejection.
- Replayed state-changing requests.
- Invalid state transitions.
- Concurrent double approval.
- Unauthorized retroactive changes.
- Trusting client-calculated balances.
- Trusting client-calculated requested days.
- Trusting client-supplied roles or ownership.
- Bypassing business rules through direct endpoint access.

**Specification Requirements**:
For every critical workflow, the specification MUST contain security acceptance criteria before implementation starts.
Security-sensitive design decisions MUST be documented in the relevant specification, plan, threat model, ADR, or pull request.

### A09:2025 — Security Logging and Alerting Failures

**Audit and Security Logging**:
Security-relevant and business-critical events MUST produce structured audit or security logs.

**Required Events**:
The system MUST log, when applicable:
- Successful authentication.
- Failed authentication.
- Authorization failures (access denied).
- Leave request creation.
- Leave approval.
- Leave rejection.
- Leave cancellation (if cancellation becomes part of an approved specification).
- Administrative leave balance changes.
- Invalid state-transition attempts.
- Repeated validation failures (potential attack).
- Suspicious direct-object-reference attempts.
- Privilege or security-configuration changes.
- Security-sensitive administrative actions.

**Log Structure and Content**:
Security and audit records MUST include, where applicable:
- UTC timestamp (ISO 8601 format).
- Actor identifier (user ID, service principal, etc.).
- Actor role (Employee, Manager, HR Admin, System, etc.).
- Action (the operation performed).
- Entity type (LeaveRequest, LeaveBalance, Employee, etc.).
- Entity identifier (resource ID affected).
- Result or outcome (success/failure, reason for failure).
- Correlation identifier (to trace related events).
- Request identifier (to correlate with HTTP logs).
- Source IP or equivalent request-origin information where legally and operationally appropriate.

**Log Redaction and Protection**:
Logs MUST NOT contain:
- Passwords.
- Password hashes.
- Access tokens.
- Refresh tokens.
- JWT signing keys.
- Secret values or configuration secrets.
- Complete medical or personal leave reasons (redact or anonymize).
- Unredacted sensitive personal information (SSNs, full medical details, etc.).
- Full request payloads containing sensitive information.

Business audit records MUST be protected against unauthorized modification or deletion.

**Production Alerting**:
Production environments MUST define alerting rules for:
- Repeated failed authentication (e.g., 5+ failures in 5 minutes from same IP).
- Repeated authorization failures (e.g., 10+ access-denied events in 10 minutes).
- Suspicious access to multiple employee records (e.g., one user accessing 50+ employees in 1 hour).
- Repeated invalid state-transition attempts (e.g., attempts to approve an already-approved request).
- High-risk administrative changes (role changes, bulk imports, retroactive adjustments).
- Unexpected spikes in rejected requests or server errors (potential DDoS or application failure).

Alerts MUST contain enough context for investigation without exposing sensitive information.

**Testing Requirements**:
Automated tests MUST verify:
- Required events are generated for critical operations.
- Failed security operations are logged.
- Correlation and request identifiers are included in logs.
- Sensitive information is redacted in logs.
- Business audit records are generated for critical state changes.

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

## Knowledge, Context, and Diagram Tooling

This section establishes governance rules for Graphify, Open Knowledge Format (OKF), and Mermaid as development, documentation, and context-management tools. These tools are **NOT** runtime dependencies, **NOT** core architectural principles, and their use is **optional** unless explicitly required by an approved feature plan or architecture decision.

### Authoritative Sources and Authority Order

Approved feature specifications, architectural decisions, the current constitution, approved plans, and source code remain the authoritative sources for NovaLeave. The authority order for decision-making and implementation is:

1. **The current constitution** — for project-wide governance, principles, and compliance rules.
2. **Approved feature specifications** — for functional behavior, acceptance criteria, and scope.
3. **Approved architecture decision records (ADRs) and technical plans** — for architectural choices, implementation strategy, and design trade-offs.
4. **Source code and automated tests** — for the currently implemented behavior, business rules, and constraints as they actually execute.
5. **OKF knowledge pages, Mermaid diagrams, and Graphify outputs** — as derived supporting artifacts that aid understanding and context.

**Critical rule**: Derived tooling artifacts (Graphify outputs, OKF knowledge entries, Mermaid diagrams) **MUST NOT** override an authoritative source. If authoritative sources conflict with each other, the conflict **MUST** be explicitly reported and resolved before implementation continues. Derived artifacts that contradict approved sources MUST be corrected, marked stale, or re-evaluated for accuracy.

### Graphify Governance

Graphify MAY be used to analyze code structure, dependencies, relationships, impact areas, and documentation connections as a derived code-analysis and context-discovery tool.

**Use and limitations**:
- Graphify output MUST be treated as derived context for exploration and understanding rather than as an independent source of truth.
- Before modifying code based on Graphify results, engineers or AI agents MUST verify the relevant source files, tests, and specifications.
- Graphify-generated information MUST NOT silently introduce: new business rules, new actors, new request states, new permissions, new dependencies, new architectural decisions, or new MVP functionality. Any such change requires specification and approval.
- A Graphify graph MAY become stale after source-code or documentation changes. Any decision or code change based on the graph MUST verify that the graph reflects the repository version being analyzed and the changes being implemented.
- Code-only local analysis via grep, codebase navigation, and static imports/exports SHOULD be preferred when it provides sufficient context and avoids external tool overhead.
- Semantic processing of documentation through an external model or API MUST remain optional and requires organizational approval if project-sensitive information may be transmitted outside approved boundaries.
- API keys, tokens, credentials, or secrets used by Graphify MUST NEVER be committed to the repository.
- Graphify failures, missing graphs, stale graphs, or unavailable semantic backends MUST NOT block the NovaLeave application from building, testing, running, or deploying.

### Open Knowledge Format Governance

Open Knowledge Format (OKF) MAY be used as a curated project-knowledge and navigation layer to summarize, link to, and contextualize authoritative sources.

**Content and provenance**:
- OKF content MUST summarize and link to authoritative sources (specifications, plans, ADRs, source files, tests) rather than duplicate entire documents.
- Every OKF knowledge entry MUST preserve source provenance by identifying the authoritative files or decisions from which it was derived (e.g., "based on spec#123" or "references src/domain/LeaveRequest.cs").
- OKF content MUST clearly distinguish: confirmed facts, approved decisions, assumptions, open questions, and deprecated information.
- OKF content MUST NOT silently introduce: new business requirements, new authorization rules, new request states, new actors, new architectural dependencies, new security controls, or new MVP functionality. Any such addition requires specification and approval.
- When OKF content conflicts with an authoritative source, the authoritative source takes precedence and the knowledge entry MUST be corrected or marked as stale with a reference to the authoritative source.
- OKF knowledge retrieval SHOULD use focused queries and bounded context where supported, so AI agents and engineers do not read broad portions of the repository unnecessarily.
- The unavailability or failure of OKF tooling MUST NOT block application execution, builds, tests, or deployment.

### Mermaid Governance

Mermaid is the preferred text-based format for version-controlled architecture, workflow, state, sequence, domain, and data-model diagrams when diagrams are required. Mermaid files MUST be committed to the repository for version control and design review.

**Diagram representation and accuracy**:
- Mermaid diagrams MUST represent information already supported by approved specifications, plans, architecture decisions, data models, or source code.
- Mermaid diagrams MUST NOT invent: actors, states, state transitions, relationships, cardinalities, endpoints, database tables, dependencies, business rules, or authorization permissions.
- When source information is incomplete or contradictory, the diagram MUST identify assumptions or unresolved questions instead of guessing.

**Diagram types and scope**:
The following Mermaid diagram types SHOULD be used according to their purpose:
- **flowchart** — for workflows, decisions, and high-level architecture flows.
- **stateDiagram-v2** — for request states and state transitions (e.g., leave request lifecycle).
- **sequenceDiagram** — for interactions between actors and system components.
- **classDiagram** — for domain entities and their relationships.
- **erDiagram** — for persistence models and database schema.

Mermaid diagrams MUST remain focused and readable. Large diagrams SHOULD be divided into smaller diagrams representing separate concerns (e.g., one diagram per aggregate or request state machine).

**Authority and maintenance**:
- Approved specifications and architecture decisions always take precedence over Mermaid diagrams.
- When an approved source changes, affected diagrams SHOULD be reviewed and updated as part of the same feature or pull request when practical.
- A diagram-rendering or validation failure MUST NOT block application execution. However, invalid Mermaid syntax in a diagram modified by a pull request SHOULD be corrected before merge when validation tooling is available.

### AI Context-Efficiency Rules

When AI agents or engineers need to understand or modify NovaLeave code, architecture, or specifications, the following context-efficiency rules guide retrieval and verification:

- AI agents SHOULD retrieve focused context before broadly reading the repository. The preferred context order is:
  1. Search focused, source-linked OKF knowledge (if available) for high-level overviews and navigation.
  2. Query Graphify for relevant files, dependencies, and impact areas (if available and accurate).
  3. Read the authoritative specification, plan, decision, test, or source files identified by those tools.
  4. Use Mermaid diagrams to visualize confirmed information and relationships.
  5. Expand repository exploration only when focused retrieval is insufficient.

- AI agents MUST state when:
  - A Graphify graph may be stale or reflect an older repository version.
  - An OKF knowledge entry may be incomplete or out-of-date.
  - A Mermaid diagram may be provisional or based on incomplete information.
  - An authoritative source could not be located.
  - Sources conflict and require manual reconciliation.
  - No guaranteed token-saving percentage may be claimed (these tools are intended to reduce unnecessary context retrieval, not to replace source verification).

### Security and Privacy Rules

Graphify outputs, OKF knowledge pages, and Mermaid diagrams may expose source-code structure, architectural decisions, security controls, and business rules. They MUST be handled according to the same repository-access and confidentiality policies as the underlying project.

**Prohibited content**:
These artifacts MUST NOT contain:
- API keys, access tokens, refresh tokens, or signing keys.
- Passwords or password hashes.
- Secret configuration values or environment-specific credentials.
- Real employee personal information (use anonymized or pseudonymous identifiers in examples).
- Complete medical or sensitive leave reasons (redact or anonymize sensitive PII).
- Production data or Personally Identifiable Information (PII) not already approved for repository storage.
- Confidential information not already approved for repository storage.

**Transmission and storage**:
- Sensitive information MUST NOT be transmitted to an external semantic-processing provider (e.g., for semantic analysis of documentation) without explicit organizational approval and data handling agreements.
- Generated graphs, knowledge exports, and rendered diagrams MUST remain within approved storage and repository boundaries.

### Derived-Artifact Consistency

Graphify outputs, OKF knowledge entries, and Mermaid diagrams are derived artifacts created from or about authoritative sources. When they contradict:
- The current constitution.
- An approved specification.
- An approved architecture decision.
- An approved plan.
- Source code.
- Automated tests.

The contradiction MUST be reported explicitly to the team or PR reviewers. The team MUST determine whether:
- The derived artifact is stale and requires update.
- The implementation is incorrect and needs fixing.
- The specification is outdated and needs amendment.
- An architectural decision has changed and requires an ADR update.
- A formal amendment to the constitution or governance rules is required.

Contradictions MUST NOT be silently resolved by an AI agent without human review and approval.

### Tooling Scope

Graphify, OKF, and Mermaid are development, documentation, analysis, and context-management tools. They MUST NOT become runtime dependencies of:
- **Domain** business logic or entities.
- **Application** use cases or services.
- **Infrastructure** data access or external integrations.
- **Presentation** endpoints or controllers.

Their availability MUST NOT be required for:
- Application startup or initialization.
- Request processing or handling.
- Business-rule execution.
- Database access or transactions.
- Authentication or authorization.
- Production operations or deployment.

If any of these tools becomes temporarily unavailable, the application MUST remain fully functional.

### Amendment Procedure for Tooling Governance

Changes to the rules governing Graphify, OKF, or Mermaid follow the normal constitution amendment procedure (see Governance section below). Their use is **NOT mandatory** for every feature. Use of Graphify, OKF, or Mermaid MAY be required by:
- An approved feature plan (e.g., "this complex refactor requires impact analysis via Graphify").
- An architecture decision or ADR (e.g., "all domain-model changes MUST include a Mermaid class diagram").
- A specific pull-request requirement (e.g., "add a state diagram for this new state machine").
- A documentation task (e.g., "create an OKF knowledge summary for the leave-approval workflow").

Any required use MUST remain proportional to the size and risk of the change and MUST respect the constitution's simplicity principle (Principle II).

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
- Security testing is mandatory for critical features. Every critical feature MUST include, as applicable:
  - Authentication tests (successful login, invalid credentials, expired tokens).
  - Authorization tests (role-based access, policy-based access, denial cases).
  - Resource-ownership tests (cross-user data access prevention).
  - Manager-to-employee relationship tests (delegation and approval authority).
  - Abuse-case tests (privilege escalation, self-approval, circumvention attempts).
  - Invalid-state-transition tests (prevented transitions, duplicate state changes).
  - Replay or duplicate-operation tests (idempotency, duplicate-submission prevention).
  - Concurrency tests (race conditions, double-applies under concurrent load).
  - Audit-event tests (required events are generated, correlation IDs are present).
  - Sensitive-log-redaction tests (sensitive data is not exposed in logs).
- Automated tests are part of the Definition of Done: a pull request introducing or modifying a Domain business rule MUST include unit tests for that rule, and one introducing or modifying a critical endpoint MUST include integration tests. A PR failing this gate MUST NOT be merged.
- A pull request affecting authentication, authorization, leave balances, request state transitions, HR access, or auditing MUST NOT be merged without the corresponding automated security tests and passing security scans (SCA, SAST).
- CI gating: unit and integration tests for changed features MUST run in CI; high-severity test regressions MUST block merges. Security scans, static analysis, and license checks MUST run in CI and block on high/critical findings.
- Performance & load testing: changes that affect critical paths (submission, approval, balance calculation) MUST include performance benchmarks. Major releases MUST include load tests validating capacity planning targets.
- UML & documentation acceptance: when domain entities or aggregates are added or materially changed, a UML class diagram (PlantUML or Mermaid) or equivalent visual model MUST be produced and committed to `docs/diagrams` or `.specify/diagrams`. The PR that changes the model MUST include the updated diagram and a short summary of the model change as part of its description. Where diagram-generation tooling is available, diagrams SHOULD be generated automatically.

**Acceptance criteria checklist (minimum) for a Domain change PR**:
- Unit tests covering the rule(s) added/changed (pass in CI).
- Integration test(s) for any changed critical endpoint (pass in CI).
- Updated UML diagram(s) in `docs/diagrams` or `.specify/diagrams` when entities/aggregates change.
- OpenAPI/Swagger updates if public API surface changes.
- Security scan results (no high/critical findings) or an approved mitigation plan.
- Performance benchmark if the change affects a critical path.

**Security-sensitive PR acceptance checklist (minimum) for PRs affecting authentication, authorization, leave balances, request state transitions, HR access, or auditing**:
- Server-side authorization rules are implemented and tested.
- Ownership and organizational relationships are validated on the server.
- Relevant abuse cases are tested (privilege escalation, self-approval, cross-user access, etc.).
- Required security and audit events are emitted (logged with correct structure).
- Logs do not expose sensitive information (redaction verified in tests).
- No high or critical security findings remain unresolved unless there is an approved mitigation plan.

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

**Amendment procedure**: Amendments are proposed via PR modifying this file,
must include an updated Sync Impact Report (prepended as an HTML comment),
and require review/approval before merge. Dependent templates
(`plan-template.md`, `spec-template.md`, `tasks-template.md`, and Spec Kit
command/skill files) MUST be checked for consistency as part of the same PR.
Changes to rules governing Graphify, OKF, or Mermaid also follow this
procedure and MUST include impact analysis on team workflows and tooling
adoption.

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

**Version**: 2.2.0 | **Ratified**: 2026-07-13 | **Last Amended**: 2026-07-14
