# Recognition Module Contract

> **Project:** CRAI
> **Module:** Recognition
> **Path:** `02-modules/recognition/CONTRACT.md`
> **Version:** 1.1.0
> **Status:** Architecture Draft
> **Architecture Reference:** `01-architecture/ocr/`

---

# 1. Purpose

Tài liệu này định nghĩa public contract của Recognition Module.

Nó đặc tả:

* Runtime-facing Attempt Input
* Runtime-facing Attempt Output
* Recognition Operation
* Recognition Profile
* Candidate Recognition Artifact
* Published Recognition Artifact
* module warnings
* module errors
* retry hints
* completeness semantics
* compatibility metadata
* privacy requirements
* validation rules
* producer obligations
* consumer obligations
* Runtime obligations
* contract evolution

Tài liệu này **không định nghĩa lại OCR semantic contracts**.

Các semantics sau thuộc `01-architecture/ocr/`:

| Concern                             | Owner               |
| ----------------------------------- | ------------------- |
| Geometry / Region                   | `DETECTION.md`      |
| Character / Word / Line / Paragraph | `RECOGNITION.md`    |
| Writing Direction                   | `TEXT_DIRECTION.md` |
| Layout                              | `LAYOUT.md`         |
| OCR Document                        | `POSTPROCESS.md`    |
| Quality Report                      | `QUALITY.md`        |
| Reading Order                       | `READING_ORDER.md`  |
| Provider Contract / Capability      | `PROVIDERS.md`      |

Recognition Contract chỉ reference các contract đó.

---

# 2. Contract Boundary

Recognition nhận image-based Artifact input và tạo Candidate Recognition Artifact.

```text
RecognitionAttemptInput
        ↓
Recognition Module
        ↓
RecognitionAttemptOutput
        ├── CandidateRecognitionArtifact?
        ├── RecognitionModuleError?
        ├── RetryHint?
        └── DiagnosticsRef?
```

Published output chỉ tồn tại sau Runtime acceptance:

```text
CandidateRecognitionArtifact
        ↓
Runtime Authority Validation
        ↓
Artifact Store Ownership Transfer
        ↓
RecognitionArtifact
```

Recognition không expose application-level commands như:

```text
RetryRecognition
CancelRecognition
PublishRecognitionResult
```

Các action đó thuộc Runtime hoặc Artifact Store.

---

# 3. Contract Principles

## 3.1 Provider Independence

Public Recognition contract không chứa Provider SDK object.

---

## 3.2 OCR Semantic Reuse

Recognition Contract tham chiếu OCR Architecture contract thay vì định nghĩa lại:

```text
Region
Recognition Result
Direction Result
Layout Result
OCR Document
Quality Report
Reading Order Result
```

---

## 3.3 Immutable Artifact

Candidate immutable sau assembly.

Published Recognition Artifact immutable sau publication.

---

## 3.4 Explicit Runtime Identity

Mọi Attempt Input phải mang Runtime identity rõ ràng.

---

## 3.5 Explicit Uncertainty

Unknown:

* confidence
* language
* script
* completeness
* quality

phải được giữ explicit.

Không tự chuyển unknown thành giá trị mặc định giả tạo.

---

## 3.6 Authority Separation

Recognition không quyết định:

* current Revision
* accepted Attempt
* publication authority

---

## 3.7 Candidate Separation

Candidate Artifact khác Published Artifact.

---

## 3.8 Privacy Preservation

Raw image và full recognized content không xuất hiện trong normal event/log.

---

## 3.9 Backward Compatibility

Contract evolution giữ semantic meaning trong cùng major version.

---

# 4. Contract Version

```text
RecognitionContractVersion
├── Major
├── Minor
└── Patch
```

Initial:

```text
1.1.0
```

Semantics:

* Major = incompatible semantic change
* Minor = backward-compatible addition
* Patch = clarification/non-semantic fix

Cross-process form phải mang version.

---

# 5. Shared Types

Recognition Contract sử dụng shared types từ shared/runtime/artifact contracts.

Ví dụ:

```text
SessionId
RevisionId
WorkItemId
AttemptId
ArtifactId
CandidateArtifactId
ConfigurationSnapshotId
TraceId
```

Recognition không redefine identifier semantics.

Identifiers là opaque.

Consumer không suy luận:

* timestamp
* ordering
* provider identity
* revision relationship

từ nội dung ID.

---

# 6. Shared Scalar Types

Recognition dùng shared scalar contracts cho:

```text
Timestamp
Duration
LanguageCode
ScriptCode
Metadata
```

Recommended standards:

```text
Timestamp
    → ISO-8601 UTC

LanguageCode
    → BCP-47 compatible

ScriptCode
    → ISO-15924 compatible
```

Metadata không chứa:

* raw image
* Provider SDK object
* secret
* executable data
* full recognized content mặc định

---

# 7. Runtime Context

```text
RecognitionRuntimeContext
├── ContractVersion
├── ApplicationInstanceId
├── SessionId?
├── RevisionId
├── WorkItemId
├── AttemptId
├── ConfigurationSnapshotId
├── TraceContext
└── CreatedAt
```

Rules:

1. `RevisionId`, `WorkItemId`, `AttemptId` required.
2. `SessionId` optional cho standalone imports.
3. Retry count không thuộc contract.
4. Priority không thuộc contract.
5. Queue class không thuộc contract.
6. Runtime identity không cấp authority cho Recognition.

---

# 8. Artifact Reference

Recognition nhận Artifact reference, không nhận embedded raw image.

```text
ArtifactRef
├── ArtifactId
├── ArtifactType
├── ContractVersion
├── ResourceId
├── ContentIdentity
└── Metadata?
```

Required input type:

```text
IMAGE_ARTIFACT
```

Rules:

1. Input immutable.
2. Resource valid trong Attempt lifetime.
3. Shared access thông qua Resource Lease.
4. Recognition không dispose shared input.
5. Remote access phải tuân Privacy Context.

---

# 9. Content Identity

```text
ContentIdentity
├── IdentityAlgorithm
├── IdentityVersion
├── Value
└── SourceScope?
```

Content Identity:

* không đồng nghĩa ArtifactId
* không đồng nghĩa RevisionId
* dùng cho semantic compatibility
* không expose raw preimage

---

# 10. Recognition Operation

```text
RecognitionOperation
├── RECOGNIZE_IMAGE
├── RECOGNIZE_REGION
└── EVALUATE_IMAGE
```

## RECOGNIZE_IMAGE

Process toàn image artifact.

---

## RECOGNIZE_REGION

Process một selected source-space region.

---

## EVALUATE_IMAGE

Diagnostic/evaluation execution.

Không mặc định tạo published user-facing artifact.

---

# 11. Recognition Profile

```text
RecognitionProfile
├── AUTOMATIC
├── COMIC_PAGE
├── SCREENSHOT
├── SINGLE_REGION
└── STRUCTURED_PAGE
```

Profile ảnh hưởng Recognition Plan.

Profile không thay đổi public Artifact contract.

---

# 12. Recognition Options

```text
RecognitionOptions
├── Profile
├── LanguageHints[]
├── ScriptHints[]
├── OrientationHint?
├── RegionSelection?
├── AllowPartialCandidate
├── DiagnosticLevel
└── OCRProfileRef?
```

Các OCR-specific semantics như:

* direction model
* geometry type
* reading-order strategy
* quality thresholds

được reference qua OCR Architecture/Profile thay vì redefine ở đây.

---

# 13. Diagnostic Level

```text
DiagnosticLevel
├── NONE
├── BASIC
├── DETAILED
└── PROTECTED_CONTENT
```

`PROTECTED_CONTENT` yêu cầu explicit privacy authorization.

---

# 14. Capability Requirements

Recognition khai báo **requirements**, không define provider capability model.

```text
RecognitionCapabilityRequirements
├── RequiredCapabilities[]
├── RequiredLanguages[]
├── RequiredScripts[]
├── LocalOnly
├── RemoteAllowed
├── PartialOutputAllowed
└── HardwarePreference?
```

Capability semantics thuộc:

```text
01-architecture/ocr/PROVIDERS.md
```

Provider Selection Policy consume requirements này.

---

# 15. Recognition Attempt Input

```text
RecognitionAttemptInput
├── RuntimeContext
├── InputArtifactRef
├── Operation
├── RegionSelection?
├── Options
├── CapabilityRequirements
├── ExecutionContextRef
├── CancellationContextRef
├── PrivacyContextRef
└── DiagnosticsContextRef?
```

