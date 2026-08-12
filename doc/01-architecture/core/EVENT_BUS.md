# CRAI Event Bus Architecture

> **Project:** CRAI
> **Path:** `doc/01-architecture/core/EVENT_BUS.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines the architecture-wide Event Bus model used by CRAI.

The Event Bus provides asynchronous distribution of committed facts between components without requiring producers to know all consumers.

Its primary role is:

```text
Committed Fact
    ↓
Event
    ↓
Event Bus
    ↓
0..N Consumers
```

The Event Bus does not own:

```text
commands
use-case invocation
Runtime scheduling
WorkItem creation
Attempt creation
retry policy
cancellation authority
pipeline orchestration
module state transitions
telemetry transport
UI-local interaction
```

---

# 2. Central Rule

The core rule is:

```text
An Event describes
something that has already happened.

It does not request
something to happen.
```

Therefore:

```text
PreferenceChanged
ReadingContextChanged
ArtifactPublished
UiCapabilityChanged
```

may be valid events.

But:

```text
StartTranslationRequested
RetryPipelineRequested
CancelAttemptRequested
RenderRequested
```

are commands/intents and must not be disguised as Event Bus events.

---

# 3. Event Bus Position

Preferred architecture:

```text
Commands / Queries
        ↓
Application / Module Contracts
        ↓
Owner commits state/result
        ↓
Event created
        ↓
Event Bus
        ↓
Interested Consumers
```

Execution does not begin because a command-like event happened.

Execution begins because an authoritative contract accepted a command or Runtime admitted work.

---

# 4. Event Bus Is Not a Command Bus

Do not use:

```text
*_REQUESTED
```

as the default Event Bus command pattern.

Examples removed from the architecture:

```text
SESSION_START_REQUESTED
CONTENT_CAPTURE_REQUESTED
OCR_PROCESS_REQUESTED
TRANSLATION_REQUESTED
PIPELINE_CANCEL_REQUESTED
RENDER_REQUESTED
```

These represent intent or command semantics.

They belong to:

```text
Application Commands
Module Contracts
Runtime Contracts
UiIntent
```

depending on owner.

---

# 5. Event Bus Is Not Runtime Scheduler

The Event Bus must not determine:

```text
which WorkItem becomes READY
which Attempt runs
when retry occurs
when cancellation commits
which provider is selected
which work gets queue priority
```

Those belong to Runtime and owning policies.

---

# 6. Event Bus Is Not Pipeline Orchestrator

CRAI v2 does not use:

```text
RecognitionCompleted
    ↓
Event Bus
    ↓
TextProcessingRequested

TextProcessingCompleted
    ↓
Event Bus
    ↓
TranslationRequested
```

as execution control.

Preferred:

```text
BusinessExecutionPlan
        ↓
Runtime dependency graph
        ↓
WorkItem readiness
```

Module events may describe completed facts.

They do not directly command downstream modules.

---

# 7. Event Bus Is Not State Authority

An Event Bus does not own state.

Correct:

```text
Owner validates transition
    ↓
Owner commits state
    ↓
Event published
```

Incorrect:

```text
Event published
    ↓
subscriber decides state
```

The committed owner state remains authoritative even if event delivery later fails.

---

# 8. Event Bus Is Not Telemetry Bus

Logging, Metrics and Tracing are observability transports.

They must not require every diagnostic signal to become a business event.

Preferred:

```text
Business Event
    ├── Event Bus consumers
    └── Diagnostics may observe

Operational Telemetry
    ↓
