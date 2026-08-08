# Text Processing Module Specification

> **Project:** CRAI
> **Module:** Text Processing
> **Path:** `02-modules/text-processing/MODULE.md`
> **Version:** 1.0
> **Status:** Architecture Draft
> **Primary Output:** `SourceDocument`

---

# 1. Module Definition

Text Processing là Core Business Processing Module chịu trách nhiệm chuyển structured source content từ Recognition thành một provider-independent `SourceDocument`.

Primary transformation:

```text
RecognitionArtifact
        ↓
Text Processing
        ↓
Candidate SourceDocument Artifact
        ↓
Runtime Authority Validation
        ↓
Published SourceDocument Artifact
```

Text Processing tập trung vào:

```text
recognized source structure
        ↓
readable source structure
```

Nó không thực hiện Translation.

---

# 2. Architectural Decision

Text Processing tạo:

```text
SourceDocument
```

Text Processing không tạo:

```text
TranslationUnit[]
```

Translation Units phụ thuộc các concern thuộc Translation Module như:

* target language
* provider/model capabilities
* token limits
* context window
* prompt strategy
* translation profile
* batching
* cost policy
* latency policy
* retry scope
* provider constraints

Boundary:

```text
Recognition
    ↓
Text Processing
    ↓
SourceDocument
    ↓
Translation
    ↓
Presentation
```

`SourceDocument` phải giữ ổn định khi Translation Provider hoặc strategy thay đổi.

---

# 3. Module Identity

```text
Module ID:
    text-processing

Module Type:
    Core Business Processing Module

Primary Domain:
    Structured Source Reconstruction

Execution Model:
    Runtime WorkItem / Attempt

Primary Input:
    Recognition Artifact Reference

Primary Candidate Output:
    Candidate SourceDocument Artifact

Published Output:
    SourceDocument Artifact

Execution Authority:
    Runtime
```

---

# 4. Purpose

Text Processing trả lời:

```text
What is the reconstructed,
normalized and traceable structure
of the recognized source content?
```

Nó không trả lời:

```text
How should this content be translated?
```

---

# 5. Responsibilities

Text Processing sở hữu:

* module-level input validation
* Recognition Artifact adaptation
* source-text normalization
* line reconstruction
* source grouping
* structural block classification
* SourceDocument construction
* source traceability
* source-block sequencing
* reversible exclusion decisions
* processing profile semantics
* processing rule semantics
* module warnings/errors
* semantic compatibility
* Candidate SourceDocument assembly
* module diagnostics

---

# 6. Non-Responsibilities

Text Processing không sở hữu:

* OCR
* Detection
* Recognition semantics
* canonical Reading Order
* OCR Quality
* source capture
* Reading Session lifecycle
* WorkItem lifecycle
* Attempt lifecycle
* Scheduler
* Work Queue
* retry execution
* cancellation authority
* Artifact publication
* Artifact retention
* global cache lifecycle
* persistent storage
* Translation planning
* Translation Units
* Translation Provider
* prompt design
* translated text
* Presentation layout
* UI rendering

---

# 7. Architecture Position

```text
Recognition Artifact
      ↓
Runtime Text Processing Attempt
      ↓
Text Processing Module
      ↓
Candidate SourceDocument Artifact
      ↓
Runtime Validation
      ↓
Published SourceDocument Artifact
      ↓
Translation
```

Text Processing không trực tiếp trigger Translation implementation.

Runtime/Business Pipeline Orchestration quyết định downstream execution.

---

# 8. Upstream Contract

Primary upstream:

```text
Recognition
```

Text Processing consume:

```text
RecognitionArtifact
```

Recognition Artifact có thể reference:

```text
OCRDocumentRef
ReadingOrderResultRef?
QualityReportRef?
```

Text Processing không quay lại Provider-native output.

---

# 9. Recognition Boundary

Text Processing không consume trực tiếp legacy:

```text
RecognitionResult
regions[]
lines[]
provider-native OCR response
```

như public module boundary.

Thay vào đó:

```text
RecognitionArtifact
      ↓
resolve canonical OCR references
      ↓
ProcessingInputDocument
```

---

# 10. Canonical OCR Inputs

Text Processing có thể consume canonical concepts từ OCR Architecture:

* OCR Document
* recognized text hierarchy
* Region references
* geometry references
* Layout references
* Direction metadata
* Reading Order Result
* Quality Report

Nó không redefine các semantics đó.

---

# 11. Reading Order Boundary

Canonical Reading Order thuộc:

```text
01-architecture/ocr/READING_ORDER.md
```

Text Processing không tự:

* resolve page reading order từ đầu
* infer panel order
* infer bubble order
* redefine ReadingDirection
* create competing ReadingOrder graph

Text Processing có thể derive:

```text
Source Block Sequence
```

từ:

```text
ReadingOrderResult
+
Source reconstruction decisions
```

---

# 12. Source Block Sequence

`SourceBlockSequence` là thứ tự các `SourceBlock` trong `SourceDocument`.

Nó không phải OCR Reading Order.

Ví dụ:

```text
OCR Reading Order
    Region 1
    Region 2
    Region 3

Text Processing Reconstruction
    Region 1 + Region 2
        → Paragraph Block A

    Region 3
        → Annotation Block B

Source Block Sequence
    A
    B
```

