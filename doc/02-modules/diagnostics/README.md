# Diagnostics Module

> **Project:** CRAI
> **Module:** `diagnostics`
> **Path:** `doc/02-modules/diagnostics/README.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Overview

The Diagnostics Module is CRAI's cross-cutting observability boundary.

It provides stable, privacy-safe semantics for understanding:

```text
what happened
where it happened
how long it took
which operation it belonged to
which capability is degraded
which error owner reported a failure
```

The preferred flow is:

```text
Business / Runtime Module
        ↓
Diagnostic Observation
        ↓
Diagnostics Semantics
        ↓
Logging / Telemetry Infrastructure
        ↓
Collector / Exporter / Store
        ↓
Diagnostic View
```

Diagnostics is observational.

It does not participate in business execution authority.

---

# 2. Architecture Position

Diagnostics sits beside CRAI business/runtime processing rather than inside the business pipeline.

```text
Reading Session
Capture
Recognition
Text Processing
Translation
Presentation
Preferences
Runtime
Infrastructure
        │
        └──────→ Diagnostic Observation
                         ↓
                    Diagnostics
                         ↓
              ┌──────────┼──────────┐
              ↓          ↓          ↓
           Logging     Metrics    Tracing
              │          │          │
              └──────────┼──────────┘
                         ↓
              Logging / Telemetry
                 Infrastructure
                         ↓
               Diagnostic Views
```

Diagnostics may observe all major components.

It does not control them.

---

# 3. Central Ownership Rule

CRAI separates observability into three ownership layers.

```text
Business / Runtime Module
    → owns meaning

Diagnostics
    → owns diagnostic representation semantics

Infrastructure
    → owns transport, buffering, export, storage
```

Example:

```text
Capture
    owns CAP-ACQ-003 ProviderTimeout

Diagnostics
    may observe that error

Telemetry Infrastructure
    transports/stores the observation
```

The original Capture error remains authoritative.

---

# 4. Module Identity

```text
Module ID: diagnostics
Module Type: Cross-Cutting Support Module
Primary Domain: Diagnostic semantics and operational visibility
Execution Authority: None
Business Authority: None
Telemetry Transport Owner: Infrastructure
Diagnostic Persistence Owner: Infrastructure
MVP Priority: Required
```

Diagnostics is not:

```text
Runtime Controller
Business Orchestrator
Logging Backend
Metrics Database
Tracing Backend
Alert Manager
Artifact Store
Global Error Owner
```

---

# 5. Primary Responsibilities

Diagnostics owns:

```text
diagnostic observation contracts
structured diagnostic record semantics
severity semantics
correlation conventions
safe diagnostic metadata
diagnostic health aggregation
diagnostic capability reporting
bounded diagnostic queries
diagnostic snapshots
support bundle semantics
privacy/redaction rules
cross-module operational summaries
```

---

# 6. Explicit Non-Responsibilities

Diagnostics MUST NOT:

* execute Capture;
* execute Recognition;
* execute Translation;
* build Presentation;
* manage Reading Session;
* create Runtime WorkItems;
* create Runtime Attempts;
* own RuntimeRevisionId;
* cancel Runtime work;
* retry processing;
* decide pipeline topology;
* modify business state because of telemetry;
* replace module-specific errors;
* own module-specific health authority;
* physically store logs;
* physically store metrics;
* physically store traces;
* expose vendor telemetry SDK objects.

---

# 7. Passive Observation

The central behavior is:

```text
System behavior
    ↓
Diagnostics observes
```

not:

```text
Diagnostics observes
    ↓
Diagnostics changes system behavior
```

Diagnostic observation must not become hidden orchestration.

---

# 8. Failure Independence

Example:

```text
Translation
    ↓
translation succeeds

Telemetry exporter
    ↓
