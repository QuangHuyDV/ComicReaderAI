# Capture Errors

> **Project:** CRAI
> **Module:** `capture`
> **Path:** `doc/02-modules/capture/ERRORS.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the Capture-owned error contract.

Capture errors describe failures or rejections that occur while Capture:

```text
validates CaptureInvocation
validates CaptureSource
acquires provider data
normalizes provider output
constructs CandidateCaptureResult
manages Capture-owned temporary resources
manages CaptureSource lifecycle
```

Capture errors do not define:

```text
Runtime Attempt failure authority
Runtime cancellation authority
Runtime deadline authority
Runtime retry execution
RuntimeRevision staleness
Artifact publication failure
Recognition failure
Translation failure
Presentation failure
Reading Session failure
```

---

# 2. Error Boundary

Canonical failure flow:

```text
CaptureOperation
    ↓
Capture-owned error/rejection
    ↓
CaptureCompletion
    ↓
Runtime
    ↓
Runtime determines Attempt outcome
    ↓
Runtime Retry Policy if applicable
```

Capture reports what happened.

Runtime decides what that means for execution.

---

# 3. Error Principles

## 3.1 Stable Error Codes

Consumers depend on:

```text
ErrorCode
```

not human-readable messages.

Messages are diagnostic only.

---

## 3.2 Fail Fast

Invalid Capture requests should be rejected before:

```text
provider acquisition
large buffer allocation
native stream acquisition
temporary frame allocation
```

whenever possible.

---

## 3.3 Capture-Owned Semantics Only

Capture must not create errors for conditions owned by Runtime.

Examples of non-Capture errors:

```text
RuntimeRevisionStale
AttemptSuperseded
WorkItemCancelled
RuntimeDeadlineExceeded
RetryExhausted
SchedulerRejected
ArtifactPublicationFailed
```

---

## 3.4 Candidate Is Not Artifact

Capture errors may describe:

```text
Candidate construction failure
Candidate validation failure
```

but not:

```text
accepted Artifact publication failure
```

Artifact publication occurs after Runtime authority validation.

---

## 3.5 Preserve Reading Domain State

Capture failure must not directly:

```text
terminate Reading Session
change ReadingContextRevision
invalidate reading history
clear Presentation state
```

Cross-module reactions belong to Application/Runtime orchestration.

---

## 3.6 Privacy

Errors must never expose:

```text
raw image
screenshot
browser page content
OCR content
translation content
credentials
cookies
token
native handle
memory address
provider object
```

---

# 4. Error Code Format

```text
CAP-<CATEGORY>-<NUMBER>
```

Examples:

```text
CAP-REQ-001
CAP-SRC-003
CAP-ACQ-002
CAP-NRM-001
CAP-PERM-001
CAP-RES-002
CAP-INT-001
```

---

# 5. Error Categories

Recommended categories:

| Prefix  | Category                   |
| ------- | -------------------------- |
| `REQ`   | Request Validation         |
| `SRC`   | Capture Source             |
| `ACQ`   | Acquisition / Provider     |
| `NRM`   | Normalization / Candidate  |
| `PERM`  | Permission                 |
| `STATE` | Capture-owned State        |
| `RES`   | Capture-owned Resources    |
| `INT`   | Internal Capture Invariant |

The previous generic `IMG` and `DEV` categories are folded into more precise Capture boundaries.

---

# 6. Severity

```text
Info
Warning
Error
Critical
```

Meaning:

| Severity   | Meaning                                              |
| ---------- | ---------------------------------------------------- |
| `Info`     | Expected non-success condition                       |
| `Warning`  | Request/source condition prevents useful Capture     |
| `Error`    | Capture operation failed                             |
| `Critical` | Capture-owned invariant or internal subsystem failed |

Severity does not determine Runtime retry.

---

# 7. Retry Classification

Capture reports classification:

```text
RetryClassification
- NonRetryable
- RetryableTransient
- RetryableAfterSourceRefresh
- RetryableAfterPermissionChange
- RetryableAfterInputCorrection
```

Capture does not execute Runtime retry policy.

---

# 8. Retry Ownership

Correct:

```text
Capture
    ↓
CaptureError
    ↓
