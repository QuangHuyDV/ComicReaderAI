# Presentation Module Specification

> **Project:** CRAI
> **Module:** `presentation`
> **Path:** `doc/02-modules/presentation/MODULE.md`
> **Version:** 1.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Last Updated:** 2026-08-08

---

# 1. Module Definition

Presentation is the CRAI business module responsible for transforming accepted source and translation artifacts into a stable, revisioned, framework-neutral model suitable for user-visible rendering.

Its primary responsibility is:

```text
Accepted Runtime Artifacts
    +
Presentation Profile
    +
Presentation Target
    +
Viewport / Geometry Context
    ↓
Presentation Execution
    ↓
Candidate Presentation State
    ↓
Runtime / Presentation Authority Revalidation
    ↓
Presentation Commit
    ↓
Committed PresentationSnapshot
    +
Committed RenderPlan
    ↓
UI Adapter
    ↓
Application Surface
```

Presentation determines:

> **What should currently be presented, how visible information should be organized, and what framework-neutral render plan should be applied.**

Presentation does not determine:

* whether a Runtime Revision is current;
* whether a WorkItem or Attempt remains authoritative;
* whether an upstream Candidate Artifact becomes published;
* how native widgets, DOM nodes, windows, overlays, or operating-system resources are created;
* how OCR or translation is performed.

Presentation is the **semantic preparation and commit owner for user-visible presentation state**.

It is not the owner of Runtime execution authority.

---

# 2. Module Identity

```text
Module ID: presentation
Module Type: Core Business Presentation Module
Primary Domain: User-visible reading representation
Execution Model: Runtime-coordinated presentation operations
Primary Inputs: Published Artifact references + presentation context
Primary Output: Candidate Presentation State
Committed Output: PresentationSnapshot + RenderPlan
State Ownership: Presentation semantic and committed display state
Runtime Authority: Runtime Control
Native Rendering Owner: UI Adapter / Platform
MVP Priority: Required
```

Unlike Recognition, Text Processing, and Translation, Presentation does not primarily publish a new pipeline Artifact for downstream semantic processing.

Its main output is a committed presentation state used by the UI boundary.

---

# 3. Architectural Position

The normal processed-content path is:

```text
Capture / Structured Source
        ↓
Recognition when required
        ↓
Recognition Artifact
        ↓
Text Processing
        ↓
SourceDocument Artifact
        ↓
Translation
        ↓
Translation Artifact
        ↓
Runtime confirms current usable inputs
        ↓
Presentation
        ↓
Presentation Commit
        ↓
PresentationSnapshot + RenderPlan
        ↓
UI Adapter
        ↓
Visible Reading Surface
```

For structured text that does not require OCR:

```text
Structured Source
        ↓
Text Processing
        ↓
SourceDocument Artifact
        ↓
Translation
        ↓
Translation Artifact
        ↓
Presentation
```

Presentation therefore consumes **published and accepted runtime inputs**, not provider DTOs, temporary module outputs, or unaccepted Candidate Artifacts.

---

# 4. Core Architectural Separation

CRAI separates four concerns that must not be collapsed into Presentation.

## 4.1 Business Presentation Semantics

Presentation owns:

```text
Presentation identity
Presentation item construction
Source/translation association
Presentation mode
Visual semantic grouping
Framework-neutral layout planning
Overflow/fallback policy
Presentation revision
Committed PresentationSnapshot
Committed RenderPlan
Presentation-specific focus/selection
```

## 4.2 Runtime Execution Authority

Runtime Control owns:

```text
Session runtime authority
Revision authority
WorkItem lifecycle
Attempt lifecycle
Cancellation authority
Retry authority
Completion acceptance
Stale-result rejection
Downstream eligibility
```

## 4.3 Artifact Publication

Runtime Artifact Store owns accepted runtime Artifact publication and Artifact lifetime.

Presentation must not impersonate Artifact Store ownership merely because it consumes Artifact references.

## 4.4 Native Rendering

UI Adapter and platform implementations own:

```text
Widgets
DOM nodes
Native windows
Native handles
Overlay surfaces
UI-thread mutation
Platform coordinate conversion
Click-through
Capture exclusion
Accessibility APIs
```

The central rule is:

```text
Runtime decides whether the work still matters.

Presentation decides what the valid current result should look like.

UI Adapter decides how that committed plan becomes actual UI.
```

---

# 5. Architectural Question

Presentation answers:

> How should current accepted reading content be represented, arranged, and exposed for rendering?

It does not answer:

> Is this Revision still authoritative?

That belongs to Runtime Control.

It also does not answer:

> How should a specific operating system or UI framework draw this model?

That belongs to UI Adapter and platform implementations.

---

# 6. Primary Goals

## 6.1 Minimal Reading Interruption

Presentation must support reading without requiring repeated:

* copy/paste;
* application switching;
* manual source/translation matching;
* region selection;
* scrolling caused by unstable incremental updates.

## 6.2 Clear Source Association

Every visible translated item should remain traceable to the accepted upstream semantic structures that produced it.

Typical lineage:

```text
Recognition Region
        ↓
Source Block / Segment
        ↓
Translation Unit / Segment
        ↓
Presentation Item
        ↓
Marker / Layout
```

Not every flow requires every layer, but mappings must use stable identifiers where those layers exist.

## 6.3 Readability First

Vietnamese output may be substantially longer than Chinese or English source text.

Presentation must prefer readable fallback over geometrically exact but unreadable output.

## 6.4 Stable Incremental Updates

Changes to translation, viewport, preferences, focus, or completeness should preserve:

* item identity;
* semantic ordering;
* focus where possible;
* scroll anchors where possible;
* marker identity where mapping has not changed.

## 6.5 Provider Independence

Presentation must remain independent from:

* OCR providers;
* translation providers;
* provider SDK types;
* model-specific output objects.

## 6.6 Platform Independence

Presentation contracts must contain no:

* native window handles;
* DOM nodes;
* UI framework objects;
* operating-system callbacks;
* mutable view instances.

## 6.7 Replaceable Presentation Strategies

Side Panel, Text Reader, Overlay, and Hybrid are strategies behind common Presentation contracts.

No strategy may redefine module ownership.

---

# 7. Inputs

Presentation consumes immutable references or bounded immutable value objects.

Typical inputs are:

```text
PresentationExecutionInput
├── RuntimeExecutionContext
├── PresentationContext
├── SourceDocumentArtifactRef?
├── TranslationArtifactRef?
├── RecognitionArtifactRef?
├── PresentationProfile
├── PresentationTarget
├── ViewportSnapshot
└── PreviousPresentationRef?
```

Not all modes require every Artifact type.

For example:

* a translated comic normally uses source, translation, and region geometry;
* a novel reader may require SourceDocument and Translation Artifact but no Recognition Artifact;
* source-only preview may temporarily have no Translation Artifact.

---

# 8. Runtime Execution Context

Presentation operations invoked through Runtime carry runtime identity.

Conceptually:

```text
RuntimeExecutionContext
├── SessionId
├── RevisionId
├── WorkItemId?
├── AttemptId?
├── CancellationContext
├── ConfigurationSnapshotRef
└── AuthorityToken / AuthorityContextRef
```

Presentation may validate that required runtime metadata is present and internally consistent.

Presentation must not independently decide that:

```text
RevisionId == current Revision
```

using a competing private authority registry.

Runtime Control remains the authoritative owner of current Revision relevance.

---

# 9. Presentation Context

A Presentation Context identifies the logical user-visible destination for presentation state.

```text
PresentationContext
├── presentationContextId
├── sessionId
├── targetId
├── contentIdentity
├── expectedPresentationRevision?
└── previousPresentationRef?
```

A context is not a native window or widget.

It is a semantic scope for committed presentation state.

Typical examples:

```text
main-reading-surface
comic-companion-panel
text-reader
temporary-focus-overlay
```

---

# 10. Presentation Operation

`PresentationOperation` represents module-owned semantic processing for one requested presentation change.

Conceptually:

```text
PresentationOperation
├── operationId
├── operationType
├── presentationContextId
├── inputRevision
├── expectedPresentationRevision?
├── phase
├── startedAt
└── diagnostics
```

Possible operation types:

```text
BUILD
UPDATE_CONTENT
REFLOW
CHANGE_MODE
UPDATE_FOCUS
CLEAR
```

`PresentationOperation` is not a Runtime WorkItem.

Presentation does not own:

```text
WorkItem state
Attempt state
retry count
scheduler state
queue state
terminal Runtime outcome
```

The module may expose operation phases for diagnostics, but these do not replace Runtime lifecycle state.

---

# 11. Candidate Presentation State

Presentation must prepare a new state before committing it.

Conceptually:

```text
CandidatePresentationState
├── presentationId
├── presentationContextId
├── basedOnPresentationRevision?
├── candidatePresentationRevision
├── sourceArtifactRefs[]
├── snapshot
├── renderPlan
├── changeSet
├── fallback
├── completeness
├── warnings[]
└── diagnostics
```

A candidate is not user-visible authoritative state.

It may be discarded because:

* Runtime authority was revoked;
* the target was replaced;
* viewport revision became obsolete;
* a newer presentation operation committed first;
* cancellation was observed;
* validation failed;
* UI target capabilities changed.

Core rule:

```text
Prepared does not mean committed.
```

---

# 12. Presentation Commit

Presentation commit is the atomic transition from a validated candidate to current Presentation state.

```text
Candidate Presentation State
        ↓
Authority Revalidation
        ↓
Presentation Revision Check
        ↓
Candidate Invariant Validation
        ↓
Atomic Presentation Commit
        ↓
Committed PresentationSnapshot
        +
Committed RenderPlan
```

A commit must update the logically paired:

```text
PresentationSnapshot
RenderPlan
PresentationRevision
```

as one coherent presentation state.

The UI must never observe:

```text
new Snapshot + old RenderPlan
```

or:

```text
old Snapshot + new RenderPlan
```

as the committed pair.

---

# 13. Runtime Authority and Presentation Commit

Presentation has no independent global authority model.

Before a candidate becomes current, the commit path must verify that Runtime still permits the result to become user-visible.

Conceptually:

```text
Presentation prepares Candidate
        ↓
Runtime authority revalidation
        ↓
ACCEPT
        ├── commit Presentation
        └── expose committed result

REJECT
        ├── discard Candidate
        └── keep current committed Presentation
```

Possible authority rejection causes include:

```text
Revision superseded
Session stopping
Session stopped
Work canceled
Newer current content
Newer conflicting operation
Target replaced
```

Presentation may additionally reject its own candidate for presentation-specific reasons.

Runtime rejection and Presentation validation rejection are separate concepts.

---

# 14. PresentationSnapshot

`PresentationSnapshot` is the immutable semantic representation of one committed Presentation revision.

```text
PresentationSnapshot
├── presentationId
├── presentationContextId
├── sessionId
├── contentIdentity
├── sourceArtifactRefs[]
├── presentationRevision
├── effectiveMode
├── appliedProfile
├── completeness
├── items[]
├── markers[]
├── issues[]
├── focusState
├── selectionState
└── createdAt
```

Rules:

1. one snapshot belongs to one Presentation Context;
2. one snapshot represents one committed Presentation revision;
3. a newer revision supersedes the previous committed revision for that context;
4. snapshots are immutable;
5. item identity remains stable when semantic grouping remains stable;
6. snapshot content must derive only from compatible accepted inputs.

---

# 15. PresentationItem

A `PresentationItem` represents one user-visible semantic reading unit.

```text
PresentationItem
├── presentationItemId
├── sourceBlockIds[]
├── sourceSegmentIds[]
├── translationUnitIds[]
├── translationSegmentIds[]
├── recognitionRegionIds[]
├── sequence
├── sourceText?
├── translatedText?
├── semanticRole
├── completeness
├── confidenceSummary?
├── layoutHints
├── availableActions[]
└── issues[]
```

Possible items include:

* comic dialogue;
* narration;
* heading;
* paragraph;
* structured novel dialogue;
* source-only item;
* partially translated item;
* failed translation placeholder;
* accepted corrected translation.

Array position is never the canonical identity mechanism.

---

# 16. Source and Translation Traceability

Presentation preserves explicit lineage whenever available.

```text
RecognitionRegionId
        ↕
SourceBlockId
        ↕
SourceSegmentId
        ↕
TranslationUnitId
        ↕
TranslationSegmentId
        ↕
PresentationItemId
```

Presentation may collapse multiple source structures into one visible unit only through an explicit presentation grouping decision.

Such grouping must not mutate upstream Artifacts.

---

# 17. Semantic Ordering

Canonical semantic reading order comes from upstream accepted content.

Presentation may:

* group adjacent items;
* place items into columns;
* build panel sections;
* hide optional secondary information;
* choose marker labels;
* reflow visual positions.

Presentation must not silently reinterpret canonical reading order merely to make layout easier.

If Presentation requires a different visual traversal order, it must be represented separately from semantic order.

---

# 18. PresentationMarker

A `PresentationMarker` provides a lightweight source-to-Presentation association.

```text
PresentationMarker
├── markerId
├── presentationItemId
├── sourceRegionRef
├── label
├── sourceGeometry
├── projectedGeometry?
├── visibility
├── emphasis
└── state
```

Presentation owns marker semantics.

UI Adapter owns marker rendering.

A marker is not:

* a native overlay window;
* a DOM element;
* a platform view;
* an input event listener.

---

# 19. PresentationProfile

`PresentationProfile` is an immutable resolved preference snapshot consumed by Presentation.

```text
PresentationProfile
├── profileId
├── profileVersion
├── preferredMode
├── typography
├── readerWidth
├── panelConfiguration
├── sourceVisibility
├── markerPolicy
├── overlayPolicy
├── themeSemantics
├── readabilityThresholds
├── fallbackPolicy
└── accessibilityPreferences
```

