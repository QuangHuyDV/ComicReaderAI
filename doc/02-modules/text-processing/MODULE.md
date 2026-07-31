# Text Processing Module Architecture

> **Project:** CRAI
> **Module:** Text Processing
> **Path:** `modules/text-processing/MODULE.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-22

---

## 1. Purpose

This document defines the internal architecture of the Text Processing module.

The module transforms structured Recognition output into a provider-independent `SourceDocument`.

Its primary responsibility is:

```text
RecognitionResult
        ↓
SourceDocument
```

The module reconstructs readable source structure while preserving traceability to the original Recognition regions and image geometry.

It does not decide how content should be batched, prompted, or sent to a translation provider.

---

## 2. Architectural Decision

Text Processing produces:

```text
SourceDocument
```

It does not produce:

```text
TranslationUnit[]
```

Translation units depend on concerns that belong to the Translation module, including:

* provider token limits;
* translation model capabilities;
* prompt strategy;
* context-window strategy;
* pricing;
* latency policy;
* translation profile;
* batching strategy;
* retry scope;
* provider-specific constraints.

The intended architecture is:

```text
Recognition
    ↓
Text Processing
    ↓
SourceDocument
    ↓
Translation
    ├── Translation Planner
    ├── Provider Adapter
    ├── Translation Executor
    └── Result Assembler
    ↓
Presentation
```

This keeps Text Processing independent of Translation providers.

---

## 3. Module Responsibility

Text Processing answers:

```text
What is the reconstructed structure of the recognized source content?
```

It owns:

* Recognition-result validation;
* source-text normalization;
* reading-order refinement;
* line reconstruction;
* region grouping;
* document-block construction;
* source-document assembly;
* source traceability;
* structural confidence;
* processing warnings.

It does not answer:

```text
How should this document be translated by a specific model?
```

---

## 4. Module Boundary

### 4.1 Inputs

Primary input:

```text
TextProcessingRequest
```

containing:

```text
RecognitionResult
ProcessingProfile
ProcessingOptions
ProcessingContext
```

---

### 4.2 Outputs

Primary output:

```text
TextProcessingResult
```

containing:

```text
SourceDocument
ProcessingWarnings
ProcessingMetrics
TraceContext
```

---

### 4.3 Upstream Dependency

Primary upstream module:

```text
Recognition
```

Text Processing consumes:

```text
RecognitionResult
```

It must not depend on:

* OCR provider SDKs;
* OCR model implementations;
* Recognition worker internals;
* image-preprocessing pipelines.

---

### 4.4 Downstream Consumer

Primary downstream module:

```text
Translation
```

Other possible consumers:

* source-text preview;
* diagnostics;
* user correction;
* search;
* export;
* reading history;
* evaluation tooling;
* Presentation for source highlighting.

---

## 5. High-Level Pipeline

```text
TextProcessingRequest
        ↓
RequestValidator
        ↓
RecognitionInputAdapter
        ↓
EffectiveOrderResolver
        ↓
TextNormalizer
        ↓
LineReconstructor
        ↓
RegionGrouper
        ↓
BlockClassifier
        ↓
SourceDocumentBuilder
        ↓
TraceabilityValidator
        ↓
ResultAssembler
        ↓
