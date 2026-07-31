# Reading Session Module

- Module: Reading Session
- Identifier: reading-session
- Layer: Business Orchestration
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

The Reading Session Module is the business orchestration layer responsible for managing a user's reading activity throughout its entire lifecycle.

A Reading Session represents a continuous interaction between a user and a readable source. The source may be a web page, comic, novel, ebook, PDF document, image collection, or any future content provider supported by CRAI.

Unlike processing modules, Reading Session never performs OCR, translation, rendering, recognition, or any business processing itself.

Its responsibility is to determine **what should happen** during a reading activity.

The responsibility of determining **how work is executed** belongs to the Runtime Architecture.

This separation allows the reading domain to evolve independently from execution technologies, processing engines, hardware acceleration, scheduling strategies, or AI providers.

Reading Session therefore serves as the domain authority for everything related to a reading activity while remaining completely independent from runtime implementation details.

---

# 2. Design Philosophy

Reading Session is designed around five core principles.

## 2.1 Reading First

Everything inside CRAI ultimately exists to support a user's reading experience.

The user never starts an OCR task.

The user never starts a Translation task.

The user never starts a Capture task.

The user starts reading.

Everything else exists only because reading requires it.

For this reason the Reading Session represents the primary business object inside the system.

---

## 2.2 Business Before Execution

Reading Session decides business intentions.

Runtime decides execution.

Workers perform execution.

This separation prevents business logic from becoming coupled to execution logic.

For example,

Reading Session may determine that a newly visible comic panel requires translation.

It does not determine:

- execution priority
- execution thread
- GPU allocation
- batching
- queue ordering
- retry strategy
- resource reservation

Those belong entirely to Runtime Control.

---

## 2.3 Stateless Processing

Processing modules should remain as stateless as possible.

Whenever possible, OCR, Translation, Recognition, Text Processing and Presentation operate using immutable requests generated from Reading Session state.

Long-lived business state must never migrate into processing modules.

Doing so would make the pipeline difficult to restart, parallelize or recover.

---

## 2.4 Revision Driven Architecture

Every meaningful change to the reading context produces a new Content Revision.

The revision represents a snapshot of the reading world.

Instead of modifying previous work, CRAI continuously creates newer revisions.

Older revisions naturally become obsolete.

This approach provides:

- deterministic execution
- cache safety
- race-condition resistance
- easier cancellation
- easier replay
- easier debugging

---

## 2.5 Runtime Independence

Reading Session must never assume:

- synchronous execution
- asynchronous execution
- local execution
- remote execution
- cloud execution
- GPU execution
- CPU execution

Its responsibility ends after expressing business intent.

How the intent becomes executable work belongs entirely to Runtime Architecture.

---

# 3. Goals

The module exists to achieve the following goals.

## 3.1 Represent Reading

Represent an ongoing reading activity using a single consistent domain model.

Regardless of whether the user is reading:

- manga
- manhua
- manhwa
- web novel
- EPUB
- PDF
- screenshots
- scanned documents

the reading experience should be represented using the same business concepts.

---

## 3.2 Maintain Reading Context

Reading is contextual.

Translation quality depends heavily on surrounding information.

Reading Session therefore maintains a consistent reading context including:

- current source
- current page
- current chapter
- visible viewport
- reading direction
- active language
- translation configuration
- user preferences
- current revision

This context becomes the authoritative business state for downstream processing.

---

## 3.3 Coordinate Business Activities

Reading Session coordinates the business lifecycle of processing without performing processing itself.

Examples include:

- reading started
- page changed
- viewport moved
- language changed
- translation mode changed
- session paused
- session resumed
- session finished

Each event may require different downstream work.

Reading Session determines which business actions are required.

---

## 3.4 Protect Reading Consistency

A user should never receive results belonging to an obsolete reading state.

If the user scrolls to another page while OCR is still running,

the old OCR result should never become visible simply because it finishes later.

Reading Session achieves this by maintaining immutable Content Revisions.

