# Secret Management Errors

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Secret Management  
> **Document:** Errors and Warnings  
> **Path:** `03-infrastructure/secret-management/ERRORS.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-05  
> **Source of Truth:**
>
> - `03-infrastructure/secret-management/MODULE.md`
> - `03-infrastructure/secret-management/CONTRACT.md`
> - `03-infrastructure/secret-management/STATES.md`
> - `03-infrastructure/secret-management/EVENTS.md`
> - `03-infrastructure/configuration/MODULE.md`
> - `03-infrastructure/configuration/CONTRACT.md`
> - `02-modules/provider-management/MODULE.md`
> - `02-modules/provider-management/CONTRACT.md`
> - `02-modules/provider-management/STATES.md`
> - `02-modules/provider-management/EVENTS.md`
> - `02-modules/provider-management/ERRORS.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RETRY_POLICY.md`
> - `docs/architecture/runtime/RESOURCE_LIFECYCLE.md`

---

## 1. Purpose

This document defines normalized errors and warnings owned by the Secret Management infrastructure module.

It specifies:

- the canonical error structure;
- error code naming;
- categories;
- scopes;
- severity;
- retryability;
- user-action requirements;
- recovery actions;
- reference errors;
- registration and replacement errors;
- access-policy errors;
- consumer-identity errors;
- backend errors;
- backend lock and user-presence errors;
- material-validation errors;
- lease errors;
- rotation errors;
- migration errors;
- revocation and removal errors;
- refresh errors;
- reconciliation errors;
- concurrency and persistence errors;
- serialization, redaction, and exposure errors;
- security violations;
- warnings and partial outcomes;
- cross-module normalization;
- state-transition implications;
- logging, metrics, tracing, and UI mapping.

This document does not define:

- raw platform exceptions;
- provider-native error payloads;
- HTTP status mappings;
- UI wording;
- database schemas;
- retry scheduling;
- state transition implementation;
- alert thresholds;
- encryption algorithms;
- secure-store SDK exception classes.

---

## 2. Error Design Goals

Secret Management error handling must:

1. prevent secret material from appearing in errors;
2. normalize platform and provider failures before they cross module boundaries;
3. distinguish caller rejection from infrastructure failure;
4. distinguish temporary unavailability from permanent invalidity;
5. distinguish cancellation from failure;
6. distinguish known failure from uncertain outcome;
7. distinguish revocation from deletion;
8. distinguish logical removal from physical erasure assurance;
9. preserve stable secret, revision, lease, backend, and operation identity;
10. support safe user action;
11. support deterministic recovery decisions;
12. avoid blind retry of non-idempotent operations;
13. prevent stale commands from overwriting newer revisions;
14. keep backend details internal;
15. preserve a last known good revision where possible;
16. make security violations fail-safe;
17. remain compatible with desktop OS secure stores and future external secret managers.

---

## 3. Error Versus Warning

An error prevents an operation from being accepted or completed safely.

A warning describes a usable but degraded, partial, or weaker-than-requested outcome.

```text
Error
    → operation rejected, failed, blocked, or cannot safely continue

Warning
    → operation completed or remains usable with bounded limitation
```

Examples:

```text
Secret reference does not exist
    → Error

Requested secure deletion is unsupported,
but logical deletion succeeded
    → Warning

Rotation activated new revision,
but old client cleanup is pending
    → Warning / partial completion

Attempted secret exposure was blocked
    → Security Error
```

---

## 4. Error Versus Lifecycle Outcome

These lifecycle outcomes are not automatically errors:

```text
Secret suspended
Secret revoked
Secret removed
Revision superseded
Revision expired
Lease released
Lease expired
Lease revoked
Backend locked
Operation canceled
Operation deferred
Operation uncertain
```

They may carry reason codes and operational consequences.

A lifecycle outcome becomes an error when:

- it violates the requested operation;
- it indicates unexpected infrastructure failure;
- it creates an unsafe or unrecoverable state;
- it violates a contract invariant;
- it requires user-visible remediation.

---

## 5. Error Versus Cancellation

Cancellation is expected control flow when intentionally requested.

```text
Canceled before irreversible action
    → no error required

Canceled after external action may have committed
    → uncertain outcome, not ordinary cancellation

Cleanup failed after cancellation
    → cleanup error
```

Cancellation must not be normalized as an internal failure unless the cancellation mechanism itself fails.

---

## 6. Error Versus Uncertain Outcome

An uncertain outcome means the system cannot determine whether an external or backend action committed.

Examples:

- provider rotated a key but the acknowledgement was lost;
- backend write timed out after possible commit;
- descriptor activation commit status is unknown;
- source deletion may have succeeded but response was lost.

Uncertain outcome requires:

```text
automaticRetryBlocked = true
reconciliationRequired = true
```

It is not equivalent to:

```text
FAILED
CANCELED
```

---

## 7. Error Ownership

Secret Management owns normalized errors concerning:

- secret references;
- secret identity;
- descriptors;
- revisions;
- access policies;
- consumer identity at the secret boundary;
- purpose authorization;
- secure backend registration and availability;
- backend lock and unlock;
- secret material validation;
- secret registration;
- replacement;
- rotation;
- migration;
- revocation;
- removal;
- refresh;
- validation;
- leases;
- redaction;
- serialization safety;
- exposure prevention;
- Secret Management persistence;
- Secret Management events;
- reconciliation;
- secure lifecycle invariants.

Secret Management does not own original errors concerning:

- Provider Management selection;
- provider capability semantics;
- Runtime scheduling;
- Translation semantics;
- Recognition semantics;
- Presentation rendering;
- Configuration parsing outside secret-reference validation;
- network-stack implementation outside approved adapters;
- operating-system UI wording.

Errors from other modules may be normalized only when they affect Secret Management contracts or state.

---

## 8. Error Ownership Matrix

| Concern | Owning module |
|---|---|
| Secret reference parsing | Secret Management |
| Secret existence and availability | Secret Management |
| Backend secure-store access | Secret Management |
| Credential provider selection | Provider Management |
| Provider authentication semantics | Provider adapter / Provider Management |
| Secret lease authority | Secret Management |
| Provider lease authority | Provider Management |
| Runtime work cancellation | Runtime |
| User prompt rendering | Presentation |
| Secret-entry transport | Trusted Presentation-to-host boundary |
| Configuration source precedence | Configuration |
| Secret material exposure prevention | Secret Management at secret boundary |
| General log pipeline failure | Logging infrastructure |
| Secret-safe logging validation | Secret Management / Logging boundary |

---

## 9. Canonical Error Model

```text
SecretManagementError {
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

    secretId?
    referenceId?
    revision?
    secretLeaseId?
    secretBackendId?
    operationId?
    rotationId?
    migrationId?
    validationId?

    consumerId?
    providerId?

    correlationId
    causationId?
    applicationInstanceId

    occurredAt

    cause?
    metadata
}
```

---

## 10. Safe Message

`safeMessage` must:

- avoid raw secret material;
- avoid authorization values;
- avoid sensitive aliases;
- avoid provider-native payloads;
- avoid platform-native exception dumps;
- remain suitable for logs and user-safe mapping.

`developerMessage` is optional and still must be secret-safe.

---

## 11. Error Cause

```text
SecretErrorCause {
    code
    category
    safeMessage
}
```

The public cause chain should be shallow.

Raw exception chains remain internal and must pass redaction inspection before restricted diagnostics.

---

## 12. Metadata

Allowed examples:

```text
requestedRevision
actualRevision
backendType
requestedLeaseDuration
maximumLeaseDuration
operationStage
validationMode
removalAssuranceRequested
removalAssuranceAchieved
retryCount
elapsedDuration
```

Prohibited examples:

```text
secretValue
apiKey
accessToken
refreshToken
password
privateKey
authorizationHeader
decryptedPayload
rawEnvironmentValue
rawProviderResponse
rawBackendEntry
```

---

## 13. Error Categories

Canonical categories:

```text
REFERENCE
DESCRIPTOR
REVISION
ACCESS_POLICY
CONSUMER_IDENTITY
PURPOSE
BACKEND
BACKEND_LOCK
USER_PRESENCE
MATERIAL
REGISTRATION
REPLACEMENT
ROTATION
MIGRATION
VALIDATION
LEASE
REFRESH
REVOCATION
REMOVAL
EXPORT
RECONCILIATION
CONCURRENCY
IDEMPOTENCY
PERSISTENCE
SERIALIZATION
REDACTION
SECURITY
EVENT_PUBLICATION
CLEANUP
CONFIGURATION_BOUNDARY
INTERNAL
```

---

## 14. Error Scopes

Canonical scopes:

```text
REFERENCE
SECRET
SECRET_REVISION
SECRET_LEASE
SECRET_BACKEND
SECRET_OPERATION
ROTATION
MIGRATION
VALIDATION
ACCESS_POLICY
CONSUMER
APPLICATION_INSTANCE
MODULE
PLATFORM
EXTERNAL_PROVIDER
```

---

## 15. Severity

```text
TRACE
NOTICE
WARNING
ERROR
CRITICAL
FATAL
```

### TRACE

Expected low-level rejection or diagnostic condition.

### NOTICE

Caller correction or ordinary unavailable condition.

### WARNING

Degraded or recoverable operational problem.

### ERROR

Operation failed or cannot continue.

### CRITICAL

Security boundary, invariant, or broad backend safety failure.

### FATAL

The application cannot maintain secret safety.

`FATAL` must be rare.

Examples:

- secret-safe serialization cannot be enforced;
- compromised backend continues returning material;
- state corruption makes active revision authority unknowable and cannot be isolated;
- redaction failure combined with mandatory diagnostic export path that cannot be blocked.

---

## 16. Retry Class

```text
NEVER
IMMEDIATE
TRANSIENT
AFTER_USER_ACTION
AFTER_CONFIGURATION_CHANGE
AFTER_UNLOCK
AFTER_REFRESH
AFTER_ROTATION
AFTER_RECONCILIATION
IDEMPOTENT_ONLY
UNKNOWN
```

Retry class is guidance.

The error does not schedule retry itself.

---

## 17. Recoverability

```text
AUTOMATIC
USER_ACTION
ADMIN_ACTION
CONFIGURATION_CHANGE
ROTATION_REQUIRED
MIGRATION_REQUIRED
RECONCILIATION_REQUIRED
APPLICATION_RESTART
NOT_RECOVERABLE
UNKNOWN
```

---

## 18. Canonical Recovery Actions

```text
RETRY
WAIT_AND_RETRY
UNLOCK_SECRET_STORE
REQUEST_USER_PRESENCE
REENTER_SECRET
SELECT_DIFFERENT_REFERENCE
SELECT_DIFFERENT_BACKEND
UPDATE_ACCESS_POLICY
USE_AUTHORIZED_CONSUMER
USE_ALLOWED_PURPOSE
REFRESH_CREDENTIAL
ROTATE_SECRET
REVOKE_SECRET
REMOVE_SECRET
RECREATE_SECRET
RESTART_APPLICATION
CHECK_PLATFORM_SECRET_SERVICE
CHECK_ENVIRONMENT_CONFIGURATION
RECONCILE_OPERATION
COMPLETE_MANUAL_RECOVERY
CONTACT_SUPPORT
NONE
```

Recovery actions are recommendations, not automatic commands.

---

## 19. Error Code Naming

Canonical format:

```text
SECRET_MANAGEMENT_<CONCERN>_<CONDITION>
```

Examples:

```text
SECRET_MANAGEMENT_REFERENCE_INVALID
SECRET_MANAGEMENT_BACKEND_LOCKED
SECRET_MANAGEMENT_LEASE_EXPIRED
SECRET_MANAGEMENT_ROTATION_OUTCOME_UNCERTAIN
```

Warnings use:

```text
SECRET_MANAGEMENT_WARNING_<CONDITION>
```

Security violations may use:

```text
SECRET_MANAGEMENT_SECURITY_<CONDITION>
```

---

## 20. Stability Rule

Once a code is used in:

- public/internal API responses;
- events;
- telemetry;
- audit records;
- persisted operation receipts;
- tests;

its semantic meaning must not change.

A new meaning requires a new code.

---

## 21. Unknown Error

Consumers must support:

```text
SECRET_MANAGEMENT_UNKNOWN_ERROR
```

```text
category: INTERNAL
scope: MODULE
severity: ERROR
retryClass: UNKNOWN
recoverability: UNKNOWN
```

Internal diagnostics should preserve a restricted correlation reference.

---

# Part I — Reference Errors

## 22. SECRET_MANAGEMENT_REFERENCE_INVALID

The reference syntax is malformed.

```text
category: REFERENCE
scope: REFERENCE
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

