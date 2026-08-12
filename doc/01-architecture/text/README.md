# CRAI Text Architecture

> **Project:** CRAI
> **Path:** `doc/01-architecture/text/README.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Architecture Owner:** Text Processing
> **Public Artifact:** `SourceDocumentArtifact`
> **Runtime Model:** Runtime v2 aligned
> **Last Updated:** 2026-08-10

---

# 1. Purpose

The Text Architecture defines CRAI's canonical source-language semantic model and the rules used to reconstruct and refine source-language structure.

It answers two primary questions:

```text
What is CRAI's canonical
source-language document model?

How are meaningful source-language
boundaries reconstructed and refined?
```

The Text Architecture belongs to:

```text
Text Processing
```

---

# 2. Central Architecture Rule

Text Architecture operates on:

```text
source-language semantics
```

It does not own:

```text
Translation semantics

Translation request construction

Translation context assembly

Presentation semantics

Runtime execution
```

The central boundary is:

```text
Text Processing
    ↓
SourceDocumentArtifact
```

---

# 3. Architecture Position

CRAI supports more than OCR-derived text.

Canonical architecture:

```text
Visual Source
    ↓
Capture
    ↓
Recognition
    ↓
RecognitionArtifact
        \
         \
          ↓
       Text Processing
          ↑
         /
        /
Structured Source
```

Text Processing then produces:

```text
Candidate SourceDocumentArtifact
    ↓
Authority Validation
    ↓
Published SourceDocumentArtifact
```

---

# 4. Downstream Position

After publication:

```text
Published SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationArtifact
    ↓
Presentation
    ↓
PresentationArtifact
    ↓
UI Adapter
```

Text Architecture ends at:

```text
SourceDocumentArtifact
```

---

# 5. Visual Source Path

For screen/comic/image content:

```text
CaptureArtifact
    ↓
Recognition
    ↓
RecognitionArtifact
    ↓
Text Processing
    ↓
SourceDocumentArtifact
```

Recognition provides source text and relevant visual/source provenance.

Text Processing reconstructs canonical source-language semantics.

---

# 6. Structured Source Path

For machine-readable content:

```text
DOM / Structured Text
Plain Text
Clipboard
Document Text Layer
        ↓
Source Adapter
        ↓
Normalized Structured Source Input
        ↓
Text Processing
        ↓
SourceDocumentArtifact
```

Structured text does not pass through OCR by default.

---

# 7. Convergence

Visual and structured paths converge at:

```text
SourceDocumentArtifact
```

Therefore downstream Translation does not need to know whether source text originated from:

```text
OCR

DOM

clipboard

plain text

document text layer

future structured source
```

---

# 8. Scope

Text Architecture currently covers two major concerns:

```text
Text Model

Segmentation
```

Together they define:

```text
canonical source-language structure

source mapping

normalization semantics

linguistic/structural boundaries

boundary provenance

manual structural corrections

incremental source reconstruction
```

---

# 9. Current Document Structure

```text
01-architecture/text/
├── README.md
├── TEXT_MODEL.md
└── SEGMENTATION.md
```

No additional architecture document is currently required.

---

# 10. TEXT_MODEL.md

`TEXT_MODEL.md` answers:

```text
What is the canonical
source-language representation?
```

It defines concepts such as:

```text
SourceDocument

Section

Block

Paragraph

Sentence

Span

Token

SourceReference

Annotation

Relationship

canonical source text

normalization

source mapping

language metadata

quality metadata
```

---

# 11. TEXT_MODEL Ownership

The Text Model belongs to:

```text
Text Processing
```

Its published cross-module representation is carried by:

```text
SourceDocumentArtifact
```

---

# 12. TEXT_MODEL Is Source-Agnostic

The Text Model is not an OCR-only representation.

It must support:

```text
Recognition-derived text

structured browser text

novel text

plain text

clipboard text

document text layers

user-authored source text

