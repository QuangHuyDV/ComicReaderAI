# Diagnostics Contract

> **Project:** CRAI
> **Module:** `diagnostics`
> **Path:** `doc/02-modules/diagnostics/CONTRACT.md`
> **Contract Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the public contract boundary of the Diagnostics module.

Diagnostics provides stable contracts for:

```text
diagnostic observations
correlation
health aggregation
diagnostic summaries
diagnostic snapshots
bounded diagnostic export
privacy-safe diagnostic metadata
```

The primary diagnostic flow is:

```text
Producing Module
    ↓
Diagnostic Observation Contract
    ↓
Diagnostics Semantics
    ↓
Logging / Telemetry Infrastructure
    ↓
Sink / Exporter / Store
```

Diagnostics does not expose infrastructure-specific logging, metrics, or tracing backends through its public domain contract.

---

# 2. Contract Scope

This document defines:

```text
DiagnosticSeverity
DiagnosticCorrelationContext
DiagnosticObservation
ErrorObservation
HealthObservation
PerformanceObservation
DiagnosticHealthSnapshot
DiagnosticIssueSummary
DiagnosticSnapshot
DiagnosticCapabilities
DiagnosticBundleExportRequest
DiagnosticBundleExportResult
diagnostic query contracts
redaction/privacy contracts
```

This document does not define:

```text
business-domain error schemas
Runtime WorkItem lifecycle
Runtime Attempt lifecycle
module-specific health semantics
log sink implementation
metrics transport
trace exporter
telemetry backend
physical persistence
business Event Bus workflow
```

---

# 3. Architectural Boundary

Canonical flow:

```text
Capture / Recognition / Translation / Runtime / ...
        ↓
module-owned operational meaning
        ↓
DiagnosticObservation
        ↓
Diagnostics
        ↓
logging / telemetry infrastructure
```

Important:

```text
Diagnostics observes owner-defined meaning.

It does not take ownership
of the meaning being observed.
```

---

# 4. Public Contract Principles

## 4.1 Backend Independence

Public Diagnostics contracts MUST NOT expose:

```text
OpenTelemetry SDK types
Prometheus collectors
Sentry objects
Datadog objects
Elastic documents
CloudWatch SDK objects
logger implementation types
native profiler handles
```

---

## 4.2 Passive Semantics

Calling a Diagnostics contract must not:

```text
change Reading Session
cancel Runtime work
retry processing
restart provider
mutate Artifact state
```

---

## 4.3 Failure Independence

Diagnostic collection failure must not normally fail the producer's business operation.

Example:

```text
Recognition succeeds
+
trace exporter unavailable
```

Recognition remains successful.

---

## 4.4 Original Ownership Preservation

When Diagnostics records a module error, it must preserve:

```text
original ErrorCode
original owner module
original severity/category where applicable
```

Diagnostics must not replace the error with a generic Diagnostics-owned error.

---

# 5. DiagnosticSeverity

```text
DiagnosticSeverity
- Trace
- Debug
- Info
- Warning
- Error
- Critical
```

Severity expresses operational significance only.

It does not prescribe:

```text
retry
restart
shutdown
cancellation
```

---

# 6. DiagnosticSignalType

```text
DiagnosticSignalType
- LogObservation
- MetricObservation
- TraceObservation
- ErrorObservation
- HealthObservation
- PerformanceObservation
```

These are diagnostic abstraction types.

They are not business Event Bus event types.

---

# 7. DiagnosticCorrelationContext

Conceptually:

```text
DiagnosticCorrelationContext
├── correlationId?
├── causationId?
├── traceId?
├── spanId?
├── sessionId?
├── readingContextRevision?
├── preferenceRevision?
├── runtimeRevisionId?
├── workItemId?
├── attemptId?
├── artifactId?
└── moduleOperationId?
```

Rules:

1. all fields are optional;
2. identifiers remain owned by their originating modules;
3. Diagnostics may correlate but not reinterpret authority;
4. correlation context must remain serializable.

---

# 8. Correlation Authority Rule

Incorrect:

```text
Diagnostics sees RuntimeRevisionId 8
    ↓
Diagnostics decides work for revision 7 is stale
```

Correct:

```text
Diagnostics records both revisions
for correlation only.
```

Runtime remains the authority owner.

---

# 9. DiagnosticAttributes

```text
DiagnosticAttributes
- bounded key/value metadata
```

Allowed values should normally be:

```text
Boolean
Integer
Decimal
String
Enum-like string
Duration
Size
Opaque identifier
```

Avoid large nested objects.

---

# 10. Attribute Safety

Diagnostic attributes MUST NOT contain:

```text
raw image bytes
OCR text
translation text
document content
raw HTML
credentials
tokens
cookies
provider prompts
provider responses
native handles
memory pointers
```

---

# 11. DiagnosticObservation

Base contract:

```text
DiagnosticObservation
├── observationId
├── signalType
├── producerModule
├── severity
├── occurredAt
├── correlationContext?
├── attributes
├── diagnosticCode?
├── messageKey?
└── diagnosticRef?
```

---

# 12. Observation Submission

Conceptually:

```text
ObserveDiagnostic(
    DiagnosticObservation
)

→ DiagnosticObservationResult
```

The implementation may map this to logging, metrics, tracing, or another backend.

The public contract does not require a specific transport.

---

# 13. DiagnosticObservationResult

```text
DiagnosticObservationResult
├── status
├── observationId
└── diagnosticFailure?
```

Possible status:

```text
Accepted
DroppedByPolicy
SamplingExcluded
RejectedUnsafe
Unavailable
```

Important:

A non-accepted diagnostic observation does not redefine the producer's business result.

---

# 14. Non-Blocking Semantics

Normal diagnostic submission SHOULD be designed so that producers do not wait on remote telemetry delivery.

Infrastructure may buffer asynchronously subject to bounded resource policy.

---

# 15. Log Observation

Conceptually:

```text
LogObservation
├── severity
├── producerModule
├── messageKey?
├── safeMessage?
├── attributes
├── correlationContext?
└── occurredAt
```

Structured fields are preferred over free-form messages.

---

# 16. Log Message Rule

`safeMessage`:

* is optional;
* is diagnostic only;
* must be privacy-safe;
* is not a programmatic contract.

Programmatic interpretation should use:

```text
diagnosticCode
ErrorCode
structured attributes
```

---

# 17. Metric Observation

Conceptually:

```text
MetricObservation
├── metricName
├── metricKind
├── value
├── unit?
├── dimensions
└── observedAt
```

Metric kinds may include:

```text
Counter
Gauge
HistogramObservation
DurationObservation
```

---

# 18. Metric Ownership

Metric names and semantic meaning remain owned by the producing module.

Example:

```text
capture_operation_duration_ms
    owner: Capture

runtime_attempt_cancelled_total
    owner: Runtime
```

Diagnostics may enforce common format conventions.

---

# 19. Metric Dimension Rules

Dimensions must remain bounded.

Do not normally use:

```text
SessionId
WorkItemId
AttemptId
ArtifactId
URL
document path
error message
```

as metric dimensions.

---

# 20. Trace Observation

Diagnostics may expose tracing abstractions conceptually:

```text
TraceSpanStart
TraceSpanAnnotation
TraceSpanEnd
```

or an equivalent scoped tracing contract.

The exact programming-language instrumentation API is implementation-specific.

---

# 21. TraceSpanDescriptor

Conceptually:

```text
TraceSpanDescriptor
├── spanName
├── spanKind?
├── producerModule
├── correlationContext
├── parentSpanId?
├── safeAttributes
└── startedAt
```

---

# 22. TraceSpanCompletion

```text
TraceSpanCompletion
├── spanId
├── status
├── finishedAt
├── duration?
└── safeAttributes?
```

This is telemetry instrumentation.

It is not a business event.

---

# 23. No Public Business `StartTrace` Command Semantics

The old:

```text
StartTrace
FinishTrace
```

should not be interpreted as module-domain commands.

Tracing APIs may exist as instrumentation interfaces.

They do not participate in business command/event semantics.

---

