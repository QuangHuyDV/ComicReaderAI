# Text Processing Module

> **Project:** CRAI
> **Module:** Text Processing
> **Path:** `modules/text-processing/README.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-22

---

## 1. Purpose

The Text Processing module transforms raw structured Recognition output into clean, ordered, and translation-ready source text.

Its primary responsibility is to answer:

```text
What source text should be sent to Translation,
and how does each text unit map back to the original image?
```

The module sits between Recognition and Translation.

```text
Recognition
    ↓
Text Processing
    ↓
Translation
```

Recognition determines what characters and text regions were detected.

Text Processing determines how those characters and regions should be organized into stable source-text units.

Translation then converts those source-text units into the target language.

---

## 2. Why This Module Exists

Recognition output is not always suitable for direct translation.

Typical OCR output may contain:

* lines split at incorrect positions;
* unnecessary line breaks;
* inconsistent spaces;
* duplicated text;
* fragmented sentences;
* uncertain reading order;
* punctuation errors;
* isolated page numbers;
* mixed dialogue and annotation text;
* vertical and horizontal text mixed together;
* text regions that should be merged;
* text regions that must remain separate.

Passing this output directly to Translation creates several problems:

```text
Recognition errors
        +
Text layout reconstruction
        +
Translation logic
        =
Unclear module ownership
```

Text Processing prevents Translation from becoming responsible for OCR cleanup and document reconstruction.

---

## 3. Position in the CRAI Pipeline

The main processing flow is:

```text
Source Image
    ↓
Capture / Observation
    ↓
Recognition
    ↓
Text Processing
    ↓
Translation
    ↓
Presentation / Overlay
```

A more detailed flow is:

```text
RecognitionResult
    ↓
Input Validation
    ↓
Reading Order Resolution
    ↓
Surface Text Normalization
    ↓
Line Reconstruction
    ↓
Region Grouping
    ↓
Source Segmentation
    ↓
Translation Unit Construction
    ↓
TextProcessingResult
```

---

## 4. Core Responsibility

Text Processing owns the transformation:

```text
Recognized text regions
        ↓
Clean source-text structure
        ↓
Translation-ready units
```

The module must preserve the relationship between:

```text
translation unit
    ↓
source segment
    ↓
recognition region
    ↓
