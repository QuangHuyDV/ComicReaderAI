# Provider Management Errors

> **Project:** CRAI  
> **Module:** Provider Management  
> **Document:** Errors and Warnings  
> **Path:** `02-modules/provider-management/ERRORS.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-04  
> **Source of Truth:**
>
> - `02-modules/provider-management/MODULE.md`
> - `02-modules/provider-management/CONTRACT.md`
> - `02-modules/provider-management/STATES.md`
> - `02-modules/provider-management/EVENTS.md`

---

## 1. Purpose

This document defines normalized errors and warnings owned by the Provider Management module.

It specifies:

- error structure;
- error identifiers;
- error categories;
- error scopes;
- severity;
- retryability;
- recovery actions;
- provider registration and configuration errors;
- provider model errors;
- capability and eligibility errors;
- provider selection errors;
- provider lease errors;
- credential availability and resolution errors;
- provider client lifecycle errors;
- availability and health errors;
- rate-limit and quota errors;
- circuit-breaker errors;
- local model lifecycle errors;
- usage and feedback errors;
- concurrency and persistence errors;
- security and privacy errors;
- state-transition implications;
- public and internal error boundaries;
- logging and observability rules.

This document does not define:

- provider-native error payloads;
- Translation errors;
- Recognition errors;
- Runtime worker errors;
- UI wording;
- HTTP mappings;
- database schemas;
- alert thresholds;
- implementation exception classes;
- raw secret-manager failures.

---

## 2. Error Design Goals

Provider Management error handling must:

- remain provider-neutral;
- remain capability-neutral;
- distinguish expected rejection from unexpected failure;
- distinguish provider-management state from Runtime execution state;
- preserve stable provider identity;
- avoid exposing credentials;
- prevent invalid provider selection;
- prevent invalid lease use;
- support fallback discovery;
- preserve local and remote provider compatibility;
- support machine-readable recovery actions;
- prevent stale configuration from overwriting current state;
- separate provider-health evidence from unrelated domain failures;
- support safe retry and administrative recovery;
- remain compatible with future providers and capabilities.

---

## 3. Error Versus Warning

An error prevents an operation from being accepted or completed as intended.

A warning describes a usable but degraded condition.

```text
Error
    → operation rejected, failed, or cannot safely continue

Warning
    → operation remains usable with limitations
```

Examples:

```text
No eligible provider satisfies a mandatory privacy requirement
    → Error

Preferred provider unavailable, fallback provider selected
    → Warning

Provider model is deprecated but explicit use remains permitted
    → Warning

Provider lease handle cannot be created
    → Error
