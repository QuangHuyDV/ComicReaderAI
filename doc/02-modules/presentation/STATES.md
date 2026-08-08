# Presentation States

> **Project:** CRAI
> **Module:** `presentation`
> **Path:** `doc/02-modules/presentation/STATES.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-08

---

# 1. Purpose

This document defines the Presentation-owned lifecycle state model.

It specifies:

* Presentation lifecycle states;
* state transitions;
* Presentation-local guards;
* Candidate Presentation lifecycle;
* commit behavior;
* supersession behavior;
* clear behavior;
* recovery behavior;
* concurrency rules;
* state/event relationships.

Presentation converts accepted immutable upstream Artifact references and presentation context into:

```text
PresentationSnapshot
+
RenderPlan
```

through:

```text
Presentation Operation
    ↓
Candidate Presentation State
    ↓
Validation
    ↓
Runtime Authority Revalidation
    ↓
Atomic Presentation Commit
```

This state model describes **Presentation state only**.

It does not define:

* Runtime Revision lifecycle;
* WorkItem lifecycle;
* Attempt lifecycle;
* Runtime cancellation authority;
* Runtime retry lifecycle;
* Reading Session lifecycle;
* Artifact Store lifecycle;
* Translation lifecycle;
* Recognition lifecycle;
* UI Adapter component lifecycle;
* native window lifecycle;
* browser DOM lifecycle.

---

# 2. State Ownership

Presentation owns:

```text
Presentation Context State
Presentation Operation Phase
Candidate Presentation State
Committed Presentation State
PresentationRevision
Presentation-local degradation
Presentation-local recovery
```

Runtime Control owns:

```text
Runtime Revision
WorkItem
Attempt
Authority
Cancellation authority
Retry
Completion acceptance
```

Artifact Store owns:

```text
Accepted Artifact publication
Artifact ownership
Artifact retention / lease semantics
```

UI Adapter owns:

```text
Native surface state
Widget / DOM lifecycle
UI apply state
Platform resource lifecycle
```

The core rule is:

```text
Runtime decides whether the work may still matter.

Presentation decides whether its candidate is semantically valid.

Presentation commits only its own state.

UI Adapter decides whether committed state becomes actual visible UI.
```

---

# 3. State Machine Scope

One Presentation state machine exists per:

```text
PresentationContextId
```

The Presentation Context may additionally reference:

```text
PresentationId?
SessionId
ContentIdentity?
CurrentPresentationRevision?
CurrentPresentationRef?
PreviousPresentationRef?
```

`RuntimeRevisionId` may be associated with active work for diagnostics and authority revalidation.

It is not part of Presentation state ownership.

---

# 4. Primary State Model

Presentation uses:

```text
EMPTY
  │
  │ BuildPresentation
  ▼
PREPARING
  │
  ├── committed ───────────────► READY
  ├── rejected ────────────────► EMPTY
  ├── superseded ──────────────► EMPTY or PREPARING
  └── fatal internal failure ──► FAILED

READY
  │
  ├── content update ─────────► UPDATING
  ├── layout change ──────────► REFLOWING
  ├── mode/profile change ────► RECONFIGURING
  ├── clear ──────────────────► CLEARING
  └── invariant failure ──────► FAILED

UPDATING
  │
  ├── committed ──────────────► READY
  ├── rejected ───────────────► READY
  ├── superseded ─────────────► READY or UPDATING
  └── active state corrupted ─► FAILED

REFLOWING
  │
  ├── committed ──────────────► READY
  ├── rejected/stale ─────────► READY
  ├── newer reflow ───────────► REFLOWING
  └── active state corrupted ─► FAILED

RECONFIGURING
  │
  ├── committed ──────────────► READY
  ├── fallback committed ─────► READY
  ├── rejected ───────────────► READY
  └── active state corrupted ─► FAILED

CLEARING
  │
  ├── logical clear complete ─► EMPTY
  └── state corruption ───────► FAILED

FAILED
  │
  ├── verified restore ───────► READY
  ├── reset ──────────────────► EMPTY
  └── clear ──────────────────► CLEARING
```

---

# 5. Stable and Transitional States

| State           | Meaning                                                            |       Stable |
| --------------- | ------------------------------------------------------------------ | -----------: |
| `EMPTY`         | No committed Presentation exists                                   |          Yes |
| `PREPARING`     | Initial Candidate Presentation is being prepared                   |           No |
| `READY`         | A valid committed Presentation exists                              |          Yes |
| `UPDATING`      | Candidate semantic/content update is being prepared                |           No |
| `REFLOWING`     | Candidate layout update is being prepared                          |           No |
| `RECONFIGURING` | Candidate mode/profile-wide representation is being prepared       |           No |
| `CLEARING`      | Current Presentation is being logically invalidated and cleaned up |           No |
| `FAILED`        | Presentation cannot currently guarantee internal correctness       | Yes, faulted |

A transitional state describes Presentation-local work.

It does not imply any particular Runtime WorkItem or Attempt state.

---

# 6. Candidate vs Committed State

Presentation distinguishes:

```text
Candidate Presentation State
```

from:

```text
Committed Presentation State
```

Candidate state is:

* immutable once prepared;
* private to the operation/commit path;
* not returned as current Presentation;
* not render-authoritative;
* not evidence of Runtime authority;
* safe to discard.

Committed state is:

```text
PresentationRevision
+
PresentationSnapshot
+
RenderPlan
```

and is the only Presentation state exposed as current.

---

# 7. Atomic Commit Boundary

Every successful mutation follows:

```text
Prepare Candidate
    ↓
Presentation Candidate Validation
    ↓
PresentationRevision Guard
    ↓
Target / Viewport Compatibility Guard
    ↓
Runtime Authority Revalidation where required
    ↓
Atomic Presentation Commit
    ↓
State transition
    ↓
