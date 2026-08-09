# Diagnostics Module

> **Project:** CRAI
> **Module:** `diagnostics`
> **Path:** `doc/02-modules/diagnostics/MODULE.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Module Definition

Diagnostics is the CRAI cross-cutting module responsible for defining stable diagnostic semantics and exposing bounded diagnostic views of system behavior.

Its primary responsibility is:

```text
System Components
    ↓
Diagnostic Contracts
    ↓
Structured Diagnostic Signals
    ↓
Logging / Telemetry Infrastructure
    ↓
Collectors / Exporters / Stores
    ↓
Diagnostic Views
```

Diagnostics answers:

> **How should CRAI describe and correlate operational behavior in a safe, module-independent way?**

Diagnostics does not answer:

> How should logs, traces, and metrics be physically transported or stored?

That belongs to infrastructure.

Diagnostics also does not answer:

> What business action should occur because a diagnostic signal was observed?

That belongs to the owning domain, Application, Business Pipeline Orchestration, or Runtime.

---

# 2. Module Identity

```text
Module ID: diagnostics
Module Type: Cross-Cutting Support Module
Primary Domain: Diagnostic semantics and operational visibility
Primary Inputs: Diagnostic signals
Primary Outputs: Diagnostic records / snapshots / summaries
Execution Authority: None
Business Authority: None
Persistence Implementation: Infrastructure
Telemetry Transport: Infrastructure
MVP Priority: Required
```

Diagnostics is not:

```text
Business Orchestrator
Runtime Controller
Logging Backend
Metrics Database
Tracing Backend
Alert Manager
Artifact Store
Error Owner for every module
```

---

# 3. Architectural Position

Preferred architecture:

```text
Reading Session
Capture
Recognition
Text Processing
Translation
Presentation
Runtime
Infrastructure
        ↓
Diagnostic Abstractions
        ↓
Diagnostics
        ↓
Logging / Telemetry Infrastructure
        ↓
Exporter / Sink / Store
        ↓
Developer / UI / Monitoring View
```

A business module may report:

```text
operation duration
error classification
state transition
resource observation
health observation
```

without knowing:

```text
log file location
OpenTelemetry exporter
metrics database
remote monitoring vendor
trace storage backend
```

---

# 4. Ownership Separation

CRAI separates four concerns.

## 4.1 Business / Runtime Modules

Own:

```text
domain events
domain errors
module-specific metrics meaning
module health meaning
operation semantics
```

Example:

```text
Recognition
    owns RecognitionError

Runtime
    owns AttemptState

Capture
    owns CaptureHealth meaning
```

---

## 4.2 Diagnostics

Owns:

```text
diagnostic contracts
diagnostic severity semantics
correlation conventions
diagnostic record shape
safe metadata rules
diagnostic snapshot/query semantics
cross-module diagnostic aggregation semantics
redaction requirements
diagnostic health summaries
```

---

## 4.3 Logging Infrastructure

Owns:

```text
log transport
log buffering
log sinks
log rotation
physical log encoding
retention implementation
```

---

## 4.4 Telemetry Infrastructure

Owns:

```text
metrics transport
trace transport
exporters
sampling implementation
telemetry backend integration
telemetry buffering
telemetry storage integration
```

---

# 5. Central Architecture Rule

```text
Modules own the meaning
of what happened.

Diagnostics owns
how operational meaning is represented safely.

Infrastructure owns
how that information is transported and stored.
```

---

# 6. Primary Responsibilities

Diagnostics is responsible for:

* defining diagnostic record contracts;
* defining correlation metadata;
* defining diagnostic severity;
* defining health aggregation semantics;
* defining bounded diagnostic snapshots;
* providing safe diagnostic queries;
* aggregating module-owned health observations;
* normalizing diagnostic metadata;
* enforcing diagnostic privacy/redaction rules;
* exposing diagnostic capabilities to Application/UI;
* supporting exporter-neutral diagnostic integration;
* defining operational summary semantics.

---

# 7. Explicit Non-Responsibilities

Diagnostics MUST NOT:

* execute business logic;
* execute Capture;
* execute Recognition;
* execute Translation;
* build Presentation;
* schedule Runtime work;
* create WorkItems;
* create Attempts;
* cancel Runtime work;
* retry failed processing;
* change Reading Session state;
* decide pipeline consequences;
* own every module's error taxonomy;
* replace domain errors with generic diagnostic errors;
* own physical log storage;
* own physical metrics storage;
* own physical trace storage;
* import vendor-specific telemetry SDKs into business contracts.

---

# 8. Passive Observation

Diagnostics is observational.

```text
System behavior
    ↓
