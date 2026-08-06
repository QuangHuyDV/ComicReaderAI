# Scheduler States

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Scheduler  
> **Document:** State Machines  
> **Path:** `03-infrastructure/scheduler/STATES.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/scheduler/MODULE.md`
> - `03-infrastructure/scheduler/CONTRACT.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/WORK_QUEUE.md`
> - `docs/architecture/runtime/SCHEDULER.md`
> - `docs/architecture/runtime/CANCELLATION.md`
> - `docs/architecture/runtime/RETRY_POLICY.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`

---

## 1. Purpose

This document defines the state machines owned by the Scheduler infrastructure module.

It covers:

- Scheduler lifecycle;
- task-definition lifecycle;
- schedule lifecycle;
- trigger lifecycle;
- recurring-occurrence lifecycle;
- job lifecycle;
- attempt lifecycle;
- queue-item lifecycle;
- queue lifecycle;
- worker lifecycle;
- worker-health lifecycle;
- concurrency-lease lifecycle;
- resource-reservation lifecycle;
- dependency-wait lifecycle;
- retry lifecycle;
- cancellation lifecycle;
- overlap-decision lifecycle;
- misfire lifecycle;
- dispatcher lifecycle;
- drain lifecycle;
- shutdown lifecycle;
- persistence lifecycle;
- recovery lifecycle;
- crash and reconciliation behavior;
- invalid transitions;
- concurrency and authority invariants.

This document does not define:

- field schemas;
- Scheduler API signatures;
- trigger syntax;
- retry formulas;
- Scheduler events;
- normalized Scheduler errors;
- concrete queues;
- concrete timers;
- concrete persistence technology;
- business workflow state.

---

## 2. State Ownership

Scheduler owns lifecycle state for:

```text
SchedulerInstance
TaskDefinition
ScheduleDefinition
Trigger
ScheduleOccurrence
JobInstance
JobAttempt
JobQueueItem
SchedulerQueue
Worker
WorkerHealth
ConcurrencyLease
ResourceReservation
DependencyWait
RetryDecision
CancellationRequest
OverlapDecision
MisfireEvaluation
Dispatcher
DrainOperation
ShutdownOperation
PersistenceAdapter
RecoveryOperation
```

Scheduler does not own lifecycle state for:

```text
RuntimeWorkItem
Pipeline
OCRJob
TranslationResult
ProviderDefinition
SecretDescriptor
ConfigurationSnapshot
EventBus
LogRecord
TelemetrySpan
DomainAggregate
```

Owning modules may request Scheduler actions.

They do not directly mutate Scheduler-owned state.

---

## 3. State-Machine Separation

Scheduler must not use one global state enumeration.

Independent state machines are required:

```text
SchedulerState
TaskDefinitionState
ScheduleState
TriggerState
ScheduleOccurrenceState
JobState
AttemptState
QueueItemState
SchedulerQueueState
WorkerState
WorkerHealthState
ConcurrencyLeaseState
ResourceReservationState
DependencyWaitState
RetryState
CancellationState
OverlapDecisionState
MisfireState
DispatcherState
DrainState
ShutdownState
PersistenceAdapterState
RecoveryState
```

This separation is necessary because:

- Scheduler may be `RUNNING` while one worker is `DEGRADED`;
- a task may be `ENABLED` while one schedule is `PAUSED`;
- a job may wait for resources while another attempt is running;
- one queue may be `BACKPRESSURED` while delayed scheduling remains healthy;
- one persistence adapter may be unavailable while in-memory jobs continue;
- a timed-out attempt may still be physically executing;
- recovery may run while normal scheduling remains disabled;
- one schedule may misfire without degrading the whole Scheduler.

---

## 4. State Principles

### 4.1 State represents accepted current truth

```text
State
    = current lifecycle condition

Event
    = immutable fact that a transition occurred
```

### 4.2 Task, schedule, job, and attempt remain distinct

```text
TaskDefinition
    ≠ ScheduleDefinition
    ≠ JobInstance
    ≠ JobAttempt
```

### 4.3 Retry creates a new attempt

A failed attempt never re-enters `RUNNING`.

### 4.4 One terminal outcome per attempt

Only one terminal attempt state may win.

### 4.5 Timeout is terminal

Late physical completion does not overwrite `TIMED_OUT` or `ABANDONED`.

### 4.6 Queue admission is not execution

```text
Job QUEUED
    ≠
Attempt RUNNING
```

### 4.7 Resource and concurrency authority precede execution

A worker cannot begin before required leases and reservations are committed.

### 4.8 Recurring behavior is explicit

Overlap and misfire decisions are separate lifecycle facts.

### 4.9 Shutdown is bounded

Quiesce, drain, cancellation, persistence, and worker shutdown all use finite deadlines.

### 4.10 Durable uncertainty is explicit

Unknown persistence or execution outcome must not be silently treated as success.

---

# Part I — Scheduler Lifecycle

## 5. SchedulerState

Canonical states:

```text
CREATED
INITIALIZING
RECOVERING
READY
RUNNING
DEGRADED
QUIESCING
DRAINING
STOPPING
TERMINATED
FAILED
```

Primary lifecycle:

```text
CREATED
    ↓
INITIALIZING
    ↓
RECOVERING?
    ↓
READY
    ↓
RUNNING
    ↓
QUIESCING
    ↓
DRAINING
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

The Scheduler instance exists but is not initialized.

Valid outgoing transitions:

```text
CREATED → INITIALIZING
CREATED → TERMINATED
```

---

## 7. INITIALIZING

Scheduler initializes:

- configuration;
- clock;
- task registry;
- schedule registry;
- trigger engine;
- queues;
- dispatcher;
- worker registry;
- resource controller;
- persistence adapters;
- diagnostics;
- Event Bus trigger adapter.

Valid outgoing transitions:

```text
INITIALIZING → RECOVERING
INITIALIZING → READY
INITIALIZING → DEGRADED
INITIALIZING → FAILED
INITIALIZING → STOPPING
```

`INITIALIZING → DEGRADED` is allowed only when safe core scheduling remains available.

---

## 8. RECOVERING

Durable or restart-recoverable state is being reconciled.

Activities may include:

- restoring tasks;
- restoring schedules;
- applying misfire policy;
- restoring pending jobs;
- marking interrupted attempts;
- reconciling uncertain state.

Valid outgoing transitions:

```text
RECOVERING → READY
RECOVERING → DEGRADED
RECOVERING → FAILED
RECOVERING → STOPPING
```

Normal trigger firing should remain blocked until recovery authority is committed.

---

## 9. READY

Core Scheduler components are available, but normal trigger evaluation and dispatch have not begun.

Valid outgoing transitions:

```text
READY → RUNNING
READY → DEGRADED
READY → STOPPING
READY → FAILED
```

---

## 10. RUNNING

Scheduler:

- evaluates triggers;
- creates jobs;
- admits jobs;
- dispatches attempts;
- supervises retries;
- manages resources;
- exposes diagnostics.

Valid outgoing transitions:

```text
RUNNING → DEGRADED
RUNNING → QUIESCING
RUNNING → FAILED
```

---

## 11. DEGRADED

Scheduler remains partially operational.

Possible causes:

- optional persistence unavailable;
- one worker unavailable;
- one resource monitor unavailable;
- queue pressure;
- event-trigger adapter unavailable;
- clock warning;
- retry backlog;
- diagnostics unavailable.

Properties:

- unsafe jobs remain rejected;
- bounded queues remain enforced;
- degraded capabilities are explicit;
- critical invariant failure must use `FAILED`.

Valid outgoing transitions:

```text
DEGRADED → RUNNING
DEGRADED → QUIESCING
DEGRADED → FAILED
```

---

## 12. QUIESCING

Scheduler stops normal new work.

Typical actions:

- stop recurring trigger creation;
- reject low-priority manual jobs;
- allow selected critical or shutdown jobs;
- freeze task and schedule mutation;
- prepare queues for drain.

Valid outgoing transitions:

```text
QUIESCING → DRAINING
QUIESCING → STOPPING
QUIESCING → FAILED
```

---

## 13. DRAINING

Scheduler allows selected queued and running jobs to complete within a finite deadline.

Properties:

- no normal trigger firing;
- no normal new queue admission;
- retry creation depends on shutdown policy;
- cancellation may begin for excluded work.

Valid outgoing transitions:

```text
DRAINING → STOPPING
DRAINING → FAILED
```

---

## 14. STOPPING

Scheduler:

- cancels remaining attempts;
- releases leases and reservations;
- persists recoverable state where configured;
- stops dispatcher;
- stops workers;
- closes trigger and queue infrastructure.

Valid outgoing transitions:

```text
STOPPING → TERMINATED
STOPPING → FAILED
```

---

## 15. TERMINATED

Scheduler accepts no new task, schedule, job, or control operation except safe status queries.

Terminal.

---

## 16. FAILED

Scheduler cannot preserve core safety or authority invariants.

Examples:

- queue corruption;
- attempt terminal-state conflict;
- task ownership registry corruption;
- active execution without valid leases;
- persistence authority corruption for required durable mode;
- cancellation authority corrupted.

Required behavior:

- stop normal trigger evaluation;
- block unsafe admission;
- request bounded shutdown;
- preserve safe diagnostics.

Valid outgoing transitions:

```text
FAILED → STOPPING
FAILED → TERMINATED
```

Direct `FAILED → RUNNING` is prohibited.

---

# Part II — Task Definition Lifecycle

## 17. TaskDefinitionState

Canonical states:

```text
DRAFT
VALIDATING
REGISTERED
ENABLED
PAUSING
PAUSED
DISABLING
DISABLED
UPDATING
REMOVING
REMOVED
REJECTED
INVALID
```

---

## 18. DRAFT

A task definition exists outside the active registry.

Valid outgoing transition:

```text
DRAFT → VALIDATING
```

---

## 19. VALIDATING

Checks include:

- identity;
- ownership;
- worker compatibility;
- input type;
- timeout;
- retry bounds;
- concurrency bounds;
- resource policy;
- overlap;
- misfire;
- persistence mode;
- authorization.

Valid outgoing transitions:

```text
VALIDATING → REGISTERED
VALIDATING → REJECTED
VALIDATING → INVALID
```

---

## 20. REGISTERED

The task exists in the registry but is not active.

Valid outgoing transitions:

```text
REGISTERED → ENABLED
REGISTERED → UPDATING
REGISTERED → REMOVING
REGISTERED → REJECTED
```

---

## 21. ENABLED

The task may produce and execute jobs.

Valid outgoing transitions:

```text
ENABLED → PAUSING
ENABLED → DISABLING
ENABLED → UPDATING
ENABLED → REMOVING
ENABLED → INVALID
```

---

## 22. PAUSING

New job creation and dispatch for the task are being stopped according to pause policy.

Valid outgoing transitions:

```text
PAUSING → PAUSED
PAUSING → ENABLED
PAUSING → DISABLED
```

---

## 23. PAUSED

No new normal jobs are created or dispatched for the task.

Pending and running jobs follow pause policy.

Valid outgoing transitions:

```text
PAUSED → ENABLED
PAUSED → DISABLING
PAUSED → UPDATING
PAUSED → REMOVING
```

---

## 24. DISABLING

Task shutdown policy is being applied.

Valid outgoing transitions:

```text
DISABLING → DISABLED
DISABLING → PAUSED
DISABLING → INVALID
```

---

## 25. DISABLED

The task remains registered but cannot produce or execute jobs.

Valid outgoing transitions:

```text
DISABLED → ENABLED
DISABLED → UPDATING
DISABLED → REMOVING
```

---

## 26. UPDATING

A new version is being validated while the current version remains authoritative.

Valid outgoing transitions:

```text
UPDATING → ENABLED
UPDATING → PAUSED
UPDATING → DISABLED
UPDATING → INVALID
```

Pending jobs follow the captured task-version policy.

---

## 27. REMOVING

Schedules, pending jobs, and registry references are being handled according to removal policy.

Valid outgoing transitions:

```text
REMOVING → REMOVED
REMOVING → DISABLED
REMOVING → INVALID
```

---

## 28. REMOVED

The task no longer exists in the active registry.

Terminal for that registration identity.

---

## 29. REJECTED

Registration was not accepted.

Terminal for that registration attempt.

---

## 30. INVALID

The task cannot safely produce or execute jobs.

Terminal for that task version.

---

# Part III — Schedule Lifecycle

## 31. ScheduleState

Canonical states:

```text
DRAFT
VALIDATING
REGISTERED
ENABLED
PAUSED
EXPIRED
DISABLING
DISABLED
UPDATING
REMOVING
REMOVED
REJECTED
INVALID
```

---

## 32. DRAFT

A schedule definition exists outside the registry.

---

## 33. VALIDATING

Checks include:

- task exists;
- trigger valid;
- timezone valid;
- interval valid;
- cron valid;
- overlap present;
- misfire present;
- input factory safe;
- start/end valid;
- persistence compatible.

Valid outgoing transitions:

```text
VALIDATING → REGISTERED
VALIDATING → REJECTED
VALIDATING → INVALID
```

---

## 34. REGISTERED

The schedule exists but is inactive.

Valid outgoing transitions:

```text
REGISTERED → ENABLED
REGISTERED → UPDATING
REGISTERED → REMOVING
```

---

## 35. ENABLED

The trigger engine may evaluate and create occurrences.

Valid outgoing transitions:

```text
ENABLED → PAUSED
ENABLED → EXPIRED
ENABLED → DISABLING
ENABLED → UPDATING
ENABLED → INVALID
```

---

## 36. PAUSED

No new occurrences are created.

Valid outgoing transitions:

```text
PAUSED → ENABLED
PAUSED → DISABLING
PAUSED → UPDATING
PAUSED → EXPIRED
```

---

## 37. EXPIRED

The schedule's end condition has passed.

No new occurrence is created.

Valid outgoing transitions:

```text
EXPIRED → UPDATING
EXPIRED → REMOVING
```

---

## 38. DISABLING

Future trigger evaluation is being stopped.

Valid outgoing transition:

```text
DISABLING → DISABLED
```

---

## 39. DISABLED

The schedule remains registered but inactive.

Valid outgoing transitions:

```text
DISABLED → ENABLED
DISABLED → UPDATING
DISABLED → REMOVING
```

---

## 40. UPDATING

A new schedule version is being validated while the current committed version remains authoritative.

Valid outgoing transitions:

```text
UPDATING → ENABLED
UPDATING → PAUSED
UPDATING → DISABLED
UPDATING → EXPIRED
UPDATING → INVALID
```

---

## 41. REMOVING

The schedule is being detached from trigger evaluation and persistence.

Valid outgoing transitions:

```text
REMOVING → REMOVED
REMOVING → DISABLED
```

---

## 42. REMOVED

Terminal.

---

## 43. REJECTED

Terminal for the registration attempt.

---

## 44. INVALID

The schedule cannot be evaluated safely.

Terminal for that schedule version.

---

# Part IV — Trigger Lifecycle

## 45. TriggerState

Canonical states:

```text
CREATED
ARMING
ARMED
EVALUATING
FIRED
WAITING_NEXT
PAUSED
DISARMING
DISARMED
EXHAUSTED
FAILED
```

---

## 46. CREATED

The trigger exists but is not active.

---

## 47. ARMING

The trigger is registered with the trigger engine.

Valid outgoing transitions:

```text
ARMING → ARMED
ARMING → FAILED
```

---

## 48. ARMED

The trigger is eligible for evaluation.

Valid outgoing transitions:

```text
ARMED → EVALUATING
ARMED → PAUSED
ARMED → DISARMING
ARMED → EXHAUSTED
ARMED → FAILED
```

---

## 49. EVALUATING

The trigger checks current time or external conditions.

Valid outgoing transitions:

```text
EVALUATING → FIRED
EVALUATING → WAITING_NEXT
EVALUATING → PAUSED
EVALUATING → EXHAUSTED
EVALUATING → FAILED
```

---

## 50. FIRED

One occurrence has been committed.

Valid outgoing transitions:

```text
FIRED → WAITING_NEXT
FIRED → EXHAUSTED
FIRED → FAILED
```

For one-shot triggers, `FIRED → EXHAUSTED`.

---

## 51. WAITING_NEXT

The trigger waits for the next eligible evaluation.

Valid outgoing transitions:

```text
WAITING_NEXT → EVALUATING
WAITING_NEXT → PAUSED
WAITING_NEXT → DISARMING
WAITING_NEXT → EXHAUSTED
WAITING_NEXT → FAILED
```

---

## 52. PAUSED

Trigger evaluation is suspended.

Valid outgoing transitions:

```text
PAUSED → ARMED
PAUSED → DISARMING
PAUSED → EXHAUSTED
```

---

## 53. DISARMING

Trigger infrastructure is being detached.

Valid outgoing transition:

```text
DISARMING → DISARMED
```

---

## 54. DISARMED

Terminal for that armed trigger instance.

---

## 55. EXHAUSTED

The trigger has no future occurrence.

Terminal.

---

## 56. FAILED

The trigger cannot continue safely.

Terminal for that trigger instance.

---

# Part V — Schedule Occurrence Lifecycle

## 57. ScheduleOccurrenceState

Canonical states:

```text
PLANNED
DUE
EVALUATING_OVERLAP
EVALUATING_MISFIRE
CREATING_JOB
CREATED_JOB
SKIPPED
MISFIRED
COALESCED
REPLACED
FAILED
```

---

## 58. PLANNED

A future occurrence identity exists.

---

## 59. DUE

The occurrence time has arrived or has been recovered as due.

Valid outgoing transitions:

```text
DUE → EVALUATING_OVERLAP
DUE → EVALUATING_MISFIRE
DUE → CREATING_JOB
```

---

## 60. EVALUATING_OVERLAP

Existing active or pending jobs are checked.

Valid outgoing transitions:

```text
EVALUATING_OVERLAP → CREATING_JOB
EVALUATING_OVERLAP → SKIPPED
EVALUATING_OVERLAP → COALESCED
EVALUATING_OVERLAP → REPLACED
EVALUATING_OVERLAP → FAILED
```

---

## 61. EVALUATING_MISFIRE

The lateness and misfire policy are evaluated.

Valid outgoing transitions:

```text
EVALUATING_MISFIRE → CREATING_JOB
EVALUATING_MISFIRE → SKIPPED
EVALUATING_MISFIRE → MISFIRED
EVALUATING_MISFIRE → FAILED
```

---

## 62. CREATING_JOB

A `JobInstance` is being created.

Valid outgoing transitions:

```text
CREATING_JOB → CREATED_JOB
CREATING_JOB → COALESCED
CREATING_JOB → REPLACED
CREATING_JOB → FAILED
```

---

## 63. Occurrence Terminal States

```text
CREATED_JOB
SKIPPED
MISFIRED
COALESCED
REPLACED
FAILED
```

Each occurrence reaches one terminal state.

---

# Part VI — Job Lifecycle

## 64. JobState

Canonical states:

```text
CREATED
VALIDATING
SCHEDULED
WAITING_TIME
WAITING_DEPENDENCY
WAITING_RESOURCE
WAITING_CONCURRENCY
READY
QUEUED
DISPATCHING
RUNNING
RETRY_WAIT
SUCCEEDED
FAILED
CANCELED
TIMED_OUT
ABANDONED
SKIPPED
EXPIRED
INTERRUPTED
UNCERTAIN
RECONCILING
```

---

## 65. CREATED

A job identity and immutable input exist.

Valid outgoing transition:

```text
CREATED → VALIDATING
```

---

## 66. VALIDATING

Checks:

- task version;
- input safety;
- deadline;
- priority;
- duplicate policy;
- idempotency;
- authorization;
- persistence compatibility.

Valid outgoing transitions:

```text
VALIDATING → SCHEDULED
VALIDATING → SKIPPED
VALIDATING → FAILED
```

---

## 67. SCHEDULED

The job is committed for future or immediate evaluation.

Valid outgoing transitions:

```text
SCHEDULED → WAITING_TIME
SCHEDULED → WAITING_DEPENDENCY
SCHEDULED → WAITING_RESOURCE
SCHEDULED → WAITING_CONCURRENCY
SCHEDULED → READY
SCHEDULED → CANCELED
SCHEDULED → EXPIRED
```

---

## 68. WAITING_TIME

The job cannot start before `notBefore` or retry time.

Valid outgoing transitions:

```text
WAITING_TIME → READY
WAITING_TIME → WAITING_DEPENDENCY
WAITING_TIME → CANCELED
WAITING_TIME → EXPIRED
```

---

## 69. WAITING_DEPENDENCY

At least one declared prerequisite is unresolved.

Valid outgoing transitions:

```text
WAITING_DEPENDENCY → WAITING_RESOURCE
WAITING_DEPENDENCY → WAITING_CONCURRENCY
WAITING_DEPENDENCY → READY
WAITING_DEPENDENCY → SKIPPED
WAITING_DEPENDENCY → FAILED
WAITING_DEPENDENCY → CANCELED
WAITING_DEPENDENCY → EXPIRED
```

---

## 70. WAITING_RESOURCE

Required resources are unavailable.

Valid outgoing transitions:

```text
WAITING_RESOURCE → WAITING_CONCURRENCY
WAITING_RESOURCE → READY
WAITING_RESOURCE → CANCELED
WAITING_RESOURCE → EXPIRED
WAITING_RESOURCE → FAILED
```

---

## 71. WAITING_CONCURRENCY

Required concurrency capacity is unavailable.

Valid outgoing transitions:

```text
WAITING_CONCURRENCY → WAITING_RESOURCE
WAITING_CONCURRENCY → READY
WAITING_CONCURRENCY → CANCELED
WAITING_CONCURRENCY → EXPIRED
WAITING_CONCURRENCY → FAILED
```

---

## 72. READY

All start conditions are satisfied.

Valid outgoing transitions:

```text
READY → QUEUED
READY → CANCELED
READY → EXPIRED
```

---

## 73. QUEUED

The job is admitted to the ready execution queue.

Valid outgoing transitions:

```text
QUEUED → DISPATCHING
QUEUED → CANCELED
QUEUED → EXPIRED
QUEUED → WAITING_RESOURCE
QUEUED → WAITING_CONCURRENCY
```

A resource or lease may become unavailable before dispatch.

---

## 74. DISPATCHING

Scheduler selects a worker and commits execution authority.

Valid outgoing transitions:

```text
DISPATCHING → RUNNING
DISPATCHING → WAITING_RESOURCE
DISPATCHING → WAITING_CONCURRENCY
DISPATCHING → FAILED
DISPATCHING → CANCELED
```

---

## 75. RUNNING

One attempt is executing.

Valid outgoing transitions:

```text
RUNNING → SUCCEEDED
RUNNING → FAILED
RUNNING → RETRY_WAIT
RUNNING → CANCELED
RUNNING → TIMED_OUT
RUNNING → ABANDONED
RUNNING → INTERRUPTED
RUNNING → UNCERTAIN
```

---

## 76. RETRY_WAIT

A retry has been approved and scheduled.

Valid outgoing transitions:

```text
RETRY_WAIT → WAITING_TIME
RETRY_WAIT → READY
RETRY_WAIT → CANCELED
RETRY_WAIT → EXPIRED
RETRY_WAIT → FAILED
```

A new attempt is created when retry execution begins.

---

## 77. SUCCEEDED

The job completed successfully.

Terminal.

---

## 78. FAILED

No retry remains or failure is permanent.

Terminal.

---

## 79. CANCELED

The job was canceled before or during execution.

Terminal.

---

## 80. TIMED_OUT

The logical execution deadline expired.

Terminal for the job when no retry is allowed.

When retry is allowed, the timed-out attempt is terminal and job may enter `RETRY_WAIT`.

---

## 81. ABANDONED

Scheduler removed logical authority while physical worker termination was unconfirmed.

Terminal.

---

## 82. SKIPPED

The job was intentionally not executed.

Examples:

- overlap policy;
- dependency policy;
- disabled task;
- shutdown policy;
- duplicate policy.

Terminal.

---

## 83. EXPIRED

The job missed its start or completion deadline before execution could complete.

Terminal.

---

## 84. INTERRUPTED

Execution stopped due to process shutdown, crash recovery, or worker loss.

Valid outgoing transitions:

```text
INTERRUPTED → RETRY_WAIT
INTERRUPTED → FAILED
INTERRUPTED → UNCERTAIN
INTERRUPTED → RECONCILING
```

---

## 85. UNCERTAIN

Scheduler cannot determine whether execution or persistence completed.

Valid outgoing transition:

```text
UNCERTAIN → RECONCILING
```

---

## 86. RECONCILING

Scheduler checks:

- attempt state;
- worker receipt;
- persistence record;
- idempotency record;
- external result reference;
- duplicate risk.

Valid outgoing transitions:

```text
RECONCILING → SUCCEEDED
RECONCILING → RETRY_WAIT
RECONCILING → FAILED
RECONCILING → CANCELED
RECONCILING → UNCERTAIN
```

---

# Part VII — Attempt Lifecycle

## 87. AttemptState

Canonical states:

```text
CREATED
ACQUIRING_CONCURRENCY
ACQUIRING_RESOURCES
STARTING
RUNNING
CANCELLATION_REQUESTED
TIMING_OUT
SUCCEEDED
FAILED
CANCELED
TIMED_OUT
ABANDONED
INTERRUPTED
UNCERTAIN
```

---

## 88. CREATED

An attempt identity exists.

---

## 89. ACQUIRING_CONCURRENCY

Required concurrency leases are being acquired.

Valid outgoing transitions:

```text
ACQUIRING_CONCURRENCY → ACQUIRING_RESOURCES
ACQUIRING_CONCURRENCY → STARTING
ACQUIRING_CONCURRENCY → CANCELED
ACQUIRING_CONCURRENCY → FAILED
```

---

## 90. ACQUIRING_RESOURCES

Required resources are being reserved.

Valid outgoing transitions:

```text
ACQUIRING_RESOURCES → STARTING
ACQUIRING_RESOURCES → CANCELED
ACQUIRING_RESOURCES → FAILED
```

---

## 91. STARTING

A worker is selected and invocation begins.

Valid outgoing transitions:

```text
STARTING → RUNNING
STARTING → FAILED
STARTING → CANCELED
STARTING → TIMED_OUT
```

---

## 92. RUNNING

Worker execution is active.

Valid outgoing transitions:

```text
RUNNING → SUCCEEDED
RUNNING → FAILED
RUNNING → CANCELLATION_REQUESTED
RUNNING → TIMING_OUT
RUNNING → INTERRUPTED
RUNNING → UNCERTAIN
```

---

## 93. CANCELLATION_REQUESTED

Cooperative cancellation has been signaled.

Valid outgoing transitions:

```text
CANCELLATION_REQUESTED → CANCELED
CANCELLATION_REQUESTED → SUCCEEDED
CANCELLATION_REQUESTED → FAILED
CANCELLATION_REQUESTED → TIMING_OUT
CANCELLATION_REQUESTED → ABANDONED
```

Completion may race cancellation.

Exactly one terminal state wins.

---

## 94. TIMING_OUT

Logical timeout has fired and cancellation is being requested.

Valid outgoing transitions:

```text
TIMING_OUT → TIMED_OUT
TIMING_OUT → ABANDONED
```

A late worker result is non-authoritative.

---

## 95. Attempt Terminal States

```text
SUCCEEDED
FAILED
CANCELED
TIMED_OUT
ABANDONED
INTERRUPTED
UNCERTAIN
```

`INTERRUPTED` and `UNCERTAIN` may trigger job-level recovery, but the original attempt never re-enters `RUNNING`.

---

# Part VIII — Queue Item Lifecycle

## 96. QueueItemState

Canonical states:

```text
CREATED
ADMITTING
QUEUED
CLAIMING
CLAIMED
DEQUEUED
EXPIRED
CANCELED
DROPPED
REJECTED
```

---

## 97. CREATED

A queue item exists but is not admitted.

---

## 98. ADMITTING

Capacity, priority, fairness, and Scheduler lifecycle are evaluated.

Valid outgoing transitions:

```text
ADMITTING → QUEUED
ADMITTING → REJECTED
ADMITTING → DROPPED
ADMITTING → CANCELED
```

---

## 99. QUEUED

The item is visible to the dispatcher.

Valid outgoing transitions:

```text
QUEUED → CLAIMING
QUEUED → EXPIRED
QUEUED → CANCELED
QUEUED → DROPPED
```

---

## 100. CLAIMING

A dispatcher is atomically attempting to claim the item.

Valid outgoing transitions:

```text
CLAIMING → CLAIMED
CLAIMING → QUEUED
CLAIMING → CANCELED
CLAIMING → EXPIRED
```

---

## 101. CLAIMED

One dispatcher owns the item.

Valid outgoing transitions:

```text
CLAIMED → DEQUEUED
CLAIMED → CANCELED
```

---

## 102. DEQUEUED

The queue item has been removed for dispatch.

Terminal for that queue item identity.

---

## 103. Queue Item Terminal States

```text
DEQUEUED
EXPIRED
CANCELED
DROPPED
REJECTED
```

---

# Part IX — Scheduler Queue Lifecycle

## 104. SchedulerQueueState

Canonical states:

```text
CREATED
INITIALIZING
AVAILABLE
BACKPRESSURED
PAUSED
DRAINING
STOPPING
TERMINATED
FAILED
```

---

## 105. INITIALIZING

Capacity, ordering, and fairness structures initialize.

Valid outgoing transitions:

```text
INITIALIZING → AVAILABLE
INITIALIZING → FAILED
```

---

## 106. AVAILABLE

The queue accepts eligible jobs.

Valid outgoing transitions:

```text
AVAILABLE → BACKPRESSURED
AVAILABLE → PAUSED
AVAILABLE → DRAINING
AVAILABLE → STOPPING
AVAILABLE → FAILED
```

---

## 107. BACKPRESSURED

Capacity or wait thresholds are exceeded.

Possible actions:

- reject low-priority jobs;
- defer recurring jobs;
- apply priority aging;
- coalesce replaceable jobs;
- reduce trigger firing;
- use critical reserve.

Valid outgoing transitions:

```text
BACKPRESSURED → AVAILABLE
BACKPRESSURED → DRAINING
BACKPRESSURED → FAILED
```

---

## 108. PAUSED

No new normal admission.

Existing items remain.

Valid outgoing transitions:

```text
PAUSED → AVAILABLE
PAUSED → DRAINING
PAUSED → STOPPING
```

---

## 109. DRAINING

No normal new admission.

Items leave until empty or deadline.

Valid outgoing transitions:

```text
DRAINING → AVAILABLE
DRAINING → STOPPING
DRAINING → FAILED
```

---

## 110. STOPPING

Queue workers and internal resources stop.

Valid outgoing transition:

```text
STOPPING → TERMINATED
```

---

## 111. TERMINATED

Terminal.

---

## 112. FAILED

Queue capacity, ordering, or claim authority cannot be trusted.

Terminal for that queue instance.

---

# Part X — Worker Lifecycle

## 113. WorkerState

Canonical states:

```text
UNREGISTERED
REGISTERED
INITIALIZING
AVAILABLE
BUSY
DRAINING
DISABLED
STOPPING
TERMINATED
FAILED
```

---

## 114. UNREGISTERED

The worker is unknown to the active registry.

---

## 115. REGISTERED

Descriptor accepted; worker not initialized.

Valid outgoing transitions:

```text
REGISTERED → INITIALIZING
REGISTERED → DISABLED
REGISTERED → TERMINATED
```

---

## 116. INITIALIZING

The worker validates dependencies and capabilities.

Valid outgoing transitions:

```text
INITIALIZING → AVAILABLE
INITIALIZING → DISABLED
INITIALIZING → FAILED
```

---

## 117. AVAILABLE

The worker may accept attempts.

Valid outgoing transitions:

```text
AVAILABLE → BUSY
AVAILABLE → DRAINING
AVAILABLE → DISABLED
AVAILABLE → STOPPING
AVAILABLE → FAILED
```

---

## 118. BUSY

The worker is executing one or more attempts within its capacity.

Valid outgoing transitions:

```text
BUSY → AVAILABLE
BUSY → DRAINING
BUSY → DISABLED
BUSY → STOPPING
BUSY → FAILED
```

---

## 119. DRAINING

No new attempt starts.

Current attempts follow shutdown policy.

Valid outgoing transitions:

```text
DRAINING → AVAILABLE
DRAINING → STOPPING
DRAINING → FAILED
```

---

## 120. DISABLED

The worker is intentionally unavailable.

Valid outgoing transitions:

```text
DISABLED → INITIALIZING
DISABLED → STOPPING
```

---

## 121. STOPPING

Resources are closing.

Valid outgoing transitions:

```text
STOPPING → TERMINATED
STOPPING → FAILED
```

---

## 122. TERMINATED

Terminal.

---

## 123. FAILED

The worker cannot safely execute attempts.

Terminal for that worker instance.

---

# Part XI — Worker Health Lifecycle

## 124. WorkerHealthState

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

- startup failures;
- execution failures;
- latency;
- timeout rate;
- cancellation response;
- resource leaks;
- duplicate completion;
- late completion.

---

## 125. UNKNOWN

Insufficient observations exist.

---

## 126. HEALTHY

Worker operates within policy.

---

## 127. SLOW

Execution latency exceeds warning thresholds.

Valid outgoing transitions:

```text
SLOW → HEALTHY
SLOW → DEGRADED
SLOW → FAILING
```

---

## 128. DEGRADED

Worker remains usable with reduced capacity or restrictions.

Valid outgoing transitions:

```text
DEGRADED → HEALTHY
DEGRADED → FAILING
DEGRADED → UNHEALTHY
```

---

## 129. FAILING

Failure rate exceeds policy.

Valid outgoing transitions:

```text
FAILING → RECOVERING
FAILING → UNHEALTHY
FAILING → DISABLED
```

---

## 130. UNHEALTHY

Worker should not receive normal attempts.

Valid outgoing transitions:

```text
UNHEALTHY → RECOVERING
UNHEALTHY → DISABLED
```

---

## 131. RECOVERING

Probes or reinitialization are occurring.

Valid outgoing transitions:

```text
RECOVERING → HEALTHY
RECOVERING → DEGRADED
RECOVERING → UNHEALTHY
```

---

## 132. DISABLED

Explicit administrative or policy disablement.

---

# Part XII — Concurrency Lease Lifecycle

## 133. ConcurrencyLeaseState

Canonical states:

```text
REQUESTED
WAITING
ACQUIRED
ACTIVE
RELEASING
RELEASED
EXPIRED
CANCELED
FAILED
```

---

## 134. REQUESTED

A lease request exists.

---

## 135. WAITING

Capacity is unavailable.

Valid outgoing transitions:

```text
WAITING → ACQUIRED
WAITING → EXPIRED
WAITING → CANCELED
WAITING → FAILED
```

---

## 136. ACQUIRED

The lease has been committed but execution has not started.

Valid outgoing transitions:

```text
ACQUIRED → ACTIVE
ACQUIRED → RELEASING
ACQUIRED → EXPIRED
```

---

## 137. ACTIVE

The lease authorizes execution.

Valid outgoing transitions:

```text
ACTIVE → RELEASING
ACTIVE → EXPIRED
ACTIVE → FAILED
```

---

## 138. RELEASING

Lease capacity is being returned.

Valid outgoing transition:

```text
RELEASING → RELEASED
```

---

## 139. Lease Terminal States

```text
RELEASED
EXPIRED
CANCELED
FAILED
```

An expired lease cannot authorize continued execution.

---

# Part XIII — Resource Reservation Lifecycle

## 140. ResourceReservationState

Canonical states:

```text
REQUESTED
EVALUATING
WAITING
RESERVED
ACTIVE
RELEASING
RELEASED
EXPIRED
REVOKED
FAILED
```

---

## 141. EVALUATING

Availability and policy are checked.

Valid outgoing transitions:

```text
EVALUATING → WAITING
EVALUATING → RESERVED
EVALUATING → FAILED
```

---

## 142. WAITING

The resource is not yet available.

Valid outgoing transitions:

```text
WAITING → RESERVED
WAITING → EXPIRED
WAITING → FAILED
```

---

## 143. RESERVED

Capacity is committed for the job.

Valid outgoing transitions:

```text
RESERVED → ACTIVE
RESERVED → RELEASING
RESERVED → EXPIRED
RESERVED → REVOKED
```

---

## 144. ACTIVE

The attempt is using the resource.

Valid outgoing transitions:

```text
ACTIVE → RELEASING
ACTIVE → REVOKED
ACTIVE → FAILED
```

---

## 145. RELEASING

Resource capacity is being returned.

Valid outgoing transition:

```text
RELEASING → RELEASED
```

---

## 146. Reservation Terminal States

```text
RELEASED
EXPIRED
REVOKED
FAILED
```

Revocation may trigger cancellation or degradation according to task policy.

---

# Part XIV — Dependency Wait Lifecycle

## 147. DependencyWaitState

Canonical states:

```text
CREATED
EVALUATING
WAITING
SATISFIED
FAILED
CANCELED
EXPIRED
CYCLE_DETECTED
```

---

## 148. EVALUATING

Dependencies are resolved and checked.

Valid outgoing transitions:

```text
EVALUATING → SATISFIED
EVALUATING → WAITING
EVALUATING → FAILED
EVALUATING → CYCLE_DETECTED
```

---

## 149. WAITING

At least one dependency remains unresolved.

Valid outgoing transitions:

```text
WAITING → EVALUATING
WAITING → SATISFIED
WAITING → FAILED
WAITING → CANCELED
WAITING → EXPIRED
```

---

## 150. Dependency Terminal States

```text
SATISFIED
FAILED
CANCELED
EXPIRED
CYCLE_DETECTED
```

---

# Part XV — Retry Lifecycle

## 151. RetryState

Canonical states:

```text
NOT_EVALUATED
EVALUATING
DENIED
SCHEDULED
WAITING
READY
EXHAUSTED
CANCELED
RECONCILIATION_REQUIRED
```

---

## 152. NOT_EVALUATED

No retry decision exists.

---

## 153. EVALUATING

Checks:

- failure class;
- attempt count;
- retry budget;
- idempotency;
- completion deadline;
- shutdown;
- policy version.

Valid outgoing transitions:

```text
EVALUATING → DENIED
EVALUATING → SCHEDULED
EVALUATING → EXHAUSTED
EVALUATING → RECONCILIATION_REQUIRED
```

---

## 154. SCHEDULED

A retry time and next attempt number are committed.

Valid outgoing transitions:

```text
SCHEDULED → WAITING
SCHEDULED → READY
SCHEDULED → CANCELED
```

---

## 155. WAITING

Retry delay has not elapsed.

Valid outgoing transitions:

```text
WAITING → READY
WAITING → CANCELED
WAITING → EXHAUSTED
```

---

## 156. READY

A new attempt may be created.

Terminal for one retry decision.

---

## 157. Retry Terminal States

```text
DENIED
READY
EXHAUSTED
CANCELED
RECONCILIATION_REQUIRED
```

---

# Part XVI — Cancellation Lifecycle

## 158. CancellationState

Canonical states:

```text
NOT_REQUESTED
REQUESTED
PROPAGATING
ACKNOWLEDGED
COMPLETED
PARTIALLY_COMPLETED
TIMED_OUT
REJECTED
```

---

## 159. REQUESTED

A valid cancellation request exists.

Valid outgoing transitions:

```text
REQUESTED → PROPAGATING
REQUESTED → REJECTED
REQUESTED → COMPLETED
```

A pending job may cancel immediately.

---

## 160. PROPAGATING

Cancellation is being sent to queues, workers, leases, and reservations.

Valid outgoing transitions:

```text
PROPAGATING → ACKNOWLEDGED
PROPAGATING → PARTIALLY_COMPLETED
PROPAGATING → TIMED_OUT
```

---

## 161. ACKNOWLEDGED

Affected components acknowledged cancellation.

Valid outgoing transitions:

```text
ACKNOWLEDGED → COMPLETED
ACKNOWLEDGED → PARTIALLY_COMPLETED
ACKNOWLEDGED → TIMED_OUT
```

---

## 162. Cancellation Terminal States

```text
COMPLETED
PARTIALLY_COMPLETED
TIMED_OUT
REJECTED
```

---

# Part XVII — Overlap Decision Lifecycle

## 163. OverlapDecisionState

Canonical states:

```text
NOT_EVALUATED
EVALUATING
ALLOW_NEW
SKIP_NEW
QUEUE_NEW
CANCEL_EXISTING
REPLACE_PENDING
REJECTED
FAILED
```

---

## 164. EVALUATING

Existing jobs in the overlap scope are inspected.

Valid outgoing transitions:

```text
EVALUATING → ALLOW_NEW
EVALUATING → SKIP_NEW
EVALUATING → QUEUE_NEW
EVALUATING → CANCEL_EXISTING
EVALUATING → REPLACE_PENDING
EVALUATING → REJECTED
EVALUATING → FAILED
```

All decision outcomes are terminal.

---

# Part XVIII — Misfire Lifecycle

## 165. MisfireState

Canonical states:

```text
NOT_DETECTED
DETECTED
EVALUATING
SKIPPED
RUN_ONCE_NOW
RUN_ALL_BOUNDED
RESCHEDULED_FROM_NOW
RESUMED_NEXT_OCCURRENCE
FAILED
```

---

## 166. DETECTED

A planned occurrence is late beyond the configured threshold.

Valid outgoing transition:

```text
DETECTED → EVALUATING
```

---

## 167. EVALUATING

The misfire policy and missed occurrence count are evaluated.

Valid outgoing transitions:

```text
EVALUATING → SKIPPED
EVALUATING → RUN_ONCE_NOW
EVALUATING → RUN_ALL_BOUNDED
EVALUATING → RESCHEDULED_FROM_NOW
EVALUATING → RESUMED_NEXT_OCCURRENCE
EVALUATING → FAILED
```

All outcomes are terminal for that evaluation.

---

# Part XIX — Dispatcher Lifecycle

## 168. DispatcherState

Canonical states:

```text
CREATED
INITIALIZING
IDLE
DISPATCHING
BACKPRESSURED
PAUSED
DRAINING
STOPPING
TERMINATED
FAILED
```

---

## 169. INITIALIZING

Queue, worker, lease, and resource integrations initialize.

Valid outgoing transitions:

```text
INITIALIZING → IDLE
INITIALIZING → FAILED
```

---

## 170. IDLE

No currently dispatchable job exists.

Valid outgoing transitions:

```text
IDLE → DISPATCHING
IDLE → PAUSED
IDLE → DRAINING
IDLE → STOPPING
```

---

## 171. DISPATCHING

Jobs are selected and execution authority is committed.

Valid outgoing transitions:

```text
DISPATCHING → IDLE
DISPATCHING → BACKPRESSURED
DISPATCHING → PAUSED
DISPATCHING → DRAINING
DISPATCHING → FAILED
```

---

## 172. BACKPRESSURED

Dispatch is constrained by:

- worker capacity;
- resources;
- concurrency;
- queue pressure;
- downstream Runtime capacity.

Valid outgoing transitions:

```text
BACKPRESSURED → DISPATCHING
BACKPRESSURED → IDLE
BACKPRESSURED → DRAINING
BACKPRESSURED → FAILED
```

---

## 173. PAUSED

No new dispatch occurs.

Valid outgoing transitions:

```text
PAUSED → IDLE
PAUSED → DISPATCHING
PAUSED → DRAINING
PAUSED → STOPPING
```

---

## 174. DRAINING

Only selected jobs are dispatched.

Valid outgoing transitions:

```text
DRAINING → STOPPING
DRAINING → FAILED
```

---

## 175. STOPPING

Dispatcher resources close.

Valid outgoing transition:

```text
STOPPING → TERMINATED
```

---

## 176. TERMINATED

Terminal.

---

## 177. FAILED

Dispatch authority cannot be trusted.

Terminal for that dispatcher instance.

---

# Part XX — Drain Lifecycle

## 178. DrainState

Canonical states:

```text
REQUESTED
VALIDATING
STOPPING_TRIGGERS
SELECTING_JOBS
DRAINING_QUEUES
WAITING_RUNNING
CANCELING_REMAINDER
FINALIZING
DRAINED
PARTIALLY_DRAINED
TIMED_OUT
FAILED
CANCELED
```

---

## 179. REQUESTED

A drain request exists.

---

## 180. VALIDATING

Checks:

- deadline;
- priorities;
- task scope;
- queue scope;
- shutdown policy.

Valid outgoing transitions:

```text
VALIDATING → STOPPING_TRIGGERS
VALIDATING → FAILED
VALIDATING → CANCELED
```

---

## 181. STOPPING_TRIGGERS

New normal occurrences are stopped.

Valid outgoing transitions:

```text
STOPPING_TRIGGERS → SELECTING_JOBS
STOPPING_TRIGGERS → FAILED
```

---

## 182. SELECTING_JOBS

Jobs eligible to complete are selected.

Valid outgoing transition:

```text
SELECTING_JOBS → DRAINING_QUEUES
```

---

## 183. DRAINING_QUEUES

Selected jobs continue through dispatch.

Valid outgoing transitions:

```text
DRAINING_QUEUES → WAITING_RUNNING
DRAINING_QUEUES → CANCELING_REMAINDER
DRAINING_QUEUES → TIMED_OUT
DRAINING_QUEUES → FAILED
```

---

## 184. WAITING_RUNNING

Running attempts are allowed to complete within deadline.

Valid outgoing transitions:

```text
WAITING_RUNNING → FINALIZING
WAITING_RUNNING → CANCELING_REMAINDER
WAITING_RUNNING → TIMED_OUT
```

---

## 185. CANCELING_REMAINDER

Remaining attempts receive cancellation.

Valid outgoing transitions:

```text
CANCELING_REMAINDER → FINALIZING
CANCELING_REMAINDER → TIMED_OUT
CANCELING_REMAINDER → FAILED
```

---

## 186. FINALIZING

Counts and remaining authority are summarized.

Valid outgoing transitions:

```text
FINALIZING → DRAINED
FINALIZING → PARTIALLY_DRAINED
FINALIZING → TIMED_OUT
FINALIZING → FAILED
```

---

## 187. Drain Terminal States

```text
DRAINED
PARTIALLY_DRAINED
TIMED_OUT
FAILED
CANCELED
```

---

# Part XXI — Shutdown Lifecycle

## 188. ShutdownState

Canonical states:

```text
NOT_STARTED
REQUESTED
QUIESCING
DRAINING
CANCELING
PERSISTING
STOPPING_WORKERS
STOPPING_INFRASTRUCTURE
TERMINATED
PARTIALLY_TERMINATED
TIMED_OUT
FAILED
```

---

## 189. REQUESTED

Shutdown authority and deadline are committed.

Valid outgoing transition:

```text
REQUESTED → QUIESCING
```

---

## 190. QUIESCING

Normal trigger and admission barriers are raised.

Valid outgoing transitions:

```text
QUIESCING → DRAINING
QUIESCING → CANCELING
QUIESCING → FAILED
```

---

## 191. DRAINING

Selected work completes.

Valid outgoing transitions:

```text
DRAINING → CANCELING
DRAINING → PERSISTING
DRAINING → STOPPING_WORKERS
DRAINING → TIMED_OUT
```

---

## 192. CANCELING

Remaining attempts receive cancellation.

Valid outgoing transitions:

```text
CANCELING → PERSISTING
CANCELING → STOPPING_WORKERS
CANCELING → TIMED_OUT
```

---

## 193. PERSISTING

Recoverable state is stored where configured.

Valid outgoing transitions:

```text
PERSISTING → STOPPING_WORKERS
PERSISTING → PARTIALLY_TERMINATED
PERSISTING → TIMED_OUT
PERSISTING → FAILED
```

---

## 194. STOPPING_WORKERS

Workers stop or become abandoned after grace.

Valid outgoing transitions:

```text
STOPPING_WORKERS → STOPPING_INFRASTRUCTURE
STOPPING_WORKERS → PARTIALLY_TERMINATED
STOPPING_WORKERS → TIMED_OUT
```

---

## 195. STOPPING_INFRASTRUCTURE

Queues, trigger engine, dispatcher, registries, and adapters stop.

Valid outgoing transitions:

```text
STOPPING_INFRASTRUCTURE → TERMINATED
STOPPING_INFRASTRUCTURE → PARTIALLY_TERMINATED
STOPPING_INFRASTRUCTURE → TIMED_OUT
STOPPING_INFRASTRUCTURE → FAILED
```

---

## 196. Shutdown Terminal States

```text
TERMINATED
PARTIALLY_TERMINATED
TIMED_OUT
FAILED
```

---

# Part XXII — Persistence Adapter Lifecycle

## 197. PersistenceAdapterState

Canonical states:

```text
UNREGISTERED
REGISTERED
INITIALIZING
AVAILABLE
DEGRADED
UNAVAILABLE
RECOVERING
STOPPING
TERMINATED
FAILED
```

---

## 198. INITIALIZING

Connection, schema, and capability checks execute.

Valid outgoing transitions:

```text
INITIALIZING → AVAILABLE
INITIALIZING → DEGRADED
INITIALIZING → UNAVAILABLE
INITIALIZING → FAILED
```

---

## 199. AVAILABLE

The adapter may persist configured entities.

Valid outgoing transitions:

```text
AVAILABLE → DEGRADED
AVAILABLE → UNAVAILABLE
AVAILABLE → RECOVERING
AVAILABLE → STOPPING
AVAILABLE → FAILED
```

---

## 200. DEGRADED

The adapter remains partially usable.

Valid outgoing transitions:

```text
DEGRADED → AVAILABLE
DEGRADED → UNAVAILABLE
DEGRADED → RECOVERING
DEGRADED → STOPPING
DEGRADED → FAILED
```

---

## 201. UNAVAILABLE

No durable writes are accepted.

Valid outgoing transitions:

```text
UNAVAILABLE → RECOVERING
UNAVAILABLE → STOPPING
UNAVAILABLE → FAILED
```

---

## 202. RECOVERING

Health checks, reconnect, or migration occur.

Valid outgoing transitions:

```text
RECOVERING → AVAILABLE
RECOVERING → DEGRADED
RECOVERING → UNAVAILABLE
RECOVERING → FAILED
```

---

## 203. STOPPING

Valid outgoing transition:

```text
STOPPING → TERMINATED
```

---

## 204. TERMINATED

Terminal.

---

## 205. FAILED

Persistence authority cannot be trusted.

Terminal for that adapter instance.

---

# Part XXIII — Recovery Lifecycle

## 206. RecoveryState

Canonical states:

```text
REQUESTED
LOADING
CLASSIFYING
RECONCILING
RESTORING
APPLYING_MISFIRE
FINALIZING
RECOVERED
PARTIALLY_RECOVERED
TIMED_OUT
FAILED
CANCELED
```

---

## 207. REQUESTED

A recovery operation exists.

---

## 208. LOADING

Durable tasks, schedules, jobs, and attempts are loaded.

Valid outgoing transitions:

```text
LOADING → CLASSIFYING
LOADING → FAILED
LOADING → TIMED_OUT
```

---

## 209. CLASSIFYING

Entities are classified as:

```text
RESTORABLE
MISFIRED
INTERRUPTED
UNCERTAIN
EXPIRED
INVALID
```

Valid outgoing transition:

```text
CLASSIFYING → RECONCILING
```

---

## 210. RECONCILING

Uncertain attempts and duplicate risk are evaluated.

Valid outgoing transitions:

```text
RECONCILING → RESTORING
RECONCILING → APPLYING_MISFIRE
RECONCILING → FAILED
RECONCILING → TIMED_OUT
```

---

## 211. RESTORING

Tasks, schedules, and jobs are restored.

Valid outgoing transitions:

```text
RESTORING → APPLYING_MISFIRE
RESTORING → FINALIZING
RESTORING → FAILED
RESTORING → TIMED_OUT
```

---

## 212. APPLYING_MISFIRE

Missed recurring occurrences apply explicit policy.

Valid outgoing transitions:

```text
APPLYING_MISFIRE → FINALIZING
APPLYING_MISFIRE → FAILED
APPLYING_MISFIRE → TIMED_OUT
```

---

## 213. FINALIZING

Recovery counts and authority are committed.

Valid outgoing transitions:

```text
FINALIZING → RECOVERED
FINALIZING → PARTIALLY_RECOVERED
FINALIZING → FAILED
```

---

## 214. Recovery Terminal States

```text
RECOVERED
PARTIALLY_RECOVERED
TIMED_OUT
FAILED
CANCELED
```

---

# Part XXIV — Cross-State Rules

## 215. Scheduler and Task Relationship

Only tasks in:

```text
ENABLED
```

may create normal jobs.

Tasks in:

```text
PAUSED
DISABLED
REMOVED
INVALID
```

must not create normal new jobs.

---

## 216. Schedule and Trigger Relationship

A schedule may have an armed trigger only when schedule state is:

```text
ENABLED
```

Pausing or disabling a schedule must pause or disarm its trigger.

---

## 217. Job and Attempt Relationship

```text
Job RUNNING
    ↔
