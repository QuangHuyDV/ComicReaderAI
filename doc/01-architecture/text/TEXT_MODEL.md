# CRAI Text Model

> **Project:** CRAI
> **Path:** `doc/01-architecture/text/TEXT_MODEL.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Architecture Owner:** Text Processing
> **Public Artifact:** `SourceDocumentArtifact`
> **Runtime Model:** Runtime v2 aligned
> **Last Updated:** 2026-08-10

---

# 1. Purpose

Text Model defines the canonical semantic representation of source-language text inside CRAI Text Processing.

It provides a provider-neutral representation for text originating from:

```text
RecognitionArtifact

Structured browser content

Plain text

Clipboard text

Imported documents

User-authored source text

Future structured source adapters
```

The public cross-module output is:

```text
SourceDocumentArtifact
```

whose semantic document body is based on the Text Model defined here.

---

# 2. Central Architecture Position

CRAI v2 does not define Text Model only as:

```text
OCR Domain
    ↓
Language Domain
```

because not all source text originates from OCR.

The broader model is:

```text
Visual Source
    ↓
Capture
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

Text Processing produces:

```text
SourceDocumentArtifact
```

which becomes the common source-language boundary for downstream semantic processing.

---

# 3. Public Boundary

The architecture boundary is:

```text
RecognitionArtifact
or
Normalized Structured Source Input
        ↓
Text Processing
        ↓
Candidate SourceDocumentArtifact
        ↓
Authority Validation
        ↓
Published SourceDocumentArtifact
```

Translation consumes the Published Artifact.

It does not consume Text Processing internals directly.

---

# 4. Text Model vs SourceDocumentArtifact

These concepts are related but distinct.

```text
Text Model
    = semantic document model owned by Text Processing

SourceDocumentArtifact
    = immutable published boundary carrying
      an authoritative Text Model snapshot
      plus Artifact identity/provenance
```

Therefore:

```text
TextDocument
```

is not an independent architecture authority competing with:

```text
SourceDocumentArtifact
```

---

# 5. Core Principle

Text Model must preserve meaning and provenance while separating text semantics from source-specific implementations.

For source-derived text, the system should be able to answer:

```text
Where did this text come from?

What source entities contributed to it?

What transformations were applied?

What is its canonical source-language representation?

What structural order does it have?

What geometry/source location can it map back to?
```

---

# 6. Objectives

Text Model must support:

```text
provider independence

source independence

structural consistency

traceability

deterministic mapping

source-language preservation

manual correction

incremental reconstruction

serialization

versioning

Translation compatibility

Presentation compatibility

Search/Export compatibility

comic text

novel text

webtoon text

mixed structured content
```

---

# 7. Non-Goals

Text Model does not own:

```text
Capture

OCR provider execution

Runtime WorkItem/Attempt lifecycle

Runtime retry

Runtime cancellation

TranslationUnit construction

TranslationBatch construction

Translation context assembly

Translation provider selection

translated-output authority

Presentation layout authority

native UI rendering
```

---

# 8. Ownership

Text Model semantics belong to:

```text
Text Processing
```

Specifically, Text Processing owns:

```text
SourceDocument

Text Node structure

source-language normalization

source mapping

semantic source ordering

document reconstruction

text provenance

document-level corrections
```

---

# 9. Downstream Ownership

Translation owns:

```text
TranslationUnit
TranslationBatch
Translation context assembly
TranslationArtifact
```

Presentation owns:

```text
PresentationArtifact
semantic presentation
layout/fitting decisions
```

UI Adapter owns:

```text
ViewModel
frontend projection
```

Text Model must not absorb those concerns.

---

# 10. Inputs

Text Processing may construct Text Model from multiple source families.

## Visual/Recognition Input

```text
RecognitionArtifact
```

may provide:

```text
recognized text

blocks/regions

geometry

reading hints/order

direction

language hints

confidence

source provenance
```

---

# 11. Structured Text Input

Structured input may provide:

```text
text blocks

paragraph structure

source locators

ordering information

language hints

semantic markup

safe source metadata
```

It must be platform-neutral before entering Text Processing.

---

# 12. No OCR-Only Input Contract

Deprecated assumption:

```text
Text Model Builder requires:

OCR Document
+
Reading Order Result
```

Current rule:

```text
Text Processing accepts
an owner-defined normalized source input
appropriate to the source family.
```

RecognitionArtifact is the visual-source public boundary.

---

# 13. Canonical Output

The canonical processing result is:

```text
SourceDocument
```

embedded in:

```text
SourceDocumentArtifact
```

The Artifact adds:

```text
ArtifactId

schema/version information

input provenance

runtime provenance where applicable

publication metadata
```

according to the Text Processing contract.

---

# 14. Semantic Model Overview

Conceptually:

```text
SourceDocument
├── Metadata
├── Pages / Logical Ranges?
├── Sections
├── Blocks
├── Paragraphs
├── Sentences?
├── Spans?
├── Tokens?
├── Auxiliary Content
├── Relationships
├── Source Mapping
├── Annotations
└── Quality Metadata
```

Not every document requires every layer.

---

# 15. Flexible Hierarchy

The hierarchy is intentionally flexible.

A comic Bubble may require:

```text
Block
└── Paragraph
    └── Provisional Sentence
```

A novel chapter may use:

```text
Document
└── Section
    └── Block
        └── Paragraph
```

Fine-grained linguistic nodes are constructed only when useful.

---

# 16. Document Node

`SourceDocument` represents one coherent source-language document scope.

Examples:

```text
comic page

stable comic viewport

webtoon range

novel chapter

selected article body

plain-text import

document page range
```

Its exact scope is determined by upstream/application processing requirements.

---

# 17. Document Identity

A SourceDocument must have stable semantic identity within its own version lineage.

Do not derive identity solely from:

```text
memory address

Runtime AttemptId

provider index

current order index
```

---

# 18. Document Version

