# Runtime Scheduler

* **Document:** Runtime Architecture / Scheduler
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how the CRAI Runtime Scheduler evaluates eligible execution work and decides whether that work may proceed toward execution.

The Scheduler is the Runtime admission engine.

It decides:

```text
which eligible work
may consume execution capacity
at what time
under which runtime resource constraints
```

The Scheduler sits between:

```text
Runtime Control
        |
        v
Scheduler
        |
        v
Work Queue / Execution Capacity
        |
        v
Worker Execution
```

The Scheduler does NOT own:

* Business workflow;
* BusinessExecutionPlan;
* Runtime execution authority;
* Retry Policy;
* Fallback selection;
* terminal WorkItem outcome;
* Business result correctness;
* Runtime Artifact commit;
* Presentation commit.

---

# 2. Core Responsibility

Scheduler owns admission decisions for eligible Runtime work.

Its responsibilities include:

* evaluating candidate execution work;
* enforcing admission preconditions;
* applying Runtime priority classes;
* considering ExecutionRevision freshness;
* enforcing bounded concurrency;
* matching execution requirements;
* applying fairness;
* reacting to resource pressure;
* preserving control capacity;
* deferring low-value work;
* rejecting ineligible work;
* replacing explicitly replaceable queued work;
* exposing deterministic decision reason codes.

---

# 3. Core Boundary

```text
BusinessExecutionPlan
        |
        v
Runtime Control
        |
        v
WorkItem / Attempt Candidate
        |
        v
Scheduler
        |
        +--> ADMIT
        +--> DEFER
        +--> REJECT
        +--> REPLACE
        |
        v
Work Queue / Worker Execution
```

Scheduler MUST NOT bypass Runtime Control.

---

# 4. Scheduler Does Not Own

Scheduler MUST NOT:

* create BusinessExecutionPlan;
* change Business Stage dependencies;
* create WorkItems from business semantics;
* create AttemptId;
* decide Retry eligibility;
* decide Fallback route;
* determine terminal WorkItem outcome;
* perform final stale-result validation;
* mutate ExecutionRevision authority;
* cancel running Attempts directly;
* lookup semantic cache results;
* choose provider/model by business policy;
* commit Runtime Artifacts;
* commit Domain state;
* commit Presentation/UI state;
* own durable persistence;
* dispose arbitrary physical resources.

---

# 5. Scheduling Philosophy

CRAI is primarily an interactive reading application.

Scheduler therefore optimizes for:

```text
Useful Current Execution Output
```

rather than:

```text
Completion Of Every Submitted WorkItem
```

Core scheduling principles:

```text
Prefer Current Eligible Work
Bounded Capacity
Protect Control Path
Eliminate Obsolete Pending Work
Respect Declared Business Priority
Respect Execution Constraints
Reject Late Results Outside Scheduler
```

---

# 6. Freshness Is Not Absolute Priority

`CURRENT` ExecutionRevision status is an important scheduling signal.

It MUST NOT automatically outrank:

* shutdown/control work;
* cancellation;
* critical deadlines;
* security containment;
* scarce-resource policy.

Therefore:

```text
Current ExecutionRevision
    = strong freshness signal

not

absolute global priority
```

---

# 7. Scheduler Inputs

Scheduler consumes read-only snapshots/projections from other Runtime components.

It MUST NOT become the owner of those states.

---

# 8. Runtime Control Input

Runtime Control may provide:

```text
ExecutionScope eligibility
ExecutionRevision eligibility
WorkItem eligibility
Attempt eligibility
cancellation state
shutdown state
business-priority metadata
deadline metadata
dependency-runtime readiness
replacement metadata
```

Scheduler does not derive these business/runtime authority meanings independently.

---

# 9. Execution State Input

Execution State Store may provide:

* ExecutionRevision identity;
* current/superseded metadata;
* revision age;
* execution lineage;
* Runtime linkage metadata.

Execution State Store does NOT grant authority.

Runtime Control remains the execution-authority owner.

---

# 10. Work Queue Input

Work Queue may expose:

* queue capacity;
* saturation;
* queued duration;
* queued candidate metadata;
* replacement candidate identity;
* execution-class occupancy.

---

# 11. Worker / Execution Pool Input

Worker/execution infrastructure may expose:

