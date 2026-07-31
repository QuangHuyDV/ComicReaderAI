# Presentation Events

- **Module:** Presentation
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture
- **Related documents:**
  - `modules/presentation/MODULE.md`
  - `modules/presentation/CONTRACT.md`
  - `modules/presentation/STATES.md`
  - `modules/presentation/ERRORS.md`
  - `docs/architecture/EVENT_BUS.md`

---

# 1. Purpose

This document defines the integration events consumed and published by the Presentation Module.

It establishes:

- event ownership;
- event naming;
- event envelopes;
- payload contracts;
- revision rules;
- idempotency requirements;
- ordering guarantees;
- failure and rejection behavior;
- relationships between events, commands, state transitions, and consumers.

This document does not define command schemas.

Command schemas belong to `CONTRACT.md`.

This document may reference the Presentation operation caused by an event for traceability, but an event is never treated as a command.

---

# 2. Event Model

Presentation supports both command-driven operations and event-driven integration.

```text
Application / Orchestrator
        │
        ├── Command
        ▼
   Presentation
        │
        ├── Published Event
        ▼
     Consumers
```

Other modules may publish facts that are relevant to Presentation:

```text
Reading Session
Translation
Preferences
UI Adapter
        │
        ├── Integration Event
        ▼
Application / Event Routing
        │
        ├── Presentation Command or Accepted Event
        ▼
Presentation
```

Presentation MUST NOT depend on UI rendering technology.

Presentation MUST NOT manipulate:

- browser DOM;
- native windows;
- UI widgets;
- canvas state;
- mouse listeners;
- keyboard listeners;
- platform-specific rendering surfaces.

Presentation produces immutable presentation data and publishes events after successful state commits.

---

# 3. Event Principles

## 3.1 Events Represent Facts

An event describes something that already happened.

Examples:

```text
SessionContentAccepted
TranslationUpdated
PresentationPrepared
PresentationCleared
```

An event MUST NOT express an imperative such as:

```text
BuildPresentation
RefreshLayout
ClearPresentation
```

Those are commands.

---

## 3.2 Events Are Immutable

After publication, an event MUST NOT be modified.

A correction requires a new event with:

- a new `EventId`;
- a new producer-owned revision when applicable;
- a causal reference to the event or operation being corrected.

---

## 3.3 Events Follow Ownership

Each event is owned by the module that owns the fact.

Examples:

```text
TranslationUpdated
    owner: Translation

ViewportChanged
    owner: UI Adapter

PresentationPrepared
    owner: Presentation
```

Translation MUST NOT publish a `PresentationId` as an authoritative identifier because Translation does not own Presentation identity.

Presentation MUST NOT publish events claiming that translation, session, or UI rendering work has completed.

---

## 3.4 Events Follow Atomic Commit

A successful Presentation event MUST be published only after the corresponding Presentation state has committed successfully.

Correct:

```text
Build candidate
Validate candidate
Commit PresentationSnapshot and RenderPlan
Advance PresentationRevision
Enter stable state
Publish success event
```

Incorrect:

```text
Publish success event
Commit state later
```

---

## 3.5 Events Are Not the Source of Mutable State

Published events may carry immutable state or immutable references.

Consumers MUST NOT mutate Presentation-owned objects received through an event.

The authoritative Presentation result is:

```text
PresentationSnapshot
RenderPlan
PresentationRevision
```

---

# 4. Event Naming

Events use completed-fact naming.

```text
<Noun><PastParticiple>
```

Examples:

```text
SessionContentAccepted
TranslationUpdated
PresentationPrepared
PresentationLayoutChanged
PresentationCleared
```

Event names MUST:

- describe an observable fact;
- use past tense;
- avoid transport-specific wording;
- avoid implementation-specific framework names;
- avoid command verbs.

Event names MUST NOT include terms such as:

```text
Do
Request
Execute
Refresh
RenderNow
```

unless the event explicitly represents a completed request lifecycle fact.

---

# 5. Common Event Envelope

Every integration event SHOULD use the following common envelope.

```text
EventEnvelope
- eventId
- eventType
- eventVersion
- occurredAt
- producer
- correlationId
- causationId
- traceId
- payload
```

## 5.1 Required Fields

### EventId

Stable unique identifier for one published event instance.

```text
eventId: EventId
```

The same `EventId` MUST be processed at most once logically.

---

### EventType

Canonical event name.

Examples:

```text
TranslationUpdated
PresentationPrepared
```

