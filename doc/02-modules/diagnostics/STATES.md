# Diagnostics States

> **Project:** CRAI
> **Module:** `diagnostics`
> **Path:** `doc/02-modules/diagnostics/STATES.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the Diagnostics-owned state model.

It specifies:

```text
Diagnostics module lifecycle
diagnostic capability state
collection degradation
diagnostic export operation phases
controlled flush/shutdown phases
recovery
state invariants
failure isolation
```

This document does not define:

```text
business module state
Reading Session state
RuntimeRevision lifecycle
WorkItem lifecycle
Attempt lifecycle
Capture health semantics
Recognition health semantics
Translation health semantics
logging backend internal state
telemetry backend internal state
```

---

# 2. State Ownership

Diagnostics owns:

```text
DiagnosticsModuleState
DiagnosticCapabilityState
DiagnosticExportOperationPhase
DiagnosticCollectionStatus
```

Individual modules own:

```text
their own domain health
their own errors
their own operation states
```

Infrastructure owns:

```text
logger sink state
telemetry exporter state
metrics backend state
trace backend state
physical buffer state
transport lifecycle
```

Diagnostics may observe and normalize infrastructure capability state.

It does not become the infrastructure owner.

---

# 3. State Model Overview

Diagnostics v2 separates three concerns:

```text
Diagnostics
├── Module Lifecycle
├── Capability / Collection Status
└── Scoped Operations
    └── Diagnostic Bundle Export
```

There is no global module state for every:

```text
log record
metric observation
trace span
health evaluation
```

---

# 4. Module Lifecycle

Recommended lifecycle:

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

# 5. `UNINITIALIZED`

## Meaning

Diagnostics has not initialized its diagnostic abstractions and available infrastructure integrations.

Characteristics:

```text
no diagnostic capability snapshot available
no diagnostic queries guaranteed
no export operation accepted
```

## Allowed Next States

```text
INITIALIZING
STOPPED
```

---

# 6. `INITIALIZING`

## Meaning

Diagnostics is discovering and initializing available diagnostic capabilities.

Possible work:

```text
load typed diagnostic configuration
initialize diagnostic adapters
discover logging capability
discover metrics capability
discover tracing capability
initialize health aggregation
initialize bounded recent-record views
validate privacy/redaction policy
```

## Allowed Next States

```text
READY
DEGRADED
STOPPING
```

---

# 7. Initialization Success

Normal:

```text
UNINITIALIZED
    ↓
INITIALIZING
    ↓
required diagnostic semantics available
    ↓
READY
```

Not every optional capability must succeed.

Example:

```text
logs available
health available
metrics available
tracing unavailable
```

may still result in:

```text
DEGRADED
```

rather than startup failure.

---

# 8. `READY`

## Meaning

Diagnostics core capability is operational.

While READY:

```text
diagnostic observations may be accepted
health aggregation may run
diagnostic queries may run
bounded snapshots may be created
support bundle exports may be requested
```

Collection is normal READY behavior.

There is no transition to a separate `COLLECTING` state for every record.

---

# 9. READY Invariants

When READY:

```text
core diagnostic contracts valid
privacy/redaction rules active
required diagnostic views available
bounded resource policy active
```

Optional capability availability may vary.

---

# 10. `DEGRADED`

## Meaning

Diagnostics remains usable but one or more diagnostic capabilities are impaired.

Examples:

```text
trace exporter unavailable
metrics backend unavailable
log sink degraded
recent-log store unavailable
profiling unavailable
support bundle export unavailable
telemetry buffer pressure
```

Business execution continues.

---

# 11. DEGRADED Behavior

While DEGRADED, Diagnostics should preserve working capabilities.

Example:

```text
Tracing = Unavailable
Metrics = Degraded
Logs = Available
Health = Available
```

Then:

```text
GetDiagnosticHealth
    → available

GetRecentLogs
    → available

