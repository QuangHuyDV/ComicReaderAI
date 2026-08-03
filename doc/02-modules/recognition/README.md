# Recognition Architecture

> Project: CRAI  
> Module: Recognition  
> Path: `modules/recognition/README.md`  
> Version: 1.0  
> Status: Architecture Overview

---

## 1. Purpose

Thư mục `modules/recognition/` định nghĩa kiến trúc của Recognition Module trong CRAI.

Recognition chuyển input dạng hình ảnh thành structured source content có thể được xử lý tiếp bởi Text Processing, Translation và Presentation.

Recognition chịu trách nhiệm trả lời các câu hỏi:

- vùng nào trong ảnh chứa văn bản;
- ký tự nào được nhận dạng;
- văn bản có hướng đọc nào;
- các vùng liên hệ với nhau ra sao;
- thứ tự đọc ban đầu là gì;
- kết quả có mức độ tin cậy như thế nào;
- kết quả ánh xạ ngược về source image ra sao.

Recognition không chỉ trả về một chuỗi OCR.

Đầu ra chính là một immutable `RecognitionArtifact` có text, geometry, ordering, confidence, provenance và traceability.

---

## 2. Module Position

```text
Source / Capture
        ↓
Observation
        ↓
Image Artifact
        ↓
Recognition
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

Nếu structured source text đã có sẵn từ browser DOM, clipboard hoặc document extraction, Runtime có thể bỏ qua Recognition.

---

## 3. Core Responsibility

Recognition chịu trách nhiệm:

- xác thực Recognition input;
- chuẩn bị ảnh cho Recognition execution;
- phát hiện vùng chứa văn bản;
- nhận dạng ký tự và dòng văn bản;
- giữ coordinate transformation;
- ánh xạ geometry về source coordinate space;
- chuẩn hóa provider output;
- xác định orientation và script/language hints;
- xây dựng initial reading order;
- đánh giá confidence;
- tạo immutable Candidate Recognition Artifact;
- cung cấp module-specific warnings và diagnostics;
- khai báo semantic compatibility cho Artifact reuse;
- cung cấp error và retry hint cho Runtime.

Recognition không sở hữu Runtime orchestration.

---

## 4. Explicit Non-Responsibilities

Recognition không chịu trách nhiệm:

- screen/window capture;
- browser DOM extraction;
- frame-change detection;
- stable-frame detection;
- Reading Session lifecycle;
- WorkItem scheduling;
- Queue admission;
- Runtime retry decision;
- Runtime authority;
- Artifact publication;
- cache-retention policy;
- durable persistence;
- semantic text correction;
- sentence segmentation;
- paragraph reconstruction;
- dialogue grouping;
- glossary application;
- translation;
- translated-text layout;
- overlay placement;
- UI rendering.

Recognition không gọi trực tiếp Translation hoặc Presentation.

---

## 5. Runtime Boundary

Recognition hoạt động bên trong một Runtime `Attempt`.

```text
Runtime WorkItem
        ↓
Recognition Attempt
        ↓
Recognition Module Execution
        ↓
Candidate Recognition Artifact
        ↓
Attempt Completion
        ↓
Runtime Authority Validation
        ↓
Ownership Transfer
        ↓
Recognition Artifact Publication
```

Recognition chỉ tạo Candidate Artifact.

Runtime Control quyết định Candidate còn authority hay không.

Artifact Store nhận ownership và thực hiện atomic publication.

---

## 6. Recognition Architecture Map

```text
                       Recognition Module

                               │

        ┌──────────────────────┼──────────────────────┐
        │                      │                      │

 Request and Plan       Recognition Pipeline    Provider Integration

        │                      │                      │
 Request Validator      Image Preparation       Capability Requirements
 Recognition Planner    Region Detection        Provider Adapter
 Profile Resolver       Text Recognition        Provider Normalization
                        Output Normalization     Provider Diagnostics
                        Reading Order
                        Artifact Assembly

                               │
                               ▼

                  Candidate Recognition Artifact
```

---

## 7. Main Processing Flow

```text
Recognition Attempt Input
        ↓
Validate Input Contract
        ↓
Build Recognition Plan
        ↓
Resolve Capability Requirements
        ↓
