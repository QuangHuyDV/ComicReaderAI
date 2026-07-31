# Presentation Errors

- **Module:** Presentation
- **Version:** 2.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

## Related Documents

- `modules/presentation/MODULE.md`
- `modules/presentation/CONTRACT.md`
- `modules/presentation/STATES.md`
- `modules/presentation/EVENTS.md`
- `docs/architecture/STATE_MACHINE.md`
- `docs/architecture/EVENT_BUS.md`

---

# 1. Purpose

This document defines the error model of the Presentation Module.

It standardizes:

- error ownership;
- error taxonomy;
- stable error codes;
- public error contracts;
- severity classification;
- retry semantics;
- recovery behavior;
- state transition behavior;
- event publication behavior;
- diagnostics and observability.

The purpose is to ensure that every Presentation failure is:

- deterministic;
- observable;
- recoverable whenever possible;
- implementation-independent;
- compatible with command-driven operations;
- compatible with asynchronous event-driven processing.

This document defines architectural behavior rather than implementation-specific exceptions.

Consumers MUST rely on stable Presentation error contracts instead of language-specific exception types.

---

# 2. Scope

This specification covers failures produced while the Presentation Module performs responsibilities defined in `MODULE.md`.

These include:

- validating presentation requests;
- validating consumed events;
- validating presentation configuration;
- creating PresentationSnapshots;
- updating PresentationSnapshots;
- rebuilding RenderPlans;
- applying PresentationProfiles;
- changing PresentationModes;
- recalculating layouts;
- transforming coordinate spaces;
- processing geometry;
- resolving layout conflicts;
- managing Presentation state;
- publishing Presentation events.

This document also defines failures related to:

- revision validation;
- concurrency;
- obsolete operations;
- fallback strategies;
- state transitions;
- recovery;
- diagnostics.

---

## Outside Scope

The Presentation Module does NOT own failures originating from:

- Reading Session;
- Content Acquisition;
- OCR;
- Translation;
- Browser Integration;
- Desktop Integration;
- Storage;
- UI Rendering;
- network providers.

Presentation MAY translate upstream failures into Presentation-specific errors only when Presentation processing itself cannot continue safely.

---

# 3. Error Philosophy

Errors are part of the public architecture contract.

Every externally observable failure MUST map to:

- a stable ErrorCode;
- an ErrorCategory;
- a Severity;
- a RetryPolicy.

Consumers MUST make decisions using these stable fields.

Consumers MUST NOT depend on:

- exception class names;
- stack traces;
- implementation language;
- internal messages.

Messages are intended only for diagnostics.

---

## 3.1 Errors are Architectural Contracts

Every Presentation error represents a semantic failure.

An error code MUST preserve its meaning across implementations.

Changing the semantic meaning of an existing error code requires a major contract version.

---

## 3.2 Expected Outcomes are NOT Internal Failures

Many unsuccessful operations are expected outcomes.

Examples include:

- duplicate events;
- stale revisions;
- obsolete asynchronous results;
- unsupported presentation modes;
- invalid viewport changes;
- missing optional preferences.

These outcomes do NOT indicate module corruption.

They normally preserve the current PresentationSnapshot.

---

## 3.3 Failed State is Reserved

The `Failed` state is reserved for situations where Presentation can no longer guarantee architectural correctness.

Typical causes include:

- broken invariants;
- corrupted active snapshot;
- impossible revision ordering;
- rollback failure;
- inconsistent internal identity;
- atomic commit failure.

Expected validation failures MUST NOT transition Presentation into `Failed`.

---

## 3.4 Preserve Previous Snapshot

Whenever possible Presentation preserves the previous committed PresentationSnapshot.

Candidate operations MUST NOT replace the active PresentationSnapshot until validation and commit both succeed.

Recoverable failures therefore never expose partially updated presentation data.

---

## 3.5 Observable Failures

Every externally visible failure SHOULD be observable through:

- structured logs;
- diagnostics;
- metrics;
- stable error codes.

Presentation SHOULD never silently discard failures except when explicitly defined by architecture (for example obsolete asynchronous operations).

---

## 3.6 Privacy

Presentation errors MUST NOT expose user content.

Error payloads MUST NOT contain:

- translated text;
- original text;
- screenshots;
- page images;
- browser content.

Instead they SHOULD reference:

- identifiers;
- revisions;
- geometry summaries;
- counts;
- language identifiers.

---

# 4. Design Principles

Presentation follows the principles below.

---

## 4.1 Stable Error Codes

Error codes are permanent public contracts.

Consumers MUST branch using ErrorCode rather than textual messages.

---

## 4.2 Immutable Error Facts

Errors describe completed facts.

Once created an error record MUST NOT be modified.

Additional diagnostics MAY be appended separately.

---

## 4.3 Deterministic Behavior

Given identical:

- command;
- event;
- PresentationState;
- revisions;

Presentation MUST produce the same error.

---

## 4.4 Explicit Recovery

Every error MUST define recovery semantics.

Recovery may include:

- ignore;
- retry;
- fallback;
- rebuild;
- restore previous snapshot;
- transition to Failed.

---

## 4.5 Explicit Retry Policy

Every error MUST declare one retry policy.

Retry behavior MUST NOT depend on implementation.