Text Processing sở hữu mapping này vì nó phát sinh sau reconstruction.

---

# 13. High-Level Processing Flow

```text
RecognitionArtifact
      ↓
1. Validate Module Input
      ↓
2. Resolve Canonical OCR References
      ↓
3. Adapt Processing Input
      ↓
4. Resolve Processing Profile
      ↓
5. Normalize Source Text
      ↓
6. Reconstruct Lines / Text Groups
      ↓
7. Group Source Structures
      ↓
8. Classify Source Blocks
      ↓
9. Build SourceDocument
      ↓
10. Validate Traceability
      ↓
11. Assemble Candidate Artifact
```

---

# 14. Internal Components

Recommended components:

```text
TextProcessingCoordinator

InputValidator

RecognitionArtifactAdapter

ProcessingProfileResolver

TextNormalizer

LineReconstructor

SourceGrouper

BlockClassifier

SourceBlockSequencer

SourceDocumentBuilder

TraceabilityValidator

ProcessingRuleRegistry

CandidateAssembler
```

Infrastructure concerns remain external.

---

# 15. Text Processing Coordinator

`TextProcessingCoordinator` coordinates module semantics inside a Runtime Attempt.

Responsibilities:

* validate Attempt input
* resolve Processing Profile
* adapt Recognition Artifact
* execute reconstruction pipeline
* check Runtime Cancellation Context
* normalize module errors
* assemble Candidate
* return Attempt output

It does not:

* create WorkItem
* cancel WorkItem
* guarantee Runtime terminal outcome
* publish Artifact
* own Request registry
* retry itself

---

# 16. Text Processing Attempt Input

Conceptual contract:

```text
TextProcessingAttemptInput
├── RuntimeContext
├── RecognitionArtifactRef
├── ProcessingProfile
├── ProcessingOptions
├── SourceContext?
├── ExecutionContextRef
├── CancellationContextRef
├── PrivacyContextRef
└── TraceContext
```

Exact public shape belongs in `CONTRACT.md`.

---

# 17. Runtime Context

Runtime identity may include:

```text
SessionId?
RevisionId
WorkItemId
AttemptId
ConfigurationSnapshotId
```

Text Processing preserves these for traceability.

It does not own their lifecycle.

---

# 18. Processing Profile

Recommended profiles:

```text
COMIC_PAGE
COMIC_REGION
NOVEL_PAGE
NOVEL_PARAGRAPH
WEB_TEXT
INTERFACE_TEXT
GENERIC_DOCUMENT
```

Profile describes source reconstruction policy.

---

# 19. Processing Profile Contents

Conceptually:

```text
ProcessingProfile
├── ProfileId
├── ProfileVersion
├── NormalizationRules
├── LineReconstructionRules
├── GroupingRules
├── ClassificationRules
├── ExclusionRules
├── BlockConstructionRules
└── ExtensionSettings
```

Not included:

```text
TranslationProvider
TargetLanguage
TokenLimit
PromptTemplate
TranslationTemperature
TranslationBatchSize
TranslationPricing
```

---

# 20. Profile Resolution

Resolution priority may be:

```text
1. Explicit request profile
2. Runtime/session-provided profile
3. Source-type hint
4. Document-type hint
5. Generic fallback
```

Inference must be recorded.

Profile is immutable for one Attempt.

---

# 21. Recognition Artifact Adapter

The adapter converts canonical Recognition output into an internal reconstruction model.

Responsibilities:

* validate Recognition Artifact compatibility
* resolve OCRDocumentRef
* resolve optional ReadingOrderResultRef
* resolve optional QualityReportRef
* preserve source identity
* preserve raw recognized text
* preserve OCR entity references
* preserve geometry references
* preserve upstream warnings
* build internal processing nodes

---

# 22. Adapter Boundary

Adapter must not:

* redefine OCR Region validity
* re-run OCR
* reinterpret Provider SDK fields
* silently repair invalid OCR hierarchy
* overwrite raw recognized text

If referenced OCR artifact is invalid:

```text
return normalized module error
```

rather than rebuild OCR semantics.

---

# 23. Processing Input Document

Internal working representation:

```text
ProcessingInputDocument
├── SourceIdentity
├── RecognitionArtifactRef
├── OCRDocumentRef
├── ReadingOrderResultRef?
├── QualityReportRef?
├── TextNodes[]
├── StructuralHints[]
├── GeometryRefs[]
├── LanguageHints[]
├── DirectionHints[]
├── UpstreamWarnings[]
└── Metadata
```

This is internal and mutable within one Attempt.

It is not public output.

---

# 24. Text Node

Conceptual working node:

```text
TextNode
├── NodeId
├── OCRSourceRefs[]
├── RawText
├── NormalizedText?
├── GeometryRefs[]
├── DirectionRef?
├── LayoutRef?
├── SequenceEvidence?
├── LanguageHint?
├── Warnings[]
└── Metadata
```

---

# 25. Text Normalization

`TextNormalizer` performs deterministic surface normalization.

Responsibilities may include:

* Unicode normalization
* control-character removal
* whitespace normalization
* line-separator normalization
* full-width/half-width normalization
* language-safe punctuation spacing
* preservation of meaningful symbols

---

# 26. Normalization Boundary

Normalization must not become semantic rewriting.

Allowed:

```text
surface normalization
```

