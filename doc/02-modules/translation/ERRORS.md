# Translation Errors

> **Project:** CRAI
> **Module:** Translation
> **Document:** Errors and Warnings
> **Path:** `modules/translation/ERRORS.md`
> **Version:** 0.2
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-03
> **Source of Truth:**
>
> * `modules/translation/MODULE.md`
> * `modules/translation/CONTRACT.md`
> * `modules/translation/EVENTS.md`
> * `modules/translation/STATES.md`

---

## 1. Purpose

This document defines the normalized errors and warnings owned by the Translation module.

It specifies:

* error structure;
* error identifiers;
* error categories;
* error scopes;
* error severity;
* retryability;
* provider error normalization;
* command validation errors;
* translation execution errors;
* result validation errors;
* alignment errors;
* cancellation and supersession handling;
* warnings;
* partial-result behavior;
* state transition implications;
* public and internal error boundaries;
* privacy and logging rules.

This document does not define:

* provider-native error payloads;
* UI design;
* localized user-interface text;
* HTTP status mappings for a specific API;
* database schemas;
* monitoring alert thresholds;
* implementation exception classes.

---

## 2. Error Design Goals

Translation error handling must support the following goals:

* keep public contracts provider-neutral;
* distinguish retryable and non-retryable failures;
* preserve successfully translated segments;
* avoid retrying invalid requests;
* avoid exposing credentials or private content;
* prevent stale output from becoming authoritative;
* support provider fallback;
* support local and remote providers;
* provide useful user-facing recovery actions;
* preserve machine-readable diagnostics;
* remain compatible with future providers.

---

## 3. Error Versus Warning

An error prevents the affected operation from being accepted or completed as intended.

A warning describes an imperfect but usable outcome.

```text
Error
    → work cannot continue or cannot be accepted

Warning
    → work remains usable with limitations
```

Examples:

```text
Provider returned no parseable segments
    → Error

Translation completed using a fallback provider
    → Warning

One selected segment is permanently missing
    → Error or partial failure

Output is longer than the source bubble
    → Warning
```

---

## 4. Error Versus Lifecycle Control

The following lifecycle outcomes are not necessarily errors:

```text
CANCELLED
SUPERSEDED
INVALIDATED
```

### Cancellation

The user or system intentionally stopped the work.

### Supersession

Newer work replaced the old work.

### Invalidation

Existing output was marked unusable after an administrative or consistency decision.

These outcomes may carry a reason code but should not automatically be reported as system failures.

---

## 5. Error Ownership

Translation owns normalized errors concerning:

* translation command validation;
* translation source resolution;
* configuration validation;
* context resolution;
* terminology resolution;
* translation-specific provider eligibility and fallback eligibility;
* provider response normalization at the Translation boundary;
* output validation;
* segment alignment;
* translation-specific retry eligibility;
* result assembly;
* publication validation;
* variant compatibility and activation intent;
* cache compatibility.

Runtime owns original execution-control failures concerning:

* queue admission and scheduling;
* retry timing and backoff;
* execution-attempt budgets;
* worker execution and worker crashes;
* generic timeout enforcement;
* physical cancellation propagation;
* resource admission and concurrency control;
* Event Bus delivery retry.

Provider Management owns original failures concerning:

* provider registration and enablement;
* provider instance lifecycle;
* credential resolution and refresh;
* provider health and capability discovery;
* local-model residency.

Translation does not own original errors concerning:

* OCR execution;
* DOM extraction;
* image acquisition;
* browser permissions;
* Reading Session persistence;
* Knowledge database internals;
* Presentation rendering.

Errors received from Runtime, Provider Management or other modules may be normalized at the Translation boundary when exposed as Translation failures. Normalization does not transfer ownership of the original failure.

### 5.1 Error Ownership Matrix

| Concern or original failure | Original owner | Translation responsibility |
|---|---|---|
| Prepared source cannot be resolved | Text Processing or source storage boundary | Normalize as a Translation source error |
| Queue admission expires | Runtime | Normalize the execution outcome when it terminates Translation work |
| Worker crashes | Runtime | Associate the failure with the affected batch attempt |
| Provider disabled or unavailable | Provider Management | Normalize eligibility or execution impact |
| Provider response is malformed | Provider adapter / Translation boundary | Normalize and validate provider-neutral output |
| Retry eligibility | Translation | Decide whether the same semantic work may be attempted again |
| Retry timing, backoff and budget enforcement | Runtime | Consume the resulting runtime outcome |
| Result alignment or assembly fails | Translation | Own and publish the normalized Translation failure |
| Reading authority rejects a result | Reading Session | Record a non-authoritative or stale Translation outcome |
| Presentation rendering fails | Presentation | No Translation error ownership |

---

# Part I — Normalized Error Contract

## 6. TranslationError

The public normalized error model is:

```text
TranslationError {
    errorId

    code
    category
    scope
    severity

    message
    userMessageKey

    retryability
    recoveryActions[]

    translationJobId
    translationAttemptId
    translationBatchId
    translationResultId
    translationVariantId
    translationIntentId
    translationRevision

    preparedDocumentId
    contentRevision
    affectedPreparedSegmentIds[]

    provider
    cause

    occurredAt

    metadata
}
```

Not every field is present for every error.

---

## 7. errorId

Uniquely identifies one normalized error occurrence.

```text
errorId
```

It supports:

* event correlation;
* logs;
* diagnostics;
* support requests;
* duplicate error handling.

The same failure published through multiple channels should retain the same `errorId` where practical.

---

## 8. code

A stable machine-readable error code.

Examples:

```text
TRANSLATION_SOURCE_NOT_FOUND
TRANSLATION_PROVIDER_TIMEOUT
TRANSLATION_OUTPUT_ALIGNMENT_FAILED
```

Rules:

* uppercase snake case;
* stable after publication;
* provider-neutral;
* sufficiently specific;
* not based on human-readable messages.

---

## 9. category

Groups related errors.

Canonical categories:

```text
COMMAND_VALIDATION
SOURCE
CONFIGURATION
CONTEXT
KNOWLEDGE
PROVIDER_SELECTION
PROVIDER_AUTHENTICATION
PROVIDER_AVAILABILITY
PROVIDER_RATE_LIMIT
PROVIDER_EXECUTION
PROVIDER_RESPONSE
OUTPUT_VALIDATION
ALIGNMENT
TIMEOUT
CANCELLATION
SUPERSESSION
CACHE
RESULT_ASSEMBLY
PUBLICATION
VARIANT
CONCURRENCY
SECURITY
PRIVACY
INTERNAL
```

---

## 10. scope

Identifies the entity or operation primarily affected.

Canonical scopes:

```text
COMMAND
JOB
ATTEMPT
BATCH
RESULT
VARIANT
SEGMENT
PROVIDER
CACHE
MODULE
```

An error may affect several entities, but it must have one primary scope.

---

## 11. severity

Canonical error severity values:

```text
NOTICE
DEGRADED
ERROR
CRITICAL
```

### NOTICE

The operation was rejected safely with no corrupted state.

Example:

```text
Unsupported target language requested
```

### DEGRADED

Part of the operation failed, but partial usable output exists.

Example:

```text
One batch failed while other batches succeeded
```

### ERROR

The intended operation could not complete.

Example:

```text
All translation attempts failed
```

### CRITICAL

A severe module integrity, security or persistent consistency problem occurred.

Example:

```text
Published result references missing persistent segments
```

`CRITICAL` should be rare.

---

## 12. message

A developer-facing normalized explanation.

The message must:

* avoid provider credentials;
* avoid full source text;
* avoid full translated text;
* avoid raw provider response bodies;
* avoid unstable provider-native wording.

Example:

```text
Provider execution exceeded the configured batch timeout.
```

---

## 13. userMessageKey

An optional localization key for user-facing presentation.

Example:

```text
translation.error.provider_timeout
```

Translation should not hardcode final localized UI wording into the domain error contract.

---

## 14. retryability

```text
Retryability {
    retryable
    recommendedRetryScope
    requiresConfigurationChange

    advisoryRetryAfter
    advisoryMaximumAdditionalAttempts
}
```

Possible retry scopes:

```text
NONE
SAME_BATCH
FAILED_SEGMENTS
NEW_ATTEMPT
NEW_PROVIDER
NEW_JOB
MANUAL_ONLY
```

`advisoryRetryAfter` and `advisoryMaximumAdditionalAttempts` are Translation recommendations only. Runtime owns actual retry timing, backoff, admission and execution-budget enforcement.

---

## 15. recoveryActions

Machine-readable recovery suggestions.

Canonical actions:

```text
RETRY
RETRY_FAILED_BATCHES
USE_FALLBACK_PROVIDER
SELECT_DIFFERENT_PROVIDER
REDUCE_BATCH_SIZE
REDUCE_CONTEXT_SIZE
CHANGE_TRANSLATION_PROFILE
CHANGE_TARGET_LANGUAGE
REFRESH_SOURCE
REBUILD_PREPARED_DOCUMENT
UPDATE_CREDENTIALS
WAIT_AND_RETRY
CHECK_NETWORK
USE_LOCAL_PROVIDER
REQUEST_RETRANSLATION
REVIEW_TERMINOLOGY
REMOVE_CONFLICTING_TERM
SELECT_ANOTHER_VARIANT
CONTACT_SUPPORT
NONE
```

These actions are recommendations, not automatic commands.

---

## 16. ProviderErrorReference

Normalized provider-related details:

```text
ProviderErrorReference {
    providerId
    modelIdentifier

    providerRequestId

    normalizedProviderCode
    providerHttpStatus

    providerRetryAfter
}
```

It must not include:

* API keys;
* access tokens;
* raw headers;
* full raw body;
* provider credentials;
* private internal prompt.

---

## 17. ErrorCause

Optional normalized cause chain:

```text
ErrorCause {
    code
    category
    message
}
```

The public cause chain should be shallow.

Full exception stacks remain internal.

---

## 18. metadata

Metadata contains bounded non-sensitive diagnostic values.

Allowed examples:

```text
segmentCount
batchSize
configuredTimeout
elapsedTime
attemptNumber
providerLimit
estimatedTokenCount
```

Prohibited examples:

```text
sourceText
translatedText
rawPrompt
apiKey
authorizationHeader
fullProviderResponse
```

---

# Part II — Error Code Naming

## 19. Naming Convention

Translation error codes use:

```text
TRANSLATION_<CONCERN>_<CONDITION>
```

Examples:

```text
TRANSLATION_SOURCE_NOT_FOUND
TRANSLATION_PROVIDER_UNAVAILABLE
TRANSLATION_OUTPUT_SEGMENT_MISSING
```

Warnings use:

```text
TRANSLATION_WARNING_<CONDITION>
```

Examples:

