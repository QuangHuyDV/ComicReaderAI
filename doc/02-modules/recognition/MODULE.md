# Recognition Module Specification

> **Project:** CRAI
> **Module:** Recognition
> **Path:** `doc/02-modules/recognition/MODULE.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-22

---

## 1. Module Definition

The Recognition module is responsible for converting image-based input into structured, spatially aligned source text.

Its module boundary begins when a valid image-processing request is received.

Its boundary ends when CRAI receives:

* recognized text regions;
* source-relative geometry;
* recognition confidence;
* text orientation;
* script and language hints;
* initial reading order;
* processing metadata;
* normalized recognition errors.

Recognition is an image-to-structured-text module.

It is not a translation module, content-acquisition module, text-understanding module, presentation module, or session controller.

---

## 2. Module Identity

```text
Module ID: recognition
Module Type: Core Processing Module
Primary Domain: Image Text Recognition
Lifecycle: Request-scoped processing
State Ownership: Recognition results and provider execution state
MVP Priority: Required
```

The Recognition module is required for CRAI's image-reading flow.

It may be bypassed when structured source text is already available, such as through browser DOM extraction or direct text input.

---

## 3. Problem Statement

Image-based reading content does not expose reliable structured text.

Examples include:

* manhua pages;
* manga pages;
* manhwa pages;
* screenshots;
* canvas-based website readers;
* scanned documents;
* image-only PDFs;
* embedded page images.

CRAI must determine:

1. where text exists;
2. what characters are present;
3. how regions relate spatially;
4. how the regions should initially be ordered;
5. how certain the recognition result is;
6. how the result maps back to the source image.

The Recognition module provides this information without attempting to interpret or translate the content.

---

## 4. Responsibilities

The Recognition module owns the following responsibilities.

### 4.1 Request Validation

Validate:

* image availability;
* image dimensions;
* image format;
* crop bounds;
* recognition mode;
* language hints;
* provider requirements;
* timeout and cancellation context.

### 4.2 Image Preparation

Prepare images for provider execution through:

* image normalization;
* resizing;
* upscaling;
* grayscale conversion;
* contrast adjustment;
* denoising;
* sharpening;
* deskewing;
* thresholding;
* rotation correction;
* provider-specific preprocessing.

### 4.3 Text Region Detection

Identify likely text-containing areas.

Detection output includes:

* stable region identifiers;
* bounding geometry;
* detection confidence;
* likely orientation;
* optional script hints;
* optional region classification.

### 4.4 Character Recognition

Convert detected image regions into source-language characters.

Recognition output may include:

* region text;
* line text;
* word or character information;
* confidence values;
* provider alternatives;
* orientation metadata.

### 4.5 Reading Order Reconstruction

Create an initial spatial reading order.

The order may use:

* top-to-bottom rules;
* left-to-right rules;
* right-to-left rules;
* vertical-column rules;
* provider output;
* mixed-layout heuristics.

### 4.6 Result Normalization

Convert provider-specific results into CRAI domain models.

### 4.7 Provider Execution

Select and invoke an OCR provider through stable interfaces.

### 4.8 Cancellation

Stop or invalidate obsolete work when requested.

### 4.9 Diagnostics

Expose stage timing, warnings, provider information, and recognition-quality metadata.

---

## 5. Non-Responsibilities

Recognition must not own:

* screen capture;
* file discovery;
* browser-page extraction;
* frame-change detection;
* stable-frame detection;
* scroll detection;
* content-session lifecycle;
* translation-job orchestration;
* linguistic normalization;
* OCR semantic correction;
* sentence segmentation;
* dialogue grouping;
* name detection;
* terminology detection;
* glossary application;
* translation;
* translated-text layout;
* overlay placement;
* user-history storage;
* cache-retention policy;
* permanent raw-image storage.

Recognition must not directly invoke Translation or Presentation.

---

## 6. Module Boundary

```text
                    ┌──────────────────────┐
                    │   Source / Capture   │
                    └──────────┬───────────┘
                               │ ImageContent
                               ▼
                    ┌──────────────────────┐
                    │    Observation       │
                    │ optional before OCR  │
                    └──────────┬───────────┘
                               │ StableImage
                               ▼
