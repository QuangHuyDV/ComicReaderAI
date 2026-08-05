# Configuration Errors

> **Project:** CRAI
>
> **Layer:** Infrastructure
>
> **Module:** Configuration
>
> **Document:** Error Specification
>
> **Path:** `03-infrastructure/configuration/ERRORS.md`
>
> **Status:** Architecture Draft

---

# 1. Purpose

This document defines the complete error taxonomy for the Configuration Infrastructure module.

It specifies:

- error hierarchy
- error ownership
- recoverability
- retry behavior
- severity
- propagation
- diagnostics
- recovery policy

This document intentionally excludes:

- commands
- queries
- lifecycle states
- events

Those belong to:

```
CONTRACT.md

STATES.md

EVENTS.md
```

---

# 2. Error Philosophy

Configuration Infrastructure treats errors as first-class architectural concepts.

Errors are:

- explicit
- deterministic
- immutable
- categorized
- diagnosable
- replay-safe

Errors are never represented solely by:

```
Boolean

Null

Empty String

Magic Number
```

Every failure must have an explicit error object.

---

# 3. Error Goals

Configuration errors exist to:

- describe failures
- enable recovery
- support diagnostics
- support observability
- support auditing
- support retry decisions

Errors are not user interface messages.

---

# 4. Error Categories

Configuration owns the following error groups.

```
Source Errors

Reload Errors

Candidate Errors

Snapshot Errors

Revision Errors

Validation Errors

Compatibility Errors

Migration Errors

Override Errors

Consumer Errors

Serialization Errors

Security Errors

Internal Errors
```

---

# 5. Error Principles

Every error must satisfy:

✓ immutable

✓ deterministic

✓ typed

✓ categorized

✓ diagnosable

✓ transport neutral

✓ secret safe

---

# 6. Error Envelope

Every error conceptually uses the same envelope.

```
ConfigurationError

{

    errorId

    errorCode

    category

    severity

    retryPolicy

    recoverability

    timestamp

    context

}
```

The envelope is conceptual.

Transport protocols may serialize differently.

---

# 7. Error Identity

Every error owns:

```
ConfigurationErrorId
```

Properties

- unique
- immutable
- never reused

---

# 8. Error Code

Each error has a stable code.

Examples

```
CONFIG_SOURCE_NOT_FOUND

CONFIG_VALIDATION_FAILED

CONFIG_SCHEMA_MISMATCH
```

Codes never contain localized text.

---

# 9. Error Severity

Supported severities.

```
INFO

WARNING

ERROR

CRITICAL

FATAL
```

Severity influences recovery.

It does not automatically determine retry policy.

---

# 10. Recoverability

Every error declares recoverability.

```
RECOVERABLE

PARTIALLY_RECOVERABLE

NON_RECOVERABLE
```

Recoverability is architecture-level metadata.

---

# 11. Retry Policy

Supported retry policies.

```
NO_RETRY

IMMEDIATE

EXPONENTIAL_BACKOFF

MANUAL
```

Retry belongs to infrastructure.

Configuration only declares intent.

---

# 12. Error Ownership

Configuration owns:

- source failures
- validation failures
- publication failures
- snapshot failures

Consumer modules own:

- business failures
- runtime execution failures
- translation failures
- OCR failures

---

# 13. Secret Safety

Errors must never expose:

- API keys
- passwords
- tokens
- decrypted credentials
- private filesystem paths

Diagnostic references are permitted.

---

# 14. Error Context

Context is optional.

Conceptually

```
ErrorContext

{

    revision

    section

    sourceId

    operation

}
```

Context must never contain secrets.

---

# 15. Error Propagation

Configuration errors propagate only until:

- handled;
- transformed;
- logged.

Unhandled propagation across module boundaries is discouraged.

---

# Part I — Source Errors

# 16. Source Error Philosophy

Source failures occur before configuration becomes authoritative.

They never mutate published configuration.

---

# 17. CONFIG_SOURCE_NOT_FOUND

Meaning

A required configuration source cannot be located.

Severity

```
ERROR
```

Recoverability

```
RECOVERABLE
```

Retry

```
MANUAL
```

Typical causes

