# Runtime Error Model

* **Document:** Runtime Architecture / Error Model
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how CRAI Runtime represents, normalizes, classifies, correlates and propagates execution/runtime failures without confusing failure with execution authority, cancellation, business correctness or Presentation state.

The Runtime Error Model defines:

* normalized Runtime errors;
* error categories;
* severity;
* impact scope;
* recoverability;
* Retry hints;
* recovery-escalation hints;
* correlation;
* cause chain;
* privacy-safe diagnostic context;
* user-safe error projection inputs;
* error aggregation;
* error deduplication;
* error-storm protection;
* fatal Runtime policy.

This document does NOT own:

* WorkItem lifecycle;
* Attempt lifecycle;
* terminal WorkItem outcome;
* execution authority;
* Retry decision;
* Fallback selection;
* Provider routing;
* Business semantic validity;
* Presentation/UI commit semantics.

Canonical execution outcomes remain defined by `PIPELINE_RUNTIME.md`.

---

# 2. Architectural Position

```text
Worker / Execution Adapter / Runtime Component
        |
        v
Failure Detected
        |
        v
Boundary Normalization
        |
        v
RuntimeError
        |
        v
Runtime Control
        |
        v
Execution Relevance / Authority Validation
        |
        +--> irrelevant / stale diagnostic only
        |
        +--> relevant error
                    |
                    v
             Recovery Evaluation
                    |
                    +--> Retry Policy
                    |
                    +--> Routing / Fallback
                    |
                    +--> Business Recovery
                    |
                    +--> User Action
                    |
                    v
             User-Safe Projection
             + Observability
```

---

# 3. Core Ownership

```text
PIPELINE_RUNTIME
    owns execution outcomes.

ERROR_MODEL
    owns normalized error meaning.

RUNTIME CONTROL
    owns execution relevance / authority.

RETRY_POLICY
    owns same-work Retry decision.

ROUTING / FALLBACK
    owns alternative execution route.

BUSINESS MODULE
    owns semantic acceptance / rejection.

PRESENTATION / APPLICATION
    owns user-visible commit / interaction.

PROVIDER MANAGEMENT
    owns provider configuration / canonical health policy.
```

---

# 4. Error Is Not Outcome

`RuntimeError` and execution outcome are separate concepts.

## RuntimeError

Describes:

```text
why something failed
or became unusable
```

Possible information:

* category;
* code;
* severity;
* scope;
* recoverability;
* diagnostics;
* recovery hints.

## Execution Outcome

Describes:

```text
what happened to execution
```

Physical Attempt outcome MAY include:

```text
COMPLETED
FAILED
CANCELLED
ABANDONED
```

Accepted logical WorkItem outcome MAY include:

```text
SUCCEEDED
FAILED
CANCELLED
ABANDONED
```

Authority decisions MAY include:

```text
ACCEPT
REJECT_STALE
REJECT_CANCELLED
REJECT_DUPLICATE
REJECT_INVALID_STATE
REJECT_INTEGRITY
```

These MUST NOT be collapsed.

---

# 5. Business Rejection Is Separate

A Runtime execution may succeed technically and remain current, while the owning Business Module rejects the semantic result.

Example:

```text
Attempt COMPLETED
        |
        v
Runtime Authority ACCEPT
        |
        v
Runtime Artifact Published
        |
        v
Business Module Validation
        |
        v
BUSINESS_RESULT_REJECTED
```

This is NOT automatically:

```text
provider failure
Runtime invariant failure
stale execution
cancellation
```

---

# 6. Presentation Failure Is Separate

A valid Business Result MAY fail to become visible because:

* target closed;
* Presentation model invalid;
* UI dispatcher unavailable;
* target state changed;
* rendering failed.

Presentation/Application owns those semantics.

Runtime Error Model may normalize the technical error, but MUST NOT redefine Presentation ownership.

---

# 7. Core Principles

1. Every failure crossing an architecture boundary is normalized.

2. Raw provider/native exceptions do not reach UI directly.

3. Cancellation is not Failure by default.

4. Stale is an authority rejection, not physical Failure.

5. Abandoned execution is distinct from Cancelled execution.

6. Business rejection is distinct from Runtime Failure.

7. Presentation commit rejection is distinct from Runtime authority rejection.

8. Error does not decide Retry.

9. Error does not select Fallback.

10. Error does not mutate WorkItem state.

11. Error affects current execution only after relevance validation.

12. Warning does not create another terminal outcome.

13. Duplicate signals are handled idempotently.

14. Fatal is reserved for inability to preserve Runtime invariants.

15. Error diagnostics contain no user content by default.

16. Recovery ownership remains external to Error Model.

---

# 8. RuntimeError Model

Recommended conceptual model:

```text
RuntimeError
├── errorCode
├── category
├── severity
├── scope
├── recoverability
├── retryHint
├── recoveryHint
├── ownerModule?
├── businessStageId?
├── workType?
├── operation
├── messageKey
├── technicalMessage?
├── userMessageKey?
├── executionBindingReference?
├── providerDiagnosticCode?
├── causeReference?
├── context
├── occurredAt
└── correlation
```

Exact implementation is language-specific.

---

# 9. Stable Error Code

Every RuntimeError MUST have stable machine-readable `ErrorCode`.

Examples:

```text
INPUT_INVALID
ARTIFACT_NOT_FOUND
ARTIFACT_INTEGRITY_FAILED

EXECUTION_BINDING_TIMEOUT
EXECUTION_BINDING_UNAVAILABLE
EXECUTION_BINDING_RATE_LIMITED

RESOURCE_BUDGET_EXCEEDED
RESOURCE_CLEANUP_FAILED

RUNTIME_CONFIGURATION_INVALID
RUNTIME_INVALID_STATE

PERSISTENCE_WRITE_FAILED

BUSINESS_RESULT_REJECTED

PRESENTATION_COMMIT_FAILED

INVARIANT_VIOLATION
```

ErrorCode MUST:

* be stable;
* contain no variable data;
* support metrics;
* support mapping;
* support tests;
* remain independent from UI wording.

---

# 10. Error Category

Recommended stable categories:

```text
INPUT
VALIDATION
BUSINESS
EXECUTION_BINDING
RESOURCE
ARTIFACT
STATE
CONFIGURATION
SECURITY
PERSISTENCE
INTEGRATION
PRESENTATION
OBSERVABILITY
INTERNAL
```

