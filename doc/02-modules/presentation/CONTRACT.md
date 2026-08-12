# Presentation Contract

> **Project:** CRAI
> **Module:** `presentation`
> **Path:** `doc/02-modules/presentation/CONTRACT.md`
> **Contract Version:** 3.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** Presentation
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines the public contract boundary of the Presentation module.

Presentation transforms compatible accepted semantic Artifacts and immutable presentation context into:

```text
PresentationSnapshot
+
RenderPlan
```

using a candidate-and-commit model:

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
Semantic Validation
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

```text
perform OCR

perform Text Processing

perform Translation

own RuntimeRevision authority

own WorkItem / Attempt lifecycle

own Reading Session lifecycle

publish upstream semantic Artifacts

own native UI resources

manipulate DOM/native widgets directly
```

---

# 2. Contract Goals

The contract exists so that:

```text
Runtime/Application
    can invoke Presentation
    without Presentation internals

Presentation
    can consume immutable
    semantic Artifact references

stale Candidates
    cannot overwrite newer state

UI Adapter
    can consume Presentation output
    without Presentation internals
```

All public values must remain:

```text
serializable

immutable after publication/commit

versionable

provider-independent

platform-independent

framework-independent

testable
```

---

# 3. Contract Scope

This file owns public definitions for:

```text
Presentation identity

Presentation semantic references

Presentation context

Presentation operation input

Candidate Presentation state

commit contracts

PresentationSnapshot

PresentationItem

PresentationMarker

RenderPlan

PresentationTarget

ViewportSnapshot

PresentationProfile

geometry contracts

overlay contracts

readability contracts

commands

queries

UI apply boundary

revision semantics

compatibility/versioning
```

---

# 4. Out of Scope

This file does not define:

```text
internal Presentation algorithms

text-fitting implementation

collision-search algorithm

Bubble detection

image reconstruction

native rendering

Runtime WorkItem/Attempt state machine

scheduler implementation

Artifact Store implementation

Event Bus transport

full Presentation STATES catalog

full Presentation EVENTS catalog

full Presentation ERRORS catalog

persistence schema
```

Those belong to their owners.

---

# 5. Architectural Boundary

Canonical flow:

```text
Published Semantic Artifacts
        ↓
Runtime / Application
        ↓
Presentation Command
        ↓
Presentation
        ↓
CandidatePresentationState
        ↓
semantic validation
        ↓
authority revalidation
        ↓
atomic commit
        ↓
PresentationSnapshot
+
RenderPlan
        ↓
UI Adapter
        ↓
ViewModel
        ↓
Frontend
```

---

# 6. Presentation Ownership

Presentation owns:

```text
PresentationId

PresentationContextId

PresentationOperationId

PresentationItemId

MarkerId

PresentationRevision

Presentation semantic grouping

requested/effective PresentationMode

OverlayPlacementStrategy semantics

Candidate construction

PresentationSnapshot

RenderPlan

layout intent

readability/fallback decisions

Presentation-local focus/selection

Presentation optimistic concurrency

atomic commit
```

---

# 7. Presentation Does Not Own

Presentation does not own:

```text
RuntimeRevisionId

WorkItemId

AttemptId

Runtime authority

Retry lifecycle

Cancellation authority

SourceDocument lifecycle

TranslationArtifact lifecycle

TranslationUnit semantics

Recognition geometry semantics

Bubble detection semantics

Reading Session lifecycle

native UI resource lifecycle

persistent Preferences

durable reading history
```

---

# 8. Serializable Boundary

All public contract values MUST be serializable.

Forbidden public values include:

```text
DOM Node

HTMLElement

native window handle

framework widget

provider client

database connection

SDK-specific response

mutable internal entity

callback

thread-affine UI object
```

---

# 9. Immutability

Published/committed public values are immutable.

This includes:

```text
PresentationSnapshot

RenderPlan

PresentationProfile

PresentationTarget

ViewportSnapshot

CandidatePresentationState

PresentationSemanticRef
```

A semantic change produces a new Presentation revision/state.

---

# 10. Stable References

Cross-module contracts SHOULD prefer:

```text
ArtifactRef

typed semantic reference

bounded immutable value
```

rather than copying whole upstream aggregates.

---

# 11. Explicit Ownership

A field appearing in Presentation data does not transfer ownership of the referenced semantic concept to Presentation.

Example:

```text
PresentationItem
    contains TranslationUnitRef
```

does not mean Presentation owns TranslationUnit.

---

# 12. No Hidden Runtime

Presentation contracts MUST NOT recreate:

```text
WorkItem state

Attempt state

Retry state

Scheduler state

global Runtime Revision registry

competing cancellation state
```

---

# 13. RuntimeExecutionIdentity

Presentation operations executed through Runtime may carry:

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

```text
runtimeRevisionId
    = Runtime execution authority

PresentationRevision
    = Presentation semantic authority
```

Presentation MUST NOT create Runtime identifiers.

---

# 14. CancellationContextRef

```text
CancellationContextRef
├── cancellationContextId
├── runtimeRevisionId
├── workItemId?
└── attemptId?
```

This enables cooperative cancellation.

Presentation MUST NOT:

```text
mark Runtime work cancelled

decide Attempt terminal state

create competing cancellation authority
```

---

# 15. PresentationContextId

Identifies one logical presentation scope.

```text
PresentationContextId
- value
```

Examples:

```text
main-reader

text-reader

comic-overlay

focused-overlay
```

It is independent from native surface identity.

---

# 16. PresentationId

Identifies one logical Presentation lineage.

```text
PresentationId
- value
```

It may survive:

```text
layout changes

profile changes

compatible content updates
```

when semantic Presentation identity remains compatible.

---

# 17. PresentationOperationId

```text
PresentationOperationId
- value
```

Used for Presentation-local:

```text
diagnostics

candidate correlation

operation tracing
```

It is not:

```text
WorkItemId

AttemptId
```

---

# 18. PresentationRequestId

Correlates public command and immediate response.

```text
PresentationRequestId
- value
```

It MUST NOT be used as semantic Presentation identity.

---

# 19. PresentationItemId

Identifies one semantic visible unit.

It SHOULD remain stable across:

```text
layout recomputation

viewport changes

compatible profile changes

compatible Translation updates
```

when semantic grouping remains unchanged.

---

# 20. MarkerId

Identifies one Presentation marker.

It remains stable while its semantic association remains compatible.

---

# 21. PresentationRevision

Represents committed Presentation semantic state.

```text
PresentationRevision
- monotonic token within Presentation scope
```

Rules:

1. scoped to a Presentation context/lineage;
2. increases when committed Presentation state changes;
3. never decreases;
4. older revision cannot overwrite newer revision;
5. layout-only change may create a revision;
6. focus/selection change may create a revision where contract requires;
7. it is distinct from RuntimeRevisionId.

---

# 22. Runtime Revision vs Presentation Revision

```text
RuntimeRevisionId
    → is execution intent still current?

PresentationRevision
    → which Presentation state is committed?
```

They MUST NOT be collapsed.

---

# 23. ArtifactRef

```text
ArtifactRef
├── artifactId
├── artifactType
├── contractVersion
├── contentIdentity
├── compatibilityMetadata?
└── owner?
```

Typical accepted Artifacts:

```text
RECOGNITION_ARTIFACT

SOURCE_DOCUMENT_ARTIFACT

TRANSLATION_ARTIFACT
```

Only Published/accepted upstream Artifacts may become authoritative Presentation inputs.

---

# 24. PresentationInputArtifactSet

```text
PresentationInputArtifactSet
├── recognitionArtifactRef?
├── sourceDocumentArtifactRef?
├── translationArtifactRef?
└── auxiliaryArtifactRefs[]?
```

Rules:

* required Artifacts depend on Presentation capability;
* all supplied Artifacts must belong to compatible lineage;
* Presentation never mutates them;
* optional absence must have defined semantics;
* compatibility is not determined solely by RuntimeRevisionId.

---

# 25. ContentIdentity

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

ContentIdentity supports semantic compatibility.

It is not Runtime authority.

---

# 26. PresentationMode

```text
PresentationMode
- SidePanel
- TextReader
- Overlay
- Hybrid
- Unknown
```

Rules:

* requested/effective mode are separate;
* unsupported values are not silently reinterpreted;
* fallback may resolve another effective mode;
* fallback reason is observable.

---

# 27. Presentation Mode vs Overlay Strategy

These are different abstractions:

```text
PresentationMode
    = high-level presentation family

OverlayPlacementStrategy
    = spatial strategy used inside
      Overlay/Hybrid presentation
```

Example:

```text
requestedMode = Overlay
overlayStrategy = Adjacent
```

---

# 28. OverlayPlacementStrategy

```text
OverlayPlacementStrategy
- Replace
- Cover
- Adjacent
- Floating
- Tooltip
- OnDemand
- Unknown
```

This is a bounded Presentation semantic value.

It MUST NOT replace `PresentationMode`.

---

# 29. Overlay Strategy Semantics

### Replace

Translation occupies a validated source replacement area.

### Cover

A readable surface covers the source text area.

### Adjacent

Translation is positioned near source content.

### Floating

Translation appears in a linked positioned container.

### Tooltip

Translation is exposed through temporary explicit interaction.

### OnDemand

Translation remains hidden until requested.

---

# 30. OverlayPlacementPolicy

```text
OverlayPlacementPolicy
├── preferredStrategies[]
├── allowAutomaticFallback
├── preserveArtwork
├── requireBubbleSafeAreaForReplace?
├── minimumReadableFontSize?
├── allowAdjacent
├── allowFloating
├── allowTooltip
└── allowOnDemand
```

Exact implementation scoring remains internal.

---

# 31. PresentationTarget

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

Possible kinds:

```text
MainWindow

CompanionPanel

FloatingSurface

OverlaySurface

BrowserSurface

Unknown
```

A PresentationTarget is not a native UI resource.

---

# 32. TargetCapabilities

Possible normalized capabilities:

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

Presentation MUST NOT query native OS APIs directly for these.

---

# 33. ViewportSnapshot

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

* numeric values finite;
* dimensions non-negative;
* coordinate space mandatory;
* stale viewport layouts cannot overwrite newer committed layouts.

---

# 34. PresentationProfile

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

# 35. TypographyProfile

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

* sizes finite and positive;
* minimum readable size is enforced;
* native font objects forbidden.

---

# 36. ReadabilityPolicy

```text
ReadabilityPolicy
├── minimumReadableFontSize
├── allowWrap
├── allowContainerExpansion
├── allowScroll
├── allowFocusedOverlay
├── allowModeFallback
└── preserveSemanticContent
```

`preserveSemanticContent` MUST be true for normal Presentation.

---

# 37. Readability Invariant

Presentation MUST NOT indefinitely shrink text to fit geometry.

Canonical fallback order may be:

```text
wrap
    ↓
bounded font reduction
    ↓
safe expansion
    ↓
scroll/focused overlay
    ↓
alternate overlay strategy
    ↓
SidePanel fallback
```

---

# 38. Translation Truncation Prohibited

Presentation MUST NOT silently truncate semantic Translation content to satisfy layout.

If fitting fails:

```text
change Presentation strategy
```

not:

```text
change Translation meaning
```

---

# 39. CoordinateSpace

```text
CoordinateSpace
- SourceImage
- CapturedFrame
- NormalizedSource
- DocumentPage
- ApplicationViewport
- BrowserViewport
- Screen
- OverlaySurface
- Unknown
```

Every public geometry object declares its coordinate space.

---

# 40. Point

```text
Point
├── x
├── y
└── coordinateSpace
```

