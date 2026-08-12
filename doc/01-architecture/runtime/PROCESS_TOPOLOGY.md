# Process Topology

* **Document:** Runtime Architecture / Process Topology
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines the process topology of CRAI Runtime.

It specifies:

* which architectural components MAY execute in the same OS process;
* which components MAY or SHOULD be isolated;
* process ownership and crash boundaries;
* communication rules across process boundaries;
* Runtime identity propagation across IPC;
* authority behavior across process boundaries;
* resource ownership rules;
* failure and restart behavior;
* shutdown ordering;
* MVP deployment topology;
* future isolation paths.

The goal is to allow CRAI to evolve from a simple local Runtime into a more isolated architecture without changing core Runtime semantics.

---

# 2. Core Principle

Process topology is an implementation/deployment boundary.

It MUST NOT redefine Runtime semantics.

The following remain canonical regardless of whether execution occurs:

```text
in-process
or
cross-process
```

Runtime identity remains:

```text
ApplicationInstance
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
```

Process boundaries MUST preserve these identities.

---

# 3. Process Boundary Is Not Ownership Boundary

An OS process does not automatically own every resource allocated inside it at the architectural level.

CRAI distinguishes:

```text
Process Ownership
Runtime Resource Ownership
Business Ownership
Retention
Lease
Visibility
Execution Authority
```

These concepts MUST NOT be conflated.

Example:

```text
Provider Worker Process
        |
        | physically hosts
        v
Provider Runtime
        |
        | produces
        v
Candidate Resource
        |
        | ownership transferred
        v
Runtime Artifact Store
```

The worker process may physically allocate the resource while Runtime lifecycle ownership belongs elsewhere after transfer.

---

# 4. Process Boundary Is Not Module Boundary

A CRAI module does not require a dedicated process.

Multiple modules MAY execute in one process.

One module MAY also use one or more isolated worker processes.

Therefore:

```text
Module != Process
Process != Module
```

Module boundaries are architectural responsibility boundaries.

Process boundaries are execution/isolation boundaries.

---

# 5. Process Boundary Is Not Thread Boundary

Process topology and threading model solve different problems.

`THREADING_MODEL.md` defines:

* execution contexts;
* thread affinity;
* synchronization;
* UI thread rules;
* blocking behavior;
* concurrency constraints.

`PROCESS_TOPOLOGY.md` defines:

* process placement;
* crash isolation;
* IPC;
* restart;
* resource boundary;
* process lifecycle.

A process may contain multiple execution contexts.

---

# 6. Topology Goals

CRAI process topology SHOULD optimize for:

1. simplicity;
2. predictable latency;
3. low memory overhead;
4. crash containment;
5. provider isolation;
6. native library isolation;
7. GPU/resource isolation;
8. plugin safety;
9. deterministic shutdown;
10. future scalability.

These goals may conflict.

MVP prioritizes simplicity unless isolation has a clear correctness, stability or security benefit.

---

# 7. Topology Non-Goals

This document does NOT define:

* distributed deployment across multiple machines;
* cloud microservice architecture;
* remote multi-user execution;
* Provider routing policy;
* Retry policy;
* Plugin API contracts;
* business pipeline semantics;
* Artifact schema;
* UI rendering architecture.

CRAI is primarily a local application Runtime.

Cross-process architecture does not imply distributed-system deployment.

---

# 8. Conceptual Process Roles

CRAI MAY contain the following conceptual process roles:

```text
Application Host
Runtime Host
Capture Worker
AI / Provider Worker
Plugin Worker
Model Worker
Utility Worker
```

Not every role requires a separate process.

---

# 9. Application Host

The Application Host owns application-level lifecycle.

Typical responsibilities:

```text
Application startup
UI bootstrap
Runtime bootstrap
Configuration loading
Process supervision
Application shutdown
```

For desktop deployment, Application Host MAY also host Runtime Control.

---

# 10. Runtime Host

Runtime Host contains the canonical Runtime coordination plane.

Typical components:

```text
Runtime Control
Scheduler
Work Queues
Execution Authority
Runtime Artifact Coordination
Resource Manager
Cache Coordination
Observability Coordination
```

Runtime Host SHOULD remain authoritative for logical Runtime state.

Worker processes MUST NOT independently redefine Runtime state.

---

# 11. Capture Worker

Capture MAY initially run in-process.

Isolation MAY later be useful for:

* native screen-capture APIs;
* platform-specific failures;
* GPU-backed capture;
* permission boundaries;
* driver instability;
* native memory isolation.

Conceptually:

```text
Runtime Host
    |
    | Capture Request
    v
Capture Worker
    |
    | Candidate Capture Result
    v
Runtime Host
```

---

# 12. AI / Provider Worker

AI/provider execution is a strong candidate for optional process isolation.

Reasons include:

* native SDK instability;
* large model memory;
* GPU ownership;
* provider-specific dependencies;
* blocking APIs;
* Python/native runtime integration;
* crash containment;
* model unload/restart.

Conceptually:

```text
Runtime Host
    |
    | Attempt Dispatch
    v
AI / Provider Worker
    |
    | Physical Execution
    v
Completion
    |
    v
Runtime Host
```

The worker performs physical execution.

Runtime Host remains responsible for accepting or rejecting the Completion.

---

# 13. Plugin Worker

Plugins MAY run:

```text
in-process
or
isolated process
```

depending on trust, capability and resource requirements.

Isolation is preferred when a Plugin:

* loads native code;
* executes untrusted third-party code;
* performs expensive blocking work;
* requires dependency isolation;
* has elevated crash risk;
* uses substantial native/GPU resources.

