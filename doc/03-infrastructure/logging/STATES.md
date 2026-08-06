# Logging States

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Logging  
> **Document:** State Machines  
> **Path:** `03-infrastructure/logging/STATES.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/logging/MODULE.md`
> - `03-infrastructure/logging/CONTRACT.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`
> - `03-infrastructure/secret-management/CONTRACT.md`
> - `03-infrastructure/secret-management/ERRORS.md`
> - `03-infrastructure/event-bus/CONTRACT.md`
> - `03-infrastructure/event-bus/STATES.md`
> - `03-infrastructure/event-bus/ERRORS.md`

---

## 1. Purpose

This document defines lifecycle states and valid transitions owned by the Logging infrastructure module.

It covers:

- Logging module lifecycle;
- logging-policy lifecycle;
- log-record lifecycle;
- redaction and safety-inspection lifecycle;
- log-scope lifecycle;
- buffer lifecycle;
- buffer-admission lifecycle;
- sink lifecycle;
- sink-health lifecycle;
- write-batch lifecycle;
- formatter lifecycle;
- log-file lifecycle;
- rotation lifecycle;
- flush lifecycle;
- retention-cleanup lifecycle;
- compression lifecycle;
- diagnostics-query lifecycle;
- diagnostics-export lifecycle;
- audit-write lifecycle;
- bootstrap-logger lifecycle;
- emergency-logger lifecycle;
- shutdown and bounded-drain behavior;
- concurrency and crash-recovery rules;
- invalid transitions;
- cross-state invariants.

This document does not define:

- log-record field schemas;
- logger method signatures;
- exact file format;
- exact queue implementation;
- detailed Logging self-events;
- normalized error codes;
- concrete operating-system file APIs;
- audit retention implementation;
- Telemetry state machines.

---

## 2. State Ownership

Logging owns lifecycle state for:

```text
LoggingInstance
LoggingPolicy
LogRecord
RecordSafetyInspection
LogScope
LogBuffer
LogAdmission
LogSink
LogSinkHealth
LogWriteBatch
LogFormatter
LogFile
LogRotation
LogFlush
RetentionCleanup
LogCompression
LoggingDiagnosticsQuery
DiagnosticsExport
AuditWrite
BootstrapLogger
EmergencyLogger
```

Logging does not own lifecycle state for:

```text
ConfigurationSnapshot
SecretDescriptor
EventBus
RuntimeWorkItem
ProviderDefinition
TranslationJob
RecognitionJob
ReadingSession
TraceSpan
MetricSeries
DomainAggregate
```

External modules may trigger Logging operations.

They do not mutate Logging state directly.

---

## 3. State-Machine Separation

The Logging module must not use one global state enumeration.

Independent state machines are required:

```text
LoggingState
LogPolicyState
LogRecordState
RecordSafetyState
LogScopeState
LogBufferState
LogAdmissionState
LogSinkState
LogSinkHealthState
LogWriteBatchState
LogFormatterState
LogFileState
LogRotationState
LogFlushState
RetentionCleanupState
LogCompressionState
LoggingDiagnosticsQueryState
DiagnosticsExportState
AuditWriteState
BootstrapLoggerState
EmergencyLoggerState
```

This separation is necessary because:

- Logging may be `RUNNING` while one sink is `DEGRADED`;
- a record may be `REDACTED` while its target sink is `ROTATING`;
- one buffer may be `BACKPRESSURED` while the security buffer remains `AVAILABLE`;
- a write batch may be `PARTIALLY_WRITTEN` while Logging remains healthy;
- a file may be `ACTIVE` while retention cleanup is `FAILED`;
- diagnostics export may be `BLOCKED_UNSAFE` without affecting normal logging;
- an audit write may fail closed while ordinary logs continue;
- bootstrap logging may be `HANDED_OFF` while emergency logging remains `AVAILABLE`.

---

## 4. State Principles

### 4.1 State represents accepted current truth

```text
State
    = current lifecycle condition

Event
    = immutable fact that a transition occurred
```

### 4.2 Record admission and sink persistence are separate

```text
LogRecord ACCEPTED
    ≠
Record persisted by every sink
```

### 4.3 Safety precedes normal admission

A record cannot enter a normal buffer before safety inspection completes.

### 4.4 Record immutability begins after admission

Once admitted, a `LogRecord` cannot be mutated.

Formatting creates sink-specific output.

### 4.5 Sink failure is isolated

One sink may degrade or fail without changing unrelated sink state.

### 4.6 Restricted classification is preserved

No state transition may route restricted or audit data into a weaker sink.

### 4.7 Shutdown is bounded

Flush, drain, rotation, and sink shutdown must have finite deadlines.

### 4.8 Terminal states do not reactivate

Terminal record, batch, flush, export, and audit-write states remain terminal.

### 4.9 Late completion is non-authoritative

A sink or handler that completes after timeout does not rewrite a committed terminal outcome.

### 4.10 Safety failure is fail-closed

If safe redaction or classification cannot be established, the record is blocked.

---

# Part I — Logging Lifecycle

## 5. LoggingState

Canonical states:

```text
CREATED
BOOTSTRAPPING
INITIALIZING
READY
RUNNING
DEGRADED
QUIESCING
DRAINING
FLUSHING
STOPPING
TERMINATED
FAILED
```

Primary lifecycle:

```text
CREATED
    ↓
BOOTSTRAPPING
    ↓
INITIALIZING
    ↓
READY
    ↓
RUNNING
    ↓
QUIESCING
    ↓
DRAINING
    ↓
FLUSHING
    ↓
STOPPING
    ↓
TERMINATED
```

Alternative operational path:

```text
RUNNING ↔ DEGRADED
DEGRADED → QUIESCING
ANY ACTIVE STATE → FAILED
FAILED → STOPPING
```

---

## 6. CREATED

The Logging instance exists but no bootstrap or normal pipeline is active.

Valid outgoing transitions:

```text
CREATED → BOOTSTRAPPING
CREATED → INITIALIZING
CREATED → TERMINATED
```

`CREATED → INITIALIZING` is allowed only when no bootstrap logger is required.

---

## 7. BOOTSTRAPPING

A minimal synchronous logger is available for startup failures.

Properties:

- local;
- bounded;
- payload-safe;
- minimal schema;
- no normal async buffer;
- no Event Bus dependency.

Valid outgoing transitions:

```text
BOOTSTRAPPING → INITIALIZING
BOOTSTRAPPING → FAILED
BOOTSTRAPPING → STOPPING
```

---

## 8. INITIALIZING

Logging initializes:

- policy;
- redaction;
- safety inspector;
- buffers;
- sink registry;
- formatters;
- file locations;
- rotation;
- retention;
- diagnostics;
- audit adapter;
- emergency path.

Valid outgoing transitions:

```text
INITIALIZING → READY
INITIALIZING → DEGRADED
INITIALIZING → FAILED
INITIALIZING → STOPPING
```

`INITIALIZING → DEGRADED` is allowed only when safe core logging remains available.

---

## 9. READY

The pipeline is initialized but has not started normal asynchronous intake.

At this point:

- policy is active;
- required buffers exist;
- mandatory sink readiness is known;
- bootstrap handoff may be pending;
- emergency path is available.

Valid outgoing transitions:

```text
READY → RUNNING
READY → DEGRADED
READY → STOPPING
READY → FAILED
```

---

## 10. RUNNING

Logging accepts records according to policy.

Properties:

- normal buffers active;
- sink router active;
- sink writes active;
- redaction active;
- diagnostics available;
- rotation and retention scheduled;
- bootstrap logger handed off or inactive.

Valid outgoing transitions:

```text
RUNNING → DEGRADED
RUNNING → QUIESCING
RUNNING → FAILED
```

---

## 11. DEGRADED

Logging remains partially operational.

Possible causes:

- one optional sink unavailable;
- buffer pressure;
- retention cleanup failure;
- compression unavailable;
- diagnostics sink unavailable;
- remote sink unavailable;
- restricted sink degraded but approved emergency fallback exists;
- formatter fallback active.

Properties:

- safety inspection must remain available;
- unsafe records remain blocked;
- degraded capabilities are explicit;
- mandatory security/audit failure may instead require `FAILED`.

Valid outgoing transitions:

```text
DEGRADED → RUNNING
DEGRADED → QUIESCING
DEGRADED → FAILED
```

---

## 12. QUIESCING

Logging reduces admission for shutdown or controlled maintenance.

Typical policy:

```text
accept:
    WARNING
    ERROR
    CRITICAL
    FATAL
    SECURITY
    AUDIT

sample or reject:
    TRACE
    DEBUG
    INFO
    PERFORMANCE
```

Valid outgoing transitions:

```text
QUIESCING → DRAINING
QUIESCING → FLUSHING
QUIESCING → STOPPING
QUIESCING → FAILED
```