Every downstream request references the revision that originated it.

Late results from obsolete revisions are rejected by Runtime before reaching Presentation.

---

## 3.5 Support Future Reading Models

Reading Session should support future reading modes without redesign.

Examples include:

- dual-page reading
- vertical scrolling
- infinite scrolling
- AI assisted reading
- collaborative reading
- synchronized mobile reading
- cloud synchronized sessions
- multi-device continuation

The domain model should remain stable even as execution technology evolves.

---

# 4. Responsibilities

Reading Session owns the business lifecycle of a reading activity.

Its responsibilities include, but are not limited to, the following areas.

## 4.1 Session Lifecycle

Reading Session owns:

- session creation
- session initialization
- session activation
- session suspension
- session resumption
- session completion
- session cancellation
- session disposal

No other module may change the lifecycle state of a Reading Session.

---

## 4.2 Reading Context

Reading Session owns the authoritative reading context.

This includes:

- current source
- current page
- active viewport
- current chapter
- active language
- reading direction
- translation mode
- selected region
- session configuration
- user reading preferences

Processing modules may observe this context but never modify it.

---

## 4.3 Content Revision

Reading Session is the owner of Content Revision.

Every meaningful change to reading context generates a new immutable revision.

Examples include:

- browser navigation
- chapter change
- page change
- viewport change
- language change
- OCR mode change
- translation provider change
- reading mode change
- manual refresh
- user selection change

Each revision becomes a new business snapshot.

Previous revisions remain immutable until discarded by Runtime.

---

## 4.4 Processing Intent

Reading Session determines which processing activities are required.

Examples:

- Capture Required
- OCR Required
- Text Processing Required
- Translation Required
- Presentation Refresh Required

These are business intentions.

They are **not execution commands**.

Runtime decides whether, when and how those intentions become executable work.

---

## 4.5 Session Coordination

Reading Session coordinates relationships between business activities.

For example,

A language change may require:

- new OCR
- new translation
- new presentation

A viewport movement may require:

- capture refresh
- OCR reuse
- translation reuse
- presentation rebuild

Reading Session determines these dependencies according to business rules.

Execution remains outside the module.

---

## 4.6 Event Publication

Reading Session publishes business lifecycle events.

Typical examples include:

- SessionCreated
- SessionStarted
- SessionActivated
- SessionUpdated
- ReadingContextChanged
- ContentRevisionCreated
- SessionPaused
- SessionResumed
- SessionCancelled
- SessionCompleted

These events describe business state changes.

They do not describe processing progress.

---

## 4.7 Configuration Ownership

Reading Session owns runtime-independent configuration for a reading activity.

Examples include:

- target language
- source language
- translation provider preference
- OCR strategy preference
- reading mode
- cache preference
- auto translation mode
- presentation preference

Processing modules consume configuration but never own it.

---

# 5. Non-Responsibilities

To preserve architectural boundaries, Reading Session explicitly does **not** perform or own the following responsibilities.

## 5.1 Image Capture

Image acquisition belongs to the Capture Module.

Reading Session may request that new visual content be processed, but it never captures pixels directly.

---

## 5.2 OCR

Reading Session never recognizes text.

OCR belongs exclusively to the Recognition domain.

---

## 5.3 Text Normalization

Cleaning, segmentation, tokenization and normalization belong to Text Processing.

---

## 5.4 Translation

Translation engines are external business processors.

Reading Session never translates text.

---

## 5.5 Presentation Rendering

Presentation owns rendering decisions.

Reading Session never builds UI models.

---

## 5.6 Runtime Scheduling

Reading Session never decides:

- execution priority
- worker selection
- queue ordering
- concurrency level
- retry timing
- batching strategy

These belong to Runtime Architecture.

---

## 5.7 Resource Management

Reading Session never manages:

- memory pools
- thread pools
- GPU resources
- network connections
- worker lifetimes
- execution queues

These are Runtime concerns.

---

