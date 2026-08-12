# Image Domain

* **Document:** Domain / Image
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

An `Image` represents a stable visual resource known to CRAI.

An Image describes:

* visual identity,
* binary asset reference,
* dimensions,
* media characteristics,
* semantic role,
* coordinate space,
* integrity,
* derivation lineage,
* and domain scope.

The Image domain does not store image bytes.

Binary content is managed by the Storage capability through an `assetId` or equivalent stable asset reference.

An Image is not inherently owned by the Page aggregate.

Instead, an Image MAY be associated with:

* a Page,
* a Chapter,
* another Image,
* a processing result,
* a presentation artifact,
* or another explicit domain scope.

---

# Domain Role

Conceptually:

```text
Domain Resource
      |
      v
Image Record
      |
      +---- metadata
      |
      +---- coordinate space
      |
      +---- lineage
      |
      +---- semantic role
      |
      v
Storage Asset
      |
      v
Binary Content
```

The Image record identifies and describes a visual resource.

Storage determines where and how the underlying bytes are physically stored.

Processing modules operate on Image identities and asset references rather than embedding binary data into domain entities.

---

# Responsibilities

The Image domain is responsible for:

* Image identity
* Asset association
* Visual metadata
* Semantic classification
* Coordinate-space definition
* Source provenance
* Derivation lineage
* Transform metadata
* Content integrity metadata
* Immutability rules
* Domain-level availability
* Domain scope/reference information

The Image domain is NOT responsible for:

* downloading remote images,
* capturing browser content,
* decoding image bytes,
* image preprocessing execution,
* OCR execution,
* layout analysis,
* rendering execution,
* image encoding,
* cache implementation,
* filesystem management,
* object-storage implementation.

Those responsibilities belong to their respective modules and infrastructure components.

---

# Identity

Every Image has a stable identifier.

Typical fields include:

```text
Image
├── imageId
├── assetId
├── projectId?
├── pageId?
├── role
├── mediaType
├── width
├── height
├── orientation?
├── byteSize?
├── contentHash
├── lifecycleStatus
├── createdAt
└── version
```

`imageId` identifies the CRAI domain resource.

`assetId` identifies the binary asset managed by Storage.

These identifiers MUST NOT be treated as interchangeable.

---

# Domain Scope

An Image MAY have a direct domain scope.

Examples:

```text
Page
  |
  v
Image
```

```text
Chapter
  |
  v
Image
```

```text
Image
  |
  v
Derived Image
```

An Image MUST NOT be required to belong to a Page unless the semantic resource genuinely represents a Page-scoped image.

Possible scope fields MAY include:

```text
projectId?
chapterId?
pageId?
parentImageId?
```

The exact scope MUST remain explicit.

An Image MUST NOT silently change scope because the same binary content appears elsewhere.

---

# Page Relationship

Page-based content commonly references Images.

Typical relationship:

```text
Page
    sourceImageId
        |
        v
Image
```

A Page MAY reference:

* one source Image,
* multiple source Images,
* preferred visual variants,
* thumbnails,
* or selected derived Images.

The Page owns those references where they are part of Page domain state.

The Image owns its own metadata and lineage.

A Page association does not make Image part of a Page-wide transactional aggregate.

---

# Image and Asset

Image and Asset represent different concepts.

```text
Image
    domain identity
    semantic meaning
    visual metadata
    lineage
    coordinate space
         |
         v
Asset
    binary identity
    storage reference
    binary lifecycle
```

One Image normally references one Asset version.

Multiple Image domain records MAY reference the same physical Asset when Storage deduplication is appropriate.

Storage deduplication MUST NOT collapse distinct domain identities.

---

# Semantic Role

An Image SHOULD declare its semantic role.

Recommended high-level roles include:

```text
SOURCE
DERIVED
PRESENTATION
PREVIEW
THUMBNAIL
```

These values describe the Image's broad domain meaning.

More specific processing purpose SHOULD be represented separately where needed.

---

# Processing Purpose

A derived Image MAY declare the processing purpose that produced it.

Examples:

```text
NORMALIZED
OCR_INPUT
REGION_CROP
DESKEWED
UPSCALED
BACKGROUND_CLEAN
MASK
INPAINTED
RENDER_OUTPUT
```

This separates:

```text
semantic role
```

from:

```text
processing purpose
```

Example:

```text
role: DERIVED
purpose: OCR_INPUT
```

instead of continually expanding a single Image-role taxonomy.

---

# Source Type

Source Images MAY describe how the visual content entered CRAI.

Possible source types include:

```text
BROWSER_CAPTURE
SCREEN_CAPTURE
REMOTE_URL
LOCAL_FILE
CLIPBOARD
CAMERA
SCANNER
ARCHIVE_ENTRY
PDF_RASTERIZATION
GENERATED
OTHER
```

Source type is descriptive metadata.

It MUST NOT create a dependency on a specific capture or import adapter.

---

# Metadata

Image metadata MAY include:

* MIME type
* file extension
* pixel width
* pixel height
* aspect ratio
* orientation
* rotation
* color space
* bit depth
* alpha-channel presence
* DPI
* animation flag
* frame count
* compression
* byte size

Only metadata required by domain behavior SHOULD become first-class fields.

Additional technical metadata MAY be stored as extensible attributes.

---

# Media Type

Image media type SHOULD use a stable normalized value.

Examples:

```text
image/png
image/jpeg
image/webp
image/avif
image/gif
```

File extension MUST NOT be used as the authoritative media-type source.

Processing capabilities remain responsible for validating actual decoder support.

---

# Coordinate Space

Each Image defines a coordinate space for geometry derived from that Image.

Canonical raster coordinate convention:

```text
origin: top-left
x-axis: left -> right
y-axis: top -> bottom
unit: pixels
```

Coordinates are interpreted against the exact Image geometry that produced them.

A coordinate-bearing artifact SHOULD reference at least:

```text
imageId
imageVersion
width
height
```

and, when required:

```text
transform
```

---

# Coordinate Identity

Geometry MUST NOT be interpreted against an Image merely because dimensions happen to match.

The producing Image identity and version are part of the coordinate context.

Example:

```text
TextRegion
    imageId: image_002
    imageVersion: 3
    bounds: ...
```

Using those bounds directly against another Image without an explicit transform is invalid.

---

# Orientation

Encoded pixel orientation and metadata orientation MAY differ.

Image metadata SHOULD preserve enough information to understand the original orientation.

Processing workflows MAY derive a normalized Image.

Example:

```text
Source Image
    orientation metadata
         |
         v
Normalization
         |
         v
Derived Image
    normalized pixels
```

Normalization MUST NOT mutate the original Image.

---

# Transform

Pixel-changing operations create new Image resources or new immutable Image versions.

Typical transforms include:

* rotation,
* scaling,
* cropping,
* padding,
* deskewing,
* perspective correction,
* color conversion,
* upscaling.

A derived Image SHOULD preserve the transform relationship to its parent where coordinate mapping is relevant.

---

# Transform Mapping

Conceptually:

```text
Parent Image
     |
     | transform T
     v
Derived Image
```

Where possible, CRAI SHOULD preserve enough metadata to support:

```text
parent coordinates
        <->
derived coordinates
```

Not every transformation is perfectly invertible.

When reverse mapping is impossible or lossy, that fact MUST be explicit.

---

# Image Lineage

Derived Images form a directed lineage graph.

Typical fields include:

```text
parentImageId
derivationType
processingProfileId?
configurationVersion?
inputContentHash
outputContentHash
transform?
createdAt
```

Example:

```text
Source
  |
  | normalize
  v
Normalized
  |
  | crop
  v
Region Crop
  |
  | upscale
  v
OCR Input
```

Lineage supports:

* reproducibility,
* cache validation,
* diagnostics,
* auditing,
* coordinate mapping,
* safe cleanup.

---

# Multiple Parents

Some generated Images MAY depend on multiple source Images.

Examples include:

* composites,
* stitched screenshots,
* overlays,
* page assemblies.

Such resources MAY use multiple lineage inputs.

Conceptually:

```text
Image A ----\
             \
              > Derived Image
             /
Image B ----/
```

The lineage model MUST NOT assume every derivation has exactly one parent.

---

# Immutability

Pixel identity is immutable.

Once an Image record refers to a particular canonical binary content:

```text
imageId
    -> contentHash
```

that pixel content MUST NOT change in place.

Operations such as:

* crop,
* rotate,
* enhance,
* clean,
* resize,
* normalize,

produce a new Image or new immutable version according to implementation policy.