Plugin process isolation MUST NOT grant additional architectural authority.

---

# 14. Model Worker

Local ML models MAY eventually use dedicated worker processes.

Possible examples:

```text
OCR Model Worker
Translation Model Worker
Vision Model Worker
Embedding Worker
```

Benefits MAY include:

* model memory isolation;
* independent restart;
* GPU resource control;
* model lifecycle management;
* dependency isolation.

MVP does not require this topology.

---

# 15. Utility Worker

Low-risk background work MAY use shared utility workers.

Examples:

* compression;
* serialization;
* thumbnail generation;
* lightweight preprocessing.

Utility workers SHOULD NOT become a generic dumping ground for architectural responsibilities.

---

# 16. MVP Recommended Topology

The default MVP topology SHOULD remain simple.

Recommended:

```text
┌───────────────────────────────────────────────┐
│                CRAI Main Process              │
│                                               │
│  UI / Presentation                            │
│  Application Host                             │
│  Runtime Control                              │
│  Scheduler                                    │
│  Work Queues                                  │
│  Runtime Artifact Store                       │
│  Resource Manager                             │
│  Cache                                        │
│  Observability                                │
│  Core Business Modules                        │
│                                               │
│  Capture Adapter                              │
│  Provider Adapters                            │
│  Built-in Plugins                             │
└───────────────────────────────────────────────┘
```

This minimizes:

* IPC complexity;
* serialization overhead;
* lifecycle complexity;
* debugging difficulty;
* memory duplication.

---

# 17. MVP Isolation Exception

A component MAY be isolated during MVP if required by:

```text
native dependency
GPU runtime
unstable external SDK
security boundary
incompatible dependency runtime
high crash risk
```

Isolation is therefore capability-driven rather than mandatory.

---

# 18. Evolution Topology

A future topology MAY become:

```text
┌───────────────────────────────┐
│       CRAI Main Process       │
│                               │
│ UI / Presentation             │
│ Application Host              │
│ Runtime Control               │
│ Scheduler                     │
│ Artifact Store                │
│ Resource Manager              │
│ Cache                         │
│ Observability                 │
└───────────────┬───────────────┘
                │
                │ IPC
                │
        ┌───────┼─────────┐
        │       │         │
        v       v         v
   Capture   Provider   Plugin
    Worker    Worker     Worker
        │       │         │
        └───────┼─────────┘
                │
                v
         Completion / Events
```

Core Runtime semantics remain unchanged.

---

# 19. Isolation Decision Model

A component SHOULD be considered for isolation when one or more of these are significant:

```text
Crash Risk
Security Risk
Native Dependency Risk
GPU Ownership
Memory Footprint
Dependency Conflict
Blocking Behavior
Trust Level
Restart Requirement
Resource Cleanup Risk
```

Isolation MUST NOT be introduced merely because a component is architecturally separate.

---

# 20. Isolation Cost

Every process boundary introduces cost.

Typical costs:

* IPC latency;
* serialization;
* copying;
* additional memory;
* startup time;
* supervision;
* restart complexity;
* diagnostic complexity;
* resource transfer complexity;
* shutdown coordination.

Therefore:

```text
Isolation Benefit > Isolation Cost
```

SHOULD be demonstrable.

---

# 21. Runtime Authority Across Processes

Worker processes do NOT own canonical Runtime execution authority.

Canonical authority remains in Runtime Host.

A worker MAY know:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
CancellationTokenReference
```

but this does not grant publication authority.

---

# 22. Completion Across Process Boundary

Cross-process execution follows:

```text
Runtime Host
    |
    | Dispatch Attempt
    v
Worker Process
    |
    | Execute
    v
Completion Message
    |
    v
Runtime Host
    |
    | Validate Execution Authority
    v
Accept / Reject
```

The worker MUST NOT assume that successful physical execution means the result is still relevant.

---

# 23. Late Completion

A worker may complete after:

* cancellation;
* ExecutionRevision replacement;
* ExecutionScope close;
* Runtime shutdown initiation.

The Runtime Host MUST validate authority after receiving Completion.

Example:

```text
Attempt executes in Worker
        |
        v
ExecutionRevision superseded
        |
        v
Worker completes
        |
        v
Completion delivered
        |
        v
Runtime Host
        |
        v
REJECT_STALE
```

No worker-side assumption overrides this decision.

---

# 24. Cancellation Across Processes

Cancellation becomes cooperative across IPC.

Recommended flow:

```text
Runtime Host
    |
    | Revoke Authority
    |
    | Remove queued work
    |
    | Send Cancellation Signal
    v
Worker Process
    |
    | Attempt physical abort
    v
Acknowledgement
```

Runtime correctness MUST NOT depend on physical cancellation succeeding.

---

# 25. Cancellation Authority Rule

The canonical sequence remains:

```text
Logical Cancellation
        |
        v
Authority Revoked
        |
        v
Physical Cancellation Attempt
```

not:

```text
Physical Cancellation
        |
        v
Authority Revoked
```

This preserves correctness when the worker cannot abort immediately.

---

# 26. Worker Unresponsiveness

If a worker does not acknowledge cancellation:

```text
Attempt
    |
    v