GetTraceSummary
    → capability unavailable
```

---

# 12. DEGRADED Is Not Business Failure

Diagnostics entering DEGRADED does not imply:

```text
Runtime degraded
Reading Session degraded
Capture degraded
Recognition degraded
Translation degraded
```

Those owners maintain their own health.

---

# 13. `STOPPING`

## Meaning

Diagnostics is shutting down.

Actions may include:

```text
reject new support exports
stop accepting optional diagnostic operations
complete bounded in-flight observation handling
request bounded infrastructure flush
release diagnostic views/resources
```

Shutdown must remain bounded.

---

# 14. `STOPPED`

## Meaning

Diagnostics has terminated.

Characteristics:

```text
no new observations accepted
no new queries guaranteed
no new export operations
diagnostic resources released
```

`STOPPED` is terminal.

---

# 15. Module Lifecycle Diagram

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

# 16. Removed Global `COLLECTING`

The v1 state:

```text
COLLECTING
```

is removed.

Reason:

recording logs, metrics, traces, and health observations is normal operation while Diagnostics is READY.

Invalid model:

```text
READY
    ↓
diagnostic observation
    ↓
COLLECTING
    ↓
READY
```

Preferred:

```text
Diagnostics = READY

ObserveDiagnostic
    ↓
bounded observation handling
```

No module lifecycle transition occurs.

---

# 17. Removed Global `MONITORING`

The v1 state:

```text
MONITORING
```

is removed.

Health aggregation is an ongoing/queryable diagnostic function, not a global lifecycle state.

Preferred:

```text
READY
    ↓
HealthObservation
    ↓
aggregate projection updated
```

Module remains READY.

---

# 18. Removed Global `EXPORTING`

Diagnostic bundle export is a scoped operation.

It does not transition the entire Diagnostics module into:

```text
EXPORTING
```

Other diagnostic operations should continue while one bundle is being exported.

---

# 19. Removed Global `FLUSHING`

Transport flushing belongs primarily to logging/telemetry infrastructure.

A controlled flush may happen during shutdown without turning the Diagnostics domain module into a global:

```text
FLUSHING
```

state.

---

# 20. Removed Global `FAILED`

The v1 state:

```text
FAILED
```

is removed from the normal module lifecycle.

Reason:

one diagnostic backend failure rarely means all Diagnostics capability is unusable.

Prefer:

```text
DEGRADED
```

when partial observability remains.

---

# 21. Catastrophic Diagnostics Failure

If Diagnostics cannot safely enforce its own core contracts, such as:

```text
redaction policy unavailable
diagnostic record invariants corrupted
all diagnostic capability initialization invalid
```

the module may:

```text
fail closed for Diagnostics operations
        ↓
DEGRADED
or
STOPPING
```

depending on application policy.

Business processing still normally continues.

---

# 22. Diagnostic Capability State

Each diagnostic capability may expose an independent state.

Recommended:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
DISABLED
UNKNOWN
```

---

# 23. Capability Examples

Capabilities may include:

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

# 24. `AVAILABLE`

Capability exists and currently operates normally.

---

# 25. `DEGRADED`

Capability operates with reduced quality.

Examples:

```text
sampling increased
some records dropped
remote exporter offline but local buffer available
partial trace data
```

---

# 26. `UNAVAILABLE`

Capability is configured/supported but currently cannot operate.

Example:

```text
trace exporter unavailable
```

---

# 27. `DISABLED`

Capability is intentionally disabled by configuration/policy.

Example:

```text
profiling disabled
remote telemetry disabled
```

This is not an error.

---

# 28. `UNKNOWN`

Diagnostics does not currently know capability state.

Useful during initialization or uncertain infrastructure recovery.

---

# 29. Capability State vs Module State

Example:

```text
Logging = AVAILABLE
Metrics = AVAILABLE
Tracing = UNAVAILABLE
Health = AVAILABLE
```

Possible module state:

```text
DEGRADED
```

not:

```text
FAILED
```

---

# 30. DiagnosticCollectionStatus

Diagnostics may expose aggregate collection status:

```text
NORMAL
PRESSURED
DEGRADED
UNAVAILABLE
```

This is a diagnostic projection.

It is not a business execution state.

---

# 31. `NORMAL`

Diagnostic input rate is within configured bounds.

---

# 32. `PRESSURED`

Diagnostic buffers or exporters are approaching limits.

Possible responses:

```text
sampling
aggregation
dropping Trace/Debug
reducing optional metadata
```

---

# 33. `DEGRADED` Collection

Some diagnostic observations are being dropped or reduced.

Higher-priority diagnostics should be preserved according to infrastructure policy.

---

# 34. `UNAVAILABLE` Collection

Current diagnostic submission path is unavailable.

Producer business execution still continues unless explicit compliance policy says otherwise.

---

# 35. Backpressure Transition

Example:

```text
NORMAL
   ↓
buffer pressure
   ↓
PRESSURED
   ↓
capacity exceeded
   ↓
DEGRADED
```

Recovery:

```text
DEGRADED
    ↓
exporter recovers
    ↓
PRESSURED
    ↓
NORMAL
```

---

# 36. Backpressure Does Not Block Business Execution

Invalid:

```text
Diagnostics buffer full
    ↓
Translation waits indefinitely
```

Preferred:

```text
Diagnostics buffer full
    ↓
drop/sample according to policy
    ↓
Translation continues
```

---

# 37. Diagnostic Export Operation

Support/debug bundle export is modeled as a scoped operation.

Recommended phases:

```text
VALIDATING
COLLECTING
REDACTING
SERIALIZING
FINALIZING
FINISHED
```

Possible outcomes:

```text
COMPLETED
COMPLETED_WITH_TRUNCATION
REJECTED
FAILED
ABORTED
```

---

# 38. Export `VALIDATING`

Validate:

```text
request
requested sections
time range
bundle size limit
redaction profile
capability availability
security policy
```

No export state is externally committed yet.

---

# 39. Export `COLLECTING`

Collect bounded diagnostic data.

Examples:

```text
recent logs
metric summary
trace summary
health snapshot
environment summary
recent issues
```

Collection must respect the export request and hard limits.

---

# 40. Export `REDACTING`

Apply privacy policy.

Actions may include:

```text
remove forbidden fields
redact sensitive metadata
exclude unsafe records
summarize sensitive sections
```

---

# 41. Export `SERIALIZING`

Build a backend-independent diagnostic bundle representation.

No vendor-specific object should escape the operation.

---

# 42. Export `FINALIZING`

Create:

```text
BundleRef
Manifest
RedactionSummary
TruncationSummary
```

and complete the operation.

---

# 43. Export `COMPLETED`

Bundle was produced within requested/hard limits.

Diagnostics module remains:

```text
READY
or
DEGRADED
```

according to capability health.

---

# 44. `COMPLETED_WITH_TRUNCATION`

A valid bundle was produced, but some data was omitted because of:

```text
size limit
retention limit
availability
privacy policy
```

This is a successful bounded outcome.

---

# 45. Export `REJECTED`

Request violates contract/policy.

Examples:

```text
invalid time range
unsupported section
unsafe redaction request
bundle limit invalid
```

No module lifecycle change.

---

# 46. Export `FAILED`

Export operation failed.

Examples:

```text
snapshot construction failure
redaction failure
serialization failure
bundle finalization failure
```

Important:

```text
Export FAILED
≠
Diagnostics Module FAILED
```

---

# 47. Export Failure Effect

One failed export normally results in:

```text
operation = FAILED
module = READY or DEGRADED
```

If failure reveals broader capability impairment:

```text
SupportBundleExport capability
    → DEGRADED / UNAVAILABLE
```

---

# 48. Concurrent Export

MVP may limit:

```text
maximumConcurrentDiagnosticExports = 1
```

for resource control.

A second request may be rejected or queued by Application/Runtime policy.

Diagnostics does not require a global Exporting module state.

---

# 49. Diagnostic Snapshot Operation

Creating:

```text
DiagnosticSnapshot
```

is read-only.

It does not require a module state transition.

---

# 50. Health Aggregation Operation

Computing:

```text
DiagnosticHealthSnapshot
```

does not require:

```text
MONITORING
```

state.

It reads current owner-provided observations/projections.

---

# 51. Observation Processing

Processing:

```text
ObserveLog
ObserveMetric
ObserveError
ObserveHealth
ObservePerformance
```

does not change Diagnostics module lifecycle under normal operation.

---

# 52. Observation Rejection

If an observation contains unsafe data:

```text
ObserveDiagnostic
    ↓
RejectedUnsafe
```

Diagnostics remains operational.

No transition to DEGRADED is required unless repeated violations reveal a systemic producer/instrumentation defect.

---

# 53. Sampling Exclusion

```text
SamplingExcluded
```

is a normal observation outcome.

It is not an error state.

---

# 54. DroppedByPolicy

```text
DroppedByPolicy
```

may occur because of:

```text
sampling
cardinality policy
buffer pressure
retention rules
severity filtering
```

It does not imply module failure.

---

# 55. Logging Backend Failure

Example:

```text
Logging capability AVAILABLE
        ↓
sink failure
        ↓
Logging capability DEGRADED / UNAVAILABLE
```

Diagnostics module may become:

```text
DEGRADED
```

Business modules continue.

---

# 56. Metrics Backend Failure

```text
Metrics capability
    ↓
backend/exporter unavailable
    ↓
DEGRADED / UNAVAILABLE
```

Other diagnostic capabilities remain independent.

---

# 57. Trace Backend Failure

```text
Tracing capability
    ↓
exporter failure
    ↓
DEGRADED / UNAVAILABLE
```

No global Diagnostics failure required.

---

# 58. Health Aggregation Failure

If Diagnostics cannot build an aggregate health view:

```text
HealthAggregation capability
    ↓
DEGRADED / UNAVAILABLE
```

Owner-specific module health remains valid.

---

# 59. Recovery

Capability recovery:

```text
UNAVAILABLE
    ↓
infrastructure restored
    ↓
DEGRADED
    ↓
validation succeeds
    ↓
AVAILABLE
```

Diagnostics module may transition:

```text
DEGRADED → READY
```

when required health criteria are satisfied.

---

# 60. Recovery Does Not Restart Business Work

Diagnostics recovery does not:

```text
retry Translation
restart Recognition
recreate CaptureSource
resume Runtime Attempt
```

---

# 61. Controlled Flush During Shutdown

Shutdown may request bounded infrastructure flush:

```text
STOPPING
    ↓
request flush
    ↓
wait until deadline/budget
    ↓
release resources
    ↓
STOPPED
```

Flush failure must not make shutdown hang indefinitely.

---

# 62. Flush Outcome

Possible infrastructure outcome:

```text
Completed
Partial
TimedOut
Failed
```

Diagnostics shutdown continues according to configured bounded policy.

These are infrastructure/lifecycle coordination outcomes, not new module states.

---

# 63. Shutdown with Telemetry Loss

Example:

```text
STOPPING
    ↓
remote exporter unavailable
    ↓
bounded flush fails
    ↓
record safe diagnostic if possible
    ↓
STOPPED
```

The application must still be able to terminate.

---

# 64. Diagnostics-Owned Events

State facts may include:

```text
DiagnosticHealthChanged
DiagnosticCollectionDegraded
DiagnosticCollectionRecovered
DiagnosticExportCompleted
```

Detailed contracts belong to `EVENTS.md`.

---

# 65. Event Timing

State events follow:

```text
state/capability transition committed
    ↓
event published
```

