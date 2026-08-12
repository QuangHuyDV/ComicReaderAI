# CRAI Text Segmentation

> **Project:** CRAI
> **Path:** `doc/01-architecture/text/SEGMENTATION.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Architecture Owner:** Text Processing
> **Parent Model:** `TEXT_MODEL.md`
> **Public Artifact:** `SourceDocumentArtifact`
> **Runtime Model:** Runtime v2 aligned
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines how CRAI Text Processing identifies and maintains meaningful linguistic and structural boundaries inside source-language content.

Segmentation operates on canonical Text Model structures such as:

```text
Block
Paragraph
Provisional Sentence
Span
Source Reference
```

and may refine them into:

```text
stable Sentence boundaries
semantic source ranges
continuation relationships
boundary metadata
segmentation annotations
```

while preserving:

```text
source meaning
paragraph relationships
dialogue boundaries
source ordering
source mappings
layout/source references
language metadata
uncertainty
manual overrides
```

---

# 2. Central Ownership Rule

Text Segmentation belongs to:

```text
Text Processing
```

It owns:

```text
source-language boundary decisions
source-language structural grouping
sentence/paragraph refinement
boundary confidence
continuation hints
segmentation provenance
```

It does not own:

```text
TranslationUnit

TranslationBatch

Translation context assembly

Translation request construction
```

Those belong to Translation.

---

# 3. Architecture Position

Canonical position:

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

Inside Text Processing:

```text
Source Reconstruction
    ↓
Normalization
    ↓
Initial Structure
    ↓
Segmentation
    ↓
Validated SourceDocument Candidate
```

Then:

```text
Candidate SourceDocumentArtifact
    ↓
Authority Validation
    ↓
Published SourceDocumentArtifact
    ↓
Translation
```

---

# 4. Segmentation Is Internal Semantic Refinement

Segmentation is not a top-level pipeline authority.

It is a semantic operation within Text Processing.

Conceptually:

```text
SourceDocument Candidate
    ↓
Segmentation refinement
    ↓
SourceDocument Candidate'
```

The Published cross-module boundary remains:

```text
SourceDocumentArtifact
```

---

# 5. Scope

Segmentation may operate on source text derived from:

```text
RecognitionArtifact

browser/DOM text

novels/web novels

comic speech bubbles

captions

narration boxes

user-selected text

plain-text imports

structured documents

mixed source content
```

Different source types may use different profiles.

---

# 6. Responsibilities

Segmentation may be responsible for:

```text
detecting sentence boundaries

preserving paragraph boundaries

grouping related source fragments

identifying continuation

preserving source order

protecting unsafe split ranges

splitting oversized source structures

merging semantically incomplete fragments

recording segmentation provenance

maintaining source mappings

assigning boundary confidence

supporting manual overrides

supporting incremental re-segmentation
```

---

# 7. Non-Responsibilities

Segmentation does not own:

```text
DOM extraction

Capture

OCR text detection

Recognition

arbitrary OCR correction

TranslationUnit construction

TranslationBatch creation

Translation context assembly

Translation provider selection

Translation result semantics

Presentation layout

physical persistence

Runtime retry

Runtime cancellation

cross-module pipeline orchestration
```

---

# 8. Segmentation Output

Segmentation output is not a standalone Translation Artifact.

It is a refined source-language structure inside:

```text
SourceDocument
```

Possible refined entities include:

```text
Paragraph

Sentence

Span

Boundary metadata

Continuation metadata

Source Mapping

Segmentation Annotation
```

---

# 9. No `Translation-Ready Unit` Ownership

Deprecated wording:

```text
Segmentation produces translation-ready units.
```

Preferred:

```text
Segmentation produces
stable source-language structure
that Translation can consume.
```

Translation decides how that structure becomes TranslationUnits.

---

# 10. Core Concepts

Segmentation uses concepts already defined by `TEXT_MODEL.md`.

Important concepts include:

```text
SourceDocument

Block

Paragraph

Sentence

Span

SourceReference

Annotation

Relationship
```

This file adds boundary-specific semantics.

---

# 11. Source Scope

A Source Scope is the bounded SourceDocument region currently being segmented.

Examples:

```text
one Paragraph

one Block

one comic Panel group

one chapter range

one incremental document window
```

Source Scope is semantic data.

It is not:

```text
WorkItemId

AttemptId

RuntimeRevisionId
```

---

# 12. Boundary

A Boundary represents a possible linguistic/structural split between adjacent source ranges.

A Boundary may carry:

```text
BoundaryType

Strength

Confidence

Source

RuleVersion

ManualOverride?

ProtectedReason?

Warnings?
```

---

# 13. Boundary Strengths

Recommended conceptual strengths:

| Boundary    | Meaning                                           |
| ----------- | ------------------------------------------------- |
| `hard`      | Must normally remain separated                    |
| `strong`    | Separation is strongly preferred                  |
| `soft`      | Separation is allowed when useful                 |
| `protected` | Splitting is prohibited unless recovery forces it |
| `unknown`   | Evidence is insufficient                          |

---

# 14. Hard Boundaries

Examples:

```text
chapter boundary

explicit scene separator

unrelated source container

user-forced split

independent heading
```

For comics, a panel boundary may be hard only when there is no strong continuation evidence.

---

# 15. Strong Boundaries

Examples:

```text
paragraph end

