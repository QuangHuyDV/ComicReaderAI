# Reading Session States

- Module: Reading Session
- Identifier: reading-session
- Layer: Business Orchestration
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

This document defines the complete state model of the Reading Session Module.

Unlike MODULE.md, which defines architectural responsibilities, and CONTRACT.md, which defines public interfaces, this document specifies how business state evolves during the lifetime of a reading activity.

This specification defines:

- Session lifecycle states
- Reading Context states
- Content Revision states
- Processing Intent states
- valid transitions
- transition guards
- transition triggers
- terminal states
- persistence behavior
- recovery behavior
- state ownership
- architectural invariants

This document intentionally excludes execution state.

Execution belongs to Runtime Architecture.

---

# 2. State Ownership

Reading Session owns only business state.

It never owns runtime state.

---

## 2.1 Reading Session Owns

```text
ReadingSessionState

ReadingContextState

ContentRevisionState

ProcessingIntentState
```

---

## 2.2 Reading Session Does Not Own

```text
Worker State

Scheduler State

Execution Queue State

Capture State

Recognition State

Translation State

Presentation State

Provider State

Retry State

Cache State
```

Those states belong to their respective modules.

Reading Session may react to them through events or contracts but never becomes their source of truth.

---

# 3. State Model Overview

The Reading Session state model consists of four independent state machines.

```text
Reading Session

├── Session Lifecycle
│
├── Reading Context
│
├── Content Revision
│
└── Processing Intent
```

Each state machine owns one business concept.

Each evolves independently while remaining consistent with the others.

---

## 3.1 Why Multiple State Machines

Reading is not represented by a single lifecycle.

For example,

the session may remain Active while:

- Reading Context changes

or

- several Content Revisions become obsolete

or

- multiple Processing Intents are published.

Representing all of these using one giant state machine would create unnecessary coupling.

Therefore each concept owns its own lifecycle.

---

# 4. State Machine Principles

---

## 4.1 One Owner Per State

Every state belongs to exactly one business concept.

No state may belong to multiple concepts.

---

## 4.2 Explicit Transitions

Every transition must be documented.

Business state may never change implicitly.

---

## 4.3 Immutable History

Previous state transitions remain historically valid.

Business history is never rewritten.

---

## 4.4 Deterministic Evolution

Given the same:

- session
- context
- revision
- configuration

Reading Session must always produce identical state transitions.

---

## 4.5 Runtime Independence

Business states never describe execution.

Examples of invalid business states:

```text
OCRRunning

TranslationQueued

WorkerBusy

GPUUnavailable
```

Those belong to Runtime.

---

## 4.6 Independent Lifecycles

Each state machine progresses independently.

For example,

Session may remain Active while Context changes multiple times.

Likewise,

ContentRevision may become Superseded without affecting Session state.

---

# 5. Session Lifecycle State

The Session Lifecycle describes the lifetime of a reading activity.

It represents the highest-level business state inside the module.

---

## 5.1 Lifecycle States

```text
ReadingSessionState

├── Created
├── Initializing
├── Active
├── Paused
├── Completing
├── Completed
├── Cancelled
└── Disposed
```

---

## 5.2 Created

A Reading Session has been created.

Business identity exists.

No business evaluation has begun.

Characteristics:

- SessionId assigned
- configuration accepted
- context not initialized
- no ProcessingIntent published

Allowed next states:

```text
Initializing

Disposed
```

---

## 5.3 Initializing

Reading Session prepares business state.

Possible activities:

- loading source metadata
- building ReadingContext
- validating configuration
- creating initial ContentRevision

Allowed next states:

```text
Active

Cancelled

Disposed
```

---

## 5.4 Active

The session is actively representing a reading activity.

Characteristics:

- accepts business updates
- evaluates ReadingContext
- produces ContentRevision
- publishes ProcessingIntent

This is the normal operating state.

Allowed next states:

```text
Paused

Completing

Cancelled
```

---

## 5.5 Paused

Business progression is temporarily suspended.

Characteristics:

- no new ProcessingIntent
- no new ContentRevision
- context remains unchanged

Runtime execution may continue independently.

Allowed next states:

```text
Active

Cancelled

Completing
```

---

## 5.6 Completing

The reading activity is naturally ending.

Examples:

- browser tab closed
- user exits reading
- source finished

The module prepares for completion.

Allowed next states:

```text
Completed
```

---

## 5.7 Completed