---

## 4.6 Candidate Isolation

Candidate operations MUST never corrupt committed PresentationSnapshots.

Failed candidates MUST be discarded.

---

## 4.7 Module Independence

Presentation errors describe Presentation behavior only.

They MUST NOT expose internal failures from:

- OCR
- Translation
- Browser
- Rendering

except through Presentation-owned contracts.

---

# 5. Error Ownership

Presentation owns only failures inside the Presentation boundary.

Ownership follows module responsibilities.

| Failure Source | Owned By |
|---------------|----------|
| Reading Session validation | Reading Session |
| OCR recognition | OCR |
| Translation generation | Translation |
| Browser capture | Browser Integration |
| UI rendering | UI Adapter |
| PresentationSnapshot construction | Presentation |
| RenderPlan generation | Presentation |
| Layout computation | Presentation |
| Geometry normalization | Presentation |
| PresentationProfile application | Presentation |
| PresentationMode selection | Presentation |
| State transition | Presentation |
| Event publication | Presentation |

Presentation MAY wrap upstream failures when Presentation cannot safely continue.

The original upstream failure MUST remain available through diagnostics whenever possible.

---

## 5.1 Revision Ownership

Presentation recognizes revisions owned by multiple modules.

Presentation MUST NOT modify revisions owned by another module.

| Revision | Owner |
|----------|-------|
| ContentRevision | Reading Session |
| TranslationRevision | Translation |
| PreferenceRevision | Preferences |
| ProfileRevision | Preferences |
| ViewportRevision | UI Adapter |
| PresentationRevision | Presentation |

Only Presentation may create or increment PresentationRevision.

---

## 5.2 Identity Ownership

Presentation recognizes the following identities.

| Identity | Owner |
|----------|-------|
| SessionId | Reading Session |
| ContentId | Reading Session |
| PresentationContextId | Presentation |
| PresentationId | Presentation |
| SnapshotId | Presentation |
| RenderPlanId | Presentation |

Presentation MUST reject inconsistent ownership relationships.

---

# 6. Error Categories

Every Presentation error belongs to exactly one category.

| Prefix | Category |
|---------|----------|
| VAL | Validation |
| CTX | Context |
| REV | Revision |
| GEO | Geometry |
| LAY | Layout |
| MODE | Presentation Mode |
| STATE | State Machine |
| EVENT | Event Processing |
| RES | Resources |
| REC | Recovery |
| INT | Internal |

Categories are architectural classifications only.

Severity and RetryPolicy are defined independently.

---

# 7. Error Code Format

Presentation uses stable error identifiers.

```
PRS-<CATEGORY>-<NUMBER>
```

Examples

```
PRS-VAL-001
PRS-REV-004
PRS-LAY-002
PRS-MODE-003
```

Rules:

- category names never change;
- existing meanings never change;
- numbers are never reused;
- deprecated codes remain documented.

---

# 8. Severity Model

Every Presentation error declares one severity.

| Severity | Meaning |
|-----------|---------|
| Info | Expected no-op or obsolete operation |
| Warning | Request rejected while current Presentation remains valid |
| Error | Operation failed and caller action or fallback is required |
| Critical | Presentation correctness can no longer be guaranteed |

Severity does not determine retry behavior.

---

## Severity Guidelines

### Info

Examples:

- stale event;
- duplicate event;
- obsolete layout result;
- obsolete translation.

Usually ignored after diagnostics.

---

### Warning

Examples:

- invalid command;
- unsupported mode;
- invalid viewport;
- missing required input.

Previous PresentationSnapshot remains active.

---

### Error

Examples:

- layout computation failed;
- geometry transformation failed;
- RenderPlan generation failed;
- event publication failed.

Fallback or recovery is required.

---

### Critical

Examples:

- invariant violation;
- atomic commit failure;
- rollback failure;
- corrupted PresentationSnapshot.

Presentation transitions to `Failed`.

---

# 9. Retry Policy

Each error defines exactly one retry policy.

| Policy | Meaning |
|---------|---------|
| Never | Retrying identical input cannot succeed |
| AfterCorrection | Retry only after correcting input |
| WithLatestRevision | Retry using current revisions |
| WithFallback | Retry using another PresentationStrategy |
| Transient | Retry may succeed without semantic changes |
| ResetRequired | Presentation must be reset before retry |

Retry behavior MUST be deterministic.

Retries MUST NOT reuse obsolete revisions.

---

# 10. Public Error Contract

Presentation exposes a conceptual public error object.

```text
PresentationError

- ErrorId
- ErrorCode
- Category
- Severity
- RetryPolicy

- Message

- Operation
- State

- SessionId
- ContentId

- PresentationContextId
- PresentationId

- SnapshotId
- RenderPlanId

- ContentRevision
- TranslationRevision
- PreferenceRevision
- ProfileRevision
- ViewportRevision
- PresentationRevision

- RequestId
- OperationId
- EventId

- CorrelationId
- CausationId
- TraceId

- Details

- OccurredAt
```

The exact serialization format is implementation dependent.

Only semantic fields are standardized.

---

## Required Fields

Every externally observable Presentation error MUST contain:

- ErrorId
- ErrorCode
- Category
- Severity
- RetryPolicy
- Operation
- State
- OccurredAt
- CorrelationId

