# Capture Module Specification

> **Project:** CRAI
> **Module:** `capture`
> **Path:** `doc/02-modules/capture/MODULE.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Module Definition

Capture is the CRAI processing module responsible for acquiring source data through a Capture Provider and normalizing that data into a platform-independent Candidate Capture Result.

Its primary responsibility is:

```text
Capture Request
    +
Capture Source
    +
Runtime Execution Context
    +
Capture Configuration
        ↓
Capture Operation
        ↓
Capture Provider
        ↓
Raw Provider Result
        ↓
Capture Normalization
        ↓
Candidate Capture Result
        ↓
Runtime Authority Validation
        ↓
Artifact Publication
        ↓
CapturedFrameArtifact
```

Capture answers:

> **How can the requested source be safely acquired and normalized into a valid capture result?**

Capture does not answer:

> Is this asynchronous work still authoritative?

That belongs to Runtime Control.

Capture also does not answer:

> Should OCR, Translation, or Presentation run next?

That belongs to Business Pipeline Orchestration.

---

# 2. Module Identity

```text
Module ID: capture
Module Type: Processing Module
Primary Domain: Source acquisition and capture normalization
Primary Execution Unit: CaptureOperation
Primary Candidate Output: CandidateCaptureResult
Published Output: CapturedFrameArtifact
Runtime Authority: Runtime Control
Artifact Publication Owner: Runtime / Artifact Store
Platform Boundary: CaptureProvider
MVP Priority: Required
```

Capture is a processing module.

It is not:

```text
Business Pipeline Orchestrator
Runtime Scheduler
Artifact Store
Recognition Module
UI Adapter
Platform Capture Implementation
```

---

# 3. Architectural Position

The processing path is:

```text
Reading Domain State
        ↓
Business Pipeline Orchestration
        ↓
Runtime Control
        ↓
Capture WorkItem / Attempt
        ↓
Capture
        ↓
Candidate Capture Result
        ↓
Runtime Completion + Authority Validation
        ↓
CapturedFrameArtifact publication
        ↓
Recognition when required
```

Capture does not directly invoke Recognition.

Recognition receives accepted Artifact references through Runtime-orchestrated flow.

---

# 4. Core Ownership Separation

CRAI separates four responsibilities around Capture.

## 4.1 Capture Semantics

Capture owns:

```text
CaptureSource semantic model
CaptureOperation semantic processing
Capture request validation
Provider capability validation
Capture normalization
Capture-result validation
Capture-specific source lifecycle
Capture-specific health
Capture-specific errors
CandidateCaptureResult
```

## 4.2 Runtime Execution

Runtime Control owns:

```text
RuntimeRevisionId
WorkItem
Attempt
execution authority
cancellation authority
retry execution
deadline enforcement policy
scheduler priority
queue admission
completion acceptance
stale-result rejection
```

## 4.3 Artifact Publication

Runtime Artifact Store owns:

```text
accepted CapturedFrameArtifact publication
accepted Artifact identity
accepted Artifact leases
retention
disposal
cross-module Artifact lifetime
```

## 4.4 Platform Capture

Capture Provider / platform adapter owns:

```text
native capture APIs
browser capture APIs
native source handles
screen/window enumeration
provider callbacks
platform permission APIs
provider-specific raw results
provider-specific errors
```

---

# 5. Central Architecture Rule

```text
Runtime decides whether Capture work still matters.

Capture decides whether acquired data is a valid Capture result.

Artifact Store owns the accepted CapturedFrameArtifact.

Capture Provider owns platform-specific acquisition.
```

---

# 6. Primary Responsibilities

Capture is responsible for:

* validating capture requests;
* resolving CaptureSource semantics;
* validating Capture Provider capabilities;
* obtaining Capture Provider leases/references;
* invoking Capture Provider;
* validating minimum raw provider output;
* normalizing capture data;
* assigning Capture-owned semantic metadata;
* validating Candidate Capture Result;
* reporting Capture-specific health;
* cooperating with Runtime cancellation;
* releasing Capture-owned temporary resources;
* enforcing Capture privacy rules.

---

# 7. Explicit Non-Responsibilities

Capture MUST NOT:

* determine whether source content changed;
* detect page changes;
* detect scrolling;
* detect duplicate visual content;
* detect text regions;
* perform OCR;
* analyze text layout;
* detect speech bubbles;
* normalize recognized text;
* translate text;
* build Presentation state;
* render UI;
* schedule its own Runtime work;
* create WorkItems;
* create Attempts;
* create RuntimeRevisionId;
* decide Runtime retry execution;
* decide global queue priority;
* publish accepted Artifacts itself;
* own long-term reading history.

---

# 8. Capture vs Recognition

Capture owns:

```text
acquisition
source geometry
raw source dimensions
pixel representation normalization
capture timestamp
capture-source metadata
```

Recognition owns:

```text
image preprocessing for recognition
text-region detection
OCR
orientation/text-direction interpretation
layout interpretation
duplicate/content analysis where defined
recognition quality
```

Capture MUST NOT optimize or mutate frame content specifically for OCR.

---

# 9. Capture vs Reading Session

Reading Session owns:

```text
what the user is reading
ReadingSource
ReadingTarget
ReadingContextRevision
reading lifecycle
```

Capture owns:

```text
how a Runtime-authorized capture request
is acquired from a CaptureSource
```

Reading Session does not directly invoke Capture implementation.

Preferred flow:

```text
ReadingContext changed
        ↓