## 5.8 Persistence

Reading Session is not responsible for persistence implementation.

It may expose business state for persistence modules, but storage mechanisms remain outside its boundary.

---

# 6. Architectural Position

Reading Session occupies the highest business layer inside the CRAI processing architecture.

```text
                User
                  │
                  ▼
        Reading Session
                  │
                  ▼
        Runtime Control
                  │
                  ▼
            Scheduler
                  │
                  ▼
         Processing Modules
                  │
                  ▼
           Presentation
```

Reading Session expresses **business intent**.

Runtime converts business intent into executable work.

Processing modules perform execution.

Presentation exposes accepted results to the user.

---

# 7. Domain Model

The Reading Session Module owns the business representation of an active reading activity.

Rather than exposing implementation details, the module defines a stable domain model that remains consistent regardless of runtime implementation or processing technology.

Every concept inside this model represents business state rather than execution state.

Execution state belongs to Runtime Architecture.

---

## 7.1 Core Domain Objects

The module owns the following domain entities.

```text
ReadingSession
├── SessionContext
├── ReadingSource
├── ReadingTarget
├── ContentRevision
├── SessionConfiguration
├── SessionState
├── ReadingStatistics
└── ReadingMetadata
```

These entities together describe everything required to represent a user's reading activity.

---

## 7.2 ReadingSession

ReadingSession is the aggregate root of the module.

Every business operation performed during a reading activity belongs to exactly one ReadingSession.

The session provides:

- lifecycle ownership
- business identity
- context ownership
- revision ownership
- configuration ownership

Nothing outside the Reading Session Module may directly modify its internal state.

All changes occur through business operations defined by this module.

---

## 7.3 SessionContext

SessionContext represents the current business environment of an active session.

It contains information required for downstream processing but does not contain processing results.

Typical fields include:

- current source
- current page
- chapter identifier
- visible viewport
- active language
- target language
- selected reading mode
- translation configuration
- active ContentRevision

The context always represents the latest accepted reading state.

---

## 7.4 ReadingSource

ReadingSource identifies where readable content originates.

Examples include:

- browser tab
- desktop application
- local image
- PDF document
- EPUB
- online novel
- comic website
- clipboard image

The Reading Source remains independent from processing technologies.

Changing OCR engines should never affect ReadingSource.

---

## 7.5 ReadingTarget

ReadingTarget identifies the specific content currently being read.

Examples include:

- current comic page

- visible manga panel

- selected paragraph

- viewport

- selected image region

- current PDF page

ReadingTarget may change frequently while the ReadingSource remains unchanged.

---

## 7.6 SessionConfiguration

SessionConfiguration stores business preferences associated with a reading activity.

Examples include:

- source language

- target language

- translation provider preference

- OCR preference

- presentation mode

- auto translate enabled

- translation quality level

- reading direction

Configuration is mutable.

Configuration changes generate new Content Revisions whenever downstream processing becomes affected.

---

## 7.7 ReadingMetadata

Metadata stores descriptive information about the reading activity.

Examples:

- creation time

- last activity

- source type

- session identifier

- application identifier

- user identifier

Metadata never participates in business decision making.

It exists primarily for diagnostics, persistence and analytics.

---

## 7.8 ReadingStatistics

Statistics describe the progress of a reading activity.

Possible information includes:

- pages viewed

- chapters completed

- translation count

- OCR count

- processing duration

- active reading time

Statistics never influence processing decisions.

---

# 8. Reading Context

Reading Context represents the current business understanding of what the user is reading.

Everything downstream depends on this context.

Whenever the context changes, the system reevaluates required processing.

---

## 8.1 Context Authority

Reading Session is the only authority capable of producing an official Reading Context.

Processing modules may observe context.

They never create or modify it.

---

## 8.2 Context Consistency

At any point in time,

a Reading Session exposes exactly one active Reading Context.

There is never ambiguity regarding:

- active page

- active chapter

- active viewport

- active language

- active revision

