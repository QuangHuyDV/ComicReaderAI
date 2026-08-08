# Recognition Module Specification

> **Project:** CRAI
> **Module:** Recognition
> **Path:** `02-modules/recognition/MODULE.md`
> **Version:** 1.1
> **Status:** Architecture Draft
> **Architecture Reference:** `01-architecture/ocr/`

---

# 1. Module Definition

Recognition là Core Business Processing Module chịu trách nhiệm chuyển image-based input thành structured source-content candidate để Runtime đánh giá và publish.

Recognition bắt đầu khi Runtime cung cấp một immutable `RecognitionAttemptInput` hợp lệ.

Recognition kết thúc khi module tạo:

* Candidate Recognition Artifact
* module warnings
* normalized module error nếu có
* retry/fallback hints nếu có
* diagnostics
* Attempt Completion

Recognition không trực tiếp tạo authoritative published result.

```text
Image Artifact
      ↓
Runtime Recognition Attempt
      ↓
Recognition Module
      ↓
Candidate Recognition Artifact
      ↓
Runtime Authority Validation
      ↓
Artifact Publication
```

Recognition là module điều phối OCR semantics cho image-based source content.

Chi tiết semantics của từng OCR stage được định nghĩa tại:

```text
01-architecture/ocr/
```

Recognition Module không định nghĩa lại các architecture contract đó.

---

# 2. Module Identity

```text
Module ID:
    recognition

Module Type:
    Core Business Processing Module

Primary Domain:
    Image-to-Structured-Source Processing

Execution Model:
    Runtime WorkItem / Attempt

Primary Input:
    Image Artifact Reference

Primary Candidate Output:
    Candidate Recognition Artifact

Published Output:
    Recognition Artifact

State Ownership:
    Recognition semantic state only

Execution Authority:
    Runtime

MVP Priority:
    Required for image-based reading flow
```

Recognition có thể được bypass khi structured source text đã tồn tại.

Ví dụ:

* browser DOM extraction
* clipboard text
* imported structured document
* direct text input

---

# 3. Architectural Position

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
Recognition Module
      ↓
Candidate Recognition Artifact
      ↓
Runtime Validation
      ↓
Artifact Publication
      ↓
Recognition Artifact
      ↓
Text Processing
```

Recognition không tự tạo downstream WorkItem.

Runtime Control và Business Pipeline Orchestration quyết định execution tiếp theo.

---

# 4. Architecture Relationship

Recognition Module sử dụng OCR Architecture thay vì định nghĩa lại nó.

Canonical OCR flow:

```text
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

Authoritative documents:

| Concern             | Architecture Owner                      |
| ------------------- | --------------------------------------- |
| OCR pipeline        | `01-architecture/ocr/PIPELINE.md`       |
| Image preprocessing | `01-architecture/ocr/PREPROCESS.md`     |
| Region / Detection  | `01-architecture/ocr/DETECTION.md`      |
| Text recognition    | `01-architecture/ocr/RECOGNITION.md`    |
| Writing direction   | `01-architecture/ocr/TEXT_DIRECTION.md` |
| Layout              | `01-architecture/ocr/LAYOUT.md`         |
| OCR Document        | `01-architecture/ocr/POSTPROCESS.md`    |
| Quality             | `01-architecture/ocr/QUALITY.md`        |
| Reading Order       | `01-architecture/ocr/READING_ORDER.md`  |
| OCR Providers       | `01-architecture/ocr/PROVIDERS.md`      |

Recognition Module owns orchestration of those semantics inside a Runtime Attempt.

---

# 5. Responsibilities

Recognition owns the following module responsibilities.

## 5.1 Input Contract Validation

Validate module-level requirements:

* input artifact reference
* supported artifact type
* image metadata
* optional region selection
* Recognition Profile
* language/script hints
* capability requirements
* configuration compatibility
* privacy requirements

Runtime authority identity is not validated by Recognition.

