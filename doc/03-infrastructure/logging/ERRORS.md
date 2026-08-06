# Logging Errors

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Logging  
> **Document:** Errors and Warnings  
> **Path:** `03-infrastructure/logging/ERRORS.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/logging/MODULE.md`
> - `03-infrastructure/logging/CONTRACT.md`
> - `03-infrastructure/logging/STATES.md`
> - `03-infrastructure/logging/EVENTS.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RETRY_POLICY.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`
> - `03-infrastructure/secret-management/ERRORS.md`
> - `03-infrastructure/event-bus/ERRORS.md`

---

## 1. Purpose

This document defines normalized errors and warnings owned by the Logging infrastructure module.

It covers:

- record validation errors;
- context and scope errors;
- property and size errors;
- privacy and classification errors;
- secret and user-content blocking;
- redaction and safety-inspection errors;
- exception-normalization errors;
- policy and configuration errors;
- buffer admission, pressure, overflow, and reserve errors;
- sink registration, routing, write, flush, and shutdown errors;
- formatter and serialization errors;
- rolling-file creation, write, rotation, recovery, and retention errors;
- compression errors;
- diagnostics query and export errors;
- audit-write errors;
- bootstrap and emergency logger errors;
- recursion and self-reporting errors;
- concurrency and lifecycle invariant errors;
- warnings and partial outcomes;
- retry and recovery guidance;
- cross-module normalization.

This document does not define:

- domain-specific errors;
- Event Bus errors;
- Telemetry errors;
- provider-native exceptions;
- raw operating-system exceptions;
- UI wording;
- alert thresholds;
- exact retry schedules;
- exact file-system error codes.

---

## 2. Error Design Goals

Logging errors must:

1. never contain original unsafe log content;
2. never contain secret material;
3. distinguish record rejection from sink failure;
4. distinguish buffer pressure from buffer corruption;
5. distinguish write failure from flush failure;
6. distinguish timeout from cancellation;
7. distinguish partial write from uncertain write;
8. distinguish restricted-sink failure from ordinary-sink failure;
9. distinguish diagnostics-export failure from normal logging failure;
10. distinguish ordinary log persistence from audit persistence;
11. preserve record, sink, buffer, batch, file, rotation, flush, export, and audit identity;
12. remain non-recursive;
13. fail closed when redaction or safety cannot be proven;
14. isolate sink failures;
15. preserve bounded shutdown;
16. provide safe retry and recovery guidance;
17. avoid overstating persistence or durability;
18. support future remote sinks without changing core meanings.

---

## 3. Error Versus Warning

An error prevents safe record admission, persistence, export, or lifecycle completion.

A warning describes degraded but still usable behavior.

Examples:

```text
Normal sink slow
    → warning

Normal sink unavailable, fallback available
    → warning or error depending on policy

Restricted sink unavailable with no approved fallback
    → critical error

Retention cleanup partially completed
    → warning

Redaction cannot prove record safety
    → critical error, record blocked
```

---

## 4. Error Versus Record Outcome

These outcomes are not automatically errors:

```text
FILTERED
SAMPLED_OUT
SUPPRESSED
DROPPED_LOW_SEVERITY
PARTIALLY_FLUSHED
PARTIALLY_EXPORTED
PARTIALLY_WRITTEN
```

They become errors when:

- a caller required persistence confirmation;
- a mandatory sink failed;
- a restricted or audit policy was violated;
- a critical record was lost;
- safety guarantees could not be maintained.

---

## 5. Error Versus Cancellation

Cancellation is expected control flow when explicitly requested.

```text
Diagnostics export canceled before destination write
    → cancellation outcome

Flush canceled by caller before shutdown barrier
    → cancellation outcome

Sink ignores shutdown cancellation and deadline expires
    → timeout / abandonment error
```

---

## 6. Error Ownership

Logging owns normalized errors concerning:

- log-record structure;
- safety inspection;
- redaction;
- exception normalization;
- policy;
- buffers;
- sinks;
- formatting;
- files;
- rotation;
- retention;
- compression;
- flush;
- diagnostics;
- export;
- audit routing;
- bootstrap and emergency paths;
- Logging lifecycle and invariants.

Logging does not own the original semantic failure that a producer is trying to log.

That failure remains owned by the producer module.

---

## 7. Canonical Error Model

```text
LoggingError {
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

    recordId?
    scopeId?
    policyRevision?
    bufferId?
    sinkId?
    batchId?
    logicalFileId?
    rotationId?
    flushId?
    cleanupId?
    compressionId?
    diagnosticsQueryId?
    exportId?
    auditRecordId?
    bootstrapLoggerId?
    emergencyLoggerId?

    correlationId?
    causationId?
    applicationInstanceId

    occurredAt

    cause?
    metadata
}
```

---

## 8. Error Categories

```text
RECORD
CONTEXT
SCOPE
PROPERTY
SIZE
POLICY
CONFIGURATION
PRIVACY
CLASSIFICATION
SAFETY
REDACTION
EXCEPTION
BUFFER
BACKPRESSURE
ADMISSION
SINK
ROUTING
FORMATTER
SERIALIZATION
WRITE
FILE
ROTATION
RETENTION
COMPRESSION
FLUSH
DIAGNOSTICS
EXPORT
AUDIT
BOOTSTRAP
EMERGENCY
RECURSION
LIFECYCLE
CONCURRENCY
PERSISTENCE
INTERNAL
```

---

## 9. Error Scopes

```text
LOG_RECORD
LOG_SCOPE
LOG_POLICY
LOG_BUFFER
LOG_SINK
LOG_BATCH
LOG_FILE
ROTATION
RETENTION_CLEANUP
COMPRESSION
FLUSH_OPERATION
DIAGNOSTICS_QUERY
DIAGNOSTICS_EXPORT
AUDIT_RECORD
BOOTSTRAP_LOGGER
EMERGENCY_LOGGER
LOGGING_INSTANCE
APPLICATION_INSTANCE
```

---

## 10. Severity

```text
TRACE
NOTICE
WARNING
ERROR
CRITICAL
FATAL
```

### NOTICE

Expected caller correction or policy outcome.

### WARNING

Degraded but safe continuation.

### ERROR

An operation failed.

### CRITICAL

Safety, restricted-routing, audit, or major lifecycle failure.

### FATAL

Logging cannot enforce redaction, boundedness, or safe routing.

---

## 11. Retry Class

```text
NEVER
IMMEDIATE
TRANSIENT
AFTER_CAPACITY_RECOVERY
AFTER_SINK_RECOVERY
AFTER_CONFIGURATION_CHANGE
AFTER_RESTART
IDEMPOTENT_ONLY
AFTER_RECONCILIATION
UNKNOWN
```

---

## 12. Recoverability

```text
AUTOMATIC
CALLER_CORRECTION
CONFIGURATION_CHANGE
SINK_RECOVERY
APPLICATION_RESTART
ADMIN_ACTION
RECONCILIATION_REQUIRED
NOT_RECOVERABLE
UNKNOWN
```

---

## 13. Recovery Actions