Logging / Telemetry Infrastructure
```

Do not publish:

```text
LogRecorded
MetricUpdated
TraceStarted
TraceCompleted
```

through the business Event Bus merely to support observability.

---

# 9. Event Bus Is Not UI Event Bus

Native/UI-local events remain outside the business Event Bus by default.

Examples:

```text
ButtonClicked
PointerMoved
ScrollChanged
ViewOpened
DialogResponded
NotificationShown
WindowResized
```

These belong to UI-local mechanisms.

A semantic user action becomes a command/UiIntent through UI Adapter/Application.

---

# 10. Event Definition

An Event is an immutable record of a committed fact.

Conceptually:

```text
Event
├── identity
├── schema identity
├── occurrence time
├── producer
├── correlation metadata
├── causation metadata
├── typed authority references
└── immutable payload
```

---

# 11. Event Categories

CRAI v2 recognizes primarily:

```text
Domain Event
State/Capability Event
Artifact Event
Runtime Fact Event
Integration/System Fact Event
```

These categories remain factual.

There is no `Command Event` category.

---

# 12. Domain Event

Represents a committed domain fact.

Examples:

```text
ReadingSessionCreated
ReadingSessionPaused
ReadingContextChanged
PreferenceChanged
SourceProfileChanged
```

---

# 13. State / Capability Event

Represents an owner-confirmed capability or lifecycle fact.

Examples:

```text
DiagnosticCapabilityChanged
UiCapabilityChanged
ProviderAvailabilityChanged
ApplicationDegraded
```

Only publish when asynchronous consumers actually need the fact.

---

# 14. Artifact Event

Represents a committed Artifact lifecycle fact.

Examples:

```text
RecognitionArtifactPublished
SourceDocumentArtifactPublished
TranslationArtifactPublished
PresentationArtifactPublished
```

Artifact event names must correspond to actual module contracts.

Do not invent generic names if the module's `EVENTS.md` defines another canonical name.

---

# 15. Runtime Fact Event

Runtime may expose selected committed execution facts.

Possible examples:

```text
WorkItemSucceeded
WorkItemFailed
AttemptTimedOut
RuntimeRevisionSuperseded
```

These events are facts.

They are not how Runtime controls itself.

---

# 16. Integration/System Fact Event

Represents external/platform facts that matter asynchronously.

Examples:

```text
NetworkAvailabilityChanged
ScreenCapturePermissionChanged
ResourcePressureDetected
```

Only publish when another component genuinely requires asynchronous awareness.

---

# 17. Event Naming

Preferred event naming:

```text
PascalCase past-tense fact
```

Examples:

```text
ReadingContextChanged
PreferenceChanged
TranslationArtifactPublished
RuntimeRevisionSuperseded
UiCapabilityChanged
```

Module-specific documents remain authoritative for exact names.

---

# 18. No `_REQUESTED` Convention

The v1 pattern:

```text
<THING>_<ACTION>_REQUESTED
```

is removed from Event Bus architecture.

A request is a:

```text
Command
Intent
Use-case request
Runtime request
```

not a fact.

---

# 19. Completed / Failed Naming

Names such as:

```text
RecognitionCompleted
TranslationFailed
```

may be valid only if they describe a stable owner-owned fact.

However, prefer stronger semantic names when possible.

Example:

```text
TranslationArtifactPublished
```

communicates more architecture meaning than:

```text
TranslationCompleted
```

---

# 20. Event Is Not Necessarily Required

A committed fact does not require an Event Bus event unless asynchronous consumers need it.

Example:

```text
Preference successfully read
```

does not require:

```text
PreferenceReadCompleted
```

unless there is a real consumer.

Avoid event proliferation.

---

# 21. Event Envelope

Canonical conceptual envelope:

```text
EventEnvelope<TPayload>
├── eventId
├── eventName
├── eventVersion
├── occurredAt
├── publishedAt
├── sourceModule
├── correlationId?
├── causationId?
├── applicationInstanceId?
├── authorityRefs?
├── payload
└── metadata?
```

---

# 22. `eventId`

Unique identity of one event occurrence.

Used for:

```text
deduplication
diagnostics
delivery tracking
debugging
```

`eventId` is the canonical event identity.

---

# 23. `eventName`

Stable registered event type.

Example:

```text
ReadingContextChanged
```

---

# 24. `eventVersion`

Schema compatibility version.

Conceptually:

```text
1
2
3
```

It does not represent:

```text
ReadingContextRevision
PreferenceRevision
RuntimeRevisionId
ArtifactVersion
```

---

# 25. `occurredAt`

The time the fact became true.

---

# 26. `publishedAt`

The time the event was placed onto Event Bus.

Normally:

```text
occurredAt <= publishedAt
```

They may differ due to:

```text
buffering
transaction/outbox publication
recovery
IPC
delivery retry
```

---

# 27. `sourceModule`

The owner that produced the fact.

Examples:

```text
reading-session
preferences
recognition
translation
runtime
diagnostics
ui-adapter
```

---

# 28. `correlationId`

Groups related work/use-case activity.

A CorrelationId is not event identity.

Multiple events may share one CorrelationId.

---

# 29. `causationId`

Optional identifier of the immediate cause.

It may reference:

```text
commandId
intentId
eventId
WorkItemId
AttemptId
```

depending on the originating contract.

Do not assume causation is always another event.

---

# 30. `applicationInstanceId`

May distinguish application process instances.

Useful for:

```text
crash recovery
IPC
stale process protection
diagnostics
```

Not all events require it.

---

# 31. Typed Authority References

Do not use a fixed legacy set:

```text
sessionId
pipelineId
taskId
contentRevision
```

for every event.

Instead, events carry owner-relevant typed references.

Possible:

```text
sessionId
readingContextRevision

runtimeRevisionId
workItemId
attemptId

artifactId
artifactVersion

preferenceRevision
presentationRevision

