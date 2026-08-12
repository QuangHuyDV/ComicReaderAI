# CRAI Event Convention

> **Project:** CRAI
> **Path:** `doc/01-architecture/core/EVENT_CONVENTION.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines the architecture-wide naming, ownership and design convention for CRAI events.

Every module that defines or publishes an Event Bus event must follow this specification.

This document standardizes:

```text
event semantics
event naming
event ownership
event payload design
event identity
authority references
versioning
publication timing
ordering expectations
privacy
compatibility
```

The goal is to keep CRAI events:

```text
factual
explicit
owner-defined
stable
small
serializable
privacy-safe
independent from implementation
```

---

# 2. Central Rule

A CRAI event describes a fact that has already become true.

```text
Owner commits fact
    ↓
Event
```

An event does not ask something to happen.

Therefore:

```text
ReadingContextChanged
PreferenceChanged
TranslationArtifactPublished
RuntimeRevisionSuperseded
```

may be events.

But:

```text
StartTranslation
TranslationRequested
RetryPipeline
CancelAttempt
RenderNow
```

are not events.

---

# 3. Event vs Command

A Command asks an owner to perform an action.

```text
Command
    ↓
Owner
```

An Event reports what an owner already committed.

```text
Owner
    ↓
Event
    ↓
0..N Consumers
```

Never hide commands inside the Event Bus.

---

# 4. Event vs UiIntent

A UiIntent expresses semantic user intention.

Examples:

```text
StartReadingIntent
RetryCurrentOperationIntent
SavePreferenceIntent
```

These are not Event Bus facts.

Typical flow:

```text
Native UI Interaction
    ↓
UiIntent
    ↓
Application Command
    ↓
Owner commits state
    ↓
Event
```

---

# 5. Event vs Telemetry

Telemetry reports operational measurements.

Examples:

```text
operation duration
metric counter
trace span
log record
```

These are not business Event Bus events by default.

Do not create events such as:

```text
LogRecorded
MetricUpdated
TraceStarted
TraceCompleted
```

merely for observability.

---

# 6. Event vs UI-Local Event

UI-local facts such as:

```text
ViewOpened
DialogResponded
NotificationShown
WindowResized
```

normally remain in the UI layer.

They enter the business Event Bus only if:

1. the fact is architecturally meaningful outside the UI;
2. UI Adapter owns it;
3. another module genuinely requires asynchronous awareness.

---

# 7. Event Naming Pattern

Preferred pattern:

```text
<Subject><PastTenseVerb>
```

Examples:

```text
ReadingContextChanged
PreferenceChanged
PermissionRevoked
ArtifactPublished
FrontendDisconnected
RuntimeRevisionSuperseded
```

The name describes a completed fact.

---

# 8. PascalCase

CRAI public event names use:

```text
PascalCase
```

Examples:

```text
ReadingSessionStopped
TranslationArtifactPublished
DiagnosticCapabilityChanged
```

Do not mix with legacy:

```text
TRANSLATION_COMPLETED
SESSION_STOPPED
```

in current-authority contracts.

---

# 9. Past Tense

Event verbs should describe something already committed.

Good:

```text
Created
Changed
Published
Revoked
Granted
Paused
Resumed
Stopped
Superseded
Failed
TimedOut
Recovered
Disconnected
```

Bad:

```text
Create
Change
Publish
Start
Retry
Cancel
Request
Execute
Process
```

---

# 10. Never Use Request Semantics

Do not use:

```text
Requested
Request
Command
Execute
Process
Do
Run
Now
Action
```

as Event Bus command semantics.

Examples to avoid:

```text
TranslationRequested
CaptureRequested
RetryRequested
PipelineCancelRequested
SessionStartRequested
```

These belong to explicit command contracts.

---

# 11. Subject Rule

The subject should name the architecture concept that owns the fact.

Examples:

```text
ReadingSession
ReadingContext
Preference
CaptureSource
RecognitionArtifact
TranslationArtifact
PresentationArtifact
DiagnosticCapability
UiCapability
RuntimeRevision
WorkItem
Attempt
Provider
```

---

# 12. Subject Is Not Limited to Business Domain Objects

v1 prohibited subjects such as:

```text
Runtime
Scheduler
Pipeline
```

because they looked implementation-specific.

Runtime v2 requires a more precise rule:

> An event subject may represent any stable architecture-owned concept.

Therefore valid examples may include:

```text
RuntimeRevisionSuperseded
WorkItemSucceeded
AttemptTimedOut
```

if Runtime owns those facts and external consumers need them.

---

# 13. Implementation Objects Remain Forbidden

Do not use implementation objects as event subjects.

Examples:

```text
ThreadFinished
WorkerDone
ControllerUpdated
ReactComponentMounted
SqlRepositoryFailed
GrpcClientDisconnected
```

unless the event belongs explicitly to infrastructure diagnostics rather than the business Event Bus.

---

# 14. `Pipeline` Naming

Avoid using generic:

```text
PipelineStarted
PipelineCompleted
PipelineFailed
```

as architecture events.

In Runtime v2, `pipeline` is too ambiguous between:

```text
business pipeline topology
Runtime execution
WorkItem graph
legacy processing pipeline instance
```

Prefer the actual owner concept.

---

# 15. Verb Selection

Prefer the most specific factual verb.

Example:

```text
PermissionRevoked
```

is better than:

```text
PermissionChanged
```

Likewise:

```text
TranslationArtifactPublished
```

is usually stronger than:

```text
TranslationCompleted
```

when publication is the meaningful architectural fact.

---

# 16. `Changed`

Use `Changed` when the meaningful fact really is a state/revision change.

Good:

```text
ReadingContextChanged
PreferenceChanged
DiagnosticCapabilityChanged
UiCapabilityChanged
```

Do not use `Changed` when a specific transition verb carries more meaning.

Prefer:

```text
PermissionRevoked
```

over:

```text
PermissionChanged
```

---

# 17. `Updated`

Use `Updated` carefully.

It is often too generic.

Prefer a domain-specific fact when possible.

Example:

```text
PreferenceChanged
```

is usually clearer than:

```text
PreferenceUpdated
```

if the important meaning is a new committed preference revision.

---

# 18. `Published`

Use `Published` when an Artifact or projection becomes authoritative and externally visible according to its owner contract.

Example:

```text
RecognitionArtifactPublished
TranslationArtifactPublished
PresentationArtifactPublished
```

`Published` must not mean:

```text
provider call returned
Candidate created
temporary output produced
```

---

# 19. `Produced`

Use `Produced` for an output that has been created but is not necessarily authoritative.

Example:

```text
FrameProduced
```

may be appropriate for a Capture-local stream.

However, if CRAI distinguishes Candidate from Published Artifact:

```text
Candidate produced
    ≠