```text
TRANSLATION_WARNING_PROVIDER_FALLBACK_USED
TRANSLATION_WARNING_PRONOUN_AMBIGUITY
```

---

## 20. Stability Rule

Once an error code is used in:

* public API responses;
* integration events;
* telemetry;
* persisted job failures;

its semantic meaning must not change.

A new meaning requires a new code.

---

## 21. Unknown Error Code

Consumers must support:

```text
TRANSLATION_UNKNOWN_ERROR
```

Unknown provider or internal failures may temporarily normalize to this code.

The implementation should still preserve internal diagnostic references.

---

# Part III — Command Validation Errors

## 22. TRANSLATION_COMMAND_INVALID

The command contract is malformed or internally inconsistent.

```text
category: COMMAND_VALIDATION
scope: COMMAND
severity: NOTICE
retryable: false
```

Examples:

* required fields missing;
* mutually exclusive source modes supplied;
* invalid publication policy combination;
* invalid retry scope.

Recovery:

```text
Correct the command and submit again.
```

---

## 23. TRANSLATION_IDEMPOTENCY_CONFLICT

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

## 24. TRANSLATION_SEGMENT_SELECTION_EMPTY

No translatable segment was selected.

```text
category: COMMAND_VALIDATION
scope: COMMAND
severity: NOTICE
retryable: false
```

This may occur when:

* explicit selection is empty;
* every selected segment is marked `DO_NOT_TRANSLATE`;
* selected range resolves to no prepared segments.

---

## 25. TRANSLATION_SEGMENT_SELECTION_INVALID

One or more selected prepared segment identifiers are invalid.

```text
category: COMMAND_VALIDATION
scope: COMMAND
severity: NOTICE
retryable: false
```

Metadata may contain:

```text
invalidSegmentCount
```

It should not include unrelated source text.

---

## 26. TRANSLATION_DUPLICATE_SEGMENT_SELECTION

The command selected the same prepared segment more than once.

```text
category: COMMAND_VALIDATION
scope: COMMAND
severity: NOTICE
retryable: false
```

Translation must not silently duplicate output for the segment.

---

## 27. TRANSLATION_TARGET_LANGUAGE_REQUIRED

No explicit target language was supplied.

```text
category: CONFIGURATION
scope: COMMAND
severity: NOTICE
retryable: false
```

Recovery:

```text
Choose a target language and create a new command.
```

---

## 28. TRANSLATION_TARGET_LANGUAGE_UNSUPPORTED

No eligible configured provider supports the target language.

```text
category: CONFIGURATION
scope: COMMAND
severity: NOTICE
retryable: conditional
```

Retry becomes possible after:

* selecting another provider;
* installing a local model;
* changing target language;
* updating provider capabilities.

---

## 29. TRANSLATION_LANGUAGE_PAIR_UNSUPPORTED

The requested source and target language pair is unsupported.

```text
category: CONFIGURATION
scope: COMMAND
severity: NOTICE
retryable: false without configuration change
```

---

## 30. TRANSLATION_PROFILE_NOT_FOUND

The referenced translation profile does not exist.

```text
category: CONFIGURATION
scope: COMMAND
severity: NOTICE
retryable: false
```

---

## 31. TRANSLATION_PROFILE_REVISION_NOT_FOUND

The requested immutable profile revision is unavailable.

```text
category: CONFIGURATION
scope: COMMAND
severity: NOTICE
retryable: false
```

Recovery may require using the current profile revision and starting a new job.

---

## 32. TRANSLATION_PUBLICATION_POLICY_INVALID

The publication configuration is inconsistent.

Examples:

* progressive output disabled but minimum progressive segments supplied;
* atomic mode combined with segment-authority requirements;
* partial output required while partial execution is prohibited.

```text
category: CONFIGURATION
scope: COMMAND
severity: NOTICE
retryable: false
```

---

## 33. TRANSLATION_PROVIDER_POLICY_UNSATISFIABLE

No provider can satisfy the requested policy.

Examples:

* `LOCAL_REQUIRED` with no local provider;
* required provider excluded;
* required capability unavailable;
* remote provider required while remote use is disabled.

```text
category: PROVIDER_SELECTION
scope: COMMAND
severity: NOTICE
retryable: false without policy change
```

---

# Part IV — Source Errors

## 34. TRANSLATION_SOURCE_NOT_FOUND

The referenced prepared document cannot be resolved.

```text
category: SOURCE
scope: JOB
severity: ERROR
retryable: conditional
```

Possible causes:

* document deleted;
* incorrect identifier;
* upstream storage unavailable;
* source expired.

Recovery actions:

```text
REFRESH_SOURCE
REBUILD_PREPARED_DOCUMENT
RETRY
```

---

## 35. TRANSLATION_SOURCE_REVISION_NOT_FOUND

The document exists but the requested immutable revision is unavailable.

```text
category: SOURCE
scope: JOB
severity: ERROR
retryable: false for the same revision
```

A new job may target the current revision.

---

## 36. TRANSLATION_SOURCE_REVISION_MISMATCH

The resolved document revision differs from the job source identity.

```text
category: SOURCE
scope: JOB
severity: ERROR
retryable: false within the current job
```

State consequence:

```text
Job → SUPERSEDED
```

or:

```text
Job → FAILED
```

depending on whether a newer revision exists.

---

## 37. TRANSLATION_SOURCE_NOT_PREPARED

The supplied source is raw or has not passed through Text Processing.

```text
category: SOURCE
scope: COMMAND
severity: NOTICE
retryable: false
```

Recovery:

```text
Process the source through Text Processing first.
```

---

## 38. TRANSLATION_SOURCE_EMPTY

The prepared source contains no usable translatable text.

```text
category: SOURCE
scope: JOB
severity: NOTICE
retryable: false
```

This may be a valid no-op in some workflows.

The caller should not treat it as an infrastructure failure.

---

## 39. TRANSLATION_SOURCE_SEGMENT_NOT_FOUND

A selected segment no longer exists in the resolved prepared document revision.

```text
category: SOURCE
scope: SEGMENT
severity: ERROR
retryable: false within the current job
```

---

## 40. TRANSLATION_SOURCE_SEGMENT_EMPTY

A selected segment has no source text and is not intentionally non-translatable.

```text
category: SOURCE
scope: SEGMENT
severity: DEGRADED
retryable: false
```

Possible job outcome:

```text
PARTIALLY_COMPLETED
COMPLETED_WITH_WARNINGS
FAILED
```

depending on publication policy.

---

## 41. TRANSLATION_SOURCE_LANGUAGE_UNKNOWN

The source language could not be determined and no explicit language was supplied.

```text
category: SOURCE
scope: JOB
severity: ERROR
retryable: conditional
```

Recovery:

```text
Provide an explicit source language or select a provider with detection support.
```

---

## 42. TRANSLATION_SOURCE_LANGUAGE_CONFLICT

Document-level and segment-level language information conflict materially.

```text
category: SOURCE
scope: SEGMENT or JOB
severity: DEGRADED
retryable: conditional
```

Possible handling:

* use explicit segment hint;
* use document language;
* perform provider detection;
* fail strict mode;
* continue with warning.

---

## 43. TRANSLATION_SOURCE_CONTENT_TOO_LARGE

The selected source exceeds job-level limits.

```text
category: SOURCE
scope: JOB
severity: ERROR
retryable: false without input change
```

Recovery:

```text
REDUCE_BATCH_SIZE
Reduce selected segment range
Split the prepared document
```

---

# Part V — Context Errors

## 44. TRANSLATION_CONTEXT_NOT_FOUND

A required context snapshot cannot be resolved.

```text
category: CONTEXT
scope: JOB
severity: ERROR
retryable: conditional
```

Behavior depends on `missingContextBehavior`.

### Strict context

```text
Job may fail.
```

### Best-effort context

```text
Continue with warning.
```

---

## 45. TRANSLATION_CONTEXT_REVISION_MISMATCH

Resolved context does not match the job’s immutable context revision.

```text
category: CONTEXT
scope: JOB
severity: ERROR
retryable: false within current job
```

A new job should use the new context revision.

---

## 46. TRANSLATION_CONTEXT_TOO_LARGE

Context exceeds configured or provider limits.

```text
category: CONTEXT
scope: ATTEMPT or BATCH
severity: ERROR
retryable: true with reduced context
```

Recovery actions:

```text
REDUCE_CONTEXT_SIZE
RETRY
```

---

## 47. TRANSLATION_CONTEXT_CONSTRUCTION_FAILED

The module could not assemble valid translation context.

```text
category: CONTEXT
scope: ATTEMPT
severity: ERROR
retryable: conditional
```

Examples:

* unresolved context references;
* invalid context ordering;
* context serialization failure;
* incompatible context type.

---

## 48. TRANSLATION_CONTEXT_PRIVACY_RESTRICTED

Requested context cannot be sent to the selected provider due to privacy policy.

```text
category: PRIVACY
scope: ATTEMPT
severity: ERROR
retryable: true with another provider
```

Recovery:

```text
USE_LOCAL_PROVIDER
SELECT_DIFFERENT_PROVIDER
Reduce context
```

---

# Part VI — Knowledge and Terminology Errors

## 49. TRANSLATION_KNOWLEDGE_SNAPSHOT_NOT_FOUND

The referenced Knowledge snapshot is unavailable.

```text
category: KNOWLEDGE
scope: JOB
severity: ERROR
retryable: conditional
```

Strict terminology policy may fail the job.

Best-effort policy may continue with warning.

---

## 50. TRANSLATION_GLOSSARY_REVISION_NOT_FOUND

The requested glossary revision cannot be resolved.

```text
category: KNOWLEDGE
scope: JOB
severity: ERROR
retryable: false within current job
```

---

## 51. TRANSLATION_TERMINOLOGY_CONFLICT

Two or more terminology constraints conflict.

Example:

```text
Same source term
    → two different LOCKED target terms
```

```text
category: KNOWLEDGE
scope: JOB or SEGMENT
severity: ERROR or DEGRADED
retryable: false without terminology change
```

Behavior depends on conflict policy:

```text
FAIL
WARN_AND_CONTINUE
PREFER_LOCKED
PREFER_MOST_SPECIFIC_SCOPE
```

---

## 52. TRANSLATION_LOCKED_TERM_VIOLATED

Provider output violates a locked terminology constraint.

```text
category: OUTPUT_VALIDATION
scope: BATCH or SEGMENT
severity: ERROR
retryable: true
```

Possible recovery:

* retry with stronger provider instructions;
* retry with another provider;
* split affected segments;
* fail after retry exhaustion.

---

## 53. TRANSLATION_TERMINOLOGY_LIMIT_EXCEEDED

Terminology constraints exceed provider or request limits.

```text
category: KNOWLEDGE
scope: ATTEMPT
severity: ERROR
retryable: true with reduced terminology context
```

Translation must not silently discard locked terms.