Document semantic version/revision changes when relevant source semantics change.

Examples:

```text
canonical source text changes

document structure changes

source mapping changes

semantic order changes

confirmed source correction changes
```

It does not change merely because:

```text
new Runtime metrics arrive

logging changes

cache metadata changes

Telemetry changes
```

---

# 19. Artifact Version vs Document Version

Do not confuse:

```text
SourceDocument schema version

SourceDocument semantic revision

SourceDocumentArtifact identity/version

RuntimeRevisionId
```

These represent different authorities.

---

# 20. Text Node

`TextNode` is a generic semantic text element.

Possible node kinds:

```text
Page / Logical Range

Section

Block

Paragraph

Sentence

Span

Token
```

Each kind must have explicit semantics.

---

# 21. Structural vs Linguistic Nodes

Structural nodes describe document organization.

Examples:

```text
Section

Block

Paragraph
```

Linguistic nodes describe language-processing structure.

Examples:

```text
Sentence

Span

Token
```

The same hierarchy must not imply all linguistic layers are eagerly available.

---

# 22. Logical Page

A logical Page may represent:

```text
physical page

comic page

webtoon range

viewport range

virtual processing range
```

A Page is optional when the source has no meaningful page semantics.

---

# 23. Virtual Page

For continuous content, a Virtual Page may represent:

```text
webtoon range

scroll segment

logical viewport

processing window
```

It must not be confused with:

```text
Runtime WorkItem
```

A semantic document range is not execution identity.

---

# 24. Section

A Section groups coherent Blocks.

Possible types:

```text
Panel

Scene

DialogueGroup

NarrationGroup

ChapterHeader

Body

Footnote

CaptionGroup

Auxiliary

Unknown
```

Section type may remain shallow; deep semantic interpretation is not mandatory.

---

# 25. Block

Block is a relatively independent semantic source-text structure.

Examples:

```text
Dialogue

Narration

Caption

SFX

BackgroundText

Sign

Title

Subtitle

Header

Footer

Footnote

BodyText

Unknown
```

---

# 26. Block Source

For recognition-derived input, Block may correspond to:

```text
Recognition block

OCR region/container

speech bubble

layout container
```

For structured input it may correspond to:

```text
DOM paragraph

document block

source paragraph

semantic HTML block
```

Downstream consumers should not need source-native types.

---

# 27. Reading Role

A Block may carry a reading role such as:

```text
Main

Auxiliary

Excluded

Reference

Decorative

Unknown
```

If trustworthy upstream ordering/classification exists, Text Processing should preserve it rather than re-infer without reason.

---

# 28. Paragraph

Paragraph represents source-language text that belongs together structurally.

Possible fields:

```text
ParagraphId

OrderIndex

ParentBlockId

SourceText

NormalizedText

CorrectedText?

Language

Direction?

SourceReferences[]

Confidence/Quality

Metadata
```

---

# 29. Paragraph Boundary

Paragraph boundaries may derive from:

```text
structured-source paragraph boundary

Recognition paragraph

line spacing

layout grouping

bubble structure

source markup

manual correction
```

OCR newline alone is not sufficient semantic evidence.

---

# 30. Sentence

Sentence is an optional/progressive linguistic node useful for:

```text
Translation preparation

Search

Annotation

Alignment

language analysis
```

Text Model must not require perfect sentence boundaries during initial SourceDocument construction.

---

# 31. Provisional Sentence

Initial construction may use:

```text
ProvisionalSentence
```

covering one Paragraph or other safe scope.

Later Text Processing segmentation may:

```text
split

merge

adjust boundaries
```

while preserving source provenance.

---

# 32. Segmentation Ownership

Sentence/semantic segmentation belongs to:

```text
Text Processing
```

Detailed rules are defined by:

```text
01-architecture/text/SEGMENTATION.md
```

Segmentation does not create TranslationUnit.

---

# 33. Span

Span represents a continuous range sharing relevant properties.

Possible reasons to split:

```text
language change

script change

source mapping change

style hint

annotation

semantic role

confidence group
```

Do not create unnecessary spans.

---

# 34. Token

Token is an optional linguistic processing unit.

Possible token kinds:

```text
Word

Character

Punctuation

Number

Symbol

Subword

Whitespace

Unknown
```

Tokenization should be lazy unless required.

---

# 35. Recognition Word vs Text Token

Recognition/OCR word and Text Token are different concepts.

Recognition word is influenced by:

```text
OCR engine

visual geometry

provider segmentation
```

Text Token is influenced by:

```text
language

tokenizer

processing purpose
```

Therefore:

```text
Recognition Word
    ≠
Text Token
```

---

# 36. CJK Tokenization

Example source:

```text
我喜欢看漫画
```

may be tokenized as:

```text
我 | 喜欢 | 看 | 漫画
```

or:

```text
我 | 喜 | 欢 | 看 | 漫 | 画
```

depending on the linguistic operation.

Text Model must not hardcode OCR provider word boundaries as linguistic token boundaries.

---

# 37. Character Range

Sentence, Span, Token, Annotation and Source Mapping may use ranges.

Recommended range semantics:

```text
[start, end)
```

where `end` is exclusive.

The convention must be globally consistent inside Text Model contracts.

---

# 38. Unicode Indexing

Every text range contract must define its indexing unit.

Possible units include:

```text
Unicode Code Point

Grapheme Cluster

UTF-16 Code Unit

UTF-8 Byte Offset
```

Do not leave the unit implicit.

---

# 39. Recommended Internal Range Unit

Prefer:

```text
Unicode Code Point
```

or a well-defined grapheme-aware abstraction for semantic text operations.

UTF-16 conversion should occur at UI/platform boundaries where necessary.

Do not use UTF-8 byte offset as the default linguistic index.

---

# 40. Grapheme Awareness

Never assume:

```text
1 displayed character
=
1 byte
```

or:

```text
1 displayed character
=
1 UTF-16 code unit
```

