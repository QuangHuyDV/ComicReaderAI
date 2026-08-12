# CRAI Presentation Module

> **Project:** CRAI
> **Path:** `doc/02-modules/presentation/MODULE.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Module Owner:** Presentation
> **Runtime Model:** Runtime v2 aligned
> **Last Updated:** 2026-08-10

---

# 1. Purpose

The Presentation module owns CRAI's semantic presentation model.

It transforms authoritative translated content and referenced source semantics into a stable, platform-neutral presentation representation suitable for projection by UI Adapter.

Canonical flow:

```text
Published TranslationArtifact
        ↓
Presentation
        ↓
PresentationSnapshot Candidate
        +
RenderPlan Candidate
        ↓
Semantic Validation
        ↓
Runtime Authority Revalidation
        ↓
Atomic Commit
        ↓
Published Presentation State
        ↓
UI Adapter
        ↓
ViewModel / Native UI
```

Presentation answers:

```text
What should be presented?

How should it be semantically arranged?

Which presentation mode applies?

Which source and Translation content belong together?

How should image/text content be positioned?

What fallback is required when ideal presentation is impossible?
```

Presentation does not answer:

```text
How should Runtime execute this work?

How should Translation determine target-language meaning?

How should OCR recognize source text?

How should a concrete frontend widget be rendered?
```

---

# 2. Ownership

Presentation owns:

```text
PresentationSnapshot

RenderPlan

PresentationItem

PresentationMode

semantic presentation ordering

source/Translation association

presentation geometry

overlay placement semantics

typography constraints

readability policy

overflow policy

presentation fallback

partial/degraded presentation semantics

presentation revision

presentation Candidate validation

semantic commit state
```

Presentation does not own:

```text
CaptureArtifact

RecognitionArtifact

SourceDocumentArtifact

TranslationArtifact

TranslationUnit

Translation Context

Runtime WorkItem

Runtime Attempt

RuntimeRevisionId

Runtime retry

Runtime cancellation

native ViewModel

frontend component state

native drawing

persistent Preferences
```

---

# 3. Core Boundary

The core module boundary is:

```text
Translation
    owns target-language semantics
        ↓
Presentation
    owns presentation semantics
        ↓
UI Adapter
    owns frontend projection
```

Therefore:

```text
TranslationArtifact
    ≠
PresentationSnapshot
    ≠
ViewModel
```

Each belongs to a different semantic owner.

---

# 4. Primary Inputs

Presentation primarily consumes:

```text
Published TranslationArtifact
```

and may follow explicit references to:

```text
SourceDocumentArtifact

RecognitionArtifact

CaptureArtifact
```

when required for:

```text
original source text

source structure

source geometry

Recognition regions

Panel/Bubble relationships

orientation

source provenance
```

Presentation must not reconstruct these upstream artifacts.

---

# 5. Primary Outputs

Presentation produces two closely related semantic outputs:

```text
PresentationSnapshot
+
RenderPlan
```

They represent one coherent committed Presentation state.

Conceptually:

```text
PresentationSnapshot
    = what is presented

RenderPlan
    = how that semantic presentation
      should be laid out/projected
```

They must correspond to the same Presentation revision.

---

# 6. PresentationSnapshot

`PresentationSnapshot` represents the immutable semantic presentation state.

Conceptually:

```text
PresentationSnapshot
├── PresentationRevision
├── TranslationArtifactRef
├── SourceArtifactRefs[]
├── Mode
├── Items[]
├── LogicalOrder
├── Completeness
├── Warnings[]
├── ConfigurationRef
└── Provenance
```

Exact schema belongs to:

```text
CONTRACT.md
```

---

# 7. RenderPlan

`RenderPlan` describes platform-neutral layout/render intent.

Conceptually:

```text
RenderPlan
├── PresentationRevision
├── Target
├── ViewportSnapshot
├── LayoutPlan
├── OverlayPlans[]
├── TypographyProfile
├── GeometryMappings[]
├── OverflowPlans[]
├── Fallbacks[]
└── Warnings[]
```

RenderPlan must not contain native frontend objects.

---

# 8. Atomic Presentation State

`PresentationSnapshot` and `RenderPlan` must be committed together.

Forbidden:

```text
PresentationSnapshot revision P2
+
RenderPlan revision P1
```

Canonical invariant:

```text
SnapshotRevision
    =
RenderPlanRevision
```

for one committed Presentation state.

---

# 9. PresentationItem

A `PresentationItem` is the semantic unit exposed by Presentation.

It may represent:

```text
translated Paragraph

dialogue

caption

heading

comic Bubble content

text Region

SFX annotation

overlay content

