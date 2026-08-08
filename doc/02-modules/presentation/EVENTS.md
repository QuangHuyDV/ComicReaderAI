# Presentation Events

> **Project:** CRAI
> **Module:** `presentation`
> **Path:** `doc/02-modules/presentation/EVENTS.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-08

---

# 1. Purpose

This document defines the event boundary of the Presentation module.

It specifies:

* Presentation-owned facts;
* event ownership;
* canonical envelope requirements;
* event payload semantics;
* PresentationRevision rules;
* event ordering;
* idempotency;
* commit/event relationship;
* rejection and failure facts;
* external integration boundaries;
* event privacy and observability rules.

Presentation events describe facts about:

```text
Presentation semantic state
Presentation commit
Presentation layout
Presentation mode
Presentation clear
Presentation rejection
Presentation failure
```

This document does not define:

* Presentation commands;
* Runtime WorkItem events;
* Runtime Attempt events;
* Runtime authority events;
* Translation execution events;
* Reading Session lifecycle;
* native UI lifecycle;
* Event Bus implementation.

Commands belong to:

```text
doc/02-modules/presentation/CONTRACT.md
```

Presentation state transitions belong to:

```text
doc/02-modules/presentation/STATES.md
```

---

# 2. Core Event Principle

An event describes:

> **A fact that already became true.**

An event does not express:

> **Work that should now be performed.**

Correct examples:

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
PresentationCleared
PresentationRejected
PresentationFailed
```

Incorrect event-as-command examples:

```text
BuildPresentation
RefreshPresentation
RecomputeLayout
RenderNow
ClearNow
```

Those are commands or application intents.

---

# 3. Event Ownership

Every event belongs to the owner of the fact.

Examples:

```text
RuntimeRevisionSuperseded
    → Runtime

TranslationArtifactPublished
    → Runtime / Artifact publication boundary

PresentationPrepared
    → Presentation

PresentationApplyFailed
    → UI Adapter

PreferenceProfileChanged
    → Preferences
```

Presentation MUST NOT publish facts claiming that it owns:

* Runtime Revision lifecycle;
* WorkItem lifecycle;
* Attempt lifecycle;
* Translation execution;
* Artifact publication;
* Reading Session lifecycle;
* persistent preference update;
* actual native rendering completion;
* storage completion.

---

# 4. Event Bus Is Not the Workflow Engine

CRAI does not use Presentation events or external events as hidden orchestration.

Invalid architecture:

```text
TranslationCompleted
    ↓
Presentation subscribes
    ↓
BuildPresentation automatically
```

Required architecture:

```text
Accepted Artifact / External Fact
        ↓
Application / Runtime / Integration Boundary
        ↓
Business Pipeline / explicit decision
        ↓
Presentation Command
        ↓
Presentation
```

Events may inform that decision.

They do not replace it.

---

# 5. Presentation Does Not Own a Consumed Business Event Set

Presentation v2 has no mandatory set of external business events that must directly mutate Presentation state.

External facts may be observed by:

```text
Application
Runtime
Integration Router
UI coordination layer
```

Those components may translate relevant facts into explicit Presentation commands.

Presentation correctness MUST NOT depend on direct Event Bus subscriptions to:

```text
TranslationUpdated
TranslationCompleted
SessionContentAccepted
ViewportChanged
PreferenceChanged
ProfileChanged
```

---

# 6. External Integration Facts

The following facts may be relevant to Presentation integration but are **not Presentation-owned commands or state transitions**.

Examples:

```text
Runtime Revision / authority facts
Artifact publication facts
Reading Session facts
Preference facts
UI Adapter geometry facts
```

The exact event names belong to their owning modules.

Presentation documentation may reference those facts only to explain integration.

---

# 7. Example Integration — Translation Result

Preferred model:

```text
Translation Candidate
    ↓
Runtime Authority Validation
    ↓
Translation Artifact published
    ↓
Business Pipeline / Application determines Presentation is needed
    ↓
BuildPresentation
or
UpdatePresentationContent
```

Presentation does not need a `TranslationCompleted` event to discover that work exists.

---

# 8. Example Integration — Viewport Change

UI Adapter may observe:

```text
Viewport change
```

It may report a normalized fact or invoke an application port.

Application/Presentation integration may then issue:

```text
RecomputePresentationLayout
```

Presentation must not depend on raw UI events or operating-system callbacks.

---

# 9. Example Integration — Preference Change

Preferences may publish a fact that resolved Presentation preferences changed.

Application may determine whether the impact requires:

```text
ApplyPresentationProfile
RecomputePresentationLayout
ChangePresentationMode
```

Presentation does not persist or own preference lifecycle.

---

# 10. Common Event Envelope

Presentation events follow the canonical CRAI Event Convention.

Conceptually:

```text
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

The canonical architecture event envelope remains authoritative if field names differ.

Presentation MUST NOT invent a competing global event-envelope definition.

---

# 11. EventId

```text
EventId
```

uniquely identifies one published event instance.

Rules:

* immutable;
* globally unique within the required event identity domain;
* duplicate delivery of the same EventId must not create duplicate logical effects.

---

# 12. EventType

Canonical Presentation-owned names include:

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
PresentationCleared
PresentationRejected
PresentationFailed
```