Success event
```

No success event may be emitted before commit.

---

# 8. State: `EMPTY`

## Meaning

No committed Presentation exists for the Presentation Context.

The module may retain reusable non-content resources such as:

* configuration references;
* strategy registry;
* reusable typography metrics;
* layout algorithm instances;
* diagnostics metadata.

It MUST NOT expose a current Presentation.

## Entry

`EMPTY` is entered when:

* Presentation Context is created;
* clear completes;
* initial build is rejected;
* initial candidate is superseded with no replacement pending;
* reset after failure succeeds.

## Allowed Commands

```text
BuildPresentation
ClearPresentation
GetCurrentPresentation
GetPresentationDiagnostics
```

`ClearPresentation` is an idempotent no-op when already empty.

## Invalid Commands

Normally rejected or no-op:

```text
UpdatePresentationContent
RecomputePresentationLayout
ChangePresentationMode
UpdatePresentationFocus
ApplyPresentationProfile
```

because no committed Presentation exists.

## Invariants

While `EMPTY`:

```text
CurrentPresentationRef = absent
CurrentPresentationRevision = absent
CurrentPresentationId = absent
```

No `PresentationPrepared` event may be emitted without first leaving this state through a successful commit.

Late candidates targeting an invalidated Presentation lineage cannot commit.

---

# 9. State: `PREPARING`

## Meaning

Presentation is preparing the first Candidate Presentation for a context.

The committed state remains logically empty until commit succeeds.

## Entry

Entered from `EMPTY` after accepting:

```text
BuildPresentation
```

Business Pipeline Orchestration or Runtime/Application decides when this command occurs.

Presentation MUST NOT require a `TranslationCompleted` event to autonomously start itself.

## Required Context

Typical operation input includes:

```text
PresentationRequestId
PresentationOperationId
PresentationContextId
RuntimeExecutionIdentity?
CancellationContextRef?
PresentationInputArtifactSet
ContentIdentity
PresentationProfile
PresentationTarget
ViewportSnapshot?
RequestedMode
```

## Entry Actions

Presentation:

1. validates Presentation-owned input semantics;
2. resolves accepted Artifact references;
3. acquires required leases according to Runtime resource policy;
4. resolves Presentation mode;
5. maps semantic Presentation items;
6. prepares geometry/layout;
7. builds `CandidatePresentationState`.

## Candidate Validation

Before commit:

* item identities are valid;
* mappings are valid;
* coordinate spaces are explicit;
* RenderPlan references valid items;
* Snapshot and RenderPlan share candidate revision;
* target capabilities are sufficient;
* readability invariants are satisfied;
* no mutable provider/native objects are present.

## Commit

If candidate validation succeeds:

```text
Candidate
    ↓
Runtime Authority Revalidation
    ↓
PresentationRevision validation
    ↓
Atomic commit
```

The first committed revision normally becomes:

```text
PresentationRevision = 1
```

unless an implementation uses another equivalent monotonic initial token.

## Success

After commit:

```text
PREPARING → READY
```

Then:

```text
PresentationPrepared
```

may be published.

## Rejection

If Presentation input is deterministically invalid:

* candidate is discarded;
* no PresentationRevision is created;
* context remains logically empty;
* `PresentationRejected` may be published.

Examples:

* incompatible Artifact references;
* invalid mapping;
* invalid required geometry;
* unsupported mode with no allowed fallback;
* invalid target;
* invalid profile.

## Runtime Authority Rejection

If Runtime rejects commit authority:

* candidate is discarded;
* Presentation does not label Runtime Revision itself stale;
* no Presentation success event is emitted;
* Presentation returns to `EMPTY` unless a replacement operation is already selected.

## Supersession

A newer build may supersede current preparation.

The older operation may:

* cooperatively cancel;
* finish physically but lose commit eligibility;
* release all temporary resources;
* produce diagnostics only.

## Invariants

While `PREPARING`:

* no Candidate is exposed as current;
* committed Presentation remains absent;
* only a still-valid candidate may commit;
* multiple calculations may exist physically, but commit must be serialized;
* stale/superseded candidates cannot become current.

---

# 10. State: `READY`

## Meaning

A valid committed Presentation exists.

Conceptually:

```text
CurrentPresentation
├── PresentationId
├── PresentationRevision
├── PresentationSnapshot
└── RenderPlan
```

This is the normal stable Presentation state.

## Entry

Entered after:

* initial commit;
* content-update commit;
* reflow commit;
* mode/profile reconfiguration commit;
* verified restoration.

## Allowed Commands

```text
UpdatePresentationContent
RecomputePresentationLayout
ChangePresentationMode
UpdatePresentationFocus
ApplyPresentationProfile
ClearPresentation
GetCurrentPresentation
GetPresentationSnapshot
GetRenderPlan
GetPresentationItem
GetPresentationDiagnostics
```

## Invariants

While `READY`:

1. exactly one current committed Presentation exists per context;
2. Snapshot and RenderPlan belong to the same PresentationRevision;
3. current Presentation is immutable;
4. mappings are internally consistent;
5. item IDs are stable;
6. current RenderPlan references only current Presentation entities;
7. no private Candidate is visible through current-state queries.

---

# 11. State: `UPDATING`

## Meaning

A new Candidate Presentation is being prepared because accepted semantic content used by Presentation changed.

Examples:

* newer Translation Artifact;
* accepted translation correction;
* newer compatible SourceDocument Artifact;
* partial Translation Artifact advances completeness;
* Presentation visibility metadata changes.

## Entry

Entered from `READY` through:

```text
UpdatePresentationContent
```

Presentation does not autonomously enter this state solely because it observed a Translation event.

## Required Context

Typical input:

```text
PresentationId
PresentationContextId
ExpectedPresentationRevision
PresentationInputArtifactSet
ContentIdentity
UpdateCause
RuntimeExecutionIdentity?
CancellationContextRef?
```

## Entry Actions

Presentation:

1. confirms target Presentation exists;
2. validates `ExpectedPresentationRevision`;
3. validates Artifact semantic compatibility;
4. calculates affected Presentation items;
5. builds Candidate Snapshot;
6. builds Candidate RenderPlan if required;
7. preserves previous committed Presentation.

## Previous State Availability

While `UPDATING`:

```text
Current committed revision N
```

remains publicly readable.

Candidate revision:

```text
N + 1
```

is private until commit.

## Incremental Strategy

Presentation MAY update only affected items.

It MAY internally rebuild more data when simpler or safer.

Externally:

* unchanged semantic item identities remain stable;
* source traceability remains stable;
* semantic order does not silently change.

## Correction Precedence

Presentation follows accepted upstream lineage/version semantics.

It must not use local completion time alone to decide that an older automatic translation outranks an accepted correction.

## Commit

Before commit:

```text
ExpectedPresentationRevision
==
CurrentPresentationRevision
```

must still hold unless the command explicitly supports merge.

Runtime authority is revalidated where the operation is Runtime-authorized.

## Success

```text
UPDATING
    ↓
Atomic Commit
    ↓
READY
    ↓