---

## Optional Fields

Identifiers SHOULD be included whenever known.

Typical examples:

- SessionId
- ContentId
- PresentationId
- SnapshotId
- RenderPlanId
- EventId
- RequestId

---

## Revision Fields

Revision fields SHOULD be included whenever relevant.

Examples:

- ContentRevision
- TranslationRevision
- ViewportRevision
- PresentationRevision

Presentation MUST NOT invent missing revision values.

---

## Message Rules

The Message field:

- is intended for diagnostics only;
- MUST be concise;
- MUST NOT contain user content;
- MUST NOT contain secrets;
- MUST NOT be used as a programmatic contract.

Consumers MUST branch using ErrorCode rather than Message.

---

# 11. Validation Errors

Validation errors indicate that an incoming command, query, event, or configuration does not satisfy the Presentation contract.

Validation failures are expected operational outcomes.

They MUST NOT transition Presentation into `Failed`.

Whenever possible, the current committed `PresentationSnapshot` remains active.

---

## PRS-VAL-001 — MissingRequiredField

### Meaning

A required field is missing from the request.

### Examples

- missing SessionId
- missing ContentId
- missing PresentationMode
- missing PresentationTarget
- missing PresentationProfile
- missing Viewport

### Severity

Warning

### Retry Policy

AfterCorrection

### State Behavior

- active PresentationSnapshot remains unchanged
- candidate operation discarded

### Event

Publish `PresentationRejected` if the request was accepted for processing.

---

## PRS-VAL-002 — InvalidFieldValue

### Meaning

A field exists but contains an invalid value.

### Examples

- invalid zoom
- empty identifier
- negative size
- unsupported enum value
- invalid PresentationTarget

### Severity

Warning

### Retry Policy

AfterCorrection

### Recovery

Preserve current PresentationSnapshot.

---

## PRS-VAL-003 — InvalidPresentationProfile

### Meaning

The supplied PresentationProfile is malformed.

### Examples

- invalid typography
- unsupported spacing
- inconsistent profile configuration

### Severity

Warning

### Retry Policy

AfterCorrection

### Recovery

Continue using the current PresentationProfile whenever possible.

---

## PRS-VAL-004 — MissingPresentationInput

### Meaning

Required presentation input is unavailable.

Presentation cannot construct a PresentationSnapshot.

### Examples

- missing translated content
- missing source markers
- missing reading content

### Severity

Warning

### Retry Policy

AfterCorrection

### Ownership

Presentation does not generate missing input.

The caller must provide complete data.

---

## PRS-VAL-005 — UnsupportedContractVersion

### Meaning

The supplied Presentation contract version is unsupported.

### Severity

Error

### Retry Policy

AfterCorrection

### Recovery

Caller must use a compatible contract version.

---

# 12. Context Errors

Context errors indicate that incoming data belongs to a different Presentation context.

They do not indicate module corruption.

---

## PRS-CTX-001 — PresentationContextNotFound

### Meaning

The requested PresentationContextId does not exist.

### Severity

Warning

### Retry Policy

AfterCorrection

### Recovery

Create a new Presentation context or use a valid identifier.

---

## PRS-CTX-002 — SessionMismatch

### Meaning

SessionId differs from the active Presentation context.

### Severity

Info or Warning

### Retry Policy

WithLatestRevision

### Behavior

Ignore stale operations.

---

## PRS-CTX-003 — ContentMismatch

### Meaning

ContentId differs from the active Presentation content.

### Severity

Info or Warning

### Retry Policy

AfterCorrection

### Recovery

Begin a replacement Presentation flow.

---

## PRS-CTX-004 — SnapshotNotFound

### Meaning

Referenced PresentationSnapshot cannot be found.

### Severity

Warning

### Retry Policy

WithLatestRevision

---

## PRS-CTX-005 — RenderPlanNotFound

### Meaning

Referenced RenderPlan does not exist.

### Severity

Warning

### Retry Policy

WithLatestRevision

---

## PRS-CTX-006 — IdentityConflict

### Meaning

Multiple Presentation identities conflict.

### Examples

- Snapshot belongs to another context
- RenderPlan belongs to another Presentation
- PresentationId mismatch

### Severity

Critical

### Retry Policy

ResetRequired

### State Behavior

Transition to Failed if committed state is affected.

---

# 13. Revision Errors

Revision errors protect deterministic updates.

Presentation only owns PresentationRevision.

Other revisions belong to their respective modules.

---

## PRS-REV-001 — StaleContentRevision

### Meaning

Incoming ContentRevision is older than the current context.

### Severity

Info

### Retry Policy

WithLatestRevision

### Behavior

Ignore operation.

---

## PRS-REV-002 — FutureContentRevision

### Meaning

Incoming ContentRevision cannot be processed incrementally.

### Severity

Warning

### Retry Policy

WithLatestRevision

### Recovery

Perform Presentation rebuild.

---

## PRS-REV-003 — StaleTranslationRevision

### Meaning

Incoming TranslationRevision is obsolete.

### Severity

Info

### Retry Policy

Never

### Behavior

Ignore duplicate translation.

---

## PRS-REV-004 — PresentationRevisionConflict