Runtime Control owns authority validation.

---

## 5.2 Recognition Planning

Recognition creates an immutable `RecognitionPlan`.

Plan may consider:

* input characteristics
* Recognition Profile
* OCR Profile
* required OCR capabilities
* language/script hints
* orientation hints
* privacy constraints
* configuration snapshot
* provider-selection request
* semantic processing requirements

Recognition Plan describes **what OCR semantics are required**.

Runtime decides **how execution is admitted and scheduled**.

---

## 5.3 OCR Pipeline Coordination

Recognition coordinates the OCR architecture stages required for the current operation.

Conceptually:

```text
RecognitionAttemptInput
        ↓
RecognitionPlan
        ↓
OCR Architecture
        ↓
OCR Document
        ↓
Quality / Reading Order
        ↓
Candidate Recognition Artifact
```

Recognition does not redefine stage semantics.

---

## 5.4 Provider Capability Requirements

Recognition declares required capability characteristics.

Examples:

* detection required
* recognition required
* language support
* script support
* vertical-text support
* geometry requirement
* confidence requirement
* local-only requirement
* remote allowed
* partial output allowed

Provider capability semantics belong to OCR Provider Architecture.

---

## 5.5 Provider Output Normalization Boundary

Recognition ensures provider-native output reaches OCR Architecture only through normalized CRAI contracts.

Provider SDK types must never become public Recognition Artifact fields.

---

## 5.6 Candidate Artifact Assembly

Recognition creates an immutable `CandidateRecognitionArtifact`.

Candidate remains private until Runtime accepts it.

---

## 5.7 Semantic Compatibility

Recognition defines semantic metadata needed to determine whether a Recognition Artifact may be reused for equivalent input.

Recognition does not decide physical cache retention.

---

## 5.8 Module Diagnostics

Recognition may expose:

* phase timings
* warnings
* provider provenance
* quality signals
* plan decisions
* output completeness
* module failure classification
* compatibility information

Diagnostics must not become hidden execution control.

---

# 6. Non-Responsibilities

Recognition does not own:

* source capture
* source observation
* frame-change detection
* scroll detection
* Reading Session lifecycle
* Revision authority
* WorkItem lifecycle
* Attempt lifecycle
* Scheduler admission
* Work Queue
* Runtime retry decision
* cancellation authority
* Artifact publication authority
* Artifact retention
* Cache Policy
* durable persistence
* provider lifecycle
* provider credential storage
* Text Processing semantics
* semantic OCR correction
* Translation
* translated-text layout
* Presentation
* user history

Recognition does not directly call Translation or Presentation implementations.

---

# 7. Runtime Boundary

Recognition executes inside a Runtime Attempt.

```text
Runtime creates WorkItem
        ↓
Scheduler admits Attempt
        ↓
Worker receives RecognitionAttemptInput
        ↓
Recognition executes
        ↓
CandidateRecognitionArtifact
        ↓
AttemptCompletion
        ↓
Runtime authority validation
        ↓
Artifact Store ownership transfer
        ↓
RecognitionArtifact published
```

Recognition does not:

* grant authority
* choose current Revision
* publish accepted Artifact
* schedule downstream WorkItem
* retry itself
* mutate Runtime state

This boundary from the original specification is preserved.

---

