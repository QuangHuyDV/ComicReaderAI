# Recognition Module Events

> **Project:** CRAI
> **Module:** Recognition
> **Path:** `02-modules/recognition/EVENTS.md`
> **Version:** 1.1
> **Status:** Architecture Draft
> **Related:** `MODULE.md`, `CONTRACT.md`, `STATES.md`, `01-architecture/ocr/`

---

# 1. Purpose

Tài liệu này định nghĩa event contract mà Recognition Module thực sự sở hữu.

Recognition events dùng để truyền các **facts đã xảy ra** tại module boundary.

Chúng hỗ trợ:

* diagnostics
* observability
* optional progress
* warnings
* module errors
* Candidate traceability
* cancellation/deadline observations

Recognition events không định nghĩa:

* OCR stage semantics
* Runtime lifecycle
* Attempt terminal outcome
* retry execution
* cancellation authority
* Artifact publication
* Provider lifecycle
* Quality semantics
* Reading Order semantics
* downstream orchestration

Command/data contract nằm trong:

```text
02-modules/recognition/CONTRACT.md
```

State ownership nằm trong:

```text
02-modules/recognition/STATES.md
```

OCR semantic ownership nằm trong:

```text
01-architecture/ocr/
```

---

# 2. Event Role of Recognition

Recognition được Runtime invoke trong một Attempt.

```text
Runtime Attempt
      ↓
Recognition Module
      ↓
Optional Recognition Facts
      ↓
Candidate Submitted to Runtime
```

Recognition events:

* describe facts already observed/produced
* may support diagnostics and observability
* are optional for correctness
* never grant authority
* never publish authoritative Artifact
* never replace Attempt Completion
* never create hidden business orchestration

MVP correctness không phụ thuộc Recognition-specific events.

---

# 3. Event Ownership

Recognition owns:

```text
Recognition Plan Facts
Recognition Execution Facts
Recognition Candidate Facts
Recognition Warning Facts
Recognition Module Error Facts
Recognition Cancellation / Deadline Observations
Recognition Diagnostic Facts
```

Recognition does not own:

```text
ATTEMPT_STARTED
ATTEMPT_COMPLETED
ATTEMPT_FAILED
ATTEMPT_CANCELED
ATTEMPT_ABANDONED

AUTHORITY_ACCEPTED
AUTHORITY_REJECTED

ARTIFACT_PUBLISHED
ARTIFACT_PUBLICATION_REJECTED

PROVIDER_READY
PROVIDER_DEGRADED
PROVIDER_UNAVAILABLE

QUALITY_REPORT_CREATED
READING_ORDER_RESOLVED
```

Các facts trên thuộc owner tương ứng.

---

# 4. OCR Event Boundary

Recognition Module không redefine events cho từng OCR stage.

Không publish dưới namespace Recognition:

```text
RECOGNITION_PREPARATION_COMPLETED
RECOGNITION_REGIONS_DETECTED
RECOGNITION_REGION_PROCESSED
RECOGNITION_PROVIDER_OUTPUT_NORMALIZED
RECOGNITION_READING_ORDER_RESOLVED
RECOGNITION_QUALITY_EVALUATED
```

Nếu cần stage-level tracing, sử dụng:

```text
OCR diagnostics / telemetry
```

theo authoritative OCR Architecture.

Recognition chỉ cần biết:

```text
OCR execution started / completed / failed
```

ở mức module orchestration.

---

# 5. Event Design Principles

## 5.1 Facts, Not Commands

Correct:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_OCR_EXECUTION_COMPLETED
RECOGNITION_CANDIDATE_VALIDATED
```

Avoid:

```text
RECOGNITION_CREATE_PLAN
RECOGNITION_RUN_OCR
RECOGNITION_VALIDATE_CANDIDATE
```

---

## 5.2 Optional for Correctness

Nếu Event Bus unavailable:

* Recognition vẫn execute
* Candidate vẫn submit qua Attempt Completion
* Runtime correctness không thay đổi

---

## 5.3 Immutable

Published event immutable.

Correction cần event mới.

---

## 5.4 Duplicate Safe

Mỗi event có `EventId`.

Consumer deduplicate theo `EventId`.

---

## 5.5 No Global Ordering

Không giả định global event order.

Chỉ causal/best-effort order theo Attempt/partition.

---

## 5.6 No Authority

Recognition event không chứng minh:

* Revision còn current
* Attempt accepted
* Candidate published
* downstream work được phép chạy

---

## 5.7 Reference-Only Large Data

Không truyền qua Event Bus:

* image bytes
* OCR Document payload
* Candidate payload
* Recognition Artifact payload
* Quality Report payload
* Reading Order graph
* raw Provider response
* full recognized text

Dùng reference khi thật sự cần.

---

## 5.8 Privacy by Contract

Default event content:

```text
OPERATIONAL_METADATA
```

---

# 6. Event Naming

Canonical name:

```text
UPPER_SNAKE_CASE
```

Pattern:

```text
RECOGNITION_<FACT>
```

Ví dụ:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_OCR_EXECUTION_COMPLETED
RECOGNITION_CANDIDATE_VALIDATED
```