The reading activity finished normally.

Characteristics:

- immutable
- no new context
- no new revision
- no new intent

Allowed next state:

```text
Disposed
```

---

## 5.8 Cancelled

Business activity terminated unexpectedly.

Examples:

- user cancelled
- source removed
- unrecoverable business failure

Cancelled sessions cannot become Active again.

Allowed next state:

```text
Disposed
```

---

## 5.9 Disposed

All business resources have been released.

Terminal state.

No transition is allowed.

---

# 6. Session Lifecycle Diagram

```text
Created
    ↓
Initializing
    ↓
Active
 ┌──┴─────┐
 │        │
 ↓        ↓
Paused  Completing
 │        │
 └──↓─────┘
   Active
      ↓
Completed
      ↓
Disposed
```

Cancellation:

```text
Created
Initializing
Active
Paused
Completing

↓

Cancelled

↓

Disposed
```

---

# 7. Session Transition Rules

The following transitions are valid.

```text
Created → Initializing

Initializing → Active

Initializing → Cancelled

Initializing → Disposed

Active → Paused

Paused → Active

Active → Completing

Paused → Completing

Completing → Completed

Created → Disposed

Completed → Disposed

Cancelled → Disposed

Active → Cancelled

Paused → Cancelled
```

Any transition not listed above is invalid.

---

# 8. Reading Context State

ReadingContext represents the current business understanding of what the user is reading.

Unlike Session Lifecycle,

ReadingContext changes much more frequently.

---

## 8.1 ReadingContext States

```text
ReadingContextState

├── Empty
├── Loading
├── Ready
├── Updating
├── Invalid
└── Disposed
```

---

## 8.2 Empty

No ReadingContext exists.

Occurs before initialization.

---

## 8.3 Loading

ReadingContext is being established.

Possible activities:

- resolve source
- resolve page
- resolve chapter
- resolve viewport

---

## 8.4 Ready

The ReadingContext accurately represents the current reading world.

Exactly one Ready context exists.

---

## 8.5 Updating

A business change is occurring.

Examples:

- page changed

- chapter changed

- viewport changed

- language changed

- reading mode changed

Updating eventually produces either:

- Ready

or

- Invalid

---

## 8.6 Invalid

ReadingContext can no longer represent reality.

Examples:

- source disappeared

- unsupported document

- corrupted metadata

Invalid contexts cannot produce new ProcessingIntent.

---

## 8.7 Disposed

Context permanently removed.

Terminal state.

---

# 9. ReadingContext Diagram

```text
Empty

↓

Loading

↓

Ready

↓

Updating

↓

Ready
```

Failure:

```text
Updating

↓

Invalid

↓

Disposed
```

---

# 10. Content Revision State

ContentRevision represents an immutable snapshot of the Reading Context.

Unlike ReadingContext,

which continuously evolves,

a ContentRevision never changes after creation.

Instead,

new revisions replace old business authority.

---

## 10.1 Revision States

```text
ContentRevisionState

├── Created
├── Current
├── Superseded
├── Archived
└── Discarded
```

Each ContentRevision progresses independently.

Multiple revisions may exist simultaneously.

Only one revision may be Current.

---

## 10.2 Created

A new ContentRevision has been generated.

Characteristics:

- immutable
- validated
- uniquely identified
- not yet authoritative

Allowed next states:

```text
Current

Discarded
```

---

## 10.3 Current

The revision represents the current reading world.

Characteristics:

- business authority
- ProcessingIntent may reference it
- newest accepted revision

Exactly one Current revision exists per Reading Session.

Allowed next states:

```text
Superseded

Archived
```

---

## 10.4 Superseded

A newer revision has replaced this revision.

Characteristics:

- immutable
- historically valid
- no longer authoritative

Superseded revisions never regain authority.

Allowed next states:

```text
Archived

Discarded
```

---

## 10.5 Archived

The revision remains available for history.

It no longer participates in business evaluation.

Archived revisions may be used for:

- diagnostics
- debugging
- timeline reconstruction
- analytics

Allowed next state:

```text
Discarded
```

---

## 10.6 Discarded

The revision has been permanently removed.

Terminal state.

---

# 11. Content Revision Diagram

```text
Created

↓

Current

↓

Superseded

↓

Archived

↓

Discarded
```

Early discard:

```text
Created

↓

Discarded
```

This may occur when a revision becomes invalid before becoming authoritative.

---

# 12. Revision Transition Rules

