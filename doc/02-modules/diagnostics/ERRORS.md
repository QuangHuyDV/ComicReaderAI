# Diagnostics Errors

> **Project:** CRAI
> **Module:** `diagnostics`
> **Path:** `doc/02-modules/diagnostics/ERRORS.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the Diagnostics-owned error model.

Diagnostics errors describe failures involving:

```text
diagnostic observation validation
diagnostic collection availability
diagnostic queries
health aggregation
diagnostic snapshots
privacy/redaction
diagnostic bundle export
diagnostic capability availability
Diagnostics-owned invariants
```

Diagnostics errors do not describe:

```text
Capture failures
Recognition failures
Translation failures
Reading Session failures
Runtime execution failures
Storage business/domain failures
logging backend implementation exceptions
telemetry SDK exceptions
provider-specific transport exceptions
```

Original domain and infrastructure ownership must remain intact.

---

# 2. Error Boundary

Canonical flow:

```text
Producer Module
    ↓
Diagnostic Observation / Query / Export
    ↓
Diagnostics
    ↓
Diagnostics-owned failure?
    ├── yes → DiagnosticError
    └── no
          ↓
      Logging / Telemetry Infrastructure
          ↓
      infrastructure-specific failure
          ↓
      normalized capability/coordination result
```

Diagnostics exposes stable public semantics.

Infrastructure-specific implementation details remain internal.

---

# 3. Error Principles

## 3.1 Passive Failure

Diagnostics failure must not normally alter business execution.

Example:

```text
Translation succeeds
    +
metric exporter unavailable
```

Result:

```text
Translation remains successful.
```

---

## 3.2 Stable Error Codes

Consumers depend on:

```text
ErrorCode
```

not human-readable error text.

---

## 3.3 Original Error Ownership

If Diagnostics observes:

```text
CAP-ACQ-003 ProviderTimeout
```

that error remains Capture-owned.

Diagnostics does not replace it with:

```text
DIAG-...
```

unless a separate Diagnostics operation itself failed.

---

## 3.4 Graceful Degradation

Where possible:

```text
one capability fails
    ↓
capability = DEGRADED / UNAVAILABLE
    ↓
Diagnostics remains partially usable
```

---

## 3.5 Fail Closed on Privacy

If Diagnostics cannot safely validate/redact diagnostic content:

```text
reject or omit the diagnostic output
```

Never export unsafe data.

---

# 4. Error Code Format

```text
DIAG-<CATEGORY>-<NUMBER>
```

Examples:

```text
DIAG-OBS-001
DIAG-QUERY-001
DIAG-HEALTH-001
DIAG-EXPORT-001
DIAG-PRIV-001
DIAG-CAP-001
DIAG-RES-001
DIAG-INT-001
```

---

# 5. Error Categories

Recommended categories:

| Prefix   | Category                            |
| -------- | ----------------------------------- |
| `OBS`    | Diagnostic Observation              |
| `QUERY`  | Diagnostic Query                    |
| `HEALTH` | Health Aggregation                  |
| `EXPORT` | Diagnostic Bundle Export            |
| `PRIV`   | Privacy / Redaction                 |
| `CAP`    | Diagnostic Capability               |
| `RES`    | Diagnostics-owned Resource Pressure |
| `CONFIG` | Diagnostics Configuration           |
| `INT`    | Diagnostics Internal Invariant      |

---

# 6. Severity

```text
Info
Warning
Error
Critical
```

Meaning:

| Severity   | Meaning                                               |
| ---------- | ----------------------------------------------------- |
| `Info`     | Expected diagnostic non-success outcome               |
| `Warning`  | Diagnostic capability reduced/rejected safely         |
| `Error`    | Diagnostics-owned operation failed                    |
| `Critical` | Core Diagnostics invariant/privacy boundary is unsafe |

Severity does not prescribe business action.

---

# 7. Recovery Classification

Recommended:

```text
RecoveryClassification
- NoAction
- CorrectInput
- RetryDiagnosticOperation
- RetryAfterCapabilityRecovery
- ReduceDiagnosticLoad
- DisableOptionalCapability
- ApplicationRecovery
```

This replaces the coarse v1:

```text
Recoverable
Non-Recoverable
```

---

# 8. DiagnosticError Contract

Conceptually:

```text
DiagnosticError
├── errorCode
├── category
├── severity
├── recoveryClassification
├── diagnosticCapability?
├── operation?
├── correlationContext?
├── diagnosticRef?
├── safeMessage?
└── safeMetadata?
```

---

# 9. Correlation Rule

DiagnosticError may carry:

```text
correlationId
traceId
sessionId?
runtimeRevisionId?
workItemId?
attemptId?
```

for correlation.

These values remain externally owned.

---

# 10. Observation Errors

## DIAG-OBS-001 — InvalidDiagnosticObservation

Meaning:

The submitted diagnostic observation violates the public Diagnostics contract.

Examples:

```text
missing producer
invalid signal type
malformed metric observation
invalid severity
invalid structured attributes
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

