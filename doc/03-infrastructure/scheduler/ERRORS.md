# Scheduler Errors

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Scheduler  
> **Document:** Errors and Warnings  
> **Path:** `03-infrastructure/scheduler/ERRORS.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/scheduler/MODULE.md`
> - `03-infrastructure/scheduler/CONTRACT.md`
> - `03-infrastructure/scheduler/STATES.md`
> - `03-infrastructure/scheduler/EVENTS.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/WORK_QUEUE.md`
> - `docs/architecture/runtime/SCHEDULER.md`
> - `docs/architecture/runtime/CANCELLATION.md`
> - `docs/architecture/runtime/RETRY_POLICY.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`
> - `03-infrastructure/event-bus/ERRORS.md`
> - `03-infrastructure/logging/ERRORS.md`
> - `03-infrastructure/telemetry/ERRORS.md`

---

## 1. Purpose

This document defines normalized errors and warnings owned by the Scheduler infrastructure module.

It covers:

- task-definition validation;
- schedule and trigger validation;
- job creation and input validation;
- duplicate and idempotency conflicts;
- queue admission and backpressure;
- dispatch and worker selection;
- attempt startup and execution;
- cancellation, timeout, and abandonment;
- retry and backoff;
- concurrency leases;
- resource reservations;
- dependency evaluation;
- overlap and misfire handling;
- persistence and recovery;
- drain and shutdown;
- authorization and security;
- lifecycle, concurrency, and invariant failures;
- retry and recovery guidance;
- warning semantics;
- cross-module normalization.

This document does not define:

- business errors from OCR, Translation, Storage, Provider Management, or other feature modules;
- raw worker exceptions;
- Event Bus errors;
- Logging errors;
- Telemetry errors;
- UI wording;
- exact retry delays;
- concrete queue or persistence exceptions.

---

## 2. Error Design Goals

Scheduler errors must:

1. distinguish task, schedule, job, and attempt failures;
2. distinguish queue rejection from worker execution failure;
3. distinguish attempt failure from terminal job failure;
4. distinguish timeout from cancellation and abandonment;
5. distinguish retry denial from retry exhaustion;
6. distinguish resource unavailability from queue capacity;
7. distinguish in-memory loss from durable uncertainty;
8. preserve normalized identifiers without exposing payloads;
9. never contain raw job input or output;
10. never contain secrets or user content;
11. support bounded retry and recovery;
12. avoid exactly-once implications;
13. preserve one terminal attempt outcome;
14. isolate worker failures;
15. keep shutdown bounded;
16. remain framework-independent.

---

## 3. Error Versus Outcome

These outcomes are not automatically errors:

```text
SKIPPED
COALESCED
LINKED_TO_EXISTING
WAITING_RESOURCE
WAITING_CONCURRENCY
MISFIRED
CANCELED
PARTIALLY_DRAINED
PARTIALLY_RECOVERED
```

They become errors only when policy, safety, durability, or caller expectations require stronger guarantees.

Examples:

```text
Recurring maintenance occurrence skipped by SKIP_NEW
    → expected outcome

Critical security job skipped because queue authority failed
    → critical error
```

---

## 4. Error Versus Warning

An error prevents safe registration, scheduling, dispatch, execution, recovery, or shutdown completion.

A warning describes degraded but safe continuation.

Examples:

```text
Optional worker unavailable
    → warning

Ready queue pressure
    → warning

Queue claim authority corrupted
    → fatal error

Late completion observed after timeout
    → warning

Secret-bearing input detected
    → critical error
```

---

## 5. Error Ownership

Scheduler owns errors concerning:

- task and schedule registration;
- trigger evaluation;
- job creation;
- job input safety;
- queue admission;
- dispatch authority;
- worker availability;
- Scheduler-level timeout and cancellation;
- retry policy;
- concurrency and resource authority;
- persistence and recovery;
- Scheduler lifecycle.

Scheduler does not own the original semantic error produced by a worker.

The worker reports a normalized safe `JobFailure`.

---

## 6. Canonical Error Model

```text
SchedulerError {
    errorId

    code
    category
    scope
    severity

    retryClass
    recoverability
    userActionRequired

    safeMessage
    developerMessage?

    recoveryActions[]
    retryAfter?

    schedulerInstanceId?
    taskId?
    scheduleId?
    triggerId?
    occurrenceId?
    jobId?
    attemptId?
    workerId?
    queueId?
    leaseId?
    reservationId?
    recoveryOperationId?
    shutdownOperationId?

    correlationId?
    causationId?
    applicationInstanceId

    occurredAt

    cause?
    metadata
}
```

---

## 7. Error Categories

```text
TASK
SCHEDULE
TRIGGER
OCCURRENCE
JOB
ATTEMPT
INPUT
DUPLICATE
IDEMPOTENCY
QUEUE
ADMISSION
DISPATCH
WORKER
RETRY
TIMEOUT
CANCELLATION
CONCURRENCY
RESOURCE
DEPENDENCY
OVERLAP
MISFIRE
PERSISTENCE
RECOVERY
SHUTDOWN
AUTHORIZATION
SECURITY
CONFIGURATION
LIFECYCLE
CONCURRENCY_CONTROL
INTERNAL
```

---

## 8. Error Scopes

```text
SCHEDULER_INSTANCE
TASK_DEFINITION
SCHEDULE_DEFINITION
TRIGGER
SCHEDULE_OCCURRENCE
JOB_INSTANCE
JOB_ATTEMPT
QUEUE
WORKER
CONCURRENCY_LEASE
RESOURCE_RESERVATION
RETRY_DECISION
CANCELLATION_OPERATION
DRAIN_OPERATION
SHUTDOWN_OPERATION
PERSISTENCE_ADAPTER
RECOVERY_OPERATION
```

---

## 9. Severity

```text
NOTICE
WARNING
ERROR
CRITICAL
FATAL
```

### NOTICE

Expected caller correction or policy outcome.

### WARNING

Degraded but safe operation.

### ERROR

One operation failed.

### CRITICAL

Important scheduling authority, durability, security, or shutdown behavior failed.

### FATAL

Scheduler cannot preserve queue, state, execution, or safety invariants.

---

## 10. Retry Class

```text
NEVER
IMMEDIATE
TRANSIENT
AFTER_CAPACITY_RECOVERY
AFTER_WORKER_RECOVERY
AFTER_RESOURCE_RECOVERY
AFTER_CONFIGURATION_CHANGE
AFTER_RESTART
AFTER_RECONCILIATION
IDEMPOTENT_ONLY
UNKNOWN
```

---

## 11. Recoverability

```text
AUTOMATIC
CALLER_CORRECTION
CONFIGURATION_CHANGE
WORKER_RECOVERY
RESOURCE_RECOVERY
APPLICATION_RESTART
ADMIN_ACTION
RECONCILIATION_REQUIRED
NOT_RECOVERABLE
UNKNOWN
```

---

## 12. Recovery Actions

```text
RETRY_REGISTRATION
UPDATE_TASK_DEFINITION
UPDATE_SCHEDULE
UPDATE_TRIGGER
REDUCE_JOB_INPUT
USE_SAFE_REFERENCE
WAIT_AND_RETRY
REDUCE_JOB_RATE
REDUCE_CONCURRENCY
FREE_QUEUE_CAPACITY
RESTORE_WORKER
RESTORE_RESOURCE
UPDATE_RETRY_POLICY
UPDATE_TIMEOUT_POLICY
UPDATE_OVERLAP_POLICY
UPDATE_MISFIRE_POLICY
RECONCILE_ATTEMPT
RECONCILE_PERSISTENCE
RESTORE_PERSISTENCE
RESTART_SCHEDULER
RESTART_APPLICATION
CANCEL_JOB
DISABLE_TASK
DISABLE_SCHEDULE
CONTACT_SUPPORT
NONE
```

