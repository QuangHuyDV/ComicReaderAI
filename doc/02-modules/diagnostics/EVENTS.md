# Diagnostics Events

> **Project:** CRAI
> **Module:** `diagnostics`
> **Path:** `doc/02-modules/diagnostics/EVENTS.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the Event Bus boundary of the Diagnostics module.

Diagnostics events describe committed Diagnostics-owned facts such as:

```text
diagnostic capability degradation
diagnostic capability recovery
aggregate diagnostic health change
collection degradation/recovery
explicit diagnostic bundle export completion
```

Diagnostics events do not carry ordinary telemetry records.

The following are not Event Bus concerns:

```text
individual log records
metric observations
trace spans
trace completion
ordinary error observations
performance observations
profiling samples
```

Those use diagnostic/logging/telemetry contracts.

---

# 2. Core Event Principle

An Event Bus event represents:

> A stable cross-module fact that has already become true.

It does not represent:

> Every diagnostic observation that occurred.

Correct:

```text
DiagnosticCollectionDegraded
DiagnosticCollectionRecovered
DiagnosticCapabilityChanged
DiagnosticHealthChanged
DiagnosticExportCompleted
```

Incorrect:

```text
LogRecorded
MetricUpdated
TraceStarted
TraceCompleted
ErrorReported
```

as general business Event Bus messages.

---

# 3. Event Bus Is Not Telemetry Transport

Invalid architecture:

```text
Recognition finishes
    ↓
RecognitionCompleted event
    ↓
Diagnostics subscriber
    ↓
MetricUpdated event
    ↓
metrics consumer
```

Preferred architecture:

```text
Recognition operation
    ↓
ObserveMetric / tracing abstraction
    ↓
Diagnostics semantics
    ↓
Telemetry infrastructure
```

The Event Bus is not required.

---

# 4. Event Ownership

Diagnostics publishes only Diagnostics-owned state facts.

Examples:

```text
DiagnosticCollectionDegraded
    → Diagnostics

DiagnosticCapabilityChanged
    → Diagnostics

CaptureHealthChanged
    → Capture

AttemptCancelled
    → Runtime

PreferenceChanged
    → Preferences
```

Diagnostics must not republish another module's fact under a generic Diagnostics event name.

---

# 5. Event Categories

Recommended Diagnostics v2 event groups:

```text
Diagnostics Events
├── Capability Events
├── Collection Events
├── Aggregate Health Events
└── Explicit Export Events
```

All are optional unless a real cross-module consumer exists.

---

# 6. Canonical Event Envelope

Diagnostics events follow CRAI Event Convention.

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
├── payload
└── metadata?
```

Canonical architecture remains authoritative for exact field naming.

Diagnostics must not define a competing Event Bus envelope.

---

# 7. Producer

Diagnostics-owned facts use:

```text
producer = diagnostics
```

---

# 8. Correlation

Diagnostics events may carry:

```text
correlationId
traceId
moduleOperationId
```

where relevant.

These values support correlation only.

They do not transfer authority from their owning domain.

---

# 9. Removed Telemetry Events

The following v1 public events are removed:

```text
LogRecorded
MetricUpdated
TraceStarted
TraceCompleted
ErrorReported
DiagnosticsFlushed
```

Reason:

they represent telemetry transport/operation details rather than stable cross-module domain facts.

---

# 10. Why `LogRecorded` Is Removed

A structured log entry belongs to:

```text
logging / diagnostic observation path
```

not:

```text
business Event Bus
```

Publishing every log entry as an event would:

* duplicate logging infrastructure;
* increase Event Bus load;
* increase coupling;
* increase privacy exposure;
* require consumers to understand logging transport details.

---

# 11. Why `MetricUpdated` Is Removed

Metrics are high-frequency telemetry observations.

They may be:

```text
aggregated
sampled
batched
dropped under pressure
```

These semantics are incompatible with treating each update as a durable business fact.

---

# 12. Why Trace Events Are Removed

`TraceStarted` and `TraceCompleted` belong to tracing instrumentation.

Trace lifecycle is maintained by tracing infrastructure.

Business Event Bus consumers should not depend on span lifecycle.

---

# 13. Why `ErrorReported` Is Removed