completed dialogue turn

narration → dialogue transition

list-item boundary

speech-bubble boundary
```

Strong does not mean absolute.

Cross-bubble language continuity may justify refinement.

---

# 16. Soft Boundaries

Examples:

```text
sentence ending

clause boundary

line break inside Paragraph

minor punctuation

Recognition line boundary
```

---

# 17. Protected Boundaries

Examples:

```text
inside proper name

inside numeric expression

inside URL

inside identifier

inside paired quotation

inside ellipsis

inside fixed expression

between Recognition lines
known to belong to one sentence
```

---

# 18. Boundary Detection Inputs

Detection may consider:

```text
Unicode punctuation

language-specific punctuation

whitespace

line breaks

paragraph structure

source hierarchy

Recognition geometry

Block type

source order

style changes

quotation state

bracket state

list markers

heading classification

source-adapter hints

language profile

manual overrides
```

No single signal is universally authoritative.

---

# 19. Structural Classification

Before detailed segmentation, Text Processing may classify structures such as:

```text
heading

paragraph

dialogue

narration

caption

list_item

footnote

sound_effect

metadata

unknown
```

Classification uncertainty must remain explicit.

---

# 20. Classification Provenance

A classification may originate from:

```text
structured source semantics

Recognition region type

layout evidence

bubble detection

punctuation

manual correction

language rules

AI-assisted proposal
```

The source of classification should be traceable where useful.

---

# 21. Sentence Segmentation

Sentence segmentation identifies probable linguistic boundaries inside source structures.

It must not be treated as:

```text
split on punctuation
```

only.

Boundary evaluation must consider language and structure.

---

# 22. Chinese Sentence Endings

Common examples:

```text
。
！
？
……
```

Western punctuation may also occur:

```text
.
!
?
...
```

But punctuation alone is insufficient.

---

# 23. Unsafe Chinese/Latin Examples

Avoid blindly splitting:

```text
3.14

www.example.com

第1.5章

A.B.C.

……

？！
```

Language-aware protection rules are required.

---

# 24. Chinese-Specific Requirements

Initial Chinese support should handle:

```text
Simplified Chinese

Traditional Chinese

full-width punctuation

mixed Chinese/Latin text

ellipsis forms

dialogue quotation marks

chapter numbering

names/titles

short omitted-subject clauses

idioms/fixed expressions

vertical source text

sentences across Recognition lines

sentences across speech bubbles

web-novel formatting
```

---

# 25. Quotation Pairs

Examples include:

```text
“ ”

‘ ’

「 」

『 』

《 》

〈 〉

（ ）

【 】
```

Segmentation should normally avoid splitting inside an unclosed paired construct.

Malformed input may require recovery behavior.

---

# 26. CJK Whitespace

Segmentation must not assume whitespace marks word/sentence boundaries in CJK content.

Likewise, absence of whitespace must not prevent linguistic grouping.

---

# 27. Source Script Preservation

Source script distinctions such as:

```text
zh-Hans
zh-Hant
```

must remain intact.

Segmentation must not perform Simplified/Traditional conversion.

---

# 28. Mixed-Language Content

A source range may include:

```text
Chinese + English

Chinese + Japanese

Chinese + numbers

Latin proper names

URLs

code

game stats

identifiers
```

Mixed-language spans should remain identifiable.

---

# 29. Protected Mixed-Language Terms

Example:

```text
打开 Skill Tree，然后选择 Fireball Lv.3。
```

Avoid splitting inside:

```text
Skill Tree
Fireball Lv.3
```

without explicit structural evidence.

---

# 30. Novel Segmentation

Novel segmentation should prioritize:

```text
paragraph fidelity

narrative coherence

dialogue-turn integrity

scene separators

safe long-paragraph splitting

stable source mapping
```

---

# 31. Novel Structure

Conceptually:

```text
Chapter
    ↓
Section
    ↓
Block
    ↓
Paragraph
    ↓
Sentence
```

Clause-level analysis may exist internally when needed.

It does not need to become a permanent SourceDocument node.

---

# 32. Novel Boundary Strategy

Preferred order:

```text
preserve Paragraph boundaries

identify Sentence boundaries

preserve dialogue turns

split oversized Paragraphs safely

retain scene separators

keep headings structurally distinct

preserve stable semantic identities
```

---

# 33. Dialogue Segmentation

Dialogue requires stronger preservation of conversational structure.

Useful concepts may include:

```text
Dialogue Group

Dialogue Turn

Continuation

Narration Link
```

These may be represented as:

```text
Section

Block

Relationship

Annotation
```

depending on final Text Model schema.

---

# 34. Dialogue Turn

A Dialogue Turn may retain:

```text
source order

quotation style

speaker candidate

source Block

continuation status

confidence
```

Speaker certainty is not required.

---

# 35. Speaker Boundary

Segmentation must not merge different likely speakers into one unstructured source string merely to create larger chunks.

If grouping is useful, preserve turn identities structurally.

---

# 36. Comic Segmentation

Comic segmentation relies on:

```text
source geometry

Block/Region membership

bubble identity

source order

direction

language continuity

dialogue continuity
```

while preserving physical source mappings.

---

# 37. Comic Source Hierarchy

Typical Recognition-derived structure may conceptually resemble:

```text
Page
    ↓
Panel
    ↓
