# Image Domain

- **Document:** Domain / Image
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

An Image represents a visual asset associated with a Page.

It provides the immutable visual input used by OCR, layout analysis, text-region detection, rendering and export. An Image is not an independent aggregate root; it exists within the Page aggregate or is referenced by it through a managed asset identifier.

The domain model describes the identity, role, metadata and lineage of an image without defining storage-provider or image-processing implementation details.

---

# Domain Role

Within CRAI, an Image may serve one of several roles:

- Original source image
- Normalized source image
- Preprocessed OCR input
- Cropped region
- Background-cleaned image
- Rendered preview
- Final translated output
- Thumbnail

The role determines how the image may be used, replaced, cached and retained.

---

# Ownership Boundary

```text
Page Aggregate
├── Source Image Reference
├── Derived Image References
├── OCR Result
├── Layout
├── Text Blocks
├── Translation Results
└── Render Layers
```

The Page is responsible for deciding which image is authoritative for each processing stage.

Binary image data may be stored outside the domain model by the Storage module. The domain retains only stable asset references, metadata, integrity information and lineage.

---

# Responsibilities

The Image domain object is responsible for:

- Identifying a visual asset
- Describing its dimensions and format
- Declaring its semantic role
- Recording its source and derivation
- Preserving orientation and coordinate-space metadata
- Providing integrity information
- Supporting deterministic cache invalidation
- Protecting immutable source assets

It is not responsible for:

- Downloading remote images
- Decoding image bytes
- Running image filters
- Performing OCR
- Detecting layout
- Rendering translated text
- Choosing storage backends

Those responsibilities belong to Capture, OCR, Rendering, Runtime and Storage architecture components.

---

# Identity

Typical fields include:

- Image ID
- Page ID
- Asset ID
- Role
- Media Type
- Width
- Height
- Orientation
- Byte Size
- Content Hash
- Created Time
- Version

`Image ID` identifies the domain record.

`Asset ID` identifies the binary object managed by Storage.

The two identifiers must not be treated as interchangeable.

---

# Image Roles

Recommended image roles:

| Role | Description |
|---|---|
| `source` | Original image obtained from capture, import or upload |
| `normalized` | Orientation, color mode or metadata normalized |
| `ocr_input` | Image prepared specifically for OCR |
| `region_crop` | Cropped region derived from another image |
| `clean_background` | Text-removed or inpainted background |
| `render_preview` | Temporary translated preview |
| `render_output` | Final rendered translation |
| `thumbnail` | Reduced-size navigation image |

Roles should be represented by stable domain values rather than storage paths or filename conventions.

---

# Source Types

An image may originate from:

- Browser capture
- Screen capture
- Remote URL
- Local file import
- Clipboard
- Camera or scanner
- Archive extraction
- PDF page rasterization
- Generated processing output

Source information is descriptive metadata. It must not make the domain dependent on a specific capture adapter.

---

# Image Metadata

Image metadata may include:

- MIME type
- File extension
- Pixel width
- Pixel height
- Aspect ratio
- Color space
- Bit depth
- Alpha-channel presence
- Orientation
- Density or DPI
- Animation flag
- Frame count
- Compression type
- Byte size

Only metadata required by domain behavior should be promoted to first-class fields. Additional technical metadata may be stored as extensible attributes.

---

# Coordinate Space

OCR regions, text blocks and render layers rely on a stable coordinate system.

The canonical image coordinate space uses:

```text
origin: top-left
x-axis: left to right
y-axis: top to bottom
unit: source pixels
```

Coordinates are interpreted relative to the dimensions of the image version that produced them.

A coordinate-bearing artifact must reference:

- Image ID
- Image version
- Width
- Height
- Transform, when applicable

This prevents OCR or layout coordinates from being reused against an incompatible image.

---

# Orientation and Transform

An image may contain orientation metadata that differs from its decoded pixel orientation.

CRAI should normalize orientation before OCR and layout analysis.

Typical transforms include:

- Rotation
- Scaling
- Cropping
- Padding
- Perspective correction
- Deskewing

Every derived image must preserve enough transform information to map coordinates back to its parent image when required.

Example lineage:

```text
Source Image
    │ normalize orientation
    ▼
Normalized Image
    │ crop and upscale
    ▼
OCR Input Image
```

---

# Image Lineage

Derived images form a directed lineage.

Typical lineage fields:

- Parent Image ID
- Derivation Type
- Processing Profile ID
- Processing Configuration Version
- Input Content Hash
- Output Content Hash
- Created Time

Lineage supports:

- Reproducibility
- Cache validation
- Diagnostics
- Auditing
- Safe cleanup
- Coordinate mapping

A derived image must never overwrite its source image.

---

# Lifecycle

```text
Registered
    │
    ▼
Available
    │
    ├──► Derived
    │
    ├──► Referenced
    │
    └──► Invalidated
             │
             ▼
          Released
```