exactly one current Attempt in STARTING/RUNNING/CANCELLATION_REQUESTED/TIMING_OUT
```

A job may have many historical terminal attempts.

It has at most one current active attempt.

---

## 218. Attempt and Lease Relationship

Before `AttemptState → STARTING`:

- required concurrency leases must be `ACQUIRED`;
- required resource reservations must be `RESERVED`.

Before `AttemptState → RUNNING`:

- leases and reservations must be `ACTIVE`.

---

## 219. Attempt Terminal Cleanup

When an attempt becomes terminal:

```text
leases → RELEASING → RELEASED
reservations → RELEASING → RELEASED
worker capacity returned
```

Cleanup failure does not rewrite the attempt outcome.

---

## 220. Queue and Job Relationship

A job in `QUEUED` must have one active queue item.

A dequeued queue item must not be claimed twice.

---

## 221. Retry and Attempt Relationship

A retry decision may be evaluated only after a terminal attempt outcome that policy considers retryable.

A new attempt is created only after retry state becomes `READY`.

---

## 222. Cancellation and Timeout Relationship

Cancellation and timeout may race.

Exactly one terminal attempt state wins.

Recommended authority precedence:

```text
explicit success committed first
    → SUCCEEDED

timeout committed first
    → TIMED_OUT

cancellation committed first
    → CANCELED

