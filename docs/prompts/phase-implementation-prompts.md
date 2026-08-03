# NovaLeave — Phase Implementation Prompts (Spec Kit methodology)

> **Non-normative orchestration aid.** The authoritative sources are always
> `.specify/memory/constitution.md` (v7.0.0), `specs/001-leave-management-mvp/`
> (spec, plan, canonical `tasks.md`, data-model, contracts, quickstart,
> research), and `docs/decisions/DR-001`/`DR-002`. If anything in a prompt
> disagrees with those files, the files win. Phase headings in canonical
> `tasks.md` are the scope boundary; EPIC/TASK ranges are cited as orientation
> into the derived `tasks/` tree only.

**Methodology (agreed 2026-07-31, revised: `main` frozen forever):** `main` is
**permanently frozen** as the pre-implementation documentation baseline —
no phase work, promotion, or release ever merges into it. Implementation flows
through two tiers:

1. **Phase tier** — one scoped `/speckit-implement` run per phase on a
   short-lived branch → build + full test suite green → manual quality &
   security gate (constitution §9.3) with recorded evidence → Conventional
   Commits → small PR into the **implementation trunk
   `001-leave-management-mvp`** (the Spec Kit feature branch) → human review
   and merge → next phase.
2. **Release tier** — at team-chosen milestones (suggested: after Phase 4 /
   US2 "core demo", after Phase 7 / US5 "beta", after Phase 11 "release"),
   Prompt P11 verifies the gate evidence and proposes an **annotated tag on
   the trunk**. Releases and rollbacks are tag operations only; `main` is
   never touched.

Consequences to accept consciously: `001-leave-management-mvp` becomes the
de-facto trunk and single source of implemented truth; living documentation
(tasks.md checkboxes, handoff notes) evolves there while `main` intentionally
preserves the untouched baseline. Syncing `main` → trunk (e.g., a docs hotfix
made on `main`) is permitted; the reverse direction is forbidden. Environments
differ by configuration, not by branch (DR-001 values; `SeedDemoUsers` never
in Production). A "test" deployment runs any trunk commit with Staging config;
"prod" deploys an approved tag on the trunk. Rollback = redeploy the previous
tag.

**Usage:** paste exactly one prompt into a fresh Claude Code session at the
repo root, with the integration branch up to date. If a run stops midway, run
`/speckit-converge` before re-running the same prompt. Never let a run touch
tasks outside its phase.

---

## Prompt P0 — Pre-flight (run once, before any implementation)

```text
You are Claude Code in the NovaLeave repo. Documentation on `main` is canonical
and complete; no code exists yet. Constitution v7.0.0 governs everything.
`main` is permanently frozen as the documentation baseline: NOTHING is ever
merged into `main` — not phase work, not releases.

1. Verify the working tree is clean and `main` matches `origin/main`.
2. Verify the implementation trunk `001-leave-management-mvp` exists on origin
   (create it from `main` if missing; sync it FROM `main` if behind — that
   direction is allowed, the reverse is forbidden). All phase PRs target this
   trunk; `main` is never a PR target.
3. Run /speckit-analyze for feature 001-leave-management-mvp and report any
   cross-artifact inconsistency between spec.md, plan.md, and tasks.md. Do not
   edit anything without asking.
4. Verify the local toolchain against quickstart.md prerequisites (.NET 10 SDK,
   SQL Server availability for integration tests) and report gaps.
5. Confirm docs/decisions/DR-001 values are reflected in quickstart.md and
   research.md (PendingRequestTimeoutDays=14, SessionTimeoutMinutes=30, jobs
   daily 00:05 UTC) and that python3 tools/validate_tasks_tree.py passes.
6. Output: a short GO / NO-GO report for starting Phase 1+2. Make no commits.
```

---

## Prompt P1 — Phases 1+2: Setup + Foundational (blocking; ≈ EPIC-001 TASK-001–019 and EPIC-002 TASK-020–058)

