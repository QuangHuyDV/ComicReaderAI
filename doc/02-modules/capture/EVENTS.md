# Capture Events

> **Project:** CRAI
> **Module:** `capture`
> **Path:** `doc/02-modules/capture/EVENTS.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the event boundary of the Capture module.

It specifies:

```text id="dvq0gx"
Capture-owned facts
event ownership
canonical event envelope
CaptureSource event semantics
Capture health event semantics
event ordering
idempotency
delivery assumptions
event publication timing
privacy
observability
```

Capture events describe committed facts about:

```text id="qz2fj5"
CaptureSource lifecycle
CaptureSource availability
SourceVersion changes
Capture permission/capability state
Capture health
```

This document does not define:

```text id="ku9tdr"
Runtime Attempt lifecycle events
WorkItem lifecycle events
Runtime cancellation events
Runtime timeout events
Runtime retry events
Candidate Capture completion transport
Artifact publication events
Recognition orchestration
Reading Session orchestration
```

---

# 2. Core Event Principle

An event describes:

> A fact that has already become true.

It does not request:

> Start another processing module.

Correct:

```text id="q3a80n"
CaptureSourceReady
CaptureSourceChanged
CaptureSourceUnavailable
CaptureSourceSuspended
CaptureSourceStopped
CaptureHealthChanged
```

Incorrect:

```text id="k93vku"
RunRecognition
StartNextCapture
RetryCapture
TranslateFrame
```

Those are commands or orchestration decisions.

---

# 3. Event Bus Is Not the Processing Pipeline

Invalid architecture:

```text id="97pi3r"
CaptureFrameReady
    ↓
Recognition subscribes
    ↓
Recognition starts
```

Required architecture:

```text id="2d6c0y"
Capture
    ↓
CandidateCaptureResult
    ↓
Runtime Completion Validation
    ↓
accepted CapturedFrameArtifact
    ↓
Business Pipeline / Runtime decision
    ↓
Recognition invocation
```

Capture events do not replace this flow.

---

# 4. Event Ownership

Capture publishes only Capture-owned facts.

Examples:

```text id="yu5rir"
CaptureSourceReady
    → Capture

CaptureHealthChanged
    → Capture

AttemptCancelled
    → Runtime

CapturedFrameArtifactPublished
    → Runtime / Artifact publication owner

RecognitionCompleted
    → Recognition / Runtime completion boundary

ReadingContextChanged
    → Reading Session
```

Capture MUST NOT publish another module's lifecycle facts.

---

# 5. Event Categories

Capture v2 has two primary public event categories:

```text id="okrihp"
Capture Events
├── CaptureSource Events
└── CaptureHealth Events
```

Optional provider/capability facts may be exposed if consumers genuinely require them.

There is no public Capture-owned operation-terminal event family.

---

# 6. Removed Operation Event Family

The following v1 events are removed from Capture-owned public events:

```text id="n88jw0"
CaptureStarted
CaptureCompleted
CaptureCancelled
CaptureTimeout
CaptureFailed
CaptureFrameReady
```

Reason:

```text id="ptfq8u"
CaptureOperation progress
    → local processing phase / tracing

Capture Attempt completion
    → Runtime completion boundary

Cancellation
    → Runtime-owned

Runtime timeout
    → Runtime-owned

Capture Candidate
    → direct completion result, not event orchestration

Accepted frame
    → Artifact publication boundary
```

---

# 7. Why `CaptureFrameReady` Is Removed

`CaptureFrameReady` previously acted as both:

```text id="ef4zsj"
data availability event
+
Recognition trigger
```

That creates hidden orchestration.

v2 separates:

```text id="yivthc"
CandidateCaptureResult
    → Capture → Runtime completion

CapturedFrameArtifact
    → Runtime/Artifact Store accepted output

Recognition
    → invoked only with accepted Artifact reference