Diagnostics observes
```

not:

```text
Diagnostics observes failure
    ↓
Diagnostics changes business state
```

A diagnostic signal must never itself alter application correctness.

---

# 9. Diagnostic Failure Independence

Diagnostic failure must not normally stop business execution.

Example:

```text
Translation succeeds
    ↓
trace exporter unavailable
```

Result:

```text
Translation remains successful.
```

Telemetry loss may be reported separately.

---

# 10. Diagnostics Is Not Error Ownership

Each module retains ownership of its errors.

Examples:

```text
CaptureSourceUnavailable
    → Capture error

ReadingContextRevisionConflict
    → Reading Session error

ProviderTimeout
    → owning provider/processing module

AttemptSuperseded
    → Runtime
```

Diagnostics may record or aggregate them.

It must not rename them into:

```text
DiagnosticsError
```

and thereby erase original ownership.

---

# 11. Diagnostic Signal

A generic diagnostic signal conceptually contains:

```text
DiagnosticSignal
├── signalId
├── signalType
├── severity
├── producer
├── occurredAt
├── correlationContext?
├── operationContext?
├── safeAttributes
└── diagnosticPayload?
```

Possible signal types:

```text
Log
MetricObservation
TraceObservation
HealthObservation
ErrorObservation
PerformanceObservation
```

This is a diagnostic abstraction.

It is not necessarily an Event Bus event.

---

# 12. Diagnostic Record

A normalized record may be:

```text
DiagnosticRecord
├── recordId
├── recordType
├── timestamp
├── producerModule
├── severity
├── correlationContext
├── safeAttributes
├── messageKey?
├── diagnosticCode?
└── payload?
```

All fields crossing public boundaries must be serializable.

---

# 13. Correlation Context

Common correlation may include:

```text
CorrelationContext
├── correlationId?
├── causationId?
├── traceId?
├── spanId?
├── sessionId?
├── runtimeRevisionId?
├── workItemId?
├── attemptId?
├── artifactId?
└── moduleOperationId?
```

Not every field applies to every operation.

Ownership of those IDs remains with their original modules.

---

# 14. Correlation Is Not Authority

Diagnostics may record:

```text
RuntimeRevisionId
WorkItemId
AttemptId
ReadingContextRevision
PreferenceRevision
```

for correlation.

Diagnostics MUST NOT interpret these identifiers as authority outside their owning domain.

---

# 15. Severity

Recommended diagnostic severity:

```text
Trace
Debug
Info
Warning
Error
Critical
```

Severity describes operational significance.

It does not automatically prescribe:

```text
retry
restart
cancel
shutdown
```

---

# 16. Structured Logging

Diagnostic logging should use structured records.

Prefer:

```text
module = recognition
operation = recognize
duration_ms = 91
error_code = REC-...
```

over opaque free-form text.

Human-readable messages may still be included as non-authoritative fields.

---

# 17. Metrics

Metric meaning remains defined by the producing module.

Examples:

```text
capture_operation_duration_ms
recognition_operation_total
translation_provider_failure_total
runtime_attempt_cancelled_total
```

Diagnostics may expose common metric registration/query contracts.

It does not redefine module ownership of the metric's semantic meaning.

---

# 18. Metric Cardinality

Metrics MUST avoid unbounded labels.

Do not normally use:

```text
SessionId
WorkItemId
AttemptId
URL
document path
full error message
user content
```

as metric labels.

High-cardinality correlation belongs to logs/traces.

---

# 19. Tracing

Tracing may represent execution relationships such as:

```text
Runtime Attempt
    ↓