---

# 16. Attempt Input Preconditions

Attempt Input hợp lệ khi:

1. Runtime Context hợp lệ.
2. InputArtifactRef resolvable.
3. Artifact type được hỗ trợ.
4. Operation được hỗ trợ.
5. RegionSelection hợp lệ nếu có.
6. Recognition Profile hợp lệ.
7. Capability Requirements internally consistent.
8. Privacy Context cho phép execution path.
9. Contract major version được hỗ trợ.

Recognition không tự kiểm tra:

```text
is this Revision still current?
```

Runtime Control sở hữu authority validation.

---

# 17. Region Selection

```text
RegionSelection
├── GeometryRef
├── CoordinateSpaceRef
├── SelectionSource
└── Metadata?
```

```text
SelectionSource
├── USER_SELECTED
├── RUNTIME_DERIVED
├── RETRY_SCOPE
└── DIAGNOSTIC
```

Geometry semantics thuộc OCR Detection/shared geometry contracts.

Recognition không redefine Rectangle/Polygon.

---

# 18. Execution Context Reference

```text
ExecutionContextRef
├── ExecutionClass
├── Deadline?
├── ResourceBudgetRef?
├── ProviderSelectionRef?
└── RuntimePolicyRef?
```

Recognition chỉ consume context.

Nó không thay đổi Runtime policy.

---

# 19. Cancellation Context

```text
CancellationContextRef
├── CancellationId
├── IsCancellationRequested
├── RequestedAt?
├── Reason?
└── CheckpointPolicyRef?
```

Recognition cooperative-check cancellation.

Recognition không:

* revoke authority
* cancel WorkItem lineage
* create global cancellation registry
* decide terminal outcome

---

# 20. Privacy Context

```text
PrivacyContextRef
├── PrivacyMode
├── PrivacyPartition
├── LocalProcessingRequired
├── RemoteProcessingAllowed
├── DiagnosticContentAllowed
└── PersistenceAllowed
```

```text
PrivacyMode
├── STANDARD
├── LOCAL_ONLY
└── EPHEMERAL
```

---

# 21. Recognition Attempt Output

```text
RecognitionAttemptOutput
├── CandidateArtifact?
├── ModuleWarnings[]
├── ModuleError?
├── RetryHint?
├── DiagnosticsRef?
├── QualitySummaryRef?
└── CompletionMetadata
```

Primary execution disposition:

```text
CandidateArtifact present
or
ModuleError present
or
Cancellation observed
```

Runtime mới quyết định accepted terminal outcome.

---

# 22. Completion Metadata

```text
RecognitionCompletionMetadata
├── StartedAt
├── CompletedAt
├── OperationPhase
├── ProviderRequestIds[]
├── ExecutionMetricsRef?
└── CancellationObserved
```

Completion metadata thuộc Attempt.

Không được copy vào Published Artifact nếu không có semantic reason.

---

# 23. Candidate Recognition Artifact

```text
CandidateRecognitionArtifact
├── CandidateArtifactId
├── ArtifactType
├── OwnerModule
├── ContractVersion
├── InputArtifactRef
├── ContentIdentity
├── OCRDocumentRef
├── ReadingOrderResultRef?
├── QualityReportRef?
├── ProviderProvenance
├── LanguageHypotheses[]
├── ScriptHypotheses[]
├── Warnings[]
├── Completeness
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

Candidate Rules:

1. non-authoritative
2. non-published
3. immutable after assembly
4. private to Runtime validation path
5. not cache eligible by default
6. cleanup required after rejection
7. no WorkItem status
8. no retry count
9. no queue timing
10. no credentials

---

# 24. Published Recognition Artifact

```text
RecognitionArtifact
├── ArtifactId
├── ArtifactType
├── ContractVersion
├── InputArtifactRef
├── ContentIdentity
├── OCRDocumentRef
├── ReadingOrderResultRef?
├── QualityReportRef?
├── ProviderProvenance
├── LanguageHypotheses[]
├── ScriptHypotheses[]
├── Warnings[]
├── Completeness
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

Published Artifact:

* immutable
* Runtime-accepted
* Artifact Store-owned
* provider-independent
* source-traceable
* reusable only when compatibility and Cache Policy allow

---

