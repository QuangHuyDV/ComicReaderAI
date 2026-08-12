# Runtime Work Queue

* **Document:** Runtime Architecture / Work Queue
* **Version:** 3.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how CRAI Runtime temporarily holds admitted execution work before physical execution begins.

The Work Queue sits between:

```text
Runtime Control
        |
        v
Scheduler
        |
        v
Work Queue
        |
        v
Worker / Execution Capacity
```

The Work Queue owns only:

```text
queued position
+
bounded waiting
+
atomic queue-state transitions
```

It does NOT own:

* business workflow;
* admission policy;
* Runtime execution authority;
* Retry Policy;
* Fallback;
* terminal WorkItem outcome;
* cancellation authority;
* physical execution;
* Runtime Artifact payloads;
* Business result correctness.

---

# 2. Core Responsibility

The Work Queue has one primary responsibility:

```text
hold already-admitted lightweight execution references
until they are dispatched, replaced, removed or drained
```

Its responsibilities include:

* storing queued execution references;
* maintaining bounded capacity;
* preserving Scheduler-provided ordering metadata;
* applying explicit replacement instructions;
* applying explicit removal instructions;
* atomically dispatching items;
* preventing duplicate queued Attempt identity;
* exposing queue capacity/pressure metrics;
* supporting controlled drain/shutdown.

---

# 3. Architectural Position

```text
BusinessExecutionPlan
        |
        v
Runtime Control
        |
        v
WorkItem / Attempt
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
Work Queue
        |
        v
Worker Execution
```

Queue MUST NOT bypass Scheduler.

Worker MUST NOT execute queued work that has not passed admission.

---

# 4. Queue Philosophy

CRAI is an interactive Runtime.

Queue is optimized for:

```text
valuable admitted work
```

not:

```text
preserving every submitted task forever
```

Queue MUST be:

* bounded;
* lightweight;
* removable;
* replacement-capable;
* observable;
* generic;
* safe under cancellation/shutdown.

Queue MUST NOT become a second Scheduler.

---

# 5. Generic Runtime Infrastructure

Work Queue is generic Runtime infrastructure.

Architecture MUST NOT define canonical queues such as:

```text
OCRQueue
LayoutQueue
TranslationQueue
PresentationQueue
```

Queue works with generic Runtime execution metadata.

Physical implementations MAY partition queues later.

Logical contracts remain capability-independent.

---

# 6. Logical Queue Classes

Runtime MAY use logical queue classes such as:

```text
CONTROL
INTERACTIVE
BACKGROUND
MAINTENANCE
```

These classes support Runtime admission and capacity isolation.

They do NOT imply:

* dedicated thread;
* dedicated process;
* Business Module ownership.

---

# 7. CONTROL Queue

CONTROL capacity is reserved for Runtime control-plane work such as:

* cancellation coordination;
* shutdown;
* ExecutionRevision replacement;
* fatal containment;
* Runtime lifecycle commands;
* bounded cleanup coordination.

CONTROL is not ordinary Business execution work.

---

# 8. INTERACTIVE Queue

INTERACTIVE work contributes directly to current user-facing useful output.

Exact business priority is supplied by Runtime Control/Scheduler metadata.

Queue does not infer user-visible importance.

---

# 9. BACKGROUND Queue

BACKGROUND may hold:

* prefetch;
* nearby-content preparation;
* nonblocking enhancement;
* other admitted background work.

---

# 10. MAINTENANCE Queue

MAINTENANCE may hold bounded:

* cleanup jobs;
* diagnostics aggregation;
* preload;
* maintenance tasks.

It MUST NOT consume all control/interactive capacity.

---

# 11. Queue Item

Queue stores an immutable lightweight item.

Recommended:

```text
QueuedExecutionItem
├── executionScopeId
├── executionRevisionId
├── workItemId
├── attemptId
├── businessStageId?
├── workType
├── priorityClass
├── orderingMetadata
├── executionRequirementsReference?
├── inputArtifactRefs[]
├── runtimeConfigurationSnapshotId
├── cancellationReference
├── replacementKey?
├── replacementPolicy?
├── enqueuedAt
├── deadline?
└── queueMetadata
```