Capture
    ↓
Artifact publication
    ↓
Recognition
```

Trace relationships improve observability.

They do not create processing dependencies.

---

# 20. Trace Ownership

Diagnostics defines common tracing semantics.

Telemetry infrastructure owns:

```text
span exporter
sampling
transport
trace backend
physical persistence
```

---

# 21. Health Observation

Each module owns the meaning of its health.

Examples:

```text
CaptureHealth
RecognitionHealth
TranslationProviderHealth
StorageHealth
SchedulerHealth
```

Diagnostics may aggregate these observations.

---

# 22. Diagnostic Health Summary

Diagnostics may expose:

```text
DiagnosticHealthSnapshot
├── overallStatus
├── componentHealth[]
├── activeDegradations[]
├── recentCriticalIssues[]
└── observedAt
```

This snapshot is observational.

It does not override owner-specific health state.

---

# 23. Overall Health

Possible aggregate values:

```text
Healthy
Degraded
Unavailable
Unknown
```

Aggregation policy must be explicit.

For example:

```text
one optional provider unavailable
```

must not necessarily imply:

```text
CRAI = Unavailable
```

---

# 24. Health Aggregation Policy

Diagnostics may classify components by importance:

```text
Required
Optional
Degradable
External
```

Overall health calculation may use these classifications.

The classification must not become hidden business orchestration.

---

# 25. Error Observation

Diagnostics may record:

```text
ErrorObservation
├── originalErrorCode
├── ownerModule
├── severity
├── recoveryClassification?
├── correlationContext?
└── diagnosticRef?
```

The original error identity must be preserved.

---

# 26. No Error Translation

Incorrect:

```text
CAP-ACQ-003 ProviderTimeout
    ↓
Diagnostics converts to
DIAG-ERR-001 GenericFailure
```

Correct:

```text
CAP-ACQ-003
    retained as original error
    +
diagnostic observation metadata
```

---

# 27. Performance Observation

Diagnostics may collect:

```text
operation duration
queue latency
provider latency
normalization duration
memory observations
CPU observations
frame counts
Artifact counts
```

The producing owner determines the semantic interpretation.

---

# 28. Profiling

Profiling is optional and must be explicitly enabled.

Potential profiling metadata:

```text
CPU sample summaries
allocation summaries
hot-path summaries
provider timing
thread activity
```

Raw profiler implementation remains infrastructure/platform-specific.

---

# 29. Profiling Safety

Profiling MUST:

* remain bounded;
* avoid raw user content;
* avoid indefinite collection by default;
* not materially alter normal application behavior;
* clearly identify diagnostic mode when overhead is significant.

---

# 30. Diagnostic Snapshot

A bounded snapshot may provide:

```text
DiagnosticSnapshot
├── capturedAt
├── applicationVersion
├── runtimeProfile?
├── componentHealth
├── recentErrors
├── recentWarnings
├── selectedMetrics
├── traceSummary?
└── environmentSummary?
```

Snapshots must obey privacy/redaction policy.

---

# 31. Diagnostic Export

Diagnostics may expose logical export semantics.

Example:

```text
ExportDiagnosticBundle
        ↓
bounded diagnostic snapshot
        ↓
redaction
        ↓
serialization
        ↓
export result
```

The physical destination/file/backend remains infrastructure/Application-owned.

---

# 32. Export Is Not Telemetry Transport

There are two distinct concepts:

```text
Telemetry Export
    → ongoing infrastructure delivery

Diagnostic Bundle Export
    → explicit bounded support/debug operation
```

Do not merge them.

---

# 33. Diagnostic Bundle

Conceptually:

```text
DiagnosticBundle
├── manifest
├── application metadata
├── health snapshot
├── selected logs
├── selected metrics
├── selected trace summaries
├── error summaries
└── redaction report
```

Raw reading content must not be included by default.

---

# 34. Redaction

Diagnostics owns common redaction semantics.

Potential sensitive classes:

```text
UserContent
Credential
Token
URL
FilesystemPath
WindowTitle
DocumentTitle
ProviderPayload
Prompt
ModelResponse
ImageData
```

---

# 35. Privacy-First Rule

The default rule is:

```text
if diagnostic value
may contain user reading content
and is not explicitly required
    ↓