Block / Text Region
    ↓
Paragraph
    ↓
Sentence
```

Segmentation should operate on Text Model semantics rather than rebuilding raw OCR hierarchy independently.

---

# 38. Comic Default Boundary

For MVP, one Bubble/independent source Block is a safe default semantic boundary.

Further grouping should require evidence.

---

# 39. Multi-Region Semantic Sentence

A single sentence may span multiple visual regions.

Example:

```text
Bubble A:
如果你现在离开

Bubble B:
就再也别回来！
```

The semantic sentence may be:

```text
如果你现在离开，就再也别回来！
```

while still retaining two distinct source mappings.

---

# 40. Cross-Bubble Representation

Prefer:

```text
Sentence S1
├── SourceReference → Bubble A
└── SourceReference → Bubble B
```

rather than inventing a Translation Segment as Text Processing authority.

Translation may later create one TranslationUnit from S1.

---

# 41. Cross-Bubble Mapping

The source boundaries must remain intact because Presentation may need separate geometric placement even when Translation uses the combined semantic sentence.

---

# 42. Recognition Line Grouping

Recognition may return one sentence as several physical lines.

Example:

```text
我从来没有
见过这样的
事情。
```

Text Processing may reconstruct:

```text
我从来没有见过这样的事情。
```

while preserving source references to all contributing lines.

---

# 43. Visual Line Is Not Semantic Boundary

Line grouping may consider:

```text
Block membership

geometry

orientation

punctuation

style

source order

Recognition confidence

language syntax
```

A visual line break alone is insufficient.

---

# 44. Vertical Text

For vertical content, segmentation should preserve:

```text
orientation

column identity

source references

ordering confidence

manual ordering overrides
```

Linear source text must not destroy the visual provenance.

---

# 45. Reading Order Dependency

Segmentation consumes the best available semantic source ordering.

For visual sources that ordering may originate from Recognition/OCR Reading Order.

For structured text it may originate from source structure.

Segmentation must not silently replace authoritative source order.

---

# 46. Suspicious Ordering

If segmentation detects likely ordering problems:

```text
record warning

produce correction proposal

lower confidence
```

rather than silently mutating upstream authority.

---

# 47. Fragment Grouping

Small fragments may be grouped when they are semantically incomplete.

Examples:

```text
但是……

所以说……

那个……

然后呢？
```

---

# 48. Fragment Grouping Signals

Consider:

```text
structural type

source proximity

source order

punctuation

dialogue continuity

Panel membership

Paragraph membership

language profile

boundary confidence
```

Do not merge unrelated fragments simply to reduce Translation request count.

---

# 49. Fragment Grouping Output

Grouping should result in source-language structures such as:

```text
Sentence

Paragraph relationship

Continuation relationship
```

not TranslationBatch.

---

# 50. Oversized Source Structures

A Paragraph/Sentence may be unusually large due to:

```text
long prose

missing punctuation

Recognition errors

generated text

large structured-source Block
```

Text Processing may require safe structural splitting.

---

# 51. Safe Split Priority

Recommended conceptual priority:

```text
1. Paragraph boundary

2. Dialogue-turn boundary

3. Sentence boundary

4. Strong clause boundary

5. Soft clause boundary

6. Explicit safe fallback boundary
```

---

# 52. Protected Split Ranges

Avoid forced splitting inside:

```text
proper names

quoted expressions

URLs

numbers

dates

identifiers

protected terminology

bracketed phrases

known fixed expressions
```

---

# 53. Forced Split

If a source structure must be split for bounded processing:

```text
forcedSplit = true
```

or equivalent provenance should be recorded.

A forced split should not masquerade as a confident linguistic boundary.

---

# 54. Size Policy Ownership

Text Processing may define limits required for:

```text
bounded semantic processing

memory safety

incremental document construction
```

But it must not size source structures according to:

```text
provider context window

provider pricing

Translation request batching

retry policy
```

Those belong to Translation/Runtime.

---

# 55. Semantic Size Inputs

Text Processing size decisions may consider:

```text
character count

Sentence count

source Block count

semantic completeness

bounded memory constraints
```

Provider token count may be diagnostic information but must not drive provider-specific segmentation authority.

---

# 56. Segmentation Profile

Different source forms may use profiles such as:

```text
novel

comic

browser_article

document

selection

caption

mixed
```

A profile configures rules.

It is not a separate architecture module.

---

# 57. Profile Contents

A profile may define:

```text
boundary rules

protected patterns

quotation pairs

language rules

source-specific structural hints

maximum semantic scope

manual override policy

AI-assistance policy
```

---

# 58. Profile Versioning

Profile identity/version must participate in deterministic segmentation and cache compatibility where semantically relevant.

---

# 59. Effective Profile

Persistent user settings remain Preferences-owned.

Session overrides remain Reading Session-owned.

Application resolves effective Text Processing configuration.

Runtime should not become owner of semantic profile selection.

---

# 60. Stable Identity

Segmentation-refined semantic nodes need stable identities where useful.

Possible identity inputs:

```text
SourceDocument identity

ordered source references

semantic range

normalized/canonical text fingerprint

segmentation policy version

manual override revision
```

---

# 61. Identity Must Not Depend On

Avoid semantic IDs based on:

```text
Translation provider

Translation model

Translation output

UI component

storage path

AttemptId

