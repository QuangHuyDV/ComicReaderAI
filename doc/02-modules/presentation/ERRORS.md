# Presentation Errors

> **Project:** CRAI
> **Module:** `presentation`
> **Path:** `doc/02-modules/presentation/ERRORS.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-08

---

# 1. Purpose

This document defines the Presentation-owned error model.

It standardizes:

* error ownership;
* stable error codes;
* error categories;
* severity;
* recovery semantics;
* retry hints;
* candidate rejection;
* PresentationRevision conflicts;
* geometry/layout failures;
* state-machine failures;
* resource failures;
* recovery failures;
* internal invariant failures;
* diagnostics and observability.

The goal is to ensure that Presentation failures remain:

```text
Deterministic
Observable
Recoverable where possible
Serializable
Implementation-independent
Privacy-safe
Runtime-v2 compatible
```

Presentation errors describe failures inside the Presentation boundary only.

---

# 2. Error Ownership

Presentation owns errors created while performing Presentation-owned responsibilities such as:

```text
Presentation input validation
Artifact compatibility validation
Presentation item mapping
Presentation geometry validation
Layout planning
Mode resolution
Profile application
Candidate Presentation creation
PresentationRevision validation
Presentation commit
Presentation state transitions
Presentation-local recovery
```

Presentation does not own failures from:

```text
Runtime Control
Scheduler
Work Queue
Recognition
Text Processing
Translation
Artifact Store
Reading Session
Preferences persistence
UI Adapter rendering
Storage
Platform integration
```

External failures may be referenced or normalized for diagnostics, but ownership must remain explicit.

---

# 3. The Four Failure Domains

CRAI distinguishes four fundamentally different conditions.

## 3.1 Presentation-Owned Error

A Presentation responsibility failed.

Example:

```text
invalid Presentation mapping
invalid geometry
layout cannot satisfy invariants
PresentationRevision conflict
committed state corruption
```

Presentation owns the error.

---

## 3.2 Runtime Authority Rejection

Runtime decides that a Presentation Candidate may no longer commit.

Examples:

```text
Runtime Revision superseded
Work canceled
Session no longer active
Attempt no longer authoritative
```

This is not a Presentation error.

Presentation receives a normalized authority result and discards its Candidate.

---

## 3.3 Expected Supersession / Cancellation Outcome

Presentation work becomes obsolete.

Examples:

```text
newer viewport
newer Presentation command
clear operation
newer PresentationRevision committed first
coalesced reflow
```

These are expected control outcomes.

They normally require:

```text
discard
+
diagnostics
```

not a public error.

---

## 3.4 UI Apply Failure

Presentation committed successfully but UI Adapter could not apply it.

Examples:

```text
target destroyed
surface unavailable
stale UI revision
platform rendering failure
```

This belongs to UI Adapter.

Presentation must not reclassify it as a Presentation internal failure unless it independently damages Presentation-owned state.

---

# 4. Error Philosophy

## 4.1 Stable Machine Contract

Every public Presentation error contains a stable machine-readable code.

Consumers MUST NOT branch on:

* exception classes;
* stack traces;
* implementation language;
* human-readable messages.

---

## 4.2 Candidate Failure Does Not Equal Current-State Failure

A Candidate may fail while current committed Presentation remains perfectly valid.

Default model:

```text
Current Presentation
        +
Candidate
        ↓
Candidate fails
        ↓
Discard Candidate
        ↓