### Meaning

Caller expects a different PresentationRevision.

### Severity

Warning

### Retry Policy

WithLatestRevision

### Typical Cause

Concurrent updates.

---

## PRS-REV-005 — ObsoleteOperation

### Meaning

A candidate operation completed after being superseded.

### Examples

- newer viewport
- newer translation
- newer PresentationProfile

### Severity

Info

### Retry Policy

Never

### Behavior

Discard candidate.

---

## PRS-REV-006 — NonMonotonicRevision

### Meaning

Presentation attempted to commit a revision lower than or equal to the active PresentationRevision.

### Severity

Critical

### Retry Policy

ResetRequired

### State Behavior

Transition to Failed.

---

## PRS-REV-007 — RevisionContextMismatch

### Meaning

Revision values belong to different Presentation contexts.

### Severity

Error

### Retry Policy

WithLatestRevision

### Recovery

Discard candidate.

Reload current context.

---

# 14. Geometry Errors

Geometry errors occur while preparing source-aligned presentation.

These errors are owned by Presentation.

---

## PRS-GEO-001 — InvalidBoundingBox

### Meaning

Bounding box contains invalid values.

### Examples

- negative size
- NaN
- infinite coordinate

### Severity

Warning

### Retry Policy

AfterCorrection

---

## PRS-GEO-002 — InvalidPolygon

### Meaning

Polygon cannot represent a valid source region.

### Severity

Warning

### Retry Policy

AfterCorrection

---

## PRS-GEO-003 — MissingCoordinateSpace

### Meaning

Geometry has no declared coordinate space.

### Severity

Error

### Retry Policy

AfterCorrection

---

## PRS-GEO-004 — UnsupportedCoordinateSpace

### Meaning

Presentation cannot interpret the supplied coordinate system.

### Severity

Error

### Retry Policy

AfterCorrection

---

## PRS-GEO-005 — CoordinateTransformationUnavailable

### Meaning

Required coordinate transformation is unavailable.

### Examples

- Image → Overlay
- Overlay → Viewport

### Severity

Error

### Retry Policy

WithFallback

### Recovery

Use a PresentationStrategy that does not require source-aligned geometry.

---

## PRS-GEO-006 — CoordinateTransformationFailed

### Meaning

Known coordinate transformation produced invalid output.

### Severity

Error

### Retry Policy

WithFallback

### Recovery

Keep previous RenderPlan.

---

## PRS-GEO-007 — GeometryOutsideViewport

### Meaning

Presentation item is outside the visible viewport.

### Severity

Info

### Retry Policy

Never

### Behavior

Hide item.

Do not remove it from PresentationSnapshot.

---

## PRS-GEO-008 — GeometryConflict

### Meaning

Geometry relationships contradict each other.

### Examples

- duplicate regions
- impossible hierarchy
- inconsistent coordinates

### Severity

Error

### Retry Policy

AfterCorrection

---

## PRS-GEO-009 — GeometryInvariantViolation

### Meaning

Committed geometry violates Presentation invariants.

### Severity

Critical

### Retry Policy

ResetRequired

### State Behavior

Transition to Failed.

---

# 15. Layout Errors

Layout errors occur while generating RenderPlans.

Presentation SHOULD preserve the previous RenderPlan whenever possible.

---

## PRS-LAY-001 — InvalidViewport

### Meaning

Viewport cannot be used for layout.

### Examples

- zero size
- invalid zoom
- invalid transform

### Severity

Warning

### Retry Policy

AfterCorrection

---

## PRS-LAY-002 — TextMeasurementFailed

### Meaning

Presentation cannot accurately measure translated text.

### Severity

Error

### Retry Policy

WithFallback

### Recovery

Use approximate metrics.

---

## PRS-LAY-003 — FontProfileUnavailable

### Meaning

Requested font profile is unavailable.

### Severity

Warning

### Retry Policy

WithFallback

### Recovery

Use default typography profile.

---

## PRS-LAY-004 — LayoutOverflowUnresolved

### Meaning

Overflow cannot be resolved.

### Severity

Error

### Retry Policy

WithFallback

### Recovery

Possible fallback:

- reduce text size
- SidePanel
- Reader
- Marker mode

---

## PRS-LAY-005 — LayoutOverlapUnresolved

### Meaning

PresentationItems overlap beyond policy limits.

### Severity

Error

### Retry Policy

WithFallback

### Recovery

Use another PresentationStrategy.

---

## PRS-LAY-006 — InvalidReadingOrder

### Meaning

Presentation cannot establish deterministic reading order.

### Severity

Error

### Retry Policy

AfterCorrection

---

## PRS-LAY-007 — LayoutComputationTimeout

### Meaning

Layout computation exceeded its execution budget.

### Severity

Warning or Error

### Retry Policy

Transient

### Recovery

Cancel obsolete computation.

---

## PRS-LAY-008 — LayoutResultObsolete

### Meaning

Computed RenderPlan belongs to an obsolete PresentationRevision.

### Severity

Info

### Retry Policy

Never

### Behavior

Discard RenderPlan.

---

## PRS-LAY-009 — LayoutInvariantViolation

### Meaning

Committed RenderPlan violates architectural invariants.

### Severity

Critical

### Retry Policy