WorkItemId

RuntimeRevisionId
```

---

# 62. Boundary Identity

A stable Boundary/segmented Sentence identity may be derived from semantic source references and relevant policy versions.

Exact hashing/ID implementation remains an implementation decision.

---

# 63. Source Mapping

Every refined source-language node must remain traceable to its source.

For Recognition-derived content:

```text
Text Node
    ↕
Recognition source entities / geometry
```

For structured text:

```text
Text Node
    ↕
source locator / text range
```

---

# 64. No Raw DOM References

Public SourceDocument semantics must not expose native:

```text
DOM Node

HTMLElement

browser object
```

Use normalized locators/references instead.

---

# 65. Many-to-Many Mapping

Segmentation must preserve:

```text
N source ranges
    ↕
M semantic nodes
```

Examples:

```text
multiple Recognition lines → one Sentence

one Bubble → multiple Sentences

two Bubbles → one semantic Sentence
```

---

# 66. Continuation

A semantic source structure may express continuation relationships.

Possible states:

```text
independent

continues_previous

continues_next

continues_both

unknown
```

These are source-language relationships.

---

# 67. Continuation Is Not Translation Context

Continuation metadata may help Translation build context.

But:

```text
Continuation
    ≠
Translation Context Window
```

Translation decides what related material actually enters a TranslationUnit/context.

---

# 68. Segmentation Confidence

Confidence describes boundary/structure reliability.

Possible dimensions:

```text
start boundary confidence

end boundary confidence

structural classification confidence

grouping confidence

source-order confidence

language confidence
```

---

# 69. Aggregate Confidence

An aggregate value may be exposed for convenience.

Detailed dimensions should remain available where diagnostics or downstream decision-making needs them.

---

# 70. Uncertainty

Ambiguous boundaries must remain explicit.

Do not convert:

```text
low evidence
```

into:

```text
certain boundary
```

only to simplify downstream code.

---

# 71. Manual Overrides

Users may correct segmentation.

Possible operations:

```text
split

merge

change semantic type

change continuation

reorder within permitted semantic authority

remove grouping

restore automatic segmentation

lock boundary

unlock boundary
```

---

# 72. Override Ownership

Manual segmentation override semantics belong to Text Processing unless/until a dedicated Correction domain is introduced.

UI Adapter only captures user intent.

---

# 73. Override Requirements

Overrides must:

```text
be explicit

be versioned

preserve source mapping

preserve provenance

invalidate only affected semantic scopes where possible

survive Translation reruns when source remains compatible
```

---

# 74. Locked Boundaries

A confirmed locked user boundary outranks automatic segmentation rules.

Automatic re-segmentation must preserve it unless an explicit user/policy action removes the lock.

---

# 75. Correction vs Segmentation Override

A text correction changes source-language content.

A segmentation override changes source-language structure.

These are distinct semantic changes and should have separate provenance.

---

# 76. Re-Segmentation

Re-segmentation may be required after:

```text
source text change

Recognition correction

source-order correction

profile change

language-policy change

manual override

algorithm/policy version change
```

---

# 77. Re-Segmentation Impact

Preferred scope is incremental.

Examples:

| Change                      | Typical Semantic Impact       |
| --------------------------- | ----------------------------- |
| Source text correction      | affected range + neighbors    |
| Source order correction     | affected structural scope     |
| Profile change              | relevant SourceDocument scope |
| Manual split                | selected node                 |
| Manual merge                | selected adjacent nodes       |
| Paragraph insertion         | insertion range + neighbors   |
| Translation provider change | none                          |
| Translation style change    | none                          |
| Presentation mode change    | none                          |

---

# 78. Glossary Change

A normal glossary change should not rebuild Text Segmentation.

Glossary semantics belong to Translation/Knowledge concerns.

Future glossary-aware protected ranges must be designed explicitly before they affect segmentation identity.

---

# 79. Incremental Processing

Long documents should support incremental semantic segmentation.

Example:

```text
Block 1 → stable

Block 2 → stable

Block 3 → provisional continuation

Block 4 → completes Block 3
```

---

# 80. Provisional Structure

A provisional Sentence/boundary may remain unresolved until:

```text
hard boundary appears

enough following source is available

input scope ends

bounded processing limit is reached

user explicitly commits processing
```

---

# 81. Provisional Is Not Published Authority

A provisional internal node must not be represented as a final confident source-language boundary without explicit status/provenance.

---

# 82. Streaming Structured Input

When input arrives incrementally, Text Processing may maintain a bounded pending semantic buffer.

Possible pending conditions:

```text
open quotation

incomplete sentence

unclosed bracket

partial source block

pending dialogue continuation
```

---

# 83. Bounded Pending Buffer

The pending buffer must remain bounded.

When the bound is exceeded, Text Processing may:

```text
create a forced provisional boundary

emit warning metadata

degrade to source Block boundary
```

according to policy.

---

# 84. Runtime Relationship

Runtime may execute Text Processing as:

```text
WorkItem
    ↓
Attempt
    ↓
Text Processing operation
```

Segmentation may occur as part of that semantic operation.

---

# 85. Runtime Does Not Own Segmentation Rules

Runtime owns:

```text
scheduling

Attempt lifecycle

retry

cancellation

supersession