source image coordinates
```

This traceability is essential for later rendering translated text back into the correct image location.

---

## 5. Module Boundary

### 5.1 Text Processing Owns

Text Processing owns:

* recognition-result validation;
* deterministic surface-text cleanup;
* whitespace normalization;
* line-break normalization;
* line joining;
* paragraph reconstruction;
* region grouping;
* source-text segmentation;
* translation-unit construction;
* reading-order refinement;
* source-language structural analysis;
* text-type classification when deterministic;
* source traceability;
* processing warnings;
* processing confidence;
* preservation of raw Recognition output references.

---

### 5.2 Text Processing Does Not Own

Text Processing does not own:

* image capture;
* screenshot selection;
* region detection;
* OCR provider execution;
* character recognition;
* image preprocessing;
* language translation;
* translated-text rewriting;
* translation glossary resolution;
* overlay rendering;
* font selection;
* permanent content storage;
* reading-session lifecycle;
* provider credentials;
* user-interface state.

---

## 6. Responsibility Separation

The three central modules must remain distinct.

### Recognition

```text
What text appears in the image?
```

Recognition outputs:

* detected regions;
* raw recognized text;
* geometry;
* confidence;
* initial reading order;
* provider metadata.

---

### Text Processing

```text
How should the recognized text be cleaned,
ordered, grouped, and divided for translation?
```

Text Processing outputs:

* normalized source text;
* source segments;
* translation units;
* traceability metadata;
* warnings;
* structural confidence.

---

### Translation

```text
What does the processed source text mean
in the target language?
```

Translation outputs:

* translated units;
* translation confidence;
* glossary usage;
* provider metadata;
* translation warnings.

---

## 7. Inputs

The primary input is:

```text
RecognitionResult
```

The result should contain:

* recognition ID;
* request ID;
* source identity;
* content identity;
* frame identity when applicable;
* recognized regions;
* recognized lines;
* source-relative geometry;
* reading order;
* provider metadata;
* confidence values;
* warnings.

Optional processing context may include:

* expected source language;
* content mode;
* reading direction;
* text genre;
* normalization policy;
* segmentation policy;
* dialogue grouping hints;
* maximum translation-unit size;
* user-defined processing preferences.

---

## 8. Outputs

The primary output is:

```text
TextProcessingResult
```

A result should contain:

```text
TextProcessingResult
├── processing_id
├── request_id
├── recognition_id
├── source_id
├── content_id
├── frame_id?
├── source_language?
├── processing_profile
├── source_segments[]
├── translation_units[]
├── reading_order
├── warnings[]
├── metrics
└── trace_context
```

---

## 9. Source Segment

A source segment is a normalized piece of source text that still maps directly to Recognition output.

```text
SourceSegment
├── segment_id
├── raw_text
├── normalized_text
├── recognition_region_ids[]
├── recognition_line_ids[]
├── source_geometry
├── reading_order_index
├── segment_type
├── confidence
└── warnings[]
```

A source segment may represent:

* one speech bubble;
* one paragraph;
* one caption;
* one narration block;
* one interface label;
* one sound-effect region;
* one title;
* one footnote;
* one isolated line.

---

## 10. Translation Unit

A translation unit is the smallest stable unit sent to Translation.

```text
TranslationUnit
├── unit_id
├── source_text
├── source_segment_ids[]
├── source_language?
├── unit_type
├── sequence_index
├── context_before?
├── context_after?
├── geometry_references[]
├── segmentation_reason
├── confidence
└── warnings[]
```

One translation unit may contain:

* one source segment;
* several related source segments;
* a merged sentence;
* a dialogue sequence;
* a paragraph;
* a context window around a short text fragment.

---

## 11. Source Segment vs Translation Unit

These concepts must remain separate.

### Source Segment

Represents reconstructed source structure.

```text
What belongs together in the source?
```

### Translation Unit

Represents Translation input boundaries.

```text
What should be translated together?
```

Example:

```text
Speech bubble line 1
Speech bubble line 2
```

may become:

```text
one SourceSegment
```

Several adjacent dialogue segments may then be translated:

```text
as separate TranslationUnits
```

or:

```text
as one contextual TranslationUnit
```

depending on segmentation policy.

---

## 12. Processing Stages

The recommended processing pipeline is:

```text
1. Validate input
2. Resolve effective reading order
3. Normalize raw surface text
4. Reconstruct lines
5. Group related regions
6. Reconstruct paragraphs or dialogue blocks
7. Classify source segments
8. Build translation units
9. Validate traceability
10. Assemble result
```

---

## 13. Input Validation

The module must validate:

* supported Recognition contract version;
* required identifiers;
* valid region references;
* valid line references;
* valid reading-order references;
* valid source coordinate space;
* non-conflicting region IDs;
* non-conflicting line IDs;
* immutable Recognition result expectations;
* geometry consistency;
* result completeness policy.

Invalid input must not be silently corrected when doing so would break traceability.

---

## 14. Reading Order

Text Processing may refine Recognition reading order.

It may consider:

* provider order;
* source language;
* text orientation;
* left-to-right reading;
* right-to-left reading;
* top-to-bottom reading;
* comic panel layout;
* speech-bubble geometry;
* region grouping;
* content type.

The module must preserve:

```text
original recognition order
```

and produce:

```text
effective processing order
```

Any change in order must be explicit and traceable.

---

## 15. Surface Text Normalization

Surface normalization is deterministic cleanup that does not change semantic meaning.

Allowed operations include:

* trim leading and trailing whitespace;
* collapse repeated spaces;
* normalize Unicode;
* normalize common line separators;
* normalize full-width and half-width characters;
* normalize repeated punctuation spacing;
* remove OCR control characters;
* preserve meaningful punctuation;
* preserve source-language characters;
* mark uncertain characters;
* record removed artifacts.

Text Processing must not perform free-form rewriting.

---

## 16. Line Reconstruction

Recognition providers may return text as fragments.

Text Processing may join lines when evidence indicates they belong together.

Possible evidence:

* same recognition region;
* overlapping or aligned geometry;
* compatible orientation;
* small vertical or horizontal gap;
* sentence continuation;
* provider line order;
* punctuation continuity;
* consistent text style metadata.

Line joining must remain explainable.

The system should record:

```text
join_reason
```

for non-trivial joins.

---

## 17. Region Grouping

Several Recognition regions may belong to one logical source segment.

Examples:

* a speech bubble split into multiple OCR regions;
* one paragraph divided by provider detection;
* vertical text split into columns;
* title text split across stylized characters;
* annotation text broken into separate boxes.

Grouping may use:

* geometry proximity;
* enclosure relationship;
* reading order;
* orientation;
* text continuity;
* shared panel;
* shared bubble identity when available;
* segmentation profile.

Grouping must not merge unrelated text merely because it is nearby.

---

## 18. Paragraph Reconstruction

For novel and prose content, Text Processing may rebuild paragraphs.

Possible rules:

* join wrapped lines;
* preserve paragraph breaks;
* preserve dialogue breaks;
* preserve list boundaries;
* preserve chapter headings;
* preserve indentation when meaningful;
* remove OCR-only hard wraps;
* detect sentence continuation.

Novel processing and comic processing should use different profiles.

---

## 19. Comic Text Processing

Comic content requires special handling.

Possible source segment types:

```text
Dialogue
Narration
Thought
SoundEffect
Caption
Title
Annotation
Unknown
```

Comic processing may consider:

* speech-bubble boundaries;
* panel order;
* vertical text;
* stylized punctuation;
* short isolated text;
* sound-effect text;
* dialogue context;
* overlapping text regions.

The MVP should not require perfect semantic classification.

Unknown classification is acceptable.

---

## 20. Novel Text Processing

Novel processing prioritizes text continuity.

Typical behavior:

* merge visual line wraps;
* preserve paragraph boundaries;
* preserve dialogue punctuation;
* preserve chapter headings;
* remove repeated headers and footers when configured;
* detect page-break continuation;
* build context-aware translation units;
* avoid splitting sentences across units.

Novel processing should not depend on comic geometry heuristics.

---

## 21. Web Text Processing

Web novel or browser text may already have structure.

Possible inputs include:

* DOM text;
* selected text;
* OCR text;
* accessibility tree text;
* rendered page snapshots.

When structured text exists, Text Processing should prefer it over OCR output.

This module may later accept a generalized source-text input contract.

The initial implementation may remain Recognition-result based.

---

## 22. Text Classification

Text Processing may classify source segments when classification is needed for downstream behavior.

Possible types:

```text
Dialogue
Narration
Paragraph
Heading
Caption
SoundEffect
Annotation
InterfaceText
PageNumber
Watermark
Unknown
```

Classification should be:

* rule-based for MVP;
* confidence-scored;
* optional;
* non-destructive;
* overridable by later modules or user correction.

Classification must not alter the raw recognized text.

---

## 23. Noise Filtering

Some recognized text may not belong to the content being translated.

Examples:

* page numbers;
* watermarks;
* website navigation;
* repeated headers;
* advertisements;
* scan artifacts;
* OCR garbage;
* browser interface labels.

Filtering policy must be explicit.

Filtered content should be:

* marked as excluded;
* preserved in traceability metadata when practical;
* removable from Translation input;
* recoverable for debugging.

The module should avoid permanently deleting source evidence during processing.

---

## 24. Semantic Limits

Text Processing may apply deterministic structural corrections.

It must not:

* infer missing story content;
* paraphrase the author;
* translate the text;
* rewrite dialogue style;
* improve prose creatively;
* replace culturally specific expressions;
* resolve character names using a translation glossary;
* invent punctuation without confidence evidence;
* silently fix ambiguous OCR into one assumed meaning.

Ambiguity must be represented as a warning or uncertainty marker.

---

## 25. Traceability

Every normalized output must be traceable to Recognition input.

At minimum:

```text
TranslationUnit
    ↓ source_segment_ids
