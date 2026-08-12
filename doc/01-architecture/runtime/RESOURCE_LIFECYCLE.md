# Runtime Resource Lifecycle

* **Document:** Runtime Architecture / Resource Lifecycle
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines the canonical lifecycle semantics for CRAI Runtime resources.

It defines:

```text
Creation
Registration
Ownership
Visibility
Retention
Lease
Use
Logical Disposal
Draining
Physical Disposal
```

The central resource-lifecycle concepts are:

```text
Ownership
Retention
Lease
Execution Relevance
Physical Use
Disposal
```

The lifecycle MUST ensure that:

* every significant resource has explicit ownership;
* sharing does not erase ownership boundaries;
* retention remains distinct from ownership;
* Runtime execution authority remains distinct from resource ownership;
* active leases protect physical use;
* physical child operations remain truthfully accounted;
* canceled/stale execution cannot revive disposed resources;
* cleanup failure does not restore execution authority;
* resource lifetime remains bounded and observable.

---

# 2. Core Model

Conceptually:

```text
Create
    |
    v
Register
    |
    v
Transfer Ownership? / Publish?
    |
    v
Retain / Acquire Lease
    |
    v
Use
    |
    v
Release Lease / Retention
    |
    v
Logical Disposal
    |
    v
Draining
    |
    v
Physical Disposal
```

Not every resource passes through every phase.

Example:

```text
Attempt-local buffer

Create
    |
    v
Use
    |
    v
Physical Disposal
```

A published shared Runtime Artifact normally uses more of the lifecycle.

---

# 3. Architectural Position

```text
Runtime Control
    -> owns execution authority

Execution State Store
    -> owns ExecutionScope / ExecutionRevision metadata

Runtime Artifact Store
    -> owns published Runtime Artifact lifecycle

Cache Policy
    -> owns cache-retention/reuse mechanics

Resource Manager
    -> coordinates physical resource accounting/disposal

Worker / Attempt
    -> owns Attempt-local resources

Provider Runtime Gateway / Adapter
    -> owns provider-runtime resources

Presentation / Application
    -> owns Presentation/UI resources

Storage
    -> owns durable persistence mechanics
```

No component may silently acquire responsibilities outside its ownership boundary.

---

# 4. Core Principles

1. Every significant physical payload/resource has one logical lifecycle owner at a time.

2. A resource MAY have multiple retention reasons.

3. Lease does not transfer ownership.

4. Publication does not automatically transfer ownership.

5. Execution authority is not ownership.

6. Retention is not ownership.

7. Visibility is not ownership.

8. Logical disposal precedes physical disposal.

9. Physical disposal requires ownership/retention/lease/physical-use eligibility.

10. Worker does not dispose shared Runtime Artifacts.

11. Cache promotion does not change payload owner.

12. Cache promotion does not copy payload by default.

13. A physically disposed resource cannot be resurrected.

14. Draining resources remain observable/accounted.

15. Cleanup failure does not restore execution authority.

16. Native/GPU/OS resources require explicit lifecycle.

17. Runtime lifecycle correctness does not depend on GC timing.

18. Runtime Artifact publication does not imply Business acceptance.

---

# 5. Resource Dimensions

CRAI SHOULD model resource lifecycle using independent dimensions rather than one overloaded state machine.

Recommended dimensions:

```text
Registration State
Ownership State
Visibility State
Retention State
Lease State
Usage State
Disposal State
Integrity State
```

These dimensions interact but are not the same thing.

---

# 6. Why Lifecycle Dimensions Are Separate

Example:

```text
Resource registered
    but still private

Resource published
    while Artifact Store owns payload

Resource logically disposed
    but still physically used by active lease

Resource no longer execution-current
    but still cache-retained
```

A single linear lifecycle state cannot represent all combinations cleanly.

---

# 7. Resource Categories

## 7.1 Runtime-Global Resource

Examples:

```text
worker pool
provider runtime client
configuration snapshot registry
shared model
Event Bus infrastructure
bounded buffer pool
```

---

## 7.2 ExecutionScope-Scoped Resource

Examples:

```text
ExecutionScope runtime metadata
scope cancellation context
scope runtime Artifact retention
scope resource accounting
```

Business ReadingSession state remains outside Runtime lifecycle ownership.

---

## 7.3 ExecutionRevision-Scoped Resource

Examples:

```text
ExecutionRevision metadata
BusinessExecutionPlan reference
WorkItem references
accepted RuntimeArtifactRefs
ExecutionRevision resource accounting
drain state
```

---

## 7.4 Attempt-Local Resource

Examples:

```text
temporary buffer
provider request body
intermediate tensor
temporary file
child process handle
request/response buffer
```

---

## 7.5 Shared Runtime Resource

Examples:

```text
published Runtime Artifact
shared local model
provider runtime client
Presentation resource
shared native/GPU backing
```

---

## 7.6 External / Native Resource

Examples:

```text
HTTP request
GPU context
capture surface
native handle
child process
memory mapping
IPC object
graphics surface
```

---

# 8. Registration State

Possible:

```text
UNREGISTERED
REGISTERED
DEREGISTERED
```

Registration provides lifecycle tracking.

It does NOT imply:

```text
published
shared
accepted
retained
```

---

# 9. Ownership State

Recommended:

```text
CREATOR_OWNED
TRANSFER_PENDING
COMPONENT_OWNED
NO_LOGICAL_OWNER
PHYSICALLY_DISPOSED
```

Ownership transfer MUST be:

```text
atomic
or
rollback-safe
```

---

# 10. Visibility State

Recommended:

```text
PRIVATE
PUBLISHED
WITHDRAWN
```

Visibility remains separate from ownership.

Examples:

```text
Attempt-local buffer
    PRIVATE

Artifact candidate
    PRIVATE

Published Runtime Artifact
    PUBLISHED

Logically disposed Artifact
    WITHDRAWN from new lookup
```

---

# 11. Disposal State

Recommended canonical disposal state:

```text
ACTIVE
    |
    v
LOGICAL_DISPOSAL_REQUESTED
    |
    v
LOGICALLY_DISPOSED
    |
    v
DRAINING
    |
    +--> DISPOSAL_FAILED
    |
    v
PHYSICALLY_DISPOSED
```

Not all resources require `DRAINING`.

---

# 12. Integrity State

Possible:

```text
VALID
INVALID
CORRUPT
UNKNOWN
```

Integrity is not lifecycle ownership.

Invalid resource may remain physically alive until disposal is safe.

---

# 13. Candidate Resource

A producer may first create a private candidate:

```text
Temporary Output
        |
        v
Candidate Resource
```

A candidate:

* remains producer-owned or transfer-pending;
* is not available through normal shared lookup;
* is not cache-retained;
* has no accepted Runtime publication status;
* must be cleaned up if validation/publication fails.

---

# 14. Candidate Resource vs Business Result

Candidate resource status says nothing about Business semantics.

Example:

```text
Candidate Runtime Artifact
    !=
accepted Translation result
```

The owning Business Module decides semantic acceptance separately.

---

# 15. Publication

Publication means:

```text
resource becomes observable
through an approved Runtime contract
```

Publication does NOT automatically mean:

* ownership transfer;
* Business acceptance;
* cache promotion;
* durable persistence;
* Domain commit;
* Presentation commit.

---

# 16. Runtime Artifact Publication

Recommended:

```text
Producer Candidate
        |
        v
Artifact Candidate Registration
        |
        v
Runtime Authority Validation
        |
        v
Ownership Transfer
        |
        v
Runtime Artifact Publication
        |
        v
RuntimeArtifactRef
```

Implementation MAY combine transfer/publication atomically.

Externally visible state MUST remain consistent.