ResetRequired

---

# 16. Presentation Mode Errors

PresentationMode errors occur while selecting or changing PresentationStrategy.

---

## PRS-MODE-001 — UnsupportedPresentationMode

### Meaning

Requested PresentationMode is not supported.

### Severity

Warning

### Retry Policy

WithFallback

---

## PRS-MODE-002 — ModeNotCompatibleWithContent

### Meaning

PresentationMode does not support the supplied content.

### Severity

Warning

### Retry Policy

WithFallback

---

## PRS-MODE-003 — ModeRequirementsMissing

### Meaning

Required input for the selected PresentationMode is unavailable.

### Examples

- missing geometry
- missing markers
- missing translated segments

### Severity

Error

### Retry Policy

WithFallback

---

## PRS-MODE-004 — ModeTransitionRejected

### Meaning

Current Presentation state does not allow the requested mode change.

### Severity

Warning

### Retry Policy

AfterCorrection

---

## PRS-MODE-005 — ModeReconfigurationFailed

### Meaning

Presentation failed to produce a valid PresentationSnapshot for the new mode.

### Severity

Error

### Retry Policy

WithFallback

### Recovery

Preserve the previous PresentationMode.

---

## PRS-MODE-006 — ModeRollbackFailed

### Meaning

Presentation could not restore the previous PresentationMode after failure.

### Severity

Critical

### Retry Policy

ResetRequired

### State Behavior

Transition to Failed.

---

# 17. State Machine Errors

State Machine errors occur when an operation violates the Presentation lifecycle defined in `STATES.md`.

These errors protect the integrity of the Presentation state machine.

---

## PRS-STATE-001 — InvalidStateTransition

### Meaning

The requested operation is not allowed from the current Presentation state.

### Examples

- UpdatePresentation while `Empty`
- ChangePresentationMode while `Preparing`
- BuildPresentation while `Clearing`

### Severity

Warning

### Retry Policy

AfterCorrection

### Recovery

Reject the operation.

The current PresentationState remains unchanged.

---

## PRS-STATE-002 — PresentationNotReady

### Meaning

The requested operation requires a ready PresentationSnapshot.

No active Presentation exists.

### Severity

Info or Warning

### Retry Policy

AfterCorrection

### Recovery

Caller should build a Presentation before retrying.

---

## PRS-STATE-003 — OperationAlreadyInProgress

### Meaning

A conflicting operation is already executing.

### Examples

- another BuildPresentation
- another RecomputePresentationLayout
- another ApplyPresentationProfile

### Severity

Info

### Retry Policy

Transient

### Recovery

Implementation MAY:

- queue
- coalesce
- reject
- supersede

according to architecture policy.

---

## PRS-STATE-004 — OperationCancelled

### Meaning

The operation was cancelled intentionally.

### Typical Causes

- newer request
- Presentation cleared
- Presentation replaced
- application shutdown

### Severity

Info

### Retry Policy

Never

### Behavior

Candidate operation is discarded.

Committed PresentationSnapshot remains unchanged.

---

## PRS-STATE-005 — ActiveSnapshotMissing

### Meaning

Presentation state indicates that an active Presentation exists but the active PresentationSnapshot is unavailable.

### Severity

Critical

### Retry Policy

ResetRequired

### State Behavior

Transition to `Failed`.

---

## PRS-STATE-006 — ActiveContextConflict

### Meaning

Internal Presentation context is inconsistent.

### Examples

- Snapshot belongs to another PresentationContext
- RenderPlan references another Snapshot
- PresentationRevision mismatch
- Active PresentationId mismatch

### Severity

Critical

### Retry Policy

ResetRequired

### State Behavior

Transition to `Failed`.

---

## PRS-STATE-007 — ClearOperationFailed

### Meaning

Presentation could not fully clear internal state.

### Severity

Error or Critical

### Retry Policy

Transient or ResetRequired

### Recovery

Old Presentation becomes logically unavailable immediately.

Physical cleanup MAY continue asynchronously.

---

# 18. Event Errors

Event errors occur while consuming or publishing Presentation events.

These errors describe event processing only.

They do not describe transport failures.

---

## PRS-EVENT-001 — UnsupportedEventType

### Meaning

Presentation received an event that is not part of its public contract.

### Severity

Warning

### Retry Policy

Never

---

## PRS-EVENT-002 — InvalidEventPayload

### Meaning

The received event does not satisfy its schema.

### Examples

- missing identifiers
- invalid revisions
- malformed payload

### Severity

Warning

### Retry Policy

AfterCorrection

### Recovery

Ignore the event.

---

## PRS-EVENT-003 — DuplicateEvent

### Meaning

An EventId has already been processed.

### Severity

Info

### Retry Policy

Never

### Behavior

Ignore idempotently.

---

## PRS-EVENT-004 — EventOutOfOrder

### Meaning

The event ordering violates Presentation revision rules.

### Examples

- TranslationUpdated arrives after PresentationCleared
- old ViewportChanged
- obsolete ProfileChanged

### Severity

Info or Warning

### Retry Policy

WithLatestRevision

---

## PRS-EVENT-005 — EventContextMismatch

### Meaning

The event belongs to another PresentationContext.

### Severity

Info

### Retry Policy