TextProcessingResult
```

Some stages may be skipped depending on the processing profile and input structure.

---

## 6. Internal Components

The Text Processing module contains the following primary components:

```text
TextProcessingFacade
RequestValidator
RecognitionInputAdapter
ProfileResolver
EffectiveOrderResolver
TextNormalizer
LineReconstructor
RegionGrouper
BlockClassifier
SourceDocumentBuilder
TraceabilityValidator
ResultAssembler
ProcessingRuleRegistry
ProcessingMetricsCollector
ProcessingCancellationCoordinator
```

---

# Public Entry Point

## 7. TextProcessingFacade

`TextProcessingFacade` is the public entry point of the module.

Responsibilities:

* accept processing requests;
* validate request identity;
* resolve processing profile;
* coordinate pipeline execution;
* propagate cancellation;
* normalize module errors;
* return or publish the final result;
* guarantee one terminal outcome.

Conceptual interface:

```text
TextProcessingFacade
├── process(request)
├── cancel(request_id, reason)
├── get_status(request_id)
└── get_capabilities()
```

Possible execution modes:

```text
Synchronous
Asynchronous
```

The domain result must remain equivalent across both modes.

---

## 8. Processing Request

Conceptual request structure:

```text
TextProcessingRequest
├── request_id
├── recognition_result
├── profile
├── options
├── context
├── timeout?
├── priority?
├── previous_processing_id?
└── trace_context
```

The request must reference one immutable Recognition result.

One processing request must not combine unrelated Recognition results unless a future multi-page contract explicitly supports it.

---

## 9. Processing Context

Optional processing context may include:

```text
ProcessingContext
├── session_id?
├── source_id
├── content_id
├── frame_id?
├── expected_language?
├── document_type_hint?
├── reading_direction_hint?
├── page_index?
├── chapter_id?
├── previous_document_reference?
└── privacy_policy
```

Context fields are hints unless the contract explicitly marks them as authoritative.

---

# Validation Layer

## 10. RequestValidator

`RequestValidator` validates the public processing request.

Responsibilities:

* validate contract version;
* validate request ID;
* reject duplicate active request IDs;
* validate timeout;
* validate profile reference;
* validate processing options;
* validate privacy policy;
* validate module availability;
* validate Recognition result presence.

It does not validate every internal Recognition element.

That responsibility belongs to `RecognitionInputAdapter`.

---

## 11. RecognitionInputAdapter

`RecognitionInputAdapter` converts Recognition output into an internal processing model.

Responsibilities:

* validate Recognition contract compatibility;
* copy immutable identifiers;
* validate region and line IDs;
* validate reading-order references;
* validate geometry references;
* normalize provider-specific optional fields;
* preserve raw recognized text;
* construct internal processing nodes;
* preserve Recognition warnings.

Output:

```text
ProcessingInputDocument
```

---

## 12. Processing Input Document

Internal representation:

```text
ProcessingInputDocument
├── recognition_id
├── source_identity
├── regions[]
├── lines[]
├── recognition_order[]
├── coordinate_space
├── source_dimensions
├── language_hints[]
├── orientation_hints[]
├── upstream_warnings[]
└── metadata
```

This is an internal mutable working representation.

It must not be exposed as the final `SourceDocument`.

---

## 13. Invalid Recognition Input

The adapter must reject input when:

* required IDs are missing;
* references point to nonexistent regions;
* line ownership is contradictory;
* reading order contains invalid references;
* geometry is unusable;
* source identity is inconsistent;
* contract major version is unsupported;
* duplicate IDs would break traceability.

Minor recoverable issues may become warnings.

Examples:

```text
MissingOptionalLineGeometry
DuplicateReadingOrderEntry
UnknownOrientation
LowRecognitionConfidence
```

---

# Profile Layer

## 14. ProfileResolver

`ProfileResolver` selects the effective processing profile.

Inputs may include:

* explicit profile request;
* source type;
* document-type hint;
* Recognition layout;
* expected language;
* orientation;
* application defaults.

Recommended profiles:

```text
ComicPage
ComicRegion
NovelPage
NovelParagraph
WebText
InterfaceText
GenericDocument
```

---

## 15. Profile Resolution Priority

Recommended priority:

```text
1. Explicit request profile
2. Session-level profile
3. Source-type mapping
4. Document-type hint
5. Recognition structure inference
6. GenericDocument fallback
```

Profile inference must be recorded.

---

## 16. Processing Profile

Conceptual structure:

```text
ProcessingProfile
├── profile_id
├── profile_version
├── normalization_rules
├── order_rules
├── line_join_rules
├── grouping_rules
├── classification_rules
├── noise_rules
├── block_rules
├── confidence_thresholds
└── extension_settings
```

Profiles must be versioned and immutable during one request.

---

## 17. Profile Independence

A profile describes document reconstruction policy.

It must not contain:

* Translation provider names;
* model token limits;
* prompt templates;
* translation temperature;
* provider pricing;
* translation batch size;
* target-language instructions.

Those belong to Translation.

---

# Ordering Layer

## 18. EffectiveOrderResolver

`EffectiveOrderResolver` produces the effective processing order.

It consumes:

* Recognition-provided order;
* region geometry;
* line geometry;
* orientation;
* reading-direction hints;
* profile rules;
* grouping hints.

Output:

```text
EffectiveReadingOrder
```

---

## 19. Effective Reading Order

Conceptual structure:

```text
EffectiveReadingOrder
├── original_order[]
├── resolved_order[]
├── reading_direction
├── resolution_method
├── confidence
├── changes[]
└── warnings[]
```

The original Recognition order must always remain available.

---

## 20. Order Resolution Policy

The resolver should prefer:

```text
explicit valid Recognition order
```

before applying heuristics.

Heuristics may be used when:

* order is missing;
* references are incomplete;
* order conflicts with orientation;
* profile requires panel or column handling;
* multiple text directions exist.

---

## 21. Comic Order Resolution

Comic order may consider:

* panel order;
* bubble position;
* speech-tail information when available;
* right-to-left page direction;
* top-to-bottom movement;
* vertical text columns;
* region overlap;
* local bubble grouping.

For MVP, panel understanding may remain unavailable.

The resolver must represent uncertainty rather than pretending the order is exact.

---

## 22. Novel Order Resolution

Novel order normally prioritizes:

* page columns;
* vertical or horizontal writing direction;
* paragraph geometry;
* line continuity;
* provider order.

Novel processing should avoid comic-specific bubble heuristics.

---

# Normalization Layer

## 23. TextNormalizer

`TextNormalizer` performs deterministic surface cleanup.

Input:

```text
raw recognized text
```

Output:

```text
normalized text
```

Responsibilities:

* Unicode normalization;
* whitespace normalization;
* control-character removal;
* script-safe punctuation spacing;
* full-width and half-width normalization;
* line-separator normalization;
* preservation of meaningful symbols;
* recording every material normalization category.

---

## 24. Normalization Rule

Conceptual interface:

```text
NormalizationRule
├── rule_id
├── version
├── applies_to(context)
├── normalize(text)
└── describe_changes()
```

Rules should be independently testable.

---

## 25. Normalization Safety Levels

Recommended levels:

```text
Safe
Conservative
Aggressive
```

### Safe

Operations with minimal semantic risk.

Examples:

* trim boundary whitespace;
* remove invalid control characters;
* normalize line separators.

### Conservative

Operations supported by strong structural evidence.

Examples:

* collapse unnecessary spaces in Chinese text;
* normalize spaces before punctuation.

### Aggressive

Potentially semantic corrections.

Examples:

* replacing OCR-confused characters;
* inserting missing punctuation;
* rewriting malformed words.

Aggressive normalization should not be part of MVP default behavior.

---

## 26. Raw Text Preservation

Every normalized node must preserve:

```text
raw_text
normalized_text
normalization_changes[]
```

The module must never overwrite raw Recognition text.

---

# Reconstruction Layer

## 27. LineReconstructor

`LineReconstructor` determines which recognized lines belong together.

Responsibilities:

* preserve independent lines when uncertain;
* join wrapped lines;
* join vertically segmented text;
* detect line continuation;
* detect explicit paragraph breaks;
* produce line groups;
* record joining evidence.

Output:

```text
ReconstructedLineGroup[]
```

---

## 28. Reconstructed Line Group

Conceptual structure:

```text
ReconstructedLineGroup
├── line_group_id
├── recognition_line_ids[]
├── raw_text
├── normalized_text
├── geometry_references[]
├── orientation
├── join_decisions[]
├── confidence
└── warnings[]
```

---

## 29. Line Join Evidence

Possible evidence:

```text
SameRecognitionRegion
AlignedGeometry
SmallLineGap
CompatibleOrientation
ProviderSequence
SentenceContinuation
PunctuationContinuation
SharedColumn
SharedBubbleHint
ManualHint
```

Each nontrivial join should have at least one evidence type.

---

## 30. Line Split Evidence

Lines should remain separate when:

* geometry suggests different blocks;
* orientations conflict;
* paragraph break is likely;
* dialogue speakers differ;
* heading boundaries exist;
* source region ownership differs strongly;
* joining confidence is low.

The module should prefer under-merging to destructive over-merging.

---

## 31. RegionGrouper

`RegionGrouper` combines related Recognition regions into logical source groups.

Responsibilities:

* group fragments from one bubble;
* combine parts of one paragraph;
* preserve independent annotations;
* separate sound effects from dialogue;
* separate headings from body text;
* preserve geometry references;
* avoid merging unrelated neighboring regions.

Output:

```text
SourceGroup[]
```

---

## 32. Source Group

Internal representation:

```text
SourceGroup
├── group_id
├── region_ids[]
├── line_group_ids[]
├── raw_text
├── normalized_text
├── geometry_references[]
├── order_range
├── orientation
├── grouping_decisions[]
├── classification_hints[]
├── confidence
└── warnings[]
```

`SourceGroup` is internal and may later become one or more document blocks.

---

## 33. Grouping Strategies

Possible strategies:

```text
RegionPreserving
GeometryBased
BubbleBased
ParagraphBased
ColumnBased
Hybrid
```

The profile determines which strategy is preferred.

---

## 34. Region-Preserving Strategy

Each Recognition region remains independent.

Useful for:

* uncertain layouts;
* selected-region translation;
* interface text;
* diagnostics;
* conservative MVP fallback.

---

## 35. Geometry-Based Strategy

Groups regions using:

* proximity;
* overlap;
* alignment;
* enclosure;
* orientation;
* gap thresholds.

Geometry alone must not be treated as proof of semantic relationship.

---

## 36. Bubble-Based Strategy

Uses speech-bubble identity when available.

Bubble identity may come from:

* Recognition metadata;
* a future Layout module;
* manual selection;
* deterministic enclosure analysis.

Text Processing should consume bubble metadata but should not become responsible for image-level bubble detection.

---

## 37. Paragraph-Based Strategy

Uses:

* line continuity;
* indentation;
* paragraph spacing;
* sentence flow;
* column alignment;
* repeated layout patterns.

Recommended for novel and document profiles.

---

# Classification Layer

## 38. BlockClassifier

`BlockClassifier` assigns structural block types.

Possible block types:

```text
Document
Page
Panel
Section
Paragraph
Dialogue
Narration
Thought
Caption
Heading
SoundEffect
Annotation
InterfaceText
PageNumber
Watermark
Unknown
```

Classification is structural metadata, not translation.

---

## 39. Classification Inputs

Classification may use:

* Recognition labels;
* geometry;
* source profile;
* text length;
* punctuation;
* orientation;
* enclosure hints;
* position;
* repeated layout;
* neighboring groups.

---

## 40. Classification Output

Conceptual structure:

```text
BlockClassification
├── block_type
├── confidence
├── method
├── evidence[]
└── alternatives[]
```

Low-confidence classification should produce:

```text
Unknown
```

or preserve alternatives.

---

## 41. Classification Limits

The classifier must not:

* determine translated style;
* infer character identity without evidence;
* rewrite text;
* infer story meaning;
* decide provider prompts;
* decide translation batching.

A dialogue classification may help Translation later, but Text Processing only records the structural observation.

---

# Document Construction Layer

## 42. SourceDocumentBuilder

`SourceDocumentBuilder` converts ordered groups into an immutable `SourceDocument`.

Responsibilities:

* assign document and block IDs;
* build block hierarchy;
* preserve effective order;
* attach raw and normalized text;
* attach geometry references;
* attach source references;
* attach confidence and warnings;
* produce deterministic structure.

---

## 43. SourceDocument

Conceptual model:

```text
SourceDocument
├── document_id
├── document_type
├── source_identity
├── recognition_id
├── language_hints[]
├── reading_direction
├── root_blocks[]
├── reading_order[]
├── excluded_blocks[]
├── document_metadata
├── confidence
├── warnings[]
└── version
```

---

## 44. Source Identity

```text
SourceIdentity
├── source_id
├── content_id
├── frame_id?
├── page_id?
├── page_index?
├── chapter_id?
├── source_type
├── source_dimensions?
└── coordinate_space?
```

The builder must preserve identity exactly from validated upstream input.

---

## 45. Source Block

Core output entity:

```text
SourceBlock
├── block_id
├── parent_block_id?
├── block_type
├── raw_text
├── normalized_text
├── child_block_ids[]
├── recognition_region_ids[]
├── recognition_line_ids[]
├── geometry_references[]
├── sequence_index
├── orientation
├── language_hint?
├── confidence
├── warnings[]
└── metadata
```

A block may be textual, structural, or both.

---

## 46. Block Hierarchy

Example comic page:

```text
SourceDocument
└── Page
    ├── Panel
    │   ├── Dialogue
    │   ├── Dialogue
    │   └── SoundEffect
    └── Panel
        ├── Narration
        └── Dialogue