---

## 13. Error Code Naming

Canonical format:

```text
SCHEDULER_<CONCERN>_<CONDITION>
```

Warnings use:

```text
SCHEDULER_WARNING_<CONDITION>
```

Security errors may use:

```text
SCHEDULER_SECURITY_<CONDITION>
```

---

# Part I — Task Definition Errors

## 14. SCHEDULER_TASK_INVALID

```text
category: TASK
scope: TASK_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 15. SCHEDULER_TASK_ID_INVALID

```text
category: TASK
scope: TASK_DEFINITION
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 16. SCHEDULER_TASK_DUPLICATE

```text
category: TASK
scope: TASK_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 17. SCHEDULER_TASK_OWNER_MISSING

```text
category: TASK
scope: TASK_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 18. SCHEDULER_TASK_HANDLER_NOT_REGISTERED

```text
category: TASK
scope: TASK_DEFINITION
severity: ERROR
retryClass: AFTER_WORKER_RECOVERY
recoverability: WORKER_RECOVERY
```

---

## 19. SCHEDULER_TASK_VERSION_CONFLICT

```text
category: CONCURRENCY_CONTROL
scope: TASK_DEFINITION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC
```

---

## 20. SCHEDULER_TASK_TIMEOUT_UNBOUNDED

```text
category: TASK
scope: TASK_DEFINITION
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 21. SCHEDULER_TASK_RETRY_UNBOUNDED

```text
category: TASK
scope: TASK_DEFINITION
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 22. SCHEDULER_TASK_CONCURRENCY_UNBOUNDED

```text
category: TASK
scope: TASK_DEFINITION
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 23. SCHEDULER_TASK_OVERLAP_POLICY_MISSING

```text
category: OVERLAP
scope: TASK_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 24. SCHEDULER_TASK_MISFIRE_POLICY_MISSING

```text
category: MISFIRE
scope: TASK_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 25. SCHEDULER_TASK_PERSISTENCE_MODE_UNSUPPORTED

```text
category: PERSISTENCE
scope: TASK_DEFINITION
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 26. SCHEDULER_TASK_INVALIDATED

```text
category: TASK
scope: TASK_DEFINITION
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

# Part II — Schedule Errors

## 27. SCHEDULER_SCHEDULE_INVALID

```text
category: SCHEDULE
scope: SCHEDULE_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 28. SCHEDULER_SCHEDULE_ID_INVALID

```text
category: SCHEDULE
scope: SCHEDULE_DEFINITION
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 29. SCHEDULER_SCHEDULE_DUPLICATE

```text
category: SCHEDULE
scope: SCHEDULE_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 30. SCHEDULER_SCHEDULE_TASK_NOT_FOUND

```text
category: SCHEDULE
scope: SCHEDULE_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 31. SCHEDULER_SCHEDULE_START_END_INVALID

```text
category: SCHEDULE
scope: SCHEDULE_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 32. SCHEDULER_SCHEDULE_TIMEZONE_REQUIRED

```text
category: SCHEDULE
scope: SCHEDULE_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 33. SCHEDULER_SCHEDULE_TIMEZONE_INVALID

```text
category: SCHEDULE
scope: SCHEDULE_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 34. SCHEDULER_SCHEDULE_INTERVAL_TOO_SHORT

```text
category: SCHEDULE
scope: SCHEDULE_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 35. SCHEDULER_SCHEDULE_INPUT_FACTORY_UNSAFE

```text
category: SECURITY
scope: SCHEDULE_DEFINITION
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 36. SCHEDULER_SCHEDULE_INVALIDATED

```text
category: SCHEDULE
scope: SCHEDULE_DEFINITION
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

# Part III — Trigger Errors

## 37. SCHEDULER_TRIGGER_INVALID

```text
category: TRIGGER
scope: TRIGGER
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 38. SCHEDULER_TRIGGER_TYPE_UNSUPPORTED

```text
category: TRIGGER
scope: TRIGGER
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 39. SCHEDULER_CRON_EXPRESSION_INVALID

```text
category: TRIGGER
scope: TRIGGER
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 40. SCHEDULER_CRON_TIMEZONE_AMBIGUOUS

```text
category: TRIGGER
scope: TRIGGER
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 41. SCHEDULER_DELAY_INVALID

```text
category: TRIGGER
scope: TRIGGER
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 42. SCHEDULER_TRIGGER_ALREADY_ARMED

```text
category: TRIGGER
scope: TRIGGER
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 43. SCHEDULER_TRIGGER_NOT_ARMED

```text
category: TRIGGER
scope: TRIGGER
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 44. SCHEDULER_TRIGGER_EVALUATION_FAILED

```text
category: TRIGGER
scope: TRIGGER
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 45. SCHEDULER_EVENT_TRIGGER_MAPPING_INVALID

```text
category: TRIGGER
scope: TRIGGER
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 46. SCHEDULER_EVENT_TRIGGER_MAPPING_UNSAFE

```text
category: SECURITY
scope: TRIGGER
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 47. SCHEDULER_EVENT_TRIGGER_ADAPTER_UNAVAILABLE

```text
category: TRIGGER
scope: SCHEDULER_INSTANCE
severity: WARNING
retryClass: AFTER_WORKER_RECOVERY
recoverability: AUTOMATIC
```

Time-based scheduling may continue.

---

# Part IV — Occurrence, Overlap, and Misfire Errors

## 48. SCHEDULER_OCCURRENCE_DUPLICATE

```text
category: OCCURRENCE
scope: SCHEDULE_OCCURRENCE
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

---

## 49. SCHEDULER_OCCURRENCE_CREATION_FAILED

```text
category: OCCURRENCE
scope: SCHEDULE_OCCURRENCE
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 50. SCHEDULER_OVERLAP_POLICY_INVALID

```text
category: OVERLAP
scope: SCHEDULE_OCCURRENCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 51. SCHEDULER_OVERLAP_EVALUATION_FAILED

```text
category: OVERLAP
scope: SCHEDULE_OCCURRENCE
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 52. SCHEDULER_OVERLAP_CANCEL_EXISTING_FAILED

```text
category: OVERLAP
scope: SCHEDULE_OCCURRENCE
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 53. SCHEDULER_OVERLAP_REPLACE_PENDING_FAILED

```text
category: OVERLAP
scope: SCHEDULE_OCCURRENCE
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC
```

---

## 54. SCHEDULER_MISFIRE_POLICY_INVALID

```text
category: MISFIRE
scope: SCHEDULE_OCCURRENCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 55. SCHEDULER_MISFIRE_CATCHUP_LIMIT_EXCEEDED

```text
category: MISFIRE
scope: SCHEDULE_OCCURRENCE
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

---

## 56. SCHEDULER_MISFIRE_EVALUATION_FAILED

```text
category: MISFIRE
scope: SCHEDULE_OCCURRENCE
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

# Part V — Job Input and Creation Errors

## 57. SCHEDULER_JOB_INVALID

```text
category: JOB
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 58. SCHEDULER_JOB_INPUT_INVALID

```text
category: INPUT
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 59. SCHEDULER_JOB_INPUT_TOO_LARGE

```text
category: INPUT
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