Combining marks, emoji, variation selectors and complex graphemes must remain valid.

---

# 41. Text Layers

Text Model distinguishes source-language representations.

Recommended conceptual layers:

```text
Raw Text?
    ↓
Source Text
    ↓
Normalized Text
    ↓
Corrected Text?
```

`Display Text` should not be a core authoritative source-language layer by default.

---

# 42. Raw Text

Raw Text is the nearest retained representation of source/provider text before canonical normalization.

For OCR-derived content it may preserve provider-normalized recognition output for:

```text
debugging

benchmarking

correction comparison
```

Raw Text should normally have limited retention.

---

# 43. Source Text

Source Text is the source-language text accepted from the upstream semantic input.

It must preserve:

```text
source meaning

important punctuation

important script distinctions

source provenance
```

It must not include translation.

---

# 44. Normalized Text

Normalized Text may apply semantics-preserving transformations such as:

```text
Unicode normalization

whitespace normalization

newline normalization

invalid control cleanup

safe punctuation compatibility normalization

known encoding-artifact cleanup
```

---

# 45. Normalization Must Not

Default normalization must not:

```text
translate text

change terminology

rewrite grammar

guess uncertain Recognition errors

convert Simplified ↔ Traditional Chinese automatically

silently delete meaningful punctuation
```

---

# 46. Corrected Text

Corrected Text represents an approved modification of source-language content.

Potential sources:

```text
confirmed user correction

approved automatic correction

OCR correction workflow

explicit rule-based correction
```

Correction must preserve provenance.

---

# 47. Correction Record

A correction should carry enough information to explain:

```text
target

previous value

new value

actor/source

reason

base version

timestamp

confidence/status where applicable
```

Do not silently overwrite source lineage.

---

# 48. Canonical Source Text

Recommended precedence:

```text
Confirmed Corrected Text
        ↓
Normalized Text
        ↓
Source Text
```

Raw Text is diagnostic/provenance material.

Presentation-formatted text is not canonical source-language truth.

---

# 49. Display Text Ownership

The v1 Text Model included:

```text
Display Text
```

as a Text Model layer.

In v2, layout-specific display transformation belongs primarily to:

```text
Presentation
```

Text Processing may preserve:

```text
source style hints

semantic whitespace

ruby annotations
```

but should not own final display formatting.

---

# 50. No Translation Inside SourceDocument

`SourceDocument` must not use:

```text
translatedText
```

as embedded current truth.

Translation is a separate module-owned Artifact.

---

# 51. Translation Association

SourceDocument must expose stable source identities/ranges sufficient for Translation to build explicit alignment.

Conceptually:

```text
SourceDocument Node(s)
        ↕
TranslationUnit source references
        ↕
TranslationArtifact
```

Translation owns the Translation-side relationships.

---

# 52. Source Reference

SourceReference links a Text Node/range to its originating source.

A generic SourceReference may contain:

```text
SourceKind

SourceArtifactRef / source identity

SourceEntityId / locator

SourceEntityType

SourceRange?

GeometryRef?

MappingType

Confidence?

Metadata?
```

The exact schema belongs to Text Processing contracts.

---

# 53. Source Kinds

Possible source families:

```text
RecognitionArtifact

StructuredText

HTML

PlainText

UserInput

DocumentTextLayer

FutureImport
```

Avoid coupling canonical contracts to legacy `OCR Document` as the only source type.

---

# 54. Recognition-Derived Reference

For Recognition-derived text, SourceReference may point back to:

```text
RecognitionArtifactId

Recognition block/region identity

line/word identity where exposed

geometry reference
```

Detailed OCR internals remain subordinate to Recognition.

---

# 55. Structured-Source Reference

For structured sources, references may use:

```text
source locator

semantic block identity

text offsets

chapter/section locator

adapter-provided stable identity
```

No geometry is required when it has no semantic purpose.

---

# 56. Many-to-Many Mapping

Source mapping must support:

```text
N source entities
        ↕
M text nodes/ranges
```

because reconstruction may:

```text
merge lines

split bubbles

combine regions

split one paragraph

combine multiple structured blocks
```

Never assume 1:1 mapping.

---

# 57. Mapping Type

Useful mapping categories may include:

```text
Exact

Merged

Split

Derived

Approximate

Manual

Synthetic

Unknown
```

---

# 58. Source Mapping Index

A SourceDocument may maintain an index for efficient bidirectional lookup.

Conceptually:

```text
Source Entity
    → Text Nodes/Ranges
```

and:

```text
Text Node/Range
    → Source References
```

---

# 59. Geometry Reference

Text Processing should normally reference geometry rather than duplicate the full geometric model.

Geometry authority remains with the upstream source Artifact/Recognition semantics.

Possible references:

```text
GeometryId

BoundingBoxRef

PolygonRef

CoordinateSpace

SourceArtifactRef
```

---

# 60. Coordinate Space

Every geometry reference must make coordinate space explicit.

Examples:

```text
source-image pixels

capture coordinates

normalized image coordinates

page coordinates

viewport coordinates
```

Do not mix coordinate systems without transformation metadata.

---

# 61. Order Model

Every ordered node scope must have deterministic ordering.

Possible fields:

```text
OrderIndex

PreviousNodeId?

NextNodeId?

SourceOrderRef?
```

Order must not depend on:

```text
hash-map iteration

provider response accident

parallel execution completion order
```

---

# 62. Global Source Order

SourceDocument should allow deterministic traversal of main semantic content.

Example:

```text
Document
    ↓
Sections
    ↓
Blocks
    ↓
Paragraphs
```

Linguistic subnodes may be traversed as available.

Global linear traversal must not destroy hierarchical relationships.

---

# 63. Order Provenance

Order may come from:

```text
Recognition reading hints

detailed OCR Reading Order

structured source order

Text Processing reconstruction

manual correction
```

Source of the order decision should remain explainable where useful.

---