network unavailable
```

Result:

```text
Translation remains successful.
Diagnostics may become DEGRADED.
```

Observability failure and business failure are separate domains.

---

# 9. Diagnostic Observation

Diagnostics receives structured observations such as:

```text
LogObservation
MetricObservation
TraceObservation
ErrorObservation
HealthObservation
PerformanceObservation
```

These use stable diagnostic abstractions.

They are not business Event Bus events.

---

# 10. DiagnosticObservation

Conceptually:

```text
DiagnosticObservation
├── observationId
├── signalType
├── producerModule
├── severity
├── occurredAt
├── correlationContext?
├── safeAttributes
├── diagnosticCode?
├── messageKey?
└── diagnosticRef?
```

All public diagnostic data must remain serializable and backend-independent.

---

# 11. Correlation

Diagnostics may correlate operations using:

```text
correlationId
causationId
traceId
spanId
sessionId
readingContextRevision
preferenceRevision
runtimeRevisionId
workItemId
attemptId
artifactId
moduleOperationId
```

Not every observation contains every field.

---

# 12. Correlation Is Not Authority

Diagnostics may know:

```text
ReadingContextRevision = 5
RuntimeRevisionId = 12
AttemptId = A31
```

but it does not decide whether any of those authorities are current.

Their owning modules remain authoritative.

---

# 13. Structured Logging

Structured diagnostic logging is preferred.

Example:

```text
module = recognition
operation = recognize
duration_ms = 84
error_code = REC-...
```

over:

```text
"Recognition failed somewhere after 84ms"
```

Structured fields improve filtering, aggregation, and privacy control.

---

# 14. Logging Ownership

Diagnostics defines:

```text
structured log semantics
severity conventions
safe attributes
correlation
redaction expectations
```

Logging infrastructure owns:

```text
logger implementation
log buffer
sink
rotation
encoding
retention implementation
file/stdout/remote transport
```

---

# 15. Metrics

Metrics represent bounded operational measurements.

Examples:

```text
capture_operation_duration_ms
recognition_operation_total
translation_provider_failure_total
runtime_attempt_cancelled_total
```

The producing module owns the metric's semantic meaning.

Diagnostics provides conventions and aggregation support.

---

# 16. Metric Cardinality

Metrics must avoid unbounded dimensions.

Avoid:

```text
SessionId
WorkItemId
AttemptId
ArtifactId
full URL
filesystem path
error message
user content
```

as normal metric dimensions.

Use logs/traces for high-cardinality correlation.

---

# 17. Tracing

Tracing explains relationships between operations.

Example:

```text
Runtime Attempt
    ↓
Capture span
    ↓
Artifact publication span
    ↓
Recognition span
```

Tracing observes execution.

It does not create execution dependencies.

---

# 18. Tracing Infrastructure

Diagnostics defines common trace semantics and correlation.

Telemetry infrastructure owns:

```text
tracer implementation
sampling
span exporter
transport
trace backend
trace persistence
```

---

# 19. Health

Each module owns the meaning of its own health.

Examples:

```text
CaptureHealth
StorageHealth
SchedulerHealth
TranslationProviderHealth
```

Diagnostics observes and aggregates these health states.

---

# 20. Health Ownership

Incorrect:

```text
Diagnostics
    ↓
sets Capture health = Degraded
```

Correct:

```text
Capture
    ↓
Capture HealthObservation
    ↓
Diagnostics
    ↓
