# Logging Events

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Logging  
> **Document:** Integration Events  
> **Path:** `03-infrastructure/logging/EVENTS.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/logging/MODULE.md`
> - `03-infrastructure/logging/CONTRACT.md`
> - `03-infrastructure/logging/STATES.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`
> - `03-infrastructure/secret-management/EVENTS.md`
> - `03-infrastructure/event-bus/EVENTS.md`

---

## 1. Purpose

This document defines the events published and consumed by the Logging infrastructure module itself.

These events communicate safe operational facts concerning:

- Logging startup, degradation, recovery, quiesce, flush, and shutdown;
- policy activation and rejection;
- record filtering, sampling, suppression, blocking, and emergency writing;
- redaction and safety-inspection outcomes;
- buffer pressure, reserve use, drops, drain, and failure;
- sink lifecycle and health;
- sink writes and partial writes;
- file creation, rotation, corruption, recovery, compression, and deletion;
- retention cleanup;
- diagnostics queries and exports;
- audit writes;
- bootstrap logger and emergency logger behavior;
- configuration changes;
- restricted security conditions.

This document does not redefine events owned by other CRAI modules.

Logging may consume those events as safe inputs for diagnostic recording, but it does not become their semantic owner.

---

## 2. Event Principles

### 2.1 Events represent committed facts

Correct:

```text
LoggingStarted
LogSinkDegraded
LogRecordBlocked
LogRotationCompleted
```

Incorrect:

```text
StartLogging
DegradeSink
BlockLogRecord
RotateLogFile
```

### 2.2 Events are immutable

Once published, a Logging event cannot be changed.

A later correction or recovery requires a new event.

### 2.3 State commits before event publication

```text
Logging state transition
    ↓
state committed
    ↓
self-event emitted
```

### 2.4 Logging self-events must not recurse

Logging cannot rely on the ordinary Logging path to report every Logging failure.

Critical self-events may use:

- guarded Event Bus publication;
- emergency logger;
- restricted security sink;
- non-recursive in-memory health channel;
- direct lifecycle callback.

### 2.5 Self-events never contain the original log record

A Logging event may include:

```text
recordId
severity
category
sourceModule
normalizedErrorCode
sinkId
bufferId
```

It must not include:

- original message text;
- rendered message;
- property values;
- exception messages;
- stack traces;
- user content;
- secret material.

### 2.6 Events are not logs

Logging events report infrastructure facts.

They are not a substitute for the original diagnostic record.

---

## 3. Event Visibility

Recommended visibility classes:

```text
PUBLIC_INTERNAL
MODULE_INTERNAL
RESTRICTED_SECURITY
OBSERVABILITY_ONLY
AUDIT_ONLY
LOCAL_COMPONENT_ONLY
```

Most Logging self-events should be:

```text
OBSERVABILITY_ONLY
MODULE_INTERNAL
```

Security failures should be:

```text
RESTRICTED_SECURITY
AUDIT_ONLY
```

Lifecycle readiness events may be:

```text
PUBLIC_INTERNAL
```

---

## 4. Event Envelope

Logging self-events use the shared CRAI event envelope:

```text
EventEnvelope<TPayload> {
    eventId
    eventType
    eventVersion
    category

    occurredAt
    publishedAt

    sourceModule = "logging"
    sourceComponent?
    publisherId

    correlationId
    causationId?

    applicationInstanceId
    entityId?
    operationId?

    ordering
    priority
    visibility
    securityClassification

    payload
    metadata
}
```

---

## 5. Naming Convention

Canonical event type:

```text
logging.<entity>.<past-tense-fact>
```

Examples:

```text
logging.lifecycle.started
logging.sink.degraded
logging.record.blocked
logging.rotation.completed
```

---

# Part I — Logging Lifecycle Events

## 6. LoggingBootstrapStarted

Event type:

```text
logging.lifecycle.bootstrap-started
```

Payload:

```text
LoggingBootstrapStartedPayload {
    loggingInstanceId
    applicationInstanceId
    bootstrapMode
    startedAt
}
```

Visibility:

```text
OBSERVABILITY_ONLY
```

---

## 7. LoggingInitializationStarted

Event type:

```text
logging.lifecycle.initialization-started
```

Payload:

```text
LoggingInitializationStartedPayload {
    loggingInstanceId
    configuredSinkCount
    configuredBufferCount
    auditEnabled
    startedAt
}
```

---

## 8. LoggingReady

Published when Logging reaches `READY`.

Event type:

```text
logging.lifecycle.ready
```

Payload:

```text
LoggingReadyPayload {
    loggingInstanceId
    activePolicyRevision
    availableSinkCount
    degradedSinkCount
    unavailableSinkCount
    bootstrapHandoffPending
    readyAt
}
```

---

## 9. LoggingStarted

Published after Logging enters `RUNNING`.

Event type:

```text
logging.lifecycle.started
```

Payload:

```text
LoggingStartedPayload {
    loggingInstanceId
    pipelineMode
    normalBufferCapacityClass
    securityBufferAvailable
    auditBufferAvailable
    startedAt
}
```

Expected MVP value:

```text
pipelineMode = ASYNC_BOUNDED_LOCAL
```

Visibility:

```text
PUBLIC_INTERNAL
```

---

## 10. LoggingDegraded

Event type:

```text
logging.lifecycle.degraded
```

Payload:

```text
LoggingDegradedPayload {
    previousState
    currentState = DEGRADED
    degradedComponents[]
    capabilityImpact[]
    normalizedReasonCode
    emergencyPathAvailable
    degradedAt
}
```

---

## 11. LoggingRecovered

Event type:

```text
logging.lifecycle.recovered
```

Payload:

```text
LoggingRecoveredPayload {
    previousState = DEGRADED
    currentState = RUNNING
    recoveredComponents[]
    recoveredAt
}
```

---

## 12. LoggingQuiescing

Event type:

```text
logging.lifecycle.quiescing
```

Payload:

```text
LoggingQuiescingPayload {
    previousState
    currentState = QUIESCING
    minimumAcceptedSeverity
    allowedCategories[]
    rejectedCategories[]
    reasonCode
    effectiveAt
}
```

Visibility:

```text
PUBLIC_INTERNAL
```

---

## 13. LoggingDrainStarted

Event type:

```text
logging.lifecycle.drain-started
```

Payload:

```text
LoggingDrainStartedPayload {
    drainOperationId
    normalBufferDepth
    securityBufferDepth
    auditBufferDepth?
    deadline
    startedAt
}
```

---

## 14. LoggingDrainCompleted

Event type:

```text
logging.lifecycle.drain-completed
```

Payload:

```text
LoggingDrainCompletedPayload {
    drainOperationId
    outcome
    recordsDrained
    recordsDropped
    recordsRemaining
    completedAt
}
```

Possible outcomes:

```text
DRAINED
PARTIALLY_DRAINED
TIMED_OUT
FAILED
```

---

## 15. LoggingFlushStarted

Event type:

```text
logging.lifecycle.flush-started
```

Payload:

```text
LoggingFlushStartedPayload {
    flushId
    reason
    sinkCount
    deadline
    startedAt
}
```

---

## 16. LoggingFlushCompleted

Event type:

```text
logging.lifecycle.flush-completed
```

Payload:

```text
LoggingFlushCompletedPayload {
    flushId
    outcome
    recordsAttempted
    recordsWritten
    recordsDropped
    sinksSucceeded
    sinksFailed
    completedAt
}
```

