# Reading Session Events

> **Project:** CRAI
> **Module:** `reading-session`
> **Path:** `doc/02-modules/reading-session/EVENTS.md`
> **Version:** 3.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-08

---

# 1. Purpose

This document defines the event boundary of the Reading Session module.

It specifies:

```text id="wma3oi"
Reading Session-owned facts
event ownership
event naming
canonical event envelope
ReadingContextRevision semantics
event ordering
idempotency
event/version compatibility
publication timing
privacy
observability
```

Reading Session events describe committed facts about:

```text id="m5i3qr"
Reading Session lifecycle
Reading Context
Reading Target
Reading Source
Reading Position
Session Configuration
```

This document does not define:

```text id="7zbtt3"
Runtime execution events
WorkItem events
Attempt events
ProcessingIntent events
Artifact publication events
Translation events
Presentation events
UI rendering events
```

---

# 2. Core Event Principle

An event describes:

> A fact that has already become true.

Events never request future work.

Correct:

```text id="cf7l5l"
ReadingSessionActivated
ReadingTargetChanged
ReadingContextChanged
ReadingConfigurationChanged
```

Incorrect:

```text id="11it8o"
RunOCR
StartTranslation
RefreshPresentation
CreateWorkItem
```

Those are commands or orchestration decisions.

---

# 3. Event Ownership

Reading Session publishes only Reading Session-owned facts.

Typical ownership:

```text id="s9p07i"
ReadingSessionCreated
    → Reading Session

ReadingContextChanged
    → Reading Session

RuntimeRevisionSuperseded
    → Runtime

TranslationArtifactPublished
    → Artifact / Runtime publication owner

PresentationUpdated
    → Presentation

PresentationApplied
    → UI Adapter
```

Reading Session MUST NOT publish events owned by another module.

---

# 4. Reading Session Event Categories

Reading Session v3 uses three main event groups:

```text id="fnqwiy"
Reading Session Events
├── Session Lifecycle Events
├── Reading Context Events
└── Reading Configuration / Navigation Facts
```

There is no Reading Session-owned:

```text id="ygad86"
ContentRevision event lifecycle
ProcessingIntent event lifecycle
```

---

# 5. Removed Event Categories

The following v2 categories are removed:

```text id="jqgrkf"
ContentRevisionEvents
ProcessingIntentEvents
```

Reason:

```text id="5xgwhf"
ReadingContextRevision
    → immutable domain version, not lifecycle object

ProcessingIntent
    → Business Pipeline Orchestration concern
```

---

# 6. Canonical Event Envelope

Reading Session events follow the CRAI canonical Event Convention.

Conceptually:

```text id="bvnmcb"
EventEnvelope
├── eventId
├── eventType
├── eventVersion
├── occurredAt
├── producer
├── aggregateId
├── aggregateVersion?
├── correlationId?
├── causationId?
├── traceId?
├── payload
└── metadata?
```

The canonical Event Convention remains authoritative if field names differ.

Reading Session MUST NOT invent a competing global event envelope.

---

# 7. Producer

Reading Session-owned events use:

```text id="1heh4o"
producer = reading-session
```

The producer identifies event ownership.

---

# 8. Aggregate Identity

For Reading Session events:

```text id="p3gtrg"
aggregateId = ReadingSessionId
```

where the event belongs to one ReadingSession aggregate.

---

# 9. Aggregate Version

Reading Session may use:

```text id="y9q4re"
ReadingContextRevision
```

as aggregate/domain version metadata when the event reflects a ReadingContext mutation.

Lifecycle-only events that do not change ReadingContext need not increment ReadingContextRevision.

---

# 10. ReadingContextRevision in Events

Events describing committed ReadingContext mutations MUST carry:

```text id="gy8y6e"
readingContextRevision
```

and SHOULD carry:

```text id="hro04z"
previousReadingContextRevision
```

when applicable.

Rules:

1. revision identifies committed reading-domain state;
2. revision changes only after successful domain commit;
3. rejected commands produce no new revision event;
4. no-op commands produce no new revision event;
5. revision does not imply Runtime authority.

---

# 11. Runtime Revision Is Not Event Ordering

Reading Session events MUST NOT use:

```text id="rvs9yj"
RuntimeRevisionId
```

as Reading Session event order.

If Runtime identity is included for correlation, it remains external metadata only.

---