```

Therefore Capture does not publish `CaptureFrameReady`.

---

# 8. Candidate Capture Results Are Not Events

`CandidateCaptureResult` is returned through the processing completion contract.

It is not published to the general Event Bus.

Reasons:

* Candidate may still be rejected by Runtime;
* Candidate may contain temporary resource references;
* Candidate is not globally authoritative;
* downstream modules must never consume unpublished Candidates.

---

# 9. Artifact Publication Events

If CRAI exposes an event such as:

```text id="wrrzx9"
CapturedFrameArtifactPublished
```

its owner is:

```text id="te4ctx"
Runtime / Artifact publication boundary
```

not Capture.

Capture may appear in provenance:

```text id="r88gyg"
producerModule = capture
```

without owning publication state.

---

# 10. Canonical Event Envelope

Capture events follow the CRAI Event Convention.

Conceptually:

```text id="n3l20x"
EventEnvelope
├── eventId
├── eventType
├── eventVersion
├── occurredAt
├── producer
├── correlationId?
├── causationId?
├── traceId?
├── runtimeIdentity?
├── payload
└── metadata?
```

Canonical architecture remains authoritative if exact field names differ.

Capture MUST NOT define a competing global envelope.

---

# 11. Producer

Capture-owned public facts use:

```text id="idgo5h"
producer = capture
```

---

# 12. Runtime Identity Metadata

Capture events may include bounded Runtime identity where the source/health change occurred during Runtime processing:

```text id="55pdip"
sessionId?
runtimeRevisionId?
workItemId?
attemptId?
```

These are correlation values only.

Capture does not own them.

---

# 13. Capture-Owned Identity

Capture events may carry:

```text id="1yz2b0"
captureSourceId
sourceVersion
captureSourceKind
providerDomain?
```

where relevant.

---

# 14. Event Publication Timing

Capture-owned state facts follow:

```text id="mrnvin"
validate transition
    ↓
commit Capture-owned state
    ↓
publish event
```

Never:

```text id="t0ew13"
publish success event
    ↓
change source state later
```

---

# 15. CaptureSource Event Set

Recommended core events:

```text id="yc3nvt"
CaptureSourceReady
CaptureSourceChanged
CaptureSourceSuspended
CaptureSourceUnavailable
CaptureSourceStopped
```

Optional:

```text id="q6159e"
CapturePermissionStateChanged
CaptureCapabilityChanged
```

only if consumers require separate facts.

---

# 16. CaptureSourceReady

## Meaning

A CaptureSource successfully entered:

```text id="48skjj"
READY
```

and may accept Capture operations.

Typical transition:

```text id="yhdkbi"
INITIALIZING → READY
```

## Payload

```text id="p2ddii"
CaptureSourceReadyPayload
├── captureSourceId
├── sourceVersion
├── sourceKind
├── capabilitySummary
├── permissionSummary?
└── readyAt
```

## Invariants

* source state committed before publication;
* event does not mean a Capture Attempt started;
* event does not mean an Artifact exists.

---

# 17. CaptureSourceChanged

## Meaning

A committed CaptureSource semantic configuration changed.

Typical causes:

```text id="ezhp34"
region changed
logical provider source replaced
capabilities materially changed
provider-native resource re-established
source descriptor changed
```

Typically:

```text id="n3o7ue"
SourceVersion N
    ↓