future source adapters
```

---

# 13. SEGMENTATION.md

`SEGMENTATION.md` answers:

```text
How are meaningful source-language
boundaries reconstructed and refined?
```

It defines:

```text
sentence boundaries

paragraph refinement

boundary strength

protected boundaries

continuation

cross-line reconstruction

cross-bubble semantics

language-specific rules

manual segmentation overrides

incremental segmentation

provisional boundaries
```

---

# 14. Segmentation Output

Segmentation does not create:

```text
TranslationUnit

TranslationBatch
```

Instead it refines:

```text
SourceDocument
```

through structures such as:

```text
Paragraph

Sentence

Span

Boundary

Continuation

SourceReference
```

---

# 15. Text Model vs Segmentation

The relationship is:

```text
TEXT_MODEL.md
    ↓
defines the semantic structures

SEGMENTATION.md
    ↓
defines how relevant boundaries
inside those structures are derived/refined
```

In short:

```text
Text Model
    = what source-language structure is

Segmentation
    = how source-language boundaries are determined
```

---

# 16. Normalization

Normalization belongs to:

```text
Text Processing
```

and is currently defined as part of:

```text
TEXT_MODEL.md
```

It covers semantics such as:

```text
Unicode normalization

whitespace normalization

line-break reconstruction

control-character cleanup

safe punctuation normalization

canonical source-text resolution
```

---

# 17. Why No NORMALIZATION.md Yet

A separate:

```text
NORMALIZATION.md
```

is not currently necessary.

Creating one now would largely duplicate:

```text
TEXT_MODEL.md
```

without establishing a new architecture boundary.

It may be extracted later if normalization grows into a sufficiently complex independent architecture concern.

---

# 18. Language Metadata

Language-related source semantics are currently represented by the Text Model.

Examples:

```text
language tag

script

confidence

mixed-language spans

inheritance
```

Language-aware boundary rules are defined in:

```text
SEGMENTATION.md
```

---

# 19. Why No LANGUAGE.md Yet

A separate:

```text
LANGUAGE.md
```

is intentionally deferred.

Language detection may involve multiple architecture owners:

```text
Recognition

Text Processing

Translation

future source adapters
```

Creating `text/LANGUAGE.md` now could incorrectly assign architecture-wide language detection ownership to Text Processing.

---

# 20. Validation

Text Model and Segmentation both define semantic validation requirements.

Examples:

```text
valid ranges

valid source references

deterministic order

mapping integrity

boundary integrity

manual override integrity

canonical text preservation
```

---

# 21. Why No VALIDATION.md Yet

A separate:

```text
VALIDATION.md
```

is not currently necessary.

Validation remains close to the semantic concept being validated.

If Text Processing later develops a large shared validation framework, this decision can be revisited.

---

# 22. Reading Order

Text Architecture does not own architecture-wide visual Reading Order.

For visual sources:

```text
Recognition / OCR architecture
```

owns source reading-order semantics.

Text Processing consumes the authoritative result.

---

# 23. Structured Source Order

For structured sources, order may originate from:

```text
DOM structure

document structure

source adapter

text offsets

chapter/section order
```

Text Processing preserves/reconstructs canonical semantic order from that input.

---

# 24. No Duplicate READING_ORDER.md

Do not create:

```text
01-architecture/text/READING_ORDER.md
```

while visual Reading Order is already defined under:

```text
01-architecture/ocr/READING_ORDER.md
```

Text Processing may detect suspicious ordering but does not silently become a second Reading Order authority.

---

# 25. Source Mapping

A central responsibility of Text Architecture is preserving traceability.

Conceptually:

```text
Source Entity / Source Range
        ↕
SourceDocument Node
```

For visual sources:

```text
Recognition entity / geometry
        ↕
Text Node
```

For structured sources:

```text
source locator / text range
        ↕
Text Node
```

---

# 26. Many-to-Many Mapping

Source mapping must support:

```text
N source entities
    ↕