# 12. Event Publication Timing

Successful Reading Session facts follow:

```text id="3y66qc"
Validate command
    ↓
Build Candidate domain state
    ↓
Commit Reading Session state
    ↓
Advance ReadingContextRevision if applicable
    ↓
Publish Reading Session fact
```

Never:

```text id="5j9wmu"
Publish success event
    ↓
attempt state commit
```

---

# 13. Candidate State Does Not Produce Success Facts

Reading Session should not publish:

```text id="m3r9gc"
ReadingContextCandidateCreated
ReadingContextCandidateValidated
```

as normal business facts.

Those are internal/diagnostic stages.

Only committed domain state is externally authoritative.

---

# 14. Session Lifecycle Event Set

Reading Session v3 may publish:

```text id="2w0acc"
ReadingSessionCreated
ReadingSessionActivated
ReadingSessionPaused
ReadingSessionResumed
ReadingSessionCompleted
ReadingSessionCancelled
ReadingSessionDisposed
```

If `INITIALIZING` or `COMPLETING` are public lifecycle states, optional events may later be added.

MVP does not require them.

---

# 15. ReadingSessionCreated

## Meaning

A ReadingSession aggregate was successfully created.

## Typical Payload

```text id="kf7xlu"
ReadingSessionCreatedPayload
├── readingSessionId
├── lifecycleState
├── readingContextRevision?
├── readingSource
├── initialTarget?
├── sessionConfigurationSummary
└── createdAt
```

## Invariants

* event is published after aggregate creation commit;
* SessionId is stable;
* event does not imply Runtime processing began.

---

# 16. ReadingSessionActivated

## Meaning

The Reading Session became active for reading-domain interaction.

Typical transition:

```text id="ro332q"
INITIALIZING → ACTIVE
```

or an MVP-equivalent activation.

## Payload

```text id="ts54nk"
ReadingSessionActivatedPayload
├── readingSessionId
├── lifecycleState
├── readingContextRevision?
└── activatedAt
```

## Invariant

Activation does not mean:

```text id="mtq43s"
Capture started
OCR started
Translation started
Presentation ready
```

---

# 17. ReadingSessionPaused

## Meaning

The reading activity was intentionally paused.

Payload:

```text id="vpyzm0"
ReadingSessionPausedPayload
├── readingSessionId
├── readingContextRevision?
├── reason?
└── pausedAt
```

Pause does not imply Runtime cancellation.

---

# 18. ReadingSessionResumed

## Meaning

A paused reading activity became active again.

Payload:

```text id="kfl4ki"
ReadingSessionResumedPayload
├── readingSessionId
├── readingContextRevision?
└── resumedAt
```

Resume does not automatically create a new ReadingContextRevision.

---

# 19. ReadingSessionCompleted

## Meaning

The reading activity ended normally.

Payload:

```text id="1q399f"
ReadingSessionCompletedPayload
├── readingSessionId
├── finalReadingContextRevision?
├── reason
└── completedAt
```

Completion does not mean:

```text id="3jsl2y"
all Runtime work completed
all UI resources disposed
all Artifacts deleted
```

---

# 20. ReadingSessionCancelled

## Meaning

The reading activity itself was canceled.

Payload:

```text id="pfipyg"
ReadingSessionCancelledPayload
├── readingSessionId
├── finalReadingContextRevision?
├── reason
└── cancelledAt
```

Important:

```text id="vi5cve"
ReadingSessionCancelled
≠
RuntimeAttemptCancelled
```

Reading Session does not publish Runtime cancellation facts.

---

# 21. ReadingSessionDisposed

## Meaning

The ReadingSession aggregate reached its irreversible business disposal state.

Payload:

```text id="yjaclp"
ReadingSessionDisposedPayload
├── readingSessionId
├── lastReadingContextRevision?
└── disposedAt
```

No further Reading Session mutation facts may occur for the disposed lifecycle.

---

# 22. Reading Context Event Set

Reading Session v3 may publish:

```text id="t64cg3"
ReadingContextPrepared
ReadingContextChanged
ReadingContextInvalidated
ReadingContextDisposed
```

`ReadingContextPrepared` is the committed initial-context fact.

It does not mean Candidate preparation began.

---

# 23. ReadingContextPrepared

## Meaning

The first valid ReadingContext was committed.

Typical transition:

```text id="pbs56d"
PREPARING → READY
```