---

# 12. Queue Identity

Canonical queued execution identity SHOULD include:

```text
WorkItemId
+
AttemptId
```

within the relevant logical queue scope.

The same Attempt MUST NOT be queued multiple times.

---

# 13. ExecutionScope / ExecutionRevision

Queue uses:

```text
ExecutionScopeId
ExecutionRevisionId
```

It MUST NOT use unqualified:

```text
SessionId
RevisionId
```

as canonical Runtime hierarchy.

Business identifiers such as ReadingSessionId MAY be attached separately for correlation.

---

# 14. Queue Item Must Stay Lightweight

Queued item MUST NOT contain:

* image buffers;
* OCR/source text payload;
* translated document payload;
* Prompt;
* AI Context;
* mutable provider SDK DTO;
* raw secrets;
* mutable Business objects;
* retry-budget source of truth;
* large binary payloads.

Large data is referenced by:

```text
ArtifactRef
```

or another approved immutable handle.

---

# 15. Queue Does Not Own Artifact Lifetime

Queue only stores Artifact references.

Runtime Artifact Store / Resource Manager owns:

* Artifact registry;
* physical backing;
* lease;
* retention;
* disposal.

Queue SHOULD NOT hold long-lived Artifact leases.

---

# 16. Attempt-Level Queueing

Physical execution queueing SHOULD normally occur at Attempt level.

Retry flow:

```text
Attempt 1 failed
        |
        v
Runtime Control / Retry Policy
        |
        v
Attempt 2 created
        |
        v
Scheduler admission
        |
        v
new QueuedExecutionItem
```

Attempt 1 MUST NOT be mutated/requeued as Attempt 2.

---

# 17. Queue Ownership

```text
Runtime Control
    -> execution authority
       WorkItem logical state

Scheduler
    -> admission decision
       scheduling policy

Work Queue
    -> waiting position
       bounded buffering

Worker
    -> physical Attempt execution

Runtime Artifact Store
    -> execution Artifact lifecycle
```

---

# 18. Queue Lifecycle

Recommended queue-item lifecycle:

```text
ENQUEUED
    |
    v
WAITING
    |
    v
SELECTED
    |
    v
DISPATCHED
```

Alternative terminal queue states:

```text
WAITING -> REPLACED
WAITING -> REMOVED
WAITING -> DRAINED
```

After `DISPATCHED`, the item is no longer Queue-owned.

---

# 19. Queue States

Queue MAY use:

```text
ENQUEUED
WAITING
SELECTED
DISPATCHED
REPLACED
REMOVED
DRAINED
```

Queue MUST NOT use execution states such as:

```text
RUNNING
SUCCEEDED
FAILED
CANCELLED
STALE
```

Those belong outside Queue ownership.

---

# 20. Bounded Capacity

Every Queue MUST be bounded.

Capacity MAY be partitioned by:

* logical queue class;
* ExecutionScope;
* WorkType;
* execution class;
* worker pool;
* provider/runtime binding;
* resource class;
* global Runtime budget.

Exact limits belong to `RUNTIME_CONFIG.md`.

---

# 21. Capacity Reservation

Runtime SHOULD reserve capacity for CONTROL.

Interactive capacity MAY also be protected.

Conceptually:

```text
Total Queue Capacity
├── Reserved Control
├── Reserved Interactive
└── Shared Background / Maintenance
```

---

# 22. Admission Boundary

Queue accepts an item only after Scheduler returns:

```text
ADMIT
```

or after an explicit Scheduler-controlled replacement operation.

Queue MUST NOT convert technical conditions into business admission policy.

---

# 23. Technical Enqueue Failure

Queue MAY fail an enqueue technically because of:

* internal integrity failure;
* duplicate queued identity;
* Queue stopping;
* serialization/representation failure;
* capacity contract violation caused by race/inconsistency.

It reports:

```text
QueueEnqueueFailure
```

to Scheduler/Runtime Control.

It does NOT independently decide:

```text
DEFER
REJECT
```

as scheduling policy.

