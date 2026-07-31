# Presentation States

* **Module:** Presentation
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture
* **Related documents:**

  * `modules/presentation/MODULE.md`
  * `modules/presentation/CONTRACT.md`
  * `modules/presentation/EVENTS.md`
  * `docs/architecture/STATE_MACHINE.md`
  * `docs/architecture/EVENT_BUS.md`

---

## 1. Purpose

This document defines the lifecycle, states, transitions, guards, actions, and recovery behavior of the Presentation Module.

The Presentation Module converts translated content, source geometry, viewport information, and presentation preferences into a stable `PresentationDocument` that can be rendered by a UI Adapter.

This document describes module-level state only.

It does not define:

* application-level state;
* reading session state;
* translation state;
* OCR state;
* browser state;
* desktop window state;
* UI component state.

---

## 2. State Machine Scope

The Presentation state machine manages the lifecycle of one active presentation context.

A presentation context is identified by:

* `PresentationId`;
* `SessionId`;
* `ContentId`;
* `ContentRevision`.

The state machine controls:

* initial presentation construction;
* translated-content updates;
* layout recomputation;
* presentation mode changes;
* clearing and replacement;
* failure recovery;
* stale-result rejection.

The state machine does not control rendering.

Rendering is the responsibility of the UI Adapter.

---

## 3. State Model

The Presentation Module uses the following primary states:

```text
Empty
  │
  │ BuildPresentation
  ▼
Preparing
  │
  ├── success ───────────────► Ready
  │
  ├── recoverable failure ───► Empty
  │
  └── internal failure ──────► Failed

Ready
  │
  ├── content update ────────► Updating
  ├── viewport/layout change ► Reflowing
  ├── mode change ───────────► Reconfiguring
  ├── clear/replace ─────────► Clearing
  └── internal failure ──────► Failed

Updating
  │
  ├── success ───────────────► Ready
  ├── stale update ──────────► Ready
  ├── superseded ────────────► Updating
  └── internal failure ──────► Failed

Reflowing
  │
  ├── success ───────────────► Ready
  ├── stale result ──────────► Ready
  ├── superseded ────────────► Reflowing
  └── internal failure ──────► Failed

Reconfiguring
  │
  ├── success ───────────────► Ready
  ├── unsupported mode ──────► Ready
  └── internal failure ──────► Failed

Clearing
  │
  ├── success ───────────────► Empty
  └── internal failure ──────► Failed

Failed
  │
  ├── reset ─────────────────► Empty
  ├── restore last snapshot ─► Ready
  └── clear ─────────────────► Clearing
```

---

## 4. State Summary

| State           | Meaning                                                          |        Stable |
| --------------- | ---------------------------------------------------------------- | ------------: |
| `Empty`         | No active presentation exists                                    |           Yes |
| `Preparing`     | A new presentation is being built                                |            No |
| `Ready`         | A valid presentation is available                                |           Yes |
| `Updating`      | Existing presentation content is being updated                   |            No |
| `Reflowing`     | Layout is being recomputed                                       |            No |
| `Reconfiguring` | Presentation mode or presentation-wide configuration is changing |            No |
| `Clearing`      | Presentation resources are being removed                         |            No |
| `Failed`        | The module cannot guarantee a valid active state                 | Yes, degraded |

A stable state can safely wait for external input.

A transitional state represents an operation currently being processed.

---

# 5. State Definitions

## 5.1 Empty

### Meaning

No active `PresentationDocument` exists.

The module may still retain:

* configuration defaults;
* cached font metrics;
* layout algorithms;
* presentation strategy registry;
* diagnostics history.

It MUST NOT expose an active presentation.

### Entry Conditions

The module enters `Empty` when:

* the application starts;
* a presentation is cleared successfully;
* preparation is rejected before a document is created;
* the module is reset after failure.

### Allowed Inputs

* `BuildPresentation`
* `TranslationCompleted`
* `ReadingSessionChanged`
* configuration events that do not require an active presentation
* diagnostic queries

### Rejected or Ignored Inputs

* `UpdatePresentation`
* `RecomputeLayout`
* `ChangePresentationMode`
* `ClearPresentation`
* `TranslationUpdated`
* `ViewportChanged` without a pending presentation request

### Entry Actions

The module MUST:

* remove the active `PresentationDocument`;
* clear active layout state;
* clear active presentation identifiers;
* cancel obsolete presentation operations;
* retain only reusable non-content caches.

### Exit Conditions

The state exits when a valid build request begins.

### Invariants

While in `Empty`:

* `ActivePresentationId` MUST be absent;
* `PresentationDocument` MUST be absent;
* no `PresentationPrepared` event may be emitted;
* presentation-specific queries MUST return `NotReady` or an empty result;
* stale asynchronous results MUST be discarded.

---

## 5.2 Preparing

### Meaning

The module is building a new `PresentationDocument`.

Preparation may include:

* validating input contracts;
* validating revisions;
* normalizing source geometry;
* selecting a presentation strategy;
* generating presentation items;
* calculating initial layout;
* resolving overflow;
* generating diagnostics;
* assigning a new presentation revision.

### Entry Conditions

The module enters `Preparing` from `Empty` when it accepts:

* `BuildPresentation`; or
* a valid `TranslationCompleted` event that triggers automatic preparation.

### Required Context

The preparation context MUST contain:

* `RequestId`;
* `SessionId`;
* `ContentId`;
* `ContentRevision`;
* `TranslationRevision`;
* `PresentationMode`;
* `TranslationSegments`;
* `SourceRegions`;
* `Viewport`;
* presentation preferences.

### Allowed Inputs

* cancellation caused by a newer content revision;
* a newer `BuildPresentation` request;
* `ReadingSessionChanged`;
* diagnostic queries;
* `ClearPresentation`.

### Inputs Requiring Supersession

A new build request MAY supersede the current preparation when:

* it belongs to the same session and content;
* its `ContentRevision` is newer;
* its request timestamp or sequence is newer.

The older operation MUST be cancelled or allowed to finish without committing its result.

### Entry Actions

The module MUST:

1. assign an operation identifier;
2. capture the expected session and content revisions;
3. validate required input;
4. resolve the target presentation strategy;
5. begin building a candidate document.

### Success Actions

On success, the module MUST:

* validate the candidate document;
* assign `PresentationId`;
* assign the first `PresentationRevision`;
* commit the document atomically;
* publish `PresentationPrepared`;
* transition to `Ready`.

### Rejection Actions

For a deterministic contract rejection, the module MUST:

* avoid committing a partial document;
* publish `PresentationRejected`;
* return to `Empty`.

Examples:

* unsupported mode;
* missing translation segments;
* invalid viewport;
* invalid geometry;
* stale content revision.

### Failure Actions

For an unexpected internal failure, the module MUST:

* record diagnostics;
* ensure no partial document becomes active;
* publish `PresentationRejected`;
* transition to `Failed`.

### Invariants

While in `Preparing`:

* at most one candidate document may be eligible for commit;
* a candidate document MUST NOT be visible through public queries;
* stale operations MUST NOT overwrite newer operations;
* `PresentationPrepared` MUST be emitted only after atomic commit;
* the active state remains logically empty until commit succeeds.

---

## 5.3 Ready

### Meaning

A valid `PresentationDocument` exists and is available to consumers.

This is the normal operational state.

### Entry Conditions

The module enters `Ready` after:

* successful preparation;
* successful content update;
* successful layout recomputation;
* successful mode reconfiguration;
* restoring a known-good snapshot after failure.

### Allowed Inputs

* `UpdatePresentation`
* `RecomputeLayout`
* `ChangePresentationMode`
* `ClearPresentation`
* `TranslationUpdated`
* `ViewportChanged`
* `ThemeChanged`
* `PreferenceChanged`
* `ReadingSessionChanged`
* presentation queries
* diagnostic queries

### Entry Actions

The module MUST:

* expose the committed `PresentationDocument`;
* expose the active presentation identifiers;
* clear completed operation metadata;
* retain the last known-good snapshot;
* update diagnostics.

### Exit Conditions

The module leaves `Ready` when:

* translated content changes;
* viewport or layout inputs change;
* presentation mode changes;
* the active presentation is cleared or replaced;
* an unrecoverable internal invariant is violated.

### Invariants

While in `Ready`:

* exactly one active `PresentationDocument` exists;
* the document is internally consistent;
* every `PresentationItem` has stable identity;
* the document revision matches the current committed revision;
* queries MUST return a coherent snapshot;
* no partial updates may be visible;
* the active document MUST be renderable by a compatible UI Adapter.

---

## 5.4 Updating

### Meaning

The translated content or item-level presentation data of an existing presentation is being updated.

This state is used when the current presentation can be incrementally updated without rebuilding the entire document.

Examples:

* one translated segment changed;
* translation quality was improved;
* a segment was manually corrected;
* item visibility changed;
* translated text metadata changed.

### Entry Conditions

The module enters `Updating` from `Ready` after accepting:

* `UpdatePresentation`; or
* `TranslationUpdated`.

### Required Context

The update context MUST include:

* `PresentationId`;
* current `PresentationRevision`;
* expected `ContentRevision`;
* new `TranslationRevision`;
* changed segment identifiers;
* changed translated content.

### Entry Actions

The module MUST:

1. validate that the target presentation exists;
2. validate presentation and content revisions;
3. identify affected presentation items;
4. create a candidate updated document;
5. determine whether layout recomputation is required.

### Update Strategies

The module MAY perform:

#### Content-only update

Used when translated text changes without affecting layout.

Result:

* update item content;
* increment `PresentationRevision`;
* publish `PresentationUpdated`;
* return to `Ready`.

#### Content and local layout update

Used when changed text affects only a small item set.

Result:

* update affected items;
* recompute local layout;
* increment `PresentationRevision`;
* publish `PresentationUpdated`;
* optionally publish `PresentationLayoutChanged`;
* return to `Ready`.

#### Full reflow

Used when incremental layout is unsafe.

The module MAY internally perform a full layout calculation before returning to `Ready`.

It does not need to expose an intermediate `Reflowing` state unless the architecture implementation requires it.

### Supersession Rules

If another update arrives while the module is in `Updating`:

* an older update MUST NOT supersede a newer update;
* compatible updates MAY be merged;
* incompatible updates MUST be ordered by revision;
* stale results MUST be discarded;
* the newest valid translation revision wins.

### Success Actions

The module MUST:

* commit the updated document atomically;
* increment `PresentationRevision`;
* preserve stable `PresentationItem` identifiers where source identity is unchanged;
* publish `PresentationUpdated`;
* return to `Ready`.

### Stale Update Behavior

A stale update MUST:

* not modify the active document;
* not increment `PresentationRevision`;
* not publish `PresentationUpdated`;
* optionally emit diagnostics;
* return to `Ready`.

### Failure Actions

If the update fails before commit:

* preserve the previous known-good document;
* publish `PresentationRejected` when appropriate;
* return to `Ready` for recoverable failures;
* transition to `Failed` only if the active document can no longer be trusted.

### Invariants

While in `Updating`:

* the previous committed document remains readable;
* partial item updates MUST NOT become visible;
* unchanged items retain stable identity;
* `PresentationRevision` increments only after successful commit;
* stale translation revisions never overwrite newer content.

---

## 5.5 Reflowing

### Meaning

The module is recalculating presentation layout while preserving the semantic content of the active presentation.

Typical triggers include:

* viewport resize;
* zoom change;
* scroll container geometry change;
* orientation change;
* side panel width change;
* typography change;
* font metric change;
* overlay surface change;
* density or accessibility preference change.