Artifact published
```

the event name must reflect the correct boundary.

---

# 20. `Completed`

Use `Completed` only when completion itself is a meaningful externally observable fact.

Examples may include:

```text
DiagnosticExportCompleted
MigrationCompleted
```

for scoped operations.

Avoid generic stage events such as:

```text
RecognitionCompleted
TranslationCompleted
```

when the actual architecture contract is Artifact publication.

---

# 21. `Started`

Do not publish `Started` for every internal operation.

Example:

```text
TranslationStarted
RecognitionStarted
```

should not automatically exist just because Runtime began an Attempt.

Runtime already owns execution state.

Publish a `Started` event only if:

1. the owner defines a meaningful committed lifecycle transition;
2. another component genuinely needs asynchronous awareness.

---

# 22. `Failed`

`Failed` events require careful ownership.

An Attempt failure is different from a module failure.

Example:

```text
AttemptFailed
```

may be Runtime-owned.

A provider timeout does not imply:

```text
TranslationFailed
```

unless Translation's domain operation itself has reached a committed failed outcome.

---

# 23. Failure Event vs Error Result

Not every error requires Event Bus publication.

Often:

```text
command
    ↓
error result
```

is enough.

Use a failure event only when an asynchronous consumer needs that committed failure fact.

---

# 24. `TimedOut`

Use `TimedOut` for a committed timeout state owned by the relevant component.

Example:

```text
AttemptTimedOut
```

Do not create:

```text
TranslationTimedOut
```

if the timeout belongs only to one Runtime Attempt.

---

# 25. `Cancelled`

Use only after cancellation has committed.

Correct:

```text
AttemptCancelled
```

Incorrect:

```text
AttemptCancelRequested
```

on Event Bus.

Cancellation request belongs to Runtime/Application command contracts.

---

# 26. `Superseded`

Use for old authority that is replaced by newer authority.

Examples:

```text
RuntimeRevisionSuperseded
WorkItemSuperseded
```

Supersession is usually control flow, not an error.

---

# 27. `Recovered`

Use when an owner had a degraded/unavailable condition and confirms recovery.

Examples:

```text
DiagnosticCapabilityRecovered
ProviderRecovered
FrontendRecovered
```

provided the module-specific event contract actually requires the event.

---

# 28. Event Ownership

Every event has exactly one semantic owner.

The owner is the component that can truthfully commit the fact.

Example:

```text
ReadingContextChanged
        ↓
