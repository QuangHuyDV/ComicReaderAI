# Capture Contract

> **Project:** CRAI
> **Module:** `capture`
> **Path:** `doc/02-modules/capture/CONTRACT.md`
> **Contract Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the public contract boundary of the Capture module.

Capture acquires source data through a platform/provider boundary and normalizes that data into a Capture-owned Candidate result.

The primary execution contract is:

```text id="g5twxr"
CaptureInvocation
        ↓
Capture
        ↓
CandidateCaptureResult
        ↓
Runtime Completion / Authority Validation
        ↓
CapturedFrameArtifact publication
```

Capture does not publish accepted Artifacts directly.

This contract exists so that:

* Runtime can invoke Capture without importing Capture internals;
* platform-specific capture implementations remain hidden;
* CaptureSource semantics remain stable across platforms;
* Candidate output remains distinct from accepted Artifact output;
* Runtime authority remains outside Capture;
* Recognition consumes only accepted Capture Artifacts;
* public types remain serializable and implementation-independent.

---

# 2. Contract Scope

This file defines:

```text id="z7y3mh"
Capture identifiers
CaptureSource contracts
Capture source commands
Capture source queries
Capture execution input
CaptureOperation
Capture configuration
Capture capability contracts
CandidateCaptureResult
normalized Capture frame semantics
Capture health
Capture errors/rejections
provider boundary abstractions
versioning
validation rules
```

This file does not define:

```text id="0soogi"
Runtime WorkItem
Runtime Attempt
RuntimeRevision lifecycle
Runtime retry policy
Runtime queue priority
Runtime scheduler policy
Artifact Store implementation
Recognition contract
UI framework objects
native capture APIs
```

---

# 3. Architectural Boundary

The canonical flow is:

```text id="9osouj"
Business Pipeline Orchestration
        ↓
Runtime Control
        ↓
Capture WorkItem / Attempt
        ↓
CaptureInvocation
        ↓
Capture
        ↓
CandidateCaptureResult
        ↓
Runtime Completion Validation
        ↓
Artifact Store
        ↓
CapturedFrameArtifact
        ↓
Recognition when required
```

Capture public contracts end before accepted Artifact publication.

---

# 4. Contract Principles

## 4.1 Serializable Boundary

All public Capture values MUST be serializable.

Public values MUST NOT contain:

```text id="w538ej"
HWND
CGWindowID
X11 Window
Wayland object
DOM Node
browser tab object
native texture handle
provider SDK object
mutable native buffer pointer
callback function
thread-affine UI object
```

---

## 4.2 Runtime Ownership

Runtime owns:

```text id="ja8a7g"
RuntimeRevisionId
WorkItemId
AttemptId
Scheduler priority
Runtime deadline policy
Runtime cancellation authority
Runtime retry execution
completion acceptance
```

Capture may receive references to these concepts.

Capture does not create or mutate them.

---

## 4.3 Candidate vs Accepted Artifact

Capture returns:

```text id="r39txj"
CandidateCaptureResult
```

not:

```text id="ybh5kz"
accepted CapturedFrameArtifact
```

Only Runtime-authoritative completion may be published as an accepted Artifact.

---

## 4.4 Capture-Owned Versioning

Capture may own:

```text id="i4owuq"
SourceVersion
CaptureStreamVersion?
```

for Capture semantics.

These values MUST NOT replace Runtime execution authority.

---

# 5. Public Service Surface

Conceptually, Capture exposes:

```text id="7g3r79"
CaptureProcessor
CaptureSourceService
CaptureCapabilityQuery
CaptureHealthQuery
```

Concrete interface names may vary by implementation.

Other modules MUST NOT access internal components such as:

```text id="fyj99s"
FrameAcquirer
FrameNormalizer
TemporaryResourceManager
CaptureCoordinator
Provider Registry internals
```

---

# 6. CaptureSourceId

```text id="o3wndv"
CaptureSourceId
- value
```

Identifies one logical Capture source.

Rules:

* stable for one logical source identity;
* independent from native platform handle;
* immutable;
* not equal to ReadingTargetId;
* not equal to Runtime WorkItem identity.

---

# 7. SourceVersion

```text id="cxmw50"
SourceVersion
- value
```

Represents significant changes to a logical CaptureSource.

Typical increments:

```text id="wwl65k"
source configuration changes
region changes
provider-native source re-created
source capability set changes
logical native association changes
```