### Entry Conditions

The module enters `Reflowing` from `Ready` after accepting:

* `RecomputeLayout`;
* `ViewportChanged`;
* a preference change that affects layout;
* a theme change that changes measurable typography.

### Required Context

The reflow context MUST include:

* `PresentationId`;
* expected `PresentationRevision`;
* new viewport;
* coordinate-space declaration;
* layout-affecting preferences;
* active presentation mode.

### Entry Actions

The module MUST:

1. validate viewport dimensions;
2. validate coordinate space;
3. capture the current presentation revision;
4. calculate a candidate layout;
5. resolve overlap and overflow;
6. validate resulting geometry.

### Coalescing Rules

Viewport events MAY arrive rapidly.

The module SHOULD:

* coalesce intermediate viewport changes;
* process the newest valid viewport;
* avoid committing obsolete layouts;
* cancel expensive obsolete calculations when possible.

Example:

```text
Viewport revision 21
Viewport revision 22
Viewport revision 23
```

The module MAY skip revisions 21 and 22 and commit only revision 23.

### Success Actions

The module MUST:

* commit the new layout atomically;
* increment the relevant layout revision;
* increment `PresentationRevision` if the public presentation document changes;
* publish `PresentationLayoutChanged`;
* return to `Ready`.

### Stale Result Behavior

If the layout result was calculated for an obsolete viewport or presentation revision, the module MUST:

* discard the result;
* not publish `PresentationLayoutChanged`;
* continue with the latest queued reflow or return to `Ready`.

### Failure Actions

For invalid external input:

* preserve the previous layout;
* publish `PresentationRejected` when required;
* return to `Ready`.

For an internal geometry failure:

* preserve the previous known-good layout when possible;
* record diagnostics;
* return to `Ready` if safe;
* otherwise transition to `Failed`.

### Invariants

While in `Reflowing`:

* translated semantic content remains unchanged;
* the previous committed layout remains available;
* coordinate spaces MUST NOT be mixed;
* geometry changes become visible only after successful commit;
* obsolete layout operations MUST NOT overwrite newer results.

---

## 5.6 Reconfiguring

### Meaning

The active presentation strategy or presentation-wide configuration is changing.

Examples:

* `SidePanel` to `SimpleOverlay`;
* `SimpleOverlay` to `TextReader`;
* `TextReader` to `Hybrid`;
* changing a layout profile;
* changing a presentation strategy implementation;
* applying a new accessibility profile that requires document restructuring.

### Entry Conditions

The module enters `Reconfiguring` from `Ready` after accepting:

* `ChangePresentationMode`;
* a preference event requiring presentation-wide reconstruction.

### Required Context

The reconfiguration context MUST include:

* `PresentationId`;
* current `PresentationRevision`;
* current mode;
* requested mode;
* current content and geometry;
* active viewport;
* applicable preferences.

### Entry Actions

The module MUST:

1. validate the requested mode;
2. verify that the mode supports the current content type;
3. preserve the current known-good document;
4. build a candidate representation for the new mode;
5. calculate the new layout;
6. validate the candidate document.

### Success Actions

The module MUST:

* commit the new mode and document atomically;
* preserve source-to-item traceability;
* preserve item identity where semantically valid;
* increment `PresentationRevision`;
* publish `PresentationModeChanged`;
* publish `PresentationUpdated` or `PresentationLayoutChanged` when relevant;
* return to `Ready`.

### Unsupported Mode Behavior

If the requested mode is unsupported:

* preserve the current presentation;
* publish `PresentationRejected`;
* return to `Ready`.

### Failure Actions

If reconfiguration fails:

* restore or retain the previous document;
* not expose partial mode changes;
* return to `Ready` when the previous document is valid;
* transition to `Failed` only if rollback cannot be guaranteed.

### Invariants

While in `Reconfiguring`:

* the current committed presentation remains available;
* the active mode changes only after commit;
* an unsupported mode never replaces a supported current mode;
* presentation mode and layout MUST always be mutually compatible.

---

## 5.7 Clearing

### Meaning

The active presentation is being removed.

Clearing may occur because:

* the user closes the presentation;
* the reading session ends;
* content changes completely;
* a new presentation replaces the old one;
* the application shuts down;
* failure recovery requires cleanup.

### Entry Conditions

The module enters `Clearing` from:

* `Ready`;
* `Preparing`;
* `Updating`;
* `Reflowing`;
* `Reconfiguring`;
* `Failed`.

### Entry Actions

The module MUST:

1. mark active operations as obsolete;
2. stop accepting mutations for the old presentation;
3. cancel or detach pending asynchronous work;
4. release presentation-specific memory;
5. clear active identifiers;
6. clear transient geometry and layout state.

### Success Actions

The module MUST:

* publish `PresentationCleared` when an active presentation previously existed;
* transition to `Empty`.

### Replacement Flow

When clearing is caused by a new content target, the module MAY retain a pending build request.

The flow becomes:

```text
Ready
  ↓
Clearing
  ↓
Empty
  ↓
Preparing
  ↓
Ready
```

A direct `Ready → Preparing` replacement MUST NOT expose two active presentations under the same presentation context unless the architecture explicitly supports parallel presentation contexts.

### Failure Actions

If cleanup partially fails:

* the module MUST prevent the old presentation from being treated as active;
* resources that cannot be released MUST be recorded in diagnostics;
* the module transitions to `Failed`.

### Invariants

While in `Clearing`:

* no new update may commit to the presentation being cleared;
* stale asynchronous work MUST be ignored;
* the presentation MUST become logically unavailable before cleanup completion;
* `PresentationCleared` MUST NOT be published more than once for the same clear operation.

---

## 5.8 Failed

### Meaning

The module encountered an internal failure and cannot guarantee that the active presentation state is valid.

`Failed` is not used for ordinary validation errors.

