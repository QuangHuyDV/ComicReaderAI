# Text Block Domain

* **Document:** Domain / Text Block
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

A `TextBlock` represents one stable logical unit of readable source text within CRAI.

A TextBlock bridges source content and downstream capabilities such as:

* translation,
* review,
* presentation,
* reading,
* search,
* export.

Depending on the source, a TextBlock may represent:

* a speech bubble,
* narration box,
* thought bubble,
* sound effect,
* caption,
* label,
* paragraph,
* heading,
* dialogue line,
* structured document fragment,
* browser text fragment,
* manually selected text region,
* manually entered text.

A TextBlock is an independently addressable domain resource when stable identity, revision history, translation linkage, geometry, or correction history matters.

It is **not required to belong to a Page**.

Image-derived TextBlocks normally reference a Page and Image.

Text-native TextBlocks may instead belong directly to a Chapter or another structured-content scope.

---

# Domain Role

TextBlock is the canonical domain representation of readable source text after source-specific extraction has crossed into CRAI's normalized domain model.

Image-oriented flow:

```text
Image
  |
  v
Detection / Recognition
  |
  v
Text Analysis
  |
  v
TextBlock
  |
  +--> Translation
  +--> Review
  +--> Presentation
```

Structured-text flow:

```text
Browser / Document / Native Text
              |
              v
         Extraction
              |
              v
          TextBlock
              |
              +--> Translation
              +--> Review
              +--> Presentation
```

Provider-specific OCR or extraction payloads MUST NOT become the canonical representation consumed directly by downstream domain modules.

They are normalized into stable TextBlock resources first.

---

# Responsibilities

A TextBlock is responsible for:

* stable logical identity,
* source-text representation,
* source provenance,
* effective source text,
* source revision,
* semantic classification,
* language metadata,
* reading-order metadata,
* optional visual geometry,
* optional structural locator,
* text orientation,
* writing direction,
* confidence metadata,
* block relationships,
* split/merge lineage,
* correction history,
* downstream revision compatibility.

A TextBlock is NOT responsible for:

* source capture,
* remote downloading,
* OCR execution,
* provider selection,
* translation execution,
* translation provider selection,
* rendering execution,
* Presentation execution,
* storage of binary image data,
* orchestration of Page or Chapter processing.

Those responsibilities remain in the corresponding capabilities.

---

# Identity

Every TextBlock has a stable logical identifier.

Typical fields include:

```text
TextBlock
├── textBlockId
├── projectId
├── chapterId?
├── pageId?
├── sourceType
├── sourceArtifactId?
├── sourceArtifactVersion?
├── blockType
├── language
├── revision
├── lifecycleStatus
├── createdAt
└── updatedAt
```

Optional content-specific fields include:

```text
geometry?
structuralLocator?
readingOrder?
orientation?
writingDirection?
```

`textBlockId` identifies the logical content unit.

It MUST NOT change merely because:

* translation changes,
* classification changes,
* user corrects OCR text,
* reading order changes,
* speaker association changes.

When the content can still be reconciled as the same logical unit, identity remains stable and its revision changes.

---

# Scope

A TextBlock MUST have an explicit valid content scope.

Common scopes include:

```text
Project
└── Chapter
    └── TextBlock
```

or:

```text
Project
└── Chapter
    └── Page
        └── TextBlock
```

A Page association is therefore optional.

Typical fields:

```text
projectId
chapterId?
pageId?
```

When `pageId` is present:

* the Page MUST exist,
* its Chapter and Project scope MUST be compatible,
* visual geometry MAY be present.

When `pageId` is absent:

* another stable source/scope reference MUST identify where the TextBlock belongs.

---

# Source Types

Recommended source types include:

| Source Type     | Meaning                                                           |
| --------------- | ----------------------------------------------------------------- |
| `IMAGE_OCR`     | Recognized from visual content                                    |
| `BROWSER_DOM`   | Extracted from browser structure                                  |
| `DOCUMENT_TEXT` | Extracted from EPUB, PDF text layer, document, etc.               |
| `CLIPBOARD`     | Created from clipboard source                                     |
| `MANUAL_REGION` | Created from a selected visual region                             |
| `MANUAL_TEXT`   | Entered directly                                                  |
| `GENERATED`     | Produced by segmentation, split, merge, or normalization workflow |
| `OTHER`         | Explicitly supported unknown source                               |

Source type describes provenance.

It MUST NOT identify a provider or adapter implementation.

---

# Source Provenance

A TextBlock SHOULD retain enough information to identify its source.

Possible fields include:

```text
sourceType
sourceArtifactId
sourceArtifactVersion
sourceLocator
sourceRevision
```

Image-derived example:

```text
sourceType: IMAGE_OCR
pageId: page_001
imageId: image_004
imageVersion: 2
```

Browser example:

```text
sourceType: BROWSER_DOM
chapterId: chapter_100
structuralLocator: ...
```

Source provenance MUST remain traceable when user correction changes effective text.

---

# Block Types

Recommended semantic block types include:

```text
DIALOGUE
NARRATION
THOUGHT
CAPTION
HEADING
PARAGRAPH
SOUND_EFFECT
LABEL
FOOTNOTE
METADATA
UNKNOWN
```

Classification MAY change after:

* layout analysis,
* source interpretation,
* user correction,
* context analysis.

`UNKNOWN` MUST remain processable.

Unknown classification MUST NOT block translation by itself.

---

# Text Representations

A TextBlock MAY retain multiple source representations.

```text
Raw Extracted Text
        |
        v
Normalized Source Text
        |
        v
User-Corrected Source Text
        |
        v
Effective Source Text
```

Not every source type produces every layer.

For example:

```text
MANUAL_TEXT
    |
    v
Effective Source Text
```

may require no OCR layer.

---

# Raw Extracted Text

Raw extracted text is the source text returned before CRAI normalization.

For OCR this may contain:

* whitespace errors,
* punctuation errors,
* recognition substitutions,
* broken lines,
* provider artifacts.

For structured extraction it may contain:

* source markup residue,
* formatting artifacts,
* unexpected whitespace.

Raw extracted text is primarily provenance and diagnostic data.

It SHOULD NOT automatically become authoritative source text.

---

# Normalized Source Text

Normalized source text results from deterministic, semantics-preserving normalization.

Possible normalization includes:

* Unicode normalization,
* whitespace cleanup,
* line-break normalization,
* punctuation normalization,
* script normalization,
* removal of extraction artifacts.

Normalization MUST preserve intended source meaning.

A semantics-changing rewrite is NOT ordinary normalization.

---

# User-Corrected Source Text

Users MAY explicitly correct source text.

A confirmed user correction has higher authority than automatically extracted text.

Corrections MUST:

* preserve prior revision traceability,
* create a new processing-significant revision,
* invalidate dependent results according to dependency rules.

---

# Effective Source Text

The effective source text is the canonical source consumed by downstream capabilities.

Recommended precedence:

```text
User-Corrected Source Text
          |
          v fallback
Normalized Source Text
          |
          v fallback
Raw Extracted Text
```

For sources without raw or normalized variants, the explicit source text MAY directly become effective text.

Resolution MUST be deterministic.

---

# Revision Model

TextBlock identity and TextBlock revision are separate.

```text
textBlockId
    stable logical identity

revision
    processing-significant version
```

Revision MUST increase whenever a change can alter dependent processing behavior.

Examples include changes to:

* effective source text,
* source language,
* semantic block type,
* geometry,
* reading order,
* orientation,
* writing direction,
* speaker association,
* block relationships,
* source mapping.

---

# Revision Compatibility

Downstream artifacts MUST reference the TextBlock revision they consumed.

Example:

```text
TextBlock
    id: block_001
    revision: 7

Translation
    textBlockId: block_001
    sourceRevision: 7
```