Examples:

- unsupported URI format;
- missing namespace;
- missing alias;
- embedded secret material;
- forbidden query parameter;
- invalid escape sequence.

Recovery:

```text
Correct the reference.
```

---

## 23. SECRET_MANAGEMENT_REFERENCE_SCHEME_UNSUPPORTED

The reference uses an unsupported scheme.

```text
category: REFERENCE
scope: REFERENCE
severity: NOTICE
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 24. SECRET_MANAGEMENT_REFERENCE_CONTAINS_SECRET_MATERIAL

The reference appears to contain raw secret material.

```text
category: SECURITY
scope: REFERENCE
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

Required behavior:

- reject the reference;
- block logging of the original input;
- emit a restricted exposure event;
- sanitize diagnostics.

---

## 25. SECRET_MANAGEMENT_REFERENCE_NOT_FOUND

The reference does not map to an active descriptor or approved external source.

```text
category: REFERENCE
scope: REFERENCE
severity: NOTICE
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: USER_ACTION or CONFIGURATION_CHANGE
```

Visibility may be hidden for unauthorized callers.

---

## 26. SECRET_MANAGEMENT_REFERENCE_HIDDEN_BY_POLICY

The caller is not permitted to learn whether the reference exists.

```text
category: ACCESS_POLICY
scope: REFERENCE
severity: NOTICE
retryClass: NEVER
recoverability: ADMIN_ACTION
```

The response must not confirm existence.

---

## 27. SECRET_MANAGEMENT_REFERENCE_KIND_MISMATCH

The referenced secret kind differs from the expected kind.

```text
category: REFERENCE
scope: SECRET
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 28. SECRET_MANAGEMENT_REFERENCE_PROVIDER_MISMATCH

The reference is not valid for the requested provider.

```text
category: REFERENCE
scope: SECRET
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 29. SECRET_MANAGEMENT_REFERENCE_REVISION_TOO_OLD

The active revision does not satisfy `minimumRevision`.

```text
category: REVISION
scope: SECRET_REVISION
severity: NOTICE
retryClass: AFTER_ROTATION
recoverability: ROTATION_REQUIRED
```

---

# Part II — Descriptor and Revision Errors

## 30. SECRET_MANAGEMENT_SECRET_NOT_ACTIVE

The descriptor exists but is not in a state permitting normal use.

```text
category: DESCRIPTOR
scope: SECRET
severity: NOTICE
retryClass: conditional
recoverability: USER_ACTION or ADMIN_ACTION
```

Possible states:

```text
REGISTERING
SUSPENDED
REVOKED
REMOVING
REMOVED
TOMBSTONED
```

---

## 31. SECRET_MANAGEMENT_SECRET_SUSPENDED

```text
category: DESCRIPTOR
scope: SECRET
severity: WARNING
retryClass: AFTER_USER_ACTION or AFTER_CONFIGURATION_CHANGE
recoverability: USER_ACTION or ADMIN_ACTION
```

---

## 32. SECRET_MANAGEMENT_SECRET_REVOKED

```text
category: REVOCATION
scope: SECRET
severity: ERROR
retryClass: NEVER for same revision
recoverability: ROTATION_REQUIRED or RECREATE_SECRET
```

---

## 33. SECRET_MANAGEMENT_SECRET_REMOVED

```text
category: REMOVAL
scope: SECRET
severity: NOTICE
retryClass: NEVER
recoverability: RECREATE_SECRET
```

---

## 34. SECRET_MANAGEMENT_ACTIVE_REVISION_MISSING

An active descriptor has no active revision.

```text
category: REVISION
scope: SECRET
severity: CRITICAL
retryClass: NEVER until recovery
recoverability: ADMIN_ACTION or RECONCILIATION_REQUIRED
```

This is an invariant violation.

Normal resolution must stop.

---

## 35. SECRET_MANAGEMENT_MULTIPLE_ACTIVE_REVISIONS

More than one revision appears active for new leases.

```text
category: REVISION
scope: SECRET
severity: CRITICAL
retryClass: NEVER
recoverability: RECONCILIATION_REQUIRED
```

No new lease may be granted until resolved.

---

## 36. SECRET_MANAGEMENT_REVISION_NOT_FOUND

```text
category: REVISION
scope: SECRET_REVISION
severity: NOTICE
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 37. SECRET_MANAGEMENT_REVISION_EXPIRED

```text
category: REVISION
scope: SECRET_REVISION
severity: ERROR
retryClass: AFTER_REFRESH or AFTER_ROTATION
recoverability: ROTATION_REQUIRED
```

---

## 38. SECRET_MANAGEMENT_REVISION_REVOKED

```text
category: REVOCATION
scope: SECRET_REVISION
severity: ERROR
retryClass: NEVER for same revision
recoverability: ROTATION_REQUIRED
```

---

## 39. SECRET_MANAGEMENT_REVISION_INVALID

```text
category: MATERIAL
scope: SECRET_REVISION
severity: ERROR
retryClass: AFTER_ROTATION
recoverability: ROTATION_REQUIRED
```

---

## 40. SECRET_MANAGEMENT_REVISION_DELETED

```text
category: REMOVAL
scope: SECRET_REVISION
severity: NOTICE
retryClass: NEVER
recoverability: RECREATE_SECRET
```

---

## 41. SECRET_MANAGEMENT_REVISION_ACTIVATION_CONFLICT

The expected active revision changed before activation.

```text
category: CONCURRENCY
scope: SECRET_REVISION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: RECONCILIATION_REQUIRED or RETRY
```

---

## 42. SECRET_MANAGEMENT_REVISION_ACTIVATION_FAILED

```text
category: REVISION
scope: SECRET_REVISION
severity: ERROR
retryClass: TRANSIENT if commit not attempted
recoverability: AUTOMATIC or ADMIN_ACTION
```

The previous active revision must remain authoritative when possible.

---

# Part III — Access Policy and Identity Errors

## 43. SECRET_MANAGEMENT_ACCESS_DENIED

The caller is not authorized for the requested operation.

```text
category: ACCESS_POLICY
scope: CONSUMER
severity: NOTICE
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 44. SECRET_MANAGEMENT_CONSUMER_IDENTITY_MISSING

