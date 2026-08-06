# Scheduler Contract

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Scheduler  
> **Document:** Public and Internal Contracts  
> **Path:** `03-infrastructure/scheduler/CONTRACT.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/scheduler/MODULE.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/WORK_QUEUE.md`
> - `docs/architecture/runtime/SCHEDULER.md`
> - `docs/architecture/runtime/CANCELLATION.md`
> - `docs/architecture/runtime/RETRY_POLICY.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`
> - `03-infrastructure/configuration/CONTRACT.md`
> - `03-infrastructure/event-bus/CONTRACT.md`
> - `03-infrastructure/logging/CONTRACT.md`
> - `03-infrastructure/telemetry/CONTRACT.md`

---

## 1. Purpose

This document defines the public and internal contracts of the Scheduler infrastructure module.

It specifies:

- Scheduler entry points;
- task definitions;
- schedule definitions;
- trigger contracts;
- job and attempt identities;
- job input and execution context;
- worker registration and execution;
- result and failure contracts;
- retry and backoff;
- timeout and cancellation;
- priority and fairness;
- concurrency controls;
- resource requirements and reservations;
- overlap and misfire policies;
- duplicate and idempotency controls;
- persistence and recovery adapters;
- lifecycle controls;
- diagnostics and status queries;
- testing contracts.

This document does not define:

- concrete queue data structures;
- concrete cron parser;
- concrete timer wheel;
- concrete persistence technology;
- concrete worker thread implementation;
- detailed state machines;
- Scheduler self-events;
- normalized Scheduler errors;
- business workflow semantics;
- UI wording.

Detailed lifecycles belong in `STATES.md`.

Self-events belong in `EVENTS.md`.

Normalized failures belong in `ERRORS.md`.

---

## 2. Contract Goals

Scheduler contracts must:

1. distinguish task, schedule, job, and attempt;
2. keep business meaning outside Scheduler;
3. support immediate, delayed, interval, cron, manual, event, and retry triggers;
4. preserve bounded queues, retries, and shutdown;
5. support priority and fairness;
6. support resource- and concurrency-aware dispatch;
7. support cancellation and timeout;
8. preserve one terminal outcome per attempt;
9. make overlap and misfire behavior explicit;
10. support safe duplicate and idempotency handling;
11. support in-memory MVP and future durable adapters;
12. avoid exactly-once claims;
13. protect interactive Runtime capacity;
14. keep inputs typed, immutable, bounded, and safe;
15. provide diagnostics without exposing job payloads;
16. isolate worker failures;
17. remain framework-independent.

---

## 3. Contract Groups

### 3.1 Core definitions

```text
TaskDefinition
ScheduleDefinition
TriggerDefinition
JobInstance
JobAttempt
JobInput
JobResult
JobFailure
```

### 3.2 Scheduler API

```text
Scheduler
TaskRegistry
ScheduleRegistry
JobRegistry
WorkerRegistry
```

### 3.3 Policy contracts

```text
RetryPolicy
BackoffPolicy
TimeoutPolicy
ConcurrencyPolicy
OverlapPolicy
MisfirePolicy
ResourcePolicy
ShutdownPolicy
DuplicatePolicy
IdempotencyPolicy
```

### 3.4 Execution contracts

```text
Worker
JobExecutionContext
JobProgressReporter
CancellationContext
ExecutionReceipt
```

### 3.5 Queue and dispatch contracts

```text
JobAdmissionRequest
JobAdmissionResult
DispatchDecision
ResourceReservation
ConcurrencyLease
```

### 3.6 Persistence and recovery contracts

```text
SchedulerStore
TaskStore
ScheduleStore
JobStore
AttemptStore
RecoveryCoordinator
RecoveryDecision
```

### 3.7 Lifecycle and diagnostics contracts

```text
SchedulerControl
SchedulerStatus
SchedulerDiagnosticsQuery
SchedulerDiagnosticsResult
TaskStatus
ScheduleStatus
JobStatus
WorkerStatus
```

---

# Part I — Core Identifiers

## 4. Core Identifiers

```text
SchedulerInstanceId
TaskId
ScheduleId
TriggerId
JobId
AttemptId
WorkerId
OwnerModuleId
ConcurrencyGroupId
ConcurrencyKey
ResourceReservationId
ResourceClassId
IdempotencyKey
OccurrenceId
RecoveryOperationId
CorrelationId
CausationId
ApplicationInstanceId
```

Rules:

- identifiers are opaque;
- identifiers must not embed secret or user content;
- `TaskId` is stable across task registrations;
- `ScheduleId` identifies one schedule rule;
- `JobId` identifies one logical execution;
- `AttemptId` identifies one execution attempt;
- retries preserve `JobId` and create a new `AttemptId`;
- `OccurrenceId` identifies one recurring occurrence;
- `IdempotencyKey` is safe and bounded.

---

# Part II — Task Contracts

## 5. TaskDefinition

```text
TaskDefinition<TInput> {
    taskId
    ownerModule
    handlerId
    inputType

    defaultPriority
    triggerPolicy

    timeoutPolicy
    retryPolicy
    concurrencyPolicy
    overlapPolicy
    misfirePolicy
    resourcePolicy
    shutdownPolicy
    duplicatePolicy
    idempotencyPolicy

    persistenceMode
    enabled

    metadata
}
```