# 24. ErrorObservation

```text
ErrorObservation
├── ownerModule
├── originalErrorCode
├── originalCategory?
├── originalSeverity?
├── diagnosticSeverity
├── recoveryClassification?
├── correlationContext?
├── safeAttributes
└── diagnosticRef?
```

---

# 25. Error Ownership Rule

Example:

```text
CAP-ACQ-003 ProviderTimeout
```

remains:

```text
ownerModule = capture
originalErrorCode = CAP-ACQ-003
```

Diagnostics must not rewrite it as:

```text
DIAG-ERR-001
```

---

# 26. ReportError Replacement

The old generic command:

```text
ReportError
```

is replaced conceptually by:

```text
ObserveError(ErrorObservation)
```

The difference is important:

```text
ReportError
    may imply Diagnostics owns the error

ObserveError
    explicitly preserves external ownership
```

---

# 27. HealthObservation

Each module may provide:

```text
HealthObservation
├── componentId
├── ownerModule
├── healthState
├── importanceClass?
├── reasonCode?
├── observedAt
├── correlationContext?
└── safeAttributes?
```

---

# 28. Module Health Ownership

Health states are owner-defined.

Examples:

```text
Capture
    Healthy / Degraded / Unavailable / Recovering / Stopped

Diagnostics
    may normalize for aggregation
```

Diagnostics must preserve the original owner state where possible.

---

# 29. Normalized Health Status

For aggregate views, Diagnostics may normalize component health into:

```text
NormalizedHealthStatus
- Healthy
- Degraded
- Unavailable
- Unknown
```

This normalized status is a diagnostic projection.

It does not replace module-owned health state.

---

# 30. ComponentImportance

Optional aggregation hint:

```text
ComponentImportance
- Required
- Degradable
- Optional
- External
```

This may influence aggregate health.

It must not be used as hidden business orchestration.

---

# 31. PerformanceObservation

```text
PerformanceObservation
├── operationName
├── producerModule
├── duration?
├── size?
├── count?
├── resourceSummary?
├── correlationContext?
└── observedAt
```

This is intended for bounded operational measurements.

---

# 32. ProfilingObservation

Optional:

```text
ProfilingObservation
├── profileType
├── producer
├── summary
├── samplingWindow
└── observedAt
```

Raw profiler-native data should remain behind infrastructure unless explicitly exported through a safe support format.

---

# 33. Public Queries

Recommended Diagnostics queries:

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

depending on MVP implementation.

---

# 34. GetDiagnosticHealth

```text
GetDiagnosticHealth()
→ DiagnosticHealthSnapshot
```

Read-only.

---

# 35. DiagnosticHealthSnapshot

```text
DiagnosticHealthSnapshot
├── overallStatus
├── componentHealth[]
├── activeDegradations[]
├── recentCriticalIssues[]
├── observedAt
└── diagnosticCapabilitySummary?
```

---

# 36. ComponentHealthSummary

```text
ComponentHealthSummary
├── componentId
├── ownerModule
├── ownerHealthState?
├── normalizedStatus
├── importanceClass?
├── reasonCode?
└── observedAt
```

---

# 37. GetRecentDiagnosticIssues

```text
GetRecentDiagnosticIssues
├── severityAtLeast?
├── producerModule?
├── since?
├── limit
└── issueTypes?
```

Returns bounded summaries only.

---

# 38. DiagnosticIssueSummary

```text
DiagnosticIssueSummary
├── issueId
├── producerModule
├── diagnosticSeverity
├── originalErrorCode?
├── diagnosticCode?
├── occurredAt
├── correlationSummary?
└── safeSummary
```

---

# 39. GetDiagnosticSnapshot

```text
GetDiagnosticSnapshot
├── includeHealth
├── includeRecentIssues
├── includeMetricSummary
├── includeTraceSummary
├── timeRange?
└── sizeLimit?
```

Returns:

```text
DiagnosticSnapshot
```

---

# 40. DiagnosticSnapshot

