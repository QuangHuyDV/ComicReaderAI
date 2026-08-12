# Runtime Memory Model

* **Document:** Runtime Architecture / Memory Model
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how CRAI Runtime manages execution-time resource ownership, lifetime, retention, sharing, resource pressure and physical disposal.

Memory is one dimension of Runtime resource lifecycle.

Runtime resources may include:

```text
Managed Memory
Runtime Artifact Backing
Native Memory
GPU Memory
Provider / Model Runtime Resource
Operating-System Handle
Temporary File / Mapping
Process / IPC Resource
UI / Graphics Resource
```

This document focuses on:

* bounded Runtime memory;
* immutable Runtime Artifacts;
* lightweight references;
* explicit resource ownership;
* scoped leases;
* logical disposal;
* physical disposal;
* Runtime resource pressure;
* Attempt-local and shared resources;
* draining resources;
* privacy-safe retention;
* truthful resource accounting.

---

# 2. Core Philosophy

CRAI follows:

```text
Resource ownership determines lifetime.

Lifetime determines resource pressure.
```

Every significant Runtime resource SHOULD have:

```text
logical owner
lifetime boundary
resource class
size/cost estimate
retention class
release condition
disposal owner
observability metadata
```

Large payloads SHOULD cross Runtime boundaries through:

```text
immutable reference
or
scoped lease
```

rather than repeated copying.

---

# 3. Architectural Position

Canonical Runtime hierarchy:

```text
Application Instance
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
Runtime Resources
```

Runtime Resources MAY include:

```text
Attempt-Local Resource
Shared Runtime Artifact
ExecutionScope-Scoped Resource
Runtime-Global Resource
External / Native Resource
Presentation Resource
Provider Runtime Resource
```

---

# 4. Ownership Boundary

```text
Business Module
    owns semantic meaning.

Runtime
    owns execution-time resource lifecycle.

Runtime Artifact Store
    owns published Runtime Artifact lifecycle.

Resource Manager
    owns physical resource accounting/disposal coordination.

Storage
    owns durable persistence capability.

Presentation/Application
    owns UI/Presentation resource semantics.

Provider Runtime Gateway / Adapter
    owns provider execution-runtime objects.
```

---

# 5. Non-Goals

This document does NOT define:

* Domain persistence schema;
* durable cache file format;
* provider SDK internals;
* Garbage Collector implementation;
* Recognition/OCR algorithms;
* Translation algorithms;
* process topology;
* Business configuration;
* Storage migration policy;
* cache semantic compatibility.

---

# 6. Runtime Resource Categories

## 6.1 Attempt-Local Resource

Exists only for one physical Attempt.

Examples:

```text
temporary buffer
request builder
provider response body
image tile
intermediate tensor
temporary geometry structure
temporary serialization buffer
```

Default owner:

```text
Worker / Execution Adapter
```

---

## 6.2 Shared Runtime Artifact

Immutable execution data that may be referenced across WorkItems/Attempts.

Examples:

```text
Source Runtime Artifact
Recognition Runtime Artifact
Source Document Runtime Artifact
Translation Runtime Artifact
Presentation Input Runtime Artifact
```

These names describe execution payloads.

They are NOT automatically canonical Domain resources.

---

## 6.3 ExecutionScope-Scoped Resource

Runtime resource retained for one ExecutionScope.

Examples MAY include:

```text
ExecutionScope runtime configuration reference
current ExecutionRevision reference
scope cancellation context
scope-local ArtifactRef set
scope resource-accounting state
scope-local execution index
```

Avoid storing business-owned mutable state here.

---

## 6.4 ExecutionRevision-Scoped Resource

Runtime resource retained only while one ExecutionRevision remains relevant/draining.

Examples:

```text
BusinessExecutionPlan reference
WorkItem references
accepted Runtime Artifact references
execution accounting
drain state
```

---

## 6.5 Runtime-Global Resource

Lives for application/runtime lifetime.

Examples:

```text
worker pool
bounded buffer pool
Artifact index infrastructure
configuration snapshot registry
provider runtime registry
shared local model handle
```

---

## 6.6 External / Native Resource

Owned physically by OS, driver, library, external runtime or process.

Examples:

```text
HTTP request
subprocess
capture handle
GPU context
GPU allocation
file mapping
native model context
IPC handle
graphics surface
```