---

### EventVersion

Version of the event payload contract.

Example:

```text
1.0
```

---

### OccurredAt

Timestamp at which the fact became true in the producer.

---

### Producer

Canonical module name.

Examples:

```text
translation
reading-session
preferences
ui-adapter
presentation
```

---

## 5.2 Optional Correlation Fields

### CorrelationId

Groups events and commands belonging to the same user-visible operation or reading flow.

### CausationId

References the command, event, or operation that caused the current event.

### TraceId

Allows end-to-end diagnostics across modules.

---

# 6. Shared Identity and Authority Fields

Events MAY carry the following identifiers when relevant:

```text
SessionId
PresentationContextId
ContentId
ContentRevision
TranslationId
TranslationRevision
PresentationId
PresentationRevision
ViewportRevision
PreferenceRevision
ProfileRevision
OperationId
RequestId
```

Each producer MUST publish only revisions it owns.

## 6.1 Revision Ownership

```text
Reading Session owns:
- ContentRevision
- accepted reading target authority

Translation owns:
- TranslationId
- TranslationRevision

UI Adapter owns:
- ViewportRevision
- SurfaceId
- TransformRevision

Preferences owns:
- PreferenceRevision
- ProfileRevision

Presentation owns:
- PresentationId
- PresentationRevision
- optional LayoutRevision
```

A consumed event MUST NOT claim an authoritative `PresentationRevision`.

Commands targeting an existing presentation MAY contain:

```text
ExpectedPresentationRevision
```

Published Presentation events MUST contain the committed:

```text
PresentationRevision
```

---

# 7. Consumed Events

Presentation consumes integration facts from other modules.

The application or integration layer MAY transform these events into Presentation commands.

Presentation MUST preserve the same validation, revision, and authority rules regardless of whether an operation began through a command or an integration event.

---

## 7.1 SessionContentAccepted

### Publisher

```text
reading-session
```

### Meaning

Reading Session accepted a content revision as the current reading target for a presentation context.

This event replaces the broad and ambiguous `ReadingSessionChanged`.

Presentation MUST NOT react to every Reading Session mutation.

Presentation reacts only when the accepted content target relevant to Presentation changes.

### Payload

```text
SessionContentAcceptedPayload
- sessionId
- presentationContextId
- contentId
- contentRevision
- contentType
- sourceReference
- previousContentId?
- previousContentRevision?
- acceptanceReason
```

### AcceptanceReason

Examples:

```text
InitialContent
ChapterChanged
PageChanged
DocumentReplaced
SessionRestored
UserSelectedContent
```

### Presentation Behavior

Presentation MAY:

- update its expected target authority;
- clear an incompatible active presentation;
- retain a pending target while clearing;
- wait for translation content;
- accept a later `TranslationUpdated` or `TranslationCompleted`;
- start `BuildPresentation` only when sufficient inputs exist.

### Possible Presentation Result

```text
PresentationCleared
PresentationPrepared
PresentationRejected
```

`PresentationPrepared` is not required immediately after `SessionContentAccepted`.

---

## 7.2 TranslationUpdated

### Publisher

```text
translation
```

### Meaning

Translation produced a new or corrected immutable translation revision for part or all of the accepted content.

This event supports partial translation.

It may arrive before translation is final.

### Payload

```text
TranslationUpdatedPayload
- sessionId
- presentationContextId
- contentId
- contentRevision
- translationId
- translationRevision
- changedSegmentIds
- changedSegments
- availableSegmentIds
- failedSegmentIds
- completeness
- isFinal
- sourceRegions?
- translationMetadata?
```

### Boundary Rule

This event MUST NOT require `PresentationId`.

Translation does not own Presentation identity.

Presentation locates the applicable presentation context using:

```text
SessionId
PresentationContextId
ContentId
ContentRevision
```

### Presentation Behavior

Presentation MAY:

- build the first partial presentation;
- update affected presentation items;
- preserve stable item identity;
- recompute local layout;
- perform a full reflow when incremental layout is unsafe;
- expose partial content according to the active presentation policy;
- ignore stale or unrelated translation revisions.

