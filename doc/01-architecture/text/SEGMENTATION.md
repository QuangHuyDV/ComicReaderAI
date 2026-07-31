# Text Segmentation

* **Document:** Text Architecture / Segmentation
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines how CRAI divides normalized and ordered source text into stable, meaningful units for translation, context building, caching, editing and presentation.

Segmentation converts a continuous or fragmented text stream into structured translation units without losing:

* Semantic meaning
* Paragraph relationships
* Dialogue boundaries
* Source ordering
* Layout references
* Speaker hints
* Formatting intent
* Traceability to the original content

Segmentation is not equivalent to splitting text at every punctuation mark.

A valid segment must be large enough to provide sufficient translation context and small enough to support incremental processing, cancellation, caching and user correction.

---

# Scope

Segmentation applies to text obtained from:

* Browser DOM extraction
* Novel and web-novel pages
* OCR text regions
* Comic speech bubbles
* Captions
* Narration boxes
* User-selected text
* Imported text documents
* Mixed text and image content

The same architecture supports these sources through different segmentation profiles.

---

# Responsibilities

The Segmentation component is responsible for:

* Detecting sentence boundaries
* Preserving paragraph boundaries
* Grouping related lines
* Grouping dialogue fragments
* Separating narration from dialogue when identifiable
* Preserving source and reading order
* Preventing unsafe splits
* Splitting oversized content
* Merging undersized fragments
* Creating stable segment identifiers
* Recording source-to-segment mappings
* Assigning confidence and diagnostic information
* Producing translation-ready units

---

# Non-Responsibilities

Segmentation is not responsible for:

* Extracting text from the DOM
* Detecting OCR regions
* Recognizing characters from images
* Correcting arbitrary OCR errors
* Determining final translation wording
* Resolving glossary entries
* Attributing speakers with certainty
* Selecting AI providers
* Rendering translated output
* Persisting physical records
* Orchestrating the full processing pipeline

These responsibilities belong to Extraction, OCR, Normalization, Context, Translation, Presentation, Storage and Runtime components.

---

# Position in the Text Pipeline

```text
Raw Extracted Text
        │
        ▼
Text Normalization
        │
        ▼
Reading Order Reconstruction
        │
        ▼
Text Segmentation
        │
        ▼
Context Building
        │
        ▼
Translation Request Construction
        │
        ▼
Translation
        │
        ▼
Post Processing
        │
        ▼
Presentation
```

Segmentation consumes normalized text whose source ordering has already been established as far as possible.

It must not silently reorder source units.

---

# Core Concepts

## Source Unit

A `SourceUnit` is the smallest input item received by segmentation.

Examples include:

* DOM text node
* DOM paragraph
* OCR line
* OCR region
* Speech bubble
* Caption box
* User selection
* Imported document block

A source unit may be incomplete and may not be suitable for direct translation.

---

## Logical Block

A `LogicalBlock` groups source units that belong to the same structural area.

Examples include:

* Paragraph
* Dialogue block
* Speech bubble
* Narration box
* Heading
* List item
* Caption
* Footnote

Logical blocks preserve more structure than plain text.

---

## Segment

A `Segment` is the primary translation unit produced by this component.

A segment contains one or more source units and represents a coherent portion of text that can be translated independently while retaining enough context.

---

## Segment Group

A `SegmentGroup` contains related segments that should share translation context.

Examples include:

* Several dialogue bubbles in one exchange
* Consecutive paragraphs in one scene
* A heading and its following paragraph
* Multiple fragments belonging to one sentence
* A comic panel containing several ordered bubbles

A segment group is not necessarily sent to a provider as one request.

It defines contextual relationships independently from execution batching.

---

## Translation Batch

A `TranslationBatch` is an execution-level collection of segments sent together for translation.

Segmentation may recommend batch boundaries, but the final batch decision belongs to the AI or Translation pipeline because it also depends on:

* Provider limits
* Model context size
* Cost policy
* Latency target
* Streaming support
* User preferences
* Retry policy

Therefore:

```text
Segment ≠ Translation Batch
```

---

# Segmentation Goals

The segmentation strategy should optimize several competing goals.

## Meaning Preservation

Related text should remain together when separating it would create ambiguity.

---

