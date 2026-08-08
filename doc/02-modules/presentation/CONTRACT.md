# Presentation Contract

> **Project:** CRAI
> **Module:** `presentation`
> **Path:** `doc/02-modules/presentation/CONTRACT.md`
> **Contract Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-08

---

# 1. Purpose

This document defines the public contract boundary of the Presentation module.

Presentation transforms compatible accepted Runtime Artifacts and presentation context into a framework-neutral user-visible Presentation model.

The primary semantic outputs are:

```text
PresentationSnapshot
+
RenderPlan
```

The module operates through a candidate-and-commit model:

```text
Accepted Artifact References
        +
Presentation Context
        +
Presentation Profile
        +
Presentation Target
        +
Viewport Snapshot
        ↓
Presentation Operation
        ↓
Candidate Presentation State
        ↓
Authority Revalidation
        ↓
Presentation Commit
        ↓
Committed PresentationSnapshot
        +
Committed RenderPlan
```

Presentation does not:

* perform OCR;
* perform Text Processing;
* execute Translation;
* own Runtime Revision authority;
* own WorkItem or Attempt lifecycle;
* publish upstream business Artifacts;
* create native windows;
* manipulate DOM nodes;
* own UI framework objects;
* process raw operating-system input.

This contract exists so that:

* Runtime/Application code can invoke Presentation without importing Presentation internals;
* Presentation can consume immutable accepted Artifact references;
* stale or superseded candidate work cannot replace newer committed Presentation state;
* `ui-adapter` can consume Presentation output without knowing Presentation internals;
* contracts remain serializable, versionable, provider-independent, platform-independent, and testable.

---

# 2. Contract Scope

This file owns public definitions for:

```text
Presentation identifiers
Presentation context
Presentation operation input
Presentation candidate state
Presentation commit contracts
PresentationSnapshot
PresentationItem
PresentationMarker
RenderPlan
PresentationTarget
ViewportSnapshot
PresentationProfile
Presentation commands
Presentation queries
Presentation command results
UI apply feedback
validation rules
revision semantics
compatibility/versioning
```

This file does not define:

* Presentation internal algorithms;
* concrete layout engines;
* internal source folders;
* Runtime WorkItem/Attempt lifecycle;
* Runtime authority implementation;
* Artifact Store implementation;
* Event Bus transport;
* full Presentation state transitions;
* full event catalog;
* full error catalog;
* persistence schemas;
* concrete UI framework bindings.

Those concerns belong to:

```text
doc/02-modules/presentation/MODULE.md
doc/02-modules/presentation/STATES.md
doc/02-modules/presentation/EVENTS.md
doc/02-modules/presentation/ERRORS.md

doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md

doc/02-modules/ui-adapter/CONTRACT.md
```

---

# 3. Architectural Boundary

The intended flow is:

```text
Published Runtime Artifacts
        ↓
Runtime / Application
        ↓
Presentation public command
        ↓
presentation
        ↓
Candidate Presentation State
        ↓
authority revalidation
        ↓
Presentation commit
        ↓
PresentationSnapshot + RenderPlan
        ↓
ui-adapter
        ↓
framework / platform UI
```

Presentation owns:

* `PresentationId`;
* `PresentationContextId`;
* `PresentationItemId`;
* `MarkerId`;
* `PresentationRevision`;
* Presentation semantic grouping;
* requested/effective Presentation mode semantics;
* Candidate Presentation construction;
* committed `PresentationSnapshot`;
* committed `RenderPlan`;
* framework-neutral layout planning;
* Presentation fallback decisions;
* Presentation-local focus and selection state;
* Presentation candidate validation;
* Presentation commit semantics.

Presentation does not own:

* Runtime `RevisionId`;
* Runtime `WorkItemId`;
* Runtime `AttemptId`;
* Runtime authority;
* scheduler state;
* retry lifecycle;
* global cancellation authority;
* upstream Artifact publication;
* Reading Session lifecycle;
* Translation Artifact lifecycle;
* native rendering resource lifecycle;
* persistent preference storage;
* durable reading history.

---

# 4. Contract Principles

## 4.1 Serializable Boundary

All public contract values MUST be serializable.

Public payloads MUST NOT contain:

* UI framework objects;
* DOM nodes;
* native handles;
* database connections;
* provider clients;
* SDK-specific response objects;
* mutable module-internal entities;
* executable callbacks;
* thread-affine UI objects.

## 4.2 Immutability

Published or committed public values are immutable.

This includes:

```text
PresentationSnapshot
RenderPlan
PresentationProfile
PresentationTarget
ViewportSnapshot
CandidatePresentationState
```

A new visible state creates a new `PresentationRevision`.

Existing committed objects are never mutated in place.

## 4.3 Stable References

Cross-module contracts SHOULD use:

```text
ArtifactRef
Identifier
Immutable bounded value
```

rather than copying mutable aggregates across module boundaries.

## 4.4 Explicit Ownership

Every public field must have one semantic owner.

A field appearing inside a Presentation contract does not imply Presentation owns its lifecycle.

## 4.5 No Hidden Runtime

Presentation contracts MUST NOT recreate:

* WorkItem state;
* Attempt state;
* retry state;
* scheduler state;
* global Revision registry;
* competing Runtime authority state.

---

# 5. Naming Convention

Public type names use PascalCase.

Public field names are shown in camelCase.

Examples:

```text
BuildPresentation
CandidatePresentationState
presentationContextId
presentationRevision
runtimeRevisionId
```

Concrete implementations may adapt naming to language conventions while preserving semantic meaning.

---

# 6. Shared Runtime Identity

Presentation work executed through Runtime may carry:

```text
RuntimeExecutionIdentity
├── sessionId
├── runtimeRevisionId
├── workItemId?
├── attemptId?
├── correlationId?
├── causationId?
└── configurationSnapshotRef?
```

Rules:

1. `runtimeRevisionId` identifies Runtime execution intent, not Presentation output revision.
2. Presentation MUST NOT generate Runtime IDs.
3. Presence of Runtime IDs does not grant Presentation authority ownership.
4. Presentation MAY use them for:

   * trace correlation;
   * validation;
   * diagnostics;
   * commit revalidation requests.
5. Runtime Control remains authoritative for current Runtime Revision relevance.

---

# 7. Cancellation Context

Presentation may receive a Runtime-controlled cancellation reference.

```text
CancellationContextRef
- cancellationContextId
- runtimeRevisionId
- workItemId?
- attemptId?
```