Current Presentation unchanged
```

---

## 4.3 `FAILED` Is Reserved

Presentation enters `FAILED` only when Presentation-owned correctness cannot be trusted.

Examples:

* current Snapshot/RenderPlan mismatch;
* impossible PresentationRevision ordering;
* corrupted Presentation registry;
* failed atomic commit leaving uncertain state;
* recovery data untrustworthy.

Validation errors do not enter `FAILED`.

---

## 4.4 Expected Outcomes Are Not Errors

Normal control behavior includes:

```text
superseded Candidate
coalesced layout operation
duplicate request
no-op request
Runtime authority rejection
PresentationRevision conflict
unsupported mode with valid fallback
```

Some may produce a rejection result, but they are not internal failures.

---

# 5. Error Categories

Presentation v2 uses:

| Prefix   | Category                       |
| -------- | ------------------------------ |
| `VAL`    | Presentation Input Validation  |
| `CTX`    | Presentation Context           |
| `ART`    | Artifact Compatibility         |
| `PRSREV` | Presentation Revision          |
| `GEO`    | Geometry                       |
| `LAY`    | Layout                         |
| `MODE`   | Presentation Mode              |
| `STATE`  | Presentation State             |
| `RES`    | Presentation Resources         |
| `REC`    | Presentation Recovery          |
| `PUB`    | Presentation Event Publication |
| `INT`    | Internal Invariant             |

Notably absent:

```text
Runtime
Translation
Reading Session
UI rendering
```

because those are external owners.

---

# 6. Error Code Format

```text
PRS-<CATEGORY>-<NUMBER>
```

Examples:

```text
PRS-VAL-001
PRS-ART-003
PRS-PRSREV-001
PRS-GEO-005
PRS-LAY-004
PRS-INT-002
```

Rules:

* codes are stable;
* meanings are never reused;
* deprecated codes remain documented;
* semantic changes require version review.

---

# 7. Severity

| Severity   | Meaning                                                                  |
| ---------- | ------------------------------------------------------------------------ |
| `Info`     | Expected non-success/control outcome                                     |
| `Warning`  | Request rejected while current Presentation remains valid                |
| `Error`    | Presentation operation cannot complete and recovery/fallback is required |
| `Critical` | Presentation-owned correctness cannot be guaranteed                      |

Severity does not decide Runtime retry.

---

# 8. Recovery Hint

Presentation error contracts expose a recovery hint rather than commanding Runtime retry.

```text
RecoveryHint
- None
- CorrectInput
- RefreshPresentationRevision
- ChangeTarget
- ChangeViewport
- UseFallback
- RebuildPresentation
- ClearPresentation
- RestoreKnownGood
- ResetPresentation
```

Presentation does not schedule retry itself.

---

# 9. Retry Hint vs Runtime Retry

Presentation may provide:

```text
RetryHint
```

but Runtime/Application owns retry execution.

Examples:

```text
DoNotRetry
RetryWithLatestPresentationRevision
RetryAfterInputChange
RetryAfterTargetChange
RetryWithFallback
```

Presentation errors MUST NOT directly create Runtime retry.

---

# 10. Public Error Contract

Conceptually:

```text
PresentationError
├── errorId
├── errorCode
├── category
├── severity
├── recoveryHint
├── retryHint?
├── messageKey?
├── operationType?
├── presentationState?
├── presentationContextId?
├── presentationId?
├── expectedPresentationRevision?
├── currentPresentationRevision?
├── targetId?
├── targetRevision?
├── viewportRevision?
├── runtimeIdentity?
├── requestId?
├── operationId?
├── correlationId?
├── causationId?
├── traceId?
├── diagnosticRef?
└── occurredAt
```

---

# 11. Runtime Identity in Errors

Presentation errors may include:

```text
sessionId
runtimeRevisionId
workItemId
attemptId
```

for diagnostics.

Presentation does not own or alter those identities.

They MUST NOT be used as Presentation error ordering mechanisms.

---

# 12. Message Rules

Human-readable message text:

* is optional;
* is diagnostic;
* must not contain user content;
* must not contain secrets;
* is never the machine contract.

Consumers branch on:

```text
errorCode
category
recoveryHint
```

---

# 13. Validation Errors

Validation errors mean a Presentation command does not satisfy Presentation contracts.

They never enter `FAILED`.

---

## PRS-VAL-001 — MissingRequiredField

A required Presentation field is absent.

Examples:

* missing `PresentationContextId`;
* missing `PresentationTarget`;
* missing `PresentationProfile`;
* missing required Artifact reference;
* missing required viewport.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

Committed Presentation remains unchanged.

---

## PRS-VAL-002 — InvalidFieldValue

A supplied field has invalid semantics.

Examples:

* invalid enum;
* invalid scale;
* negative size;
* malformed identifier;
* invalid typography value.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## PRS-VAL-003 — InvalidPresentationProfile

PresentationProfile cannot be safely applied.

Examples:

* invalid typography constraints;
* impossible fallback configuration;
* inconsistent overlay policy.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
or
UseFallback
```

---

## PRS-VAL-004 — MissingPresentationInput

Required semantic input is unavailable.

Examples:

* required TranslationArtifactRef missing;
* required SourceDocumentArtifactRef missing;
* mode requires geometry but no compatible Recognition Artifact exists.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

Presentation does not generate missing upstream data itself.

---

## PRS-VAL-005 — UnsupportedContractVersion

Presentation command contract version is incompatible.

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
```

---

# 14. Presentation Context Errors

---

## PRS-CTX-001 — PresentationContextNotFound

Requested Presentation Context does not exist.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## PRS-CTX-002 — PresentationNotFound

Requested PresentationId does not exist in the context.

Severity:

```text
Warning
```

Recovery:

```text
RefreshPresentationRevision
or
RebuildPresentation
```

---

## PRS-CTX-003 — PresentationItemNotFound

Referenced PresentationItemId does not exist.

Severity:

```text
Warning
```

Recovery:

```text
RefreshPresentationRevision
```

---

## PRS-CTX-004 — PresentationIdentityConflict

Public Presentation identities disagree.

Examples:

* Snapshot belongs to another Presentation;
* RenderPlan references another PresentationId;
* marker references unknown item;
* PresentationContext mismatch.

Severity:

```text
Error
```

If only Candidate affected:

```text
discard Candidate
```

If committed state affected:

```text
Critical
→ FAILED
```

---

# 15. Artifact Compatibility Errors

Presentation consumes accepted immutable Artifact references.

These errors concern compatibility, not publication authority.

---

## PRS-ART-001 — UnsupportedArtifactType

Presentation received an Artifact type it cannot consume.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## PRS-ART-002 — ArtifactReferenceInvalid

Artifact reference is structurally invalid or cannot be resolved through the permitted runtime boundary.

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
or
RebuildPresentation
```

---

## PRS-ART-003 — ArtifactCompatibilityMismatch

Supplied Artifacts do not represent compatible semantic content.

Examples:

* Translation Artifact maps to another SourceDocument;
* geometry belongs to unrelated source content;
* required source identifiers do not match.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## PRS-ART-004 — RequiredArtifactDataUnavailable

Artifact exists but lacks information required by the selected mode.

Example:

```text
Overlay requires geometry
but accepted Artifacts provide no usable source geometry
```

Severity:

```text
Warning
```

Recovery:

```text
UseFallback
```