```

Example novel page:

```text
SourceDocument
└── Page
    ├── Heading
    ├── Paragraph
    ├── Paragraph
    │   ├── Dialogue
    │   └── Narration
    └── PageNumber
```

The MVP may use a flat block list when hierarchy cannot be inferred reliably.

---

## 47. Flat and Hierarchical Documents

The contract should support both:

```text
flat block structure
```

and:

```text
hierarchical block structure
```

A flat document remains valid.

Hierarchy must not be fabricated solely to satisfy a preferred schema.

---

## 48. Root Blocks

`root_blocks` contain blocks without a known parent.

Examples:

* one page block;
* several paragraph blocks;
* several bubble blocks;
* one selected-region block.

Consumers must not assume one root block only.

---

## 49. Reading Order

The final `SourceDocument` contains:

```text
reading_order[]
```

Each entry references a block.

Conceptual structure:

```text
ReadingOrderEntry
├── index
├── block_id
├── parent_context?
├── confidence
└── source
```

The order must be deterministic for the same input and profile.

---

## 50. Excluded Blocks

Noise or non-translatable content may be represented in:

```text
excluded_blocks[]
```

Examples:

* page numbers;
* repeated headers;
* watermarks;
* browser controls;
* advertisements;
* OCR garbage.

Excluded blocks must preserve traceability when possible.

They must not be silently deleted from all processing records.

---

## 51. Exclusion Reason

```text
BlockExclusion
├── block_id
├── reason
├── confidence
├── rule_id
└── reversible
```

Possible reasons:

```text
LikelyPageNumber
RepeatedHeader
LikelyWatermark
InterfaceNoise
LowConfidenceGarbage
ProfileExcludedType
ExplicitUserExclusion
```

---

# Traceability Layer

## 52. TraceabilityValidator

`TraceabilityValidator` verifies every output block.

It ensures:

* source block IDs are unique;
* region references exist;
* line references exist;
* geometry references remain valid;
* reading-order references exist;
* parent-child relationships are acyclic;
* excluded blocks remain attributable;
* normalized text has raw-source evidence.

---

## 53. Traceability Chain

Required chain:

```text
SourceDocument
    ↓
