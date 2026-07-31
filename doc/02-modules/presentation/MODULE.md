# Presentation Module

> **Project:** CRAI  
> **Module:** `presentation`  
> **Path:** `doc/02-modules/presentation/`  
> **Version:** 0.2  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-07-30

---

## 1. Purpose

Presentation transforms accepted reading output into a stable, revisioned, provider-neutral model that a UI Adapter can render.

Its primary responsibility is:

```text
Accepted session content
    +
accepted translation result
    +
resolved presentation preferences
    +
source and viewport geometry
    ↓
PresentationSnapshot
    +
RenderPlan
    ↓
UI Adapter
```

Presentation decides **what should be presented and how it should be arranged**.

Presentation does not directly render native windows, DOM nodes, framework widgets, or operating-system overlays.

---

## 2. Module Role

Presentation is the final business module in the active processing pipeline.

```text
Capture
    ↓
Recognition
    ↓
Text Processing
    ↓
Translation
    ↓
Reading Session accepts current result
    ↓
Presentation
    ↓
UI Adapter
    ↓
Application surface
```

Presentation must only build visible state from content that is still authoritative for the active Reading Session.

It must never assume that every late Recognition, Translation, or Runtime result is still safe to display.

---

## 3. Architectural Question

Presentation answers:

> How should accepted reading content be represented, arranged, and prepared for display?

It does not answer:

> How is that model rendered by a specific UI framework or operating system?

That second question belongs to `ui-adapter`.

---

## 4. Primary Goals

### 4.1 Minimal Reading Interruption

The user should continue reading without repeatedly copying text, moving between applications, or manually matching translations to source regions.

### 4.2 Clear Source Association

Every visible translation item must retain explicit traceability to its source block, source segment, translation segment, and source region where applicable.

### 4.3 Readability First

Vietnamese text may be substantially longer than Chinese or English source text.

Presentation must prefer readable fallback behavior over forcing text into unsuitable bounds.

### 4.4 Stable Incremental Updates

Partial results may arrive over time, but item identity, ordering, focus, and scroll position should remain stable whenever the semantic content has not changed.

### 4.5 Platform Independence

Presentation models must remain independent from:

- native window handles;
- browser DOM nodes;
- Flutter, Qt, Tauri, Electron, Wails, or other UI frameworks;
- operating-system APIs;
- OCR and translation provider implementations.

### 4.6 Replaceable Strategies

Side Panel, Text Reader, Overlay, and Hybrid modes share common contracts but remain independently replaceable.

---

## 5. Core Concepts

### 5.1 PresentationRequest

Represents one request to build or update presentation state.

```text
PresentationRequest
- requestId
- sessionId
- contentId
- contentRevision
- sourceDocumentRef
- translationResultRef
- requestedMode
- profileSnapshot
- viewportSnapshot
- targetDescriptor
- authorityContext
```

A request is immutable.

### 5.2 PresentationJob

Represents the logical lifecycle of preparing one presentation result.

```text
PresentationJob
- presentationJobId
- requestId
- sessionId
- contentRevision
- expectedPresentationRevision
- state
- startedAt
- completedAt
```

A layout retry or strategy fallback does not create a new semantic content revision.

### 5.3 PresentationSnapshot

Represents the authoritative presentation state for one content revision and one presentation revision.

```text
PresentationSnapshot
- presentationId
- sessionId
- contentId
- contentRevision
- presentationRevision
- effectiveMode
- profileVersion
- strategyVersion
- items[]
- markers[]
- renderPlanRef
- issues[]
- status
- createdAt
- updatedAt
```

Rules:

- one snapshot belongs to one Reading Session;
- one snapshot targets one accepted content revision;
- a newer presentation revision supersedes an older presentation revision;
- a snapshot must not combine unrelated content revisions;
- the same visible content should keep a stable `presentationId` during partial updates.

### 5.4 PresentationItem

Represents one user-visible source/translation unit.

```text
PresentationItem
- presentationItemId
- sourceBlockIds[]
- sourceSegmentIds[]
- translationSegmentIds[]
- regionIds[]
- sequence
- sourceText
- translatedText
- status
- confidence
- semanticRole
- layoutHints
- availableActions[]
- issues[]
```

A PresentationItem may represent:

- one comic speech region;
- multiple OCR lines grouped into one dialogue;
- one structured paragraph;
- one heading;
- one narration block;
- one source segment with multiple translation variants.