---

# 7. Resource Classification

| Resource                   | Default logical owner                                | Lifetime                           |
| -------------------------- | ---------------------------------------------------- | ---------------------------------- |
| Candidate Runtime Artifact | Producer Attempt                                     | Attempt until transfer/discard     |
| Published Runtime Artifact | Runtime Artifact Store                               | Retention/lease governed           |
| Temporary Buffer           | Worker / Adapter                                     | Attempt                            |
| Queue Metadata             | Work Queue                                           | Queued position                    |
| ExecutionRevision Metadata | Execution State Store                                | Revision lifecycle                 |
| Provider Request           | Execution Adapter                                    | Attempt/child-operation lifetime   |
| Provider Client            | Provider Runtime Gateway / Adapter                   | Runtime/provider-instance lifetime |
| Local Model Handle         | Provider Runtime Gateway + Resource Manager          | Runtime/provider lifetime          |
| GPU Allocation             | Resource-owning runtime component + Resource Manager | Explicit                           |
| Runtime Artifact Lease     | Lease holder                                         | Scoped use                         |
| Presentation Resource      | Presentation/Application                             | Presentation lifetime              |
| Durable Business Snapshot  | Storage / Business owner                             | Outside Runtime resource ownership |

---

# 8. Control Memory

Control memory MUST remain lightweight and bounded.

It MAY contain:

```text
ApplicationInstanceId
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
CancellationContextRef
RuntimeConfigurationSnapshotId
Scheduler metadata
Queue metadata
state references
event envelope
metrics counters
```

It MUST NOT contain:

```text
full screenshot
large source text
large translated text
model tensor
raw provider body
secret
full Artifact payload
```

---

# 9. ExecutionScope Memory

ExecutionScope runtime memory MAY contain:

```text
current ExecutionRevisionRef
Runtime configuration reference
scope cancellation context
scope-local ArtifactRefs
execution accounting
runtime priority metadata
```

It SHOULD NOT own:

```text
Reading Session business state
Glossary truth
Translation Profile semantics
Presentation preferences
Provider Configuration
```

---

# 10. ExecutionRevision Memory

Recommended:

```text
ExecutionRevision
├── ExecutionRevisionMetadata
├── BusinessExecutionPlanRef
├── InputArtifactRefs[]
├── AcceptedRuntimeArtifactRefs[]
├── WorkItemRefs[]
├── ResourceAccounting
└── DrainState
```

ExecutionRevision metadata MUST remain lightweight.

---

# 11. ExecutionRevision Lifetime

Recommended:

```text
CREATED
    |
    v
CURRENT
    |
    +--> SUPERSEDED
    |
    +--> CANCELLED
    |
    v
DRAINING
    |
    v
DISPOSED
```

When authority is lost:

* no new relevant execution should be materialized;
* queued work may be removed;
* running Attempts drain/cancel;
* new leases MAY be denied depending on resource semantics;
* existing valid leases remain protected;
* physical payload is not necessarily released immediately.

---

# 12. Execution State Store

Execution State Store manages Runtime metadata such as:

```text
ExecutionScope identity
ExecutionRevision identity
current/superseded state
ExecutionRevision → WorkItem relation
ExecutionRevision → RuntimeArtifactRef relation
resource accounting metadata
drain/disposal eligibility
```

It is NOT durable Domain Storage.

It does NOT own physical Artifact payload.

---

# 13. Runtime Artifact Store

Runtime Artifact Store owns published immutable Runtime Artifacts.

Responsibilities MAY include:

* Artifact registration;
* Artifact identity;
* atomic publication;
* Artifact metadata;
* retention ownership;
* lease tracking;
* size estimate;
* Runtime lookup;
* backing-resource reference;
* disposal eligibility.

It MUST NOT own:

* Business result semantics;
* cache semantic compatibility;
* Scheduler policy;
* durable Domain truth.

---

# 14. Runtime Artifact Model

Recommended:

```text
RuntimeArtifact
├── artifactId
├── artifactType
├── producerWorkItemId
├── producerAttemptId
├── producerExecutionRevisionId
├── contentIdentity?
├── outputContractVersion?
├── createdAt
├── sizeEstimate
├── retentionClass
├── retentionOwners[]
├── backingResourceRef
└── integrityMetadata
```

Exact implementation remains open.

---