source/Translation comparison item
```

Conceptually:

```text
PresentationItem
├── PresentationItemId
├── SourceSemanticRefs[]
├── TranslationUnitRefs[]
├── LogicalOrder
├── Content
├── ContentRole
├── PresentationState
├── LayoutIntent
├── StyleIntent
├── GeometryRef?
└── Warnings[]
```

---

# 10. Typed Semantic References

Presentation must not rely on one universal legacy:

```text
segmentId
```

as its primary semantic identity.

Prefer typed references to actual owners:

```text
SourceDocument node

TranslationUnit

RecognitionRegion

PresentationItem
```

Examples:

```text
BlockRef

ParagraphRef

SentenceRef

SpanRef

TranslationUnitRef

RecognitionRegionRef
```

Exact public reference types belong to `CONTRACT.md`.

---

# 11. No Generic Segment Authority

Deprecated:

```text
SourceSegment
TranslationSegment
PresentationSegment
```

as universal cross-module architecture objects.

Presentation consumes explicit semantic references rather than recreating a global Segment abstraction.

---

# 12. Presentation Modes

Canonical high-level Presentation modes are:

```text
SIDE_PANEL

TEXT_READER

OVERLAY

HYBRID
```

These represent user-visible presentation families.

They are distinct from low-level overlay strategies.

---

# 13. Side Panel

`SIDE_PANEL` presents Translation separately from source content.

Useful when:

```text
source geometry is unsuitable

overlay would obscure artwork

Translation is long

Recognition confidence is low

viewport is constrained
```

---

# 14. Text Reader

`TEXT_READER` presents structured translated content as a reading document.

Primary use:

```text
web novels

plain text

structured browser text

documents

clipboard text
```

It prioritizes:

```text
paragraph continuity

dialogue readability

semantic ordering

comfortable typography

long-form reading
```

---

# 15. Overlay

`OVERLAY` associates translated content spatially with image/source regions.

Primary use:

```text
comics

manga

manhua

manhwa

screen capture

scanned pages

image-only content
```

---

# 16. Hybrid

`HYBRID` combines multiple Presentation strategies.

Examples:

```text
safe Bubbles
    → overlay

unsafe/small Bubbles
    → side panel
```

or:

```text
main dialogue
    → overlay

long narration
    → reader/panel
```

---

# 17. Mode vs Overlay Strategy

Do not confuse:

```text
PresentationMode
```

with:

```text
OverlayPlacementStrategy
```

Example:

```text
PresentationMode = OVERLAY

OverlayPlacementStrategy = ADJACENT
```

The first is high-level Presentation semantics.

The second is an internal spatial strategy.

---

# 18. Overlay Placement Strategies

Presentation may internally support:

```text
REPLACE

COVER

ADJACENT

FLOATING

TOOLTIP

ON_DEMAND
```

These strategies do not need to become top-level public Presentation modes.

---

# 19. Replace Strategy

`REPLACE` means translated text semantically occupies the original text area.

It is valid only when Presentation has sufficient evidence that:

```text
usable replacement geometry exists

background treatment is available

Translation remains readable

important artwork is not damaged
```

---

# 20. Cover Strategy

`COVER` places a readable presentation surface over the source text area.

This is a practical early strategy when:

```text
full text removal

or

background reconstruction
```

is unavailable.

---

# 21. Adjacent Strategy

`ADJACENT` places translated content near the corresponding source region.

Useful when:

```text
source region is too small

artwork should remain visible

Translation expansion is large

Bubble geometry is uncertain
```

---

# 22. Floating Strategy

`FLOATING` uses a positioned external surface linked to source content.

Useful when:

```text
page density is high

overlays collide

geometry is uncertain

source regions are very small
```

---

# 23. Tooltip Strategy

`TOOLTIP` exposes Translation temporarily through explicit interaction.

It may be useful for:

```text
comparison

language learning

low-clutter presentation
```

It must not be the only accessible mechanism where hover is unavailable.

---

# 24. On-Demand Strategy

`ON_DEMAND` keeps translated content hidden until explicitly requested.

Useful when:

```text
artwork preservation is prioritized

user wants source-first reading

visual clutter must be minimized
```

---

# 25. Strategy Selection

Overlay strategy may depend on:

```text
source Region size

Bubble geometry

translated text length

orientation

artwork complexity hint

Recognition confidence

available presentation space

collision state

user preference

accessibility requirements
```

Selection must remain deterministic for equivalent semantic inputs and configuration where practical.

---

# 26. Geometry Ownership

Presentation consumes geometry.

It does not become the owner of upstream source geometry.

Canonical relationship:

```text
Capture / Recognition
    owns source geometry
        ↓
Presentation
    references geometry
        ↓
UI Adapter
    maps it to concrete viewport/native coordinates
```

---

# 27. Source-Relative Geometry

Reusable Presentation geometry should remain source-relative whenever practical.

```text
Source Coordinates
    ↓
Presentation Geometry
    ↓
Viewport Transform
    ↓
Native Rendering
```

Changing:

```text
window size

zoom

device pixel ratio

scroll position
```

must not mutate canonical source geometry.

---

# 28. Coordinate Spaces

Every geometry reference must identify its coordinate space.

Examples:

```text
CAPTURE

