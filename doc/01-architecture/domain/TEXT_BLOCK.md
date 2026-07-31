# Text Block Domain

* **Document:** Domain / Text Block
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

A Text Block represents one logically readable unit of source text within a Page.

It connects visual or structured source content to downstream translation, review, presentation and export processes.

Depending on the content source, a Text Block may represent:

* A speech bubble
* A narration box
* A sound effect
* A caption
* A label
* A paragraph
* A heading
* A dialogue line
* A structured text fragment
* A manually selected text region

A Text Block is not an independent aggregate root.

It belongs to exactly one Page and is managed within the Page aggregate.

---

# Domain Role

The Text Block is the canonical domain representation of readable source text after content extraction and text analysis.

```text
Image or Structured Source
            │
            ▼
       Text Detection
            │
            ▼
            OCR
            │
            ▼
       Text Analysis
            │
            ▼
        Text Block
            │
      ┌─────┼──────────┐
      ▼     ▼          ▼
Translation Review  Presentation
```

OCR provider output must not be passed directly to Translation or Presentation.

Provider-specific OCR results are normalized into Text Blocks before crossing the domain boundary.

---

# Ownership Boundary

```text
Page Aggregate
├── Images
├── OCR Results
├── Layout
├── Text Blocks
│   ├── Source Text
│   ├── Geometry
│   ├── Reading Order
│   ├── Classification
│   ├── Confidence
│   ├── Revisions
│   └── Translation References
├── Translation Results
├── Render Layers
└── Diagnostics
```

The Page owns:

* Text Block identity
* Block ordering
* Block geometry
* Source-text revisions
* Block relationships
* Translation associations
* Block lifecycle

The Chapter may coordinate ordering and translation context across Pages, but it does not take ownership away from the Page.

---

# Responsibilities

A Text Block is responsible for:

* Identifying one readable source-text unit
* Preserving normalized source text
* Referencing its source artifact
* Recording spatial geometry when applicable
* Declaring reading order
* Describing text orientation and direction
* Classifying semantic role
* Preserving OCR confidence
* Tracking source-text revisions
* Supporting manual correction
* Linking translation results to the correct source revision
* Supporting rendering and presentation alignment
* Preserving derivation and processing lineage

A Text Block is not responsible for:

* Capturing screen or browser content
* Downloading source images
* Executing OCR
* Choosing an OCR provider
* Translating text
* Selecting a translation provider
* Rendering translated text
* Persisting binary image data
* Managing provider-specific response formats
* Orchestrating Page processing

Those responsibilities belong to Capture, OCR, Translation, Presentation, Rendering, Storage and Runtime components.

---

# Identity

Every Text Block has a stable identity within its Page.

Typical fields include:

* Text Block ID
* Page ID
* Source Type
* Source Artifact ID
* Source Artifact Version
* Block Type
* Source Text
* Normalized Text
* Language
* Geometry
* Reading Order
* Orientation
* Writing Direction
* OCR Confidence
* Status
* Revision
* Created Time
* Updated Time

`Text Block ID` identifies the logical block.

It must not be replaced merely because its translation changes.

When the block can still be recognized as the same logical content after OCR correction or user editing, its identity remains stable and its revision increases.

---

# Source Types

A Text Block may be created from different source forms.

| Source Type     | Description                                                   |
| --------------- | ------------------------------------------------------------- |
| `image_ocr`     | Recognized from an image through OCR                          |
| `browser_dom`   | Extracted from structured browser content                     |
| `document_text` | Extracted from PDF, EPUB or another document                  |
| `clipboard`     | Created from clipboard text                                   |
| `manual_region` | Created from a user-selected visual region                    |
| `manual_text`   | Entered directly by the user                                  |
| `generated`     | Produced by segmentation, merging or another processing stage |

Source type describes origin only.

It must not couple the domain model to a specific adapter, provider or platform.

---

# Block Types

Recommended semantic block types include:

| Block Type     | Description                                                  |
| -------------- | ------------------------------------------------------------ |
| `dialogue`     | Spoken dialogue associated with a character or speech region |
| `narration`    | Narrative text outside normal dialogue                       |
| `thought`      | Internal thought or monologue                                |
| `caption`      | Supporting caption or explanatory text                       |
| `heading`      | Title, chapter heading or section heading                    |
| `paragraph`    | Continuous prose paragraph                                   |
| `sound_effect` | Visual or textual sound effect                               |
| `label`        | Name, sign, interface label or object annotation             |
| `footnote`     | Footnote, translator note or reference note                  |
| `metadata`     | Publication or structural metadata                           |
| `unknown`      | Unclassified readable text                                   |