Not allowed by default:

```text
fix OCR-confused characters

invent punctuation

rewrite words

change meaning
```

Aggressive semantic correction does not belong in MVP default flow.

---

# 27. Raw Text Preservation

Every normalized textual entity must preserve:

```text
RawText
NormalizedText
NormalizationChanges[]
```

Raw OCR text is never overwritten.

---

# 28. Normalization Safety

Recommended levels:

```text
SAFE
CONSERVATIVE
```

Optional future:

```text
AGGRESSIVE
```

`AGGRESSIVE` requires explicit architecture/policy decision because it may change meaning.

---

# 29. Normalization Rule

Conceptually:

```text
NormalizationRule
├── RuleId
├── RuleVersion
├── SupportedProfiles
├── Preconditions
├── Decision
└── ChangeDescription
```

Rules must be:

* deterministic
* independently testable
* versioned

---

# 30. Line Reconstruction

`LineReconstructor` determines when recognized text fragments belong to one reconstructed textual unit.

Possible actions:

* preserve independent line
* join wrapped lines
* join OCR-fragmented line
* preserve vertical columns
* detect continuation
* detect paragraph boundary

---

# 31. Reconstructed Text Group

```text
ReconstructedTextGroup
├── GroupId
├── OCRSourceRefs[]
├── RawText
├── NormalizedText
├── GeometryRefs[]
├── ReconstructionDecisions[]
├── Evidence[]
├── Confidence?
└── Warnings[]
```

Confidence here belongs to Text Processing reconstruction decision only.

It does not replace OCR confidence.

---

# 32. Reconstruction Evidence

Possible evidence:

```text
SameOCRContainer

SameRecognitionRegion

AlignedGeometry

SmallGap

CompatibleDirection

CanonicalSequence

SentenceContinuation

PunctuationContinuation

SharedColumn

SharedContainerHint

ManualHint
```

Each non-trivial join should have explicit evidence.

---

# 33. Reconstruction Safety

When uncertain:

```text
preserve separate structures
```

rather than destructively merge.

Principle:

```text
under-merge
    is safer than
over-merge
```

---

# 34. Source Grouping

`SourceGrouper` groups reconstructed text structures into logical source groups.

Examples:

* one dialogue block
* one paragraph
* one narration block
* one caption
* one annotation
* one sound effect

---

# 35. Grouping Inputs

Grouping may use:

* reconstructed text groups
* canonical Layout references
* OCR container hints
* geometry relationships
* Direction metadata
* canonical Reading Order
* Processing Profile
* source-specific deterministic rules

---

# 36. Grouping Boundary

Text Processing may group OCR entities.

It does not detect new image-level structures.

Example:

```text
Bubble detection
    → OCR Layout / Detection owner

Bubble-based source grouping
    → Text Processing
```

---

# 37. Source Group

```text
SourceGroup
├── GroupId
├── OCRSourceRefs[]
├── ReconstructedGroupRefs[]
├── RawText
├── NormalizedText
├── GeometryRefs[]
├── GroupingDecisions[]
├── ClassificationHints[]
├── Confidence?
└── Warnings[]
```

Internal only.

---

# 38. Grouping Strategies

Possible:

```text
REGION_PRESERVING

LAYOUT_CONTAINER_BASED

PARAGRAPH_BASED

COLUMN_BASED

GEOMETRY_ASSISTED

HYBRID
```

`REGION_PRESERVING` is the conservative fallback.

---

# 39. Geometry-Assisted Grouping

Geometry may support decisions via:

* proximity
* alignment
* overlap
* containment
* gap
* orientation

Geometry alone does not prove semantic relationship.

---

# 40. Block Classification

`BlockClassifier` assigns structural source roles.

Possible block types:

```text
PARAGRAPH
DIALOGUE
NARRATION
THOUGHT
CAPTION
HEADING
SOUND_EFFECT
ANNOTATION
INTERFACE_TEXT
PAGE_NUMBER
WATERMARK
UNKNOWN
```

Optional structural types:

```text
PAGE
PANEL
SECTION
CONTAINER
```

---

# 41. Classification Boundary

Classification describes source structure.

It does not:

* determine translated style
* identify speaker without evidence
* infer story meaning
* rewrite source text
* choose translation prompt
* choose translation batch

---

# 42. Classification Result

```text
BlockClassification
├── BlockType
├── Confidence?
├── Method
├── Evidence[]
└── Alternatives[]
```

Low-confidence classification should use:

```text
UNKNOWN
```

or explicit alternatives.

---

# 43. SourceDocument

`SourceDocument` is the canonical output of Text Processing semantics.

Conceptually:

```text
SourceDocument
├── DocumentId
├── DocumentVersion
├── DocumentType
├── SourceIdentity
├── RecognitionArtifactRef
├── RootBlockIds[]
├── Blocks[]
├── BlockSequence[]
├── ExcludedBlocks[]
├── LanguageHints[]
├── Warnings[]
├── CompatibilityMetadata
└── Metadata
```

---

# 44. SourceDocument Ownership

Text Processing is authoritative owner of:

```text
SourceDocument
SourceBlock
BlockSequence
BlockExclusion
Source reconstruction metadata
```

Translation consumes these models.

Translation must not redefine them.

---

# 45. SourceBlock