SOURCE_IMAGE

DOCUMENT_PAGE

NORMALIZED_SOURCE

VIEWPORT
```

Implicit coordinate-space mixing is forbidden.

---

# 29. ViewportSnapshot

Presentation may consume an immutable:

```text
ViewportSnapshot
```

containing the stable presentation constraints required for one calculation.

It must not read mutable frontend state continuously during an Attempt.

---

# 30. Viewport vs Source Geometry

These are different:

```text
SourceGeometry
    = semantic location in source

ViewportSnapshot
    = current presentation constraints
```

Presentation maps between them without rewriting source truth.

---

# 31. Critical Geometry Invariant

A fundamental invariant is:

```text
Text Region
    ≠
Speech Bubble Region
```

An OCR/Recognition text bounding box identifies recognized text extent.

It does not automatically identify:

```text
full Bubble boundary

safe Bubble interior

available translated-text area
```

---

# 32. Forbidden Bubble Assumption

Forbidden:

```text
Recognition TextBox
    ↓
assume entire Speech Bubble
    ↓
fit Translation directly
```

This may cause:

```text
tiny translated text

incorrect background cover

artwork obstruction

bad alignment

false Bubble ownership
```

---

# 33. Bubble Geometry

If Bubble semantics are available upstream, Presentation may consume:

```text
Bubble boundary

safe interior

Bubble orientation

tail location

associated text Region(s)

Panel association
```

through explicit contracts.

Presentation does not infer authoritative Bubble geometry merely because a text Region exists.

---

# 34. Missing Bubble Geometry

When Bubble geometry is unavailable:

```text
do not fabricate it
```

Presentation should choose safer strategies such as:

```text
COVER

ADJACENT

FLOATING

SIDE_PANEL
```

according to available evidence.

---

# 35. Bubble Detection Ownership

Presentation does not own Bubble detection.

Future Bubble semantics may belong to:

```text
Recognition

image-analysis capability

another explicitly assigned owner
```

Presentation consumes the result.

---

# 36. Semantic Reading Order

Presentation consumes canonical semantic reading order.

It does not reconstruct Reading Order from:

```text
XY sorting

OCR box order

DOM position guesses

visual proximity
```

when upstream semantic ordering already exists.

---

# 37. Logical vs Visual Order

Distinguish:

```text
Logical Reading Order

Visual Presentation Order

Physical Source Position
```

These may differ.

Presentation may visually reposition content while preserving logical reading order.

---

# 38. Native Text Presentation

Native text Presentation preserves meaningful structure such as:

```text
Title

Heading

Section

Paragraph

Dialogue

Quote

List

Caption

Separator
```

It does not copy source-native UI structures directly.

---

# 39. Paragraph Composition

Multiple TranslationUnits may belong to one semantic Paragraph.

Presentation may compose them visually as one Paragraph while preserving TranslationUnit references.

```text
TranslationUnit A
TranslationUnit B
TranslationUnit C
        ↓
Presentation Paragraph
```

---

# 40. Dialogue Presentation

Dialogue may receive semantic presentation intent such as:

```text
dialogue spacing

quotation formatting

speaker label

dialogue grouping

indentation
```

Speaker labels may only be shown when speaker semantics are authoritative enough for the configured policy.

Presentation does not invent speaker identity.

---

# 41. Original and Translation Arrangement

Native text may support arrangements such as:

```text
TRANSLATED_ONLY

SOURCE_ONLY

INTERLEAVED

SIDE_BY_SIDE
```

These are arrangement strategies within a Presentation mode rather than necessarily new top-level module modes.

---

# 42. Typography

Presentation owns semantic typography constraints.

Examples:

```text
preferred font size

minimum readable font size

line-height intent

paragraph spacing

alignment

maximum useful line width

emphasis role

source/Translation distinction
```

---

# 43. Font Ownership

Presentation does not own:

```text
installed font discovery

native font rendering

font rasterization

platform-specific font fallback
```

These belong below the semantic Presentation boundary.

---

# 44. Text Fitting

Image-based Translation often cannot fit directly into source geometry.

Presentation therefore owns bounded text-fitting policy.

Conceptual flow:

```text
Translated Content
    ↓
Preferred Typography
    ↓
Line Wrap
    ↓
Fit Evaluation
    ↓
Fits?
    ├── YES
    │    ↓
    │  Accept
    │
    └── NO
         ↓
      Reduce Font
      Within Readable Limit
         ↓
      Re-evaluate
         ↓
      Expand Safe Area?
         ↓
      Alternate Strategy
```

---

# 45. Readability Invariant

Presentation must never continuously shrink text merely to force it into original geometry.

Canonical rule:

```text
readability
    >