---

# 17. Runtime Artifact vs Business Acceptance

Critical distinction:

```text
Runtime Artifact published
    = execution payload safely available
```

```text
Business Result accepted
    = owning Business Module accepts semantics
```

These MUST remain separate.

---

# 18. Ownership vs Execution Authority

```text
Runtime Artifact Store
    owns Artifact lifecycle

Runtime Control
    owns execution authority
```

A resource may still physically exist after execution authority is gone.

---

# 19. Authority Loss

When execution loses authority:

* new accepted execution use may be denied;
* queued/running execution may be canceled/drained;
* relevant ExecutionRevision retention may be released;
* cache/Presentation retention may remain;
* existing valid leases may remain;
* physical disposal is re-evaluated.

Authority loss alone MUST NOT force immediate physical disposal.

---

# 20. Ownership vs Retention

Example:

```text
Runtime Artifact Store
    = lifecycle owner

ExecutionRevision retention
ExecutionScope retention
Cache retention
Presentation retention
Diagnostic retention
    = retention reasons
```

Retention prevents disposal.

Retention does not permit arbitrary mutation/disposal.

---

# 21. Retention Model

Recommended:

```text
RetentionRecord
├── retentionId
├── resourceId
├── retentionKind
├── ownerReference
├── createdAt
├── expiresAt?
└── metadata
```

Possible retention kinds:

```text
EXECUTION_REVISION
EXECUTION_SCOPE
CACHE
PRESENTATION
DIAGNOSTIC
APPLICATION
EXTERNAL
```

---

# 22. Retention Release

Removing one retention reason:

```text
does not
```

imply resource disposal if another reason remains.

---

# 23. Resource Lease

`ResourceLease` represents temporary protected use.

Lease does NOT transfer ownership.

Possible lease types:

```text
RuntimeArtifactLease
GpuResourceLease
NativeHandleLease
ProviderResourceLease
CaptureSurfaceLease
GraphicsResourceLease
```

---

# 24. Lease Requirements

Lease SHOULD include:

```text
LeaseId
ResourceId
LeaseOwner
AcquiredAt
ExpectedLifetime?
CancellationReference?
AffinityMetadata?
```

Lease MUST be:

* releasable;
* observable;
* bounded or leak-detectable;
* invalid after release.

---

# 25. Lease Timeline

```text
Acquire Lease
        |
        v
Use Resource
        |
        v
Release Lease
        |
        v
Disposal Eligibility Re-evaluated
```

Lease release does not itself dispose resource.

---

# 26. New Lease During Disposal

Once logical disposal begins:

```text
new lease
```

SHOULD normally be rejected.

Existing valid leases remain protected.

Exceptions MUST be explicit and bounded.

---

# 27. Creation

Creator owns the resource immediately after successful creation.

Creator MUST know:

* resource class;
* initial owner;
* cleanup path;
* expected lifetime;
* size/cost estimate where relevant;
* transfer target if applicable;
* failure behavior before registration.

---

# 28. Registration

Large/shared resources SHOULD normally register before sharing.

Registration MAY assign:

```text
ResourceId
ResourceType
Owner
SizeEstimate
BackingResourceRef
IntegrityState
CreatedAt
DisposalCoordinator
```

---

# 29. Ownership Transfer

Example:

```text
Worker owns candidate
        |
        v
Transfer Prepared
        |
        v
Runtime Authority Validation
        |
        v
Runtime Artifact Store accepts lifecycle ownership
        |
        v
Publication becomes visible
        |
        v
Worker releases creator ownership
```

---

# 30. Transfer Failure

If transfer fails:

* producer remains owner;
* candidate remains private;
* candidate cleanup is required;
* no half-published resource exists;
* another component MUST NOT assume ownership.

---

# 31. Shared Resource

Shared resource has:

```text
one lifecycle owner
zero..N retention records
zero..N active leases
explicit physical-disposal coordinator
```