PresentationUpdated
```

## Rejected Candidate

If candidate validation fails while current Presentation remains valid:

```text
UPDATING → READY
```

The existing revision remains unchanged.

## Presentation Revision Conflict

If another operation committed first:

```text
expected = N
current = N+1
```

the older candidate must not commit.

It may:

* be discarded;
* be recomputed against the new Presentation revision when explicitly requested;
* be superseded.

## Runtime Authority Rejection

Runtime rejection discards the candidate.

It does not itself move Presentation into `FAILED`.

## Fatal Failure

`FAILED` is entered only if:

* current committed Presentation becomes internally untrustworthy;
* Presentation registry/identity is corrupted;
* rollback/retention invariants cannot be guaranteed.

## Invariants

While `UPDATING`:

* current committed Presentation remains readable;
* Candidate state remains private;
* PresentationRevision changes only after commit;
* unchanged items preserve stable identity;
* stale candidate cannot overwrite current state.

---

# 12. State: `REFLOWING`

## Meaning

Presentation is preparing a layout-only or primarily layout-oriented Candidate.

Semantic source/translation content remains compatible.

Typical triggers:

* viewport resize;
* zoom;
* scroll-related geometry update;
* target movement;
* panel width;
* typography measurement changes;
* target capability change;
* overlay geometry change.

## Entry

Entered from `READY` through:

```text
RecomputePresentationLayout
```

## Required Context

```text
PresentationId
PresentationContextId
ExpectedPresentationRevision
PresentationTarget
ViewportSnapshot
PresentationProfile?
Reason
CancellationContextRef?
```

## Entry Actions

Presentation:

1. validates Presentation revision;
2. validates target revision;
3. validates viewport revision;
4. validates coordinate spaces;
5. calculates Candidate RenderPlan;
6. resolves overflow;
7. evaluates fallback if current mode becomes unsafe.

## Coalescing

Viewport changes may arrive faster than layout work completes.

Example:

```text
ViewportRevision 20
ViewportRevision 21
ViewportRevision 22
```

Presentation SHOULD prefer useful newest work:

```text
20 → obsolete
21 → obsolete
22 → calculate / commit
```

Intermediate obsolete computations do not require failure events.

## Previous Layout

The previous committed RenderPlan remains current until replacement commit.

## Layout Commit

A Candidate layout may commit only if:

```text
ExpectedPresentationRevision still matches
TargetRevision still compatible
ViewportRevision still acceptable
Runtime authority valid where required
Candidate geometry valid
```

## Success

```text
REFLOWING → READY
```

Then:

```text
PresentationLayoutChanged
```

is published.

## Fallback

If Overlay geometry becomes unsafe but Side Panel remains valid:

```text
REFLOWING
    ↓
Candidate effectiveMode = SidePanel
    ↓
Commit
    ↓
READY
```

This is degraded successful Presentation behavior, not automatically failure.

## Stale Layout

A stale viewport or target result:

* is discarded;
* does not increment PresentationRevision;
* does not emit success;
* may immediately be followed by the newest reflow.

## Invariants

While `REFLOWING`:

* semantic content remains unchanged;
* previous committed layout remains current;
* coordinate spaces remain explicit;
* no obsolete geometry is committed;
* item semantic identity remains stable.

---

# 13. State: `RECONFIGURING`

## Meaning

Presentation is preparing a candidate whose Presentation-wide representation changes.

Examples:

```text
SidePanel → Overlay
Overlay → SidePanel
SidePanel → TextReader
TextReader → Hybrid
```

or profile changes that require strategy-wide reconstruction.

## Entry

Entered from `READY` through:

```text
ChangePresentationMode
```

or:

```text
ApplyPresentationProfile
```

when the profile requires Presentation-wide restructuring.

## Entry Actions

Presentation:

1. preserves current committed Presentation;
2. validates requested mode/profile;
3. checks normalized target capabilities;
4. maps current semantic content into target strategy;
5. builds Candidate Snapshot;
6. builds Candidate RenderPlan;
7. validates readability and geometry.

## Requested vs Effective Mode

Presentation distinguishes:

```text
RequestedMode
EffectiveMode
```

A fallback may produce:

```text
RequestedMode = Overlay
EffectiveMode = SidePanel
```

if policy allows.

## Success

After valid commit:

```text
RECONFIGURING → READY
```

and appropriate committed facts may be published.

At minimum:

```text
PresentationModeChanged
```

when the active mode changed.

## Unsupported Mode

If no valid fallback exists:

* preserve current committed Presentation;
* candidate is rejected;
* return to `READY`;
* PresentationRevision does not increment.

## Invariants

While `RECONFIGURING`:

* active committed mode remains unchanged until commit;
* partial new strategy is not exposed;
* current Presentation remains readable where safe;
* item identity is preserved where semantics permit;
* fallback must be explicit.

---

# 14. Focus Updates

A focus-only update may be processed as a lightweight `UPDATING` operation or a specialized internal operation.

It does not require a separate top-level state in v2.

Command:

```text
UpdatePresentationFocus
```

Rules:

* raw mouse/keyboard events do not enter Presentation;
* target item must exist;
* expected PresentationRevision must match;
* change may produce a new PresentationRevision;
* semantic source content does not change.

A future implementation may introduce a dedicated interaction state only if required by observable lifecycle semantics.

---

# 15. State: `CLEARING`

## Meaning

Current Presentation state is being logically invalidated and Presentation-owned resources are being detached or released.

Logical invalidation takes priority over physical cleanup.

## Entry

May be entered from:

```text
READY
PREPARING
UPDATING
REFLOWING
RECONFIGURING
FAILED
```

through:

```text
ClearPresentation
```

## Clear Reasons

Typical reasons:

```text
SessionStopped
SessionReplaced
ContentReplaced
TargetDestroyed
PrivacyInvalidation
ApplicationShutdown
UserRequested
```

## Logical Clear

The critical operation is:

```text
Current Presentation
    ↓
Revoke Presentation-local commit eligibility
    ↓
CurrentPresentationRef removed
    ↓
Old candidates cannot commit
```

Logical clear MUST occur before slow physical cleanup is allowed to delay state correctness.

## Entry Actions

Presentation should:

1. invalidate current Presentation state;
2. invalidate outstanding Presentation operations for that context;
3. detach current Presentation references;
4. release Presentation-owned temporary state;
5. release Artifact leases according to Runtime resource policy;
6. notify UI integration through committed clear semantics.

Presentation does not directly destroy:

* native windows;
* widgets;
* DOM nodes;
* platform overlay resources.

Those belong to UI Adapter/platform.

## Success

```text
CLEARING → EMPTY
```

Then:

```text
PresentationCleared
```

is published according to event ordering rules.

## Idempotency

Repeated clear requests must safely converge on:

```text
EMPTY
```

One logical Presentation lifecycle must not emit duplicate committed clear facts for the same clear transition.

## Physical Cleanup Failure

Failure to dispose a non-authoritative temporary resource does not automatically restore the Presentation as active.

The Presentation is already logically unavailable.

Cleanup failures should be:

* diagnosed;
* reported to Resource Manager where appropriate;
* escalated to `FAILED` only if Presentation internal correctness is compromised.

## Invariants

While `CLEARING`:

* old Presentation is not eligible for new commit;
* old Candidate cannot become current;
* Presentation is logically unavailable;
* native resource ownership remains outside Presentation.

---

# 16. Replacement Flow

A content replacement generally follows:

```text
READY
    ↓