Prepare Image View
        ↓
Detect Text Regions
        ↓
Recognize Text
        ↓
Normalize Provider Output
        ↓
Map Geometry to Source Space
        ↓
Resolve Initial Reading Order
        ↓
Validate Output Invariants
        ↓
Create Candidate Recognition Artifact
        ↓
Submit Attempt Completion
```

Not every provider requires separate Detection and Recognition phases.

The plan may use:

```text
Combined Recognition
```

or:

```text
Detection Provider
        ↓
Recognition Provider
```

---

## 8. Recognition Artifact

Conceptual model:

```text
RecognitionArtifact
├── ArtifactIdentity
├── SourceArtifactRef
├── OwnerModule
├── ContractVersion
├── ContentIdentity
├── ProviderProvenance
├── SourceCoordinateSpace
├── ProcessedImageMetadata
├── CoordinateTransform
├── LanguageHypotheses
├── ScriptHypotheses
├── RecognizedRegions[]
├── ReadingOrder
├── Warnings[]
├── QualityMetadata
├── CompatibilityMetadata
└── TraceabilityMetadata
```

A published Recognition Artifact is immutable.

User corrections create a separate correction object or a newer derived Artifact.

Raw recognized text must not be overwritten silently.

---

## 9. Recognized Region

```text
RecognizedRegion
├── RegionId
├── Geometry
├── SourceGeometry
├── RawText
├── SurfaceNormalizedText
├── RecognizedLines[]
├── DetectionConfidence
├── RecognitionConfidence
├── Orientation
├── ReadingDirection
├── LanguageHint
├── ScriptHint
├── RegionType
├── Alternatives[]
├── Warnings[]
└── ProviderMetadata
```

`SurfaceNormalizedText` chỉ được phép thực hiện cleanup không thay đổi ý nghĩa.

Allowed examples:

- normalize line separator;
- trim outer whitespace;
- remove provider control characters;
- deterministic Unicode cleanup.

Not allowed:

- semantic OCR correction;
- name replacement;
- sentence reconstruction;
- guessing omitted words;
- translation.

---

## 10. Recognition Profiles

Recognition behavior có thể thay đổi theo profile:

```text
AUTOMATIC
COMIC_PAGE
SCREENSHOT
SINGLE_REGION
STRUCTURED_PAGE
```

Profile ảnh hưởng planning và capability requirement.

Profile không tạo contract output khác nhau.

### Automatic

Runtime/module chọn strategy theo input và provider capability.

### Comic Page

Ưu tiên:

- irregular regions;
- vertical text;
- text over artwork;
- multiple text areas;
- speech-bubble-like structures.

### Screenshot

Ưu tiên:

- horizontal interface text;
- browser/application labels;
- mixed structured regions.

### Single Region

Bỏ qua page-level detection khi phù hợp.

### Structured Page

Ưu tiên regular lines, columns và prose layout.

---

## 11. Provider Boundary

Recognition Module định nghĩa capability requirement.

Provider Manager và Provider Selection Policy chọn implementation phù hợp.

```text
Recognition Capability Requirement
        ↓
Provider Selection Policy
        ↓
Provider Manager
        ↓
Recognition Provider Adapter
```

Recognition không hard-code một OCR engine cụ thể.

Provider-specific SDK type phải nằm trong adapter.

---

## 12. Recognition Provider Capabilities

Possible capabilities:

```text
Supported Languages
Supported Scripts
Supported Orientations
Region Detection
Text Recognition
Combined Recognition
Confidence
Line Geometry
Character Geometry
Partial Output
Cancellation
Batching
CPU Execution
GPU Execution
Local Processing
Remote Processing
Maximum Image Size
Recommended Concurrency
Initialization Cost
```

Capability phải phản ánh behavior đã được kiểm chứng hoặc tài liệu hóa.

Không được khai báo capability chưa được validate.

---

## 13. Provider Privacy

Provider selection phải tuân thủ:

```text
LOCAL_ONLY
REMOTE_ALLOWED
EXACT_PROVIDER
PREFERRED_PROVIDER
AUTOMATIC
```

Rules:

- `LOCAL_ONLY` không bao giờ dùng remote provider;
- remote fallback phải được policy cho phép;
- provider credential không đi vào Recognition Artifact;
- remote execution phải observable;
- raw image không xuất hiện trong normal event/log.

---

## 14. Coordinate Model

Recognition có thể xử lý một image view khác source:

```text
Source Image
    ↓ Crop
    ↓ Resize
    ↓ Rotate
    ↓ Preprocess