RetryClassification
    ↓
Runtime Retry Policy
    ↓
retry / no retry
```

Incorrect:

```text
CaptureError
    ↓
Capture automatically retries Runtime Attempt
```

---

# 9. Recovery Hint

Errors may include:

```text
RecoveryHint
```

Examples:

```text
RecreateSource
RefreshSource
RequestPermissionExternally
ReduceCaptureRegion
ReduceFrameSize
WaitForProvider
ChangeCaptureMode
CheckProvider
```

Recovery hints are advisory.

---

# 10. CaptureError Contract

Conceptually:

```text
CaptureError
├── errorCode
├── category
├── severity
├── retryClassification
├── recoveryHint?
├── captureSourceId?
├── sourceVersion?
├── operationId?
├── runtimeExecutionIdentity?
├── providerDomain?
├── diagnosticRef?
└── safeMessage?
```

---

# 11. RuntimeExecutionIdentity in Errors

Optional correlation:

```text
sessionId?
runtimeRevisionId?
workItemId?
attemptId?
correlationId?
traceId?
```

These values are externally owned.

Capture must not mutate or reinterpret them.

---

# 12. Request Validation Errors

## CAP-REQ-001 MissingCaptureSource

Meaning:

No CaptureSource was supplied.

```text
Severity:
Warning

RetryClassification:
RetryableAfterInputCorrection
```

---

# 13. CAP-REQ-002 InvalidCaptureSourceReference

Meaning:

The supplied CaptureSource reference is malformed or cannot identify a logical Capture source.

Examples:

```text
empty CaptureSourceId
invalid serialized source reference
invalid source version format
```

```text
Severity:
Warning

RetryClassification:
RetryableAfterInputCorrection
```

---

# 14. CAP-REQ-003 UnsupportedCaptureMode

Meaning:

Requested acquisition semantics are unsupported.

Examples:

```text
unsupported provider-event mode
unsupported continuous sample capability
unsupported source/mode combination
```

```text
Severity:
Warning

RetryClassification:
RetryableAfterInputCorrection
```

---

# 15. CAP-REQ-004 InvalidCaptureRegion

Meaning:

Requested region is invalid.

Examples:

```text
negative dimensions
non-finite coordinates
region outside authorized source scope
invalid coordinate space
```

```text
Severity:
Warning

RetryClassification:
RetryableAfterInputCorrection
```

---

# 16. CAP-REQ-005 UnsupportedCaptureCapability

Meaning:

Request requires a capability unavailable for the source/provider.

Examples:

```text
cursor exclusion unsupported
occluded-window capture unsupported
provider event trigger unsupported
requested pixel representation unsupported
```

```text
Severity:
Warning

RetryClassification:
NonRetryable
```

A changed source/provider may make a future invocation valid.

---

# 17. CAP-REQ-006 InvalidCaptureOptions

Meaning:

CaptureOptions are internally inconsistent.

Examples:

```text
maximumWidth <= 0
maximumHeight <= 0
unsupported pixel format
invalid privacy scope
conflicting provider-neutral options
```

```text
Severity:
Warning

RetryClassification:
RetryableAfterInputCorrection
```

---

# 18. CAP-REQ-007 ConfigurationIncompatible

Meaning:

The supplied configuration snapshot cannot be safely used by this Capture contract/provider combination.

```text
Severity:
Error

RetryClassification:
RetryableAfterInputCorrection
```

---

# 19. Capture Source Errors

## CAP-SRC-001 CaptureSourceNotFound

Meaning:

CaptureSourceId does not identify a known CaptureSource.

```text
Severity:
Warning

RetryClassification:
RetryableAfterSourceRefresh
```

---

# 20. CAP-SRC-002 CaptureSourceUnavailable

Meaning:

The logical CaptureSource exists but cannot currently serve acquisition.

Examples:

```text
browser connector disconnected
display unavailable
window/source disappeared
provider source invalidated
```

```text
Severity:
Warning

RetryClassification:
RetryableTransient
```

Recovery hint may be:

```text
RefreshSource
RecreateSource
```

---

# 21. CAP-SRC-003 SourceVersionConflict

Meaning:

Invocation expected one SourceVersion but CaptureSource has another incompatible version.

Example:

```text
expected = 12
current = 13
```

```text
Severity:
Info