frontendId
```

Only fields relevant to that event should exist.

---

# 32. No Universal `pipelineId`

`pipelineId` is not an architecture-wide event requirement in Runtime v2.

Business pipeline topology and Runtime execution identity are distinct concepts.

---

# 33. No Universal `taskId`

Use:

```text
WorkItemId
AttemptId
```

where Runtime identity is required.

Do not keep a generic `taskId` solely for legacy compatibility.

---

# 34. No Universal `contentRevision`

Use the revision actually owned by the source domain.

Examples:

```text
ReadingContextRevision
PreferenceRevision
PresentationRevision
```

Do not treat all changes as one numeric `contentRevision`.

---

# 35. Payload

Payload contains data specific to the fact.

Rules:

```text
immutable
schema-defined
minimal
privacy-safe
serializable where practical
```

---

# 36. Large Payloads

Events should not carry large raw objects such as:

```text
screenshots
large OCR result graphs
full documents
full translation documents
large diagnostic bundles
```

Prefer:

```text
ArtifactRef
ContentRef
BlobRef
SnapshotRef
```

where architecture supports them.

---

# 37. References Must Be Stable Enough

Do not place an unsafe process-only pointer into a public event if the architecture may later cross process boundaries.

Bad:

```text
native image buffer pointer
DOM node
Qt pointer
native window object
```

Preferred:

```text
opaque stable reference
ArtifactId
platform-neutral resource reference
```

---

# 38. Metadata

Metadata may include non-authoritative transport/diagnostic context.

Examples:

```text
trace context
safe debug flags
schema hints
transport metadata
```

Business decisions must not depend on optional metadata.

---

# 39. Privacy

Event payload and metadata MUST NOT expose:

```text
API key
access token
cookie
private key
provider secret
raw screenshot
raw OCR text
full translation text
clipboard content
```

unless explicitly allowed by a narrowly defined internal contract.

Default architecture should prefer references.

---

# 40. Event Registry

CRAI may maintain a registry.

Conceptually:

```text
EventDefinition
├── eventName
├── currentVersion
├── ownerModule
├── payloadSchema
├── deliveryExpectation
├── orderingScope?
├── compatibilityPolicy
└── privacyClassification
```

---

# 41. Registry Consumer List

A static list of consumers may be useful for documentation.

It must not imply the producer owns those consumers.

Consumers may evolve independently.

---

# 42. Event Publication Rule

The canonical publication order is:

```text
validate operation
    ↓
commit authoritative state/result
    ↓
construct immutable event
    ↓
publish
```

---

# 43. Publish-After-Commit

Never publish:

```text
ReadingContextChanged
```

before Reading Session has actually committed the new ReadingContextRevision.

Likewise:

```text
TranslationArtifactPublished
```

must not occur before Artifact publication commits.

---

# 44. Publication Failure

If:

```text
state/result commit succeeds
```

but:

```text
event publication fails
```

the committed state/result remains authoritative.

Do not rollback valid domain state solely because notification failed.

---

# 45. Reliable Publication

If guaranteed notification becomes important, infrastructure may later use:

```text
transactional outbox
durable local queue
journal
IPC retry
```

This is infrastructure policy.

It does not change event semantics.

---

# 46. Event Consumption

A subscriber may:

```text
update a read projection
invalidate non-authoritative cache
refresh UI/application projection
trigger diagnostics
record audit information
schedule follow-up use-case evaluation through an explicit contract
```

A subscriber must not assume receiving an event grants business authority.

---

# 47. Event Handler Responsibilities

A handler should:

```text
validate envelope
validate supported version
check consumer relevance
deduplicate where required
invoke bounded consumer logic
record diagnostics
```

---

# 48. Event Handler Must Remain Bounded

Avoid:

```text
event handler
    ↓
long provider request
    ↓
OCR
    ↓
Translation
    ↓
Presentation
```

Long-running work should be submitted through explicit Application/Runtime contracts.

---

# 49. Event Handler Is Not Runtime Worker

Do not model:

```text
Async Subscriber = OCR Worker
```

as the core architecture.

Runtime workers execute WorkItems/Attempts.

The Event Bus may notify interested consumers of the resulting facts.

---

# 50. Subscriber Failure Isolation

One subscriber failure should not prevent unrelated subscribers from receiving the event where the transport model supports isolation.

Conceptually:

```text
Event
 ├── Consumer A succeeds
 ├── Consumer B fails
 └── Consumer C succeeds