Processed Recognition Image
```

Mọi geometry-changing operation phải đóng góp vào reversible transform chain.

Public geometry phải trở về source coordinate space.

---

## 15. Geometry

MVP có thể dùng:

```text
Rectangle
```

Architecture phải cho phép mở rộng sang:

```text
Polygon
```

Consumer không được giả định mọi region tương lai đều là axis-aligned rectangle.

---

## 16. Reading Order

Recognition chỉ tạo **initial spatial reading order**.

Reading order có thể dựa trên:

- provider order;
- spatial rules;
- orientation;
- reading direction;
- profile;
- mixed-layout heuristic.

Reading order entry phải giữ:

```text
OrderIndex
RegionId
Source
Confidence
ManualOverrideState
```

Array position không phải source of truth duy nhất.

Text Processing có thể tái cấu trúc semantic order ở bước sau nhưng phải giữ traceability.

---

## 17. Confidence

Provider confidence không được so sánh trực tiếp giữa các provider nếu chưa normalize.

```text
Confidence
├── RawValue
├── NormalizedValue
├── Level
├── Source
└── NormalizationMethod
```

Levels:

```text
UNKNOWN
LOW
MEDIUM
HIGH
```

Low confidence:

- không tự động discard text;
- có thể tạo warning;
- có thể tạo retry hint;
- không tự quyết định Runtime retry.

---

## 18. Warnings

Warnings mô tả degraded-but-usable output.

Examples:

```text
NO_READABLE_TEXT_DETECTED
LOW_DETECTION_CONFIDENCE
LOW_RECOGNITION_CONFIDENCE
READING_ORDER_UNCERTAIN
REGION_GEOMETRY_INFERRED
LINE_GEOMETRY_UNAVAILABLE
PROVIDER_CONFIDENCE_UNAVAILABLE
IMAGE_UPSCALED
IMAGE_DOWNSCALED
REMOTE_PROVIDER_USED
PARTIAL_RECOGNITION
PREPROCESSING_FALLBACK_USED
```

Warning không tạo terminal outcome riêng.

Runtime dùng:

```text
SUCCEEDED + warnings
```

---

## 19. Error Boundary

Recognition sở hữu semantic error codes của module.

Runtime Error Model sở hữu normalized RuntimeError contract.

Examples:

```text
RECOGNITION_INPUT_INVALID
RECOGNITION_IMAGE_UNSUPPORTED
RECOGNITION_LANGUAGE_UNSUPPORTED
RECOGNITION_IMAGE_TOO_LARGE
RECOGNITION_PREPROCESSING_FAILED
RECOGNITION_DETECTION_FAILED
RECOGNITION_TEXT_FAILED
RECOGNITION_COORDINATE_MAPPING_FAILED
RECOGNITION_READING_ORDER_FAILED
RECOGNITION_OUTPUT_INVALID
RECOGNITION_INTERNAL_ERROR
```

Provider-specific failures được Provider Adapter normalize thành Runtime provider errors.

Recognition chỉ cung cấp retry hint.

Runtime Retry Policy quyết định retry strategy.

---

## 20. Cancellation Boundary

Recognition hỗ trợ cooperative cancellation checkpoint:

- before expensive image preparation;
- before provider execution;
- between bounded region batches;
- after provider completion;
- before Candidate assembly;
- before Completion submission.

Recognition không sở hữu cancellation authority.

Runtime Control revoke authority.

Nếu provider không cancel được:

```text
Attempt becomes draining or abandoned
        ↓
Late output rejected by Runtime
        ↓