RetryClassification:
RetryableAfterSourceRefresh
```

This is CaptureSource optimistic concurrency.

It is not Runtime staleness.

---

# 22. CAP-SRC-004 CaptureSourceSuspended

Meaning:

CaptureSource exists but is currently:

```text
SUSPENDED
```

and cannot accept new acquisition.

```text
Severity:
Info

RetryClassification:
RetryableTransient
```

---

# 23. CAP-SRC-005 CaptureSourceStopping

Meaning:

CaptureSource has entered:

```text
STOPPING
```

and cannot accept new acquisition.

```text
Severity:
Info

RetryClassification:
NonRetryable
```

A new/recreated source may be used instead.

---

# 24. CAP-SRC-006 CaptureSourceStopped

Meaning:

CaptureSource is terminally stopped.

```text
Severity:
Warning

RetryClassification:
NonRetryable
```

Do not retry against the same stopped source instance.

---

# 25. CAP-SRC-007 SourceChangedDuringAcquisition

Meaning:

CaptureSource semantics changed while provider acquisition or Candidate construction was in progress.

Examples:

```text
source replaced
region changed
provider association changed
SourceVersion advanced
```

```text
Severity:
Info

RetryClassification:
RetryableAfterSourceRefresh
```

This is SourceVersion compatibility, not Runtime supersession.

---

# 26. Acquisition Errors

## CAP-ACQ-001 ProviderUnavailable

Meaning:

Selected CaptureProvider cannot currently perform acquisition.

Examples:

```text
provider not initialized
browser connector unavailable
platform API unavailable
provider backend disconnected
```

```text
Severity:
Error

RetryClassification:
RetryableTransient
```

---

# 27. CAP-ACQ-002 ProviderFailure

Meaning:

CaptureProvider failed while acquiring source data.

```text
Severity:
Error

RetryClassification:
RetryableTransient
```

Provider-specific errors must be normalized before crossing Capture boundary.

---

# 28. CAP-ACQ-003 ProviderTimeout

Meaning:

The CaptureProvider failed to complete its own bounded provider operation within the provider-specific timeout.

```text
Severity:
Warning

RetryClassification:
RetryableTransient
```

Important:

```text
ProviderTimeout
≠
RuntimeDeadlineExceeded
```

---

# 29. Provider Timeout vs Runtime Deadline

Provider timeout:

```text
Capture-owned/provider-owned acquisition failure
        ↓
CAP-ACQ-003 ProviderTimeout
```

Runtime deadline:

```text
Runtime Attempt deadline expires
        ↓
Runtime cancellation/deadline handling
```

Capture MUST NOT convert every Runtime deadline into:

```text
CAP-ACQ-003
```

---

# 30. CAP-ACQ-004 InvalidProviderResult

Meaning:

Provider returned data that violates Capture provider contract.

Examples:

```text
missing buffer
invalid dimensions
invalid stride
unsupported provider representation
corrupted metadata
```

```text
Severity:
Error

RetryClassification:
RetryableTransient
```

Repeated occurrence may degrade Capture health.

---

# 31. CAP-ACQ-005 EmptyProviderFrame

Meaning:

Provider returned no usable frame content.

```text
Severity:
Warning

RetryClassification:
RetryableTransient
```

This does not mean OCR would fail.

Capture must not evaluate OCR usefulness.

---

# 32. CAP-ACQ-006 ProviderCapabilityChanged

Meaning:

Provider/source capability changed between validation and acquisition.

```text
Severity:
Info

RetryClassification:
RetryableAfterSourceRefresh
```

---

# 33. Normalization Errors

## CAP-NRM-001 FrameNormalizationFailed

Meaning:

Raw provider data could not be converted into Capture's normalized Candidate representation.

Examples:

```text
pixel conversion failure
orientation normalization failure
geometry normalization failure
scale metadata invalid
```

```text
Severity:
Error

RetryClassification:
RetryableTransient
```

---

# 34. CAP-NRM-002 UnsupportedFrameFormat

Meaning:

Provider output format cannot be normalized by current Capture implementation.

```text
Severity:
Warning