```text
RETRY_WRITE
WAIT_AND_RETRY
REDUCE_LOG_LEVEL
REDUCE_LOG_RATE
REDUCE_RECORD_SIZE
REMOVE_UNSAFE_PROPERTY
USE_SAFE_REFERENCE
UPDATE_LOG_POLICY
UPDATE_SINK_CONFIGURATION
RESTORE_SINK
ROTATE_LOG_FILE
FREE_DISK_SPACE
CHECK_FILE_PERMISSIONS
RECREATE_ACTIVE_FILE
RUN_RETENTION_CLEANUP
RECONCILE_ROTATION
RETRY_FLUSH
RETRY_EXPORT
RESELECT_EXPORT_DESTINATION
RESTORE_AUDIT_SINK
USE_APPROVED_EMERGENCY_PATH
RESTART_LOGGING
RESTART_APPLICATION
CONTACT_SUPPORT
NONE
```

---

## 14. Error Code Naming

Canonical format:

```text
LOGGING_<CONCERN>_<CONDITION>
```

Examples:

```text
LOGGING_RECORD_INVALID
LOGGING_REDACTION_FAILED_SAFE
LOGGING_SINK_UNAVAILABLE
LOGGING_ROTATION_OUTCOME_UNCERTAIN
```

Warnings use:

```text
LOGGING_WARNING_<CONDITION>
```

Security failures may use:

```text
LOGGING_SECURITY_<CONDITION>
```

---

# Part I — Record Validation Errors

## 15. LOGGING_RECORD_INVALID

The record draft is malformed.

```text
category: RECORD
scope: LOG_RECORD
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

Examples:

- missing severity;
- missing category;
- missing message template;
- missing source module;
- invalid classification.

---

## 16. LOGGING_RECORD_ID_INVALID

```text
category: RECORD
scope: LOG_RECORD
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 17. LOGGING_MESSAGE_TEMPLATE_MISSING

```text
category: RECORD
scope: LOG_RECORD
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 18. LOGGING_MESSAGE_TEMPLATE_UNSAFE

The template contains secret or prohibited user-controlled content.

```text
category: SAFETY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 19. LOGGING_CATEGORY_INVALID

```text
category: RECORD
scope: LOG_RECORD
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 20. LOGGING_SEVERITY_INVALID

```text
category: RECORD
scope: LOG_RECORD
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 21. LOGGING_RECORD_SIZE_EXCEEDED

```text
category: SIZE
scope: LOG_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

Recovery:

```text
Reduce properties, use safe references, or summarize.
```

---

## 22. LOGGING_RECORD_PROPERTY_COUNT_EXCEEDED

```text
category: SIZE
scope: LOG_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 23. LOGGING_RECORD_UNSUPPORTED_VALUE_TYPE

```text
category: PROPERTY
scope: LOG_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

Arbitrary object reflection is prohibited.

---

## 24. LOGGING_RECORD_MUTABLE_VALUE_BLOCKED

```text
category: SAFETY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 25. LOGGING_RESERVED_PROPERTY_OVERRIDE_BLOCKED

```text
category: PROPERTY
scope: LOG_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

# Part II — Context and Scope Errors

## 26. LOGGING_CONTEXT_INVALID

```text
category: CONTEXT
scope: LOG_RECORD
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 27. LOGGING_CONTEXT_VALUE_UNSAFE

```text
category: SAFETY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 28. LOGGING_SCOPE_INVALID

```text
category: SCOPE
scope: LOG_SCOPE
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 29. LOGGING_SCOPE_DEPTH_EXCEEDED

```text
category: SCOPE
scope: LOG_SCOPE
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 30. LOGGING_SCOPE_CONTEXT_LEAK_DETECTED

A scope leaked into unrelated async work.

```text
category: INTERNAL
scope: LOG_SCOPE
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 31. LOGGING_SCOPE_ALREADY_DISPOSED

```text
category: SCOPE
scope: LOG_SCOPE
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

Repeated disposal is normally idempotent.

---

# Part III — Privacy and Classification Errors

## 32. LOGGING_PRIVACY_CLASSIFICATION_INVALID

```text
category: PRIVACY
scope: LOG_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 33. LOGGING_SECURITY_CLASSIFICATION_INVALID

```text
category: CLASSIFICATION
scope: LOG_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 34. LOGGING_CLASSIFICATION_DOWNGRADE_BLOCKED

```text
category: CLASSIFICATION
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 35. LOGGING_SECRET_DATA_BLOCKED

Secret material was detected.

```text
category: SAFETY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

The matched data must not be included in the error.

---

## 36. LOGGING_AUTHORIZATION_HEADER_BLOCKED

```text
category: SAFETY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 37. LOGGING_PRIVATE_KEY_BLOCKED

```text
category: SAFETY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 38. LOGGING_PASSWORD_FIELD_BLOCKED

```text
category: SAFETY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 39. LOGGING_TOKEN_FIELD_BLOCKED

```text
category: SAFETY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 40. LOGGING_RAW_USER_CONTENT_BLOCKED

```text
category: PRIVACY
scope: LOG_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 41. LOGGING_RAW_PROVIDER_PAYLOAD_BLOCKED

```text
category: SAFETY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 42. LOGGING_RAW_ENVIRONMENT_VALUE_BLOCKED

```text
category: SAFETY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 43. LOGGING_UNSAFE_URI_BLOCKED

```text
category: SAFETY
scope: LOG_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 44. LOGGING_UNSAFE_PATH_BLOCKED

```text
category: PRIVACY
scope: LOG_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

# Part IV — Redaction and Safety Errors

## 45. LOGGING_SAFETY_INSPECTION_FAILED_SAFE

Safety inspection failed and the record was blocked.

```text
category: SAFETY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 46. LOGGING_REDACTION_REQUIRED

A record requires redaction before admission.

```text
category: REDACTION
scope: LOG_RECORD
severity: NOTICE
retryClass: NEVER
recoverability: AUTOMATIC
```

This is usually an internal outcome, not a caller-visible error.

---

## 47. LOGGING_REDACTION_FAILED_SAFE

```text
category: REDACTION
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

The unsafe record must be blocked.

---

## 48. LOGGING_REDACTION_TRANSFORMATION_UNSUPPORTED

```text
category: REDACTION
scope: LOG_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 49. LOGGING_REDACTION_POLICY_INVALID

```text
category: REDACTION
scope: LOG_POLICY
severity: CRITICAL
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 50. LOGGING_REDACTION_FALSE_POSITIVE

A safe value was blocked.

```text
category: REDACTION
scope: LOG_RECORD
severity: WARNING
retryClass: NEVER
recoverability: ADMIN_ACTION
```

The original value still must not be echoed into unrestricted diagnostics.

---

## 51. LOGGING_SAFETY_CLASSIFIER_UNAVAILABLE

```text
category: SAFETY
scope: LOGGING_INSTANCE
severity: FATAL
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

Normal admission must stop.

---

# Part V — Exception Normalization Errors

## 52. LOGGING_EXCEPTION_NORMALIZATION_FAILED_SAFE

```text
category: EXCEPTION
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

The raw exception must not be persisted.

---

## 53. LOGGING_EXCEPTION_MESSAGE_UNSAFE

```text
category: EXCEPTION
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 54. LOGGING_EXCEPTION_STACK_UNSAFE

```text
category: EXCEPTION
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 55. LOGGING_EXCEPTION_CAUSE_DEPTH_EXCEEDED

```text
category: EXCEPTION
scope: LOG_RECORD
severity: WARNING
retryClass: NEVER
recoverability: AUTOMATIC
```

The cause chain is truncated safely.

---

# Part VI — Policy and Configuration Errors

## 56. LOGGING_POLICY_INVALID