When TextBlock revision becomes 8:

```text
Translation(sourceRevision=7)
    -> STALE
```

The historical Translation MAY remain available.

It MUST NOT be treated as current without compatibility validation.

---

# Content Hash

A TextBlock SHOULD expose deterministic hashes for processing compatibility.

A translation-oriented content hash MAY include:

```text
effectiveSourceText
sourceLanguage
blockType if translation-significant
orientation if significant
writingDirection if significant
normalizationRevision
```

Geometry SHOULD NOT automatically be included unless translation behavior depends upon it.

A separate structural hash MAY include:

* geometry,
* ordering,
* structural locator,
* relationships.

This prevents unnecessary translation invalidation after presentation-only changes.

---

# Geometry

Image-derived TextBlocks MAY contain visual geometry.

Recommended form:

```text
Geometry
├── imageId
├── imageVersion
├── coordinateWidth
├── coordinateHeight
├── boundingBox?
├── polygon?
├── baseline?
├── rotation?
└── transformReference?
```

Geometry MUST reference the exact Image identity/version whose coordinate system produced it.

Geometry MUST NOT be interpreted solely from `pageId`.

---

# Coordinate Space

Canonical raster coordinate convention:

```text
origin: top-left
x-axis: left -> right
y-axis: top -> bottom
unit: pixels
```

A geometry-bearing TextBlock MUST retain enough coordinate metadata to prevent accidental reuse against incompatible Images.

Mapping geometry to another Image requires an explicit or lineage-derived transform.

---

# Structural Locator

Text-native TextBlocks MAY use structural location rather than image geometry.

Possible forms include:

* DOM node identity,
* selector,
* XPath,
* document element ID,
* paragraph index,
* character range,
* text anchor,
* fragment identity,
* adapter-defined stable locator.

A structural locator is not guaranteed to remain valid forever.

Locator invalidation MUST NOT automatically destroy already preserved TextBlock content.

---

# Geometry vs Locator

A TextBlock MAY contain:

```text
geometry
```

or:

```text
structuralLocator
```

or, when useful:

```text
both
```

The domain MUST NOT require pixel geometry for text-native content.

---

# Orientation

Text orientation describes the visual orientation of the text unit.

Recommended values:

```text
HORIZONTAL
VERTICAL
ROTATED
MIXED
UNKNOWN
```

Orientation is distinct from writing direction.

---

# Writing Direction

Possible writing-direction values include:

```text
LEFT_TO_RIGHT
RIGHT_TO_LEFT
TOP_TO_BOTTOM
BOTTOM_TO_TOP
MIXED
UNKNOWN
```

Writing direction MAY influence:

* recognition,
* line reconstruction,
* reading-order inference,
* translation grouping,
* Presentation behavior.

Automatic inference MAY be corrected by the user.

---

# Reading Order

Reading order MUST be explicit when ordering matters.

Possible representation:

```text
ReadingOrder
├── groupId?
├── sequence
├── confidence?
└── source
```

Possible sources:

```text
OCR_PROVIDER
LAYOUT_ANALYSIS
DOCUMENT_STRUCTURE
DOMAIN_RULE
USER
```

Physical geometry alone MUST NOT automatically be treated as canonical reading order.

---

# Ordering Scope

Reading order is defined within a compatible ordering scope.

Possible scopes include:

* Page,
* Chapter,
* structural container,
* translation group.

The architecture MUST NOT assume Page is the only possible ordering scope.

This is required for text-native Chapters.

---

# Block Relationships

TextBlocks MAY have explicit relationships.

Examples:

```text
CONTINUES
CONTINUED_BY
PARENT_OF
CHILD_OF
OVERLAPS
ALTERNATIVE_OF
ANNOTATION_OF
DERIVED_FROM
```

Relationships MUST reference compatible domain resources.

Cross-Page relationships MAY exist.