ABANDONED
```

MAY be recorded logically.

The worker process MAY later be:

* allowed to drain;
* restarted;
* terminated;

depending on process policy.

---

# 27. IPC Principles

IPC SHOULD be:

* explicit;
* versioned;
* bounded;
* typed where practical;
* asynchronous where practical;
* cancellable where meaningful;
* content-aware only where required;
* observable;
* failure-tolerant.

IPC MUST NOT become an implicit shared-state mechanism.

---

# 28. IPC Message Classes

Conceptual IPC classes MAY include:

```text
CONTROL
COMMAND
COMPLETION
EVENT
RESOURCE_REFERENCE
HEALTH
DIAGNOSTIC
```

---

# 29. Control Messages

Examples:

```text
INITIALIZE
READY
STOP_ACCEPTING_WORK
CANCEL_ATTEMPT
DRAIN
SHUTDOWN
PING
```

Control messages SHOULD remain small and high priority.

---

# 30. Command Messages

Command messages initiate physical work.

Conceptually:

```text
ExecuteAttemptCommand
├── ExecutionScopeId
├── ExecutionRevisionId
├── WorkItemId
├── AttemptId
├── WorkType
├── ExecutionBinding
├── InputReference
├── ConfigurationSnapshot
└── Deadline?
```

Large input SHOULD use resource references where practical.

---

# 31. Completion Messages

Conceptually:

```text
AttemptCompletion
├── ExecutionScopeId
├── ExecutionRevisionId
├── WorkItemId
├── AttemptId
├── PhysicalOutcome
├── CandidateResourceReference?
├── NormalizedError?
├── Timing
└── DiagnosticMetadata?
```

Completion does not imply acceptance.

---

# 32. IPC Events

Worker-originated events MAY report:

* lifecycle;
* resource pressure;
* degradation;
* diagnostic state;
* physical operation status.

Events MUST NOT mutate canonical Runtime state without Runtime Host processing.

---

# 33. Resource References

Large resources SHOULD avoid unnecessary IPC copies.

Possible mechanisms:

```text
shared memory
memory-mapped file
temporary bounded storage
GPU/native shared handle
Runtime-managed resource reference
```

Exact mechanism is platform-specific.

---

# 34. Resource Transfer Principle

Cross-process transfer MUST distinguish:

```text
Data Copy
Resource Reference
Ownership Transfer
Lease Grant
Retention Grant
```

They are not equivalent.

---

# 35. Data Copy

If IPC serializes a value:

```text
Process A Value
    |
    | serialize/copy
    v
Process B Value
```

each process owns its physical copy.

No Runtime ownership transfer is implied.

---

# 36. Resource Reference

A resource reference identifies externally managed data.

Example:

```text
ResourceReference
├── ResourceId
├── ResourceType
├── AccessMode
├── LifetimeToken?
└── IntegrityMetadata?
```

Receiving a reference does not automatically grant ownership.

---

# 37. Cross-Process Lease

If Process B needs temporary access:

```text
Resource Owner
    |
    | Lease
    v
Process B
```

The Lease protects resource lifetime.

Lease release MUST occur on:

* normal completion;
* cancellation;
* worker disconnect;
* worker termination;
* shutdown.

---

# 38. Worker Crash and Lease Recovery

Process supervision MUST support orphaned Lease cleanup.

Conceptually:

```text
Worker Crash
    |
    v
Process Death Detected
    |
    v
Worker-Owned Leases Identified
    |
    v
Leases Revoked / Released
    |
    v
Resources Re-evaluated for Disposal
```

This is a major reason process identity may be tracked internally by Resource Manager.

---

# 39. Process Identity

Each managed worker SHOULD have a runtime-local identity:

```text
WorkerProcessId
```

Optionally:

```text
WorkerGeneration
```

Example:

```text
ProviderWorker / Generation 4
```

after three restarts.

These identifiers MUST NOT become high-cardinality metric labels.

---

# 40. Worker Lifecycle

Recommended worker lifecycle:

```text
CREATED
    |
    v
STARTING
    |
    v
INITIALIZING
    |
    v
READY
    |
    v
DRAINING
    |
    v
STOPPED
```

Failure paths MAY include:

```text
FAILED
CRASHED
UNRESPONSIVE
```

---

# 41. Worker Readiness

A process being alive does not mean it is ready.

Runtime MUST distinguish:

```text
Process Alive
Runtime Initialized
Execution Binding Ready
Accepting Work
```

Scheduler admission SHOULD depend on execution readiness, not PID existence.

---

# 42. Worker Startup

Conceptually:

```text
Process Spawned
        |
        v
IPC Connected
        |
        v
Protocol Negotiated
        |
        v
Dependencies Initialized
        |
        v
Execution Bindings Registered
        |
        v
READY
```

Only then should normal work be dispatched.

---

# 43. Protocol Compatibility

Cross-process protocols SHOULD be versioned.

Handshake MAY include:

```text
ProtocolVersion
WorkerType
Capabilities
SupportedWorkTypes
FeatureFlags
RuntimeCompatibility
```

Incompatible workers MUST fail initialization cleanly.

---

# 44. Worker Crash

Worker crash is a physical execution failure boundary.

Runtime Host SHOULD:

1. detect process death;

2. identify active Attempts;

3. mark unresolved physical execution appropriately;

4. release/revoke process-bound Leases;

5. normalize failure;

6. evaluate Retry or Recovery through existing Runtime policies;

7. restart worker if policy allows.

Worker crash MUST NOT bypass Retry Policy.

---

# 45. Crash Does Not Equal WorkItem Failure

Example:

```text
WorkItem W1
    |
    v
Attempt A1
    |
    v
Worker Crash
    |
    v
A1 FAILED
    |
    v
Retry Policy
    |
    v
Attempt A2
```

The logical WorkItem may still succeed.

---

# 46. Crash During Completion Delivery

A difficult case:

```text
Worker creates result
        |
        v