---

# 24. Capacity Pressure vs Admission Decision

Critical distinction:

```text
Queue
    reports capacity state
```

```text
Scheduler
    decides admission response
```

Queue MUST NOT become a second admission engine.

---

# 25. Ordering Metadata

Queue preserves Scheduler-provided ordering metadata.

Queue SHOULD NOT independently derive scheduling priority.

Possible metadata MAY include:

```text
priorityClass
priorityRank
schedulerDecisionSequence
deadline
fairnessToken
```

Exact format is implementation-specific.

---

# 26. Ordering Boundary

Preferred architecture:

```text
Scheduler
    computes order/selection semantics

Queue
    preserves/applies them
```

Queue MUST NOT reinterpret:

* business priority;
* ExecutionRevision freshness;
* provider preference;
* deadline policy;
* fairness semantics.

---

# 27. Non-FIFO Behavior

Queue need not be strict FIFO.

Example:

```text
A waiting
B waiting
C waiting
```

Scheduler may choose C before A/B based on policy.

Queue executes that scheduling decision.

---

# 28. Queue Selection Models

Implementation MAY use one of several valid models.

### Model A — Scheduler Selects

```text
Scheduler
    identifies next queued item

Queue
    atomically selects/removes it
```

### Model B — Queue Uses Scheduler Ordering Token

```text
Scheduler
    assigns immutable ordering metadata

Queue
    selects highest-ranked item
```

Both are valid if Queue does not redefine policy.

---

# 29. Replacement

Replacement applies only to queued/pending work explicitly declared replaceable.

Queue MUST NOT invent replacement semantics.

---

# 30. Replacement Key

`replacementKey` SHOULD be treated as opaque by Queue.

Example:

```text
replacementKey = "opaque-runtime-lineage-key"
```

Its semantic meaning belongs to Runtime Control/planning contracts.

---

# 31. Replacement Policy

Possible supplied policies MAY include:

```text
NONE
LATEST_ELIGIBLE
EXPLICIT_REPLACEMENT_ONLY
```

Exact taxonomy remains open.

Queue only applies the provided policy.

---

# 32. Atomic Replacement

Replacement MUST be atomic.

After successful replacement:

```text
old item
    MUST NOT dispatch
```

---

# 33. Replacement Does Not Affect Running Attempt

Queue can replace only queued/waiting items.

A running Attempt is outside Queue ownership.

Running work may:

* lose authority;
* receive cancellation;
* drain.

Those actions belong to Runtime Control/Cancellation.

---

# 34. Duplicate Prevention

Queue MUST prevent duplicate queued Attempt identity.

Recommended key:

```text
logicalQueueScope
+
WorkItemId
+
AttemptId
```

Duplicate enqueue MUST:

* fail safely;
* emit reason code;
* not create duplicate physical execution.

---

# 35. Eligibility

Queue does NOT determine execution eligibility.

Eligibility is supplied by Runtime Control/Scheduler.

Queue MAY retain eligibility metadata for efficient removal.

It MUST NOT independently decide which ExecutionRevision is current.

---

# 36. Eligibility Loss

Possible causes supplied externally:

* ExecutionScope inactive;
* ExecutionRevision superseded;
* cancellation authority revoked;
* BusinessExecutionPlan replaced;
* deadline invalid;
* dependency runtime invalidated;
* executable binding unavailable;
* Runtime stopping.

Queue receives explicit instruction to remove/replace/drain affected work.

---

# 37. Removal

Queued item MAY be removed:

* before dispatch;
* after eligibility revocation;
* after cancellation;
* through explicit replacement;
* during drain;
* after integrity failure.

Removal MUST be:

* atomic;
* observable;
* idempotent where practical.

---

# 38. Removal Instruction

Recommended:

```text
QueueRemovalInstruction
├── instructionId
├── targetSelector
├── reasonCode
├── issuedBy
├── correlationId?
└── issuedAt
```

---

# 39. Target Selectors

Possible selectors:

```text
WORK_ITEM
ATTEMPT
EXECUTION_SCOPE
EXECUTION_REVISION
REPLACEMENT_KEY
QUEUE_CLASS
ALL_NON_CONTROL
ALL
```

