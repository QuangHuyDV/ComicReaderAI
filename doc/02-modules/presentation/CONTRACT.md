# Presentation Contract

> **Project:** CRAI  
> **Module:** `presentation`  
> **Path:** `doc/02-modules/presentation/CONTRACT.md`  
> **Contract Version:** 1.0.0  
> **Status:** Architecture Draft  
> **Owner:** CRAI Architecture

---

## 1. Purpose

This document defines the public boundary of the Presentation module.

Presentation converts accepted reading content into stable, revisioned presentation data that can be applied by `ui-adapter`.

The module produces two primary public outputs:

```text
PresentationSnapshot
+
RenderPlan
```

Presentation does not render pixels, create native windows, manipulate browser DOM, receive raw operating-system input, perform OCR, or execute translation.

The public contract exists to ensure that:

- upstream modules can request presentation work without importing internal Presentation types;
- `ui-adapter` can render Presentation output without depending on Presentation internals;
- asynchronous and incremental updates can be rejected when stale;
- commands, queries, events, and errors remain serializable and versionable;
- Presentation remains independent from any UI framework or operating system.

---

## 2. Contract Scope

This file owns the public definitions for:

- commands accepted by Presentation;
- queries exposed by Presentation;
- public data contracts;
- references shared across module boundaries;
- required validation metadata;
- revision and authority rules;
- compatibility and versioning rules.

This file does not define:

- internal algorithms;
- concrete layout implementations;
- module lifecycle state transitions;
- canonical event envelope details;
- complete event catalog;
- complete error catalog;
- persistence schemas;
- UI framework bindings.

Those concerns belong to:

```text
doc/02-modules/presentation/MODULE.md
doc/02-modules/presentation/STATES.md
doc/02-modules/presentation/EVENTS.md
doc/02-modules/presentation/ERRORS.md
doc/02-modules/ui-adapter/CONTRACT.md
```

---

## 3. Boundary Summary

The intended dependency flow is:

```text
translation / reading-session / preferences
                ↓
       Presentation commands
                ↓
          presentation
                ↓
 PresentationSnapshot + RenderPlan
                ↓
           ui-adapter
                ↓
      framework / platform UI
```

Presentation owns:

- presentation identity;
- presentation revision;
- presentation item identity;
- effective presentation mode;
- presentation snapshot construction;
- presentation-level geometry;
- layout planning;
- overflow and fallback decisions;
- presentation diagnostics metadata;
- stale-update rejection at its boundary.

Presentation does not own:

- reading-session lifecycle;
- translation-result lifecycle;
- persistent preferences;
- platform surface lifecycle;
- renderer or widget instances;
- native coordinates obtained directly from platform APIs;
- storage repositories.

---

## 4. Contract Conventions

### 4.1 Naming

Public contract type names use PascalCase.

Public field names are shown in camelCase.

Examples:

```text
BuildPresentation
PresentationRequest
presentationId
contentRevision
```

Concrete source code may adapt naming to the chosen language while preserving semantic meaning.

### 4.2 Serialization

All public contracts MUST be serializable.

Public payloads MUST NOT contain:

- UI framework objects;
- database connections;
- provider clients;
- native window handles;
- DOM nodes;
- mutable module-internal entities;
- executable callbacks.

### 4.3 Immutability

Public snapshots and plans are immutable values.

An update produces a new revision rather than mutating a previously published object in place.

### 4.4 Optional Fields

Optional fields MUST have explicit fallback behavior.

Absence of an optional field MUST NOT silently change semantic ownership.

### 4.5 References

Cross-module payloads should prefer stable references and bounded value objects over importing another module's internal aggregate.

---

## 5. Shared Identifier Contracts

### 5.1 PresentationId

Identifies one logical presentation across incremental updates for the same accepted content revision.

```text
PresentationId
- value
```

Rules:

- globally unique within one CRAI installation or runtime authority domain;
- stable while the same logical presentation is incrementally updated;
- replaced when a new logical presentation is created;
- never inferred from list position.