CLEARING
    ↓
EMPTY
    ↓
PREPARING
    ↓
READY
```

An implementation may optimize internal operations, but externally it must preserve:

```text
one current Presentation per PresentationContext
```

unless multiple concurrent Presentation contexts are explicitly supported.

---

# 17. State: `FAILED`

## Meaning

Presentation detected an internal condition where it cannot guarantee correctness of its owned Presentation state.

`FAILED` is reserved for broken Presentation invariants.

It is not used for:

* stale Runtime Revision;
* Runtime cancellation;
* Presentation revision conflict;
* unsupported mode;
* invalid external viewport;
* expected candidate supersession;
* target temporarily unavailable;
* fallback to Side Panel.

## Examples

Possible fatal causes:

* committed Snapshot/RenderPlan mismatch;
* corrupted Presentation registry;
* impossible PresentationRevision ordering;
* current `PresentationId` disagrees with current snapshot;
* rollback state corrupt;
* internal graph corruption;
* current committed Presentation cannot be trusted.

## Entry Actions

Presentation MUST:

1. prevent normal mutation commits;
2. invalidate any untrusted current state;
3. discard private Candidates where safe;
4. preserve last known-good state only if verified;
5. capture diagnostics;
6. release temporary resources;
7. publish `PresentationFailed` when externally relevant.

## Allowed Commands

```text
ClearPresentation
ResetPresentation
RestoreLastKnownGoodPresentation
GetPresentationDiagnostics
GetPresentationState
```

## Prohibited Commands

Normal mutation commands are rejected until recovery succeeds.

## Recovery

Preferred recovery:

```text
FAILED
    ↓
verify last known-good Presentation
    ├── valid → READY
    └── invalid
          ↓
        reset / clear
          ↓
        EMPTY
```

## Invariants

While `FAILED`:

* no unverified Presentation is reported as current;
* new normal mutation commits are blocked;
* recovery is explicit and verified;
* stale candidates remain invalid.

---

# 18. `DEGRADED_READY`

`DEGRADED_READY` is not a required top-level state in v2.

A valid Presentation with reduced capability may remain:

```text
READY
```

with:

```text
fallback
issues[]
completeness = Degraded
```

Examples:

* Overlay fallback to Side Panel;
* unavailable preferred font;
* marker suppression;
* incomplete translation;
* unavailable optional source text.

A dedicated `DEGRADED_READY` state should only be added if consumers require different lifecycle behavior.

---

# 19. Presentation Operation Phases

Presentation may expose diagnostic operation phases independent from state.

Example:

```text
VALIDATING_INPUT
RESOLVING_ARTIFACTS
MAPPING_ITEMS
RESOLVING_MODE
PROJECTING_GEOMETRY
BUILDING_LAYOUT
VALIDATING_CANDIDATE
WAITING_FOR_AUTHORITY_REVALIDATION
COMMITTING
CLEANING_UP
```

These are not:

```text
Runtime Attempt states
```

and must not become a competing Runtime lifecycle.

---

# 20. Transition Table

| Current         | Trigger                           | Guard                                                                   | Action                           | Next                               |
| --------------- | --------------------------------- | ----------------------------------------------------------------------- | -------------------------------- | ---------------------------------- |
| `EMPTY`         | `BuildPresentation`               | Presentation input valid enough to start                                | Begin Candidate preparation      | `PREPARING`                        |
| `EMPTY`         | `ClearPresentation`               | Always                                                                  | Idempotent no-op                 | `EMPTY`                            |
| `PREPARING`     | Candidate committed               | candidate valid + Presentation revision valid + Runtime authority valid | Atomic commit                    | `READY`                            |
| `PREPARING`     | Presentation validation rejection | deterministic invalid input                                             | Discard candidate                | `EMPTY`                            |
| `PREPARING`     | Runtime authority rejected        | external authority invalid                                              | Discard candidate                | `EMPTY` or replacement preparation |
| `PREPARING`     | New build supersedes              | newer selected operation                                                | Obsolete previous candidate      | `PREPARING`                        |
| `PREPARING`     | Clear                             | Always                                                                  | Invalidate operation             | `CLEARING`                         |
| `PREPARING`     | Fatal Presentation corruption     | internal correctness lost                                               | Record failure                   | `FAILED`                           |
| `READY`         | `UpdatePresentationContent`       | expected Presentation revision valid                                    | Begin Candidate update           | `UPDATING`                         |
| `READY`         | `RecomputePresentationLayout`     | request valid                                                           | Begin Candidate reflow           | `REFLOWING`                        |
| `READY`         | `ChangePresentationMode`          | request differs or requires reconstruction                              | Begin Candidate reconfiguration  | `RECONFIGURING`                    |
| `READY`         | `ApplyPresentationProfile`        | profile requires structural change                                      | Begin Candidate reconfiguration  | `RECONFIGURING`                    |
| `READY`         | `ClearPresentation`               | Always                                                                  | Logically invalidate             | `CLEARING`                         |
| `UPDATING`      | Candidate committed               | guards valid                                                            | Atomic commit                    | `READY`                            |
| `UPDATING`      | Candidate rejected                | current Presentation remains valid                                      | Discard                          | `READY`                            |
| `UPDATING`      | Revision conflict                 | newer Presentation already committed                                    | Discard or explicitly restart    | `READY` or `UPDATING`              |
| `UPDATING`      | Fatal internal corruption         | current Presentation untrusted                                          | Record failure                   | `FAILED`                           |
| `REFLOWING`     | Candidate committed               | newest compatible viewport/target                                       | Atomic commit                    | `READY`                            |
| `REFLOWING`     | Candidate stale                   | newer viewport/Presentation revision                                    | Discard                          | `READY` or `REFLOWING`             |
| `REFLOWING`     | Newer viewport                    | coalescing allowed                                                      | Replace pending layout work      | `REFLOWING`                        |
| `REFLOWING`     | Recoverable layout failure        | previous plan remains valid                                             | Preserve current                 | `READY`                            |
| `REFLOWING`     | Fatal internal corruption         | current Presentation untrusted                                          | Record failure                   | `FAILED`                           |
| `RECONFIGURING` | Candidate committed               | valid/fallback valid                                                    | Atomic commit                    | `READY`                            |
| `RECONFIGURING` | Unsupported mode                  | no fallback                                                             | Preserve current                 | `READY`                            |
| `RECONFIGURING` | Candidate rejected                | previous state valid                                                    | Preserve current                 | `READY`                            |
| `RECONFIGURING` | Fatal corruption                  | current state untrusted                                                 | Record failure                   | `FAILED`                           |
| `CLEARING`      | Logical clear completes           | old Presentation invalidated                                            | Finish detach                    | `EMPTY`                            |
| `FAILED`        | Verified restore                  | known-good state valid                                                  | Restore                          | `READY`                            |
| `FAILED`        | Reset                             | reset succeeds                                                          | Remove active Presentation state | `EMPTY`                            |
| `FAILED`        | Clear                             | Always                                                                  | Logical clear                    | `CLEARING`                         |

---

# 21. Presentation Revision Guard

Commands targeting existing Presentation state should carry:

```text
ExpectedPresentationRevision
```

Normal guard:

```text
ExpectedPresentationRevision
==
CurrentPresentationRevision
```

If not equal:

```text
Presentation Revision Conflict
```

The candidate MUST NOT commit unless an explicitly supported deterministic merge exists.

MVP does not require automatic concurrent merge.

---

# 22. Runtime Authority Guard

Presentation does not maintain a competing current Runtime Revision registry.

At commit:

```text
Candidate
    ↓
