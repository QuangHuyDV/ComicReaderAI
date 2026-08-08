# Presentation Module

> **Project:** CRAI
> **Module:** `presentation`
> **Path:** `doc/02-modules/presentation/README.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-08

---

# 1. Overview

The Presentation Module transforms accepted reading Artifacts into immutable, platform-independent presentation state.

Its purpose is to answer:

```text
Given accepted reading content,
how should that content be presented to the user?
```

Presentation owns:

```text
Presentation semantics
Presentation mapping
Presentation layout
Presentation geometry interpretation
Presentation modes
Presentation profiles
PresentationRevision
PresentationSnapshot
RenderPlan
Presentation lifecycle
```

Presentation does **not** own:

```text
OCR
Translation
Runtime scheduling
Runtime authority
Artifact publication
Persistent storage
Native UI rendering
Browser DOM manipulation
Platform graphics
```

The module produces presentation state.

It does not draw that state itself.

---

# 2. Architecture Position

Presentation sits between accepted semantic reading data and platform-specific rendering.

```text
Recognition / Text Processing / Translation
                    │
                    ▼
            Immutable Artifacts
                    │
                    ▼
             Runtime Control
                    │
                    ▼
               Application
                    │
                    ▼
              Presentation
                    │
          ┌─────────┴─────────┐
          ▼                   ▼
PresentationSnapshot      RenderPlan
          │                   │
          └─────────┬─────────┘
                    ▼
                UI Adapter
                    │
                    ▼
       Desktop / Browser / Mobile
```

The important boundary is:

```text
Presentation decides WHAT the presentation means.

UI Adapter decides HOW that presentation
is realized on a specific platform.
```

---

# 3. Why Presentation Exists

Without a Presentation boundary, reading behavior would become coupled to individual frontends.

For example:

```text
Desktop overlay logic
Browser overlay logic
Mobile overlay logic
Reader layout logic
```

could each independently decide:

* reading order;
* translation placement;
* geometry mapping;
* overflow behavior;
* typography fallback;
* mode behavior.

That would create inconsistent reading experiences.

Presentation centralizes these semantics into reusable platform-independent models.

---

# 4. Primary Responsibilities

Presentation is responsible for:

* validating Presentation commands;
* consuming accepted Artifact references;
* mapping semantic content into PresentationItems;
* building immutable PresentationSnapshots;
* building immutable RenderPlans;
* interpreting source geometry;
* computing presentation layout;
* applying PresentationProfiles;
* selecting compatible Presentation modes and strategies;
* processing viewport/target information supplied through commands;
* maintaining Presentation-local state;
* maintaining PresentationRevision;
* performing candidate validation;
* performing atomic Presentation commit;
* publishing Presentation-owned facts;
* preserving previous committed state during recoverable failure.

---

# 5. Explicit Non-Responsibilities

Presentation MUST NOT:

* perform OCR;
* perform translation;
* modify translated content;
* modify source content;
* publish Recognition or Translation Artifacts;
* schedule Runtime work;
* decide Runtime retry;
* decide whether Runtime work is still authoritative;
* manipulate Work Queue state;
* manipulate browser DOM;
* draw native widgets;
* call platform graphics APIs;
* own native window lifecycle;
* own persistent user storage;
* fabricate source geometry;
* fabricate semantic reading order.

These boundaries are architectural invariants.

---

# 6. Core Runtime Model

Presentation follows a Candidate-based execution model.

```text
Command
   ↓
Validate
   ↓
Read accepted Artifact references
   ↓
Build Candidate
   ↓
Validate Candidate
   ↓
Runtime authority revalidation
   ↓
Commit
   ↓
New PresentationRevision
   ↓
Publish Presentation fact
   ↓
UI Adapter applies committed state
```

Until commit succeeds:

```text
Candidate ≠ Current Presentation
```

The previous committed Presentation remains authoritative.

---

# 7. Runtime Authority

Presentation does not decide whether Runtime work is still current.

Before commit, Runtime may reject authority because:

```text
Runtime Revision changed
Work was canceled
Session became inactive
Attempt was superseded
```

When authority is rejected:

```text
discard Candidate
do not commit
do not increment PresentationRevision
do not enter FAILED
```

Runtime authority rejection is not a Presentation internal failure.

---

# 8. PresentationRevision

Presentation owns exactly one revision domain:

```text
PresentationRevision
```

It identifies committed Presentation state.

Conceptually:

```text
PresentationRevision N
    │
    ├── PresentationSnapshot N
    └── RenderPlan N