Business Pipeline Orchestration
        ↓
Runtime
        ↓
Capture
```

---

# 10. Capture vs Business Pipeline Orchestration

Business Pipeline Orchestration decides:

```text
Is Capture required?
Can an existing CapturedFrameArtifact be reused?
Is Recognition required afterward?
Should Capture be skipped?
```

Capture does not maintain those dependency rules.

---

# 11. Capture vs Runtime

Runtime invokes Capture through stable processing contracts.

Runtime provides execution context such as:

```text
SessionId
RuntimeRevisionId
WorkItemId
AttemptId
ConfigurationSnapshotRef
CancellationContext
Correlation / Trace Context
```

Capture may use these values for:

* tracing;
* diagnostics;
* cancellation observation;
* Candidate correlation.

Capture does not own them.

---

# 12. CaptureSource

`CaptureSource` represents a logical capturable source.

Conceptually:

```text
CaptureSource
├── captureSourceId
├── sourceKind
├── sourceDescriptor
├── sourceVersion
├── capabilities
└── sourceState
```

Possible source kinds:

```text
ScreenRegion
ApplicationWindow
Display
BrowserConnector
LocalInput
RenderedDocument
```

A CaptureSource is not a native handle.

---

# 13. CaptureSourceId

```text
CaptureSourceId
```

identifies one logical Capture source.

It must remain stable across provider/native-handle replacement when the logical source is still considered the same source.

---

# 14. SourceVersion

`SourceVersion` identifies significant CaptureSource configuration/lifecycle changes.

Examples:

* region changed;
* underlying window re-created;
* source descriptor changed;
* capture capability set changed;
* provider-side logical source re-established.

SourceVersion is Capture-owned.

It is not Runtime execution authority.

A mismatch may make a Candidate semantically invalid for the requested CaptureSource.

Runtime authority remains a separate check.

---

# 15. Source State

Capture may own a small source-local lifecycle such as:

```text
UNINITIALIZED
INITIALIZING
READY
SUSPENDED
UNAVAILABLE
STOPPING
STOPPED
```

Detailed transitions belong to `STATES.md`.

Source state describes CaptureSource availability only.

It does not describe Runtime WorkItem state.

---

# 16. Source Manager

A logical Source Manager may be responsible for:

* creating CaptureSource instances;
* validating source descriptors;
* tracking SourceVersion;
* acquiring provider source resources;
* exposing normalized capability information;
* suspending/resuming CaptureSource;
* replacing platform resources behind the same logical source;
* releasing Capture-owned source references.

It must not become a Reading Session registry.

---

# 17. Native Source Handle Ownership

Native source handles belong behind the Capture Provider/platform boundary.

Conceptually:

```text
Capture
    ↓
Opaque Provider Source Reference
    ↓
CaptureProvider
    ↓
Native Window / Display / Browser Resource
```

Capture public contracts MUST NOT expose:

```text
HWND
CGWindowID
X11 Window
Wayland object
DOM Node
browser tab object
native texture handle
```

---

# 18. Permission Boundary

Capture must enforce permission-related semantic requirements such as:

```text
required capture permission exists
requested scope is permitted
capture region is within granted scope
privacy fallback is permitted
```

Actual permission APIs and prompts belong to:

```text
Capture Provider
Platform Adapter
Application/UI integration
```

Capture MUST NOT directly import operating-system permission APIs.

---

# 19. Permission Rules

Capture must preserve:

1. never widen capture scope automatically;
2. never fall back from window/region to full display without explicit policy/user authorization;
3. stop producing valid Candidates when required permission is revoked;
4. avoid permission-prompt loops;
5. expose normalized permission requirement/error semantics;
6. keep native permission objects outside public contracts.

---

# 20. CaptureOperation

`CaptureOperation` is Capture-owned semantic processing for one Runtime invocation.

Conceptually:

```text
CaptureOperation
├── operationId
├── operationType
├── captureSourceId
├── sourceVersion
├── requestedRegion?
├── captureMode
├── runtimeExecutionIdentity
├── configurationSnapshotRef
└── diagnosticState
```

`CaptureOperation` is not:

```text
WorkItem
Attempt
SchedulerJob
```

---

# 21. Runtime Execution Identity

Conceptually:

```text
RuntimeExecutionIdentity
├── sessionId
├── runtimeRevisionId
├── workItemId
├── attemptId
├── configurationSnapshotRef
├── correlationId?
├── causationId?
└── traceContext?
```

Capture MUST NOT create these IDs.

---

# 22. Removal of Generation-as-Authority

The previous Capture model used:

```text
GenerationId
```

as a primary stale-result authority mechanism.

Runtime v2 already owns execution authority through:

```text
RuntimeRevisionId
WorkItem
Attempt
authority validation
```

Therefore Capture MUST NOT maintain a competing global/session generation authority.

If a Capture-specific generation/version is still required for source semantics, it must be scoped and renamed appropriately, for example:

```text
SourceVersion
CaptureStreamVersion
```

and must not replace Runtime authority.

---

# 23. CaptureRequest

A Capture request conceptually contains:

```text
CaptureRequest
├── captureSourceRef
├── requestedRegion?
├── captureMode
├── captureOptions
├── runtimeExecutionContext
└── configurationSnapshotRef
```

Detailed public schemas belong to `CONTRACT.md`.

---

# 24. Capture Mode

Typical semantic modes:

```text
ON_DEMAND
CONTINUOUS_SAMPLE
PROVIDER_EVENT_TRIGGERED
```

Capture Mode describes how a single Capture capability is requested.

It does not grant Capture permission to create an independent scheduler.

---

# 25. On-Demand Capture

Normal flow:

```text
Runtime invokes Capture
        ↓