DiagnosticHealthSnapshot
```

Capture remains authoritative.

---

# 21. DiagnosticHealthSnapshot

Diagnostics may expose:

```text
DiagnosticHealthSnapshot
├── overallStatus
├── componentHealth[]
├── activeDegradations[]
├── recentCriticalIssues[]
└── observedAt
```

The snapshot is a diagnostic projection.

---

# 22. Normalized Health

For aggregation, Diagnostics may normalize health into:

```text
Healthy
Degraded
Unavailable
Unknown
```

This normalized state does not replace the owner module's native health vocabulary.

---

# 23. Error Observation

Module errors retain their original identity.

Example:

```text
CAP-ACQ-003 ProviderTimeout
```

may become:

```text
ErrorObservation
├── ownerModule = capture
├── originalErrorCode = CAP-ACQ-003
├── diagnosticSeverity = Error
└── correlationContext
```

No new `DIAG-*` error is created merely because Diagnostics observed it.

---

# 24. DIAG Errors

`DIAG-*` errors are used only when Diagnostics itself fails.

Examples:

```text
InvalidDiagnosticObservation
DiagnosticQueryUnavailable
HealthAggregationFailed
DiagnosticExportFailed
DiagnosticRedactionFailed
DiagnosticCapabilityUnavailable
DiagnosticInvariantViolation
```

Detailed taxonomy belongs to:

```text
ERRORS.md
```

---

# 25. Performance Observations

Diagnostics may observe:

```text
operation duration
provider duration
queue latency
memory summary
CPU summary
buffer pressure
Artifact counts
processing counts
```

Semantic ownership remains with the producer.

---

# 26. Profiling

Profiling is optional.

Possible profiling data:

```text
CPU sample summary
allocation summary
hot-path summary
thread activity
provider timing summary
```

Profiling must remain:

```text
bounded
explicitly enabled
privacy-safe
low-impact
```

---

# 27. Privacy First

Diagnostics is one of CRAI's highest-risk cross-cutting modules because it can observe many components.

Default rule:

```text
if diagnostic data
may expose reading content
and is not required
    ↓
do not collect it
```

Avoid collection rather than relying only on later redaction.

---

# 28. Forbidden Diagnostic Content

Normal Diagnostics MUST NOT include:

```text
raw screenshots
raw frame buffers
OCR text
full source text
translation text
raw HTML
provider prompts
provider responses
credentials
cookies
authentication tokens
private keys
```

---

# 29. Sensitive Metadata

Potentially sensitive metadata includes:

```text
URLs
filesystem paths
window titles
document titles
provider payload metadata
user-defined labels
```

Such metadata must be minimized, bounded, or redacted.

---

# 30. Safe Metadata

Prefer:

```text
opaque identifiers
enum states
durations
counts
sizes
revision IDs
safe error codes
capability identifiers
provider names
```

---

# 31. Diagnostic Queries

Possible public queries:

```text
GetDiagnosticHealth
GetRecentDiagnosticIssues
GetDiagnosticSnapshot
GetModuleDiagnosticSummary
GetRuntimeDiagnosticSummary
GetDiagnosticCapabilities
```

Optional:

```text
GetRecentLogs
GetMetricSummary
GetTraceSummary
```

depending on runtime profile.

---

# 32. Query Semantics

Diagnostic queries are read-only.

They must not:

```text
restart modules
retry work
repair state
cancel Runtime
change provider
change health
```

---

# 33. Diagnostic Snapshot

A bounded diagnostic snapshot may contain:

```text
application version
runtime profile
component health
recent errors
recent warnings
selected metric summaries
trace summaries
safe environment information
redaction summary
```

It must not automatically contain full telemetry history.

---

# 34. Support Bundle

Diagnostics may support an explicit support/debug bundle.

Flow:

```text
User / Developer
    ↓
ExportDiagnosticBundle
    ↓
collect bounded diagnostic data
    ↓
redact
    ↓
serialize
    ↓
DiagnosticBundle
```

This is different from continuous telemetry export.

---

# 35. Diagnostic Bundle Export

A bundle may contain:

```text
manifest
health snapshot
selected recent logs
selected metrics
trace summaries
error summaries
safe environment summary
redaction report
```

Raw reading content is excluded by default.

---

# 36. Support Export vs Telemetry Export

Two separate concepts:

```text
Diagnostic Bundle Export
    → explicit bounded support/debug operation

Telemetry Export
    → ongoing infrastructure transport