All values finite.

---

# 41. Size

```text
Size
├── width
└── height
```

Values finite and non-negative.

---

# 42. Rect

```text
Rect
├── x
├── y
├── width
├── height
└── coordinateSpace
```

---

# 43. Polygon

```text
Polygon
├── points[]
└── coordinateSpace
```

A polygon MUST contain valid finite points.

---

# 44. GeometryTransform

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

A coordinate-space conversion requires an explicit compatible transform.

---

# 45. SourceRegionRef

References accepted Recognition/source-region semantics.

```text
SourceRegionRef
├── regionId
├── recognitionArtifactRef?
├── bounds
├── polygon?
├── rotation?
├── semanticOrder?
├── confidence?
├── direction?
└── regionRole?
```

Presentation does not redefine Recognition geometry.

---

# 46. Critical Region Invariant

```text
SourceRegionRef
    ≠
SpeechBubbleRef
```

A recognized text region represents text extent.

It MUST NOT automatically represent:

```text
full Bubble boundary

safe Bubble interior

available replacement area
```

---

# 47. SpeechBubbleRef

Optional reference available only when an upstream owner provides authoritative/accepted Bubble semantics.

```text
SpeechBubbleRef
├── bubbleId
├── sourceArtifactRef
├── bounds?
├── polygon?
├── safeInterior?
├── tailGeometry?
├── orientation?
├── associatedRegionIds[]
├── panelRef?
├── confidence?
└── coordinateSpace
```

Presentation MUST NOT fabricate this contract from `SourceRegionRef`.

---

# 48. Bubble Geometry Ownership

`SpeechBubbleRef` is a Presentation-facing bounded reference.

The underlying Bubble semantics remain owned upstream.

Presentation may consume them for:

```text
fit evaluation

Replace strategy

Bubble-fill background

collision analysis
```

---

# 49. Missing Bubble Semantics

When no `SpeechBubbleRef` exists:

```text
Replace
```

must not assume one.

Safer strategies may include:

```text
Cover

Adjacent

Floating

SidePanel
```

---

# 50. SourceSemanticNodeKind

Typed Text Processing semantic node kinds:

```text
SourceSemanticNodeKind
- Section
- Block
- Paragraph
- Sentence
- Span
- Token
- Auxiliary
- Unknown
```

This enum mirrors only public semantic identity categories required by Presentation.

It does not transfer ownership.

---

# 51. SourceSemanticRef

Replaces legacy `SourceSegmentRef`.

```text
SourceSemanticRef
├── nodeId
├── nodeKind
├── sourceDocumentArtifactRef
├── parentNodeId?
├── sourceRegionIds[]
├── sourceRange?
├── sequence
├── semanticRole?
├── language?
├── direction?
└── sourceText?
```

Rules:

* bounded Presentation view only;
* full SourceDocument must not be embedded;
* node identity belongs to Text Processing;
* `sourceText` is optional convenience data.

---

# 52. SourceBlockRef

`SourceBlockRef` remains available where Block-specific semantics are important.

```text
SourceBlockRef
├── sourceBlockId
├── sourceDocumentArtifactRef
├── semanticNodeRefs[]
├── regionIds[]
├── semanticRole?
└── sequence
```

Removed:

```text
sourceSegmentIds[]
```

---

# 53. TranslationUnitRef

Replaces legacy `TranslationSegmentRef`.

```text
TranslationUnitRef
├── translationUnitId
├── translationArtifactRef
├── sourceSemanticRefs[]
├── sequence
├── translatedText?
├── translationState
├── translationRevision?
├── correctionRevision?
├── confidence?
└── issues[]?
```

Presentation MUST NOT consume provider chunks through this type.

---

# 54. Translation Unit Ownership

`TranslationUnitRef` is a bounded Presentation-facing view.

Translation remains owner of:

```text
TranslationUnit

Translation semantic revision

correction semantics
```

---

# 55. No TranslationSegmentRef

Deprecated and removed:

```text
TranslationSegmentRef
```

because `TranslationUnit` is the canonical Translation-owned semantic unit.

---

# 56. No SourceSegmentRef

Deprecated and removed:

```text
SourceSegmentRef
```

because Text Processing now exposes typed semantic nodes.

---

# 57. PresentationCompleteness

```text
PresentationCompleteness
- SourceOnly
- WaitingForTranslation
- Partial
- Complete
- Corrected
- Degraded
```

This describes semantic availability.

It is not Runtime Attempt status.

---

# 58. PresentationItemState

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

Forbidden Runtime-style states:

```text
Running

Retrying

Cancelled

TimedOut
```

---

# 59. FocusState

```text
FocusState
- None
- Focused
- Selected
- ActiveForCorrection
```

Focus is Presentation-local semantic UI state.

Raw device events remain outside Presentation.

---

# 60. PresentationIssue

```text
PresentationIssue
├── issueId
├── code
├── severity
├── presentationItemId?
├── sourceRegionId?
├── sourceSemanticNodeId?
├── translationUnitId?
├── messageKey
├── recoverability
└── diagnosticRef?
```

Full user content should not appear in normal issue metadata.

---

# 61. PresentationSnapshot

```text
PresentationSnapshot
├── presentationId
├── presentationContextId
├── sessionId
├── runtimeRevisionId?
├── contentIdentity
├── sourceArtifactRefs[]
├── translationArtifactRef?
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

---

# 62. PresentationSnapshot Invariants

1. exactly one Presentation context;
2. exactly one committed PresentationRevision;
3. derived only from compatible accepted Artifacts;
4. no unrelated content lineage;
5. immutable after commit;
6. unchanged semantic items preserve IDs;
7. Snapshot and RenderPlan share the same revision.

---

# 63. PresentationItem

Updated canonical structure:

```text
PresentationItem
├── presentationItemId
├── sourceSemanticRefs[]
├── sourceBlockRefs[]?
├── translationUnitRefs[]
├── recognitionRegionRefs[]?
├── speechBubbleRefs[]?
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