Immutable Runtime Artifact additionally has immutable payload semantics.

---

# 32. Attempt-Local Resource

Attempt-local resource:

* belongs to Worker/Execution Adapter;
* is private by default;
* has no shared retention by default;
* is released during Attempt cleanup;
* may remain draining if physical child operation cannot stop;
* MUST NOT be retained by Event Bus.

---

# 33. Runtime Artifact Lifecycle

Conceptually:

```text
Candidate Created
        |
        v
Registered
        |
        v
Runtime Authority Validated
        |
        v
Ownership Transferred
        |
        v
Published
        |
        v
Retained / Leased
        |
        v
Logical Disposal
        |
        v
Drain
        |
        v
Physical Disposal
```

Published Runtime Artifact is immutable.

---

# 34. Cache Promotion

Default cache promotion flow:

```text
Runtime Artifact published
        |
        v
Business Result accepted
        |
        v
Owner declares cache eligibility
        |
        v
Cache Policy permits retention
        |
        v
Cache retention added
```

Payload owner remains Runtime Artifact Store.

---

# 35. Cache Eviction

```text
Cache retention removed
        |
        v
Disposal eligibility re-evaluated
```

If another retention/lease remains:

```text
payload remains alive
```

---

# 36. ExecutionRevision Lifecycle Interaction

When an ExecutionRevision is superseded/cancelled:

```text
Execution Authority Revoked
        |
        v
No New Relevant Work
        |
        v
Queued Work Removed
        |
        v
Running Attempts Drain / Cancel
        |
        v
ExecutionRevision Retention Released
        |
        v
Resource Disposal Eligibility Re-evaluated
```

ExecutionRevision metadata and physical Artifact payloads have independent lifecycle.

---

# 37. ExecutionScope Shutdown

Recommended:

```text
Runtime Control revokes ExecutionScope authority
        |
        v
Child cancellation propagated
        |
        v
Queued Work removed
        |
        v
Running Attempts drain
        |
        v
ExecutionScope runtime retention released
        |
        v
Presentation retention handled by Presentation owner
        |
        v
Physical disposal when eligible
```

ReadingSession business lifecycle remains separately owned.

---

# 38. Application Shutdown

Recommended:

```text
Stop New Admission
        |
        v
Revoke Application Execution Authority
        |
        v
Cancel ExecutionScopes
        |
        v
Drain Queues / Workers
        |
        v
Release Attempt Resources
        |
        v
Release Leases / Runtime Retention
        |
        v
Stop Provider / Plugin Runtime
        |
        v
Dispose Runtime Resources
        |
        v
Flush Bounded Diagnostics
```

Exact ordering follows `BOOT_SEQUENCE.md`.

---

# 39. Provider Runtime Resource Lifecycle

Provider Runtime Gateway / Adapter owns provider-runtime instances/resources.

Possible conceptual lifecycle:

```text
CREATED
    |
    v
INITIALIZING
    |
    v
READY
    |
    +--> IDLE
    |
    v
DRAINING
    |
    v
UNLOADING
    |
    v
DISPOSED
```

Provider Management does NOT own these physical runtime resources.

---

# 40. Local Model Lifecycle

Possible:

```text
UNLOADED
    |
    v
LOADING
    |
    v
READY
    |
    +--> IDLE
    |
    v
DRAINING
    |
    v
UNLOADING
    |
    v
DISPOSED
```

Unload must account for:

* active provider operations;
* active lease;
* GPU/native memory;
* shutdown;
* Runtime resource pressure.

---

# 41. Worker Boundary

Worker:

```text
owns Attempt-local resources
may acquire leases
releases Attempt-local ownership
reports cleanup outcome
```

Worker MUST NOT independently dispose shared Runtime Artifact backing.

---

# 42. Native / GPU / OS Resource Lifecycle

Native/external resources require:

* explicit owner;
* creation-failure path;
* affinity rules where relevant;
* explicit release/disposal;
* disposal timeout;
* leak observability;
* bounded cleanup retry;
* containment strategy when cleanup fails.

GC/finalizers are not sufficient guarantees.

---

# 43. Resource Dependency Graph

Conceptually:

```text
ExecutionScope
    |
    v
ExecutionRevision
    |
    +--> Runtime Artifact Retention
    |
    +--> Presentation Retention Reference
    |
    +--> WorkItem
            |
            v
          Attempt
            |
            +--> Temporary Buffer
            +--> Provider Child Operation
            +--> GPU Lease
            +--> RuntimeArtifactLease
```

Resource dependency graph is about lifecycle dependencies.

It MUST NOT encode Business pipeline stage order.

---

# 44. Disposal Eligibility

Physical disposal SHOULD require:

```text
No Lifecycle Ownership Need
No Retention
No Active Lease
No Active Physical Use
No Required Presentation Retention
No Required Diagnostic Retention
Physical Cleanup Safe
```

---

# 45. Authority and Disposal Eligibility

Do NOT model:

```text
No Runtime Authority
```

as equivalent to:

```text
Disposable
```

Instead:

```text
Execution authority loss
    may trigger logical disposal/release

Physical disposal
    depends on ownership/retention/lease/use
```

---

# 46. Logical Disposal

Logical disposal means the resource should no longer participate in new normal Runtime use.

Typical actions:

* withdraw from new lookup;
* deny new lease;
* release eligible retention;
* reject new publication/use;
* mark pending cleanup/drain;
* prevent resurrection.

Resource MAY still physically exist.

---

# 47. Draining

`DRAINING` means:

```text
resource is no longer accepting normal new use
but physical use/cleanup remains
```

Possible causes:

* active lease;
* non-cancelable provider request;
* native operation;
* UI/graphics release pending;
* asynchronous cleanup;
* process termination pending.

Draining MUST remain observable.

---

# 48. Draining Timeout

Draining resource SHOULD have:

```text
expected lifetime
or
timeout/leak threshold
```

Timeout does not necessarily justify unsafe forced disposal.

It triggers diagnostics/containment policy.

---

# 49. Physical Disposal

Physical disposal MUST:

* respect active leases;
* respect physical child use;
* respect affinity requirements;
* be idempotent where practical;
* report success/failure;
* release backing resource;
* transition to terminal disposed state.

---

# 50. Disposal Coordinator

Physical disposal may be coordinated by:

```text
Runtime Artifact Store
Resource Manager
Provider Runtime Gateway / Adapter
Presentation/Application
```

depending on resource ownership.

There is no universal disposal owner for every resource type.

---

# 51. Generic Disposal Order

Recommended conceptual order:

```text
Stop New Use
        |
        v
Revoke/Withdraw Runtime Eligibility
        |
        v
Release Attempt-Local Resources
        |
        v
Release Retention Owners
        |
        v
Drain Physical Operations
        |
        v
Release Leases
        |
        v
Dispose Shared Physical Backing
        |
        v
Dispose ExecutionRevision / ExecutionScope Metadata
        |
        v
Dispose Runtime-Global Resources
```

Actual ordering follows the resource dependency graph.

---

# 52. Resource Resurrection

After:

```text
PHYSICALLY_DISPOSED
```

the same ResourceId MUST NOT become active again.

Reuse requires:

```text
new resource
+
new lifecycle registration
```

Resurrection is an invariant violation.

---

# 53. Resource Identity

Recommended:

```text
ResourceId
ResourceType
ResourceVersion?
CompatibilityMetadata?
```

Identity/version/semantic compatibility are separate concerns.

Business Module defines semantic compatibility where relevant.

---

# 54. Cleanup Failure

Cleanup failure creates a normalized Runtime cleanup error.

Recommended model:

```text
Primary Execution Outcome
        +
Cleanup Outcome
```

