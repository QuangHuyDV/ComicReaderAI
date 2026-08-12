# Runtime Threading Model

* **Document:** Runtime Architecture / Threading Model
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how CRAI maps Runtime work onto logical execution contexts while preserving:

* execution-authority correctness;
* UI responsiveness;
* bounded concurrency;
* resource ownership;
* thread/process affinity;
* cancellation;
* shutdown safety;
* deterministic state transitions.

An `ExecutionContext` is a logical scheduling/execution abstraction.

```text
ExecutionContext
    !=
OS Thread
    !=
Thread Pool Worker
    !=
Coroutine
    !=
Task
    !=
Process
```

An implementation MAY map one or more logical contexts onto:

```text
thread
event loop
task scheduler
worker pool
GPU queue
process
remote runtime
```

provided architecture semantics remain unchanged.

---

# 2. Scope

This document covers:

* logical execution contexts;
* context ownership;
* Runtime Control serialization;
* UI execution boundary;
* Worker execution;
* CPU/GPU/native execution;
* provider I/O;
* callbacks;
* execution affinity;
* immutable cross-context transfer;
* Resource Lease;
* synchronization;
* event dispatch;
* cancellation;
* timers;
* shutdown;
* process isolation;
* threading/runtime metrics.

This document does NOT define:

* exact threading API;
* UI framework dispatcher syntax;
* Scheduler priority algorithm;
* memory budget values;
* provider SDK internals;
* Business Module algorithms;
* final process topology;
* Business workflow semantics.

---

# 3. Core Principles

1. `ExecutionContext` is logical.

2. Physical thread/process mapping is an implementation detail.

3. Runtime execution-orchestration state has one logical writer.

4. Runtime Control owns execution authority.

5. Runtime Control does NOT own every Runtime component's mutable state.

6. Worker does not mutate Runtime Control state directly.

7. Worker does not own shared Runtime Artifact payload after transfer.

8. Published Runtime Artifact is immutable.

9. Resource Lease does not grant ownership.

10. Provider callback never grants execution authority.

11. Physical completion order does not determine accepted logical outcome.

12. UI context does not run heavy execution work.

13. Control path is isolated from heavy-work starvation.

14. CPU/GPU/provider/process concurrency is bounded.

15. Blocking wait is forbidden on UI and Runtime Control paths.

16. Event subscribers run on declared execution contexts.

17. Cancellation revokes execution authority before physical drain.

18. Shutdown stops admission before destructive cleanup.

19. Execution affinity is explicit.

20. Publication/ownership transfer is atomic or rollback-safe.

21. Threading correctness does not depend on Cache availability.

22. Presentation/UI commit semantics remain Presentation-owned.

---

# 4. Architectural Position

```text
Runtime Control
        |
        v
WorkItem / Attempt Candidate
        |
        v
Scheduler
        |
        v
Work Queue / Execution Resource Pool
        |
        v
Worker Execution
        |
        v
Business Module / Execution Adapter
        |
        v
Completion
        |
        v
Runtime Control
        |
        v
Execution Authority Validation
        |
        v
Runtime Artifact Publication
        |
        v
Business / Presentation Boundary
```

Threading Model owns execution placement mechanics.

It does NOT own:

* Business workflow;
* terminal WorkItem outcome;
* Business result correctness;
* Presentation semantics.

---

# 5. Logical Execution Contexts

Core Runtime contexts MAY include:

```text
Application Runtime
├── UI Execution Context
├── Runtime Control Context
├── Execution Resource Pool(s)
├── Provider I/O Context
├── Maintenance / Control Context
└── Optional Isolated Process Context
```

Application-specific contexts MAY additionally include:

```text
Capture Context
Observation Context
Native Serial Context
GPU Context
```

---

# 6. Core vs Application-Specific Contexts

Critical distinction:

```text
Core Runtime Context
    = generic execution architecture
```

```text
Application-Specific Context
    = CRAI capability/platform need
```

Capture/Observation contexts are useful for CRAI but are not universal Runtime abstractions.

---

# 7. Context Merging

Implementation MAY merge contexts when:

* affinity remains valid;
* control path remains protected;
* concurrency remains bounded;
* blocking policy remains valid;
* ownership boundaries remain unchanged;
* observability can still distinguish logical activity.