SourceBlock
    ↓
RecognitionRegion / RecognitionLine
    ↓
Source Geometry
    ↓
Source Image
```

Translation output will later extend the chain:

```text
TranslatedBlock
    ↓
SourceBlock
    ↓
RecognitionRegion
    ↓
Source Image
```

---

## 54. Synthetic Structural Blocks

Some structural blocks may not contain direct text.

Examples:

```text
Page
Panel
Section
```

These blocks may derive their traceability from child blocks.

A synthetic structural block must be marked:

```text
synthetic = true
```

It must not claim raw text that was not recognized.

---

## 55. Text Traceability Rule

Every nonempty `normalized_text` must derive from:

```text
one or more raw Recognition text values
```

Text Processing must not invent semantic source text.

Allowed synthetic content is limited to explicit structural separators or metadata.

---

# Result Layer

## 56. ResultAssembler

`ResultAssembler` creates the immutable `TextProcessingResult`.

Responsibilities:

* assign `processing_id`;
* attach `SourceDocument`;
* merge upstream and local warnings;
* calculate metrics;
* attach processing profile identity;
* attach timestamps;
* attach trace context;
* validate final contract;
* register a result reference when required.

---

## 57. TextProcessingResult

Conceptual structure:

```text
TextProcessingResult
├── processing_id
├── request_id
├── recognition_id
├── source_document
├── processing_profile
├── warnings[]
├── metrics
├── started_at
├── completed_at
└── trace_context
```

The result does not contain:

```text
translation_units
translation_provider
translation_prompt
target_language
translated_text
provider_token_budget
```

---

## 58. Empty Document Result

When Recognition contains no processable text, the module returns:

```text
TextProcessingResult
└── SourceDocument
    ├── root_blocks = []
    ├── reading_order = []
    └── excluded_blocks = []
