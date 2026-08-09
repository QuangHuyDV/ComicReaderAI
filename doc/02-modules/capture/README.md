# Capture Module

> **Project:** CRAI
> **Module:** `capture`
> **Path:** `doc/02-modules/capture/README.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Overview

The Capture Module is CRAI's source-acquisition processing boundary.

Its responsibility is to acquire authorized source data through a Capture Provider and normalize that data into a platform-independent Candidate Capture Result.

The primary flow is:

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
```

Capture answers:

> How can CRAI safely acquire the requested source data?

Capture does not answer:

> Is this work still authoritative?

That belongs to Runtime Control.

Capture also does not answer:

> Should Recognition, Translation, or Presentation run next?

That belongs to Business Pipeline Orchestration.

---

# 2. Architecture Position

Capture sits between Runtime-controlled processing and platform-specific acquisition.

```text
Reading Session
      ↓
Reading-domain state
      ↓
Business Pipeline Orchestration
      ↓
Runtime Control
      ↓
Capture WorkItem / Attempt
      ↓
Capture
      ↓
CandidateCaptureResult
      ↓
Runtime authority validation
      ↓
Artifact publication
      ↓
CapturedFrameArtifact
      ↓
Recognition when required
```

Capture does not invoke Recognition directly.

Recognition consumes only accepted Capture Artifact references.

---

# 3. Ownership Model

The Capture boundary is divided across four owners.

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

This ownership separation is mandatory.

---

# 4. Primary Responsibilities

Capture owns:

```text
CaptureSource semantics
CaptureSource lifecycle
SourceVersion
Capture request validation
provider capability validation
source permission semantics
source acquisition
provider-result normalization
CandidateCaptureResult construction
Capture-local Candidate validation
Capture-local temporary resource cleanup
Capture health
Capture-specific errors
Capture privacy enforcement
```

---

# 5. Explicit Non-Responsibilities

Capture MUST NOT:

* detect text;
* perform OCR;
* perform image preprocessing specifically for OCR;
* detect speech bubbles;
* infer reading order;
* normalize recognized text;
* translate content;
* construct Presentation state;
* render UI;
* decide whether content changed;
* determine whether a frame is duplicate;
* detect page/chapter changes;
* determine pipeline dependencies;
* create Runtime WorkItems;
* create Runtime Attempts;
* create RuntimeRevisionId;
* own Runtime cancellation;
* own Runtime retry;
* own Runtime scheduling;
* publish accepted Artifacts itself;
* own long-term reading history.

---

# 6. Product Context

CRAI is designed to support uninterrupted reading.

Typical sources may include:

```text
comic website
novel website
browser document
desktop reading application
PDF reader
selected screen region
local image/document
```

The user should not need to repeatedly:

```text
save screenshot
upload screenshot
translate screenshot
return to reader
```

Capture provides the acquisition capability required to automate that workflow.

However, automation must remain bounded by:

```text
explicit source selection
privacy scope
Runtime authority
resource policy
```

---

# 7. CaptureSource

`CaptureSource` represents one logical capturable source.

Typical kinds:

```text
ScreenRegion
ApplicationWindow
Display
BrowserCapture
LocalInput
RenderedDocument
```

Conceptually:

```text
CaptureSource
├── CaptureSourceId
├── SourceVersion
├── SourceKind
├── SourceDescriptor
├── Capabilities
└── SourceState
```

---

# 8. CaptureSourceId

`CaptureSourceId` identifies a logical Capture source.

It is independent from:

```text
ReadingSessionId
ReadingTargetId
Runtime WorkItemId
Runtime AttemptId
native window handle
```

A CaptureSourceId may remain stable while its underlying platform-native resource is replaced.

---

# 9. SourceVersion

Capture owns:

```text
SourceVersion
```

It changes when CaptureSource semantics materially change.

Examples:

```text
capture region changed
provider-native source replaced
source descriptor changed
capabilities changed materially
logical platform association changed
```

SourceVersion is used for CaptureSource compatibility.

It is **not Runtime execution authority**.

---

# 10. SourceVersion vs RuntimeRevisionId

These are independent domains.

```text
SourceVersion
    → version of CaptureSource semantics

RuntimeRevisionId
    → version of Runtime execution intent
```

A Candidate may fail because:

```text
SourceVersion is incompatible
```

even while Runtime execution remains authoritative.

A Candidate may also be locally valid but rejected because:

```text
Runtime execution is obsolete
```

The two protections must not be merged.

---

# 11. Removed GenerationId Authority

Capture v1 used:

```text
GenerationId
```

to identify stale Capture work.

That model is removed.

Runtime v2 already owns execution authority through:

```text
RuntimeRevisionId
WorkItemId
AttemptId
cancellation
completion validation
```

Capture MUST NOT create a competing session-generation authority.

---

# 12. CaptureSource Lifecycle

CaptureSource lifecycle is:

```text
UNINITIALIZED
      ↓
INITIALIZING
      ↓
READY
  ↕
SUSPENDED

READY / SUSPENDED
      ↓
UNAVAILABLE
      ↓
INITIALIZING

*
↓
STOPPING
↓
STOPPED
```

Detailed semantics are defined in:

```text
STATES.md
```

---

# 13. Why There Is No `ACTIVE` Source State

CaptureSource `READY` means:

```text
the source can serve Capture operations
```

It does not mean:

```text
no operation currently exists
```

or:

```text
a Runtime Attempt is currently running
```

Runtime execution is tracked separately.

Therefore Capture v2 does not require a source-level:

```text
ACTIVE / CAPTURING
```

state.

---

# 14. CaptureSource State vs Runtime State

Example:

```text
CaptureSource = READY
```

while Runtime may have:

```text
0 Capture Attempts
1 running Capture Attempt
a queued future Capture WorkItem
```

CaptureSource state must not mirror Runtime activity.

---

# 15. Supported Source — Screen Region

`ScreenRegion` captures a bounded area selected or authorized by the user/application.

Typical use:

* comic website;
* novel website without connector;
* PDF reader;
* desktop reader;
* arbitrary visual source.

This is a primary MVP source.

---

# 16. Supported Source — Application Window

`ApplicationWindow` represents one logical application-window capture source.

Useful when CRAI should:

* stay within one application;
* avoid unrelated screen content;
* follow a moving window;
* maintain a stable logical source.

Native window handles remain behind CaptureProvider.

---

# 17. Supported Source — Display

`Display` captures a full display.

It should be used only when explicitly authorized.

It must not be an automatic fallback from:

```text
ScreenRegion
or
ApplicationWindow
```

because its privacy scope is broader.

---

# 18. Supported Source — Browser Capture

Browser integration may provide:

```text
pixel screenshot
viewport metadata
page dimensions
connector metadata
```

Capture may normalize acquisition-oriented data.

Structured DOM/text content should not automatically be forced into image Capture semantics.

A separate structured-source capability may be used where appropriate.

---

# 19. Supported Source — Local Input

Possible local sources:

```text
image
image collection
clipboard image
rendered document page
```

Local Input is useful but is not the primary continuous-reading experience.

---

# 20. CaptureProvider Boundary

Capture core depends only on a stable abstraction:

```text
CaptureProvider
```

Possible implementations:

```text
ScreenCaptureProvider
WindowCaptureProvider
BrowserCaptureProvider
LocalInputProvider
DocumentRenderProvider
```

Concrete provider implementations are wired outside Capture core.

---

# 21. Native Handle Boundary

Public Capture contracts MUST NOT expose:

```text
HWND
CGWindowID
X11 Window
Wayland object
DOM Node
browser tab object
native GPU handle
provider SDK object
```

The platform/provider layer owns those representations.

Capture works with:

```text
CaptureSourceId
PlatformSourceRef
SourceDescriptor
```

or equivalent stable contracts.

---

# 22. Permission Boundary

Capture owns normalized permission semantics.

Examples:

```text
permission available?
requested scope permitted?
region inside authorized source?
```

Platform-specific permission APIs belong to:

```text
CaptureProvider
Platform Adapter
Application/UI integration
```

---

# 23. Permission Rules

Capture must:

1. capture only authorized source scope;
2. never silently widen the scope;
3. never automatically fall back to full-display Capture;
4. fail closed on privacy-scope violations;
5. stop producing valid Candidates when permission is lost;
6. expose normalized permission errors/state;
7. avoid permission-prompt loops.

---

# 24. Capture Modes

Capture supports semantic acquisition modes such as:

```text
OnDemand
ContinuousSample
ProviderEventTriggered
```

These modes describe acquisition intent.

They do not grant Capture ownership of Runtime scheduling.

---

# 25. On-Demand Capture

Typical flow:

```text
Runtime invokes Capture
    ↓
validate
    ↓
acquire
    ↓
normalize
    ↓
CandidateCaptureResult
    ↓
return to Runtime
```

One invocation performs one bounded Capture operation.

---

# 26. Continuous Sampling

`ContinuousSample` does not mean:

```text
Capture starts its own scheduler loop
```

Preferred flow:

```text
Runtime policy
    ↓
Capture WorkItem
    ↓
bounded Capture invocation
    ↓
completion

Runtime policy
    ↓
next Capture WorkItem
```

Repeated scheduling remains Runtime-owned.

---

# 27. Provider-Native Continuous Streams

Some platform APIs may require a long-lived native capture stream.

Capture may adapt such mechanics internally through CaptureProvider.

Example:

```text
Runtime-authorized Capture scope
        ↓
provider-native stream
        ↓
bounded samples
        ↓
Capture Candidates
```

The provider stream does not become a second Runtime.

---

# 28. Event-Triggered Acquisition

A provider or connector may expose a signal indicating useful acquisition opportunity.

That signal may contribute to Runtime/orchestration decisions.