Errors remain owned by their originating module.

Example:

```text
CAP-ACQ-003 ProviderTimeout
```

Diagnostics may observe it.

It must not create a second fact:

```text
ErrorReported
```

that becomes the authoritative error identity.

---

# 14. Why `DiagnosticsFlushed` Is Removed

Flush is:

```text
logging / telemetry infrastructure lifecycle coordination
```

typically during shutdown or tests.

It is not normally a cross-module domain fact.

---

# 15. DiagnosticCapabilityChanged

## Meaning

A Diagnostics capability changed operational state.

Examples:

```text
AVAILABLE → DEGRADED
DEGRADED → UNAVAILABLE
UNAVAILABLE → AVAILABLE
DISABLED → AVAILABLE
```

## Payload

```text
DiagnosticCapabilityChangedPayload
├── capability
├── previousState
├── state
├── reasonCode?
├── importance?
└── changedAt
```

---

# 16. Capability Examples

Possible capability identifiers:

```text
Logging
Metrics
Tracing
HealthAggregation
Profiling
RecentRecordQuery
SupportBundleExport
RemoteTelemetry
```

These are diagnostic capabilities, not business modules.

---

# 17. Capability Event Semantics

Example:

```text
Tracing
AVAILABLE → UNAVAILABLE
    ↓
DiagnosticCapabilityChanged
```

This does not imply:

```text
Runtime failure
Recognition failure
Application failure
```

---

# 18. DiagnosticCollectionDegraded

## Meaning

Diagnostics collection quality has materially degraded.

Possible causes:

```text
buffer pressure
significant diagnostic dropping
sampling increased materially
multiple exporters unavailable
bounded retention exhausted
```

## Payload

```text
DiagnosticCollectionDegradedPayload
├── previousStatus
├── collectionStatus
├── reasonCode
├── affectedCapabilities[]
├── dropPolicySummary?
└── changedAt
```

---

# 19. DiagnosticCollectionRecovered

## Meaning

Diagnostics collection recovered from a previously degraded state.

## Payload

```text
DiagnosticCollectionRecoveredPayload
├── previousStatus
├── collectionStatus
├── recoveredCapabilities[]
└── changedAt
```

Typical target:

```text
PRESSURED / DEGRADED
    ↓
NORMAL
```

---

# 20. Collection Events Are State Facts

Do not emit:

```text
DiagnosticCollectionDegraded
```

for every single dropped Debug log.

Emit only after collection status itself changes materially.

---

# 21. DiagnosticHealthChanged

## Meaning

The Diagnostics module's aggregate diagnostic health changed.

This refers to:

```text
Diagnostics capability/collection health
```

not the health of every observed business component.

## Payload

```text
DiagnosticHealthChangedPayload
├── previousModuleState?
├── moduleState?
├── previousOverallStatus
├── overallStatus
├── activeDegradations[]
└── changedAt
```

---

# 22. Owner Health Remains Separate

Suppose Capture publishes:

```text
CaptureHealthChanged
```

Diagnostics may update its aggregate view.

It must not republish that as though:

```text
DiagnosticHealthChanged
```

means Capture changed.

`DiagnosticHealthChanged` refers only to Diagnostics-owned aggregate/service health.

---

# 23. Component Health Aggregation

Diagnostics may expose current:

```text
DiagnosticHealthSnapshot
```

through queries.

A query result does not require a new Event Bus event every time one observed component changes.

---

# 24. DiagnosticExportCompleted

## Meaning

An explicit support/debug diagnostic bundle export completed successfully.

## Payload

```text
DiagnosticExportCompletedPayload
├── exportOperationId
├── purpose
├── status
├── bundleRef?
├── includedSections[]
├── truncated
├── redactionSummary
└── completedAt
```

---

# 25. Export Event Is Optional

`DiagnosticExportCompleted` should exist only if another component needs asynchronous awareness.

If export is invoked synchronously/request-response:

```text
ExportDiagnosticBundleResult
```

may be sufficient.

Do not add an Event Bus event solely for completeness.

---

# 26. Export Failure Event

A public:

```text
DiagnosticExportFailed
```

is not required for MVP.

Failures return through:

```text
ExportDiagnosticBundleResult
+
DiagnosticError
```

