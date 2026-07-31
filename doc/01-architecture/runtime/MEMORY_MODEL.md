# Runtime Memory Model

> Project: CRAI  
> Version: 0.1  
> Status: Architecture Draft

---

## 1. Purpose

This document defines how CRAI allocates, references, retains, shares, limits, and releases runtime memory.

CRAI continuously processes screen images, OCR data, layout structures, translation results, and presentation models.

Without an explicit memory model, the runtime may:

- retain obsolete screenshots
- duplicate large image buffers
- keep canceled revisions alive
- accumulate provider responses
- allow queues to retain large payloads
- exhaust RAM or GPU memory during continuous reading

The memory model ensures that runtime data remains bounded and that obsolete resources can be released safely.

---

## 2. Scope

This document covers:

- runtime memory categories
- revision-scoped memory
- artifact ownership
- lightweight queue references
- memory budgets
- reference lifetime
- shared immutable data
- buffer allocation and reuse
- memory-pressure behavior
- CPU and GPU memory boundaries
- provider-related memory
- UI memory
- cleanup and disposal
- memory observability
- MVP memory policy

This document does not define:

- persistent database schemas
- disk cache formats
- exact programming-language garbage collector behavior
- provider SDK internals
- operating-system virtual-memory configuration
- detailed image-processing algorithms

Those concerns belong to implementation or feature-specific documents.

---

## 3. Memory Goals

The runtime memory model must:

- keep total memory usage bounded
- avoid unnecessary payload copies
- release obsolete revision data promptly
- support cancellation safely
- preserve immutable processing inputs
- separate transient memory from reusable artifacts
- protect active user-visible work
- respond to memory pressure before failure
- remain observable
- remain practical for desktop implementation

---

## 4. Memory Philosophy

CRAI follows this core rule:

> Large runtime data should have one clear owner and be shared through immutable references.

The runtime should avoid flows such as:

```text
Captured Image
    ↓ copy
OCR Queue Image
    ↓ copy
OCR Worker Image
    ↓ copy
Layout Input
    ↓ copy
Presentation Input
```

Instead, CRAI should use:

```text
Revision Store
    └── Image Artifact

Work Queue
    └── RevisionId + ArtifactId

OCR Worker
    └── Read-only artifact reference
```

This reduces:

- memory duplication
- allocation overhead
- synchronization complexity
- accidental mutation
- garbage-collection pressure

---

## 5. Memory Categories

CRAI runtime memory is divided into logical categories.

```text
Application Memory
├── Control Memory
├── Session Memory
├── Revision Memory
├── Artifact Memory
├── Worker Memory
├── Provider Memory
├── Cache Memory
├── UI Memory
└── Diagnostics Memory
```

Each category has different ownership and retention rules.

---

## 6. Control Memory

Control memory contains lightweight runtime coordination data.

Examples:

- session identifiers
- revision identifiers
- WorkItems
- cancellation tokens
- scheduler metadata
- queue metadata
- state-machine state
- event envelopes
- metrics counters

Control memory must remain small.

It must not contain:

- full screenshots
- raw model tensors
- large OCR payload copies
- complete provider request bodies unless unavoidable

---

## 7. Session Memory

Session memory contains data required for one active reading session.

Examples:

- selected screen region
- source configuration
- language configuration
- active provider selection
- current revision reference
- session-scoped glossary references
- active presentation reference
- session cancellation scope

Session memory exists from:

```text
Session Created
    ↓
Session Active
    ↓
Session Closed
```

Closing the session revokes ownership of all revision-scoped runtime memory.

Reusable artifacts may survive only if cache policy explicitly retains them.

---

## 8. Revision Memory

Each source revision owns a revision-scoped memory boundary.

Conceptually:

```text
Revision
├── Revision Metadata
├── Source Image Artifact
├── OCR Artifact
├── Layout Artifact
├── Translation Artifacts
├── Presentation Artifact
└── Processing Metadata
```

Revision memory is logically isolated from other revisions.

A worker processing Revision 42 must not mutate Revision 41 or Revision 43.

---

## 9. Revision Memory Lifecycle

```text
Revision Created
    ↓
Source Artifact Registered
    ↓
Pipeline Artifacts Added
    ↓
Revision Becomes Current or Obsolete
    ↓
Active References Released
    ↓
Reusable Artifacts Promoted to Cache, if allowed
    ↓
Revision Memory Disposed
```