---

## PRS-ART-005 — ArtifactLeaseUnavailable

Presentation cannot acquire required Artifact access according to Runtime resource policy.

Ownership:

```text
resource/Artifact infrastructure
```

Presentation records normalized failure but does not claim Artifact ownership.

Severity:

```text
Error
```

Recovery:

```text
RebuildPresentation
or external retry decision
```

---

# 16. Presentation Revision Errors

Presentation owns only `PresentationRevision`.

---

## PRS-PRSREV-001 — PresentationRevisionConflict

Caller expected:

```text
PresentationRevision = N
```

but current is:

```text
N + K
```

Severity:

```text
Info or Warning
```

Recovery:

```text
RefreshPresentationRevision
```

This is expected optimistic concurrency behavior.

---

## PRS-PRSREV-002 — CandidateSuperseded

Candidate became obsolete because newer Presentation work won.

Examples:

* newer reflow;
* newer update;
* newer mode operation;
* clear.

Severity:

```text
Info
```

Recovery:

```text
None
```

Normally diagnostics only.

---

## PRS-PRSREV-003 — CandidateViewportObsolete

Candidate layout uses an obsolete viewport revision.

Severity:

```text
Info
```

Recovery:

```text
None
```

Newer useful work should already exist or be requested externally.

---

## PRS-PRSREV-004 — CandidateTargetObsolete

Target revision changed before commit.

Severity:

```text
Info or Warning
```

Recovery:

```text
ChangeTarget
```

---

## PRS-PRSREV-005 — NonMonotonicPresentationRevision

Presentation attempts to commit a revision not greater than current revision when mutation requires advancement.

Severity:

```text
Critical
```

Recovery:

```text
ResetPresentation
```

State:

```text
FAILED
```

---

## PRS-PRSREV-006 — SnapshotRenderPlanRevisionMismatch

Candidate or committed Snapshot and RenderPlan do not share the same PresentationRevision.

Candidate-only:

```text
Error
discard
```

Committed:

```text
Critical
FAILED
```

---

# 17. Runtime Authority Outcomes

Runtime authority rejection is **not a PRS error category**.

Presentation receives:

```text
AuthorityRevalidationResult
```

Possible normalized results:

```text
RejectedStale
RejectedCanceled
RejectedSessionInactive
RejectedRuntimeRevision
RejectedOther
```

Presentation behavior:

```text
discard Candidate
do not commit
do not increment PresentationRevision
do not enter FAILED
```

Optional external rejection fact:

```text
PresentationRejected
rejectionSource = RuntimeAuthority
```

---

# 18. Cancellation Outcomes

Presentation does not define:

```text
PRS-STATE-OperationCancelled
```

as a module failure.

Cancellation is represented as:

```text
Runtime cancellation observation
or
Presentation-local supersession
```

Candidate is discarded.

No new PresentationRevision.

No PresentationFailed.

---

# 19. Geometry Errors

---

## PRS-GEO-001 — InvalidBoundingBox

Geometry contains invalid numeric bounds.

Examples:

* negative dimensions;
* NaN;
* infinity.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
or
UseFallback
```

---

## PRS-GEO-002 — InvalidPolygon

Polygon cannot represent valid geometry.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
or
UseFallback
```

---

## PRS-GEO-003 — MissingCoordinateSpace

Public geometry has no explicit coordinate space.

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
```

---

## PRS-GEO-004 — UnsupportedCoordinateSpace

Presentation cannot interpret required coordinate semantics.

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
or
UseFallback
```

---

## PRS-GEO-005 — CoordinateTransformationUnavailable

Required transform is unavailable.

Example:

```text
NormalizedSource → OverlaySurface
```

Severity:

```text
Error
```

Recovery:

```text
UseFallback
```

Typical fallback:

```text
Overlay → SidePanel
```

---

## PRS-GEO-006 — CoordinateTransformationFailed

A known transform produced invalid output.

Severity:

```text
Error
```

Recovery:

```text
UseFallback
```

Previous committed RenderPlan remains valid.

---

## PRS-GEO-007 — GeometryNotVisible

Source geometry is outside current visible region.

This is not necessarily an error.

Default classification:

```text
Expected Presentation condition
```

Behavior:

* hide or mark unavailable;
* do not remove semantic PresentationItem.

No error event required.

---

## PRS-GEO-008 — GeometryRelationshipInvalid

Presentation-facing geometry relationships are inconsistent.

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
or
UseFallback
```

---

## PRS-GEO-009 — CommittedGeometryInvariantViolation

Committed Presentation geometry violates internal invariant.

Severity:

```text
Critical
```

Recovery:

```text
ResetPresentation
```

State:

```text
FAILED
```

---

# 20. Layout Errors

---

## PRS-LAY-001 — InvalidViewport

Viewport cannot safely support layout.

Severity:

```text
Warning
```

Recovery:

```text
ChangeViewport
```

Previous committed layout remains current.

---

## PRS-LAY-002 — TextMeasurementFailed

Presentation cannot produce trusted text measurements.

Severity:

```text
Error
```

Recovery:

```text
UseFallback
```

Fallback may use bounded approximate metrics or a simpler Presentation mode.

---

## PRS-LAY-003 — TypographyUnavailable

Requested typography resource or semantic profile cannot be used.

Severity:

```text
Warning
```

Recovery:

```text
UseFallback
```

---

## PRS-LAY-004 — OverflowUnresolved

Layout cannot satisfy overflow policy.

Severity:

```text
Error
```

Recovery:

```text
UseFallback
```

Presentation MUST NOT shrink text below readability rules merely to avoid this error.

---

## PRS-LAY-005 — OverlapUnresolved

Layout violates overlap policy after allowed resolution attempts.

Severity:

```text
Error
```

Recovery:

```text
UseFallback
```

---

## PRS-LAY-006 — SemanticOrderUnavailable

Presentation cannot obtain a deterministic semantic ordering required by the active strategy.

Presentation MUST NOT invent a new semantic reading order.

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
or
UseFallback
```