┌────────────────────────────────────────────────────────────┐
│                    Recognition Module                      │
│                                                            │
│  Validate → Preprocess → Detect → Recognize → Order        │
│                         → Normalize                         │
└─────────────────────────────┬──────────────────────────────┘
                              │ RecognitionResult
                              ▼
                    ┌──────────────────────┐
                    │   Text Processing    │
                    └──────────────────────┘
```

Recognition accepts image-based content only.

Structured text must use a different extraction path.

---

## 7. Public Commands

The module exposes command-oriented operations.

### 7.1 Recognize Image

```text
RecognizeImage
```

Processes a complete image.

Use cases:

* complete comic page;
* screenshot;
* scanned page;
* imported image;
* stable captured frame.

### 7.2 Recognize Region

```text
RecognizeImageRegion
```

Processes a selected subregion.

Use cases:

* manual OCR selection;
* retrying one failed region;
* user-selected speech bubble;
* low-confidence region reprocessing.

### 7.3 Retry Recognition

```text
RetryRecognition
```

Reprocesses a previous recognition request with changed settings.

Possible changes:

* provider;
* preprocessing profile;
* language hint;
* orientation hint;
* recognition mode.

### 7.4 Cancel Recognition

```text
CancelRecognition
```

Requests cancellation of an active recognition operation.

---

## 8. Public Queries

### 8.1 Get Recognition Result

```text
GetRecognitionResult
```

Returns a completed immutable result.

### 8.2 Get Provider Capabilities

```text
GetRecognitionProviderCapabilities
```

Returns supported:

* languages;
* scripts;
* orientations;
* image sizes;
* local or remote modes;
* cancellation behavior;
* confidence support;
* hardware requirements.

### 8.3 List Providers

```text
ListRecognitionProviders
```

Returns configured and available recognition implementations.

### 8.4 Get Diagnostics

```text
GetRecognitionDiagnostics
```

Returns processing details for debugging or evaluation.

Diagnostics access may be limited by privacy configuration.

---

## 9. Public Interface

Conceptual module interface:

```text
RecognitionService
├── recognize_image(request) -> RecognitionResult
├── recognize_region(request) -> RecognitionResult
├── retry(request) -> RecognitionResult
├── cancel(request_id) -> CancellationResult
├── get_result(recognition_id) -> RecognitionResult?
├── list_providers() -> RecognitionProviderSummary[]
├── get_provider_capabilities(provider_id)
└── get_diagnostics(recognition_id)
```

The exact programming language interface will be defined during implementation design.

---

## 10. Command Contract

### 10.1 RecognizeImage Request

```text
RecognizeImageRequest
├── request_id: RequestId
├── session_id?: ReadingSessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── image: ImageReference
├── coordinate_space: CoordinateSpace
├── recognition_mode: RecognitionMode
├── language_hints: LanguageCode[]
├── script_hints: ScriptCode[]
├── orientation_hint?: TextOrientation
├── reading_direction_hint?: ReadingDirection
├── provider_policy?: RecognitionProviderPolicy
├── preprocessing_profile?: PreprocessingProfileId
├── timeout?: Duration
├── priority?: ProcessingPriority
├── trace_context: TraceContext
└── cancellation_token: CancellationToken
```

### 10.2 RecognizeRegion Request

```text
RecognizeRegionRequest
├── request_id: RequestId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── image: ImageReference
├── source_coordinate_space: CoordinateSpace
├── region: Geometry
├── recognition_mode: RecognitionMode
├── language_hints: LanguageCode[]
├── orientation_hint?: TextOrientation
├── provider_policy?: RecognitionProviderPolicy
├── preprocessing_profile?: PreprocessingProfileId
├── timeout?: Duration
├── trace_context: TraceContext
└── cancellation_token: CancellationToken
```

---

## 11. Result Contract

```text
RecognitionResult
├── recognition_id: RecognitionId
├── request_id: RequestId
├── session_id?: ReadingSessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── provider: RecognitionProviderIdentity
├── source_coordinate_space: CoordinateSpace
├── processed_image_metadata: ProcessedImageMetadata
├── detected_languages: LanguageHypothesis[]
├── detected_scripts: ScriptHypothesis[]
├── regions: RecognizedRegion[]
├── reading_order: ReadingOrderEntry[]
├── warnings: RecognitionWarning[]
├── metrics: RecognitionMetrics
├── status: RecognitionStatus
├── trace_context: TraceContext
├── started_at: Timestamp
└── completed_at: Timestamp
```

A completed result is immutable.

---

## 12. Recognized Region Model

```text
RecognizedRegion
├── region_id: RegionId
├── geometry: Geometry
├── source_geometry: Geometry
├── raw_text: string
├── normalized_surface_text: string
├── lines: RecognizedLine[]
├── detection_confidence?: Confidence
├── recognition_confidence?: Confidence
├── orientation: TextOrientation
├── reading_direction?: ReadingDirection
├── language_hint?: LanguageCode
├── script_hint?: ScriptCode
├── region_type?: RecognitionRegionType
├── provider_alternatives: RecognitionAlternative[]
├── warnings: RecognitionWarning[]
└── provider_metadata?: ProviderMetadata
```

`normalized_surface_text` may perform non-semantic cleanup only.

Allowed cleanup:

* trimming outer whitespace;
* normalizing provider line separators;
* removing provider control characters;
* converting invalid Unicode replacement artifacts where deterministic.

Not allowed:

* correcting Chinese characters based on sentence meaning;
* replacing names;
* joining sentences;
* translating punctuation semantically;
* guessing omitted words.

---

## 13. Recognized Line Model

```text
RecognizedLine
├── line_id: LineId
├── region_id: RegionId
├── geometry: Geometry
├── raw_text: string
├── confidence?: Confidence
├── orientation: TextOrientation
├── order_index: integer
└── provider_metadata?: ProviderMetadata
```

Line output is optional when a provider only returns region-level text.

The module must not fabricate line geometry without marking it as inferred.

---

## 14. Recognition Status

```text
RecognitionStatus
├── Pending
├── Running
├── Completed
├── CompletedWithWarnings
├── Failed
└── Cancelled
```

A result with no detected text may still be a successful result.

Example:

```text
status = Completed
regions = []
warnings = [NoReadableTextDetected]
```

No-text detection is not automatically a module failure.

---

## 15. Recognition Modes

```text
RecognitionMode
├── Automatic
├── ComicPage
├── Screenshot
├── SingleRegion
└── StructuredPage
```

### Automatic

The module selects a strategy from available hints and provider capability.

### ComicPage

Prioritizes:

* multiple regions;
* irregular layout;
* vertical Chinese;
* text over artwork;
* speech-bubble-like structures.

### Screenshot

Prioritizes:

* horizontal interface text;
* browser text;
* application labels;
* mixed structured regions.

### SingleRegion

Skips page-level detection where appropriate.

### StructuredPage

Prioritizes:

* regular lines;
* page columns;
* prose-oriented layouts.

`StructuredPage` may be deferred from the first implementation.

---

## 16. Provider Policy

```text
RecognitionProviderPolicy
├── ExactProvider
├── PreferredProvider
├── Automatic
├── LocalOnly
└── RemoteAllowed
```

Provider policy may contain:

```text
ProviderSelectionRequest
├── policy
├── preferred_provider_id?
├── required_capabilities[]
├── forbidden_capabilities[]
├── local_only
├── remote_allowed
├── max_expected_latency?
└── hardware_preference?
```

A local-only request must never use a remote provider.

A remote provider must not be used merely because a local provider failed unless fallback is explicitly allowed.

---

## 17. Provider Interface

```text
RecognitionProvider
├── provider_id()
├── provider_version()
├── capabilities()
├── validate_configuration()
├── initialize()
├── recognize(request)
├── cancel(provider_request_id)
├── health_check()
└── shutdown()
```

Provider adapters must normalize:

* requests;
* coordinates;
* confidence;
* errors;
* text orientation;
* language identifiers;
* result metadata.

Provider-specific SDK types must remain inside provider adapters.

---

## 18. Provider Capability Model

```text
RecognitionProviderCapabilities
├── provider_id
├── supported_languages[]
├── supported_scripts[]
├── supported_orientations[]
├── supported_modes[]
├── supports_region_detection
├── supports_text_recognition
├── supports_combined_ocr
├── supports_confidence
├── supports_line_geometry
├── supports_character_geometry
├── supports_partial_results
├── supports_cancellation
├── supports_batching
├── supports_gpu
├── supports_cpu
├── supports_local_processing
├── supports_remote_processing
├── maximum_width?
├── maximum_height?
├── maximum_pixels?
├── recommended_concurrency
└── initialization_cost
```

Capabilities must describe observed or documented provider behavior.

They must not claim functionality that has not been validated.

---

## 19. Internal Components

Recommended internal components:

```text
recognition/
├── RecognitionApplicationService
├── RecognitionRequestValidator
├── RecognitionProviderSelector
├── RecognitionPipeline
├── ImageNormalizer
├── ImagePreprocessor
├── TextRegionDetector
├── TextRecognizer
├── RegionPostProcessor
├── ReadingOrderResolver
├── RecognitionResultAssembler
├── RecognitionCancellationRegistry
├── RecognitionDiagnosticsCollector
└── ProviderAdapters
```

These are logical components, not mandatory source-code files.

Implementation may combine simple components until complexity justifies separation.

---

## 20. Internal Processing Flow

```text
Receive Recognition Request
    ↓