A revision becoming obsolete does not always mean immediate physical deallocation.

Memory can only be released after no active worker safely requires it.

However, obsolete revisions immediately lose:

- UI commit permission
- downstream scheduling permission
- active-session priority

---

## 10. Artifact-Based Memory Model

Runtime outputs are represented as immutable artifacts.

Possible artifact types include:

```text
SourceImageArtifact
ImageFingerprintArtifact
PreprocessedImageArtifact
OCRArtifact
LayoutArtifact
TranslationUnitArtifact
TranslationArtifact
PresentationArtifact
```

Each artifact has:

```text
Artifact
├── ArtifactId
├── ArtifactType
├── ContentIdentity
├── ProducerVersion
├── CreatedAt
├── SizeEstimate
├── OwnershipScope
├── RetentionClass
└── PayloadReference
```

The exact structure is implementation-specific.

---

## 11. Artifact Immutability

Once published, an artifact must not be modified.

A change produces a new artifact.

Incorrect:

```text
OCRArtifact
    ↓ mutate text
Same OCRArtifact
```

Correct:

```text
OCRArtifact v1
    ↓ correction
OCRArtifact v2
```

Immutability allows:

- safe sharing between workers
- deterministic cache keys
- simpler cancellation
- reliable stale-result checks
- reduced locking

---

## 12. Artifact Identity and Memory Identity

Artifact identity and physical memory identity are not necessarily the same.

Two revisions may reference the same artifact:

```text
Revision 100
    └── OCR Artifact A

Revision 101
    └── OCR Artifact A
```

This is valid when both revisions resolve to the same compatible content fingerprint.

The artifact payload should exist only once in memory where practical.

---

## 13. Lightweight Runtime References

Queues and events should store lightweight references.

Preferred WorkItem:

```text
WorkItem
├── SessionId
├── RevisionId
├── InputArtifactIds
├── RequestedOutputType
├── Stage
├── AttemptId
└── CancellationHandle
```

Avoid:

```text
WorkItem
├── Full Screenshot
├── Complete OCR Result
├── Complete Translation Context
└── Presentation Payload
```

Large payloads should be resolved through the revision or artifact store.

---

## 14. Revision Store

The Revision Store tracks active revision ownership.

It should provide operations conceptually equivalent to:

```text
CreateRevision
GetRevision
AttachArtifact
GetArtifactReference
MarkObsolete
ReleaseRevision
```

The Revision Store is not necessarily persistent storage.

For the MVP, it may be an in-memory runtime component.

---

## 15. Artifact Store

The Artifact Store manages immutable runtime artifacts.

It may support:

- artifact registration
- artifact lookup
- shared references
- size accounting
- retention classification
- cache promotion
- eviction
- disposal

The Artifact Store should not decide pipeline scheduling.

It provides artifact availability and ownership information to runtime components.

---

## 16. Revision Store and Artifact Store Relationship

The two concepts have different responsibilities.

### Revision Store

Answers:

> Which artifacts belong to or are referenced by this revision?

### Artifact Store

Answers:

> Which immutable artifacts currently exist and how are their resources retained?

Example:

```text
Revision Store

Revision 42
├── SourceArtifact: IMG-A
├── OCRArtifact: OCR-B
└── LayoutArtifact: LAYOUT-C
```

```text
Artifact Store

IMG-A
OCR-B
LAYOUT-C
```

When Revision 42 is disposed, artifacts with no remaining owner or cache retention may also be disposed.

---

## 17. Ownership Model

Every memory resource must have one explicit logical owner.

Possible owners:

- application
- session
- revision
- artifact store
- cache
- worker
- provider adapter
- UI

Ownership may be transferred.

Example:

```text
Capture Worker creates image buffer
    ↓
Registers SourceImageArtifact
    ↓
Artifact Store accepts ownership
    ↓
Capture Worker releases local ownership
```

After transfer, the Capture Worker must not independently dispose the shared payload.

---

## 18. Reference Model

Artifacts may be referenced by multiple components.

Example:

```text
SourceImageArtifact
├── Revision Store reference
├── OCR Worker reference
└── Diagnostics metadata reference
```

The implementation may use:

- reference counting
- ownership handles
- scoped leases
- garbage-collected references
- explicit resource handles

The architecture does not require one particular mechanism.

It requires that release behavior remain deterministic enough to prevent unbounded retention.