```text
You are Claude Code in the NovaLeave repo. Constitution v7.0.0 governs.
Implement ONLY "Phase 1: Setup (Shared Infrastructure)" and "Phase 2:
Foundational (Blocking Prerequisites)" from specs/001-leave-management-mvp/tasks.md.
`main` is the protected rollback baseline — never target it.

Setup
1. Confirm clean tree; update integration branch `001-leave-management-mvp`
   from origin; create branch `001-phase-01-02-foundation` from it.

Execute
2. Run /speckit-implement scoped strictly to Phases 1 and 2. Do not start any
   user-story task. Authority order: constitution → spec/plan/data-model/
   contracts/quickstart/research → DR-001 (config values) → DR-002 (generated
   artifacts). Test-first (Principle VII); TimeProvider only — never
   DateTime.Now/UtcNow in Domain/Application; Clean Architecture boundaries
   exactly as §3.1. Mark canonical tasks.md checkboxes as tasks complete.

Verify (checkpoint)
3. Solution builds; full test suite green; EF Core migrations apply to a clean
   database; configuration fail-fast tests pass (missing
   PendingRequestTimeoutDays or SessionTimeoutMinutes must abort startup);
   Identity + roles (User, Approver, HR) and active-status model in place;
   demo seeding only outside Production.
4. Run the manual quality & security gate (§9.3) and record evidence.

Deliver
5. Conventional Commits. Open a PR into `001-leave-management-mvp`:
   "feat(foundation): phases 1-2 — solution, domain, persistence, identity".
   Describe scope, evidence, and the §16.2 checklist. Do NOT merge — request
   human review. Report what a reviewer should sample per DR-002.
```

---

## Prompt P2 — Phase 3 / US1 (P1): User submits and manages a vacation request (≈ EPIC-003 TASK-059–077)

```text
You are Claude Code in the NovaLeave repo. Constitution v7.0.0 governs.
Implement ONLY "Phase 3: User Story 1" from
specs/001-leave-management-mvp/tasks.md. `main` is the protected rollback
baseline — never target it. Precondition: the foundation PR (Phases 1-2) is
merged and green on `001-leave-management-mvp`; verify, else stop and report.

1. Update `001-leave-management-mvp` from origin; create branch
   `001-phase-03-us1` from it.
2. Run /speckit-implement scoped strictly to Phase 3. Authority: constitution →
   specs/001 artifacts → DR-001/DR-002. Test-first; TimeProvider only; server
   derives all business values (never trust client totals/dates/balances).
3. Checkpoint: build + full suite green, plus US1 independent-test criteria
   from spec.md — create via BOTH input modes (date range; start + working
   days), server normalization, next-day minimum, weekend exclusion with
   holidays counted, zero-working-day rejection, overlap rejection, reservation
   math (available = accrued − deducted − reserved), Pending edit with full
   atomic revalidation, IDOR denial, creation+reservation+audit atomicity.
4. Run the §9.3 manual gate; record evidence. Update tasks.md checkboxes.
5. Conventional Commits; PR into `001-leave-management-mvp`:
   "feat(us1): vacation request creation and Pending management".
   Do NOT merge — request review; list DR-002 samples.
```

---

## Prompt P3 — Phase 4 / US2 (P1): Approver resolves a vacation request (≈ EPIC-004 TASK-078–093)

```text
You are Claude Code in the NovaLeave repo. Constitution v7.0.0 governs.
Implement ONLY "Phase 4: User Story 2" from
specs/001-leave-management-mvp/tasks.md. `main` is the protected rollback
baseline — never target it. Precondition: US1 merged and green on
`001-leave-management-mvp`.

1. Update `001-leave-management-mvp` from origin; create branch
   `001-phase-04-us2` from it.
2. Run /speckit-implement scoped strictly to Phase 4. Authority: constitution →
   specs/001 artifacts → DR-001/DR-002. Note constitution v6.0.1+: resolution
   requires an ACTIVE Approver with canResolveRequests=true; self-resolution is
   always denied; requesting and approving are mutually exclusive per resource.
3. Checkpoint: build + full suite green, plus US2 criteria — approve converts
   reservation to permanent deduction exactly once, atomically with transition
   and audit; reject requires a 10–500 char reason and releases the
   reservation; concurrent approvals yield exactly one winner (rowversion);
   stale operations rejected with a safe conflict outcome; inactive-Approver
   and self-resolution denials produce security events.
4. Run the §9.3 manual gate; record evidence. Update tasks.md checkboxes.
5. Conventional Commits; PR into `001-leave-management-mvp`:
   "feat(us2): approver resolution". Do NOT merge — request review; list
   DR-002 samples. After this phase merges, the team may run Prompt P11 to
   tag the "core demo" milestone on the trunk.
```