### Possible Presentation Result

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationRejected
PresentationFailed
```

---

## 7.3 TranslationCompleted

### Publisher

```text
translation
```

### Meaning

A translation job reached its terminal state for the specified content revision.

Terminal does not necessarily mean every segment succeeded.

### Payload

```text
TranslationCompletedPayload
- sessionId
- presentationContextId
- contentId
- contentRevision
- translationId
- translationRevision
- availableSegmentIds
- failedSegmentIds
- completeness
- finalStatus
- segments?
- sourceRegions?
- translationMetadata?
```

### FinalStatus

Examples:

```text
Completed
CompletedWithPartialFailure
Cancelled
Failed
```

### Presentation Behavior

Presentation MAY:

- build a presentation if none exists;
- finalize an existing partial presentation;
- mark unavailable segments;
- apply a fallback presentation mode;
- preserve a valid partial presentation;
- reject only when the content cannot produce a valid presentation.

### Important Rule

Presentation MUST NOT reject a request only because translation is incomplete.

Partial translation is allowed when the active profile and presentation mode support it.

### Possible Presentation Result

```text
PresentationPrepared
PresentationUpdated
PresentationRejected
PresentationFailed
```

---

## 7.4 ViewportChanged

### Publisher

```text
ui-adapter
```

### Meaning

The visible reading surface or its geometry changed.

### Payload

```text
ViewportChangedPayload
- presentationContextId
- surfaceId
- viewportRevision
- viewport
- coordinateSpace
- transformRevision
- presentationId?
- expectedPresentationRevision?
```

### Viewport

```text
Viewport
- width
- height
- zoom
- scrollOffset
- deviceScale?
- orientation?
- visibleRegion?
```

### Boundary Rule

`PresentationId` is optional correlation data.

The authoritative surface identity is:

```text
PresentationContextId
SurfaceId
ViewportRevision
```

A viewport may change while Presentation is still `Empty` or `Preparing`.

### Presentation Behavior

Presentation MAY:

- cache the latest viewport for a pending build;
- recompute layout;
- coalesce rapid viewport events;
- discard obsolete calculations;
- preserve the previous committed render plan until a new plan commits.

### Possible Presentation Result

```text
PresentationLayoutChanged
PresentationUpdated
PresentationRejected
PresentationFailed
```

---

## 7.5 PresentationPreferenceChanged

### Publisher

```text
preferences
```

### Meaning

A user preference relevant to Presentation changed.

### Payload

```text
PresentationPreferenceChangedPayload
- sessionId?
- presentationContextId?
- preferenceRevision
- changedKeys
- impact
- effectivePreferences
```

### Impact

```text
StyleOnly
Layout
Mode
FullRebuild
NoPresentationImpact
```

### Presentation Behavior

```text
StyleOnly
    → update RenderPlan without semantic rebuild

Layout
    → recompute layout

Mode
    → reconfigure presentation strategy

FullRebuild
    → rebuild PresentationSnapshot and RenderPlan

NoPresentationImpact
    → no Presentation mutation