This guarantee simplifies downstream processing.

---

## 8.3 Context Evolution

Reading Context evolves through immutable revisions.

Instead of modifying previous context,

Reading Session creates a new snapshot.

Example:

```text
Revision 15

Page 8
Viewport A
Language JP

↓

User Scrolls

↓

Revision 16

Page 8
Viewport B
Language JP
```

The previous revision remains immutable.

---

## 8.4 Context Stability

Minor execution failures never invalidate Reading Context.

For example,

an OCR failure does not change what the user is reading.

Only business events may change Reading Context.

---

# 9. Content Revision Model

Content Revision is one of the most important concepts in CRAI.

It represents a complete immutable snapshot of the reading context.

Everything downstream references ContentRevision.

---

## 9.1 Why Content Revision Exists

Without revisions,

late processing results could overwrite newer content.

Example:

```text
User opens Page 1

↓

OCR starts

↓

User jumps to Page 8

↓

OCR(Page1) finishes

↓

Wrong translation appears
```

Revision ownership prevents this situation.

---

## 9.2 Immutable Revision

A Content Revision is immutable.

Once created,

it can never be modified.

Changing any business property creates a completely new revision.

---

## 9.3 Revision Identity

Every revision owns a unique identifier.

Example:

```text
Revision 1001

Revision 1002

Revision 1003
```

Identifiers are strictly increasing within a session.

---

## 9.4 Revision Creation

Typical revision triggers include:

- page navigation

- viewport movement

- chapter change

- language change

- source replacement

- OCR mode change

- translation mode change

- presentation mode change

- manual refresh

Not every UI event creates a revision.

Only business-significant changes do.

---

## 9.5 Revision Lifetime

A revision exists until:

- session termination

or

- runtime discards obsolete history

Discarding history never changes revision identity.

---

## 9.6 Revision Authority

Only Reading Session creates Content Revisions.

No downstream module may create, replace or modify them.

---

# 10. Processing Intent Model

Processing Intent represents business requirements rather than executable work.

This distinction is fundamental.

---

## 10.1 Business Intent

Reading Session answers:

"What work is required?"

It never answers:

"How should work execute?"

---

## 10.2 Intent Types

Typical intents include:

- Capture Required

- OCR Required

- Text Processing Required

- Translation Required

- Presentation Refresh Required

Future versions may introduce additional intent types without redesigning the module.

---

## 10.3 Intent Independence

Multiple intents may exist simultaneously.

Example:

```text
Language Changed

↓

OCR Required

↓

Translation Required

↓

Presentation Refresh Required
```

The order of execution is not determined here.

---

## 10.4 Runtime Responsibility

Runtime receives Processing Intent.

Runtime decides:

- whether execution begins

- execution priority

- scheduling

- batching

- retry

- cancellation

Reading Session remains unaware of these implementation details.

---

# 11. Coordination Model

Reading Session coordinates business relationships.

It never coordinates execution.

---

## 11.1 Coordination Responsibility

The module determines dependencies between business activities.

For example,

changing the reading language invalidates existing translations.

Therefore Translation becomes required.

This is a business decision.

---

## 11.2 Execution Responsibility

Runtime transforms business decisions into executable work.

Workers perform execution.

Reading Session never communicates directly with workers.

---

## 11.3 Accepted Coordination Flow

```text
User Action

↓

Reading Session

↓

Business Intent

↓

Runtime Control

↓

Scheduler

↓

Workers

↓

Accepted Result

↓

Presentation
```

Every stage owns exactly one responsibility.

---

## 11.4 Forbidden Coordination

The following architecture is prohibited.

```text
OCR

↓

Translation

↓

Presentation
```

Processing modules must never invoke each other directly.

All coordination passes through Runtime using contracts defined by the architecture.

---

# 12. Ownership Model

One of the primary objectives of the Reading Session Module is to establish clear ownership boundaries across the CRAI architecture.

