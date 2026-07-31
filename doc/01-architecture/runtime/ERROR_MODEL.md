# Runtime Error Model

> Project: CRAI  
> Version: 0.1  
> Status: Architecture Draft

---

## 1. Purpose

This document defines how CRAI represents, classifies, propagates, observes, recovers from, and presents runtime errors.

CRAI processes continuously changing screen content through multiple asynchronous stages:

```text
Capture
    ↓
Observation
    ↓
OCR
    ↓
Layout
    ↓
Translation
    ↓
Presentation
    ↓
UI Commit
```

Errors may occur at any stage.

Without a consistent error model, the runtime may:

- treat cancellation as failure
- retry permanent errors
- show obsolete errors to the user
- hide fatal resource failures
- lose provider diagnostics
- leak implementation-specific exceptions across module boundaries
- produce inconsistent UI behavior
- repeatedly execute unrecoverable work

The error model provides a shared language for runtime failure handling.

---

## 2. Scope

This document covers:

- runtime outcome categories
- error taxonomy
- error structure
- error ownership
- stage errors
- provider errors
- resource errors
- configuration errors
- validation errors
- cancellation outcomes
- stale-result outcomes
- transient and permanent failures
- recoverability
- propagation
- aggregation
- user-visible error mapping
- fatal runtime errors
- observability
- MVP error policy

This document does not define:

- exact retry counts or backoff schedules
- provider-specific response-code tables
- user-interface visual design
- logging storage implementation
- crash-reporting provider
- programming-language exception syntax

Those concerns belong to related documents or implementation details.

---

## 3. Error Model Goals

The runtime error model must:

- distinguish failure from cancellation
- distinguish failure from stale-result rejection
- classify recoverability
- preserve useful diagnostic context
- prevent provider details from leaking across architecture boundaries
- prevent obsolete errors from disturbing current work
- support deterministic retry decisions
- support user-friendly messages
- keep control flow explicit
- preserve privacy
- remain testable

---

## 4. Core Philosophy

CRAI follows this rule:

> Every runtime operation ends with an explicit outcome, not merely an exception or missing value.

Possible terminal outcomes are:

```text
SUCCEEDED
FAILED
CANCELED
STALE
ABANDONED
```

These outcomes are not interchangeable.

---

## 5. Runtime Outcome Model

A WorkItem conceptually completes with:

```text
WorkOutcome
├── Status
├── WorkItemId
├── SessionId
├── RevisionId
├── AttemptId
├── Stage
├── StartedAt
├── CompletedAt
├── OutputArtifactId
├── Error
├── CancellationReason
└── Diagnostics
```

Only relevant fields are populated for each status.

---

## 6. Outcome Statuses

### 6.1 SUCCEEDED

The work completed successfully and produced a valid output.

Example:

```text
OCR completed
    ↓
OCR Artifact validated
    ↓
Artifact published
```

A successful stage result may still later be rejected as stale before commit.

---

### 6.2 FAILED

The work could not produce a valid output because of an error.

Examples:

- OCR provider unavailable
- malformed provider response
- unsupported image format
- resource allocation failure
- invalid stage input

A failed WorkItem may or may not be retryable.

---

### 6.3 CANCELED

The work stopped because cancellation was requested and acknowledged.

Examples:

- revision became obsolete
- session stopped
- application shutting down
- user changed provider
- memory pressure canceled background work

Cancellation is expected control flow, not necessarily an error.

---

### 6.4 STALE

The work completed or returned a result, but the result no longer has logical authority.

Examples:

- result belongs to an older revision
- attempt was superseded by a retry
- session was closed before completion
- provider configuration changed
- presentation commit arrived too late

A stale result must not update runtime state or UI.

---

### 6.5 ABANDONED

The runtime stopped waiting for the work, but cannot confirm that physical execution ended cleanly.

Examples:

- provider does not support cancellation
- child process failed to acknowledge shutdown
- native call remains blocked beyond the grace period
- application shutdown timeout elapsed

Abandoned work is logically invalid and must never regain commit authority.

---

## 7. Failure, Cancellation, and Staleness

These concepts must remain separate.

### Failure

```text
The operation could not produce a valid result.
```

### Cancellation

```text
The operation was intentionally asked to stop.
```

### Staleness

```text
The operation may have produced a technically valid result,
but the result no longer belongs to the active logical state.
```

Example:

```text
Revision 20 translation succeeds
    ↓
Revision 21 already became current
    ↓
Revision 20 result = STALE
```

The provider request did not fail.

The runtime correctly rejects the obsolete result.

---

## 8. Error Taxonomy

Runtime errors are classified into major categories.

```text
Runtime Error
├── Input Error
├── Validation Error
├── Capture Error
├── Observation Error
├── OCR Error
├── Layout Error
├── Translation Error
├── Presentation Error
├── Provider Error
├── Resource Error
├── Cache Error
├── State Error
├── Configuration Error
├── Security Error
├── Persistence Error
├── Integration Error
└── Internal Error
```

---

## 9. Error Structure

A normalized runtime error should conceptually contain:

```text
RuntimeError
├── ErrorCode
├── Category
├── Severity
├── RetryClass
├── Scope
├── Stage
├── MessageKey
├── TechnicalMessage
├── UserMessageKey
├── ProviderCode
├── Cause
├── Context
├── OccurredAt
└── Correlation
```

The exact implementation structure is language-specific.

---

## 10. Error Code

Every normalized error should have a stable machine-readable code.

Examples:

```text
CAPTURE_PERMISSION_DENIED
CAPTURE_SOURCE_UNAVAILABLE
OCR_PROVIDER_TIMEOUT
OCR_RESPONSE_INVALID
TRANSLATION_RATE_LIMITED
TRANSLATION_CONTEXT_TOO_LARGE
ARTIFACT_NOT_FOUND
ARTIFACT_DISPOSED
REVISION_NOT_CURRENT
MEMORY_BUDGET_EXCEEDED
INVALID_RUNTIME_STATE
```

Error codes should:

- remain stable across user-interface wording changes
- avoid embedding variable details
- support metrics aggregation
- support retry policy
- support automated tests

---

## 11. Error Category

The category identifies the architectural area that owns the error.

Examples:

```text
INPUT
CAPTURE
OCR
TRANSLATION
PROVIDER
RESOURCE
STATE
CONFIGURATION
INTERNAL
```

Category is broader than `ErrorCode`.

---

## 12. Error Severity

Suggested severity levels:

```text
INFO
WARNING
ERROR
CRITICAL
FATAL
```

### INFO

Expected non-problematic control outcome.

Example:

```text
Obsolete result rejected
```

### WARNING

Recoverable degradation or temporary issue.

Example:

```text
Remote provider slow
```

### ERROR

Current operation failed and requires recovery, retry, or user action.

### CRITICAL

Major runtime capability is unavailable.

Example:

```text
Capture subsystem cannot initialize
```

### FATAL

The application cannot safely continue.

Example:

```text
Core runtime state corrupted
```

Severity does not automatically determine retry behavior.

---

## 13. Retry Classification

Errors should include one retry class.

```text
NOT_RETRYABLE
RETRYABLE_IMMEDIATE
RETRYABLE_DELAYED
RETRYABLE_AFTER_CHANGE
RETRYABLE_AFTER_USER_ACTION
RETRYABLE_WITH_DIFFERENT_PROVIDER
UNKNOWN
```

Detailed retry execution belongs in `RETRY_POLICY.md`.

---

## 14. Error Scope

An error must declare its impact scope.

Possible scopes:

```text
WORK_ITEM
STAGE
REVISION
SESSION
PROVIDER
APPLICATION
```

### WorkItem Scope

Only one attempt failed.

### Stage Scope

The current stage cannot proceed for the revision.

### Revision Scope

The current revision cannot produce a complete presentation.

### Session Scope

The reading session cannot continue.

### Provider Scope

A provider should be degraded, disabled, or reinitialized.

### Application Scope

The runtime cannot safely continue.

---

## 15. Error Ownership

The component that detects an error is not always the component that decides recovery.

Example:

```text
Provider Adapter detects timeout
    ↓
Normalizes ProviderError
    ↓
Submits WorkFailed
    ↓
Runtime Control validates relevance
    ↓
Retry Policy decides next action
```

Responsibilities are separated as follows:

| Component | Responsibility |
|---|---|
| Worker | Detect local execution failure |
| Provider Adapter | Normalize provider-specific failure |
| Runtime Control | Validate scope and relevance |
| Retry Policy | Decide retry eligibility |
| Scheduler | Admit replacement work |
| UI Presenter | Map current relevant error to UI |
| Observability | Record diagnostics and metrics |

---

## 16. Exception Boundary

Implementation-specific exceptions must not cross architecture boundaries unnormalized.

Incorrect:

```text
Provider SDK Exception
    ↓
UI
```

Correct:

```text
Provider SDK Exception
    ↓
Provider Adapter
    ↓
Normalized RuntimeError
    ↓
Runtime Outcome
```

Raw exceptions may be retained as internal causes for diagnostics, but should not become domain control flow.

---

## 17. Error Context

Errors should include only the context needed for diagnosis and recovery.

Possible fields:

```text
SessionId
RevisionId
WorkItemId
AttemptId
Stage
ProviderId
ModelId
Operation
Timeout
InputSize
RegionCount
MemoryPressureLevel
```

Sensitive content must not be included by default.

Avoid including:

- full screenshots
- full OCR text
- full translation input
- access tokens
- provider credentials
- complete request bodies
- personal reading content

---

## 18. Error Cause Chain

A normalized error may retain an internal cause chain.

Example:

```text
TRANSLATION_REQUEST_FAILED
    caused by
HTTP_REQUEST_FAILED
    caused by
SOCKET_TIMEOUT
```

The cause chain helps diagnostics.

Recovery decisions should normally use the normalized top-level error code and retry classification.

---

## 19. Expected and Unexpected Errors

### Expected Errors

Known operational outcomes that the runtime is designed to handle.

Examples:

- provider timeout
- permission denied
- unsupported capture source
- rate limiting
- cancellation
- stale result
- invalid user configuration

### Unexpected Errors

Violations or conditions not anticipated by normal runtime behavior.

Examples:

- impossible state transition
- null artifact after successful publication
- reference count corruption
- unhandled native failure
- duplicate ownership transfer
- internal invariant violation

Unexpected errors require stronger diagnostics and may escalate scope.

---

## 20. Input Errors

Input errors occur when required external input is missing, invalid, or unsupported.

Examples:

```text
CAPTURE_REGION_INVALID
SOURCE_LANGUAGE_UNSUPPORTED
TARGET_LANGUAGE_UNSUPPORTED
EMPTY_TRANSLATION_INPUT
IMAGE_FORMAT_UNSUPPORTED
INPUT_TOO_LARGE
```

Input errors are usually:

- non-retryable without changing input
- revision-scoped or session-scoped
- suitable for direct user explanation

---

## 21. Validation Errors

Validation errors occur when data fails internal compatibility or integrity checks.

Examples:

```text
ARTIFACT_KEY_MISMATCH
ARTIFACT_VERSION_UNSUPPORTED
OCR_ARTIFACT_INVALID
LAYOUT_GEOMETRY_INVALID
TRANSLATION_UNIT_INVALID
PRESENTATION_MODEL_INVALID
```

Validation failure must prevent publication or commit.

Invalid artifacts must not be cached.

---

## 22. Capture Errors

Capture errors may include:

```text
CAPTURE_PERMISSION_DENIED
CAPTURE_SOURCE_NOT_FOUND
CAPTURE_SOURCE_CLOSED
CAPTURE_API_UNAVAILABLE
CAPTURE_FRAME_FAILED
CAPTURE_SURFACE_INVALID
CAPTURE_REGION_OUT_OF_BOUNDS
CAPTURE_DEVICE_LOST
```

Capture errors may affect:

- one frame
- one source
- the session
- the application capability

---