and diagnostics/metrics.

Add a failure event only if a real cross-module consumer requires it.

---

# 27. Removed Generic `HealthStatusChanged`

The v1 event:

```text
HealthStatusChanged
```

is too ambiguous.

It does not identify whether the health owner is:

```text
Diagnostics
Capture
Storage
Runtime
Translation provider
```

Use owner-specific events.

For Diagnostics:

```text
DiagnosticHealthChanged
```

---

# 28. Published Event Set — MVP

Recommended minimal set:

```text
DiagnosticCapabilityChanged
DiagnosticCollectionDegraded
DiagnosticCollectionRecovered
DiagnosticHealthChanged
```

Optional:

```text
DiagnosticExportCompleted
```

---

# 29. No Mandatory Consumed Business Events

Diagnostics v2 requires no direct Event Bus subscriptions for correctness.

The v1 consumed events are removed:

```text
ErrorOccurred
ReadingSessionChanged
TranslationCompleted
RecognitionCompleted
PresentationUpdated
StorageReady
StorageFailed
ApplicationStarted
ApplicationShutdownRequested
```

---

# 30. Why `ErrorOccurred` Subscription Is Removed

Errors should be observed directly through:

```text
ObserveError
```

or equivalent instrumentation.

Using an Event Bus subscription requires every error owner to publish another event solely for Diagnostics.

That creates duplicate error transport.

---

# 31. Why Processing Completion Subscriptions Are Removed

The v1 model used:

```text
TranslationCompleted
RecognitionCompleted
PresentationUpdated
```

to infer performance metrics.

Preferred:

```text
owning module measures its own operation
    ↓
ObserveMetric / Trace
```

The owner has the most accurate semantic timing.

---

# 32. Why Storage Events Are Removed

Storage owns:

```text
StorageHealth
StorageError
```

Diagnostics may observe health through a health abstraction or query/projection.

It does not need hard dependency on:

```text
StorageReady
StorageFailed
```

business events.

---

# 33. Why Application Startup/Shutdown Events Are Removed

Diagnostics initialization and shutdown should be driven by explicit application lifecycle/composition-root control.

Invalid:

```text
ApplicationStarted event
    ↓
Diagnostics initialize
```

as correctness dependency.

Preferred:

```text
Application lifecycle
    ↓
Diagnostics.Initialize()
```

---

# 34. Shutdown Flush

Similarly:

```text
ApplicationShutdownRequested
    ↓
DiagnosticsFlushed event
```

is not required.

Shutdown lifecycle directly coordinates bounded flush with logging/telemetry infrastructure.

---

# 35. Optional Event Observation

Diagnostics may subscribe to selected events for secondary analytics only when:

1. event semantics already exist for business reasons;
2. Diagnostics is not the reason the event exists;
3. telemetry is not duplicated;
4. privacy remains safe;
5. correctness does not depend on the subscription.

---

# 36. Events Are Facts, Not Instrumentation Commands

Invalid:

```text
RecordMetricRequested
StartTraceRequested
FlushDiagnosticsRequested
```

through Event Bus.

Preferred:

```text
ObserveMetric
trace instrumentation abstraction
explicit lifecycle flush
```

---

# 37. Event Ordering

Diagnostics guarantees ordering only for Diagnostics-owned state transitions where meaningful.

Example:

```text
DiagnosticCollectionDegraded
    ↓
DiagnosticCollectionRecovered
```

for one collection-state sequence.

Do not assume total ordering across all telemetry or business events.

---

# 38. Removed Trace Event Ordering

The v1 ordering:

```text
TraceStarted
    ↓
MetricUpdated
    ↓
TraceCompleted
```

is removed from Event Bus semantics.

Tracing infrastructure owns span ordering.

---

# 39. Removed Error Event Ordering

The v1 sequence:

```text
ErrorOccurred
    ↓
ErrorReported
    ↓
LogRecorded
```

is removed.

An error may be:

```text
returned to Runtime
logged
traced
counted
```

through separate diagnostic channels.

No Event Bus chain is required.

---

# 40. Capability Ordering

For one capability:

```text
AVAILABLE
    ↓
DEGRADED
    ↓
UNAVAILABLE
```

events should reflect committed transitions in order when published.

