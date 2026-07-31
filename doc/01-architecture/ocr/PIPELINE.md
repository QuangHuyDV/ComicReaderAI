# OCR Pipeline

- **Document:** OCR Architecture / Pipeline
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture
- **Last Updated:** 2026-07-28

---

# Purpose

This document defines the end-to-end OCR processing pipeline used by CRAI to transform an image into structured, traceable and reviewable recognized text.

The OCR Pipeline covers image preparation, text-region discovery, recognition, geometric reconstruction, normalization, validation and result publication.

It does not perform translation, translated-text rendering, source acquisition or persistent binary storage.

---

# Scope

The pipeline supports image-based reading sources such as:

- Manga, manhua and manhwa pages
- Browser images
- Screen captures
- Scanned pages
- Imported image files
- Rasterized document pages
- Cropped user-selected regions

The architecture must support Simplified Chinese, Traditional Chinese and English as initial source languages while remaining language-neutral at the contract level.

---

# Pipeline Position

```text
Capture / Import
      │
      ▼
Source Image
      │
      ▼
OCR Pipeline
      │
      ├──► Structured OCR Result
      ├──► Text Regions
      ├──► Reading Order
      ├──► Confidence and Diagnostics
      └──► Derived OCR Images
                │
                ▼
       Translation Pipeline
                │
                ▼
          Presentation
```

OCR ends when CRAI has a validated structured representation of the source text and its relationship to the source image.

---

# Primary Design Goals

The OCR Pipeline should provide:

- Accurate recognition of text in complex visual layouts
- Stable mapping between recognized text and image coordinates
- Support for horizontal and vertical writing
- Support for mixed languages and scripts
- Non-destructive processing of source images
- Provider-independent execution
- Partial retry without restarting the entire page
- Deterministic caching and invalidation
- Observable quality and performance
- Graceful degradation when confidence is low
- Cancellation at every expensive stage

---

# Non-Goals

The OCR Pipeline is not responsible for:

- Translating recognized text
- Rewriting dialogue
- Choosing translated terminology
- Painting over source text
- Rendering translated text into speech bubbles
- Downloading or capturing source content
- Persisting raw image bytes directly
- Managing the full Reading Session lifecycle
- Owning global provider credentials
- Guaranteeing semantic correctness beyond recognition

Those responsibilities belong to Translation, Presentation, Capture, Storage, Reading Session, Preferences and Provider Management.

---

# Canonical Pipeline

```text
OCR Request
    │
    ▼
1. Request Validation
    │
    ▼
2. Input Resolution
    │
    ▼
3. Image Normalization
    │
    ▼
4. OCR Profile Resolution
    │
    ▼
5. Cache Lookup
    │
    ├── Cache Hit ──────────────────────────────┐
    │                                           │
    ▼                                           │
6. Preprocessing                               │
    │                                           │
    ▼                                           │
7. Text Region Detection                       │
    │                                           │
    ▼                                           │
8. Region Preparation                          │
    │                                           │
    ▼                                           │
9. Text Recognition                            │
    │                                           │
    ▼                                           │
10. Geometry Reconstruction                    │
    │                                           │
    ▼                                           │
11. Reading Order Resolution                   │
    │                                           │
    ▼                                           │
12. Text Normalization                         │
    │                                           │
    ▼                                           │
13. Confidence Evaluation                      │
    │                                           │
    ▼                                           │
14. Quality Validation                         │
    │                                           │
    ├── Retry / Fallback                        │
    │                                           │
    ▼                                           │
15. Result Assembly ◄──────────────────────────┘
    │
    ▼
16. Cache Write
    │
    ▼
17. Result Publication
```

Stages may be skipped, combined or executed in parallel when the selected OCR strategy explicitly supports it. The externally visible result must remain equivalent to the canonical contract.

---

# Stage 1: Request Validation

The pipeline validates the OCR request before allocating expensive resources.

Validation includes:

- Request ID is present
- Session and Page references are valid
- Input Image ID exists
- Requested image version is available
- Image role is acceptable for OCR
- Target region is within image bounds
- Requested language hints are supported
- OCR mode is supported
- Cancellation token is active
- Resource and cost limits are valid

Invalid requests must fail before provider invocation.

---