Recovery:

```text
Use an ArtifactId, DocumentId, PageId, or another safe reference.
```

---

## 60. SCHEDULER_JOB_INPUT_UNSUPPORTED_TYPE

```text
category: INPUT
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 61. SCHEDULER_JOB_INPUT_NOT_SERIALIZABLE

```text
category: INPUT
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

Relevant when durable mode is enabled.

---

## 62. SCHEDULER_JOB_INPUT_SECRET_BLOCKED

```text
category: SECURITY
scope: JOB_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

The detected secret must not be included in the error.

---

## 63. SCHEDULER_JOB_INPUT_USER_CONTENT_BLOCKED

```text
category: SECURITY
scope: JOB_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

Use a safe content reference.

---

## 64. SCHEDULER_JOB_TASK_VERSION_UNSUPPORTED

```text
category: JOB
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 65. SCHEDULER_JOB_DEADLINE_INVALID

```text
category: JOB
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 66. SCHEDULER_JOB_PRIORITY_INVALID

```text
category: JOB
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 67. SCHEDULER_JOB_CREATION_FAILED

```text
category: JOB
scope: JOB_INSTANCE
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

# Part VI — Duplicate and Idempotency Errors

## 68. SCHEDULER_JOB_DUPLICATE_REJECTED

```text
category: DUPLICATE
scope: JOB_INSTANCE
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 69. SCHEDULER_DUPLICATE_POLICY_INVALID

```text
category: DUPLICATE
scope: TASK_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 70. SCHEDULER_IDEMPOTENCY_KEY_REQUIRED

```text
category: IDEMPOTENCY
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 71. SCHEDULER_IDEMPOTENCY_KEY_INVALID

```text
category: IDEMPOTENCY
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 72. SCHEDULER_IDEMPOTENCY_CONFLICT

```text
category: IDEMPOTENCY
scope: JOB_INSTANCE
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 73. SCHEDULER_AT_MOST_ONCE_RETRY_BLOCKED

```text
category: IDEMPOTENCY
scope: RETRY_DECISION
severity: ERROR
retryClass: NEVER
recoverability: NONE
```

---

# Part VII — Queue and Admission Errors

## 74. SCHEDULER_QUEUE_NOT_INITIALIZED

```text
category: QUEUE
scope: QUEUE
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 75. SCHEDULER_QUEUE_NOT_RUNNING

```text
category: QUEUE
scope: QUEUE
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 76. SCHEDULER_QUEUE_BACKPRESSURED

```text
category: QUEUE
scope: QUEUE
severity: WARNING
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 77. SCHEDULER_QUEUE_CAPACITY_EXCEEDED

```text
category: ADMISSION
scope: QUEUE
severity: ERROR
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 78. SCHEDULER_JOB_ADMISSION_TIMED_OUT

```text
category: ADMISSION
scope: JOB_INSTANCE
severity: ERROR
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 79. SCHEDULER_LOW_PRIORITY_JOB_DROPPED

```text
category: ADMISSION
scope: JOB_INSTANCE
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 80. SCHEDULER_QUEUE_CRITICAL_RESERVE_EXHAUSTED

```text
category: QUEUE
scope: SCHEDULER_INSTANCE
severity: CRITICAL
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC or APPLICATION_RESTART
```

---

## 81. SCHEDULER_QUEUE_CLAIM_CONFLICT

```text
category: CONCURRENCY_CONTROL
scope: QUEUE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 82. SCHEDULER_QUEUE_INVARIANT_BROKEN

```text
category: INTERNAL
scope: QUEUE
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 83. SCHEDULER_QUEUE_ITEM_EXPIRED

```text
category: QUEUE
scope: JOB_INSTANCE
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

# Part VIII — Dispatch Errors

## 84. SCHEDULER_DISPATCHER_NOT_RUNNING

```text
category: DISPATCH
scope: SCHEDULER_INSTANCE
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 85. SCHEDULER_DISPATCH_FAILED

```text
category: DISPATCH
scope: JOB_INSTANCE
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 86. SCHEDULER_NO_ELIGIBLE_WORKER

```text
category: DISPATCH
scope: JOB_INSTANCE
severity: ERROR
retryClass: AFTER_WORKER_RECOVERY
recoverability: WORKER_RECOVERY
```

---

## 87. SCHEDULER_WORKER_SELECTION_CONFLICT

```text
category: DISPATCH
scope: JOB_ATTEMPT
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 88. SCHEDULER_DISPATCH_AUTHORITY_UNCERTAIN

```text
category: DISPATCH
scope: JOB_ATTEMPT
severity: CRITICAL
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 89. SCHEDULER_RUNTIME_CAPACITY_UNAVAILABLE

```text
category: DISPATCH
scope: JOB_INSTANCE
severity: WARNING
retryClass: AFTER_RESOURCE_RECOVERY
recoverability: AUTOMATIC
```

---

# Part IX — Worker Errors

## 90. SCHEDULER_WORKER_ID_DUPLICATE

```text
category: WORKER
scope: WORKER
severity: ERROR
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 91. SCHEDULER_WORKER_CONFIGURATION_INVALID

```text
category: WORKER
scope: WORKER
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 92. SCHEDULER_WORKER_INITIALIZATION_FAILED

```text
category: WORKER
scope: WORKER
severity: ERROR
retryClass: TRANSIENT
recoverability: WORKER_RECOVERY
```

---

## 93. SCHEDULER_WORKER_UNAVAILABLE

```text
category: WORKER
scope: WORKER
severity: ERROR
retryClass: AFTER_WORKER_RECOVERY
recoverability: WORKER_RECOVERY
```

---

## 94. SCHEDULER_WORKER_START_FAILED

```text
category: WORKER
scope: JOB_ATTEMPT
severity: ERROR
retryClass: TRANSIENT
recoverability: WORKER_RECOVERY
```

---

## 95. SCHEDULER_WORKER_EXECUTION_FAILED

```text
category: WORKER
scope: JOB_ATTEMPT
severity: ERROR
retryClass: controlled by JobFailure
recoverability: WORKER_RECOVERY or CALLER_CORRECTION
```

---

## 96. SCHEDULER_WORKER_IGNORED_CANCELLATION

```text
category: CANCELLATION
scope: WORKER
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 97. SCHEDULER_WORKER_DUPLICATE_COMPLETION

```text
category: CONCURRENCY_CONTROL
scope: JOB_ATTEMPT
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 98. SCHEDULER_WORKER_LATE_COMPLETION_IGNORED

```text
category: ATTEMPT
scope: JOB_ATTEMPT
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

---

## 99. SCHEDULER_WORKER_RESOURCE_LEAK_DETECTED

```text
category: RESOURCE
scope: WORKER
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 100. SCHEDULER_WORKER_SHUTDOWN_FAILED

```text
category: WORKER
scope: WORKER
severity: ERROR
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

# Part X — Attempt Errors

## 101. SCHEDULER_ATTEMPT_INVALID

```text
category: ATTEMPT
scope: JOB_ATTEMPT
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 102. SCHEDULER_ATTEMPT_ALREADY_RUNNING

```text
category: ATTEMPT
scope: JOB_ATTEMPT
severity: ERROR
retryClass: NEVER
recoverability: NONE
```

---

## 103. SCHEDULER_ATTEMPT_ALREADY_TERMINAL

