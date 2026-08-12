# Page Domain

* **Document:** Domain / Page
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

A `Page` represents one ordered visual content unit within a Chapter.

A Page is primarily used for image-oriented content such as:

* manga,
* manhua,
* manhwa,
* comics,
* scanned books,
* illustrated documents,
* image-based reading material.

A Page provides:

* stable identity,
* Chapter association,
* page ordering,
* source visual references,
* page metadata,
* visual coordinate context,
* and page-level content scope.

A Page MAY be used as an execution scope by OCR, layout, translation, rendering, review, or export capabilities.

However, Page does **not** own the authoritative runtime state of those processing capabilities.

---

# Domain Role

Typical hierarchy:

```text
Project
└── Book?
    └── Chapter
        └── Page
```

A Page represents the visual reading surface consumed by downstream capabilities.

Conceptually:

```text
Page
   |
   +--> Source Image / Visual Asset
   |
   +--> OCR / Recognition
   |
   +--> Layout Analysis
   |
   +--> Translation
   |
   +--> Presentation
   |
   +--> Review
```

The Page supplies identity and visual scope.

Each capability retains ownership of its own execution state and artifacts.

---

# Applicability

Page is NOT a mandatory content unit for every Chapter.

Image-oriented Chapter:

```text
Chapter
└── Page
```

Text-oriented Chapter:

```text
Chapter
└── Text Content
```

Mixed Chapter:

```text
Chapter
├── Page
└── Text Content
```

The domain model MUST NOT create artificial Pages solely to represent text-native content.

---

# Responsibilities

A Page is responsible for:

* Page identity
* Chapter association
* Page ordering
* Page metadata
* Source visual references
* Visual dimensions
* Coordinate-space definition
* Orientation metadata
* page-level content references
* page-level preferences or hints
* Page lifecycle

A Page MAY expose derived processing summaries.

A Page MUST NOT own authoritative state for:

* preprocessing execution
* OCR execution
* recognition execution
* layout execution
* translation execution
* rendering execution
* presentation execution
* review execution
* export execution
* provider runtime state

---

# Identity

Every Page has an immutable identifier.

Typical fields include:

```text
Page
├── pageId
├── projectId
├── chapterId
├── sequence
├── pageNumber?
├── sourceAssetId
├── width?
├── height?
├── orientation?
├── lifecycleStatus
├── createdAt
├── updatedAt
└── version
```

`pageId` MUST remain immutable.

`chapterId` identifies the Chapter that owns the Page membership.

`projectId` MAY be stored explicitly for efficient scoping and authorization.

If stored, it MUST match the Project associated with the Chapter.

---

# Parent Relationship

Every Page belongs to exactly one Chapter.

```text
Chapter
├── Page A
├── Page B
└── Page C
```

A Page MUST NOT belong to multiple Chapters simultaneously.

Moving a Page between Chapters MAY be supported through an explicit structural operation.

If moved, both Chapters MUST belong to the same Project unless an explicit cross-project migration workflow exists.

---

# Ordering

Page order MUST be deterministic within a Chapter.

Typical fields MAY include:

```text
sequence
pageNumber
displayLabel
```

Example:

```text
sequence: 30
pageNumber: "3"
displayLabel: "Page 3"
```

`sequence` SHOULD be used as the canonical navigation order.

Display-oriented page numbers MUST NOT be the only ordering mechanism.

Duplicate canonical ordering positions MUST be prevented or resolved deterministically.

---

# Source Visual

A Page normally references one primary source visual.

Example:

```text
Page
    sourceAssetId
        |
        v
Image / Asset
```

The Page stores the reference.

The underlying binary data belongs to the appropriate Asset or Storage domain.

Page MUST NOT embed large source binaries directly into the domain entity.

---

# Multiple Visual Sources

A Page MAY reference more than one visual resource when required.

Examples:

* original image,
* high-resolution image,
* normalized image,
* alternate source image,
* cropped variant.

These references MUST have explicit semantic roles.

Example:

```text
PageVisualReferences
├── originalAssetId
├── preferredSourceAssetId
└── thumbnailAssetId
```

Derived processing artifacts SHOULD NOT automatically become Page-owned state.

---

# Dimensions

A Page MAY define its canonical coordinate space.

Typical values include:

```text
width
height
orientation
rotation
```

These values provide a common reference for:

* text-region geometry,
* layout geometry,
* overlays,
* presentation,
* user selection.

The canonical coordinate system MUST be explicitly defined by the relevant image/layout architecture.

---

