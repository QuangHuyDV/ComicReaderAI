# CRAI Module Map

> Project: CRAI  
> Version: 0.2  
> Status: Architecture Draft

---

## 1. Purpose

This document defines the logical module boundaries of CRAI.

It describes:

- module responsibilities
- module ownership
- module boundaries
- shared dependencies
- currently proposed modules
- unresolved module decisions

This document focuses on architecture responsibilities rather than implementation details.

All module definitions must follow:

- `.meta/AI_BOOT.md`
- `.meta/PROJECT_RULE.md`
- `.meta/MODULE_RULE.md`
- `MODULE_DEPENDENCY.md`
- `CAPABILITY_MAP.md`

---

## 2. Current Design Status

Current project phase:

```text
Architecture Exploration
```

Current architectural assumptions:

- Structured text and image-based content use different acquisition and extraction workflows.
- OCR is only required for image-based content.
- Translation is independent from presentation.
- OCR and translation providers must be replaceable.
- Screen comic translation is the initial MVP.
- Shared capabilities must not be duplicated across feature modules.
- Runtime coordination is separated from domain processing.

Nothing in this document is final unless explicitly marked as `Stable`.

---

## 3. Module Design Principles

A module must:

- own one coherent responsibility
- expose an explicit public interface
- hide implementation details
- avoid circular dependencies
- communicate through contracts or events
- avoid owning unrelated global state
- remain replaceable where provider behavior may vary

A capability does not automatically become a module.

A new module should only be created when it has:

- clear ownership
- a stable responsibility
- meaningful internal cohesion
- an explicit boundary from other modules

---

## 4. Product Flow Boundaries

CRAI currently supports two major product flow families.

### 4.1 Structured Text Flow

```text
Text Source
    ↓
Text Acquisition
    ↓
Text Processing
    ↓
Translation
    ↓
Reading Presentation
```

Examples:

- web novels
- HTML
- TXT
- EPUB

This flow does not require OCR when the source text is already structured.

### 4.2 Image Content Flow

```text
Image Source
    ↓
Image Acquisition
    ↓
Image Processing
    ↓
OCR
    ↓
Layout Understanding
    ↓
Translation
    ↓
Comic Presentation
```

Examples:

- manga
- manhua
- manhwa
- screenshots
- scanned documents
- image-based PDFs

### 4.3 Shared Responsibilities

Both flow families may share:

- session management
- translation
- glossary
- translation memory
- cache
- settings
- provider management
- diagnostics
- storage
- presentation infrastructure

---

## 5. Proposed Core Modules

| Module | Primary responsibility | Status |
|---|---|---|
| Source | Acquire content from screen, browser, file, image, or clipboard | Proposed |
| Observation | Detect meaningful source changes and stable content | Proposed |
| Image Processing | Prepare image content before OCR | Proposed |
| Text Processing | Normalize and reconstruct structured text | Proposed |
| OCR | Extract text and text geometry from images | Proposed |
| Layout | Determine semantic regions and reading order | Proposed |
| Translation | Convert source-language units into target-language units | Proposed |
| Knowledge | Manage glossary, names, context, and translation consistency | Proposed |
| Presentation | Build UI-ready presentation models | Proposed |
| Reader | Coordinate user-facing reading behavior | Proposed |
| Session | Manage active reading-session lifecycle | Proposed |
| Runtime | Coordinate pipeline execution, scheduling, and cancellation | Proposed |
| Cache | Store reusable OCR, translation, and derived results | Proposed |
| Storage | Persist user-approved durable data | Proposed |
| Settings | Manage user preferences and feature configuration | Proposed |
| Provider | Adapt external or local OCR and translation providers | Proposed |
| Diagnostics | Collect logs, metrics, traces, and failure information | Proposed |

---

## 6. Module Groups

For documentation and implementation planning, modules may be grouped into the following areas.

### 6.1 Acquisition

- Source
- Observation