Business effect:

```text
None
```

---

# 11. DIAG-OBS-002 — UnsafeDiagnosticPayload

Meaning:

The observation contains data forbidden by Diagnostics privacy policy.

Examples:

```text
raw screenshot
OCR text
translation text
credential
token
cookie
provider response
```

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
```

The observation must be rejected or sanitized according to explicit policy.

---

# 12. DIAG-OBS-003 — UnsupportedDiagnosticSignal

Meaning:

The current runtime profile does not support the requested diagnostic signal type.

Example:

```text
profiling observation
when profiling capability is disabled
```

Severity:

```text
Info / Warning
```

Recovery:

```text
DisableOptionalCapability
```

---

# 13. DIAG-OBS-004 — DiagnosticObservationDropped

Meaning:

A valid observation could not be retained because bounded diagnostics policy intentionally dropped it.

Possible causes:

```text
sampling
buffer pressure
severity filtering
retention policy
```

Severity:

```text
Info
```

Recovery:

```text
NoAction
or
ReduceDiagnosticLoad
```

This may also be represented as a non-error observation result where appropriate.

---

# 14. Query Errors

## DIAG-QUERY-001 — DiagnosticQueryUnavailable

Meaning:

The requested Diagnostics query cannot currently be served.

Examples:

```text
recent-record projection unavailable
diagnostic store unavailable
query subsystem degraded
```

Severity:

```text
Warning
```

Recovery:

```text
RetryAfterCapabilityRecovery
```

---

# 15. DIAG-QUERY-002 — InvalidDiagnosticQuery

Meaning:

The query parameters are invalid.

Examples:

```text
negative limit
invalid time range
unsupported filter combination
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

# 16. DIAG-QUERY-003 — DiagnosticRecordNotFound

Meaning:

A requested retained diagnostic record or trace summary does not exist.

Severity:

```text
Info
```

Recovery:

```text
NoAction
```

This replaces backend-oriented concepts such as:

```text
TraceNotFound
```

with a public Diagnostics query semantic.

---

# 17. DIAG-QUERY-004 — DiagnosticDataExpired

Meaning:

The requested diagnostic data existed previously but is no longer retained under current policy.

Severity:

```text
Info
```

Recovery:

```text
NoAction
```

---

# 18. Health Errors

## DIAG-HEALTH-001 — HealthAggregationFailed

Meaning:

Diagnostics could not build a valid aggregate health projection.

Possible causes:

```text
inconsistent health observations
aggregation rule failure
required health projection unavailable
```

Severity:

```text
Error
```

Recovery:

```text
RetryDiagnosticOperation
```

Owner-specific module health remains authoritative.

---

# 19. DIAG-HEALTH-002 — InvalidHealthObservation

Meaning:

An owner module submitted an invalid HealthObservation.

Examples:

```text
missing owner
unknown normalized state
invalid component identity
malformed importance classification
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

Diagnostics must not overwrite the owner module's health.

---

# 20. DIAG-HEALTH-003 — HealthCapabilityUnavailable

Meaning:

Diagnostics health aggregation/query capability is unavailable.

Severity:

```text
Warning
```

Recovery:

```text
RetryAfterCapabilityRecovery
```

Other Diagnostics capabilities may remain usable.

---

# 21. Export Errors

## DIAG-EXPORT-001 — DiagnosticExportFailed

Meaning:

An explicit diagnostic bundle export could not complete.

Examples:

```text
snapshot assembly failed
serialization failed
bundle finalization failed
```

Severity:

```text
Error
```

Recovery:

```text
RetryDiagnosticOperation
```

A failed export does not mean the Diagnostics module failed globally.

---

# 22. DIAG-EXPORT-002 — InvalidDiagnosticExportRequest

Meaning:

The export request violates contract or policy.

Examples:

```text
invalid time range
invalid size limit
unsupported section
invalid purpose
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

# 23. DIAG-EXPORT-003 — DiagnosticExportCapabilityUnavailable

Meaning:

Support-bundle export capability is currently unavailable.

Severity:

```text
Warning
```

Recovery:

```text
RetryAfterCapabilityRecovery
```

---

# 24. DIAG-EXPORT-004 — DiagnosticBundleSizeLimitExceeded

Meaning:

Requested diagnostic export cannot fit within allowed hard limits.

Preferred handling:

```text
truncate safely
```

when contract allows.

Otherwise return this error.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
or
ReduceDiagnosticLoad
```

---

# 25. DIAG-EXPORT-005 — DiagnosticExportFinalizationFailed

Meaning:

A safe bundle was constructed but the implementation-neutral export result could not be finalized.

Severity:

```text
Error
```

Recovery:

```text
RetryDiagnosticOperation
```

Physical destination failures should remain infrastructure detail unless exposed through this normalized boundary.

---

# 26. Privacy Errors

## DIAG-PRIV-001 — DiagnosticRedactionFailed

Meaning:

Diagnostics could not guarantee that output was privacy-safe.

Severity:

```text
Critical
```

Recovery:

```text
ApplicationRecovery
```

Required behavior:

```text
fail closed
```

No unsafe export/query result may be returned.

---

# 27. DIAG-PRIV-002 — ForbiddenDiagnosticContent

Meaning:

Diagnostic content belongs to a forbidden class.

Examples:

```text
secret
credential
raw reading content
authentication token
private key
```

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
```

---

# 28. DIAG-PRIV-003 — RedactionProfileUnsupported

Meaning:

The requested redaction profile is unavailable or disallowed.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

# 29. DIAG-PRIV-004 — UnsafeDiagnosticMetadata

Meaning:

Diagnostic metadata contains unsafe or unbounded information.

Examples:

```text
full filesystem path
full sensitive URL
window title containing user content
large arbitrary nested payload
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

# 30. Capability Errors

## DIAG-CAP-001 — DiagnosticCapabilityUnavailable

Meaning:

A requested Diagnostics capability currently cannot operate.

Examples:

```text
Tracing unavailable
Metrics unavailable
RecentRecordQuery unavailable
Profiling unavailable
```

Severity:

```text
Warning
```

Recovery:

```text
RetryAfterCapabilityRecovery
```

---

# 31. DIAG-CAP-002 — DiagnosticCapabilityDisabled

Meaning:

A requested optional capability is intentionally disabled.

Severity:

```text
Info
```

Recovery:

```text
NoAction
```

This is not an infrastructure failure.

---

# 32. DIAG-CAP-003 — DiagnosticCapabilityDegraded

Meaning:

Capability exists but cannot provide its normal level of detail or reliability.

Examples:

```text
aggressive sampling
partial trace data
remote exporter unavailable
local fallback active
```

Severity:

```text
Warning
```

Recovery:

```text
RetryAfterCapabilityRecovery
```

---

# 33. DIAG-CAP-004 — DiagnosticCapabilityInitializationFailed

Meaning:

A Diagnostics capability could not initialize.

Severity:

```text
Warning / Error
```

Recovery:

```text
RetryAfterCapabilityRecovery
```

This replaces public backend-specific:

```text
CollectorInitializationFailed
```

---

# 34. Resource Errors

## DIAG-RES-001 — DiagnosticBufferPressure

Meaning:

Diagnostics-owned bounded buffering is under significant pressure.

Severity:

```text
Warning
```

Recovery:

```text
ReduceDiagnosticLoad
```

Preferred behavior:

```text
sample
aggregate
drop lower-priority observations
```

---

# 35. DIAG-RES-002 — DiagnosticBufferCapacityExceeded

Meaning:

A bounded diagnostic buffer reached its hard capacity.

Severity:

```text
Warning
```

Recovery:

```text
ReduceDiagnosticLoad
```

Business operations must not block indefinitely.

---

# 36. DIAG-RES-003 — DiagnosticRetentionLimitExceeded

Meaning:

Diagnostics cannot retain additional optional data within configured limits.

Severity:

```text
Warning
```

Recovery:

```text
ReduceDiagnosticLoad
```

Old data or low-priority data may be evicted according to policy.

---

# 37. DIAG-RES-004 — DiagnosticBundleResourceLimitExceeded

Meaning:

A bundle export exceeded Diagnostics-owned temporary resource budget.

Severity:

```text
Warning / Error
```

Recovery:

```text
ReduceDiagnosticLoad
```

---

# 38. Configuration Errors

## DIAG-CONFIG-001 — InvalidDiagnosticsConfiguration

Meaning:

Diagnostics configuration violates schema or invariant requirements.

Examples:

```text
invalid severity threshold
invalid sampling ratio
invalid buffer size
invalid export limit
invalid redaction profile
```

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
```

---

# 39. DIAG-CONFIG-002 — UnsupportedDiagnosticsConfiguration

Meaning:

The configuration is valid structurally but unsupported by this runtime profile.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

# 40. DIAG-CONFIG-003 — UnsafeDiagnosticsConfiguration

Meaning:

Configuration would weaken required privacy/safety policy.

Examples:

```text
disable mandatory redaction
allow secret export
unbounded buffer request
```

Severity:

```text
Critical
```

Recovery:

```text
CorrectInput
```

Configuration must be rejected.

---

# 41. Internal Errors

## DIAG-INT-001 — InternalDiagnosticsFailure

Meaning:

Unexpected Diagnostics-owned internal operation failure.

Severity:

```text
Error
```

Recovery:

```text
RetryDiagnosticOperation
or
ApplicationRecovery
```

depending on scope.

---

# 42. DIAG-INT-002 — DiagnosticInvariantViolation

Meaning:

A Diagnostics architectural invariant was violated.

Examples:

```text
unsafe payload escaped validation
capability state impossible
immutable snapshot mutated
error ownership lost
```

Severity:

```text
Critical
```

Recovery:

```text
ApplicationRecovery
```

---

# 43. DIAG-INT-003 — CorrelationInvariantViolation

Meaning:

Diagnostics detected inconsistent correlation metadata that violates type/ownership rules.

Examples:

```text
RuntimeRevision stored as ReadingContextRevision
malformed trace/span relationship
conflicting identifier types
```

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
or
ApplicationRecovery
```

This replaces the vague v1:

```text
CorrelationFailed
```

---

# 44. DIAG-INT-004 — DiagnosticSnapshotInvariantViolation

Meaning:

A generated diagnostic snapshot violates required snapshot invariants.

Examples:

```text
unbounded section
missing redaction summary
mutable result
unsafe backend object leaked
```

Severity:

```text
Critical
```

Recovery:

```text
ApplicationRecovery
```

---

# 45. Removed `LoggerUnavailable`

The v1 public error:

```text
LoggerUnavailable
```

is removed.

Reason:

a logger/sink is an infrastructure implementation.

Public Diagnostics should expose:

```text
DIAG-CAP-001 DiagnosticCapabilityUnavailable
```

with:

```text
capability = Logging
```

---

# 46. Removed `LogWriteFailed`

The v1:

```text
LogWriteFailed
```

is replaced by public semantics such as:

```text
DiagnosticObservationDropped
DiagnosticCapabilityDegraded
DiagnosticCapabilityUnavailable
```

depending on actual effect.

The sink-specific write failure remains internal infrastructure detail.

---

# 47. Removed `MetricCollectionFailed`

Prefer:

```text
DIAG-OBS-001 InvalidDiagnosticObservation
DIAG-CAP-003 DiagnosticCapabilityDegraded
DIAG-CAP-001 DiagnosticCapabilityUnavailable
```

depending on ownership.

---

# 48. Removed `InvalidMetric`

Malformed metric input becomes:

```text
DIAG-OBS-001 InvalidDiagnosticObservation
```

with:

```text
signalType = MetricObservation
```

This avoids creating a separate public error family for every telemetry signal type.

---

# 49. Removed `TraceStorageFailed`

Trace physical persistence belongs to telemetry infrastructure.

Public query/diagnostic effects may surface as:

```text
DIAG-CAP-003 DiagnosticCapabilityDegraded
DIAG-QUERY-001 DiagnosticQueryUnavailable
```

---

# 50. Removed `TraceNotFound`

Replaced by:

```text
DIAG-QUERY-003 DiagnosticRecordNotFound
```

when the public operation is querying retained diagnostic data.

---

# 51. Removed `HealthCheckFailed`

Diagnostics v2 does not own health checks for every component.

Modules own their health observations.

Diagnostics owns only:

```text
health aggregation
health query
```

Use:

```text
DIAG-HEALTH-001 HealthAggregationFailed
```

when that operation fails.

---

# 52. Removed `InvalidHealthStatus`

Replaced by:

```text
DIAG-HEALTH-002 InvalidHealthObservation
```

because Diagnostics observes owner-provided health rather than authoritatively setting it.

---

# 53. Removed Generic `ExportTargetUnavailable`

Destination/exporter reachability belongs to infrastructure.

Public Diagnostics exposes:

```text
DIAG-EXPORT-003 DiagnosticExportCapabilityUnavailable
```

when appropriate.

---

# 54. Removed `FlushFailed`

Flush is logging/telemetry infrastructure lifecycle coordination.

It is not a normal Diagnostics domain error.

Shutdown may record a bounded infrastructure flush outcome separately.

---

# 55. Removed Generic `BufferOverflow`

The term is too implementation-specific.

Use explicit bounded semantics:

```text
DiagnosticBufferPressure
DiagnosticBufferCapacityExceeded
```

and degrade gracefully.

---

# 56. Removed `CollectorInitializationFailed`

Collector is infrastructure terminology.

Use:

```text
DIAG-CAP-004 DiagnosticCapabilityInitializationFailed
```

with the affected capability.

---

# 57. Original Business Errors

Diagnostics may observe:

```text
CAP-...
REC-...
TRN-...
SES-...
PREF-...
RUN-...
```

but those codes remain unchanged.

Example:

```text
ObserveError
ownerModule = capture
originalErrorCode = CAP-ACQ-003
```

No DIAG error is generated merely because the error was observed.

---

# 58. Observation of Business Failure vs Diagnostics Failure

Example:

```text
Translation fails
    ↓
