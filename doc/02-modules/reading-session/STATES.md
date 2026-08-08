# Reading Session States

> **Project:** CRAI
> **Module:** `reading-session`
> **Path:** `doc/02-modules/reading-session/STATES.md`
> **Version:** 3.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-08

---

# 1. Purpose

This document defines the Reading Session-owned state model.

It specifies:

```text
Reading Session lifecycle
Reading Context lifecycle
ReadingContextRevision behavior
candidate context behavior
state transitions
transition guards
terminal states
recovery
persistence semantics
concurrency
state invariants
```

This state specification describes reading-domain state only.

It does not define:

```text
Runtime Revision state
WorkItem state
Attempt state
Runtime cancellation state
Runtime retry state
ProcessingIntent state
Artifact state
Presentation state
UI surface state
```

Those concepts belong to their respective owners.

---

# 2. State Ownership

Reading Session owns:

```text
ReadingSessionState
ReadingContextState
Current ReadingContext reference
ReadingContextRevision
Candidate ReadingContext
```

Reading Session does not own:

```text
RuntimeRevisionState
WorkItemState
AttemptState
ProcessingIntentState
ArtifactPublicationState
PresentationState
ViewportLifecycleState
```

---

# 3. State Model Overview

Reading Session v3 contains two primary state machines.

```text
Reading Session
├── Session Lifecycle
└── Reading Context Lifecycle
```

`ReadingContextRevision` is not a separate lifecycle state machine.

It is an immutable version identifier attached to committed ReadingContext snapshots.

---

# 4. Why Only Two State Machines

The previous model separated:

```text
Session Lifecycle
Reading Context
Content Revision
Processing Intent
```

That design overloaded Reading Session with orchestration and execution-authority concepts.

Runtime v2 separates those responsibilities.

The new model is:

```text
Session Lifecycle
    → whether the reading activity exists and is usable

Reading Context Lifecycle
    → whether trustworthy reading-domain context currently exists

ReadingContextRevision
    → which committed version of that context is current
```

Pipeline planning belongs elsewhere.

---

# 5. Core State Principle

Reading Session state answers:

> What is the current business state of this reading activity?

It does not answer:

> Which processing work is currently valid?

Runtime Control owns execution authority.

---

# 6. Session Lifecycle States

```text
ReadingSessionState

CREATED
INITIALIZING
ACTIVE
PAUSED
COMPLETING
COMPLETED
CANCELLED
DISPOSED
```

These states describe the reading activity itself.

---

# 7. `CREATED`

## Meaning

The Reading Session aggregate exists, but active reading has not yet begun.

Typical properties:

```text
ReadingSessionId exists
initial configuration accepted
initial source may exist
context may not yet be committed
```

## Allowed Next States

```text
INITIALIZING
DISPOSED
```

An implementation MAY collapse `CREATED → INITIALIZING` internally for MVP.

---

# 8. `INITIALIZING`

## Meaning

Reading Session is establishing its initial valid business state.

Possible work includes:

```text
validate ReadingSource
validate initial ReadingTarget
resolve session configuration
build initial ReadingContext
create initial ReadingContextRevision
```

These are Reading Session-owned domain operations.

They do not include:

```text
Capture
OCR
Translation
Runtime scheduling
```

## Allowed Next States

```text
ACTIVE
CANCELLED
DISPOSED
```

---

# 9. `ACTIVE`

## Meaning

The reading activity is currently active and may accept reading-domain mutations.

Typical operations:

```text
UpdateReadingTarget
ReplaceReadingSource
UpdateReadingPosition
UpdateSessionConfiguration
PauseReadingSession
CompleteReadingSession
CancelReadingSession
```

## Characteristics

* current ReadingContext may exist;
* new ReadingContext revisions may be committed;
* domain facts may be published;
* processing failure does not automatically leave `ACTIVE`.

## Allowed Next States

