# SCREEN_COMIC_FLOW

**Status:** Draft

**Version:** 1.0

---

# 1. Purpose

This document describes the complete runtime flow of the comic reading experience in CRAI.

The goal is to define:

- user interactions
- internal processing pipeline
- state transitions
- emitted events
- cancellation rules
- recovery behavior
- expected performance

This document intentionally focuses on **runtime behavior**, not implementation details.

---

# 2. Scope

This flow covers the MVP scenario:

- Desktop Application
- Screen Region Capture
- Comic Image Translation
- Side-by-Side Reading
- Automatic Continuous Translation

Not included:

- Browser extension
- Novel mode
- Batch translation
- OCR model implementation
- Translation provider implementation

---

# 3. Actors

## Primary Actor

User

## Internal Components

Capture Engine

Frame Observer

Revision Manager

OCR Engine

Layout Analyzer

Reading Order Resolver

Translation Engine

Presentation Engine

Session Manager

Cache Manager

Event Bus

---

# 4. Preconditions

Before this flow starts:

- CRAI is running
- Translation provider is available
- OCR provider is available
- User has configured source language
- User selects a screen region

---

# 5. High-Level Flow

```text
Launch CRAI
    ↓
Select Screen Region
    ↓
Reading Session Created
    ↓
Capture Starts
    ↓
Observe Screen Changes
    ↓
Stable Frame Detected
    ↓
Create Source Revision
    ↓
OCR
    ↓
Layout Analysis
    ↓
Reading Order
    ↓
Build Translation Units
    ↓
Translate
    ↓
Build Presentation Model
    ↓
Update Reader UI
    ↓
Continue Monitoring
```

---

# 6. Reading Session

A reading session represents one continuous reading activity.

A session owns:

- selected region
- current revision
- processing state
- cache references
- user preferences

The session remains active until:

- user closes CRAI
- user stops translation
- capture region changes

---

# 7. Capture Phase

State:

```
OBSERVING
```

Capture Engine continuously watches the selected screen region.

Every captured frame receives:

- timestamp
- frame id
- checksum
- image buffer

Frames are **not** translated immediately.

Instead, they are evaluated for stability.

---

# 8. Stability Detection

Most comic readers scroll.

During scrolling:

- OCR must not start
- Translation must not start

The Frame Observer waits until:

- image becomes stable
- no significant movement
- no scrolling detected

Example:

```text
Frame A

↓

Frame B

↓

Frame C

↓

Movement

↓

Ignore

↓

Frame D

↓

Frame E

↓

No changes

↓

Stable
```

Once stable:

```
frame.stable
```

is emitted.

---

# 9. Source Revision

Every stable frame becomes a new immutable revision.

Example:

```
Revision 18
```

contains

- screenshot
- checksum
- capture time
- region metadata

Previous revisions are never modified.

---

# 10. OCR Phase

State:

```
OCR_RUNNING
```

Input:

Source Revision

Output:

OCR Result

Each detected text block contains:

- bounding box
- confidence
- language
- raw text

No translation occurs yet.

---

# 11. Layout Analysis

OCR blocks are converted into logical comic regions.

Example:

Speech Bubble

Caption

Sound Effect

Narration

Unknown

Each region receives its own identifier.

---

# 12. Reading Order

Comic reading order is reconstructed.

Instead of OCR order:

```
1
3
2
```

The resolver produces:

```
1
2
3
```

The output becomes ordered segments.

---

# 13. Translation Unit Construction

Each readable segment becomes one Translation Unit.

A Translation Unit contains:

- source text
- context
- neighboring bubbles
- page position
- glossary references

Translation never operates directly on OCR blocks.

---

# 14. Translation

State

```
TRANSLATING
```

Translation Engine processes Translation Units.

Possible sources:

- local model
- cloud model
- cached result

Each unit returns:

- translated text
- confidence
- provider metadata

---

# 15. Presentation Model

Translation output is converted into a Presentation Model.

Presentation Model is optimized for UI rendering.

It contains:

- ordered paragraphs
- original text
- translated text
- references
- interaction metadata

---

# 16. UI Update

State

```
PRESENTING
```

Reader UI updates atomically.

The user should never see:

- half translated page
- partially rendered layout
- mixed revisions

Only complete Presentation Models are rendered.

---

# 17. Continuous Monitoring

After rendering:

State returns to

```
OBSERVING
```

Capture continues.

The cycle repeats automatically.

---

# 18. Cancellation

If user scrolls while OCR is running:

```
Revision 18

↓

OCR Running

↓

User Scrolls

↓

Revision 19 Created

↓

Cancel OCR 18

↓

Start OCR 19
```

Late OCR results are discarded.

---

# 19. Stale Result Protection

Every processing task carries:

- Session ID
- Revision ID

Before committing results:

```
Current Revision == Result Revision ?
```

If false:

Discard immediately.

Late work must never overwrite newer translations.

---

# 20. Cache Usage

Cache lookup happens before translation.

Possible cache levels:

- OCR Cache
- Translation Cache
- Layout Cache

Cache validity depends on Revision checksum.

---

# 21. Error Recovery

Possible failures:

OCR timeout

↓

Retry

Translation timeout

↓

Retry

Provider unavailable

↓

Fallback Provider

Capture failure

↓

Restart Capture

Every recovery emits events.

---

# 22. Runtime State Flow

```text
IDLE
    ↓
SESSION_CREATED
    ↓
OBSERVING
    ↓
WAIT_STABLE
    ↓
OCR_RUNNING
    ↓
LAYOUT_ANALYSIS
    ↓
BUILD_TRANSLATION_UNITS
    ↓
TRANSLATING
    ↓
BUILD_PRESENTATION
    ↓
PRESENTING
    ↓
OBSERVING
```

---

# 23. Event Timeline

Typical runtime:

```text
session.created

↓

capture.started

↓

frame.changed

↓

frame.stable

↓

revision.created

↓

ocr.started

↓

ocr.completed

↓

layout.completed

↓

translation.started

↓

translation.completed

↓

presentation.updated
```

---

# 24. Performance Targets (MVP)

| Stage | Target |
|--------|---------|
| Capture | <30 ms |
| Stability Detection | <100 ms |
| OCR | <300 ms |
| Layout Analysis | <50 ms |
| Translation | <700 ms |
| Presentation Build | <50 ms |
| UI Update | <30 ms |

Target end-to-end latency:

```
≤ 1 second
```

---

# 25. Design Principles

The comic reading pipeline must satisfy the following principles:

- Immutable revisions
- Event-driven communication
- Stateless processing workers
- Atomic presentation updates
- Cancellation-first design
- Revision-aware processing
- Cache before computation
- User interaction always has highest priority

---

# 26. Related Documents

- ../STATE_MACHINE.md
- ../EVENT_BUS.md
- ../DATA_FLOW.md
- ../MODULE_DEPENDENCY.md
- ../CAPABILITY_MAP.md