---

# 40. Canonical Runtime Names

Use:

```text
REMOVE_EXECUTION_SCOPE
REMOVE_EXECUTION_REVISION
```

not:

```text
REMOVE_SESSION
REMOVE_REVISION
```

unless the command is explicitly Business Session-specific.

---

# 41. Dispatch

Dispatch MUST transfer ownership atomically.

Recommended:

```text
WAITING
    |
    v
SELECTED
    |
    v
Queue ownership removed
    |
    v
Worker ownership established
    |
    v
DISPATCHED
```

---

# 42. No Ambiguous Ownership Window

There MUST NOT be an observable state where one item is simultaneously:

```text
Queue-owned
and
Worker-owned
```

without an explicit transfer protocol.

---

# 43. Dispatch Token

Implementation MAY use a short-lived:

```text
DispatchToken
```

to make transfer atomic/idempotent.

Exact mechanism remains open.

---

# 44. Dispatch Failure

If selected work cannot be handed to Worker:

```text
SELECTED
    |
    v
Dispatch Failed
```

Queue MUST NOT automatically perform execution Retry.

It reports failure to Runtime Control/Scheduler.

Possible later actions include:

* re-admit;
* defer;
* reject;
* create another Attempt if policy requires.

---

# 45. Dispatch Failure vs Attempt Failure

Critical distinction:

```text
Dispatch failed
    = Attempt may never have started
```

```text
Attempt failed
    = physical execution began
      and ended in failure
```

These MUST remain distinguishable.

---

# 46. Drain

Drain removes or freezes queued work according to an explicit drain instruction.

Possible reasons:

* Scheduler pause;
* Runtime shutdown;
* ExecutionScope termination;
* ExecutionRevision replacement;
* worker pool restart;
* Runtime configuration transition;
* security containment.

---

# 47. Drain Modes

Possible modes:

```text
REMOVE_NON_CONTROL
REMOVE_BACKGROUND
REMOVE_EXECUTION_SCOPE
REMOVE_EXECUTION_REVISION
REMOVE_QUEUE_CLASS
REMOVE_ALL
HOLD_PENDING
```

Queue MUST NOT decide the scope semantically by itself.

---

# 48. Drain Authority

Drain is requested by the appropriate Runtime owner.

Examples:

```text
Runtime Control
Scheduler lifecycle
Cancellation Coordinator
Shutdown coordination
```

Queue only executes the instruction.

---

# 49. Scheduler PAUSED Behavior

When Scheduler admission is `PAUSED`:

* no new normal Business work is admitted;
* queued work may be held or drained by explicit policy;
* CONTROL work remains available;
* cancellation/shutdown continues;
* queue metrics remain available.

This does NOT mean a Reading Session is paused.

---

# 50. Shutdown

Recommended:

```text
Stop New Scheduler Admission
        |
        v
Stop Normal Dispatch
        |
        v
Apply Queue Drain Instructions
        |
        v
Preserve Required Control Work
        |
        v
Finalize Queue Metadata
        |
        v
Dispose Queue Infrastructure
```

Running Attempts are outside Queue ownership.

---

# 51. Backpressure

Queue participates in backpressure by exposing capacity/pressure state.

It does NOT decide the high-level admission response.

---

# 52. Queue Pressure Signals

Possible:

```text
NORMAL
ELEVATED
HIGH
CRITICAL
```

or:

```text
SOFT_LIMIT
HARD_LIMIT
```

These are operational observations.

---

# 53. Pressure Flow

Correct:

```text
Queue Pressure
        |
        v
Queue Metrics / Signal
        |
        v
Scheduler
        |
        v
Admission Reduced
```

Possible Scheduler reactions:

* defer Background work;
* reject obsolete work;
* apply explicit replacement;
* reduce expensive execution;
* protect CONTROL/INTERACTIVE capacity.

---

# 54. Queue Must Not Expand Unbounded

A Queue MUST NOT respond to saturation by increasing memory without bound.

Bounded capacity is a hard Runtime property.