---

## 6. Required Task Fields

Every registered task requires:

```text
taskId
ownerModule
handlerId
inputType
defaultPriority
timeoutPolicy
retryPolicy
concurrencyPolicy
overlapPolicy
misfirePolicy
resourcePolicy
shutdownPolicy
persistenceMode
```

---

## 7. Task Ownership

Only the owning module or authorized administrator may:

- register;
- update;
- enable;
- pause;
- disable;
- remove;
- manually execute;
- cancel its jobs;

unless an explicit cross-module authority exists.

---

## 8. Task Metadata

Task metadata may contain:

```text
description
category
maintenanceClass
diagnosticTags
version
```

It must not contain:

```text
secret
credential
raw user content
raw job input
provider response
```

---

## 9. Task Versioning

```text
TaskVersion
```

A task update must define whether pending jobs use:

```text
ORIGINAL_VERSION
LATEST_COMPATIBLE_VERSION
CANCEL_AND_RECREATE
```

The default should preserve the version captured when the job was created.

---

# Part III — Schedule and Trigger Contracts

## 10. ScheduleDefinition

```text
ScheduleDefinition<TInput> {
    scheduleId
    taskId
    trigger
    inputFactory
    enabled

    startAt?
    endAt?
    timezone?

    overlapPolicy?
    misfirePolicy?
    priorityOverride?
    policyOverrides?

    persistenceMode
    metadata
}
```

---

## 11. TriggerDefinition

```text
TriggerDefinition {
    triggerId
    triggerType
    triggerConfiguration
}
```

Trigger types:

```text
IMMEDIATE
DELAYED
INTERVAL
CRON
MANUAL
EVENT
RETRY
```

Future:

```text
CONDITIONAL
IDLE_TIME
NETWORK_AVAILABLE
RESOURCE_THRESHOLD
```

---

## 12. ImmediateTrigger

```text
ImmediateTrigger {
    fireOnce = true
}
```

Creates one job when activated.

---

## 13. DelayedTrigger

```text
DelayedTrigger {
    delay?
    targetTime?
}
```

Exactly one of:

```text
delay
targetTime
```

must be supplied.

Delay uses monotonic time where practical.

---

## 14. IntervalTrigger

```text
IntervalTrigger {
    interval
    mode
    initialDelay?
}
```

Modes:

```text
FIXED_RATE
FIXED_DELAY
```

Rules:

- interval must be positive;
- minimum interval is configured;
- fixed-rate scheduling uses planned occurrence times;
- fixed-delay scheduling uses previous completion time.

---

## 15. CronTrigger

```text
CronTrigger {
    expression
    timezone
    ambiguityPolicy
}
```

Rules:

- timezone is explicit;
- expression is validated;
- daylight-saving ambiguity is explicit;
- no implicit local-machine timezone unless allowed by policy.

Ambiguity policies:

```text
RUN_FIRST
RUN_SECOND
RUN_BOTH
SKIP
```

---

## 16. ManualTrigger

```text
ManualTrigger {
    allowedAuthorities[]
}
```

---

## 17. EventTrigger

```text
EventTrigger {
    eventType
    eventVersionRange
    filterId?
    mappingId
    duplicateWindow?
}
```

Rules:

- trigger registration is explicit;
- Event Bus payload is not copied blindly into job input;
- mapping produces a typed safe input;
- unauthorized events are rejected;
- filters are registered and bounded.

---

## 18. RetryTrigger

```text
RetryTrigger {
    sourceJobId
    previousAttemptId
    retryAt
    attemptNumber
}
```

Created only by Scheduler.

---

## 19. TriggerEvaluationResult

```text
TriggerEvaluationResult {
    outcome
    occurrenceId?
    scheduledAt?
    nextOccurrenceAt?
    warningCodes[]
    rejectionCode?
}
```

Possible outcomes:

```text
FIRED
NOT_DUE
SKIPPED
MISFIRED
DISABLED
REJECTED
FAILED
```

---

# Part IV — Job Contracts

## 20. JobInstance

```text
JobInstance<TInput> {
    jobId
    taskId
    taskVersion
    ownerModule

    scheduleId?
    triggerId
    occurrenceId?

    createdAt
    scheduledAt
    notBefore?
    startDeadline?
    completionDeadline?

    priority
    input

    idempotencyKey?
    duplicateGroupKey?
    concurrencyKey?

    retryState
    persistenceMode

    correlationId
    causationId?

    metadata
}
```

---

## 21. Job Input

```text
JobInput<T> {
    value
    inputType
    schemaVersion
    estimatedSize
    securityClassification
}
```

Rules:

- typed;
- immutable;
- bounded;
- serializable when persistence is enabled;
- safe for Scheduler storage;
- large content uses references;
- secret-bearing types are prohibited.

Allowed examples:

```text
ArtifactId
DocumentId
PageId
ChapterId
ProviderRequestId
ConfigurationRevision
SecretReferenceId
```

Prohibited examples:

```text
raw image bytes
OCR text
translated text
API key
access token
provider client
file stream
UI control
```

---

## 22. Job Priority