Event publication failure does not revert the committed diagnostic state.

---

# 66. No Collection Events as State Commands

Do not use:

```text
DiagnosticEventReceived
    ↓
Collecting
```

as a module transition.

Observation arrival is data flow, not lifecycle transition.

---

# 67. No Health Evaluation Request Transition

Do not use:

```text
HealthEvaluationRequested
    ↓
Monitoring
```

Health queries/aggregation are read operations.

---

# 68. No ExportRequested Global Transition

Do not use:

```text
READY
    ↓
ExportRequested
    ↓
EXPORTING
```

Export has its own scoped operation state.

---

# 69. Error-to-State Mapping

| Condition                          | Module/Capability Effect                               |
| ---------------------------------- | ------------------------------------------------------ |
| Invalid diagnostic observation     | None                                                   |
| Unsafe diagnostic payload          | None                                                   |
| Sampling exclusion                 | None                                                   |
| One metric observation dropped     | None                                                   |
| Log sink unavailable               | Logging capability degraded/unavailable                |
| Metrics exporter unavailable       | Metrics capability degraded/unavailable                |
| Trace exporter unavailable         | Tracing capability degraded/unavailable                |
| Support bundle export failed       | Export operation failed; export capability may degrade |
| Health aggregation failed          | Health capability degraded                             |
| All optional exporters down        | Module may be DEGRADED                                 |
| Core diagnostic contract corrupted | DEGRADED or STOPPING                                   |
| Shutdown requested                 | STOPPING                                               |

---

# 70. Module State Aggregation

Module state should be calculated from required capability availability.

Example policy:

```text
all required capabilities available
    → READY

one or more required capabilities degraded
but core Diagnostics usable
    → DEGRADED

core Diagnostics unsafe/unusable
    → STOPPING / unavailable by application policy
```

---

# 71. Optional Capability Failure

Optional feature failure should not necessarily change module state.

Example:

```text
Profiling = UNAVAILABLE
```

while:

```text
Logs = AVAILABLE
Metrics = AVAILABLE
Health = AVAILABLE
```

may still permit:

```text
READY
```

depending on profile.

---

# 72. Capability Importance

Capabilities may be classified:

```text
Required
Degradable
Optional
```

This classification affects Diagnostics module-health aggregation only.

It does not control business pipelines.

---

# 73. State Snapshot

Diagnostics may expose:

```text
DiagnosticsStateSnapshot
├── moduleState
├── collectionStatus
├── capabilityStates[]
├── activeDegradations[]
├── activeExportOperations?
└── observedAt
```

This snapshot is diagnostic.

---

# 74. Capability State Snapshot

```text
DiagnosticCapabilityStateSnapshot
├── capability
├── state
├── reasonCode?
├── lastSuccessAt?
├── lastFailureAt?
└── observedAt
```

---

# 75. No Backend Object in State

State snapshots must not contain:

```text
logger object
exporter instance
HTTP client
OpenTelemetry tracer
Prometheus registry
file handle
database connection
```

---

# 76. Passive State Rule

Diagnostics state transitions must never directly mutate observed business components.

Invalid:

```text
Tracing → Unavailable
    ↓
cancel Runtime Attempt
```

Correct:

```text
Tracing → Unavailable
    ↓
Diagnostics DEGRADED
```

---

# 77. Business Health Observation

Suppose:

```text
CaptureHealth = Unavailable
```

Diagnostics may update its aggregate health projection.

It must not transition:

```text
CaptureSourceState
```

itself.

---

# 78. Runtime Health Observation

Suppose Runtime reports:

```text
scheduler pressure
```

Diagnostics may show:

```text
Runtime component = Degraded
```

in its health snapshot.

Diagnostics must not change Runtime queue policy.

---

# 79. State Persistence

Diagnostics module lifecycle state generally does not require durable persistence.

Historical diagnostic records may be retained by infrastructure according to policy.

