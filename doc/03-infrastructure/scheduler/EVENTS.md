# Scheduler Events

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Scheduler  
> **Document:** Integration and Internal Events  
> **Path:** `03-infrastructure/scheduler/EVENTS.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/scheduler/MODULE.md`
> - `03-infrastructure/scheduler/CONTRACT.md`
> - `03-infrastructure/scheduler/STATES.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/WORK_QUEUE.md`
> - `docs/architecture/runtime/SCHEDULER.md`
> - `docs/architecture/runtime/CANCELLATION.md`
> - `docs/architecture/runtime/RETRY_POLICY.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `03-infrastructure/event-bus/EVENTS.md`
> - `03-infrastructure/logging/EVENTS.md`
> - `03-infrastructure/telemetry/EVENTS.md`

---

## 1. Purpose

This document defines events published and consumed by the Scheduler infrastructure module.

These events communicate safe operational facts concerning:

- Scheduler startup, degradation, recovery, quiesce, drain, and shutdown;
- task registration, update, enablement, pause, disablement, and removal;
- schedule registration, activation, pause, expiration, and removal;
- trigger arming, firing, exhaustion, and failure;
- recurring occurrences, overlap decisions, and misfires;
- job creation, validation, admission, waiting, dispatch, execution, and completion;
- attempt lifecycle, timeout, cancellation, abandonment, interruption, and uncertainty;
- queue pressure, admission rejection, claim, drain, and failure;
- worker registration, health, degradation, recovery, and termination;
- concurrency-lease and resource-reservation lifecycle;
- retry evaluation and scheduling;
- cancellation propagation;
- persistence health and durable-state changes;
- recovery and reconciliation;
- diagnostics and configuration changes.

These are Scheduler infrastructure events.

They do not redefine business events owned by OCR, Translation, Provider Management, Storage, Logging, Telemetry, or other CRAI modules.

---

## 2. Event Principles

### 2.1 Events represent committed facts

Correct:

```text
SchedulerStarted
TaskRegistered
TriggerFired
JobQueued
JobSucceeded
JobTimedOut
```

Incorrect:

```text
StartScheduler
RegisterTask
FireTrigger
QueueJob
CompleteJob
```

### 2.2 State commits before publication

```text
Scheduler-owned state transition
    ↓
state committed
    ↓
event published
```

### 2.3 Events are immutable

Published events are never modified.

Corrections, recovery, or reconciliation create new events.

### 2.4 Events do not contain job payloads

Scheduler events may contain:

```text
taskId
scheduleId
triggerId
occurrenceId
jobId
attemptId
workerId
priority
state
normalizedErrorCode
resourceClass
```

They must not contain:

- raw job input;
- raw job output;
- OCR text;
- translated text;
- prompts;
- provider request or response bodies;
- image bytes;
- secrets;
- credentials;
- authorization headers.

### 2.5 Job events do not imply business success

```text
JobSucceeded
```

means the Scheduler worker contract returned a successful terminal result.

The owning module remains authoritative for business state.

### 2.6 Attempt events and job events remain distinct

An attempt may fail and be retried while the logical job remains non-terminal.

### 2.7 High-volume events may be sampled

Queue and progress observations may be sampled or aggregated.

Terminal job, attempt, security, shutdown, and recovery events must not be sampled away.

---

## 3. Event Naming

Canonical event type:

```text
scheduler.<entity>.<past-tense-fact>
```

Examples:

```text
scheduler.lifecycle.started
scheduler.task.registered
scheduler.trigger.fired
scheduler.job.queued
scheduler.attempt.timed-out
scheduler.worker.degraded
```

---

## 4. Event Envelope

Scheduler events use the shared CRAI envelope:

```text
EventEnvelope<TPayload> {
    eventId
    eventType
    eventVersion
    category

    occurredAt
    publishedAt

    sourceModule = "scheduler"
    sourceComponent?
    publisherId

    correlationId
    causationId?

    applicationInstanceId

    taskId?
    scheduleId?
    triggerId?
    occurrenceId?
    jobId?
    attemptId?
    workerId?

    ordering
    priority
    visibility
    securityClassification

    payload
    metadata
}
```

---

## 5. Event Categories

Recommended categories:

```text
SYSTEM
INTEGRATION
RESULT
PROGRESS
SECURITY
AUDIT
OBSERVABILITY
```

Scheduler command requests are not modeled as normal events.

---

## 6. Event Visibility

Typical visibility:

```text
PUBLIC_INTERNAL
MODULE_INTERNAL
OBSERVABILITY_ONLY
RESTRICTED_SECURITY
AUDIT_ONLY
LOCAL_COMPONENT_ONLY
```

Recommended use:

```text
Scheduler lifecycle
    → PUBLIC_INTERNAL / OBSERVABILITY_ONLY

Task and schedule administration
    → MODULE_INTERNAL / AUDIT_ONLY

Job lifecycle
    → MODULE_INTERNAL / OBSERVABILITY_ONLY

Security and authorization failures
    → RESTRICTED_SECURITY

High-volume queue observations
    → LOCAL_COMPONENT_ONLY / OBSERVABILITY_ONLY
