# NovaLeave MVP — Open Questions for the Product Owner

**Feature**: `001-leave-management-mvp`
**Prepared**: 2026-07-16
**For**: Friday spec review with the Product Owner
**Source**: A demanding-PO critical read of [`specs/001-leave-management-mvp/spec.md`](../specs/001-leave-management-mvp/spec.md).

## How to use this document

The critical review surfaced 19 points. I resolved 5 of them myself with defensible defaults
(recorded in the spec as **PD-017 through PD-021** and summarized in the **Appendix** below —
please veto any you disagree with). Of the remaining 14, a `/speckit.clarify` session on
**2026-07-16** resolved **three** (Q1, Q3, and Q7 — marked ✅ below), leaving **11 genuine
business, policy, legal, or org-structure decisions** that need your call. They are grouped by impact:

- **Rule-affecting** (now **Q2 and Q4**; Q1 and Q3 resolved): answering against the current spec
  default would change a business rule or user story and should be settled before build starts.
- **Launch-blocking policy** (Q5–Q6, Q8–Q10; **Q7 resolved**): needed before the MVP goes to
  production, but do not block starting the core workflow.
- **Confirm-and-defer** (Q11–Q14): lower urgency; a reasonable default exists and is noted.

Each item lists the **question**, **why it matters**, the **current spec assumption**, and my
**recommendation** so the meeting can be a yes/no/adjust rather than an open design session.

---

## Rule-affecting — settle before build