* available execution slots;
* execution-class support;
* worker/pool availability;
* runtime isolation availability;
* CPU/GPU/native context availability;
* pool utilization.

---

# 12. Provider Runtime Input

Scheduler MAY consume runtime execution capacity from:

```text
Provider Runtime Gateway
or
Resolved Execution Binding Projection
```

Possible inputs:

```text
binding availability
binding concurrency capacity
rate-limit pressure
runtime saturation
execution deadline viability
```

Scheduler MUST NOT depend on canonical Provider Management internals.

---

# 13. Provider Management Boundary

```text
Provider Management
    owns provider configuration/governance

Routing / Selection
    chooses execution binding

Provider Runtime Gateway
    exposes executable runtime capacity

Scheduler
    decides whether that bound execution
    may be admitted now
```

---

# 14. Resource Pressure Input

Resource/Runtime monitoring may expose:

```text
CPU pressure
memory pressure
GPU pressure
temporary-storage pressure
Artifact pressure
network execution pressure
process capacity
```

These inputs are operational projections.

Scheduler does not own Resource state.

---

# 15. Runtime Configuration Input

Scheduler consumes immutable Runtime Configuration values such as:

```text
queue limits
concurrency limits
priority classes
fairness policy
resource budget
admission threshold
control reserve
replacement policy
preemption recommendation policy
```

---

# 16. Scheduling Unit

The Scheduler evaluates executable Runtime candidates associated with a WorkItem/Attempt.

Canonical WorkItem/Attempt semantics are defined by `PIPELINE_RUNTIME.md`.

Recommended scheduling projection:

```text
SchedulingCandidate
├── executionScopeId
├── executionRevisionId
├── workItemId
├── attemptId?
├── businessStageId
├── workType
├── priorityClass
├── businessPriority
├── createdAt
├── deadline?
├── costHint?
├── executionRequirements
├── runtimeConfigurationSnapshotId
├── cancellationReference
├── replacementKey?
└── replacementPolicy?
```

---

# 17. Scheduler Does Not Read Business Payload

Scheduler MUST NOT inspect:

* source text;
* translated text;
* screenshots;
* Prompt;
* provider DTOs;
* Domain mutable objects.

Large payloads remain referenced through ArtifactRef or other approved handles.

---

# 18. Execution Requirements

Scheduler MAY consume already-resolved execution requirements.

Recommended:

```text
ExecutionRequirements
├── executionClass
├── CPU requirement?
├── GPU requirement?
├── memoryClass?
├── localRemoteConstraint?
├── runtimeIsolationConstraint?
├── executableBindingRef?
└── resourceClass
```

---

# 19. Business Capability Semantics Stay Outside Scheduler

Scheduler SHOULD NOT interpret fields such as:

```text
Language Capability
Model Capability
Provider Preference
Translation Strategy
OCR Engine Preference
```

These must be resolved before Scheduler admission into executable Runtime requirements/bindings.

---

# 20. Admission Decisions

Scheduler produces exactly one of:

| Decision  | Meaning                                                                       |
| --------- | ----------------------------------------------------------------------------- |
| `ADMIT`   | Candidate may consume queue/execution capacity                                |
| `DEFER`   | Candidate remains pending for reevaluation                                    |
| `REJECT`  | Candidate is not admitted                                                     |
| `REPLACE` | An explicitly replaceable queued candidate is superseded by another candidate |

Scheduler MUST NOT produce:

```text
RETRY
FALLBACK
SUCCEED
FAIL
CANCEL
CACHE_HIT
COMMIT
```

---

# 21. Decision Result

Recommended:

```text
SchedulerDecision
├── decisionId
├── candidateReference
├── decision
├── reasonCode
├── evaluatedAt
├── schedulerSnapshotId?
├── resourceSnapshotReference?
├── queueReference?
└── diagnosticsMetadata?
```

---

# 22. Decision Reason Codes

Reason codes SHOULD be stable and explicit.

Possible:

```text
CURRENT_EXECUTION_REVISION
EXECUTION_REVISION_SUPERSEDED
EXECUTION_SCOPE_INACTIVE
WORK_NOT_ELIGIBLE
DEPENDENCY_RUNTIME_NOT_READY
QUEUE_CAPACITY_AVAILABLE
QUEUE_CAPACITY_EXCEEDED
REPLACED_BY_NEWER_WORK
NO_EXECUTION_CAPACITY
NO_COMPATIBLE_EXECUTION_PATH
CONCURRENCY_LIMIT
RESOURCE_BUDGET_EXCEEDED
EXECUTION_BINDING_UNAVAILABLE
EXECUTION_BINDING_SATURATED
DEADLINE_EXPIRED
ADMISSION_PAUSED
RUNTIME_STOPPING
CONTROL_CAPACITY_RESERVED
HARD_SAFETY_LIMIT
```

---

# 23. REJECT Semantics

`REJECT` is one admission decision with different causes.

It MUST NOT silently conflate:

```text
no longer eligible
never executable
hard resource denial
deadline expiration
shutdown rejection
```

Reason code carries the distinction.

Runtime Control decides the logical consequence.

---

# 24. Priority Classes

Recommended small stable Runtime classes:

```text
CONTROL
INTERACTIVE_CRITICAL
INTERACTIVE_SUPPORTING
BACKGROUND
MAINTENANCE
```

Default ordering:

```text
CONTROL
    >
INTERACTIVE_CRITICAL
    >
INTERACTIVE_SUPPORTING
    >
BACKGROUND
    >
MAINTENANCE
```

---

# 25. CONTROL Priority

CONTROL represents Runtime control-plane work such as:

* cancellation;
* shutdown;
* lifecycle control;
* execution-authority replacement;
* fatal containment;
* minimal cleanup coordination.

It is not ordinary Business work.

---

# 26. Business Priority

Business Orchestration MAY declare relative business importance.

Examples:

```text
VISIBLE_CONTENT
NEARBY_CONTENT
PREFETCH
BACKGROUND
```

Runtime Control maps that business priority into Scheduling metadata.

Scheduler does not infer business importance from payload.

---

# 27. Priority Inputs

Scheduler MAY consider:

```text
PriorityClass
ExecutionRevision freshness
BusinessPriority
User interaction elevation
Deadline pressure
Queue age
Cost hint
Resource cost
Execution-binding availability
Obsolescence risk
Fairness weight
```

---

# 28. Priority Algorithm

Exact scoring is implementation-specific.

It SHOULD be:

* deterministic;
* bounded;
* explainable;
* observable;
* testable.

---

# 29. Current ExecutionRevision Preference

Current ExecutionRevision is a strong preference among otherwise comparable Business work.

Recommended:

```text
candidate.executionRevisionId
    == current eligible ExecutionRevisionId
```

may increase scheduling priority.

---

# 30. Superseded Work

Queued/pending work from superseded execution normally SHOULD:

* become ineligible;
* be rejected;
* or be replaced when an explicit replacement relation exists.

Running Attempt is not directly terminated by Scheduler.

---

# 31. Obsolete Work Elimination

Scheduler SHOULD eliminate obsolete work early.

Validation points MAY include:

```text
before admission
while pending
before queue dispatch
before worker assignment
```

After execution:

```text
Runtime Control
```

performs final authority validation.

---

# 32. Admission Preconditions

Before `ADMIT`, Scheduler SHOULD verify from trusted projections:

* Runtime admission open;
* ExecutionScope eligible;
* ExecutionRevision eligible;
* WorkItem/Attempt eligible;
* declared runtime dependencies ready;
* cancellation not revoked;
* required ArtifactRefs available by metadata;
* queue/execution capacity available;
* execution requirements satisfiable;
* concurrency budget available;
* resource budget available;
* executable binding capacity available where applicable;
* deadline still viable;
* control reserve preserved.

---

# 33. Admission Is Not Authority

Scheduler admission means:

```text
may proceed toward execution
```

It does NOT mean:

```text
result will be accepted
```

Final Runtime acceptance remains authority-validated later.

---

# 34. Resource Classes

Possible logical classes:

```text
CONTROL
CPU_LIGHT
CPU_HEAVY
GPU
NETWORK_IO
DISK_IO
NATIVE_SERIAL
EXTERNAL_PROVIDER
```

These are Runtime execution classes.

They are NOT Business capabilities.

---

# 35. UI Is Not a Worker Resource Class by Default

Scheduler MAY preserve responsiveness for Presentation/UI-related execution.

However UI state remains Presentation/Application-owned.

Avoid defining generic:

```text
UI
```

as a physical resource class unless implementation requires one.