Rules:

1. scoped to one CaptureSourceId;
2. monotonic or otherwise deterministically ordered;
3. owned by Capture;
4. not Runtime authority;
5. old incompatible SourceVersion Candidates cannot be treated as valid for a newer source configuration.

---

# 8. CaptureOperationId

```text id="r69p76"
CaptureOperationId
- value
```

Identifies one Capture-owned semantic operation.

It is not:

```text id="jnb73z"
WorkItemId
AttemptId
SchedulerJobId
```

---

# 9. RuntimeExecutionIdentity

Capture invocation may carry:

```text id="4j7e9r"
RuntimeExecutionIdentity
├── sessionId
├── runtimeRevisionId
├── workItemId
├── attemptId
├── correlationId?
├── causationId?
├── traceId?
└── configurationSnapshotRef?
```

Rules:

* Capture MUST NOT generate these IDs;
* values may be used for diagnostics and Candidate provenance;
* Runtime remains authoritative.

---

# 10. CancellationContextRef

Long-running provider operations may receive:

```text id="3ro14x"
CancellationContextRef
├── cancellationContextId
├── runtimeRevisionId?
├── workItemId?
└── attemptId?
```

Capture may observe cancellation.

Capture does not mark Runtime work canceled.

---

# 11. CaptureSourceKind

```text id="tw6i9y"
CaptureSourceKind
- ScreenRegion
- ApplicationWindow
- Display
- BrowserCapture
- LocalInput
- RenderedDocument
- Unknown
```

Unknown future values follow contract compatibility rules.

---

# 12. CaptureSourceDescriptor

Public descriptor:

```text id="0fjepl"
CaptureSourceDescriptor
├── sourceKind
├── logicalSourceIdentity
├── region?
├── displayRef?
├── connectorRef?
├── documentRef?
├── requestedCapabilities[]?
└── metadata?
```

Important:

```text id="qrozlh"
Native WindowHandle is forbidden.
```

Any platform-specific handle lives behind CaptureProvider.

---

# 13. Opaque Platform Reference

If a source must refer to platform-owned state, use a serializable opaque reference:

```text id="pmjp9g"
PlatformSourceRef
├── refId
├── refKind
└── providerDomain?
```

This reference must not expose native resource representation.

---

# 14. CaptureRegion

```text id="vlulsc"
CaptureRegion
├── x
├── y
├── width
├── height
└── coordinateSpace
```

Rules:

* finite values;
* non-negative width/height;
* explicit coordinate space;
* must remain inside authorized capture scope.

---

# 15. CaptureMode

```text id="jphouq"
CaptureMode
- OnDemand
- ContinuousSample
- ProviderEventTriggered
```

`ContinuousSample` describes requested acquisition semantics.

It does not mean Capture owns an independent scheduling system.

---

# 16. CaptureOptions

```text id="iof9g4"
CaptureOptions
├── requestedRegion?
├── includeCursor?
├── preferredPixelFormat?
├── maximumWidth?
├── maximumHeight?
├── allowProviderScaling?
├── privacyScope?
└── providerHints?
```

Provider hints MUST remain generic and optional.

Provider-specific SDK objects are forbidden.

---

# 17. CaptureConfiguration

Capture consumes a typed immutable configuration snapshot.

Conceptually:

```text id="wmuwj3"
CaptureConfiguration
├── preferredPixelFormat
├── maximumWidth
├── maximumHeight
├── maximumCandidateBytes
├── sourceConcurrencyLimit
├── localSampleBufferLimit
├── includeCursorDefault
├── allowFullDisplayCapture
├── rawPersistencePolicy
└── privacyPolicy
```

Capture does not read environment/YAML directly.

---

# 18. CaptureInvocation

Primary processing input:

```text id="kzv583"
CaptureInvocation
├── requestId
├── contractVersion
├── runtimeExecutionIdentity
├── cancellationContextRef?
├── captureSourceRef
├── expectedSourceVersion?
├── captureMode
├── options
└── configurationSnapshotRef
```

This replaces the old `CaptureRequest` semantics containing Runtime scheduler policy.

---

# 19. CaptureSourceRef

```text id="2m8r7a"
CaptureSourceRef
├── captureSourceId
└── sourceVersion?
```

If a source version is supplied, Capture validates compatibility.

---

# 20. Removed Runtime-Scheduling Fields

