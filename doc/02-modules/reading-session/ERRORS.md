# Reading Session Errors

> **Project:** CRAI
> **Module:** `reading-session`
> **Path:** `doc/02-modules/reading-session/ERRORS.md`
> **Version:** 3.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-08

---

# 1. Purpose

This document defines the Reading Session-owned error model.

It standardizes:

```text
error ownership
stable ErrorCode values
error categories
severity
recovery semantics
ReadingContextRevision conflicts
lifecycle rejection
context validation
configuration validation
consistency failures
event publication failures
internal invariant failures
diagnostics
privacy
```

Reading Session errors describe failures inside the reading-domain boundary only.

They do not describe Runtime or processing execution failure.

---

# 2. Error Ownership

Reading Session owns errors related to:

```text
ReadingSession lifecycle
ReadingContext
ReadingSource
ReadingTarget
ReadingPosition
ReadingContextRevision
SessionConfiguration
domain validation
domain consistency
domain commit
domain recovery
```

Reading Session does not own:

```text
RuntimeRevision failure
WorkItem failure
Attempt failure
Runtime cancellation
Runtime retry
Capture failure
Recognition failure
Translation failure
Artifact publication failure
Presentation failure
UI apply failure
Storage implementation failure
```

External failures may be referenced for correlation when necessary.

They must not be reclassified as Reading Session-owned errors.

---

# 3. Error Philosophy

## 3.1 Stable Machine Contract

Every public error has a stable machine-readable:

```text
ErrorCode
```

Consumers MUST NOT branch on:

* exception types;
* stack traces;
* implementation language;
* human-readable messages.

---

## 3.2 Domain Failure Only

Reading Session errors answer:

> Why could this reading-domain operation not be committed safely?

They do not answer:

> Why did processing execution fail?

---

## 3.3 Failed Candidate Does Not Corrupt Current State

Default behavior:

```text
Current committed ReadingContext
        +
Candidate ReadingContext
        ↓
Candidate rejected
        ↓
Discard Candidate
        ↓
Current ReadingContext unchanged
```

---

## 3.4 Expected Outcomes Are Not Fatal Errors

Examples:

```text
duplicate request
no-op update
ReadingContextRevision conflict
already paused
already active
candidate superseded by newer domain mutation
```

These may produce `Info` or `Warning` results.

They do not imply internal corruption.

---

# 4. Error Categories

Reading Session v3 defines:

| Prefix | Category                 |
| ------ | ------------------------ |
| `VAL`  | Input Validation         |
| `SES`  | Session Lifecycle        |
| `CTX`  | Reading Context          |
| `REV`  | ReadingContextRevision   |
| `CFG`  | Session Configuration    |
| `CON`  | Domain Consistency       |
| `REC`  | Recovery                 |
| `PUB`  | Domain Event Publication |
| `INT`  | Internal Invariant       |

Removed categories:

```text
ProcessingIntent
ContentRevision lifecycle
```

---

# 5. Error Code Format

```text
RS-<CATEGORY>-<NUMBER>
```

Examples:

```text
RS-VAL-001
RS-SES-004
RS-CTX-003
RS-REV-001
RS-CFG-002
RS-CON-001
RS-INT-002
```

Rules:

* codes are stable;
* meanings are never reused;
* deprecated codes remain documented;
* incompatible meaning changes require major version review.

---

# 6. Severity

```text
Info
Warning
Error
Critical
```

Meaning:

| Severity   | Meaning                                                    |
| ---------- | ---------------------------------------------------------- |
| `Info`     | Expected control/no-op condition                           |
| `Warning`  | Invalid/rejected domain request                            |
| `Error`    | Domain operation failed and recovery is required           |
| `Critical` | Reading Session-owned correctness is no longer trustworthy |

Severity does not determine Runtime retry.

---

# 7. Recovery Hint

```text
RecoveryHint
- None
- CorrectInput
- RefreshReadingContext
- RefreshReadingContextRevision
- ReplaceSource
- UpdateTarget
- UpdateConfiguration
- RebuildContext
- RestoreKnownGood
- CancelSession
- DisposeSession
- RestartSession
```

Reading Session recovery hints describe reading-domain recovery.

They do not schedule Runtime retry.

---

# 8. Public Error Contract

Conceptually:

```text
ReadingSessionError
├── errorId
├── errorCode
├── category
├── severity
├── recoveryHint
├── messageKey?
├── readingSessionId?
├── readingContextRevision?
├── expectedReadingContextRevision?
├── currentReadingContextRevision?
├── sourceId?
├── targetId?
├── lifecycleState?
├── requestId?
├── correlationId?
├── causationId?
├── traceId?
├── diagnosticRef?
└── occurredAt
```

All public error values are immutable and serializable.

---

# 9. Message Rules

Human-readable messages:

* are optional;
* are non-authoritative;
* must not contain raw reading content;
* must not expose secrets;
* must not expose implementation internals.

Consumers branch on:

```text
errorCode
category
recoveryHint
```

---

# 10. Validation Errors

Validation errors occur before domain state commit.

They never partially mutate ReadingSession or ReadingContext.

---

## RS-VAL-001 — MissingReadingSessionId

Required ReadingSessionId is missing.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## RS-VAL-002 — InvalidReadingSessionId

ReadingSessionId is malformed or invalid.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## RS-VAL-003 — MissingRequiredField

A command is missing required reading-domain data.

Examples:

```text
ReadingSource
ReadingTarget
SessionConfiguration
expectedReadingContextRevision
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## RS-VAL-004 — InvalidFieldValue

A supplied domain value is invalid.

Examples:

* malformed target;
* invalid position;
* invalid language identifier;
* unsupported enum value.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## RS-VAL-005 — UnsupportedContractVersion

Command contract version is incompatible.

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
```

---

# 11. Session Lifecycle Errors

---

## RS-SES-001 — SessionNotFound

The ReadingSession does not exist.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## RS-SES-002 — SessionAlreadyExists

A ReadingSession with the requested identity already exists.

Severity:

```text
Warning
```

Recovery:

```text
None
```

---

## RS-SES-003 — SessionAlreadyActive

Session is already `ACTIVE`.

Requested activation is a no-op.

Severity:

```text
Info
```

Recovery:

```text
None
```

---

## RS-SES-004 — SessionAlreadyPaused

Session is already `PAUSED`.

Severity:

```text
Info
```

Recovery:

```text
None
```

---

## RS-SES-005 — SessionCompleted

Requested mutation targets an already completed ReadingSession.

Severity:

```text
Info
```

Recovery:

```text
RestartSession
```

---

## RS-SES-006 — SessionCancelled

Requested mutation targets a canceled ReadingSession.

Severity:

```text
Info
```

Recovery:

```text
RestartSession
```

---

## RS-SES-007 — SessionDisposed

Requested mutation targets a disposed ReadingSession.

Severity:

```text
Info
```

Recovery:

```text
RestartSession
```

---

## RS-SES-008 — InvalidSessionTransition

Requested lifecycle transition is not legal.

Examples:

```text
COMPLETED → ACTIVE
CANCELLED → ACTIVE
DISPOSED → PAUSED
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

Important:

A rejected invalid command is not itself a Critical consistency failure.

A Critical error occurs only if the invalid transition was actually committed internally.

---

# 12. Reading Context Errors

---

## RS-CTX-001 — ReadingContextUnavailable

No committed ReadingContext currently exists.

Severity:

```text
Warning
```

Recovery:

```text
RebuildContext
```

---

## RS-CTX-002 — ReadingContextInvalid

Current ReadingContext cannot be trusted.

Severity:

```text
Error
```

Recovery:

```text
RebuildContext
```

This must describe a Reading Session domain problem.

Do not use this for:

```text
OCR failed
Translation failed
Presentation failed
```

---

## RS-CTX-003 — ReadingContextDisposed

Requested context has been disposed.

Severity:

```text
Warning
```

Recovery:

```text
RebuildContext
```

---

## RS-CTX-004 — ReadingContextSessionMismatch

Supplied ReadingContext belongs to another ReadingSession.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## RS-CTX-005 — InvalidReadingSource

ReadingSource cannot represent a valid reading-domain source.

Severity:

```text
Warning
```

Recovery:

```text
ReplaceSource
```

---

## RS-CTX-006 — InvalidReadingTarget

ReadingTarget is invalid or incompatible with the current ReadingSource.

Severity:

```text
Warning
```

Recovery:

```text
UpdateTarget
```

---

## RS-CTX-007 — InvalidReadingPosition

ReadingPosition cannot be interpreted for the current source/target.

Severity:

```text
Warning
```

Recovery:

```text
UpdateTarget
```

---

## RS-CTX-008 — CandidateContextInvalid

Candidate ReadingContext violates domain invariants.

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
or
RebuildContext
```