The reference allows Presentation to cooperate with cancellation.

Presentation MUST NOT:

* mark Runtime work canceled;
* create a new cancellation state;
* determine terminal Attempt outcome.

Cancellation observation is not cancellation authority.

---

# 8. PresentationContextId

Identifies one logical presentation scope.

```text
PresentationContextId
- value
```

Examples:

```text
main-reader
comic-panel
text-reader
focused-overlay
```

Rules:

* stable while the logical display context exists;
* independent from native surface identity;
* MAY survive multiple Presentation revisions;
* MUST NOT be inferred from widget identity;
* MUST NOT be reused simultaneously for unrelated active contexts.

---

# 9. PresentationId

Identifies one logical Presentation lineage.

```text
PresentationId
- value
```

Rules:

* stable across compatible incremental updates;
* stable across layout-only revisions when semantic Presentation identity is unchanged;
* replaced when a new logical Presentation is created;
* not derived from array position;
* not equal to Runtime Revision identity.

---

# 10. PresentationOperationId

Identifies one Presentation-owned semantic operation.

```text
PresentationOperationId
- value
```

It may be used for:

* diagnostics;
* local operation tracking;
* candidate correlation;
* commit diagnostics.

It is not:

```text
WorkItemId
AttemptId
Runtime operation authority
```

---

# 11. PresentationRequestId

Correlates a public command with its immediate response.

```text
PresentationRequestId
- value
```

Rules:

* SHOULD be unique within the active application instance;
* MAY support duplicate command detection;
* MUST NOT be used as Presentation identity.

---

# 12. PresentationItemId

Identifies one semantic visible unit.

```text
PresentationItemId
- value
```

Rules:

* stable across layout recomputation;
* stable across PresentationProfile changes where semantic grouping remains unchanged;
* stable across compatible partial translation updates;
* replaced only when semantic grouping genuinely changes;
* never derived solely from array index.

---

# 13. MarkerId

Identifies one Presentation marker.

```text
MarkerId
- value
```

A marker remains stable while its Presentation-item/source association remains semantically stable.

---

# 14. PresentationRevision

Represents the committed visible Presentation version.

```text
PresentationRevision
- value: monotonic non-negative integer or equivalent monotonic token
```

Rules:

1. scoped to one `PresentationContextId` or Presentation lineage;
2. MUST increase when committed visible Presentation state changes;
3. MUST NOT decrease;
4. older revisions MUST NOT overwrite newer committed revisions;
5. layout-only changes MAY create a new PresentationRevision;
6. focus/selection changes MAY create a new PresentationRevision;
7. PresentationRevision is distinct from Runtime Revision.

---

# 15. Runtime Revision vs Presentation Revision

These identifiers serve different purposes.

```text
Runtime RevisionId
    → whether processing intent is still current

PresentationRevision
    → which user-visible Presentation state is current
```

Example:

```text
Runtime Revision 42
    ↓
Presentation Revision 1
    ↓
viewport changes
    ↓
Presentation Revision 2
    ↓
focus changes
    ↓
Presentation Revision 3
```

A new PresentationRevision MUST NOT be interpreted as a new Runtime Revision.

---

# 16. ArtifactRef

Presentation consumes accepted immutable Runtime Artifact references.

Conceptually:

```text
ArtifactRef
├── artifactId
├── artifactType
├── contractVersion
├── contentIdentity
├── compatibilityMetadata?
└── owner?
```

Typical accepted input types include:

```text
RECOGNITION_ARTIFACT
SOURCE_DOCUMENT_ARTIFACT
TRANSLATION_ARTIFACT
```

Presentation MUST NOT consume an upstream Candidate Artifact through this contract unless an explicitly separate candidate-only diagnostic contract exists.

---

# 17. PresentationInputArtifactSet

```text
PresentationInputArtifactSet
├── recognitionArtifactRef?
├── sourceDocumentArtifactRef?
├── translationArtifactRef?
└── auxiliaryArtifactRefs[]?
```

Rules:

* required artifacts depend on requested Presentation capability;
* all supplied references MUST be compatible with one logical content lineage;
* Presentation MUST NOT mutate referenced Artifacts;
* missing optional Artifact types MUST have explicit semantic behavior;
* Artifact reuse is governed by compatibility semantics, not merely matching Runtime Revision IDs.

---

# 18. ContentIdentity

Presentation should not depend solely on a mutable application content counter.

Use a bounded semantic identity structure where possible.

```text
ContentIdentity
├── contentId
├── sourceId?
├── sourceVersion?
├── documentId?
├── chapterId?
├── pageId?
├── semanticFingerprint?
└── lineageMetadata?
```

The exact fields depend on upstream contracts.

Presentation uses ContentIdentity to establish semantic compatibility.

Runtime Revision remains separate.

---

# 19. PresentationMode

```text
PresentationMode
- SidePanel
- TextReader
- Overlay
- Hybrid
- Unknown
```

Rules:

* requested and effective mode are separate;
* unsupported mode MUST NOT be silently interpreted;
* fallback MAY resolve requested mode to another mode;
* fallback reason MUST be observable;
* unknown future values must follow version compatibility policy.

---

# 20. PresentationTarget

Describes the logical destination surface.

```text
PresentationTarget
├── targetId
├── targetKind
├── targetRevision
├── capabilities[]
├── bounds?
├── coordinateSpace
├── scale?
├── safeInsets?
└── sourceAssociation?
```

Possible `targetKind` values:

```text
MainWindow
CompanionPanel
FloatingSurface
OverlaySurface
BrowserSurface
Unknown
```

A PresentationTarget is not a native surface resource.

---

# 21. Target Capabilities

Possible capability values include:

```text
SupportsScrolling
SupportsOverlay
SupportsMarkers
SupportsPointerFocus
SupportsKeyboardFocus
SupportsTextSelection
SupportsDynamicResize
SupportsBilingualLayout
SupportsCaptureExclusion
SupportsTransparency
SupportsAlwaysOnTop
```

Capabilities describe normalized logical ability.

Presentation must not query operating-system APIs directly to obtain them.

---

# 22. ViewportSnapshot

Describes one immutable normalized viewport observation.

```text
ViewportSnapshot
├── viewportId
├── viewportRevision
├── targetId
├── targetRevision
├── width
├── height
├── scale
├── zoom?
├── scrollOffset?
├── visibleBounds?
├── coordinateSpace
├── transforms[]
├── safeInsets?
└── capturedAt?
```