Array position is never the canonical mapping mechanism.

### 5.5 PresentationMarker

Represents a lightweight visual association between a source region and a PresentationItem.

```text
PresentationMarker
- markerId
- presentationItemId
- regionId
- label
- sourceGeometry
- projectedGeometry
- state
- visibility
- emphasis
```

Markers are presentation metadata. Native marker rendering belongs to UI Adapter.

### 5.6 PresentationProfile

An immutable snapshot of resolved presentation preferences.

```text
PresentationProfile
- profileId
- profileVersion
- preferredMode
- fontFamily
- fontSize
- lineHeight
- paragraphSpacing
- readerWidth
- panelWidth
- panelPlacement
- showSourceText
- showMarkers
- markerStyle
- overlayOpacity
- theme
- minimumReadableFontSize
- autoFallbackEnabled
- accessibilityOptions
```

Presentation consumes this snapshot but does not persist it.

### 5.7 PresentationMode

Supported conceptual modes:

```text
SIDE_PANEL
TEXT_READER
OVERLAY
HYBRID
```

Not every mode must be implemented in the MVP.

### 5.8 PresentationTarget

Describes the logical destination surface.

```text
PresentationTarget
- targetId
- targetKind
- capabilities[]
- bounds
- coordinateSpace
- scale
- safeInsets
```

Example target kinds:

```text
MAIN_WINDOW
COMPANION_PANEL
FLOATING_WINDOW
OVERLAY_SURFACE
BROWSER_SURFACE
```

A target is not a native window handle.

### 5.9 RenderPlan

Represents the framework-neutral arrangement that a UI Adapter applies.

```text
RenderPlan
- renderPlanId
- presentationId
- presentationRevision
- strategy
- targetId
- itemLayouts[]
- markerLayouts[]
- overflowItems[]
- hiddenItems[]
- focusState
- fallbackReason
- diagnostics
```

Presentation owns the RenderPlan.

UI Adapter owns the actual rendered widget, DOM element, or native surface.

### 5.10 PresentationRevision

Changes whenever visible presentation state changes without changing semantic source content.

Examples:

- translation progresses from partial to complete;
- a correction is accepted;
- font size changes;
- panel width changes;
- viewport geometry changes;
- focus or selection changes;
- strategy fallback changes the effective mode.

---

## 6. Owned Responsibilities

Presentation owns:

### 6.1 Mode Resolution

Resolve the effective mode from:

- requested mode;
- content kind;
- target capabilities;
- available geometry;
- profile snapshot;
- readability constraints;
- fallback policy.

### 6.2 Presentation Model Construction

Build stable PresentationItems from accepted SourceDocument and TranslationResult references.

### 6.3 Source-to-Translation Mapping

Preserve explicit relationships:

```text
SourceBlockId
    ↔
SourceSegmentId
    ↔
TranslationSegmentId
    ↔
PresentationItemId
    ↔
RegionId
```

### 6.4 Visual Ordering

Respect semantic reading order from upstream.

Presentation may apply visual grouping but must not silently redefine canonical reading order.

### 6.5 Presentation Strategy Selection

Select Side Panel, Text Reader, Overlay, or Hybrid strategy according to the effective mode.

### 6.6 Render Plan Construction

Create a deterministic RenderPlan from:

- PresentationItems;
- target capabilities;
- viewport snapshot;
- geometry transforms;
- typography profile;
- overflow policy;
- focus state.

### 6.7 Overflow Resolution

Resolve overflow through bounded fallback behavior.

```text
Wrap
    ↓
Expand available container
    ↓
Allow scrolling
    ↓
Collapse secondary content
    ↓
Use focus-only overlay
    ↓
Fallback to Side Panel
```

### 6.8 Marker Model Generation

Generate stable marker identities, ordering, labels, and association metadata.

### 6.9 Progressive Presentation

Support incremental states such as:

```text
WAITING
RECOGNIZED
TRANSLATING
PARTIALLY_TRANSLATED
TRANSLATED
CORRECTED
FAILED
SUPPRESSED
```

### 6.10 Presentation Authority Validation

Before publishing a new authoritative snapshot, verify:

- session is still active;
- content revision is still current;
- expected presentation revision is valid;
- input references belong to the same content lineage;
- late results cannot overwrite newer corrections or snapshots.

### 6.11 Presentation Diagnostics

Expose structured diagnostics without logging user content by default.

---

## 7. Responsibilities Not Owned