do not record it
```

Redaction is a fallback.

Avoid collection where possible.

---

# 36. Raw Content Prohibition

Normal diagnostics MUST NOT include:

```text
captured screenshots
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
```

---

# 37. Safe Identifiers

Prefer:

```text
opaque IDs
enum states
durations
sizes
counts
revision IDs
safe error codes
provider identifiers
bounded capability names
```

---

# 38. Public Diagnostic Queries

Possible queries:

```text
GetDiagnosticHealth
GetRecentDiagnosticIssues
GetDiagnosticSnapshot
GetModuleDiagnosticSummary
GetRuntimeDiagnosticSummary
GetDiagnosticCapabilities
```

Detailed schemas belong to `CONTRACT.md`.

---

# 39. Query Semantics

Diagnostic queries are read-only.

They must not:

```text
retry failed work
repair state
restart providers
cancel Runtime
change module health
```

---

# 40. Diagnostics vs Logging Infrastructure

Diagnostics owns:

```text
structured log semantics
safe fields
severity conventions
correlation conventions
query/view semantics
```

Logging infrastructure owns:

```text
logger implementation
buffer
sink
rotation
retention
file/stdout/backend transport
```

---

# 41. Diagnostics vs Telemetry Infrastructure

Diagnostics owns:

```text
metric/trace semantic conventions
health aggregation
diagnostic views
```

Telemetry infrastructure owns:

```text
meter implementation
tracer implementation
sampling
exporter
transport
backend
```

---

# 42. Diagnostics vs Runtime Observability

Runtime owns detailed execution semantics such as:

```text
RuntimeRevision
WorkItem
Attempt
queue time
retry count
supersession
cancellation
```

Diagnostics may aggregate or expose Runtime observability data.

It does not re-own those concepts.

---

# 43. Diagnostics vs Module Observability

Each module defines its important domain-specific measurements.

Example:

```text
Capture
    defines capture_candidate_discard_total

Translation
    defines translation_provider_failure_total