Transport topic có thể map thành:

```text
recognition.plan_created
```

nhưng canonical identity vẫn là architectural event name.

---

# 7. Event Categories

```text
Recognition Events
├── Domain Facts
├── Execution Facts
├── Warning / Error Facts
└── Diagnostic Facts
```

Không tồn tại Recognition-owned terminal lifecycle category.

---

# 8. Shared Event Envelope

Recognition sử dụng Event Bus envelope chuẩn.

Conceptually:

```text
RecognitionEventEnvelope<T>
├── EventId
├── EventName
├── ContractVersion
├── ProducerModule
├── OccurredAt
├── TraceContext
├── RuntimeContext
├── PrivacyClassification
├── PartitionKey?
├── SequenceNumber?
└── Payload
```

Envelope semantics thuộc Event Bus Architecture.

Recognition không redefine:

* delivery guarantee
* transport retry
* subscriber behavior
* retention

---

# 9. Contract Version

Initial revised Recognition event contract:

```text
1.1.0
```

Consumers reject unsupported major version.

Unknown minor/additive fields phải được xử lý an toàn.

---

# 10. Producer Module

```text
ProducerModule = recognition
```

Provider Adapter không publish public Recognition facts dưới provider-specific namespace.

---

# 11. Trace Context

```text
TraceContext
├── TraceId
├── ParentSpanId?
├── CorrelationId?
└── Baggage?
```

Baggage không chứa:

* image
* recognized text
* credentials
* provider request body
* sensitive path

---

# 12. Recognition Event Runtime Context

```text
RecognitionEventContext
├── ApplicationInstanceId
├── SessionId?
├── RevisionId
├── WorkItemId
├── AttemptId
├── CandidateArtifactId?
├── InputArtifactId?
├── ProviderId?
├── ConfigurationSnapshotId
├── RecognitionOperation
├── RecognitionProfile
├── OperationPhase?
└── BusinessStageId?
```

Rules:

1. `RevisionId`, `WorkItemId`, `AttemptId` required.
2. `SessionId` optional.
3. `CandidateArtifactId` chỉ sau khi Candidate tồn tại.
4. Published `ArtifactId` không được Recognition tự thêm trước publication.
5. ProviderId optional.
6. Retry count không embedded.
7. Priority/Queue class không embedded.
8. Runtime context chỉ phục vụ traceability.

---

# 13. Partition Key

Recommended:

```text
AttemptId
```

cho Attempt-local facts.

Alternative:

```text
RevisionId
```

cho Revision-level diagnostics.

Selection tuân Event Convention.

---

# 14. Sequence Number

Optional monotonic sequence per partition.

Ví dụ:

```text
RECOGNITION_PLAN_CREATED                  1
RECOGNITION_OCR_EXECUTION_COMPLETED       2
RECOGNITION_CANDIDATE_VALIDATED           3
RECOGNITION_CANDIDATE_SUBMITTED           4
```

Consumer vẫn phải chịu được:

* missing optional fact
* delayed fact
* duplicate
* out-of-order delivery

---

# 15. Canonical Domain Facts