exact geometric replacement
```

---

# 46. Minimum Readable Font Size

Typography configuration defines:

```text
minimumReadableFontSize
```

Once reached, Presentation must choose another solution.

Forbidden:

```text
fontSize--
until text fits
```

without a bounded readable minimum.

---

# 47. Text-Fit Fallback Ladder

A typical fallback ladder is:

```text
wrap
    ↓
bounded font reduction
    ↓
safe-area expansion
    ↓
scrollable/focused presentation
    ↓
ADJACENT / FLOATING
    ↓
SIDE_PANEL
```

Exact availability depends on Presentation target and configuration.

---

# 48. Translation Semantics Must Not Be Truncated

Presentation must not solve fit failure by silently changing:

```text
Translation meaning
```

Forbidden:

```text
Translation too long
    ↓
truncate semantic content
```

Instead:

```text
Translation too long
    ↓
change Presentation strategy
```

---

# 49. Translation Length Hints

Presentation may provide advisory constraints to Translation such as:

```text
prefer concise wording

preferred line count

preferred approximate length
```

These are hints.

They do not grant Presentation authority over Translation semantics.

---

# 50. Overflow

Presentation owns semantic overflow policy.

Possible outcomes:

```text
WRAP

EXPAND

SCROLL

FOCUSED_OVERLAY

FLOATING

SIDE_PANEL
```

depending on mode and target.

---

# 51. Overflow Must Be Observable

When Presentation falls back due to overflow, the resulting state should preserve enough information for:

```text
diagnostics

UI indication where useful

reproducibility
```

---

# 52. Vertical Source Text

Source text may be vertical, especially in:

```text
Chinese comics

Japanese manga

stylized captions
```

Source orientation does not require Vietnamese Translation to preserve the same orientation.

---

# 53. Vietnamese Target Orientation

Default Vietnamese presentation should normally favor horizontal text.

Possible strategies for vertical source content:

```text
horizontal inside Bubble

horizontal adjacent

floating

side panel

on-demand
```

Avoid character-by-character vertical Vietnamese unless explicitly supported as a deliberate typography feature.

---

# 54. SFX Presentation

Sound effects may require specialized presentation.

Possible strategies:

```text
preserve source SFX

translated annotation

adjacent Translation

replacement when safe

on-demand Translation
```

Translation owns the target semantic result.

Presentation owns its display strategy.

---

# 55. Stylized SFX

Large stylized SFX may be part of artwork.

Presentation should not automatically cover the entire visual Region.

Prefer:

```text
annotation

adjacent label

on-demand display
```

when replacement would materially damage artwork.

---

# 56. Background Treatment

Overlay Presentation may require a background treatment.

Possible semantic intents:

```text
TRANSPARENT

SOLID

SEMI_TRANSPARENT

BLUR

SAMPLED_FILL

BUBBLE_FILL

RECONSTRUCTED_BACKGROUND
```

---

# 57. Background Reconstruction Boundary

Presentation may request/use a reconstructed background capability.

It does not own:

```text
image inpainting

text removal

image reconstruction model
```

Those belong to the capability/provider that produces such image data.

---

# 58. Collision Detection

Presentation layout may detect collisions such as:

```text
overlay ↔ overlay

overlay ↔ protected artwork

overlay ↔ source Region

overlay ↔ viewport boundary

floating label ↔ floating label
```

---

# 59. Collision Recovery

Possible recovery:

```text
reposition

reduce padding

stack

number/link markers

switch to ADJACENT

switch to FLOATING

switch to SIDE_PANEL
```

Logical reading order must remain intact.

---

# 60. Collision Does Not Change Source Truth

Presentation may reposition translated content.

It must not rewrite:

```text
source Region geometry

source Reading Order

Translation semantics
```

to solve a collision.

---

# 61. Partial Presentation

Presentation may represent partial translated content only when upstream Translation semantics explicitly permit partial publication.

It must not infer partial semantic truth from raw Runtime progress.

---

# 62. Provisional Presentation

If Translation exposes authoritative provisional content, Presentation may represent:

```text
PROVISIONAL
```

items.

Raw provider token streaming is not automatically a valid Presentation input.

---

# 63. PresentationItem State

Semantic item state may include concepts such as:

```text
READY

PROVISIONAL

MISSING_TRANSLATION

DEGRADED

HIDDEN
```

Exact enum belongs to `CONTRACT.md`.

---

# 64. Item State vs Runtime State

Do not use PresentationItem state for:

```text
RUNNING

RETRYING

CANCELLED

TIMED_OUT
```

Those are Runtime execution concerns.

---

# 65. Presentation Revision

Every committed Presentation state has a semantic:

```text
PresentationRevision
```

It identifies coherent Presentation state.

It is not:

```text
AttemptId

RuntimeRevisionId

TranslationArtifactId
```

though those may participate in provenance/authority validation.

---

# 66. Candidate Construction

Presentation work first produces:

```text
PresentationSnapshot Candidate
+
RenderPlan Candidate
```

Candidates are not current Published Presentation state.

---

# 67. Candidate Validation

Before commit, Presentation validates semantic invariants such as:

```text
Snapshot/RenderPlan revision match