## Incremental Processing

Long chapters must be processable without waiting for the entire chapter to complete.

---

## Stable Presentation

Translated output must map predictably back to source paragraphs, lines, regions or bubbles.

---

## Efficient Caching

A small source change should invalidate only affected segments whenever possible.

---

## Correctable Output

Users should be able to correct, retranslate or reorder a segment without retranslating the entire document.

---

## Provider Independence

Segments must not depend on the tokenization behavior or request format of a specific translation provider.

---

## Determinism

Identical normalized input, reading order and segmentation configuration must produce equivalent segment boundaries and stable identities.

---

# Segmentation Profiles

Different content types require different segmentation behavior.

Recommended profiles include:

| Profile           | Primary Use                            |
| ----------------- | -------------------------------------- |
| `novel`           | Novels, web novels and prose chapters  |
| `comic`           | Comics, manga, manhua and manhwa       |
| `browser_article` | Structured browser text                |
| `document`        | Imported text documents                |
| `selection`       | Small user-selected passages           |
| `caption`         | Captions and subtitle-like text        |
| `mixed`           | Pages containing several content types |

A profile defines rules and thresholds, not a separate implementation.

---

# Segmentation Pipeline

```text
Ordered Source Units
        │
        ▼
Structural Classification
        │
        ▼
Boundary Candidate Detection
        │
        ▼
Unsafe Boundary Protection
        │
        ▼
Fragment Grouping
        │
        ▼
Oversized Segment Splitting
        │
        ▼
Undersized Segment Merging
        │
        ▼
Segment Validation
        │
        ▼
Identity and Mapping
        │
        ▼
Segmented Text Document
```

---

# Structural Classification

Before sentence-level segmentation, source units should be classified where possible.

Recommended structural types include:

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

Classification may use:

* DOM element semantics
* OCR region type
* Speech-bubble detection
* Layout position
* Punctuation patterns
* Quotation marks
* Source adapter hints
* User corrections
* Language profile rules

Classification may be uncertain.

Uncertain classification must be represented explicitly rather than converted into false certainty.

---

# Boundary Types

Segmentation recognizes multiple boundary strengths.

| Boundary    | Meaning                                                |
| ----------- | ------------------------------------------------------ |
| `hard`      | Segments must normally be separated                    |
| `strong`    | Separation is strongly preferred                       |
| `soft`      | Separation is allowed when size or context requires it |
| `protected` | Separation is prohibited unless recovery is required   |
| `unknown`   | Boundary confidence is insufficient                    |

Examples:

## Hard Boundaries

* Chapter boundary
* Independent heading
* Explicit scene separator
* Unrelated DOM container
* Separate comic panel when no dialogue continuity exists
* User-forced boundary

## Strong Boundaries

* Paragraph end
* Completed dialogue turn
* Narration-to-dialogue transition
* List item boundary
* Speech bubble boundary

## Soft Boundaries

* Sentence ending
* Clause boundary
* Line break inside one paragraph
* Minor punctuation
* OCR line boundary

## Protected Boundaries

* Inside a proper name
* Inside a numeric expression
* Inside a URL
* Inside an identifier
* Between paired quotation marks
* Inside an ellipsis
* Between OCR lines belonging to one sentence
* Between parts of a detected fixed expression

---

# Boundary Detection Inputs

Boundary detection may consider:

* Unicode punctuation
* Language-specific punctuation
* Whitespace
* Line breaks
* Paragraph breaks
* DOM hierarchy
* OCR coordinates
* Region type
* Reading order
* Font and style changes
* Quotation state
* Bracket state
* List markers
* Heading classification
* Source adapter annotations
* Language profile
* User-defined rules

No individual signal should be treated as universally authoritative.

---

# Sentence Segmentation

Sentence segmentation identifies probable sentence boundaries inside logical blocks.

For Chinese source text, common sentence-ending punctuation includes:

```text
。
！
？
……
……
```

Western punctuation may also appear:

```text
.
!
?
...
```

However, punctuation alone is insufficient.

Examples that should not be split blindly include:

```text
3.14
www.example.com
第1.5章
A.B.C.
……
？！ 
```

The segmenter should apply language-aware boundary rules and preserve ambiguous cases for later validation.