---

# 11. Category Boundary

Category describes broad technical/semantic ownership.

It SHOULD NOT mirror every internal capability such as:

```text
OCR
LAYOUT
SEGMENTATION
PROMPT
```

Module-specific error codes remain allowed.

Examples:

```text
RECOGNITION_OUTPUT_INVALID
TRANSLATION_INPUT_INVALID
PRESENTATION_TARGET_INVALID
```

---

# 12. Severity

Recommended:

```text
INFO
WARNING
ERROR
CRITICAL
FATAL
```

---

# 13. INFO

Used for expected operational situations such as:

* stale diagnostic suppressed;
* duplicate signal ignored;
* expected cancellation;
* benign cleanup race.

---

# 14. WARNING

Used for recoverable degradation.

Examples:

* temporary provider/runtime slowdown;
* cache unavailable;
* optional persistence unavailable;
* cleanup delayed;
* optional plugin degraded.

---

# 15. ERROR

Current logical operation cannot complete successfully through the current path.

---

# 16. CRITICAL

Major Runtime capability is unavailable or repeated failure materially threatens service quality/correctness.

---

# 17. FATAL

Runtime cannot continue while preserving required invariants.

Examples:

* Runtime Control unavailable;
* Artifact ownership accounting corrupt;
* security boundary compromised;
* physical resource lifecycle cannot be trusted;
* accepted-state integrity cannot be guaranteed.

Severity does NOT determine Retry.

---

# 18. Error Scope

Recommended impact scopes:

```text
ATTEMPT
WORK_ITEM
EXECUTION_REVISION
EXECUTION_SCOPE
EXECUTION_BINDING
RUNTIME_COMPONENT
APPLICATION
```

Optional external/business correlation scopes remain separate.

---

# 19. ATTEMPT Scope

Only current physical Attempt failed or became unusable.

---

# 20. WORK_ITEM Scope

The logical WorkItem cannot complete through its current valid execution path.

---

# 21. EXECUTION_REVISION Scope

Required output for one ExecutionRevision cannot be produced under current accepted plan/execution conditions.

---

# 22. EXECUTION_SCOPE Scope

One Runtime ExecutionScope cannot continue its current execution intent.

This does NOT redefine ReadingSession business state.

---

# 23. EXECUTION_BINDING Scope

A resolved executable binding/runtime endpoint is unavailable, degraded or invalid.

Examples:

* provider deployment unavailable;
* local model runtime unavailable;
* plugin execution binding unavailable;
* native adapter unusable.

---

# 24. RUNTIME_COMPONENT Scope

A Runtime component cannot operate correctly.

Examples:

* Scheduler failure;
* Artifact Store failure;
* Resource Manager failure.

---

# 25. APPLICATION Scope

The entire application/runtime cannot continue safely.

Use sparingly.

---

# 26. BusinessStageId Is Context

`BusinessStageId` is correlation/context.

It is NOT a canonical Runtime error-impact scope.

---

# 27. Recoverability

Recommended:

```text
RECOVERABLE
RECOVERABLE_WITH_DEGRADATION
RECOVERABLE_AFTER_CONFIGURATION_CHANGE
RECOVERABLE_AFTER_USER_ACTION
RECOVERABLE_THROUGH_ALTERNATIVE_EXECUTION
NON_RECOVERABLE_FOR_WORK_ITEM
NON_RECOVERABLE_FOR_EXECUTION_REVISION
NON_RECOVERABLE_FOR_EXECUTION_SCOPE
NON_RECOVERABLE_FOR_APPLICATION
```

---

# 28. Recoverability vs Retryability

Critical distinction:

```text
Recoverable
    !=
Retry same Attempt path
```

Example:

```text
Execution binding unavailable
    ->
Recoverable through another binding
```

but:

```text
Retry same binding
```

may be useless.

---

# 29. Retry Hint

Error Model provides only a bounded hint.

Recommended:

```text
NONE
TRANSIENT
RETRY_AFTER
AFTER_CONFIGURATION_CHANGE
AFTER_USER_ACTION
RESOURCE_RECOVERY_POSSIBLE
UNKNOWN
```

Do NOT include:

```text
RETRY_WITH_FALLBACK
```

as an Error Model hint.

---

# 30. Recovery Hint

A separate non-binding `RecoveryHint` MAY exist:

```text
NONE
ALTERNATIVE_EXECUTION_MAY_EXIST
REPLAN_MAY_BE_REQUIRED
USER_ACTION_MAY_BE_REQUIRED
DEGRADATION_MAY_BE_POSSIBLE
RUNTIME_RESTART_MAY_BE_REQUIRED
UNKNOWN
```

This does NOT choose a recovery action.

---

# 31. Why Retry and Recovery Hints Are Separate

Example:

```text
EXECUTION_BINDING_UNAVAILABLE

RetryHint:
    NONE

RecoveryHint:
    ALTERNATIVE_EXECUTION_MAY_EXIST
```

This prevents Fallback from being hidden inside Retry taxonomy.

---

# 32. Correlation

Recommended:

```text
ApplicationInstanceId
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
BusinessStageId?
OwnerModule?
WorkType?
ExecutionBindingReference?
ProviderId?
Operation
CorrelationId?
TraceId?
```

Not every field is required.

---

# 33. ReadingSession Correlation

Where applicable:

```text
ReadingSessionId
```

MAY appear as business correlation metadata.

It MUST NOT replace:

```text
ExecutionScopeId
```

---

# 34. Cause Chain

Error MAY preserve an internal normalized cause chain.

Example:

```text
TRANSLATION_EXECUTION_FAILED
    caused by
EXECUTION_BINDING_NETWORK_ERROR
    caused by
SOCKET_TIMEOUT
```

Recovery uses normalized codes.

Raw exception detail remains diagnostics-only and redacted.

---

# 35. Error Context

Context MAY contain bounded metadata such as:

```text
Timeout
InputSizeClass
RegionCount
MemoryPressureLevel
ExecutionBindingReference
ProviderRequestId
ModelDeploymentId
RuntimeConfigurationSnapshotId
ArtifactType
ArtifactVersion
OperationPhase
ResourceClass
```

---

# 36. Error Context Privacy

Context MUST NOT contain by default:

* screenshot;
* OCR/source text;
* translated text;
* Prompt;
* full AI Context;
* API key;
* access token;
* complete provider payload;
* private path without sanitization;
* raw source URL.

---

# 37. Error Ownership

| Component                    | Responsibility                                     |
| ---------------------------- | -------------------------------------------------- |
| Worker                       | Detect local physical execution failure            |
| Execution / Provider Adapter | Normalize binding/provider-specific failure        |
| Business Module              | Validate semantic correctness / Business rejection |
| Runtime Control              | Validate execution relevance / authority           |
| Retry Policy                 | Decide same-work Retry                             |
| Routing / Fallback           | Decide alternative execution binding               |
| Scheduler                    | Admit next Attempt                                 |
| Runtime Artifact Store       | Detect publication/integrity failure               |
| Resource Manager             | Detect resource/disposal failure                   |
| Storage                      | Normalize persistence failure                      |
| Presentation/Application     | Map relevant user-facing state and commit errors   |
| Observability                | Record diagnostics/metrics                         |

Detecting component does not automatically own recovery.

---

# 38. Exception Boundary

Incorrect:

```text
Provider SDK Exception
        |
        v
UI
```

Correct:

```text
Provider SDK Exception
        |
        v
Execution Adapter
        |
        v
Normalized RuntimeError
        |
        v
AttemptCompletion
        |
        v
Runtime Control
```

---

# 39. AttemptCompletion Boundary

Example:

```text
AttemptCompletion
├── ExecutionScopeId
├── ExecutionRevisionId
├── WorkItemId
├── AttemptId
├── PhysicalOutcome
├── RuntimeError?
└── TimingMetadata
```

AttemptCompletion is not the accepted WorkItem outcome.

---

# 40. Relevance Validation

Before an error changes current Runtime state, Retry or user-facing state, Runtime Control checks:

* ExecutionScope still eligible;
* ExecutionRevision still relevant;
* WorkItem not already terminal;
* Attempt lineage valid;
* execution authority not revoked;
* signal not duplicate;
* Runtime configuration identity compatible;
* no newer accepted outcome already exists.

Obsolete errors may remain diagnostics-only.

---

# 41. Error Relevance

Recommended:

```text
CURRENT_RELEVANT
OBSOLETE
STALE
DUPLICATE
SUPPRESSED
```

This metadata describes whether the error matters to current execution.

It does NOT replace execution outcome.

---

# 42. Failure

Failure means:

```text
execution or required technical operation
could not produce an acceptable technical result
```

Failure MAY have RuntimeError.

---

# 43. Cancellation

Cancellation means:

```text
execution authority was explicitly revoked
and stop semantics were requested/applied
```

Expected cancellation normally does not create generic RuntimeError.

---

# 44. Stale

Stale means:

```text
result or error belongs to execution
that no longer has current authority
```

Stale is not physical failure.

---

# 45. Abandoned

Abandoned means:

```text
Runtime stopped waiting for physical execution
while physical work may still continue
```

Abandonment does NOT prove provider/runtime failure.

---

# 46. Business Rejection

Business rejection means:

```text
execution result technically exists
but owning Business Module rejects its semantics
```

Possible normalized error:

```text
BUSINESS_RESULT_REJECTED
```

or a module-specific code.

---

# 47. Presentation Rejection

Presentation/Application may reject visible commit because target/UI state is no longer valid.

Possible normalized codes:

```text
PRESENTATION_TARGET_INVALID
PRESENTATION_COMMIT_REJECTED
PRESENTATION_DISPATCH_FAILED
```

These MUST remain distinct from Runtime stale/authority rejection.

---

# 48. Cancellation Normalization

Provider/native SDK may represent cancellation as exception.

Adapter MUST normalize expected cancellation to:

```text
PhysicalOutcome = CANCELLED
```

not:

```text
generic FAILED
```

Only cancellation-related cleanup/abort failure may produce additional RuntimeError.

---

# 49. Stale Error Suppression

Example:

```text
ExecutionRevision A
    provider timeout

ExecutionRevision B
    already current and successful
```

A's timeout:

* may be retained for diagnostics;
* does not affect current UI;
* does not downgrade B;
* does not create Retry;
* does not trigger user-facing failure by default.

---

# 50. Abandoned Error Handling

When Attempt becomes abandoned:

* execution authority remains revoked;
* downstream execution remains forbidden;
* physical binding/resource capacity may still be occupied;
* late Completion is rejected;
* resources remain tracked;
* prolonged physical lifetime may generate operational warnings/errors.

Abandonment alone is not a Provider failure.

---

# 51. Expected vs Unexpected Errors

## Expected

Examples:

* timeout;
* permission denied;
* invalid input;
* rate limit;
* configuration missing;
* resource pressure;
* persistence unavailable;
* cancellation cleanup timeout.

## Unexpected

Examples:

* impossible state transition;
* ownership-accounting corruption;
* duplicate accepted publication;
* required Runtime Artifact missing unexpectedly;
* unhandled native crash;
* impossible Attempt lineage.

Unexpected errors require stronger diagnostics.

---

# 52. Input Errors

Examples:

```text
INPUT_MISSING
INPUT_INVALID
INPUT_TOO_LARGE
SOURCE_UNSUPPORTED
LANGUAGE_PAIR_UNSUPPORTED
```

Usually:

* same-input Retry is inappropriate;
* user action or replan may be required;
* scope is WorkItem/ExecutionRevision/ExecutionScope depending on impact.

---

# 53. Validation Errors

Examples:

```text
ARTIFACT_TYPE_MISMATCH
ARTIFACT_VERSION_UNSUPPORTED
OUTPUT_CONTRACT_INVALID
GEOMETRY_INVALID
TRACEABILITY_INVALID
```

Validation failure must prevent unsafe publication/consumption.

---

# 54. Business Module Errors

Business Module owns its detailed error catalog.

Examples:

```text
02-modules/recognition/ERRORS.md
02-modules/translation/ERRORS.md
02-modules/presentation/ERRORS.md
```

Runtime Error Model defines only shared normalized dimensions.

---

# 55. Business Validation Errors

Possible shared forms:

```text
BUSINESS_RESULT_REJECTED
BUSINESS_CONTRACT_UNSATISFIED
BUSINESS_RESULT_INCOMPATIBLE
BUSINESS_RECOVERY_REQUIRED
```