# 15. Runtime Artifact Immutability

Published Runtime Artifact MUST be immutable.

Transformation/correction creates another result:

```text
Artifact A
    |
    v
Transformation
    |
    v
Artifact B
```

Immutability supports:

* safe sharing;
* reduced locking;
* cache identity;
* stale-result safety;
* retry isolation;
* traceability.

---

# 16. Runtime Artifact vs Business Result

Critical distinction:

```text
Runtime Artifact
    = accepted execution payload
```

```text
Business Result
    = owner-module accepted semantic result
```

Runtime Artifact ownership does NOT imply Domain ownership.

---

# 17. Artifact Publication Flow

Recommended:

```text
Worker produces temporary output
        |
        v
Artifact Candidate
        |
        v
Runtime execution-authority validation
        |
        v
Runtime Artifact Store publishes
        |
        v
ArtifactRef
        |
        v
Owning Business Module validates/commits semantics
```

Cache promotion normally occurs only after required Business acceptance.

---

# 18. Candidate Ownership

Before publication:

```text
Producer Attempt
```

owns candidate output.

After successful publication:

```text
Runtime Artifact Store
```

owns Runtime Artifact lifecycle.

Producer MUST release its temporary ownership/reference according to transfer protocol.

---

# 19. Lightweight WorkItem / Queue References

WorkItem/Queue metadata SHOULD contain:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
BusinessStageId
WorkType
InputArtifactRefs[]
RuntimeConfigurationSnapshotId
ExecutionBindingReference?
CancellationContextRef
```

No large payload.

---

# 20. Ownership Model

Every resource MUST have an explicit logical owner.

Possible owners:

```text
Application Bootstrap
Runtime Control
Execution State Store
Runtime Artifact Store
Worker / Attempt
Work Queue
Provider Runtime Gateway
Execution Adapter
Presentation/Application
Resource Manager
Cache Retention
Storage Boundary
```

Ownership transfer MUST be explicit.

---

# 21. Ownership vs Retention

Ownership and retention are related but distinct.

A Runtime Artifact may have one lifecycle owner:

```text
Runtime Artifact Store
```

while multiple retention reasons exist:

```text
ExecutionRevision retention
ExecutionScope retention
Cache retention
Presentation retention
Diagnostic retention
```

---

# 22. Retention Tracking

Implementation MAY use:

* lease table;
* owner set;
* reference count;
* pin count;
* generation token;
* explicit handle;
* managed reference.

Architecture requires only:

```text
Physical disposal MUST NOT occur
while a valid owner, retention or lease remains.
```

---

# 23. Resource Lease

`ResourceLease` is the generic safe-use abstraction.

Possible:

```text
RuntimeArtifactLease
GpuResourceLease
NativeHandleLease
ProviderResourceLease
CaptureSurfaceLease
GraphicsResourceLease
```

Lease SHOULD be:

* scoped;
* read-only where possible;
* associated with owner;
* timestamped;
* releasable;
* cancellation-safe;
* observable;
* leak-detectable.

---

# 24. Lease Acquisition

```text
Resolve ResourceRef
        |
        v
Validate Resource State
        |
        v
Acquire Lease
        |
        v
Use Resource
        |
        v
Release Lease
```

After logical disposal begins, new lease acquisition MAY be denied.

Existing valid leases remain protected until their termination rules permit release.

---

# 25. Logical Disposal

Logical disposal means:

```text
resource is no longer eligible
for new runtime use/retention
```

Possible actions:

* remove active index;
* deny new lease;
* remove retention ownership;
* mark pending physical disposal;
* reject new use through stale/invalid references.

Logical disposal MUST NOT be defined as:

```text
Domain commit revocation
Presentation commit revocation
```

Those belong to their respective owners.

---

# 26. Physical Disposal

Physical disposal occurs only when safe.

Typical conditions:

```text
no active owner
no retention owner
no valid lease
no active physical operation
no Presentation retention
no provider/native use
diagnostic retention expired
cleanup safe
```

Recommended:

```text
Logical Disposal
        |
        v
Drain
        |
        v
No Owners / Retention / Leases
        |
        v
