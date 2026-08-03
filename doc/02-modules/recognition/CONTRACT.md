# Recognition Module Contract

> Project: CRAI  
> Module: Recognition  
> Path: `modules/recognition/CONTRACT.md`  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa public contract của Recognition Module.

Nó đặc tả:

- Runtime-facing Attempt input;
- Runtime-facing Attempt output;
- Candidate Recognition Artifact;
- published Recognition Artifact;
- domain identifiers;
- image and coordinate contracts;
- region and line contracts;
- reading-order contract;
- confidence and quality contracts;
- provider capability contract;
- provider-adapter contract;
- warnings;
- module errors;
- retry hints;
- cancellation checkpoints;
- compatibility metadata;
- privacy rules;
- validation rules;
- producer, consumer và Runtime obligations;
- contract evolution.

Tài liệu này không định nghĩa:

- OCR algorithms;
- provider SDK integration details;
- image-processing library;
- Runtime scheduling policy;
- Runtime retry decision;
- Runtime cancellation authority;
- Artifact Store implementation;
- persistence technology;
- UI behavior;
- source capture;
- Text Processing behavior;
- Translation behavior.

Contract phải ổn định ngay cả khi internal detector, recognizer, provider hoặc image library thay đổi.

---

## 2. Contract Boundary

Recognition nhận image-based Artifact input và tạo structured source-content Candidate Artifact.

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

Recognition không expose application-level commands:

```text
RetryRecognition
CancelRecognition
GetRecognitionResult
```

Các hành vi đó thuộc Runtime, Artifact Store hoặc orchestration.

---

## 3. Contract Principles

### 3.1 Provider Independence

Consumers không phụ thuộc provider SDK model.

Mọi provider output phải normalize thành CRAI contract.

### 3.2 Stable Source Mapping

Mọi public geometry phải map về source coordinate space.

### 3.3 Immutable Artifact

Published Recognition Artifact không được mutate.

Corrections hoặc downstream transformations tạo object/Artifact mới.

### 3.4 Explicit Runtime Identity

Attempt input phải chứa explicit Runtime identity.

### 3.5 Explicit Reading Order

Consumer không suy luận reading order chỉ từ array position.

### 3.6 Explicit Uncertainty

Unknown confidence, language, script và uncertain order phải giữ explicit.

### 3.7 Authority Separation

Recognition không quyết định Candidate còn current hay có publication authority.

### 3.8 Candidate Separation

Candidate Artifact không phải published Artifact.

### 3.9 Privacy Preservation

Raw image và full recognized text không xuất hiện trong normal event/log.

### 3.10 Backward Compatibility

Contract evolution phải giữ existing meaning trong cùng major version.

---

## 4. Contract Version

```text
RecognitionContractVersion
├── Major
├── Minor
└── Patch
```

Initial:

```text
1.0.0
```

Meaning:

- `Major`: incompatible contract change;
- `Minor`: backward-compatible field/capability addition;
- `Patch`: clarification hoặc non-semantic correction.

Cross-process contract phải mang version.

---

## 5. Common Identifier Types

```text
ApplicationInstanceId = opaque string
SessionId             = opaque string
RevisionId            = opaque string
WorkItemId            = opaque string
AttemptId             = opaque string
ArtifactId            = opaque string
CandidateArtifactId   = opaque string
ResourceId            = opaque string
ProviderId            = opaque string
ProviderRequestId     = opaque string
ConfigurationSnapshotId = opaque string
RegionId              = opaque string
LineId                = opaque string
TraceId               = opaque string
```

Consumers không suy luận timestamp, provider hoặc ordering từ identifier content.

---

## 6. Common Scalar Types

### 6.1 Timestamp

```text
Timestamp = ISO-8601 UTC datetime
```

### 6.2 Duration

```text
DurationMilliseconds = non-negative integer
```

### 6.3 Language Code

```text
LanguageCode = BCP-47-compatible string
```

Examples:

```text
zh-Hans
zh-Hant
zh
en
vi
ja
ko
```

### 6.4 Script Code

```text
ScriptCode = ISO-15924-compatible string
```

Examples:

```text
Hans
Hant
Latn
Jpan
Kore
```

### 6.5 Metadata

```text
Metadata = map<string, scalar | scalar[]>
```

Allowed scalar:

```text
string
integer
decimal
boolean
null
```

Metadata không chứa:

- raw image bytes;
- arbitrary provider objects;
- executable value;
- credential;
- unrestricted nested graphs;
- full recognized content mặc định.

---

## 7. Runtime Identity Context

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

### Rules

1. `RevisionId`, `WorkItemId`, `AttemptId` bắt buộc.
2. SessionId có thể absent cho standalone imported image.
3. Priority không thuộc Recognition contract.
4. Queue class không thuộc Recognition contract.
5. Retry count không thuộc Recognition contract.
6. Timeout/deadline được tham chiếu qua ExecutionContextRef.
7. Runtime identity chỉ dùng traceability, không cấp authority cho module.

---

## 8. Trace Context

```text
TraceContext
├── TraceId
├── ParentSpanId?
├── CorrelationId?
└── Baggage?
```

Baggage không chứa source text, image content hoặc secret.

---

## 9. Artifact Reference Contract

Recognition nhận Artifact reference thay vì embedded image payload.

```text
ArtifactRef
├── ArtifactId
├── ArtifactType
├── ContractVersion
├── ResourceId
├── ContentIdentity
├── CoordinateSpace?
└── Metadata?
```

Input expected type:

```text
IMAGE_ARTIFACT
```

### Artifact Reference Rules

1. Input Artifact phải immutable.
2. Resource phải tồn tại trong Attempt lifetime.
3. Recognition truy cập qua Resource Lease.
4. Recognition không giữ reference sau lifecycle cho phép.
5. Recognition không dispose shared input Artifact.
6. Input content gửi remote chỉ khi privacy policy cho phép.

---

## 10. Content Identity

```text
ContentIdentity
├── IdentityAlgorithm
├── IdentityVersion
├── Value
└── SourceScope?
```

Rules:

- không đồng nghĩa ArtifactId;
- không đồng nghĩa RevisionId;
- không export raw preimage;
- dùng cho semantic compatibility và reuse query;
- algorithm/version phải explicit.

---

## 11. Coordinate Space Contract