---

# 8. Execution Context Ownership

Recommended:

| Context                  | Logical Owner                                      |
| ------------------------ | -------------------------------------------------- |
| UI Context               | Presentation/Application                           |
| Runtime Control Context  | Runtime Control                                    |
| Execution Resource Pool  | Worker Execution infrastructure                    |
| Provider I/O Context     | Provider Runtime Gateway / Execution Adapter       |
| GPU Context              | GPU/Resource Coordinator or owning Runtime Adapter |
| Maintenance Context      | Runtime infrastructure                             |
| Isolated Process Context | Process Supervisor                                 |
| Capture Context          | Capture runtime adapter                            |
| Observation Context      | owning observation adapter                         |

Provider Management does NOT own execution-thread/runtime context.

---

# 9. Execution Context Lifecycle

Recommended:

```text
CREATED
    |
    v
INITIALIZED
    |
    v
RUNNING
    |
    +--> PAUSED
    |
    v
DRAINING
    |
    v
STOPPED
    |
    v
DISPOSED
```

Not every context supports `PAUSED`.

Disposal MUST respect:

* active Attempt;
* active Lease;
* physical child operations;
* execution affinity;
* shutdown ordering.

---

# 10. UI Execution Context

UI Context is the only context allowed to mutate framework-bound UI state.

It may own:

* UI control state;
* Presentation target state;
* visible replacement;
* loading/error display;
* selection UI;
* framework-bound graphics resources;
* UI-local lifecycle.

---

# 11. UI Restrictions

UI Context MUST NOT:

* run heavy Recognition/Translation work;
* perform synchronous provider calls;
* block waiting for Runtime completion;
* process large images synchronously;
* hold Runtime Control locks;
* dispose shared Runtime Artifact backing;
* mutate Runtime Control state directly.

---

# 12. UI Command Flow

```text
User Action
        |
        v
Application/UI validates local intent
        |
        v
Application Command
        |
        v
Business / Runtime Boundary
        |
        v
Runtime executes asynchronously
        |
        v
Accepted result / notification
        |
        v
UI Context updates Presentation state
```

UI does not synchronously wait for full pipeline completion.

---

# 13. Presentation Commit Boundary

Recommended:

```text
Accepted Presentation Input
        |
        v
Runtime execution relevance validated
        |
        v
Commit request queued to UI Context
        |
        v
Presentation validates target/view state
        |
        v
Atomic visible replacement
```

These are two different validations.

---

# 14. Runtime Validation vs UI Validation

```text
Runtime Control
    asks:
    "Is this execution still current?"
```

```text
Presentation/UI
    asks:
    "Is this target/view still valid for commit?"
```

Runtime Control does NOT own Presentation commit semantics.

---

# 15. Runtime Control Context

Runtime Control Context serializes execution-orchestration state transitions.

It MAY own:

* current ExecutionRevision per ExecutionScope;
* WorkItem logical state;
* Attempt lineage;
* execution-authority state;
* cancellation authority;
* accepted execution outcome;
* execution replacement;
* shutdown coordination.

---

# 16. Runtime Control Does Not Own

Runtime Control does NOT own:

* Scheduler internal state;
* Work Queue internal state;
* Runtime Artifact backing state;
* Resource Manager state;
* Provider Runtime internal state;
* Plugin lifecycle state;
* Presentation/UI state;
* Business result correctness;
* canonical Provider Configuration.

---

# 17. Runtime Control Context Requirements

Runtime Control processing MUST be:

```text
serialized
fast
deterministic
non-blocking
bounded
```

It MUST NOT:

* execute heavy domain work;
* wait for provider;
* wait for Worker;
* wait for UI;
* perform durable I/O synchronously;
* hold broad locks.

---

# 18. Single Logical Writer

Core execution-orchestration state has one logical writer.

Other contexts:

```text
read immutable snapshots
submit commands
submit Completion
```

They MUST NOT mutate Runtime Control objects directly.

Single logical writer does NOT imply one dedicated OS thread.

---

# 19. Runtime Command Model

Runtime commands SHOULD describe generic execution state transitions.

Examples:

```text
EXECUTION_SCOPE_OPENED
EXECUTION_SCOPE_CLOSED
EXECUTION_REVISION_CREATED
EXECUTION_REVISION_REPLACED
WORK_ITEM_MATERIALIZED
ATTEMPT_COMPLETION_REPORTED
CANCELLATION_REQUESTED
RETRY_READY
RESOURCE_PRESSURE_CHANGED
RUNTIME_STOP_REQUESTED
```

Exact command names remain implementation-specific.

---

# 20. Business Commands Are External

Commands such as:

```text
START_READING_SESSION
CHANGE_SOURCE
CHANGE_LANGUAGE
REQUEST_TRANSLATION
```

belong to Application/Business use-case contracts.

Runtime Control receives their resolved execution consequences.

---

# 21. Command Payload

Runtime command payload SHOULD contain only lightweight data:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
ArtifactRef
RuntimeConfigurationSnapshotId
ReasonCode
CorrelationId
```

No large payload or raw secrets.

---

# 22. Scheduler Execution Context

Scheduler MAY run inside Runtime Control Context in MVP if:

* decision remains fast;
* candidate set remains small;
* no blocking operation occurs;
* no heavy policy computation occurs.

Scheduler MAY receive a separate logical context later.

---

# 23. Scheduler Separation

Even when colocated physically:

```text
Runtime Control
    owns authority
```

```text
Scheduler
    owns admission policy
```

Co-location MUST NOT merge ownership.

---

# 24. WorkItem Execution Mapping

A WorkItem does not architecturally “run on a thread.”

```text
WorkItem
    |
    v
Attempt
    |
    v
Scheduler Admission
    |
    v
Execution Context Selection
    |
    v
Worker / Adapter Assignment
    |
    v
Physical Thread / Event Loop / GPU Queue / Process
```

---

# 25. Execution Resource Pool

`ExecutionResourcePool` is a generic abstraction.

Possible implementations:

```text
CPU Worker Pool
GPU Queue
Dedicated Native Thread
Provider Async Capacity
Process Pool
```

Pool MUST be:

* bounded;
* execution-requirement aware;
* pressure-aware;
* observable;
* cancellation-compatible where platform permits.

---

# 26. Pool Boundary

Pool manages physical execution capacity.

It does NOT decide:

* business priority;
* execution authority;
* Retry;
* Fallback;
* WorkItem terminal outcome.

---

# 27. Capture Context

Capture MAY require a dedicated logical context because of:

* OS callback;
* affinity;
* GPU-backed surfaces;
* pacing/timing;
* source lifecycle.

This context belongs to Capture runtime integration, not generic Runtime business orchestration.

---

# 28. Capture Backpressure

Continuous capture SHOULD avoid unbounded frame queues.

Possible latest-value semantics:

```text
Frame A pending
Frame B arrives -> A replaceable
Frame C arrives -> B replaceable
```

The owning Capture/Observation contract defines semantic replacement.

Queue/Scheduler applies the resulting metadata.

---

# 29. Observation Context

Observation MAY perform:

* change detection;
* stability detection;
* fingerprint preparation;
* source candidate preparation.

Observation does NOT directly create canonical ExecutionRevision authority.

It reports/business-plans through the owning orchestration boundary.

---

# 30. CPU-Bound Work

CPU-heavy work MUST stay outside:

```text
UI Context
Runtime Control Context
```

Possible examples:

* image processing;
* local inference;
* normalization;
* document transformation;
* Presentation-model construction.

---

# 31. CPU Execution Pool

CPU concurrency MUST remain bounded to avoid:

* oversubscription;
* memory spikes;
* excessive context switching;
* control-path starvation;
* shutdown delays.

MVP SHOULD start conservatively.

---

# 32. Execution Limits

Pool capacity and WorkType/execution-binding concurrency are separate.

```text
CPU Pool Capacity = N
Execution Binding Limit = M
WorkType Limit = K
```

Scheduler combines these constraints for admission.

---

# 33. Work Granularity

Work SHOULD be:

* large enough to avoid excessive scheduling overhead;
* small enough for useful cancellation/fairness;
* bounded in memory/cost;
* owner-defined according to Business/WorkType semantics.

Threading Model does not define semantic chunk size.

---

# 34. I/O-Bound Work

I/O SHOULD use asynchronous APIs when practical.

Examples:

* remote provider;
* Storage;
* telemetry;
* file/document loading;
* authentication.

Async does NOT mean unlimited concurrency.

---

# 35. Provider I/O Context

Recommended:

```text
Attempt
    |
    v