---

# 36. Concurrency Control

Concurrency MUST remain bounded.

Limits MAY apply to:

```text
global Runtime
ExecutionScope
WorkType
execution class
worker pool
provider binding
GPU context
network execution
native serial context
resource class
```

---

# 37. ExecutionScope Concurrency

Per-ExecutionScope limits MAY prevent one execution scope from monopolizing capacity.

This is a Runtime fairness concern.

It does not redefine Reading Session business semantics.

---

# 38. Admission Budget

Scheduler MAY combine:

```text
Queue Budget
Worker Budget
Execution-Binding Budget
CPU Budget
GPU Budget
Memory Budget
Artifact Budget
Temporary Storage Budget
Control Capacity Reserve
```

No single budget is sufficient by itself.

---

# 39. Control Capacity

Runtime MUST reserve enough capacity for:

* cancellation;
* shutdown;
* Runtime Control commands;
* ExecutionRevision replacement;
* fatal-error handling;
* bounded cleanup;
* security containment.

Control work MUST NOT be starved behind long-running Business work.

---

# 40. Scheduling Cycle

Typical cycle:

```text
Scheduling Signal
        |
        v
Collect Eligible Candidates
        |
        v
Remove Ineligible Candidates
        |
        v
Evaluate Explicit Replacement
        |
        v
Compute Priority
        |
        v
Check Execution Requirements
        |
        v
Check Capacity / Resource Budgets
        |
        v
Produce Admission Decision
        |
        v
Emit Decision Telemetry
```

---

# 41. Scheduling Signals

Possible signals:

* WorkItem created;
* Attempt created;
* queue changed;
* execution slot available;
* ExecutionRevision changed;
* ExecutionScope changed;
* binding capacity changed;
* resource pressure changed;
* Runtime configuration activated;
* scheduler admission state changed;
* shutdown started.

Scheduler SHOULD be event-driven where practical.

---

# 42. Replacement

`REPLACE` applies only to queued/pending work explicitly declared replaceable.

Critical rule:

```text
Scheduler does not invent semantic equivalence.
```

---

# 43. Replacement Metadata

Runtime Control SHOULD provide:

```text
replacementKey
replacementPolicy
```

based on accepted plan/runtime semantics.

Scheduler merely applies the declared replacement relation.

---

# 44. Replacement Key

Example conceptual key:

```text
ExecutionScopeId
BusinessStageId
WorkType
TargetIdentity
ReplacementLineage
```

Exact semantics belong to Runtime Control/owning plan contracts.

---

# 45. Latest-Value Behavior

A declared policy MAY state:

```text
for one replacement lineage:
keep newest eligible pending work
```

Scheduler MUST NOT assume latest-value semantics globally.

---

# 46. Running Attempt Replacement

Scheduler MUST NOT replace a running Attempt directly.

Running work may instead:

* lose execution authority;
* receive cooperative cancellation;
* continue draining.

Those actions are coordinated by Runtime Control/Cancellation.

---

# 47. Preemption Boundary

Scheduler MAY emit:

```text
PREEMPTION_RECOMMENDED
SCARCE_CAPACITY_BLOCKED
OBSOLETE_RUNNING_WORK
```

It MUST NOT cancel running Attempts itself.

---

# 48. Preemption Decision Ownership

Recommended:

```text
Scheduler
    detects capacity pressure

Runtime Control / Cancellation
    decides authority/cancellation action
```

---

# 49. Fairness

Scheduler SHOULD prevent one ExecutionScope, WorkType or binding from monopolizing capacity indefinitely.

Possible inputs:

```text
foreground ExecutionScope weight
secondary scope weight
background scope weight
WorkType share
execution-binding share
queue age
```

---

# 50. MVP Fairness

MVP MAY optimize primarily for one foreground interactive ExecutionScope.

Architecture MUST NOT hard-code the assumption that only one ExecutionScope can ever exist.

---

# 51. User Interaction Priority

User-triggered actions MAY receive elevated Runtime priority metadata.

Examples:

* manual retranslation;
* explicit source change;
* explicit region change;
* explicit cancellation;
* requested presentation refresh.

Scheduler uses the provided metadata.

It does not infer business intent from payload.

---

# 52. Deadline Handling

Deadline is scheduling metadata.

If deadline expires before admission, Scheduler MAY:

```text
REJECT
```

with:

```text
DEADLINE_EXPIRED
```

Scheduler MUST NOT directly mark WorkItem as `FAILED`.

---

# 53. Running Deadline

Once execution has started, timeout/cancellation handling belongs to Runtime Control, Cancellation and execution contracts.

Scheduler no longer owns that Attempt.

---

# 54. Provider / Binding Pressure

If executable binding is saturated:

```text
Binding Pressure
        |
        v
Scheduler Reduces Admission
        |
        v
Background Work Deferred
        |
        v
Interactive Work Prioritized Where Capacity Allows
```

Scheduler does NOT choose Fallback.

---

# 55. Fallback Boundary

Correct:

```text
Binding unavailable
        |
        v
Routing / Recovery Architecture
        |
        v
new ExecutionBindingReference
        |
        v
Runtime Control creates/updates Attempt
        |
        v
Scheduler evaluates admission
```

---

# 56. Cache Boundary

Scheduler does not perform semantic cache lookup.

Correct:

```text
Cache / Reuse Evaluation
        |
        v
Runtime Control determines whether work is needed
        |
        v
Scheduler receives remaining eligible work
```

`CACHE_HIT` is not a Scheduler decision.

---

# 57. Retry Boundary

Correct:

```text
Attempt Failure
        |
        v
Runtime Control validates relevance
        |
        v
Retry Policy evaluates
        |
        v
new Attempt created
        |
        v
Scheduler evaluates admission
```

Scheduler MUST NOT:

* classify retryable failures;
* own retry budget;
* create AttemptId;
* choose Fallback binding.

---

# 58. Partial Result Boundary

Partial-result semantics are defined by BusinessExecutionPlan and owning module.

Scheduler only schedules associated Runtime work.

It does NOT define:

* partial correctness;
* partial ordering;
* partial commit;
* Presentation semantics.

---

# 59. Backpressure

When downstream capacity is exhausted, Scheduler SHOULD reduce admission instead of allowing unbounded queue growth.

Possible actions:

1. reject obsolete pending work;

2. replace explicitly replaceable older pending work;

3. defer Background/Maintenance work;

4. reduce expensive concurrency;

5. preserve control capacity;

6. preserve valuable current interactive work;

7. emit resource-pressure decisions.

---

# 60. Memory Pressure

Under high memory pressure, Scheduler MAY:

* stop Maintenance admission;
* defer/reject Background work;
* reduce expensive concurrency;
* preserve control capacity;
* preserve current useful work;
* recommend cancellation/cleanup;
* reject non-critical work above hard safety limits.

Scheduler MUST NOT directly dispose Runtime Artifacts.

---

# 61. Artifact Pressure

Artifact pressure MAY influence admission.

Artifact lifecycle remains owned by Runtime Artifact Store/Resource Manager.

Scheduler only changes admission.

---

# 62. Scheduler Lifecycle

Recommended:

```text
STOPPED
    |
    v
STARTING
    |
    v
RUNNING
    |
    +--> PAUSED
    |
    v
STOPPING
    |
    v
STOPPED
```

---

# 63. Scheduler PAUSED

`PAUSED` means:

```text
normal Business admission paused
```

while control operations remain functional.

It is NOT:

```text
Reading Session paused
Application business state paused
```

---

# 64. Scheduler State Ownership

Scheduler owns:

* internal policy state;
* admission decision state;
* fairness counters;
* bounded scheduling metadata;
* Scheduler lifecycle;
* decision diagnostics.

Scheduler does NOT own:

* WorkItem terminal outcome;
* ExecutionRevision authority;
* cancellation authority;
* Runtime Artifact lifecycle;
* provider business selection;
* retry lineage source of truth.

---

# 65. Scheduler Events

Possible normalized events:

```text
SchedulerStarted
SchedulerPaused
SchedulerResumed
SchedulerStopped

WorkAdmitted
WorkDeferred
WorkRejected
WorkReplaced

CapacityPressureDetected
PreemptionRecommended
ExecutionBindingCapacityDegraded
```

---

# 66. Scheduler Must Not Emit Terminal Ownership Events

Scheduler MUST NOT emit:

```text
WorkSucceeded
WorkFailed
WorkRetried
WorkCancelled
```

as if it owned those state transitions.

---

# 67. Metrics