Validate Request
    ↓
Resolve Provider Policy
    ↓
Select Provider
    ↓
Normalize Image
    ↓
Build Preprocessing Plan
    ↓
Apply Preprocessing
    ↓
Detect Text Regions
    ↓
Recognize Text
    ↓
Normalize Provider Output
    ↓
Suppress Invalid or Duplicate Regions
    ↓
Resolve Initial Reading Order
    ↓
Map Coordinates to Source Space
    ↓
Collect Metrics and Warnings
    ↓
Create Immutable Recognition Result
    ↓
Publish Completion Event
```

Every stage must check cancellation where practical.

---

## 21. Detection and Recognition Strategies

The module supports two execution strategies.

### 21.1 Combined OCR

```text
Image
    ↓
Combined OCR Provider
    ↓
Detected and Recognized Regions
```

Suitable when one provider provides:

* sufficient region detection;
* sufficient recognition accuracy;
* correct coordinates;
* acceptable reading order;
* acceptable latency.

### 21.2 Composed OCR

```text
Image
    ↓
Detection Provider
    ↓
Regions
    ↓
Recognition Provider
    ↓
Recognized Regions
```

Suitable when:

* comic text detection needs specialization;
* vertical recognition requires another provider;
* provider quality differs by stage;
* detection and recognition require separate benchmarking.

The first MVP should prefer the simpler strategy that meets quality requirements.

---

## 22. Preprocessing Plan

```text
PreprocessingPlan
├── profile_id
├── operations[]
├── source_dimensions
├── target_dimensions
├── coordinate_transform
├── provider_specific
└── configuration_version
```

Possible operations:

```text
Resize
Upscale
Grayscale
Contrast
Brightness
Threshold
Denoise
Sharpen
Deskew
Rotate
Invert
Pad
Crop
ColorChannelSelection
```

Each geometry-changing operation must contribute to a reversible coordinate transform.

---

## 23. Coordinate Transformation

Recognition may process an image that differs from the source.

Example:

```text
Source Frame
    ↓ Crop
    ↓ Resize
    ↓ Rotate