```text
CoordinateSpace
├── SpaceId
├── Width
├── Height
├── Origin
├── Unit
└── RotationDegrees
```

### Coordinate Origin

```text
TOP_LEFT
TOP_RIGHT
BOTTOM_LEFT
BOTTOM_RIGHT
```

MVP dùng:

```text
TOP_LEFT
```

### Coordinate Unit

```text
PIXEL
NORMALIZED
```

Normalized:

```text
x, y, width, height ∈ [0, 1]
```

Recognition public output ưu tiên pixel coordinates trong source space.

---

## 12. Geometry Contract

### Rectangle

```text
RectangleGeometry
├── GeometryType = RECTANGLE
├── X
├── Y
├── Width
└── Height
```

Validation:

```text
Width > 0
Height > 0
X >= 0
Y >= 0
X + Width <= CoordinateSpace.Width
Y + Height <= CoordinateSpace.Height
```

Floating-point tolerance nhỏ được cho phép khi transform.

### Polygon

```text
PolygonGeometry
├── GeometryType = POLYGON
├── Points[]
└── BoundingRectangle
```

```text
Point
├── X
└── Y
```

Polygon optional cho MVP.

### Geometry Union

```text
Geometry =
    RectangleGeometry
    | PolygonGeometry
```

Consumer phải đọc `GeometryType`.

---

## 13. Recognition Operation

```text
RecognitionOperation
├── RECOGNIZE_IMAGE
├── RECOGNIZE_REGION
└── EVALUATE_IMAGE
```

### RECOGNIZE_IMAGE

Process toàn image.

### RECOGNIZE_REGION

Process một source-space region.

### EVALUATE_IMAGE

Diagnostic/benchmark execution; không mặc định tạo user-facing publication.

---

## 14. Recognition Profile

```text
RecognitionProfile
├── AUTOMATIC
├── COMIC_PAGE
├── SCREENSHOT
├── SINGLE_REGION
└── STRUCTURED_PAGE
```

Profile ảnh hưởng Recognition Plan nhưng không thay output contract.

---

## 15. Text Orientation

```text
TextOrientation
├── UNKNOWN
├── HORIZONTAL
├── VERTICAL
├── ROTATED_90_CLOCKWISE
├── ROTATED_90_COUNTER_CLOCKWISE
├── UPSIDE_DOWN
└── MIXED
```

---

## 16. Reading Direction

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

---

## 17. Recognition Options

```text
RecognitionOptions
├── Profile
├── LanguageHints[]
├── ScriptHints[]
├── OrientationHint?
├── ReadingDirectionHint?
├── ReturnLineGeometry
├── ReturnProviderAlternatives
├── AllowPartialCandidate
├── DiagnosticLevel
└── QualityPolicyRef?
```

Provider selection policy không embedded toàn bộ trong Recognition options.

Recognition chỉ định nghĩa capability requirements.

---

## 18. Diagnostic Level

```text
DiagnosticLevel
├── NONE
├── BASIC
├── DETAILED
└── PROTECTED_CONTENT
```

`PROTECTED_CONTENT` cần explicit privacy authorization.

---

## 19. Recognition Capability Requirements

```text
RecognitionCapabilityRequirements
├── SupportedLanguages[]
├── SupportedScripts[]
├── SupportedOrientations[]
├── SupportedProfiles[]
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
├── MaximumImageSize?
└── HardwarePreference?
```

Provider Selection Policy chọn provider dựa trên contract này.

---

## 20. Recognition Attempt Input

```text
RecognitionAttemptInput
├── RuntimeContext
├── InputArtifactRef
├── Operation
├── SourceCoordinateSpace
├── RegionSelection?
├── Options
├── CapabilityRequirements
├── ExecutionContextRef
├── CancellationContextRef
├── PrivacyContextRef
└── DiagnosticsContextRef?
```

### Preconditions

1. Input Artifact resolvable.
2. Artifact type supported.
3. Coordinate space valid.
4. RegionSelection nằm trong source space.
5. Recognition Profile supported.
6. Capability Requirements internally consistent.
7. Contract major version supported.
8. Privacy context permits selected execution path.
9. Runtime owns valid Attempt identity.
10. Recognition không tự validate current Revision authority.

---

## 21. Region Selection

```text
RegionSelection
├── Geometry
├── CoordinateSpaceId
├── SelectionSource
└── SelectionMetadata?
```

```text
SelectionSource
├── USER_SELECTED
├── RUNTIME_DERIVED
├── RETRY_SCOPE
└── DIAGNOSTIC
```

Rules:

- region area > 0;
- must fit source space;
- public output geometry vẫn map về source space;
- internal subdivision không làm mất mapping.

---

## 22. Execution Context Reference

```text
ExecutionContextRef
├── ExecutionClass
├── Deadline?
├── ResourceBudgetRef?
├── ProviderSelectionRef?
└── RuntimePolicyRef?
```

```text
ExecutionClass
├── CPU
├── GPU
├── REMOTE_IO
├── NATIVE_SERIAL
├── PROCESS
└── HYBRID
```

Recognition đọc context; không thay đổi Runtime policy.

---

## 23. Cancellation Context Reference

```text
CancellationContextRef
├── CancellationId
├── IsCancellationRequested
├── RequestedAt?
├── Reason?
└── CheckpointPolicyRef?
```

Recognition chỉ cooperative check.

Nó không:

- create global cancellation registry;
- revoke authority;
- cancel WorkItem lineage;
- decide terminal outcome.

---

## 24. Privacy Context

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

## 25. Recognition Attempt Output

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

Exactly one primary execution disposition:

```text
CandidateArtifact present
or
ModuleError present
or
Cancellation observed
```

Runtime quyết định accepted terminal outcome.

---

## 26. Completion Metadata

```text
RecognitionCompletionMetadata
├── StartedAt
├── CompletedAt
├── OperationPhase
├── ProviderRequestIds[]
├── ExecutionMetrics
└── CancellationObserved
```

Metadata này thuộc Attempt output, không thuộc published Artifact.

---

## 27. Candidate Recognition Artifact

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
├── ReadingOrder[]
├── Warnings[]
├── QualityMetadata
├── Completeness
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

### Candidate Rules