Source Image records SHOULD normally remain immutable for their entire lifetime.

---

# Versioning

Two valid approaches MAY be used:

```text
new imageId for pixel-changing derivation
```

or:

```text
immutable image version
```

However, the architecture MUST clearly distinguish:

* metadata updates that preserve binary identity,
* pixel-changing operations that alter visual identity.

For most processing lineage, a new `imageId` is preferred because it simplifies:

* coordinate references,
* cache identity,
* audit history,
* reproducibility.

---

# Integrity

Every persistent Image SHOULD have a canonical content hash when practical.

The content hash MAY support:

* duplicate detection,
* idempotent import,
* cache keys,
* corruption detection,
* integrity validation,
* derivation verification,
* storage reconciliation.

Image equality MUST NOT be determined using:

* filename,
* URL,
* timestamps,
* dimensions alone.

---

# Deduplication

Multiple domain Images MAY reference identical binary content.

Example:

```text
Page A
  |
  v
Image A ----\
             \
              -> Asset X
             /
Image B ----/
  ^
  |
Page B
```

Storage MAY deduplicate Asset X.

Image A and Image B remain distinct domain resources if their domain scope or lineage differs.

Binary equality does not imply domain identity equality.

---

# Availability

Image availability describes whether its underlying Asset can currently be used.

Possible values MAY include:

```text
AVAILABLE
UNAVAILABLE
INVALID
```

Availability does not describe whether OCR or translation succeeded.

---

# Lifecycle

Recommended Image domain lifecycle:

```text
Registered
    |
    v
Active
    |
    v
Archived
    |
    v
Released
```

Possible explicit deletion states MAY include:

```text
DELETING
DELETED
```

Meaning:

* `Registered`: domain record exists.
* `Active`: Image is available for normal domain use.
* `Archived`: Image is retained but no longer selected for normal use.
* `Released`: retention permits associated binary cleanup when no remaining references require it.

Processing-specific states MUST NOT become Image lifecycle states.

---

# Usage State

Whether an Image is:

* referenced,
* cached,
* used by OCR,
* used by presentation,
* used by export,

is NOT part of the core Image lifecycle.

Such information MAY be queried or represented by projections.

This prevents domain lifecycle from changing merely because another resource starts or stops referencing the Image.

---

# Retention

Retention policy depends on semantic role, reproducibility, cost, and references.

Possible guidance:

```text
SOURCE
    long-lived

DERIVED / expensive
    retain while useful or referenced

DERIVED / cheap
    regenerable

PREVIEW
    short-lived

THUMBNAIL
    regenerable

PRESENTATION
    according to presentation/export policy
```

The Image domain defines semantic retention intent.

Storage determines safe physical deletion according to:

* references,
* retention policy,
* storage rules,
* cleanup strategy.

---

# Source Protection

Authoritative source Images MUST NOT be deleted solely because a derived Image exists.

Example:

```text
Source
  |
  +--> Normalized
  |
  +--> OCR Input
```

Deleting `OCR Input` MUST NOT delete `Source`.

Cleanup MUST respect lineage direction and retention policies.

---

# Processing Compatibility

Capabilities MAY validate Images before execution.

Possible checks include:

* supported media type,
* minimum dimensions,
* maximum dimensions,
* maximum byte size,
* decodability,
* orientation,
* alpha-channel support,
* animation support,
* color-space support.

Compatibility state belongs to the consuming capability.

A compatibility failure MUST NOT automatically invalidate the Image domain resource.

---

# OCR Use

OCR capabilities MAY consume:

```text
imageId
assetId
```

along with effective processing configuration.

Typical flow:

```text
Image
   |
   v
Preprocess
   |
   v
Derived Image
   |
   v
Detection / Recognition
```

OCR runtime state remains owned by OCR-related modules.

---

# Layout Use

Layout results MUST identify the Image coordinate space they were derived from.

Example:

```text
LayoutResult
    imageId
    imageVersion
    geometry
```

Layout MUST NOT assume the current preferred Page image is the same Image that originally produced the result.

---

# Text Region Use

Text-region geometry SHOULD reference the exact Image that produced it.

Example:

```text
TextRegion
├── imageId
├── geometry
└── confidence
```

If a region is mapped to another Image, the transform used MUST be explicit or recoverable from lineage.

---

# Presentation Use

Presentation MAY generate Images such as:

* translated page images,
* previews,
* cleaned backgrounds,
* composited outputs.

Those generated Images remain Images in the domain when they require:

* stable identity,
* storage,
* lineage,
* coordinate semantics,
* reuse.

Presentation execution state remains owned by Presentation.

---

# Temporary Visual Buffers

Not every in-memory visual representation should become an Image domain record.

Examples that SHOULD normally remain runtime-only:

* decoded buffers,
* intermediate tensors,
* temporary masks,
* short-lived GPU surfaces,
* internal provider representations.

A domain Image SHOULD exist only when stable identity or lifecycle matters.

---

# Persistence

Recommended persistence separation:

```text
Image Record
├── imageId
├── assetId
├── scope
├── role
├── purpose?
├── dimensions
├── media metadata
├── contentHash
├── coordinate metadata
├── lineage
└── lifecycle

Storage Asset
└── binary content
```

Image records MUST NOT contain binary image payloads.

---

# Diagnostics

Useful diagnostic metadata MAY include:

* imageId,
* assetId,
* scope identifiers,
* role,
* processing purpose,
* dimensions,
* byte size,
* content-hash prefix,
* parent Image identities,
* derivation type,
* validation result.

Runtime diagnostics such as:

* decoder used,
* processing duration,
* GPU memory,
* worker identity,

belong to processing/observability records rather than permanent Image domain state unless explicitly required.

---

# Logging

Ordinary logs SHOULD avoid including:

* raw binary data,
* full base64 content,
* sensitive source URLs,
* unnecessary user content.

Stable identifiers SHOULD be preferred.

Example:

```text
imageId
assetId
pageId
processingJobId
```

---

# Errors

Possible stable domain errors include:

```text
IMAGE_NOT_FOUND
IMAGE_ASSET_UNAVAILABLE
IMAGE_METADATA_INVALID
IMAGE_DIMENSIONS_INVALID
IMAGE_CONTENT_HASH_MISMATCH
IMAGE_PARENT_NOT_FOUND
IMAGE_LINEAGE_INVALID
IMAGE_COORDINATE_MISMATCH
IMAGE_IMMUTABILITY_VIOLATION
IMAGE_SCOPE_INVALID
```

Provider-, decoder-, network-, filesystem-, and storage-specific failures SHOULD be translated at architecture boundaries.

---

# Events

Typical Image domain events include:

```text
ImageRegistered
ImageActivated
ImageDerived
ImageMetadataUpdated
ImageArchived
ImageReleased
ImageDeleted
ImageIntegrityFailed
ImageAvailabilityChanged
```

Events SHOULD contain identifiers and domain metadata.

They MUST NOT contain raw image bytes.

Execution events such as:

```text
ImagePreprocessed
ImageDecoded
ImageRendered
```

belong to the module that performed the operation unless they represent a true Image-domain state transition.

---

# Aggregate Boundary

Image SHOULD be independently addressable when its identity, lineage, or lifecycle matters.

Recommended ownership:

```text
Image Domain

owns
    Image identity
    Asset reference
    semantic role
    visual metadata
    coordinate space
    integrity metadata
    lineage
    Image lifecycle

does not own
    Asset binary data
    Page state
    OCR execution
    Layout execution
    Translation execution
    Presentation execution
    provider/runtime state
```

An Image MAY therefore act as its own small aggregate/resource rather than living inside a large Page aggregate.

---

# Transactional Consistency

Typical Image-owned operations include:

```text
Register Image
    -> Image transaction
```

```text
Update non-pixel metadata
    -> Image transaction
```

```text
Create derived Image
    -> new Image + lineage operation
```

Processing execution:

```text
Run preprocessing
    -> preprocessing workflow
```

```text
Run OCR
    -> OCR workflow
```

```text
Generate translated page
    -> Presentation workflow
```

Processing workflows MAY create Images but do not transfer their runtime state into the Image aggregate.

---

# Architecture Invariants

1. `imageId` is immutable.

2. `assetId` and `imageId` represent different identities.

3. Image binary content MUST NOT change in place while retaining the same visual identity.

4. Pixel-changing operations MUST produce a new Image identity or immutable Image version.

5. Every persistent Image binary MUST be referenced through Storage-managed asset identity.

6. Coordinate-bearing artifacts MUST reference the exact Image identity/version that produced their geometry.