Presentation does not own:

### 7.1 Capture or Observation

It does not capture the screen, observe window changes, or detect stable frames.

### 7.2 OCR or Recognition

It does not detect text regions, recognize characters, infer reading order, or calculate OCR confidence.

### 7.3 Text Processing

It does not normalize OCR text, reconstruct lines, group semantic blocks, or create SourceDocument.

### 7.4 Translation

It does not call translation providers, build prompts, apply glossary injection, retry provider requests, or own TranslationResult.

### 7.5 Reading Session Lifecycle

It does not decide whether a session is active, paused, stopped, resumed, or superseded.

### 7.6 Preference Persistence

It consumes resolved PresentationProfile snapshots but does not persist user settings.

### 7.7 Native Rendering

It does not:

- create windows;
- render framework widgets;
- create DOM nodes;
- apply click-through;
- call DPI APIs;
- own native handles;
- attach overlays to operating-system windows.

These responsibilities belong to `ui-adapter` and platform implementations.

### 7.8 User Input Capture

It does not directly process mouse, keyboard, touch, global hotkey, or operating-system input events.

### 7.9 Persistent Storage

It does not persist snapshots, history, corrections, cache entries, or binary assets.

### 7.10 Image Modification

The MVP does not erase original text, inpaint images, replace speech-bubble text, or export modified images.

---

## 8. Boundary with UI Adapter

The central boundary is:

```text
Presentation
    ↓
PresentationSnapshot + RenderPlan
    ↓
UI Adapter
    ↓
Framework / Native Surface
```

Presentation owns:

- logical presentation identity;
- presentation revision;
- effective mode;
- PresentationItems;
- marker models;
- RenderPlan;
- overflow and fallback decisions;
- presentation warnings and diagnostics.

UI Adapter owns:

- framework component instances;
- native window handles;
- actual widget lifecycle;
- DOM or view-tree mutation;
- operating-system window operations;
- applying final geometry on the required UI thread;
- capture exclusion implementation;
- click-through implementation;
- platform event subscription.

Presentation must not import UI Adapter implementations.

UI Adapter may depend on Presentation contracts.

---

## 9. Boundary with Reading Session

Reading Session owns:

- active session identity;
- current source identity;
- current content identity;
- current content revision;
- session pause, resume, stop, and transition lifecycle;
- acceptance or rejection of processing results.

Presentation owns:

- presentation identity;
- presentation revision;
- effective presentation mode;
- visible logical model;
- RenderPlan;
- presentation-specific focus and selection state.

Recommended interaction:

```text
Translation result becomes available
    ↓
Reading Session validates authority
    ↓
Reading Session accepts current result
    ↓
BuildPresentation requested
    ↓
Presentation performs defensive validation
    ↓
PresentationSnapshot prepared
```

---

## 10. Boundary with Translation

Translation owns:

- TranslationJob;
- TranslationAttempt;
- TranslationBatch;
- TranslationResult;
- TranslationVariant;
- provider orchestration;
- glossary and context application;
- retry and fallback.

Presentation consumes stable TranslationResult references.

Presentation must not consume raw provider DTOs or provider streaming events.

Translation may publish partial or completed normalized results. Presentation decides how those states appear visually.

---

## 11. Boundary with Preferences

Preferences owns persistence and resolution of user settings.

Presentation consumes an immutable PresentationProfile snapshot.

A preference change may trigger a new presentation revision but must not change the semantic content revision.

---

## 12. Boundary with Diagnostics

Presentation produces structured diagnostic records such as:

```text
sessionId
contentId
contentRevision
presentationId
presentationRevision
effectiveMode
targetKind
itemCount
markerCount
overflowCount
hiddenItemCount
layoutDurationMs
fallbackReason
issueCode
```

Standard diagnostics must not contain:

- raw screenshots;
- full source text;
- full translated text;
- provider request bodies;
- credentials;
- private window titles.

---

## 13. Presentation Modes

### 13.1 Side Panel

Primary MVP strategy for screen-comic content.

Characteristics:

- ordered translation items;
- optional source text;
- stable region-to-item association;
- lightweight numbered markers;
- readable handling of long Vietnamese text;
- scrollable content;
- correction and retranslation action metadata;
- confidence and warning indicators.

Side Panel is preferred when overlay readability or geometry reliability is insufficient.

### 13.2 Text Reader

Used for structured text such as novels, imported text, clipboard text, and future document readers.