```text
SourceBlock
├── BlockId
├── ParentBlockId?
├── BlockType
├── RawText
├── NormalizedText
├── ChildBlockIds[]
├── OCRSourceRefs[]
├── GeometryRefs[]
├── SequenceIndex?
├── LanguageHint?
├── ReconstructionConfidence?
├── Warnings[]
└── Metadata
```

---

# 46. OCR Source Reference

A `SourceBlock` preserves explicit lineage to OCR data.

Conceptually:

```text
OCRSourceRef
├── OCRDocumentRef
├── RegionRef?
├── ParagraphRef?
├── LineRef?
├── WordRefs[]?
├── CharacterRefs[]?
└── GeometryRefs[]
```

Exact reference shape belongs in `CONTRACT.md`.

---

# 47. Block Hierarchy

SourceDocument supports:

```text
flat block structure
```

and:

```text
hierarchical block structure
```

Hierarchy is optional.

MVP should allow flat documents.

Do not fabricate hierarchy just to satisfy schema aesthetics.

---

# 48. Synthetic Structural Blocks

Structural blocks such as:

```text
PAGE
PANEL
SECTION
```

may exist without direct text.

They must be marked conceptually as:

```text
Synthetic = true
```

Their lineage derives from children or canonical Layout references.

They must never claim invented source text.

---

# 49. Source Block Sequence

SourceDocument may include:

```text
BlockSequence[]
```

Each entry references a SourceBlock.

Conceptually:

```text
SourceBlockSequenceEntry
├── Index
├── BlockId
├── SourceOrderRefs[]
├── ReconstructionMethod
└── Confidence?
```

This is not a replacement for OCR Reading Order.

---

# 50. Excluded Blocks

Some blocks may be excluded from Translation-oriented downstream processing.

Examples:

* page number
* repeated header
* watermark
* UI noise
* high-confidence OCR garbage
* explicitly excluded content

Excluded source is not silently deleted.

---

# 51. Block Exclusion

```text
BlockExclusion
├── BlockId
├── Reason
├── Confidence?
├── RuleId
├── Reversible
└── Evidence[]
```

Recommended reasons:

```text
LIKELY_PAGE_NUMBER
REPEATED_HEADER
LIKELY_WATERMARK
INTERFACE_NOISE
LOW_CONFIDENCE_GARBAGE
PROFILE_EXCLUDED_TYPE
EXPLICIT_USER_EXCLUSION
```

---

# 52. Exclusion Safety

When uncertain:

```text
keep included
```

rather than silently remove content.

Exclusion should be reversible where practical.

---

# 53. Traceability

Traceability is mandatory.

Required conceptual chain:

```text
SourceDocument
      ↓
SourceBlock
      ↓
OCR Source Reference
      ↓
OCR Document
      ↓
Source Geometry
      ↓
Source Image
```

---

# 54. Text Traceability

Every non-empty:

```text
NormalizedText
```

must derive from one or more:

```text
RawText
```

values.

Text Processing must not invent semantic source text.

---

# 55. Traceability Validator

`TraceabilityValidator` checks:

* SourceBlock IDs unique
* OCR references resolvable
* Geometry references valid
* Parent/child graph acyclic
* BlockSequence references valid
* excluded blocks attributable
* normalized text linked to raw evidence
* source identity consistent

---

# 56. SourceDocument Immutability

Published SourceDocument must be immutable.

If processing changes:

```text
new SourceDocument revision
```

must be created.

No silent mutation.

---

# 57. Candidate SourceDocument Artifact

Text Processing first creates:

```text
CandidateSourceDocumentArtifact
```

Conceptually:

```text
CandidateSourceDocumentArtifact
├── CandidateArtifactId
├── ArtifactType
├── OwnerModule
├── ContractVersion
├── RecognitionArtifactRef
├── SourceDocumentRef / Payload
├── Completeness
├── Warnings[]
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

---

# 58. Candidate Boundary

Candidate is:

* immutable after module validation
* non-authoritative
* non-published
* Runtime-submitted
* cleaned if rejected

Runtime owns acceptance.

Artifact Store owns accepted Artifact lifecycle.

---

# 59. Published SourceDocument Artifact

After Runtime acceptance:

```text
SourceDocumentArtifact
├── ArtifactId
├── ArtifactType
├── ContractVersion
├── RecognitionArtifactRef
├── SourceDocument
├── Completeness
├── Warnings[]
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

---

# 60. Completeness

Recommended:

```text
COMPLETE
PARTIAL
EMPTY_VALID
UNKNOWN
```

---

# 61. Empty SourceDocument

If Recognition contains no processable source text:

```text
SourceDocument
├── Blocks = []
├── RootBlockIds = []
├── BlockSequence = []
└── ExcludedBlocks = []
```

with:

```text
Completeness = EMPTY_VALID
```

This is a successful semantic result.

---

# 62. Partial SourceDocument

Partial output may be allowed when:

* usable source remains
* traceability preserved
* Processing Profile permits degraded output
* omitted content is explicit
* Candidate remains contract-valid

---

# 63. Processing Rules

`ProcessingRuleRegistry` stores versioned deterministic rules.

Categories:

```text
NormalizationRule
LineReconstructionRule
GroupingRule
ClassificationRule
ExclusionRule
BlockConstructionRule
SequenceMappingRule
```

---

# 64. Rule Contract