resource admission
```

Text Processing owns segmentation semantics.

---

# 86. Cancellation

Text Processing must cooperate with Runtime cancellation.

It may:

```text
check cancellation context

stop expensive computation

discard provisional internal state

return cancellation-aware execution result
```

It does not own Runtime cancellation state.

---

# 87. No `Segmentation Cancelled` Authority

Deprecated assumption:

```text
Segmentation
    changes itself to Cancelled
```

as execution authority.

Runtime owns Attempt cancellation outcome.

Text Processing may report that computation was interrupted.

---

# 88. Supersession

If a newer RuntimeRevision supersedes current work:

```text
old Text Processing Attempt
```

may finish late.

Its Candidate SourceDocumentArtifact must still fail current-authority publication validation.

---

# 89. Cancellation Is Not Stale-Result Protection

Correctness depends on:

```text
Candidate
    ↓
Authority Validation
    ↓
Published Artifact
```

not merely successful cancellation.

---

# 90. Validation

Before a segmented SourceDocument Candidate can be published, validate relevant semantic invariants.

Examples:

```text
deterministic order

valid source mappings

valid ranges

required source content preserved

protected boundaries respected

manual locks respected

continuation references valid

node identities valid

profile/policy version recorded
```

---

# 91. Validation Failure

Validation failure prevents publication of the affected SourceDocument Candidate.

It does not mutate the previously Published Artifact.

---

# 92. Source Preservation

Segmentation must not silently discard:

```text
canonical source text

meaningful whitespace

Paragraph structure

dialogue punctuation

source references

order metadata

orientation hints

language spans

confirmed manual overrides

uncertainty metadata
```

---

# 93. Recovery Strategy

When ideal segmentation is impossible, prioritize preserving source semantics and traceability.

Possible degradations:

```text
keep original Paragraph boundary

one Sentence per Block

retain provisional Sentence

use simpler deterministic profile

mark uncertain boundary

request manual correction
```

---

# 94. Translation Request Size Is Not Recovery Rule

Do not split source semantics solely because:

```text
Provider X context window is small
```

Translation may split/group TranslationUnits later.

---

# 95. Failure Ownership

Exact Text Processing errors are defined in:

```text
02-modules/text-processing/ERRORS.md
```

This file defines only conceptual segmentation failure categories.

---

# 96. Conceptual Failure Categories

Examples:

```text
invalid source structure

invalid source mapping

unsupported language policy

protected-boundary conflict

manual override conflict

pending-buffer overflow

semantic validation failure

internal segmentation failure
```

---

# 97. Do Not Duplicate Runtime Errors

Do not define Text Processing errors for:

```text
Runtime cancellation

Retry exhausted

Attempt timeout

Superseded execution
```

unless they are merely preserved causal information.

Those execution semantics remain Runtime-owned.

---

# 98. Events

Exact Text Processing events are owned by:

```text
02-modules/text-processing/EVENTS.md
```

This architecture document does not define a second event catalog.

---

# 99. Deprecated Execution Events

Do not use generic Event Bus events such as:

```text
SegmentationStarted

SegmentProposed

SegmentationCompleted

SegmentationFailed

SegmentationCancelled
```

as execution control.

These are typically Runtime/internal progress concerns.

---

# 100. Valid Fact Events

A module event may be appropriate when a committed semantic fact needs asynchronous observation.

Examples conceptually:

```text
SourceDocumentPublished

TextStructureCorrected

SegmentationOverrideApplied
```

only if such events are actually defined in the module contract.

---

# 101. Event Bus Is Not Downstream Trigger

Forbidden:

```text
SegmentationCompleted
    ↓
TranslationRequested
```

Runtime/business dependency readiness controls execution.

---

# 102. Observability

Useful measurements may include:

```text
segmentation computation latency

input Block count

Sentence count

boundary count

forced split count

fragment grouping count

low-confidence boundary count

manual override count

re-segmentation count

validation failure count
```

---

# 103. Derived Metrics

Possible derived measures:

```text
sentences per Paragraph

source refs per Sentence

forced boundaries per document

manual override rate

low-confidence boundary rate
```

Do not optimize them blindly without Translation-quality evaluation.

---

# 104. Privacy

Segmentation processes user-readable source content.

Default principles:

```text
do not log raw text by default

prefer IDs/hashes/lengths for diagnostics

bound temporary buffers

release provisional data

follow SourceDocument retention policy
```

---

# 105. Performance

Segmentation should:

```text
remain deterministic where rules are deterministic

operate with low semantic-processing latency

support incremental documents

avoid recomputing unchanged scopes

use bounded memory

cooperate promptly with cancellation

allow safe parallel processing
```

---

# 106. Concurrency

Independent SourceDocument scopes may be processed concurrently.

Concurrency must not change:

```text
source order

node identity

boundary semantics

mapping semantics
```

---

# 107. Parallel Merge

If multiple independent scopes are processed in parallel, final assembly must use canonical semantic order rather than completion order.

---

# 108. Cache

Segmentation-derived Text Processing results may be cached.

Compatibility may depend on:

```text
source semantic fingerprint

SourceDocument base revision

segmentation profile/version

rule-set version

manual override revision

language policy version
```

---

# 109. Cache Is Not Semantic Owner

Cache storage does not own segmentation decisions.

Cached results must still satisfy:

```text
semantic compatibility