Physical Disposal
```

---

# 27. Retention Classes

Recommended:

```text
EPHEMERAL
ATTEMPT_SCOPED
EXECUTION_REVISION_SCOPED
EXECUTION_SCOPE_SCOPED
CACHE_ELIGIBLE
APPLICATION_SCOPED
EXTERNAL_LIFETIME
```

Retention class describes intended maximum lifetime.

It does NOT replace ownership/lease state.

---

# 28. Attempt-Local Resource

Attempt-local resources:

* belong to Worker/Adapter;
* remain bounded;
* are not shared by default;
* are released at Attempt terminal cleanup;
* do not become shared Runtime Artifact automatically;
* are not retained through Event/Queue references.

---

# 29. Shared Runtime Artifact Retention

Published Runtime Artifact MAY have multiple retention reasons:

```text
ExecutionRevision retention
ExecutionScope retention
Cache retention
Presentation retention
Diagnostic retention
```

Physical payload MAY be shared once where implementation allows.

---

# 30. Cache Promotion

Cache promotion means:

```text
add cache retention
```

not:

```text
copy payload
```

Correct:

```text
Business-accepted reusable result
        |
        v
Cache Policy approves retention
        |
        v
Cache retention added
```

---

# 31. Cache Eviction

Cache eviction means:

```text
remove cache retention
```

It does NOT mean:

```text
free payload immediately
```

Payload remains while any other owner/retention/lease exists.

---

# 32. Runtime Artifact Store vs Cache Policy

```text
Runtime Artifact Store
    owns execution Artifact lifecycle

Cache Policy
    owns reuse-retention decision
```

Artifact Store SHOULD NOT own semantic cache policy.

---

# 33. Runtime Artifact Store vs Storage

```text
Runtime Artifact Store
    -> volatile/runtime Artifact lifecycle
```

```text
Storage
    -> durable persistence
       recovery
       durable retention
       schema/versioning
```

Runtime Artifact Store is not durable by default.

---

# 34. Large Payload Policy

A payload is considered large when copying materially affects:

* latency;
* RAM;
* GPU transfers;
* allocation/GC pressure;
* serialization cost;
* IPC cost.

Default:

```text
reference / lease
```

rather than copy.

---

# 35. Copy Policy

Copy MAY be required for:

* format conversion;
* immutable snapshot;
* process isolation;
* GPU/CPU transfer;
* provider encoding;
* API ownership contract;
* thread-affinity safety;
* security boundary.

Convenience-only copies SHOULD be avoided.

---

# 36. Image Resources

Image processing may involve:

```text
Capture Surface
CPU Buffer
Preprocessed View
Tensor
Preview Surface
Graphics Texture
```

Every representation SHOULD declare:

* owner;
* resource type;
* lifetime;
* size estimate;
* release point;
* sharing policy;
* backing-resource type.

---

# 37. Frame Retention

Runtime SHOULD retain only a bounded set of frames.

Possible MVP guidance:

```text
latest observed frame
previous comparison frame
current stable source Runtime Artifact
small bounded draining set
optional previous displayed presentation resource
```

No unbounded frame history.

---

# 38. Recognition Resources

Recognition execution MAY create:

* preprocessing buffers;
* tensors;
* geometry structures;
* provider responses;
* normalized Runtime Artifacts.

Runtime does not hard-code OCR/Layout internals.

Temporary resources are released after output transfer or Attempt cleanup.

---

# 39. Translation Resources

Translation execution MAY use:

* Source Document reference;
* bounded context;
* request buffer;
* provider response;
* glossary reference;
* normalized Translation Runtime Artifact.

Raw provider request/response is Attempt-local by default.

---

# 40. Presentation Resources

Presentation/Application MAY own:

* Presentation ArtifactRef;
* text/layout model;
* render surface;
* graphics texture;
* font layout cache;
* UI dispatch handle.

Runtime Memory Model only defines resource lifecycle interactions.

Presentation owns visible state semantics.

---

# 41. Previous Presentation Retention

Presentation MAY retain a previous accepted representation until replacement is successfully committed.

Example:

```text
Old accepted presentation visible
        |
        v
New execution processing
        |
        v
New presentation committed
        |
        v