---

# Chinese-Specific Rules

Initial Chinese support must handle:

* Simplified and Traditional Chinese
* Chinese full-width punctuation
* Mixed Chinese and Latin characters
* Ellipses represented by repeated punctuation
* Dialogue quotation marks
* Chapter numbering
* Names and titles
* Short omitted-subject clauses
* Idioms and fixed expressions
* Vertical OCR output
* Sentences split across OCR lines
* Sentences split across speech bubbles
* Web-novel formatting conventions

Typical quotation pairs include:

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

Segmentation should avoid splitting while a paired construct remains open unless the input is known to be malformed.

---

# Vietnamese Target Considerations

Segmentation is based primarily on source structure, not expected Vietnamese sentence length.

However, Chinese-to-Vietnamese translation often expands in character count.

Therefore, the segmenter may expose presentation hints such as:

* Expected expansion risk
* Very short source fragment
* Dense dialogue region
* Limited rendering area
* Long proper-name sequence
* Potential overlay overflow

These hints may influence batching or presentation but must not alter source meaning.

---

# Novel Segmentation

Novel segmentation should prioritize paragraph and narrative coherence.

Recommended hierarchy:

```text
Chapter
    └── Section
          └── Paragraph
                └── Sentence
                      └── Clause
```

Preferred behavior:

1. Preserve source paragraph boundaries.
2. Keep short related sentences in the same segment.
3. Split long paragraphs at sentence boundaries.
4. Split oversized sentences only at safe clause boundaries.
5. Avoid merging different speakers.
6. Preserve explicit blank-line and scene separators.
7. Keep headings separate but linked to their following content.
8. Maintain stable paragraph identifiers.

Example:

```text
Source paragraph
    │
    ├── Sentence 1
    ├── Sentence 2
    └── Sentence 3
```

Possible result:

```text
Segment A: Sentence 1 + Sentence 2
Segment B: Sentence 3
```

The result depends on size thresholds, dialogue structure and context requirements.

---

# Dialogue Segmentation

Dialogue requires stronger structural preservation than ordinary prose.

A dialogue turn should remain identifiable even when grouped with nearby turns for context.

Recommended model:

```text
Dialogue Exchange
├── Turn 1
├── Turn 2
├── Turn 3
└── Narration Link
```

Each turn should retain:

* Source order
* Quotation style
* Speaker hint, when available
* Associated source block
* Relationship to adjacent turns
* Whether the turn is complete
* Whether continuation is expected

Different speakers must not be merged into one source string without explicit structural markers.

---

# Comic Segmentation

Comic segmentation operates primarily on spatial and semantic regions rather than prose paragraphs.

Recommended hierarchy:

```text
Page
    └── Panel
          └── Text Region
                └── Line
                      └── Token
```

A comic segment will usually correspond to:

* One speech bubble
* One narration box
* One caption
* One grouped multi-region utterance
* One independent text region

Multiple regions may be grouped when:

* One sentence continues across regions
* The same speaker continues speaking
* The regions belong to one panel
* Reading order indicates direct continuation
* Translation requires nearby context
* OCR divided one visual text area incorrectly

Multiple regions should remain separate when:

* Different speakers are likely
* Narration and dialogue differ
* Regions belong to unrelated panels
* Reading order is uncertain
* Overlay rendering requires independent placement
* User corrections have fixed the boundaries

---

# Cross-Bubble Sentences

Chinese comics may distribute one sentence across multiple speech bubbles.

Example:

```text
Bubble A: 如果你现在离开
Bubble B: 就再也别回来！
```

These bubbles form one semantic sentence but remain two presentation regions.

The representation should support:

```text
Segment
├── semantic_text: 如果你现在离开，就再也别回来！
└── source_fragments
    ├── Bubble A
    └── Bubble B
```

Translation may be performed with the combined semantic text while translated fragments remain mappable to individual regions.

The original region boundaries must never be discarded.

---

# OCR Line Grouping

OCR engines may return one source sentence as several physical lines.

Example:

```text
我从来没有
见过这样的
事情。
```

The segmenter may merge the lines into:

```text
我从来没有见过这样的事情。
```

The merged segment must preserve line mappings:

```text
Segment
├── Source Line 1
├── Source Line 2
└── Source Line 3
```