# 64. Auxiliary Content

Content outside the main reading flow may be retained as auxiliary.

Examples:

```text
SFX

watermark

UI text

advertisement

decorative text

metadata text

footnotes
```

Auxiliary content should retain provenance.

---

# 65. Auxiliary Does Not Mean Deleted

Auxiliary content may still support:

```text
Presentation

manual inspection

Search

future Translation

export options
```

depending on policy.

---

# 66. Relationship Model

SourceDocument may support non-hierarchical semantic relationships.

Possible examples:

```text
continues

references

annotation_of

visually_near

derived_from

alternative
```

Relationships requiring deeper downstream semantics should be owned by the appropriate downstream domain.

---

# 67. Translation Relationship Boundary

Do not store:

```text
translation_of
```

as the sole authoritative Translation relationship inside SourceDocument.

Translation-specific alignment belongs to Translation.

Text Model may expose stable source references that enable it.

---

# 68. Speaker Relationship Boundary

Speaker inference is not mandatory Text Model authority.

Possible speaker information should initially remain:

```text
Annotation
or
low-confidence semantic hint
```

unless a future Knowledge/Dialogue owner is created.

---

# 69. Annotation

Annotation attaches auxiliary semantic information without mutating canonical text.

Examples:

```text
proper-name candidate

term candidate

speaker candidate

emphasis

uncertainty

ruby/furigana

correction proposal

glossary match hint
```

---

# 70. Annotation Integrity

Annotation should identify:

```text
target NodeId

range where applicable

type

value/reference

source

confidence

metadata
```

It must not encode semantic annotations directly into source strings.

---

# 71. Style Hints

Text Model may preserve source-style hints useful downstream.

Examples:

```text
emphasis

bold/italic source hint

handwritten

source font-size estimate

source color

vertical writing

ruby

decorative
```

These are hints, not final Presentation styles.

---

# 72. Language Metadata

Language metadata may exist at multiple scopes:

```text
Document

Section

Block

Paragraph

Sentence

Span

Token
```

where useful.

---

# 73. Language Metadata Fields

Possible fields:

```text
LanguageTag

Script

Confidence

DetectionSource

Inherited?

Mixed?
```

---

# 74. Language Inheritance

Child nodes may inherit a parent language.

Example:

```text
Document = zh-Hans

Paragraph = inherited zh-Hans

Span = en
```

This avoids redundant data while supporting mixed-language content.

---

# 75. Language Tags

Prefer standard language identifiers such as:

```text
BCP 47
```

Examples:

```text
vi

en

zh-Hans

zh-Hant

ja

ko
```

Do not invent arbitrary CRAI-specific language codes unless unavoidable.

---

# 76. Script Metadata

Useful script identifiers may include:

```text
Latin

Han

Hiragana

Katakana

Hangul

Arabic

Cyrillic

Mixed

Unknown
```

Prefer standards-based representation where practical.

---

# 77. Direction Metadata

Text nodes may retain source-writing information such as:

```text
writing mode

text direction

line direction

block direction

rotation
```

For Recognition-derived input, reuse authoritative Recognition/Text Direction results.

Do not infer geometry direction twice without reason.

---

# 78. Confidence Model

Avoid one undifferentiated confidence value.

Possible dimensions:

```text
RecognitionConfidence

OrderConfidence

BoundaryConfidence

LanguageConfidence

MappingConfidence

NormalizationConfidence
```

A high recognition confidence does not imply high sentence-boundary confidence.

---

# 79. Quality Flags

Possible flags:

```text
LowRecognitionConfidence

AmbiguousOrder

UnknownLanguage

BrokenMapping

MissingSource

SuspectedDuplicate

IncompleteText

ManualReviewRequired
```

Flags describe uncertainty.

They must not silently modify text.

---

# 80. Normalization Pipeline

Conceptually:

```text
Accepted Source Text
    ↓
Unicode Validation
    ↓
Unicode Normalization
    ↓
Whitespace Analysis
    ↓
Line-Break Analysis
    ↓
Control-Character Cleanup
    ↓
Safe Punctuation Normalization
    ↓
Normalized Text
```

Exact enabled operations depend on Text Processing policy/profile.

---

# 81. Unicode Normalization Policy

Text Processing must explicitly choose normalization policy.

Possible choices include:

```text
NFC

NFKC in explicitly compatible scenarios
```

NFC is generally safer for preserving source distinctions.

The applied normalization policy/version must be recorded.

---

# 82. Whitespace

Whitespace normalization must consider source semantics.

Do not blindly collapse whitespace in:

```text
code

ASCII art

mathematical expression

intentional formatting

certain imported text
```

---

# 83. Recognition Line Breaks

Recognition line breaks often represent visual layout rather than linguistic paragraph/sentence boundaries.

Example:

```text
我真的
不知道
怎么办。
```

may normalize to a continuous linguistic unit while preserving original source mappings.

---

# 84. Latin Hyphenation

Line-end hyphenation may be reconstructed only when sufficiently supported.

Example:

```text
trans-
lation
```

may become:

```text
translation
```

with a Normalization Record/provenance.

---

# 85. CJK Handling

For Chinese/Japanese/Korean:

```text
do not rely on whitespace for word boundaries

do not insert spaces between Recognition words mechanically

preserve full-width punctuation semantics

preserve vertical/horizontal source hints
```

---

# 86. Simplified and Traditional Chinese

Default source normalization must preserve:

```text
zh-Hans
```

and:

```text
zh-Hant
```

as source distinctions.

Simplified/Traditional conversion is not generic normalization.

---

# 87. Punctuation Preservation

Preserve meaningful source punctuation such as:

```text
…

!?

—

repeated punctuation

CJK quotation marks

elongation marks
```

unless an explicit semantics-preserving normalization rule applies.

---

# 88. Ruby and Furigana

Ruby/furigana should remain structured.

Example:

```text
BaseText = 漢字

Ruby = かんじ
```