Classification may be revised after OCR, layout analysis or user review.

Unknown classification must not prevent translation.

---

# Text Representations

A Text Block may retain several text representations.

```text
Raw OCR Text
      │
      ▼
Normalized Source Text
      │
      ▼
User-Corrected Source Text
      │
      ▼
Effective Source Text
```

## Raw OCR Text

Text returned by the OCR stage before domain normalization.

It may contain:

* Incorrect whitespace
* Broken punctuation
* OCR control symbols
* Character substitutions
* Line-break artifacts
* Provider-specific formatting

Raw OCR text is diagnostic and should not automatically be treated as authoritative.

## Normalized Source Text

Text after deterministic normalization.

Normalization may include:

* Unicode normalization
* Whitespace cleanup
* Line-break normalization
* Punctuation normalization
* Script normalization
* Removal of provider artifacts

Normalization must preserve semantic meaning.

## User-Corrected Source Text

An optional correction explicitly made or approved by the user.

User correction has higher authority than OCR and automatic normalization.

## Effective Source Text

The source text consumed by Translation and Presentation.

Resolution order:

```text
User-Corrected Text
        ↓ fallback
Normalized Source Text
        ↓ fallback
Raw OCR Text
```

The effective source text must always be derived deterministically.

---

# Geometry

Image-derived Text Blocks must preserve their position in a stable image coordinate space.

Recommended geometry representation:

```text
Geometry
├── Bounding Box
├── Polygon
├── Baseline
├── Rotation
└── Coordinate Reference
```

Typical fields include:

* Image ID
* Image Version
* Coordinate Width
* Coordinate Height
* Bounding Box
* Polygon Points
* Rotation Angle
* Transform Reference

Canonical coordinates use:

```text
origin: top-left
x-axis: left to right
y-axis: top to bottom
unit: source pixels
```

A Text Block geometry is valid only for the referenced image version.

Geometry must not be reused against another image version without a verified transform.

Structured text blocks such as browser paragraphs may not require pixel geometry. They may instead contain a structural locator.

---

# Structural Locator

Non-image Text Blocks may use a structural locator instead of visual geometry.

Possible locator forms include:

* DOM node reference
* CSS selector
* XPath
* Document element ID
* Paragraph index
* Character range
* Source fragment identifier
* Adapter-defined stable locator

A locator is descriptive metadata.

The domain must not rely on a locator remaining valid forever.

When source structure changes, the locator may be invalidated while the Text Block content remains available.

---

# Text Orientation

Supported orientations may include:

| Orientation  | Description                                  |
| ------------ | -------------------------------------------- |
| `horizontal` | Text flows primarily along a horizontal axis |
| `vertical`   | Text flows primarily along a vertical axis   |
| `rotated`    | Text uses a non-standard rotation            |
| `mixed`      | Several orientations exist inside one block  |
| `unknown`    | Orientation has not been determined          |

Orientation is different from writing direction.

For example, a vertical Chinese block may still preserve a defined character and line progression.

---

# Writing Direction

Supported writing directions may include:

* Left to right
* Right to left
* Top to bottom
* Bottom to top
* Mixed
* Unknown

Writing direction affects:

* OCR interpretation
* Line reconstruction
* Reading order
* Translation grouping
* Rendering strategy
* User navigation

Writing direction must be explicit when provider inference is unreliable.

---

# Reading Order

Each Text Block has a reading-order position within its Page.

Typical fields:

* Group ID
* Sequence Index
* Parent Block ID
* Previous Block ID
* Next Block ID
* Reading-Order Confidence

Reading order may be produced by:

* OCR provider
* Layout analysis
* Domain ordering rules
* Source document structure
* User correction

Reading order must be unique within an ordering group.

The physical position of a block alone must not be treated as guaranteed reading order.

---

# Block Relationships

Text Blocks may have explicit relationships.

Supported relationships may include:

| Relationship     | Description                                   |
| ---------------- | --------------------------------------------- |
| `continues`      | Text continues into another block             |
| `continued_by`   | Another block continues this text             |
| `parent_of`      | Block contains smaller logical blocks         |
| `child_of`       | Block belongs to a larger logical block       |
| `overlaps`       | Visual regions overlap                        |
| `alternative_of` | Competing detection of the same source region |
| `speaker_of`     | Block is associated with a detected speaker   |
| `annotation_of`  | Block annotates another block                 |
| `derived_from`   | Block was created from another block          |

Relationships must reference Text Blocks within a compatible ownership boundary.

Cross-Page relationships may be allowed for continuation and reading context, but they must not transfer ownership.