Lifecycle meaning:

- `Registered`: Metadata and asset reference have been created.
- `Available`: Binary data is readable and integrity checks pass.
- `Derived`: One or more processing outputs reference the image.
- `Referenced`: The image is actively used by Page artifacts.
- `Invalidated`: The asset or its metadata is no longer valid for processing.
- `Released`: Retention rules allow the underlying binary to be removed.

This lifecycle is separate from temporary runtime loading and memory disposal.

---

# Mutability

Original source images are immutable.

When the user rotates, crops, enhances or cleans an image, CRAI creates a new derived image rather than modifying the source binary.

Mutable information should be limited to:

- Availability status
- Retention state
- User-facing labels
- Non-semantic annotations

Changes that affect pixels, dimensions, coordinate space or processing behavior require a new image version or a new derived Image record.

---

# Integrity

An Image should contain a content hash computed from canonical binary content.

The hash may be used for:

- Duplicate detection
- Cache keys
- Corruption detection
- Idempotent import
- Derivation validation
- Storage reconciliation

Metadata alone must not be used to determine binary equality.

---

# Duplicate Handling

Multiple Pages may encounter identical binary images.

Storage may deduplicate binary assets by content hash, but domain ownership remains explicit:

```text
Page A ──► Image Record A ──┐
                            ├──► Shared Asset
Page B ──► Image Record B ──┘
```

This preserves Page-level lineage and lifecycle while allowing storage optimization.

A domain Image must not silently move from one Page to another because its binary content matches.

---

# Processing Compatibility

Before an image is accepted by OCR or rendering, compatibility checks may validate:

- Supported format
- Maximum dimensions
- Minimum dimensions
- Maximum byte size
- Valid orientation
- Decodable content
- Animation policy
- Alpha-channel policy
- Color-space support

Compatibility failure does not necessarily invalidate the Page. The system may derive a normalized image or select a fallback processing path.

---

# Error Conditions

Typical domain-level errors include:

- Image asset unavailable
- Unsupported image format
- Image metadata invalid
- Image content corrupted
- Image dimensions invalid
- Content hash mismatch
- Parent image missing
- Coordinate version mismatch
- Derivation lineage invalid
- Source image mutation attempted

Provider-specific decoding, network and filesystem errors should be translated into stable domain or module errors before crossing architecture boundaries.

---

# Events

Typical domain events:

- `ImageRegistered`
- `ImageAvailable`
- `ImageDerived`
- `ImageInvalidated`
- `ImageReplaced`
- `ImageReleased`
- `ImageIntegrityFailed`

Events should carry identifiers and metadata, not raw image bytes.

---

# Persistence

Persistent Image records should contain only domain metadata and asset references.

Recommended separation:

```text
Image Domain Record
├── identity
├── page reference
├── role
├── dimensions
├── content hash
├── lineage
└── asset reference

Storage Asset
└── binary image data
```

Temporary in-memory buffers are runtime resources and must not be represented as durable domain entities.

---

# Retention

Retention depends on image role.

Suggested policy:

- Source images: retain while the Page exists
- Normalized images: retain while referenced or reproducible cost is high
- OCR input images: cache according to OCR policy
- Region crops: retain only when required for diagnostics or review
- Render previews: short-lived and replaceable
- Final outputs: retain according to export policy
- Thumbnails: cacheable and regenerable

The Page aggregate determines semantic references. Storage determines physical deletion according to retention and reference-count rules.

---

# Observability

Useful image diagnostics include:

- Image ID
- Page ID
- Role
- Dimensions
- Byte size
- Content hash prefix
- Parent Image ID
- Derivation type
- Processing duration
- Decoder or encoder used
- Validation result

Raw image content and sensitive source URLs should not be included in ordinary logs.

---

# Architecture Invariants

1. Every Image domain record is associated with exactly one Page.
2. Original source image content is immutable.
3. Pixel-changing operations produce a new image record or version.
4. Every binary image is referenced through a Storage-managed Asset ID.
5. Coordinate-bearing artifacts reference the exact image version that produced them.
6. Derived images preserve parent lineage and transformation metadata.
7. Image equality is determined by canonical content hash, not filename or URL.
8. Deleting a derived image must not delete an authoritative source image.
9. Domain events never contain raw image bytes.
10. Storage implementation details do not leak into the domain model.
11. Page processing may fail or retry without mutating the original image.
12. Exported image outputs are immutable once published.

---

# Related Documents

- README.md
- PROJECT.md
- BOOK.md
- CHAPTER.md
- PAGE.md
- TEXT_BLOCK.md
- TRANSLATION.md
- `../storage/CONTRACT.md`
- `../storage/BACKENDS.md`
- `../runtime/RESOURCE_LIFECYCLE.md`
- `../runtime/CACHE_POLICY.md`