Validate Capture request
        ↓
Resolve CaptureSource
        ↓
Acquire provider reference
        ↓
Invoke provider
        ↓
RawCaptureResult
        ↓
Normalize
        ↓
Validate Candidate
        ↓
CandidateCaptureResult
        ↓
Return Completion to Runtime
```

Runtime then determines whether the result may be accepted.

---

# 26. Continuous Capture

Continuous capture must not create a hidden scheduler inside Capture.

Preferred Runtime-v2 model:

```text
Business/Runtime policy decides repeated sampling
        ↓
Runtime schedules Capture WorkItems
        ↓
Capture executes one bounded acquisition per invocation
```

Alternatively, if a provider requires a continuous native stream:

```text
Runtime-authorized stream lifetime
        ↓
CaptureProvider stream
        ↓
bounded samples
        ↓
Capture Candidate results
```

Even then:

* Runtime owns execution authority;
* Capture does not create arbitrary unbounded work;
* every accepted sample remains authority-validated.

---

# 27. Continuous Provider Stream

Some platforms expose capture as a callback stream rather than request/response.

Capture may adapt such providers behind:

```text
CaptureProviderStream
```

but must normalize provider callbacks into bounded Capture results associated with current Runtime execution context.

Provider callback frequency must not define Runtime scheduling policy.

---

# 28. Backpressure

Capture should preserve freshness and bounded memory.

Default domain preference:

```text
Latest relevant capture result
>
large stale backlog
```

However, global queue admission and Runtime scheduling belong to Runtime.

Capture may apply local bounded behavior such as:

* drop obsolete provider callback samples;
* retain only the newest unsubmitted sample;
* refuse a new local acquisition while one source operation is active;
* report pressure to Runtime;
* reduce provider callback buffering.

---

# 29. Backpressure Ownership

Capture may decide:

```text
this local provider sample is obsolete
this local source cannot safely accept concurrent acquisition
local buffer limit exceeded
```

Runtime decides:

```text
whether another WorkItem is created
queue priority
global fairness
retry scheduling
resource admission
```

---

# 30. No Private Capture Queue

Capture MUST NOT own separate global queues such as:

```text
UserTriggeredQueue
ContinuousCaptureQueue
RecoveryQueue
```

when Runtime Work Queue already owns scheduling.

Those may exist as Runtime priority/classes, not Capture-owned queue infrastructure.

---

# 31. Capture Policy

Capture may own semantic policy specific to acquisition behavior.

Examples:

```text
requested capture mode
provider format preference
maximum accepted dimensions
region policy
cursor inclusion
capture privacy policy
source concurrency constraint
```

Capture policy MUST NOT duplicate:

```text
Runtime priority policy
global deadline policy
Runtime retry scheduling
global resource admission
```

---

# 32. Configuration

Capture consumes immutable typed configuration.

Example:

```text
CaptureConfiguration
├── preferredPixelFormat
├── maximumWidth
├── maximumHeight
├── maximumCandidateBytes
├── sourceConcurrencyLimit
├── localSampleBufferLimit
├── includeCursor
├── allowFullDisplayCapture
├── rawFramePersistencePolicy
└── privacyPolicy
```

Capture MUST NOT parse YAML/environment variables directly.

---

# 33. Frame Acquirer

A logical Frame Acquirer:

* invokes CaptureProvider;
* supplies requested source/region;
* cooperates with cancellation;
* receives provider result;
* performs minimum provider-output checks;
* normalizes provider-specific failures;
* releases provider temporary resources.

It does not:

* retry indefinitely;
* schedule Runtime retry;
* call Recognition;
* persist raw frame;
* create private workers.

---

# 34. RawCaptureResult

`RawCaptureResult` is provider-facing transient data.

It may contain provider-native representation internally.

Its lifetime is bounded to:

```text
provider acquisition
    ↓
Capture normalization
```

It MUST NOT cross the public Capture module boundary.

---

# 35. Capture Normalization

Normalization converts provider result into a stable Capture-owned representation.

Typical normalized semantics:

```text
width
height
pixel format
orientation
capture region
coordinate space
DPI / scale metadata
source dimensions
capture timestamp
CaptureSourceId
SourceVersion
```

Runtime execution identity may be attached as provenance, not Capture-owned authority.

---

# 36. What Capture Normalization Must Not Do

Capture normalization does not:

* sharpen;
* denoise for OCR;
* deskew text;
* detect bubbles;
* detect text;
* binarize for Recognition;
* resize purely to improve OCR;
* infer reading order;
* interpret language.

Those responsibilities remain downstream.

---

# 37. CandidateCaptureResult

The primary Capture module result is:

```text
CandidateCaptureResult
├── operationId
├── captureSourceId
├── sourceVersion
├── normalizedFrame
├── captureMetadata
├── runtimeExecutionIdentity
├── warnings[]
└── diagnostics?
```

A Candidate is not an accepted Artifact.

---

# 38. Candidate Validation

Before returning Candidate completion, Capture validates:

* source identity matches request;
* source version compatible;
* dimensions valid;
* pixel format supported;
* buffer exists;
* buffer bounds valid;
* coordinate metadata valid;
* capture region valid;
* privacy scope respected;
* candidate size within configured limits;
* required provider result metadata exists.

Capture MUST NOT determine global Runtime staleness itself.

---

# 39. Runtime Authority Validation

After Capture completes:

```text
CandidateCaptureResult
        ↓