Execution Adapter
    |
    v
Asynchronous Provider Operation
    |
    v
Callback / Completion
    |
    v
Normalize
    |
    v
Runtime Completion Command
```

---

# 36. Provider Callback Boundary

Provider callback MUST:

1. capture minimum execution identity;

2. normalize provider output/error;

3. release request-local resources when safe;

4. submit Completion;

5. never mutate Runtime Control directly;

6. never grant authority;

7. never advance downstream Business work;

8. never update UI directly;

9. never perform orchestration-level Retry.

---

# 37. Provider Execution Declaration

Executable provider/runtime binding MAY declare:

```text
ExecutionClass
ExecutionAffinity
MaximumConcurrency
CancellationSupport
ProcessIsolation
MemoryCostHint
GpuCostHint
BlockingBehavior
```

These are Runtime execution requirements.

---

# 38. Execution Class

Possible:

```text
CPU
GPU
REMOTE_IO
NATIVE_SERIAL
PROCESS
HYBRID
```

Execution Class is not a Business capability.

---

# 39. Execution Affinity

`ExecutionAffinity` MAY include:

```text
ANY
SPECIFIC_THREAD
SPECIFIC_EVENT_LOOP
SPECIFIC_PROCESS
SPECIFIC_GPU_QUEUE
SERIAL_CONTEXT
UI_CONTEXT
```

Affinity MUST be explicit before execution.

---

# 40. Affinity Boundary

Affinity describes:

```text
where execution/resource use is valid
```

It does NOT grant:

* execution authority;
* ownership;
* Scheduler admission.

---

# 41. GPU Context

GPU execution MAY require:

* dedicated command queue;
* serial model inference;
* explicit synchronization;
* memory budgeting;
* rendering-capacity protection.

Parallel GPU execution is not assumed beneficial.

---

# 42. GPU Ownership

GPU execution scheduling/lifecycle MAY be owned by:

```text
GPU/Resource Coordinator
or
Provider Runtime Adapter
```

depending on implementation.

Provider Management does not own GPU execution context.

---

# 43. Process Isolation

Isolated process MAY be appropriate for:

* unstable native library;
* large local model;
* strong cancellation requirement;
* third-party plugin;
* high-memory import;
* risky provider adapter.

Isolated process communicates through explicit contracts only.

It MUST NOT mutate Runtime Control state directly.

---

# 44. Cross-Process Payload

Large payloads SHOULD avoid repeated serialization.

Possible mechanisms:

```text
shared memory
memory-mapped file
temporary file
shared graphics surface
RuntimeArtifact handle
IPC-capable ResourceRef
```

Process topology belongs to `PROCESS_TOPOLOGY.md`.

---

# 45. Worker Ownership

Worker owns:

* Attempt-local resources;
* acquired Resource Leases;
* request-local handles;
* candidate output before ownership transfer.

Worker does NOT own:

* shared Runtime Artifact after transfer;
* ExecutionScope/ExecutionRevision authority;
* Cache Policy;
* downstream scheduling;
* Presentation/UI state.

---

# 46. Worker Execution Contract

```text
Receive immutable Attempt input
        |
        v
Validate Cancellation Context
        |
        v
Acquire required Resource Leases
        |
        v
Validate physical inputs
        |
        v
Execute bounded operation
        |
        v
Observe cancellation
        |
        v
Produce Candidate Output
        |
        v
Submit Completion
        |
        v
Release Attempt-local resources / leases
```

Runtime Control determines execution acceptance.

---

# 47. Candidate Publication Flow

```text
Worker Completion
        |
        v
Candidate Runtime Artifact
        |
        v
Runtime Control validates execution authority
        |
        v
Runtime Artifact Store accepts ownership
        |
        v