```text
category: CONSUMER_IDENTITY
scope: CONSUMER
severity: ERROR
retryClass: NEVER
recoverability: USE_AUTHORIZED_CONSUMER
```

---

## 45. SECRET_MANAGEMENT_CONSUMER_IDENTITY_INVALID

The identity could not be trusted or verified.

```text
category: CONSUMER_IDENTITY
scope: CONSUMER
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 46. SECRET_MANAGEMENT_CONSUMER_MISMATCH

A consumer attempted to use another consumer's lease or handle.

```text
category: SECURITY
scope: SECRET_LEASE
severity: CRITICAL
retryClass: NEVER
recoverability: NONE
```

Required behavior:

- block use;
- revoke or isolate the lease;
- emit restricted security event;
- preserve safe audit metadata.

---

## 47. SECRET_MANAGEMENT_PURPOSE_MISSING

```text
category: PURPOSE
scope: SECRET_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

Generic unscoped material access is prohibited.

---

## 48. SECRET_MANAGEMENT_PURPOSE_NOT_ALLOWED

```text
category: PURPOSE
scope: SECRET_OPERATION
severity: ERROR
retryClass: NEVER
recoverability: UPDATE_ACCESS_POLICY or USE_ALLOWED_PURPOSE
```

---

## 49. SECRET_MANAGEMENT_SCOPE_NOT_ALLOWED

```text
category: ACCESS_POLICY
scope: SECRET
severity: ERROR
retryClass: NEVER
recoverability: UPDATE_ACCESS_POLICY
```

---

## 50. SECRET_MANAGEMENT_PROVIDER_NOT_ALLOWED

```text
category: ACCESS_POLICY
scope: SECRET
severity: ERROR
retryClass: NEVER
recoverability: UPDATE_ACCESS_POLICY or SELECT_DIFFERENT_REFERENCE
```

---

## 51. SECRET_MANAGEMENT_LEASE_DURATION_EXCEEDS_POLICY

```text
category: ACCESS_POLICY
scope: SECRET_LEASE
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

The caller may retry with a shorter duration.

---

## 52. SECRET_MANAGEMENT_USER_PRESENCE_REQUIRED

```text
category: USER_PRESENCE
scope: SECRET_OPERATION
severity: NOTICE
retryClass: AFTER_USER_ACTION
recoverability: USER_ACTION
```

Recovery:

```text
REQUEST_USER_PRESENCE
```

---

## 53. SECRET_MANAGEMENT_USER_PRESENCE_CANCELED

```text
category: USER_PRESENCE
scope: SECRET_OPERATION
severity: NOTICE
retryClass: AFTER_USER_ACTION
recoverability: USER_ACTION
```

This is usually not an infrastructure failure.

---

## 54. SECRET_MANAGEMENT_USER_PRESENCE_FAILED

```text
category: USER_PRESENCE
scope: PLATFORM
severity: ERROR
retryClass: TRANSIENT or AFTER_USER_ACTION
recoverability: USER_ACTION
```

---

# Part IV — Backend Errors

## 55. SECRET_MANAGEMENT_BACKEND_NOT_REGISTERED

```text
category: BACKEND
scope: SECRET_BACKEND
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 56. SECRET_MANAGEMENT_BACKEND_UNSUPPORTED

The platform or requested backend type is unsupported.

```text
category: BACKEND
scope: PLATFORM
severity: ERROR
retryClass: NEVER
recoverability: SELECT_DIFFERENT_BACKEND
```

---

## 57. SECRET_MANAGEMENT_BACKEND_INITIALIZATION_FAILED

```text
category: BACKEND
scope: SECRET_BACKEND
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC or APPLICATION_RESTART
```

---

## 58. SECRET_MANAGEMENT_BACKEND_LOCKED

```text
category: BACKEND_LOCK
scope: SECRET_BACKEND
severity: NOTICE
retryClass: AFTER_UNLOCK
recoverability: USER_ACTION
```

This must not be normalized as missing secret.

---

## 59. SECRET_MANAGEMENT_BACKEND_UNLOCK_FAILED

```text
category: BACKEND_LOCK
scope: SECRET_BACKEND
severity: ERROR
retryClass: AFTER_USER_ACTION
recoverability: USER_ACTION
```

---

## 60. SECRET_MANAGEMENT_BACKEND_UNAVAILABLE

```text
category: BACKEND
scope: SECRET_BACKEND
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC or CHECK_PLATFORM_SECRET_SERVICE
```

---

## 61. SECRET_MANAGEMENT_BACKEND_DEGRADED

```text
category: BACKEND
scope: SECRET_BACKEND
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

The operation may still complete with limitations.

---

## 62. SECRET_MANAGEMENT_BACKEND_PERMISSION_DENIED

```text
category: BACKEND
scope: PLATFORM
severity: ERROR
retryClass: AFTER_USER_ACTION or AFTER_CONFIGURATION_CHANGE
recoverability: USER_ACTION or ADMIN_ACTION
```

---

## 63. SECRET_MANAGEMENT_BACKEND_CAPABILITY_MISSING

The backend cannot satisfy a mandatory property.

```text
category: BACKEND
scope: SECRET_BACKEND
severity: ERROR
retryClass: NEVER
recoverability: SELECT_DIFFERENT_BACKEND
```

Examples:

- persistence unsupported;
- secret kind unsupported;
- required user presence unsupported;
- atomic replace unsupported;
- size too large.

---

## 64. SECRET_MANAGEMENT_BACKEND_VALUE_TOO_LARGE

```text
category: BACKEND
scope: SECRET_BACKEND
severity: ERROR
retryClass: NEVER
recoverability: SELECT_DIFFERENT_BACKEND
```

---

## 65. SECRET_MANAGEMENT_BACKEND_READ_FAILED

```text
category: BACKEND
scope: SECRET_BACKEND
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

Raw backend exception remains internal.

---

## 66. SECRET_MANAGEMENT_BACKEND_WRITE_FAILED

```text
category: BACKEND
scope: SECRET_BACKEND
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC or RECONCILIATION_REQUIRED
```

If commit may have occurred, use uncertain outcome.

---

## 67. SECRET_MANAGEMENT_BACKEND_DELETE_FAILED

```text
category: BACKEND
scope: SECRET_BACKEND
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC or ADMIN_ACTION
```

The descriptor remains blocked from use.

---

## 68. SECRET_MANAGEMENT_BACKEND_DELETE_OUTCOME_UNCERTAIN

```text
category: RECONCILIATION
scope: SECRET_BACKEND
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 69. SECRET_MANAGEMENT_BACKEND_CORRUPTED

```text
category: BACKEND
scope: SECRET_BACKEND
severity: CRITICAL
retryClass: NEVER
recoverability: MIGRATION_REQUIRED or ADMIN_ACTION
```

Affected descriptors should be suspended or revoked.

---

## 70. SECRET_MANAGEMENT_BACKEND_COMPROMISED

```text
category: SECURITY
scope: SECRET_BACKEND
severity: CRITICAL
retryClass: NEVER
recoverability: MIGRATION_REQUIRED or ROTATION_REQUIRED
```

Required behavior:

- block new access;
- revoke active leases;
- isolate backend;
- emit restricted event;
- require explicit remediation.

---

## 71. SECRET_MANAGEMENT_INSECURE_BACKEND_DOWNGRADE_BLOCKED

An operation attempted to move or store a secret in a backend that does not satisfy mandatory security policy.

```text
category: SECURITY
scope: SECRET_BACKEND
severity: CRITICAL
retryClass: NEVER
recoverability: SELECT_DIFFERENT_BACKEND
```

---

# Part V — Material and Validation Errors

## 72. SECRET_MANAGEMENT_MATERIAL_MISSING

```text
category: MATERIAL
scope: SECRET_REVISION
severity: ERROR
retryClass: AFTER_USER_ACTION
recoverability: REENTER_SECRET
```

---

## 73. SECRET_MANAGEMENT_MATERIAL_EMPTY

```text
category: MATERIAL
scope: SECRET_REVISION
severity: NOTICE
retryClass: NEVER
recoverability: REENTER_SECRET
```

---

## 74. SECRET_MANAGEMENT_MATERIAL_ENCODING_UNSUPPORTED

```text
category: MATERIAL
scope: SECRET_REVISION
severity: ERROR
retryClass: NEVER
recoverability: REENTER_SECRET or SELECT_DIFFERENT_BACKEND
```

---

## 75. SECRET_MANAGEMENT_MATERIAL_STRUCTURALLY_INVALID

```text
category: MATERIAL
scope: SECRET_REVISION
severity: ERROR
retryClass: AFTER_USER_ACTION
recoverability: REENTER_SECRET
```

Examples:

- malformed PEM;
- missing JSON compound part;
- invalid certificate format;
- invalid private key structure.

---

## 76. SECRET_MANAGEMENT_COMPOUND_PART_MISSING

```text
category: MATERIAL
scope: SECRET_REVISION
severity: ERROR
retryClass: AFTER_USER_ACTION
recoverability: REENTER_SECRET
```

---

## 77. SECRET_MANAGEMENT_COMPOUND_PART_CONFLICT

```text
category: MATERIAL
scope: SECRET_REVISION
severity: ERROR
retryClass: AFTER_USER_ACTION
recoverability: REENTER_SECRET
```

Example:

- certificate does not match private key.

---

## 78. SECRET_MANAGEMENT_MATERIAL_INTEGRITY_FAILED

```text
category: SECURITY
scope: SECRET_REVISION
severity: CRITICAL
retryClass: NEVER
recoverability: ROTATION_REQUIRED
```

---

## 79. SECRET_MANAGEMENT_VALIDATION_UNSUPPORTED

```text
category: VALIDATION
scope: VALIDATION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