```

Diagnostics provides common conventions and aggregation.

---

# 44. Diagnostics vs UI Adapter

UI Adapter may display:

```text
system health
recent errors
diagnostic status
support bundle controls
basic performance summary
```

Diagnostics provides platform-independent data.

UI Adapter owns actual controls/rendering.

---

# 45. Diagnostics vs Application

Application may decide:

```text
show degraded-health banner
offer support bundle export
show provider warning
request diagnostic snapshot
```

Diagnostics does not directly manipulate product UI.

---

# 46. Event Bus Boundary

Diagnostic telemetry does not automatically belong on the business Event Bus.

Do not publish one Event Bus event for every:

```text
log line
metric update
trace completion
span
counter increment
```

This would create unnecessary load and coupling.

---

# 47. Removed `LogRecorded`

A public:

```text
LogRecorded
```

event is not required.

Log records travel through logging/diagnostic abstractions.

---

# 48. Removed `MetricUpdated`

A public:

```text
MetricUpdated
```

event is not required.

Metrics belong to telemetry collection, not business Event Bus flow.

---

# 49. Removed `TraceCompleted`

A public:

```text
TraceCompleted
```

event is not required.

Trace completion belongs to tracing infrastructure.

---

# 50. Diagnostic Events

Only architecturally meaningful Diagnostics-owned state facts should use the Event Bus.

Potential examples:

```text
DiagnosticHealthChanged
DiagnosticCollectionDegraded
DiagnosticCollectionRecovered
DiagnosticExportCompleted
```

Even these should be added only when a real cross-module consumer exists.

---

# 51. ErrorReported Event

A generic:

```text
ErrorReported
```

Event Bus event should normally be avoided.

Errors are already returned by their owners and observed through diagnostics.

Publishing every error to the Event Bus duplicates the error transport path.

---

# 52. Mandatory Consumed Events

Recommended:

```text
None.
```

Diagnostics should normally receive diagnostic signals through explicit diagnostic abstractions rather than subscribe to every business event.

---

# 53. Why Diagnostics Should Not Subscribe to Everything

A universal Event Bus subscriber creates:

* unnecessary event traffic;
* hidden coupling;
* dependency on complete business event coverage;
* duplicate telemetry;
* privacy risk;
* observability correctness tied to workflow events.

Modules should instrument directly through stable observability contracts.

---

# 54. Module Lifecycle

Diagnostics module lifecycle should remain small.

Recommended:

```text
UNINITIALIZED
INITIALIZING
READY
DEGRADED
STOPPING
STOPPED
```

Detailed transitions belong to `STATES.md`.

---

# 55. Removed `COLLECTING`

Continuous collection is normal behavior while:

```text
READY
```

It does not need a separate global lifecycle state.

---

# 56. Removed `EXPORTING`

One diagnostic export operation does not need to change the entire module lifecycle.

Export should use a scoped operation phase.

---

# 57. Removed Global `FAILED`

A failed exporter/logger should not automatically put the entire Diagnostics module into:

```text
FAILED
```

If some observability remains available:

```text
DEGRADED
```

is more precise.

---

# 58. Diagnostic Export Operation

A scoped export may use:

```text
PREPARING
COLLECTING
REDACTING
SERIALIZING
COMPLETED
```

with:

```text
REJECTED
FAILED
```

as operation outcomes.

These are not module lifecycle states.

---

# 59. Collector Failure

Example:

```text
trace exporter fails
```

Possible result:

```text
Diagnostics = DEGRADED
logs still available
metrics still available
business processing continues
```

---

# 60. Full Diagnostic Unavailability

If all diagnostic paths are unavailable:

```text
Diagnostics may remain DEGRADED
```

while the application continues.

Diagnostics should only cause application shutdown when an explicit security/compliance policy requires it.

That is not the default CRAI behavior.

---

# 61. Error Ownership

Diagnostics owns only errors about Diagnostics-owned functionality.

Examples:

```text
DiagnosticContractInvalid
DiagnosticSnapshotFailed
DiagnosticExportFailed
DiagnosticRedactionFailed
DiagnosticCollectorUnavailable
DiagnosticExporterUnavailable
DiagnosticInvariantViolation
```

---

# 62. External Telemetry Failures

Failures such as:

```text
log sink unavailable
trace exporter unavailable
metrics backend unavailable
```

may be normalized into diagnostic availability errors.

They do not become business-domain failures.

---

# 63. Logging Failure

A logging sink failure must not:

```text
fail Recognition
rollback Reading Session
reject Translation Artifact
cancel Runtime Attempt
```

---

# 64. Event Publication Failure

If Diagnostics owns an actual state event and publication fails:

```text
Diagnostics state remains committed.
```

Event publication recovery follows infrastructure policy.

---

# 65. Configuration

Diagnostics consumes typed configuration.

Possible configuration:

```text
minimum log severity
trace sampling preference
metrics enabled
profiling enabled
diagnostic retention preference
support bundle limits
redaction policy
export policy
```

---

# 66. Hard Safety Limits

Infrastructure/runtime policy may impose hard limits such as:

```text
maximum diagnostic buffer memory
maximum bundle size
maximum trace rate
maximum exporter queue length
```

User preference cannot override those safety constraints.

---

# 67. Dependency Rules

Business modules may depend on:

```text
diagnostic interfaces
logging interfaces
metrics interfaces
tracing interfaces
```

They MUST NOT depend on:

```text
Diagnostics implementation
vendor exporter
telemetry backend
log database
monitoring SDK
```

---

# 68. No Reverse Business Dependency

Diagnostics must not require business modules to import Diagnostics implementation.

Preferred dependency:

```text
Business Module
    ↓
Observability Contract
```

not:

```text
Business Module
    ↓
Concrete Diagnostics Service
```

---

# 69. Backend Independence

Diagnostics semantics must remain independent from:

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

Any such integration belongs to infrastructure/provider composition.

---

# 70. Performance Goals

Diagnostics should prioritize:

```text
low overhead
non-blocking collection where safe
bounded buffering
cheap structured metadata
sampling
aggregation
graceful degradation
```

---

# 71. Hot Path Rule

Diagnostic instrumentation on critical processing paths must avoid expensive synchronous work.

Example:

```text
Recognition hot path
    ↓