Worker crashes before Completion acknowledged
```

Runtime MUST NOT assume publication succeeded.

Any externally created resource requires cleanup/reconciliation rules.

---

# 47. Duplicate Completion

IPC reconnect/retry MAY produce duplicate messages.

Runtime MUST preserve:

```text
AttemptId-based idempotency
```

A duplicate Completion MUST NOT create:

* duplicate Artifact;
* duplicate terminal Attempt transition;
* duplicate Business result;
* duplicate Presentation commit.

---

# 48. IPC Delivery Semantics

CRAI SHOULD NOT require exactly-once IPC transport.

Preferred design:

```text
at-least-once or best-effort transport
+
idempotent Runtime handling
+
authority validation
```

Correctness belongs to Runtime semantics rather than transport guarantees.

---

# 49. Ordering

IPC message order MUST NOT be globally assumed.

Ordering guarantees SHOULD be scoped narrowly.

Example:

```text
same connection
same Attempt
same control channel
```

may provide ordering if implementation supports it.

Runtime correctness SHOULD tolerate delayed messages.

---

# 50. Backpressure

Every IPC queue MUST be bounded.

When saturated:

* CONTROL messages retain priority;
* Cancellation MUST remain deliverable where possible;
* new work MAY be deferred/rejected;
* low-priority diagnostics MAY be dropped;
* Runtime Host MUST remain responsive.

---

# 51. Control Plane Priority

Recommended conceptual priority:

```text
1. Shutdown / Fatal Control
2. Cancellation / Authority Control
3. Completion
4. Interactive Commands
5. Background Commands
6. Diagnostics
```

Exact queue implementation is defined elsewhere.

---

# 52. Worker Capacity

Workers SHOULD expose bounded capacity.

Examples:

```text
MaxConcurrentAttempts
CurrentInflight
QueueCapacity?
MemoryPressure
GPUCapacity?
```

Scheduler MAY use normalized capacity information.

Workers MUST NOT become hidden unbounded queues.

---

# 53. Hidden Queue Rule

Avoid:

```text
Runtime Queue
    |
    v
Worker IPC Queue
    |
    v
Provider SDK Queue
    |
    v
Unknown Internal Queue
```

without visibility.

Each significant queue SHOULD be:

* bounded;
* observable;
* included in latency accounting where practical.

---

# 54. Latency Accounting

Cross-process latency SHOULD distinguish:

```text
Runtime Queue Wait
IPC Dispatch Delay
Worker Queue Wait
Physical Execution
Completion Delivery
Authority Validation
Publication
```

This allows Observability to explain where latency occurs.

---

# 55. Process Supervision

Application Host or Runtime Host SHOULD supervise managed workers.

Supervision responsibilities MAY include:

* spawn;
* readiness;
* heartbeat;
* crash detection;
* restart;
* drain;
* termination;
* diagnostic capture.

---

# 56. Restart Policy

Restart SHOULD depend on failure classification.

Examples:

```text
Transient Worker Crash
    -> restart allowed

Configuration Error
    -> restart with backoff or stop

Protocol Incompatibility
    -> do not loop restart

Repeated Fatal Crash
    -> circuit-break / degrade
```

Restart policy MUST be bounded.

---

# 57. Restart Backoff

Repeated restart loops MUST be prevented.

Conceptually:

```text
Crash
    |
    v
Restart
    |
    v
Crash
    |
    v
Backoff
    |
    v
Limited Restart
    |
    v
Degraded / Disabled
```

Exact thresholds belong to configuration.

---

# 58. Worker Generation

After restart:

```text
WorkerProcessId = same logical worker?
WorkerGeneration = incremented
```

or a new process identity MAY be allocated.

In either model, stale messages from previous generation MUST be rejectable.

---

# 59. Generation Safety

A message MAY carry:

```text
WorkerProcessId
WorkerGeneration
AttemptId
```

Runtime MAY reject messages originating from obsolete worker generation where required.

Attempt identity remains the primary logical execution identity.

---

# 60. Process Failure Domains

A process boundary creates a physical failure domain.

Examples:

```text
Plugin Worker crash
    != Runtime Host crash

Provider Worker crash
    != UI crash

Capture Worker crash
    != Application termination
```

when isolation is enabled.

---

# 61. Runtime Host Failure

Runtime Host failure is more severe because it owns canonical Runtime coordination state.

MVP MAY treat Runtime Host crash as application-level failure.

Future architecture MAY support:

* persisted recovery metadata;
* Artifact reconciliation;
* worker termination;
* application restart recovery.

Transparent Runtime Host failover is NOT an MVP requirement.

---

# 62. Main Process Failure

If Runtime Host and UI share the main process:

```text
Main Process Crash
    |
    v
Application Crash
```

This is acceptable for MVP.

Process isolation primarily protects the main Runtime from unstable external/native execution.

---

# 63. Plugin Isolation

Plugin isolation level MAY be classified:

```text
TRUSTED_IN_PROCESS
ISOLATED_STANDARD
ISOLATED_RESTRICTED
```

Exact policy belongs to Plugin architecture.

Process topology only defines the execution boundary.

---

# 64. Plugin Capability Boundary

An isolated Plugin SHOULD receive only required capabilities.

Examples:

```text
Read Artifact Reference
Request Work
Publish Candidate Result
Emit Diagnostic
```

It SHOULD NOT receive direct unrestricted Runtime state access.

---

# 65. Security Boundary

Process isolation MAY contribute to security, but:

```text
Separate Process
```

does not automatically mean:

```text
Secure Sandbox
```

True sandboxing may require:

* OS permissions;
* restricted filesystem;
* restricted network;
* capability control;
* token isolation;
* process integrity policy.

---

# 66. Credential Boundary

Provider credentials SHOULD be exposed only to components that require them.

Possible topology:

```text
Runtime Host
    |
    | Credential Reference / Scoped Capability
    v