May produce a warning if validation was optional.

---

## 80. SECRET_MANAGEMENT_VALIDATION_FAILED

The validation infrastructure failed.

```text
category: VALIDATION
scope: VALIDATION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

This does not prove the secret invalid.

---

## 81. SECRET_MANAGEMENT_VALIDATION_RESULT_INVALID

Authoritative validation proved the revision unusable.

```text
category: VALIDATION
scope: SECRET_REVISION
severity: ERROR
retryClass: AFTER_ROTATION
recoverability: ROTATION_REQUIRED
```

---

## 82. SECRET_MANAGEMENT_VALIDATION_RESULT_UNKNOWN

```text
category: VALIDATION
scope: VALIDATION
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

Policy decides whether use remains allowed.

---

## 83. SECRET_MANAGEMENT_VALIDATION_DEFERRED

```text
category: VALIDATION
scope: VALIDATION
severity: NOTICE
retryClass: AFTER_USER_ACTION or TRANSIENT
recoverability: USER_ACTION or AUTOMATIC
```

---

## 84. SECRET_MANAGEMENT_PROVIDER_AUTHENTICATION_REJECTED

The provider rejected the credential.

```text
category: VALIDATION
scope: EXTERNAL_PROVIDER
severity: ERROR
retryClass: AFTER_REFRESH or AFTER_ROTATION
recoverability: ROTATION_REQUIRED
```

One rejection should not automatically prove global backend corruption.

---

## 85. SECRET_MANAGEMENT_PROVIDER_VALIDATION_RATE_LIMITED

```text
category: VALIDATION
scope: EXTERNAL_PROVIDER
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

`retryAfter` may be included.

---

# Part VI — Registration and Replacement Errors

## 86. SECRET_MANAGEMENT_REGISTRATION_INVALID

The registration command is malformed or inconsistent.

```text
category: REGISTRATION
scope: SECRET_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 87. SECRET_MANAGEMENT_REGISTRATION_REFERENCE_CONFLICT

The desired reference already points to another logical secret.

```text
category: REGISTRATION
scope: REFERENCE
severity: ERROR
retryClass: NEVER
recoverability: SELECT_DIFFERENT_REFERENCE or REPLACEMENT
```

---

## 88. SECRET_MANAGEMENT_REGISTRATION_BACKEND_SELECTION_FAILED

```text
category: REGISTRATION
scope: SECRET_OPERATION
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: SELECT_DIFFERENT_BACKEND
```

---

## 89. SECRET_MANAGEMENT_REGISTRATION_FAILED

```text
category: REGISTRATION
scope: SECRET_OPERATION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC or USER_ACTION
```

No incomplete active descriptor may remain.

---

## 90. SECRET_MANAGEMENT_REGISTRATION_CLEANUP_FAILED

```text
category: CLEANUP
scope: SECRET_OPERATION
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

The candidate must remain unusable.

---

## 91. SECRET_MANAGEMENT_REPLACEMENT_INVALID

```text
category: REPLACEMENT
scope: SECRET_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 92. SECRET_MANAGEMENT_REPLACEMENT_REVISION_CONFLICT

```text
category: CONCURRENCY
scope: SECRET_REVISION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: RETRY
```

---

## 93. SECRET_MANAGEMENT_REPLACEMENT_FAILED

```text
category: REPLACEMENT
scope: SECRET_OPERATION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC or USER_ACTION
```

The previous active revision remains active where possible.

---

# Part VII — Lease Errors

## 94. SECRET_MANAGEMENT_LEASE_REQUEST_INVALID

```text
category: LEASE
scope: SECRET_LEASE
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 95. SECRET_MANAGEMENT_LEASE_SECRET_UNAVAILABLE

```text
category: LEASE
scope: SECRET_LEASE
severity: ERROR
retryClass: conditional
recoverability: depends on availability
```

The nested safe reason identifies:

```text
LOCKED
EXPIRED
REVOKED
INVALID
BACKEND_UNAVAILABLE
USER_ACTION_REQUIRED
```

---

## 96. SECRET_MANAGEMENT_LEASE_CAPACITY_EXCEEDED

```text
category: LEASE
scope: SECRET_LEASE
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 97. SECRET_MANAGEMENT_LEASE_NOT_FOUND

```text
category: LEASE
scope: SECRET_LEASE
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 98. SECRET_MANAGEMENT_LEASE_EXPIRED

```text
category: LEASE
scope: SECRET_LEASE
severity: NOTICE
retryClass: NEVER for same lease
recoverability: RETRY with new lease
```

---

## 99. SECRET_MANAGEMENT_LEASE_REVOKED

```text
category: LEASE
scope: SECRET_LEASE
severity: ERROR
retryClass: NEVER for same lease
recoverability: depends on secret state
```

---

## 100. SECRET_MANAGEMENT_LEASE_RELEASED

The caller attempted to use a released lease.

```text
category: LEASE
scope: SECRET_LEASE
severity: NOTICE
retryClass: NEVER for same lease
recoverability: RETRY with new lease
```

---

## 101. SECRET_MANAGEMENT_LEASE_PURPOSE_MISMATCH

```text
category: SECURITY
scope: SECRET_LEASE
severity: CRITICAL
retryClass: NEVER
recoverability: NONE
```

---

## 102. SECRET_MANAGEMENT_LEASE_CONSUMER_MISMATCH

```text
category: SECURITY
scope: SECRET_LEASE
severity: CRITICAL
retryClass: NEVER
recoverability: NONE
```

---

## 103. SECRET_MANAGEMENT_LEASE_REVISION_MISMATCH

```text
category: LEASE
scope: SECRET_LEASE
severity: ERROR
retryClass: NEVER for same lease
recoverability: RETRY with new lease
```

---

## 104. SECRET_MANAGEMENT_LEASE_RENEWAL_DENIED

```text
category: LEASE
scope: SECRET_LEASE
severity: NOTICE
retryClass: NEVER for same lease
recoverability: RETRY with new lease
```

---

## 105. SECRET_MANAGEMENT_LEASE_CLEANUP_FAILED

```text
category: CLEANUP
scope: SECRET_LEASE
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

Logical authority must already be removed.

---

## 106. SECRET_MANAGEMENT_LEASE_ABANDONED

```text
category: CLEANUP
scope: SECRET_LEASE
severity: WARNING
retryClass: NEVER for same lease
recoverability: AUTOMATIC cleanup
```

---

# Part VIII — Rotation Errors

## 107. SECRET_MANAGEMENT_ROTATION_INVALID

```text
category: ROTATION
scope: ROTATION
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 108. SECRET_MANAGEMENT_ROTATION_NOT_SUPPORTED

```text
category: ROTATION
scope: ROTATION
severity: ERROR
retryClass: NEVER
recoverability: USER_ACTION or RECREATE_SECRET
```

---

## 109. SECRET_MANAGEMENT_ROTATION_REVISION_CONFLICT

```text
category: CONCURRENCY
scope: ROTATION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: RETRY
```

---

## 110. SECRET_MANAGEMENT_ROTATION_GENERATION_FAILED

```text
category: ROTATION
scope: ROTATION
severity: ERROR
retryClass: TRANSIENT or AFTER_USER_ACTION
recoverability: AUTOMATIC or USER_ACTION
```

---

## 111. SECRET_MANAGEMENT_ROTATION_CANDIDATE_STORE_FAILED

```text
category: ROTATION
scope: ROTATION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC
```

---

## 112. SECRET_MANAGEMENT_ROTATION_VALIDATION_FAILED

```text
category: ROTATION
scope: ROTATION
severity: ERROR
retryClass: AFTER_USER_ACTION or TRANSIENT
recoverability: USER_ACTION or AUTOMATIC
```

The old revision remains active.

---

## 113. SECRET_MANAGEMENT_ROTATION_ACTIVATION_FAILED

```text
category: ROTATION
scope: ROTATION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC or RECONCILIATION_REQUIRED
```

---

## 114. SECRET_MANAGEMENT_ROTATION_LEASE_POLICY_FAILED

The new revision may already be active, but old lease handling failed.

```text
category: ROTATION
scope: ROTATION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC or ADMIN_ACTION
```

The operation may be partially completed.

---

## 115. SECRET_MANAGEMENT_ROTATION_OUTCOME_UNCERTAIN

```text
category: RECONCILIATION
scope: ROTATION
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