---

## 54. TRANSLATION_CHARACTER_CONTEXT_INVALID

Character relationship or name context is malformed or contradictory.

```text
category: KNOWLEDGE
scope: JOB
severity: DEGRADED
retryable: conditional
```

Best-effort translation may continue with warnings.

---

# Part VII — Provider Selection Errors

## 55. TRANSLATION_PROVIDER_NOT_FOUND

The required or preferred provider does not exist in Provider Management.

```text
category: PROVIDER_SELECTION
scope: JOB
severity: ERROR
retryable: false without configuration change
```

---

## 56. TRANSLATION_PROVIDER_DISABLED

The selected provider is disabled.

```text
category: PROVIDER_SELECTION
scope: ATTEMPT
severity: ERROR
retryable: true with fallback
```

---

## 57. TRANSLATION_PROVIDER_UNAVAILABLE

The provider is temporarily unavailable.

```text
category: PROVIDER_AVAILABILITY
scope: ATTEMPT
severity: ERROR
retryable: true
```

Recovery:

```text
RETRY
USE_FALLBACK_PROVIDER
WAIT_AND_RETRY
```

---

## 58. TRANSLATION_PROVIDER_CAPABILITY_MISSING

The provider lacks a required capability.

Examples:

* unsupported language pair;
* no local execution;
* no structured output;
* insufficient context size;
* glossary support required but unavailable.

```text
category: PROVIDER_SELECTION
scope: ATTEMPT
severity: ERROR
retryable: true with another provider
```

---

## 59. TRANSLATION_NO_ELIGIBLE_PROVIDER

Provider selection found no eligible provider.

```text
category: PROVIDER_SELECTION
scope: JOB
severity: ERROR
retryable: conditional
```

Possible state consequence:

```text
Job → FAILED
```

unless provider availability is expected to change and queue waiting is permitted.

---

## 60. TRANSLATION_PROVIDER_POLICY_VIOLATION

Execution attempted to use a provider prohibited by the job policy.

```text
category: SECURITY
scope: ATTEMPT
severity: CRITICAL
retryable: false until corrected
```

Examples:

* remote provider used under `LOCAL_REQUIRED`;
* excluded provider selected;
* unapproved data region selected.

This indicates an orchestration defect rather than a normal provider failure.

---

# Part VIII — Provider Authentication Errors

## 61. TRANSLATION_PROVIDER_CREDENTIALS_MISSING

Required credentials are unavailable.

```text
category: PROVIDER_AUTHENTICATION
scope: PROVIDER
severity: ERROR
retryable: false until credentials are configured
```

Recovery:

```text
UPDATE_CREDENTIALS
SELECT_DIFFERENT_PROVIDER
USE_LOCAL_PROVIDER
```

---

## 62. TRANSLATION_PROVIDER_AUTHENTICATION_FAILED

The provider rejected configured credentials.

```text
category: PROVIDER_AUTHENTICATION
scope: PROVIDER
severity: ERROR
retryable: false without credential change
```

The public error must not include the credential value or authorization header.

---

## 63. TRANSLATION_PROVIDER_AUTHORIZATION_FAILED

Credentials are valid but lack permission for the requested resource or model.

```text
category: PROVIDER_AUTHENTICATION
scope: PROVIDER
severity: ERROR
retryable: false without permission or provider change
```

---

## 64. TRANSLATION_PROVIDER_CREDENTIALS_EXPIRED

Configured credentials have expired.

```text
category: PROVIDER_AUTHENTICATION
scope: PROVIDER
severity: ERROR
retryable: conditional
```

Automated credential refresh may occur within Provider Management.

Translation should not own long-term credential refresh logic.

---

# Part IX — Provider Rate and Quota Errors

## 65. TRANSLATION_PROVIDER_RATE_LIMITED

The provider temporarily rejected execution due to request-rate limits.

```text
category: PROVIDER_RATE_LIMIT
scope: ATTEMPT or BATCH
severity: ERROR
retryable: true
```

Required normalized information where available:

```text
retryAfter
providerId
providerRequestId
```

Recovery:

```text
WAIT_AND_RETRY
USE_FALLBACK_PROVIDER
```

---

## 66. TRANSLATION_PROVIDER_QUOTA_EXCEEDED

Provider usage quota or account balance is exhausted.

```text
category: PROVIDER_RATE_LIMIT
scope: PROVIDER
severity: ERROR
retryable: false without quota change
```

Fallback may still be possible.

---

## 67. TRANSLATION_PROVIDER_REQUEST_TOO_LARGE

The provider rejected the request because it exceeded size or context limits.

```text
category: PROVIDER_EXECUTION
scope: BATCH
severity: ERROR
retryable: true
```

Recovery:

```text
REDUCE_BATCH_SIZE
REDUCE_CONTEXT_SIZE
RETRY_FAILED_BATCHES
```

---

## 68. TRANSLATION_PROVIDER_OUTPUT_LIMIT_EXCEEDED

Provider generation stopped because output limits were reached.

```text
category: PROVIDER_RESPONSE
scope: BATCH
severity: DEGRADED or ERROR
retryable: true
```

If complete aligned segments exist, they may be retained.

Missing segments must be explicit.

---

# Part X — Network and Timeout Errors

## 69. TRANSLATION_NETWORK_UNAVAILABLE

No network path is available for a remote provider.

```text
category: PROVIDER_AVAILABILITY
scope: ATTEMPT
severity: ERROR
retryable: true
```

Recovery:

```text
CHECK_NETWORK
USE_LOCAL_PROVIDER
WAIT_AND_RETRY
```

---

## 70. TRANSLATION_PROVIDER_CONNECTION_FAILED

A connection to the provider could not be established.

```text
category: PROVIDER_EXECUTION
scope: BATCH or ATTEMPT
severity: ERROR
retryable: true
```

---

## 71. TRANSLATION_PROVIDER_CONNECTION_INTERRUPTED

The connection was lost during provider execution.

```text
category: PROVIDER_EXECUTION
scope: BATCH
severity: ERROR
retryable: true
```

Partial provider output must not be accepted unless it passes normal structural validation.

---

## 72. TRANSLATION_QUEUE_TIMEOUT

Runtime reported that execution admission or queue waiting exceeded the permitted deadline for this Translation job. Translation does not own the queue implementation.

```text
category: TIMEOUT
scope: JOB
severity: ERROR
retryable: conditional
```

State consequence:

```text
QUEUED → FAILED
```

or a new runtime admission request according to policy. Runtime owns whether and when execution is admitted again.

---

## 73. TRANSLATION_ATTEMPT_TIMEOUT

An attempt exceeded its allowed execution duration.

```text
category: TIMEOUT
scope: ATTEMPT
severity: ERROR
retryable: true
```

State consequence:

```text
Attempt RUNNING → FAILED
Job RUNNING → RETRY_SCHEDULED or FAILED
```

---

## 74. TRANSLATION_BATCH_TIMEOUT

A batch exceeded its allowed execution duration.

```text
category: TIMEOUT
scope: BATCH
severity: ERROR
retryable: true
```

Successful sibling batches remain completed.

---

## 75. TRANSLATION_JOB_TIMEOUT

The total job deadline was exceeded.

```text
category: TIMEOUT
scope: JOB
severity: ERROR
retryable: false within the same job
```

State consequence:

```text
Job → FAILED
```

A caller deadline cancellation may instead produce `CANCELLED`.

---

# Part XI — Provider Execution Errors

## 76. TRANSLATION_PROVIDER_REQUEST_REJECTED

The provider rejected a syntactically valid request for a non-authentication reason.

```text
category: PROVIDER_EXECUTION
scope: BATCH
severity: ERROR
retryable: conditional
```

Examples:

* unsupported parameter combination;
* content rejected;
* model unavailable for account;
* unsupported language pair.

---

## 77. TRANSLATION_PROVIDER_CONTENT_REJECTED

The provider refused to process the supplied content.

```text
category: PROVIDER_EXECUTION
scope: BATCH
severity: ERROR
retryable: conditional
```

Recovery may include:

* using another eligible provider;
* using a local provider;
* reducing context;
* informing the user.

Translation must not falsely report content rejection as a network error.

---

## 78. TRANSLATION_PROVIDER_INTERNAL_ERROR

The provider reported an internal server failure.

```text
category: PROVIDER_EXECUTION
scope: BATCH or ATTEMPT
severity: ERROR
retryable: true
```

---

## 79. TRANSLATION_PROVIDER_MODEL_UNAVAILABLE

The selected model is temporarily or permanently unavailable.

```text
category: PROVIDER_AVAILABILITY
scope: ATTEMPT
severity: ERROR
retryable: true with fallback
```

---

## 80. TRANSLATION_PROVIDER_CANCELLED

The provider acknowledged cancellation.

```text
category: CANCELLATION
scope: BATCH or ATTEMPT
severity: NOTICE
retryable: false
```

This is normally recorded as lifecycle information rather than surfaced as a user-facing failure.

---

## 81. TRANSLATION_LOCAL_MODEL_LOAD_FAILED

A local translation model could not be loaded.

```text
category: PROVIDER_EXECUTION
scope: PROVIDER
severity: ERROR
retryable: conditional
```

Possible causes:

* model files missing;
* insufficient memory;
* incompatible runtime;
* corrupted model;
* initialization timeout.

---

## 82. TRANSLATION_LOCAL_RESOURCE_EXHAUSTED

Local execution lacks required CPU, GPU or memory resources.

```text
category: PROVIDER_EXECUTION
scope: ATTEMPT
severity: ERROR
retryable: conditional
```

Recovery:

```text
REDUCE_BATCH_SIZE
WAIT_AND_RETRY
USE_FALLBACK_PROVIDER
```

---

# Part XII — Provider Response Errors

## 83. TRANSLATION_PROVIDER_RESPONSE_EMPTY

The provider returned no usable output.

```text
category: PROVIDER_RESPONSE
scope: BATCH
severity: ERROR
retryable: true
```

---

## 84. TRANSLATION_PROVIDER_RESPONSE_MALFORMED

The provider response could not be parsed according to the adapter contract.

```text
category: PROVIDER_RESPONSE
scope: BATCH
severity: ERROR
retryable: true
```

Raw response content must not be exposed publicly.

---

## 85. TRANSLATION_PROVIDER_RESPONSE_TRUNCATED

The provider response ended before the expected structured output completed.

```text
category: PROVIDER_RESPONSE
scope: BATCH
severity: ERROR or DEGRADED
retryable: true
```

Accepted complete segments may be preserved.

---

## 86. TRANSLATION_PROVIDER_RESPONSE_UNEXPECTED_FORMAT

The response is syntactically parseable but does not match the requested output structure.

```text
category: PROVIDER_RESPONSE
scope: BATCH
severity: ERROR
retryable: true
```

---