```text
category: POLICY
scope: LOG_POLICY
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 57. LOGGING_POLICY_REVISION_CONFLICT

```text
category: CONCURRENCY
scope: LOG_POLICY
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC
```

---

## 58. LOGGING_POLICY_CLASSIFICATION_CONFLICT

```text
category: POLICY
scope: LOG_POLICY
severity: CRITICAL
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 59. LOGGING_POLICY_UNBOUNDED_RETENTION

```text
category: POLICY
scope: LOG_POLICY
severity: CRITICAL
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 60. LOGGING_POLICY_UNSAFE_FALLBACK

```text
category: POLICY
scope: LOG_POLICY
severity: CRITICAL
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 61. LOGGING_POLICY_AUDIT_SAMPLING_NOT_ALLOWED

```text
category: POLICY
scope: LOG_POLICY
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 62. LOGGING_CONFIGURATION_INVALID

```text
category: CONFIGURATION
scope: LOGGING_INSTANCE
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 63. LOGGING_CONFIGURATION_RESTART_REQUIRED

```text
category: CONFIGURATION
scope: LOGGING_INSTANCE
severity: NOTICE
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 64. LOGGING_REMOTE_CREDENTIAL_REFERENCE_INVALID

```text
category: CONFIGURATION
scope: LOG_SINK
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

Raw remote credentials are prohibited.

---

# Part VII — Buffer and Admission Errors

## 65. LOGGING_BUFFER_NOT_INITIALIZED

```text
category: BUFFER
scope: LOG_BUFFER
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 66. LOGGING_BUFFER_NOT_RUNNING

```text
category: BUFFER
scope: LOG_BUFFER
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 67. LOGGING_BUFFER_BACKPRESSURED

```text
category: BACKPRESSURE
scope: LOG_BUFFER
severity: WARNING
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 68. LOGGING_BUFFER_CAPACITY_EXCEEDED

```text
category: BUFFER
scope: LOG_BUFFER
severity: ERROR
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 69. LOGGING_BUFFER_ADMISSION_TIMED_OUT

```text
category: ADMISSION
scope: LOG_RECORD
severity: ERROR
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 70. LOGGING_LOW_SEVERITY_RECORD_DROPPED

```text
category: BUFFER
scope: LOG_RECORD
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 71. LOGGING_CRITICAL_RESERVE_USED

```text
category: BUFFER
scope: LOG_BUFFER
severity: WARNING
retryClass: NEVER
recoverability: AUTOMATIC
```

---

## 72. LOGGING_CRITICAL_RESERVE_EXHAUSTED

```text
category: BUFFER
scope: LOGGING_INSTANCE
severity: CRITICAL
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: APPLICATION_RESTART or AUTOMATIC
```

---

## 73. LOGGING_BUFFER_MEMORY_LIMIT_EXCEEDED

```text
category: BUFFER
scope: LOGGING_INSTANCE
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 74. LOGGING_BUFFER_INVARIANT_BROKEN

```text
category: INTERNAL
scope: LOG_BUFFER
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 75. LOGGING_RECORD_SUPPRESSION_CONFLICT

Suppression attempted to hide a non-suppressible record.

```text
category: POLICY
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

# Part VIII — Sink Registration and Routing Errors

## 76. LOGGING_SINK_ID_DUPLICATE

```text
category: SINK
scope: LOG_SINK
severity: ERROR
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 77. LOGGING_SINK_TYPE_UNSUPPORTED

```text
category: SINK
scope: LOG_SINK
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 78. LOGGING_SINK_CONFIGURATION_INVALID

```text
category: SINK
scope: LOG_SINK
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 79. LOGGING_SINK_CLASSIFICATION_MISMATCH

```text
category: CLASSIFICATION
scope: LOG_SINK
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 80. LOGGING_SINK_ROUTING_FAILED

```text
category: ROUTING
scope: LOG_RECORD
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 81. LOGGING_NO_ELIGIBLE_SINK

```text
category: ROUTING
scope: LOG_RECORD
severity: ERROR
retryClass: AFTER_SINK_RECOVERY
recoverability: SINK_RECOVERY
```

---

## 82. LOGGING_MANDATORY_SINK_UNAVAILABLE

```text
category: SINK
scope: LOGGING_INSTANCE
severity: CRITICAL
retryClass: AFTER_SINK_RECOVERY
recoverability: SINK_RECOVERY
```

---

## 83. LOGGING_RESTRICTED_SINK_UNAVAILABLE

```text
category: SINK
scope: LOGGING_INSTANCE
severity: CRITICAL
retryClass: AFTER_SINK_RECOVERY
recoverability: SINK_RECOVERY
```

Restricted records must not fall back to weaker sinks.

---

## 84. LOGGING_UNSAFE_FALLBACK_BLOCKED

```text
category: CLASSIFICATION
scope: LOG_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 85. LOGGING_SINK_RECOVERY_FAILED

```text
category: SINK
scope: LOG_SINK
severity: ERROR
retryClass: AFTER_SINK_RECOVERY
recoverability: SINK_RECOVERY
```

---

# Part IX — Formatter and Serialization Errors

## 86. LOGGING_FORMATTER_NOT_REGISTERED

```text
category: FORMATTER
scope: LOG_SINK
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 87. LOGGING_FORMATTER_FAILED_SAFE

```text
category: FORMATTER
scope: LOG_BATCH
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

The batch must not be written through the unsafe formatter.

---

## 88. LOGGING_FORMATTER_OUTPUT_TOO_LARGE

```text
category: FORMATTER
scope: LOG_BATCH
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 89. LOGGING_SERIALIZATION_FAILED_SAFE

```text
category: SERIALIZATION
scope: LOG_BATCH
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 90. LOGGING_SERIALIZATION_UNSUPPORTED_TYPE

```text
category: SERIALIZATION
scope: LOG_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

# Part X — Sink Write Errors

## 91. LOGGING_SINK_INITIALIZATION_FAILED

```text
category: SINK
scope: LOG_SINK
severity: ERROR
retryClass: TRANSIENT
recoverability: SINK_RECOVERY
```

---

## 92. LOGGING_SINK_UNAVAILABLE

```text
category: SINK
scope: LOG_SINK
severity: ERROR
retryClass: AFTER_SINK_RECOVERY
recoverability: SINK_RECOVERY
```

---

## 93. LOGGING_SINK_PERMISSION_DENIED

```text
category: SINK
scope: LOG_SINK
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 94. LOGGING_SINK_DISK_FULL

```text
category: SINK
scope: LOG_SINK
severity: CRITICAL
retryClass: AFTER_SINK_RECOVERY
recoverability: ADMIN_ACTION
```

Recovery:

```text
FREE_DISK_SPACE
RUN_RETENTION_CLEANUP
```

---

## 95. LOGGING_SINK_WRITE_FAILED

```text
category: WRITE
scope: LOG_BATCH
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: SINK_RECOVERY
```

---

## 96. LOGGING_SINK_PARTIAL_WRITE

```text
category: WRITE
scope: LOG_BATCH
severity: WARNING or ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: SINK_RECOVERY
```

---

## 97. LOGGING_SINK_WRITE_TIMED_OUT

```text
category: WRITE
scope: LOG_BATCH
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

Physical write may still complete.

---

## 98. LOGGING_SINK_WRITE_OUTCOME_UNCERTAIN