```

### Possible Presentation Result

```text
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
PresentationRejected
PresentationFailed
```

---

## 7.6 PresentationProfileChanged

### Publisher

```text
preferences
```

### Meaning

The active presentation profile changed.

A profile may include:

- typography;
- spacing;
- accessibility rules;
- marker behavior;
- overflow policy;
- preferred presentation mode;
- fallback policy.

### Payload

```text
PresentationProfileChangedPayload
- sessionId?
- presentationContextId?
- profileId
- profileRevision
- previousProfileId?
- changedCapabilities
- profile
```

### Presentation Behavior

Presentation MAY:

- apply the profile;
- reflow layout;
- change mode;
- rebuild presentation structure;
- apply a safe fallback when the profile is incompatible.

### Possible Presentation Result

```text
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
PresentationRejected
PresentationFailed
```

---

# 8. Published Events

Presentation publishes events describing committed Presentation facts.

Successful events MUST include the newly committed `PresentationRevision`.

Events MAY carry immutable output directly or immutable references according to the runtime profile.

The delivery form MUST be consistent within one runtime.

---

## 8.1 PresentationPrepared

### Meaning

A new Presentation became available for the accepted content target.

### Typical Transition

```text
Preparing → Ready
```

### Payload

```text
PresentationPreparedPayload
- presentationId
- presentationContextId
- sessionId
- contentId
- contentRevision
- translationId?
- translationRevision?
- presentationRevision
- mode
- target
- completeness
- snapshot
- renderPlan
- appliedProfileId?
- fallback?
- operationId
- requestId?
```

### Invariants

- `snapshot` is immutable;
- `renderPlan` is immutable;
- both belong to the same `PresentationRevision`;
- the event is emitted only after atomic commit;
- the event references the accepted content authority;
- the result is renderable by a compatible UI Adapter.

### Subscribers

Typical consumers:

```text
ui-adapter
diagnostics
application
```

---

## 8.2 PresentationUpdated

### Meaning

The committed Presentation content or non-layout presentation data changed.

### Typical Transition

```text
Updating → Ready
```

### Payload

```text
PresentationUpdatedPayload
- presentationId
- presentationContextId
- sessionId
- contentId
- contentRevision
- previousPresentationRevision
- presentationRevision
- changeSet
- snapshot
- renderPlan
- completeness
- operationId
```

### ChangeSet

```text
PresentationChangeSet
- addedItemIds
- updatedItemIds
- removedItemIds
- styleChanged
- visibilityChanged
- focusChanged
- semanticContentChanged
- layoutChanged
```

### Invariants

- unchanged semantic items preserve stable identity;
- a stale update does not emit this event;
- no partial candidate state is exposed;
- `presentationRevision` is greater than `previousPresentationRevision`.

---

## 8.3 PresentationLayoutChanged

### Meaning

The committed geometry or layout changed while the active content authority remained compatible.

### Typical Transition

```text
Reflowing → Ready
```

### Payload

```text
PresentationLayoutChangedPayload
- presentationId
- presentationContextId
- sessionId
- contentId
- contentRevision
- previousPresentationRevision
- presentationRevision
- viewportRevision
- renderPlan
- changedItemIds
- operationId
```

### Revision Policy

For v1, `PresentationRevision` is the required public revision.

A separate `LayoutRevision` MAY exist internally.

If `LayoutRevision` is exposed, it MUST NOT replace `PresentationRevision`.

### Invariants

- semantic translated content is unchanged unless the same atomic commit also contains an explicit content update;
- layout is deterministic for equivalent inputs;
- the event is not emitted for stale layout calculations;
- coordinate spaces are valid and explicit.

---

## 8.4 PresentationModeChanged

### Meaning

The active Presentation mode changed successfully.

### Typical Transition

```text
Reconfiguring → Ready
```

### Payload

```text
PresentationModeChangedPayload
- presentationId
- presentationContextId
- sessionId
- contentId
- contentRevision
- previousPresentationRevision
- presentationRevision
- previousMode
- currentMode
- fallbackApplied
- fallbackReason?
- snapshot
- renderPlan
- operationId
```

### Invariants

- `currentMode` is the committed mode;
- an unsupported requested mode does not replace the current mode;
- source traceability remains available;
- `snapshot` and `renderPlan` match `currentMode`.

---

## 8.5 PresentationCleared

### Meaning

An active Presentation was logically removed from a presentation context.

### Typical Transition

```text
Clearing → Empty
```

### Payload

```text
PresentationClearedPayload
- presentationId
- presentationContextId
- sessionId
- contentId
- contentRevision
- lastPresentationRevision
- reason
- operationId
```

### Reason

```text
UserRequested
SessionEnded
ContentReplaced
SurfaceClosed
ApplicationShutdown
RecoveryReset
InternalRecovery
```

### Invariants

- the old Presentation is no longer active before publication;
- outstanding operations for the cleared Presentation are invalid;
- one active Presentation lifecycle emits this event at most once;
- repeated clear commands while already empty do not emit duplicate clear events.

---

## 8.6 PresentationRejected

### Meaning

A Presentation request, event-driven operation, or mutation was deterministically rejected without corrupting the current valid state.

### Typical Outcome

```text
Stable state preserved
```

or:

```text
Preparing → Empty
```

### Payload

```text
PresentationRejectedPayload
- requestId?
- operationId
- presentationId?
- presentationContextId?
- sessionId?
- contentId?
- contentRevision?
- expectedPresentationRevision?
- currentPresentationRevision?
- errorCode
- category
- retryability
- message?
- rejectedEventId?
```

### Category

```text
Validation
StaleRevision
AuthorityMismatch
UnsupportedMode
InvalidGeometry
InvalidViewport
MissingRequiredData
Conflict
Cancelled
Superseded
```

### Retryability

```text
NotRetryable
RetryWithLatestRevision
RetryAfterInputUpdate
RetryAfterCapabilityChange
```

### Invariants

- rejection does not imply state corruption;
- `PresentationRevision` does not increase;
- the previous committed Presentation remains valid when one exists;
- `message` is not the authoritative machine-readable contract.

---

## 8.7 PresentationFailed

### Meaning

Presentation entered or confirmed an internal failure condition where it cannot guarantee a valid active state.

### Typical Transition

```text
Any transitional state → Failed
```

or:

```text
Ready → Failed
```

only when committed state can no longer be trusted.

### Payload

```text
PresentationFailedPayload
- operationId
- presentationId?
- presentationContextId?
- sessionId?
- contentId?
- contentRevision?
- presentationRevision?
- errorCode
- failureCategory
- recoveryState
- lastKnownGoodAvailable
- traceId?
```

### FailureCategory

```text
InvariantViolation
RevisionCorruption
RollbackFailure
GeometryStateCorruption
ResourceLifecycleCorruption
UnexpectedInternalFailure
```

### RecoveryState

```text
ResetRequired
ClearRequired
RestorePossible
ApplicationRestartRecommended
```

### Invariants

- this event is not used for ordinary validation errors;
- normal mutation operations stop until verified recovery;
- an unverified snapshot is not exposed as `Ready`;
- diagnostics contain enough information to identify the failed operation without logging translated content by default.

---

# 9. Event-to-Operation Mapping

Consumed events do not directly bypass Presentation contracts.

The application or Presentation integration boundary maps accepted events to operations.

| Consumed Event | Typical Presentation Operation |
|---|---|
| `SessionContentAccepted` | update expected target, clear incompatible presentation, or prepare pending build |
| `TranslationUpdated` | `BuildPresentation` or `UpdatePresentationContent` |
| `TranslationCompleted` | finalize, build, update, or apply partial-result policy |
| `ViewportChanged` | `RecomputePresentationLayout` |
| `PresentationPreferenceChanged` | update style, reflow, change mode, or rebuild |
| `PresentationProfileChanged` | `ApplyPresentationProfile` |

The same guards defined in `CONTRACT.md` and `STATES.md` apply.

---

# 10. Event Ordering

## 10.1 No Global Total Order

Presentation MUST NOT assume one total order across all producers.

The following may occur concurrently:

- viewport changes;
- translation updates;
- preference changes;
- session target changes;
- presentation commands.

A viewport event may arrive before translation.

A preference event may arrive while Presentation is `Empty`.

A content replacement may supersede an in-flight layout operation.

---

## 10.2 Per-Producer Revision Order

Events from one producer stream are ordered by that producer's revision.

Examples:

```text
TranslationRevision
ViewportRevision
PreferenceRevision
ProfileRevision
ContentRevision
```

Lower revisions are stale.

Equal revisions are duplicates or idempotent replays.

Higher revisions may supersede older in-flight work.

---

## 10.3 Per-Context Authority

Ordering comparisons are valid only inside the same relevant authority context.

Examples:

```text
same SessionId
same PresentationContextId
same ContentId
same ContentRevision
```

A revision from another context MUST NOT supersede the active context.

---

## 10.4 Causal Ordering

The following causal rules MUST hold:

```text
PresentationPrepared
    causally follows