```text
DiagnosticSnapshot
├── snapshotId
├── capturedAt
├── applicationVersion
├── runtimeProfile?
├── health
├── recentIssues
├── metricSummary?
├── traceSummary?
├── environmentSummary?
├── redactionSummary
└── truncationSummary?
```

---

# 41. Snapshot Boundaries

Snapshots must be:

```text
bounded
immutable
privacy-safe
backend-independent
```

They must not automatically include full raw telemetry history.

---

# 42. GetModuleDiagnosticSummary

```text
GetModuleDiagnosticSummary
- moduleId
```

Returns a bounded module-focused summary.

It does not query or mutate the module's business state directly.

---

# 43. GetRuntimeDiagnosticSummary

May expose Runtime-owned operational projections such as:

```text
queue summary
Attempt summary
cancellation counts
retry summary
resource summary
```

The Runtime remains semantic owner.

Diagnostics is only exposing an aggregated view.

---

# 44. GetDiagnosticCapabilities

```text
GetDiagnosticCapabilities()
→ DiagnosticCapabilities
```

---

# 45. DiagnosticCapabilities

```text
DiagnosticCapabilities
├── logsAvailable
├── metricsAvailable
├── tracingAvailable
├── healthAvailable
├── profilingAvailable
├── supportBundleAvailable
├── remoteTelemetryAvailable
└── observedAt
```

Capabilities may vary by runtime profile.

---

# 46. Capability vs Health

Important:

```text
tracingAvailable = true
```

does not imply:

```text
tracing currently healthy
```

Capability describes existence.

Health describes current operability.

---

# 47. Diagnostic Bundle Export

Explicit support/debug export uses:

```text
ExportDiagnosticBundle
```

not generic ongoing telemetry export.

---

# 48. ExportDiagnosticBundle

```text
ExportDiagnosticBundle
├── requestId
├── timeRange?
├── includeLogs?
├── includeMetrics?
├── includeTraceSummary?
├── includeHealth?
├── includeEnvironmentSummary?
├── maximumBundleSize?
├── redactionProfile?
└── purpose?
```

---

# 49. Export Purpose

Optional values:

```text
UserSupport
DeveloperDebug
LocalInspection
```

Future compliance/audit export should use a separate architecture contract.

---

# 50. ExportDiagnosticBundleResult

```text
ExportDiagnosticBundleResult
├── requestId
├── status
├── bundleRef?
├── manifest?
├── redactionSummary
├── truncationSummary?
├── warnings[]
└── failure?
```

Possible status:

```text
Completed
CompletedWithTruncation
Rejected
Failed
```

---

# 51. BundleRef

`BundleRef` must be an implementation-neutral reference.

It must not encode:

```text
filesystem handle
cloud SDK object
HTTP client object
vendor exporter object
```

Application/infrastructure determines how the bundle is delivered to the user.

---

# 52. DiagnosticBundleManifest

```text
DiagnosticBundleManifest
├── bundleVersion
├── createdAt
├── includedSections[]
├── excludedSections[]
├── redactionProfile
├── estimatedSize?
└── privacyWarnings[]
```

---

# 53. Telemetry Export vs Diagnostic Bundle Export

Ongoing telemetry:

```text
Diagnostics / Telemetry Infrastructure
    ↓
configured exporter
```

Explicit support bundle:

```text
User/Application
    ↓
ExportDiagnosticBundle
```

These are separate contracts.

---

# 54. Removed Generic Export Targets

The old public list:

```text
Local Files
Console
HTTP Endpoint
OpenTelemetry
Future monitoring systems
```

should not be a stable module-domain enum.

Those are infrastructure deployment choices.

---

# 55. ExporterProfileRef

If an explicit infrastructure profile must be referenced, use:

```text
ExporterProfileRef
├── profileId
└── profileKind?
```

without exposing vendor-specific configuration through the Diagnostics domain contract.

---

# 56. Flush Semantics

The old public:

```text
Flush
```

should not be a normal business-facing Diagnostics command.

Flushing transport buffers belongs primarily to logging/telemetry infrastructure.

---

# 57. Controlled Flush

A controlled flush hook MAY exist for:

```text
application shutdown
test harness
support export completion
```

