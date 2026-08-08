# Text Processing Module

> **Project:** CRAI
> **Module:** Text Processing
> **Path:** `02-modules/text-processing/README.md`
> **Version:** 1.0
> **Status:** Architecture Draft
> **Owner:** CRAI Architecture

---

# 1. Purpose

Text Processing transforms canonical Recognition output into a stable, traceable source-document representation that downstream Translation can consume.

Its central question is:

```text
Given recognized source text and OCR structure,
how should that source be normalized and reconstructed
without changing its meaning?
```

Text Processing sits between Recognition and Translation:

```text
Recognition
    ↓
Text Processing
    ↓
Translation
```

Recognition determines what text exists.

Text Processing reconstructs usable source structure.

Translation decides what should be translated together and converts meaning into the target language.

---

# 2. Core Boundary

Canonical transformation:

```text
Recognition Artifact
        ↓
Input Adaptation
        ↓
Normalization
        ↓
Reconstruction
        ↓
Grouping
        ↓
Classification
        ↓
SourceDocument Construction
        ↓
Traceability Validation
        ↓
CandidateSourceDocumentArtifact
        ↓
Runtime
```

Text Processing produces:

```text
SourceDocument
```

It does not produce:

```text
TranslationUnit
```

Translation owns Translation Unit construction and segmentation policy.

---

# 3. Position in CRAI

```text
Capture / Observation
        ↓
Recognition
        ↓
Text Processing
        ↓
Translation
        ↓
Presentation
```

At the module boundary:

```text
Recognition
    → Recognition Artifact

Text Processing
    → Candidate SourceDocument Artifact

Runtime
    → validates authority / freshness
    → publishes accepted Artifact

Translation
    → consumes SourceDocument
```

---

# 4. Why Text Processing Exists

Recognition output is optimized for describing detected text.

It may contain:

* OCR lines
* regions
* geometry
* confidence
* raw recognized text
* canonical Reading Order references
* Quality information
* provider evidence

That representation is not necessarily the best semantic structure for Translation.

Examples:

```text
OCR line wrapping

fragmented dialogue

paragraph lines split visually

multiple OCR regions belonging together

noise blocks

headings

captions

sound effects

annotations
```

Without Text Processing:

```text
Recognition cleanup
        +
source reconstruction
        +
translation segmentation
        +
translation logic
        ↓
Translation becomes responsible
for too many unrelated concerns
```

Text Processing creates the stable boundary between OCR semantics and translation semantics.

---

# 5. Ownership

Text Processing owns:

```text
Recognition Artifact adaptation

deterministic text normalization

source reconstruction

logical source grouping

source block classification

noise/exclusion decisions

SourceDocument construction

source traceability

processing warnings

Candidate validation

Candidate submission
```

Text Processing does not own:

```text
Capture

Detection

Recognition provider execution

OCR Reading Order

OCR Quality

OCR geometry

Translation Unit segmentation

target-language translation

translation provider selection

Presentation

Runtime scheduling

Work Queue

retry execution

cancellation outcome

stale-result disposition

Artifact publication
```

---

# 6. Recognition vs Text Processing vs Translation

## Recognition

Answers:

```text
What text was detected?
```

Produces:

```text
Recognition Artifact
OCR Document
OCR Regions
OCR Lines
RawText
Geometry
Reading Order
Quality evidence
```

---

## Text Processing

Answers:

```text
How should the recognized source
be represented as a stable source document?
```

Produces:

```text
SourceDocument
SourceBlock
NormalizedText
Traceability
Warnings
Candidate Artifact
```

---

## Translation

Answers:

```text
What source content should be translated together,
and what does it mean in the target language?
```

Translation may create:

```text
TranslationUnit
TranslationContext
TranslatedUnit
```

---

# 7. Primary Input

Text Processing receives an Attempt-scoped input.

Conceptually:

```text
TextProcessingInput
├── RuntimeContext
├── RecognitionArtifactRef
├── ProcessingProfileRef
├── ProcessingOptions
├── ConfigurationSnapshotRef
├── PrivacyContextRef
└── TraceContext
```

Text Processing resolves required upstream artifacts through references.

---

# 8. Upstream Semantic Inputs

Primary upstream artifact:

```text
Recognition Artifact
```

It may reference:

```text
OCRDocument

ReadingOrderResult

QualityReport

Recognition warnings

Geometry

Provider evidence
```

Text Processing must preserve upstream ownership.

It must not reinterpret an upstream error as a Text Processing-owned error unless the failure occurs inside Text Processing itself.

---

# 9. Processing Input Document

Recognition Artifact is adapted into an internal processing representation.

```text
ProcessingInputDocument
├── SourceIdentity
├── ContentIdentity
├── OCRDocumentRef
├── ReadingOrderRef?
├── QualityReportRef?
├── Regions[]
├── Lines[]
├── RawTextRefs[]
├── GeometryRefs[]
├── UpstreamWarnings[]
└── Metadata
```

This structure is internal.

It does not replace the Recognition contract.

---

# 10. Processing Plan

Before semantic processing begins:

```text
ProcessingPlan
├── Profile
├── Rules
├── Options
├── RequiredInputs
├── OptionalInputs
├── FallbackPolicy
└── ConfigurationSnapshot
```

Plan creation must be deterministic for equivalent inputs/configuration.

A valid plan becomes:

```text
READY
```

before processing begins.

---

# 11. Main Processing Flow

```text
Recognition Artifact
        ↓
Resolve Input
        ↓
Build Processing Plan
        ↓
Adapt Source
        ↓
Normalize
        ↓
Reconstruct
        ↓
Group
        ↓
Classify
        ↓
Build SourceDocument
        ↓
Validate Traceability
        ↓
Assemble Candidate
        ↓
Validate Candidate
        ↓
Submit Candidate
```

---

# 12. Normalization

Normalization performs deterministic cleanup without intentionally changing semantic meaning.

Examples:

```text
trim non-semantic whitespace

normalize Unicode representation

normalize safe spacing

normalize known OCR control characters

normalize line separator representation

preserve meaningful punctuation

preserve source-language characters
```

Text Processing must preserve source evidence.

Conceptually:

```text
RawText
    ↓ deterministic transformation
NormalizedText
```

Both remain traceable.

---

# 13. Normalization Must Not Become Rewriting

Text Processing must not:

```text
paraphrase

translate

rewrite dialogue

improve prose

invent missing content

resolve cultural expressions

apply target-language glossary

silently choose one interpretation
for ambiguous OCR
```

Ambiguity should remain explicit.

---

# 14. Reconstruction

OCR structure and source-document structure are different concepts.

Example:

```text
OCRLine A
OCRLine B
OCRLine C
```

may represent:

```text
one paragraph
```

or:

```text
one dialogue block
```

or:

```text
three independent source blocks
```

Text Processing may reconstruct these structures using evidence.

---

# 15. Reconstruction Evidence

Possible evidence:

```text
OCR region membership

canonical Reading Order

geometry proximity

orientation

line continuity

punctuation continuity

layout metadata

content profile

Quality hints

provider structure
```

Reconstruction must remain explainable.

---

# 16. Conservative Reconstruction

When evidence is weak:

```text
preserve separate
```

is preferred over:

```text
unsafe merge
```

Example:

```text
uncertain line join
    ↓
preserve separate SourceBlocks
    +
RECONSTRUCTION_UNCERTAIN
```

---

# 17. Grouping

Grouping determines which reconstructed source elements belong to the same logical source block.

Examples:

```text
speech bubble

paragraph

caption

heading

annotation

sound effect

interface label
```

Grouping must not merge unrelated content merely because it is spatially close.

---

# 18. Classification

Text Processing may classify SourceBlocks.

Possible types:

```text
DIALOGUE

NARRATION

PARAGRAPH

HEADING

CAPTION

SOUND_EFFECT

ANNOTATION

INTERFACE_TEXT

PAGE_NUMBER

WATERMARK

UNKNOWN
```

Classification is:

* optional where possible
* non-destructive
* confidence-aware
* deterministic for the same rules
* allowed to return `UNKNOWN`

Uncertainty is not automatically failure.

---

# 19. Noise and Exclusion

Some recognized content may not belong to normal Translation input.

Examples:

```text
page numbers

watermarks

repeated headers

browser UI

advertisements

OCR garbage
```

Text Processing may mark such blocks as excluded.

It must not destroy source evidence.

Conceptually:

```text
SourceBlock
├── Included
└── Excluded
```

Exclusion remains traceable.

---

# 20. SourceDocument

Primary semantic result:

```text
SourceDocument
├── DocumentId
├── SourceIdentity
├── ContentIdentity
├── RootBlocks[]
├── Blocks[]
├── BlockSequence[]
├── ExcludedBlocks[]
├── Completeness
├── TraceabilityMetadata
├── ProcessingMetadata
└── Warnings[]
```

The exact canonical schema is defined in `CONTRACT.md`.

---

# 21. SourceBlock

Conceptually:

```text
SourceBlock
├── BlockId
├── BlockType
├── RawText
├── NormalizedText
├── ChildBlockRefs[]
├── SourceEvidenceRefs[]
├── GeometryRefs[]
├── Confidence?
├── Exclusion?
└── Warnings[]
```

A SourceBlock represents reconstructed source structure.

It does not represent a Translation Unit.

---

# 22. SourceBlockSequence

Text Processing may derive:

```text
SourceBlockSequence
```

from canonical upstream Reading Order and reconstructed source structure.

This answers:

```text
In what sequence should these SourceBlocks
be exposed downstream?
```

It does not redefine:

```text
canonical OCR Reading Order
```

---

# 23. Reading Order Boundary

Canonical Reading Order belongs to OCR Architecture.

Text Processing:

```text
consumes Reading Order
        ↓
maps OCR entities
to SourceBlocks
        ↓
derives SourceBlockSequence
```

Text Processing must not independently replace canonical OCR Reading Order.

---

# 24. RawText and NormalizedText

These are distinct.

```text
RawText
```

preserves recognized source content.

```text
NormalizedText
```

contains deterministic cleanup.

Example:

```text
RawText:
"你  好 ！"

NormalizedText:
"你好！"
```

NormalizedText must remain traceable to RawText.

---

# 25. Traceability

Every textual SourceBlock must remain attributable to upstream Recognition evidence.

Conceptually:

```text
SourceBlock
    ↓
SourceEvidenceRef
    ↓
OCR Line / Region
    ↓
Geometry
    ↓
Source Image
```

For normalization:

```text
NormalizedText
    ↓
NormalizationChange
    ↓
RawText
```

---

# 26. Geometry

Text Processing does not own OCR geometry.

It preserves:

```text
GeometryRef
```

or derived non-authoritative geometry relationships.

Text Processing must not independently redefine the source coordinate system.

---

# 27. Completeness

SourceDocument explicitly describes completeness.

Possible conceptual values:

```text
COMPLETE

PARTIAL_VALID

EMPTY_VALID
```

Empty content is valid.

---

# 28. Empty Input

Recognition may validly contain no processable text.

Result:

```text
SourceDocument
    Completeness = EMPTY_VALID
    Blocks = []
```

with warning:

```text
NO_PROCESSABLE_TEXT
```

This is not an error.

---

# 29. Partial Input

Recognition may be partial but still usable.

Text Processing may produce:

```text
Completeness = PARTIAL_VALID
```

when:

* available references remain valid
* traceability remains valid
* Processing Profile permits partial output

Upstream warnings must remain visible through references/metadata.

---

# 30. Candidate Artifact

Text Processing does not directly publish the canonical Artifact.

It creates:

```text
CandidateSourceDocumentArtifact
```

Conceptually:

```text
CandidateSourceDocumentArtifact
├── CandidateArtifactId
├── SourceDocument
├── RecognitionArtifactRef
├── ProcessingProfileRef
├── ConfigurationSnapshotRef
├── CompatibilityMetadata
├── TraceabilityMetadata
├── Warnings[]
└── CandidateMetadata
```

---

# 31. Candidate Lifecycle

```text
ASSEMBLING
    ↓
VALIDATING
    ↓
VALID
    ↓
SUBMITTED
```

Failure path:

```text
ASSEMBLING / VALIDATING
    ↓
INVALID
```

Runtime may later classify submitted Candidate as:

```text
ACCEPTED

REJECTED_STALE

REJECTED_INVALID_AUTHORITY

REJECTED_INCOMPATIBLE
```

Those are not Text Processing-owned processing states.

---

# 32. Runtime Authority Boundary

Text Processing may determine:

```text
this Candidate is internally valid
```

It may not determine:

```text
this Candidate is still current

this Attempt owns publication authority

this result should replace the canonical Artifact
```

Runtime owns those decisions.

---

# 33. Artifact Publication Boundary

Canonical flow:

```text
Text Processing
    ↓
Candidate
    ↓
Runtime
    ↓
Artifact Store
    ↓
Published SourceDocument Artifact
```

Text Processing does not own:

```text
Artifact publication

ownership transfer

canonical Artifact replacement
```

---

# 34. Translation Boundary

Translation consumes the accepted SourceDocument Artifact.

Translation may decide:

```text
which blocks translate together

context window

Translation Unit size

provider token boundaries

sentence splitting

paragraph batching

target-language handling
```

Therefore Text Processing must not encode provider-oriented Translation Units.

---

# 35. TranslationUnit Is Not a Text Processing Type

Old architecture:

```text
Text Processing
    ↓
TranslationUnit[]
```

Current architecture:

```text
Text Processing
    ↓
SourceDocument
    ↓
Translation
    ↓
TranslationUnit[]
```

This separation is intentional.

---

# 36. Processing Profiles

Different content types require different reconstruction rules.

Recommended profiles:

```text
COMIC

NOVEL

GENERIC_DOCUMENT

INTERFACE_TEXT
```

Possible future profiles:

```text
WEB_TEXT

EPUB

PDF_TEXT_LAYER
```

Profiles affect source reconstruction.

They must not contain Translation-provider segmentation rules.

---

# 37. Comic Profile

Typical behavior:

```text
preserve bubble separation

support vertical text

preserve sound effects

use OCR Reading Order

use geometry for grouping

avoid paragraph-style over-merging

allow UNKNOWN classification
```

Perfect semantic classification is not required.

---

# 38. Novel Profile

Typical behavior:

```text
merge visual wraps conservatively

preserve paragraphs

preserve dialogue boundaries

preserve headings

reduce geometry dependence

remove configured repeated headers

maintain source continuity
```

Novel processing should not depend heavily on comic-specific geometry heuristics.

---

# 39. Generic Document Profile

Typical behavior:

```text
preserve block structure

preserve headings

preserve lists

preserve known table boundaries

avoid genre assumptions

use conservative grouping
```

---

# 40. Web Text

Future CRAI inputs may provide structured text without OCR.

Examples:

```text
DOM

Accessibility Tree

EPUB

PDF text layer
```

A future generalized upstream Source Artifact may allow Text Processing to consume these directly.

For the current Recognition-based pipeline:

```text
Recognition Artifact
```

remains the primary input.

---

# 41. Determinism

Equivalent:

```text
Input Artifact

Processing Profile

Processing Options

Configuration Snapshot
```

should produce equivalent semantic:

```text
SourceDocument
```

External nondeterministic rewriting must not be part of the core Text Processing path.

---

# 42. Idempotency

Re-executing the same deterministic processing configuration should not change semantic output merely because execution occurred again.

Runtime identifiers such as:

```text
AttemptId
```

may differ.