---

## PRS-LAY-007 — LayoutBudgetExceeded

Layout exceeded Presentation-local computation budget.

Severity:

```text
Warning
```

Recovery:

```text
UseFallback
```

If work is already obsolete, classify as supersession rather than error.

---

## PRS-LAY-008 — LayoutCandidateObsolete

Deprecated in v2.

Replacement:

```text
PRS-PRSREV-003 CandidateViewportObsolete
```

This condition is an expected supersession outcome.

---

## PRS-LAY-009 — CommittedLayoutInvariantViolation

Committed RenderPlan violates Presentation invariants.

Severity:

```text
Critical
```

Recovery:

```text
ResetPresentation
```

State:

```text
FAILED
```

---

# 21. Presentation Mode Errors

---

## PRS-MODE-001 — UnsupportedPresentationMode

Requested mode is unknown or unsupported.

Severity:

```text
Warning
```

Recovery:

```text
UseFallback
```

---

## PRS-MODE-002 — ModeIncompatibleWithContent

Mode cannot safely represent current accepted content.

Severity:

```text
Warning
```

Recovery:

```text
UseFallback
```

---

## PRS-MODE-003 — ModeRequirementsUnavailable

Required capability/data for the selected mode is missing.

Examples:

* Overlay needs geometry;
* marker mode needs source associations;
* target does not support overlay.

Severity:

```text
Warning or Error
```

Recovery:

```text
UseFallback
```

---

## PRS-MODE-004 — ModeTransitionRejected

Presentation-local state does not permit the requested mode mutation.

Severity:

```text
Warning
```

Recovery:

```text
RefreshPresentationRevision
or
CorrectInput
```

---

## PRS-MODE-005 — ModeCandidateInvalid

Candidate for requested mode fails Presentation validation.

Severity:

```text
Error
```

Recovery:

```text
UseFallback
```

Previous mode remains current.

---

## PRS-MODE-006 — ModeRecoveryFailed

Presentation cannot retain or verify previously committed state after a failed reconfiguration.

Severity:

```text
Critical
```

Recovery:

```text
ResetPresentation
```

State:

```text
FAILED
```

---

# 22. Presentation State Errors

---

## PRS-STATE-001 — InvalidStateTransition

Requested Presentation operation is invalid from current Presentation state.

Examples:

* update while `EMPTY`;
* mode change while `CLEARING`;
* normal mutation while `FAILED`.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## PRS-STATE-002 — PresentationNotReady

Operation requires a committed Presentation but none exists.

Severity:

```text
Info or Warning
```

Recovery:

```text
RebuildPresentation
```

---

## PRS-STATE-003 — ConflictingOperation

Presentation-local operation conflicts with another active operation.

This is not automatically an error.

Possible behavior:

```text
queue
coalesce
supersede
reject
```

If rejected:

Severity:

```text
Info
```

Recovery:

```text
None
or
RefreshPresentationRevision
```

---

## PRS-STATE-004 — Deprecated

Old meaning:

```text
OperationCancelled
```

Removed in v2.

Cancellation belongs to Runtime or expected Presentation supersession.

---

## PRS-STATE-005 — CurrentSnapshotMissing

Presentation state claims a committed Presentation exists but current Snapshot cannot be resolved.

Severity:

```text
Critical
```

Recovery:

```text
ResetPresentation
```

State:

```text
FAILED
```

---

## PRS-STATE-006 — CurrentPresentationStateConflict

Current committed identifiers disagree.

Examples:

* PresentationId mismatch;
* context mismatch;
* Snapshot/RenderPlan mismatch;
* impossible revision relationship.

Severity:

```text
Critical
```

Recovery:

```text
ResetPresentation
```

---

## PRS-STATE-007 — LogicalClearFailed

Presentation could not establish a trustworthy logically cleared state.

Severity:

```text
Critical
```

Recovery:

```text
ResetPresentation
```

Important distinction:

failure to physically destroy a UI resource is not this error.

That belongs to UI Adapter/platform.

---

# 23. Event Publication Errors

Presentation no longer owns errors for consuming arbitrary external business events.

The only Presentation-relevant event errors concern publication of Presentation-owned facts.

---

## PRS-PUB-001 — EventSerializationFailed

A Presentation-owned event cannot be serialized according to the canonical event contract.

Severity:

```text
Error
```

Recovery:

```text
CorrectInput / fix implementation
```

The committed Presentation remains committed.

---

## PRS-PUB-002 — EventPublicationFailed

Presentation committed successfully but Event Bus publication failed.

Severity:

```text
Error
```

Key invariant:

```text
Presentation commit remains valid
```

The Presentation operation MUST NOT be rerun merely to recreate the event.

Recovery belongs to Event Bus/outbox policy.

Presentation may report diagnostics.

---