1. Candidate không authoritative.
2. Candidate không published.
3. Candidate không cache eligible mặc định.
4. Candidate private cho producer/Runtime validation path.
5. Candidate phải immutable sau assembly.
6. Candidate rejected phải cleanup.
7. Candidate không chứa WorkItem status.
8. Candidate không chứa retry count.
9. Candidate không chứa queue timing.
10. Candidate không chứa credential.

---

## 28. Published Recognition Artifact

```text
RecognitionArtifact
├── ArtifactId
├── ArtifactType
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
├── ReadingOrder[]
├── Warnings[]
├── QualityMetadata
├── Completeness
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

Published Recognition Artifact:

- immutable;
- accepted bởi Runtime;
- owned bởi Artifact Store;
- provider-independent;
- source-traceable;
- reusable khi compatibility và Cache Policy cho phép.

---

## 29. Artifact Type

```text
ArtifactType = RECOGNITION_ARTIFACT
```

Future subtype không được thay contract core nếu chỉ là metadata/profile difference.

---

## 30. Processed Image Metadata

```text
ProcessedImageMetadata
├── SourceWidth
├── SourceHeight
├── ProcessedWidth
├── ProcessedHeight
├── PreparationProfileId?
├── Operations[]
├── SourceChecksumRef?
└── ProcessedChecksumRef?
```

### Preparation Operation Summary

```text
PreparationOperationSummary
├── Operation
├── ChangedGeometry
└── ParametersSummary?
```

Operations:

```text
CROP
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
COLOR_CHANNEL_SELECTION
```

Sensitive provider config không xuất hiện trong summary.

---

## 31. Coordinate Transform

```text
CoordinateTransform
├── SourceSpaceId
├── ProcessedSpaceId
├── CropOffsetX
├── CropOffsetY
├── ScaleX
├── ScaleY
├── RotationDegrees
├── PaddingLeft
├── PaddingTop
├── PaddingRight
├── PaddingBottom
├── TransformChain[]
└── InverseTransformAvailable
```

Consumers thường không cần tự transform vì public geometry đã ở source space.

---

## 32. Provider Provenance

```text
ProviderProvenance
├── ProviderId
├── ProviderVersion
├── AdapterVersion
├── ExecutionLocation
├── ExecutionClass
├── ModelId?
├── ModelVersion?
├── FallbackIndex
├── DetectionProvider?
├── RecognitionProvider?
└── SanitizedMetadata?
```

```text
ExecutionLocation
├── LOCAL_PROCESS
├── LOCAL_SIDECAR
├── LOCAL_SERVICE
└── REMOTE_SERVICE
```

Credential, endpoint secret và SDK internals không được expose.

---

## 33. Language Hypothesis

```text
LanguageHypothesis
├── LanguageCode
├── Confidence
├── Source
└── RegionIds[]
```

## 34. Script Hypothesis

```text
ScriptHypothesis
├── ScriptCode
├── Confidence
├── Source
└── RegionIds[]
```

```text
HypothesisSource
├── REQUEST_HINT
├── PROVIDER_DETECTION
├── SCRIPT_CLASSIFIER
├── RECOGNITION_OUTPUT
└── COMBINED_INFERENCE
```

Request hint không được represent như detected fact.

---

## 35. Recognized Region Contract

```text
RecognizedRegion
├── RegionId
├── Geometry
├── RawText
├── SurfaceText
├── Lines[]
├── DetectionConfidence
├── RecognitionConfidence
├── Orientation
├── ReadingDirection
├── LanguageHypothesisRef?
├── ScriptHypothesisRef?
├── RegionType
├── GeometrySource
├── Alternatives[]
├── Warnings[]
└── ProviderMetadata?
```

### Region Type

```text
UNKNOWN
TEXT_BLOCK
TEXT_LINE
VERTICAL_COLUMN
SPEECH_BUBBLE_TEXT
CAPTION
PAGE_TITLE
INTERFACE_TEXT
SOUND_EFFECT_CANDIDATE
DECORATIVE_TEXT
```

Region type là Recognition hint, không phải semantic truth.

---

## 36. Geometry Source

```text
GeometrySource
├── PROVIDER_DETECTED
├── DETECTOR_DETECTED
├── REQUEST_REGION
├── DERIVED_FROM_LINES
├── INFERRED
└── USER_SELECTED
```

---

## 37. Surface Text Rules

`RawText` giữ normalized provider output gần nhất có thể.

`SurfaceText` cho phép deterministic non-semantic cleanup.

Allowed:

- normalize line separator;
- remove null/control characters;
- trim outer whitespace;
- normalize invalid Unicode sequence;
- remove documented provider formatting artifact.

Forbidden:

- contextual character correction;
- name correction;
- punctuation interpretation;
- sentence reconstruction;
- translation;
- glossary substitution;
- inferred missing-word insertion.

---

## 38. Recognized Line Contract

```text
RecognizedLine
├── LineId
├── RegionId
├── Geometry
├── RawText
├── SurfaceText
├── Confidence
├── Orientation
├── RegionOrderIndex?
├── GeometrySource
└── ProviderMetadata?
```

Rules:

1. LineId unique trong Artifact.
2. RegionId reference existing Region.
3. Geometry intersect/inside parent region.
4. Inferred geometry marked `INFERRED`.
5. Missing line output represented by empty array.
6. Consumers không assume provider luôn có line geometry.

---

## 39. Recognition Alternative

```text
RecognitionAlternative
├── Text
├── Confidence
├── Rank
├── Source
└── ProviderMetadata?
```

```text
AlternativeSource
├── PROVIDER_CANDIDATE
├── ALTERNATIVE_PREPARATION
├── SECONDARY_PROVIDER
└── DIAGNOSTIC_COMPARISON
```

Alternatives optional và thường omitted.

---

## 40. Confidence Contract

```text
Confidence
├── Level
├── NormalizedValue?
├── RawValue?
├── RawScale?
├── Source
└── NormalizationMethod?
```

### Confidence Level

```text
UNKNOWN
LOW
MEDIUM
HIGH
```

### Raw Scale

```text
ZERO_TO_ONE
ZERO_TO_HUNDRED
LOG_PROBABILITY
PROVIDER_SPECIFIC
UNKNOWN
```

### Confidence Source

```text
PROVIDER
DETECTOR
RECOGNIZER
HEURISTIC
AGGREGATED
UNAVAILABLE
```

Rules:

1. NormalizedValue ∈ [0,1].
2. RawValue chỉ interpret cùng RawScale.
3. Missing confidence = UNKNOWN.
4. Missing confidence không convert thành zero.
5. Aggregated confidence ghi Source=AGGREGATED.
6. Threshold có thể provider-specific.
7. Consumer ưu tiên Level.
8. Confidence không tự discard text.

---

## 41. Reading Order Contract

```text
ReadingOrderEntry
├── OrderIndex
├── RegionId
├── OrderSource
├── Confidence
├── GroupId?
├── ParentGroupId?
├── ManuallyOverridden
└── RuleId?
```

```text
ReadingOrderSource
├── PROVIDER
├── SPATIAL_HEURISTIC
├── ORIENTATION_HEURISTIC
├── COMBINED_HEURISTIC
├── REQUEST_HINT
├── USER_OVERRIDE
└── UNKNOWN
```

Rules:

1. OrderIndex bắt đầu từ 0.
2. OrderIndex unique.
3. Readable region nên có one entry.
4. Decorative region có thể omitted.
5. RegionId phải tồn tại.
6. Consumer dùng explicit OrderIndex.
7. Mixed layout có thể dùng groups.
8. Uncertainty qua Confidence hoặc Warning.
9. User correction không mutate original Artifact.

---

## 42. Warning Contract

```text
RecognitionWarning
├── WarningCode
├── OperationPhase
├── Severity
├── MessageKey
├── RegionId?
├── LineId?
├── ProviderId?
└── Metadata?
```

### Severity

```text
INFORMATION
DEGRADED
ATTENTION_REQUIRED
```

Warning không có fatal severity.

### Warning Codes

```text
NO_READABLE_TEXT_DETECTED
LOW_DETECTION_CONFIDENCE
LOW_RECOGNITION_CONFIDENCE
READING_ORDER_UNCERTAIN
UNSUPPORTED_LANGUAGE_FALLBACK
UNSUPPORTED_SCRIPT_FALLBACK
UNSUPPORTED_ORIENTATION_FALLBACK
PROVIDER_CONFIDENCE_UNAVAILABLE
LINE_GEOMETRY_UNAVAILABLE
REGION_GEOMETRY_INFERRED
OVERLAPPING_REGIONS_SUPPRESSED
DUPLICATE_REGION_SUPPRESSED
PARTIAL_RECOGNITION
PREPARATION_FALLBACK_USED
IMAGE_UPSCALED
IMAGE_DOWNSCALED
IMAGE_ROTATED
REMOTE_PROVIDER_USED
FALLBACK_PROVIDER_USED
PROVIDER_ALTERNATIVES_UNAVAILABLE
MIXED_ORIENTATION_DETECTED
MIXED_LANGUAGE_DETECTED
OUTPUT_TRUNCATED
DIAGNOSTIC_DATA_LIMITED
```

---

## 43. Recognition Operation Phase

```text
RecognitionOperationPhase
├── VALIDATING
├── PLANNING
├── ACQUIRING_INPUT
├── PREPARING
├── DETECTING
├── RECOGNIZING
├── NORMALIZING
├── MAPPING_COORDINATES
├── ORDERING
├── ASSEMBLING_CANDIDATE
└── FINALIZING
```

Đây là diagnostic phase, không phải Runtime Attempt state.

---

## 44. Completeness Contract

```text
RecognitionCompleteness
├── COMPLETE
├── PARTIAL
├── EMPTY_VALID
└── UNKNOWN
```

### Empty Valid

```text
Completeness = EMPTY_VALID
Regions = []
ReadingOrder = []
Warnings includes NO_READABLE_TEXT_DETECTED
```

Không tương đương failure.

### Partial

```text
Completeness = PARTIAL
Warnings includes PARTIAL_RECOGNITION
```

Partial Candidate phải explicit và policy cho phép.

---

## 45. Quality Contract

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

```text
QualityLevel
├── UNKNOWN
├── LOW
├── MEDIUM
└── HIGH
```

Quality không khẳng định semantic correctness ngoài Recognition scope.

---

## 46. Execution Metrics Contract

```text
RecognitionExecutionMetrics
├── ValidationDurationMs
├── PlanningDurationMs
├── InputLeaseWaitMs?
├── PreparationDurationMs
├── DetectionDurationMs
├── RecognitionDurationMs
├── NormalizationDurationMs
├── CoordinateMappingDurationMs
├── ReadingOrderDurationMs
├── CandidateAssemblyDurationMs
├── TotalExecutionDurationMs
├── TimeToFirstRegionMs?
├── SourcePixelCount
├── ProcessedPixelCount
├── DetectedRegionCount
├── RecognizedRegionCount
├── RecognizedLineCount
├── RecognizedCharacterCount
├── ProviderRequestCount
├── ExecutionClass
└── PeakMemoryBytes?
```

Không chứa queue time, retry count hoặc authority/publication latency.

Metrics không expose content.

---

## 47. Recognition Module Error

```text
RecognitionModuleError
├── ContractVersion
├── ErrorCode
├── OperationPhase
├── MessageKey
├── RetryHint?
├── AffectedRegionId?
├── ProviderErrorRef?
├── DiagnosticsRef?
├── Metadata?
└── OccurredAt
```

### Error Codes

```text
RECOGNITION_INPUT_INVALID
RECOGNITION_ARTIFACT_UNAVAILABLE
RECOGNITION_IMAGE_INVALID
RECOGNITION_IMAGE_FORMAT_UNSUPPORTED
RECOGNITION_COORDINATE_SPACE_INVALID
RECOGNITION_REGION_INVALID
RECOGNITION_LANGUAGE_UNSUPPORTED
RECOGNITION_SCRIPT_UNSUPPORTED
RECOGNITION_ORIENTATION_UNSUPPORTED
RECOGNITION_IMAGE_TOO_LARGE
RECOGNITION_CAPABILITY_UNAVAILABLE
RECOGNITION_PREPARATION_FAILED
RECOGNITION_DETECTION_FAILED
RECOGNITION_TEXT_FAILED
RECOGNITION_COORDINATE_MAPPING_FAILED
RECOGNITION_READING_ORDER_FAILED
RECOGNITION_CANDIDATE_INVALID
RECOGNITION_RESOURCE_EXHAUSTED
RECOGNITION_INTERNAL_ERROR
```

Provider failure codes thuộc normalized ProviderError contract và được reference.

---

## 48. Error Message Rules

Messages phải:

- safe for logs;
- không chứa raw OCR text;
- không chứa sensitive path;
- không chứa credential;
- không chứa full provider response;
- identify operation phase;
- understandable without provider SDK knowledge.

---

## 49. Retry Hint Contract

```text
RetryHint
├── Retryability
├── SuggestedStrategies[]
├── SuggestedDelayMs?
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