Runtime Completion Validation
        ↓
authority current?
    ├── no → discard
    └── yes
          ↓
       Artifact publication
```

Capture must not publish a Candidate merely because local validation succeeded.

---

# 40. CapturedFrameArtifact

After Runtime accepts completion, an immutable accepted Artifact is published:

```text
CapturedFrameArtifact
├── artifactId
├── contentIdentity
├── captureSourceId
├── sourceVersion
├── frame representation/ref
├── geometry metadata
├── capture metadata
├── provenance
└── contractVersion
```

The exact schema belongs to Capture/Artifact contracts.

---

# 41. Artifact Ownership

Once accepted and published:

```text
CapturedFrameArtifact
```

belongs to Runtime Artifact Store lifetime rules.

Capture does not continue to own the accepted Artifact merely because it created the Candidate.

---

# 42. Frame Memory Ownership

Before publication:

```text
Raw provider data
    → provider/acquirer temporary ownership

Normalized Candidate data
    → Capture operation ownership
```

After accepted Artifact publication:

```text
Artifact payload
    → Artifact Store / Runtime resource ownership
```

This prevents Capture from becoming a second Artifact lifetime manager.

---

# 43. Removal of Frame Lifecycle Manager as Global Owner

The previous `Frame Lifecycle Manager` owned:

```text
retention
reference tracking
stale-frame disposal
cross-module frame lifetime
```

Those responsibilities overlap Runtime Resource Lifecycle and Artifact Store.

In v2, Capture may retain only a:

```text
Capture Temporary Resource Manager
```

for:

* raw provider buffers;
* Candidate-local normalized buffers;
* provider leases;
* source-local transient sample buffers.

Accepted Artifact retention belongs outside Capture.

---

# 44. Temporary Resource Manager

A logical Capture Temporary Resource Manager may:

* release raw provider buffers;
* release discarded Candidate buffers;
* enforce local sample buffer limits;
* release provider leases;
* clean source-local temporary state;
* cooperate with Runtime resource pressure signals.

It does not own accepted Artifact leases globally.

---

# 45. Immutability

Normalized Candidate data must be treated as immutable once handed to Runtime completion boundary.

Accepted `CapturedFrameArtifact` is immutable.

Recognition MUST NOT mutate the shared Artifact in place.

If Recognition requires preprocessing, it creates a separate representation according to Memory Model.

---

# 46. Source Concurrency

MVP default:

```text
maxConcurrentAcquisitionPerSource = 1
```

This is a Capture/provider safety constraint, not Scheduler ownership.

It prevents:

* native handle races;
* duplicate acquisitions;
* provider instability;
* excessive local buffering.

Runtime should respect the declared capability/constraint.

---

# 47. Provider Capability

Normalized capability examples:

```text
SupportsRegionCapture
SupportsWindowCapture
SupportsDisplayCapture
SupportsContinuousStream
SupportsCursorExclusion
SupportsOccludedWindowCapture
SupportsDpiMetadata
SupportsStructuredCapture
SupportsEventTrigger
```

Capture resolves semantic capability from provider declarations.

Public contracts must not expose provider-specific SDK objects.

---

# 48. Provider Boundary

```text
Capture
    ↓
CaptureProvider Contract
    ├── Desktop Screen Provider
    ├── Desktop Window Provider
    ├── Browser Capture Provider
    ├── Local Input Provider
    └── Document Render Provider
```

CaptureProvider owns platform-specific acquisition mechanics.

---

# 49. CaptureProvider Responsibilities

Provider owns:

* native API invocation;
* provider-specific source handle;
* native callback lifecycle;
* provider result extraction;
* provider-specific capability detection;
* platform-specific permission bridge where applicable;
* provider-specific error capture.

Provider does not own:

* Runtime retry;
* pipeline orchestration;
* accepted Artifact publication;
* Recognition.

---

# 50. Retry Semantics

Capture may classify an error as:

```text
retryable
non-retryable
retryable-after-source-refresh
retryable-after-permission-change
```

Capture does not execute Runtime retry policy.

Correct ownership:

```text
Capture
    → classify Capture failure

Runtime Retry Policy
    → decide retry

Scheduler
    → schedule retry execution
```

---

# 51. Hidden Provider Retry

Capture Providers MUST NOT perform unbounded hidden retry.

Provider-local retry is permitted only when:

* bounded;
* required by the platform API;
* transparent to deadline/cancellation semantics;
* observable;
* does not consume Runtime retry budget invisibly.

---

# 52. Cancellation

Runtime owns cancellation authority.

Capture receives a cancellation context and must cooperate.

Capture may check cancellation:

* before provider invocation;
* during long provider operations where supported;
* before normalization;
* before returning Candidate completion.

Capture MUST NOT mutate Runtime Attempt state.

---

# 53. Late Provider Result

If provider cannot hard-cancel:

```text
Runtime cancellation
        ↓
provider continues physically
        ↓
provider returns result
        ↓
Capture may stop local processing if cancellation observed
        ↓