Preferences owns:

* setting persistence;
* preference resolution;
* user preference lifecycle.

Presentation owns:

* applying the resolved profile to Presentation semantics.

Presentation does not persist PresentationProfile.

---

# 20. Presentation Modes

Conceptual modes:

```text
SIDE_PANEL
TEXT_READER
OVERLAY
HYBRID
```

Mode is a semantic Presentation decision.

Native surface implementation remains outside Presentation.

---

# 21. Side Panel

Side Panel is the primary MVP mode for comic and screen-based reading.

Characteristics:

* ordered translation items;
* optional source text;
* stable source-region association;
* lightweight markers;
* scrollable translated content;
* readable long Vietnamese output;
* translation status;
* issue indicators;
* correction/retranslation action metadata.

Side Panel should be the fallback when overlay readability or geometry capability is insufficient.

---

# 22. Text Reader

Text Reader targets structured text such as:

* novels;
* imported text;
* clipboard text;
* structured browser content;
* future document readers.

Characteristics:

* paragraph preservation;
* headings;
* dialogue roles;
* stable navigation anchors;
* configurable reader width;
* bilingual/source toggle;
* typography suitable for long reading sessions.

Text Reader may share PresentationItem contracts with Side Panel while using a different RenderPlan strategy.

---

# 23. Overlay

Overlay displays translated information near source geometry without modifying the source content.

Overlay requires:

* valid geometry lineage;
* explicit coordinate spaces;
* supported target capabilities;
* minimum readable font constraints;
* overlap handling;
* stale overlay removal;
* deterministic fallback;
* correct viewport revision;
* compatible UI Adapter/platform features.

Presentation creates overlay semantics and RenderPlan.

UI Adapter creates the real overlay surface.

---

# 24. Hybrid

Hybrid may combine:

```text
Source markers
+
Side Panel
+
Focused temporary overlay
```

Hybrid remains deferred until user testing demonstrates sufficient value.

The architecture must permit Hybrid without requiring new ownership rules.

---

# 25. PresentationTarget

`PresentationTarget` describes a logical render destination.

```text
PresentationTarget
├── targetId
├── targetKind
├── capabilities[]
├── bounds
├── coordinateSpace
├── scale
├── safeInsets
├── targetRevision
└── capabilityRevision
```

Possible target kinds:

```text
MAIN_WINDOW
COMPANION_PANEL
FLOATING_SURFACE
OVERLAY_SURFACE
BROWSER_SURFACE
```

A target is never a native handle.

---

# 26. ViewportSnapshot

Presentation consumes an immutable normalized viewport snapshot.

```text
ViewportSnapshot
├── viewportId
├── viewportRevision
├── targetId
├── bounds
├── coordinateSpace
├── scale
├── visibleRegion
├── transforms[]
└── capturedAt
```

Viewport acquisition belongs to UI Adapter/platform integration.

Presentation consumes normalized geometry.

Rapid viewport changes may be coalesced before expensive layout work.

---

# 27. Geometry Ownership

Geometry must always declare its coordinate space.

Typical spaces:

```text
SOURCE_IMAGE
CAPTURED_FRAME
NORMALIZED_SOURCE
APPLICATION_VIEWPORT
SCREEN
OVERLAY_SURFACE
BROWSER_VIEWPORT
```

A public rectangle without coordinate-space metadata is invalid.

Presentation may own platform-neutral projection logic.

Presentation does not own:

* OS coordinate queries;
* window-position acquisition;
* display enumeration;
* DPI APIs;
* native monitor transforms.

---

# 28. Geometry Validation

Presentation must reject or degrade invalid geometry such as:

* NaN or infinite coordinates;
* negative width or height;
* invalid bounds;
* zero source dimensions where projection requires dimensions;
* unknown coordinate spaces;
* missing required transform;
* stale viewport revision;
* invalid transform chain;
* source region outside declared source bounds.

Geometry failure does not automatically mean the entire Presentation must fail.

Side Panel may remain usable even when overlay projection cannot be trusted.

---

# 29. RenderPlan

`RenderPlan` is the immutable framework-neutral arrangement produced by Presentation.

```text
RenderPlan
├── renderPlanId
├── presentationId
├── presentationRevision
├── effectiveMode
├── targetId
├── targetRevision
├── viewportRevision
├── strategyVersion
├── itemLayouts[]
├── markerLayouts[]
├── overflowItems[]
├── hiddenItems[]
├── focusLayout
├── fallback?
└── diagnostics
```

Presentation owns RenderPlan semantics.

UI Adapter owns applying RenderPlan.

---

# 30. RenderPlan Invariants

A committed RenderPlan must:

1. belong to exactly one committed Presentation revision;
2. target a compatible PresentationTarget;
3. use explicit coordinate spaces;
4. preserve semantic item identity;
5. preserve semantic reading order unless an explicit visual order field differs;
6. honor minimum readability constraints;
7. contain no framework objects;
8. contain no native window handles;
9. contain no executable callbacks;
10. be deterministic for semantically equivalent fixed inputs and strategy versions.

---

# 31. Mode Resolution

Presentation resolves the effective mode using:

```text
Requested Mode
+
Content Kind
+
Presentation Profile
+
Target Capabilities
+
Viewport
+
Geometry Quality
+
Readability Constraints
+
Fallback Policy
    ↓
Effective Mode
```

The effective mode may differ from the preferred mode.

Any fallback must expose a reason.

---

# 32. Readability Policy

Presentation prioritizes:

```text
Readable
    ↓
Stable
    ↓
Spatially Associated
    ↓
Geometrically Exact
```

Exact geometric placement must not make translated text unreadable.

Presentation must not silently shrink Vietnamese text indefinitely to preserve source-region bounds.

---

# 33. Overflow Policy

Typical bounded overflow sequence:

```text
Wrap
    ↓
Use available flexible space
    ↓
Expand logical container where allowed
    ↓
Allow scrolling
    ↓
Collapse secondary content
    ↓
Use focused overlay only
    ↓
Fallback to Side Panel
```

A strategy may define a narrower policy.

All fallback behavior must remain bounded.

---

# 34. Typography Policy

Presentation may decide framework-neutral typography semantics such as:

* font family preference;
* font size;
* line height;
* paragraph spacing;
* emphasis;
* minimum readable size;
* text alignment;
* wrapping policy.

Presentation should not depend on framework-specific font handles.

Font measurement may be provided through an abstraction when actual text metrics depend on the selected rendering stack.

---

# 35. Text Direction

Presentation consumes accepted text-direction and reading-order semantics from upstream where available.

Presentation may use direction information to choose layout.

It must not rerun OCR text-direction inference.

Examples:

```text
LTR
RTL
VERTICAL_TTB
VERTICAL_BTT
MIXED
UNKNOWN
```

Visual layout rules may depend on direction without changing the semantic source Artifact.

---

# 36. Progressive Presentation