```text
RetryStrategy
├── SAME_PROVIDER
├── ALTERNATIVE_PROVIDER
├── ALTERNATIVE_PREPARATION
├── REGION_ONLY
├── RESOURCE_WAIT
└── NO_RETRY
```

RetryHint là advisory.

Runtime Retry Policy mới là authority cho retry.

---

## 50. Provider Capability Contract

```text
RecognitionProviderCapabilities
├── ContractVersion
├── ProviderIdentity
├── SupportedMediaTypes[]
├── SupportedLanguages[]
├── SupportedScripts[]
├── SupportedOrientations[]
├── SupportedProfiles[]
├── Capabilities[]
├── MaximumImageWidth?
├── MaximumImageHeight?
├── MaximumImagePixels?
├── RecommendedConcurrency
├── InitializationCost
├── PrivacyClassification
├── ExecutionClasses[]
└── CapabilityMetadata?
```

### Capabilities

```text
REGION_DETECTION
TEXT_RECOGNITION
COMBINED_DETECTION_AND_RECOGNITION
HORIZONTAL_TEXT
VERTICAL_TEXT
MIXED_ORIENTATION
LANGUAGE_DETECTION
SCRIPT_DETECTION
READING_ORDER
REGION_CONFIDENCE
LINE_CONFIDENCE
LINE_GEOMETRY
CHARACTER_GEOMETRY
RECOGNITION_ALTERNATIVES
PARTIAL_OUTPUT
CANCELLATION
BATCH_RECOGNITION
LOCAL_EXECUTION
REMOTE_EXECUTION
CPU_EXECUTION
GPU_EXECUTION
NPU_EXECUTION
```

