# Recognition Module Errors

> **Project:** CRAI
> **Module:** Recognition
> **Path:** `doc/02-modules/recognition/ERRORS.md`
> **Version:** 1.1
> **Status:** Architecture Draft
> **Related:** `MODULE.md`, `CONTRACT.md`, `STATES.md`, `EVENTS.md`

---

# 1. Purpose

Tài liệu này định nghĩa error và warning contract mà Recognition Module thực sự sở hữu.

Recognition Error Model chịu trách nhiệm:

* stable module error codes
* RecognitionModuleError
* RecognitionWarning
* RetryHint
* external error references
* Candidate assembly/validation errors
* capability/planning errors
* Recognition-local resource errors
* Recognition-owned state invariant errors
* privacy violations
* cancellation/deadline boundary
* Runtime disposition boundary
* logging and observability requirements
* compatibility
* testing
* invariants

Recognition Error Model không định nghĩa lại error semantics của:

* Image Preprocessing
* Detection
* Text Recognition
* Text Direction
* Layout
* OCR Postprocessing
* OCR Quality
* Reading Order
* Provider lifecycle
* Runtime
* Scheduler
* Work Queue
* Artifact Store
* Storage

Các errors từ những owner đó được giữ dưới dạng normalized reference.

---

# 2. Error Ownership

Recognition owns:

```text
RecognitionModuleError

RecognitionWarning

RetryHint

Candidate Assembly Errors

Candidate Validation Errors

Recognition Planning Errors

Capability Requirement Errors

Module Boundary Errors

Recognition-Local Resource Errors

Recognition State Invariant Errors

Recognition Privacy Errors
```

Recognition does not own:

```text
PreprocessingError

DetectionError

TextRecognitionError

TextDirectionError

LayoutError

OCRDocumentError

QualityError

ReadingOrderError

ProviderLifecycleError

RuntimeError

SchedulerError

QueueError

ArtifactPublicationError

StorageError
```

---

# 3. Error Architecture

```text
External / OCR Error
        ↓
Normalized Error Reference
        ↓
Recognition Module Context
        ↓
RecognitionModuleError?
        ↓
RetryHint?
        ↓
Runtime
        ↓
Runtime Error Normalization
        ↓
Retry / Fail / Cancel / Abandon
```

Không phải mọi external OCR error đều cần được đổi thành một Recognition-specific error code.

Nếu owner error đã đủ semantic information:

```text
RecognitionModuleError
    may reference
ExternalErrorRef
```

---

# 4. Error Principles

## 4.1 Stable Codes

Consumers dựa vào stable code.

Không dựa vào:

* exception class
* stack trace
* provider text
* localized message

---

## 4.2 Error vs Warning

```text
Warning
    = degraded but usable module output

ModuleError
    = module cannot produce
      contract-valid Candidate
      under current Attempt
```

---

## 4.3 Do Not Fabricate

Khi OCR output không chắc chắn:

* preserve uncertainty
* preserve external Quality Report
* add module warning if appropriate
* return RetryHint if useful

Recognition không tự sửa:

* OCR text
* Geometry
* Reading Order
* Quality

---

## 4.4 Preserve Input

Failure không mutate:

* Source Image
* Input Artifact
* OCR Artifact
* published Artifact

---

## 4.5 Runtime Owns Disposition

Recognition mô tả failure.

Runtime quyết định:

```text
FAIL
RETRY
FALLBACK
CANCEL
ABANDON
REJECT_STALE
```

---

## 4.6 Privacy

Error không chứa:

* image bytes
* full OCR text
* translated text
* Provider credential
* authorization header
* full Provider response
* sensitive temporary path

---

# 5. RecognitionModuleError

```text
RecognitionModuleError
├── ContractVersion
├── ErrorCode
├── SymbolicName
├── Category
├── Severity
├── OperationPhase
├── MessageKey
├── RetryHint?
├── ExternalErrorRef?
├── AffectedScopeRef?
├── CandidateArtifactId?
├── DiagnosticsRef?
├── Metadata?
└── OccurredAt
```

---

# 6. Error Contract Rules

1. ErrorCode stable trong cùng major version.

2. SymbolicName stable.

3. MessageKey localization-friendly.

4. Provider SDK exception không crossing boundary.

5. OCR-stage native error không bị redefine.