Do not flatten them into an ambiguous string by default.

---

# 89. Empty Nodes

Empty/whitespace nodes may result from:

```text
Recognition failure

decorative region

source markup

incomplete mapping
```

Possible treatment:

```text
omit from main flow

retain with quality flag

move to auxiliary

record diagnostic observation
```

Do not create meaningless empty linguistic hierarchies.

---

# 90. Duplicate Text

Do not remove duplicated strings based solely on text equality.

Two different source Blocks may legitimately contain identical content.

Duplicate analysis should consider:

```text
source identity

mapping

geometry/locator

relationships

content provenance
```

---

# 91. Synthetic Nodes

Synthetic nodes may be created when needed to maintain a usable structure.

Examples:

```text
SyntheticSection

SyntheticBlock

SyntheticParagraph

ProvisionalSentence
```

They must carry explicit provenance such as:

```text
isSynthetic

reason

source

confidence
```

---

# 92. Orphan Handling

Unmapped content must not disappear silently.

Possible outcomes:

```text
OrphanSection

AuxiliaryContent

Invalid flag

ManualReviewRequired
```

---

# 93. Structural Validation

Validate at least:

```text
unique IDs in scope

valid parent references

no parent-child cycles

valid ordering

valid ranges

valid source references

valid language tags

deterministic canonical text

mapping target existence
```

---

# 94. Mapping Validation

For source mappings validate:

```text
source exists

range is valid

mapping type is valid

coordinate space is known when geometry exists

confidence is valid
```

---

# 95. Canonical Text Validation

Every translatable/searchable source node must be able to resolve canonical source text according to the Text Processing contract.

A node without canonical text may still exist as structural/auxiliary metadata.

---

# 96. Source Mapping Coverage

Recognition-derived text should strive for very high mapping coverage.

Ideal target:

```text
close to 100%
```

for source-derived content where the upstream source exposes stable identity.

User-created text may instead reference:

```text
UserInput
```

or another explicit source type.

---

# 97. Explainability

Text Processing should be able to explain important transformations.

Examples:

```text
which source entities produced this Block?

why were two lines merged?

why was content classified Auxiliary?

what normalization changed this string?

which segmentation operation created this boundary?
```

---

# 98. Normalization Record

Meaningful normalization changes should be traceable.

A record may capture:

```text
operation

before

after

range

rule/version

confidence

reason
```

Exact record schema belongs to Text Processing contracts.

---

# 99. Correction vs Normalization

Normalization:

```text
preserves semantic source content
```

Correction:

```text
changes accepted source representation
based on explicit evidence/approval
```

These must remain distinguishable.

---

# 100. Immutable Published Model

A Published SourceDocumentArtifact is immutable.

Do not mutate it in place for:

```text
new segmentation

manual correction

new source content

new semantic structure
```

Create a new Candidate/Artifact revision as defined by Text Processing.

---

# 101. Incremental Reconstruction

Text Processing may optimize rebuilding by recomputing only affected scopes.

Examples:

```text
one Recognition Block corrected

one Paragraph boundary changed

one source block updated

one ordering relationship changed
```

Optimization must produce a coherent new immutable result.

---

# 102. Incremental Does Not Mean Mutable Publication

Internal incremental computation is allowed.

Public Artifact semantics remain:

```text
Candidate
    ↓
validation
    ↓
Published immutable Artifact
```

---

# 103. Node Identity Stability

Node IDs should remain stable when a node still represents the same logical semantic entity.

Changing one character does not necessarily require a new BlockId.

Changing a structural identity completely may.

---

# 104. Node ID Must Not Depend Solely On

Avoid identities derived only from:

```text
OrderIndex

RuntimeRevisionId

AttemptId

memory address

provider array index

content hash
```

These values do not define semantic identity reliably.

---

# 105. Content Hash

Content hash may assist:

```text
cache lookup

change detection

deduplication

Translation reuse
```

It should define its inputs explicitly, such as:

```text
canonical text

language

node kind

normalization version
```

Hash is not semantic Node identity.

---

# 106. Serialization

SourceDocument must be serializable in a stable representation.

Serialization must preserve:

```text
identity

ordering

Unicode

source mapping

semantic versions

relationships required by the contract
```

---

# 107. Serialization Format

Possible physical formats:

```text
JSON

MessagePack

Protobuf

database representation
```

Logical semantics must not depend on one serialization technology.

---

# 108. No Runtime Objects

Serialized/public Text Model must not contain:

```text
thread

mutex

callback

closure

provider SDK response

native DOM node

native window handle

database connection
```

---

# 109. Compact Representation

Compact physical representation may:

```text
omit heavy Diagnostics

omit Raw Text

compress metadata

use reference tables

lazy-load Tokens
```

It must preserve required semantic contract information.

---

# 110. Full Representation

A full/debug representation may retain more:

```text
Raw Text

detailed confidence

source mappings

normalization records

revision history

diagnostic references
```

Privacy/retention rules still apply.

---

# 111. Lazy Construction

Recommended general principle:

```text
Document / core Blocks
    → construct when needed for SourceDocument

Paragraph
    → construct according to profile/source

Sentence
    → provisional or segmented

Span
    → on demand

Token
    → lazy
```

Do not tokenize whole long documents unless required.

---

# 112. Large Documents

For long novels/webtoons:

```text
bounded document windows

paged/lazy loading

incremental SourceDocument scopes
```

may be required.

Text Model must not force the whole book into memory.

---

# 113. Text Profile

A Text Processing profile may configure:

```text
document/source type

normalization policy

structural granularity

language policy

auxiliary-content policy

source-mapping policy

initial segmentation policy

lazy construction policy
```

---

# 114. Profile Is Not User Preference Authority

Text Profile is an effective Text Processing configuration snapshot.

Persistent preference authority remains:

```text
Preferences
```

Session overrides remain:

```text
Reading Session
```

Application resolves effective configuration.