M semantic nodes
```

because Text Processing may:

```text
merge Recognition lines

split one Block into multiple Sentences

combine multiple Bubbles semantically

reconstruct Paragraphs

refine structured source ranges
```

---

# 27. Geometry Boundary

Text Processing may reference source geometry.

It does not re-own geometry.

Conceptually:

```text
SourceDocument Node
    ↓
SourceReference
    ↓
Recognition/Capture geometry
```

---

# 28. Geometry Is Optional

Structured text may have no meaningful geometry.

Therefore:

```text
geometry
```

is optional in the generic Text Architecture.

---

# 29. Canonical Source Text

Text Architecture owns source-language representation.

Conceptually:

```text
Source Text
    ↓
Normalized Text
    ↓
Corrected Text?
```

Canonical source text resolves from the appropriate authoritative layer.

---

# 30. No Translation in SourceDocument

SourceDocument must not store:

```text
translatedText
```

as canonical source truth.

Translation output belongs to:

```text
TranslationArtifact
```

---

# 31. Translation Boundary

Translation consumes:

```text
Published SourceDocumentArtifact
```

and owns:

```text
TranslationUnit

TranslationBatch

context assembly

provider request construction

TranslationArtifact
```

---

# 32. No Translation Context in Text Architecture

Deprecated architecture:

```text
Text Model
    ↓
Segmentation
    ↓
Translation Context
    ↓
Translation
```

Current architecture:

```text
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationUnit
    ↓
TranslationBatch
    ↓
Context Assembly
```

---

# 33. No CONTEXT.md Under Text

Do not create:

```text
01-architecture/text/CONTEXT.md
```

for Translation context.

Context assembly belongs to Translation.

---

# 34. Target-Language Independence

Source-language Text Model and Segmentation should normally remain independent from:

```text
target language

Translation provider

Translation model

Translation pricing

Translation request limits

target font

overlay size
```

---

# 35. Presentation Boundary

Presentation consumes:

```text
TranslationArtifact
```

and owns:

```text
PresentationArtifact

layout semantics

text fitting

display formatting

presentation degradation
```

---

# 36. Display Text Boundary

Text Processing may preserve:

```text
source style hints

semantic whitespace

ruby/furigana

writing direction
```

It does not own final target-language display formatting.

---

# 37. Runtime Boundary

Runtime owns:

```text
RuntimeRevision

WorkItem

Attempt

scheduling

retry

cancellation

supersession

resource control
```

Text Architecture owns source-language semantic correctness.

---

# 38. Runtime Execution

Conceptually:

```text
TextProcessing WorkItem
    ↓
Attempt
    ↓
Text Processing
    ↓
Candidate SourceDocumentArtifact
```

Runtime execution identity does not become Text Model identity.

---

# 39. Candidate and Published

Text Processing follows:

```text
Attempt
    ↓
Candidate SourceDocumentArtifact
    ↓
Authority Validation
    ↓
Published SourceDocumentArtifact
```

A successfully computed Candidate is not automatically current authority.

---

# 40. Late Results

If older work finishes after newer authoritative content exists:

```text
old Candidate
```

must not replace the current Published Artifact.

Cancellation is an optimization.

Publication validation is the correctness boundary.

---

# 41. Reading Session Boundary

Reading Session owns:

```text
ReadingContext

ReadingContextRevision

session lifecycle
```

Text Processing does not own reading-session authority.

---

# 42. ReadingContextRevision vs SourceDocument

A new SourceDocument does not automatically imply:

```text
new ReadingContextRevision
```

Example:

```text
same comic reading context
    ↓
user scrolls
    ↓
new source content
    ↓
new RuntimeRevision
    ↓
new SourceDocumentArtifact
```

while ReadingContext may remain unchanged.

---

# 43. Preferences Boundary

Persistent user preferences belong to:

```text
Preferences
```

Text Processing receives an effective immutable configuration/profile for the operation.

---

# 44. Text Processing Profile

A Text Processing profile may configure:

```text
source/document type