Required flags:

```text
automaticRetryBlocked = true
reconciliationRequired = true
```

---

## 116. SECRET_MANAGEMENT_ROTATION_RECONCILIATION_FAILED

```text
category: RECONCILIATION
scope: ROTATION
severity: CRITICAL
retryClass: AFTER_USER_ACTION
recoverability: ADMIN_ACTION
```

The descriptor should remain suspended until resolved.

---

## 117. SECRET_MANAGEMENT_ROTATION_CLEANUP_FAILED

```text
category: CLEANUP
scope: ROTATION
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

# Part IX — Migration Errors

## 118. SECRET_MANAGEMENT_MIGRATION_INVALID

```text
category: MIGRATION
scope: MIGRATION
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 119. SECRET_MANAGEMENT_MIGRATION_SOURCE_UNAVAILABLE

```text
category: MIGRATION
scope: MIGRATION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 120. SECRET_MANAGEMENT_MIGRATION_DESTINATION_UNAVAILABLE

```text
category: MIGRATION
scope: MIGRATION
severity: ERROR
retryClass: TRANSIENT or AFTER_CONFIGURATION_CHANGE
recoverability: AUTOMATIC or SELECT_DIFFERENT_BACKEND
```

---

## 121. SECRET_MANAGEMENT_MIGRATION_CAPABILITY_MISMATCH

```text
category: MIGRATION
scope: MIGRATION
severity: ERROR
retryClass: NEVER
recoverability: SELECT_DIFFERENT_BACKEND
```

---

## 122. SECRET_MANAGEMENT_MIGRATION_COPY_FAILED

```text
category: MIGRATION
scope: MIGRATION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC
```

The source remains authoritative.

---

## 123. SECRET_MANAGEMENT_MIGRATION_DESTINATION_VALIDATION_FAILED

```text
category: MIGRATION
scope: MIGRATION
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE or TRANSIENT
recoverability: SELECT_DIFFERENT_BACKEND or AUTOMATIC
```

---

## 124. SECRET_MANAGEMENT_MIGRATION_SWITCH_FAILED

```text
category: MIGRATION
scope: MIGRATION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: RECONCILIATION_REQUIRED
```

---

## 125. SECRET_MANAGEMENT_MIGRATION_SOURCE_CLEANUP_FAILED

The destination is active but source cleanup failed.

```text
category: CLEANUP
scope: MIGRATION
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC or ADMIN_ACTION
```

This is a partial completion warning or error depending on policy.

---

## 126. SECRET_MANAGEMENT_MIGRATION_OUTCOME_UNCERTAIN

```text
category: RECONCILIATION
scope: MIGRATION
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 127. SECRET_MANAGEMENT_MIGRATION_RECONCILIATION_FAILED

```text
category: RECONCILIATION
scope: MIGRATION
severity: CRITICAL
retryClass: AFTER_USER_ACTION
recoverability: ADMIN_ACTION
```

---

# Part X — Refresh Errors

## 128. SECRET_MANAGEMENT_REFRESH_NOT_SUPPORTED

```text
category: REFRESH
scope: SECRET_REVISION
severity: NOTICE
retryClass: NEVER
recoverability: ROTATION_REQUIRED
```

---

## 129. SECRET_MANAGEMENT_REFRESH_TOKEN_MISSING

```text
category: REFRESH
scope: SECRET_REVISION
severity: ERROR
retryClass: AFTER_USER_ACTION
recoverability: REENTER_SECRET
```

---

## 130. SECRET_MANAGEMENT_REFRESH_REJECTED

```text
category: REFRESH
scope: EXTERNAL_PROVIDER
severity: ERROR
retryClass: AFTER_USER_ACTION
recoverability: REENTER_SECRET or ROTATION_REQUIRED
```

---

## 131. SECRET_MANAGEMENT_REFRESH_FAILED

```text
category: REFRESH
scope: EXTERNAL_PROVIDER
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 132. SECRET_MANAGEMENT_REFRESH_OUTCOME_UNCERTAIN

```text
category: RECONCILIATION
scope: EXTERNAL_PROVIDER
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

# Part XI — Revocation and Removal Errors

## 133. SECRET_MANAGEMENT_REVOCATION_INVALID

```text
category: REVOCATION
scope: SECRET_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 134. SECRET_MANAGEMENT_EXTERNAL_REVOCATION_UNSUPPORTED

```text
category: REVOCATION
scope: EXTERNAL_PROVIDER
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

Local revocation may still succeed.

---

## 135. SECRET_MANAGEMENT_EXTERNAL_REVOCATION_FAILED

```text
category: REVOCATION
scope: EXTERNAL_PROVIDER
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC or ADMIN_ACTION
```

---

## 136. SECRET_MANAGEMENT_EXTERNAL_REVOCATION_OUTCOME_UNCERTAIN

```text
category: RECONCILIATION
scope: EXTERNAL_PROVIDER
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 137. SECRET_MANAGEMENT_REMOVAL_INVALID

```text
category: REMOVAL
scope: SECRET_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 138. SECRET_MANAGEMENT_REMOVAL_BLOCKED_BY_ACTIVE_LEASES

```text
category: REMOVAL
scope: SECRET_OPERATION
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC or ADMIN_ACTION
```

---

## 139. SECRET_MANAGEMENT_MATERIAL_DELETION_FAILED

```text
category: REMOVAL
scope: SECRET_REVISION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC or ADMIN_ACTION
```

The descriptor remains blocked.

---

## 140. SECRET_MANAGEMENT_DELETION_VERIFICATION_FAILED

```text
category: REMOVAL
scope: SECRET_REVISION
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 141. SECRET_MANAGEMENT_TOMBSTONE_PERSIST_FAILED

```text
category: PERSISTENCE
scope: SECRET
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 142. SECRET_MANAGEMENT_REMOVAL_PARTIALLY_COMPLETED

```text
category: REMOVAL
scope: SECRET_OPERATION
severity: WARNING or ERROR
retryClass: conditional
recoverability: ADMIN_ACTION or AUTOMATIC
```

The error must identify safe completion dimensions:

```text
materialDeletionStatus
externalRevocationStatus
tombstoneStatus
```

---

# Part XII — Export Errors

## 143. SECRET_MANAGEMENT_EXPORT_DENIED

```text
category: EXPORT
scope: SECRET_OPERATION
severity: ERROR
retryClass: NEVER
recoverability: UPDATE_ACCESS_POLICY
```

---

## 144. SECRET_MANAGEMENT_EXPORT_TARGET_UNAPPROVED

```text
category: SECURITY
scope: SECRET_OPERATION
severity: CRITICAL
retryClass: NEVER
recoverability: NONE
```

Examples:

- clipboard;
- ordinary file;
- unrestricted environment variable;
- log;
- event;
- UI text.

---

## 145. SECRET_MANAGEMENT_EXPORT_NOT_SUPPORTED_BY_BACKEND

```text
category: EXPORT
scope: SECRET_BACKEND
severity: ERROR
retryClass: NEVER
recoverability: SELECT_DIFFERENT_BACKEND
```

---

## 146. SECRET_MANAGEMENT_EXPORT_CHANNEL_BINDING_FAILED

```text
category: SECURITY
scope: SECRET_OPERATION
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

# Part XIII — Reconciliation Errors

## 147. SECRET_MANAGEMENT_RECONCILIATION_REQUIRED

```text
category: RECONCILIATION
scope: SECRET_OPERATION
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: RECONCILIATION_REQUIRED
```

---

## 148. SECRET_MANAGEMENT_RECONCILIATION_EVIDENCE_INSUFFICIENT

```text
category: RECONCILIATION
scope: SECRET_OPERATION
severity: ERROR
retryClass: AFTER_USER_ACTION
recoverability: ADMIN_ACTION
```

---

## 149. SECRET_MANAGEMENT_RECONCILIATION_CONFLICT

Evidence sources disagree.

```text
category: RECONCILIATION
scope: SECRET_OPERATION
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 150. SECRET_MANAGEMENT_MANUAL_RECOVERY_REQUIRED

```text
category: RECONCILIATION
scope: SECRET_OPERATION
severity: ERROR
retryClass: AFTER_USER_ACTION
recoverability: ADMIN_ACTION
```

---

# Part XIV — Concurrency and Idempotency Errors