# 25. Recognition Artifact vs OCR Document

Recognition Artifact là **module-level publication object**.

OCR Document là **OCR semantic artifact**.

```text
RecognitionArtifact
    ↓ references
OCRDocument
```

Recognition Artifact không redefine:

* Region
* Character
* Word
* Line
* Paragraph
* Direction
* Layout
* Reading Graph

Những semantics đó thuộc OCR Architecture.

---

# 26. Provider Provenance

```text
ProviderProvenance
├── ProviderId
├── ProviderVersion
├── AdapterVersion
├── ExecutionLocation
├── ExecutionClass
├── ModelId?
├── ModelVersion?
└── SanitizedMetadata?
```

No:

* credentials
* secret endpoint
* SDK internals

Provider provenance dùng cho:

* traceability
* compatibility
* diagnostics
* evaluation

Không dùng làm core semantic dependency.

---

# 27. Language Hypotheses

```text
LanguageHypothesis
├── LanguageCode
├── ConfidenceRef?
├── Source
└── ScopeRefs[]
```

Request hint không được biểu diễn như detected fact.

---

# 28. Script Hypotheses

```text
ScriptHypothesis
├── ScriptCode
├── ConfidenceRef?
├── Source
└── ScopeRefs[]
```

---

# 29. OCR Semantic References

Recognition Contract **không embed duplicate OCR structures**.

Candidate/Artifact có thể reference:

```text
OCRDocumentRef
ReadingOrderResultRef
QualityReportRef
```

Nếu implementation cần in-process expanded object:

```text
OCRDocument
ReadingOrderResult
QualityReport
```

phải tuân authoritative OCR Architecture contract.

---

# 30. Completeness

```text
RecognitionCompleteness
├── COMPLETE
├── PARTIAL
├── EMPTY_VALID
└── UNKNOWN
```

## EMPTY_VALID

Không có readable text nhưng execution thành công.

```text
Completeness = EMPTY_VALID
OCRDocument contains no readable text entity
```

Không phải failure.

---

## PARTIAL

Một phần OCR result usable.

Partial phải explicit.

Runtime/consumer quyết định có publish/use hay không.

---

# 31. Warning Contract

```text
RecognitionWarning
├── WarningCode
├── OperationPhase
├── Severity
├── MessageKey
├── ScopeRef?
├── ProviderId?
└── Metadata?
```

Severity:

```text
INFORMATION
DEGRADED
ATTENTION_REQUIRED
```

Warning không phải fatal terminal state.

---

# 32. Warning Codes

Module-level warning examples:

```text
NO_READABLE_TEXT_DETECTED
PARTIAL_RECOGNITION
REMOTE_PROVIDER_USED
FALLBACK_PROVIDER_USED
PROVIDER_CONFIDENCE_UNAVAILABLE
OUTPUT_TRUNCATED
DIAGNOSTIC_DATA_LIMITED
```

Stage-specific warnings như:

```text
LOW_DETECTION_CONFIDENCE
READING_ORDER_UNCERTAIN
```

có thể được surfaced, nhưng semantic definition thuộc OCR owner tương ứng.

---

# 33. Recognition Operation Phase

Diagnostic-only phases:

```text
VALIDATING
PLANNING
ACQUIRING_INPUT
EXECUTING_OCR
ASSEMBLING_CANDIDATE
FINALIZING
```

Không phải Runtime Attempt state machine.

---

# 34. Module Error Contract

```text
RecognitionModuleError
├── ContractVersion
├── ErrorCode
├── OperationPhase
├── MessageKey
├── RetryHint?
├── AffectedScopeRef?
├── ProviderErrorRef?
├── DiagnosticsRef?
├── Metadata?
└── OccurredAt
```

---

# 35. Module Error Codes

Module-level errors:

```text
RECOGNITION_INPUT_INVALID
RECOGNITION_ARTIFACT_UNAVAILABLE
RECOGNITION_OPERATION_UNSUPPORTED
RECOGNITION_CAPABILITY_UNAVAILABLE
RECOGNITION_OCR_EXECUTION_FAILED
RECOGNITION_CANDIDATE_INVALID
RECOGNITION_RESOURCE_EXHAUSTED
RECOGNITION_INTERNAL_ERROR
```

Detailed OCR-stage errors remain owned by:

* `DETECTION.md`
* `RECOGNITION.md`
* `TEXT_DIRECTION.md`
* `LAYOUT.md`
* `POSTPROCESS.md`
* `QUALITY.md`
* `READING_ORDER.md`

Recognition may wrap/reference them.

---

# 36. Error Message Rules

Error message:

* safe for logs
* no raw OCR text
* no secret path
* no credential
* no full provider response
* no SDK-dependent vocabulary required for understanding

---

# 37. Retry Hint

```text
RetryHint
├── Retryability
├── SuggestedStrategies[]
├── AlternativeProviderAllowed
├── AlternativePreparationAllowed
└── ReasonCode
```

```text
Retryability
├── RETRYABLE
├── CONDITIONALLY_RETRYABLE
└── NON_RETRYABLE
```

Possible hints:

```text
SAME_PROVIDER
ALTERNATIVE_PROVIDER
ALTERNATIVE_PREPARATION
REGION_ONLY
RESOURCE_WAIT
NO_RETRY
```

RetryHint là advisory.

Runtime Retry Policy là authority.

---

# 38. Diagnostic Facts

Recognition có thể emit:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_OCR_COMPLETED
RECOGNITION_WARNING_RECORDED
RECOGNITION_CANDIDATE_CREATED
```

Không emit Recognition-owned terminal authority events.

Terminal Attempt semantics thuộc Runtime.

Diagnostic facts:

* immutable
* bounded
* optional
* content-safe
* not required for correctness
* not used for hidden orchestration

---

# 39. Compatibility Metadata

```text
RecognitionCompatibilityMetadata
├── InputContentIdentity
├── RecognitionContractVersion
├── RecognitionProfileVersion
├── OCRPipelineVersion
├── OCRProfileVersion
├── ProviderProfileVersion
├── LanguageHints[]
├── ScriptHints[]
├── ConfigurationVersions[]
├── PrivacyPartition
└── OCRSemanticDependencies
```

`OCRSemanticDependencies` có thể reference stage/result/model versions thông qua OCR Document lineage.

Recognition không cần duplicate:

* Detection Model Version
* Reading Order Policy Version
* Quality Policy Version

nếu chúng đã có trong referenced OCR artifacts.

---

# 40. Compatibility Evaluation

Hai Recognition Artifact có thể reusable về semantic khi:

* InputContentIdentity tương thích
* major contract compatible
* Recognition Profile compatible
* OCR semantic dependencies compatible
* required language/script requirements compatible
* Provider differences được policy cho phép
* privacy partition compatible
* required output references tồn tại

`RevisionId` match không bắt buộc.

Cache Policy quyết định reuse cuối cùng.

---

# 41. Attempt Input Validation

Invalid khi:

* unsupported contract major
* Runtime identity thiếu
* Input Artifact thiếu
* Artifact type sai
* RegionSelection không hợp lệ
* Profile sai
* impossible Capability Requirements
* Privacy Context thiếu/xung đột
* Cancellation Context malformed

---

# 42. Candidate Validation

Candidate phải đảm bảo:

* CandidateArtifactId present
* OwnerModule = recognition
* correct ArtifactType
* InputArtifactRef present
* ContentIdentity present
* OCRDocumentRef valid
* referenced optional ReadingOrder/Quality artifact compatible
* ProviderProvenance valid
* Completeness consistent
* no SDK object
* no credential
* CompatibilityMetadata sufficient
* IntegrityMetadata valid

OCR entity-level validation thuộc OCR Document/ReadingOrder/Quality owner.

---

# 43. Published Artifact Validation

Recognition Contract yêu cầu:

* Candidate semantics preserved
* ArtifactId assigned
* ownership transferred
* publication atomic
* no Attempt state embedded
* immutable references retained

Publication validation implementation thuộc Runtime/Artifact Store.

---

# 44. Authority Rules

1. Recognition cannot grant authority.
2. Candidate có thể valid nhưng stale.
3. Late output không tự publish.
4. Canceled Attempt có thể physically finish.
5. Completion không đồng nghĩa publication.
6. Runtime owns current Revision relevance.
7. Artifact Store publication requires Runtime approval.

---

# 45. Cancellation Rules

1. Recognition cooperative-check Runtime cancellation.
2. Cancellation không đảm bảo immediate physical stop.
3. Provider interruption capability được khai báo ngoài module contract.
4. Attempt-local resources phải release.
5. Recognition không publish.
6. Late output không authoritative.
7. Runtime owns singular terminal outcome.

---

# 46. Empty Artifact Contract

No readable text:

```text
RecognitionArtifact
├── Completeness = EMPTY_VALID
├── OCRDocumentRef → valid empty OCR Document
└── Warnings includes NO_READABLE_TEXT_DETECTED
```

Không phải failure.

---

# 47. Partial Artifact Contract

Khi partial được policy cho phép:

```text
CandidateRecognitionArtifact
├── Completeness = PARTIAL
├── OCRDocumentRef → partial-compatible OCR Document
└── Warnings includes PARTIAL_RECOGNITION
```

Rules:

1. partial explicit
2. failed scopes traceable
3. provider total failure không bị che giấu
4. Runtime decides publish/use
5. Candidate not cache eligible by default

---

# 48. Privacy Contract

## Local-Only Guarantee

Khi:

```text
LocalProcessingRequired = true
```

Recognition phải đảm bảo:

* image không gửi remote
* recognized content không gửi remote
* remote fallback disabled
* provenance reflects local processing

---

## Logging

Normal logs có thể chứa:

* Runtime IDs
* Provider ID
* duration
* entity counts
* warning count
* error code
* phase

Không được chứa:

* image payload
* full recognized text
* API key
* provider token
* authorization header
* sensitive temp path
* full remote response

---

# 49. Producer Obligations

Recognition implementation phải:

1. validate Attempt Input
2. preserve Runtime identity
3. use OCR Architecture contracts
4. preserve source traceability
5. preserve raw source meaning
6. represent uncertainty explicitly
7. create immutable Candidate
8. normalize module errors
9. separate warning/error
10. enforce Privacy Context
11. release Attempt-local resources
12. use Resource Lease correctly
13. never grant authority
14. never publish accepted Artifact
15. never retry itself
16. never own Provider lifecycle
17. never depend on Translation implementation
18. never depend on UI
19. maintain contract compatibility
20. keep diagnostics content-safe

---

# 50. Consumer Obligations

Consumers must:

1. honor immutability
2. handle EMPTY_VALID
3. handle PARTIAL separately
4. preserve Artifact traceability
5. not parse Provider metadata for core semantics
6. use OCRDocumentRef as canonical OCR content
7. use ReadingOrderResultRef when ordering is required
8. use QualityReportRef when quality evaluation is required
9. handle unknown enum values safely
10. not mutate Recognition Artifact for correction
11. perform semantic cleanup outside Recognition

---

# 51. Runtime Obligations

Runtime must:

1. create WorkItem/Attempt identity
2. supply immutable ArtifactRef
3. provide Execution Context
4. provide Cancellation Context
5. own deadline
6. own Scheduler admission
7. own retry decision
8. own authority validation
9. own terminal Attempt outcome
10. coordinate Candidate cleanup
11. transfer ownership to Artifact Store
12. publish atomically
13. reject stale/duplicate Candidate
14. provide Cache Policy
15. keep Provider operational state outside Recognition contract

---

# 52. Provider Integration Obligations

Provider integration must:

* expose capabilities through OCR Provider Contract
* isolate SDK objects
* normalize Provider responses
* normalize Provider errors
* protect credentials
* enforce privacy
* report Provider identity/version
* avoid hidden retry
* avoid Runtime publication

Detailed provider contract belongs to:

```text
01-architecture/ocr/PROVIDERS.md
```

---

# 53. Artifact Store Obligations

Artifact Store must:

* receive accepted Candidate transfer
* assign ArtifactId
* own published payload lifecycle
* publish atomically
* provide immutable lookup
* manage lease/retention
* reject duplicate publication
* clean failed transfers

---

# 54. Serialization Guidance

Recommended:

```text
In-process
    → typed native objects