## 87. TRANSLATION_PROVIDER_SEGMENT_ID_UNKNOWN

The provider returned an identifier that was not included in the batch.

```text
category: ALIGNMENT
scope: BATCH
severity: ERROR
retryable: true
```

Unknown output must never be silently attached to another segment.

---

## 88. TRANSLATION_PROVIDER_SEGMENT_ID_DUPLICATED

The provider returned multiple output items for one expected segment without an allowed variant structure.

```text
category: ALIGNMENT
scope: BATCH
severity: ERROR
retryable: true
```

---

## 89. TRANSLATION_PROVIDER_SEGMENT_MISSING

The provider omitted one or more required segments.

```text
category: ALIGNMENT
scope: BATCH
severity: DEGRADED or ERROR
retryable: true
```

Missing segment IDs must be explicit.

---

# Part XIII — Output Validation Errors

## 90. TRANSLATION_OUTPUT_VALIDATION_FAILED

A general normalized error when translated output fails one or more required validators.

```text
category: OUTPUT_VALIDATION
scope: BATCH or RESULT
severity: ERROR
retryable: conditional
```

A more specific error code should be preferred where possible.

---

## 91. TRANSLATION_OUTPUT_EMPTY

A translatable source segment produced empty output.

```text
category: OUTPUT_VALIDATION
scope: SEGMENT
severity: ERROR
retryable: true
```

An intentionally preserved empty output must be represented explicitly and should not use this error.

---

## 92. TRANSLATION_OUTPUT_SOURCE_LEAKAGE

The output appears to contain excessive untranslated source text.

```text
category: OUTPUT_VALIDATION
scope: SEGMENT or BATCH
severity: DEGRADED or ERROR
retryable: true
```

Mixed-language names and locked source terms must not automatically trigger this error.

---

## 93. TRANSLATION_OUTPUT_TARGET_LANGUAGE_MISMATCH

The output is not primarily in the configured target language.

```text
category: OUTPUT_VALIDATION
scope: SEGMENT or BATCH
severity: ERROR
retryable: true
```

---

## 94. TRANSLATION_OUTPUT_CONTROL_TEXT_LEAKED

Provider instructions, structural markers or internal control syntax appeared in public translated text.

```text
category: OUTPUT_VALIDATION
scope: BATCH
severity: ERROR
retryable: true
```

Examples:

```text
SYSTEM:
TRANSLATION:
JSON wrapper
internal segment markers
provider refusal preamble
```

---

## 95. TRANSLATION_OUTPUT_LENGTH_INVALID

Output length violates hard configured limits.

```text
category: OUTPUT_VALIDATION
scope: SEGMENT
severity: ERROR
retryable: conditional
```

A merely long output should normally produce a warning rather than an error.

---

## 96. TRANSLATION_OUTPUT_DUPLICATED

Multiple segments contain suspicious duplicated output caused by provider failure.

```text
category: OUTPUT_VALIDATION
scope: BATCH
severity: ERROR
retryable: true
```

---

## 97. TRANSLATION_OUTPUT_INCOMPLETE

Output is syntactically valid but semantically or structurally incomplete.

```text
category: OUTPUT_VALIDATION
scope: SEGMENT or BATCH
severity: DEGRADED or ERROR
retryable: true
```

Examples:

* sentence cut off;
* dialogue ends mid-clause;
* only part of source translated;
* provider stopped before completing the segment.

---

## 98. TRANSLATION_OUTPUT_UNSAFE_STRUCTURE

Output contains structural content that cannot safely be passed to Presentation.

```text
category: OUTPUT_VALIDATION
scope: SEGMENT
severity: ERROR
retryable: true
```

This concerns contract safety, not general content moderation.

---

# Part XIV — Alignment Errors

## 99. TRANSLATION_ALIGNMENT_FAILED

The module cannot reliably map provider output to prepared segments.

```text
category: ALIGNMENT
scope: BATCH or RESULT
severity: ERROR
retryable: conditional
```

No ambiguous output may become authoritative.

---

## 100. TRANSLATION_ALIGNMENT_SEGMENT_MISSING

A selected prepared segment has no corresponding translated segment in final assembly.

```text
category: ALIGNMENT
scope: RESULT
severity: ERROR or DEGRADED
retryable: true
```

---

## 101. TRANSLATION_ALIGNMENT_DUPLICATE_TARGET

More than one authoritative translated segment maps to the same prepared segment within one variant.

```text
category: ALIGNMENT
scope: RESULT
severity: CRITICAL
retryable: false through normal provider retry
```

This indicates an assembly or persistence defect.

---

## 102. TRANSLATION_ALIGNMENT_UNKNOWN_SOURCE

A translated segment references a prepared segment outside the job source identity.

```text
category: ALIGNMENT
scope: RESULT
severity: CRITICAL
retryable: false
```

The result must be invalidated.

---

## 103. TRANSLATION_ALIGNMENT_ORDER_UNRESOLVABLE

Source sequence information is missing or contradictory, preventing deterministic result assembly.

```text
category: ALIGNMENT
scope: RESULT
severity: ERROR
retryable: false without source repair
```

Recovery:

```text
REBUILD_PREPARED_DOCUMENT
```

---

## 104. TRANSLATION_ALIGNMENT_REVISION_MISMATCH

Translated output references a different prepared content revision.

```text
category: ALIGNMENT
scope: RESULT
severity: ERROR
retryable: false within current job
```

State consequence:

```text
Result → NON_AUTHORITATIVE or INVALIDATED
Job → SUPERSEDED or FAILED
```

---

# Part XV — Batch Construction Errors

## 105. TRANSLATION_BATCH_EMPTY

A batch was created without translatable segments.

```text
category: INTERNAL
scope: BATCH
severity: ERROR
retryable: false without reconstruction
```

This is an orchestration defect.

---

## 106. TRANSLATION_BATCH_DUPLICATE_SEGMENT

The same prepared segment was assigned more than once within one batch.

```text
category: INTERNAL
scope: BATCH
severity: ERROR
retryable: false without reconstruction
```

---

## 107. TRANSLATION_BATCH_SEGMENT_CONFLICT

A segment was assigned to incompatible active batches within the same attempt.

```text
category: CONCURRENCY
scope: ATTEMPT
severity: ERROR
retryable: false until attempt reconstruction
```

---

## 108. TRANSLATION_BATCH_LIMIT_EXCEEDED

The constructed batch exceeds resolved provider or execution limits.

```text
category: CONFIGURATION
scope: BATCH
severity: ERROR
retryable: true after reconstruction
```

Recovery:

```text
REDUCE_BATCH_SIZE
REDUCE_CONTEXT_SIZE
```

---

## 109. TRANSLATION_BATCH_CONSTRUCTION_FAILED

The module could not form valid batches.

```text
category: INTERNAL
scope: ATTEMPT
severity: ERROR
retryable: conditional
```

Possible causes:

* invalid segment grouping;
* impossible locked group size;
* provider limit conflict;
* context boundary conflict.

---

# Part XVI — Retry and Fallback Errors

## 110. TRANSLATION_RETRY_NOT_ALLOWED

A retry was requested for a non-retryable failure or terminal job.

```text
category: COMMAND_VALIDATION
scope: JOB
severity: NOTICE
retryable: false
```

---

## 111. TRANSLATION_RETRY_LIMIT_EXCEEDED

The job exhausted its configured attempt budget.

```text
category: PROVIDER_EXECUTION
scope: JOB
severity: ERROR
retryable: false within current job
```

State consequence:

```text
Job → FAILED
```

---

## 112. TRANSLATION_RETRY_ALREADY_ACTIVE

A retry command was received while an equivalent attempt is already active.

```text
category: CONCURRENCY
scope: JOB
severity: NOTICE
retryable: false immediately
```

The command may return the active attempt reference.

---

## 113. TRANSLATION_FALLBACK_NOT_AVAILABLE

No eligible fallback provider exists.

```text
category: PROVIDER_SELECTION
scope: JOB
severity: ERROR
retryable: conditional
```

---

## 114. TRANSLATION_FALLBACK_LIMIT_EXCEEDED

The configured maximum number of provider fallbacks was reached.

```text
category: PROVIDER_SELECTION
scope: JOB
severity: ERROR
retryable: false within current job
```

---

## 115. TRANSLATION_FALLBACK_POLICY_BLOCKED

Fallback was possible technically but prohibited by policy.

```text
category: PROVIDER_SELECTION
scope: JOB
severity: NOTICE or ERROR
retryable: false without policy change
```

---

# Part XVII — Cancellation and Supersession Outcomes

## 116. TRANSLATION_CANCELLED

Normalized lifecycle reason representing intentional cancellation.

```text
category: CANCELLATION
scope: JOB
severity: NOTICE
retryable: false
```

This should normally map to:

```text
Job → CANCELLED
```

It is not treated as provider failure.

---

## 117. TRANSLATION_CANCELLATION_NOT_ALLOWED

Cancellation was requested for an entity that cannot be cancelled.

Examples:

* invalid identifier;
* already invalidated entity;
* unsupported cancellation scope.

```text
category: COMMAND_VALIDATION
scope: COMMAND
severity: NOTICE
retryable: false
```

---

## 118. TRANSLATION_ALREADY_TERMINAL

A state-changing command targeted an already terminal job.

```text
category: COMMAND_VALIDATION
scope: JOB
severity: NOTICE
retryable: false
```

The response should include current terminal state where safe.

---

## 119. TRANSLATION_SUPERSEDED

The job became obsolete because newer work replaced it.

```text
category: SUPERSESSION
scope: JOB
severity: NOTICE
retryable: false
```

State consequence:

```text
Job → SUPERSEDED
```

This is not a system failure.

---

## 120. TRANSLATION_STALE_RESULT_REJECTED

A completed provider result was rejected because it no longer matched current authority. Reading Session may be the authority that rejects the result when the active content revision or reading context has changed.

```text
category: SUPERSESSION
scope: RESULT
severity: NOTICE
retryable: false
```

Possible causes:

* newer job active;
* source revision changed;
* cancellation completed;
* attempt replaced;
* target language changed;
* Translation intent changed;
* an older Translation revision lost authority.

This outcome is lifecycle control, not necessarily a technical failure.

---

## 121. TRANSLATION_STALE_ATTEMPT_REJECTED

Output arrived from an attempt that was no longer active.

```text
category: SUPERSESSION
scope: ATTEMPT
severity: NOTICE
retryable: false
```

This should normally remain an internal or observability error.

---

# Part XVIII — Result Assembly and Publication Errors

## 122. TRANSLATION_RESULT_ASSEMBLY_FAILED

Accepted batch outputs could not be assembled into a coherent result.

```text
category: RESULT_ASSEMBLY
scope: RESULT
severity: ERROR
retryable: conditional
```

Possible causes:

* conflicting segment mappings;
* missing source sequence;
* incompatible result revisions;
* duplicate translated segments.

---

## 123. TRANSLATION_RESULT_INCOMPLETE

Final assembly is missing required segments.

```text
category: RESULT_ASSEMBLY
scope: RESULT
severity: DEGRADED or ERROR
retryable: conditional
```

Behavior depends on publication policy.

---

## 124. TRANSLATION_RESULT_REVISION_CONFLICT

Concurrent result assembly attempted to publish incompatible revisions.

```text
category: CONCURRENCY
scope: RESULT
severity: ERROR
retryable: true
```

Older revision must not overwrite newer revision.

---

## 125. TRANSLATION_RESULT_NOT_FOUND

A referenced result does not exist.

```text
category: RESULT_ASSEMBLY
scope: RESULT
severity: NOTICE or ERROR
retryable: conditional
```

---

## 126. TRANSLATION_RESULT_NOT_AUTHORITATIVE

The requested result exists but is not eligible for active use.

```text
category: PUBLICATION
scope: RESULT
severity: NOTICE
retryable: false
```

Possible reasons:

* superseded;
* cancelled;
* stale;
* historical;
* partial and not publishable.

---

## 127. TRANSLATION_RESULT_PUBLICATION_FAILED

The result was valid but could not be published or activated.

```text
category: PUBLICATION
scope: RESULT
severity: ERROR
retryable: true
```

Possible causes:

* event publication failure;
* projection persistence failure;
* variant activation failure;
* transactional outbox failure.

The result may already exist durably even when publication fails. The system must avoid reporting completion before the result becomes retrievable and the corresponding durable state can be queried.

---

## 128. TRANSLATION_RESULT_PERSISTENCE_FAILED

The result could not be durably stored.

```text
category: INTERNAL
scope: RESULT
severity: CRITICAL
retryable: true
```

No `TranslationCompleted` or `TranslationCompletedWithWarnings` event may be published until durable storage succeeds.

---

# Part XIX — Variant Errors

## 129. TRANSLATION_VARIANT_NOT_FOUND

The requested translation variant does not exist.

```text
category: VARIANT
scope: VARIANT
severity: NOTICE
retryable: false
```

---

## 130. TRANSLATION_VARIANT_INCOMPATIBLE

The variant does not match the active:

* prepared document;
* content revision;
* target language;
* reading context;
* Translation intent.

```text
category: VARIANT
scope: VARIANT
severity: NOTICE
retryable: false
```

---

## 131. TRANSLATION_VARIANT_INVALIDATED

The selected variant has been invalidated.

```text
category: VARIANT
scope: VARIANT
severity: NOTICE
retryable: false
```

Recovery:

```text
SELECT_ANOTHER_VARIANT
REQUEST_RETRANSLATION
```

---

## 132. TRANSLATION_VARIANT_ACTIVATION_CONFLICT

Concurrent activation produced a conflict between variants.

```text
category: CONCURRENCY
scope: VARIANT
severity: ERROR
retryable: true
```

The invariant remains:

```text
At most one ACTIVE variant per:

ReadingSessionId
PreparedDocumentId
ContentRevision
TargetLanguage
TranslationIntentId
```

---

## 133. TRANSLATION_VARIANT_CREATION_FAILED

A result could not be converted into an immutable translation variant.

```text
category: VARIANT
scope: VARIANT
severity: ERROR
retryable: conditional
```

The job must not publish successful completion if its required variant cannot be retrieved.

---

## 134. TRANSLATION_CORRECTION_INVALID

A submitted correction is malformed or targets incompatible segments.

```text
category: VARIANT
scope: COMMAND
severity: NOTICE
retryable: false
```

---

## 135. TRANSLATION_CORRECTION_BASE_MISMATCH

The correction targets a base variant or source revision that no longer matches.

```text
category: VARIANT
scope: VARIANT
severity: NOTICE
retryable: false
```

A new correction should be based on the current compatible variant.

---

# Part XX — Cache Errors

## 136. TRANSLATION_CACHE_READ_FAILED

The cache could not be queried.

```text
category: CACHE
scope: CACHE
severity: DEGRADED
retryable: true
```

Default behavior:

```text
Continue without cache when execution policy permits.
```

Cache failure must not automatically fail translation.

---

## 137. TRANSLATION_CACHE_WRITE_FAILED

A completed result could not be written to cache.

```text
category: CACHE
scope: CACHE
severity: DEGRADED
retryable: true
```

The translation result may still complete successfully.

---

## 138. TRANSLATION_CACHE_ENTRY_INCOMPATIBLE

A cache entry exists but does not match the required semantic input identity.

```text
category: CACHE
scope: CACHE
severity: NOTICE
retryable: false
```

The entry must not be reused.

Normal provider execution may continue.

---

## 139. TRANSLATION_CACHE_ENTRY_CORRUPTED

The cache entry cannot be parsed or fails integrity validation.

```text
category: CACHE
scope: CACHE
severity: DEGRADED
retryable: false for that entry
```

Recovery:

```text
Invalidate cache entry and execute translation normally.
```

---

## 140. TRANSLATION_CACHE_ALIGNMENT_MISMATCH

Cached translated segments do not align with the current prepared segments.

```text
category: CACHE
scope: CACHE
severity: ERROR
retryable: false for that entry
```

The cache entry must be invalidated.

---

# Part XXI — Concurrency and Persistence Errors

## 141. TRANSLATION_STATE_CONFLICT

A state transition failed because the entity was no longer in the expected state.

```text
category: CONCURRENCY
scope: JOB, ATTEMPT, BATCH, RESULT or VARIANT
severity: NOTICE or ERROR
retryable: conditional
```

Examples:

* completion lost race to cancellation;
* retry lost race to supersession;
* activation lost race to another variant.

---

## 142. TRANSLATION_STATE_REVISION_CONFLICT

The expected entity revision differed from the stored revision.

```text
category: CONCURRENCY
scope: MODULE ENTITY
severity: NOTICE
retryable: true
```

The caller should reload authoritative state before retrying.

---

## 143. TRANSLATION_DUPLICATE_ACTIVE_ATTEMPT

More than one active attempt exists for the same Translation batch where policy permits only one active execution attempt.

```text
category: CONCURRENCY
scope: BATCH
severity: CRITICAL
retryable: false until reconciliation
```

---

## 144. TRANSLATION_DUPLICATE_ACTIVE_VARIANT

More than one compatible variant is active for the same reading context.

```text
category: CONCURRENCY
scope: VARIANT
severity: CRITICAL
retryable: false until reconciliation
```

---

## 145. TRANSLATION_EVENT_PUBLICATION_FAILED

A committed transition could not be published to the Event Bus.

```text
category: PUBLICATION
scope: MODULE
severity: ERROR
retryable: true
```

A transactional outbox or equivalent mechanism should retry publication.

The state transition must not be repeated as a new business action.

---

## 146. TRANSLATION_EVENT_SEQUENCE_CONFLICT

An event could not receive a valid monotonic job sequence.

```text
category: CONCURRENCY
scope: JOB
severity: ERROR
retryable: true
```

---

# Part XXII — Security and Privacy Errors

## 147. TRANSLATION_REMOTE_EXECUTION_PROHIBITED

The selected execution path would transmit content remotely against policy.

```text
category: PRIVACY
scope: ATTEMPT
severity: ERROR
retryable: true with local provider
```

---

## 148. TRANSLATION_DATA_REGION_PROHIBITED

The selected provider region is not allowed.

```text
category: PRIVACY
scope: ATTEMPT
severity: ERROR
retryable: true with another region or provider
```

---

## 149. TRANSLATION_SENSITIVE_CONTENT_LOGGING_BLOCKED

An operation attempted to write raw source or translated content into a restricted log channel.

```text
category: SECURITY
scope: MODULE
severity: CRITICAL
retryable: false until corrected
```

The content must not be logged.

---

## 150. TRANSLATION_CREDENTIAL_EXPOSURE_BLOCKED

An operation attempted to include provider credentials in a public error, event or result.

```text
category: SECURITY
scope: MODULE
severity: CRITICAL
retryable: false until corrected
```

---

## 151. TRANSLATION_UNTRUSTED_INSTRUCTION_DETECTED

Source content appears to contain instructions intended to manipulate an LLM provider.

```text
category: SECURITY
scope: BATCH
severity: DEGRADED
retryable: conditional
```

The source remains data.

Possible behavior:

* continue using hardened provider instructions;
* add a warning;
* reject output if structural control was compromised;
* retry using another provider.

The presence of instruction-like text alone must not automatically block legitimate translation.

---

## 152. TRANSLATION_PROVIDER_INSTRUCTION_LEAKAGE

Provider output reveals internal translation instructions or prompt fragments.

```text
category: SECURITY
scope: BATCH
severity: ERROR
retryable: true
```

The output must not become public.

---

# Part XXIII — Internal Errors

## 153. TRANSLATION_INTERNAL_ERROR

An unexpected internal failure occurred.

```text
category: INTERNAL
scope: MODULE
severity: ERROR
retryable: conditional
```

This is the fallback code when no more specific normalized error applies.

---

## 154. TRANSLATION_CONFIGURATION_RESOLUTION_FAILED

The module could not resolve an immutable configuration snapshot.

```text
category: INTERNAL
scope: JOB
severity: ERROR
retryable: conditional
```

---

## 155. TRANSLATION_PROVIDER_ADAPTER_ERROR

A provider adapter failed outside a recognized provider response category.

```text
category: INTERNAL
scope: PROVIDER
severity: ERROR
retryable: conditional
```

The public contract must still avoid adapter stack traces and raw payloads.

---

## 156. TRANSLATION_SERIALIZATION_FAILED

An internal provider-neutral request, result or event could not be serialized.

```text
category: INTERNAL
scope: MODULE
severity: ERROR
retryable: conditional
```

---

## 157. TRANSLATION_DESERIALIZATION_FAILED

Stored or transmitted Translation data could not be deserialized.

```text
category: INTERNAL
scope: MODULE
severity: ERROR
retryable: conditional
```

---

## 158. TRANSLATION_INVARIANT_VIOLATED

A core Translation invariant was violated.

```text
category: INTERNAL
scope: MODULE
severity: CRITICAL
retryable: false until reconciliation
```

Examples:

* translated segment without source segment;
* cancelled job publishing authoritative result;
* invalidated variant becoming active;
* failed batch returning to running;
* one batch belonging to multiple attempts.

---

# Part XXIV — Warning Contract

## 159. TranslationWarning

The normalized warning model is:

```text
TranslationWarning {
    warningId

    code
    category
    severity

    message
    userMessageKey

    translationJobId
    translationAttemptId
    translationBatchId
    translationResultId
    translationVariantId

    affectedPreparedSegmentIds[]

    suggestedActions[]

    metadata
}
```