```

---

## 4. Error Versus Lifecycle Outcome

The following lifecycle outcomes are not automatically system errors:

```text
Provider disabled
Provider archived
Model deprecated
Lease released
Lease expired
Lease revoked
Selection rejected
Circuit opened
Local model unloaded
```

They may carry reason codes and operational consequences.

A lifecycle outcome becomes an error only when it violates the requested operation or indicates unexpected system failure.

---

## 5. Error Ownership

Provider Management owns normalized errors concerning:

- provider definition validation;
- provider configuration validation;
- provider model validation;
- capability metadata;
- eligibility;
- provider selection;
- provider lease creation and lifecycle;
- execution-handle creation;
- credential-reference availability;
- provider client lifecycle;
- provider availability;
- provider health evaluation;
- rate-limit normalization;
- circuit-breaker transitions;
- local model registration and lifecycle;
- provider usage normalization;
- provider outcome feedback acceptance;
- Provider Management persistence;
- Provider Management event publication;
- provider-policy enforcement.

Provider Management does not own original errors concerning:

- Translation semantics;
- Translation alignment;
- Recognition semantics;
- Reading Session authority;
- Presentation rendering;
- Runtime scheduling;
- Runtime worker crashes;
- generic network-stack implementation outside provider adapters;
- secret storage internals;
- source acquisition;
- OCR region extraction.

Errors received from other modules may be normalized at the Provider Management boundary only when they affect Provider Management contracts or state.

---

## 6. Error Ownership Matrix

| Failure source | Original owner | Provider Management responsibility |
|---|---|---|
| Invalid Translation batch | Translation | Do not convert into provider-health failure |
| Runtime worker crash | Runtime | Record lease impact when relevant |
| Provider timeout | Provider adapter / Provider Management | Normalize and apply health/circuit evidence |
| Credential missing | Secret boundary / Provider Management | Normalize credential availability |
| Provider disabled | Provider Management | Reject selection or lease |
| Model removed | Provider Management | Reject new lease |
| Local RAM admission denied | Runtime / Resource Management | Normalize as resource-constrained availability |
| Local model load failed | Provider Management | Normalize and transition LocalModel state |
| Reading Session rejected stale result | Reading Session | Do not affect provider health |
| Presentation failed to render | Presentation | Do not affect provider health |
| Provider event publication failed | Provider Management infrastructure | Retry publication safely |
| Provider result validation failed semantically | Consumer module | Apply health evidence only when provider-relevant |

---

# Part I — Normalized Error Contract

## 7. ProviderManagementError

```text
ProviderManagementError {
    errorId

    code
    category
    scope
    severity

    message
    userMessageKey

    retryability
    recoveryActions[]

    providerId
    providerRevision
    providerConfigurationRevision

    providerModelId
    providerModelRevision

    providerSelectionId
    providerLeaseId
    providerCircuitId
    localModelInstanceId
    providerClientInstanceId
    providerCredentialReferenceId

    consumerModule
    operationReference
    capability

    state
    expectedState
    actualState
    stateRevision

    cause
    providerReference

    occurredAt
    metadata
}
```

Not every field is present for every error.

Raw secrets and domain content must never appear.

---

## 8. errorId

Uniquely identifies one normalized error occurrence.

It supports:

- event correlation;
- logs;
- diagnostics;
- support requests;
- deduplication;
- retry analysis;
- audit history.

The same failure published through multiple channels should retain the same `errorId` where practical.

---

## 9. code

A stable machine-readable code.

Naming convention:

```text
PROVIDER_MANAGEMENT_<CONCERN>_<CONDITION>
```

Examples:

```text
PROVIDER_MANAGEMENT_PROVIDER_NOT_FOUND
PROVIDER_MANAGEMENT_SELECTION_NO_ELIGIBLE_PROVIDER
PROVIDER_MANAGEMENT_LEASE_EXPIRED
PROVIDER_MANAGEMENT_CREDENTIAL_UNAVAILABLE
PROVIDER_MANAGEMENT_LOCAL_MODEL_LOAD_FAILED
```

Warnings use:

```text
PROVIDER_MANAGEMENT_WARNING_<CONDITION>
```

---

## 10. category

Canonical categories:

```text
COMMAND_VALIDATION
PROVIDER_DEFINITION
PROVIDER_CONFIGURATION
PROVIDER_MODEL
CAPABILITY
ELIGIBILITY
SELECTION
LEASE
EXECUTION_HANDLE
CREDENTIAL
CLIENT_LIFECYCLE
AVAILABILITY
HEALTH
RATE_LIMIT
QUOTA
CIRCUIT_BREAKER
LOCAL_MODEL
RESOURCE
USAGE
OUTCOME_FEEDBACK
STATE_TRANSITION
CONCURRENCY
PERSISTENCE
EVENT_PUBLICATION
SECURITY
PRIVACY
INTERNAL
```

---

## 11. scope

Canonical scopes:

```text
COMMAND
PROVIDER
PROVIDER_MODEL
CAPABILITY
SELECTION
LEASE
EXECUTION_HANDLE
CREDENTIAL_REFERENCE
CLIENT_INSTANCE
AVAILABILITY
HEALTH
RATE_LIMIT
QUOTA
CIRCUIT
LOCAL_MODEL
RESOURCE_ADMISSION
USAGE_RECORD
OUTCOME_FEEDBACK
MODULE
```

One error may affect multiple entities but must have one primary scope.

---

## 12. severity

Canonical values:

```text
NOTICE
DEGRADED
ERROR
CRITICAL
```

### NOTICE

The operation was safely rejected or a normal lifecycle condition was reported.

### DEGRADED

The system remains usable but with reduced provider options or capability.

### ERROR

The requested operation could not complete.

### CRITICAL

A severe integrity, security, or persistent consistency defect occurred.

`CRITICAL` should be rare.

---

## 13. retryability

```text
Retryability {
    retryable
    recommendedRetryScope
    requiresConfigurationChange
    requiresCredentialChange
    requiresPolicyChange
    advisoryRetryAfter
    advisoryMaximumAdditionalAttempts
}
```

Possible retry scopes:

```text
NONE
SAME_SELECTION
NEW_SELECTION
SAME_PROVIDER
ANOTHER_PROVIDER
SAME_LEASE
NEW_LEASE
NEW_CLIENT
NEW_LOCAL_MODEL_INSTANCE
AFTER_RESOURCE_ADMISSION
AFTER_CREDENTIAL_REFRESH
AFTER_CIRCUIT_PROBE
MANUAL_ONLY
```

Advisory fields do not override Runtime retry budgets or timing.

---

## 14. recoveryActions

Canonical actions:

```text
RETRY
REQUEST_NEW_SELECTION
REQUEST_NEW_LEASE
RELEASE_LEASE
SELECT_ANOTHER_PROVIDER
SELECT_ANOTHER_MODEL
ENABLE_PROVIDER
ENABLE_MODEL
UPDATE_PROVIDER_CONFIGURATION
REFRESH_CAPABILITIES
REFRESH_MODEL_CATALOG
REFRESH_HEALTH
WAIT_AND_RETRY
RESET_CIRCUIT
WAIT_FOR_CIRCUIT_PROBE
UPDATE_CREDENTIALS
ROTATE_CREDENTIAL_REFERENCE
USE_LOCAL_PROVIDER
USE_REMOTE_PROVIDER
CHANGE_REGION
CHANGE_PRIVACY_POLICY
CHANGE_PROVIDER_POLICY
REDUCE_RESOURCE_REQUIREMENTS
REQUEST_RUNTIME_ADMISSION
INSTALL_LOCAL_MODEL
VALIDATE_LOCAL_MODEL
LOAD_LOCAL_MODEL
UNLOAD_LOCAL_MODEL
REINSTALL_LOCAL_MODEL
RECREATE_PROVIDER_CLIENT
ARCHIVE_PROVIDER
CONTACT_SUPPORT
NONE
```

These are recommendations, not automatic commands.

---

## 15. ProviderErrorReference

```text
ProviderErrorReference {
    providerId
    providerModelId

    providerRequestId
    normalizedProviderCode
    providerHttpStatus

    providerRetryAfter
    providerRegion
}
```

It must not contain:

- API keys;
- access tokens;
- raw headers;
- private endpoint credentials;
- raw response bodies;
- provider SDK objects.

---

## 16. ErrorCause

```text
ErrorCause {
    code
    category
    message
}
```

Public cause chains should remain shallow.

Full stacks remain internal.

---

## 17. metadata

Allowed examples:

```text
candidateCount
eligibleCandidateCount
activeLeaseCount
configuredLeaseLimit
expectedStateRevision
actualStateRevision
retryAfter
resetAt
resourceClass
modelSizeClass
healthEvidenceCount
failureThreshold
providerLatencyClass
```

Prohibited examples:

```text
apiKey
accessToken
authorizationHeader
clientSecret
privateKey
sourceText
translatedText
recognizedText
rawProviderResponse
fullProviderPrompt
privateModelPath
```

---

# Part II — Error Code Stability

## 18. Stability Rule

Once a code appears in:

- public contracts;
- events;
- telemetry;
- persisted failures;
- administrative workflows;

its semantic meaning must not change.

A new meaning requires a new code.

---

## 19. Unknown Error

Consumers must support:

```text
PROVIDER_MANAGEMENT_UNKNOWN_ERROR
```

Unknown failures may normalize temporarily to this code while preserving internal diagnostics.

---

# Part III — Command Validation Errors

## 20. PROVIDER_MANAGEMENT_COMMAND_INVALID

The command is malformed or internally inconsistent.

```text
category: COMMAND_VALIDATION
scope: COMMAND
severity: NOTICE
retryable: false
```

Examples:

- required fields missing;
- mutually exclusive selection modes;
- invalid provider state expectation;
- invalid lease duration;
- invalid disable policy;
- invalid local-model transition request.

---

## 21. PROVIDER_MANAGEMENT_IDEMPOTENCY_CONFLICT

The same idempotency key was used with different semantic input.

```text
category: COMMAND_VALIDATION
scope: COMMAND
severity: NOTICE
retryable: false
```

Recovery:

```text
Use a new idempotency key or resend the original equivalent command.
```

---

## 22. PROVIDER_MANAGEMENT_OPERATION_REFERENCE_INVALID

The consumer operation reference is missing or malformed where required.

```text
category: COMMAND_VALIDATION
scope: COMMAND
severity: NOTICE
retryable: false
```

The reference must remain opaque and content-free.

---

## 23. PROVIDER_MANAGEMENT_CAPABILITY_REQUIRED

No capability was specified for a selection or lease request.

```text
category: COMMAND_VALIDATION
scope: COMMAND
severity: NOTICE
retryable: false
```

---

## 24. PROVIDER_MANAGEMENT_LEASE_DURATION_INVALID

The requested lease duration violates configured limits.

```text
category: COMMAND_VALIDATION
scope: COMMAND
severity: NOTICE
retryable: false
```

Recovery:

```text
Request a lease duration within allowed bounds.
```

---

# Part IV — Provider Definition Errors

## 25. PROVIDER_MANAGEMENT_PROVIDER_NOT_FOUND

The referenced provider does not exist.

```text
category: PROVIDER_DEFINITION
scope: PROVIDER
severity: NOTICE or ERROR
retryable: false
```

---

## 26. PROVIDER_MANAGEMENT_PROVIDER_ALREADY_EXISTS

A provider with the same stable identity already exists.

```text
category: PROVIDER_DEFINITION
scope: PROVIDER
severity: NOTICE
retryable: false
```

Equivalent idempotent registration may return the existing provider.

---

## 27. PROVIDER_MANAGEMENT_PROVIDER_ARCHIVED

The operation targeted an archived provider.

```text
category: PROVIDER_DEFINITION
scope: PROVIDER
severity: NOTICE
retryable: false
```

An archived provider cannot be enabled or leased.

---

## 28. PROVIDER_MANAGEMENT_PROVIDER_DISABLED

The provider is disabled and cannot participate in new selection or lease creation.

```text
category: PROVIDER_DEFINITION
scope: PROVIDER
severity: NOTICE
retryable: conditional
```

Recovery:

```text
ENABLE_PROVIDER
SELECT_ANOTHER_PROVIDER
```

---

## 29. PROVIDER_MANAGEMENT_PROVIDER_ENABLEMENT_FAILED

The provider could not be enabled because required validation failed.

```text
category: PROVIDER_CONFIGURATION
scope: PROVIDER
severity: ERROR
retryable: conditional
```

Possible causes:

- invalid adapter binding;
- missing required model;
- invalid configuration;
- credential requirement unresolved;
- policy conflict.

---

## 30. PROVIDER_MANAGEMENT_PROVIDER_DISABLE_CONFLICT

The provider could not be disabled under the requested policy.

```text
category: STATE_TRANSITION
scope: PROVIDER
severity: ERROR
retryable: conditional
```

Examples:

- active leases exist but immediate revocation is prohibited;
- archival requested while leases remain active;
- stale state revision.

---

## 31. PROVIDER_MANAGEMENT_PROVIDER_ARCHIVE_CONFLICT

The provider cannot be archived safely.

```text
category: STATE_TRANSITION
scope: PROVIDER
severity: ERROR
retryable: conditional
```

Recommended recovery:

```text
Disable provider
Drain or revoke leases
Retry archive
```

---

# Part V — Provider Configuration Errors

## 32. PROVIDER_MANAGEMENT_CONFIGURATION_INVALID

Provider configuration is malformed or inconsistent.

```text
category: PROVIDER_CONFIGURATION
scope: PROVIDER
severity: ERROR
retryable: false without configuration change
```

---

## 33. PROVIDER_MANAGEMENT_CONFIGURATION_REVISION_NOT_FOUND

The requested immutable provider configuration revision cannot be resolved.

```text
category: PROVIDER_CONFIGURATION
scope: PROVIDER
severity: ERROR
retryable: false for the same revision
```

---

## 34. PROVIDER_MANAGEMENT_CONFIGURATION_REVISION_MISMATCH

An operation used a stale provider configuration revision.

```text
category: CONCURRENCY
scope: PROVIDER
severity: NOTICE or ERROR
retryable: true after reload
```

---

## 35. PROVIDER_MANAGEMENT_ADAPTER_BINDING_INVALID

The provider configuration references an unavailable or incompatible adapter.

```text
category: PROVIDER_CONFIGURATION
scope: PROVIDER
severity: ERROR
retryable: false without configuration change
```

---

## 36. PROVIDER_MANAGEMENT_REGION_CONFIGURATION_INVALID

Provider region configuration is missing, unsupported, or contradictory.

```text
category: PROVIDER_CONFIGURATION
scope: PROVIDER
severity: ERROR
retryable: false without configuration change
```

---

## 37. PROVIDER_MANAGEMENT_ENDPOINT_CONFIGURATION_INVALID

The provider endpoint configuration is invalid.

```text
category: PROVIDER_CONFIGURATION
scope: PROVIDER
severity: ERROR
retryable: false without configuration change
```

Public errors must not expose secret-bearing endpoint data.

---

# Part VI — Provider Model Errors

## 38. PROVIDER_MANAGEMENT_MODEL_NOT_FOUND

The referenced provider model does not exist.

```text
category: PROVIDER_MODEL
scope: PROVIDER_MODEL
severity: NOTICE or ERROR
retryable: false
```

---

## 39. PROVIDER_MANAGEMENT_MODEL_ALREADY_EXISTS

A model with the same CRAI identity already exists.

```text
category: PROVIDER_MODEL
scope: PROVIDER_MODEL
severity: NOTICE
retryable: false
```

---

## 40. PROVIDER_MANAGEMENT_MODEL_NOT_ACTIVE

The model is not eligible for normal new selection.

```text
category: PROVIDER_MODEL
scope: PROVIDER_MODEL
severity: NOTICE
retryable: conditional
```

Possible states:

```text
REGISTERED
DEPRECATED
DISABLED
REMOVED
```

---

## 41. PROVIDER_MANAGEMENT_MODEL_DEPRECATED

The selected model is deprecated.

```text
category: PROVIDER_MODEL
scope: PROVIDER_MODEL
severity: DEGRADED
retryable: true with another model
```

This may be a warning when explicit use is allowed.

---

## 42. PROVIDER_MANAGEMENT_MODEL_DISABLED

The model is disabled.

```text
category: PROVIDER_MODEL
scope: PROVIDER_MODEL
severity: NOTICE
retryable: conditional
```

---

## 43. PROVIDER_MANAGEMENT_MODEL_REMOVED

The model identity remains historical but cannot be used.

```text
category: PROVIDER_MODEL
scope: PROVIDER_MODEL
severity: NOTICE
retryable: false
```

---

## 44. PROVIDER_MANAGEMENT_MODEL_REVISION_MISMATCH

The model metadata revision no longer matches the selection or lease request.

```text
category: CONCURRENCY
scope: PROVIDER_MODEL
severity: NOTICE or ERROR
retryable: true with new selection
```

---

# Part VII — Capability Errors

## 45. PROVIDER_MANAGEMENT_CAPABILITY_NOT_FOUND

No capability declaration exists for the requested provider path.

```text
category: CAPABILITY
scope: CAPABILITY
severity: ERROR
retryable: conditional
```

Recovery:

```text
REFRESH_CAPABILITIES
SELECT_ANOTHER_PROVIDER
```

---

## 46. PROVIDER_MANAGEMENT_CAPABILITY_UNSUPPORTED

The provider path explicitly does not support the requested capability.

```text
category: CAPABILITY
scope: CAPABILITY
severity: NOTICE
retryable: true with another provider or model
```

---

## 47. PROVIDER_MANAGEMENT_CAPABILITY_UNKNOWN

Capability support is unknown and policy does not permit optimistic use.

```text
category: CAPABILITY
scope: CAPABILITY
severity: NOTICE
retryable: conditional
```

---

## 48. PROVIDER_MANAGEMENT_CAPABILITY_LIMIT_EXCEEDED

The request exceeds declared provider capability limits.

```text
category: CAPABILITY
scope: CAPABILITY
severity: ERROR
retryable: true after requirement change
```

Examples:

- context too large;
- output limit too small;
- segment count exceeded;
- image dimensions unsupported;
- unsupported language pair.

---

## 49. PROVIDER_MANAGEMENT_CAPABILITY_SNAPSHOT_STALE

The capability snapshot used by selection or lease creation is stale.

```text
category: CAPABILITY
scope: CAPABILITY
severity: NOTICE
retryable: true
```

Recovery:

```text
REFRESH_CAPABILITIES
REQUEST_NEW_SELECTION
```

---

## 50. PROVIDER_MANAGEMENT_LANGUAGE_PAIR_UNSUPPORTED

No selected provider path supports the required language pair.

```text
category: CAPABILITY
scope: CAPABILITY
severity: NOTICE
retryable: true with another provider or model
```

---

## 51. PROVIDER_MANAGEMENT_STRUCTURED_OUTPUT_UNSUPPORTED

The provider path cannot satisfy mandatory structured output.

```text
category: CAPABILITY
scope: CAPABILITY
severity: NOTICE
retryable: true with another provider
```

---

## 52. PROVIDER_MANAGEMENT_STREAMING_UNSUPPORTED

The requested streaming behavior is unsupported.

```text
category: CAPABILITY
scope: CAPABILITY
severity: NOTICE or DEGRADED
retryable: conditional
```

If streaming is only preferred, selection may continue with a warning.

---

# Part VIII — Eligibility Errors

## 53. PROVIDER_MANAGEMENT_PROVIDER_INELIGIBLE

A provider path failed one or more mandatory eligibility checks.

```text
category: ELIGIBILITY
scope: PROVIDER
severity: NOTICE
retryable: conditional
```

The error should include normalized failed constraints.

---

## 54. PROVIDER_MANAGEMENT_PRIVACY_REQUIREMENT_UNSATISFIED

No provider path satisfies the mandatory privacy requirement.

```text
category: PRIVACY
scope: SELECTION
severity: ERROR
retryable: false without policy or provider change
```

---

## 55. PROVIDER_MANAGEMENT_LOCALITY_REQUIREMENT_UNSATISFIED

No provider path satisfies the required locality.

```text
category: ELIGIBILITY
scope: SELECTION
severity: ERROR
retryable: false without policy or provider change
```

---

## 56. PROVIDER_MANAGEMENT_REGION_REQUIREMENT_UNSATISFIED

No provider path satisfies the required region policy.

```text
category: PRIVACY
scope: SELECTION
severity: ERROR
retryable: false without region or provider change
```

---

## 57. PROVIDER_MANAGEMENT_CREDENTIAL_REQUIREMENT_UNSATISFIED

The provider path requires unavailable credentials.

```text
category: CREDENTIAL
scope: SELECTION
severity: ERROR
retryable: conditional
```

---

## 58. PROVIDER_MANAGEMENT_RESOURCE_REQUIREMENT_UNSATISFIED

A local provider path requires unavailable resources.

```text
category: RESOURCE
scope: SELECTION
severity: ERROR
retryable: true after Runtime admission or another provider
```

---

# Part IX — Selection Errors

## 59. PROVIDER_MANAGEMENT_SELECTION_NO_ELIGIBLE_PROVIDER

No provider path satisfies all mandatory constraints.

```text
category: SELECTION
scope: SELECTION
severity: ERROR
retryable: conditional
```

Recovery actions may include:

```text
SELECT_ANOTHER_PROVIDER
CHANGE_PROVIDER_POLICY
CHANGE_PRIVACY_POLICY
CHANGE_REGION
USE_LOCAL_PROVIDER
USE_REMOTE_PROVIDER
WAIT_AND_RETRY
```

---

## 60. PROVIDER_MANAGEMENT_SELECTION_REQUIRED_PROVIDER_UNAVAILABLE

The explicitly required provider is unavailable.

```text
category: SELECTION
scope: SELECTION
severity: ERROR
retryable: conditional
```

Provider Management must not silently choose another provider.

---

## 61. PROVIDER_MANAGEMENT_SELECTION_REQUIRED_MODEL_UNAVAILABLE

The explicitly required model is unavailable.

```text
category: SELECTION
scope: SELECTION
severity: ERROR
retryable: conditional
```

---

## 62. PROVIDER_MANAGEMENT_SELECTION_POLICY_INVALID

The selection policy is internally inconsistent.

```text
category: SELECTION
scope: COMMAND
severity: NOTICE
retryable: false
```

Example:

```text
LOCAL_REQUIRED
+
REMOTE_ONLY
```

---

## 63. PROVIDER_MANAGEMENT_SELECTION_RESULT_STALE

The selected provider path is no longer eligible when lease creation begins.

```text
category: SELECTION
scope: SELECTION
severity: NOTICE
retryable: true with new selection
```

Possible causes:

- provider disabled;
- model disabled;
- circuit opened;
- credential revoked;
- capability changed;
- rate limit changed;
- configuration revision changed.

---

## 64. PROVIDER_MANAGEMENT_SELECTION_EXPLANATION_UNAVAILABLE

The selection succeeded but explainability detail could not be produced.

```text
category: SELECTION
scope: SELECTION
severity: DEGRADED
retryable: true
```

Selection may remain valid when explanation is optional.

---

## 65. PROVIDER_MANAGEMENT_SELECTION_SCORING_FAILED

Internal ranking failed after eligibility filtering.

```text
category: INTERNAL
scope: SELECTION
severity: ERROR
retryable: true
```

---

# Part X — Lease Errors

## 66. PROVIDER_MANAGEMENT_LEASE_NOT_FOUND

The referenced lease does not exist.

```text
category: LEASE
scope: LEASE
severity: NOTICE or ERROR
retryable: false
```

---

## 67. PROVIDER_MANAGEMENT_LEASE_ALREADY_TERMINAL

A state-changing command targeted a terminal lease.

```text
category: LEASE
scope: LEASE
severity: NOTICE
retryable: false
```

Terminal states:

```text
RELEASED
EXPIRED
REVOKED
REJECTED
FAILED
```

---

## 68. PROVIDER_MANAGEMENT_LEASE_REJECTED

A valid lease request was denied.

```text
category: LEASE
scope: LEASE
severity: NOTICE
retryable: conditional
```

This maps to:

```text
Lease → REJECTED
```

---

## 69. PROVIDER_MANAGEMENT_LEASE_CREATION_FAILED

Lease creation failed unexpectedly.

```text
category: LEASE
scope: LEASE
severity: ERROR
retryable: true
```

---

## 70. PROVIDER_MANAGEMENT_LEASE_LIMIT_EXCEEDED

The provider, model, credential, capability, or consumer reached a lease limit.

```text
category: LEASE
scope: LEASE
severity: ERROR
retryable: true after capacity becomes available
```

---

## 71. PROVIDER_MANAGEMENT_LEASE_EXPIRED

The lease expired.

```text
category: LEASE
scope: LEASE
severity: NOTICE
retryable: false for the same lease
```

Recovery:

```text
REQUEST_NEW_LEASE
```

---

## 72. PROVIDER_MANAGEMENT_LEASE_REVOKED

The lease was revoked.

```text
category: LEASE
scope: LEASE
severity: NOTICE or ERROR
retryable: conditional
```

The severity depends on whether revocation was administrative, security-related, or unexpected.

---

## 73. PROVIDER_MANAGEMENT_LEASE_ACTIVATION_NOT_ALLOWED

The lease cannot enter `ACTIVE`.

```text
category: STATE_TRANSITION
scope: LEASE
severity: NOTICE or ERROR
retryable: conditional
```

Possible causes:

- lease not in `GRANTED`;
- lease expired;
- provider disabled;
- handle unavailable;
- Runtime work reference invalid.

---

## 74. PROVIDER_MANAGEMENT_LEASE_RELEASE_FAILED

Normal lease release could not complete.

```text
category: LEASE
scope: LEASE
severity: ERROR
retryable: true
```

The lease may remain `RELEASE_REQUESTED` until cleanup succeeds.

---

## 75. PROVIDER_MANAGEMENT_LEASE_STATE_CONFLICT

Concurrent lease operations conflict.

```text
category: CONCURRENCY
scope: LEASE
severity: NOTICE or ERROR
retryable: true after reload
```

Examples:

- release raced with revoke;
- activation raced with expiration;
- duplicate grant;
- stale state revision.

---

# Part XI — Execution Handle Errors

## 76. PROVIDER_MANAGEMENT_EXECUTION_HANDLE_NOT_FOUND

The lease references no resolvable execution handle.

```text
category: EXECUTION_HANDLE
scope: EXECUTION_HANDLE
severity: ERROR
retryable: true
```

A granted lease must not remain publicly usable in this condition.

---

## 77. PROVIDER_MANAGEMENT_EXECUTION_HANDLE_CREATION_FAILED

The provider-neutral handle could not be created.

```text
category: EXECUTION_HANDLE
scope: EXECUTION_HANDLE
severity: ERROR
retryable: conditional
```

---

## 78. PROVIDER_MANAGEMENT_EXECUTION_HANDLE_EXPIRED

The handle was used after lease expiration.

```text
category: EXECUTION_HANDLE
scope: EXECUTION_HANDLE
severity: NOTICE
retryable: false for the same handle
```

---

## 79. PROVIDER_MANAGEMENT_EXECUTION_HANDLE_REVOKED

The handle was used after lease revocation.

```text
category: EXECUTION_HANDLE
scope: EXECUTION_HANDLE
severity: NOTICE or ERROR
retryable: false for the same handle
```

---

## 80. PROVIDER_MANAGEMENT_EXECUTION_HANDLE_CAPABILITY_MISMATCH

The consumer attempted to use a handle outside its granted capability.

```text
category: SECURITY
scope: EXECUTION_HANDLE
severity: CRITICAL
retryable: false
```

This indicates a contract or security violation.

---

## 81. PROVIDER_MANAGEMENT_EXECUTION_HANDLE_CONSUMER_MISMATCH

A consumer attempted to use another consumer's lease or handle.

```text
category: SECURITY
scope: EXECUTION_HANDLE
severity: CRITICAL
retryable: false
```

---

# Part XII — Credential Errors

## 82. PROVIDER_MANAGEMENT_CREDENTIAL_REFERENCE_NOT_FOUND

The configured credential reference does not exist.

```text
category: CREDENTIAL
scope: CREDENTIAL_REFERENCE
severity: ERROR
retryable: false until configuration changes
```

---

## 83. PROVIDER_MANAGEMENT_CREDENTIAL_UNAVAILABLE

The credential reference exists but cannot currently be resolved.

```text
category: CREDENTIAL
scope: CREDENTIAL_REFERENCE
severity: ERROR
retryable: conditional
```

---

## 84. PROVIDER_MANAGEMENT_CREDENTIAL_EXPIRED

The provider credential is expired.

```text
category: CREDENTIAL
scope: CREDENTIAL_REFERENCE
severity: ERROR
retryable: true after refresh or rotation
```

---

## 85. PROVIDER_MANAGEMENT_CREDENTIAL_REVOKED

The provider credential has been revoked.

```text
category: CREDENTIAL
scope: CREDENTIAL_REFERENCE
severity: ERROR
retryable: false until replacement
```

Active leases may require revocation.

---

## 86. PROVIDER_MANAGEMENT_CREDENTIAL_LOCKED

Credential access is temporarily locked.

```text
category: CREDENTIAL
scope: CREDENTIAL_REFERENCE
severity: ERROR
retryable: conditional
```

---

## 87. PROVIDER_MANAGEMENT_CREDENTIAL_RESOLUTION_FAILED

The approved secret boundary failed to resolve credential material.

```text
category: CREDENTIAL
scope: CREDENTIAL_REFERENCE
severity: ERROR
retryable: true
```

Raw secret-manager errors must remain internal.

---

## 88. PROVIDER_MANAGEMENT_CREDENTIAL_PROVIDER_MISMATCH

A credential reference is not valid for the selected provider.

```text
category: CREDENTIAL
scope: CREDENTIAL_REFERENCE
severity: ERROR
retryable: false without configuration change
```

---

## 89. PROVIDER_MANAGEMENT_CREDENTIAL_EXPOSURE_BLOCKED

An operation attempted to expose raw credentials through a contract, event, result, or log.

```text
category: SECURITY
scope: MODULE
severity: CRITICAL
retryable: false until corrected
```

---

# Part XIII — Provider Client Lifecycle Errors

## 90. PROVIDER_MANAGEMENT_CLIENT_CREATION_FAILED

A provider client instance could not be created.

```text
category: CLIENT_LIFECYCLE
scope: CLIENT_INSTANCE
severity: ERROR
retryable: conditional
```

Possible causes:

- invalid adapter;
- credential resolution failure;
- endpoint initialization failure;
- SDK initialization failure;
- unsupported configuration.

---

## 91. PROVIDER_MANAGEMENT_CLIENT_NOT_READY

A lease attempted to bind to a client that is not ready.

```text
category: CLIENT_LIFECYCLE
scope: CLIENT_INSTANCE
severity: ERROR
retryable: true
```

---

## 92. PROVIDER_MANAGEMENT_CLIENT_REPLACEMENT_FAILED

A provider client could not be safely replaced.

```text
category: CLIENT_LIFECYCLE
scope: CLIENT_INSTANCE
severity: ERROR
retryable: true
```

---

## 93. PROVIDER_MANAGEMENT_CLIENT_DISPOSAL_FAILED

Client disposal failed.

```text
category: CLIENT_LIFECYCLE
scope: CLIENT_INSTANCE
severity: DEGRADED or ERROR
retryable: true
```

This may be operationally degraded without invalidating completed leases.

---

## 94. PROVIDER_MANAGEMENT_CLIENT_CONFIGURATION_STALE

A client instance uses an obsolete provider configuration revision.

```text
category: CLIENT_LIFECYCLE
scope: CLIENT_INSTANCE
severity: DEGRADED
retryable: true with replacement
```

---

# Part XIV — Availability Errors

## 95. PROVIDER_MANAGEMENT_AVAILABILITY_UNKNOWN

Current availability cannot be determined.

```text
category: AVAILABILITY
scope: AVAILABILITY
severity: NOTICE or DEGRADED
retryable: true
```

Selection policy should treat unknown state conservatively.

---

## 96. PROVIDER_MANAGEMENT_PROVIDER_UNAVAILABLE

The provider cannot accept new work.

```text
category: AVAILABILITY
scope: PROVIDER
severity: ERROR
retryable: true
```

---

## 97. PROVIDER_MANAGEMENT_PROVIDER_DRAINING

The provider is draining and cannot accept new leases.

```text
category: AVAILABILITY
scope: PROVIDER
severity: NOTICE
retryable: conditional
```

---

## 98. PROVIDER_MANAGEMENT_PROVIDER_MAINTENANCE

The provider is in maintenance mode.

```text
category: AVAILABILITY
scope: PROVIDER
severity: NOTICE
retryable: true after maintenance
```

---

## 99. PROVIDER_MANAGEMENT_PROVIDER_RESOURCE_CONSTRAINED

The provider path is blocked by current resource pressure.

```text
category: RESOURCE
scope: AVAILABILITY
severity: ERROR
retryable: true
```

---

# Part XV — Health Errors

## 100. PROVIDER_MANAGEMENT_HEALTH_EVIDENCE_INVALID

Submitted health evidence is malformed, stale, or unrelated.

```text
category: HEALTH
scope: HEALTH
severity: NOTICE
retryable: false
```

---

## 101. PROVIDER_MANAGEMENT_HEALTH_EVIDENCE_STALE

An older health observation attempted to overwrite newer health state.

```text
category: CONCURRENCY
scope: HEALTH
severity: NOTICE
retryable: false
```

---

## 102. PROVIDER_MANAGEMENT_HEALTH_EVALUATION_FAILED

Provider Management could not evaluate normalized health.

```text
category: HEALTH
scope: HEALTH
severity: ERROR
retryable: true
```

Availability may become `UNKNOWN`.

---

## 103. PROVIDER_MANAGEMENT_PROVIDER_UNHEALTHY

The provider path is currently unhealthy.

```text
category: HEALTH
scope: HEALTH
severity: ERROR
retryable: conditional
```

---

## 104. PROVIDER_MANAGEMENT_FALSE_HEALTH_ATTRIBUTION_BLOCKED

An unrelated consumer or Presentation failure was submitted as provider-health evidence.

```text
category: HEALTH
scope: OUTCOME_FEEDBACK
severity: NOTICE
retryable: false
```

The evidence must not affect health or circuit state.

---

# Part XVI — Rate Limit and Quota Errors

## 105. PROVIDER_MANAGEMENT_RATE_LIMITED

The provider path is temporarily rate-limited.

```text
category: RATE_LIMIT
scope: RATE_LIMIT
severity: ERROR
retryable: true
```

Normalized information may include:

```text
retryAfter
resetAt
rateLimitScope
```

---

## 106. PROVIDER_MANAGEMENT_QUOTA_EXCEEDED

Quota or account capacity is exhausted.

```text
category: QUOTA
scope: QUOTA
severity: ERROR
retryable: conditional
```

Fallback may remain possible.

---

## 107. PROVIDER_MANAGEMENT_RATE_LIMIT_STATE_INVALID

Provider-native rate-limit data could not be normalized safely.

```text
category: RATE_LIMIT
scope: RATE_LIMIT
severity: DEGRADED
retryable: true
```

Selection should behave conservatively.

---

## 108. PROVIDER_MANAGEMENT_RATE_LIMIT_REVISION_CONFLICT

Concurrent rate-limit updates conflict.

```text
category: CONCURRENCY
scope: RATE_LIMIT
severity: NOTICE
retryable: true after reload
```

---

# Part XVII — Circuit Breaker Errors

## 109. PROVIDER_MANAGEMENT_CIRCUIT_OPEN

The provider path is blocked by an open circuit.

```text
category: CIRCUIT_BREAKER
scope: CIRCUIT
severity: NOTICE or ERROR
retryable: true after probe window
```

---

## 110. PROVIDER_MANAGEMENT_CIRCUIT_NOT_FOUND

The referenced circuit scope does not exist.

```text
category: CIRCUIT_BREAKER
scope: CIRCUIT
severity: NOTICE
retryable: conditional
```

---

## 111. PROVIDER_MANAGEMENT_CIRCUIT_TRANSITION_INVALID

The requested circuit transition is invalid.

```text
category: STATE_TRANSITION
scope: CIRCUIT
severity: NOTICE
retryable: false
```

Example:

```text
CLOSED → HALF_OPEN
```

without first entering `OPEN`.

---

## 112. PROVIDER_MANAGEMENT_CIRCUIT_PROBE_REJECTED

A half-open probe could not be admitted.

```text
category: CIRCUIT_BREAKER
scope: CIRCUIT
severity: NOTICE
retryable: true
```

Possible causes:

- probe limit reached;
- Runtime admission denied;
- provider disabled;
- credentials unavailable.

---

## 113. PROVIDER_MANAGEMENT_CIRCUIT_EVIDENCE_INVALID

Failure or success evidence cannot be applied to the circuit scope.

```text
category: CIRCUIT_BREAKER
scope: OUTCOME_FEEDBACK
severity: NOTICE
retryable: false
```

---

# Part XVIII — Local Model Errors

## 114. PROVIDER_MANAGEMENT_LOCAL_MODEL_NOT_FOUND

The referenced local model instance does not exist.

```text
category: LOCAL_MODEL
scope: LOCAL_MODEL
severity: NOTICE or ERROR
retryable: false
```

---

## 115. PROVIDER_MANAGEMENT_LOCAL_MODEL_NOT_INSTALLED

The local model is registered but required files are not installed.

```text
category: LOCAL_MODEL
scope: LOCAL_MODEL
severity: ERROR
retryable: true after installation
```

---

## 116. PROVIDER_MANAGEMENT_LOCAL_MODEL_INSTALL_FAILED

Local model installation failed.

```text
category: LOCAL_MODEL
scope: LOCAL_MODEL
severity: ERROR
retryable: conditional
```

---

## 117. PROVIDER_MANAGEMENT_LOCAL_MODEL_VALIDATION_FAILED

Model integrity or compatibility validation failed.

```text
category: LOCAL_MODEL
scope: LOCAL_MODEL
severity: ERROR
retryable: conditional
```

Possible causes:

- corrupted files;
- incompatible runtime;
- missing tokenizer;
- unsupported device;
- invalid model metadata;
- adapter mismatch.

---

## 118. PROVIDER_MANAGEMENT_LOCAL_MODEL_LOAD_FAILED

The local model could not enter `READY`.

```text
category: LOCAL_MODEL
scope: LOCAL_MODEL
severity: ERROR
retryable: conditional
```

Recovery:

```text
REQUEST_RUNTIME_ADMISSION
REDUCE_RESOURCE_REQUIREMENTS
REINSTALL_LOCAL_MODEL
SELECT_ANOTHER_PROVIDER
```

---

## 119. PROVIDER_MANAGEMENT_LOCAL_MODEL_RESOURCE_ADMISSION_DENIED

Runtime or Resource Management denied required resources.

```text
category: RESOURCE
scope: RESOURCE_ADMISSION
severity: NOTICE or ERROR
retryable: true
```

This must not automatically mark provider health as unhealthy.

---

## 120. PROVIDER_MANAGEMENT_LOCAL_MODEL_BUSY

The local model is at or near capacity.

```text
category: RESOURCE
scope: LOCAL_MODEL
severity: DEGRADED or ERROR
retryable: true
```

---

## 121. PROVIDER_MANAGEMENT_LOCAL_MODEL_UNLOAD_FAILED

The model could not be unloaded safely.

```text
category: LOCAL_MODEL
scope: LOCAL_MODEL
severity: ERROR
retryable: true
```

---

## 122. PROVIDER_MANAGEMENT_LOCAL_MODEL_REMOVED

The local model was removed and cannot be loaded.

```text
category: LOCAL_MODEL
scope: LOCAL_MODEL
severity: NOTICE
retryable: false
```

---

## 123. PROVIDER_MANAGEMENT_LOCAL_MODEL_STATE_CONFLICT

Concurrent local model lifecycle operations conflict.

```text
category: CONCURRENCY
scope: LOCAL_MODEL
severity: NOTICE or ERROR
retryable: true after reload
```

Examples:

- load raced with unload;
- remove raced with validation;
- duplicate load;
- stale state revision.

---

## 124. PROVIDER_MANAGEMENT_LOCAL_MODEL_PATH_RESTRICTED

The configured local model path violates security policy.

```text
category: SECURITY
scope: LOCAL_MODEL
severity: CRITICAL
retryable: false until corrected
```

Public messages must not expose the restricted path.

---

# Part XIX — Usage and Outcome Feedback Errors

## 125. PROVIDER_MANAGEMENT_USAGE_RECORD_INVALID

Normalized usage data is malformed or internally inconsistent.

```text
category: USAGE
scope: USAGE_RECORD
severity: NOTICE
retryable: false
```

---

## 126. PROVIDER_MANAGEMENT_USAGE_RECORD_PERSISTENCE_FAILED

Provider usage could not be stored.

```text
category: PERSISTENCE
scope: USAGE_RECORD
severity: DEGRADED or ERROR
retryable: true
```

Execution may still be considered complete.

---

## 127. PROVIDER_MANAGEMENT_OUTCOME_FEEDBACK_INVALID

Submitted outcome feedback is malformed.

```text
category: OUTCOME_FEEDBACK
scope: OUTCOME_FEEDBACK
severity: NOTICE
retryable: false
```

---

## 128. PROVIDER_MANAGEMENT_OUTCOME_FEEDBACK_DUPLICATE

Equivalent feedback was already accepted.

```text
category: OUTCOME_FEEDBACK
scope: OUTCOME_FEEDBACK
severity: NOTICE
retryable: false
```

Idempotent handling should return the existing result.

---

## 129. PROVIDER_MANAGEMENT_OUTCOME_FEEDBACK_STALE

The feedback references an obsolete lease, client, model, or configuration revision.

```text
category: OUTCOME_FEEDBACK
scope: OUTCOME_FEEDBACK
severity: NOTICE
retryable: false
```

It may be retained for audit but not applied to current health state.

---

## 130. PROVIDER_MANAGEMENT_OUTCOME_NOT_PROVIDER_RELEVANT

Feedback was valid but not relevant to provider health or circuit state.

```text
category: OUTCOME_FEEDBACK
scope: OUTCOME_FEEDBACK
severity: NOTICE
retryable: false
```

Examples:

- Translation alignment failure;
- stale result rejection;
- Presentation failure;
- user cancellation;
- invalid source.

---

# Part XX — State and Concurrency Errors

## 131. PROVIDER_MANAGEMENT_STATE_CONFLICT

A transition failed because the entity was no longer in the expected state.

```text
category: CONCURRENCY
scope: MODULE
severity: NOTICE or ERROR
retryable: conditional
```

---

## 132. PROVIDER_MANAGEMENT_STATE_REVISION_CONFLICT

Expected state revision differs from stored revision.

```text
category: CONCURRENCY
scope: MODULE
severity: NOTICE
retryable: true
```

The caller should reload authoritative state.

---

## 133. PROVIDER_MANAGEMENT_DUPLICATE_ACTIVE_LEASE

Duplicate active authority exists where policy permits only one lease.

```text
category: CONCURRENCY
scope: LEASE
severity: CRITICAL
retryable: false until reconciliation
```

This invariant may be scoped by:

```text
consumer
operationReference
provider path
capability
```

---

## 134. PROVIDER_MANAGEMENT_DUPLICATE_LOCAL_MODEL_INSTANCE

More than one active local model instance exists where policy permits only one.

```text
category: CONCURRENCY
scope: LOCAL_MODEL
severity: CRITICAL
retryable: false until reconciliation
```

---

## 135. PROVIDER_MANAGEMENT_PROVIDER_DISABLE_LEASE_RACE

A lease grant raced with provider disablement.

```text
category: CONCURRENCY
scope: LEASE
severity: ERROR
retryable: true with new selection
```

No new valid lease may survive if disablement won the authoritative transition.

---

# Part XXI — Persistence Errors

## 136. PROVIDER_MANAGEMENT_PROVIDER_PERSISTENCE_FAILED

Provider definition or configuration could not be stored.

```text
category: PERSISTENCE
scope: PROVIDER
severity: CRITICAL
retryable: true
```

No lifecycle event may be published before durable persistence succeeds.

---

## 137. PROVIDER_MANAGEMENT_MODEL_PERSISTENCE_FAILED

Provider model metadata could not be stored.

```text
category: PERSISTENCE
scope: PROVIDER_MODEL
severity: ERROR
retryable: true
```

---

## 138. PROVIDER_MANAGEMENT_LEASE_PERSISTENCE_FAILED

Lease state could not be durably stored.

```text
category: PERSISTENCE
scope: LEASE
severity: CRITICAL
retryable: true
```

`ProviderLeaseGranted` must not be published.

---

## 139. PROVIDER_MANAGEMENT_HEALTH_PERSISTENCE_FAILED

Health projection could not be stored.

```text
category: PERSISTENCE
scope: HEALTH
severity: DEGRADED or ERROR
retryable: true
```

Selection should behave conservatively until reconciliation.

---

## 140. PROVIDER_MANAGEMENT_CIRCUIT_PERSISTENCE_FAILED

Circuit state could not be stored durably.

```text
category: PERSISTENCE
scope: CIRCUIT
severity: CRITICAL
retryable: true
```

The system must avoid reopening traffic accidentally.

---

## 141. PROVIDER_MANAGEMENT_LOCAL_MODEL_PERSISTENCE_FAILED

Local model lifecycle state could not be stored.

```text
category: PERSISTENCE
scope: LOCAL_MODEL
severity: ERROR
retryable: true
```

---

# Part XXII — Event Publication Errors

## 142. PROVIDER_MANAGEMENT_EVENT_PUBLICATION_FAILED

A committed transition could not be published to the Event Bus.

```text
category: EVENT_PUBLICATION
scope: MODULE
severity: ERROR
retryable: true
```

A transactional outbox or equivalent mechanism should retry publication.

The business transition must not be repeated as a new action.

---

## 143. PROVIDER_MANAGEMENT_EVENT_SEQUENCE_CONFLICT

An event could not receive a valid monotonic stream sequence.

```text
category: CONCURRENCY
scope: MODULE
severity: ERROR
retryable: true
```

---

## 144. PROVIDER_MANAGEMENT_EVENT_PAYLOAD_REJECTED

The event payload violated schema or security rules.

```text
category: EVENT_PUBLICATION
scope: MODULE
severity: ERROR
retryable: conditional
```

Secret-bearing payloads must be blocked rather than retried unchanged.

---

# Part XXIII — Security and Privacy Errors

## 145. PROVIDER_MANAGEMENT_REMOTE_EXECUTION_PROHIBITED

The selected provider path violates mandatory local-only policy.

```text
category: PRIVACY
scope: SELECTION
severity: ERROR
retryable: true with local provider
```

---

## 146. PROVIDER_MANAGEMENT_DATA_REGION_PROHIBITED

The selected provider region is not allowed.

```text
category: PRIVACY
scope: SELECTION
severity: ERROR
retryable: true with another region or provider
```

---

## 147. PROVIDER_MANAGEMENT_PROVIDER_POLICY_VIOLATION

Provider Management attempted to grant access contrary to mandatory policy.

```text
category: SECURITY
scope: LEASE
severity: CRITICAL
retryable: false until corrected
```

Examples:

- remote provider under `LOCAL_REQUIRED`;
- excluded provider selected;
- unapproved region selected;
- required capability absent;
- revoked credential used.

---

## 148. PROVIDER_MANAGEMENT_UNTRUSTED_METADATA_BLOCKED

Untrusted provider or model metadata attempted to alter routing, security, or registry state.

```text
category: SECURITY
scope: PROVIDER
severity: CRITICAL
retryable: false until corrected
```

Provider-native metadata is always treated as data.

---

## 149. PROVIDER_MANAGEMENT_SENSITIVE_LOGGING_BLOCKED

An operation attempted to write credentials, private paths, or provider content into restricted logs.

```text
category: SECURITY
scope: MODULE
severity: CRITICAL
retryable: false until corrected
```

---

## 150. PROVIDER_MANAGEMENT_ADAPTER_NOT_TRUSTED

An unapproved provider adapter attempted registration or execution.

```text
category: SECURITY
scope: PROVIDER
severity: CRITICAL
retryable: false
```

---

# Part XXIV — Warning Contract

## 151. ProviderManagementWarning

```text
ProviderManagementWarning {
    warningId
    code
    category
    severity

    providerId
    providerModelId
    providerLeaseId

    message
    userMessageKey

    affectedCapabilities[]
    recoveryActions[]

    occurredAt
    metadata
}
```

Warnings must remain provider-neutral and secret-free.

---

## 152. Warning Codes

Recommended warnings:

```text
PROVIDER_MANAGEMENT_WARNING_FALLBACK_SELECTED
PROVIDER_MANAGEMENT_WARNING_PREFERRED_PROVIDER_UNAVAILABLE
PROVIDER_MANAGEMENT_WARNING_MODEL_DEPRECATED
PROVIDER_MANAGEMENT_WARNING_PROVIDER_DEGRADED
PROVIDER_MANAGEMENT_WARNING_RATE_LIMIT_NEAR_THRESHOLD
PROVIDER_MANAGEMENT_WARNING_QUOTA_LOW
PROVIDER_MANAGEMENT_WARNING_LOCAL_MODEL_COLD_START
PROVIDER_MANAGEMENT_WARNING_LOCAL_MODEL_BUSY
PROVIDER_MANAGEMENT_WARNING_SELECTION_EXPLANATION_INCOMPLETE
PROVIDER_MANAGEMENT_WARNING_USAGE_ESTIMATED
PROVIDER_MANAGEMENT_WARNING_HEALTH_UNKNOWN
PROVIDER_MANAGEMENT_WARNING_CAPABILITY_LIMIT_UNKNOWN
PROVIDER_MANAGEMENT_WARNING_CLIENT_REPLACED
PROVIDER_MANAGEMENT_WARNING_LEASE_NEAR_EXPIRATION
```

---

## 153. PROVIDER_MANAGEMENT_WARNING_FALLBACK_SELECTED

A fallback provider or model was selected.

```text
category: SELECTION
severity: DEGRADED
```

The operation may continue.

---

## 154. PROVIDER_MANAGEMENT_WARNING_MODEL_DEPRECATED

A deprecated model was selected under explicit policy.

```text
category: PROVIDER_MODEL
severity: DEGRADED
```

---

## 155. PROVIDER_MANAGEMENT_WARNING_PROVIDER_DEGRADED

The provider is usable but operationally degraded.

```text
category: HEALTH
severity: DEGRADED
```

---

## 156. PROVIDER_MANAGEMENT_WARNING_LOCAL_MODEL_COLD_START

Local model startup may increase latency.

```text
category: LOCAL_MODEL
severity: NOTICE
```

---

## 157. PROVIDER_MANAGEMENT_WARNING_USAGE_ESTIMATED

Usage or cost is estimated rather than provider-reported.

```text
category: USAGE
severity: NOTICE
```

---

# Part XXV — Error-to-State Mapping

## 158. Provider Definition Mapping

| Error | Typical state consequence |
|---|---|
| Provider configuration invalid | remain `REGISTERED` or enter `DISABLED` |
| Provider disable conflict | no transition |
| Provider archived | operation rejected |
| Provider persistence failed | transition not committed |

---

## 159. Model Mapping

| Error | Typical state consequence |
|---|---|
| Model validation failed | remain `REGISTERED` or enter `DISABLED` |
| Model removed | no new lease |
| Model revision mismatch | selection invalidated |
| Deprecated model warning | may remain `DEPRECATED` |

---

## 160. Lease Mapping

| Error | Typical state consequence |
|---|---|
| Lease rejected | `REQUESTED → REJECTED` |
| Lease creation failed | `REQUESTED → FAILED` |
| Lease expired | active state → `EXPIRED` |
| Lease revoked | active state → `REVOKED` |
| Release failed | remain `RELEASE_REQUESTED` or enter `FAILED` |
| Handle creation failed | `REQUESTED/GRANTED → FAILED` |

---

## 161. Health and Circuit Mapping

| Error | Typical consequence |
|---|---|
| Provider timeout | health evidence; possible circuit increment |
| False health attribution blocked | no health change |
| Circuit open | availability may become `CIRCUIT_OPEN` |
| Probe rejected | remain `OPEN` or `HALF_OPEN` |
| Health evaluation failed | health may become `UNKNOWN` |

---

## 162. Local Model Mapping

| Error | Typical state consequence |
|---|---|
| Install failed | `INSTALLING → FAILED` |
| Validation failed | `VALIDATING → FAILED` |
| Load failed | `LOADING → FAILED` |
| Unload failed | `UNLOADING → FAILED` |
| Resource admission denied | no health failure; availability `RESOURCE_CONSTRAINED` |
| Model removed | → `REMOVED` |

---

# Part XXVI — Public Versus Internal Errors

## 163. Public Errors

Public errors should expose:

- stable code;
- category;
- scope;
- severity;
- normalized message;
- user message key;
- retryability;
- recovery actions;
- stable identifiers;
- safe metadata.

---

## 164. Internal Errors

Internal diagnostics may additionally contain:

- stack traces;
- adapter call sites;
- provider SDK exception types;
- internal client state;
- secret-manager error references;
- local model loader details;
- private file paths;
- raw health evidence.

Internal diagnostics must still follow secure logging policy.

---

## 165. Public Redaction Rule

Before an error leaves Provider Management, it must remove:

- credentials;
- provider-native authorization data;
- private model paths;
- source and result content;
- raw provider bodies;
- unrestricted stack traces;
- secret-manager internals.

---

# Part XXVII — Logging and Observability

## 166. Logging Fields

Recommended safe fields:

```text
errorId
code
category
scope
severity