Valid transitions include:

```text
Created → Current

Created → Discarded

Current → Superseded

Current → Archived

Superseded → Archived

Superseded → Discarded

Archived → Discarded
```

All other transitions are forbidden.

Examples of invalid transitions:

```text
Superseded → Current

Archived → Current

Discarded → Current

Discarded → Created
```

---

# 13. Processing Intent State

ProcessingIntent represents business requirements generated from a ContentRevision.

It is not executable work.

Execution belongs to Runtime.

ProcessingIntent exists only to express what business outcomes are required.

---

## 13.1 ProcessingIntent States

```text
ProcessingIntentState

├── Created
├── Published
├── Accepted
├── Fulfilled
├── Obsolete
└── Discarded
```

The lifecycle of ProcessingIntent is intentionally independent from Runtime execution.

---

## 13.2 Created

The business requirement has been generated.

It has not yet been published.

Characteristics:

- immutable
- linked to one ContentRevision
- not visible outside Reading Session

Allowed next state:

```text
Published
```

---

## 13.3 Published

The ProcessingIntent has been published through the Runtime contract.

Characteristics:

- visible to Runtime
- immutable
- awaiting Runtime ownership

Reading Session performs no scheduling.

Allowed next states:

```text
Accepted

Obsolete
```

---

## 13.4 Accepted

Runtime has accepted responsibility for execution.

Reading Session no longer controls execution.

Business ownership remains unchanged.

Reading Session does not know:

- execution order
- worker assignment
- processing progress

Allowed next states:

```text
Fulfilled

Obsolete
```

---

## 13.5 Fulfilled

The business intent has been satisfied.

Reading Session has accepted that the requested business outcome exists.

Fulfilled does not imply successful execution of every intermediate stage.

It means the business objective represented by this ProcessingIntent is complete.

Terminal state.

---

## 13.6 Obsolete

The intent is no longer relevant.

Typical causes:

- newer ContentRevision
- newer ProcessingIntent
- session cancelled
- session completed

Obsolete intents never become Fulfilled.

Allowed next state:

```text
Discarded
```

---

## 13.7 Discarded

The intent has been removed permanently.

Terminal state.

---

# 14. Processing Intent Diagram

```text
Created

↓

Published

↓

Accepted

↓

Fulfilled
```

Replacement path:

```text
Published

↓

Obsolete

↓

Discarded
```

or

```text
Accepted

↓

Obsolete

↓

Discarded
```

---

# 15. Processing Intent Rules

ProcessingIntent follows several mandatory rules.

1. Every ProcessingIntent belongs to exactly one ContentRevision.

2. Every ContentRevision may produce zero or more ProcessingIntent objects.

3. Only one ProcessingIntent may be considered active for a particular business objective.

4. Obsolete intents never become Fulfilled.

5. Fulfilled intents never become Obsolete.

6. ProcessingIntent never changes after publication.

7. Runtime execution never mutates ProcessingIntent.

---

# 16. State Transition Guards

Every state transition must satisfy explicit business guards.

Transitions are never implicit.

---

## 16.1 Session Guards

Examples:

```text
Create Session

↓

No existing active session
```

```text
Pause Session

↓

Current state = Active
```

```text
Resume Session

↓

Current state = Paused
```

```text
Complete Session

↓

Current state ∈ {Active, Paused}
```

---

## 16.2 Reading Context Guards

Context updates require:

- valid ReadingSession
- supported source
- valid configuration
- successful context evaluation

Invalid context must not become Ready.

---

## 16.3 Revision Guards

A new ContentRevision may be created only when business state changes.

Examples:

- page changed
- chapter changed
- viewport changed
- language changed
- configuration changed

Repeated evaluation of identical business state must not create duplicate revisions.

---

## 16.4 Processing Intent Guards

ProcessingIntent may be published only when:

- ContentRevision is Current
- ReadingContext is Ready
- Session is Active

Otherwise,

no ProcessingIntent is produced.

---

# 17. Transition Triggers

Transitions occur because business events change the reading domain.

Typical triggers include:

```text
SessionCreated

SourceChanged

ViewportChanged

PageChanged

ChapterChanged

ConfigurationChanged

LanguageChanged

ReadingModeChanged

SessionPaused

SessionResumed

SessionCompleted

SessionCancelled
```

Runtime events do not directly trigger business state transitions.

Instead,

Runtime publishes completion through its own contracts,

which Reading Session evaluates as business facts.