6. RetryHint chỉ advisory.

7. Metadata bounded.

8. Full content forbidden.

9. OperationPhase dùng phase của Recognition Module.

10. Error không chứa Runtime terminal state.

11. Error không publish Artifact.

---

# 7. Stable Error-Code Format

Canonical format:

```text
REC-<CATEGORY>-<NUMBER>
```

Ví dụ:

```text
REC-INPUT-001
REC-PLAN-001
REC-CAP-001
REC-OCR-001
REC-CAND-001
REC-RES-001
REC-STATE-001
REC-PRIV-001
REC-INT-001
```

Mỗi code map tới đúng một symbolic meaning.

---

# 8. Error Categories

| Prefix  | Category                                     |
| ------- | -------------------------------------------- |
| `INPUT` | Recognition Attempt input / module boundary  |
| `PLAN`  | Recognition planning                         |
| `CAP`   | Capability requirement resolution            |
| `OCR`   | Aggregate OCR execution/reference failure    |
| `CAND`  | Candidate assembly / validation / submission |
| `RES`   | Recognition-local resource usage             |
| `STATE` | Recognition-owned state invariant            |
| `PRIV`  | Recognition privacy boundary                 |
| `INT`   | Internal module invariant/failure            |

Removed as Recognition-owned categories:

```text
IMAGE
PREP
DETECT
REC
COORD
ORDER
PROV
```

Những semantics này thuộc OCR Architecture hoặc Provider Integration.

---

# 9. Severity

```text
RecognitionErrorSeverity
├── INFORMATION
├── WARNING
├── ERROR
└── CRITICAL
```

## INFORMATION

Diagnostic condition.

Không thường dùng làm ModuleError.

---

## WARNING

Degraded-but-usable situation.

Phần lớn trường hợp này nên dùng:

```text
RecognitionWarning
```

thay vì ModuleError.

---

## ERROR

Recognition không thể tạo valid Candidate cho Attempt hiện tại.

---

## CRITICAL

Module invariant, security hoặc privacy boundary bị vi phạm.

`CRITICAL` không tự động đưa module vào global failed state.

Runtime/Container quyết định degradation/drain/restart.

---

# 10. RetryHint

```text
RetryHint
├── Retryability
├── SuggestedStrategies[]
├── AlternativeProviderAllowed
├── AlternativeOCRProfileAllowed
├── RegionOnlyAllowed
└── ReasonCode
```

```text
Retryability
├── RETRYABLE
├── CONDITIONALLY_RETRYABLE
└── NON_RETRYABLE
```

Possible suggestions:

```text
SAME_EXECUTION_PATH

ALTERNATIVE_PROVIDER

ALTERNATIVE_OCR_PROFILE

REGION_ONLY

RESOURCE_WAIT

NO_RETRY
```

Recognition không:

* create Attempt
* choose retry delay
* consume retry budget
* select final fallback
* bypass Runtime authority

---

# 11. RecognitionWarning

```text
RecognitionWarning
├── WarningCode
├── Severity
├── OperationPhase
├── MessageKey
├── ScopeRef?
├── ExternalWarningRef?
├── ProviderId?
├── Metadata?
└── RecordedAt
```

Warnings:

* có thể coexist với valid Candidate
* không tạo terminal failure
* không trigger retry tự động
* không redefine Quality
* không mutate OCR output

---

# 12. Module-Level Warning Codes

Recommended Recognition-owned warnings:

```text
NO_READABLE_TEXT_DETECTED

PARTIAL_RECOGNITION

REMOTE_PROVIDER_USED

FALLBACK_PROVIDER_USED

OCR_RESULT_DEGRADED

QUALITY_BELOW_PREFERRED_LEVEL

OPTIONAL_READING_ORDER_UNAVAILABLE

OPTIONAL_QUALITY_REPORT_UNAVAILABLE

DIAGNOSTIC_DATA_LIMITED

OUTPUT_TRUNCATED
```

---

# 13. OCR-Owned Warnings

Các warning như:

```text
LOW_DETECTION_CONFIDENCE

LOW_RECOGNITION_CONFIDENCE

READING_ORDER_UNCERTAIN

INVALID_DIRECTION_HINT

AMBIGUOUS_LAYOUT
```

không được Recognition định nghĩa lại.