## 23. Capture Error Recovery

Possible recovery actions:

| Error | Typical action |
|---|---|
| Temporary frame failure | Skip frame and continue |
| Source temporarily unavailable | Delayed retry |
| Region invalid | Ask user to reselect region |
| Permission denied | Request user action |
| Source closed | Stop or reconfigure session |
| Capture API unavailable | Disable capture capability |

Repeated frame failure must not generate unbounded logs or retries.

---

## 24. Observation Errors

Observation errors may include:

```text
FRAME_COMPARISON_FAILED
FINGERPRINT_GENERATION_FAILED
STABILITY_ANALYSIS_FAILED
OBSERVATION_INPUT_DISPOSED
OBSERVATION_OVERLOADED
```

Observation should prefer skipping one invalid frame rather than terminating the session where safe.

Repeated observation failures may escalate to session scope.

---

## 25. OCR Errors

OCR errors may include:

```text
OCR_INPUT_INVALID
OCR_PROVIDER_UNAVAILABLE
OCR_PROVIDER_TIMEOUT
OCR_RATE_LIMITED
OCR_AUTHENTICATION_FAILED
OCR_RESPONSE_INVALID
OCR_MODEL_LOAD_FAILED
OCR_MODEL_INFERENCE_FAILED
OCR_TEXT_NOT_DETECTED
OCR_OUTPUT_TOO_LARGE
OCR_UNSUPPORTED_SCRIPT
```

`OCR_TEXT_NOT_DETECTED` may be a valid empty outcome rather than a technical failure, depending on the stage contract.

---

## 26. Empty OCR Result

The runtime must distinguish:

```text
No text exists
```

from:

```text
OCR failed to detect existing text
```

The pipeline may not always know the difference with certainty.

Suggested outcomes:

```text
OCR_SUCCESS_EMPTY
OCR_LOW_CONFIDENCE
OCR_FAILED
```

A successful empty OCR result may produce an empty presentation without retry.

A low-confidence result may trigger:

- warning
- alternative OCR configuration
- user-visible quality indicator
- optional retry

---

## 27. Layout Errors

Layout errors may include:

```text
LAYOUT_INPUT_INVALID
LAYOUT_REGION_GRAPH_INVALID
LAYOUT_READING_ORDER_FAILED
LAYOUT_GEOMETRY_OUT_OF_RANGE
LAYOUT_TOO_COMPLEX
LAYOUT_UNSUPPORTED_ORIENTATION
```

Layout may support degraded output.

Example:

```text
Reading-order calculation fails
    ↓
Fallback to OCR provider order
```

Fallback must be explicit and observable.

---

## 28. Translation Errors

Translation errors may include:

```text
TRANSLATION_INPUT_INVALID
TRANSLATION_PROVIDER_UNAVAILABLE
TRANSLATION_PROVIDER_TIMEOUT
TRANSLATION_RATE_LIMITED
TRANSLATION_AUTHENTICATION_FAILED
TRANSLATION_QUOTA_EXCEEDED
TRANSLATION_CONTEXT_TOO_LARGE
TRANSLATION_RESPONSE_INVALID
TRANSLATION_OUTPUT_EMPTY
TRANSLATION_LANGUAGE_UNSUPPORTED
TRANSLATION_CONTENT_REJECTED
TRANSLATION_MODEL_UNAVAILABLE
```

Some translation failures permit fallback to:

- a smaller context
- smaller batch
- different provider
- untranslated source display
- partial result

---

## 29. Presentation Errors

Presentation errors may include:

```text
PRESENTATION_INPUT_INVALID
PRESENTATION_LAYOUT_FAILED
FONT_RESOLUTION_FAILED
TEXT_MEASUREMENT_FAILED
PRESENTATION_MODEL_INVALID
UI_DISPATCH_FAILED
UI_TARGET_DISPOSED
UI_COMMIT_REJECTED
```

`UI_COMMIT_REJECTED` may represent a stale or closed-session outcome rather than a failure.

---

## 30. Provider Error Model

Provider-specific errors must be mapped into normalized categories.

Conceptually:

```text
ProviderResponse
    ↓
Provider Adapter Mapping
    ↓
Normalized ProviderError
```

Normalized provider types may include:

```text
PROVIDER_TIMEOUT
PROVIDER_UNAVAILABLE
PROVIDER_RATE_LIMITED
PROVIDER_AUTH_FAILED
PROVIDER_QUOTA_EXCEEDED
PROVIDER_REQUEST_INVALID
PROVIDER_RESPONSE_INVALID
PROVIDER_CONTENT_REJECTED
PROVIDER_INTERNAL_ERROR
PROVIDER_NETWORK_ERROR
PROVIDER_CANCELED
```

---

## 31. Provider Code Preservation

The normalized error may preserve:

```text
ProviderCode
ProviderHttpStatus
ProviderRequestId
```

for diagnostics.

Recovery logic must not depend directly on unstable provider message strings.

---

## 32. Provider Authentication Errors

Authentication failures are generally not suitable for automatic repeated retry.

Examples:

```text
Invalid API key
Expired credential
Revoked token
Incorrect project configuration
```

Typical classification:

```text
RETRYABLE_AFTER_USER_ACTION
```

Repeated automatic retries would waste requests and obscure the actual problem.

---

## 33. Provider Rate Limiting

Rate limiting should be classified separately from unavailability.

Possible recovery:

```text
Respect retry-after
    ↓
Apply provider backoff
    ↓
Reduce concurrency
    ↓
Retry only if revision remains current
```

Rate-limit handling must not block the runtime control context.

---

## 34. Provider Quota Exhaustion

Quota exhaustion differs from temporary rate limiting.

Possible outcomes:

- provider disabled for the remaining quota period
- switch provider
- use local mode
- notify user
- stop the affected stage

Quota errors normally require configuration or billing changes.

---

## 35. Provider Timeout

Timeout means the operation exceeded CRAI's configured execution limit.

It does not always mean the provider failed internally.

The runtime should distinguish:

```text
Connection timeout
Request timeout
Response-read timeout
Overall operation timeout
```