Old Presentation retention released
```

Runtime does not call this “previous Runtime Revision commit ownership”.

---

# 42. Provider Runtime Resources

Provider Runtime Gateway / Execution Adapter MAY own:

* client;
* connection pool;
* loaded model;
* tokenizer;
* process;
* native context;
* reusable buffer pool.

Resource Manager provides physical accounting/pressure coordination.

---

# 43. Provider Request Resources

Attempt-level provider resources MAY include:

* encoded request;
* response body;
* request handle;
* timeout/cancellation state;
* temporary tensor;
* stream buffer.

Release occurs after physical operation permits cleanup.

Logical abandonment does not imply immediate release.

---

# 44. Local Model Residency

Possible runtime residency policies MAY include:

```text
ALWAYS_RESIDENT
EXECUTION_SCOPE_RESIDENT
ON_DEMAND
IDLE_TIMEOUT
```

Policy ownership belongs to:

```text
Provider Runtime / Provider configuration owner
+
Runtime resource configuration
```

Memory Model only requires:

* explicit load/unload;
* cost estimate;
* bounded residency;
* pressure awareness;
* observable lifecycle.

---

# 45. GPU Resources

GPU resources require explicit lifecycle where platform requires it.

Examples:

```text
tensor
model allocation
capture surface
graphics texture
shared graphics handle
```

GC MUST NOT be treated as sufficient timely cleanup.

---

# 46. Native Resources

Examples:

```text
native image handle
model context
capture handle
file mapping
file handle
process handle
IPC handle
```

Managed wrappers MUST expose explicit disposal/lifetime semantics.

---

# 47. Buffer Pooling

Pooling SHOULD be introduced only after profiling.

Risks:

* oversized retention;
* stale sensitive data;
* use-after-return;
* hidden global memory;
* cross-thread misuse.

Pool MUST be:

```text
bounded
observable
explicitly owned
```

---

# 48. Buffer Pool Rules

1. Rented buffer has one temporary owner.

2. Buffer cannot return while referenced/leased.

3. Returned buffer becomes invalid immediately.

4. Sensitive data is cleared when policy requires.

5. Oversized buffers MAY be discarded.

6. Pool capacity is bounded.

7. Pool pressure is observable.

8. Pool MUST NOT become hidden durable/cache storage.

---

# 49. Context Memory Boundary

Business modules MAY construct bounded execution context.

Memory Model only requires bounded resource usage.

It MUST NOT define Translation semantic context composition.

Examples of possible resource limits:

```text
byte count
token estimate
unit count
memory class
```

Exact semantic context belongs to owning module/AI architecture.

---

# 50. Runtime Resource Budget

Recommended:

```text
RuntimeResourceBudget
├── ManagedMemoryBudget
├── NativeMemoryBudget
├── GpuMemoryBudget
├── ArtifactBudget
├── LeaseBudget
├── ProviderRuntimeBudget
├── PresentationResourceBudget
├── TemporaryStorageBudget
└── DiagnosticsBudget
```

Budgets are control limits, not necessarily physical partitions.

---

# 51. Resource Pressure Levels

Recommended:

```text
NORMAL
ELEVATED
HIGH
CRITICAL
```

Transitions SHOULD use hysteresis.

---

# 52. Resource Pressure Ownership

Critical distinction:

```text
Resource Manager
    detects/accounts pressure

Scheduler
    changes admission

Runtime Control
    coordinates execution cancellation/supersession

Cache Policy
    releases cache retention

Provider Runtime Gateway
    unloads eligible runtime resources

Artifact Store / Resource Manager
    performs eligible disposal
```

No single component owns every pressure response.

---

# 53. Pressure Signal Flow

```text
Resource Pressure Detected
        |
        v
Resource State Projection
        |
        +--> Scheduler
        |       reduces admission
        |
        +--> Runtime Control
        |       may cancel obsolete work
        |
        +--> Cache Policy
        |       releases low-value retention
        |
        +--> Provider Runtime Gateway
        |       may unload idle resources
        |
        v
Eligible Physical Disposal
```

---

# 54. Pressure Response Guidance

Possible ordered response:

1. stop speculative work;

2. stop cache warming;

3. expire/release low-value cache retention;

4. stop Background/Maintenance admission;

5. remove/supersede obsolete execution;

6. reduce expensive concurrency;

7. unload eligible idle provider/model runtime;

8. reject non-critical admission;

9. cancel expensive obsolete execution;

10. fail current work safely only when Runtime invariants cannot otherwise be preserved.

Each action remains owned by its authoritative component.

---

# 55. Admission Cost Hints

Work MAY expose estimates such as:

```text
ManagedMemoryCostHint
GpuCostHint
NativeCostHint
ArtifactCostHint
TemporaryStorageCostHint
```

Possible classes:

```text
SMALL
MEDIUM
LARGE
UNKNOWN
```

Hints are estimates, not guarantees.

---

# 56. Memory and Cancellation

Cancellation revokes execution authority before physical resources necessarily disappear.

```text
Authority Revoked
        |
        v