### 5.2 PresentationRequestId

Correlates a public command with its result or rejection.

```text
PresentationRequestId
- value
```

### 5.3 PresentationJobId

Identifies asynchronous Presentation work.

```text
PresentationJobId
- value
```

It may be absent for purely synchronous implementations, but the contract reserves it for cancellation, tracing, and diagnostics.

### 5.4 PresentationItemId

Identifies one semantic visible item inside a presentation.

```text
PresentationItemId
- value
```

Rules:

- stable across layout recomputation;
- stable across partial translation updates when semantic grouping is unchanged;
- replaced only when the underlying semantic item is removed or regrouped;
- never derived solely from the current array index.

### 5.5 MarkerId

Identifies one source association marker.

```text
MarkerId
- value
```

### 5.6 SurfaceId

Identifies a rendering surface known through a platform-independent contract.

```text
SurfaceId
- value
```

Presentation does not create or own the actual surface resource.

---

## 6. Revision and Authority Contracts

### 6.1 ContentRevision

Represents the accepted semantic source-content revision owned by `reading-session`.

```text
ContentRevision
- value: non-negative integer or opaque monotonic token
```

Presentation MUST NOT invent a new `contentRevision`.

### 6.2 PresentationRevision

Represents the version of Presentation output.

```text
PresentationRevision
- value: non-negative monotonic integer
```

Rules:

- increases whenever published Presentation output changes;
- MUST NOT decrease;
- older revisions MUST NOT overwrite newer revisions;
- layout-only changes may increase `presentationRevision` without changing `contentRevision`;
- focus or selection changes may increase `presentationRevision` when they are part of published snapshot state.

### 6.3 AuthorityContext

Carries the minimum information required to determine whether work is still authorized to become current.

```text
AuthorityContext
- sessionId
- contentId
- contentRevision
- expectedPresentationId?
- expectedPresentationRevision?
- sessionEpoch?
- requestStartedAt?
```

Rules:

- `sessionId`, `contentId`, and `contentRevision` are required for presentation work tied to active reading content;
- expected revision fields provide optimistic concurrency protection;
- `sessionEpoch` may be used when session identifiers can be reused;
- Presentation MUST reject results that fail authority checks.

---

## 7. Public Value Contracts

## 7.1 PresentationMode

```text
PresentationMode
- SidePanel
- Overlay
- TextReader
- Hybrid
```

Rules:

- the requested mode and effective mode are distinct values;
- unsupported requested modes may resolve to a supported fallback;
- fallback MUST be observable;
- unknown enum values MUST be handled as unsupported, not silently interpreted.

## 7.2 PresentationTarget

Describes the requested logical destination without exposing native UI objects.

```text
PresentationTarget
- surfaceId
- surfaceKind
- capabilities
- coordinateSpace
- sourceAssociation?
```

Possible `surfaceKind` values:

```text
MainReader
CompanionPanel
OverlaySurface
FloatingSurface
BrowserSurface
Unknown
```

`capabilities` may include:

```text
SupportsScrolling
SupportsOverlay
SupportsMarkers
SupportsPointerFocus
SupportsKeyboardFocus
SupportsTextSelection
SupportsDynamicResize
SupportsBilingualLayout
```

Presentation consumes these capabilities when resolving mode and fallback behavior.

## 7.3 PresentationProfile

An immutable, resolved preference snapshot used for one build or update.

```text
PresentationProfile
- profileId?
- profileRevision?
- preferredMode
- fontFamily?
- fontSize
- minimumReadableFontSize
- lineHeight
- paragraphSpacing
- panelWidth?
- panelPlacement?
- showSourceText
- showRegionMarkers
- markerStyle?
- overlayOpacity?
- theme
- autoFallbackEnabled
- accessibilityHints?
```

Rules:

- Presentation consumes but does not persist this profile;
- unsupported values produce bounded fallback or validation issues;
- profile changes do not alter `contentRevision`;
- profile changes may produce a new `presentationRevision`.