---

# Grouping

Several Text Blocks may be grouped for translation without being permanently merged.

```text
Text Block A ─┐
Text Block B ─┼──► Translation Group
Text Block C ─┘
```

Grouping may improve:

* Dialogue context
* Pronoun resolution
* Terminology consistency
* Paragraph reconstruction
* Translation quality
* Request efficiency

A Translation Group is a processing structure.

It does not replace the identity of its member Text Blocks.

Translation output must remain mappable back to individual blocks.

---

# Split and Merge

OCR and layout analysis may produce blocks with incorrect boundaries.

CRAI must support splitting and merging.

## Split

```text
Text Block A
    │
    ├──► Text Block A1
    └──► Text Block A2
```

A split creates new logical blocks and records derivation from the original block.

The original block becomes superseded rather than silently overwritten.

## Merge

```text
Text Block A ─┐
              ├──► Text Block C
Text Block B ─┘
```

A merge creates a new logical block whose lineage references every source block.

Merged source blocks become superseded.

Translations associated with superseded blocks must be invalidated or explicitly remapped.

---

# Confidence

Confidence represents uncertainty in automatically derived properties.

A Text Block may contain separate confidence values for:

* Region detection
* Text recognition
* Language detection
* Orientation detection
* Reading order
* Block classification
* Character association

Confidence should not be compressed into one value when separate values are available.

Recommended normalized range:

```text
0.0 <= confidence <= 1.0
```

Provider-specific confidence formats must be normalized before entering the domain.

Missing confidence is different from zero confidence.

---

# Language

Each Text Block may declare:

* Detected Language
* Effective Source Language
* Script
* Language Confidence
* Mixed-Language Flag

The effective source language may be resolved from:

```text
Explicit Block Override
        ↓ fallback
Page Configuration
        ↓ fallback
Chapter Configuration
        ↓ fallback
Project Configuration
        ↓ fallback
Detected Language
```

Mixed-language Text Blocks should preserve the original text and may be segmented when required by translation policy.

Language detection must not modify the source text.

---

# Character and Speaker Association

Dialogue or thought blocks may reference a Character.

Typical fields:

* Character ID
* Speaker Confidence
* Association Source
* Association Revision

Association source may be:

* User selection
* Layout inference
* Visual analysis
* Conversation inference
* Imported metadata

User-confirmed association takes precedence over inferred association.

A Text Block must remain translatable when no speaker is known.

---

# Lifecycle

```text
Detected
    │
    ▼
Recognized
    │
    ▼
Normalized
    │
    ▼
Ready
    │
    ├──► Translating
    │         │
    │         ▼
    │      Translated
    │
    ├──► Needs Review
    │
    ├──► Superseded
    │
    └──► Invalidated
```

Lifecycle meaning:

* `Detected`: A source region or structured fragment exists.
* `Recognized`: Source text has been extracted.
* `Normalized`: Deterministic text normalization has completed.
* `Ready`: The block is valid for translation.
* `Translating`: A translation request currently references the block.
* `Translated`: A compatible translation result exists.
* `Needs Review`: Confidence or validation rules require attention.
* `Superseded`: The block was replaced by a split, merge or regeneration.
* `Invalidated`: The source reference or block data is no longer valid.

A block may return from `Translated` to `Ready` when its effective source text changes.

---

# Revision Model

Text Block identity and Text Block revision are separate concepts.

```text
Text Block ID: stable logical identity
Revision:      version of mutable source data
```

A revision must increase when changing information that can affect downstream processing, including:

* Effective source text
* Language
* Reading order
* Geometry
* Orientation
* Classification
* Speaker association
* Block relationships

Presentation-only annotations that do not affect processing may be stored separately.

Every Translation must reference the exact Text Block revision used as input.

---

# Mutability

The following information may be edited:

* Corrected source text
* Block type
* Reading order
* Language override
* Speaker association
* Review status
* User annotation

The following changes require regeneration, supersession or explicit revision:

* Source artifact replacement
* Image coordinate-space change
* Block split
* Block merge
* OCR result replacement
* Geometry remapping
* Source fragment relocation

Historical source revisions should be retained when required for auditing, correction rollback or translation comparison.

---

# Translation Association

A Translation does not overwrite Text Block source text.

Recommended relationship:

```text
Text Block
├── Source Revision 1
│   └── Translation Revision A
├── Source Revision 2
│   ├── Translation Revision B
│   └── Translation Revision C
└── Active Translation Reference
```

A Translation must reference:

* Text Block ID
* Text Block Revision
* Source Language
* Target Language
* Effective Source Text Hash
* Translation Profile Revision
* Context Revision
* Translation Revision