RetryClassification:
NonRetryable
```

A provider/configuration change may resolve it.

---

# 35. CAP-NRM-003 CandidateFrameTooLarge

Meaning:

Normalized Candidate exceeds configured Capture limits.

```text
Severity:
Warning

RetryClassification:
RetryableAfterInputCorrection
```

Recovery hints:

```text
ReduceCaptureRegion
ReduceFrameSize
AllowProviderScaling
```

---

# 36. CAP-NRM-004 CandidateFrameInvalid

Meaning:

Normalized Candidate violates Capture frame invariants.

Examples:

```text
zero width
zero height
invalid coordinate space
invalid representation length
missing required frame metadata
```

```text
Severity:
Error

RetryClassification:
RetryableTransient
```

---

# 37. CAP-NRM-005 CandidateRepresentationUnavailable

Meaning:

Capture could not create a valid temporary/immutable Candidate frame representation.

```text
Severity:
Error

RetryClassification:
RetryableTransient
```

---

# 38. CAP-NRM-006 CandidateSourceMismatch

Meaning:

Candidate no longer matches the CaptureSource/SourceVersion semantics under which it can safely be returned.

```text
Severity:
Info

RetryClassification:
RetryableAfterSourceRefresh
```

---

# 39. No `ImageTooSmallForOCR`

The v1 concept:

```text
ImageTooSmall
    because image is too small for OCR
```

is removed from Capture.

Capture does not know Recognition quality requirements.

Capture may reject only structurally invalid or configured-limit violations.

Recognition owns OCR suitability.

---

# 40. Permission Errors

## CAP-PERM-001 PermissionUnavailable

Meaning:

Required Capture permission is not currently available.

Possible normalized causes:

```text
Denied
Revoked
Restricted
NotGranted
```

```text
Severity:
Warning

RetryClassification:
RetryableAfterPermissionChange
```

---

# 41. CAP-PERM-002 PermissionRevokedDuringAcquisition

Meaning:

Permission became unavailable after acquisition began.

```text
Severity:
Warning

RetryClassification:
RetryableAfterPermissionChange
```

Capture may abort local work.

Runtime owns Attempt outcome.

---

# 42. CAP-PERM-003 PrivacyScopeViolation

Meaning:

Requested or effective acquisition would exceed authorized Capture privacy scope.

Examples:

```text
full display fallback when only region authorized
capture outside approved region
provider attempts broader source access
```

```text
Severity:
Error

RetryClassification:
NonRetryable
```

This must fail closed.

---

# 43. Capture State Errors

## CAP-STATE-001 InvalidCaptureSourceState

Meaning:

Requested CaptureSource operation is not valid in the current CaptureSource state.

Examples:

```text
capture against STOPPED source
resume source that cannot resume
replace source during terminal cleanup
```

```text
Severity:
Warning

RetryClassification:
RetryableAfterSourceRefresh
```

---

# 44. CAP-STATE-002 InvalidSourceStateTransition

Meaning:

A Capture-owned source transition violates `STATES.md`.

```text
Severity:
Critical

RetryClassification:
NonRetryable
```

This may indicate an implementation bug.

---

# 45. CAP-STATE-003 AcquisitionConcurrencyLimit

Meaning:

CaptureProvider/CaptureSource cannot safely accept another simultaneous acquisition.

This replaces the old:

```text
CaptureAlreadyRunning
```

semantics.

```text
Severity:
Info

RetryClassification:
RetryableTransient
```

Important:

This is a Capture/provider concurrency constraint.

It is not a Runtime queue state.

---

# 46. Removed `CaptureNotStarted`

The old:

```text
CaptureNotStarted
```

is removed because Capture no longer exposes a public:

```text
StartContinuousCapture
StopContinuousCapture
```

execution lifecycle.

Each `InvokeCapture` is bounded.

---

# 47. Resource Errors

## CAP-RES-001 TemporaryMemoryLimitExceeded

Meaning:

Capture-owned temporary memory usage exceeded configured budget.

Examples:

```text
raw provider buffer pressure
Candidate buffer pressure
source-local sample buffer pressure
```

```text
Severity:
Error

