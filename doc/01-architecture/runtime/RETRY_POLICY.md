# Runtime Retry Policy

* **Document:** Runtime Architecture / Retry Policy
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how CRAI Runtime decides whether a failed or interrupted physical execution may create another `Attempt` for the same logical `WorkItem`.

Retry preserves logical work identity.

Canonical rule:

```text
Same WorkItemId
+
New AttemptId
```

Retry exists to recover from transient execution failure without:

* recreating logical work;
* reviving stale execution;
* reviving canceled execution;
* exceeding retry budgets;
* creating retry storms;
* bypassing Scheduler;
* bypassing execution authority;
* hiding Fallback selection inside Runtime Retry;
* duplicating unsafe side effects.

---

# 2. Architectural Position

```text
Attempt Ends
    |
    v
Runtime Control
    validates relevance / authority
    |
    v
Retry Policy
    evaluates retry eligibility
    |
    +--> DO_NOT_RETRY
    |
    +--> RETRY_NOW
    |
    +--> RETRY_LATER
    |
    +--> RECOVERY_ESCALATION_REQUIRED
    |
    v
New AttemptId
    |
    v
Scheduler Admission
    |
    v
Worker Execution
```

Retry Policy does NOT:

* create BusinessExecutionPlan;
* create another WorkItem when logical work is unchanged;
* choose Provider/Model/RoutePlan;
* execute Attempt;
* perform cache lookup directly;
* commit Runtime Artifact;
* commit Business result;
* commit Presentation/UI state;
* own WorkItem terminal outcome;
* bypass Runtime Control;
* bypass Scheduler.

---

# 3. Core Principle

Retry is:

```text
physical execution replacement
```

not:

```text
logical work recreation
```

Example:

```text
WorkItem W1
    |
    +--> Attempt A1
    +--> Attempt A2
    +--> Attempt A3
```

`WorkItemId` remains stable.

Every Attempt has independent identity and lifecycle.

---

# 4. Retry Ownership

```text
Runtime Control
    -> determines current relevance
       and execution authority

Retry Policy
    -> determines whether another Attempt
       is allowed and when

Scheduler
    -> determines admission

Worker / Execution Adapter
    -> executes Attempt
```

Optional recovery escalation:

```text
Retry Policy
    -> may report that ordinary Retry
       is no longer appropriate

Routing / Recovery Architecture
    -> MAY choose another execution binding

Pipeline Runtime
    -> MAY create another Attempt
```

---

# 5. Retry Vocabulary

## Retry Evaluation

Evaluation of whether another Attempt may be created.

## Retry Timing

How a permitted retry is scheduled:

```text
IMMEDIATE
DELAYED
RETRY_AFTER
```

## Retry Budget

Bounded limits controlling retry count/cost/concurrency.

## Attempt Lineage

All Attempts belonging to one WorkItem.

## Recovery Escalation

Signal that normal same-binding Retry should stop and another recovery mechanism MAY be considered.

This is NOT itself Fallback selection.

## Manual Re-execution

A new Application/user request.

It is not automatically an automatic Retry.

---

# 6. Retry Trigger

Retry evaluation MAY start after:

* transient Attempt failure;
* normalized timeout;
* worker interruption;
* temporary execution resource failure;
* recoverable process restart;
* controlled Attempt abandonment;
* normalized transient runtime error;
* explicit Runtime recovery request.

Not every trigger is necessarily an error.

---

# 7. Retry Eligibility

Retry is allowed only when all required conditions remain satisfied.

Recommended checks:

* WorkItem still exists;
* WorkItem has no accepted terminal outcome;
* ExecutionScope remains eligible;
* ExecutionRevision retains execution authority;
* cancellation has not revoked the relevant scope;
* retry budget remains;
* deadline remains useful;
* Runtime configuration permits retry;
* input ArtifactRefs remain valid;
* normalized failure is retryable;
* Runtime is not stopping;
* execution binding remains retry-compatible;
* actual resource/capacity state can support another Attempt;
* no newer accepted Attempt outcome supersedes the retry candidate.

---