```text
category: WRITE
scope: LOG_BATCH
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

Blind retry may duplicate records.

---

## 99. LOGGING_SINK_WRITE_ABANDONED

```text
category: WRITE
scope: LOG_BATCH
severity: WARNING or ERROR
retryClass: NEVER for same batch
recoverability: SINK_RECOVERY
```

---

## 100. LOGGING_LATE_WRITE_COMPLETION_IGNORED

```text
category: WRITE
scope: LOG_BATCH
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

---

## 101. LOGGING_SINK_SHUTDOWN_FAILED

```text
category: SINK
scope: LOG_SINK
severity: ERROR
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

# Part XI — File Lifecycle Errors

## 102. LOGGING_FILE_PATH_INVALID

```text
category: FILE
scope: LOG_FILE
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 103. LOGGING_FILE_PATH_ESCAPE_BLOCKED

A path escaped the approved application log directory.

```text
category: SAFETY
scope: LOG_FILE
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 104. LOGGING_FILE_CREATE_FAILED

```text
category: FILE
scope: LOG_FILE
severity: ERROR
retryClass: TRANSIENT
recoverability: SINK_RECOVERY
```

---

## 105. LOGGING_FILE_PERMISSION_DENIED

```text
category: FILE
scope: LOG_FILE
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 106. LOGGING_FILE_WRITE_FAILED

```text
category: FILE
scope: LOG_FILE
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: SINK_RECOVERY
```

---

## 107. LOGGING_FILE_FLUSH_FAILED

```text
category: FILE
scope: LOG_FILE
severity: ERROR
retryClass: TRANSIENT
recoverability: SINK_RECOVERY
```

---

## 108. LOGGING_FILE_CLOSE_FAILED

```text
category: FILE
scope: LOG_FILE
severity: ERROR
retryClass: TRANSIENT
recoverability: SINK_RECOVERY
```

---

## 109. LOGGING_FILE_CORRUPTED

```text
category: FILE
scope: LOG_FILE
severity: CRITICAL
retryClass: NEVER
recoverability: RECREATE_ACTIVE_FILE or ADMIN_ACTION
```

---

## 110. LOGGING_FILE_ORPHANED

```text
category: FILE
scope: LOG_FILE
severity: WARNING
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 111. LOGGING_ACTIVE_FILE_MISSING

```text
category: FILE
scope: LOG_SINK
severity: CRITICAL
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 112. LOGGING_MULTIPLE_ACTIVE_FILES

```text
category: FILE
scope: LOG_SINK
severity: CRITICAL
retryClass: NEVER
recoverability: RECONCILIATION_REQUIRED
```

Normal file writes must pause.

---

# Part XII — Rotation Errors

## 113. LOGGING_ROTATION_INVALID

```text
category: ROTATION
scope: ROTATION
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 114. LOGGING_ROTATION_ALREADY_RUNNING

```text
category: ROTATION
scope: ROTATION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 115. LOGGING_ROTATION_NOT_SUPPORTED

```text
category: ROTATION
scope: LOG_SINK
severity: ERROR
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 116. LOGGING_ROTATION_ACTIVE_FILE_FLUSH_FAILED

```text
category: ROTATION
scope: ROTATION
severity: ERROR
retryClass: TRANSIENT
recoverability: SINK_RECOVERY
```

---

## 117. LOGGING_ROTATION_CLOSE_FAILED

```text
category: ROTATION
scope: ROTATION
severity: ERROR
retryClass: TRANSIENT
recoverability: SINK_RECOVERY
```

---

## 118. LOGGING_ROTATION_FINALIZE_FAILED

```text
category: ROTATION
scope: ROTATION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: SINK_RECOVERY
```

---

## 119. LOGGING_ROTATION_NEW_FILE_CREATE_FAILED

```text
category: ROTATION
scope: ROTATION
severity: CRITICAL
retryClass: TRANSIENT
recoverability: SINK_RECOVERY
```

The previous file may already be sealed.

---

## 120. LOGGING_ROTATION_ACTIVATION_FAILED

```text
category: ROTATION
scope: ROTATION
severity: CRITICAL
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 121. LOGGING_ROTATION_TIMED_OUT

```text
category: ROTATION
scope: ROTATION
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 122. LOGGING_ROTATION_OUTCOME_UNCERTAIN

```text
category: ROTATION
scope: ROTATION
severity: CRITICAL
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

Normal writes should pause or use approved emergency path.

---

## 123. LOGGING_ROTATION_RECONCILIATION_FAILED

```text
category: ROTATION
scope: ROTATION
severity: CRITICAL
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 124. LOGGING_ROTATION_PARTIALLY_COMPLETED

```text
category: ROTATION
scope: ROTATION
severity: WARNING
retryClass: conditional
recoverability: AUTOMATIC or ADMIN_ACTION
```

---

# Part XIII — Retention Errors

## 125. LOGGING_RETENTION_POLICY_INVALID

```text
category: RETENTION
scope: LOG_POLICY
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 126. LOGGING_RETENTION_SCAN_FAILED

```text
category: RETENTION
scope: RETENTION_CLEANUP
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 127. LOGGING_RETENTION_UNKNOWN_FILE_SKIPPED

```text
category: RETENTION
scope: LOG_FILE
severity: WARNING
retryClass: NEVER
recoverability: ADMIN_ACTION
```

Unknown files must not be deleted automatically.

---

## 128. LOGGING_RETENTION_DELETE_FAILED

```text
category: RETENTION
scope: LOG_FILE
severity: WARNING or ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 129. LOGGING_RETENTION_TIMED_OUT

```text
category: RETENTION
scope: RETENTION_CLEANUP
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 130. LOGGING_RETENTION_PARTIALLY_COMPLETED

```text
category: RETENTION
scope: RETENTION_CLEANUP
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 131. LOGGING_RETENTION_ACTIVE_FILE_PROTECTION_FAILED

```text
category: RETENTION
scope: LOG_FILE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

# Part XIV — Compression Errors

## 132. LOGGING_COMPRESSION_NOT_SUPPORTED

```text
category: COMPRESSION
scope: LOG_FILE
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 133. LOGGING_COMPRESSION_SOURCE_READ_FAILED

```text
category: COMPRESSION
scope: COMPRESSION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 134. LOGGING_COMPRESSION_ARCHIVE_WRITE_FAILED

```text
category: COMPRESSION
scope: COMPRESSION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 135. LOGGING_COMPRESSION_VERIFY_FAILED

```text
category: COMPRESSION
scope: COMPRESSION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

The source must remain preserved.

---

## 136. LOGGING_COMPRESSION_SOURCE_DELETE_FAILED

```text
category: COMPRESSION
scope: COMPRESSION
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

A verified archive exists.

---

## 137. LOGGING_COMPRESSION_TIMED_OUT

```text
category: COMPRESSION
scope: COMPRESSION
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

# Part XV — Flush Errors

## 138. LOGGING_FLUSH_INVALID

```text
category: FLUSH
scope: FLUSH_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 139. LOGGING_FLUSH_ALREADY_RUNNING

```text
category: FLUSH
scope: FLUSH_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 140. LOGGING_FLUSH_BUFFER_DRAIN_FAILED

```text
category: FLUSH
scope: FLUSH_OPERATION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 141. LOGGING_FLUSH_SINK_FAILED