Cross-scope relationships MUST NOT imply aggregate ownership.

---

# Character and Speaker Association

Dialogue-like TextBlocks MAY reference Character-domain identities.

Possible metadata includes:

```text
characterId
speakerConfidence
associationSource
associationRevision
```

Association sources MAY include:

* user assignment,
* visual inference,
* layout inference,
* conversation inference,
* imported metadata.

User-confirmed associations SHOULD take precedence over inferred associations.

Unknown speaker identity MUST NOT block translation.

---

# Confidence

Different inferred properties SHOULD preserve independent confidence values.

Examples:

```text
detectionConfidence
recognitionConfidence
languageConfidence
orientationConfidence
readingOrderConfidence
classificationConfidence
speakerConfidence
```

Recommended normalized range:

```text
0.0 <= confidence <= 1.0
```

`unknown` confidence MUST remain distinguishable from numeric zero.

Provider-specific confidence formats MUST be normalized before entering domain state.

---

# Split

A TextBlock MAY be split when one detected/extracted unit contains multiple logical units.

```text
Block A
  |
  +--> Block A1
  |
  +--> Block A2
```

Split creates new TextBlock identities.

The source TextBlock becomes:

```text
SUPERSEDED
```

rather than being silently rewritten into an unrelated logical shape.

Derivation lineage MUST remain available.

---

# Merge

Multiple TextBlocks MAY be merged.

```text
Block A ---\
            > Block C
Block B ---/
```

The resulting TextBlock receives a new logical identity.

Source TextBlocks become superseded.

Dependent artifacts MUST either:

* become stale,
* be invalidated,
* or be explicitly remapped through a validated reconciliation process.

---

# Regeneration and Reconciliation

TextBlocks MAY be regenerated when source extraction changes.

Possible causes:

* OCR provider change,
* OCR configuration change,
* source Image change,
* preprocessing change,
* layout algorithm change,
* language configuration change,
* source document update,
* user-requested reprocessing.

Regeneration SHOULD use reconciliation when practical.

Possible matching signals include:

* geometry overlap,
* text similarity,
* structural locator,
* reading-order proximity,
* source identity,
* content hash.

Successful reconciliation preserves stable logical identity.

Unmatched new content receives new TextBlock identities.

Unmatched prior TextBlocks become superseded or invalidated according to domain rules.

---

# Lifecycle

TextBlock lifecycle represents validity of the logical source-text resource.

Recommended lifecycle:

```text
Created
   |
   v
Active
   |
   +--> Superseded
   |
   +--> Invalidated
   |
   v
Archived
```

Recommended statuses:

```text
CREATED
ACTIVE
SUPERSEDED
INVALIDATED
ARCHIVED
```

Optional deletion lifecycle MAY include:

```text
DELETING
DELETED
```

---

# Processing State Is Separate

The following MUST NOT be core TextBlock lifecycle states:

```text
DETECTED
RECOGNIZED
NORMALIZED
TRANSLATING
TRANSLATED
NEEDS_REVIEW
```

These describe:

* derivation stages,
* Translation workflow,
* Review workflow,
* processing projections.

A TextBlock MAY be Active while Translation is:

```text
NOT_REQUESTED
RUNNING
COMPLETED
FAILED
STALE
```

without changing the TextBlock's lifecycle.

---

# Derivation Stage

When useful, source derivation metadata MAY record how the TextBlock reached canonical domain form.

Example:

```text
sourceDerivation:
    DETECTED
    RECOGNIZED
    NORMALIZED
```

This is provenance, not the TextBlock lifecycle.

A browser paragraph can become a valid TextBlock without passing through `DETECTED` or `RECOGNIZED`.

---

# Translation Association

Translation MUST remain a separate domain resource.

Recommended relationship:

```text
TextBlock Revision
       |
       +--> Translation A
       |
       +--> Translation B
```

Translation SHOULD reference:

```text
textBlockId
textBlockRevision
sourceLanguage
targetLanguage
effectiveSourceTextHash
translationProfileRevision
contextRevision
translationRevision
```

Translation MUST NOT overwrite source text.

---

# Translation Staleness

A Translation becomes stale when a processing-significant input changes.

Examples:

* effective source text revision,
* source language,
* translation profile,
* required context,
* terminology dependencies.

Stale Translation resources MAY remain available for:

* history,
* comparison,
* rollback,
* diagnostics.

They MUST NOT automatically be presented as current.

---

# Translation Grouping

Multiple TextBlocks MAY be grouped temporarily for contextual translation.

```text
Block A ---\
Block B ----> Translation Group
Block C ---/
```

A Translation Group is a Translation-processing construct.

It MUST NOT replace member TextBlock identities.

Results MUST remain mappable to logical source units.

---

# Presentation Association

Presentation consumes TextBlocks and compatible Translation resources.

Presentation MAY maintain:

```text
textBlockId
textBlockRevision
translationId?
sourceGeometry?
displayGeometry
fontProfile
overflowStrategy
visibility
layerOrder
```

Presentation-specific geometry MUST NOT overwrite canonical source geometry.

---

# Manual Correction

Users MAY correct:

* source text,
* geometry,
* ordering,
* block type,
* language,
* speaker association.

A processing-significant correction MUST:

1. preserve prior revision history where required,
2. create a new TextBlock revision,
3. make the corrected value authoritative,
4. mark incompatible dependent Translation resources stale,
5. invalidate incompatible Presentation artifacts,
6. leave unrelated resources unchanged.

Correction SHOULD invalidate only dependencies affected by the change.

---

# Review

Review is not TextBlock lifecycle.

Different review domains MAY exist:

```text
OCR Review
Source Text Review
Translation Review
Presentation Review
```

The TextBlock MAY expose derived review indicators.

Those indicators MUST NOT replace authoritative Review workflow state.

---

# Persistence

Recommended conceptual separation:

```text
TextBlock
├── identity
├── scope
├── current revision
├── effective source representation
└── lifecycle
```

```text
TextBlockRevision
├── revision
├── changed fields
├── source representations
├── geometry / locator
├── language
├── classification
├── ordering
├── relationships
├── hashes
├── actor/source
└── timestamp
```

```text
Translation
├── textBlockId
├── textBlockRevision
├── targetLanguage
├── translatedText
└── translation metadata
```

Raw provider payloads MAY be retained separately for diagnostics or reproducibility.

They MUST NOT become canonical TextBlock state.

---

# Retention

Possible retention policy:

* Active TextBlocks: retain while source content remains relevant.
* User corrections: durable domain history.
* Superseded TextBlocks: retain while referenced or required for reconciliation/history.
* Raw OCR/extraction text: policy-controlled.
* Provider payloads: short-lived unless explicitly retained.
* Geometry revisions: retain when required for reconciliation or audit.
* stale Translation resources: Translation retention policy.

Deleting TextBlocks MUST NOT leave invalid durable references.

---

# Privacy

TextBlocks may contain copyrighted, private, or sensitive source material.

Default rules SHOULD include:

* do not log raw source text,
* do not include source text in telemetry by default,
* send only necessary context to external providers,
* respect provider-retention policy,
* support temporary processing modes,
* persist only according to configured retention policy.

Diagnostics SHOULD prefer:

```text
textBlockId
revision
hash
size
language
confidence
```

over raw content.

---

# Events

Typical TextBlock domain events include:

```text
TextBlockCreated
TextBlockUpdated
TextBlockCorrected
TextBlockReordered
TextBlockClassified
TextBlockSpeakerAssigned
TextBlockSplit
TextBlocksMerged
TextBlockSuperseded
TextBlockInvalidated
TextBlockArchived
TextBlockRestored
```

Events MAY identify:

* TextBlock identity,
* revision,
* changed fields,
* actor,
* source.

Domain events SHOULD NOT contain:

* raw Image bytes,
* provider payloads,
* unnecessarily large source text.

Processing events such as:

```text
TextDetected
TextRecognized
TranslationStarted
TranslationCompleted
ReviewRequested
```

belong to their responsible modules/workflows unless they correspond to an actual TextBlock domain mutation.

---

# Validation

An Active TextBlock SHOULD satisfy:

* valid TextBlock identity,
* valid Project/content scope,
* valid effective source text,
* valid revision,
* valid source provenance,
* valid language representation,
* valid relationship references,
* valid ordering when required.

Image-derived TextBlocks MUST additionally validate:

* Image reference,
* Image version,
* coordinate compatibility,
* geometry bounds.

Structured TextBlocks SHOULD have a valid structural/source locator when required by their source model.

---

# Error Conditions

Possible stable domain errors include:

```text
TEXT_BLOCK_NOT_FOUND
TEXT_BLOCK_SCOPE_INVALID
TEXT_BLOCK_SOURCE_INVALID
TEXT_BLOCK_EMPTY_SOURCE
TEXT_BLOCK_REVISION_MISMATCH
TEXT_BLOCK_GEOMETRY_INVALID
TEXT_BLOCK_COORDINATE_MISMATCH
TEXT_BLOCK_READING_ORDER_INVALID
TEXT_BLOCK_RELATIONSHIP_INVALID
TEXT_BLOCK_RELATIONSHIP_CYCLE
TEXT_BLOCK_SUPERSEDED
TEXT_BLOCK_INVALIDATED
TEXT_BLOCK_SPLIT_INVALID
TEXT_BLOCK_MERGE_INVALID
TEXT_BLOCK_CORRECTION_CONFLICT
TEXT_BLOCK_CONFIDENCE_INVALID
```

Provider-specific failures MUST be translated before crossing domain/module boundaries.

---

# Aggregate Boundary

TextBlock SHOULD be treated as an independently addressable domain resource when its stable identity and revision history matter.

Recommended ownership:

```text
TextBlock Domain

owns
    TextBlock identity
    source provenance
    source representations
    effective source text
    revision
    classification
    language metadata
    reading-order metadata
    optional geometry
    optional locator
    source relationships
    split/merge lineage
    correction history
    lifecycle
```

It does NOT own:

```text
Page state
Image state
OCR execution
Translation execution
Review execution
Presentation execution
Rendering execution
provider/runtime state
```

---

# Transactional Consistency

TextBlock-domain mutations include:

```text
Correct source text
    -> TextBlock revision transaction
```

```text
Change classification
    -> TextBlock revision transaction
```

```text
Split TextBlock
    -> TextBlock lineage operation
```

```text
Merge TextBlocks
    -> TextBlock lineage operation
```

Translation remains separate:

```text
Translate TextBlock
    -> Translation workflow
```

Presentation remains separate:

```text
Present TextBlock
    -> Presentation workflow
```

---

# Architecture Invariants

1. `textBlockId` represents stable logical identity.

2. A TextBlock MUST belong to a valid Project/content scope.

3. A TextBlock is NOT required to belong to a Page.

4. Image-derived TextBlocks MUST reference compatible Image/Page scope where required.

5. Text-native TextBlocks MAY use structural/source scope without pixel geometry.

6. Processing-significant mutation MUST create a new TextBlock revision.

7. Translation MUST NOT overwrite source text.

8. Every Translation MUST reference the exact compatible TextBlock revision.

9. Provider-specific extraction/OCR payloads MUST NOT become canonical TextBlock representation.

10. Image-derived geometry MUST reference the exact Image identity/version that produced it.

11. Geometry MUST NOT be reused against another Image without an explicit valid transform.

12. Reading order MUST be explicit when ordering affects semantics.

13. User-corrected source text takes precedence over automatic extraction when explicitly accepted.

14. Split and merge operations MUST preserve lineage.

15. Superseded TextBlocks MUST NOT be selected as current processing input.