Cleanup failure MUST NOT:

* restore execution authority;
* restore ownership already released;
* revive resource;
* create false successful disposal.

---

# 55. Disposal Failure State

If physical cleanup fails:

```text
DRAINING
    |
    v
DISPOSAL_FAILED
```

The resource remains:

* non-active;
* non-reusable;
* observable;
* physically accounted.

---

# 56. Cleanup Retry

Cleanup retry is distinct from WorkItem Retry.

Cleanup retry:

* creates no Business WorkItem;
* does not change WorkItem/Attempt lineage;
* grants no execution authority;
* only retries physical cleanup;
* remains bounded;
* may use backoff;
* respects physical safety/affinity.

---

# 57. Cleanup Retry Ownership

Cleanup retry belongs to the physical lifecycle owner/coordinator.

Examples:

```text
Resource Manager
Runtime Artifact Store
Provider Runtime Gateway
Presentation resource owner
```

not Runtime Retry Policy.

---

# 58. Cleanup Escalation

Repeated cleanup failure MAY trigger:

* leak classification;
* provider/plugin degradation;
* isolated process termination;
* Runtime degraded state;
* shutdown containment.

Exact escalation belongs to Error/Runtime policy.

---

# 59. Lifecycle Events

Possible normalized events:

```text
ResourceCreated
ResourceRegistered
ResourceOwnershipTransferStarted
ResourceOwnershipTransferCompleted
ResourcePublished
RetentionAdded
RetentionRemoved
LeaseAcquired
LeaseReleased
ResourceLogicalDisposalStarted
ResourceDraining
ResourcePhysicalDisposalCompleted
ResourceDisposalFailed
ResourceLeakDetected
```

---

# 60. Event Payload

Recommended:

```text
ResourceId
ResourceType
Owner
PreviousOwner?
RetentionKind?
LeaseId?
ScopeType?
ExecutionScopeId?
ExecutionRevisionId?
WorkItemId?
AttemptId?
OccurredAt
ReasonCode?
```

Payload content itself MUST NOT be embedded.

---

# 61. Metrics

Recommended:

```text
active resource count
resource count by class
ownership-transfer count
retention count
active lease count
lease lifetime
logical-disposal count
draining-resource count
physical-disposal count
disposal latency
disposal blocked-by-lease
cleanup retry count
cleanup failure count
resource leak count
native handle count
GPU resource count
provider-runtime resource count
```

---

# 62. Leak Detection

Possible indicators:

```text
lease exceeds expected lifetime
resource drains too long
resource owner released but physical backing remains
provider child operation never releases
native handle count monotonically grows
GPU usage does not fall after expected cleanup
ExecutionScope closes but runtime retention remains
Presentation keeps obsolete resource indefinitely
```

---

# 63. Privacy

Resource lifecycle telemetry MUST NOT contain raw user content.

Sensitive resources SHOULD:

* exist only as long as necessary;
* be cleared when policy requires;
* avoid implicit durable persistence;
* avoid debug retention by default;
* never appear directly in lifecycle events.

---

# 64. Failure Isolation

If one resource disposal fails:

* unrelated resources continue cleanup;
* ownership registry remains valid;
* no new authority is granted;
* no new normal lease is issued;
* failed resource remains accounted/observable;
* Runtime MAY continue if invariants remain safe.

---

# 65. Process Restart

Persisted metadata from a prior process MUST NOT imply a physical resource still exists.

After restart:

```text
old Runtime physical resource
    !=
current live resource
```

Any restored durable representation must create/materialize a new Runtime resource identity/lifecycle where appropriate.

---

# 66. Plugin Resource Boundary

Plugin-provided runtime resources follow the same lifecycle rules.

Plugin lifecycle shutdown MUST:

* stop new capability work;
* drain/cancel active operations;
* release leases;
* dispose plugin-owned physical resources;
* then unload plugin runtime.