### Initialization Cost

```text
NONE
LOW
MEDIUM
HIGH
```

### Privacy Classification

```text
LOCAL_ONLY
LOCAL_SERVICE
REMOTE_PRIVATE
REMOTE_THIRD_PARTY
UNKNOWN
```

Provider operational status không thuộc Recognition Artifact.

---

## 51. Provider Adapter Input

```text
ProviderRecognitionRequest
├── ProviderRequestId
├── AttemptId
├── ImageInputRef
├── ProcessedCoordinateSpace
├── RecognitionProfile
├── LanguageHints[]
├── ScriptHints[]
├── OrientationHint?
├── ReadingDirectionHint?
├── ReturnLineGeometry
├── ReturnAlternatives
├── Deadline?
├── TraceContext
└── ProviderCancellationContext
```

Provider adapter không nhận Session object, UI object hoặc Runtime mutable state.

---

## 52. Provider Adapter Output

```text
ProviderRecognitionResponse
├── ProviderRequestId
├── ProviderIdentity
├── Regions[]
├── ProviderReadingOrder?
├── DetectedLanguages?
├── DetectedScripts?
├── Warnings[]
├── Metrics?
└── Metadata?
```

Provider response type là internal Recognition contract.

Phải normalize trước Candidate assembly.

---

## 53. Provider Adapter Obligations

Mỗi adapter phải:

1. validate provider config;
2. report capability accurately;
3. convert normalized request;
4. normalize coordinates;
5. normalize language/script/orientation;
6. normalize confidence;
7. normalize errors;
8. preserve raw text;
9. support cancellation khi available;
10. declare unsupported cancellation;
11. remove credential from output;
12. prevent SDK object leakage;
13. report provider/model version;
14. release provider-request resources;
15. map empty result thành empty-valid khi appropriate;
16. distinguish no-text from provider failure;
17. enforce privacy before remote transmission;
18. not perform hidden retry;
19. not publish Runtime Artifact;
20. not mutate Runtime state.

---

## 54. Capability Description Queries

Recognition có thể expose read-only description contracts:

```text
DescribeRecognitionProfiles
DescribeRecognitionContract
DescribeRecognitionCapabilityRequirements
```

Provider-specific availability/health queries thuộc Provider Manager.

Published Artifact query thuộc Artifact Store.

---

## 55. Recognition Diagnostic Facts