Removed:

```text
sourceSegmentIds[]

translationSegmentIds[]
```

---

# 64. PresentationItem Mapping Rules

* array position is not canonical mapping;
* `sequence` is semantic visible order;
* semantic refs identify provenance;
* corrected Translation cannot be overwritten by older automatic Translation;
* absent target text requires explicit state;
* item identity survives non-semantic layout changes.

---

# 65. PresentationMarker

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

# 66. RenderPlan

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

---

# 67. RenderPlan Invariants

* immutable;
* exactly one PresentationRevision;
* framework-neutral;
* valid item/marker references only;
* coordinate spaces explicit;
* deterministic equivalent inputs should produce equivalent semantic plans.

---

# 68. ItemRenderPlan

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

# 69. MarkerRenderPlan

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

# 70. OverlayRenderPlan

Expanded contract:

```text
OverlayRenderPlan
├── presentationItemId
├── sourceRegionRef
├── speechBubbleRef?
├── strategy
├── sourceBounds
├── placementBounds
├── safeBounds?
├── coordinateSpace
├── textLayout
├── backgroundTreatment?
├── visibility
├── collisionState?
├── readabilityState
├── fallback?
└── issues[]?
```

---

# 71. Overlay Strategy Rule

`strategy` uses:

```text
OverlayPlacementStrategy
```

and MUST be compatible with:

```text
geometry availability

readability policy

target capability

profile policy
```

---

# 72. Replace Strategy Requirement

`Replace` SHOULD require either:

```text
validated SpeechBubble safe area
```

or another explicitly accepted replacement geometry.

A plain OCR text bounding box is insufficient by default.

---

# 73. BackgroundTreatment

```text
BackgroundTreatment
- Transparent
- Solid
- SemiTransparent
- Blur
- SampledFill
- BubbleFill
- ReconstructedBackground
- None
- Unknown
```

`ReconstructedBackground` does not imply Presentation owns image reconstruction.

---

# 74. TextLayoutPlan

```text
TextLayoutPlan
├── text
├── fontSize
├── lineHeight
├── alignment
├── wrapping
├── lineCount?
├── overflowBehavior
├── orientation
├── readabilityState
└── fitIterationCount?
```

Implementation-specific glyph layout stays internal/UI-side as appropriate.

---

# 75. TextOrientation

```text
TextOrientation
- Horizontal
- Vertical
- Rotated
- SourceAligned
- Unknown
```

Vietnamese target Presentation should normally prefer horizontal output unless an explicit policy says otherwise.

---

# 76. SfxPresentationPolicy

```text
SfxPresentationPolicy
- PreserveSource
- ReplaceWhenSafe
- AdjacentTranslation
- Annotation
- OnDemand
- HideTranslation
```

Translation owns SFX meaning.

Presentation owns SFX display strategy.

---

# 77. CollisionState

```text
CollisionState
- None
- Detected
- Resolved
- RequiresFallback
- Unresolved
```

Collision status does not modify source geometry authority.

---

# 78. ReadabilityState

```text
ReadabilityState
- Readable
- ReducedButReadable
- RequiresExpansion
- RequiresAlternateStrategy
- Unreadable
```

---

# 79. OverflowBehavior

```text
OverflowBehavior
- Wrap
- Expand
- Scroll
- FocusedOverlay
- Adjacent
- Floating
- SidePanel
- Hide
```

`Hide` is only valid where product semantics explicitly permit it.

---

# 80. PresentationFallback

```text
PresentationFallback
├── requestedMode
├── effectiveMode
├── requestedOverlayStrategy?
├── effectiveOverlayStrategy?
├── reasonCode
├── affectedItemIds[]?
├── automatic
└── degradedCapability?
```

Fallback must be observable.

---

# 81. CandidatePresentationState

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

* immutable after preparation;
* never exposed as current Presentation;
* can be discarded safely;
* does not imply Runtime authority;
* does not imply UI apply;
* Candidate Snapshot and RenderPlan revisions match.

---

# 82. PresentationChangeSet

```text
PresentationChangeSet
├── addedItemIds[]
├── updatedItemIds[]
├── removedItemIds[]
├── layoutChanged
├── modeChanged
├── overlayStrategyChanged
├── styleChanged
├── visibilityChanged
├── focusChanged
└── completenessChanged
```

It describes semantic differences.

It is not a native UI mutation patch.

---

# 83. PresentationCommitRequest

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

---

# 84. AuthorityRevalidationResult

```text
AuthorityRevalidationResult
├── status
├── runtimeRevisionId?
├── reasonCode?
└── evaluatedAt
```

Possible statuses:

```text
Accepted

RejectedStale

RejectedCanceled

RejectedSessionInactive

RejectedRuntimeRevision

RejectedTargetInvalidated

RejectedOther
```

Runtime/Application authority evaluation is final.

---

# 85. PresentationCommitResult

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

Statuses:

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

# 86. Atomic Commit Rule

Commit atomically advances:

```text
PresentationRevision
+
PresentationSnapshot
+
RenderPlan
```

Forbidden:

```text
new Snapshot + old RenderPlan

old Snapshot + new RenderPlan

new Revision + incomplete semantic state
```

---

# 87. Preserve Previous Rule

During preparation:

```text
previous committed Presentation
```

remains authoritative.

Recoverable candidate failure normally means:

```text
discard candidate
+
keep previous committed Presentation
```

---

# 88. Common Command Envelope

```text
requestId

contractVersion

issuedAt

presentationContextId

runtimeExecutionIdentity?

cancellationContextRef?
```

Runtime metadata is optional for purely Presentation-local operations unless required by integration.

---

# 89. BuildPresentation

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

Validation includes:

```text
context valid

required Artifacts present

content lineage compatible

target valid

viewport valid

profile valid

typed semantic mapping resolvable

no upstream Candidate used as authority
```