Payload:

```text id="rct1fi"
ReadingContextPreparedPayload
├── readingSessionId
├── readingContextRevision
├── contextSnapshot
├── changeReason
└── committedAt
```

## Invariants

* context snapshot immutable;
* revision committed;
* event occurs after state commit;
* event does not imply downstream processing.

---

# 24. ReadingContextChanged

## Meaning

A new committed ReadingContext replaced the previous committed ReadingContext.

Typical transition:

```text id="jrqqd2"
UPDATING → READY
```

Payload:

```text id="q0f81l"
ReadingContextChangedPayload
├── readingSessionId
├── previousReadingContextRevision
├── readingContextRevision
├── contextSnapshot
├── changeSet
├── reason
└── committedAt
```

---

# 25. ReadingContextChangeSet

Conceptually:

```text id="02xj8p"
ReadingContextChangeSet
├── sourceChanged
├── targetChanged
├── positionChanged
├── sourceLanguageChanged
├── targetLanguageChanged
├── configurationChanged
└── otherDomainChange?
```

This describes what changed.

It does not say what processing is required.

---

# 26. ReadingContextInvalidated

## Meaning

The current ReadingContext can no longer be trusted as a valid representation of the reading activity.

Payload:

```text id="98nu64"
ReadingContextInvalidatedPayload
├── readingSessionId
├── readingContextRevision?
├── reasonCode
├── recoveryHint?
└── invalidatedAt
```

Examples:

```text id="jb6yys"
source invalid
restored context corrupt
logical target invalid
domain identity conflict
```

Do not publish this because:

```text id="wrpa3a"
OCR failed
Translation failed
Runtime timed out
UI rendering failed
```

---

# 27. ReadingContextDisposed

## Meaning

Reading Context state was permanently removed from the Reading Session lifecycle.

Payload:

```text id="f9js36"
ReadingContextDisposedPayload
├── readingSessionId
├── lastReadingContextRevision?
└── disposedAt
```

This does not describe Runtime Artifact cleanup.

---

# 28. Navigation / Domain Change Facts

Some changes are important enough to deserve specialized facts in addition to `ReadingContextChanged`.

Potential events:

```text id="p1ltnb"
ReadingTargetChanged
ReadingSourceChanged
ReadingPositionChanged
ReadingConfigurationChanged
ReadingLanguageChanged
ReadingModeChanged
```

These events are optional specialized domain projections.

The architecture must avoid emitting redundant overlapping events without a clear consumer need.

---

# 29. Preferred MVP Event Strategy

For MVP, prefer a compact event set:

```text id="5bfb38"
ReadingSessionCreated
ReadingSessionActivated
ReadingContextPrepared
ReadingContextChanged
ReadingSessionPaused
ReadingSessionResumed
ReadingSessionCompleted
ReadingSessionCancelled
ReadingContextInvalidated
ReadingSessionDisposed
```

`ReadingContextChanged.changeSet` provides most fine-grained information.

Specialized events should be added only where they materially simplify consumers.

---

# 30. ReadingTargetChanged

If retained as a specialized event:

```text id="kjy6gr"
ReadingTargetChangedPayload
├── readingSessionId
├── previousReadingContextRevision
├── readingContextRevision
├── previousTarget?
├── target
└── reason
```

It must correspond to the same committed revision as the related context change.

---

# 31. ReadingSourceChanged

If retained:

```text id="p1s5fo"
ReadingSourceChangedPayload
├── readingSessionId
├── previousReadingContextRevision
├── readingContextRevision
├── previousSource?
├── source
└── reason
```

It means logical ReadingSource changed.

It does not mean Runtime restart.

---

# 32. ReadingPositionChanged

If business-significant ReadingPosition changed:

```text id="hfwd6c"
ReadingPositionChangedPayload
├── readingSessionId
├── previousReadingContextRevision
├── readingContextRevision
├── previousPosition?
├── position
└── reason
```

Raw scrolling should be normalized/coalesced before Reading Session sees it.

---

# 33. ReadingConfigurationChanged

Preferred replacement for generic:

```text id="x24wo5"
ConfigurationUpdated
```

Payload:

```text id="j4441c"
ReadingConfigurationChangedPayload
├── readingSessionId
├── previousReadingContextRevision
├── readingContextRevision
├── changedFields[]
├── configurationSummary
└── reason
```