---

# 13. EventVersion

Each event contract has its own version.

Example:

```text
2.0
```

Event schema compatibility follows the canonical Event Convention.

Presentation module contract version and individual event version are related but not necessarily identical values.

---

# 14. Producer

Presentation-owned events declare:

```text
producer = presentation
```

A Presentation event MUST NOT impersonate:

```text
runtime
translation
reading-session
preferences
ui-adapter
storage
```

---

# 15. Correlation and Causation

Presentation events may include:

```text
correlationId
causationId
traceId
```

Typical causation:

```text
Presentation Command
    ↓
Presentation Operation
    ↓
Presentation Commit
    ↓
Presentation Event
```

The event should be traceable to the operation that caused it without embedding mutable operation state.

---

# 16. Runtime Identity Metadata

Where Presentation work was invoked through Runtime, events may carry bounded Runtime identity:

```text
RuntimeEventIdentity
├── sessionId
├── runtimeRevisionId
├── workItemId?
├── attemptId?
└── configurationSnapshotRef?
```

Rules:

1. Presentation may report these values.
2. Presentation does not own them.
3. Their presence does not make the event authoritative for Runtime lifecycle.
4. Presentation events MUST NOT change Runtime state.

---

# 17. Presentation-Owned Identity

Presentation events may carry:

```text
presentationContextId
presentationId
presentationRevision
operationId?
requestId?
```

Presentation owns:

```text
PresentationContext state
PresentationId
PresentationRevision
Presentation Operation identity
```

It does not own Runtime Revision.

---

# 18. PresentationRevision Rule

Every successful committed Presentation mutation event MUST carry:

```text
presentationRevision
```

Update-like events SHOULD also carry:

```text
previousPresentationRevision
```

PresentationRevision:

* increases only after committed visible state changes;
* never decreases;
* does not increase for rejected Candidates;
* does not increase for superseded operations;
* does not increase for Runtime authority rejection;
* does not increase for no-op operations.

---

# 19. Commit Before Event

Successful Presentation events follow:

```text
Candidate
    ↓
Presentation validation
    ↓
Runtime authority revalidation
    ↓
PresentationRevision validation
    ↓
Atomic Presentation Commit
    ↓
Presentation state transition
    ↓
Success Event
```

Never:

```text
Success Event
    ↓
Commit later
```

---

# 20. Candidate Events

Presentation does not publish a normal success event merely because a Candidate exists.

For example, v2 does not require:

```text
PresentationCandidateCreated
PresentationCandidateValidated
```

as business integration events.

Candidate observations may appear in:

* traces;
* diagnostics;
* internal telemetry;

but are not required for business correctness.

---

# 21. PresentationPrepared

## Meaning

A new Presentation became the current committed Presentation for a Presentation Context.

Typical state transition:

```text
PREPARING → READY
```

## Payload

```text
PresentationPreparedPayload
├── presentationContextId
├── presentationId
├── presentationRevision
├── contentIdentity
├── sourceArtifactRefs[]
├── requestedMode
├── effectiveMode
├── completeness
├── targetId
├── targetRevision
├── snapshot
├── renderPlan
├── fallback?
├── runtimeIdentity?
├── operationId?
└── requestId?
```

## Invariants

* Snapshot is immutable.
* RenderPlan is immutable.
* Both share the same PresentationRevision.
* Event occurs only after Presentation commit.
* Input Artifact references are accepted immutable references.
* Event does not claim that UI Adapter successfully rendered it.

---

# 22. PresentationUpdated

## Meaning

A committed Presentation changed semantically or in Presentation-level visible data.

Typical transition:

```text
UPDATING → READY
```

## Payload

```text
PresentationUpdatedPayload
├── presentationContextId
├── presentationId
├── previousPresentationRevision
├── presentationRevision
├── contentIdentity
├── sourceArtifactRefs[]
├── changeSet
├── snapshot
├── renderPlan
├── completeness
├── fallback?
├── runtimeIdentity?
└── operationId?
```

## Invariants

* `presentationRevision > previousPresentationRevision`;
* unchanged semantic items preserve stable IDs;
* stale Candidate does not emit this event;
* no Candidate-only state is exposed;
* Snapshot and RenderPlan share one revision.

---

# 23. PresentationChangeSet

Conceptual payload:

```text
PresentationChangeSet
├── addedItemIds[]
├── updatedItemIds[]
├── removedItemIds[]
├── semanticContentChanged
├── layoutChanged
├── styleChanged
├── visibilityChanged
├── focusChanged
├── completenessChanged
└── modeChanged
```

ChangeSet describes already committed changes.

It is not a mutable UI patch command.

---

# 24. PresentationLayoutChanged

## Meaning

The committed framework-neutral layout changed while Presentation semantic content remained compatible.

Typical transition:

```text
REFLOWING → READY
```

## Payload

```text
PresentationLayoutChangedPayload
├── presentationContextId
├── presentationId
├── previousPresentationRevision
├── presentationRevision
├── targetId
├── targetRevision
├── viewportRevision
├── renderPlan
├── changedItemIds[]
├── fallback?
├── runtimeIdentity?
└── operationId?
```

## Invariants

* semantic order remains compatible;
* geometry uses explicit coordinate spaces;
* obsolete viewport result never emits this event;
* event reflects the committed RenderPlan;
* RenderPlan uses the same PresentationRevision.

---

# 25. PresentationModeChanged

## Meaning

The effective committed Presentation mode changed.

Typical transition:

```text
RECONFIGURING → READY
```

## Payload

```text
PresentationModeChangedPayload
├── presentationContextId
├── presentationId
├── previousPresentationRevision
├── presentationRevision
├── requestedMode
├── previousEffectiveMode
├── effectiveMode
├── fallbackApplied
├── fallbackReason?
├── snapshot
├── renderPlan
├── targetId
├── targetRevision
└── operationId?
```

## Invariants

* `effectiveMode` is the committed mode;
* unsupported mode does not replace a valid current mode unless fallback commits;
* source traceability remains preserved;
* Snapshot and RenderPlan agree with effective mode.

---

# 26. Mode Fallback Semantics

A mode fallback does not require a separate:

```text
PresentationModeFallbackApplied
```

event in v2.

Fallback is represented in committed facts such as:

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
```

through:

```text
requestedMode
effectiveMode
fallback
```

This avoids duplicate events describing the same commit.

---

# 27. PresentationCleared

## Meaning

A previously committed Presentation is no longer current for the Presentation Context.

Typical transition:

```text
CLEARING → EMPTY
```

## Payload

```text
PresentationClearedPayload
├── presentationContextId
├── presentationId
├── lastPresentationRevision
├── contentIdentity?
├── reason
├── runtimeIdentity?
└── operationId?
```

Possible reasons:

```text
SessionStopped
SessionReplaced
ContentReplaced
TargetDestroyed
PrivacyInvalidation
ApplicationShutdown
UserRequested
RecoveryReset
```

## Invariants

* logical invalidation occurred before publication;
* old Candidates cannot commit;
* repeated clear while already empty emits no duplicate clear fact;
* event does not claim that native resources have already been physically destroyed.

---

# 28. Logical Clear vs Physical UI Cleanup

`PresentationCleared` means:

```text
Presentation logical state no longer current
```

It does not mean:

```text
native window destroyed
DOM node removed
platform resource fully disposed
```

Those facts belong to UI Adapter/platform owners.

---

# 29. PresentationRejected

## Meaning

A Presentation command, Candidate, or commit attempt was deterministically rejected without Presentation-owned state corruption.

Typical outcomes:

```text
PREPARING → EMPTY
```

or:

```text
transitional state → previous stable READY
```

## Payload

```text
PresentationRejectedPayload
├── requestId?
├── operationId?
├── presentationContextId?
├── presentationId?
├── expectedPresentationRevision?
├── currentPresentationRevision?
├── rejectionSource
├── reasonCode
├── recoverability
├── retryHint?
├── runtimeReasonCode?
├── issues[]
└── runtimeIdentity?
```

---

# 30. RejectionSource

```text
RejectionSource
- PresentationValidation
- PresentationRevision
- RuntimeAuthority
- TargetCompatibility
- ViewportCompatibility
- CancellationObservation
- Supersession
```

This prevents Presentation from redefining external Runtime errors as Presentation-owned errors.

---

# 31. Presentation-Owned Rejection Categories

Typical categories:

```text
InvalidCommand
InvalidMapping
IncompatibleArtifact
InvalidGeometry
InvalidViewport
UnsupportedMode
TargetCapabilityMismatch
InvalidProfile
PresentationRevisionConflict
CandidateInvalid
```

---

# 32. Runtime Authority Rejection

When Runtime rejects a commit:

```text
rejectionSource = RuntimeAuthority
```

Presentation may expose the normalized Runtime reason.

Examples:

```text
RejectedStale
RejectedCanceled
RejectedSessionInactive
RejectedRuntimeRevision
```

Presentation MUST NOT reclassify these as internal Presentation failure.

---

# 33. Supersession Event Policy

A superseded Presentation Candidate does not require `PresentationRejected`.

Default behavior:

```text
discard
+
diagnostics / trace
```

Publish `PresentationRejected` only when the rejection is externally meaningful to a caller or user-visible workflow.

This avoids flooding the Event Bus during normal coalescing.

---

# 34. Cancellation Event Policy

Presentation does not publish:

```text
PresentationCancelled
```

as a Runtime terminal event.

Runtime owns cancellation lifecycle.

Presentation may:

* observe cancellation;
* discard Candidate;
* optionally publish `PresentationRejected` with normalized cancellation source if externally useful.

---

# 35. PresentationFailed

## Meaning

Presentation entered an internal state where Presentation-owned correctness cannot be guaranteed.

Typical transition:

```text
READY → FAILED
```

or:

```text
transitional state → FAILED
```

only when Presentation-owned current state cannot be trusted.

## Payload

```text
PresentationFailedPayload
├── presentationContextId
├── presentationId?
├── presentationRevision?
├── operationId?
├── failureCode
├── failureCategory
├── recoveryState
├── lastKnownGoodAvailable
├── runtimeIdentity?
└── diagnosticRef?
```

---

# 36. Failure Categories

```text
InvariantViolation
PresentationRevisionCorruption
CommittedStateMismatch
RollbackStateCorruption
GeometryStateCorruption
InternalResourceStateCorruption
UnexpectedInternalFailure
```

Do not include Runtime terminal categories such as:

```text
AttemptFailed
RetryExhausted
RuntimeCanceled
RevisionSuperseded
```

---

# 37. Recovery State

```text
RecoveryState
- RestorePossible
- ClearRequired
- ResetRequired
- ApplicationRestartRecommended
```

`PresentationFailed` is not used for ordinary candidate rejection.

---

# 38. UI Apply Events

Presentation does not publish:

```text
PresentationRendered
PresentationDisplayed
```

because actual rendering belongs to UI Adapter.

UI Adapter may own facts such as:

```text
PresentationApplied
PresentationApplyRejected
PresentationApplyFailed
PresentationTargetUnavailable
```

if such events are architecturally required.

These events MUST NOT be published by Presentation.

---

# 39. Presentation Event Subscribers

Typical consumers of Presentation-owned events:

```text
UI Adapter
Application
Diagnostics
Telemetry bridge
```

Consumers use Presentation events as facts.

They MUST NOT mutate Presentation-owned payloads.

---

# 40. UI Adapter Consumption Rule

UI Adapter may consume:

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
PresentationCleared
```