Never

### Recovery

Ignore stale event.

---

## PRS-EVENT-006 — EventPublicationFailed

### Meaning

Presentation committed state successfully but failed to publish the corresponding Presentation event.

### Severity

Error

### Retry Policy

Transient

### Recovery

Implementation SHOULD retry publication using an Outbox or equivalent mechanism.

Committed state MUST NOT be repeated.

---

## PRS-EVENT-007 — EventSerializationFailed

### Meaning

Presentation event cannot be serialized.

### Severity

Error

### Retry Policy

AfterCorrection

### Recovery

Do not publish invalid events.

---

# 19. Resource Errors

Resource errors occur when Presentation exceeds implementation limits.

These limits are implementation-dependent.

---

## PRS-RES-001 — PresentationTooLarge

### Meaning

Presentation exceeds configured limits.

### Examples

- too many PresentationItems
- excessive geometry
- excessive translated content

### Severity

Error

### Retry Policy

WithFallback

### Recovery

Possible actions:

- pagination
- lazy loading
- partial Presentation
- visible region only

---

## PRS-RES-002 — MemoryBudgetExceeded

### Meaning

Presentation cannot continue within memory limits.

### Severity

Error

### Retry Policy

Transient or WithFallback

### Recovery

Discard obsolete PresentationSnapshots.

Reduce cached geometry.

---

## PRS-RES-003 — ComputationBudgetExceeded

### Meaning

Presentation exceeded its CPU or processing budget.

### Severity

Warning or Error

### Retry Policy

WithFallback

### Recovery

Use a simpler PresentationStrategy.

---

## PRS-RES-004 — TooManyPendingOperations

### Meaning

Too many Presentation operations are queued.

### Severity

Warning

### Retry Policy

Transient

### Recovery

Coalesce compatible operations.

Discard obsolete operations.

---

## PRS-RES-005 — StrategyUnavailable

### Meaning

Requested PresentationStrategy cannot be created.

### Severity

Error

### Retry Policy

WithFallback

### Recovery

Select another compatible strategy.

---

# 20. Recovery Errors

Recovery errors occur while restoring Presentation after failure.

---

## PRS-REC-001 — CandidateRollbackFailed

### Meaning

Presentation failed to restore the previous committed PresentationSnapshot.

### Severity

Critical

### Retry Policy

ResetRequired

### State Behavior

Transition to `Failed`.

---

## PRS-REC-002 — SnapshotUnavailable

### Meaning

No recoverable PresentationSnapshot exists.

### Severity

Error

### Retry Policy

ResetRequired

### Recovery

Clear Presentation.

Return to `Empty`.

---

## PRS-REC-003 — SnapshotInvalid

### Meaning

Stored PresentationSnapshot failed validation.

### Severity

Error

### Retry Policy

ResetRequired

### Recovery

Snapshot MUST NOT become active.

---

## PRS-REC-004 — SnapshotVersionUnsupported

### Meaning

Snapshot version is incompatible with the current Presentation contract.

### Severity

Error

### Retry Policy

AfterCorrection

### Recovery

Rebuild Presentation.

---

## PRS-REC-005 — RecoveryAttemptFailed

### Meaning

Presentation recovery could not produce a valid PresentationSnapshot.

### Severity

Critical

### Retry Policy

ResetRequired

### State Behavior

Transition to `Failed`.

---

# 21. Internal Errors

Internal errors indicate architectural correctness can no longer be guaranteed.

These errors should be extremely rare.

---

## PRS-INT-001 — UnexpectedInternalFailure

### Meaning

Presentation encountered an unexpected implementation failure.

No more specific Presentation error applies.

### Severity

Error or Critical

### Retry Policy

Transient or ResetRequired

---

## PRS-INT-002 — InvariantViolation

### Meaning

A core Presentation architecture invariant has been violated.

### Examples

- mutable committed PresentationSnapshot
- invalid identity ownership
- stale candidate committed
- invalid revision ordering

### Severity

Critical

### Retry Policy

ResetRequired

### State Behavior

Transition to `Failed`.

---

## PRS-INT-003 — AtomicCommitFailed

### Meaning

Presentation could not atomically commit:

- PresentationSnapshot
- RenderPlan
- PresentationRevision

### Severity

Critical

### Retry Policy

ResetRequired

### Recovery

Committed state MUST remain unchanged.

---

## PRS-INT-004 — StrategyContractViolation

### Meaning

PresentationStrategy returned output violating the public Presentation contract.

### Severity

Error or Critical

### Retry Policy

WithFallback

### Recovery

Disable the faulty strategy for the current operation.

Use another compatible strategy.

---

## PRS-INT-005 — SnapshotSerializationViolation

### Meaning

PresentationSnapshot cannot be represented using the public Presentation contract.

### Severity

Critical

### Retry Policy

ResetRequired

---

# 22. Error → State Mapping

| Error Category | Typical State Result |
|----------------|----------------------|
| Validation | Preserve current state |
| Context | Ignore or reject |
| Revision | Ignore obsolete request |
| Geometry | Reject candidate |
| Layout | Preserve previous RenderPlan |
| Mode | Preserve previous PresentationMode |
| State | Reject operation |
| Event | Ignore or retry publication |
| Resource | Apply fallback |
| Recovery | Transition to Failed if unrecoverable |
| Internal | Transition to Failed |