Examples of failures that may lead to `Failed`:

* committed document violates an invariant;
* rollback fails;
* corrupted internal geometry state;
* impossible revision ordering;
* unrecoverable layout engine error;
* resource lifecycle corruption;
* active document and active identifiers disagree.

### Entry Conditions

The module enters `Failed` when:

* an internal invariant is violated;
* a transitional operation cannot safely roll back;
* the active document can no longer be trusted;
* required internal state is corrupted.

### Entry Actions

The module MUST:

* stop committing updates;
* mark the current presentation as unavailable or degraded;
* cancel pending operations;
* capture diagnostics;
* retain the last known-good snapshot when safe;
* publish `PresentationRejected` or a system-level failure event according to the global event contract.

### Allowed Inputs

* `ResetPresentation`
* `ClearPresentation`
* `RestoreLastKnownGoodPresentation`
* diagnostic queries
* health queries
* application shutdown

### Recovery Strategy

The module SHOULD attempt recovery in this order:

1. preserve or recover a known-good committed document;
2. discard transient candidate state;
3. reset active operations;
4. restore `Ready` when the document is trustworthy;
5. otherwise clear all presentation state;
6. transition to `Empty`.

### Prohibited Behavior

While in `Failed`, the module MUST NOT:

* accept normal content updates;
* commit new layouts;
* publish `PresentationPrepared`;
* expose an unverified document as valid;
* silently return to `Ready`.

### Invariants

While in `Failed`:

* failure diagnostics exist;
* normal mutation operations are blocked;
* recovery requires an explicit verified action;
* stale asynchronous results remain invalid.

---

# 6. Transition Table

| Current State   | Trigger                    | Guard                                     | Action                                             | Next State             |
| --------------- | -------------------------- | ----------------------------------------- | -------------------------------------------------- | ---------------------- |
| `Empty`         | `BuildPresentation`        | Request valid                             | Start building candidate document                  | `Preparing`            |
| `Empty`         | `TranslationCompleted`     | Auto-build enabled and revisions valid    | Start building candidate document                  | `Preparing`            |
| `Empty`         | `ViewportChanged`          | No pending presentation                   | Ignore or record diagnostic                        | `Empty`                |
| `Preparing`     | Build succeeds             | Candidate valid and current               | Commit document and publish `PresentationPrepared` | `Ready`                |
| `Preparing`     | Validation rejected        | Deterministic input failure               | Publish `PresentationRejected`                     | `Empty`                |
| `Preparing`     | Newer build request        | New revision supersedes current           | Cancel or obsolete current operation               | `Preparing`            |
| `Preparing`     | `ClearPresentation`        | Always                                    | Cancel preparation and clean transient state       | `Clearing`             |
| `Preparing`     | Internal failure           | Rollback cannot be guaranteed             | Record failure                                     | `Failed`               |
| `Ready`         | `UpdatePresentation`       | Revisions valid                           | Start candidate content update                     | `Updating`             |
| `Ready`         | `TranslationUpdated`       | Translation revision newer                | Start candidate content update                     | `Updating`             |
| `Ready`         | `ViewportChanged`          | Viewport valid and changed                | Start layout calculation                           | `Reflowing`            |
| `Ready`         | `RecomputeLayout`          | Request valid                             | Start layout calculation                           | `Reflowing`            |
| `Ready`         | `ChangePresentationMode`   | Requested mode differs                    | Start mode reconstruction                          | `Reconfiguring`        |
| `Ready`         | `ClearPresentation`        | Always                                    | Begin cleanup                                      | `Clearing`             |
| `Ready`         | `ReadingSessionChanged`    | Content target changed                    | Begin replacement cleanup                          | `Clearing`             |
| `Updating`      | Update succeeds            | Candidate current                         | Commit and publish `PresentationUpdated`           | `Ready`                |
| `Updating`      | Update is stale            | Incoming revision not newer               | Discard candidate                                  | `Ready`                |
| `Updating`      | Newer update arrives       | Newer revision available                  | Supersede or merge operation                       | `Updating`             |
| `Updating`      | Recoverable update failure | Current document remains valid            | Preserve current document                          | `Ready`                |
| `Updating`      | Internal corruption        | Current document untrusted                | Record failure                                     | `Failed`               |
| `Reflowing`     | Layout succeeds            | Candidate matches latest viewport         | Commit and publish `PresentationLayoutChanged`     | `Ready`                |
| `Reflowing`     | Result stale               | Viewport or presentation revision changed | Discard result                                     | `Ready` or `Reflowing` |
| `Reflowing`     | Newer viewport arrives     | Newer viewport available                  | Coalesce and recompute                             | `Reflowing`            |
| `Reflowing`     | Invalid viewport           | Current layout remains valid              | Publish rejection if required                      | `Ready`                |
| `Reflowing`     | Internal geometry failure  | Current layout untrusted                  | Record failure                                     | `Failed`               |
| `Reconfiguring` | Mode change succeeds       | Candidate valid                           | Commit and publish mode change                     | `Ready`                |
| `Reconfiguring` | Mode unsupported           | Current document valid                    | Publish rejection                                  | `Ready`                |
| `Reconfiguring` | Reconstruction fails       | Rollback succeeds                         | Preserve previous mode                             | `Ready`                |
| `Reconfiguring` | Rollback fails             | Current document untrusted                | Record failure                                     | `Failed`               |
| `Clearing`      | Cleanup succeeds           | Logical state cleared                     | Publish `PresentationCleared`                      | `Empty`                |
| `Clearing`      | Cleanup fails              | Resources or state remain corrupted       | Record failure                                     | `Failed`               |
| `Failed`        | Restore snapshot           | Snapshot verified                         | Restore known-good document                        | `Ready`                |
| `Failed`        | Reset                      | Reset succeeds                            | Remove all active state                            | `Empty`                |
| `Failed`        | `ClearPresentation`        | Always                                    | Attempt cleanup                                    | `Clearing`             |