# Stage 2: Input Resolution

The pipeline resolves the authoritative image and optional target region.

Possible inputs:

- Complete normalized page image
- Complete source image when normalization is unnecessary
- User-selected region
- Previously detected text region
- Region scheduled for retry

The resolved input contains:

- Image ID
- Image version
- Asset ID
- Dimensions
- Canonical coordinate space
- Region bounds, when applicable
- Parent lineage
- Content hash

The pipeline must never infer coordinates against a different image version than the one declared by the request.

---

# Stage 3: Image Normalization

Image normalization creates a stable visual input for later stages.

Possible operations:

- Apply orientation metadata
- Convert unsupported color mode
- Remove unsupported animation frames
- Normalize alpha handling
- Correct rotation
- Correct perspective
- Deskew scanned content
- Constrain excessive dimensions
- Decode into a supported pixel format

Normalization creates a derived Image when pixels, dimensions or coordinate space change.

The source image remains immutable.

Required output metadata includes:

- Derived Image ID
- Parent Image ID
- Applied transforms
- Output dimensions
- Output content hash
- Mapping to parent coordinates

---

# Stage 4: OCR Profile Resolution

An OCR Profile defines the processing behavior for a request.

The effective profile may be resolved from:

```text
Request Override
      ↓
Session Preference
      ↓
Source Profile
      ↓
Global Preference
      ↓
System Default
```

A profile may contain:

- Pipeline version
- Recognition mode
- Expected languages
- Script hints
- Reading direction
- Detection strategy
- Preprocessing strategy
- Recognition provider policy
- Region padding
- Upscaling policy
- Confidence thresholds
- Retry policy
- Fallback policy
- Cache policy
- Resource limits
- Privacy policy

Profile resolution is deterministic and the resolved profile revision must be recorded in the result.

---

# Stage 5: Cache Lookup

The pipeline checks for reusable OCR output before processing.

A cache key should include at least:

- Input content hash
- Input region
- Image version
- OCR Pipeline version
- OCR Profile revision
- Detection strategy version
- Recognition provider and model capability version
- Language and script hints
- Preprocessing configuration hash
- Normalization configuration version

A result must not be reused when any semantic input affecting recognition has changed.

Cache entries may exist at several levels:

- Full-page OCR result
- Region-detection result
- Prepared region image
- Per-region recognition result
- Reading-order result
- Normalized text result

Partial cache reuse is allowed when lineage and compatibility can be proven.

---

# Stage 6: Preprocessing

Preprocessing improves recognition quality without changing source semantics.

Possible operations:

- Grayscale conversion
- Contrast adjustment
- Binarization
- Denoising
- Sharpening
- Upscaling
- Border removal
- Background suppression
- Color-channel isolation
- Speech-bubble enhancement
- Text-stroke enhancement
- Inversion for light text on dark background

Preprocessing may generate one or more candidate OCR input images.

```text
Normalized Image
      │
      ├──► General OCR Input
      ├──► High-Contrast Input
      ├──► Inverted Input
      └──► Upscaled Input
```

Candidate generation should be policy-driven. The pipeline must avoid applying every filter blindly because excessive preprocessing increases latency and may damage text features.

---

# Stage 7: Text Region Detection

Text Region Detection locates areas likely to contain readable text.

A detected region should contain:

- Region ID
- Polygon or bounding box
- Detection confidence
- Orientation estimate
- Script or language hint, when available
- Region type hint
- Parent Image ID and version
- Detector version

Possible region types:

- Dialogue
- Narration
- Caption
- Sound effect
- Label
- Sign
- Interface text
- Unknown text

Region typing is advisory. Recognition must not depend on perfect classification.

Detection may be performed by:

- Dedicated text detector
- OCR provider with layout support
- Local vision model
- Site-specific adapter
- User selection
- Hybrid detection

---

# Stage 8: Region Preparation

Each detected region may be prepared independently for recognition.

Typical operations:

- Crop region
- Add safe padding
- Rectify rotated polygon
- Correct perspective
- Upscale small text
- Select preprocessing candidate
- Detect text orientation
- Split oversized region
- Merge fragmented regions when justified

Every prepared region must retain a transform back to the canonical page coordinate space.