Processed OCR Image
```

The module must retain the complete transform chain.

```text
CoordinateTransform
├── source_space
├── target_space
├── crop_offset
├── scale_x
├── scale_y
├── rotation
├── padding
└── inverse_transform
```

Public region coordinates must be returned in source coordinate space.

Processed-space coordinates may be retained in diagnostics.

---

## 24. Geometry Model

The initial geometry model should support:

```text
Rectangle
├── x
├── y
├── width
└── height
```

The architecture should allow later support for:

```text
Polygon
├── points[]
└── bounding_rectangle
```

Consumers must not assume every future region will be a perfect axis-aligned rectangle.

For the MVP, rectangular geometry is sufficient unless provider evaluation proves otherwise.

---

## 25. Reading Direction Model

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

Reading direction may be defined:

* at request level;
* at page level;
* at region level.

Region-level orientation takes precedence when mixed layouts exist.

---

## 26. Reading Order Resolution

```text
ReadingOrderResolver
├── use_provider_order()
├── use_spatial_rules()
├── use_orientation_rules()
├── resolve_conflicts()
└── assign_order_entries()
```

The resolver must preserve:

* provider order;
* computed order;
* rule used;
* order confidence;
* manual override state.

Example:

```text
ReadingOrderEntry
├── order_index: 0
├── region_id: region-01
├── source: SpatialHeuristic
├── confidence: Medium
└── manually_overridden: false
```

Array order alone is not sufficient as the source of truth.

---

## 27. Confidence

```text
Confidence
├── raw_value?
├── normalized_value?
├── level
├── source
└── normalization_method?
```

Confidence level:

```text
Unknown
Low
Medium
High
```

Rules:

1. Missing provider confidence becomes `Unknown`.
2. Provider values must not be directly compared without normalization.
3. Normalization logic must be provider-specific and documented.
4. Low confidence must not automatically discard text.
5. Confidence may generate warnings or trigger optional retry policy.
6. Automatic retry must be bounded.

---

## 28. Warnings

Recognition warnings describe degraded but usable results.

```text
RecognitionWarning
├── code
├── stage
├── message
├── region_id?
├── provider_id?
└── metadata?
```

Candidate warning codes:

```text
NoReadableTextDetected
LowDetectionConfidence
LowRecognitionConfidence
UnsupportedOrientationFallback
UnsupportedLanguageFallback
ReadingOrderUncertain
OverlappingRegionsSuppressed
RegionGeometryInferred
LineGeometryUnavailable
ProviderConfidenceUnavailable
ImageUpscaled
ImageDownscaled
RemoteProviderUsed
PartialResult
PreprocessingFallbackUsed
```

Warnings must not be represented as fatal errors.

---

## 29. Error Model

```text
RecognitionError
├── code
├── stage
├── message
├── retryable
├── request_id
├── recognition_id?
├── provider_id?
├── provider_error_code?
├── cause?
├── diagnostics_reference?
└── occurred_at
```

### Error Codes

```text
InvalidRequest
InvalidImage
UnsupportedImageFormat
UnsupportedLanguage
UnsupportedScript
UnsupportedOrientation
ImageTooLarge
ProviderUnavailable
ProviderNotConfigured
ProviderInitializationFailed
ProviderAuthenticationFailed
ProviderRateLimited
ProviderTimeout
ProviderRejectedContent
PreprocessingFailed
DetectionFailed
RecognitionFailed
CoordinateMappingFailed
ReadingOrderFailed
Cancelled
OutOfMemory
HardwareUnavailable
InternalError
```

### Retry Rules

Retryable examples:

* transient provider failure;
* timeout;
* rate limiting;
* temporary hardware failure.

Non-retryable examples:

* invalid image;
* unsupported format;
* invalid crop;
* missing credentials;
* user cancellation.

Retry policy belongs to orchestration or provider policy, not hidden provider behavior.

---

## 30. Cancellation Model

```text
RecognitionCancellationRegistry
├── register(request_id)
├── cancel(request_id)
├── is_cancelled(request_id)
└── unregister(request_id)
```

Cancellation must be checked:

* before provider selection;
* before preprocessing;
* after preprocessing;
* before provider execution;
* between regions where possible;
* before result assembly;
* before completion publication.

When provider execution cannot be interrupted:

1. mark the request cancelled;
2. ignore the returned provider result;
3. release resources;
4. publish `recognition.cancelled`;
5. do not publish `recognition.completed`.

---

## 31. Stale Result Handling

Recognition preserves identifiers needed for stale-result rejection.

```text
request_id
session_id
source_id
content_id
frame_id
recognition_id
```

Recognition does not determine whether a frame is still current.

The Session or Orchestration module performs that check.

Recognition guarantees that every result is traceable to the exact request and frame that created it.

---

## 32. Events Produced

Minimum events:

```text
RecognitionStarted
RecognitionCompleted
RecognitionFailed
RecognitionCancelled
```

Optional diagnostic events:

```text
RecognitionPreprocessingCompleted
RecognitionRegionsDetected
RecognitionRegionRecognized
RecognitionReadingOrderResolved
```

### RecognitionStarted

```text
RecognitionStarted
├── request_id
├── source_id
├── content_id
├── frame_id?
├── provider_id
├── recognition_mode
├── trace_context
└── occurred_at
```

### RecognitionCompleted

```text
RecognitionCompleted
├── recognition_id
├── request_id
├── source_id
├── content_id
├── frame_id?
├── region_count
├── warning_count
├── duration
├── result_reference
├── trace_context
└── occurred_at
```

### RecognitionFailed

```text
RecognitionFailed
├── request_id
├── source_id
├── content_id
├── frame_id?
├── error
├── trace_context
└── occurred_at
```

### RecognitionCancelled

```text
RecognitionCancelled
├── request_id
├── source_id
├── content_id
├── frame_id?
├── cancellation_reason?
├── trace_context
└── occurred_at
```

Image payloads must not be included in events.

---

## 33. Events Consumed

Recognition may consume:

```text
StableFrameReady
ImageImported
RecognitionRequested
RecognitionCancellationRequested
SessionStopped
SourceClosed
ApplicationShutdownRequested
ProviderConfigurationChanged
```

Recognition should normally receive direct commands from orchestration.

Event consumption is appropriate for asynchronous processing but must not create hidden control flow.

---

## 34. Dependencies

### 34.1 Allowed Dependencies

Recognition may depend on:

```text
shared-kernel
configuration
provider-contracts
image-primitives
geometry-primitives
event-bus
diagnostics
tracing
cancellation
scheduler-contracts
security-contracts
```

### 34.2 Forbidden Direct Dependencies

Recognition must not directly depend on:

```text
translation
presentation
desktop-ui
browser-extension
capture implementation
observation implementation
session storage implementation
glossary implementation
reading-history implementation
export implementation
```

Recognition may exchange contracts with orchestration and session modules without importing their internal implementations.

---

## 35. Data Ownership

Recognition owns:

* recognition-result domain models;
* normalized provider output;
* raw recognized source text;
* region geometry;
* line geometry where available;
* initial reading order;
* recognition warnings;
* processing metrics;
* provider-execution metadata;
* recognition diagnostics.

Recognition does not own:

* original source-image lifecycle;
* long-term image persistence;
* translation results;
* corrected semantic text;
* user glossary;
* session state;
* reading history;
* cache eviction;
* UI layout state.

---

## 36. Result Persistence

Recognition results may be stored temporarily or cached.

The module may produce:

```text
RecognitionCacheIdentity
├── image_fingerprint
├── crop
├── provider_id
├── provider_version
├── recognition_mode
├── preprocessing_profile
├── language_hints
├── orientation_hint
└── configuration_version
```

Recognition does not decide:

* storage duration;
* disk location;
* cache size;
* encryption policy;
* retention period;
* history visibility.

These belong to Storage and Privacy policies.

---

## 37. Immutability

A completed `RecognitionResult` is immutable.

User corrections must create a separate object.

```text
SourceTextCorrection
├── correction_id
├── recognition_id
├── region_id
├── original_text
├── corrected_text
├── correction_source
├── created_at
└── trace_context
```

Recognition may expose the original result for comparison.

It must never overwrite raw OCR text with user-corrected text.

---

## 38. Privacy Rules

1. Raw image bytes must not appear in normal logs.
2. Full recognized text must not appear in normal production logs.
3. Provider credentials must never appear in errors or diagnostics.
4. Remote provider use must be explicit and traceable.
5. Local-only requests must remain local.
6. Temporary image files must be deleted after use.
7. Image events must use references, not embedded payloads.
8. Diagnostic captures require explicit diagnostic policy.
9. Recognition results must follow session privacy configuration.
10. Copyrighted source content must not be retained permanently by default.

---

## 39. Performance Model

```text
RecognitionMetrics
├── validation_duration
├── normalization_duration
├── preprocessing_duration
├── detection_duration
├── recognition_duration
├── ordering_duration
├── result_assembly_duration
├── total_duration
├── time_to_first_region?
├── source_width
├── source_height
├── processed_width
├── processed_height
├── detected_region_count
├── recognized_region_count
├── recognized_character_count
├── provider_id
├── execution_device
├── peak_memory_estimate?
└── cancelled
```

Metrics must avoid exposing source text.

---

## 40. Concurrency

Recognition operations may run concurrently only under scheduler limits.

Rules:

1. UI threads must never execute OCR directly.
2. Provider concurrency must respect provider recommendations.
3. GPU-heavy providers may require single-operation execution.
4. Obsolete requests should be cancelled before starting new expensive work.
5. Visible content receives higher priority than preload work.
6. Region-level parallelism must be bounded.
7. Provider initialization must not occur for every request.
8. Shared models must use safe concurrency controls.
9. Memory use must be considered before parallel image copies.
10. Shutdown waits only for bounded cleanup, not indefinite provider completion.

---

## 41. Resource Lifecycle

### Provider Lifecycle

```text
Uninitialized
    ↓