The normalized top-level code may remain:

```text
PROVIDER_TIMEOUT
```

while diagnostics preserve the timeout phase.

---

## 36. Resource Errors

Resource errors may include:

```text
ARTIFACT_NOT_FOUND
ARTIFACT_NOT_PUBLISHED
ARTIFACT_ALREADY_DISPOSED
ARTIFACT_LEASE_FAILED
ARTIFACT_TYPE_MISMATCH
RESOURCE_OWNERSHIP_INVALID
RESOURCE_DISPOSAL_FAILED
MEMORY_BUDGET_EXCEEDED
GPU_MEMORY_EXHAUSTED
NATIVE_RESOURCE_UNAVAILABLE
BUFFER_ALLOCATION_FAILED
```

Some resource errors indicate normal races.

Example:

```text
Artifact unavailable because revision was canceled
```

This should normally become `CANCELED` or `STALE`, not an internal failure.

---

## 37. Resource Error Normalization

Before escalating a resource error, the runtime should check:

```text
Was the session closed?
Was the revision canceled?
Was the artifact evicted legitimately?
Was the lease denied because disposal began?
```

Expected lifecycle races should not be misclassified as corruption.

---

## 38. Cache Errors

Cache errors may include:

```text
CACHE_LOOKUP_FAILED
CACHE_ENTRY_CORRUPT
CACHE_KEY_INVALID
CACHE_INSERT_FAILED
CACHE_EVICTION_FAILED
CACHE_STORAGE_UNAVAILABLE
```

Cache failure must not normally break pipeline correctness.

Preferred behavior:

```text
Cache error
    ↓
Record diagnostics
    ↓
Treat as cache miss
    ↓
Continue pipeline
```

Exceptions include resource failures that threaten application stability.

---

## 39. State Errors

State errors occur when an operation violates the runtime state machine.

Examples:

```text
INVALID_STATE_TRANSITION
SESSION_NOT_ACTIVE
REVISION_ALREADY_CLOSED
WORK_ALREADY_COMPLETED
DUPLICATE_ARTIFACT_PUBLICATION
COMMIT_WITHOUT_AUTHORITY
ATTEMPT_ALREADY_SUPERSEDED
```

Some state errors may be benign duplicate commands.

Others represent runtime invariant violations.

---

## 40. Idempotent Duplicate Handling

Duplicate completion or cancellation commands may occur.

Example:

```text
Provider callback completes
Timeout handler also fires
```

The runtime should handle terminal state transitions idempotently where possible.

Preferred outcome:

```text
First terminal outcome accepted
Subsequent duplicate ignored and recorded
```

Do not treat every duplicate completion as fatal.

---

## 41. Configuration Errors

Configuration errors may include:

```text
PROVIDER_NOT_CONFIGURED
PROVIDER_CREDENTIAL_MISSING
INVALID_LANGUAGE_PAIR
INVALID_TIMEOUT
INVALID_CONCURRENCY_LIMIT
INVALID_MEMORY_BUDGET
INVALID_CAPTURE_RATE
UNSUPPORTED_RUNTIME_MODE
```

Configuration errors are generally:

```text
RETRYABLE_AFTER_CHANGE
```

or:

```text
RETRYABLE_AFTER_USER_ACTION
```

---

## 42. Security Errors

Security errors may include:

```text
CREDENTIAL_ACCESS_DENIED
CREDENTIAL_DECRYPTION_FAILED
UNTRUSTED_PROVIDER_CONFIGURATION
UNSAFE_FILE_PATH
INVALID_PLUGIN_SIGNATURE
PERMISSION_REQUIRED
```

Security-related errors must:

- avoid leaking secrets
- avoid automatic unsafe fallback
- use elevated diagnostics severity
- produce clear user action where appropriate

---

## 43. Persistence Errors

Future persistent storage may produce:

```text
STORAGE_UNAVAILABLE
STORAGE_READ_FAILED
STORAGE_WRITE_FAILED
STORAGE_CORRUPT
STORAGE_QUOTA_EXCEEDED
STORAGE_VERSION_UNSUPPORTED
```

For the MVP memory-only runtime, most persistence errors are optional.

Persistent cache failure should usually degrade to in-memory processing.

---

## 44. Internal Errors

Internal errors represent unexpected runtime defects.

Examples:

```text
INVARIANT_VIOLATION
NULL_REQUIRED_ARTIFACT
OWNERSHIP_ACCOUNTING_CORRUPT
UNEXPECTED_WORK_STATUS
PIPELINE_GRAPH_INVALID
UNHANDLED_EXCEPTION
```

Internal errors should include:

- high diagnostic severity
- correlation identifiers
- safe state capture
- no raw private content by default

Depending on scope, the runtime may:

- fail one WorkItem
- terminate one session
- restart a subsystem
- request application restart
- terminate safely

---

## 45. Fatal Errors

A fatal error means CRAI cannot safely preserve runtime invariants.

Possible examples:

- core runtime control loop terminated unexpectedly
- Artifact Store ownership state corrupted
- unrecoverable state-machine corruption
- repeated native crashes in the main process
- inability to release critical system resources
- application-wide security failure

Fatal handling should prioritize:

```text
Stop New Work
    ↓
Prevent Invalid Commit
    ↓
Cancel Sessions
    ↓
Attempt Bounded Cleanup
    ↓
Persist Safe Diagnostics
    ↓
Exit or Restart Safely
```

---

## 46. Recoverability

Each error must have a recoverability classification.

```text
RECOVERABLE
RECOVERABLE_WITH_DEGRADATION
RECOVERABLE_AFTER_USER_ACTION
NON_RECOVERABLE_FOR_REVISION
NON_RECOVERABLE_FOR_SESSION
NON_RECOVERABLE_FOR_APPLICATION
```

Recoverability and retryability are related but not identical.

Example:

```text
OCR provider unavailable
```

may be recoverable through a different provider without retrying the same provider.

---

## 47. Transient Errors

Transient errors may resolve without permanent configuration changes.

Examples:

- temporary network loss
- provider timeout
- provider overload
- rate limiting
- temporary capture failure
- temporary model startup failure
- resource pressure

Transient does not imply unlimited retry.

The revision may become obsolete before retry is useful.

---

## 48. Permanent Errors

Permanent errors are unlikely to succeed again with identical input and configuration.

Examples:

- unsupported language
- invalid credential
- unsupported image format
- provider rejects required operation
- invalid configuration
- input violates provider limit with no transformation

Permanent errors should not be retried unchanged.

---

## 49. Deterministic Errors

A deterministic error is expected to repeat for the same:

```text
Input
Configuration
Provider
Version
```

Examples:

- invalid artifact format
- unsupported language pair
- translation context always exceeds a fixed limit
- provider request schema invalid

Deterministic failures may be cached as negative knowledge in future versions, but this is not required for the MVP.

---

## 50. Error Propagation

Errors should propagate through structured outcomes.

Preferred flow:

```text
Worker detects failure
    ↓
Create normalized RuntimeError
    ↓
Submit WorkFailed command
    ↓
Runtime validates WorkItem state
    ↓
Runtime records terminal outcome
    ↓
Recovery decision
    ↓
Relevant event published
```

Workers must not decide all recovery actions themselves.

---

## 51. Error Relevance Validation

Before an error affects state or UI, the runtime must validate:

```text
Session still active?
Revision still current or relevant?
Attempt still authoritative?
Stage still expected?
Error not superseded?
```

An error from an obsolete revision should normally be recorded as stale and not shown prominently.

---

## 52. Stale Error Suppression

Example:

```text
Revision 30 translation times out
    ↓
Revision 31 already committed
```

The timeout remains useful for diagnostics.

However, the UI should not display:

```text
Translation failed
```

for Revision 30.

Stale errors must not replace the state of current successful work.

---

## 53. Cancellation Error Suppression

Provider SDKs may represent cancellation as an exception.

The adapter must normalize expected cancellation into:

```text
CANCELED
```

not:

```text
FAILED
```

Unexpected cancellation failure may still produce a resource or provider error.

---

## 54. Abandoned Work Handling

When work is abandoned:

- logical authority is revoked
- new downstream work is forbidden
- late completion becomes stale
- resources are tracked as draining
- diagnostics record abandonment reason
- capacity may remain consumed until physical completion

Repeated abandonment may degrade or disable a provider.

---

## 55. Error Aggregation

One revision may produce multiple failures.

Example:

```text
Primary translation provider timeout
    ↓
Fallback provider unavailable
```

The runtime may retain an internal error aggregate:

```text
RevisionFailure
├── PrimaryError
├── FallbackErrors
├── FinalDisposition
└── UserVisibleCause
```

The user should receive one clear final message rather than every internal attempt.

---

## 56. Root Error Selection

When several errors occur, select the most useful root error based on:

1. final blocking cause
2. user actionability
3. architectural scope
4. causal relationship
5. severity

Example:

```text
Provider timeout
    ↓
Fallback blocked because no credential
```

The user-facing error may be:

```text
No translation provider is currently available.
```

Diagnostics should preserve both underlying errors.

---

## 57. User-Visible Error Model

The UI should receive a safe presentation object.

Conceptually:

```text
UserVisibleError
├── TitleKey
├── MessageKey
├── Severity
├── SuggestedAction
├── RetryAllowed
├── OpenSettingsAllowed
├── PreservePreviousPresentation
└── TechnicalReference
```

The UI should not receive raw provider exception text.

---

## 58. User Error Levels

Suggested user-facing levels:

```text
INLINE_NOTICE
NON_BLOCKING_WARNING
REVISION_ERROR
SESSION_BLOCKING_ERROR
APPLICATION_BLOCKING_ERROR
```

### Inline Notice

Small issue that does not block reading.

Example:

```text
Some text could not be recognized.
```

### Non-Blocking Warning

Degraded operation with continued service.

Example:

```text
Using fallback translator.
```

### Revision Error

Current content could not be translated.

### Session-Blocking Error

The session cannot continue without action.

### Application-Blocking Error

The application cannot safely operate.

---

## 59. Preserve Previous Presentation

When the current revision fails, the UI may retain the previous valid presentation.

Example:

```text
Revision 40 displayed
    ↓
Revision 41 translation fails
```

Possible UI behavior:

```text
Keep Revision 40 visible
Show that it belongs to previous content
Show non-blocking error for Revision 41
```

The UI must not misrepresent old translation as current.

---

## 60. User Action Mapping

Errors may suggest actions such as:

```text
RETRY
RESELECT_CAPTURE_REGION
CHECK_NETWORK
OPEN_PROVIDER_SETTINGS
CHANGE_PROVIDER
REDUCE_CAPTURE_REGION
RELOAD_MODEL
RESTART_SESSION
RESTART_APPLICATION
REPORT_PROBLEM
NONE
```

Suggested actions must be derived from normalized error codes.

---

## 61. Retry UI

A retry control should only be shown when:

- retry is allowed
- the revision remains relevant
- required input still exists
- no retry is already active
- repeated retry is not blocked by policy

The UI must not provide infinite immediate retry loops for permanent errors.

---

## 62. Partial Success

A stage may produce usable but incomplete output.

Examples:

- OCR recognizes only some regions
- translation fails for one bubble
- layout falls back to simple ordering
- font rendering uses fallback font

Possible outcome:

```text
SUCCEEDED_WITH_WARNINGS
```

or:

```text
SUCCEEDED
    +
Warnings
```

For simplicity, the MVP should use:

```text
SUCCEEDED
```

with a bounded warning collection attached to the output artifact or completion metadata.

---

## 63. Warning Model

Warnings do not invalidate the output.

Examples:

```text
OCR_LOW_CONFIDENCE
LAYOUT_FALLBACK_USED
TRANSLATION_PARTIAL
FONT_FALLBACK_USED
CACHE_WRITE_SKIPPED
```

Warnings should be:

- observable
- bounded
- optionally user-visible
- excluded from automatic retry unless policy explicitly requires it