## 7.4 PresentationViewport

Describes the current logical rendering area.

```text
PresentationViewport
- surfaceId
- width
- height
- scale
- zoom
- scrollOffset
- devicePixelRatio?
- coordinateSpace
- sourceTransform?
- safeInsets?
```

Rules:

- width and height MUST be finite and non-negative;
- coordinate space MUST be declared;
- viewport changes do not create a new semantic content revision;
- viewport changes may require a new RenderPlan.

## 7.5 CoordinateSpace

```text
CoordinateSpace
- SourceImage
- CapturedFrame
- NormalizedSource
- ApplicationViewport
- BrowserViewport
- Screen
- OverlaySurface
```

A geometry value without coordinate-space metadata is invalid at a public module boundary.

## 7.6 Point

```text
Point
- x
- y
- coordinateSpace
```

## 7.7 Size

```text
Size
- width
- height
```

## 7.8 Rect

```text
Rect
- x
- y
- width
- height
- coordinateSpace
```

Validation:

- all numeric values MUST be finite;
- width and height MUST be non-negative;
- normalized coordinates MUST follow the declared normalized range policy;
- conversion between spaces requires an explicit transform.

## 7.9 GeometryTransform

```text
GeometryTransform
- fromSpace
- toSpace
- matrix?
- scaleX?
- scaleY?
- offsetX?
- offsetY?
- rotation?
- sourceRevision?
```

Presentation may consume normalized transforms supplied by adapters or upstream contracts.

It MUST NOT call native platform geometry APIs directly.

## 7.10 SourceRegionRef

A bounded Presentation-facing reference to recognized source geometry.

```text
SourceRegionRef
- regionId
- sourceId
- contentId
- contentRevision
- bounds
- polygon?
- rotation?
- readingOrder
- confidence?
```

## 7.11 SourceSegmentRef

```text
SourceSegmentRef
- segmentId
- regionIds[]
- sequence
- sourceText?
- language?
- status?
```

## 7.12 TranslationSegmentRef

A Presentation-facing view of accepted translation content.

```text
TranslationSegmentRef
- translationSegmentId
- sourceSegmentIds[]
- sequence
- translatedText
- status
- confidence?
- correctionRevision?
- translationRevision?
- issues[]?
```

Presentation MUST NOT receive provider-specific chunks as this contract.

## 7.13 PresentationStatus

```text
PresentationStatus
- Preparing
- Ready
- Updating
- Reflowing
- Degraded
- Clearing
- Cleared
- Rejected
```

The detailed lifecycle is defined by `STATES.md`.

## 7.14 PresentationItemStatus

```text
PresentationItemStatus
- Waiting
- Recognized
- Translating
- PartiallyTranslated
- Translated
- Corrected
- Failed
- Empty
- Suppressed
```

## 7.15 FocusState

```text
FocusState
- None
- Focused
- Selected
- ActiveForCorrection
```

## 7.16 PresentationIssue

```text
PresentationIssue
- issueId
- code
- severity
- presentationItemId?
- regionId?
- messageKey
- recoverable
- technicalDetails?
```

Technical details are diagnostic metadata and MUST NOT automatically be shown to end users.

---

## 8. Core Output Contracts

## 8.1 PresentationSnapshot

Represents the immutable semantic presentation state for one revision.

```text
PresentationSnapshot
- presentationId
- sessionId
- sourceId?
- contentId
- contentRevision
- presentationRevision
- requestedMode
- effectiveMode
- status
- items[]
- markers[]
- profileSummary?
- target
- issues[]
- createdAt
- updatedAt
```

Invariants:

- one snapshot belongs to one session and one content revision;
- a snapshot MUST NOT combine items from unrelated content revisions;
- every item maintains explicit references to upstream source data;
- snapshot identity remains stable across incremental updates of the same logical presentation;
- published snapshots are immutable;
- a newer revision replaces, but does not mutate, an older snapshot.