Conceptually:

```text
ProcessingRule
├── RuleId
├── RuleVersion
├── Priority
├── SupportedProfiles
├── SupportedLanguages
├── Preconditions
├── Decision
├── Evidence
└── Confidence?
```

---

# 65. Rule Ordering

Deterministic ordering:

```text
Priority
    ↓
RuleId
    ↓
RuleVersion
```

Conflict resolution must be explicit.

---

# 66. Conflict Strategies

Possible:

```text
HIGHEST_CONFIDENCE

HIGHEST_PRIORITY

CONSERVATIVE_FALLBACK

PRESERVE_SEPARATE

REQUIRE_CONSENSUS
```

For grouping/reconstruction:

```text
PRESERVE_SEPARATE
```

is preferred fallback.

---

# 67. Language-Specific Rules

Language-specific rules may support:

* Chinese spacing
* Japanese punctuation
* English wrapping
* quotation marks
* paragraph indentation
* vertical text surface handling

Unknown language must remain processable.

---

# 68. Source Profiles

## COMIC_PAGE

Typical reconstruction:

```text
Recognition Artifact
      ↓
Canonical OCR / Reading Order
      ↓
Normalize
      ↓
Reconstruct Text Groups
      ↓
Group by Layout / Container Evidence
      ↓
Classify Source Blocks
      ↓
Build SourceDocument
```

---

# 69. COMIC_REGION

For selected region:

```text
Recognition Artifact
      ↓
Normalize
      ↓
Reconstruct Local Text
      ↓
Single / Few Source Blocks
      ↓
SourceDocument
```

No page-order re-analysis required.

---

# 70. NOVEL_PAGE

Typical:

```text
Recognition Artifact
      ↓
Canonical OCR sequence/layout
      ↓
Normalize
      ↓
Wrapped-Line Reconstruction
      ↓
Paragraph Grouping
      ↓
Heading / Paragraph Classification
      ↓
SourceDocument
```

---

# 71. GENERIC_DOCUMENT

Conservative flow:

```text
Recognition Artifact
      ↓
Normalize
      ↓
Minimal Reconstruction
      ↓
Conservative Grouping
      ↓
Generic Classification
      ↓
SourceDocument
```

---

# 72. Structured Text Future Path

Future Text Processing may consume non-OCR structured source adapters.

Possible:

```text
DOMTextArtifact
EPUBTextArtifact
PDFTextLayerArtifact
PlainTextArtifact
AccessibilityTreeArtifact
```

Each adapter should normalize into a stable internal reconstruction model.

This does not change Translation boundary.

---

# 73. Runtime Boundary

Text Processing executes inside Runtime Attempt.

```text
Runtime creates WorkItem
      ↓
Attempt admitted
      ↓
Text Processing executes
      ↓
Candidate SourceDocument Artifact
      ↓
Attempt Completion
      ↓
Runtime Authority Validation
      ↓
Artifact Store Publication
```

Text Processing does not own:

* WorkItem
* Attempt
* authority
* retry
* publication
* terminal outcome

---

# 74. Cancellation

Text Processing consumes Runtime-provided:

```text
CancellationContext
```

Recommended checkpoints:

* before input resolution
* before normalization
* between bounded reconstruction batches
* before SourceDocument assembly
* before Candidate assembly
* before Candidate submission

Text Processing does not register a global cancellation system.

---

# 75. Cancellation Boundary

Text Processing does not directly react to:

```text
SessionStopped
SourceClosed
ApplicationShutdown
SupersededFrame
```

through hidden subscriptions.

Runtime converts those external facts into cancellation/authority context.

---

# 76. Deadline

Deadline belongs to Runtime Execution Context.

Text Processing may observe remaining budget.

It does not own:

* timeout lifecycle
* terminal timeout outcome
* retry policy

---

# 77. Retry Boundary

Text Processing may return:

```text
RetryHint
```

Examples:

```text
SAME_PROFILE
CONSERVATIVE_PROFILE
SOURCE_PRESERVING_FALLBACK
RESOURCE_WAIT
NO_RETRY
```

Runtime decides whether retry occurs.

---

# 78. State Ownership

Text Processing owns only semantic/local states such as:

```text
Module Availability
Processing Plan State
Operation Phase
Candidate Validation State
SourceDocument Completeness
```

Detailed state contract belongs in:

```text
STATES.md
```

---

# 79. Operation Phase

Recommended diagnostic phases:

```text
VALIDATING

ADAPTING_INPUT

NORMALIZING

RECONSTRUCTING

GROUPING

CLASSIFYING

BUILDING_DOCUMENT

VALIDATING_TRACEABILITY

ASSEMBLING_CANDIDATE

FINALIZING
```

These are not Runtime Attempt states.

---

# 80. No Request Registry

Text Processing should not own:

```text
active processing request registry
```

as a business state source.

Attempt-local execution state belongs to Runtime worker/execution context.

---

# 81. Events Boundary

Text Processing events are optional module facts.

Possible:

```text
TEXT_PROCESSING_PLAN_CREATED

TEXT_PROCESSING_DOCUMENT_BUILT

TEXT_PROCESSING_CANDIDATE_VALIDATED

TEXT_PROCESSING_WARNING_RECORDED

TEXT_PROCESSING_MODULE_ERROR_RECORDED
```

