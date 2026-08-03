# Recognition Module Specification

> Project: CRAI  
> Module: Recognition  
> Path: `modules/recognition/MODULE.md`  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Module Definition

Recognition là Business Processing Module chịu trách nhiệm chuyển image-based input thành structured, spatially aligned source content.

Module boundary bắt đầu khi Runtime cung cấp một immutable Recognition Attempt Input hợp lệ.

Module boundary kết thúc khi Recognition tạo:

- Candidate Recognition Artifact;
- module warnings;
- normalized module error hoặc retry hint;
- module-specific diagnostics;
- Attempt Completion để Runtime đánh giá.

Recognition không trực tiếp tạo authoritative published result.

```text
Image Artifact
    ↓
Recognition Execution
    ↓
Candidate Recognition Artifact
    ↓
Runtime Authority Validation
    ↓
Artifact Store Publication
```

Recognition là image-to-structured-source module.

Nó không phải:

- Capture Module;
- Observation Module;
- Text Processing Module;
- Translation Module;
- Presentation Module;
- Runtime Controller;
- Provider Manager;
- Storage Module.

---

## 2. Module Identity

```text
Module ID: recognition
Module Type: Core Business Processing Module
Primary Domain: Image Text Recognition
Execution Model: Runtime WorkItem / Attempt
Primary Input: Image Artifact Reference
Primary Output: Candidate Recognition Artifact
Published Output: Recognition Artifact
State Ownership: Recognition semantic state only
MVP Priority: Required for image-reading flow
```

Recognition có thể bị bypass khi structured source text đã có sẵn từ:

- browser DOM extraction;
- clipboard text;
- imported structured document;
- direct user text input.

---

## 3. Architectural Position

```text
Source / Capture
        ↓
Observation
        ↓
Image Artifact
        ↓
Runtime WorkItem
        ↓
Recognition Attempt
        ↓
Candidate Recognition Artifact
        ↓
Runtime Validation and Publication
        ↓
Recognition Artifact
        ↓
Text Processing
```

Recognition không tự tạo downstream WorkItem.

Runtime Control và Business Pipeline Orchestration chịu trách nhiệm điều phối bước tiếp theo.

---

## 4. Problem Statement

Image-based reading content thường không cung cấp structured text đáng tin cậy.

Examples:

- manhua;
- manga;
- manhwa;
- screenshots;
- canvas-based readers;
- scanned documents;
- image-only PDFs;
- embedded page images.

CRAI cần xác định:

1. vùng nào chứa text;
2. ký tự nào xuất hiện;
3. geometry của từng vùng;
4. orientation và reading direction;
5. region relationships;
6. initial reading order;
7. confidence và uncertainty;
8. mapping về source coordinate space;
9. provider provenance và compatibility metadata.

Recognition cung cấp các thông tin này mà không cố diễn giải hoặc dịch nội dung.

---

## 5. Responsibilities

Recognition sở hữu các trách nhiệm semantic sau.

### 5.1 Input Contract Validation

Validate:

- InputArtifactRef tồn tại;
- Artifact type được hỗ trợ;
- image dimensions;
- image format;
- crop bounds;
- coordinate-space metadata;
- Recognition Profile;
- language/script hints;
- orientation hint;
- capability requirements;
- Recognition configuration compatibility.

Runtime identity và authority được Runtime Control validate.

### 5.2 Recognition Planning

Build a Recognition Plan dựa trên:

- input characteristics;
- Recognition Profile;
- provider capabilities;
- language/script hints;
- orientation hints;
- preprocessing requirements;
- device/resource constraints;
- privacy policy;
- configuration snapshot.

### 5.3 Image Preparation

Prepare an Attempt-local image view thông qua:

- normalization;
- resizing;
- upscaling;
- grayscale conversion;
- contrast adjustment;
- denoising;
- sharpening;
- deskewing;
- thresholding;
- rotation;
- inversion;
- crop/padding;
- provider-specific preparation.

### 5.4 Text Region Detection

Identify likely text-containing regions.

Detection output có thể gồm:

- RegionId;
- geometry;
- detection confidence;
- orientation;
- script hint;
- region classification;
- detector provenance.

### 5.5 Text Recognition

Convert region image content thành source-language characters.

Output có thể gồm:

- region text;
- line text;
- word/character data;
- recognition confidence;
- alternatives;
- orientation metadata;
- provider provenance.

### 5.6 Coordinate Mapping

Maintain transform chain và map public geometry về source coordinate space.

### 5.7 Initial Reading Order

Create spatial reading-order proposal dựa trên:

- provider order;
- top-to-bottom rules;
- left-to-right rules;
- right-to-left rules;
- vertical-column rules;
- orientation;
- mixed-layout heuristics;
- Recognition Profile.