```text
category: ATTEMPT
scope: JOB_ATTEMPT
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 104. SCHEDULER_ATTEMPT_START_FAILED

```text
category: ATTEMPT
scope: JOB_ATTEMPT
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 105. SCHEDULER_ATTEMPT_TERMINAL_STATE_CONFLICT

```text
category: CONCURRENCY_CONTROL
scope: JOB_ATTEMPT
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 106. SCHEDULER_ATTEMPT_INTERRUPTED

```text
category: ATTEMPT
scope: JOB_ATTEMPT
severity: WARNING or ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 107. SCHEDULER_ATTEMPT_OUTCOME_UNCERTAIN

```text
category: ATTEMPT
scope: JOB_ATTEMPT
severity: CRITICAL
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

# Part XI — Timeout and Abandonment Errors

## 108. SCHEDULER_TIMEOUT_POLICY_INVALID

```text
category: TIMEOUT
scope: TASK_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 109. SCHEDULER_ATTEMPT_TIMED_OUT

```text
category: TIMEOUT
scope: JOB_ATTEMPT
severity: ERROR
retryClass: controlled by retry policy
recoverability: AUTOMATIC or RECONCILIATION_REQUIRED
```

---

## 110. SCHEDULER_TIMEOUT_CANCELLATION_FAILED

```text
category: TIMEOUT
scope: JOB_ATTEMPT
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 111. SCHEDULER_ATTEMPT_ABANDONED

```text
category: TIMEOUT
scope: JOB_ATTEMPT
severity: CRITICAL
retryClass: NEVER for same attempt
recoverability: WORKER_RECOVERY
```

---

## 112. SCHEDULER_JOB_START_DEADLINE_EXPIRED

```text
category: TIMEOUT
scope: JOB_INSTANCE
severity: NOTICE or ERROR
retryClass: NEVER
recoverability: NONE
```

---

## 113. SCHEDULER_JOB_COMPLETION_DEADLINE_EXPIRED

```text
category: TIMEOUT
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER or policy-controlled
recoverability: NONE
```

---

# Part XII — Cancellation Errors

## 114. SCHEDULER_CANCELLATION_REQUEST_INVALID

```text
category: CANCELLATION
scope: CANCELLATION_OPERATION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 115. SCHEDULER_CANCELLATION_TARGET_NOT_FOUND

```text
category: CANCELLATION
scope: CANCELLATION_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 116. SCHEDULER_CANCELLATION_NOT_AUTHORIZED

```text
category: AUTHORIZATION
scope: CANCELLATION_OPERATION
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 117. SCHEDULER_CROSS_MODULE_CANCELLATION_BLOCKED

```text
category: AUTHORIZATION
scope: CANCELLATION_OPERATION
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 118. SCHEDULER_CANCELLATION_PROPAGATION_FAILED

```text
category: CANCELLATION
scope: CANCELLATION_OPERATION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 119. SCHEDULER_CANCELLATION_TIMED_OUT

```text
category: CANCELLATION
scope: CANCELLATION_OPERATION
severity: ERROR
retryClass: NEVER
recoverability: NONE
```

Remaining attempts may become abandoned.

---

# Part XIII — Retry Errors

## 120. SCHEDULER_RETRY_POLICY_INVALID

```text
category: RETRY
scope: TASK_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 121. SCHEDULER_RETRY_NOT_ALLOWED

```text
category: RETRY
scope: RETRY_DECISION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 122. SCHEDULER_RETRY_EXHAUSTED

```text
category: RETRY
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: NONE
```

---

## 123. SCHEDULER_RETRY_BUDGET_EXHAUSTED

```text
category: RETRY
scope: JOB_INSTANCE
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 124. SCHEDULER_RETRY_DELAY_INVALID

```text
category: RETRY
scope: RETRY_DECISION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 125. SCHEDULER_RETRY_SCHEDULING_FAILED

```text
category: RETRY
scope: JOB_INSTANCE
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 126. SCHEDULER_RETRY_REQUIRES_RECONCILIATION

```text
category: RETRY
scope: JOB_INSTANCE
severity: CRITICAL
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

Blind retry is blocked.

---

# Part XIV — Concurrency Lease Errors

## 127. SCHEDULER_CONCURRENCY_POLICY_INVALID

```text
category: CONCURRENCY
scope: TASK_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 128. SCHEDULER_CONCURRENCY_LEASE_UNAVAILABLE

```text
category: CONCURRENCY
scope: CONCURRENCY_LEASE
severity: WARNING
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 129. SCHEDULER_CONCURRENCY_ACQUISITION_TIMED_OUT

```text
category: CONCURRENCY
scope: CONCURRENCY_LEASE
severity: ERROR
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 130. SCHEDULER_CONCURRENCY_LEASE_EXPIRED

```text
category: CONCURRENCY
scope: CONCURRENCY_LEASE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 131. SCHEDULER_CONCURRENCY_LEASE_RELEASE_FAILED

```text
category: CONCURRENCY
scope: CONCURRENCY_LEASE
severity: CRITICAL
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 132. SCHEDULER_CONCURRENCY_LIMIT_BROKEN

```text
category: INTERNAL
scope: SCHEDULER_INSTANCE
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

# Part XV — Resource Errors

## 133. SCHEDULER_RESOURCE_POLICY_INVALID

```text
category: RESOURCE
scope: TASK_DEFINITION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 134. SCHEDULER_RESOURCE_CLASS_UNKNOWN

```text
category: RESOURCE
scope: RESOURCE_RESERVATION
severity: ERROR
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 135. SCHEDULER_RESOURCE_UNAVAILABLE

```text
category: RESOURCE
scope: RESOURCE_RESERVATION
severity: WARNING
retryClass: AFTER_RESOURCE_RECOVERY
recoverability: RESOURCE_RECOVERY
```

---

## 136. SCHEDULER_RESOURCE_ACQUISITION_TIMED_OUT

```text
category: RESOURCE
scope: RESOURCE_RESERVATION
severity: ERROR
retryClass: AFTER_RESOURCE_RECOVERY
recoverability: RESOURCE_RECOVERY
```

---

## 137. SCHEDULER_RESOURCE_RESERVATION_FAILED

```text
category: RESOURCE
scope: RESOURCE_RESERVATION
severity: ERROR
retryClass: TRANSIENT
recoverability: RESOURCE_RECOVERY
```

---

## 138. SCHEDULER_RESOURCE_RESERVATION_EXPIRED

```text
category: RESOURCE
scope: RESOURCE_RESERVATION
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 139. SCHEDULER_RESOURCE_RESERVATION_REVOKED

```text
category: RESOURCE
scope: RESOURCE_RESERVATION
severity: ERROR
retryClass: AFTER_RESOURCE_RECOVERY
recoverability: RESOURCE_RECOVERY
```

---

## 140. SCHEDULER_RESOURCE_RELEASE_FAILED

```text
category: RESOURCE
scope: RESOURCE_RESERVATION
severity: CRITICAL
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 141. SCHEDULER_RESOURCE_DEADLOCK_DETECTED

```text
category: RESOURCE
scope: SCHEDULER_INSTANCE
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

# Part XVI — Dependency Errors

## 142. SCHEDULER_DEPENDENCY_INVALID

```text
category: DEPENDENCY
scope: JOB_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 143. SCHEDULER_DEPENDENCY_NOT_FOUND