shutdown deadline after ignored cancellation
    → ABANDONED
```

---

## 223. Overlap and Occurrence Relationship

A recurring occurrence may create a job only after overlap decision permits it.

---

## 224. Misfire and Occurrence Relationship

Recovered late occurrences must apply misfire policy before job creation.

---

## 225. Scheduler and Persistence Relationship

If persistence mode is `IN_MEMORY`, persistence-adapter state does not affect core Scheduler readiness.

If a task requires durable mode, adapter unavailability may:

- block the task;
- degrade Scheduler;
- fail Scheduler when required by policy.

---

## 226. Scheduler and Dispatcher Relationship

When Scheduler is:

```text
RUNNING
```

Dispatcher may be:

```text
IDLE
DISPATCHING
BACKPRESSURED
```

When Scheduler is:

```text
QUIESCING
DRAINING
STOPPING
```

Dispatcher must not return to unrestricted normal dispatch.

---

# Part XXV — Invalid Transitions

## 227. Invalid Scheduler Transitions

```text
CREATED → RUNNING
FAILED → RUNNING
TERMINATED → READY
DRAINING → RUNNING during shutdown
```

---

## 228. Invalid Task Transitions

```text
DRAFT → ENABLED
REMOVED → ENABLED
INVALID → ENABLED
DISABLED → PAUSED
```

A removed task requires a new registration identity.

---

## 229. Invalid Schedule Transitions

```text
DRAFT → ENABLED
REMOVED → ENABLED
INVALID → ENABLED
EXPIRED → ENABLED without update
```

---

## 230. Invalid Job Transitions

```text
CREATED → RUNNING
QUEUED → SUCCEEDED
SUCCEEDED → RETRY_WAIT
FAILED → RUNNING
TIMED_OUT → SUCCEEDED
ABANDONED → SUCCEEDED
```

Late completion is non-authoritative.

---

## 231. Invalid Attempt Transitions

```text
CREATED → RUNNING
SUCCEEDED → RUNNING
FAILED → STARTING
TIMED_OUT → SUCCEEDED
ABANDONED → CANCELED
```

---

## 232. Invalid Queue Item Transitions

```text
REJECTED → QUEUED
DEQUEUED → CLAIMED
DROPPED → QUEUED
```

---

## 233. Invalid Lease Transitions

```text
RELEASED → ACTIVE
EXPIRED → ACTIVE
FAILED → ACQUIRED
```

---

## 234. Invalid Reservation Transitions

```text
RELEASED → ACTIVE
REVOKED → ACTIVE
FAILED → RESERVED
```

---

## 235. Invalid Retry Transitions

```text
EXHAUSTED → READY
DENIED → SCHEDULED
CANCELED → READY
```

---

## 236. Invalid Shutdown Transitions

```text
TERMINATED → QUIESCING
FAILED → DRAINING
TIMED_OUT → RUNNING
```

---

# Part XXVI — Concurrency and Authority

## 237. Single Logical Writer

Scheduler is the single logical writer for:

- Scheduler lifecycle;
- task state;
- schedule state;
- trigger state;
- occurrence state;
- job state;
- attempt state;
- queue item claims;
- retry state;
- cancellation state;
- drain;
- shutdown;
- recovery.

Workers report outcomes.

They do not mutate authoritative state directly.

---

## 238. State Versioning

Mutable entities should include:

```text
stateVersion
```

Control operations may require:

```text
expectedStateVersion
```

---

## 239. Attempt Completion Race

Possible race:

```text
worker reports success
and timeout fires
```

Exactly one terminal state wins through an atomic transition.

The losing signal becomes a safe late observation.

---

## 240. Cancellation Race

Possible race:

```text
manual cancellation
shutdown cancellation
timeout cancellation
worker completion
```

Authority is resolved by committed state version and terminal transition rules.

---

## 241. Queue Claim Race

Exactly one dispatcher may successfully move:

```text
CLAIMING → CLAIMED
```

for one queue item.

---

## 242. Resource Acquisition Race

Resource and concurrency acquisition should use a deterministic order to avoid deadlock.

If partial acquisition fails:

```text
release already acquired authority
    ↓