7. Derived Images MUST preserve sufficient lineage to identify their inputs.

8. Coordinate transforms MUST be explicit when geometry is mapped between different Images.

9. Binary equality MUST be determined using canonical content identity, not filenames or URLs.

10. Binary deduplication MUST NOT collapse distinct domain Image identities.

11. Image domain records MUST NOT contain raw image bytes.

12. Image lifecycle MUST remain independent from OCR, translation, presentation, and processing job lifecycles.

13. Processing failure MUST NOT mutate authoritative source Image pixels.

14. Deleting a derived Image MUST NOT implicitly delete authoritative ancestor Images.

15. Image events MUST NOT contain raw image bytes.

16. Storage implementation details MUST NOT leak into the Image domain.

17. An Image MAY exist without a Page when another valid domain scope or lineage relationship exists.

18. A Page association MUST remain explicit when present.

---

# Failure Isolation

Processing failures MUST remain isolated from Image identity.

Examples:

```text
OCR failed
    != Image corrupted

Translation failed
    != Image corrupted

Presentation failed
    != Image corrupted
```

An Image becomes invalid only when an Image-domain condition exists, such as:

* invalid metadata,
* unavailable or corrupted Asset,
* integrity mismatch,
* broken lineage required for domain correctness.

---

# Scalability

Image resources SHOULD support independent lookup.

Typical APIs MAY include:

```text
getImage(imageId)

getImageAsset(imageId)

listImagesByPage(pageId)

listDerivedImages(imageId)

getImageLineage(imageId)
```

Large lineage graphs SHOULD be queried incrementally.

Image retrieval MUST NOT require loading:

* OCR results,
* translations,
* presentation artifacts,
* entire Page state.

---

# Example: Page Source Image

```text
Image
  imageId: image_001
  assetId: asset_001

  projectId: project_001
  pageId: page_001

  role: SOURCE

  mediaType: image/jpeg
  width: 1600
  height: 2400

  contentHash: sha256:...

  lifecycleStatus: ACTIVE
```

---

# Example: OCR Input

```text
Image
  imageId: image_002
  assetId: asset_002

  projectId: project_001
  pageId: page_001

  role: DERIVED
  purpose: OCR_INPUT

  width: 2400
  height: 3600

  parentImageId: image_001

  derivation:
    type: UPSCALE
    configurationVersion: 3

  lifecycleStatus: ACTIVE
```

OCR geometry generated from this Image MUST reference `image_002`, not merely `page_001`.

---

# Example: Shared Binary

```text
Image A
  imageId: image_a
  pageId: page_a
  assetId: asset_shared

Image B
  imageId: image_b
  pageId: page_b
  assetId: asset_shared
```

Storage MAY keep only one physical binary.

Image A and Image B remain separate domain identities.

---

# Ownership Summary

```text
Image Domain

owns
    Image identity
    Asset association
    semantic role
    processing-purpose metadata
    visual metadata
    canonical coordinate space
    content integrity metadata
    derivation lineage
    Image lifecycle

references
    optional Project
    optional Chapter
    optional Page
    parent/source Images
    Storage Asset

does not own
    binary storage
    Page lifecycle
    preprocessing execution
    OCR execution
    layout execution
    translation execution
    presentation execution
    cache implementation
    runtime buffers
```

Image is therefore an immutable visual-resource and lineage domain, not a child processing object embedded inside Page.

---

# Related Documents

Domain:

* `README.md`
* `PROJECT.md`
* `BOOK.md`
* `CHAPTER.md`
* `PAGE.md`
* `TEXT_BLOCK.md`
* `TRANSLATION.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/DATA_FLOW.md`

OCR:

* `01-architecture/ocr/PIPELINE.md`
* `01-architecture/ocr/PREPROCESS.md`
* `01-architecture/ocr/DETECTION.md`
* `01-architecture/ocr/RECOGNITION.md`
* `01-architecture/ocr/LAYOUT.md`
* `01-architecture/ocr/TEXT_DIRECTION.md`
* `01-architecture/ocr/POSTPROCESS.md`

Infrastructure / Runtime:

* `../storage/CONTRACT.md`
* `../storage/BACKENDS.md`
* `../runtime/RESOURCE_LIFECYCLE.md`
* `../runtime/CACHE_POLICY.md`

Module contracts remain authoritative for module-specific execution ownership and runtime behavior.