---

## 13. DRAINING

Accepted buffered records are read and routed to sinks.

Properties:

- low-severity new intake restricted;
- buffer order preserved where configured;
- critical reserve drains first or by policy;
- deadline is finite.

Valid outgoing transitions:

```text
DRAINING → FLUSHING
DRAINING → STOPPING
DRAINING → FAILED
```

---

## 14. FLUSHING

Logging asks sinks to flush accepted writes.

Properties:

- bounded deadline;
- security and audit sinks prioritized;
- timeout and partial flush are explicit;
- no state rollback for already written records.

Valid outgoing transitions:

```text
FLUSHING → STOPPING
FLUSHING → DEGRADED
FLUSHING → FAILED
```

During shutdown, `FLUSHING → DEGRADED` normally proceeds immediately to `STOPPING`.

---

## 15. STOPPING

Logging closes:

- sink workers;
- file handles;
- buffers;
- diagnostics;
- bootstrap path;
- emergency path where appropriate.

Valid outgoing transitions:

```text
STOPPING → TERMINATED
STOPPING → FAILED
```

---

## 16. TERMINATED

The Logging instance no longer accepts records.

A minimal terminal status query may remain available.

`TERMINATED` is terminal.

---

## 17. FAILED

Logging cannot maintain required safety or lifecycle invariants.

Examples:

- redaction unavailable;
- unsafe data cannot be blocked;
- restricted data would fall back to unrestricted sink;
- buffer invariant corrupted;
- all approved critical sinks unavailable under fail-closed policy;
- audit requirement cannot be enforced.

Required behavior:

- block unsafe admission;
- preserve emergency safe reporting where possible;
- expose critical status;
- move toward stop.

Valid outgoing transitions:

```text
FAILED → STOPPING
FAILED → TERMINATED
```

Direct `FAILED → RUNNING` is prohibited.

---

# Part II — Logging Policy Lifecycle

## 18. LogPolicyState

Canonical states:

```text
DRAFT
VALIDATING
READY
ACTIVE
UPDATING
SUPERSEDED
REJECTED
INVALID
```

---

## 19. DRAFT

A policy candidate exists but is not validated.

---

## 20. VALIDATING

Validation covers:

- severity rules;
- classification;
- sink routing;
- bounded retention;
- sampling exclusions;
- suppression exclusions;
- redaction requirements;
- audit behavior;
- fallback safety.

Valid outgoing transitions:

```text
VALIDATING → READY
VALIDATING → REJECTED
VALIDATING → INVALID
```

---

## 21. READY

The policy is valid and may be activated.

Valid outgoing transitions:

```text
READY → ACTIVE
READY → REJECTED
```

---

## 22. ACTIVE

The policy governs new records.

Exactly one policy revision should be active per Logging instance.

Valid outgoing transitions:

```text
ACTIVE → UPDATING
ACTIVE → SUPERSEDED
ACTIVE → INVALID
```

---

## 23. UPDATING

A new candidate is being prepared while the current active policy remains authoritative.

Valid outgoing transitions:

```text
UPDATING → ACTIVE
UPDATING → SUPERSEDED
UPDATING → INVALID
```

The old policy remains active until the new policy commits.

---

## 24. SUPERSEDED

A newer policy is active.

`SUPERSEDED` is terminal for the old revision.

---

## 25. REJECTED

The candidate was not activated.

Terminal.

---

## 26. INVALID

The policy cannot safely route or classify records.

Normal logging must stop or remain on the last known good active policy.

Terminal for that policy revision.

---

# Part III — Log Record Lifecycle

## 27. LogRecordState

Canonical states:

```text
DRAFT
FILTERING
ENRICHING
NORMALIZING_EXCEPTION
INSPECTING_SAFETY
REDACTING
READY
ADMITTING
ACCEPTED
FILTERED
SAMPLED_OUT
SUPPRESSED
REJECTED_UNSAFE
REJECTED_CAPACITY
TIMED_OUT
EMERGENCY_WRITTEN
FAILED_SAFE
```

---

## 28. DRAFT

The producer has created a mutable draft.

Valid outgoing transition:

```text
DRAFT → FILTERING
```

---

## 29. FILTERING

Severity, category, module, and policy rules are evaluated.

Valid outgoing transitions:

```text
FILTERING → ENRICHING
FILTERING → FILTERED
FILTERING → SAMPLED_OUT
FILTERING → SUPPRESSED
FILTERING → FAILED_SAFE
```

Safety-critical records may bypass ordinary sampling and suppression.

---

## 30. ENRICHING

Safe context and identities are added.

Valid outgoing transitions:

```text
ENRICHING → NORMALIZING_EXCEPTION
ENRICHING → INSPECTING_SAFETY
ENRICHING → FAILED_SAFE
```

---

## 31. NORMALIZING_EXCEPTION

A raw exception is converted into a safe summary.

Valid outgoing transitions:

```text
NORMALIZING_EXCEPTION → INSPECTING_SAFETY
NORMALIZING_EXCEPTION → REJECTED_UNSAFE
NORMALIZING_EXCEPTION → FAILED_SAFE
```

---

## 32. INSPECTING_SAFETY

The draft is inspected for:

- secrets;
- user content;
- unsafe metadata;
- large binary data;
- unsafe paths;
- unsafe URIs;
- unsupported types;
- classification conflicts.

Valid outgoing transitions:

```text
INSPECTING_SAFETY → REDACTING
INSPECTING_SAFETY → READY
INSPECTING_SAFETY → REJECTED_UNSAFE
INSPECTING_SAFETY → FAILED_SAFE
```

---

## 33. REDACTING

Approved transformations are applied.

Valid outgoing transitions:

```text
REDACTING → READY
REDACTING → REJECTED_UNSAFE
REDACTING → FAILED_SAFE
```

---

## 34. READY

The record is safe and immutable-ready.

Valid outgoing transitions:

```text
READY → ADMITTING
READY → EMERGENCY_WRITTEN
```

---

## 35. ADMITTING

The record attempts bounded buffer admission.

Valid outgoing transitions:

```text
ADMITTING → ACCEPTED
ADMITTING → REJECTED_CAPACITY
ADMITTING → TIMED_OUT
ADMITTING → EMERGENCY_WRITTEN
ADMITTING → FAILED_SAFE
```

---

## 36. ACCEPTED

The immutable record entered the Logging pipeline.

`ACCEPTED` is terminal for record-admission lifecycle.

Sink writes continue in separate batch states.

---

## 37. FILTERED

Policy disabled the record before expensive processing.

Terminal.

---

## 38. SAMPLED_OUT

Sampling intentionally removed the record.

Terminal.

---

## 39. SUPPRESSED

Duplicate suppression removed the independent record.

A later suppression summary may be produced.

Terminal.

---

## 40. REJECTED_UNSAFE

The record was blocked due to safety policy.

Terminal.

The original unsafe content must not be retained.

---

## 41. REJECTED_CAPACITY

Buffer admission failed due to bounded capacity.

Terminal.

Critical records may instead use the emergency path.

---

## 42. TIMED_OUT

Admission exceeded its deadline.

Terminal.

---

## 43. EMERGENCY_WRITTEN

The record bypassed the normal buffer and was written through the emergency path.

Terminal.

---

## 44. FAILED_SAFE

Logging failed to process the record but blocked it safely.

Terminal.

---

# Part IV — Safety Inspection Lifecycle

## 45. RecordSafetyState

Canonical states:

```text
NOT_STARTED
INSPECTING
SAFE
REDACTION_REQUIRED
REDACTING
SAFE_REDACTED
BLOCKED
FAILED_SAFE
```

---

## 46. NOT_STARTED

No inspection has occurred.

---

## 47. INSPECTING

The safety inspector evaluates the complete draft.

Valid outgoing transitions:

```text
INSPECTING → SAFE
INSPECTING → REDACTION_REQUIRED
INSPECTING → BLOCKED
INSPECTING → FAILED_SAFE
```

---

## 48. SAFE

No prohibited data was found.

Terminal for that inspection pass.

---

## 49. REDACTION_REQUIRED

Approved transformations are necessary.

Valid outgoing transition:

```text
REDACTION_REQUIRED → REDACTING
```

---

## 50. REDACTING

Transformations execute.

Valid outgoing transitions:

```text
REDACTING → SAFE_REDACTED
REDACTING → BLOCKED
REDACTING → FAILED_SAFE
```

---

## 51. SAFE_REDACTED

The record is safe after transformation.

Terminal.

---

## 52. BLOCKED

Safety policy rejected the record.

Terminal.

---

## 53. FAILED_SAFE

The inspector or redactor failed, and the record was blocked.

Terminal.

---

# Part V — Log Scope Lifecycle

## 54. LogScopeState