owner = Reading Session
```

Only Reading Session may publish that fact.

---

# 29. Owner vs Publisher Infrastructure

The semantic owner and physical publisher mechanism are different.

Example:

```text
Reading Session
    owns ReadingContextChanged

Event Bus Infrastructure
    transports the event
```

Infrastructure does not become semantic owner.

---

# 30. One Semantic Owner

Do not allow:

```text
Reading Session
Application
UI Adapter
```

all to publish:

```text
ReadingContextChanged
```

The fact has one owner.

Other components may publish their own projection/state facts if necessary.

---

# 31. Event Authority

Receiving an event does not transfer authority.

Example:

```text
PreferenceChanged
    ↓
UI Adapter
```

UI Adapter may rebuild SettingsViewModel.

It cannot mutate Preference state merely because it received the event.

---

# 32. Publish After Commit

Every event must represent already committed state.

Correct:

```text
commit ReadingContextRevision 17
    ↓
ReadingContextChanged
```

Incorrect:

```text
ReadingContextChanged
    ↓
attempt commit
```

---

# 33. Event Publication Failure

If owner commit succeeded but Event Bus publication fails:

```text
committed owner state remains authoritative
```

Do not roll back valid state solely because notification failed.

---

# 34. Artifact Publication Event

Canonical sequence:

```text
Attempt completes
    ↓
Candidate Artifact
    ↓
authority validation
    ↓
Published Artifact
    ↓
ArtifactPublished event
```

Never publish an `ArtifactPublished` event for a Candidate.

---

# 35. Event Payload

An event payload should contain only what consumers need to understand the fact.

Prefer:

```text
identity
typed revisions
safe metadata
references
small summaries
```

---

# 36. Payload Must Be Immutable

Once the event is published:

```text
payload
```

must not mutate.

---

# 37. Payload Must Be Serializable

Stable public events should be serializable where practical.

Do not expose:

```text
callback
closure
DOM node
native window handle
Qt pointer
framework component
provider SDK object
mutable entity reference
```

---

# 38. Large Data

Do not include large raw values such as:

```text
raw screenshot
pixel buffer
full OCR document
full translation document
large binary
diagnostic bundle
```

Prefer stable references.

---

# 39. Reference Examples

Possible references:

```text
ArtifactId
ArtifactRef
ContentRef
BlobRef
SnapshotRef
```

Exact reference type must belong to an explicit architecture contract.

---

# 40. No Raw Reading Content by Default

Business events should not normally contain:

```text
recognizedText
sourceText
translatedText
clipboard content
raw HTML
```

Use references and safe summary metadata.

---

# 41. Sensitive Data

Event payloads MUST NOT contain:

```text
API keys
access tokens
cookies
private keys
passwords
provider credentials
```

---

# 42. Event Envelope Identity

Every Event Bus event must have:

```text
EventId
EventName
EventVersion
OccurredAt
SourceModule
Payload
```

Depending on context it may also have:

```text
PublishedAt
CorrelationId
CausationId
ApplicationInstanceId
AuthorityRefs
Metadata
```

---

# 43. EventId

`EventId` uniquely identifies one event occurrence.

It is used for:

```text
deduplication
diagnostics
delivery tracking
debugging
```

---

# 44. EventName / EventType

Use one canonical concept in implementation.

Recommended architecture term:

```text
EventName
```

Example:

```text
ReadingContextChanged
```

Avoid maintaining both `EventType` and `EventName` unless the implementation gives them explicitly different meanings.

---

# 45. EventVersion

`EventVersion` identifies schema compatibility.

Example:

```text
ReadingContextChanged v1
ReadingContextChanged v2
```

It is not a state revision.

---

# 46. OccurredAt

Represents when the committed fact became true.

---

# 47. PublishedAt

Represents when the event entered Event Bus.

This may be later than `OccurredAt`.

---

# 48. CorrelationId

Groups related operations/facts.

It is not event identity.

Example:

```text
StartReadingIntent
ReadingContextChanged
RuntimeRevisionCreated
PresentationArtifactPublished
```

may share one CorrelationId.

---

# 49. CausationId

Represents the immediate causal operation/fact when useful.

It may refer to:

```text
CommandId
IntentId
EventId
WorkItemId
AttemptId
```

Do not assume every event is caused by another event.

---

# 50. TraceId

Trace correlation may exist through observability context.

It should not be treated as required business identity.

Tracing ownership belongs to Diagnostics/Telemetry infrastructure.

---

# 51. Typed Authority References

Events should carry only owner-relevant typed authority references.

Examples:

```text
SessionId
ReadingContextRevision

