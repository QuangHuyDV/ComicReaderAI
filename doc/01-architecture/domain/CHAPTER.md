# Chapter Domain

- **Document:** Domain / Chapter
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

A Chapter is the primary processing aggregate in CRAI.

It represents one logical reading unit and owns every page, OCR result, text block, translation and rendering artifact produced during the translation workflow.

Most AI, OCR and rendering operations execute at the Chapter boundary.

---

# Responsibilities

A Chapter is responsible for:

- Managing page order
- Coordinating OCR processing
- Managing text blocks
- Managing translations
- Tracking processing status
- Producing rendered output
- Maintaining chapter statistics

---

# Aggregate Structure

```text
Chapter
├── Pages
│   ├── Images
│   ├── Text Blocks
│   ├── OCR Results
│   └── Render Layers
├── Translation Sessions
├── Statistics
└── Settings
```

---

# Identity

Every Chapter has:

- Chapter ID
- Book ID
- Chapter Number
- Title
- Reading Order
- Source URL
- Created Time
- Updated Time
- Version

Chapter ID never changes.

---

# Lifecycle

```text
Created
   │
   ▼
Imported
   │
   ▼
OCR Ready
   │
   ▼
Translating
   │
   ▼
Reviewing
   │
   ▼
Completed
   │
   ▼
Archived
```

Each stage represents business progress rather than technical state.

---

# Processing Pipeline

Typical workflow:

```text
Import
   │
   ▼
Page Extraction
   │
   ▼
OCR
   │
   ▼
Text Analysis
   │
   ▼
Translation
   │
   ▼
Rendering
   │
   ▼
Review
```

A Chapter owns the complete processing history.

---

# Relationships

A Chapter belongs to:

- One Book

A Chapter owns:

- Pages
- OCR Results
- Text Blocks
- Translation Sessions
- Rendered Assets

No child entity may exist outside its Chapter.

---

# Progress

Derived metrics include:

- Page count
- OCR completion
- Translation completion
- Review completion
- Rendering completion
- Error count
- Last processing time

---

# Configuration

Chapter-level overrides may include:

- OCR engine
- Translation profile
- Rendering profile
- Language override
- Review policy

Overrides supplement Book and Project settings.

---

# Events

Typical domain events:

- ChapterImported
- OCRCompleted
- TranslationStarted
- TranslationCompleted
- RenderingCompleted
- ReviewCompleted
- ChapterArchived

---

# Invariants

1. Every Chapter belongs to exactly one Book.
2. Page order is immutable once published unless explicitly reordered.
3. Every Page has a unique index within the Chapter.
4. OCR, Translation and Rendering operate within the Chapter boundary.
5. Child entities cannot exist without a Chapter.
6. Progress is derived from child entities.
7. Processing history is preserved for auditing.

---

# Related Documents

- README.md
- PROJECT.md
- BOOK.md
- PAGE.md
- IMAGE.md
- TEXT_BLOCK.md
- TRANSLATION.md
- GLOSSARY.md
- CHARACTER.md
- SESSION.md