Consumers should still tolerate duplicate/out-of-order delivery according to canonical Event Bus rules.

---

# 41. Collection Ordering

Example:

```text
NORMAL
    ↓
DEGRADED
    ↓
NORMAL
```

may publish:

```text
DiagnosticCollectionDegraded
    ↓
DiagnosticCollectionRecovered
```

---

# 42. No Global Event Ordering

Diagnostics does not guarantee ordering across:

```text
DiagnosticCapabilityChanged
Runtime events
Capture events
Preference events
Reading Session events
```

Use owner-specific state/version/correlation semantics.

---

# 43. Event Idempotency

Each event carries:

```text
EventId
```

Consumers must tolerate duplicate delivery according to canonical Event Bus semantics.

Duplicate event processing must not create duplicate logical state transitions.

---

# 44. CorrelationId Is Not Deduplication Identity

The v1 suggested deduplication using:

```text
EventId
CorrelationId
Timestamp
```

Only:

```text
EventId
```

should be treated as the event identity.

Multiple legitimate events may share the same CorrelationId.

Timestamps are not safe idempotency identities.

---

# 45. Event Delivery Semantics

Diagnostics does not hard-code:

```text
At-least-once
ordered within trace
```

at module level.

Delivery guarantees belong to:

```text
EVENT_BUS.md
configured Event Bus profile
```

---

# 46. Event Immutability

Once published, a Diagnostics event is immutable.

Correction occurs through a later fact/state transition, not mutation of an existing event.

---

# 47. Event Publication Timing

Correct:

```text
Diagnostics-owned state transition
    ↓
state committed
    ↓
publish event
```

Incorrect:

```text
publish event
    ↓
attempt state change
```

---

# 48. Event Publication Failure

Example:

```text
Tracing capability
AVAILABLE → UNAVAILABLE
    ↓
state committed
    ↓
DiagnosticCapabilityChanged publication fails
```

The capability remains:

```text
UNAVAILABLE
```

Do not revert state.

---

# 49. Publication Recovery

Infrastructure may use:

```text
outbox
retry publication
state-query reconciliation
```

Diagnostics does not repeat the original capability transition.

---

# 50. Event Payload Size

Diagnostics events must remain small.

Do not include:

```text
log collections
trace bodies
metric histories
diagnostic bundle bytes
raw stack dumps
raw screenshots
OCR text
translation text
```

---

# 51. BundleRef in Event

If `DiagnosticExportCompleted` includes:

```text
bundleRef
```

it must remain:

```text
opaque
serializable
backend-independent
```

No file handles or vendor SDK objects.

---

# 52. Privacy Rules

Diagnostics events must never contain:

```text
credentials
tokens
passwords
cookies
raw reading content
raw screenshots
OCR text
translation text
provider prompt/response
private certificate
```

---

# 53. Safe Event Metadata

Preferred:

```text
capability name
state
reason code
severity
counts
durations
bounded enum-like values
opaque identifiers
```

---

# 54. Diagnostic Export Privacy

`DiagnosticExportCompleted` may include:

```text
redactionSummary
truncationSummary
includedSections
```

but not the diagnostic bundle content itself.

---

# 55. Events vs Queries

Events answer:

> What Diagnostics-owned state changed?

Queries answer:

> What is Diagnostics state now?

Typical queries:

```text
GetDiagnosticHealth
GetDiagnosticCapabilities
GetDiagnosticSnapshot
GetRecentDiagnosticIssues
```

Consumers should query current state rather than reconstructing it exclusively from event history.

---

# 56. Events vs Logs

Logs answer:

> What operational detail was recorded?

They are not Event Bus domain facts.

---

# 57. Events vs Metrics

Metrics answer:

> What measurable operational value changed or was observed?

They are telemetry streams, not domain events.

---

# 58. Events vs Traces

Traces answer:

> How were correlated operations related in time?

They are telemetry structures, not Event Bus state facts.

---

# 59. Events vs Errors

Errors answer:

> Why did an operation fail or reject?

They are returned by their owning module.

Diagnostics may observe them without publishing a generic error event.

---

# 60. Event Consumers

Potential consumers of Diagnostics-owned events:

```text
Application
Diagnostics UI
Support tooling
Runtime operational policy
Monitoring projection
```

Only consume when a real reaction is required.

---

# 61. Application Example

```text
DiagnosticCollectionDegraded
    ↓
Application
    ↓
show non-blocking developer/support warning
```

Application should not assume business processing is broken.

---

# 62. Runtime Policy Example

Runtime might optionally observe:

```text
DiagnosticCollectionDegraded
```

for operational awareness.

It should not automatically cancel work solely because Diagnostics degraded.

---

# 63. UI Example

```text
DiagnosticCapabilityChanged
capability = Tracing
state = Unavailable
    ↓
Diagnostics UI
    ↓
show "Tracing unavailable"
```

---

# 64. No Processing Module Dependency

Capture, Recognition, Translation, and Presentation should not require Diagnostics Event Bus events for normal execution.

---

# 65. No Event Loop

Invalid:

```text
DiagnosticCollectionDegraded
    ↓
Diagnostics subscriber
    ↓
ObserveError
    ↓
ErrorReported
    ↓
Diagnostics subscriber
```

Avoid telemetry/event feedback loops.

---

# 66. Observability of Diagnostics Events

Publishing Diagnostics events may itself be logged/traced internally.

That observability must not recursively create infinite event or telemetry loops.

---

# 67. Recursion Protection

Diagnostics infrastructure should mark/internalize its own telemetry where necessary to prevent:

```text
log about log
metric about metric emission
trace about trace exporter
```

from recursively expanding without bound.

---

# 68. Failure Handling

If Diagnostics event publication fails:

* preserve Diagnostics-owned committed state;
* do not block unrelated business processing;
* retry publication according to Event Bus/infrastructure policy;
* expose diagnostic degradation if appropriate;
* do not generate an unbounded recursive error stream.

---

# 69. Collection Failure Handling

Collection failure is represented through:

```text
DiagnosticCollectionStatus
DiagnosticCapabilityState
```

not through per-record failure events.

---

# 70. Export Failure Handling

Export failure normally returns:

```text
ExportDiagnosticBundleResult = Failed
```

and Diagnostics-owned error.

Do not publish a generic failure event by default.

---

# 71. Future Event Criteria

A new Diagnostics event should be added only when:

1. Diagnostics owns the underlying fact;
2. the fact is stable after publication;
3. another component genuinely needs asynchronous awareness;
4. a query alone is insufficient;
5. the event is not merely telemetry transport.

---

# 72. Deferred Event Candidates

Potential future events:

```text
DiagnosticPolicyChanged
DiagnosticRetentionPressureChanged
DiagnosticRemoteExportStateChanged
```

Only add after ownership and consumer requirements are clear.

---

# 73. Removed Future v1 Events

The v1 candidates:

```text
ProfileCaptured
RetentionCompleted
ExportRetryScheduled
AlertTriggered
CollectorRegistered
```

are not accepted automatically.

Reasons:

```text
ProfileCaptured
    → profiling telemetry/operation result

RetentionCompleted
    → infrastructure retention concern

ExportRetryScheduled
    → infrastructure scheduler/retry concern

AlertTriggered
    → future alerting subsystem ownership

CollectorRegistered
    → internal infrastructure lifecycle
```

---

# 74. Architecture Invariants

1. Diagnostics events describe Diagnostics-owned facts only.

2. Event Bus is not telemetry transport.

3. Logs are not published as domain events.

4. Metrics are not published as domain events.

5. Trace span lifecycle is not published as domain events.

6. Generic error observations are not published as domain events.

7. `LogRecorded` is removed.

8. `MetricUpdated` is removed.

9. `TraceStarted` is removed.

10. `TraceCompleted` is removed.

11. `ErrorReported` is removed.

12. `DiagnosticsFlushed` is removed.

13. `HealthStatusChanged` is replaced by explicit Diagnostics-owned health facts.

14. Diagnostics has no mandatory business-event subscriptions.

15. Diagnostics does not rely on processing-completion events to derive telemetry.

16. Application lifecycle is explicit, not hidden Event Bus control.

17. Diagnostics events are immutable.

18. Events publish after state commit.

19. Publication failure does not revert committed Diagnostics state.

20. EventId is the idempotency identity.