but should live in lifecycle/infrastructure contracts rather than general business API.

---

# 58. Querying Logs

If CRAI exposes:

```text
GetRecentLogs
```

the result must be:

```text
bounded
redacted
safe
read-only
```

It should not expose the physical logging backend.

---

# 59. DiagnosticLogEntry

```text
DiagnosticLogEntry
├── recordId
├── timestamp
├── severity
├── producerModule
├── diagnosticCode?
├── safeMessage?
├── correlationSummary?
└── safeAttributes
```

---

# 60. Querying Metrics

Prefer:

```text
GetMetricSummary
```

over exposing arbitrary raw backend metric storage.

---

# 61. MetricSummary

```text
MetricSummary
├── metricName
├── ownerModule
├── metricKind
├── unit?
├── timeWindow
├── aggregateValues
└── observedAt
```

Backend-specific time-series query languages are out of scope.

---

# 62. Querying Traces

Prefer:

```text
GetTraceSummary
```

or:

```text
GetTrace(traceId)
```

only if local/runtime profile retains trace data.

Trace availability is capability-dependent.

---

# 63. TraceSummary

```text
TraceSummary
├── traceId
├── startedAt
├── finishedAt?
├── duration?
├── rootOperation?
├── spanCount?
├── status?
└── safeAttributes?
```

---

# 64. Trace Storage Is External

Failure to retrieve a trace does not imply Diagnostics owns trace persistence.

Telemetry infrastructure determines retention and storage availability.

---

# 65. Diagnostic Error Contract

Diagnostics-owned operations may return:

```text
DiagnosticError
├── errorCode
├── category
├── severity
├── recoveryHint?
├── operation?
├── diagnosticRef?
└── safeMetadata?
```

Detailed codes belong to `ERRORS.md`.

---

# 66. Diagnostics-Owned Error Categories

Recommended:

```text
Validation
Collection
Query
Aggregation
Redaction
Export
Capability
InfrastructureCoordination
Internal
```

---

# 67. Diagnostics-Owned Errors

Examples:

```text
InvalidDiagnosticObservation
UnsafeDiagnosticPayload
DiagnosticCollectionUnavailable
DiagnosticQueryUnavailable
HealthAggregationFailed
DiagnosticSnapshotFailed
DiagnosticRedactionFailed
DiagnosticExportFailed
DiagnosticCapabilityUnavailable
DiagnosticInvariantViolation
```

---

# 68. Removed Backend-Specific Error Names

The old errors:

```text
LoggerUnavailable
MetricCollectionFailed
TraceStorageFailed
HealthCheckFailed
```

are too tied to implementation detail or imply incorrect ownership.

Prefer normalized Diagnostics-owned operation failures.

Underlying backend details remain diagnostic internals.

---

# 69. Producer Failure Semantics

If:

```text
ObserveDiagnostic
```

fails:

```text
producer business state remains unchanged
```

unless an explicit compliance policy states otherwise.

---

# 70. Privacy Classification

Diagnostic data may be classified as:

```text
Safe
SensitiveMetadata
UserContent
Credential
Secret
Forbidden
```

---

# 71. RedactionProfile

```text
RedactionProfile
- Standard
- Strict
- SupportBundle
- DeveloperLocal
```

Profiles may define increasing diagnostic detail.

No profile may permit secrets.

---

# 72. Redaction Result

```text
RedactionSummary
├── removedFieldCount
├── redactedFieldCount
├── excludedSections[]
└── warnings[]
```

---

# 73. Privacy Rule

Preferred:

```text
do not collect sensitive data
```

over:

```text
collect everything
then redact later
```

---

# 74. Sensitive Data Prohibition

Public Diagnostics contracts MUST NOT expose:

```text
password
API key
access token
refresh token
cookie
credential secret
private key
raw screenshot
raw OCR text
raw translation text
full provider prompt
full provider response
```

---

# 75. Environment Summary

A diagnostic snapshot may contain bounded environment information such as:

```text
application version
OS family/version
runtime profile
CPU architecture
memory class
GPU capability summary
enabled capability flags
```