Region preparation creates transient buffers or derived `region_crop` / `ocr_input` Images according to retention policy.

---

# Stage 9: Text Recognition

Text Recognition converts prepared visual regions into recognition candidates.

The recognition layer must be provider independent.

A recognition request may include:

- Prepared image reference or in-memory image buffer
- Expected languages
- Script hints
- Orientation
- Region type
- Character whitelist or blacklist
- Recognition mode
- Timeout
- Privacy classification

A recognition response may contain:

- Raw text
- Character, token or line alternatives
- Confidence values
- Word or character geometry
- Detected language
- Detected script
- Provider metadata
- Model version
- Processing duration

Provider-specific response formats must be normalized before leaving the provider adapter.

---

# Stage 10: Geometry Reconstruction

Recognition geometry is converted into CRAI's canonical coordinate space.

This stage:

- Maps provider coordinates to prepared-region coordinates
- Maps region coordinates to OCR-input coordinates
- Maps OCR-input coordinates to normalized-image coordinates
- Maps normalized-image coordinates to source-image coordinates when required
- Validates bounds after every transform
- Preserves polygon geometry when available

```text
Provider Coordinates
        ↓
Prepared Region Coordinates
        ↓
OCR Input Coordinates
        ↓
Normalized Image Coordinates
        ↓
Source Image Coordinates
```

Rounding must not cause text polygons to move outside valid image bounds.

The exact transform chain must be retained for diagnostics and rendering compatibility.

---

# Stage 11: Reading Order Resolution

Reading Order Resolution determines the sequence in which recognized regions and lines should be consumed.

Supported patterns may include:

- Horizontal left-to-right
- Horizontal right-to-left
- Vertical top-to-bottom, columns right-to-left
- Vertical top-to-bottom, columns left-to-right
- Mixed page layout
- Panel-aware comic order
- Explicit user-defined order

Inputs may include:

- Region geometry
- Line geometry
- Orientation
- Script
- Panel layout
- Speech-bubble relationships
- Source preferences
- Provider order hints

Reading order must be represented explicitly. Array position alone must not be treated as authoritative without an order revision or ordering metadata.

When order is uncertain, the result should preserve alternatives or mark the order as low confidence rather than silently presenting a false certainty.

---

# Stage 12: Text Normalization

Text Normalization converts provider output into a stable OCR text representation.

Possible operations:

- Unicode normalization
- Standardize line breaks
- Remove provider artifacts
- Normalize whitespace
- Join fragmented glyphs
- Preserve meaningful punctuation
- Preserve source-script characters
- Normalize repeated OCR control characters
- Associate lines with regions
- Mark uncertain characters

Normalization must not:

- Translate text
- Rewrite style
- Replace names using a glossary
- Correct meaning through unsupported guesses
- Remove intentional punctuation or sound effects solely because they appear unusual

Raw provider text must remain available for diagnostics and manual comparison when retention policy permits.

---

# Stage 13: Confidence Evaluation

CRAI computes normalized confidence independent of any one provider.

Confidence may be evaluated at:

- Character level
- Token level
- Line level
- Region level
- Reading-order level
- Page level

Evaluation may consider:

- Provider confidence
- Detection confidence
- Agreement between recognition candidates
- Language-model plausibility
- Expected script match
- Character corruption rate
- Geometry consistency
- Empty-result anomalies
- Region size and resolution
- Retry history

Provider confidence values must not be compared directly unless calibrated to CRAI's normalized confidence model.

---

# Stage 14: Quality Validation

Quality validation decides whether a result can be accepted, retried, downgraded or rejected.

Validation checks may include:

- Required text exists for detected text-like regions
- Geometry lies inside the referenced image
- Text and geometry counts are consistent
- Declared script matches observed characters
- Reading order contains no cycles
- Region identifiers are unique
- Result schema is valid
- Confidence meets the selected threshold
- Provider response is complete
- Cancellation did not occur during assembly

Possible outcomes:

```text
Accepted
AcceptedWithWarnings
RetryRegion
RetryPage
UseFallback
RequiresUserReview
Rejected
Cancelled
```

A low-confidence result is not automatically a technical failure. It may remain useful for manual correction or side-panel presentation.

---

# Stage 15: Result Assembly

The pipeline assembles a canonical `OCRResult`.