This fact describes session-specific configuration only.

Persistent preference facts belong to Preferences.

---

# 34. ReadingLanguageChanged

Optional specialized event:

```text id="wgs3ok"
ReadingLanguageChangedPayload
├── readingSessionId
├── previousReadingContextRevision
├── readingContextRevision
├── previousSourceLanguage?
├── sourceLanguage?
├── previousTargetLanguage?
└── targetLanguage?
```

This event does not say:

```text id="9qg06s"
Translation must run
```

Business Pipeline Orchestration decides that.

---

# 35. ReadingModeChanged

If reading mode is a Reading Session-owned domain configuration:

```text id="bwcm5c"
ReadingModeChangedPayload
├── readingSessionId
├── previousReadingContextRevision
├── readingContextRevision
├── previousReadingMode
└── readingMode
```

Do not confuse this with PresentationMode.

---

# 36. ReadingMode vs PresentationMode

Reading mode may mean:

```text id="6i2x7o"
Comic
Novel
Document
VerticalReading
PagedReading
```

Presentation mode may mean:

```text id="5bg4eu"
SidePanel
Overlay
TextReader
```

Their events must remain owned by their respective domains.

---

# 37. Removed ContentRevision Events

The following v2 events are removed:

```text id="w8o4pc"
ContentRevisionCreated
ContentRevisionActivated
ContentRevisionSuperseded
ContentRevisionArchived
ContentRevisionDiscarded
```

Replacement semantics:

```text id="xm0etn"
ReadingContextRevision appears
inside committed ReadingContext events.
```

No separate revision lifecycle is required.

---

# 38. Why ContentRevisionActivated Is Removed

Previously:

```text id="sp6yvk"
ContentRevisionCreated
    ↓
ContentRevisionActivated
```

created an unnecessary two-phase business authority model.

v3 uses:

```text id="7x0k2l"
Candidate ReadingContext
    ↓
atomic commit
    ↓
ReadingContextRevision N
is current
```

There is no externally visible “created but not active revision” business state.

---

# 39. Why ContentRevisionSuperseded Is Removed

A previous ReadingContextRevision does not need a business lifecycle transition.

Example:

```text id="hozex9"
Revision 20 current
    ↓
Revision 21 commits
```

Revision 20 remains historical if retained.

Runtime execution supersession is a separate Runtime concept.

---

# 40. Removed ProcessingIntent Events

The following v2 events are removed:

```text id="l1j77j"
ProcessingIntentCreated
ProcessingIntentPublished
ProcessingIntentAccepted
ProcessingIntentFulfilled
ProcessingIntentObsoleted
ProcessingIntentDiscarded
```

They no longer belong to Reading Session.

---

# 41. Why ProcessingIntentAccepted Is Removed

This fact requires Reading Session to know:

```text id="f8jzy4"
Runtime accepted execution responsibility
```

That is Runtime-owned execution state.

---

# 42. Why ProcessingIntentFulfilled Is Removed

This event required Reading Session to determine:

```text id="v294fc"
the requested processing objective was fulfilled
```

That requires processing topology and accepted Artifact knowledge.

Those belong to:

```text id="kk166z"
Business Pipeline Orchestration
Runtime
Artifact lifecycle
```

not Reading Session.

---

# 43. Events Do Not Replace Pipeline Orchestration

Invalid:

```text id="lgbfjw"
ReadingContextChanged
    ↓
Recognition starts directly
```

Preferred:

```text id="c89q8z"
ReadingContextChanged / current context state
        ↓
Business Pipeline Orchestration
        ↓
pipeline requirement evaluation
        ↓
Runtime Control
```

---

# 44. Event Consumers

Potential consumers of Reading Session facts include:

```text id="0rranq"
Business Pipeline Orchestration
Application
Persistence projection
Analytics
Diagnostics
History
UI coordination
```

Processing modules should generally not subscribe directly to Reading Session events to self-orchestrate work.

---

# 45. Consumer Independence

Reading Session publishes facts without assuming:

* subscriber count;
* subscriber success;
* subscriber implementation;
* processing topology.

A failed subscriber does not roll back already committed Reading Session state.

---

# 46. External Input Events

Reading Session has no mandatory direct consumed-event set.

External facts such as:

```text id="99f1si"
BrowserNavigated
ViewportChanged
PreferenceChanged
UserSelectedRegion
```