RetryClassification:
RetryableTransient
```

---

# 48. CAP-RES-002 TemporaryResourceUnavailable

Meaning:

Capture could not acquire a required bounded temporary resource.

Examples:

```text
shared-memory slot unavailable
temporary buffer unavailable
provider lease unavailable
```

```text
Severity:
Warning

RetryClassification:
RetryableTransient
```

---

# 49. CAP-RES-003 CandidateSizeLimitExceeded

Meaning:

Candidate representation exceeds configured Capture byte limit.

```text
Severity:
Warning

RetryClassification:
RetryableAfterInputCorrection
```

---

# 50. CAP-RES-004 SourceConcurrencyLimitExceeded

Meaning:

CaptureSource/provider-local concurrency capacity is exhausted.

```text
Severity:
Info

RetryClassification:
RetryableTransient
```

This must not be interpreted as Runtime scheduler overload.

---

# 51. Removed Generic Capture Timeout

The old:

```text
CAP-RES-002 Timeout
```

is removed.

Reason:

```text
Capture operation exceeded time limit
```

is ambiguous between:

```text
provider timeout
Runtime deadline
scheduler delay
cancellation
```

Use:

```text
CAP-ACQ-003 ProviderTimeout
```

only for actual provider-bounded timeout.

Runtime owns Runtime deadline errors.

---

# 52. Removed `TooManyRequests`

The old generic:

```text
TooManyRequests
```

is removed from Capture unless the provider itself exposes a normalized throttling condition.

Runtime owns global scheduling/rate policy.

If provider throttling is needed later, add:

```text
CAP-ACQ-xxx ProviderThrottled
```

with explicit provider semantics.

---

# 53. Internal Errors

## CAP-INT-001 InternalFailure

Meaning:

Unexpected Capture implementation failure occurred.

```text
Severity:
Critical

RetryClassification:
RetryableTransient
```

Repeated occurrence may transition Capture health to:

```text
DEGRADED
or
UNAVAILABLE
```

It does not automatically create a global Capture `FAILED` state.

---

# 54. CAP-INT-002 InvariantViolation

Meaning:

A Capture-owned invariant was violated.

Examples:

```text
Candidate marked valid without representation
READY source without required provider source
SourceVersion regression
double release of Capture-owned resource
```

```text
Severity:
Critical

RetryClassification:
NonRetryable
```

The affected operation must fail closed.

---

# 55. CAP-INT-003 TemporaryResourceLifecycleViolation

Meaning:

Capture detected invalid ownership/lifetime handling of Capture-owned temporary resources.

Examples:

```text
double release
use after release
expired Candidate lease reused
provider buffer escaped public boundary
```

```text
Severity:
Critical

RetryClassification:
NonRetryable
```

---

# 56. CAP-INT-004 ProviderContractViolation

Meaning:

CaptureProvider violated a mandatory provider contract invariant that cannot safely be treated as an ordinary provider failure.

```text
Severity:
Critical

RetryClassification:
NonRetryable
```

Provider health may become degraded/unavailable.

---

# 57. Removed `AtomicCommitFailed`

The old:

```text
AtomicCommitFailed
```

assumed Capture committed its final result.

Runtime v2 instead uses:

```text
Capture
    ↓
CandidateCaptureResult
    ↓
Runtime authority validation
    ↓
Artifact Store publication
```

Capture therefore does not own accepted Artifact commit.

Artifact publication/commit failures belong to Runtime/Artifact Store.

---

# 58. Cancellation Observation

Runtime cancellation is not a CaptureError by default.

Possible Capture completion:

```text
status = CanceledObserved
```

or local phase:

```text
ABORTED_LOCAL
```

with diagnostics such as:

```text
cancellationObserved = true
```

Capture does not create:

```text
CAP-CANCEL-xxx
```

for Runtime cancellation.

---

# 59. Runtime Supersession

If Runtime authority changes while Capture is executing:

```text
old Capture Candidate
    ↓
Runtime authority validation
    ↓
rejected
```

Capture must not report:

```text
GenerationChanged
RuntimeRevisionStale
AttemptSuperseded
```

as Capture errors.

---

# 60. Source Replacement vs Runtime Supersession

These are different:

```text
SourceVersion changed
    → Capture may report CAP-SRC-003 / CAP-SRC-007 / CAP-NRM-006