16. Duplicate text does not imply duplicate TextBlock identity.

17. Presentation layout MUST NOT mutate canonical source geometry.

18. TextBlock mutations SHOULD invalidate only dependent artifacts.

19. Unknown confidence MUST remain distinguishable from zero confidence.

20. TextBlocks MUST remain provider- and storage-independent.

21. Raw source text SHOULD be excluded from ordinary logs and telemetry.

22. Cross-Page relationships MUST NOT transfer ownership.

23. TextBlock lifecycle MUST remain independent from Translation, Review, and Presentation lifecycle.

24. Detection, recognition, and normalization stages MUST NOT be mandatory lifecycle states for text-native sources.

---

# Example: Comic Page

```text
Page
  |
  v
Source Image
  |
  v
Detection
  |
  v
Recognition
  |
  v
Text Analysis
  |
  +--> TextBlock 1: narration
  +--> TextBlock 2: dialogue
  +--> TextBlock 3: sound effect
             |
             v
     contextual translation
             |
             v
         Presentation
```

Each visual TextBlock references the relevant Image coordinate space.

---

# Example: Browser Novel

```text
Chapter
   |
   v
Browser Content Extraction
   |
   +--> Heading TextBlock
   +--> Paragraph TextBlock
   +--> Paragraph TextBlock
   +--> Paragraph TextBlock
              |
              v
       contextual translation
              |
              v
      formatted presentation
```

No Page entity or Image geometry is required.

---

# Example: Manual Text

```text
Chapter
   |
   v
Manual Text
   |
   v
TextBlock
   |
   v
Translation
```

This TextBlock never passes through OCR.

It is still a first-class TextBlock.

---

# Ownership Summary

```text
TextBlock Domain

owns
    logical text identity
    content scope
    source provenance
    source text representations
    effective source text
    revision history
    semantic classification
    language metadata
    reading order
    optional geometry
    optional structural locator
    relationships
    split/merge lineage
    correction state
    lifecycle

references
    Project
    optional Chapter
    optional Page
    optional Image
    optional Character
    source artifacts

consumed by
    Translation
    Review
    Presentation
    Reading
    Export

does not own
    OCR execution
    Translation execution
    Review workflow
    Presentation execution
    Image lifecycle
    Page lifecycle
    provider state
```

TextBlock therefore represents a stable logical source-text unit across both visual and text-native CRAI workflows.

---

# Open Decisions

The following decisions SHOULD remain open until prototype validation:

* whether raw OCR/extracted text is persisted by default,
* whether every TextBlock revision is persisted,
* canonical geometry representation,
* whether polygons are required for MVP,
* reconciliation strategy after re-OCR or recapture,
* sound-effect translation policy,
* segmentation of vertical Chinese text,
* mixed-language segmentation behavior,
* Translation grouping/context size,
* browser structural-locator strategy,
* stale Translation retention,
* speaker association scope for MVP,
* whether some corrections become reusable recognition knowledge,
* whether text-native content requires a dedicated parent document/content entity,
* whether reading order uses integer sequence, fractional ordering, or stable sort keys.

---

# Related Documents

Domain:

* `README.md`
* `PROJECT.md`
* `BOOK.md`
* `CHAPTER.md`
* `PAGE.md`
* `IMAGE.md`
* `TRANSLATION.md`
* `LANGUAGE.md`
* `GLOSSARY.md`
* `CHARACTER.md`
* `SESSION.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/DATA_FLOW.md`

OCR:

* `01-architecture/ocr/PIPELINE.md`
* `01-architecture/ocr/DETECTION.md`
* `01-architecture/ocr/RECOGNITION.md`
* `01-architecture/ocr/LAYOUT.md`
* `01-architecture/ocr/TEXT_DIRECTION.md`
* `01-architecture/ocr/POSTPROCESS.md`

Module contracts remain authoritative for module-specific execution ownership and runtime behavior.