Capture must not turn provider callbacks into hidden autonomous pipeline execution.

---

# 29. CaptureInvocation

Primary processing input is conceptually:

```text
CaptureInvocation
├── requestId
├── contractVersion
├── RuntimeExecutionIdentity
├── CancellationContextRef?
├── CaptureSourceRef
├── expectedSourceVersion?
├── CaptureMode
├── CaptureOptions
└── ConfigurationSnapshotRef
```

Full schema is defined in:

```text
CONTRACT.md
```

---

# 30. Runtime Execution Context

Runtime may supply:

```text
sessionId
runtimeRevisionId
workItemId
attemptId
configurationSnapshotRef
cancellationContext
correlationId
traceId
```

Capture may use these values for:

```text
correlation
diagnostics
cancellation cooperation
Candidate provenance
```

Capture does not own them.

---

# 31. Removed Capture-Owned Scheduler Fields

Capture contracts do not own:

```text
Priority
QueueClass
SchedulerDeadline
RetryCount
GenerationId
SchedulerJobId
```

These are Runtime concerns.

---

# 32. CaptureOperation

`CaptureOperation` represents Capture-local semantic processing for one invocation.

Its phases are:

```text
VALIDATING
    ↓
ACQUIRING
    ↓
NORMALIZING
    ↓
VALIDATING_CANDIDATE
    ↓
COMPLETING
    ↓
FINISHED
```

Possible local outcomes include:

```text
REJECTED
FAILED
ABORTED_LOCAL
```

These are not Runtime Attempt states.

---

# 33. No Capture-Owned `QUEUED`

Capture does not own:

```text
QUEUED
```

By the time Capture is invoked, Runtime has already handled queue/scheduler execution.

---

# 34. No Capture-Owned Runtime `CANCELLED`

Capture may observe Runtime cancellation and stop local useful work.

It does not decide:

```text
AttemptState = CANCELLED
```

Runtime owns that transition.

---

# 35. No Capture-Owned Runtime `TIMED_OUT`

Capture distinguishes:

```text
ProviderTimeout
```

from:

```text
RuntimeDeadlineExceeded
```

ProviderTimeout is a Capture/provider error.

Runtime deadline is Runtime-owned.

---

# 36. Raw Provider Result

CaptureProvider may return provider-specific raw data.

Conceptually:

```text
RawCaptureResult
```

Its lifetime is limited to:

```text
provider acquisition
    ↓
Capture normalization
```

Raw provider result never crosses the public Capture boundary.

---

# 37. Capture Normalization

Capture normalization may normalize:

```text
pixel representation
width
height
orientation
capture region
source dimensions
coordinate space
DPI/scale metadata
capture timestamp
CaptureSourceId
SourceVersion
```

---

# 38. What Capture Normalization Does Not Do

Capture does not:

```text
denoise for OCR
sharpen for OCR
deskew text
detect text
detect speech bubbles
infer text direction
infer reading order
classify document content
```

These belong downstream.

---

# 39. CandidateCaptureResult

Capture's primary processing output is:

```text
CandidateCaptureResult
```

Conceptually:

```text
CandidateCaptureResult
├── candidateId
├── operationId
├── CaptureSourceId
├── SourceVersion
├── NormalizedCaptureFrame
├── CaptureMetadata
├── RuntimeExecutionIdentity
├── warnings[]
└── createdAt
```

A Candidate is not globally authoritative.

---

# 40. Candidate Validation

Capture validates:

```text
source identity
SourceVersion compatibility
frame dimensions
pixel format
buffer bounds
coordinate metadata
privacy scope
Candidate size
temporary resource lifetime
required metadata
```

Capture does not decide global Runtime staleness.

---

# 41. Candidate vs Accepted Artifact

The distinction is:

```text
CandidateCaptureResult
    → Capture-owned operation output

CapturedFrameArtifact
    → Runtime-accepted Artifact
```

Flow:

```text
CandidateCaptureResult
        ↓
Runtime authority validation
        ↓
accepted?
    ├── no → discard
    └── yes
          ↓
      Artifact publication
          ↓
      CapturedFrameArtifact
```

---

# 42. CapturedFrameArtifact

Once accepted, the result becomes an immutable Artifact.

Conceptually:

```text
CapturedFrameArtifact
├── artifactId
├── artifactType
├── CaptureSourceId
├── SourceVersion
├── frame representation/ref
├── geometry metadata
├── provenance
└── contractVersion
```

Artifact lifetime belongs to Artifact Store/Runtime Resource Lifecycle.

---

# 43. Recognition Boundary

Recognition consumes:

```text
CapturedFrameArtifactRef
```

It MUST NOT consume:

```text
RawCaptureResult
CandidateCaptureResult
provider-native frame
```

Correct:

```text
Capture Candidate
    ↓
Runtime acceptance
    ↓
CapturedFrameArtifact
    ↓
Recognition
```

---

# 44. Resource Ownership