SourceVersion N+1
```

## Payload

```text id="25r0oa"
CaptureSourceChangedPayload
├── captureSourceId
├── previousSourceVersion
├── sourceVersion
├── sourceKind
├── changeSet
├── capabilitySummary?
└── changedAt
```

---

# 18. CaptureSourceChangeSet

Conceptually:

```text id="6fgzqs"
CaptureSourceChangeSet
├── regionChanged
├── descriptorChanged
├── capabilityChanged
├── providerAssociationChanged
├── permissionChanged
└── otherSemanticChange?
```

This describes source semantics.

It does not tell Runtime what WorkItems to cancel or create.

---

# 19. CaptureSourceSuspended

## Meaning

CaptureSource still exists but must not accept new Capture acquisitions.

Typical transition:

```text id="qbho0v"
READY → SUSPENDED
```

## Payload

```text id="4bn4er"
CaptureSourceSuspendedPayload
├── captureSourceId
├── sourceVersion
├── reasonCode
└── suspendedAt
```

Possible reasons:

```text id="1l5886"
UserRequested
TemporaryPolicy
TemporaryPermissionCondition
ProviderPaused
ApplicationSuspended
```

This event does not mean Runtime Attempts are canceled.

---

# 20. CaptureSourceUnavailable

## Meaning

CaptureSource cannot currently serve Capture.

Typical transitions:

```text id="uibpcv"
INITIALIZING → UNAVAILABLE
READY → UNAVAILABLE
SUSPENDED → UNAVAILABLE
```

## Payload

```text id="r18pz1"
CaptureSourceUnavailablePayload
├── captureSourceId
├── sourceVersion
├── reasonCode
├── recoverability
├── capabilitySummary?
├── permissionSummary?
└── unavailableAt
```

Typical reasons:

```text id="zh99yi"
SourceLost
PermissionUnavailable
ProviderUnavailable
ConnectorDisconnected
DisplayDisconnected
SourceReferenceInvalid
```

---

# 21. CaptureSourceStopped

## Meaning

A CaptureSource reached terminal state:

```text id="fyk5sm"
STOPPED
```

Typical transition:

```text id="snbem9"
STOPPING → STOPPED
```

## Payload

```text id="x2rf4m"
CaptureSourceStoppedPayload
├── captureSourceId
├── lastSourceVersion
├── reasonCode
└── stoppedAt
```

This does not claim Runtime Attempts have all transitioned terminally.

---

# 22. Source Creation Event

A separate:

```text id="wla7bb"
CaptureSourceCreated
```

is optional.

For MVP, `CaptureSourceReady` is usually more useful because an uninitialized source that cannot become READY provides limited integration value.

If creation itself must be observed, it may be added without changing source authority semantics.

---

# 23. Removed `CaptureSourceActivated`

The old event:

```text id="j0b3u0"
CaptureSourceActivated
```

is removed because `ACTIVE` source state was removed.

`READY` now means:

```text id="bdykto"
source can serve Capture
```

independent of whether a Runtime Attempt is currently invoking it.

---

# 24. Removed `CaptureSourceResumed`

A separate resumed fact is unnecessary by default.

Transition:

```text id="5kssj3"
SUSPENDED → READY
```

may publish:

```text id="s9zq5e"
CaptureSourceReady
```

with a reason/change metadata if consumers need to distinguish recovery.

A future dedicated `CaptureSourceResumed` may be introduced only if it has distinct consumer semantics.

---

# 25. Removed `CaptureSourceRemoved`

The preferred terminal fact is:

```text id="pjxv40"
CaptureSourceStopped
```

because removal is a command/cause, while stopped is the resulting committed fact.

---

# 26. Permission Event Policy

Permission is primarily platform/provider state normalized into CaptureSource availability.

Prefer representing permission effect through:

```text id="rgi3jg"
CaptureSourceUnavailable
CaptureSourceReady
CaptureHealthChanged
```

rather than a mandatory global permission event family.

---

# 27. Optional CapturePermissionStateChanged

If consumers explicitly need permission-state visibility:

```text id="w6jz15"
CapturePermissionStateChangedPayload
├── captureSourceId?
├── permissionKind
├── previousState
├── state
├── reasonCode?
└── changedAt
```

Possible normalized states:

```text id="k1ad58"
Unknown
Granted
Denied
Revoked
Restricted
```

Native permission tokens/objects must never appear.

---

# 28. Permission Fact Ownership

Capture may own the normalized Capture-facing permission fact.

Platform adapter owns:

```text id="agx0ge"
actual OS permission API state
```

Capture event describes only what Capture now understands about its ability to use the source.

---

# 29. Capture Capability Event

Optional:

```text id="yjrax6"
CaptureCapabilityChanged
```

when normalized provider/source capabilities materially change.

Payload:

```text id="e2xpa3"
CaptureCapabilityChangedPayload
├── captureSourceId
├── sourceVersion
├── previousCapabilities
├── capabilities
└── changedAt
```

Avoid emitting for transient provider telemetry.

---

# 30. Capture Health Event Set

Core:

```text id="gmjbhz"
CaptureHealthChanged
```

---

# 31. CaptureHealthChanged

## Meaning

Capture-owned health state changed.

## Payload

```text id="76kqmh"
CaptureHealthChangedPayload
├── previousHealthState
├── healthState
├── captureSourceId?
├── reasonCode?
├── observedMetricsSummary?
└── changedAt
```

Health states:

```text id="efhylg"
Healthy
Degraded
Unavailable
Recovering
Stopped
```

---

# 32. Health Event Does Not Command Runtime

`CaptureHealthChanged` is informational.

Runtime/Application may use it as input to policy.

Capture does not expect the event itself to mutate Runtime state.

---

# 33. Operation Completion Boundary

Normal successful Capture execution is:

```text id="ov5byo"
CaptureOperation
    ↓