PreferenceRevision

RuntimeRevisionId
WorkItemId
AttemptId

ArtifactId
ArtifactVersion

PresentationRevision

FrontendId
```

---

# 52. No Universal Legacy Identifier Set

Do not require every event to carry:

```text
pipelineId
taskId
contentRevision
```

Those are legacy generic concepts.

Use the actual owner-specific identities instead.

---

# 53. Revision Type Matters

These are distinct:

```text
ReadingContextRevision
PreferenceRevision
RuntimeRevisionId
PresentationRevision
ViewModelRevision
```

Do not collapse them into:

```text
revision
contentRevision
version
```

without typed meaning.

---

# 54. EventVersion vs Domain Revision

Example:

```text
eventVersion = 2
readingContextRevision = 18
```

These numbers have unrelated semantics.

---

# 55. Event Ordering

CRAI does not guarantee global ordering.

---

# 56. Owner/Aggregate Ordering

Where ordering matters, scope it to an explicit owner/aggregate.

Example:

```text
Reading Session S
Revision 12
Revision 13
Revision 14
```

Consumers should prefer authoritative revisions over event arrival order.

---

# 57. No Pipeline Stage Ordering Convention

Do not encode assumptions such as:

```text
RecognitionCompleted
must occur before
TranslationStarted
```

as Event Bus ordering law.

Runtime WorkItems may overlap.

Execution ordering belongs to Runtime dependency contracts.

---

# 58. Event Arrival Can Be Out of Order

Example:

```text
PreferenceChanged revision 13
```

may arrive before:

```text
PreferenceChanged revision 12
```

A consumer must not allow revision 12 to overwrite revision 13.

---

# 59. Stale Event

A stale event is a delivery/projection concern.

Example:

```text
received revision 12
current projection revision 13
```

Result:

```text
ignore event
or
resynchronize snapshot
```

---

# 60. Stale Event Is Not Stale Candidate

A stale Candidate Artifact is rejected before publication by Runtime/owner authority.

A stale Event is an already-published fact arriving too late to matter to a consumer.

Do not merge these concepts.

---

# 61. Deduplication

Canonical event deduplication identity:

```text
EventId
```

for a given subscriber.

---

# 62. Do Not Deduplicate by Business Fields

Do not treat:

```text
SessionId + EventName
RuntimeRevisionId + EventName
ArtifactId + EventName
```

as automatically duplicate.

Two legitimate event occurrences may share those values.

---

# 63. Correlation Is Not Deduplication

Events with the same CorrelationId may represent different facts.

---

# 64. Event Versioning

Compatible changes may include:

```text
adding optional fields
adding optional safe metadata
adding enum values with documented unknown handling
```

---

# 65. Breaking Event Changes

Require a new schema version when:

```text
required field removed
field type changed
semantic meaning changed
ownership changed
required authority reference changed
optional field becomes mandatory
```

---

# 66. Event Name Semantic Stability

Never reuse the same event name for a different fact.

If:

```text
TranslationArtifactPublished
```

means an Artifact became authoritative, later versions must preserve that meaning.

---

# 67. Event Compatibility

Consumers should explicitly handle unsupported versions by:

```text
rejecting
ignoring safely
falling back
resynchronizing authoritative state
```

depending on contract.

---

# 68. Event Registry

Every public event should be registered/documented with:

```text
eventName
ownerModule
schemaVersion
payload schema
authority references
ordering scope if any
privacy classification
delivery importance
```

---

# 69. Module-Specific Authority

Exact event definitions belong to:

```text
doc/02-modules/<module>/EVENTS.md
```

`EVENT_CONVENTION.md` defines shared rules only.

---

# 70. Architecture Examples Are Non-Authoritative Catalog

Examples in this file illustrate naming conventions.

They do not create new events by themselves.

An event becomes canonical only when its owner module defines it.

---

# 71. Event Consumer Rule

Consumers subscribe because they need asynchronous awareness of a fact.

Do not subscribe merely because an event exists.

---

# 72. No Event Proliferation

Avoid creating:

```text
OperationStarted
OperationProgressChanged
OperationCompleted
OperationFailed
```

for every function call.

Most operations should simply:

```text
return a result
```

or be represented through Runtime state.

---

# 73. When to Create an Event

Create an event when all are true:

1. a stable fact has committed;
2. the fact has a clear owner;
3. at least one asynchronous consumer has a legitimate need;
4. direct command/query coupling is inappropriate;
5. payload can remain stable and safe.

---

# 74. When Not to Create an Event

Do not create an event solely for:

```text
function completion
debugging
metrics
logging
tracing
UI animation
button click
retry request
cache lookup request
provider request
internal stage transition
```

---

# 75. Capture Event Naming

Prefer owner-semantic facts defined in Capture `EVENTS.md`.

Potential naming shapes:

```text
CaptureSourceChanged
CaptureCapabilityChanged
CaptureArtifactPublished
```

Avoid assuming generic:

```text
CaptureStarted
CaptureCompleted
CaptureFailed
```

unless those are actual committed Capture-domain facts.

---

# 76. Recognition Event Naming

Prefer:

```text
RecognitionArtifactPublished
RecognitionCapabilityChanged
```

where those reflect the module contract.

Avoid using:

```text
TextRecognized
BubbleDetected
RecognitionCompleted
```

as global canonical events merely because internal Recognition operations occurred.

---

# 77. Text Processing Event Naming

Text Processing owns SourceDocument/structured text semantics.

Possible fact-oriented shape:

```text
SourceDocumentArtifactPublished
```

Exact name belongs to its `EVENTS.md`.

Do not create:

```text
SegmentationRequested
SegmentationCompleted
```

as generic pipeline control events.

---

# 78. Translation Event Naming

Prefer facts tied to Translation-owned contracts such as:

```text
TranslationArtifactPublished
TranslationCapabilityChanged
```

Do not create:

```text
TranslationRequested
TranslationRetryRequested
TranslationFallbackRequested
```

as business events.

---

# 79. Presentation Event Naming

Presentation owns semantic presentation output.

Prefer module-defined factual names such as:

```text
PresentationArtifactPublished
```

where appropriate.

Avoid confusing native rendering with Presentation business semantics.

---

# 80. Reading Session Event Naming

Potential shapes:

```text
ReadingSessionCreated
ReadingSessionPaused
ReadingSessionResumed
ReadingSessionStopped
ReadingContextChanged
```

Use exact names defined in Reading Session `EVENTS.md`.

---

# 81. Preferences Event Naming

Prefer:

```text
PreferenceChanged
```

or the exact Preferences-owned event.

Do not define a universal:

```text
EffectivePreferencesChanged
```

because effective preferences are contextual.

---

# 82. Diagnostics Event Naming

Only Diagnostics-owned capability/state facts belong on Event Bus.

Examples may include:

```text
DiagnosticCapabilityChanged
DiagnosticCollectionDegraded
DiagnosticCollectionRecovered
```

Do not create:

```text
LogRecorded
MetricUpdated
TraceCompleted
```

as business events.

---

# 83. UI Adapter Event Naming

Most UI events remain local.

Possible global facts should be restricted to meaningful adapter capability/frontend changes.

Examples:

```text
UiCapabilityChanged
FrontendDisconnected
```

only when Application needs them.

---

# 84. Runtime Event Naming

Runtime-owned factual events may use Runtime concepts.

Examples:

```text
RuntimeRevisionSuperseded
WorkItemSucceeded
AttemptTimedOut
```

only when there is a real asynchronous consumer.

Runtime events must not become Runtime's own command mechanism.

---

# 85. Provider Event Naming

Provider-management facts may include:

```text
ProviderAvailabilityChanged
ProviderRateLimited
ProviderRecovered
```

if Provider Management owns those facts.

Avoid:

```text
ProviderValidationRequested
ProviderFallbackRequested
```

as Event Bus events.

---

# 86. Application Event Naming

Application lifecycle facts may include:

```text
ApplicationReady
ApplicationDegraded
ApplicationStopping
ApplicationStopped
```

Command forms such as:

```text
ApplicationShutdownRequested
```

belong to Application lifecycle control, not Event Bus convention.

---

# 87. Error Events

Do not introduce one generic:

```text
ErrorOccurred
```

for all modules.

Errors retain their original module ownership.

---

# 88. Error Observation vs Error Event

Diagnostics may observe an error without requiring an Event Bus event.

Example:

```text
TRN-PROV-003
    ↓