- missing file
- removed source
- incorrect path

---

# 18. CONFIG_SOURCE_ACCESS_DENIED

Meaning

Configuration source exists but cannot be accessed.

Examples

- permission denied
- locked file
- restricted resource

Severity

```
ERROR
```

---

# 19. CONFIG_SOURCE_PARSE_FAILED

Meaning

The source cannot be parsed.

Examples

```
Malformed YAML

Malformed JSON

Unsupported Encoding
```

Publication cannot continue.

---

# 20. CONFIG_SOURCE_DISABLED

Meaning

Attempted operation requires an enabled source.

The source is currently disabled.

---

# 21. CONFIG_SOURCE_ALREADY_REGISTERED

Meaning

A source with the same logical identity already exists.

Retry

```
NO_RETRY
```

The existing source should be reused.

---

# 22. CONFIG_SOURCE_UNKNOWN_TYPE

Meaning

Source type is unsupported.

Examples

```
Unknown Plugin

Unknown Remote Source

Unknown Format
```

---

# 23. CONFIG_SOURCE_PRECEDENCE_CONFLICT

Meaning

Source precedence cannot be resolved.

Examples

- invalid precedence
- unsupported merge policy

---

# 24. CONFIG_SOURCE_LOAD_TIMEOUT

Meaning

Loading exceeded configured timeout.

Retry

```
EXPONENTIAL_BACKOFF
```

if policy allows.

---

# 25. CONFIG_SOURCE_REMOVED

Meaning

Requested source no longer exists.

Historical revisions remain unaffected.

---

# Part II — Reload Errors

# 26. Reload Error Philosophy

Reload errors terminate one reload execution.

They never invalidate the currently active snapshot.

---

# 27. CONFIG_RELOAD_ALREADY_RUNNING

Meaning

A second reload request arrived while another reload is active.

Severity

```
WARNING
```

Recovery

Retry later.

---

# 28. CONFIG_RELOAD_CANCELLED

Meaning

Reload execution stopped intentionally.

Cancellation is not considered a failure.

Severity

```
INFO
```

---

# 29. CONFIG_RELOAD_DISCOVERY_FAILED

Meaning

Configuration source discovery failed.

Possible causes

- registry unavailable
- configuration directory inaccessible
- provider discovery failure

---

# 30. CONFIG_RELOAD_FAILED

Meaning

Reload terminated unsuccessfully.

The active configuration remains unchanged.

---

# 31. CONFIG_RELOAD_PUBLICATION_FAILED

Meaning

The candidate configuration was successfully constructed but could not become the active configuration.

Examples

- snapshot persistence failure
- revision allocation failure
- publication transaction failure

Severity

```
CRITICAL
```

Recoverability

```
RECOVERABLE
```

Retry

```
MANUAL
```

The previously active snapshot remains authoritative.

---

# 32. CONFIG_RELOAD_INCONSISTENT_STATE

Meaning

The reload pipeline entered an unexpected internal state.

Examples

- duplicated pipeline stage
- missing intermediate result
- corrupted execution context

Severity

```
CRITICAL
```

Recoverability

```
NON_RECOVERABLE
```

---

# 33. CONFIG_RELOAD_INTERRUPTED

Meaning

Reload execution was interrupted unexpectedly.

Possible causes

- application shutdown
- infrastructure failure
- unrecoverable dependency failure

A new reload must begin from the beginning.

---

# 34. Reload Error Summary

Reload errors guarantee:

✓ active configuration remains valid

✓ no partial publication

✓ deterministic recovery

✓ explicit failure stage

---

# Part III — Candidate Errors

# 35. Candidate Error Philosophy

Candidate errors occur before publication.

Candidate failures never modify active configuration.

---

# 36. CONFIG_CANDIDATE_NOT_FOUND

Meaning

Requested candidate does not exist.

Possible causes

- expired candidate
- invalid identifier
- already discarded

Severity

```
ERROR
```

Recoverability

```
RECOVERABLE
```

---

# 37. CONFIG_CANDIDATE_ALREADY_PUBLISHED

Meaning

The candidate has already become an active snapshot.

Publishing it again is forbidden.

Retry

```
NO_RETRY
```