---

## State Mapping Principles

Recoverable errors MUST preserve the previous committed PresentationSnapshot.

Candidate failures MUST NOT mutate committed state.

Only Critical correctness failures transition Presentation into `Failed`.

---

# 23. Error → Event Mapping

Presentation SHOULD publish events describing externally visible failures.

---

## PresentationRejected

Published when:

- command validation fails
- unsupported PresentationMode
- invalid PresentationProfile
- invalid PresentationTarget
- invalid revisions
- invalid state transition

PresentationRejected describes failures visible to callers.

---

## PresentationFailed

Published only when architectural correctness cannot be guaranteed.

Typical causes:

- invariant violation
- atomic commit failure
- rollback failure
- active context corruption
- committed snapshot corruption

PresentationFailed indicates Presentation entered `Failed`.

---

## No Event Required

The following errors normally do NOT publish events:

- DuplicateEvent
- ObsoleteOperation
- LayoutResultObsolete
- GeometryOutsideViewport
- stale revisions
- repeated ClearPresentation

These conditions SHOULD be recorded only through diagnostics.

---

# 24. Fallback Policy

Presentation SHOULD preserve the user's reading experience whenever a recoverable failure occurs.

Fallback MUST be:

- deterministic;
- observable;
- reversible;
- compatible with the current PresentationContext.

Fallback MUST NEVER silently violate architectural correctness.

---

## 24.1 Fallback Principles

Presentation MAY apply fallback only when:

- correctness is preserved;
- user intent remains recognizable;
- PresentationSnapshot remains internally consistent.

Fallback MUST NOT:

- fabricate content;
- modify translated text;
- modify source content;
- silently ignore corrupted Presentation state.

---

## 24.2 Strategy Fallback

When the requested PresentationStrategy cannot be used, Presentation SHOULD attempt another compatible strategy.

Typical order:

```text
Hybrid
    ↓
SimpleOverlay
    ↓
MarkerOverlay
    ↓
SidePanel
    ↓
Reader
    ↓
NoPresentation
```

The selected fallback depends on the current PresentationTarget and PresentationMode.

---

## 24.3 Layout Fallback

When layout computation cannot produce a valid RenderPlan, Presentation MAY attempt:

1. reduce typography complexity;
2. simplify spacing;
3. switch to marker layout;
4. move translated content into SidePanel;
5. switch to Reader mode.

Every fallback MUST generate a new RenderPlan.

---

## 24.4 Geometry Fallback

If source geometry cannot safely be used:

```text
Precise Overlay
        ↓
Marker Overlay
        ↓
SidePanel
        ↓
Reader
```

Presentation MUST NOT fabricate geometry.

---

## 24.5 Typography Fallback

When the requested typography profile is unavailable:

```text
Requested Profile
        ↓
Compatible Profile
        ↓
System Default
        ↓
Approximate Metrics
```

Approximate metrics SHOULD only be used as a temporary fallback.

---

## 24.6 Snapshot Preservation

Whenever fallback succeeds:

- previous PresentationSnapshot remains valid until commit;
- candidate Snapshot is validated;
- atomic replacement occurs.

Fallback MUST NOT partially mutate the active Snapshot.

---

## 24.7 Recording Fallback

Whenever fallback is applied, diagnostics SHOULD record:

- original strategy;
- fallback strategy;
- triggering ErrorCode;
- affected Snapshot;
- affected RenderPlan;
- whether user-visible behavior changed.

---

# 25. Diagnostics

Diagnostics help developers understand Presentation behavior without exposing user content.

Diagnostics MUST NOT be treated as public APIs.

---

## 25.1 Diagnostic Principles

Diagnostics SHOULD be:

- structured;
- deterministic;
- privacy-safe;
- machine-readable.

---

## 25.2 Diagnostic Context

Diagnostics MAY include:

- PresentationContextId
- PresentationId
- SnapshotId
- RenderPlanId

- SessionId
- ContentId

- PresentationRevision
- ContentRevision
- TranslationRevision
- ViewportRevision

- OperationId
- EventId

- CorrelationId
- CausationId
- TraceId

---

## 25.3 Privacy

Diagnostics MUST NOT contain:

- translated text;
- original text;
- screenshots;
- browser HTML;
- image data;
- user credentials;
- provider secrets.

---

# 26. Logging

Presentation SHOULD produce structured logs.

Logging format is implementation-dependent.

Only semantic content is standardized.

---

## 26.1 Logging Levels

Presentation uses four logical levels.

| Level | Typical Usage |
|--------|---------------|
| Debug | Development diagnostics |
| Info | Successful operations |
| Warning | Recoverable failures |
| Error | Failed operations |
| Critical | Broken architectural invariants |

---

## 26.2 Structured Fields

Recommended log fields:

```text
timestamp

operation

state

errorCode

severity

retryPolicy

presentationContextId

presentationId

snapshotId

renderPlanId

sessionId

contentId

presentationRevision

contentRevision

translationRevision

viewportRevision

operationId

eventId

correlationId

causationId

traceId

durationMs

fallbackApplied
```

---

## 26.3 Logging Rules