```

with:

```text
NoProcessableText
```

This is a successful result.

---

## 59. Partial Document Result

Partial output is allowed when:

* valid regions remain;
* traceability is preserved;
* profile permits degraded processing;
* omitted content is explicitly warned.

Examples:

```text
UnresolvedRegionOrder
SkippedInvalidOptionalRegion
MixedOrientationPartiallySupported
```

The module must not hide that the document is partial.

---

# Rule System

## 60. ProcessingRuleRegistry

`ProcessingRuleRegistry` stores versioned deterministic rules.

Rule categories:

```text
NormalizationRule
OrderRule
LineJoinRule
GroupingRule
ClassificationRule
NoiseRule
BlockConstructionRule
```

---

## 61. Rule Execution

Each rule should expose:

```text
rule_id
rule_version
priority
supported_profiles
supported_languages
preconditions
decision
evidence
confidence
```

Rules should not mutate unrelated state.

---

## 62. Rule Ordering

Rules within one category should execute deterministically.

Recommended ordering:

```text
priority
then rule_id
then rule_version
```

Conflicting rule outcomes must be resolved by an explicit strategy.

---

## 63. Rule Conflict Strategies

Possible strategies:

```text
HighestConfidenceWins
HighestPriorityWins
ConservativeFallback
PreserveSeparateBlocks
RequireConsensus
```

For grouping and joining, the recommended fallback is:

```text
PreserveSeparateBlocks
```

---

## 64. Language-Specific Rules

Language-specific rules may exist for:

* Chinese spacing;
* Japanese vertical text;
* English word wrapping;
* punctuation;
* quotation marks;
* paragraph indentation.

Language-specific rules must be isolated from generic structural rules.

Unknown-language input must still be processable.

---

# Metrics and Diagnostics

## 65. ProcessingMetricsCollector

The collector records stage-level metrics.

Possible metrics:

```text
validation_duration_ms
adaptation_duration_ms
order_resolution_duration_ms
normalization_duration_ms
line_reconstruction_duration_ms
grouping_duration_ms
classification_duration_ms
document_build_duration_ms
traceability_validation_duration_ms
total_duration_ms
input_region_count
input_line_count
output_block_count
excluded_block_count
warning_count
order_change_count
line_join_count
region_group_count
```

---

## 66. Diagnostic Records

Diagnostics may record:

* rule decisions;
* joining evidence;
* grouping evidence;
* order changes;
* classification alternatives;
* excluded-block reasons;
* traceability failures.

Diagnostics must not log full source text by default.

---

## 67. Text Logging Policy

Default operational logs may contain:

```text
request_id
processing_id
recognition_id
block counts
warning codes
rule IDs
durations
profile identity
```

They should not contain:

```text
complete OCR text
complete normalized text
source image
translated text
```

Text samples require explicit diagnostic mode and bounded retention.

---

# Cancellation and Concurrency

## 68. ProcessingCancellationCoordinator

Responsibilities:

* register request cancellation token;
* react to explicit cancellation;
* react to session stop;
* react to superseded frames;
* react to application shutdown;
* check cancellation between stages;
* prevent result publication after cancellation.

---

## 69. Cancellation Points

Cancellation should be checked:

```text
before validation
after input adaptation
after order resolution
during normalization batches
during line reconstruction
during grouping
before document assembly
before result publication
```

Text Processing stages should generally be short, but cancellation must remain explicit.

---

## 70. Stateless Processing Preference

Core processors should be stateless where possible:

```text
TextNormalizer
LineReconstructor
RegionGrouper
BlockClassifier
TraceabilityValidator
```

Request-specific state should live in:

```text
ProcessingExecutionContext
```

---

## 71. Processing Execution Context

```text
ProcessingExecutionContext
├── request
├── active_profile
├── input_document
├── effective_order
├── working_nodes
├── warnings
├── metrics
├── cancellation_token
├── stage
└── trace_context
```

The context is request-scoped and must not be shared across requests.

---

## 72. Parallel Processing

Safe parallelism may include:

* normalizing independent regions;
* calculating geometry features;
* classifying independent groups;
* computing confidence evidence.

Ordering-sensitive stages should remain deterministic.

Parallel execution must not change final output order.

---

## 73. Idempotency

Given the same:

```text
RecognitionResult
ProcessingProfile version
ProcessingOptions
Rule versions
```

the module should produce the same:

```text
SourceDocument
```

excluding runtime-only fields such as timestamps and generated IDs.

A deterministic content fingerprint may support caching.

---

## 74. Processing Fingerprint

Conceptual fingerprint input:

```text
recognition_result_fingerprint
profile_id
profile_version
options_fingerprint
rule_set_version
```

Output:

```text
processing_fingerprint
```

This may be used to detect equivalent processing work.

---

# Error Handling

## 75. Error Normalizer

Internal exceptions must be converted to stable module errors.

Possible categories:

```text
InvalidProcessingRequest
UnsupportedRecognitionVersion
InvalidRecognitionResult
UnsupportedProcessingProfile
OrderResolutionFailed
NormalizationFailed
LineReconstructionFailed
RegionGroupingFailed
BlockClassificationFailed
DocumentConstructionFailed
TraceabilityValidationFailed
ProcessingCancelled
ProcessingTimeout
ResultAssemblyFailed
InternalProcessingError
```

---

## 76. Recoverable Stage Failure

A stage may degrade instead of fail when safe.

Examples:

```text
classification failure
→ block_type = Unknown
```

```text
order refinement failure
→ preserve valid Recognition order
```

```text
grouping uncertainty
→ preserve separate blocks
```

```text
noise classification uncertainty
→ keep block included
```

Recovery must preserve source evidence.

---

## 77. Non-Recoverable Failure

Processing must fail when:

* Recognition input is structurally invalid;
* traceability cannot be guaranteed;
* source identity is contradictory;
* references cannot be resolved;
* output hierarchy is cyclic;
* normalized text cannot be linked to raw input;
* cancellation has already committed;
* a required profile is unsupported.

---

# Profile-Specific Pipelines

## 78. Comic Page Pipeline

Recommended flow:

```text
RecognitionInputAdapter
        ↓
EffectiveOrderResolver
        ↓
TextNormalizer
        ↓
LineReconstructor
        ↓
Bubble/Geometry RegionGrouper
        ↓
Comic BlockClassifier
        ↓
SourceDocumentBuilder
```

Expected blocks:

```text
Panel
Dialogue
Narration
Thought
Caption
SoundEffect
Annotation
Unknown
```

For MVP, `Panel` may be omitted when panel structure is unavailable.

---

## 79. Comic Region Pipeline

For a user-selected bubble or region:

```text
RecognitionInputAdapter
        ↓
TextNormalizer
        ↓
LineReconstructor
        ↓
Single Root Block Construction
        ↓
SourceDocumentBuilder
```

Output may contain one root block.

No page-level order resolution is required.

---

## 80. Novel Page Pipeline

Recommended flow:

```text
RecognitionInputAdapter
        ↓