### 5.8 Provider Output Normalization

Convert provider-specific output thành provider-independent Recognition domain models.

### 5.9 Candidate Artifact Assembly

Assemble immutable Candidate Recognition Artifact.

### 5.10 Semantic Compatibility

Define metadata cần thiết để xác định hai Recognition Artifact có semantically reusable hay không.

### 5.11 Module Diagnostics

Expose:

- operation timing;
- warning;
- quality metadata;
- provider provenance;
- plan decision;
- region count;
- output completeness;
- module error classification.

---

## 6. Non-Responsibilities

Recognition không sở hữu:

- screen/window capture;
- file discovery;
- browser-page extraction;
- source observation;
- frame-change detection;
- stable-frame detection;
- scroll detection;
- Reading Session lifecycle;
- Revision authority;
- WorkItem lifecycle;
- Attempt lifecycle;
- Scheduler admission;
- Work Queue;
- Runtime retry decision;
- global cancellation authority;
- Artifact publication;
- Artifact payload ownership sau publication;
- Cache Policy;
- cache retention;
- durable persistence;
- linguistic normalization;
- semantic OCR correction;
- sentence segmentation;
- paragraph reconstruction;
- dialogue grouping;
- glossary application;
- translation;
- translated-text layout;
- overlay placement;
- user-history storage.

Recognition không trực tiếp gọi Translation hoặc Presentation.

---

## 7. Runtime Boundary

Recognition chạy trong một Runtime Attempt.

```text
Runtime creates WorkItem
        ↓
Scheduler admits Attempt
        ↓
Worker receives RecognitionAttemptInput
        ↓
Recognition executes
        ↓
CandidateRecognitionArtifact created
        ↓
AttemptCompletion submitted
        ↓
Runtime validates authority
        ↓
Artifact Store accepts ownership
        ↓
RecognitionArtifact published
```

Recognition không:

- grant authority;
- decide current Revision;
- publish accepted Artifact;
- schedule downstream WorkItem;
- retry itself;
- mutate Runtime state.

---

## 8. Recognition Attempt Input

```text
RecognitionAttemptInput
├── SessionId?
├── RevisionId
├── WorkItemId
├── AttemptId
├── InputArtifactRef
├── RecognitionOperation
├── RecognitionProfile
├── CapabilityRequirements
├── LanguageHints[]
├── ScriptHints[]
├── OrientationHint?
├── ReadingDirectionHint?
├── RegionSelection?
├── ConfigurationSnapshotId
├── PrivacyContextRef
├── ExecutionContextRef
├── CancellationContextRef
└── TraceContext
```

### Input Rules

1. Input Artifact immutable.
2. Recognition không nhận raw mutable image object qua module boundary.
3. RegionSelection phải dùng source coordinate space.
4. Priority và queue metadata không thuộc Recognition input contract.
5. Provider credential không nằm trong input.
6. Timeout được Runtime execution context cung cấp.
7. SessionId có thể absent cho imported standalone image.

---

## 9. Recognition Operations

```text
RecognitionOperation
├── RECOGNIZE_IMAGE
├── RECOGNIZE_REGION
└── EVALUATE_IMAGE
```

### RECOGNIZE_IMAGE

Process toàn bộ image.

Use cases:

- comic page;
- screenshot;
- scanned page;
- imported image;
- stable captured frame.

### RECOGNIZE_REGION

Process selected source-space region.

Use cases:

- manual selection;
- low-confidence region reprocessing;
- user-selected bubble;
- provider comparison.

### EVALUATE_IMAGE

Optional diagnostic/evaluation operation.

Không tạo user-facing Artifact mặc định nếu policy không cho phép.

---

## 10. Recognition Attempt Output

```text
RecognitionAttemptOutput
├── CandidateArtifact?
├── ModuleWarnings[]
├── ModuleError?
├── RetryHint?
├── DiagnosticsRef?
├── QualitySummary?
└── CompletionMetadata
```

Attempt Output không phải published Recognition Artifact.

Runtime Control quyết định disposition:

```text
ACCEPT
REJECT_STALE
REJECT_CANCELED
REJECT_DUPLICATE
REJECT_INVALID
```

---

## 11. Candidate Recognition Artifact

