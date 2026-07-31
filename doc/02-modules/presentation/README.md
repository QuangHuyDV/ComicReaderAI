# Presentation Module

> Transforms translated reading content into immutable presentation models that can be rendered consistently across different platforms.

---

# Overview

The Presentation Module converts translated reading content into a platform-independent presentation model.

Presentation owns:

- presentation business logic
- layout computation
- geometry processing
- presentation strategy selection
- presentation state
- presentation revisions

Presentation does **not** render UI.

Instead, it produces immutable PresentationSnapshots and RenderPlans that are consumed by UI Adapters.

---

# Architecture Position

```text
                    Reading Session
                           │
                           ▼
                     Presentation
                           │
                PresentationSnapshot
                           │
                      RenderPlan
                           │
                           ▼
                      UI Adapter
                           │
                           ▼
         Desktop / Browser / Mobile / Future Clients
```

Presentation acts as the boundary between business logic and rendering.

UI Adapters remain responsible for platform-specific rendering.

---

# Responsibilities

Presentation is responsible for:

- validating presentation requests
- building PresentationSnapshots
- maintaining Presentation state
- creating RenderPlans
- computing layouts
- processing geometry
- applying PresentationProfiles
- selecting PresentationStrategies
- reacting to viewport changes
- publishing Presentation events

Presentation is **not** responsible for:

- OCR
- Translation
- Screen Capture
- Browser DOM
- Image Rendering
- Window Management
- Storage

---

# Core Concepts

## PresentationSnapshot

Immutable representation of the current presentation.

A PresentationSnapshot contains everything required for rendering but contains no rendering logic itself.

---

## RenderPlan

A RenderPlan describes **how** a PresentationSnapshot should be rendered.

It is consumed by UI Adapters.

Presentation owns its construction.

UI owns its execution.

---

## PresentationItem

A single logical presentation unit.

Examples include:

- translated speech bubble
- marker
- side panel entry
- text block

---

## PresentationStrategy

Defines how translated content is presented.

Examples:

- Overlay
- Marker Overlay
- Side Panel
- Reader
- Hybrid

---

## PresentationProfile

Defines user presentation preferences such as:

- typography
- spacing
- visual density
- accessibility

---

## PresentationMode

Defines the active presentation behavior for the current reading session.

---

# Public Interface

Presentation exposes the following commands.

## Commands

- BuildPresentation
- UpdatePresentationContent
- RecomputePresentationLayout
- UpdatePresentationFocus
- ApplyPresentationProfile
- ChangePresentationMode
- ClearPresentation

See:

```
CONTRACT.md
```

---

# Events

Presentation consumes events from:

- Reading Session
- Translation
- Preferences
- UI Adapter

Presentation publishes:

- PresentationPrepared
- PresentationUpdated
- PresentationLayoutChanged
- PresentationModeChanged
- PresentationRejected
- PresentationFailed
- PresentationCleared

See:

```
EVENTS.md
```

---

# Lifecycle

Presentation follows the lifecycle defined in:

```
STATES.md
```

Typical flow:

```text
Empty

↓

Preparing

↓

Ready

↓

Updating

↓

Ready

↓

Reflowing

↓

Ready

↓

Clearing

↓

Empty
```

Unexpected invariant failures transition Presentation into:

```text
Failed
```

---

# Error Model

Presentation exposes stable architecture-level error contracts.

Consumers must never rely on implementation-specific exceptions.

Errors define:

- ErrorCode
- Severity
- RetryPolicy
- Recovery behavior

See:

```
ERRORS.md
```

---

# Revision Ownership

Presentation recognizes revisions owned by multiple modules.

| Revision | Owner |
|----------|-------|
| ContentRevision | Reading Session |
| TranslationRevision | Translation |
| PreferenceRevision | Preferences |
| ProfileRevision | Preferences |
| ViewportRevision | UI Adapter |
| PresentationRevision | Presentation |

Presentation only owns PresentationRevision.

---

# Design Principles

Presentation follows these principles:

- Platform Independent
- Immutable Output
- Deterministic
- Event Driven
- Revision Safe
- Geometry Aware
- Atomic Commit
- Idempotent
- UI Independent

---

# Module Boundaries

Presentation MUST:

- produce PresentationSnapshots
- produce RenderPlans
- validate presentation requests
- preserve revision ordering
- publish Presentation events

Presentation MUST NOT:

- perform OCR
- translate content
- render UI
- manipulate browser DOM
- access platform graphics APIs
- store persistent user data

---

# Directory Structure

```text
presentation/

├── README.md
├── MODULE.md
├── CONTRACT.md
├── EVENTS.md
├── STATES.md
└── ERRORS.md
```

---

# Reading Order

For new contributors:

1. README.md
2. MODULE.md
3. CONTRACT.md
4. EVENTS.md
5. STATES.md
6. ERRORS.md

For debugging:

1. ERRORS.md
2. STATES.md
3. EVENTS.md

For feature development:

1. MODULE.md
2. CONTRACT.md
3. EVENTS.md

---

# Related Documents

Architecture:

```text
docs/architecture/

CAPABILITY_MAP.md
STATE_MACHINE.md
EVENT_BUS.md
MODULE_DEPENDENCY.md
DATA_FLOW.md
```

Presentation:

```text
modules/presentation/

README.md
MODULE.md
CONTRACT.md
EVENTS.md
STATES.md
ERRORS.md
```

---

# Completion Checklist

Presentation is considered architecturally complete when:

- responsibilities are clearly defined
- public contracts are stable
- commands are documented
- events are documented
- state transitions are deterministic
- error contracts are standardized
- revision ownership is explicit
- rendering remains outside Presentation
- business logic remains platform independent

---

# Summary

Presentation is a pure application module that transforms translated reading content into immutable, platform-independent presentation models.

It owns presentation business logic while delegating all rendering responsibilities to UI Adapters.

This separation allows multiple frontends to share the same presentation behavior without changing business logic.