Runtime Authority Revalidation
```

Runtime may return:

```text
Accepted
RejectedStale
RejectedCanceled
RejectedSessionInactive
RejectedRuntimeRevision
RejectedOther
```

If rejected:

```text
Candidate cannot commit
```

Presentation does not modify Runtime state in response.

---

# 23. Artifact Compatibility Guard

Presentation validates semantic compatibility of supplied accepted Artifact references.

It may check:

* ContentIdentity;
* required source mappings;
* contract compatibility;
* geometry availability;
* translation/source relationship.

It MUST NOT declare an Artifact current solely because:

```text
artifact.runtimeRevisionId == operation.runtimeRevisionId
```

Artifact semantic compatibility and Runtime authority are separate concerns.

---

# 24. Target Guard

Candidate must target a compatible current Presentation target.

Checks may include:

```text
TargetId
TargetRevision
Capabilities
CoordinateSpace
```

If target changed incompatibly before commit:

* candidate is rejected or superseded;
* unsafe layout must not commit.

---

# 25. Viewport Guard

Viewport validation requires:

* finite dimensions;
* valid scale;
* valid coordinate space;
* valid transforms;
* compatible target;
* valid viewport revision semantics.

A zero-sized viewport may be invalid for a mode requiring geometry.

A hidden/minimized target may instead be represented through a separate normalized capability/state contract if needed.

---

# 26. Geometry Guard

Presentation validates only Presentation-facing geometry.

Required properties include:

* finite coordinates;
* non-negative dimensions;
* valid coordinate-space metadata;
* valid source references;
* compatible transforms;
* valid target bounds where required.

Presentation does not modify Recognition-owned source geometry.

---

# 27. Mode Guard

A requested mode is valid when:

* mode is recognized;
* required semantic data exists;
* target capabilities allow it;
* required geometry is available;
* readability constraints can be satisfied or a documented fallback exists.

Failure may result in:

```text
Fallback
```

or:

```text
PresentationRejected
```

depending on policy.

---

# 28. Concurrency Model

State transitions for one `PresentationContextId` must be logically serialized.

Physical computation may be parallel.

Example:

```text
Candidate A layout task
Candidate B text-measure task
Candidate C diagnostics task
```

may execute concurrently.

But committed state follows one deterministic order.

---

# 29. Commit Serialization

Only one Candidate may win a commit for a given expected Presentation revision.

Example:

```text
Current = 7

Candidate A expected 7
Candidate B expected 7

B commits → 8

A commit attempt:
expected 7
current 8
→ reject
```

This prevents lost Presentation updates.

---

# 30. Supersession

Supersession is normal behavior.

Candidate may become obsolete because:

* newer Presentation command;
* newer Presentation revision;
* newer target revision;
* newer viewport revision;
* Runtime authority revoked;
* Presentation clear.

Supersession does not imply `FAILED`.

---

# 31. Cooperative Cancellation

Presentation may receive Runtime cancellation context.

During expensive work it should check cancellation at bounded checkpoints.

Cancellation MAY stop physical work early.

If physical work still finishes:

```text
authority/commit guard
```

must prevent obsolete commit.

Presentation cancellation observation does not mutate Runtime Attempt state.

---

# 32. Last Known-Good Presentation

When leaving `READY` for:

```text
UPDATING
REFLOWING
RECONFIGURING
```

the current committed Presentation remains the known-good state.

It is not a rollback copy necessarily.

An immutable current reference may be retained.

Its purpose is:

* continued readability;
* safe candidate rejection;
* comparison;
* recovery.

---

# 33. No Partial Visibility

Consumers may observe:

```text
Committed Revision N
```

or:

```text
Committed Revision N+1
```

They must not observe partially mutated state between them.

---

# 34. State and Event Relationship

State and event are distinct.

State means:

> What Presentation currently owns as lifecycle condition.

Event means:

> What committed Presentation fact occurred.

Typical relationship:

| Transition              | Event                                           |
| ----------------------- | ----------------------------------------------- |
| `PREPARING → READY`     | `PresentationPrepared`                          |
| `UPDATING → READY`      | `PresentationUpdated`                           |
| `REFLOWING → READY`     | `PresentationLayoutChanged`                     |
| `RECONFIGURING → READY` | `PresentationModeChanged` when mode changed     |
| `CLEARING → EMPTY`      | `PresentationCleared`                           |
| Candidate rejection     | `PresentationRejected` when externally relevant |
| Internal fatal failure  | `PresentationFailed`                            |

---

# 35. Event Timing

Correct:

```text
Validate Candidate
    ↓
Authority Revalidate
    ↓
Commit
    ↓
Transition State
    ↓
Publish success fact
```

Incorrect:

```text
Publish success
    ↓
Commit later
```

No success event is emitted for discarded Candidates.

---

# 36. Event Bus Boundary

Presentation state transitions should originate through explicit Presentation contracts.

Presentation does not require:

```text
TranslationCompleted
    ↓