```

Do not merge them.

---

# 37. Exporter Ownership

The old architecture exposed destinations such as:

```text
Console
Local File
HTTP
OpenTelemetry
```

directly in Diagnostics.

v2 treats those as infrastructure deployment choices.

Stable Diagnostics contracts remain vendor-neutral.

---

# 38. Backend Independence

Diagnostics must remain independent from:

```text
OpenTelemetry
Prometheus
Grafana
Sentry
Datadog
Elastic
CloudWatch
vendor SDK
```

These may be infrastructure implementations.

They are not public domain types.

---

# 39. Event Bus Boundary

The business Event Bus is not the telemetry channel.

Do not publish one event for every:

```text
log entry
metric update
trace start
trace completion
error observation
performance sample
```

---

# 40. Removed Telemetry Events

Diagnostics v2 does not require:

```text
LogRecorded
MetricUpdated
TraceStarted
TraceCompleted
ErrorReported
DiagnosticsFlushed
```

as public Event Bus events.

---

# 41. Diagnostics-Owned Events

Only stable Diagnostics-owned state facts may use Event Bus.

Recommended minimal candidates:

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

when asynchronous consumers actually require it.

---

# 42. No Mandatory Business Event Subscriptions

Diagnostics does not require direct subscriptions to:

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

for correctness.

---

# 43. Direct Instrumentation

Preferred:

```text
Recognition operation
    ↓
Recognition measures itself
    ↓
ObserveMetric / Trace
```

rather than:

```text
RecognitionCompleted Event
    ↓
Diagnostics guesses timing
```

The producer owns the most accurate semantics.

---

# 44. Application Lifecycle

Diagnostics initialization/shutdown should use explicit application lifecycle integration.

Preferred:

```text
Application composition root
    ↓
Diagnostics.Initialize()
```

not:

```text
ApplicationStarted event
    ↓
Diagnostics subscriber
```

as a correctness dependency.

---

# 45. Module Lifecycle

Diagnostics v2 lifecycle:

```text
UNINITIALIZED
      ↓
INITIALIZING
      ↓
READY
  ↕
DEGRADED
      ↓
STOPPING
      ↓
STOPPED
```

---

# 46. READY

`READY` means Diagnostics core semantics are available.

While READY:

```text
observations may be accepted
queries may run
health aggregation may run
snapshots may be produced
support exports may be requested
```

---

# 47. No `COLLECTING`

Collection is normal behavior while READY.

There is no:

```text
READY
    ↓
COLLECTING
    ↓
READY
```

transition for every observation.

---

# 48. No `MONITORING`

Health aggregation/query is also normal READY behavior.

There is no global:

```text
MONITORING
```

state.

---

# 49. DEGRADED

`DEGRADED` means Diagnostics remains partially usable while one or more capabilities are impaired.

Example:

```text
Logging = Available
Metrics = Available
Tracing = Unavailable
Health = Available
```

Diagnostics may be:

```text
DEGRADED
```

while business execution continues normally.

---

# 50. No Global `FAILED`

One diagnostic capability failure does not imply total Diagnostics failure.

For example:

```text
trace exporter unavailable
```

does not require:

```text
Diagnostics = FAILED
```

Use capability state plus `DEGRADED`.

---

# 51. Capability State

Each diagnostic capability may have:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
DISABLED
UNKNOWN
```

Potential capabilities:

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

---

# 52. Capability vs Module State

Example:

```text
Tracing = UNAVAILABLE
Metrics = AVAILABLE
Logs = AVAILABLE
```

does not imply the entire module is unavailable.

---

# 53. Collection Status

Diagnostics may expose:

```text
NORMAL
PRESSURED
DEGRADED
UNAVAILABLE
```

for bounded collection behavior.

This describes observability collection only.

---

# 54. Backpressure

Diagnostic infrastructure must remain bounded.

Under pressure:

```text
sample
aggregate
drop lower-priority observations
truncate optional detail
```

rather than:

```text
grow memory indefinitely
block business processing indefinitely
```

---

# 55. Freshness and Retention

Diagnostics may retain only bounded recent operational information.

Retention semantics may include:

```text
Ephemeral
Recent
SupportEligible
```

Physical retention implementation belongs to infrastructure.

---

# 56. Export Operation State

Support export uses scoped phases:

```text
VALIDATING
    ↓
COLLECTING
    ↓
REDACTING
    ↓
SERIALIZING
    ↓
FINALIZING
    ↓
COMPLETED
```

Possible outcomes:

```text
COMPLETED_WITH_TRUNCATION
REJECTED
FAILED
ABORTED
```

The whole Diagnostics module does not enter `EXPORTING`.

---

# 57. Flush

Buffer flushing belongs primarily to logging/telemetry lifecycle infrastructure.