Characteristics:

- paragraph preservation;
- heading and dialogue roles;
- configurable reader width;
- typography profile;
- source/translation toggle;
- bilingual layout;
- stable navigation anchors.

### 13.3 Overlay

Displays translation near source regions without permanently modifying source content.

Overlay requires:

- explicit geometry lineage;
- reliable target capabilities;
- overlap detection;
- minimum readable font size;
- stale overlay removal;
- fallback behavior;
- capture-exclusion support from UI Adapter/platform.

Presentation creates overlay RenderPlans. UI Adapter applies them.

### 13.4 Hybrid

Combines markers, Side Panel, and focused temporary overlay.

Hybrid remains deferred until real reading tests show clear value.

---

## 14. Geometry Model

Geometry must declare its coordinate space.

Recommended spaces:

```text
SOURCE_IMAGE
CAPTURED_FRAME
NORMALIZED_SOURCE
APPLICATION_VIEWPORT
SCREEN
OVERLAY_SURFACE
BROWSER_VIEWPORT
```

A rectangle without coordinate-space metadata is invalid at a public boundary.

Presentation may project platform-neutral geometry through explicit transform metadata.

Native geometry acquisition and final platform conversion belong to adapters.

Presentation must reject or flag:

- non-finite coordinates;
- negative sizes;
- unsupported coordinate spaces;
- invalid transform chains;
- zero-sized source dimensions;
- stale transform versions;
- regions outside valid source bounds.

---

## 15. Layout Principles

### 15.1 Readability Before Exact Placement

Readable presentation takes priority over exact source-region containment.

### 15.2 Deterministic Ordering

The same semantic order, profile, viewport, and strategy version should produce the same visible ordering.

### 15.3 Stable Identity

Reflow must preserve `presentationItemId` and `markerId` unless semantic grouping changes.

### 15.4 Minimum Readable Font

Presentation must not silently reduce text below the configured threshold.

### 15.5 Vietnamese Expansion

Layout must not assume target text length is similar to source text length.

### 15.6 Bounded Fallback

Unsupported or unreadable modes must produce a documented fallback reason rather than silently changing behavior.

---

## 16. Partial Result Policy

Recommended flow:

```text
Accepted source structure available
    ↓
Create stable PresentationItems
    ↓
Reserve semantic order
    ↓
Show normalized progress state
    ↓
Update translated text by segment or accepted chunk
    ↓
Preserve focus and item identity
    ↓
Mark snapshot settled when all required items reach terminal state
```

Presentation should avoid:

- rebuilding the full list for every segment;
- changing sequence numbers during partial completion;
- unexpected scroll jumps;
- flashing empty states;
- replacing manual correction with a late automatic result;
- applying token-level updates when they cause unreadable instability.

Segment-level publication is the default architectural expectation.

Token-level streaming remains optional and strategy-dependent.

---

## 17. Focus, Selection, and Editing State

Presentation distinguishes:

```text
FOCUSED
SELECTED
ACTIVE_FOR_CORRECTION
```

Focus is temporary visual attention.

Selection is a persistent user choice.

Active-for-correction identifies the item currently being edited or retranslating.

These changes create a presentation revision but do not create a semantic content revision.

Presentation may expose available action metadata:

```text
FOCUS
SELECT
RETRANSLATE
EDIT_SOURCE
EDIT_TRANSLATION
ADD_GLOSSARY_ENTRY
COPY_SOURCE
COPY_TRANSLATION
REPORT_ISSUE
```

Execution of actions owned by other modules must be routed outside Presentation.

---

## 18. State Model

Detailed state transitions belong in `STATES.md`.

The high-level module lifecycle is:

```text
EMPTY
    ↓
PREPARING
    ↓
READY
    ↔
UPDATING
    ↔
REFLOWING
    ↓
CLEARING
    ↓
EMPTY
```

Additional outcomes:

```text
REJECTED
CANCELLED
SUPERSEDED
DEGRADED_READY
```

Error is not required to be a permanent terminal state.

A recoverable layout failure may produce `DEGRADED_READY` using a fallback mode.

---

## 19. Revision and Authority Rules

Presentation uses:

```text
contentRevision
presentationRevision
```

`contentRevision` changes when semantic source content changes.

`presentationRevision` changes when visible representation changes.

An asynchronous result must not be accepted when:

```text
result.contentRevision != current.contentRevision
```

or when:

```text
result.expectedPresentationRevision
    !=
current.presentationRevision
```

unless the command explicitly supports merge against a later revision.

Stopping or superseding a session revokes commit authority.

Late results become stale or superseded and must not replace current visible state.

---

## 20. Public Commands

Detailed schemas belong in `CONTRACT.md`.

Presentation conceptually supports:

```text
BuildPresentation
UpdatePresentationContent
RecomputePresentationLayout
ChangePresentationMode
UpdatePresentationFocus
ClearPresentation
```

Commands must use public serializable contracts and references rather than importing another module's internal objects.

---

## 21. Public Queries

Presentation may expose queries such as:

```text
GetCurrentPresentation
GetPresentationSnapshot
GetEffectivePresentationMode
GetPresentationIssues
GetPresentationCapabilities
```

Queries must not expose mutable internal state.

---

## 22. Published Events

Detailed events belong in `EVENTS.md`.

Conceptual events include:

```text
PRESENTATION_REQUESTED
PRESENTATION_STARTED
PRESENTATION_PREPARED
PRESENTATION_UPDATED
PRESENTATION_LAYOUT_CHANGED
PRESENTATION_MODE_FALLBACK_APPLIED
PRESENTATION_ISSUE_DETECTED
PRESENTATION_REJECTED
PRESENTATION_CANCELLATION_REQUESTED
PRESENTATION_CANCELLED
PRESENTATION_SUPERSEDED
PRESENTATION_CLEARED
```

Event payloads should contain metadata and references rather than full source or translated documents.

---

## 23. Consumed Events

Presentation may react to normalized and accepted events such as:

```text
SESSION_CONTENT_ACCEPTED
TRANSLATION_PARTIALLY_COMPLETED
TRANSLATION_COMPLETED
TRANSLATION_CORRECTED
SOURCE_GEOMETRY_CHANGED
PRESENTATION_PREFERENCE_CHANGED
SESSION_STOPPING
SESSION_STOPPED
```

The exact canonical names must follow `doc/01-architecture/core/EVENT_CONVENTION.md` and `EVENTS.md`.

Presentation must not consume raw provider events.

---

## 24. Error Model

Detailed error codes belong in `ERRORS.md`.

Presentation distinguishes:

- invalid input;
- unsupported mode;
- invalid geometry;
- stale or superseded result;
- target capability mismatch;
- layout failure;
- overflow degradation;
- UI Adapter rejection;
- cancellation;
- invariant violation.

Expected cancellation, stale work, and superseded work are not automatically user-visible failures.

---

## 25. Performance Model

Presentation should support measurement of:

- accepted-result-to-first-visible-model latency;
- full snapshot build duration;
- incremental update duration;
- reflow duration;
- marker generation duration;
- fallback frequency;
- coalesced or cancelled reflow requests;
- active snapshot memory;
- stale presentation completion count.

Rapid viewport events should be coalesced.

A single segment update should not rebuild unrelated PresentationItems when semantic grouping is unchanged.

Expensive preparation should not require execution entirely on the UI thread.

---

## 26. Accessibility

Presentation models should contain sufficient semantic data for accessible rendering:

- readable labels;
- deterministic focus order;
- source and translated text roles;
- status and issue message keys;
- action labels;
- marker-to-item relationship;
- non-color-only state indicators.

Actual platform accessibility APIs belong to UI Adapter.

---

## 27. Privacy

Presentation may temporarily hold:

- source text;
- translated text;
- source geometry;
- content identifiers;
- presentation preferences;
- accepted corrections.

It must not:

- persist data automatically;
- write full content to normal logs;
- retain stopped-session snapshots indefinitely;
- include credentials in diagnostics;
- capture additional screen content;
- expose content to unrelated modules.

Retention follows Reading Session, Storage, cache, and privacy policies.

---

## 28. Dependencies

Presentation may depend on stable contracts from:

```text
core contracts
reading-session
translation
text-processing
preferences
diagnostics
shared geometry and identifiers
```

Presentation must not depend directly on:

```text
capture implementation
recognition implementation details
OCR provider implementation
translation provider implementation
storage backend implementation
native UI toolkit
browser extension implementation
operating-system APIs
```

Dependency direction:

```text
ui-adapter
    ↓
presentation contracts

presentation
    ↓
stable business and core contracts
```

Presentation never imports UI Adapter implementation back into the module.

---

## 29. Conceptual Internal Components