Diagnostics ObserveError
```

No:

```text
TranslationErrorOccurred
```

event is automatically necessary.

---

# 89. Failure Event Payload

If a module genuinely defines a failure event, payload should reference:

```text
original ErrorCode
safe classification
relevant authority identity
diagnosticRef?
```

Do not embed raw exception details.

---

# 90. Event Payload Example

Conceptual:

```text
TranslationArtifactPublished
├── translationArtifactId
├── runtimeRevisionId?
├── sourceDocumentArtifactId
├── targetLanguage
└── publishedAt
```

The actual schema belongs to Translation `EVENTS.md`.

---

# 91. State Event Example

Conceptual:

```text
ReadingContextChanged
├── sessionId
├── previousReadingContextRevision
├── readingContextRevision
├── changedFields
└── changedAt
```

Exact schema belongs to Reading Session.

---

# 92. Capability Event Example

Conceptual:

```text
UiCapabilityChanged
├── capability
├── previousState
├── state
├── frontendId?
└── changedAt
```

Exact schema belongs to UI Adapter.

---

# 93. Event Payload Does Not Carry Mutable Authority

An event describes authority.

It does not provide a mutable object that lets consumers alter the owner's state.

---

# 94. Event Payload References

An `ArtifactId` in an event means:

```text
this fact refers to Artifact X
```

It does not mean:

```text
consumer now owns Artifact X
```

---

# 95. Metadata

Metadata must remain optional for business logic unless explicitly promoted into the typed payload contract.

Examples:

```text
transport diagnostics
trace context
safe implementation hint
```

Do not hide required business fields in generic metadata.

---

# 96. Logging Convention

When logging event dispatch, prefer:

```text
EventId
EventName
EventVersion
SourceModule
CorrelationId
OccurredAt
PublishedAt
delivery outcome
```

and only owner-relevant safe references.

---

# 97. Metric Convention

Event Bus metrics may use:

```text
eventName
sourceModule
consumerModule
deliveryOutcome
```

Avoid high-cardinality identity fields as metric labels.

---

# 98. Event Priority

Priority is a transport concern, not domain meaning.

If Event Bus infrastructure supports priority, it must not alter factual semantics.

Do not encode business logic like:

```text
Critical event overrides owner state
```

---

# 99. Delivery Semantics

Naming convention does not guarantee:

```text
at-most-once
at-least-once
ordering
durability
```

These are defined by `EVENT_BUS.md` and infrastructure.

---

# 100. Exactly-Once Assumption

Event consumers must not require exactly-once transport for correctness.

Use:

```text
EventId
idempotent behavior
owner revision checks
snapshot resynchronization
```

where appropriate.

---

# 101. Event Registration Checklist

Before adding a new Event Bus event, answer:

1. What committed fact does this describe?

2. Which component owns that fact?

3. Has the owner committed it before publication?

4. Is the name past tense?

5. Is the subject a stable architecture-owned concept?

6. Is there an actual asynchronous consumer?

7. Could this be a Command, Query, UiIntent or direct result instead?

8. Could this be Telemetry or a UI-local event instead?

9. Is the payload minimal?

10. Does the payload avoid large/raw content?

11. Does it use typed authority references?

12. Does it preserve privacy?

13. Is the schema versioned?

14. What ordering scope, if any, is required?

15. Can consumers recover if the event is missed?

---

# 102. Forbidden Patterns

Do not create events matching these architecture patterns:

```text
*Requested
*Request
Do*
Run*
Execute*
Process*