Resources released when physical execution ends
```

---

## 21. Authority and Publication

Recognition không quyết định input Revision còn current hay không.

Recognition giữ đầy đủ identity:

```text
SessionId
RevisionId
WorkItemId
AttemptId
SourceArtifactRef
ConfigurationSnapshotId
```

Runtime Control thực hiện authority validation.

Recognition không publish authoritative Artifact trực tiếp.

---

## 22. Artifact Reuse

Recognition định nghĩa semantic compatibility metadata.

Possible dependencies:

```text
InputContentIdentity
RecognitionContractVersion
RecognitionProfile
PreprocessingProfileVersion
DetectionModelVersion
RecognitionModelVersion
ProviderProfileVersion
LanguageHints
ScriptHints
OrientationHints
CoordinateTransformVersion
ConfigurationVersions
PrivacyPartition
```

Cache Policy quyết định reuse.

Artifact Store quản lý runtime retention.

Storage cung cấp durable persistence khi được phép.

---

## 23. Events

Recognition-specific facts có thể gồm:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_PREPROCESSING_COMPLETED
RECOGNITION_REGIONS_DETECTED
RECOGNITION_PROVIDER_COMPLETED
RECOGNITION_CANDIDATE_CREATED
RECOGNITION_WARNING_RECORDED
```

Attempt lifecycle events thuộc Runtime:

```text
ATTEMPT_STARTED
ATTEMPT_COMPLETED
ATTEMPT_FAILED
ATTEMPT_CANCELED
```

Artifact publication event thuộc Runtime/Artifact Store:

```text
ARTIFACT_PUBLISHED
```

Recognition event không cấp authority và không tạo hidden orchestration.

---

## 24. Data Ownership

Recognition owns:

- Recognition domain contract;
- normalized provider output;
- recognized source text;
- region/line geometry;
- initial reading order;
- quality metadata;
- module warnings;
- semantic compatibility rules;
- module-specific diagnostics.

Recognition does not own:

- source image lifecycle;
- WorkItem/Attempt lifecycle;
- Runtime authority;
- published payload ownership;
- cache retention;
- durable persistence;
- Session state;
- corrected semantic text;
- translation output;
- UI state.

Artifact Store owns published Artifact payload.

---

## 25. Dependencies

Allowed dependencies:

```text
shared-kernel
image-primitives
geometry-primitives
provider-contracts
runtime-contracts
artifact-contracts
configuration-contracts
security-contracts
diagnostics-contracts
```

Forbidden direct dependencies:

```text
translation implementation
text-processing implementation
presentation implementation
desktop UI
browser extension
capture implementation
observation implementation
storage implementation
glossary implementation
reading-history implementation
```

Recognition integrates through contracts, not internal implementations.

---

## 26. Concurrency

Recognition concurrency is controlled by Runtime Scheduler and Provider Manager.

Rules:

- UI Context never runs Recognition;
- provider concurrency respects declared capacity;
- GPU/native provider may be serial;
- region-level parallelism bounded;
- Worker owns only Attempt-local resource;
- shared image/Artifact uses Resource Lease;
- provider initialization is not request-scoped;
- image copies minimized;
- obsolete work loses authority quickly.

---

## 27. Resource Lifecycle

```text
Input Artifact Lease
        ↓
Attempt-Local Image Views
        ↓
Provider Request Resources
        ↓
Candidate Recognition Artifact
        ↓
Ownership Transfer or Candidate Cleanup
        ↓
Release Leases and Temporary Resources
```

Recognition must not dispose shared input Artifact.

Provider-lifetime resource belongs to Provider Manager.

---

## 28. Observability

Recognition should expose:

- plan duration;
- preprocessing duration;
- detection duration;
- recognition duration;
- coordinate mapping duration;
- ordering duration;
- Candidate assembly duration;
- input-size class;
- region count;
- character count;
- provider/profile;
- execution class;
- warning count;
- Candidate rejection reason;
- cancellation checkpoint;
- quality buckets.

Normal telemetry không chứa raw image hoặc recognized text.

---

## 29. Performance Goal

Recognition performance được đánh giá bằng contribution tới current useful result.

Không chỉ đo provider throughput.

Important metrics:

```text
Recognition Attempt Latency
Time to First Region
Current-Revision Acceptance Ratio
Stale Recognition Ratio
Provider Queue Wait
Resource/Lease Wait
Candidate Assembly Time
Recognition Reuse Benefit
```

---

## 30. MVP Scope