Canonical states:

```text
CREATED
ACTIVE
DISPOSING
DISPOSED
LEAKED
INVALID
```

---

## 55. CREATED

A scope exists but is not yet bound to execution context.

Valid outgoing transitions:

```text
CREATED → ACTIVE
CREATED → INVALID
```

---

## 56. ACTIVE

The scope contributes context to records.

Valid outgoing transitions:

```text
ACTIVE → DISPOSING
ACTIVE → LEAKED
```

---

## 57. DISPOSING

Context restoration and cleanup are in progress.

Valid outgoing transition:

```text
DISPOSING → DISPOSED
```

---

## 58. DISPOSED

The scope is no longer active.

Terminal.

Repeated dispose is idempotent.

---

## 59. LEAKED

The scope remained active beyond its expected execution boundary.

Terminal for that scope instance.

A restricted diagnostic may be produced.

---

## 60. INVALID

The scope contained unsafe or malformed context.

Terminal.

---

# Part VI — Log Buffer Lifecycle

## 61. LogBufferState

Canonical states:

```text
CREATED
INITIALIZING
AVAILABLE
BACKPRESSURED
PAUSED
DRAINING
CLEARING
STOPPING
TERMINATED
FAILED
```

---

## 62. CREATED

Buffer resources are defined but unavailable.

---

## 63. INITIALIZING

Capacity, reserve, and internal structures initialize.

Valid outgoing transitions:

```text
INITIALIZING → AVAILABLE
INITIALIZING → FAILED
```

---

## 64. AVAILABLE

The buffer accepts eligible records.

Valid outgoing transitions:

```text
AVAILABLE → BACKPRESSURED
AVAILABLE → PAUSED
AVAILABLE → DRAINING
AVAILABLE → CLEARING
AVAILABLE → STOPPING
AVAILABLE → FAILED
```

---

## 65. BACKPRESSURED

Utilization exceeds a configured threshold.

Possible effects:

- low-level sampling;
- duplicate suppression;
- bounded producer wait;
- reserve usage;
- low-severity dropping.

Valid outgoing transitions:

```text
BACKPRESSURED → AVAILABLE
BACKPRESSURED → DRAINING
BACKPRESSURED → FAILED
```

---

## 66. PAUSED

The buffer does not admit normal records.

Existing records remain.

Valid outgoing transitions:

```text
PAUSED → AVAILABLE
PAUSED → DRAINING
PAUSED → CLEARING
PAUSED → STOPPING
```

---

## 67. DRAINING

No normal new admission.

Records are read until empty or deadline.

Valid outgoing transitions:

```text
DRAINING → AVAILABLE
DRAINING → STOPPING
DRAINING → FAILED
```

During shutdown, it proceeds to `STOPPING`.

---

## 68. CLEARING

Records are removed under an explicit policy.

Examples:

- test cleanup;
- privacy cleanup;
- shutdown cleanup;
- diagnostic-buffer reset.

Valid outgoing transitions:

```text
CLEARING → AVAILABLE
CLEARING → STOPPING
CLEARING → FAILED
```

---

## 69. STOPPING

Internal workers and resources close.

Valid outgoing transition:

```text
STOPPING → TERMINATED
```

---

## 70. TERMINATED

The buffer cannot be reused.

Terminal.

---

## 71. FAILED

Capacity or ordering invariants cannot be trusted.

Terminal for the buffer instance.

Logging may degrade, use emergency path, or fail depending on buffer class.

---

# Part VII — Buffer Admission Lifecycle

## 72. LogAdmissionState

Canonical states:

```text
CREATED
CHECKING_POLICY
CHECKING_CAPACITY
USING_RESERVE
WAITING_FOR_CAPACITY
ADMITTED
FILTERED
SAMPLED_OUT
SUPPRESSED
DROPPED_LOW_SEVERITY
REJECTED_CAPACITY
TIMED_OUT
BUFFER_NOT_RUNNING
```

---

## 73. CREATED

An admission request exists.

---

## 74. CHECKING_POLICY

Overflow, severity, and buffer-class rules are evaluated.

Valid outgoing transitions:

```text
CHECKING_POLICY → CHECKING_CAPACITY
CHECKING_POLICY → FILTERED
CHECKING_POLICY → SAMPLED_OUT
CHECKING_POLICY → SUPPRESSED
```

---

## 75. CHECKING_CAPACITY

The buffer checks record and byte capacity.

Valid outgoing transitions:

```text
CHECKING_CAPACITY → ADMITTED
CHECKING_CAPACITY → USING_RESERVE
CHECKING_CAPACITY → WAITING_FOR_CAPACITY
CHECKING_CAPACITY → DROPPED_LOW_SEVERITY
CHECKING_CAPACITY → REJECTED_CAPACITY
CHECKING_CAPACITY → BUFFER_NOT_RUNNING
```

---

## 76. USING_RESERVE

The record uses critical reserve.

Valid outgoing transitions:

```text
USING_RESERVE → ADMITTED
USING_RESERVE → REJECTED_CAPACITY
```

---

## 77. WAITING_FOR_CAPACITY

The producer waits within a bounded deadline.

Valid outgoing transitions:

```text
WAITING_FOR_CAPACITY → ADMITTED
WAITING_FOR_CAPACITY → TIMED_OUT
WAITING_FOR_CAPACITY → BUFFER_NOT_RUNNING
```

---

## 78. Admission Terminal States

```text
ADMITTED
FILTERED
SAMPLED_OUT
SUPPRESSED
DROPPED_LOW_SEVERITY
REJECTED_CAPACITY
TIMED_OUT
BUFFER_NOT_RUNNING
```

These are terminal for one admission attempt.

---

# Part VIII — Sink Lifecycle

## 79. LogSinkState

Canonical states:

```text
UNREGISTERED
REGISTERED
INITIALIZING
AVAILABLE
DEGRADED
UNAVAILABLE
ROTATING
FLUSHING
DRAINING
STOPPING
TERMINATED
FAILED
```

---

## 80. UNREGISTERED

The sink is unknown to the active Logging instance.

---

## 81. REGISTERED

The sink descriptor is accepted but not initialized.

Valid outgoing transitions:

```text
REGISTERED → INITIALIZING
REGISTERED → TERMINATED
```

---

## 82. INITIALIZING

The sink validates:

- destination;
- permissions;
- formatter;
- retention;
- classification support;
- file state;
- network or platform availability.

Valid outgoing transitions:

```text
INITIALIZING → AVAILABLE
INITIALIZING → DEGRADED
INITIALIZING → UNAVAILABLE
INITIALIZING → FAILED
```

---

## 83. AVAILABLE

The sink can accept eligible batches.

Valid outgoing transitions:

```text
AVAILABLE → DEGRADED
AVAILABLE → UNAVAILABLE
AVAILABLE → ROTATING
AVAILABLE → FLUSHING
AVAILABLE → DRAINING
AVAILABLE → STOPPING
AVAILABLE → FAILED
```

---

## 84. DEGRADED

The sink remains partially usable.

Possible causes:

- slow writes;
- compression unavailable;
- retention cleanup failed;
- partial write support;
- reduced batching;
- temporary permission warning;
- remote path intermittent.

Valid outgoing transitions:

```text
DEGRADED → AVAILABLE
DEGRADED → UNAVAILABLE
DEGRADED → ROTATING
DEGRADED → FLUSHING
DEGRADED → DRAINING
DEGRADED → STOPPING
DEGRADED → FAILED
```

---

## 85. UNAVAILABLE

The sink cannot currently accept writes.

Valid outgoing transitions:

```text
UNAVAILABLE → INITIALIZING
UNAVAILABLE → AVAILABLE
UNAVAILABLE → DEGRADED
UNAVAILABLE → STOPPING
UNAVAILABLE → FAILED
```

---

## 86. ROTATING

File or storage rotation is in progress.

Properties:

- writes may buffer;
- some sinks may continue to a temporary active file;
- classification remains preserved;
- deadline is bounded.

Valid outgoing transitions:

```text
ROTATING → AVAILABLE
ROTATING → DEGRADED
ROTATING → UNAVAILABLE
ROTATING → FAILED
```

---

## 87. FLUSHING

Pending writes are being flushed.

Valid outgoing transitions:

```text
FLUSHING → AVAILABLE
FLUSHING → DEGRADED
FLUSHING → UNAVAILABLE
FLUSHING → STOPPING
FLUSHING → FAILED
```

---

## 88. DRAINING

The sink accepts no new batch from normal routing.

Existing work completes within a deadline.

Valid outgoing transitions:

```text
DRAINING → FLUSHING
DRAINING → STOPPING
DRAINING → FAILED
```

---

## 89. STOPPING

The sink closes resources.

Valid outgoing transitions:

```text
STOPPING → TERMINATED
STOPPING → FAILED
```