SourceSegment
    ↓ recognition_region_ids
RecognitionRegion
    ↓ source_geometry
Source Image
```

Text Processing must not create source text that cannot be linked to one or more Recognition elements, except for explicit structural markers.

---

## 26. Geometry Preservation

Text Processing does not own OCR geometry, but it must preserve geometry references.

Possible output geometry:

* union rectangle;
* union polygon;
* ordered geometry references;
* region list;
* primary region;
* anchor point.

Geometry must remain in source coordinate space.

Text Processing must not independently remap image coordinates.

---

## 27. Raw and Normalized Text

The module must distinguish:

```text
raw_text
```

from:

```text
normalized_text
```

`raw_text` preserves Recognition output.

`normalized_text` contains deterministic cleanup.

Example:

```text
raw_text:
"你  好 ！"

normalized_text:
"你好！"
```

The raw value must remain available for debugging and correction.

---

## 28. Confidence

Text Processing confidence represents structural reliability.

It is not the same as OCR confidence.

Possible confidence dimensions:

```text
TextProcessingConfidence
├── normalization_confidence
├── grouping_confidence
├── reading_order_confidence
├── segmentation_confidence
└── overall_confidence
```

A high OCR confidence does not guarantee correct grouping.

A low OCR confidence does not necessarily mean the reading order is uncertain.

---

## 29. Warnings

Possible warnings include:

```text
UncertainReadingOrder
AmbiguousLineJoin
AmbiguousRegionGrouping
PossibleDuplicateText
PossibleNoiseText
UnsupportedOrientation
MixedTextDirections
IncompleteRecognitionInput
LowRecognitionConfidence
UnmappedRecognitionRegion
TranslationUnitTooLarge
TranslationUnitTooSmall
SentenceBoundaryUncertain
UnknownSegmentType
NormalizationChangedText
```

Warnings must be machine-readable.

Human-readable messages may be attached.

---

## 30. Processing Profiles

Different content types require different policies.

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

A profile determines:

* grouping rules;
* line-join rules;
* reading direction;
* paragraph reconstruction;
* segment classification;
* translation-unit size;
* noise filtering;
* warning thresholds.

---

## 31. Comic Profile

Suggested defaults:

```text
preserve region separation
prefer bubble-level grouping
allow vertical text
keep short dialogue units
preserve sound effects
use spatial reading order
avoid paragraph-style line merging
```

---

## 32. Novel Profile

Suggested defaults:

```text
merge wrapped lines
preserve paragraphs
prefer sentence-complete units
use larger translation context
filter repeated page headers
reduce geometry dependence
```

---

## 33. Generic Document Profile

Suggested defaults:

```text
preserve block structure
preserve headings
preserve lists
preserve table boundaries when available
use moderate line merging
avoid genre assumptions
```

---

## 34. Translation Unit Construction

Translation-unit construction should optimize for:

* semantic completeness;
* stable context;
* provider token limits;
* output-to-source mapping;
* minimal fragmentation;
* rendering compatibility.

Units must not be created only by fixed character count.

Character limits may be used as safety boundaries.

---

## 35. Translation Context

A translation unit may contain context fields.

```text
context_before
context_after
```

These provide surrounding source text without changing the primary text to translate.

Context may improve:

* pronoun resolution;
* character-name consistency;
* dialogue continuity;
* sentence interpretation;
* Chinese-to-Vietnamese translation quality.

Context must not cause duplicate visible output.

---

## 36. Unit Size Policy

A unit-size policy may define:

```text
minimum_character_count
preferred_character_count
maximum_character_count
maximum_segment_count
sentence_boundary_preference
paragraph_boundary_preference
```

When a unit exceeds limits, splitting should prefer:

1. paragraph boundaries;
2. sentence boundaries;
3. dialogue boundaries;
4. source-segment boundaries;
5. safe character boundaries as last resort.

---

## 37. Idempotency

The same input and processing profile should produce the same result.

Conceptually:

```text
TextProcessingResult
=
process(
    RecognitionResult,
    ProcessingProfile,
    ProcessingOptions
)
```

Deterministic behavior supports:

* caching;
* testing;
* debugging;
* reproducibility;
* user correction comparison.

External nondeterministic AI rewriting must not be part of the core processing path.

---

## 38. Immutability

A completed `TextProcessingResult` is immutable.

Changes require:

* a new processing request;
* a new processing ID;
* a new profile version;
* a user-correction object;
* or a downstream derived result.

The module must not silently mutate previous results.

---

## 39. Versioning

Text Processing must version:

* public contract;
* processing profile;
* normalization rules;
* segmentation rules;
* classification rules.

Example:

```text
contract_version: 1.0.0
processing_profile:
  id: ComicPage
  version: 1.0.0