```text
CRITICAL
HIGH
NORMAL
LOW
BACKGROUND
MAINTENANCE
```

A job may override task default priority only when authorized.

---

## 23. Job Deadline Fields

```text
scheduledAt
notBefore
startDeadline
executionTimeout
completionDeadline
```

These fields are independent.

---

## 24. Job Metadata

Safe metadata may contain:

```text
source
reasonCode
configurationRevision
resourceClass
diagnosticTags
```

It must not contain raw business input.

---

# Part V — Attempt Contracts

## 25. JobAttempt

```text
JobAttempt {
    attemptId
    jobId
    attemptNumber

    createdAt
    startedAt?
    completedAt?

    workerId?
    resourceReservationId?
    concurrencyLeaseId?

    timeout
    cancellationContext

    result?
    failure?
}
```

---

## 26. Attempt Number

```text
attemptNumber >= 1
```

The first attempt is `1`.

Each retry increments by one.

---

## 27. Attempt Terminality

An attempt has exactly one terminal result:

```text
SUCCEEDED
FAILED
CANCELED
TIMED_OUT
ABANDONED
SKIPPED
EXPIRED
```

A terminal attempt never re-enters execution.

---

## 28. Execution Authority

Scheduler owns logical attempt authority.

A worker may report completion.

It cannot mutate attempt state directly.

---

# Part VI — Worker Contracts

## 29. Worker

```text
Worker<TInput> {
    workerId()
    taskIds()

    execute(
        input,
        context
    ) -> JobResult
}
```

---

## 30. JobExecutionContext

```text
JobExecutionContext {
    schedulerInstanceId
    taskId
    jobId
    attemptId
    attemptNumber

    scheduledAt
    startedAt
    deadline

    priority
    correlationId
    causationId?

    cancellation
    progressReporter
    resourceLeaseView
    safeMetadata
}
```

---

## 31. CancellationContext

```text
CancellationContext {
    isCancellationRequested()
    reason()
    deadline?
    throwIfCancellationRequested()
}
```

Cancellation is cooperative.

Workers must check cancellation at safe points.

---

## 32. JobProgressReporter

```text
JobProgressReporter {
    report(progress)
}
```

Progress is:

- optional;
- bounded;
- non-authoritative;
- throttleable;
- coalescible;
- droppable.

---

## 33. JobProgress

```text
JobProgress {
    fraction?
    stage?
    safeMessageCode?
    safeMetadata
}
```

No raw business content is allowed.

---

## 34. Worker Registration

```text
WorkerRegistry {
    register(workerDescriptor, workerFactory)
    unregister(workerId)
    findForTask(taskId)
    status(workerId)
}
```

---

## 35. WorkerDescriptor

```text
WorkerDescriptor {
    workerId
    supportedTaskIds[]
    ownerModule
    concurrencyCapabilities
    resourceCapabilities
    version
}
```

---

## 36. Worker Result Contract

A worker returns one `JobResult`.

Raw exceptions remain inside the Scheduler boundary.

---

# Part VII — Result and Failure Contracts

## 37. JobResult

```text
JobResult {
    outcome

    outputReference?
    safeSummary?

    retryRecommendation?
    failure?

    completedAt
    warnings[]
}
```

Possible outcomes:

```text
SUCCEEDED
FAILED
RETRY_REQUESTED
CANCELED
TIMED_OUT
ABANDONED
SKIPPED
EXPIRED
```

---

## 38. Output Reference

Scheduler may retain:

```text
ArtifactId
ResultId
RecordId
SnapshotId
```

It must not retain full business output.

---

## 39. JobFailure

```text
JobFailure {
    normalizedErrorCode
    failureClass
    retryable
    retryAfter?
    safeMessage
    safeMetadata
}
```

Failure classes:

```text
TRANSIENT
PERMANENT
TIMEOUT
CANCELLATION
RESOURCE_UNAVAILABLE
DEPENDENCY_FAILED
RATE_LIMITED
CONFIGURATION
SECURITY
UNKNOWN
```

---

## 40. Raw Exception Rule

Raw worker exceptions:

```text
Worker exception
    ↓
Scheduler boundary
    ↓
normalize
    ↓
safe JobFailure
```

They must not cross the public contract.

---

# Part VIII — Retry Contracts

## 41. RetryPolicy

```text
RetryPolicy {
    enabled
    maximumAttempts
    retryableFailureClasses[]
    backoffPolicy
    retryBudget?
    requireIdempotency
}
```

---

## 42. BackoffPolicy

```text
BackoffPolicy {
    strategy
    initialDelay
    maximumDelay
    multiplier?
    jitter?
    customPolicyId?
}
```

Strategies:

```text
NONE
FIXED
LINEAR
EXPONENTIAL
EXPONENTIAL_WITH_JITTER
CUSTOM_REGISTERED
```

---

## 43. RetryDecision

```text
RetryDecision {
    outcome
    nextAttemptNumber?
    retryAt?
    reasonCode
}
```

Possible outcomes:

```text
RETRY_SCHEDULED
RETRY_DENIED
RETRY_EXHAUSTED
RECONCILIATION_REQUIRED
```

---

## 44. Retry Safety

Retry is allowed only when:

- policy allows it;
- attempt count remains bounded;
- failure class is retryable;
- idempotency requirement is satisfied;
- shutdown policy permits it;
- completion deadline permits it.

---

# Part IX — Timeout Contracts

## 45. TimeoutPolicy

```text
TimeoutPolicy {
    executionTimeout
    cancellationGracePeriod
    abandonmentAfterGrace
}
```

---

## 46. Timeout Semantics

When execution timeout fires:

```text
RUNNING
    ↓
TIMED_OUT
```

Cancellation is requested.

If worker remains physically active beyond grace:

```text
ABANDONED
```

Late completion is non-authoritative.

---

# Part X — Concurrency Contracts

## 47. ConcurrencyPolicy

```text
ConcurrencyPolicy {
    globalLimit?
    perTaskLimit?
    perOwnerLimit?
    perKeyLimit?
    groupId?
    keySelectorId?
    acquisitionTimeout?
}
```

---

## 48. Concurrency Lease

```text
ConcurrencyLease {
    leaseId
    groupId
    key?
    units
    acquiredAt
    expiresAt?
}
```

A job may start only after required leases are acquired.

---

## 49. Concurrency Acquisition Result

```text
ConcurrencyAcquisitionResult {
    outcome
    leases[]
    retryAt?
    rejectionCode?
}
```

Possible outcomes:

```text
ACQUIRED
WAITING
TIMED_OUT
REJECTED
```

---

# Part XI — Resource Contracts

## 50. ResourcePolicy

```text
ResourcePolicy {
    requirements[]
    acquisitionMode
    waitTimeout?
    releaseOnPause
}
```

---

## 51. ResourceRequirement

```text
ResourceRequirement {
    resourceClass
    units
    exclusive
    minimumAvailability?
    reservationKey?
}
```

Resource classes:

```text
CPU
GPU
DISK_IO
NETWORK
MEMORY
UI_SENSITIVE
PROVIDER_QUOTA
CUSTOM
```

---

## 52. Resource Reservation

```text
ResourceReservation {
    reservationId
    jobId
    attemptId?
    resourceClass
    units
    acquiredAt
    expiresAt?
}
```

---

## 53. Resource Controller

```text
ResourceController {
    evaluate(requirements, context)
    acquire(requirements, context)
    release(reservations)
    status(resourceClass)
}
```

---

## 54. Resource Evaluation Result

```text
ResourceEvaluationResult {
    outcome
    availableUnits
    requiredUnits
    retryAt?
    reasonCode?
}
```

Possible outcomes:

```text
AVAILABLE
WAIT
UNAVAILABLE
REJECTED
```

---

# Part XII — Overlap and Misfire Contracts

## 55. OverlapPolicy

```text
ALLOW
SKIP_NEW
QUEUE_ONE
QUEUE_ALL_BOUNDED
CANCEL_PREVIOUS
REPLACE_PENDING
```

---

## 56. OverlapDecision

```text
OverlapDecision {
    outcome
    affectedJobIds[]
    reasonCode
}
```

Possible outcomes:

```text
ALLOW_NEW
SKIP_NEW
QUEUE_NEW
CANCEL_EXISTING
REPLACE_PENDING
REJECT
```

---

## 57. MisfirePolicy

```text
SKIP
RUN_ONCE_NOW
RUN_ALL_BOUNDED
RESCHEDULE_FROM_NOW
RESUME_NEXT_OCCURRENCE
```

---

## 58. MisfireDecision

```text
MisfireDecision {
    policy
    missedOccurrenceCount
    createdJobCount
    nextOccurrenceAt?
    warnings[]
}
```

---

# Part XIII — Duplicate and Idempotency Contracts

## 59. DuplicatePolicy

```text
DuplicatePolicy {
    mode
    duplicateWindow?
    keySelectorId?
}
```

Modes:

```text
ALLOW
REJECT
COALESCE
REPLACE_PENDING
LINK_TO_EXISTING
QUEUE_SEPARATELY
```

---

## 60. IdempotencyPolicy

```text
IdempotencyPolicy {
    mode
    keyRequired
    keySelectorId?
}
```

Modes:

```text
IDEMPOTENT
IDEMPOTENCY_KEY_REQUIRED
AT_MOST_ONCE
RECONCILIATION_REQUIRED
```

---

## 61. DuplicateDecision

```text
DuplicateDecision {
    outcome
    existingJobId?
    replacementJobId?
    reasonCode
}
```

Possible outcomes:

```text
ACCEPT_NEW
REJECT_DUPLICATE
COALESCE_WITH_EXISTING
REPLACE_EXISTING
LINK_TO_EXISTING
```

---

# Part XIV — Shutdown Policy Contracts

## 62. ShutdownPolicy

```text
ShutdownPolicy {
    behavior
    gracePeriod
    persistPending
    allowRetryDuringShutdown
}
```

Behaviors:

```text
ALLOW_TO_COMPLETE
CANCEL
ABANDON_AFTER_GRACE
PERSIST_AND_STOP
SKIP_IF_NOT_STARTED
```

---

## 63. SchedulerQuiesceRequest

```text
SchedulerQuiesceRequest {
    allowedPriorities[]
    allowedTaskIds[]
    rejectNewSchedules
    stopRecurringTriggers
    reasonCode
    effectiveAt
}
```