Recognition có thể surface:

```text
ExternalWarningRef
```

hoặc tạo một higher-level module warning khi cần.

---

# 14. Warning vs Quality

Recognition Warning không thay thế Quality Report.

Ví dụ:

```text
QualityReport
    Grade = Poor
```

Recognition có thể thêm:

```text
QUALITY_BELOW_PREFERRED_LEVEL
```

nhưng:

* score semantics vẫn thuộc `QUALITY.md`
* grade semantics vẫn thuộc `QUALITY.md`
* Recognition không đổi Quality state

---

# 15. No Readable Text

Canonical result:

```text
Completeness = EMPTY_VALID

Warning =
NO_READABLE_TEXT_DETECTED
```

Không phải ModuleError.

---

# 16. Partial Result

Khi Plan cho phép partial Candidate:

```text
Completeness = PARTIAL

Warning =
PARTIAL_RECOGNITION
```

Nếu partial không đáp ứng contract:

```text
ModuleError
```

mới được tạo.

---

# 17. Input Errors

## REC-INPUT-001 — RECOGNITION_INPUT_INVALID

RecognitionAttemptInput malformed hoặc internally inconsistent.

Examples:

* missing Runtime identity
* missing operation
* missing InputArtifactRef
* invalid options
* invalid PrivacyContextRef

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

cho tới khi caller sửa input.

---

## REC-INPUT-002 — RECOGNITION_ARTIFACT_UNAVAILABLE

Input Artifact không resolve/lease được.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
RESOURCE_WAIT
```

---

## REC-INPUT-003 — RECOGNITION_ARTIFACT_TYPE_UNSUPPORTED

ArtifactType không được Recognition hỗ trợ.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

---

## REC-INPUT-004 — RECOGNITION_REGION_SELECTION_INVALID

Requested RegionSelection invalid đối với input Artifact.

Recognition không định nghĩa Geometry semantics.

Validation dùng shared/OCR geometry contract.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

cho tới khi input được sửa.

---

## REC-INPUT-005 — RECOGNITION_CONTRACT_VERSION_UNSUPPORTED

Unsupported Recognition Contract major version.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

---

# 18. Planning Errors

## REC-PLAN-001 — RECOGNITION_PLAN_INVALID

RecognitionPlan không thể đạt `READY`.

Examples:

* incompatible Profile
* contradictory options
* impossible requested outputs
* unresolved OCR Profile
* invalid Configuration Snapshot

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## REC-PLAN-002 — RECOGNITION_PROFILE_UNSUPPORTED

Recognition Profile không được module hỗ trợ.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

---

## REC-PLAN-003 — RECOGNITION_EXECUTION_PATH_UNAVAILABLE

Không thể xây executable orchestration path.

Có thể do:

* OCR capability unavailable
* execution class unavailable
* Plan constraint mismatch

External causes phải giữ qua ErrorRef.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

# 19. Capability Errors

## REC-CAP-001 — RECOGNITION_CAPABILITY_UNAVAILABLE

Không có eligible OCR capability path đáp ứng requirements.

Examples:

* required language unsupported
* required script unsupported
* vertical-text support unavailable
* local-only requirement không đáp ứng

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PROVIDER
RESOURCE_WAIT
```

---

## REC-CAP-002 — RECOGNITION_CAPABILITY_REQUIREMENT_INVALID

CapabilityRequirements malformed hoặc contradictory.

Example:

```text
LocalOnly = true

RemoteAllowed = true

and policy declares them mutually exclusive
```

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

until request corrected.

---

# 20. OCR Execution Errors

Recognition không define error cho từng OCR sub-stage.

Không còn:

```text
REC-PREP-*
REC-DETECT-*
REC-REC-*
REC-COORD-*
REC-ORDER-*
```

Thay vào đó Recognition sử dụng aggregate boundary.

---

## REC-OCR-001 — RECOGNITION_OCR_EXECUTION_FAILED

Canonical OCR execution không tạo được usable OCR result/reference cho module.

```text
Recognition Module
      ↓
OCR Architecture
      ↓
External OCR Error
      ↓
REC-OCR-001
```

`ExternalErrorRef` phải giữ original semantic error.

Severity:

```text
ERROR
```

Retry:

phụ thuộc external error.

---