```

A successful mutation produces:

```text
N → N + 1
```

A failed, canceled, or superseded Candidate produces:

```text
N → N
```

PresentationRevision is independent from Runtime Revision.

---

# 9. Artifact Consumption

Presentation consumes accepted immutable Artifact references.

Typical inputs may include:

```text
SourceDocumentArtifact
RecognitionArtifact
TranslationArtifact
```

depending on Presentation mode.

Presentation does not own those Artifacts.

It may validate:

```text
Artifact type
Artifact compatibility
semantic associations
required geometry availability
required translation availability
```

but it must not mutate upstream Artifact content.

---

# 10. PresentationSnapshot

`PresentationSnapshot` is the immutable semantic representation of a committed Presentation.

Conceptually:

```text
PresentationSnapshot
├── presentationId
├── presentationRevision
├── presentationMode
├── presentationProfile
├── target
├── viewport
├── items[]
├── semantic associations
└── artifact references
```

The exact contract is defined in `CONTRACT.md`.

Once committed, a Snapshot is immutable.

---

# 11. RenderPlan

`RenderPlan` describes how a committed PresentationSnapshot should be realized by a UI Adapter.

Conceptually:

```text
RenderPlan
├── presentationRevision
├── target information
├── viewport information
├── layout
├── geometry
├── typography semantics
├── visibility
├── ordering
└── rendering instructions
```

Presentation owns RenderPlan construction.

UI Adapter owns RenderPlan execution.

---

# 12. PresentationItem

A `PresentationItem` is a logical presentation unit.

Examples:

```text
translated text block
speech-bubble presentation
marker
side-panel entry
reader paragraph
focused translation item
```

PresentationItems describe semantic presentation.

They are not native UI elements.

---

# 13. PresentationMode

`PresentationMode` represents the user-visible presentation behavior.

Typical modes may include:

```text
Overlay
SidePanel
Reader
```

Additional modes may be introduced as capabilities evolve.

A mode describes presentation intent.

It must not encode platform-specific rendering implementation.

---

# 14. PresentationStrategy

A `PresentationStrategy` is an internal architectural strategy used to realize a PresentationMode.

Examples may include:

```text
PreciseOverlay
MarkerOverlay
FocusedOverlay
StructuredReader
SimplifiedReader
```

Strategies may change without changing the public meaning of a PresentationMode.

This allows Presentation to use fallback while preserving user intent.

---

# 15. PresentationProfile

`PresentationProfile` defines presentation preferences and semantic visual policy.

Typical properties include:

```text
typography
spacing
density
readability
accessibility
alignment preferences
overflow preferences
```

PresentationProfile remains platform-independent.

It must not contain native font handles or UI objects.

---

# 16. Geometry

Presentation may interpret source geometry supplied by accepted Artifacts.

Examples:

```text
bounding boxes
polygons
source regions
normalized coordinates
```

Presentation may transform these into presentation-space geometry.

It MUST NOT fabricate source geometry when upstream geometry is unavailable.

Typical fallback:

```text
Precise Overlay
      ↓
Marker-based Presentation
      ↓
SidePanel
```

---

# 17. Semantic Reading Order

Presentation may consume semantic ordering supplied by accepted upstream Artifacts.

Presentation may:

* preserve it;
* filter invisible items;
* map it into PresentationItems;
* derive presentation traversal from it where contractually valid.

Presentation MUST NOT silently invent semantic reading order when required ordering information is unavailable.

---

# 18. Layout

Presentation owns platform-independent layout planning.

This may include:

* item placement;
* wrapping;
* spacing;
* overflow handling;
* overlap resolution;
* marker positioning;
* side-panel organization;
* reader layout;
* visibility decisions.

Presentation does not own final pixel rendering.

---

# 19. Readability

Layout optimization must preserve readability.

Presentation MUST NOT solve layout problems by silently producing unusable text.

For example:

```text
text does not fit
```

must not automatically become:

```text
shrink indefinitely until it fits
```

Instead Presentation may:

```text
wrap
expand
scroll
simplify
change strategy
change mode
```

according to Presentation policy.

---

# 20. Public Commands

Presentation exposes commands such as:

```text
BuildPresentation
UpdatePresentation
RecomputePresentationLayout
UpdatePresentationFocus
ApplyPresentationProfile
ChangePresentationMode
ClearPresentation
```

Commands represent explicit Presentation intent.

They do not represent Runtime scheduling instructions.

Full command contracts are defined in:

```text
CONTRACT.md
```

---

# 21. Command Flow

Typical mutation flow:

```text
Application
    │
    ▼