accepted build input and atomic commit
```

```text
PresentationUpdated
    causally follows
accepted content update and atomic commit
```

```text
PresentationLayoutChanged
    causally follows
accepted viewport/layout input and atomic commit
```

```text
PresentationCleared
    invalidates
all earlier outstanding operations for that presentation lifecycle
```

Successful Presentation events MUST NOT be emitted before their commits.

---

## 10.5 Supersession

A newer accepted authority revision MAY supersede an older operation.

Superseded operations:

- MUST NOT commit;
- MUST NOT increment `PresentationRevision`;
- MUST NOT publish success events;
- MAY publish diagnostics;
- MAY publish `PresentationRejected` only when externally useful.

---

# 11. Idempotency

## 11.1 Event Identity

The same `EventId` MUST be processed at most once logically.

Duplicate delivery MUST NOT:

- commit twice;
- increment Presentation revision twice;
- emit duplicate success events;
- clear the same lifecycle twice.

---

## 11.2 Semantic Duplicates

Two events with different `EventId` values MAY still represent the same semantic revision.

Examples:

```text
same TranslationRevision
same ViewportRevision
same PreferenceRevision
same ContentRevision
```

Equivalent semantic duplicates MUST result in:

- no-op;
- cached acknowledgement;
- or deterministic same-state behavior.

They MUST NOT produce an unnecessary Presentation revision.

---

## 11.3 Duplicate Translation Update

A duplicate `TranslationUpdated` MUST NOT:

- replace newer translated content;
- increment `PresentationRevision`;
- emit `PresentationUpdated` again.

---

## 11.4 Duplicate Viewport Event

Equivalent viewport inputs MUST produce equivalent layout output.

Repeated identical viewport events MUST NOT create new revisions unless an explicitly changed layout dependency also exists.

---

## 11.5 Duplicate Clear

Repeated clear operations MUST end in:

```text
Empty
```

Only one `PresentationCleared` event may be emitted for one active lifecycle.

---

# 12. Revision Rules

## 12.1 Consumed Events

Consumed events carry producer-owned revisions.

```text
SessionContentAccepted
    → ContentRevision