return job to waiting state
```

---

## 243. Task Update Race

A job captures one committed task version.

A live task update does not partially mutate an existing job.

---

## 244. Schedule Evaluation Race

One occurrence identity may produce at most one authoritative occurrence terminal outcome.

Duplicate trigger evaluation must resolve through `OccurrenceId`.

---

## 245. Shutdown Admission Barrier

Jobs racing with quiesce are decided by a committed barrier:

```text
admitted before barrier
    → follows drain policy

not admitted before barrier
    → rejected, skipped, or persisted by shutdown policy
```

---

# Part XXVII — Crash and Recovery Rules

## 246. In-Memory MVP Crash

After process crash:

- in-memory queues are lost;
- pending in-memory jobs are lost;
- running attempts are interrupted;
- retry timers are lost;
- leases and reservations are lost;
- recurrence history since last durable state is lost.

The MVP must not claim restart recovery.

---

## 247. Durable Schedule Recovery

A durable schedule may be restored and evaluated for missed occurrences.

Misfire policy must be applied.

---

## 248. Durable Pending Job Recovery

Pending jobs may be restored to:

```text
WAITING_TIME
WAITING_DEPENDENCY
WAITING_RESOURCE
WAITING_CONCURRENCY
READY
```

according to persisted authority.

---

## 249. Running Attempt Recovery

A persisted `RUNNING` attempt found after restart becomes:

```text
INTERRUPTED
or
UNCERTAIN
```

It must not be assumed successful.

---

## 250. Uncertain Attempt Recovery

Recovery checks:

- worker receipt;
- external result reference;
- idempotency record;
- persistence transition;
- duplicate risk.

Possible outcomes:

```text
SUCCEEDED
RETRY_WAIT
FAILED
UNCERTAIN
```

---

## 251. Retry Recovery

A durable retry preserves:

```text
jobId
nextAttemptNumber
retryAt
retry policy revision
```

Recovery must not create multiple next attempts.

---

## 252. Lease Recovery

Concurrency leases and resource reservations are process-local unless explicitly durable.

Stale leases must expire during recovery.

---

# Part XXVIII — Command-to-State Mapping

## 253. Initialize Scheduler

```text
Scheduler CREATED
    ↓ INITIALIZING
    ↓ RECOVERING?
    ↓ READY
