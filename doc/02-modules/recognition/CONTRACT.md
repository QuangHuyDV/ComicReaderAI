# Recognition Module Contract

> **Project:** CRAI
> **Module:** Recognition
> **Path:** `doc/02-modules/recognition/CONTRACT.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-22

---

## 1. Purpose

This document defines the public contract of the Recognition module.

It specifies:

* accepted inputs;
* produced outputs;
* public commands;
* public queries;
* domain identifiers;
* result structures;
* provider contracts;
* event payloads;
* error structures;
* cancellation behavior;
* compatibility rules;
* consumer obligations;
* provider-adapter obligations.

This document does not define:

* OCR implementation algorithms;
* provider SDK integration details;
* image-processing libraries;
* persistence technology;
* UI behavior;
* source-capture behavior;
* translation behavior.

The contract is intended to remain stable even when the internal OCR implementation changes.

---

## 2. Contract Boundary

The Recognition module accepts image-based source content and returns structured recognized text.

```text
Image Reference
    +
Recognition Request
    ↓
Recognition Module
    ↓
Recognition Result
```

The public contract begins with one of the following operations:

```text
RecognizeImage
RecognizeRegion
RetryRecognition
CancelRecognition
```

The public contract ends with one of the following outcomes:

```text
RecognitionResult
RecognitionError
RecognitionCancellationResult
```

Recognition does not accept already extracted plain text as its primary input.

Recognition does not return translated text.

---

## 3. Contract Principles

The Recognition contract follows these principles.

### 3.1 Provider Independence

Consumers must not depend on provider-specific SDK models.

All provider results must be normalized into CRAI contracts.

### 3.2 Stable Source Mapping

Every recognized region must map back to the source coordinate space.

### 3.3 Immutable Results

A completed recognition result must not be modified.

Corrections or transformations create separate domain objects.

### 3.4 Explicit Identity

Requests, source content, frames, recognition operations, regions, and lines must have explicit identifiers.

### 3.5 Explicit Reading Order

Consumers must not rely only on array position to infer reading order.

### 3.6 Explicit Uncertainty

Unknown confidence, unknown language, and uncertain reading order must remain explicit.

### 3.7 Cancellation Safety

A cancelled request must not later be reported as successfully completed.

### 3.8 Privacy Preservation

Raw image data and recognized text must not be included in ordinary events or logs.

### 3.9 Backward Compatibility

Contract evolution must preserve existing fields and meanings unless a major contract version is introduced.

---

## 4. Contract Version

```text
RecognitionContractVersion
├── major: integer
├── minor: integer
└── patch: integer
```

Initial version:

```text
1.0.0
```

Version meaning:

* `major`: incompatible contract change;
* `minor`: backward-compatible field or capability addition;
* `patch`: documentation clarification or non-semantic correction.

Every command and event should carry a contract version when crossing process or module boundaries.

---

## 5. Common Types

### 5.1 Identifier Types

```text
RequestId = string
RecognitionId = string
SessionId = string
SourceId = string
ContentId = string
FrameId = string
RegionId = string
LineId = string
ProviderId = string
ProviderRequestId = string
TraceId = string
ConfigurationVersion = string
PreprocessingProfileId = string
```

Identifiers must be opaque to consumers.

Consumers must not infer timestamps, provider names, or ordering from identifier contents.

---

### 5.2 Timestamp

```text
Timestamp = ISO-8601 UTC datetime
```

Example:

```text
2026-07-22T03:15:42.184Z
```

All contract timestamps must use UTC.

Display conversion belongs to consumers.

---

### 5.3 Duration

```text
DurationMilliseconds = integer
```

Duration values must be non-negative.

---

### 5.4 Language Code

```text
LanguageCode = BCP-47-compatible string
```

Examples:

```text
zh-Hans
zh-Hant
en
vi
ja
ko
```

Recognition may return a more general code when exact language identification is unavailable.

Example:

```text
zh
```

---

### 5.5 Script Code

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

---

### 5.6 Metadata

```text
Metadata = map<string, scalar | scalar[]>
```

Allowed scalar types:

```text
string
integer
decimal
boolean
null
```

Metadata must not contain:

* raw image bytes;
* arbitrary provider objects;
* executable values;
* credentials;
* unrestricted nested object graphs.

---

## 6. Image Reference Contract

Recognition requests must use an image reference rather than embedding image bytes directly into event payloads.

```text
ImageReference
├── reference_type: ImageReferenceType
├── reference_value: string
├── media_type: string
├── width: integer
├── height: integer
├── byte_size?: integer
├── checksum?: string
├── created_at?: Timestamp
└── expires_at?: Timestamp
```

### 6.1 Image Reference Type

```text
ImageReferenceType
├── InMemoryHandle
├── TemporaryFile
├── SharedBuffer
├── ContentStoreReference
└── ExternalResourceReference
```

`ExternalResourceReference` may be disabled by policy.

Recognition must not assume all references are permanent.

---

### 6.2 Media Types

Initial supported media types may include:

```text
image/png
image/jpeg
image/webp
image/bmp
```

Support must be reported by provider or module capability contracts.

---

### 6.3 Image Reference Rules

1. `width` and `height` must be positive.
2. The referenced image must remain available until request completion or cancellation.
3. Recognition must not retain the reference beyond its allowed lifecycle.
4. A temporary reference must include sufficient cleanup ownership information outside this contract.
5. Remote providers must receive image data only when the request policy permits it.
6. Consumers must not publish raw image references to untrusted subscribers.

---

## 7. Coordinate Space Contract

### 7.1 Coordinate Space

```text
CoordinateSpace
├── space_id: string
├── width: decimal
├── height: decimal
├── origin: CoordinateOrigin
├── unit: CoordinateUnit
└── rotation_degrees: decimal
```

### 7.2 Coordinate Origin

```text
CoordinateOrigin
├── TopLeft
├── TopRight
├── BottomLeft
└── BottomRight
```

Initial CRAI contracts should use:

```text
TopLeft
```

### 7.3 Coordinate Unit

```text
CoordinateUnit
├── Pixel
└── Normalized
```

For `Normalized` coordinates:

```text
x, y, width, height ∈ [0, 1]
```

Initial Recognition output should prefer pixel coordinates in source space.

---

## 8. Geometry Contract

### 8.1 Rectangle Geometry

```text
RectangleGeometry
├── geometry_type: Rectangle
├── x: decimal
├── y: decimal
├── width: decimal
└── height: decimal
```

Validation:

```text
width > 0
height > 0
x >= 0
y >= 0
x + width <= coordinate_space.width
y + height <= coordinate_space.height
```

A small floating-point tolerance may be allowed during transform calculations.

---

### 8.2 Polygon Geometry

```text
PolygonGeometry
├── geometry_type: Polygon
├── points: Point[]
└── bounding_rectangle: RectangleGeometry
```

```text
Point
├── x: decimal
└── y: decimal
```

Polygon support is optional for the MVP.

Consumers must use `geometry_type` instead of assuming rectangular geometry permanently.

---

### 8.3 Geometry Union

```text
Geometry =
    RectangleGeometry
    | PolygonGeometry