Plugin identity does not bypass Resource Lifecycle.

---

# 67. Resource Lifecycle and Cancellation

Cancellation:

```text
revokes execution authority
```

Resource Lifecycle:

```text
drains and disposes physical resources safely
```

Cancellation MUST NOT imply immediate physical disposal.

---

# 68. Resource Lifecycle and Retry

WorkItem Retry:

```text
creates new Attempt
```

Cleanup Retry:

```text
retries physical cleanup
```

These MUST remain separate.

---

# 69. Resource Lifecycle and Cache

Cache:

```text
adds/removes retention
```

Resource Lifecycle:

```text
decides physical disposal eligibility
```

Cache eviction never directly destroys leased/owned payload.

---

# 70. Resource Lifecycle and Presentation

Presentation MAY retain visible resources.

Presentation retention is one retention reason.

Runtime Resource Lifecycle respects it but does not own Presentation semantics.

---

# 71. Resource Lifecycle and Storage

Runtime resource:

```text
execution-time physical resource
```

Durable Storage:

```text
persistent representation
```

Persisting an Artifact does not mean keeping its original Runtime backing alive.

Materializing from Storage creates another Runtime resource lifecycle.

---

# 72. Architecture Invariants

1. Every significant physical resource has one lifecycle owner.

2. Multiple retention reasons MAY exist.

3. Lease does not transfer ownership.

4. Publication does not automatically transfer ownership.

5. Visibility is distinct from ownership.

6. Execution authority is distinct from ownership.

7. Retention is distinct from ownership.

8. Logical disposal precedes physical disposal.

9. Authority loss alone does not make a resource physically disposable.

10. Physical disposal waits for ownership/retention/lease/use eligibility.

11. Cache promotion does not change Runtime Artifact payload owner.

12. Cache promotion does not copy payload by default.

13. Worker disposes Attempt-local resources only.

14. Worker does not independently dispose shared Runtime Artifact.

15. Runtime Artifact Store owns published Runtime Artifact lifecycle.

16. Runtime Control owns execution authority.

17. Business Module owns Business result semantics.

18. Runtime Artifact publication does not imply Business acceptance.

19. Resource in DRAINING remains observable.

20. Physically disposed ResourceId cannot be resurrected.

21. Ownership transfer is atomic or rollback-safe.

22. Candidate resource is not normal shared/reusable state before publication/acceptance.

23. Cleanup failure does not restore execution authority.

24. Cleanup failure does not revive resource.

25. Native/GPU resources use explicit lifecycle.

26. Resource events contain no payload content.

27. Retention release never breaks active valid lease.

28. ExecutionScope shutdown revokes execution authority before physical cleanup.

29. Application shutdown stops admission before destructive cleanup.

30. Physical disposal is coordinated by the owning resource boundary.

31. Resource accounting remains bounded/observable.

32. Lease lifetime is bounded or leak-detectable.

33. Storage does not manage Runtime leases.

34. Runtime Artifact Store does not replace Storage.

35. Provider Management does not own provider physical runtime resources.

36. Provider Runtime Gateway / Adapter owns provider-runtime lifecycle.

37. ExecutionScope/ExecutionRevision terminology is canonical.

38. Resource state dimensions SHOULD remain orthogonal.

39. Cleanup Retry and WorkItem Retry remain separate.

40. Materialization from durable Storage creates a new Runtime lifecycle.

---

# 73. Recommended MVP

CRAI MVP SHOULD support:

* explicit lifecycle owner;
* explicit retention records;
* ResourceLease;
* process-local lifecycle registry;
* Runtime Artifact Store ownership;
* Worker Attempt-local ownership;
* ExecutionScope/ExecutionRevision retention;
* ownership transfer;
* publication boundary;
* logical/physical disposal split;
* draining state;
* bounded cleanup retry;
* no resurrection;
* explicit native/GPU cleanup;
* provider runtime lifecycle;
* Presentation retention;
* content-free lifecycle events;
* leak detection.