Before Artifact publication:

```text
raw provider resources
    → provider/Capture temporary ownership

Candidate resources
    → Capture operation / temporary lease ownership
```

After publication:

```text
accepted Artifact payload
    → Artifact Store / Runtime resource ownership
```

Capture must not act as a second accepted-Artifact lifetime manager.

---

# 45. Temporary Resource Cleanup

Capture owns cleanup for:

```text
raw provider buffer
temporary Candidate buffer
provider source lease
source-local sample buffer
temporary normalization resources
```

Cleanup must be deterministic.

---

# 46. Accepted Artifact Cleanup

Capture does not dispose accepted Artifacts merely because:

```text
source changes
new Candidate is produced
Capture operation finishes
```

Artifact lifetime follows Artifact Store leases and Runtime Resource Lifecycle.

---

# 47. Immutability

Once Candidate validation completes:

```text
CandidateCaptureResult
```

must be treated as immutable.

Accepted:

```text
CapturedFrameArtifact
```

is also immutable.

Recognition creates separate preprocessing representations rather than mutating Capture Artifact data in place.

---

# 48. Coordinate Space

Capture output must explicitly describe coordinate semantics.

Possible coordinate spaces include:

```text
Display
Window
SourceContent
Viewport
CaptureFrame
NormalizedSource
```

Capture must never assume:

```text
display coordinates == frame coordinates
DPI == 1.0
browser zoom == 100%
window position is stable
```

---

# 49. Geometry Responsibility

Capture owns acquisition geometry metadata such as:

```text
requested capture region
effective capture region
source dimensions
capture-frame dimensions
coordinate space
scale metadata
```

It does not detect textual geometry.

---

# 50. Reading Session Boundary

Reading Session owns:

```text
what the user is reading
ReadingTarget
ReadingPosition
ReadingContextRevision
```

Capture owns:

```text
how a requested CaptureSource is acquired
```

Reading Session should not directly call Capture implementation.

---

# 51. Business Pipeline Orchestration Boundary

Business Pipeline Orchestration decides:

```text
is Capture required?
can an existing Capture Artifact be reused?
should Capture be skipped?
does Recognition need to run afterward?
```

Capture does not own those rules.

---

# 52. Runtime Boundary

Runtime owns:

```text
RuntimeRevisionId
WorkItem
Attempt
queueing
priority
deadline
authority
cancellation
retry
completion acceptance
supersession
```

Capture only performs one bounded processing responsibility inside that execution model.

---

# 53. Scheduling

Capture owns no global scheduler.

Invalid:

```text
Capture
    ↓
CaptureQueue
    ↓
CaptureWorker
    ↓
retry scheduler
```

when Runtime already owns those components.

Correct:

```text
Runtime Scheduler
    ↓
WorkItem / Attempt
    ↓
Capture
```

---

# 54. Backpressure

Capture must remain bounded.

Capture-local mechanisms may include:

```text
drop obsolete provider callbacks
retain only newest unsubmitted sample
limit provider buffer count
reject unsafe concurrent source acquisition
```

Runtime owns:

```text
global queue admission
priority
fairness
retry
frequency policy
resource admission
```

---

# 55. Freshness Principle

For interactive reading:

```text
fresh relevant capture
>
large stale capture backlog
```

This principle may influence local provider buffering and Runtime scheduling policy.

It must not create a private Capture scheduler.

---

# 56. Source Concurrency

MVP default:

```text
maximumConcurrentAcquisitionPerSource = 1
```

unless the provider explicitly supports safe higher concurrency.

This is a Capture/provider safety constraint.

It is not a global queue rule.

---

# 57. Cancellation

Runtime owns cancellation authority.

Capture cooperates through a cancellation context.

Typical checkpoints:

```text
before provider acquisition
during provider operation where supported
before normalization
before Candidate completion
```

---

# 58. Late Provider Result

Example:

```text
Runtime cancellation observed
        ↓
provider cannot stop immediately
        ↓
provider returns result later
        ↓
Capture stops local processing when possible
        ↓
Runtime authority validation remains final protection
```

No GenerationId is required.

---

# 59. Source Replacement

CaptureSource replacement may cause:

```text
SourceVersion N
    ↓
SourceVersion N+1
```

An operation using version N may be rejected if incompatible.

Runtime independently determines whether its Attempt is obsolete.

---

# 60. Continuous Capture and Cancellation

Repeated sampling is Runtime-owned.

Stopping a Reading Session may cause Runtime/Application to:

```text
stop creating Capture work
cancel relevant Attempts
stop CaptureSources
```

through explicit owner-specific actions.

Capture does not subscribe to Reading Session events and autonomously cancel Runtime execution.

---

# 61. Events

Capture publishes only Capture-owned source/health facts.

Recommended core events:

```text
CaptureSourceReady
CaptureSourceChanged
CaptureSourceSuspended
CaptureSourceUnavailable
CaptureSourceStopped
CaptureHealthChanged
```