valid semantic references

valid logical order

valid geometry spaces

valid typography constraints

valid fallback strategy

valid Presentation mode
```

---

# 68. Runtime Authority Revalidation

Semantic validity is not enough.

Before commit, current Runtime/Application authority must be revalidated.

Example:

```text
Attempt A1
    computes Presentation for Translation T1

Translation T2 becomes current

A1 finishes

Candidate is semantically valid

but no longer current
```

Therefore:

```text
semantic success
    ≠
publication authority
```

---

# 69. Atomic Commit

After successful validation:

```text
PresentationSnapshot Candidate
+
RenderPlan Candidate
        ↓
atomic commit
        ↓
current Presentation state
```

Partial mixed commit is forbidden.

---

# 70. Stale Candidate

A stale Candidate may be:

```text
discarded

retained for diagnostics

retained in cache when semantically reusable
```

but must not become current UI authority.

---

# 71. Runtime Boundary

Runtime owns:

```text
WorkItem

Attempt

scheduling

priority

cancellation

deadline

retry

resource admission

supersession

RuntimeRevisionId
```

Presentation cooperates with Runtime but does not duplicate these concepts.

---

# 72. Cancellation

Cancellation means:

```text
Runtime no longer wants this Attempt
```

It does not mutate Presentation semantics.

A cancelled Attempt simply cannot commit new current Presentation state unless Runtime authority explicitly still permits it.

---

# 73. Retry

Presentation does not own retry policy.

Runtime may execute another Attempt using compatible semantic inputs.

---

# 74. UI Adapter Boundary

After Presentation commit:

```text
PresentationSnapshot
+
RenderPlan
    ↓
UI Adapter
    ↓
ViewModel
    ↓
Native UI
```

UI Adapter owns concrete projection.

---

# 75. UI Adapter Owns

UI Adapter may own:

```text
frontend component mapping

native coordinates

widget creation

DOM/native element identity

focus state

selection state

interaction wiring

platform accessibility API mapping
```

These must not leak back into canonical Presentation state.

---

# 76. UI Apply Failure

A UI apply failure does not invalidate an already valid committed Presentation semantic state.

Distinguish:

```text
Presentation commit failure
```

from:

```text
UI projection/apply failure
```

---

# 77. UI Intent

User interaction flows upward as:

```text
Native UI
    ↓
UI Adapter
    ↓
UiIntent
    ↓
Application
```

Presentation does not directly execute business commands from native events.

---

# 78. Translation Edit

Example:

```text
User edits Translation
    ↓
UiIntent
    ↓
Application
    ↓
Translation owner
    ↓
new TranslationArtifact
    ↓
Presentation recalculation
```

Presentation never mutates TranslationArtifact directly.

---

# 79. Source Correction

Similarly:

```text
User reports OCR/source error
    ↓
UiIntent
    ↓
Application
    ↓
Recognition/Text Processing owner
```

Presentation does not rewrite source truth.

---

# 80. Retry Intent

A visible Retry action becomes an Application intent.

Presentation does not publish:

```text
TranslationRetryRequested
```

as an execution Event Bus command.

---

# 81. Preferences

Persistent Presentation preferences belong to:

```text
Preferences
```

Examples:

```text
preferred Presentation mode

font size

show original

overlay opacity

overlay preference

minimum readable size

accessibility preferences
```

---

# 82. Effective Configuration

Presentation consumes an immutable effective configuration snapshot.

It does not repeatedly query mutable Preferences during one Attempt.

---

# 83. Configuration Changes

Configuration changes may have different invalidation scope.

Example:

```text
font size change
    → Presentation recalculation
```

but normally:

```text
font size change
    ↛ Translation rerun
```

---

# 84. Mode Change

Changing:

```text
OVERLAY
    →
SIDE_PANEL
```

does not change Translation semantics.

It may require a new Presentation state.

---

# 85. Overlay Strategy Change

Changing:

```text
COVER
    →
ADJACENT
```

does not require a new TranslationArtifact.

---

# 86. Accessibility

Presentation semantics must support:

```text
logical reading order

source/Translation distinction

semantic grouping

focusable item identity

large-text compatibility

non-hover access

screen-reader ordering

contrast requirements
```

---

# 87. Screen Reader Order

For image Presentation:

```text
screen reader order
    =
semantic Reading Order
```

not:

```text
sort by X/Y coordinate
```

---

# 88. Responsive Presentation

Presentation may select semantic fallback based on stable viewport constraints.

Examples:

```text
SIDE_BY_SIDE
    ↓ narrow viewport
INTERLEAVED
```

or:

```text
dense OVERLAY
    ↓
FLOATING / SIDE_PANEL
```

---

# 89. Long-Form Reading

Text Reader should support architecture compatible with:

```text
incremental presentation