Rules:

* numeric values MUST be finite;
* width/height MUST be non-negative;
* coordinate space is mandatory;
* viewport revisions SHOULD be monotonic per target;
* stale viewport work MUST NOT overwrite newer committed layout.

---

# 23. PresentationProfile

An immutable resolved preference snapshot.

```text
PresentationProfile
├── profileId?
├── profileVersion?
├── preferredMode
├── typography
├── panelConfiguration?
├── readerConfiguration?
├── sourceVisibility
├── markerPolicy
├── overlayPolicy
├── fallbackPolicy
├── themeSemantics
└── accessibilityPreferences?
```

Presentation consumes but does not persist the profile.

---

# 24. TypographyProfile

```text
TypographyProfile
├── fontFamily?
├── fontSize
├── minimumReadableFontSize
├── lineHeight
├── paragraphSpacing
├── alignment?
├── wrappingPolicy?
└── emphasisPolicy?
```

Rules:

* font sizes MUST be finite and positive;
* minimum readable size MUST NOT exceed configured maximum where a maximum exists;
* platform-specific font objects MUST NOT appear here.

---

# 25. CoordinateSpace

```text
CoordinateSpace
- SourceImage
- CapturedFrame
- NormalizedSource
- ApplicationViewport
- BrowserViewport
- Screen
- OverlaySurface
- Unknown
```

Geometry crossing a public boundary MUST declare its coordinate space.

---

# 26. Point

```text
Point
├── x
├── y
└── coordinateSpace
```

Values MUST be finite.

---

# 27. Size

```text
Size
├── width
└── height
```

Values MUST be finite and non-negative.

---

# 28. Rect

```text
Rect
├── x
├── y
├── width
├── height
└── coordinateSpace
```

Values MUST be finite.

Width and height MUST be non-negative.

---

# 29. GeometryTransform

```text
GeometryTransform
├── transformId?
├── transformVersion?
├── fromSpace
├── toSpace
├── matrix?
├── scaleX?
├── scaleY?
├── offsetX?
├── offsetY?
├── rotation?
└── sourceGeometryVersion?
```

A conversion between coordinate spaces requires an explicit compatible transform.

Presentation MUST NOT call native platform geometry APIs directly.

---

# 30. SourceRegionRef

A Presentation-facing reference to accepted region semantics.

```text
SourceRegionRef
├── regionId
├── recognitionArtifactRef?
├── bounds
├── polygon?
├── rotation?
├── semanticOrder?
├── confidence?
└── direction?
```

The exact underlying Recognition representation remains owned by Recognition Artifact contracts.

Presentation MUST NOT redefine Recognition geometry semantics.

---

# 31. SourceBlockRef

```text
SourceBlockRef
├── sourceBlockId
├── sourceDocumentArtifactRef
├── sourceSegmentIds[]
├── regionIds[]
├── semanticRole?
└── sequence
```

---

# 32. SourceSegmentRef

```text
SourceSegmentRef
├── sourceSegmentId
├── sourceBlockId?
├── sourceDocumentArtifactRef
├── regionIds[]
├── sequence
├── sourceText?
├── language?
├── direction?
└── semanticRole?
```

This is a bounded Presentation view.

Presentation MUST prefer Artifact references plus bounded fields rather than embedding a complete SourceDocument.

---

# 33. TranslationSegmentRef

```text
TranslationSegmentRef
├── translationSegmentId
├── translationUnitId?
├── translationArtifactRef
├── sourceSegmentIds[]
├── sequence
├── translatedText?
├── translationState
├── translationRevision?
├── correctionRevision?
├── confidence?
└── issues[]?
```

Presentation MUST NOT receive provider-specific chunks through this type.

---

# 34. PresentationCompleteness

```text
PresentationCompleteness
- SourceOnly
- WaitingForTranslation
- Partial
- Complete
- Corrected
- Degraded
```

Completeness describes user-visible semantic availability.

It is not Runtime Attempt status.

---

# 35. PresentationItemState

```text
PresentationItemState
- SourceAvailable
- WaitingForTranslation
- PartiallyTranslated
- Translated
- Corrected
- TranslationFailed
- Empty
- Suppressed
```

Avoid Runtime-style names such as:

```text
Running
Completed
Cancelled
Retrying
```

for PresentationItem state.

---

# 36. FocusState

```text
FocusState
- None
- Focused
- Selected
- ActiveForCorrection
```

Focus state is Presentation-local semantic UI state.

Raw input events remain outside Presentation.

---

# 37. PresentationIssue

```text
PresentationIssue
├── issueId
├── code
├── severity
├── presentationItemId?
├── sourceRegionId?
├── messageKey
├── recoverability
└── diagnosticRef?
```

Full user content SHOULD NOT appear inside standard issue metadata.

---

# 38. PresentationSnapshot

Represents one immutable committed Presentation revision.

```text
PresentationSnapshot
├── presentationId
├── presentationContextId
├── sessionId
├── runtimeRevisionId?
├── contentIdentity
├── sourceArtifactRefs[]
├── presentationRevision
├── requestedMode
├── effectiveMode
├── completeness
├── items[]
├── markers[]
├── profileSummary?
├── targetSummary
├── focusState?
├── selectionState?
├── issues[]
└── createdAt
```

Invariants:

1. belongs to exactly one Presentation Context;
2. belongs to exactly one committed PresentationRevision;
3. derives from compatible accepted Artifact references;
4. MUST NOT combine unrelated content lineage;
5. immutable after commit;
6. unchanged semantic items preserve identity;
7. snapshot and RenderPlan of one commit share the same PresentationRevision.

---