Initializing
    ↓
Ready
    ↓
Degraded
    ↓
Unavailable
    ↓
ShuttingDown
    ↓
Stopped
```

### Request Lifecycle

```text
Pending
    ↓
Validating
    ↓
Preparing
    ↓
Detecting
    ↓
Recognizing
    ↓
Ordering
    ↓
Completing
    ↓
Completed
```

Alternative terminal states:

```text
Failed
Cancelled
```

Each request must release:

* image buffers;
* temporary files;
* provider request handles;
* cancellation registry entries;
* diagnostic buffers;
* GPU resources where applicable.

---

## 42. Module State

Recognition should minimize persistent mutable state.

Allowed long-lived state:

* provider registry;
* loaded provider models;
* provider health state;
* provider capability cache;
* bounded active-request registry;
* static preprocessing profiles;
* configuration snapshot.

Request-specific state must be isolated by `request_id`.

Recognition must not retain completed image buffers after the result is assembled.

---

## 43. Configuration

Candidate configuration:

```text
recognition:
  default_provider: local-ocr
  default_mode: automatic
  remote_provider_fallback: false
  local_only_default: true

  timeout:
    default_ms: 5000
    maximum_ms: 30000

  image:
    maximum_width: 12000
    maximum_height: 12000
    maximum_pixels: 60000000

  concurrency:
    maximum_requests: 2
    maximum_regions_per_request: 4

  confidence:
    low_threshold: provider-specific

  preprocessing:
    default_profile: comic-default

  diagnostics:
    enabled: false
    retain_processed_images: false