Core Recognition domain facts:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_OCR_EXECUTION_COMPLETED
RECOGNITION_CANDIDATE_VALIDATED
RECOGNITION_CANDIDATE_SUBMITTED
```

Đây là semantic milestones ở **module boundary**.

---

# 16. RECOGNITION_PLAN_CREATED

## Meaning

Một immutable `RecognitionPlan` hợp lệ đã được tạo.

State relation:

```text
RecognitionPlanState = READY
```

---

## Payload

```text
RecognitionPlanCreatedEvent
├── PlanId
├── RecognitionOperation
├── RecognitionProfile
├── OCRProfileRef?
├── CapabilityRequirementSummary
├── ExecutionStrategy
├── ConfigurationSnapshotId
├── PrivacyMode
├── ExecutionClasses[]
├── EstimatedResourceCost?
└── CreatedAt
```

---

## ExecutionStrategy

Có thể biểu diễn:

```text
COMBINED_OCR
COMPOSED_OCR
SINGLE_REGION
DIAGNOSTIC
```

Đây là module orchestration strategy.

Không encode chi tiết:

* Detection model
* Reading Order policy
* Quality policy
* coordinate algorithm
* preprocessing algorithm

---

## Rules

1. emit sau Plan READY
2. optional for correctness
3. no credentials
4. no image payload
5. no provider SDK object
6. provider selection có thể chưa final
7. không đồng nghĩa OCR execution started

---

# 17. RECOGNITION_OCR_EXECUTION_COMPLETED

## Meaning

Recognition Module đã hoàn tất việc điều phối OCR Architecture cho Attempt hiện tại và có OCR-level result/reference để tiếp tục Candidate assembly.

Nó không có nghĩa:

* Candidate valid
* Attempt successful
* Artifact published

---

## Payload

```text
RecognitionOCRExecutionCompletedEvent
├── OCRDocumentRef?
├── ReadingOrderResultRef?
├── QualityReportRef?
├── ProviderProvenanceSummary
├── Completeness
├── WarningCount
├── ExecutionDurationMs
└── CompletedAt
```

---

## Rules

1. không embed OCR Document
2. không embed Reading Order graph
3. không embed Quality Report
4. output refs optional theo Plan
5. no full OCR text
6. no Region/Line/Character dump
7. event is informational
8. Candidate assembly may still fail

---

# 18. Why No OCR Stage Events Here

Recognition no longer publishes events like:

```text
REGIONS_DETECTED
TEXT_RECOGNIZED
DIRECTION_ANALYZED
LAYOUT_RESOLVED
QUALITY_EVALUATED
READING_ORDER_RESOLVED
```

vì các concept này có owner riêng.

Nếu một concrete consumer thật sự cần stage diagnostics:

```text
OCR Architecture
    → defines semantic diagnostic fact

Telemetry/Event infrastructure
    → transports it