## 8.2 PresentationItem

```text
PresentationItem
- presentationItemId
- regionIds[]
- sourceSegmentIds[]
- translationSegmentIds[]
- sequence
- sourceText?
- translatedText?
- status
- confidence?
- focusState
- availableActions[]
- issues[]
- semanticHints?
```

Rules:

- mapping MUST NOT rely only on list position;
- `sequence` represents visible semantic order, not object identity;
- unchanged items preserve `presentationItemId` across updates;
- empty translation text requires an explicit status;
- a corrected item MUST NOT be overwritten by an older automatic result.

## 8.3 SourceMarker

```text
SourceMarker
- markerId
- presentationItemId
- regionId
- label
- sourceBounds
- state
- visibility
- emphasis?
```

Possible marker states:

```text
Normal
Focused
Selected
Uncertain
Failed
Hidden
```

## 8.4 RenderPlan

Represents a platform-independent instruction model that `ui-adapter` can apply.

```text
RenderPlan
- presentationId
- presentationRevision
- target
- effectiveMode
- viewport
- panelPlan?
- itemPlans[]
- markerPlans[]
- overlayPlans[]
- readerPlan?
- overflowItems[]
- hiddenItems[]
- fallback?
- issues[]
- strategyVersion
```

Invariants:

- RenderPlan is immutable;
- RenderPlan belongs to exactly one `presentationRevision`;
- RenderPlan contains no framework or native object;
- for the same accepted inputs and strategy version, output SHOULD be deterministic;
- `ui-adapter` may reject a RenderPlan whose surface is no longer valid;
- applying a RenderPlan does not transfer surface ownership to Presentation.

## 8.5 ItemRenderPlan

```text
ItemRenderPlan
- presentationItemId
- bounds?
- order
- visibility
- textStyle
- containerStyle?
- overflowBehavior
- interactionHints[]
```

## 8.6 MarkerRenderPlan

```text
MarkerRenderPlan
- markerId
- presentationItemId
- bounds
- label
- visibility
- emphasis
- connector?
```

## 8.7 PresentationFallback

```text
PresentationFallback
- requestedMode
- effectiveMode
- reasonCode
- affectedItemIds[]?
- automatic
```

---

## 9. Public Command Contracts

All commands MUST include:

```text
requestId
issuedAt
contractVersion
```

Commands tied to active content MUST also include `authority`.

## 9.1 BuildPresentation

Creates a new logical presentation from accepted content.

```text
BuildPresentation
- requestId
- contractVersion
- issuedAt
- authority
- sourceId?
- contentKind
- regions[]
- sourceSegments[]
- translationSegments[]
- translationStatus
- requestedMode
- target
- viewport
- profile
- correlationId?
```

Required validation:

- authority identifiers are present;
- `contentRevision` matches all supplied content references;
- region geometry declares coordinate space;
- segment mappings are internally consistent;
- requested mode is known or safely rejectable;
- target capabilities are declared sufficiently for mode resolution;
- viewport is valid;
- supplied translation data has been accepted upstream.

Success result:

```text
BuildPresentationResult
- requestId
- presentationId
- presentationRevision
- snapshot
- renderPlan
- fallback?
- diagnosticsSummary?
```

Failure result:

```text
PresentationCommandRejected
```

## 9.2 UpdatePresentationContent

Applies accepted incremental content changes without replacing unaffected item identities.

```text
UpdatePresentationContent
- requestId
- contractVersion
- issuedAt
- authority
- presentationId
- expectedPresentationRevision
- changedTranslationSegments[]
- removedTranslationSegmentIds[]
- changedSourceSegments[]?
- removedSourceSegmentIds[]?
- translationStatus
- updateCause
```

Rules:

- unchanged items MUST preserve identity;
- a mismatched expected revision is rejected or superseded according to the update policy;
- corrected content takes precedence over older automatic content;
- removed segments MUST NOT remain visible as active content;
- a full rebuild is allowed internally but MUST preserve public identity where semantics are unchanged.