---

## Prompt P4 — Phase 5 / US3 (P2): Automatic timeout cancellation (≈ EPIC-005 TASK-094–100)

```text
You are Claude Code in the NovaLeave repo. Constitution v7.0.0 governs.
Implement ONLY "Phase 5: User Story 3" from
specs/001-leave-management-mvp/tasks.md. `main` is the protected rollback
baseline — never target it. Precondition: US2 merged and green on
`001-leave-management-mvp`.

1. Update `001-leave-management-mvp` from origin; create branch
   `001-phase-05-us3` from it.
2. Run /speckit-implement scoped strictly to Phase 5. DR-001 supplies the
   values: threshold NovaLeave:PendingRequestTimeoutDays=14; scan cadence daily
   00:05 UTC read from configuration with no code default. The System actor
   performs the transition to CancelledByTimeout; no human reason is required.
3. Checkpoint: build + full suite green, plus US3 criteria — eligible Pending
   requests transition exactly once with reservation release and system-actor
   audit naming the timeout rule; the job is idempotent and re-runnable; a
   timeout racing an approval/rejection yields exactly one successful
   transition; hosted service failures are caught and logged without stopping.
4. Run the §9.3 manual gate; record evidence. Update tasks.md checkboxes.
5. Conventional Commits; PR into `001-leave-management-mvp`:
   "feat(us3): automatic timeout cancellation". Do NOT merge — request review.
   (May be combined with Phase 6 in one PR only if the net diff stays
   reviewable per §10.)
```

---

## Prompt P5 — Phase 6 / US4 (P2): Pre-start deactivation of an approved request (≈ EPIC-006 TASK-101–107)

```text
You are Claude Code in the NovaLeave repo. Constitution v7.0.0 governs.
Implement ONLY "Phase 6: User Story 4" from
specs/001-leave-management-mvp/tasks.md. `main` is the protected rollback
baseline — never target it. Precondition: US2 merged and green on
`001-leave-management-mvp` (US3 recommended merged first to keep history
linear).

1. Update `001-leave-management-mvp` from origin; create branch
   `001-phase-06-us4` from it.
2. Run /speckit-implement scoped strictly to Phase 6. Rules: only an ACTIVE
   Approver with canResolveRequests=true who does not own the request; only
   while the vacation period has NOT begun; whole-request only.
3. Checkpoint: build + full suite green, plus US4 criteria — valid deactivation
   transitions Approved → CancelledByApprover and restores the previously
   deducted days atomically with the audit record; post-start and partial
   deactivations are rejected preserving state; deactivation racing timeout or
   any concurrent mutation yields exactly one winner; failure mid-operation
   commits nothing.
4. Run the §9.3 manual gate; record evidence. Update tasks.md checkboxes.
5. Conventional Commits; PR into `001-leave-management-mvp`:
   "feat(us4): pre-start deactivation with balance restoration".
   Do NOT merge — request review.
```

---

## Prompt P6 — Phase 7 / US5 (P2): Balance and monthly accrual (≈ EPIC-007 TASK-108–116)