Detailed contracts:

```text
EVENTS.md
```

---

# 62. No `CaptureFrameReady`

Capture v2 does not publish:

```text
CaptureFrameReady
```

Recognition must never subscribe directly to Capture frame availability.

The accepted Artifact path is authoritative.

---

# 63. No Capture Operation Event Lifecycle

Capture v2 does not require public Event Bus events:

```text
CaptureStarted
CaptureCompleted
CaptureCancelled
CaptureTimeout
CaptureFailed
```

Operation completion returns through Runtime processing contracts.

Operation progress belongs to telemetry.

---

# 64. Events Are Facts, Not Commands

Example:

```text
CaptureSourceUnavailable
```

means:

```text
the source is now unavailable
```

It does not mean:

```text
cancel all Runtime work now
```

Consumers decide their own policy.

---

# 65. No Mandatory Direct Subscriptions

Capture has no mandatory subscriptions to:

```text
ReadingSessionPaused
ReadingSessionStopped
ReadingGenerationChanged
ConfigurationChanged
RuntimeShutdown
```

for correctness.

Cross-module actions should use explicit contracts/orchestration.

---

# 66. Shutdown

Runtime/Application shutdown should explicitly drive Capture shutdown.

Capture-side flow:

```text
stop accepting new Capture invocation
        ↓
CaptureSources → STOPPING
        ↓
release Capture-owned resources
        ↓
CaptureSources → STOPPED
        ↓
CaptureHealth → STOPPED
```

Runtime separately handles WorkItems/Attempts.

---

# 67. Capture Health

Capture health states:

```text
HEALTHY
DEGRADED
UNAVAILABLE
RECOVERING
STOPPED
```

Health describes Capture capability.

It does not directly mutate Runtime state.

---

# 68. Error Model

Capture errors describe Capture-owned problems.

Primary categories:

```text
Request Validation
Capture Source
Acquisition / Provider
Normalization / Candidate
Permission
Capture State
Temporary Resources
Internal
```

Detailed codes:

```text
ERRORS.md
```

---

# 69. Runtime-Owned Errors

Capture does not own:

```text
RuntimeRevisionStale
AttemptSuperseded
WorkItemCancelled
RuntimeDeadlineExceeded
RetryExhausted
SchedulerRejected
ArtifactPublicationFailed
```

---

# 70. Retry Classification

Capture may classify failures:

```text
NonRetryable
RetryableTransient
RetryableAfterSourceRefresh
RetryableAfterPermissionChange
RetryableAfterInputCorrection
```

Runtime Retry Policy decides whether another Attempt actually occurs.

---

# 71. Provider Timeout vs Runtime Deadline

Important separation:

```text
ProviderTimeout
    → Capture/provider error

RuntimeDeadlineExceeded
    → Runtime error/outcome
```

Capture must not expose one generic `CaptureTimeout` for both.

---

# 72. No `ImageTooSmallForOCR`

Capture must not decide:

```text
image is too small for OCR
```

That requires Recognition knowledge.

Capture may only enforce:

```text
structural frame validity
Capture-configured dimensions
Capture-configured resource limits
```

---

# 73. No Global Capture `FAILED` State

Capture v2 uses scoped state:

```text
CaptureSourceState
CaptureOperationPhase
CaptureHealthState
```

A critical error may:

```text
fail one operation
make one source unavailable
degrade Capture health
or require controlled Capture shutdown
```

It does not automatically transition the entire module into one global `FAILED` state.

---

# 74. Privacy Model

Capture is one of CRAI's most privacy-sensitive capabilities.

Mandatory rules:

1. capture only explicitly authorized source scope;
2. do not silently switch source;
3. do not silently enlarge capture region;
4. do not automatically fall back to full display;
5. raw frame is memory-only by default;
6. raw frame is never logged;
7. remote transmission requires explicit policy;
8. source metadata is minimized;
9. discarded Candidates are released promptly;
10. accepted Artifact retention follows explicit Artifact policy.

Default:

```text
Raw capture data is memory-only.
```

---

# 75. Raw Frame Persistence

Capture does not persist raw frame data by default.

Persistence requires an explicit product/architecture policy covering:

```text
user intent
privacy
encryption
retention
deletion
storage owner
```

Capture itself does not own persistent raw-image storage.

---

# 76. Security Boundary

Public Capture contracts must not expose:

```text
API key
provider secret
permission token
native source handle
browser privileged object
memory pointer
```

---

# 77. Configuration

Capture consumes a typed immutable configuration snapshot.

Typical Capture-owned configuration:

```text
preferred pixel format
maximum dimensions
maximum Candidate bytes
source concurrency limit
local provider buffer limit
cursor inclusion
region policy
privacy policy
raw-persistence policy
```

---

# 78. Runtime-Owned Configuration

Capture configuration must not duplicate:

```text
Runtime priority
global queue policy
Runtime retry policy
Runtime scheduler fairness
global deadline policy
```