Capability state may be recomputed after initialization.

---

# 80. Restart

A new application process/runtime lifecycle reinitializes Diagnostics through:

```text
UNINITIALIZED
    ↓
INITIALIZING
```

There is no:

```text
STOPPED → READY
```

transition on the same module instance.

---

# 81. Initialization Partial Failure

Example:

```text
logging initializes
metrics initializes
tracing fails
health initializes
```

Possible result:

```text
INITIALIZING
    ↓
DEGRADED
```

not:

```text
FAILED
```

---

# 82. Recovery from Initialization Degradation

```text
DEGRADED
    ↓
tracing infrastructure recovers
    ↓
capability validation
    ↓
READY
```

---

# 83. Resource Pressure

Diagnostic resource pressure affects collection state/capabilities.

It does not own global Runtime resource policy.

Examples:

```text
diagnostic ring buffer full
export queue full
bundle memory budget reached
```

Responses must remain bounded.

---

# 84. Diagnostic Memory Pressure

Preferred behavior:

```text
pressure
    ↓
drop/summarize lower-priority diagnostics
    ↓
collection = PRESSURED / DEGRADED
```

not:

```text
allocate indefinitely
```

---

# 85. Trace Sampling Under Pressure

Tracing may transition from:

```text
AVAILABLE
```

to:

```text
DEGRADED
```

when sampling is aggressively reduced due to resource pressure.

---

# 86. Metrics Aggregation Under Pressure

Metrics may remain:

```text
AVAILABLE
```

while raw high-frequency observations are aggregated before export.

Internal implementation is infrastructure-owned.

---

# 87. Privacy Failure

If redaction cannot be safely applied to a support export:

```text
Export operation
    ↓
FAILED / REJECTED
```

Fail closed.

Do not export unsafe content.

---

# 88. Core Redaction Failure

If Diagnostics cannot safely enforce required redaction across its public query/export boundary:

```text
Redaction capability
    ↓
UNAVAILABLE
```

Depending on importance:

```text
Diagnostics → DEGRADED
```

and unsafe export/query capabilities must be disabled.

---

# 89. Event Publication Failure

If:

```text
DiagnosticCollectionDegraded
```

state is committed but event publication fails:

```text
collection remains DEGRADED
```

Infrastructure handles publication recovery.

---

# 90. Testing — Module Lifecycle

Verify:

```text
UNINITIALIZED → INITIALIZING
INITIALIZING → READY
INITIALIZING → DEGRADED
READY ↔ DEGRADED
READY → STOPPING → STOPPED
DEGRADED → STOPPING → STOPPED
```

---

# 91. Testing — No Collection State

Verify repeated:

```text
ObserveLog
ObserveMetric
ObserveError
ObserveHealth
```

does not cause module lifecycle transitions from READY.

---

# 92. Testing — Capability Isolation

Inject tracing failure.

Verify:

```text
Tracing = UNAVAILABLE
Logs remain AVAILABLE
Metrics remain AVAILABLE
business execution unaffected
```

---

# 93. Testing — Export Isolation

During one support export:

```text
ObserveMetric
GetDiagnosticHealth
GetRecentDiagnosticIssues
```

should continue according to capability/resource policy.

No global EXPORTING state should block them.

---

# 94. Testing — Export Failure

Inject serialization failure.

Verify:

```text
Export operation = FAILED
Diagnostics module != FAILED
```

---

# 95. Testing — Flush Failure

Inject exporter flush failure during shutdown.

Verify Diagnostics reaches:

```text
STOPPED
```

within bounded shutdown policy.

---

# 96. Testing — Backpressure

Fill diagnostic buffer.

Verify:

```text
NORMAL → PRESSURED → DEGRADED
```

or equivalent bounded behavior.

Verify business processing does not block indefinitely.

---

# 97. Testing — Recovery

Recover telemetry backend and verify:

```text
Capability UNAVAILABLE
    ↓
DEGRADED
    ↓
AVAILABLE
```

and module returns to READY when appropriate.

---

# 98. Testing — Health Authority

Provide Capture/Runtime/Storage health observations.

Verify Diagnostics:

* aggregates them;
* preserves owner identity;
* does not mutate owner state.

---

# 99. Testing — Privacy Failure

Inject unsafe diagnostic export content.

Verify:

```text
export rejected/failed closed
no unsafe bundle produced
module remains operational
```

---

# 100. Removed v1 State Concepts

Removed:

```text
Collecting
Monitoring
Exporting
Flushing
Failed
Shutdown
```

as v1 global state vocabulary.

Replaced by:

```text
READY normal collection
CapabilityState
DiagnosticCollectionStatus
Scoped ExportOperationPhase
STOPPING / STOPPED
DEGRADED
```

`Shutdown` is represented as lifecycle transition:

```text
STOPPING → STOPPED
```

rather than an ambiguous active state.

---

# 101. Architecture Invariants

1. Diagnostics module lifecycle is small.

2. Normal diagnostic collection occurs while READY.

3. Observation arrival does not change module lifecycle.

4. Health evaluation does not change module lifecycle by itself.

5. Export is a scoped operation.

6. Flush is infrastructure/lifecycle coordination.

7. One exporter failure does not imply total Diagnostics failure.

8. Partial capability loss is represented through capability state.

9. DEGRADED preserves working diagnostic capabilities.

10. Diagnostic failure does not change business state.

11. Business module health remains owner-defined.

12. Diagnostics health aggregation is observational.

13. Capability state is distinct from owner module health.

14. Backpressure is bounded.

15. Diagnostic pressure must not cause unbounded memory growth.

16. Low-priority diagnostics may be dropped/sampled under policy.

17. Support bundle export is bounded.

18. Export failure does not corrupt diagnostic state.

19. Privacy failure fails closed.

20. Event publication failure does not roll back committed Diagnostics state.

21. STOPPED is terminal.

22. Shutdown is bounded.

23. Backend objects never appear in public state snapshots.

24. Infrastructure failures remain isolated from business execution.

---

# 102. Related Documents

```text
doc/02-modules/diagnostics/MODULE.md
doc/02-modules/diagnostics/CONTRACT.md
doc/02-modules/diagnostics/EVENTS.md
doc/02-modules/diagnostics/ERRORS.md
doc/02-modules/diagnostics/README.md

doc/01-architecture/core/STATE_MACHINE.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/03-infrastructure/logging/
doc/03-infrastructure/telemetry/
```

---

# 103. Completion Criteria

This specification is synchronized when:

* module lifecycle is reduced to initialization/readiness/degradation/shutdown;
* `Collecting` is removed as global state;
* `Monitoring` is removed as global state;
* `Exporting` is replaced with scoped export phases;
* `Flushing` is moved to infrastructure/lifecycle coordination;
* global `Failed` is removed from normal failure handling;
* capability-specific state is explicit;
* one capability failure does not disable unrelated diagnostics;
* collection backpressure is bounded;
* support export is isolated from observation collection;
* export failure does not cause total module failure;
* privacy failure fails closed;
* owner module health remains authoritative;
* shutdown remains bounded.

---

# 104. Summary

Diagnostics v2 module lifecycle is:

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

Capability state is independent:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
DISABLED
UNKNOWN
```

Collection status is:

```text
NORMAL
PRESSURED
DEGRADED
UNAVAILABLE
```

Support export is scoped:

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

with:

```text
COMPLETED_WITH_TRUNCATION
REJECTED
FAILED
ABORTED
```

as operation outcomes.

The central state rule is:

```text
Diagnostics lifecycle describes
whether Diagnostics is usable.

Capability state describes
which diagnostic features are usable.

Scoped operation state describes
what one diagnostic operation is doing.

These state domains must not be merged.
```