```

---

## 9. Recognition Request Context

Common request context:

```text
RecognitionRequestContext
├── contract_version: string
├── request_id: RequestId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── trace_context: TraceContext
├── priority: ProcessingPriority
├── timeout_ms?: integer
├── created_at: Timestamp
└── requested_by?: RequestActor
```

### 9.1 Trace Context

```text
TraceContext
├── trace_id: TraceId
├── parent_span_id?: string
├── correlation_id?: string
└── baggage?: Metadata
```

Trace baggage must not contain source text or image content.

---

### 9.2 Request Actor

```text
RequestActor
├── User
├── SessionOrchestrator
├── ObservationModule
├── RetryPolicy
├── ImportWorkflow
└── DiagnosticWorkflow
```

---

### 9.3 Processing Priority

```text
ProcessingPriority
├── Immediate
├── Interactive
├── Normal
├── Background
└── Preload
```

Priority is a scheduling hint.

It does not guarantee execution order.

---

## 10. Recognition Options

```text
RecognitionOptions
├── mode: RecognitionMode
├── language_hints: LanguageCode[]
├── script_hints: ScriptCode[]
├── orientation_hint?: TextOrientation
├── reading_direction_hint?: ReadingDirection
├── provider_policy: RecognitionProviderPolicy
├── preprocessing_profile_id?: PreprocessingProfileId
├── return_line_geometry: boolean
├── return_provider_alternatives: boolean
├── allow_partial_result: boolean
└── diagnostic_level: DiagnosticLevel
```

### 10.1 Recognition Mode

```text
RecognitionMode
├── Automatic
├── ComicPage
├── Screenshot
├── SingleRegion
└── StructuredPage
```

---

### 10.2 Text Orientation

```text
TextOrientation
├── Unknown
├── Horizontal
├── Vertical
├── Rotated90Clockwise
├── Rotated90CounterClockwise
├── UpsideDown
└── Mixed
```

---

### 10.3 Reading Direction

```text
ReadingDirection
├── Unknown
├── LeftToRight
├── RightToLeft
├── TopToBottom
├── BottomToTop
├── VerticalColumnsRightToLeft
├── VerticalColumnsLeftToRight
└── Mixed
```

---

### 10.4 Diagnostic Level

```text
DiagnosticLevel
├── None
├── Basic
├── Detailed
└── ProtectedContent
```

`ProtectedContent` diagnostics require explicit privacy authorization.

---

## 11. Provider Policy Contract

```text
RecognitionProviderPolicy
├── selection_mode: ProviderSelectionMode
├── preferred_provider_id?: ProviderId
├── required_capabilities: RecognitionCapability[]
├── excluded_provider_ids: ProviderId[]
├── local_processing_required: boolean
├── remote_processing_allowed: boolean
├── fallback_allowed: boolean
├── maximum_fallback_count: integer
├── maximum_expected_latency_ms?: integer
└── execution_device_preference?: ExecutionDevice
```

### 11.1 Provider Selection Mode

```text
ProviderSelectionMode
├── ExactProvider
├── PreferredProvider
├── Automatic
└── DefaultProvider
```

### 11.2 Execution Device

```text
ExecutionDevice
├── Automatic
├── CPU
├── GPU
├── NPU
└── Remote
```

### 11.3 Provider Policy Rules

1. `ExactProvider` requires `preferred_provider_id`.
2. `local_processing_required = true` implies `remote_processing_allowed = false`.
3. `maximum_fallback_count` must be zero when fallback is disabled.
4. An excluded provider must never be selected.
5. A provider must satisfy all required capabilities.
6. A failed local request must not fall back to remote execution unless explicitly allowed.
7. Provider selection decisions must be visible in result metadata.

---

## 12. Recognize Image Command

### 12.1 Command Name

```text
recognition.recognize_image
```

### 12.2 Request

```text
RecognizeImageRequest
├── context: RecognitionRequestContext
├── image: ImageReference
├── source_coordinate_space: CoordinateSpace
└── options: RecognitionOptions
```

### 12.3 Preconditions

1. `request_id` is unique among active requests.
2. The image reference is resolvable.
3. Image dimensions match or are compatible with the coordinate space.
4. The request has not expired.
5. The provider policy is internally valid.
6. At least one configured provider can potentially satisfy the request.
7. The source content is authorized for processing.
8. Remote processing is permitted when a remote provider is selected.

### 12.4 Successful Response

```text
RecognitionAccepted
├── request_id: RequestId
├── accepted_at: Timestamp
├── execution_mode: RecognitionExecutionMode
└── provider_id?: ProviderId
```

### 12.5 Execution Mode

```text
RecognitionExecutionMode
├── Synchronous
└── Asynchronous
```

For synchronous use, the operation may directly return `RecognitionResult`.

For asynchronous use, it returns `RecognitionAccepted` and later publishes a terminal event.

---

## 13. Recognize Region Command

### 13.1 Command Name

```text
recognition.recognize_region
```

### 13.2 Request

```text
RecognizeRegionRequest
├── context: RecognitionRequestContext
├── image: ImageReference
├── source_coordinate_space: CoordinateSpace
├── region: Geometry
└── options: RecognitionOptions
```

### 13.3 Region Rules

1. The region must be inside the source coordinate space.
2. The region must have a non-zero area.
3. The result must still return geometry in source coordinate space.
4. `SingleRegion` mode should normally be used.
5. Detection may be skipped when the provider supports direct region recognition.
6. The module may subdivide the region internally.
7. Internal subdivisions must not alter the public source geometry mapping.

---

## 14. Retry Recognition Command

### 14.1 Command Name

```text
recognition.retry
```

### 14.2 Request

```text
RetryRecognitionRequest
├── context: RecognitionRequestContext
├── previous_recognition_id: RecognitionId
├── retry_scope: RecognitionRetryScope
├── region_ids: RegionId[]
├── image?: ImageReference
├── source_coordinate_space?: CoordinateSpace
├── options_override: RecognitionOptionsOverride
└── retry_reason: RecognitionRetryReason
```

### 14.3 Retry Scope

```text
RecognitionRetryScope
├── EntireImage
├── FailedRegions
├── LowConfidenceRegions
├── SelectedRegions
└── ReadingOrderOnly
```

### 14.4 Retry Reason

```text
RecognitionRetryReason
├── UserRequested
├── LowConfidence
├── ProviderFailure
├── UnsupportedOrientationFallback
├── AlternativeProviderEvaluation
├── ConfigurationChanged
└── DiagnosticComparison
```

### 14.5 Recognition Options Override

```text
RecognitionOptionsOverride
├── mode?: RecognitionMode
├── language_hints?: LanguageCode[]
├── script_hints?: ScriptCode[]
├── orientation_hint?: TextOrientation
├── reading_direction_hint?: ReadingDirection
├── provider_policy?: RecognitionProviderPolicy
├── preprocessing_profile_id?: PreprocessingProfileId
├── return_line_geometry?: boolean
├── return_provider_alternatives?: boolean
├── allow_partial_result?: boolean
└── diagnostic_level?: DiagnosticLevel
```

### 14.6 Retry Rules

1. A retry creates a new `request_id`.
2. A retry creates a new `recognition_id`.
3. The previous result remains immutable.
4. The new result references the previous result.
5. Retry count must be bounded by orchestration policy.
6. A retry must not silently change privacy policy.
7. Reusing the original image requires the reference to remain available.
8. Missing source content must return a normalized error.

---

## 15. Cancel Recognition Command

### 15.1 Command Name

```text
recognition.cancel
```

### 15.2 Request

```text
CancelRecognitionRequest
├── contract_version: string
├── request_id: RequestId
├── reason?: CancellationReason
├── requested_at: Timestamp
└── trace_context: TraceContext
```

### 15.3 Cancellation Reason

```text
CancellationReason
├── UserCancelled
├── SessionStopped
├── SourceChanged
├── NewerFrameAvailable
├── RequestSuperseded
├── ApplicationShutdown
├── Timeout
└── ResourcePressure
```

### 15.4 Response

```text
RecognitionCancellationResult
├── request_id: RequestId
├── status: CancellationStatus
├── requested_at: Timestamp
└── effective_at?: Timestamp
```

### 15.5 Cancellation Status

```text
CancellationStatus
├── CancellationRequested
├── AlreadyCancelled
├── AlreadyCompleted
├── RequestNotFound
└── CancellationUnsupported
```

`CancellationUnsupported` means the provider cannot be interrupted immediately.

The module must still suppress obsolete completion publication when cancellation has been accepted internally.

---

## 16. Recognition Result Contract

```text
RecognitionResult
├── contract_version: string
├── recognition_id: RecognitionId
├── request_id: RequestId
├── previous_recognition_id?: RecognitionId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── status: RecognitionStatus
├── provider: RecognitionProviderIdentity
├── source_coordinate_space: CoordinateSpace
├── processed_image: ProcessedImageMetadata
├── language_hypotheses: LanguageHypothesis[]
├── script_hypotheses: ScriptHypothesis[]
├── regions: RecognizedRegion[]
├── reading_order: ReadingOrderEntry[]
├── warnings: RecognitionWarning[]
├── metrics: RecognitionMetrics
├── started_at: Timestamp
├── completed_at: Timestamp
├── trace_context: TraceContext
└── result_metadata?: Metadata
```

---

## 17. Recognition Status Contract

```text
RecognitionStatus
├── Completed
└── CompletedWithWarnings
```

Failed and cancelled operations use terminal errors or cancellation events rather than a successful `RecognitionResult`.

A result containing zero regions may still be `Completed`.

---

## 18. Provider Identity Contract

```text
RecognitionProviderIdentity
├── provider_id: ProviderId
├── provider_name: string
├── provider_version: string
├── adapter_version: string
├── execution_location: ExecutionLocation
├── execution_device: ExecutionDevice
├── model_id?: string
├── model_version?: string
└── fallback_index: integer
```

### 18.1 Execution Location

```text
ExecutionLocation
├── LocalProcess
├── LocalSidecar
├── LocalService
└── RemoteService
```

Credentials, endpoint secrets, and internal SDK information must not be exposed.

---

## 19. Processed Image Metadata

```text
ProcessedImageMetadata
├── source_width: integer
├── source_height: integer
├── processed_width: integer
├── processed_height: integer
├── preprocessing_profile_id?: PreprocessingProfileId
├── operations: PreprocessingOperationSummary[]
├── coordinate_transform: CoordinateTransformSummary
├── source_checksum?: string
└── processed_checksum?: string
```

### 19.1 Preprocessing Operation Summary

```text
PreprocessingOperationSummary
├── operation: PreprocessingOperationType
├── changed_geometry: boolean
└── parameters_summary?: Metadata
```

### 19.2 Preprocessing Operation Type

```text
PreprocessingOperationType
├── Crop
├── Resize
├── Upscale
├── Grayscale
├── Contrast
├── Brightness
├── Threshold
├── Denoise
├── Sharpen
├── Deskew
├── Rotate
├── Invert
├── Pad
└── ColorChannelSelection
```

Sensitive provider configuration must not appear in parameter summaries.

---

## 20. Coordinate Transform Summary

```text
CoordinateTransformSummary
├── source_space_id: string
├── processed_space_id: string
├── crop_offset_x: decimal
├── crop_offset_y: decimal
├── scale_x: decimal
├── scale_y: decimal
├── rotation_degrees: decimal
├── padding_left: decimal
├── padding_top: decimal
├── padding_right: decimal
└── padding_bottom: decimal
```

Consumers normally do not need to apply this transform because public region geometry is already mapped to source space.

The transform exists for diagnostics and reproducibility.

---

## 21. Language Hypothesis Contract

```text
LanguageHypothesis
├── language_code: LanguageCode
├── confidence: Confidence
├── source: HypothesisSource
└── region_ids: RegionId[]
```

### 21.1 Script Hypothesis

```text
ScriptHypothesis
├── script_code: ScriptCode
├── confidence: Confidence
├── source: HypothesisSource
└── region_ids: RegionId[]
```

### 21.2 Hypothesis Source

```text
HypothesisSource
├── RequestHint
├── ProviderDetection
├── ScriptClassifier
├── RecognitionOutput
└── CombinedInference
```

Request hints must not be represented as detected facts.

---

## 22. Recognized Region Contract

```text
RecognizedRegion
├── region_id: RegionId
├── geometry: Geometry
├── raw_text: string
├── surface_text: string
├── lines: RecognizedLine[]
├── detection_confidence: Confidence
├── recognition_confidence: Confidence
├── orientation: TextOrientation
├── reading_direction: ReadingDirection
├── language_hypothesis?: LanguageHypothesisReference
├── script_hypothesis?: ScriptHypothesisReference
├── region_type: RecognitionRegionType
├── geometry_source: GeometrySource
├── alternatives: RecognitionAlternative[]
├── warnings: RecognitionWarning[]
└── provider_metadata?: Metadata
```

### 22.1 Region Type

```text
RecognitionRegionType
├── Unknown
├── TextBlock
├── TextLine
├── VerticalColumn
├── SpeechBubbleText
├── Caption
├── PageTitle
├── InterfaceText
├── SoundEffectCandidate
└── DecorativeText
```

Region type is a recognition hint, not semantic truth.

---

### 22.2 Geometry Source

```text
GeometrySource
├── ProviderDetected
├── DetectorDetected
├── RequestRegion
├── DerivedFromLines
├── Inferred
└── UserSelected
```

---

### 22.3 Surface Text

`raw_text` preserves normalized provider output as closely as possible.

`surface_text` allows deterministic, non-semantic cleanup.

Allowed operations:

* normalize line separators;
* remove null characters;
* trim outer whitespace;
* normalize invalid Unicode sequences;
* remove documented provider formatting artifacts.

Forbidden operations:

* contextual character correction;
* name correction;
* punctuation interpretation;
* sentence reconstruction;
* text translation;
* glossary substitution;
* inferred missing-word insertion.

---

## 23. Recognized Line Contract

```text
RecognizedLine
├── line_id: LineId
├── region_id: RegionId
├── geometry: Geometry
├── raw_text: string
├── surface_text: string
├── confidence: Confidence
├── orientation: TextOrientation
├── region_order_index: integer
├── geometry_source: GeometrySource
└── provider_metadata?: Metadata
```

Rules:

1. `line_id` must be unique inside the result.
2. `region_id` must reference an existing region.
3. Line geometry should be inside or intersect its parent region.
4. Inferred line geometry must use `GeometrySource.Inferred`.
5. Missing line output must be represented by an empty array.
6. Consumers must not assume every provider supports line geometry.

---

## 24. Recognition Alternative Contract

```text
RecognitionAlternative
├── text: string
├── confidence: Confidence
├── rank: integer
├── source: RecognitionAlternativeSource
└── provider_metadata?: Metadata
```

### 24.1 Alternative Source

```text
RecognitionAlternativeSource
├── ProviderCandidate
├── RetryCandidate
├── PreprocessingCandidate
└── SecondaryProviderCandidate
```

Alternatives are optional.

They should normally be omitted unless requested or required for diagnostics.

---

## 25. Confidence Contract

```text
Confidence
├── level: ConfidenceLevel
├── normalized_value?: decimal
├── raw_value?: decimal
├── raw_scale?: ConfidenceScale
├── source: ConfidenceSource
└── normalization_method?: string
```

### 25.1 Confidence Level

```text
ConfidenceLevel
├── Unknown
├── Low
├── Medium
└── High
```

### 25.2 Confidence Scale

```text
ConfidenceScale
├── ZeroToOne
├── ZeroToHundred
├── LogProbability
├── ProviderSpecific
└── Unknown
```

### 25.3 Confidence Source

```text
ConfidenceSource
├── Provider
├── Detector
├── Recognizer
├── Heuristic
├── Aggregated
└── Unavailable
```

### 25.4 Confidence Rules

1. `normalized_value`, when present, must be between `0` and `1`.
2. `raw_value` may only be interpreted with `raw_scale`.
3. Unknown provider confidence must produce `ConfidenceLevel.Unknown`.
4. Missing confidence must not be converted to zero.
5. Aggregated region confidence must identify its source as `Aggregated`.
6. Confidence thresholds may vary by provider.
7. Consumers should primarily use `level`, not provider raw values.
8. Confidence alone must not automatically determine whether text is discarded.

---

## 26. Reading Order Contract

```text
ReadingOrderEntry
├── order_index: integer
├── region_id: RegionId
├── order_source: ReadingOrderSource
├── confidence: Confidence
├── group_id?: string
├── parent_group_id?: string
├── manually_overridden: boolean
└── rule_id?: string
```

### 26.1 Reading Order Source

```text
ReadingOrderSource
├── Provider
├── SpatialHeuristic
├── OrientationHeuristic
├── CombinedHeuristic
├── RequestHint
├── UserOverride
└── Unknown
```

### 26.2 Reading Order Rules

1. `order_index` begins at zero.
2. Order indexes must be unique in one result.
3. Every region intended for text consumption should have one order entry.
4. Non-readable decorative regions may be omitted.
5. Every referenced `region_id` must exist.
6. Array position may match `order_index`, but consumers must use the explicit field.
7. Mixed layouts may use groups.
8. Reading-order uncertainty must be represented through confidence or warnings.
9. A later user correction must not mutate the original result.

---

## 27. Warning Contract

```text
RecognitionWarning
├── warning_code: RecognitionWarningCode
├── stage: RecognitionStage
├── severity: WarningSeverity
├── message: string
├── region_id?: RegionId
├── line_id?: LineId
├── provider_id?: ProviderId
└── metadata?: Metadata
```

### 27.1 Warning Severity

```text
WarningSeverity
├── Information
├── Degraded
└── AttentionRequired
```

Warnings must not use fatal severity.

Fatal conditions use `RecognitionError`.

---

### 27.2 Recognition Warning Codes

```text
NoReadableTextDetected
LowDetectionConfidence
LowRecognitionConfidence
ReadingOrderUncertain
UnsupportedLanguageFallback
UnsupportedScriptFallback
UnsupportedOrientationFallback
ProviderConfidenceUnavailable
LineGeometryUnavailable
RegionGeometryInferred
OverlappingRegionsSuppressed
DuplicateRegionSuppressed
PartialRecognitionResult
PreprocessingFallbackUsed
ImageUpscaled
ImageDownscaled
ImageRotated
RemoteProviderUsed
FallbackProviderUsed
ProviderAlternativesUnavailable
MixedOrientationDetected
MixedLanguageDetected
ResultTruncated
DiagnosticDataLimited
```

---

## 28. Recognition Stage Contract

```text
RecognitionStage
├── RequestValidation
├── ProviderSelection
├── ImageResolution
├── ImageNormalization
├── Preprocessing
├── RegionDetection
├── TextRecognition
├── RegionPostProcessing
├── ReadingOrder
├── CoordinateMapping
├── ResultAssembly
└── Cancellation
```

---

## 29. Metrics Contract

```text
RecognitionMetrics
├── queue_duration_ms?: integer
├── validation_duration_ms: integer
├── provider_selection_duration_ms: integer
├── image_resolution_duration_ms: integer
├── normalization_duration_ms: integer
├── preprocessing_duration_ms: integer
├── detection_duration_ms: integer
├── recognition_duration_ms: integer
├── post_processing_duration_ms: integer
├── reading_order_duration_ms: integer
├── coordinate_mapping_duration_ms: integer
├── result_assembly_duration_ms: integer
├── total_duration_ms: integer
├── time_to_first_region_ms?: integer
├── source_pixel_count: integer
├── processed_pixel_count: integer
├── detected_region_count: integer
├── recognized_region_count: integer
├── recognized_line_count: integer
├── recognized_character_count: integer
├── retry_count: integer
├── fallback_count: integer
├── provider_id: ProviderId
├── execution_device: ExecutionDevice
├── peak_memory_bytes?: integer
└── provider_metrics?: Metadata
```

Metrics must not expose recognized content.

---

## 30. Recognition Error Contract

```text
RecognitionError
├── contract_version: string
├── error_code: RecognitionErrorCode
├── stage: RecognitionStage
├── message: string
├── retryable: boolean
├── request_id: RequestId
├── recognition_id?: RecognitionId
├── source_id?: SourceId
├── content_id?: ContentId
├── frame_id?: FrameId
├── provider_id?: ProviderId
├── provider_error_code?: string
├── provider_http_status?: integer
├── occurred_at: Timestamp
├── trace_context: TraceContext
├── metadata?: Metadata
└── cause_reference?: string
```

### 30.1 Recognition Error Codes

```text
InvalidRequest
DuplicateRequestId
InvalidImageReference
ImageReferenceExpired
ImageNotFound
InvalidImage
UnsupportedImageFormat
ImageTooLarge
InvalidCoordinateSpace
InvalidRegion
UnsupportedLanguage
UnsupportedScript
UnsupportedOrientation
InvalidProviderPolicy
NoEligibleProvider
ProviderUnavailable
ProviderNotConfigured
ProviderInitializationFailed
ProviderAuthenticationFailed
ProviderPermissionDenied
ProviderRateLimited
ProviderTimeout
ProviderRejectedContent
ProviderProtocolError
ProviderResponseInvalid
PreprocessingFailed
DetectionFailed
RecognitionFailed
CoordinateMappingFailed
ReadingOrderFailed
ResultAssemblyFailed
SourceContentUnavailable
RequestExpired
RequestCancelled
ResourceExhausted
OutOfMemory
HardwareUnavailable
NetworkUnavailable
InternalError
```

---

### 30.2 Error Message Rules

Error messages must:

* be safe for logs;
* avoid raw OCR text;
* avoid image paths when sensitive;
* avoid credentials;
* avoid complete provider responses;
* identify the processing stage;
* be understandable without provider-specific knowledge.

Provider-specific details belong in protected diagnostics.

---

### 30.3 Retryable Errors

Potentially retryable:

```text
ProviderUnavailable
ProviderRateLimited
ProviderTimeout
NetworkUnavailable
HardwareUnavailable
ResourceExhausted
ProviderProtocolError
```

Normally non-retryable:

```text
InvalidRequest
InvalidImageReference
UnsupportedImageFormat
InvalidCoordinateSpace
InvalidRegion
ProviderNotConfigured
ProviderAuthenticationFailed
ProviderPermissionDenied
RequestCancelled
```

The `retryable` field is authoritative for the specific failure.

Consumers must not retry solely based on the error code.

---

## 31. Provider Capability Contract

```text
RecognitionProviderCapabilities
├── contract_version: string
├── provider: RecognitionProviderIdentity
├── operational_status: ProviderOperationalStatus
├── supported_media_types: string[]
├── supported_languages: LanguageCode[]
├── supported_scripts: ScriptCode[]
├── supported_orientations: TextOrientation[]
├── supported_modes: RecognitionMode[]
├── capabilities: RecognitionCapability[]
├── maximum_image_width?: integer
├── maximum_image_height?: integer
├── maximum_image_pixels?: integer
├── recommended_concurrency: integer
├── initialization_cost: InitializationCost
├── privacy_classification: ProviderPrivacyClassification
└── capability_metadata?: Metadata
```

### 31.1 Recognition Capability

```text
RecognitionCapability
├── RegionDetection
├── TextRecognition
├── CombinedDetectionAndRecognition
├── HorizontalText
├── VerticalText
├── MixedOrientation
├── LanguageDetection
├── ScriptDetection
├── ReadingOrder
├── RegionConfidence
├── LineConfidence
├── LineGeometry
├── CharacterGeometry
├── RecognitionAlternatives
├── PartialResults
├── Cancellation
├── BatchRecognition
├── LocalExecution
├── RemoteExecution
├── CPUExecution
├── GPUExecution
└── NPUExecution
```

### 31.2 Provider Operational Status

```text
ProviderOperationalStatus
├── Initializing
├── Ready
├── Degraded
├── Unavailable
└── Misconfigured
```

### 31.3 Initialization Cost

```text
InitializationCost
├── None
├── Low
├── Medium
└── High
```

### 31.4 Provider Privacy Classification

```text
ProviderPrivacyClassification
├── LocalOnly
├── LocalService
├── RemotePrivate
├── RemoteThirdParty
└── Unknown
```

---

## 32. Provider Adapter Input Contract

The internal provider adapter receives a normalized request.

```text
ProviderRecognitionRequest
├── provider_request_id: ProviderRequestId
├── public_request_id: RequestId
├── image_input: ProviderImageInput
├── processed_coordinate_space: CoordinateSpace
├── mode: RecognitionMode
├── language_hints: LanguageCode[]
├── script_hints: ScriptCode[]
├── orientation_hint?: TextOrientation
├── reading_direction_hint?: ReadingDirection
├── return_line_geometry: boolean
├── return_alternatives: boolean
├── timeout_ms?: integer
├── trace_context: TraceContext
└── cancellation_context: ProviderCancellationContext
```

Provider adapters must not receive session or UI objects.

---

## 33. Provider Adapter Output Contract

```text
ProviderRecognitionResponse
├── provider_request_id: ProviderRequestId
├── provider: RecognitionProviderIdentity
├── regions: ProviderRecognizedRegion[]
├── provider_reading_order?: ProviderReadingOrderEntry[]
├── detected_languages?: ProviderLanguageHypothesis[]
├── detected_scripts?: ProviderScriptHypothesis[]
├── warnings: ProviderWarning[]
├── metrics?: ProviderExecutionMetrics
└── metadata?: Metadata
```

All provider response types are internal to the Recognition module.

They must be normalized before crossing the public module boundary.

---

## 34. Provider Adapter Obligations

Every provider adapter must:

1. validate provider configuration;
2. report capabilities accurately;
3. convert normalized requests into provider requests;
4. normalize provider coordinates;
5. normalize provider language identifiers;
6. normalize provider orientation identifiers;
7. normalize provider confidence;
8. normalize provider errors;
9. preserve raw recognized text;
10. support cancellation where available;
11. declare when cancellation is unsupported;
12. remove provider credentials from outputs;
13. avoid leaking SDK classes;
14. report provider and model versions;
15. expose operational health;
16. document fallback behavior;
17. release provider resources;
18. map empty OCR results to a successful empty result where appropriate;
19. distinguish provider failure from no-text detection;
20. follow privacy policy before remote transmission.

---

## 35. Query Contracts

### 35.1 Get Recognition Result

```text
GetRecognitionResultRequest
├── contract_version: string
├── recognition_id: RecognitionId
└── trace_context: TraceContext
```

Response:

```text
GetRecognitionResultResponse
├── found: boolean
└── result?: RecognitionResult
```

A result may be unavailable when:

* it has expired;
* it was never persisted;
* the identifier is invalid;
* privacy policy prevents retrieval.

---

### 35.2 List Recognition Providers

```text
ListRecognitionProvidersRequest
├── contract_version: string
├── include_unavailable: boolean
└── trace_context: TraceContext
```

Response:

```text
ListRecognitionProvidersResponse
├── providers: RecognitionProviderSummary[]
└── generated_at: Timestamp
```

```text
RecognitionProviderSummary
├── provider_id: ProviderId
├── provider_name: string
├── provider_version: string
├── operational_status: ProviderOperationalStatus
├── execution_locations: ExecutionLocation[]
├── supported_modes: RecognitionMode[]
├── supported_languages: LanguageCode[]
├── local_processing_supported: boolean
└── remote_processing_supported: boolean
```

---

### 35.3 Get Provider Capabilities

```text
GetRecognitionProviderCapabilitiesRequest
├── contract_version: string
├── provider_id: ProviderId
└── trace_context: TraceContext
```

Response:

```text
GetRecognitionProviderCapabilitiesResponse
└── capabilities: RecognitionProviderCapabilities
```

---

### 35.4 Get Recognition Diagnostics

```text
GetRecognitionDiagnosticsRequest
├── contract_version: string
├── recognition_id: RecognitionId
├── requested_level: DiagnosticLevel
├── authorization_context?: string
└── trace_context: TraceContext
```

Response:

```text
GetRecognitionDiagnosticsResponse
├── recognition_id: RecognitionId
├── available_level: DiagnosticLevel
├── stages: RecognitionStageDiagnostic[]
├── provider_diagnostics?: Metadata
├── protected_artifacts: DiagnosticArtifactReference[]
└── generated_at: Timestamp
```

Diagnostics must obey privacy and retention policy.

---

## 36. Event Envelope

All Recognition events use a shared envelope.

```text
RecognitionEventEnvelope<T>
├── event_id: string
├── event_name: string
├── contract_version: string
├── occurred_at: Timestamp
├── producer: recognition
├── trace_context: TraceContext
└── payload: T
```

Events must be immutable.

Event names should use lowercase dot-separated notation.

---

## 37. Recognition Started Event

### 37.1 Event Name

```text
recognition.started
```

### 37.2 Payload

```text
RecognitionStartedEvent
├── request_id: RequestId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── recognition_mode: RecognitionMode
├── provider_id: ProviderId
├── priority: ProcessingPriority
└── started_at: Timestamp
```

No image reference is required in the public event.

---

## 38. Recognition Completed Event

### 38.1 Event Name

```text
recognition.completed
```

### 38.2 Payload

```text
RecognitionCompletedEvent
├── recognition_id: RecognitionId
├── request_id: RequestId
├── previous_recognition_id?: RecognitionId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── status: RecognitionStatus
├── provider_id: ProviderId
├── region_count: integer
├── warning_count: integer
├── total_duration_ms: integer
├── result_reference: RecognitionResultReference
└── completed_at: Timestamp
```

### 38.3 Result Reference

```text
RecognitionResultReference
├── reference_type: RecognitionResultReferenceType
└── reference_value: string
```

```text
RecognitionResultReferenceType
├── InMemoryResult
├── TemporaryResultStore
├── PersistentResultStore
└── InlinePermitted
```

`InlinePermitted` should only be used for trusted in-process communication.

Large recognition results should not be embedded in general Event Bus messages.

---

## 39. Recognition Failed Event

### 39.1 Event Name

```text
recognition.failed
```

### 39.2 Payload

```text
RecognitionFailedEvent
├── request_id: RequestId
├── recognition_id?: RecognitionId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── provider_id?: ProviderId
├── error: RecognitionError
└── failed_at: Timestamp
```

---

## 40. Recognition Cancelled Event

### 40.1 Event Name

```text
recognition.cancelled
```

### 40.2 Payload

```text
RecognitionCancelledEvent
├── request_id: RequestId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── reason?: CancellationReason
├── provider_interrupted: boolean
└── cancelled_at: Timestamp
```

A cancelled request must publish no later completed event.

---

## 41. Optional Progress Events

Optional events:

```text
recognition.preprocessing_completed
recognition.regions_detected
recognition.region_recognized
recognition.reading_order_resolved
```

These events are not guaranteed.

Consumers must not require them for correctness.

### 41.1 Progress Event Rules

1. Progress events are diagnostic or UX hints.
2. Missing progress events must not be interpreted as failure.
3. Partial text should not be persisted as a final result.
4. Progress events may be disabled for performance.
5. Raw recognized text should not be included unless the channel is explicitly trusted.
6. Event frequency must be bounded.

---

## 42. Idempotency Contract

Recognition command idempotency is based on `request_id`.

Rules:

1. A duplicate active `request_id` returns `DuplicateRequestId`.
2. A repeated completed `request_id` may return the existing outcome when available.
3. A repeated failed request may return the existing failure or reject the duplicate.
4. A retry must use a new `request_id`.
5. Consumers must not reuse request identifiers for different image content.
6. Idempotency does not imply cache equivalence.
7. Cache identity may use content checksum and configuration independently.

---

## 43. Ordering Contract

For asynchronous event consumers:

1. `recognition.started` should precede the terminal event.
2. Exactly one terminal event should be visible per accepted request.
3. Terminal events are:

```text
recognition.completed
recognition.failed
recognition.cancelled
```

4. Event delivery may be duplicated by the transport.
5. Consumers must deduplicate by `event_id`.
6. Consumers must correlate by `request_id`.
7. Consumers must not assume global ordering across requests.
8. A newer frame may complete before an older frame.
9. Session orchestration must reject stale results using frame identity.
10. Recognition guarantees traceability, not session relevance.

---

## 44. Consumer Obligations

Consumers of Recognition must:

1. provide a valid and resolvable image reference;
2. preserve the image until request termination;
3. provide correct source dimensions;
4. assign unique request identifiers;
5. use `request_id` for correlation;
6. use `frame_id` when frame freshness matters;
7. honor result immutability;
8. use explicit reading-order entries;
9. handle unknown confidence;
10. handle empty successful results;
11. handle warnings separately from failures;
12. not assume line geometry exists;
13. not assume all regions are rectangles forever;
14. not parse provider metadata for core behavior;
15. not expose raw text through unsafe logs;
16. cancel obsolete requests where practical;
17. reject stale results outside Recognition;
18. respect provider privacy classification;
19. not treat request language hints as verified language;
20. perform semantic cleanup outside Recognition.

---

## 45. Recognition Obligations

The Recognition module must:

1. validate every public request;
2. normalize every public output;
3. return source-relative geometry;
4. preserve raw recognized text;
5. explicitly represent uncertainty;
6. report provider identity;
7. report fallback use;
8. enforce local-only requests;
9. normalize errors;
10. distinguish warning from failure;
11. produce immutable results;
12. release image and provider resources;
13. support cancellation semantics;
14. suppress completion after accepted cancellation;
15. avoid direct Translation dependency;
16. avoid direct UI dependency;
17. avoid session-currentness decisions;
18. avoid permanent raw-image retention by default;
19. avoid provider object leakage;
20. maintain contract-version compatibility.

---

## 46. Validation Rules

### 46.1 Request Validation

A request is invalid when:

* `request_id` is missing;
* `source_id` is missing;
* `content_id` is missing;
* image dimensions are zero or negative;
* coordinate-space dimensions are invalid;
* requested region is outside the image;
* timeout is negative;
* fallback count is negative;
* exact provider mode has no provider ID;
* local-only and remote-required settings conflict;
* contract major version is unsupported.

---

### 46.2 Result Validation

Before publishing a result, Recognition must validate:

* all identifiers are present;
* every region ID is unique;
* every line ID is unique;
* every line references an existing region;
* every reading-order entry references an existing region;
* reading-order indexes are unique;
* source geometry is in bounds;
* normalized confidence is in range;
* completion time is not earlier than start time;
* region count matches metrics;
* provider identity is present;
* cancelled requests are not completed;
* no provider SDK object appears in metadata;
* no credentials appear in output.

---

## 47. Empty Result Contract

An image containing no readable text produces:

```text
RecognitionResult
├── status: Completed
├── regions: []
├── reading_order: []
└── warnings:
    └── NoReadableTextDetected