# 8. Eligibility Boundary

Retry Policy SHOULD consume already-resolved inputs such as:

```text
ExecutionEligibility
ExecutionBindingViability
ResolvedPolicyConstraints
ResourceAvailabilityProjection
```

It SHOULD NOT independently interpret:

* Workspace privacy policy;
* Provider policy;
* AI routing;
* Plugin trust;
* model compatibility;
* Business semantics.

---

# 9. Retry Decisions

Recommended canonical decisions:

```text
RETRY_NOW
RETRY_LATER
DO_NOT_RETRY
RETRY_EXHAUSTED
RECOVERY_ESCALATION_REQUIRED
```

---

# 10. Why Fallback Is Not a Retry Decision

Do NOT define:

```text
RETRY_WITH_FALLBACK
```

as a Runtime Retry decision.

Correct boundary:

```text
Retry Policy
    says ordinary Retry should stop

Recovery / Routing
    decides whether another binding exists

Pipeline Runtime
    executes another Attempt if appropriate
```

---

# 11. Retry Reason Codes

Possible:

```text
TRANSIENT_NETWORK_FAILURE
EXECUTION_TIMEOUT
RATE_LIMITED
WORKER_INTERRUPTED
TEMPORARY_RESOURCE_UNAVAILABLE
PROCESS_RECOVERABLE
ATTEMPT_ABANDONED
RETRY_BUDGET_EXHAUSTED
EXECUTION_REVISION_NOT_ELIGIBLE
EXECUTION_SCOPE_INACTIVE
CANCELLATION_REQUESTED
DEADLINE_EXCEEDED
NON_RETRYABLE_ERROR
RUNTIME_STOPPING
EXECUTION_BINDING_UNAVAILABLE
DEPENDENCY_INVALIDATED
IDEMPOTENCY_UNSAFE
RECOVERY_ESCALATION_REQUIRED
```

Reason codes are diagnostic/policy signals.

---

# 12. Attempt Lineage

Each WorkItem has one Attempt lineage.

Example:

```text
WorkItemId = W1

Attempt A1
Attempt A2
Attempt A3
```

Rules:

1. WorkItemId remains unchanged.

2. Every Retry creates a new AttemptId.

3. AttemptNumber increases monotonically within lineage.

4. Previous Attempt remains terminal.

5. Previous Attempt is never resumed.

6. Late Completion always passes authority validation.

7. At most one logical WorkItem outcome is accepted.

8. Attempt lineage is observable.

---

# 13. Retry Flow

```text
Attempt Outcome Reported
        |
        v
Runtime Control Validates:
    identity
    ExecutionScope
    ExecutionRevision
    cancellation
    accepted outcome
        |
        v
Retry Policy Evaluates
        |
        v
Budget / Deadline / Error / Binding Check
        |
        +--> DO_NOT_RETRY
        |
        +--> RECOVERY_ESCALATION_REQUIRED
        |
        +--> RETRY_NOW / RETRY_LATER
                    |
                    v
             New AttemptId
                    |
                    v
             Scheduler Admission
```

---

# 14. Retry Timing Classes

Automatic Retry timing SHOULD use:

```text
NONE
IMMEDIATE
DELAYED
RETRY_AFTER
```

Do not include:

```text
PROVIDER_FALLBACK
```

as a Retry timing class.

---

# 15. Immediate Retry

Immediate Retry SHOULD be limited to failures likely to disappear immediately.

Use only when:

* failure is transient;
* retry does not worsen pressure;
* idempotency is safe;
* budget remains;
* deadline remains useful;
* ExecutionRevision still has value;
* binding is still viable.

Avoid immediate Retry when:

* memory pressure is persistent;
* rate limit still active;
* network outage is ongoing;
* configuration is invalid;
* credentials are invalid;
* deterministic input is invalid.

---

# 16. Delayed Retry

Delayed Retry is appropriate for:

* temporary network issue;
* temporary runtime overload;
* transient timeout;
* bounded recovery delay;
* temporary resource unavailability.

Delay MUST be:

* cancelable;
* tied to CancellationContextRef;
* bounded;
* observable;
* authority-checked again before new Attempt creation;
* resource-light while waiting.

---

# 17. Exponential Backoff

Possible:

```text
baseDelay × growthFactor^attemptIndex
```

Backoff MUST:

* have an upper bound;
* respect useful latency;
* remain cancelable;
* not exceed deadline;
* avoid long invisible delay for interactive work.

---

# 18. Jitter

Jitter MAY be applied to delayed Retry.

It MUST be:

* bounded;
* deterministic in tests;
* deadline-aware;
* subordinate to normalized Retry-After.

MVP MAY disable jitter in simple single-instance deployments.

---

# 19. Retry-After

If an execution adapter returns normalized:

```text
RetryAfter
```

Retry Policy MAY honor it when:

* WorkItem remains useful;
* authority remains valid;
* deadline permits;
* budget permits;
* cancellation remains inactive.

Provider-specific headers remain hidden behind adapters.

---

# 20. Retry Budget

Retry budgets SHOULD remain bounded.

Possible scopes:

```text
WORK_ITEM
EXECUTION_REVISION
EXECUTION_SCOPE
EXECUTION_BINDING
GLOBAL_RUNTIME
```

---

# 21. WorkItem Budget

Example:

```text
maxAttemptsPerWorkItem
```

This is the primary logical Retry bound.

---

# 22. ExecutionRevision Budget

Limits retry amplification across many WorkItems inside one ExecutionRevision.

---

# 23. ExecutionScope Budget

Limits retry amplification across one Runtime execution scope.

This replaces the ambiguous Session-level Runtime budget.

---

# 24. Execution Binding Budget

A retry budget MAY apply to one executable binding/deployment/runtime endpoint.

This is an operational bound.

It does not own Provider Management policy.

---

# 25. Global Runtime Budget

Global Retry concurrency/count must remain bounded to avoid Runtime self-overload.

---

# 26. Concurrent Retry Budget

Possible:

```text
maxConcurrentRetries
maxDelayedRetries
maxExecutionScopeRetries
maxBindingRetries
```

These values belong to Runtime configuration.

---

# 27. Retry Cost Budget

Retry may consume:

* provider/network cost;
* CPU/GPU time;
* memory;
* user-visible latency;
* execution capacity.

Policy MAY reject Retry when expected recovery value is too low relative to bounded cost.

Exact cost model MAY be deferred.

---

# 28. Deadline Boundary

Retry MUST NOT be created when:

```text
now
+
expected delay
+
expected execution duration
>
useful deadline
```

Exact estimation is implementation-specific.

Architecture only requires deadline-aware Retry.

---

# 29. ExecutionRevision Validation

Before another Attempt is created:

* ExecutionRevision must remain eligible;
* authority must not be revoked;
* WorkItem must remain current/relevant;
* accepted terminal outcome must still be absent.

Retry Policy MUST NOT inspect Presentation target semantics directly.

---

# 30. ExecutionScope Validation

Retry is denied when:

* ExecutionScope is inactive;
* ExecutionScope is stopping;
* parent cancellation is active;
* current execution intent no longer applies.

Business Session semantics remain external.

---

# 31. Cancellation Boundary

Cancellation invalidates:

* pending Retry evaluation;
* delayed Retry timer;
* pending Retry Attempt before admission;
* resource-wait timer owned by Retry;
* recovery escalation that still depends on revoked execution.

Canceled work MUST NOT be revived.

---

# 32. Retry and Stale Completion

Late Completion is rejected because authority is absent.

Example:

```text
Attempt A1 finishes late
        |
        v
Runtime Control validates
        |
        v
REJECT_STALE
```

A newer Attempt does not automatically gain authority merely because it is newer.

---

# 33. Pre-Retry Execution Reuse Check

Before creating another Attempt, Runtime MAY ask:

```text
Is another accepted compatible result
already available?
```

This is NOT Retry Policy-owned cache lookup.

Correct flow:

```text
Retry Candidate
        |
        v
Runtime Control
        |
        v
Cache / Reuse Policy
        |
        v
Reusable Accepted Result?
```