Module-specific code SHOULD normally be preferred when semantics matter.

---

# 56. Execution Binding Error Model

Generic runtime binding errors:

```text
EXECUTION_BINDING_TIMEOUT
EXECUTION_BINDING_UNAVAILABLE
EXECUTION_BINDING_RATE_LIMITED
EXECUTION_BINDING_AUTH_FAILED
EXECUTION_BINDING_QUOTA_EXCEEDED
EXECUTION_BINDING_REQUEST_INVALID
EXECUTION_BINDING_RESPONSE_INVALID
EXECUTION_BINDING_CONTENT_REJECTED
EXECUTION_BINDING_INTERNAL_ERROR
EXECUTION_BINDING_NETWORK_ERROR
EXECUTION_BINDING_CANCELLED
```

Provider-specific adapters MAY additionally retain diagnostic provider codes.

---

# 57. Provider-Compatible Error Codes

Provider Management/module layers MAY expose provider-oriented aliases where useful.

However generic Runtime Error Model SHOULD prefer:

```text
EXECUTION_BINDING_*
```

for Runtime execution semantics.

---

# 58. Execution Binding Authentication Error

Typical:

```text
Recoverability =
    RECOVERABLE_AFTER_USER_ACTION
or
RECOVERABLE_AFTER_CONFIGURATION_CHANGE

RetryHint =
    NONE
```

Repeated unchanged automatic Retry is inappropriate.

---

# 59. Rate Limit

Rate limit MAY provide:

```text
RetryHint = RETRY_AFTER
```

Possible recovery paths include:

* delayed Retry;
* admission reduction;
* alternative execution evaluation.

Error Model does not choose among them.

---

# 60. Quota Exhaustion

Quota exhaustion usually implies:

```text
RetryHint = NONE
RecoveryHint = ALTERNATIVE_EXECUTION_MAY_EXIST
or
AFTER_CONFIGURATION_CHANGE / USER_ACTION
```

Do not repeatedly Retry unchanged binding.

---

# 61. Resource Errors

Examples:

```text
RESOURCE_BUDGET_EXCEEDED
MEMORY_BUDGET_EXCEEDED
GPU_MEMORY_EXHAUSTED
BUFFER_ALLOCATION_FAILED
RESOURCE_OWNERSHIP_INVALID
RESOURCE_DISPOSAL_FAILED
RESOURCE_LEASE_FAILED
NATIVE_RESOURCE_UNAVAILABLE
```

---

# 62. Resource Race Boundary

A Resource error during cancellation/draining may be an expected consequence.

Runtime MUST check:

* cancellation state;
* ExecutionRevision relevance;
* logical disposal state;
* lease state

before escalating.

---

# 63. Artifact Errors

Examples:

```text
ARTIFACT_NOT_FOUND
ARTIFACT_NOT_PUBLISHED
ARTIFACT_ALREADY_DISPOSED
ARTIFACT_LEASE_FAILED
ARTIFACT_TYPE_MISMATCH
ARTIFACT_INTEGRITY_FAILED
DUPLICATE_ARTIFACT_PUBLICATION
ARTIFACT_OWNERSHIP_TRANSFER_FAILED
```

Not every Artifact miss is an internal invariant failure.

---

# 64. State / Invariant Errors

Examples:

```text
RUNTIME_INVALID_STATE
INVALID_STATE_TRANSITION
WORKITEM_ALREADY_TERMINAL
ATTEMPT_LINEAGE_INVALID
DUPLICATE_ACCEPTED_OUTCOME
DUPLICATE_ARTIFACT_PUBLICATION
EXECUTION_AUTHORITY_VIOLATION
OWNERSHIP_ACCOUNTING_CORRUPT
INVARIANT_VIOLATION
```

---

# 65. Avoid Generic COMMIT_WITHOUT_AUTHORITY

Do NOT use one ambiguous:

```text
COMMIT_WITHOUT_AUTHORITY
```

because CRAI now has separate boundaries:

* Runtime Artifact publication;
* Business acceptance;
* durable Storage commit;
* Presentation visible commit.

Use boundary-specific errors instead.

---

# 66. Configuration Errors

Examples:

```text
RUNTIME_CONFIGURATION_INVALID
EXECUTION_BINDING_NOT_CONFIGURED
CREDENTIAL_REFERENCE_MISSING
INVALID_TIMEOUT
INVALID_CONCURRENCY_LIMIT
INVALID_MEMORY_BUDGET
UNSUPPORTED_RUNTIME_MODE
```

Provider canonical configuration errors belong to Provider Management.

---

# 67. Security Errors

Examples:

```text
CREDENTIAL_ACCESS_DENIED
CREDENTIAL_RESOLUTION_FAILED
UNTRUSTED_EXECUTION_BINDING
UNSAFE_PATH
INVALID_PLUGIN_SIGNATURE
PERMISSION_REQUIRED
SECURITY_POLICY_BLOCKED
```

Security error MUST:

* never leak secret;
* never perform unsafe Fallback;
* use appropriate severity;
* provide bounded actionable metadata.

---

# 68. Persistence Errors

Storage normalizes:

```text
PERSISTENCE_UNAVAILABLE
PERSISTENCE_READ_FAILED
PERSISTENCE_WRITE_FAILED
PERSISTENCE_CONFLICT
PERSISTENCE_CORRUPT
PERSISTENCE_QUOTA_EXCEEDED
PERSISTENCE_VERSION_UNSUPPORTED
PERSISTENCE_MIGRATION_FAILED
RECOVERY_POINT_INVALID
```

Business data ownership remains with owning Business Module.

Runtime Artifact Store MUST NOT become durable Storage because persistence failed.

---

# 69. Observability Errors

Examples:

```text
TELEMETRY_EXPORT_FAILED
TRACE_BUFFER_FULL
METRIC_PUBLISH_FAILED
DIAGNOSTIC_SNAPSHOT_FAILED
```

Default:

```text
record bounded local diagnostic if possible
degrade observability
continue Runtime execution
```

Observability failure MUST NOT alter correctness.

---

# 70. Integration Errors

Examples:

```text
INTEGRATION_CONTRACT_INVALID
ADAPTER_UNAVAILABLE
PROTOCOL_MISMATCH
UNSUPPORTED_EXECUTION_CAPABILITY
```