# Coordinate Space

Page-scoped geometric data SHOULD reference a consistent coordinate system.

Conceptually:

```text
Page
width: W
height: H

origin: (0, 0)
```

Text regions, bounding boxes, polygons, masks, and overlays MAY reference this Page coordinate space.

Processing modules MUST NOT silently mix incompatible coordinate systems.

Transformations between coordinate spaces MUST be explicit when preprocessing changes dimensions, rotation, crop, or scale.

---

# Metadata

Page metadata MAY include:

* source page number
* display label
* source URL
* source asset reference
* width
* height
* aspect ratio
* orientation
* rotation
* publication notes
* import timestamp
* source revision

Metadata describes the Page as a visual content unit.

It MUST NOT be used to store authoritative runtime processing status.

---

# Lifecycle

Page lifecycle describes whether the Page exists and is available inside CRAI.

Recommended lifecycle:

```text
Created
   |
   v
Active
   |
   v
Archived
```

Optional deletion lifecycle MAY include:

```text
DELETING
DELETED
```

Processing states such as:

```text
PREPROCESSED
OCR_COMPLETED
TRANSLATED
RENDERED
REVIEWED
EXPORTED
```

MUST NOT be used as the Page lifecycle.

Those belong to the corresponding workflows or jobs.

---

# Import

Importing a Page is an acquisition/content operation.

Example:

```text
Page
    lifecycle: ACTIVE

Import Job
    status: COMPLETED
```

A failed or retried import MUST NOT become a generic Page processing state.

If the source Page cannot be established successfully, the acquisition workflow MAY decide whether the incomplete Page entity should remain or be removed.

---

# Processing Scope

Page is a common processing scope.

Capabilities MAY receive:

```text
pageId
```

as part of their input.

Typical execution:

```text
Page
  |
  +--> Preprocess
  |
  +--> Detect
  |
  +--> Recognize
  |
  +--> Layout
  |
  +--> Translate
  |
  +--> Present
```

Using Page as an execution scope does not make Page the owner of those jobs.

---

# Preprocessing

Image preprocessing MAY operate on a Page source visual.

Example:

```text
Page source image
       |
       v
Preprocessing Module
       |
       v
Derived Image
```

The preprocessing module owns:

* execution state,
* transformation metadata,
* preprocessing diagnostics,
* produced artifacts.

The Page MAY reference an accepted/preferred derived visual when required by domain policy.

---

# OCR and Recognition

OCR-related processing operates on Page-scoped images or regions.

Typical flow:

```text
Page
 |
 v
Image
 |
 v
Detection
 |
 v
Recognition
```

The Page does not own OCR runtime state.

Recognition output MAY reference:

```text
pageId
imageId
regionId
```

depending on the processing architecture.

The Page MAY expose derived OCR coverage for UI or navigation purposes.

---

# Layout

Layout analysis MAY use Page as its visual scope.

Example:

```text
Page
   |
   v
Layout Analysis
   |
   v
Layout Result
```

Layout results remain owned by the relevant layout capability.

Page MUST NOT duplicate complete layout state as embedded authoritative data.

---

# Text Blocks

Text Blocks MAY be spatially associated with a Page.

Typical relationship:

```text
Page
└── Text Block References
```

or:

```text
Recognition Result
└── Text Blocks
      |
      +--> pageId
```

The precise ownership model is defined by `TEXT_BLOCK.md` and the recognition/layout architecture.

Page provides:

* visual scope,
* coordinate context,
* membership/discovery scope.

Page does NOT automatically own all Text Block internal state.

---

# Translation

Translation operates on recognized or native text content.

Conceptually:

```text
Page
 |
 v
Text Blocks
 |
 v
Translation Module
 |
 v
Translation Records
```

Translation Records MAY reference `pageId` for discovery or context.

The Page MUST NOT store authoritative translation state.

Fields such as:

```text
translationCompleted
translationStatus
```

SHOULD NOT become authoritative Page fields.

---

# Rendering and Presentation

Rendered or presented output may be generated for Page-scoped content.

Example:

```text
Page
 |
 +--> Source visual
 |
 +--> Text / Translation
 |
 v
Presentation Module
 |
 v
Presentation Artifact
```

Presentation execution state and rendered artifacts remain owned by the responsible capability.

A Page MAY reference a selected or current presentation artifact when the product requires it.

Such a reference MUST NOT transfer artifact lifecycle ownership to Page.

---

# Review

Review MAY occur at Page scope.

Examples:

* OCR review,
* translation review,
* layout review,
* presentation review.

These workflows have independent state.

The Page MUST NOT have one generic `REVIEWED` state intended to represent every type of review.

A derived summary MAY be exposed if required.

---

# Export

Export is an operation over content or presentation artifacts.

Possible export scopes include:

```text
Page
Chapter
Book
Project
```

Therefore export MUST NOT be treated as an intrinsic Page lifecycle stage.

An exported artifact MAY reference a Page.

The Page does not become immutable merely because one exported artifact exists.

---

# Processing Summary

A Page MAY expose derived processing information.

Example:

```text
PageProcessingSummary
├── preprocessingStatus
├── detectionStatus
├── recognitionStatus
├── layoutStatus
├── translationStatus
├── presentationStatus
├── reviewStatus
├── issueCount
└── lastProcessedAt
```

This is a projection or cached summary.

It MUST NOT become the authoritative state machine for underlying processing modules.

---

# Parallel Execution

Pages SHOULD support independent processing where capability semantics allow it.

Example:

```text
Chapter

Page 1 ---> OCR
Page 2 ---> OCR
Page 3 ---> OCR
```

Failure processing one Page SHOULD NOT block unrelated Pages.

However, Page independence MUST NOT imply that every operation is necessarily Page-local.

Some capabilities MAY require:

* neighboring Pages,
* Chapter context,
* glossary context,
* character context,
* cross-page layout context,
* shared translation context.

Modules define those requirements explicitly.

---

# Retry

Processing operations MAY be independently retryable.

Retry behavior belongs to the corresponding execution/job architecture.

Example:

```text
Recognition Job
    pageId: page_001
    attempt: 3
```

Retry counters MUST NOT become Page-owned domain state unless used only as derived diagnostic projections.

---

# Cache Scope

Page MAY be a useful cache-key dimension.

Example:

```text
CacheKey
    pageId
    inputVersion
    configurationVersion
    pipelineVersion
```

However, Page is NOT necessarily the universal cache boundary.

Caches MAY exist at:

* asset scope,
* image scope,
* region scope,
* text-block scope,
* Page scope,
* Chapter scope,
* provider-request scope.

The responsible capability defines its own cache granularity.

---

# Cache Validity

Cached processing artifacts SHOULD consider all inputs that affect the result.

Possible inputs include:

```text
source identity/version
configuration
model/provider
pipeline version
language
processing options
```

A Page modification MUST NOT blindly invalidate unrelated cache entries.

Cache invalidation policy belongs to the responsible capability.

---

# Diagnostics

Diagnostics MAY be associated with Page-scoped processing.

Examples:

* detection warnings,
* OCR confidence issues,
* invalid geometry,
* untranslated regions,
* presentation overflow.

Diagnostics are owned by the module that produced them.

The Page MAY provide a query scope:

```text
listDiagnostics(pageId)
```

Page SHOULD NOT maintain duplicate authoritative diagnostic records.

---

# Progress

Page processing progress is derived information.

Possible UI summary:

```text
OCR       100%
Translation 80%
Presentation 50%
```

These values are calculated from processing-owned states.

They MUST NOT define Page lifecycle.

---

# Assets

Page-related assets MAY include:

```text
source image
normalized image
cropped image
mask
preview
presentation image
export artifact
```

These assets may all be associated with the same Page while having different owners and lifecycles.

Association with Page does NOT imply Page aggregate ownership.

---

# Aggregate Boundary

Recommended Page aggregate scope:

```text
Page Aggregate

owns
    Page identity
    Chapter association
    Page metadata
    Page lifecycle
    Page ordering
    source visual references
    canonical dimensions
    Page preferences/hints
```

It does NOT transactionally contain:

```text
Images
OCR Results
Detection Results
Recognition Results
Layout Results
Text Blocks
Translation Records
Render Layers
Presentation Artifacts
Diagnostics
Export Artifacts
Processing Jobs
```

Those resources MAY reference Page while retaining independent ownership.

---

# Transactional Consistency

Page-owned operations:

```text
Change Page metadata
    -> Page transaction
```

```text
Change Page sequence
    -> Page / Chapter ordering operation
```

```text
Change source asset reference
    -> Page transaction
```

Processing operations:

```text
Run OCR
    -> OCR / Recognition workflow
```

```text
Analyze layout
    -> Layout workflow
```

```text
Translate text
    -> Translation workflow
```

```text
Generate presentation
    -> Presentation workflow
```

These MUST NOT require a Page-wide aggregate transaction.

---