current Runtime authority
```

before current publication.

---

# 110. Translation Cache

Translation cache may use SourceDocument/semantic-node identity.

Its exact compatibility semantics belong to Translation/cache architecture.

Text Processing must not define Translation cache identity.

---

# 111. Interaction With Normalization

Normalization answers:

```text
What is the semantics-preserving canonical source text?
```

Segmentation answers:

```text
Where are the meaningful source-language boundaries
and relationships?
```

These responsibilities remain distinct.

---

# 112. Normalization Must Precede Boundary Decisions Where Required

Boundary algorithms should operate on canonical/normalized source text while retaining source references back to original accepted source representation.

---

# 113. Normalization Does Not Own Segmentation

Normalization may merge obvious visual line artifacts or clean Unicode/whitespace.

It must not silently perform deep sentence/dialogue grouping unless that behavior is explicitly part of Text Processing segmentation policy.

---

# 114. Interaction With Reading Order

Segmentation consumes authoritative semantic source order.

It may:

```text
group adjacent nodes

detect suspicious transitions

preserve order confidence
```

It does not silently rewrite source order authority.

---

# 115. Interaction With Translation

Translation consumes:

```text
Published SourceDocumentArtifact
```

and may use:

```text
Paragraphs

Sentences

Continuation relationships

source structure

neighbor references

boundary confidence
```

to build TranslationUnits.

---

# 116. Translation Owns Unit Construction

Canonical relationship:

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

Segmentation never publishes TranslationUnit as its own output.

---

# 117. Segment Legacy Term

The v1 architecture used:

```text
Segment
```

as “primary translation unit”.

That meaning is deprecated.

If the term `segment` remains in implementation, it must be qualified clearly as:

```text
TextSegment
or
SourceSegment
```

and must not be confused with TranslationUnit.

---

# 118. Preferred Vocabulary

Prefer Text Model vocabulary:

```text
Paragraph

Sentence

Span

Boundary

Continuation

Source Range
```

over an ambiguous universal:

```text
Segment
```

where possible.

---

# 119. Segment Group Legacy Term

The v1:

```text
SegmentGroup
```

mixed source semantic relationships with Translation context grouping.

Preferred v2 representation uses explicit source relationships such as:

```text
DialogueGroup

Continuation

Section

Neighbor relationship
```

Translation then chooses context from those relationships.

---

# 120. TranslationBatch Boundary

`TranslationBatch` belongs exclusively to:

```text
Translation
```

Text Processing must not recommend provider batching boundaries based on:

```text
model context size

provider limits

pricing

streaming

Retry Policy
```

---

# 121. Context Builder Boundary

The v1 architecture had a standalone:

```text
Context Builder
```

after Segmentation.

In current architecture:

```text
Translation
```

owns context assembly.

Segmentation merely supplies source structure that Translation can query.

---

# 122. Interaction With Presentation

Presentation's primary semantic input is:

```text
TranslationArtifact
```

It may use source provenance to map translated content to:

```text
Block

Region

geometry

source paragraph
```

where required.

---

# 123. Segmentation Does Not Size for Presentation

Deprecated:

```text
Segmentation changes boundaries
to avoid overlay overflow
```

Presentation layout constraints should not redefine source-language sentence/paragraph semantics.

---

# 124. Presentation Hints

Text Processing may preserve source facts such as:

```text
dense source Block

geometry

source region size

long source text
```

Presentation may derive layout risk from them.

Do not make target-language expansion an authoritative segmentation rule.

---

# 125. Vietnamese Target Considerations

The v1 architecture exposed:

```text
expected expansion risk

overlay overflow
```

from source segmentation.

These should move to:

```text
Translation / Presentation
```

because they depend on target-language output/presentation.

Source segmentation remains target-language independent.

---

# 126. Target-Language Independence

The same SourceDocument segmentation should normally remain valid regardless of whether target language is:

```text
Vietnamese

English

Japanese

Korean
```

unless there is a separately designed target-aware processing feature.

---

# 127. Interaction With Storage

Text Processing owns:

```text
semantic segmentation

boundary provenance

manual override semantics
```

Storage owns:

```text
physical persistence

retrieval

indexing

durability

migration mechanism
```

Storage must not reconstruct semantic boundaries independently.

---

# 128. Interaction With Runtime

Runtime owns:

```text
WorkItem readiness

Attempt execution

cancellation

retry

supersession

stale-result rejection authority

scheduling

resource management
```

Text Processing owns:

```text
semantic segmentation correctness
```

---

# 129. Runtime Profile Boundary

Runtime may carry an immutable effective Text Processing configuration snapshot.

It must not decide semantic segmentation rules itself.

Application/configuration resolution produces the effective profile.

---

# 130. No Downstream Stage Trigger

Runtime does not need Segmentation to emit:

```text
TranslationRequested
```

Once a Published SourceDocumentArtifact satisfies Translation WorkItem dependencies, Runtime may make Translation work ready.

---

# 131. Novel MVP

Recommended MVP support:

```text
structured Paragraph preservation

Chinese punctuation-aware sentence detection

long Paragraph safe splitting

short incomplete-fragment grouping

stable Source Mapping

incremental chapter processing
```

---

# 132. Novel MVP Deferrals

Defer:

```text
deep speaker attribution

advanced scene graph

complex cross-chapter semantic reconstruction