---

## 64. Error Events

Possible runtime events include:

```text
work.failed
work.canceled
work.stale
work.abandoned
revision.failed
session.degraded
session.failed
provider.degraded
provider.recovered
resource.cleanup.failed
runtime.fatal
```

Events must carry lightweight metadata.

---

## 65. Error Event Payload

Suggested event metadata:

```text
ErrorEvent
├── ErrorCode
├── Category
├── Severity
├── Scope
├── Stage
├── SessionId
├── RevisionId
├── WorkItemId
├── AttemptId
├── ProviderId
├── RetryClass
└── Timestamp
```

Raw input or output content must not be embedded.

---

## 66. Logging Policy

Errors should be logged according to severity and expectedness.

### Debug or Trace

- stale result rejected
- expected cancellation
- duplicate terminal command ignored

### Info

- provider fallback activated
- temporary degradation entered

### Warning

- retryable provider failure
- cache operation failed
- cleanup took too long

### Error

- revision failed
- provider initialization failed
- session capability unavailable

### Critical or Fatal

- invariant violation
- core runtime corruption
- application-wide failure

Expected cancellation must not flood error logs.

---

## 67. Error Metrics

Track:

- errors by code
- errors by stage
- errors by provider
- errors by scope
- transient versus permanent errors
- retryable versus non-retryable errors
- stale errors suppressed
- canceled operations
- abandoned operations
- revision failure rate
- session failure rate
- provider degradation count
- fatal error count
- fallback success rate
- user retry count

---

## 68. Error Tracing

Error traces should identify:

```text
Which revision failed?
Which stage failed?
Which attempt failed?
Which provider was used?
Was the result current?
Was retry attempted?
Was fallback used?
What was the final disposition?
```

The trace should connect the error to the original revision pipeline.

---

## 69. Privacy and Errors

Error diagnostics must not expose reading content by default.

Do not log:

- source screenshots
- recognized text
- translated text
- provider prompt
- access token
- API key
- private file path unless sanitized
- user-selected window title unless explicitly allowed

Content diagnostics may be enabled only through explicit debug consent and bounded retention.

---

## 70. Error Deduplication

Repeated identical errors may occur rapidly.

Examples:

- capture permission denied every frame
- provider unavailable for every revision
- model load failure for every request

The runtime should deduplicate or rate-limit repeated notifications and logs.

Possible deduplication key:

```text
ErrorCode
+
ProviderId
+
Stage
+
SessionId
+
Time Window
```

Metrics should still preserve occurrence counts.

---

## 71. Error Storm Protection

An error storm can overload:

- logs
- UI notifications
- event bus
- telemetry
- retry system

Protection may include:

- repeated-error suppression
- provider circuit state
- session pause
- retry backoff
- event sampling
- aggregate notifications

---

## 72. Provider Degradation

Repeated provider errors may transition provider health:

```text
HEALTHY
    ↓
DEGRADED
    ↓
UNAVAILABLE
```

Recovery may transition:

```text
UNAVAILABLE
    ↓
PROBING
    ↓
HEALTHY
```

Detailed circuit-breaker behavior may be defined later.

The MVP may use a simpler consecutive-failure threshold.

---

## 73. Revision Failure

A revision is failed when required pipeline output cannot be produced and no valid recovery path remains.

Conceptually:

```text
Required Stage Failed
    ↓
Retry or Fallback Exhausted
    ↓
Revision Marked Failed
```

Revision failure must not automatically terminate the session.

The next captured revision may process normally.

---

## 74. Session Failure

A session fails when it cannot continue processing its configured source.

Examples:

- capture source permanently closed
- required permission unavailable
- no usable OCR provider
- session configuration invalid
- repeated critical resource failure

The session may enter:

```text
FAILED
```

while the application remains usable.

---

## 75. Application Failure

Application-level failure is reserved for conditions that invalidate the runtime itself.

Examples:

- control loop terminated
- artifact ownership corrupted
- unrecoverable native subsystem failure
- core configuration unreadable
- security boundary compromised

The runtime should avoid escalating revision or provider errors to application scope unnecessarily.

---

## 76. Error State Transitions

Possible WorkItem transitions:

```text
QUEUED
    ↓
RUNNING
    ├── SUCCEEDED
    ├── FAILED
    ├── CANCELED
    ├── STALE
    └── ABANDONED
```

A WorkItem may transition to one terminal state only.

Duplicate terminal events are ignored or recorded as diagnostics.

---

## 77. Revision Error States

A revision may conceptually be:

```text
ACTIVE
PROCESSING
DEGRADED
SUCCEEDED
FAILED
CANCELED
OBSOLETE
```

`OBSOLETE` is not the same as `FAILED`.

An obsolete revision may have been technically successful but replaced before commit.

---

## 78. Error and Artifact Publication

Failed, canceled, stale, or abandoned stage outputs must not be published as normal reusable artifacts.

Allowed publication:

```text
SUCCEEDED
```

Potentially allowed with explicit metadata:

```text
SUCCEEDED with warnings
```

Forbidden by default:

```text
FAILED
CANCELED
STALE
ABANDONED
PARTIAL UNVALIDATED OUTPUT
```

---

## 79. Error and Cache Policy

The cache must not retain invalid stage results as successful artifacts.

A future negative-result cache may store deterministic failures separately.

For the MVP:

```text
Failure
    ↓
No cache promotion
```

Warnings may be retained only if the artifact remains valid and compatible.

---

## 80. Error and Resource Cleanup

Every terminal outcome must trigger cleanup.

```text
SUCCEEDED
FAILED
CANCELED
STALE
ABANDONED
```

all require:

- release worker-local resources
- release leases where possible
- release provider request handles
- update concurrency accounting
- complete WorkItem terminal bookkeeping

`ABANDONED` may leave physical resources draining, but logical bookkeeping must still complete.

---

## 81. Cleanup Failure

Cleanup failure must not restore a completed WorkItem to active state.

Example:

```text
Work failed
    ↓
Cleanup also fails
```