---

# 38. CONFIG_CANDIDATE_ALREADY_REJECTED

Meaning

The candidate reached a terminal rejected state.

Rejected candidates cannot be recovered.

A new candidate must be created.

---

# 39. CONFIG_CANDIDATE_NOT_READY

Meaning

Publication was requested before candidate processing completed.

Examples

```
VALIDATING

MERGING

LOADING
```

Publication is only allowed from:

```
READY
```

---

# 40. CONFIG_CANDIDATE_BINDING_FAILED

Meaning

Typed configuration binding failed.

Examples

- invalid object structure
- incompatible schema
- unsupported property mapping

---

# 41. CONFIG_CANDIDATE_VALIDATION_FAILED

Meaning

Candidate failed structural validation.

Publication is forbidden.

Consumers are never notified.

---

# 42. CONFIG_CANDIDATE_COMPATIBILITY_FAILED

Meaning

Candidate is structurally valid but incompatible with the current application.

Migration may be required.

---

# 43. Candidate Error Summary

Candidate errors guarantee:

✓ no partial publication

✓ immutable failure

✓ deterministic retry path

---

# Part IV — Snapshot Errors

# 44. Snapshot Error Philosophy

Snapshot failures relate only to snapshot lifecycle.

Historical snapshots remain immutable.

---

# 45. CONFIG_SNAPSHOT_NOT_FOUND

Meaning

Requested snapshot cannot be located.

Possible causes

- invalid identifier
- retention expiration

Severity

```
ERROR
```

---

# 46. CONFIG_SNAPSHOT_ALREADY_ACTIVE

Meaning

Attempted activation of the already active snapshot.

No state change occurs.

---

# 47. CONFIG_SNAPSHOT_ALREADY_EXPIRED

Meaning

Historical retention already removed this snapshot.

Recovery is impossible.

---

# 48. CONFIG_SNAPSHOT_PUBLICATION_FAILED

Meaning

Snapshot creation succeeded.

Publication failed.

The active snapshot remains unchanged.

---

# 49. CONFIG_SNAPSHOT_CORRUPTED

Meaning

Snapshot integrity verification failed.

Severity

```
CRITICAL
```

Recoverability

```
NON_RECOVERABLE
```

---

# 50. Snapshot Error Summary

Snapshot guarantees:

✓ immutable history

✓ explicit publication failures

✓ deterministic recovery

---

# Part V — Revision Errors

# 51. Revision Error Philosophy

Revision failures affect configuration history only.

They never mutate existing revisions.

---

# 52. CONFIG_REVISION_NOT_FOUND

Meaning

Requested revision does not exist.

Possible causes

- invalid revision
- expired history

---

# 53. CONFIG_REVISION_ALREADY_ACTIVE

Meaning

Attempted activation of the current revision.

No operation is required.

---

# 54. CONFIG_REVISION_ALLOCATION_FAILED

Meaning

Configuration could not allocate a new revision.

Publication cannot continue.

---

# 55. CONFIG_REVISION_EXPIRED

Meaning

Historical revision has exceeded retention policy.

The revision is no longer available.

---

# 56. Revision Error Summary

Revision guarantees:

✓ append-only history

✓ immutable numbering

✓ deterministic allocation

---

# Part VI — Validation Errors

# 57. Validation Error Philosophy

Validation errors indicate structural problems.

They are independent from business semantics.

---

# 58. CONFIG_VALIDATION_FAILED

Meaning

One or more blocking validation violations were detected.

Publication is forbidden.

---

# 59. CONFIG_REQUIRED_FIELD_MISSING

Meaning

A required configuration field is absent.

Example

```
translation.defaultProvider
```

---

# 60. CONFIG_INVALID_FIELD_TYPE

Meaning

Actual value type differs from schema.

Example

```
Expected Integer

Received String
```

---

# 61. CONFIG_UNKNOWN_FIELD

Meaning

Configuration contains a field unknown to the schema.

Behavior depends on compatibility policy.

---

# 62. CONFIG_INVALID_ENUM_VALUE

Meaning

Configuration specifies an unsupported enum value.

Example

```
logLevel = EXTREME
```

---