A translation becomes stale when any required input revision changes.

Stale translations may remain available for history but must not be treated as current.

---

# Presentation Association

Presentation consumes Text Blocks and compatible Translation results.

A presentation mapping may include:

* Text Block ID
* Translation ID
* Source Geometry
* Display Geometry
* Presentation Mode
* Font Profile
* Overflow Strategy
* Visibility
* Layer Order

Presentation-specific layout must not modify Text Block source geometry.

For example:

```text
Source Geometry
      │
      ├──► Overlay Placement
      ├──► Side-Panel Marker
      └──► Export Placement
```

Each presentation mode may derive its own display geometry.

---

# Manual Correction

Users may correct:

* OCR text
* Block boundary
* Reading order
* Block type
* Language
* Speaker association

Correction rules:

1. The original OCR result remains traceable.
2. The correction creates a new revision.
3. The user-corrected value becomes authoritative.
4. Dependent translations are marked stale.
5. Dependent render outputs are invalidated.
6. Unaffected blocks remain valid.
7. Correction events identify the actor and changed fields.

Manual correction must invalidate only dependent artifacts.

It must not restart the entire Chapter unless the change affects chapter-level context.

---

# Regeneration

Text Blocks may be regenerated when:

* OCR provider changes
* OCR configuration changes
* Source image changes
* Image preprocessing changes
* Layout algorithm changes
* Language settings change
* User requests reprocessing

Regeneration must use reconciliation rather than blind replacement when possible.

Reconciliation may match blocks using:

* Geometry overlap
* Source-text similarity
* Reading-order proximity
* Structural locator
* Content hash
* Stable adapter identity

Successful reconciliation preserves logical identity.

Unmatched old blocks become superseded.

Unmatched new blocks receive new identities.

---

# Deduplication

Duplicate Text Blocks may be detected by combining:

* Page ID
* Source Artifact Version
* Geometry
* Structural Locator
* Effective Source Text
* Reading Order
* Content Hash

Text equality alone is insufficient.

Two speech bubbles may contain identical text while representing different logical blocks.

Deduplication must never merge blocks solely because their source strings match.

---

# Content Hash

A Text Block should expose a deterministic source-content hash.

The hash may include:

* Effective source text
* Source language
* Block type when translation-significant
* Text orientation
* Writing direction
* Normalization revision

The hash may support:

* Translation cache lookup
* Stale-result detection
* Idempotent processing
* Duplicate request prevention
* Diagnostics
* Revision comparison

Geometry should not automatically be included in the translation-content hash unless translation behavior depends on it.

A separate structural hash may include geometry and ordering data.

---

# Validation

Before entering the `Ready` state, a Text Block should satisfy:

* Text Block ID is present
* Page ID is present
* Source reference is valid
* Effective source text is not empty
* Language representation is valid
* Revision is valid
* Reading order does not conflict
* Geometry references a compatible image version
* Coordinates are within valid bounds
* Confidence values are within valid ranges
* Relationships reference valid blocks
* Superseded blocks are not selected as active input

Structured text blocks without image geometry must contain a valid structural or source reference.

---

# Error Conditions

Typical domain-level errors include:

* Text block not found
* Page ownership mismatch
* Empty effective source text
* Unsupported language
* Invalid geometry
* Coordinate version mismatch
* Invalid reading order
* Duplicate reading-order position
* Invalid block relationship
* Circular block relationship
* Source artifact unavailable
* Source revision mismatch
* Translation revision mismatch
* Superseded block used as active input
* Invalid split operation
* Invalid merge operation
* Confidence value out of range
* Structural locator invalid
* User correction conflict

Provider-specific errors must be translated into stable module or domain errors before crossing architecture boundaries.

---

# Events

Typical domain events include:

* `TextBlockDetected`
* `TextBlockRecognized`
* `TextBlockCreated`
* `TextBlockNormalized`
* `TextBlockReady`
* `TextBlockUpdated`
* `TextBlockCorrected`
* `TextBlockReordered`
* `TextBlockClassified`
* `TextBlockSpeakerAssigned`
* `TextBlockSplit`
* `TextBlocksMerged`
* `TextBlockSuperseded`
* `TextBlockInvalidated`
* `TextBlockTranslationStale`
* `TextBlockReviewRequested`

Events should contain identifiers, revisions and changed-field metadata.

Events should not carry raw image bytes or provider-specific OCR payloads.

Large source text should be omitted from general event envelopes unless explicitly required by the consumer contract.

---

# Persistence

Recommended persistent separation:

```text
Text Block Record
├── identity
├── page ownership
├── source reference
├── effective text
├── language
├── classification
├── geometry or locator
├── reading order
├── confidence
├── status
└── current revision

Text Block Revision
├── revision number
├── changed fields
├── previous revision
├── correction source
├── actor
├── timestamp
└── content hashes

Translation Record
├── text block reference
├── source revision
├── target language
├── translated text
└── translation metadata
```

Raw OCR provider payloads may be retained separately for diagnostics or reproducibility.

They must not become the canonical Text Block representation.

---

# Retention

Suggested retention policy:

* Active Text Blocks: retain while the Page exists
* User corrections: retain as durable domain history
* Superseded blocks: retain while referenced by revisions or diagnostics
* Raw OCR text: retain according to privacy and diagnostic policy
* Provider payloads: short-lived unless required for debugging
* Geometry history: retain when required for reconciliation or auditing
* Temporary grouping metadata: cacheable and regenerable
* Stale translations: retain according to translation-history policy

Removing a Text Block must not leave dangling Translation, Presentation or Revision references.

---

# Privacy

Text Blocks may contain private or copyrighted content.

Requirements:

* Do not log source text by default.
* Avoid sending unrelated blocks to external providers.
* Apply provider-retention policy before remote processing.
* Protect user corrections and reading history.
* Allow temporary-session processing without durable persistence.
* Store only content required by configured retention policy.
* Exclude source text from telemetry unless explicitly permitted.

Diagnostics should prefer hashes, identifiers, sizes and confidence values over raw content.

---

# Processing Example: Comic Page

```text
Page Image
    │
    ▼
Detect three visual text regions
    │
    ▼
OCR each region
    │
    ▼
Create Text Blocks
    │
    ├── Block 1: narration
    ├── Block 2: dialogue
    └── Block 3: sound effect
    │
    ▼
Determine reading order
    │
    ▼
Normalize Chinese source text
    │
    ▼
Group Blocks 1 and 2 for contextual translation
    │
    ▼
Store translations per Text Block
    │
    ▼
Present in side panel or overlay
```

---

# Processing Example: Browser Novel

```text
Browser Document
    │
    ▼
Locate chapter content
    │
    ▼
Extract headings and paragraphs
    │
    ▼
Create Text Blocks
    │
    ├── Heading
    ├── Paragraph 1
    ├── Paragraph 2
    └── Paragraph 3
    │
    ▼
Preserve DOM reading order
    │
    ▼
Translate in contextual groups
    │
    ▼
Map translations back to paragraphs
    │
    ▼
Present formatted Vietnamese text
```

Image geometry is not required in this flow.

Structural locators and source order provide alignment.

---

# Architecture Invariants

1. Every Text Block belongs to exactly one Page.
2. Text Block ID represents stable logical identity.
3. Every processing-significant change creates a new revision.
4. Translation never overwrites source text.
5. Every Translation references an exact Text Block revision.
6. Provider-specific OCR data is normalized before entering the domain.
7. Image-derived geometry references an exact image version.
8. Structured Text Blocks may use locators instead of pixel geometry.
9. Reading order is explicit and versioned.
10. User-corrected source text takes precedence over automatic text.
11. Split and merge operations preserve lineage.
12. Superseded blocks are not used as active processing input.
13. Duplicate text does not imply duplicate Text Blocks.
14. Presentation layout does not modify source geometry.
15. Text Block changes invalidate only dependent artifacts.
16. Confidence values preserve the distinction between unknown and zero.
17. Text Blocks remain provider and storage independent.
18. Raw source text is excluded from logs by default.
19. A Text Block cannot outlive its Page.
20. Cross-Page relationships never transfer ownership.

---

# Open Decisions

The following decisions should remain open until prototype validation:

* Whether raw OCR text is stored by default
* Whether all Text Block revisions are persisted
* Which geometry representation becomes canonical
* Whether polygons are required for the MVP
* How OCR blocks are reconciled after recapture
* Whether sound effects are translated automatically
* How vertical Chinese text is segmented
* How mixed-language blocks are handled
* How much surrounding context is grouped for translation
* Whether browser locators should use DOM identity, text anchors or both
* How long stale translations are retained
* Whether speaker association belongs in the initial MVP
* Which corrections should be promoted into reusable OCR knowledge

---

# Related Documents

* README.md
* PROJECT.md
* BOOK.md
* CHAPTER.md
* PAGE.md
* IMAGE.md
* TRANSLATION.md
* LANGUAGE.md
* GLOSSARY.md
* CHARACTER.md
* SESSION.md
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/DATA_FLOW.md`
