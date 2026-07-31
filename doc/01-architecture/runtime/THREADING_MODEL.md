# Runtime Threading Model

> Project: CRAI  
> Version: 0.1  
> Status: Architecture Draft

---

## 1. Purpose

This document defines how CRAI distributes runtime work across execution contexts, threads, workers, asynchronous operations, and optional isolated processes.

CRAI continuously performs:

- screen capture
- frame observation
- image processing
- OCR
- layout analysis
- translation
- provider communication
- presentation-model creation
- UI rendering

Without an explicit threading model, the runtime may:

- freeze the user interface
- create too many competing workers
- block capture while waiting for translation
- access thread-affine resources incorrectly
- mutate shared state concurrently
- retain resources across unsafe thread boundaries
- create race conditions between revisions
- reduce performance through excessive parallelism

The threading model protects responsiveness, correctness, and bounded resource usage.

---

## 2. Scope

This document covers:

- execution contexts
- UI thread boundaries
- scheduler execution
- screen-capture execution
- CPU-bound workers
- I/O-bound operations
- provider concurrency
- worker pools
- thread-affine resources
- synchronization
- event dispatch
- immutable data transfer
- blocking restrictions
- cancellation behavior
- process isolation
- MVP threading policy

This document does not define:

- exact programming-language thread APIs
- desktop-framework-specific dispatcher syntax
- provider SDK internals
- scheduler priority policy
- memory budget values
- detailed stage algorithms

Those concerns belong to implementation or related runtime documents.

---

## 3. Threading Goals

The threading model must:

- keep the UI responsive
- keep capture and observation responsive
- prevent expensive AI work from blocking control operations
- bound CPU and provider concurrency
- preserve revision correctness
- support cooperative cancellation
- minimize shared mutable state
- avoid unnecessary thread creation
- support thread-affine resources safely
- remain understandable and testable

---

## 4. Threading Philosophy

CRAI follows this core rule:

> Execution contexts coordinate through immutable artifacts, lightweight commands, and explicit state ownership.

The runtime should avoid:

```text
Multiple workers
    ↓
Mutating the same revision object
    ↓
Locking large shared structures
    ↓
Unpredictable state
```

Instead, CRAI should prefer:

```text
Worker reads immutable input
    ↓
Worker creates immutable output
    ↓
Runtime validates output
    ↓
Artifact is published atomically
```

Threads should communicate through:

- WorkItems
- immutable artifact references
- events
- completion messages
- cancellation tokens

They should not communicate by modifying shared pipeline payloads directly.

---

## 5. Execution Contexts

CRAI defines logical execution contexts rather than requiring one physical thread for every component.

```text
Application Runtime
├── UI Execution Context
├── Runtime Control Context
├── Capture Context
├── Observation Context
├── CPU Worker Pool
├── Provider I/O Context
├── Optional GPU Context
└── Optional Isolated Worker Process
```

An implementation may map multiple logical contexts onto the same underlying thread or task scheduler when safe.

---

## 6. UI Execution Context

The UI execution context owns all thread-affine user-interface operations.

It is responsible for:

- reading and updating UI controls
- applying presentation models
- showing loading and error states
- handling user commands
- updating capture-region selection UI
- creating or destroying UI-bound surfaces
- interacting with framework-specific visual objects

Only the UI context may mutate UI state.

---

## 7. UI Thread Restrictions

The UI context must not directly perform:

- OCR inference
- translation requests
- large image preprocessing
- blocking file access
- synchronous network calls
- provider initialization with significant startup cost
- long cache scans
- expensive layout analysis
- blocking cancellation cleanup

The UI context may initiate these operations by submitting commands to the runtime.

---

## 8. UI Command Flow

User actions should follow:

```text
User Action
    ↓
UI validates local input
    ↓
UI submits runtime command
    ↓
UI updates immediate interaction state
    ↓
Runtime processes command asynchronously
    ↓
Runtime publishes result or state event
    ↓
UI applies validated update
```

Examples:

- start reading
- stop translation
- change capture region
- switch provider
- retry current revision
- close session

The UI must not wait synchronously for the complete pipeline.

---

## 9. UI Commit Boundary

Presentation output crosses into the UI through an explicit commit boundary.

```text
Presentation Artifact Ready
    ↓
Validate SessionId
    ↓
Validate RevisionId
    ↓
Validate AttemptId
    ↓
Dispatch to UI Context
    ↓
Validate Again
    ↓
Apply Atomically
```

The second validation on the UI context is mandatory because the active revision may change while the update is waiting in the UI queue.

---

## 10. Runtime Control Context

The runtime control context coordinates:

- session lifecycle
- revision lifecycle
- state-machine transitions
- queue admission
- scheduler signals
- cancellation propagation
- artifact publication
- commit validation
- worker completion handling

This context provides serialized ownership of core runtime state.

---

## 11. Single-Writer Principle

Core mutable runtime state should have one logical writer.

Examples:

```text
Session state
Current revision pointer
Scheduler state
Worker assignment state
Artifact ownership metadata
```

Other contexts may read snapshots or submit commands, but should not mutate these structures directly.

This reduces the need for broad locking.

---

## 12. Runtime Command Queue

External contexts communicate with the runtime control context through a command queue or equivalent serialized mechanism.

Possible commands:

```text
StartSession
StopSession
UpdateCaptureRegion
SubmitStableFrame
WorkCompleted
WorkFailed
CancelRevision
ProviderHealthChanged
MemoryPressureChanged
CommitPresentation
```

Commands should contain lightweight metadata and artifact references.

---

## 13. Runtime Control Restrictions

The runtime control context must not perform long-running domain work.

It should not:

- call OCR synchronously
- wait synchronously for providers
- process full images
- translate text directly
- block while workers cancel
- render presentation surfaces

Its responsibilities should remain fast and deterministic.

---

## 14. Scheduler Execution Context

The Scheduler may execute within the runtime control context for the MVP.

Scheduling decisions should be short:

```text
Collect candidates
    ↓
Validate
    ↓
Choose work
    ↓
Assign worker
```

A dedicated scheduler thread is not required initially.

A separate scheduler context may be introduced only when:

- scheduling computation becomes expensive
- many sessions are active
- many worker pools must be coordinated
- profiling shows contention in the control loop

---

## 15. Capture Execution Context

Screen capture may require a dedicated execution context because capture APIs may be:

- thread-affine
- event-driven
- tied to operating-system callbacks
- backed by GPU surfaces
- sensitive to timing

Capture must not depend on OCR or translation completion.

The capture loop should remain independently responsive.

---

## 16. Capture Responsibilities

The capture context may:

- acquire a screen frame
- timestamp the frame
- create or reference a capture surface
- register a lightweight source artifact
- notify observation
- release temporary capture resources

It should not perform:

- OCR
- translation
- large synchronous encoding
- presentation rendering
- cache persistence

---

## 17. Capture Frequency

Capture should not create one permanent thread per frame.

The capture context should use:

- one long-lived capture loop
- operating-system frame callbacks
- a timer-driven asynchronous loop
- framework-supported capture events

Only one active capture operation per capture source should normally exist.

---

## 18. Capture Backpressure

When downstream processing is slow, capture should not queue every frame.

Preferred behavior:

```text
Frame 1 observed
Frame 2 replaces pending Frame 1
Frame 3 replaces pending Frame 2
```

The observation path should use latest-value behavior rather than an unbounded frame queue.

Capture remains responsive while old unprocessed observations are discarded.

---

## 19. Observation Execution Context

Observation includes:

- frame comparison
- change detection
- stability detection
- fingerprint preparation
- candidate revision creation

Observation may share an execution context with capture only when its work is lightweight and bounded.

If observation becomes CPU-intensive, it should execute through a dedicated serial worker or CPU pool.

---

## 20. Observation Serialization

Observation for one capture source should normally remain serial.

Incorrect:

```text
Frame 10 comparison running
Frame 11 comparison running
Frame 12 comparison running
```

This may produce out-of-order stability decisions.

Preferred:

```text
Observe latest frame
    ↓
Complete comparison
    ↓
Observe newest available frame
```

Intermediate frames may be replaced.

---

## 21. CPU-Bound Work

CPU-bound work includes:

- image preprocessing
- frame-difference calculation
- OCR with a local CPU model
- layout analysis
- reading-order resolution
- text normalization
- presentation-model construction

CPU-bound work must execute outside the UI and runtime control contexts.

---

## 22. CPU Worker Pool

CRAI should use a bounded CPU worker pool.

A worker pool prevents:

- one new thread per WorkItem
- excessive context switching
- CPU oversubscription
- uncontrolled temporary-memory growth

Conceptually:

```text
CPU Work Queue
    ↓
Bounded CPU Worker Pool
    ↓
CPU Work Completion
```

---

## 23. CPU Pool Size

The number of CPU workers should be conservative.

A general-purpose formula such as:

```text
CPU count = logical processor count
```

must not be used blindly.

CRAI shares CPU resources with:

- the desktop UI
- screen capture
- browser or reader application
- operating system
- local models
- background applications

The MVP should begin with low concurrency.

Suggested initial assumptions:

| Work class | Suggested concurrency |
|---|---:|
| Observation | 1 |
| Image preprocessing | 1 |
| OCR | 1 |
| Layout | 1 |
| Presentation building | 1 |

Some stages may share the same physical pool while preserving separate concurrency limits.

---

## 24. Stage Concurrency Limits

Worker-pool capacity and stage concurrency are separate concepts.

Example:

```text
CPU Pool Capacity: 3

OCR concurrency: 1
Layout concurrency: 1
Presentation concurrency: 1
```

This prevents all workers from being consumed by one expensive stage.

The Scheduler enforces stage-specific limits.

---

## 25. CPU Work Granularity

CPU tasks should be large enough to avoid scheduling overhead but small enough to support cancellation and fairness.

Too small:

```text
One WorkItem per character
```

Too large:

```text
One uninterruptible WorkItem for the whole book
```

Suitable examples:

- one stable screen revision
- one OCR region batch
- one page layout
- one bounded translation-unit batch
- one presentation model

---

## 26. I/O-Bound Work

I/O-bound work includes:

- remote OCR requests
- remote translation requests
- provider authentication refresh
- disk-cache access
- persistent storage
- telemetry export
- optional document loading

I/O-bound operations should use asynchronous APIs where available.

Waiting for network or disk must not occupy a dedicated CPU thread unnecessarily.

---

## 27. Asynchronous Does Not Mean Unlimited

Asynchronous provider calls can still consume:

- memory
- sockets
- provider quotas
- billing
- request slots
- completion callbacks

Provider concurrency must remain bounded even when no physical thread is blocked.

Incorrect:

```text
Start every queued translation request asynchronously
```

Correct:

```text
Provider concurrency limit
    ↓
Admit bounded requests
    ↓
Queue or drop remaining work
```

---

## 28. Provider I/O Context

Provider requests may use the platform asynchronous runtime.

A dedicated network thread is usually unnecessary.

Conceptually:

```text
Translation WorkItem
    ↓
Provider Adapter
    ↓
Asynchronous Request
    ↓
Completion Callback
    ↓
Runtime Command Queue
```

Provider callbacks must not mutate runtime state directly.

They submit completion messages to the runtime control context.

---

## 29. Provider Adapter Responsibilities

Provider adapters are responsible for:

- asynchronous request creation
- timeout setup
- cancellation integration
- response normalization
- provider error classification
- request-local resource cleanup
- concurrency accounting

They must not:

- commit UI output
- advance the pipeline directly
- mutate the active revision
- bypass Scheduler admission

---

## 30. Provider Completion Ordering

Provider responses may complete out of order.

Example:

```text
Revision 20 request starts
Revision 21 request starts later
Revision 21 completes first
Revision 20 completes later
```

The runtime must not rely on completion order.

Every result must include:

```text
SessionId
RevisionId
Stage
AttemptId
WorkItemId
```

Commit validation determines whether the result remains valid.

---

## 31. Local Provider Execution

Local OCR or translation providers may be:

- synchronous CPU libraries
- asynchronous libraries
- GPU-backed libraries
- thread-affine native libraries
- process-hosted models

Each provider adapter must declare its execution requirements.

Conceptually:

```text
ProviderCapabilities
├── ExecutionClass
├── ThreadAffinity
├── MaximumConcurrency
├── CancellationSupport
├── ProcessIsolation
└── MemoryCost
```

---

## 32. Thread-Affine Providers

Some providers or resources may require all operations on the same thread.

Examples may include:

- native graphics contexts
- capture APIs
- particular GPU contexts
- embedded browser objects
- some OCR library handles

Such a provider should use a dedicated serial execution context.

```text
Provider Command Queue
    ↓
Dedicated Provider Thread
    ↓
Provider Operation
```

Thread affinity must be declared explicitly rather than discovered through runtime failures.

---

## 33. GPU Execution Context

GPU work may require:

- a dedicated command stream
- serialized model inference
- thread-affine resource creation
- explicit synchronization
- bounded GPU concurrency

The architecture does not assume that GPU work is automatically parallel.

Running multiple GPU tasks concurrently may increase:

- memory pressure
- context contention
- latency
- transfer overhead

The MVP may serialize each local GPU provider.

---

## 34. GPU and UI Interaction

UI rendering and AI inference may share the same physical GPU.

The runtime should protect UI responsiveness by:

- limiting GPU inference concurrency
- avoiding unnecessary high-resolution processing
- releasing obsolete tensors
- avoiding speculative GPU work
- monitoring inference latency
- reducing load under rendering pressure where possible

The UI must not wait synchronously for GPU AI work.

---

## 35. Worker Ownership