---

# 115. Comic Profile

Likely priorities:

```text
Panel/Block grouping

Bubble semantics

geometry/source mapping

Main/Auxiliary separation

direction/order preservation
```

---

# 116. Novel Profile

Likely priorities:

```text
chapter/section structure

paragraph fidelity

header/footer isolation

linguistic segmentation readiness

semantic source locators
```

---

# 117. Webtoon Profile

Likely priorities:

```text
continuous ordering

virtual ranges

geometry mapping

incremental processing

bounded memory
```

---

# 118. Plain Text Profile

Likely priorities:

```text
paragraphs

source offsets

language metadata

segmentation readiness
```

Geometry is unnecessary by default.

---

# 119. SourceDocument Builder

A Text Processing implementation may contain a builder.

Conceptually:

```text
Normalized Input
    ↓
Structural Reconstruction
    ↓
Source Mapping
    ↓
Normalization
    ↓
Optional Initial Segmentation
    ↓
Validation
    ↓
Candidate SourceDocument
```

The Builder is an implementation component, not an architecture owner.

---

# 120. Builder Contract Boundary

Do not promote internal operations such as:

```text
MapStructure

NormalizeText

CreateSourceMapping
```

into global Event Bus events or Runtime stages automatically.

They may remain internal functions/suboperations.

---

# 121. Runtime Relationship

Runtime may execute a logical Text Processing WorkItem.

Conceptually:

```text
TextProcessing WorkItem
    ↓
Attempt
    ↓
Text Processing operation
    ↓
Candidate SourceDocumentArtifact
```

Runtime owns WorkItem/Attempt state.

Text Processing owns semantic result validity.

---

# 122. No Builder State Machine

Deprecated architecture:

```text
Created
→ Building
→ Mapping
→ Normalizing
→ Validating
→ Ready
```

as a public Text Model state machine.

These are implementation phases.

Module lifecycle belongs to:

```text
02-modules/text-processing/STATES.md
```

Runtime operation lifecycle belongs to Runtime.

---

# 123. No Build Command Events

Deprecated:

```text
TextDocumentBuildRequested

TextDocumentBuildStarted

TextStructureMapped

TextNormalized

SourceMappingCreated

TextDocumentValidated

TextDocumentCompleted

TextDocumentFailed
```

as a generic Event Bus execution chain.

Exact canonical module events belong to:

```text
02-modules/text-processing/EVENTS.md
```

and must represent committed facts.

---

# 124. Artifact Publication Event

If Text Processing defines a publication event, its semantics should correspond to:

```text
Published SourceDocumentArtifact
```

not internal builder progress.

Exact event name remains module-owned.

---

# 125. Error Ownership

Exact Text Processing errors belong to:

```text
02-modules/text-processing/ERRORS.md
```

This architecture document may describe failure categories but must not create a competing error taxonomy.

---

# 126. Useful Failure Categories

Conceptual categories include:

```text
invalid source input

invalid structure

source mapping failure

invalid text range

unsupported source/document form

invalid language metadata

normalization failure

serialization failure

revision/version conflict
```

Use exact Text Processing error codes/contracts elsewhere.

---

# 127. Diagnostics

Text Model-related diagnostic observations may include:

```text
missing source mapping

invalid range

unknown language

normalization change

synthetic node creation

orphan node

order mismatch

low boundary confidence

unsupported script

mapping coverage
```

Diagnostics observes these without owning Text Processing semantics.

---

# 128. Statistics

Derived statistics may include:

```text
BlockCount

ParagraphCount

SentenceCount

TokenCount

CharacterCount

LanguageDistribution

MainAuxiliaryCount

LowConfidenceNodeCount

MappingCoverage
```

Statistics are derived data, not source truth.

---

# 129. Translation Compatibility

Translation's canonical input is:

```text
Published SourceDocumentArtifact
```

Translation may query or project from it:

```text
Blocks

Paragraphs

Sentences

Spans

source ranges

language metadata

annotations

source mapping
```

according to Translation contracts.

---

# 130. TranslationUnit Boundary

Deprecated:

```text
Text Segmentation
    ↓
Context Assembly
    ↓
Translation
```

as if Text Processing creates Translation-ready units/context.

Current:

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

Translation owns those concepts.

---

# 131. Translation Alignment

Text Model must expose stable source references so Translation can construct:

```text
1:1

1:N

N:1

N:M
```

source-to-translation alignment.

The alignment authority belongs to Translation.

---

# 132. Presentation Compatibility

Presentation should normally consume:

```text
TranslationArtifact
```

rather than Text Model as its primary translated-content authority.

It may reference SourceDocument/Recognition provenance as required for:

```text
source geometry

source structure

original text display
```

through explicit Artifact references.

---

# 133. No Text Model → Presentation Ownership Shortcut

Deprecated implication:

```text
Text Model
    stores translated reference/display text
    and directly determines Presentation
```

Current boundary:

```text
SourceDocumentArtifact
    ↓
TranslationArtifact
    ↓
PresentationArtifact
```

---

# 134. Search Compatibility

Future Search may index:

```text
canonical source text

corrected source text

language

document metadata

node kinds
```

Translation search should use TranslationArtifact or an explicit combined search projection.

Do not put Translation truth back into SourceDocument.

---

# 135. Export Compatibility

Export may combine multiple owner Artifacts.

Example:

```text
SourceDocumentArtifact
+
TranslationArtifact
    ↓
Bilingual Export
```

Text Model alone does not own translated export truth.

---

# 136. Source Geometry and Presentation

For image-derived content:

```text
SourceDocument Node
    ↓
SourceReference
    ↓
Recognition/Capture geometry
```

allows Presentation to map Translation output back to source location.

Geometry authority is not transferred into Text Processing.

---

# 137. Structured Source Without Geometry

For browser novels/plain text:

```text
SourceReference
```

may use semantic locators/ranges instead of geometry.