```text
PAUSED
COMPLETING
CANCELLED
```

---

# 10. `PAUSED`

## Meaning

Reading-domain progression is intentionally suspended.

Pause does not imply:

```text
Runtime work canceled
Presentation cleared
UI hidden
Artifacts disposed
```

Those decisions belong outside Reading Session.

## Default Behavior

While paused:

* ordinary ReadingContext mutations are rejected or deferred according to contract;
* current committed ReadingContext remains readable/queryable;
* ReadingContextRevision does not change merely because the session is paused.

## Allowed Next States

```text
ACTIVE
COMPLETING
CANCELLED
```

---

# 11. `COMPLETING`

## Meaning

The reading activity is transitioning toward a normal terminal state.

Typical causes:

```text
user exits reading mode
document is finished
application closes reader
```

`COMPLETING` exists so implementations may perform Reading Session-owned final domain work before committing `COMPLETED`.

It does not wait for all Runtime processing to finish unless an explicit future business requirement says so.

## Allowed Next States

```text
COMPLETED
CANCELLED
```

---

# 12. `COMPLETED`

## Meaning

The reading activity finished normally.

Characteristics:

```text
no new ReadingContext mutation
no return to ACTIVE
historical state may remain queryable
```

## Allowed Next State

```text
DISPOSED
```

Processing work may still physically exist elsewhere until Runtime handles it.

---

# 13. `CANCELLED`

## Meaning

The reading activity itself was terminated without normal completion.

Possible causes:

```text
explicit user cancellation
source becomes unusable
business-level unrecoverable condition
application aborts reading activity
```

Important:

```text
ReadingSession CANCELLED
≠
Runtime Attempt CANCELLED
```

Reading Session does not perform Runtime cancellation itself.

## Allowed Next State

```text
DISPOSED
```

`CANCELLED` never returns to `ACTIVE`.

---

# 14. `DISPOSED`

## Meaning

Reading Session business state has reached its final lifecycle boundary.

No mutation may succeed.

The state is irreversible.

`DISPOSED` does not imply every external Runtime/UI/Storage resource has already been physically deleted.

Those owners have separate lifecycle rules.

---

# 15. Session Lifecycle Diagram

```text
CREATED
   ↓
INITIALIZING
   ↓
ACTIVE
 ┌─┴───────────────┐
 ↓                 ↓
PAUSED         COMPLETING
 │   ↑             ↓
 └───┘          COMPLETED
                  ↓
               DISPOSED
```

Cancellation path:

```text
INITIALIZING
ACTIVE
PAUSED
COMPLETING
     ↓
 CANCELLED
     ↓
 DISPOSED
```

---

# 16. Valid Session Transitions

```text
CREATED → INITIALIZING
CREATED → DISPOSED

INITIALIZING → ACTIVE
INITIALIZING → CANCELLED
INITIALIZING → DISPOSED

ACTIVE → PAUSED
ACTIVE → COMPLETING
ACTIVE → CANCELLED

PAUSED → ACTIVE
PAUSED → COMPLETING
PAUSED → CANCELLED

COMPLETING → COMPLETED
COMPLETING → CANCELLED

COMPLETED → DISPOSED
CANCELLED → DISPOSED
```

Every other lifecycle transition is invalid unless introduced by a future contract version.

---

# 17. Reading Context Lifecycle

ReadingContext has its own state because the Reading Session may exist while its current reading context is being established or replaced.

States:

```text
ReadingContextState

EMPTY
PREPARING
READY
UPDATING
INVALID
DISPOSED
```

---

# 18. `EMPTY`

## Meaning

No committed ReadingContext currently exists.

Typical situations:

* session newly created;
* context intentionally cleared before replacement;
* restored session not yet validated.

## Allowed Next States

```text
PREPARING
DISPOSED
```

---

# 19. `PREPARING`

## Meaning

Reading Session is preparing the initial Candidate ReadingContext.

Conceptually:

```text
EMPTY
  ↓
CandidateReadingContext
  ↓
validation
  ↓
commit
  ↓
READY
```

Candidate state is not externally authoritative.

## Success

```text
PREPARING → READY
```

## Rejection

If initial context cannot be constructed:

```text
PREPARING → EMPTY
```

or:

```text
PREPARING → INVALID
```

depending on whether the failure means the source/context itself is unusable.

---

# 20. `READY`

## Meaning

A valid committed ReadingContext exists.

Conceptually:

```text
Current ReadingContext
+
Current ReadingContextRevision
```

This is the normal stable context state.

## Characteristics

* exactly one current committed context;
* snapshot immutable;
* revision stable until next commit;
* safe for queries and orchestration.

## Allowed Next States

```text
UPDATING
INVALID
DISPOSED
```

---

# 21. `UPDATING`

## Meaning

Reading Session is preparing a Candidate ReadingContext to replace the current context.

Typical causes:

```text
ReadingTarget changed
ReadingSource replaced
ReadingPosition changed
language changed
session configuration changed
```

## Previous State Preservation

While updating:

```text
current committed ReadingContext N
```

remains authoritative.

Candidate:

```text
ReadingContextRevision N+1
```

remains private until commit.

## Success

```text
UPDATING
   ↓
atomic commit
   ↓
READY
```

## Rejection

If Candidate validation fails but current context remains valid:

```text
UPDATING → READY
```

Current revision remains unchanged.

---

# 22. `INVALID`

## Meaning

Reading Session cannot currently represent a trustworthy ReadingContext.

Possible causes:

```text
ReadingSource invalid
required domain identity inconsistent
restored context corrupt
current committed context becomes semantically unusable
```

`INVALID` does not mean Runtime processing failed.

## Behavior

Normal context mutations may be restricted.

Recovery may:

```text
replace source
rebuild context
clear context
terminate session
```

## Possible Next States

Depending on command and final contract:

```text
PREPARING
DISPOSED
```

A direct `INVALID → READY` transition should occur only through a validated preparation/commit path.

---

# 23. `DISPOSED` Context

The ReadingContext is no longer part of an active Reading Session lifecycle.

Terminal state.

No context mutation is permitted.

---

# 24. Reading Context Diagram

```text
EMPTY
  ↓
PREPARING
  ↓
READY
  ↓
UPDATING
  ↓
READY
```

Invalidation:

```text
PREPARING
READY
UPDATING
    ↓
 INVALID
    ↓
PREPARING
or
DISPOSED
```

Final disposal:

```text
READY / INVALID / EMPTY
        ↓
     DISPOSED
```

---

# 25. ReadingContextRevision Model

`ReadingContextRevision` is not a state machine.

It is an immutable committed version identifier.

Example:

```text
Revision 40
    ↓
context update
    ↓
Candidate Revision 41
    ↓
commit
    ↓
Revision 41 becomes current
```

Revision 40 remains an immutable historical snapshot if retained.

It does not transition into a `Superseded` business state object.

---

# 26. Why Revision Lifecycle Was Removed

The previous model used:

```text
Created
Current
Superseded
Archived
Discarded
```

for each ContentRevision.

Runtime v2 separates three concepts:

```text
Current business context
Historical context retention
Execution authority
```

Reading Session only needs to own the first.

Retention belongs to Reading Session/Storage policy.

Execution supersession belongs to Runtime.

Therefore:

```text
ReadingContextRevision
```

is simply an immutable version of committed domain state.

---

# 27. Current Revision

Reading Session maintains one:

```text
currentReadingContextRevision
```

for a non-empty committed context.

When a new context commits:

```text
current = N
    ↓
commit Candidate N+1
    ↓
current = N+1
```

This does not mutate revision N.

---

# 28. Historical Revisions

Older ReadingContextSnapshots may be:

```text
retained
persisted
evicted
```