RuntimeRevision superseded
    → Runtime-owned
```

Never merge them.

---

# 61. Error vs Rejection

CaptureCompletion should distinguish:

```text
Rejected
Failed
CanceledObserved
CandidateProduced
```

Typical mapping:

```text
invalid request
    → Rejected

SourceVersion conflict
    → Rejected

privacy scope violation
    → Rejected

provider failure
    → Failed

normalization failure
    → Failed

temporary memory failure
    → Failed

Runtime cancellation observed
    → CanceledObserved
```

---

# 62. Error-to-Operation Mapping

| Condition                     | Capture Outcome                           |
| ----------------------------- | ----------------------------------------- |
| Invalid request               | `Rejected`                                |
| Unsupported capability        | `Rejected`                                |
| SourceVersion conflict        | `Rejected`                                |
| Privacy violation             | `Rejected`                                |
| Source unavailable            | `Rejected` or `Failed` depending on phase |
| Provider failure              | `Failed`                                  |
| Provider timeout              | `Failed`                                  |
| Invalid provider result       | `Failed`                                  |
| Normalization failure         | `Failed`                                  |
| Invalid Candidate             | `Rejected` or `Failed`                    |
| Temporary resource exhaustion | `Failed`                                  |
| Runtime cancellation observed | `CanceledObserved`                        |

Runtime maps CaptureCompletion to Runtime Attempt state.

---

# 63. Error-to-Source-State Mapping

Some errors may influence CaptureSource state.

Examples:

```text
PermissionUnavailable
    ↓
CaptureSource → UNAVAILABLE or SUSPENDED

ProviderUnavailable
    ↓
CaptureSource → UNAVAILABLE

Source disappeared
    ↓
CaptureSource → UNAVAILABLE

InvariantViolation
    ↓
CaptureHealth → DEGRADED / UNAVAILABLE
```

Not every operation error changes source state.

---

# 64. Error-to-Health Mapping

Potential health effects:

```text
repeated ProviderFailure
    → DEGRADED

provider unavailable
    → UNAVAILABLE

provider reconnecting
    → RECOVERING

single invalid request
    → no health change
```

Health transition policy belongs to Capture implementation/health contract.

---

# 65. No Global Failed State

Capture v2 does not require:

```text
CaptureModuleState = FAILED
```

Instead:

```text
CaptureSourceState
+
CaptureHealthState
+
CaptureOperationPhase
```

represent failure scope precisely.

A critical internal error may:

```text
fail current operation
degrade health
make one source unavailable
or trigger controlled Capture shutdown
```

depending on scope.

---

# 66. Provider Error Normalization

Provider errors must not escape directly.

Invalid:

```text
DXGI_ERROR_ACCESS_LOST
CGError
DOMException object
browser SDK exception
native HRESULT object
```

Correct:

```text
CAP-ACQ-001 ProviderUnavailable
CAP-ACQ-002 ProviderFailure
CAP-ACQ-003 ProviderTimeout
CAP-ACQ-004 InvalidProviderResult
```

Native diagnostic code may appear only in protected diagnostic metadata where policy permits.

---

# 67. Logging Rules

Safe structured logging may include:

```text
errorCode
severity
retryClassification
operationId
captureSourceId
sourceVersion
runtimeRevisionId?
workItemId?
attemptId?
providerDomain?
durationMs?
traceId?
```

---

# 68. Logging Prohibitions

Logs MUST NOT include:

```text
raw image
screenshot
pixel buffer
browser page content
OCR result
translation content
cookie
credential
token
native handle
memory address
provider object
full sensitive URL
```

---

# 69. Error Message Rules

Human-readable error messages must:

* be safe for logs;
* avoid secrets;
* avoid source content;
* avoid native object dumps;
* not be used for programmatic decisions.

Programmatic decisions use:

```text
errorCode
retryClassification
recoveryHint
```

---

# 70. Metrics

Recommended Capture-owned metrics:

```text
capture_error_total
capture_rejection_total
capture_provider_failure_total
capture_provider_timeout_total
capture_source_unavailable_total
capture_source_version_conflict_total
capture_permission_unavailable_total
capture_normalization_failure_total
capture_candidate_invalid_total
capture_temporary_resource_limit_total
capture_privacy_violation_total
```

---

# 71. Runtime Metrics Separation

Do not expose Capture-owned metrics named as though Capture owns:

```text
capture_runtime_timeout_total
capture_retry_total
capture_attempt_cancelled_total
capture_scheduler_rejected_total
```

Those belong to Runtime observability.

Capture may expose:

```text
capture_cancellation_observed_total
```

as a local observation metric.

---

# 72. Error Event Policy

Capture errors are not automatically published as Event Bus events.

Primary path:

```text
CaptureError
    ↓