Provider Worker
```

Raw credentials MUST NOT appear in:

* IPC diagnostics;
* trace;
* logs;
* Runtime Events;
* crash reports.

---

# 67. Reading Content Boundary

Cross-process messages may need reading content for actual work.

However:

```text
work payload
```

and:

```text
telemetry payload
```

remain separate.

Reading content MUST NOT leak into standard IPC diagnostics or telemetry.

---

# 68. Large Payload Strategy

Large data such as:

* screenshots;
* image regions;
* OCR buffers;
* model tensors;

SHOULD avoid repeated serialization/copying where practical.

Preferred future pattern:

```text
Runtime-Managed Resource
        |
        +--> Resource Reference
        |
        +--> Lease
        |
        v
Worker
```

---

# 69. Shared Memory

Shared memory MAY be used for high-volume resources.

If used, Resource Manager MUST define:

* creator;
* owner;
* access rights;
* Lease;
* cleanup;
* crash recovery;
* size bounds;
* integrity validation.

Shared memory MUST NOT bypass Resource Lifecycle semantics.

---

# 70. GPU Resources

GPU resources require special care because handles may be:

* process-local;
* API-specific;
* driver-specific;
* non-transferable.

Therefore process topology MUST NOT assume all Runtime resources can cross process boundaries by reference.

A GPU-backed Artifact MAY require:

```text
copy
conversion
re-materialization
or process-local consumption
```

depending on implementation.

---

# 71. Artifact Store Placement

MVP SHOULD keep canonical Runtime Artifact Store with Runtime Host.

Workers MAY create candidate resources.

Canonical publication remains coordinated by Runtime Host.

Conceptually:

```text
Worker Candidate
        |
        v
Completion
        |
        v
Authority Validation
        |
        v
Ownership Transfer / Materialization
        |
        v
Runtime Artifact Store
```

---

# 72. Candidate Resource Failure

If worker creates a candidate but Completion is rejected:

```text
Candidate Resource
        |
        v
Rejected Completion
        |
        v
Release / Cleanup
```

No stale candidate may become published merely because it exists physically.

---

# 73. Business Acceptance Boundary

Business acceptance remains outside physical worker authority.

A worker MAY produce:

```text
Runtime Artifact
```

but cannot independently declare:

```text
Business Result Accepted
```

unless that business module explicitly owns such acceptance and itself runs there.

Process placement does not change ownership semantics.

---

# 74. Presentation Boundary

Presentation/UI SHOULD normally remain in the main application process.

Reasons:

* UI framework affinity;
* low-latency interaction;
* window lifecycle;
* input handling;
* platform restrictions.

Background workers MUST NOT directly mutate UI state.

---

# 75. Worker to UI Rule

Forbidden:

```text
Worker Process
    |
    v
Direct UI Mutation
```

Required conceptual path:

```text
Worker Completion
        |
        v
Runtime Host
        |
        v
Business Acceptance
        |
        v
Presentation
        |
        v
UI Commit
```

---

# 76. Observability Across Processes

Observability correlation MUST survive IPC.

Minimum relevant identities:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
TraceId
SpanContext?
```

WorkerProcessId MAY be added for local diagnostics.

---

# 77. Cross-Process Trace

Example:

```text
ExecutionRevision Trace
├── Scheduler Decision
├── IPC Dispatch
├── Worker Queue
├── Attempt Execution
├── IPC Completion
├── Authority Validation
├── Artifact Publication
├── Business Acceptance
└── Presentation Commit
```

---

# 78. Worker Metrics

Useful process-level metrics MAY include:

```text
worker.process_count
worker.restart_total
worker.crash_total
worker.unresponsive_total
worker.startup_ms
worker.inflight
worker.ipc_queue_depth
worker.ipc_dispatch_ms
worker.completion_delivery_ms
worker.memory_bytes
```

WorkerProcessId MUST NOT be a metric label.

---

# 79. IPC Metrics

Possible:

```text
ipc.message_total
ipc.send_failure_total
ipc.receive_failure_total
ipc.serialization_ms
ipc.payload_size_bytes
ipc.queue_depth
ipc.queue_wait_ms
ipc.connection_restart_total
ipc.protocol_error_total
```

Dimensions MUST remain bounded.

---

# 80. Worker Logs

Worker logs SHOULD include:

```text
worker.type
worker.generation
execution_scope.id?
execution_revision.id?
workitem.id?
attempt.id?
error.code
physical.outcome
```

Raw payload remains excluded.

---

# 81. Worker Diagnostic Snapshot

Development diagnostics MAY expose:

```text
Worker
├── Type
├── Lifecycle State
├── Generation
├── Ready
├── Accepting Work
├── Inflight Attempts
├── Queue Depth
├── Active Leases
├── Memory Estimate
├── Last Heartbeat
└── Recent Errors
```

---

# 82. Heartbeat

Long-lived isolated workers MAY support heartbeat/health signaling.

Heartbeat MUST remain lightweight.

Heartbeat loss does not immediately prove process death.

Possible progression:

```text
Healthy
    |
    v
Suspected Unresponsive
    |
    v
Unresponsive
    |
    v
Recovery / Termination
```