A bounded flush may occur during shutdown.

Diagnostics does not expose `FLUSHING` as a global business-domain state.

---

# 58. Shutdown

Typical:

```text
READY / DEGRADED
        ↓
STOPPING
        ↓
request bounded telemetry flush
        ↓
release resources
        ↓
STOPPED
```

Shutdown must not wait indefinitely for remote telemetry delivery.

---

# 59. Error Model

Diagnostics-owned error categories include:

```text
Observation
Query
Health Aggregation
Diagnostic Export
Privacy / Redaction
Capability
Diagnostics Resources
Configuration
Internal
```

---

# 60. Removed Backend-Oriented Errors

Public Diagnostics v2 does not use backend-oriented names such as:

```text
LoggerUnavailable
LogWriteFailed
MetricCollectionFailed
TraceStorageFailed
ExportTargetUnavailable
FlushFailed
CollectorInitializationFailed
```

These reveal implementation details.

---

# 61. Capability-Oriented Errors

Instead use stable semantics such as:

```text
DiagnosticCapabilityUnavailable
DiagnosticCapabilityDegraded
DiagnosticCapabilityInitializationFailed
DiagnosticObservationDropped
DiagnosticQueryUnavailable
DiagnosticExportFailed
```

---

# 62. Privacy Errors

Privacy errors include:

```text
DiagnosticRedactionFailed
ForbiddenDiagnosticContent
UnsafeDiagnosticMetadata
```

Privacy failures must fail closed.

---

# 63. Business Error Observation

Example:

```text
Translation
    ↓
TRN-PROV-003 ProviderTimeout
```

Diagnostics may observe it.

The public error remains:

```text
TRN-PROV-003
```

not a new `DIAG-*` error.

---

# 64. Infrastructure Failure

Example:

```text
trace exporter
    ↓
network unavailable
```

Infrastructure detail may be normalized to:

```text
Tracing capability = DEGRADED / UNAVAILABLE
```

and, when needed:

```text
DiagnosticCapabilityUnavailable
```

---

# 65. Diagnostics vs Runtime Observability

Runtime owns:

```text
RuntimeRevision
WorkItem
Attempt
queue latency
retry
cancellation
supersession
deadline
```

Diagnostics may expose projections/summaries of those facts.

It does not redefine them.

---

# 66. Diagnostics vs Module Observability

Each module owns important module-specific observability semantics.

Examples:

```text
Capture
    → capture_candidate_discard_total

Recognition
    → recognition_operation_duration_ms

Translation
    → translation_provider_failure_total
```

Diagnostics provides shared conventions and views.

---

# 67. Diagnostics vs Logging Infrastructure

```text
Diagnostics
    → diagnostic semantics

Logging Infrastructure
    → log transport / buffering / sink / retention
```

---

# 68. Diagnostics vs Telemetry Infrastructure

```text
Diagnostics
    → metric/trace semantics and diagnostic views

Telemetry Infrastructure
    → meter/tracer implementation, sampling, exporters, transport
```

---

# 69. Diagnostics vs Storage

Diagnostics does not own persistent telemetry storage.

Storage/telemetry infrastructure may persist:

```text
logs
trace summaries
metric aggregates
support bundle data
```

according to configured retention policy.

---

# 70. Diagnostics vs UI Adapter

UI Adapter may display:

```text
system health
recent diagnostic issues
capability state
support bundle controls
performance summary
```

Diagnostics provides data.

UI Adapter owns rendering.

---

# 71. Diagnostics vs Application

Application may react to Diagnostics facts.

Examples:

```text
show tracing unavailable
offer support bundle
show observability degraded warning
```

Diagnostics does not directly manipulate product UI.

---

# 72. Diagnostic Capabilities

Conceptually:

```text
DiagnosticCapabilities
├── logsAvailable
├── metricsAvailable
├── tracingAvailable
├── healthAvailable
├── profilingAvailable
├── supportBundleAvailable
└── remoteTelemetryAvailable
```

Capability and health are distinct.

---

# 73. Capability vs Health Example

```text
TracingAvailable = true
TracingState = DEGRADED
```