Success result:

```text
UpdatePresentationContentResult
- requestId
- presentationId
- previousPresentationRevision
- presentationRevision
- changedItemIds[]
- removedItemIds[]
- snapshot
- renderPlan
```

## 9.3 RecomputePresentationLayout

Recomputes RenderPlan without changing semantic content.

```text
RecomputePresentationLayout
- requestId
- contractVersion
- issuedAt
- authority
- presentationId
- expectedPresentationRevision
- viewport
- target?
- profile?
- reason
```

Possible reasons:

```text
WindowResized
SourceMoved
ZoomChanged
ScrollChanged
PanelResized
ThemeChanged
FontChanged
DisplayChanged
TargetCapabilityChanged
ManualRefresh
```

Rules:

- semantic item identity MUST remain stable;
- semantic reading order MUST remain unchanged;
- obsolete layout jobs may be cancelled or coalesced;
- source-transform invalidation may produce a degraded side-panel plan instead of unsafe overlay geometry.

Success result:

```text
RecomputePresentationLayoutResult
- requestId
- presentationId
- previousPresentationRevision
- presentationRevision
- renderPlan
- fallback?
```

## 9.4 ChangePresentationMode

Requests a new presentation strategy.

```text
ChangePresentationMode
- requestId
- contractVersion
- issuedAt
- authority
- presentationId
- expectedPresentationRevision
- requestedMode
- target
- viewport
- profile?
```

Rules:

- effective mode may differ from requested mode;
- fallback reason MUST be returned when different;
- mode change MUST NOT alter `contentRevision`;
- Presentation item identity SHOULD remain stable when semantic grouping is unchanged.

Success result:

```text
ChangePresentationModeResult
- requestId
- presentationId
- previousMode
- requestedMode
- effectiveMode
- presentationRevision
- snapshot
- renderPlan
- fallback?
```

## 9.5 UpdatePresentationFocus

Updates Presentation-owned focus, selection, or correction state after an application intent is resolved outside the module.

```text
UpdatePresentationFocus
- requestId
- contractVersion
- issuedAt
- authority
- presentationId
- expectedPresentationRevision
- presentationItemId?
- regionId?
- focusState
- cause
```

Rules:

- raw mouse, keyboard, touch, or OS events MUST NOT enter this command;
- the requested item or region MUST belong to the active presentation;
- focus changes MUST NOT alter `contentRevision`;
- focus changes may produce a new `presentationRevision`.

## 9.6 ApplyPresentationProfile

Applies a new resolved PresentationProfile.

```text
ApplyPresentationProfile
- requestId
- contractVersion
- issuedAt
- authority
- presentationId
- expectedPresentationRevision
- profile
- viewport?
```

Rules:

- Presentation does not persist the profile;
- profile application may trigger layout recomputation;
- unsupported values produce bounded fallback or issues;
- content semantics remain unchanged.

## 9.7 ClearPresentation

Invalidates and removes active Presentation state.

```text
ClearPresentation
- requestId
- contractVersion
- issuedAt
- sessionId
- presentationId?
- expectedPresentationRevision?
- reason
```

Possible reasons:

```text
SessionStopping
SessionStopped
SourceChanged
ContentInvalidated
TargetUnavailable
PermissionLost
ApplicationClosing
UserRequested
```

Rules:

- clearing is idempotent;
- late work for the cleared authority MUST NOT become current;
- clear does not imply persistent content deletion;
- platform resource cleanup remains the responsibility of `ui-adapter`.

---

## 10. Public Query Contracts

Queries return immutable snapshots or summaries.

Queries MUST NOT expose mutable internal entities.

## 10.1 GetPresentationSnapshot

```text
GetPresentationSnapshot
- presentationId
- minimumRevision?
```

Result:

```text
GetPresentationSnapshotResult
- found
- snapshot?
```

## 10.2 GetRenderPlan