If yes:

```text
no Retry Attempt required
```

---

# 34. Cache Boundary

Retry Policy MUST NOT:

* query Runtime Artifact Store directly;
* define semantic compatibility;
* promote cached result;
* accept cached Business result.

Those belong to Cache/Runtime/owning capability contracts.

---

# 35. Recovery Escalation

Retry Policy MAY determine:

```text
ordinary Retry is not appropriate
```

and emit:

```text
RECOVERY_ESCALATION_REQUIRED
```

Possible causes:

* repeated binding failure;
* binding unavailable;
* retry budget for current binding exhausted;
* persistent rate limiting;
* runtime execution environment degraded.

---

# 36. Fallback Boundary

After escalation:

```text
Recovery / Routing Architecture
        |
        v
chooses whether another execution binding exists
```

If a new binding is selected:

```text
Pipeline Runtime
    MAY create another Attempt
```

Retry Policy does not choose the Provider/Model/RoutePlan.

---

# 37. Same WorkItem with New Binding

If logical work and business inputs remain unchanged:

```text
same WorkItemId
new AttemptId
new ExecutionBindingReference
```

MAY be valid.

But the new binding decision is external to Retry Policy.

---

# 38. Replan Boundary

If recovery changes business semantics or required output:

```text
Business Orchestrator
    creates another BusinessExecutionPlan
```

Potentially:

```text
new ExecutionRevision
```

This is not Retry.

---

# 39. Execution Adapter Boundary

Execution Adapter / Provider Adapter MUST NOT:

* perform hidden orchestration Retry;
* select Fallback;
* increment AttemptId;
* own Retry budget;
* decide terminal WorkItem outcome.

Adapter executes one Attempt.

---

# 40. Low-Level Transport Retry

A bounded low-level transport retry MAY exist only when:

* explicitly part of adapter contract;
* invisible to WorkItem semantics;
* does not duplicate non-idempotent side effects;
* does not violate Attempt deadline;
* does not hide meaningful cost/latency.

Architecture SHOULD minimize hidden transport Retry.

---

# 41. Resource Boundary

Physical cleanup completeness and Retry eligibility are distinct.

An old Attempt may still be draining.

Another Attempt may be created only if:

* real capacity remains;
* shared resources can be used safely;
* duplicate side effects are controlled.

---

# 42. Abandoned Attempt

An `ABANDONED` Attempt may still hold:

* provider capacity;
* process capacity;
* network connection;
* billing exposure;
* native/GPU resources.

Retry Policy MUST use truthful capacity projection.

---

# 43. Idempotency

Retry is safe only when side effects are:

* idempotent;
* deduplicated;
* or protected with a stable idempotency mechanism.

Possible execution identity:

```text
WorkItemId
AttemptId
IdempotencyKey
```

Exact provider mechanism belongs outside Runtime Retry Policy.

---

# 44. Retry Storm Prevention

Use bounded controls such as:

* max Attempt count;
* global concurrent Retry budget;
* per-ExecutionScope budget;
* per-binding budget;
* exponential backoff;
* jitter;
* cancellation;
* delayed Retry deduplication;
* Scheduler admission;
* resource pressure.

---

# 45. Retry Deduplication

For one failed Attempt:

* only one Retry evaluation may be accepted;
* only one Retry timer should exist;
* duplicate signals are ignored;
* duplicate new Attempt creation is forbidden.

---

# 46. Retry Evaluation Identity

Recommended:

```text
RetryEvaluation
├── retryEvaluationId
├── workItemId
├── previousAttemptId
├── executionRevisionId
├── reasonCode
├── decision
├── retryTiming?
├── budgetSnapshotReference?
├── evaluatedAt
└── correlationId?
```

---

# 47. Delayed Retry Resource

A delayed Retry timer is a Runtime resource.

It MUST:

* be cancelable;
* be disposed after firing/cancellation;
* not hold large Artifacts;
* revalidate authority before creating new Attempt.

---

# 48. Manual Re-execution

Manual re-execution begins from Application/user intent.