```text
You are Claude Code in the NovaLeave repo. Constitution v7.0.0 governs.
Implement ONLY "Phase 7: User Story 5" from
specs/001-leave-management-mvp/tasks.md. `main` is the protected rollback
baseline — never target it. Precondition: US1 merged and green on
`001-leave-management-mvp`.

1. Update `001-leave-management-mvp` from origin; create branch
   `001-phase-07-us5` from it.
2. Run /speckit-implement scoped strictly to Phase 7. Accrual semantics per the
   resolved OQ-002: one whole day per fully elapsed calendar month since
   EmploymentStartDate; first partial month does not count; no proration; days
   never expire. Scheduler cadence per DR-001 (daily 00:05 UTC from
   configuration); idempotency via the unique (UserId, AccrualPeriod)
   constraint with a BalanceMovement ledger entry and audit record per accrual.
3. Checkpoint: build + full suite green, plus US5 criteria — accrual
   boundary tests (hire on the 1st, mid-month, month-end/31st), catch-up after
   missed runs without duplicates, balance view shows accrued / reserved /
   deducted / available consistently with the ledger.
4. Run the §9.3 manual gate; record evidence. Update tasks.md checkboxes.
5. Conventional Commits; PR into `001-leave-management-mvp`:
   "feat(us5): global balance and monthly accrual". Do NOT merge — request
   review. After this phase merges, the team may run Prompt P11 to tag the
   "beta" milestone on the trunk.
```

---

## Prompt P7 — Phase 8 / US6 (P3): Basic vacation calendar (≈ EPIC-008 TASK-117–122)

```text
You are Claude Code in the NovaLeave repo. Constitution v7.0.0 governs.
Implement ONLY "Phase 8: User Story 6" from
specs/001-leave-management-mvp/tasks.md. `main` is the protected rollback
baseline — never target it. Precondition: US1 and US2 merged and green on
`001-leave-management-mvp`. This story is P3 — confirm with the team that it
is not being deferred before starting.

1. Update `001-leave-management-mvp` from origin; create branch
   `001-phase-08-us6` from it.
2. Run /speckit-implement scoped strictly to Phase 8. Scope discipline: the
   User calendar shows only the authenticated User's own authorized requests
   (per the resolved OQ-005); Approver calendar per spec; no external calendar
   integrations (out of MVP).
3. Checkpoint: build + full suite green, plus calendar criteria — authorized
   visibility only, accessible markup (not color-only status), responsive
   behavior, navigation from event to authorized detail only (FR-024).
4. Run the §9.3 manual gate; record evidence. Update tasks.md checkboxes.
5. Conventional Commits; PR into `001-leave-management-mvp`:
   "feat(us6): basic vacation calendar". Do NOT merge — request review.
```

---

## Prompt P8 — Phase 9 / US7 (P2): HR read-only organization-wide views (≈ EPIC-009 TASK-123–136)

```text
You are Claude Code in the NovaLeave repo. Constitution v7.0.0 governs.
Implement ONLY "Phase 9: User Story 7" from
specs/001-leave-management-mvp/tasks.md. `main` is the protected rollback
baseline — never target it. Precondition: US1, US2, US5 merged and green on
`001-leave-management-mvp`.

1. Update `001-leave-management-mvp` from origin; create branch
   `001-phase-09-us7` from it.
2. Run /speckit-implement scoped strictly to Phase 9. HR boundary (constitution
   §4.3): organization-wide READ of requests, organizational calendar, balances
   and movements, and relevant audit records. HR MUST NOT approve, reject,
   deactivate, modify balances, or assign/remove roles — enforce deny-by-default
   and add negative tests for every write attempt.
3. Checkpoint: build + full suite green, plus HR criteria — org-wide reads
   authorized and paginated (lists >50 rows), sensitive reasons visible only
   through authorized views, every HR write attempt denied without data change
   and logged as a security event.
4. Run the §9.3 manual gate; record evidence. Update tasks.md checkboxes.
5. Conventional Commits; PR into `001-leave-management-mvp`:
   "feat(us7): HR read-only org-wide views". Do NOT merge — request review.
```

---

## Prompt P9 — Phase 10 / US8 (P2): HR manages approver resolution capability (≈ EPIC-010 TASK-137–145)