---

# 83. Health vs Readiness

Distinguish:

```text
Health
Readiness
Capacity
```

A worker may be:

```text
Healthy
but
Not Ready
```

during model loading.

It may also be:

```text
Healthy
Ready
but
At Capacity
```

Scheduler decisions should use the appropriate signal.

---

# 84. Process-Level Resource Pressure

Workers MAY report normalized pressure:

```text
NORMAL
ELEVATED
HIGH
CRITICAL
```

The Runtime Host MAY use pressure for admission/routing decisions.

Workers SHOULD NOT independently alter global Runtime policy.

---

# 85. Process Isolation and Retry

Worker restart and WorkItem Retry are distinct.

Example:

```text
Worker Crash
    |
    +--> Process Supervisor decides restart
    |
    +--> Runtime Retry Policy evaluates WorkItem
```

Neither decision implies the other.

---

# 86. Process Isolation and Recovery

Alternative execution MAY involve another worker/binding.

Example:

```text
Attempt A1
    |
    v
Worker A unavailable
    |
    v
Recovery / Routing
    |
    v
Binding B selected
    |
    v
Attempt A2?
```

Whether this is the same WorkItem and whether a new Attempt is created follows Runtime execution semantics.

Fallback MUST NOT be redefined as process restart.

---

# 87. Process Isolation and Cache

Cache ownership does not depend on worker placement.

Workers MAY request or consume cache-backed resource references if permitted.

Workers MUST NOT silently create independent canonical caches that bypass Runtime Cache Policy.

Process-local ephemeral caches MAY exist if:

* bounded;
* non-authoritative;
* safe to lose;
* observable where significant.

---

# 88. Process Isolation and Storage

Persistent Storage SHOULD remain accessed through defined ownership/contracts.

A worker SHOULD NOT gain unrestricted Storage access merely because it is isolated.

Where possible:

```text
Worker
    |
    v
Runtime/Module Contract
    |
    v
Storage
```

is preferred over arbitrary direct database/filesystem mutation.

---

# 89. Process Isolation and Event Bus

Cross-process architecture MUST NOT imply that the entire Event Bus becomes IPC.

Possible implementation:

```text
Local Event Bus
+
IPC Bridge for selected event classes
```

Only events requiring cross-process delivery SHOULD cross the boundary.

---

# 90. Event Bridge

An IPC Event Bridge MUST preserve:

* event identity;
* correlation;
* schema version;
* bounded payload;
* ordering assumptions;
* duplicate handling.

It MUST NOT forward all events indiscriminately.

---

# 91. Command vs Event

Across process boundaries:

```text
Command
```

requests an action.

```text
Event
```

reports something that happened.

These semantics MUST remain distinct even if both use the same IPC transport.

---

# 92. IPC Error Normalization

Transport-specific failures SHOULD be normalized.

Examples:

```text
IPC_CONNECTION_LOST
IPC_PROTOCOL_MISMATCH
IPC_SERIALIZATION_FAILED
WORKER_UNAVAILABLE
WORKER_CRASHED
WORKER_UNRESPONSIVE
```

Runtime policies SHOULD consume normalized errors rather than platform-specific exceptions.

---

# 93. Process Startup Ordering

Recommended conceptual startup:

```text
Application Host
        |
        v
Runtime Core
        |
        v
Resource / Artifact Infrastructure
        |
        v
Process Supervisor
        |
        v
Required Workers
        |
        v
Execution Bindings Ready
        |
        v
Business Modules Ready
        |
        v
Presentation Ready
        |
        v
Accept Work
```

Optional workers MUST NOT block startup unless required by active capabilities.

---

# 94. Lazy Worker Startup

Expensive workers MAY start lazily.

Example:

```text
Translation Work First Requested
        |
        v
Translation Worker Start
        |
        v
Model Load
        |
        v
Binding Ready
        |
        v
Attempt Dispatch
```

Cold-start latency MUST be observable.

---

# 95. Prewarming

Frequently used workers MAY be prewarmed based on configuration.

Prewarming is an optimization.

It MUST NOT alter correctness or authority semantics.

---

# 96. Shutdown Ordering

Recommended:

```text
Shutdown Requested
        |
        v
Stop New Admission
        |
        v
Revoke Application / Scope Authority
        |
        v
Cancel Queued / Running Work
        |
        v
Drain Worker Attempts
        |
        v
Release Worker Leases
        |
        v
Shutdown Workers
        |
        v
Dispose Runtime Resources
        |
        v
Flush Bounded Telemetry
        |
        v
Stop Runtime Host
        |
        v
Exit Application
```

---

# 97. Forced Worker Termination

If a worker exceeds shutdown deadline:

```text
Graceful Drain
    |
    v
Timeout
    |
    v
Forced Termination
```

MAY occur.

Before/after termination Runtime SHOULD reconcile:

* Attempts;
* Leases;
* candidate resources;
* temporary storage;
* worker diagnostics.

---

# 98. Shutdown Invariant

Application shutdown MUST NOT wait indefinitely for an isolated worker.

All worker drain waits require bounded deadlines.

---

# 99. Process Topology Configuration

Runtime configuration MAY include:

```text
worker.isolation_mode
worker.max_processes
worker.startup_timeout
worker.shutdown_timeout
worker.heartbeat_interval
worker.unresponsive_timeout
worker.restart_limit
worker.restart_backoff
worker.memory_limit?
worker.prewarm
```

Exact configuration schema belongs to `RUNTIME_CONFIG.md`.

---

# 100. Isolation Modes