---

## 64. SchedulerShutdownRequest

```text
SchedulerShutdownRequest {
    deadline
    drainPriorities[]
    drainTaskIds[]
    cancelRemaining
    persistRecoverableState
    forceAfterDeadline
    reasonCode
}
```

---

## 65. SchedulerShutdownResult

```text
SchedulerShutdownResult {
    outcome

    jobsCompleted
    jobsCanceled
    jobsAbandoned
    jobsPersisted
    jobsLost

    workersTerminated
    workersAbandoned

    completedAt
    warnings[]
}
```

Possible outcomes:

```text
TERMINATED
PARTIALLY_TERMINATED
TIMED_OUT
FAILED
```

---

# Part XV — Scheduler API Contracts

## 66. Scheduler

```text
Scheduler {
    registerTask(request)
    updateTask(request)
    enableTask(taskId)
    pauseTask(taskId)
    disableTask(taskId)
    removeTask(taskId)

    registerSchedule(request)
    updateSchedule(request)
    enableSchedule(scheduleId)
    pauseSchedule(scheduleId)
    disableSchedule(scheduleId)
    removeSchedule(scheduleId)

    runNow(request)
    scheduleOnce(request)
    cancelJob(request)
    retryJob(request)

    status()
    diagnostics(query)
}
```

---

## 67. Task Registration Request

```text
TaskRegistrationRequest<TInput> {
    definition
    expectedExistingVersion?
    authority
}
```

---

## 68. Task Registration Result

```text
TaskRegistrationResult {
    outcome
    taskId
    taskVersion?
    warnings[]
    rejectionCode?
}
```

Possible outcomes:

```text
REGISTERED
UPDATED
UNCHANGED
REJECTED
VERSION_CONFLICT
```

---

## 69. Schedule Registration Request

```text
ScheduleRegistrationRequest<TInput> {
    definition
    expectedExistingVersion?
    authority
}
```

---

## 70. Schedule Registration Result

```text
ScheduleRegistrationResult {
    outcome
    scheduleId
    scheduleVersion?
    nextOccurrenceAt?
    warnings[]
    rejectionCode?
}
```

---

## 71. RunNowRequest

```text
RunNowRequest<TInput> {
    taskId
    input
    priorityOverride?
    deadlineOverrides?
    idempotencyKey?
    duplicatePolicyOverride?
    authority
    correlationId
    causationId?
}
```

---

## 72. ScheduleOnceRequest

```text
ScheduleOnceRequest<TInput> {
    taskId
    input
    targetTime?
    delay?
    priorityOverride?
    authority
    correlationId
    causationId?
}
```

---

## 73. JobCreationResult

```text
JobCreationResult {
    outcome
    jobId?
    existingJobId?
    scheduledAt?
    nextEligibleAt?
    warnings[]
    rejectionCode?
}
```

Possible outcomes:

```text
CREATED
LINKED_TO_EXISTING
COALESCED
REPLACED_PENDING
REJECTED
TIMED_OUT
```

---

## 74. CancelJobRequest

```text
CancelJobRequest {
    jobId
    scope
    authority
    reasonCode
    deadline?
}
```

Scopes:

```text
PENDING_ONLY
CURRENT_ATTEMPT
ENTIRE_JOB
TASK
CONCURRENCY_KEY
OWNER_MODULE
```

---

## 75. CancelJobResult

```text
CancelJobResult {
    outcome
    affectedJobIds[]
    affectedAttemptIds[]
    warnings[]
}
```

Possible outcomes:

```text
CANCELLATION_REQUESTED
CANCELED
PARTIALLY_CANCELED
NOT_FOUND
NOT_AUTHORIZED
ALREADY_TERMINAL
```

---

# Part XVI — Queue and Dispatch Contracts

## 76. JobAdmissionRequest

```text
JobAdmissionRequest {
    job
    queueClass
    deadline?
}
```

Queue classes:

```text
READY
DELAYED
RETRY
RESOURCE_WAIT
DEPENDENCY_WAIT
```

---

## 77. JobAdmissionResult

```text
JobAdmissionResult {
    outcome
    queueClass
    queueDepthAfter?
    nextEligibleAt?
    reasonCode?
}
```

Possible outcomes:

```text
ADMITTED
WAITING
REJECTED_CAPACITY
TIMED_OUT
DUPLICATE_HANDLED
SCHEDULER_NOT_RUNNING
```

---

## 78. DispatchDecision

```text
DispatchDecision {
    outcome
    workerId?
    resourceReservations[]
    concurrencyLeases[]
    nextEvaluationAt?
    reasonCode?
}
```

Possible outcomes:

```text
DISPATCH
WAIT_RESOURCE
WAIT_CONCURRENCY
WAIT_DEPENDENCY
WAIT_TIME
SKIP
CANCEL
REJECT
```

---

## 79. Fairness Context

```text
FairnessContext {
    ownerModule
    taskId
    priority
    enqueueSequence
    waitingDuration
    fairnessGroup?
}
```

---

# Part XVII — Persistence Contracts

## 80. PersistenceMode

```text
IN_MEMORY
DURABLE_SCHEDULE
DURABLE_PENDING_JOB
DURABLE_EXECUTION
```