Semantic source structure should not.

---

# 43. Immutability

Text Processing must never mutate:

```text
Recognition Artifact

OCR Document

Reading Order Result

Quality Report
```

A valid Candidate is immutable after validation.

Changes require a new Candidate.

---

# 44. Warnings

Warnings represent degraded but valid processing.

Examples:

```text
NO_PROCESSABLE_TEXT

PARTIAL_SOURCE_DOCUMENT

NORMALIZATION_SKIPPED

NORMALIZATION_DEGRADED

RECONSTRUCTION_UNCERTAIN

GROUPING_UNCERTAIN

CLASSIFICATION_UNCERTAIN

STRUCTURE_FLATTENED

OPTIONAL_READING_ORDER_UNAVAILABLE

OPTIONAL_QUALITY_REPORT_UNAVAILABLE

BLOCK_EXCLUSION_UNCERTAIN

UPSTREAM_WARNING_PRESERVED
```

Warnings are machine-readable.

---

# 45. Errors

Text Processing-owned errors use:

```text
TXT-<CATEGORY>-<NUMBER>
```

Categories:

```text
INPUT

PLAN

ADAPT

NORM

RECON

GROUP

CLASS

DOC

TRACE

CAND

RES

STATE

PRIV

INT
```

Detailed definitions belong to:

```text
ERRORS.md
```

---

# 46. Error Ownership Boundary

Text Processing does not redefine:

```text
OCR errors

Reading Order errors

Quality errors

Queue errors

Scheduler errors

Runtime deadline errors

cancellation outcomes

stale-result outcomes

Artifact publication errors

Translation errors
```

External errors are preserved through references.

---

# 47. Retry

Text Processing may return:

```text
RetryHint
```

Example strategies:

```text
SAME_PROFILE

CONSERVATIVE_PROFILE

DISABLE_OPTIONAL_GROUPING

DISABLE_OPTIONAL_CLASSIFICATION

FLAT_STRUCTURE

RESOURCE_WAIT

NO_RETRY
```

RetryHint is advisory.

Runtime owns retry execution.

---

# 48. Cancellation

Text Processing observes:

```text
CancellationContext
```

When cancellation is observed it should:

```text
stop new expensive work

release local resources

avoid invalid Candidate submission

return control to Runtime
```

Runtime owns the final cancellation outcome.

---

# 49. Deadline

Runtime owns Attempt deadline.

Text Processing may observe remaining budget and:

```text
skip optional work

use permitted fallback

stop processing

cleanup
```

Text Processing does not own a global `ProcessingTimeout` lifecycle.

---

# 50. State Model

Text Processing does not own the WorkItem/Attempt lifecycle.

It owns several local state machines.

Primary ones:

```text
Module Availability

Processing Plan

Operation Phase

Candidate Validation
```

Detailed definitions belong to:

```text
STATES.md
```

---

# 51. Operation Phases

Typical phases:

```text
RESOLVING_INPUT

BUILDING_PLAN

ADAPTING_SOURCE

NORMALIZING

RECONSTRUCTING

GROUPING

CLASSIFYING

BUILDING_DOCUMENT

VALIDATING_TRACEABILITY

ASSEMBLING_CANDIDATE

VALIDATING_CANDIDATE

SUBMITTING_CANDIDATE

CLEANING_UP
```

These are operation phases.

They are not Runtime WorkItem states.

---

# 52. Events

Text Processing emits bounded semantic/diagnostic events.

Examples:

```text
text_processing.plan_ready

text_processing.processing_started

text_processing.processing_warning

text_processing.candidate_ready

text_processing.candidate_submitted

text_processing.processing_failed
```

Events must not become a second state machine.

Detailed event contract belongs to:

```text
EVENTS.md
```

---

# 53. Event Privacy

Normal events must not contain:

```text
RawText

NormalizedText

full SourceDocument

image bytes

browser content

translated text

credentials
```

Use references instead.

---

# 54. Resource Management

Text Processing may acquire Attempt-local resources such as:

```text
Recognition Artifact lease

OCR Document reference

temporary reconstruction buffers

temporary grouping structures

Candidate assembly buffers
```

Resources must be:

```text
bounded

Attempt-scoped

released on completion

released on failure

released on cancellation

released after Candidate rejection
```

Infrastructure-level resource ownership remains with Resource Manager.

---

# 55. Privacy

Text Processing handles source content and must assume it may be sensitive.

Rules:

```text
do not log source text by default

do not expose source text in normal events

do not embed source text in errors

respect PrivacyContext

bound diagnostic retention

use references across boundaries

avoid external services in core processing
```

MVP Text Processing should not require a remote provider.

---

# 56. Observability

Recommended module metrics:

```text
text_processing.attempt_total

text_processing.success_total

text_processing.warning_total

text_processing.error_total

text_processing.normalization_duration

text_processing.reconstruction_duration

text_processing.grouping_duration

text_processing.document_build_duration

text_processing.candidate_validation_duration

text_processing.total_duration

text_processing.source_block_count
```

High-cardinality identifiers belong in traces/logs, not metric labels.

---

# 57. Performance

Text Processing should generally be lighter than Recognition and Translation.

Interactive priorities:

```text
low latency

bounded memory

deterministic rules

limited allocations

cancellation responsiveness

reuse of immutable upstream Artifacts

avoid repeated whole-document work
```

Optimization must not weaken traceability.

---

# 58. Dependencies

Text Processing depends conceptually on:

```text
Recognition contract

OCR canonical contracts

Runtime Context

Artifact references

Configuration

Privacy Context

Resource Manager

Telemetry / Logging

Event Bus
```

It must not depend directly on:

```text
OCR provider SDK

Translation provider SDK

Capture implementation

Presentation implementation

UI framework
```

---

# 59. Consumers

Primary semantic consumer:

```text
Translation
```

Other possible consumers:

```text
source preview

user correction

diagnostics

evaluation tooling

export

future indexing
```

Consumers should depend on the stable SourceDocument contract rather than Text Processing implementation details.

---

# 60. MVP Scope

MVP should support:

```text
Recognition Artifact adaptation

deterministic Unicode normalization

safe whitespace normalization

RawText preservation

canonical Reading Order consumption

basic reconstruction

basic grouping

basic classification

UNKNOWN classification

Comic profile

Novel profile

SourceDocument construction

SourceBlockSequence

traceability

EMPTY_VALID result

PARTIAL_VALID result

warnings

Candidate assembly

Candidate validation

Candidate submission

cancellation observation

bounded resources

deterministic output
```

---

# 61. MVP Non-Goals

MVP does not require:

```text
AI semantic rewriting

perfect speech-bubble classification

perfect panel understanding

advanced document reconstruction

handwriting correction

grammar rewriting

author-style restoration

cross-chapter semantic context

learned grouping models

user-trained reconstruction models

permanent text indexing

Translation Unit construction

Translation provider optimization
```

---

# 62. Future Extensions

Possible later capabilities:

```text
learned source grouping

panel-aware reconstruction

speech-bubble metadata integration

advanced vertical-text reconstruction

structured DOM input

EPUB input

PDF text-layer input

table reconstruction

list reconstruction

user correction workflow

genre-aware processing

long-document source reconstruction

repeated-header detection

bilingual source alignment
```

These should remain optional extensions.

---

# 63. Architecture Invariants

Text Processing must guarantee:

1. Recognition Artifact is immutable.

2. OCR Document is immutable.

3. RawText is preserved.

4. NormalizedText remains traceable to RawText.

5. Every textual SourceBlock has upstream evidence.

6. Text Processing does not redefine OCR geometry.

7. Text Processing does not redefine canonical Reading Order.

8. SourceBlockSequence is distinct from OCR Reading Order.

9. Text Processing produces SourceDocument.

10. Text Processing does not produce TranslationUnit.

11. Translation segmentation belongs to Translation.