---

# 79. Observability

Capture may expose metrics such as:

```text
capture_operation_total
capture_operation_duration_ms
capture_provider_duration_ms
capture_normalization_duration_ms
capture_candidate_bytes
capture_source_unavailable_total
capture_provider_failure_total
capture_provider_timeout_total
capture_permission_unavailable_total
capture_local_sample_drop_total
capture_candidate_discard_total
capture_health_state
```

---

# 80. Runtime Observability Separation

Runtime owns:

```text
WorkItem queue time
Attempt retry count
Runtime cancellation count
Runtime deadline failures
scheduler rejection
Runtime supersession
```

Capture should not duplicate these as Capture-owned metrics.

---

# 81. Logging

Safe Capture diagnostic fields include:

```text
operationId
CaptureSourceId
SourceVersion
sourceKind
runtimeRevisionId?
workItemId?
attemptId?
provider domain
frame dimensions
Candidate byte size
capture duration
normalization duration
error code
health state
```

---

# 82. Logging Prohibitions

Never log:

```text
raw image
screenshot
pixel buffer
OCR text
translation text
DOM content
cookies
credentials
tokens
native handles
full sensitive URLs
```

---

# 83. Platform Independence

Capture core must remain independent from:

```text
Windows Graphics Capture
DXGI
macOS ScreenCaptureKit
X11
Wayland
browser extension SDK
Electron
Flutter
Qt
Android APIs
```

Concrete integrations live behind provider/adapters.

---

# 84. Dependencies

Capture may depend on stable abstractions for:

```text
Runtime execution context
Cancellation context
CaptureProvider
typed configuration
geometry primitives
temporary resource abstractions
diagnostics
common error contracts
```

Capture must not depend directly on:

```text
Reading Session implementation
Recognition implementation
Translation implementation
Presentation implementation
Scheduler implementation
Work Queue implementation
Storage backend
native UI framework
provider SDK from core module
```

---

# 85. Browser Connector Boundary

Browser integration may later act as:

```text
CaptureProvider
structured-source adapter
Application connector
```

The exact design may evolve.

The invariant remains:

```text
browser/platform-specific details
must stay outside Capture core contracts
```

---

# 86. Structured Content

If browser integration can provide:

```text
structured text
DOM-derived semantic blocks
```

CRAI should evaluate whether those inputs belong to a structured-source capability rather than forcing them into pixel Capture.

Capture should not become a generic “all external input” module without clear semantic boundaries.

---

# 87. MVP Scope

Recommended Capture v2 MVP:

```text
ScreenRegion CaptureSource
ApplicationWindow where platform permits
OnDemand acquisition
Runtime-controlled repeated sampling
single concurrent acquisition per source
SourceVersion validation
CaptureProvider abstraction
CandidateCaptureResult
Runtime authority-aware completion
memory-only raw data
bounded temporary buffers
basic permission handling
privacy-safe diagnostics
fake provider testing
```

---

# 88. Continuous Sampling in MVP

Continuous reading may still be supported in MVP.

But the architecture is:

```text
Runtime schedules repeated Capture invocation
```

not:

```text
Capture owns continuous scheduler session
```

This distinction should remain even if implementation initially uses a simple timer in the Runtime layer.

---

# 89. Deferred Scope

Possible future capabilities:

```text
browser extension integration
structured browser acquisition
multi-source Capture
multi-monitor coordination
GPU-native frame sharing
adaptive capture resolution
remote Capture source
mobile Capture
capture recording/replay
advanced document rendering
automatic source discovery
```

---

# 90. Automatic Reader-Area Detection

Automatic reader-area detection is not automatically Capture-owned.

It requires a separate ownership decision because it may involve:

```text
visual analysis
content classification
layout understanding
user interaction
```

Capture should remain responsible for acquisition, not interpretation.

---

# 91. Performance Principles

Capture should prioritize:

```text
low acquisition latency
bounded temporary memory
low UI interference
bounded provider buffering
deterministic cleanup
fresh relevant samples
```

Capture should avoid:

```text
large stale frame backlog
unbounded streams
private worker pools
duplicate frame retention
```

---

# 92. Freshness over Backlog

For CRAI's interactive reading experience:

```text
latest relevant sample
>
processing every historical sample
```

The principle may inform Runtime policy and local provider buffering.

---

# 93. Common Architecture Mistakes

## Mistake 1 — Direct Reading Session → Capture

Wrong:

```text
Reading Session
    ↓
Capture.execute()
```

Correct:

```text
Reading Session state
    ↓
Business Pipeline Orchestration
    ↓
Runtime
    ↓
Capture
```

---

# 94. Mistake 2 — Capture → Recognition Direct Call

Wrong:

```text
Capture
    ↓
Recognition.execute(Candidate)
```

Correct:

```text
Capture Candidate
    ↓
Runtime authority validation
    ↓
CapturedFrameArtifact
    ↓
Runtime/Orchestration
    ↓
Recognition
```