# 63. CONFIG_DUPLICATE_KEY

Meaning

Duplicate configuration keys detected.

Canonical merge cannot continue.

---

# 64. CONFIG_SCHEMA_VERSION_UNSUPPORTED

Meaning

The declared schema version is unsupported.

Migration may be required.

---

# 65. CONFIG_CROSS_FIELD_VALIDATION_FAILED

Meaning

Relationships inside one section are invalid.

Example

```
minWorkers

>

maxWorkers
```

---

# 66. CONFIG_CROSS_SECTION_VALIDATION_FAILED

Meaning

Configuration sections conflict.

Examples

```
Runtime

↓

Disabled

Scheduler

↓

Enabled
```

---

# 67. Validation Error Summary

Validation errors guarantee:

✓ explicit schema violations

✓ deterministic evaluation

✓ publication prevention

---

# Part VII — Compatibility Errors

# 68. Compatibility Error Philosophy

Compatibility errors occur after successful validation.

They determine whether the configuration may be used.

---

# 69. CONFIG_INCOMPATIBLE_VERSION

Meaning

Configuration targets an unsupported application version.

---

# 70. CONFIG_MIGRATION_REQUIRED

Meaning

Configuration requires migration before publication.

This is not a validation failure.

Migration is the expected recovery path.

---

# 71. CONFIG_INCOMPATIBLE_SCHEMA

Meaning

The declared schema version cannot be interpreted by the running Configuration Infrastructure.

Examples

- unsupported major schema version;
- removed schema generation;
- incompatible contract evolution.

Severity

```
ERROR
```

Recoverability

```
PARTIALLY_RECOVERABLE
```

Recommended recovery

```
Migration
```

---

# 72. CONFIG_DEPRECATED_FIELD

Meaning

Configuration contains deprecated fields.

Publication may still proceed.

Severity

```
WARNING
```

Deprecated fields should eventually be removed through migration.

---

# 73. CONFIG_REMOVED_FIELD

Meaning

Configuration references a field that no longer exists.

Publication is normally rejected.

---

# 74. CONFIG_UNSUPPORTED_PROFILE

Meaning

Requested configuration profile is unknown.

Examples

```
production-eu

↓

Not Registered
```

---

# 75. CONFIG_MODULE_VERSION_CONFLICT

Meaning

One or more consumer modules do not support the published configuration version.

Compatibility evaluation terminates.

---

# 76. Compatibility Error Summary

Compatibility errors guarantee:

✓ explicit compatibility failures

✓ deterministic evaluation

✓ migration-aware recovery

✓ immutable diagnostics

---

# Part VIII — Migration Errors

# 77. Migration Error Philosophy

Migration transforms configuration.

Migration never mutates the original configuration.

---

# 78. CONFIG_MIGRATION_NOT_FOUND

Meaning

Requested migration definition does not exist.

Possible causes

- unsupported migration path;
- missing migration package.

Recovery

```
MANUAL
```

---

# 79. CONFIG_MIGRATION_FAILED

Meaning

Migration execution terminated unsuccessfully.

The original configuration remains unchanged.

---

# 80. CONFIG_MIGRATION_CANCELLED

Meaning

Migration stopped intentionally.

Examples

- administrator cancelled migration;
- application shutdown;
- newer migration superseded current request.

Cancellation is not considered corruption.

---

# 81. CONFIG_MIGRATION_OUTPUT_INVALID

Meaning

Migration completed but produced invalid configuration.

Validation failed.

Publication is impossible.

---

# 82. CONFIG_MIGRATION_LOOP_DETECTED

Meaning

Migration dependency graph contains a cycle.

Example

```
V2

↓

V3

↓

V2
```

Migration execution is aborted.

---

# 83. Migration Error Summary

Migration errors guarantee:

✓ immutable source configuration

✓ deterministic failure

✓ explicit recovery path

---

# Part IX — Override Errors

# 84. Override Error Philosophy

Override failures affect temporary effective configuration only.

Historical configuration remains unchanged.

---

# 85. CONFIG_OVERRIDE_NOT_FOUND

Meaning

Requested override does not exist.

Possible causes

- removed override;
- expired override;
- invalid identifier.

