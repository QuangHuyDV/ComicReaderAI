
# Page Domain

- **Document:** Domain / Page
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

A Page is the core processing unit of CRAI.

Every OCR operation, layout analysis, AI translation, rendering, review and export is performed at the Page level.

Pages are independently executable, cacheable, retryable and parallelizable while remaining logically owned by a Chapter.

---

# Responsibilities

A Page is responsible for:

- Managing the original image
- Running OCR
- Detecting text regions
- Managing text blocks
- Executing translation
- Producing rendered output
- Tracking processing state
- Recording diagnostics

---

# Aggregate Structure

```text
Page
├── Source Image
├── OCR Result
├── Layout
├── Text Blocks
├── Translation Results
├── Render Layers
├── Diagnostics
└── Metadata
```

The Page is the aggregate root for all page-processing entities.

---

# Identity

Every Page contains:

- Page ID
- Chapter ID
- Page Index
- Source URI
- Width
- Height
- Created Time
- Updated Time
- Version

The Page ID is immutable.

---

# Processing Lifecycle

```text
Imported
   │
   ▼
Preprocessed
   │
   ▼
OCR Completed
   │
   ▼
Layout Analyzed
   │
   ▼
Translated
   │
   ▼
Rendered
   │
   ▼
Reviewed
   │
   ▼
Exported
```

Each transition is independently retryable.

---

# Processing Pipeline

```text
Source Image
      │
      ▼
Image Preprocessing
      │
      ▼
OCR
      │
      ▼
Layout Analysis
      │
      ▼
Text Block Generation
      │
      ▼
AI Translation
      │
      ▼
Post Processing
      │
      ▼
Rendering
```

---

# Relationships

A Page belongs to:

- One Chapter

A Page owns:

- Source Image
- OCR Result
- Layout
- Text Blocks
- Translation Results
- Render Layers
- Diagnostics

No processing artifact exists outside a Page.

---

# Parallel Execution

Pages may execute independently.

Supported operations:

- OCR
- Translation
- Rendering
- Validation
- Export

Failures on one Page must not block other Pages.

---

# Cache Boundary

A Page is the primary cache boundary.

Cacheable artifacts include:

- OCR results
- Layout
- AI responses
- Render layers
- Final output

---

# Progress

Derived metrics include:

- OCR progress
- Translation progress
- Rendering progress
- Review status
- Processing duration
- Retry count

---

# Events

Typical domain events:

- PageImported
- OCRCompleted
- LayoutCompleted
- TranslationCompleted
- RenderingCompleted
- ReviewCompleted
- ExportCompleted

---

# Invariants

1. Every Page belongs to exactly one Chapter.
2. Page order is unique within a Chapter.
3. All processing artifacts belong to a single Page.
4. Processing is deterministic for identical inputs and configuration.
5. Page processing may execute independently.
6. Cached artifacts are version-aware.
7. Exported output is immutable.

---

# Related Documents

- README.md
- CHAPTER.md
- IMAGE.md
- TEXT_BLOCK.md
- TRANSLATION.md
- SESSION.md