TranslationUpdated
TranslationCompleted
    → TranslationRevision

ViewportChanged
    → ViewportRevision

PresentationPreferenceChanged
    → PreferenceRevision

PresentationProfileChanged
    → ProfileRevision
```

Consumed events SHOULD NOT require `PresentationRevision`.

They MAY carry:

```text
ExpectedPresentationRevision
```

as an optional optimistic concurrency hint when the producer legitimately observed it.

The final authority check remains inside Presentation.

---

## 12.2 Published Events

Every successful Presentation mutation event MUST carry:

```text
PresentationRevision
```

Update-like events SHOULD also carry:

```text
PreviousPresentationRevision
```

`PresentationRevision` MUST increase monotonically after each successful committed Presentation mutation.

It MUST NOT increase for:

- rejected operations;
- stale events;
- duplicate events;
- superseded operations;
- no-op changes;
- failed candidate calculations.

---

## 12.3 Content Authority

An event containing source-dependent data MUST identify:

```text
SessionId
PresentationContextId
ContentId
ContentRevision
```

Presentation MUST reject or discard events that do not match the current accepted authority.

---

# 13. Delivery Semantics

Presentation MUST tolerate duplicate delivery.

The actual transport guarantee is runtime-specific.

Possible runtime profiles:

```text
InProcessNonDurable
AtMostOnce
AtLeastOnce
DurableAtLeastOnce
```

## 13.1 MVP Runtime

For a local CRAI MVP, the event bus MAY use:

```text
in-process
non-durable
ordered only within one publisher callback stream
```

The architecture MUST NOT claim durable at-least-once delivery unless the runtime actually provides persistence, replay, and acknowledgement.

---

## 13.2 Consumer Responsibility

Consumers of Presentation events MUST:

- handle duplicate events;
- compare revisions;
- avoid applying stale snapshots;
- treat `PresentationCleared` as lifecycle invalidation;
- avoid mutating event payloads.

---

## 13.3 Outbox

An outbox or equivalent mechanism MAY be introduced when state and event durability become necessary.

Until then, atomic in-memory commit followed by immediate event publication is acceptable for MVP, provided failure behavior is explicit.

---

# 14. Invalid Event Handling

Invalid events MUST be classified.

## 14.1 Duplicate

Behavior:

```text
No-op or return cached outcome
```

---

## 14.2 Stale

Examples:

- older translation revision;
- older viewport revision;
- obsolete content revision.

Behavior:

```text
Discard
Do not mutate
Do not increment revision
Record diagnostic when useful
```

---

## 14.3 Unrelated

Examples:

- another presentation context;
- another session;
- another content target.

Behavior:

```text
Ignore or route elsewhere
```

---

## 14.4 Malformed

Examples:

- missing required identifiers;
- invalid event version;
- invalid payload shape;
- invalid coordinate declaration.

Behavior:

```text
Reject or quarantine
Record diagnostic
Do not mutate Presentation
```

---

## 14.5 Authority Mismatch

Examples:

- event claims obsolete content authority;
- session does not own the target;
- source region does not belong to content revision.

Behavior:

```text
Reject or discard
Do not mutate current Presentation
Emit security or integrity diagnostic when appropriate
```

---

## 14.6 Recoverable Processing Failure

Examples:

- optional font metrics unavailable;
- one layout strategy fails;
- source geometry can fall back to side panel;
- one segment cannot be displayed.

Behavior MAY include:

```text
Fallback
Partial Presentation
Item suppression
PresentationRejected
PresentationUpdated
PresentationModeChanged
```

The committed result MUST still satisfy all Presentation invariants.

---

## 14.7 Internal Failure

Examples:

- impossible revision ordering;
- rollback failure;
- corrupted committed graph;
- active identifiers disagree with committed snapshot.

Behavior:

```text
Transition to Failed
Publish PresentationFailed
Stop normal mutations
Require verified recovery
```

---

# 15. Event Dependencies

```text
Reading Session ─────────────┐
Translation ─────────────────┤
Preferences ─────────────────┤
UI Adapter ──────────────────┤
                             ▼
                  Application / Event Routing
                             │
                             ▼
                      Presentation
                        │         │
                        ▼         ▼
                    UI Adapter  Diagnostics
                        │
                        ▼
                     Renderer