```text
category: DEPENDENCY
scope: JOB_INSTANCE
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 144. SCHEDULER_DEPENDENCY_FAILED

```text
category: DEPENDENCY
scope: JOB_INSTANCE
severity: ERROR
retryClass: controlled by policy
recoverability: AUTOMATIC or NONE
```

---

## 145. SCHEDULER_DEPENDENCY_TIMED_OUT

```text
category: DEPENDENCY
scope: JOB_INSTANCE
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 146. SCHEDULER_DEPENDENCY_CYCLE_DETECTED

```text
category: DEPENDENCY
scope: SCHEDULER_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

# Part XVII — Persistence Errors

## 147. SCHEDULER_PERSISTENCE_NOT_CONFIGURED

```text
category: PERSISTENCE
scope: PERSISTENCE_ADAPTER
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 148. SCHEDULER_PERSISTENCE_ADAPTER_UNAVAILABLE

```text
category: PERSISTENCE
scope: PERSISTENCE_ADAPTER
severity: ERROR
retryClass: AFTER_RESOURCE_RECOVERY
recoverability: AUTOMATIC
```

---

## 149. SCHEDULER_PERSISTENCE_CONFIGURATION_INVALID

```text
category: PERSISTENCE
scope: PERSISTENCE_ADAPTER
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 150. SCHEDULER_PERSISTENCE_WRITE_FAILED

```text
category: PERSISTENCE
scope: PERSISTENCE_ADAPTER
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC
```

---

## 151. SCHEDULER_PERSISTENCE_READ_FAILED

```text
category: PERSISTENCE
scope: PERSISTENCE_ADAPTER
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 152. SCHEDULER_PERSISTENCE_VERSION_CONFLICT

```text
category: CONCURRENCY_CONTROL
scope: PERSISTENCE_ADAPTER
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC
```

---

## 153. SCHEDULER_PERSISTENCE_STATE_CORRUPTED

```text
category: PERSISTENCE
scope: PERSISTENCE_ADAPTER
severity: FATAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 154. SCHEDULER_PERSISTENCE_AUTHORITY_UNCERTAIN

```text
category: PERSISTENCE
scope: PERSISTENCE_ADAPTER
severity: CRITICAL
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 155. SCHEDULER_DURABLE_JOB_INPUT_UNSAFE

```text
category: SECURITY
scope: JOB_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

# Part XVIII — Recovery Errors

## 156. SCHEDULER_RECOVERY_NOT_SUPPORTED

```text
category: RECOVERY
scope: RECOVERY_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

Expected for in-memory MVP.

---

## 157. SCHEDULER_RECOVERY_LOAD_FAILED

```text
category: RECOVERY
scope: RECOVERY_OPERATION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 158. SCHEDULER_RECOVERY_ENTITY_INVALID

```text
category: RECOVERY
scope: RECOVERY_OPERATION
severity: ERROR
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 159. SCHEDULER_RECOVERY_ATTEMPT_UNCERTAIN

```text
category: RECOVERY
scope: JOB_ATTEMPT
severity: CRITICAL
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 160. SCHEDULER_RECOVERY_RECONCILIATION_FAILED

```text
category: RECOVERY
scope: RECOVERY_OPERATION
severity: CRITICAL
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 161. SCHEDULER_RECOVERY_MISFIRE_APPLICATION_FAILED

```text
category: RECOVERY
scope: RECOVERY_OPERATION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 162. SCHEDULER_RECOVERY_TIMED_OUT

```text
category: RECOVERY
scope: RECOVERY_OPERATION
severity: CRITICAL
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 163. SCHEDULER_RECOVERY_PARTIALLY_COMPLETED

```text
category: RECOVERY
scope: RECOVERY_OPERATION
severity: WARNING
retryClass: conditional
recoverability: AUTOMATIC or ADMIN_ACTION
```

---

# Part XIX — Authorization and Security Errors

## 164. SCHEDULER_OPERATION_NOT_AUTHORIZED

```text
category: AUTHORIZATION
scope: SCHEDULER_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 165. SCHEDULER_TASK_REGISTRATION_NOT_AUTHORIZED

```text
category: AUTHORIZATION
scope: TASK_DEFINITION
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 166. SCHEDULER_MANUAL_RUN_NOT_AUTHORIZED

```text
category: AUTHORIZATION
scope: JOB_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 167. SCHEDULER_JOB_CANCEL_NOT_AUTHORIZED

```text
category: AUTHORIZATION
scope: CANCELLATION_OPERATION
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 168. SCHEDULER_DIAGNOSTICS_ACCESS_DENIED

```text
category: AUTHORIZATION
scope: SCHEDULER_INSTANCE
severity: ERROR
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 169. SCHEDULER_UNSAFE_METADATA_BLOCKED

```text
category: SECURITY
scope: SCHEDULER_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 170. SCHEDULER_RAW_JOB_PAYLOAD_EXPOSURE_BLOCKED

```text
category: SECURITY
scope: SCHEDULER_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

# Part XX — Configuration Errors

## 171. SCHEDULER_CONFIGURATION_INVALID

```text
category: CONFIGURATION
scope: SCHEDULER_INSTANCE
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 172. SCHEDULER_CONFIGURATION_CONFLICT

```text
category: CONFIGURATION
scope: SCHEDULER_INSTANCE
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC
```

---

## 173. SCHEDULER_CONFIGURATION_RESTART_REQUIRED

```text
category: CONFIGURATION
scope: SCHEDULER_INSTANCE
severity: NOTICE
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 174. SCHEDULER_QUEUE_CAPACITY_CONFIGURATION_INVALID

```text
category: CONFIGURATION
scope: QUEUE
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 175. SCHEDULER_GLOBAL_CONCURRENCY_CONFIGURATION_INVALID

```text
category: CONFIGURATION
scope: SCHEDULER_INSTANCE
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

# Part XXI — Drain and Shutdown Errors

## 176. SCHEDULER_DRAIN_REQUEST_INVALID

```text
category: SHUTDOWN
scope: DRAIN_OPERATION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 177. SCHEDULER_DRAIN_ALREADY_RUNNING

```text
category: SHUTDOWN
scope: DRAIN_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 178. SCHEDULER_DRAIN_TIMED_OUT

```text
category: SHUTDOWN
scope: DRAIN_OPERATION
severity: WARNING or ERROR
retryClass: NEVER during shutdown
recoverability: NONE
```

---

## 179. SCHEDULER_DRAIN_PARTIALLY_COMPLETED

```text
category: SHUTDOWN
scope: DRAIN_OPERATION
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

---

## 180. SCHEDULER_SHUTDOWN_REQUEST_INVALID

```text
category: SHUTDOWN
scope: SHUTDOWN_OPERATION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 181. SCHEDULER_SHUTDOWN_ALREADY_RUNNING

```text
category: SHUTDOWN
scope: SHUTDOWN_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 182. SCHEDULER_SHUTDOWN_TRIGGER_STOP_FAILED

```text
category: SHUTDOWN
scope: SHUTDOWN_OPERATION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 183. SCHEDULER_SHUTDOWN_CANCELLATION_FAILED

```text
category: SHUTDOWN
scope: SHUTDOWN_OPERATION
severity: ERROR
retryClass: NEVER
recoverability: NONE
```

---

## 184. SCHEDULER_SHUTDOWN_PERSISTENCE_FAILED

```text
category: SHUTDOWN
scope: SHUTDOWN_OPERATION
severity: CRITICAL
retryClass: NEVER
recoverability: RECONCILIATION_REQUIRED
```

---

## 185. SCHEDULER_SHUTDOWN_WORKER_STOP_FAILED

```text
category: SHUTDOWN
scope: SHUTDOWN_OPERATION
severity: ERROR
retryClass: NEVER
recoverability: NONE
```

---

## 186. SCHEDULER_SHUTDOWN_TIMED_OUT

```text
category: SHUTDOWN
scope: SHUTDOWN_OPERATION
severity: CRITICAL
retryClass: NEVER
recoverability: NONE
```

---

## 187. SCHEDULER_SHUTDOWN_PARTIALLY_TERMINATED

```text
category: SHUTDOWN
scope: SHUTDOWN_OPERATION
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