means tracing exists but is currently impaired.

---

# 74. Performance Principles

Diagnostics should prioritize:

```text
low overhead
non-blocking observation
bounded queues
structured metadata
sampling
aggregation
graceful degradation
```

---

# 75. Hot Path Rule

Critical processing paths should perform only bounded lightweight instrumentation.

Example:

```text
Recognition
    ↓
record duration/counter
    ↓
bounded async telemetry
```

Avoid synchronous heavy diagnostic export on processing hot paths.

---

# 76. Backpressure Priority

Possible relative drop priority:

```text
Critical / Error
    >
Warning
    >
Info
    >
Debug
    >
Trace
```

Exact dropping/sampling policy belongs to infrastructure.

---

# 77. Recursion Protection

Diagnostics must prevent recursive telemetry failure loops.

Example to avoid:

```text
logger fails
    ↓
log logger failure
    ↓
logger fails
    ↓
log logger failure
    ↓
...
```

Possible infrastructure protections:

```text
recursion guard
emergency sink
rate limiting
one-shot suppression
```

---

# 78. Diagnostic Failure Events

Normal Diagnostics errors are returned through:

```text
DiagnosticError / operation result
```

They are not automatically published as Event Bus events.

State changes may independently emit Diagnostics-owned facts.

---

# 79. Event Publication Failure

If a Diagnostics capability transition commits but event publication fails:

```text
capability state remains committed.
```

Do not revert it.

Infrastructure handles publication recovery.

---

# 80. Example — Recognition Failure

```text
Recognition
    ↓
RecognitionError
    ↓
Runtime completion
```

Separately:

```text
Recognition
    ↓
ObserveError
    ↓
Diagnostics / telemetry
```

Diagnostics does not own Recognition failure.

---

# 81. Example — Capture Health

```text
Capture
    ↓
CaptureHealth = Degraded
    ↓
HealthObservation
    ↓
Diagnostics
    ↓
ComponentHealthSummary
```

Capture remains authoritative.

---

# 82. Example — Metric

```text
Recognition
    ↓
ObserveMetric
metricName = recognition_operation_duration_ms
value = 84 ms
```

No `MetricUpdated` Event Bus event is required.

---

# 83. Example — Trace Backend Failure

```text
Tracing exporter
    ↓
network failure
    ↓
Tracing capability = UNAVAILABLE
    ↓
Diagnostics = DEGRADED
```

Processing continues.

---

# 84. Example — Support Bundle

```text
User requests support information
        ↓
ExportDiagnosticBundle
        ↓
collect bounded snapshot
        ↓
privacy filtering
        ↓
redaction
        ↓
serialize
        ↓
BundleRef
```

No raw reading content is included.

---

# 85. Example — Diagnostic Buffer Pressure

```text
diagnostic input rate increases
        ↓
buffer approaches capacity
        ↓
collection = PRESSURED
        ↓
sampling increases
        ↓
lower-priority records dropped
```

Business work continues.

---

# 86. Common Architecture Mistake — Diagnostics Owns Errors

Wrong:

```text
Translation error
    ↓
DiagnosticsError
```

Correct:

```text
Translation error
    ↓
Diagnostics observes original error
```

---

# 87. Common Architecture Mistake — Event Bus as Telemetry

Wrong:

```text
LogRecorded
MetricUpdated
TraceCompleted
```

for every telemetry record.

Correct:

```text
ObserveLog
ObserveMetric
Trace instrumentation
```

through diagnostic/telemetry abstractions.

---

# 88. Common Architecture Mistake — Diagnostics Controls Health

Wrong:

```text
Diagnostics.setCaptureHealth(...)
```

Correct:

```text
Capture emits/provides its own health
Diagnostics aggregates it
```

---

# 89. Common Architecture Mistake — Vendor Type in Contract

Wrong:

```text
OpenTelemetrySpan
PrometheusCounter
SentryEvent
```

in public Diagnostics contracts.

Correct:

```text
DiagnosticObservation
MetricObservation
TraceSpanDescriptor
```

---

# 90. Common Architecture Mistake — Global Export State

Wrong:

```text
Diagnostics = EXPORTING
```

and all other diagnostic activity stops.

Correct:

```text
Diagnostics = READY

ExportOperation = COLLECTING / REDACTING / ...
```

---

# 91. Common Architecture Mistake — Telemetry Failure Blocks Work

Wrong:

```text
metrics exporter unavailable
    ↓
Translation waits/fails
```

Correct:

```text
metrics exporter unavailable
    ↓
Diagnostics capability degrades
    ↓
Translation continues
```

---

# 92. Architecture Invariants

1. Diagnostics is passive.

2. Diagnostics is not business execution authority.

3. Business modules retain semantic ownership.

4. Runtime retains Runtime observability ownership.

5. Diagnostics owns diagnostic representation semantics.

6. Logging infrastructure owns log transport/storage.

7. Telemetry infrastructure owns metrics/traces transport.

8. Domain errors preserve their original owner.

9. Diagnostics only creates `DIAG-*` errors for Diagnostics-owned failures.

10. Module-owned health remains authoritative.

11. Diagnostics may aggregate health.

12. Correlation does not transfer authority.

13. Diagnostic observations are structured.

14. Public diagnostic data is serializable.

15. Raw reading content is excluded by default.

16. Secrets are always forbidden.

17. Metric dimensions remain bounded.

18. Diagnostic buffers remain bounded.

19. Diagnostic failure normally does not fail business execution.

20. Event Bus is not telemetry transport.

21. `LogRecorded` is not required.

22. `MetricUpdated` is not required.

23. `TraceStarted` is not required.

24. `TraceCompleted` is not required.

25. Generic `ErrorReported` is not required.

26. Diagnostics has no mandatory subscription to business completion events.

27. Diagnostics lifecycle remains small.

28. Collection occurs while READY.

29. Health aggregation occurs while READY.

30. Export is a scoped operation.

31. Flush is lifecycle/infrastructure coordination.

32. Capability-specific degradation is supported.

33. Global FAILED is not required for ordinary telemetry failures.

34. Vendor telemetry implementations remain outside public contracts.

35. Diagnostic queries are read-only.

36. Support bundle export is explicit and bounded.

37. Privacy failure fails closed.

38. Telemetry backpressure never grows unbounded.

39. Event publication failure does not revert committed Diagnostics state.

40. Recursive telemetry failure loops must remain bounded.

---

# 93. MVP Scope

Recommended MVP:

```text
structured diagnostic observation contract
correlation context
basic log observation
basic metric observation
basic tracing abstraction
ErrorObservation
HealthObservation
DiagnosticHealthSnapshot
DiagnosticCapabilities
recent issue summary
basic diagnostic snapshot
privacy/redaction policy
bounded buffering
fake telemetry adapters
```

Optional if needed early:

```text
ExportDiagnosticBundle
```

---

# 94. Deferred Scope

Possible future capabilities:

```text
remote telemetry backend
advanced tracing UI
interactive profiling
performance timeline
diagnostic replay
crash dump integration
anomaly detection
user-consented diagnostic upload
audit subsystem
advanced support bundles
```

---

# 95. Testing Priorities

Diagnostics tests should focus on:

```text
observation validation
error ownership
health aggregation
correlation
privacy
redaction
capability degradation
bounded buffering
support export
failure isolation
backend independence
recursion protection
```

---

# 96. Ownership Tests

Verify Diagnostics never:

```text
creates Runtime WorkItem
creates Attempt
changes Reading Session
retries Translation
restarts Recognition
changes Capture health
takes ownership of external ErrorCode
```

---

# 97. Privacy Tests

Verify Diagnostics rejects/excludes:

```text
raw screenshot
OCR text
translation text
password
API key
token
cookie
provider response
```

---

# 98. Failure Isolation Tests

Inject:

```text
log sink failure
metric exporter failure
trace exporter failure
health aggregation failure
bundle export failure
```

Verify business processing remains unaffected.

---

# 99. Capability Tests

Verify:

```text
Tracing unavailable
```

does not prevent:

```text
GetDiagnosticHealth
GetRecentLogs
```

when those capabilities remain available.

---