Recognition-specific facts có thể emit:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_PREPARATION_COMPLETED
RECOGNITION_REGIONS_DETECTED
RECOGNITION_PROVIDER_OUTPUT_NORMALIZED
RECOGNITION_READING_ORDER_RESOLVED
RECOGNITION_WARNING_RECORDED
RECOGNITION_CANDIDATE_CREATED
```

Không emit:

```text
RECOGNITION_COMPLETED
RECOGNITION_FAILED
RECOGNITION_CANCELLED
```

như terminal authority events; Runtime đã sở hữu Attempt events.

Diagnostic fact rules:

- immutable;
- bounded;
- content-free;
- optional;
- not required for correctness;
- not used for downstream orchestration.

---

## 56. Artifact Compatibility Metadata

```text
RecognitionCompatibilityMetadata
├── InputContentIdentity
├── RecognitionContractVersion
├── RecognitionProfileVersion
├── PreparationProfileVersion
├── DetectionModelVersion?
├── RecognitionModelVersion
├── ProviderProfileVersion
├── LanguageHints[]
├── ScriptHints[]
├── OrientationHint?
├── ReadingOrderPolicyVersion
├── CoordinateTransformVersion
├── QualityPolicyVersion
├── ConfigurationVersions[]
└── PrivacyPartition
```

Recognition defines semantic dependency.

Cache Policy decides reuse.

---

## 57. Compatibility Evaluation

Two Recognition Artifacts may be compatible when:

- InputContentIdentity matches;
- contract major version compatible;
- profile semantics compatible;
- preparation version compatible;
- provider/model differences allowed by Recognition policy;
- language/script/orientation requirements compatible;
- coordinate transform semantics compatible;
- privacy partition allows reuse;
- required output fields present.

RevisionId match không bắt buộc.

---

## 58. Validation Rules — Attempt Input

Invalid khi:

- unsupported major contract version;
- missing Runtime identity;
- missing InputArtifactRef;
- unsupported Artifact type;
- invalid image metadata;
- invalid CoordinateSpace;
- RegionSelection out of bounds;
- conflicting LocalOnly/RemoteAllowed;
- invalid profile;
- impossible capability requirement;
- privacy context missing;
- cancellation context malformed.

---

## 59. Validation Rules — Candidate Artifact

Before submission:

- CandidateArtifactId present;
- OwnerModule = recognition;
- ArtifactType = RECOGNITION_ARTIFACT;
- InputArtifactRef present;
- ContentIdentity present;
- RegionId unique;
- LineId unique;
- line references valid region;
- ReadingOrder references valid region;
- OrderIndex unique;
- geometry in bounds;
- confidence normalized range valid;
- ProviderProvenance present;
- Completeness consistent;
- region/line counts consistent;
- no SDK object in Metadata;
- no credential;
- transform chain valid;
- CompatibilityMetadata complete enough for declared reuse scope.

---

## 60. Validation Rules — Published Artifact

Published Artifact validation belongs to Artifact Store/Runtime, nhưng Recognition contract requires:

- all Candidate semantic invariants preserved;
- ArtifactId assigned;
- ownership transferred;
- publication atomic;
- no mutable Candidate-only reference remains;
- no Attempt state embedded.

---

## 61. Authority Rules

1. Recognition cannot accept/reject Runtime authority.
2. Candidate may be technically valid but authority-rejected.
3. Late provider output may create no published Artifact.
4. Canceled Attempt may still physically finish.
5. Recognition Completion does not imply publication.
6. Runtime Control owns current Revision relevance.
7. Artifact Store publication requires Runtime approval.

---

## 62. Cancellation Rules

1. Recognition checks CancellationContext at declared checkpoints.
2. Cancellation request does not guarantee immediate physical stop.
3. Provider interruption capability must be declared.
4. Recognition must release Attempt-local resource.
5. Recognition must not publish Artifact.
6. Late provider output remains traceable but not authoritative.
7. Runtime terminal outcome stays singular.
8. Cancellation reason remains Runtime-owned.

---

## 63. Empty Artifact Contract

No readable text:

```text
RecognitionArtifact
├── Completeness = EMPTY_VALID
├── Regions = []
├── ReadingOrder = []
└── Warnings includes NO_READABLE_TEXT_DETECTED
```

Không phải failure.

Failure nghĩa là requested Recognition execution không thể hoàn thành đáng tin cậy.

---

## 64. Partial Artifact Contract

Khi `AllowPartialCandidate = true`:

```text
CandidateRecognitionArtifact
├── Completeness = PARTIAL
├── Regions = usable recognized regions
└── Warnings includes PARTIAL_RECOGNITION
```

Rules:

1. Partial explicit.
2. Failed regions ghi trong warning/diagnostics metadata.
3. Không che giấu total provider failure.
4. Runtime/consumer quyết định có publish/use hay không.
5. Partial Candidate không cache eligible mặc định.
6. Recognition không tạo `CompletedWithWarnings` status.

---

## 65. Privacy Contract

### Local-Only Guarantee

Khi:

```text
LocalProcessingRequired = true
```

Guarantees:

- no image data sent remote;
- no recognized text sent remote;
- remote provider not selected;
- remote fallback disabled;
- provenance reports local execution.

### Remote Disclosure

Remote execution phải reflect:

```text
ExecutionLocation = REMOTE_SERVICE
```

và normally include:

```text
REMOTE_PROVIDER_USED
```

### Logging

Normal logs may include:

- Runtime IDs;
- provider ID;
- duration;
- region count;
- warning count;
- error code;
- operation phase.

Must not include:

- image payload;
- complete recognized text;
- provider token;
- API key;
- authorization header;
- sensitive temporary path;
- full remote response.

---

## 66. Producer Obligations

Recognition implementation must:

1. validate Attempt input;
2. preserve Runtime identity;
3. normalize provider output;
4. return source-space geometry;
5. preserve raw recognized text;
6. represent uncertainty explicitly;
7. create immutable Candidate;
8. normalize errors;
9. separate warning from error;
10. enforce local-only privacy;
11. release Attempt-local resource;
12. use Resource Lease correctly;
13. never grant authority;
14. never publish accepted Artifact;
15. never retry itself;
16. never own provider lifetime;
17. never depend on Translation implementation;
18. never depend on UI;
19. maintain contract compatibility;
20. keep diagnostics content-safe.

---

## 67. Consumer Obligations

Consumers of Recognition Artifact must:

1. honor immutability;
2. use explicit ReadingOrder;
3. handle UNKNOWN confidence;
4. handle EMPTY_VALID;
5. handle PARTIAL separately;
6. not assume line geometry exists;
7. not assume all geometry rectangular forever;
8. not parse ProviderMetadata for core behavior;
9. not treat hints as detected facts;
10. perform semantic cleanup outside Recognition;
11. not expose raw text in unsafe logs;
12. preserve Artifact traceability;
13. use ArtifactRef/Lease for shared access;
14. not mutate Recognition Artifact for corrections;
15. handle unknown enum values safely.

---

## 68. Runtime Obligations

Runtime must:

1. create WorkItem/Attempt identity;
2. supply immutable input ArtifactRef;
3. provide ExecutionContextRef;
4. provide CancellationContextRef;
5. own timeout/deadline;
6. own Scheduler admission;
7. own retry decision;
8. own authority validation;
9. own terminal Attempt outcome;
10. coordinate Candidate cleanup;
11. transfer ownership to Artifact Store;
12. publish atomically;
13. reject stale/duplicate/unauthorized Candidate;
14. maintain Artifact lifecycle;
15. provide Cache Policy;
16. keep provider availability outside Recognition contract.

---

## 69. Provider Manager Obligations

Provider Manager must:

- maintain provider registry;
- initialize provider;
- own health state;
- own model/client lifecycle;
- enforce concurrency;
- resolve credentials;
- expose capability;
- supervise shutdown;
- preserve privacy;
- provide selected adapter through stable contract.

---

## 70. Artifact Store Obligations

Artifact Store must:

- register Candidate transfer;
- verify transfer preconditions;
- assign ArtifactId;
- own published payload;
- publish atomically;
- provide immutable lookup;
- coordinate Lease;
- coordinate retention/disposal;
- reject duplicate publication;
- clean up failed transfer.

---

## 71. Serialization Guidance

Recommended:

```text
In-process:
- typed native objects