## REC-OCR-002 — RECOGNITION_OCR_RESULT_UNAVAILABLE

OCR execution hoàn thành nhưng required OCR artifact/reference không tồn tại.

Ví dụ:

* missing OCRDocumentRef
* required ReadingOrderResult unavailable
* required QualityReport unavailable

Nếu output là optional, dùng warning thay vì error.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## REC-OCR-003 — RECOGNITION_OCR_RESULT_INCOMPATIBLE

OCR result/reference không compatible với:

* input content identity
* current Recognition Plan
* OCR Profile
* Contract Version
* privacy partition

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

# 21. External OCR Error Reference

```text
ExternalOCRErrorRef
├── Owner
├── ErrorCode
├── ErrorContractVersion
├── Stage?
├── Retryability?
├── DiagnosticsRef?
└── Metadata?
```

Possible owners:

```text
PREPROCESS

DETECTION

RECOGNITION

TEXT_DIRECTION

LAYOUT

POSTPROCESS

QUALITY

READING_ORDER

OCR_PROVIDER
```

Recognition không đổi meaning của `ErrorCode`.

---

# 22. Provider Error Reference

```text
ProviderErrorRef
├── ProviderId
├── ProviderErrorCode
├── ProviderCategory
├── Retryability
├── SanitizedMessageKey
├── ProviderRequestId?
├── DiagnosticsRef?
└── OccurredAt
```

Rules:

1. no raw exception
2. no credentials
3. no full Provider response
4. no image/text payload
5. Provider code remains traceable
6. Provider lifecycle owner remains external

---

# 23. Candidate Errors

## REC-CAND-001 — RECOGNITION_CANDIDATE_ASSEMBLY_FAILED

Recognition không thể assemble Candidate contract.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## REC-CAND-002 — RECOGNITION_CANDIDATE_INVALID

Candidate failed module-level validation.

Examples:

* missing CandidateArtifactId
* wrong ArtifactType
* missing OCRDocumentRef
* incompatible QualityReportRef
* incompatible ReadingOrderResultRef
* missing ProviderProvenance
* invalid Completeness
* invalid CompatibilityMetadata
* Provider SDK object leak

Không dùng error này cho:

* invalid Region geometry
* invalid OCR Line hierarchy
* invalid Reading Graph internals
* invalid Quality Score internals

Những lỗi đó thuộc artifact owner.

Severity:

```text
ERROR
```

hoặc:

```text
CRITICAL
```

nếu module invariant bị phá.

---

## REC-CAND-003 — RECOGNITION_CANDIDATE_PRIVACY_VIOLATION

Candidate chứa forbidden content hoặc vi phạm Privacy Context.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

Candidate không được submit.

---

## REC-CAND-004 — RECOGNITION_CANDIDATE_SUBMISSION_FAILED

Recognition không thể submit Candidate qua Attempt Completion boundary vì local serialization/contract failure.

Không dùng cho:

* stale rejection
* authority rejection
* ownership transfer failure
* Artifact publication failure

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

# 24. Recognition-Local Resource Errors

## REC-RES-001 — RECOGNITION_RESOURCE_EXHAUSTED

Recognition không đủ Attempt-local resource trong Runtime-provided budget.

Examples:

* temporary buffer
* Candidate assembly buffer
* bounded local transformation buffer

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
RESOURCE_WAIT
REGION_ONLY
```

---

## REC-RES-002 — RECOGNITION_INPUT_LEASE_FAILED

Không acquire hoặc maintain Input Artifact Lease.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## REC-RES-003 — RECOGNITION_LOCAL_CLEANUP_FAILED

Attempt-local Recognition resource cleanup thất bại.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

cho cùng Attempt.

Resource Manager/Runtime có thể degrade/drain component.

---

# 25. State Errors

## REC-STATE-001 — RECOGNITION_STATE_INVARIANT_VIOLATION

Recognition-owned state transition vi phạm `STATES.md`.

Applies to:

* Availability
* Plan
* Operation Phase
* Candidate Validation

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

for same Attempt.

---

## REC-STATE-002 — RECOGNITION_DUPLICATE_CANDIDATE_SUBMISSION

Một Candidate được submit hơn một lần từ cùng Recognition execution.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

---

# 26. Privacy Errors

## REC-PRIV-001 — RECOGNITION_PRIVACY_CONFLICT

Recognition Plan yêu cầu execution path vi phạm PrivacyContext.

Examples:

* LocalOnly nhưng chỉ remote path có sẵn
* protected diagnostics không authorized
* persistence required trong EPHEMERAL mode

Severity:

```text
ERROR
```

hoặc `CRITICAL` khi implementation cố bypass policy.

Retry:

```text
NON_RETRYABLE
```

until Plan/policy changes.

---

## REC-PRIV-002 — RECOGNITION_CONTENT_EXPOSURE_DETECTED

Recognition phát hiện dữ liệu forbidden sắp crossing module/log/event boundary.

Examples:

* raw OCR text in event
* raw image in diagnostics metadata
* Provider credential in Candidate
* raw Provider response in error

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

---

# 27. Internal Errors

## REC-INT-001 — RECOGNITION_INTERNAL_ERROR

Unexpected Recognition Module implementation failure.

Severity:

```text
CRITICAL
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Runtime decides.