Ownership determines which module has the authority to create, modify, validate, or invalidate a specific business concept.

Without explicit ownership, multiple modules may attempt to control the same state, leading to race conditions, inconsistent behavior, and tightly coupled implementations.

Reading Session therefore serves as the sole business authority for the reading domain.

---

## 12.1 Ownership Philosophy

Every business concept must have exactly one owner.

Ownership includes the authority to:

- create
- modify
- validate
- invalidate
- retire

No ownership may be shared between modules.

Other modules may observe or consume business state but never mutate it.

---

## 12.2 Reading Session Owns

The Reading Session Module owns:

- ReadingSession
- SessionContext
- ReadingSource
- ReadingTarget
- ContentRevision
- SessionConfiguration
- SessionLifecycle
- ReadingContext
- ProcessingIntent

These concepts exist only because a user is reading.

---

## 12.3 Runtime Owns

Runtime Architecture owns execution.

Examples include:

- work queue
- execution scheduling
- cancellation propagation
- resource allocation
- retry strategy
- execution timeout
- worker lifecycle
- result acceptance

Reading Session has no authority over runtime implementation.

---

## 12.4 Processing Modules Own

Each processing module owns only its internal execution state.

Examples:

Capture

- CaptureRequest
- CaptureResult

Recognition

- OCRRequest
- OCRResult

Translation

- TranslationRequest
- TranslationResult

Presentation

- PresentationSnapshot
- RenderPlan

Reading Session never modifies these objects.

---

## 12.5 Ownership Matrix

| Concept | Owner |
|---------|-------|
| Reading Session | Reading Session |
| Session Context | Reading Session |
| Reading Source | Reading Session |
| Reading Target | Reading Session |
| Content Revision | Reading Session |
| Processing Intent | Reading Session |
| Runtime Queue | Runtime |
| Scheduler | Runtime |
| Worker | Runtime |
| Capture Result | Capture |
| OCR Result | Recognition |
| Translation Result | Translation |
| Presentation Snapshot | Presentation |

Ownership is exclusive.

No concept should ever appear with multiple owners.

---

# 13. Lifecycle Model

The lifecycle of a Reading Session represents the evolution of a user's reading activity.

It is independent of execution.

Processing failures do not automatically terminate a Reading Session.

---

## 13.1 Session Lifecycle

```text
Created

↓

Initializing

↓

Active

↓

Paused

↓

Active

↓

Completing

↓

Completed

↓

Disposed
```

Cancellation may occur from any active state.

---

## 13.2 Session Creation

Session creation establishes:

- session identifier
- initial context
- initial configuration
- initial ContentRevision
- initial state

No processing begins automatically until business intent is produced.

---

## 13.3 Session Activation

A session becomes Active when it is capable of accepting user interaction.

Activation does not imply that processing has finished.

Processing continues independently.

---

## 13.4 Session Update

While active, the session continuously evaluates business changes.

Typical triggers include:

- page navigation
- viewport movement
- chapter change
- language change
- configuration update
- source replacement

Each update may create a new ContentRevision.

---

## 13.5 Session Pause

Pause suspends business progression.

No new Processing Intent is produced while paused.

Already executing work may continue or be cancelled depending on Runtime policy.

---

## 13.6 Session Resume

Resuming re-enables business evaluation.

A new ContentRevision may be created if the source changed during suspension.

---

## 13.7 Session Completion

Completion occurs when the reading activity naturally ends.

Examples:

- browser closed
- document finished
- user exits reading mode

Completion publishes lifecycle events before disposal.

---

## 13.8 Session Disposal

Disposal permanently releases business resources.

Disposed sessions may never return to Active.

---

# 14. Cancellation Model

Cancellation exists to preserve business correctness rather than improve performance.

Performance is only a secondary benefit.

---

## 14.1 Business Cancellation

Reading Session determines that previous business intent is no longer valid.

Examples:

- page changed
- viewport changed
- chapter changed
- source replaced
- language changed

These events revoke the authority of older ContentRevisions.