```

Recognition Module không trở thành second owner.

---

# 19. RECOGNITION_CANDIDATE_VALIDATED

## Meaning

Candidate đã vượt Recognition module-level validation.

State:

```text
CandidateValidationState = VALID
```

---

## Payload

```text
RecognitionCandidateValidatedEvent
├── CandidateArtifactId
├── ArtifactType
├── OCRDocumentRef
├── ReadingOrderResultRef?
├── QualityReportRef?
├── Completeness
├── WarningCount
├── ProviderProvenanceSummary
├── CompatibilityMetadataVersion
├── ValidationDurationMs
└── ValidatedAt
```

---

## Rules

1. no Candidate payload
2. no OCR payload
3. no full recognized text
4. Candidate vẫn non-authoritative
5. event không transfer ownership
6. event không trigger Text Processing
7. Runtime có thể reject Candidate
8. EMPTY_VALID allowed
9. Quality may be poor while Candidate is contract-valid

---

# 20. RECOGNITION_CANDIDATE_SUBMITTED

## Meaning

Recognition đã submit Candidate cho Runtime qua Attempt Completion path.

---

## Payload

```text
RecognitionCandidateSubmittedEvent
├── CandidateArtifactId
├── ArtifactType
├── SubmissionMode
├── SubmittedAt
└── DiagnosticsRef?
```

```text
SubmissionMode
├── ATTEMPT_COMPLETION
└── DIAGNOSTIC_EVALUATION
```

---

## Rules

1. submission ≠ publication
2. no published ArtifactId
3. no downstream trigger
4. Candidate immutable after submission
5. Runtime disposition may accept/reject
6. event optional

---

# 21. Optional Execution Progress Fact

Recognition MAY expose one coarse-grained progress event:

```text
RECOGNITION_PHASE_CHANGED
```

chỉ khi có concrete consumer.

---

# 22. RECOGNITION_PHASE_CHANGED

## Payload

```text
RecognitionPhaseChangedEvent
├── PreviousPhase
├── CurrentPhase
├── PhaseStartedAt
└── ProgressHint?
```

Allowed module phases:

```text
VALIDATING
PLANNING
ACQUIRING_INPUT
EXECUTING_OCR
ASSEMBLING_CANDIDATE
VALIDATING_CANDIDATE
FINALIZING
FINISHED
```

Không expose internal OCR sub-stages qua Recognition phase model.

---

## Rules

* optional
* sampleable
* not authoritative
* not persisted for correctness
* no payload content
* consumers tolerate missing transitions

---

# 23. Warning and Error Facts

Recognition owns:

```text
RECOGNITION_WARNING_RECORDED
RECOGNITION_MODULE_ERROR_RECORDED
RECOGNITION_CANCELLATION_OBSERVED
RECOGNITION_DEADLINE_OBSERVED
```

Không event nào trong nhóm này định nghĩa Runtime terminal outcome.

---

# 24. RECOGNITION_WARNING_RECORDED

## Payload

```text
RecognitionWarningRecordedEvent
├── WarningCode
├── Severity
├── OperationPhase
├── ScopeRef?
├── ProviderId?
├── Metadata?
└── RecordedAt
```

Rules:

* no full text
* no raw provider response
* warning ≠ failure
* OCR-stage warning có thể được reference, không redefine semantics

---

# 25. RECOGNITION_MODULE_ERROR_RECORDED

## Meaning

Recognition tạo normalized module-level error.

---

## Payload

```text
RecognitionModuleErrorRecordedEvent
├── ErrorCode
├── OperationPhase
├── Retryability
├── SuggestedStrategies[]
├── ProviderErrorRef?
├── ProviderId?
├── DiagnosticsRef?
└── RecordedAt
```

---

## Rules

1. not terminal Runtime event
2. Runtime may retry/fail/cancel/abandon
3. no credential
4. no provider raw response
5. no full recognized content
6. no `RECOGNITION_FAILED` terminal event

---

# 26. RECOGNITION_CANCELLATION_OBSERVED

## Payload

```text
RecognitionCancellationObservedEvent
├── OperationPhase
├── ProviderCancellationRequested
├── ProviderCancellationSupported
├── LocalWorkStopped
├── ObservedAt
└── Metadata?
```

Meaning:

```text
Recognition observed CancellationContext.
```

Không có nghĩa:

```text
ATTEMPT_CANCELED
```

---

# 27. RECOGNITION_DEADLINE_OBSERVED

## Payload

```text
RecognitionDeadlineObservedEvent
├── OperationPhase
├── DeadlineSource
├── RemainingBudgetMs?
├── Expired
├── ProviderTimeoutObserved
└── ObservedAt
```

Runtime owns timeout/terminal disposition.

---

# 28. Diagnostic Facts

Allowed optional diagnostics:

```text
RECOGNITION_DIAGNOSTIC_RECORDED
RECOGNITION_BENCHMARK_SAMPLE_RECORDED
RECOGNITION_INVARIANT_VIOLATION_RECORDED
```

Requirements:

* bounded
* protected when necessary
* reference-oriented
* not workflow input
* not authoritative
* appropriate retention

---

# 29. Candidate Event Boundary

Recognition Candidate facts có thể mô tả:

```text
Candidate assembled
Candidate validated
Candidate submitted
```

Recognition không mô tả:

```text
Candidate accepted
Ownership transferred
Artifact published
Artifact available
Artifact retained
Artifact evicted
```

Những facts này thuộc Runtime/Artifact Store/Resource owner.

---

# 30. Runtime Event Boundary

Runtime owns:

```text
WORKITEM_CREATED

ATTEMPT_STARTED
ATTEMPT_COMPLETED
ATTEMPT_FAILED
ATTEMPT_CANCELED
ATTEMPT_ABANDONED

