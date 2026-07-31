# Text Architecture

## Purpose

The Text Architecture converts extracted text into structured,
translation-ready content.

It is responsible for:

- Text normalization
- Structural reconstruction
- Reading order
- Language analysis
- Segmentation

It is not responsible for OCR recognition, translation or presentation.

---

# Position in the Pipeline

```text
OCR Reading Order
        │
        ▼
Text Model
        │
        ▼
Segmentation
        │
        ▼
Translation Context
        │
        ▼
Translation
        │
        ▼
Presentation
```

The Text layer acts as the bridge between content extraction and translation.

---

# Responsibilities

The Text Architecture provides:

- Canonical text representation
- Stable source mapping
- Paragraph and dialogue preservation
- Reading order reconstruction
- Language-aware normalization
- Translation-ready segments

---

# Document Structure

| Document | Responsibility |
|----------|----------------|
| TEXT_MODEL.md | Canonical representation of text |
| SEGMENTATION.md | Build translation units |
| (future) NORMALIZATION.md | Unicode and cleanup rules |
| (future) LANGUAGE.md | Language detection |
| (future) VALIDATION.md | Validation rules |

---

# Interaction

```text
OCR
   │
   ▼
Text
   │
   ▼
Translation
   │
   ▼
Presentation
```

Text never performs translation.

Translation never reconstructs document structure.

Presentation never modifies text.

---

# Design Principles

- Deterministic
- Source-preserving
- Provider-independent
- Language-aware
- Incremental
- Traceable

---

# Related Documents

- ../ocr/READING_ORDER.md
- TEXT_MODEL.md
- SEGMENTATION.md
- ../translation/CONTEXT.md
- ../translation/TRANSLATION.md