MVP default:

```text
IN_MEMORY
```

---

## 81. SchedulerStore

```text
SchedulerStore {
    initialize()
    transaction(operation)
    health()
    shutdown()
}
```

---

## 82. TaskStore

```text
TaskStore {
    saveTask(definition)
    loadTasks()
    removeTask(taskId)
}
```

---

## 83. ScheduleStore

```text
ScheduleStore {
    saveSchedule(definition)
    loadSchedules()
    removeSchedule(scheduleId)
}
```

---

## 84. JobStore

```text
JobStore {
    saveJob(job)
    updateJobState(jobId, transition)
    loadRecoverableJobs()
    findByIdempotencyKey(key)
}
```

---

## 85. AttemptStore

```text
AttemptStore {
    saveAttempt(attempt)
    updateAttemptState(attemptId, transition)
    loadUncertainAttempts()
}
```

---

## 86. Persistence Requirements

Durable operations require:

- atomic state transition where needed;
- version checks;
- duplicate-safe writes;
- bounded recovery;
- explicit uncertainty;
- no raw secret or business payload;
- schema versioning.

---

# Part XVIII — Recovery Contracts

## 87. RecoveryCoordinator

```text
RecoveryCoordinator {
    recover(request)
        -> RecoveryResult
}
```

---

## 88. RecoveryRequest

```text
RecoveryRequest {
    schedulerInstanceId
    applicationInstanceId
    recoveryTime
    policy
}
```

---

## 89. RecoveryDecision

```text
RecoveryDecision {
    entityType
    entityId
    action
    reasonCode
}
```

Actions:

```text
RESTORE
RESCHEDULE
RETRY
MARK_INTERRUPTED
MARK_UNCERTAIN
SKIP
CANCEL
RECONCILE
```

---

## 90. RecoveryResult

```text
RecoveryResult {
    outcome
    tasksRestored
    schedulesRestored
    jobsRestored
    jobsRescheduled
    jobsSkipped
    attemptsMarkedUncertain
    warnings[]
}
```

Possible outcomes:

```text
RECOVERED
PARTIALLY_RECOVERED
FAILED
TIMED_OUT
```

---

# Part XIX — Lifecycle Contracts

## 91. SchedulerControl

```text
SchedulerControl {
    initialize(request)
    start()
    quiesce(request)
    drain(request)
    shutdown(request)
    status()
}
```

---

## 92. SchedulerInitializeRequest

```text
SchedulerInitializeRequest {
    schedulerInstanceId
    applicationInstanceId

    configuration
    clock
    taskRegistry
    workerRegistry
    resourceController
    persistenceAdapters?
    eventTriggerAdapter?
}
```

---

## 93. SchedulerStartResult

```text
SchedulerStartResult {
    outcome
    restoredTaskCount
    restoredScheduleCount
    activeWorkerCount
    degradedComponents[]
    startedAt?
}
```

Possible outcomes:

```text
RUNNING
RUNNING_DEGRADED
FAILED
```

---

## 94. SchedulerDrainRequest

```text
SchedulerDrainRequest {
    deadline
    priorities[]
    taskIds[]
    includeRetryQueue
    includeDelayedQueue
}
```

---

## 95. SchedulerDrainResult

```text
SchedulerDrainResult {
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
```

---

# Part XX — Status and Diagnostics Contracts

## 96. SchedulerStatus

```text
SchedulerStatus {
    lifecycleState
    healthState

    registeredTaskCount
    activeScheduleCount
    activeWorkerCount

    readyQueueDepth
    delayedQueueDepth
    retryQueueDepth
    resourceWaitCount
    dependencyWaitCount

    runningAttemptCount
    degradedWorkerCount
    unavailableWorkerCount

    persistenceMode
    persistenceHealth?
    nextOccurrenceAt?

    recentFailureSummary
}
```

---

## 97. TaskStatus

```text
TaskStatus {
    taskId
    taskVersion
    ownerModule
    lifecycleState
    enabled
    activeJobCount
    pendingJobCount
    recentOutcomeSummary
}
```

---

## 98. ScheduleStatus

```text
ScheduleStatus {
    scheduleId
    taskId
    lifecycleState
    triggerType
    nextOccurrenceAt?
    lastOccurrenceAt?
    lastOutcome?
    misfireCount
}
```

---

## 99. JobStatus

```text
JobStatus {
    jobId
    taskId
    lifecycleState
    currentAttemptId?
    attemptCount
    priority
    scheduledAt
    nextEligibleAt?
    terminalOutcome?
    normalizedErrorCode?
}
```

No job input is exposed by default.

---

## 100. WorkerStatus

```text
WorkerStatus {
    workerId
    lifecycleState
    healthState
    supportedTaskIds[]
    activeAttemptCount
    capacity
    recentFailureSummary
}
```

---

## 101. SchedulerDiagnosticsQuery

```text
SchedulerDiagnosticsQuery {
    taskIds?
    ownerModules?
    states?
    priorities?
    timeRange?
    includeSchedules
    includeWorkers
    includeRecentFailures
    maximumJobs
    callerAuthority
}
```

---

## 102. SchedulerDiagnosticsResult