Record:

```text
Primary Work Error
+
Cleanup Error
```

The cleanup error may escalate resource or provider health.

The primary logical outcome remains terminal.

---

## 82. Error and Shutdown

During shutdown, many operations may return cancellation-like failures.

Expected shutdown outcomes should normalize to:

```text
CANCELED
```

or:

```text
ABANDONED
```

rather than generating user-visible errors.

Only cleanup failures that threaten safe shutdown should be elevated.

---

## 83. Error and Performance

Errors affect performance through:

- timeout duration
- retry delay
- fallback cost
- repeated provider initialization
- stale execution
- cleanup latency
- UI error rendering

Performance metrics should distinguish:

```text
Successful Useful Latency
Failed Attempt Latency
Recovery Latency
```

---

## 84. Error and Cancellation Race

A failure and cancellation may occur nearly simultaneously.

Example:

```text
Provider connection fails
Cancellation requested at the same time
```

Suggested resolution:

1. if cancellation was already authoritative before failure observation, classify as canceled;
2. if failure became terminal first, classify as failed;
3. record the concurrent cancellation in diagnostics;
4. allow only one terminal state.

Exact ordering is determined by serialized Runtime Control processing.

---

## 85. Error and Timeout Race

A response may arrive near the timeout boundary.

The first terminal event accepted by Runtime Control wins.

Example:

```text
Timeout command accepted first
    ↓
Work marked failed or timed out
    ↓
Later provider completion becomes stale
```

or:

```text
Provider completion accepted first
    ↓
Work succeeds
    ↓
Later timeout command ignored
```

---

## 86. Error and Retry Attempt Identity

Every retry receives a new `AttemptId`.

```text
Revision 60
├── Attempt 1: timeout
├── Attempt 2: provider error
└── Attempt 3: success
```

Late results from Attempt 1 or Attempt 2 must not overwrite Attempt 3.

---

## 87. Error Mapping Registry

The runtime should centralize error mapping.

Conceptually:

```text
ErrorMappingRegistry
├── Provider SDK error → RuntimeError
├── Native error → RuntimeError
├── RuntimeError → RetryClass
├── RuntimeError → UserVisibleError
└── RuntimeError → Severity
```

This registry may be implemented as configuration, code, or provider-specific adapters.

It does not require one global class in the MVP.

---

## 88. MVP Error Policy

The MVP should use a conservative error model.

### Required Terminal Outcomes

```text
SUCCEEDED
FAILED
CANCELED
STALE
ABANDONED
```

### Required Error Fields

```text
ErrorCode
Category
Severity
RetryClass
Scope
Stage
TechnicalMessage
Correlation identifiers
```

### Required Behaviors

1. Provider-specific errors are normalized.
2. Cancellation is not logged as failure.
3. Stale results never update the UI.
4. Obsolete errors are suppressed from current user state.
5. Failed artifacts are not cached.
6. One WorkItem accepts only one terminal outcome.
7. Error messages do not expose secrets or reading content.
8. Retry decisions are centralized.
9. Session errors do not terminate the application unnecessarily.
10. Fatal errors stop new work before shutdown.

---

## 89. Suggested MVP Error Codes

Initial codes may include:

### Runtime

```text
RUNTIME_INTERNAL_ERROR
RUNTIME_INVALID_STATE
RUNTIME_SHUTTING_DOWN
```

### Capture

```text
CAPTURE_PERMISSION_DENIED
CAPTURE_SOURCE_UNAVAILABLE
CAPTURE_FRAME_FAILED
CAPTURE_REGION_INVALID
```

### OCR

```text
OCR_INPUT_INVALID
OCR_PROVIDER_TIMEOUT
OCR_PROVIDER_UNAVAILABLE
OCR_AUTHENTICATION_FAILED
OCR_RESPONSE_INVALID
OCR_MODEL_FAILED
```

### Layout

```text
LAYOUT_INPUT_INVALID
LAYOUT_PROCESSING_FAILED
```

### Translation

```text
TRANSLATION_INPUT_INVALID
TRANSLATION_PROVIDER_TIMEOUT
TRANSLATION_PROVIDER_UNAVAILABLE
TRANSLATION_AUTHENTICATION_FAILED
TRANSLATION_RATE_LIMITED
TRANSLATION_QUOTA_EXCEEDED
TRANSLATION_RESPONSE_INVALID
```

### Resource

```text
ARTIFACT_NOT_FOUND
ARTIFACT_DISPOSED
MEMORY_BUDGET_EXCEEDED
RESOURCE_CLEANUP_FAILED
```

### Presentation

```text
PRESENTATION_BUILD_FAILED
UI_COMMIT_REJECTED
UI_TARGET_UNAVAILABLE
```

This list should remain intentionally small at first.

---

## 90. Example: Remote Translation Timeout

```text
Translation request starts
    ↓
Configured timeout expires
    ↓
Provider Adapter aborts request
    ↓
TRANSLATION_PROVIDER_TIMEOUT created
    ↓
WorkFailed submitted
    ↓
Runtime validates revision is current
    ↓
Retry Policy evaluates retry
```

Possible final behavior:

- delayed retry
- fallback provider
- revision failure
- non-blocking UI error

---

## 91. Example: Timeout for Obsolete Revision

```text
Revision 70 translation running
    ↓
Revision 71 becomes current
    ↓
Revision 70 cancellation requested
    ↓
Provider later times out
```

Runtime outcome:

```text
Revision 70 result has no current authority
    ↓
Record stale or canceled diagnostic
    ↓
Do not show translation error to user
```

---

## 92. Example: Invalid OCR Response

```text
OCR provider returns malformed regions
    ↓
Provider Adapter normalization fails
    ↓
OCR_RESPONSE_INVALID
    ↓
No OCR Artifact published
    ↓
No cache promotion
    ↓
Retry or fallback evaluated
```

---

## 93. Example: Empty OCR Result

```text
OCR completes successfully
    ↓
No text regions detected
```

Possible result:

```text
SUCCEEDED
Output: empty OCR Artifact
Warning: none
```