Current committed context remains unchanged.

---

# 13. ReadingContextRevision Errors

Reading Session owns only:

```text
ReadingContextRevision
```

It does not own Runtime Revision authority.

---

## RS-REV-001 — ReadingContextRevisionConflict

Command expected:

```text
Revision N
```

but current is:

```text
Revision N+K
```

Severity:

```text
Info or Warning
```

Recovery:

```text
RefreshReadingContextRevision
```

This is normal optimistic concurrency behavior.

---

## RS-REV-002 — ReadingContextRevisionMissing

A command requiring current revision did not supply one.

Severity:

```text
Warning
```

Recovery:

```text
RefreshReadingContextRevision
```

---

## RS-REV-003 — ReadingContextRevisionInvalid

Revision value is malformed or incompatible with the ReadingSession.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

## RS-REV-004 — NonMonotonicReadingContextRevision

Internal commit attempts to advance to an invalid/non-monotonic revision.

Severity:

```text
Critical
```

Recovery:

```text
RestoreKnownGood
or
RestartSession
```

This indicates implementation/state corruption.

---

## RS-REV-005 — DuplicateSemanticRevision

A candidate would create a new revision without any semantic ReadingContext change.

Preferred result:

```text
NoOp
```

Severity:

```text
Info
```

Recovery:

```text
None
```

This should normally be prevented before reaching an error path.

---

# 14. Removed Revision Errors

The following old error semantics are removed:

```text
RevisionAlreadyCurrent
RevisionSuperseded
RevisionArchived
RevisionDiscarded
DuplicateRevision
```

as lifecycle states.

Reason:

ReadingContextRevision no longer has:

```text
Current
Superseded
Archived
Discarded
```

public lifecycle states.

Historical retention is a storage/lifetime concern.

Runtime supersession is Runtime-owned.

---

# 15. Runtime Staleness Is Not RS-REV

Do not map:

```text
Runtime Revision obsolete
Attempt superseded
Work canceled
```

to:

```text
ReadingContextRevisionConflict
```

These are separate authority domains.

---

# 16. Configuration Errors

---

## RS-CFG-001 — UnsupportedLanguage

Requested language value is unsupported by Reading Session domain policy.

Severity:

```text
Warning
```

Recovery:

```text
UpdateConfiguration
```

Provider capability failure belongs outside Reading Session.

---

## RS-CFG-002 — UnsupportedReadingMode

Requested reading-domain mode is unsupported.

Severity:

```text
Warning
```

Recovery:

```text
UpdateConfiguration
```

Do not confuse ReadingMode with PresentationMode.

---

## RS-CFG-003 — InvalidConfigurationCombination

Session configuration contains incompatible domain options.

Severity:

```text
Warning
```

Recovery:

```text
UpdateConfiguration
```

---

## RS-CFG-004 — ConfigurationVersionConflict

Configuration mutation was based on stale Reading Session configuration state.

Severity:

```text
Warning
```

Recovery:

```text
RefreshReadingContext
```

Where configuration is part of ReadingContext, prefer the primary ReadingContextRevision concurrency guard rather than maintaining a second unnecessary concurrency domain.

---

# 17. Processing Intent Errors Removed

Reading Session v3 does not define:

```text
ProcessingIntentNotFound
ProcessingIntentAlreadyPublished
ProcessingIntentObsolete
ProcessingIntentFulfilled
ProcessingIntentDiscarded
ProcessingIntentCannotBeCreated
```

Reason:

Reading Session no longer owns ProcessingIntent.

Pipeline requirement evaluation belongs to:

```text
Business Pipeline Orchestration
```

Runtime execution lifecycle belongs to:

```text
Runtime Control
```

---

# 18. Domain Consistency Errors

Consistency errors indicate Reading Session-owned invariants were actually violated.

They should be rare.

---

## RS-CON-001 — MultipleCurrentContexts

More than one ReadingContext is exposed as current for one ReadingSession.

Severity:

```text
Critical
```

Recovery:

```text
RestoreKnownGood
```

---

## RS-CON-002 — ContextRevisionMismatch

Current ReadingContextSnapshot revision does not match:

```text
currentReadingContextRevision
```

Severity:

```text
Critical
```

Recovery:

```text
RestoreKnownGood
```

---

## RS-CON-003 — CommittedInvalidLifecycleTransition

ReadingSession internal state contains a lifecycle transition forbidden by `STATES.md`.

Severity:

```text
Critical
```

Recovery:

```text
RestoreKnownGood
or
RestartSession
```

This differs from `RS-SES-008`, where an invalid requested transition is safely rejected.

---

## RS-CON-004 — DomainHistoryCorrupted

Retained Reading Session history cannot be reconstructed consistently.

Examples:

* duplicate revision sequence;
* missing required committed snapshot;
* impossible lifecycle ordering;
* conflicting aggregate identities.

Severity:

```text
Critical
```

Recovery:

```text
RestoreKnownGood
or
RestartSession
```

---

## RS-CON-005 — AggregateIdentityConflict

ReadingSession-owned objects disagree about aggregate identity.

Severity:

```text
Critical
```

Recovery:

```text
RestoreKnownGood
```

---

# 19. Recovery Errors

---

## RS-REC-001 — RestoredSessionInvalid

Persisted ReadingSession data fails domain validation.

Severity:

```text
Error
```

Recovery:

```text
RestartSession
```

Invalid restored data must not become authoritative.

---

## RS-REC-002 — RestoredContextInvalid

Persisted ReadingContext fails validation.

Severity:

```text
Error
```

Recovery:

```text
RebuildContext
```

---

## RS-REC-003 — RestoredRevisionInvalid

Persisted current ReadingContextRevision is inconsistent with restored domain state.

Severity:

```text
Critical
```

Recovery:

```text
RestoreKnownGood
or
RestartSession
```

---

## RS-REC-004 — KnownGoodStateUnavailable

Recovery requires a valid known-good domain snapshot but none exists.

Severity:

```text
Error
```

Recovery:

```text
RestartSession
```

---

# 20. Event Publication Errors

Reading Session commit and event publication are separate technical operations.

---

## RS-PUB-001 — EventSerializationFailed

A Reading Session-owned event cannot satisfy its public event schema.

Severity:

```text
Error
```

The already committed domain state remains committed.

---

## RS-PUB-002 — EventPublicationFailed

Reading Session state committed successfully but publication of the corresponding domain fact failed.

Severity:

```text
Error
```

Critical invariant:

```text
Domain commit remains valid.
```

Do not:

```text
roll back valid domain state
rerun domain command
create another ReadingContextRevision
```

merely to recreate the event.

Recovery belongs to Event Bus/outbox/reconciliation policy.

---

## RS-PUB-003 — EventPayloadContractViolation

Constructed event payload violates Reading Session event schema.

Severity:

```text
Error
```

If the problem only affects event construction:

```text
committed Reading Session state remains valid
```

If it reveals committed domain corruption, escalate to a Consistency/Internal error.

---

# 21. Internal Errors

---

## RS-INT-001 — UnexpectedInternalFailure

Unexpected failure inside Reading Session with no more specific classification.

If committed state remains trustworthy:

```text
Severity = Error
```

If correctness is uncertain:

```text
Severity = Critical
```

---

## RS-INT-002 — InvariantViolation

A core Reading Session invariant was violated.

Examples:

* Candidate exposed as current;
* committed snapshot mutated;
* impossible revision sequence;
* multiple current contexts;
* illegal domain ownership mutation.

Severity:

```text
Critical
```

Recovery:

```text
RestoreKnownGood
or
RestartSession
```

---

## RS-INT-003 — AtomicDomainCommitFailed

Reading Session could not atomically commit:

```text
ReadingContextRevision
+
ReadingContextSnapshot
+
current context reference
```

If previous state is certainly intact:

```text
Error
preserve previous state
```

If commit outcome is uncertain:

```text
Critical
restore/restart
```

---

## RS-INT-004 — SnapshotSerializationViolation

Candidate or committed ReadingContextSnapshot violates required serialization contract.

Candidate-only:

```text
Error
discard Candidate
```

Committed state affected:

```text
Critical
```

---

# 22. Runtime Errors

The following are explicitly not Reading Session errors:

```text
RuntimeRevisionSuperseded
AttemptCancelled
AttemptTimedOut
RetryExhausted
WorkItemRejected
SchedulerOverloaded
```

Reading Session may remain:

```text
ACTIVE
+
ReadingContext READY
```

while any of these occur.

---

# 23. Processing Module Errors

Also external:

```text
Capture failure
Recognition failure
Text Processing failure
Translation failure
Presentation failure
```

A processing failure does not automatically cause:

```text
ReadingContextInvalid
ReadingSessionCancelled
```

---

# 24. UI Errors

UI Adapter failures such as:

```text
viewport unavailable
surface destroyed
Presentation apply failed
native window failure
```

remain UI-owned.

A UI interaction may later cause Application to issue a Reading Session domain command, but the UI failure itself is not a Reading Session error.

---

# 25. Storage Errors

Storage implementation failure remains Storage-owned.

Reading Session may receive normalized persistence failure through a persistence port.

If current in-memory domain state remains valid, Storage failure does not become Reading Session invariant corruption.

---

# 26. Error-to-State Mapping

| Condition                       | Reading Session Result                   |
| ------------------------------- | ---------------------------------------- |
| Invalid command                 | Preserve current state                   |
| Invalid target/source           | Preserve current state                   |
| ReadingContextRevision conflict | Preserve current state                   |
| Candidate context invalid       | Preserve current context                 |
| Already active/paused           | No-op                                    |
| Runtime failure                 | No automatic domain transition           |
| Processing failure              | No automatic domain transition           |
| UI failure                      | No automatic domain transition           |
| Event publication failure       | Committed domain state remains committed |
| Restored context invalid        | Do not expose as `READY`                 |
| Domain invariant corruption     | Recovery/restart required                |

---

# 27. ReadingContextInvalid vs Processing Failure

Use:

```text
ReadingContextInvalid
```

only when Reading Session cannot trust its understanding of:

```text
source
target
position
configuration
domain identity
```

Do not use it merely because the system failed to translate or recognize the content.

---

# 28. SessionCancelled vs RuntimeCancelled

`ReadingSessionCancelled` means:

```text
reading activity terminated
```

Runtime cancellation means:

```text
execution work stopped or lost authority
```

They may be correlated.

They are not the same state or error.

---

# 29. No-Op Conditions

Prefer no-op over error where possible.

Examples:

```text
Activate already ACTIVE session
Pause already PAUSED session
apply equivalent configuration
update to same ReadingTarget
update to same ReadingPosition
```

No-op:

* does not increment ReadingContextRevision;
* does not publish context mutation success event;
* may emit diagnostics at Debug/Info level.

---

# 30. Candidate Supersession

If a newer Reading Session command wins before an older Candidate commits:

```text
older Candidate → discard
```

This is expected optimistic concurrency behavior.

Do not classify as Critical failure.

Usually:

```text
RS-REV-001
```

or internal supersession diagnostics are sufficient.

---

# 31. Event Publication Failure Is Not Domain Failure

Example:

```text
Revision 18 committed
    ↓
ReadingContextChanged publication fails
```

Current domain state remains:

```text
Revision 18
```

Observability should distinguish:

```text
domain commit success
event publication failure
```

---

# 32. Error Logging

Recommended structured fields:

```text
errorCode
category
severity
recoveryHint
readingSessionId
lifecycleState
readingContextRevision
expectedReadingContextRevision
sourceId?
targetId?
requestId?
correlationId?
traceId?
occurredAt
```

---

# 33. Privacy

Normal error payloads and logs MUST NOT contain:

```text
screenshot
full source text
translated text
raw HTML
provider prompt
provider response
auth token
secret
cookie
native handle
```

Use:

```text
opaque IDs
state
revision
reason code
bounded metadata
```

instead.

---

# 34. Logging Levels

Suggested:

```text
Debug
    no-op / low-level diagnostic

Info
    expected revision conflict or duplicate condition

Warning
    invalid domain request

Error
    recoverable Reading Session operation failure

Critical
    Reading Session-owned correctness compromised
```

---

# 35. Metrics

Recommended:

```text
reading_session_error_total
reading_session_rejection_total
reading_context_invalid_total
reading_context_revision_conflict_total
reading_session_invalid_transition_total
reading_session_recovery_total
reading_session_event_publish_failure_total
reading_session_consistency_failure_total
```

Remove old metrics such as:

```text
content_revision_superseded_total
processing_intent_created_total
processing_intent_obsolete_total
```

from Reading Session ownership.

---

# 36. Testing — Ownership

Tests MUST verify Reading Session never returns internal error codes for:

```text
Runtime timeout
Attempt cancellation
Translation provider failure
OCR failure
Presentation failure
UI apply failure
Scheduler overload
```

---

# 37. Testing — Candidate Isolation

Tests MUST verify:

* invalid Candidate does not mutate current ReadingContext;
* revision conflict does not mutate current context;
* source/target validation failure does not increment revision;
* configuration rejection preserves previous committed state.

---

# 38. Testing — Revision

Tests MUST verify:

```text
successful context commit
    → revision increments

no-op
    → revision unchanged

failed Candidate
    → revision unchanged

revision conflict
    → revision unchanged
```

---

# 39. Testing — Lifecycle

Tests MUST distinguish:

```text
invalid requested lifecycle transition
```

from:

```text
committed lifecycle corruption
```

The former is normally `Warning`.

The latter is `Critical`.

---

# 40. Testing — Runtime Independence

Verify:

```text
Runtime work fails
```

while:

```text
ReadingSession = ACTIVE
ReadingContext = READY
```

remains valid.

---

# 41. Testing — Event Publication

Tests MUST verify:

```text
domain commit succeeds
event publication fails
```

does not:

* roll back domain state automatically;
* rerun the command;
* create another revision;
* corrupt ReadingSession.

---

# 42. Testing — Recovery

Tests should cover:

* invalid restored Session;
* invalid restored Context;
* inconsistent restored Revision;
* successful known-good restoration;
* missing known-good state;
* no activation before restored data validation.

---

# 43. Deprecated v2 Errors

The following v2 errors are removed or replaced.

## Content Revision

```text
SES-REVISION-001 RevisionNotFound
SES-REVISION-002 RevisionAlreadyCurrent
SES-REVISION-003 RevisionSuperseded
SES-REVISION-004 RevisionArchived
SES-REVISION-005 RevisionDiscarded
SES-REVISION-006 DuplicateRevision
```

Replacement active semantics:

```text
RS-REV-001 ReadingContextRevisionConflict
RS-REV-002 ReadingContextRevisionMissing
RS-REV-003 ReadingContextRevisionInvalid
RS-REV-004 NonMonotonicReadingContextRevision
RS-REV-005 DuplicateSemanticRevision
```

---

## Processing Intent

All:

```text
SES-INTENT-*
```

are removed from Reading Session ownership.

---

# 44. Error Code Summary

## Validation

```text
RS-VAL-001 MissingReadingSessionId
RS-VAL-002 InvalidReadingSessionId
RS-VAL-003 MissingRequiredField
RS-VAL-004 InvalidFieldValue
RS-VAL-005 UnsupportedContractVersion
```

## Session

```text
RS-SES-001 SessionNotFound
RS-SES-002 SessionAlreadyExists
RS-SES-003 SessionAlreadyActive
RS-SES-004 SessionAlreadyPaused
RS-SES-005 SessionCompleted
RS-SES-006 SessionCancelled
RS-SES-007 SessionDisposed
RS-SES-008 InvalidSessionTransition
```

## Context

```text
RS-CTX-001 ReadingContextUnavailable
RS-CTX-002 ReadingContextInvalid
RS-CTX-003 ReadingContextDisposed
RS-CTX-004 ReadingContextSessionMismatch
RS-CTX-005 InvalidReadingSource
RS-CTX-006 InvalidReadingTarget
RS-CTX-007 InvalidReadingPosition
RS-CTX-008 CandidateContextInvalid
```

## ReadingContextRevision

```text
RS-REV-001 ReadingContextRevisionConflict
RS-REV-002 ReadingContextRevisionMissing
RS-REV-003 ReadingContextRevisionInvalid
RS-REV-004 NonMonotonicReadingContextRevision
RS-REV-005 DuplicateSemanticRevision
```

## Configuration