AUTHORITY_ACCEPTED
AUTHORITY_REJECTED
LATE_COMPLETION_REJECTED
```

Recognition không tạo alias terminal event như:

```text
RECOGNITION_COMPLETED
RECOGNITION_FAILED
RECOGNITION_CANCELED
```

---

# 31. Artifact Store Event Boundary

Artifact Store owns:

```text
OWNERSHIP_TRANSFER_REQUESTED
OWNERSHIP_TRANSFERRED
ARTIFACT_PUBLISHED
ARTIFACT_PUBLICATION_REJECTED
```

Resource lifecycle events thuộc Resource owner tương ứng.

Text Processing bắt đầu từ Runtime-orchestrated published Artifact availability.

Không bắt đầu từ Recognition Candidate event.

---

# 32. Provider Manager Event Boundary

Provider Manager owns:

```text
PROVIDER_REGISTERED
PROVIDER_READY
PROVIDER_DEGRADED
PROVIDER_UNAVAILABLE
PROVIDER_CONFIGURATION_CHANGED
PROVIDER_DRAINING
PROVIDER_STOPPED
```

Recognition không publish:

```text
RECOGNITION_PROVIDER_READY
RECOGNITION_PROVIDER_UNAVAILABLE
```

---

# 33. OCR Quality Event Boundary

Quality semantics thuộc:

```text
01-architecture/ocr/QUALITY.md
```

Recognition không publish:

```text
RECOGNITION_QUALITY_EVALUATED
```

Nếu architecture cần một semantic event, owner phải là Quality concern, ví dụ conceptually:

```text
OCR_QUALITY_ASSESSED
```

Recognition Candidate event chỉ reference `QualityReportRef`.

---

# 34. Reading Order Event Boundary

Reading Order semantics thuộc:

```text
01-architecture/ocr/READING_ORDER.md
```

Recognition không publish:

```text
RECOGNITION_READING_ORDER_RESOLVED
```

Candidate/OCR execution fact có thể reference:

```text
ReadingOrderResultRef
```

nếu result tồn tại.

---

# 35. Consumed Event Policy

Recognition không subscribe trực tiếp broad workflow events.

Không trực tiếp consume:

```text
SOURCE_IMPORTED
STABLE_FRAME_READY
SESSION_STOPPED
SOURCE_CLOSED
APPLICATION_SHUTDOWN_REQUESTED
CONFIGURATION_CHANGED
RECOGNITION_REQUESTED
RECOGNITION_RETRY_REQUESTED
```

Runtime/Business Orchestration xử lý các facts đó.

Recognition nhận explicit execution contracts:

```text
RecognitionAttemptInput
ExecutionContextRef
CancellationContextRef
PrivacyContextRef
ConfigurationSnapshotId
ProviderAvailabilitySnapshot
```

---

# 36. No Hidden Orchestration

Recognition event handler không được:

* create WorkItem
* create Attempt
* trigger retry
* cancel Session
* select current Revision
* publish Artifact
* trigger Text Processing
* mutate Provider Manager
* activate Configuration
* stop Runtime

Event subscription không trở thành hidden pipeline.

---

# 37. Preferred Causal Order

Typical Attempt:

```text
RECOGNITION_PLAN_CREATED
        ↓
RECOGNITION_OCR_EXECUTION_COMPLETED
        ↓
RECOGNITION_CANDIDATE_VALIDATED
        ↓
RECOGNITION_CANDIDATE_SUBMITTED
```

Optional interleaving:

```text
RECOGNITION_PHASE_CHANGED
RECOGNITION_WARNING_RECORDED
RECOGNITION_MODULE_ERROR_RECORDED
RECOGNITION_CANCELLATION_OBSERVED
RECOGNITION_DEADLINE_OBSERVED
```

Runtime correctness không được phụ thuộc event order này.

---

# 38. Delivery Guarantees

Recognition tuân Event Bus guarantee.

Nếu Event Bus cung cấp at-least-once:

consumer phải chịu:

* duplicates
* delays
* out-of-order facts
* missing optional progress
* facts arriving after Attempt terminal state

Recognition EVENTS document không redefine delivery semantics.

---

# 39. Consumer Deduplication

Consumer deduplicate theo:

```text
EventId
```

Duplicate event không được:

* create duplicate WorkItem
* display duplicate authoritative result
* duplicate business counter
* append duplicate durable diagnostic record

---

# 40. Late Facts

Ví dụ:

```text
ATTEMPT_CANCELED
        ↓
late RECOGNITION_OCR_EXECUTION_COMPLETED
```

Consumer:

* may retain for diagnostics
* must not change Runtime state
* must not publish Candidate
* must not trigger downstream processing
* may mark as late

---

# 41. Stale Completion

Staleness không được quyết định bởi Recognition event consumer.

Canonical path:

```text
Attempt Completion
      ↓
Runtime Authority Validation
      ↓