It may result in:

```text
same WorkItem
```

only when owning orchestration/runtime logic confirms logical work is unchanged.

It may instead require:

```text
new BusinessExecutionPlan
new ExecutionRevision
new WorkItem
```

depending on business intent.

Retry Policy does not make this business decision.

---

# 49. Provider / Model Switch

A manual or automatic binding switch is NOT itself Retry Policy logic.

Correct:

```text
Application / Recovery / Routing
    decides new binding

Runtime Control
    determines whether logical WorkItem remains same

Pipeline Runtime
    creates new Attempt if appropriate

Scheduler
    decides admission
```

---

# 50. Retry During Shutdown

When Runtime shutdown begins:

* no new Retry evaluation should produce new Attempt;
* delayed timers are canceled;
* pending Retry admission is canceled/rejected;
* queued Retry Attempts are removed through normal cancellation/drain;
* running Attempts follow Cancellation Policy.

No Retry survives Runtime shutdown.

---

# 51. Retry Events

Recommended:

```text
RetryEvaluated
RetryApproved
RetrySkipped
RetryDelayed
RetryAttemptCreated
RetryAdmissionRequested
RetryExhausted
RetryCancelled
RetryDuplicateRejected
RecoveryEscalationRequested
```

Do NOT emit:

```text
ProviderFallbackSelected
```

as a Retry-owned event.

---

# 52. Event Payload

Recommended:

```text
eventId
occurredAt
executionScopeId
executionRevisionId
workItemId
previousAttemptId
newAttemptId?
attemptNumber?
retryDecision
retryTiming?
reasonCode
delay?
executionBindingReference?
budgetSnapshotReference?
```

No raw user/provider content.

---

# 53. Metrics

Recommended:

```text
retry evaluation count
retry approved count
retry skipped count
retry exhausted count
retry cancelled count
retry success ratio
attempt count per WorkItem
retry delay
retry queue wait
retry execution latency
recovery latency
delayed retry count
global concurrent retry count
retry budget exhaustion
retry deduplication count
retry storm prevention activation
abandoned-overlap count
recovery escalation count
```

Fallback count belongs to recovery/routing observability.

---

# 54. Performance Accounting

Keep separate:

```text
Initial Attempt Latency
Retry Delay
Retry Queue Wait
Retry Execution Latency
Recovery Escalation Latency
Total Useful Result Latency
```

Do not collapse these into one opaque retry metric.

---

# 55. Failure Classification Boundary

Retry Policy consumes normalized:

```text
RuntimeError
```

or normalized recovery trigger.

Retry Policy MUST NOT parse raw provider SDK errors.

Error normalization belongs to `ERROR_MODEL.md` and execution adapters.

---

# 56. Retry Error Categories

Retry Policy SHOULD distinguish at least:

```text
RETRYABLE_TRANSIENT
NON_RETRYABLE
RETRY_EXHAUSTED
AUTHORITY_INVALID
CANCELLED
DEADLINE_EXPIRED
RESOURCE_UNAVAILABLE
BINDING_UNAVAILABLE
RECOVERY_ESCALATION_REQUIRED
```

Exact error taxonomy belongs to `ERROR_MODEL.md`.

---

# 57. MVP Retry Policy

MVP SHOULD support:

```text
NONE
IMMEDIATE
DELAYED
RETRY_AFTER
```

MVP SHOULD NOT define:

```text
PROVIDER_FALLBACK
```

as a Retry Strategy.

---

# 58. MVP Rules

1. Same WorkItemId.

2. New AttemptId.

3. Runtime Control owns relevance/authority validation.

4. Retry Policy owns Retry eligibility and timing.

5. Scheduler owns admission.

6. Worker/Adapter never perform orchestration-level Retry.

7. ExecutionRevision must remain eligible.

8. ExecutionScope must remain eligible.

9. Cancellation invalidates Retry.

10. Retry budget is bounded.

11. Delayed Retry is cancelable.

12. Shutdown cancels all pending Retry.

13. Cache/reuse may be re-evaluated outside Retry Policy.