---

## 19. Artifact Lease

For resources requiring explicit lifecycle control, workers should obtain a temporary lease.

Conceptually:

```text
Acquire Artifact Lease
    ↓
Read Artifact
    ↓
Process
    ↓
Release Lease
```

While an active lease exists, the underlying payload cannot be physically disposed.

A lease must be:

- scoped
- read-only by default
- cancel-safe
- releasable more than once without corruption where practical

---

## 20. Logical Disposal and Physical Disposal

CRAI distinguishes between logical and physical disposal.

### Logical Disposal

The resource is no longer valid for new runtime work.

Actions:

- remove from active indexes
- deny new leases
- revoke commit authority
- mark as pending disposal

### Physical Disposal

The actual memory resource is released after all active leases finish.

Example:

```text
Revision becomes obsolete
    ↓
Artifact logically released
    ↓
OCR worker still holds lease
    ↓
Worker finishes or cancels
    ↓
Lease released
    ↓
Artifact physically disposed
```

This prevents use-after-free behavior.

---

## 21. Memory Retention Classes

Artifacts should declare a retention class.

Suggested classes:

### Ephemeral

Exists only during one operation.

Examples:

- temporary resized image
- intermediate tensor
- transient request buffer

### Revision Scoped

Exists while its revision remains active or safely draining.

Examples:

- captured frame
- OCR result
- layout graph

### Session Scoped

May be reused within one session.

Examples:

- repeated page fingerprint
- current-session translation artifact
- session glossary index

### Cache Eligible

May survive revision or session disposal according to cache policy.

Examples:

- validated OCR artifact
- validated translation artifact

### Application Scoped

Exists for the lifetime of the application.

Examples:

- loaded model handle
- provider client
- reusable buffer pool
- shared configuration

Retention class does not replace ownership.

It describes the intended maximum lifetime.

---

## 22. Large Payload Definition

A payload should be treated as large when copying it may meaningfully affect:

- latency
- memory consumption
- garbage collection
- GPU transfer cost

Examples:

- screenshots
- decoded images
- image-processing matrices
- OCR model tensors
- local AI model buffers
- long provider responses
- rendered image surfaces

Large payloads should not be copied by default.

---

## 23. Image Memory

Image processing may create multiple representations:

```text
Captured GPU Surface
    ↓
CPU Image Buffer
    ↓
Preprocessed Image
    ↓
OCR Input Tensor
    ↓
UI Preview Surface
```

The runtime must avoid retaining all representations longer than necessary.

Each representation requires:

- clear owner
- retention class
- estimated size
- release point

---

## 24. Image Copy Policy

A new image copy is allowed only when required by:

- API ownership constraints
- format conversion
- safe thread or process boundary
- immutable snapshot creation
- GPU-to-CPU transfer
- provider encoding

Copies made for convenience should be avoided.

Where possible, use:

- views
- slices
- read-only references
- shared surfaces
- pooled buffers

---

## 25. Source Frame Retention

CRAI does not need to retain every captured frame.

Suggested behavior:

```text
Latest Observed Frame
Previous Comparison Frame
Current Stable Revision Frame
```

Intermediate unstable frames should be released after change detection unless required for diagnostics.

For the MVP, retain at most:

- one previous comparison frame
- one latest observation frame
- one current stable source artifact
- limited draining artifacts from canceled work

---

## 26. Frame Deduplication

When two frames have the same validated fingerprint:

```text
Frame A
Frame B
```

the runtime should avoid creating duplicate long-lived source artifacts.

Possible behavior:

```text
Frame B
    ↓
Fingerprint matches Artifact A
    ↓
Reuse Artifact A
```

The runtime may still create new revision metadata if the source timeline requires it.

---

## 27. OCR Memory

OCR memory may include:

- preprocessed images
- provider request payloads
- model input tensors
- model output tensors
- bounding boxes
- confidence data
- recognized text

Temporary tensors and preprocessing buffers should be released immediately after producing a validated OCR artifact.

The final OCR artifact should contain only data required by downstream stages.

---

## 28. Layout Memory

Layout artifacts may contain:

- text regions
- geometry
- reading-order graph
- semantic classification
- adjacency relationships

Layout implementations should avoid retaining:

- temporary image-processing matrices
- full duplicated OCR payloads
- debug visualizations by default

Instead, layout artifacts should reference source OCR artifact identifiers where possible.