```text
You are Claude Code in the NovaLeave repo. Constitution v7.0.0 governs.
Implement ONLY "Phase 10: User Story 8" from
specs/001-leave-management-mvp/tasks.md. `main` is the protected rollback
baseline — never target it. Precondition: US2 and US7 merged and green on
`001-leave-management-mvp`.

1. Update `001-leave-management-mvp` from origin; create branch
   `001-phase-10-us8` from it.
2. Run /speckit-implement scoped strictly to Phase 10. The single HR write:
   toggle canResolveRequests ONLY for an identity that already holds Approver,
   requiring authorized active HR identity, explicit reason, confirmation,
   expected row version, transaction-safe persistence, and an audit record —
   all in one atomic operation (constitution §4.3).
3. Checkpoint: build + full suite green, plus US8 criteria — toggle works with
   concurrency conflict handling (stale row version → safe conflict outcome);
   an Approver with canResolveRequests=false immediately loses resolution
   queues/actions (v6.0.1 gate); toggling for a non-Approver is rejected;
   inactive HR denied; every denial produces a security event.
4. Run the §9.3 manual gate; record evidence. Update tasks.md checkboxes.
5. Conventional Commits; PR into `001-leave-management-mvp`:
   "feat(us8): approver capability management". Do NOT merge — request review.
```

---

## Prompt P10 — Phase 11: Polish, cross-cutting validation, and release readiness (≈ EPIC-011 TASK-146–153)

```text
You are Claude Code in the NovaLeave repo. Constitution v7.0.0 governs.
Implement ONLY "Phase 11: Polish & Cross-Cutting Validation" from
specs/001-leave-management-mvp/tasks.md. `main` is the protected rollback
baseline — never target it. Precondition: Phases 1-10 merged and green on
`001-leave-management-mvp`; all prior tasks.md checkboxes checked.

1. Update `001-leave-management-mvp` from origin; create branch
   `001-phase-11-release` from it.
2. Run /speckit-implement scoped strictly to Phase 11: cross-cutting
   accessibility/responsive review, security abuse-case sweep (§7.4 list),
   coverage measurement (≥80% on critical Domain/Application), full-suite run
   from NovaLeave.sln with recorded evidence, and implementation handoff notes.
3. Also run: /speckit-converge to confirm zero unimplemented approved scope,
   and python3 tools/validate_tasks_tree.py for derived-tree integrity.
4. Execute the FULL manual quality & security gate (§9.3) end to end and
   attach the evidence bundle to the PR.
5. Conventional Commits; PR into `001-leave-management-mvp`:
   "chore(release): phase 11 validation and MVP handoff". Do NOT merge —
   request review. After it merges, run Prompt P11 for the final "release"
   milestone tag on the trunk.
```

---

## Prompt P11 — Milestone release tag on the trunk (run only at approved milestones)

```text
You are Claude Code in the NovaLeave repo. Constitution v7.0.0 governs.
`main` is permanently frozen as the documentation baseline — NOTHING is ever
merged or pushed into it, including by this flow. Releases are annotated tags
on the implementation trunk `001-leave-management-mvp`. Run this only when the
team has approved a milestone (suggested: after Phase 4 / US2 "core demo";
after Phase 7 / US5 "beta"; after Phase 11 "release").

1. Verify: working tree clean; `001-leave-management-mvp` up to date with
   origin; all milestone-scoped phase PRs merged; build + full test suite
   green on the trunk; and the §9.3 manual-gate evidence for the milestone is
   complete. If anything is missing, stop and report.
2. Confirm the trunk strictly contains `main` (every `main` commit is present
   in the trunk). If `main` has newer documentation commits, sync `main` →
   trunk first — that direction is allowed; NEVER merge or push the trunk
   into `main`.
3. Propose (do not push) the annotated tag on the trunk's current commit —
   e.g. v0.1.0-core-demo, v0.2.0-beta, v1.0.0-mvp — with a tag message that
   summarizes the included phases/user stories, links the gate evidence, and
   states the rollback procedure: redeploy the previous tag.
4. A human reviews and pushes the tag. Production deploys tags only; rollback
   is redeploying the prior tag. `main` remains untouched throughout.
```