Cross-process:
- Protocol Buffers
- JSON
- MessagePack
```

JSON naming:

```text
snake_case
```

Large payload uses references.

---

## 72. Contract Evolution

### Backward-Compatible

Allowed within same major:

- optional fields;
- enum values when consumers support unknown handling;
- warning/error codes;
- capability additions;
- optional diagnostic facts;
- clarified descriptions;
- optional metadata keys.

### Breaking

Major required for:

- field removal/rename;
- semantic meaning change;
- coordinate convention change;
- confidence range change;
- optional → required;
- identity semantics change;
- privacy guarantee change;
- raw-text preservation change;
- Candidate/publication boundary change;
- ownership semantics change.

### Unknown Values

Consumers:

- preserve unknown where possible;
- fallback safely;
- not crash on unknown warning;
- treat unknown capability unsupported;
- treat unknown confidence UNKNOWN;
- reject unsupported major version.

---

## 73. Example Recognition Attempt Input

```json
{
  "runtime_context": {
    "contract_version": "1.0.0",
    "application_instance_id": "app_01",
    "session_id": "session_01",
    "revision_id": "revision_104",
    "work_item_id": "work_recognition_104",
    "attempt_id": "attempt_01",
    "configuration_snapshot_id": "config_42",
    "trace_context": {
      "trace_id": "trace_01"
    },
    "created_at": "2026-08-03T01:15:42.184Z"
  },
  "input_artifact_ref": {
    "artifact_id": "image_artifact_104",
    "artifact_type": "IMAGE_ARTIFACT",
    "contract_version": "1.0.0",
    "resource_id": "resource_image_104",
    "content_identity": {
      "identity_algorithm": "sha256",
      "identity_version": "1",
      "value": "content_identity_redacted"
    }
  },
  "operation": "RECOGNIZE_IMAGE",
  "source_coordinate_space": {
    "space_id": "frame_104",
    "width": 1600,
    "height": 900,
    "origin": "TOP_LEFT",
    "unit": "PIXEL",
    "rotation_degrees": 0
  },
  "options": {
    "profile": "COMIC_PAGE",
    "language_hints": ["zh-Hans"],
    "script_hints": ["Hans"],
    "orientation_hint": "MIXED",
    "reading_direction_hint": "TOP_TO_BOTTOM",
    "return_line_geometry": true,
    "return_provider_alternatives": false,
    "allow_partial_candidate": true,
    "diagnostic_level": "BASIC"
  },
  "capability_requirements": {
    "region_detection_required": true,
    "text_recognition_required": true,
    "combined_recognition_allowed": true,
    "line_geometry_required": false,
    "local_only": true,
    "remote_allowed": false,
    "execution_classes": ["CPU", "GPU"]
  },
  "execution_context_ref": {
    "execution_class": "GPU"
  },
  "cancellation_context_ref": {
    "cancellation_id": "cancel_attempt_01",
    "is_cancellation_requested": false
  },
  "privacy_context_ref": {
    "privacy_mode": "LOCAL_ONLY",
    "privacy_partition": "profile_local",
    "local_processing_required": true,
    "remote_processing_allowed": false,
    "diagnostic_content_allowed": false,
    "persistence_allowed": false
  }
}
```

---

## 74. Example Candidate Recognition Artifact

```json
{
  "candidate_artifact_id": "candidate_recognition_104",
  "artifact_type": "RECOGNITION_ARTIFACT",
  "owner_module": "recognition",
  "contract_version": "1.0.0",
  "input_artifact_ref": {
    "artifact_id": "image_artifact_104",
    "artifact_type": "IMAGE_ARTIFACT"
  },
  "content_identity": {
    "identity_algorithm": "sha256",
    "identity_version": "1",
    "value": "content_identity_redacted"
  },
  "source_coordinate_space": {
    "space_id": "frame_104",
    "width": 1600,
    "height": 900,
    "origin": "TOP_LEFT",
    "unit": "PIXEL",
    "rotation_degrees": 0
  },
  "provider_provenance": {
    "provider_id": "local_recognition_01",
    "provider_version": "1.2.0",
    "adapter_version": "1.0.0",
    "execution_location": "LOCAL_PROCESS",
    "execution_class": "GPU",
    "model_id": "chinese_comic_recognition",
    "model_version": "0.4",
    "fallback_index": 0
  },
  "regions": [
    {
      "region_id": "region_01",
      "geometry": {
        "geometry_type": "RECTANGLE",
        "x": 104,
        "y": 88,
        "width": 260,
        "height": 142
      },
      "raw_text": "你今天怎么来了？",
      "surface_text": "你今天怎么来了？",
      "lines": [],
      "detection_confidence": {
        "level": "HIGH",
        "normalized_value": 0.95,
        "source": "DETECTOR"
      },
      "recognition_confidence": {
        "level": "HIGH",
        "normalized_value": 0.91,
        "source": "RECOGNIZER"
      },
      "orientation": "HORIZONTAL",
      "reading_direction": "LEFT_TO_RIGHT",
      "region_type": "SPEECH_BUBBLE_TEXT",
      "geometry_source": "PROVIDER_DETECTED",
      "alternatives": [],
      "warnings": []
    }
  ],
  "reading_order": [
    {
      "order_index": 0,
      "region_id": "region_01",
      "order_source": "COMBINED_HEURISTIC",
      "confidence": {
        "level": "MEDIUM",
        "normalized_value": 0.76,
        "source": "HEURISTIC"
      },
      "manually_overridden": false,
      "rule_id": "comic_mixed_layout_v1"
    }
  ],
  "warnings": [
    {
      "warning_code": "READING_ORDER_UNCERTAIN",
      "operation_phase": "ORDERING",
      "severity": "DEGRADED",
      "message_key": "recognition.reading_order_uncertain"
    }
  ],
  "completeness": "COMPLETE"
}
```

---

## 75. Example Empty Candidate

```json
{
  "candidate_artifact_id": "candidate_recognition_105",
  "artifact_type": "RECOGNITION_ARTIFACT",
  "owner_module": "recognition",
  "contract_version": "1.0.0",
  "regions": [],
  "reading_order": [],
  "warnings": [
    {
      "warning_code": "NO_READABLE_TEXT_DETECTED",
      "operation_phase": "RECOGNIZING",
      "severity": "INFORMATION",
      "message_key": "recognition.no_readable_text"
    }
  ],
  "completeness": "EMPTY_VALID"
}
```

---

## 76. Contract Test Requirements

### Attempt Input

- valid full-image input;
- valid region input;
- invalid ArtifactRef;
- unsupported Artifact type;
- invalid coordinate space;
- invalid region;
- unsupported major version;
- privacy conflict;
- malformed capability requirement.

### Candidate Artifact

- valid Candidate;
- Candidate with warnings;
- empty-valid Candidate;
- partial Candidate;
- missing line geometry;
- unknown confidence;
- mixed orientation;
- mixed language;
- source-coordinate mapping;
- duplicate RegionId;
- invalid ReadingOrder reference;
- missing compatibility metadata.

### Error and Cancellation

- provider unavailable;
- provider timeout normalized;
- invalid provider response;
- preparation failure;
- coordinate mapping failure;
- cancellation checkpoint;
- non-cancelable provider;
- resource exhaustion;
- late output returned to Runtime.

### Provider Adapter

- local-only enforcement;
- confidence normalization;
- coordinate normalization;
- no SDK leakage;
- no hidden retry;
- empty result mapping;
- credential redaction.

### Privacy

- raw image excluded from events/logs;
- raw text excluded from normal logs;
- remote provider disclosure;
- protected diagnostics authorization;
- EPHEMERAL persistence restriction.

---

## 77. Contract Invariants

1. Recognition input is image-based Artifact.
2. Runtime identity explicit.
3. Candidate and published Artifact are different.
4. Recognition creates Candidate only.
5. Runtime owns authority.
6. Artifact Store owns published payload.
7. Provider SDK type never crosses adapter boundary.
8. Provider credential never appears in public output.
9. Local-only never sends source data remote.
10. Public geometry maps to source coordinate space.
11. RegionId unique.
12. LineId unique.
13. Line references existing Region.
14. ReadingOrder references existing Region.
15. Raw recognized text preserved.
16. Semantic correction outside Recognition.
17. Missing confidence = UNKNOWN.
18. Empty text may be successful.
19. Partial output explicit.
20. Warning does not replace error.
21. Candidate immutable after assembly.
22. Published Artifact immutable.
23. Recognition never retries itself.
24. Recognition never owns cancellation authority.
25. Recognition never publishes accepted Artifact.
26. Recognition never owns provider lifecycle.
27. Recognition output contains source text only.
28. Runtime Attempt status is not embedded in Artifact.
29. Queue/retry timing is not embedded in Artifact.
30. Compatibility metadata is explicit.
31. RevisionId is not reuse identity by default.
32. Input Artifact accessed via Lease.
33. Candidate rejection triggers cleanup.
34. Late provider output cannot gain authority.
35. Diagnostic facts do not create hidden orchestration.
36. Event/log payloads remain content-safe.
37. Unknown enum values handled safely.
38. Contract major version protects semantic compatibility.
39. Consumer does not parse provider metadata for core logic.
40. User correction never mutates original Recognition Artifact.

---

## 78. MVP Contract Subset

### Required Operation

```text
RECOGNIZE_IMAGE
RECOGNIZE_REGION
```

### Required Input

```text
Image ArtifactRef
Source CoordinateSpace
Simplified Chinese hint
English hint
AUTOMATIC profile
COMIC_PAGE profile
SINGLE_REGION profile
LOCAL_ONLY policy support
CancellationContextRef
ExecutionContextRef
```

### Required Candidate Output

```text
CandidateArtifactId
ProviderProvenance
Rectangle regions
RawText
SurfaceText
Source geometry
Confidence when available
Orientation
Initial ReadingOrder
Warnings
Completeness
CompatibilityMetadata
TraceabilityMetadata
```

### Optional for MVP

```text
Line geometry
Polygon geometry
Recognition alternatives
Language auto-detection
Script auto-detection
Partial Candidate
Remote providers
Character-level geometry
STRUCTURED_PAGE profile
```

No Recognition-owned terminal events are required.

---

## 79. Deferred Extensions

Potential future:

- streaming provider output;
- long-page chunks;
- tiled-image recognition;
- character geometry;
- speech-bubble geometry;
- sound-effect recognition;
- handwriting;
- table/formula recognition;
- cross-frame region tracking;
- incremental Artifact derivation;
- provider ensemble voting;
- benchmark metadata;
- region semantic classification;
- Artifact-difference contract;
- manual reading-order correction;
- OCR correction feedback;
- encrypted remote envelope.

Chỉ thêm khi capability cụ thể yêu cầu.

---

## 80. Related Documents

```text
modules/recognition/README.md
modules/recognition/MODULE.md
modules/recognition/PIPELINE.md
modules/recognition/PROVIDER.md
modules/recognition/COORDINATE_MODEL.md
modules/recognition/READING_ORDER.md
modules/recognition/QUALITY_MODEL.md
modules/recognition/ERRORS.md
modules/recognition/EVENTS.md
runtime/PIPELINE_RUNTIME.md
runtime/CANCELLATION.md
runtime/RETRY_POLICY.md
runtime/RESOURCE_LIFECYCLE.md
runtime/CACHE_POLICY.md
runtime/RUNTIME_OBSERVABILITY.md
modules/text-processing/README.md
```

---

## 81. Summary

Recognition Contract định nghĩa cách Runtime đưa image Artifact vào Recognition execution và nhận Candidate structured source-content Artifact.

```text
RecognitionAttemptInput
    ↓
Recognition Execution
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

- provider-independent domain contract;
- immutable Candidate/Artifact;
- source-relative geometry;
- explicit reading order;
- explicit uncertainty;
- normalized warnings/errors;
- local-only privacy enforcement;
- safe cancellation cooperation;
- clear authority/publication separation;
- compatibility metadata for safe reuse.

Recognition contract rộng hơn một OCR string interface, nhưng hẹp hơn Runtime orchestration contract.