bounded visible windows

stable PresentationItem identity

reading-position anchors

frontend virtualization
```

Virtualization itself remains UI Adapter/frontend responsibility.

---

# 90. Reading Position

Reading position belongs to:

```text
Reading Session
```

Presentation may expose stable anchors.

It does not own the user's canonical reading progress.

---

# 91. Caching

Presentation may cache:

```text
layout calculations

fitting results

geometry transforms

Presentation Candidates

RenderPlan fragments
```

when compatibility is proven.

---

# 92. Cache Compatibility

Compatibility may depend on:

```text
TranslationArtifact identity

source geometry identity

Presentation configuration

Presentation mode

viewport constraint class

Presentation engine version
```

---

# 93. Cache Is Not Authority

A cache hit does not bypass:

```text
semantic validation

Runtime authority validation
```

before current commit.

---

# 94. Presentation Engine Version

Semantically relevant algorithm changes may use:

```text
PresentationEngineVersion
```

for:

```text
cache compatibility

diagnostics

reproducibility
```

It is not Runtime execution identity.

---

# 95. Determinism

Equivalent:

```text
Translation semantics

source semantics

source geometry

Presentation configuration

ViewportSnapshot

PresentationEngineVersion
```

should produce equivalent semantic Presentation output where deterministic algorithms apply.

---

# 96. Performance

Presentation should prioritize:

```text
currently visible/requested content

bounded fitting

incremental recalculation

stable layout reuse

safe cache reuse

fast fallback
```

---

# 97. Bounded Algorithms

Potentially iterative algorithms must be bounded.

Examples:

```text
font fitting

collision resolution

placement search

geometry optimization
```

When the bound is reached:

```text
fallback
```

rather than indefinite computation.

---

# 98. Progressive Calculation

Presentation may calculate affected items incrementally.

Published state must still remain coherent.

---

# 99. Prefetch

Near-future Presentation may be precomputed.

Prefetch is speculative and cannot become current solely because it completed first.

---

# 100. Error Boundary

Presentation errors describe semantic Presentation failures.

Examples:

```text
invalid input

invalid semantic reference

invalid geometry

unsupported mode

layout failure

fit failure

invalid configuration
```

Exact error taxonomy belongs to:

```text
ERRORS.md
```

---

# 101. Runtime Failure Boundary

Do not redefine:

```text
cancelled

timed out

retry exhausted

superseded Attempt
```

as Presentation semantic errors.

They remain Runtime-owned.

---

# 102. Events

Presentation events describe committed semantic facts.

Exact catalog belongs to:

```text
EVENTS.md
```

Possible fact:

```text
PresentationCommitted
```

or equivalent contract-defined event.

---

# 103. No Event Bus Execution Orchestration

Deprecated:

```text
PresentationRequested
    ↓
PresentationPreparationStarted
    ↓
PresentationLayoutCalculated
    ↓
PresentationCompleted
```

as Event Bus execution orchestration.

Runtime/Application controls execution.

---

# 104. Diagnostics

Useful diagnostics include:

```text
Presentation calculation latency

commit latency

mode

overlay strategy

fit fallback count

collision count

degraded item count

missing Translation count

cache hit

stale Candidate rejection

UI apply failure
```

---

# 105. Diagnostic Privacy

Do not log raw:

```text
source text

translated text

private annotations
```

by default.

Prefer:

```text
IDs

counts

sizes

hashes

strategy names

warning codes

latencies
```

---

# 106. Security

All source/translated content is untrusted data.

Presentation must not allow content to become:

```text
script

native command

Event Bus command

provider instruction

frontend event handler
```

---

# 107. Structured Source Security

Source HTML/CSS must not be injected directly into the frontend through Presentation.

Use normalized semantic content.

---

# 108. Privacy

Presentation may expose:

```text
private documents

authenticated web content

clipboard text

copyrighted reading content

personal corrections
```

Therefore:

```text
minimize retention

avoid raw-content logging

release obsolete state

respect local-only policies
```

---

# 109. MVP — Text Reader

Initial Text Reader should support:

```text
translated-only display

source/Translation comparison

Paragraph preservation

dialogue formatting

readable typography

stable logical order

incremental updates

basic accessibility
```

---

# 110. MVP — Image / Comic

Initial image Presentation should support:

```text
source-relative geometry

OVERLAY mode

basic COVER strategy

basic ADJACENT fallback

bounded text fitting

minimum readable font size

show/hide Translation

source association

SIDE_PANEL fallback
```

---

# 111. MVP Does Not Require

```text
automatic image inpainting

Bubble-shape-aware text fitting

curved text

font-style cloning

complex collision optimization

AI-generated layout

automatic Bubble detection

AR presentation
```

---

# 112. Future Extensions

Possible future extensions:

```text
Bubble-aware layout

background reconstruction integration

typography matching

curved SFX text

semantic overlay ranking

advanced collision solving