Recommended structure:

```text
OCRResult
├── Result ID
├── Request ID
├── Session ID
├── Page ID
├── Source Image Reference
├── OCR Input Image Reference
├── Pipeline Version
├── Profile Revision
├── Status
├── Detected Languages
├── Reading Direction
├── Regions[]
│   ├── Region ID
│   ├── Geometry
│   ├── Region Type
│   ├── Orientation
│   ├── Raw Recognition
│   ├── Normalized Text
│   ├── Confidence
│   ├── Lines[]
│   ├── Provider Metadata
│   └── Warnings[]
├── Reading Order
├── Page Confidence
├── Warnings[]
├── Diagnostics Summary
├── Cache Metadata
└── Created Time
```

The result must be self-consistent and reference the exact image versions used to produce it.

---

# Stage 16: Cache Write

Accepted results and useful intermediate artifacts may be cached according to policy.

Cache writes must be:

- Atomic
- Versioned
- Content-addressable where practical
- Safe under concurrent requests
- Associated with lineage
- Subject to privacy and retention rules

Cancelled, corrupted or schema-invalid results must not be written as successful cache entries.

Low-confidence results may be cached with their quality status to avoid repeated expensive execution, but retry policy may intentionally bypass them.

---

# Stage 17: Result Publication

After successful assembly, the pipeline publishes the result through its public contract and event model.

Typical outputs:

- Return `OCRResult` to the caller
- Attach result reference to the Page processing state
- Publish completion or warning events
- Record metrics and diagnostics
- Notify Translation that recognized source text is available

Events must carry identifiers and structured metadata, not raw image bytes.

---

# Execution Granularity

The pipeline supports three processing granularities.

## Full Page

Used when:

- A page has not been processed
- Layout and reading order are unknown
- Region detection must run
- The source image changed

## Region

Used when:

- One detected region requires recognition
- A region was manually added or modified
- A low-confidence region is retried
- A user requests correction for one bubble

## Incremental Page

Used when:

- New regions become visible during scrolling
- Screen observation detects a changed area
- Cached regions remain valid
- Only part of a long page requires OCR

Granularity must be explicit in the request and result lineage.

---

# Parallelism

Safe parallelism may include:

- Preparing independent regions concurrently
- Recognizing independent regions concurrently
- Running selected candidate preprocessors concurrently
- Comparing primary and fallback OCR candidates concurrently when policy permits

The pipeline must control parallelism using resource budgets.

```text
Page Job
├── Region Worker 1
├── Region Worker 2
├── Region Worker 3
└── Region Worker N
```

Unbounded region fan-out is prohibited.

Reading-order resolution and final result assembly occur only after required region dependencies are available.

---

# Cancellation

Every expensive or blocking stage must observe cancellation.

Cancellation may originate from:

- User action
- Page replacement
- Session closure
- Newer OCR request superseding the current request
- Timeout
- Application shutdown
- Resource-pressure policy

On cancellation, the pipeline should:

- Stop scheduling new work
- Cancel provider calls when supported
- Release image buffers
- Preserve valid reusable intermediate cache entries only when policy allows
- Mark the request as cancelled
- Avoid publishing a successful result
- Emit cancellation diagnostics

A stale result must never replace a newer Page OCR revision.

---

# Retry Strategy

Retries should occur at the smallest useful scope.

Preferred order:

```text
Retry Recognition for Region
        ↓
Retry Region with Alternate Preprocessing
        ↓
Retry Region with Alternate Provider
        ↓
Retry Region with Expanded Bounds
        ↓
Retry Page Detection
        ↓
Request User Review
```

Retry decisions may consider:

- Failure category
- Region confidence
- Provider health
- Remaining latency budget
- Remaining cost budget
- Attempt count
- Whether the request is interactive or background

Retries must use bounded attempt counts and record attempt lineage.

---

# Fallback Strategy

Fallback may change:

- OCR provider
- OCR model
- Detection strategy
- Preprocessing profile
- Recognition granularity
- Language hints
- Region bounds
- Online versus offline execution

Fallback must not change the semantic request without recording the change.

Example:

```text
Primary: Local Chinese OCR
      │ low confidence
      ▼
Fallback: Cloud OCR with vertical-text support
      │ still uncertain
      ▼
Result: AcceptedWithWarnings + User Review
```