# Events

Typical Page domain events include:

```text
PageCreated
PageMetadataUpdated
PageActivated
PageArchived
PageRestored
PageDeleted
PageOrderChanged
PageSourceChanged
PageDimensionsChanged
```

Processing events remain owned by their respective capabilities.

Examples:

```text
ImagePreprocessed
TextDetected
TextRecognized
LayoutAnalyzed
TranslationCompleted
PresentationGenerated
ReviewCompleted
ExportCompleted
```

Those are not Page domain events merely because they include `pageId`.

---

# Invariants

1. `pageId` is immutable.

2. Every Page belongs to exactly one Chapter.

3. The Chapter associated with the Page MUST exist.

4. Page ordering MUST be deterministic within its Chapter.

5. Page source visual references MUST be valid according to Asset domain rules.

6. Canonical Page dimensions, when known, MUST be internally consistent.

7. Page metadata MUST NOT contain authoritative processing runtime state.

8. Page lifecycle MUST remain independent from OCR, translation, presentation, review, and export lifecycles.

9. A processing failure MUST NOT automatically invalidate Page domain state.

10. Processing artifacts MAY reference Page without belonging to the Page aggregate.

11. Derived processing summaries MUST NOT become the sole authoritative state.

12. Archived Pages MUST reject normal mutation unless explicitly permitted.

13. Page retrieval MUST NOT require loading every related processing artifact.

14. Coordinate transformations MUST be explicit when derived images use a different geometry from the canonical Page coordinate space.

---

# Failure Isolation

Processing failures SHOULD remain isolated.

Examples:

```text
OCR failed
    != Page failed

Recognition failed
    != Page failed

Translation failed
    != Page failed

Presentation failed
    != Page failed

Export failed
    != Page failed
```

The Page remains a valid content resource unless its own domain state or source becomes invalid.

---

# Scalability

Page design MUST support highly parallel workloads.

Typical pattern:

```text
Chapter
├── Page 1 --> processing workers
├── Page 2 --> processing workers
├── Page 3 --> processing workers
└── ...
```

Processing modules SHOULD use stable identifiers rather than requiring complete Page graphs.

Typical access:

```text
getPage(pageId)

getPageSource(pageId)

listTextBlocks(pageId)

getProcessingSummary(pageId)
```

The latter operations may query independently owned resources.

---

# Example

```text
Page
  pageId: page_003
  projectId: project_001
  chapterId: chapter_001

  sequence: 30
  pageNumber: "3"

  sourceAssetId: asset_page_003

  width: 1600
  height: 2400

  orientation: PORTRAIT
  lifecycleStatus: ACTIVE
```

Processing data remains separate:

```text
Detection Result
    pageId: page_003

Recognition Result
    pageId: page_003

Translation Records
    pageId: page_003

Presentation Artifact
    pageId: page_003
```

The Page does not embed those complete resources.

---

# Ownership Summary

```text
Page Domain

owns
    Page identity
    Chapter association
    Page ordering
    Page metadata
    Page lifecycle
    source visual references
    canonical geometry
    Page-level preferences/hints

references
    Chapter
    Assets
    Images
    Page-scoped content resources

derives
    OCR progress
    recognition progress
    translation progress
    presentation progress
    review progress
    diagnostics summary

does not own
    Asset binary data
    Image processing state
    OCR execution
    Detection execution
    Recognition execution
    Layout execution
    Text Block internal state
    Translation execution
    Presentation execution
    Review execution
    Export execution
    Processing jobs
```

Page therefore acts as an ordered visual content and coordinate scope, not as a monolithic processing aggregate.

---

# Related Documents

Domain:

* `README.md`
* `PROJECT.md`
* `BOOK.md`
* `CHAPTER.md`
* `IMAGE.md`
* `TEXT_BLOCK.md`
* `TRANSLATION.md`
* `SESSION.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/DATA_FLOW.md`

OCR / processing architecture:

* `01-architecture/ocr/PIPELINE.md`
* `01-architecture/ocr/PREPROCESS.md`
* `01-architecture/ocr/DETECTION.md`
* `01-architecture/ocr/RECOGNITION.md`
* `01-architecture/ocr/LAYOUT.md`
* `01-architecture/ocr/TEXT_DIRECTION.md`
* `01-architecture/ocr/POSTPROCESS.md`
* `01-architecture/ocr/QUALITY.md`
* `01-architecture/ocr/PROVIDERS.md`

Module contracts remain authoritative for module-specific execution ownership and runtime behavior.