## 151. SECRET_MANAGEMENT_VERSION_CONFLICT

```text
category: CONCURRENCY
scope: SECRET
severity: NOTICE
retryClass: IDEMPOTENT_ONLY
recoverability: RETRY
```

---

## 152. SECRET_MANAGEMENT_STALE_REVISION_UPDATE

A command targeted an older revision.

```text
category: CONCURRENCY
scope: SECRET_REVISION
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: RETRY
```

---

## 153. SECRET_MANAGEMENT_IDEMPOTENCY_CONFLICT

The same idempotency key was reused with different semantic input.

```text
category: IDEMPOTENCY
scope: SECRET_OPERATION
severity: ERROR
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 154. SECRET_MANAGEMENT_OPERATION_ALREADY_COMPLETED

```text
category: IDEMPOTENCY
scope: SECRET_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

The existing receipt should be returned when safe.

---

## 155. SECRET_MANAGEMENT_OPERATION_IN_PROGRESS

```text
category: CONCURRENCY
scope: SECRET_OPERATION
severity: NOTICE
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 156. SECRET_MANAGEMENT_CONCURRENT_ROTATION_CONFLICT

```text
category: CONCURRENCY
scope: ROTATION
severity: ERROR
retryClass: AFTER_RECONCILIATION or TRANSIENT
recoverability: AUTOMATIC
```

---

## 157. SECRET_MANAGEMENT_CONCURRENT_MIGRATION_CONFLICT

```text
category: CONCURRENCY
scope: MIGRATION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

# Part XV — Persistence and Event Errors

## 158. SECRET_MANAGEMENT_STATE_PERSIST_FAILED

```text
category: PERSISTENCE
scope: MODULE
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC or APPLICATION_RESTART
```

No event should claim success.

---

## 159. SECRET_MANAGEMENT_OPERATION_RECEIPT_PERSIST_FAILED

```text
category: PERSISTENCE
scope: SECRET_OPERATION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 160. SECRET_MANAGEMENT_AUDIT_RECORD_PERSIST_FAILED

```text
category: PERSISTENCE
scope: MODULE
severity: CRITICAL when audit is mandatory
retryClass: TRANSIENT
recoverability: AUTOMATIC or APPLICATION_RESTART
```

Sensitive operations may need to fail closed.

---

## 161. SECRET_MANAGEMENT_EVENT_PUBLICATION_FAILED

```text
category: EVENT_PUBLICATION
scope: MODULE
severity: WARNING or ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

Committed state remains authoritative.

---

## 162. SECRET_MANAGEMENT_EVENT_PAYLOAD_UNSAFE

The event payload failed secret-safety validation.

```text
category: SECURITY
scope: MODULE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

Publication must be blocked.

---

# Part XVI — Serialization, Redaction, and Exposure Errors

## 163. SECRET_MANAGEMENT_SENSITIVE_TYPE_SERIALIZATION_BLOCKED

An attempt was made to serialize a sensitive type.

```text
category: SERIALIZATION
scope: MODULE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

Examples:

- `SecretHandle`;
- `SecretMaterialInput`;
- secure buffer;
- platform credential object;
- decrypted backend entry.

---

## 164. SECRET_MANAGEMENT_SECRET_EXPOSURE_BLOCKED

A prohibited exposure attempt was detected and blocked.

```text
category: SECURITY
scope: MODULE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

Possible boundaries:

```text
EVENT
LOG
TRACE
METRIC
DIAGNOSTIC
CONFIGURATION
UI
CLIPBOARD
FILE
EXCEPTION
CHILD_PROCESS
```

The matched value must not be included in the error.

---

## 165. SECRET_MANAGEMENT_SECRET_EXPOSURE_DETECTED

Evidence indicates material may already have crossed a prohibited boundary.

```text
category: SECURITY
scope: MODULE
severity: CRITICAL
retryClass: NEVER
recoverability: ROTATION_REQUIRED or ADMIN_ACTION
```

Required response may include:

- isolate output;
- rotate affected secret;
- revoke leases;
- restrict diagnostics;
- preserve safe audit evidence.

---

## 166. SECRET_MANAGEMENT_REDACTION_FAILED

```text
category: REDACTION
scope: MODULE
severity: CRITICAL
retryClass: NEVER for unsafe output
recoverability: ADMIN_ACTION
```

Fail-safe behavior:

```text
block output
```

---

## 167. SECRET_MANAGEMENT_REDACTION_FALSE_POSITIVE

A value was blocked but later determined not to be secret material.

```text
category: REDACTION
scope: MODULE
severity: WARNING
retryClass: NEVER
recoverability: ADMIN_ACTION
```

The original value must still not be echoed into unrestricted diagnostics.

---

## 168. SECRET_MANAGEMENT_UNSAFE_EXCEPTION_BLOCKED

A platform or provider exception contained potentially sensitive data.

```text
category: SECURITY
scope: MODULE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

The exception must be replaced by a safe normalized error.

---

## 169. SECRET_MANAGEMENT_UNSAFE_LOG_METADATA_BLOCKED

```text
category: SECURITY
scope: MODULE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 170. SECRET_MANAGEMENT_UNSAFE_REFERENCE_DISPLAY_BLOCKED

```text
category: REDACTION
scope: REFERENCE
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

A redacted label should be used.

---

# Part XVII — Configuration Boundary Errors

## 171. SECRET_MANAGEMENT_RAW_SECRET_IN_CONFIGURATION

Raw secret material was detected in ordinary configuration.

```text
category: CONFIGURATION_BOUNDARY
scope: MODULE
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

Required behavior:

- reject configuration activation;
- redact diagnostics;
- guide migration to `SecretReference`;
- consider rotation if exposure may have occurred.

---

## 172. SECRET_MANAGEMENT_SECRET_REFERENCE_CONFIGURATION_INVALID

```text
category: CONFIGURATION_BOUNDARY
scope: REFERENCE
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 173. SECRET_MANAGEMENT_ENVIRONMENT_SECRET_NOT_ALLOWED

Environment-backed secret use is prohibited by current policy.

```text
category: CONFIGURATION_BOUNDARY
scope: SECRET_BACKEND
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: SELECT_DIFFERENT_BACKEND
```

---

# Part XVIII — Internal Invariant Errors

## 174. SECRET_MANAGEMENT_INVALID_STATE_TRANSITION

```text
category: INTERNAL
scope: MODULE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 175. SECRET_MANAGEMENT_STATE_VERSION_REGRESSION

```text
category: INTERNAL
scope: MODULE
severity: CRITICAL
retryClass: NEVER
recoverability: RECONCILIATION_REQUIRED
```

---

## 176. SECRET_MANAGEMENT_SECRET_HANDLE_LEAK_DETECTED

A handle remained live beyond lease authority.

```text
category: SECURITY
scope: SECRET_LEASE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 177. SECRET_MANAGEMENT_TEMPORARY_BUFFER_CLEANUP_FAILED

```text
category: CLEANUP
scope: MODULE
severity: WARNING or CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

Severity depends on whether material may remain exposed.

---

## 178. SECRET_MANAGEMENT_MEMORY_ONLY_SECRET_LOST

A memory-backed secret disappeared after restart or process loss.

```text
category: MATERIAL
scope: SECRET
severity: NOTICE
retryClass: AFTER_USER_ACTION
recoverability: REENTER_SECRET
```

---

## 179. SECRET_MANAGEMENT_MODULE_UNAVAILABLE

```text
category: INTERNAL
scope: MODULE
severity: CRITICAL
retryClass: TRANSIENT or APPLICATION_RESTART
recoverability: APPLICATION_RESTART
```

Provider paths requiring credentials must become unavailable.

---

## 180. SECRET_MANAGEMENT_FATAL_SAFETY_INVARIANT_BROKEN

```text
category: SECURITY
scope: MODULE
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART or NOT_RECOVERABLE
```

Use only when the application cannot safely protect secret material.

---

# Part XIX — Warnings

## 181. Warning Model

```text
SecretManagementWarning {
    warningId
    code
    severity
    scope
    safeMessage
    recoveryActions[]
    metadata
}
```

Warnings:

- do not grant access;
- do not change state by themselves;
- do not schedule retry;
- must be bounded;
- must be secret-safe;
- may accompany successful or partially completed operations.

---

## 182. SECRET_MANAGEMENT_WARNING_BACKEND_DEGRADED

The backend remains usable with reduced capability.

---

## 183. SECRET_MANAGEMENT_WARNING_VALIDATION_UNKNOWN

The secret remains usable under policy, but validation is inconclusive.

---

## 184. SECRET_MANAGEMENT_WARNING_ROTATION_CLEANUP_PENDING

The new revision is active, but old material or client cleanup is pending.

---

## 185. SECRET_MANAGEMENT_WARNING_MIGRATION_SOURCE_CLEANUP_PENDING

The destination is active, but the source copy remains.

---

## 186. SECRET_MANAGEMENT_WARNING_SECURE_DELETE_NOT_GUARANTEED

Logical or backend-confirmed deletion completed, but physical erasure cannot be guaranteed.

---

## 187. SECRET_MANAGEMENT_WARNING_EXTERNAL_REVOCATION_UNSUPPORTED

Local revocation succeeded, but provider-side revocation is unsupported.

---

## 188. SECRET_MANAGEMENT_WARNING_ACTIVE_LEASES_DRAINING

A rotation, migration, suspension, or removal is waiting for bounded lease drain.

---

## 189. SECRET_MANAGEMENT_WARNING_ENVIRONMENT_BACKED_SECRET

A secret is sourced from environment configuration under an explicitly permitted but weaker operational mode.

---

## 190. SECRET_MANAGEMENT_WARNING_REFERENCE_REDACTED

The full reference was withheld because its alias may reveal sensitive metadata.

---

## 191. SECRET_MANAGEMENT_WARNING_PARTIAL_COMPLETION

An operation completed its safety-critical core but one or more cleanup or external steps remain.

Metadata must identify safe dimensions only.

---

# Part XX — Retry and Recovery Rules

## 192. Errors Do Not Retry Themselves

```text
Error normalized
    ↓