### 6.2 Content Understanding

- Image Processing
- Text Processing
- OCR
- Layout

### 6.3 Language Processing

- Translation
- Knowledge

### 6.4 User Experience

- Reader
- Presentation
- Session
- Settings

### 6.5 Runtime Infrastructure

- Runtime
- Cache
- Storage
- Provider
- Diagnostics

These groups organize related modules but do not create additional runtime layers.

---

## 7. Shared Module Rules

Shared modules must not depend on product-specific UI behavior.

For example:

- Translation must not depend on comic overlay UI.
- OCR must not depend on the reader panel.
- Cache must not decide presentation behavior.
- Provider adapters must not own session state.
- Runtime scheduling must not perform domain translation logic.

Feature-specific modules may depend on shared modules through explicit contracts.

---

## 8. Current Module Mapping

The current documentation directories approximately map to module areas as follows.

| Documentation directory | Module area |
|---|---|
| `02-reader` | Reader and Session |
| `03-ocr` | OCR and image text extraction |
| `04-layout` | Layout analysis and reading order |
| `05-translation` | Translation |
| `06-ai` | AI orchestration and model-assisted processing |
| `07-database` | Storage |
| `08-cache` | Cache |
| `09-ui` | Presentation and UI |
| `10-plugin` | Plugin and provider extensibility |
| `18-api` | Public and internal contracts |
| `19-security` | Privacy and security boundaries |
| `20-performance` | Measurements and performance verification |

Directory boundaries are documentation boundaries and do not automatically define code packages.

---

## 9. Modules Not Yet Confirmed

The following areas must not become independent modules until their responsibilities are better understood:

- Reading History
- Download Manager
- Browser Extension
- Local Library
- AI Assistant
- OCR Correction
- Translation Memory
- Glossary
- Offline Model Package
- Cross-device Synchronization

Some of these may become:

- capabilities inside an existing module
- submodules
- provider implementations
- product features
- independent modules

The decision must be based on ownership and dependency boundaries.

---

## 10. Open Questions

### Reader

- Is Reader a coordinator or a full domain module?
- Does the browser reader share the same session model as screen capture?
- Should manual image import use the same reader lifecycle?

### OCR and Layout

- Does text-region detection belong to OCR or Layout?
- Which module owns reading-order correction?
- Which module owns user-edited OCR text?

### Translation and Knowledge

- Does Translation Memory belong to Translation, Knowledge, or Storage?
- Does glossary resolution happen before or during translation?
- Which module owns character-name consistency?

### Runtime

- Is Runtime a single application service or a collection of infrastructure components?
- Which module creates processing revisions?
- Which module owns pipeline cancellation?

### Storage

- Which data is transient?
- Which data requires explicit user consent before persistence?
- Should translated chapters be stored by default?

---

## 11. Module Creation Checklist

Before introducing a module, verify:

- Does it own a clear responsibility?
- Is that responsibility different from existing modules?
- Does it require an independent lifecycle?
- Does it expose a meaningful contract?
- Can dependencies remain one-directional?
- Is it more than an implementation detail?
- Can it be tested independently?
- Does it avoid duplicating an existing capability?

If most answers are no, the area should remain part of an existing module.

---

## 12. Related Documents

- `CAPABILITY_MAP.md`
- `MODULE_DEPENDENCY.md`
- `DATA_FLOW.md`
- `STATE_MACHINE.md`
- `EVENT_BUS.md`
- `flows/SCREEN_COMIC_FLOW.md`
- `runtime/PIPELINE_RUNTIME.md`

---

## 13. Next Step

Module boundaries will be refined after the primary runtime flow has been validated.

The current sequence is:

```text
Screen Comic Flow
    ↓
Runtime Pipeline
    ↓
Queue and Scheduling
    ↓
Cancellation and Resource Lifecycle
    ↓
Concrete Module Contracts
```

Detailed module implementation must not begin until runtime ownership and dependency directions are clear.