PipelineStarted
PipelineCompleted
PipelineFailed

RetryRequested
FallbackRequested
CancelRequested

LogRecorded
MetricUpdated
TraceStarted
TraceCompleted

ButtonClicked
ViewOpened
DialogOpened
WindowResized
```

unless a module-specific architecture explicitly documents a justified exception.

---

# 103. Deprecated v1 Examples

The following v1 examples should not be treated as current canonical events:

```text
CaptureStarted
CaptureCompleted
RecognitionStarted
RecognitionCompleted
TranslationStarted
TranslationCompleted
OverlayRendered
TranslationDisplayed
```

They may still be valid names in a specific owner contract if their semantics are explicitly re-established.

Do not assume them globally.

---

# 104. Why Generic Stage Events Are Deprecated

The old pattern suggests:

```text
stage starts
stage completes
next stage starts
```

Runtime v2 instead uses:

```text
WorkItem
    ↓
Attempt
    ↓
Candidate Artifact
    ↓
authority validation
    ↓
Published Artifact
```

Event naming should reflect stable externally meaningful facts, not internal stage orchestration.

---

# 105. Compatibility with Module EVENTS Files

If this convention conflicts with an older module `EVENTS.md`:

```text
the module document must be updated
```

Do not silently reinterpret an old event.

---

# 106. Compatibility with EVENT_BUS.md

`EVENT_CONVENTION.md` defines:

```text
what a valid event looks like
```

`EVENT_BUS.md` defines:

```text
how valid events are distributed
```

Neither document should redefine the other's ownership.

---

# 107. Compatibility with STATE_MACHINE.md

`STATE_MACHINE.md` defines:

```text
who owns state
how state commits
```

This file defines:

```text
how committed facts are named as events
```

Event publication occurs after the owning state/result commit.

---

# 108. Architecture Invariants

1. An Event is a committed fact.

2. An Event is not a Command.

3. An Event is not a Request.

4. An Event is not a UiIntent.

5. An Event is not Telemetry.

6. An Event is not automatically a UI-local event.

7. Public event names use PascalCase.

8. Public event names use past-tense facts.

9. Every event has one semantic owner.

10. Event owner commits the fact before publication.

11. Event publication failure does not undo committed owner state.

12. Implementation object names do not appear in public event names.

13. Runtime-owned architecture concepts may legitimately be event subjects.

14. Generic Pipeline events are discouraged.

15. `Requested` events are prohibited by default.

16. `Started` events are not generated for every Runtime operation.

17. `Completed` events are used only when completion is itself meaningful.

18. Artifact publication events occur only after authority validation.

19. Event payloads are immutable.

20. Event payloads are small.

21. Event payloads are serializable where practical.

22. Event payloads contain no native pointers/framework objects.

23. Event payloads contain no secrets.

24. Raw reading content is excluded by default.

25. EventId uniquely identifies one event occurrence.

26. CorrelationId does not identify an event.

27. CausationId may point to a command, intent, event or Runtime identity.

28. EventVersion is schema version only.

29. Typed authority references replace generic `pipelineId/taskId/contentRevision`.

30. Global ordering is not required.

31. Runtime stage ordering is not inferred from Event Bus order.

32. Consumers use owner revisions for stale projection protection where possible.

33. Stale Event and stale Candidate Artifact are distinct concepts.

34. Event deduplication uses EventId.

35. Events are created only when asynchronous consumers actually need them.

36. Module-specific `EVENTS.md` files own exact event catalogs and payloads.

---

# 109. Minimal Template

When defining a new event:

```text
Event Name:
<Subject><PastTenseVerb>