# 39. PresentationItem

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
├── state
├── completeness
├── confidenceSummary?
├── semanticRole?
├── focusState
├── availableActions[]
├── layoutHints?
└── issues[]
```

Rules:

* array position is not canonical mapping;
* `sequence` represents semantic visible order;
* corrected content MUST NOT be overwritten by older automatic content;
* absent translated text requires explicit state;
* item identity survives non-semantic layout changes.

---

# 40. PresentationMarker

```text
PresentationMarker
├── markerId
├── presentationItemId
├── sourceRegionRef
├── label
├── sourceGeometry
├── projectedGeometry?
├── visibility
├── emphasis?
└── state
```

Marker semantics belong to Presentation.

Native marker resources belong to UI Adapter.

---

# 41. RenderPlan

Represents framework-neutral display arrangement.

```text
RenderPlan
├── renderPlanId
├── presentationId
├── presentationContextId
├── presentationRevision
├── effectiveMode
├── targetId
├── targetRevision
├── viewportRevision?
├── strategyVersion
├── panelPlan?
├── readerPlan?
├── itemPlans[]
├── markerPlans[]
├── overlayPlans[]
├── overflowItems[]
├── hiddenItems[]
├── focusPlan?
├── fallback?
└── issues[]
```

Invariants:

* immutable;
* belongs to exactly one committed PresentationRevision;
* contains no framework/native object;
* references valid Presentation items and markers;
* coordinate spaces are explicit;
* semantically equivalent fixed inputs SHOULD produce deterministic equivalent plans.

---

# 42. ItemRenderPlan

```text
ItemRenderPlan
├── presentationItemId
├── bounds?
├── order
├── visibility
├── typography
├── containerHints?
├── overflowBehavior
└── interactionHints[]
```

---

# 43. MarkerRenderPlan

```text
MarkerRenderPlan
├── markerId
├── presentationItemId
├── bounds
├── coordinateSpace
├── label
├── visibility
├── emphasis
└── connector?
```

---

# 44. OverlayRenderPlan

```text
OverlayRenderPlan
├── presentationItemId
├── sourceRegionId
├── bounds
├── coordinateSpace
├── textLayout
├── visibility
├── collisionState?
└── readabilityState
```

Overlay plans remain framework-neutral.

---

# 45. PresentationFallback

```text
PresentationFallback
├── requestedMode
├── effectiveMode
├── reasonCode
├── affectedItemIds[]?
├── automatic
└── degradedCapability?
```

Fallback MUST be observable.

---

# 46. CandidatePresentationState

Represents a prepared but not yet authoritative visible state.

```text
CandidatePresentationState
├── candidateId
├── operationId
├── presentationId
├── presentationContextId
├── basedOnPresentationRevision?
├── candidatePresentationRevision
├── runtimeExecutionIdentity?
├── sourceArtifactRefs[]
├── snapshot
├── renderPlan
├── changeSet?
├── completeness
├── fallback?
├── warnings[]
└── diagnosticsSummary?
```

Rules:

1. immutable after preparation;
2. MUST NOT be exposed as current Presentation;
3. MAY be discarded without affecting committed state;
4. does not imply Runtime authority;
5. does not imply UI application;
6. candidate snapshot and RenderPlan share one candidate revision.

---

# 47. PresentationChangeSet

```text
PresentationChangeSet
├── addedItemIds[]
├── updatedItemIds[]
├── removedItemIds[]
├── layoutChanged
├── modeChanged
├── styleChanged
├── visibilityChanged
├── focusChanged
└── completenessChanged
```

A change set describes differences between Presentation revisions.

It does not contain mutable patch instructions for native UI objects.

---

# 48. PresentationCommitRequest

A Presentation candidate must pass commit validation.

```text
PresentationCommitRequest
├── candidateId
├── presentationContextId
├── expectedPresentationRevision?
├── runtimeExecutionIdentity?
├── authorityContextRef?
├── targetId
├── targetRevision
└── viewportRevision?
```

This contract carries data needed to ask whether the candidate may become current.

It does not make Presentation the owner of Runtime authority.

---

# 49. AuthorityRevalidationResult

Authority evaluation is supplied by Runtime/Application authority services.

```text
AuthorityRevalidationResult
├── status
├── runtimeRevisionId?
├── reasonCode?
└── evaluatedAt
```

Possible values:

```text
Accepted
RejectedStale
RejectedCanceled
RejectedSessionInactive
RejectedRuntimeRevision
RejectedTargetInvalidated
RejectedOther
```

Presentation MUST treat Runtime authority rejection as final for that candidate.

Presentation MUST NOT override it.

---

# 50. PresentationCommitResult

```text
PresentationCommitResult
├── status
├── presentationId
├── presentationContextId
├── previousPresentationRevision?
├── presentationRevision?
├── snapshot?
├── renderPlan?
├── fallback?
├── rejectionReason?
└── committedAt?
```

Possible status:

```text
Committed
RejectedAuthority
RejectedPresentationRevision
RejectedTarget
RejectedValidation
DiscardedSuperseded
Failed
```

Only `Committed` creates current Presentation state.

---

# 51. Atomic Commit Rule

A commit MUST atomically advance:

```text
PresentationRevision
+
PresentationSnapshot
+
RenderPlan
```

The following states are invalid:

```text
new Snapshot + old RenderPlan
old Snapshot + new RenderPlan
new Revision + missing Snapshot
new Revision + missing RenderPlan
```

where the active mode requires both outputs.

---

# 52. Preserve-Previous Rule

During preparation:

```text
previous committed Presentation
```

remains authoritative until candidate commit succeeds.

Recoverable failure MUST normally produce:

```text
discard Candidate
+
retain previous committed Presentation
```

not:

```text
clear current Presentation automatically
```

unless policy explicitly requires invalidation.

---

# 53. Common Command Envelope

Presentation commands SHOULD contain:

```text
requestId
contractVersion
issuedAt
presentationContextId
runtimeExecutionIdentity?
cancellationContextRef?
```

Commands MUST NOT require Runtime WorkItem metadata when invoked for purely Presentation-local UI state unless Runtime integration specifically requires it.

---

# 54. BuildPresentation

Creates a new logical Presentation from accepted Artifact references.

```text
BuildPresentation
├── requestId
├── contractVersion
├── issuedAt
├── presentationContextId
├── runtimeExecutionIdentity?
├── cancellationContextRef?
├── inputArtifacts
├── contentIdentity
├── requestedMode
├── target
├── viewport?
├── profile
└── previousPresentationRef?
```

Required validation:

* Presentation Context valid;
* required Artifact references present;
* Artifact types supported;
* content lineage compatible;
* target valid;
* viewport valid when required;
* requested mode known or safely rejectable;
* profile valid;
* required mappings obtainable from accepted Artifact data;
* no Candidate upstream Artifact is passed as authoritative input.

Preparation result:

```text
BuildPresentationPreparedResult
├── requestId
├── operationId
└── candidate
```

This result does not imply commit.

---

# 55. UpdatePresentationContent

Updates Presentation using newer accepted Artifact references.

```text
UpdatePresentationContent
├── requestId
├── contractVersion
├── issuedAt
├── presentationContextId
├── presentationId
├── expectedPresentationRevision
├── runtimeExecutionIdentity?
├── cancellationContextRef?
├── inputArtifacts
├── updateCause
└── contentIdentity
```

Possible causes:

```text
TranslationUpdated
TranslationCorrected
SourceDocumentUpdated
ArtifactReplaced
PartialResultAdvanced
ManualRefresh
```

Rules:

* unchanged semantic items preserve IDs;
* older corrections cannot overwrite newer accepted correction semantics;
* removed upstream semantic content must not remain current;
* full internal rebuild is allowed if external identity invariants remain valid.

---

# 56. RecomputePresentationLayout

Recomputes layout without changing semantic source/translation content.

```text
RecomputePresentationLayout
├── requestId
├── contractVersion
├── issuedAt
├── presentationContextId
├── presentationId
├── expectedPresentationRevision
├── target
├── viewport
├── profile?
├── cancellationContextRef?
└── reason
```

Reasons may include:

```text
WindowResized
SourceMoved
ZoomChanged
ScrollChanged
PanelResized
FontChanged
ThemeChanged
DisplayChanged
TargetCapabilityChanged
ManualRefresh
```

Rules:

* semantic item identity remains stable;
* semantic reading order remains stable;
* obsolete reflow work MAY be coalesced;
* invalid overlay geometry MAY produce Side Panel fallback;
* stale viewport result MUST NOT commit.

---

# 57. ChangePresentationMode

```text
ChangePresentationMode
├── requestId
├── contractVersion
├── issuedAt
├── presentationContextId
├── presentationId
├── expectedPresentationRevision
├── requestedMode
├── target
├── viewport?
├── profile?
└── cancellationContextRef?
```

Rules:

* requested and effective modes may differ;
* fallback reason required when they differ;
* semantic content lineage does not change;
* item identity SHOULD remain stable where possible.

---

# 58. UpdatePresentationFocus

```text
UpdatePresentationFocus
├── requestId
├── contractVersion
├── issuedAt
├── presentationContextId
├── presentationId
├── expectedPresentationRevision
├── presentationItemId?
├── sourceRegionId?
├── focusState
└── cause
```

Rules:

* raw input events MUST NOT appear;
* referenced item/region must belong to current Presentation;
* focus does not change Runtime Revision;
* focus MAY create new PresentationRevision.

---

# 59. ApplyPresentationProfile

```text
ApplyPresentationProfile
├── requestId
├── contractVersion
├── issuedAt
├── presentationContextId
├── presentationId
├── expectedPresentationRevision
├── profile
├── target?
└── viewport?
```

Presentation does not persist the profile.

Profile application may trigger:

* style update;
* reflow;
* mode fallback;
* new PresentationRevision.

---

# 60. ClearPresentation

Logically invalidates committed Presentation state.

```text
ClearPresentation
├── requestId
├── contractVersion
├── issuedAt
├── presentationContextId
├── presentationId?
├── expectedPresentationRevision?
└── reason
```

Reasons:

```text
SessionStopped
SessionReplaced
ContentReplaced
TargetDestroyed
PrivacyInvalidation
ApplicationShutdown
UserRequested
```

Rules:

* idempotent;
* logically invalidates commit eligibility for older Presentation candidates in the context;
* does not delete persistent source/translation content;
* does not itself destroy native surface resources.

---

# 61. Command Preparation Result

Presentation commands that prepare a candidate return:

```text
PresentationPreparationResult
├── requestId
├── operationId
├── status
├── candidate?
├── rejection?
└── diagnosticsSummary?
```

Possible status:

```text
Prepared
Rejected
CanceledLocally
SupersededLocally
Failed
```

`Prepared` means candidate creation succeeded.

It does not mean:

```text
Runtime accepted it
Presentation committed it
UI rendered it
```

---

# 62. PresentationCommandRejection

```text
PresentationCommandRejection
├── requestId
├── commandName
├── presentationContextId?
├── presentationId?
├── expectedPresentationRevision?
├── currentPresentationRevision?
├── reasonCode
├── recoverability
├── retryHint?
└── issues[]
```

Typical Presentation-owned rejection reasons:

```text
PRESENTATION_INVALID_COMMAND
PRESENTATION_CONTEXT_NOT_FOUND
PRESENTATION_NOT_FOUND
PRESENTATION_ITEM_NOT_FOUND
PRESENTATION_REVISION_CONFLICT
PRESENTATION_INCOMPATIBLE_ARTIFACT
PRESENTATION_INVALID_MAPPING
PRESENTATION_INVALID_GEOMETRY
PRESENTATION_INVALID_VIEWPORT
PRESENTATION_UNSUPPORTED_MODE
PRESENTATION_TARGET_CAPABILITY_MISSING
PRESENTATION_INVALID_PROFILE
PRESENTATION_EMPTY_INPUT
PRESENTATION_CANDIDATE_INVALID
```

Runtime authority rejection SHOULD remain distinguishable from Presentation semantic rejection.

---

# 63. Do Not Duplicate Runtime Error Semantics

Presentation SHOULD NOT invent local equivalents of:

```text
SESSION_NOT_ACTIVE
RUNTIME_REVISION_STALE
ATTEMPT_CANCELLED
WORKITEM_SUPERSEDED
RETRY_EXHAUSTED
```

when those facts are owned by Runtime.

Instead, Presentation may expose:

```text
CommitRejected
reasonSource = RuntimeAuthority
runtimeReasonCode = ...
```

or equivalent normalized cross-boundary representation.

---

# 64. Idempotency

Commands SHOULD support deduplication where duplicate delivery is possible.

`ClearPresentation` MUST be idempotent.

Repeated preparation with the same `requestId` MUST NOT create conflicting committed revisions.

Candidate IDs MAY differ if an implementation intentionally re-executes after prior candidate disposal, but observable commit semantics must remain deterministic.

---

# 65. Query — GetCurrentPresentation

```text
GetCurrentPresentation
├── presentationContextId
└── minimumRevision?
```

Result:

```text
GetCurrentPresentationResult
├── found
├── presentationId?
├── presentationRevision?
├── snapshot?
└── renderPlan?
```

Snapshot and RenderPlan returned together MUST belong to the same revision.

---

# 66. Query — GetPresentationSnapshot

```text
GetPresentationSnapshot
├── presentationId
└── presentationRevision?
```

Result:

```text
GetPresentationSnapshotResult
├── found
└── snapshot?
```

---

# 67. Query — GetRenderPlan

```text
GetRenderPlan
├── presentationId
└── presentationRevision?
```

Result:

```text
GetRenderPlanResult
├── found
└── renderPlan?
```

---

# 68. Query — GetPresentationItem

```text
GetPresentationItem
├── presentationId
├── presentationRevision?
└── presentationItemId
```

Result:

```text
GetPresentationItemResult
├── found
└── item?
```

---

# 69. Query — GetPresentationSummary

```text
GetPresentationSummary
- presentationContextId
```

Result:

```text
PresentationSummary
├── presentationId?
├── presentationRevision?
├── contentIdentity?
├── effectiveMode?
├── completeness?
├── itemCount
├── markerCount
├── overflowCount
├── issueCount
└── targetId?
```

---

# 70. Query — GetPresentationDiagnostics

```text
GetPresentationDiagnostics
├── presentationContextId
├── presentationId?
└── includeTechnicalDetails
```

Diagnostic queries MUST follow privacy restrictions.

Full source/translation text MUST NOT be returned by default.

---

# 71. UI Apply Request

Presentation logical commit and actual UI apply are distinct.

A compatible UI Adapter consumes:

```text
PresentationApplyRequest
├── presentationContextId
├── presentationId
├── presentationRevision
├── snapshotRefOrValue
├── renderPlanRefOrValue
├── targetId
└── targetRevision
```

The concrete delivery mechanism may vary, but semantics must remain equivalent.

---

# 72. UI Apply Result

```text
PresentationApplyResult
├── presentationContextId
├── presentationId
├── presentationRevision
├── targetId
├── status
├── appliedAt?
├── reasonCode?
└── diagnostics?
```

Possible status:

```text
Applied
RejectedStale
RejectedTargetMismatch
TargetUnavailable
Failed
```

Presentation MUST NOT depend on framework-specific exception classes.

---

# 73. Presentation Commit vs UI Apply

These are separate:

```text
Presentation Commit
    ↓