```text
CandidateRecognitionArtifact
├── CandidateArtifactId
├── ArtifactType
├── OwnerModule
├── ContractVersion
├── InputArtifactRef
├── ContentIdentity
├── SourceCoordinateSpace
├── ProcessedImageMetadata
├── CoordinateTransform
├── ProviderProvenance
├── LanguageHypotheses[]
├── ScriptHypotheses[]
├── Regions[]
├── ReadingOrder
├── Warnings[]
├── QualityMetadata
├── Completeness
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

Candidate Artifact:

- private cho đến khi accepted;
- không reusable;
- không authoritative;
- không cache eligible mặc định;
- vẫn thuộc producer/transfer-pending ownership;
- phải cleanup nếu Runtime reject.

---

## 12. Published Recognition Artifact

Sau Runtime acceptance và Artifact Store publication:

```text
RecognitionArtifact
├── ArtifactId
├── ArtifactType
├── ArtifactContractVersion
├── InputArtifactRef
├── ContentIdentity
├── SourceCoordinateSpace
├── ProcessedImageMetadata
├── CoordinateTransform
├── ProviderProvenance
├── LanguageHypotheses[]
├── ScriptHypotheses[]
├── Regions[]
├── ReadingOrder
├── Warnings[]
├── QualityMetadata
├── Completeness
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

Published Recognition Artifact:

- immutable;
- provider-independent;
- spatially aligned;
- source-traceable;
- reusable khi Cache Policy và compatibility cho phép;
- không chứa Runtime Attempt status.

---

## 13. Recognized Region Model

```text
RecognizedRegion
├── RegionId
├── Geometry
├── SourceGeometry
├── RawText
├── SurfaceNormalizedText
├── Lines[]
├── DetectionConfidence?
├── RecognitionConfidence?
├── Orientation
├── ReadingDirection?
├── LanguageHint?
├── ScriptHint?
├── RegionType?
├── Alternatives[]
├── Warnings[]
├── GeometrySource
└── ProviderMetadata?
```

`SurfaceNormalizedText` chỉ cho phép cleanup không thay đổi meaning.

Allowed:

- trimming outer whitespace;
- normalizing provider line separators;
- removing provider control characters;
- deterministic invalid-Unicode cleanup.

Not allowed:

- correcting Chinese characters by semantic context;
- replacing names;
- joining sentences;
- guessing omitted words;
- semantic punctuation rewriting;
- translation.

---

## 14. Recognized Line Model

```text
RecognizedLine
├── LineId
├── RegionId
├── Geometry
├── SourceGeometry
├── RawText
├── Confidence?
├── Orientation
├── OrderIndex?
├── GeometrySource
└── ProviderMetadata?
```

Line-level output có thể absent.

Recognition không fabricate line geometry mà không đánh dấu:

```text
INFERRED
```

---

## 15. Recognition Profile

```text
RecognitionProfile
├── AUTOMATIC
├── COMIC_PAGE
├── SCREENSHOT
├── SINGLE_REGION
└── STRUCTURED_PAGE
```

Profile ảnh hưởng plan, không thay public Artifact contract.

### AUTOMATIC

Select strategy từ hints và capabilities.

### COMIC_PAGE

Ưu tiên:

- irregular layout;
- multiple regions;
- vertical Chinese;
- text over artwork;
- speech-bubble-like structures.

### SCREENSHOT

Ưu tiên:

- horizontal interface text;
- browser/application labels;
- mixed structured regions.

### SINGLE_REGION

May skip page-level detection.

### STRUCTURED_PAGE

Ưu tiên:

- regular lines;
- columns;
- prose-oriented layout.

---

## 16. Recognition Plan

```text
RecognitionPlan
├── PlanId
├── Operation
├── RecognitionProfile
├── ImagePreparationPlan
├── Strategy
├── DetectionCapabilityRequirement?
├── RecognitionCapabilityRequirement
├── ProviderSelectionRequest
├── RegionExecutionPolicy
├── CoordinateMappingPolicy
├── ReadingOrderPolicy
├── QualityPolicy
├── ConfigurationVersions
└── PrivacyConstraints
```

Plan immutable trong một Attempt.

---

## 17. Internal Processing Flow

```text
Receive RecognitionAttemptInput
        ↓
Validate Module Contract
        ↓
Build Recognition Plan
        ↓
Resolve Provider Capabilities
        ↓
Acquire Input Artifact Lease
        ↓
Create Attempt-Local Image View
        ↓
Apply Image Preparation Plan
        ↓
Execute Combined or Composed Recognition
        ↓
Normalize Provider Output
        ↓
Suppress Invalid/Duplicate Regions
        ↓
Map Coordinates to Source Space
        ↓
Resolve Initial Reading Order
        ↓
Validate Recognition Invariants
        ↓
Assemble CandidateRecognitionArtifact
        ↓
Submit AttemptCompletion
        ↓
Release Temporary Resources and Leases
```

Cancellation checkpoint được đặt tại bounded boundaries.

---

## 18. Combined Recognition Strategy