Owner:
<module>

Meaning:
<one committed fact>

Occurs After:
<authoritative commit>

Authority References:
<typed IDs/revisions required by this fact>

Payload:
<minimal safe fields>

Consumers:
<actual asynchronous consumers>

Ordering Scope:
<none / explicit owner scope>

Version:
v1
```

---

# 110. Example — Good Event Definition

```text
Event Name:
ReadingContextChanged

Owner:
Reading Session

Meaning:
A new ReadingContextRevision became authoritative.

Occurs After:
Reading Session successfully commits the context change.

Authority References:
SessionId
ReadingContextRevision

Payload:
changedFields
previousRevision?
changedAt

Consumers:
Application projection
selected Runtime/application integration

Ordering Scope:
Session

Version:
v1
```

---

# 111. Example — Bad Event Definition

```text
Event Name:
TranslationRequested

Owner:
Pipeline

Meaning:
Please run translation.
```

Invalid because:

```text
not past-tense fact
command semantics
ambiguous owner
hidden Runtime invocation
```

---

# 112. Example — Better Translation Flow

```text
Translation WorkItem
    ↓
Runtime Attempt
    ↓
Candidate TranslationArtifact
    ↓
authority validation
    ↓
Published TranslationArtifact
    ↓
TranslationArtifactPublished
```

---

# 113. Example — Runtime Fact

```text
Event Name:
RuntimeRevisionSuperseded

Owner:
Runtime