The following fields are NOT Capture-owned public command semantics:

```text id="1uqe2y"
GenerationId
Priority
SchedulerDeadline
RetryCount
QueueClass
SchedulerJobId
```

These belong to Runtime.

A Runtime-controlled deadline/cancellation reference may still be supplied through Runtime execution context.

---

# 21. InvokeCapture

Conceptual processing operation:

```text id="e2jbot"
InvokeCapture(
    CaptureInvocation
)

→ CaptureCompletion
```

`InvokeCapture` executes one bounded Capture operation.

It does not publish an accepted Artifact.

---

# 22. CaptureCompletion

```text id="cl65ce"
CaptureCompletion
├── requestId
├── operationId
├── status
├── candidate?
├── rejection?
├── warnings[]
└── diagnosticsSummary?
```

Possible status:

```text id="oj53fw"
CandidateProduced
Rejected
CanceledObserved
Failed
```

These statuses describe Capture processing only.

They are not Runtime Attempt terminal states.

---

# 23. CandidateCaptureResult

```text id="a1m3h1"
CandidateCaptureResult
├── candidateId
├── operationId
├── captureSourceId
├── sourceVersion
├── normalizedFrame
├── captureMetadata
├── runtimeExecutionIdentity
├── compatibilityMetadata?
├── warnings[]
└── createdAt
```

Rules:

1. not an accepted Artifact;
2. immutable once returned;
3. must not be consumed directly by Recognition;
4. may be discarded by Runtime;
5. must not outlive its temporary resource contract without ownership transfer.

---

# 24. NormalizedCaptureFrame

```text id="j1cizt"
NormalizedCaptureFrame
├── frameRepresentation
├── width
├── height
├── pixelFormat
├── orientation
├── captureRegion
├── coordinateSpace
├── sourceDimensions?
├── scaleMetadata?
└── capturedAt
```

This is Capture's normalized Candidate representation.

---

# 25. FrameRepresentation

Public representation must remain implementation-neutral.

Possible contract forms:

```text id="i33t5i"
FrameRepresentation
├── inlineBytes?          // bounded local profile only
├── temporaryBufferRef?
├── sharedMemoryRef?
├── immutableImageRef?
└── representationType
```

The actual MVP representation is intentionally not locked here.

---

# 26. Frame Representation Rules

Whatever representation is chosen:

* consumers must treat it as immutable;
* native pointers must not cross public boundary;
* lifetime must be explicit;
* byte length must be bounded;
* ownership transfer must be deterministic;
* discarded Candidate resources must be releasable.

---

# 27. CaptureMetadata

```text id="cl7rzh"
CaptureMetadata
├── sourceKind
├── requestedRegion?
├── effectiveRegion?
├── providerName?
├── providerCapabilityVersion?
├── sourceVersion
├── captureDurationMs?
├── normalizationDurationMs?
└── privacyScope?
```

Provider-specific raw response must not appear.

---

# 28. CaptureCompatibilityMetadata

```text id="spz7t8"
CaptureCompatibilityMetadata
├── captureContractVersion
├── sourceDescriptorVersion?
├── pixelFormatVersion?
├── geometryVersion?
└── providerContractVersion?
```

This metadata does not determine Runtime authority.

---

# 29. CapturedFrameArtifact Boundary

After Runtime accepts Candidate completion, Artifact publication produces:

```text id="txfgs8"
CapturedFrameArtifactRef
```

Capture Contract does not define the complete Artifact Store implementation.

Conceptually, accepted Artifact contains:

```text id="b08ijd"
artifactId
artifactType = CAPTURED_FRAME
captureSourceId
sourceVersion
contentIdentity?
frame data/ref
geometry metadata
provenance
contractVersion
```

---

# 30. Candidate vs Artifact

```text id="3xgbfj"
CandidateCaptureResult
    owner: Capture operation
    authority: none outside Candidate boundary

CapturedFrameArtifact
    owner: Artifact Store
    authority: accepted Runtime Artifact
```

This distinction is mandatory.

---

# 31. CreateCaptureSource

```text id="dh4wtd"
CreateCaptureSource
├── requestId
├── descriptor
└── requestedConfiguration?
```

Result:

```text id="rcf6jx"
CreateCaptureSourceResult
├── captureSourceId
├── sourceVersion
├── sourceState
├── capabilities
└── warnings[]
```

Creation may require Application/UI permission interaction externally.

---