---

# 90. BuildPresentationPreparedResult

```text
BuildPresentationPreparedResult
├── requestId
├── operationId
└── candidate
```

Prepared does not mean committed.

---

# 91. UpdatePresentationContent

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

RecognitionGeometryUpdated

ArtifactReplaced

PartialResultAdvanced

ManualRefresh
```

---

# 92. Update Identity Rules

During compatible update:

```text
unchanged PresentationItems
    preserve IDs

unchanged SourceSemanticRefs
    preserve owner identity

unchanged TranslationUnits
    preserve Translation identity
```

Full internal rebuild is permitted if public invariants remain valid.

---

# 93. RecomputePresentationLayout

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

Reasons:

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

GeometryUpdated

ManualRefresh
```

---

# 94. Layout Recompute Rules

* semantic item identity stable;
* semantic source order stable;
* obsolete reflow may be coalesced;
* unsafe overlay may fall back;
* stale viewport Candidate cannot commit.

---

# 95. ChangePresentationMode

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

---

# 96. ChangeOverlayStrategy

Optional explicit Presentation-local command:

```text
ChangeOverlayStrategy
├── requestId
├── contractVersion
├── issuedAt
├── presentationContextId
├── presentationId
├── expectedPresentationRevision
├── presentationItemIds[]?
├── requestedStrategy
├── target
├── viewport?
└── profile?
```

This changes Presentation layout semantics only.

It does not change TranslationArtifact.

---

# 97. UpdatePresentationFocus

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

Raw OS/frontend input events are forbidden.

---

# 98. ApplyPresentationProfile

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

---

# 99. ClearPresentation

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

Reasons may include:

```text
SessionStopped

SessionReplaced

ContentReplaced

TargetDestroyed

PrivacyInvalidation

ApplicationShutdown

UserRequested
```

---

# 100. PresentationPreparationResult

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

These do not represent Runtime Attempt terminal authority.

---

# 101. PresentationCommandRejection

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

Presentation-owned reasons may include:

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

PRESENTATION_UNSUPPORTED_OVERLAY_STRATEGY

PRESENTATION_TARGET_CAPABILITY_MISSING

PRESENTATION_INVALID_PROFILE

PRESENTATION_EMPTY_INPUT

PRESENTATION_CANDIDATE_INVALID

PRESENTATION_UNREADABLE_LAYOUT

PRESENTATION_BUBBLE_GEOMETRY_REQUIRED
```

Exact final codes remain synchronized with `ERRORS.md`.

---

# 102. Runtime Error Separation

Presentation SHOULD NOT recreate:

```text
SESSION_NOT_ACTIVE

RUNTIME_REVISION_STALE

ATTEMPT_CANCELLED

WORKITEM_SUPERSEDED

RETRY_EXHAUSTED
```

These remain Runtime-owned facts.

---

# 103. Idempotency

* `ClearPresentation` is idempotent.
* Duplicate request handling must not create conflicting committed revisions.
* Re-executed candidate preparation may use a new candidate ID.
* Observable commit behavior must remain deterministic.

---

# 104. Query — GetCurrentPresentation

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

Snapshot and RenderPlan must share the same revision.

---

# 105. Query — GetPresentationSnapshot

```text
GetPresentationSnapshot
├── presentationId
└── presentationRevision?
```

---

# 106. Query — GetRenderPlan

```text
GetRenderPlan
├── presentationId
└── presentationRevision?
```

---

# 107. Query — GetPresentationItem

```text
GetPresentationItem
├── presentationId
├── presentationRevision?
└── presentationItemId
```

---

# 108. Query — GetPresentationSummary

```text
PresentationSummary
├── presentationId?
├── presentationRevision?
├── contentIdentity?
├── effectiveMode?
├── completeness?
├── itemCount
├── markerCount
├── overlayCount
├── overflowCount
├── issueCount
└── targetId?
```

---

# 109. Query — GetPresentationDiagnostics

Diagnostics MUST respect privacy.

Full source/Translation content MUST NOT be returned by default.

---

# 110. UI Apply Request

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

---

# 111. UI Apply Result

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

Statuses:

```text
Applied

RejectedStale

RejectedTargetMismatch

TargetUnavailable

Failed
```

---

# 112. Presentation Commit vs UI Apply

These are separate authorities:

```text
Presentation Commit
    ↓
semantic Presentation current

UI Apply
    ↓
frontend reflects it
```

Valid state:

```text
Presentation committed
+
UI apply failed
```

must be observable.

---

# 113. Input Validation

Presentation rejects or degrades when:

```text
required identifiers absent

Artifact unsupported

Artifact lineage incompatible

semantic refs unresolved

PresentationRevision conflict

coordinate space absent

geometry invalid

transform invalid

viewport invalid

target capability insufficient

mode unsupported

overlay strategy incompatible

mandatory readability impossible

Bubble-safe geometry required but unavailable

candidate invariants violated
```

---

# 114. Runtime Authority Validation

At commit:

```text
Candidate
    ↓
Runtime/Application Authority Service
    ↓
Accepted / Rejected
```

Presentation MUST NOT determine current Runtime authority from private state alone.

---

# 115. Presentation Revision Validation

Presentation owns optimistic concurrency for PresentationRevision.

For MVP:

```text
expectedPresentationRevision
!=
currentPresentationRevision
```

causes conflict/supersession unless explicit deterministic merge is supported.

---

# 116. Target Revision Validation

Layout-sensitive candidate must verify target compatibility before commit.

Stale target state must not create unsafe overlays.

---

# 117. Viewport Revision Validation

High-frequency viewport changes may coalesce older candidates without treating them as errors.

---

# 118. Candidate Invariant Validation

Before commit verify:

```text
candidate identity valid

Presentation identity valid

Snapshot/RenderPlan revision match

SourceSemanticRefs resolve