```

Consumer B failure does not invalidate the producer's committed fact.

---

# 51. Subscriber Errors

Subscriber failures are:

```text
consumer-side failures
```

not producer domain failure.

Infrastructure/Diagnostics records them separately.

---

# 52. Delivery Model — MVP

Recommended MVP:

```text
in-process
in-memory
typed Event Bus
```

Advantages:

```text
simple
fast
low resource cost
easy local debugging
appropriate for desktop-first MVP
```

---

# 53. Future Inter-Process Delivery

If CRAI later separates:

```text
UI process
Core process
Capture worker
Recognition worker
```

the logical Event Bus contract may use:

```text
IPC
named pipe
local socket
process messaging
```

Event semantics should remain stable.

---

# 54. Serialization Boundary

Public Event Bus schemas should avoid relying on:

```text
non-serializable framework objects
raw pointers
closures
native controls
SDK instances
```

---

# 55. Persistent Event Store

CRAI Event Bus is not an event-sourcing store by default.

Events do not automatically become permanent application history.

Persistence of domain state remains owner-specific.

---

# 56. Delivery Semantics

Delivery semantics belong to Event Bus infrastructure.

MVP may use:

```text
at-most-once in-process delivery
```

if that matches implementation.

Future transports may provide:

```text
at-least-once
```

for selected events.

Consumers must not assume exactly-once execution.

---

# 57. Exactly-Once Is Not Required

Correctness must not depend on exactly-once delivery.

Use:

```text
eventId
idempotent consumer behavior
authoritative state/revision checks
```

where duplication matters.

---

# 58. Event Identity

Duplicate detection is based primarily on:

```text
eventId
```

for one subscriber.

Do not define duplicate identity as:

```text
sessionId
+ pipelineId
+ eventName
+ taskId
```

because those fields may describe different legitimate facts.

---

# 59. Correlation Is Not Deduplication

Two different events may share:

```text
correlationId
```

They must not be treated as duplicates.

---

# 60. Timestamp Is Not Deduplication

Two events may have identical or near-identical timestamps.

Timestamp is not identity.

---

# 61. Ordering

CRAI does not require global event ordering.

Independent owners may publish concurrently.

---

# 62. Owner-Scoped Ordering

Where an owner exposes monotonically increasing revisions, consumers should prefer those revisions to transport order.

Example:

```text
ReadingContextChanged
revision = 13
```

arriving before:

```text
ReadingContextChanged
revision = 12
```

allows the consumer to reject the older projection update.

---

# 63. Runtime Ordering

Do not infer Runtime execution ordering from event arrival.

Use:

```text
RuntimeRevisionId
WorkItem dependency
Attempt state
Artifact provenance
```

from authoritative Runtime contracts.

---

# 64. No Pipeline Ordering Requirement

v1-style:

```text
OCR_STARTED
OCR_COMPLETED
TRANSLATION_STARTED
```

is not the architecture-wide Event Bus ordering model.

Different WorkItems may overlap or execute concurrently.

---

# 65. Event Sequence Numbers

Owner-specific sequence numbers may exist if required.

Examples:

```text
sessionEventSequence
providerEventSequence
```

They should not be introduced globally without a concrete consumer need.

---

# 66. Stale Event Handling

A stale Event Bus event is generally handled by consumer-side authority checks.

Example:

```text
ReadingContextChanged
revision = 12

consumer already has revision = 13
```

Result:

```text
ignore stale projection update
```

No new global:

```text
PipelineStaleDetected
```

event is required.

---

# 67. Event Staleness vs Candidate Staleness

Do not confuse:

```text
stale event delivery
```

with:

```text
stale Candidate Artifact
```

Candidate Artifact publication safety is enforced before publication by Runtime/owner authority.

Event Bus is not the primary stale-result gate.

---

# 68. Backpressure

Event Bus infrastructure must protect the process from unbounded queues.

Possible policies:

```text
bounded queues
consumer isolation
coalescing
droppable advisory events
transport-specific backpressure
```

---

# 69. Not All Events Are Droppable

Committed important facts should not be arbitrarily discarded if consumer correctness depends on them.

If infrastructure cannot guarantee delivery, consumers should be able to resynchronize from authoritative state where practical.

---

# 70. Snapshot Recovery

Preferred resilience pattern:

```text
event missed
    ↓
consumer detects gap/staleness
    ↓
query authoritative snapshot
    ↓
rebuild projection
```

This is preferable to requiring the Event Bus to be permanent history for every event.

---

# 71. High-Frequency Signals

High-frequency operational signals often should not be business events.

Examples:

```text
OCR progress percentage
translation token stream progress
mouse movement
frame-by-frame screen change
trace span timing
metric sample
```

Use:

```text
local observable stream
Runtime progress channel
UI-local mechanism
Telemetry
```

as appropriate.

---

# 72. Progress Events

A module may define a progress event only when:

1. another architectural consumer genuinely requires it;
2. the event represents useful externally observable progress;
3. frequency is bounded/throttled;
4. dropping intermediate values is safe.

Do not add generic progress events by default.

---

# 73. Content Change Signals

Raw watcher/frame signals may be extremely high-frequency.

These should generally be:

```text
Capture/Watcher-local stream
```

until a stable meaningful fact is committed.

Example:

```text
many frame observations
    ↓
stability/change policy
    ↓
meaningful Source/ReadingContext fact
```

---

# 74. Application Events

Possible application-level facts:

```text
ApplicationReady
ApplicationDegraded
ApplicationStopping
ApplicationStopped
```

Exact naming should match the Application architecture.

Commands such as:

```text
ApplicationShutdownRequested
```

belong to Application command interfaces, not Event Bus taxonomy.

---

# 75. Reading Session Events

Canonical Reading Session events are defined by:

```text
doc/02-modules/reading-session/EVENTS.md
```

This architecture file must not duplicate or override that file.

Typical facts may include:

```text
ReadingSessionCreated
ReadingSessionPaused
ReadingSessionResumed
ReadingSessionStopped
ReadingContextChanged
```

Use the exact module-defined names.

---

# 76. Capture Events

Capture event authority belongs to:

```text
doc/02-modules/capture/EVENTS.md
```

Avoid legacy names such as:

```text
CONTENT_CAPTURE_REQUESTED
CAPTURE_RETRY_REQUESTED
```

in the architecture-level registry.

---

# 77. Recognition Events

Recognition event authority belongs to:

```text
doc/02-modules/recognition/EVENTS.md
```

Avoid architecture-level assumptions such as:

```text
OCR_COMPLETED
    ↓