Column and Reading Order Resolution
        ↓
TextNormalizer
        ↓
Wrapped-Line Reconstruction
        ↓
Paragraph Grouping
        ↓
Heading / Paragraph Classification
        ↓
SourceDocumentBuilder
```

Expected blocks:

```text
Heading
Paragraph
Dialogue
Narration
PageNumber
Annotation
Unknown
```

---

## 81. Web Text Pipeline

When structured source text is available:

```text
StructuredTextAdapter
        ↓
TextNormalizer
        ↓
StructurePreserver
        ↓
SourceDocumentBuilder
```

This future path may bypass Recognition.

The public Text Processing contract should eventually support multiple source adapters.

---

## 82. Generic Document Pipeline

```text
RecognitionInputAdapter
        ↓
OrderResolver
        ↓
TextNormalizer
        ↓
LineReconstructor
        ↓
BlockGrouper
        ↓
GenericClassifier
        ↓
SourceDocumentBuilder
```

The generic profile should use conservative reconstruction.

---

# Extensibility

## 83. Source Adapter Extension Point

Future adapters may include:

```text
RecognitionResultAdapter
DOMTextAdapter
EPUBAdapter
PDFTextLayerAdapter
PlainTextAdapter
AccessibilityTreeAdapter
```

All adapters should output:

```text
ProcessingInputDocument
```

This allows the internal pipeline to remain stable.

---

## 84. Rule Extension Point

New rules may be added without changing the module contract.

Examples:

* Chinese vertical-order rule;
* Japanese punctuation rule;
* repeated web-header filter;
* comic sound-effect classifier;
* novel dialogue-boundary rule.

---

## 85. Block Type Extension

New block types should be added carefully.

Consumers must handle unknown future types through:

```text
Unknown
```

or a generic block fallback.

A minor contract version may add optional block types when compatibility is preserved.

---

## 86. Layout Module Integration

A future Layout Analysis module may provide:

```text
panel boundaries
speech-bubble boundaries
text-container identity
visual relationships
reading-order hints
```

Text Processing should consume this metadata through optional context or input references.

It should not directly depend on a Layout provider implementation.

Possible future flow:

```text
RecognitionResult
LayoutResult
        ↓
Text Processing
        ↓
SourceDocument
```

---

## 87. User Correction Integration

User corrections should not mutate the original `SourceDocument`.

Recommended model:

```text
SourceDocument
        +
SourceCorrectionSet
        ↓
CorrectedSourceDocumentView
```

A new processing result may later incorporate accepted corrections.

Correction ownership should be defined in a separate module or document.

---

# Internal Interfaces

## 88. Processor Interface

Conceptual processor interface:

```text
Processor<TInput, TOutput>
├── id
├── version
├── supports(context)
└── process(input, execution_context)
```

Processors should return:

```text
ProcessorResult<T>
├── value
├── warnings[]
├── metrics
└── diagnostics?
```

---

## 89. Order Resolver Interface

```text
EffectiveOrderResolver
└── resolve(
        input_document,
        profile,
        context
    ) → EffectiveReadingOrder
```

---

## 90. Normalizer Interface

```text
TextNormalizer
└── normalize(
        text_nodes,
        profile,
        context
    ) → NormalizedTextNodes
```

---

## 91. Line Reconstructor Interface

```text
LineReconstructor
└── reconstruct(
        normalized_nodes,
        effective_order,
        profile
    ) → ReconstructedLineGroup[]
```

---

## 92. Region Grouper Interface

```text
RegionGrouper
└── group(
        line_groups,
        input_document,
        effective_order,
        profile
    ) → SourceGroup[]
```

---

## 93. Block Classifier Interface

```text
BlockClassifier
└── classify(
        source_groups,
        profile,
        context
    ) → ClassifiedSourceGroup[]
```

---

## 94. Document Builder Interface

```text
SourceDocumentBuilder
└── build(
        classified_groups,
        effective_order,
        source_identity,
        profile
    ) → SourceDocument
```

---

## 95. Traceability Validator Interface

```text
TraceabilityValidator
└── validate(
        source_document,
        input_document
    ) → TraceabilityValidationResult
```

---

# State Ownership

## 96. Module State

Text Processing owns:

```text
module availability
active processing requests
processing execution stage
result assembly state
```

It does not own:

```text
Recognition request state
Translation request state
reading session state
current frame state
Presentation state
```

---

## 97. Request State

Recommended detailed internal states:

```text
Received
Validating
AdaptingInput
ResolvingOrder
Normalizing
ReconstructingLines
GroupingRegions
ClassifyingBlocks
BuildingDocument
ValidatingTraceability
AssemblingResult
PublishingResult
Cancelling
Completed
Failed
Cancelled
```

The public state contract may expose a simplified subset.

---

# Events

## 98. Produced Events

Required lifecycle events:

```text
text_processing.started
text_processing.completed
text_processing.failed
text_processing.cancelled
```

Optional progress events:

```text
text_processing.order_resolved
text_processing.normalization_completed
text_processing.blocks_created
text_processing.document_built
```

---

## 99. Consumed Events

Possible consumed events:

```text
recognition.completed
text_processing.requested
text_processing.cancellation_requested
session.stopped
source.closed
application.shutdown_requested
configuration.text_processing_changed
```

Text Processing should normally process a Recognition result only after receiving a valid result reference.

---

## 100. Event Payload Rule

Public events must not contain:

```text
full OCR text
full normalized text
SourceDocument payload
image bytes
translated text
```

Completion events should reference:

```text
TextProcessingResult
```

or:

```text
SourceDocument
```

through a secure result reference.

---

# Dependencies

## 101. Required Dependencies

Text Processing may depend on:

```text
Recognition public contract
Common identifier types
Common geometry types
Event Bus abstraction
Cancellation abstraction
Configuration abstraction
Diagnostics abstraction
Result registry abstraction
Clock abstraction
```

---

## 102. Forbidden Dependencies

Text Processing must not depend directly on:

```text
OCR provider SDKs
Translation provider SDKs
LLM SDKs
Presentation framework
Browser automation implementation
Capture driver
Session UI
Permanent storage schema
```

---

## 103. Dependency Direction

```text
Text Processing
    ↓