```text
GetRenderPlan
- presentationId
- presentationRevision?
```

Result:

```text
GetRenderPlanResult
- found
- renderPlan?
```

## 10.3 GetPresentationItem

```text
GetPresentationItem
- presentationId
- presentationItemId
```

Result:

```text
GetPresentationItemResult
- found
- item?
```

## 10.4 GetPresentationSummary

```text
GetPresentationSummary
- presentationId
```

Result:

```text
PresentationSummary
- presentationId
- contentId
- contentRevision
- presentationRevision
- effectiveMode
- status
- itemCount
- markerCount
- overflowCount
- issueCount
```

## 10.5 GetPresentationDiagnostics

```text
GetPresentationDiagnostics
- presentationId
- includeTechnicalDetails
```

Result MUST follow diagnostics and privacy rules.

Full source or translated content MUST NOT be returned by default as diagnostic data.

---

## 11. Command Result and Rejection Contract

## 11.1 PresentationCommandRejected

```text
PresentationCommandRejected
- requestId
- commandName
- presentationId?
- sessionId?
- contentId?
- contentRevision?
- expectedPresentationRevision?
- currentPresentationRevision?
- reasonCode
- recoverable
- retryAdvice?
- issues[]
```

Common reason codes:

```text
PRESENTATION_INVALID_COMMAND
PRESENTATION_UNKNOWN_SESSION
PRESENTATION_SESSION_NOT_ACTIVE
PRESENTATION_CONTENT_REVISION_STALE
PRESENTATION_REVISION_CONFLICT
PRESENTATION_NOT_FOUND
PRESENTATION_ITEM_NOT_FOUND
PRESENTATION_INVALID_MAPPING
PRESENTATION_INVALID_GEOMETRY
PRESENTATION_INVALID_VIEWPORT
PRESENTATION_UNSUPPORTED_MODE
PRESENTATION_TARGET_UNAVAILABLE
PRESENTATION_TARGET_CAPABILITY_MISSING
PRESENTATION_EMPTY_CONTENT
PRESENTATION_CANCELLED
PRESENTATION_INTERNAL_FAILURE
```

The complete catalog belongs to `ERRORS.md`.

## 11.2 Idempotency

Commands SHOULD support deduplication by `requestId` where duplicate delivery is possible.

Repeated `ClearPresentation` requests MUST be safe.

Repeated build or update requests with the same `requestId` MUST NOT create conflicting current revisions.

---

## 12. Consumed Event Boundary

Presentation may react to normalized domain events including:

```text
ReadingContentAccepted
TranslationPartiallyCompleted
TranslationCompleted
TranslationCorrected
SourceGeometryChanged
PresentationPreferenceChanged
PresentationTargetChanged
ReadingSessionStopping
ReadingSessionStopped
```

Rules:

- consumed events MUST follow the canonical event envelope;
- Presentation MUST NOT consume raw provider-specific OCR or translation events;
- event handlers MUST translate event payloads into the same validation path used by public commands;
- event delivery does not bypass revision or authority checks;
- duplicate events MUST be safely handled.

Exact names and payloads are defined in `EVENTS.md` and the architecture event convention.

---

## 13. Published Event Boundary