# 100. Backpressure Tests

Fill diagnostic buffers.

Verify:

```text
bounded drop/sampling
collection degradation
no unbounded memory
no indefinite business blocking
```

---

# 101. Correlation Tests

Verify correct coexistence of:

```text
SessionId
ReadingContextRevision
PreferenceRevision
RuntimeRevisionId
WorkItemId
AttemptId
ArtifactId
TraceId
```

without authority confusion.

---

# 102. Document Set

```text
02-modules/
└── diagnostics/
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

Defines:

```text
what Diagnostics is
where it sits
what it owns
what infrastructure owns
how it should be used
```

## MODULE.md

Defines:

```text
module ownership
cross-cutting responsibilities
diagnostic semantics
infrastructure boundary
architecture invariants
```

## CONTRACT.md

Defines:

```text
DiagnosticObservation
correlation
health summaries
queries
capabilities
support export
privacy contracts
```

## STATES.md

Defines:

```text
Diagnostics lifecycle
capability state
collection status
export operation phases
recovery
shutdown
```

## EVENTS.md

Defines:

```text
Diagnostics-owned state facts
Event Bus boundary
telemetry/event separation
event publication semantics
```

## ERRORS.md

Defines:

```text
DIAG error taxonomy
capability failures
query failures
export failures
privacy failures
resource pressure
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

Recommended:

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

This keeps semantic ownership and failure isolation clear before Event Bus integration.

---

# 106. Related Documents

```text
doc/02-modules/diagnostics/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
└── ERRORS.md

doc/01-architecture/core/
├── EVENT_BUS.md
├── EVENT_CONVENTION.md
└── STATE_MACHINE.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── MODULE_DEPENDENCY.md
└── OWNERSHIP_MAP.md

doc/01-architecture/runtime/
├── PIPELINE_RUNTIME.md
└── RUNTIME_OBSERVABILITY.md

doc/03-infrastructure/logging/
doc/03-infrastructure/telemetry/
```

---

# 107. Completion Checklist

Diagnostics is synchronized when:

* [ ] Diagnostics is classified as cross-cutting support;
* [ ] Diagnostics remains passive;
* [ ] business modules retain semantic ownership;
* [ ] module errors retain original ErrorCode ownership;
* [ ] module health remains owner-defined;
* [ ] Diagnostics only aggregates health;
* [ ] logging transport/storage is infrastructure-owned;
* [ ] metrics/tracing transport is infrastructure-owned;
* [ ] backend/vendor types are absent from public contracts;
* [ ] Event Bus is not used as telemetry transport;
* [ ] `LogRecorded` is removed;
* [ ] `MetricUpdated` is removed;
* [ ] `TraceStarted/TraceCompleted` are removed;
* [ ] generic `ErrorReported` is removed;
* [ ] mandatory business-event subscriptions are removed;
* [ ] lifecycle is `UNINITIALIZED → INITIALIZING → READY ↔ DEGRADED → STOPPING → STOPPED`;
* [ ] collection is normal READY behavior;
* [ ] export uses scoped phases;
* [ ] capability-specific degradation is supported;
* [ ] global FAILED is absent for ordinary telemetry failure;
* [ ] diagnostic buffers remain bounded;
* [ ] support export is bounded and privacy-safe;
* [ ] recursion protection exists;
* [ ] all six Diagnostics documents use the same terminology and ownership model.

---

# 108. Summary

Diagnostics v2 follows:

```text
Business / Runtime Module
        ↓
Diagnostic Observation
        ↓
Diagnostics Semantics
        ↓
Logging / Telemetry Infrastructure
        ↓
Diagnostic View
```

Health:

```text
Owner Module
    ↓
Owner Health
    ↓
Diagnostics Observation
    ↓
Aggregate Diagnostic Health
```

Errors:

```text
Owner Module
    ↓
Owner Error
    ↓
Diagnostics Observation
```

not:

```text
Owner Error
    ↓
Diagnostics takes ownership
```

The central invariant is:

```text
Diagnostics observes
and explains system behavior.

It does not own
the behavior,
execution authority,
or business failures
that it observes.
```