Cross-process
    → Protocol Buffers / JSON / MessagePack
```

Large OCR payload should use artifact references.

Contract should avoid embedding duplicated OCR graphs when references are sufficient.

---

# 55. Contract Evolution

Backward-compatible changes within same major:

* optional fields
* new warning codes
* new module error codes
* optional metadata
* new Recognition Profiles
* new optional references
* additive enum values when unknown-safe

Breaking changes requiring major version:

* removing/renaming required field
* semantic meaning change
* identity semantics change
* privacy guarantee change
* Candidate/publication boundary change
* ownership change
* OCRDocumentRef contract meaning change
* compatibility semantics change

---

# 56. Unknown Values

Consumers must:

* preserve unknown when possible
* fall back safely
* reject unsupported major version
* not crash on unknown warning
* treat unknown capability as unsupported
* retain unknown uncertainty rather than fabricate values

---

# 57. Example Recognition Attempt Input

```json
{
  "runtime_context": {
    "contract_version": "1.1.0",
    "session_id": "session_01",
    "revision_id": "revision_104",
    "work_item_id": "work_recognition_104",
    "attempt_id": "attempt_01",
    "configuration_snapshot_id": "config_42"
  },
  "input_artifact_ref": {
    "artifact_id": "image_artifact_104",
    "artifact_type": "IMAGE_ARTIFACT",
    "content_identity": {
      "identity_algorithm": "sha256",
      "identity_version": "1",
      "value": "content_identity_redacted"
    }
  },
  "operation": "RECOGNIZE_IMAGE",
  "options": {
    "profile": "COMIC_PAGE",
    "language_hints": ["zh-Hans"],
    "script_hints": ["Hans"],
    "allow_partial_candidate": true,
    "diagnostic_level": "BASIC",
    "ocr_profile_ref": "ocr_comic_default"
  },
  "capability_requirements": {
    "required_capabilities": [
      "DETECTION",
      "RECOGNITION"
    ],
    "required_languages": ["zh-Hans"],
    "local_only": true,
    "remote_allowed": false
  },
  "privacy_context_ref": {
    "privacy_mode": "LOCAL_ONLY",
    "privacy_partition": "profile_local",
    "local_processing_required": true,
    "remote_processing_allowed": false
  }
}
```

---

# 58. Example Candidate Recognition Artifact

```json
{
  "candidate_artifact_id": "candidate_recognition_104",
  "artifact_type": "RECOGNITION_ARTIFACT",
  "owner_module": "recognition",
  "contract_version": "1.1.0",
  "input_artifact_ref": {
    "artifact_id": "image_artifact_104",
    "artifact_type": "IMAGE_ARTIFACT"
  },
  "content_identity": {
    "identity_algorithm": "sha256",
    "identity_version": "1",
    "value": "content_identity_redacted"
  },
  "ocr_document_ref": {
    "artifact_id": "ocr_document_candidate_104",
    "contract_version": "1.1"
  },
  "reading_order_result_ref": {
    "artifact_id": "reading_order_candidate_104"
  },
  "quality_report_ref": {
    "artifact_id": "quality_report_candidate_104"
  },
  "provider_provenance": {
    "provider_id": "local_ocr_01",
    "provider_version": "1.2.0",
    "adapter_version": "1.1.0",
    "execution_location": "LOCAL_PROCESS",
    "model_id": "chinese_comic_ocr",
    "model_version": "0.4"
  },
  "warnings": [],
  "completeness": "COMPLETE"
}
```

---

# 59. Contract Test Requirements

## Attempt Input

* valid full-image input
* valid selected-region input
* missing ArtifactRef
* unsupported Artifact type
* invalid profile
* privacy conflict
* malformed capability requirement
* unsupported contract major

---

## Candidate Artifact

* valid Candidate
* warnings
* empty-valid
* partial
* invalid OCRDocumentRef
* incompatible ReadingOrderResultRef
* incompatible QualityReportRef
* missing CompatibilityMetadata
* SDK leakage
* credential leakage

---

## Runtime / Authority

* stale Candidate rejected
* canceled Attempt output rejected
* non-cancelable provider returns late
* Candidate accepted and atomically published

---

## Privacy

* local-only enforcement
* raw image excluded from event/log
* raw OCR content excluded from normal logs
* remote execution disclosure
* credential redaction

---

# 60. Contract Invariants

1. Recognition input is image-based Artifact.

2. Runtime identity is explicit.

3. Candidate and Published Artifact are distinct.

4. Recognition creates Candidate only.

5. Runtime owns authority.

6. Artifact Store owns published payload lifecycle.

7. Provider SDK types never cross public boundary.

8. Provider credentials never appear in public output.

9. Local-only content never goes remote.

10. OCR semantics are referenced from `01-architecture/ocr/`.

11. Recognition Contract does not redefine Region semantics.

12. Recognition Contract does not redefine text hierarchy.

13. Recognition Contract does not redefine Direction.

14. Recognition Contract does not redefine Layout.

15. Recognition Contract does not redefine Reading Order.

16. Recognition Contract does not redefine Quality.

17. OCRDocumentRef is the canonical OCR semantic reference.

18. Raw source meaning is preserved.

19. Empty result may be successful.

20. Partial output is explicit.

21. Warning does not replace error.

22. Candidate immutable after assembly.

23. Published Artifact immutable.

24. Recognition never retries itself.

25. Recognition never owns cancellation authority.

26. Recognition never publishes accepted Artifact.

27. Recognition never owns Provider lifecycle.

28. Runtime Attempt state is not embedded in Artifact.

29. Queue/retry timing is not embedded in Artifact.

30. Compatibility metadata is explicit.

31. RevisionId is not reuse identity by default.

32. Input Artifact accessed through lease/reference.

33. Candidate rejection triggers cleanup.

34. Late output cannot regain authority.

35. Diagnostic facts do not create hidden orchestration.

36. Event/log payloads remain content-safe.

37. Unknown values handled safely.

38. Contract major version protects semantic compatibility.

39. Provider metadata is not core business semantics.

40. User correction never mutates original Recognition Artifact.

---

# 61. MVP Contract Subset

Required operations:

```text
RECOGNIZE_IMAGE
RECOGNIZE_REGION
```

Required input:

```text
Image ArtifactRef
AUTOMATIC / COMIC_PAGE / SINGLE_REGION Profile
Simplified Chinese hint
English hint
LOCAL_ONLY support
CancellationContextRef
ExecutionContextRef
```

Required Candidate output:

```text
CandidateArtifactId
OCRDocumentRef
ProviderProvenance
Warnings
Completeness
CompatibilityMetadata
TraceabilityMetadata
```

Recommended when available:

```text
ReadingOrderResultRef
QualityReportRef
```

Optional MVP features:

* Partial Candidate
* remote Provider
* character geometry
* advanced Provider alternatives
* structured document profile

---

# 62. Deferred Extensions

Future possibilities:

* streaming OCR candidate updates
* long-page chunk contracts
* tiled image processing
* Artifact diff
* user correction layer
* provider ensemble result
* OCR feedback contract
* encrypted remote request envelope

Only add when a concrete capability requires them.

---

# 63. Related Documents

```text
01-architecture/ocr/README.md
01-architecture/ocr/PIPELINE.md
01-architecture/ocr/PREPROCESS.md
01-architecture/ocr/DETECTION.md
01-architecture/ocr/RECOGNITION.md
01-architecture/ocr/TEXT_DIRECTION.md
01-architecture/ocr/LAYOUT.md
01-architecture/ocr/POSTPROCESS.md
01-architecture/ocr/QUALITY.md
01-architecture/ocr/READING_ORDER.md
01-architecture/ocr/PROVIDERS.md

01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/RESOURCE_LIFECYCLE.md
01-architecture/runtime/CACHE_POLICY.md

02-modules/recognition/MODULE.md
02-modules/recognition/STATES.md
02-modules/recognition/EVENTS.md
02-modules/recognition/ERRORS.md
02-modules/recognition/README.md
```

---

# 64. Summary

Recognition Contract định nghĩa cách Runtime đưa Image Artifact vào Recognition Module và nhận Candidate Artifact.

```text
RecognitionAttemptInput
        ↓
Recognition Module
        ↓
OCR Architecture
        ↓
RecognitionAttemptOutput
        ↓
CandidateRecognitionArtifact
        ↓
Runtime Authority Validation
        ↓
RecognitionArtifact
```

Essential guarantees:

```text
OCR Architecture
    owns OCR semantics.

Recognition Contract
    owns module boundary objects.

Runtime
    owns authority, retry and cancellation.

Artifact Store
    owns published artifact lifecycle.

Provider Integration
    owns provider-specific adaptation.
```

Contract quan trọng nhất là:

```text
RecognitionArtifact
    references OCR semantics.

RecognitionArtifact
    does not redefine OCR semantics.
```