---

## 160. Warning Severity

Canonical warning severities:

```text
INFO
NOTICE
DEGRADED
```

### INFO

Informational behavior worth recording.

Example:

```text
Cache result reused.
```

### NOTICE

A limitation exists but the translation remains normally usable.

Example:

```text
Sound effect preserved in source language.
```

### DEGRADED

Translation is usable but quality or completeness may be materially reduced.

Example:

```text
Required context was unavailable.
```

---

# Part XXV — Warning Catalog

## 161. TRANSLATION_WARNING_MISSING_CONTEXT

Some desired context could not be supplied.

```text
category: CONTEXT
severity: DEGRADED
```

Translation continued under best-effort policy.

---

## 162. TRANSLATION_WARNING_CONTEXT_TRUNCATED

Context was reduced to fit configured or provider limits.

```text
category: CONTEXT
severity: NOTICE
```

---

## 163. TRANSLATION_WARNING_LOW_CONFIDENCE

One or more translated segments have low normalized confidence.

```text
category: OUTPUT_VALIDATION
severity: DEGRADED
```

Confidence must not be presented as objective truth.

---

## 164. TRANSLATION_WARNING_AMBIGUOUS_MEANING

The source allows multiple plausible interpretations.

```text
category: OUTPUT_VALIDATION
severity: NOTICE
```

---

## 165. TRANSLATION_WARNING_PRONOUN_AMBIGUITY

The relationship, gender, rank or formality required for Vietnamese pronouns was unclear.

```text
category: KNOWLEDGE
severity: NOTICE
```

---

## 166. TRANSLATION_WARNING_TERMINOLOGY_CONFLICT_RESOLVED

Conflicting non-fatal terminology constraints were resolved using configured policy.

```text
category: KNOWLEDGE
severity: NOTICE
```

The chosen resolution should be recorded in bounded metadata.

---

## 167. TRANSLATION_WARNING_PROVIDER_FALLBACK_USED

The preferred provider failed or was unavailable, and a fallback provider completed the work.

```text
category: PROVIDER_SELECTION
severity: NOTICE
```

---

## 168. TRANSLATION_WARNING_RETRY_USED

Translation required one or more retry attempts.

```text
category: PROVIDER_EXECUTION
severity: INFO
```

This warning need not be shown to the user unless latency or quality was affected.

---

## 169. TRANSLATION_WARNING_PARTIAL_RESULT

Only a subset of selected segments is currently available.

```text
category: RESULT_ASSEMBLY
severity: DEGRADED
```

Missing and failed segment IDs must be explicit.

---

## 170. TRANSLATION_WARNING_SOURCE_INCOMPLETE

The prepared source appears incomplete.

```text
category: SOURCE
severity: DEGRADED
```

Examples:

* OCR text cut off;
* sentence fragment;
* missing dialogue continuation.

Translation must not repair the upstream source silently.

---

## 171. TRANSLATION_WARNING_SOURCE_LANGUAGE_UNCERTAIN

Source language detection confidence was insufficient for certainty.

```text
category: SOURCE
severity: NOTICE
```

---

## 172. TRANSLATION_WARNING_MIXED_LANGUAGE_CONTENT

The source contains multiple languages.

```text
category: SOURCE
severity: INFO or NOTICE
```

This is not automatically a failure.

---

## 173. TRANSLATION_WARNING_UNTRANSLATED_FRAGMENT

A bounded source fragment remained untranslated.

```text
category: OUTPUT_VALIDATION
severity: DEGRADED
```

Locked names, symbols and intentional preservation must be excluded.

---

## 174. TRANSLATION_WARNING_OUTPUT_LONGER_THAN_SOURCE

Translated output is significantly longer than its source segment.

```text
category: OUTPUT_VALIDATION
severity: NOTICE
```

This is especially relevant to comic presentation.

Presentation decides how to fit the text.

---

## 175. TRANSLATION_WARNING_OUTPUT_SHORTER_THAN_EXPECTED

Output is suspiciously short but still passed minimum validation.

```text
category: OUTPUT_VALIDATION
severity: DEGRADED
```

---

## 176. TRANSLATION_WARNING_SOUND_EFFECT_PRESERVED

A comic sound effect was preserved instead of translated.

```text
category: OUTPUT_VALIDATION
severity: INFO
```

---

## 177. TRANSLATION_WARNING_SOUND_EFFECT_TRANSLITERATED

A sound effect was transliterated rather than semantically translated.

```text
category: OUTPUT_VALIDATION
severity: INFO
```

---

## 178. TRANSLATION_WARNING_CACHE_RESULT_REUSED

A compatible cached translation result was reused.

```text
category: CACHE
severity: INFO
```

This normally does not need user display.

---

## 179. TRANSLATION_WARNING_PROVIDER_USAGE_ESTIMATED

Provider usage values are estimated rather than provider-reported.

```text
category: PROVIDER_RESPONSE
severity: INFO
```

---

## 180. TRANSLATION_WARNING_PROVIDER_METADATA_INCOMPLETE

Optional provider usage or request metadata was unavailable.

```text
category: PROVIDER_RESPONSE
severity: INFO
```

Translation output remains valid.

---

## 181. TRANSLATION_WARNING_GLOSSARY_NOT_APPLIED

Optional glossary data could not be applied.

```text
category: KNOWLEDGE
severity: DEGRADED
```

Locked glossary failure must be an error, not this warning.

---

## 182. TRANSLATION_WARNING_NAME_MAPPING_UNCERTAIN

A proper name could not be mapped confidently.

```text
category: KNOWLEDGE
severity: NOTICE
```

---

## 183. TRANSLATION_WARNING_HONORIFIC_NORMALIZED

Source honorifics were adapted according to the selected Vietnamese profile.

```text
category: KNOWLEDGE
severity: INFO
```

---

# Part XXVI — Error-to-State Mapping

## 184. Command Errors

Command validation errors normally produce no Translation job.

```text
Command rejected
    ↓
No job state created
```

Examples:

```text
TRANSLATION_COMMAND_INVALID
TRANSLATION_TARGET_LANGUAGE_REQUIRED
TRANSLATION_PROVIDER_POLICY_UNSATISFIABLE
```

---

## 185. Source Errors

| Error                                   | Typical state consequence                              |
| --------------------------------------- | ------------------------------------------------------ |
| `TRANSLATION_SOURCE_NOT_FOUND`          | Job `FAILED`                                           |
| `TRANSLATION_SOURCE_REVISION_NOT_FOUND` | Job `FAILED`                                           |
| `TRANSLATION_SOURCE_REVISION_MISMATCH`  | Job `SUPERSEDED` or `FAILED`                           |
| `TRANSLATION_SOURCE_EMPTY`              | Job `FAILED` or successful no-op                       |
| `TRANSLATION_SOURCE_SEGMENT_EMPTY`      | Job `PARTIALLY_COMPLETED` or `COMPLETED_WITH_WARNINGS` |
| `TRANSLATION_SOURCE_CONTENT_TOO_LARGE`  | Job `FAILED`                                           |

---

## 186. Attempt Errors

Retryable attempt errors produce:

```text
Attempt RUNNING
    ↓
Attempt FAILED
    ↓
Job RETRY_SCHEDULED
```

Non-retryable or exhausted failures produce:

```text
Attempt FAILED
    ↓
Job FAILED
```

---

## 187. Batch Errors

A batch failure produces:

```text
Batch RUNNING or VALIDATING
    ↓
Batch FAILED
```

The attempt may become:

```text
PARTIALLY_COMPLETED
FAILED
```

The job may remain:

```text
RUNNING
PARTIALLY_COMPLETED
RETRY_SCHEDULED
```

---

## 188. Alignment Errors

Batch-level alignment errors usually produce:

```text
Batch VALIDATING → FAILED
```

Final result alignment errors produce:

```text
Result FINALIZING → INVALIDATED
Job RUNNING → FAILED
```

Severe alignment defects discovered after publication produce:

```text
Result AVAILABLE → INVALIDATED
Variant ACTIVE → INVALIDATED
Job COMPLETED → INVALIDATED
```

---

## 189. Cancellation Outcomes

```text
TRANSLATION_CANCELLED
    ↓
Job CANCELLATION_REQUESTED → CANCELLED
```

Cancellation must not become `FAILED` unless a separate cleanup failure affects module integrity.

---

## 190. Supersession Outcomes

```text
TRANSLATION_SUPERSEDED
    ↓
Job → SUPERSEDED
Result → NON_AUTHORITATIVE
Variant → INACTIVE
```

---

## 191. Cache Errors

Cache errors should normally degrade rather than fail the job.

```text
Cache read failed
    ↓
Continue provider execution
    ↓
Attach warning or operational error
```

Strict offline cache-only mode may treat cache failure as fatal.

---

# Part XXVII — Retry Classification

## 192. Always Retryable Examples

Subject to retry budget:

```text
TRANSLATION_PROVIDER_TIMEOUT
TRANSLATION_BATCH_TIMEOUT
TRANSLATION_PROVIDER_RATE_LIMITED
TRANSLATION_PROVIDER_INTERNAL_ERROR
TRANSLATION_PROVIDER_CONNECTION_FAILED
TRANSLATION_PROVIDER_RESPONSE_MALFORMED
TRANSLATION_PROVIDER_RESPONSE_EMPTY
```

---

## 193. Retryable After Request Adjustment

```text
TRANSLATION_PROVIDER_REQUEST_TOO_LARGE
TRANSLATION_CONTEXT_TOO_LARGE
TRANSLATION_BATCH_LIMIT_EXCEEDED
TRANSLATION_PROVIDER_OUTPUT_LIMIT_EXCEEDED
```

Required adjustment may include:

* smaller batches;
* reduced context;
* fewer glossary entries;
* different provider.

---

## 194. Retryable With Another Provider

```text
TRANSLATION_PROVIDER_UNAVAILABLE
TRANSLATION_PROVIDER_CAPABILITY_MISSING
TRANSLATION_PROVIDER_MODEL_UNAVAILABLE
TRANSLATION_PROVIDER_CONTENT_REJECTED
TRANSLATION_REMOTE_EXECUTION_PROHIBITED
```

Provider policy must permit fallback.

---

## 195. Not Retryable Within the Same Job

```text
TRANSLATION_SOURCE_REVISION_MISMATCH
TRANSLATION_TARGET_LANGUAGE_REQUIRED
TRANSLATION_PROFILE_NOT_FOUND
TRANSLATION_GLOSSARY_REVISION_NOT_FOUND
TRANSLATION_PROVIDER_POLICY_UNSATISFIABLE
TRANSLATION_RESULT_NOT_AUTHORITATIVE
TRANSLATION_VARIANT_INCOMPATIBLE
TRANSLATION_CANCELLED
TRANSLATION_SUPERSEDED
```