TranslationUnitRefs resolve

PresentationItem IDs unique

Marker IDs unique

semantic sequence valid

geometry valid

Bubble refs valid when supplied

overlay strategy valid

readability policy satisfied or fallback applied

target compatible

no native/framework object
```

---

# 119. Compatibility Metadata

```text
PresentationCompatibilityMetadata
├── presentationContractVersion
├── sourceArtifactContractVersions[]
├── translationArtifactContractVersion?
├── presentationProfileVersion?
├── targetCapabilityVersion?
├── geometryTransformVersion?
├── strategyVersion
├── overlayPolicyVersion?
├── typographyPolicyVersion?
├── locale?
└── privacyPartition?
```

This describes semantic dependencies.

It does not determine Runtime authority.

---

# 120. Determinism Contract

Equivalent fixed:

```text
Artifact set

ContentIdentity

PresentationProfile

PresentationTarget

ViewportSnapshot

strategy version

overlay policy version
```

should produce semantically equivalent:

```text
item mapping

logical ordering

mode resolution

overlay strategy

fallback decision

layout classification

PresentationSnapshot

RenderPlan
```

where algorithms are deterministic.

---

# 121. Partial Presentation

Presentation may represent:

```text
source only

waiting

partial Translation

complete Translation

corrected Translation

failed Translation item
```

only from accepted upstream semantics.

Raw provider streams are not Presentation authority.

---

# 122. Correction Precedence

Accepted higher-priority/manual correction must not be overwritten by older automatic output.

Presentation follows upstream semantic lineage/revision authority.

---

# 123. Reading Order Contract

Presentation consumes canonical semantic reading order.

Presentation may define separate:

```text
semanticSequence

visualPosition
```

It MUST NOT silently rewrite source semantic order.

---

# 124. Geometry Contract

Public geometry requires explicit coordinate space.

Invalid:

```text
non-finite values

negative dimensions

required Unknown coordinate space

unsupported transform

incompatible source/target revision

zero-size required overlay area
```

Invalid overlay geometry may still allow SidePanel presentation.

---

# 125. Bubble Contract

A Source Region and Speech Bubble are separate semantic references.

Invariant:

```text
SourceRegionRef
    MUST NOT
implicitly satisfy
SpeechBubbleRef
```

`Replace` layout may require Bubble-safe geometry depending on policy.

---

# 126. Readability Contract

Presentation MUST:

```text
respect minimum readable size

bound fitting iterations

preserve full semantic Translation

fallback when necessary
```

Presentation MUST NOT:

```text
shrink indefinitely

truncate Translation meaning

invent larger Bubble geometry
```

---

# 127. Vertical Text Contract

Source orientation may be preserved as provenance.

Target Presentation orientation is chosen independently.

For Vietnamese target content, horizontal presentation is the default unless explicitly configured.

---

# 128. SFX Contract

SFX presentation strategy is explicit and separate from Translation semantics.

Presentation must be able to preserve artwork without forcing destructive replacement.

---

# 129. Collision Contract

Collision resolution may:

```text
reposition

change overlay strategy

change mode

fallback
```

It MUST NOT mutate:

```text
source geometry authority

semantic Reading Order

Translation semantics
```

---

# 130. Security Contract

Presentation MUST NOT:

```text
store provider credentials

call Translation/OCR provider APIs directly

execute scripts contained in content

expose native handles

execute content callbacks

bypass Artifact access policy
```

---

# 131. Privacy Contract

Presentation may temporarily process:

```text
source text

translated text

geometry

corrections

Presentation profile
```

Normal diagnostics must not contain:

```text
screenshots

complete source documents

complete TranslationArtifact content

provider prompts

credentials

private window titles
```

---

# 132. Resource Contract

Presentation may temporarily hold:

```text
Artifact leases

semantic mapping indexes

layout state

CandidatePresentationState

current committed Presentation

previous committed Presentation
```

Artifact lease does not transfer Artifact ownership.

---

# 133. Performance Contract

Public design supports measurement of:

```text
candidate preparation duration

semantic mapping duration

layout duration

fit duration

geometry projection duration

collision resolution duration

authority revalidation latency

commit latency

UI apply latency

coalesced operation count

superseded Candidate count

Presentation memory
```

---

# 134. Event Boundary

Success facts describe already committed state.

Correct order:

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

Never publish success before commit.

---

# 135. Event Bus Is Not Orchestrator

Presentation MUST NOT rely on:

```text
TranslationCompleted
    ↓
Presentation automatically executes
```

Runtime/Application determines required work.

---

# 136. Presentation Success Facts

Exact event names belong to `EVENTS.md`.

Conceptual committed facts include:

```text
PresentationPrepared

PresentationUpdated

PresentationLayoutChanged

PresentationModeChanged

PresentationCleared
```

They reference committed Presentation identity/revision.

---

# 137. Rejection vs Failure

`PresentationRejected` conceptually represents:

```text
invalid command

invalid Candidate

optimistic concurrency conflict

unsupported capability
```

`PresentationFailed` should be reserved for Presentation-owned internal/unrecoverable semantic failure.

Runtime cancellation/staleness is not automatically Presentation failure.

---

# 138. Contract Compatibility

Presentation contracts remain independent from:

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

---

# 139. Contract Versioning

Semantic versioning:

```text
MAJOR.MINOR.PATCH
```

This revision is **3.0.0** because it changes public semantic references:

```text
SourceSegmentRef
    → SourceSemanticRef

TranslationSegmentRef
    → TranslationUnitRef

PresentationItem segment arrays
    → typed semantic refs
```

and adds explicit Bubble/overlay strategy semantics.

---

# 140. Patch Changes

May include:

```text
clarification

documentation correction

compatible validation correction

new optional diagnostics
```

---

# 141. Minor Changes

May include backward-compatible:

```text
optional fields

optional commands

optional capabilities

queries