Presentation should support incomplete but useful reading states.

Examples:

```text
SOURCE_AVAILABLE
WAITING_FOR_TRANSLATION
PARTIALLY_TRANSLATED
TRANSLATED
CORRECTED
TRANSLATION_FAILED
SUPPRESSED
```

These are item/content presentation states.

They are not Runtime Attempt states.

---

# 37. Partial Translation Policy

Presentation may build a useful Presentation from an accepted partial Translation Artifact or other explicitly supported partial upstream result.

Default expectation:

```text
Accepted partial semantic result
        ↓
Stable item mapping
        ↓
Commit useful partial Presentation
        ↓
Later accepted Translation revision
        ↓
Update affected PresentationItems
        ↓
Commit new Presentation revision
```

Presentation must not consume raw provider token streams directly.

Token-level rendering is optional and must first be normalized through an owned upstream contract.

---

# 38. Stable Identity

Reflow or status changes must not generate new semantic identities unnecessarily.

Preserve:

```text
PresentationItemId
MarkerId
PresentationId
```

when:

* semantic grouping is unchanged;
* source lineage is unchanged;
* only viewport changes;
* typography changes;
* translation status changes within the same mapped item;
* focus changes;
* presentation mode changes where stable mapping is possible.

---

# 39. PresentationRevision

`PresentationRevision` changes whenever committed user-visible Presentation state changes.

Examples:

* translation content changes;
* partial result becomes more complete;
* correction becomes active;
* profile affects visible output;
* viewport causes layout change;
* mode changes;
* visibility changes;
* focus/selection changes when represented in committed Presentation state.

`PresentationRevision` is separate from Runtime `RevisionId`.

This distinction is mandatory.

```text
Runtime RevisionId
    → execution authority / current processing intent

PresentationRevision
    → committed visible representation version
```

---

# 40. Runtime Revision vs Presentation Revision

One Runtime Revision may produce multiple Presentation revisions.

Example:

```text
Runtime Revision 42
    ↓
Presentation Revision 1
    ↓
Translation partial update
    ↓
Presentation Revision 2
    ↓
Viewport resize
    ↓
Presentation Revision 3
    ↓
Focus change
    ↓
Presentation Revision 4
```

A new PresentationRevision does not automatically create a new Runtime Revision.

Conversely, a superseded Runtime Revision revokes commit authority for Presentation candidates derived exclusively from that obsolete runtime context.

---

# 41. Focus and Selection

Presentation distinguishes:

```text
FOCUSED
SELECTED
ACTIVE_FOR_CORRECTION
```

Focus:

* temporary visual attention.

Selection:

* explicit persistent user choice within the current Presentation.

Active-for-correction:

* identifies the semantic item currently targeted by a correction workflow.

Presentation may own this Presentation-local state.

Actual pointer/keyboard input remains UI Adapter responsibility.

---

# 42. Presentation Actions

Presentation may expose semantic action metadata such as:

```text
FOCUS
SELECT
COPY_SOURCE
COPY_TRANSLATION
RETRANSLATE
EDIT_SOURCE
EDIT_TRANSLATION
ADD_GLOSSARY_ENTRY
REPORT_ISSUE
```

Presentation does not execute actions belonging to other modules.

Example:

```text
RETRANSLATE
    ↓
Presentation exposes action
    ↓
UI Adapter emits user intent
    ↓
Application / Runtime command handling
    ↓
Translation work scheduled
```

No reverse dependency from Presentation to Translation implementation is required.

---

# 43. Presentation State Ownership

Presentation owns its semantic state.

High-level committed-state model:

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
  ↔
RECONFIGURING
  ↓
CLEARING
  ↓
EMPTY
```

Potential degraded condition:

```text
DEGRADED_READY
```

Detailed state semantics belong to `STATES.md`.

These are Presentation-owned states only.

They are not Runtime WorkItem or Attempt states.

---

# 44. Preserve Previous Committed State

Presentation should generally preserve the previous known-good committed state while preparing a replacement.

```text
Current Committed Presentation
        +
Candidate being prepared
        ↓
Candidate valid?
    ├── yes → atomic replacement
    └── no  → discard candidate
              keep previous committed state
```

This avoids unnecessary blanking or flicker.

Exceptions include:

* Session stop;
* explicit clear;
* source privacy invalidation;
* target destruction;
* content replacement where old content must no longer be shown.

---

# 45. Clear Semantics

`ClearPresentation` logically invalidates current visible Presentation state.

Typical reasons:

```text
SESSION_STOPPED
SESSION_REPLACED
CONTENT_REPLACED
TARGET_DESTROYED
USER_CLEARED
PRIVACY_INVALIDATION
APPLICATION_SHUTDOWN
```

Clear must invalidate Presentation authority before or atomically with UI removal semantics.

Physical widget destruction remains UI Adapter responsibility.

---

# 46. Cancellation

Runtime owns cancellation authority.

Presentation must cooperate with cancellation through supplied cancellation context.

Presentation should check cancellation before or during expensive operations such as:

* large item mapping;
* overlay layout;
* typography measurement;
* geometry projection;
* expensive reflow.

Cancellation means:

```text
stop producing obsolete candidate work where practical
```

It does not mean Presentation may mutate Runtime Attempt state.

---

# 47. Supersession

A Presentation candidate can become obsolete because:

* Runtime Revision is superseded;
* newer Translation Artifact becomes current;
* newer viewport revision arrives;
* newer preference revision arrives;
* target is replaced;
* a newer Presentation operation commits.

Obsolete candidates must not overwrite newer committed Presentation state.

Supersession is expected concurrency behavior, not automatically an error.

---

# 48. Stale Result Protection

At minimum, commit protection should consider:

```text
Runtime authority
PresentationContext identity
source Artifact compatibility
translation Artifact compatibility
target identity
target revision
viewport revision when layout-sensitive
expected PresentationRevision
```

A stale result must be discarded or rejected without corrupting current Presentation state.

---

# 49. Relationship with Reading Session

Reading Session owns business reading-session semantics, such as:

* reading session identity;
* user reading context;
* source selection;
* pause/resume semantics where defined;
* user-level reading lifecycle.

Runtime Control owns execution authority associated with the current Runtime Revision.

Presentation consumes the necessary immutable context and current accepted Artifact references.

Presentation must not maintain a second authoritative reading-session registry.

---

# 50. Relationship with Runtime Control

Runtime Control owns:

```text
Revision
WorkItem
Attempt
Authority
Cancellation
Retry
Completion acceptance
Downstream eligibility
```

Presentation owns:

```text
Presentation semantic operation
Candidate Presentation State
PresentationRevision
Committed PresentationSnapshot
Committed RenderPlan
Presentation-local focus/selection
Presentation fallback semantics
```

Boundary:

```text
Runtime may authorize presentation work.

Presentation may produce a valid candidate.

Runtime authority must still be valid at commit.