Current state and authority validated
    ↓
Retry / recovery policy evaluates
    ↓
New operation or attempt created
```

Secret Management may own local administrative retry policy for its own operations.

Runtime retry policy governs Runtime work.

---

## 193. Never Retry Blindly

Do not blindly retry:

- provider-side key generation;
- provider-side revocation;
- uncertain refresh;
- uncertain backend write;
- uncertain activation;
- uncertain migration switch;
- uncertain deletion;
- non-idempotent export.

These require reconciliation.

---

## 194. Safe Retry Examples

Potentially safe:

- backend availability probe;
- lease cleanup;
- validation after temporary network failure;
- event publication;
- idempotent state persistence;
- descriptor query;
- backend unlock after explicit user action.

---

## 195. Retry After User Action

Examples:

- backend locked;
- credential re-entry;
- device unlock;
- external login;
- permission grant;
- manual reconciliation.

---

## 196. Retry After Rotation

Examples:

- expired revision;
- revoked revision;
- invalid material;
- provider authentication rejection;
- compromised old credential.

---

# Part XXI — State Transition Implications

## 197. Reference Errors

Reference parsing errors do not change secret state.

---

## 198. Backend Locked

```text
Backend → LOCKED
Availability → LOCKED
Descriptor remains unchanged
No new lease
```

---

## 199. Backend Unavailable

```text
Backend → UNAVAILABLE
Availability → BACKEND_UNAVAILABLE
Descriptor remains unchanged
Existing leases follow policy
```

---

## 200. Backend Compromised

```text
Backend → COMPROMISED
Affected descriptors → SUSPENDED or REVOKED
Active leases → REVOKED
Availability → REVOKED or UNAVAILABLE
```

---

## 201. Active Revision Invalid

```text
Revision → INVALID
Descriptor → SUSPENDED or REVOKED
Availability → INVALID
Active leases → REVOKED when required
```

---

## 202. Rotation Failure

Safe failure:

```text
Candidate → INVALID / CLEANUP_PENDING
Old revision remains ACTIVE
Descriptor ROTATING → ACTIVE
```

Uncertain failure:

```text
Rotation → UNCERTAIN
Descriptor → SUSPENDED
Reconciliation required
```

---

## 203. Migration Failure

Before switch:

```text
Source remains authoritative
Descriptor MIGRATING → ACTIVE
```

After uncertain switch:

```text
Descriptor → SUSPENDED
Migration → UNCERTAIN
Reconciliation required
```

---

## 204. Removal Failure

Material deletion failure:

```text
Descriptor remains REMOVING or REVOKED
No new lease
Material use remains blocked
```

---

## 205. Exposure Error

Potential exposure:

```text
Block output
    ↓
Identify affected secret safely
    ↓
Revoke leases if needed
    ↓
Rotate or revoke affected revision
    ↓
Emit restricted security event
```

---

# Part XXII — Cross-Module Normalization

## 206. Provider Management Mapping

Examples:

| Secret Management code | Provider Management projection |
|---|---|
| `REFERENCE_NOT_FOUND` | credential reference not found |
| `BACKEND_LOCKED` | credential unavailable / user action required |
| `REVISION_EXPIRED` | credential expired |
| `REVISION_REVOKED` | credential revoked |
| `PROVIDER_AUTHENTICATION_REJECTED` | authentication failure |
| `LEASE_REVOKED` | provider client or lease must stop |
| `BACKEND_COMPROMISED` | credential path prohibited |

Provider Management must not expose Secret Management internals or raw backend messages.

---

## 207. Configuration Mapping

Examples:

| Secret Management code | Configuration interpretation |
|---|---|
| `REFERENCE_INVALID` | invalid configuration value |
| `RAW_SECRET_IN_CONFIGURATION` | configuration security violation |
| `ENVIRONMENT_SECRET_NOT_ALLOWED` | policy validation failure |
| `REFERENCE_NOT_FOUND` | reference availability error |

Configuration should retain references, not material.

---

## 208. Runtime Mapping

Runtime may normalize secret-access failure into a runtime-safe provider or dependency failure.

Examples:

```text
BACKEND_LOCKED
    → user action required, do not retry automatically

LEASE_REVOKED
    → current provider operation loses authority

BACKEND_UNAVAILABLE
    → transient dependency failure

CONSUMER_MISMATCH
    → security invariant failure
```

Runtime must not receive raw secret diagnostics.

---

## 209. Presentation Mapping

User-facing levels may include:

```text
INLINE_NOTICE
NON_BLOCKING_WARNING
PROVIDER_BLOCKING_ERROR
SESSION_BLOCKING_ERROR
APPLICATION_BLOCKING_ERROR
```

Examples:

- backend locked → inline action;
- secret missing → provider settings error;
- secret revoked → provider blocking;
- backend compromised → application/security blocking;
- rotation cleanup pending → non-blocking warning.

Technical severity does not map directly to user impact.

---

# Part XXIII — Logging and Observability

## 210. Logging Policy

### TRACE

- duplicate release ignored;
- stale idempotent request rejected;
- optional validation skipped;
- availability recomputed unchanged.

### INFO

- backend recovered;
- rotation completed;
- migration completed;
- user action resolved;
- descriptor reactivated.

### WARNING

- backend degraded;
- transient validation failure;
- cleanup pending;
- active leases draining;
- secure deletion not guaranteed.

### ERROR

- registration failed;
- rotation failed;
- migration failed;
- backend unavailable;
- removal failed;
- persistence failed.

### CRITICAL

- consumer mismatch;
- purpose violation;
- raw secret in configuration;
- unsafe serialization blocked;
- exposure detected;
- backend compromised;
- multiple active revisions.

### FATAL

- application cannot enforce secret safety.

---

## 211. Log Fields

Allowed:

```text
errorCode
category
severity
scope
secretId
referenceId
revision
secretLeaseId
secretBackendId
operationId
rotationId
migrationId
consumerId
providerId
correlationId
retryClass
recoverability
```

Prohibited:

```text
raw secret
token fragment
private key
authorization header
password
decrypted payload
raw exception containing sensitive data
full sensitive alias
```

---

## 212. Metrics

Recommended metrics:

```text
secret_management_errors_total
secret_management_warnings_total
secret_management_access_denied_total
secret_management_backend_errors_total
secret_management_rotation_errors_total
secret_management_migration_errors_total
secret_management_lease_errors_total
secret_management_reconciliation_required_total
secret_management_exposure_blocked_total
secret_management_redaction_failures_total
secret_management_fatal_total
```

Labels should use:

```text
code
category
scope
severity
backendType
operationClass
```

Do not use high-cardinality secret IDs or aliases as metric labels.

---

## 213. Tracing

Trace spans may contain:

- normalized code;
- operation stage;
- backend type;
- consumer module;
- provider ID when safe;
- retry class;
- state transition;
- duration.

Trace spans must not contain secret material.

---

## 214. Error Events

Error events follow `EVENTS.md`.

Only safe fields may be published.

Critical security errors use restricted visibility.

Error publication failure does not alter authoritative state.

---

# Part XXIV — Testing Requirements

## 215. Reference Tests

- malformed reference;
- unsupported scheme;
- embedded material;
- hidden existence;
- kind mismatch;
- provider mismatch;
- minimum revision mismatch.

---

## 216. Policy Tests

- missing identity;
- untrusted identity;
- denied consumer;
- denied purpose;
- denied provider;
- excessive duration;
- hidden existence behavior.

---

## 217. Backend Tests

- initialization failure;
- locked;
- unlock canceled;
- unavailable;
- permission denied;
- capability mismatch;
- read/write/delete failure;
- uncertain write;
- corruption;
- compromise;
- downgrade blocked.

---

## 218. Material Tests

- empty;
- unsupported encoding;
- malformed PEM;
- compound part missing;
- certificate/private-key mismatch;
- integrity failure;
- provider authentication rejected.

---

## 219. Lease Tests

- expired;
- revoked;
- released;
- consumer mismatch;
- purpose mismatch;
- revision mismatch;
- capacity exceeded;
- renewal denied;
- cleanup failure;
- abandonment.

---

## 220. Rotation Tests

- revision conflict;
- generation failure;
- candidate store failure;
- validation failure;
- activation failure;
- lease policy failure;
- uncertain outcome;
- reconciliation failure;
- cleanup warning.

---

## 221. Migration Tests

- source unavailable;
- destination unavailable;
- capability mismatch;
- copy failure;
- destination validation failure;
- switch failure;
- source cleanup failure;
- uncertain outcome;
- reconciliation conflict.

---

## 222. Removal Tests

- active leases block;
- external revocation unsupported;
- external revocation failed;
- deletion failed;
- deletion verification failed;
- tombstone persistence failed;
- partial completion;
- assurance warning.

---

## 223. Security Tests

- raw secret in config;
- secret in event;
- secret in log metadata;
- secret in exception;
- sensitive type serialization;
- clipboard export;
- unapproved file export;
- child-process channel mismatch;
- redaction failure;
- handle leak;
- multiple active revisions.

---

## 224. Privacy Tests

Every public error must prove absence of:

```text
secret values
token fragments
passwords
private keys
authorization headers
raw backend payloads
raw provider payloads
sensitive aliases
```

---

# Part XXV — MVP Error Boundary

## 225. Required MVP Codes

The desktop MVP should implement at least:

```text
SECRET_MANAGEMENT_REFERENCE_INVALID
SECRET_MANAGEMENT_REFERENCE_NOT_FOUND
SECRET_MANAGEMENT_REFERENCE_KIND_MISMATCH