record lightweight timing
    ↓
asynchronous/bounded exporter
```

Avoid:

```text
Recognition
    ↓
serialize large diagnostic bundle synchronously
```

---

# 72. Backpressure

Diagnostic pipelines must be bounded.

When telemetry infrastructure is overloaded, preferred degradation may include:

```text
drop low-priority diagnostics
sample
aggregate
reduce detail
report collection degradation
```

Never accumulate unbounded telemetry memory.

---

# 73. Diagnostic Priority

Possible relative priority:

```text
Critical/Error
    >
Warning
    >
Info
    >
Debug
    >
Trace
```

Dropping strategy belongs to infrastructure policy.

Diagnostics may define semantic priority hints.

---

# 74. Diagnostic Retention

Diagnostics may define retention semantics such as:

```text
ephemeral
short-lived
support-bundle eligible
audit-required
```

Physical retention implementation belongs to infrastructure.

---

# 75. Audit vs Diagnostics

Security/compliance audit records should not automatically be treated as ordinary diagnostics.

If CRAI later requires audit logging:

```text
Audit
```

should receive an explicit architecture contract because durability and retention requirements differ.

---

# 76. User-Facing Diagnostics

Potential product-visible information:

```text
Capture source unavailable
Translation provider degraded
Storage unavailable
System running in reduced capability mode
```

UI should consume safe summaries, not internal stack traces.

---

# 77. Developer Diagnostics

Developer mode may expose more detail:

```text
trace IDs
module operation IDs
safe timing information
provider diagnostic codes
bounded state summaries
```

Privacy rules still apply.

---

# 78. Support Bundle

A user-support bundle should be:

```text
explicitly requested
bounded
redacted
reviewable where possible
versioned
```

It must not silently capture raw reading content.

---

# 79. Diagnostic Capability

Diagnostics may expose supported capabilities:

```text
DiagnosticCapabilities
├── LogsAvailable
├── MetricsAvailable
├── TracingAvailable
├── HealthAvailable
├── ProfilingAvailable
├── SupportBundleAvailable
└── RemoteExportAvailable
```

Capability availability may change at runtime.

---

# 80. Health vs Capability

These are distinct:

```text
Capability
    → feature exists

Health
    → feature currently operating
```

Example:

```text
TracingAvailable = true
TracingHealth = Degraded
```

---

# 81. Initialization

Typical initialization:

```text
UNINITIALIZED
      ↓
INITIALIZING
      ↓
load diagnostic configuration
      ↓
initialize available collectors/exporters
      ↓
READY
```

Partial failure may result in:

```text
DEGRADED
```

rather than startup failure.

---

# 82. Shutdown

```text
READY / DEGRADED
        ↓
STOPPING
        ↓
flush bounded diagnostics where policy permits
        ↓
release exporters/collectors
        ↓
STOPPED
```

Shutdown must not hang indefinitely waiting for telemetry export.

---

# 83. Example — Recognition Failure

```text
Recognition
    ↓
RecognitionError
    ↓
Runtime receives processing completion
```

Separately:

```text
Recognition
    ↓
Diagnostic error observation
    ↓
Diagnostics / telemetry
```

Diagnostics does not become the Recognition error owner.

---

# 84. Example — Runtime Attempt

```text
Runtime Attempt A
    ↓
trace span begins
    ↓
Capture span
    ↓
Recognition span
    ↓
Attempt completion
    ↓
trace completed by tracing infrastructure
```

No `TraceCompleted` business event is necessary.

---

# 85. Example — Capture Health

```text
Capture
    ↓
CaptureHealthChanged
```

Diagnostics may observe:

```text
CaptureHealth = Degraded
```

and include it in:

```text
DiagnosticHealthSnapshot
```

The Capture-owned health fact remains authoritative.

---

# 86. Example — Exporter Failure

```text
Telemetry exporter
    ↓
network unavailable
    ↓
export fails
    ↓