providerId
providerModelId
providerLeaseId
providerCircuitId
localModelInstanceId

consumerModule
capability

state
stateRevision

retryable
recoveryActionCodes

occurredAt
traceId
correlationId
```

---

## 167. Prohibited Logging

Logs must not include:

- API keys;
- access tokens;
- refresh tokens;
- client secrets;
- authorization headers;
- raw provider prompts;
- source reading content;
- translated or recognized text;
- full provider responses;
- private local model paths;
- unrelated user context.

---

## 168. Metrics

Recommended error metrics:

```text
provider_management_error_count
provider_selection_rejection_count
provider_selection_failure_count
provider_lease_rejection_count
provider_lease_failure_count
provider_lease_revocation_count
provider_credential_failure_count
provider_client_failure_count
provider_health_evaluation_failure_count
provider_rate_limit_count
provider_quota_exhaustion_count
provider_circuit_open_count
local_model_install_failure_count
local_model_load_failure_count
local_model_unload_failure_count
provider_event_publication_failure_count
provider_state_conflict_count
provider_security_block_count
```

Metrics should be labeled with normalized identities and categories.

---

# Part XXVIII — Core Error Invariants

## 169. Invariant 1 — Provider neutrality

Public errors never depend on provider-native exception classes.

---

## 170. Invariant 2 — Credential secrecy

No public error, warning, event, or log contains raw credentials.

---

## 171. Invariant 3 — Rejection is not failure

A valid selection or lease rejection is not automatically an internal system failure.

---

## 172. Invariant 4 — Runtime ownership

Runtime scheduling and worker failures remain Runtime-owned, even when Provider Management records lease consequences.

---

## 173. Invariant 5 — Consumer semantic ownership

Translation and Recognition semantic validation errors remain owned by those modules.

---

## 174. Invariant 6 — Health attribution safety

Only provider-relevant evidence affects Provider Health or Circuit state.

---

## 175. Invariant 7 — Lease terminality

A terminal lease error never reactivates the same lease.

---

## 176. Invariant 8 — Durable state before events

No Provider Management lifecycle event is published before state is durable.

---

## 177. Invariant 9 — Stale revision protection

Older configuration, health, capability, rate-limit, circuit, or state revisions cannot overwrite newer state.

---

## 178. Invariant 10 — Policy enforcement

Mandatory privacy, locality, region, capability, and credential rules are never silently downgraded.

---

## 179. Invariant 11 — Local resource attribution

Runtime resource denial does not automatically mark a local provider unhealthy.

---

## 180. Invariant 12 — Historical identity

Disablement, archival, deprecation, removal, expiration, and revocation preserve historical identity.

---

## 181. Invariant 13 — Handle isolation

A handle capability or consumer mismatch is treated as a security error.

---

## 182. Invariant 14 — Unknown states are conservative

Unknown health, capability, credential, or availability state must not be treated optimistically without explicit policy.

---

# Part XXIX — MVP Error Scope

## 183. Required MVP Error Groups

The MVP requires:

```text
Command validation errors
Provider definition errors
Provider configuration errors
Provider model errors
Capability errors
Eligibility errors
Selection errors
Lease errors
Execution handle errors
Credential errors
Availability errors
Health errors
Rate-limit errors
Circuit errors
Local model errors
Concurrency errors
Persistence errors
Event publication errors
Security and privacy errors
Warnings
```

---

## 184. Required MVP Codes

At minimum:

```text
PROVIDER_MANAGEMENT_COMMAND_INVALID
PROVIDER_MANAGEMENT_PROVIDER_NOT_FOUND
PROVIDER_MANAGEMENT_PROVIDER_DISABLED
PROVIDER_MANAGEMENT_CONFIGURATION_INVALID
PROVIDER_MANAGEMENT_MODEL_NOT_FOUND
PROVIDER_MANAGEMENT_MODEL_NOT_ACTIVE
PROVIDER_MANAGEMENT_CAPABILITY_UNSUPPORTED
PROVIDER_MANAGEMENT_CAPABILITY_LIMIT_EXCEEDED
PROVIDER_MANAGEMENT_SELECTION_NO_ELIGIBLE_PROVIDER
PROVIDER_MANAGEMENT_SELECTION_RESULT_STALE
PROVIDER_MANAGEMENT_LEASE_REJECTED
PROVIDER_MANAGEMENT_LEASE_CREATION_FAILED
PROVIDER_MANAGEMENT_LEASE_EXPIRED
PROVIDER_MANAGEMENT_LEASE_REVOKED
PROVIDER_MANAGEMENT_EXECUTION_HANDLE_CREATION_FAILED
PROVIDER_MANAGEMENT_CREDENTIAL_UNAVAILABLE
PROVIDER_MANAGEMENT_PROVIDER_UNAVAILABLE
PROVIDER_MANAGEMENT_RATE_LIMITED
PROVIDER_MANAGEMENT_CIRCUIT_OPEN
PROVIDER_MANAGEMENT_LOCAL_MODEL_LOAD_FAILED
PROVIDER_MANAGEMENT_STATE_REVISION_CONFLICT
PROVIDER_MANAGEMENT_LEASE_PERSISTENCE_FAILED
PROVIDER_MANAGEMENT_EVENT_PUBLICATION_FAILED
PROVIDER_MANAGEMENT_PROVIDER_POLICY_VIOLATION
PROVIDER_MANAGEMENT_CREDENTIAL_EXPOSURE_BLOCKED
PROVIDER_MANAGEMENT_UNKNOWN_ERROR
```

---

# Part XXX — Open Decisions

## 185. Error Prefix

This document uses:

```text
PROVIDER_MANAGEMENT_*
```

The project may later choose a shorter stable prefix such as:

```text
PROVIDER_*
```

The decision must be made before implementation and public release.

---

## 186. Selection Rejection Visibility

Decide whether expected selection rejection is returned as:

- normal result with rejection reason;
- normalized `NOTICE` error;
- both, depending on API surface.

Recommended approach:

```text
command response
    → structured rejection result