Attempt / Child Operation Draining
        |
        v
Leases Released
        |
        v
Retention Released
        |
        v
Physical Resource Disposal
```

Draining resources remain accounted.

---

# 57. Draining Resource

A draining resource belongs to execution that has lost current authority but is not yet physically releasable.

Metrics SHOULD distinguish:

```text
active
cache-retained
execution-draining
provider-resident
presentation-retained
diagnostic-retained
```

---

# 58. Memory and Retry

Retry preserves compatible shared input references but releases Attempt-local resources.

```text
Attempt 1 ends
        |
        v
Release Attempt-local resources
        |
        v
Retain compatible shared ArtifactRefs
        |
        v
Attempt 2 may be created
```

If Attempt 1 physical operation still drains, truthful resource accounting applies.

---

# 59. Queue Memory

Work Queue stores lightweight metadata and references only.

Queue SHOULD NOT hold long-lived Artifact leases while waiting unless an explicit short bounded protocol requires it.

---

# 60. Event Memory

Events carry lightweight identity/reference data.

Preferred:

```text
AttemptCompleted
├── ExecutionScopeId
├── ExecutionRevisionId
├── WorkItemId
├── AttemptId
└── RuntimeArtifactRef?
```

No embedded large payload.

---

# 61. Diagnostics Memory

Ordinary diagnostics MUST NOT retain:

* screenshot;
* source text;
* translated text;
* Prompt;
* provider body;
* secret.

Sensitive debug content requires explicit bounded/privacy-authorized mode.

---

# 62. Resource Leak

Possible leaks:

```text
managed memory
lease
native handle
GPU allocation
provider runtime resource
event subscription
Presentation retention
queue metadata
process/IPC handle
temporary file
buffer pool
```

---

# 63. Disposal Eligibility

A Runtime resource becomes physically disposable when:

* no valid logical owner remains;
* no retention owner remains;
* no active lease remains;
* no active physical operation remains;
* Presentation no longer retains it;
* cache retention released;
* diagnostic retention expired;
* disposal is safe.

ExecutionRevision authority alone is not sufficient to determine disposal.

---

# 64. Disposal Coordination

```text
Worker
    releases Attempt-local ownership / leases

Runtime Artifact Store
    coordinates published Artifact disposal eligibility

Provider Runtime Gateway / Adapter
    releases provider/runtime-owned resources

Resource Manager
    coordinates physical resource cleanup/accounting
```

Worker MUST NOT independently dispose shared Runtime Artifact backing.

---

# 65. Automatic Memory Management

Automatic GC MAY reclaim ordinary managed objects.

Architecture MUST NOT rely on GC for timely cleanup of:

```text
native resources
GPU resources
file mappings
process handles
capture surfaces
provider handles
pooled buffers
graphics resources
```

---

# 66. Metrics

Runtime SHOULD measure:

```text
process memory
managed heap estimate
native memory
GPU memory
active Runtime Artifact memory
cache-retained memory
Attempt-local memory
provider-resident memory
draining memory
Presentation-retained memory
temporary-storage use
Runtime Artifact count
active lease count
lease lifetime
native-handle count
GPU allocation count
queue metadata memory
disposal latency
resource admission rejection
pressure state
```

---

# 67. Size Accounting

Large resources SHOULD have approximate size/cost sufficient for:

* admission;
* eviction;
* diagnostics;
* profiling;
* capacity planning.

Exact byte-perfect accounting is not required for MVP.

---

# 68. Diagnostics Questions

Resource diagnostics SHOULD answer:

```text
which resource class consumes most?
which ExecutionRevision retains resources?
which owner/retention keeps them alive?
which leases exceed expected lifetime?
which provider runtime retains memory?
how much resource is draining?
which resource exceeded intended lifetime?
how much is held by cache / Presentation / diagnostics?
```

---

# 69. Privacy

Sensitive resource lifetime SHOULD be minimized.

Runtime SHOULD:

* avoid implicit disk persistence;
* avoid content-bearing crash dumps where configurable;
* clear pooled buffers when required;
* avoid raw payload in telemetry;
* release source images as early as safe;
* defer durable persistence to Storage/Business policy.

---

# 70. MVP Resource Policy

CRAI MVP SHOULD use:

```text
process-local Runtime Artifact Store
bounded Work Queues
low bounded worker concurrency
one current ExecutionRevision per lineage
small bounded draining ExecutionRevision set
bounded memory cache
explicit native/GPU cleanup
no implicit durable Artifact cache
no custom pooling before profiling
truthful provider/model residency accounting
```

---

# 71. MVP Retention Guidance

| Resource                               | MVP retention                                     |
| -------------------------------------- | ------------------------------------------------- |
| Observation frame                      | latest + previous comparison where needed         |
| Current stable source Runtime Artifact | current required                                  |
| Previous displayed presentation        | at most one by default                            |
| Draining ExecutionRevision             | small bounded count                               |
| Shared Runtime Artifact                | bounded by ownership/lease/cache                  |
| Background Artifact                    | disabled or strictly bounded                      |
| Debug content                          | disabled by default                               |
| Local model                            | tightly bounded / one large class unless profiled |
| Provider response                      | Attempt-local only                                |

---

# 72. Example — Normal Execution

```text
Source Runtime Artifact available
        |
        v