---

# 55. Upstream Coordination

Queue MUST NOT directly pause:

* Business Module;
* Business Stage;
* Pipeline Orchestrator.

Correct:

```text
Queue Pressure
        |
        v
Scheduler admission changes
        |
        v
Runtime Control materializes less work
```

---

# 56. Runtime Control Materialization Boundary

Runtime Control SHOULD avoid creating unnecessary downstream WorkItems when sustained pressure indicates work is already obsolete or not yet useful.

But Queue itself does not decide Business progression.

---

# 57. Artifact Reference

Queued item stores:

```text
ArtifactRef
```

only.

Worker acquires required lease immediately before execution.

---

# 58. Queue Artifact Validation

Queue MAY validate only technical reference shape:

* non-empty ID;
* valid reference encoding;
* supported reference type.

It MUST NOT validate:

* business correctness;
* execution authority;
* cache compatibility;
* Domain validity.

---

# 59. Cancellation Boundary

Correct:

```text
Cancellation Requested
        |
        v
Runtime Control Revokes Authority
        |
        v
Queue Removal Instruction
        |
        v
Queued Item Removed
```

Queue does not own cancellation authority.

---

# 60. Running Attempt Cancellation

Running Attempt has already left Queue ownership.

Cancellation is handled by:

```text
Runtime Control
Cancellation Coordinator
Worker / Adapter
```

---

# 61. Retry Boundary

Queue knows no Retry policy.

Correct:

```text
Attempt failed
        |
        v
Retry Policy
        |
        v
new Attempt
        |
        v
Scheduler
        |
        v
new Queue Item
```

Queue does not store authoritative retry count/budget.

---

# 62. Fallback Boundary

Queue has no knowledge of:

```text
provider fallback
model fallback
route fallback
```

A new execution binding, if produced externally, simply appears as another admitted Attempt candidate.

---

# 63. Scheduler Interaction

Scheduler owns:

* admission;
* priority/order semantics;
* fairness;
* resource-budget decision;
* explicit replacement decision;
* dispatch selection policy.

Queue owns:

* storage of admitted item;
* bounded capacity;
* queue state;
* atomic replacement/removal;
* atomic dispatch transfer;
* capacity metrics.

---

# 64. Worker Interaction

Worker:

* receives dispatched item;
* acquires Artifact leases;
* executes Attempt;
* reports Completion to Runtime Control;
* releases resources.

Worker MUST NOT mutate Queue internals or re-enqueue itself.

---

# 65. Queue Metrics

Recommended:

```text
current length
capacity
utilization ratio
enqueue count
dispatch count
replace count
remove count
drain count
duplicate rejection count
technical enqueue failure count
dispatch failure count
average wait time
P50/P90/P95/P99 wait time
saturation duration
CONTROL utilization
INTERACTIVE utilization
BACKGROUND utilization
```

Metrics MUST contain no user content.

---

# 66. Queue Events

Possible normalized events:

```text
WorkEnqueued
WorkSelected
WorkDispatched
WorkReplaced
WorkRemoved

QueuePressureChanged
QueueDrainStarted
QueueDrainCompleted
QueueIntegrityFailed
QueueDispatchFailed
```

---

# 67. Event Boundary

Queue events describe Queue infrastructure state.

Queue MUST NOT emit terminal WorkItem outcomes such as:

```text
WorkSucceeded
WorkFailed
WorkCancelled
```

as if it owned them.

---

# 68. Integrity Rules

Queue MUST ensure:

1. no duplicate queued Attempt identity;

2. capacity never exceeds hard contract;

3. dispatched item is no longer waiting;

4. replaced item cannot dispatch;

5. removed item cannot dispatch;

6. drained item cannot dispatch;

7. queue state transitions are valid;

8. immutable ordering metadata does not mutate after enqueue;

9. replacement is atomic;

10. dispatch ownership transfer is atomic;

11. CONTROL capacity remains protected according to policy;

12. technical failure never creates duplicate execution.

---

# 69. Failure Isolation

If one Queue partition fails:

```text
Stop affected dispatch
        |
        v
Preserve CONTROL path where possible
        |
        v
Notify Scheduler / Runtime Control
        |
        v
Hold / drain affected queued items safely
        |
        v
Emit diagnostics
```

Queue failure MUST NOT corrupt:

* Runtime Artifact;
* Domain data;
* accepted Business result;
* Runtime authority.

---

# 70. Recovery

Queue recovery MAY:

* reconstruct pending metadata from authoritative Runtime state if supported;
* discard non-durable queue state;
* require Runtime Control to rematerialize/re-admit eligible work.

Queue MUST NOT assume it is the source of truth for WorkItem logical state.

---

# 71. Durable Queue Boundary

MVP Queue is expected to be process-local/in-memory.

Future durable queue support MUST preserve:

```text
Queue state
    !=
Runtime Control authority state
```

Durable delivery MUST NOT cause stale work to regain authority after restart.

---

# 72. Process Restart

After host restart:

```text
old queued item
```

MUST NOT automatically execute solely because it existed before crash.

Runtime authority and eligibility must be reconstructed/revalidated first.

---

# 73. MVP Queue Model

MVP MAY use:

```text
CONTROL Queue
INTERACTIVE Queue
BACKGROUND Queue
MAINTENANCE Queue
```

Each SHOULD be:

* in-memory;
* bounded;
* process-local;
* typed;
* observable;
* replacement-capable.

No external message broker is required.

---

# 74. MVP Replacement Policy

MVP SHOULD use replacement only for explicitly declared fast-changing interactive work.

Example:

```text
replacementKey supplied
+
replacementPolicy = LATEST_ELIGIBLE
```

Do NOT globally assume:

```text
same WorkType
    => replace older
```

---

# 75. MVP Dispatch Policy

Conceptually:

1. preserve CONTROL dispatch;

2. prefer current useful INTERACTIVE work;

3. dispatch supporting work when capacity permits;

4. dispatch BACKGROUND when interactive latency remains protected;

5. run MAINTENANCE with bounded low priority;

6. never dispatch externally-marked ineligible work.

Actual selection policy remains Scheduler-owned.

---

# 76. Example — Rapid Scrolling

```text
ExecutionRevision 30 item queued

ExecutionRevision 31 becomes current

Runtime Control / Scheduler:
    mark Revision 30 queued item obsolete/replaced

ExecutionRevision 31 item queued

ExecutionRevision 32 becomes current

Revision 31 item replaced

Worker becomes available

Scheduler selects Revision 32 item

Queue dispatches Revision 32
```

Queue never independently decides which revision is newest.

---

# 77. Example — Queue Saturation

```text
INTERACTIVE Queue reaches pressure threshold
        |
        v
Queue emits capacity signal
        |
        v
Scheduler reduces low-value admission
        |
        v
Explicitly replaceable obsolete items removed
        |
        v
CONTROL capacity remains protected
```

---

# 78. Example — Cancellation

```text
ExecutionScope cancellation requested
        |
        v
Runtime Control revokes authority
        |
        v
Queue receives REMOVE_EXECUTION_SCOPE
        |
        v
matching queued items removed
        |
        v
running Attempts handled outside Queue
```

---

# 79. Example — Retry

```text
Attempt 1 fails
        |
        v
Retry Policy permits retry
        |
        v
Attempt 2 created
        |
        v
Scheduler ADMIT
        |
        v
Attempt 2 queued
```

Queue does not care whether Attempt 2 is Retry except for optional diagnostics metadata.

---

# 80. Example — Dispatch Failure

```text
Queued item selected
        |
        v
Worker handoff fails
        |
        v
Queue reports DISPATCH_FAILED
        |
        v
Runtime Control / Scheduler decide next action
```

Queue does not silently requeue itself.

---

# 81. Architecture Invariants

1. Work Queue owns queued position only.

2. Queue does not own WorkItem terminal state.

3. Queue does not own Runtime authority.

4. Queue does not decide Scheduler admission policy.

5. Queue does not create Retry.

6. Queue does not choose Fallback.

7. Queue does not cancel running Attempt.