```text
SchedulerDiagnosticsResult {
    schedulerStatus
    taskStatuses[]
    scheduleStatuses[]
    jobStatuses[]
    workerStatuses[]
    resourceStatuses[]
    warnings[]
}
```

No raw job input or output is returned.

---

# Part XXI — Authorization Contracts

## 103. SchedulerAuthority

```text
SchedulerAuthority {
    actorType
    actorId
    ownerModule?
    permissions[]
}
```

Permissions:

```text
REGISTER_TASK
UPDATE_TASK
REMOVE_TASK
REGISTER_SCHEDULE
RUN_TASK
CANCEL_JOB
RETRY_JOB
READ_DIAGNOSTICS
ADMINISTER_SCHEDULER
```

---

## 104. Authorization Decision

```text
SchedulerAuthorizationDecision {
    allowed
    reasonCode?
    effectiveScope?
}
```

---

# Part XXII — Configuration Contracts

## 105. SchedulerConfiguration

```text
SchedulerConfiguration {
    enabled

    queueCapacities
    globalConcurrency
    priorityPolicy
    fairnessPolicy

    defaultTimeoutPolicy
    defaultRetryPolicy
    defaultOverlapPolicy
    defaultMisfirePolicy

    resourcePolicies
    interactiveProtectionPolicy

    shutdownPolicy
    diagnosticsPolicy

    persistenceMode
    cronTimezoneDefault?
}
```

---

## 106. Queue Capacity Configuration

```text
SchedulerQueueCapacities {
    ready
    delayed
    retry
    resourceWait
    dependencyWait
    criticalReserve?
}
```

All capacities are bounded.

---

## 107. Live Configuration Changes

Potentially live:

- global concurrency;
- queue thresholds;
- fairness weights;
- default retry delays;
- resource thresholds;
- interactive protection;
- diagnostics limits.

---

## 108. Restart-Required Changes

Typically restart-required:

- persistence adapter;
- queue implementation;
- clock implementation;
- event-trigger adapter;
- worker execution technology;
- cron parser implementation.

---

# Part XXIII — Testing Contracts

## 109. TestScheduler

```text
TestScheduler {
    registerTask(...)
    advanceTime(duration)
    fireTrigger(triggerId)
    runUntilIdle()
    status()
    jobs()
    attempts()
}
```

---

## 110. ManualClock

```text
ManualClock {
    now()
    monotonicNow()
    advance(duration)
    setWallClock(time)
}
```

---

## 111. RecordingWorker

```text
RecordingWorker {
    executions()
    results()
    cancellations()
}
```

---

## 112. FaultInjectingWorker

Supported failures:

```text
START_FAILURE
TRANSIENT_FAILURE
PERMANENT_FAILURE
TIMEOUT
IGNORE_CANCELLATION
RESOURCE_LEAK
LATE_COMPLETION
DUPLICATE_COMPLETION
```

---

## 113. DeterministicTriggerEngine

Supports deterministic tests for:

- delayed trigger;
- interval trigger;
- cron;
- timezone;
- daylight-saving ambiguity;
- misfire;
- recovery.

---

## 114. ManualResourceController

Allows tests to set:

```text
CPU available
GPU available
NETWORK available
DISK_IO available
PROVIDER_QUOTA available
UI_SENSITIVE pressure
```

---

# Part XXIV — Validation Rules

## 115. Task Validation

Reject when:

- task ID invalid;
- owner missing;
- worker missing;
- unsupported input type;
- timeout unbounded;
- retry unbounded;
- concurrency unbounded;
- overlap missing for recurring task;
- misfire missing for recurring task;
- unsafe input type;
- unauthorized registration.

---

## 116. Schedule Validation

Reject when:

- task unknown;
- trigger invalid;
- cron invalid;
- timezone missing;
- interval below minimum;
- end before start;
- input factory unsafe;
- persistence incompatible;
- duplicate schedule conflict.

---

## 117. Job Validation

Reject when:

- input unsafe;
- input too large;
- deadline invalid;
- priority invalid;
- idempotency key invalid;
- owner mismatch;
- unsupported task version.

---

## 118. Worker Validation

Reject when:

- worker ID duplicate;
- task ownership conflict;
- unsupported task version;
- resource capabilities insufficient;
- unbounded concurrency;
- unsafe worker factory.

---

## 119. Retry Validation

Reject when:

- maximum attempts below one;
- maximum attempts unbounded;
- delay negative;
- maximum delay below initial delay;
- jitter invalid;
- retry enabled for at-most-once non-idempotent task.

---

## 120. Resource Validation

Reject when:

- units non-positive;
- unknown resource class;
- exclusive requirement conflicts;
- reservation key unsafe;
- acquisition timeout unbounded.

---

## 121. Persistence Validation

Reject when:

- input not serializable;
- schema version missing;
- persistence adapter unavailable for required mode;
- secret-bearing field detected;
- durable execution requested without recovery policy.

---

# Part XXV — Cross-Module Rules

## 122. Runtime

Scheduler may dispatch work into Runtime through an explicit adapter.

Scheduler does not own Runtime work-item state.

---

## 123. Event Bus

Event-triggered jobs require an explicit registered mapping.

Scheduler self-events never carry raw job input.

---

## 124. Logging

