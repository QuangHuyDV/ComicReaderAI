# Recognition Module

> **Project:** CRAI
> **Module:** Recognition
> **Path:** `doc/02-modules/recognition/README.md`
> **Version:** 1.1
> **Status:** Architecture Overview

---

# 1. Purpose

Thư mục `02-modules/recognition/` định nghĩa Recognition Module của CRAI.

Recognition Module chịu trách nhiệm điều phối quá trình biến input dạng hình ảnh thành một module-level Candidate Artifact chứa structured source content.

Recognition không tự định nghĩa toàn bộ OCR semantics.

Các semantics như:

* Image Preprocessing
* Text Detection
* Text Recognition
* Text Direction
* Layout
* OCR Document
* Quality Assessment
* Reading Order
* OCR Provider abstraction

được định nghĩa authoritative tại:

```text
01-architecture/ocr/
```

Recognition Module sử dụng các contract đó để thực hiện business/module orchestration.

---

# 2. Module Position

```text
Source / Capture
      ↓
Observation
      ↓
Image Artifact
      ↓
Runtime WorkItem / Attempt
      ↓
Recognition Module
      ↓
Candidate Recognition Artifact
      ↓
Runtime Authority Validation
      ↓
Recognition Artifact
      ↓
Text Processing
      ↓
Translation
      ↓
Presentation
```

Recognition chỉ tham gia image-based content path.

Nếu structured source text đã tồn tại từ:

* browser DOM
* clipboard
* imported structured document
* direct text source

Runtime có thể bypass Recognition.

---

# 3. Core Responsibility

Recognition Module chịu trách nhiệm:

* validate Recognition Attempt Input
* build immutable Recognition Plan
* declare OCR capability requirements
* coordinate canonical OCR Architecture
* receive/reference OCR Document
* receive/reference optional Quality Report
* receive/reference optional Reading Order Result
* assemble Candidate Recognition Artifact
* provide module warnings/errors
* provide RetryHint
* define semantic compatibility metadata
* provide module diagnostics
* submit Candidate through Runtime completion boundary

Recognition không định nghĩa lại:

* Region semantics
* Geometry semantics
* Character/Word/Line/Paragraph semantics
* Writing Direction
* Layout Tree
* Quality Score/Grade
* Reading Order Graph
* Provider capability semantics

---

# 4. Explicit Non-Responsibilities

Recognition không chịu trách nhiệm:

* screen/window capture
* browser DOM extraction
* frame-change detection
* stable-frame detection
* Reading Session lifecycle
* WorkItem lifecycle
* Attempt lifecycle
* Scheduler admission
* Work Queue
* Runtime retry execution
* cancellation authority
* Runtime authority
* Artifact publication
* Artifact retention
* cache retention policy
* durable persistence
* Provider lifecycle
* Provider credential management
* semantic OCR correction
* sentence reconstruction
* translation segmentation
* glossary application
* Translation
* translated-text layout
* overlay placement
* UI rendering

Recognition không gọi trực tiếp Translation hoặc Presentation implementation.

---

# 5. Runtime Boundary

Recognition hoạt động bên trong một Runtime Attempt.

```text
Runtime WorkItem
      ↓
Recognition Attempt
      ↓
Recognition Module
      ↓
Candidate Recognition Artifact
      ↓
Attempt Completion
      ↓
Runtime Authority Validation
      ↓
Artifact Store Ownership Transfer
      ↓
Recognition Artifact Publication
```

Recognition chỉ tạo Candidate.

Runtime Control quyết định Candidate còn authority hay không.

Artifact Store sở hữu accepted published Artifact lifecycle.

---

# 6. Recognition Architecture Map

```text
                    Recognition Module
                           │
          ┌────────────────┼────────────────┐
          │                │                │
          ▼                ▼                ▼
     Module Input      OCR Coordination   Module Output
          │                │                │
   Input Validation   OCR Architecture   Candidate Assembly
   Recognition Plan   Contracts           Warnings / Errors
   Capability Req.    OCR Document        Compatibility
   Privacy Context    Quality Report?     Diagnostics
                      Reading Order?
                           │
                           ▼
               Candidate Recognition Artifact
```

OCR implementation details không nằm trong module README.

---

# 7. OCR Architecture Relationship

Canonical OCR flow:

```text
Image
   ↓
Preprocessing
   ↓
Detection
   ↓
Recognition
   ↓
Text Direction
   ↓
Layout
   ↓
Postprocessing
   ↓
OCR Document
   ↓
Quality Assessment
   ↓
Reading Order
```