---

# Part XXII — Lifecycle and Internal Errors

## 188. SCHEDULER_NOT_INITIALIZED

```text
category: LIFECYCLE
scope: SCHEDULER_INSTANCE
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 189. SCHEDULER_NOT_RUNNING

```text
category: LIFECYCLE
scope: SCHEDULER_INSTANCE
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 190. SCHEDULER_QUIESCING

```text
category: LIFECYCLE
scope: SCHEDULER_INSTANCE
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 191. SCHEDULER_SHUTDOWN_IN_PROGRESS

```text
category: LIFECYCLE
scope: SCHEDULER_INSTANCE
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 192. SCHEDULER_INVALID_STATE_TRANSITION

```text
category: INTERNAL
scope: SCHEDULER_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 193. SCHEDULER_STATE_VERSION_CONFLICT

```text
category: CONCURRENCY_CONTROL
scope: SCHEDULER_INSTANCE
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC
```

---

## 194. SCHEDULER_MULTIPLE_ACTIVE_ATTEMPTS

```text
category: INTERNAL
scope: JOB_INSTANCE
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 195. SCHEDULER_ACTIVE_ATTEMPT_WITHOUT_LEASE

```text
category: INTERNAL
scope: JOB_ATTEMPT
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 196. SCHEDULER_ACTIVE_ATTEMPT_WITHOUT_RESOURCE_RESERVATION

```text
category: INTERNAL
scope: JOB_ATTEMPT
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 197. SCHEDULER_FATAL_INVARIANT_BROKEN

```text
category: INTERNAL
scope: SCHEDULER_INSTANCE
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

Use only when Scheduler cannot preserve execution, state, queue, authorization, or boundedness guarantees.

---

# Part XXIII — Warnings

## 198. Warning Model

```text
SchedulerWarning {
    warningId
    code
    scope
    safeMessage
    recoveryActions[]
    metadata
}
```

Warnings never contain job payloads or raw worker output.

---

## 199. SCHEDULER_WARNING_QUEUE_PRESSURE

Queue utilization is high but safe admission remains possible.

---

## 200. SCHEDULER_WARNING_LOW_PRIORITY_JOB_DROPPED

A low-value job was dropped according to policy.

---

## 201. SCHEDULER_WARNING_WORKER_SLOW

A worker remains available but exceeds latency thresholds.

---

## 202. SCHEDULER_WARNING_WORKER_DEGRADED

A worker remains usable with reduced capacity.

---

## 203. SCHEDULER_WARNING_OPTIONAL_WORKER_UNAVAILABLE

An optional worker is unavailable.

---

## 204. SCHEDULER_WARNING_RESOURCE_WAIT

A job remains safely blocked waiting for resources.

---

## 205. SCHEDULER_WARNING_CONCURRENCY_WAIT

A job remains safely blocked waiting for concurrency.

---

## 206. SCHEDULER_WARNING_MISFIRE_SKIPPED

One or more occurrences were skipped by policy.

---

## 207. SCHEDULER_WARNING_RETRY_DELAYED

A retry was delayed due to pressure, shutdown, or resource constraints.

---

## 208. SCHEDULER_WARNING_LATE_COMPLETION

A physical completion arrived after terminal Scheduler authority.

---

## 209. SCHEDULER_WARNING_RECOVERY_PARTIAL

Some durable state could not be restored.

---

## 210. SCHEDULER_WARNING_DRAIN_PARTIAL

Some jobs remained after bounded drain.

---

## 211. SCHEDULER_WARNING_SHUTDOWN_PARTIAL

Scheduler terminated with abandoned workers or jobs.

---

# Part XXIV — Retry and Recovery Rules

## 212. Errors Do Not Retry Themselves

```text
Error normalized
    ↓
Current job, attempt, idempotency, and deadline checked
    ↓
RetryPolicy evaluated
    ↓
New retry decision
    ↓
New attempt created if allowed
```

---

## 213. Safe Retry

Potentially safe when:

- previous attempt definitely failed;
- failure class is retryable;
- idempotency requirement is satisfied;
- retry budget remains;
- completion deadline permits it;
- shutdown policy permits it.

---

## 214. Unsafe Blind Retry

Do not blindly retry when:

- attempt outcome is uncertain;
- timeout occurred while external side effect may have completed;
- persistence authority is uncertain;
- at-most-once task lacks reconciliation;
- worker returned duplicate completion.

These require reconciliation.

---

## 215. Capacity Recovery

Recommended order:

```text
delay low-priority jobs
    ↓
coalesce replaceable jobs
    ↓
skip low-value recurring occurrences
    ↓
reduce dispatch concurrency
    ↓
reject low-priority admission
    ↓
use critical reserve for authorized critical jobs
```

---

## 216. Worker Recovery

Worker recovery may:

- reinitialize dependencies;
- reduce capacity;
- run health probes;
- disable affected task support;
- re-enable after successful probe.

Running uncertain attempts require reconciliation.

---

## 217. Resource Recovery

Jobs in resource wait are reevaluated when safe resource-availability facts change.

Polling must remain bounded.

---

## 218. Recovery After Crash

For in-memory MVP:

```text
No recovery guarantee
```

For durable modes:

```text
load state
    ↓
classify interrupted and uncertain attempts
    ↓
apply reconciliation
    ↓
apply misfire policy
    ↓
restore eligible jobs
```

---

# Part XXV — State Implications

## 219. Task Validation Failure

```text
Task → REJECTED or INVALID
No normal job creation
```

---

## 220. Queue Capacity Failure

```text
Job admission → REJECTED / TIMED_OUT
Queue → BACKPRESSURED
Scheduler remains RUNNING or DEGRADED
```

---

## 221. Worker Failure

```text
Worker → FAILED / UNHEALTHY
Affected attempt → FAILED / INTERRUPTED / UNCERTAIN
Scheduler → RUNNING / DEGRADED
```

One worker failure does not fail unrelated workers.

---

## 222. Timeout

```text
Attempt → TIMED_OUT
Cancellation requested
Late completion non-authoritative
Job → RETRY_WAIT or TIMED_OUT
```

---

## 223. Abandonment

```text
Attempt → ABANDONED
Leases and reservations released logically
Worker health degraded
Job → ABANDONED or retry/reconciliation according to policy
```

---

## 224. Persistence Uncertainty

```text
Entity → UNCERTAIN
Recovery → RECONCILING
Blind retry blocked
```

---

## 225. Shutdown Timeout

```text
Drain → TIMED_OUT / PARTIAL
Remaining attempts → CANCELED / ABANDONED
Scheduler continues bounded stop
```

---

# Part XXVI — Cross-Module Normalization

## 226. Producer Mapping

Task owners may receive:

```text
TASK_INVALID
SCHEDULE_INVALID
JOB_INPUT_INVALID
JOB_INPUT_TOO_LARGE
QUEUE_CAPACITY_EXCEEDED
JOB_ADMISSION_TIMED_OUT
NO_ELIGIBLE_WORKER
RESOURCE_UNAVAILABLE
CANCELLATION_NOT_AUTHORIZED
SCHEDULER_NOT_RUNNING
SCHEDULER_QUIESCING
```

They must not receive raw queue, worker, persistence, or system exceptions.

---

## 227. Runtime Mapping

Runtime may interpret:

```text
SCHEDULER_RUNTIME_CAPACITY_UNAVAILABLE
    → defer dispatch