The pipeline may produce an empty presentation.

It should not automatically classify this as provider failure.

---

## 94. Example: Cache Corruption

```text
Translation artifact found in cache
    ↓
Validation fails
    ↓
CACHE_ENTRY_CORRUPT
    ↓
Entry evicted
    ↓
Treat as cache miss
    ↓
Execute translation normally
```

The user may never need to see this error.

---

## 95. Example: Artifact Disposed During Cancellation

```text
Worker requests artifact lease
    ↓
Revision cancellation already began
    ↓
Lease denied
```

Expected outcome:

```text
CANCELED
```

or:

```text
STALE
```

not:

```text
RUNTIME_INTERNAL_ERROR
```

---

## 96. Example: Authentication Failure

```text
Provider rejects credential
    ↓
TRANSLATION_AUTHENTICATION_FAILED
    ↓
Provider marked unavailable
    ↓
No immediate automatic retry
    ↓
UI offers provider settings
```

---

## 97. Example: Memory Budget Exceeded

```text
OCR WorkItem requests admission
    ↓
Memory budget unavailable
```

Possible outcomes:

- defer WorkItem
- evict cache
- use lower-resolution processing
- cancel background work
- fail revision with resource error

The error is not necessarily fatal to the session.

---

## 98. Example: Fatal Runtime Invariant

```text
Artifact ownership count becomes invalid
    ↓
Runtime cannot determine safe disposal
```

Possible response:

```text
Stop admitting new work
    ↓
Prevent new commits
    ↓
Cancel sessions
    ↓
Record critical diagnostics
    ↓
Shutdown safely
```

---

## 99. Error Invariants

The runtime must preserve these invariants:

1. Every WorkItem has exactly one terminal outcome.
2. Failure, cancellation, staleness, and abandonment remain distinct.
3. Provider-specific exceptions are normalized before leaving adapters.
4. Obsolete errors do not replace current user-visible state.
5. Stale results cannot publish current pipeline authority.
6. Failed artifacts are not promoted to normal cache.
7. Cancellation is expected control flow.
8. Retry decisions do not belong to individual workers.
9. UI receives safe normalized error models.
10. Raw credentials and reading content are excluded from standard errors.
11. Session-scoped failures do not become application-fatal without cause.
12. Cache failure does not normally break pipeline correctness.
13. Cleanup occurs for every terminal outcome.
14. Duplicate terminal commands are handled idempotently.
15. Internal invariant violations receive elevated diagnostics.
16. Retry requires a new AttemptId.
17. Late attempt results cannot overwrite newer attempts.
18. User-visible messages are based on current relevant state.
19. Errors remain bounded under repeated failure.
20. Fatal errors stop new work before shutdown.

---

## 100. Testing Requirements

Tests should cover:

- successful terminal outcome
- provider timeout
- authentication failure
- rate limiting
- quota exhaustion
- malformed provider response
- empty OCR result
- low-confidence OCR warning
- layout fallback
- cache corruption
- cache unavailable
- resource lease denied after cancellation
- artifact disposed unexpectedly
- memory admission failure
- cancellation during provider request
- provider completion after cancellation
- timeout and completion race
- retry result arriving before old attempt
- duplicate completion command
- obsolete revision error suppression
- session close during error handling
- application shutdown during failure
- cleanup failure after primary failure
- fatal runtime invariant violation
- repeated-error deduplication
- error privacy validation

---

## 101. Open Questions

The following questions remain open:

- Which provider-specific error codes need dedicated normalized mappings?
- Should low-confidence OCR trigger automatic fallback?
- Should partial translation be considered success with warnings?
- Which errors should preserve the previous presentation?
- What consecutive-failure threshold should degrade a provider?
- Will the MVP support automatic provider fallback?
- Should deterministic failures be negatively cached later?
- How much technical detail should be available in the user-facing diagnostics screen?
- Should fatal subsystem errors restart only that subsystem?
- Which errors should trigger application restart?
- How should local model crashes be isolated?
- Should error messages include provider names?
- Which warning types should be visible by default?
- How long should repeated-error suppression remain active?

These questions can be resolved after provider selection and UI design.

---

## 102. Related Documents

- `README.md`
- `PIPELINE_RUNTIME.md`
- `WORK_QUEUE.md`
- `SCHEDULER.md`
- `CANCELLATION.md`
- `CACHE_POLICY.md`
- `MEMORY_MODEL.md`
- `THREADING_MODEL.md`
- `RESOURCE_LIFECYCLE.md`
- `PERFORMANCE_MODEL.md`
- `RETRY_POLICY.md`
- `RUNTIME_OBSERVABILITY.md`
- `RUNTIME_CONFIG.md`
- `../STATE_MACHINE.md`
- `../EVENT_BUS.md`
- `../DATA_FLOW.md`
- `../flows/SCREEN_COMIC_FLOW.md`

---

## 103. Next Step

The next runtime document should be:

```text
RETRY_POLICY.md
```

It should define:

- retry eligibility
- attempt identity
- maximum attempts
- immediate and delayed retry
- exponential backoff
- jitter
- retry-after support
- provider fallback
- retry budgets
- current-revision validation
- cancellation interaction
- timeout interaction
- resource cleanup between attempts
- user-triggered retry
- retry observability
- retry-storm protection

---

## 104. Summary

CRAI treats runtime completion as an explicit outcome:

```text
SUCCEEDED
FAILED
CANCELED
STALE
ABANDONED
```

Errors are normalized through:

```text
Detection
    ↓
Classification
    ↓
Scope Validation
    ↓
Relevance Validation
    ↓
Recovery Decision
    ↓
User-Safe Presentation
    ↓
Observability
```

The most important distinction is:

```text
Technical failure
    ≠
Cancellation
    ≠
Obsolete result
```

This distinction allows CRAI to:

- avoid false error messages during rapid scrolling
- prevent stale provider failures from disturbing current reading
- retry only meaningful work
- preserve clean runtime metrics
- recover providers independently
- keep application failures isolated to the smallest valid scope