Runtime authority validation remains final protection
```

Capture does not need a competing Generation authority to solve this.

---

# 54. Source Replacement

Source replacement is a CaptureSource semantic operation.

Typical flow:

```text
Replace CaptureSource request
        ↓
validate replacement
        ↓
stop accepting new acquisition on old source
        ↓
release/replace provider source resource
        ↓
increment SourceVersion
        ↓
new source READY
```

Runtime separately decides what existing WorkItems/Attempts become obsolete.

---

# 55. Source Replacement and Authority

A Candidate referencing an incompatible old `SourceVersion` fails Capture semantic validation or Runtime compatibility validation.

This is distinct from:

```text
Runtime Revision superseded
```

Both protections may apply independently.

---

# 56. Health Model

Capture may expose Capture-specific health:

```text
Healthy
Degraded
Unavailable
Recovering
Stopped
```

Health reflects CaptureSource/provider capability.

Examples:

### Healthy

* source available;
* permission usable;
* provider operating;
* latency within expected Capture budget.

### Degraded

* high Capture latency;
* provider callback drops;
* temporary source instability;
* local sample pressure.

### Unavailable

* source lost;
* required permission unavailable;
* provider unavailable.

### Recovering

* source resource being reacquired;
* provider reconnect in progress.

### Stopped

* source explicitly stopped.

---

# 57. Health Does Not Control Runtime

Capture health may be exposed to Runtime/Application.

Capture Health Monitor does not:

* restart Runtime;
* schedule WorkItems;
* change Runtime Revision;
* cancel Attempts;
* alter global queue policy.

---

# 58. Error Ownership

Capture owns errors such as:

```text
InvalidCaptureRequest
UnsupportedSource
SourceUnavailable
PermissionUnavailable
InvalidRegion
ProviderFailure
CaptureTimeoutClassification
InvalidRawResult
InvalidFrameGeometry
NormalizationFailed
SourceVersionMismatch
CandidateInvalid
```

Detailed taxonomy belongs to `ERRORS.md`.

---

# 59. Runtime Errors Are External

Capture does not own:

```text
RuntimeRevisionStale
WorkItemCancelled
AttemptSuperseded
RetryExhausted
SchedulerOverloaded
RuntimeShutdown
```

These may affect Capture execution but remain Runtime-owned outcomes.

---

# 60. Event Ownership

Capture may publish Capture-owned facts only.

Possible facts:

```text
CaptureSourceReady
CaptureSourceChanged
CaptureSourceUnavailable
CaptureSourceStopped
CaptureHealthChanged
```

Processing success associated with Runtime WorkItem completion should not create a competing terminal lifecycle event model.

Detailed events belong to `EVENTS.md`.

---

# 61. No Hidden Event Orchestration

Capture MUST NOT require:

```text
ReadingTargetChanged
    ↓
Capture subscribes
    ↓
Capture starts itself
```

Correct:

```text
Reading domain change
        ↓
Business Pipeline Orchestration
        ↓
Runtime
        ↓