SECRET_MANAGEMENT_ACCESS_DENIED
SECRET_MANAGEMENT_CONSUMER_MISMATCH
SECRET_MANAGEMENT_PURPOSE_NOT_ALLOWED
SECRET_MANAGEMENT_USER_PRESENCE_REQUIRED

SECRET_MANAGEMENT_BACKEND_LOCKED
SECRET_MANAGEMENT_BACKEND_UNAVAILABLE
SECRET_MANAGEMENT_BACKEND_PERMISSION_DENIED
SECRET_MANAGEMENT_BACKEND_READ_FAILED
SECRET_MANAGEMENT_BACKEND_WRITE_FAILED
SECRET_MANAGEMENT_BACKEND_COMPROMISED

SECRET_MANAGEMENT_MATERIAL_MISSING
SECRET_MANAGEMENT_MATERIAL_STRUCTURALLY_INVALID
SECRET_MANAGEMENT_VALIDATION_RESULT_INVALID
SECRET_MANAGEMENT_PROVIDER_AUTHENTICATION_REJECTED

SECRET_MANAGEMENT_REGISTRATION_FAILED
SECRET_MANAGEMENT_REPLACEMENT_REVISION_CONFLICT

SECRET_MANAGEMENT_LEASE_EXPIRED
SECRET_MANAGEMENT_LEASE_REVOKED
SECRET_MANAGEMENT_LEASE_CONSUMER_MISMATCH
SECRET_MANAGEMENT_LEASE_PURPOSE_MISMATCH

SECRET_MANAGEMENT_ROTATION_FAILED
SECRET_MANAGEMENT_ROTATION_OUTCOME_UNCERTAIN

SECRET_MANAGEMENT_REMOVAL_PARTIALLY_COMPLETED
SECRET_MANAGEMENT_DELETION_VERIFICATION_FAILED

SECRET_MANAGEMENT_IDEMPOTENCY_CONFLICT
SECRET_MANAGEMENT_VERSION_CONFLICT
SECRET_MANAGEMENT_STATE_PERSIST_FAILED

SECRET_MANAGEMENT_RAW_SECRET_IN_CONFIGURATION
SECRET_MANAGEMENT_SENSITIVE_TYPE_SERIALIZATION_BLOCKED
SECRET_MANAGEMENT_SECRET_EXPOSURE_BLOCKED
SECRET_MANAGEMENT_REDACTION_FAILED

SECRET_MANAGEMENT_UNKNOWN_ERROR
```

---

## 226. Required MVP Warnings

```text
SECRET_MANAGEMENT_WARNING_BACKEND_DEGRADED
SECRET_MANAGEMENT_WARNING_VALIDATION_UNKNOWN
SECRET_MANAGEMENT_WARNING_ROTATION_CLEANUP_PENDING
SECRET_MANAGEMENT_WARNING_SECURE_DELETE_NOT_GUARANTEED
SECRET_MANAGEMENT_WARNING_ACTIVE_LEASES_DRAINING
SECRET_MANAGEMENT_WARNING_PARTIAL_COMPLETION
```

---

# Part XXVI — Error Decisions

## 227. Decisions

### Decision 1 — Errors never contain secret material

This applies to public, internal, restricted, audit, and fatal errors.

### Decision 2 — Errors do not own retry scheduling

They expose retry class and recovery guidance.

### Decision 3 — Uncertain outcome is distinct

Non-idempotent uncertainty blocks blind retry.

### Decision 4 — Backend lock is not missing

It maps to user action or unlock.

### Decision 5 — Validation failure is not always invalidity

Infrastructure failure and authoritative invalid result remain separate.

### Decision 6 — Last known good revision survives safe failure

Failed candidates do not replace the current revision.

### Decision 7 — Security errors fail closed

Exposure, mismatch, compromise, and unsafe serialization block the operation.

### Decision 8 — Revocation differs from deletion

Errors and recovery actions preserve this distinction.

### Decision 9 — Deletion assurance is explicit

Warnings prevent overstating physical erasure.

### Decision 10 — Consumer and purpose mismatch are critical

A secret lease is non-transferable and purpose-bound.

### Decision 11 — Raw platform exceptions remain internal

Only normalized safe errors cross boundaries.

### Decision 12 — Unknown errors remain supported

Consumers must handle future codes safely.

---

# Part XXVII — Open Decisions

## 228. Policy Decisions

Still to finalize:

- exact retry delays;
- lockout thresholds;
- repeated access-denial escalation;
- when validation unknown blocks use;
- when backend degradation becomes unavailable;
- rotation cleanup timeout;
- migration cleanup timeout;
- reconciliation escalation timing;
- fatal safety shutdown policy;
- automatic rotation after exposure;
- active lease policy on suspected compromise.

---

## 229. User Mapping Decisions

Still to finalize:

- exact user-visible wording;
- when provider settings open automatically;
- when application restart is required;
- when a security notification is shown;
- how hidden existence errors appear;
- how partial deletion assurance is explained.

---

## 230. Platform Decisions

Still to finalize:

- Windows error normalization;
- macOS Keychain error normalization;
- Linux Secret Service error normalization;
- biometric cancellation mapping;
- OS account change mapping;
- secure-delete assurance mapping;
- external secret-manager timeout normalization.

---

## 231. Observability Decisions

Still to finalize:

- alert thresholds;
- restricted error retention;
- audit failure escalation;
- exposure event paging;
- compromise alerting;
- error sampling;
- safe cardinality limits.

---

# Part XXVIII — Related Documents

## 232. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/RESOURCE_LIFECYCLE.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/configuration/MODULE.md
03-infrastructure/configuration/CONTRACT.md

03-infrastructure/secret-management/MODULE.md
03-infrastructure/secret-management/CONTRACT.md
03-infrastructure/secret-management/STATES.md
03-infrastructure/secret-management/EVENTS.md

02-modules/provider-management/MODULE.md
02-modules/provider-management/CONTRACT.md
02-modules/provider-management/STATES.md
02-modules/provider-management/EVENTS.md
02-modules/provider-management/ERRORS.md
```

Future document:

```text
03-infrastructure/secret-management/README.md
```

---

## 233. Summary

Secret Management errors normalize reference, policy, identity, backend, material, lease, rotation, migration, validation, refresh, revocation, removal, persistence, serialization, redaction, and security failures without exposing secret material.

The core error flow is:

```text
Platform / Provider / Backend failure
    ↓
Secret boundary catches raw failure
    ↓
Sensitive data removed
    ↓
Normalized error created
    ↓
Current state and authority validated
    ↓
State transition accepted where appropriate
    ↓
Recovery guidance returned
    ↓
Safe logging / metrics / event
```

The model preserves these distinctions:

```text
Error
    ≠ Warning
    ≠ Cancellation
    ≠ Revocation
    ≠ Removal
    ≠ Uncertain Outcome
```

The architecture guarantees:

- raw secret material never appears in errors;
- raw backend and provider exceptions remain internal;
- retry is guided but not scheduled by errors;
- uncertain non-idempotent outcomes require reconciliation;
- backend lock maps to user action, not missing secret;
- validation infrastructure failure differs from invalid material;
- failed rotation or migration preserves the last known good revision where possible;
- lease consumer and purpose mismatches are critical security violations;
- exposure attempts fail closed;
- deletion assurance is never overstated;
- state remains authoritative;
- warnings remain bounded and safe;
- unknown future error codes are supported.

This document is the error source of truth for Secret Management implementation and README documentation.