```text
Image View
    ↓
Combined Recognition Provider
    ↓
Detected and Recognized Regions
```

Phù hợp khi provider cung cấp:

- region detection;
- text recognition;
- usable geometry;
- acceptable confidence;
- acceptable latency;
- adequate reading order.

MVP nên ưu tiên combined strategy nếu quality đạt yêu cầu.

---

## 19. Composed Recognition Strategy

```text
Image View
    ↓
Detection Provider
    ↓
Detected Regions
    ↓
Recognition Provider
    ↓
Recognized Regions
```

Phù hợp khi:

- comic-region detection cần specialization;
- vertical text cần model khác;
- detection và recognition quality khác nhau;
- provider evaluation cần tách;
- region-level fallback cần thiết.

Composed strategy phải giữ provider provenance theo từng phase.

---

## 20. Image Preparation Plan

```text
ImagePreparationPlan
├── ProfileId
├── Operations[]
├── SourceDimensions
├── TargetDimensions
├── CoordinateTransform
├── ProviderSpecific
├── ConfigurationVersion
└── EstimatedResourceCost
```

Possible operations:

```text
RESIZE
UPSCALE
GRAYSCALE
CONTRAST
BRIGHTNESS
THRESHOLD
DENOISE
SHARPEN
DESKEW
ROTATE
INVERT
PAD
CROP
COLOR_CHANNEL_SELECTION
```

Mọi geometry-changing operation phải cập nhật transform chain.

---

## 21. Coordinate Transform

```text
CoordinateTransform
├── SourceSpace
├── TargetSpace
├── CropOffset
├── ScaleX
├── ScaleY
├── Rotation
├── Padding
├── TransformChain[]
└── InverseTransform
```

Public coordinates phải ở source coordinate space.

Processed-space geometry chỉ dành cho:

- provider normalization;
- diagnostics;
- evaluation.

---

## 22. Geometry Model

MVP geometry:

```text
Rectangle
├── X
├── Y
├── Width
└── Height
```

Architecture cho phép mở rộng:

```text
Polygon
├── Points[]
└── BoundingRectangle
```

Consumer không được giả định mọi region luôn axis-aligned rectangle.

---

## 23. Reading Direction

```text
ReadingDirection
├── UNKNOWN
├── LEFT_TO_RIGHT
├── RIGHT_TO_LEFT
├── TOP_TO_BOTTOM
├── BOTTOM_TO_TOP
├── VERTICAL_COLUMNS_RIGHT_TO_LEFT
├── VERTICAL_COLUMNS_LEFT_TO_RIGHT
└── MIXED
```

Reading direction có thể ở:

- request/profile level;
- page level;
- region level.

Region-level orientation có precedence trong mixed layout.

---

## 24. Reading Order

```text
ReadingOrderEntry
├── OrderIndex
├── RegionId
├── Source
├── Confidence
├── RuleId?
└── ManuallyOverridden
```

Possible source:

```text
PROVIDER
SPATIAL_HEURISTIC
ORIENTATION_RULE
PROFILE_RULE
USER_OVERRIDE
```

Array order alone không phải source of truth.

Recognition tạo initial spatial order.

Text Processing có thể tái cấu trúc semantic order nhưng phải giữ traceability.

---

## 25. Confidence Model

```text
Confidence
├── RawValue?
├── NormalizedValue?
├── Level
├── Source
└── NormalizationMethod?
```

Levels:

```text
UNKNOWN
LOW
MEDIUM
HIGH
```

Rules:

1. Missing provider confidence → `UNKNOWN`.
2. Provider values không so sánh trực tiếp nếu chưa normalize.
3. Normalization provider-specific và documented.
4. Low confidence không tự discard text.
5. Low confidence có thể tạo warning/retry hint.
6. Recognition không tự tạo retry Attempt.

---

## 26. Quality Model

```text
RecognitionQualityMetadata
├── RegionCoverage
├── TextCompleteness
├── GeometryQuality
├── ReadingOrderQuality
├── ConfidenceAvailability
├── LanguageSupportQuality
├── WarningCount
└── QualityLevel
```

Quality metadata không khẳng định linguistic correctness beyond Recognition scope.

---

## 27. Completeness

```text
RecognitionCompleteness
├── COMPLETE
├── PARTIAL
├── EMPTY_VALID
└── UNKNOWN
```

No-text result có thể thành công:

```text
Completeness = EMPTY_VALID
Regions = []
Warnings = [NO_READABLE_TEXT_DETECTED]
```

Empty result không tự động là failure.

---

## 28. Recognition Warnings

Warnings mô tả degraded-but-usable output.