Atomic publication
```

Publication does not directly occur from Worker.

Business acceptance remains separate.

---

# 48. Shared Mutable State

Forbidden by default:

* multiple Workers mutate one payload;
* provider callback mutates Runtime state;
* UI and Runtime share mutable collections;
* Worker modifies Queue priority;
* Worker modifies ExecutionRevision graph;
* Capture callback directly mutates Business state.

Mutable builders remain local until finalized.

---

# 49. Cross-Context Data

Data crossing logical contexts SHOULD be immutable or treated as immutable.

Examples:

```text
RuntimeCommand
WorkItem
AttemptInput
ArtifactRef
Completion
RuntimeEvent
ResolvedExecutionBinding
PresentationInput
```

---

# 50. Synchronization Strategy

Preferred order:

1. explicit ownership;

2. single logical writer;

3. serialized command processing;

4. immutable data;

5. Resource Lease;

6. bounded concurrent structures;

7. narrow lock;

8. broad locking only as last resort.

---

# 51. Lock Rules

If lock is required:

* scope is narrow;
* hold duration is short;
* no provider call while held;
* no UI dispatch while held;
* no heavy work while held;
* no shutdown wait while held;
* nested locks avoided.

---

# 52. Blocking Policy

Blocking is forbidden on:

```text
UI Context
Runtime Control Context
Capture OS callback
Provider callback
Event Bus dispatch loop
```

Dedicated Worker MAY block when:

* API is inherently synchronous;
* capacity is accounted;
* timeout is bounded;
* cancellation policy is explicit;
* UI/control path remains unaffected.

---

# 53. Sync-over-Async

Avoid patterns equivalent to:

```text
Async().Wait()
Async().Result
```

on UI/Runtime Control paths.

Async chains SHOULD remain async end-to-end where practical.

---

# 54. Event Dispatch

```text
Publisher
    |
    v
Event Dispatcher
    |
    v
Declared Execution Context
    |
    v
Subscriber
```

Publisher MUST NOT synchronously execute arbitrary subscriber logic.

Subscriber wishing to change Runtime state sends a Runtime command.

---

# 55. Event Ordering

Ordering guarantees MUST be explicit and scoped.

Possible ordering scopes:

```text
ExecutionScopeId
Business Aggregate Key
Event Stream Key
Provider Runtime Instance
Plugin Runtime Instance
```

There is no global total-order guarantee by default.

---

# 56. Event Reentrancy

State transition completes before event-driven follow-up command is processed.

```text
Transition Completes
        |
        v
Event Queued
        |
        v
Subscriber Executes
        |
        v
New Command Queued
```

---

# 57. Cancellation and Contexts

```text
Cancellation Request
        |
        v
Runtime Control revokes execution authority
        |
        v
Queued work removed
        |
        v
Running execution contexts signaled
        |
        v
Attempt drains / stops / becomes abandoned
        |
        v
Physical resources released when eligible
```

Caller does not block indefinitely waiting for physical stop.

---

# 58. Generic Cancellation Checkpoints

Worker/Adapter checkpoints MAY include:

* before expensive acquisition;
* before heavy execution;
* between bounded batches;
* after external operation;
* before Candidate creation;
* before Completion submission.

Presentation commit is NOT a Worker cancellation checkpoint.

---

# 59. Presentation Revalidation

Before visible Presentation commit:

```text
Presentation/Application
    revalidates target/view state
```

and MAY consume current Runtime execution-relevance information.

---

# 60. Timers

Timer callback MUST remain short.

Possible uses:

* capture pacing;
* stability timing;
* provider timeout;
* Retry delay;
* idle unload;
* metrics sampling;
* cleanup retry scheduling.

Timer callback submits a command/signal rather than executing heavy work.

---

# 61. Retry Delay

Delayed Retry MUST NOT hold a blocked thread.

```text
Cancelable delay
        |
        v
Timer completes
        |
        v
Execution authority revalidated
        |
        v
Retry-ready command
        |
        v