A worker owns only:

- its execution-local temporary resources
- its acquired artifact leases
- its provider-request handle
- its completion result until publication

A worker does not own:

- session lifecycle
- current revision state
- global cache policy
- downstream scheduling
- UI state

This boundary prevents worker code from becoming a second runtime coordinator.

---

## 36. Work Execution Contract

A worker execution should conceptually follow:

```text
Receive WorkItem
    ↓
Validate cancellation
    ↓
Acquire input artifact leases
    ↓
Validate inputs
    ↓
Execute bounded operation
    ↓
Check cancellation
    ↓
Create immutable output
    ↓
Submit completion command
    ↓
Release temporary resources and leases
```

The runtime control context decides whether the output is published or rejected.

---

## 37. Worker Completion

Workers must not directly enqueue downstream work.

Preferred behavior:

```text
Worker completes OCR
    ↓
Submits WorkCompleted
    ↓
Runtime validates result
    ↓
Artifact published
    ↓
Scheduler admits Layout WorkItem
```

This preserves centralized pipeline authority.

---

## 38. Shared Mutable State

Shared mutable state should be minimized.

Forbidden by default:

- multiple workers appending to one shared OCR result
- translation workers modifying one presentation model
- provider callbacks updating session state directly
- UI and runtime mutating one shared collection
- workers changing queue priority themselves

Where shared mutation is unavoidable, ownership and synchronization must be explicit.

---

## 39. Immutable Data Across Boundaries

Data crossing execution contexts should be immutable or treated as immutable.

Examples:

```text
RevisionSnapshot
ArtifactReference
ProviderResult
PresentationModel
RuntimeEvent
WorkItem
```

Mutable builder objects should remain local to one worker until finalized.

---

## 40. Synchronization Strategy

CRAI should prefer synchronization in this order:

1. single ownership
2. serialized command processing
3. immutable values
4. bounded concurrent collections
5. narrow locks
6. broader locking only when unavoidable

The runtime should not begin with a complex graph of locks.

---

## 41. Lock Scope

When locks are required, they must:

- protect one clearly defined resource
- remain held for short periods
- never surround provider calls
- never surround long image processing
- never surround UI dispatch waits
- avoid nested acquisition where possible

Incorrect:

```text
Lock Session
    ↓
Call translation provider
    ↓
Wait several seconds
```

Correct:

```text
Lock briefly
    ↓
Read required metadata
    ↓
Release lock
    ↓
Call provider
```

---

## 42. Lock Ordering

If multiple locks become necessary, a global acquisition order must be documented.

Possible order:

```text
Application
    ↓
Session
    ↓
Revision
    ↓
Artifact
    ↓
Worker
```

However, the preferred architecture should avoid acquiring multiple locks simultaneously.

---

## 43. Blocking Policy

Blocking waits are forbidden in:

- UI context
- runtime control context
- capture callback
- provider completion callback
- event-dispatch loop

Examples of forbidden behavior:

- synchronous waiting for an async task
- sleeping to wait for cancellation
- blocking on network completion
- waiting for UI dispatcher while holding a lock
- waiting for worker shutdown inside an event callback

---

## 44. Allowed Blocking

Blocking may be acceptable inside a dedicated worker when:

- the provider API is inherently synchronous
- the worker pool accounts for the blocked capacity
- cancellation or timeout is bounded
- the UI and control contexts remain unaffected
- concurrency remains limited

Example:

```text
Dedicated OCR Worker
    ↓
Synchronous native OCR call
```

This is safer than performing the call on the UI thread, but asynchronous or process-isolated execution may still be preferable.

---

## 45. Sync-Over-Async Prohibition

Implementations should avoid patterns equivalent to:

```text
AsyncOperation().Wait()
AsyncOperation().Result
```

Such patterns can cause:

- deadlocks
- thread-pool starvation
- UI freezes
- lost cancellation responsiveness

Asynchronous flows should remain asynchronous through their call chain where practical.

---

## 46. Event Dispatch Model

Runtime events should be published through an event bus or serialized event dispatcher.

The event publisher should not execute arbitrary subscriber logic synchronously on the publisher's thread.

Incorrect:

```text
Worker publishes event
    ↓
UI subscriber executes immediately on worker thread
```

Preferred:

```text
Worker submits completion
    ↓
Runtime publishes event
    ↓
Subscriber dispatcher routes event
    ↓
Subscriber handles in its own execution context
```

---

## 47. Event Subscriber Isolation

One slow event subscriber must not block:

- worker completion
- scheduler progress
- cancellation propagation
- UI control commands

Subscriber execution may use:

- UI dispatcher
- runtime command queue
- diagnostics queue
- bounded background handler

The required context should be declared per subscriber.

---

## 48. Event Ordering

Event ordering is only guaranteed where explicitly required.

Within one serialized session control stream, CRAI should preserve causal order.

Example:

```text
revision.created
    ↓
work.started
    ↓
work.completed
    ↓
presentation.committed
```

Across unrelated sessions or providers, global ordering is not required.

Consumers must use identifiers and timestamps rather than assuming total event order.

---

## 49. Event Reentrancy

Event handlers must not reenter mutable runtime operations unpredictably.

Example risk:

```text
Runtime changes state
    ↓
Publishes event synchronously
    ↓
Subscriber calls runtime again
    ↓
State transition occurs inside original transition
```

Preferred behavior:

```text
State transition completes
    ↓
Event queued
    ↓
Subscriber command processed later
```

This preserves state-machine consistency.

---

## 50. Cancellation and Threads

Cancellation is cooperative across all execution contexts.

A cancellation request should:

- update cancellation state quickly
- invalidate queued work
- signal running workers
- abort asynchronous requests where supported
- avoid blocking the requesting context
- defer cleanup completion when necessary

The thread issuing cancellation must not wait indefinitely for physical termination.

---

## 51. Cancellation Checkpoints

CPU workers should check cancellation:

- before acquiring expensive resources
- before starting heavy processing
- between batches or regions
- after provider completion
- before publishing output

I/O operations should connect cancellation to:

- request abort
- timeout
- response-read cancellation
- provider adapter state

---

## 52. Worker Shutdown

Worker-pool shutdown should follow:

```text
Stop accepting new work
    ↓
Cancel queued work
    ↓
Request active work cancellation
    ↓
Wait for bounded grace period
    ↓
Mark remaining work abandoned
    ↓
Release worker resources
```

The UI should remain responsive during shutdown.

---

## 53. Application Shutdown

Application shutdown should coordinate:

```text
UI requests shutdown
    ↓
Runtime stops new sessions
    ↓
Capture stops
    ↓
Session cancellation propagates
    ↓
Provider requests canceled
    ↓
Workers drain within timeout
    ↓
Native resources disposed
    ↓
Application exits
```

The shutdown sequence belongs in more detail in `RESOURCE_LIFECYCLE.md`.

---

## 54. Process Isolation

Some work may eventually run in isolated child processes.

Suitable candidates:

- unstable native OCR libraries
- large local AI models
- providers requiring hard termination
- plugins with uncertain reliability
- high-memory import pipelines

Process isolation can provide:

- crash isolation
- hard cancellation through process termination
- separate memory accounting
- provider restart
- reduced native-library contamination

---

## 55. Process Isolation Costs

Process isolation also introduces:

- inter-process communication
- serialization cost
- image-transfer cost
- startup latency
- deployment complexity
- process supervision
- more difficult debugging

It should not be introduced without a clear requirement.

---

## 56. Cross-Process Payload Policy

Large artifacts should not be serialized repeatedly across processes.

Possible strategies:

- shared memory
- memory-mapped files
- temporary files
- operating-system shared surfaces
- compressed transfer
- artifact handles

The selected strategy depends on the implementation platform.

For the MVP, all primary runtime work may remain in one process.

---

## 57. Timer Execution

Timers may be used for:

- capture pacing
- stability detection
- provider timeouts
- idle model unloading
- retry delay
- metrics sampling

Timer callbacks must remain short.

They should submit commands rather than perform full pipeline work directly.

---

## 58. Delay and Retry

Retry delay must not occupy a blocked thread.

Preferred:

```text
Register asynchronous delay
    ↓
Delay completes
    ↓
Submit retry-ready command
```

Delayed retries must remain cancelable.

---

## 59. Thread-Pool Starvation

Thread-pool starvation can occur when:

- synchronous provider calls occupy every worker
- tasks block waiting for tasks from the same pool
- too many CPU jobs are submitted
- callbacks require unavailable worker threads
- retries start without bounds

CRAI should avoid using one unrestricted global pool for every workload.

---

## 60. Workload Separation

Logical workload separation should exist even if the platform uses one physical pool underneath.

Suggested classes:

```text
CONTROL
CAPTURE
OBSERVATION
CPU_HEAVY
CPU_LIGHT
NETWORK_IO
GPU
UI
MAINTENANCE
```

The Scheduler uses these classes for admission and concurrency policy.

---

## 61. CPU-Heavy and CPU-Light Work

CPU-heavy work:

- local OCR inference
- image transformations
- local AI translation
- complex page segmentation

CPU-light work:

- validation
- artifact metadata creation
- cache-key calculation
- state transitions
- event-envelope creation

CPU-light control work should not wait behind a queue full of CPU-heavy tasks.

---

## 62. Control-Path Protection

CRAI must reserve enough execution capacity for:

- cancellation
- session stop
- provider timeout handling
- revision replacement
- UI-state updates
- worker completion processing

The control path must remain responsive even when domain workers are saturated.

This may be achieved through:

- a dedicated runtime control context
- reserved worker capacity
- non-blocking asynchronous commands

---

## 63. Priority and Threading

Thread priority at the operating-system level should not be the primary scheduling mechanism.

CRAI should express priority through:

- queue selection
- admission control
- stage concurrency
- worker assignment
- obsolete-work cancellation

Operating-system thread priority may be platform-dependent and difficult to reason about.

---

## 64. Background Work

Background work includes:

- cache cleanup
- diagnostics export
- history indexing
- model preloading
- persistent-cache writes

Background work must:

- use low concurrency
- yield to interactive work
- stop under memory or CPU pressure
- remain cancelable
- avoid UI-bound resources

The MVP should minimize background work.

---

## 65. Thread Safety of Artifact Store

The Artifact Store may receive concurrent:

- reads
- lease acquisition
- lease release
- publication requests
- eviction requests

Its internal synchronization must preserve:

- immutable payload access
- atomic publication
- correct ownership accounting
- safe logical disposal
- safe physical disposal

Artifact payloads themselves should remain read-only after publication.

---

## 66. Thread Safety of Revision Store

The Revision Store should have one logical writer through the runtime control context.

Workers may request:

```text
GetArtifactLease
```

but should not directly mutate revision relationships.

This reduces concurrent mutation of revision metadata.

---

## 67. Atomic Publication

Artifact publication must appear atomic to consumers.

Consumers should observe either:

```text
Artifact not available
```

or:

```text
Complete valid artifact available
```

They must not observe partially constructed artifacts.

Workers build outputs locally, then submit them for publication.

---

## 68. Atomic Presentation Replacement

Presentation replacement should be atomic from the UI perspective.

Incorrect:

```text
Clear old text
    ↓
Add translated lines one by one
    ↓
Failure halfway
```

Preferred:

```text
Build complete presentation model
    ↓
Validate
    ↓
Replace current model in one UI transaction
```

Progressive rendering, if added later, requires its own explicit consistency model.

---

## 69. Data Race Prevention

Potential data races include:

- revision becoming obsolete while worker completes
- cache eviction while worker reads
- UI closure while update is queued
- provider switch while request completes
- session close while capture callback runs
- artifact disposal while native operation uses it

These are handled through:

- immutable artifacts
- leases
- serialized runtime state
- cancellation tokens
- commit validation
- bounded resource lifecycle

---

## 70. Deadlock Prevention

CRAI should prevent deadlocks by following these rules:

1. Never block the UI waiting for runtime work.
2. Never block runtime control waiting for UI completion.
3. Never hold locks across provider calls.
4. Never wait for worker completion while holding Artifact Store locks.
5. Avoid nested locks.
6. Use timeouts for external operations.
7. Use asynchronous shutdown coordination.
8. Process event commands after current state transitions complete.

---

## 71. Error Handling Across Contexts

Exceptions or failures must not escape arbitrarily across thread boundaries.

Workers should convert failures into structured completion outcomes:

```text
WorkSucceeded
WorkFailed
WorkCanceled
WorkAbandoned
```

The outcome should include:

- WorkItemId
- SessionId
- RevisionId
- AttemptId
- stage
- error classification
- timing information

The runtime then decides the next action.

---

## 72. Unhandled Worker Failure

An unhandled worker exception must:

- release worker-local resources
- release artifact leases
- mark the worker operation failed
- notify runtime control
- avoid terminating the entire application where possible
- trigger provider or worker health checks if needed

Failures in native code may require process isolation in later versions.

---

## 73. Diagnostics and Thread Context

Diagnostics should record execution context information such as:

- logical context
- worker identifier
- stage
- session
- revision
- attempt
- queue wait time
- execution time
- cancellation state

Physical thread IDs may be useful for debugging but should not become domain identifiers.

---

## 74. Testing the Threading Model

Tests should cover:

- UI remains responsive during slow OCR
- capture continues while translation waits
- control commands execute while workers are saturated
- provider responses complete out of order
- revision changes during UI dispatch
- session closes during capture callback
- cache eviction occurs during artifact read
- cancellation occurs during CPU work
- cancellation occurs during provider request
- worker fails before publishing output
- duplicate completions are rejected
- event handler submits a new runtime command
- shutdown occurs during active processing
- stage concurrency limits are respected
- no unbounded thread creation occurs