Presentation may publish:

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationModeFallbackApplied
PresentationFocusChanged
PresentationIssueDetected
PresentationRejected
PresentationCleared
```

Published events SHOULD include, where applicable:

```text
presentationId
sessionId
contentId
contentRevision
presentationRevision
requestId
correlationId
causationId
```

Events SHOULD carry bounded metadata or references rather than duplicating entire source and translation documents.

Exact event contracts belong to `EVENTS.md`.

---

## 14. Validation Contract

Presentation MUST reject or degrade input when any of the following is true:

- required authority identifiers are missing;
- the reading session is stopped or no longer authoritative;
- `contentRevision` is stale;
- expected `presentationRevision` conflicts with current state;
- cross-reference mappings are inconsistent;
- geometry has no coordinate space;
- numeric geometry values are non-finite;
- viewport dimensions are invalid;
- a requested mode is unsupported and no fallback is allowed;
- target capabilities cannot support the resolved plan;
- source or translation references belong to another content revision;
- malformed text cannot be represented safely;
- a command references an unknown presentation item.

Incomplete translation MUST NOT automatically cause whole-request rejection.

It may produce explicit waiting, partial, failed, or degraded item states.

---

## 15. Determinism Contract

For the same:

```text
accepted content
presentation profile
presentation target
viewport
strategy version
```

Presentation SHOULD produce semantically equivalent:

```text
PresentationSnapshot
RenderPlan
```

Determinism does not require equal timestamps or generated correlation identifiers.

Layout algorithms MUST NOT reorder semantic items because of insignificant geometry drift when an accepted reading order is available.

---

## 16. Performance Contract

Performance values are initially measurement targets, not permanent compatibility guarantees.

The contract MUST support measurement of:

- build duration;
- incremental-update duration;
- layout-recompute duration;
- marker-plan duration;
- cancelled or coalesced work;
- active snapshot memory;
- active RenderPlan memory.

Recommended prototype targets:

```text
Initial side-panel model preparation: <= 100 ms for typical MVP input
Incremental item update: <= 50 ms for typical MVP input
Layout preparation for ordinary viewport changes: <= 50 ms
Final UI application: owned by ui-adapter, not this contract
```

A strict `16 ms` Presentation guarantee is not required because heavy layout preparation may run off the UI thread.

Public contracts SHOULD avoid unnecessary duplication of large text payloads, but immutable snapshots may contain bounded repeated data when required for safe independent rendering.

---

## 17. Security and Privacy Contract

Presentation MUST NOT:

- store provider credentials;
- access the network directly;
- execute scripts from presentation content;
- access the filesystem directly;
- log full source or translated content by default;
- expose native window handles in public contracts;
- persist user content automatically;
- retain stopped-session data indefinitely.

Presentation may temporarily process:

- source text;
- translated text;
- source geometry;
- presentation preferences;
- user-correction metadata;
- content identifiers.

Persistence is performed only through explicit contracts owned by other modules, primarily `storage`.

---

## 18. Compatibility Contract

Presentation MUST remain independent from concrete technologies such as:

```text
Electron
Tauri
Flutter
Qt
Wails
Browser Extension APIs
Android Views
SwiftUI
Canvas implementations
```

Technology-specific adapters may depend on Presentation public contracts.

Presentation core MUST NOT import those adapters.

Unknown optional fields MUST be ignored when safe.

Unknown required semantic enum values MUST be rejected or mapped through an explicitly versioned fallback rule.

---

## 19. Contract Versioning

The contract version follows semantic versioning.

```text
MAJOR.MINOR.PATCH
```

### Patch

May include:

- clarification with no semantic change;
- additional documentation;
- compatible validation correction;
- new optional diagnostics metadata.

### Minor

May include:

- new optional fields;
- new commands or queries;
- new optional capabilities;
- new enum values that older consumers can safely treat as unknown;
- new published events.

### Major

Required for:

- removal of public fields;
- renaming public commands;
- changing required-field semantics;
- changing revision meaning;
- changing ownership of snapshot or RenderPlan;
- incompatible enum handling;
- changing immutable output into mutable shared state.

Every command SHOULD declare `contractVersion`.

Event versioning follows the canonical event convention.

---

## 20. Architecture Invariants

The following MUST always remain true:

1. Presentation does not perform OCR or translation.
2. Presentation does not own reading-session lifecycle.
3. Presentation owns `PresentationSnapshot` and `RenderPlan` semantics.
4. `ui-adapter` owns actual rendering and platform resource application.
5. Every visible item has explicit source references.
6. Coordinate spaces are explicit at public boundaries.
7. Published snapshots and RenderPlans are immutable.
8. `presentationRevision` never decreases.
9. Stale semantic content never becomes current.
10. Layout changes do not silently change accepted reading order.
11. Item identity survives non-semantic layout changes.
12. Rendering frameworks remain unknown to Presentation.
13. Incomplete translation is represented explicitly rather than hidden as success.
14. Readability takes priority over forcing overlay placement.
15. Session stop prevents late Presentation work from becoming visible.

---

## 21. Example Build Flow

```text
Reading Session accepts content revision 42
    ↓