```text
NO_READABLE_TEXT_DETECTED
LOW_DETECTION_CONFIDENCE
LOW_RECOGNITION_CONFIDENCE
UNSUPPORTED_ORIENTATION_FALLBACK
UNSUPPORTED_LANGUAGE_FALLBACK
READING_ORDER_UNCERTAIN
OVERLAPPING_REGIONS_SUPPRESSED
REGION_GEOMETRY_INFERRED
LINE_GEOMETRY_UNAVAILABLE
PROVIDER_CONFIDENCE_UNAVAILABLE
IMAGE_UPSCALED
IMAGE_DOWNSCALED
REMOTE_PROVIDER_USED
PARTIAL_RECOGNITION
PREPROCESSING_FALLBACK_USED
```

Warning structure:

```text
RecognitionWarning
├── Code
├── OperationPhase
├── RegionId?
├── ProviderId?
├── MessageKey
└── Metadata?
```

Warnings không phải Runtime terminal outcome.

---

## 29. Module Error Model

Recognition defines module error semantics.

```text
RecognitionModuleError
├── Code
├── OperationPhase
├── RetryHint
├── AffectedRegionId?
├── ProviderErrorRef?
├── DiagnosticRef?
└── Metadata?
```

Codes:

```text
RECOGNITION_INPUT_INVALID
RECOGNITION_IMAGE_INVALID
RECOGNITION_IMAGE_FORMAT_UNSUPPORTED
RECOGNITION_LANGUAGE_UNSUPPORTED
RECOGNITION_SCRIPT_UNSUPPORTED
RECOGNITION_ORIENTATION_UNSUPPORTED
RECOGNITION_IMAGE_TOO_LARGE
RECOGNITION_PREPARATION_FAILED
RECOGNITION_DETECTION_FAILED
RECOGNITION_TEXT_FAILED
RECOGNITION_COORDINATE_MAPPING_FAILED
RECOGNITION_READING_ORDER_FAILED
RECOGNITION_CANDIDATE_INVALID
RECOGNITION_RESOURCE_EXHAUSTED
RECOGNITION_INTERNAL_ERROR
```

Provider-specific errors normalize qua Provider Adapter và Runtime Error Model.

---

## 30. Retry Hint

Recognition may return:

```text
RetryHint
├── Retryability
├── SuggestedStrategy[]
├── SuggestedDelay?
├── AlternativeProviderAllowed
├── AlternativePreparationAllowed
└── ReasonCode
```

Possible strategy:

```text
SAME_PROVIDER
ALTERNATIVE_PROVIDER
ALTERNATIVE_PREPROCESSING
REGION_ONLY
RESOURCE_WAIT
NO_RETRY
```

Runtime Retry Policy quyết định:

- whether retry;
- when;
- provider fallback;
- attempt count;
- budget;
- authority revalidation.

Recognition không gọi retry trực tiếp.

---

## 31. Cancellation

Recognition dùng Runtime-provided `CancellationContextRef`.

Checkpoints:

- before Lease acquisition;
- before image preparation;
- after image preparation;
- before provider execution;
- between bounded region batches;
- after provider completion;
- before coordinate mapping;
- before Candidate assembly;
- before Completion submission.

Khi provider không cancel được:

```text
Runtime authority revoked
        ↓
Provider execution continues physically
        ↓
Attempt becomes draining/abandoned
        ↓
Late output rejected
        ↓
Resources released on physical completion
```

Recognition không sở hữu global cancellation registry.

---

## 32. Authority

Recognition không quyết định:

- current Session;
- current Revision;
- accepted Attempt;
- commit authority;
- publication authority.

Recognition phải preserve:

```text
SessionId?
RevisionId
WorkItemId
AttemptId
ConfigurationSnapshotId
InputArtifactRef
```

Runtime Control là authority owner.

---

## 33. Publication

Recognition không publish accepted Artifact.

```text
Candidate Artifact
    ↓
Attempt Completion
    ↓
Runtime Authority Validation
    ↓
Artifact Store Ownership Transfer
    ↓
Atomic Publication
```

Recognition event không thay thế publication.

---

## 34. Provider Capability Requirement

Recognition defines capability requirements:

```text
RecognitionCapabilityRequirements
├── Languages[]
├── Scripts[]
├── Orientations[]
├── RecognitionProfiles[]
├── RegionDetectionRequired
├── TextRecognitionRequired
├── CombinedRecognitionAllowed
├── ConfidenceRequired
├── LineGeometryRequired
├── CharacterGeometryRequired
├── PartialOutputAllowed
├── CancellationPreference
├── LocalOnly
├── RemoteAllowed
├── ExecutionClasses[]
├── MaximumImageSize
└── HardwarePreference?
```

---

## 35. Provider Selection Boundary