```

This allows result differences to be explained after rule changes.

---

## 40. Privacy

Text Processing handles recognized source text.

This data may be sensitive.

The module must:

* process locally when local-only policy is active;
* avoid logging complete source text by default;
* avoid exposing source text through operational events;
* limit diagnostic retention;
* respect session cleanup;
* use references for large results across process boundaries;
* avoid forwarding text to external services.

Text Processing itself should not require a remote provider for MVP.

---

## 41. Performance

Text Processing is expected to be lighter than Recognition and Translation.

Interactive usage should prioritize:

* low latency;
* deterministic rules;
* bounded memory;
* incremental processing where useful;
* avoidance of repeated full-page work;
* cacheable results;
* cancellation support.

Performance metrics may include:

```text
validation_duration_ms
normalization_duration_ms
grouping_duration_ms
segmentation_duration_ms
total_duration_ms
input_region_count
output_segment_count
translation_unit_count
```

---

## 42. Cancellation

Text Processing requests may be cancelled when:

* the source frame becomes stale;
* the session stops;
* the user changes the selected region;
* a newer Recognition result supersedes the request;
* the application shuts down.

Cancellation must prevent downstream Translation from starting.

A late internal result must be discarded after cancellation is accepted.

---

## 43. Empty Input

A valid Recognition result may contain no readable text.

Text Processing should return a successful empty result:

```text
source_segments = []
translation_units = []
```

with a warning such as:

```text
NoProcessableText
```

Empty input is not a processing failure.

---

## 44. Partial Input

Recognition may return a partial result with warnings.

Text Processing may continue when:

* region references remain valid;
* available text is structurally processable;
* policy permits partial processing.

The result must preserve upstream warnings and add its own warnings.

Text Processing must not hide partial-recognition status.

---

## 45. Error Categories

Possible module errors:

```text
InvalidProcessingRequest
UnsupportedRecognitionVersion
InvalidRecognitionResult
InvalidReadingOrder
InvalidRegionReference
InvalidLineReference
UnsupportedProcessingProfile
ProcessingCancelled
ProcessingTimeout
NormalizationFailed
GroupingFailed
SegmentationFailed
TraceabilityValidationFailed
ResultAssemblyFailed
InternalProcessingError
```

No-text input must not use an error category.

---

## 46. Events

The stable lifecycle events are expected to be:

```text
text_processing.started
text_processing.completed
text_processing.failed
text_processing.cancelled
```

Optional progress events may later include:

```text
text_processing.normalization_completed
text_processing.segments_created
text_processing.translation_units_created
```

Other modules must not depend on optional progress events for correctness.

---

## 47. State Model

The simplified request state flow is:

```text
Received
    ↓