events
```

---

# 142. Major Changes

Required for incompatible changes to:

```text
required fields

public semantic reference identity

PresentationRevision meaning

commit model

ownership semantics

Artifact boundary

immutability semantics
```

---

# 143. Unknown Fields

Unknown optional fields may be ignored when safe.

Unknown required semantic enum values must:

```text
reject
```

or use an explicitly documented compatibility fallback.

Never silently reinterpret them.

---

# 144. Architecture Invariants

1. Presentation does not perform OCR.

2. Presentation does not perform Text Processing.

3. Presentation does not perform Translation.

4. Presentation consumes accepted immutable Artifacts.

5. Presentation does not consume provider-native Translation output.

6. Presentation does not own RuntimeRevision authority.

7. Presentation does not own WorkItem lifecycle.

8. Presentation does not own Attempt lifecycle.

9. Presentation does not own Runtime retry.

10. Presentation does not own cancellation authority.

11. PresentationOperationId is not WorkItemId.

12. PresentationRevision is not RuntimeRevisionId.

13. Candidate is not committed Presentation.

14. Prepared does not mean committed.

15. Committed does not mean UI applied.

16. Runtime authority must be valid at commit.

17. Presentation cannot override Runtime authority rejection.

18. Presentation owns PresentationRevision optimistic concurrency.

19. Snapshot and RenderPlan commit atomically.

20. Previous committed Presentation remains authoritative during preparation.

21. Stale Candidate cannot overwrite newer Presentation.

22. Upstream Artifacts remain immutable.

23. Presentation does not publish upstream Artifacts.

24. Presentation does not own native surface resources.

25. Presentation does not own DOM/widget instances.

26. Coordinate spaces are explicit.

27. Item identity is independent from array position.

28. Semantic Reading Order is not rewritten by layout.

29. Partial Translation is explicit.

30. Accepted correction outranks older automatic content.

31. Readability outranks forced exact placement.

32. Event Bus does not replace Runtime orchestration.

33. Success facts describe committed state.

34. Standard diagnostics do not contain complete reading content.

35. Generic SourceSegmentRef is removed.

36. Generic TranslationSegmentRef is removed.

37. Text Processing nodes are referenced through SourceSemanticRef.

38. Translation semantic units are referenced through TranslationUnitRef.

39. Presentation does not own TranslationUnit semantics.

40. Text Region is not Speech Bubble Region.

41. SourceRegionRef must not implicitly satisfy SpeechBubbleRef.

42. Presentation does not own Bubble detection.

43. Replace strategy must use valid replacement geometry.

44. Missing Bubble geometry must not be fabricated.

45. PresentationMode and OverlayPlacementStrategy are distinct.

46. Text fitting is bounded.

47. Minimum readable font size is enforced.

48. Translation meaning is not truncated for fitting.

49. Vertical source text does not force vertical Vietnamese target text.

50. SFX strategy is Presentation-owned display semantics.

51. Collision recovery cannot rewrite source truth.

52. Published Presentation values are immutable.

53. Cache does not create Presentation authority.

54. Frontend implementation does not leak into contract types.

---

# 145. Deprecated v2 Contract Concepts

Deprecated and removed:

```text
SourceSegmentRef

TranslationSegmentRef

sourceSegmentIds[]

translationSegmentIds[]
```

Deprecated implication:

```text
SourceBlock
    contains generic Segments
```

Current:

```text
SourceBlockRef
    references typed semantic nodes
```

Deprecated implication:

```text
Translated output
    identified primarily by TranslationSegment
```

Current:

```text
TranslationUnit
    is canonical Translation semantic unit
```

---

# 146. Migration — SourceSegmentRef

Old:

```text
SourceSegmentRef
├── sourceSegmentId
├── sourceBlockId?
├── sourceDocumentArtifactRef
└── ...
```

New:

```text
SourceSemanticRef
├── nodeId
├── nodeKind
├── sourceDocumentArtifactRef
└── ...
```

Migration must determine the canonical Text Processing node type.

Do not invent a generic segment when the source model exposes:

```text
Block

Paragraph

Sentence

Span
```

---

# 147. Migration — TranslationSegmentRef

Old:

```text
TranslationSegmentRef
├── translationSegmentId
├── translationUnitId?
└── ...
```

New:

```text
TranslationUnitRef
├── translationUnitId
└── ...
```

The optional `translationUnitId` becomes mandatory semantic identity.

---

# 148. Migration — PresentationItem

Old:

```text
PresentationItem
├── sourceBlockIds[]
├── sourceSegmentIds[]
├── translationUnitIds[]
├── translationSegmentIds[]
└── recognitionRegionIds[]
```

New:

```text
PresentationItem
├── sourceSemanticRefs[]
├── sourceBlockRefs[]?
├── translationUnitRefs[]
├── recognitionRegionRefs[]?
└── speechBubbleRefs[]?
```

---

# 149. Example — Initial Build

```text
Published RecognitionArtifact
        +
Published SourceDocumentArtifact
        +
Published TranslationArtifact
        ↓
BuildPresentation
        ↓
resolve SourceSemanticRefs
        ↓
resolve TranslationUnitRefs
        ↓
map PresentationItems
        ↓
resolve effective mode
        ↓
resolve overlay strategies
        ↓
CandidateSnapshot
+
CandidateRenderPlan
        ↓
authority revalidation
        ↓
atomic commit
        ↓
PresentationRevision 1
        ↓
UI Adapter apply
```

---

# 150. Example — Comic Overlay With Bubble Geometry

```text
TranslationUnit T10
        ↓
SourceSemanticRef Sentence S5
        ↓
SourceRegion R8
        +
SpeechBubble B3
        ↓
PresentationItem P4
        ↓
Overlay strategy evaluation
        ↓
Bubble safe area valid
        ↓
Replace / Cover candidate
        ↓
fit validation
        ↓