or use a direct Presentation binding/query contract.

If events are used:

* it must compare PresentationRevision;
* stale events must not overwrite newer applied state;
* clear invalidates older Presentation lifecycle output;
* actual UI apply result remains UI Adapter-owned.

---

# 41. Events Are Not Guaranteed Query Replacement

Event delivery may be missed depending on Runtime profile.

Therefore consumers requiring current Presentation state should use:

```text
GetCurrentPresentation
GetPresentationSnapshot
GetRenderPlan
```

where appropriate.

Event Bus does not replace Query Interface.

---

# 42. Event Delivery Semantics

Actual delivery guarantees belong to Event Bus architecture.

Presentation must tolerate the delivery semantics declared by the active Runtime.

Possible profiles may include:

```text
InProcess
NonDurable
AtMostOnce
AtLeastOnce
DurableAtLeastOnce
```

Presentation EVENTS documentation does not redefine Event Bus guarantees.

---

# 43. MVP Event Delivery

If CRAI MVP uses an in-process non-durable bus:

* events may be lost on process crash;
* no durability is implied;
* replay is not guaranteed;
* ordering is bounded by canonical Event Bus policy.

Presentation correctness must not depend solely on event durability.

---

# 44. Event Idempotency

Duplicate delivery of the same `EventId` must not produce duplicate consumer-side logical effects.

Presentation itself publishes one event per committed Presentation fact.

Consumers should deduplicate when delivery semantics require it.

---

# 45. Semantic Duplicate Facts

Two different EventIds may describe equivalent Presentation state only in exceptional replay/recovery scenarios.

Consumers should primarily compare:

```text
presentationContextId
presentationId
presentationRevision
eventType
```

A lower PresentationRevision is stale.

An equal revision is duplicate/equivalent unless event semantics explicitly differ.

---

# 46. No Global Ordering

Presentation consumers MUST NOT assume a total order across:

```text
Runtime events
Preference events
UI Adapter events
Presentation events
Storage events
```

Causality and owner-owned revisions must be used instead.

---

# 47. Presentation Event Ordering

Within one Presentation lineage:

```text
PresentationRevision 7
PresentationRevision 8
PresentationRevision 9
```

defines Presentation state order.

Consumers MUST NOT apply:

```text
Revision 8
```

after already applying:

```text
Revision 9
```

---

# 48. Clear Ordering

`PresentationCleared` logically invalidates prior Presentation revisions for that lifecycle.

Example:

```text
PresentationUpdated revision 12
    ↓
PresentationCleared lastRevision = 12
```

A late duplicate:

```text
PresentationUpdated revision 11
```

must not recreate the cleared Presentation.

---

# 49. New Presentation After Clear

A future Presentation may use a new:

```text
PresentationId
```

for the same Presentation Context.

Example:

```text
Context A
Presentation P1
    ↓
Cleared
    ↓
Presentation P2
```

Events for P1 must not mutate or replace P2.

---

# 50. Presentation Context Ordering

Revision comparison is valid only inside compatible Presentation lineage/context.

Do not compare:

```text
Context A revision 10
Context B revision 7
```

as if they belonged to one global sequence.

---

# 51. Runtime Revision Is Not Presentation Event Order