Therefore geometry must remain optional in the generic Text Model.

---

# 138. Deterministic Output

Given equivalent:

```text
source semantic input

Text Processing profile

normalization version

segmentation/reconstruction version
```

Text Processing should produce deterministic semantic structure where algorithms are deterministic.

Runtime timing must not change semantic output identity.

---

# 139. Provider Independence

SourceDocument must not expose:

```text
PaddleOCR DTO

Google Vision DTO

browser DOM object

Translation provider DTO
```

as canonical fields.

Provider/source adapters normalize first.

---

# 140. Runtime Independence

Text Model must not embed:

```text
WorkItem mutable state

Attempt lifecycle state

Scheduler priority

cancellation token object

queue handle
```

Runtime provenance may be referenced through immutable identifiers when appropriate at Artifact level.

---

# 141. Versioning

The architecture should distinguish at least:

```text
SourceDocumentContractVersion

TextProcessingImplementationVersion?

NormalizationPolicyVersion

TextProfileVersion

SegmentationPolicyVersion where relevant

Source input schema/version
```

Only semantically relevant versions should participate in compatibility/cache decisions.

---

# 142. Breaking Contract Change

Increment SourceDocument contract/schema version when changes alter:

```text
required fields

node semantics

range semantics

mapping semantics

identity semantics

serialization compatibility

required hierarchy interpretation
```

---

# 143. Migration

Migration may:

```text
add defaults

convert node kinds

rebuild mapping indexes

convert range convention

preserve stable IDs where meaningful
```

Migration must not silently discard:

```text
Source Text

confirmed corrections

source provenance
```

---

# 144. Cache Compatibility

Text Processing cache identity may depend on:

```text
source semantic fingerprint

source Artifact/schema version

Text Profile

normalization version

reconstruction/segmentation policy version

confirmed correction state
```

Do not hardcode cache identity only around legacy:

```text
OCR Document ID
Reading Order Version
```

because structured sources exist too.

---

# 145. Cache Is Not Authority

A cached SourceDocumentArtifact still requires:

```text
compatibility validation

current Runtime authority validation
```

before it becomes current output.

---

# 146. Correction and Cache

A confirmed source correction may invalidate:

```text
SourceDocument cache

Translation cache

Presentation derived output
```

depending on affected semantic scope.

Invalidation consequences are coordinated through owner contracts/Runtime, not mutated backward through Artifacts.

---

# 147. Privacy

SourceDocument may contain sensitive reading content.

Default principles:

```text
do not log full text

retain only according to policy

protect persisted content

minimize remote transmission

allow deletion/retention control where required
```

---

# 148. Raw Text Retention

Raw provider/Recognition text should have shorter retention than canonical source-language Artifacts unless needed for:

```text
debugging

benchmark

manual correction

explicit audit
```

---

# 149. Security

When importing/deserializing SourceDocument-like data:

```text
validate size

validate nesting depth

validate IDs

validate ranges

reject cycles

validate source refs

treat content as data, never executable code

escape appropriately during export
```

---

# 150. Performance

Text Model must support:

```text
small comic bubbles

long chapters

long webtoon ranges

large mixed-language documents

incremental processing

browser/desktop environments
```

Do not require eagerly materializing every linguistic node.

---

# 151. Memory Optimization

Possible implementation techniques:

```text
string interning/table

reference tables

compact IDs

lazy diagnostics

mapping index separation

paged loading

immutable structural sharing
```

These must not alter logical contract semantics.

---

# 152. Testing Strategy

Coverage should include:

```text
comic LTR

manga RTL

Chinese manhua

Korean webtoon

structured Chinese novel

plain text

mixed language

vertical source text

multiple sentences in one Bubble

sentence across recognition lines

merged source regions

manual correction

incremental reconstruction

auxiliary text

missing mapping

structured source without geometry
```

---

# 153. Golden Tests

Golden tests should cover both source families.

## Recognition-Derived

Inputs may include:

```text
RecognitionArtifact fixture

Text Profile
```

Expected:

```text
SourceDocument structure

canonical text

source mapping

quality metadata
```

## Structured Source

Inputs may include:

```text
StructuredSource fixture

Text Profile
```

Expected the same canonical semantic output categories.

---

# 154. Round-Trip Mapping Test

For Recognition-derived content verify:

```text
Recognition Entity
    ↓
SourceDocument Node
    ↓
SourceReference
    ↓
Recognition Entity
```

For structured input verify:

```text
Source Locator/Range
    ↓
SourceDocument Node
    ↓
SourceReference
    ↓
Source Locator/Range
```

---

# 155. Geometry Round-Trip

Where geometry exists:

```text
SourceDocument Node
    ↓
GeometryRef
    ↓
source geometry
```

must remain valid across serialization and publication.

---

# 156. Incremental Stability Test

Given a small source change, verify:

```text
unaffected logical Node identities remain stable where appropriate

affected semantic scopes update

new SourceDocumentArtifact is coherent

old Artifact remains immutable
```

---

# 157. Unicode Tests

Test at minimum:

```text
CJK punctuation

Simplified Chinese

Traditional Chinese

combining accents

emoji

variation selectors

surrogate-pair UI conversion

ruby/furigana

mixed scripts
```

---

# 158. Architecture Invariants

1. Text Processing owns Text Model semantics.

2. `SourceDocumentArtifact` is the public cross-module boundary.

3. Text Model is not OCR-only.

4. RecognitionArtifact is the visual-text input boundary.

5. Structured source input may bypass Capture/Recognition.

6. Visual and structured inputs converge inside Text Processing.

7. TranslationUnit does not belong to Text Processing.

8. TranslationBatch does not belong to Text Processing.

9. Translation context assembly does not belong to Text Processing.

10. SourceDocument never stores translated text as its source of truth.

11. Source Text remains source-language data.

12. Confirmed corrections preserve history/provenance.

13. Normalization does not silently change meaning.