should normally be normalized by Application/Adapters into Reading Session commands.

---

# 47. Why Direct Event Consumption Is Avoided

Direct event-driven domain mutation creates hidden command paths.

Preferred:

```text id="71szl6"
External Adapter
    ↓
normalized application intent
    ↓
Reading Session command
```

This preserves:

* command validation;
* optimistic concurrency;
* testability;
* deterministic mutation.

---

# 48. Correlation

Reading Session events may include:

```text id="y6kzcx"
correlationId
causationId
traceId
requestId?
```

to connect business activity across modules.

Correlation metadata does not affect event meaning.

---

# 49. Causation

Typical causation:

```text id="ujkvlt"
UpdateReadingTarget command
        ↓
ReadingContext commit
        ↓
ReadingContextChanged
```

If a specialized `ReadingTargetChanged` event is also emitted, both events may reference the same command causation.

---

# 50. Event Ordering

Reading Session guarantees logical ordering only within one ReadingSession aggregate according to committed domain state.

Ordering is derived from:

```text id="xd7289"
ReadingContextRevision
+
Session lifecycle transition order
```

No global ordering is assumed across different ReadingSessions or modules.

---

# 51. Startup Ordering

A typical startup may produce:

```text id="evp892"
ReadingSessionCreated
        ↓
ReadingContextPrepared
        ↓
ReadingSessionActivated
```

or:

```text id="48e7cl"
ReadingSessionCreated
        ↓
ReadingSessionActivated
        ↓
ReadingContextPrepared
```

depending on whether activation requires initial context.

The final choice must be fixed by `STATES.md`/command semantics.

The event model itself must reflect committed transition order.

---

# 52. Context Update Ordering

Example:

```text id="zk0euu"
Revision 10 committed
        ↓
new domain command
        ↓
Revision 11 committed
        ↓
ReadingContextChanged revision 11
```

If specialized events are emitted for the same commit:

```text id="wa4xs3"
ReadingTargetChanged revision 11
ReadingContextChanged revision 11
```

their relative ordering must be documented if consumers depend on it.

Prefer avoiding such dependency.

---

# 53. Session Completion Ordering

Typical:

```text id="6vc8ue"
ReadingSessionCompleted
        ↓
ReadingSessionDisposed
```

if disposal immediately follows completion.

A completed session may also remain retained before disposal.

---

# 54. Cancellation Ordering

Typical:

```text id="1p5mda"
ReadingSessionCancelled
        ↓
ReadingSessionDisposed
```

Runtime cancellation events may occur before or after due to independent ownership.

Consumers MUST NOT infer Runtime cancellation completion from ReadingSessionCancelled ordering.

---

# 55. No Cross-Module Global Order

Do not assume:

```text id="x3jzo7"
ReadingContextChanged
must globally occur before
RuntimeRevisionCreated
```

at transport level.

Use causation/correlation and owner-specific revisions.

---

# 56. Event Idempotency

Duplicate delivery of the same:

```text id="x6c0xd"
EventId
```

must not create duplicate logical consumer effects.

Consumers should implement idempotency according to Event Bus delivery profile.

---

# 57. Event Delivery Semantics

Actual delivery guarantees belong to Event Bus architecture.

Reading Session does not redefine:

```text id="rl7de2"
at-most-once
at-least-once
durability
replay
ordering implementation
```

Those are infrastructure/runtime profile decisions.

---

# 58. Event Replay

If Event Bus or Storage supports replay:

* historical event contents remain immutable;
* replay does not mutate historical facts;
* consumers must remain idempotent;
* replay does not make stale ReadingContextRevision current.

Current state comes from Reading Session state/query or authoritative projection.

---

# 59. Events Are Not State

Event:

```text id="3br4k7"
ReadingContextChanged revision 20
```

means revision 20 was committed at that point.

State query may later return:

```text id="r0n302"
ReadingContextRevision 25
```

Historical event remains correct.

---

# 60. Events Are Not Query Replacement

Consumers needing current Reading Session state should use queries such as:

```text id="cjhc3y"
GetReadingSession
GetReadingContext
GetReadingContextRevision
GetSessionState
```

Event delivery alone should not be treated as guaranteed current-state storage.

---

# 61. Event Publication Failure

Reading Session state commit and event publication are separate technical operations.

If:

```text id="rbj7h7"
ReadingContextRevision 15 committed
```

but event publication fails:

```text id="9phr2q"
Revision 15 remains current.
```

Do not roll back the valid domain commit merely to recreate the event.

---

# 62. Publication Failure Recovery

Possible infrastructure recovery:

```text id="4umlfn"
outbox
retry publication
projection reconciliation
query-based recovery
```

Those mechanisms belong to infrastructure/Application policy.

Reading Session does not rerun the domain command automatically.

---

# 63. Privacy

Reading Session event payloads should avoid unnecessary raw reading content.

Prefer:

```text id="1j25gl"
ReadingSessionId
ReadingContextRevision
source identifiers
target identifiers
language values
bounded configuration summary
change flags
```

Avoid:

* screenshots;
* full page text;
* full translation;
* raw HTML;
* authentication data;
* provider secrets;
* native handles.

---

# 64. Source Locator Privacy

ReadingSource locators may contain sensitive information.

Events SHOULD avoid publishing complete URLs, filesystem paths, or private identifiers unless required.

Prefer stable opaque source identifiers plus bounded metadata.

---

# 65. Event Versioning

Each event has its own semantic version.

Compatible additions may include:

* new optional fields;
* new optional metadata;
* compatible enum extensions.

Major version required when:

* event meaning changes;
* required field removed;
* ownership changes;
* ReadingContextRevision semantics change.

---

# 66. Module Version Migration

Reading Session EVENTS v3 is a major revision because it removes:

```text id="zpgof6"
ContentRevision lifecycle events
ProcessingIntent lifecycle events
```

and replaces them with committed ReadingContextRevision semantics.

---

# 67. Stable Event Set

Recommended v3 core:

```text id="p73dii"
ReadingSessionCreated
ReadingSessionActivated
ReadingSessionPaused
ReadingSessionResumed
ReadingSessionCompleted
ReadingSessionCancelled
ReadingSessionDisposed

ReadingContextPrepared
ReadingContextChanged
ReadingContextInvalidated
ReadingContextDisposed
```

Optional specialized facts:

```text id="k6d0zr"
ReadingTargetChanged
ReadingSourceChanged
ReadingPositionChanged
ReadingConfigurationChanged
ReadingLanguageChanged
ReadingModeChanged
```

---

# 68. Event Error Semantics

Reading Session events themselves should not expose Runtime execution errors.

If a Reading Session command was rejected:

```text id="2d06wx"
no success domain fact is emitted
```

A separate `ReadingSessionCommandRejected` event is not required for MVP.

Errors are returned through command contracts and diagnostics.

---

# 69. Failure Facts

Reading Session does not require a general:

```text id="oovnzf"
ReadingSessionFailed
```

event in v3 because the lifecycle has no generic `FAILED` state.

Domain invalidity is represented by:

```text id="ya577v"
ReadingContextInvalidated
```

and activity termination by:

```text id="wscacp"
ReadingSessionCancelled
```

A future fatal-domain event may be introduced only if it has distinct business meaning.

---

# 70. Domain Facts vs Diagnostics

Business events are stable architectural facts.

Diagnostics may record:

```text id="si7ahq"
candidate validation failed
revision conflict
no-op
duplicate command
publication latency
```

without promoting every condition to a business event.

---

# 71. Observability

Useful event metrics:

```text id="m83au9"
reading_session_event_published_total
reading_session_event_publish_failed_total
reading_context_revision_total
reading_context_changed_total
reading_context_invalidated_total
reading_session_lifecycle_transition_total
```

Avoid user-content labels.

---

# 72. Testing — Ownership

Tests MUST verify Reading Session never publishes:

```text id="u03tk6"
ProcessingIntentPublished
ProcessingIntentAccepted
ProcessingIntentFulfilled
ContentRevisionSuperseded
RuntimeRevisionSuperseded
WorkItemCompleted
AttemptCancelled
TranslationCompleted
PresentationUpdated
```

---

# 73. Testing — Commit Timing

Tests MUST verify:

```text id="urkrcc"
state commit
before
success event
```

for:

* ReadingSessionCreated;
* ReadingSessionActivated;
* ReadingContextPrepared;
* ReadingContextChanged;
* ReadingSessionPaused;
* ReadingSessionResumed;
* ReadingSessionCompleted;
* ReadingSessionCancelled;
* ReadingSessionDisposed.