ACCEPT / REJECT_STALE
```

Recognition facts giữ:

```text
RevisionId
WorkItemId
AttemptId
```

để correlation.

---

# 42. Privacy Classification

Default:

```text
OPERATIONAL_METADATA
```

Optional:

```text
PROTECTED_DIAGNOSTIC_REFERENCE
```

Allowed normal fields:

* Runtime IDs
* Provider ID
* CandidateArtifactId
* OCR/Quality/ReadingOrder ArtifactRefs when safe
* count summary
* duration
* warning/error code
* Completeness
* sanitized diagnostics ref

Forbidden:

```text
image bytes
raw image paths
complete OCR text
provider credentials
authorization headers
provider full response
translated text
user glossary content
```

---

# 43. Geometry in Recognition Events

Recognition events should not normally contain geometry.

Geometry belongs to OCR artifacts/contracts.

Only include a bounded geometry reference/summary when:

* concrete trusted consumer exists
* privacy allows
* Event Convention allows
* event remains bounded

Default:

```text
use ArtifactRef / EntityRef
```

---

# 44. Remote Provider Disclosure

Safe metadata may include:

```text
ProviderId
ExecutionLocation
RemoteExecutionUsed
PrivacyClassification
```

No image/text payload.

Primary provenance remains in Candidate/Artifact.

---

# 45. Diagnostic Security

Protected diagnostics may reference:

* processed image Artifact
* provider raw-response Artifact
* OCR visualization Artifact
* benchmark sample
* comparison report

Requirements:

* explicit diagnostic mode
* authorization
* bounded retention
* redaction
* auditability
* secure Artifact reference
* no unrestricted file path

---

# 46. Event Publication Failure

Nếu optional Recognition fact publish thất bại:

```text
record bounded local diagnostic if possible
        ↓
Event Bus policy retry/drop
        ↓
continue Recognition correctness path
```

Rules:

1. do not rerun Recognition
2. do not fail Attempt solely because optional fact failed
3. do not block Candidate submission
4. do not block Runtime
5. idempotent retry reuses EventId
6. record dropped-event metric when applicable

Artifact publication failure là concern khác.

---

# 47. Event Bus Unavailable

Recognition vẫn tiếp tục nếu execution contract/direct Runtime completion path hoạt động.

Behavior:

* suppress optional facts
* preserve bounded diagnostics if appropriate
* report observability degradation
* never invent terminal fallback events

---

# 48. Consumer Expectations — Diagnostics

Diagnostics may consume all Recognition facts.

Possible uses:

* plan creation timing
* OCR execution duration
* Candidate validation timing
* warning/error rate
* Completeness distribution
* Provider provenance
* cancellation/deadline observation
* late fact detection

Diagnostics không suy luận OCR quality chỉ từ module success.

---

# 49. Consumer Expectations — Presentation

Presentation may consume safe coarse progress only when a real UI need exists.

It must not:

* treat progress fact as result
* read full OCR output from Event Bus
* trigger Translation
* depend on Provider internals

---

# 50. Consumer Expectations — Text Processing

Text Processing không consume Recognition event for correctness.

It receives:

```text
Published RecognitionArtifact
```

through Runtime orchestration.

---

# 51. Consumer Expectations — Business Orchestration

Business Orchestration consumes:

* Runtime lifecycle
* authority
* published Artifact availability

Không dùng Recognition diagnostic facts làm authoritative pipeline trigger.

---

# 52. MVP Event Set

MVP correctness requires:

```text
no Recognition-specific event
```

Recommended optional MVP facts:

```text
RECOGNITION_CANDIDATE_VALIDATED
RECOGNITION_WARNING_RECORDED
RECOGNITION_MODULE_ERROR_RECORDED
```

Useful optional diagnostic fact:

```text
RECOGNITION_OCR_EXECUTION_COMPLETED
```

Development-only:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_PHASE_CHANGED
RECOGNITION_DIAGNOSTIC_RECORDED
```

Không thêm event nếu chưa có concrete consumer.

---

# 53. Deferred Extensions

Potential future module facts:

```text
RECOGNITION_PARTIAL_CANDIDATE_VALIDATED
RECOGNITION_LONG_PAGE_CHUNK_COMPLETED
RECOGNITION_PROVIDER_COMPARISON_RECORDED
RECOGNITION_STREAM_SEGMENT_OBSERVED
```

Không bao gồm:

```text
RECOGNITION_QUALITY_EVALUATED
RECOGNITION_READING_ORDER_RESOLVED
```

vì ownership đã thuộc OCR Architecture.

---

# 54. Example — Plan Created