Presentation commits only its own state.
```

---

# 51. Relationship with Artifact Store

Presentation consumes accepted immutable Artifact references.

Possible references:

```text
RecognitionArtifactRef
SourceDocumentArtifactRef
TranslationArtifactRef
```

Presentation must acquire and release Runtime resource leases according to Runtime resource policy when dereferencing retained Artifacts.

Presentation does not:

* mutate accepted Artifacts;
* publish upstream module Candidate Artifacts;
* manage Artifact retention policy globally;
* decide Artifact disposal.

---

# 52. Relationship with Recognition

Presentation may use Recognition Artifact information for:

* source region mapping;
* region geometry;
* source-region identifiers;
* confidence summaries;
* text orientation metadata.

Presentation does not:

* detect text;
* recognize characters;
* infer OCR reading order;
* rerun OCR quality evaluation;
* consume provider SDK output;
* consume Recognition Candidate Artifacts before publication.

---

# 53. Relationship with Text Processing

Presentation may consume SourceDocument Artifact structures such as:

* blocks;
* paragraphs;
* semantic roles;
* source segments;
* normalized text;
* canonical reading order.

Presentation does not:

* normalize OCR text;
* merge OCR lines semantically;
* reconstruct paragraphs;
* segment translation input;
* redefine SourceDocument.

---

# 54. Relationship with Translation

Translation owns:

* TranslationUnit;
* TranslationBatch;
* provider execution;
* glossary/context application;
* translation candidate generation;
* Translation Artifact semantics.

Presentation consumes accepted Translation Artifact references.

Presentation determines how translated states appear visually.

Presentation does not:

* call translation providers;
* retry translation;
* choose Translation provider;
* consume raw provider streams;
* modify Translation Artifact.

---

# 55. Relationship with Preferences

Preferences owns:

* persisted preference values;
* preference validation;
* preference resolution;
* preference versioning.

Presentation consumes an immutable resolved PresentationProfile.

A preference update may create a new PresentationRevision.

It does not necessarily create a new Runtime content Revision.

---

# 56. Relationship with UI Adapter

This boundary is fundamental.

```text
Presentation
    ↓
Committed PresentationSnapshot
+
Committed RenderPlan
    ↓
UI Adapter
    ↓
Framework / Platform UI
```

Presentation owns:

* semantic Presentation state;
* item identities;
* marker identities;
* framework-neutral layout;
* effective mode;
* overflow policy;
* fallback reason;
* PresentationRevision.

UI Adapter owns:

* component instances;
* widget lifecycle;
* DOM/view mutation;
* native surface lifecycle;
* UI-thread scheduling;
* native geometry application;
* input event handling;
* platform accessibility APIs;
* click-through;
* capture exclusion;
* always-on-top implementation.

Presentation must not import concrete UI Adapter implementations.

---

# 57. UI Commit

CRAI distinguishes:

```text
Presentation Commit
```

from:

```text
UI Apply / UI Commit
```

Presentation Commit means:

> a new PresentationSnapshot + RenderPlan pair became the current logical Presentation state.

UI apply means:

> the UI Adapter successfully applied that committed state to the actual visible UI surface.

A Presentation may therefore be logically committed but fail to become visible because the UI Adapter rejects or fails to apply it.

This failure must be observable.

---

# 58. UI Apply Feedback

UI Adapter should be capable of returning normalized apply feedback.

Conceptually:

```text
PresentationApplyResult
├── presentationId
├── presentationRevision
├── targetId
├── status
├── appliedAt?
├── rejectionReason?
└── diagnostics?
```

Possible statuses:

```text
APPLIED
REJECTED_STALE
REJECTED_TARGET_MISMATCH
FAILED
TARGET_UNAVAILABLE
```

Presentation must not depend on framework-specific error types.

---

# 59. Presentation Runtime

The application may host a Presentation Runtime responsible for coordinating:

* current/previous Presentation references;
* Presentation commit serialization;
* authority revalidation bridge;
* UI apply bridge;
* presentation retention;
* coalescing layout changes.

This does not make Presentation Runtime a replacement for global Runtime Control.

Conceptual separation:

```text
Runtime Control
    → global execution authority

Presentation Runtime
    → presentation-specific commit and display coordination
```

---

# 60. Presentation Retention

Presentation may retain:

```text
Current Presentation
Previous Presentation
In-flight Candidate Presentation
```

according to bounded policy.

Typical purpose:

* atomic replacement;
* rollback when safe;
* avoiding flicker;
* comparison during incremental update.

Presentation must not become long-term history storage.

Durable history belongs to Storage capability.

---

# 61. Resource Lifetime

Presentation resource categories include:

```text
Presentation semantic objects
RenderPlan
Temporary layout structures
Typography measurements
Geometry projections
Artifact leases
UI Presentation references
```

Temporary preparation resources belong to the operation preparing the candidate.

Committed Presentation state belongs to Presentation for its display lifetime.

Artifact payload ownership remains with Artifact Store.

Native UI resource ownership remains with UI Adapter/platform.

---

# 62. Dependency Rules

Presentation may depend on stable contracts from:

```text
core contracts
runtime execution contracts
runtime Artifact references
reading contracts
recognition Artifact contracts
source-document contracts
translation Artifact contracts
preferences contracts
geometry primitives
diagnostics abstractions
UI Adapter port abstractions where required
```

Presentation must not directly depend on:

```text
OCR provider implementation
Translation provider implementation
Capture implementation
Storage backend implementation
Event Bus implementation
Scheduler implementation
Native UI toolkit
Browser extension implementation
Operating-system APIs
Concrete UI Adapter implementation
```

---

# 63. Dependency Direction

Conceptually:

```text
UI Adapter
    ↓
Presentation public contracts / binding interface

Presentation
    ↓
stable semantic + runtime abstractions
```

Concrete implementations are wired at Composition Root.

Presentation must not introduce reverse imports from business logic into platform UI implementations.

---

# 64. Event Usage

Presentation events describe committed Presentation facts or meaningful Presentation-local rejection/failure.

Events must not become hidden workflow orchestration.

Typical successful facts may include:

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
PresentationCleared
```

Typical abnormal facts may include:

```text
PresentationRejected
PresentationFailed
```

Detailed event schemas belong in `EVENTS.md`.

---

# 65. Event Correctness Rule

A success event must describe committed state.

Therefore:

```text
Prepare candidate
    ↓
Validate
    ↓
Commit
    ↓
Publish Presentation success fact
```

Never:

```text
Publish success
    ↓
Attempt commit
```

A candidate that is later discarded must not produce a committed-success event.

---

# 66. Consumed Events

Presentation must not rely on broad Event Bus subscriptions as the primary correctness mechanism.

Application/Runtime orchestration should invoke Presentation through explicit commands/contracts.

Presentation may observe normalized facts for convenience where architecture permits, for example:

* viewport changed;
* preference changed;
* target capability changed.

But downstream correctness must not depend on:

```text
TranslationCompleted event
    → Presentation secretly starts itself
```

Business Pipeline Orchestration and Runtime determine required work.

---