CaptureCompletion
    ↓
Runtime
```

Operation-level error observability uses:

```text
logs
metrics
traces
```

Capture source/health state changes may independently publish Capture-owned fact events.

---

# 73. Example — Invalid Region

```text
CaptureInvocation
    ↓
region outside authorized scope
    ↓
CAP-REQ-004 InvalidCaptureRegion
    ↓
CaptureCompletion = Rejected
    ↓
Runtime
```

No provider resource is allocated.

---

# 74. Example — SourceVersion Conflict

```text
Request expects SourceVersion 4
Current SourceVersion = 5
        ↓
CAP-SRC-003 SourceVersionConflict
        ↓
Rejected
        ↓
Runtime may rebuild/reinvoke using current source
```

Capture does not call this a stale Runtime Attempt.

---

# 75. Example — Provider Timeout

```text
CaptureProvider acquisition
    ↓
provider-specific bounded timeout
    ↓
CAP-ACQ-003 ProviderTimeout
    ↓
CaptureCompletion = Failed
    ↓
Runtime Retry Policy
```

---

# 76. Example — Runtime Deadline

```text
Capture acquisition running
    ↓
Runtime deadline expires
    ↓
Runtime cancellation becomes observable
    ↓
Capture stops local work when possible
    ↓
CanceledObserved / ABORTED_LOCAL
    ↓
Runtime determines terminal Attempt outcome
```

No `CAP-RES-Timeout`.

---

# 77. Example — Candidate Invalid

```text
provider acquisition succeeds
    ↓
normalization completes
    ↓
Candidate validation fails
    ↓
CAP-NRM-004 CandidateFrameInvalid
    ↓
Candidate discarded
    ↓
CaptureCompletion = Failed/Rejected
```

No Artifact is published.

---

# 78. Example — Runtime Rejects Valid Candidate

```text
Capture produces valid Candidate
    ↓
CaptureCompletion
    ↓
Runtime authority check fails
    ↓
Candidate discarded
```

This is not a Capture error.

---

# 79. Example — Artifact Publication Fails

```text
Capture produces valid Candidate
    ↓
Runtime accepts authority
    ↓
Artifact Store publication fails
```

This is not:

```text
CAP-INT-003 AtomicCommitFailed
```

It belongs to Runtime/Artifact Store error ownership.

---

# 80. Example — Permission Revoked

```text
CaptureSource READY
    ↓
permission revoked
    ↓
CAP-PERM-002 PermissionRevokedDuringAcquisition
    ↓
local operation stops if required
    ↓
CaptureSource → UNAVAILABLE
    ↓
CaptureSourceUnavailable event
```

Runtime handles the Attempt separately.

---

# 81. Example — Source Replaced

```text
Capture operation uses S/v10
    ↓
source replaced with S/v11
    ↓
old provider result returns
    ↓
SourceVersion compatibility fails
    ↓
CAP-SRC-007 SourceChangedDuringAcquisition
or
CAP-NRM-006 CandidateSourceMismatch
    ↓
Candidate discarded
```

---

# 82. Deprecated v1 Errors

The following v1 semantics are removed or replaced:

```text
MissingCaptureTarget
    → MissingCaptureSource

InvalidCaptureTarget
    → InvalidCaptureSourceReference / InvalidCaptureRegion

SourceChanged
    → SourceVersionConflict / SourceChangedDuringAcquisition

EmptyFrame
    → EmptyProviderFrame

InvalidImage
    → InvalidProviderResult / CandidateFrameInvalid

UnsupportedImageFormat
    → UnsupportedFrameFormat