TRN-PROV-003
```

Diagnostics successfully records it:

```text
no DIAG error
```

If Diagnostics itself cannot record the observation due to unavailable telemetry capability:

```text
DIAG-CAP-001
```

may occur separately.

The two errors have different ownership.

---

# 59. Error-to-Module-State Mapping

| Condition                        | Diagnostics Effect                                           |
| -------------------------------- | ------------------------------------------------------------ |
| Invalid observation              | None                                                         |
| Unsafe observation               | None                                                         |
| Query parameter invalid          | None                                                         |
| One record dropped               | None                                                         |
| Logging capability unavailable   | Capability unavailable; module may degrade                   |
| Tracing capability unavailable   | Capability unavailable; module may degrade                   |
| Export operation failed          | Export operation failed; module may stay READY               |
| Buffer pressure                  | Collection PRESSURED/DEGRADED                                |
| Redaction failure                | Unsafe operation fails closed; module/capability may degrade |
| Core invariant violation         | DEGRADED or STOPPING                                         |
| Business module failure observed | No Diagnostics lifecycle change required                     |

---

# 60. No Universal Global Failure

A Diagnostics error does not automatically imply:

```text
DiagnosticsModuleState = FAILED
```

because v2 does not use global `FAILED` for ordinary capability failure.

Example:

```text
Tracing unavailable
    ↓
Tracing capability = UNAVAILABLE
    ↓
Diagnostics = DEGRADED
```

---

# 61. Capability-Specific Error Handling

A query should fail at the narrowest meaningful scope.

Example:

```text
Tracing unavailable
```

Then:

```text
GetTraceSummary
    → DIAG-CAP-001
```

while:

```text
GetDiagnosticHealth
    → succeeds
```

---

# 62. Backpressure Semantics

If Diagnostics reaches bounded capacity:

```text
producer business operation
    ↓
diagnostic observation
    ↓
capacity unavailable
    ↓
drop/sample according to policy
```

Do not:

```text
block business execution indefinitely
```

---

# 63. Retry Semantics

Diagnostics may advise:

```text
RetryDiagnosticOperation
RetryAfterCapabilityRecovery
```

but it does not own business Runtime retry.

For example:

```text
DIAG-EXPORT-001
```

may be retried as a diagnostic export operation.

That is unrelated to retrying Recognition or Translation.

---

# 64. No `ExportRetryScheduled`

Retry scheduling for diagnostic infrastructure belongs to infrastructure/scheduler policy.

Diagnostics may expose retry classification/hint only.

---

# 65. Logging Rules

Safe Diagnostics error logs may include:

```text
DIAG ErrorCode
operation
capability
severity
recovery classification
correlationId
traceId
bounded counts
bounded sizes
diagnosticRef
```

---

# 66. Logging Prohibitions

Never include:

```text
raw screenshot
OCR text
translation text
raw prompt
provider response
password
API key
token
cookie
private certificate
native backend object
```

---

# 67. DiagnosticRef

Detailed implementation exceptions may be stored behind:

```text
diagnosticRef
```

according to infrastructure/privacy policy.

Do not expose raw backend exceptions through the public contract.

---

# 68. Infrastructure Exception Normalization

Examples of internal exceptions:

```text
OpenTelemetry exporter exception
filesystem write failure
HTTP exporter connection failure
Prometheus registry exception
```

Public Diagnostics must normalize them to stable capability/operation semantics.

---

# 69. Metrics

Recommended Diagnostics-owned error metrics:

```text
diagnostics_error_total
diagnostics_observation_rejected_total
diagnostics_observation_dropped_total
diagnostics_capability_unavailable_total
diagnostics_query_failure_total
diagnostics_health_aggregation_failure_total
diagnostics_export_failure_total
diagnostics_redaction_failure_total
diagnostics_buffer_pressure_total
diagnostics_invariant_violation_total
```

---

# 70. Avoid Recursive Error Storms

Recording a Diagnostics error may itself require Diagnostics.

Implementations must prevent:

```text
diagnostic export fails
    ↓