## PRS-PUB-003 — EventPayloadContractViolation

Presentation constructed an event payload violating its public event schema.

Severity:

```text
Error
```

If caused by isolated event-building code:

```text
Presentation remains committed
```

If it reveals committed Presentation corruption:

```text
escalate to internal invariant failure
```

---

# 24. Removed Event Errors

The following v1/v2-pre-sync concepts are no longer Presentation-owned:

```text
UnsupportedEventType
InvalidExternalEventPayload
DuplicateExternalEvent
ExternalEventOutOfOrder
ExternalEventContextMismatch
```

Those belong to:

```text
Event Router
owning producer/consumer adapter
canonical Event Bus validation
```

Presentation's business correctness does not depend on directly consuming those events.

---

# 25. Resource Errors

Presentation owns only Presentation-local resource behavior.

Global Runtime admission/resource policy remains external.

---

## PRS-RES-001 — PresentationComplexityLimitExceeded

Candidate Presentation exceeds configured Presentation complexity limits.

Examples:

* too many visible items;
* excessive marker count;
* excessive layout nodes.

Severity:

```text
Error
```

Recovery:

```text
UseFallback
```

Possible strategies:

* pagination;
* visible-region presentation;
* simplified marker plan;
* partial Presentation.

---

## PRS-RES-002 — PresentationMemoryBudgetExceeded

Presentation-local preparation or retention exceeds budget assigned to Presentation.

Severity:

```text
Error
```

Recovery:

```text
UseFallback
or
ClearPresentation
```

Presentation may release:

* obsolete Candidate state;
* previous Presentation reference where policy allows;
* reusable local caches.

It MUST NOT evict arbitrary Runtime Artifacts it does not own.

---

## PRS-RES-003 — PresentationComputationBudgetExceeded

Presentation computation exceeds assigned budget.

Severity:

```text
Warning or Error
```

Recovery:

```text
UseFallback
```

---

## PRS-RES-004 — TooManyPresentationOperations

Too many Presentation-local operations are pending.

Severity:

```text
Warning
```

Recovery:

```text
coalesce
supersede
reject low-value work
```

This does not modify Scheduler state.

---

## PRS-RES-005 — PresentationStrategyUnavailable

Requested Presentation strategy cannot be instantiated or used.

Severity:

```text
Error
```

Recovery:

```text
UseFallback
```

---

# 26. Recovery Errors

---

## PRS-REC-001 — KnownGoodStateUnavailable

Presentation expected a safe previous committed state but cannot access or verify one.

Severity:

```text
Error
```

Recovery:

```text
ClearPresentation
```

May escalate to `FAILED` if current state cannot be trusted.

---

## PRS-REC-002 — RestoredSnapshotUnavailable

Requested restoration source does not exist.

Severity:

```text
Error
```

Recovery:

```text
RebuildPresentation
```

---

## PRS-REC-003 — RestoredSnapshotInvalid

Restored/persisted PresentationSnapshot fails validation.

Severity:

```text
Error
```

Recovery:

```text
RebuildPresentation
or
ClearPresentation
```

Snapshot MUST NOT become `READY`.

---

## PRS-REC-004 — RestoredSnapshotVersionUnsupported

Stored snapshot contract incompatible with current Presentation version.

Severity:

```text
Error
```

Recovery:

```text
RebuildPresentation
```

---

## PRS-REC-005 — VerifiedRecoveryFailed

Presentation cannot restore a trustworthy committed state.

Severity:

```text
Critical
```

Recovery:

```text
ResetPresentation
```

State:

```text
FAILED
```

---

# 27. Internal Errors

Internal errors are rare and indicate Presentation-owned correctness risk.

---

## PRS-INT-001 — UnexpectedInternalFailure

Unexpected implementation failure with no better Presentation error classification.

Severity depends on effect.

If current committed state remains trusted:

```text
Error
```

If current state cannot be trusted:

```text
Critical
→ FAILED
```

---

## PRS-INT-002 — InvariantViolation

A core Presentation invariant has been violated.

Examples:

* mutable committed Snapshot;
* Candidate exposed as current;
* stale Candidate committed;
* invalid identity ownership;
* impossible PresentationRevision ordering.

Severity:

```text
Critical
```

Recovery:

```text
ResetPresentation
```

---

## PRS-INT-003 — AtomicCommitFailed

Presentation cannot atomically commit:

```text
PresentationRevision
+
PresentationSnapshot
+
RenderPlan
```

If previous committed state is certainly unchanged:

```text
Error
preserve previous state
```

If commit outcome is uncertain:

```text
Critical
FAILED
```

This distinction must be explicit in implementation.

---

## PRS-INT-004 — StrategyContractViolation

A Presentation strategy produces output violating Presentation contracts.

Severity:

```text
Error
```

Recovery:

```text
UseFallback
```

If invalid output reached committed state:

```text
Critical
FAILED
```

---

## PRS-INT-005 — SnapshotSerializationViolation

Committed or Candidate Snapshot cannot satisfy the public serialization contract.

Candidate-only:

```text
Error
discard
```

Committed state affected:

```text
Critical
FAILED
```

---

## PRS-INT-006 — RenderPlanSerializationViolation

RenderPlan cannot satisfy public serialization contract.

Same escalation rules as Snapshot serialization failure.

---

# 28. UI Apply Failures

These are not Presentation errors.

Typical UI Adapter-owned failures:

```text
PresentationApplyRejectedStale
PresentationApplyTargetMismatch
PresentationApplyTargetUnavailable
PresentationApplyFailed
```

Presentation may observe them for recovery coordination.

It MUST NOT emit:

```text
PresentationFailed
```

solely because UI apply failed.

---

# 29. Artifact Store Failures

Artifact publication/access failures remain externally owned.

Presentation may receive normalized outcomes such as:

```text
ArtifactUnavailable
LeaseRejected
ArtifactDisposed
```

Presentation then decides whether it can:

```text
fallback
reject Candidate
request rebuild through Application
```

but it must not rename Artifact Store failure as an internal Presentation invariant unless its own state became corrupt.

---

# 30. Error-to-State Mapping

| Condition                     | Presentation State Result                |
| ----------------------------- | ---------------------------------------- |
| Validation rejection          | Preserve current stable state            |
| Artifact incompatibility      | Preserve current stable state            |
| PresentationRevision conflict | Preserve current stable state            |
| Candidate superseded          | Preserve current stable state            |
| Runtime authority rejected    | Preserve current stable state            |
| Invalid geometry              | Reject Candidate / fallback              |
| Layout failure                | Preserve previous RenderPlan / fallback  |
| Mode failure                  | Preserve current mode / fallback         |
| UI apply failure              | No automatic Presentation state failure  |
| Event publication failure     | Committed Presentation remains committed |
| Resource pressure             | Fallback / reject Candidate              |
| Recovery failure              | `FAILED` if correctness lost             |
| Internal invariant violation  | `FAILED`                                 |

---

# 31. PresentationRejected Mapping

`PresentationRejected` may describe externally meaningful non-commit outcomes such as:

```text
invalid Presentation command
Artifact compatibility rejection
PresentationRevision conflict
unsupported mode without usable fallback
invalid geometry
invalid viewport
target capability mismatch
Runtime authority rejection
```

It does not mean Presentation state is corrupted.

---

# 32. PresentationFailed Mapping

`PresentationFailed` is published only when:

```text
Presentation-owned correctness cannot be trusted
```

Typical causes:

* committed Snapshot/RenderPlan mismatch;
* PresentationRevision corruption;
* state registry corruption;
* uncertain atomic commit;
* verified recovery failure.

---

# 33. No Public Error Event Required

Normally diagnostics-only:

```text
CandidateSuperseded
CandidateViewportObsolete
duplicate request
coalesced operation
no-op
Runtime cancellation observed
same mode requested
same viewport received
```

These should not flood the Event Bus.

---

# 34. Fallback Principles

Presentation prefers graceful degradation when correctness is preserved.

Fallback must be:

* deterministic;
* explicit;
* observable;
* reversible where practical;
* semantically valid.

Fallback MUST NOT:

* fabricate source geometry;
* fabricate translation;
* mutate upstream Artifact content;
* silently violate semantic reading order;
* reduce text below readability policy without explicit policy;
* conceal internal corruption.

---

# 35. Mode Fallback

Possible image-reading path:

```text
Overlay
    ↓
FocusedOverlay
    ↓
SidePanel
```

Possible structured-text path:

```text
StyledTextReader
    ↓
SimplifiedTextReader
```

There is no universal fallback order for every content type.

---

# 36. Geometry Fallback

If spatial mapping is unavailable:

```text
Precise source-aligned layout
        ↓
Marker-based association
        ↓
SidePanel
```

Presentation MUST NOT fabricate geometry to keep Overlay alive.

---

# 37. Layout Fallback

Possible bounded actions:

```text
wrap
expand allowed container
scroll
reduce secondary content
simplify marker placement
switch Presentation mode
```

Readability rules remain mandatory.

---

# 38. Typography Fallback

Possible:

```text
Requested semantic typography
    ↓
Compatible fallback typography
    ↓
System-neutral default semantic profile
```

Presentation contracts should not depend on native font handles.

---

# 39. Diagnostics

Error diagnostics may include:

```text
errorCode
category
severity
recoveryHint
presentationContextId
presentationId
presentationRevision
expectedPresentationRevision
runtimeRevisionId
workItemId
attemptId
targetId
targetRevision
viewportRevision
operationId
requestId
correlationId
traceId
fallbackApplied
```

---

# 40. Privacy

Public error payloads and normal diagnostics MUST NOT contain:

* source text;
* translated text;
* screenshots;
* page images;
* raw geometry arrays unless explicitly diagnostic and access-controlled;
* provider prompts;
* provider responses;
* secrets;
* native handles.

Use:

```text
IDs
counts
bounded summaries
error codes
```

instead.

---

# 41. Logging

Recommended structured fields:

```text
timestamp
operation
presentationState
errorCode
category
severity
recoveryHint
presentationContextId
presentationId
presentationRevision
runtimeRevisionId
workItemId
attemptId
targetRevision
viewportRevision
operationId
correlationId
traceId
durationMs
fallbackApplied
```

Do not log every normal supersession or duplicate at warning/error level.

---

# 42. Logging Levels

Suggested:

```text
Debug
    fine-grained diagnostic

Info
    normal supersession/no-op where useful

Warning
    recoverable Presentation rejection

Error
    operation failure requiring fallback/recovery

Critical
    Presentation correctness compromised
```

---

# 43. Metrics

Recommended:

```text
presentation_error_total
presentation_rejection_total
presentation_failure_total
presentation_fallback_total
presentation_candidate_superseded_total
presentation_revision_conflict_total
presentation_geometry_failure_total
presentation_layout_failure_total
presentation_mode_failure_total
presentation_event_publish_failure_total
presentation_recovery_total
```

Avoid high-cardinality labels such as:

```text
PresentationId
SessionId
ContentId
ArtifactId
```

---

# 44. Testing — Ownership

Tests MUST verify:

* Runtime authority rejection does not map to internal Presentation failure;
* Runtime cancellation does not create Presentation failure;
* UI apply failure remains UI Adapter-owned;
* Translation failure remains Translation-owned;
* Artifact Store failure remains externally owned;
* Presentation errors describe only Presentation responsibilities.

---

# 45. Testing — Candidate Isolation

Tests MUST verify:

* validation error does not mutate current Presentation;
* layout error does not partially mutate RenderPlan;
* mode Candidate failure preserves previous mode;
* Artifact compatibility failure preserves current state;
* failed Candidate never becomes current.

---

# 46. Testing — Presentation Revision

Tests MUST verify:

* conflict is deterministic;
* old Candidate cannot commit;
* PresentationRevision increments only after successful commit;
* non-monotonic commit triggers invariant protection;
* RuntimeRevisionId never substitutes for PresentationRevision.

---

# 47. Testing — Runtime Authority

Tests MUST verify:

```text
Runtime authority rejected
    ↓
Candidate discarded
    ↓
no PresentationRevision increment
    ↓
no PresentationFailed
```

---

# 48. Testing — UI Apply Boundary

Tests MUST verify:

```text
Presentation commit succeeds
UI apply fails
```

does not automatically corrupt Presentation state.

---

# 49. Testing — Event Publication

Tests MUST verify:

```text
Presentation commit succeeds
Presentation event publication fails
```

does not:

* roll back committed Presentation automatically;
* create duplicate commit;
* rerun business work automatically.

---

# 50. Testing — Fatal Failures

Tests MUST verify `FAILED` only for Presentation-owned correctness failure such as:

* committed revision mismatch;
* committed Snapshot/RenderPlan mismatch;
* state registry corruption;
* uncertain atomic commit;
* unrecoverable recovery failure.

---

# 51. Compatibility

Error contract versioning follows semantic versioning.

Major revision required when:

* existing ErrorCode meaning changes;
* ownership changes;
* severity semantics become incompatible;
* recovery semantics become incompatible;
* public required fields change.

New compatible codes may be added in a minor version where consumers safely handle unknown codes.

---

# 52. Deprecated Error Codes

The following old concepts are deprecated or removed from active v2 semantics:

```text
PRS-REV-001 StaleContentRevision
PRS-REV-002 FutureContentRevision
PRS-REV-003 StaleTranslationRevision
PRS-STATE-004 OperationCancelled

PRS-EVENT-001 UnsupportedEventType
PRS-EVENT-002 InvalidEventPayload
PRS-EVENT-003 DuplicateEvent
PRS-EVENT-004 EventOutOfOrder
PRS-EVENT-005 EventContextMismatch
```

Reason:

```text
Runtime / upstream Artifact compatibility / Event Router
now own those semantics.
```

Historical codes should remain documented for one compatibility cycle if implementation has already shipped them.

---

# 53. Error Code Summary

## Validation

```text
PRS-VAL-001 MissingRequiredField
PRS-VAL-002 InvalidFieldValue
PRS-VAL-003 InvalidPresentationProfile
PRS-VAL-004 MissingPresentationInput
PRS-VAL-005 UnsupportedContractVersion
```

## Context

```text
PRS-CTX-001 PresentationContextNotFound
PRS-CTX-002 PresentationNotFound
PRS-CTX-003 PresentationItemNotFound
PRS-CTX-004 PresentationIdentityConflict
```

## Artifact

```text
PRS-ART-001 UnsupportedArtifactType
PRS-ART-002 ArtifactReferenceInvalid
PRS-ART-003 ArtifactCompatibilityMismatch
PRS-ART-004 RequiredArtifactDataUnavailable
PRS-ART-005 ArtifactLeaseUnavailable
```

## Presentation Revision

```text
PRS-PRSREV-001 PresentationRevisionConflict
PRS-PRSREV-002 CandidateSuperseded
PRS-PRSREV-003 CandidateViewportObsolete
PRS-PRSREV-004 CandidateTargetObsolete
PRS-PRSREV-005 NonMonotonicPresentationRevision
PRS-PRSREV-006 SnapshotRenderPlanRevisionMismatch
```

## Geometry

```text
PRS-GEO-001 InvalidBoundingBox
PRS-GEO-002 InvalidPolygon
PRS-GEO-003 MissingCoordinateSpace
PRS-GEO-004 UnsupportedCoordinateSpace
PRS-GEO-005 CoordinateTransformationUnavailable
PRS-GEO-006 CoordinateTransformationFailed
PRS-GEO-008 GeometryRelationshipInvalid
PRS-GEO-009 CommittedGeometryInvariantViolation
```

## Layout

```text
PRS-LAY-001 InvalidViewport
PRS-LAY-002 TextMeasurementFailed
PRS-LAY-003 TypographyUnavailable
PRS-LAY-004 OverflowUnresolved
PRS-LAY-005 OverlapUnresolved
PRS-LAY-006 SemanticOrderUnavailable
PRS-LAY-007 LayoutBudgetExceeded
PRS-LAY-009 CommittedLayoutInvariantViolation
```