# 32. ReplaceCaptureSource

```text id="egor0b"
ReplaceCaptureSource
├── requestId
├── captureSourceId
├── expectedSourceVersion
└── descriptor
```

Result:

```text id="8pis62"
ReplaceCaptureSourceResult
├── captureSourceId
├── previousSourceVersion
├── sourceVersion
├── sourceState
└── capabilities
```

Replacement does not mutate Runtime WorkItems.

---

# 33. SuspendCaptureSource

```text id="dpv0xp"
SuspendCaptureSource
- captureSourceId
```

Suspends CaptureSource availability.

It does not suspend Reading Session.

It does not suspend Runtime globally.

---

# 34. ResumeCaptureSource

```text id="69gu81"
ResumeCaptureSource
- captureSourceId
```

Returns source to usable state if provider/platform conditions permit.

---

# 35. RemoveCaptureSource

```text id="kfs39i"
RemoveCaptureSource
- captureSourceId
```

Rules:

* idempotent;
* source becomes unavailable/stopped;
* Capture releases Capture-owned/provider source references;
* Runtime separately handles outstanding execution.

---

# 36. No Public StartContinuousCapture Lifecycle

The old commands:

```text id="5w9jj0"
StartContinuousCapture
StopContinuousCapture
SuspendCapture
ResumeCapture
CaptureSessionHandle
```

are removed as general Runtime-v2 public scheduling contracts.

Reason:

they create a parallel execution lifecycle inside Capture.

Repeated acquisition belongs to Runtime orchestration/scheduling.

---

# 37. Provider Stream Exception

If a provider requires one long-lived native stream, Capture may expose an internal/provider-oriented stream lease.

Conceptually:

```text id="o6oypl"
CaptureProviderStreamLease
```

It must remain behind Capture's processing/provider boundary unless a later explicit public contract is required.

The lease does not replace Runtime authority.

---

# 38. No Public CancelCapture Authority

The old:

```text id="l6gm6h"
CancelCapture(CaptureOperationId)
```

is removed as a primary public authority contract.

Runtime owns cancellation.

Capture cooperates through:

```text id="9l9f09"
CancellationContextRef
```

A Capture-local source/stream shutdown command may still exist for CaptureSource lifecycle.

---

# 39. GetCaptureSource

```text id="r8ztnb"
GetCaptureSource
- captureSourceId
```

Result:

```text id="0qgrg3"
CaptureSourceSnapshot
├── captureSourceId
├── sourceKind
├── sourceVersion
├── sourceState
├── capabilities
└── descriptorSummary
```

No native handles are returned.

---

# 40. GetCaptureCapabilities

```text id="uz1rvf"
GetCaptureCapabilities
- captureSourceId
```

Result:

```text id="7konvc"
CaptureCapabilities
├── supportsRegionCapture
├── supportsWindowCapture
├── supportsDisplayCapture
├── supportsContinuousStream
├── supportsProviderEventTrigger
├── supportsCursorExclusion
├── supportsOccludedWindowCapture
├── supportsDpiMetadata
├── supportsStructuredCapture
└── maximumConcurrency?
```

---

# 41. Capability Meaning

Capabilities describe normalized semantic ability.

They do not expose provider SDK feature objects.

---

# 42. GetCaptureHealth

```text id="hmk8f2"
GetCaptureHealth
- captureSourceId?
```

Result:

```text id="nedfks"
CaptureHealthSnapshot
├── state
├── sourceId?
├── sourceVersion?
├── providerAvailable
├── permissionState?
├── recentFailureCategory?
├── latencySummary?
└── observedAt
```

---

# 43. CaptureHealthState

```text id="6wn4fh"
CaptureHealthState
- Healthy
- Degraded
- Unavailable
- Recovering
- Stopped
```

Health does not mutate Runtime execution authority.

---

# 44. GetCaptureStatistics

Statistics are optional diagnostic query output.

```text id="7swyb9"
CaptureStatistics
├── totalOperations
├── candidateProducedCount
├── providerFailureCount
├── localDroppedSampleCount
├── averageCaptureLatency
├── averageNormalizationLatency
├── sourceReconnectCount
└── observedWindow
```

Do not expose Runtime retry count as Capture-owned unless clearly labeled as correlated external telemetry.

---

# 45. Source State

```text id="rooqs4"
CaptureSourceState
- Uninitialized
- Initializing
- Ready
- Suspended
- Unavailable
- Stopping
- Stopped
```