```

---

## 254. Register Task

```text
Task DRAFT
    ↓ VALIDATING
    ↓ REGISTERED
    ↓ ENABLED
```

---

## 255. Register Schedule

```text
Schedule DRAFT
    ↓ VALIDATING
    ↓ REGISTERED
    ↓ ENABLED

Trigger CREATED
    ↓ ARMING
    ↓ ARMED
```

---

## 256. Fire Trigger

```text
Trigger ARMED
    ↓ EVALUATING
    ↓ FIRED

Occurrence PLANNED
    ↓ DUE
    ↓ overlap/misfire evaluation
    ↓ CREATING_JOB
    ↓ CREATED_JOB
```

---

## 257. Execute Job

```text
Job CREATED
    ↓ VALIDATING
    ↓ SCHEDULED
    ↓ waiting states
    ↓ READY
    ↓ QUEUED
    ↓ DISPATCHING
    ↓ RUNNING
    ↓ terminal outcome / RETRY_WAIT
```

---

## 258. Execute Attempt

```text
Attempt CREATED
    ↓ ACQUIRING_CONCURRENCY
    ↓ ACQUIRING_RESOURCES
    ↓ STARTING
    ↓ RUNNING
    ↓ terminal outcome
```

---

## 259. Retry Job

```text
Attempt FAILED / TIMED_OUT / INTERRUPTED
    ↓ Retry EVALUATING
    ↓ SCHEDULED
    ↓ WAITING
    ↓ READY
    ↓ new Attempt CREATED