logical Presentation is current

UI Apply
    ↓
actual native/framework surface reflects it
```

Therefore a valid state exists where:

```text
Presentation committed
UI apply failed
```

This condition must be observable.

It does not automatically invalidate Runtime Artifact publication.

---

# 74. Input Validation

Presentation MUST reject or degrade input when:

* required Presentation identifiers are absent;
* Artifact type unsupported;
* Artifact references incompatible;
* ContentIdentity lineage conflicts;
* expected PresentationRevision conflicts;
* geometry missing coordinate-space metadata;
* numeric geometry invalid;
* transform chain invalid;
* viewport invalid;
* target capabilities insufficient;
* requested mode unsupported and fallback disabled;
* mapping refers to missing source structures;
* RenderPlan cannot satisfy mandatory readability policy;
* candidate violates Presentation invariants.

---

# 75. Runtime Authority Validation

Presentation MUST NOT determine Runtime authority using private local state.

At commit boundary:

```text
candidate
    ↓
authority revalidation service / Runtime Control
    ↓
accepted or rejected
```

If Runtime rejects authority:

* candidate is discarded;
* committed Presentation remains unchanged unless separate invalidation applies;
* Presentation MUST NOT override the Runtime decision.

---

# 76. Presentation Revision Validation

Presentation itself owns optimistic concurrency for PresentationRevision.

Candidate commit MUST fail or be superseded when:

```text
expectedPresentationRevision
!=
currentPresentationRevision
```

unless the command explicitly supports deterministic merge.

For MVP:

```text
automatic concurrent merge is not required.
```

---

# 77. Target Revision Validation

When layout depends on target capabilities, candidate commit should verify:

```text
candidate.targetRevision
==
current target revision
```

or otherwise prove compatibility.

A stale target revision MUST NOT result in unsafe overlay placement.

---

# 78. Viewport Revision Validation

Layout-sensitive candidates should verify the current viewport revision.

For high-frequency viewport changes:

```text
older candidates may be silently superseded
```

without being treated as errors.

---

# 79. Candidate Invariant Validation

Before commit:

* candidate ID exists;
* Presentation ID valid;
* Presentation Context valid;
* PresentationRevision valid;
* Snapshot valid;
* RenderPlan valid;
* Snapshot/RenderPlan revisions match;
* source references resolve;
* item IDs unique;
* marker IDs unique;
* item mappings valid;
* marker mappings valid;
* semantic sequence valid;
* geometry valid;
* target compatible;
* no framework object present;
* no native handle present;
* no mutable upstream object present.

---

# 80. Compatibility Metadata

Presentation may attach compatibility metadata to committed state.

```text
PresentationCompatibilityMetadata
├── presentationContractVersion
├── sourceArtifactContractVersions[]
├── presentationProfileVersion?
├── targetCapabilityVersion?
├── geometryTransformVersion?
├── strategyVersion
├── locale?
├── typographyPolicyVersion?
└── privacyPartition?
```

This describes semantic dependencies.

It does not determine Runtime current authority.

---

# 81. Determinism Contract

For semantically equivalent fixed:

```text
accepted Artifact set
ContentIdentity
PresentationProfile
PresentationTarget
ViewportSnapshot
strategy version
```

Presentation SHOULD produce semantically equivalent:

```text
item mapping
semantic ordering
mode resolution
fallback decision
layout classification
PresentationSnapshot content
RenderPlan structure
```

Generated IDs and timestamps may differ unless deterministic fixture policy requires fixed values.

---

# 82. Partial Presentation

Incomplete Translation does not automatically invalidate Presentation.

Presentation may represent:

```text
source only
waiting
partial translation
completed translation
corrected translation
failed translation item
```

Partial semantics must come from accepted upstream contracts.

Presentation MUST NOT treat raw provider token output as accepted Presentation content.

---

# 83. Correction Precedence

If accepted upstream data indicates a manual or higher-priority correction:

```text
older automatic translation
```

must not overwrite it.

Presentation should use upstream version/lineage metadata rather than local timestamps alone.

---

# 84. Reading Order Contract

Presentation consumes canonical semantic order from accepted upstream content.

Presentation MAY define separate visual ordering metadata.

It MUST NOT silently mutate source semantic order.

Example:

```text
semanticSequence
visualPosition
```

are separate concepts.

---

# 85. Readability Contract

Presentation MUST honor minimum readability thresholds.

It MUST NOT indefinitely reduce font size to force text into original source geometry.

Valid fallback may include:

```text
wrap
expand container
scroll
collapse secondary source text
focused overlay
Side Panel fallback
```

---

# 86. Geometry Contract

Public geometry must declare coordinate space.

The following are invalid:

* non-finite values;
* negative dimensions;
* unknown required coordinate spaces;
* unsupported transform chain;
* incompatible source/target revisions;
* zero-size geometry where projection requires non-zero dimensions.

Invalid overlay geometry MAY still allow a valid Side Panel Presentation.

---

# 87. Security Contract

Presentation MUST NOT:

* store provider credentials;
* call Translation/OCR provider endpoints directly;
* execute scripts contained in content;
* expose native handles;
* dynamically execute content callbacks;
* bypass Runtime Artifact access policy.

---

# 88. Privacy Contract

Presentation may temporarily process:

* source text;
* translated text;
* geometry;
* accepted correction data;
* profile preferences.

Normal diagnostics MUST NOT contain:

* screenshots;
* complete source documents;
* complete Translation Artifact contents;
* provider prompts;
* credentials;
* private window titles.

Presentation persistence requires an explicit external persistence contract.

---

# 89. Resource Contract

Presentation may hold:

```text
Artifact leases
temporary mapping structures
temporary layout state
CandidatePresentationState
current committed Presentation
previous committed Presentation
```

according to bounded resource policy.

Presentation does not become Artifact owner by acquiring a lease.

All leases MUST be released according to Runtime resource-lifecycle rules.

---

# 90. Performance Contract

The public design must support measuring:

```text
candidate preparation duration
mapping duration
layout duration
geometry projection duration
authority revalidation latency
Presentation commit latency
UI apply latency
coalesced operation count
superseded candidate count
current Presentation memory
candidate Presentation memory
```

Performance targets are implementation targets, not permanent wire compatibility guarantees.

---

# 91. Event Boundary

Presentation events describe Presentation-owned facts.

Successful facts MUST describe already committed Presentation state.

Correct ordering:

```text
prepare
    ↓