normalization policy

structural granularity

language policy

segmentation policy

auxiliary-content policy

source-mapping policy
```

The profile is configuration, not a separate architecture owner.

---

# 45. Source-Specific Profiles

Possible profiles:

```text
comic

webtoon

novel

browser_article

plain_text

document

selection

mixed
```

They configure one Text Processing architecture.

Do not create separate Text Processing modules per source type.

---

# 46. Comic Priorities

Comic processing emphasizes:

```text
Block/Bubble semantics

source geometry

cross-line reconstruction

source order

cross-bubble continuation

manual correction
```

---

# 47. Novel Priorities

Novel processing emphasizes:

```text
chapter/section structure

paragraph fidelity

dialogue preservation

sentence boundaries

incremental processing

stable source locators
```

---

# 48. Structured Text Priorities

Structured text emphasizes:

```text
source structure preservation

semantic locators

paragraph fidelity

avoiding unnecessary OCR

incremental source changes
```

---

# 49. Unicode

Text Architecture must remain Unicode-safe.

Every range contract must define its indexing unit explicitly.

Never assume:

```text
one visible character
=
one byte
```

or:

```text
one visible character
=
one UTF-16 code unit
```

---

# 50. Chinese Support

Initial Text Architecture must support:

```text
Simplified Chinese

Traditional Chinese

full-width punctuation

mixed Chinese/Latin text

Chinese quotation marks

ellipsis

vertical source text

sentences across visual lines

sentences across Bubbles
```

---

# 51. Script Preservation

Normalization must not silently convert:

```text
zh-Hans
↔
zh-Hant
```

Script conversion is not generic source normalization.

---

# 52. Manual Corrections

Text Architecture may support source-level corrections such as:

```text
text correction

boundary split

boundary merge

continuation correction

semantic reorder within allowed authority
```

Corrections must preserve provenance.

---

# 53. Correction Boundary

Do not create:

```text
text/CORRECTION.md
```

yet.

Correction spans multiple domains:

```text
Recognition/source correction

Text Processing correction

Translation correction

Presentation consequences
```

A future architecture-level correction flow/domain should be considered before assigning all correction semantics to Text Processing.

---

# 54. Incremental Processing

Text Architecture must support incremental reconstruction for:

```text
long novels

webtoons

continuously changing browser content

partial source updates

manual corrections
```

---

# 55. Incremental Does Not Mean Mutable

Published Artifacts remain immutable.

Incremental internal computation produces a coherent new Candidate.

---

# 56. Determinism

Given equivalent:

```text
source semantic input

Text Processing profile

normalization policy version

segmentation policy version
```

deterministic algorithms should produce equivalent semantic structure.

Runtime scheduling must not change semantic output.

---

# 57. Provider Independence

Text Architecture must not expose provider-native structures such as:

```text
PaddleOCR DTO

Google Vision DTO

DOM Node

HTMLElement

Translation provider request
```

as canonical Text Model fields.

---

# 58. Platform Independence

Text Architecture must not expose:

```text
browser native objects

window handles

native UI objects

platform-specific text structures
```

as stable cross-module contracts.

---

# 59. Events

Exact Text Processing events belong to:

```text
02-modules/text-processing/EVENTS.md
```

Architecture documents may describe semantic facts but must not create a competing event catalog.

---

# 60. Event Bus Rule

Do not orchestrate processing through:

```text
TextSegmentationCompleted
    ↓
TranslationRequested
```

Event Bus reports committed facts.

Runtime/business dependency readiness controls executable work.

---

# 61. States

Text Architecture does not define a global state machine such as:

```text
NORMALIZING
→ SEGMENTING
→ CONTEXT_BUILDING
→ TRANSLATING
```

Text Processing module states belong to:

```text
02-modules/text-processing/STATES.md
```

Runtime operation states belong to Runtime.

---

# 62. Errors

Exact Text Processing errors belong to:

```text
02-modules/text-processing/ERRORS.md
```

Architecture-level Text documents describe invariants and failure categories only.

---

# 63. Diagnostics

Diagnostics may observe:

```text
normalization changes