RenderPlan
```

---

# 151. Example — Comic Overlay Without Bubble Geometry

```text
TranslationUnit T10
        ↓
SourceRegion R8
        ↓
no SpeechBubbleRef
        ↓
Presentation MUST NOT assume
R8 == Bubble
        ↓
COVER / ADJACENT / FLOATING
        ↓
readability validation
```

---

# 152. Example — Translation Too Long

```text
TranslationUnit
        ↓
overlay candidate
        ↓
wrap
        ↓
still overflow
        ↓
bounded font reduction
        ↓
minimum readable size reached
        ↓
alternate strategy
        ↓
Adjacent / Floating / SidePanel
```

Not:

```text
truncate Translation
```

---

# 153. Example — Vertical Chinese Source

```text
vertical Chinese Region
        ↓
TranslationUnit Vietnamese
        ↓
Presentation
        ↓
horizontal target layout
        ↓
adjacent / overlay / floating
```

Source orientation remains available as provenance.

---

# 154. Example — Partial Translation Update

```text
PresentationRevision 4
        ↓
new Published TranslationArtifact
        ↓
UpdatePresentationContent
        ↓
existing compatible TranslationUnitRefs preserved
        ↓
changed units remapped
        ↓
CandidateRevision 5
        ↓
authority validation
        ↓
commit
```

---

# 155. Example — Viewport Coalescing

```text
Viewport 20
Viewport 21
Viewport 22
        ↓
20 obsolete
21 obsolete
        ↓
recompute using 22
        ↓
Candidate
        ↓
commit if still current
```

Discarded viewport candidates are normal supersession.

---

# 156. Example — Runtime Supersession

```text
RuntimeRevision 14
        ↓
Presentation Candidate
        ↓
RuntimeRevision 15 becomes current
        ↓
Candidate reaches commit
        ↓
AuthorityRevalidation
        ↓
RejectedStale
        ↓
discard
        ↓
current Presentation unchanged
```

---

# 157. Example — UI Apply Failure

```text
Presentation committed
        ↓
UI Adapter Apply
        ↓
TargetUnavailable
```

Semantic Presentation remains valid.

UI projection failure is observable separately.

---

# 158. Related Documents

```text
doc/01-architecture/core/
├── DATA_FLOW.md
├── STATE_MACHINE.md
├── EVENT_BUS.md
└── EVENT_CONVENTION.md

doc/01-architecture/text/
├── TEXT_MODEL.md
└── SEGMENTATION.md

doc/01-architecture/translate/
├── TRANSLATION.md
└── CONTEXT.md

doc/01-architecture/ocr/
└── READING_ORDER.md

doc/02-modules/presentation/
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
├── ERRORS.md
└── README.md

doc/02-modules/
├── recognition/
├── text-processing/
├── translation/
├── preferences/
├── reading-session/
└── ui-adapter/

doc/01-architecture/runtime/
```

---

# 159. Open Decisions

The following remain intentionally open:

```text
final SourceSemanticRef field set

whether Token refs are ever exposed to Presentation

final TranslationUnitRef contract

Bubble semantic owner

Bubble-safe-area schema

Bubble/Panel association contract

whether OverlayPlacementStrategy
becomes fully public configuration

background reconstruction capability owner

TextMeasurementPort design

font measurement boundary

partial/provisional Presentation model

overlay collision algorithm

Bubble-shape-aware fitting

SFX typography policy

viewport-class cache model

Presentation engine version contract
```

---

# 160. Completion Criteria

The Presentation Contract is synchronized when:

* `SourceSegmentRef` is removed;
* `TranslationSegmentRef` is removed;
* `sourceSegmentIds[]` is removed;
* `translationSegmentIds[]` is removed;
* Text Processing nodes use typed `SourceSemanticRef`;
* Translation uses `TranslationUnitRef`;
* PresentationItem uses typed semantic refs;
* `Text Region ≠ Speech Bubble Region` is explicit;
* optional `SpeechBubbleRef` does not transfer ownership;
* PresentationMode and OverlayPlacementStrategy remain distinct;
* overlay contract supports strategy/fallback semantics;
* bounded text fitting is enforceable;
* minimum readable typography is explicit;
* fitting cannot truncate Translation meaning;
* vertical text and SFX presentation boundaries are explicit;
* Snapshot + RenderPlan atomic commit remains unchanged;
* Runtime authority remains external;
* UI Adapter remains native projection owner;
* contract remains serializable/platform-neutral.

---

# 161. Summary

The v2 Contract correctly established:

```text
Artifact References
    ↓
Candidate Presentation
    ↓
Authority Revalidation
    ↓
Atomic Commit
    ↓
PresentationSnapshot + RenderPlan
```

but still retained legacy:

```text
SourceSegmentRef

TranslationSegmentRef

sourceSegmentIds[]

translationSegmentIds[]
```

The v3 contract replaces them with:

```text
SourceSemanticRef
    → Text Processing-owned semantic node

TranslationUnitRef
    → Translation-owned semantic unit
```

and updates:

```text
PresentationItem
```

to reference those typed semantic authorities directly.

For visual Presentation it additionally locks:

```text
SourceRegionRef
    ≠
SpeechBubbleRef
```

and:

```text
PresentationMode
    ≠
OverlayPlacementStrategy
```

while preserving:

```text
readability
    >
forced exact geometry
```

The canonical boundary remains:

```text
TranslationArtifact
        ↓
Presentation
        ↓
Candidate
        ↓
Semantic Validation
        ↓
Runtime Authority Revalidation
        ↓
Atomic Commit
        ↓
PresentationSnapshot
+
RenderPlan
        ↓
UI Adapter
```

The core ownership rule is:

```text
Text Processing
    owns source semantic nodes

Translation
    owns TranslationUnit

Presentation
    owns PresentationSnapshot + RenderPlan

UI Adapter
    owns concrete projection

Runtime
    owns execution authority
```