Scheduler logs only safe identifiers, policy names, states, timing, and normalized error codes.

---

## 125. Telemetry

Scheduler emits bounded metrics and traces with low-cardinality labels.

---

## 126. Secret Management

Job inputs may contain safe secret references only when the owning worker resolves them through Secret Management.

Secret material is prohibited.

---

## 127. Configuration

Configuration supplies Scheduler defaults.

Task-specific business rules remain owned by task owners.

---

# Part XXVI — Contract Decisions

## 128. Decisions

### Decision 1 — Task, Schedule, Job, and Attempt are distinct

Each has separate identity and lifecycle.

### Decision 2 — Job input is typed and bounded

Large data uses references.

### Decision 3 — Workers return one terminal result

Raw exceptions remain internal.

### Decision 4 — Retry creates a new attempt

The job identity remains stable.

### Decision 5 — Timeout ends logical authority

Late completion is non-authoritative.

### Decision 6 — Overlap and misfire are mandatory for recurring schedules

No implicit behavior.

### Decision 7 — Resource and concurrency acquisition precede dispatch

A worker cannot start before leases are acquired.

### Decision 8 — Scheduler does not promise exactly-once

Durable recovery may cause duplicates.

### Decision 9 — Persistence modes are explicit

The MVP defaults to in-memory.

### Decision 10 — Diagnostics exclude payloads

Only safe status and summaries are exposed.

### Decision 11 — Authorization is explicit

One module cannot administer another module's jobs by default.

### Decision 12 — Shutdown is bounded

Unresponsive attempts become abandoned.

---

# Part XXVII — Open Decisions

## 129. API Decisions

Still to finalize:

- exact language-level generic interfaces;
- task version compatibility;
- input factory contract;
- worker factory lifetime;
- progress contract;
- output-reference contract;
- manual trigger receipt.

---

## 130. Trigger Decisions

Still to finalize:

- cron expression standard;
- daylight-saving defaults;
- minimum interval;
- event-trigger duplicate window;
- event mapping registry;
- fixed-rate catch-up limits.

---

## 131. Queue Decisions

Still to finalize:

- priority aging;
- fairness weights;
- critical reserve;
- queue capacities;
- starvation threshold;
- deadline-first behavior;
- coalescing support.

---

## 132. Retry and Timeout Decisions

Still to finalize:

- default maximum attempts;
- default backoff;
- jitter source;
- cancellation grace;
- abandonment threshold;
- reconciliation after timeout.

---

## 133. Resource Decisions

Still to finalize:

- resource unit model;
- UI pressure signal;
- GPU capacity model;
- provider quota reservations;
- resource reservation expiry;
- deadlock avoidance.

---

## 134. Persistence Decisions

Still to finalize:

- durable schema;
- atomic transition boundaries;
- job input serialization;
- uncertain execution reconciliation;
- migration policy;
- retention of completed jobs.

---

# Part XXVIII — Documentation Order

## 135. Recommended Order

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

`STATES.md` should next define:

- Scheduler lifecycle;
- task lifecycle;
- schedule lifecycle;
- trigger lifecycle;
- job lifecycle;
- attempt lifecycle;
- queue lifecycle;
- worker lifecycle;
- worker-health lifecycle;
- concurrency lease lifecycle;
- resource reservation lifecycle;
- retry lifecycle;
- cancellation lifecycle;
- drain and shutdown lifecycle;
- persistence and recovery lifecycle.

---

# Part XXIX — Related Documents

## 136. Related Documents

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

03-infrastructure/configuration/CONTRACT.md
03-infrastructure/event-bus/CONTRACT.md
03-infrastructure/logging/CONTRACT.md
03-infrastructure/telemetry/CONTRACT.md
03-infrastructure/secret-management/CONTRACT.md
```

Future Scheduler documents:

```text
03-infrastructure/scheduler/STATES.md
03-infrastructure/scheduler/EVENTS.md
03-infrastructure/scheduler/ERRORS.md
03-infrastructure/scheduler/README.md
```

---

## 137. Summary

The Scheduler contract defines a shared background-execution boundary for task registration, trigger evaluation, job creation, queue admission, resource acquisition, worker dispatch, retries, timeout, cancellation, persistence, recovery, and diagnostics.

The main execution flow is:

```text
TaskDefinition
    ↓
Schedule / Trigger
    ↓
JobInstance
    ↓
Queue admission
    ↓
Concurrency and resource acquisition
    ↓
JobAttempt
    ↓
Worker execution
    ↓
JobResult
    ↓
Retry / terminal completion
```

The contract guarantees:

- task, schedule, job, and attempt remain distinct;
- inputs are typed, immutable, bounded, and safe;
- recurring schedules define overlap and misfire behavior;
- retries are bounded and create new attempts;
- timeout ends logical authority;
- late completion is non-authoritative;
- concurrency and resource leases precede execution;
- worker failures are isolated;
- queues and shutdown are bounded;
- exact-once execution is not promised;
- persistence and recovery are explicit extensions;
- diagnostics expose no job payloads;
- authorization is enforced;
- Scheduler remains separate from Runtime and business orchestration.

This document is the contract source of truth for subsequent Scheduler states, events, errors, and implementation documentation.