new Attempt may be created
```

---

# 62. Control Path Protection

Control path requires reserved execution capacity for:

* cancellation;
* execution replacement;
* Completion processing;
* shutdown;
* timeout handling;
* fatal containment;
* resource-pressure response.

Heavy work MUST NOT consume all capacity required for control.

---

# 63. Workload Classes

Possible Runtime workload classes:

```text
CONTROL
UI_DISPATCH
CAPTURE
OBSERVATION
CPU_LIGHT
CPU_HEAVY
NETWORK_IO
GPU
NATIVE_SERIAL
PROCESS
MAINTENANCE
```

These are execution classes, not Business Stage types.

---

# 64. Background Work

Background work SHOULD:

* use low bounded concurrency;
* yield to interactive/control work;
* reduce under pressure;
* remain cancelable;
* avoid UI-affine resource ownership;
* not block shutdown.

---

# 65. Runtime Artifact Store Concurrency

Runtime Artifact Store MUST be:

* thread-safe;
* ownership-safe;
* lease-safe;
* publication-safe;
* disposal-safe.

Consumers observe either:

```text
not available
```

or:

```text
complete published Runtime Artifact
```

not partially published payload.

---

# 66. Execution State Store Concurrency

Execution State Store mutations associated with execution authority occur through Runtime Control's logical writer path.

Worker only consumes snapshots/references.

Worker MUST NOT mutate ExecutionRevision relation directly.

---

# 67. Presentation Replacement

Visible Presentation replacement SHOULD be atomic from the application's observable UI perspective.

Progressive rendering, if supported, requires an explicit consistency model.

---

# 68. Race Prevention

Important races include:

* ExecutionRevision replaced while Attempt completes;
* cache retention removed while Lease active;
* Presentation target closed while commit is queued;
* execution binding replaced while provider callback arrives;
* ExecutionScope closed while capture callback runs;
* logical disposal while native use continues.

Primary mechanisms:

```text
execution-authority validation
immutable payload
Resource Lease
serialized Runtime Control
execution affinity
atomic publication
Presentation target validation
```

---

# 69. Deadlock Prevention

1. UI never synchronously waits for Runtime.

2. Runtime Control never waits for UI.

3. Locks are not held across provider/native execution.

4. Artifact Store lock does not wait for Worker.

5. Nested locks are minimized.

6. External operations are bounded by timeout/cancellation policy.

7. Shutdown is asynchronous/bounded.

8. Event subscriber does not directly re-enter a state transition.

---

# 70. Completion Across Contexts

Worker/Adapter produces normalized Completion such as:

```text
AttemptCompletion
├── ExecutionScopeId
├── ExecutionRevisionId
├── WorkItemId
├── AttemptId
├── BusinessStageId?
├── WorkType
├── PhysicalOutcome
├── RuntimeErrorRef?
└── TimingMetadata
```

Runtime Control decides execution acceptance.

---

# 71. Physical Outcome

Physical Attempt outcomes MAY include:

```text
COMPLETED
FAILED
CANCELLED
ABANDONED
```

`STALE` remains an authority-rejection concept, not physical Worker outcome.

---

# 72. Unhandled Worker Failure

Unhandled Worker/Adapter failure SHOULD:

* release Attempt-local resources where safe;
* release leases;
* report Completion/failure to Runtime Control;
* update relevant runtime-health observation;
* avoid crashing whole application when isolation contains failure;
* trigger stronger isolation consideration when repeated native failure occurs.

---

# 73. Health Boundary

Worker/Adapter MAY emit operational health observations.

It MUST NOT directly mutate canonical Provider Health/Policy state unless it owns that projection contract.

---

# 74. Shutdown

Recommended:

```text
Stop New Admission
        |
        v
Revoke Execution Authority
        |
        v
Remove Queued Work
        |
        v
Signal Running Contexts
        |
        v
Bounded Drain
        |
        v
Mark Remaining Attempts Abandoned
        |
        v
Release Leases / Attempt Resources
        |
        v
Dispose Contexts / Provider Runtime / Resources
```

Exact dependency order belongs to `BOOT_SEQUENCE.md`.

---

# 75. Structured Concurrency

Logical Runtime ownership hierarchy:

```text
Application
    |
    v
ExecutionScope
    |
    v
ExecutionRevision
    |
    v
WorkItem
    |
    v
Attempt
    |
    v