Worker acquires ResourceLease
        |
        v
Attempt-local buffers created
        |
        v
Candidate output produced
        |
        v
Execution authority validated
        |
        v
Runtime Artifact Store publishes
        |
        v
Worker releases lease/temp resources
        |
        v
Business Module validates semantic result
```

---

# 73. Example — Rapid ExecutionRevision Replacement

```text
ExecutionRevision A CURRENT
        |
        v
ExecutionRevision B created
        |
        v
A execution authority revoked
        |
        v
new work for A stops
        |
        v
A running Attempt drains
        |
        v
leases released
        |
        v
A retention released
        |
        v
physical disposal when safe
```

---

# 74. Example — Shared Runtime Artifact

```text
ExecutionRevision A ─┐
                     ├── RuntimeArtifactRef X
ExecutionRevision B ─┘
```

One physical payload MAY satisfy both references.

---

# 75. Example — Cache Eviction with Active Lease

```text
Cache retention removed
        |
        v
Worker lease still active
        |
        v
payload remains
        |
        v
lease released
        |
        v
no owner/retention remains
        |
        v
physical disposal
```

---

# 76. Example — Critical Resource Pressure

```text
Pressure = CRITICAL
        |
        +--> Scheduler stops non-critical admission
        |
        +--> Cache Policy releases low-value retention
        |
        +--> Runtime Control cancels obsolete execution
        |
        +--> Provider Runtime unloads eligible idle resources
        |
        v