Recognition MVP nên hỗ trợ:

- full-image recognition;
- selected-region recognition;
- Simplified Chinese;
- Traditional Chinese nếu provider hỗ trợ;
- English;
- horizontal text;
- basic vertical Chinese;
- rectangular region geometry;
- combined provider strategy;
- optional composed detection + recognition;
- source coordinate mapping;
- basic reading order;
- confidence normalization;
- warnings;
- cancellation checkpoints;
- immutable Candidate Artifact;
- local provider first when feasible;
- optional remote provider under explicit policy.

MVP chưa cần:

- character-level geometry;
- advanced bubble classification;
- universal comic layout;
- semantic OCR repair;
- handwriting;
- live per-character streaming;
- distributed recognition;
- learned reading-order model;
- permanent raw-image history.

---

## 31. Recognition Documents

Recommended structure:

```text
recognition/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── PIPELINE.md
├── PROVIDER.md
├── PREPROCESSING.md
├── REGION_DETECTION.md
├── TEXT_RECOGNITION.md
├── COORDINATE_MODEL.md
├── READING_ORDER.md
├── QUALITY_MODEL.md
├── EVENTS.md
├── ERRORS.md
├── CONFIG.md
├── OBSERVABILITY.md
└── TESTING.md
```

Không nhất thiết tạo tất cả file ngay trong MVP.

---

## 32. Recommended Reading Order

```text
README
    ↓
MODULE
    ↓
CONTRACT
    ↓
PIPELINE
    ↓
PROVIDER
    ↓
PREPROCESSING
    ↓
REGION_DETECTION
    ↓
TEXT_RECOGNITION
    ↓
COORDINATE_MODEL
    ↓
READING_ORDER
    ↓
QUALITY_MODEL
    ↓
ERRORS
    ↓
EVENTS
    ↓
CONFIG
    ↓
OBSERVABILITY
    ↓
TESTING
```

---

## 33. Architecture Invariants

1. Recognition transforms image input into structured source content.
2. Recognition does not translate.
3. Recognition does not perform semantic text correction.
4. Recognition only creates Candidate Artifact.
5. Runtime Control owns authority.
6. Artifact Store owns published payload.
7. Recognition Attempt uses immutable input.
8. Published Recognition Artifact is immutable.
9. Geometry maps back to source coordinate space.
10. Coordinate-changing preprocessing preserves transform chain.
11. Reading order is explicit, not implied only by array position.
12. Provider SDK types remain in adapters.
13. Provider capability must be validated.
14. Local-only input never uses remote provider.
15. Provider failure does not trigger hidden retry.
16. Recognition only provides retry hint.
17. Runtime Retry Policy decides retry.
18. Cancellation is cooperative.
19. Late provider output does not gain authority.
20. Warnings remain separate from failures.
21. Empty text can be valid success.
22. Confidence is provider-aware.
23. Cache compatibility belongs to Recognition semantics.
24. Cache retention belongs to Runtime.
25. Durable persistence belongs to Storage.
26. Normal telemetry contains no image/text payload.
27. Worker does not own shared input payload.
28. Attempt-local resource is released deterministically.
29. Recognition does not call Translation or Presentation.
30. Recognition contracts remain provider-independent.

---

## 34. Relationship With Text Processing

```text
Recognition Artifact
        ↓
Text Processing
        ↓
Prepared Source Artifact
```

Recognition owns visual extraction:

- text regions;
- geometry;
- raw source text;
- orientation;
- initial spatial order;
- confidence.

Text Processing owns semantic preparation:

- conservative normalization;
- line merging;
- paragraph/dialogue structure;
- translation units;
- local context;
- source-to-prepared mapping.

Recognition must not absorb Text Processing responsibilities.

---

## 35. Summary

Recognition is CRAI's image-to-structured-source module.

```text
Image Artifact
    ↓
Recognition Plan
    ↓
Detection and Recognition
    ↓
Geometry and Order
    ↓
Candidate Recognition Artifact
    ↓
Runtime Validation and Publication
```

Core boundary:

```text
Recognition owns visual extraction semantics.

Runtime owns execution authority and publication.

Artifact Store owns accepted shared payload.

Text Processing owns semantic preparation for Translation.
```