Presentation Command
    │
    ▼
Validate command
    │
    ▼
Validate current PresentationRevision
    │
    ▼
Prepare Candidate
    │
    ▼
Validate Candidate
    │
    ▼
Runtime authority revalidation
    │
    ├── rejected
    │      ↓
    │   discard
    │
    └── accepted
           ↓
        atomic commit
           ↓
    PresentationRevision + 1
```

---

# 22. Events

Presentation publishes facts about committed Presentation state and externally meaningful rejection/failure outcomes.

Typical events include:

```text
PresentationPrepared
PresentationUpdated
PresentationLayoutChanged
PresentationModeChanged
PresentationRejected
PresentationFailed
PresentationCleared
```

Presentation does not directly subscribe to arbitrary business events from Recognition, Translation, Reading Session, or UI Adapter.

Routing and orchestration belong outside Presentation.

Full event semantics are defined in:

```text
EVENTS.md
```

---

# 23. Presentation Events Are Facts

Presentation events describe completed facts.

Example:

```text
PresentationUpdated
```

means:

```text
a new Presentation state was successfully committed
```

It does not mean:

```text
please update Presentation
```

Commands express intent.

Events express facts.

---

# 24. Lifecycle

Presentation uses the lifecycle defined in `STATES.md`.

Primary stable states:

```text
EMPTY
READY
FAILED
```

Transient operation states may include:

```text
PREPARING
UPDATING
REFLOWING
RECONFIGURING
CLEARING
RECOVERING
```

Typical flow:

```text
EMPTY
  ↓
PREPARING
  ↓
READY
  ↓
UPDATING / REFLOWING / RECONFIGURING
  ↓
READY
  ↓
CLEARING
  ↓
EMPTY
```

---

# 25. Candidate Isolation

During transient work:

```text
Current Presentation
        +
Candidate Presentation
```

exist conceptually at the same time.

The Candidate must not mutate current committed state.

If Candidate processing fails:

```text
discard Candidate
```

Current Presentation remains active.

---

# 26. FAILED State

`FAILED` is reserved for Presentation-owned correctness loss.

Examples:

```text
committed Snapshot/RenderPlan mismatch
impossible PresentationRevision
corrupted Presentation registry
uncertain atomic commit
unrecoverable Presentation recovery failure
```

The following do not normally enter `FAILED`:

```text
invalid command
unsupported mode
layout fallback
PresentationRevision conflict
Candidate supersession
Runtime authority rejection
Runtime cancellation
UI apply failure
```

---

# 27. Error Model

Presentation exposes stable architecture-level error contracts.

Errors define:

```text
ErrorCode
Category
Severity
RecoveryHint
RetryHint where appropriate
```

Presentation does not own Runtime retry execution.

Presentation errors describe Presentation-owned failures only.

Full specification:

```text
ERRORS.md
```

---

# 28. Failure Ownership

A useful ownership rule is:

| Failure                       | Owner                   |
| ----------------------------- | ----------------------- |
| Invalid Presentation command  | Presentation            |
| Invalid Presentation geometry | Presentation            |
| Layout failure                | Presentation            |
| PresentationRevision conflict | Presentation            |
| Runtime Revision obsolete     | Runtime                 |
| Work canceled                 | Runtime                 |
| Translation failed            | Translation             |
| Artifact publication failed   | Artifact infrastructure |
| UI rendering failed           | UI Adapter              |
| Native window failed          | Platform/UI Adapter     |

Presentation must not absorb external failures into generic internal Presentation errors.

---

# 29. UI Adapter Boundary

UI Adapter consumes committed Presentation output.

Conceptually:

```text
PresentationRevision N
        ↓
PresentationSnapshot
+
RenderPlan
        ↓
UI Adapter
        ↓