Recognition Module điều phối flow này nhưng không sở hữu semantics của từng stage.

Authoritative owners:

| Concern            | Owner                                   |
| ------------------ | --------------------------------------- |
| OCR Pipeline       | `01-architecture/ocr/PIPELINE.md`       |
| Preprocessing      | `01-architecture/ocr/PREPROCESS.md`     |
| Detection / Region | `01-architecture/ocr/DETECTION.md`      |
| Text Recognition   | `01-architecture/ocr/RECOGNITION.md`    |
| Text Direction     | `01-architecture/ocr/TEXT_DIRECTION.md` |
| Layout             | `01-architecture/ocr/LAYOUT.md`         |
| OCR Document       | `01-architecture/ocr/POSTPROCESS.md`    |
| Quality            | `01-architecture/ocr/QUALITY.md`        |
| Reading Order      | `01-architecture/ocr/READING_ORDER.md`  |
| OCR Providers      | `01-architecture/ocr/PROVIDERS.md`      |

---

# 8. Main Module Flow

```text
RecognitionAttemptInput
      ↓
Validate Module Contract
      ↓
Build RecognitionPlan
      ↓
Resolve Capability Requirements
      ↓
Acquire Input Artifact Lease
      ↓
Execute OCR Architecture
      ↓
Receive OCRDocumentRef
      ↓
Receive optional QualityReportRef
      ↓
Receive optional ReadingOrderResultRef
      ↓
Validate Module Requirements
      ↓
Assemble CandidateRecognitionArtifact
      ↓
Submit Attempt Completion
```

OCR sub-stage execution detail belongs to `01-architecture/ocr/PIPELINE.md`.

---

# 9. Recognition Attempt Input

Conceptual boundary:

```text
RecognitionAttemptInput
├── RuntimeContext
├── InputArtifactRef
├── RecognitionOperation
├── RecognitionProfile
├── RegionSelection?
├── RecognitionOptions
├── CapabilityRequirements
├── ExecutionContextRef
├── CancellationContextRef
├── PrivacyContextRef
└── DiagnosticsContextRef?
```

Exact contract:

```text
CONTRACT.md
```

---

# 10. Recognition Operations

Supported operation model:

```text
RECOGNIZE_IMAGE
RECOGNIZE_REGION
EVALUATE_IMAGE
```

## RECOGNIZE_IMAGE

Process complete image input.

---

## RECOGNIZE_REGION

Process selected source-space region.

---

## EVALUATE_IMAGE

Diagnostic/evaluation execution.

Không mặc định tạo published user-facing Artifact.

---

# 11. Recognition Profiles

```text
AUTOMATIC
COMIC_PAGE
SCREENSHOT
SINGLE_REGION
STRUCTURED_PAGE
```

Profile ảnh hưởng:

* planning
* OCR Profile selection/reference
* capability requirements
* output requirements

Profile không thay đổi Recognition Artifact public contract.

---

# 12. Recognition Plan

Recognition tạo immutable:

```text
RecognitionPlan
```

Plan có thể chứa:

* RecognitionOperation
* RecognitionProfile
* OCRProfileRef
* CapabilityRequirements
* execution strategy
* compatibility policy
* Configuration Snapshot
* Privacy Constraints

Plan không chứa:

* Provider credentials
* Runtime priority mutation
* retry budget
* Runtime authority

---

# 13. OCR Execution Strategy

Recognition có thể điều phối:

## Combined OCR

```text
Image
    ↓
Combined OCR Provider
    ↓
CRAI OCR Contracts
```

## Composed OCR

```text
Image
    ↓
Detection Capability
    ↓
Recognition Capability
    ↓
Direction / Layout / Postprocessing
```

Cả hai strategy phải produce cùng canonical OCR contracts.

---

# 14. Recognition Artifact

Recognition Artifact là module-level published Artifact.

Conceptually:

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

Recognition Artifact không embed lại toàn bộ OCR model nếu reference là đủ.

---

# 15. Recognition Artifact vs OCR Document

Hai concept phải tách rõ.

```text
OCR Document
    = canonical structured OCR result

Recognition Artifact
    = module-level publication object
      referencing OCR results
```

Recognition Artifact không redefine:

* Region
* Character
* Word
* Line
* Paragraph
* Direction
* Layout
* Reading Order

---

# 16. Candidate Recognition Artifact

Recognition trước tiên tạo:

```text
CandidateRecognitionArtifact
```

Candidate:

* immutable sau validation
* non-authoritative
* chưa published
* chưa thuộc shared Artifact lifecycle
* có thể bị Runtime reject
* phải cleanup nếu rejected