```text
Recognition Plan
    ↓ capability requirements
Provider Selection Policy
    ↓
Provider Manager
    ↓
Selected Provider Adapter
```

Recognition không sở hữu system-wide provider health, credential hoặc lifecycle.

---

## 36. Recognition Provider Adapter

```text
RecognitionProviderAdapter
├── ProviderIdentity
├── Capabilities
├── ValidateRequest
├── PrepareProviderRequest
├── Execute
├── RequestCancellation
├── NormalizeOutput
├── NormalizeError
└── ProviderDiagnostics
```

Provider-specific SDK object không vượt adapter boundary.

---

## 37. Provider Capabilities

```text
RecognitionProviderCapabilities
├── ProviderId
├── ProviderVersion
├── SupportedLanguages[]
├── SupportedScripts[]
├── SupportedOrientations[]
├── SupportedProfiles[]
├── SupportsRegionDetection
├── SupportsTextRecognition
├── SupportsCombinedRecognition
├── SupportsConfidence
├── SupportsLineGeometry
├── SupportsCharacterGeometry
├── SupportsPartialOutput
├── SupportsCancellation
├── SupportsBatching
├── ExecutionClasses[]
├── SupportsLocalProcessing
├── SupportsRemoteProcessing
├── MaximumWidth?
├── MaximumHeight?
├── MaximumPixels?
├── RecommendedConcurrency
└── InitializationCost
```

Capabilities phải phản ánh documented hoặc validated behavior.

---

## 38. Provider Lifecycle Ownership

Provider Manager owns:

- registration;
- initialization;
- health;
- model loading;
- client lifetime;
- GPU/native context;
- concurrency capacity;
- shutdown.

Recognition owns only provider-specific semantic contract and normalized output.

---

## 39. Artifact Compatibility

Recognition defines:

```text
RecognitionCompatibilityMetadata
├── InputContentIdentity
├── RecognitionContractVersion
├── RecognitionProfileVersion
├── PreparationProfileVersion
├── DetectionModelVersion?
├── RecognitionModelVersion
├── ProviderProfileVersion
├── LanguageHints
├── ScriptHints
├── OrientationHints
├── ReadingOrderPolicyVersion
├── CoordinateTransformVersion
├── QualityPolicyVersion
├── ConfigurationVersions
└── PrivacyPartition
```

Recognition không quyết định retention hoặc physical cache.

---

## 40. Artifact Reuse Boundary

```text
Recognition
    → defines semantic compatibility

Cache Policy
    → decides whether reuse is allowed

Artifact Store
    → manages runtime Artifact and retention

Storage
    → provides durable persistence
```

RevisionId không phải reuse identity mặc định.

---

## 41. Resource Lifecycle

```text
Acquire Input Artifact Lease
        ↓
Create Attempt-Local Image View
        ↓
Allocate Preparation Buffers
        ↓
Acquire Provider Resource
        ↓
Execute Recognition
        ↓
Create Candidate Artifact
        ↓
Transfer Candidate or Cleanup
        ↓
Release Provider Request Resource
        ↓
Release Buffers
        ↓
Release Input Lease
```

Recognition không dispose shared input Artifact.

---

## 42. Recognition Operation Phases

Phases chỉ dùng cho diagnostics:

```text
VALIDATING
PLANNING
ACQUIRING_INPUT
PREPARING
DETECTING
RECOGNIZING
NORMALIZING
MAPPING_COORDINATES
ORDERING
ASSEMBLING_CANDIDATE
FINALIZING
```

Đây không phải WorkItem/Attempt state machine.

---

## 43. Concurrency

Rules:

1. Recognition không chạy trên UI Context.
2. Runtime Scheduler controls admission.
3. Provider Manager controls provider concurrency.
4. GPU/native provider có thể serial.
5. Region parallelism bounded.
6. Worker owns only Attempt-local resources.
7. Shared input accessed via Resource Lease.
8. Provider model initialization không request-scoped.
9. Image copies minimized.
10. Same Attempt output deterministic với same input/config/provider behavior.
11. Obsolete work mất authority nhanh.
12. Shutdown waits only bounded drain.

---

## 44. Module State

Recognition giữ persistent mutable state tối thiểu.

Allowed module-owned long-lived state:

- static Recognition profiles;
- quality policy definitions;
- normalization strategies;
- provider contract metadata;
- capability requirement builders.

Not module-owned:

- provider health;
- loaded models;
- active WorkItem registry;
- active Attempt registry;
- cancellation registry;
- Artifact retention;
- Session state.

---

## 45. Events