---

# 86. CONFIG_OVERRIDE_ALREADY_ACTIVE

Meaning

The override is already active.

Repeated activation has no effect.

---

# 87. CONFIG_OVERRIDE_ALREADY_EXPIRED

Meaning

Override lifetime has already ended.

Expired overrides cannot become active again.

---

# 88. CONFIG_OVERRIDE_SCOPE_INVALID

Meaning

Requested override scope is unsupported.

Examples

```
GLOBAL_CLUSTER
```

when only

```
APPLICATION

MODULE

SESSION

REQUEST
```

are supported.

---

# 89. CONFIG_OVERRIDE_VALIDATION_FAILED

Meaning

Override violates schema or compatibility requirements.

Override activation is rejected.

---

# 90. CONFIG_OVERRIDE_REMOVAL_FAILED

Meaning

Override could not be removed.

Current effective configuration remains unchanged.

---

# 91. Override Error Summary

Override errors guarantee:

✓ explicit scope validation

✓ deterministic precedence

✓ immutable override history

---

# Part X — Consumer Errors

# 92. Consumer Error Philosophy

Configuration tracks consumer adoption.

Consumer modules remain responsible for business behavior.

---

# 93. CONFIG_CONSUMER_NOT_FOUND

Meaning

Requested consumer identifier is unknown.

Possible causes

- removed module;
- invalid identifier.

---

# 94. CONFIG_CONSUMER_ALREADY_ACCEPTED

Meaning

The consumer has already accepted the revision.

Repeated acceptance is ignored.

---

# 95. CONFIG_CONSUMER_REJECTED

Meaning

The consumer explicitly rejected configuration adoption.

Configuration publication remains valid.

---

# 96. CONFIG_CONSUMER_TIMEOUT

Meaning

The consumer did not respond within the expected interval.

Acceptance remains:

```
PENDING
```

---

# 97. CONFIG_CONSUMER_RESTART_REQUIRED

Meaning

The consumer requires restart before adoption.

The configuration itself remains valid.

---

# 98. Consumer Error Summary

Consumer errors guarantee:

✓ independent consumer state

✓ explicit adoption failures

✓ publication isolation

---

# Part XI — Serialization Errors

# 99. Serialization Error Philosophy

Serialization failures affect transport only.

They never invalidate authoritative configuration.

---

# 100. CONFIG_SERIALIZATION_FAILED

Meaning

Configuration could not be serialized.

Possible causes

- unsupported serializer;
- invalid output encoding.

---

# 101. CONFIG_DESERIALIZATION_FAILED

Meaning

Serialized configuration cannot be reconstructed.

Input data is invalid or corrupted.

---

# 102. CONFIG_UNSUPPORTED_FORMAT

Meaning

Requested serialization format is unsupported.

Examples

```
BinaryFormatX
```

---

# 103. CONFIG_HASH_MISMATCH

Meaning

Configuration fingerprint does not match expected value.

Possible causes

- corruption;
- incomplete serialization.

---

# 104. Serialization Error Summary

Serialization errors guarantee:

✓ immutable authoritative state

✓ deterministic serialization failure

✓ explicit diagnostics

---

# Part XII — Security Errors

# 105. Security Error Philosophy

Security errors prevent unauthorized configuration access or mutation.

---

# 106. CONFIG_PERMISSION_DENIED

Meaning

Caller lacks required permission.

Examples

- reload denied;
- rollback denied;
- override denied.

Severity

```
ERROR
```

---

# 107. CONFIG_UNAUTHORIZED

Meaning

Caller identity cannot be authenticated.

No mutation occurs.

---

# 108. CONFIG_SECRET_REFERENCE_INVALID

Meaning

Referenced credential cannot be resolved.

Configuration remains valid.

Consumers may fail later.

---

# 109. CONFIG_SECRET_DISCLOSURE_ATTEMPT

Meaning

A request attempted to expose protected secret material.

Severity

```
CRITICAL
```

Raw secret values must never be returned.

---

# 110. CONFIG_AUDIT_FAILURE

Meaning

Required audit entry could not be recorded.

Administrative policy determines whether the command should continue.

---

# 111. Security Error Summary