Capture invocation
```

---

# 62. Public Module Surface

Capture should expose a small stable contract boundary, conceptually:

```text
CaptureProcessor
CaptureSourcePort
CaptureCapabilityQuery
CaptureHealthQuery
```

Detailed public commands/data contracts belong to:

```text
CONTRACT.md
```

not `API.md`.

Internal services must not be imported by other modules.

---

# 63. Internal Logical Components

Possible internal responsibilities:

```text
Capture Module
├── Capture Coordinator
├── Capture Source Manager
├── Capture Source Validator
├── Capture Capability Resolver
├── Capture Operation Factory
├── Frame Acquirer
├── Frame Normalizer
├── Capture Result Validator
├── Temporary Resource Manager
├── Capture Health Monitor
└── Capture Diagnostics
```

These names describe responsibilities, not required package layout.

---

# 64. Capture Coordinator

Capture Coordinator may:

* receive Runtime-authorized Capture invocation;
* validate Capture request;
* resolve source;
* resolve provider capability;
* create CaptureOperation;
* invoke Frame Acquirer;
* invoke Frame Normalizer;
* validate Candidate;
* return Candidate completion;
* coordinate Capture-owned cleanup.

It MUST NOT:

* create Runtime WorkItem;
* create Attempt;
* submit arbitrary Scheduler jobs;
* implement Runtime retry;
* publish accepted Artifact;
* invoke Recognition.

---

# 65. Capture Operation Factory

Creates Capture-owned operation state from:

```text
CaptureRequest
+
RuntimeExecutionContext
+
CaptureSource
+
CaptureConfiguration
```

It does not create Runtime job identity.

---

# 66. Capture Result Validator

Validator checks Capture-specific invariants only.

Examples:

* valid dimensions;
* supported format;
* correct CaptureSourceId;
* compatible SourceVersion;
* valid capture region;
* valid coordinate space;
* privacy policy respected;
* normalized representation valid.

It does not decide:

```text
RuntimeRevision is current
Attempt is authoritative
```

---

# 67. Threading Model

Capture owns no global thread pool.

Runtime controls processing execution.

CaptureProvider may internally require:

* native callback thread;
* OS capture thread;
* platform async runtime;
* device-specific worker.

Those details remain behind CaptureProvider.

---

# 68. Scheduler Boundary

Capture must not call the global Scheduler merely to run its normal processing operation.

Runtime has already scheduled/invoked the WorkItem/Attempt.

If a Capture Provider requires internal asynchronous callbacks, those are provider mechanics, not Runtime scheduling.

---

# 69. Resource Pressure

Runtime may expose resource pressure.

Capture may respond locally by:

* reducing temporary provider buffering;
* declining optional expensive normalization;
* reporting degraded health;
* returning a classified resource failure.

Capture does not independently change global Runtime scheduling.

---

# 70. Privacy

Capture handles especially sensitive data.

Required rules:

1. capture only explicitly authorized source scope;
2. never silently widen source scope;
3. never automatically fall back to full display;
4. raw provider data remains temporary by default;
5. accepted frame persistence is disabled by default unless explicit feature/policy permits it;
6. raw image bytes are never logged;
7. full screenshots are never included in normal diagnostics;
8. native source titles/paths are minimized;
9. remote transmission requires explicit capability/policy;
10. discarded Candidates are released promptly.

---

# 71. Raw Frame Persistence

Default:

```text
Raw captured image content is memory-only.
```

Persistence requires explicit architecture/product policy covering:

* user intent;
* privacy;
* encryption;
* retention;
* deletion;
* storage ownership.

Capture itself does not persist raw frame content.

---

# 72. Diagnostics

Capture diagnostics may include:

```text
operationId
sessionId
runtimeRevisionId
workItemId
attemptId
captureSourceId
sourceVersion
sourceKind
frame dimensions
frame byte size
capture duration
normalization duration
provider name
health state
error code
candidate result size
```

Do not include:

```text
raw frame bytes
screenshots
recognized text
translated text
DOM content
secrets
tokens
full sensitive source titles
```

---

# 73. Observability

Recommended metrics:

```text
capture_operation_total
capture_operation_duration_ms
capture_provider_duration_ms
capture_normalization_duration_ms
capture_candidate_bytes
capture_source_unavailable_total
capture_provider_failure_total
capture_permission_failure_total
capture_local_sample_drop_total
capture_candidate_discard_total
capture_health_state
```

Runtime queue/retry metrics remain Runtime-owned.

---

# 74. Dependency Rules

Capture may depend on stable contracts for:

```text
Runtime execution context
Cancellation context
Configuration snapshot
Capture Provider
Artifact Candidate interface
Geometry primitives
Diagnostics
Common error model
Resource abstractions
```

Capture MUST NOT directly depend on:

```text
Reading Session implementation
Recognition implementation
Translation implementation
Presentation implementation
Scheduler implementation
Work Queue implementation
Storage implementation
Desktop UI implementation
Browser SDK implementation
OCR provider implementation
```

---

# 75. Provider Dependency Rule

Capture core may depend on:

```text
CaptureProvider interface
```

but not concrete:

```text
WindowsCaptureProvider
MacScreenProvider
WaylandProvider
BrowserExtensionSDK
```

Concrete implementations are wired at Composition Root/provider infrastructure.

---

# 76. Browser Connector Boundary

The Browser Connector may eventually be:

```text
CaptureProvider
Application connector
Structured-source adapter
```

This remains an open architecture choice.

Whichever design is selected must preserve:

```text
platform/browser details
outside Capture core contracts
```

---

# 77. Structured Browser Content

If a browser integration can provide structured DOM/text rather than pixels, it should not automatically be forced into `CapturedFrameArtifact`.

The architecture may route structured source through a separate structured-source capability.

Capture should remain focused on acquisition semantics appropriate to the requested source type.

---

# 78. Storage Boundary

Capture does not directly depend on Storage.

Storage may persist:

* source preferences;
* region preferences;
* non-sensitive Capture configuration;

through application-owned persistence flows.

Accepted Artifact persistence follows Artifact/Storage policies.

---

# 79. Interaction with Recognition

Recognition consumes accepted Capture Artifact references when required.

Correct:

```text
Capture Candidate
    ↓
Runtime accepts
    ↓
CapturedFrameArtifact
    ↓
Runtime/Orchestration
    ↓
Recognition
```

Incorrect:

```text
Capture
    ↓
Recognition.execute(frame)
```

---

# 80. Interaction with Presentation/UI

UI may allow the user to select a capture source or region.

Correct separation:

```text
UI Adapter
    → user interaction / platform surface

Application
    → normalized capture-source selection intent

Capture
    → validates CaptureSource / region semantics

CaptureProvider
    → native source resource