```text
category: FLUSH
scope: FLUSH_OPERATION
severity: ERROR
retryClass: TRANSIENT
recoverability: SINK_RECOVERY
```

---

## 142. LOGGING_FLUSH_PARTIALLY_COMPLETED

```text
category: FLUSH
scope: FLUSH_OPERATION
severity: WARNING
retryClass: conditional
recoverability: AUTOMATIC
```

---

## 143. LOGGING_FLUSH_TIMED_OUT

```text
category: FLUSH
scope: FLUSH_OPERATION
severity: WARNING or ERROR
retryClass: NEVER during shutdown
recoverability: NONE or SINK_RECOVERY
```

---

## 144. LOGGING_FLUSH_PERSISTENCE_CONFIRMATION_FAILED

```text
category: PERSISTENCE
scope: FLUSH_OPERATION
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

# Part XVI — Diagnostics Query Errors

## 145. LOGGING_DIAGNOSTICS_QUERY_INVALID

```text
category: DIAGNOSTICS
scope: DIAGNOSTICS_QUERY
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 146. LOGGING_DIAGNOSTICS_ACCESS_DENIED

```text
category: DIAGNOSTICS
scope: DIAGNOSTICS_QUERY
severity: ERROR
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 147. LOGGING_DIAGNOSTICS_RESTRICTED_ACCESS_DENIED

```text
category: CLASSIFICATION
scope: DIAGNOSTICS_QUERY
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 148. LOGGING_DIAGNOSTICS_READ_FAILED

```text
category: DIAGNOSTICS
scope: DIAGNOSTICS_QUERY
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 149. LOGGING_DIAGNOSTICS_OUTPUT_REDACTION_FAILED_SAFE

```text
category: REDACTION
scope: DIAGNOSTICS_QUERY
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 150. LOGGING_DIAGNOSTICS_QUERY_TIMED_OUT

```text
category: DIAGNOSTICS
scope: DIAGNOSTICS_QUERY
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

# Part XVII — Diagnostics Export Errors

## 151. LOGGING_EXPORT_INVALID

```text
category: EXPORT
scope: DIAGNOSTICS_EXPORT
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 152. LOGGING_EXPORT_ACCESS_DENIED

```text
category: EXPORT
scope: DIAGNOSTICS_EXPORT
severity: ERROR
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 153. LOGGING_EXPORT_DESTINATION_INVALID

```text
category: EXPORT
scope: DIAGNOSTICS_EXPORT
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 154. LOGGING_EXPORT_DESTINATION_UNSAFE

```text
category: SAFETY
scope: DIAGNOSTICS_EXPORT
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 155. LOGGING_EXPORT_RAW_FILE_COPY_BLOCKED

```text
category: SAFETY
scope: DIAGNOSTICS_EXPORT
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 156. LOGGING_EXPORT_RECORD_SELECTION_FAILED

```text
category: EXPORT
scope: DIAGNOSTICS_EXPORT
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 157. LOGGING_EXPORT_REDACTION_FAILED_SAFE

```text
category: REDACTION
scope: DIAGNOSTICS_EXPORT
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 158. LOGGING_EXPORT_BUNDLE_INSPECTION_FAILED_SAFE

```text
category: SAFETY
scope: DIAGNOSTICS_EXPORT
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

No bundle may be delivered.

---

## 159. LOGGING_EXPORT_BUNDLE_TOO_LARGE

```text
category: SIZE
scope: DIAGNOSTICS_EXPORT
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 160. LOGGING_EXPORT_WRITE_FAILED

```text
category: EXPORT
scope: DIAGNOSTICS_EXPORT
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 161. LOGGING_EXPORT_PARTIAL_BUNDLE_CLEANUP_FAILED

```text
category: EXPORT
scope: DIAGNOSTICS_EXPORT
severity: CRITICAL
retryClass: TRANSIENT
recoverability: ADMIN_ACTION
```

The partial bundle may contain restricted data.

---

## 162. LOGGING_EXPORT_TIMED_OUT

```text
category: EXPORT
scope: DIAGNOSTICS_EXPORT
severity: WARNING or ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 163. LOGGING_EXPORT_PARTIALLY_COMPLETED

```text
category: EXPORT
scope: DIAGNOSTICS_EXPORT
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

---

# Part XVIII — Audit Errors

## 164. LOGGING_AUDIT_RECORD_INVALID

```text
category: AUDIT
scope: AUDIT_RECORD
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 165. LOGGING_AUDIT_RECORD_UNSAFE

```text
category: AUDIT
scope: AUDIT_RECORD
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 166. LOGGING_AUDIT_SINK_UNAVAILABLE

```text
category: AUDIT
scope: AUDIT_RECORD
severity: CRITICAL
retryClass: AFTER_SINK_RECOVERY
recoverability: SINK_RECOVERY
```

---

## 167. LOGGING_AUDIT_WRITE_FAILED

```text
category: AUDIT
scope: AUDIT_RECORD
severity: CRITICAL
retryClass: IDEMPOTENT_ONLY
recoverability: SINK_RECOVERY
```

---

## 168. LOGGING_AUDIT_WRITE_TIMED_OUT

```text
category: AUDIT
scope: AUDIT_RECORD
severity: CRITICAL
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 169. LOGGING_AUDIT_WRITE_OUTCOME_UNCERTAIN

```text
category: AUDIT
scope: AUDIT_RECORD
severity: CRITICAL
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 170. LOGGING_AUDIT_RECONCILIATION_FAILED

```text
category: AUDIT
scope: AUDIT_RECORD
severity: CRITICAL
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 171. LOGGING_AUDIT_MANDATORY_ACTION_BLOCKED

```text
category: AUDIT
scope: AUDIT_RECORD
severity: CRITICAL
retryClass: AFTER_SINK_RECOVERY
recoverability: SINK_RECOVERY
```

The owning action must remain blocked under fail-closed policy.

---

# Part XIX — Bootstrap Logger Errors

## 172. LOGGING_BOOTSTRAP_NOT_AVAILABLE

```text
category: BOOTSTRAP
scope: BOOTSTRAP_LOGGER
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 173. LOGGING_BOOTSTRAP_WRITE_FAILED

```text
category: BOOTSTRAP
scope: BOOTSTRAP_LOGGER
severity: ERROR
retryClass: TRANSIENT
recoverability: APPLICATION_RESTART
```

---

## 174. LOGGING_BOOTSTRAP_HANDOFF_FAILED

```text
category: BOOTSTRAP
scope: BOOTSTRAP_LOGGER
severity: WARNING or ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

The normal pipeline may still start.

---

## 175. LOGGING_BOOTSTRAP_RECORD_UNSAFE

```text
category: SAFETY
scope: BOOTSTRAP_LOGGER
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

# Part XX — Emergency Logger Errors

## 176. LOGGING_EMERGENCY_NOT_AVAILABLE

```text
category: EMERGENCY
scope: EMERGENCY_LOGGER
severity: CRITICAL
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 177. LOGGING_EMERGENCY_RECORD_UNSAFE

```text
category: SAFETY
scope: EMERGENCY_LOGGER
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 178. LOGGING_EMERGENCY_WRITE_TIMED_OUT

```text
category: EMERGENCY
scope: EMERGENCY_LOGGER
severity: CRITICAL
retryClass: NEVER
recoverability: NONE
```

---

## 179. LOGGING_EMERGENCY_WRITE_FAILED

```text
category: EMERGENCY
scope: EMERGENCY_LOGGER
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 180. LOGGING_EMERGENCY_PATH_DEGRADED