```

---

## 260. Cancel Job

```text
Cancellation REQUESTED
    ↓ PROPAGATING
    ↓ ACKNOWLEDGED
    ↓ COMPLETED / PARTIAL / TIMED_OUT
```

---

## 261. Shutdown Scheduler

```text
Scheduler RUNNING / DEGRADED
    ↓ QUIESCING
    ↓ DRAINING
    ↓ STOPPING
    ↓ TERMINATED
```

---

# Part XXIX — State Events

## 262. Event Principle

Scheduler events report committed transitions such as:

```text
SchedulerStarted
SchedulerDegraded
SchedulerRecovered
TaskRegistered
TaskEnabled
ScheduleActivated
TriggerFired
ScheduleMisfired
JobCreated
JobQueued
JobStarted
JobSucceeded
JobFailed
JobRetryScheduled
JobTimedOut
JobCanceled
JobAbandoned
QueueBackpressured
WorkerDegraded
ResourceReservationGranted
DrainCompleted
SchedulerTerminated
RecoveryCompleted
```

Detailed payloads belong in `EVENTS.md`.

---

# Part XXX — Security Invariants

## 263. Safe Input Before Scheduling

A job may enter `SCHEDULED` only after input validation succeeds.

---

## 264. No Secret Material

A job input containing secret material must end before scheduling in:

```text
FAILED
or rejected registration/creation outcome
```

Safe references may be accepted.

---

## 265. Ownership

A task, schedule, job, or cancellation operation must pass authorization before state mutation.

---

## 266. One Active Attempt

A job may have at most one active attempt.

---

## 267. One Terminal Attempt State

Exactly one terminal attempt state may commit.

---

## 268. Bounded Queue

Queue depth must never exceed configured capacity.

---

## 269. Bounded Retry

Attempt count must never exceed retry policy.

---

## 270. Bounded Shutdown

All drain, cancellation, persistence, worker-stop, and shutdown phases have finite deadlines.

---

## 271. No Exactly-Once Claim

No Scheduler state implies exactly-once business effect.

---

# Part XXXI — MVP State Boundary

## 272. Required MVP State Machines

The MVP must implement:

```text
SchedulerState
TaskDefinitionState
ScheduleState
TriggerState
ScheduleOccurrenceState
JobState
AttemptState
QueueItemState
SchedulerQueueState
WorkerState
WorkerHealthState
ConcurrencyLeaseState
ResourceReservationState
RetryState
CancellationState
OverlapDecisionState
MisfireState
DispatcherState
DrainState
ShutdownState
```

The MVP may simplify:

```text
DependencyWaitState
PersistenceAdapterState
RecoveryState
```

when durable execution and complex dependencies are deferred.

It must still preserve:

- separate task/job/attempt identities;
- terminal attempt finality;
- timeout and abandonment semantics;
- bounded queues;
- bounded retry;
- overlap and misfire decisions;
- resource/concurrency authority before execution;
- bounded shutdown.

---

## 273. MVP Simplifications

Allowed:

- in-memory only;
- local process only;
- no distributed workers;
- no durable retry;
- no complex DAG dependency engine;
- basic cron;
- static resource classes;
- simple fairness.

Not allowed:

- unbounded queues;
- unbounded retry;
- implicit overlap behavior;
- implicit misfire behavior;
- multiple active attempts per job;
- secret-bearing job input;
- infinite cancellation wait;
- late completion overwriting terminal state;
- exactly-once claims.

---

# Part XXXII — State Decisions

## 274. Decisions

### Decision 1 — Independent state machines

Scheduler, tasks, schedules, triggers, jobs, attempts, queues, workers, leases, retries, cancellation, and recovery remain separate.

### Decision 2 — Job and attempt are separate

Retry never reopens a terminal attempt.

### Decision 3 — One active attempt per job

Concurrent attempts require an explicit future policy and are not the default.

### Decision 4 — Timeout is terminal

Late completion is non-authoritative.

### Decision 5 — Resource and concurrency authority precede execution

Workers cannot start first and acquire later.

### Decision 6 — Overlap and misfire are explicit state decisions

Recurring behavior is deterministic.

### Decision 7 — Queue claim is atomic

One queue item has one dispatcher authority.

### Decision 8 — Shutdown uses barriers

Trigger, admission, dispatch, cancellation, and persistence phases are explicit.

### Decision 9 — Durable uncertainty is explicit

Interrupted or uncertain attempts require reconciliation.

### Decision 10 — In-memory MVP admits loss on crash

No recovery guarantee is implied.

### Decision 11 — Worker failure is isolated

It degrades worker health, not unrelated jobs.

### Decision 12 — Scheduler remains execution infrastructure

Business workflow state remains outside this module.

---

# Part XXXIII — Open Decisions

## 275. Lifecycle Decisions

Still to finalize:

- whether Scheduler quiesce can be reversed;
- task pause effect on running jobs;
- schedule update behavior for planned occurrences;
- worker reactivation after failure;
- dispatcher restart behavior;
- Scheduler `FAILED` threshold.

---

## 276. Job Decisions

Still to finalize:

- exact `TIMED_OUT` job versus attempt behavior under retry;
- `EXPIRED` semantics;
- `INTERRUPTED` persistence;
- duplicate-linked job state;
- output-reference retention;
- completion-deadline enforcement.

---

## 277. Queue Decisions

Still to finalize:

- ready-queue aging;
- delayed-queue promotion;
- critical reserve;
- fairness algorithm;
- queue item expiration;
- coalescing lifecycle.

---

## 278. Resource Decisions

Still to finalize:

- reservation acquisition order;
- reservation expiry;
- revocation;
- provider-quota lease behavior;
- UI-sensitive resource signal;
- GPU exclusivity.

---

## 279. Recovery Decisions

Still to finalize:

- uncertain attempt reconciliation;
- durable lease cleanup;
- retry restoration;
- recovered task-version compatibility;
- duplicate occurrence detection;
- maximum catch-up count.

---

# Part XXXIV — Documentation Order

## 280. Recommended Order

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

- Scheduler lifecycle events;
- task events;
- schedule and trigger events;
- occurrence and misfire events;
- job and attempt events;
- queue and dispatcher events;
- worker and health events;
- concurrency and resource events;
- retry and cancellation events;
- drain and shutdown events;
- persistence and recovery events.

---

# Part XXXV — Related Documents

## 281. Related Documents

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

03-infrastructure/configuration/
03-infrastructure/event-bus/
03-infrastructure/logging/
03-infrastructure/telemetry/
03-infrastructure/secret-management/
```