---

## 75. Deterministic Testing

Threading tests should avoid relying only on real timing.

Use controllable test components:

```text
FakeScheduler
FakeWorker
FakeProvider
ManualCompletionGate
ManualCancellationGate
DeterministicEventQueue
```

Tests should explicitly control:

- when work starts
- when work completes
- when cancellation occurs
- when UI dispatch executes
- which result arrives first

This makes race-condition tests repeatable.

---

## 76. Performance Observation

Threading metrics should include:

- UI dispatch delay
- runtime command-queue length
- capture callback delay
- observation latency
- worker utilization
- CPU pool saturation
- provider in-flight requests
- task queue wait time
- cancellation acknowledgment time
- event-dispatch delay
- blocked-worker count
- active thread count where measurable
- process-isolated worker restart count

---

## 77. MVP Threading Model

The initial implementation should remain deliberately simple.

### 77.1 Required Execution Contexts

```text
1 UI Context
1 Runtime Control Context
1 Capture/Observation Context
1 Bounded CPU Worker Pool
Asynchronous Provider I/O
```

Capture and observation may initially share one serial context if profiling shows the observation work is lightweight.

### 77.2 Required Concurrency

Suggested MVP limits:

| Stage | Concurrency |
|---|---:|
| Capture source | 1 |
| Observation | 1 |
| OCR | 1 |
| Layout | 1 |
| Translation provider | 1 |
| Presentation build | 1 |
| UI commit | 1 |

Remote translation concurrency may later increase to `2` if translation-unit batching requires it.

### 77.3 Required Rules

1. UI work runs only on the UI context.
2. No domain processing runs on the UI context.
3. Core runtime state has one logical writer.
4. Workers receive immutable inputs.
5. Workers return immutable outputs.
6. Workers do not schedule downstream work directly.
7. Provider calls use asynchronous APIs where possible.
8. Every stage has bounded concurrency.
9. Capture does not queue every frame.
10. Every UI commit validates revision identity.
11. Cancellation never blocks the caller indefinitely.
12. No unbounded thread creation is allowed.

---

## 78. Suggested MVP Execution Flow

```text
UI Thread
    ↓ StartSession command

Runtime Control
    ↓ Starts capture and session state

Capture/Observation Context
    ↓ Stable frame detected
    ↓ SubmitStableFrame command

Runtime Control
    ↓ Creates revision
    ↓ Scheduler admits OCR

CPU Worker
    ↓ Runs OCR
    ↓ Submits WorkCompleted

Runtime Control
    ↓ Publishes OCR artifact
    ↓ Scheduler admits Layout

CPU Worker
    ↓ Runs Layout
    ↓ Submits WorkCompleted

Runtime Control
    ↓ Publishes Layout artifact
    ↓ Scheduler admits Translation

Provider Async I/O
    ↓ Translation response
    ↓ Submits WorkCompleted

Runtime Control
    ↓ Publishes Translation artifact
    ↓ Scheduler admits Presentation

CPU Worker
    ↓ Builds Presentation artifact
    ↓ Submits WorkCompleted

Runtime Control
    ↓ Validates commit
    ↓ Dispatches UI update

UI Thread
    ↓ Validates revision again
    ↓ Replaces presentation atomically
```

---

## 79. Example: User Scrolls During Translation

```text
Translation request for Revision 20 active
    ↓
Capture context detects new stable frame
    ↓
Runtime creates Revision 21
    ↓
Revision 20 cancellation requested
    ↓
Provider abort requested asynchronously
    ↓
Scheduler admits Revision 21 work
```

If Revision 20 completes late:

```text
Provider completion callback
    ↓
Submit WorkCompleted
    ↓
Runtime rejects stale result
```

No provider callback updates the UI directly.

---

## 80. Example: Slow OCR

```text
OCR worker busy
    ↓
Capture continues
    ↓
Observation replaces pending frames
    ↓
Newest stable revision becomes current
    ↓
Old OCR work canceled when useful
```

The UI and capture path remain responsive because OCR runs outside both contexts.

---

## 81. Example: UI Closed Before Commit

```text
Presentation update queued to UI
    ↓
User closes session
    ↓
Session becomes inactive
    ↓
UI callback executes
    ↓
Commit validation fails
    ↓
Presentation discarded
```

The UI callback must not assume that a queued update remains valid.

---

## 82. Example: Cache Eviction During OCR Read