# 67. Public Commands

Conceptual Presentation commands:

```text
BuildPresentation
UpdatePresentationContent
RecomputePresentationLayout
ChangePresentationMode
UpdatePresentationFocus
ClearPresentation
```

Detailed schemas belong in `CONTRACT.md`.

Commands must carry explicit context and serializable values/references.

---

# 68. Public Queries

Possible Presentation queries:

```text
GetCurrentPresentation
GetPresentationSnapshot
GetEffectivePresentationMode
GetPresentationIssues
GetPresentationCapabilities
```

Queries return immutable snapshots or references.

They must not expose mutable internal state.

---

# 69. Error Ownership

Presentation errors describe Presentation failures only.

Possible categories:

```text
INVALID_INPUT
INCOMPATIBLE_ARTIFACT
UNSUPPORTED_MODE
INVALID_GEOMETRY
STALE_PRESENTATION_OPERATION
TARGET_CAPABILITY_MISMATCH
LAYOUT_FAILED
OVERFLOW_DEGRADED
COMMIT_REJECTED
UI_APPLY_REJECTED
CANCELLED_OPERATION
INVARIANT_VIOLATION
```

Runtime terminal execution failure remains Runtime-owned.

Detailed error taxonomy belongs in `ERRORS.md`.

---

# 70. Expected Non-Failures

The following are not automatically user-visible failures:

* obsolete candidate;
* superseded reflow;
* coalesced viewport update;
* cancellation caused by newer content;
* unsupported overlay with successful Side Panel fallback;
* stale candidate discarded before commit.

These should generally be represented as expected control outcomes or diagnostics.

---

# 71. Diagnostics

Presentation diagnostics should be structured and content-safe.

Typical fields:

```text
SessionId
RevisionId
WorkItemId?
AttemptId?
PresentationContextId
PresentationId
PresentationRevision
OperationId
OperationType
EffectiveMode
TargetKind
TargetRevision
ViewportRevision
ItemCount
MarkerCount
OverflowCount
HiddenItemCount
FallbackReason
PreparationDurationMs
CommitDurationMs
UiApplyDurationMs?
IssueCode
```

---

# 72. Privacy

Presentation may temporarily process:

* source text;
* translated text;
* source geometry;
* accepted corrections;
* presentation preferences.

Normal diagnostics must not include:

* full screenshots;
* full source text;
* full translated text;
* provider prompts;
* provider responses;
* credentials;
* private window titles.

Presentation must not persist content automatically.

---

# 73. Performance Model

Presentation performance should optimize for useful visible output.

Important metrics include:

```text
accepted-artifact-to-candidate latency
candidate preparation latency
authority revalidation latency
presentation commit latency
UI apply latency
reflow latency
incremental update latency
marker generation latency
fallback rate
stale candidate count
coalesced reflow count
active presentation memory
```

A fast stale layout result has no user value.

---

# 74. Coalescing

High-frequency updates may be coalesced.

Typical candidates:

* viewport resize;
* scrolling;
* target movement;
* zoom changes;
* repeated focus movement.

Example:

```text
Viewport 20
Viewport 21
Viewport 22
    ↓
Coalesce
    ↓
Prepare layout for 22
```

Presentation must not spend significant work committing obsolete intermediate layouts.

---

# 75. Incremental Rebuild

A single translated segment update should not rebuild unrelated items if semantic grouping remains unchanged.

Prefer:

```text
Changed semantic input
    ↓
Affected PresentationItems
    ↓
Affected layout region
    ↓
New Presentation revision
```

over:

```text
Rebuild everything unconditionally
```

unless the active strategy genuinely requires full recomputation.

---

# 76. Accessibility

Presentation models should expose enough semantic information for accessible UI rendering.

Examples:

* deterministic focus order;
* semantic roles;
* source/translation distinction;
* status message keys;
* issue message keys;
* action labels;
* marker-to-item relation;
* non-color-only state representation.

Actual accessibility API calls belong to UI Adapter/platform.

---

# 77. Non-Destructive MVP

MVP Presentation does not modify original reading media.

It does not:

* erase original text;
* inpaint artwork;
* burn translation into images;
* rewrite source image files;
* permanently replace speech-bubble text.

Overlay is a separate render surface.

---

# 78. Conceptual Internal Components

Logical responsibilities may include:

```text
Presentation Module
├── Presentation Coordinator
├── Input Resolver
├── Candidate Builder
├── Snapshot Builder
├── Item Mapper
├── Mode Resolver
├── Strategy Registry
│   ├── Side Panel Strategy
│   ├── Text Reader Strategy
│   ├── Overlay Strategy
│   └── Hybrid Strategy
├── Render Plan Builder
├── Geometry Projector
├── Typography Policy
├── Overflow Policy
├── Progressive Update Policy
├── Presentation Validator
├── Commit Coordinator
├── Presentation Retention
└── Presentation Diagnostics
```

These names describe responsibility, not mandatory source folders.

---

# 79. Presentation Coordinator

Presentation Coordinator coordinates module-owned semantic work.

It may:

* validate Presentation input;
* resolve required Artifact references;
* invoke item mapping;
* choose strategy;
* build candidate snapshot;
* build candidate RenderPlan;
* validate candidate invariants;
* submit candidate for commit.

It must not:

* create Runtime WorkItems;
* perform Runtime retries;
* change Runtime Revision authority;
* schedule arbitrary downstream work.

---

# 80. Commit Coordinator

The Commit Coordinator protects Presentation state from incompatible concurrent updates.

It conceptually verifies:

```text
presentation context still matches
+
expected PresentationRevision still matches
+
candidate is internally valid
+
Runtime authority revalidation succeeds
+
target remains compatible
    ↓
atomic commit
```

Commit serialization may be per Presentation Context.

---

# 81. Candidate Validation

Before commit, validate at least:

* required identities present;
* source Artifact references valid;
* Artifact types compatible;
* item identifiers unique;
* marker identifiers unique;
* item mappings valid;
* sequence/order valid;
* RenderPlan references valid items;
* marker layouts reference valid markers;
* geometry valid;
* target capability requirements satisfied;
* snapshot and RenderPlan share candidate PresentationRevision;
* candidate does not contain framework/native objects.

---

# 82. Determinism

For fixed equivalent inputs:

```text
accepted Artifacts
PresentationProfile
PresentationTarget
ViewportSnapshot
strategy version
```

Presentation should produce deterministically equivalent:

```text
semantic ordering
item mapping
mode decision
fallback decision
layout classification
```

Exact floating-point output may require tolerance rules.

---

# 83. Testing Strategy

Presentation must be testable without:

* OCR provider;
* translation provider;
* native desktop window;
* browser DOM;
* operating-system APIs.

---

# 84. Unit Tests

Test:

* Presentation input validation;
* item mapping;
* stable identity;
* source/translation traceability;
* mode resolution;
* strategy fallback;
* marker identity;
* geometry validation;
* overflow classification;
* typography thresholds;
* partial updates;
* correction precedence;
* focus/selection transitions;
* stale candidate rejection;
* revision mismatch;
* deterministic layout decisions;
* clear behavior.