---

## 90. TERMINATED

The sink cannot be reactivated.

Terminal.

---

## 91. FAILED

The sink cannot preserve required write or safety invariants.

Terminal for that sink instance.

---

# Part IX — Sink Health Lifecycle

## 92. LogSinkHealthState

Canonical states:

```text
UNKNOWN
HEALTHY
SLOW
DEGRADED
FAILING
UNHEALTHY
RECOVERING
DISABLED
```

Health is derived from:

- write latency;
- write failures;
- flush failures;
- rotation failures;
- permission errors;
- capacity;
- persistence confirmation;
- retention failure.

---

## 93. UNKNOWN

Insufficient observations exist.

---

## 94. HEALTHY

The sink operates within policy.

---

## 95. SLOW

Latency exceeds warning threshold.

Valid outgoing transitions:

```text
SLOW → HEALTHY
SLOW → DEGRADED
SLOW → FAILING
```

---

## 96. DEGRADED

The sink remains usable with limitations.

Valid outgoing transitions:

```text
DEGRADED → HEALTHY
DEGRADED → FAILING
DEGRADED → UNHEALTHY
```

---

## 97. FAILING

Failure rate exceeds threshold.

Valid outgoing transitions:

```text
FAILING → RECOVERING
FAILING → UNHEALTHY
FAILING → DISABLED
```

---

## 98. UNHEALTHY

The sink should not receive normal writes.

Valid outgoing transitions:

```text
UNHEALTHY → RECOVERING
UNHEALTHY → DISABLED
```

---

## 99. RECOVERING

Controlled probes or reinitialization occur.

Valid outgoing transitions:

```text
RECOVERING → HEALTHY
RECOVERING → DEGRADED
RECOVERING → UNHEALTHY
```

---

## 100. DISABLED

The sink is intentionally excluded.

Recovery requires explicit reactivation.

---

# Part X — Write Batch Lifecycle

## 101. LogWriteBatchState

Canonical states:

```text
CREATED
FORMATTING
READY
WRITING
PARTIALLY_WRITTEN
WRITTEN
RETRY_WAIT
REJECTED
TIMED_OUT
FAILED
CANCELED
ABANDONED
```

---

## 102. CREATED

A batch of immutable records exists for one sink.

---

## 103. FORMATTING

Records are converted to sink format.

Valid outgoing transitions:

```text
FORMATTING → READY
FORMATTING → REJECTED
FORMATTING → FAILED
```

---

## 104. READY

The batch is formatted and eligible for write.

Valid outgoing transitions:

```text
READY → WRITING
READY → CANCELED
```

---

## 105. WRITING

The sink performs the write.

Valid outgoing transitions:

```text
WRITING → WRITTEN
WRITING → PARTIALLY_WRITTEN
WRITING → RETRY_WAIT
WRITING → TIMED_OUT
WRITING → FAILED
WRITING → CANCELED
WRITING → ABANDONED
```

---

## 106. PARTIALLY_WRITTEN

Some records were written, others were not.

Terminal for that batch unless a new retry batch is created for known unwritten records.

The original batch does not return to `WRITING`.

---

## 107. WRITTEN

The sink accepted and wrote the batch.

Terminal.

Persistence confirmation is metadata, not a separate lifecycle transition.

---

## 108. RETRY_WAIT

A bounded idempotent sink retry is scheduled.

Valid outgoing transitions:

```text
RETRY_WAIT → WRITING
RETRY_WAIT → FAILED
RETRY_WAIT → CANCELED
```

---

## 109. Batch Terminal States

```text
PARTIALLY_WRITTEN
WRITTEN
REJECTED
TIMED_OUT
FAILED
CANCELED
ABANDONED
```

Late write completion cannot overwrite a terminal state.

---

# Part XI — Formatter Lifecycle

## 110. LogFormatterState

Canonical states:

```text
UNREGISTERED
REGISTERED
INITIALIZING
AVAILABLE
DEGRADED
DISABLED
FAILED
TERMINATED
```

---

## 111. AVAILABLE

The formatter can safely produce sink output.

---

## 112. DEGRADED

The formatter falls back to a reduced safe representation.

Example:

- stack trace omitted;
- optional fields removed;
- structured text used instead of richer output.

---

## 113. DISABLED

No formatting requests are accepted.

---

## 114. FAILED

The formatter cannot guarantee safe output.

The affected sink must reject writes or use an approved safe fallback formatter.

---

# Part XII — Log File Lifecycle

## 115. LogFileState

Canonical states:

```text
PLANNED
CREATING
ACTIVE
ROLLING
FINALIZING
SEALED
COMPRESSING
COMPRESSED
RETENTION_ELIGIBLE
DELETING
DELETED
CORRUPTED
ORPHANED
FAILED
```

---

## 116. PLANNED

A logical file identity and naming decision exist.

---

## 117. CREATING

The sink creates the file and applies permissions.

Valid outgoing transitions:

```text
CREATING → ACTIVE
CREATING → FAILED
```

---

## 118. ACTIVE

The file accepts writes.

Valid outgoing transitions:

```text
ACTIVE → ROLLING
ACTIVE → FINALIZING
ACTIVE → CORRUPTED
ACTIVE → FAILED
```

---

## 119. ROLLING

The file is transitioning out of active use.

Valid outgoing transitions:

```text
ROLLING → FINALIZING
ROLLING → FAILED
```

---

## 120. FINALIZING

The file is flushed, closed, and renamed or finalized.

Valid outgoing transitions:

```text
FINALIZING → SEALED
FINALIZING → ORPHANED
FINALIZING → FAILED
```

---

## 121. SEALED

The file is immutable for normal writes.

Valid outgoing transitions:

```text
SEALED → COMPRESSING
SEALED → RETENTION_ELIGIBLE
SEALED → CORRUPTED
```

---

## 122. COMPRESSING

Compression is in progress.

Valid outgoing transitions:

```text
COMPRESSING → COMPRESSED
COMPRESSING → RETENTION_ELIGIBLE
COMPRESSING → FAILED
```

Compression failure must not delete the sealed source file.

---

## 123. COMPRESSED

A compressed immutable archive exists.

Valid outgoing transition:

```text
COMPRESSED → RETENTION_ELIGIBLE
```

---

## 124. RETENTION_ELIGIBLE

Retention policy may remove the file.

Valid outgoing transitions:

```text
RETENTION_ELIGIBLE → DELETING
RETENTION_ELIGIBLE → CORRUPTED
```

---

## 125. DELETING

File deletion is in progress.

Valid outgoing transitions:

```text
DELETING → DELETED
DELETING → FAILED
```

---

## 126. DELETED

The file is no longer available.

Terminal.

---

## 127. CORRUPTED

The file cannot be trusted or parsed safely.

Possible actions:

- isolate;
- rename;
- preserve for restricted diagnostics;
- create a new active file.

Valid outgoing transitions:

```text
CORRUPTED → RETENTION_ELIGIBLE
CORRUPTED → DELETING
CORRUPTED → FAILED
```

---

## 128. ORPHANED

The file exists but is not registered in the expected active lifecycle.

Examples:

- crash during rotation;
- rename completed but metadata update lost;
- temporary file remained.

Valid outgoing transitions:

```text
ORPHANED → SEALED
ORPHANED → RETENTION_ELIGIBLE
ORPHANED → DELETING
ORPHANED → FAILED
```

---

## 129. FAILED

The file lifecycle cannot safely continue.

Terminal for that file operation path.

---

# Part XIII — Rotation Lifecycle

## 130. LogRotationState

Canonical states:

```text
REQUESTED
VALIDATING
FLUSHING_ACTIVE_FILE
CLOSING_ACTIVE_FILE
FINALIZING_OLD_FILE
CREATING_NEW_FILE
ACTIVATING_NEW_FILE
SCHEDULING_RETENTION
COMPLETED
PARTIALLY_COMPLETED
NOT_REQUIRED
TIMED_OUT
FAILED
CANCELED
UNCERTAIN
RECONCILING
```

---

## 131. REQUESTED

A rotation request exists.

---

## 132. VALIDATING

Checks:

- sink supports rotation;
- active file known;
- naming safe;
- path approved;
- flush possible;
- deadline valid.

Valid outgoing transitions:

```text
VALIDATING → FLUSHING_ACTIVE_FILE
VALIDATING → NOT_REQUIRED
VALIDATING → FAILED
VALIDATING → CANCELED
```

---

## 133. FLUSHING_ACTIVE_FILE

Pending records are flushed.

Valid outgoing transitions:

```text
FLUSHING_ACTIVE_FILE → CLOSING_ACTIVE_FILE
FLUSHING_ACTIVE_FILE → TIMED_OUT
FLUSHING_ACTIVE_FILE → FAILED
```

---