Detailed transitions belong to `STATES.md`.

---

# 46. SourceVersion Guard

Source mutation commands may use:

```text id="5kwekm"
expectedSourceVersion
```

Guard:

```text id="yt38h8"
expectedSourceVersion
==
currentSourceVersion
```

This is CaptureSource optimistic concurrency.

It is not Runtime authority validation.

---

# 47. SourceVersionConflict

Conceptual rejection:

```text id="9aqa4i"
SourceVersionConflict
├── captureSourceId
├── expectedSourceVersion
└── currentSourceVersion
```

This protects CaptureSource state.

---

# 48. Capture Request Validation

Capture validates:

* source exists;
* source state usable;
* requested region valid;
* requested region within allowed source scope;
* requested capabilities supported;
* expectedSourceVersion compatible;
* options valid;
* configuration snapshot compatible;
* privacy policy satisfied.

---

# 49. Candidate Validation

Candidate must satisfy:

* buffer/data exists;
* dimensions finite and positive;
* pixel format supported;
* byte length valid;
* geometry metadata valid;
* source ID correct;
* SourceVersion compatible;
* privacy scope respected;
* representation lifetime valid;
* required metadata present.

Capture does not perform Runtime authority check as final authority owner.

---

# 50. Cancellation Semantics

Capture may return:

```text id="5odnpc"
CanceledObserved
```

when cancellation was observed before Candidate completion.

This does not mean:

```text id="fjf3bq"
Runtime Attempt state = Cancelled
```

Runtime owns the terminal execution decision.

---

# 51. Timeout Semantics

Provider timeout may be normalized as Capture-specific failure.

However:

```text id="bfq4d7"
Runtime deadline
```

remains Runtime-owned.

Capture should distinguish:

```text id="7zqzpo"
ProviderTimeout
RuntimeCancellationObserved
```

where useful.

---

# 52. Retry Semantics

Capture may classify failure:

```text id="ag871r"
RetryClassification
- NonRetryable
- RetryableTransient
- RetryableAfterSourceRefresh
- RetryableAfterPermissionChange
```

Runtime Retry Policy decides whether another Attempt occurs.

---

# 53. Error Model

Capture returns stable Capture error contracts.

Conceptual:

```text id="28uckr"
CaptureError
├── errorCode
├── category
├── severity
├── retryClassification
├── recoveryHint?
├── captureSourceId?
├── sourceVersion?
├── operationId?
├── runtimeExecutionIdentity?
└── diagnosticRef?
```

Detailed codes belong to `ERRORS.md`.

---

# 54. Typical Capture Errors

Examples:

```text id="jos06w"
InvalidCaptureRequest
CaptureSourceNotFound
CaptureSourceUnavailable
SourceVersionConflict
PermissionUnavailable
InvalidCaptureRegion
UnsupportedCaptureCapability
ProviderUnavailable
ProviderFailure
ProviderTimeout
InvalidRawCaptureResult
CaptureNormalizationFailed
CandidateCaptureResultInvalid
PrivacyScopeViolation
TemporaryResourceLimitExceeded
```

---

# 55. Runtime-Owned Outcomes

Capture MUST NOT define internal ownership for:

```text id="5yghsn"
RuntimeRevisionStale
WorkItemCancelled
AttemptSuperseded
RetryExhausted
SchedulerRejected
RuntimeShutdown
```

These remain Runtime-owned.

---

# 56. Idempotency

Source lifecycle commands should be idempotent where sensible.

Examples:

```text id="7ttkkf"
SuspendCaptureSource
RemoveCaptureSource
```

Repeated equivalent source mutation should not create unnecessary SourceVersion increments.

---

# 57. Capture Invocation Idempotency

One `InvokeCapture` represents one execution Attempt.

Repeated Runtime invocation may produce a new CaptureOperation.

Deduplication/retry semantics remain Runtime-owned.

Capture must not attempt to merge Runtime Attempts based only on local request identity.

---

# 58. Thread Safety

Public Capture contracts must support concurrent callers.

Logical mutation of one CaptureSource must be serialized.

Provider concurrency is constrained by normalized capability.

Runtime owns global scheduling.

---

# 59. Source Concurrency

Default:

```text id="e5gdma"
maximumConcurrentAcquisition = 1
```

per source unless provider capability says otherwise.