```

Exact values must be determined through prototype measurements.

---

## 44. Provider Health

Provider health may use:

```text
ProviderHealth
├── Ready
├── Degraded
├── Unavailable
├── Misconfigured
└── Initializing
```

Health checks may validate:

* model availability;
* runtime library availability;
* API credentials;
* GPU availability;
* network access;
* provider quota;
* initialization state.

A provider being available does not mean it meets quality requirements.

Quality validation is separate from operational health.

---

## 45. Observability

Required observability fields:

```text
trace_id
request_id
recognition_id
session_id?
source_id
content_id
frame_id?
provider_id
recognition_mode
status
duration
region_count
warning_count
error_code?
```

Logs must describe processing without revealing raw content.

Example safe log:

```text
Recognition completed:
request_id=req-42
provider=local-ocr
regions=12
duration_ms=840
warnings=2
```

Unsafe log:

```text
OCR result: 他今天去了……
```

Raw recognized text belongs in protected diagnostics or result storage, not ordinary logs.

---

## 46. Invariants

1. Recognition accepts only image-based input.
2. Recognition never mutates the supplied source image.
3. Every request has a unique `request_id`.
4. Every completed result has a unique `recognition_id`.
5. Every region identifier is stable inside one result.
6. Every public region maps to source coordinate space.
7. Every result references its source content.
8. Reading order is represented explicitly.
9. Raw recognized text is preserved.
10. Provider SDK objects never cross the adapter boundary.
11. Missing confidence remains unknown.
12. Cancelled results are never published as completed.
13. Recognition never calls Translation directly.
14. Recognition never renders UI.
15. Recognition does not own session-currentness decisions.
16. Remote OCR cannot bypass privacy policy.
17. User corrections never overwrite raw recognition results.
18. Recognition errors are normalized.
19. Image payloads are not published through the Event Bus.
20. Recognition remains usable outside an active reading session.

---

## 47. Testing Requirements

### Unit Tests

Required:

* validation;
* coordinate transforms;
* reading-order rules;
* confidence normalization;
* error mapping;
* warning generation;
* duplicate-region suppression;
* cancellation registry;
* result immutability;
* provider selection.

### Contract Tests

Every provider must pass:

* valid request;
* invalid image;
* unsupported language;
* empty text result;
* timeout;
* cancellation;
* provider unavailable;
* coordinate normalization;
* confidence behavior;
* error normalization.

### Integration Tests

Required flows:

```text
Imported Image → Recognition → Text Processing
```

```text
Stable Frame → Recognition → Result
```

```text
Frame A Recognition → Frame B Arrives → Frame A Cancelled
```

```text
Low Confidence Region → Manual Retry with New Provider
```

### Regression Tests

Compare:

* detected text regions;
* recognized source text;
* reading order;
* confidence;
* warnings;
* latency;
* memory use.

---

## 48. MVP Implementation Contract

The first usable Recognition implementation must support:

```text
Input:
- complete image
- optional crop
- Simplified Chinese language hint
- English language hint
- automatic or comic-page mode