CaptureCompletion
    ↓
Runtime
```

not:

```text id="u7mgi0"
CaptureCompleted Event
```

The Runtime processing contract is the authoritative completion channel.

---

# 34. Operation Progress Observability

Capture may emit traces/spans/metrics for local phases:

```text id="8rnjz4"
VALIDATING
ACQUIRING
NORMALIZING
VALIDATING_CANDIDATE
COMPLETING
```

These are observability data.

They should not become stable Event Bus integration contracts.

---

# 35. Capture Failure Boundary

Capture-owned failure is returned in:

```text id="quu86i"
CaptureCompletion
```

with normalized:

```text id="c4hv3x"
CaptureError
RetryClassification
RecoveryHint
```

Runtime decides:

```text id="yg5m0b"
Attempt outcome
retry
supersession
```

No mandatory `CaptureFailed` Event Bus event is required.

---

# 36. Why `CaptureFailed` Is Removed

A public `CaptureFailed` event would duplicate:

```text id="ebnh28"
Runtime processing completion
+
CaptureError contract
```

and creates ambiguity around retries:

```text id="zra1x5"
Did the WorkItem fail?
Did only one Attempt fail?
Will Runtime retry?
```

Therefore Runtime completion/observability is the canonical path.

---

# 37. Why `CaptureCancelled` Is Removed

Cancellation belongs to Runtime.

Capture may observe:

```text id="27tcji"
CancellationContext
```

and abort local processing.

That does not make Capture the owner of:

```text id="6vkkxb"
AttemptCancelled
```

---

# 38. Why `CaptureTimeout` Is Removed

There are two different timeout domains:

```text id="hsfrik"
ProviderTimeout
RuntimeDeadlineExceeded
```

Provider timeout is a Capture error returned through CaptureCompletion.

Runtime deadline is Runtime-owned.

One generic `CaptureTimeout` event would blur those boundaries.

---

# 39. Event Ordering

Capture guarantees ordering only for transitions it owns.

Example:

```text id="wyvmwk"
CaptureSourceChanged version 7
    ↓
CaptureSourceUnavailable version 7
    ↓
CaptureSourceReady version 8
```

Consumers should use:

```text id="qj0nej"
captureSourceId
SourceVersion
event causality
```

rather than global event ordering.

---

# 40. No Per-Operation Event Ordering Contract

The old:

```text id="i7k7dg"
CaptureStarted
→ CaptureFrameReady
→ CaptureCompleted
```

ordering contract is removed.

Capture operation completion is a direct Runtime processing contract, not Event Bus lifecycle.

---

# 41. Continuous Capture Ordering Removed

There is no Capture-owned:

```text id="nq7fhm"
continuous event sequence
```

such as repeated:

```text id="9x8xfb"
Started
FrameReady
Completed
Started
FrameReady
Completed
```

Repeated capture scheduling belongs to Runtime.

---

# 42. Source Replacement Ordering

Preferred semantics:

```text id="d0lzmc"
CaptureSource S v4 READY
        ↓
Replace source
        ↓
CaptureSourceChanged S v5
        ↓