Integration failure is normalized at adapter/integration boundary.

---

# 71. Presentation Errors

Possible:

```text
PRESENTATION_TARGET_INVALID
PRESENTATION_MODEL_INVALID
PRESENTATION_DISPATCH_FAILED
PRESENTATION_COMMIT_REJECTED
PRESENTATION_RENDER_FAILED
```

Presentation/Application owns final user-facing interpretation.

---

# 72. Internal Errors

Examples:

```text
INVARIANT_VIOLATION
OWNERSHIP_ACCOUNTING_CORRUPT
UNEXPECTED_EXECUTION_STATE
UNHANDLED_EXCEPTION
RUNTIME_CONTROL_FAILED
SCHEDULER_INTERNAL_FAILURE
RESOURCE_MANAGER_FAILED
```

Internal error SHOULD include:

* correlation;
* bounded safe snapshot;
* no user content;
* explicit escalation scope.

---

# 73. Fatal Error Policy

Fatal is reserved for inability to preserve Runtime invariants.

Possible triggers:

* Runtime Control no longer trustworthy;
* Artifact ownership state corrupt;
* security boundary compromised;
* Resource lifecycle accounting unusable;
* repeated uncontained main-process native crash;
* safe execution acceptance cannot be guaranteed.

---

# 74. Fatal Handling

Recommended:

```text
Stop New Admission
        |
        v
Revoke Application Execution Authority
        |
        v
Prevent New Runtime Artifact Publication
        |
        v
Cancel / Drain ExecutionScopes
        |
        v
Bounded Resource Cleanup
        |
        v
Persist Safe Diagnostics
        |
        v
Exit / Restart Safely
```

Business Session shutdown is coordinated separately by Application/Business owners.

---

# 75. Error Propagation

```text
Failure Detected
        |
        v
RuntimeError Normalized
        |
        v
AttemptCompletion / Component Error Signal
        |
        v
Runtime Control Validates Relevance
        |
        +--> obsolete -> diagnostics only
        |
        +--> current
                    |
                    v
             Recovery Evaluation
                    |
                    v
             User-Safe Mapping / Observability
```

---

# 76. Recovery Evaluation

Recovery may involve separate owners:

```text
Retry Policy
Routing / Fallback
Business Orchestration
Application / User Action
Provider Management
Runtime Restart / Degradation
```

Error Model only supplies normalized input/hints.

---

# 77. Error Aggregation

A WorkItem/ExecutionRevision may accumulate multiple errors.

Recommended:

```text
FailureAggregate
├── primaryError
├── attemptErrors[]
├── executionBindingErrors[]
├── businessRejections[]
├── presentationErrors[]
├── cleanupErrors[]
├── recoveryEvents[]
├── finalDisposition
└── userVisibleCause?
```

---

# 78. Fallback Errors

Do NOT make:

```text
FallbackErrors[]
```

a Runtime Error Model primitive.

Alternative-binding failures may be stored as:

```text
executionBindingErrors[]
recoveryEvents[]
```

Routing/Fallback observability owns route lineage.

---

# 79. Root Error Selection

Root user-visible cause may consider:

1. final blocking cause;

2. user actionability;

3. current relevance;

4. impact scope;

5. causal relation;

6. severity;

7. Business/Presentation outcome.

Diagnostics may retain a bounded full chain.

---

# 80. UserVisibleError Projection

Recommended normalized projection input:

```text
UserVisibleError
├── titleKey
├── messageKey
├── level
├── suggestedActions[]
├── technicalReference?
├── currentRelevance
└── preservePreviousPresentationHint?
```

---

# 81. UserVisibleError Boundary

Error Model MAY suggest actions.

Presentation/Application decides:

* which action button appears;
* whether Retry is enabled;
* whether Settings opens;
* whether previous Presentation remains;
* whether modal/inline/banner is used.

---

# 82. User Error Levels

Possible:

```text
INLINE_NOTICE
NON_BLOCKING_WARNING
EXECUTION_BLOCKING_ERROR
EXECUTION_SCOPE_BLOCKING_ERROR
APPLICATION_BLOCKING_ERROR
```

Avoid using Runtime term:

```text
SESSION_BLOCKING_ERROR
```

unless specifically referring to Reading Session business state.

---

# 83. Suggested User Actions

Possible hints:

```text
RETRY
RESELECT_SOURCE
CHECK_NETWORK
OPEN_PROVIDER_SETTINGS
CHANGE_EXECUTION_OPTION
REDUCE_INPUT
RESTART_OPERATION
RESTART_APPLICATION
REPORT_PROBLEM
NONE
```

Exact Presentation/UI behavior is external.

---

# 84. Preserve Previous Presentation

Error Model MAY expose:

```text
preservePreviousPresentationHint
```

but Presentation decides whether it is valid and non-misleading.

---

# 85. Warning Model

Warnings do not invalidate the primary result by default.

Recommended:

```text
Warning
├── warningCode
├── severity
├── ownerModule?
├── userVisibleHint
└── metadata
```

MVP:

```text
SUCCEEDED
+
bounded warnings
```

Do NOT create:

```text
SUCCEEDED_WITH_WARNINGS
```

unless Pipeline Runtime explicitly adopts such an outcome later.

---

# 86. Warning Rules

Warnings MUST:

* be bounded;
* be observable;
* be privacy-safe;
* not automatically trigger Retry;
* not mutate immutable Runtime Artifact;
* remain separate from Error terminal semantics.

---

# 87. Error Events

Possible normalized events:

```text
RuntimeErrorNormalized
ExecutionFailureAccepted
ExecutionErrorSuppressed
ExecutionRevisionDegraded
ExecutionRevisionFailed
ExecutionScopeDegraded
ExecutionScopeFailed
ExecutionBindingDegraded
ExecutionBindingRecovered
BusinessResultRejected
PresentationCommitFailed
ResourceCleanupFailed
PersistenceDegraded
RuntimeFatal
```

Final names follow Event Standard.

---

# 88. Provider Health Events

`ExecutionBindingDegraded` is a Runtime observation.

Canonical Provider health/governance event ownership belongs to Provider Management.

Runtime MUST NOT silently mutate canonical Provider health state.

---

# 89. Event Payload

Recommended:

```text
errorCode
category
severity
scope
recoverability
retryHint
recoveryHint
executionScopeId?
executionRevisionId?
workItemId?
attemptId?
businessStageId?
workType?
executionBindingReference?
occurredAt
```

No raw content.

---

# 90. Logging Policy

## Trace / Debug

* stale error suppressed;
* expected cancellation;
* duplicate signal ignored;
* obsolete error diagnostics.

## Info

* recovery path activated;
* execution binding recovered;
* degraded mode entered/exited.

## Warning

* transient execution-binding failure;
* cache/persistence degraded;
* cleanup slow;
* repeated Business rejection.

## Error

* current WorkItem/ExecutionRevision failed;
* Business Result cannot be accepted;
* Presentation required commit failed.

## Critical / Fatal

* invariant violation;
* Runtime Control failure;
* unsafe resource/security state.

---

# 91. Error Metrics

Recommended:

```text
errors by code/category/scope
execution-binding failures
resource failures
persistence failures
business-result rejection
presentation failure
transient vs permanent
retry-hint distribution
recovery-hint distribution
stale error suppression
cancellation count
abandonment count
ExecutionRevision failure rate
ExecutionScope failure rate
recovery success
fatal count
cleanup failure
user-action rate
warning count
```

Fallback success belongs to Routing/Recovery observability.

---

# 92. Metrics Dimensions

Prefer:

```text
ErrorCode
Category
Scope
OwnerModule
BusinessStageId
WorkType
ExecutionClass
ExecutionBindingClass
PhysicalOutcome
AuthorityOutcome
```

Avoid raw IDs as aggregate metric labels.

---

# 93. Error Tracing

Trace SHOULD answer:

* which ExecutionScope;
* which ExecutionRevision;
* which WorkItem;
* which Attempt;
* which owner module;
* which operation;
* which execution binding/provider runtime;
* whether execution authority remained;
* whether Retry occurred;
* whether alternative execution/recovery occurred;
* whether Business acceptance succeeded;
* whether Presentation commit succeeded;
* final disposition.

---

# 94. Privacy

Standard diagnostics MUST NOT contain:

* screenshot;
* recognized text;
* translated text;
* Prompt;
* full AI Context;
* tokens;
* API keys;
* raw provider body;
* window title;
* raw source URL;
* unsanitized private path.

Content diagnostics require explicit authorization and bounded retention.

---

# 95. Error Deduplication

Possible dedup key:

```text
ErrorCode
OwnerModule
ExecutionBindingClass
ExecutionScopeId?
TimeWindow
```

Deduplication affects notification/logging.

It MUST NOT erase occurrence counts.

---

# 96. Error Storm Protection

Mechanisms MAY include:

* deduplication;
* log throttling;
* event sampling;
* bounded diagnostic buffer;
* Retry backoff;
* recovery aggregation;
* execution-binding degradation signal;
* Scheduler admission reduction;
* user-notification aggregation.

Error handling MUST NOT create additional overload.

---

# 97. Provider / Execution Binding Degradation

Repeated binding error may generate Runtime health observations such as:

```text
READY
    |
    v
DEGRADED
    |
    v
UNAVAILABLE
    |
    v
PROBING
    |
    v
READY
```

Canonical ownership depends on Provider Runtime / Provider Management architecture.

Error Model only provides normalized signals.

---

# 98. ExecutionRevision Failure

ExecutionRevision MAY become unable to produce required output when:

```text
required WorkItem cannot succeed
        |
        v
same-path Retry exhausted / inappropriate
        |
        v
external recovery has no accepted executable path
        |
        v
ExecutionRevision failure accepted
```

Error Model does not decide whether alternative execution paths are exhausted.

Runtime/Application receives that conclusion from responsible recovery owners.

---

# 99. ExecutionScope Failure

ExecutionScope may fail when its current execution intent cannot continue.

Examples:

* source unavailable;
* permission unavailable;
* required capability unavailable;
* unrecoverable Runtime configuration;
* repeated critical resource failure.

Application/business layer decides how this maps to ReadingSession state.

---

# 100. Application Failure

Use only when Runtime itself cannot continue safely.

One WorkItem, ExecutionRevision, provider or Storage error MUST NOT automatically escalate to Application scope.

---

# 101. Artifact Publication Boundary

Candidate output associated with:

```text
FAILED
CANCELLED
ABANDONED
```

or rejected authority:

```text
REJECT_STALE
REJECT_CANCELLED
REJECT_INTEGRITY
```

MUST NOT be published as accepted reusable Runtime Artifact.

---

# 102. Business Rejection After Publication

A Runtime Artifact may already be technically published before Business semantic validation.

If Business rejects it:

* it MUST NOT become canonical Business truth;
* it SHOULD NOT be cache-promoted by default;
* downstream Business Stage satisfaction does not occur;
* Runtime Artifact follows ordinary retention/disposal policy.

---

# 103. Cleanup Failure

Cleanup failure does not replace the primary logical outcome.

Recommended:

```text
Primary Execution / Business Outcome
+
Cleanup RuntimeError
```

Cleanup failure may degrade:

* Resource Manager;
* Provider Runtime;
* Plugin Runtime;
* Runtime component health.

---

# 104. Cleanup Retry

Cleanup Retry is not WorkItem Retry.

It:

* creates no new Business WorkItem;
* grants no execution authority;
* only retries physical cleanup;
* remains bounded.

---

# 105. Error During Shutdown

During shutdown:

* expected cancellation remains cancellation;
* unresponsive execution may become abandoned;
* late stale result is rejected;
* cleanup failure only escalates when safety is threatened;
* user should not see cascades of expected shutdown errors.

---

# 106. Error Race Resolution

## Failure vs Cancellation

Runtime Control serializes authority-relevant transition.

## Completion vs Timeout

Physical events may race, but accepted logical outcome remains serialized.

## Duplicate Completion

First accepted logical outcome wins.

Duplicate signal is ignored/diagnosed.

## Late Attempt Result

Authority validation rejects obsolete result.

## Business Rejection vs New Revision

Business rejection from obsolete ExecutionRevision MUST NOT overwrite current user state.

---

# 107. Error Mapping Registry

Conceptual:

```text
Raw Execution Error
    ->
RuntimeError

RuntimeError
    ->
RetryHint / RecoveryHint

RuntimeError + Current Runtime State
    ->
UserVisibleError Projection

RuntimeError
    ->
Severity / Scope / Recoverability
```

A single global implementation registry is optional.

---

# 108. MVP Error Fields

Required:

```text
ErrorCode
Category
Severity
Scope
Recoverability
RetryHint
RecoveryHint
TechnicalMessage?
Correlation
OccurredAt
```

---

# 109. MVP Required Behaviors

1. Provider/execution-binding errors are normalized.

2. Cancellation is not logged as generic Failure.

3. Stale error does not affect current UI.

4. Business rejection remains distinct from Runtime failure.

5. Failed/rejected output does not promote to reusable cache.

6. One WorkItem accepts one logical terminal outcome.

7. Error contains no secret/reading content.

8. Retry decision remains centralized.

9. Fallback decision remains outside Retry/Error Model.

10. ExecutionScope failure does not automatically fail Application.

11. Fatal error stops admission before shutdown.

12. Warning does not create another outcome.

13. Persistence error normalizes at Storage boundary.

14. Observability failure does not break Runtime correctness.

15. Cleanup failure does not replace primary outcome.

16. Presentation commit failure remains Presentation-owned.

---

# 110. Suggested MVP Error Codes

## Runtime

```text
RUNTIME_INTERNAL_ERROR
RUNTIME_INVALID_STATE
RUNTIME_SHUTTING_DOWN
RUNTIME_CONTROL_FAILED
EXECUTION_AUTHORITY_VIOLATION
```

## Execution Binding

```text
EXECUTION_BINDING_TIMEOUT
EXECUTION_BINDING_UNAVAILABLE
EXECUTION_BINDING_AUTH_FAILED
EXECUTION_BINDING_RATE_LIMITED
EXECUTION_BINDING_QUOTA_EXCEEDED
EXECUTION_BINDING_RESPONSE_INVALID
```

## Resource / Artifact

```text
ARTIFACT_NOT_FOUND
ARTIFACT_INTEGRITY_FAILED
ARTIFACT_ALREADY_DISPOSED
ARTIFACT_OWNERSHIP_TRANSFER_FAILED
RESOURCE_BUDGET_EXCEEDED
RESOURCE_CLEANUP_FAILED
RESOURCE_LEASE_FAILED
```

## Business

```text
BUSINESS_RESULT_REJECTED
BUSINESS_CONTRACT_UNSATISFIED
```

## Presentation

```text
PRESENTATION_TARGET_INVALID
PRESENTATION_COMMIT_REJECTED
PRESENTATION_DISPATCH_FAILED
```

## Configuration / Security

```text
RUNTIME_CONFIGURATION_INVALID
CREDENTIAL_REFERENCE_MISSING
CREDENTIAL_ACCESS_DENIED
PERMISSION_REQUIRED
SECURITY_POLICY_BLOCKED
```

## Persistence

```text
PERSISTENCE_UNAVAILABLE
PERSISTENCE_READ_FAILED
PERSISTENCE_WRITE_FAILED
PERSISTENCE_CONFLICT
PERSISTENCE_CORRUPT
```

Module-specific codes remain in module documents.

---

# 111. Example — Execution Binding Timeout

```text
Execution binding times out
        |
        v
Adapter creates EXECUTION_BINDING_TIMEOUT
        |
        v
AttemptCompletion submitted
        |
        v
Runtime Control validates relevance
        |
        +--> obsolete
        |       -> diagnostics only
        |
        +--> current
                -> Retry Policy evaluates
```

---

# 112. Example — Alternative Execution Recovery

```text
Execution binding unavailable
        |
        v
RuntimeError normalized
        |
        v
RetryHint = NONE
RecoveryHint = ALTERNATIVE_EXECUTION_MAY_EXIST
        |
        v
Routing / Recovery evaluates
        |
        v
new binding selected?
```

Error Model does not select the binding.

---

# 113. Example — Invalid Execution Response

```text
Provider/runtime response malformed
        |
        v
Adapter validation fails
        |
        v
EXECUTION_BINDING_RESPONSE_INVALID
        |
        v
No accepted Runtime Artifact publication
        |
        v
Retry / Recovery evaluated by owners
```

---

# 114. Example — Business Rejection

```text
Attempt completes
        |
        v
Runtime authority accepted
        |
        v
Runtime Artifact published
        |
        v
Translation Module validates
        |
        v
BUSINESS_RESULT_REJECTED
        |
        v
No downstream Business Stage satisfaction
        |
        v
Recovery / replan / user action
```

---

# 115. Example — Presentation Commit Rejected

```text
Business Result valid
        |
        v
Presentation commit queued
        |
        v
Target closes
        |
        v
PRESENTATION_TARGET_INVALID
        |
        v
No visible commit
```

This does not retroactively invalidate the Business Result.

---

# 116. Example — Artifact Missing During Cancellation

```text
Worker requests Artifact Lease
        |
        v
ExecutionRevision already draining
        |
        v
Lease denied
```

Expected disposition may be cancellation/stale handling rather than internal invariant failure.

---

# 117. Example — Persistence Conflict

```text
Storage write uses outdated persistence version
        |
        v
PERSISTENCE_CONFLICT
        |
        v
Owning Business Module decides reload/merge/retry semantics
```

Storage does not acquire business ownership.

---

# 118. Example — Observability Failure

```text
Trace export fails
        |
        v
TELEMETRY_EXPORT_FAILED
        |
        v
bounded local diagnostics if possible
        |
        v
Runtime continues
```

---

# 119. Example — Fatal Invariant

```text
Runtime Artifact ownership accounting corrupt
        |
        v
INVARIANT_VIOLATION
        |
        v
Stop Admission
        |
        v
Revoke Application Authority
        |
        v
Bounded Cleanup
        |
        v
Safe Shutdown
```

---

# 120. Architecture Invariants

1. Error Model does not own terminal WorkItem outcome.

2. RuntimeError is distinct from execution outcome.

3. Physical Attempt outcome is distinct from authority outcome.

4. Failure is distinct from Cancellation.

5. Failure is distinct from Stale.

6. Failure is distinct from Abandoned.

7. Runtime failure is distinct from Business rejection.

8. Runtime authority rejection is distinct from Presentation commit rejection.

9. Provider/native exception is normalized before boundary crossing.