14. Fallback selection remains outside Retry Policy.

15. Retry never restores execution authority.

16. Actual physical capacity must be truthful.

17. Abandoned Attempts may overlap only when resource/side-effect policy permits.

---

# 59. MVP Budget Guidance

Exact values belong to `RUNTIME_CONFIG.md`.

Conceptually:

```text
interactive immediate Retry:
    at most one or very small number

total Attempts per WorkItem:
    small bounded number

background Retry:
    larger delay but still bounded

global concurrent Retry:
    very small

ExecutionScope Retry:
    bounded
```

No hard-coded OCR/Translation-specific retry counts.

---

# 60. Example — Transient Timeout

```text
Attempt A1 times out
        |
        v
Runtime Control confirms WorkItem still relevant
        |
        v
Retry Policy selects DELAYED
        |
        v
Cancelable timer
        |
        v
Authority revalidated
        |
        v
Attempt A2 created
        |
        v
Scheduler admission
```

---

# 61. Example — Binding Failure Escalation

```text
Attempt A1 fails
        |
        v
Ordinary Retry no longer useful
        |
        v
RECOVERY_ESCALATION_REQUIRED
        |
        v
Routing / Recovery evaluates alternatives
        |
        v
new execution binding selected
        |
        v
Attempt A2 created
        |
        v
Scheduler admission
```

Retry Policy did not select the alternative binding.

---

# 62. Example — ExecutionRevision Superseded

```text
Attempt A1 fails
        |
        v
ExecutionRevision becomes superseded
        |
        v
Retry evaluation
        |
        v
authority invalid
        |
        v
DO_NOT_RETRY
```

---

# 63. Example — Reuse Satisfies Work

```text
Attempt A1 fails
        |
        v
Retry candidate
        |
        v
Runtime Control asks Cache / Reuse Policy
        |
        v
compatible accepted result found
        |
        v
no new Attempt needed
```

Retry Policy did not perform the cache lookup.

---

# 64. Example — Abandoned Physical Execution

```text
Attempt A1 logically abandoned
physical operation still running
        |
        v
Retry evaluation
        |
        v
real resource/capacity checked
        |
        +--> enough capacity
        |       -> another Attempt MAY proceed
        |
        +--> insufficient capacity
                -> RETRY_LATER or DO_NOT_RETRY
```

---

# 65. Architecture Invariants

1. Retry preserves WorkItemId.

2. Retry creates a new AttemptId.

3. Previous Attempt is never resumed.

4. Runtime Control owns execution relevance/authority.

5. Retry Policy owns Retry eligibility/timing, not business routing.

6. Scheduler does not create Retry.

7. Worker does not create Retry.

8. Provider Adapter does not create orchestration Retry.

9. Retry does not bypass Scheduler.

10. Retry does not bypass cancellation.

11. Retry does not bypass authority validation.

12. Retry does not create new business work.

13. Retry does not commit Runtime Artifact, Business result or UI state.

14. Retry does not revive superseded ExecutionRevision.

15. Retry does not revive canceled ExecutionScope.

16. Retry budget is always bounded.

17. Delayed Retry is always cancelable.

18. Retry timers are Runtime resources.

19. Fallback is not a Runtime Retry strategy.

20. Retry Policy does not select Provider/Model/RoutePlan.

21. Recovery escalation and Fallback selection are distinct.

22. Cache lookup is outside Retry Policy.

23. Retry may be skipped if reusable accepted result already satisfies work.

24. Abandoned Attempt may still hold resources.

25. Retry does not assume physical cleanup completed.

26. Manual re-execution is not automatically Retry.

27. Shutdown removes all pending Retry.

28. Retry events contain no user content.

29. ExecutionScope/ExecutionRevision terminology is canonical.

30. Retry Policy does not interpret canonical Privacy/Provider/AI routing policy.

31. Retry uses normalized errors rather than raw provider errors.

32. Hidden adapter Retry must remain bounded and semantically invisible if allowed.

---

# 66. Recommended MVP

CRAI MVP SHOULD support:

* same WorkItem / new Attempt identity;
* immediate Retry;
* delayed Retry;
* Retry-After;
* bounded WorkItem budget;
* bounded ExecutionRevision budget;
* bounded ExecutionScope budget;
* bounded global Retry budget;
* cancelable Retry timer;
* Retry deduplication;
* authority revalidation before Attempt creation;
* Scheduler admission;
* abandoned-capacity awareness;
* idempotency safeguards;
* recovery escalation signal;
* content-free Retry telemetry.

MVP MAY defer:

* adaptive backoff;
* persistent Retry across application restart;
* complex monetary cost budget;
* circuit breaker;
* distributed Retry coordination;
* automated recovery escalation scoring.

---

# 67. Open Decisions

The following remain open:

* exact RetryEvaluation schema;
* max Attempts per WorkItem;
* ExecutionRevision budget;
* ExecutionScope budget;
* global concurrent Retry budget;
* Retry delay defaults;
* jitter defaults;
* Retry-After maximum;
* cost budget;
* retry success attribution;
* idempotency-key format;
* low-level adapter Retry allowance;
* recovery escalation contract;
* reusing newer RuntimeConfigurationSnapshot on Retry;
* persistent Retry policy;
* abandoned-attempt accounting.

---

# 68. Testing Requirements

Tests SHOULD include:

* immediate Retry;
* delayed Retry;
* Retry-After;
* deterministic jitter;
* WorkItem budget exhausted;
* ExecutionRevision budget exhausted;
* ExecutionScope budget exhausted;
* global budget exhausted;
* ExecutionScope inactive;
* ExecutionRevision superseded;
* cancellation before delayed timer;
* cancellation after timer before admission;
* duplicate Retry signal;
* recovery escalation;
* Retry Policy does not select Fallback;
* reusable accepted result avoids Retry;
* abandoned physical operation retains capacity;
* shutdown cancels pending Retry;
* same WorkItemId / new AttemptId;
* late previous Attempt Completion;
* resource delay;
* manual re-execution boundary;
* raw provider error not parsed by Retry Policy.

---

# 69. Related Documents

Runtime:

* `PIPELINE_RUNTIME.md`
* `RUNTIME_COMPONENTS.md`
* `SCHEDULER.md`
* `WORK_QUEUE.md`
* `CANCELLATION.md`
* `ERROR_MODEL.md`
* `CACHE_POLICY.md`
* `MEMORY_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `PERFORMANCE_MODEL.md`
* `RUNTIME_CONFIG.md`
* `RUNTIME_OBSERVABILITY.md`

External:

* `../ai/RETRY.md`
* `../ai/FALLBACK.md`
* `../ai/ROUTING.md`
* `../../02-modules/provider-management/`

---

# 70. Completion Criteria

`RETRY_POLICY.md` is synchronized when:

* Retry remains same WorkItem + new Attempt;
* ExecutionScope/ExecutionRevision terminology is used;
* authority remains Runtime Control-owned;
* Retry Policy owns Retry eligibility/timing only;
* Scheduler owns admission;
* Retry Policy does not select provider/model;
* PROVIDER_FALLBACK is removed as Retry Strategy;
* recovery escalation is separated from Fallback;
* cache lookup remains external;
* automatic Retry and manual re-execution remain distinct;
* budgets are bounded at Runtime scopes;
* abandoned Attempt resource accounting remains truthful;
* Retry events/metrics no longer claim ownership of Fallback.

---

# 71. Summary

CRAI Runtime Retry follows:

```text
Attempt Ends
    |
    v
Runtime Control
    validates relevance / authority
    |
    v
Retry Policy
    decides whether another Attempt is allowed
    |
    +--> DO_NOT_RETRY
    |
    +--> RECOVERY_ESCALATION_REQUIRED
    |
    +--> RETRY_NOW / RETRY_LATER
                    |
                    v
              New AttemptId
                    |
                    v
             Scheduler Admission
                    |
                    v
               Execution
```

The central boundary is:

```text
Retry repeats execution.

Fallback changes execution route.

Those are not the same decision.
```