```json
{
  "event_id": "evt_rec_plan_001",
  "event_name": "RECOGNITION_PLAN_CREATED",
  "contract_version": "1.1.0",
  "producer_module": "recognition",
  "occurred_at": "2026-08-03T01:15:42.190Z",
  "privacy_classification": "OPERATIONAL_METADATA",
  "partition_key": "attempt_01",
  "trace_context": {
    "trace_id": "trace_01"
  },
  "runtime_context": {
    "session_id": "session_01",
    "revision_id": "revision_104",
    "work_item_id": "work_recognition_104",
    "attempt_id": "attempt_01",
    "configuration_snapshot_id": "config_42",
    "recognition_operation": "RECOGNIZE_IMAGE",
    "recognition_profile": "COMIC_PAGE",
    "operation_phase": "PLANNING"
  },
  "payload": {
    "plan_id": "plan_rec_104_01",
    "recognition_operation": "RECOGNIZE_IMAGE",
    "recognition_profile": "COMIC_PAGE",
    "ocr_profile_ref": "ocr_comic_default",
    "execution_strategy": "COMBINED_OCR",
    "capability_requirement_summary": [
      "DETECTION",
      "RECOGNITION",
      "VERTICAL_TEXT"
    ],
    "privacy_mode": "LOCAL_ONLY",
    "execution_classes": ["GPU", "CPU"],
    "created_at": "2026-08-03T01:15:42.190Z"
  }
}
```

---

# 55. Example — OCR Execution Completed

```json
{
  "event_id": "evt_rec_ocr_completed_001",
  "event_name": "RECOGNITION_OCR_EXECUTION_COMPLETED",
  "contract_version": "1.1.0",
  "producer_module": "recognition",
  "occurred_at": "2026-08-03T01:15:42.840Z",
  "privacy_classification": "OPERATIONAL_METADATA",
  "partition_key": "attempt_01",
  "trace_context": {
    "trace_id": "trace_01"
  },
  "runtime_context": {
    "session_id": "session_01",
    "revision_id": "revision_104",
    "work_item_id": "work_recognition_104",
    "attempt_id": "attempt_01",
    "configuration_snapshot_id": "config_42",
    "recognition_operation": "RECOGNIZE_IMAGE",
    "recognition_profile": "COMIC_PAGE",
    "operation_phase": "EXECUTING_OCR"
  },
  "payload": {
    "ocr_document_ref": {
      "artifact_id": "ocr_document_candidate_104"
    },
    "reading_order_result_ref": {
      "artifact_id": "reading_order_candidate_104"
    },
    "quality_report_ref": {
      "artifact_id": "quality_report_candidate_104"
    },
    "provider_provenance_summary": {
      "provider_id": "local_ocr_01",
      "execution_location": "LOCAL_PROCESS"
    },
    "completeness": "COMPLETE",
    "warning_count": 1,
    "execution_duration_ms": 612,
    "completed_at": "2026-08-03T01:15:42.840Z"
  }
}
```

---

# 56. Example — Candidate Validated

```json
{
  "event_id": "evt_rec_candidate_valid_001",
  "event_name": "RECOGNITION_CANDIDATE_VALIDATED",
  "contract_version": "1.1.0",
  "producer_module": "recognition",
  "occurred_at": "2026-08-03T01:15:42.871Z",
  "privacy_classification": "OPERATIONAL_METADATA",
  "partition_key": "attempt_01",
  "trace_context": {
    "trace_id": "trace_01"
  },
  "runtime_context": {
    "session_id": "session_01",
    "revision_id": "revision_104",
    "work_item_id": "work_recognition_104",
    "attempt_id": "attempt_01",
    "candidate_artifact_id": "candidate_recognition_104",
    "configuration_snapshot_id": "config_42",
    "recognition_operation": "RECOGNIZE_IMAGE",
    "recognition_profile": "COMIC_PAGE",
    "operation_phase": "VALIDATING_CANDIDATE"
  },
  "payload": {
    "candidate_artifact_id": "candidate_recognition_104",
    "artifact_type": "RECOGNITION_ARTIFACT",
    "ocr_document_ref": {
      "artifact_id": "ocr_document_candidate_104"
    },
    "reading_order_result_ref": {
      "artifact_id": "reading_order_candidate_104"
    },
    "quality_report_ref": {
      "artifact_id": "quality_report_candidate_104"
    },
    "completeness": "COMPLETE",
    "warning_count": 1,
    "provider_provenance_summary": {
      "provider_id": "local_ocr_01",
      "execution_location": "LOCAL_PROCESS"
    },
    "compatibility_metadata_version": "1",
    "validation_duration_ms": 3,
    "validated_at": "2026-08-03T01:15:42.871Z"
  }
}
```

---

# 57. Testing Requirements

## Envelope

* EventId unique
* ContractVersion supported
* UTC timestamp
* producer = recognition
* Runtime context valid
* Privacy Classification present
* no credentials

---

## Plan Fact

* emitted only after READY Plan
* no OCR internal policy duplication
* no Provider SDK fields
* no image data

---

## OCR Execution Fact

* refs only
* no OCR payload
* no Quality payload
* no Reading Graph
* valid Completeness
* safe Provider summary

---