Presentation starts itself
```

as an architectural mechanism.

Events may notify or inform Application/Runtime orchestration.

They do not replace Business Pipeline Orchestration.

---

# 37. Command Acceptance Matrix

| Command                       |                                `EMPTY` |              `PREPARING` |                       `READY` |         `UPDATING` |               `REFLOWING` |    `RECONFIGURING` |          `CLEARING` |               `FAILED` |
| ----------------------------- | -------------------------------------: | -----------------------: | ----------------------------: | -----------------: | ------------------------: | -----------------: | ------------------: | ---------------------: |
| `BuildPresentation`           |                                 Accept |          Supersede/queue | Replace through orchestration |    Queue/supersede |           Queue/supersede |              Queue |               Queue |           Reject/reset |
| `UpdatePresentationContent`   |                                 Reject |             Queue/reject |                        Accept |    Supersede/queue |                     Queue |              Queue |              Reject |                 Reject |
| `RecomputePresentationLayout` |                                 Reject |         Coalesce pending |                        Accept |     Queue/coalesce |                  Coalesce |              Queue |              Reject |                 Reject |
| `ChangePresentationMode`      |                                 Reject |                    Queue |                        Accept |              Queue |                     Queue |    Supersede/queue |              Reject |                 Reject |
| `ApplyPresentationProfile`    | Store externally / no current mutation | Merge pending where safe |                        Accept |        Queue/merge | Coalesce where compatible |    Merge/supersede | No current mutation |        Reject mutation |
| `UpdatePresentationFocus`     |                                 Reject |             Reject/queue |                        Accept |              Queue |                     Queue |              Queue |              Reject |                 Reject |
| `ClearPresentation`           |                                  No-op |                   Accept |                        Accept |             Accept |                    Accept |             Accept |          Idempotent |                 Accept |
| Queries                       |                           Empty result |           Current absent |                       Current | Previous committed |        Previous committed | Previous committed |   Empty/unavailable | Verified/degraded only |

---

# 38. Idempotency

## Clear

Repeated:

```text
ClearPresentation
```

must converge to:

```text
EMPTY
```

## Duplicate Request

A duplicate request that has already committed MUST NOT create another PresentationRevision.

## Equivalent Viewport

Equivalent layout input should not create a new revision unless some other committed Presentation value actually changes.

## Same Mode

Changing to already active effective mode with equivalent profile/target is:

```text
NO_OP
```

unless explicit recomputation is requested.

---

# 39. No-op Rule

A successful command that changes no committed Presentation value SHOULD NOT increment PresentationRevision.

Examples:

* same focus state;
* same effective mode;
* equivalent viewport;
* equivalent profile;
* duplicate accepted Artifact set.

---

# 40. Recovery Classes

Presentation distinguishes:

```text
Expected Control Outcome
Recoverable Presentation Rejection
Recoverable Presentation Degradation
Fatal Presentation Failure
```

---

# 41. Expected Control Outcomes

Examples:

* superseded Candidate;
* coalesced reflow;
* Runtime authority rejection;
* cancellation;
* Presentation revision conflict;
* duplicate command.

These do not enter `FAILED`.

---

# 42. Recoverable Presentation Rejection

Examples:

* unsupported requested mode;
* invalid optional layout request;
* invalid target capability;
* invalid viewport while previous layout remains usable;
* incompatible incremental update.

Behavior:

```text
discard candidate
+
preserve current committed state
```

---

# 43. Recoverable Degradation

Examples:

* Overlay unavailable → Side Panel;
* preferred font unavailable → fallback font;
* marker geometry unavailable → hide markers;
* partial Translation → partial Presentation.

Result remains valid and usually returns to:

```text
READY
```

with fallback/issues metadata.

---

# 44. Fatal Presentation Failure

Examples:

* current Snapshot/RenderPlan mismatch;
* current revision registry corruption;
* active Presentation references impossible;
* internal immutable state mutated unexpectedly;
* recovery source untrustworthy.

Behavior:

```text
→ FAILED
```

---

# 45. Fallback Policy

Fallback order is strategy- and content-dependent.

There is no universal requirement that:

```text
Hybrid
→ Overlay
→ SidePanel
→ TextReader
```

always applies.

For image/comic reading, typical safe preference may be:

```text
Overlay
    ↓
Focused Overlay
    ↓
Side Panel
```

For structured text:

```text
Styled Text Reader
    ↓
Simplified Text Reader
```

Presentation records:

```text
requestedMode
effectiveMode
fallbackReason
```

---

# 46. Presentation State Snapshot

Presentation may expose a diagnostic state snapshot:

```text
PresentationStateSnapshot
├── state
├── presentationContextId
├── presentationId?
├── presentationRevision?
├── runtimeRevisionId?
├── activeOperationId?
├── activeOperationType?
├── operationPhase?
├── targetId?
├── targetRevision?
├── viewportRevision?
├── lastStableState?
├── fallbackActive
├── lastIssueCode?
└── enteredAt
```

It MUST NOT expose:

* mutable Candidate object;
* native UI object;
* provider DTO;
* complete user content by default.

---

# 47. UI Interpretation

Presentation states may guide UI, but UI Adapter owns actual behavior.

Suggested semantics:

| Presentation State | Possible UI interpretation                  |
| ------------------ | ------------------------------------------- |
| `EMPTY`            | No Presentation binding                     |
| `PREPARING`        | Optional non-blocking preparation indicator |
| `READY`            | Apply current committed Presentation        |
| `UPDATING`         | Keep current revision visible               |
| `REFLOWING`        | Keep previous RenderPlan visible            |
| `RECONFIGURING`    | Keep previous effective mode visible        |
| `CLEARING`         | Remove binding for invalidated Presentation |
| `FAILED`           | Safe Presentation failure indication        |

UI Adapter must not derive Presentation business state only from widget lifecycle.

---

# 48. Presentation Commit vs UI Apply

`READY` means:

```text
Presentation logical commit exists
```

It does not guarantee:

```text
UI Adapter successfully applied that revision
```

UI apply may independently report:

```text
Applied
RejectedStale
RejectedTargetMismatch
TargetUnavailable
Failed
```

A UI apply failure does not automatically move Presentation from `READY` to `FAILED`.

Recovery belongs to Presentation/Application/UI integration policy.

---

# 49. Example — Initial Comic Presentation

```text
State: EMPTY
    ↓
BuildPresentation
    ↓
State: PREPARING
    ↓
Resolve accepted Artifacts
    ↓
Map PresentationItems
    ↓
Resolve SidePanel
    ↓
Build Candidate Snapshot
    ↓
Build Candidate RenderPlan
    ↓
Validate Candidate
    ↓
Runtime Authority Revalidation
    ↓
Atomic Commit Revision 1
    ↓
State: READY
    ↓
PresentationPrepared
```

---

# 50. Example — Translation Update

```text
State: READY
PresentationRevision = 4
    ↓
UpdatePresentationContent
with newer accepted TranslationArtifactRef
    ↓
State: UPDATING
    ↓
Map affected items
    ↓
Candidate Revision 5
    ↓