---

## 29. Translation Memory

Runtime translation memory may include:

- translation units
- neighboring context
- provider request payload
- provider response
- glossary snapshot
- translated units
- confidence and metadata

Provider request and response payloads should be released after producing the normalized translation artifact unless diagnostics policy explicitly permits temporary retention.

---

## 30. Presentation Memory

Presentation memory may include:

- presentation model
- original and translated text references
- geometry references
- UI layout structures
- rendered surfaces
- font-layout caches

Only the current presentation should normally remain attached to the active UI.

Previous presentation may be retained temporarily to prevent visual blanking while a new revision is processed.

---

## 31. Previous Presentation Retention

To minimize reading interruption, the UI may keep the previous valid presentation visible until the replacement is ready.

Conceptually:

```text
Presentation Revision 20 visible
    ↓
Revision 21 processing
    ↓
Presentation Revision 21 committed atomically
    ↓
Presentation Revision 20 released
```

The previous presentation must not be interpreted as belonging to the new revision.

The UI should distinguish:

- currently displayed presentation
- currently processing revision

---

## 32. Worker-Local Memory

Workers may own temporary memory during execution.

Examples:

- intermediate arrays
- temporary strings
- request builders
- decompression buffers
- image tiles

Worker-local memory must:

- not be published before completion
- be released on completion, cancellation, or failure
- not be retained accidentally by event handlers
- remain within stage-specific limits

---

## 33. Buffer Pooling

Frequently allocated buffers may use pooling.

Suitable candidates:

- screen capture buffers
- image conversion buffers
- network encoding buffers
- reusable byte arrays

Pooling may reduce:

- allocation rate
- garbage-collection pressure
- fragmentation

However, pooling introduces risks:

- retained oversized buffers
- stale private data
- use-after-return
- cross-thread misuse

Pooling should only be introduced after profiling identifies meaningful allocation pressure.

---

## 34. Buffer Pool Rules

If pooling is used:

1. A rented buffer has one temporary owner.
2. A buffer must not be returned while referenced.
3. A returned buffer must be considered invalid immediately.
4. Sensitive contents should be cleared when required.
5. Oversized buffers may be discarded rather than pooled.
6. Pool capacity must be bounded.
7. Pool metrics must be observable.

The MVP may begin without custom pooling if the selected platform already provides efficient buffer management.

---

## 35. String Memory

OCR and translation may produce many repeated strings.

Potential repeated values include:

- language identifiers
- provider identifiers
- model identifiers
- character names
- glossary terms

The runtime should avoid unnecessary duplicated normalized text but should not introduce complex string interning prematurely.

Normalization should occur once when building canonical artifacts.

---

## 36. Context Memory

Translation context may grow continuously if every previous page is retained.

CRAI must use bounded context.

Possible context sources:

- current page
- neighboring segments
- recent character names
- glossary
- short rolling session summary

The runtime must not pass the complete reading history into every translation request by default.

---

## 37. Context Window Policy

For the MVP, translation context should use:

- the current translation unit
- nearby units from the same revision
- bounded session glossary
- bounded recent-name context

Context limits must be expressed by:

- number of units
- number of characters
- provider token estimate
- memory size

The exact values belong to translation configuration.

---

## 38. Cache Memory

Cache memory is managed separately from active revision memory.

Cache entries may be evicted even while the application remains active.

Active processing must use an acquired artifact lease rather than assuming a cache entry will remain present.

Cache retention must not prevent active revision disposal indefinitely.

---

## 39. Cache Promotion

A revision-scoped artifact may become cache eligible.

Conceptually:

```text
Revision Artifact
    ↓
Validation
    ↓
Cache Policy Check
    ↓
Promote Retention Ownership to Cache
```

Promotion should not require duplicating the payload.

Instead, ownership or retention metadata should change.

Canceled, failed, partial, or invalid artifacts must not be promoted under the MVP policy.

---

## 40. Cache Eviction and Active References

Evicting an artifact means:

```text
Remove cache retention
```

It does not mean:

```text
Immediately free memory still used by a worker
```

Physical disposal occurs only after:

- cache ownership is released
- revision ownership is released
- active leases are released
- UI ownership is released

---

## 41. Provider Memory

Provider adapters may retain:

- HTTP connection pools
- request buffers
- model handles
- tokenizers
- local model weights
- GPU contexts

Provider-scoped memory must be distinguished from per-request memory.