mapping coverage

segmentation confidence

forced boundaries

manual overrides

invalid ranges

processing latency

SourceDocument size
```

Diagnostics does not control Text semantics.

---

# 64. Cache

Text Processing output may be cached using semantically relevant inputs such as:

```text
source semantic fingerprint

source schema/version

Text Processing profile

normalization version

segmentation version

manual correction revision
```

---

# 65. Cache Is Not Authority

A cache hit does not mean:

```text
current SourceDocument authority
```

Cached output still requires compatibility and current-authority validation.

---

# 66. Large Documents

Text Architecture must not require an entire:

```text
novel

webtoon

large document
```

to be eagerly represented in memory.

Possible strategies include:

```text
bounded document windows

paged semantic ranges

lazy linguistic nodes

incremental reconstruction
```

---

# 67. Testing

Architecture-level tests should cover at least:

```text
Recognition-derived Chinese comic

structured Chinese novel

Traditional Chinese

mixed-language content

vertical text

cross-line sentences

cross-bubble sentences

manual corrections

incremental changes

structured source without geometry

Unicode edge cases
```

---

# 68. Reading Order for This Folder

Recommended:

```text
1. README.md

2. TEXT_MODEL.md

3. SEGMENTATION.md
```

---

# 69. Why This Order

First:

```text
README.md
```

establishes:

```text
scope

ownership

public boundary

upstream/downstream relationships
```

Then:

```text
TEXT_MODEL.md
```

defines:

```text
what canonical source-language structure is
```

Finally:

```text
SEGMENTATION.md
```

defines:

```text
how linguistic/structural boundaries
inside that model are refined
```

---

# 70. Documents Intentionally Not Added

Current architecture intentionally does not add:

```text
NORMALIZATION.md

LANGUAGE.md

VALIDATION.md

READING_ORDER.md

CONTEXT.md

CORRECTION.md

CONTRACTS.md

STRUCTURE.md
```

This avoids premature decomposition and duplicate ownership.

---

# 71. When to Add NORMALIZATION.md

Add it only if normalization develops sufficiently independent complexity such as:

```text
large rule families

language-specific normalization engines

independent versioning

complex compatibility behavior

dedicated testing/benchmark architecture
```

Until then it remains part of `TEXT_MODEL.md`.

---

# 72. When to Add LANGUAGE.md

Add it only after architecture ownership is decided for:

```text
language detection

language reconciliation

script detection

mixed-language resolution

confidence arbitration
```

across Recognition/Text Processing/Translation.

---

# 73. When to Add VALIDATION.md

Add it only if shared Text Processing validation becomes large enough that keeping rules near:

```text
Text Model

Segmentation
```

causes significant duplication or ambiguity.

---

# 74. When to Add CORRECTION Architecture

Before creating a text-specific correction document, determine whether CRAI needs a broader:

```text
CORRECTION_FLOW.md
```

or correction domain covering:

```text
source correction

segmentation correction

translation correction

provenance