Provider fallback remains subject to privacy policy. An image classified as local-only must never be sent to a cloud provider.

---

# Provider Routing

OCR provider selection is based on capability and policy rather than direct implementation references.

Routing factors may include:

- Required languages and scripts
- Vertical-text support
- Layout-detection support
- Local or cloud requirement
- Privacy classification
- Provider health
- Latency target
- Cost budget
- Image size
- Offline availability
- Model confidence history
- User preference

The pipeline requests an OCR capability. Provider Management selects a compatible implementation.

---

# Resource Management

OCR is CPU-, GPU-, memory- and network-intensive.

The pipeline must operate under explicit budgets for:

- Concurrent page jobs
- Concurrent region jobs
- In-memory image bytes
- Maximum image dimensions
- Provider calls
- GPU execution slots
- Request timeout
- Retry count
- Estimated monetary cost

Large images should be tiled or downscaled only through a strategy that preserves coordinate mapping.

Temporary buffers must be released as soon as downstream stages no longer need them.

---

# Privacy and Security

OCR inputs may contain private reading content, account information or captured screen data.

The pipeline must:

- Respect local-only processing policies
- Avoid logging raw image content
- Avoid logging full recognized text by default
- Redact sensitive provider errors
- Use encrypted transport for remote providers
- Apply provider retention policy
- Prevent unauthorized cross-session cache reuse
- Validate imported image formats
- Limit decompression and image dimensions
- Isolate untrusted decoders where practical

OCR cache scope must be compatible with the source privacy classification.

---

# Observability

Recommended metrics:

- Total OCR duration
- Time per stage
- Time to first recognized region
- Region count
- Recognized character count
- Empty-region rate
- Page confidence
- Low-confidence region count
- Cache-hit rate
- Retry count
- Fallback count
- Provider success rate
- Provider latency
- Preprocessing variant count
- Peak memory usage
- Cancellation rate
- Estimated cost

Recommended trace identifiers:

- Request ID
- Session ID
- Page ID
- Image ID
- OCR Result ID
- Region ID
- Provider request ID

Diagnostics must preserve stage and provider metadata without exposing raw private content in ordinary logs.

---

# Error Categories

Stable OCR error categories include:

## Request Errors

- Invalid OCR request
- Unsupported OCR mode
- Invalid target region
- Incompatible profile

## Image Errors

- Image unavailable
- Unsupported format
- Decode failure
- Invalid dimensions
- Coordinate mismatch
- Transform failure

## Detection Errors

- Detector unavailable
- Detection timeout
- Invalid region geometry
- No text regions detected

## Recognition Errors

- Provider unavailable
- Authentication failure
- Rate limited
- Recognition timeout
- Invalid provider response
- Unsupported language
- Empty recognition result

## Quality Errors

- Confidence below threshold
- Script mismatch
- Reading order unresolved
- Geometry inconsistent
- Result validation failed

## Runtime Errors

- Cancelled
- Resource budget exceeded
- Memory pressure
- Stale request
- Internal pipeline failure

Provider-specific errors must be translated into these stable categories before crossing the OCR boundary.

---

# Event Model

Typical events:

- `OCRRequested`
- `OCRStarted`
- `OCRStageStarted`
- `OCRRegionDetected`
- `OCRRegionRecognized`
- `OCRProgressChanged`
- `OCRRetryScheduled`
- `OCRFallbackSelected`
- `OCRCompleted`
- `OCRCompletedWithWarnings`
- `OCRReviewRequired`
- `OCRFailed`
- `OCRCancelled`
- `OCRCacheHit`
- `OCRResultInvalidated`

High-frequency progress events should be throttled or coalesced before reaching the UI.

Events describe facts. They do not instruct other modules how to implement their reactions.

---

# State Model

A pipeline execution may use the following states:

```text
Created
   │
   ▼
Validating
   │
   ▼
Preparing
   │
   ▼
Detecting
   │
   ▼
Recognizing
   │
   ▼
Reconstructing
   │
   ▼
ValidatingResult
   │
   ├──► Retrying
   │        │
   │        └──────────────► Preparing / Recognizing
   │
   ├──► ReviewRequired
   ├──► Failed
   ├──► Cancelled
   └──► Completed
```