---

# 18. Transition Actions

Every transition may execute business actions.

Examples include:

```text
Create ReadingContext

Create ContentRevision

Publish ProcessingIntent

Update Session Metadata

Record Business Timeline

Publish Business Event

Update Statistics

Invalidate Previous Revision
```

Actions must remain deterministic.

They must never invoke processing modules directly.

---

# 19. State Persistence

Reading Session distinguishes between business state that must survive over time and transient state that exists only during the current runtime.

Business persistence is determined by business value rather than implementation convenience.

---

## 19.1 Persistent State

The following business objects may be persisted.

```text
ReadingSession

ReadingContext

ContentRevision

SessionConfiguration

Business Timeline

Session Metadata
```

Persistence strategy is implementation-dependent.

The contract only defines what may survive beyond the current runtime.

---

## 19.2 Ephemeral State

The following information is transient.

```text
Current Event

Temporary Evaluation Data

Internal Comparison Buffers

Derived Intermediate Objects

Validation Cache
```

These objects are internal implementation details.

They are not business state.

---

## 19.3 Runtime State Is Never Persisted Here

Reading Session must never persist Runtime execution state.

Examples:

```text
Running Worker

Current OCR Progress

Queued Task

Retry Counter

Execution Pipeline

Processing Queue
```

Those belong exclusively to Runtime.

---

## 19.4 State Snapshot

At any moment the Reading Session can be represented by a complete business snapshot.

Conceptually:

```text
ReadingSessionSnapshot

├── Session
├── ReadingContext
├── CurrentRevision
├── SessionConfiguration
├── ActiveProcessingIntent
└── Metadata
```

A snapshot must represent one consistent business moment.

Partial snapshots are not allowed.

---

# 20. Recovery

Recovery reconstructs business state after interruption.

Recovery never reconstructs execution.

Execution restarts independently.

---

## 20.1 Recovery Philosophy

Reading Session restores business authority.

Runtime restores execution capability.

These are separate responsibilities.

---

## 20.2 Session Recovery

When a persisted Reading Session exists,

the module attempts to restore:

- Session Lifecycle
- Reading Context
- Current Revision
- Configuration

The module does not attempt to resume unfinished worker execution.

---

## 20.3 Revision Recovery

Only one ContentRevision may become Current.

If multiple candidate revisions exist,

business rules determine the newest authoritative revision.

All remaining revisions become:

```text
Superseded

or

Archived
```

---

## 20.4 Intent Recovery

Previously fulfilled ProcessingIntent objects remain historical facts.

Published but unfinished intents are re-evaluated.

Possible outcomes:

```text
Republish

Obsolete

Discard
```

Reading Session does not assume Runtime completed previous execution.

---

## 20.5 Recovery Constraints

Recovery must preserve:

- Session identity
- Revision identity
- business ordering
- historical correctness

Recovery must never create duplicate business history.

---

# 21. Invalid Transitions

Only documented transitions are valid.

Every other transition is considered a contract violation.

---

## 21.1 Invalid Session Transitions

Forbidden examples:

```text
Completed → Active

Disposed → Active

Cancelled → Active

Paused → Created

Completed → Initializing
```

---

## 21.2 Invalid ReadingContext Transitions

Forbidden examples:

```text
Disposed → Ready

Invalid → Loading

Ready → Empty
```

---

## 21.3 Invalid Revision Transitions

Forbidden examples:

```text
Superseded → Current

Archived → Current

Discarded → Current

Discarded → Created
```

Business authority always moves forward.

It never returns.

---

## 21.4 Invalid ProcessingIntent Transitions

Forbidden examples:

```text
Fulfilled → Published

Obsolete → Fulfilled

Discarded → Published

Discarded → Accepted
```

Once an intent reaches a terminal outcome,

it remains immutable forever.

---

## 21.5 Invalid Transition Handling

When an invalid transition is attempted,

Reading Session must:

1. reject the transition;
2. preserve the current state;
3. record diagnostics;
4. avoid publishing business events;
5. preserve business consistency.

Invalid transitions must never corrupt business history.

---

# 22. State Invariants

The following invariants must always remain true.

---

## 22.1 Session Invariants

1. Every Reading Session has exactly one current lifecycle state.

2. Completed sessions never become Active.

3. Disposed sessions never change again.

4. Cancelled sessions never resume.

5. Only Active sessions may produce new business activity.

---

## 22.2 ReadingContext Invariants