Line merging should consider:

* Region membership
* Spatial proximity
* Text orientation
* Punctuation
* Font similarity
* Reading order
* OCR confidence
* Language-specific syntax

A visual line break is not automatically a semantic boundary.

---

# Vertical Text

Vertical Chinese text may be returned as:

* One complete region
* One character per OCR line
* Multiple vertical columns
* Incorrect left-to-right ordering
* Mixed vertical and horizontal text

Segmentation assumes the Reading Order component has already produced the best available order.

The segmenter must retain:

* Original orientation
* Column identity
* Region coordinates
* Ordering confidence
* Any manual ordering override

Vertical layout metadata must not be removed after text is converted into a linear string.

---

# Mixed-Language Content

Segments may contain:

* Chinese and English
* Chinese and Japanese
* Chinese and numbers
* Names written in Latin characters
* URLs
* Code fragments
* Game statistics
* Item identifiers

Mixed-language spans should be preserved as typed ranges when detected.

Example:

```text
打开 Skill Tree，然后选择 Fireball Lv.3。
```

The segmenter should avoid separating:

```text
Skill Tree
Fireball Lv.3
```

unless the source structure explicitly requires it.

---

# Fragment Merging

Short fragments may be merged when they do not contain enough meaning for reliable translation.

Examples include:

```text
但是……
所以说……
那个……
然后呢？
```

Merge candidates should be evaluated using:

* Structural type
* Source proximity
* Reading order
* Punctuation
* Dialogue continuity
* Panel membership
* Paragraph membership
* Language profile
* Maximum segment size
* Confidence

Fragments from unrelated source blocks must not be merged solely to reduce request count.

---

# Oversized Segment Splitting

Segments may exceed configured limits because of:

* Long paragraphs
* Missing punctuation
* OCR errors
* Generated text
* Large DOM blocks
* Provider limitations

Recommended split priority:

```text
1. Paragraph boundary
2. Dialogue-turn boundary
3. Sentence boundary
4. Strong clause boundary
5. Soft clause boundary
6. Token-safe fallback boundary
```

The segmenter should avoid splitting:

* Proper names
* Quoted expressions
* URLs
* Numbers
* Dates
* Identifiers
* Glossary terms
* Bracketed phrases
* Detected fixed expressions

Forced splits must be marked explicitly.

---

# Segment Size Policy

Segment size must not be defined only by character count.

The policy may consider:

* Character count
* Estimated tokens
* Sentence count
* Source block count
* OCR region count
* Semantic completeness
* Dialogue-turn count
* Layout constraints
* Provider-independent application limits

Recommended conceptual thresholds:

```text
minimum preferred size
target size
maximum preferred size
absolute maximum size
```

These values belong to a segmentation profile and may vary by content type.

Exact production thresholds should be determined through prototype testing rather than fixed prematurely in architecture.

---

# Segment Identity

Every segment requires a stable identity.

Recommended identity inputs include:

* Document or page ID
* Segmentation profile version
* Ordered source-unit identifiers
* Normalized source text hash
* Boundary configuration version
* Manual override revision

Example conceptual identity:

```text
segment_id = stable_hash(
    source_scope_id,
    ordered_source_unit_ids,
    normalized_text_hash,
    segmentation_profile_version,
    override_revision
)
```

The exact hashing implementation is not part of this document.

A segment identifier must not depend on:

* Translation provider
* Selected model
* Translation result
* UI component
* Storage path

---

# Source Mapping

Every segment must be traceable to its source.

A source mapping may contain:

```text
SegmentSourceMapping
├── Source Unit ID
├── Source Range
├── Segment Range
├── Reading Order
├── Region ID
├── Image ID
├── DOM Reference
├── Coordinate Reference
└── Mapping Confidence
```

A segment may map to several source units.

A source unit may contribute to several segments only when a controlled split occurs.

---

# Canonical Segment Contract

```text
TextSegment
├── id
├── document_id
├── group_id
├── sequence
├── type
├── source_language
├── normalized_text
├── source_unit_refs[]
├── source_ranges[]
├── structure
│   ├── block_type
│   ├── paragraph_id
│   ├── dialogue_turn_id
│   ├── panel_id
│   └── region_ids[]
├── boundary
│   ├── start_type
│   ├── end_type
│   ├── forced_split
│   └── continuation
├── language_spans[]
├── confidence
├── warnings[]
├── profile_id
├── profile_version
├── source_revision
└── created_at
```