```text
category: EMERGENCY
scope: EMERGENCY_LOGGER
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

# Part XXI — Recursion and Self-Reporting Errors

## 181. LOGGING_RECURSION_DETECTED

```text
category: RECURSION
scope: LOGGING_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 182. LOGGING_SELF_REPORTING_RECURSION_BLOCKED

```text
category: RECURSION
scope: LOGGING_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 183. LOGGING_EVENT_BUS_LOOP_BLOCKED

A Logging ↔ Event Bus reporting loop was detected.

```text
category: RECURSION
scope: LOGGING_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 184. LOGGING_SELF_EVENT_PUBLICATION_FAILED

```text
category: RECURSION
scope: LOGGING_INSTANCE
severity: WARNING or CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

Severity depends on whether a critical fact could not be reported.

---

# Part XXII — Lifecycle and Internal Errors

## 185. LOGGING_NOT_RUNNING

```text
category: LIFECYCLE
scope: LOGGING_INSTANCE
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 186. LOGGING_QUIESCING

```text
category: LIFECYCLE
scope: LOGGING_INSTANCE
severity: NOTICE
retryClass: NEVER for low-priority record
recoverability: NONE
```

---

## 187. LOGGING_SHUTDOWN_IN_PROGRESS

```text
category: LIFECYCLE
scope: LOGGING_INSTANCE
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 188. LOGGING_SHUTDOWN_TIMED_OUT

```text
category: LIFECYCLE
scope: LOGGING_INSTANCE
severity: WARNING or ERROR
retryClass: NEVER
recoverability: NONE
```

---

## 189. LOGGING_INVALID_STATE_TRANSITION

```text
category: INTERNAL
scope: LOGGING_INSTANCE
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 190. LOGGING_STATE_VERSION_CONFLICT

```text
category: CONCURRENCY
scope: LOGGING_INSTANCE
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC
```

---

## 191. LOGGING_BATCH_TERMINAL_STATE_CONFLICT

```text
category: CONCURRENCY
scope: LOG_BATCH
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

Only one terminal result may win.

---

## 192. LOGGING_FATAL_SAFETY_INVARIANT_BROKEN

```text
category: INTERNAL
scope: LOGGING_INSTANCE
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

Use only when Logging cannot enforce redaction, classification, boundedness, or safe routing.

---

# Part XXIII — Warnings

## 193. Warning Model

```text
LoggingWarning {
    warningId
    code
    scope
    safeMessage
    recoveryActions[]
    metadata
}
```

Warnings are bounded, non-recursive, and contain no original unsafe data.

---

## 194. LOGGING_WARNING_BUFFER_PRESSURE

Buffer utilization is high but safe admission still works.

---

## 195. LOGGING_WARNING_LOW_SEVERITY_DROPPED

Low-value records were dropped under policy.

---

## 196. LOGGING_WARNING_RECORD_SAMPLED

A record was sampled out.

---

## 197. LOGGING_WARNING_RECORD_SUPPRESSED

Repeated identical records were suppressed.

---

## 198. LOGGING_WARNING_SINK_SLOW

A sink remains available but exceeds latency thresholds.

---

## 199. LOGGING_WARNING_SINK_DEGRADED

A sink is usable with limitations.

---

## 200. LOGGING_WARNING_OPTIONAL_SINK_UNAVAILABLE

An optional sink is unavailable while approved sinks remain.

---

## 201. LOGGING_WARNING_PARTIAL_WRITE

Some records were written, some failed.

---

## 202. LOGGING_WARNING_ROTATION_PARTIAL

Rotation completed its safety-critical switch but cleanup remains.

---

## 203. LOGGING_WARNING_RETENTION_PARTIAL

Some eligible files could not be removed.

---

## 204. LOGGING_WARNING_COMPRESSION_PARTIAL

A verified archive exists but source cleanup remains.

---

## 205. LOGGING_WARNING_FLUSH_PARTIAL

Some sinks did not flush before the deadline.

---

## 206. LOGGING_WARNING_EXPORT_PARTIAL

A safe bundle was produced with omitted sections.

---

## 207. LOGGING_WARNING_BOOTSTRAP_HANDOFF_PARTIAL

Some bootstrap records were excluded or could not transfer.

---

# Part XXIV — Retry and Recovery Rules

## 208. Errors Do Not Retry Themselves

```text
Error normalized
    ↓
Current lifecycle, safety, and certainty checked
    ↓
Caller or recovery coordinator evaluates
    ↓
New write / flush / rotation / export attempt
```

---

## 209. Safe Record Retry

Potentially safe when:

- the previous admission definitely failed before acceptance;
- the record remains safe and immutable;
- the record ID is preserved or duplicate tolerance is defined;
- no sink may already have written it.

Examples:

- buffer capacity rejection;
- Logging not yet running;
- transient sink routing failure before batch creation.

---

## 210. Unsafe Blind Retry

Do not blindly retry when:

- sink write outcome is uncertain;
- write timed out and physical I/O may continue;
- audit persistence is uncertain;
- rotation activation outcome is uncertain;
- export destination may contain a partial bundle.

These require reconciliation or cleanup.

---

## 211. Sink Retry

Sink retry is allowed only when:

- bounded;
- write operation is idempotent or duplicate-safe;
- classification is preserved;
- the sink remains authorized;
- shutdown deadline permits it.

---

## 212. Backpressure Recovery

Recommended order:

```text
filter low severity
    ↓
sample repetitive records
    ↓
suppress duplicates
    ↓
drop TRACE / DEBUG
    ↓
drop INFO where policy allows
    ↓
use bounded producer wait
    ↓
use critical reserve
    ↓
use emergency path
```

---

## 213. Rotation Recovery

If rotation is uncertain:

```text
pause normal file writes
    ↓
use approved emergency path
    ↓
inspect file authority
    ↓
select one active file
    ↓
seal or quarantine others
    ↓
resume normal writes
```

---

# Part XXV — State Transition Implications

## 214. Safety Failure

```text
Record → REJECTED_UNSAFE or FAILED_SAFE
No buffer admission
Restricted security event emitted
```

---

## 215. Buffer Capacity Failure

```text
Admission → REJECTED_CAPACITY / TIMED_OUT
Buffer → BACKPRESSURED
Logging remains RUNNING or DEGRADED
```

Critical records may use reserve or emergency path.

---

## 216. Mandatory Sink Failure

```text
Sink → UNAVAILABLE / FAILED
Logging → DEGRADED or FAILED
Restricted or audit records may be blocked
```

---

## 217. Write Timeout

```text
Batch → TIMED_OUT
Physical write may continue
Late completion non-authoritative
Reconciliation may be required
```

---

## 218. Rotation Uncertainty

```text
Rotation → UNCERTAIN
Sink → DEGRADED / UNAVAILABLE
Normal file writes pause
Emergency path may activate
```

---

## 219. Export Safety Failure

```text
Export → BLOCKED_UNSAFE
Partial bundle removed
No bundle reference returned
```

---

## 220. Audit Sink Failure

```text
AuditWrite → AUDIT_SINK_UNAVAILABLE / FAILED
Owning action follows audit failure policy
```

---

## 221. Shutdown Timeout

```text
Flush → TIMED_OUT / PARTIALLY_FLUSHED
Remaining batches → CANCELED / ABANDONED
Logging continues to STOPPING
```