starts segmentation
```

Recognition events describe Recognition-owned facts only.

---

# 78. Text Processing Events

Text Processing event authority belongs to:

```text
doc/02-modules/text-processing/EVENTS.md
```

Do not use generic segmentation events to command Translation.

---

# 79. Translation Events

Translation event authority belongs to:

```text
doc/02-modules/translation/EVENTS.md
```

Events may describe:

```text
artifact publication
capability change
other stable Translation-owned facts
```

They do not own Runtime retry.

---

# 80. Presentation Events

Presentation event authority belongs to:

```text
doc/02-modules/presentation/EVENTS.md
```

Presentation events must not be confused with native UI rendering events.

---

# 81. Preferences Events

Preferences owns:

```text
PreferenceChanged
```

or the exact module-defined equivalent.

There is no architecture-wide global:

```text
EffectivePreferencesChanged
```

authority.

Effective state is contextual.

---

# 82. Diagnostics Events

Diagnostics publishes only selected Diagnostics-owned state/capability facts.

Observations such as:

```text
LogRecorded
MetricUpdated
TraceStarted
```

remain observability transport concerns rather than business Event Bus events.

---

# 83. UI Adapter Events

UI-local events remain local by default.

Only selected adapter capability/lifecycle facts should enter global Event Bus when Application has a genuine asynchronous dependency.

---

# 84. Runtime Events

Runtime may publish selected facts, but Runtime does not control execution by consuming its own command-like Event Bus messages.

Preferred execution path:

```text
Runtime API
    ↓
Runtime state/work graph
```

Event Bus:

```text
Runtime committed fact
    ↓
interested consumers
```

---

# 85. Retry

Retry is not requested by Event Bus.

Correct:

```text
Attempt failed
    ↓
Runtime retry policy
    ↓
new Attempt
```

Optional fact:

```text
RetryExhausted
```

may be published if another consumer genuinely needs it.

---

# 86. Cancellation

Cancellation commands go through explicit Runtime/Application contracts.

Potential resulting facts may include:

```text
WorkItemCancelled
AttemptCancelled
RuntimeRevisionSuperseded
```

according to Runtime definitions.

Do not use:

```text
PIPELINE_CANCEL_REQUESTED
TASK_CANCEL_REQUESTED
```

as Event Bus control messages.

---

# 87. Provider Fallback

Provider fallback is policy/execution behavior.

It is not commanded by:

```text
OCR_FALLBACK_REQUESTED
TRANSLATION_FALLBACK_REQUESTED
```

events.

A provider availability change may be a factual event.

---

# 88. Cache Interaction

Cache reads/writes should normally use explicit cache interfaces.

Avoid turning:

```text
CacheLookupRequested
CacheWriteRequested
```

into business Event Bus messages.

Cache invalidation facts may be events where multiple consumers require them.

---

# 89. UI Interaction

Correct:

```text
User
    ↓
UI Adapter
    ↓
UiIntent
    ↓
Application Command
```

Incorrect:

```text
User
    ↓
SESSION_START_REQUESTED Event
```

as the primary command route.

---

# 90. Commands vs Events

Summary:

| Concept   | Direction                | Authority                       |
| --------- | ------------------------ | ------------------------------- |
| Command   | caller → owner           | asks owner to act               |
| Query     | caller → owner           | asks owner for information      |
| UiIntent  | UI → Application         | semantic user intention         |
| Event     | owner → 0..N consumers   | reports committed fact          |
| Telemetry | producer → observability | reports operational measurement |

---

# 91. Command Outcome

A command may return:

```text
accepted result
rejection
error
new snapshot
async operation reference
```

It does not need a matching `*_COMPLETED` Event Bus event unless asynchronous consumers need that fact.

---

# 92. Request/Result Pair Is Not Mandatory

v1-style:

```text
OCR_PROCESS_REQUESTED
    ↓
OCR_COMPLETED
```

is removed as a universal pattern.

Runtime/contract invocation already provides request/execution correlation through:

```text
WorkItemId
AttemptId
CorrelationId
```

---

# 93. Correlation Example

```text
StartReadingIntent
correlationId = C1

ReadingContextChanged
correlationId = C1

Runtime work
correlationId = C1

PresentationArtifactPublished
correlationId = C1
```

Different events remain distinct via EventId.

---

# 94. Causation Example

```text
Command:
ChangeReadingContext
commandId = CMD-1