---

# 85. Runtime Integration Tests

Verify:

* Presentation never changes Runtime WorkItem state;
* Presentation never changes Attempt state;
* Presentation never grants Runtime authority;
* superseded Runtime Revision cannot commit Presentation;
* canceled work cannot replace current Presentation;
* newer Presentation operation cannot be overwritten by older completion;
* runtime cancellation is cooperatively observed;
* Artifact leases are released correctly.

---

# 86. Contract Tests

Verify compatibility with:

* Runtime execution context;
* Artifact references;
* Recognition Artifact;
* SourceDocument Artifact;
* Translation Artifact;
* PresentationProfile;
* PresentationTarget;
* UI Adapter RenderPlan/application contract;
* canonical event envelope.

---

# 87. Golden Model Tests

Given fixed inputs, verify deterministic serialized logical outputs.

Fixtures should include:

* horizontal comic dialogue;
* vertical Chinese text;
* mixed text directions;
* long Vietnamese translation;
* missing translation;
* partial translation;
* corrected translation;
* viewport resize;
* zoom;
* invalid overlay geometry;
* overlay-to-Side-Panel fallback;
* structured novel paragraph;
* source-only content.

---

# 88. UI Adapter Integration Tests

Test:

```text
Committed Presentation
    ↓
UI Adapter applies exact revision
```

including:

* stale UI apply rejection;
* target replacement;
* surface unavailable;
* retry-safe application where supported;
* framework-independent error normalization;
* clear semantics;
* PresentationRevision ordering.

Visual regression remains primarily a UI Adapter responsibility.

---

# 89. Concurrency Tests

Test races between:

* translation update and viewport change;
* preference change and reflow;
* content replacement and old Presentation completion;
* Session stop and candidate commit;
* rapid focus updates;
* target replacement and UI apply;
* multiple reflow requests.

The result must always converge on the newest valid committed state.

---

# 90. MVP Scope

Required:

```text
Side Panel
Ordered PresentationItems
Stable source/translation traceability
Lightweight region markers
Partial translation representation
Translation failure representation
Stable item identity
PresentationRevision
Runtime authority-aware commit
Basic PresentationProfile
Basic PresentationTarget
ViewportSnapshot
Overflow-safe text wrapping
Presentation diagnostics
Clear on content/session invalidation
UI Adapter boundary
```

---

# 91. Optional MVP / Near-Term

May be added once core Side Panel behavior is stable:

```text
Text Reader
Focused source markers
Simple bilingual mode
Limited Overlay prototype
Basic accessibility semantics
Basic focus synchronization
```

---

# 92. Deferred Scope

Deferred until prototypes demonstrate value:

* source-image text removal;
* inpainting;
* permanent translated-image rendering;
* artwork-aware text replacement;
* curved text layout;
* automatic source font matching;
* advanced multi-monitor overlay optimization;
* animated presentation transitions;
* plugin-provided strategies;
* user-created themes;
* print layout;
* complete browser in-page rewriting;
* synchronized advanced bilingual novel reader.

---

# 93. Open Decisions

## 93.1 Presentation Runtime Placement

Determine final implementation ownership of:

```text
current Presentation registry
commit serialization
previous Presentation retention
UI apply coordination
```

The architecture requires the semantics but not yet a fixed source folder layout.

## 93.2 Side Panel Surface

Determine:

* embedded vs detachable;
* always-on-top policy;
* source-window following;
* position persistence.

These primarily affect UI Adapter/Application Shell.

## 93.3 Marker Policy

Determine:

* always visible vs focus-only;
* numeric vs outline marker;
* density limits;
* obstruction thresholds.

## 93.4 Overlay Policy

Determine:

* all-item vs focused-only;
* automatic fallback threshold;
* user-adjustable placement;
* scroll behavior;
* required capture-exclusion capability.

## 93.5 Partial Presentation

Determine:

* minimum completeness required for first useful display;
* segment-level publication threshold;
* whether untranslated source placeholders are shown;
* whether token-level updates are ever allowed.

## 93.6 UI Apply Failure

Determine whether a failed UI apply:

* retains logical committed Presentation and retries apply;
* rolls back to previous Presentation;
* invalidates target only;
* triggers Presentation degradation.

This must be resolved before implementing complex overlay behavior.

---

# 94. Architecture Invariants

1. Presentation does not perform Capture, Recognition, Text Processing, or Translation.

2. Presentation consumes accepted immutable Artifact references, not raw provider output.

3. Presentation does not own Runtime Revision authority.

4. Presentation does not own WorkItem or Attempt lifecycle.

5. Presentation does not own Runtime retry or cancellation authority.

6. Presentation owns Presentation semantic state and PresentationRevision.

7. `Runtime RevisionId` and `PresentationRevision` are distinct concepts.

8. Prepared Presentation state is not committed Presentation state.

9. Runtime authority must still permit a candidate at commit time.

10. A stale candidate never overwrites a newer committed Presentation.

11. `PresentationSnapshot` and `RenderPlan` commit as one coherent revision.

12. Presentation does not expose mutable internal state.

13. Every visible item retains explicit upstream traceability where upstream identifiers exist.

14. Array position is not canonical identity.

15. Presentation does not silently redefine semantic reading order.

16. Coordinate spaces are explicit at public boundaries.

17. Readability takes precedence over forcing translation into unsuitable source bounds.

18. Presentation preserves stable item identity when semantic grouping is unchanged.

19. Presentation keeps previous valid committed state during recoverable preparation failure where safe.

20. Presentation does not persist reading history.

21. Presentation does not own native widgets, DOM nodes, windows, or platform handles.

22. UI Adapter owns actual framework/native rendering.

23. UI Adapter apply and Presentation logical commit are distinct lifecycle points.

24. Presentation core does not depend on concrete UI Adapter implementations.

25. Presentation does not use Event Bus subscriptions as hidden workflow orchestration.

26. Presentation success events describe already committed state.

27. Expected stale/superseded operations are not automatically failures.

28. Side Panel is the default readable MVP fallback.

29. MVP Presentation is non-destructive.

30. Standard diagnostics do not contain full reading content.

---

# 95. Example — Initial Comic Presentation

```text
Current Runtime Revision
        ↓
Published Recognition Artifact
        ↓
Published SourceDocument Artifact
        ↓
Published Translation Artifact
        ↓
Runtime schedules / invokes Presentation work
        ↓
Presentation resolves inputs
        ↓
Stable PresentationItems created
        ↓
Side Panel strategy selected
        ↓
Markers created from source-region lineage
        ↓
RenderPlan prepared
        ↓
Candidate Presentation State validated
        ↓
Runtime authority revalidated
        ↓
Presentation committed
        ↓
PresentationPrepared fact
        ↓
UI Adapter applies committed revision
        ↓
User sees translated panel
```

---