Platform rendering
```

UI Adapter may reject application because:

```text
target disappeared
target revision changed
UI state is stale
platform rendering failed
```

These are UI-owned outcomes.

Presentation does not enter `FAILED` solely because rendering failed.

---

# 30. Logical Clear vs Physical Cleanup

`ClearPresentation` logically removes current Presentation state.

Conceptually:

```text
READY
  ↓
CLEARING
  ↓
EMPTY
```

Once logical clear commits:

```text
Presentation is unavailable
```

even if UI Adapter still needs time to physically remove:

```text
overlay windows
DOM nodes
native surfaces
cached platform resources
```

Physical cleanup belongs outside Presentation.

---

# 31. Fallback

Presentation supports correctness-preserving fallback.

Example for image-oriented content:

```text
Precise Overlay
      ↓
Focused / Marker Presentation
      ↓
SidePanel
```

Example for structured text:

```text
Styled Reader
      ↓
Simplified Reader
```

Fallback must remain:

```text
deterministic
observable
semantically valid
readable
```

Fallback must not conceal corrupted state.

---

# 32. Atomic Commit

Presentation mutation commits as one logical unit.

Conceptually:

```text
PresentationRevision
+
PresentationSnapshot
+
RenderPlan
```

must become current consistently.

Partial commit is forbidden.

If atomic commit fails but previous state is known intact:

```text
preserve previous Presentation
```

If commit outcome becomes untrustworthy:

```text
FAILED
```

---

# 33. Idempotency

Presentation operations should be safely repeatable where semantics allow.

Examples:

```text
same ClearPresentation
same profile already active
same focus already selected
same effective viewport
```

may resolve as no-op.

No-op does not require a new PresentationRevision unless the public contract explicitly defines otherwise.

---

# 34. Supersession

Presentation work may be superseded by newer work.

Examples:

```text
viewport A
viewport B
viewport C
```

If C supersedes A and B:

```text
A → discard
B → discard
C → eligible to commit
```

Supersession is expected control behavior.

It is not an internal error.

---

# 35. Runtime Cancellation

Runtime cancellation may terminate Presentation work.

Presentation must cooperate by:

```text
stopping useful work where possible
discarding Candidate
avoiding commit
avoiding PresentationRevision increment
```

Cancellation does not automatically imply `PresentationFailed`.

---

# 36. Event Publication Failure

Presentation state commit and event publication are separate concerns.

If:

```text
Presentation commit succeeds
```

but:

```text
event publication fails
```

then:

```text
committed Presentation remains valid
```

The Presentation operation must not be repeated merely to regenerate the event.

Reliable publication belongs to Event Bus/outbox infrastructure policy.

---

# 37. Platform Independence

Presentation contracts must not expose:

```text
DOM Element
HTMLElement
CanvasRenderingContext
HWND
NSView
UIView
Android View
native font handle
GPU texture
platform window handle
```

Platform-specific objects belong to adapters.

Presentation contracts use serializable semantic data.

---

# 38. Immutability

Committed public Presentation objects are immutable.

This includes:

```text
PresentationSnapshot
RenderPlan
PresentationItem
committed Presentation metadata
```

Changes produce new committed objects and, when semantically mutated, a new PresentationRevision.

---

# 39. Determinism

Given equivalent:

```text
accepted Artifacts
Presentation command
PresentationProfile
PresentationMode
target capabilities
viewport
current Presentation state
```

Presentation should produce semantically equivalent output.

Platform rendering differences do not change Presentation semantics.

---

# 40. Observability

Presentation should expose diagnostics for:

```text
operation duration
layout duration
candidate creation
candidate supersession
PresentationRevision conflict
fallback
geometry failure
layout failure
commit
rejection
failure
event publication failure
```

Observability must remain privacy-safe.

Full error/diagnostic rules are defined in:

```text
ERRORS.md
```

---

# 41. Privacy

Presentation diagnostics should prefer:

```text
IDs
revisions
counts
bounded geometry summaries
mode
strategy
error code
```

instead of:

```text
source text
translated text
screenshots
page images
raw provider output
```

User content must not leak into normal error payloads or telemetry.

---

# 42. Directory Structure

```text
02-modules/
└── presentation/
    ├── README.md
    ├── MODULE.md
    ├── CONTRACT.md
    ├── STATES.md
    ├── EVENTS.md
    └── ERRORS.md