Event:
ReadingContextChanged
causationId = CMD-1
```

Causation does not require another event.

---

# 95. Event Versioning

Compatible additions may include:

```text
optional field
optional metadata
new enum value with explicit unknown/fallback handling
```

Breaking changes require a new event schema version.

---

# 96. Breaking Changes

Examples:

```text
remove required field
change field type
change event semantic meaning
change ownership
change required authority reference
```

These require version/contract migration.

---

# 97. Event Semantic Stability

Do not reuse an existing event name for a different fact.

Example:

If:

```text
RecognitionArtifactPublished
```

means an accepted Artifact became authoritative, it must never later mean merely “provider call finished.”

---

# 98. Unknown Event Version

Consumer behavior should be explicit:

```text
reject
ignore safely
fallback to supported version
request resynchronization
```

depending on compatibility contract.

---

# 99. Subscriber Version Independence

Producer and consumer deployment/version evolution must not silently reinterpret payload semantics.

---

# 100. Event Validation

Before dispatch, infrastructure may validate:

```text
EventId present
eventName registered
eventVersion supported
occurredAt valid
sourceModule valid
payload schema valid
required authority references present
```

---

# 101. Semantic Validation

Event Bus validates transport/schema-level requirements.

It should not duplicate full domain validation.

Domain semantic validity was established before publication by the owner.

---

# 102. Event Middleware

Allowed infrastructure concerns may include:

```text
schema validation
trace-context propagation
dispatch metrics
deduplication support
subscriber isolation
serialization
privacy/redaction guard
```

---

# 103. Event Middleware Must Not Own Business Policy

Do not put into Event Bus middleware:

```text
retry business work
select provider
decide downstream stage
mutate Reading Session
publish Artifacts
change Preferences
```

---

# 104. Logging

Event Bus infrastructure may log safe dispatch metadata:

```text
eventId
eventName
eventVersion
sourceModule
correlationId
occurredAt
publishedAt
dispatch duration
subscriber identity
delivery outcome
```

---

# 105. Typed References in Logs

Where useful:

```text
sessionId
runtimeRevisionId
workItemId
attemptId
artifactId
```

may be logged.

Avoid unnecessary high-cardinality metrics.

---

# 106. Logging Privacy

Do not log event payload content merely because it passed through Event Bus.

Especially avoid:

```text
recognized text
translated text
screen image
clipboard content
credentials
```

---

# 107. Metrics

Event Bus infrastructure may expose operational metrics such as:

```text
event_bus_published_total
event_bus_dispatched_total
event_bus_invalid_total
event_bus_duplicate_total
event_bus_handler_failed_total
event_bus_dispatch_duration
event_bus_queue_depth
event_bus_queue_delay
```

These are Telemetry metrics.

They are not Event Bus events.

---

# 108. Metric Labels

Use low-cardinality labels such as:

```text
eventName
sourceModule
consumerModule
deliveryOutcome
```

Avoid:

```text
SessionId
ArtifactId
WorkItemId
AttemptId
full source URL
```

as metric labels.

---

# 109. Tracing

Event Bus may propagate trace context.

Conceptually:

```text
correlationId
traceparent?
tracestate?
```

Actual trace transport belongs to Telemetry infrastructure.

---

# 110. Event Bus Failure

Event Bus infrastructure failure should be handled according to event criticality and delivery model.

It must not redefine producer state.

---

# 111. Consumer Resynchronization

Consumers should prefer current authoritative snapshots when event history is incomplete.

Example:

```text
Settings UI missed PreferenceChanged
    ↓
GetPreferencesSnapshot
    ↓
rebuild ViewModel
```

---

# 112. No Event Replay Assumption

Unless explicitly implemented later, consumers must not assume the Event Bus can replay all historical events.

---

# 113. Event Bus and Persistence

Persistence belongs to owning modules/Storage.

Event Bus does not automatically save all events.

If an outbox is introduced, it is delivery infrastructure rather than domain history.

---

# 114. Event Bus and Artifact Store

An Artifact reference in an event does not make Event Bus responsible for Artifact lifecycle.

Artifact ownership remains with the producing architecture/module.

---

# 115. Event Bus and Cache

The Event Bus may notify cache invalidation if appropriate.

Cache ownership, keying and eviction remain in Cache infrastructure/policy.

---

# 116. Event Bus and Resource Lifecycle

Event delivery must not extend large resource lifetime indefinitely.

References used in events should have explicit lifetime semantics when necessary.

---

# 117. Application Shutdown

Shutdown is invoked through Application command/lifecycle control.

Possible fact after commit:

```text
ApplicationStopping
```

may be published to consumers that need asynchronous cleanup awareness.

---

# 118. Session Stop

Session stop is invoked through Reading Session/Application contract.

Possible resulting fact:

```text
ReadingSessionStopped
```

is an event.

Do not send:

```text
SESSION_STOP_REQUESTED
```

through Event Bus as the primary stop mechanism.

---

# 119. Runtime Supersession

When a new RuntimeRevision supersedes an old one:

```text
Runtime commits supersession
    ↓
optional RuntimeRevisionSuperseded event
```

Consumers may update projections.

Execution cancellation remains Runtime-owned.

---

# 120. Artifact Publication Example

```text
Recognition Attempt completes
    ↓
Candidate RecognitionArtifact
    ↓
authority validation
    ↓
Published RecognitionArtifact
    ↓
RecognitionArtifactPublished
    ↓
Event Bus
```

The event occurs last.

---

# 121. Downstream Execution Example

After Recognition Artifact publication:

```text
Runtime dependency graph
```

already determines whether Text Processing WorkItem becomes READY.

The event does not command Text Processing.

---

# 122. Preference Example

```text
SetPreference command
    ↓
Preferences validates
    ↓
commit PreferenceRevision N+1
    ↓
PreferenceChanged
    ↓
Event Bus
```

Possible consumers:

```text
Application projection
UI settings projection
cache invalidation
diagnostics
```

---

# 123. Reading Context Example

```text
ChangeReadingContext command
    ↓
Reading Session commits revision 18
    ↓