These are logical responsibilities, not mandatory source folders:

```text
Presentation Module
├── Presentation Coordinator
├── Snapshot Builder
├── Item Mapper
├── Mode Resolver
├── Strategy Registry
├── Side Panel Strategy
├── Text Reader Strategy
├── Overlay Strategy
├── Render Plan Builder
├── Geometry Projector
├── Typography Policy
├── Overflow Policy
├── Progressive Update Policy
├── Authority Validator
└── Presentation Diagnostics
```

A future implementation may organize these differently as long as module boundaries remain intact.

---

## 30. Testing Strategy

Presentation must be testable without OCR providers, translation providers, native windows, or browser APIs.

### 30.1 Unit Tests

Test:

- mode resolution;
- strategy fallback;
- explicit mapping;
- stable item identity;
- stable marker identity;
- overflow classification;
- geometry validation;
- revision rejection;
- partial updates;
- correction precedence;
- focus transitions;
- empty-content behavior.

### 30.2 Contract Tests

Verify compatibility with:

- Reading Session acceptance contracts;
- SourceDocument references;
- TranslationResult references;
- PresentationProfile snapshots;
- UI Adapter RenderPlan contracts;
- canonical event envelope.

### 30.3 Golden Model Tests

Given fixed inputs, verify deterministic serialized PresentationSnapshot and RenderPlan outputs.

Representative fixtures:

- one horizontal comic region;
- vertical Chinese text;
- mixed directions;
- long Vietnamese translation;
- missing translation;
- partial translation;
- corrected translation;
- viewport resize;
- zoom transform;
- overlay-to-side-panel fallback.

### 30.4 Visual Regression Tests

Visual regression belongs mainly to UI Adapter, using deterministic RenderPlans from Presentation.

---

## 31. MVP Scope

Required for MVP:

```text
Side Panel
Ordered PresentationItems
Explicit source-to-item mapping
Lightweight numbered markers
Partial translation state
Translation failure state
Stable item identity
Content revision validation
Basic PresentationProfile
Basic target and viewport model
Overflow-safe text wrapping
Presentation diagnostics
Clear on session stop
```

Text Reader may be included when structured-text reading enters the MVP.

A limited Overlay prototype may begin only after the Side Panel flow is usable.

---

## 32. Deferred Scope

Deferred unless validated by prototypes:

- source-text removal;
- image inpainting;
- permanent translated-image rendering;
- artwork-aware placement;
- curved text layout;
- automatic font matching;
- advanced multi-monitor overlay optimization;
- animated transitions;
- plugin-provided presentation strategies;
- user-created themes;
- print layout;
- complete browser in-page rendering;
- synchronized bilingual novel reader.

---

## 33. Open Decisions

### 33.1 Side Panel Surface

- embedded in main window or detachable companion window;
- always-on-top behavior;
- source-window following;
- position persistence scope.

These are mainly Application Shell and UI Adapter decisions but affect target contracts.

### 33.2 Marker Design

- always visible or focus-only;
- number, outline, or both;
- obstruction limits;
- behavior with dense comic regions.

### 33.3 Overlay Policy

- all items or focused item only;
- automatic fallback threshold;
- draggable user adjustment;
- behavior during active scroll;
- capability requirements for capture exclusion.

### 33.4 Synchronization

- marker focus scrolls panel item;
- panel focus highlights source region;
- whether panel scrolling affects source focus;
- protection against unexpected movement.

### 33.5 Partial Publication

- show items after Recognition or after Translation starts;
- reserve order for untranslated items;
- segment-level versus token-level updates;
- acceptable visual movement threshold.

---

## 34. Architecture Invariants

1. Presentation does not perform OCR, Text Processing, or Translation.
2. Presentation does not own Reading Session lifecycle.
3. Every visible item keeps explicit source and translation traceability.
4. Presentation never accepts stale semantic content as current.
5. Coordinate spaces are explicit at public boundaries.
6. Readability takes priority over forcing text into source bounds.
7. MVP presentation is non-destructive.
8. Presentation owns PresentationSnapshot and RenderPlan.
9. UI Adapter owns actual framework and native rendering resources.
10. Presentation core does not depend on UI Adapter implementations.
11. Layout changes do not silently change semantic reading order.
12. Partial updates preserve stable identity whenever semantic grouping is unchanged.
13. Manual accepted correction outranks older automatic output.
14. Stopped or superseded sessions revoke commit authority.
15. Standard diagnostics do not contain user content.