Meaning:
A previously authoritative RuntimeRevision was replaced by a newer RuntimeRevision.
```

This is valid even though `Runtime` is not a business-domain noun.

It is a stable architecture owner.

---

# 114. Example — Telemetry That Is Not an Event

```text
translation_operation_duration_ms = 325
```

This belongs to Telemetry.

Do not create:

```text
TranslationDurationRecorded
```

on the business Event Bus.

---

# 115. Example — UI-Local Fact

```text
DialogResponded
```

may remain local to UI Adapter.

The semantic user action produced afterward may become:

```text
SavePreferenceIntent
```

Neither automatically becomes a business Event Bus event.

---

# 116. Example — Provider Availability

Possible:

```text
ProviderAvailabilityChanged
```

if Provider Management owns and commits provider availability state.

Not:

```text
ProviderFallbackRequested
```

which is policy/command semantics.

---

# 117. Example — Error

A Translation provider error:

```text
TRN-PROV-003
```

may simply be returned through Translation/Runtime contracts and observed by Diagnostics.

There is no automatic requirement for:

```text
TranslationFailed
```

Event Bus publication.

---

# 118. Testing — Naming

Reject event definitions that:

```text
use imperative/request verbs
use implementation subjects
lack clear owner
describe future action
```

---

# 119. Testing — Ownership

Verify only one owner can publish each canonical event.

---

# 120. Testing — Commit Ordering

Verify event publication occurs after the owner commit.

---

# 121. Testing — Payload

Verify event payloads contain:

```text
no raw image
no raw reading text
no secret
no mutable object
no native pointer
```

---

# 122. Testing — Typed References

Verify relevant events use:

```text
ReadingContextRevision
RuntimeRevisionId
WorkItemId
AttemptId
ArtifactId
PreferenceRevision
```

rather than ambiguous generic IDs.

---

# 123. Testing — Command Separation

Search canonical events for:

```text
Requested
RetryRequested
FallbackRequested
CancelRequested
```

and verify none are being used as Event Bus commands.

---

# 124. Testing — Stage Chain

Verify event definitions do not imply:

```text
RecognitionCompleted
    ↓
Translation starts
```

or equivalent direct execution control.

---

# 125. Testing — Event Necessity

For every canonical event, verify at least one real asynchronous consumer exists or there is a documented architectural reason for publication.

---

# 126. Related Documents

```text
doc/01-architecture/core/
├── EVENT_CONVENTION.md
├── EVENT_BUS.md
├── STATE_MACHINE.md
├── DATA_FLOW.md
├── CAPABILITY_MAP.md
└── README.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── RETRY_POLICY.md
└── CANCELLATION.md

doc/02-modules/*/EVENTS.md

doc/03-infrastructure/
├── event-bus/
├── logging/
└── telemetry/
```

---

# 127. Documentation Authority

This document defines:

```text
event semantic convention
event naming
event ownership
event payload conventions
event identity
event versioning
fact/command separation
fact/telemetry separation
fact/UI-local separation
```

It does not define:

```text
Event Bus implementation
complete module event catalog
Runtime execution state
module-specific payload schemas
```

---

# 128. Completion Criteria

This document is synchronized when:

* events are fact-only;
* PascalCase past-tense naming is standard;
* request semantics are prohibited;
* Runtime-owned subjects are allowed when architecturally meaningful;
* generic Pipeline subjects are removed from current event guidance;
* generic stage Started/Completed events are no longer assumed;
* ArtifactPublished semantics respect Candidate/Published boundary;
* exact module event catalogs remain module-owned;
* typed authority references replace generic legacy identifiers;
* EventVersion is separated from domain revisions;
* publication-after-commit is explicit;
* telemetry and UI-local events are separated from business events;
* privacy and serialization rules are explicit;
* event necessity requires a real asynchronous consumer.

---

# 129. Summary

A valid CRAI Event follows:

```text
Owner
    ↓
commits state/result
    ↓
<Subject><PastTenseVerb>
    ↓
Event Bus
    ↓
Interested Consumers
```

Examples:

```text
ReadingContextChanged
PreferenceChanged
TranslationArtifactPublished
RuntimeRevisionSuperseded
UiCapabilityChanged
```

Not:

```text
TranslationRequested
RetryRequested
CancelRequested
DoCapture
RunRecognition
```

The central rule is:

```text
Commands ask.

Owners act.

Events report
what is already true.
```