---

# 7. Transition Guards

## 7.1 Session Guard

An operation is valid only when its `SessionId` matches the active or expected presentation session.

A mismatched session MUST result in:

* rejection;
* stale-event discard; or
* presentation replacement flow.

It MUST NOT mutate the current presentation.

---

## 7.2 Content Revision Guard

An operation carrying source-dependent data MUST include `ContentRevision`.

The operation may proceed only when:

```text
IncomingContentRevision == ExpectedContentRevision
```

A lower revision is stale.

A higher revision indicates that the current presentation context is outdated and may require replacement rather than incremental update.

---

## 7.3 Translation Revision Guard

A translated-content update may proceed only when:

```text
IncomingTranslationRevision > CurrentTranslationRevision
```

Duplicate revisions MUST be idempotent.

Lower revisions MUST be ignored.

---

## 7.4 Presentation Revision Guard

Commands targeting an existing presentation SHOULD include the expected `PresentationRevision`.

The operation proceeds when:

```text
ExpectedPresentationRevision == CurrentPresentationRevision
```

A mismatch prevents lost updates.

The caller may:

* refresh the current presentation;
* retry using the latest revision;
* abandon the obsolete command.

---

## 7.5 Viewport Guard

A viewport is valid only when:

* width is greater than zero;
* height is greater than zero;
* zoom is finite and greater than zero;
* scroll values are finite;
* coordinate space is declared;
* transformation metadata is internally consistent.

---

## 7.6 Geometry Guard

Geometry is valid only when:

* coordinates are finite;
* dimensions are non-negative;
* polygons satisfy the minimum point requirement;
* declared coordinate space exists;
* required transformations are available;
* source-region identifiers are valid.

Invalid geometry MUST NOT be committed.

---

## 7.7 Presentation Mode Guard

A mode change is valid only when:

* the mode is registered;
* the mode supports the current content type;
* required geometry is available;
* required viewport capabilities are available;
* the mode is compatible with active preferences.

---

# 8. Concurrency Model

## 8.1 Logical Serialization

State transitions for one `PresentationId` MUST be logically serialized.

This does not require all computation to run on one operating-system thread.

It requires that commits occur in a deterministic order.

---

## 8.2 Parallel Computation

The implementation MAY perform in parallel:

* text measurement;
* geometry normalization;
* item generation;
* overflow analysis;
* diagnostics collection.

Parallel tasks MUST produce a single candidate result that is validated before commit.

---

## 8.3 Atomic Commit

The active presentation changes only through atomic commit.

Consumers MUST observe either:

* the previous complete document; or
* the new complete document.

Consumers MUST NOT observe partial candidate state.

---

## 8.4 Last Known-Good Snapshot

Before entering a transitional state from `Ready`, the module MUST retain access to the current committed document.

This document serves as the rollback target.

---

## 8.5 Operation Identity

Every asynchronous mutation MUST have an `OperationId`.

A result may commit only when:

* its operation is still active;
* its session is current;
* its content revision is current;
* its expected presentation revision is current;
* it has not been superseded.

---

# 9. State and Event Relationship

State transitions and event publication are related but not identical.

An event describes something that has happened.

A state describes the module’s current lifecycle condition.

## 9.1 Event Publication Rules

| Transition              | Required Event                                        |
| ----------------------- | ----------------------------------------------------- |
| `Preparing → Ready`     | `PresentationPrepared`                                |
| `Updating → Ready`      | `PresentationUpdated`                                 |
| `Reflowing → Ready`     | `PresentationLayoutChanged`                           |
| `Reconfiguring → Ready` | `PresentationModeChanged`                             |
| `Clearing → Empty`      | `PresentationCleared`                                 |
| Validation rejection    | `PresentationRejected` when externally relevant       |
| Internal failure        | `PresentationRejected` and/or system diagnostic event |

## 9.2 Publication Timing

Events MUST be published after successful state commit.

Incorrect:

```text
Publish PresentationPrepared
Commit document
Enter Ready
```

Correct:

```text
Commit document
Enter Ready
Publish PresentationPrepared
```

The implementation MAY use an outbox or equivalent mechanism to prevent state-event inconsistency.

---

# 10. Command Acceptance by State

| Command                  |        Empty |              Preparing |                       Ready |                  Updating |                 Reflowing |                 Reconfiguring |             Clearing |                  Failed |
| ------------------------ | -----------: | ---------------------: | --------------------------: | ------------------------: | ------------------------: | ----------------------------: | -------------------: | ----------------------: |
| `BuildPresentation`      |       Accept |     Supersede or queue | Replace through clear/build |        Queue or supersede |        Queue or supersede |                         Queue |                Queue |   Reject or reset first |
| `UpdatePresentation`     |       Reject |        Reject or queue |                      Accept |        Merge or supersede |                     Queue |                         Queue |               Reject |                  Reject |
| `RecomputeLayout`        |       Reject |  Queue latest viewport |                      Accept |              Queue latest |                  Coalesce |                         Queue |               Reject |                  Reject |
| `ChangePresentationMode` |       Reject |                  Queue |                      Accept |                     Queue |                     Queue | Supersede or reject duplicate |               Reject |                  Reject |
| `ClearPresentation`      |        No-op |                 Accept |                      Accept |                    Accept |                    Accept |                        Accept |           Idempotent |                  Accept |
| `GetPresentation`        | Empty result | Previous result absent |              Return current | Return previous committed | Return previous committed |     Return previous committed | Empty or unavailable | Degraded or unavailable |
| `GetDiagnostics`         |       Accept |                 Accept |                      Accept |                    Accept |                    Accept |                        Accept |               Accept |                  Accept |

---

# 11. Event Acceptance by State