Text Processing does not publish terminal aliases:

```text
text_processing.completed
text_processing.failed
text_processing.cancelled
```

as authoritative Runtime lifecycle facts.

---

# 82. Event Consumption Boundary

Text Processing should not directly consume broad workflow events like:

```text
recognition.completed
session.stopped
source.closed
application.shutdown_requested
```

Runtime/Business Orchestration provides explicit Attempt Input.

---

# 83. Error Boundary

Text Processing owns module-level errors such as:

```text
TEXT_PROCESSING_INPUT_INVALID

TEXT_PROCESSING_PROFILE_INVALID

TEXT_PROCESSING_SOURCE_ADAPTATION_FAILED

TEXT_PROCESSING_NORMALIZATION_FAILED

TEXT_PROCESSING_RECONSTRUCTION_FAILED

TEXT_PROCESSING_GROUPING_FAILED

TEXT_PROCESSING_CLASSIFICATION_FAILED

TEXT_PROCESSING_DOCUMENT_BUILD_FAILED

TEXT_PROCESSING_TRACEABILITY_FAILED

TEXT_PROCESSING_CANDIDATE_INVALID

TEXT_PROCESSING_INTERNAL_ERROR
```

Detailed error contract belongs in `ERRORS.md`.

---

# 84. Recoverable Degradation

Safe degradation examples:

```text
classification uncertain
    → BlockType = UNKNOWN
```

```text
grouping uncertain
    → preserve separate blocks
```

```text
hierarchy uncertain
    → flat structure
```

```text
exclusion uncertain
    → keep block included
```

Recovery must preserve evidence and traceability.

---

# 85. Non-Recoverable Conditions

Fail module processing when:

* upstream Artifact incompatible
* Source identity contradictory
* required references cannot resolve
* traceability impossible
* SourceDocument hierarchy cyclic
* normalized text lacks source evidence
* required Processing Profile unsupported
* Candidate contract invalid
* privacy boundary violated

---

# 86. Semantic Compatibility

Text Processing defines when SourceDocument output is semantically compatible.

Possible dependencies:

```text
RecognitionArtifact ContentIdentity

SourceDocument Contract Version

Processing Profile Version

Processing Rule Set Version

Normalization Policy Version

Reconstruction Policy Version

Grouping Policy Version

Classification Policy Version

Privacy Partition
```

---

# 87. Compatibility vs Cache

Text Processing owns:

```text
semantic compatibility
```

Runtime Cache Policy owns:

```text
whether reuse occurs
```

Artifact Store owns:

```text
shared runtime Artifact lifecycle
```

Storage owns:

```text
durable persistence
```

---

# 88. Processing Fingerprint

A deterministic fingerprint may derive from:

```text
Recognition Artifact semantic identity
+
Processing Profile version
+
Processing Options
+
Rule Set versions
```

It may support compatibility/reuse checks.

It is not itself cache policy.

---

# 89. Determinism

Given equivalent:

```text
Recognition semantic input
+
Processing Profile
+
Processing Options
+
Rule versions
```

Text Processing should create structurally equivalent `SourceDocument`.

Runtime-only timestamps/IDs may differ.

---

# 90. Concurrency

Safe local parallelism may include:

* normalizing independent nodes
* computing grouping evidence
* classifying independent groups
* computing traceability metadata

Ordering-sensitive decisions must remain deterministic.

Parallel execution must not alter final SourceDocument semantics.

---

# 91. Stateless Processor Preference

Core processors should be stateless:

```text
TextNormalizer

LineReconstructor

SourceGrouper

BlockClassifier

TraceabilityValidator
```

Attempt-local mutable data belongs in:

```text
ProcessingExecutionContext
```

---

# 92. Processing Execution Context

Conceptually:

```text
ProcessingExecutionContext
├── AttemptInput
├── ProcessingProfile
├── InputDocument
├── WorkingNodes
├── ReconstructionDecisions
├── Warnings
├── Diagnostics
├── RuntimeContexts
└── OperationPhase
```

This context:

* is Attempt-local
* is not shared
* is not persisted as domain state

---

# 93. Resource Lifecycle

Text Processing should mainly operate on:

```text
text
IDs
small structural metadata
Artifact references
geometry references
```

It should not retain:

```text
full image buffers
OCR model buffers
provider responses
unbounded text diagnostics
```

---

# 94. Resource Flow

```text
Acquire Recognition Artifact Lease
      ↓
Resolve OCR References
      ↓
Build Attempt-Local Structures
      ↓
Build Candidate SourceDocument
      ↓
Transfer or Cleanup
      ↓
Release Attempt-Local Resources
      ↓
Release Leases
```

Physical resource lifecycle belongs to Runtime/Resource Manager.

---

# 95. Privacy

Text Processing handles source text and therefore privacy is material.

Rules:

1. Full OCR/source text not logged by default.
2. Full normalized text not logged by default.
3. Translation text does not exist in this module.
4. Diagnostics containing text require explicit policy.
5. Artifact references preserve Privacy Partition.
6. No Provider credentials are present.
7. No image bytes appear in module events.
8. Raw and normalized source text remain protected Artifact content.

---

# 96. Observability

Useful module measurements:

```text
text_processing.total_ms

text_processing.normalization_ms

text_processing.reconstruction_ms

text_processing.grouping_ms

text_processing.classification_ms

text_processing.document_build_ms

text_processing.traceability_ms

text_processing.input_node_count

text_processing.output_block_count

text_processing.excluded_block_count

text_processing.warning_count
```

Detailed telemetry transport belongs to Infrastructure.

---

# 97. Diagnostics

Diagnostics may record:

* normalization decisions
* reconstruction evidence
* grouping evidence
* classification alternatives
* exclusion decisions
* traceability failure
* Processing Profile identity
* rule versions

Diagnostics should be reference-based and content-safe by default.

---

# 98. Source Correction Boundary

User corrections should not mutate published SourceDocument.

Recommended concept:

```text
SourceDocument
      +
SourceCorrectionSet
      ↓
CorrectedSourceDocumentView
```

Correction ownership belongs to a separate concern/module.

---

# 99. Translation Boundary

Translation consumes:

```text
SourceDocument Artifact
+
Translation Request
```

Translation then owns:

```text
Translation Plan
Translation Unit
Provider Selection
Prompt Strategy
Context Strategy
Target Language
Translated Result Assembly
```

Text Processing never imports Translation Planner implementation.

---

# 100. SourceDocument Stability

SourceDocument semantics must remain stable when:

* Translation provider changes
* model changes
* token limits change
* target language changes
* prompt strategy changes
* context strategy changes
* pricing changes
* batching changes

This is the principal architecture reason for separating Text Processing from Translation.

---

# 101. Data Ownership

Text Processing owns:

```text
SourceDocument

SourceBlock

SourceBlockSequence

BlockExclusion

NormalizationChange

ReconstructionDecision

GroupingDecision

BlockClassification

Processing Profile

Processing Rule

Candidate SourceDocument Artifact

Text Processing warnings/errors

semantic compatibility
```

Text Processing does not own:

```text
OCR Document

OCR Reading Order

OCR Quality

Runtime authority

WorkItem / Attempt

Artifact retention

Translation Unit

Translated Result

Presentation layout
```

---

# 102. Dependencies

Allowed categories:

```text
shared-kernel

runtime-contracts

artifact-contracts

recognition-contracts

ocr-artifact-contracts

geometry-primitives

configuration-contracts

security-contracts

diagnostics-contracts
```

---

# 103. Forbidden Direct Dependencies

```text
OCR Provider SDK

Translation Provider SDK

LLM SDK

Recognition implementation

Translation implementation

Presentation implementation

Desktop UI

Browser Extension

Capture implementation

Session implementation

Scheduler implementation

Storage implementation
```

---

# 104. Testing Focus

MODULE-level tests should focus on Text Processing-owned semantics:

* profile resolution
* normalization
* reconstruction
* grouping
* classification
* SourceDocument construction
* SourceBlock sequencing
* exclusion
* traceability
* compatibility
* Candidate assembly
* privacy invariants

Runtime lifecycle tests belong to Runtime integration tests.

---

# 105. Unit Test Examples

## Normalization

* Chinese whitespace
* Unicode normalization
* control characters
* punctuation preservation
* idempotency
* raw text preservation

## Reconstruction

* wrapped lines
* vertical text fragments
* unrelated adjacent lines
* uncertain join
* conservative separation

## Grouping

* one logical paragraph split across OCR nodes
* nearby unrelated groups
* container-based grouping
* selected-region preservation
* ambiguous grouping

## Classification

* dialogue
* narration
* heading
* page number
* sound effect
* unknown fallback

---

# 106. SourceDocument Tests

Test:

* flat document
* hierarchical document
* empty document
* partial document
* excluded blocks
* stable block sequence
* unique IDs
* acyclic hierarchy
* synthetic block traceability

---

# 107. Property Tests

```text
every textual SourceBlock
has raw-source evidence
```

```text
every BlockSequence entry
references an existing SourceBlock
```

```text
SourceBlock hierarchy is acyclic
```

```text
normalization is idempotent
under same rule version
```

```text
same semantic input and profile
produce equivalent SourceDocument
```

```text
no Translation-specific field
exists in SourceDocument
```

---

# 108. Runtime Integration Tests

```text
Recognition Artifact
      ↓
Text Processing Attempt
      ↓
Candidate SourceDocument
      ↓
Runtime Publication
```

```text
old Revision Candidate
      ↓
Runtime rejects stale
```

```text
cancellation during processing
      ↓
no Candidate publication
```

```text
Candidate valid
but Runtime authority lost
      ↓
Candidate rejected
```

---

# 109. MVP Scope

Required MVP:

```text
RecognitionArtifactAdapter

ProcessingProfileResolver

TextNormalizer

LineReconstructor

SourceGrouper

BasicBlockClassifier

SourceDocumentBuilder

TraceabilityValidator

CandidateAssembler
```

---

# 110. MVP Profiles

Required:

```text
COMIC_PAGE

COMIC_REGION

NOVEL_PAGE

GENERIC_DOCUMENT
```

---

# 111. MVP Block Types

Required:

```text
PARAGRAPH

DIALOGUE

NARRATION

CAPTION

HEADING

SOUND_EFFECT

ANNOTATION

PAGE_NUMBER

UNKNOWN
```

Optional:

```text
PAGE

PANEL

SECTION
```

---

# 112. MVP Simplifications

MVP may:

* produce flat SourceDocument
* derive block sequence from canonical OCR Reading Order
* use rule-based classification
* avoid panel reconstruction
* avoid semantic punctuation correction
* avoid cross-page reconstruction
* preserve OCR grouping when uncertain
* classify uncertain source as `UNKNOWN`
* treat selected region as one root block
* store Geometry references rather than merged geometry

---

# 113. Recommended MVP Flow

```text
Recognition Artifact
      ↓
Validate
      ↓
Resolve OCR Document / Reading Order
      ↓
Normalize Text
      ↓
Reconstruct Text Groups
      ↓
Group Source Structures
      ↓
Classify Blocks
      ↓
Build Flat SourceDocument
      ↓
Build Source Block Sequence
      ↓
Validate Traceability
      ↓
Candidate SourceDocument Artifact
```

---

# 114. Deferred Extensions

Possible future additions:

```text
StructuredTextAdapter

DOMTextAdapter

EPUBAdapter

PDFTextLayerAdapter

LanguageDetector

AdvancedNoiseClassifier

SemanticBoundaryDetector

IncrementalDocumentUpdater

CrossPageDocumentMerger

UserCorrectionIntegrator

DocumentDiffEngine
```

Add only when concrete requirements exist.

---

# 115. Open Architecture Decisions

Still open:

* multi-page SourceDocument support
* exact SourceDocument Artifact representation
* flat vs hierarchy as default
* merged geometry vs refs
* language detection ownership
* page-number exclusion default
* sound-effect translation default
* cross-page reconstruction
* correction representation
* processing fingerprint algorithm
* raw-text transport privacy views
* DOM/EPUB adapter timing

These decisions must not change the ownership boundaries above.

---

# 116. Architecture Invariants

1. Text Processing produces `SourceDocument`.

2. Text Processing does not produce `TranslationUnit`.

3. Recognition Artifact is immutable input.

4. Raw recognized text is preserved.

5. Normalized text is stored separately.

6. Text Processing does not re-run OCR.

7. Text Processing does not redefine OCR Region semantics.

8. Text Processing does not redefine OCR Reading Order.

9. Text Processing may derive SourceBlock sequence from canonical Reading Order.

10. Text Processing does not redefine OCR Quality.

11. Every textual SourceBlock is traceable to OCR source evidence.

12. Geometry remains source-traceable.

13. Published SourceDocument is immutable.

14. Processing Profile is versioned.

15. Processing Rules are versioned.

16. Processing is deterministic by default.

17. Empty text is valid success.

18. Partial output is explicit.

19. Uncertainty is explicit.

20. Classification is non-destructive.

21. Exclusion is reversible where practical.

22. Uncertain grouping preserves separate structures.

23. Translation concerns do not enter Text Processing.

24. Translation Provider concerns do not enter SourceDocument.

25. Runtime owns WorkItem and Attempt lifecycle.

26. Runtime owns retry.

27. Runtime owns cancellation authority.

28. Runtime owns acceptance authority.

29. Text Processing creates Candidate only.

30. Artifact Store owns accepted Artifact lifecycle.

31. Text Processing does not publish authoritative Artifact.

32. Text Processing does not subscribe to broad workflow events for hidden orchestration.

33. Text Processing events are optional for correctness.

34. Text Processing does not maintain its own global request registry.

35. Attempt-local processing state is ephemeral.

36. Source text is not logged by default.

37. Candidate rejection triggers cleanup.

38. Semantic compatibility is explicit.

39. Cache retention belongs to Runtime.

40. Durable persistence belongs to Storage.

41. User correction does not mutate original SourceDocument.

42. SourceDocument remains stable across Translation provider/model changes.

---

# 117. Related Documents

```text
02-modules/text-processing/README.md
02-modules/text-processing/MODULE.md
02-modules/text-processing/CONTRACT.md
02-modules/text-processing/STATES.md
02-modules/text-processing/EVENTS.md
02-modules/text-processing/ERRORS.md

02-modules/recognition/README.md
02-modules/recognition/MODULE.md
02-modules/recognition/CONTRACT.md

01-architecture/ocr/README.md
01-architecture/ocr/POSTPROCESS.md
01-architecture/ocr/QUALITY.md
01-architecture/ocr/READING_ORDER.md

01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/CACHE_POLICY.md
01-architecture/runtime/RESOURCE_LIFECYCLE.md

02-modules/translation/
```

---

# 118. Summary

Text Processing performs:

```text
Recognition Artifact
      ↓
Canonical OCR references
      ↓
Normalize source text
      ↓
Reconstruct textual structure
      ↓
Group source content
      ↓
Classify source blocks
      ↓
Build SourceDocument
      ↓
Build Source Block Sequence
      ↓
Validate traceability
      ↓
Candidate SourceDocument Artifact
```

Ownership:

```text
OCR Architecture
    owns OCR semantics
    and canonical Reading Order.

Recognition
    owns Recognition Artifact.

Text Processing
    owns source reconstruction
    and SourceDocument.

Runtime
    owns execution authority,
    retry and cancellation.

Artifact Store
    owns accepted Artifact lifecycle.

Translation
    owns translation planning
    and translated content.
```

Core rule:

```text
Recognition tells us
what source content was recognized.

Text Processing tells us
how that source content should be represented
as a stable readable document.

Translation decides
how that document should be translated.
```