dual-language learning mode

word-level vocabulary interaction

Translation alternatives

pronunciation display

adaptive layout policy
```

---

# 113. Future Bubble Support

If Bubble semantics become available, Presentation may evolve from:

```text
TextRegion-based conservative placement
```

toward:

```text
Bubble-safe-area-aware placement
```

without changing the invariant:

```text
Text Region
    ≠
Speech Bubble Region
```

---

# 114. Future Overlay Strategy Model

If strategy configuration becomes externally important, a bounded contract may later expose:

```text
OverlayPlacementPolicy
```

This should remain separate from:

```text
PresentationMode
```

unless architecture explicitly changes.

---

# 115. Relationship to Translation

Translation owns:

```text
TranslationUnit

TranslationBatch

Translation Context

TranslationArtifact
```

Presentation consumes TranslationArtifact.

Presentation does not become a Translation submodule.

---

# 116. Relationship to Text Processing

Text Processing owns canonical source semantics.

Presentation consumes source references through Translation/source provenance.

It does not reconstruct:

```text
Paragraph

Sentence

Reading Order

source relationships
```

independently.

---

# 117. Relationship to Recognition

Recognition owns recognized visual source semantics.

Presentation may consume:

```text
Region geometry

orientation

confidence

Bubble/Panel semantics if available
```

through explicit contracts.

---

# 118. Relationship to Reading Session

Reading Session owns:

```text
current content identity

reading position

session lifecycle
```

Presentation may consume stable session-derived constraints but does not own reading progress.

---

# 119. Relationship to Preferences

Preferences owns persistent user configuration.

Presentation consumes effective immutable Presentation configuration.

---

# 120. Relationship to UI Adapter

UI Adapter owns:

```text
PresentationSnapshot + RenderPlan
        ↓
ViewModel
        ↓
native/frontend rendering
```

Presentation never exposes native frontend objects as semantic contracts.

---

# 121. Relationship to Runtime

Runtime owns execution.

Presentation owns semantic Presentation state.

Canonical distinction:

```text
Attempt
    = execution identity

PresentationRevision
    = semantic Presentation identity
```

---

# 122. Architecture Decisions

The module adopts the following decisions:

```text
Presentation is a semantic owner.

TranslationArtifact is authoritative translated input.

PresentationSnapshot + RenderPlan form one coherent committed state.

PresentationMode is separate from overlay placement strategy.

Text Region is not Speech Bubble Region.

Presentation does not own Bubble detection.

Source geometry remains upstream-owned.

Presentation consumes canonical Reading Order.

Readability outranks exact geometry fitting.

Font reduction is bounded.

Fit failure changes Presentation strategy, not Translation meaning.

Vertical source orientation does not force vertical Vietnamese output.

SFX may use specialized Presentation strategy.

Collision resolution cannot rewrite source truth.

Presentation does not duplicate Runtime execution lifecycle.

UI Adapter owns native rendering.

Published Presentation state is immutable.

Candidate publication requires authority revalidation.
```

---

# 123. Module Invariants

1. Presentation owns semantic Presentation state.

2. Presentation does not own Translation semantics.

3. Presentation does not own source semantics.

4. Presentation does not own Runtime execution.

5. Presentation does not own native rendering.

6. `PresentationSnapshot` and `RenderPlan` commit atomically.

7. Their Presentation revisions must match.

8. Presentation consumes Published TranslationArtifact.

9. Presentation uses typed semantic references.

10. Generic `segmentId` is not architecture-wide authority.

11. PresentationMode and OverlayPlacementStrategy are different abstractions.

12. Source geometry remains upstream-owned.

13. Coordinate spaces are explicit.

14. Source-relative geometry is preferred for reusable image Presentation.

15. `Text Region ≠ Speech Bubble Region`.

16. OCR text bounds do not imply Bubble-safe area.

17. Missing Bubble geometry must not be fabricated.

18. Presentation does not own Bubble detection.

19. Presentation consumes canonical semantic Reading Order.

20. Visual placement may differ from logical order.

21. Logical order must remain preserved.

22. Presentation may compose multiple TranslationUnits visually.

23. Presentation does not invent speaker identity.

24. Typography reduction has a readable minimum.

25. Text fitting is bounded.

26. Readability outranks exact geometric replacement.

27. Fit failure triggers fallback rather than semantic truncation.

28. Presentation constraints to Translation are advisory.

29. Vertical source text does not require vertical Vietnamese Translation.

30. SFX presentation may preserve artwork.

31. Collision handling does not mutate source truth.

32. Partial Presentation requires explicit upstream partial semantics.

33. Raw Runtime progress is not semantic Presentation state.

34. PresentationItem state does not duplicate Runtime Attempt state.

35. Published Presentation state is immutable.

36. Candidate semantic validity does not imply publication authority.

37. Runtime authority must be revalidated before commit.

38. Stale Candidate cannot become current.

39. Retry is Runtime-owned.

40. Cancellation is Runtime-owned.

41. User edits route through Application to semantic owners.

42. Persistent configuration remains Preferences-owned.

43. Reading position remains Reading Session-owned.

44. Cache does not grant authority.

45. UI apply failure is distinct from Presentation commit failure.

46. Event Bus does not orchestrate Presentation execution.

47. Raw reading content is not logged by default.

48. Source markup is untrusted.

49. Presentation contracts remain platform-neutral.

50. Frontend virtualization does not alter semantic Presentation state.

---

# 124. Deprecated Concepts

Deprecated:

```text
PresentationRequest
    as primary semantic authority