This is a provider/Capture safety constraint.

---

# 60. Temporary Resource Lifetime

Candidate/raw resources are bounded to Capture operation or explicit temporary lease.

Capture must release them when:

* Candidate rejected locally;
* Runtime rejects completion;
* cancellation observed;
* source removed;
* provider fails;
* temporary lease expires.

Accepted Artifact lifetime is external.

---

# 61. Privacy Contract

Capture commands and results must preserve:

1. explicit capture scope;
2. no silent full-display fallback;
3. no raw frame logging;
4. no secret/native handle exposure;
5. raw provider representation remains temporary;
6. remote-provider use requires policy approval;
7. privacy scope metadata remains available for validation;
8. source title/path metadata is minimized.

---

# 62. Security Contract

Capture public contracts MUST NOT expose:

```text id="eo6qi0"
API key
provider secret
OS capability token
native screen handle
browser privileged object
raw permission object
```

---

# 63. Event Boundary

Capture events describe Capture-owned facts.

Possible examples:

```text id="m1ptx3"
CaptureSourceReady
CaptureSourceChanged
CaptureSourceUnavailable
CaptureSourceStopped
CaptureHealthChanged
```

Capture Candidate completion normally returns through Runtime processing completion, not an independent terminal event lifecycle.

Detailed events belong to `EVENTS.md`.

---

# 64. No Processing Chain Through Events

Invalid:

```text id="hf9hwh"
CaptureSucceeded
    ↓
Recognition directly starts
```

Correct:

```text id="bsqmbe"
Capture Candidate
    ↓
Runtime accepts + publishes Artifact
    ↓
Business/Runtime orchestration
    ↓
Recognition
```

---

# 65. UI Boundary

UI/Application may provide normalized source selection intent.

Capture does not receive raw UI events.

Example:

```text id="98in9v"
User selects screen region
        ↓
UI Adapter / Application
        ↓
CaptureSourceDescriptor
        ↓
CreateCaptureSource
```

---

# 66. Reading Session Boundary

Reading Session may describe logical ReadingSource/Target.

CaptureSource creation/mapping occurs through Application/orchestration integration.

Reading Session does not directly call Capture implementation.

---

# 67. Recognition Boundary

Recognition accepts:

```text id="hxlmpl"
CapturedFrameArtifactRef
```

not:

```text id="vd6y9c"
CandidateCaptureResult
RawCaptureResult
provider frame
```

---

# 68. Storage Boundary

Capture does not persist raw frame content through this contract.

Source preferences and region settings may be persisted by Application/Storage through separate contracts.

---

# 69. Compatibility

Semantic versioning:

```text id="kse4ny"
MAJOR.MINOR.PATCH
```

Major change required for:

* changing Candidate vs Artifact semantics;
* changing Runtime ownership;
* exposing native handles;
* changing SourceVersion meaning;
* changing required public command fields incompatibly.

This migration from old API semantics to Runtime-v2 contracts is a major revision.

---

# 70. Unknown Fields

Unknown optional fields should be ignored when safe.

Unknown required semantic enum values must be rejected or handled by explicitly documented fallback.

---

# 71. Architecture Invariants

1. Capture public contracts remain serializable.

2. Native handles never cross public boundary.

3. CaptureSourceId identifies logical Capture source.

4. SourceVersion is Capture-owned.

5. SourceVersion is not Runtime authority.

6. CaptureOperationId is not WorkItemId.

7. RuntimeExecutionIdentity is externally owned.

8. Capture does not create RuntimeRevisionId.

9. Capture does not create WorkItem.

10. Capture does not create Attempt.

11. Capture does not own global scheduler priority.

12. Capture does not own Runtime retry execution.

13. Capture does not expose public scheduling lifecycle through StartContinuousCapture.

14. Runtime owns repeated scheduling.

15. Capture may adapt provider-native streams internally.

16. CandidateCaptureResult is not an accepted Artifact.

17. Recognition never receives CandidateCaptureResult directly.

18. Runtime authority validation occurs before accepted Artifact publication.

19. Accepted CapturedFrameArtifact lifetime belongs to Artifact Store.

20. Capture owns temporary Candidate/raw resources only.

21. RawCaptureResult never crosses public boundary.

22. Normalized Candidate data is immutable after completion.

23. Capture scope never expands silently.

24. Public descriptors contain no OS-specific object.

25. Provider-specific errors are normalized.