events and diagnostics
    → normalized rejection code
```

---

## 187. Lease Expiration Severity

Recommended rule:

```text
expected unused lease expiration
    → NOTICE

expiration during required active use
    → ERROR
```

---

## 188. Circuit Open Severity

Recommended rule:

```text
fallback available
    → NOTICE or DEGRADED

no eligible alternative
    → ERROR
```

---

## 189. Health Unknown Behavior

Decide whether unknown health:

- excludes the provider;
- lowers ranking;
- permits explicit probes;
- permits explicit required-provider use.

Recommended default:

```text
exclude from automatic selection
allow controlled probe
```

---

## 190. Local Model Busy Semantics

Error severity depends on whether `BUSY` means:

- at least one active execution;
- capacity materially constrained;
- concurrency limit reached.

Recommended interpretation:

```text
material capacity constraint
```

---

# Part XXXI — Related Documents

```text
02-modules/provider-management/MODULE.md
02-modules/provider-management/CONTRACT.md
02-modules/provider-management/STATES.md
02-modules/provider-management/EVENTS.md
02-modules/provider-management/README.md
```

Architecture references:

```text
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
```

Runtime references:

```text
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/RESOURCE_LIFECYCLE.md
docs/architecture/runtime/WORK_QUEUE.md
docs/architecture/runtime/SCHEDULER.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md
```

Related module references:

```text
02-modules/translation/ERRORS.md
02-modules/translation/CONTRACT.md
02-modules/recognition/ERRORS.md
02-modules/reading-session/ERRORS.md
02-modules/presentation/ERRORS.md
```

---

## 191. Summary

Provider Management errors cover:

```text
Provider definitions
Provider configuration
Provider models
Capabilities
Eligibility
Selection
Leases
Execution handles
Credentials
Provider clients
Availability
Health
Rate limits and quotas
Circuit breakers
Local models
Usage
Outcome feedback
Concurrency
Persistence
Event publication
Security and privacy
```

The central error boundary is:

```text
Consumer requirement
    ↓
Provider eligibility
    ↓
Provider selection
    ↓
Provider lease
    ↓
Execution handle
```

Failures are normalized at the point where they affect Provider Management contracts or state.

The most important distinctions are:

```text
Selection rejected
    ≠ internal failure

Lease revoked
    ≠ provider failure

Runtime admission denied
    ≠ provider unhealthy

Consumer semantic validation failed
    ≠ provider unhealthy

Credential reference
    ≠ raw credential

Provider health
    ≠ provider availability
```

Every public error must remain:

- provider-neutral;
- capability-neutral;
- credential-safe;
- revision-aware;
- machine-readable;
- recovery-oriented;
- free from user reading content;
- compatible with Runtime and consumer-module ownership boundaries.

This document is the error-model source of truth for Provider Management.