A new job or corrected command may be required.

---

## 196. Never Automatically Retry

```text
TRANSLATION_PROVIDER_AUTHENTICATION_FAILED
TRANSLATION_PROVIDER_AUTHORIZATION_FAILED
TRANSLATION_PROVIDER_QUOTA_EXCEEDED
TRANSLATION_INVARIANT_VIOLATED
TRANSLATION_CREDENTIAL_EXPOSURE_BLOCKED
TRANSLATION_DUPLICATE_ACTIVE_VARIANT
```

These require intervention or reconciliation.

---

# Part XXVIII — Provider Error Normalization

## 197. Normalization Boundary

Provider adapters convert provider-native failures into normalized Translation errors.

```text
Provider-native error
        ↓
Provider adapter
        ↓
Normalized TranslationError
```

Translation core must not switch logic directly on provider-native error types.

---

## 198. Normalization Priority

Adapters should classify failures in this order:

```text
Cancellation
Authentication or authorization
Rate limit or quota
Request limit
Timeout
Connection
Provider availability
Content rejection
Response parsing
Unknown provider failure
```

Specific classifications should take precedence over generic HTTP status mappings.

---

## 199. HTTP Status Guidance

Indicative mapping only:

| Provider response | Normalized category                          |
| ----------------- | -------------------------------------------- |
| `400`             | request rejected or invalid provider request |
| `401`             | authentication failed                        |
| `403`             | authorization failed or content rejected     |
| `404`             | provider model or endpoint unavailable       |
| `408`             | provider timeout                             |
| `413`             | request too large                            |
| `429`             | rate limited or quota exceeded               |
| `5xx`             | provider unavailable or internal error       |

Provider documentation and response semantics take precedence over generic HTTP interpretation.

---

## 200. Unknown Provider Error

When an adapter cannot classify a failure:

```text
code: TRANSLATION_PROVIDER_UNKNOWN_ERROR
category: PROVIDER_EXECUTION
scope: ATTEMPT or BATCH
retryable: conditional
```

Internal diagnostics may preserve:

* provider-native error type;
* redacted status;
* provider request ID;
* safe response metadata.

---

## 201. Raw Error Retention

Raw provider errors may be retained only in restricted diagnostic storage when:

* credentials are redacted;
* private source and translated content are removed or protected;
* retention policy permits it;
* access is controlled.

Raw provider errors must not be placed in public events.

---

# Part XXIX — Partial Failure Behavior

## 202. Partial Failure Definition

Partial failure occurs when:

* at least one selected segment completed;
* at least one selected segment failed or remains missing.

```text
completed subset
+
failed or missing subset
=
partial result
```

---

## 203. Partial Failure Representation

A partial result must include:

```text
completedPreparedSegmentIds[]
failedPreparedSegmentIds[]
missingPreparedSegmentIds[]
warnings[]
failure summaries
```

No selected segment may disappear silently.

---

## 204. Partial Failure Outcomes

Depending on policy, a partial failure may produce:

```text
Job PARTIALLY_COMPLETED
```

followed by retry.

After retry exhaustion:

```text
Job FAILED
```

or, when partial success is accepted:

```text
Job COMPLETED_WITH_WARNINGS
```

The latter should be used only when application semantics explicitly permit incomplete success.

---

## 205. Comic Partial Failure

For comics, progressive publication may preserve successfully translated bubbles.

Example:

```text
Bubble 1 completed
Bubble 2 failed
Bubble 3 completed
```

Presentation may show Bubbles 1 and 3 while Bubble 2 remains untranslated or displays a retry indicator.

Translation must preserve exact region alignment.

---

## 206. Novel Partial Failure

For novels, publishing non-contiguous translated paragraphs may disrupt reading.

The default novel profile may therefore prefer:

```text
retry before publishing incomplete paragraph groups
```

The policy belongs to Translation publication configuration.

---

# Part XXX — User-Facing Error Guidance

## 207. User-Facing Error Principles

User-facing messages should:

* explain what failed;
* avoid technical provider internals;
* suggest a meaningful next action;
* distinguish temporary failures from configuration problems;
* avoid blaming the user;
* not expose credentials or private content.

---

## 208. Temporary Failure Example

Developer error:

```text
TRANSLATION_PROVIDER_TIMEOUT
```

Possible UI meaning:

```text
The translation service took too long to respond.
```

Suggested action:

```text
Retry or use another translation provider.
```

---

## 209. Configuration Failure Example

Developer error:

```text
TRANSLATION_PROVIDER_CREDENTIALS_MISSING
```

Possible UI meaning:

```text
This translation provider has not been configured.
```

Suggested action:

```text
Configure the provider or choose another one.
```

---

## 210. Partial Failure Example

Developer error:

```text
TRANSLATION_PROVIDER_SEGMENT_MISSING
```

Possible UI meaning:

```text
Some text regions could not be translated.
```

Suggested action:

```text
Retry the untranslated regions.
```

---

## 211. Supersession Example

Developer outcome:

```text
TRANSLATION_SUPERSEDED
```

Normal UI behavior:

```text
Do not show an error.
Discard the outdated result and continue with newer work.
```

---

# Part XXXI — Logging and Observability

## 212. Required Error Metrics

Translation observability should track:

```text
error count by code
error count by category
error count by provider
retry count
fallback count
timeout count
alignment failure count
validation failure count
final job failure rate
partial completion rate
```

---

## 213. Log Fields

Recommended safe fields:

```text
errorId
errorCode
category
scope
severity

translationJobId
translationAttemptId
translationBatchId

preparedDocumentId
contentRevision

providerId
providerRequestId

attemptNumber
batchSegmentCount
elapsedTime
retryable
```

---

## 214. Prohibited Log Fields

Do not log by default:

```text
full source text
full translated text
raw prompts
provider credentials
authorization headers
complete provider response
private glossary data
complete character context
```

---

## 215. Error Sampling

High-volume transient errors may be sampled in logs.

Metrics must remain complete enough to detect:

* provider outages;
* rising timeout rates;
* repeated malformed responses;
* alignment regressions;
* retry storms.

Critical invariant and security errors must not be sampled away.

---

## 216. Alert Candidates

Possible alerts:

```text
High final job failure rate
Provider authentication failures
Persistent provider rate limiting
Alignment failure spike
Duplicate active variants
Invariant violations
Result persistence failure
Credential exposure attempt
Event publication backlog
```

Exact thresholds belong to operational documentation.

---

# Part XXXII — Event Integration

## 217. Attempt Failure Event

`TranslationAttemptFailed` should carry:

```text
errorId
code
category
retryable
affectedBatchIds
```

It should not carry the complete internal exception.

---

## 218. Batch Failure Event

`TranslationBatchFailed` should carry:

```text
errorId
code
category
retryable
preparedSegmentIds
```

---

## 219. Final Job Failure Event

`TranslationFailed` should include:

```text
final error summary
completed segment IDs
missing segment IDs
failed segment IDs
partial result reference
retryAllowed
```

---

## 220. Warning Events

Warnings normally travel with:

```text
TranslationBatchCompleted
TranslationPartialResultAvailable
TranslationCompletedWithWarnings
```

A separate event per warning should not be introduced unless a concrete consumer requires it.

---

# Part XXXIII — HTTP or Transport Mapping Guidance

## 221. Transport Independence

Translation domain error codes must remain independent from HTTP.

The same errors may be used over:

* in-process calls;
* message bus;
* HTTP;
* RPC;
* desktop application boundaries;
* browser extension communication.

---

## 222. Indicative HTTP Mapping

When Translation is exposed over HTTP, an adapter may use:

| Error category                   |                          Possible HTTP status |
| -------------------------------- | --------------------------------------------: |
| command validation               |                                         `400` |
| authentication configuration     | `401` or `503`, depending on caller ownership |
| authorization policy             |                                         `403` |
| entity not found                 |                                         `404` |
| state or idempotency conflict    |                                         `409` |
| request or content too large     |                                         `413` |
| rate limited                     |                                         `429` |
| provider temporarily unavailable |                                `502` or `503` |
| timeout                          |                                         `504` |
| internal failure                 |                                         `500` |

This table is guidance only.

Transport adapters own exact mappings.

---

# Part XXXIV — MVP Error Set

## 223. Required MVP Command Errors

```text
TRANSLATION_COMMAND_INVALID
TRANSLATION_IDEMPOTENCY_CONFLICT
TRANSLATION_SEGMENT_SELECTION_EMPTY
TRANSLATION_SEGMENT_SELECTION_INVALID
TRANSLATION_TARGET_LANGUAGE_REQUIRED
TRANSLATION_LANGUAGE_PAIR_UNSUPPORTED
TRANSLATION_PROVIDER_POLICY_UNSATISFIABLE
```

---

## 224. Required MVP Source Errors

```text
TRANSLATION_SOURCE_NOT_FOUND
TRANSLATION_SOURCE_REVISION_MISMATCH
TRANSLATION_SOURCE_NOT_PREPARED
TRANSLATION_SOURCE_EMPTY
TRANSLATION_SOURCE_SEGMENT_EMPTY
TRANSLATION_SOURCE_LANGUAGE_UNKNOWN
```

---

## 225. Required MVP Provider Errors

```text
TRANSLATION_PROVIDER_NOT_FOUND
TRANSLATION_PROVIDER_UNAVAILABLE
TRANSLATION_PROVIDER_CREDENTIALS_MISSING
TRANSLATION_PROVIDER_AUTHENTICATION_FAILED
TRANSLATION_PROVIDER_RATE_LIMITED
TRANSLATION_PROVIDER_QUOTA_EXCEEDED
TRANSLATION_PROVIDER_REQUEST_TOO_LARGE
TRANSLATION_PROVIDER_TIMEOUT
TRANSLATION_PROVIDER_CONNECTION_FAILED
TRANSLATION_PROVIDER_INTERNAL_ERROR
TRANSLATION_PROVIDER_RESPONSE_EMPTY
TRANSLATION_PROVIDER_RESPONSE_MALFORMED
```

The canonical timeout code should be unified during implementation.

Recommended final code:

```text
TRANSLATION_PROVIDER_TIMEOUT
```

rather than separate adapter-specific timeout names.

---

## 226. Required MVP Validation and Alignment Errors

```text
TRANSLATION_OUTPUT_VALIDATION_FAILED
TRANSLATION_OUTPUT_EMPTY
TRANSLATION_OUTPUT_TARGET_LANGUAGE_MISMATCH
TRANSLATION_OUTPUT_CONTROL_TEXT_LEAKED
TRANSLATION_PROVIDER_SEGMENT_MISSING
TRANSLATION_ALIGNMENT_FAILED
TRANSLATION_ALIGNMENT_DUPLICATE_TARGET
TRANSLATION_ALIGNMENT_REVISION_MISMATCH
```

---