```

Each file has a distinct responsibility.

---

# 43. Documentation Roles

## README.md

Entry point and conceptual overview.

Answers:

```text
What is Presentation?
Where does it sit?
What does it own?
How should I read this module?
```

---

## MODULE.md

Defines:

```text
module boundary
responsibilities
ownership
dependencies
architecture invariants
```

---

## CONTRACT.md

Defines:

```text
commands
queries
public data models
PresentationSnapshot
RenderPlan
PresentationItem
revision contracts
```

---

## STATES.md

Defines:

```text
Presentation lifecycle
stable states
transient states
candidate lifecycle
commit behavior
recovery
```

---

## EVENTS.md

Defines:

```text
Presentation-owned facts
event payload semantics
event publication rules
correlation
causation
```

---

## ERRORS.md

Defines:

```text
error ownership
stable ErrorCodes
severity
recovery
fallback
fatal failures
diagnostics
```

---

# 44. Recommended Reading Order

For a new contributor:

```text
1. README.md
2. MODULE.md
3. CONTRACT.md
4. STATES.md
5. EVENTS.md
6. ERRORS.md
```

This order moves from:

```text
concept
→ boundary
→ API
→ lifecycle
→ facts
→ failure behavior
```

---

# 45. Reading Order for Implementation

When implementing Presentation:

```text
MODULE.md
    ↓
CONTRACT.md
    ↓
STATES.md
    ↓
ERRORS.md
    ↓
EVENTS.md
```

Why:

```text
know ownership
→ know public API
→ know legal lifecycle
→ know failure semantics
→ publish correct facts
```

---

# 46. Reading Order for Debugging

For Presentation-specific failures:

```text
ERRORS.md
    ↓
STATES.md
    ↓
CONTRACT.md
    ↓
EVENTS.md
```

For Runtime authority/cancellation problems, inspect Runtime architecture instead of assuming Presentation is the owner.

---

# 47. Common Architecture Mistakes

## Mistake 1 — Rendering inside Presentation

Wrong:

```text
Presentation → create DOM node
```

Correct:

```text
Presentation → RenderPlan
UI Adapter → DOM
```

---

## Mistake 2 — Translation inside Presentation

Wrong:

```text
Presentation → translate missing text
```

Correct:

```text
Translation → TranslationArtifact
Application → Presentation command
Presentation → presentation mapping
```

---

## Mistake 3 — Presentation deciding Runtime authority

Wrong:

```text
Presentation checks Runtime Revision
and decides work authority itself
```

Correct:

```text
Runtime authority revalidation
→ normalized result
→ Presentation commits or discards Candidate
```

---

## Mistake 4 — Using Runtime Revision as PresentationRevision

Wrong:

```text
PresentationRevision = RuntimeRevision
```

Correct:

```text
RuntimeRevision
    ≠
PresentationRevision
```

They represent different domains.

---

## Mistake 5 — Treating UI failure as Presentation failure

Wrong:

```text
overlay failed to draw
→ Presentation FAILED
```

Correct:

```text
Presentation committed
UI Adapter apply failed
→ UI-owned recovery
```

---

## Mistake 6 — Mutating committed Snapshot

Wrong:

```text
currentSnapshot.items.push(...)
```

Correct:

```text
current Snapshot
    ↓
build Candidate Snapshot
    ↓
validate
    ↓