ReadingContextChanged
```

Business Pipeline/Application may separately evaluate new execution requirements through explicit orchestration.

---

# 124. Diagnostics Example

Diagnostics may observe:

```text
ReadingContextChanged
```

to correlate health data.

It does not need:

```text
LogRecorded
MetricUpdated
TraceCompleted
```

events from Event Bus.

---

# 125. UI Example

```text
PreferenceChanged
    ↓
Application/settings projection
    ↓
UI Adapter builds new ViewModel
```

UI Adapter does not need to subscribe to every internal Runtime/module event.

---

# 126. Common Architecture Mistake — Command Event

Wrong:

```text
TranslationRequested
    ↓
Event Bus
    ↓
Translation starts
```

Correct:

```text
Application/Runtime
    ↓
Translation WorkItem / module contract
```

---

# 127. Common Architecture Mistake — Event-Driven Stage Chain

Wrong:

```text
RecognitionCompleted
    ↓
TextProcessingCompleted
    ↓
TranslationCompleted
    ↓
PresentationCompleted
```

as execution control.

Correct:

```text
BusinessExecutionPlan
    ↓
Runtime dependency graph
```

---

# 128. Common Architecture Mistake — Event Bus Retry

Wrong:

```text
TranslationFailed
    ↓
RetryRequested Event
```

Correct:

```text
Attempt Failed
    ↓
Runtime Retry Policy
```

---

# 129. Common Architecture Mistake — Event Bus Cancellation

Wrong:

```text
PipelineCancelRequested
    ↓
all stages subscribe
```

Correct:

```text
Application/Reading Session command
    ↓
Runtime cancellation authority
```

---

# 130. Common Architecture Mistake — Event Bus Telemetry

Wrong:

```text
MetricUpdated
TraceCompleted
LogRecorded
```

as business events.

Correct:

```text
Telemetry / Logging infrastructure
```

---

# 131. Common Architecture Mistake — UI Event Bus

Wrong:

```text
ButtonClicked
DialogOpened
WindowResized
```

on global Event Bus.

Correct:

```text
UI-local mechanism
```

---

# 132. Common Architecture Mistake — Generic `StateChanged`

Avoid:

```text
StateChanged
```

without owner/domain semantics.

Prefer:

```text
ReadingContextChanged
UiCapabilityChanged
DiagnosticCapabilityChanged
```

where a real event is required.

---

# 133. Common Architecture Mistake — Every Result Is Event

Do not publish:

```text
ReadPreferenceCompleted
CacheLookupCompleted
ValidationCompleted
```

simply because a function returned.

Use direct contract results unless asynchronous consumers need a fact.

---

# 134. Common Architecture Mistake — Event as Transaction Authority

Event Bus delivery success must not determine whether owner state was committed.

State authority precedes publication.

---

# 135. Architecture Invariants

1. Events describe committed facts.

2. Event Bus contains no command-event category.

3. `*_REQUESTED` is not the standard event pattern.

4. Commands use explicit Application/module/Runtime contracts.

5. UiIntent is not a business Event Bus event.

6. Event Bus does not orchestrate stage execution.

7. Event Bus does not create WorkItems.

8. Event Bus does not create Attempts.

9. Event Bus does not own Runtime retry.

10. Event Bus does not own cancellation authority.

11. Event Bus does not select providers.

12. Event Bus does not publish Artifacts.

13. Event Bus does not own state transitions.

14. Owner state commits before event publication.

15. Publication failure does not rollback committed owner state.

16. Event Bus is not telemetry transport.

17. Event Bus is not logging transport.

18. Event Bus is not tracing transport.

19. Event Bus is not UI-local event transport.

20. EventId is event identity.

21. CorrelationId is not deduplication identity.

22. Timestamp is not deduplication identity.

23. There is no universal pipelineId requirement.

24. There is no universal taskId requirement.

25. There is no universal contentRevision requirement.

26. Typed authority references are preferred.

27. EventVersion is schema version only.

28. Large payloads use references where practical.

29. Public events contain no native/platform objects.

30. Event payloads remain privacy-safe.

31. Subscribers do not gain ownership from event reception.

32. Subscriber failure does not invalidate producer state.

33. Global event ordering is not required.

34. Runtime ordering is not inferred from Event Bus order.

35. Stale-result validation occurs before Artifact publication.

36. Event Bus is not the primary stale-result authority.

37. Consumers may recover from snapshots.

38. Event Bus need not be event-sourced.

39. Delivery semantics belong to infrastructure.

40. Exactly-once delivery is not required.

41. Consumer idempotency is required where duplicate delivery matters.

42. Module `EVENTS.md` documents remain authoritative for module-specific facts.

---

# 136. MVP Event Bus

Recommended MVP:

```text
typed
in-process
in-memory
bounded
subscriber-isolated
schema-validated
instrumented
```

No external broker required.

---

# 137. Minimal MVP Event Set

Do not define a huge global event catalog in advance.

Implement only facts with real consumers.

Likely early candidates may include:

```text
ApplicationReady
ReadingSessionCreated
ReadingSessionStopped
ReadingContextChanged
PreferenceChanged
selected ArtifactPublished facts
selected capability changes
```

Exact names must come from module-specific `EVENTS.md`.

---

# 138. MVP Exclusions

Do not introduce in MVP unless demonstrated necessary:

```text
Command Events
PipelineRequested events
OCRRequested events
TranslationRequested events
RenderRequested events
RetryRequested events
FallbackRequested events
CancelRequested events
per-token progress events
per-frame events
global UI lifecycle events
generic telemetry events
```

---

# 139. Testing — Fact Semantics

Verify every registered Event Bus event represents a fact already committed by its owner.

---

# 140. Testing — No Command Events

Search registry for:

```text
Requested
Request
CommandEvent
```

and verify such entries are absent from business Event Bus taxonomy unless explicitly documented as an exception.

---

# 141. Testing — Publish After Commit

Inject publication failure.

Verify owner state/result remains committed.

---

# 142. Testing — Deduplication

Deliver the same EventId twice to one consumer.

Verify required idempotent handling.

---

# 143. Testing — Correlation

Deliver two different EventIds with the same CorrelationId.

Verify they are treated as separate events.

---

# 144. Testing — Ordering

Deliver owner revisions out of transport order.

Verify stale consumer projection does not overwrite newer state.

---

# 145. Testing — Subscriber Isolation

Fail one subscriber.

Verify unrelated consumers still receive/process the event where transport guarantees allow it.

---

# 146. Testing — Privacy

Attempt to publish payload containing:

```text
secret
raw screenshot
raw OCR text
raw translation text
native UI object
```

Verify rejection/redaction according to policy.

---

# 147. Testing — Runtime Isolation

Verify no Event Bus subscriber directly creates:

```text
WorkItem
Attempt
Runtime retry
Runtime cancellation transition
```

outside explicit Runtime contracts.

---

# 148. Testing — UI Isolation

Verify UI-local events do not enter global Event Bus by default.

---

# 149. Testing — Telemetry Isolation

Verify:

```text
logs
metrics
traces
```

use observability infrastructure rather than business Event Bus events.

---

# 150. Testing — Snapshot Recovery

Drop one non-durable event.

Verify an affected projection can resynchronize from authoritative owner state where required.

---

# 151. Related Documents

```text
doc/01-architecture/core/
├── EVENT_BUS.md
├── EVENT_CONVENTION.md
├── STATE_MACHINE.md
├── DATA_FLOW.md
├── CAPABILITY_MAP.md
└── README.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── CANCELLATION.md
├── RETRY_POLICY.md
├── WORK_QUEUE.md
├── SCHEDULER.md
└── RUNTIME_OBSERVABILITY.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── MODULE_DEPENDENCY.md
└── OWNERSHIP_MAP.md