Validating
    ↓
Processing
    ↓
AssemblingResult
    ↓
Completed
```

Alternative terminal outcomes:

```text
Failed
Cancelled
```

Detailed internal states may include:

```text
Normalizing
ResolvingOrder
Grouping
Reconstructing
Segmenting
BuildingTranslationUnits
ValidatingTraceability
```

---

## 48. Consumers

Primary consumer:

```text
Translation
```

Other possible consumers:

* diagnostics;
* user correction;
* source-text preview;
* search indexing;
* reading history;
* export;
* evaluation tooling.

The primary public contract should remain optimized for Translation.

---

## 49. Dependencies

Text Processing depends on:

```text
Recognition contract
Common identifier types
Common geometry types
Event Bus contract
Cancellation model
Configuration
Diagnostics
```

It must not depend directly on:

```text
Recognition provider SDKs
Translation provider SDKs
Presentation implementation
Capture implementation
Browser DOM implementation
UI framework
```

---

## 50. Expected Folder Structure

```text
modules/text-processing/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── EVENTS.md
├── STATES.md
├── profiles/
│   ├── COMIC.md
│   ├── NOVEL.md
│   └── GENERIC.md
├── rules/
│   ├── NORMALIZATION.md
│   ├── GROUPING.md
│   ├── READING_ORDER.md
│   └── SEGMENTATION.md
└── tests/
    └── CONTRACT_CASES.md