Possible outcomes:

```text
FLUSHED
PARTIALLY_FLUSHED
TIMED_OUT
FAILED
CANCELED
```

---

## 17. LoggingStopping

Event type:

```text
logging.lifecycle.stopping
```

Payload:

```text
LoggingStoppingPayload {
    previousState
    currentState = STOPPING
    remainingBufferDepth
    activeSinkCount
    reasonCode
    stoppingAt
}
```

---

## 18. LoggingTerminated

Event type:

```text
logging.lifecycle.terminated
```

Payload:

```text
LoggingTerminatedPayload {
    finalState = TERMINATED
    flushOutcome?
    recordsLostBySeverity
    sinkTerminationSummary
    terminatedAt
}
```

Visibility:

```text
PUBLIC_INTERNAL
```

---

## 19. LoggingFailed

Event type:

```text
logging.lifecycle.failed
```

Payload:

```text
LoggingFailedPayload {
    previousState
    currentState = FAILED
    failedComponent
    normalizedErrorCode
    unsafeAdmissionBlocked
    emergencyPathAvailable
    stopRequired
    failedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
PUBLIC_INTERNAL safe projection
```

---

# Part II — Policy Events

## 20. LogPolicyValidationStarted

Event type:

```text
logging.policy.validation-started
```

Payload:

```text
LogPolicyValidationStartedPayload {
    candidatePolicyRevision
    currentPolicyRevision?
    validationMode
    startedAt
}
```

Visibility:

```text
OBSERVABILITY_ONLY
```

---

## 21. LogPolicyActivated

Event type:

```text
logging.policy.activated
```

Payload:

```text
LogPolicyActivatedPayload {
    previousPolicyRevision?
    currentPolicyRevision
    applyMode
    liveChanges[]
    restartRequiredChanges[]
    activatedAt
}
```

---

## 22. LogPolicySuperseded

Event type:

```text
logging.policy.superseded
```

Payload:

```text
LogPolicySupersededPayload {
    supersededPolicyRevision
    successorPolicyRevision
    supersededAt
}
```

---

## 23. LogPolicyRejected

Event type:

```text
logging.policy.rejected
```

Payload:

```text
LogPolicyRejectedPayload {
    candidatePolicyRevision
    normalizedErrorCode
    rejectionClass
    previousPolicyRetained
    rejectedAt
}
```

---

## 24. LogPolicyInvalidated

Event type:

```text
logging.policy.invalidated
```

Payload:

```text
LogPolicyInvalidatedPayload {
    policyRevision
    normalizedErrorCode
    fallbackPolicyRevision?
    loggingImpact
    invalidatedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

# Part III — Record Admission Events

## 25. LogRecordAccepted

High-volume success event.

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY
OBSERVABILITY_ONLY
```

Event type:

```text
logging.record.accepted
```

Payload:

```text
LogRecordAcceptedPayload {
    recordId
    severity
    category
    sourceModule
    effectivePrivacyClassification
    effectiveSecurityClassification
    targetSinkClassCount
    acceptedAt
}
```

No message or property values are included.

---

## 26. LogRecordFiltered

Event type:

```text
logging.record.filtered
```

Payload:

```text
LogRecordFilteredPayload {
    severity
    category
    sourceModule
    filterReason
    policyRevision
    filteredAt
}
```

---

## 27. LogRecordSampledOut

Event type:

```text
logging.record.sampled-out
```

Payload:

```text
LogRecordSampledOutPayload {
    severity
    category
    sourceModule
    samplingRuleId
    sampledAt
}
```

---

## 28. LogRecordSuppressed

Event type:

```text
logging.record.suppressed
```

Payload:

```text
LogRecordSuppressedPayload {
    suppressionKeyId
    severity
    category
    sourceModule
    suppressionWindow
    suppressedAt
}
```

---

## 29. LogSuppressionSummaryCreated

Event type:

```text
logging.record.suppression-summary-created
```

Payload:

```text
LogSuppressionSummaryCreatedPayload {
    suppressionKeyId
    severity
    category
    sourceModule
    suppressedCount
    windowStartedAt
    windowEndedAt
    createdAt
}
```

The original message and property values are excluded.

---

## 30. LogRecordBlocked

Event type:

```text
logging.record.blocked
```

Payload:

```text
LogRecordBlockedPayload {
    attemptedRecordId?
    severity
    category
    sourceModule
    findingClasses[]
    blockStage
    originalContentDiscarded
    blockedAt
}
```

Possible block stages:

```text
VALIDATION
EXCEPTION_NORMALIZATION
SAFETY_INSPECTION
REDACTION
BUFFER_ADMISSION
FORMATTER
EXPORT
```

Visibility:

```text
RESTRICTED_SECURITY
OBSERVABILITY_ONLY safe projection
```

---

## 31. LogRecordCapacityRejected

Event type:

```text
logging.record.capacity-rejected
```

Payload:

```text
LogRecordCapacityRejectedPayload {
    attemptedRecordId?
    severity
    category
    sourceModule
    bufferId
    bufferClass
    capacityClass
    reserveExhausted
    rejectedAt
}
```

---

## 32. LogRecordAdmissionTimedOut

Event type:

```text
logging.record.admission-timed-out
```

Payload:

```text
LogRecordAdmissionTimedOutPayload {
    attemptedRecordId?
    severity
    category
    bufferId
    admissionTimeout
    timedOutAt
}
```

---

## 33. LogRecordEmergencyWritten

Event type:

```text
logging.record.emergency-written
```

Payload:

```text
LogRecordEmergencyWrittenPayload {
    recordId
    severity
    category
    sourceModule
    emergencySinkClass
    normalPipelineUnavailable
    writtenAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
OBSERVABILITY_ONLY
```

---

## 34. LogRecordFailedSafe

Event type:

```text
logging.record.failed-safe
```

Payload:

```text
LogRecordFailedSafePayload {
    attemptedRecordId?
    severity
    category
    sourceModule
    normalizedErrorCode
    unsafeOutputBlocked = true
    failedAt
}
```

---

# Part IV — Safety and Redaction Events

## 35. LogSafetyInspectionCompleted

Normally metrics-only.

Event type:

```text
logging.safety.inspection-completed
```

Payload:

```text
LogSafetyInspectionCompletedPayload {
    recordId?
    outcome
    findingCount
    effectivePrivacyClassification
    effectiveSecurityClassification
    policyRevision
    completedAt
}
```

Possible outcomes:

```text
SAFE
SAFE_REDACTED
BLOCKED
FAILED_SAFE
```

---

## 36. LogRedactionApplied

Event type:

```text
logging.redaction.applied
```

Payload:

```text
LogRedactionAppliedPayload {
    recordId?
    transformationClasses[]
    redactedFieldCount
    removedFieldCount
    policyRevision
    appliedAt
}
```

No original or transformed values are included.

---

## 37. LogRedactionBlocked

Event type:

```text
logging.redaction.blocked
```

Payload:

```text
LogRedactionBlockedPayload {
    recordId?
    findingClasses[]
    normalizedErrorCode
    outputBlocked = true
    blockedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

## 38. LogRedactionFailedSafe

Event type:

```text
logging.redaction.failed-safe
```

Payload:

```text
LogRedactionFailedSafePayload {
    recordId?
    failureStage
    normalizedErrorCode
    outputBlocked = true
    failedAt
}
```

---

## 39. SecretMaterialLoggingAttemptBlocked

Event type:

```text
logging.security.secret-material-attempt-blocked
```

Payload:

```text
SecretMaterialLoggingAttemptBlockedPayload {
    recordId?
    sourceModule
    sourceComponent?
    findingClasses[]
    boundaryType = LOGGING
    blockedAt
}
```

The matched material is never included.

Visibility:

```text
RESTRICTED_SECURITY
AUDIT_ONLY
```

---

## 40. UserContentLoggingAttemptBlocked

Event type:

```text
logging.security.user-content-attempt-blocked
```

Payload:

```text
UserContentLoggingAttemptBlockedPayload {
    recordId?
    sourceModule
    category
    contentClass
    diagnosticMode
    blockedAt
}
```

---

## 41. ClassificationDowngradeBlocked

Event type:

```text
logging.security.classification-downgrade-blocked
```

Payload:

```text
ClassificationDowngradeBlockedPayload {
    recordId?
    detectedClassification
    requestedClassification
    effectiveClassification
    sourceModule
    blockedAt
}
```

---

# Part V — Buffer Events

## 42. LogBufferInitialized

Event type:

```text
logging.buffer.initialized
```

Payload:

```text
LogBufferInitializedPayload {
    bufferId
    bufferClass
    capacityRecords
    criticalReserveRecords
    initializedAt
}
```

---

## 43. LogBufferAvailable

Event type:

```text
logging.buffer.available
```

Payload:

```text
LogBufferAvailablePayload {
    bufferId
    bufferClass
    previousState
    currentState = AVAILABLE
    queueDepth
    availableAt
}
```

---

## 44. LogBufferBackpressured

Event type:

```text
logging.buffer.backpressured
```

Payload:

```text
LogBufferBackpressuredPayload {
    bufferId
    bufferClass
    queueDepth
    capacityRecords
    utilizationClass
    reserveUtilizationClass?
    affectedSeverities[]
    detectedAt
}
```

---

## 45. LogBufferRecovered

Event type:

```text
logging.buffer.recovered
```

Payload:

```text
LogBufferRecoveredPayload {
    bufferId
    bufferClass
    previousState = BACKPRESSURED
    currentState = AVAILABLE
    queueDepth
    recoveredAt
}
```

---

## 46. LogCriticalReserveUsed

Event type:

```text
logging.buffer.critical-reserve-used
```

Payload:

```text
LogCriticalReserveUsedPayload {
    bufferId
    severity
    reserveCapacity
    reserveRemaining
    usedAt
}
```

---

## 47. LogCriticalReserveExhausted

Event type:

```text
logging.buffer.critical-reserve-exhausted
```

Payload:

```text
LogCriticalReserveExhaustedPayload {
    bufferId
    bufferClass
    severity
    reserveCapacity
    emergencyPathAttempted
    exhaustedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

## 48. LogRecordsDropped

Event type:

```text
logging.buffer.records-dropped
```

Payload:

```text
LogRecordsDroppedPayload {
    bufferId
    timeWindow
    droppedBySeverity
    droppedByCategory
    totalDropped
    overflowPolicy
    droppedAt
}
```

This event must be aggregated and non-recursive.

---

## 49. LogBufferDrainStarted

Event type:

```text
logging.buffer.drain-started
```

Payload:

```text
LogBufferDrainStartedPayload {
    bufferId
    queueDepth
    deadline
    startedAt
}
```

---

## 50. LogBufferDrainCompleted

Event type:

```text
logging.buffer.drain-completed
```

Payload:

```text
LogBufferDrainCompletedPayload {
    bufferId
    outcome
    recordsDrained
    recordsRemaining
    completedAt
}
```

---

## 51. LogBufferFailed

Event type:

```text
logging.buffer.failed
```

Payload:

```text
LogBufferFailedPayload {
    bufferId
    bufferClass
    normalizedErrorCode
    queueDepth
    normalAdmissionBlocked
    failedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

# Part VI — Sink Lifecycle Events

## 52. LogSinkRegistered

Event type:

```text
logging.sink.registered
```

Payload:

```text
LogSinkRegisteredPayload {
    sinkId
    sinkType
    acceptedSecurityClassifications[]
    supportsRotation
    supportsPersistenceConfirmation
    registeredAt
}
```

---

## 53. LogSinkInitializationStarted

Event type:

```text
logging.sink.initialization-started
```

Payload:

```text
LogSinkInitializationStartedPayload {
    sinkId
    sinkType
    startedAt
}
```

---

## 54. LogSinkAvailable

Event type:

```text
logging.sink.available
```

Payload:

```text
LogSinkAvailablePayload {
    sinkId
    sinkType
    previousState
    currentState = AVAILABLE
    recovered
    availableAt
}
```

---

## 55. LogSinkDegraded

Event type:

```text
logging.sink.degraded
```

Payload:

```text
LogSinkDegradedPayload {
    sinkId
    sinkType
    previousState
    currentState = DEGRADED
    degradedCapabilities[]
    normalizedReasonCode
    fallbackActive
    degradedAt
}
```

---

## 56. LogSinkRecovered

Event type:

```text
logging.sink.recovered
```

Payload:

```text
LogSinkRecoveredPayload {
    sinkId
    sinkType
    previousState
    currentState = AVAILABLE
    recoveredCapabilities[]
    recoveredAt
}
```

---

## 57. LogSinkUnavailable

Event type:

```text
logging.sink.unavailable
```

Payload:

```text
LogSinkUnavailablePayload {
    sinkId
    sinkType
    previousState
    currentState = UNAVAILABLE
    normalizedErrorCode
    mandatory
    fallbackAvailable
    unavailableAt
}
```

---

## 58. LogSinkDisabled

Event type:

```text
logging.sink.disabled
```

Payload:

```text
LogSinkDisabledPayload {
    sinkId
    sinkType
    disableReason
    mandatory
    disabledAt
}
```

---

## 59. LogSinkStopping

Event type:

```text
logging.sink.stopping
```

Payload:

```text
LogSinkStoppingPayload {
    sinkId
    pendingBatchCount
    stoppingAt
}
```

---

## 60. LogSinkTerminated

Event type:

```text
logging.sink.terminated
```

Payload:

```text
LogSinkTerminatedPayload {
    sinkId
    finalState = TERMINATED
    finalWriteCount
    abandonedBatchCount
    terminatedAt
}
```

---

## 61. LogSinkFailed

Event type:

```text
logging.sink.failed
```

Payload:

```text
LogSinkFailedPayload {
    sinkId
    sinkType
    normalizedErrorCode
    mandatory
    classificationImpact[]
    failedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

# Part VII — Sink Health Events

## 62. LogSinkHealthChanged

Event type:

```text
logging.sink.health-changed
```

Payload:

```text
LogSinkHealthChangedPayload {
    sinkId
    previousHealth
    currentHealth
    writeLatencyClass?
    recentFailureCount?
    reasonCode
    changedAt
}
```

---

## 63. LogSinkSlowDetected

Event type:

```text
logging.sink.slow-detected
```

Payload:

```text
LogSinkSlowDetectedPayload {
    sinkId
    sinkType
    latencyClass
    configuredThreshold
    consecutiveSlowWrites
    detectedAt
}
```

---

## 64. LogSinkRecoveryStarted

Event type:

```text
logging.sink.recovery-started
```

Payload:

```text
LogSinkRecoveryStartedPayload {
    sinkId
    recoveryMode
    startedAt
}
```

---

# Part VIII — Batch Write Events

## 65. LogBatchWriteStarted

High-volume event.

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY
OBSERVABILITY_ONLY
```

Event type:

```text
logging.batch.write-started
```

Payload:

```text
LogBatchWriteStartedPayload {
    batchId
    sinkId
    recordCount
    estimatedBytes
    startedAt
}
```

---

## 66. LogBatchWritten

Event type:

```text
logging.batch.written
```

Payload:

```text
LogBatchWrittenPayload {
    batchId
    sinkId
    recordCount
    persistenceConfirmed
    writeDuration
    writtenAt
}
```

---

## 67. LogBatchPartiallyWritten

Event type:

```text
logging.batch.partially-written
```

Payload:

```text
LogBatchPartiallyWrittenPayload {
    batchId
    sinkId
    attemptedCount
    writtenCount
    failedCount
    retryBatchCreated
    normalizedErrorCode
    occurredAt
}
```

---

## 68. LogBatchWriteTimedOut

Event type:

```text
logging.batch.write-timed-out
```

Payload:

```text
LogBatchWriteTimedOutPayload {
    batchId
    sinkId
    recordCount
    timeout
    physicalWriteMayContinue
    timedOutAt
}
```

---

## 69. LogBatchWriteFailed

Event type:

```text
logging.batch.write-failed
```

Payload:

```text
LogBatchWriteFailedPayload {
    batchId
    sinkId
    recordCount
    normalizedErrorCode
    retryable
    retryScheduled
    failedAt
}
```

---

## 70. LogBatchAbandoned

Event type:

```text
logging.batch.abandoned
```

Payload:

```text
LogBatchAbandonedPayload {
    batchId
    sinkId
    recordCount
    abandonmentReason
    physicalWriteUnconfirmed
    abandonedAt
}
```

---

## 71. LogBatchLateCompletionObserved

Event type:

```text
logging.batch.late-completion-observed
```

Payload:

```text
LogBatchLateCompletionObservedPayload {
    batchId
    sinkId
    terminalState
    physicalCompletionObservedAt
}
```

Late completion is non-authoritative.

---

# Part IX — File Lifecycle Events

## 72. LogFileCreated

Event type:

```text
logging.file.created
```

Payload:

```text
LogFileCreatedPayload {
    logicalFileId
    sinkId
    fileClass
    fileSequence
    createdAt
}
```

Raw absolute path is excluded.

---

## 73. LogFileActivated

Event type:

```text
logging.file.activated
```

Payload:

```text
LogFileActivatedPayload {
    logicalFileId
    sinkId
    fileSequence
    previousActiveFileId?
    activatedAt
}
```

---

## 74. LogFileSealed

Event type:

```text
logging.file.sealed
```

Payload:

```text
LogFileSealedPayload {
    logicalFileId
    sinkId
    recordCount?
    fileSizeClass
    sealedAt
}
```

---

## 75. LogFileCorrupted

Event type:

```text
logging.file.corrupted
```

Payload:

```text
LogFileCorruptedPayload {
    logicalFileId
    sinkId
    corruptionClass
    activeFileAffected
    quarantineAction
    detectedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

## 76. LogFileOrphanedDetected

Event type:

```text
logging.file.orphaned-detected
```

Payload:

```text
LogFileOrphanedDetectedPayload {
    logicalFileId
    sinkId
    orphanClass
    reconciliationRequired
    detectedAt
}
```

---

## 77. LogFileRecovered

Event type:

```text
logging.file.recovered
```

Payload:

```text
LogFileRecoveredPayload {
    logicalFileId
    sinkId
    previousState
    currentState
    recoveryAction
    recoveredAt
}
```

---

## 78. LogFileDeleted

Event type:

```text
logging.file.deleted
```

Payload:

```text
LogFileDeletedPayload {
    logicalFileId
    sinkId
    deletionReason
    deletedAt
}
```

---

# Part X — Rotation Events

## 79. LogRotationStarted

Event type:

```text
logging.rotation.started
```

Payload:

```text
LogRotationStartedPayload {
    rotationId
    sinkId
    activeFileId
    reason
    deadline
    startedAt
}
```

---

## 80. LogRotationOldFileSealed

Event type:

```text
logging.rotation.old-file-sealed
```

Payload:

```text
LogRotationOldFileSealedPayload {
    rotationId
    sinkId
    sealedFileId
    sealedAt
}
```

---

## 81. LogRotationNewFileActivated

Event type:

```text
logging.rotation.new-file-activated
```

Payload:

```text
LogRotationNewFileActivatedPayload {
    rotationId
    sinkId
    previousFileId
    currentFileId
    currentFileSequence
    activatedAt
}
```

---

## 82. LogRotationCompleted

Event type:

```text
logging.rotation.completed
```

Payload:

```text
LogRotationCompletedPayload {
    rotationId
    sinkId
    previousFileId
    currentFileId
    compressionScheduled
    retentionCleanupScheduled
    completedAt
}
```

---

## 83. LogRotationPartiallyCompleted

Event type:

```text
logging.rotation.partially-completed
```

Payload:

```text
LogRotationPartiallyCompletedPayload {
    rotationId
    sinkId
    activeFileId?
    incompleteStage
    emergencyPathActive
    reconciliationRequired
    occurredAt
}
```

---

## 84. LogRotationNotRequired

Event type:

```text
logging.rotation.not-required
```

Payload:

```text
LogRotationNotRequiredPayload {
    rotationId
    sinkId
    reason
    evaluatedAt
}
```

---

## 85. LogRotationTimedOut

Event type:

```text
logging.rotation.timed-out
```

Payload:

```text
LogRotationTimedOutPayload {
    rotationId
    sinkId
    timeoutStage
    knownActiveFileId?
    activeFileAuthorityUncertain
    timedOutAt
}
```

---

## 86. LogRotationFailed

Event type:

```text
logging.rotation.failed
```

Payload:

```text
LogRotationFailedPayload {
    rotationId
    sinkId
    failureStage
    normalizedErrorCode
    previousFileStillUsable
    emergencyPathActive
    failedAt
}
```

---

## 87. LogRotationBecameUncertain

Event type:

```text
logging.rotation.became-uncertain
```

Payload:

```text
LogRotationBecameUncertainPayload {
    rotationId
    sinkId
    uncertaintyStage
    knownFileIds[]
    normalWritesPaused
    reconciliationRequired = true
    occurredAt
}
```

---

## 88. LogRotationReconciled

Event type:

```text
logging.rotation.reconciled
```

Payload:

```text
LogRotationReconciledPayload {
    rotationId
    sinkId
    resolution
    activeFileId?
    orphanedFileIds[]
    cleanupRequired
    reconciledAt
}
```

---

# Part XI — Retention Events

## 89. LogRetentionCleanupStarted

Event type:

```text
logging.retention.cleanup-started
```

Payload:

```text
LogRetentionCleanupStartedPayload {
    cleanupId
    sinkId
    policyRevision
    triggeredBy
    startedAt
}
```

---

## 90. LogRetentionCleanupCompleted

Event type:

```text
logging.retention.cleanup-completed
```

Payload:

```text
LogRetentionCleanupCompletedPayload {
    cleanupId
    sinkId
    examinedFiles
    deletedFiles
    retainedFiles
    bytesFreedClass?
    completedAt
}
```

---

## 91. LogRetentionCleanupPartiallyCompleted

Event type:

```text
logging.retention.cleanup-partially-completed
```

Payload:

```text
LogRetentionCleanupPartiallyCompletedPayload {
    cleanupId
    sinkId
    examinedFiles
    deletedFiles
    failedDeletionCount
    normalizedWarningCode
    occurredAt
}
```

---

## 92. LogRetentionCleanupFailed

Event type:

```text
logging.retention.cleanup-failed
```

Payload:

```text
LogRetentionCleanupFailedPayload {
    cleanupId
    sinkId
    normalizedErrorCode
    activeFilePreserved
    failedAt
}
```

---

## 93. LogRetentionUnknownFileSkipped

Event type:

```text
logging.retention.unknown-file-skipped
```

Payload:

```text
LogRetentionUnknownFileSkippedPayload {
    cleanupId
    sinkId
    unknownFileClass
    deletionSkipped = true
    detectedAt
}
```

---

# Part XII — Compression Events

## 94. LogCompressionStarted

Event type:

```text
logging.compression.started
```

Payload:

```text
LogCompressionStartedPayload {
    compressionId
    sinkId
    sourceFileId
    format
    startedAt
}
```

---

## 95. LogCompressionCompleted

Event type:

```text
logging.compression.completed
```

Payload:

```text
LogCompressionCompletedPayload {
    compressionId
    sinkId
    sourceFileId
    archiveFileId
    sourceDeleted
    completedAt
}
```

---

## 96. LogCompressionPartiallyCompleted

Event type:

```text
logging.compression.partially-completed
```

Payload:

```text
LogCompressionPartiallyCompletedPayload {
    compressionId
    sinkId
    sourceFileId
    archiveFileId
    archiveVerified
    sourceDeletionPending
    occurredAt
}
```

---

## 97. LogCompressionFailed

Event type:

```text
logging.compression.failed
```

Payload:

```text
LogCompressionFailedPayload {
    compressionId
    sinkId
    sourceFileId
    failureStage
    normalizedErrorCode
    sourcePreserved
    failedAt
}
```

---

# Part XIII — Diagnostics Query Events

## 98. LoggingDiagnosticsQueryStarted

Event type:

```text
logging.diagnostics.query-started
```

Payload:

```text
LoggingDiagnosticsQueryStartedPayload {
    queryId
    callerClearanceClass
    includeRestricted
    maximumRecords
    startedAt
}
```

---

## 99. LoggingDiagnosticsQueryCompleted

Event type:

```text
logging.diagnostics.query-completed
```

Payload:

```text
LoggingDiagnosticsQueryCompletedPayload {
    queryId
    outcome
    recordsReturned
    recordsExcluded
    restrictedRecordsIncluded
    completedAt
}
```

---

## 100. LoggingDiagnosticsQueryRejected

Event type:

```text
logging.diagnostics.query-rejected
```

Payload:

```text
LoggingDiagnosticsQueryRejectedPayload {
    queryId
    rejectionClass
    normalizedErrorCode
    rejectedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

# Part XIV — Diagnostics Export Events

## 101. DiagnosticsExportStarted

Event type:

```text
logging.diagnostics.export-started
```

Payload:

```text
DiagnosticsExportStartedPayload {
    exportId
    requestedByClass
    destinationType
    includeRestrictedSecurity
    includeAudit
    maximumBundleSizeClass
    startedAt
}
```

---

## 102. DiagnosticsExportRecordsRedacted

Event type:

```text
logging.diagnostics.export-records-redacted
```

Payload:

```text
DiagnosticsExportRecordsRedactedPayload {
    exportId
    examinedRecords
    redactedRecords
    excludedRecords
    findingCountsByClass
    redactedAt
}
```

---

## 103. DiagnosticsExportBundleInspected

Event type:

```text
logging.diagnostics.export-bundle-inspected
```

Payload:

```text
DiagnosticsExportBundleInspectedPayload {
    exportId
    outcome
    findingCount
    bundleSafe
    inspectedAt
}
```

---

## 104. DiagnosticsExportCompleted

Event type:

```text
logging.diagnostics.export-completed
```

Payload:

```text
DiagnosticsExportCompletedPayload {
    exportId
    bundleReference
    manifestReference
    recordsIncluded
    recordsExcluded
    includesRestrictedData
    includesAuditData
    completedAt
}
```

`bundleReference` must be a safe logical reference, not unrestricted raw path.

---

## 105. DiagnosticsExportPartiallyCompleted

Event type:

```text
logging.diagnostics.export-partially-completed
```

Payload:

```text
DiagnosticsExportPartiallyCompletedPayload {
    exportId
    bundleReference?
    omittedSections[]
    normalizedWarningCode
    occurredAt
}
```

---

## 106. DiagnosticsExportBlocked

Event type:

```text
logging.diagnostics.export-blocked
```

Payload:

```text
DiagnosticsExportBlockedPayload {
    exportId
    blockStage
    findingClasses[]
    destinationType
    unsafeOutputPrevented = true
    blockedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
AUDIT_ONLY
```

---

## 107. DiagnosticsExportFailed

Event type:

```text
logging.diagnostics.export-failed
```

Payload:

```text
DiagnosticsExportFailedPayload {
    exportId
    failureStage
    normalizedErrorCode
    partialBundleRemoved
    failedAt
}
```

---

## 108. DiagnosticsExportCanceled

Event type:

```text
logging.diagnostics.export-canceled
```

Payload:

```text
DiagnosticsExportCanceledPayload {
    exportId
    cancellationStage
    partialBundleRemoved
    canceledAt
}
```

---

# Part XV — Audit Events

## 109. AuditWriteStarted

Event type:

```text
logging.audit.write-started
```

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY
AUDIT_ONLY
```

Payload:

```text
AuditWriteStartedPayload {
    auditRecordId
    actionType
    targetType
    requirePersistenceConfirmation
    startedAt
}
```

---

## 110. AuditRecordWritten

Event type:

```text
logging.audit.record-written
```

Payload:

```text
AuditRecordWrittenPayload {
    auditRecordId
    actionType
    targetType
    outcome
    persistenceConfirmed
    writtenAt
}
```

Visibility:

```text
AUDIT_ONLY
```

---

## 111. AuditRecordRejectedUnsafe

Event type:

```text
logging.audit.record-rejected-unsafe
```

Payload:

```text
AuditRecordRejectedUnsafePayload {
    auditRecordId?
    actionType?
    findingClasses[]
    blockedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
AUDIT_ONLY
```

---

## 112. AuditSinkUnavailable

Event type:

```text
logging.audit.sink-unavailable
```

Payload:

```text
AuditSinkUnavailablePayload {
    auditRecordId
    actionType
    mandatory
    emergencyAuditAvailable
    failureMode
    unavailableAt
}
```

---

## 113. AuditWriteTimedOut

Event type:

```text
logging.audit.write-timed-out
```

Payload:

```text
AuditWriteTimedOutPayload {
    auditRecordId
    actionType
    timeout
    persistenceOutcomeUncertain
    timedOutAt
}
```

---

## 114. AuditWriteFailed

Event type:

```text
logging.audit.write-failed
```

Payload:

```text
AuditWriteFailedPayload {
    auditRecordId
    actionType
    normalizedErrorCode
    mandatory
    owningActionBlocked
    failedAt
}
```

---

## 115. AuditWriteBecameUncertain

Event type:

```text
logging.audit.write-became-uncertain
```

Payload:

```text
AuditWriteBecameUncertainPayload {
    auditRecordId
    actionType
    uncertaintyStage
    duplicatePossible
    reconciliationRequired
    occurredAt
}
```

---

## 116. AuditWriteReconciled

Event type:

```text
logging.audit.write-reconciled
```

Payload:

```text
AuditWriteReconciledPayload {
    auditRecordId
    actionType
    resolution
    persistenceConfirmed
    duplicateDetected
    reconciledAt
}
```

---

# Part XVI — Bootstrap Logger Events

## 117. BootstrapLoggerAvailable

Event type:

```text
logging.bootstrap.available
```

Payload:

```text
BootstrapLoggerAvailablePayload {
    bootstrapLoggerId
    destinationClass
    availableAt
}
```

---

## 118. BootstrapLoggerHandoffStarted

Event type:

```text
logging.bootstrap.handoff-started
```

Payload:

```text
BootstrapLoggerHandoffStartedPayload {
    bootstrapLoggerId
    targetLoggingInstanceId
    bufferedRecordCount
    startedAt
}
```

---

## 119. BootstrapLoggerHandoffCompleted

Event type:

```text
logging.bootstrap.handoff-completed
```

Payload:

```text
BootstrapLoggerHandoffCompletedPayload {
    bootstrapLoggerId
    targetLoggingInstanceId
    transferredRecordCount
    excludedRecordCount
    completedAt
}
```

---

## 120. BootstrapLoggerFailed

Event type:

```text
logging.bootstrap.failed
```

Payload:

```text
BootstrapLoggerFailedPayload {
    bootstrapLoggerId
    normalizedErrorCode
    emergencyPathAvailable
    failedAt
}
```

---

# Part XVII — Emergency Logger Events

## 121. EmergencyLoggerAvailable

Event type:

```text
logging.emergency.available
```

Payload:

```text
EmergencyLoggerAvailablePayload {
    emergencyLoggerId
    destinationClass
    availableAt
}
```

---

## 122. EmergencyLoggerUsed

Event type:

```text
logging.emergency.used
```

Payload:

```text
EmergencyLoggerUsedPayload {
    emergencyLoggerId
    triggerClass
    severity
    category
    normalPipelineState
    usedAt
}
```

This event must use a non-recursive reporting path.

---

## 123. EmergencyLoggerDegraded

Event type:

```text
logging.emergency.degraded
```

Payload:

```text
EmergencyLoggerDegradedPayload {
    emergencyLoggerId
    degradedCapabilities[]
    normalizedReasonCode
    degradedAt
}
```

---

## 124. EmergencyLoggerUnavailable

Event type:

```text
logging.emergency.unavailable
```

Payload:

```text
EmergencyLoggerUnavailablePayload {
    emergencyLoggerId
    normalizedErrorCode
    criticalFallbackLost
    unavailableAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

## 125. EmergencyWriteFailed

Event type:

```text
logging.emergency.write-failed
```

Payload:

```text
EmergencyWriteFailedPayload {
    emergencyLoggerId
    triggerClass
    normalizedErrorCode
    unsafeContentBlocked
    failedAt
}
```

This event must not depend on the emergency logger itself.

---

# Part XVIII — Configuration Events

## 126. LoggingConfigurationApplied

Event type:

```text
logging.configuration.applied
```

Payload:

```text
LoggingConfigurationAppliedPayload {
    configurationRevision
    previousPolicyRevision
    currentPolicyRevision
    liveChanges[]
    restartRequiredChanges[]
    appliedAt
}
```

---

## 127. LoggingConfigurationRejected

Event type:

```text
logging.configuration.rejected
```

Payload:

```text
LoggingConfigurationRejectedPayload {
    configurationRevision
    normalizedErrorCode
    previousConfigurationRetained
    rejectedAt
}
```

---

## 128. LoggingRestartRequired

Event type:

```text
logging.configuration.restart-required
```

Payload:

```text
LoggingRestartRequiredPayload {
    configurationRevision
    changeClasses[]
    currentPipelineRemainsActive
    detectedAt
}
```

---

# Part XIX — Consumed Events

## 129. Events Consumed by Logging

Logging may consume safe lifecycle and health facts from other modules.

It must not consume module events in order to mutate their domain state.

---

## 130. ApplicationShutdownStarted

Potential source:

```text
application.shutdown.started
```

Reaction:

```text
RUNNING / DEGRADED
    ↓
QUIESCING
    ↓
DRAINING
    ↓
FLUSHING
```

Logging should shut down after Event Bus and most feature modules.

---

## 131. ConfigurationSnapshotActivated

Potential source:

```text
configuration.snapshot.activated
```

Logging may evaluate:

- severity;
- categories;
- buffer size;
- sink enablement;
- rotation;
- retention;
- diagnostics;
- audit settings.

Unsafe or restart-required changes do not apply silently.

---

## 132. SecretExposureBlocked

Potential source:

```text
secret-management.security.exposure-blocked
```

Logging records only the safe event metadata.

It must not inspect or request the blocked material.

---

## 133. EventBusFailed

Potential source:

```text
event-bus.lifecycle.failed
```

Logging records the safe failure and remains capable of emergency reporting without Event Bus.

---

## 134. EventBusDrainStarted

Potential source:

```text
event-bus.lifecycle.drain-started
```

Logging may prepare for shutdown ordering.

It should not stop normal intake before higher-level shutdown policy requires it.

---

## 135. TelemetryUnavailable

Potential source:

```text
telemetry.lifecycle.unavailable
```

Logging may record safe telemetry degradation.

Telemetry failure must not block ordinary logging.

---

## 136. StoragePressureChanged

Potential source:

```text
storage.pressure.changed
```

Logging may respond by:

- rotating early;
- reducing retention;
- disabling compression;
- increasing sampling;
- warning;
- protecting security/audit minimum retention.

---

# Part XX — Event Ordering

## 137. Logging Lifecycle Ordering

Expected startup sequence:

```text
LoggingBootstrapStarted
    ↓
LoggingInitializationStarted
    ↓
LoggingReady
    ↓
LoggingStarted
```

Shutdown sequence:

```text
LoggingQuiescing
    ↓
LoggingDrainStarted
    ↓
LoggingDrainCompleted
    ↓
LoggingFlushStarted
    ↓
LoggingFlushCompleted
    ↓
LoggingStopping
    ↓
LoggingTerminated
```

---

## 138. Sink Lifecycle Ordering

```text
LogSinkRegistered
    ↓
LogSinkInitializationStarted
    ↓
LogSinkAvailable / LogSinkDegraded / LogSinkUnavailable
    ↓
LogSinkStopping
    ↓
LogSinkTerminated
```

---

## 139. Rotation Ordering

```text
LogRotationStarted
    ↓
LogRotationOldFileSealed
    ↓
LogRotationNewFileActivated
    ↓
LogRotationCompleted
```

Failure alternatives may replace later events:

```text
LogRotationFailed
LogRotationTimedOut
LogRotationBecameUncertain
```

---

## 140. Audit Ordering

```text
AuditWriteStarted
    ↓
one terminal outcome
```

Terminal outcomes:

```text
AuditRecordWritten
AuditRecordRejectedUnsafe
AuditSinkUnavailable
AuditWriteTimedOut
AuditWriteFailed
AuditWriteBecameUncertain
```

`AuditWriteBecameUncertain` may later be followed by `AuditWriteReconciled`.

---

# Part XXI — Duplicate and Stale Handling

## 141. Duplicate Self-Events

Consumers must deduplicate using:

```text
eventId
```

or:

```text
entityId + stateVersion + eventType
```

---

## 142. Out-of-Order Self-Events

Consumers compare:

- state version;
- policy revision;
- sink identity;
- file sequence;
- occurredAt.

A delayed `LogSinkDegraded` must not overwrite a newer `LogSinkUnavailable` or `LogSinkRecovered`.

---

## 143. Late Write Completion

When a batch is already terminal as:

```text
TIMED_OUT
ABANDONED
FAILED
```

a late physical completion cannot publish `LogBatchWritten` as authoritative.

It may publish:

```text
LogBatchLateCompletionObserved
```

---

## 144. Rotation Revision

Rotation consumers use:

```text
rotationId
sinkId
fileSequence
```

to prevent an older rotation event from replacing the current active-file view.

---

# Part XXII — Publication and Sampling Rules

## 145. Self-Event Admission

Logging self-events should use:

- guarded Event Bus publication;
- reserved internal capacity;
- payload-free metadata;
- emergency fallback;
- recursion guard.

---

## 146. High-Volume Events

May be sampled, aggregated, or metrics-only:

```text
LogRecordAccepted
LogRecordFiltered
LogRecordSampledOut
LogRecordSuppressed
LogSafetyInspectionCompleted
LogBatchWriteStarted
LogBatchWritten
```

---

## 147. Events That Must Not Be Sampled Away

```text
LoggingFailed
LogPolicyInvalidated
LogRecordBlocked
SecretMaterialLoggingAttemptBlocked
ClassificationDowngradeBlocked
LogCriticalReserveExhausted
LogBufferFailed
LogSinkFailed
LogFileCorrupted
LogRotationBecameUncertain
DiagnosticsExportBlocked
AuditSinkUnavailable
AuditWriteFailed
EmergencyLoggerUnavailable
```

---

## 148. Coalescing Rules

Allowed for repeated pressure or success observations:

```text
LogBufferBackpressured
LogSinkSlowDetected
LogRecordsDropped
```

Not allowed for:

```text
security incidents
audit outcomes
lifecycle terminal events
rotation activation
diagnostics export completion
```

---

# Part XXIII — Observability

## 149. Metrics Mapping

Self-events may feed:

```text
logging_lifecycle_transition_total
logging_records_blocked_total
logging_records_dropped_total
logging_records_sampled_total
logging_records_suppressed_total
logging_buffer_backpressure_total
logging_critical_reserve_exhausted_total
logging_sink_failures_total
logging_batch_write_failures_total
logging_rotation_failures_total
logging_flush_outcomes_total
logging_retention_failures_total
logging_export_outcomes_total
logging_audit_write_failures_total
logging_emergency_path_use_total
```

---

## 150. Safe Logging of Logging Events

Logging self-events must not be written as full ordinary log records through an unguarded path.

Allowed safe fields:

```text
selfEventType
recordId
sinkId
bufferId
batchId
rotationId
flushId
exportId
auditRecordId
normalizedErrorCode
stateTransition
policyRevision
correlationId
```

---

## 151. Tracing

Logging events may annotate Telemetry spans:

```text
logging.lifecycle
logging.record
logging.buffer
logging.sink
logging.rotation
logging.flush
logging.export
logging.audit
```

No original log content is attached.

---

# Part XXIV — Security Validation

## 152. Pre-Publication Validation

Every Logging self-event passes:

```text
schema validation
    ↓
bounded payload validation
    ↓
original-record-content rejection
    ↓
secret and user-content inspection
    ↓
visibility validation
    ↓
recursion guard
    ↓
publication
```

---

## 153. Prohibited Event Fields

Logging self-events must never include:

```text
messageTemplate
renderedMessage
propertyValues
exceptionMessage
stackTrace
userText
OCRText
translatedText
providerResponse
secretValue
authorizationHeader
rawFilePath
```

A safe logical file reference may be included.

---

## 154. Restricted Export Rule

Events about diagnostics export or audit operations must not reveal:

- destination path;
- actor personal details;
- bundle contents;
- restricted record values.

---

# Part XXV — Event Catalog Summary

## 155. Lifecycle Events

```text
LoggingBootstrapStarted
LoggingInitializationStarted
LoggingReady
LoggingStarted
LoggingDegraded
LoggingRecovered
LoggingQuiescing
LoggingDrainStarted
LoggingDrainCompleted
LoggingFlushStarted
LoggingFlushCompleted
LoggingStopping
LoggingTerminated
LoggingFailed
```

## 156. Policy Events

```text
LogPolicyValidationStarted
LogPolicyActivated
LogPolicySuperseded
LogPolicyRejected
LogPolicyInvalidated
```

## 157. Record and Safety Events

```text
LogRecordAccepted
LogRecordFiltered
LogRecordSampledOut
LogRecordSuppressed
LogSuppressionSummaryCreated
LogRecordBlocked
LogRecordCapacityRejected
LogRecordAdmissionTimedOut
LogRecordEmergencyWritten
LogRecordFailedSafe
LogSafetyInspectionCompleted
LogRedactionApplied
LogRedactionBlocked
LogRedactionFailedSafe
SecretMaterialLoggingAttemptBlocked
UserContentLoggingAttemptBlocked
ClassificationDowngradeBlocked
```

## 158. Buffer Events

```text
LogBufferInitialized
LogBufferAvailable
LogBufferBackpressured
LogBufferRecovered
LogCriticalReserveUsed
LogCriticalReserveExhausted
LogRecordsDropped
LogBufferDrainStarted
LogBufferDrainCompleted
LogBufferFailed
```

## 159. Sink and Batch Events

```text
LogSinkRegistered
LogSinkInitializationStarted
LogSinkAvailable
LogSinkDegraded
LogSinkRecovered
LogSinkUnavailable
LogSinkDisabled
LogSinkStopping
LogSinkTerminated
LogSinkFailed
LogSinkHealthChanged
LogSinkSlowDetected
LogSinkRecoveryStarted
LogBatchWriteStarted
LogBatchWritten
LogBatchPartiallyWritten
LogBatchWriteTimedOut
LogBatchWriteFailed
LogBatchAbandoned
LogBatchLateCompletionObserved
```

## 160. File and Rotation Events

```text
LogFileCreated
LogFileActivated
LogFileSealed
LogFileCorrupted
LogFileOrphanedDetected
LogFileRecovered
LogFileDeleted
LogRotationStarted
LogRotationOldFileSealed
LogRotationNewFileActivated
LogRotationCompleted
LogRotationPartiallyCompleted
LogRotationNotRequired
LogRotationTimedOut
LogRotationFailed
LogRotationBecameUncertain
LogRotationReconciled
```

## 161. Retention and Compression Events

```text
LogRetentionCleanupStarted
LogRetentionCleanupCompleted
LogRetentionCleanupPartiallyCompleted
LogRetentionCleanupFailed
LogRetentionUnknownFileSkipped
LogCompressionStarted
LogCompressionCompleted
LogCompressionPartiallyCompleted
LogCompressionFailed
```

## 162. Diagnostics and Audit Events

```text
LoggingDiagnosticsQueryStarted
LoggingDiagnosticsQueryCompleted
LoggingDiagnosticsQueryRejected
DiagnosticsExportStarted
DiagnosticsExportRecordsRedacted
DiagnosticsExportBundleInspected
DiagnosticsExportCompleted
DiagnosticsExportPartiallyCompleted
DiagnosticsExportBlocked
DiagnosticsExportFailed
DiagnosticsExportCanceled
AuditWriteStarted
AuditRecordWritten
AuditRecordRejectedUnsafe
AuditSinkUnavailable
AuditWriteTimedOut
AuditWriteFailed
AuditWriteBecameUncertain
AuditWriteReconciled
```

## 163. Bootstrap, Emergency, and Configuration Events

```text
BootstrapLoggerAvailable
BootstrapLoggerHandoffStarted
BootstrapLoggerHandoffCompleted
BootstrapLoggerFailed
EmergencyLoggerAvailable
EmergencyLoggerUsed
EmergencyLoggerDegraded
EmergencyLoggerUnavailable
EmergencyWriteFailed
LoggingConfigurationApplied
LoggingConfigurationRejected
LoggingRestartRequired
```

---

# Part XXVI — MVP Event Boundary

## 164. Required MVP Events

The MVP should implement:

```text
LoggingStarted
LoggingDegraded
LoggingRecovered
LoggingQuiescing
LoggingDrainStarted
LoggingDrainCompleted
LoggingFlushCompleted
LoggingTerminated
LoggingFailed

LogPolicyActivated
LogPolicyRejected
LogPolicyInvalidated

LogRecordBlocked
LogRecordCapacityRejected
LogRecordEmergencyWritten
LogRecordFailedSafe

LogRedactionApplied
LogRedactionBlocked
LogRedactionFailedSafe
SecretMaterialLoggingAttemptBlocked
UserContentLoggingAttemptBlocked
ClassificationDowngradeBlocked

LogBufferBackpressured
LogBufferRecovered
LogCriticalReserveExhausted
LogRecordsDropped
LogBufferFailed

LogSinkAvailable
LogSinkDegraded
LogSinkRecovered
LogSinkUnavailable
LogSinkFailed

LogBatchPartiallyWritten
LogBatchWriteTimedOut
LogBatchWriteFailed
LogBatchAbandoned

LogFileCorrupted
LogFileOrphanedDetected
LogRotationCompleted
LogRotationPartiallyCompleted
LogRotationFailed
LogRotationBecameUncertain
LogRotationReconciled

LogRetentionCleanupFailed

DiagnosticsExportCompleted
DiagnosticsExportBlocked
DiagnosticsExportFailed

AuditRecordWritten
AuditRecordRejectedUnsafe
AuditSinkUnavailable
AuditWriteFailed

BootstrapLoggerHandoffCompleted
BootstrapLoggerFailed
EmergencyLoggerUsed
EmergencyLoggerUnavailable
EmergencyWriteFailed
```

---

## 165. Optional MVP Events

May remain metrics-only:

```text
LogRecordAccepted
LogRecordFiltered
LogRecordSampledOut
LogRecordSuppressed
LogSafetyInspectionCompleted
LogBufferInitialized
LogBatchWriteStarted
LogBatchWritten
LogFileCreated
LogRetentionCleanupStarted
LoggingDiagnosticsQueryStarted
```

---

## 166. Deferred Events

May be deferred:

```text
remote sink events
platform-native sink events
encrypted archive events
advanced compression scheduling
tamper-evident audit events
cross-device diagnostics events
automatic support-upload events
```

---

# Part XXVII — Event Decisions

## 167. Decisions

### Decision 1 — Self-events never contain original log data

Only safe identity, state, counts, classifications, and reason codes are permitted.

### Decision 2 — Logging self-reporting is guarded

Critical internal failures must not create recursive Logging or Event Bus loops.

### Decision 3 — Record acceptance and sink persistence remain distinct

`LogRecordAccepted` does not imply every sink persisted the record.

### Decision 4 — Security blocks are explicit

Secret, user-content, and classification-downgrade attempts produce restricted facts.

### Decision 5 — High-volume success events are optional

Metrics may replace per-record success events.

### Decision 6 — Critical failures are never sampled away

Safety, audit, active-file authority, and emergency-path failures remain explicit.

### Decision 7 — Late write completion is non-authoritative

It cannot overwrite timeout or abandonment.

### Decision 8 — Rotation uncertainty is explicit

Unknown active-file authority requires reconciliation.

### Decision 9 — Diagnostics export is observable

Second-pass redaction, full-bundle inspection, and blocking are explicit facts.

### Decision 10 — Audit outcomes remain separate

Ordinary logging events cannot substitute for audit events.

---

# Part XXVIII — Open Decisions

## 168. Visibility Decisions

Still to finalize:

- which lifecycle events are public;
- exact restricted subscribers;
- whether sink identities are public internal;
- audit event duplication rules;
- diagnostics export event visibility.

---

## 169. Sampling Decisions

Still to finalize:

- record-success sampling;
- sink-success aggregation;
- buffer-recovery throttle;
- suppression-summary cadence;
- batch-success sampling.

---

## 170. Security Decisions

Still to finalize:

- exact guarded self-reporting path;
- classification downgrade escalation;
- secret-attempt audit retention;
- user-content attempt handling;
- emergency failure reporting when Event Bus and Logging both fail.

---

## 171. File and Audit Decisions

Still to finalize:

- file-recovery event granularity;
- compression event retention;
- audit uncertainty events;
- emergency audit path events;
- tamper-evident audit events.

---

# Part XXIX — Related Documents

## 172. Related Documents

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
03-infrastructure/logging/STATES.md

03-infrastructure/secret-management/EVENTS.md
03-infrastructure/secret-management/ERRORS.md

03-infrastructure/event-bus/CONTRACT.md
03-infrastructure/event-bus/STATES.md
03-infrastructure/event-bus/EVENTS.md
03-infrastructure/event-bus/ERRORS.md
```

Future Logging documents:

```text
03-infrastructure/logging/ERRORS.md
03-infrastructure/logging/README.md
```

---

## 173. Summary

Logging self-events expose safe lifecycle and operational facts without copying original log content.

The main lifecycle flow is:

```text
LoggingBootstrapStarted
    ↓
LoggingInitializationStarted
    ↓
LoggingReady
    ↓
LoggingStarted
    ↓
LoggingQuiescing
    ↓
LoggingDrainStarted
    ↓
LoggingDrainCompleted
    ↓
LoggingFlushCompleted
    ↓
LoggingTerminated
```

The record-safety flow is:

```text
LogRecordAccepted
or
LogRecordFiltered / SampledOut / Suppressed
or
LogRecordBlocked / FailedSafe / EmergencyWritten
```

The sink and file flow is:

```text
LogSinkAvailable
    ↓
LogBatchWritten / Failed
    ↓
LogRotationStarted
    ↓
LogRotationNewFileActivated
    ↓
LogRotationCompleted
```

The event model guarantees:

- immutable past-tense facts;
- state commits before publication;
- no original log records, messages, properties, exceptions, or stack traces;
- no secrets or user content;
- guarded non-recursive reporting;
- record acceptance and persistence remain distinct;
- security blocks use restricted visibility;
- high-volume success events may be metrics-only;
- critical failures are never sampled away;
- late completion cannot rewrite terminal write state;
- rotation uncertainty is explicit;
- diagnostics export safety steps are observable;
- audit outcomes remain independent.

This document is the event source of truth for subsequent Logging errors and implementation documentation.