AI-only segmentation
```

---

# 133. Comic MVP

Recommended MVP support:

```text
one semantic Block per ordered Bubble/Region by default

multi-line reconstruction inside Block

explicit Block/Region → source mapping

optional evidence-based cross-Block continuation

manual boundary correction

boundary confidence/warnings
```

---

# 134. Comic MVP Deferrals

Defer:

```text
fully automatic speaker attribution

complex SFX grouping

aggressive cross-panel sentence composition

AI-only segmentation
```

---

# 135. Deterministic Baseline

A deterministic rule-based baseline should remain available even if AI-assisted segmentation is later introduced.

This supports:

```text
fallback

testing

debugging

offline mode

reproducibility
```

---

# 136. AI-Assisted Segmentation

AI may propose:

```text
dialogue continuation

cross-bubble grouping

semantic Paragraph reconstruction

speaker-turn boundaries

ambiguous punctuation interpretation

scene boundaries
```

---

# 137. AI Proposal Boundary

AI output is:

```text
proposal
```

not immediate SourceDocument authority.

Text Processing validates proposed boundary changes.

---

# 138. AI Requirements

AI-assisted proposals must:

```text
be optional

use canonical inputs/outputs

preserve source mapping

include provenance/model version

include confidence

remain reviewable

support deterministic fallback
```

---

# 139. AI Failure

AI failure must not prevent deterministic segmentation when a supported fallback exists.

---

# 140. Testing Strategy

Unit tests should cover:

```text
Chinese punctuation

quotation pairs

ellipsis

mixed-language spans

numeric expressions

URLs

Recognition line reconstruction

Paragraph splitting

forced safe boundaries

protected spans

stable semantic identity
```

---

# 141. Property Tests

Verify:

```text
no canonical source content is silently lost

output order matches canonical source order

deterministic input produces deterministic boundaries

source mappings cover refined text

manual locks survive re-segmentation

protected ranges are respected
```

---

# 142. Integration Tests

Integration should cover:

```text
RecognitionArtifact → Text Processing

structured source → Text Processing

source order correction

Published SourceDocumentArtifact → Translation

manual segmentation override

cache compatibility

comic Presentation source mapping
```

---

# 143. Representative Datasets

Include:

```text
Simplified Chinese novels

Traditional Chinese prose

horizontal comic dialogue

vertical comic dialogue

low-quality Recognition output

mixed Chinese-English

long unpunctuated Paragraphs

dialogue-heavy chapters

cross-bubble sentences

manual corrections
```

---

# 144. Architecture Decision — Segmentation Is Source Semantic

Segmentation determines source-language structure.

It does not determine Translation execution grouping.

---

# 145. Architecture Decision — Source Mapping Is Mandatory

Every Recognition-derived semantic node created/refined by segmentation must retain traceability to contributing source entities.

Structured inputs retain equivalent source locators/ranges.

---

# 146. Architecture Decision — Physical and Semantic Boundaries Coexist

Semantic grouping may cross:

```text
line

Bubble

Region
```

boundaries.

Physical/source boundaries must remain traceable.

---

# 147. Architecture Decision — Profiles Configure One Model

Novel/comic/browser behavior uses profile-specific policy over one Text Processing architecture.

Do not create independent segmentation architecture modules per source type.

---

# 148. Architecture Decision — Manual Overrides Are First-Class

Confirmed segmentation corrections are versioned semantic data.

They are not temporary ViewModel state.

---

# 149. Architecture Decision — Determinism Is Baseline

AI assistance may improve ambiguous cases but does not eliminate deterministic validated behavior.

---

# 150. Architecture Decision — Uncertainty Is Explicit

Low-confidence boundary decisions remain represented through:

```text
confidence

warnings

provisional state
```

rather than hidden certainty.

---

# 151. Architecture Decision — Target-Language Independent

Source segmentation is normally independent from:

```text
Translation provider

target-language wording

provider request size

target font/layout
```

---

# 152. Architecture Decision — No Standalone Segment Artifact

Segmentation does not publish:

```text
SegmentArtifact

SegmentBatchArtifact
```

as a separate cross-module authority.

Its semantic result is incorporated into:

```text
SourceDocumentArtifact
```

---

# 153. Architecture Invariants

1. Segmentation is owned by Text Processing.

2. Segmentation refines SourceDocument semantics.

3. Public cross-module output remains SourceDocumentArtifact.

4. Segmentation does not own TranslationUnit.

5. Segmentation does not own TranslationBatch.

6. Segmentation does not own Translation context assembly.

7. Source-language boundaries remain provider-independent.

8. Target-language choice normally does not change source segmentation.

9. Translation provider selection does not change source segmentation.

10. Physical source boundaries remain traceable after semantic grouping.

11. Source order is preserved unless changed by its authoritative owner.

12. Segmentation does not silently discard canonical source text.

13. Cross-bubble/cross-line sentences may be represented without losing physical mappings.

14. Manual locked boundaries override automatic segmentation.

15. Boundary uncertainty is explicit.

16. Deterministic input/configuration produces equivalent deterministic segmentation.

17. AI output is proposal until validated.

18. Internal segmentation phases are not Runtime states.

19. Runtime owns WorkItem/Attempt execution.

20. Runtime owns retry/cancellation/supersession mechanics.

21. Cancellation alone does not guarantee stale-result safety.

22. Candidate SourceDocumentArtifact requires authority validation.

23. Event Bus does not trigger Translation from segmentation completion.

24. Exact errors belong to Text Processing ERRORS.md.

25. Exact events belong to Text Processing EVENTS.md.

26. Segmentation cache does not own semantic authority.

27. Provider limits do not define source-language boundaries.

28. Presentation layout does not define source-language boundaries.

29. Provisional boundaries do not masquerade as confirmed boundaries.

30. Large documents use bounded/incremental segmentation.

---

# 154. Deprecated v1 Concepts

Deprecated:

```text
Segment
    = primary Translation unit