```

Presentation does not own source-selection UI merely because it displays translated output.

UI Adapter/Application owns actual interaction.

---

# 81. MVP Scope

Required:

```text
ScreenRegion CaptureSource
On-Demand Capture
limited provider-stream support if platform requires it
single active acquisition per source
immutable normalized Candidate result
Runtime authority-aware completion
SourceVersion validation
memory-only temporary raw provider data
bounded local sample buffering
CaptureProvider abstraction
permission-safe acquisition
privacy-safe diagnostics
fake provider testing
```

---

# 82. Deferred Scope

Deferred:

```text
automatic source discovery
automatic reading-area detection
multi-source Capture coordination
GPU frame pools
zero-copy sharing
adaptive capture resolution
remote Capture source
capture recording
frame replay
advanced multi-monitor coordination
browser DOM-specific capture
```

Automatic reading-area detection does not become Capture-owned merely because it is listed as future functionality.

Its final owner must be decided separately.

---

# 83. Architecture Risks

## 83.1 Capture Scope Expansion

Do not move into Capture:

* duplicate detection;
* image preprocessing for OCR;
* text-region detection;
* scroll detection;
* page-change detection;
* content classification.

---

## 83.2 Runtime Duplication

Do not reintroduce:

```text
Capture Job
Capture private Scheduler
Capture retry loop
Session Generation authority
Capture global queue
```

when Runtime already owns those concerns.

---

## 83.3 Platform Leakage

Never allow native types through public contracts.

---

## 83.4 Artifact Ownership Leakage

Do not let Capture retain accepted Artifacts indefinitely.

Accepted Artifact lifetime belongs to Artifact Store.

---

## 83.5 Hidden Provider Retry

Provider retries must remain bounded and observable.

---

## 83.6 Full-Screen Privacy Fallback

Never automatically expand Capture scope.

---

# 84. Design Trade-offs

## Freshness over Historical Completeness

For interactive reading:

```text
fresh relevant capture
>
processing every historical sample
```

This applies to local Capture sampling behavior.

Global scheduling remains Runtime-owned.

## Simplicity over Zero-Copy

MVP prioritizes:

* clear ownership;
* deterministic cleanup;
* fake-provider testability;
* platform independence.

## Explicit Source over Automatic Discovery

MVP requires deliberate source selection.

## Runtime Scheduling over Private Workers

Capture performs bounded operations under Runtime control.

---

# 85. Testing Strategy

Capture must be testable without:

```text
real screen capture
real browser
Recognition
Translation
native UI
Storage backend
```

---

# 86. Unit Tests

Test:

* request validation;
* CaptureSource validation;
* SourceVersion logic;
* capability resolution;
* provider result normalization;
* coordinate metadata;
* Candidate validation;
* privacy scope;
* source concurrency rule;
* error classification;
* cancellation observation;
* temporary resource release.

---

# 87. Runtime Integration Tests

Verify:

* Capture never creates WorkItem;
* Capture never creates Attempt;
* Capture never creates RuntimeRevisionId;
* Capture never schedules Runtime retry;
* Runtime authority rejection prevents Artifact publication;
* canceled Attempt cannot publish accepted Capture Artifact;
* late provider result cannot bypass Runtime completion validation.

---

# 88. Provider Integration Tests

Test:

* provider success;
* provider failure;
* permission denied;
* source lost;
* provider callback stream;
* unsupported region;
* source replacement;
* provider late completion;
* provider cleanup.

---

# 89. Resource Tests

Verify:

* raw provider buffers released;
* discarded Candidate released;
* accepted Artifact ownership transferred correctly;
* Capture does not dispose Artifact while leased downstream;
* source provider resources released exactly once;
* local buffering remains bounded.

---

# 90. Privacy Tests

Verify:

* no full-display fallback without authorization;
* requested region cannot escape authorized scope;
* raw bytes never logged;
* Candidate discarded promptly after authority rejection;
* sensitive source metadata is redacted in diagnostics;
* raw content is memory-only by default.

---

# 91. Concurrency Tests

Test races between:

* source replacement and capture completion;
* permission revocation and capture;
* cancellation and provider callback;
* Runtime supersession and Candidate completion;
* two acquisitions for one source;
* provider stream shutdown and Runtime cancellation.

---

# 92. Architecture Invariants

1. Capture only acquires and normalizes source data.

2. Capture does not interpret textual content.

3. Capture does not perform Recognition.

4. Capture does not perform Translation.

5. Capture does not build Presentation.

6. Capture does not decide pipeline topology.

7. Capture does not own RuntimeRevisionId.

8. Capture does not own WorkItem lifecycle.

9. Capture does not own Attempt lifecycle.

10. Capture does not own Runtime cancellation authority.

11. Capture does not own Runtime retry execution.

12. Capture does not own global Scheduler queues.

13. CaptureOperation is not a Runtime WorkItem.

14. SourceVersion is not Runtime authority.

15. CandidateCaptureResult is not an accepted Artifact.

16. Runtime validates authority before Artifact publication.

17. Accepted CapturedFrameArtifact lifetime belongs to Artifact Store.

18. Capture owns only temporary Candidate/raw resources.

19. Raw provider result never crosses public module boundary.

20. Accepted frame data is immutable.

21. Provider-specific types never cross public contracts.

22. Native source handles remain behind provider boundary.

23. Capture scope is never widened silently.

24. Full-display fallback requires explicit authorization.

25. Capture does not persist raw frame by default.

26. Capture does not log raw frame content.

27. Source concurrency remains bounded.

28. Local provider buffering remains bounded.

29. Hidden infinite provider retry is forbidden.

30. Capture events do not become hidden workflow orchestration.

31. Reading Session does not directly invoke Capture implementation.

32. Recognition does not receive unpublished Capture Candidates.

33. Runtime execution state and Capture source state remain separate.

34. Diagnostics remain privacy-safe.

---

# 93. Example — On-Demand Screen Region Capture

```text
Business Pipeline Orchestration
        ↓
Runtime WorkItem
        ↓
Attempt
        ↓
Capture Request
        ↓
Capture validates ScreenRegion source
        ↓
CaptureProvider acquires image
        ↓
Raw provider result
        ↓
Capture normalization
        ↓
CandidateCaptureResult
        ↓
Return Attempt completion
        ↓
Runtime authority validation
        ↓
ACCEPT
        ↓
CapturedFrameArtifact publication
        ↓