validate
    ↓
authority revalidate
    ↓
commit
    ↓
publish success fact
```

Incorrect ordering:

```text
publish success
    ↓
commit later
```

Detailed schemas belong to `EVENTS.md`.

---

# 92. Event Bus Is Not the Orchestrator

Presentation MUST NOT require this implicit chain:

```text
TranslationCompleted
    ↓
Presentation automatically starts
```

for architecture correctness.

Business Pipeline Orchestration / Runtime decides required work.

Events may provide:

* notification;
* observability;
* UI update signaling;
* optional integration.

They must not secretly redefine execution ownership.

---

# 93. Presentation Success Facts

Typical committed-success events:

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
PresentationCleared
```

Each should reference:

```text
presentationContextId
presentationId
presentationRevision
```

where applicable.

---

# 94. Presentation Rejection / Failure Facts

Presentation may expose:

```text
PresentationRejected
PresentationFailed
```

`PresentationRejected` typically means:

* command invalid;
* candidate invalid;
* optimistic concurrency conflict;
* unsupported capability.

`PresentationFailed` should be reserved for Presentation-owned unrecoverable/internal failures.

Runtime cancellation or stale Revision is not automatically Presentation failure.

---

# 95. Contract Compatibility

Presentation contracts MUST remain independent from:

```text
Electron
Tauri
Flutter
Qt
Wails
Browser Extension APIs
Android Views
SwiftUI
WinUI
AppKit
DOM
Canvas implementations
```