---

# Part XXVI — Cross-Module Normalization

## 222. Producer Mapping

Producers may receive:

```text
RECORD_INVALID
RECORD_SIZE_EXCEEDED
UNSAFE_PROPERTY
SECRET_DATA_BLOCKED
RAW_USER_CONTENT_BLOCKED
BUFFER_CAPACITY_EXCEEDED
LOGGING_NOT_RUNNING
LOGGING_QUIESCING
```

They must not receive raw sink or file-system exceptions.

---

## 223. Event Bus Mapping

Event Bus may interpret:

```text
LOGGING_RECURSION_DETECTED
    → guarded self-reporting failure

LOGGING_MANDATORY_SINK_UNAVAILABLE
    → infrastructure degradation

LOGGING_SECRET_DATA_BLOCKED
    → restricted security fact

LOGGING_SHUTDOWN_TIMED_OUT
    → bounded shutdown warning
```

---

## 224. Secret Management Mapping

Secret Management may receive safe indicators that:

- a secret logging attempt was blocked;
- redaction failed safe;
- restricted sink is unavailable.

Logging never returns or requests the secret value.

---

## 225. Runtime Mapping

Runtime may treat:

```text
BUFFER_BACKPRESSURED
    → observability pressure only

LOGGING_FAILED
    → application infrastructure degradation

AUDIT_MANDATORY_ACTION_BLOCKED
    → administrative action cannot proceed
```

Logging errors do not imply Runtime work failure unless the workflow explicitly requires logging/audit persistence.

---

## 226. Presentation Mapping

Potential user-facing levels:

```text
optional sink unavailable
    → usually hidden

diagnostics export failed
    → inline error

logging failed
    → application diagnostics degraded

mandatory audit unavailable
    → administrative action blocked
```

---

# Part XXVII — Logging and Observability

## 227. Error Logging Policy

Logging errors must use a guarded non-recursive path.

Allowed safe fields:

```text
errorCode
category
severity
recordId
bufferId
sinkId
batchId
logicalFileId
rotationId
flushId
exportId
auditRecordId
policyRevision
correlationId
```

Prohibited:

```text
original message
rendered message
property values
raw exception
stack trace
user content
secret material
raw file path
```

---

## 228. Metrics

Recommended metrics:

```text
logging_errors_total
logging_warnings_total
logging_records_blocked_total
logging_records_dropped_total
logging_buffer_errors_total
logging_sink_errors_total
logging_write_errors_total
logging_rotation_errors_total
logging_flush_errors_total
logging_export_errors_total
logging_audit_errors_total
logging_emergency_errors_total
logging_fatal_total
```

Safe labels:

```text
code
category
scope
severity
sinkType
bufferClass
```

Avoid IDs and file references as metric labels.

---

## 229. Tracing

Trace annotations may include:

- normalized error code;
- pipeline stage;
- sink type;
- buffer class;
- retry class;
- timeout duration;
- outcome.

No original log content is allowed.

---

# Part XXVIII — Testing Requirements

## 230. Record Tests

- missing fields;
- invalid category;
- invalid severity;
- oversized record;
- unsupported property type;
- reserved property override;
- mutable value.

---

## 231. Safety Tests

- secret value;
- password;
- token;
- authorization header;
- private key;
- raw user content;
- raw provider response;
- unsafe path;
- unsafe URI;
- classification downgrade;
- redaction failure.

---

## 232. Scope Tests

- unsafe scope property;
- excessive depth;
- async leak;
- repeated dispose;
- scope merge precedence.

---

## 233. Buffer Tests

- capacity exceeded;
- backpressure;
- critical reserve;
- reserve exhaustion;
- admission timeout;
- low-severity drop;
- buffer invariant failure.

---

## 234. Sink Tests

- duplicate sink ID;
- unsupported sink;
- classification mismatch;
- optional sink unavailable;
- mandatory sink unavailable;
- unsafe fallback;
- recovery failure.

---

## 235. Write Tests

- partial write;
- timeout;
- uncertain outcome;
- abandonment;
- late completion;
- permission denied;
- disk full.

---

## 236. File and Rotation Tests

- active file missing;
- multiple active files;
- create failure;
- flush failure;
- finalize failure;
- new file create failure;
- activation uncertainty;
- reconciliation failure;
- path escape.

---

## 237. Retention and Compression Tests

- unknown file skipped;
- delete failure;
- active file preserved;
- compression verify failure;
- source preserved;
- source deletion failure.

---

## 238. Export Tests

- access denied;
- unsafe destination;
- raw file copy blocked;
- second redaction failure;
- full-bundle inspection failure;
- partial cleanup failure;
- timeout.

---

## 239. Audit Tests

- invalid audit record;
- unsafe data;
- sink unavailable;
- timeout;
- uncertain persistence;
- reconciliation failure;
- owning action blocked.

---

## 240. Recursion Tests

- sink failure while reporting sink failure;
- Event Bus ↔ Logging loop;
- emergency logger failure;
- self-event publication failure.

---

# Part XXIX — MVP Error Boundary

## 241. Required MVP Codes

The MVP should implement at least:

```text
LOGGING_RECORD_INVALID
LOGGING_MESSAGE_TEMPLATE_UNSAFE
LOGGING_RECORD_SIZE_EXCEEDED
LOGGING_RECORD_UNSUPPORTED_VALUE_TYPE
LOGGING_CONTEXT_VALUE_UNSAFE
LOGGING_SCOPE_CONTEXT_LEAK_DETECTED

LOGGING_CLASSIFICATION_DOWNGRADE_BLOCKED
LOGGING_SECRET_DATA_BLOCKED
LOGGING_AUTHORIZATION_HEADER_BLOCKED
LOGGING_PRIVATE_KEY_BLOCKED
LOGGING_RAW_USER_CONTENT_BLOCKED
LOGGING_RAW_PROVIDER_PAYLOAD_BLOCKED

LOGGING_SAFETY_INSPECTION_FAILED_SAFE
LOGGING_REDACTION_FAILED_SAFE
LOGGING_REDACTION_POLICY_INVALID
LOGGING_SAFETY_CLASSIFIER_UNAVAILABLE

LOGGING_EXCEPTION_NORMALIZATION_FAILED_SAFE
LOGGING_EXCEPTION_MESSAGE_UNSAFE

LOGGING_POLICY_INVALID
LOGGING_POLICY_UNBOUNDED_RETENTION
LOGGING_POLICY_UNSAFE_FALLBACK
LOGGING_CONFIGURATION_INVALID

LOGGING_BUFFER_BACKPRESSURED
LOGGING_BUFFER_CAPACITY_EXCEEDED
LOGGING_BUFFER_ADMISSION_TIMED_OUT
LOGGING_CRITICAL_RESERVE_EXHAUSTED
LOGGING_BUFFER_INVARIANT_BROKEN

LOGGING_SINK_CONFIGURATION_INVALID
LOGGING_SINK_CLASSIFICATION_MISMATCH
LOGGING_NO_ELIGIBLE_SINK
LOGGING_MANDATORY_SINK_UNAVAILABLE
LOGGING_RESTRICTED_SINK_UNAVAILABLE
LOGGING_UNSAFE_FALLBACK_BLOCKED

LOGGING_FORMATTER_FAILED_SAFE
LOGGING_SERIALIZATION_FAILED_SAFE

LOGGING_SINK_UNAVAILABLE
LOGGING_SINK_PERMISSION_DENIED
LOGGING_SINK_DISK_FULL
LOGGING_SINK_WRITE_FAILED
LOGGING_SINK_PARTIAL_WRITE
LOGGING_SINK_WRITE_TIMED_OUT
LOGGING_SINK_WRITE_OUTCOME_UNCERTAIN
LOGGING_SINK_WRITE_ABANDONED

LOGGING_FILE_CREATE_FAILED
LOGGING_FILE_PERMISSION_DENIED
LOGGING_FILE_CORRUPTED
LOGGING_ACTIVE_FILE_MISSING
LOGGING_MULTIPLE_ACTIVE_FILES

LOGGING_ROTATION_NEW_FILE_CREATE_FAILED
LOGGING_ROTATION_ACTIVATION_FAILED
LOGGING_ROTATION_OUTCOME_UNCERTAIN
LOGGING_ROTATION_RECONCILIATION_FAILED
LOGGING_ROTATION_PARTIALLY_COMPLETED

LOGGING_RETENTION_DELETE_FAILED
LOGGING_RETENTION_PARTIALLY_COMPLETED
LOGGING_RETENTION_ACTIVE_FILE_PROTECTION_FAILED

LOGGING_FLUSH_PARTIALLY_COMPLETED
LOGGING_FLUSH_TIMED_OUT

LOGGING_EXPORT_ACCESS_DENIED
LOGGING_EXPORT_DESTINATION_UNSAFE
LOGGING_EXPORT_RAW_FILE_COPY_BLOCKED
LOGGING_EXPORT_REDACTION_FAILED_SAFE
LOGGING_EXPORT_BUNDLE_INSPECTION_FAILED_SAFE
LOGGING_EXPORT_PARTIAL_BUNDLE_CLEANUP_FAILED

LOGGING_AUDIT_RECORD_UNSAFE
LOGGING_AUDIT_SINK_UNAVAILABLE
LOGGING_AUDIT_WRITE_FAILED
LOGGING_AUDIT_WRITE_OUTCOME_UNCERTAIN
LOGGING_AUDIT_MANDATORY_ACTION_BLOCKED

LOGGING_BOOTSTRAP_HANDOFF_FAILED
LOGGING_EMERGENCY_NOT_AVAILABLE
LOGGING_EMERGENCY_WRITE_FAILED

LOGGING_RECURSION_DETECTED
LOGGING_EVENT_BUS_LOOP_BLOCKED
LOGGING_INVALID_STATE_TRANSITION
LOGGING_BATCH_TERMINAL_STATE_CONFLICT
LOGGING_FATAL_SAFETY_INVARIANT_BROKEN
```

---

## 242. Required MVP Warnings

```text
LOGGING_WARNING_BUFFER_PRESSURE
LOGGING_WARNING_LOW_SEVERITY_DROPPED
LOGGING_WARNING_RECORD_SAMPLED
LOGGING_WARNING_RECORD_SUPPRESSED
LOGGING_WARNING_SINK_SLOW
LOGGING_WARNING_SINK_DEGRADED
LOGGING_WARNING_OPTIONAL_SINK_UNAVAILABLE
LOGGING_WARNING_PARTIAL_WRITE
LOGGING_WARNING_ROTATION_PARTIAL
LOGGING_WARNING_RETENTION_PARTIAL
LOGGING_WARNING_FLUSH_PARTIAL
LOGGING_WARNING_EXPORT_PARTIAL
```

---

# Part XXX — Decisions

## 243. Decisions

### Decision 1 — Errors contain no original log content

No message, rendered text, property values, exception text, or stack trace.

### Decision 2 — Record rejection differs from sink failure

Admission and persistence remain separate.

### Decision 3 — Safety errors fail closed

Unsafe records are blocked.

### Decision 4 — Restricted-sink failure is critical

No fallback to weaker sinks.

### Decision 5 — Timeout is terminal

Late writes cannot overwrite timeout or abandonment.

### Decision 6 — Uncertain write requires reconciliation

Blind retry may duplicate records.

### Decision 7 — Rotation uncertainty is explicit

Normal file authority must be reconciled.

### Decision 8 — Export safety is independent

A safe stored record may still be blocked during bundle inspection.

### Decision 9 — Audit failure is separate

Ordinary log success cannot satisfy audit persistence.

### Decision 10 — Shutdown remains bounded

Flush failures do not wait forever.

### Decision 11 — Self-reporting is guarded

Logging cannot recursively log every internal failure.

### Decision 12 — Persistence guarantees are not overstated

Sink confirmation is limited to the sink's actual boundary.

---

# Part XXXI — Open Decisions

## 244. Severity Decisions

Still to finalize:

- when partial write is warning versus error;
- when flush timeout becomes critical;
- optional sink unavailability severity;
- bootstrap handoff failure severity;
- diagnostics export partial severity.

---

## 245. Retry Decisions

Still to finalize:

- default sink retry count;
- write-timeout reconciliation;
- file-create retry;
- rotation retry;
- retention cleanup cadence;
- export write retry;
- audit reconciliation timeout.

---

## 246. Safety Decisions

Still to finalize:

- false-positive workflow;
- high-entropy detection behavior;
- known-secret matching integration;
- path and URL masking rules;
- user-content opt-in diagnostics;
- safe snippet rules.

---

## 247. File and Audit Decisions

Still to finalize:

- corrupted-file quarantine;
- multiple-active-file recovery;
- partial bundle cleanup guarantees;
- audit duplicate handling;
- emergency audit store;
- audit fail-closed action list.

---

# Part XXXII — Related Documents

## 248. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/STATE_MACHINE.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/logging/MODULE.md
03-infrastructure/logging/CONTRACT.md
03-infrastructure/logging/STATES.md
03-infrastructure/logging/EVENTS.md

03-infrastructure/secret-management/ERRORS.md
03-infrastructure/event-bus/ERRORS.md
```

Future document:

```text
03-infrastructure/logging/README.md
```

---

## 249. Summary

Logging errors normalize record, scope, safety, redaction, policy, buffer, sink, write, file, rotation, retention, flush, export, audit, bootstrap, emergency, and recursion failures without exposing original log content.

The error flow is:

```text
Raw producer / sink / file failure
    ↓
Logging boundary catches failure
    ↓
Unsafe message, properties, exception, and paths removed
    ↓
Normalized LoggingError created
    ↓
Lifecycle and classification validated
    ↓
State transition applied where appropriate
    ↓
Guarded safe observability
```

The model preserves these distinctions:

```text
Record Rejection
    ≠ Buffer Pressure
    ≠ Sink Failure
    ≠ Write Timeout
    ≠ Rotation Uncertainty
    ≠ Export Failure
    ≠ Audit Failure
```

The architecture guarantees:

- errors never contain original log records;
- secret and user content remain absent;
- safety failures fail closed;
- admission and persistence remain separate;
- sink failures are isolated;
- restricted records never fall back to weaker sinks;
- timeout is terminal;
- late completion is non-authoritative;
- uncertain writes require reconciliation;
- rotation authority is explicit;
- diagnostics exports receive independent safety checks;
- audit persistence remains distinct;
- self-reporting is non-recursive;
- shutdown remains bounded;
- persistence guarantees are not overstated.

This document is the error source of truth for the Logging implementation and README.