---

## 14.2 Runtime Cancellation

Runtime receives cancellation requests and determines how execution should stop.

Possible mechanisms include:

- cooperative cancellation
- queue removal
- timeout
- worker interruption

Reading Session remains unaware of implementation.

---

## 14.3 Late Results

Processing may complete after cancellation.

Late results are not considered errors.

Instead,

they are simply obsolete.

Runtime rejects obsolete results before Presentation consumes them.

---

## 14.4 Revision Revocation

Cancellation never modifies existing revisions.

Instead,

Reading Session revokes their authority.

Example:

```text
Revision 18

↓

User Scroll

↓

Revision 19

↓

Revision 18 becomes obsolete
```

Revision 18 still exists historically.

It simply no longer represents the active reading world.

---

# 15. Dependency Model

Reading Session interacts with other modules exclusively through contracts.

Direct implementation coupling is prohibited.

---

## 15.1 Upstream Dependencies

Reading Session may receive business requests from:

- User Interface
- Browser Adapter
- Automation
- Session Recovery
- External API

These requests initiate business operations.

---

## 15.2 Downstream Dependencies

Reading Session publishes business intent consumed by:

- Runtime Control

Reading Session never invokes Capture, Recognition, Translation or Presentation directly.

Those interactions are delegated through Runtime.

---

## 15.3 Architectural Dependency

```text
User

↓

Reading Session

↓

Runtime

↓

Processing

↓

Presentation
```

Dependencies flow downward.

Authority never flows upward.

---

# 16. Design Principles

The Reading Session Module follows several architectural principles.

---

## Single Source of Truth

Only one active Reading Context exists for a session.

---

## Immutable Revisions

Business history is never modified.

New revisions replace old authority.

---

## Explicit Ownership

Every business concept has one owner.

---

## Runtime Independence

Business logic never depends on execution technology.

---

## Loose Coupling

Modules communicate using contracts and events.

Never through direct orchestration.

---

## Future Compatibility

The business model must remain stable as new OCR engines, AI providers, rendering systems and execution environments are introduced.

---

# 17. Performance Goals

Although Reading Session is not responsible for execution performance, its business model should encourage efficient execution.

Goals include:

- minimize unnecessary ContentRevisions
- maximize cache reuse opportunities
- reduce redundant processing
- avoid duplicate business intent
- minimize invalid processing
- support long-running sessions
- support future multi-session reading
- support distributed execution
- remain deterministic under concurrency

---

# 18. Architecture Invariants

The following invariants must always remain true.

1. Every ReadingSession has exactly one active ReadingContext.

2. Every ReadingContext belongs to exactly one ReadingSession.

3. Every ContentRevision belongs to exactly one ReadingSession.

4. Every ProcessingIntent references exactly one ContentRevision.

5. ContentRevisions are immutable.

6. Business state may only be modified by Reading Session.

7. Runtime may never modify ReadingContext.

8. Processing modules may never modify SessionContext.

9. Presentation may never modify business state.

10. Processing modules never invoke each other directly.

11. Reading Session never schedules execution.

12. Reading Session never performs business processing.

13. Runtime never owns business state.

14. Only one active ContentRevision exists within a ReadingSession.

15. Obsolete ContentRevisions never reach Presentation.

16. Session disposal is irreversible.

17. Ownership boundaries must never overlap.

18. Every business decision must be deterministic given identical input state.

---

# 19. Related Documents

The Reading Session Module serves as the architectural foundation for the remaining Reading Session specifications.

The following documents extend this module specification:

- CONTRACT.md
- STATES.md
- EVENTS.md
- ERRORS.md
- README.md

These documents inherit the terminology, ownership rules, architectural boundaries, and invariants defined in this specification.

No downstream document may redefine concepts introduced by this module.

Instead, they provide implementation contracts, lifecycle definitions, event specifications, error semantics, and usage guidance while remaining fully consistent with the architecture established here.

---

# End of Document