Possible conceptual modes:

```text
IN_PROCESS
AUTO
ISOLATED
```

Meaning:

### IN_PROCESS

Run compatible component inside Runtime process.

### AUTO

Architecture/runtime chooses based on capability and platform.

### ISOLATED

Require process isolation when supported.

Not every component must support every mode.

---

# 101. Platform Differences

Process topology MAY differ by platform because of:

* screen-capture APIs;
* GPU runtime;
* process sandboxing;
* UI framework;
* model runtime;
* IPC capabilities;
* OS security model.

Architecture semantics MUST remain stable across platforms.

---

# 102. Windows

Potential implementation mechanisms MAY include:

* named pipes;
* memory-mapped files;
* process handles;
* job objects;
* shared GPU/native handles where supported.

No specific mechanism is mandated at architecture level.

---

# 103. Linux

Potential mechanisms MAY include:

* Unix domain sockets;
* pipes;
* shared memory;
* memfd;
* process groups;
* file descriptors.

No specific mechanism is mandated here.

---

# 104. macOS

Potential mechanisms MAY include:

* Unix domain sockets;
* XPC-like isolation patterns;
* shared memory;
* platform-native process services.

Exact mechanism remains implementation-specific.

---

# 105. Process Topology and Packaging

Packaging MAY choose:

```text
single executable
+
spawned worker mode
```

or:

```text
main executable
+
worker executables
```

Architecture does not require a specific packaging model.

---

# 106. Same-Binary Worker Mode

A future implementation MAY support:

```text
crai --worker=provider
crai --worker=capture
crai --worker=plugin
```

This can reduce packaging complexity.

This is an implementation option, not an architecture requirement.

---

# 107. Separate Worker Binary

Separate binaries MAY be preferred when:

* dependencies differ significantly;
* language/runtime differs;
* security policy differs;
* deployment lifecycle differs.

Example:

```text
crai.exe
crai-provider-worker.exe
crai-plugin-worker.exe
```

Again, this is not required for MVP.

---

# 108. Cross-Language Worker

A worker MAY use a different implementation language.

Example:

```text
Main Runtime: native/.NET/etc.
Model Worker: Python
```

provided IPC contract remains:

* explicit;
* versioned;
* bounded;
* observable;
* lifecycle-safe.

Architecture MUST NOT depend on shared in-process objects across such boundary.

---

# 109. Process Topology and Plugin Ecosystem

Plugin isolation creates a future path for third-party extensibility.

However MVP SHOULD prefer:

```text
built-in / trusted plugins
```

unless external Plugin execution is already required.

Security sandboxing should not be simulated with process separation alone.

---

# 110. Development Mode

Development mode MAY run more components in-process for:

* debugging;
* profiling;
* simpler stack traces.

Alternatively it MAY force isolation to test production topology.

Both modes SHOULD preserve Runtime semantics.

---

# 111. Test Topology

Tests SHOULD support deterministic fake workers.

Example:

```text
Runtime Host
    |
    v
Fake IPC Worker
```

Test harness SHOULD control:

* Completion timing;
* crash;
* disconnect;
* duplicate messages;
* stale messages;
* cancellation acknowledgement;
* resource transfer;
* restart.

---

# 112. Process Failure Tests

Required scenarios SHOULD include:

* worker crashes before execution;
* worker crashes during execution;
* worker crashes after result creation;
* worker crashes during Completion delivery;
* worker ignores cancellation;
* worker becomes unresponsive;
* worker reconnects;
* old-generation message arrives;
* duplicate Completion arrives;
* IPC queue saturates;
* protocol mismatch;
* worker restart loop.

---

# 113. Resource Failure Tests

Test:

* Lease cleanup after worker death;
* candidate cleanup after rejected Completion;
* shared-resource disposal;
* disposal while worker still references resource;
* worker death during ownership transfer;
* temporary file/shared memory cleanup;
* shutdown with active worker resources.

---

# 114. Authority Tests

Test:

```text
Worker completes after ExecutionRevision superseded
    -> REJECT_STALE

Worker completes after cancellation
    -> REJECT_CANCELLED

Duplicate Completion
    -> no duplicate terminal effect

Old WorkerGeneration Completion
    -> rejected where generation validation applies
```

---

# 115. Observability Tests