```

This is not equivalent to:

```text
RecognitionFailed
```

Failure means the module could not reliably perform the requested operation.

An empty result means the operation completed but found no readable text.

---

## 48. Partial Result Contract

When `allow_partial_result = true`, Recognition may return usable regions when part of the request fails.

```text
RecognitionResult
├── status: CompletedWithWarnings
├── regions: successfully recognized regions
└── warnings:
    └── PartialRecognitionResult
```

Rules:

1. Partial output must be explicitly marked.
2. Failed region identifiers should be included in warning metadata.
3. Provider or stage failure details should remain available in diagnostics.
4. A partially processed image must not be reported as fully successful.
5. Partial results must not hide a total provider failure.
6. Consumers decide whether partial output proceeds to Text Processing.

---

## 49. Timeout Contract

1. Timeout begins when execution is accepted unless otherwise documented.
2. Queue time may count toward the timeout depending on scheduler policy.
3. Timeout expiration requests cancellation.
4. A timed-out request must terminate with `ProviderTimeout`, `RequestExpired`, or `RequestCancelled`, based on the failing stage.
5. A provider response arriving after timeout must be ignored.
6. Timeout does not guarantee immediate resource termination.
7. The terminal outcome must remain singular.
8. Timeout values above the configured maximum must be rejected or clamped explicitly.

---

## 50. Privacy Contract

### 50.1 Local-Only Guarantee

When:

```text
local_processing_required = true
```

Recognition guarantees:

* no image data is sent to remote services;
* no OCR text is sent to remote services;
* remote providers are not selected;
* remote fallback is disabled;
* provider identity reports local execution.

### 50.2 Remote Processing Disclosure

When a remote provider is used, the result must expose:

```text
provider.execution_location = RemoteService
```

and normally include:

```text
RemoteProviderUsed
```

as an informational warning.

### 50.3 Logging Rules

Normal logs may include:

* request ID;
* provider ID;
* durations;
* region count;
* warning count;
* error code.

Normal logs must not include:

* source image;
* complete recognized text;
* provider token;
* API key;
* authorization headers;
* sensitive temporary paths;
* full remote response payloads.

---

## 51. Compatibility Rules

### 51.1 Backward-Compatible Changes

Allowed within the same major version:

* adding optional fields;
* adding new enum values when consumers use unknown-value handling;
* adding warning codes;
* adding capabilities;
* adding optional events;
* clarifying field descriptions;
* adding provider metadata keys.

### 51.2 Breaking Changes

Require a major version change:

* removing a field;
* renaming a field;
* changing field meaning;
* changing coordinate conventions;
* changing confidence range;
* making an optional field required;
* changing identifier semantics;
* changing terminal event guarantees;
* changing privacy guarantees;
* changing raw-text preservation semantics.

### 51.3 Unknown Enum Values

Consumers must:

* preserve unknown enum values when possible;
* fall back safely;
* not crash on unknown warning codes;
* treat unknown provider capabilities as unsupported;
* treat unknown confidence level as `Unknown`;
* reject unknown major contract versions.

---

## 52. Serialization Guidance

Recommended serialization formats:

```text
In-process:
- native typed objects