8. Queue does not execute WorkItem.

9. Queue does not contain large payloads.

10. Queue does not contain raw secrets.

11. Queue is always bounded.

12. CONTROL capacity is protected.

13. Queue does not infer current ExecutionRevision.

14. Queue does not invent replacement semantics.

15. Replacement metadata is externally supplied.

16. Replacement is atomic.

17. Dispatched item is no longer Queue-owned.

18. Replaced/removed/drained item cannot dispatch.

19. Retry creates another queue item for another Attempt.

20. Queue remains capability-independent.

21. Queue does not define OCR/Translation-specific topology.

22. Queue pressure creates signals/backpressure, not independent admission policy.

23. Queue does not independently `DEFER` or `REJECT` as Scheduler policy.

24. Queue ordering follows Scheduler semantics.

25. Queue does not reinterpret business priority.

26. Queue artifact validation is technical only.

27. Cancellation removal follows external authority.

28. Queue lifecycle ends before physical Attempt execution.

29. Queue failure does not corrupt Runtime authority or Business data.

30. Shutdown stops admission before destructive Queue drain.

31. Durable queue support must not resurrect stale authority.

32. ExecutionScope/ExecutionRevision terminology is canonical.

---

# 82. Recommended MVP

CRAI MVP SHOULD support:

* process-local Work Queues;
* bounded capacity;
* CONTROL/INTERACTIVE/BACKGROUND/MAINTENANCE classes;
* immutable lightweight queue items;
* WorkItemId + AttemptId duplicate prevention;
* ArtifactRef-only payload references;
* Scheduler-controlled ordering;
* explicit replacement key/policy;
* atomic replacement;
* explicit removal instructions;
* atomic dispatch transfer;
* queue drain;
* queue pressure metrics;
* dispatch-failure reporting;
* graceful shutdown.

MVP MAY defer:

* durable queue persistence;
* distributed queue;
* remote workers;
* queue replication;
* persistent delivery acknowledgments;
* advanced multi-consumer work stealing;
* dynamic queue topology.

---

# 83. Open Decisions

The following remain open:

* exact queue-item schema;
* physical queue count;
* heap vs deque vs partitioned implementation;
* Scheduler-select vs ordering-token dispatch model;
* dispatch-token mechanism;
* replacement-policy taxonomy;
* queue partition keys;
* per-ExecutionScope capacity;
* queue pressure thresholds;
* durable queue recovery model;
* process-restart rematerialization strategy;
* dispatch failure recovery;
* fairness metadata retention;
* metrics sampling.

---

# 84. Related Documents

Runtime:

* `PIPELINE_RUNTIME.md`
* `SCHEDULER.md`
* `RUNTIME_COMPONENTS.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`
* `MEMORY_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `THREADING_MODEL.md`
* `PERFORMANCE_MODEL.md`
* `RUNTIME_CONFIG.md`
* `RUNTIME_OBSERVABILITY.md`

---

# 85. Completion Criteria

`WORK_QUEUE.md` is synchronized when:

* Queue remains generic;
* Queue owns only waiting position;
* ExecutionScope/ExecutionRevision terminology is used;
* queue lifecycle ends at dispatch/removal;
* Queue does not own Retry/cancellation authority;
* replacement semantics are externally supplied;
* Queue does not invent current-revision logic;
* Scheduler remains the only admission policy owner;
* Queue pressure does not become independent DEFER/REJECT policy;
* Queue is bounded/lightweight;
* large payloads use ArtifactRef;
* CONTROL capacity is preserved;
* dispatch transfer is atomic;
* process restart does not resurrect stale queued authority.

---

# 86. Summary

CRAI Work Queue follows:

```text
Runtime Control
    creates eligible execution work

        |
        v

Scheduler
    decides admission/order

        |
        v

Work Queue
    holds bounded waiting position

        |
        v

Worker
    executes Attempt

        |
        v

Runtime Control
    accepts/rejects Completion
```

The central boundary is:

```text
Queue stores references, not payloads.

Queue owns waiting, not execution.

Queue applies decisions, not policy.

Queue does not invent execution meaning.
```