Security errors guarantee:

✓ no secret disclosure

✓ explicit authorization failure

✓ immutable audit intent

---

# Part XIII — Internal Errors

# 112. Internal Error Philosophy

Internal errors indicate defects or unexpected infrastructure failures.

They should be rare.

---

# 113. CONFIG_INTERNAL_ERROR

Meaning

Unexpected internal failure.

No additional assumptions should be made.

---

# 114. CONFIG_STATE_CORRUPTION

Meaning

Internal state violates architectural invariants.

Severity

```
FATAL
```

Normal recovery is impossible.

---

# 115. CONFIG_ASSERTION_FAILED

Meaning

An internal architectural assumption has been violated.

This indicates a programming defect.

---

# 116. CONFIG_EVENT_REPLAY_FAILED

Meaning

Historical event replay could not reconstruct expected state.

Possible causes

- corrupted event history;
- incompatible event version.

---

# 117. CONFIG_UNKNOWN_ERROR

Meaning

Unexpected failure with no specific classification.

Should only be used as a temporary fallback.

---

# 118. Internal Error Summary

Internal errors guarantee:

✓ explicit infrastructure failures

✓ deterministic classification

✓ implementation diagnostics

---

# Part XIV — Error Recovery Matrix

# 119. Recovery Strategy

Errors are classified by preferred recovery.

| Category | Typical Recovery |
|----------|------------------|
| Source | Fix source, reload |
| Validation | Correct configuration |
| Compatibility | Migrate configuration |
| Migration | Correct migration path |
| Override | Remove or recreate override |
| Consumer | Consumer-specific recovery |
| Serialization | Retry serialization |
| Security | Correct authorization |
| Internal | Investigation and fix |

---

# 120. Retry Matrix

| Error Type | Retry |
|------------|-------|
| Missing Source | Manual |
| Timeout | Exponential Backoff |
| Validation Failure | No Retry |
| Compatibility Failure | No Retry |
| Migration Failure | Manual |
| Serialization Failure | Immediate or Manual |
| Permission Denied | No Retry |
| Internal Error | Manual |

---

# 121. Recovery Invariants

Recovery guarantees:

✓ published configuration never becomes partially invalid

✓ historical revisions remain immutable

✓ failed operations never silently succeed

✓ recovery creates new execution

---

# Part XV — Global Error Invariants

# 122. Invariant 1

Every error has exactly one category.

---

# 123. Invariant 2

Every error has exactly one severity.

---

# 124. Invariant 3

Every error has one recoverability classification.

---

# 125. Invariant 4

Every error has one retry policy.

---

# 126. Invariant 5

Errors never expose raw secrets.

---

# 127. Invariant 6

Published snapshots remain immutable after errors.

---

# 128. Invariant 7

Historical revisions are never modified during recovery.

---

# 129. Invariant 8

Errors are deterministic for identical failure conditions.

---

# 130. Invariant 9

Recovery never mutates historical error records.

---

# 131. Invariant 10

Errors describe failures.

They never contain recovery commands.

---

# Part XVI — Error Specification Summary

# 132. Error Categories Covered

This document specifies:

```
✓ Source Errors

✓ Reload Errors

✓ Candidate Errors

✓ Snapshot Errors

✓ Revision Errors

✓ Validation Errors

✓ Compatibility Errors

✓ Migration Errors

✓ Override Errors

✓ Consumer Errors

✓ Serialization Errors

✓ Security Errors

✓ Internal Errors
```

---

# 133. Architectural Guarantees

Configuration Errors guarantee:

✓ deterministic classification

✓ immutable error objects

✓ explicit recoverability

✓ retry awareness

✓ transport neutrality

✓ secret safety

✓ replay compatibility

✓ implementation independence

---

# 134. Relationship to Other Documents

The complete Configuration specification consists of:

```
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

Each document defines one independent aspect of the module architecture.

---

# 135. End of Error Specification

This document defines the complete error taxonomy for the Configuration Infrastructure module.

Every implementation must preserve:

- error semantics;
- recoverability rules;
- retry policies;
- security guarantees;
- architectural invariants;

regardless of implementation language, runtime, messaging technology, or storage backend.