reprocessing consequences
```

---

# 75. Architecture Invariants

1. Text Architecture is owned by Text Processing.

2. SourceDocumentArtifact is the public Text Processing boundary.

3. Text Architecture is not OCR-only.

4. RecognitionArtifact is the visual-text upstream boundary.

5. Structured source text may bypass Capture/Recognition.

6. Visual and structured paths converge at SourceDocumentArtifact.

7. Text Model defines canonical source-language structure.

8. Segmentation refines source-language boundaries.

9. Text Processing does not own TranslationUnit.

10. Text Processing does not own TranslationBatch.

11. Text Processing does not own Translation context assembly.

12. Text Processing does not own TranslationArtifact.

13. Text Processing does not own PresentationArtifact.

14. SourceDocument does not contain translated text as canonical truth.

15. Source Mapping supports many-to-many relationships.

16. Geometry remains upstream-owned/reference-based.

17. Geometry is optional for structured sources.

18. Reading Order is not duplicated under Text Architecture.

19. Normalization remains source-language semantics preserving.

20. Simplified/Traditional conversion is not default normalization.

21. Manual corrections preserve provenance.

22. Published SourceDocumentArtifact is immutable.

23. Runtime owns WorkItem/Attempt execution.

24. Runtime owns retry/cancellation/supersession.

25. Candidate output requires authority validation.

26. Event Bus does not orchestrate Translation from Text events.

27. Exact events belong to Text Processing EVENTS.md.

28. Exact errors belong to Text Processing ERRORS.md.

29. Text Architecture remains provider-independent.

30. Text Architecture remains platform-independent.

31. Cache remains optimization, not semantic authority.

32. Large documents support bounded/incremental representation.

33. Target-language layout does not determine source segmentation.

34. Translation provider limits do not determine source segmentation.

35. Source semantics remain traceable to their origin.

---

# 76. Deprecated README v1 Model

The previous README modeled:

```text
OCR Reading Order
    ↓
Text Model
    ↓
Segmentation
    ↓
Translation Context
    ↓
Translation
    ↓
Presentation
```

This is no longer the canonical architecture.

---

# 77. Current Model

Current architecture is:

```text
RecognitionArtifact
       \
        \
         ↓
      Text Processing
         ↑
        /
       /
Structured Source
```

Then:

```text
Text Model
    +
Segmentation
    ↓
Candidate SourceDocumentArtifact
    ↓
Authority Validation
    ↓
Published SourceDocumentArtifact
```

Then:

```text
Translation
    ↓
TranslationUnit
    ↓
TranslationBatch
    ↓
Context Assembly
    ↓
TranslationArtifact
    ↓
Presentation
```

---

# 78. Related Documents

```text
doc/01-architecture/core/
├── DATA_FLOW.md
├── STATE_MACHINE.md
└── EVENT_CONVENTION.md

doc/01-architecture/ocr/
└── READING_ORDER.md

doc/01-architecture/text/
├── README.md
├── TEXT_MODEL.md
└── SEGMENTATION.md

doc/01-architecture/flows/
├── SCREEN_COMIC_FLOW.md
├── STRUCTURED_TEXT_FLOW.md
└── CONTENT_CHANGE_FLOW.md

doc/02-modules/
├── recognition/
├── text-processing/
├── translation/
└── presentation/

doc/01-architecture/runtime/
```

---

# 79. Completion Criteria

The Text Architecture set is synchronized when:

* README identifies Text Processing as owner;
* SourceDocumentArtifact is the public boundary;
* OCR is not assumed to be the only input;
* structured text has a direct Text Processing path;
* TEXT_MODEL defines source-language semantics;
* SEGMENTATION defines source-language boundaries;
* TranslationUnit/Batch/context are Translation-owned;
* Reading Order is not duplicated;
* normalization remains within current Text Model scope;
* source mapping remains traceable;
* Candidate → Published semantics align with Runtime v2;
* no duplicate event/state/error authority exists;
* unnecessary architecture documents are not introduced prematurely.

---

# 80. Summary

The current Text Architecture intentionally consists of only:

```text
README.md

TEXT_MODEL.md

SEGMENTATION.md
```

Their roles are:

```text
README
    → scope and ownership

TEXT_MODEL
    → canonical source-language representation

SEGMENTATION
    → source-language boundary refinement
```

Together:

```text
Source Input
    ↓
Text Processing
    ↓
Canonical Source Structure
    ↓
Segmentation Refinement
    ↓
SourceDocumentArtifact
```

The central rule is:

```text
Text Architecture owns
source-language semantics.

Translation owns
translation semantics.

Runtime owns
execution semantics.

Presentation owns
presentation semantics.
```