SCHEDULER_ATTEMPT_TIMED_OUT
    → stop logical authority

SCHEDULER_JOB_CANCELED
    → cancel associated Runtime work when explicitly linked
```

Scheduler errors do not rewrite Runtime work-item state directly.

---

## 228. Event Bus Mapping

Event Bus may interpret:

```text
EVENT_TRIGGER_ADAPTER_UNAVAILABLE
    → event-triggered schedules degraded

EVENT_TRIGGER_MAPPING_UNSAFE
    → security rejection

SCHEDULER_FAILED
    → infrastructure failure
```

---

## 229. Logging Mapping

Scheduler error logs include only safe identifiers, state, policy, and normalized codes.

---

## 230. Telemetry Mapping

Recommended metrics:

```text
scheduler_errors_total
scheduler_warnings_total
scheduler_queue_rejections_total
scheduler_worker_failures_total
scheduler_attempt_timeouts_total
scheduler_attempt_abandonments_total
scheduler_retry_exhausted_total
scheduler_resource_failures_total
scheduler_persistence_failures_total
scheduler_recovery_failures_total
scheduler_shutdown_failures_total
scheduler_security_blocks_total
```

Safe labels:

```text
code
category
scope
severity
workerClass
queueClass
resourceClass
```

Do not use job or attempt IDs as metric labels.

---

# Part XXVII — Observability Rules

## 231. Safe Error Logging

Allowed fields:

```text
errorCode
category
severity
taskId
scheduleId
jobId
attemptId
workerId
queueId
resourceClass
state
correlationId
```

Prohibited:

```text
jobInput
jobOutput
rawException
stackTrace
OCRText
translatedText
prompt
imageData
providerPayload
secret
credential
authorizationHeader
```

---

## 232. Tracing

Trace annotations may include:

- normalized error code;
- Scheduler stage;
- trigger type;
- queue class;
- resource class;
- attempt number;
- retry class;
- timeout duration;
- outcome.

No job payload is attached.

---

# Part XXVIII — Testing Requirements

## 233. Task and Schedule Tests

- invalid task;
- duplicate task;
- missing owner;
- missing worker;
- unbounded retry;
- unbounded timeout;
- invalid schedule;
- invalid cron;
- invalid timezone;
- missing overlap;
- missing misfire.

---

## 234. Input and Security Tests

- oversized input;
- unsupported type;
- unserializable durable input;
- secret-bearing input;
- raw user content;
- unsafe metadata;
- unauthorized manual run;
- unauthorized cancellation.

---

## 235. Queue and Dispatch Tests

- queue full;
- admission timeout;
- critical reserve exhaustion;
- claim conflict;
- dispatcher unavailable;
- no eligible worker;
- worker-selection conflict.

---

## 236. Attempt Tests

- startup failure;
- execution failure;
- timeout;
- cancellation;
- ignored cancellation;
- abandonment;
- duplicate completion;
- late completion;
- multiple terminal results.

---

## 237. Retry Tests

- non-retryable failure;
- maximum attempts;
- retry budget;
- invalid backoff;
- at-most-once retry block;
- uncertain outcome reconciliation.

---

## 238. Resource and Concurrency Tests

- lease unavailable;
- lease timeout;
- lease expiry;
- resource unavailable;
- reservation failure;
- reservation revocation;
- release failure;
- deadlock detection.

---

## 239. Persistence and Recovery Tests

- persistence unavailable;
- version conflict;
- corrupted state;
- uncertain authority;
- interrupted attempt;
- reconciliation failure;
- misfire restoration;
- partial recovery;
- recovery timeout.

---

## 240. Shutdown Tests

- trigger-stop failure;
- drain timeout;
- cancellation timeout;
- persistence failure;
- worker-stop failure;
- partial termination;
- bounded shutdown.

---

# Part XXIX — MVP Error Boundary

## 241. Required MVP Codes

The MVP should implement at least:

```text
SCHEDULER_TASK_INVALID
SCHEDULER_TASK_DUPLICATE
SCHEDULER_TASK_HANDLER_NOT_REGISTERED
SCHEDULER_TASK_TIMEOUT_UNBOUNDED
SCHEDULER_TASK_RETRY_UNBOUNDED
SCHEDULER_TASK_OVERLAP_POLICY_MISSING
SCHEDULER_TASK_MISFIRE_POLICY_MISSING

SCHEDULER_SCHEDULE_INVALID
SCHEDULER_SCHEDULE_TASK_NOT_FOUND
SCHEDULER_SCHEDULE_TIMEZONE_INVALID
SCHEDULER_SCHEDULE_INTERVAL_TOO_SHORT

SCHEDULER_TRIGGER_INVALID
SCHEDULER_CRON_EXPRESSION_INVALID
SCHEDULER_TRIGGER_EVALUATION_FAILED
SCHEDULER_EVENT_TRIGGER_MAPPING_UNSAFE

SCHEDULER_OVERLAP_EVALUATION_FAILED
SCHEDULER_MISFIRE_EVALUATION_FAILED
SCHEDULER_MISFIRE_CATCHUP_LIMIT_EXCEEDED

SCHEDULER_JOB_INVALID
SCHEDULER_JOB_INPUT_INVALID
SCHEDULER_JOB_INPUT_TOO_LARGE
SCHEDULER_JOB_INPUT_UNSUPPORTED_TYPE
SCHEDULER_JOB_INPUT_SECRET_BLOCKED
SCHEDULER_JOB_INPUT_USER_CONTENT_BLOCKED
SCHEDULER_JOB_TASK_VERSION_UNSUPPORTED
SCHEDULER_JOB_CREATION_FAILED

SCHEDULER_JOB_DUPLICATE_REJECTED
SCHEDULER_IDEMPOTENCY_KEY_REQUIRED
SCHEDULER_IDEMPOTENCY_CONFLICT
SCHEDULER_AT_MOST_ONCE_RETRY_BLOCKED

SCHEDULER_QUEUE_BACKPRESSURED
SCHEDULER_QUEUE_CAPACITY_EXCEEDED
SCHEDULER_JOB_ADMISSION_TIMED_OUT
SCHEDULER_QUEUE_CRITICAL_RESERVE_EXHAUSTED
SCHEDULER_QUEUE_CLAIM_CONFLICT
SCHEDULER_QUEUE_INVARIANT_BROKEN

SCHEDULER_DISPATCH_FAILED
SCHEDULER_NO_ELIGIBLE_WORKER
SCHEDULER_DISPATCH_AUTHORITY_UNCERTAIN

SCHEDULER_WORKER_INITIALIZATION_FAILED
SCHEDULER_WORKER_UNAVAILABLE
SCHEDULER_WORKER_START_FAILED
SCHEDULER_WORKER_EXECUTION_FAILED
SCHEDULER_WORKER_IGNORED_CANCELLATION
SCHEDULER_WORKER_DUPLICATE_COMPLETION
SCHEDULER_WORKER_LATE_COMPLETION_IGNORED

SCHEDULER_ATTEMPT_TERMINAL_STATE_CONFLICT
SCHEDULER_ATTEMPT_INTERRUPTED
SCHEDULER_ATTEMPT_OUTCOME_UNCERTAIN