# 8. Recognition Attempt Input

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
├── RegionSelection?
├── ConfigurationSnapshotId
├── PrivacyContextRef
├── ExecutionContextRef
├── CancellationContextRef
└── TraceContext
```

## Input Rules

1. Input Artifact is immutable.
2. Recognition does not receive mutable raw image objects across the module boundary.
3. RegionSelection uses source coordinate space.
4. Queue priority is not part of Recognition semantics.
5. Provider credentials are never included.
6. Timeout/deadline comes from Runtime Execution Context.
7. SessionId may be absent for standalone imported images.

---

# 9. Recognition Operations

```text
RecognitionOperation
├── RECOGNIZE_IMAGE
├── RECOGNIZE_REGION
└── EVALUATE_IMAGE
```

## RECOGNIZE_IMAGE

Processes a complete image input.

Typical use:

* comic page
* screenshot
* scanned document
* imported image
* stable captured frame

---

## RECOGNIZE_REGION

Processes a selected source-space region.

Typical use:

* manual region selection
* local OCR rerun
* selected speech bubble
* evaluation
* provider comparison

---

## EVALUATE_IMAGE

Diagnostic/evaluation operation.

It does not produce a user-facing artifact by default unless Runtime policy allows it.

---

# 10. Recognition Profile

```text
RecognitionProfile
├── AUTOMATIC
├── COMIC_PAGE
├── SCREENSHOT
├── SINGLE_REGION
└── STRUCTURED_PAGE
```

Profile affects planning.

It does not change the public Recognition Artifact contract.

---

## AUTOMATIC

Select processing requirements using:

* input characteristics
* hints
* architecture capabilities
* provider capabilities

---

## COMIC_PAGE

Optimized for:

* irregular region layout
* multiple text regions
* vertical text
* text over artwork
* speech/narration structures

---

## SCREENSHOT

Optimized for:

* application text
* browser UI
* mixed structured regions
* mostly horizontal content

---

## SINGLE_REGION

May bypass page-wide processing that is unnecessary for the selected Region.

---

## STRUCTURED_PAGE

Optimized for:

* columns
* prose
* regular line structure
* document-like layout

---

# 11. Recognition Plan

```text
RecognitionPlan
├── PlanId
├── Operation
├── RecognitionProfile
├── OCRProfile
├── RequiredCapabilities
├── ProviderSelectionRequest
├── RegionExecutionPolicy
├── CompatibilityPolicy
├── ConfigurationVersions
└── PrivacyConstraints
```

The plan is immutable within one Attempt.

Detailed OCR preprocessing, detection, direction, layout, quality and reading-order semantics remain in `01-architecture/ocr/`.

---

# 12. Internal Processing Flow

```text
Receive RecognitionAttemptInput
        ↓
Validate Module Contract
        ↓
Build RecognitionPlan
        ↓
Resolve OCR Capability Requirements
        ↓
Acquire Input Artifact Lease
        ↓
Execute Canonical OCR Architecture
        ↓
Receive OCR Document
        ↓
Apply Quality / Reading Order policy
        ↓
Validate Module Invariants
        ↓
Assemble CandidateRecognitionArtifact
        ↓
Submit AttemptCompletion
        ↓
Release Attempt-local Resources
```

This intentionally replaces the previous MODULE-level duplication of preprocessing, detection, geometry and reading-order algorithms.

---

# 13. Combined and Composed OCR Execution

Recognition may request either execution form.

## Combined

```text
Image
    ↓
Combined OCR Provider
    ↓
Normalized OCR contracts
```

Suitable when a Provider supplies multiple OCR capabilities with acceptable quality.

---

## Composed

```text
Image
    ↓
Detection Capability
    ↓
Recognition Capability
    ↓
Direction / Layout / Postprocessing
```

Suitable when specialized capabilities produce better results.

Combined and Composed strategies must still obey the same OCR Architecture contracts.

---

# 14. Recognition Attempt Output

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

Attempt Output is not a published Recognition Artifact.

Runtime determines disposition:

```text
ACCEPT
REJECT_STALE
REJECT_CANCELED
REJECT_DUPLICATE
REJECT_INVALID
```

---

# 15. Candidate Recognition Artifact

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

Candidate Artifact is:

* private until Runtime acceptance
* non-authoritative
* not reusable by default
* pending ownership transfer
* cleaned up if Runtime rejects it

---

# 16. Published Recognition Artifact

After Runtime acceptance:

```text
RecognitionArtifact
├── ArtifactId
├── ArtifactType
├── ArtifactContractVersion
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

Published Artifact is:

* immutable
* provider-independent
* source-traceable
* spatially aligned through OCR Document
* reusable only when compatibility and Cache Policy permit
* independent of Runtime Attempt status

---

# 17. Recognition Artifact vs OCR Document

This distinction is important.

```text
OCR Document
    → OCR Architecture canonical structured result

Recognition Artifact
    → module-level published artifact
       referencing OCR results and module metadata
```

Recognition Artifact does not redefine:

* Region
* Character
* Word
* Line
* Paragraph
* Layout Tree
* Text Direction
* Reading Order Graph

Those semantics remain owned by OCR Architecture.

---

# 18. Completeness

Recognition uses:

```text
RecognitionCompleteness
├── COMPLETE
├── PARTIAL
├── EMPTY_VALID
└── UNKNOWN
```

An empty OCR result may still represent successful processing.

Example:

```text
Completeness = EMPTY_VALID
OCR Document contains no readable Regions
```

Empty does not automatically mean failure.

---

# 19. Module Warnings

Recognition may expose degraded-but-usable conditions.

Examples:

```text
NO_READABLE_TEXT_DETECTED
LOW_DETECTION_CONFIDENCE
LOW_RECOGNITION_CONFIDENCE
UNSUPPORTED_ORIENTATION_FALLBACK
UNSUPPORTED_LANGUAGE_FALLBACK
READING_ORDER_UNCERTAIN
PROVIDER_CONFIDENCE_UNAVAILABLE
REMOTE_PROVIDER_USED
PARTIAL_RECOGNITION
```

Warnings are not Runtime terminal outcomes.

Detailed Detection/Recognition/Direction quality semantics remain owned by OCR Architecture.

---

# 20. Module Error Model

Recognition owns module-level failure semantics.

```text
RecognitionModuleError
├── Code
├── OperationPhase
├── RetryHint?
├── AffectedScope?
├── ProviderErrorRef?
├── DiagnosticRef?
└── Metadata?
```

Examples:

```text
RECOGNITION_INPUT_INVALID
RECOGNITION_IMAGE_INVALID
RECOGNITION_LANGUAGE_UNSUPPORTED
RECOGNITION_OPERATION_FAILED
RECOGNITION_CANDIDATE_INVALID
RECOGNITION_RESOURCE_EXHAUSTED
RECOGNITION_INTERNAL_ERROR
```

Stage-specific semantic errors remain owned by their OCR Architecture documents.

Provider-native errors must be normalized before crossing provider boundaries.

---

# 21. Retry Hint

Recognition may return:

```text
RetryHint
├── Retryability
├── SuggestedStrategy[]
├── AlternativeProviderAllowed
├── AlternativePreparationAllowed
└── ReasonCode
```

Possible semantic suggestions:

```text
SAME_PROVIDER
ALTERNATIVE_PROVIDER
ALTERNATIVE_PREPARATION
REGION_ONLY
RESOURCE_WAIT
NO_RETRY
```

Runtime Retry Policy owns:

* whether retry occurs
* retry count
* delay
* budget
* WorkItem/Attempt creation
* fallback execution
* authority revalidation

Recognition never creates its own retry Attempt.

---

# 22. Cancellation

Recognition consumes Runtime-provided cancellation context.

Meaningful checkpoints include:

* before input acquisition
* before expensive OCR work
* between bounded Region batches
* before Candidate assembly
* before Completion submission

Recognition does not own the global cancellation registry.

Late provider completion does not regain authority after Runtime revocation.

---

# 23. Authority

Recognition preserves:

```text
SessionId?
RevisionId
WorkItemId
AttemptId
ConfigurationSnapshotId
InputArtifactRef
```

but never owns authority over these identities.

Runtime Control remains the authority owner.

---

# 24. Publication

Recognition does not publish accepted Artifact.

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

Module event emission does not replace publication.

---

# 25. Provider Boundary

Recognition declares required OCR capabilities.

OCR Provider Architecture owns:

* Provider Contract
* Provider Adapter
* capability semantics
* provider response normalization
* provider-native error mapping

Recognition does not redefine the provider model.

See:

```text
01-architecture/ocr/PROVIDERS.md
```

---

# 26. Provider Lifecycle Ownership

Recognition does not own:

* provider registration
* initialization
* model loading
* provider health
* GPU context
* provider concurrency
* provider client lifetime
* shutdown

Those responsibilities belong to Provider Manager / Runtime / Resource Manager according to the applicable infrastructure contracts.

---

# 27. Semantic Compatibility

Recognition owns the semantic definition of artifact compatibility.

Conceptually:

```text
RecognitionCompatibilityMetadata
├── InputContentIdentity
├── RecognitionContractVersion
├── RecognitionProfileVersion
├── OCRPipelineVersion
├── OCRProfileVersion
├── ProviderProfileVersion
├── LanguageHints
├── ScriptHints
├── ConfigurationVersions
└── PrivacyPartition
```

Exact stage/model versions may be captured through OCR Document lineage rather than duplicated here.

---

# 28. Reuse Boundary

```text
Recognition
    → defines semantic compatibility

Runtime Cache Policy
    → decides whether reuse is allowed

Artifact Store
    → owns shared runtime artifact lifecycle

Storage
    → provides durable persistence
```

`RevisionId` alone is not reuse identity.

---

# 29. Resource Lifecycle

Recognition uses Runtime-managed resources through contracts.

Conceptual flow:

```text
Acquire Input Artifact Lease
        ↓
Execute Attempt-local OCR processing
        ↓
Create Candidate Artifact
        ↓
Transfer Candidate or Cleanup
        ↓
Release Attempt-local resources
        ↓
Release Input Lease
```

Recognition never disposes shared input artifacts directly.

---

# 30. Recognition Operation Phases

These phases are diagnostic only:

```text
VALIDATING
PLANNING
ACQUIRING_INPUT
EXECUTING_OCR
ASSEMBLING_CANDIDATE
FINALIZING
```

They are not WorkItem or Attempt states.

Detailed stage execution remains represented by OCR Architecture/diagnostics.

---

# 31. Concurrency

Recognition follows these rules:

1. Runtime Scheduler owns admission.
2. Provider capacity is externally controlled.
3. Region-level parallelism must be bounded.
4. Worker resources are Attempt-local unless explicitly leased.
5. Input Artifact is accessed through a Resource Lease.
6. Image copies should be minimized.
7. Shared provider/model initialization is not request-scoped.
8. Same semantic input/config should produce equivalent normalized structure where provider behavior permits.
9. Obsolete work loses authority quickly.
10. Shutdown uses bounded drain.

---

# 32. Module State

Recognition should keep minimal persistent mutable state.

Allowed module-owned long-lived state:

* Recognition Profile definitions
* plan construction rules
* module error/warning definitions
* compatibility rules
* capability requirement builders
* module diagnostics schema

Not module-owned:

* provider health
* loaded models
* WorkItem registry
* Attempt registry
* cancellation registry
* Artifact retention
* Session state
* Runtime authority

---

# 33. Events