Recognition may consume Artifact
```

---

# 94. Example — Runtime Revision Superseded

```text
Runtime Revision 20
        ↓
Capture operation begins
        ↓
Runtime Revision 21 becomes current
        ↓
provider returns old result
        ↓
Capture locally normalizes or exits early if canceled
        ↓
Candidate completion reaches Runtime
        ↓
authority rejected
        ↓
Candidate discarded
```

Capture does not need to mutate a Session Generation registry.

---

# 95. Example — Source Replacement

```text
CaptureSource S version 4
        ↓
source replaced/reconfigured
        ↓
CaptureSource S version 5
        ↓
old provider result references version 4
        ↓
Capture Candidate validation detects incompatibility
        ↓
candidate rejected
```

Runtime authority checks still apply independently.

---

# 96. Example — Permission Revoked

```text
CaptureSource READY
        ↓
platform permission revoked
        ↓
provider reports normalized permission failure
        ↓
CaptureSource → UNAVAILABLE
        ↓
Capture health changes
        ↓
new Capture requests rejected
```

Reading Session lifecycle is not directly changed.

---

# 97. Example — Provider Callback Stream

```text
Runtime-authorized Capture stream
        ↓
CaptureProvider native callback
        ↓
sample A
sample B
sample C
        ↓
local bounded freshness policy
        ↓
obsolete A/B dropped
        ↓
C normalized
        ↓
Candidate completion
        ↓
Runtime authority validation
```

Capture does not build an unbounded frame queue.

---

# 98. Recommended Implementation Order

```text
1. Capture identifiers and Source contracts
2. CaptureProvider interface
3. Runtime execution boundary
4. CaptureOperation
5. Raw provider result adapter
6. normalization model
7. CandidateCaptureResult
8. Capture result validation
9. SourceVersion semantics
10. temporary resource ownership
11. Runtime completion/publication bridge
12. privacy rules
13. Capture health
14. source replacement
15. provider-stream support
16. advanced CaptureSource types
```

Do not implement continuous Capture orchestration before Runtime scheduling/authority boundaries are correct.

---

# 99. Completion Criteria

Capture is architecturally usable when:

* CaptureSource semantics are stable;
* native source details remain behind CaptureProvider;
* one bounded Capture operation can produce CandidateCaptureResult;
* Candidate result is distinct from accepted Artifact;
* Runtime authority controls publication eligibility;
* accepted CapturedFrameArtifact has clear Artifact Store ownership;
* Capture no longer owns Runtime scheduling;
* Capture no longer owns Runtime retry execution;
* Capture no longer depends on session Generation as execution authority;
* SourceVersion remains Capture-specific semantic versioning;
* raw provider data has bounded lifetime;
* Recognition never receives unpublished Candidate results;
* privacy scope cannot silently expand;
* provider-specific errors are normalized;
* fake-provider tests cover core behavior.

---

# 100. Related Documents

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULE_ROLE.md
.meta/WORKFLOW.md
.meta/CHANGE_RULE.md

doc/01-architecture/core/CAPABILITY_MAP.md
doc/01-architecture/core/DATA_FLOW.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
doc/01-architecture/core/STATE_MACHINE.md

doc/01-architecture/modules/MODULE_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md
doc/01-architecture/modules/OWNERSHIP_MAP.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/MEMORY_MODEL.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/02-modules/capture/README.md
doc/02-modules/capture/CONTRACT.md
doc/02-modules/capture/STATES.md
doc/02-modules/capture/EVENTS.md
doc/02-modules/capture/ERRORS.md

doc/02-modules/reading-session/MODULE.md
doc/02-modules/recognition/MODULE.md
doc/02-modules/presentation/MODULE.md

doc/03-contracts/
doc/04-providers/
```

---

# 101. Documentation Ownership

This file defines:

* Capture module identity;
* Capture ownership;
* Runtime boundary;
* CaptureSource semantics;
* provider boundary;
* Capture operation semantics;
* Candidate Capture Result;
* Artifact publication boundary;
* temporary resource ownership;
* privacy rules;
* major architecture invariants.

Detailed public schemas belong to:

```text
CONTRACT.md
```

Detailed Capture-owned state transitions belong to:

```text
STATES.md
```

Detailed Capture-owned facts belong to:

```text
EVENTS.md
```

Detailed Capture error taxonomy belongs to:

```text
ERRORS.md
```

Runtime scheduling, retry, authority, and Artifact publication belong to Runtime architecture.

---

# 102. Summary

Capture v2 is CRAI's source-acquisition processing boundary.

Its core flow is:

```text
Runtime-authorized Capture invocation
        ↓
CaptureSource
        ↓
CaptureProvider
        ↓
Raw Provider Result
        ↓
Capture Normalization
        ↓
CandidateCaptureResult
        ↓
Runtime Authority Validation
        ↓
CapturedFrameArtifact
        ↓
Downstream Processing
```

The ownership model is:

```text
Business Pipeline Orchestration
    → decides whether Capture is required

Runtime Control
    → owns execution authority

Capture
    → owns acquisition and normalization semantics

CaptureProvider
    → owns platform-native acquisition

Artifact Store
    → owns accepted CapturedFrameArtifact lifetime

Recognition
    → owns interpretation of captured image content
```

The central invariant is:

```text
Capture may produce a valid image Candidate.

Only Runtime-authoritative work
may publish that Candidate as an accepted Artifact.

Only downstream modules
may interpret its content.
```