RuntimeRevisionId may appear for correlation.

It does not replace PresentationRevision ordering.

One Runtime Revision may lead to multiple Presentation revisions.

Example:

```text
Runtime Revision 42
    ↓
Presentation Revision 1
Presentation Revision 2
Presentation Revision 3
```

---

# 52. No TranslationRevision Ordering Inside Presentation Events

Presentation events may optionally reference upstream Artifact provenance.

They SHOULD NOT require:

```text
TranslationRevision
```

as the Presentation ordering mechanism.

The authoritative accepted input is represented through:

```text
TranslationArtifactRef
```

and compatibility metadata.

PresentationRevision orders Presentation state.

---

# 53. Artifact References in Events

Presentation events should prefer immutable references where payload size is significant.

For example:

```text
snapshotRef?
renderPlanRef?
sourceArtifactRefs[]
```

An implementation may embed bounded immutable Snapshot/RenderPlan values for local MVP.

The chosen delivery form should remain consistent within one runtime profile.

---

# 54. Large Payload Rule

Events MUST NOT routinely duplicate:

* entire SourceDocument;
* complete Recognition payload;
* complete Translation Artifact;
* screenshots;
* page images.

Use immutable references where payload size would be excessive.

---

# 55. Privacy

Normal Presentation event payloads and metadata MUST NOT expose:

* provider credentials;
* full page images;
* full source documents in diagnostic fields;
* full translated documents in diagnostic fields;
* native window handles;
* private filesystem paths;
* raw provider responses.

Snapshot/RenderPlan may contain user-visible text where required by their semantic contract.

That content must be treated according to CRAI privacy policy and must not be copied unnecessarily into logs/telemetry.

---

# 56. Diagnostic Event Metadata

Useful bounded metadata:

```text
eventId
eventType
eventVersion
presentationContextId
presentationId
presentationRevision
previousPresentationRevision?
runtimeRevisionId?
workItemId?
attemptId?
operationId?
requestId?
targetId?
targetRevision?
viewportRevision?
effectiveMode?
fallbackReason?
result?
issueCode?
```

---

# 57. Diagnostic Content Rule

Logs/telemetry generated from Presentation events should normally record:

```text
IDs
counts
revisions
timings
modes
fallback categories
issue codes
```

not full content.

---

# 58. Events vs Telemetry

A Presentation event is an architectural fact.

It is not automatically:

* a log entry;
* a metric;
* a trace span.

Observability infrastructure may derive telemetry from events.

Event schema must not be polluted with arbitrary telemetry-only data.

---

# 59. Events vs State

State answers:

> What lifecycle condition is Presentation in now?

Event answers:

> What Presentation fact became true?

Example:

```text
State:
READY

Fact:
PresentationUpdated revision 15
```

They are related but not equivalent.

---

# 60. Events vs Commands

Command:

```text
RecomputePresentationLayout
```

means:

> Please attempt this operation.

Event:

```text
PresentationLayoutChanged
```

means:

> A new layout was successfully committed.

A command may produce no success event if:

* rejected;
* superseded;
* canceled;
* no-op;
* authority rejected.

---

# 61. No-op Event Rule

A command that results in no committed Presentation change should normally emit no success mutation event.

Examples:

* same effective mode;
* same focus;
* equivalent viewport;
* duplicate profile;
* equivalent Artifact input.

Diagnostics may still record the operation.

---

# 62. PresentationPrepared Invariant

`PresentationPrepared` means:

```text
A committed initial Presentation exists.
```

It does not mean:

```text
Translation completed.
UI rendered successfully.
Reading Session changed.
Artifact publication completed.
```

---

# 63. PresentationUpdated Invariant

`PresentationUpdated` means:

```text
Committed Presentation semantic/visible state changed.
```

It does not imply that every upstream Artifact changed.

---

# 64. PresentationLayoutChanged Invariant

`PresentationLayoutChanged` means:

```text
Committed framework-neutral layout changed.
```

It does not mean the platform UI has already repainted.

---

# 65. PresentationModeChanged Invariant

`PresentationModeChanged` means:

```text
effective committed Presentation mode changed.
```

A requested unsupported mode that falls back to the already-current mode may be a rejection/no-op rather than a mode-change event.

---

# 66. PresentationCleared Invariant

`PresentationCleared` means:

```text
Presentation no longer has current logical state for that lifecycle.
```

It does not guarantee physical memory/UI resources have all been disposed.

---

# 67. PresentationRejected Invariant

`PresentationRejected` means:

```text
requested Presentation mutation did not commit.
```

PresentationRevision does not increase.

Existing committed Presentation remains valid when applicable.

---

# 68. PresentationFailed Invariant

`PresentationFailed` means:

```text
Presentation-owned internal correctness cannot be guaranteed.
```

It does not mean:

```text
Runtime Attempt failed
Translation failed
UI apply failed
```

unless those external failures caused a separate genuine Presentation invariant failure.

---

# 69. Event Error Payload

Machine-readable fields are authoritative:

```text
reasonCode
failureCode
category
rejectionSource
recoverability
```

Human text:

```text
message?
```

is optional and non-authoritative.

---

# 70. Retry Semantics

Presentation events do not decide Runtime retry.

`PresentationRejected` may provide:

```text
retryHint
```

Examples:

```text
DoNotRetry
RetryWithLatestPresentationRevision
RetryAfterInputChange
RetryAfterTargetChange
```

Runtime/Application decides whether to issue new work.

---

# 71. Supersession Does Not Trigger Retry

A Candidate superseded by newer useful work normally requires no retry.

Example:

```text
Viewport 20 candidate
Viewport 21 candidate
Viewport 22 candidate
```

Only 22 may matter.

Discarding 20/21 is optimization/correctness behavior.

---

# 72. Runtime Event Separation

Presentation MUST NOT publish:

```text
PresentationAttemptCompleted
PresentationAttemptFailed
PresentationAttemptCancelled
PresentationRetryScheduled
PresentationWorkItemCompleted
```

as Runtime lifecycle facts.

Those belong to Runtime.

---

# 73. Artifact Event Separation

Presentation MUST NOT publish:

```text
TranslationArtifactPublished
RecognitionArtifactPublished
SourceDocumentArtifactPublished
```

Those facts belong to Runtime/Artifact publication owners.

---

# 74. UI Event Separation

Presentation MUST NOT publish:

```text
OverlayRendered
WindowOpened
WidgetMounted
DOMUpdated
PresentationPainted
```

Those facts belong to UI Adapter/platform implementations.

---

# 75. Storage Event Separation

Presentation MUST NOT publish:

```text
PresentationSaved
PresentationHistoryPersisted
PresentationCacheWritten
```

unless a future explicitly Presentation-owned persistence lifecycle is introduced.

Current ownership belongs outside Presentation.

---

# 76. Deferred Presentation Events

Not part of Presentation v2:

```text
PresentationCandidateCreated
PresentationCandidateValidated
PresentationCancelled
PresentationRetryScheduled
PresentationRendered
PresentationDisplayed
PresentationExported
PresentationCached
PresentationRestored
OverlayRebuilt
AccessibilityProfileChanged
```

Ownership or reasoning:

```text
Candidate events
    → diagnostics/trace unless future need

PresentationCancelled
    → Runtime cancellation lifecycle

PresentationRendered / Displayed
    → UI Adapter

PresentationExported
    → Export

PresentationCached / Restored
    → Storage/Application unless future explicit ownership

OverlayRebuilt
    → UI Adapter/rendering integration

AccessibilityProfileChanged
    → Preferences
```

---

# 77. MVP Published Event Set

Presentation v2 MVP publishes:

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
PresentationCleared
PresentationRejected
PresentationFailed
```

This is intentionally small.

---

# 78. MVP Mandatory Consumed Event Set

Presentation has:

```text
No mandatory direct consumed business-event set.
```

Correctness flows through explicit Presentation contracts.

Implementations may subscribe to integration facts as optimization or adapter convenience only when doing so does not introduce hidden orchestration.

---

# 79. Optional Integration Subscription Rule

If an implementation subscribes to an external event:

1. event must be owned externally;
2. handler must not bypass Presentation command validation;
3. handler must not grant Runtime authority;
4. handler should translate fact into an explicit command/application decision;
5. Event Bus subscription must be replaceable;
6. Presentation correctness must remain testable without the subscription.

---

# 80. Invalid External Event

Malformed or unrelated external integration facts are handled by their routing/integration owner.

Presentation should not become a generic event quarantine service.

If a mapped Presentation command is invalid, Presentation returns its normal rejection contract.

---

# 81. Event Publication Failure

Presentation commit and Event Bus publication are separate technical operations.

For MVP:

```text
Presentation commit
    ↓