Presentation SHOULD log:

- rejected operations;
- fallback execution;
- state transitions;
- recovery attempts;
- invariant violations.

Presentation SHOULD NOT log:

- every duplicate event;
- every obsolete operation;
- every stale revision.

---

# 27. Metrics

Presentation SHOULD expose implementation-independent metrics.

Metrics support operational monitoring only.

---

## Recommended Metrics

```text
presentation_operation_total

presentation_snapshot_total

presentation_renderplan_total

presentation_error_total

presentation_rejection_total

presentation_failure_total

presentation_fallback_total

presentation_recovery_total

presentation_layout_total

presentation_geometry_total

presentation_event_total

presentation_strategy_total
```

---

## Suggested Labels

Metrics MAY include:

- operation
- state
- presentationMode
- presentationTarget
- strategy
- errorCode
- severity
- retryPolicy
- fallbackApplied

Metrics SHOULD NOT include:

- PresentationId
- SnapshotId
- SessionId
- ContentId

High-cardinality identifiers reduce metric usefulness.

---

# 28. Testing Requirements

Presentation implementations MUST verify the complete error model.

---

## 28.1 Error Mapping

Tests MUST verify:

- every failure maps to exactly one ErrorCode;
- ErrorCodes remain stable;
- categories are correct.

---

## 28.2 Recovery

Tests MUST verify:

- previous PresentationSnapshot is preserved;
- failed candidates never commit;
- fallback executes correctly.

---

## 28.3 State Machine

Tests MUST verify:

- invalid transitions are rejected;
- recoverable failures preserve state;
- Critical failures enter Failed.

---

## 28.4 Event Processing

Tests MUST verify:

- duplicate events are idempotent;
- obsolete events are ignored;
- publication failures never duplicate commits.

---

## 28.5 Revision Handling

Tests MUST verify:

- stale revisions are ignored;
- newer revisions supersede older work;
- revision ownership is respected.

---

## 28.6 Privacy

Tests MUST verify that logs and diagnostics never expose user content.

---

## 28.7 Fallback

Tests MUST verify:

- strategy fallback;
- layout fallback;
- geometry fallback;
- typography fallback.

---

# 29. Compatibility Rules

Presentation error contracts are versioned.

Compatibility rules apply to every implementation.

---

## 29.1 Minor Version

A minor version MAY:

- add new ErrorCodes;
- add optional fields;
- add diagnostics;
- add new categories.

Existing behavior MUST remain compatible.

---

## 29.2 Major Version

A major version is required when:

- removing ErrorCodes;
- changing semantic meaning;
- changing required fields;
- changing retry behavior incompatibly;
- changing severity incompatibly.

---

## 29.3 Deprecated Errors

Deprecated ErrorCodes SHOULD remain documented for at least one compatibility cycle.

Consumers SHOULD migrate to replacement codes.

---

# 30. Architecture Invariants

Every Presentation implementation MUST preserve these invariants.

1. ErrorCodes are stable contracts.

2. Consumers branch using ErrorCode.

3. PresentationSnapshot is immutable after commit.

4. RenderPlan belongs to exactly one Snapshot.

5. PresentationRevision is monotonic.

6. Candidate operations never mutate committed state.

7. Failed candidates are discarded.

8. Duplicate events are idempotent.

9. Obsolete operations never commit.

10. Fallback never violates correctness.

11. Recovery never exposes invalid Snapshots.

12. Critical failures transition Presentation to Failed.

13. PresentationRejected never indicates corruption.

14. PresentationFailed always indicates loss of architectural correctness.

15. User content never appears in public error payloads.

16. Diagnostics remain implementation-independent.

17. Logging remains privacy-safe.

18. Retry behavior is explicitly defined.

19. Revision ownership is respected.

20. Presentation remains independent from UI frameworks.

---

# 31. Error Code Summary

Every Presentation error code defined in this specification belongs to one of the following categories.

| Category | Prefix |
|----------|--------|
| Validation | PRS-VAL |
| Context | PRS-CTX |
| Revision | PRS-REV |
| Geometry | PRS-GEO |
| Layout | PRS-LAY |
| Presentation Mode | PRS-MODE |
| State Machine | PRS-STATE |
| Event | PRS-EVENT |
| Resource | PRS-RES |
| Recovery | PRS-REC |
| Internal | PRS-INT |

Each ErrorCode MUST:

- belong to exactly one category;
- define exactly one Severity;
- define exactly one RetryPolicy;
- remain stable after release.

---

# 32. Completion Criteria

This specification is considered complete when:

- every Presentation failure maps to a stable ErrorCode;
- every ErrorCode defines Severity;
- every ErrorCode defines RetryPolicy;
- ownership boundaries are respected;
- revision ownership is explicitly defined;
- recoverable failures preserve committed PresentationSnapshots;
- RenderPlans are never partially committed;
- fallback behavior is deterministic;
- state transitions follow `STATES.md`;
- published failure events follow `EVENTS.md`;
- command failures follow `CONTRACT.md`;
- diagnostics are privacy-safe;
- logging is structured;
- metrics avoid high-cardinality identifiers;
- testing covers every ErrorCode category;
- Presentation remains independent of implementation language and UI framework.

---

**End of Presentation Errors**