The detailed execution state machine belongs in the OCR state document. This pipeline document defines only the expected stage progression.

---

# Manual Review and Correction

OCR must support human correction without destroying machine output.

A correction should record:

- OCR Result ID
- Region ID
- Original normalized text
- Corrected text
- Correction source
- Correction time
- Base result revision
- Optional reason

Manual correction creates a new revision or correction layer. It must not silently mutate historical provider output.

Corrected text may be consumed by Translation while raw OCR remains available for audit and comparison.

---

# Result Invalidation

An OCR result becomes invalid when a semantic dependency changes.

Invalidation triggers may include:

- Source image content changed
- Image version changed
- Target region changed
- OCR Profile changed with OCR impact
- Pipeline compatibility changed
- Detection or recognition model became incompatible
- User requested forced reprocessing
- Coordinate lineage became invalid
- Manual region structure changed

Presentation-only changes do not invalidate OCR.

Translation preference changes do not invalidate OCR unless they also alter source text correction policy.

---

# Cache Invalidation Matrix

| Change | Detection Cache | Recognition Cache | Reading Order | Final OCR Result |
|---|---:|---:|---:|---:|
| Source image pixels changed | Invalidate | Invalidate | Invalidate | Invalidate |
| Image metadata only changed | Usually retain | Usually retain | Usually retain | Revalidate |
| Region bounds changed | Affected region | Affected region | Recompute | Invalidate affected result |
| Preprocessing profile changed | Retain detection when compatible | Invalidate | Usually retain | Reassemble |
| Recognition provider changed | Retain | Invalidate | Usually retain | Reassemble |
| Language hint changed | Usually retain | Invalidate | Revalidate | Reassemble |
| Reading direction changed | Retain | Retain | Invalidate | Reassemble |
| Confidence threshold changed | Retain | Retain | Retain | Revalidate |
| Translation settings changed | Retain | Retain | Retain | Retain |
| Presentation settings changed | Retain | Retain | Retain | Retain |

The table describes default behavior. A versioned compatibility policy may override it when safety can be proven.

---

# Performance Modes

Recommended execution modes:

## Interactive Fast

Prioritizes low latency.

- Reuse cache aggressively
- Limit preprocessing candidates
- Use low-latency provider
- Return partial region results
- Schedule uncertain regions for later refinement

## Balanced

Balances latency and quality.

- Standard detection
- Selected preprocessing retry
- Normal confidence thresholds
- Bounded provider fallback

## Quality

Prioritizes recognition accuracy.

- More preprocessing candidates
- Higher-resolution region preparation
- Candidate comparison
- Stronger fallback policy
- More expensive reading-order analysis

## Offline

Restricts execution to local capabilities.

- Local detector
- Local OCR model
- No cloud fallback
- Local cache only

Mode selection is a preference and routing concern. The public OCR result contract remains stable.

---

# Incremental Results

The pipeline may publish partial results to reduce perceived latency.

Possible progression:

```text
Regions Detected
      ↓
First Regions Recognized
      ↓
Partial Reading Order
      ↓
All Required Regions Recognized
      ↓
Validated Final OCR Result
```

Partial results must declare:

- Result revision
- Completion status
- Available regions
- Pending regions
- Whether reading order is provisional
- Whether translation may safely begin

A partial result must never be presented as final without explicit completion status.

---

# Idempotency and Concurrency

Equivalent active requests for the same Page, image version, region and OCR profile may be deduplicated.

Concurrency rules:

1. Every execution has a unique Request ID.
2. Every Page tracks the latest relevant OCR request revision.
3. A completed stale request may be cached but must not replace a newer result.
4. Result attachment uses compare-and-set or equivalent revision validation.
5. Repeated requests may share work only when privacy, cancellation and lifecycle rules remain valid.
6. Cancelling one consumer must not cancel shared work still required by another consumer unless the shared-work contract supports reference counting.

---

# Public Input Contract

A conceptual `OCRRequest` contains:

- Request ID
- Session ID
- Page ID
- Image ID
- Image version
- Optional target region
- Processing granularity
- OCR mode
- Language hints
- Reading-direction hint
- Effective OCR Profile reference
- Priority
- Deadline
- Privacy classification
- Cache policy
- Cancellation context