Cross-process:
- Protocol Buffers
- JSON
- MessagePack
```

JSON field naming recommendation:

```text
snake_case
```

Example:

```json
{
  "contract_version": "1.0.0",
  "request_id": "req_01",
  "source_id": "source_01",
  "content_id": "content_42",
  "frame_id": "frame_104"
}
```

Large image and result payloads should use references rather than inline serialization.

---

## 53. Example Recognize Image Request

```json
{
  "context": {
    "contract_version": "1.0.0",
    "request_id": "req_20260722_0001",
    "session_id": "session_01",
    "source_id": "desktop_region_01",
    "content_id": "page_104",
    "frame_id": "frame_104_08",
    "priority": "Interactive",
    "timeout_ms": 5000,
    "created_at": "2026-07-22T03:15:42.184Z",
    "requested_by": "ObservationModule",
    "trace_context": {
      "trace_id": "trace_01"
    }
  },
  "image": {
    "reference_type": "SharedBuffer",
    "reference_value": "buffer://frame_104_08",
    "media_type": "image/png",
    "width": 1600,
    "height": 900
  },
  "source_coordinate_space": {
    "space_id": "frame_104_08",
    "width": 1600,
    "height": 900,
    "origin": "TopLeft",
    "unit": "Pixel",
    "rotation_degrees": 0
  },
  "options": {
    "mode": "ComicPage",
    "language_hints": [
      "zh-Hans"
    ],
    "script_hints": [
      "Hans"
    ],
    "orientation_hint": "Mixed",
    "reading_direction_hint": "TopToBottom",
    "provider_policy": {
      "selection_mode": "Automatic",
      "required_capabilities": [
        "RegionDetection",
        "TextRecognition",
        "VerticalText"
      ],
      "excluded_provider_ids": [],
      "local_processing_required": true,
      "remote_processing_allowed": false,
      "fallback_allowed": true,
      "maximum_fallback_count": 1,
      "execution_device_preference": "Automatic"
    },
    "return_line_geometry": true,
    "return_provider_alternatives": false,
    "allow_partial_result": true,
    "diagnostic_level": "Basic"
  }
}
```

---

## 54. Example Recognition Result

```json
{
  "contract_version": "1.0.0",
  "recognition_id": "rec_20260722_0001",
  "request_id": "req_20260722_0001",
  "session_id": "session_01",
  "source_id": "desktop_region_01",
  "content_id": "page_104",
  "frame_id": "frame_104_08",
  "status": "CompletedWithWarnings",
  "provider": {
    "provider_id": "local_ocr_01",
    "provider_name": "Local OCR",
    "provider_version": "1.2.0",
    "adapter_version": "1.0.0",
    "execution_location": "LocalProcess",
    "execution_device": "GPU",
    "model_id": "chinese_comic_ocr",
    "model_version": "0.4",
    "fallback_index": 0
  },
  "source_coordinate_space": {
    "space_id": "frame_104_08",
    "width": 1600,
    "height": 900,
    "origin": "TopLeft",
    "unit": "Pixel",
    "rotation_degrees": 0
  },
  "processed_image": {
    "source_width": 1600,
    "source_height": 900,
    "processed_width": 2400,
    "processed_height": 1350,
    "preprocessing_profile_id": "comic_default",
    "operations": [
      {
        "operation": "Upscale",
        "changed_geometry": true
      },
      {
        "operation": "Contrast",
        "changed_geometry": false
      }
    ],
    "coordinate_transform": {
      "source_space_id": "frame_104_08",
      "processed_space_id": "ocr_processed_01",
      "crop_offset_x": 0,
      "crop_offset_y": 0,
      "scale_x": 1.5,
      "scale_y": 1.5,
      "rotation_degrees": 0,
      "padding_left": 0,
      "padding_top": 0,
      "padding_right": 0,
      "padding_bottom": 0
    }
  },
  "language_hypotheses": [
    {
      "language_code": "zh-Hans",
      "confidence": {
        "level": "High",
        "normalized_value": 0.94,
        "source": "Provider"
      },
      "source": "ProviderDetection",
      "region_ids": [
        "region_01",
        "region_02"
      ]
    }
  ],
  "script_hypotheses": [
    {
      "script_code": "Hans",
      "confidence": {
        "level": "High",
        "normalized_value": 0.97,
        "source": "Provider"
      },
      "source": "ProviderDetection",
      "region_ids": [
        "region_01",
        "region_02"
      ]
    }
  ],
  "regions": [
    {
      "region_id": "region_01",
      "geometry": {
        "geometry_type": "Rectangle",
        "x": 104,
        "y": 88,
        "width": 260,
        "height": 142
      },
      "raw_text": "你今天怎么来了？",
      "surface_text": "你今天怎么来了？",
      "lines": [
        {
          "line_id": "line_01",
          "region_id": "region_01",
          "geometry": {
            "geometry_type": "Rectangle",
            "x": 112,
            "y": 96,
            "width": 236,
            "height": 42
          },
          "raw_text": "你今天",
          "surface_text": "你今天",
          "confidence": {
            "level": "High",
            "normalized_value": 0.93,
            "raw_value": 93,
            "raw_scale": "ZeroToHundred",
            "source": "Provider"
          },
          "orientation": "Horizontal",
          "region_order_index": 0,
          "geometry_source": "ProviderDetected"
        }
      ],
      "detection_confidence": {
        "level": "High",
        "normalized_value": 0.95,
        "source": "Detector"
      },
      "recognition_confidence": {
        "level": "High",
        "normalized_value": 0.91,
        "source": "Recognizer"
      },
      "orientation": "Horizontal",
      "reading_direction": "LeftToRight",
      "region_type": "SpeechBubbleText",
      "geometry_source": "ProviderDetected",
      "alternatives": [],
      "warnings": []
    }
  ],
  "reading_order": [
    {
      "order_index": 0,
      "region_id": "region_01",
      "order_source": "CombinedHeuristic",
      "confidence": {
        "level": "Medium",
        "normalized_value": 0.76,
        "source": "Heuristic"
      },
      "manually_overridden": false,
      "rule_id": "comic_mixed_layout_v1"
    }
  ],
  "warnings": [
    {
      "warning_code": "ReadingOrderUncertain",
      "stage": "ReadingOrder",
      "severity": "Degraded",
      "message": "Reading order contains a low-separation region cluster."
    }
  ],
  "metrics": {
    "validation_duration_ms": 3,
    "provider_selection_duration_ms": 2,
    "image_resolution_duration_ms": 1,
    "normalization_duration_ms": 8,
    "preprocessing_duration_ms": 86,
    "detection_duration_ms": 174,
    "recognition_duration_ms": 388,
    "post_processing_duration_ms": 12,
    "reading_order_duration_ms": 5,
    "coordinate_mapping_duration_ms": 1,
    "result_assembly_duration_ms": 2,
    "total_duration_ms": 682,
    "source_pixel_count": 1440000,
    "processed_pixel_count": 3240000,
    "detected_region_count": 2,
    "recognized_region_count": 2,
    "recognized_line_count": 3,
    "recognized_character_count": 18,
    "retry_count": 0,
    "fallback_count": 0,
    "provider_id": "local_ocr_01",
    "execution_device": "GPU"
  },
  "started_at": "2026-07-22T03:15:42.190Z",
  "completed_at": "2026-07-22T03:15:42.872Z",
  "trace_context": {
    "trace_id": "trace_01"
  }
}
```

---

## 55. Example Empty Recognition Result

```json
{
  "contract_version": "1.0.0",
  "recognition_id": "rec_20260722_0002",
  "request_id": "req_20260722_0002",
  "source_id": "desktop_region_01",
  "content_id": "page_105",
  "status": "Completed",
  "provider": {
    "provider_id": "local_ocr_01",
    "provider_name": "Local OCR",
    "provider_version": "1.2.0",
    "adapter_version": "1.0.0",
    "execution_location": "LocalProcess",
    "execution_device": "GPU",
    "fallback_index": 0
  },
  "source_coordinate_space": {
    "space_id": "frame_105_01",
    "width": 1600,
    "height": 900,
    "origin": "TopLeft",
    "unit": "Pixel",
    "rotation_degrees": 0
  },
  "regions": [],
  "reading_order": [],
  "language_hypotheses": [],
  "script_hypotheses": [],
  "warnings": [
    {
      "warning_code": "NoReadableTextDetected",
      "stage": "TextRecognition",
      "severity": "Information",
      "message": "No readable text regions were detected."
    }
  ]
}
```

---

## 56. Example Recognition Failure

```json
{
  "contract_version": "1.0.0",
  "error_code": "ProviderTimeout",
  "stage": "TextRecognition",
  "message": "The selected recognition provider exceeded the request timeout.",
  "retryable": true,
  "request_id": "req_20260722_0003",
  "source_id": "desktop_region_01",
  "content_id": "page_106",
  "frame_id": "frame_106_02",
  "provider_id": "remote_ocr_01",
  "occurred_at": "2026-07-22T03:20:14.421Z",
  "trace_context": {
    "trace_id": "trace_03"
  }
}
```

---

## 57. Contract Test Requirements

Every Recognition implementation must pass contract tests covering:

### Request Contracts

* valid full-image request;
* valid region request;
* duplicate request ID;
* invalid image dimensions;
* expired image reference;
* invalid region bounds;
* invalid provider policy;
* unsupported contract major version.

### Result Contracts

* valid completed result;
* completed result with warnings;
* empty successful result;
* partial result;
* line geometry unavailable;
* unknown confidence;
* mixed orientation;
* mixed language;
* provider fallback;
* source-coordinate mapping.

### Error Contracts

* provider unavailable;
* provider timeout;
* invalid provider response;
* preprocessing failure;
* coordinate-mapping failure;
* cancellation;
* resource exhaustion.

### Event Contracts

* started then completed;
* started then failed;
* started then cancelled;
* no double terminal event;
* duplicate event delivery;
* stale frame completion;
* result-reference retrieval.

### Privacy Contracts

* local-only request never uses remote provider;
* credentials removed from errors;
* raw text excluded from normal logs;
* image bytes excluded from events;
* protected diagnostics require authorization.

---

## 58. Contract Invariants

The following invariants must always hold.

1. Every accepted request has one unique `request_id`.
2. Every completed result has one unique `recognition_id`.
3. Every region ID is unique within a result.
4. Every line ID is unique within a result.
5. Every line references an existing region.
6. Every reading-order entry references an existing region.
7. Public geometry is mapped to source coordinate space.
8. Raw OCR text is preserved.
9. Semantic text correction is outside the contract.
10. Missing confidence is represented as unknown.
11. Provider SDK types never cross the public boundary.
12. Provider credentials never appear in public output.
13. Local-only requests never transmit source data remotely.
14. Cancelled requests never publish successful completion.
15. Exactly one terminal outcome exists for each accepted request.
16. Empty text is not automatically a failure.
17. Warnings do not replace errors.
18. Failed operations do not produce successful result objects.
19. Results are immutable.
20. Retries produce new request and recognition identifiers.
21. Reading order is explicit.
22. Request hints are not treated as detected facts.
23. Provider fallback is traceable.
24. Image payloads are not embedded in public Event Bus messages.
25. Recognition output contains source text only, never translated text.

---

## 59. MVP Contract Subset

The MVP only needs to implement the following required subset.

### Required Commands

```text
RecognizeImage
RecognizeRegion
CancelRecognition
```

### Required Queries

```text
ListRecognitionProviders
GetRecognitionProviderCapabilities
```

### Required Input

```text
PNG, JPEG, or WebP image reference
source coordinate space
Simplified Chinese language hint
English language hint
automatic mode
comic-page mode
single-region mode
local-only provider policy
timeout
cancellation
```

### Required Output

```text
recognition identity
request identity
provider identity
rectangular regions
raw text
surface text
source geometry
region confidence where available
orientation
initial reading order
warnings
metrics
timestamps
```

### Required Terminal Events

```text
recognition.completed
recognition.failed
recognition.cancelled
```

### Optional for MVP

```text
line geometry
polygon geometry
recognition alternatives
language auto-detection
script auto-detection
partial progress events
remote providers
character-level geometry
structured-page mode
```

---

## 60. Deferred Contract Extensions

Potential future extensions:

* streaming OCR results;
* long-page chunk contracts;
* tiled-image recognition;
* character-level geometry;
* speech-bubble geometry;
* sound-effect recognition;
* handwritten text;
* document table recognition;
* formula recognition;
* cross-frame region tracking;
* incremental OCR updates;
* provider ensemble voting;
* model benchmark metadata;
* region semantic classification;
* result-difference contracts;
* manual reading-order correction contracts;
* OCR correction feedback contracts;
* provider learning feedback;
* encrypted remote-processing envelopes.

These must not be added until a concrete capability requires them.

---

## 61. Related Documents

```text
doc/02-modules/recognition/README.md
doc/02-modules/recognition/MODULE.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
docs/architecture/CAPABILITY_MAP.md
```

---

## 62. Summary

The Recognition contract defines how CRAI transforms an image reference into structured, traceable source text.

Its essential guarantees are:

* provider-independent input and output;
* immutable recognition results;
* explicit request and result identity;
* source-relative geometry;
* explicit reading order;
* explicit uncertainty;
* normalized warnings and errors;
* strict cancellation semantics;
* local-only privacy enforcement;
* safe asynchronous event integration.

The contract is intentionally broader than a simple OCR string interface.

Its primary output is a structured recognition result that downstream modules can process without knowing which detector, OCR engine, image library, model, or provider produced it.