Recognition may define module/domain facts such as:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_OCR_COMPLETED
RECOGNITION_WARNING_RECORDED
RECOGNITION_CANDIDATE_CREATED
```

Runtime owns execution facts such as:

```text
WORKITEM_CREATED
ATTEMPT_STARTED
ATTEMPT_COMPLETED
ATTEMPT_FAILED
ATTEMPT_CANCELED
ARTIFACT_PUBLISHED
```

Events:

* contain references rather than image payload
* do not grant authority
* do not schedule hidden downstream work
* follow Event Bus architecture

---

# 34. Observability

Recognition may expose:

```text
recognition.plan_ms
recognition.execution_ms
recognition.candidate_assembly_ms
recognition.total_ms
recognition.region_count
recognition.character_count
recognition.warning_count
recognition.completeness
recognition.provider_profile
recognition.execution_class
```

Detailed OCR-stage measurements are owned by their respective architecture/instrumentation boundaries.

Authority validation and publication latency belong to Runtime Observability.

Telemetry must not contain raw image or full recognized text by default.

---

# 35. Privacy

Recognition must ensure:

1. Raw image bytes do not appear in normal logs.
2. Full recognized text is not emitted in production logs by default.
3. Credentials never appear in module input/output.
4. Remote execution is explicit and traceable.
5. Local-only input never uses remote provider.
6. Temporary image resources are released after use.
7. Events carry references rather than image payload.
8. Diagnostic capture requires explicit policy.
9. Artifact output respects privacy partition.
10. Provider metadata is sanitized.

---

# 36. Data Ownership

Recognition owns:

* module boundary
* Recognition Attempt contract
* Recognition Plan
* Candidate Recognition Artifact
* Recognition Artifact module contract
* module warnings/errors
* retry hints
* compatibility semantics
* module diagnostics
* completeness semantics

Recognition does **not** own:

* Region semantics
* Recognition text hierarchy semantics
* Text Direction semantics
* Layout Tree semantics
* OCR Document semantics
* Quality semantics
* Reading Order semantics

Those belong to `01-architecture/ocr/`.

Recognition also does not own:

* provider lifecycle
* WorkItem/Attempt state
* Runtime authority
* Artifact retention
* durable persistence
* Translation output
* UI layout

This is the main ownership correction from the previous document. The old specification explicitly claimed ownership of region/line, initial reading order and quality rules; those are now delegated to the authoritative OCR Architecture documents.

---

# 37. Dependencies

Allowed dependency categories:

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
session implementation
storage implementation
scheduler implementation
work-queue implementation
provider SDK outside Adapter
```

Runtime integration occurs through contracts.

---

# 38. Testing Requirements

## Unit Tests

Recognition Module tests should focus on module-owned semantics:

* input validation
* Recognition Plan
* Candidate assembly
* completeness
* warning/error mapping
* retry hints
* compatibility metadata
* ownership invariants

OCR algorithm semantics should be tested in the OCR Architecture/component implementations that own them.

---

## Provider Contract Tests

Verify:

* provider adapter isolation
* capability compatibility
* valid request mapping
* normalized result
* normalized error
* privacy/local-only enforcement

---

## Runtime Integration Tests

```text
Image Artifact
    → Recognition Attempt
    → Candidate
    → Runtime Publication
```

```text
Old Revision Attempt
    → New Revision current
    → old Candidate rejected
```

```text
Non-cancelable Provider
    → authority revoked
    → late output rejected
```

```text
Recognition Artifact
    → Text Processing
```

---

# 39. MVP Implementation Contract

## Input

```text
Image Artifact Reference
optional source-space Region
Simplified Chinese hint
optional Traditional Chinese hint
English hint
Automatic / Comic Page profile
```

## Output

```text
Candidate Recognition Artifact
OCR Document reference
basic Reading Order result/reference
Warnings
Completeness
Compatibility Metadata
Quality summary/reference where available
```

## Runtime Controls

```text
WorkItem / Attempt identity
Cancellation Context
Timeout / Deadline
Provider Selection
Authority Validation
Publication
Retry
```

---

# 40. MVP Non-Requirements

MVP does not require:

* handwriting recognition
* full SFX semantic understanding
* character-level geometry everywhere
* polygon output everywhere
* provider marketplace
* semantic OCR repair
* inpainting
* translated-text insertion
* distributed Recognition
* per-character streaming publication

---

# 41. Acceptance Criteria

Recognition Module architecture is acceptable when:

1. OCR Provider implementation is replaceable.
2. Recognition Artifact is independent from Provider SDK types.
3. Public OCR geometry remains source-traceable through OCR Document.
4. Runtime prevents stale publication.
5. Text Processing consumes Recognition Artifact without Provider knowledge.
6. Recognition never directly invokes Translation.
7. Local-only execution policy is enforceable.
8. Raw OCR source text is preserved by OCR Architecture.
9. Reading Order is explicit and externally owned.
10. Errors and warnings are normalized.
11. Recognition creates Candidate only.
12. Runtime owns acceptance/publication.
13. Retry and cancellation are not module-owned.
14. Compatibility metadata supports safe reuse.
15. Attempt-local resources release correctly.
16. Normal telemetry remains content-free.
17. OCR semantic concepts have a single owner in `01-architecture/ocr/`.

---

# 42. Architecture Invariants

1. Recognition accepts image-based input.

2. Input Artifact is immutable.

3. Recognition never mutates source image.

4. Recognition creates Candidate Artifact only.

5. Recognition never grants Runtime authority.

6. Recognition never publishes accepted Artifact.

7. Runtime Control owns authority.

8. Artifact Store owns published payload lifecycle.

9. Recognition does not own WorkItem lifecycle.

10. Recognition does not own Attempt lifecycle.

11. Recognition does not retry itself.

12. Recognition observes Runtime cancellation.

13. Provider lifecycle is external to Recognition.

14. Provider SDK types stay behind Adapter boundaries.

15. OCR geometry remains traceable to source coordinates.

16. OCR semantic models are owned by `01-architecture/ocr/`.

17. Raw source text is not semantically rewritten by Recognition.

18. Missing confidence remains explicit rather than fabricated.

19. Warning differs from failure.

20. Empty OCR result may be valid success.

21. User correction never silently overwrites machine OCR artifact.

22. Local-only input never uses remote Provider.

23. Recognition does not perform Translation.

24. Recognition does not render UI.

25. Image payload never travels through Event Bus.

26. Recognition defines semantic compatibility.

27. Runtime owns cache retention.

28. Storage owns durable persistence.

29. Shared input is accessed via lease/reference.

30. Candidate rejection triggers cleanup.

31. Late output cannot regain authority.

32. Same semantic input/config should produce equivalent normalized structure where Provider behavior permits.

33. Recognition remains usable outside an active Reading Session.

34. Detection, Recognition, Direction, Layout, Postprocessing, Quality and Reading Order semantics are never redefined inside this module specification.

---

# 43. Open Architecture Decisions

Implementation-dependent decisions still requiring prototype/benchmark evidence include:

* first OCR Provider
* local vs remote default
* combined vs composed execution
* vertical-Chinese quality requirement
* default OCR/Recognition Profile
* Provider-specific image preparation
* partial Candidate exposure
* timeout defaults
* concurrency limits
* confidence normalization strategy
* long-page processing strategy
* GPU model-loading policy
* Traditional Chinese MVP level
* polygon geometry timing

These decisions must not alter the module ownership boundaries defined above.

---

# 44. Related Documents

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

01-architecture/runtime/RESOURCE_LIFECYCLE.md
01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/CACHE_POLICY.md

02-modules/recognition/CONTRACT.md
02-modules/recognition/STATES.md
02-modules/recognition/EVENTS.md
02-modules/recognition/ERRORS.md
02-modules/recognition/README.md

02-modules/text-processing/
```

---

# 45. Summary

Recognition transforms immutable image input into a module-level candidate for structured source content.

```text
Image Artifact
      ↓
Runtime Recognition Attempt
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

Core boundaries:

```text
OCR Architecture
    owns OCR semantics.

Recognition Module
    owns module orchestration,
    Candidate Artifact,
    module compatibility,
    warnings/errors
    and Runtime integration.

Runtime
    owns WorkItem,
    Attempt,
    authority,
    cancellation,
    retry
    and publication decisions.

Provider Integration
    owns Provider abstraction
    and Adapter boundaries.

Artifact Store
    owns published shared payload lifecycle.

Text Processing
    owns semantic preparation
    after Recognition.
```