| Event                   |                  Empty |             Preparing |                     Ready |           Updating |     Reflowing | Reconfiguring |              Clearing |                      Failed |
| ----------------------- | ---------------------: | --------------------: | ------------------------: | -----------------: | ------------: | ------------: | --------------------: | --------------------------: |
| `TranslationCompleted`  |                  Build |    Supersede if newer |    Replace if new content | Queue or supersede |         Queue |         Queue |                 Queue | Ignore or recovery workflow |
| `TranslationUpdated`    |                 Ignore |  Queue if same target |                    Update | Merge or supersede |         Queue |         Queue |                Ignore |                      Ignore |
| `ViewportChanged`       | Cache latest if useful |          Cache latest |                    Reflow |       Cache latest |      Coalesce |  Cache latest |                Ignore |                      Ignore |
| `ThemeChanged`          |        Update defaults | Update pending config |          Update or reflow |              Queue |      Coalesce |         Merge |       Update defaults |        Update defaults only |
| `PreferenceChanged`     |        Update defaults | Update pending config | Update/reflow/reconfigure |              Queue |      Coalesce |         Merge |       Update defaults |        Update defaults only |
| `ReadingSessionChanged` | Update expected target |    Supersede or clear |                   Replace |      Clear/replace | Clear/replace | Clear/replace | Update pending target |              Reset or clear |

---

# 12. Idempotency Rules

## 12.1 Clear Idempotency

Calling `ClearPresentation` multiple times MUST produce the same final state:

```text
Empty
```

Only one `PresentationCleared` event may be emitted for one active presentation lifecycle.

---

## 12.2 Duplicate Update Events

Duplicate `TranslationUpdated` events with the same:

* `EventId`;
* `TranslationRevision`;
* changed content;

MUST NOT increment `PresentationRevision` more than once.

---

## 12.3 Duplicate Viewport Events

Receiving the same viewport repeatedly MUST NOT create different layout results.

Layout calculation MUST be deterministic for equivalent inputs.

---

## 12.4 Duplicate Mode Change

Changing to the already active mode MUST be treated as:

* a no-op; or
* a deterministic reconfiguration only when configuration also changed.

It MUST NOT produce an unnecessary presentation revision.

---

# 13. Timeout Rules

State transitions that perform asynchronous work SHOULD define implementation-specific timeouts.

Suggested initial targets:

| Operation                  |                                           Target | Timeout behavior                           |
| -------------------------- | -----------------------------------------------: | ------------------------------------------ |
| Initial preparation        |                  under 100 ms for normal content | Cancel or reject obsolete operation        |
| Incremental content update |                                      under 50 ms | Preserve current document                  |
| Viewport reflow            | under 16 ms target, may exceed for complex pages | Coalesce and skip obsolete frames          |
| Mode reconfiguration       |                                     under 100 ms | Preserve previous mode                     |
| Clearing                   |                                      under 50 ms | Mark logical clear before resource cleanup |

These values are performance targets, not hard architectural guarantees.

A timeout MUST NOT cause partial state commit.

---

# 14. Recovery Rules

## 14.1 Recoverable Validation Failure

Examples:

* invalid viewport;
* unsupported mode;
* stale revision;
* missing optional style profile.

Behavior:

* preserve the last known-good document;
* publish a deterministic rejection when required;
* return to the previous stable state.

---

## 14.2 Recoverable Processing Failure

Examples:

* one layout strategy fails but a safe fallback exists;
* local text measurement fails;
* an optional font is unavailable.

Behavior MAY include:

* fallback strategy;
* default font metrics;
* side-panel fallback;
* hiding only the affected item;
* diagnostic annotation.

The resulting document MUST still satisfy all invariants.

---

## 14.3 Unrecoverable Internal Failure

Examples:

* inconsistent committed revision;
* invalid active identifier mapping;
* rollback failure;
* corrupted presentation graph.

Behavior:

* transition to `Failed`;
* stop normal mutations;
* retain diagnostics;
* require reset, clear, or verified restoration.

---

# 15. Fallback Behavior

Presentation SHOULD prefer graceful degradation over complete failure.

Recommended fallback order:

```text
Hybrid
  ↓
Simple Overlay
  ↓
Side Panel
  ↓
Text Reader
  ↓
No Presentation
```

The actual order depends on content type and user preference.

For comic image content, a safe fallback is usually:

```text
Simple Overlay
  ↓
Side Panel
```

For novel text content, a safe fallback is usually:

```text
Formatted Reader
  ↓
Plain Text Reader
```

Fallback MUST be observable through diagnostics.

A fallback that changes the requested presentation mode SHOULD publish an appropriate mode or update event.

---

# 16. State Invariants

The following invariants apply to the entire state machine.

## 16.1 Single Active Presentation

Only one active presentation may exist per presentation context.

---

## 16.2 Stable Identity

A `PresentationItem` representing the same semantic source item SHOULD preserve its `ItemId` across:

* content updates;
* layout changes;
* viewport changes.

A mode change MAY replace item identity when the representation is structurally different, but source traceability MUST remain available.

---

## 16.3 Monotonic Revision

`PresentationRevision` MUST increase monotonically after each successful committed presentation mutation.

It MUST NOT increase for:

* rejected commands;
* stale events;
* no-op operations;
* failed candidate calculations.

---

## 16.4 Immutable Committed Document

A committed `PresentationDocument` is immutable.

Updates create a new document or a logically immutable revision.

---

## 16.5 Deterministic Layout

Equivalent inputs MUST produce equivalent layout output, excluding explicitly nondeterministic diagnostic metadata such as timestamps.

---

## 16.6 No Stale Commit

An asynchronous operation MUST NOT commit if any required revision changed after the operation started.

---

## 16.7 Previous State Availability

During `Updating`, `Reflowing`, and `Reconfiguring`, the previous committed document remains the public readable snapshot.

---

## 16.8 No Rendering Ownership

No state transition may directly manipulate:

* browser DOM;
* native windows;
* UI widgets;
* canvas rendering state;
* input focus;
* mouse or keyboard listeners.

The state machine produces presentation data only.

---

# 17. State Query Contract

The module SHOULD expose a lightweight state query.

Example conceptual result:

```text
PresentationStateSnapshot
- state
- presentationId
- sessionId
- contentId
- contentRevision
- translationRevision
- presentationRevision
- activeOperationId
- activeOperationType
- enteredAt
- lastStableState
- degraded
- lastErrorCode
```

This snapshot is intended for:

* diagnostics;
* developer tools;
* health checks;
* tests;
* UI loading indicators.

It MUST NOT expose mutable internal objects.

---

# 18. Loading and UI Interpretation

Presentation state does not directly control UI, but a UI Adapter may interpret states as follows:

| State           | Suggested UI behavior                                                      |
| --------------- | -------------------------------------------------------------------------- |
| `Empty`         | Hide presentation surface                                                  |
| `Preparing`     | Show non-blocking loading indicator                                        |
| `Ready`         | Render current document                                                    |
| `Updating`      | Continue showing previous document, optionally show subtle updating status |
| `Reflowing`     | Continue showing previous layout until new layout commits                  |
| `Reconfiguring` | Continue showing previous mode until new mode commits                      |
| `Clearing`      | Remove presentation surface                                                |
| `Failed`        | Show safe error state or fallback presentation                             |

The UI Adapter MUST NOT infer business state only from animation or component lifecycle.

It SHOULD rely on explicit state or presentation events.

---

# 19. Example Flows

## 19.1 Initial Comic Presentation

```text
State: Empty

TranslationCompleted
  │
  ▼
Validate content and geometry
  │
  ▼
State: Preparing
  │
  ├── Select SidePanel strategy
  ├── Build numbered source markers
  ├── Build translated items
  ├── Calculate panel layout
  └── Commit PresentationDocument
  │
  ▼
State: Ready
  │
  ▼
Publish PresentationPrepared
```

---

## 19.2 Viewport Resize

```text
State: Ready

ViewportChanged
  │
  ▼
Validate viewport
  │
  ▼
State: Reflowing
  │
  ├── Keep old layout readable
  ├── Recalculate item positions
  ├── Resolve overflow
  └── Commit new layout
  │
  ▼
State: Ready
  │
  ▼
Publish PresentationLayoutChanged
```

---

## 19.3 Translation Correction

```text
State: Ready

TranslationUpdated
  │
  ▼
Validate translation revision
  │
  ▼
State: Updating
  │
  ├── Locate affected item
  ├── Replace translated content
  ├── Measure changed text
  ├── Reflow affected layout
  └── Commit new document
  │
  ▼
State: Ready
  │
  ▼
Publish PresentationUpdated
```

---

## 19.4 Rapid Viewport Changes

```text
State: Ready

ViewportChanged revision 10
  │
  ▼
State: Reflowing

ViewportChanged revision 11
ViewportChanged revision 12
ViewportChanged revision 13
  │
  ▼
Discard or cancel calculations for 10–12
  │
  ▼
Calculate revision 13
  │
  ▼
Commit latest layout
  │
  ▼
State: Ready
```

---

## 19.5 Change Presentation Mode

```text
State: Ready
Mode: SidePanel

ChangePresentationMode(SimpleOverlay)
  │
  ▼
State: Reconfiguring
  │
  ├── Validate overlay support
  ├── Transform source geometry
  ├── Build overlay items
  ├── Resolve overlap
  └── Commit new mode
  │
  ▼
State: Ready
Mode: SimpleOverlay
  │
  ▼
Publish PresentationModeChanged
```

---

## 19.6 Unsupported Mode

```text
State: Ready
Mode: SidePanel

ChangePresentationMode(AdvancedImageRewrite)
  │
  ▼
State: Reconfiguring
  │
  ▼
Mode not supported in v1
  │
  ▼
Preserve SidePanel document
  │
  ▼
State: Ready
  │
  ▼
Publish PresentationRejected
```

---

## 19.7 Reading Content Changed

```text
State: Ready
ContentId: Chapter-10

ReadingSessionChanged
ContentId: Chapter-11
  │
  ▼
State: Clearing
  │
  ▼
Publish PresentationCleared
  │
  ▼
State: Empty
  │
  ▼
TranslationCompleted for Chapter-11
  │
  ▼
State: Preparing
  │
  ▼
State: Ready
```

---

## 19.8 Internal Failure and Recovery

```text
State: Reflowing

Committed layout invariant violated
  │
  ▼
Discard candidate
  │
  ├── previous document valid
  │       ▼
  │     State: Ready
  │
  └── previous document untrusted
          ▼
        State: Failed
          │
          ▼
        ResetPresentation
          │
          ▼
        State: Empty
```

---

# 20. Testing Requirements

## 20.1 State Transition Tests

Tests MUST verify every valid transition.

Examples:

* `Empty → Preparing`;
* `Preparing → Ready`;
* `Ready → Updating → Ready`;
* `Ready → Reflowing → Ready`;
* `Ready → Reconfiguring → Ready`;
* `Ready → Clearing → Empty`;
* transitional state to `Failed`;
* `Failed → Empty`;
* `Failed → Ready` through verified restoration.

---

## 20.2 Invalid Transition Tests

Tests MUST verify invalid commands do not mutate state.

Examples:

* update while `Empty`;
* reflow while `Empty`;
* normal update while `Failed`;
* mode change while `Clearing`;
* stale update after presentation replacement.

---

## 20.3 Revision Tests

Tests MUST verify:

* stale `ContentRevision` is rejected;
* duplicate `TranslationRevision` is idempotent;
* newer updates supersede older updates;
* stale asynchronous layout cannot commit;
* `PresentationRevision` increments only after successful commit.

---

## 20.4 Concurrency Tests

Tests SHOULD cover:

* update arriving during reflow;
* viewport changes arriving during update;
* clear arriving during preparation;
* session change during mode reconfiguration;
* multiple rapid viewport events;
* delayed obsolete operation completion.

---

## 20.5 Rollback Tests

Tests MUST verify the previous committed document remains valid when:

* update fails;
* reflow fails;
* mode reconfiguration is rejected;
* candidate validation fails.

---

## 20.6 Event Tests

Tests MUST verify:

* success events publish only after commit;
* rejection events contain the correct identifiers;
* duplicate events do not create duplicate mutations;
* `PresentationCleared` publishes once;
* stale operations do not emit successful update events.

---

## 20.7 Determinism Tests

Equivalent inputs MUST produce:

* equivalent presentation items;
* equivalent reading order;
* equivalent layout;
* equivalent overflow decisions;
* equivalent revision behavior.

---

# 21. Observability

Every transition SHOULD generate structured diagnostics containing:

* previous state;
* next state;
* trigger;
* `OperationId`;
* `PresentationId`;
* `SessionId`;
* `ContentId`;
* content revision;
* translation revision;
* presentation revision;
* operation duration;
* result;
* rejection or failure code.

Translated text SHOULD NOT be included in normal logs.

Geometry details SHOULD be summarized rather than fully logged unless diagnostic mode is explicitly enabled.

---

# 22. Metrics

Recommended module metrics:

```text
presentation_state_transition_total
presentation_prepare_duration_ms
presentation_update_duration_ms
presentation_reflow_duration_ms
presentation_reconfigure_duration_ms
presentation_clear_duration_ms
presentation_rejected_total
presentation_failed_total
presentation_stale_operation_total
presentation_rollback_total
presentation_fallback_total
presentation_active_count
```

Metrics SHOULD support labels such as:

* presentation mode;
* content type;
* result;
* rejection category;
* previous state;
* next state.

Metrics MUST avoid user content.

---

# 23. Persistence

The state machine itself does not require persistent storage for MVP.

The module MAY persist through the Storage Module:

* presentation preferences;
* last selected mode;
* reusable layout profiles;
* optional presentation snapshots;
* diagnostics metadata.

The Presentation Module MUST NOT directly access persistent storage.

A restored presentation snapshot MUST be validated before entering `Ready`.

---

# 24. Multiple Presentation Contexts

Version 1 assumes one active presentation context per reading surface.

Future implementations MAY support multiple concurrent presentation contexts.

Examples:

* multiple browser tabs;
* multiple desktop capture regions;
* dual-page comic view;
* side-by-side source and translation.

Each context MUST have an independent state machine identified by a stable context identifier.

Global state MUST NOT replace per-context lifecycle state.

---

# 25. Future States

The following states are reserved for future consideration and are not part of version 1:

## Suspended

Presentation remains available but processing is temporarily paused.

Possible use:

* application background mode;
* hidden browser tab;
* resource-saving mode.

## Restoring

A persisted presentation snapshot is being loaded and validated.

## Exporting

A presentation is being converted into an export format.

This SHOULD probably remain a separate Export Module operation rather than a core Presentation state.

## Degraded

A valid but partially reduced presentation exists.

Version 1 represents degradation through diagnostics and fallback mode while remaining in `Ready`.

A dedicated `Degraded` state should be added only if consumers require different lifecycle behavior.

---

# 26. Architectural Decisions

## 26.1 `Ready` Remains Readable During Mutation

During `Updating`, `Reflowing`, and `Reconfiguring`, the previously committed presentation remains available.

Reason:

* avoids visual flicker;
* prevents empty UI between revisions;
* enables atomic replacement;
* supports rollback.

---

## 26.2 Reflow Is Separate from Content Update

`Updating` changes semantic or presentation content.

`Reflowing` changes geometry and layout only.

Reason:

* clearer diagnostics;
* better performance measurement;
* easier testing;
* avoids unnecessary content reconstruction.

---

## 26.3 Mode Change Has a Dedicated State

`Reconfiguring` is separate because presentation mode changes may rebuild:

* item structure;
* geometry;
* reading order;
* overflow policy;
* accessibility representation.

---

## 26.4 Validation Errors Do Not Imply Failure

Invalid commands and stale revisions are normal operational outcomes.

They produce rejection or no-op behavior.

They do not transition the module to `Failed`.

---

## 26.5 `Failed` Is Reserved for Broken Invariants

`Failed` indicates that internal correctness cannot be guaranteed.

It is not a general-purpose error state.

---

# 27. Architecture Invariants

The Presentation state machine MUST always preserve the following rules:

1. A committed presentation is immutable.
2. Only a valid committed document may be exposed as active.
3. Candidate state is never visible to external consumers.
4. Successful events are emitted only after commit.
5. Stale operations never overwrite newer revisions.
6. The previous committed document remains available during safe mutations.
7. Presentation item identity remains stable where semantic identity is unchanged.
8. Coordinate spaces are explicitly declared.
9. Layout calculations are deterministic for equivalent inputs.
10. `PresentationRevision` increases only after successful mutation.
11. Clearing invalidates all outstanding operations for the cleared presentation.
12. Ordinary validation errors do not place the module in `Failed`.
13. Rendering technology remains outside the Presentation state machine.
14. User content is not written to diagnostics by default.
15. Recovery never exposes an unverified snapshot as `Ready`.

---

# 28. Completion Criteria

This state specification is considered implemented when:

* every state has an explicit runtime representation;
* every transition is guarded;
* operations use revision checks;
* commits are atomic;
* stale asynchronous results are rejected;
* the previous document is retained during safe mutations;
* published events match committed transitions;
* invalid transitions are deterministic;
* failure recovery is implemented;
* transition tests cover valid and invalid paths;
* diagnostics expose state and operation identity;
* UI Adapters do not own or duplicate Presentation lifecycle logic.