---

# 74. Testing — Revision

Tests MUST verify:

* ReadingContextChanged carries committed revision;
* previous revision is correct;
* no-op emits no context-change event;
* rejected candidate emits no success fact;
* revision never decreases;
* RuntimeRevisionId is not used as Reading Session event ordering.

---

# 75. Testing — Lifecycle Independence

Tests MUST verify:

```text id="60mygi"
ReadingSessionCancelled
```

does not imply a Runtime cancellation event was already published.

Likewise:

```text id="q69uun"
ReadingSessionCompleted
```

does not imply Runtime work completed.

---

# 76. Testing — Processing Independence

Tests MUST verify:

* OCR success/failure does not directly produce Reading Session events;
* Translation completion does not directly produce Reading Session lifecycle events;
* Presentation commit does not mutate ReadingContext;
* UI apply failure does not mutate ReadingSession events.

---

# 77. Testing — Event Publication Failure

Tests should verify:

```text id="5q6atl"
domain commit succeeds
event publication fails
```

results in:

```text id="3y37jj"
domain state remains committed
```

without duplicate domain execution.

---

# 78. Architecture Invariants

1. Reading Session events describe Reading Session-owned facts only.

2. Events describe facts, not commands.

3. Reading Session events publish only after domain commit.

4. Candidate state produces no success event.

5. ReadingContextRevision is carried by committed context facts.

6. ReadingContextRevision is not a lifecycle event family.

7. ContentRevision lifecycle events are removed.

8. ProcessingIntent events are removed.

9. Runtime execution events remain Runtime-owned.

10. Processing-module events remain module-owned.

11. Presentation events remain Presentation-owned.

12. UI rendering events remain UI Adapter-owned.

13. ReadingSessionCancelled does not mean Runtime Attempt cancellation.

14. ReadingSessionCompleted does not mean processing completion.

15. ReadingContextInvalidated does not mean processing failure.

16. Events do not replace Business Pipeline Orchestration.

17. Processing modules should not self-orchestrate directly from Reading Session events.

18. External UI/browser facts should normally become commands through adapters/Application.

19. Events are immutable.

20. EventId supports idempotency.

21. Ordering is scoped, not global.

22. ReadingContextRevision orders Reading Context state only.

23. Event publication failure does not roll back committed domain state.

24. Replay does not change historical facts.

25. Event payloads remain serializable and platform-independent.

26. Normal payloads avoid raw reading content.

---

# 79. Related Documents

```text id="9u5vwg"
doc/02-modules/reading-session/MODULE.md
doc/02-modules/reading-session/CONTRACT.md
doc/02-modules/reading-session/STATES.md
doc/02-modules/reading-session/ERRORS.md
doc/02-modules/reading-session/README.md

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
doc/01-architecture/core/STATE_MACHINE.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
```

---

# 80. Completion Criteria

This event specification is synchronized when:

* all events belong to Reading Session-owned domain facts;
* ProcessingIntent events are absent;
* ContentRevision lifecycle events are absent;
* ReadingContextRevision appears only as committed domain version;
* Runtime authority is absent from event ownership;
* events publish after state commit;
* lifecycle events remain independent from Runtime lifecycle;
* context facts do not imply pipeline execution;
* external UI/browser signals are normalized outside Reading Session;
* event publication failure does not invalidate committed state;
* event ordering uses ReadingSession/ReadingContext semantics;
* tests cover ownership, revision, lifecycle independence, publication timing, and privacy.

---

# 81. Summary

Reading Session v3 event flow is:

```text id="2ws7ny"
Application / User Domain Intent
        ↓
Reading Session Command
        ↓
Domain Validation
        ↓
Candidate Reading Context / Lifecycle Change
        ↓
Atomic Commit
        ↓
ReadingContextRevision if applicable
        ↓
Reading Session-owned Fact
        ↓
Business Pipeline Orchestration / Other Consumers
```

Core event ownership is:

```text id="2ollne"
Reading Session
    → reading-domain facts

Business Pipeline Orchestration
    → processing requirement decisions

Runtime
    → execution facts

Processing Modules
    → processing-result facts

Presentation
    → presentation facts

UI Adapter
    → rendering/apply facts
```

The central rule is:

```text id="w8trrc"
Reading Session events say
what changed in the reading world.

They do not say
what processing should run
or whether execution is still authoritative.
```