```

Presentation MAY publish events consumed by the application, UI Adapter, and Diagnostics.

Presentation MUST NOT publish events claiming ownership of:

- translation completion;
- session acceptance;
- preference persistence;
- rendering completion;
- export completion;
- storage completion.

---

# 16. Event Invariants

The following invariants MUST always hold:

1. Events are immutable.
2. Every event has a stable `EventId`.
3. Successful Presentation events are emitted only after atomic commit.
4. Every successful Presentation mutation event carries `PresentationRevision`.
5. A stale operation never publishes a success event.
6. A duplicate event does not cause duplicate mutation.
7. Translation events do not require authoritative `PresentationId`.
8. Consumed events carry producer-owned revisions.
9. Published Presentation events carry Presentation-owned revisions.
10. `PresentationPrepared` includes or references immutable `PresentationSnapshot` and `RenderPlan`.
11. `PresentationUpdated` references an existing Presentation lifecycle.
12. `PresentationCleared` invalidates all outstanding operations for that lifecycle.
13. `PresentationRejected` is used for deterministic rejection, not internal corruption.
14. `PresentationFailed` is reserved for broken invariants or untrusted internal state.
15. Events do not expose mutable internal objects.
16. Event payloads do not contain translated user content in diagnostics fields by default.
17. No event grants Presentation ownership of UI rendering.
18. No total ordering is assumed across independent producers.
19. Per-context revision comparison is deterministic.
20. Event processing preserves the state machine defined in `STATES.md`.

---

# 17. Observability Requirements

Event diagnostics SHOULD include:

```text
eventId
eventType
eventVersion
producer
correlationId
causationId
traceId
sessionId
presentationContextId
contentId
contentRevision
translationRevision
viewportRevision
preferenceRevision
presentationId
presentationRevision
operationId
processingResult
processingDuration
rejectionCode
failureCode
```

Normal diagnostics SHOULD NOT include:

- full translated text;
- full source text;
- full page images;
- full geometry arrays;
- credentials;
- filesystem paths containing private user data.

Geometry SHOULD be summarized unless explicit diagnostic mode is enabled.

---

# 18. Testing Requirements

## 18.1 Contract Tests

Tests MUST verify:

- required envelope fields;
- valid event versions;
- required payload fields;
- event ownership;
- producer revision ownership;
- immutable payload behavior.

---

## 18.2 Idempotency Tests

Tests MUST verify:

- duplicate `EventId` causes one logical mutation;
- duplicate translation revision does not increment Presentation revision;
- duplicate viewport revision does not create a second layout commit;
- duplicate clear emits one clear event.

---

## 18.3 Ordering Tests

Tests MUST verify:

- stale translation does not overwrite newer translation;
- stale viewport result does not overwrite newer layout;
- preference change during reflow is queued, merged, or superseded deterministically;
- content replacement invalidates old in-flight work;
- no global producer order is assumed.

---

## 18.4 Event-after-Commit Tests

Tests MUST verify:

- `PresentationPrepared` publishes after commit;
- `PresentationUpdated` publishes after commit;
- `PresentationLayoutChanged` publishes after commit;
- `PresentationModeChanged` publishes after commit;
- `PresentationCleared` publishes after logical invalidation;
- no success event publishes after failed candidate validation.

---

## 18.5 Rejection and Failure Tests

Tests MUST verify:

- stale input produces rejection or discard without entering `Failed`;
- unsupported mode preserves the previous presentation;
- malformed event does not mutate state;
- authority mismatch does not mutate another context;
- invariant corruption publishes `PresentationFailed`;
- ordinary validation errors do not publish `PresentationFailed`.

---

## 18.6 Payload Consistency Tests

Tests MUST verify:

- snapshot and render plan share one Presentation revision;
- event mode matches render plan mode;
- content identifiers match accepted authority;
- previous and current Presentation revisions are ordered correctly;
- changed item identifiers belong to the published snapshot.

---

# 19. MVP Event Set

The v1 Presentation event set is:

## Consumed

```text
SessionContentAccepted
TranslationUpdated
TranslationCompleted
ViewportChanged
PresentationPreferenceChanged
PresentationProfileChanged
```

## Published

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
PresentationCleared
PresentationRejected
PresentationFailed
```