SCHEDULER_ATTEMPT_TIMED_OUT
SCHEDULER_TIMEOUT_CANCELLATION_FAILED
SCHEDULER_ATTEMPT_ABANDONED

SCHEDULER_CANCELLATION_NOT_AUTHORIZED
SCHEDULER_CROSS_MODULE_CANCELLATION_BLOCKED
SCHEDULER_CANCELLATION_TIMED_OUT

SCHEDULER_RETRY_POLICY_INVALID
SCHEDULER_RETRY_EXHAUSTED
SCHEDULER_RETRY_SCHEDULING_FAILED
SCHEDULER_RETRY_REQUIRES_RECONCILIATION

SCHEDULER_CONCURRENCY_ACQUISITION_TIMED_OUT
SCHEDULER_CONCURRENCY_LEASE_EXPIRED
SCHEDULER_CONCURRENCY_LIMIT_BROKEN

SCHEDULER_RESOURCE_UNAVAILABLE
SCHEDULER_RESOURCE_ACQUISITION_TIMED_OUT
SCHEDULER_RESOURCE_RESERVATION_FAILED
SCHEDULER_RESOURCE_RESERVATION_EXPIRED
SCHEDULER_RESOURCE_RESERVATION_REVOKED

SCHEDULER_DEPENDENCY_FAILED
SCHEDULER_DEPENDENCY_CYCLE_DETECTED

SCHEDULER_CONFIGURATION_INVALID
SCHEDULER_NOT_RUNNING
SCHEDULER_QUIESCING
SCHEDULER_INVALID_STATE_TRANSITION
SCHEDULER_MULTIPLE_ACTIVE_ATTEMPTS
SCHEDULER_ACTIVE_ATTEMPT_WITHOUT_LEASE
SCHEDULER_FATAL_INVARIANT_BROKEN

SCHEDULER_DRAIN_TIMED_OUT
SCHEDULER_SHUTDOWN_CANCELLATION_FAILED
SCHEDULER_SHUTDOWN_TIMED_OUT
SCHEDULER_SHUTDOWN_PARTIALLY_TERMINATED

SCHEDULER_OPERATION_NOT_AUTHORIZED
SCHEDULER_MANUAL_RUN_NOT_AUTHORIZED
SCHEDULER_UNSAFE_METADATA_BLOCKED
SCHEDULER_RAW_JOB_PAYLOAD_EXPOSURE_BLOCKED
```

Persistence and recovery codes become mandatory when durable modes are enabled.

---

## 242. Required MVP Warnings

```text
SCHEDULER_WARNING_QUEUE_PRESSURE
SCHEDULER_WARNING_LOW_PRIORITY_JOB_DROPPED
SCHEDULER_WARNING_WORKER_SLOW
SCHEDULER_WARNING_WORKER_DEGRADED
SCHEDULER_WARNING_OPTIONAL_WORKER_UNAVAILABLE
SCHEDULER_WARNING_RESOURCE_WAIT
SCHEDULER_WARNING_CONCURRENCY_WAIT
SCHEDULER_WARNING_MISFIRE_SKIPPED
SCHEDULER_WARNING_RETRY_DELAYED
SCHEDULER_WARNING_LATE_COMPLETION
SCHEDULER_WARNING_DRAIN_PARTIAL
SCHEDULER_WARNING_SHUTDOWN_PARTIAL
```

---

# Part XXX — Decisions

## 243. Decisions

### Decision 1 — Errors contain no job payloads

No raw input, output, exception, user content, or secret material.

### Decision 2 — Attempt failure differs from job failure

A failed attempt may still lead to retry.

### Decision 3 — Timeout differs from cancellation

Timeout ends Scheduler authority and initiates cancellation.

### Decision 4 — Abandonment is explicit

Ignored cancellation does not remain indefinitely ambiguous.

### Decision 5 — Retry exhaustion is terminal

A new attempt cannot be created after exhaustion without an explicit new administrative action.

### Decision 6 — Resource and queue failures are distinct

Waiting for GPU is not the same as a full ready queue.

### Decision 7 — Uncertain outcomes require reconciliation

Blind retry may duplicate business side effects.

### Decision 8 — Exactly-once is never implied

No success state promises exactly-once business effect.

### Decision 9 — Worker failures are isolated

One worker failure does not fail unrelated tasks.

### Decision 10 — Security violations are fail-closed

Unsafe job input is blocked before scheduling.

### Decision 11 — Shutdown remains bounded

Cancellation, persistence, and worker stop cannot wait indefinitely.

### Decision 12 — Fatal invariant failures stop normal scheduling

Queue, lease, or terminal-state authority corruption requires shutdown.

---

# Part XXXI — Open Decisions

## 244. Severity Decisions

Still to finalize:

- when worker execution failure is warning versus error;
- when queue rejection becomes critical;
- when an interrupted attempt is warning versus error;
- shutdown partial-termination severity;
- optional persistence failure severity.

---

## 245. Retry Decisions

Still to finalize:

- default maximum attempts;
- default retry budget;
- timeout retry defaults;
- interrupted-attempt retry behavior;
- retry scheduling under shutdown;
- late-completion reconciliation.

---

## 246. Resource Decisions

Still to finalize:

- reservation expiry severity;
- revocation behavior;
- provider quota recovery;
- resource deadlock prevention;
- UI-sensitive pressure warning thresholds.

---

## 247. Persistence and Recovery Decisions

Still to finalize:

- durable authority boundary;
- reconciliation source priority;
- corrupted-state quarantine;
- interrupted-attempt classification;
- partial recovery policy;
- completed-job retention.

---

# Part XXXII — Related Documents

## 248. Related Documents

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
03-infrastructure/scheduler/EVENTS.md

03-infrastructure/event-bus/ERRORS.md
03-infrastructure/logging/ERRORS.md
03-infrastructure/telemetry/ERRORS.md
```

Future document:

```text
03-infrastructure/scheduler/README.md
```

---

## 249. Summary

Scheduler errors normalize failures across task registration, schedules, triggers, job creation, queue admission, dispatch, worker execution, retries, timeout, cancellation, concurrency, resources, dependencies, persistence, recovery, and shutdown.

The error flow is:

```text
Raw trigger / queue / worker / persistence failure
    ↓
Scheduler boundary catches failure
    ↓
Raw payloads and exceptions removed
    ↓
Normalized SchedulerError created
    ↓
Job, attempt, and authority state evaluated
    ↓
Retry, reconciliation, or terminal transition selected
    ↓
Safe Logging and Telemetry
```

The model preserves these distinctions:

```text
Task Failure
    ≠ Schedule Failure
    ≠ Queue Rejection
    ≠ Dispatch Failure
    ≠ Attempt Failure
    ≠ Job Failure
    ≠ Timeout
    ≠ Cancellation
    ≠ Abandonment
    ≠ Recovery Uncertainty
```

The architecture guarantees:

- errors contain no raw job inputs or outputs;
- secret and user content remain absent;
- attempt and job failures remain separate;
- retries are bounded;
- timeout ends logical authority;
- late completion is non-authoritative;
- uncertain outcomes require reconciliation;
- resource and queue pressure remain distinct;
- worker failures are isolated;
- exactly-once is not implied;
- unsafe job inputs fail closed;
- queue, retry, cancellation, and shutdown remain bounded;
- fatal authority corruption stops normal scheduling.

This document is the error source of truth for the Scheduler implementation and README.