ExpectedPresentationRevision = 4
CurrentPresentationRevision = 4
    ↓
Runtime Authority Revalidation
    ↓
Commit
    ↓
State: READY
PresentationRevision = 5
    ↓
PresentationUpdated
```

---

# 51. Example — Concurrent Presentation Revision Conflict

```text
State: READY
Revision = 7

Operation A expected 7
Operation B expected 7

Operation B commits
    ↓
Revision = 8

Operation A reaches commit
    ↓
expected 7 != current 8
    ↓
Candidate discarded
    ↓
State remains READY
Revision remains 8
```

No Runtime Revision change is required.

---

# 52. Example — Runtime Supersession

```text
Runtime Revision 14
    ↓
Presentation preparation starts
    ↓
Runtime Revision 15 becomes current
    ↓
old Candidate physically completes
    ↓
Runtime Authority Revalidation
    ↓
RejectedStale
    ↓
Candidate discarded
```

Presentation does not transition Runtime Revision state.

---

# 53. Example — Rapid Viewport Changes

```text
State: READY

ViewportRevision 20
    ↓
State: REFLOWING

ViewportRevision 21
ViewportRevision 22
ViewportRevision 23
    ↓
20, 21, 22 superseded/coalesced
    ↓
Candidate using 23
    ↓
commit
    ↓
State: READY
    ↓
PresentationLayoutChanged
```

No successful events are emitted for the discarded layouts.

---

# 54. Example — Overlay Fallback

```text
State: READY
Mode = SidePanel
    ↓
ChangePresentationMode(Overlay)
    ↓
State: RECONFIGURING
    ↓
Overlay geometry readable? NO
    ↓
Fallback allowed? YES
    ↓
Candidate EffectiveMode = SidePanel
    ↓
Commit only if committed state meaningfully changes
    ↓
State: READY
```

If no visible state changed, this may instead resolve as a rejection/no-op without incrementing PresentationRevision.

---

# 55. Example — Clear During Preparation

```text
State: PREPARING
    ↓
ClearPresentation
    ↓
Candidate loses Presentation-local commit eligibility
    ↓
State: CLEARING
    ↓
Current Presentation already absent
    ↓
temporary resources released
    ↓
State: EMPTY
```

A late preparation result is discarded.

---

# 56. Example — Clear Current Presentation

```text
State: READY
Revision = 12
    ↓
ClearPresentation
    ↓
State: CLEARING
    ↓
Current Presentation logically invalidated
    ↓
Old Candidates invalidated
    ↓
PresentationCleared
    ↓
State: EMPTY
```

UI Adapter independently removes actual surface representation.

---

# 57. Example — UI Apply Failure

```text
Presentation commit Revision 18
    ↓
State: READY
    ↓
UI Adapter apply Revision 18
    ↓
TargetUnavailable
```

Presentation remains logically committed unless Application/Presentation policy explicitly clears or replaces it.

This is not automatically Presentation internal failure.

---

# 58. Example — Fatal Internal Corruption

```text
State: READY
    ↓
Invariant check detects:
Snapshot revision != RenderPlan revision
    ↓
Current state cannot be trusted
    ↓
State: FAILED
    ↓
PresentationFailed
    ↓
verified restore?
    ├── yes → READY
    └── no  → reset → EMPTY