Common Contracts
Event Abstractions
Configuration
Diagnostics
```

Not:

```text
Text Processing
    ↓
Translation implementation
Recognition implementation
Presentation implementation
```

---

# Configuration

## 104. Configuration Categories

```text
TextProcessingConfiguration
├── default_profile
├── profile_overrides
├── normalization_settings
├── reading_order_settings
├── grouping_settings
├── classification_settings
├── noise_filter_settings
├── confidence_thresholds
├── result_retention
├── diagnostic_settings
└── performance_limits
```

---

## 105. Configuration Snapshot

Each processing request should use one immutable configuration snapshot.

A configuration change affects new requests only.

In-flight processing must not switch profiles or rules halfway through execution.

---

## 106. Configuration Safety

Configuration must not permit:

* disabling traceability validation;
* overwriting raw Recognition text;
* inserting arbitrary source text;
* weakening local privacy policy;
* loading executable rules from untrusted sources.

---

# Performance

## 107. Performance Goals

Text Processing should be low-latency compared with OCR and Translation.

Recommended goals:

* avoid remote calls;
* avoid full-page repeated processing;
* use deterministic rules;
* support result caching;
* use bounded working memory;
* allow cancellation;
* avoid copying image data;
* process text metadata rather than image pixels.

---

## 108. Memory Policy

Text Processing should retain:

```text
text
IDs
geometry references
small structural metadata
```

It should not retain:

```text
full image buffers
OCR model buffers
provider response objects
unbounded diagnostic text
```

---

## 109. Incremental Processing

Future incremental processing may support:

```text
previous SourceDocument
        +
changed Recognition regions
        ↓
updated SourceDocument
```

MVP should prefer full deterministic reconstruction unless profiling demonstrates a need.

Incremental updates must preserve immutability by creating a new document version.

---

# Testing Architecture

## 110. Unit Tests

Each processor requires isolated tests.

### RequestValidator

* valid request;
* unsupported contract;
* duplicate request ID;
* invalid profile;
* invalid timeout.

### RecognitionInputAdapter

* valid region references;
* invalid region references;
* duplicate line IDs;
* invalid order references;
* partial geometry.

### EffectiveOrderResolver

* valid provider order;
* missing order;
* vertical text;
* right-to-left comic;
* multicolumn novel;
* mixed orientation;
* uncertain order.

### TextNormalizer

* Chinese whitespace;
* English spacing;
* Unicode normalization;
* control characters;
* punctuation preservation;
* idempotency.

### LineReconstructor

* wrapped novel lines;
* vertical columns;
* unrelated adjacent lines;
* punctuation continuation;
* low-confidence separation.

### RegionGrouper

* one bubble split into regions;
* independent nearby bubbles;
* paragraph grouping;
* selected-region preservation;
* ambiguous grouping.

### BlockClassifier

* dialogue;
* heading;
* narration;
* page number;
* sound effect;
* unknown fallback.

### SourceDocumentBuilder

* flat document;
* hierarchical document;
* empty document;
* excluded blocks;
* stable order;
* unique IDs.

### TraceabilityValidator

* valid traceability;
* missing region;
* cyclic hierarchy;
* invalid order entry;
* synthetic structural block;
* normalized text without raw evidence.

---

## 111. Integration Tests

Required integration cases:

```text
comic page → SourceDocument
comic selected region → SourceDocument
novel page → SourceDocument
vertical Chinese page → SourceDocument
empty Recognition result → empty SourceDocument
partial Recognition result → partial SourceDocument
cancel during grouping
cancel before publication
invalid Recognition result
deterministic repeated processing
```

---

## 112. Golden Document Tests

Representative Recognition inputs should be paired with expected `SourceDocument` outputs.

Recommended datasets:

```text
Chinese comic dialogue
Chinese vertical comic
English comic
Chinese web novel
English prose page
mixed-language page
interface text
no-text image
watermarked page
low-confidence OCR page
```

Golden tests must version:

```text
Recognition contract
processing profile
rule-set version
expected SourceDocument
```

---

## 113. Property Tests

Useful properties:

```text
every textual SourceBlock has raw-source evidence
```

```text
every reading-order block exists
```

```text
block hierarchy is acyclic
```

```text
normalization is idempotent
```

```text
same input and profile produce equivalent document
```

```text
excluded and included region references do not disappear silently
```

```text
no Translation-specific fields exist in SourceDocument
```

---

# MVP Architecture

## 114. Required MVP Components

The MVP should implement:

```text
TextProcessingFacade
RequestValidator
RecognitionInputAdapter
ProfileResolver
EffectiveOrderResolver
TextNormalizer
LineReconstructor
RegionGrouper
BasicBlockClassifier
SourceDocumentBuilder
TraceabilityValidator
ResultAssembler
```

---

## 115. MVP Profiles

Required:

```text
ComicPage
ComicRegion
NovelPage
GenericDocument
```

---

## 116. MVP Block Types

Required:

```text
Paragraph
Dialogue
Narration
Caption
Heading
SoundEffect
Annotation
PageNumber
Unknown
```

Optional structural types:

```text
Page
Panel
Section
```

---

## 117. MVP Simplifications

The MVP may:

* produce a flat block list;
* preserve Recognition order when refinement is uncertain;
* use rule-based classification;
* avoid panel detection;
* avoid semantic punctuation repair;
* avoid cross-page reconstruction;
* use region-preserving fallback;
* classify uncertain text as `Unknown`;
* treat one selected region as one root block;
* store only geometry references instead of merged geometry.

---

## 118. MVP Pipeline

```text
RecognitionResult
        ↓