```

The additional folders may be added after the five core documents are stable.

---

## 51. MVP Scope

The MVP should support:

* Recognition-result validation;
* Chinese and English Unicode normalization;
* whitespace normalization;
* basic punctuation spacing;
* preservation of raw text;
* Recognition reading-order consumption;
* basic reading-order refinement;
* line joining inside one region;
* simple region grouping;
* comic and novel profiles;
* source-segment creation;
* translation-unit creation;
* source traceability;
* empty-result handling;
* cancellation;
* warnings;
* deterministic output.

---

## 52. MVP Non-Goals

The MVP does not require:

* AI-based semantic rewriting;
* perfect speech-bubble classification;
* perfect panel understanding;
* advanced document-layout reconstruction;
* table reconstruction;
* handwriting correction;
* automatic author-style restoration;
* grammar rewriting;
* full-page semantic understanding;
* user-trained segmentation models;
* cross-chapter context;
* permanent source-text indexing;
* collaborative correction.

---

## 53. Future Extensions

Potential later capabilities:

* learned region grouping;
* learned reading-order resolution;
* speech-bubble detection integration;
* panel-aware dialogue order;
* semantic sentence repair;
* OCR error candidate generation;
* user correction workflow;
* profile customization;
* source-language detection;
* genre detection;
* long-document context;
* repeated header detection;
* duplicate scan detection;
* bilingual source alignment;
* structured DOM input;
* EPUB input;
* PDF text-layer input;
* table and list reconstruction.

These should remain optional extensions.

---

## 54. Key Design Decisions

The initial design adopts the following decisions:

1. Text Processing is separate from Recognition.
2. Text Processing is separate from Translation.
3. Raw Recognition text is preserved.
4. Normalized text is stored separately.
5. Source segments and translation units are different concepts.
6. Every output remains traceable to Recognition regions.
7. Geometry remains source-relative.
8. Processing is deterministic by default.
9. Processing profiles are versioned.
10. Empty text is a successful result.
11. Semantic rewriting is outside the core module.
12. Progress events are optional.
13. Completed results are immutable.
14. Comic and novel processing use different profiles.
15. Translation receives clean units, not raw OCR regions.

---

## 55. Open Questions

The following decisions require later validation:

* whether comic panel detection belongs in Observation, Recognition, or a separate Layout module;
* whether speech-bubble classification belongs in Text Processing;
* whether sound effects should be translated by default;
* how vertical Chinese text should be grouped;
* how much punctuation correction is safe;
* whether Translation units should normally map one-to-one to source segments;
* how much surrounding context Translation should receive;
* whether web text should bypass Recognition;
* how user corrections should affect future processing;
* whether repeated headers should be filtered automatically;
* how to represent uncertain OCR character alternatives;
* whether layout rules should be language-specific;
* whether source-language detection belongs here or in Translation;
* whether result references should be temporary or session-scoped;
* whether segmentation should consider provider token pricing.

---

## 56. Related Documents

```text
modules/recognition/README.md
modules/recognition/MODULE.md
modules/recognition/CONTRACT.md
modules/recognition/EVENTS.md
modules/recognition/STATES.md

modules/text-processing/MODULE.md
modules/text-processing/CONTRACT.md
modules/text-processing/EVENTS.md
modules/text-processing/STATES.md

docs/architecture/DATA_FLOW.md
docs/architecture/EVENT_BUS.md
docs/architecture/STATE_MACHINE.md
docs/architecture/MODULE_DEPENDENCY.md
```

---

## 57. Summary

The Text Processing module transforms structured OCR output into stable source text for Translation.

Its core flow is:

```text
RecognitionResult
    ↓
Normalize
    ↓
Order
    ↓
Group
    ↓
Reconstruct
    ↓
Segment
    ↓
Build Translation Units
    ↓
TextProcessingResult
```

The module guarantees:

* preservation of raw OCR text;
* deterministic normalized text;
* explicit source structure;
* translation-ready units;
* traceability to Recognition regions;
* source-relative geometry;
* separate comic and novel policies;
* immutable results;
* warnings for uncertainty;
* no translation behavior;
* no image-recognition behavior;
* safe empty-result handling.

The most important boundary is:

```text
Recognition identifies visible text.

Text Processing reconstructs usable source text.

Translation converts meaning into another language.
```