Technology-specific UI adapters may depend on Presentation public contracts.

Presentation core MUST NOT import concrete adapters.

---

# 96. Contract Versioning

Semantic versioning:

```text
MAJOR.MINOR.PATCH
```

## Patch

May include:

* clarification;
* documentation correction;
* compatible validation correction;
* new optional diagnostics field.

## Minor

May include:

* new optional fields;
* new optional commands;
* new optional capability;
* new backward-compatible query;
* new event fact.

## Major

Required for:

* removing required fields;
* renaming public commands incompatibly;
* changing PresentationRevision meaning;
* changing ownership semantics;
* changing commit model;
* changing immutable outputs into mutable state;
* replacing ArtifactRef input with incompatible direct data model.

This Runtime-v2 synchronization is a major contract revision from the previous authority model.

---

# 97. Unknown Fields

Unknown optional fields SHOULD be ignored when safe.

Unknown required semantic enum values MUST:

* be rejected;
* or use an explicitly documented version-compatible fallback.

Never silently reinterpret an unknown required value.

---

# 98. Architecture Invariants

1. Presentation does not perform OCR.

2. Presentation does not perform Text Processing.

3. Presentation does not execute Translation.

4. Presentation consumes accepted immutable Artifact references.

5. Presentation does not consume raw provider output.

6. Presentation does not own Runtime Revision authority.

7. Presentation does not own WorkItem lifecycle.

8. Presentation does not own Attempt lifecycle.

9. Presentation does not own Runtime retry.

10. Presentation does not own global cancellation authority.

11. `PresentationOperationId` is not a Runtime `WorkItemId`.

12. `PresentationRevision` is distinct from Runtime `RevisionId`.

13. Candidate Presentation state is not committed Presentation state.

14. Prepared does not mean committed.

15. Committed does not mean UI applied.

16. Runtime authority must be valid at Presentation commit.

17. Runtime authority rejection cannot be overridden by Presentation.

18. Presentation owns optimistic concurrency of PresentationRevision.

19. Snapshot and RenderPlan commit atomically as one PresentationRevision.

20. Previous committed Presentation remains current until replacement commit succeeds.

21. Stale candidates cannot overwrite newer committed Presentation state.

22. Accepted upstream Artifacts remain immutable.

23. Presentation does not publish Recognition, SourceDocument, or Translation Artifacts.

24. Presentation does not own native window resources.

25. Presentation does not own DOM or widget instances.

26. Coordinate spaces are explicit.

27. Semantic item identity is independent from array position.

28. Semantic reading order is not silently rewritten by layout.

29. Partial translation is explicitly represented.

30. Manual accepted correction outranks older automatic content.

31. Side Panel fallback is allowed when overlay is unsafe or unreadable.

32. Readability outranks forced exact placement.

33. Events do not replace Runtime orchestration.

34. Success events describe committed Presentation state.

35. Standard diagnostics do not contain complete user reading content.

---

# 99. Example — Initial Build