Validate
        ↓
Adapt Input
        ↓
Resolve Order
        ↓
Normalize Text
        ↓
Join Lines
        ↓
Group Regions
        ↓
Classify Blocks
        ↓
Build Flat SourceDocument
        ↓
Validate Traceability
        ↓
TextProcessingResult
```

---

# Future Architecture

## 119. Potential Future Components

```text
LayoutMetadataAdapter
PanelStructureResolver
BubbleRelationshipResolver
LanguageDetector
AdvancedNoiseDetector
SemanticBoundaryDetector
IncrementalDocumentUpdater
CrossPageDocumentMerger
UserCorrectionIntegrator
DocumentDiffEngine
```

These should be added only when concrete requirements justify them.

---

## 120. Translation Planner Boundary

Translation Planner consumes:

```text
SourceDocument
TranslationRequest
ProviderCapabilities
TargetLanguage
TranslationPolicy
```

and produces internal:

```text
TranslationPlan
TranslationUnit[]
```

Text Processing must not import or call Translation Planner.

The only shared boundary is:

```text
SourceDocument contract
```

---

## 121. SourceDocument Stability Requirement

`SourceDocument` should remain stable even when:

* Translation provider changes;
* token limits change;
* target language changes;
* prompt strategy changes;
* translation pricing changes;
* models gain larger context windows;
* Translation batches are regrouped.

This is the main reason for separating document reconstruction from translation planning.

---

# Architectural Invariants

## 122. Module Invariants

1. Text Processing produces a `SourceDocument`.
2. Text Processing does not produce provider-specific Translation units.
3. Every textual output remains traceable to Recognition input.
4. Raw Recognition text is preserved.
5. Normalized text is stored separately.
6. Geometry remains in source coordinate space.
7. Completed results are immutable.
8. Processing is deterministic by default.
9. Profiles are versioned.
10. Rule sets are versioned.
11. Empty text is a successful result.
12. Uncertainty is represented explicitly.
13. Classification is non-destructive.
14. Noise exclusion is reversible where practical.
15. Translation concerns do not enter this module.
16. Recognition implementation details do not enter this module.
17. Public events do not carry complete source text.
18. Cancellation prevents downstream completion.
19. Traceability validation cannot be skipped.
20. The same request reaches exactly one terminal outcome.

---

# Open Decisions

## 123. Unresolved Questions

The following remain open:

* whether `SourceDocument` should support multiple pages in the first contract;
* whether panel structures belong in this module or a future Layout module;
* whether language detection belongs in Text Processing;
* whether source blocks should store merged geometry or only references;
* whether block hierarchy is required in MVP;
* whether repeated headers should be excluded automatically;
* whether sound effects are included by default;
* whether page numbers remain included or excluded;
* how vertical Chinese columns should be ordered;
* how mixed-language text should be represented;
* whether corrections create new documents or document views;
* how processing fingerprints should be calculated;
* how long result references remain available;
* whether `raw_text` may be omitted from some transport views for privacy;
* whether DOM and EPUB adapters should be added before Translation;
* whether profile selection should be explicit or inferred by default.

---

## 124. Recommended MVP Decisions

Recommended initial decisions:

```text
one SourceDocument represents one page or selected source scope
flat block list is valid and preferred initially
panel hierarchy is optional
language remains a hint
store geometry references and optional union bounds
preserve sound effects as blocks
exclude page numbers only with high confidence
preserve uncertain noise as included Unknown blocks
use explicit profile from Session when available
fall back to GenericDocument
keep active processing state in memory
make results session-scoped
```

---

# Related Documents

## 125. Related Architecture

```text
modules/text-processing/README.md
modules/text-processing/CONTRACT.md
modules/text-processing/EVENTS.md
modules/text-processing/STATES.md

modules/recognition/README.md
modules/recognition/MODULE.md
modules/recognition/CONTRACT.md
modules/recognition/EVENTS.md
modules/recognition/STATES.md

modules/translation/README.md
modules/translation/MODULE.md

docs/architecture/DATA_FLOW.md
docs/architecture/EVENT_BUS.md
docs/architecture/STATE_MACHINE.md
docs/architecture/MODULE_DEPENDENCY.md
```

---

# Summary

## 126. Final Architecture

The Text Processing module performs:

```text
RecognitionResult
        ↓
Validate
        ↓
Normalize
        ↓
Resolve Reading Order
        ↓
Reconstruct Lines
        ↓
Group Regions
        ↓
Classify Blocks
        ↓
Build SourceDocument
        ↓
Validate Traceability
        ↓
TextProcessingResult
```

Its public output is:

```text
SourceDocument
```

not:

```text
TranslationUnit[]
```

The module guarantees:

* provider-independent source structure;
* raw and normalized text separation;
* deterministic reconstruction;
* explicit reading order;
* source-block classification;
* Recognition traceability;
* source-relative geometry;
* immutable results;
* profile-based behavior;
* safe uncertainty handling;
* no Translation-provider dependency;
* no prompt or token-budget logic.

The central architectural boundary is:

```text
Text Processing reconstructs the source document.

Translation plans how that document should be translated.
```