Diagnostics collection status = DEGRADED
```

Business processing continues.

---

# 87. Example — Support Export

```text
User requests diagnostic bundle
        ↓
Application
        ↓
Diagnostics export contract
        ↓
collect bounded safe records
        ↓
redact
        ↓
serialize
        ↓
return export result
```

No business state mutation occurs.

---

# 88. Example — Metrics Backend Down

```text
metrics backend unavailable
        ↓
metrics exporter degrades
        ↓
Diagnostics reports reduced observability
```

Capture/Recognition/Translation continue normally.

---

# 89. Architecture Risks

## 89.1 Diagnostics Becoming a God Module

Do not move every:

```text
error
health state
metric semantic
log semantic
Runtime state
```

into Diagnostics ownership.

Original modules retain domain meaning.

---

## 89.2 Event Bus Flooding

Do not publish:

```text
LogRecorded
MetricUpdated
TraceCompleted
```

for every telemetry record.

---

## 89.3 Privacy Leakage

Do not record raw user reading content for convenience.

---

## 89.4 Vendor Lock-In

Do not expose vendor-specific SDK objects through diagnostics contracts.

---

## 89.5 Business Coupling

Business correctness must not depend on successful telemetry export.

---

## 89.6 Unbounded Collection

Logs/traces/metrics must remain bounded through infrastructure policy.

---

# 90. Design Principles

1. Diagnostics is passive.

2. Diagnostics is non-authoritative for business state.

3. Original modules own domain semantics.

4. Diagnostics owns diagnostic representation semantics.

5. Infrastructure owns diagnostic transport/storage.

6. Telemetry failure normally does not fail business execution.

7. Public diagnostic records are structured.

8. Correlation metadata preserves original identifier ownership.

9. Diagnostics is backend-independent.

10. Privacy is enforced before export.

11. Avoid collection of sensitive data rather than relying only on redaction.

12. Diagnostic queries are read-only.

13. Event Bus is not telemetry transport.

14. Operation telemetry belongs to logging/metrics/tracing abstractions.

15. Diagnostics module lifecycle remains small.

16. Export operation state is scoped, not global.

17. Diagnostic buffering is bounded.

18. Business modules do not depend on diagnostics implementation.

---

# 91. Architecture Invariants

1. Diagnostics never executes business logic.

2. Diagnostics never owns Runtime execution authority.

3. Diagnostics never owns Reading Session state.

4. Diagnostics never owns processing module results.

5. Diagnostics does not replace module-specific errors.

6. Original ErrorCode is preserved in diagnostic observations.

7. Diagnostics defines common diagnostic semantics.

8. Logging infrastructure owns log transport/storage.

9. Telemetry infrastructure owns metric/trace transport.

10. Diagnostic data is structured.

11. Diagnostic data is privacy-safe.

12. Raw image content is not logged.

13. OCR/translation content is not logged by default.

14. Credentials are never logged.

15. Correlation IDs may be propagated across module boundaries.

16. Correlation identifiers do not transfer authority.

17. Metrics avoid unbounded cardinality.

18. Tracing does not create business dependency.

19. Module health remains owner-defined.

20. Diagnostics may aggregate health without replacing owner state.

21. `LogRecorded` is not required as Event Bus event.

22. `MetricUpdated` is not required as Event Bus event.

23. `TraceCompleted` is not required as Event Bus event.

24. Generic `ErrorReported` Event Bus flow is not required.

25. Diagnostics has no mandatory subscription to every business event.

26. Collection failure normally degrades observability only.

27. Diagnostics module does not require global `COLLECTING` state.

28. Diagnostic export does not require global `EXPORTING` state.

29. Ordinary collector failure does not require global `FAILED`.

30. Telemetry buffers remain bounded.

31. Support bundle export is explicit and bounded.

32. Vendor-specific implementations remain outside module contracts.

33. Public diagnostic queries are read-only.

34. Diagnostic failure never silently changes business state.

---

# 92. MVP Scope

Recommended MVP:

```text
structured log contract
common correlation context
basic metric contract
basic tracing abstraction
module health aggregation
recent error summaries
DiagnosticHealthSnapshot
bounded recent diagnostic view
privacy/redaction rules
diagnostic query API
fake collector/exporter tests
```

Optional MVP if support workflow requires it:

```text
bounded diagnostic bundle export
```

---

# 93. Deferred Scope

Possible future capabilities:

```text
interactive profiler
remote telemetry backend
distributed tracing backend
advanced support bundle
performance timeline
diagnostic replay
crash dump integration
automated anomaly detection
user-consented diagnostic upload
audit subsystem
```

---

# 94. Testing Strategy

Diagnostics must be testable without:

```text
real cloud telemetry
real monitoring backend
real log database
Recognition
Translation
native UI
```

---

# 95. Unit Tests

Test:

```text
record normalization
severity handling
correlation propagation
health aggregation
redaction
safe metadata filtering
snapshot generation
export filtering
bounded collection
```

---

# 96. Ownership Tests

Verify Diagnostics never:

```text
changes Reading Session
creates Runtime WorkItem
creates Attempt
retries processing
changes Capture health directly
changes Translation provider
converts domain errors into Diagnostics-owned errors
```

---

# 97. Privacy Tests

Verify:

```text
image bytes rejected/redacted
OCR text excluded
translation text excluded
credentials excluded
tokens excluded
URLs/path metadata bounded
support bundle safe by default
```

---

# 98. Failure Tests

Inject:

```text
log sink failure
metrics exporter failure
trace exporter failure
health collector failure
bundle export failure
```

Verify business execution remains unaffected.

---

# 99. Backpressure Tests

Verify:

```text
diagnostic buffer capacity reached
```

results in bounded degradation rather than unbounded memory growth.

---

# 100. Correlation Tests

Verify correlation across:

```text
Reading Session
Runtime Revision
WorkItem
Attempt
Capture
Recognition
Translation
Presentation
```

without Diagnostics claiming authority over any identifier.

---

# 101. Related Documents

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

# 102. Documentation Ownership

This file defines:

```text
Diagnostics module identity
diagnostic semantics ownership
correlation rules
privacy boundary
health aggregation role
logging/telemetry infrastructure boundary
Event Bus boundary
module lifecycle expectations
architecture invariants
```

Detailed public schemas belong to:

```text
CONTRACT.md
```

Detailed lifecycle belongs to:

```text
STATES.md
```

Detailed Diagnostics-owned facts belong to:

```text
EVENTS.md
```

Detailed error taxonomy belongs to:

```text
ERRORS.md
```

---

# 103. Completion Criteria

Diagnostics is architecturally synchronized when:

* Diagnostics is classified as cross-cutting support;
* business modules retain semantic ownership;
* error ownership remains with original modules;
* logging transport/storage is infrastructure-owned;
* metrics/tracing transport is telemetry infrastructure-owned;
* diagnostic contracts remain backend-independent;
* Event Bus is not used as general telemetry transport;
* `LogRecorded`, `MetricUpdated`, and `TraceCompleted` are absent as mandatory public events;
* module lifecycle is reduced to initialization/readiness/degradation/shutdown;
* export is a scoped operation;
* collector failures degrade observability rather than business execution;
* health aggregation preserves owner-specific health;
* correlation IDs remain non-authoritative;
* sensitive user content is excluded by default;
* diagnostic buffering remains bounded;
* tests verify privacy, passive behavior, failure independence, and backend independence.

---

# 104. Summary

Diagnostics v2 follows:

```text
Business / Runtime Module
        ↓
Diagnostic Signal
        ↓
Diagnostics Semantics
        ↓
Logging / Telemetry Infrastructure
        ↓
Collector / Exporter / Store
        ↓
Diagnostic View
```

Health follows:

```text
Module-owned Health
        ↓
Diagnostics Observation
        ↓
Aggregate Health Snapshot
```

Errors follow:

```text
Module-owned Error
        ↓
Diagnostic Observation
```

not:

```text
Module Error
    ↓
Diagnostics takes ownership
```

The central invariant is:

```text
Diagnostics observes
and explains system behavior.

It does not own
the behavior being observed.
```