---

## 35. Example Screen-Comic Flow

```text
Translation publishes accepted result reference
    ↓
Reading Session validates current authority
    ↓
BuildPresentation requested
    ↓
Presentation validates lineage and revisions
    ↓
Snapshot Builder creates stable PresentationItems
    ↓
Mode Resolver selects Side Panel
    ↓
Marker policy creates numbered marker models
    ↓
Overflow policy handles long Vietnamese text
    ↓
RenderPlan created
    ↓
PRESENTATION_PREPARED published
    ↓
UI Adapter renders markers and panel
    ↓
UI Adapter reports commit result
```

---

## 36. Example Viewport Change Flow

```text
UI Adapter observes target geometry change
    ↓
Normalized viewport snapshot submitted
    ↓
RecomputePresentationLayout requested
    ↓
Older reflow work cancelled or superseded
    ↓
Geometry Projector recalculates platform-neutral positions
    ↓
New RenderPlan created
    ↓
PRESENTATION_LAYOUT_CHANGED published
    ↓
UI Adapter applies newest presentation revision
```

If overlay geometry becomes invalid, Presentation may retain a usable Side Panel plan.

---

## 37. Example Stale Result Flow

```text
Content revision 14 begins presentation work
    ↓
User scrolls
    ↓
Content revision 15 becomes current
    ↓
Revision 15 presentation is accepted
    ↓
Late work for revision 14 completes
    ↓
Authority validation fails
    ↓
Revision 14 result becomes stale
    ↓
No current visible content is replaced
```

---

## 38. Recommended Implementation Order

```text
1. Public identifiers and references
2. PresentationProfile and PresentationTarget
3. PresentationSnapshot and PresentationItem
4. Explicit source-to-translation mapping
5. Side Panel strategy
6. Stable marker identity and numbering
7. Partial update behavior
8. Revision and authority validation
9. RenderPlan contract
10. Basic geometry projection
11. Overflow policy
12. Focus synchronization
13. Diagnostics
14. Text Reader strategy
15. Simple Overlay prototype
```

---

## 39. Completion Criteria

The module is architecturally usable when:

- accepted content can produce a deterministic Side Panel snapshot;
- every item can be traced to source and translation identifiers;
- markers can be rendered from stable adapter-neutral models;
- partial updates preserve identities and order;
- long Vietnamese text remains readable;
- stale revisions are rejected;
- stopped sessions clear or revoke visible presentation;
- viewport changes produce revisioned RenderPlans;
- Presentation can be tested without a native UI toolkit;
- Presentation has no dependency on OCR or translation providers;
- diagnostics and fallback reasons are observable;
- UI Adapter boundary is explicit and enforceable.

---

## 40. Related Documents

```text
.meta/AI_BOOT.md
.meta/CHANGE_RULE.md
.meta/MODULE_ROLE.md
.meta/PROJECT_RULE.md
.meta/SESSION_RULE.md
.meta/WORKFLOW.md

doc/01-architecture/core/CAPABILITY_MAP.md
doc/01-architecture/core/DATA_FLOW.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
doc/01-architecture/core/STATE_MACHINE.md

doc/01-architecture/modules/MODULE_DEPENDENCY.md
doc/01-architecture/modules/MODULE_MAP.md

doc/01-architecture/translate/PRESENTATION.md

doc/02-modules/presentation/CONTRACT.md
doc/02-modules/presentation/EVENTS.md
doc/02-modules/presentation/STATES.md
doc/02-modules/presentation/ERRORS.md

doc/02-modules/reading-session/MODULE.md
doc/02-modules/text-processing/MODULE.md
doc/02-modules/translation/MODULE.md
doc/02-modules/preferences/MODULE.md
doc/02-modules/diagnostics/MODULE.md
doc/02-modules/ui-adapter/MODULE.md
doc/02-modules/storage/MODULE.md
```

---

## 41. Documentation Ownership

This file defines:

- module purpose;
- boundary;
- ownership;
- core concepts;
- architectural responsibilities;
- cross-module relationships;
- invariants;
- MVP and deferred scope.

Detailed public schemas belong to `CONTRACT.md`.

Detailed state transitions belong to `STATES.md`.

Detailed event definitions belong to `EVENTS.md`.

Detailed error codes and recovery rules belong to `ERRORS.md`.

Implementation-specific rendering behavior belongs to `ui-adapter` and platform documentation.