```text
RS-CFG-001 UnsupportedLanguage
RS-CFG-002 UnsupportedReadingMode
RS-CFG-003 InvalidConfigurationCombination
RS-CFG-004 ConfigurationVersionConflict
```

## Consistency

```text
RS-CON-001 MultipleCurrentContexts
RS-CON-002 ContextRevisionMismatch
RS-CON-003 CommittedInvalidLifecycleTransition
RS-CON-004 DomainHistoryCorrupted
RS-CON-005 AggregateIdentityConflict
```

## Recovery

```text
RS-REC-001 RestoredSessionInvalid
RS-REC-002 RestoredContextInvalid
RS-REC-003 RestoredRevisionInvalid
RS-REC-004 KnownGoodStateUnavailable
```

## Publication

```text
RS-PUB-001 EventSerializationFailed
RS-PUB-002 EventPublicationFailed
RS-PUB-003 EventPayloadContractViolation
```

## Internal

```text
RS-INT-001 UnexpectedInternalFailure
RS-INT-002 InvariantViolation
RS-INT-003 AtomicDomainCommitFailed
RS-INT-004 SnapshotSerializationViolation
```

---

# 45. Architecture Invariants

1. Reading Session errors describe Reading Session-owned failures only.

2. Runtime failure is not a Reading Session error.

3. Runtime cancellation is not a Reading Session error.

4. Processing module failure is not a Reading Session error.

5. UI apply failure is not a Reading Session error.

6. Reading Session owns ReadingContextRevision errors only.

7. RuntimeRevisionId is never treated as ReadingContextRevision.

8. ContentRevision lifecycle errors are removed.

9. ProcessingIntent errors are removed.

10. Validation errors never partially mutate domain state.

11. Candidate rejection never mutates committed ReadingContext.

12. ReadingContextRevision conflict is expected concurrency behavior.

13. No-op does not advance ReadingContextRevision.

14. Processing failure alone does not invalidate ReadingContext.

15. ReadingSessionCancelled is not Runtime Attempt cancellation.

16. Invalid requested lifecycle transition is not automatically Critical.

17. Committed lifecycle corruption is Critical.

18. Event publication failure does not roll back valid committed domain state.

19. Event publication failure does not rerun the domain command.

20. Atomic commit failure preserves previous state when certainty exists.

21. Reading Session recovery restores domain state only.

22. Error payloads remain immutable.

23. Error codes remain stable.

24. Normal diagnostics contain no raw reading content.

---

# 46. Related Documents

```text
doc/02-modules/reading-session/MODULE.md
doc/02-modules/reading-session/CONTRACT.md
doc/02-modules/reading-session/STATES.md
doc/02-modules/reading-session/EVENTS.md
doc/02-modules/reading-session/README.md

doc/01-architecture/core/STATE_MACHINE.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
```

---

# 47. Completion Criteria

This error specification is synchronized when:

* only Reading Session-owned failure categories remain;
* ProcessingIntent errors are removed;
* old ContentRevision lifecycle errors are removed;
* ReadingContextRevisionConflict replaces ambiguous stale-revision semantics;
* Runtime staleness remains external;
* processing failures remain external;
* UI failures remain external;
* validation/candidate failures preserve current committed state;
* event publication failure preserves committed domain state;
* consistency errors distinguish rejected illegal requests from actual state corruption;
* recovery restores reading-domain state only;
* diagnostics and metrics remain privacy-safe.

---

# 48. Summary

Reading Session v3 error flow is:

```text
Reading Session Command
        ↓
Domain Validation
        ├── rejected
        │      ↓
        │   Reading Session error
        │   committed state unchanged
        │
        └── accepted
               ↓
        Candidate Reading Context
               ↓
        candidate validation
               ↓
        ReadingContextRevision guard
               ↓
        atomic domain commit
               ↓
        committed Reading Session state
               ↓
        event publication
               ├── success
               └── publication failure
                    domain state remains committed
```

External execution remains separate:

```text
Reading Session
    → reading-domain errors

Business Pipeline Orchestration
    → pipeline-decision errors

Runtime
    → execution/authority errors

Processing Modules
    → processing errors

Presentation
    → presentation errors

UI Adapter
    → rendering/apply errors
```

The central rule is:

```text
Reading Session owns errors
about the reading world.

It does not own errors
about executing work for that world.
```