---

# 95. Mistake 3 — Capture Publishes Frame Event

Wrong:

```text
CaptureFrameReady
    ↓
Recognition subscriber
```

Correct:

```text
Candidate completion
    ↓
Runtime
    ↓
Artifact publication
```

---

# 96. Mistake 4 — GenerationId Controls Capture Authority

Wrong:

```text
Generation changed
    ↓
Capture declares old result invalid globally
```

Correct:

```text
SourceVersion
    → source semantic compatibility

RuntimeRevisionId
    → Runtime execution authority
```

---

# 97. Mistake 5 — Capture Owns Continuous Scheduler

Wrong:

```text
StartContinuousCapture
    ↓
Capture timer
    ↓
private queue
```

Correct:

```text
Runtime repeated scheduling
    ↓
bounded Capture invocations
```

---

# 98. Mistake 6 — Capture Owns Retry

Wrong:

```text
Capture provider failed
    ↓
Capture retries three times automatically
```

Correct:

```text
Capture classifies failure
    ↓
Runtime Retry Policy
    ↓
new Attempt if policy allows
```

Provider-internal retry is allowed only when bounded and transparent.

---

# 99. Mistake 7 — Native Handle in Contract

Wrong:

```text
CaptureSourceDescriptor.windowHandle
```

Correct:

```text
CaptureSourceId
PlatformSourceRef
```

with native resource hidden inside provider.

---

# 100. Mistake 8 — Capture Owns Accepted Frame Lifetime

Wrong:

```text
Capture Frame Manager
    ↓
keep accepted frames
    ↓
delete when newer frame exists
```

Correct:

```text
Capture owns temporary Candidate data

Artifact Store owns accepted Artifact lifetime
```

---

# 101. Architecture Invariants

1. Capture only acquires and normalizes source data.

2. Capture does not interpret textual content.

3. Capture does not perform Recognition.

4. Capture does not perform Translation.

5. Capture does not build Presentation.

6. Capture does not decide pipeline topology.

7. Business Pipeline Orchestration decides whether Capture is required.

8. Runtime owns RuntimeRevisionId.

9. Runtime owns WorkItem.

10. Runtime owns Attempt.

11. Runtime owns execution authority.

12. Runtime owns cancellation.

13. Runtime owns retry execution.

14. Runtime owns scheduling.

15. Capture owns CaptureSource.

16. Capture owns SourceVersion.

17. SourceVersion is not Runtime authority.

18. GenerationId is not a Capture v2 authority primitive.

19. CaptureSource state is independent from Runtime Attempt state.

20. CaptureOperation phases do not duplicate Runtime lifecycle.

21. Continuous sampling scheduling is Runtime-owned.

22. Provider-native streams may remain internal mechanics.

23. CandidateCaptureResult is not an accepted Artifact.

24. Runtime authority validation precedes Artifact publication.

25. Recognition consumes accepted Artifact references only.

26. Accepted CapturedFrameArtifact lifetime belongs to Artifact Store.

27. Capture owns temporary resources only.

28. Raw provider result never crosses public Capture boundary.

29. Native handles never cross public contracts.

30. Capture scope never expands silently.

31. Full-display fallback requires explicit authorization.

32. Capture does not persist raw frame by default.

33. Capture does not log raw frame.

34. Capture errors remain Capture-owned only.

35. ProviderTimeout is distinct from Runtime deadline.

36. Capture classifies retryability but does not execute Runtime retry.

37. Capture does not publish operation lifecycle events as a competing Runtime state model.

38. Capture Event Bus facts describe source/health state only.

39. Capture does not require direct Reading Session event subscriptions.

40. Diagnostics remain privacy-safe.

---

# 102. Document Set

```text
02-modules/
└── capture/
    ├── README.md
    ├── MODULE.md
    ├── CONTRACT.md
    ├── STATES.md
    ├── EVENTS.md
    └── ERRORS.md
```

---

# 103. Document Responsibilities

## README.md

Entry point and architecture overview.

Answers:

```text
What is Capture?
What does it own?
Where does it sit?
What should I read next?
```

## MODULE.md

Defines:

```text
module ownership
internal responsibilities
provider boundary
Runtime boundary
Artifact boundary
architecture invariants
```

## CONTRACT.md

Defines:

```text
CaptureInvocation
CaptureSource contracts
queries
CandidateCaptureResult
SourceVersion
Capture capability
Capture health
public validation rules
```

## STATES.md

Defines:

```text
CaptureSource lifecycle
CaptureOperation phases
Candidate lifecycle
Capture health
source replacement
permission recovery
```

## EVENTS.md

Defines:

```text
Capture-owned source facts
Capture health facts
event ownership
publication timing
```

## ERRORS.md

Defines:

```text
Capture error codes
retry classification
source errors
provider errors
Candidate errors
permission errors
resource errors
internal invariants
```

---