CaptureSourceReady S v5
```

Depending on provider mechanics, consumers may also observe temporary:

```text id="05518n"
CaptureSourceUnavailable
```

or Suspended.

There is no mandatory:

```text id="7uuf2u"
CaptureCancelled
```

event in this sequence.

Runtime handles outstanding execution separately.

---

# 43. Runtime Supersession Ordering

Do not assume:

```text id="z2mwuj"
RuntimeRevisionSuperseded
must occur before
CaptureSourceChanged
```

or vice versa globally.

They belong to different owners.

Use causation and owner-specific versions.

---

# 44. No Global Ordering

Capture does not guarantee total order across:

```text id="hql32d"
Capture events
Runtime events
Reading Session events
Artifact events
Recognition events
Presentation events
```

---

# 45. Event Delivery Semantics

Delivery guarantee belongs to canonical Event Bus/runtime profile.

Capture MUST NOT hard-code:

```text id="f4nm5r"
AtLeastOnce
```

as a universal module guarantee.

Possible Runtime profiles may provide different delivery semantics.

Consumers must follow canonical Event Bus rules.

---

# 46. Idempotency

When delivery can duplicate events:

```text id="ggvjbl"
EventId
```

must support deduplication.

Source consumers should also compare:

```text id="ln54vv"
captureSourceId
sourceVersion
eventType
```

where relevant.

---

# 47. Stale Source Event

A consumer that has already observed:

```text id="wy5w8h"
SourceVersion 10
```

must not let:

```text id="zhq5ya"
late SourceVersion 9
```

overwrite current CaptureSource state.

---

# 48. Event Identity

Capture events follow canonical identity fields.

Typical:

```text id="s01gja"
eventId
eventType
eventVersion
occurredAt
correlationId?
causationId?
traceId?
captureSourceId?
sourceVersion?
```

No `FrameId` is required for public Capture-owned events because frame output is no longer distributed via Capture Event Bus events.

---

# 49. Removed `GenerationId`

Capture events MUST NOT use:

```text id="wmrgej"
GenerationId
```

as stale-result authority.

Runtime uses its own execution identities.

CaptureSource uses `SourceVersion` for source semantics.

---

# 50. Event Size

Events must remain small.

Do not include:

```text id="itc1jw"
raw image
screenshot
pixel buffer
CandidateCaptureResult
CapturedFrameArtifact payload
OCR output
Translation output
provider response
native handle
```

---

# 51. Event Payload References

Where a Capture-owned fact requires an external reference, prefer:

```text id="ykuavk"
opaque source ID
capability summary
diagnosticRef
```

Do not expose temporary Candidate frame references through Capture events.

---

# 52. Event Lifetime

Capture source/health events are facts and may be retained/replayed according to Event Bus policy.

They should not be modeled as ephemeral “discard after CaptureCompleted” operation messages.

Current state should still be obtained through queries when necessary.

---

# 53. Events vs Queries

Events answer:

> What Capture-owned fact occurred?

Queries answer:

> What is Capture state now?

Typical queries:

```text id="s0b64k"
GetCaptureSource
GetCaptureCapabilities
GetCaptureHealth
```

Consumers needing current state should not rely only on historical event delivery.

---

# 54. Event Publication Failure

Capture-owned state commit and event publication are separate.

Example:

```text id="jzhl5c"
CaptureSource becomes UNAVAILABLE
        ↓
state committed
        ↓
CaptureSourceUnavailable publication fails
```

The source remains:

```text id="w9nx2u"
UNAVAILABLE
```

Do not revert state merely to recreate the event.

---

# 55. Publication Failure Recovery

Infrastructure may use:

```text id="7wlpel"
outbox
retry publication
state-query reconciliation
diagnostic alert
```

Capture does not repeat source mutation solely to publish the event again.

---

# 56. External Events Consumed by Capture

Capture v2 has no mandatory direct business-event subscription set.

The previous direct subscriptions to:

```text id="g0osbe"
ReadingSessionStarted
ReadingSessionPaused
ReadingSessionResumed
ReadingSessionStopped
ReadingGenerationChanged
RuntimeShutdown
ConfigurationChanged
```

are removed as Capture correctness requirements.

---

# 57. Why Reading Session Events Are Not Direct Inputs

Invalid:

```text id="7vac7v"
ReadingSessionPaused
    ↓
Capture auto-suspends itself
```

Preferred:

```text id="71bwrh"
ReadingSessionPaused
        ↓
Application / orchestration policy
        ↓