## 227. Required MVP Lifecycle Errors

```text
TRANSLATION_RETRY_NOT_ALLOWED
TRANSLATION_RETRY_LIMIT_EXCEEDED
TRANSLATION_CANCELLED
TRANSLATION_ALREADY_TERMINAL
TRANSLATION_SUPERSEDED
TRANSLATION_STALE_RESULT_REJECTED
TRANSLATION_STATE_CONFLICT
```

---

## 228. Required MVP Warnings

```text
TRANSLATION_WARNING_MISSING_CONTEXT
TRANSLATION_WARNING_LOW_CONFIDENCE
TRANSLATION_WARNING_AMBIGUOUS_MEANING
TRANSLATION_WARNING_PRONOUN_AMBIGUITY
TRANSLATION_WARNING_PROVIDER_FALLBACK_USED
TRANSLATION_WARNING_PARTIAL_RESULT
TRANSLATION_WARNING_SOURCE_INCOMPLETE
TRANSLATION_WARNING_UNTRANSLATED_FRAGMENT
TRANSLATION_WARNING_OUTPUT_LONGER_THAN_SOURCE
TRANSLATION_WARNING_SOUND_EFFECT_PRESERVED
TRANSLATION_WARNING_CACHE_RESULT_REUSED
```

---

# Part XXXV — Error Classification Matrix

## 229. Common Error Matrix

| Error                                        | Scope         |             Retryable | Typical job consequence   |
| -------------------------------------------- | ------------- | --------------------: | ------------------------- |
| `TRANSLATION_COMMAND_INVALID`                | Command       |                    No | No job created            |
| `TRANSLATION_SOURCE_NOT_FOUND`               | Job           |           Conditional | `FAILED`                  |
| `TRANSLATION_SOURCE_REVISION_MISMATCH`       | Job           |                    No | `SUPERSEDED` or `FAILED`  |
| `TRANSLATION_CONTEXT_TOO_LARGE`              | Attempt       | Yes, after adjustment | `RETRY_SCHEDULED`         |
| `TRANSLATION_TERMINOLOGY_CONFLICT`           | Job/Segment   |           Conditional | `FAILED` or warning       |
| `TRANSLATION_PROVIDER_UNAVAILABLE`           | Attempt       |                   Yes | `RETRY_SCHEDULED`         |
| `TRANSLATION_PROVIDER_AUTHENTICATION_FAILED` | Provider      |                    No | `FAILED` or fallback      |
| `TRANSLATION_PROVIDER_RATE_LIMITED`          | Batch/Attempt |                   Yes | `RETRY_SCHEDULED`         |
| `TRANSLATION_PROVIDER_QUOTA_EXCEEDED`        | Provider      |                    No | `FAILED` or fallback      |
| `TRANSLATION_PROVIDER_REQUEST_TOO_LARGE`     | Batch         |     Yes, after resize | `RETRY_SCHEDULED`         |
| `TRANSLATION_BATCH_TIMEOUT`                  | Batch         |                   Yes | retry failed batch        |
| `TRANSLATION_PROVIDER_RESPONSE_MALFORMED`    | Batch         |                   Yes | batch `FAILED`            |
| `TRANSLATION_PROVIDER_SEGMENT_MISSING`       | Batch         |                   Yes | partial or retry          |
| `TRANSLATION_OUTPUT_EMPTY`                   | Segment       |                   Yes | partial or retry          |
| `TRANSLATION_ALIGNMENT_FAILED`               | Batch/Result  |           Conditional | retry or `FAILED`         |
| `TRANSLATION_RETRY_LIMIT_EXCEEDED`           | Job           |                    No | `FAILED`                  |
| `TRANSLATION_CANCELLED`                      | Job           |                    No | `CANCELLED`               |
| `TRANSLATION_SUPERSEDED`                     | Job           |                    No | `SUPERSEDED`              |
| `TRANSLATION_STALE_RESULT_REJECTED`          | Result        |                    No | `NON_AUTHORITATIVE`       |
| `TRANSLATION_CACHE_READ_FAILED`              | Cache         |                   Yes | continue without cache    |
| `TRANSLATION_RESULT_PERSISTENCE_FAILED`      | Result        |                   Yes | no completion publication |
| `TRANSLATION_INVARIANT_VIOLATED`             | Module        |    No automatic retry | reconciliation required   |

---

# Part XXXVI — Core Error Invariants

## 230. Invariant 1 — Provider neutrality

Public error codes never depend on provider-native exception classes.

## 231. Invariant 2 — Stable codes

Published error-code semantics do not change.

## 232. Invariant 3 — Explicit retryability

Every execution error defines retry behavior.

## 233. Invariant 4 — Retry does not erase history

A failed attempt or batch remains failed after retry.

## 234. Invariant 5 — Partial failures remain explicit

Missing and failed segments are never silently omitted.

## 235. Invariant 6 — Warnings do not masquerade as failures

Usable degraded output uses warnings or a warning completion state.

## 236. Invariant 7 — Cancellation is not failure

Intentional cancellation maps to cancellation lifecycle semantics.

## 237. Invariant 8 — Supersession is not failure

Outdated work is rejected without presenting a provider failure.

## 238. Invariant 9 — Invalid provider output never becomes authoritative

Parsing, validation and alignment happen before batch completion.

## 239. Invariant 10 — Credentials never enter public errors

No public error contains authentication secrets.

## 240. Invariant 11 — Content minimization

Source and translated text are omitted from errors and logs by default.

## 241. Invariant 12 — Cache failure is normally non-fatal

Translation continues without cache when policy permits.

## 242. Invariant 13 — Terminal jobs do not automatically retry

Manual recovery after final failure normally creates a new derived job.

## 243. Invariant 14 — State transition consequences are consistent

Error handling must obey `modules/translation/STATES.md`.

## 244. Invariant 15 — Result references remain trustworthy

A completion event is never published for an unavailable result or variant.

---

# Part XXXVII — Open Decisions

## 245. Canonical Timeout Codes

This document currently distinguishes:

```text
TRANSLATION_QUEUE_TIMEOUT
TRANSLATION_ATTEMPT_TIMEOUT
TRANSLATION_BATCH_TIMEOUT
TRANSLATION_JOB_TIMEOUT
```

Provider-specific execution timeout should use:

```text
TRANSLATION_PROVIDER_TIMEOUT
```

The implementation must avoid overlapping meanings between:

```text
PROVIDER_TIMEOUT
BATCH_TIMEOUT
```

Recommended distinction:

```text
PROVIDER_TIMEOUT
    = provider or network request exceeded its deadline

BATCH_TIMEOUT
    = complete batch lifecycle exceeded its Translation deadline
```

---

## 246. Incomplete Final Result

The project must decide whether an incomplete result can produce:

```text
COMPLETED_WITH_WARNINGS
```

Recommended policy:

* comic progressive mode may accept missing optional or non-critical segments;
* novel atomic mode should normally end in `FAILED` when required paragraphs remain missing;
* selected required segments must be explicitly classified.

A future contract may add:

```text
segmentRequirement:
    REQUIRED
    OPTIONAL
    CONTEXT_ONLY
```

---

## 247. User-Visible Provider Names

The project must decide whether user-facing errors expose normalized provider names.

Recommended behavior:

* show provider name when the user explicitly selected it;
* avoid provider details under automatic mode unless action is required;
* never expose model IDs unnecessarily.

---

## 248. Warning Persistence

The project must define whether all informational warnings are persisted.

Recommended approach:

```text
DEGRADED warnings
    → persist

NOTICE warnings
    → persist with result when relevant

INFO warnings
    → metrics or optional metadata
```

---

## 249. Error Localization

`userMessageKey` is defined, but localization ownership remains open.

Recommended ownership:

```text
Translation
    → owns machine-readable code and localization key

Presentation
    → resolves locale-specific user wording
```

---

## 250. Error Cause Depth

Recommended public cause depth:

```text
maximum 1 normalized cause
```

Full nested exception chains remain internal.

This prevents unstable implementation details from leaking into contracts.

---

## 250. Cross-Architecture Error Invariants

The following ownership and authority rules apply to every Translation error:

1. Translation policy errors must not be confused with Runtime execution failures.
2. Runtime may enforce retry timing, timeout, cancellation and execution budgets without owning Translation semantic intent.
3. A normalized Translation error may reference an original Runtime or Provider Management failure without transferring ownership.
4. `TranslationIntentId` and `translationRevision` should be included whenever the failure can affect result authority or visible output.
5. Reading Session owns whether a compatible Translation result is accepted for the current content revision.
6. Publication failure does not imply result persistence failure, and persistence failure must prevent completion events.
7. Active-variant conflicts are scoped by reading session, prepared content revision, target language and Translation intent.
8. Duplicate active-attempt detection is evaluated within the owning Translation batch unless an explicit broader execution policy exists.

---

# Part XXXVIII — Related Documents

```text
modules/translation/MODULE.md
modules/translation/CONTRACT.md
modules/translation/EVENTS.md
modules/translation/STATES.md
modules/translation/README.md
```

Architecture references:

```text
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/WORK_QUEUE.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md
```

Upstream references:

```text
modules/text-processing/MODULE.md
modules/text-processing/CONTRACT.md
modules/text-processing/EVENTS.md
modules/text-processing/STATES.md
modules/text-processing/ERRORS.md
```

Future integration references:

```text
modules/provider-management/ERRORS.md
modules/knowledge/ERRORS.md
modules/reading-session/ERRORS.md
modules/presentation/ERRORS.md
```

---

# 251. Summary

Translation errors are normalized around:

```text
code
category
scope
severity
retryability
recovery actions
affected entity identities
```

The primary error flow is:

```text
Provider or internal failure
        ↓
Normalize error
        ↓
Apply error to Batch or Attempt
        ↓
Evaluate retry and fallback policy
        ↓
Retry, complete partially or fail job
```

Retryable execution path:

```text
Batch FAILED
    ↓
Attempt FAILED
    ↓
Job RETRY_SCHEDULED
    ↓
New Attempt
```

Final failure path:

```text
Retry budget exhausted
    ↓
Job FAILED
```

Partial path:

```text
Some segments completed
Some segments failed
    ↓
Result PARTIAL
    ↓
Retry or final policy decision
```

Lifecycle controls remain separate:

```text
Cancellation
    → CANCELLED

Newer work
    → SUPERSEDED

Administrative rejection
    → INVALIDATED
```

The most important rules are:

* provider errors are normalized before leaving adapters;
* every execution error has explicit retry semantics;
* retries create new attempts and batches;
* partial results identify all missing and failed segments;
* cancellation and supersession are not reported as provider failures;
* invalid provider output never becomes authoritative;
* public errors never expose credentials, raw prompts or full provider responses;
* cache failures normally degrade rather than fail translation;
* final state consequences must follow `STATES.md`.