# 104. Recommended Reading Order

For a new contributor:

```text
1. README.md
2. MODULE.md
3. CONTRACT.md
4. STATES.md
5. EVENTS.md
6. ERRORS.md
```

---

# 105. Implementation Reading Order

For implementation:

```text
MODULE.md
    ↓
CONTRACT.md
    ↓
STATES.md
    ↓
ERRORS.md
    ↓
EVENTS.md
```

This keeps implementation focused on ownership and processing contracts before Event Bus integration.

---

# 106. Testing Priorities

Capture tests should verify:

```text
source lifecycle
SourceVersion
provider abstraction
request validation
permission boundaries
normalization
Candidate validation
Runtime cancellation cooperation
temporary resource cleanup
late provider result
Runtime authority rejection
privacy
```

---

# 107. Runtime Boundary Tests

Tests must verify Capture never:

```text
creates WorkItem
creates Attempt
creates RuntimeRevisionId
owns scheduler queue
schedules Runtime retry
publishes accepted Artifact
marks Attempt Cancelled
marks Attempt TimedOut
```

---

# 108. Artifact Boundary Tests

Verify:

```text
valid Candidate
    ↓
Runtime rejects authority
    ↓
no CapturedFrameArtifact

valid Candidate
    ↓
Runtime accepts
    ↓
Artifact publication

accepted Artifact
    ↓
Capture operation ends
    ↓
Artifact remains valid according to Artifact lease
```

---

# 109. Provider Boundary Tests

Verify:

* provider success;
* provider unavailable;
* permission denied/revoked;
* source lost;
* provider timeout;
* invalid provider result;
* native stream callbacks;
* resource cleanup;
* no native objects escape public boundary.

---

# 110. Privacy Tests

Verify:

```text
no full-display fallback
no unauthorized region
no raw frame log
no sensitive provider object in diagnostics
raw Candidate released after rejection
memory-only default respected
```

---

# 111. Related Documents

```text
doc/02-modules/capture/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
└── ERRORS.md

doc/01-architecture/core/
├── CAPABILITY_MAP.md
├── DATA_FLOW.md
├── EVENT_BUS.md
├── EVENT_CONVENTION.md
└── STATE_MACHINE.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── MODULE_DEPENDENCY.md
└── OWNERSHIP_MAP.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── CANCELLATION.md
├── RETRY_POLICY.md
├── RESOURCE_LIFECYCLE.md
├── MEMORY_MODEL.md
└── RUNTIME_OBSERVABILITY.md

doc/02-modules/reading-session/
doc/02-modules/recognition/
doc/02-modules/presentation/

doc/03-contracts/
doc/04-providers/
```

---

# 112. Completion Checklist

The Capture module is synchronized when:

* [ ] Capture is classified as a Processing Module;
* [ ] Runtime invokes Capture through bounded contracts;
* [ ] Reading Session does not invoke Capture directly;
* [ ] Business Pipeline Orchestration owns Capture-required decisions;
* [ ] Runtime owns execution authority;
* [ ] `GenerationId` Capture authority has been removed;
* [ ] SourceVersion is Capture-owned;
* [ ] native handles are absent from public contracts;
* [ ] CaptureSource lifecycle is independent from Runtime execution;
* [ ] CaptureOperation is a local phase model;
* [ ] continuous scheduling is Runtime-owned;
* [ ] provider-native streams remain bounded implementation mechanics;
* [ ] CandidateCaptureResult is distinct from CapturedFrameArtifact;
* [ ] Runtime authority validation occurs before Artifact publication;
* [ ] Recognition consumes accepted Artifact refs only;
* [ ] accepted Artifact lifetime is external;
* [ ] Capture owns only temporary resources;
* [ ] Capture does not perform OCR-specific preprocessing;
* [ ] Capture events do not drive Recognition directly;
* [ ] operation lifecycle events do not duplicate Runtime;
* [ ] retry is classified by Capture but executed by Runtime;
* [ ] ProviderTimeout and Runtime deadline remain distinct;
* [ ] privacy scope cannot silently expand;
* [ ] raw data is memory-only by default;
* [ ] all six Capture documents agree on ownership and terminology.

---

# 113. Summary

Capture v2 is CRAI's acquisition boundary.

Its processing flow is:

```text
Business Pipeline Orchestration
        ↓
Runtime Control
        ↓
Capture Attempt
        ↓
CaptureInvocation
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
Recognition when required
```

Its source model is:

```text
CaptureSourceId
+
SourceVersion
+
CaptureSourceState
+
CaptureProvider
```

Its ownership boundary is:

```text
Capture
    owns acquisition semantics

Runtime
    owns execution authority

CaptureProvider
    owns native acquisition

Artifact Store
    owns accepted frame lifetime

Recognition
    owns content interpretation
```

The central invariant is:

```text
Capture may produce
a valid Candidate.

Only Runtime-authoritative work
may turn that Candidate
into an accepted Artifact.
```