### Q1. Can approved leave be cancelled or withdrawn?
> **✅ RESOLVED — /speckit.clarify 2026-07-16**: Keep `Approved` **final** for the MVP; no withdrawal,
> and the balance deduction is permanent (recorded in the spec Clarifications section and as an
> explicit out-of-scope assumption). Reopen only if you want a withdrawal flow.
- **Why it matters**: Today `Approved` is a final state (BR-007, BR-011). Once approved, the balance
  is deducted and there is **no way to give it back** inside the MVP — no cancellation, no reversal.
  A common real case (employee's plans change, trip cancelled) has no path except a manual DB fix.
- **Current spec assumption**: Approved is final; balance deduction is permanent for the MVP.
- **Recommendation**: Confirm this is acceptable for launch. If not, we need a scoped "withdraw
  approved leave" flow that restores balance and audits the reversal — that is new user-story work.

### Q2. Should Medical Leave be exempt from the overlap block?
- **Why it matters**: BR-019 / PD-005 block a new request that overlaps **any** existing `Pending`
  or `Approved` request **regardless of leave type**. That means an employee who is on approved
  Vacation and then falls ill cannot file overlapping Medical Leave.
- **Current spec assumption**: All leave types block overlap uniformly.
- **Recommendation**: Decide whether Medical Leave (or any type) may overlap other leave. This is a
  policy call with balance/attendance implications; I did not want to weaken the invariant silently.

### Q3. Must a reason be mandatory for every leave type — including Vacation?
> **✅ RESOLVED — /speckit.clarify 2026-07-16**: Keep the reason **mandatory (10–500 chars) for all
> types**, confirming FR-002 / PD-008 (recorded in the spec Clarifications section). Reopen if legal
> advises that Vacation should not require a justification.
- **Why it matters**: FR-002 / PD-008 require a 10–500 character reason for **all** requests. In
  several jurisdictions employees are not required to justify vacation, and a forced free-text
  minimum can invite either junk text or a privacy concern.
- **Current spec assumption**: Reason mandatory (10–500 chars) for every type.
- **Recommendation**: Confirm mandatory-for-all, or make the reason optional / shorter-floor for
  Vacation and Personal Leave while keeping it for exceptional cases.

### Q4. Single Mon–Fri workweek and one org-wide holiday calendar — correct for a multi-region workforce?
- **Why it matters**: BR-004 hard-codes Monday–Friday as working days and a **single** authoritative
  holiday calendar, yet BR-003 evaluates dates in **each employee's own time zone**, implying a
  multi-region workforce. Different regions have different weekends (e.g., Fri–Sat) and different
  public holidays. If employees are international, requested-unit math will be wrong for some of them.
- **Current spec assumption**: One Mon–Fri workweek, one org-wide holiday calendar.
- **Recommendation**: Confirm the workforce is single-region for the MVP (default OK), or tell us we
  need per-region workweeks and holiday calendars — which changes the unit-calculation rule.

---

## Launch-blocking policy — settle before production

### Q5. What are the default balance entitlements, and do they reset annually?
- **Why it matters**: The spec assumes "an authoritative source provides one balance per employee
  per balance-consuming type," but accrual/expiry/carryover are out of scope, so the number is a
  **static seeded figure** (see PD-021). We need the actual entitlement values to seed data and set
  expectations, and whether the figure represents an annual allotment (and if/when it resets).
- **Current spec assumption**: Balances exist and are seeded; values and period semantics undefined.
- **Recommendation**: Provide per-type default day counts (by grade/tenure if relevant) and confirm
  whether the MVP treats them as a fixed remaining balance with no reset.

### Q6. Is Medical Leave truly uncapped, and is documentation required?
- **Why it matters**: Medical Leave is non-balance-consuming (PD-002), so nothing caps it — an
  employee could request an arbitrarily long medical absence with no balance check. Many orgs cap
  paid sick leave or require a doctor's note beyond N days (legal/compliance).
- **Current spec assumption**: Medical Leave is unlimited and requires only the standard reason.
- **Recommendation**: Confirm uncapped, or define a cap / documentation threshold (may pair with Q11).

### Q7. What happens when a manager is unavailable — is there any fallback approver?
> **✅ RESOLVED — /speckit.clarify 2026-07-16**: Added an explicit **delegate (alternate) Direct
> Manager** approver (PD-022, AC-039). HR stays read-only. Delegation is sourced from the authoritative
> org system (not automatic). This is a scope addition `plan.md` and tests must reflect. Still open:
> confirm the org source can actually supply delegate relationships, and that every manager (Q8) has
> either a primary or delegate approver.
- **Why it matters**: EC-006 leaves a request `Pending` **indefinitely** when there is no authorized
  manager (vacant role, manager on leave, ambiguous assignment), and HR has no write/override power.
  Delegation is explicitly out of scope. So legitimate requests can get permanently stuck.
- **Current spec assumption**: No fallback; the request waits until org data provides a manager.
- **Recommendation**: Confirm poll-and-wait is acceptable for launch, or decide the minimum fallback
  (delegate, second approver, or a controlled HR override).

### Q8. Who approves a manager's own leave request?
- **Why it matters**: A person can be both Manager and Employee. If a manager has no upline manager
  in the org source, their own requests hit the Q7 stuck-request case. The spec does not state that
  every manager has an approver.
- **Current spec assumption**: Unspecified; relies on org source having an upline manager.
- **Recommendation**: Confirm the org hierarchy guarantees an approver for every manager, or accept
  that manager requests may stall (ties to Q7).

### Q9. Is HR allowed to read every employee's free-text reason org-wide, including medical context?
- **Why it matters**: SEC-005 lets HR read every request reason and rejection reason across the whole
  organization. Reasons can contain sensitive personal or medical information. Broad HR visibility of
  free text may conflict with privacy/compliance expectations.
- **Current spec assumption**: HR sees all reasons org-wide through read-only views.
- **Recommendation**: Confirm with privacy/compliance, or restrict reason visibility (e.g., mask for
  some HR roles, or hide reasons for Medical Leave).

### Q10. Is a no-notifications MVP acceptable?
- **Why it matters**: Notifications are out of scope, so a manager only learns of a pending request —
  and an employee only learns of approval/rejection — by **checking the app**. That is a real
  usability gap that can stall the whole workflow.
- **Current spec assumption**: Poll-only; no email or in-app notification at launch.
- **Recommendation**: Confirm poll-only is acceptable for the first release, or add at least email
  notification for "request pending" and "request resolved" to scope.

---

## Confirm-and-defer — reasonable default exists

### Q11. Is there a maximum advance horizon and a maximum span per request?
- **Why it matters**: There is no upper bound on how far ahead leave may be booked, nor a max number
  of days per request. Balance caps consuming types, but Medical Leave (Q6) has no cap at all.
- **Current spec assumption**: No upper bounds.
- **Recommendation (provisional)**: Add sanity guardrails — e.g., start date no more than 12 months
  ahead, single request span no more than ~90 calendar days — pending your numbers. Provisional only;
  not baked into the spec because the values are policy.

### Q12. What are the audit and leave-history retention obligations?
- **Why it matters**: Audit records are immutable (AUD-005) but the spec gives no retention period or
  deletion/anonymization policy (e.g., GDPR right-to-erasure for departed employees).
- **Current spec assumption**: Retained indefinitely; no deletion policy.
- **Recommendation**: Provide the compliance retention period and any anonymization requirement.

### Q13. Which systems are the authoritative sources, and who keeps them current?
- **Why it matters**: The MVP has **no in-app administration** (see PD-018); org structure/assignments,
  time zones, the holiday calendar, the Leave Type catalog, and initial balances all come from
  external authoritative sources. We need to name those systems and their owners, or someone will
  assume there are admin screens in scope.
- **Current spec assumption**: External authoritative sources exist and are read-only inputs.
- **Recommendation**: Name the source-of-truth system and owner for each reference dataset.

### Q14. What are the expected volume and scale figures?
- **Why it matters**: Success criteria are all correctness; there are no scale targets. Rough
  employee/manager counts and requests-per-period help validate seed data and performance assumptions.
- **Current spec assumption**: Unspecified.
- **Recommendation (low priority)**: Provide ballpark headcount and monthly request volume.

---

## Appendix — decisions I already resolved (please veto if wrong)

These were underspecified points I settled with defensible defaults and recorded in the spec as
policy decisions. They are low-risk clarifications, not new business policy — but you can override
any of them on Friday.

| ID | Decision | Rationale |
|---|---|---|
| **PD-017** | Requested units and balances are **whole numbers** of working days (non-negative integers). | Partial/hourly requests are already out of scope; removes any ambiguity about fractional balances. |
| **PD-018** | **No in-app administration** in the MVP. Leave Types, holiday calendar, teams/assignments, and initial balances are provisioned from external authoritative sources and treated as read-only inputs. | Consistent with the existing "authoritative source" assumptions and out-of-scope list; prevents anyone assuming CRUD/admin screens are in scope. Depends on Q13. |
| **PD-019** | The MVP user interface is **English-only**. | Low-risk, easily revisited; does not affect any business rule. Revisit if the workforce is multilingual (relates to Q4). |
| **PD-020** | The not-in-the-past rule (BR-002) is enforced **at submission only**. A request whose start date passes while it is still `Pending` **may still be approved**; approval revalidates balance and overlap but not the past-date rule. | The employee submitted validly and on time; blocking would penalize them for the manager's delay. Makes existing behavior unambiguous. The broader "stuck request" concern is Q7. |
| **PD-021** | Within the MVP, an applicable balance is a **static seeded entitlement** with no accrual, expiry, carryover, or period reset inside this feature. | Accrual/expiry/carryover are already out of scope; this only makes the meaning of the stored number explicit. The entitlement **values** and reset expectation remain a business decision — Q5. |