explicit CaptureSource/Runtime action if needed
```

This preserves ownership and makes cross-module policy explicit.

---

# 58. Runtime Shutdown

Shutdown is integrated through explicit lifecycle/composition-root control.

Capture does not require a generic `RuntimeShutdown` Event Bus subscription as its correctness mechanism.

Application/Runtime may explicitly call Capture shutdown/source-stop contracts.

---

# 59. Configuration Change

Capture receives immutable configuration snapshots through commands/runtime context.

It does not require direct subscription to global:

```text id="a8ht59"
ConfigurationChanged
```

events.

New execution uses the appropriate resolved configuration snapshot.

---

# 60. Optional Infrastructure Subscriptions

An implementation may use infrastructure events internally for optimization only if:

1. ownership remains external;
2. the subscription is replaceable;
3. Capture public correctness does not depend on hidden workflow;
4. explicit state/command contracts remain authoritative;
5. tests can run without the subscription.

---

# 61. Event Security

Capture events MUST NOT contain:

```text id="5rkz3v"
raw image
screenshot
secret
token
native handle
memory address
provider object
browser privileged object
OS permission token
```

---

# 62. Source Metadata Privacy

Source identifiers and titles may themselves be sensitive.

Prefer:

```text id="dbxt6f"
opaque CaptureSourceId
sourceKind
bounded redacted metadata
```

over full:

```text id="flqdui"
window titles
URLs
filesystem paths
application document content
```

---

# 63. Observability vs Event Contract

Capture operation details belong primarily in telemetry.

Examples:

```text id="fkqe1d"
capture_operation_started_total
capture_operation_duration_ms
capture_provider_failure_total
capture_candidate_discard_total
capture_cancellation_observed_total
```

These do not require public events such as `CaptureStarted`.

---

# 64. Capture Diagnostics Consumers

Diagnostics/Telemetry should use:

```text id="ygjl7t"
traces
metrics
structured logs
Runtime observability
```

for operation-level details.

Public Event Bus should remain focused on stable cross-module facts.

---

# 65. Source Event Consumers

Potential consumers:

```text id="0ol3w8"
Application
Runtime policy
Diagnostics
UI coordination
Source selection UI
Persistence projection
```

They consume facts, not commands.

---

# 66. Recognition Integration

Recognition MUST NOT subscribe to:

```text id="n7b3xe"
CaptureFrameReady
```

because that event no longer exists.

Correct integration:

```text id="6ytphd"
CapturedFrameArtifactRef
    ↓
Runtime/Business Pipeline
    ↓
Recognition invocation
```

---

# 67. Reading Session Integration

Reading Session SHOULD NOT directly subscribe to Capture operational events.

Capture source availability may be shown to Application/UI or used by orchestration policy.

Reading Session remains the authority for the user's reading-domain state.

---

# 68. Runtime Integration

Runtime may consume:

```text id="0g2ggx"
CaptureHealthChanged
CaptureSourceUnavailable
```

as policy inputs where useful.

Capture processing completion still uses Runtime completion contracts rather than Event Bus terminal events.

---

# 69. MVP Published Events

Recommended v2 MVP:

```text id="scf1xe"
CaptureSourceReady
CaptureSourceChanged
CaptureSourceSuspended
CaptureSourceUnavailable
CaptureSourceStopped
CaptureHealthChanged
```

Optional only if product/runtime consumers require them:

```text id="098n00"
CapturePermissionStateChanged
CaptureCapabilityChanged
```

---

# 70. MVP Direct Consumed Events

```text id="5s5h2l"
None required.
```

Capture is invoked through explicit contracts.

---

# 71. Deferred Events

Potential future facts:

```text id="pzdv6o"
CaptureProviderChanged
CaptureCapabilityChanged
CapturePermissionStateChanged
CaptureSourceRecovered
CaptureStreamHealthChanged
```

Add only when there is a real cross-module consumer.

Do not add them merely because an internal state exists.

---

# 72. Deprecated v1 Events

Removed/deprecated:

```text id="ay34eg"
CaptureStarted
CaptureCompleted
CaptureCancelled
CaptureTimeout
CaptureFailed
CaptureFrameReady
CaptureSourceActivated
CaptureSourceResumed
CaptureSourceRemoved
CapturePermissionGranted
CapturePermissionRevoked
```

Replacement semantics:

```text id="kr2cpc"
operation execution
    → Runtime completion + telemetry

frame availability
    → accepted Artifact publication

source usability
    → CaptureSourceReady / Unavailable / Suspended / Stopped

permission
    → normalized source state or optional permission-state event
```

---

# 73. Testing — Ownership

Tests MUST verify Capture does not publish:

```text id="n25hxu"
AttemptCancelled
RuntimeTimeout
RetryScheduled
CapturedFrameArtifactPublished
RecognitionStarted
ReadingSessionPaused
```

---

# 74. Testing — No Frame Event Orchestration

Verify:

```text id="co6e63"
CandidateCaptureResult produced
```

does not result in direct Event Bus delivery to Recognition.

Recognition may start only after accepted Artifact publication and orchestration.

---

# 75. Testing — Source Events

Verify:

```text id="4s0ebq"
INITIALIZING → READY
    → CaptureSourceReady

READY → SUSPENDED
    → CaptureSourceSuspended

READY → UNAVAILABLE
    → CaptureSourceUnavailable

STOPPING → STOPPED
    → CaptureSourceStopped