## 134. CLOSING_ACTIVE_FILE

The active handle closes.

Valid outgoing transitions:

```text
CLOSING_ACTIVE_FILE → FINALIZING_OLD_FILE
CLOSING_ACTIVE_FILE → UNCERTAIN
CLOSING_ACTIVE_FILE → FAILED
```

---

## 135. FINALIZING_OLD_FILE

The old file is renamed or sealed.

Valid outgoing transitions:

```text
FINALIZING_OLD_FILE → CREATING_NEW_FILE
FINALIZING_OLD_FILE → UNCERTAIN
FINALIZING_OLD_FILE → FAILED
```

---

## 136. CREATING_NEW_FILE

A new active file is created.

Valid outgoing transitions:

```text
CREATING_NEW_FILE → ACTIVATING_NEW_FILE
CREATING_NEW_FILE → PARTIALLY_COMPLETED
CREATING_NEW_FILE → FAILED
```

If the old file is already sealed but the new file cannot be created, Logging may use emergency fallback.

---

## 137. ACTIVATING_NEW_FILE

The sink atomically switches normal writes to the new file.

Valid outgoing transitions:

```text
ACTIVATING_NEW_FILE → SCHEDULING_RETENTION
ACTIVATING_NEW_FILE → UNCERTAIN
ACTIVATING_NEW_FILE → FAILED
```

---

## 138. SCHEDULING_RETENTION

Compression and retention cleanup are scheduled.

Valid outgoing transitions:

```text
SCHEDULING_RETENTION → COMPLETED
SCHEDULING_RETENTION → PARTIALLY_COMPLETED
```

---

## 139. Rotation Terminal States

```text
COMPLETED
PARTIALLY_COMPLETED
NOT_REQUIRED
TIMED_OUT
FAILED
CANCELED
```

`UNCERTAIN` requires `RECONCILING`.

---

## 140. UNCERTAIN

The system cannot determine which file is active or whether finalization committed.

Valid outgoing transition:

```text
UNCERTAIN → RECONCILING
```

Normal file writes should use a safe emergency path or pause until active-file authority is resolved.

---

## 141. RECONCILING

The system inspects:

- open handles;
- file existence;
- expected active name;
- sealed files;
- temporary files;
- last committed file sequence.

Valid outgoing transitions:

```text
RECONCILING → COMPLETED
RECONCILING → PARTIALLY_COMPLETED
RECONCILING → FAILED
RECONCILING → UNCERTAIN
```

---

# Part XIV — Flush Lifecycle

## 142. LogFlushState

Canonical states:

```text
REQUESTED
VALIDATING
DRAINING_BUFFERS
FLUSHING_SINKS
WAITING_FOR_CONFIRMATION
FINALIZING
FLUSHED
PARTIALLY_FLUSHED
TIMED_OUT
FAILED
CANCELED
```

---

## 143. REQUESTED

A flush request exists.

---

## 144. VALIDATING

The deadline, sink scope, classification scope, and lifecycle state are validated.

Valid outgoing transitions:

```text
VALIDATING → DRAINING_BUFFERS
VALIDATING → FLUSHING_SINKS
VALIDATING → FAILED
VALIDATING → CANCELED
```

---

## 145. DRAINING_BUFFERS

Eligible records move from buffers to sink batches.

Valid outgoing transitions:

```text
DRAINING_BUFFERS → FLUSHING_SINKS
DRAINING_BUFFERS → TIMED_OUT
DRAINING_BUFFERS → FAILED
```

---

## 146. FLUSHING_SINKS

Sinks flush pending writes.

Valid outgoing transitions:

```text
FLUSHING_SINKS → WAITING_FOR_CONFIRMATION
FLUSHING_SINKS → FINALIZING
FLUSHING_SINKS → TIMED_OUT
FLUSHING_SINKS → FAILED
```

---

## 147. WAITING_FOR_CONFIRMATION

Persistence-confirming sinks report completion.

Valid outgoing transitions:

```text
WAITING_FOR_CONFIRMATION → FINALIZING
WAITING_FOR_CONFIRMATION → TIMED_OUT
WAITING_FOR_CONFIRMATION → FAILED
```

---

## 148. FINALIZING

Counts and sink outcomes are summarized.

Valid outgoing transitions:

```text
FINALIZING → FLUSHED
FINALIZING → PARTIALLY_FLUSHED
FINALIZING → TIMED_OUT
FINALIZING → FAILED
```

---

## 149. Flush Terminal States

```text
FLUSHED
PARTIALLY_FLUSHED
TIMED_OUT
FAILED
CANCELED
```

Late sink confirmation is non-authoritative for the completed flush result.

---

# Part XV — Retention Cleanup Lifecycle

## 150. RetentionCleanupState

Canonical states:

```text
REQUESTED
VALIDATING_POLICY
ENUMERATING_FILES
CLASSIFYING_FILES
DELETING_ELIGIBLE_FILES
FINALIZING
COMPLETED
PARTIALLY_COMPLETED
NOT_REQUIRED
TIMED_OUT
FAILED
CANCELED
```

---

## 151. VALIDATING_POLICY

Checks:

- bounded limits;
- current file preservation;
- classification overrides;
- minimum files to keep;
- path boundaries.

---

## 152. ENUMERATING_FILES

The sink discovers managed files.

Raw arbitrary directories must not be traversed beyond the approved root.

---

## 153. CLASSIFYING_FILES

Files are classified as:

```text
ACTIVE
PRESERVE
ELIGIBLE
CORRUPTED
UNKNOWN
```

Unknown files are not deleted automatically.

---

## 154. DELETING_ELIGIBLE_FILES

Deletion runs within a bounded deadline.

---

## 155. Retention Terminal States

```text
COMPLETED
PARTIALLY_COMPLETED
NOT_REQUIRED
TIMED_OUT
FAILED
CANCELED
```

---

# Part XVI — Compression Lifecycle

## 156. LogCompressionState

Canonical states:

```text
REQUESTED
VALIDATING
READING_SOURCE
WRITING_ARCHIVE
VERIFYING_ARCHIVE
DELETING_SOURCE
COMPLETED
PARTIALLY_COMPLETED
TIMED_OUT
FAILED
CANCELED
```

---

## 157. Compression Safety Invariant

The source file must not be deleted before archive verification succeeds.

---

## 158. PARTIALLY_COMPLETED

A verified archive exists, but source deletion failed.

The source remains retention-eligible.

---

# Part XVII — Diagnostics Query Lifecycle

## 159. LoggingDiagnosticsQueryState

Canonical states:

```text
REQUESTED
AUTHORIZING
FILTERING
READING
REDACTING_OUTPUT
COMPLETED
PARTIALLY_COMPLETED
REJECTED
TIMED_OUT
FAILED
CANCELED
```

---

## 160. AUTHORIZING

Caller clearance is evaluated.

Restricted and audit data require explicit authority.

---

## 161. FILTERING

Time, severity, category, module, and maximum record limits are applied.

---

## 162. READING

Safe records are read from approved diagnostic sources.

---

## 163. REDACTING_OUTPUT

A query-specific output inspection occurs.

This is required even though stored records were already inspected.

---

## 164. Diagnostics Query Terminal States

```text
COMPLETED
PARTIALLY_COMPLETED
REJECTED
TIMED_OUT
FAILED
CANCELED
```

---

# Part XVIII — Diagnostics Export Lifecycle

## 165. DiagnosticsExportState

Canonical states:

```text
REQUESTED
AUTHORIZING
SELECTING_DATA
REDACTING_RECORDS
ASSEMBLING_BUNDLE
INSPECTING_COMPLETE_BUNDLE
WRITING_DESTINATION
FINALIZING_MANIFEST
EXPORTED
PARTIALLY_EXPORTED
BLOCKED_UNSAFE
TIMED_OUT
FAILED
CANCELED
```

---

## 166. REQUESTED

An export request exists.

---

## 167. AUTHORIZING

Checks:

- caller clearance;
- restricted inclusion;
- audit inclusion;
- consent;
- destination policy.

Valid outgoing transitions:

```text
AUTHORIZING → SELECTING_DATA
AUTHORIZING → BLOCKED_UNSAFE
AUTHORIZING → FAILED
AUTHORIZING → CANCELED
```

---

## 168. SELECTING_DATA

Approved logs and summaries are selected.

Valid outgoing transitions:

```text
SELECTING_DATA → REDACTING_RECORDS
SELECTING_DATA → FAILED
SELECTING_DATA → TIMED_OUT
```

---

## 169. REDACTING_RECORDS

A second record-level redaction pass runs.

Valid outgoing transitions:

```text
REDACTING_RECORDS → ASSEMBLING_BUNDLE
REDACTING_RECORDS → BLOCKED_UNSAFE
REDACTING_RECORDS → FAILED
```