12. Empty source content is valid.

13. Partial source content may be valid.

14. UNKNOWN classification is valid.

15. Ambiguous grouping prefers preservation.

16. Ambiguous reconstruction prefers preservation.

17. Source evidence is never silently deleted.

18. Text Processing does not translate.

19. Text Processing does not perform creative rewriting.

20. Candidate is validated before submission.

21. Valid Candidate becomes immutable.

22. Runtime owns publication authority.

23. Runtime owns stale-result disposition.

24. Runtime owns retry execution.

25. Runtime owns cancellation outcome.

26. Runtime owns deadline outcome.

27. Artifact Store owns canonical publication.

28. Text Processing errors contain no source content.

29. Events contain no source content by default.

30. Processing remains deterministic for equivalent semantic inputs/configuration.

---

# 64. Key Design Decisions

Current architecture adopts these decisions:

1. Text Processing remains separate from Recognition.

2. Text Processing remains separate from Translation.

3. Recognition produces canonical Recognition Artifacts.

4. Text Processing consumes Recognition Artifacts through references.

5. Text Processing creates SourceDocument.

6. Translation creates Translation Units.

7. RawText and NormalizedText remain distinct.

8. Source structure remains traceable to OCR evidence.

9. OCR Reading Order remains upstream-owned.

10. SourceBlockSequence is a downstream projection, not replacement Reading Order.

11. Uncertainty is preserved instead of guessed.

12. Empty content is valid.

13. Candidate validation occurs before Runtime submission.

14. Runtime decides whether Candidate is current and authoritative.

15. Artifact Store performs canonical publication.

16. Processing profiles control reconstruction, not Translation-provider segmentation.

17. Comic and Novel processing use different source-reconstruction policies.

18. Core processing remains deterministic.

---

# 65. Folder Structure

Current core module:

```text
02-modules/text-processing/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
└── ERRORS.md
```

Optional future documentation may include:

```text
profiles/
├── COMIC.md
├── NOVEL.md
└── GENERIC.md

rules/
├── NORMALIZATION.md
├── RECONSTRUCTION.md
├── GROUPING.md
└── CLASSIFICATION.md

tests/
└── CONTRACT_CASES.md
```

Do not add these until the architecture requires additional detail.

---

# 66. Related Documents

```text
02-modules/text-processing/MODULE.md
02-modules/text-processing/CONTRACT.md
02-modules/text-processing/STATES.md
02-modules/text-processing/EVENTS.md
02-modules/text-processing/ERRORS.md

02-modules/recognition/README.md
02-modules/recognition/MODULE.md
02-modules/recognition/CONTRACT.md
02-modules/recognition/STATES.md
02-modules/recognition/EVENTS.md
02-modules/recognition/ERRORS.md

01-architecture/ocr/README.md
01-architecture/ocr/READING_ORDER.md
01-architecture/ocr/POSTPROCESS.md
01-architecture/ocr/QUALITY.md

01-architecture/runtime/

03-infrastructure/artifact-store/
03-infrastructure/resource-manager/

02-modules/translation/
```

---

# 67. Summary

Text Processing owns the transformation:

```text
Recognition Artifact
        ↓
Normalization
        ↓
Source Reconstruction
        ↓
Grouping
        ↓
Classification
        ↓
SourceDocument
        ↓
Candidate
```

The critical ownership chain is:

```text
Recognition
    ↓
What text exists?

Text Processing
    ↓
How is that source text represented
as a stable source document?

Translation
    ↓
What should be translated together
and what does it mean?

Presentation
    ↓
How should translated content
be displayed?
```

And the Runtime authority boundary is:

```text
Text Processing
    ↓
Candidate
    ↓
Runtime
    ↓
Artifact Store
    ↓
Canonical SourceDocument Artifact
```

The most important rule is:

```text
Text Processing reconstructs source structure.

Translation constructs translation units.

Runtime decides whether results are current.

Artifact Store publishes canonical artifacts.
```