21. CorrelationId is not a deduplication identity.

22. Delivery guarantees come from canonical Event Bus.

23. Event payloads remain small.

24. Event payloads remain privacy-safe.

25. Diagnostic bundle contents never travel in Event Bus events.

26. Owner module health remains authoritative.

27. Diagnostics health events describe Diagnostics only.

28. Processing modules do not depend on Diagnostics events for correctness.

29. Future events require explicit ownership and consumers.

30. Event/telemetry recursion must remain bounded.

---

# 75. Testing — Removed Telemetry Events

Verify Diagnostics does not publish:

```text
LogRecorded
MetricUpdated
TraceStarted
TraceCompleted
ErrorReported
```

when normal observations occur.

---

# 76. Testing — No Mandatory Subscriptions

Diagnostics core must work without consuming:

```text
RecognitionCompleted
TranslationCompleted
PresentationUpdated
ReadingSessionChanged
StorageReady
StorageFailed
ApplicationStarted
ApplicationShutdownRequested
```

---

# 77. Testing — Capability Events

Verify:

```text
Tracing AVAILABLE → UNAVAILABLE
```

may publish:

```text
DiagnosticCapabilityChanged
```

after the state transition commits.

---

# 78. Testing — Collection Events

Verify:

```text
NORMAL → DEGRADED
```

publishes at most the intended state-change event rather than one event per dropped observation.

---

# 79. Testing — Recovery

Verify:

```text
DEGRADED → NORMAL
```

publishes:

```text
DiagnosticCollectionRecovered
```

when configured.

---

# 80. Testing — Export

Verify successful explicit export may publish:

```text
DiagnosticExportCompleted
```

only if event integration is enabled/required.

No bundle bytes appear in the event.

---

# 81. Testing — Publication Failure

Inject Event Bus failure after:

```text
DiagnosticCapabilityChanged
```

state commit.

Verify capability state remains authoritative.

---

# 82. Testing — Idempotency

Deliver the same event twice.

Verify downstream projection handles duplicate `EventId` safely.

---

# 83. Testing — Privacy

Verify Diagnostics events never contain:

```text
raw logs
raw traces
raw screenshots
OCR text
translation text
credentials
tokens
```

---

# 84. Testing — Recursion

Simulate Diagnostics event publication failure generating internal diagnostics.

Verify the system does not create an infinite diagnostic/event loop.

---

# 85. Related Documents

```text
doc/02-modules/diagnostics/MODULE.md
doc/02-modules/diagnostics/CONTRACT.md
doc/02-modules/diagnostics/STATES.md
doc/02-modules/diagnostics/ERRORS.md
doc/02-modules/diagnostics/README.md

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md

doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/03-infrastructure/logging/
doc/03-infrastructure/telemetry/
```

---

# 86. Completion Criteria

This specification is synchronized when:

* business Event Bus is no longer the telemetry transport;
* `LogRecorded` is removed;
* `MetricUpdated` is removed;
* `TraceStarted/TraceCompleted` are removed;
* generic `ErrorReported` is removed;
* `DiagnosticsFlushed` is removed;
* mandatory subscriptions to business events are removed;
* Application lifecycle dependency is explicit;
* Diagnostics events describe capability/collection/health facts only;
* export events remain optional and bounded;
* EventId is the deduplication identity;
* delivery guarantees defer to canonical Event Bus;
* event payloads remain privacy-safe;
* recursion protection is recognized;
* future events require actual ownership and consumers.

---

# 87. Summary

Diagnostics has two separate communication paths.

Telemetry path:

```text
Business / Runtime Module
        ↓
Diagnostic Observation
        ↓
Diagnostics Semantics
        ↓
Logging / Telemetry Infrastructure
```

Event path:

```text
Diagnostics-owned State Change
        ↓
state committed
        ↓
Diagnostics Event
        ↓
Event Bus
```

Examples:

```text
Tracing capability unavailable
    ↓
DiagnosticCapabilityChanged
```

or:

```text
Diagnostic collection degraded
    ↓
DiagnosticCollectionDegraded
```

The central rule is:

```text
Telemetry describes
operational observations.

Events describe
stable Diagnostics-owned state facts.

Those two channels must not be merged.
```