---

## REC-INT-002 — RECOGNITION_INVARIANT_VIOLATION

Module architecture invariant bị phá.

Examples:

* Candidate mutated after VALID
* Runtime authority assumed by module
* OCR contract redefined locally
* Provider SDK object crosses module boundary
* Recognition directly invokes downstream Translation
* local-only path sends data remote

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

until implementation/configuration corrected.

---

## REC-INT-003 — RECOGNITION_REFERENCE_NORMALIZATION_FAILED

Module không thể normalize external artifact/error/reference vào Recognition contract.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

# 28. Errors Not Owned by Recognition

Không tạo Recognition-specific code cho:

```text
QueueOverflow

SchedulerAdmissionRejected

WorkItemCanceled

AttemptAbandoned

RuntimeDeadlineExpired

ProviderInitializationFailed

ProviderHealthUnavailable

ProviderModelLoadFailed

GPUCapacityUnavailable

OCRDetectionInvalidGeometry

OCRRecognitionInvalidStructure

OCRReadingOrderCycle

OCRQualityAssessmentFailed

ArtifactOwnershipTransferFailed

ArtifactPublicationFailed

CacheEvictionFailed

StorageWriteFailed

ApplicationShutdownFailed
```

Recognition chỉ reference canonical owner error.

---

# 29. Removed Legacy Recognition Error Categories

Removed:

```text
PREP
DETECT
REC
COORD
ORDER
PROV
```

Reason:

```text
OCR Architecture / Provider Integration
now owns those semantics.
```

Examples:

```text
RECOGNITION_DETECTION_FAILED
    → Detection error reference
      wrapped by REC-OCR-001 when needed

RECOGNITION_TEXT_FAILED
    → OCR Recognition error reference

RECOGNITION_COORDINATE_MAPPING_FAILED
    → Detection/Postprocessing error reference

RECOGNITION_READING_ORDER_FAILED
    → Reading Order error reference

RECOGNITION_PROVIDER_OUTPUT_INVALID
    → Provider/OCR Adapter error reference
```

---

# 30. Removed Legacy Warning Ownership

Recognition no longer authoritatively defines:

```text
LOW_DETECTION_CONFIDENCE

LOW_RECOGNITION_CONFIDENCE

READING_ORDER_UNCERTAIN

REGION_GEOMETRY_INFERRED

LINE_GEOMETRY_UNAVAILABLE

OVERLAPPING_REGIONS_SUPPRESSED

DUPLICATE_REGION_SUPPRESSED

MIXED_ORIENTATION_DETECTED
```

Những warnings/signals thuộc OCR owner tương ứng.

Recognition có thể surface chúng through references.

---

# 31. Cancellation Boundary

Cancellation không phải RecognitionModuleError mặc định.

Recognition observe:

```text
CancellationContext
```

rồi:

* stop new expensive work
* request provider cancellation if supported
* cleanup local resources
* avoid Candidate submission
* return cancellation observation

Runtime decides:

```text
ATTEMPT_CANCELED
ATTEMPT_ABANDONED
ATTEMPT_FAILED
```

---

# 32. Deadline Boundary

Deadline thuộc Runtime Execution Context.

Recognition có thể observe:

```text
deadline exceeded
```

hoặc receive external Provider timeout error.

Recognition không:

* own global timeout
* cancel WorkItem lineage
* decide retry
* convert every deadline into module error

---

# 33. Error to Runtime Disposition

```text
RecognitionModuleError
      ↓
Runtime Error Normalization
      ↓
Retry Policy
Cancellation Policy
Authority Validation
      ↓
Attempt Disposition
```

Possible external dispositions:

```text
ATTEMPT_FAILED

ATTEMPT_CANCELED

ATTEMPT_ABANDONED

RETRY_SCHEDULED

FALLBACK_ATTEMPT_CREATED

REJECTED_STALE
```

Recognition không map trực tiếp error → terminal Attempt state.

---

# 34. Error and Candidate Relationship

```text
Valid Candidate
    → no ModuleError
    → warnings allowed
```

```text
Invalid Candidate
    → ModuleError
    → Candidate not submitted
```

```text
Cancellation before Candidate
    → no Candidate
    → Runtime decides outcome
```

```text
Valid Candidate rejected stale
    → no RecognitionModuleError
```

---

# 35. Quality Relationship

Poor Quality không tự động là Recognition failure.

```text
QualityReport Grade = Poor
        ↓
Recognition Plan / Runtime Policy
        ↓
may still allow Candidate
```

Nếu required quality gate không đạt và Candidate policy không cho phép:

```text
REC-OCR-002
or
REC-OCR-003
```

có thể được dùng ở module boundary.

Recognition không redefine Quality error codes.

---

# 36. Reading Order Relationship

Reading Order uncertainty không tự động là Recognition error.

Nếu Reading Order optional:

```text
Warning =
OPTIONAL_READING_ORDER_UNAVAILABLE
```

Nếu Plan yêu cầu valid ReadingOrderResult và result không tồn tại:

```text
REC-OCR-002
```

Original Reading Order failure vẫn giữ qua ExternalOCRErrorRef.

---

# 37. Logging Contract

Safe structured fields:

```text
ErrorCode
SymbolicName
Category
Severity
OperationPhase
ApplicationInstanceId
SessionId?
RevisionId
WorkItemId
AttemptId
InputArtifactId?
CandidateArtifactId?
ProviderId?
ExternalErrorCode?
Retryability
TraceId
OccurredAt
```

Forbidden:

```text
image_bytes

image_base64

recognized_text

translated_text

browser_content

credentials

authorization_header

provider_full_response

sensitive_file_path
```

---

# 38. Metrics

Recognition-owned:

```text
recognition.error.total

recognition.error.by_code

recognition.error.by_category

recognition.error.by_phase

recognition.error.critical_total

recognition.retry_hint.total

recognition.warning.total

recognition.warning.by_code

recognition.empty_valid.total

recognition.partial.total

recognition.candidate_invalid_total

recognition.ocr_execution_failure_total

recognition.invariant_violation_total

recognition.cleanup_failure_total
```

Not Recognition-owned:

```text
detection_error_total

recognition_confidence_distribution

reading_order_failure_total

quality_grade_distribution

provider_health_state

queue_overflow_total

scheduler_rejection_total

artifact_publication_failure_total

runtime_deadline_total
```

---

# 39. Error Observability

Mọi RecognitionModuleError nên traceable tới:

```text
RevisionId

WorkItemId

AttemptId

OperationPhase

ConfigurationSnapshotId

ProviderId?

TraceId
```

High-cardinality IDs ở trace/log, không mặc định dùng metric labels.

---

# 40. Privacy

Error metadata content-free by default.

Protected diagnostics chỉ thông qua:

```text
DiagnosticsRef
```

và yêu cầu:

* explicit diagnostic mode
* authorization
* secure Artifact
* bounded retention
* redaction
* auditability

---

# 41. Error Contract Evolution

Backward-compatible:

* add new code
* add optional Metadata
* add warning code
* add RetryStrategy
* clarification without semantic change

Breaking:

* change existing code meaning
* materially change severity
* materially change retryability
* rename/remove stable code
* change privacy guarantee
* change ownership boundary

Requires major version.

---

# 42. Unknown Codes

Consumer phải:

* preserve original code
* use category if understood
* tolerate unknown additive values
* not crash
* reject unsupported major contract version

---

# 43. Testing Requirements — Contract

Verify:

* every code unique
* one symbolic name per code
* category matches prefix
* valid Severity
* valid RetryHint
* MessageKey present
* no Runtime state embedded
* no OCR semantic error redefinition

---

# 44. Testing Requirements — Warning vs Error

Test:

```text
no readable text
    → EMPTY_VALID + warning
```

```text
poor quality but usable
    → warning / QualityReport
```

```text
optional ReadingOrder missing
    → warning
```

```text
required OCR result missing
    → ModuleError
```

```text
invalid Candidate
    → ModuleError
```

---

# 45. Testing Requirements — Ownership

Verify:

* Detection error remains Detection-owned
* ReadingOrder error remains ReadingOrder-owned
* Quality error remains Quality-owned
* Provider lifecycle error remains Provider-owned
* Runtime deadline remains Runtime-owned
* Queue error remains Queue-owned
* Artifact publication failure remains Artifact Store-owned
* stale rejection is not Recognition failure

---

# 46. Testing Requirements — Input and Plan

Test:

* missing ArtifactRef
* unsupported ArtifactType
* invalid RegionSelection
* unsupported major version
* invalid Recognition Profile
* invalid CapabilityRequirements
* Privacy conflict
* no executable Plan

---

# 47. Testing Requirements — Candidate

Test:

* missing OCRDocumentRef
* incompatible QualityReportRef
* incompatible ReadingOrderResultRef
* missing ProviderProvenance
* invalid Completeness
* missing CompatibilityMetadata
* SDK leakage
* privacy leakage
* duplicate Candidate submission

---

# 48. Testing Requirements — Runtime Integration

Test:

* RetryHint evaluated externally
* new Attempt receives new AttemptId
* cancellation does not create module terminal error
* valid Candidate may be rejected stale
* critical invariant may cause external module degradation
* Recognition never schedules retry itself

---

# 49. Error Invariants

1. Every Recognition-owned failure has stable ErrorCode.

2. Every code maps to one symbolic meaning.

3. OCR stage errors retain their original owner.

4. Provider SDK exceptions never cross Recognition boundary.

5. Raw image never appears in error.

6. Full OCR text never appears in error.

7. Credentials never appear.

8. Warning differs from ModuleError.

9. No-text is not error.

10. Poor Quality is not automatically error.

11. Reading Order uncertainty is not automatically error.

12. Recognition does not own Detection errors.

13. Recognition does not own Text Recognition errors.

14. Recognition does not own Direction errors.

15. Recognition does not own Layout errors.

16. Recognition does not own Postprocessing errors.

17. Recognition does not own Quality errors.

18. Recognition does not own Reading Order errors.

19. Recognition does not own Provider lifecycle errors.

20. Recognition does not own Runtime errors.

21. Recognition does not own Artifact publication errors.

22. Recognition does not own Storage errors.

23. Recognition never decides retry.

24. Recognition never creates retry Attempt.

25. Cancellation terminal outcome belongs to Runtime.

26. Stale rejection is not Recognition error.

27. Candidate validation failure prevents valid submission.

28. Publication failure is not Candidate validation failure.

29. Input Artifact remains immutable after failure.

30. Attempt-local resources are cleaned up after failure.

31. RetryHint is advisory.

32. ExternalErrorRef preserves owner semantics.

33. ProviderErrorRef is sanitized.

34. Metadata remains bounded.

35. OperationPhase uses Recognition module phases only.

36. Error contract is versioned.

37. Unknown codes are handled safely.

38. Privacy violations are explicit.

39. Invalid Candidate is never submitted as valid.

40. Error handling preserves traceability.

---

# 50. MVP Error Set

Required MVP errors:

```text
REC-INPUT-001
RECOGNITION_INPUT_INVALID

REC-INPUT-002
RECOGNITION_ARTIFACT_UNAVAILABLE

REC-INPUT-003
RECOGNITION_ARTIFACT_TYPE_UNSUPPORTED

REC-INPUT-004
RECOGNITION_REGION_SELECTION_INVALID

REC-INPUT-005
RECOGNITION_CONTRACT_VERSION_UNSUPPORTED


REC-PLAN-001
RECOGNITION_PLAN_INVALID

REC-PLAN-003
RECOGNITION_EXECUTION_PATH_UNAVAILABLE


REC-CAP-001
RECOGNITION_CAPABILITY_UNAVAILABLE


REC-OCR-001
RECOGNITION_OCR_EXECUTION_FAILED

REC-OCR-002
RECOGNITION_OCR_RESULT_UNAVAILABLE


REC-CAND-001
RECOGNITION_CANDIDATE_ASSEMBLY_FAILED

REC-CAND-002
RECOGNITION_CANDIDATE_INVALID

REC-CAND-003
RECOGNITION_CANDIDATE_PRIVACY_VIOLATION


REC-RES-001
RECOGNITION_RESOURCE_EXHAUSTED

REC-RES-002
RECOGNITION_INPUT_LEASE_FAILED


REC-STATE-001
RECOGNITION_STATE_INVARIANT_VIOLATION


REC-PRIV-001
RECOGNITION_PRIVACY_CONFLICT


REC-INT-001
RECOGNITION_INTERNAL_ERROR

REC-INT-002
RECOGNITION_INVARIANT_VIOLATION
```

---

# 51. MVP Warning Set

Required:

```text
NO_READABLE_TEXT_DETECTED

PARTIAL_RECOGNITION

REMOTE_PROVIDER_USED

FALLBACK_PROVIDER_USED

OCR_RESULT_DEGRADED

QUALITY_BELOW_PREFERRED_LEVEL
```

Optional:

```text
OPTIONAL_READING_ORDER_UNAVAILABLE

OPTIONAL_QUALITY_REPORT_UNAVAILABLE

OUTPUT_TRUNCATED
```

OCR-stage warning codes remain owned by OCR Architecture.

---

# 52. Completion Criteria

Tài liệu hoàn chỉnh khi:

* mọi Recognition-owned module failure có stable code
* OCR-stage failure không bị định nghĩa lại
* warning/error tách biệt
* RetryHint advisory
* ExternalErrorRef giữ source ownership
* Provider errors sanitized
* Runtime boundary explicit
* Candidate validation errors explicit
* stale/cancellation/publication externalized
* privacy enforced
* backward compatibility defined
* tests cover ownership boundaries

---

# 53. Related Documents

```text
doc/02-modules/recognition/README.md
doc/02-modules/recognition/MODULE.md
doc/02-modules/recognition/CONTRACT.md
doc/02-modules/recognition/STATES.md
doc/02-modules/recognition/EVENTS.md

doc/01-architecture/OWNERSHIP_MAP.md

doc/01-architecture/ocr/PIPELINE.md
doc/01-architecture/ocr/PREPROCESS.md
doc/01-architecture/ocr/DETECTION.md
doc/01-architecture/ocr/RECOGNITION.md
doc/01-architecture/ocr/TEXT_DIRECTION.md
doc/01-architecture/ocr/LAYOUT.md
doc/01-architecture/ocr/POSTPROCESS.md
doc/01-architecture/ocr/QUALITY.md
doc/01-architecture/ocr/READING_ORDER.md
doc/01-architecture/ocr/PROVIDERS.md

doc/01-architecture/runtime/ERROR_MODEL.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
```

---

# 54. Summary

Recognition error flow:

```text
OCR / External Component Error
        ↓
ExternalErrorRef
        ↓
Recognition Module Context
        ↓
RecognitionModuleError
        ↓
RetryHint
        ↓
Runtime
        ↓
Runtime Disposition
```

Recognition owns:

```text
Module Boundary Errors

Plan Errors

Capability Requirement Errors

Aggregate OCR Execution Errors

Candidate Errors

Recognition-Local Resource Errors

Recognition State Errors

Recognition Privacy Errors

Warnings

Retry Hints
```

OCR Architecture owns:

```text
Preprocessing Errors

Detection Errors

Recognition-stage Errors

Direction Errors

Layout Errors

Postprocessing Errors

Quality Errors

Reading Order Errors
```

Runtime owns:

```text
Retry Execution

Cancellation Outcome

Deadline Outcome

WorkItem / Attempt Outcome

Authority
```

Artifact Store owns:

```text
Ownership Transfer Errors

Publication Errors
```

Provider Integration owns:

```text
Provider-native Error Mapping

Provider Protocol Errors

Provider Lifecycle Errors
```

Core rule:

```text
The owner that defines a semantic operation
also defines its semantic failure.

Recognition only owns failures
at the Recognition Module boundary.

Runtime decides what happens next.
```