log export failure
    ↓
logger fails
    ↓
log logger failure
    ↓
...
```

Recursion must remain bounded.

---

# 71. Recursion Protection

Possible strategies:

```text
internal emergency sink
recursion depth guard
dedicated low-level fallback
rate limiting
one-shot suppression
```

Exact implementation belongs to infrastructure.

---

# 72. Error Events

Normal Diagnostics errors are not automatically Event Bus events.

Primary path:

```text
Diagnostic operation
    ↓
DiagnosticError / result
```

Diagnostics-owned state changes may separately publish:

```text
DiagnosticCapabilityChanged
DiagnosticCollectionDegraded
DiagnosticHealthChanged
```

---

# 73. Event Publication Failure

If a Diagnostics state transition commits and its event publication fails:

```text
state remains authoritative
```

Do not convert the original state transition into failure solely because Event Bus publication failed.

---

# 74. Export Example

```text
ExportDiagnosticBundle
    ↓
collect
    ↓
redact
    ↓
serialization fails
    ↓
DIAG-EXPORT-001
DiagnosticExportFailed
```

Diagnostics module may remain READY.

---

# 75. Privacy Example

```text
ExportDiagnosticBundle
    ↓
raw OCR text detected
    ↓
redaction cannot safely guarantee removal
    ↓
DIAG-PRIV-001
DiagnosticRedactionFailed
    ↓
no bundle returned
```

Fail closed.

---

# 76. Capability Example

```text
GetTraceSummary
    ↓
Tracing capability = UNAVAILABLE
    ↓
DIAG-CAP-001
```

Metrics/logging queries remain unaffected.

---

# 77. Buffer Pressure Example

```text
diagnostic queue approaching capacity
    ↓
DIAG-RES-001
DiagnosticBufferPressure
    ↓
increase sampling / drop low-priority observations
    ↓
business operation continues
```

---

# 78. Invalid Metric Example

```text
ObserveMetric
metricName = invalid
value = malformed
    ↓
DIAG-OBS-001
InvalidDiagnosticObservation
```

No separate `InvalidMetric` public error required.

---

# 79. Business Error Observation Example

```text
Capture
    ↓
CAP-SRC-002 CaptureSourceUnavailable
```

Diagnostics records:

```text
ErrorObservation
ownerModule = capture
originalErrorCode = CAP-SRC-002
```

Result:

```text
no new DIAG error
```

unless diagnostic observation itself fails.

---

# 80. Trace Backend Example

```text
Tracing exporter loses network
    ↓
Tracing capability = DEGRADED / UNAVAILABLE
    ↓
DIAG-CAP-003 or DIAG-CAP-001
```

No public:

```text
TraceStorageFailed
```

is required.

---

# 81. Error Idempotency

Repeated equivalent invalid diagnostic requests do not mutate Diagnostics state.

Error identity does not require Event Bus publication.

---

# 82. Compatibility

Adding a new DIAG error code is generally backward-compatible.

Changing an existing code's meaning is not.

Backend-specific implementation errors may evolve without changing the public Diagnostics contract.

---

# 83. Architecture Invariants

1. Diagnostics errors represent Diagnostics-owned failures only.

2. Business errors preserve original owner and ErrorCode.

3. Infrastructure exceptions do not cross public boundaries directly.

4. Diagnostics failures normally do not alter business execution.

5. Diagnostics errors do not trigger business Runtime retry.

6. Capability failures are isolated.

7. One telemetry backend failure does not imply total Diagnostics failure.

8. Diagnostics does not use global FAILED for ordinary failures.

9. Privacy/redaction failure fails closed.

10. Unsafe diagnostic payloads are rejected.

11. Metric/log/trace-specific backend errors are normalized.

12. `LoggerUnavailable` is removed from public taxonomy.

13. `LogWriteFailed` is removed from public taxonomy.

14. `MetricCollectionFailed` is removed from public taxonomy.

15. `TraceStorageFailed` is removed from public taxonomy.

16. `HealthCheckFailed` is removed from public taxonomy.

17. `ExportTargetUnavailable` is removed from public taxonomy.

18. `FlushFailed` is removed from public taxonomy.

19. `CollectorInitializationFailed` is removed from public taxonomy.

20. Correlation metadata does not transfer authority.

21. Diagnostic buffers remain bounded.

22. Backpressure does not block business execution indefinitely.

23. Error payloads remain privacy-safe.

24. Event publication failure does not roll back committed Diagnostics state.

25. Diagnostic error recursion remains bounded.

---

# 84. Testing — Observation Errors

Test:

```text
invalid log observation
invalid metric observation
unsafe payload
unsupported signal type
```

Verify stable DIAG errors and no business mutation.

---

# 85. Testing — Capability Isolation

Disable tracing.

Verify:

```text
GetTraceSummary
    → DIAG-CAP-001