```

---

# Part I — Scheduler Lifecycle Events

## 7. SchedulerInitializationStarted

Event type:

```text
scheduler.lifecycle.initialization-started
```

Payload:

```text
SchedulerInitializationStartedPayload {
    schedulerInstanceId
    persistenceMode
    configuredQueueCount
    configuredWorkerCount
    recoveryRequired
    startedAt
}
```

---

## 8. SchedulerRecoveryStarted

Event type:

```text
scheduler.lifecycle.recovery-started
```

Payload:

```text
SchedulerRecoveryStartedPayload {
    schedulerInstanceId
    recoveryOperationId
    persistenceMode
    startedAt
}
```

---

## 9. SchedulerReady

Published after Scheduler reaches `READY`.

Event type:

```text
scheduler.lifecycle.ready
```

Payload:

```text
SchedulerReadyPayload {
    schedulerInstanceId
    registeredTaskCount
    restoredScheduleCount
    activeWorkerCount
    degradedComponents[]
    readyAt
}
```

---

## 10. SchedulerStarted

Published after Scheduler enters `RUNNING`.

Event type:

```text
scheduler.lifecycle.started
```

Payload:

```text
SchedulerStartedPayload {
    schedulerInstanceId
    executionMode
    persistenceMode
    registeredTaskCount
    activeScheduleCount
    activeWorkerCount
    startedAt
}
```

Expected MVP mode:

```text
LOCAL_IN_MEMORY
```

Visibility:

```text
PUBLIC_INTERNAL
```

---

## 11. SchedulerDegraded

Event type:

```text
scheduler.lifecycle.degraded
```

Payload:

```text
SchedulerDegradedPayload {
    previousState
    currentState = DEGRADED
    degradedComponents[]
    capabilityImpact[]
    normalizedReasonCode
    degradedAt
}
```

---

## 12. SchedulerRecovered

Event type:

```text
scheduler.lifecycle.recovered
```

Payload:

```text
SchedulerRecoveredPayload {
    previousState = DEGRADED
    currentState = RUNNING
    recoveredComponents[]
    recoveredAt
}
```

---

## 13. SchedulerQuiescing

Event type:

```text
scheduler.lifecycle.quiescing
```

Payload:

```text
SchedulerQuiescingPayload {
    previousState
    currentState = QUIESCING
    allowedPriorities[]
    allowedTaskIds[]
    stopRecurringTriggers
    reasonCode
    effectiveAt
}
```

Visibility:

```text
PUBLIC_INTERNAL
```

---

## 14. SchedulerDrainStarted

Event type:

```text
scheduler.lifecycle.drain-started
```

Payload:

```text
SchedulerDrainStartedPayload {
    drainOperationId
    readyQueueDepth
    delayedQueueDepth
    retryQueueDepth
    runningAttemptCount
    deadline
    startedAt
}
```

---

## 15. SchedulerDrainCompleted

Event type:

```text
scheduler.lifecycle.drain-completed
```

Payload:

```text
SchedulerDrainCompletedPayload {
    drainOperationId
    outcome
    jobsCompleted
    jobsCanceled
    jobsAbandoned
    jobsRemaining
    completedAt
}
```

Possible outcomes:

```text
DRAINED
PARTIALLY_DRAINED
TIMED_OUT
FAILED
CANCELED
```

---

## 16. SchedulerStopping

Event type:

```text
scheduler.lifecycle.stopping
```

Payload:

```text
SchedulerStoppingPayload {
    previousState
    currentState = STOPPING
    runningAttemptCount
    pendingJobCount
    persistenceRequested
    reasonCode
    stoppingAt
}
```

---

## 17. SchedulerTerminated

Event type:

```text
scheduler.lifecycle.terminated
```

Payload:

```text
SchedulerTerminatedPayload {
    finalState = TERMINATED
    jobsCompleted
    jobsCanceled
    jobsAbandoned
    jobsPersisted
    jobsLost
    workersAbandoned
    terminatedAt
}
```

Visibility:

```text
PUBLIC_INTERNAL
```

---

## 18. SchedulerFailed

Event type:

```text
scheduler.lifecycle.failed
```

Payload:

```text
SchedulerFailedPayload {
    previousState
    currentState = FAILED
    failedComponent
    normalizedErrorCode
    admissionBlocked
    shutdownRequired
    failedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
PUBLIC_INTERNAL safe projection
```

---

# Part II — Task Events

## 19. TaskValidationStarted

Event type:

```text
scheduler.task.validation-started
```

Payload:

```text
TaskValidationStartedPayload {
    taskId
    ownerModule
    candidateVersion
    validationMode
    startedAt
}
```

Visibility:

```text
OBSERVABILITY_ONLY
```

---

## 20. TaskRegistered

Event type:

```text
scheduler.task.registered
```

Payload:

```text
TaskRegisteredPayload {
    taskId
    taskVersion
    ownerModule
    handlerId
    defaultPriority
    persistenceMode
    registeredAt
}
```

Visibility:

```text
MODULE_INTERNAL
AUDIT_ONLY when administrative
```

---

## 21. TaskEnabled

Event type:

```text
scheduler.task.enabled
```

Payload:

```text
TaskEnabledPayload {
    taskId
    taskVersion
    previousState
    currentState = ENABLED
    enabledAt
}
```

---

## 22. TaskPauseStarted

Event type:

```text
scheduler.task.pause-started
```

Payload:

```text
TaskPauseStartedPayload {
    taskId
    activeJobCount
    pendingJobCount
    runningJobPolicy
    startedAt
}
```

---

## 23. TaskPaused

Event type:

```text
scheduler.task.paused
```

Payload:

```text
TaskPausedPayload {
    taskId
    previousState
    currentState = PAUSED
    pendingJobsAffected
    runningJobsAffected
    pausedAt
}
```

---

## 24. TaskDisabled

Event type:

```text
scheduler.task.disabled
```

Payload:

```text
TaskDisabledPayload {
    taskId
    previousState
    currentState = DISABLED
    pendingJobsAffected
    runningJobsAffected
    disabledAt
}
```

---

## 25. TaskUpdated

Event type:

```text
scheduler.task.updated
```

Payload:

```text
TaskUpdatedPayload {
    taskId
    previousVersion
    currentVersion
    compatibilityMode
    changedPolicyClasses[]
    updatedAt
}
```

---

## 26. TaskRemovalStarted

Event type:

```text
scheduler.task.removal-started
```

Payload:

```text
TaskRemovalStartedPayload {
    taskId
    activeScheduleCount
    pendingJobCount
    removalPolicy
    startedAt
}
```

---

## 27. TaskRemoved

Event type:

```text
scheduler.task.removed
```

Payload:

```text
TaskRemovedPayload {
    taskId
    finalVersion
    removedScheduleCount
    affectedPendingJobCount
    removedAt
}
```

---

## 28. TaskRegistrationRejected

Event type:

```text
scheduler.task.registration-rejected
```

Payload:

```text
TaskRegistrationRejectedPayload {
    taskId?
    ownerModule?
    normalizedErrorCode
    rejectionClass
    rejectedAt
}
```

---

## 29. TaskInvalidated

Event type:

```text
scheduler.task.invalidated
```

Payload:

```text
TaskInvalidatedPayload {
    taskId
    taskVersion
    normalizedErrorCode
    normalExecutionBlocked
    invalidatedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY when safety-related
```

---

# Part III — Schedule Events

## 30. ScheduleRegistered

Event type:

```text
scheduler.schedule.registered
```

Payload:

```text
ScheduleRegisteredPayload {
    scheduleId
    taskId
    scheduleVersion
    triggerType
    timezone?
    persistenceMode
    registeredAt
}
```

---

## 31. ScheduleEnabled

Event type:

```text
scheduler.schedule.enabled
```

Payload:

```text
ScheduleEnabledPayload {
    scheduleId
    taskId
    previousState
    currentState = ENABLED
    nextOccurrenceAt?
    enabledAt
}
```

---

## 32. SchedulePaused

Event type:

```text
scheduler.schedule.paused
```

Payload:

```text
SchedulePausedPayload {
    scheduleId
    taskId
    nextPlannedOccurrenceAt?
    pausedAt
}
```

---

## 33. ScheduleResumed

Event type:

```text
scheduler.schedule.resumed
```

Payload:

```text
ScheduleResumedPayload {
    scheduleId
    taskId
    nextOccurrenceAt?
    resumedAt
}
```

---

## 34. ScheduleUpdated

Event type:

```text
scheduler.schedule.updated
```

Payload:

```text
ScheduleUpdatedPayload {
    scheduleId
    taskId
    previousVersion
    currentVersion
    triggerChanged
    policyChanges[]
    nextOccurrenceAt?
    updatedAt
}
```

---

## 35. ScheduleExpired

Event type:

```text
scheduler.schedule.expired
```

Payload:

```text
ScheduleExpiredPayload {
    scheduleId
    taskId
    endAt?
    lastOccurrenceAt?
    expiredAt
}
```

---

## 36. ScheduleDisabled

Event type:

```text
scheduler.schedule.disabled
```

Payload:

```text
ScheduleDisabledPayload {
    scheduleId
    taskId
    previousState
    currentState = DISABLED
    disabledAt
}
```

---

## 37. ScheduleRemoved

Event type:

```text
scheduler.schedule.removed
```

Payload:

```text
ScheduleRemovedPayload {
    scheduleId
    taskId
    removedAt
}
```

---

## 38. ScheduleRegistrationRejected

Event type:

```text
scheduler.schedule.registration-rejected
```

Payload:

```text
ScheduleRegistrationRejectedPayload {
    scheduleId?
    taskId?
    normalizedErrorCode
    rejectionClass
    rejectedAt
}
```

---

## 39. ScheduleInvalidated

Event type:

```text
scheduler.schedule.invalidated
```

Payload:

```text
ScheduleInvalidatedPayload {
    scheduleId
    taskId
    scheduleVersion
    normalizedErrorCode
    triggerDisarmed
    invalidatedAt
}
```

---

# Part IV — Trigger Events

## 40. TriggerArmingStarted

Event type:

```text
scheduler.trigger.arming-started
```

Payload:

```text
TriggerArmingStartedPayload {
    triggerId
    scheduleId
    triggerType
    startedAt
}
```

---

## 41. TriggerArmed

Event type:

```text
scheduler.trigger.armed
```

Payload:

```text
TriggerArmedPayload {
    triggerId
    scheduleId
    triggerType
    nextEvaluationAt?
    armedAt
}
```

---

## 42. TriggerEvaluationStarted

High-volume event.

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY
OBSERVABILITY_ONLY
```

Event type:

```text
scheduler.trigger.evaluation-started
```

Payload:

```text
TriggerEvaluationStartedPayload {
    triggerId
    scheduleId
    triggerType
    evaluatedAt
}
```

---

## 43. TriggerFired

Event type:

```text
scheduler.trigger.fired
```

Payload:

```text
TriggerFiredPayload {
    triggerId
    scheduleId
    taskId
    occurrenceId
    triggerType
    scheduledAt
    firedAt
}
```

---

## 44. TriggerWaitingNext

Event type:

```text
scheduler.trigger.waiting-next
```

Payload:

```text
TriggerWaitingNextPayload {
    triggerId
    scheduleId
    nextOccurrenceAt
    waitingAt
}
```

---

## 45. TriggerPaused

Event type:

```text
scheduler.trigger.paused
```

Payload:

```text
TriggerPausedPayload {
    triggerId
    scheduleId
    previousState
    currentState = PAUSED
    pausedAt
}
```

---

## 46. TriggerDisarmed

Event type:

```text
scheduler.trigger.disarmed
```

Payload:

```text
TriggerDisarmedPayload {
    triggerId
    scheduleId
    reasonCode
    disarmedAt
}
```

---

## 47. TriggerExhausted

Event type:

```text
scheduler.trigger.exhausted
```

Payload:

```text
TriggerExhaustedPayload {
    triggerId
    scheduleId
    finalOccurrenceId?
    exhaustedAt
}
```

---

## 48. TriggerFailed

Event type:

```text
scheduler.trigger.failed
```

Payload:

```text
TriggerFailedPayload {
    triggerId
    scheduleId
    triggerType
    failureStage
    normalizedErrorCode
    failedAt
}
```

---

# Part V — Occurrence, Overlap, and Misfire Events

## 49. ScheduleOccurrencePlanned

Event type:

```text
scheduler.occurrence.planned
```

Payload:

```text
ScheduleOccurrencePlannedPayload {
    occurrenceId
    scheduleId
    taskId
    plannedAt
    dueAt
}
```

---

## 50. ScheduleOccurrenceDue

Event type:

```text
scheduler.occurrence.due
```

Payload:

```text
ScheduleOccurrenceDuePayload {
    occurrenceId
    scheduleId
    taskId
    dueAt
    evaluatedAt
}
```

---

## 51. OverlapEvaluationStarted

Event type:

```text
scheduler.overlap.evaluation-started
```

Payload:

```text
OverlapEvaluationStartedPayload {
    occurrenceId
    scheduleId
    taskId
    overlapPolicy
    existingActiveJobCount
    existingPendingJobCount
    startedAt
}
```

---

## 52. OverlapAllowed

Event type:

```text
scheduler.overlap.allowed
```

Payload:

```text
OverlapAllowedPayload {
    occurrenceId
    scheduleId
    overlapPolicy
    evaluatedAt
}
```

---

## 53. OverlapSkipped

Event type:

```text
scheduler.overlap.skipped
```

Payload:

```text
OverlapSkippedPayload {
    occurrenceId
    scheduleId
    overlapPolicy
    existingJobIds[]
    skippedAt
}
```

---

## 54. OverlapQueued

Event type:

```text
scheduler.overlap.queued
```

Payload:

```text
OverlapQueuedPayload {
    occurrenceId
    scheduleId
    overlapPolicy
    queueMode
    queuedAt
}
```

---

## 55. OverlapExistingCancellationRequested

Event type:

```text
scheduler.overlap.existing-cancellation-requested
```

Payload:

```text
OverlapExistingCancellationRequestedPayload {
    occurrenceId
    scheduleId
    existingJobIds[]
    cancellationReason
    requestedAt
}
```

---

## 56. OverlapPendingJobReplaced

Event type:

```text
scheduler.overlap.pending-job-replaced
```

Payload:

```text
OverlapPendingJobReplacedPayload {
    occurrenceId
    scheduleId
    replacedJobId
    replacementJobId?
    replacedAt
}
```

---

## 57. MisfireDetected

Event type:

```text
scheduler.misfire.detected
```

Payload:

```text
MisfireDetectedPayload {
    scheduleId
    taskId
    firstMissedOccurrenceAt
    lastMissedOccurrenceAt
    missedOccurrenceCount
    detectedAt
}
```

---

## 58. MisfireSkipped

Event type:

```text
scheduler.misfire.skipped
```

Payload:

```text
MisfireSkippedPayload {
    scheduleId
    missedOccurrenceCount
    nextOccurrenceAt?
    skippedAt
}
```

---

## 59. MisfireRunOnceScheduled

Event type:

```text
scheduler.misfire.run-once-scheduled
```

Payload:

```text
MisfireRunOnceScheduledPayload {
    scheduleId
    missedOccurrenceCount
    occurrenceId
    jobId?
    scheduledAt
}
```

---

## 60. MisfireCatchUpScheduled

Event type:

```text
scheduler.misfire.catch-up-scheduled
```

Payload:

```text
MisfireCatchUpScheduledPayload {
    scheduleId
    missedOccurrenceCount
    boundedCreatedCount
    occurrenceIds[]
    scheduledAt
}
```

---

## 61. MisfireRescheduledFromNow

Event type:

```text
scheduler.misfire.rescheduled-from-now
```

Payload:

```text
MisfireRescheduledFromNowPayload {
    scheduleId
    previousNextOccurrenceAt?
    currentNextOccurrenceAt
    rescheduledAt
}
```

---

## 62. MisfireEvaluationFailed

Event type:

```text
scheduler.misfire.evaluation-failed
```

Payload:

```text
MisfireEvaluationFailedPayload {
    scheduleId
    normalizedErrorCode
    futureTriggerPaused
    failedAt
}
```

---

# Part VI — Job Creation and Admission Events

## 63. JobCreationStarted

Event type:

```text
scheduler.job.creation-started
```

Payload:

```text
JobCreationStartedPayload {
    taskId
    scheduleId?
    triggerId
    occurrenceId?
    requestedPriority
    startedAt
}
```

---

## 64. JobCreated

Event type:

```text
scheduler.job.created
```

Payload:

```text
JobCreatedPayload {
    jobId
    taskId
    taskVersion
    scheduleId?
    triggerId
    occurrenceId?
    priority
    persistenceMode
    scheduledAt
    createdAt
}
```

---

## 65. JobCreationRejected

Event type:

```text
scheduler.job.creation-rejected
```

Payload:

```text
JobCreationRejectedPayload {
    taskId
    scheduleId?
    occurrenceId?
    rejectionStage
    normalizedErrorCode
    rejectedAt
}
```

---

## 66. JobDuplicateRejected

Event type:

```text
scheduler.job.duplicate-rejected
```

Payload:

```text
JobDuplicateRejectedPayload {
    taskId
    existingJobId
    duplicatePolicy
    duplicateGroupId?
    rejectedAt
}
```

---

## 67. JobCoalesced

Event type:

```text
scheduler.job.coalesced
```

Payload:

```text
JobCoalescedPayload {
    taskId
    requestedJobId?
    existingJobId
    duplicatePolicy
    coalescedAt
}
```

---

## 68. JobLinkedToExisting

Event type:

```text
scheduler.job.linked-to-existing
```

Payload:

```text
JobLinkedToExistingPayload {
    taskId
    existingJobId
    correlationId
    linkedAt
}
```

---

## 69. JobScheduled

Event type:

```text
scheduler.job.scheduled
```

Payload:

```text
JobScheduledPayload {
    jobId
    taskId
    scheduledAt
    notBefore?
    startDeadline?
    completionDeadline?
}
```

---

## 70. JobWaitingForTime

Event type:

```text
scheduler.job.waiting-for-time
```

Payload:

```text
JobWaitingForTimePayload {
    jobId
    taskId
    nextEligibleAt
    waitReason
    waitingAt
}
```

---

## 71. JobWaitingForDependency

Event type:

```text
scheduler.job.waiting-for-dependency
```

Payload:

```text
JobWaitingForDependencyPayload {
    jobId
    taskId
    unresolvedDependencyCount
    dependencyClasses[]
    waitingAt
}
```

---

## 72. JobWaitingForResource

Event type:

```text
scheduler.job.waiting-for-resource
```

Payload:

```text
JobWaitingForResourcePayload {
    jobId
    taskId
    resourceClasses[]
    nextEvaluationAt?
    waitingAt
}
```

---

## 73. JobWaitingForConcurrency

Event type:

```text
scheduler.job.waiting-for-concurrency
```

Payload:

```text
JobWaitingForConcurrencyPayload {
    jobId
    taskId
    concurrencyGroupId?
    concurrencyKeyPresent
    nextEvaluationAt?
    waitingAt
}
```

---

## 74. JobReady

Event type:

```text
scheduler.job.ready
```

Payload:

```text
JobReadyPayload {
    jobId
    taskId
    effectivePriority
    readyAt
}
```

---

## 75. JobQueued

Event type:

```text
scheduler.job.queued
```

Payload:

```text
JobQueuedPayload {
    jobId
    taskId
    queueClass
    effectivePriority
    queueDepthAfter
    queuedAt
}
```

---

## 76. JobAdmissionRejected

Event type:

```text
scheduler.job.admission-rejected
```

Payload:

```text
JobAdmissionRejectedPayload {
    jobId
    taskId
    queueClass
    effectivePriority
    normalizedErrorCode
    criticalReserveExhausted
    rejectedAt
}
```

---

## 77. JobAdmissionTimedOut

Event type:

```text
scheduler.job.admission-timed-out
```

Payload:

```text
JobAdmissionTimedOutPayload {
    jobId
    taskId
    queueClass
    admissionTimeout
    timedOutAt
}
```

---

# Part VII — Dispatch and Attempt Events

## 78. JobDispatchStarted

Event type:

```text
scheduler.job.dispatch-started
```

Payload:

```text
JobDispatchStartedPayload {
    jobId
    taskId
    attemptId
    requestedWorkerClass?
    startedAt
}
```

---

## 79. JobDispatched

Event type:

```text
scheduler.job.dispatched
```

Payload:

```text
JobDispatchedPayload {
    jobId
    taskId
    attemptId
    workerId
    concurrencyLeaseCount
    resourceReservationCount
    dispatchedAt
}
```

---

## 80. JobDispatchDeferred

Event type:

```text
scheduler.job.dispatch-deferred
```

Payload:

```text
JobDispatchDeferredPayload {
    jobId
    taskId
    attemptId?
    deferReason
    nextEvaluationAt?
    deferredAt
}
```

Possible reasons:

```text
WAIT_RESOURCE
WAIT_CONCURRENCY
WAIT_DEPENDENCY
WAIT_WORKER
WAIT_RUNTIME_CAPACITY
```

---

## 81. JobDispatchFailed

Event type:

```text
scheduler.job.dispatch-failed
```

Payload:

```text
JobDispatchFailedPayload {
    jobId
    taskId
    attemptId?
    failureStage
    normalizedErrorCode
    retryable
    failedAt
}
```

---

## 82. AttemptCreated

Event type:

```text
scheduler.attempt.created
```

Payload:

```text
AttemptCreatedPayload {
    attemptId
    jobId
    taskId
    attemptNumber
    timeout
    createdAt
}
```

---

## 83. AttemptConcurrencyAcquisitionStarted

Event type:

```text
scheduler.attempt.concurrency-acquisition-started
```

Payload:

```text
AttemptConcurrencyAcquisitionStartedPayload {
    attemptId
    jobId
    requiredLeaseCount
    startedAt
}
```

---

## 84. AttemptResourceAcquisitionStarted

Event type:

```text
scheduler.attempt.resource-acquisition-started
```

Payload:

```text
AttemptResourceAcquisitionStartedPayload {
    attemptId
    jobId
    resourceClasses[]
    startedAt
}
```

---

## 85. AttemptStarted

Event type:

```text
scheduler.attempt.started
```

Payload:

```text
AttemptStartedPayload {
    attemptId
    jobId
    taskId
    attemptNumber
    workerId
    startedAt
    executionDeadline
}
```

---

## 86. JobStarted

Event type:

```text
scheduler.job.started
```

Payload:

```text
JobStartedPayload {
    jobId
    taskId
    attemptId
    attemptNumber
    workerId
    queueWaitDuration
    startedAt
}
```

---

## 87. JobProgressReported

High-volume and non-authoritative.

Event type:

```text
scheduler.job.progress-reported
```

Payload:

```text
JobProgressReportedPayload {
    jobId
    attemptId
    progressClass
    fractionBucket?
    stageCode?
    reportedAt
}
```

May be throttled, coalesced, sampled, or dropped.

---

# Part VIII — Job and Attempt Completion Events

## 88. AttemptSucceeded

Event type:

```text
scheduler.attempt.succeeded
```

Payload:

```text
AttemptSucceededPayload {
    attemptId
    jobId
    taskId
    attemptNumber
    outputReference?
    executionDuration
    succeededAt
}
```

---

## 89. JobSucceeded

Event type:

```text
scheduler.job.succeeded
```

Payload:

```text
JobSucceededPayload {
    jobId
    taskId
    finalAttemptId
    attemptCount
    outputReference?
    totalDuration
    succeededAt
}
```

---

## 90. AttemptFailed

Event type:

```text
scheduler.attempt.failed
```

Payload:

```text
AttemptFailedPayload {
    attemptId
    jobId
    taskId
    attemptNumber
    failureClass
    normalizedErrorCode
    retryable
    failedAt
}
```

---

## 91. JobFailed

Event type:

```text
scheduler.job.failed
```

Payload:

```text
JobFailedPayload {
    jobId
    taskId
    finalAttemptId
    attemptCount
    failureClass
    normalizedErrorCode
    retryExhausted
    failedAt
}
```

---

## 92. AttemptCancellationRequested

Event type:

```text
scheduler.attempt.cancellation-requested
```

Payload:

```text
AttemptCancellationRequestedPayload {
    attemptId
    jobId
    cancellationReason
    cancellationScope
    deadline?
    requestedAt
}
```

---

## 93. AttemptCanceled

Event type:

```text
scheduler.attempt.canceled
```

Payload:

```text
AttemptCanceledPayload {
    attemptId
    jobId
    taskId
    attemptNumber
    cancellationReason
    canceledAt
}
```

---

## 94. JobCanceled

Event type:

```text
scheduler.job.canceled
```

Payload:

```text
JobCanceledPayload {
    jobId
    taskId
    finalAttemptId?
    cancellationReason
    canceledBeforeStart
    canceledAt
}
```

---

## 95. AttemptTimeoutStarted

Event type:

```text
scheduler.attempt.timeout-started
```

Payload:

```text
AttemptTimeoutStartedPayload {
    attemptId
    jobId
    executionTimeout
    cancellationGracePeriod
    timedOutLogicallyAt
}
```

---

## 96. AttemptTimedOut

Event type:

```text
scheduler.attempt.timed-out
```

Payload:

```text
AttemptTimedOutPayload {
    attemptId
    jobId
    taskId
    attemptNumber
    physicalExecutionMayContinue
    timedOutAt
}
```

---

## 97. JobTimedOut

Event type:

```text
scheduler.job.timed-out
```

Payload:

```text
JobTimedOutPayload {
    jobId
    taskId
    finalAttemptId
    retryAllowed
    terminalForJob
    timedOutAt
}
```

---

## 98. AttemptAbandoned

Event type:

```text
scheduler.attempt.abandoned
```

Payload:

```text
AttemptAbandonedPayload {
    attemptId
    jobId
    taskId
    attemptNumber
    abandonmentReason
    physicalExecutionUnconfirmed
    abandonedAt
}
```

---

## 99. JobAbandoned

Event type:

```text
scheduler.job.abandoned
```

Payload:

```text
JobAbandonedPayload {
    jobId
    taskId
    finalAttemptId
    abandonmentReason
    abandonedAt
}
```

---

## 100. AttemptInterrupted

Event type:

```text
scheduler.attempt.interrupted
```

Payload:

```text
AttemptInterruptedPayload {
    attemptId
    jobId
    interruptionReason
    persistenceMode
    recoveryRequired
    interruptedAt
}
```

---

## 101. JobInterrupted

Event type:

```text
scheduler.job.interrupted
```

Payload:

```text
JobInterruptedPayload {
    jobId
    taskId
    currentAttemptId
    interruptionReason
    recoveryPolicy
    interruptedAt
}
```

---

## 102. AttemptOutcomeBecameUncertain

Event type:

```text
scheduler.attempt.outcome-became-uncertain
```

Payload:

```text
AttemptOutcomeBecameUncertainPayload {
    attemptId
    jobId
    uncertaintyStage
    duplicateRisk
    reconciliationRequired
    occurredAt
}
```

---

## 103. AttemptLateCompletionObserved

Event type:

```text
scheduler.attempt.late-completion-observed
```

Payload:

```text
AttemptLateCompletionObservedPayload {
    attemptId
    jobId
    authoritativeTerminalState
    reportedPhysicalOutcome
    observedAt
}
```

Late completion is non-authoritative.

---

## 104. DuplicateAttemptCompletionBlocked

Event type:

```text
scheduler.attempt.duplicate-completion-blocked
```

Payload:

```text
DuplicateAttemptCompletionBlockedPayload {
    attemptId
    jobId
    authoritativeTerminalState
    duplicateReportedOutcome
    blockedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
OBSERVABILITY_ONLY
```

---

## 105. JobSkipped

Event type:

```text
scheduler.job.skipped
```

Payload:

```text
JobSkippedPayload {
    jobId
    taskId
    skipReason
    overlapPolicy?
    misfirePolicy?
    skippedAt
}
```

---

## 106. JobExpired

Event type:

```text
scheduler.job.expired
```

Payload:

```text
JobExpiredPayload {
    jobId
    taskId
    expirationType
    deadline
    expiredAt
}
```

---

# Part IX — Retry Events

## 107. RetryEvaluationStarted

Event type:

```text
scheduler.retry.evaluation-started
```

Payload:

```text
RetryEvaluationStartedPayload {
    jobId
    previousAttemptId
    previousAttemptNumber
    failureClass
    startedAt
}
```

---

## 108. JobRetryScheduled

Event type:

```text
scheduler.job.retry-scheduled
```

Payload:

```text
JobRetryScheduledPayload {
    jobId
    previousAttemptId
    nextAttemptNumber
    retryAt
    backoffStrategy
    retryDelay
    scheduledAt
}
```

---

## 109. JobRetryReady

Event type:

```text
scheduler.job.retry-ready
```

Payload:

```text
JobRetryReadyPayload {
    jobId
    nextAttemptNumber
    readyAt
}
```

---

## 110. JobRetryDenied

Event type:

```text
scheduler.job.retry-denied
```

Payload:

```text
JobRetryDeniedPayload {
    jobId
    previousAttemptId
    failureClass
    denialReason
    deniedAt
}
```

---

## 111. JobRetryExhausted

Event type:

```text
scheduler.job.retry-exhausted
```

Payload:

```text
JobRetryExhaustedPayload {
    jobId
    finalAttemptId
    attemptCount
    maximumAttempts
    finalErrorCode
    exhaustedAt
}
```

---

## 112. JobRetryReconciliationRequired

Event type:

```text
scheduler.job.retry-reconciliation-required
```

Payload:

```text
JobRetryReconciliationRequiredPayload {
    jobId
    previousAttemptId
    uncertaintyClass
    blindRetryBlocked
    requiredAt
}
```

---

# Part X — Cancellation Events

## 113. CancellationRequested

Event type:

```text
scheduler.cancellation.requested
```

Payload:

```text
CancellationRequestedPayload {
    cancellationOperationId
    targetScope
    targetId
    reasonCode
    deadline?
    requestedAt
}
```

---

## 114. CancellationPropagationStarted

Event type:

```text
scheduler.cancellation.propagation-started
```

Payload:

```text
CancellationPropagationStartedPayload {
    cancellationOperationId
    affectedJobCount
    affectedAttemptCount
    affectedWorkerCount
    startedAt
}
```

---

## 115. CancellationAcknowledged

Event type:

```text
scheduler.cancellation.acknowledged
```

Payload:

```text
CancellationAcknowledgedPayload {
    cancellationOperationId
    acknowledgedJobCount
    acknowledgedAttemptCount
    acknowledgedAt
}
```

---

## 116. CancellationCompleted

Event type:

```text
scheduler.cancellation.completed
```

Payload:

```text
CancellationCompletedPayload {
    cancellationOperationId
    outcome
    jobsCanceled
    attemptsCanceled
    attemptsStillRunning
    completedAt
}
```

---

## 117. CancellationTimedOut

Event type:

```text
scheduler.cancellation.timed-out
```

Payload:

```text
CancellationTimedOutPayload {
    cancellationOperationId
    remainingAttemptIds[]
    abandonmentRequired
    timedOutAt
}
```

---

## 118. CancellationRejected

Event type:

```text
scheduler.cancellation.rejected
```

Payload:

```text
CancellationRejectedPayload {
    cancellationOperationId
    targetScope
    targetId
    rejectionReason
    rejectedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY when unauthorized
```

---

# Part XI — Queue and Dispatcher Events

## 119. SchedulerQueueInitialized

Event type:

```text
scheduler.queue.initialized
```

Payload:

```text
SchedulerQueueInitializedPayload {
    queueId
    queueClass
    capacity
    criticalReserve?
    initializedAt
}
```

---

## 120. SchedulerQueueAvailable

Event type:

```text
scheduler.queue.available
```

Payload:

```text
SchedulerQueueAvailablePayload {
    queueId
    queueClass
    previousState
    currentState = AVAILABLE
    queueDepth
    availableAt
}
```

---

## 121. SchedulerQueueBackpressured

Event type:

```text
scheduler.queue.backpressured
```

Payload:

```text
SchedulerQueueBackpressuredPayload {
    queueId
    queueClass
    queueDepth
    capacity
    utilizationClass
    affectedPriorities[]
    detectedAt
}
```

---

## 122. SchedulerQueueRecovered

Event type:

```text
scheduler.queue.recovered
```

Payload:

```text
SchedulerQueueRecoveredPayload {
    queueId
    queueClass
    previousState = BACKPRESSURED
    currentState = AVAILABLE
    queueDepth
    recoveredAt
}
```

---

## 123. SchedulerQueueCriticalReserveUsed

Event type:

```text
scheduler.queue.critical-reserve-used
```

Payload:

```text
SchedulerQueueCriticalReserveUsedPayload {
    queueId
    jobId
    priority
    reserveRemaining
    usedAt
}
```

---

## 124. SchedulerQueueCriticalReserveExhausted

Event type:

```text
scheduler.queue.critical-reserve-exhausted
```

Payload:

```text
SchedulerQueueCriticalReserveExhaustedPayload {
    queueId
    queueClass
    rejectedPriority
    reserveCapacity
    exhaustedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

## 125. QueueItemClaimed

High-volume event.

Event type:

```text
scheduler.queue.item-claimed
```

Payload:

```text
QueueItemClaimedPayload {
    queueItemId
    queueId
    jobId
    dispatcherId
    claimedAt
}
```

---

## 126. QueueItemExpired

Event type:

```text
scheduler.queue.item-expired
```

Payload:

```text
QueueItemExpiredPayload {
    queueItemId
    queueId
    jobId
    expirationReason
    expiredAt
}
```

---

## 127. QueueItemDropped

Event type:

```text
scheduler.queue.item-dropped
```

Payload:

```text
QueueItemDroppedPayload {
    queueItemId
    queueId
    jobId
    priority
    dropReason
    droppedAt
}
```

---

## 128. SchedulerQueueDrainStarted

Event type:

```text
scheduler.queue.drain-started
```

Payload:

```text
SchedulerQueueDrainStartedPayload {
    queueId
    queueClass
    queueDepth
    deadline
    startedAt
}
```

---

## 129. SchedulerQueueDrainCompleted

Event type:

```text
scheduler.queue.drain-completed
```

Payload:

```text
SchedulerQueueDrainCompletedPayload {
    queueId
    queueClass
    outcome
    jobsDequeued
    jobsRemaining
    completedAt
}
```

---

## 130. SchedulerQueueFailed

Event type:

```text
scheduler.queue.failed
```

Payload:

```text
SchedulerQueueFailedPayload {
    queueId
    queueClass
    normalizedErrorCode
    claimAuthorityTrusted
    failedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

## 131. DispatcherStarted

Event type:

```text
scheduler.dispatcher.started
```

Payload:

```text
DispatcherStartedPayload {
    dispatcherId
    queueCount
    workerCount
    startedAt
}
```

---

## 132. DispatcherBackpressured

Event type:

```text
scheduler.dispatcher.backpressured
```

Payload:

```text
DispatcherBackpressuredPayload {
    dispatcherId
    reasonClasses[]
    dispatchableJobCount
    availableWorkerCount
    detectedAt
}
```

---

## 133. DispatcherRecovered

Event type:

```text
scheduler.dispatcher.recovered
```

Payload:

```text
DispatcherRecoveredPayload {
    dispatcherId
    previousState = BACKPRESSURED
    recoveredAt
}
```

---

## 134. DispatcherFailed

Event type:

```text
scheduler.dispatcher.failed
```

Payload:

```text
DispatcherFailedPayload {
    dispatcherId
    normalizedErrorCode
    dispatchAuthorityTrusted
    failedAt
}
```

---

# Part XII — Worker Events

## 135. WorkerRegistered

Event type:

```text
scheduler.worker.registered
```

Payload:

```text
WorkerRegisteredPayload {
    workerId
    ownerModule
    supportedTaskIds[]
    capacityClass
    registeredAt
}
```

---

## 136. WorkerInitializationStarted

Event type:

```text
scheduler.worker.initialization-started
```

Payload:

```text
WorkerInitializationStartedPayload {
    workerId
    ownerModule
    startedAt
}
```

---

## 137. WorkerAvailable

Event type:

```text
scheduler.worker.available
```

Payload:

```text
WorkerAvailablePayload {
    workerId
    previousState
    currentState = AVAILABLE
    capacity
    availableAt
}
```

---

## 138. WorkerBusy

High-volume event.

Event type:

```text
scheduler.worker.busy
```

Payload:

```text
WorkerBusyPayload {
    workerId
    activeAttemptCount
    capacity
    changedAt
}
```

---

## 139. WorkerDraining

Event type:

```text
scheduler.worker.draining
```

Payload:

```text
WorkerDrainingPayload {
    workerId
    activeAttemptCount
    deadline
    drainingAt
}
```

---

## 140. WorkerDisabled

Event type:

```text
scheduler.worker.disabled
```

Payload:

```text
WorkerDisabledPayload {
    workerId
    disableReason
    activeAttemptCount
    disabledAt
}
```

---

## 141. WorkerTerminated

Event type:

```text
scheduler.worker.terminated
```

Payload:

```text
WorkerTerminatedPayload {
    workerId
    finalState = TERMINATED
    attemptsCompleted
    attemptsAbandoned
    terminatedAt
}
```

---

## 142. WorkerFailed

Event type:

```text
scheduler.worker.failed
```

Payload:

```text
WorkerFailedPayload {
    workerId
    ownerModule
    normalizedErrorCode
    activeAttemptIds[]
    failureImpact[]
    failedAt
}
```

---

## 143. WorkerHealthChanged

Event type:

```text
scheduler.worker.health-changed
```

Payload:

```text
WorkerHealthChangedPayload {
    workerId
    previousHealth
    currentHealth
    reasonCode
    changedAt
}
```

---

## 144. WorkerSlowDetected

Event type:

```text
scheduler.worker.slow-detected
```

Payload:

```text
WorkerSlowDetectedPayload {
    workerId
    latencyClass
    consecutiveSlowAttempts
    detectedAt
}
```

---

## 145. WorkerRecoveryStarted

Event type:

```text
scheduler.worker.recovery-started
```

Payload:

```text
WorkerRecoveryStartedPayload {
    workerId
    recoveryMode
    startedAt
}
```

---

## 146. WorkerRecovered

Event type:

```text
scheduler.worker.recovered
```

Payload:

```text
WorkerRecoveredPayload {
    workerId
    previousHealth
    currentHealth
    recoveredAt
}
```

---

# Part XIII — Concurrency Events

## 147. ConcurrencyLeaseRequested

Event type:

```text
scheduler.concurrency.lease-requested
```

Payload:

```text
ConcurrencyLeaseRequestedPayload {
    leaseId
    jobId
    attemptId
    concurrencyGroupId
    concurrencyKeyPresent
    units
    requestedAt
}
```

---

## 148. ConcurrencyLeaseWaiting

Event type:

```text
scheduler.concurrency.lease-waiting
```

Payload:

```text
ConcurrencyLeaseWaitingPayload {
    leaseId
    jobId
    concurrencyGroupId
    nextEvaluationAt?
    waitingAt
}
```

---

## 149. ConcurrencyLeaseAcquired

Event type:

```text
scheduler.concurrency.lease-acquired
```

Payload:

```text
ConcurrencyLeaseAcquiredPayload {
    leaseId
    jobId
    attemptId
    concurrencyGroupId
    units
    acquiredAt
}
```

---

## 150. ConcurrencyLeaseActivated

Event type:

```text
scheduler.concurrency.lease-activated
```

Payload:

```text
ConcurrencyLeaseActivatedPayload {
    leaseId
    jobId
    attemptId
    activatedAt
}
```

---

## 151. ConcurrencyLeaseReleased

Event type:

```text
scheduler.concurrency.lease-released
```

Payload:

```text
ConcurrencyLeaseReleasedPayload {
    leaseId
    jobId
    attemptId
    releaseReason
    releasedAt
}
```

---

## 152. ConcurrencyLeaseExpired

Event type:

```text
scheduler.concurrency.lease-expired
```

Payload:

```text
ConcurrencyLeaseExpiredPayload {
    leaseId
    jobId
    attemptId?
    expirationReason
    activeAttemptAffected
    expiredAt
}
```

---

## 153. ConcurrencyLeaseFailed

Event type:

```text
scheduler.concurrency.lease-failed
```

Payload:

```text
ConcurrencyLeaseFailedPayload {
    leaseId
    jobId
    attemptId?
    normalizedErrorCode
    failedAt
}
```

---

# Part XIV — Resource Events

## 154. ResourceReservationRequested

Event type:

```text
scheduler.resource.reservation-requested
```

Payload:

```text
ResourceReservationRequestedPayload {
    reservationId
    jobId
    attemptId?
    resourceClass
    units
    exclusive
    requestedAt
}
```

---

## 155. ResourceReservationWaiting

Event type:

```text
scheduler.resource.reservation-waiting
```

Payload:

```text
ResourceReservationWaitingPayload {
    reservationId
    jobId
    resourceClass
    requiredUnits
    availableUnits
    nextEvaluationAt?
    waitingAt
}
```

---

## 156. ResourceReservationGranted

Event type:

```text
scheduler.resource.reservation-granted
```

Payload:

```text
ResourceReservationGrantedPayload {
    reservationId
    jobId
    attemptId?
    resourceClass
    units
    grantedAt
}
```

---

## 157. ResourceReservationActivated

Event type:

```text
scheduler.resource.reservation-activated
```

Payload:

```text
ResourceReservationActivatedPayload {
    reservationId
    jobId
    attemptId
    resourceClass
    activatedAt
}
```

---

## 158. ResourceReservationReleased

Event type:

```text
scheduler.resource.reservation-released
```

Payload:

```text
ResourceReservationReleasedPayload {
    reservationId
    jobId
    attemptId?
    resourceClass
    releaseReason
    releasedAt
}
```

---

## 159. ResourceReservationExpired

Event type:

```text
scheduler.resource.reservation-expired
```

Payload:

```text
ResourceReservationExpiredPayload {
    reservationId
    jobId
    attemptId?
    resourceClass
    activeAttemptAffected
    expiredAt
}
```

---

## 160. ResourceReservationRevoked

Event type:

```text
scheduler.resource.reservation-revoked
```

Payload:

```text
ResourceReservationRevokedPayload {
    reservationId
    jobId
    attemptId
    resourceClass
    revocationReason
    cancellationRequested
    revokedAt
}
```

---

## 161. ResourceReservationFailed

Event type:

```text
scheduler.resource.reservation-failed
```

Payload:

```text
ResourceReservationFailedPayload {
    reservationId
    jobId
    resourceClass
    normalizedErrorCode
    failedAt
}
```

---

## 162. SchedulerResourcePressureDetected

Event type:

```text
scheduler.resource.pressure-detected
```

Payload:

```text
SchedulerResourcePressureDetectedPayload {
    resourceClass
    pressureClass
    affectedPriorityClasses[]
    affectedTaskIds[]
    detectedAt
}
```

---

## 163. SchedulerResourcePressureRecovered

Event type:

```text
scheduler.resource.pressure-recovered
```

Payload:

```text
SchedulerResourcePressureRecoveredPayload {
    resourceClass
    previousPressureClass
    recoveredAt
}
```

---

# Part XV — Dependency Events

## 164. DependencyEvaluationStarted

Event type:

```text
scheduler.dependency.evaluation-started
```

Payload:

```text
DependencyEvaluationStartedPayload {
    jobId
    dependencyCount
    startedAt
}
```

---

## 165. JobDependenciesSatisfied

Event type:

```text
scheduler.dependency.satisfied
```

Payload:

```text
JobDependenciesSatisfiedPayload {
    jobId
    dependencyCount
    satisfiedAt
}
```

---

## 166. JobDependencyWaiting

Event type:

```text
scheduler.dependency.waiting
```

Payload:

```text
JobDependencyWaitingPayload {
    jobId
    unresolvedCount
    dependencyClasses[]
    waitingAt
}
```

---

## 167. JobDependencyFailed

Event type:

```text
scheduler.dependency.failed
```

Payload:

```text
JobDependencyFailedPayload {
    jobId
    failedDependencyClass
    failedDependencyId?
    normalizedErrorCode
    failedAt
}
```

---

## 168. DependencyCycleDetected

Event type:

```text
scheduler.dependency.cycle-detected
```

Payload:

```text
DependencyCycleDetectedPayload {
    taskIds[]
    jobIds[]
    cycleLength
    schedulingBlocked
    detectedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

# Part XVI — Persistence Events

## 169. PersistenceAdapterRegistered

Event type:

```text
scheduler.persistence.adapter-registered
```

Payload:

```text
PersistenceAdapterRegisteredPayload {
    adapterId
    persistenceModes[]
    registeredAt
}
```

---

## 170. PersistenceAdapterAvailable

Event type:

```text
scheduler.persistence.adapter-available
```

Payload:

```text
PersistenceAdapterAvailablePayload {
    adapterId
    previousState
    currentState = AVAILABLE
    availableAt
}
```

---

## 171. PersistenceAdapterDegraded

Event type:

```text
scheduler.persistence.adapter-degraded
```

Payload:

```text
PersistenceAdapterDegradedPayload {
    adapterId
    degradedCapabilities[]
    normalizedReasonCode
    affectedTaskIds[]
    degradedAt
}
```

---

## 172. PersistenceAdapterUnavailable

Event type:

```text
scheduler.persistence.adapter-unavailable
```

Payload:

```text
PersistenceAdapterUnavailablePayload {
    adapterId
    normalizedErrorCode
    affectedPersistenceModes[]
    affectedTaskIds[]
    unavailableAt
}
```

---

## 173. PersistenceAdapterRecovered

Event type:

```text
scheduler.persistence.adapter-recovered
```

Payload:

```text
PersistenceAdapterRecoveredPayload {
    adapterId
    recoveredCapabilities[]
    recoveredAt
}
```

---

## 174. TaskStatePersisted

High-volume event.

Event type:

```text
scheduler.persistence.task-state-persisted
```

Payload:

```text
TaskStatePersistedPayload {
    taskId
    taskVersion
    stateVersion
    persistedAt
}
```

---

## 175. ScheduleStatePersisted

Event type:

```text
scheduler.persistence.schedule-state-persisted
```

Payload:

```text
ScheduleStatePersistedPayload {
    scheduleId
    scheduleVersion
    stateVersion
    persistedAt
}
```

---

## 176. JobStatePersisted

Event type:

```text
scheduler.persistence.job-state-persisted
```

Payload:

```text
JobStatePersistedPayload {
    jobId
    lifecycleState
    stateVersion
    persistedAt
}
```

---

## 177. AttemptStatePersisted

Event type:

```text
scheduler.persistence.attempt-state-persisted
```

Payload:

```text
AttemptStatePersistedPayload {
    attemptId
    jobId
    lifecycleState
    stateVersion
    persistedAt
}
```

---

## 178. SchedulerPersistenceFailed

Event type:

```text
scheduler.persistence.failed
```

Payload:

```text
SchedulerPersistenceFailedPayload {
    adapterId
    entityType
    entityId
    operationType
    normalizedErrorCode
    authorityUncertain
    failedAt
}
```

---

# Part XVII — Recovery and Reconciliation Events

## 179. SchedulerRecoveryLoadingStarted

Event type:

```text
scheduler.recovery.loading-started
```

Payload:

```text
SchedulerRecoveryLoadingStartedPayload {
    recoveryOperationId
    persistenceMode
    startedAt
}
```

---

## 180. SchedulerRecoveryEntitiesClassified

Event type:

```text
scheduler.recovery.entities-classified
```

Payload:

```text
SchedulerRecoveryEntitiesClassifiedPayload {
    recoveryOperationId
    restorableCount
    misfiredCount
    interruptedCount
    uncertainCount
    expiredCount
    invalidCount
    classifiedAt
}
```

---

## 181. InterruptedAttemptDetected

Event type:

```text
scheduler.recovery.interrupted-attempt-detected
```

Payload:

```text
InterruptedAttemptDetectedPayload {
    recoveryOperationId
    attemptId
    jobId
    previousState
    idempotencyMode
    detectedAt
}
```

---

## 182. UncertainAttemptReconciliationStarted

Event type:

```text
scheduler.recovery.uncertain-attempt-reconciliation-started
```

Payload:

```text
UncertainAttemptReconciliationStartedPayload {
    recoveryOperationId
    attemptId
    jobId
    duplicateRisk
    startedAt
}
```

---

## 183. AttemptReconciled

Event type:

```text
scheduler.recovery.attempt-reconciled
```

Payload:

```text
AttemptReconciledPayload {
    recoveryOperationId
    attemptId
    jobId
    previousState
    resolvedJobState
    retryScheduled
    reconciledAt
}
```

---

## 184. RecoveryMisfirePolicyApplied

Event type:

```text
scheduler.recovery.misfire-policy-applied
```

Payload:

```text
RecoveryMisfirePolicyAppliedPayload {
    recoveryOperationId
    scheduleId
    missedOccurrenceCount
    policy
    createdJobCount
    appliedAt
}
```

---

## 185. SchedulerRecoveryCompleted

Event type:

```text
scheduler.recovery.completed
```

Payload:

```text
SchedulerRecoveryCompletedPayload {
    recoveryOperationId
    outcome
    tasksRestored
    schedulesRestored
    jobsRestored
    jobsRescheduled
    attemptsReconciled
    completedAt
}
```

---

## 186. SchedulerRecoveryPartiallyCompleted

Event type:

```text
scheduler.recovery.partially-completed
```

Payload:

```text
SchedulerRecoveryPartiallyCompletedPayload {
    recoveryOperationId
    restoredEntityCount
    failedEntityCount
    uncertainEntityCount
    degradedCapabilities[]
    occurredAt
}
```

---

## 187. SchedulerRecoveryFailed

Event type:

```text
scheduler.recovery.failed
```

Payload:

```text
SchedulerRecoveryFailedPayload {
    recoveryOperationId
    failureStage
    normalizedErrorCode
    normalSchedulingBlocked
    failedAt
}
```

---

## 188. SchedulerRecoveryTimedOut

Event type:

```text
scheduler.recovery.timed-out
```

Payload:

```text
SchedulerRecoveryTimedOutPayload {
    recoveryOperationId
    timeoutStage
    remainingEntityCount
    normalSchedulingBlocked
    timedOutAt
}
```

---

# Part XVIII — Shutdown Events

## 189. SchedulerShutdownRequested

Event type:

```text
scheduler.shutdown.requested
```

Payload:

```text
SchedulerShutdownRequestedPayload {
    shutdownOperationId
    deadline
    drainPriorities[]
    drainTaskIds[]
    persistRecoverableState
    requestedAt
}
```

---

## 190. SchedulerShutdownStarted

Event type:

```text
scheduler.shutdown.started
```

Payload:

```text
SchedulerShutdownStartedPayload {
    shutdownOperationId
    runningAttemptCount
    pendingJobCount
    activeScheduleCount
    startedAt
}
```

---

## 191. SchedulerShutdownTriggersStopped

Event type:

```text
scheduler.shutdown.triggers-stopped
```

Payload:

```text
SchedulerShutdownTriggersStoppedPayload {
    shutdownOperationId
    triggersStopped
    triggersFailed
    stoppedAt
}
```

---

## 192. SchedulerShutdownCancellationStarted

Event type:

```text
scheduler.shutdown.cancellation-started
```

Payload:

```text
SchedulerShutdownCancellationStartedPayload {
    shutdownOperationId
    remainingAttemptCount
    gracePeriod
    startedAt
}
```

---

## 193. SchedulerShutdownStatePersisted

Event type:

```text
scheduler.shutdown.state-persisted
```

Payload:

```text
SchedulerShutdownStatePersistedPayload {
    shutdownOperationId
    jobsPersisted
    schedulesPersisted
    attemptsPersisted
    persistedAt
}
```

---

## 194. SchedulerShutdownWorkersStopped

Event type:

```text
scheduler.shutdown.workers-stopped
```

Payload:

```text
SchedulerShutdownWorkersStoppedPayload {
    shutdownOperationId
    workersStopped
    workersAbandoned
    stoppedAt
}
```

---

## 195. SchedulerShutdownCompleted

Event type:

```text
scheduler.shutdown.completed
```

Payload:

```text
SchedulerShutdownCompletedPayload {
    shutdownOperationId
    outcome
    jobsCompleted
    jobsCanceled
    jobsAbandoned
    jobsPersisted
    jobsLost
    completedAt
}
```

---

## 196. SchedulerShutdownTimedOut

Event type:

```text
scheduler.shutdown.timed-out
```

Payload:

```text
SchedulerShutdownTimedOutPayload {
    shutdownOperationId
    timeoutStage
    remainingAttemptCount
    workersStillActive
    forceStopApplied
    timedOutAt
}
```

---

## 197. SchedulerShutdownFailed

Event type:

```text
scheduler.shutdown.failed
```

Payload:

```text
SchedulerShutdownFailedPayload {
    shutdownOperationId
    failureStage
    normalizedErrorCode
    partialTermination
    failedAt
}
```

---

# Part XIX — Configuration Events

## 198. SchedulerConfigurationApplied

Event type:

```text
scheduler.configuration.applied
```

Payload:

```text
SchedulerConfigurationAppliedPayload {
    configurationRevision
    liveChanges[]
    restartRequiredChanges[]
    affectedTaskIds[]
    appliedAt
}
```

---

## 199. SchedulerConfigurationRejected

Event type:

```text
scheduler.configuration.rejected
```

Payload:

```text
SchedulerConfigurationRejectedPayload {
    configurationRevision
    normalizedErrorCode
    previousConfigurationRetained
    rejectedAt
}
```

---

## 200. SchedulerRestartRequired

Event type:

```text
scheduler.configuration.restart-required
```

Payload:

```text
SchedulerRestartRequiredPayload {
    configurationRevision
    changeClasses[]
    currentSchedulerRemainsActive
    detectedAt
}
```

---

# Part XX — Diagnostics and Security Events

## 201. SchedulerDiagnosticsQueryStarted

Event type:

```text
scheduler.diagnostics.query-started
```

Payload:

```text
SchedulerDiagnosticsQueryStartedPayload {
    queryId
    callerAuthorityClass
    maximumJobs
    includeWorkers
    includeSchedules
    startedAt
}
```

---

## 202. SchedulerDiagnosticsQueryCompleted

Event type:

```text
scheduler.diagnostics.query-completed
```

Payload:

```text
SchedulerDiagnosticsQueryCompletedPayload {
    queryId
    jobsReturned
    tasksReturned
    schedulesReturned
    workersReturned
    completedAt
}
```

---

## 203. SchedulerDiagnosticsQueryRejected

Event type:

```text
scheduler.diagnostics.query-rejected
```

Payload:

```text
SchedulerDiagnosticsQueryRejectedPayload {
    queryId
    normalizedErrorCode
    rejectionClass
    rejectedAt
}
```

---

## 204. UnsafeJobInputBlocked

Event type:

```text
scheduler.security.unsafe-job-input-blocked
```

Payload:

```text
UnsafeJobInputBlockedPayload {
    taskId
    jobId?
    inputType
    findingClasses[]
    originalContentDiscarded
    blockedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
AUDIT_ONLY
```

---

## 205. SecretBearingJobInputBlocked

Event type:

```text
scheduler.security.secret-bearing-job-input-blocked
```

Payload:

```text
SecretBearingJobInputBlockedPayload {
    taskId
    jobId?
    ownerModule
    findingClasses[]
    originalContentDiscarded
    blockedAt
}
```

The detected secret is never included.

---

## 206. UnauthorizedTaskOperationBlocked

Event type:

```text
scheduler.security.unauthorized-task-operation-blocked
```

Payload:

```text
UnauthorizedTaskOperationBlockedPayload {
    operationType
    taskId?
    scheduleId?
    jobId?
    actorType
    authorityClass
    blockedAt
}
```

---

## 207. CrossModuleCancellationBlocked

Event type:

```text
scheduler.security.cross-module-cancellation-blocked
```

Payload:

```text
CrossModuleCancellationBlockedPayload {
    requesterModule
    ownerModule
    targetScope
    targetId
    blockedAt
}
```

---

## 208. UnsafeEventTriggerMappingBlocked

Event type:

```text
scheduler.security.unsafe-event-trigger-mapping-blocked
```

Payload:

```text
UnsafeEventTriggerMappingBlockedPayload {
    triggerId
    scheduleId
    sourceEventType
    mappingId
    findingClasses[]
    blockedAt
}
```

---

# Part XXI — Consumed Events

## 209. Events Consumed by Scheduler

Scheduler may consume safe facts from other modules through explicit adapters.

Scheduler does not infer job creation from arbitrary events.

---

## 210. ApplicationShutdownStarted

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
STOPPING
```

---

## 211. ConfigurationSnapshotActivated

Potential source:

```text
configuration.snapshot.activated
```

Scheduler may reevaluate:

- queue capacities;
- concurrency;
- retry defaults;
- resource thresholds;
- shutdown deadlines;
- diagnostics;
- schedule configuration.

---

## 212. ResourcePressureChanged

Potential source:

```text
runtime.resource-pressure.changed
telemetry.resource.pressure-changed
```

Scheduler may:

- reduce background concurrency;
- defer resource-heavy jobs;
- protect interactive work;
- resume deferred work after recovery.

---

## 213. RuntimeCapacityChanged

Potential source:

```text
runtime.capacity.changed
```

Scheduler may reevaluate jobs that dispatch into Runtime.

---

## 214. ProviderAvailabilityChanged

Potential source:

```text
provider-management.provider.availability-changed
```

Only explicitly registered tasks may use this as a trigger or resource signal.

---

## 215. SecretRevoked

Potential source:

```text
secret-management.secret.revoked
```

Scheduler may:

- cancel authorized jobs dependent on the revoked secret reference;
- block future attempts;
- schedule safe cleanup.

No secret material is consumed.

---

## 216. EventBusFailed

Potential source:

```text
event-bus.lifecycle.failed
```

Event-triggered schedules may degrade.

Time-based scheduling may continue.

---

# Part XXII — Event Ordering

## 217. Scheduler Startup Ordering

```text
SchedulerInitializationStarted
    ↓
SchedulerRecoveryStarted?
    ↓
SchedulerRecoveryCompleted?
    ↓
SchedulerReady
    ↓
SchedulerStarted
```

---

## 218. Task Ordering

```text
TaskValidationStarted
    ↓
TaskRegistered
    ↓
TaskEnabled
```

Possible later paths:

```text
TaskPauseStarted → TaskPaused
TaskUpdated
TaskDisabled
TaskRemovalStarted → TaskRemoved
```

---

## 219. Trigger Ordering

```text
TriggerArmingStarted
    ↓
TriggerArmed
    ↓
TriggerFired
    ↓
TriggerWaitingNext / TriggerExhausted
```

---

## 220. Job Ordering

```text
JobCreationStarted
    ↓
JobCreated
    ↓
JobScheduled
    ↓
JobReady
    ↓
JobQueued
    ↓
JobDispatchStarted
    ↓
AttemptCreated
    ↓
JobDispatched
    ↓
AttemptStarted
    ↓
JobStarted
    ↓
terminal outcome or retry
```

---

## 221. Retry Ordering

```text
AttemptFailed / AttemptTimedOut / AttemptInterrupted
    ↓
RetryEvaluationStarted
    ↓
JobRetryScheduled
    ↓
JobRetryReady
    ↓
AttemptCreated
```

---

## 222. Timeout Ordering

```text
AttemptTimeoutStarted
    ↓
AttemptTimedOut
    ↓
JobTimedOut or JobRetryScheduled
```

If physical execution ignores cancellation:

```text
AttemptAbandoned
    ↓
JobAbandoned
```

---

## 223. Shutdown Ordering

```text
SchedulerShutdownRequested
    ↓
SchedulerShutdownStarted
    ↓
SchedulerShutdownTriggersStopped
    ↓
SchedulerDrainStarted
    ↓
SchedulerDrainCompleted
    ↓
SchedulerShutdownCancellationStarted?
    ↓
SchedulerShutdownStatePersisted?
    ↓
SchedulerShutdownWorkersStopped
    ↓
SchedulerShutdownCompleted
```

---

# Part XXIII — Duplicate, Stale, and Late Events

## 224. Event Deduplication

Consumers deduplicate using:

```text
eventId
```

or:

```text
entityId + stateVersion + eventType
```

---

## 225. Job Versioning

Job consumers compare:

```text
jobId
stateVersion
attemptNumber
```

An older `JobQueued` must not overwrite a newer `JobStarted` or terminal outcome.

---

## 226. Attempt Finality

After an authoritative terminal attempt event:

```text
AttemptSucceeded
AttemptFailed
AttemptCanceled
AttemptTimedOut
AttemptAbandoned
AttemptInterrupted
```

a later completion report cannot publish another authoritative terminal event.

It may publish:

```text
AttemptLateCompletionObserved
DuplicateAttemptCompletionBlocked
```

---

## 227. Schedule Occurrence Deduplication

Recurring occurrences use:

```text
scheduleId
occurrenceId
```

One occurrence produces one authoritative occurrence terminal outcome.

---

## 228. Recovery Events

Recovery events must include:

```text
recoveryOperationId
```

to prevent stale recovery results from overwriting a newer Scheduler instance.

---

# Part XXIV — Publication and Sampling Rules

## 229. High-Volume Events

May be sampled, throttled, aggregated, or metrics-only:

```text
TriggerEvaluationStarted
JobProgressReported
QueueItemClaimed
WorkerBusy
TaskStatePersisted
ScheduleStatePersisted
JobStatePersisted
AttemptStatePersisted
ConcurrencyLeaseWaiting
ResourceReservationWaiting
```

---

## 230. Events That Must Not Be Sampled Away

```text
SchedulerFailed
TaskInvalidated
ScheduleInvalidated
TriggerFailed
MisfireEvaluationFailed
JobCreationRejected
JobFailed
JobTimedOut
JobAbandoned
AttemptOutcomeBecameUncertain
DuplicateAttemptCompletionBlocked
JobRetryExhausted
CancellationTimedOut
SchedulerQueueFailed
WorkerFailed
DependencyCycleDetected
SchedulerPersistenceFailed
SchedulerRecoveryFailed
SchedulerRecoveryTimedOut
SchedulerShutdownTimedOut
SchedulerShutdownFailed
UnsafeJobInputBlocked
SecretBearingJobInputBlocked
UnauthorizedTaskOperationBlocked
```

---

## 231. Coalescing Rules

May be coalesced:

```text
SchedulerQueueBackpressured
DispatcherBackpressured
WorkerSlowDetected
ResourceReservationWaiting
SchedulerResourcePressureDetected
JobProgressReported
```

Must not be coalesced:

```text
task registration/removal
job terminal outcomes
attempt terminal outcomes
retry exhaustion
security violations
schedule misfire decisions
recovery outcomes
shutdown outcomes
```

---

# Part XXV — Security Validation

## 232. Pre-Publication Validation

Every Scheduler event passes:

```text
schema validation
    ↓
bounded payload validation
    ↓
job-input and output rejection
    ↓
secret and user-content inspection
    ↓
visibility validation
    ↓
publication
```

---

## 233. Prohibited Event Fields

Scheduler events must never include:

```text
jobInput
jobOutput
rawWorkerResult
rawException
stackTrace
OCRText
translatedText
prompt
imageData
providerPayload
secretValue
authorizationHeader
rawCredential
```

---

## 234. Safe Metadata

Permitted examples:

```text
taskId
scheduleId
jobId
attemptId
workerId
priority
triggerType
state
failureClass
normalizedErrorCode
resourceClass
queueClass
attemptNumber
duration
count
```

---

# Part XXVI — Observability Mapping

## 235. Metrics Mapping

Events may feed:

```text
scheduler_lifecycle_transition_total
scheduler_tasks_registered_total
scheduler_schedules_active
scheduler_triggers_fired_total
scheduler_misfires_total
scheduler_jobs_created_total
scheduler_jobs_queued_total
scheduler_jobs_started_total
scheduler_jobs_succeeded_total
scheduler_jobs_failed_total
scheduler_jobs_retried_total
scheduler_jobs_timed_out_total
scheduler_jobs_abandoned_total
scheduler_queue_backpressure_total
scheduler_worker_failures_total
scheduler_resource_wait_total
scheduler_cancellation_timeout_total
scheduler_recovery_outcomes_total
scheduler_shutdown_outcomes_total
scheduler_security_blocks_total
```

---

## 236. Logging Mapping

Safe log fields:

```text
eventType
taskId
scheduleId
jobId
attemptId
workerId
state
priority
failureClass
normalizedErrorCode
resourceClass
correlationId
```

No event payload should be logged wholesale.

---

## 237. Trace Mapping

Scheduler traces may contain spans for:

```text
trigger evaluation
job creation
queue wait
resource acquisition
concurrency acquisition
worker execution
retry delay
cancellation
recovery
shutdown
```

No raw job content is attached.

---

# Part XXVII — Event Catalog Summary

## 238. Lifecycle Events

```text
SchedulerInitializationStarted
SchedulerRecoveryStarted
SchedulerReady
SchedulerStarted
SchedulerDegraded
SchedulerRecovered
SchedulerQuiescing
SchedulerDrainStarted
SchedulerDrainCompleted
SchedulerStopping
SchedulerTerminated
SchedulerFailed
```

## 239. Task and Schedule Events

```text
TaskValidationStarted
TaskRegistered
TaskEnabled
TaskPauseStarted
TaskPaused
TaskDisabled
TaskUpdated
TaskRemovalStarted
TaskRemoved
TaskRegistrationRejected
TaskInvalidated

ScheduleRegistered
ScheduleEnabled
SchedulePaused
ScheduleResumed
ScheduleUpdated
ScheduleExpired
ScheduleDisabled
ScheduleRemoved
ScheduleRegistrationRejected
ScheduleInvalidated
```

## 240. Trigger and Occurrence Events

```text
TriggerArmingStarted
TriggerArmed
TriggerEvaluationStarted
TriggerFired
TriggerWaitingNext
TriggerPaused
TriggerDisarmed
TriggerExhausted
TriggerFailed

ScheduleOccurrencePlanned
ScheduleOccurrenceDue
OverlapEvaluationStarted
OverlapAllowed
OverlapSkipped
OverlapQueued
OverlapExistingCancellationRequested
OverlapPendingJobReplaced
MisfireDetected
MisfireSkipped
MisfireRunOnceScheduled
MisfireCatchUpScheduled
MisfireRescheduledFromNow
MisfireEvaluationFailed
```

## 241. Job and Attempt Events

```text
JobCreationStarted
JobCreated
JobCreationRejected
JobDuplicateRejected
JobCoalesced
JobLinkedToExisting
JobScheduled
JobWaitingForTime
JobWaitingForDependency
JobWaitingForResource
JobWaitingForConcurrency
JobReady
JobQueued
JobAdmissionRejected
JobAdmissionTimedOut
JobDispatchStarted
JobDispatched
JobDispatchDeferred
JobDispatchFailed

AttemptCreated
AttemptConcurrencyAcquisitionStarted
AttemptResourceAcquisitionStarted
AttemptStarted
JobStarted
JobProgressReported
AttemptSucceeded
JobSucceeded
AttemptFailed
JobFailed
AttemptCancellationRequested
AttemptCanceled
JobCanceled
AttemptTimeoutStarted
AttemptTimedOut
JobTimedOut
AttemptAbandoned
JobAbandoned
AttemptInterrupted
JobInterrupted
AttemptOutcomeBecameUncertain
AttemptLateCompletionObserved
DuplicateAttemptCompletionBlocked
JobSkipped
JobExpired
```

## 242. Retry and Cancellation Events

```text
RetryEvaluationStarted
JobRetryScheduled
JobRetryReady
JobRetryDenied
JobRetryExhausted
JobRetryReconciliationRequired

CancellationRequested
CancellationPropagationStarted
CancellationAcknowledged
CancellationCompleted
CancellationTimedOut
CancellationRejected
```

## 243. Queue, Worker, Concurrency, and Resource Events

```text
SchedulerQueueInitialized
SchedulerQueueAvailable
SchedulerQueueBackpressured
SchedulerQueueRecovered
SchedulerQueueCriticalReserveUsed
SchedulerQueueCriticalReserveExhausted
QueueItemClaimed
QueueItemExpired
QueueItemDropped
SchedulerQueueDrainStarted
SchedulerQueueDrainCompleted
SchedulerQueueFailed

DispatcherStarted
DispatcherBackpressured
DispatcherRecovered
DispatcherFailed

WorkerRegistered
WorkerInitializationStarted
WorkerAvailable
WorkerBusy
WorkerDraining
WorkerDisabled
WorkerTerminated
WorkerFailed
WorkerHealthChanged
WorkerSlowDetected
WorkerRecoveryStarted
WorkerRecovered

ConcurrencyLeaseRequested
ConcurrencyLeaseWaiting
ConcurrencyLeaseAcquired
ConcurrencyLeaseActivated
ConcurrencyLeaseReleased
ConcurrencyLeaseExpired
ConcurrencyLeaseFailed

ResourceReservationRequested
ResourceReservationWaiting
ResourceReservationGranted
ResourceReservationActivated
ResourceReservationReleased
ResourceReservationExpired
ResourceReservationRevoked
ResourceReservationFailed
SchedulerResourcePressureDetected
SchedulerResourcePressureRecovered
```

## 244. Persistence, Recovery, Shutdown, and Security Events

```text
PersistenceAdapterRegistered
PersistenceAdapterAvailable
PersistenceAdapterDegraded
PersistenceAdapterUnavailable
PersistenceAdapterRecovered
TaskStatePersisted
ScheduleStatePersisted
JobStatePersisted
AttemptStatePersisted
SchedulerPersistenceFailed

SchedulerRecoveryLoadingStarted
SchedulerRecoveryEntitiesClassified
InterruptedAttemptDetected
UncertainAttemptReconciliationStarted
AttemptReconciled
RecoveryMisfirePolicyApplied
SchedulerRecoveryCompleted
SchedulerRecoveryPartiallyCompleted
SchedulerRecoveryFailed
SchedulerRecoveryTimedOut

SchedulerShutdownRequested
SchedulerShutdownStarted
SchedulerShutdownTriggersStopped
SchedulerShutdownCancellationStarted
SchedulerShutdownStatePersisted
SchedulerShutdownWorkersStopped
SchedulerShutdownCompleted
SchedulerShutdownTimedOut
SchedulerShutdownFailed

SchedulerConfigurationApplied
SchedulerConfigurationRejected
SchedulerRestartRequired

SchedulerDiagnosticsQueryStarted
SchedulerDiagnosticsQueryCompleted
SchedulerDiagnosticsQueryRejected

UnsafeJobInputBlocked
SecretBearingJobInputBlocked
UnauthorizedTaskOperationBlocked
CrossModuleCancellationBlocked
UnsafeEventTriggerMappingBlocked
```

---

# Part XXVIII — MVP Event Boundary

## 245. Required MVP Events

The MVP should implement:

```text
SchedulerReady
SchedulerStarted
SchedulerDegraded
SchedulerRecovered
SchedulerQuiescing
SchedulerDrainStarted
SchedulerDrainCompleted
SchedulerTerminated
SchedulerFailed

TaskRegistered
TaskEnabled
TaskPaused
TaskDisabled
TaskUpdated
TaskRemoved
TaskRegistrationRejected
TaskInvalidated

ScheduleRegistered
ScheduleEnabled
SchedulePaused
ScheduleUpdated
ScheduleExpired
ScheduleRemoved
ScheduleRegistrationRejected
ScheduleInvalidated

TriggerArmed
TriggerFired
TriggerExhausted
TriggerFailed

OverlapSkipped
OverlapQueued
OverlapExistingCancellationRequested
OverlapPendingJobReplaced
MisfireDetected
MisfireSkipped
MisfireRunOnceScheduled
MisfireCatchUpScheduled
MisfireEvaluationFailed

JobCreated
JobCreationRejected
JobDuplicateRejected
JobCoalesced
JobScheduled
JobWaitingForResource
JobWaitingForConcurrency
JobReady
JobQueued
JobAdmissionRejected
JobDispatchFailed
AttemptCreated
AttemptStarted
JobStarted
AttemptSucceeded
JobSucceeded
AttemptFailed
JobFailed
AttemptCancellationRequested
AttemptCanceled
JobCanceled
AttemptTimedOut
JobTimedOut
AttemptAbandoned
JobAbandoned
AttemptOutcomeBecameUncertain
AttemptLateCompletionObserved
DuplicateAttemptCompletionBlocked
JobSkipped
JobExpired

JobRetryScheduled
JobRetryDenied
JobRetryExhausted
JobRetryReconciliationRequired

CancellationCompleted
CancellationTimedOut
CancellationRejected

SchedulerQueueBackpressured
SchedulerQueueRecovered
SchedulerQueueCriticalReserveExhausted
SchedulerQueueFailed
DispatcherBackpressured
DispatcherFailed

WorkerAvailable
WorkerDegraded
WorkerFailed
WorkerRecovered

ConcurrencyLeaseAcquired
ConcurrencyLeaseReleased
ConcurrencyLeaseExpired
ResourceReservationGranted
ResourceReservationReleased
ResourceReservationExpired
ResourceReservationRevoked
ResourceReservationFailed

DependencyCycleDetected

SchedulerShutdownStarted
SchedulerShutdownCompleted
SchedulerShutdownTimedOut
SchedulerShutdownFailed

UnsafeJobInputBlocked
SecretBearingJobInputBlocked
UnauthorizedTaskOperationBlocked
CrossModuleCancellationBlocked
```

Note:

```text
WorkerDegraded
```

may be represented in implementation by:

```text
WorkerHealthChanged(currentHealth = DEGRADED)
```

instead of a separate event.

---

## 246. Optional MVP Events

May remain metrics-only:

```text
TriggerEvaluationStarted
ScheduleOccurrencePlanned
JobCreationStarted
JobWaitingForTime
JobProgressReported
QueueItemClaimed
WorkerBusy
ConcurrencyLeaseWaiting
ResourceReservationWaiting
TaskStatePersisted
ScheduleStatePersisted
JobStatePersisted
AttemptStatePersisted
SchedulerDiagnosticsQueryStarted
```

---

## 247. Deferred Events

May be deferred with durable and distributed capabilities:

```text
distributed leader events
remote worker events
multi-process claim events
durable lease events
cross-device recovery events
conditional-trigger events
battery-state scheduling events
machine-idle scheduling events
advanced preemption events
```

---

# Part XXIX — Event Decisions

## 248. Decisions

### Decision 1 — Events contain no job payloads

Only safe identities, state, policy, timing, counts, resource classes, and error codes are allowed.

### Decision 2 — Job and attempt events remain distinct

A retryable attempt failure does not necessarily terminate the job.

### Decision 3 — State commits before publication

Events never authorize state mutation retroactively.

### Decision 4 — Late completion is non-authoritative

It produces an observation event, not a replacement terminal event.

### Decision 5 — Recurring behavior is observable

Overlap and misfire decisions are explicit facts.

### Decision 6 — Queue and resource pressure may be aggregated

Terminal and security events must remain explicit.

### Decision 7 — Recovery uncertainty is observable

Interrupted and uncertain attempts require reconciliation events.

### Decision 8 — Scheduler events do not represent business state

Owning modules remain authoritative for OCR, Translation, Storage, and provider outcomes.

### Decision 9 — Event triggers require explicit mapping

Arbitrary Event Bus events do not automatically create jobs.

### Decision 10 — Security blocks use restricted visibility

Unsafe input and unauthorized operations are never ordinary public events.

---

# Part XXX — Open Decisions

## 249. Visibility Decisions

Still to finalize:

- which job events are visible to owning modules;
- whether `TaskRegistered` is audit-only;
- worker identity visibility;
- schedule-administration event visibility;
- diagnostics-event visibility.

---

## 250. Sampling Decisions

Still to finalize:

- job-progress cadence;
- queue-claim sampling;
- worker-busy sampling;
- resource-wait aggregation;
- trigger-evaluation sampling;
- persistence-success sampling.

---

## 251. Security Decisions

Still to finalize:

- security-event audit retention;
- unsafe input finding taxonomy;
- event-trigger mapping audit;
- cross-module cancellation escalation;
- whether duplicate completion is security or internal invariant severity.

---

## 252. Recovery Decisions

Still to finalize:

- reconciliation event granularity;
- uncertain external side-effect events;
- recovered retry event ordering;
- stale Scheduler-instance event handling;
- partial-recovery reporting.

---

# Part XXXI — Related Documents

## 253. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/STATE_MACHINE.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/WORK_QUEUE.md
docs/architecture/runtime/SCHEDULER.md
docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/scheduler/MODULE.md
03-infrastructure/scheduler/CONTRACT.md
03-infrastructure/scheduler/STATES.md

03-infrastructure/event-bus/EVENTS.md
03-infrastructure/logging/EVENTS.md
03-infrastructure/telemetry/EVENTS.md
```

Future Scheduler documents:

```text
03-infrastructure/scheduler/ERRORS.md
03-infrastructure/scheduler/README.md
```

---

## 254. Summary

Scheduler events expose safe execution facts for tasks, schedules, triggers, occurrences, jobs, attempts, queues, workers, resources, retries, cancellation, persistence, recovery, and shutdown.

The primary job event flow is:

```text
TriggerFired
    ↓
JobCreated
    ↓
JobScheduled
    ↓
JobReady
    ↓
JobQueued
    ↓
JobDispatched
    ↓
AttemptStarted
    ↓
JobStarted
    ↓
JobSucceeded / JobFailed / JobCanceled / JobTimedOut / JobAbandoned
```

The retry flow is:

```text
AttemptFailed / AttemptTimedOut / AttemptInterrupted
    ↓
RetryEvaluationStarted
    ↓
JobRetryScheduled
    ↓
JobRetryReady
    ↓
new AttemptCreated
```

The shutdown flow is:

```text
SchedulerShutdownRequested
    ↓
SchedulerShutdownStarted
    ↓
SchedulerShutdownTriggersStopped
    ↓
SchedulerDrainStarted
    ↓
SchedulerDrainCompleted
    ↓
SchedulerShutdownCancellationStarted?
    ↓
SchedulerShutdownWorkersStopped
    ↓
SchedulerShutdownCompleted
```

The event model guarantees:

- immutable past-tense facts;
- state commits before publication;
- no raw job inputs, outputs, exceptions, or secrets;
- task, schedule, job, and attempt remain distinct;
- attempt failure and job failure remain distinct;
- late completion cannot rewrite terminal state;
- overlap and misfire are explicit;
- queue and resource pressure can be aggregated;
- terminal and security facts are never sampled away;
- recovery uncertainty is observable;
- event-triggered jobs require explicit mappings;
- Scheduler events never replace owning-module business state.

This document is the event source of truth for subsequent Scheduler errors and implementation documentation.