```

Deprecated:

```text
requestId
requestRevision
sourceRevision
translationRevision
```

as one generic correctness mechanism.

Deprecated:

```text
PresentationSegment
SourceSegment
TranslationSegment
```

as universal cross-module architecture objects.

Deprecated:

```text
Presentation
    owns cancelled/retrying execution state
```

Deprecated:

```text
PresentationRequested
PresentationPreparationStarted
PresentationLayoutCalculated
PresentationCancelled
```

as Event Bus execution workflow.

Deprecated:

```text
OCR Text Region
    =
Speech Bubble Region
```

Deprecated:

```text
shrink font until Translation fits
```

Deprecated:

```text
truncate Translation
to satisfy source geometry
```

---

# 125. Preserved Legacy Strengths

The following earlier Presentation concepts remain valid and are intentionally preserved:

```text
Native Text vs image Presentation distinction

source/Translation comparison

Paragraph preservation

dialogue presentation

source-relative geometry

coordinate-space discipline

overlay placement strategies

Text Region / Bubble distinction

bounded text fitting

minimum readable size

readability fallback

vertical-source handling

SFX presentation

background treatment

collision handling

responsive fallback

partial/provisional awareness

accessibility

security

cache-aware layout

framework independence
```

They are now placed under the correct Runtime v2 and module ownership boundaries.

---

# 126. Related Documents

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
├── translation/
├── text-processing/
├── recognition/
├── reading-session/
├── preferences/
└── ui-adapter/
```

---

# 127. Open Decisions

The following remain intentionally open:

```text
final typed SourceSemanticRef contract

removal/migration of SourceSegmentRef

removal/migration of TranslationSegmentRef

Bubble geometry owner

Bubble-safe-area contract

OverlayPlacementPolicy public/private boundary

background reconstruction owner

TextMeasurementPort design

font measurement ownership

partial/provisional Presentation contract

PresentationItem final state enum

Presentation cache compatibility key

viewport constraint classification

advanced collision algorithm

Bubble-shape-aware fitting

SFX style policy

Presentation engine version contract
```

These decisions must not reintroduce:

```text
generic Segment authority

Presentation-owned Runtime lifecycle

native UI types inside Presentation contracts
```

---

# 128. Completion Criteria

Presentation MODULE is architecture-complete when:

* Presentation ownership is explicit;
* Translation/Presentation/UI Adapter boundaries are explicit;
* Runtime execution authority remains external;
* `PresentationSnapshot + RenderPlan` atomicity is explicit;
* typed semantic references replace generic Segment assumptions;
* high-level modes are separated from overlay strategies;
* source-relative geometry is preserved;
* coordinate spaces are explicit;
* Text Region and Bubble Region are explicitly distinct;
* Bubble detection remains externally owned;
* bounded text fitting is defined;
* minimum readable typography is enforced;
* fitting failure falls back instead of truncating meaning;
* vertical text/SFX behavior is bounded;
* collision recovery preserves source truth;
* partial/provisional semantics do not depend on raw Runtime state;
* stale Candidate protection follows Runtime v2;
* UI Adapter remains native-rendering owner;
* security/privacy/accessibility boundaries are explicit.

---

# 129. Summary

Presentation is the semantic boundary between:

```text
what Translation means
```

and:

```text
what the user interface renders
```

Canonical architecture:

```text
Published TranslationArtifact
        ↓
Presentation
        ↓
PresentationSnapshot Candidate
        +
RenderPlan Candidate
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
        ↓
ViewModel
        ↓
Native UI
```

For image content:

```text
Translation
    +
source geometry references
        ↓
Presentation
        ↓
OVERLAY / HYBRID
        ↓
placement strategy
        ↓
bounded fitting
        ↓
readability validation
        ↓
fallback when required
```

The most important geometry rule is:

```text
Text Region
    ≠
Speech Bubble Region
```

The most important fitting rule is:

```text
readability
    >
exact geometric replacement
```

The most important ownership rule is:

```text
Translation
    owns meaning

Presentation
    owns semantic presentation

UI Adapter
    owns concrete UI projection

Runtime
    owns execution
```

Presentation therefore remains:

```text
semantic

immutable after commit

platform-neutral

source-traceable

readability-first

Runtime-v2 aligned
```