according to retention policy.

Those are storage/lifetime concerns.

They do not require public lifecycle states such as:

```text
Archived
Discarded
```

unless future product requirements make those states business-visible.

---

# 29. ReadingContextRevision Authority

ReadingContextRevision is authoritative only for Reading Session domain state.

It does not determine:

```text
whether Runtime work may commit
whether an Artifact is current
whether Presentation may commit
whether UI apply is stale
```

Those domains own their own authority/version mechanisms.

---

# 30. Candidate Reading Context

Conceptually:

```text
CandidateReadingContext
├── readingSessionId
├── basedOnReadingContextRevision?
├── candidateReadingContextRevision
├── readingSource
├── readingTarget?
├── readingPosition?
├── sessionConfiguration
└── changeSet
```

A Candidate is:

* private;
* mutable only while being constructed internally;
* immutable once validation begins or according to implementation policy;
* never externally current before commit.

---

# 31. Candidate Isolation

During `UPDATING`:

```text
Committed Context N
+
Candidate Context N+1
```

may coexist.

External queries continue returning:

```text
Committed Context N
```

until commit succeeds.

---

# 32. Atomic Context Commit

A successful context mutation atomically replaces:

```text
current ReadingContextSnapshot
+
current ReadingContextRevision
+
current context reference
```

Partial visibility is forbidden.

---

# 33. ReadingContextRevision Guard

Commands targeting current context SHOULD carry:

```text
expectedReadingContextRevision
```

Guard:

```text
expectedReadingContextRevision
==
currentReadingContextRevision
```

If false:

```text
ReadingContextRevisionConflict
```

Candidate must not commit.

---

# 34. Revision Conflict Is Not Runtime Staleness

Example:

```text
Reading Context current = 12

Command A expects 12
Command B expects 12

B commits 13

A reaches commit
    ↓
revision conflict
```

This is Reading Session optimistic concurrency.

It says nothing about RuntimeRevisionId or Runtime Attempts.

---

# 35. No-Op Rule

A semantically equivalent domain mutation should not create another ReadingContextRevision.

Examples:

```text
same target
same position
same target language
same source identity
same effective session configuration
```

Result:

```text
NO_OP
```

Current revision remains unchanged.

---

# 36. High-Frequency Position Changes

Reading Session should not create revisions for every raw scroll event.

Expected flow:

```text
Raw UI movement
    ↓
UI/Application normalization
    ↓
coalesce
    ↓
business-significant ReadingPosition/Target change
    ↓
Reading Session mutation
```

Presentation-only layout changes may not touch Reading Session at all.

---

# 37. Session State and Context State Independence

Session state and context state are related but not identical.

Examples:

```text
Session = ACTIVE
Context = READY
```

normal operation.

```text
Session = ACTIVE
Context = UPDATING
```

context mutation in progress.

```text
Session = PAUSED
Context = READY
```

paused activity retaining valid context.

```text
Session = INITIALIZING
Context = PREPARING
```

initial setup.

---

# 38. Lifecycle Compatibility Matrix

Typical valid combinations:

| Session        | Context                          |
| -------------- | -------------------------------- |
| `CREATED`      | `EMPTY`                          |
| `INITIALIZING` | `EMPTY` / `PREPARING`            |
| `ACTIVE`       | `READY` / `UPDATING` / `INVALID` |
| `PAUSED`       | `READY` / `INVALID`              |
| `COMPLETING`   | `READY` / `INVALID`              |
| `COMPLETED`    | `READY` / `INVALID` / `DISPOSED` |
| `CANCELLED`    | `READY` / `INVALID` / `DISPOSED` |
| `DISPOSED`     | `DISPOSED`                       |

Exact implementation may simplify transitional combinations while preserving ownership.

---

# 39. Pause Semantics

Pause changes Reading Session lifecycle.

It does not automatically:

```text
create ReadingContextRevision
invalidate ReadingContext
cancel Runtime work
mark Runtime Revision obsolete
clear Presentation
```

Application/Runtime may separately react according to policy.

---

# 40. Resume Semantics

Resume changes:

```text
PAUSED → ACTIVE
```

It does not automatically create a new ReadingContextRevision.

If domain state changed while paused, an explicit context update creates the revision.

---

# 41. Completion Semantics

Completion changes Reading Session lifecycle.

It does not automatically wait for:

```text
OCR completion
Translation completion
Presentation commit
event delivery
UI cleanup
```

Those are independent lifecycles.

---

# 42. Cancellation Semantics

`CancelReadingSession` means:

```text
cancel the reading activity
```

not:

```text
cancel a Runtime Attempt
```

After Reading Session commits `CANCELLED`, Application/Runtime may derive appropriate execution cancellation.

Reading Session does not mutate Runtime state.

---

# 43. Runtime Supersession

A ReadingContext change may make existing Runtime work less relevant.

But the state flow is:

```text
Reading Session commits Revision N+1
        ↓
domain fact/state available
        ↓
Business Pipeline Orchestration
        ↓
Runtime establishes newer execution authority
        ↓
Runtime supersedes old work
```

Reading Session does not perform:

```text
oldRuntimeRevision.state = Superseded
```

---

# 44. ProcessingIntent State Removed

Reading Session v3 does not own:

```text
ProcessingIntentState
```

Therefore the following states are removed:

```text
Created
Published
Accepted
Fulfilled
Obsolete
Discarded
```

as Reading Session concepts.

Pipeline requirement decisions belong to Business Pipeline Orchestration.

Execution lifecycle belongs to Runtime.

---

# 45. Why ProcessingIntent Lifecycle Was Removed

A state such as:

```text
ProcessingIntent Accepted
```

implicitly requires Reading Session to know Runtime accepted work.

A state such as:

```text
ProcessingIntent Fulfilled
```

requires Reading Session to evaluate whether pipeline execution fulfilled an objective.

Those responsibilities duplicate Runtime and Business Pipeline Orchestration.

Reading Session should remain unaware of processing topology.

---

# 46. State Transition Guards

Every Reading Session-owned transition must use explicit guards.

---

# 47. Session Guards

Examples:

```text
ActivateReadingSession
requires
state = CREATED or INITIALIZING according to flow
```

```text
PauseReadingSession
requires
state = ACTIVE
```

```text
ResumeReadingSession
requires
state = PAUSED
```

```text
CompleteReadingSession
requires
state ∈ {ACTIVE, PAUSED}
```

```text
DisposeReadingSession
requires
state ∈ {CREATED, COMPLETED, CANCELLED}
```

Exact command/state rules must match `CONTRACT.md`.

---

# 48. Context Guards

Context mutation typically requires:

```text
valid ReadingSession
allowed Session lifecycle
valid ReadingSource
valid ReadingTarget if supplied
valid SessionConfiguration
expectedReadingContextRevision match
candidate invariant validation
```

Runtime execution state is not a Reading Context guard.

---

# 49. ReadingContextRevision Guards

A new revision may be created only if committed reading-domain state changes.

Examples:

```text
source changed
target changed
position changed meaningfully
language changed
session-specific configuration changed
```

Raw technical changes do not automatically qualify.

---

# 50. Transition Triggers

Reading Session transitions should originate from explicit domain commands.

Examples:

```text
CreateReadingSession
ActivateReadingSession
UpdateReadingTarget
ReplaceReadingSource
UpdateReadingPosition
UpdateSessionConfiguration
PauseReadingSession
ResumeReadingSession
CompleteReadingSession
CancelReadingSession
DisposeReadingSession
```

---

# 51. Events Are Not Direct State Mutators

External events such as:

```text
BrowserNavigated
ViewportChanged
PreferenceChanged
```

should normally be translated by Application/Adapters into domain commands.