```

---

# 59. Testing — State Transitions

Tests MUST cover:

```text
EMPTY → PREPARING
PREPARING → READY
PREPARING → EMPTY on rejection
READY → UPDATING → READY
READY → REFLOWING → READY
READY → RECONFIGURING → READY
READY → CLEARING → EMPTY
FAILED → READY via verified restore
FAILED → EMPTY via reset
```

---

# 60. Testing — Ownership

Tests MUST verify:

* Presentation never mutates Runtime Revision state;
* Presentation never mutates WorkItem state;
* Presentation never mutates Attempt state;
* Presentation never grants cancellation authority;
* Presentation never performs Runtime retry;
* Presentation never publishes accepted upstream Artifacts;
* UI Adapter lifecycle state does not mutate Presentation lifecycle implicitly.

---

# 61. Testing — Commit Authority

Tests MUST verify:

* Runtime authority rejection prevents commit;
* Presentation revision conflict prevents commit;
* target revision mismatch prevents unsafe commit;
* stale viewport cannot commit;
* clear invalidates old candidate commit eligibility;
* candidate validation occurs before commit;
* success event occurs only after commit.

---

# 62. Testing — Concurrency

Tests SHOULD include:

* content update during reflow;
* reflow during content update;
* clear during preparation;
* clear during update;
* target replacement during reconfiguration;
* Runtime Revision superseded during Candidate build;
* multiple rapid viewport revisions;
* two operations expecting the same PresentationRevision.

---

# 63. Testing — Previous State Preservation

Tests MUST verify current committed Presentation remains intact when:

* update Candidate rejected;
* reflow Candidate stale;
* mode change rejected;
* profile application rejected;
* Runtime authority rejects candidate;
* Candidate validation fails.

---

# 64. Testing — Clear

Tests MUST verify:

* logical clear occurs before slow physical cleanup;
* repeated clear is idempotent;
* late Candidates cannot commit;
* current queries return empty after logical clear;
* Artifact leases are released;
* native UI resource cleanup remains outside Presentation.

---

# 65. Testing — Failure

Tests MUST distinguish:

```text
Expected supersession
Runtime authority rejection
Presentation validation rejection
Recoverable fallback
Fatal Presentation corruption
```

Only the last category should normally produce `FAILED`.

---

# 66. Observability

State transitions should expose bounded diagnostics such as:

```text
previousState
nextState
presentationContextId
presentationId?
presentationRevision?
runtimeRevisionId?
workItemId?
attemptId?
operationId
operationType
operationPhase
targetRevision?
viewportRevision?
fallbackReason?
durationMs
result
issueCode?
```

Normal state diagnostics must not include full reading content.

---

# 67. Metrics

Recommended metrics include:

```text
presentation_state_transition_total
presentation_prepare_duration_ms
presentation_update_duration_ms
presentation_reflow_duration_ms
presentation_reconfigure_duration_ms
presentation_commit_duration_ms
presentation_clear_duration_ms
presentation_candidate_rejected_total
presentation_candidate_superseded_total
presentation_authority_rejected_total
presentation_revision_conflict_total
presentation_fallback_total
presentation_failed_total
presentation_active_context_count
```

Useful labels:

* operation type;
* mode;
* result;
* fallback category;
* rejection source;
* previous state;
* next state.

Metrics MUST NOT contain user content.

---

# 68. Resource Lifetime

Presentation state may retain:

```text
current committed Presentation
previous committed Presentation reference where required
in-flight Candidate
temporary layout resources
Artifact leases
diagnostic metadata
```

Temporary Candidate resources are operation-scoped.

Committed Presentation state is Presentation-owned for display lifetime.

Accepted Artifact payload ownership stays with Artifact Store.

Native UI resource ownership stays with UI Adapter/platform.

---

# 69. Persistence

Presentation state machine requires no durable persistence for MVP.

Presentation MUST NOT directly access persistent storage.

Storage may optionally persist selected Presentation-related information through explicit Storage contracts.

Restored Presentation state must be validated before becoming `READY`.

A persisted snapshot is not authoritative merely because it exists.

---

# 70. Multiple Presentation Contexts

Each logical Presentation Context owns an independent state machine.

Example:

```text
Context A: comic-side-panel
Context B: focused-overlay
Context C: text-reader
```

Multiple contexts may exist only when Application architecture explicitly supports them.

Global Presentation state MUST NOT replace per-context lifecycle state.

---

# 71. Future States

Potential future states include:

## `SUSPENDED`

A committed Presentation exists but active Presentation mutations are intentionally paused.

## `RESTORING`

A persisted Presentation candidate is being validated.

It must remain Candidate state until all current authority/compatibility requirements pass.

## `DEGRADED_READY`

May become a distinct state if consumers require lifecycle-specific degraded handling.

## `EXPORTING`

Should remain outside Presentation unless Export becomes an explicitly Presentation-owned capability.

---

# 72. Architecture Decisions

## 72.1 Transitional State Does Not Replace Current Presentation

While:

```text
UPDATING
REFLOWING
RECONFIGURING
```

current committed Presentation remains readable.

## 72.2 Reflow Is Separate from Semantic Update

`UPDATING`:

```text
Presentation semantic/content mapping changes
```

`REFLOWING`:

```text
layout/geometry changes while semantic content remains compatible
```

## 72.3 Reconfiguration Is Separate

Mode/profile-wide changes may rebuild:

* strategy;
* item representation;
* marker plan;
* overlay plan;
* layout.

## 72.4 Runtime Authority Is External

Presentation never uses its state machine as the authoritative current Runtime Revision registry.

## 72.5 Validation Rejection Is Not Failure

Invalid candidate or stale operation usually returns to stable state.

## 72.6 `FAILED` Means Owned Invariant Failure

`FAILED` is reserved for state Presentation itself can no longer trust.

## 72.7 Commit and UI Apply Are Separate

`READY` describes Presentation commit state.

It does not guarantee actual surface application.

---

# 73. Architecture Invariants

1. Presentation state machine owns Presentation lifecycle only.

2. Runtime Revision lifecycle is external.

3. WorkItem lifecycle is external.

4. Attempt lifecycle is external.

5. Runtime cancellation authority is external.

6. Candidate state is never current state.

7. Prepared does not mean committed.

8. Committed does not mean UI applied.

9. Runtime authority must be revalidated at commit where required.

10. Presentation cannot override Runtime authority rejection.

11. PresentationRevision is separate from Runtime RevisionId.

12. PresentationRevision changes only after committed Presentation mutation.

13. PresentationRevision never decreases.

14. Current Snapshot and RenderPlan share the same PresentationRevision.

15. Current Presentation is immutable.

16. Candidate Presentation is immutable once prepared.

17. Previous committed Presentation remains available during safe mutations.

18. Stale Candidate never overwrites newer committed Presentation.

19. Presentation revision conflict never causes lost update.

20. Target and viewport revisions protect layout commit.

21. Semantic item identity survives non-semantic layout changes.

22. Semantic reading order is not silently changed by reflow.

23. Coordinate spaces are explicit.

24. Readability fallback is preferred over unsafe overlay placement.

25. Expected supersession is not failure.

26. Runtime authority rejection is not Presentation internal failure.

27. Ordinary validation rejection does not enter `FAILED`.

28. `FAILED` is reserved for broken Presentation-owned invariants.

29. Logical clear invalidates Presentation before physical cleanup completes.

30. Clearing does not make Presentation the owner of native UI resources.

31. Artifact leases follow Runtime resource policy.

32. Presentation does not persist state directly.

33. Presentation does not autonomously orchestrate itself from Translation events.

34. Success events describe already committed Presentation state.

35. Normal diagnostics contain no full user reading content.

---

# 74. Related Documents

```text
doc/02-modules/presentation/MODULE.md
doc/02-modules/presentation/CONTRACT.md
doc/02-modules/presentation/EVENTS.md
doc/02-modules/presentation/ERRORS.md
doc/02-modules/presentation/README.md

doc/01-architecture/core/STATE_MACHINE.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/MEMORY_MODEL.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/02-modules/translation/CONTRACT.md
doc/02-modules/ui-adapter/CONTRACT.md
doc/02-modules/reading-session/CONTRACT.md
```

---

# 75. Completion Criteria

This state specification is synchronized when:

* every state represents Presentation-owned lifecycle only;
* Runtime authority is not duplicated;
* WorkItem/Attempt state is absent from Presentation ownership;
* Candidate state is distinct from committed state;
* `PresentationRevision` is distinct from Runtime Revision;
* commit is atomic;
* Runtime authority rejection blocks commit;
* Presentation revision conflict blocks obsolete commit;
* target and viewport supersession are deterministic;
* previous committed Presentation survives recoverable mutations;
* clear logically invalidates old Presentation before cleanup completion;
* `FAILED` is used only for Presentation invariant failure;
* events publish only after committed transitions;
* expected supersession does not appear as module failure;
* UI apply remains outside the Presentation state machine;
* tests cover ownership, concurrency, authority, commit, clear, recovery, and event timing.

---

# 76. Summary

Presentation lifecycle is:

```text
EMPTY
  ↓
PREPARING
  ↓
READY
  ↔ UPDATING
  ↔ REFLOWING
  ↔ RECONFIGURING
  ↓
CLEARING
  ↓
EMPTY
```

with:

```text
FAILED
```

reserved for internal Presentation correctness failure.

Every mutation follows:

```text
Current Committed Presentation
        +
Presentation Operation
        ↓
Candidate Presentation State
        ↓
Presentation Validation
        ↓
Runtime Authority Revalidation
        ↓
PresentationRevision Guard
        ↓
Atomic Commit
        ↓
New Committed Presentation
```

The critical ownership boundary is:

```text
Runtime
    owns whether work may still commit

Presentation
    owns what Presentation state is committed

UI Adapter
    owns whether that committed state becomes actual visible UI
```

And the central state invariant is:

```text
Candidate state is disposable.

Committed Presentation state is authoritative only inside Presentation.

Runtime authority remains external.

UI rendering remains external.
```