Future Scheduler documents:

```text
03-infrastructure/scheduler/EVENTS.md
03-infrastructure/scheduler/ERRORS.md
03-infrastructure/scheduler/README.md
```

---

## 282. Summary

Scheduler uses independent state machines for the module lifecycle, tasks, schedules, triggers, occurrences, jobs, attempts, queues, workers, leases, resources, retries, cancellation, dispatch, drain, shutdown, persistence, and recovery.

The main Scheduler lifecycle is:

```text
CREATED
    ↓
INITIALIZING
    ↓
RECOVERING?
    ↓
READY
    ↓
RUNNING
    ↓
QUIESCING
    ↓
DRAINING
    ↓
STOPPING
    ↓
TERMINATED
```

The main job lifecycle is:

```text
CREATED
    ↓
VALIDATING
    ↓
SCHEDULED
    ↓
WAITING_*
    ↓
READY
    ↓
QUEUED
    ↓
DISPATCHING
    ↓
RUNNING
    ↓
terminal outcome / RETRY_WAIT
```

The main attempt lifecycle is:

```text
CREATED
    ↓
ACQUIRING_CONCURRENCY
    ↓
ACQUIRING_RESOURCES
    ↓
STARTING
    ↓
RUNNING
    ↓
SUCCEEDED / FAILED / CANCELED / TIMED_OUT / ABANDONED
```

The architecture preserves these invariants:

- task, schedule, job, and attempt remain distinct;
- a job has at most one active attempt;
- each attempt has exactly one terminal outcome;
- retry creates a new attempt;
- timeout is terminal;
- late completion is non-authoritative;
- concurrency and resource authority precede execution;
- recurring overlap and misfire are explicit;
- queues, retries, cancellation, and shutdown are bounded;
- durable uncertainty requires reconciliation;
- in-memory MVP does not imply crash recovery;
- Scheduler does not own business workflow state.

This document is the state-machine source of truth for subsequent Scheduler events, errors, and implementation documentation.