Recognition-specific diagnostic/domain facts:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_PREPARATION_COMPLETED
RECOGNITION_REGIONS_DETECTED
RECOGNITION_PROVIDER_OUTPUT_NORMALIZED
RECOGNITION_READING_ORDER_RESOLVED
RECOGNITION_WARNING_RECORDED
RECOGNITION_CANDIDATE_CREATED
```

Runtime owns:

```text
WORKITEM_CREATED
ATTEMPT_STARTED
ATTEMPT_COMPLETED
ATTEMPT_FAILED
ATTEMPT_CANCELED
ARTIFACT_PUBLISHED
```

Events:

- contain no image payload;
- do not grant authority;
- do not schedule downstream work;
- do not create hidden control flow.

---

## 46. Observability

Recognition metrics:

```text
recognition.plan_ms
recognition.input_lease_wait_ms
recognition.preparation_ms
recognition.detection_ms
recognition.text_ms
recognition.normalization_ms
recognition.coordinate_mapping_ms
recognition.ordering_ms
recognition.candidate_assembly_ms
recognition.total_execution_ms
recognition.time_to_first_region_ms
recognition.region_count
recognition.character_count
recognition.warning_count
recognition.quality_level
recognition.provider_profile
recognition.execution_class
recognition.partial_total
recognition.empty_valid_total
```

Authority validation, ownership transfer và publication latency thuộc Runtime Observability.

Telemetry không chứa raw image hoặc full recognized text.

---

## 47. Privacy

1. Raw image bytes không xuất hiện trong normal logs.
2. Full recognized text không xuất hiện trong production logs.
3. Credentials không xuất hiện trong module input/output.
4. Remote execution explicit và traceable.
5. Local-only input không dùng remote provider.
6. Temporary image files deleted/released after use.
7. Events carry references, not payload.
8. Diagnostic capture requires explicit policy.
9. Output follows privacy partition.
10. Copyrighted content không retained permanently mặc định.
11. Content fingerprint không export mặc định.
12. Provider metadata được sanitize.

---

## 48. Data Ownership

Recognition owns:

- Recognition contract;
- region/line semantics;
- normalized provider representation;
- raw recognized source text semantics;
- initial reading-order semantics;
- confidence normalization rules;
- quality rules;
- warning/error semantics;
- Artifact compatibility semantics;
- module diagnostics definitions.

Recognition does not own:

- source-image payload lifecycle;
- published Artifact payload;
- provider instance lifecycle;
- WorkItem/Attempt state;
- Runtime authority;
- Cache retention;
- Storage persistence;
- corrected semantic text;
- Translation output;
- UI layout.

---

## 49. Dependencies

Allowed:

```text
shared-kernel
runtime-contracts
artifact-contracts
provider-contracts
configuration-contracts
image-primitives
geometry-primitives
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
session implementation
storage implementation
glossary implementation
history implementation
scheduler implementation
work-queue implementation
```

Runtime integration happens through contracts.

---

## 50. Testing Requirements

### Unit Tests

- input validation;
- Recognition Plan;
- coordinate transforms;
- geometry mapping;
- reading-order rules;
- confidence normalization;
- quality classification;
- warning generation;
- duplicate-region suppression;
- Candidate invariants;
- compatibility metadata;
- provider output normalization.

### Provider Contract Tests

Every adapter must test:

- valid request;
- invalid image;
- unsupported language;
- empty-valid result;
- timeout;
- cancellation request;
- provider unavailable;
- coordinate normalization;
- confidence behavior;
- partial output;
- error normalization;
- privacy/local-only enforcement.

### Runtime Integration Tests

```text
Image Artifact
    → Recognition Attempt
    → Candidate Artifact
    → Runtime Publication
```

```text
Revision A Attempt
    → Revision B Current
    → A Candidate Rejected
```

```text
Provider Non-Cancelable
    → Authority Revoked
    → Attempt Abandoned
    → Late Output Rejected
```

```text
Recognition Artifact
    → Text Processing