Publication chỉ xảy ra sau Runtime authority validation.

---

# 17. Completeness

Recognition owns module-level completeness:

```text
COMPLETE
PARTIAL
EMPTY_VALID
UNKNOWN
```

`EMPTY_VALID` nghĩa là:

```text
OCR execution valid
but no readable text found
```

Không phải failure.

---

# 18. Warnings

Recognition owns module-level warnings như:

```text
NO_READABLE_TEXT_DETECTED
PARTIAL_RECOGNITION
REMOTE_PROVIDER_USED
FALLBACK_PROVIDER_USED
OCR_RESULT_DEGRADED
QUALITY_BELOW_PREFERRED_LEVEL
```

OCR-stage warnings như:

```text
LOW_DETECTION_CONFIDENCE
READING_ORDER_UNCERTAIN
```

thuộc owner OCR tương ứng.

Recognition chỉ surface/reference chúng khi cần.

---

# 19. Error Boundary

Recognition owns only module-boundary errors.

Examples:

```text
RECOGNITION_INPUT_INVALID
RECOGNITION_PLAN_INVALID
RECOGNITION_CAPABILITY_UNAVAILABLE
RECOGNITION_OCR_EXECUTION_FAILED
RECOGNITION_CANDIDATE_INVALID
RECOGNITION_RESOURCE_EXHAUSTED
RECOGNITION_PRIVACY_CONFLICT
RECOGNITION_INTERNAL_ERROR
```

Recognition không redefine:

* Detection errors
* Text Recognition errors
* Direction errors
* Layout errors
* Quality errors
* Reading Order errors
* Provider lifecycle errors

Chi tiết:

```text
ERRORS.md
```

---

# 20. Provider Boundary

Recognition chỉ khai báo:

```text
RecognitionCapabilityRequirements
```

Provider semantics thuộc:

```text
01-architecture/ocr/PROVIDERS.md
```

Provider Manager/Selection Policy chịu trách nhiệm lựa chọn executable provider path.

Recognition không hard-code OCR engine.

Provider SDK type không crossing Adapter boundary.

---

# 21. Provider Privacy

Provider selection phải tuân Privacy Context.

Ví dụ:

```text
LOCAL_ONLY
STANDARD
EPHEMERAL
```

Rules:

* local-only không dùng remote provider
* remote fallback phải explicit policy
* credentials không vào Artifact
* remote execution traceable
* raw image không vào normal event/log

---

# 22. Geometry and Coordinate Boundary

Recognition Module không sở hữu Geometry model.

Geometry semantics thuộc:

```text
01-architecture/ocr/DETECTION.md
```

Preprocessing transform semantics thuộc:

```text
01-architecture/ocr/PREPROCESS.md
```

Recognition chỉ đảm bảo OCR artifacts giữ đủ lineage để map visual entities về source image.

---

# 23. Reading Order Boundary

Reading Order semantics thuộc:

```text
01-architecture/ocr/READING_ORDER.md
```

Recognition có thể require hoặc reference:

```text
ReadingOrderResultRef
```

Recognition không define ReadingOrderEntry/Direction enum riêng.

---

# 24. Confidence and Quality Boundary

Confidence semantics thuộc component tạo ra confidence.

Quality semantics thuộc:

```text
01-architecture/ocr/QUALITY.md
```

Recognition không define generic Confidence model hoặc Quality Grade.

Recognition có thể reference:

```text
QualityReportRef
```

và expose module-level warning nếu quality không đạt preferred level.

---

# 25. Cancellation Boundary

Recognition cooperative-check Runtime cancellation tại các checkpoint như:

* before input acquisition
* before OCR execution
* between bounded OCR work where supported
* before Candidate assembly
* before Candidate submission

Recognition không sở hữu cancellation authority.

Nếu Provider không physically cancel được:

```text
Runtime revokes authority
      ↓
Provider may finish late
      ↓
Late output rejected
      ↓
Resources released
```

---

# 26. Authority and Publication

Recognition giữ trace identity:

```text
SessionId?
RevisionId
WorkItemId
AttemptId
InputArtifactRef
ConfigurationSnapshotId
```

Nhưng Recognition không quyết định input Revision còn current hay không.

Runtime Control performs authority validation.

Artifact Store performs ownership transfer/publication.

---

# 27. Artifact Reuse

Recognition defines semantic compatibility metadata.

Possible dependencies:

```text
InputContentIdentity
RecognitionContractVersion
RecognitionProfileVersion
OCRPipelineVersion
OCRProfileVersion
ProviderProfileVersion
LanguageHints
ScriptHints
ConfigurationVersions
PrivacyPartition
OCR Semantic Dependencies
```

Runtime Cache Policy quyết định reuse.

Artifact Store quản lý runtime Artifact lifecycle.

Storage cung cấp durable persistence khi được phép.

---

# 28. Events

Recognition-specific module facts:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_OCR_EXECUTION_COMPLETED
RECOGNITION_CANDIDATE_VALIDATED
RECOGNITION_CANDIDATE_SUBMITTED
RECOGNITION_WARNING_RECORDED
RECOGNITION_MODULE_ERROR_RECORDED
```

Optional diagnostics:

```text
RECOGNITION_PHASE_CHANGED
RECOGNITION_CANCELLATION_OBSERVED
RECOGNITION_DEADLINE_OBSERVED
```

Recognition không emit OCR-stage mirror events.

Chi tiết:

```text
EVENTS.md
```

---

# 29. State Model

Recognition-owned states:

```text
RecognitionAvailabilityState
RecognitionPlanState
RecognitionOperationPhase
CandidateValidationState
RecognitionCompleteness
```

Recognition không own:

* AttemptState
* WorkItemState
* RetryState
* QualityState
* ReadingOrderState
* ProviderLifecycleState

Chi tiết:

```text
STATES.md
```

---

# 30. Data Ownership

Recognition owns:

* module boundary
* RecognitionAttemptInput/Output
* RecognitionPlan
* Candidate Recognition Artifact
* Recognition Artifact contract
* module completeness
* module warnings/errors
* RetryHint
* semantic compatibility
* module diagnostics

Recognition does not own:

* OCR Region semantics
* OCR text hierarchy
* Text Direction
* Layout Tree
* OCR Document semantics
* Quality semantics
* Reading Order semantics
* Provider lifecycle
* Runtime authority
* Artifact retention
* Storage
* Translation output
* UI state

---

# 31. Dependencies

Allowed categories:

```text
shared-kernel
runtime-contracts
artifact-contracts
ocr-architecture-contracts
provider-contracts
configuration-contracts
image-primitives
geometry-primitives
security-contracts
diagnostics-contracts
```

Forbidden direct implementation dependencies:

```text
translation implementation
text-processing implementation
presentation implementation
desktop UI
browser extension
capture implementation
observation implementation
storage implementation
scheduler implementation
provider SDK outside Adapter
```

---

# 32. Concurrency

Recognition concurrency do Runtime Scheduler và Provider Manager control.

Rules:

* UI context không chạy Recognition
* provider concurrency bounded
* region-level parallelism bounded
* shared Artifact accessed via lease
* Attempt-local resources cleaned deterministically
* Provider lifecycle not request-scoped
* obsolete work loses authority
* Candidate submitted at most once

---

# 33. Resource Lifecycle

```text
Input Artifact Lease
      ↓
Attempt-Local OCR Resources
      ↓
Candidate Recognition Artifact
      ↓
Ownership Transfer or Cleanup
      ↓
Release Attempt-Local Resources
      ↓
Release Input Lease
```

Recognition không dispose shared input Artifact.

Provider-lifetime resources thuộc Provider Manager/Resource Manager.

---

# 34. Observability

Recognition-level observability nên tập trung:

* plan duration
* OCR execution duration
* Candidate assembly duration
* Candidate validation duration
* total module latency
* warning/error count
* completeness
* Provider profile
* execution class
* Candidate disposition correlation

OCR-stage detailed metrics thuộc owner tương ứng.

Normal telemetry không chứa raw image hoặc recognized text.

---

# 35. Performance Perspective

Recognition performance được đánh giá theo contribution tới useful current result.

Relevant module/runtime metrics:

```text
Recognition Attempt Latency
Candidate Assembly Time
Current-Revision Acceptance Ratio
Stale Recognition Ratio
Resource / Lease Wait
Recognition Reuse Benefit
```

Provider-level throughput/latency metrics thuộc Provider/Telemetry boundaries.

---

# 36. MVP Scope

Recognition MVP nên hỗ trợ:

* full-image Recognition
* selected-region Recognition
* Simplified Chinese
* Traditional Chinese when supported
* English
* horizontal text
* basic vertical text
* canonical OCR Document reference
* basic Reading Order result/reference
* basic Quality Report/reference
* immutable Candidate Artifact
* module warnings/errors
* cancellation checkpoints
* semantic compatibility
* local provider path when feasible
* optional remote provider by explicit policy

MVP không cần:

* handwriting
* semantic OCR correction
* universal comic-layout understanding
* live per-character streaming
* distributed Recognition
* learned provider routing
* permanent raw-image history

---

# 37. Recognition Module Documents

Canonical module document set:

```text
recognition/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
└── ERRORS.md
```

OCR-specific architecture files **không được tạo lại** trong `02-modules/recognition/`.

Các file như:

```text
PIPELINE.md
PREPROCESSING.md
REGION_DETECTION.md
TEXT_RECOGNITION.md
COORDINATE_MODEL.md
READING_ORDER.md
QUALITY_MODEL.md
PROVIDER.md
```

đã được thay thế bởi authoritative documents tại:

```text
01-architecture/ocr/
```

---

# 38. Recommended Reading Order

Module docs:

```text
README
   ↓