Avoid:

```text
username
home directory
full filesystem paths
machine secrets
precise unrelated application inventory
```

---

# 76. Event Bus Boundary

Diagnostics contracts do not require general telemetry to travel through Event Bus.

The following are not required public Event Bus events:

```text
LogRecorded
MetricUpdated
TraceCompleted
ErrorReported
```

---

# 77. Diagnostics-Owned Events

Only stable state facts may become Diagnostics events if needed.

Possible:

```text
DiagnosticHealthChanged
DiagnosticCollectionDegraded
DiagnosticCollectionRecovered
DiagnosticExportCompleted
```

Detailed policy belongs to `EVENTS.md`.

---

# 78. No Mandatory Business Event Subscriptions

Diagnostics v2 does not require direct subscriptions to:

```text
ErrorOccurred
ReadingSessionChanged
StorageFailed
TranslationCompleted
RecognitionCompleted
```

to function correctly.

---

# 79. Why Business Event Subscriptions Are Removed

Module-specific instrumentation is more accurate.

Example:

```text
Recognition operation finishes
        ↓
Recognition records its metric
```

instead of:

```text
RecognitionCompleted event
        ↓
Diagnostics guesses Recognition metric semantics
```

---

# 80. Event-Based Observation Is Optional

An implementation may observe selected business events for secondary analytics if:

1. semantic ownership remains with the producer;
2. correctness does not depend on that subscription;
3. telemetry is not duplicated incorrectly;
4. privacy rules are preserved.

---

# 81. Health Update Contract

The old:

```text
UpdateHealthStatus
```

should be understood as:

```text
ObserveHealth
```

not:

```text
Diagnostics authoritatively sets another module's health.
```

---

# 82. ObserveHealth

```text
ObserveHealth(
    HealthObservation
)

→ DiagnosticObservationResult
```

The owner module remains authoritative.

---

# 83. Metric Update Contract

The old:

```text
RecordMetric
```

may remain as a low-level instrumentation abstraction internally, but public semantic naming should emphasize:

```text
ObserveMetric
```

to reinforce passive behavior.

---

# 84. ObserveMetric

```text
ObserveMetric(
    MetricObservation
)

→ DiagnosticObservationResult
```

---

# 85. ObserveLog

```text
ObserveLog(
    LogObservation
)

→ DiagnosticObservationResult
```

---

# 86. ObserveError

```text
ObserveError(
    ErrorObservation
)

→ DiagnosticObservationResult
```

---

# 87. ObservePerformance

```text
ObservePerformance(
    PerformanceObservation
)

→ DiagnosticObservationResult
```

---

# 88. Trace Instrumentation

Tracing may use language-friendly scoped APIs at implementation level.

The architecture only requires:

```text
stable trace semantic fields
correlation
privacy
backend independence
```

---

# 89. Module Lifecycle Boundary

Public queries should normally be available when Diagnostics is:

```text
READY
DEGRADED
```

subject to capability.

Submission may degrade gracefully.

Detailed lifecycle belongs to `STATES.md`.

---

# 90. DEGRADED Behavior

If one diagnostic capability fails:

```text
metrics unavailable
```

but:

```text
logs available
health available
```

Diagnostics may remain usable in DEGRADED mode.

---

# 91. Capability-Specific Failure

Queries should return capability-specific errors instead of claiming the entire Diagnostics module failed.

Example:

```text
GetTraceSummary
    ↓
TracingCapabilityUnavailable
```

while:

```text
GetDiagnosticHealth
```

may still succeed.

---

# 92. Thread Safety

Observation contracts must support concurrent producers.

Diagnostic implementations must not require global serialization of all business operations.

---

# 93. Bounded Resource Contract

Diagnostics infrastructure must enforce bounded:

```text
queue length
buffer bytes
recent-record retention
bundle size
trace sampling
```

Exact hard limits belong to Runtime/infrastructure configuration.

---

# 94. Overload Behavior

When diagnostic capacity is exceeded:

```text
drop low-priority observations
sample
aggregate
truncate
```

according to policy.

Do not block critical business processing indefinitely.