```

### Regression Tests

Compare:

- detected regions;
- recognized text;
- source coordinates;
- reading order;
- confidence;
- warnings;
- quality;
- latency;
- memory;
- provider compatibility.

---

## 51. MVP Implementation Contract

Input:

```text
- Image Artifact Reference
- optional source-space region
- Simplified Chinese hint
- optional Traditional Chinese hint
- English hint
- Automatic or Comic Page profile
```

Output:

```text
- rectangular text regions
- raw recognized text
- source-space geometry
- initial reading order
- confidence when available
- warnings
- quality metadata
- compatibility metadata
- Candidate Recognition Artifact
```

Control is provided by Runtime:

```text
- WorkItem / Attempt identity
- cancellation context
- timeout/deadline
- provider selection policy
- authority validation
- publication
- retry
```

MVP does not require:

- handwriting;
- sound-effect understanding;
- character-level geometry;
- polygon output;
- provider marketplace;
- semantic OCR repair;
- inpainting;
- translated-text insertion;
- distributed Recognition;
- streaming per-character publication.

---

## 52. Acceptance Criteria

Architecture is acceptable when:

1. Provider implementation replaceable.
2. Recognition Artifact independent from SDK types.
3. Every public geometry maps to source coordinates.
4. Runtime cancellation/authority prevents stale publication.
5. Text Processing consumes Artifact without provider knowledge.
6. Recognition does not call Translation.
7. Local-only policy enforceable.
8. Raw recognition text preserved.
9. Reading order explicitly represented.
10. Session and imported-image use cases supported.
11. Errors/warnings normalized.
12. Provider quality benchmarkable.
13. Recognition creates Candidate only.
14. Artifact Store owns published payload.
15. Retry/cancel/publication are not module-owned.
16. Compatibility metadata supports safe reuse.
17. Attempt-local resources release correctly.
18. Normal telemetry remains content-free.

---

## 53. Architecture Invariants

1. Recognition accepts only image-based input.
2. Input Artifact immutable.
3. Recognition never mutates source image.
4. Recognition creates Candidate Artifact only.
5. Recognition never grants authority.
6. Recognition never publishes accepted Artifact.
7. Runtime Control owns authority.
8. Artifact Store owns published payload.
9. Recognition never owns WorkItem lifecycle.
10. Recognition never owns Attempt lifecycle.
11. Recognition never retries itself.
12. Recognition uses Runtime cancellation context.
13. Provider lifecycle belongs to Provider Manager.
14. Provider SDK types stay inside adapter.
15. Every public region maps to source coordinate space.
16. Geometry-changing preparation preserves transform chain.
17. Reading order explicit.
18. Raw text preserved.
19. Missing confidence remains unknown.
20. Warning differs from failure.
21. Empty text may be valid success.
22. User corrections never overwrite raw Artifact.
23. Local-only never uses remote provider.
24. Recognition does not call Translation.
25. Recognition does not render UI.
26. Image payload never travels through Event Bus.
27. Cache compatibility belongs to Recognition semantics.
28. Cache retention belongs to Runtime.
29. Durable persistence belongs to Storage.
30. Worker never owns shared input payload.
31. Input Lease released after Attempt use.
32. Candidate rejection triggers cleanup.
33. Late provider output cannot gain authority.
34. Same input/config produces deterministic normalized structure where provider behavior permits.
35. Recognition remains usable outside active Reading Session.

---

## 54. Open Architecture Decisions

- first Recognition provider;
- local vs remote default;
- combined vs composed strategy;
- required vertical-Chinese accuracy;
- default Preparation Profile;
- provider-specific preparation ownership;
- speech-bubble detection ownership;
- script detection timing;
- region alternatives retention;
- partial Candidate exposure;
- timeout values;
- concurrency limits;
- confidence normalization;
- default reading-order algorithm;
- character-level geometry need;
- long-page splitting;
- GPU model-loading policy;
- Traditional Chinese MVP support;
- polygon geometry timing.

Decisions phải dựa trên prototype và benchmark.

---

## 55. Related Documents

```text
modules/recognition/README.md
modules/recognition/CONTRACT.md
modules/recognition/PIPELINE.md
modules/recognition/PROVIDER.md
modules/recognition/PREPROCESSING.md
modules/recognition/REGION_DETECTION.md
modules/recognition/TEXT_RECOGNITION.md
modules/recognition/COORDINATE_MODEL.md
modules/recognition/READING_ORDER.md
modules/recognition/QUALITY_MODEL.md
modules/recognition/EVENTS.md
modules/recognition/ERRORS.md
modules/recognition/CONFIG.md
modules/recognition/OBSERVABILITY.md
runtime/PIPELINE_RUNTIME.md
runtime/RESOURCE_LIFECYCLE.md
runtime/CANCELLATION.md
runtime/RETRY_POLICY.md
runtime/CACHE_POLICY.md
modules/text-processing/README.md
```

---

## 56. Summary

Recognition transforms immutable image input into structured spatial source content.

```text
Image Artifact
    ↓
Recognition Plan
    ↓
Preparation
    ↓
Detection and Recognition
    ↓
Geometry and Reading Order
    ↓
Candidate Recognition Artifact
    ↓
Runtime Validation and Publication
```

Core boundaries:

```text
Recognition owns visual extraction semantics.

Runtime owns WorkItem, Attempt, authority, cancellation and retry.

Provider Manager owns provider lifecycle.

Artifact Store owns published shared payload.

Text Processing owns semantic preparation for Translation.
```