doc/02-modules/
├── reading-session/
├── capture/
├── recognition/
├── text-processing/
├── translation/
├── presentation/
├── preferences/
├── diagnostics/
└── ui-adapter/

doc/03-infrastructure/
├── event-bus/
├── logging/
└── telemetry/
```

---

# 152. Documentation Authority

This file defines:

```text
architecture-wide Event Bus role
fact-only event semantics
command/event separation
Event envelope
delivery principles
ordering principles
deduplication principles
publication semantics
privacy constraints
Runtime/UI/Telemetry boundaries
```

It does not define the complete event catalog of every module.

---

# 153. Module Event Authority

Module-specific event definitions belong to:

```text
02-modules/<module>/EVENTS.md
```

Architecture documents may show examples but must not contradict those files.

---

# 154. Infrastructure Authority

Implementation details such as:

```text
queue structure
subscription API
dispatch algorithm
backpressure implementation
handler timeout
serialization
IPC transport
```

belong to:

```text
03-infrastructure/event-bus/
```

---

# 155. Completion Criteria

This document is synchronized when:

* Event Bus is fact-only;
* command-event category is removed;
* `_REQUESTED` event convention is removed;
* Runtime execution no longer depends on event-driven stage chaining;
* retry is not Event Bus-owned;
* cancellation is not Event Bus-owned;
* provider fallback is not Event Bus-owned;
* Artifact publication occurs before publication events;
* event envelope uses typed authority references;
* `pipelineId/taskId/contentRevision` are not universal required fields;
* Event Bus is separated from Telemetry/Logging;
* Event Bus is separated from UI-local events;
* consumer failure does not invalidate producer state;
* EventId is deduplication identity;
* correlation/causation semantics are explicit;
* delivery/order semantics do not recreate pipeline authority;
* module-specific EVENTS documents remain authoritative.

---

# 156. Summary

CRAI v1 effectively allowed:

```text
Command Event
    ↓
Event Bus
    ↓
Pipeline Orchestrator
    ↓
Requested Stage Event
    ↓
Module
```

Runtime v2 uses:

```text
Command / UiIntent
    ↓
Application / Module Contract
    ↓
Business Execution Planning
    ↓
Runtime
    ↓
WorkItems / Attempts
    ↓
Committed Result / State
    ↓
Event
    ↓
Event Bus
    ↓
Interested Consumers
```

The central invariant is:

```text
Commands ask.

Runtime executes.

Owners commit.

Events report.

The Event Bus
never becomes the authority
for the other three.
```