---

# 95. Sampling

Sampling may apply to:

```text
Trace
Debug log
high-volume metric observations
profiling
```

Sampling must not remove mandatory security/compliance information if such a policy exists.

---

# 96. Diagnostic Retention

Public contracts may expose retention class:

```text
DiagnosticRetentionClass
- Ephemeral
- Recent
- SupportEligible
```

Physical duration and storage backend remain infrastructure configuration.

---

# 97. Import/Export Boundary

Diagnostics has no general import semantics for diagnostic state in MVP.

Diagnostic replay, trace import, or offline bundle analysis are deferred.

---

# 98. Serialization

Public Diagnostics values must be serializable.

No public contract may contain:

```text
file handle
network connection
SDK tracer object
metric collector instance
callback closure
native profiler handle
```

---

# 99. Compatibility

Contract compatibility uses:

```text
MAJOR.MINOR.PATCH
```

Major version change required when:

* diagnostic ownership changes;
* required public fields change incompatibly;
* privacy semantics weaken/change;
* observation meaning changes;
* health aggregation semantics change incompatibly.

---

# 100. Event Version vs Contract Version

These are separate:

```text
Diagnostics ContractVersion
Diagnostics EventVersion
DiagnosticBundleVersion
```

Do not conflate them.

---

# 101. Example — Error Observation

```text
Translation
    ↓
TRN-PROV-003 ProviderTimeout
    ↓
Runtime completion
```

Separately:

```text
ObserveError
├── ownerModule = translation
├── originalErrorCode = TRN-PROV-003
├── diagnosticSeverity = Error
└── correlationContext
```

Diagnostics does not change Translation result.

---

# 102. Example — Capture Health

```text
Capture
    ↓
HealthObservation
├── ownerModule = capture
├── ownerHealthState = Degraded
└── reasonCode = ProviderLatency
```

Diagnostics aggregate:

```text
ComponentHealthSummary
```

Capture remains authoritative.

---

# 103. Example — Metrics

```text
Recognition
    ↓
ObserveMetric
metricName = recognition_operation_duration_ms
value = 82
unit = ms
```

No:

```text
MetricUpdated Event Bus event
```

is required.

---

# 104. Example — Tracing

```text
Runtime Attempt
    ↓
Trace span
        ↓
Capture child span
        ↓
Recognition child span
```

Trace relationships describe execution.

They do not create execution.

---

# 105. Example — Backend Failure

```text
ObserveMetric
    ↓
telemetry exporter unavailable
    ↓
DiagnosticObservationResult = Unavailable/DroppedByPolicy
```

Recognition/Translation/Capture operation remains unchanged.

---

# 106. Example — Diagnostic Bundle

```text
User requests support bundle
        ↓
ExportDiagnosticBundle
        ↓
collect bounded snapshot
        ↓
redact
        ↓
serialize
        ↓
BundleRef
```

No raw reading content is included.

---

# 107. Example — Query During Degradation

```text
Tracing unavailable
Metrics unavailable
Logs available
Health available
```

Then:

```text
GetDiagnosticHealth
    → succeeds

GetRecentLogs
    → succeeds

GetTraceSummary
    → DiagnosticCapabilityUnavailable
```

No global Diagnostics failure is required.

---

# 108. Architecture Invariants

1. Diagnostics public contracts remain backend-independent.

2. Diagnostics is passive.

3. Diagnostic submission does not mutate business state.

4. Diagnostic failure normally does not fail business operations.

5. Original module error ownership is preserved.

6. Original ErrorCode is preserved in ErrorObservation.

7. Health ownership remains with original modules.

8. Diagnostics only aggregates health.

9. Correlation identifiers do not transfer authority.

10. Metric semantics remain producer-owned.

11. Diagnostics may enforce metric naming/cardinality conventions.

12. Trace semantics remain observational.

13. Event Bus is not general telemetry transport.

14. `LogRecorded` is not required.

15. `MetricUpdated` is not required.

16. `TraceCompleted` is not required.

17. Generic `ErrorReported` Event Bus flow is not required.