The detailed schema belongs in the OCR contract document.

---

# Public Output Contract

A conceptual `OCRResult` contains:

- Result identity and revision
- Request and Page references
- Exact source and derived image references
- Structured regions and lines
- Canonical geometry
- Raw and normalized recognized text
- Reading order
- Language and script metadata
- Confidence
- Warnings
- Provider and pipeline version metadata
- Cache and lineage metadata
- Completion status

Translation and Presentation must depend on this stable contract, not on provider-native OCR responses.

---

# Architecture Invariants

1. OCR never mutates the original source image.
2. Every coordinate-bearing OCR artifact references the exact image version that produced it.
3. Every derived OCR image preserves parent lineage and transform metadata.
4. Provider-specific OCR formats never cross the OCR provider adapter boundary.
5. OCR output remains source-language text; translation is a separate pipeline.
6. Raw recognition and normalized recognition are distinguishable.
7. Reading order is explicit, versioned and allowed to be uncertain.
8. Low confidence is represented as quality metadata, not hidden.
9. Retry and fallback are bounded by latency, cost, privacy and resource policy.
10. Cache keys include every semantic dependency that can affect recognition.
11. A stale execution never replaces a newer Page OCR revision.
12. Cancellation is observed by every expensive stage.
13. Events never contain raw image bytes.
14. Ordinary logs do not contain full private images or recognized text.
15. Manual corrections preserve original machine output and revision history.
16. Translation and Presentation consume the canonical OCR contract, not provider responses.
17. Region-level retry is preferred over full-page retry when valid.
18. Identical inputs and profile versions produce structurally equivalent results, subject to provider nondeterminism explicitly recorded in metadata.

---

# Recommended Initial MVP Path

The first CRAI OCR implementation should prefer a narrow and testable pipeline:

```text
Normalized Page Image
      ↓
General Text Detection
      ↓
Region Crop + Padding
      ↓
Chinese / English Recognition
      ↓
Basic Geometry Mapping
      ↓
Rule-Based Reading Order
      ↓
Unicode and Whitespace Normalization
      ↓
Confidence Threshold
      ↓
Structured OCR Result
```

Initial MVP constraints:

- Full-page and manual-region OCR
- Simplified Chinese, Traditional Chinese and English hints
- Horizontal and vertical text support where provider capability exists
- One primary OCR provider
- One fallback provider or local fallback
- Region-level retry
- Full-result and per-region cache
- Side-panel review for low-confidence text
- No automatic image inpainting inside the OCR boundary
- No semantic rewriting inside normalization

Advanced panel understanding, sound-effect extraction and multi-candidate OCR voting should remain optional until validated by real reading tests.

---

# Open Decisions

The following questions require prototypes or product validation:

- Which OCR provider best handles Chinese vertical dialogue at acceptable latency and cost?
- Should detection and recognition use one provider or separate specialized providers?
- What confidence thresholds are useful for automatic translation versus manual review?
- How much preprocessing improves accuracy before latency becomes unacceptable?
- Should sound effects be detected by default or only on demand?
- How should panel order influence dialogue reading order?
- Which intermediate images should be retained for diagnostics?
- Should OCR correction learn per series, globally or not at all in the MVP?
- When should CRAI run dual-provider recognition?
- How should long scrolling images be tiled without breaking reading order?

These decisions must not be hidden inside provider adapters. They belong to explicit, versioned OCR policies.

---

# Related Documents

- `README.md`
- `CONTRACT.md`
- `STAGES.md`
- `PROVIDERS.md`
- `PREPROCESSING.md`
- `DETECTION.md`
- `RECOGNITION.md`
- `READING_ORDER.md`
- `CONFIDENCE.md`
- `RETRY.md`
- `FALLBACK.md`
- `CACHE.md`
- `OBSERVABILITY.md`
- `../../architecture/DATA_FLOW.md`
- `../../architecture/STATE_MACHINE.md`
- `../../architecture/EVENT_BUS.md`
- `../../architecture/MODULE_DEPENDENCY.md`
- `../../domain/IMAGE.md`
- `../../domain/PAGE.md`
- `../../domain/TEXT_BLOCK.md`