The concrete serialization format is defined in the shared contracts layer.

---

# Segment Type

Recommended segment types include:

```text
heading
prose
dialogue
narration
caption
list_item
footnote
sound_effect
metadata
mixed
unknown
```

`unknown` is a valid type.

Downstream components must tolerate it.

---

# Continuation Model

A segment may be incomplete because it continues from or into another segment.

Recommended continuation states:

```text
independent
continues_previous
continues_next
continues_both
unknown
```

This information helps the Context Builder include adjacent segments when needed.

---

# Segmentation Confidence

Confidence expresses the reliability of segment boundaries, not the correctness of the text itself.

Possible confidence dimensions include:

* Start boundary confidence
* End boundary confidence
* Structural classification confidence
* Fragment grouping confidence
* Reading-order confidence
* Language detection confidence

A single aggregate score may be exposed for convenience, but detailed confidence should remain available for diagnostics.

---

# Manual Overrides

Users must be able to correct segmentation when automatic rules fail.

Supported operations may include:

* Split segment
* Merge adjacent segments
* Reorder segments
* Change segment type
* Link continuation
* Remove incorrect grouping
* Restore automatic segmentation
* Lock a boundary
* Unlock a boundary

Manual overrides must:

* Be explicit
* Be versioned
* Preserve source traceability
* Survive retranslation when source content is unchanged
* Invalidate affected caches
* Avoid rewriting unrelated segments

A locked user boundary has higher priority than automatic rules.

---

# Re-Segmentation

Re-segmentation may be triggered by:

* Source text change
* OCR correction
* Reading-order correction
* Profile change
* Language-profile change
* User override
* Segmentation algorithm upgrade
* Configuration revision change

Re-segmentation should be incremental when possible.

Only affected source ranges and neighboring dependency ranges should be recalculated.

---

# Change Impact

Typical change impact:

| Change                      | Expected Impact                 |
| --------------------------- | ------------------------------- |
| Translation provider change | No segmentation change          |
| Translation style change    | No segmentation change          |
| OCR text correction         | Affected segment and neighbors  |
| Reading-order change        | Affected block or page          |
| Segmentation profile change | Relevant document or session    |
| Manual split                | Selected segment                |
| Manual merge                | Selected adjacent segments      |
| Source paragraph insertion  | Insertion area and neighbors    |
| Glossary update             | Normally no segmentation change |
| Presentation mode change    | No segmentation change          |

Glossary-aware protected spans may be introduced later, but glossary changes should not normally rebuild all segmentation.

---

# Caching

Segmentation results may be cached using:

* Source revision
* Normalized text hash
* Reading-order revision
* Segmentation profile ID
* Segmentation profile version
* Rule-set version
* Manual override revision

Cached segmentation must not be reused when structural inputs differ.

Translation cache identity may reference segment identity but remains owned by Translation or AI Cache architecture.

---

# Incremental Processing

Long documents should support incremental segmentation.

Example:

```text
Chapter Stream
    │
    ├── Block 1 → Segment
    ├── Block 2 → Segment
    ├── Block 3 → Pending continuation
    └── Block 4 → Completes previous segment
```

A segment may remain provisional until:

* A hard boundary is found
* Enough following context is available
* The input stream ends
* A configured size limit is reached
* The user requests immediate processing

Provisional segments must not be treated as immutable final segments.

---

# Streaming Input

When input arrives incrementally, the segmenter should maintain a limited pending buffer.

The buffer may contain:

* Open quotation
* Incomplete sentence
* Unclosed bracket
* Partial OCR region
* Incomplete DOM block
* Pending dialogue continuation

The buffer must have a bounded size.

If the bound is reached, the segmenter may emit a forced segment with an appropriate warning.

---

# Cancellation

Segmentation must support cancellation.

Cancellation may occur because of:

* User action
* Reading session change
* Newer source revision
* Page navigation
* Pipeline restart
* Application shutdown
* Timeout

On cancellation:

* No partial result becomes authoritative
* Completed immutable segments may be retained when safe
* Provisional state is discarded
* Diagnostics remain available
* Downstream processing is not started for cancelled segments

---

# Validation

Before segmentation output is published, validate:

* Segment IDs are unique
* Segment order is deterministic
* Source mappings are valid
* Source ranges do not exceed source boundaries
* Required text is not silently discarded
* Protected spans are not split unexpectedly
* Hard boundaries are preserved
* Manual locks are respected
* Segment sizes remain within absolute limits
* Continuation references are valid
* Group references are valid
* Profile version is recorded

Validation failure prevents publication of the affected segmentation result.

---

# Text Preservation Rules

The following information must not be silently discarded:

* Original normalized text
* Meaningful whitespace
* Paragraph boundaries
* Dialogue punctuation
* Region references
* Ordering information
* Orientation metadata
* Language spans
* Manual corrections
* Uncertainty warnings

Presentation-only whitespace may be normalized, but source mappings must remain valid.

---

# Failure Categories

Recommended error categories include:

```text
INVALID_INPUT
MISSING_READING_ORDER
UNSUPPORTED_LANGUAGE_PROFILE
SEGMENT_SIZE_EXCEEDED
INVALID_SOURCE_MAPPING
PROTECTED_BOUNDARY_VIOLATION
MANUAL_OVERRIDE_CONFLICT
STREAM_BUFFER_EXCEEDED
SEGMENT_VALIDATION_FAILED
CANCELLED
INTERNAL_SEGMENTATION_ERROR
```

Failures should include:

* Error code
* Affected source scope
* Recoverability
* Diagnostic details
* Rule-set version
* Profile version
* Correlation ID

Raw user content should not be placed in logs by default.

---

# Recovery Strategies

Possible recovery strategies include:

* Retry with the same profile
* Use a simpler fallback profile
* Preserve source block boundaries only
* Emit one segment per source unit
* Split at safe size boundaries
* Mark uncertain boundaries
* Request user correction
* Skip only invalid source units
* Cancel downstream translation

Fallback behavior must prioritize preserving content and traceability over producing ideal segment sizes.

---

# Events

Typical events include:

```text
SegmentationStarted
SegmentProposed
SegmentFinalized
SegmentGroupCreated
SegmentationCompleted
SegmentationFailed
SegmentationCancelled
SegmentSplit
SegmentsMerged
SegmentOrderChanged
SegmentationOverrideApplied
SegmentationInvalidated
```

Events should contain identifiers and metadata rather than full source text unless explicitly permitted.

---

# Observability

Recommended metrics include:

* Segmentation duration
* Input source-unit count
* Output segment count
* Average segment size
* Maximum segment size
* Forced split count
* Fragment merge count
* Low-confidence boundary count
* Manual override count
* Re-segmentation count
* Validation failure count
* Cancellation count
* Profile usage
* Language-profile usage

Useful derived metrics include:

```text
source units per segment
characters per segment
tokens per segment
segments per paragraph
segments per comic page
manual correction rate
```

These metrics help determine whether segmentation rules improve translation quality and reading continuity.

---

# Privacy

Segmentation processes user-visible source content.

Therefore:

* Raw text must not be logged by default.
* Diagnostic logging should prefer hashes, IDs, lengths and rule results.
* Temporary buffers must be released after use.
* Persistent storage must follow the active privacy policy.
* Cloud services are not required for deterministic segmentation.
* User corrections must follow project and retention settings.

---

# Performance Goals

Segmentation should:

* Process normal text blocks with low latency
* Support long chapters incrementally
* Avoid reprocessing unchanged content
* Use bounded memory
* Support cancellation promptly
* Scale across multiple pages or chapters
* Permit parallel processing of independent blocks
* Preserve deterministic output under concurrency

Parallel processing must not change final source order.

---

# Extension Model

The architecture may support language-specific and source-specific extensions.

Examples:

```text
ChineseLanguageProfile
JapaneseLanguageProfile
VietnameseLanguageProfile
NovelSegmentationProfile
ComicSegmentationProfile
BrowserArticleProfile
SiteAdapterHints
```

Extensions may provide:

* Boundary rules
* Protected patterns
* Quotation pairs
* Abbreviation rules
* Structural hints
* Size recommendations
* Mixed-language handling

Extensions must return canonical segmentation contracts.

They must not create provider-specific segments.

---

# Interaction with Normalization

Normalization prepares textual content.

Segmentation consumes normalized output but must retain references to the normalized source revision.

Normalization may:

* Normalize Unicode
* Repair whitespace
* Merge obvious OCR fragments
* Repair punctuation
* Mark uncertain corrections

Segmentation decides semantic boundaries.

The two responsibilities must remain separate.

---

# Interaction with Reading Order

Reading Order determines the sequence of source units.

Segmentation:

* Consumes that order
* Preserves ordering metadata
* Groups adjacent units
* Reports suspicious transitions
* Never silently replaces reading order

When segmentation detects a likely ordering problem, it should emit a diagnostic or correction suggestion instead of mutating the authoritative order.

---

# Interaction with Context Building

The Context Builder consumes:

* Segments
* Segment groups
* Continuation relationships
* Structural types
* Neighbor references
* Confidence information

Segmentation defines structural relationships.

Context Building decides which related information is actually included in a translation request.

---

# Interaction with Translation

Translation consumes stable segment identities and text.

Translation may process:

* One segment
* Several segments
* One segment group
* An execution batch containing several groups

Translation must return results mapped to segment IDs.

It must not depend on array position alone.

---

# Interaction with Presentation

Presentation consumes:

* Segment order
* Source mappings
* Region references
* Paragraph relationships
* Fragment mappings
* Translation results

For comic content, presentation may render different translated fragments into separate visual regions even when those regions were translated as one semantic segment.

Segmentation must therefore preserve both semantic grouping and physical region boundaries.

---

# Interaction with Storage

Segmentation owns:

* Segmentation rules
* Segment construction
* Boundary decisions
* Source mappings
* Profile versioning
* Manual override semantics

Storage owns:

* Physical persistence
* Retrieval
* Indexing
* Retention
* Migration execution

Storage must not recreate segmentation decisions from raw text.

---

# Interaction with Runtime

Runtime is responsible for:

* Starting segmentation
* Passing cancellation signals
* Selecting effective profiles
* Managing dependencies
* Scheduling independent work
* Rejecting stale results
* Triggering downstream stages

Segmentation must not orchestrate OCR, Translation or Presentation.

---

# Recommended MVP Strategy

For the first CRAI implementation, segmentation should remain conservative.

## Novel MVP

Support:

* DOM paragraph preservation
* Chinese punctuation-based sentence detection
* Long-paragraph splitting
* Short-fragment merging
* Stable source mapping
* Incremental chapter processing

Defer:

* Advanced speaker attribution
* Deep semantic scene detection
* Machine-learned paragraph reconstruction
* Complex cross-chapter segmentation

## Comic MVP

Support:

* One segment per ordered OCR region by default
* Multi-line merging inside one region
* Explicit region-to-segment mapping
* Optional adjacent-region grouping
* Manual split and merge
* Confidence and warnings

Defer:

* Fully automatic speaker attribution
* Reliable sound-effect grouping
* Complex cross-panel sentence reconstruction
* AI-driven segmentation as the only strategy

The deterministic rule-based path should remain available even when AI-assisted segmentation is introduced.

---

# Future AI-Assisted Segmentation

AI may later assist with:

* Dialogue continuation detection
* Cross-bubble sentence grouping
* Semantic paragraph reconstruction
* Speaker-turn separation
* Ambiguous punctuation repair
* Scene-boundary detection

AI-assisted decisions must:

* Be optional
* Use canonical input and output contracts
* Preserve deterministic fallback
* Include confidence
* Remain reviewable
* Never remove source mappings
* Never become the only recovery path
* Be versioned by model and prompt configuration

AI output should propose segmentation decisions.

The deterministic Segmentation component remains responsible for validating and publishing them.

---

# Testing Strategy

## Unit Tests

Test:

* Chinese punctuation boundaries
* Quotation pairs
* Ellipses
* Mixed-language spans
* Numeric expressions
* URLs
* OCR line merging
* Paragraph splitting
* Forced-size splits
* Protected spans
* Stable identity generation

## Property Tests

Verify:

* No source content is lost
* Output order matches input order
* Repeated execution is deterministic
* Source mappings cover all emitted text
* Segment sizes do not exceed absolute limits
* Locked boundaries are preserved

## Integration Tests

Test with:

* Browser DOM extraction
* OCR output
* Reading-order corrections
* Context Builder
* Translation batching
* Comic overlay presentation
* Side-panel novel presentation
* Storage cache invalidation

## Representative Datasets

Include:

* Simplified Chinese web novels
* Traditional Chinese prose
* Horizontal comic dialogue
* Vertical comic dialogue
* Low-quality OCR output
* Mixed Chinese-English content
* Long unpunctuated paragraphs
* Dialogue-heavy chapters
* Multi-region comic sentences
* User-corrected segmentation cases

---

# Architecture Decisions

## Decision 1 — Segments Are Semantic and Traceable

A segment is a semantic translation unit with explicit source mappings.

It is not merely a string slice.

---

## Decision 2 — Segment and Batch Are Separate

Segmentation defines meaning and structure.

Translation batching defines execution efficiency.

---

## Decision 3 — Source Boundaries Are Preserved

Physical regions, DOM blocks and OCR lines remain traceable even after semantic grouping.

---

## Decision 4 — Profiles Replace Hard-Coded Content Paths

Novel and comic segmentation use profile-specific rules over one canonical architecture.

---

## Decision 5 — Manual Overrides Are First-Class

User corrections are versioned architecture data, not temporary UI state.

---

## Decision 6 — Deterministic Segmentation Is the Baseline

AI-assisted segmentation may improve results but cannot replace deterministic fallback behavior.

---

## Decision 7 — Uncertainty Is Explicit

Ambiguous structure and boundaries are represented through confidence and warnings rather than hidden assumptions.

---

# Architecture Invariants

1. Every segment maps back to one or more source units.
2. Segmentation never silently discards source text.
3. Segment order follows the authoritative reading order.
4. Translation provider selection does not affect segmentation.
5. Segment identity is independent from translated output.
6. Physical source boundaries remain traceable after semantic grouping.
7. Manual locked boundaries override automatic rules.
8. Identical inputs and configuration produce equivalent segmentation.
9. Segmentation profiles are versioned.
10. Forced splits are explicitly marked.
11. Provisional segments cannot become authoritative without validation.
12. Different speakers are not silently merged into one unstructured string.
13. Segment groups and translation batches remain separate concepts.
14. Cancellation prevents stale segmentation from reaching downstream stages.
15. AI-assisted decisions are validated before publication.

---

# Open Questions

The following decisions require prototype evidence:

* What target segment size provides the best Chinese-to-Vietnamese translation quality?
* How many neighboring comic regions should be grouped as context?
* Should cross-bubble grouping be enabled automatically in the MVP?
* How reliably can continuation be detected without AI?
* Which DOM structures should be considered authoritative paragraph boundaries?
* How should segmentation behave on infinite-scroll novel websites?
* When should low-confidence segmentation pause for user correction?
* Should manual overrides apply only to one page or persist across reprocessing?
* How much segmentation metadata should be persisted by default?
* When is AI-assisted segmentation worth its additional latency and cost?

---

# Related Documents

* `docs/architecture/text/README.md`
* `docs/architecture/text/NORMALIZATION.md`
* `docs/architecture/text/READING_ORDER.md`
* `docs/architecture/text/LANGUAGE_DETECTION.md`
* `docs/architecture/text/STRUCTURE.md`
* `docs/architecture/text/CONTRACTS.md`
* `docs/architecture/ocr/READING_ORDER.md`
* `docs/architecture/ai/CONTEXT.md`
* `docs/architecture/ai/PROMPTS.md`
* `docs/architecture/ai/STAGES.md`
* `docs/architecture/presentation/MODULE.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`

---

# Summary

Text Segmentation transforms normalized and ordered source content into stable, semantic and traceable translation units.

It preserves both meaning and source structure, supports different profiles for novels and comics, separates semantic segmentation from provider batching, allows incremental processing and manual correction, and ensures every translation result can be mapped back to its original paragraph, line, bubble or region.

This component forms the boundary between low-level text preparation and context-aware translation processing.