18. Diagnostics has no mandatory business-event subscriptions.

19. Exporter vendors do not appear in stable public contracts.

20. Generic `Flush` is not a normal business command.

21. Explicit support bundle export is bounded.

22. Diagnostic queries are read-only.

23. Diagnostic records are structured.

24. Public records are serializable.

25. Raw user content is excluded by default.

26. Secrets are always forbidden.

27. Metric dimensions remain bounded.

28. Diagnostic buffering remains bounded.

29. Capability-specific degradation is supported.

30. One telemetry backend failure does not imply total Diagnostics failure.

31. Instrumentation API and business API remain conceptually separate.

32. Logging and telemetry transport remain infrastructure-owned.

---

# 109. Testing — Observation Contracts

Verify:

```text
ObserveLog
ObserveMetric
ObserveError
ObserveHealth
ObservePerformance
```

accept valid safe records and reject unsafe payloads.

---

# 110. Testing — Error Ownership

Verify:

```text
CaptureError
RecognitionError
TranslationError
RuntimeError
```

retain original owner and ErrorCode after diagnostic observation.

---

# 111. Testing — Privacy

Attempt to submit:

```text
raw screenshot
OCR text
token
credential
raw provider response
```

Verify safe rejection/redaction according to policy.

---

# 112. Testing — Correlation

Verify:

```text
SessionId
ReadingContextRevision
RuntimeRevisionId
WorkItemId
AttemptId
ArtifactId
```

may coexist in correlation metadata without type/authority confusion.

---

# 113. Testing — Health

Verify owner health remains authoritative while Diagnostics builds aggregate normalized health.

---

# 114. Testing — Backend Failure

Inject:

```text
logger sink failure
metric exporter failure
trace exporter failure
```

and verify producer business execution is unaffected.

---

# 115. Testing — Query Capability

Verify one unavailable capability does not disable unrelated Diagnostics queries.

---

# 116. Testing — Bounded Export

Verify support export:

```text
respects size limit
applies redaction
reports truncation
does not include secrets
does not include raw reading content
```

---

# 117. Testing — Event Independence

Diagnostics must function without subscriptions to:

```text
RecognitionCompleted
TranslationCompleted
ReadingSessionChanged
StorageFailed
ErrorOccurred
```

---

# 118. Related Documents

```text
doc/02-modules/diagnostics/MODULE.md
doc/02-modules/diagnostics/STATES.md
doc/02-modules/diagnostics/EVENTS.md
doc/02-modules/diagnostics/ERRORS.md
doc/02-modules/diagnostics/README.md

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md

doc/03-infrastructure/logging/
doc/03-infrastructure/telemetry/
```

---

# 119. Completion Criteria

This contract is synchronized when:

* diagnostics observations are passive;
* error ownership remains with producing modules;
* health ownership remains with producing modules;
* logging/metrics/traces use backend-neutral contracts;
* Event Bus is removed from the normal telemetry path;
* `LogRecorded`, `MetricUpdated`, and `TraceCompleted` are absent as mandatory events;
* mandatory business-event subscriptions are removed;
* explicit support-bundle export is separated from ongoing telemetry export;
* vendor-specific export targets are absent from stable contracts;
* `Flush` is moved to lifecycle/infrastructure control;
* diagnostic queries remain bounded/read-only;
* privacy and redaction semantics are explicit;
* correlation metadata remains non-authoritative;
* capability-specific degradation is supported.

---

# 120. Summary

Diagnostics v2 exposes an observational boundary:

```text
Producer Module
    ↓
Diagnostic Observation
    ↓
Diagnostics Semantics
    ↓
Logging / Telemetry Infrastructure
```

Query boundary:

```text
Application / UI / Developer Tools
    ↓
Diagnostics Query
    ↓
Diagnostic Snapshot / Health / Summary
```

Support export:

```text
Explicit Request
    ↓
Bounded Diagnostic Snapshot
    ↓
Redaction
    ↓
Diagnostic Bundle
```

The central contract rule is:

```text
Diagnostics observes.

It does not own
the business state,
error authority,
or execution
that it observes.
```