### Provider Lifetime Memory

Examples:

- reusable HTTP client
- loaded OCR model
- loaded translation model

### Request Lifetime Memory

Examples:

- encoded image request
- prompt
- response body
- temporary inference tensor

Request-lifetime memory must be released after completion or cancellation.

---

## 42. Local Model Memory

Local AI models may consume significant RAM or GPU memory.

Model loading should be explicit.

Possible states:

```text
UNLOADED
LOADING
READY
IDLE
UNLOADING
FAILED
```

The runtime must know the approximate memory cost before loading a local model.

Large models should not be loaded speculatively under memory pressure.

---

## 43. Model Residency Policy

Possible model residency modes:

### Always Resident

Keep the model loaded for low latency.

Suitable when:

- memory cost is acceptable
- feature is used continuously

### Session Resident

Load at session start and unload after session end or idle timeout.

### On Demand

Load for each use and release afterward.

Suitable only when startup cost is acceptable.

The MVP should select one simple policy per provider based on measured startup and memory costs.

---

## 44. GPU Memory

GPU memory must be managed separately from general RAM.

GPU-related resources may include:

- capture surfaces
- OCR tensors
- translation model tensors
- UI textures
- shared graphics resources

GPU memory pressure may occur even when system RAM remains available.

The runtime must not assume garbage collection will promptly release native GPU resources.

---

## 45. GPU Resource Rules

GPU resources should:

- use explicit disposal when required
- remain owned by a clear component
- avoid unnecessary GPU-to-CPU copies
- avoid retaining obsolete tensors
- respect model-specific concurrency limits
- be released on provider unload
- be monitored separately from RAM

Local GPU processing is optional for the MVP and must not be required by the architecture.

---

## 46. Native and Managed Memory

Depending on implementation technology, CRAI may use:

- managed memory
- native memory
- GPU memory
- memory-mapped data
- operating-system capture surfaces

Managed object release does not necessarily release native resources.

Wrappers around native resources must define explicit disposal behavior.

Examples:

- image handles
- native OCR contexts
- GPU tensors
- window capture surfaces
- file mappings

---

## 47. Memory Budget Model

The runtime should operate under configurable memory budgets.

Conceptually:

```text
Total Runtime Budget
├── Active Revision Budget
├── Worker Temporary Budget
├── Cache Budget
├── Provider Budget
├── UI Budget
└── Diagnostics Budget
```

These budgets are not strict physical partitions.

They are control limits used for:

- admission decisions
- cache eviction
- concurrency reduction
- provider selection
- cleanup prioritization

---

## 48. Memory Budget Levels

The runtime should recognize pressure levels.

### Normal

Memory use remains comfortably below the configured budget.

### Elevated

Memory use is increasing and non-critical retention should be reduced.

### High

Background work and cache retention should be restricted.

### Critical

Obsolete work must be canceled and new expensive work may be rejected.

Conceptually:

```text
NORMAL
    ↓
ELEVATED
    ↓
HIGH
    ↓
CRITICAL
```

Transitions should use hysteresis to avoid rapid state switching.

---

## 49. Memory Pressure Response

When memory pressure increases, apply actions in this order:

1. stop speculative work
2. stop cache warming
3. evict expired cache artifacts
4. evict least valuable cache artifacts
5. dispose obsolete revisions
6. cancel background pipelines
7. reduce worker concurrency
8. unload idle local models
9. reject new non-critical work
10. fail the current pipeline safely if stability cannot be preserved

The active revision and UI control path receive the highest protection.

---

## 50. Critical Memory Behavior

At critical pressure, CRAI must avoid uncontrolled continuation.

Possible behavior:

```text
Critical Memory Pressure
    ↓
Pause new expensive admissions
    ↓
Cancel obsolete and background work
    ↓
Evict cache
    ↓
Release inactive providers
    ↓
Attempt current-revision recovery
```

If pressure remains unsafe:

```text
Fail active processing gracefully
    ↓
Keep UI responsive
    ↓
Inform user
    ↓
Allow retry after resources recover
```

The runtime must not intentionally continue until process termination by out-of-memory failure.

---

## 51. Memory Admission Control

Before starting memory-intensive work, the Scheduler may request a memory admission decision.

Example:

```text
OCR WorkItem
    ↓
Estimated temporary memory: 180 MB
    ↓
Budget available?
```

Possible outcomes:

- admit
- defer
- use lower-cost provider
- reduce image resolution
- evict cache
- reject safely

Exact estimates may be approximate.

---

## 52. Cost Hints

WorkItems may include memory cost hints.

Examples:

```text
SMALL
MEDIUM
LARGE
UNKNOWN
```

or an estimated byte range.

Cost hints improve scheduling but must not be trusted as exact guarantees.

Runtime metrics should later refine estimates.

---

## 53. Memory and Cancellation

Cancellation immediately revokes the work's logical value.

It does not always release physical memory immediately.

Example:

```text
Translation canceled
    ↓
Provider request still completing
    ↓
Request memory remains temporarily allocated
    ↓
Late result rejected
    ↓
Request resources released
```

The memory model must account for draining canceled work.

---

## 54. Draining Memory

Draining memory belongs to work that has been canceled but has not completed physical cleanup.

It must be tracked separately.

Metrics should distinguish:

- active memory
- cached memory
- draining memory
- provider-resident memory

A large amount of draining memory may indicate:

- poor provider cancellation
- insufficient checkpoints
- leaked leases
- cleanup failure

---

## 55. Memory and Retry

A retry must not retain the complete failed attempt unless explicitly needed.

Correct behavior:

```text
Attempt 1 fails
    ↓
Release attempt-local memory
    ↓
Create Attempt 2
```

Shared immutable input artifacts may remain.

Failed provider responses should not be retained by default.

---

## 56. UI Thread Memory

The UI thread should receive only data required for rendering.

Avoid dispatching:

- full mutable pipeline state
- unnecessary raw images
- provider responses
- model tensors

UI dispatch should use:

- PresentationArtifactId
- immutable presentation model
- minimal update metadata

---

## 57. Event Memory

Events should carry lightweight metadata.

Preferred:

```text
translation.completed
├── SessionId
├── RevisionId
├── AttemptId
└── TranslationArtifactId
```

Avoid:

```text
translation.completed
└── Entire translation payload
```

Large event payloads create hidden memory copies and long-lived references.

---

## 58. Diagnostics Memory

Diagnostics may accidentally retain large private payloads.

By default, diagnostics must not retain:

- screenshots
- OCR text
- translation text
- full provider request bodies
- full provider responses

Debug capture of such data must be:

- explicitly enabled
- bounded
- short-lived
- privacy-aware
- clearly separated from standard logging

---

## 59. Memory Leaks

Common leak risks include:

- event subscriptions not removed
- cancellation sources retained after completion
- queue items referencing disposed sessions
- UI models retaining old revisions
- provider callbacks capturing large payloads
- cache entries without eviction
- worker tasks stored in global collections
- native image handles not disposed
- GPU tensors retained after cancellation
- unbounded translation context
- debug logs retaining source content

These risks must be covered by lifecycle tests and profiling.

---

## 60. Disposal Trigger

A revision becomes eligible for disposal when:

- it is no longer current
- no active work may commit for it
- all queued items are removed or invalidated
- active workers have released their leases
- UI no longer displays its presentation
- cache promotion decisions are complete

Disposal eligibility should be observable.

---

## 61. Disposal Coordination

The Revision Store or dedicated Resource Lifecycle component should coordinate disposal.

Individual workers must not independently dispose shared revision artifacts.

Workers only release their own leases and temporary resources.

Conceptually:

```text
Worker releases lease
    ↓
Artifact Store updates reference state
    ↓
No owner remains
    ↓
Artifact disposed
```

---

## 62. Automatic Memory Management

Garbage collection may manage ordinary objects, but architecture must not rely on it for timely cleanup of large or native resources.

Explicit cleanup remains required for:

- native image buffers
- GPU resources
- provider handles
- file mappings
- operating-system capture resources
- pooled buffers
- child processes

---

## 63. Memory Metrics

The runtime should expose:

- total process memory
- managed heap estimate
- native memory estimate
- GPU memory estimate where available
- active revision memory
- cache memory
- worker temporary memory
- provider-resident memory
- draining memory
- artifact count by type
- artifact size by type
- active lease count
- cache eviction count
- revision disposal latency
- model load memory
- memory-pressure state

Measurements may be approximate, but trends must be observable.

---

## 64. Artifact Size Accounting

Every large artifact should provide an estimated memory size.

Examples:

```text
SourceImageArtifact: width × height × bytes per pixel
OCRArtifact: text + regions + metadata
TranslationArtifact: text + context metadata
PresentationArtifact: model + optional render surfaces
```

Accounting does not need perfect byte-level accuracy for the MVP.

It must be accurate enough to guide:

- eviction
- diagnostics
- admission control
- profiling

---

## 65. Memory Diagnostics

When memory exceeds expected limits, diagnostics should answer:

- Which artifact types consume the most memory?
- How many revisions remain retained?
- Which revisions are obsolete?
- Which workers hold active leases?
- How much memory belongs to cache?
- How much belongs to local models?
- How much belongs to draining canceled work?
- Which resources have exceeded expected lifetime?

---

## 66. Privacy and Memory

Sensitive source content should remain in memory only as long as required.

The runtime should:

- avoid writing memory artifacts to disk implicitly
- avoid crash dumps containing raw content where configurable
- clear sensitive pooled buffers when necessary
- avoid sending raw memory data to telemetry
- dispose source images promptly after their usefulness ends

Local-first privacy does not remove the need for memory-retention controls.

---

## 67. MVP Memory Policy

The first implementation should use a simple bounded model.

### 67.1 Active Revisions

Retain:

- current revision
- previous displayed revision, if needed
- limited draining obsolete revisions

Do not retain an unbounded revision history in runtime memory.

### 67.2 Queue Payload

Queues contain only lightweight references.

### 67.3 Artifact Storage

Use an in-memory Artifact Store.

Artifacts are immutable after publication.

### 67.4 Cache

Use a bounded memory cache.

No persistent artifact cache is required initially.

### 67.5 Images

Keep no more than the frames required for:

- current comparison
- stable current revision
- active OCR
- currently displayed presentation when needed

### 67.6 Worker Concurrency

Use low concurrency until profiling proves additional workers are beneficial.

### 67.7 Local Models

Do not load multiple large local models simultaneously by default.

### 67.8 Disposal

Dispose obsolete revision artifacts after active leases and UI ownership are released.

---

## 68. Suggested MVP Retention Limits

Initial assumptions may be:

| Resource | Suggested retention |
|---|---|
| Observation frames | Latest and previous frame |
| Current stable revision | 1 |
| Previous displayed revision | At most 1 |
| Draining obsolete revisions | Small bounded count |
| OCR artifacts | Bounded by memory cache |
| Layout artifacts | Bounded by memory cache |
| Translation artifacts | Bounded by memory cache |
| Presentation artifacts | Current and optional previous |
| Background artifacts | Disabled initially |
| Debug screenshots | Disabled by default |

Exact numeric memory limits must be selected after initial technology and device profiling.

---

## 69. Example: Normal Revision Processing

```text
Frame captured
    ↓
SourceImageArtifact created
    ↓
Revision 90 references image artifact
    ↓
OCR worker acquires image lease
    ↓
OCRArtifact published
    ↓
OCR worker releases temporary buffers and image lease
    ↓
Layout worker references OCRArtifact
    ↓
TranslationArtifact published
    ↓
PresentationArtifact committed
```

After commit, unnecessary intermediate temporary resources are already released.

---

## 70. Example: Rapid Scrolling

```text
Revision 100 created
    ↓
OCR running

Revision 101 created
    ↓
Revision 100 logically canceled
    ↓
No new leases allowed for Revision 100
    ↓
OCR worker releases lease after cancellation
    ↓
Revision 100 artifacts disposed
```

Revision 101 proceeds without waiting for Revision 100 to update the UI.

---

## 71. Example: Shared Artifact Reuse

```text
Revision 110 source fingerprint = HASH-A
Revision 111 source fingerprint = HASH-A
```

Artifact Store contains:

```text
SourceImageArtifact HASH-A
OCRArtifact HASH-A + OCR Config
```

Revision 111 may reference existing compatible artifacts instead of allocating duplicates.

---

## 72. Example: Cache Eviction

```text
Memory Cache reaches budget
    ↓
Least valuable artifact selected
    ↓
Cache ownership released
```

If no other owner exists:

```text
Artifact physically disposed
```

If a worker still has a lease:

```text
Artifact remains until lease release
```

---

## 73. Example: Memory Pressure During Translation

```text
Memory pressure becomes HIGH
    ↓
Background work stopped
    ↓
Expired artifacts evicted
    ↓
Obsolete revisions disposed
    ↓
Translation concurrency reduced
```