```

Deprecated:

```text
SegmentGroup
    = Translation context grouping authority
```

Deprecated:

```text
Segmentation
    → Context Builder
    → Translation Request Construction
```

Deprecated:

```text
Segmentation may recommend TranslationBatch boundaries
based on provider/model/cost/retry concerns
```

Deprecated:

```text
SegmentationStarted
SegmentationCompleted
SegmentationCancelled
```

as Event Bus execution-control events.

---

# 155. Preserved v1 Strengths

The following v1 concepts are intentionally retained:

```text
boundary strengths

language-aware segmentation

Chinese-specific rules

quotation protection

cross-bubble sentences

Recognition line reconstruction

vertical-text preservation

mixed-language handling

protected ranges

source mapping

stable semantic identity

manual overrides

incremental re-segmentation

bounded streaming input

deterministic fallback

AI-assisted proposal validation
```

---

# 156. Open Decisions

Prototype evidence is still required for:

```text
best Chinese sentence-boundary rules

cross-bubble grouping confidence threshold

whether cross-bubble grouping is enabled automatically in MVP

how reliably continuation can be inferred without AI

which structured-source paragraph boundaries are authoritative

infinite-scroll novel segmentation window

when low-confidence boundaries require user correction

manual override persistence scope

how much segmentation provenance is persisted

when AI-assisted segmentation is worth latency/cost

final SourceDocument Sentence/Relationship schema

whether Clause becomes a formal Text Model node
or remains internal analysis

semantic identity policy for re-segmented Sentences
```

---

# 157. Relationship to TEXT_MODEL.md

`TEXT_MODEL.md` defines:

```text
SourceDocument

Paragraph

Sentence

Span

SourceReference

Annotation

Relationship

canonical source text
```

This document defines how relevant structural/linguistic boundaries are derived and refined.

---

# 158. Relationship to Translation

Translation consumes:

```text
Published SourceDocumentArtifact
```

and decides:

```text
which source nodes become TranslationUnits

which units are grouped

which neighboring context is included

how batches are constructed
```

---

# 159. Relationship to CONTENT_CHANGE_FLOW.md

If source content changes during segmentation:

```text
new RuntimeRevision
    ↓
old processing may be superseded
```

Late Text Processing Candidates cannot regain current publication authority.

---

# 160. Related Documents

```text
doc/01-architecture/core/
├── DATA_FLOW.md
├── STATE_MACHINE.md
└── EVENT_CONVENTION.md

doc/01-architecture/text/
├── TEXT_MODEL.md
└── SEGMENTATION.md

doc/01-architecture/ocr/
└── READING_ORDER.md

doc/01-architecture/flows/
├── CONTENT_CHANGE_FLOW.md
├── SCREEN_COMIC_FLOW.md
└── STRUCTURED_TEXT_FLOW.md

doc/02-modules/
├── recognition/
├── text-processing/
├── translation/
└── presentation/

doc/01-architecture/runtime/
├── PIPELINE_RUNTIME.md
├── RETRY_POLICY.md
└── CANCELLATION.md
```

---

# 161. Completion Criteria

This segmentation architecture is synchronized when:

* Segmentation is clearly Text Processing-owned;
* `Segment` is no longer defined as the primary Translation unit;
* `TranslationUnit` remains Translation-owned;
* `TranslationBatch` remains Translation-owned;
* standalone Context Builder ownership is removed;
* provider limits/cost/retry do not determine source segmentation;
* target-language Presentation concerns do not determine source boundaries;
* segmentation refines SourceDocument rather than publishing a separate Translation-facing Artifact;
* cross-bubble/cross-line source semantics retain source mappings;
* incremental/provisional segmentation remains supported;
* Runtime owns cancellation/supersession;
* stale Candidate publication remains protected;
* Events/Errors are not duplicated here;
* deterministic fallback remains available alongside AI-assisted proposals.

---

# 162. Summary

CRAI v1 modeled:

```text
Normalized Text
    ↓
Segmentation
    ↓
Segment
    ↓
Segment Group
    ↓
Context Builder
    ↓
Translation Batch
    ↓
Translation
```

CRAI v2 models:

```text
Canonical Source Text
    ↓
Text Processing Segmentation
    ↓
Paragraph / Sentence / Span
Boundary / Continuation
    ↓
Candidate SourceDocumentArtifact
    ↓
Authority Validation
    ↓
Published SourceDocumentArtifact
```

Then Translation independently performs:

```text
SourceDocumentArtifact
    ↓
TranslationUnit
    ↓
TranslationBatch
    ↓
Context Assembly
    ↓
TranslationArtifact
```

The central rule is:

```text
Segmentation decides
how source-language content is structured.

Translation decides
how that source structure is packaged
for translation.
```