Recommended content-free metrics:

```text
admission decision count
defer count
reject count
replace count
decision latency
pending candidate count
queue saturation
queue wait
execution-capacity saturation
resource-pressure decisions
current-revision admission ratio
background deferral ratio
fairness wait
preemption recommendation count
```

---

# 68. Determinism

For identical:

* candidate set;
* Runtime Control eligibility snapshot;
* ExecutionRevision state;
* queue state;
* execution-capacity state;
* binding capacity;
* resource projection;
* Runtime Configuration snapshot;
* Scheduler implementation/version;

the decision SHOULD be deterministic.

---

# 69. Time as Input

Because:

* queue age;
* deadline;
* time-based fairness

may affect scheduling, the evaluation timestamp/Clock snapshot is part of deterministic input.

Tests SHOULD use an injected deterministic Clock.

---

# 70. Randomness

Randomness MAY be used only when:

* intentional;
* seeded;
* observable;
* testable;
* not required for correctness.

---

# 71. Scheduler Failure Isolation

On internal Scheduler failure:

```text
Stop New Admission
        |
        v
Preserve Control Path
        |
        v
Notify Runtime Control
        |
        v
Hold / Reject Pending Work Safely
        |
        v
Emit Diagnostics
        |
        v
Controlled Runtime Recovery / Shutdown
```

Scheduler failure MUST NOT mutate accepted Business state or Runtime Artifacts.

---

# 72. MVP Scheduling Policy

MVP assumptions MAY include:

* one foreground interactive ExecutionScope;
* one current ExecutionRevision per lineage;
* bounded Work Queues;
* bounded execution pools;
* cooperative cancellation;
* explicit execution-binding capacity;
* current-useful-work preference.

---

# 73. MVP Admission Rules

1. Reject when Runtime admission is closed.

2. Reject inactive ExecutionScope work.

3. Reject ineligible ExecutionRevision/WorkItem.

4. Replace explicitly replaceable obsolete pending work.

5. Preserve Control capacity.

6. Prefer current useful execution.

7. Prefer Interactive Critical work.

8. Respect execution requirements.

9. Respect queue/concurrency/resource budgets.

10. Admit highest-value eligible candidate according to deterministic policy.

---

# 74. Example: Rapid Scrolling

```text
ExecutionRevision 30 Attempt running

User scrolls

ExecutionRevision 31 created
Runtime Control revokes Revision 30 authority

Revision 31 work pending

User scrolls again

ExecutionRevision 32 created
Revision 31 pending work becomes obsolete/replaced
Scheduler recommends preemption for obsolete running work
Revision 32 receives next scarce interactive slot
```

Runtime Control rejects late Revision 30 Completion as stale.

---

# 75. Example: Provider Saturation

```text
Execution binding saturated
        |
        v
Scheduler defers background provider work
        |
        v
Current interactive work remains preferred
        |
        v
Routing / Recovery may choose another binding
        |
        v
Scheduler evaluates the new Attempt
```

Scheduler itself does not choose the fallback.

---

# 76. Example: Memory Pressure

```text
Memory Pressure High
        |
        v
Maintenance Admission Stops
        |
        v
Background Work Deferred / Rejected
        |
        v
Expensive Concurrency Reduced
        |
        v
Current Useful Work Preserved
        |
        v
Runtime Control may coordinate cancellation/cleanup
```

---

# 77. Example: Replacement

```text
Pending WorkItem A
    ExecutionScope = E1
    Stage = PresentationPrepare
    Target = viewport-42
    ReplacementKey = R

New WorkItem B
    same ReplacementKey = R
    newer eligible ExecutionRevision

Scheduler:
    REPLACE A with B
```

Scheduler does not define what `R` means.

Runtime Control supplied that semantics.

---

# 78. Architecture Invariants

1. Scheduler owns Runtime admission decisions.

2. Scheduler does not create BusinessExecutionPlan.

3. Scheduler does not create WorkItem from business semantics.

4. Scheduler does not create AttemptId.

5. Scheduler does not decide Retry.

6. Scheduler does not decide Fallback.

7. Scheduler does not decide terminal WorkItem outcome.

8. Scheduler does not commit Runtime Artifact.

9. Scheduler does not commit Domain or Presentation state.

10. Scheduler does not mutate ExecutionRevision authority.

11. Scheduler does not read Business payloads.