Verify cross-process trace retains:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
```

and correctly separates:

```text
IPC Dispatch
Worker Queue
Physical Execution
Completion Delivery
Authority Validation
Publication
```

---

# 116. Performance Tests

Measure:

* IPC overhead;
* serialization cost;
* payload copy cost;
* shared-resource overhead;
* worker startup;
* model cold-start;
* worker restart;
* memory duplication;
* cross-process cancellation latency.

Isolation SHOULD be justified by measured benefit where performance-sensitive.

---

# 117. Long-Session Tests

Verify:

* no orphan workers;
* no leaked IPC handles;
* no leaked shared memory;
* no orphaned Leases;
* bounded worker count;
* bounded restart rate;
* stable process memory;
* clean shutdown.

---

# 118. Architecture Invariants

1. Process topology does not redefine Runtime semantics.

2. ExecutionScope/ExecutionRevision/WorkItem/Attempt identities survive process boundaries.

3. Worker processes do not own canonical Runtime execution authority.

4. Physical success does not imply Runtime acceptance.

5. Runtime Host validates Completion authority.

6. Logical cancellation precedes dependence on physical cancellation.

7. Worker inability to abort does not preserve authority.

8. Process restart and WorkItem Retry are separate decisions.

9. Worker crash does not automatically fail the logical WorkItem.

10. Fallback/recovery is not equivalent to worker restart.

11. Module boundary and process boundary are distinct.

12. Process boundary and thread boundary are distinct.

13. Process placement does not redefine Business ownership.

14. Process placement does not redefine Artifact ownership.

15. Data copy and ownership transfer are distinct.

16. Resource reference and ownership transfer are distinct.

17. Lease and ownership are distinct.

18. Worker crash releases/reconciles process-bound Leases.

19. IPC queues are bounded.

20. Worker queues are bounded.

21. Control/cancellation traffic cannot be indefinitely blocked behind background work.

22. Duplicate Completion is idempotently handled.

23. Global IPC ordering is not assumed.

24. Runtime correctness does not require exactly-once transport.

25. Runtime Artifact publication remains Runtime-coordinated.

26. Business acceptance remains owner-controlled.

27. Worker processes do not directly mutate UI.

28. Reading content does not leak into standard telemetry.

29. Credentials do not leak through IPC diagnostics.

30. Worker process IDs are not aggregate metric labels.

31. Worker restart loops are bounded.

32. Worker readiness is distinct from process liveness.

33. Health, readiness and capacity remain distinct.

34. Shared memory does not bypass Resource Lifecycle.

35. GPU resource transferability is never assumed.

36. Process shutdown waits are bounded.

37. Optional workers do not block startup unnecessarily.

38. Cross-process protocol is versioned.

39. Transport errors are normalized before Runtime policy consumes them.

40. MVP may remain predominantly single-process.

---

# 119. MVP Decision

For CRAI MVP:

```text
Default:
    Single Main Process
```

containing:

```text
UI / Presentation
Application Host
Runtime Control
Scheduler
Work Queues
Artifact Store
Resource Manager
Cache
Observability
Business Modules
Capture Adapter
Provider Adapters
Built-in Plugins
```

Process isolation is introduced only where a concrete dependency requires it.

---

# 120. Post-MVP Evolution

Likely isolation order:

```text
1. Unstable / native Provider or Model Runtime
2. Third-party Plugin execution
3. Heavy local AI models
4. Platform-specific Capture runtime
5. Other high-risk native components
```

This is a likely evolution path, not a mandatory implementation sequence.

---

# 121. Open Questions

* Which implementation language/runtime will host CRAI Runtime?
* Which UI framework will be used?
* Does local OCR require native/model process isolation?
* Does Translation use remote providers, local models or both?
* Which GPU APIs need support?
* Should Plugin isolation be mandatory for third-party Plugins?
* Which IPC mechanism is preferred per platform?
* Is shared memory required for screenshots in MVP?
* Should workers be same-binary modes or separate binaries?
* How much worker state survives restart?
* Should worker supervision belong to Application Host or Runtime Host implementation?
* Are process memory limits required?
* Should local model workers be pooled or one-per-model?
* Which execution bindings require prewarming?
* How are worker capabilities negotiated?
* How should crash dumps be sanitized?
* Which resource types can safely cross process boundaries without copying?

---

# 122. Related Documents

Runtime:

* `RUNTIME_COMPONENTS.md`
* `PIPELINE_RUNTIME.md`
* `THREADING_MODEL.md`
* `WORK_QUEUE.md`
* `SCHEDULER.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`
* `ERROR_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `MEMORY_MODEL.md`
* `CACHE_POLICY.md`
* `PERFORMANCE_MODEL.md`
* `RUNTIME_CONFIG.md`
* `BOOT_SEQUENCE.md`
* `RUNTIME_OBSERVABILITY.md`

External:

* `../core/EVENT_BUS.md`
* `../plugin/PLUGIN_SYSTEM.md`
* `../plugin/PLUGIN_LIFECYCLE.md`
* `../../02-modules/provider-management/`
* `../../02-modules/presentation/`

---

# 123. Completion Criteria

`PROCESS_TOPOLOGY.md` is considered architecture-ready when:

* process and module boundaries are clearly separated;
* process and threading boundaries are clearly separated;
* MVP topology is defined;
* future isolation path is defined without requiring premature multi-process design;
* Runtime Host remains authoritative for Runtime coordination;
* cross-process identity propagation is defined;
* Completion authority validation remains Runtime-owned;
* cancellation semantics survive IPC;
* worker crash behavior is defined;
* Retry and worker restart remain separate;
* Recovery/Fallback and worker restart remain separate;
* IPC is bounded and versioned;
* duplicate handling is defined;
* resource references, copies, ownership and Leases are distinguished;
* worker crash cleanup is defined;
* UI mutation remains outside workers;
* observability survives process boundaries;
* shutdown ordering and bounded drain are defined;
* privacy and credential boundaries are explicit;
* implementation-specific IPC technology remains replaceable.

---

# 124. Summary

CRAI begins with a predominantly:

```text
Single-Process Runtime
```

because this minimizes complexity and latency.

The architecture nevertheless preserves a migration path toward:

```text
Main Runtime Host
        |
        +--> Capture Worker
        |
        +--> AI / Provider Worker
        |
        +--> Model Worker
        |
        +--> Plugin Worker
```

when isolation becomes justified.

The fundamental rule is:

```text
Process placement may change.
Runtime semantics must not.
```

Across every topology:

```text
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
```

remains the canonical execution identity.

Workers perform physical execution.

Runtime Host validates authority.

Business owners accept business results.

Presentation owns visible commit.

Resource Lifecycle governs ownership, retention, Lease and disposal.

Process isolation therefore strengthens CRAI's stability and extensibility without becoming a second orchestration model.