26. Capture can classify retryability but does not execute Runtime retry policy.

27. Runtime cancellation observation does not make Capture owner of Runtime cancellation.

28. Event Bus does not replace Runtime processing orchestration.

29. Capture does not perform Recognition.

30. Capture does not perform Translation.

31. Public diagnostics remain privacy-safe.

---

# 72. Example — Runtime-Authorized Capture

```text id="2lps3j"
Runtime Attempt A
    ↓
InvokeCapture
    ↓
Capture validates source
    ↓
CaptureProvider acquires data
    ↓
Capture normalizes
    ↓
CandidateCaptureResult
    ↓
CaptureCompletion
    ↓
Runtime validates Attempt A authority
    ↓
accepted
    ↓
CapturedFrameArtifact published
```

---

# 73. Example — Attempt Becomes Obsolete

```text id="qtve1x"
Attempt A starts Capture
    ↓
Runtime establishes newer authority
    ↓
provider completes A
    ↓
CandidateCaptureResult A
    ↓
Runtime rejects A
    ↓
Candidate temporary data released
```

No Capture `GenerationId` authority is required.

---

# 74. Example — SourceVersion Conflict

```text id="bqjnz0"
CaptureSource S version 5
    ↓
Request expects version 4
    ↓
Capture rejects
    ↓
SourceVersionConflict
```

This is CaptureSource concurrency, not Runtime staleness.

---

# 75. Example — Continuous Sampling

```text id="qg6xd8"
Business Pipeline wants periodic captures
    ↓
Runtime schedules repeated Capture WorkItems
    ↓
InvokeCapture
    ↓
Candidate / Artifact
    ↓
next Runtime scheduling decision
```

Capture owns no standalone continuous scheduling lifecycle.

---

# 76. Example — Provider Native Stream

```text id="hy3uq3"
Runtime authorizes Capture stream scope
    ↓
Capture opens provider-native stream internally
    ↓
provider samples
    ↓
Capture keeps bounded latest sample
    ↓
Runtime-triggered/bounded Candidate completion
```

The native stream is provider mechanics, not Runtime replacement.

---

# 77. Example — Source Selection

```text id="eim1jb"
User selects window
    ↓
UI/Application resolves opaque platform reference
    ↓
CreateCaptureSource
    ↓
CaptureProvider owns native handle
    ↓
Capture returns CaptureSourceId
```

No WindowHandle appears in Capture public contracts.

---

# 78. Related Documents

```text id="3brs82"
doc/02-modules/capture/MODULE.md
doc/02-modules/capture/STATES.md
doc/02-modules/capture/EVENTS.md
doc/02-modules/capture/ERRORS.md
doc/02-modules/capture/README.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/MEMORY_MODEL.md

doc/03-contracts/
doc/04-providers/
```

---

# 79. Completion Criteria

This contract is synchronized when:

* public API is renamed/conceptualized as Capture Contract;
* Runtime execution fields are no longer Capture-owned policy;
* `GenerationId` authority is removed;
* native WindowHandle is absent;
* CaptureInvocation is bounded to one Capture operation;
* continuous Capture scheduling is externalized to Runtime;
* CaptureOperationId is distinct from Runtime WorkItem/Attempt;
* CandidateCaptureResult is distinct from CapturedFrameArtifact;
* Runtime authority controls publication;
* Recognition consumes accepted Artifact refs only;
* accepted Artifact lifetime is external;
* SourceVersion remains Capture-specific concurrency/versioning;
* cancellation cooperation remains Runtime-owned;
* source lifecycle commands remain explicit;
* capability and health queries remain stable;
* privacy rules are enforceable from public contracts.

---

# 80. Summary

Capture's public boundary is:

```text id="74qqco"
Runtime
    ↓
CaptureInvocation
    ↓
Capture
    ↓
CandidateCaptureResult
    ↓
Runtime Completion / Authority Validation
    ↓
CapturedFrameArtifact
```

Source management is:

```text id="p13f8x"
Application / UI intent
    ↓
CaptureSourceDescriptor
    ↓
CaptureSourceService
    ↓
CaptureSourceId + SourceVersion
    ↓
CaptureProvider
    ↓
native platform resource
```

The central rule is:

```text id="ihuw24"
Capture owns acquisition semantics.

Runtime owns execution authority.

Artifact Store owns accepted frame lifetime.

Recognition owns interpretation.
```