12. Queue and concurrency are always bounded.

13. Control path must not starve.

14. Current ExecutionRevision is a strong freshness preference, not absolute global priority.

15. Worker/execution assignment must satisfy resolved execution requirements.

16. Provider/model business selection remains outside Scheduler.

17. Scheduler consumes runtime binding capacity, not canonical Provider Management semantics.

18. Cache lookup is outside Scheduler.

19. Final stale validation is outside Scheduler.

20. Scheduler decisions are observable.

21. Every decision has a reason code.

22. Backpressure reduces admission rather than allowing unbounded queue growth.

23. Scheduler failure must not corrupt Runtime correctness.

24. Telemetry failure must not alter decision semantics.

25. Scheduler does not invent replacement equivalence.

26. Replacement semantics are supplied explicitly.

27. Running Attempt is never directly replaced by Scheduler.

28. Scheduler may recommend preemption but does not cancel running work.

29. Scheduler PAUSED is admission state, not Business Session state.

30. Scheduler execution requirements are Runtime concepts, not Business capability semantics.

---

# 79. Recommended MVP

CRAI MVP SHOULD support:

* one process-local Scheduler;
* `ADMIT / DEFER / REJECT / REPLACE`;
* bounded admission;
* Control capacity reservation;
* Interactive Critical priority;
* Background/Maintenance classes;
* current ExecutionRevision preference;
* deterministic scheduling;
* explicit reason codes;
* bounded Queue awareness;
* bounded worker/execution-pool awareness;
* execution-binding capacity;
* basic resource-pressure handling;
* explicit replacement key/policy;
* preemption recommendations;
* one foreground interactive ExecutionScope.

MVP MAY defer:

* adaptive concurrency;
* multi-Scheduler federation;
* distributed admission;
* predictive cost modeling;
* advanced fairness;
* speculative execution;
* provider racing;
* automatic Scheduler-driven preemption;
* dynamic priority learning.

---

# 80. Open Decisions

The following remain open:

* exact SchedulingCandidate schema;
* exact priority-class numeric mapping;
* Scheduler algorithm;
* fairness algorithm;
* cost-hint estimation;
* replacement-key schema;
* replacement-policy taxonomy;
* preemption recommendation thresholds;
* execution-binding capacity model;
* resource-pressure thresholds;
* per-ExecutionScope quotas;
* adaptive concurrency;
* local vs remote execution pools;
* GPU reservation model;
* scheduling snapshot identity;
* Scheduler persistence needs;
* exact control-capacity reserve.

---

# 81. Related Documents

Runtime:

* `PIPELINE_RUNTIME.md`
* `RUNTIME_COMPONENTS.md`
* `BUSINESS_PIPELINE_ORCHESTRATION.md`
* `WORK_QUEUE.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`
* `CACHE_POLICY.md`
* `MEMORY_MODEL.md`
* `THREADING_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `PERFORMANCE_MODEL.md`
* `RUNTIME_CONFIG.md`
* `RUNTIME_OBSERVABILITY.md`

External:

* `../ai/ROUTING.md`
* `../ai/FALLBACK.md`
* `../../02-modules/provider-management/`

---

# 82. Completion Criteria

`SCHEDULER.md` is synchronized when:

* Scheduler owns only admission decisions;
* only `ADMIT / DEFER / REJECT / REPLACE` remain;
* ExecutionScope/ExecutionRevision terminology is used;
* Current Revision preference is not absolute priority;
* Retry and Fallback are outside Scheduler;
* provider/model selection is outside Scheduler;
* Provider Management is not a Scheduler dependency;
* Scheduler consumes resolved execution-binding capacity;
* replacement semantics are explicit and externally supplied;
* Scheduler does not inspect business capability semantics;
* queue and concurrency remain bounded;
* control path remains protected;
* decisions are deterministic, observable and reason-coded.

---

# 83. Summary

CRAI Scheduler follows:

```text
Runtime Control
    creates eligible executable work

        |
        v

Scheduler
    decides admission

        |
        v

Work Queue / Execution Capacity
    holds admitted work

        |
        v

Workers
    execute Attempts

        |
        v

Runtime Control
    accepts/rejects execution outcomes
```

The central rule is:

```text
Scheduler decides when eligible work may run.

Scheduler does not decide what the work means.
```
