# Architecture Flows

This directory contains end-to-end flows for CRAI.

A flow describes what happens across multiple modules from the beginning of a user action until the system produces a visible result.

Flow documents focus on behavior rather than implementation.

---

## Flow Responsibilities

A flow may define:

- actor interactions
- preconditions
- processing sequence
- state transitions
- emitted events
- data movement
- cancellation points
- error recovery
- user-visible outcomes

A flow must not define:

- thread implementation
- queue data structures
- specific libraries
- provider SDK details
- database schemas

Those concerns belong to runtime or feature-specific documents.

---

## Current Flows

### `SCREEN_COMIC_FLOW.md`

Defines the primary MVP flow:

```text
Select Region
    ↓
Observe Screen
    ↓
Detect Stable Content
    ↓
OCR
    ↓
Layout Analysis
    ↓
Translation
    ↓
Presentation
    ↓
Continue Observing
```

---

## Planned Flows

Future flow documents may include:

- `STRUCTURED_TEXT_FLOW.md`
- `MANUAL_IMAGE_FLOW.md`
- `REGION_SELECTION_FLOW.md`
- `OCR_CORRECTION_FLOW.md`
- `TRANSLATION_CORRECTION_FLOW.md`
- `SESSION_RECOVERY_FLOW.md`

A new flow should only be created when it represents a distinct end-to-end scenario.

---

## Relationship With Runtime

Flow documents answer:

> What happens?

Runtime documents answer:

> How is that work executed safely and efficiently?

For example:

- `SCREEN_COMIC_FLOW.md` defines that OCR is canceled when content changes.
- `runtime/CANCELLATION.md` defines the mechanism used to perform that cancellation.

---

## Related Documents

- `../DATA_FLOW.md`
- `../STATE_MACHINE.md`
- `../EVENT_BUS.md`
- `../runtime/README.md`