MODULE
   ↓
CONTRACT
   ↓
STATES
   ↓
EVENTS
   ↓
ERRORS
```

Nếu cần hiểu OCR internals:

```text
01-architecture/ocr/README.md
        ↓
01-architecture/ocr/PIPELINE.md
        ↓
specialized OCR documents
```

Nếu cần Runtime semantics:

```text
01-architecture/runtime/
```

---

# 39. Architecture Invariants

1. Recognition transforms image-based input into module-level structured-source Candidate.

2. Recognition does not translate.

3. Recognition does not perform semantic text correction.

4. Recognition creates Candidate only.

5. Runtime Control owns authority.

6. Artifact Store owns accepted published payload.

7. Recognition Attempt uses immutable input.

8. Published Recognition Artifact is immutable.

9. OCR geometry remains source-traceable.

10. OCR semantic ownership remains in `01-architecture/ocr/`.

11. Recognition does not redefine Region.

12. Recognition does not redefine Recognition text hierarchy.

13. Recognition does not redefine Text Direction.

14. Recognition does not redefine Layout.

15. Recognition does not redefine Quality.

16. Recognition does not redefine Reading Order.

17. Provider SDK types remain inside adapters.

18. Provider capability requirements are explicit.

19. Local-only input never uses remote Provider.

20. Provider failure does not trigger hidden retry.

21. Recognition only provides RetryHint.

22. Runtime Retry Policy decides retry.

23. Cancellation is cooperative.

24. Late Provider output does not regain authority.

25. Warning remains separate from ModuleError.

26. Empty text can be valid success.

27. Compatibility belongs to Recognition module semantics.

28. Cache retention belongs to Runtime.

29. Durable persistence belongs to Storage.

30. Normal telemetry contains no image/text payload.

31. Worker does not own shared input payload.

32. Attempt-local resources release deterministically.

33. Recognition does not call Translation or Presentation.

34. Recognition contracts remain Provider-independent.

35. OCR Architecture remains single source of truth for OCR concepts.

---

# 40. Relationship With Text Processing

```text
Recognition Artifact
      ↓
Text Processing
      ↓
Prepared Source Artifact
```

Recognition supplies structured visual-source results through OCR artifact references.

Text Processing owns:

* semantic source normalization
* line/paragraph reconstruction where needed
* translation units
* local context
* source-to-prepared mapping

Recognition không absorb Text Processing responsibilities.

---

# 41. Related Documents

```text
doc/02-modules/recognition/MODULE.md
doc/02-modules/recognition/CONTRACT.md
doc/02-modules/recognition/STATES.md
doc/02-modules/recognition/EVENTS.md
doc/02-modules/recognition/ERRORS.md

doc/01-architecture/OWNERSHIP_MAP.md

doc/01-architecture/ocr/README.md
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

doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/CACHE_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md
```

---

# 42. Summary

Recognition là CRAI module chịu trách nhiệm điều phối image-based structured-source processing.

```text
Image Artifact
      ↓
Recognition Attempt
      ↓
Recognition Plan
      ↓
OCR Architecture
      ↓
OCR Document
      ↓
Quality / Reading Order
      ↓
Candidate Recognition Artifact
      ↓
Runtime Validation
      ↓
Published Recognition Artifact
```

Ownership boundary:

```text
OCR Architecture
    owns OCR semantics.

Recognition Module
    owns orchestration and module boundary.

Runtime
    owns execution authority, retry and cancellation.

Artifact Store
    owns accepted shared payload lifecycle.

Provider Integration
    owns Provider-specific integration.

Text Processing
    owns semantic preparation after Recognition.
```

The key rule is:

```text
Recognition coordinates OCR.

Recognition does not redefine OCR.
```