Resource Manager disposes eligible resources
```

---

# 77. Architecture Invariants

1. Large payloads do not travel through Work Queue.

2. Published Runtime Artifacts are immutable.

3. Every significant Runtime resource has explicit logical ownership.

4. Ownership transfer is explicit.

5. Retention tracking is explicit.

6. ResourceLease is scoped and observable.

7. Physical disposal waits for ownership/retention/lease eligibility.

8. Logical disposal and physical disposal are distinct.

9. Scheduler does not own payload.

10. Work Queue does not own payload.

11. Worker owns Attempt-local resource unless ownership is explicitly transferred.

12. ExecutionRevision is not Cache ownership.

13. Cache promotion normally adds retention rather than copying payload.

14. Cache eviction does not invalidate active lease.

15. Runtime Artifact Store and Storage remain separate.

16. Runtime Artifact and Business Result remain separate.

17. Native/GPU resources have explicit lifecycle.

18. GC does not guarantee timely native/GPU cleanup.

19. Runtime resource budgets are bounded.

20. Resource Manager does not decide Business failure.

21. Scheduler owns admission reaction to resource pressure.

22. Runtime Control owns execution-authority cancellation/supersession.

23. Cache Policy owns cache-retention release.

24. Provider Runtime owns provider/model unload decisions.

25. Cancellation revokes execution authority before physical disposal.

26. Retry preserves compatible shared input and releases Attempt-local resource.

27. Draining resources remain accounted.

28. Presentation retention is bounded and Presentation-owned.

29. ExecutionRevision history/resource retention is bounded.

30. Diagnostics do not retain user content by default.

31. Resource leaks are observable.

32. Lease lifetime cannot be unbounded without detection.

33. Provider capacity reflects physical reality.

34. Cleanup failure does not revive revoked execution.

35. Runtime remains correct if cache retention is completely removed.

36. ExecutionScope/ExecutionRevision terminology is canonical.

37. ReadingSession business state is not Runtime memory ownership.

38. Provider Management is not the Runtime physical resource owner.

39. Runtime Artifact publication does not imply Business commit.

40. Physical resource disposal never depends solely on execution freshness.

---

# 78. Recommended MVP

CRAI MVP SHOULD support:

* explicit Runtime resource ownership;
* ExecutionScope/ExecutionRevision resource scopes;
* immutable Runtime Artifact;
* RuntimeArtifactRef;
* ResourceLease;
* process-local Artifact Store;
* explicit ownership transfer;
* logical/physical disposal separation;
* bounded RAM/native/GPU budgets;
* cache retention;
* Presentation retention;
* provider/model residency accounting;
* draining-resource accounting;
* pressure signals;
* explicit native/GPU cleanup;
* leak diagnostics;
* privacy-safe resource telemetry.

MVP MAY defer:

* custom buffer pooling;
* memory-mapped Artifact backing;
* distributed Artifact Store;
* remote lease protocol;
* GPU memory defragmentation;
* sophisticated adaptive budgets;
* cross-process shared-memory Artifact transport.

---

# 79. Open Decisions

The following remain open:

* exact ResourceLease implementation;
* Artifact Store retention representation;
* Resource Manager API;
* ownership-transfer protocol;
* backing-resource abstraction;
* RAM/native/GPU default budgets;
* ExecutionScope retention policy;
* ExecutionRevision drain limits;
* provider/model residency policy;
* Presentation previous-resource retention;
* capture surface backing;
* memory-mapped backing;
* process-isolated resource ownership;
* lease leak threshold;
* hard vs soft resource limits;
* resource cleanup retry;
* minimum device RAM/GPU requirements.

---

# 80. Related Documents

Runtime:

* `PIPELINE_RUNTIME.md`
* `RUNTIME_COMPONENTS.md`
* `WORK_QUEUE.md`
* `SCHEDULER.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`
* `CACHE_POLICY.md`
* `RESOURCE_LIFECYCLE.md`
* `THREADING_MODEL.md`
* `PERFORMANCE_MODEL.md`
* `RUNTIME_CONFIG.md`
* `RUNTIME_OBSERVABILITY.md`

External:

* `../plugin/PLUGIN_LIFECYCLE.md`
* `../../02-modules/provider-management/`
* `../../02-modules/presentation/`
* `../../02-modules/storage/`

---

# 81. Completion Criteria

`MEMORY_MODEL.md` is synchronized when:

* Runtime uses ExecutionScope/ExecutionRevision terminology;
* ReadingSession business state is excluded from Runtime memory ownership;
* Execution State Store and Runtime Artifact Store remain distinct;
* Runtime Artifact and Business Result remain distinct;
* Provider Runtime resources are separated from Provider Management;
* ResourceLease remains generic;
* queue/work metadata remains lightweight;
* ownership transfer remains explicit;
* logical/physical disposal remain separate;
* cache promotion does not copy payload by default;
* cache eviction does not imply immediate physical disposal;
* resource pressure ownership is partitioned correctly;
* Runtime resource budgets cover managed/native/GPU/Artifact/lease/provider/Presentation resources;
* Retry/cancellation/drain semantics match Runtime v2;
* native/GPU cleanup remains explicit;
* diagnostics/privacy remain bounded and content-safe.

---

# 82. Summary

CRAI Runtime Memory Model follows:

```text
Explicit Resource Owner
        |
        v
Defined Lifetime
        |
        v
Immutable Artifact or Scoped Resource
        |
        v
Lightweight Reference / Lease
        |
        v
Bounded Retention
        |
        v
Logical Disposal
        |
        v
Physical Disposal When Safe
```

The central rule is:

```text
Ownership determines lifetime.

Authority determines whether execution may still matter.

Retention determines how long a reusable resource stays available.

Leases protect active physical use.

These are related but separate concepts.
```