## Mode

```text
PRS-MODE-001 UnsupportedPresentationMode
PRS-MODE-002 ModeIncompatibleWithContent
PRS-MODE-003 ModeRequirementsUnavailable
PRS-MODE-004 ModeTransitionRejected
PRS-MODE-005 ModeCandidateInvalid
PRS-MODE-006 ModeRecoveryFailed
```

## State

```text
PRS-STATE-001 InvalidStateTransition
PRS-STATE-002 PresentationNotReady
PRS-STATE-003 ConflictingOperation
PRS-STATE-005 CurrentSnapshotMissing
PRS-STATE-006 CurrentPresentationStateConflict
PRS-STATE-007 LogicalClearFailed
```

## Publication

```text
PRS-PUB-001 EventSerializationFailed
PRS-PUB-002 EventPublicationFailed
PRS-PUB-003 EventPayloadContractViolation
```

## Resource

```text
PRS-RES-001 PresentationComplexityLimitExceeded
PRS-RES-002 PresentationMemoryBudgetExceeded
PRS-RES-003 PresentationComputationBudgetExceeded
PRS-RES-004 TooManyPresentationOperations
PRS-RES-005 PresentationStrategyUnavailable
```

## Recovery

```text
PRS-REC-001 KnownGoodStateUnavailable
PRS-REC-002 RestoredSnapshotUnavailable
PRS-REC-003 RestoredSnapshotInvalid
PRS-REC-004 RestoredSnapshotVersionUnsupported
PRS-REC-005 VerifiedRecoveryFailed
```

## Internal

```text
PRS-INT-001 UnexpectedInternalFailure
PRS-INT-002 InvariantViolation
PRS-INT-003 AtomicCommitFailed
PRS-INT-004 StrategyContractViolation
PRS-INT-005 SnapshotSerializationViolation
PRS-INT-006 RenderPlanSerializationViolation
```

---

# 54. Architecture Invariants

1. Presentation errors describe Presentation-owned failures only.

2. Runtime authority rejection is not a Presentation internal error.

3. Runtime cancellation is not a Presentation internal error.

4. Supersession is not internal failure.

5. UI apply failure belongs to UI Adapter.

6. Translation failures remain Translation-owned.

7. Artifact publication failures remain externally owned.

8. Presentation owns only PresentationRevision.

9. Runtime Revision does not replace PresentationRevision.

10. Candidate failure never partially mutates committed state.

11. Previous committed Presentation remains valid during recoverable failure.

12. PresentationRevision increments only after successful commit.

13. PresentationRevision conflict is expected concurrency behavior.

14. Candidate supersession normally requires no public error event.

15. Geometry fallback must never fabricate source geometry.

16. Layout fallback must preserve readability.

17. Unsupported mode may fall back without entering `FAILED`.

18. `PresentationRejected` means no Presentation commit occurred.

19. `PresentationFailed` means Presentation-owned correctness is untrusted.

20. Event publication failure does not invalidate an already committed Presentation.

21. Event publication failure does not rerun Presentation work automatically.

22. Logical clear is distinct from native resource destruction.

23. Resource errors do not grant Presentation ownership of Runtime resource policy.

24. Error payloads are immutable and serializable.

25. Error messages are non-authoritative.

26. Diagnostics remain privacy-safe.

27. Normal error payloads contain no full user content.

28. Stable ErrorCodes preserve meaning across implementations.

---

# 55. Related Documents

```text
doc/02-modules/presentation/MODULE.md
doc/02-modules/presentation/CONTRACT.md
doc/02-modules/presentation/STATES.md
doc/02-modules/presentation/EVENTS.md
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
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/02-modules/translation/ERRORS.md
doc/02-modules/ui-adapter/ERRORS.md
doc/02-modules/reading-session/ERRORS.md
```

---

# 56. Completion Criteria

This error specification is synchronized when:

* Presentation error ownership is explicit;
* Runtime authority rejection is external;
* Runtime cancellation is external;
* PresentationRevision is the only Presentation-owned revision guard;
* Artifact compatibility errors replace old ContentRevision/TranslationRevision ownership;
* event-consumption errors are removed from Presentation;
* event publication failure remains distinguishable from Presentation business failure;
* UI apply failure remains UI Adapter-owned;
* expected supersession is not treated as failure;
* recoverable Candidate failure preserves current Presentation;
* `FAILED` is reserved for Presentation-owned correctness loss;
* every stable Presentation error has deterministic severity and recovery semantics;
* fallback is bounded and correctness-preserving;
* tests cover ownership, revision, authority, candidate isolation, publication, recovery, and privacy.

---

# 57. Summary

Presentation error flow is:

```text
Presentation Operation
    ↓
Candidate preparation
    ↓
Possible Presentation-owned failure?
    ├── yes
    │    ↓
    │  reject / fallback / FAILED if invariant broken
    │
    └── no
         ↓
      Runtime authority revalidation
         ├── rejected
         │      ↓
         │   discard Candidate
         │   not Presentation failure
         │
         └── accepted
                ↓
             commit
                ↓
             Presentation current
                ↓
             UI Adapter apply
                ├── success
                └── UI-owned failure
```

The central rule is:

```text
Presentation owns failures in constructing
and maintaining Presentation state.

Runtime owns whether the work may commit.

UI Adapter owns whether committed state becomes visible.
```