BuildPresentation
- authority.contentRevision = 42
- requestedMode = SidePanel
- target capabilities supplied
    ↓
Presentation validates mappings and geometry
    ↓
Presentation creates:
- PresentationSnapshot revision 1
- RenderPlan revision 1
    ↓
BuildPresentationResult
    ↓
PresentationPrepared
    ↓
UI Adapter verifies target and revision
    ↓
UI Adapter applies RenderPlan
```

---

## 22. Example Incremental Update Flow

```text
Presentation revision 4 is visible
    ↓
Translation correction is accepted upstream
    ↓
UpdatePresentationContent
- expectedPresentationRevision = 4
- changedTranslationSegments = corrected segment
    ↓
Presentation preserves unaffected item identities
    ↓
Presentation creates revision 5
    ↓
PresentationUpdated
    ↓
UI Adapter ignores any later revision 4 plan
```

---

## 23. Example Viewport Flow

```text
Source viewport changes
    ↓
UI Adapter or application shell normalizes viewport data
    ↓
RecomputePresentationLayout
    ↓
Presentation validates coordinate spaces and authority
    ↓
New RenderPlan created
    ↓
PresentationLayoutChanged
    ↓
UI Adapter applies newest revision only
```

---

## 24. Example Stale Rejection

```text
Content revision 14 begins Presentation work
    ↓
Reading Session advances to content revision 15
    ↓
Late work for revision 14 completes
    ↓
Authority check fails
    ↓
PresentationCommandRejected
- reasonCode = PRESENTATION_CONTENT_REVISION_STALE
    ↓
No current snapshot or RenderPlan is replaced
```

---

## 25. Deferred Contract Extensions

The following are intentionally deferred:

- plugin-provided presentation strategies;
- persisted user-adjusted overlay geometry;
- collaborative annotations;
- image inpainting output;
- export-oriented page layout;
- framework-specific animation instructions;
- multi-monitor native placement contracts;
- complete accessibility certification contracts;
- AI-generated artwork-aware placement hints;
- direct browser DOM mutation contracts.

Future extensions MUST preserve the Presentation/UI Adapter boundary.

---

## 26. Related Documents

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULE_ROLE.md
.meta/WORKFLOW.md

doc/01-architecture/core/CAPABILITY_MAP.md
doc/01-architecture/core/DATA_FLOW.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
doc/01-architecture/core/STATE_MACHINE.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md
doc/01-architecture/modules/MODULE_MAP.md

doc/02-modules/presentation/MODULE.md
doc/02-modules/presentation/STATES.md
doc/02-modules/presentation/EVENTS.md
doc/02-modules/presentation/ERRORS.md
doc/02-modules/presentation/README.md

doc/02-modules/reading-session/CONTRACT.md
doc/02-modules/translation/CONTRACT.md
doc/02-modules/preferences/CONTRACT.md
doc/02-modules/diagnostics/CONTRACT.md
doc/02-modules/ui-adapter/CONTRACT.md
```

---

## 27. Completion Criteria

This contract is ready for implementation review when:

- every public command has required authority and revision metadata;
- `PresentationSnapshot` and `RenderPlan` ownership is unambiguous;
- `ui-adapter` can render without importing Presentation internals;
- partial translation can be represented without rejecting the complete presentation;
- item identity rules are testable;
- stale content and revision conflicts are deterministic;
- coordinate spaces are explicit;
- mode fallback is observable;
- public payloads are serializable and platform-independent;
- event and error files can refine this contract without redefining its ownership.