```text
OCR worker acquires SourceImageArtifact lease
    ↓
Cache evicts source artifact
    ↓
Cache retention released
    ↓
Worker lease remains valid
    ↓
OCR finishes
    ↓
Lease released
    ↓
Artifact physically disposed
```

No global lock is held during OCR processing.

---

## 83. Example: Provider Callback

Incorrect:

```text
Provider response
    ↓
Callback updates Session.CurrentRevision
```

Correct:

```text
Provider response
    ↓
Provider adapter normalizes result
    ↓
Submit WorkCompleted command
    ↓
Runtime validates and publishes
```

---

## 84. Future Threading Evolution

Potential future improvements include:

- separate capture and observation contexts
- dedicated local-model processes
- independent OCR and translation worker pools
- multi-session weighted scheduling
- GPU execution coordinator
- shared-memory artifact transport
- adaptive concurrency based on CPU and memory pressure
- parallel translation-unit processing
- structured concurrency framework

These should be introduced only when product needs and profiling justify them.

---

## 85. Structured Concurrency Direction

Where the implementation platform supports it, child work should remain tied to an owning scope.

Example:

```text
Session Task Scope
    └── Revision Task Scope
        ├── OCR Task
        ├── Layout Task
        └── Translation Task
```

When the parent scope ends, child work receives cancellation and cannot outlive its logical owner without being tracked as abandoned work.

The architecture does not require a specific structured-concurrency library, but it adopts the ownership principle.

---

## 86. Architecture Invariants

The threading model must preserve these invariants:

1. Only the UI context mutates UI state.
2. The UI context never performs expensive domain work.
3. Core mutable runtime state has one logical writer.
4. Capture remains independent from OCR and translation completion.
5. Observation for one capture source is ordered.
6. CPU and provider concurrency are bounded.
7. Published artifacts are immutable.
8. Workers cannot mutate session or revision state directly.
9. Workers cannot commit UI output directly.
10. Provider callbacks submit runtime messages instead of mutating state.
11. Every UI commit revalidates session and revision ownership.
12. Locks are never held across long-running operations.
13. Event subscribers cannot block the publisher indefinitely.
14. Cancellation and control commands retain execution capacity.
15. No stage creates an unbounded number of threads or asynchronous requests.
16. Physical completion order never determines logical authority.
17. Native thread-affine resources execute only in their declared context.
18. Threading correctness does not depend on cache availability.

---

## 87. Open Questions

The following questions remain open:

- Which desktop framework will provide the UI dispatcher?
- Which screen-capture API will be used?
- Is the capture API thread-affine?
- Can capture and observation safely share one serial context?
- Which OCR provider will be selected for the MVP?
- Is the OCR API synchronous, asynchronous, CPU-based, or GPU-based?
- Does the selected OCR provider support cancellation?
- Will translation use only remote providers initially?
- Should local model providers use dedicated threads or processes?
- What CPU worker-pool size is appropriate for minimum-spec devices?
- Should presentation construction occur on a worker or partially on the UI context?
- Which event bus implementation preserves required per-session ordering?
- Will the selected platform support structured concurrency?
- Which native resources require explicit thread affinity?

These questions do not block the initial architecture.

---

## 88. Related Documents

- `README.md`
- `PIPELINE_RUNTIME.md`
- `WORK_QUEUE.md`
- `SCHEDULER.md`
- `CANCELLATION.md`
- `CACHE_POLICY.md`
- `MEMORY_MODEL.md`
- `RESOURCE_LIFECYCLE.md`
- `PERFORMANCE_MODEL.md`
- `../STATE_MACHINE.md`
- `../EVENT_BUS.md`
- `../DATA_FLOW.md`
- `../flows/SCREEN_COMIC_FLOW.md`

---

## 89. Next Step

The next runtime document should be:

```text
RESOURCE_LIFECYCLE.md
```

It should define:

- resource ownership transfer
- resource creation and registration
- artifact leases
- temporary-resource cleanup
- revision disposal
- session shutdown
- provider loading and unloading
- native-resource disposal
- draining canceled work
- shutdown order
- cleanup failure handling
- lifecycle observability

---

## 90. Summary

CRAI uses a small number of explicit execution contexts rather than creating one thread for every component or WorkItem.

The practical model is:

```text
UI Context
    +
Serialized Runtime Control
    +
Responsive Capture and Observation
    +
Bounded CPU Workers
    +
Bounded Asynchronous Provider I/O
    +
Immutable Artifact Exchange
```

The MVP should favor serialized ownership and low concurrency over aggressive parallelism.

Correctness and UI responsiveness are more important than maximizing worker utilization.

Parallel execution should only be increased after profiling confirms that it improves end-to-end reading latency without causing memory pressure, UI contention, or provider overload.