attempt event publication
```

If event publication fails:

* the committed Presentation MUST NOT be rolled back automatically merely to recreate the event;
* Presentation operation MUST NOT be re-executed automatically;
* failure is diagnosed;
* UI/application may recover via queries where possible.

A future durable outbox may strengthen this guarantee.

---

# 82. Event Publication Failure Is Not Business Failure

If:

```text
Presentation Revision 8 committed
```

but:

```text
PresentationUpdated event publication fails
```

the Presentation is still Revision 8.

Observability must distinguish:

```text
business commit success
event publication failure
```

---

# 83. Outbox

A durable outbox may be introduced if CRAI later requires:

* crash-safe event publication;
* replay;
* stronger delivery guarantees.

The outbox is infrastructure.

It does not change Presentation event semantics.

---

# 84. Event Compatibility

Unknown optional fields may be ignored when safe.

Unknown required semantic values require:

* rejection;
* or explicitly documented compatibility fallback.

Major semantic changes require major event-version changes.

---

# 85. Event Versioning

Patch-level event changes may include:

* clarification;
* optional diagnostics;
* non-semantic documentation fixes.

Minor-compatible changes may include:

* new optional field;
* new optional enum values safely ignorable;
* bounded new metadata.

Major version required for:

* ownership change;
* required-field removal;
* revision semantics change;
* event meaning change;
* committed-vs-candidate meaning change.

---

# 86. Testing — Ownership

Tests MUST verify:

* Presentation publishes only Presentation-owned facts;
* Presentation does not publish Runtime terminal facts;
* Presentation does not publish Translation lifecycle facts;
* Presentation does not publish UI rendering completion;
* Presentation does not publish Storage completion.

---

# 87. Testing — Commit Ordering

Tests MUST verify:

```text
commit
before
success event
```

for:

* `PresentationPrepared`;
* `PresentationUpdated`;
* `PresentationLayoutChanged`;
* `PresentationModeChanged`;
* `PresentationCleared`.

---

# 88. Testing — Candidate Rejection

Tests MUST verify:

* Candidate validation failure emits no success fact;
* Runtime authority rejection emits no success fact;
* Presentation revision conflict emits no success fact;
* superseded reflow emits no success fact;
* stale viewport Candidate emits no success fact.

---

# 89. Testing — Revision

Tests MUST verify:

* success event carries committed PresentationRevision;
* previous revision is correct when applicable;
* lower PresentationRevision cannot replace newer;
* equal revision is duplicate/no-op;
* RuntimeRevisionId is never used as Presentation order.

---

# 90. Testing — Clear

Tests MUST verify:

* `PresentationCleared` publishes after logical invalidation;
* old Presentation events cannot recreate cleared state;
* repeated clear does not create duplicate clear events;
* new PresentationId after clear remains isolated from old lifecycle.

---

# 91. Testing — Rejection vs Failure

Tests MUST verify:

```text
unsupported mode
revision conflict
Runtime authority rejection
supersession
invalid viewport
```

do not produce `PresentationFailed` unless an actual Presentation invariant also breaks.

---

# 92. Testing — UI Boundary

Tests MUST verify:

* `PresentationPrepared` does not claim UI apply success;
* UI apply failure does not rewrite Presentation event history;
* stale UI Adapter consumers reject lower Presentation revisions.

---

# 93. Testing — Event Publication Failure

Tests SHOULD verify:

* committed Presentation survives Event Bus publication failure;
* operation is not re-executed automatically;
* diagnostics record publication failure;
* query path can recover current Presentation state.

---

# 94. Observability

Recommended event metrics:

```text
presentation_event_published_total
presentation_event_publish_failed_total
presentation_event_payload_bytes
presentation_rejected_event_total
presentation_failed_event_total
presentation_success_event_total
```

Useful labels:

```text
eventType
eventVersion
mode
result
rejectionSource
failureCategory
```

Never label metrics with raw user content.

---

# 95. Example — Initial Presentation

```text
Accepted upstream Artifacts
        ↓
Runtime / Application determines Presentation work
        ↓
BuildPresentation
        ↓
PREPARING
        ↓
Candidate built
        ↓
Runtime authority revalidation
        ↓
Atomic Presentation commit
        ↓
READY
        ↓
PresentationPrepared
        ↓
UI Adapter receives fact or queries current Presentation
```

---

# 96. Example — Translation Update

```text
New TranslationArtifact published
        ↓
Application / Business Pipeline decides update required
        ↓
UpdatePresentationContent
        ↓
UPDATING
        ↓
Candidate Presentation Revision 15
        ↓
commit
        ↓
READY
        ↓
PresentationUpdated
```

There is no required direct:

```text
TranslationUpdated event
    → Presentation mutation
```

subscription.

---

# 97. Example — Rapid Viewport Changes

```text
UI Adapter observes viewport 20
UI Adapter observes viewport 21
UI Adapter observes viewport 22
        ↓
Application / Presentation integration coalesces
        ↓
RecomputePresentationLayout viewport 22
        ↓
REFLOWING
        ↓
commit
        ↓
PresentationLayoutChanged
```

No success Presentation events are emitted for discarded layout candidates.

---

# 98. Example — Runtime Authority Rejection

```text
Presentation Candidate prepared
        ↓
Runtime Revision superseded
        ↓
Authority Revalidation
        ↓
RejectedStale
        ↓
Candidate discarded
```

Optional externally useful fact:

```text
PresentationRejected
rejectionSource = RuntimeAuthority
```

No:

```text
PresentationFailed
```

and no Presentation success event.

---

# 99. Example — Presentation Revision Conflict

```text
Current PresentationRevision = 8

Candidate A expects 8
Candidate B expects 8

Candidate B commits 9

Candidate A commit attempt
    ↓
revision conflict
    ↓
discard
```

Optional:

```text
PresentationRejected
rejectionSource = PresentationRevision
```

Revision remains 9.

---

# 100. Example — Mode Fallback

```text
ChangePresentationMode
requestedMode = Overlay
        ↓
Overlay cannot satisfy readability
        ↓
SidePanel fallback allowed
        ↓
Candidate effectiveMode = SidePanel
        ↓