Physical Child Operations
```

---

# 76. Structured Concurrency Rule

Child execution MUST have a tracked owning parent.

When parent ends/revokes:

* children receive cancellation;
* child work cannot escape tracking;
* non-cancelable child becomes abandoned/tracked;
* physical resources remain accounted until release.

No specific structured-concurrency framework is required.

---

# 77. Metrics

Runtime SHOULD measure:

```text
UI dispatch delay
Runtime command-queue length
Runtime Control processing delay
control-path delay
capture callback delay
observation latency
execution-context utilization
Worker utilization
CPU/GPU saturation
provider in-flight count
queue wait
lease wait
publication delay
authority-validation delay
cancellation acknowledgment
event dispatch delay
blocked Worker count
active thread count
isolated-process restart count
context drain duration
```

---

# 78. MVP Execution Contexts

Recommended MVP:

```text
1 UI Context
1 Serialized Runtime Control Context
1 Capture/Observation Serial Context
1 Bounded CPU Execution Pool
Asynchronous Provider I/O
Optional Serial GPU/Native Context
```

Capture and Observation MAY share one serial context when lightweight.

---

# 79. MVP Rules

1. UI mutates UI only.

2. Heavy work never executes on UI.

3. Runtime execution-authority state has one logical writer.

4. Workers receive immutable inputs.

5. Workers return Completion and candidate output.

6. Workers do not schedule downstream Business work.

7. Provider callback only normalizes/submits Completion.

8. Concurrency is bounded.

9. Capture MAY use explicit latest-value semantics.

10. Runtime execution relevance and Presentation target validity are validated separately.

11. Cancellation never blocks caller indefinitely.

12. No unbounded task/thread creation.

13. Worker does not own shared payload after transfer.

14. Resource Lease protects shared physical use.

15. Shutdown stops admission before drain.

16. Provider Management does not own provider execution context.

---

# 80. Example — Generic Execution

```text
Runtime Control materializes WorkItem
        |
        v
Scheduler admits
        |
        v
Execution Context selected
        |
        v
Worker executes Attempt
        |
        v
Candidate Runtime Artifact
        |
        v
Completion submitted
        |
        v
Runtime Control validates execution authority
        |
        v
Runtime Artifact Store accepts ownership
        |
        v
Runtime Artifact published
```

---

# 81. Example — Late Provider Callback

```text
Provider callback arrives
        |
        v
Adapter normalizes result
        |
        v
Completion submitted
        |
        v
Runtime Control detects revoked/stale authority
        |
        v
Completion rejected
```

Callback never updates UI or Runtime state directly.

---

# 82. Example — Presentation Target Closed

```text
Accepted Presentation input
        |
        v
Commit queued to UI
        |
        v
Presentation target closes
        |
        v
UI callback executes
        |
        v
Presentation target validation fails
        |
        v
No visible replacement
```

Runtime execution authority may still have been valid.

---

# 83. Example — Cache Eviction During Read

```text
Worker holds RuntimeArtifactLease
        |
        v
Cache retention removed
        |
        v
Payload remains
        |
        v
Worker releases Lease
        |
        v
Physical disposal may become eligible
```

---

# 84. Example — Native Serial Provider

```text
Attempt admitted
        |
        v
Dedicated serial execution context
        |
        v
Synchronous native operation
        |
        v
Result normalized
        |
        v
Completion submitted
```

No Runtime Control lock is held during native execution.

---

# 85. Example — ExecutionRevision Replacement During Completion

```text
Attempt A running for ExecutionRevision 10
        |
        v
ExecutionRevision 11 becomes current
        |
        v
Attempt A physically completes
        |
        v
Completion submitted
        |
        v
Runtime Control
        |
        v
