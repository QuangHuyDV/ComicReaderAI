# Runtime Processing Pipeline

**Status:** Draft

**Version:** 1.0

---

# 1. Purpose

This document defines the end-to-end processing pipeline executed by CRAI at runtime.

The pipeline transforms screen pixels into translated content presented to the user.

The pipeline is independent of specific OCR engines, translation providers, or UI implementations.

---

# 2. Design Goals

The runtime pipeline is designed to be:

- Continuous
- Event-driven
- Revision-aware
- Cancelable
- Low latency
- Modular
- Observable

---

# 3. High-Level Pipeline

```
Capture

↓

Observe

↓

Stable Frame

↓

Create Revision

↓

OCR

↓

Layout Analysis

↓

Reading Order

↓

Translation Units

↓

Translation

↓

Presentation Model

↓

Render

↓

Observe Again
```

The pipeline repeats continuously until the reading session ends.

---

# 4. Processing Stages

## Stage 1 — Capture

Responsible for obtaining the latest screen image.

Input:

Selected screen region.

Output:

Captured frame.

---

## Stage 2 — Observation

Determines whether the captured frame represents meaningful changes.

Possible outcomes:

- Ignore
- Continue observing
- Stable frame detected

---

## Stage 3 — Revision Creation

Every stable frame becomes an immutable revision.

Each revision has:

- Revision ID
- Timestamp
- Checksum
- Source metadata

No processing stage modifies an existing revision.

---

## Stage 4 — OCR

Extracts raw textual information from the revision.

Output:

OCR blocks.

---

## Stage 5 — Layout Analysis

Groups OCR blocks into semantic regions such as:

- Speech bubbles
- Narration
- Captions
- Sound effects

---

## Stage 6 — Reading Order

Resolves the natural reading sequence.

The output is an ordered list of readable segments.

---

## Stage 7 — Translation Unit Construction

Creates context-aware translation units.

Each unit contains:

- source text
- neighboring context
- glossary references
- metadata

---

## Stage 8 — Translation

Translates every translation unit.

Possible execution sources:

- Cache
- Local AI
- Remote AI

---

## Stage 9 — Presentation Model

Transforms translated content into a UI-friendly structure.

The presentation model is immutable.

---

## Stage 10 — Rendering

The UI renders the presentation model atomically.

Users never see partially processed pages.

---

# 5. Pipeline Characteristics

The runtime pipeline is:

Continuous

Every new revision starts a new pipeline.

Independent

Each revision owns its own processing state.

Cancelable

Every stage can be interrupted.

Immutable

Stages never modify previous outputs.

Observable

Every stage emits runtime events.

---

# 6. Pipeline Lifecycle

```
Revision Created

↓

Pipeline Created

↓

Stages Execute

↓

Presentation Generated

↓

Pipeline Completed

↓

Disposed
```

Pipelines never survive after their owning revision becomes obsolete.

---

# 7. Concurrent Pipelines

Multiple pipelines may exist simultaneously.

Example:

```
Revision 20

↓

OCR
```

```
Revision 21

↓

Capture
```

```
Revision 22

↓

Waiting
```

However,

only the newest valid revision is allowed to update the UI.

---

# 8. Cancellation

Pipelines are canceled when:

- newer revision arrives
- reading session ends
- user changes capture region
- provider becomes unavailable
- unrecoverable runtime error occurs

Canceled pipelines release resources immediately.

---

# 9. Error Handling

Each stage reports:

- success
- retryable failure
- fatal failure
- canceled

Failures do not terminate the runtime.

The scheduler determines recovery behavior.

---

# 10. Performance Objectives

Target latency:

| Stage | Target |
|--------|---------|
| Capture | <30 ms |
| Observation | <100 ms |
| OCR | <300 ms |
| Layout | <50 ms |
| Translation | <700 ms |
| Presentation | <50 ms |
| Rendering | <30 ms |

Target end-to-end latency:

≤ 1 second.

---

# 11. Related Runtime Documents

This document provides only the high-level pipeline.

Detailed behavior is defined in:

- WORK_QUEUE.md
- SCHEDULER.md
- CANCELLATION.md
- CACHE_POLICY.md
- MEMORY_MODEL.md
- THREADING_MODEL.md
- PERFORMANCE_MODEL.md
- RESOURCE_LIFECYCLE.md

---

# 12. Summary

The runtime pipeline defines the canonical execution flow of CRAI.

Every screen update becomes an immutable revision processed through a deterministic, observable, and cancelable sequence of stages before being presented to the user.