Output:
- rectangular text regions
- recognized text
- source coordinates
- initial reading order
- confidence when available
- normalized warnings
- timing metrics

Control:
- provider selection
- cancellation
- timeout
- structured errors
```

It does not need:

* handwritten OCR;
* sound-effect understanding;
* character-level geometry;
* polygonal regions;
* automatic provider marketplace;
* semantic OCR correction;
* inpainting;
* translated-text insertion.

---

## 49. Acceptance Criteria

The module architecture is acceptable when:

1. OCR providers can be replaced without changing downstream modules.
2. Recognition output is independent from provider SDK models.
3. Every result maps correctly to source-image coordinates.
4. Cancellation prevents stale completion.
5. Text Processing can consume Recognition output without image-provider knowledge.
6. Translation is not referenced by Recognition implementation.
7. Local-only and remote-provider policies are enforceable.
8. Raw OCR output remains available for diagnostics and correction.
9. Reading order can be corrected later.
10. The module can process both session frames and imported images.
11. Errors and warnings are normalized.
12. Provider quality can be benchmarked independently.

---

## 50. Open Architecture Decisions

The following remain unresolved:

* first OCR provider;
* local versus remote default;
* combined versus composed OCR;
* required vertical-Chinese accuracy;
* default preprocessing profile;
* provider-specific preprocessing ownership;
* whether speech-bubble detection belongs here;
* whether script detection occurs before provider selection;
* whether region alternatives are persisted;
* whether partial recognition results are exposed;
* exact timeout values;
* exact concurrency limits;
* confidence normalization rules;
* default reading-order algorithm;
* character-level geometry need;
* long-page splitting strategy;
* GPU model-loading policy.

These decisions must be based on prototype results, not assumptions.

---

## 51. Related Module Contracts

```text
doc/02-modules/recognition/README.md
doc/02-modules/source/MODULE.md
doc/02-modules/observation/MODULE.md
doc/02-modules/text-processing/MODULE.md
doc/02-modules/providers/MODULE.md
doc/02-modules/session/MODULE.md
doc/02-modules/storage/MODULE.md
doc/02-modules/diagnostics/MODULE.md
```

Recognition depends on stable shared contracts but must not depend on the internal implementation of neighboring modules.

---

## 52. Summary

The Recognition module transforms images into immutable structured source-text results.

Its contract guarantees:

* provider independence;
* source-coordinate alignment;
* explicit reading order;
* normalized confidence;
* cancellable processing;
* traceable results;
* safe provider execution;
* separation from translation and presentation.

The module's primary output is not merely an OCR string.

Its primary output is a spatially aligned, traceable, structured representation of text detected in an image.