REJECT_STALE
```

Physical completion order never grants logical authority.

---

# 86. Architecture Invariants

1. ExecutionContext is logical.

2. Physical thread/process is an implementation detail.

3. Context ownership is explicit.

4. Context lifecycle is bounded.

5. UI mutates UI only.

6. Runtime Control owns execution authority.

7. Execution-orchestration state has one logical writer.

8. Runtime Control does not own every Runtime component's state.

9. Heavy work never blocks Runtime Control.

10. Control path remains protected.

11. Worker does not own shared Runtime Artifact after transfer.

12. Worker owns Attempt-local resource/lease only.

13. Lease never grants ownership.

14. Provider callback never grants execution authority.

15. Callback never mutates Runtime Control directly.

16. Runtime Artifact publication never implies Business acceptance.

17. Ownership transfer is explicit.

18. Physical completion order never determines logical acceptance.

19. Execution order never determines Business/Presentation commit order.

20. Shared Runtime Artifact payload is immutable.

21. CPU/GPU/provider/process concurrency is bounded.

22. Execution affinity is explicit.

23. Locks are not held across external/heavy execution.

24. Event subscriber does not block publisher indefinitely.

25. Subscriber changes Runtime only through command/message boundary.

26. Runtime execution relevance and Presentation target validation remain distinct.

27. Cancellation revokes authority before physical drain.

28. Delayed Retry uses non-blocking/cancelable timing.

29. Shutdown stops admission before destructive cleanup.

30. Runtime Artifact Store preserves publication/ownership/lease safety.

31. Threading correctness remains independent of Cache.

32. ExecutionScope/ExecutionRevision terminology is canonical.

33. Provider Management does not own Provider I/O/GPU execution contexts.

34. Capture/Observation contexts are CRAI-specific, not universal Runtime ownership.

35. Business commands remain outside generic Runtime threading architecture.

36. Structured child operations never escape ownership tracking.

---

# 87. Recommended MVP

CRAI MVP SHOULD support:

* logical ExecutionContext abstraction;
* one serialized Runtime Control path;
* one UI context;
* bounded CPU execution;
* async provider I/O;
* optional serial GPU/native context;
* Capture/Observation serial context;
* immutable cross-context payload references;
* Resource Lease;
* callback-to-Completion normalization;
* explicit ExecutionAffinity;
* bounded timers;
* event dispatch onto declared contexts;
* graceful context drain;
* process isolation compatibility;
* control-path protection.

MVP MAY defer:

* dedicated Scheduler context;
* GPU coordinator;
* separate Capture and Observation threads;
* general process pool;
* distributed execution contexts;
* advanced work-stealing scheduler.

---

# 88. Open Decisions

The following remain open:

* desktop UI framework and dispatcher;
* Capture API affinity;
* Capture/Observation merge;
* provider execution classes;
* default CPU pool size;
* GPU coordinator need;
* local model thread vs process;
* Event Bus ordering-key implementation;
* structured-concurrency support in selected stack;
* Presentation model build split between Worker/UI;
* context lifecycle registry location;
* dispatch-token implementation;
* process callback protocol.

---

# 89. Related Documents

Runtime:

* `PIPELINE_RUNTIME.md`
* `RUNTIME_COMPONENTS.md`
* `SCHEDULER.md`
* `WORK_QUEUE.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`
* `MEMORY_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `CACHE_POLICY.md`
* `ERROR_MODEL.md`
* `PERFORMANCE_MODEL.md`
* `RUNTIME_CONFIG.md`
* `RUNTIME_OBSERVABILITY.md`
* `BOOT_SEQUENCE.md`
* `PROCESS_TOPOLOGY.md`

External:

* `../core/EVENT_BUS.md`
* `../plugin/PLUGIN_LIFECYCLE.md`
* `../../02-modules/provider-management/`
* `../../02-modules/presentation/`

---

# 90. Completion Criteria

`THREADING_MODEL.md` is synchronized when:

* ExecutionContext remains distinct from physical thread/task/process;
* Runtime Control remains the execution-authority logical writer;
* Runtime Control ownership is not globalized;
* ExecutionScope/ExecutionRevision terminology is used;
* Worker does not own shared Runtime Artifact;
* Resource Lease semantics match Resource Lifecycle;
* provider callback only normalizes/submits Completion;
* Provider Management is removed from runtime execution-context ownership;
* ExecutionAffinity remains generic;
* Event subscriber uses declared context;
* event ordering is scoped explicitly rather than Session-specific;
* Runtime authority validation and Presentation target validation are distinct;
* Capture/Observation are identified as CRAI-specific contexts;
* cancellation/shutdown match Runtime v2;
* no stage-specific business vocabulary leaks into generic threading core.

---

# 91. Summary

CRAI Threading Model follows:

```text
Logical Execution Context
        |
        v
Explicit Owner
        |
        v
Bounded Execution Capacity
        |
        v
Immutable Input / Resource Lease
        |
        v
Physical Execution
        |
        v
Completion
        |
        v
Serialized Runtime Authority Validation
        |
        v
Owner-Specific Commit / Publication
```

The central rules are:

```text
ExecutionContext defines where work may run.

Runtime Control determines whether execution still matters.

Worker performs physical execution.

Runtime Artifact Store owns published execution payload.

Business Modules own semantic acceptance.

Presentation owns visible commit.

Physical thread/process mapping does not change those ownership boundaries.
```