```

Event fires after state commit.

---

# 76. Testing — Source Version

Verify late lower-SourceVersion events cannot overwrite newer consumer state.

---

# 77. Testing — Publication Failure

Verify:

```text id="klh9nq"
CaptureSource state commits
event publication fails
```

does not revert CaptureSource state.

---

# 78. Testing — Runtime Independence

Verify:

```text id="pmcs0h"
Runtime Attempt canceled
```

does not require Capture to publish `CaptureCancelled`.

Verify Runtime timeout does not require `CaptureTimeout`.

---

# 79. Testing — Subscription Independence

Capture core tests must succeed without subscriptions to:

```text id="sy251i"
Reading Session
RuntimeShutdown
ConfigurationChanged
Recognition
Translation
```

---

# 80. Testing — Privacy

Verify event payloads do not contain:

* raw image;
* frame buffer refs;
* native handles;
* provider objects;
* secrets;
* full source titles/URLs where unnecessary.

---

# 81. Event Compatibility

Each event has its own version.

Major change required when:

* event meaning changes;
* ownership changes;
* required field removed;
* SourceVersion semantics change incompatibly.

Compatible optional fields may be added under canonical Event Convention rules.

---

# 82. Architecture Invariants

1. Capture events describe Capture-owned facts only.

2. Events describe facts, not commands.

3. Event Bus is not the processing workflow engine.

4. Capture does not publish Runtime Attempt lifecycle events.

5. Capture does not publish Runtime cancellation events.

6. Capture does not publish Runtime deadline events.

7. Capture does not publish Runtime retry events.

8. Capture does not publish accepted Artifact publication facts.

9. CandidateCaptureResult is not an Event Bus payload.

10. Recognition never subscribes directly to Capture Candidate availability.

11. `CaptureFrameReady` is removed.

12. `GenerationId` is removed from Capture event authority.

13. SourceVersion is Capture-owned.

14. SourceVersion orders source semantics only.

15. CaptureSource events publish after state commit.

16. CaptureHealthChanged does not directly mutate Runtime.

17. Reading Session events are not mandatory Capture subscriptions.

18. Configuration events are not mandatory Capture subscriptions.

19. Runtime shutdown is not implemented as hidden Event Bus dependency.

20. Capture operation progress belongs to telemetry/direct completion.

21. No global event ordering is assumed.

22. Event delivery guarantees come from canonical Event Bus profile.

23. Duplicate delivery must be tolerable.

24. Event publication failure does not revert valid state.

25. Public event payloads are bounded and privacy-safe.

---

# 83. Related Documents

```text id="ps2mil"
doc/02-modules/capture/MODULE.md
doc/02-modules/capture/CONTRACT.md
doc/02-modules/capture/STATES.md
doc/02-modules/capture/ERRORS.md
doc/02-modules/capture/README.md

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
doc/01-architecture/core/STATE_MACHINE.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/02-modules/recognition/EVENTS.md
```

---

# 84. Completion Criteria

This event specification is synchronized when:

* operation execution events no longer duplicate Runtime lifecycle;
* `CaptureFrameReady` is removed;
* Recognition is no longer triggered directly by Capture Event Bus events;
* Candidate output flows through Runtime completion;
* accepted Artifact publication remains external;
* GenerationId is absent;
* source/health facts remain Capture-owned;
* SourceVersion is used only for source semantics;
* direct Reading Session subscriptions are removed;
* delivery guarantees defer to canonical Event Bus;
* event publication follows state commit;
* operation observability moves to telemetry;
* privacy-sensitive frame references are absent from events;
* tests verify ownership and no hidden orchestration.

---

# 85. Summary

Capture v2 has two distinct communication paths.

Processing result path:

```text id="ybtvnu"
CaptureOperation
    ↓
CandidateCaptureResult
    ↓
Runtime Completion
    ↓
Authority Validation
    ↓
CapturedFrameArtifact
    ↓
Business Pipeline / Runtime
    ↓
Recognition
```

Event path:

```text id="9q7n5y"
Capture-owned state change
    ↓
state commit
    ↓
CaptureSource / Health Fact
    ↓
Event Bus
```

The central rule is:

```text id="qf6n72"
Capture results travel through
Runtime processing contracts.

Capture events describe
Capture-owned state facts.

Events must never become
the hidden processing pipeline.
```