Reading Session does not require direct Event Bus subscriptions for correctness.

---

# 52. Transition Actions

Reading Session-owned transition actions may include:

```text
validate domain state
build Candidate ReadingContext
commit ReadingContext
increment ReadingContextRevision
update lifecycle state
update session metadata
publish reading-domain fact
```

They must not include:

```text
create WorkItem
cancel Attempt
retry Translation
start OCR
rebuild Presentation
```

---

# 53. Domain Event Timing

Reading-domain success facts publish only after state commit.

Correct:

```text
validate
    ↓
commit state
    ↓
transition complete
    ↓
publish fact
```

Incorrect:

```text
publish success fact
    ↓
attempt state mutation
```

---

# 54. Context Failure Behavior

If a Candidate context fails validation:

```text
discard Candidate
```

If previous context remains valid:

```text
return to READY
```

Do not invalidate a valid current context merely because a replacement failed.

---

# 55. `INVALID` Entry Rules

Enter `INVALID` only when current ReadingContext itself cannot be trusted or represented.

Examples:

* current source identity becomes invalid;
* restored current context is corrupt;
* committed domain invariant is violated.

Do not enter `INVALID` because:

```text
OCR failed
Translation failed
Runtime timed out
Presentation failed
```

---

# 56. Session Failure Model

Reading Session does not require a generic `FAILED` lifecycle state in v3.

Reading-domain failures are normally:

```text
command rejection
context INVALID
session CANCELLED
```

A future `FAILED` session state should only be introduced if there is a distinct business meaning not already represented by `INVALID` or `CANCELLED`.

---

# 57. Persistence

Potentially persistable domain state includes:

```text
ReadingSession
ReadingContextSnapshot
ReadingContextRevision
SessionConfiguration
ReadingPosition
ReadingMetadata
```

Persistence implementation is external.

---

# 58. Runtime State Is Never Persisted Here

Reading Session persistence must not contain:

```text
RuntimeRevisionId as owned state
WorkItem state
Attempt state
retry counter
scheduler queue
worker assignment
provider connection state
```

Runtime may separately persist what its architecture requires.

---

# 59. ReadingSessionSnapshot

For persistence/query purposes, a consistent aggregate snapshot may be:

```text
ReadingSessionSnapshot
├── readingSessionId
├── lifecycleState
├── readingContextState
├── currentReadingContextRevision?
├── currentReadingContextSnapshot?
├── sessionConfiguration
├── readingMetadata?
└── capturedAt
```

It does not contain ProcessingIntent.

---

# 60. Recovery

Recovery reconstructs Reading Session business state.

It does not reconstruct Runtime execution.

Conceptually:

```text
persisted session state
    ↓
load
    ↓
validate
    ↓
Candidate restored session
    ↓
commit
```

---

# 61. Recovery Authority

Recovery restores:

```text
Reading Session business authority
```

meaning:

```text
which ReadingContext is now current
```

It does not restore:

```text
Runtime execution authority
```

Runtime determines execution after recovery.

---

# 62. Revision Recovery

Recovery may select one validated ReadingContextSnapshot as current according to persisted domain ordering.

The selected revision does not need a lifecycle transition such as:

```text
Archived → Current
```

because old revisions do not have public lifecycle states.

Instead:

```text
validate retained snapshots
    ↓
select valid current domain snapshot
    ↓
restore currentReadingContextRevision
```

---

# 63. Invalid Restored Context

If persisted ReadingContext cannot be validated:

```text
do not expose as READY
```

Possible recovery outcomes:

```text
context = INVALID
rebuild through PREPARING
cancel session
dispose session
```

according to business policy.

---

# 64. Invalid Session Transitions

Examples:

```text
COMPLETED → ACTIVE
CANCELLED → ACTIVE
DISPOSED → ACTIVE
DISPOSED → PAUSED
PAUSED → CREATED
```

Invalid transition handling:

1. reject command;
2. preserve current state;
3. record diagnostics;
4. emit no success fact.

---

# 65. Invalid Context Transitions

Examples:

```text
DISPOSED → READY
DISPOSED → UPDATING
EMPTY → READY without commit path
INVALID → READY without validated preparation/commit
```

---

# 66. State Invariants — Session

1. Every ReadingSession has exactly one lifecycle state.

2. `COMPLETED` never becomes `ACTIVE`.

3. `CANCELLED` never becomes `ACTIVE`.

4. `DISPOSED` never changes.

5. Processing failures do not automatically change ReadingSessionState.

6. Runtime state never directly mutates ReadingSessionState.

---

# 67. State Invariants — Context

1. Every non-disposed ReadingSession owns at most one current committed ReadingContext.

2. A committed ReadingContext is immutable.

3. Candidate context is never externally current.

4. Current context remains visible during safe update preparation.

5. Failed Candidate does not mutate current context.

6. `INVALID` means Reading Session cannot trust current reading context.

7. Processing failure alone does not make ReadingContext invalid.

---

# 68. State Invariants — Revision

1. Every ReadingContextRevision belongs to exactly one ReadingSession.

2. ReadingContextRevision is immutable.

3. ReadingContextRevision is monotonic.

4. Only successful domain commit advances it.

5. No-op does not advance it.

6. Candidate rejection does not advance it.

7. ReadingContextRevision is not Runtime execution authority.

8. Older revisions do not require `Superseded` state.

9. Historical retention does not change revision semantics.

---

# 69. Ownership Invariants

1. Reading Session owns reading lifecycle.

2. Reading Session owns ReadingContext lifecycle.

3. Reading Session owns ReadingContextRevision.

4. Reading Session does not own ProcessingIntent lifecycle.

5. Reading Session does not own Runtime Revision lifecycle.

6. Reading Session does not own WorkItem lifecycle.

7. Reading Session does not own Attempt lifecycle.

8. Reading Session does not own Artifact lifecycle.

9. Reading Session does not own Presentation lifecycle.

10. Reading Session does not own UI lifecycle.

---

# 70. MVP Session States

MVP may publicly expose:

```text
CREATED
ACTIVE
PAUSED
COMPLETED
CANCELLED
DISPOSED
```

while treating:

```text
INITIALIZING
COMPLETING
```

as internal transitional states.

The architecture remains compatible with the full model.

---

# 71. MVP Context States

Recommended MVP:

```text
EMPTY
PREPARING
READY
UPDATING
INVALID
DISPOSED
```

These states are sufficiently small while preserving Candidate isolation.

---

# 72. No MVP Revision State Machine

MVP must not reintroduce:

```text
Current
Superseded
Archived
Discarded
```

as ReadingContextRevision lifecycle states.

Instead expose:

```text
currentReadingContextRevision
```

plus optional historical snapshot retention.

---

# 73. No MVP ProcessingIntent State

MVP must not implement Reading Session-owned:

```text
ProcessingIntent
```

or its lifecycle.

Business Pipeline Orchestration owns processing requirement evaluation.

---

# 74. Testing — Session Lifecycle

Tests must cover:

```text
create
initialize
activate
pause
resume
complete
cancel
dispose
invalid transitions
terminal-state irreversibility
```

---

# 75. Testing — Context Lifecycle

Tests must cover:

```text
EMPTY → PREPARING → READY
READY → UPDATING → READY
candidate rejection → previous READY retained
invalid current context → INVALID
context rebuild
context disposal
```

---

# 76. Testing — Revision

Tests must verify:

```text
initial revision
successful increment
no-op does not increment
failed Candidate does not increment
revision conflict rejects mutation
revision monotonicity
historical snapshots remain immutable
```

---

# 77. Testing — Ownership

Tests must verify Reading Session never:

```text
creates RuntimeRevisionId
marks Runtime work superseded
mutates WorkItem
mutates Attempt
publishes ProcessingIntent
marks ProcessingIntent fulfilled
starts processing modules
waits for worker completion to change domain state
```

---

# 78. Testing — Pause and Cancellation

Verify:

```text
PauseReadingSession
```

does not mutate Runtime execution state.

Verify:

```text
CancelReadingSession
```

changes reading lifecycle only.

Runtime cancellation behavior must be tested in Runtime integration tests.

---

# 79. Testing — Processing Failure Independence

Examples:

```text
OCR failure
Translation failure
Presentation rejection
UI apply failure
```

must not automatically transition:

```text
ACTIVE → CANCELLED
```

or:

```text
READY Context → INVALID
```

---

# 80. Testing — Concurrency

Test:

* two context mutations with same expected revision;
* target change racing configuration change;
* pause racing context update;
* cancel racing context update;
* completion racing context update;
* stale command after new revision;
* equivalent duplicate command.

---

# 81. Open Decisions

The following may remain open:

* whether `INITIALIZING` is public;
* whether `COMPLETING` is public;
* whether completed sessions are restorable;
* historical ReadingContext retention duration;
* whether PAUSED allows selected context metadata updates;
* whether `INVALID → PREPARING` is direct or requires explicit reset;
* whether multiple concurrent ReadingSessions ship in MVP.

These decisions do not change ownership.

---

# 82. Removed v2 Concepts

The following are removed from Reading Session v3 state ownership:

```text
ContentRevisionState
├── Created
├── Current
├── Superseded
├── Archived
└── Discarded

ProcessingIntentState
├── Created
├── Published
├── Accepted
├── Fulfilled
├── Obsolete
└── Discarded
```

Replacement model:

```text
ReadingContextRevision
    → immutable committed version

Business Pipeline Orchestration
    → processing requirements

Runtime
    → execution lifecycle and authority
```

---

# 83. Related Documents

```text
doc/02-modules/reading-session/MODULE.md
doc/02-modules/reading-session/CONTRACT.md
doc/02-modules/reading-session/EVENTS.md
doc/02-modules/reading-session/ERRORS.md
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

# 84. Completion Criteria

This specification is synchronized when:

* Reading Session has only Reading Session-owned state machines;
* ProcessingIntent lifecycle has been removed;
* ContentRevision lifecycle has been removed;
* ReadingContextRevision is an immutable committed version;
* Runtime authority is absent from Reading Session state ownership;
* lifecycle cancellation is distinct from Runtime cancellation;
* lifecycle completion is distinct from Runtime completion;
* processing failure does not mutate Reading Session state automatically;
* Candidate ReadingContext is isolated;
* previous committed context survives failed replacement;
* optimistic concurrency uses ReadingContextRevision;
* raw viewport noise does not create unnecessary revisions;
* persistence restores business state only;
* tests verify state ownership and Runtime independence.

---

# 85. Summary

Reading Session v3 state consists of:

```text
ReadingSessionState
+
ReadingContextState
+
ReadingContextRevision
```

The lifecycle model is:

```text
ReadingSession
CREATED
   ↓
INITIALIZING
   ↓
ACTIVE
  ↕
PAUSED
   ↓
COMPLETING
   ↓
COMPLETED
   ↓
DISPOSED
```

with:

```text
CANCELLED
```

as a business terminal path.

Reading Context follows:

```text
EMPTY
   ↓
PREPARING
   ↓
READY
   ↕
UPDATING
   ↓
INVALID
   ↓
DISPOSED
```

And revision semantics are:

```text
Committed Context Revision N
        ↓
Candidate N+1
        ↓
domain validation
        ↓
commit
        ↓
Current Revision N+1
```

The critical rule is:

```text
Reading Session states describe
the reading activity.

ReadingContextRevision describes
the version of reading-domain state.

Runtime states describe
execution.

These lifecycles must remain separate.
```