1. Every Reading Session owns at most one active ReadingContext.

2. Ready contexts represent the latest accepted reading world.

3. Invalid contexts never produce ProcessingIntent.

4. Disposed contexts never reappear.

---

## 22.3 ContentRevision Invariants

1. Every revision belongs to one Reading Session.

2. Revisions are immutable.

3. Exactly one revision is Current.

4. Superseded revisions never regain authority.

5. Archived revisions remain historically valid.

6. Discarded revisions never reappear.

---

## 22.4 ProcessingIntent Invariants

1. Every ProcessingIntent belongs to one ContentRevision.

2. Published intents are immutable.

3. Obsolete intents never become Fulfilled.

4. Fulfilled intents remain historical facts.

5. Runtime never changes ProcessingIntent ownership.

---

## 22.5 Business Invariants

1. Business authority always moves forward.

2. Business history is append-only.

3. Previous revisions remain historically correct.

4. Business state is deterministic.

5. Reading Session never owns Runtime execution.

6. Every business object has exactly one owner.

---

# 23. MVP State Model

The initial implementation may use a simplified state model while preserving the architectural guarantees.

---

## 23.1 MVP Session States

```text
Created

Active

Paused

Completed

Cancelled

Disposed
```

The `Initializing` and `Completing` states may be implemented internally without public exposure.

---

## 23.2 MVP ReadingContext States

```text
Loading

Ready

Updating

Invalid
```

This provides sufficient expressiveness for the first implementation.

---

## 23.3 MVP Revision States

```text
Current

Superseded
```

Archiving and discarding policies may be added later.

---

## 23.4 MVP ProcessingIntent States

```text
Created

Published

Fulfilled

Obsolete
```

The `Accepted` state may initially be omitted if Runtime acknowledgment is not yet implemented.

---

# 24. Testing

The Reading Session state machine must be validated through deterministic state transition tests.

---

## 24.1 Session Tests

Required scenarios include:

- create session
- activate session
- pause session
- resume session
- complete session
- cancel session
- dispose session
- invalid lifecycle transition

---

## 24.2 ReadingContext Tests

Required scenarios include:

- initial context creation
- page navigation
- chapter navigation
- viewport update
- invalid source
- context disposal

---

## 24.3 Revision Tests

Required scenarios include:

- create first revision
- create replacement revision
- supersede previous revision
- archive revision
- discard revision
- reject duplicate revision

---

## 24.4 ProcessingIntent Tests

Required scenarios include:

- publish intent
- obsolete previous intent
- fulfill current intent
- reject obsolete fulfillment
- discard obsolete intent

---

## 24.5 Recovery Tests

Recovery testing should verify:

- session restoration
- context restoration
- revision restoration
- historical consistency
- duplicate prevention
- business ordering

---

# 25. Open Decisions

The following architectural decisions remain intentionally open.

- Should Completed sessions be recoverable after application restart?

- Should historical ReadingContexts be persisted or regenerated?

- Should archived revisions remain indefinitely?

- How long should obsolete ProcessingIntent objects be retained?

- Should business history support time-travel debugging?

- Should multiple Reading Sessions be supported in parallel?

These questions do not affect the correctness of the current state model.

They may be resolved as implementation requirements evolve.

---

# 26. Related Documents

This specification complements the remaining Reading Session documentation.

```text
README.md

MODULE.md

CONTRACT.md

EVENTS.md

ERRORS.md
```

Responsibilities are divided as follows.

| Document | Responsibility |
|-----------|----------------|
| README | Module overview |
| MODULE | Architectural responsibilities |
| CONTRACT | Public interfaces |
| STATES | Business lifecycle |
| EVENTS | Business event definitions |
| ERRORS | Business failure model |

No document should redefine concepts owned by another document.

---

# 27. Summary

The Reading Session state model separates four independent business lifecycles.

```text
Reading Session

├── Session Lifecycle
├── Reading Context
├── Content Revision
└── Processing Intent
```

The key guarantees are:

- business state is deterministic;
- lifecycle transitions are explicit;
- business ownership is unambiguous;
- ContentRevision is immutable;
- ProcessingIntent expresses business intent rather than execution;
- Runtime execution is completely independent of business state;
- historical business information is append-only;
- business authority always moves forward.

Together, these guarantees ensure that Reading Session remains the single source of truth for the reading domain while remaining fully decoupled from Runtime execution.

---

# End of Document