## Candidate Fact

* CandidateValidated only after module validation
* CandidateSubmitted at most once
* Candidate facts do not imply publication
* stale Runtime rejection remains possible

---

## Warning/Error Facts

* warning separate from error
* module error not terminal Runtime event
* cancellation observed ≠ Attempt canceled
* deadline observed ≠ Attempt failed

---

## Boundaries

* no Recognition terminal event
* no Provider lifecycle event
* no Quality-owned event
* no ReadingOrder-owned event
* no WorkItem creation
* no Artifact publication
* no Text Processing trigger

---

## Privacy

* no raw image
* no full OCR text
* no Provider response
* no credentials
* safe remote disclosure
* protected diagnostic refs controlled

---

## Failure

* Event Bus unavailable does not fail Recognition
* retry uses same EventId
* dropped fact does not block Candidate
* optional progress loss tolerated

---

# 58. Event Invariants

1. Recognition facts are immutable.

2. Every fact has unique EventId.

3. Every fact carries ContractVersion.

4. Attempt-local facts carry RevisionId, WorkItemId and AttemptId.

5. Recognition emits no terminal Attempt event.

6. Recognition emits no authoritative Artifact publication event.

7. Recognition emits no Provider lifecycle event.

8. Recognition does not redefine OCR stage events.

9. Recognition does not redefine Quality events.

10. Recognition does not redefine Reading Order events.

11. Recognition facts do not create WorkItem.

12. Recognition facts do not trigger retry.

13. Recognition facts do not cancel Session.

14. Recognition facts do not grant authority.

15. Candidate VALID does not mean published.

16. Candidate SUBMITTED does not mean accepted.

17. Progress facts are optional.

18. Consumers do not depend on progress facts for correctness.

19. Duplicate delivery cannot duplicate business execution.

20. Global order is not assumed.

21. Late facts do not mutate Runtime state.

22. Raw images never appear in normal facts.

23. Full OCR content never appears in normal facts.

24. Provider credentials never appear.

25. Provider SDK types never appear.

26. Event publication failure does not rerun Recognition.

27. Event publication failure does not block Candidate submission.

28. Remote execution is disclosed safely.

29. Geometry is reference-first and bounded when included.

30. Warning is not failure.

31. Module error fact is not terminal outcome.

32. Cancellation observation is not cancellation authority.

33. Text Processing consumes published Artifact, not Recognition events.

34. Recognition does not consume broad workflow events directly.

35. Runtime correctness is Event Bus-independent.

36. Protected diagnostic events have stricter access/retention.

37. Unknown additive event fields are handled safely.

38. Unsupported major version is rejected.

39. Event metadata remains bounded.

40. No hidden orchestration exists through Recognition subscriptions.

---

# 59. Related Documents

```text
doc/02-modules/recognition/README.md
doc/02-modules/recognition/MODULE.md
doc/02-modules/recognition/CONTRACT.md
doc/02-modules/recognition/STATES.md
doc/02-modules/recognition/ERRORS.md

doc/01-architecture/ocr/PIPELINE.md
doc/01-architecture/ocr/DETECTION.md
doc/01-architecture/ocr/RECOGNITION.md
doc/01-architecture/ocr/TEXT_DIRECTION.md
doc/01-architecture/ocr/LAYOUT.md
doc/01-architecture/ocr/POSTPROCESS.md
doc/01-architecture/ocr/QUALITY.md
doc/01-architecture/ocr/READING_ORDER.md
doc/01-architecture/ocr/PROVIDERS.md

doc/01-architecture/EVENT_BUS.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
```

---

# 60. Summary

Recognition events communicate only module-level facts.

```text
Recognition Plan Created
        ↓
OCR Execution Completed
        ↓
Candidate Validated
        ↓
Candidate Submitted
```

Optional observations may accompany the flow:

```text
Warning
Module Error
Cancellation Observed
Deadline Observed
Phase Changed
Diagnostics
```

OCR Architecture owns:

```text
OCR stage semantics
Quality semantics
Reading Order semantics
Provider semantic contracts
```

Runtime owns:

```text
Attempt Outcome
Authority
Retry
Cancellation
```

Artifact Store owns:

```text
Ownership Transfer
Publication
Artifact Availability
```

Provider Manager owns:

```text
Provider Lifecycle
Provider Health
Provider Capacity
```

The core rule is:

```text
Recognition events explain
what the Recognition Module observed or produced.

They do not redefine OCR internals.

They do not decide whether work still matters.

They do not publish the authoritative result.
```