commit
```

If effective committed mode changed:

```text
PresentationModeChanged
fallbackApplied = true
```

If resulting state is equivalent to the current state:

```text
no success mutation event
```

may be appropriate.

---

# 101. Example — Clear

```text
ClearPresentation
        ↓
Presentation logically invalidated
        ↓
CLEARING → EMPTY
        ↓
PresentationCleared
        ↓
UI Adapter removes native representation independently
```

---

# 102. Example — UI Apply Failure

```text
PresentationUpdated revision 12
        ↓
UI Adapter tries to apply revision 12
        ↓
TargetUnavailable
```

Possible UI Adapter-owned fact:

```text
PresentationApplyFailed
```

Presentation does not emit `PresentationFailed` solely because of this.

---

# 103. Example — Internal Presentation Failure

```text
Current Presentation
Snapshot revision = 20
RenderPlan revision = 19
        ↓
Presentation invariant violated
        ↓
FAILED
        ↓
PresentationFailed
failureCategory = CommittedStateMismatch
```

This is a genuine Presentation-owned failure.

---

# 104. Architecture Invariants

1. Events describe facts, not commands.

2. Presentation events describe Presentation-owned facts only.

3. Presentation has no mandatory direct consumed business-event set.

4. Event Bus does not replace Business Pipeline Orchestration.

5. External facts do not bypass Presentation command validation.

6. Presentation does not publish Runtime WorkItem events.

7. Presentation does not publish Runtime Attempt events.

8. Presentation does not publish Runtime cancellation events.

9. Presentation does not publish Translation terminal events.

10. Presentation does not publish Artifact publication facts.

11. Presentation does not publish native rendering completion.

12. Successful Presentation events publish only after atomic commit.

13. Candidate preparation alone produces no success fact.

14. Every successful mutation fact contains PresentationRevision.

15. PresentationRevision orders Presentation state.

16. RuntimeRevisionId is correlation metadata, not Presentation ordering.

17. Stale Candidate publishes no success event.

18. Superseded Candidate normally publishes no business event.

19. Runtime authority rejection is not Presentation internal failure.

20. Presentation revision conflict is not Presentation internal failure.

21. `PresentationRejected` means no commit occurred.

22. `PresentationFailed` means Presentation-owned correctness is untrusted.

23. Clear fact means logical invalidation, not physical UI destruction.

24. UI apply remains UI Adapter-owned.

25. Duplicate delivery does not create duplicate logical effects.

26. No global total ordering is assumed.

27. Event payloads are immutable.

28. Large upstream payloads should use Artifact references.

29. Standard diagnostics do not duplicate complete user content.

30. Event publication failure does not roll back valid Presentation commit automatically.

31. Event publication failure does not rerun Presentation business work automatically.

32. Query Interface remains available independently of Event Bus delivery.

---

# 105. Related Documents

```text
doc/02-modules/presentation/MODULE.md
doc/02-modules/presentation/CONTRACT.md
doc/02-modules/presentation/STATES.md
doc/02-modules/presentation/ERRORS.md
doc/02-modules/presentation/README.md

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
doc/01-architecture/core/STATE_MACHINE.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/02-modules/translation/EVENTS.md
doc/02-modules/reading-session/EVENTS.md
doc/02-modules/preferences/EVENTS.md
doc/02-modules/ui-adapter/EVENTS.md
```

---

# 106. Completion Criteria

This specification is synchronized when:

* Presentation event ownership is explicit;
* Presentation has no hidden event-driven workflow;
* no external business event directly defines Presentation state transitions;
* Runtime/Application remains responsible for invoking Presentation work;
* success facts occur only after Presentation commit;
* PresentationRevision is the public ordering mechanism;
* Runtime identity remains external metadata;
* Runtime authority rejection is distinct from Presentation rejection;
* Presentation rejection is distinct from Presentation failure;
* candidate/supersession behavior does not flood the Event Bus;
* clear means logical Presentation invalidation;
* UI apply remains outside Presentation event ownership;
* Artifact publication remains outside Presentation ownership;
* Event Bus publication failure does not invalidate committed Presentation state;
* event payloads remain immutable, serializable, bounded, and privacy-aware;
* tests cover ownership, commit timing, ordering, idempotency, rejection, failure, clear, and UI boundary.

---

# 107. Summary

Presentation v2 event flow is:

```text
External Artifact / Fact
        ↓
Application / Runtime Decision
        ↓
Presentation Command
        ↓
Presentation Operation
        ↓
Candidate
        ↓
Authority Revalidation
        ↓
Presentation Commit
        ↓
Presentation-owned Fact
```

Presentation publishes:

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
PresentationCleared
PresentationRejected
PresentationFailed
```

The critical rule is:

```text
Events explain what Presentation already committed,
rejected, cleared, or failed.

Events do not tell Presentation what work to execute.
```

And the ownership boundary remains:

```text
Runtime
    → execution authority and terminal work lifecycle

Artifact Store
    → accepted Artifact publication

Presentation
    → Presentation state and Presentation facts

UI Adapter
    → actual rendering and UI-apply facts
```