14. Simplified/Traditional conversion is not default normalization.

15. Source mapping supports many-to-many relationships.

16. Geometry is referenced, not re-owned.

17. Geometry is optional for non-visual sources.

18. Source-native/provider-native DTOs do not cross the boundary.

19. Text ranges use one explicit indexing convention.

20. Node order is deterministic.

21. Runtime completion does not imply SourceDocumentArtifact publication.

22. Published SourceDocumentArtifact is immutable.

23. Runtime owns WorkItem/Attempt execution.

24. Text Model does not expose Runtime execution state.

25. Internal Builder phases are not module states.

26. Internal Builder phases are not Event Bus command chains.

27. Exact module events belong to `text-processing/EVENTS.md`.

28. Exact module errors belong to `text-processing/ERRORS.md`.

29. Cache is optimization, not authority.

30. Serialization preserves semantic identity/order/mapping.

31. Large documents may use lazy/incremental construction.

32. Current SourceDocument contract remains provider-independent.

---

# 159. Deprecated v1 Concepts

The following v1 assumptions are deprecated as current architecture:

```text
OCR Document
    ↓
Reading Order
    ↓
Text Model Builder
```

as the only Text Model input path.

Also deprecated:

```text
Text Document
    ↓
Text Segmentation
    ↓
Context Assembly
    ↓
Translation
```

where Context Assembly appears Text Processing-owned.

And:

```text
TextDocumentBuildRequested
TextDocumentBuildStarted
TextStructureMapped
TextNormalized
TextDocumentCompleted
TextDocumentFailed
```

as execution-control Event Bus events.

---

# 160. Preserved v1 Strengths

The following v1 concepts are intentionally retained:

```text
hierarchical source-text model

structural vs linguistic nodes

Source/Normalized/Corrected text separation

SourceReference

many-to-many mapping

geometry references

deterministic order

Unicode-safe ranges

language/script metadata

annotation separation

confidence dimensions

auxiliary content

synthetic/orphan handling

correction provenance

stable Node identity

incremental processing

serialization

versioning

migration

privacy/security

lazy construction

deterministic testing
```

---

# 161. Relationship to SEGMENTATION.md

`SEGMENTATION.md` refines:

```text
Paragraph / Sentence / semantic boundary construction
```

inside Text Processing.

It may transform provisional linguistic structure into a newer SourceDocument Candidate.

It must not create TranslationUnit authority.

---

# 162. Relationship to Translation

Translation consumes:

```text
Published SourceDocumentArtifact
```

and owns:

```text
TranslationUnit

TranslationBatch

context assembly

TranslationArtifact
```

---

# 163. Relationship to Presentation

Presentation consumes:

```text
Published TranslationArtifact
```

and may follow references back to:

```text
SourceDocumentArtifact

RecognitionArtifact

CaptureArtifact
```

where source geometry/original text is required.

---

# 164. Relationship to DATA_FLOW.md

`core/DATA_FLOW.md` owns architecture-wide Artifact movement.

This file owns the semantic model inside the Text Processing boundary.

---

# 165. Relationship to OCR Architecture

Detailed OCR documents may define:

```text
Region

OCR reading order

layout

direction

postprocessing
```

Recognition normalizes/exposes the public RecognitionArtifact contract.

Text Processing must not depend on provider-specific OCR internals.

---

# 166. Related Documents

```text
doc/01-architecture/core/
├── DATA_FLOW.md
├── STATE_MACHINE.md
└── EVENT_CONVENTION.md

doc/01-architecture/ocr/
├── RECOGNITION.md
├── LAYOUT.md
├── READING_ORDER.md
└── POSTPROCESS.md

doc/01-architecture/text/
├── TEXT_MODEL.md
└── SEGMENTATION.md

doc/02-modules/
├── recognition/
├── text-processing/
├── translation/
└── presentation/
```

---

# 167. Open Decisions

The following remain open:

```text
final SourceDocument schema

whether Page is required or optional

final Node identity policy

Unicode Code Point vs Grapheme-based semantic ranges

exact structured-source input contract

exact incremental Artifact strategy

whether segmentation produces one new Artifact
or a staged internal Candidate

persistent correction domain design

semantic locator format

cross-page/cross-bubble sentence model

speaker/character ownership

Knowledge module boundary

annotation extension policy

partial-document publication model
```

---

# 168. Completion Criteria

This Text Model is synchronized when:

* Text Model is owned by Text Processing;
* SourceDocumentArtifact is the public boundary;
* OCR is no longer the only source path;
* RecognitionArtifact replaces legacy OCR Document as public visual input;
* structured text can enter Text Processing directly;
* Text Processing ends at SourceDocumentArtifact;
* TranslationUnit/Batch/context assembly are absent from Text Processing ownership;
* translated text is not embedded as SourceDocument truth;
* Presentation display formatting is no longer Text Model authority;
* Source Mapping remains many-to-many;
* Unicode/range conventions remain explicit;
* corrections preserve source provenance;
* internal Builder phases are not public state machine stages;
* BuildRequested/Started/Completed events are removed as command/execution flow;
* cache/versioning support structured and visual inputs;
* Candidate → Published authority matches Runtime v2.

---

# 169. Summary

CRAI v1 modeled Text Model mainly as:

```text
OCR Document
    ↓
Reading Order
    ↓
Text Model Builder
    ↓
Text Document
    ↓
Segmentation
    ↓
Context Assembly
    ↓
Translation
```

CRAI v2 models:

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

which produces:

```text
Candidate SourceDocumentArtifact
    ↓
Authority Validation
    ↓
Published SourceDocumentArtifact
```

Then:

```text
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationUnit / TranslationBatch
    ↓
TranslationArtifact
    ↓
PresentationArtifact
```

The central rule is:

```text
Text Processing owns
source-language structure.

Translation owns
translation preparation and output.

Runtime owns execution.

SourceDocumentArtifact
is the stable boundary
between them.
```