```text
Published Recognition Artifact
        +
Published SourceDocument Artifact
        +
Published Translation Artifact
        ↓
BuildPresentation
        ↓
Presentation validates Artifact compatibility
        ↓
Presentation maps PresentationItems
        ↓
Presentation resolves effective mode
        ↓
CandidateSnapshot
        +
CandidateRenderPlan
        ↓
CandidatePresentationState
        ↓
Runtime authority revalidation
        ↓
Presentation commit
        ↓
Presentation Revision 1
        ↓
PresentationPrepared
        ↓
UI Adapter apply
```

---

# 100. Example — Partial Translation Update

```text
Presentation Revision 4 current
        ↓
New accepted Translation Artifact
        ↓
UpdatePresentationContent
expectedPresentationRevision = 4
        ↓
affected PresentationItems updated
        ↓
Candidate Revision 5
        ↓
authority revalidation
        ↓
commit
        ↓
Presentation Revision 5
```

Unchanged items preserve identity.

---

# 101. Example — Viewport Coalescing

```text
Viewport 20
Viewport 21
Viewport 22
        ↓
reflow 20 obsolete
reflow 21 obsolete
        ↓
RecomputePresentationLayout
viewportRevision = 22
        ↓
Candidate Presentation Revision N+1
        ↓
commit
```

No failure is required for discarded revisions 20 and 21.

---

# 102. Example — Runtime Revision Superseded

```text
Runtime Revision 14
        ↓
Presentation candidate prepared
        ↓
Runtime Revision 15 becomes current
        ↓
candidate 14 reaches commit boundary
        ↓
Runtime authority revalidation
        ↓
RejectedStale
        ↓
candidate discarded
        ↓
current Presentation unchanged
```

Presentation does not change Runtime Revision state.

---

# 103. Example — Presentation Revision Conflict

```text
Current Presentation Revision 7

Operation A
expected = 7

Operation B
expected = 7
        ↓
Operation B commits Revision 8
        ↓
Operation A reaches commit
        ↓
expected 7 != current 8
        ↓
Operation A rejected / superseded
```

Runtime Revision may remain unchanged throughout this flow.

---

# 104. Example — Overlay Fallback

```text
requestedMode = Overlay
        ↓
target supports overlay
        ↓
geometry valid
        ↓
Vietnamese text cannot satisfy readability threshold
        ↓
fallback policy
        ↓
effectiveMode = SidePanel
        ↓
Candidate records fallback
        ↓
commit
```

This is a successful degraded Presentation.

---

# 105. Example — UI Apply Failure

```text
Presentation Revision 12 committed
        ↓
PresentationUpdated
        ↓
UI Adapter apply
        ↓
TargetUnavailable
```

Presentation logical state and actual visual state are now temporarily divergent.

The failure must be observable and handled according to UI/Application recovery policy.

Presentation MUST NOT pretend the UI apply succeeded.

---

# 106. Example — Clear

```text
Session stopping
        ↓
Runtime revokes processing authority
        ↓
ClearPresentation
        ↓
Presentation current state invalidated
        ↓
PresentationCleared
        ↓
UI Adapter removes binding / visible content
```

Native resource destruction remains UI Adapter/platform responsibility.

---

# 107. Deferred Extensions

Deferred contracts include:

* plugin-provided Presentation strategies;
* persisted user-adjusted overlay geometry;
* artwork-aware translation placement;
* image inpainting;
* translated-image export;
* framework-specific animation instructions;
* advanced multi-monitor native placement;
* collaborative annotations;
* browser DOM rewriting;
* print/export page composition;
* rich annotation systems.

Future extensions MUST preserve:

```text
Runtime Authority
≠
Presentation Commit
≠
UI Apply
```

---

# 108. Related Documents

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULE_ROLE.md
.meta/WORKFLOW.md
.meta/CHANGE_RULE.md

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

doc/02-modules/presentation/MODULE.md
doc/02-modules/presentation/STATES.md
doc/02-modules/presentation/EVENTS.md
doc/02-modules/presentation/ERRORS.md
doc/02-modules/presentation/README.md

doc/02-modules/recognition/CONTRACT.md
doc/02-modules/text-processing/CONTRACT.md
doc/02-modules/translation/CONTRACT.md
doc/02-modules/reading-session/CONTRACT.md
doc/02-modules/preferences/CONTRACT.md
doc/02-modules/diagnostics/CONTRACT.md
doc/02-modules/ui-adapter/CONTRACT.md
```

---

# 109. Completion Criteria

This contract is ready for implementation review when:

* Presentation accepts published Artifact references rather than raw upstream mutable objects;
* Runtime identity and Presentation identity are clearly separated;
* `PresentationOperationId` cannot be confused with WorkItem/Attempt identity;
* `PresentationRevision` is explicitly independent from Runtime `RevisionId`;
* Candidate Presentation state exists as a separate concept from committed state;
* Runtime authority is revalidated rather than reimplemented inside Presentation;
* PresentationRevision optimistic concurrency is deterministic;
* Snapshot and RenderPlan commit atomically;
* Presentation and UI apply lifecycle are explicitly separate;
* stale Runtime work cannot commit Presentation;
* stale Presentation operations cannot overwrite newer Presentation revisions;
* target and viewport revisions protect layout commit;
* Side Panel/Overlay fallback is contractually observable;
* partial Translation can be represented;
* stable item identity rules are testable;
* public geometry always has coordinate-space semantics;
* `ui-adapter` requires no Presentation internal types;
* no contract exposes provider SDK or native UI objects;
* all public contracts are serializable;
* EVENTS and ERRORS can refine behavior without redefining ownership.

---

# 110. Summary

The Presentation public boundary is:

```text
Accepted Runtime ArtifactRefs
        ↓
Presentation Command
        ↓
Presentation Operation
        ↓
CandidatePresentationState
        ↓
Runtime Authority Revalidation
        +
PresentationRevision Validation
        ↓
Atomic Presentation Commit
        ↓
PresentationSnapshot
        +
RenderPlan
        ↓
UI Adapter Apply
```

Ownership is:

```text
Runtime Control
    → Runtime Revision / WorkItem / Attempt / authority

Artifact Store
    → accepted Runtime Artifacts

Presentation
    → Presentation semantic state
    → Candidate Presentation
    → PresentationRevision
    → PresentationSnapshot
    → RenderPlan

UI Adapter / Platform
    → actual UI resources and rendering
```

The central contract invariant is:

```text
Prepared is not committed.

Committed is not rendered.

Runtime authority determines whether work may commit.

PresentationRevision determines which Presentation state is current.

UI Adapter determines whether that state became actual visible UI.
```