10. Error does not self-Retry.

11. Error does not select Fallback.

12. Error does not mutate Runtime state directly.

13. Relevance validation is required before current-state impact.

14. Obsolete error does not change current Presentation state.

15. Warnings do not create another terminal outcome.

16. Failed/rejected output does not become successful reusable result.

17. Runtime authority acceptance alone does not imply Business correctness.

18. Business result rejection does not imply Runtime invariant failure.

19. Cleanup failure does not change primary execution/business outcome.

20. Duplicate completion/error signal is handled idempotently.

21. Fatal error stops admission before destructive shutdown.

22. Persistence errors remain at Storage boundary.

23. Artifact errors do not automatically become persistence errors.

24. Observability failure does not break Runtime correctness.

25. User-facing mapping never receives raw provider exception directly.

26. Diagnostics contain no user content by default.

27. Error categories do not hard-code internal OCR/Layout capability taxonomy.

28. RetryHint is not Retry decision.

29. RecoveryHint is not Recovery decision.

30. Alternative execution is not Retry taxonomy.

31. Business Module owns semantic error catalog.

32. Runtime Control owns final execution relevance.

33. Routing/Fallback owns alternative binding choice.

34. Provider Management owns canonical provider policy/health semantics.

35. ExecutionScope/ExecutionRevision terminology is canonical.

36. ReadingSession is only optional business correlation.

37. Error handling remains bounded during error storms.

38. Execution-binding degradation signal does not itself mutate provider policy.

39. Presentation owns visible commit failure semantics.

40. Error Model remains an interpretation/normalization layer, not an orchestration engine.

---

# 121. Recommended MVP

CRAI MVP SHOULD support:

* RuntimeError normalization;
* stable ErrorCode;
* category;
* severity;
* scope;
* recoverability;
* RetryHint;
* RecoveryHint;
* ExecutionScope/ExecutionRevision correlation;
* provider/execution-binding normalization;
* Business rejection separation;
* Presentation failure separation;
* stale/cancellation/abandonment separation;
* user-safe projection;
* bounded cause chain;
* deduplication;
* error-storm protection;
* content-free diagnostics;
* cleanup-error composition;
* fatal Runtime policy.

MVP MAY defer:

* advanced automatic root-cause inference;
* distributed error aggregation;
* persistent cross-run error correlation;
* adaptive degradation scoring;
* sophisticated provider-health inference;
* AI-generated user troubleshooting.

---

# 122. Testing Requirements

Tests SHOULD include:

* execution-binding timeout;
* authentication failure;
* rate limit;
* quota exhaustion;
* malformed provider response;
* invalid input;
* validation failure;
* Business Result rejection;
* Presentation target rejection;
* Artifact lease denied after cancellation;
* Artifact integrity failure;
* persistence conflict;
* persistence unavailable;
* observability export failure;
* stale error suppression;
* cancellation exception normalization;
* abandoned physical operation;
* duplicate Completion;
* timeout/completion race;
* cleanup failure after primary failure;
* recovery escalation without Retry fallback;
* alternative execution selected externally;
* user-safe mapping;
* warning handling;
* privacy validation;
* error deduplication;
* error storm;
* fatal invariant;
* ExecutionScope failure without Application failure.

---

# 123. Open Decisions

The following remain open:

* exact RuntimeError schema;
* Error Mapping Registry implementation;
* module-specific error-catalog conventions;
* RecoveryHint taxonomy;
* user-visible action taxonomy;
* execution-binding degradation thresholds;
* Business rejection normalization contract;
* Presentation error projection contract;
* warning visibility defaults;
* previous-Presentation retention hints;
* diagnostics technical-detail level;
* deduplication window;
* fatal subsystem restart boundary;
* provider name visibility in UI;
* persisted diagnostic retention.

---

# 124. Related Documents

Runtime:

* `PIPELINE_RUNTIME.md`
* `RUNTIME_COMPONENTS.md`
* `RETRY_POLICY.md`
* `CANCELLATION.md`
* `SCHEDULER.md`
* `WORK_QUEUE.md`
* `MEMORY_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `CACHE_POLICY.md`
* `PERFORMANCE_MODEL.md`
* `RUNTIME_OBSERVABILITY.md`
* `RUNTIME_CONFIG.md`
* `BOOT_SEQUENCE.md`

External:

* `../ai/ROUTING.md`
* `../ai/FALLBACK.md`
* `../../02-modules/provider-management/`
* `../../02-modules/presentation/`
* `../../02-modules/storage/`
* `../../02-modules/*/ERRORS.md`

---

# 125. Completion Criteria

`ERROR_MODEL.md` is synchronized when:

* terminal outcome remains owned by Pipeline Runtime;
* RuntimeError remains distinct from AttemptCompletion;
* physical outcome and authority outcome remain separate;
* ExecutionScope/ExecutionRevision terminology is canonical;
* Session/Revision Runtime scopes are removed;
* RetryHint contains no Fallback decision;
* alternative execution uses RecoveryHint / Routing ownership;
* execution-binding errors replace Provider-owned Runtime semantics where appropriate;
* Provider Management no longer becomes Runtime recovery executor;
* Business rejection is distinct from Runtime Failure;
* Presentation commit failure is distinct from Runtime authority;
* cleanup failure remains secondary;
* module-specific error catalogs remain module-owned;
* persistence remains Storage-owned;
* warnings do not create another terminal outcome;
* privacy/deduplication/error-storm policy remains explicit;
* fatal handling stops admission before shutdown.

---

# 126. Summary

CRAI Runtime Error Model follows:

```text
Failure / Rejection Detected
        |
        v
Normalize
        |
        v
RuntimeError
        |
        v
Validate Current Relevance
        |
        +--> Obsolete Diagnostic
        |
        +--> Current Relevant Error
                    |
                    v
             Recovery Owner
                    |
                    v
             User-Safe Projection
             + Observability
```

The central distinctions are:

```text
RuntimeError
    explains why something failed.

PhysicalOutcome
    says what happened physically.

AuthorityDecision
    says whether execution may still matter.

BusinessAcceptance
    says whether semantic result is valid.

PresentationCommit
    says whether the result becomes visible.

Retry
    repeats compatible execution.

Fallback
    changes execution route.
```

These concepts MUST remain separate.