---

## 170. ASSEMBLING_BUNDLE

The bundle and provisional manifest are built.

Valid outgoing transitions:

```text
ASSEMBLING_BUNDLE → INSPECTING_COMPLETE_BUNDLE
ASSEMBLING_BUNDLE → TIMED_OUT
ASSEMBLING_BUNDLE → FAILED
```

---

## 171. INSPECTING_COMPLETE_BUNDLE

The entire assembled bundle is inspected.

Valid outgoing transitions:

```text
INSPECTING_COMPLETE_BUNDLE → WRITING_DESTINATION
INSPECTING_COMPLETE_BUNDLE → BLOCKED_UNSAFE
INSPECTING_COMPLETE_BUNDLE → FAILED
```

---

## 172. WRITING_DESTINATION

The approved destination is written.

Valid outgoing transitions:

```text
WRITING_DESTINATION → FINALIZING_MANIFEST
WRITING_DESTINATION → PARTIALLY_EXPORTED
WRITING_DESTINATION → TIMED_OUT
WRITING_DESTINATION → FAILED
```

---

## 173. FINALIZING_MANIFEST

Checksums, counts, and redaction summary are committed.

Valid outgoing transitions:

```text
FINALIZING_MANIFEST → EXPORTED
FINALIZING_MANIFEST → PARTIALLY_EXPORTED
FINALIZING_MANIFEST → FAILED
```

---

## 174. Export Terminal States

```text
EXPORTED
PARTIALLY_EXPORTED
BLOCKED_UNSAFE
TIMED_OUT
FAILED
CANCELED
```

---

# Part XIX — Audit Write Lifecycle

## 175. AuditWriteState

Canonical states:

```text
CREATED
VALIDATING
INSPECTING_SAFETY
ADMITTING
WRITING
WAITING_FOR_CONFIRMATION
WRITTEN
REJECTED_UNSAFE
AUDIT_SINK_UNAVAILABLE
TIMED_OUT
FAILED
CANCELED
UNCERTAIN
RECONCILING
```

---

## 176. CREATED

An audit record exists.

---

## 177. VALIDATING

Checks:

- actor;
- action;
- target;
- outcome;
- mandatory policy;
- classification;
- retention.

Valid outgoing transitions:

```text
VALIDATING → INSPECTING_SAFETY
VALIDATING → REJECTED_UNSAFE
VALIDATING → FAILED
```

---

## 178. INSPECTING_SAFETY

Audit data is inspected.

Audit classification does not permit secrets.

Valid outgoing transitions:

```text
INSPECTING_SAFETY → ADMITTING
INSPECTING_SAFETY → REJECTED_UNSAFE
INSPECTING_SAFETY → FAILED
```

---

## 179. ADMITTING

The audit sink or emergency audit path is selected.

Valid outgoing transitions:

```text
ADMITTING → WRITING
ADMITTING → AUDIT_SINK_UNAVAILABLE
ADMITTING → TIMED_OUT
ADMITTING → FAILED
```

---

## 180. WRITING

The audit record is written.

Valid outgoing transitions:

```text
WRITING → WAITING_FOR_CONFIRMATION
WRITING → WRITTEN
WRITING → TIMED_OUT
WRITING → FAILED
WRITING → UNCERTAIN
```

---

## 181. WAITING_FOR_CONFIRMATION

Persistence confirmation is required.

Valid outgoing transitions:

```text
WAITING_FOR_CONFIRMATION → WRITTEN
WAITING_FOR_CONFIRMATION → TIMED_OUT
WAITING_FOR_CONFIRMATION → UNCERTAIN
WAITING_FOR_CONFIRMATION → FAILED
```

---

## 182. WRITTEN

The audit write completed according to sink guarantees.

Terminal.

---

## 183. REJECTED_UNSAFE

The audit record contained prohibited data or invalid semantics.

Terminal.

---

## 184. AUDIT_SINK_UNAVAILABLE

No approved audit sink was available.

Terminal for the write attempt.

The owning module applies configured fail-closed or warning behavior.

---

## 185. UNCERTAIN

The system cannot determine whether persistence completed.

Valid outgoing transition:

```text
UNCERTAIN → RECONCILING
```

---

## 186. RECONCILING

The system checks:

- audit record identity;
- sink state;
- append position;
- persistence receipt;
- duplicate possibility.

Valid outgoing transitions:

```text
RECONCILING → WRITTEN
RECONCILING → FAILED
RECONCILING → UNCERTAIN
```

---

# Part XX — Bootstrap Logger Lifecycle

## 187. BootstrapLoggerState

Canonical states:

```text
CREATED
AVAILABLE
HANDING_OFF
HANDED_OFF
STOPPING
TERMINATED
FAILED
```

---

## 188. AVAILABLE

The minimal logger accepts approved bootstrap records.

Valid outgoing transitions:

```text
AVAILABLE → HANDING_OFF
AVAILABLE → STOPPING
AVAILABLE → FAILED
```

---

## 189. HANDING_OFF

Buffered bootstrap records are optionally transferred to the normal pipeline.

Valid outgoing transitions:

```text
HANDING_OFF → HANDED_OFF
HANDING_OFF → FAILED
```

---

## 190. HANDED_OFF

The normal logger is authoritative.

Bootstrap logging may remain as emergency fallback only if policy allows.

Valid outgoing transitions:

```text
HANDED_OFF → STOPPING
```

---

## 191. TERMINATED

The bootstrap logger is closed.

Terminal.

---

# Part XXI — Emergency Logger Lifecycle

## 192. EmergencyLoggerState

Canonical states:

```text
CREATED
AVAILABLE
WRITING
DEGRADED
UNAVAILABLE
STOPPING
TERMINATED
FAILED
```

---

## 193. AVAILABLE

The emergency path can accept minimal critical records.

Valid outgoing transitions:

```text
AVAILABLE → WRITING
AVAILABLE → DEGRADED
AVAILABLE → UNAVAILABLE
AVAILABLE → STOPPING
```

---

## 194. WRITING

A bounded synchronous write is in progress.

Valid outgoing transitions:

```text
WRITING → AVAILABLE
WRITING → DEGRADED
WRITING → UNAVAILABLE
WRITING → FAILED
```

---

## 195. DEGRADED

The path can write only a reduced format or destination.

Valid outgoing transitions:

```text
DEGRADED → AVAILABLE
DEGRADED → UNAVAILABLE
DEGRADED → STOPPING
DEGRADED → FAILED
```

---

## 196. UNAVAILABLE

No emergency write is currently possible.

Valid outgoing transitions:

```text
UNAVAILABLE → AVAILABLE
UNAVAILABLE → STOPPING
UNAVAILABLE → FAILED
```

---

## 197. TERMINATED

The emergency logger is closed.

Terminal.

---

# Part XXII — Cross-State Rules

## 198. Logging and Buffer Relationship

| Logging state | Buffer behavior |
|---|---|
| `BOOTSTRAPPING` | normal buffers absent |
| `INITIALIZING` | buffers initializing |
| `READY` | buffers available but intake limited |
| `RUNNING` | buffers available |
| `DEGRADED` | some buffers may be backpressured |
| `QUIESCING` | low-severity admission restricted |
| `DRAINING` | no normal new admission |
| `FLUSHING` | buffers empty or draining |
| `STOPPING` | buffers stopping |
| `TERMINATED` | buffers terminated |

---

## 199. Logging and Sink Relationship

When Logging is `RUNNING`, at least one approved normal sink should be `AVAILABLE` or `DEGRADED`.

If no safe sink remains:

- critical records may use emergency path;
- Logging becomes `DEGRADED` or `FAILED`;
- restricted and audit policy may fail closed.

---

## 200. Record and Safety Relationship

A record may enter `READY` only when safety state is:

```text
SAFE
SAFE_REDACTED
```

A record in:

```text
BLOCKED
FAILED_SAFE
```

must not enter a normal buffer.

---

## 201. Record and Batch Relationship

```text
LogRecord ACCEPTED
    ↓
zero or more sink routes
    ↓
one batch membership per selected sink
```

A record may be accepted even when one optional sink later fails.

---

## 202. Sink and File Relationship

A rolling file sink may be `AVAILABLE` only when it has one authoritative `ACTIVE` file or an approved temporary write path.

At most one file is active per rolling sink instance.

---

## 203. Rotation and Sink Relationship

```text
Sink AVAILABLE / DEGRADED
    ↓
Sink ROTATING
    ↓
Sink AVAILABLE / DEGRADED / UNAVAILABLE / FAILED
```

Rotation state remains independent.

---

## 204. Flush and Shutdown Relationship

Normal shutdown:

```text
Logging QUIESCING
    ↓
Logging DRAINING
    ↓
Flush REQUESTED ... terminal
    ↓
Logging STOPPING
```