```

while:

```text
GetDiagnosticHealth
GetRecentLogs
```

remain available when supported.

---

# 86. Testing — Export

Inject:

```text
snapshot failure
serialization failure
bundle finalization failure
```

Verify export fails without moving Diagnostics into global FAILED.

---

# 87. Testing — Privacy

Inject:

```text
raw screenshot
OCR content
translation content
credential
token
```

Verify rejection/fail-closed behavior.

---

# 88. Testing — Buffer Pressure

Reach diagnostic capacity.

Verify:

```text
bounded dropping/sampling
collection degradation
business processing continues
```

---

# 89. Testing — Original Error Ownership

Observe errors from:

```text
Capture
Recognition
Translation
Reading Session
Runtime
Preferences
```

Verify original ErrorCode/owner remains unchanged.

---

# 90. Testing — Infrastructure Failure

Inject:

```text
log sink failure
metric exporter failure
trace exporter failure
```

Verify public error is normalized to capability/operation semantics.

---

# 91. Testing — Recursion

Cause Diagnostics' own error-reporting path to fail repeatedly.

Verify recursion guard prevents infinite error generation.

---

# 92. Related Documents

```text
doc/02-modules/diagnostics/MODULE.md
doc/02-modules/diagnostics/CONTRACT.md
doc/02-modules/diagnostics/STATES.md
doc/02-modules/diagnostics/EVENTS.md
doc/02-modules/diagnostics/README.md

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md

doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/03-infrastructure/logging/
doc/03-infrastructure/telemetry/
```

---

# 93. Completion Criteria

This error specification is synchronized when:

* only Diagnostics-owned failures use DIAG codes;
* business errors retain original ownership;
* backend exceptions are normalized;
* capability-specific degradation is explicit;
* coarse Recoverable/Non-Recoverable classification is replaced;
* global FAILED behavior is absent for ordinary failures;
* logger/metric/trace backend error names are removed from public taxonomy;
* health aggregation errors are distinct from owner health failures;
* support export errors are bounded and explicit;
* privacy/redaction failures fail closed;
* resource pressure is bounded;
* diagnostic errors do not trigger business retry;
* recursion protection is recognized;
* event publication failure preserves committed Diagnostics state.

---

# 94. Summary

Diagnostics error flow is:

```text
Diagnostics operation
    ↓
Diagnostics-owned failure
    ↓
DIAG Error
```

Business error observation is:

```text
Business Module
    ↓
Module-owned Error
    ↓
Diagnostics observes
    ↓
original ErrorCode preserved
```

Infrastructure failure is:

```text
Logging / Telemetry Backend
    ↓
implementation-specific failure
    ↓
Diagnostics normalization
    ↓
Capability / Operation Error
```

The central rule is:

```text
Diagnostics errors explain
why observability itself failed.

They never replace
the errors of the system
being observed.
```