atomic commit
```

---

## Mistake 7 — Inventing missing geometry

Wrong:

```text
geometry missing
→ guess bubble position
```

Correct:

```text
geometry missing
→ compatible fallback
```

---

## Mistake 8 — Treating supersession as failure

Wrong:

```text
old layout finished late
→ PresentationFailed
```

Correct:

```text
old Candidate obsolete
→ discard
```

---

# 48. Architecture Invariants

Every Presentation implementation must preserve the following:

1. Presentation remains platform-independent.

2. Presentation never performs OCR.

3. Presentation never performs Translation.

4. Presentation consumes immutable accepted Artifact references.

5. Presentation does not mutate upstream Artifacts.

6. Presentation owns Presentation semantics.

7. Presentation owns PresentationRevision.

8. Presentation does not own Runtime Revision.

9. PresentationRevision and Runtime Revision are distinct domains.

10. PresentationSnapshot is immutable after commit.

11. RenderPlan is immutable after commit.

12. RenderPlan and Snapshot belong to the same committed PresentationRevision.

13. Candidate state never partially mutates current state.

14. Failed Candidates are discarded.

15. Superseded Candidates are discarded.

16. Runtime authority rejection prevents commit.

17. Runtime authority rejection is not Presentation internal failure.

18. Runtime cancellation does not automatically enter `FAILED`.

19. PresentationRevision increments only after successful semantic commit.

20. Presentation commands express intent.

21. Presentation events express completed facts.

22. Presentation does not directly consume arbitrary business events.

23. Presentation does not render UI.

24. UI Adapter owns platform-specific application.

25. UI apply failure does not automatically invalidate committed Presentation state.

26. Logical clear is distinct from physical UI cleanup.

27. Presentation does not fabricate source geometry.

28. Presentation does not fabricate translated content.

29. Presentation does not silently invent semantic reading order.

30. Layout preserves readability.

31. Fallback preserves correctness.

32. Atomic commit preserves Snapshot/RenderPlan consistency.

33. Event publication failure does not invalidate successful commit.

34. Presentation error ownership remains inside Presentation boundary.

35. `FAILED` is reserved for loss of Presentation-owned correctness.

36. Public contracts remain serializable and implementation-independent.

37. Diagnostics remain privacy-safe.

---

# 49. Related Documents

## Presentation

```text
doc/02-modules/presentation/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
└── ERRORS.md
```

## Core Architecture

```text
doc/01-architecture/core/
├── STATE_MACHINE.md
├── EVENT_BUS.md
└── EVENT_CONVENTION.md
```

## Module Architecture

```text
doc/01-architecture/modules/
├── OWNERSHIP_MAP.md
├── MODULE_DEPENDENCY.md
└── CAPABILITY_MAP.md
```

## Runtime Architecture

```text
doc/01-architecture/runtime/
├── PIPELINE_RUNTIME.md
├── CANCELLATION.md
├── RETRY_POLICY.md
├── RESOURCE_LIFECYCLE.md
└── RUNTIME_OBSERVABILITY.md
```

Relevant neighboring modules include:

```text
reading-session
recognition
text-processing
translation
ui-adapter
```

---

# 50. Completion Checklist

The Presentation module is architecturally synchronized when:

* [ ] module ownership is explicit;
* [ ] public contracts are stable;
* [ ] PresentationSnapshot is immutable;
* [ ] RenderPlan is immutable;
* [ ] PresentationRevision ownership is explicit;
* [ ] Runtime Revision remains externally owned;
* [ ] Runtime authority revalidation is respected;
* [ ] Candidate isolation is enforced;
* [ ] commands express Presentation intent;
* [ ] events express committed Presentation facts;
* [ ] arbitrary upstream event consumption is removed;
* [ ] Artifact compatibility is explicit;
* [ ] geometry ownership is explicit;
* [ ] semantic reading-order boundary is explicit;
* [ ] layout preserves readability;
* [ ] fallback is deterministic and safe;
* [ ] UI rendering remains outside Presentation;
* [ ] UI apply failure remains UI Adapter-owned;
* [ ] cancellation remains Runtime-owned;
* [ ] event publication failure does not invalidate commit;
* [ ] `FAILED` is reserved for Presentation correctness loss;
* [ ] error ownership follows module boundaries;
* [ ] diagnostics are privacy-safe;
* [ ] all six Presentation documents agree on terminology and ownership.

---

# 51. Summary

Presentation converts accepted reading semantics into committed presentation semantics.

The complete boundary is:

```text
Immutable Artifacts
       ↓
Runtime / Application
       ↓
Presentation Command
       ↓
Presentation
       │
       ├── map semantic content
       ├── interpret geometry
       ├── select mode/strategy
       ├── compute layout
       ├── build Candidate
       ├── validate Candidate
       │
       ▼
Runtime Authority Revalidation
       │
       ├── rejected
       │      ↓
       │   discard Candidate
       │
       └── accepted
              ↓
         Atomic Commit
              ↓
      PresentationRevision + 1
              ↓
    PresentationSnapshot + RenderPlan
              ↓
           UI Adapter
              ↓
      Platform-specific rendering
```

The core ownership rule is:

```text
Runtime owns whether work may commit.

Presentation owns what committed
presentation state means.

UI Adapter owns how committed
presentation state becomes visible.
```

That separation allows CRAI to provide consistent reading behavior across:

```text
Desktop
Browser
Mobile
Future presentation surfaces
```

without coupling Presentation business logic to any individual UI framework.