MVP SHOULD avoid:

* general-purpose distributed resource graph engine;
* automatic hard disposal while leased;
* implicit resurrection;
* GC-only cleanup of native/GPU resources.

---

# 74. Testing Requirements

Tests SHOULD include:

* create/register;
* ownership transfer success;
* ownership transfer rollback;
* publish without transfer;
* double publication;
* duplicate registration;
* lease acquire/release;
* disposal with active lease;
* late lease release;
* cache promotion;
* cache eviction;
* Business acceptance after Artifact publication;
* ExecutionRevision drain;
* ExecutionScope shutdown;
* Application shutdown;
* provider runtime unload;
* local model unload with active lease;
* native disposal failure;
* GPU cleanup;
* abandoned provider request;
* double physical disposal;
* resurrection attempt;
* cleanup retry;
* leak detection;
* event privacy;
* shared Runtime Artifact reuse;
* durable Storage materialization;
* process restart;
* plugin runtime disposal.

---

# 75. Open Decisions

The following remain open:

* exact Resource lifecycle registry implementation;
* ResourceLease representation;
* ownership registry representation;
* retention-record representation;
* ownership-transfer protocol;
* Artifact publication atomicity model;
* disposal coordinator API;
* logical disposal API;
* native cleanup retry limit;
* lease timeout policy;
* provider runtime isolation;
* GPU resource manager necessity;
* Presentation retention API;
* durable materialization adapter;
* cleanup execution context;
* leak thresholds by resource class;
* draining timeout semantics.

---

# 76. Related Documents

Runtime:

* `MEMORY_MODEL.md`
* `CACHE_POLICY.md`
* `PIPELINE_RUNTIME.md`
* `RUNTIME_COMPONENTS.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`
* `THREADING_MODEL.md`
* `ERROR_MODEL.md`
* `RUNTIME_CONFIG.md`
* `RUNTIME_OBSERVABILITY.md`
* `BOOT_SEQUENCE.md`
* `PROCESS_TOPOLOGY.md`

External:

* `../plugin/PLUGIN_LIFECYCLE.md`
* `../../02-modules/provider-management/`
* `../../02-modules/presentation/`
* `../../02-modules/storage/`

---

# 77. Completion Criteria

`RESOURCE_LIFECYCLE.md` is synchronized when:

* ownership remains the central responsibility model;
* ownership/retention/lease/visibility/authority are distinct;
* Resource lifecycle uses orthogonal state dimensions;
* ExecutionScope/ExecutionRevision terminology is used;
* Provider Runtime ownership replaces Provider Manager physical ownership;
* candidate/publication/Business acceptance boundaries remain distinct;
* Runtime Artifact ownership remains distinct from Business semantics;
* logical/physical disposal remain distinct;
* authority loss does not directly imply physical disposal;
* draining remains explicit;
* cache promotion only adds retention;
* physical disposal eligibility is explicit;
* no-resurrection invariant remains;
* cleanup Retry remains separate from WorkItem Retry;
* events/metrics/tests remain content-safe;
* no Business pipeline-specific disposal order is hard-coded.

---

# 78. Summary

CRAI Runtime Resource Lifecycle follows:

```text
Create
    |
    v
Own
    |
    v
Register
    |
    v
Transfer / Publish
    |
    v
Retain / Lease
    |
    v
Use
    |
    v
Release
    |
    v
Logical Disposal
    |
    v
Drain
    |
    v
Physical Disposal
```

The central ownership model is:

```text
Ownership
    defines responsibility.

Retention
    extends availability.

Lease
    protects active physical use.

Execution Authority
    determines whether execution may still matter.

Visibility
    determines whether resource may be discovered.

Physical Use
    determines whether backing may still be in use.

Disposal
    occurs only when lifecycle dependencies allow it.
```