# 96. Example — Partial Translation Update

```text
Presentation Revision 7 committed
        ↓
New accepted Translation Artifact becomes available
        ↓
Presentation update requested
        ↓
Only affected items remapped
        ↓
Candidate Presentation Revision 8
        ↓
Runtime authority valid
        ↓
Atomic commit Revision 8
        ↓
PresentationUpdated
        ↓
UI Adapter applies Revision 8
```

Unchanged items preserve identity.

---

# 97. Example — Viewport Change

```text
Viewport Revision 20
Viewport Revision 21
Viewport Revision 22
        ↓
Obsolete reflow requests coalesced
        ↓
Reflow using Viewport Revision 22
        ↓
Candidate RenderPlan
        ↓
Presentation context + authority revalidated
        ↓
Presentation Revision N+1 committed
        ↓
PresentationLayoutChanged
        ↓
UI Adapter applies newest RenderPlan
```

No committed-success fact is required for viewport revisions 20 and 21.

---

# 98. Example — Stale Runtime Result

```text
Runtime Revision 14 active
        ↓
Presentation candidate begins
        ↓
Runtime Revision 15 becomes current
        ↓
Revision 14 loses commit authority
        ↓
Old candidate finishes physical preparation
        ↓
Authority revalidation fails
        ↓
Candidate discarded
        ↓
Current Presentation unchanged
```

Presentation does not mark Runtime Revision 14 itself as superseded.

Runtime already owns that fact.

---

# 99. Example — Overlay Fallback

```text
Requested Mode = OVERLAY
        ↓
Geometry / capability validation
        ↓
Overlay cannot satisfy readability threshold
        ↓
Fallback Policy
        ↓
Effective Mode = SIDE_PANEL
        ↓
Candidate records fallback reason
        ↓
Commit
        ↓
Readable Side Panel remains available
```

Fallback is successful degraded behavior, not necessarily failure.

---

# 100. Example — Session Stop

```text
Session stop initiated
        ↓
Runtime authority revoked / cancellation propagated
        ↓
In-flight Presentation candidates lose commit eligibility
        ↓
Presentation current state logically cleared
        ↓
PresentationCleared
        ↓
UI Adapter removes visible binding/surface content
        ↓
Presentation resources released according to lifetime policy
```

Native window destruction remains outside Presentation.

---

# 101. Recommended Implementation Order

```text
1. Presentation identifiers and PresentationContext
2. Runtime-v2 execution boundary
3. PresentationSnapshot
4. PresentationItem
5. RenderPlan
6. PresentationRevision
7. Candidate Presentation State
8. Presentation commit semantics
9. Source/translation traceability
10. Side Panel strategy
11. Stable marker model
12. Runtime authority revalidation bridge
13. Partial translation updates
14. PresentationTarget + ViewportSnapshot
15. Geometry projection
16. Overflow/readability policy
17. UI Adapter application contract
18. Focus/selection
19. Diagnostics
20. Text Reader
21. Limited Overlay prototype
```

Do not implement complex Overlay before:

* commit authority;
* stable PresentationRevision;
* viewport revision handling;
* stale candidate rejection;
* Side Panel fallback

are working correctly.

---

# 102. Completion Criteria

The Presentation module is architecturally usable when:

* published current source/translation Artifacts can produce a deterministic Presentation;
* every visible item can be traced to accepted upstream identifiers;
* Side Panel can render long Vietnamese text readably;
* stable item and marker identities survive incremental updates;
* PresentationRevision is separate from Runtime RevisionId;
* Presentation candidate state is separated from committed state;
* obsolete Runtime work cannot commit Presentation;
* stale viewport/layout work cannot replace newer Presentation;
* PresentationSnapshot and RenderPlan commit atomically;
* previous valid Presentation survives recoverable candidate failure where safe;
* Presentation can clear correctly on session/content invalidation;
* UI Adapter applies Presentation through a framework-neutral contract;
* Presentation does not own native UI resources;
* Presentation does not own Runtime WorkItem/Attempt/authority;
* Presentation is testable without OCR/translation providers;
* Presentation is testable without native windows or browser APIs;
* diagnostics explain fallback, rejection, commit, and UI apply behavior;
* normal diagnostics remain content-safe.

---

# 103. Related Documents

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
doc/01-architecture/modules/OWNERSHIP_MAP.md

doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/MEMORY_MODEL.md
doc/01-architecture/runtime/PERFORMANCE_MODEL.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/01-architecture/translate/PRESENTATION.md

doc/02-modules/presentation/CONTRACT.md
doc/02-modules/presentation/STATES.md
doc/02-modules/presentation/EVENTS.md
doc/02-modules/presentation/ERRORS.md
doc/02-modules/presentation/README.md

doc/02-modules/recognition/MODULE.md
doc/02-modules/text-processing/MODULE.md
doc/02-modules/translation/MODULE.md
doc/02-modules/reading-session/MODULE.md
doc/02-modules/preferences/MODULE.md
doc/02-modules/diagnostics/MODULE.md
doc/02-modules/ui-adapter/MODULE.md
doc/02-modules/storage/MODULE.md
```

---

# 104. Documentation Ownership

This file defines:

* Presentation module identity;
* module purpose;
* architectural position;
* semantic ownership;
* Runtime boundary;
* Artifact boundary;
* Presentation commit model;
* PresentationSnapshot and RenderPlan roles;
* strategy responsibilities;
* UI Adapter boundary;
* invariants;
* MVP scope;
* deferred scope.

Detailed public schemas belong to:

```text
CONTRACT.md
```

Detailed Presentation-owned state transitions belong to:

```text
STATES.md
```

Detailed event definitions belong to:

```text
EVENTS.md
```

Detailed error codes and recovery semantics belong to:

```text
ERRORS.md
```

Native rendering behavior belongs to:

```text
ui-adapter
platform implementations
```

Runtime authority and WorkItem/Attempt lifecycle belong to:

```text
Runtime Control
Pipeline Runtime
```

Artifact publication and accepted Artifact lifetime belong to:

```text
Runtime Artifact Store
```

---

# 105. Summary

Presentation is CRAI's semantic user-visible presentation boundary.

Its core flow is:

```text
Accepted Runtime Artifacts
        ↓
Presentation semantic mapping
        ↓
Presentation strategy
        ↓
Candidate PresentationSnapshot
        +
Candidate RenderPlan
        ↓
Validation
        ↓
Runtime authority revalidation
        ↓
Atomic Presentation Commit
        ↓
Committed PresentationRevision
        ↓
UI Adapter
        ↓
Visible Reading Experience
```

The critical ownership model is:

```text
Runtime Control
    owns execution authority

Artifact Store
    owns accepted runtime Artifacts

Presentation
    owns committed semantic Presentation state

UI Adapter / Platform
    owns actual rendering resources
```

The most important invariant is:

```text
Presentation may prepare what should be shown.

Only still-authoritative work may commit it.

Only UI Adapter may turn the committed plan into actual framework/native UI.
```