ImageTooLarge
    → CandidateFrameTooLarge / CandidateSizeLimitExceeded

ImageTooSmallForOCR
    → removed from Capture

DeviceUnavailable
    → ProviderUnavailable / CaptureSourceUnavailable

DeviceLost
    → CaptureSourceUnavailable / ProviderFailure

UnsupportedDevice
    → UnsupportedCaptureCapability

PermissionDenied
    → PermissionUnavailable

PermissionRevoked
    → PermissionRevokedDuringAcquisition

CaptureAlreadyRunning
    → AcquisitionConcurrencyLimit

CaptureNotStarted
    → removed

Timeout
    → ProviderTimeout only when provider-owned

TooManyRequests
    → Runtime scheduling or future ProviderThrottled

AtomicCommitFailed
    → Runtime / Artifact Store ownership
```

---

# 83. Architecture Invariants

1. Capture errors describe Capture-owned conditions only.

2. Stable error codes are used for programmatic handling.

3. Human messages are diagnostic only.

4. Capture does not own Runtime deadline errors.

5. Capture does not own Runtime cancellation errors.

6. Capture does not own Runtime retry errors.

7. Capture does not own RuntimeRevision staleness.

8. Capture does not own Attempt supersession.

9. SourceVersion conflict remains Capture-owned.

10. SourceVersion conflict is not Runtime staleness.

11. ProviderTimeout is distinct from RuntimeDeadlineExceeded.

12. Runtime cancellation observation is not a Capture failure authority.

13. Candidate validation failure is Capture-owned.

14. Runtime rejection of a valid Candidate is not a Capture error.

15. Artifact publication failure is not a Capture error.

16. Capture does not own accepted Artifact commit.

17. Capture does not expose native provider errors directly.

18. Capture classifies retryability but does not execute Runtime retry.

19. Capture failure does not directly terminate Reading Session.

20. Capture failure does not directly invoke Recognition.

21. Capture failure does not directly invoke Translation.

22. Capture errors do not automatically become Event Bus events.

23. Critical error does not require a global Capture `FAILED` state.

24. CaptureSource/Health states express failure scope.

25. Invalid request fails before expensive resource allocation where possible.

26. Privacy scope violations fail closed.

27. Raw image content never appears in errors.

28. Native handles never appear in errors.

29. Temporary resource errors remain Capture-owned.

30. Accepted Artifact resource failures remain external.

---

# 84. Related Documents

```text
doc/02-modules/capture/MODULE.md
doc/02-modules/capture/CONTRACT.md
doc/02-modules/capture/STATES.md
doc/02-modules/capture/EVENTS.md
doc/02-modules/capture/README.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/MEMORY_MODEL.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
```

---

# 85. Completion Criteria

This specification is synchronized when:

* errors are limited to Capture-owned semantics;
* SourceVersion conflicts are distinct from Runtime staleness;
* generic Capture operation timeout is removed;
* ProviderTimeout is explicitly provider-specific;
* Runtime cancellation is not represented as Capture error authority;
* Capture retry policy becomes retry classification only;
* `CaptureAlreadyRunning` is replaced by provider/source concurrency semantics;
* `CaptureNotStarted` is removed;
* OCR-specific `ImageTooSmall` is removed;
* `AtomicCommitFailed` is removed from Capture ownership;
* Candidate errors remain distinct from Artifact publication errors;
* global Capture `FAILED` state is not required;
* errors remain privacy-safe;
* provider errors are normalized;
* logging/metrics follow Runtime ownership boundaries.

---

# 86. Summary

Capture error handling is:

```text
Capture-owned condition
    ↓
CaptureError
    ↓
CaptureCompletion
    ↓
Runtime
    ↓
Attempt decision
    ↓
Runtime Retry Policy
```

Source compatibility is:

```text
CaptureSourceId
+
SourceVersion
    ↓
Capture validation
```

Execution authority is:

```text
RuntimeRevisionId
+
WorkItemId
+
AttemptId
    ↓
Runtime validation
```

They are intentionally separate.

The central rule is:

```text
Capture explains
why Capture could not produce
a valid Candidate.

Runtime decides
what happens to execution next.
```