Flush timeout does not prevent bounded stop.

---

## 205. Restricted Sink Rule

A `RESTRICTED_SECURITY` or `AUDIT_RESTRICTED` record may be routed only to sinks whose descriptor accepts that classification.

If none exists:

- record is blocked or emergency-routed only to an approved equivalent;
- unrestricted fallback is prohibited.

---

## 206. Diagnostics Export Relationship

Diagnostics export reads only:

- already-safe records;
- approved summaries;
- authorized restricted data.

It then performs an additional export-specific inspection.

---

## 207. Audit and Ordinary Logging Relationship

An audit write is not considered successful merely because an ordinary log record was written.

Audit has its own state machine and persistence requirement.

---

# Part XXIII — Invalid Transitions

## 208. Invalid Logging Transitions

```text
CREATED → RUNNING
RUNNING → TERMINATED
FAILED → RUNNING
TERMINATED → RUNNING
DRAINING → RUNNING during normal shutdown
```

---

## 209. Invalid Record Transitions

```text
DRAFT → ACCEPTED
REJECTED_UNSAFE → ACCEPTED
FILTERED → ADMITTING
ACCEPTED → REDACTING
EMERGENCY_WRITTEN → ACCEPTED
```

---

## 210. Invalid Safety Transitions

```text
BLOCKED → SAFE
FAILED_SAFE → REDACTING
SAFE → INSPECTING
SAFE_REDACTED → REDACTING
```

A new inspection requires a new operation.

---

## 211. Invalid Buffer Transitions

```text
CREATED → AVAILABLE
TERMINATED → AVAILABLE
FAILED → AVAILABLE
DRAINING → BACKPRESSURED
```

---

## 212. Invalid Sink Transitions

```text
UNREGISTERED → AVAILABLE
TERMINATED → AVAILABLE
FAILED → AVAILABLE
ROTATING → WRITING directly without returning to available/degraded state
```

---

## 213. Invalid Batch Transitions

```text
WRITTEN → WRITING
TIMED_OUT → WRITTEN
ABANDONED → WRITTEN
PARTIALLY_WRITTEN → WRITING
```

Late completion is non-authoritative.

---

## 214. Invalid File Transitions

```text
DELETED → ACTIVE
SEALED → ACTIVE
COMPRESSED → ACTIVE
FAILED → ACTIVE
```

A new active file requires a new file identity.

---

## 215. Invalid Audit Transitions

```text
REJECTED_UNSAFE → WRITTEN
FAILED → WRITTEN
TIMED_OUT → WRITTEN
AUDIT_SINK_UNAVAILABLE → WRITING
```

---

# Part XXIV — Concurrency and Authority

## 216. Single Logical Writer

Logging is the single logical writer for:

- Logging lifecycle;
- active policy revision;
- record admission state;
- buffer state;
- sink state;
- batch state;
- rotation state;
- flush state;
- export state;
- audit-write state.

Sinks report outcomes.

They do not directly mutate Logging lifecycle.

---

## 217. State Versioning

Mutable entities should include:

```text
stateVersion
```

Control operations validate expected versions when concurrent mutation is possible.

---

## 218. Policy Update Race

A record uses one committed policy revision for its complete safety and admission decision.

A live policy update must not cause one record to be filtered under one revision and redacted under another without explicit compatibility rules.

---

## 219. Record Completion Race

Possible race:

```text
buffer admission succeeds
and timeout fires
```

Exactly one terminal admission state wins.

The losing signal is recorded as late and does not change authority.

---

## 220. Batch Write Race

Possible race:

```text
sink reports success
and write timeout fires
```

Exactly one terminal batch outcome wins.

If persistence is uncertain, the result must not be silently rewritten as success.

---

## 221. Rotation Race

Only one active rotation may exist per rolling sink.

A concurrent rotation request should return:

```text
already running
not required
or version conflict
```

---

## 222. Shutdown Race

Records racing with quiesce are decided by the admission barrier:

```text
accepted before barrier
    → eligible for drain

not accepted before barrier
    → filtered, rejected, or emergency-routed by shutdown policy
```

---

## 223. Sink Disable Race

Batches already in `WRITING` follow policy:

```text
ALLOW_TO_COMPLETE
CANCEL
ABANDON_AFTER_DEADLINE
```

No new batch starts after the disable barrier.

---

# Part XXV — Persistence and Crash Recovery

## 224. In-Memory Buffer Recovery

After process crash:

- buffered records are lost;
- active batches are lost;
- in-memory diagnostics are lost;
- scope state is lost;
- flush state is lost.

Logging must not claim persistence for records not confirmed by a sink.

---

## 225. Rolling File Recovery

At startup, the rolling file sink should inspect:

- active file;
- temporary files;
- orphaned files;
- incomplete compression;
- retention metadata;
- file sequence.

Possible recovery actions:

```text
resume active file
seal orphaned file
rename temporary file
quarantine corrupted file
create new active file
run retention cleanup
```

---

## 226. Crash During Rotation

If crash occurs during rotation:

```text
old file may be sealed
new file may or may not exist
active authority may be uncertain
```

Startup reconciliation determines one active file.

No blind overwrite is allowed.

---

## 227. Crash During Audit Write

If audit persistence outcome is unknown:

- preserve audit record identity;
- check sink receipt or record presence;
- avoid duplicate administrative action;
- duplicates may be acceptable only under append-only idempotency rules.

---

## 228. Bootstrap Recovery

If normal Logging initialization fails:

- bootstrap logging remains authoritative if safe;
- emergency path remains available where possible;
- application may continue degraded or stop according to policy.

---

# Part XXVI — Command-to-State Mapping

## 229. Initialize Logging

```text
Logging CREATED
    ↓ BOOTSTRAPPING
    ↓ INITIALIZING
    ↓ READY

Policy DRAFT
    ↓ VALIDATING
    ↓ READY
    ↓ ACTIVE

Buffers CREATED
    ↓ INITIALIZING
    ↓ AVAILABLE

Sinks REGISTERED
    ↓ INITIALIZING
    ↓ AVAILABLE / DEGRADED
```

---

## 230. Start Logging

```text
Logging READY → RUNNING
Bootstrap AVAILABLE → HANDING_OFF → HANDED_OFF
```

---

## 231. Write Log Record

```text
Record DRAFT
    ↓ FILTERING
    ↓ ENRICHING
    ↓ NORMALIZING_EXCEPTION?
    ↓ INSPECTING_SAFETY
    ↓ REDACTING?
    ↓ READY
    ↓ ADMITTING
    ↓ ACCEPTED / terminal rejection
```

---

## 232. Route and Write Batch

```text
Accepted Record
    ↓ Sink routing
    ↓ Batch CREATED
    ↓ FORMATTING
    ↓ READY
    ↓ WRITING
    ↓ terminal outcome
```

---

## 233. Rotate File

```text
Rotation REQUESTED
    ↓ VALIDATING
    ↓ FLUSHING_ACTIVE_FILE
    ↓ CLOSING_ACTIVE_FILE
    ↓ FINALIZING_OLD_FILE
    ↓ CREATING_NEW_FILE
    ↓ ACTIVATING_NEW_FILE
    ↓ SCHEDULING_RETENTION
    ↓ COMPLETED
```

---

## 234. Flush

```text
Flush REQUESTED
    ↓ VALIDATING
    ↓ DRAINING_BUFFERS
    ↓ FLUSHING_SINKS
    ↓ WAITING_FOR_CONFIRMATION?
    ↓ FINALIZING
    ↓ terminal outcome
```

---

## 235. Export Diagnostics

```text
Export REQUESTED
    ↓ AUTHORIZING
    ↓ SELECTING_DATA
    ↓ REDACTING_RECORDS
    ↓ ASSEMBLING_BUNDLE
    ↓ INSPECTING_COMPLETE_BUNDLE
    ↓ WRITING_DESTINATION
    ↓ FINALIZING_MANIFEST
    ↓ EXPORTED
```

---

## 236. Write Audit Record

```text
Audit CREATED
    ↓ VALIDATING
    ↓ INSPECTING_SAFETY
    ↓ ADMITTING
    ↓ WRITING
    ↓ WAITING_FOR_CONFIRMATION?
    ↓ WRITTEN
```

---

## 237. Shutdown

```text
Logging RUNNING / DEGRADED
    ↓ QUIESCING
    ↓ DRAINING
    ↓ FLUSHING
    ↓ STOPPING
    ↓ TERMINATED
```

---

# Part XXVII — State Events

## 238. Event Principle

Logging self-events report committed state transitions such as:

```text
LoggingStarted
LoggingDegraded
LoggingRecovered
LoggingQuiescing
LoggingFlushStarted
LoggingFlushCompleted
LoggingTerminated

LogPolicyActivated
LogRecordBlocked
LogBufferBackpressured
LogRecordsDropped
LogSinkAvailable
LogSinkDegraded
LogSinkUnavailable
LogRotationStarted
LogRotationCompleted
LogRetentionCleanupFailed
DiagnosticsExportCompleted
DiagnosticsExportBlocked
AuditWriteFailed
EmergencyLoggerUsed
```

Detailed payloads belong in `EVENTS.md`.

---

# Part XXVIII — Security Invariants

## 239. Safety Before Admission

```text
RecordSafetyState must be SAFE or SAFE_REDACTED
before LogRecordState may become ACCEPTED.
```

---

## 240. Secret Blocking

A record containing secret material must end in:

```text
REJECTED_UNSAFE
or FAILED_SAFE
```

It must never become:

```text
ACCEPTED
EMERGENCY_WRITTEN
```

unless the unsafe field has been fully removed and the resulting record is re-inspected as safe.

---

## 241. Restricted Routing

A restricted record must never produce a batch for an unauthorized sink.

---

## 242. One Active File

For each rolling file sink:

```text
At most one LogFileState = ACTIVE
```

---

## 243. Audit Independence

An ordinary log write cannot satisfy a mandatory audit-write requirement.

---

## 244. Terminal Batch Invariant

One write batch has exactly one terminal state.

---

## 245. Bounded Buffer Invariant

```text
recordCount <= configuredRecordCapacity
bytes <= configuredByteCapacity
```

when byte capacity is configured.

---

## 246. Bounded Shutdown Invariant

Every drain, flush, rotation, export, and shutdown operation has a finite deadline.

---

## 247. No Unsafe Fallback

Classification and privacy strength must not decrease during fallback.

---

# Part XXIX — MVP State Boundary

## 248. Required MVP State Machines

The MVP must implement:

```text
LoggingState
LogPolicyState
LogRecordState
RecordSafetyState
LogScopeState
LogBufferState
LogAdmissionState
LogSinkState
LogSinkHealthState
LogWriteBatchState
LogFormatterState
LogFileState
LogRotationState
LogFlushState
RetentionCleanupState
LoggingDiagnosticsQueryState
DiagnosticsExportState
AuditWriteState
BootstrapLoggerState
EmergencyLoggerState
```

The MVP may simplify active implementation of:

```text
LogCompressionState
AuditWrite reconciliation
sink circuit breaker
```

But it must preserve:

- safety-before-admission;
- immutable accepted records;
- bounded buffers;
- one active file;
- bounded flush;
- restricted fallback rules;
- export second inspection;
- terminal batch finality.

---

## 249. MVP Simplifications

Allowed:

- local files only;
- no remote sink;
- no distributed log ingestion;
- no durable in-memory queue recovery;
- no full tamper-evident audit chain;
- no advanced sink circuit breaker;
- no automatic encrypted archive.

Not allowed:

- unbounded buffers;
- unbounded retention;
- secret admission;
- user-content admission by default;
- unrestricted fallback for restricted records;
- infinite shutdown wait;
- raw exception persistence;
- direct raw-file diagnostics export.

---

# Part XXX — State Decisions

## 250. Decisions

### Decision 1 — Independent state machines

Logging, records, safety, buffers, sinks, files, rotation, flush, export, and audit remain separate.

### Decision 2 — Safety precedes admission

No normal record enters a buffer before inspection.

### Decision 3 — Accepted records are immutable

Sink formatting does not mutate the original record.

### Decision 4 — Admission and persistence are separate

Accepted does not mean every sink wrote successfully.

### Decision 5 — Sink failure is isolated

Optional sink failure degrades rather than corrupts unrelated sinks.

### Decision 6 — One active rolling file

Active-file authority is explicit and reconciled after crash.

### Decision 7 — Timeout is terminal

Late sink completion is non-authoritative.

### Decision 8 — Export has a full-bundle inspection state

Second-pass safety is mandatory.

### Decision 9 — Audit has an independent lifecycle

Ordinary logging cannot substitute for audit persistence.

### Decision 10 — Emergency logging is separate

It is bounded, synchronous, minimal, and non-recursive.

### Decision 11 — Shutdown is bounded

Records may be dropped or abandoned according to policy after deadline.

### Decision 12 — Fail-closed safety

When safety cannot be proven, the record is blocked.

---

# Part XXXI — Open Decisions

## 251. Lifecycle Decisions

Still to finalize:

- whether reversible quiesce is supported;
- exact bootstrap handoff behavior;
- failed sink reactivation;
- policy update barriers;
- when Logging enters `FAILED` versus `DEGRADED`;
- emergency logger shutdown order.

---

## 252. Record Decisions

Still to finalize:

- whether `renderedMessage` is stored;
- exact suppression-summary lifecycle;
- whether sampled-out records get record IDs;
- admission timeout defaults;
- record-age expiration.

---

## 253. Buffer Decisions

Still to finalize:

- default capacity;
- critical reserve size;
- byte-capacity enforcement;
- fairness between security and normal buffers;
- drain order;
- low-severity drop thresholds.

---

## 254. Sink and File Decisions

Still to finalize:

- sink recovery probes;
- file reconciliation rules;
- partial-write handling;
- rotation uncertainty thresholds;
- compression retry;
- corrupted-file quarantine;
- active-file naming.

---

## 255. Flush and Shutdown Decisions

Still to finalize:

- default flush deadline;
- persistence-confirmation timeout;
- records-lost summary;
- security sink flush priority;
- audit sink fail-closed behavior;
- force-stop conditions.

---

## 256. Export and Audit Decisions

Still to finalize:

- export cancellation cleanup;
- manifest checksum policy;
- partial-export semantics;
- audit uncertainty reconciliation;
- emergency audit store;
- audit retention.

---

# Part XXXII — Documentation Order

## 257. Recommended Order

```text
MODULE.md
    ↓
CONTRACT.md
    ↓
STATES.md
    ↓
EVENTS.md
    ↓
ERRORS.md
    ↓
README.md
```

`EVENTS.md` should next define:

- Logging lifecycle events;
- policy events;
- record-blocked and redaction events;
- buffer pressure and drop events;
- sink lifecycle and health events;
- write-batch failure events;
- rotation, flush, retention, and compression events;
- diagnostics query and export events;
- audit-write events;
- bootstrap and emergency logger events.

---

# Part XXXIII — Related Documents

## 258. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/STATE_MACHINE.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/logging/MODULE.md
03-infrastructure/logging/CONTRACT.md

03-infrastructure/configuration/MODULE.md
03-infrastructure/configuration/CONTRACT.md

03-infrastructure/secret-management/MODULE.md
03-infrastructure/secret-management/CONTRACT.md
03-infrastructure/secret-management/ERRORS.md

03-infrastructure/event-bus/MODULE.md
03-infrastructure/event-bus/CONTRACT.md
03-infrastructure/event-bus/STATES.md
03-infrastructure/event-bus/ERRORS.md
```

Future Logging documents:

```text
03-infrastructure/logging/EVENTS.md
03-infrastructure/logging/ERRORS.md
03-infrastructure/logging/README.md
```

---

## 259. Summary

Logging uses separate state machines for the module lifecycle, policy, records, safety inspection, scopes, buffers, sinks, files, rotation, flush, retention, diagnostics, export, audit, bootstrap, and emergency paths.

The main Logging lifecycle is:

```text
CREATED
    ↓
BOOTSTRAPPING
    ↓
INITIALIZING
    ↓
READY
    ↓
RUNNING
    ↓
QUIESCING
    ↓
DRAINING
    ↓
FLUSHING
    ↓
STOPPING
    ↓
TERMINATED
```

The main record lifecycle is:

```text
DRAFT
    ↓
FILTERING
    ↓
ENRICHING
    ↓
NORMALIZING_EXCEPTION?
    ↓
INSPECTING_SAFETY
    ↓
REDACTING?
    ↓
READY
    ↓
ADMITTING
    ↓
ACCEPTED / FILTERED / SAMPLED_OUT / SUPPRESSED / REJECTED
```

The main write-batch lifecycle is:

```text
CREATED
    ↓
FORMATTING
    ↓
READY
    ↓
WRITING
    ↓
WRITTEN / PARTIALLY_WRITTEN / FAILED / TIMED_OUT / ABANDONED
```

The architecture preserves these invariants:

- safety precedes admission;
- accepted records are immutable;
- admission differs from persistence;
- buffers and files are bounded;
- sink failure is isolated;
- restricted classification never weakens;
- one active file exists per rolling sink;
- timeout is terminal;
- late completion is non-authoritative;
- export performs second-pass inspection;
- audit has an independent lifecycle;
- shutdown is bounded;
- safety failures fail closed.

This document is the state-machine source of truth for subsequent Logging events, errors, and implementation documentation.