The current revision remains protected when possible.

---

## 74. Example: Local Model Pressure

```text
OCR model resident
Translation model requested
    ↓
Combined memory exceeds safe budget
```

Possible decision:

```text
Keep OCR model
Use remote translation provider
```

or:

```text
Unload idle OCR model
Load translation model
```

The Scheduler and Provider Manager coordinate the decision using memory information.

---

## 75. Architecture Invariants

The memory model must preserve the following invariants:

1. Large queue payloads are forbidden by default.
2. Published artifacts are immutable.
3. Every large resource has one explicit logical owner.
4. Shared artifacts are accessed through references or leases.
5. Obsolete revisions lose commit permission immediately.
6. Physical disposal waits for active leases to end.
7. Cache eviction does not invalidate active leases.
8. Worker-local temporary memory is released after terminal completion.
9. UI retains only current or explicitly permitted previous presentation data.
10. Runtime revision history is bounded.
11. Canceled work cannot retain memory indefinitely without being observable.
12. Native and GPU resources use explicit lifecycle management where required.
13. Diagnostics do not retain raw private content by default.
14. Memory pressure can reduce or reject runtime work.
15. The application must remain correct when every cache entry is evicted.

---

## 76. Testing Requirements

Memory tests should cover:

- repeated frame capture
- rapid revision replacement
- cancellation during OCR
- cancellation during translation
- stale provider result
- cache promotion
- cache eviction with active lease
- session close during active processing
- previous presentation replacement
- local model loading and unloading
- provider timeout
- repeated retries
- event subscription cleanup
- native-resource disposal
- memory-pressure transitions
- bounded revision retention
- artifact reuse across matching fingerprints

Long-running tests should verify that memory stabilizes rather than increasing continuously.

---

## 77. Profiling Requirements

Before setting final memory budgets, profile:

- image size at common screen resolutions
- source image duplication count
- OCR peak memory
- local model resident memory
- translation context size
- presentation rendering memory
- cancellation cleanup latency
- cache hit and retention behavior
- GPU resource release
- long-session memory trend

Profiling results belong in:

```text
doc/20-performance/
```

---

## 78. Open Questions

The following questions remain open:

- Which implementation language and desktop framework will be selected?
- Does the capture API produce CPU or GPU-backed frames?
- Can capture surfaces be shared without copying?
- Which OCR providers require native buffers?
- Will local translation models be supported in the MVP?
- Should Artifact Store use reference counting, leases, or platform-managed references?
- What is the minimum supported device RAM?
- What memory budget should be used by default?
- Should the previous presentation remain visible during new processing?
- Which artifacts should become persistent in later versions?
- How should large EPUB or PDF imports use streaming?
- Should local AI workers run in isolated processes?

These questions do not block the initial memory architecture.

---

## 79. Related Documents

- `README.md`
- `PIPELINE_RUNTIME.md`
- `WORK_QUEUE.md`
- `SCHEDULER.md`
- `CANCELLATION.md`
- `CACHE_POLICY.md`
- `THREADING_MODEL.md`
- `RESOURCE_LIFECYCLE.md`
- `PERFORMANCE_MODEL.md`
- `../DATA_FLOW.md`
- `../STATE_MACHINE.md`
- `../flows/SCREEN_COMIC_FLOW.md`

---

## 80. Next Step

The next runtime document should be:

```text
THREADING_MODEL.md
```

It should define:

- UI execution boundary
- capture execution context
- scheduler execution context
- worker pools
- CPU-bound and I/O-bound work
- provider request concurrency
- thread-affine resources
- event dispatch behavior
- synchronization rules
- blocking restrictions
- process-isolation possibilities

---

## 81. Summary

CRAI uses a revision-scoped, artifact-based memory model.

The practical runtime model is:

```text
One Logical Owner
    ↓
Immutable Artifact
    ↓
Lightweight References
    ↓
Scoped Leases
    ↓
Bounded Retention
    ↓
Logical Disposal
    ↓
Physical Disposal When Safe
```

The initial implementation should remain conservative:

- low worker concurrency
- bounded in-memory artifacts
- no unbounded revision history
- no large queue payloads
- no persistent cache requirement
- explicit cleanup for native resources
- memory-pressure handling before failure

More advanced pooling, adaptive budgets, GPU sharing, and persistent artifact reuse should only be introduced after real profiling demonstrates their value.