Events outside this set require a documented architecture decision before becoming part of v1.

---

# 20. Deferred Events

The following are not Presentation-owned v1 events:

```text
PresentationExported
OverlayRebuilt
AccessibilityProfileChanged
PresentationCached
PresentationRestored
```

Ownership guidance:

```text
PresentationExported
    → Export Module

OverlayRebuilt
    → UI Adapter or renderer integration

AccessibilityProfileChanged
    → Preferences

PresentationCached
PresentationRestored
    → Storage or application orchestration
```

Presentation MAY later publish a specific restoration-related event only if Presentation itself owns a validated restoration lifecycle in a future version.

---

# 21. Example Flows

## 21.1 Initial Partial Comic Presentation

```text
SessionContentAccepted
    │
    ▼
Presentation target authority updated
    │
    ▼
TranslationUpdated
isFinal = false
completeness = partial
    │
    ▼
BuildPresentation
    │
    ▼
Preparing
    │
    ├── Build partial PresentationSnapshot
    ├── Build marker RenderPlan
    ├── Apply side-panel fallback if needed
    └── Commit
    │
    ▼
Ready
    │
    ▼
PresentationPrepared
```

---

## 21.2 Translation Correction

```text
TranslationUpdated
translationRevision = 8
    │
    ▼
Validate authority and revision
    │
    ▼
UpdatePresentationContent
    │
    ▼
Updating
    │
    ├── preserve previous committed snapshot
    ├── update affected items
    ├── reflow affected geometry if required
    └── commit revision 15
    │
    ▼
Ready
    │
    ▼
PresentationUpdated
previousPresentationRevision = 14
presentationRevision = 15
```

---

## 21.3 Rapid Viewport Changes

```text
ViewportChanged revision 20
ViewportChanged revision 21
ViewportChanged revision 22
    │
    ▼
Coalesce obsolete viewport work
    │
    ▼
RecomputePresentationLayout for revision 22
    │
    ▼
Commit latest RenderPlan
    │
    ▼
PresentationLayoutChanged
```

No success event is emitted for viewport revisions 20 or 21.

---

## 21.4 Content Replacement

```text
SessionContentAccepted
ContentId = Chapter-11
    │
    ▼
Current Presentation belongs to Chapter-10
    │
    ▼
ClearPresentation
    │
    ▼
PresentationCleared
reason = ContentReplaced
    │
    ▼
Wait for compatible translation
    │
    ▼
TranslationUpdated or TranslationCompleted
    │
    ▼
PresentationPrepared
```

---

## 21.5 Unsupported Mode

```text
PresentationPreferenceChanged
impact = Mode
requested mode = AdvancedImageRewrite
    │
    ▼
Mode capability check fails
    │
    ▼
Previous Presentation remains committed
    │
    ▼
PresentationRejected
category = UnsupportedMode
retryability = RetryAfterCapabilityChange
```

The module remains `Ready`.

---

## 21.6 Internal Invariant Failure

```text
Reflowing
    │
    ▼
Candidate geometry violates internal invariant
    │
    ├── previous committed snapshot trusted
    │       ▼
    │     discard candidate
    │     return Ready
    │     optional PresentationRejected
    │
    └── previous committed snapshot untrusted
            ▼
          Failed
            ▼
          PresentationFailed
```

---

# 22. Completion Criteria

This event specification is considered implemented when:

- every v1 event has a runtime schema;
- all events use a common envelope;
- event ownership is enforced;
- consumed events use producer-owned revisions;
- published Presentation events use `PresentationRevision`;
- duplicate delivery is safe;
- stale revisions cannot commit;
- successful events publish only after atomic commit;
- partial translation is supported;
- `PresentationRejected` and `PresentationFailed` are distinct;
- `SessionContentAccepted` replaces broad session-change coupling;
- Translation does not depend on `PresentationId`;
- UI Adapter can obtain immutable `PresentationSnapshot` and `RenderPlan`;
- contract tests cover payloads, revisions, ordering, idempotency, and failure behavior;
- the event model remains consistent with `MODULE.md`, `CONTRACT.md`, and `STATES.md`.
