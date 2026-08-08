# Reading Session Module

> **Project:** CRAI
> **Module:** `reading-session`
> **Path:** `doc/02-modules/reading-session/MODULE.md`
> **Version:** 3.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-08

---

# 1. Module Definition

Reading Session is the CRAI domain module responsible for representing and maintaining a user's active reading activity.

Its primary responsibility is:

```text
User Reading Intent
        +
Reading Source
        +
Reading Target
        +
Session-specific Configuration
        ↓
Reading Session Domain Logic
        ↓
Committed Reading State
        ↓
ReadingContextSnapshot
        +
ReadingContextRevision
        +
Reading Domain Facts
```

Reading Session answers:

> **What is the user currently reading, and what is the current business state of that reading activity?**

It does not answer:

> Which processing pipeline should now run?

That belongs to Business Pipeline Orchestration.

It does not answer:

> How should required processing execute?

That belongs to Runtime Control.

---

# 2. Module Identity

```text
Module ID: reading-session
Module Type: Core Reading Domain Module
Primary Domain: User reading activity
Aggregate Root: ReadingSession
Primary State: ReadingContext
Primary Revision: ReadingContextRevision
Execution Authority: Runtime Control
Pipeline Decision Owner: Business Pipeline Orchestration
Persistence Implementation: Storage
MVP Priority: Required
```

Reading Session is a long-lived business-domain module.

It is not a processing module.

It is not a Runtime execution module.

It is not the pipeline orchestrator.

---

# 3. Architectural Position

The primary architecture is:

```text
User / Application Intent
        ↓
Reading Session
        ↓
Committed Reading Context
        +
Reading Domain Facts
        ↓
Business Pipeline Orchestration
        ↓
Runtime Control
        ↓
Processing Modules
        ↓
Accepted Artifacts
        ↓
Presentation
        ↓
UI Adapter
```

The ownership split is:

```text
Reading Session
    → what the user is reading

Business Pipeline Orchestration
    → what processing capabilities are required

Runtime Control
    → what execution is currently authoritative
      and how work progresses

Processing Modules
    → module-specific candidate results

Artifact Store
    → accepted runtime Artifacts

Presentation
    → committed user-visible Presentation state

UI Adapter
    → actual platform rendering
```

---

# 4. Why Reading Session Exists

CRAI processes reading content across many possible surfaces:

* browser pages;
* comics;
* images;
* novels;
* EPUB;
* PDF;
* captured application content;
* future structured readers.

Those environments differ technically.

The business concept does not.

The user is performing a reading activity.

Reading Session provides one stable business model for that activity independent from:

* OCR engine;
* Translation provider;
* Runtime implementation;
* scheduler;
* desktop framework;
* browser framework;
* storage backend.

---

# 5. Reading-First Principle

Users do not conceptually begin by starting:

```text
OCR
Translation
Capture
Presentation
```

They begin a reading activity.

Therefore CRAI should preserve a business model centered on:

```text
ReadingSession
ReadingSource
ReadingTarget
ReadingPosition
ReadingContext
SessionConfiguration
```

Processing exists to support those concepts.

Processing lifecycle must not leak back into the Reading Session aggregate.

---

# 6. Core Responsibility

Reading Session owns the business state of an active reading activity.

Typical responsibilities include:

```text
session lifecycle
reading source identity
current logical reading target
reading position
session-specific configuration
reading-context snapshots
ReadingContextRevision
domain validation
domain state changes
reading-domain event publication
```

---

# 7. Explicit Non-Responsibilities

Reading Session does not own:

```text
Capture execution
Recognition execution
Text Processing execution
Translation execution
Presentation construction
Pipeline dependency evaluation
WorkItem lifecycle
Attempt lifecycle
Runtime Revision
Runtime authority
Runtime cancellation
Runtime retry
Artifact publication
UI rendering
technical viewport lifecycle
persistent storage implementation
```

This boundary is mandatory.

---

# 8. Reading Session vs Business Pipeline Orchestration

This is a fundamental separation.

Reading Session determines:

```text
Page changed
Target language changed
Reading target changed
Session paused
Session resumed
Reading source replaced
```

Business Pipeline Orchestration determines:

```text
Does this change require Capture?
Can Recognition be reused?
Does Translation need rerunning?
Can an existing Artifact be reused?
Does Presentation need rebuilding?
```

Reading Session MUST NOT maintain pipeline dependency rules.

---

# 9. Reading Session vs Runtime Control

Reading Session owns business state.

Runtime Control owns execution authority.

```text
ReadingContextRevision
    → version of reading-domain state

RuntimeRevisionId
    → version of current execution intent
```

These are distinct revision domains.

Reading Session MUST NOT:

* create RuntimeRevisionId;
* determine Runtime WorkItem authority;
* mark Attempts superseded;
* reject Runtime completion;
* manage Runtime cancellation tokens;
* schedule retry.

---

# 10. Reading Session vs Processing Modules

Reading Session never directly invokes:

```text
Capture
Recognition
Text Processing
Translation
Presentation
```

Processing modules do not mutate Reading Session.

The relationship is mediated through:

```text
Business Pipeline Orchestration
+
Runtime Control
```

---

# 11. Aggregate Root

The aggregate root is:

```text
ReadingSession
```

Conceptually:

```text
ReadingSession
├── ReadingSessionId
├── SessionState
├── ReadingContext
├── ReadingContextRevision
├── SessionConfiguration
├── ReadingProgress
└── ReadingMetadata
```

All mutation of ReadingSession business state occurs through Reading Session commands/domain operations.

---

# 12. ReadingSessionId

`ReadingSessionId` identifies one logical reading activity.

Examples:

```text
reading a comic chapter
reading a novel
reading a PDF
reading content in a browser tab
```

A ReadingSessionId is not:

* a Runtime Session ID unless architecture explicitly aliases them;
* a WorkItem ID;
* a browser tab handle;
* a native window handle.

---

# 13. ReadingContext

`ReadingContext` represents the currently committed business understanding of the reading activity.

Conceptually:

```text
ReadingContext
├── readingSessionId
├── readingSource
├── readingTarget
├── readingPosition?
├── sourceLanguage?
├── targetLanguage?
├── sessionConfigurationRef
├── contextRevision
└── updatedAt
```

It describes business state.

It contains no processing results.

---

# 14. ReadingContextSnapshot

A `ReadingContextSnapshot` is an immutable representation of one committed Reading Context revision.

```text
ReadingContextSnapshot
├── readingSessionId
├── readingContextRevision
├── source
├── target
├── position?
├── sessionConfiguration
├── contentIdentity
└── createdAt
```

A snapshot may be passed to orchestration or Runtime as immutable provenance.

---

# 15. ReadingContextRevision

Reading Session owns:

```text
ReadingContextRevision
```

It represents changes to committed reading-domain state.

Example:

```text
ReadingContextRevision 31
    ↓
user moves to next page
    ↓
ReadingContextRevision 32
```

Rules:

1. monotonic within one ReadingSession;
2. immutable once committed;
3. only Reading Session creates it;
4. does not itself grant Runtime execution authority;
5. is not PresentationRevision;
6. is not Artifact version.

---

# 16. Deprecation of `ContentRevision`

The previous Reading Session specification used:

```text
ContentRevision
```

for both:

```text
reading-context version
+
downstream execution authority
```

That overloaded meaning is deprecated.

The preferred business-domain term is now:

```text
ReadingContextRevision
```

Runtime execution authority uses:

```text
RuntimeRevisionId
```

This removes ambiguity between:

```text
business state changed
```

and:

```text
execution result may still commit
```

---

# 17. Revision Separation

CRAI therefore has distinct revision/version domains.

```text
ReadingContextRevision
    owner: Reading Session

RuntimeRevisionId
    owner: Runtime Control

Artifact version / identity
    owner: Artifact contracts/store

PresentationRevision
    owner: Presentation

ViewportRevision
    owner: UI Adapter / normalized viewport owner

Preference/Profile version
    owner: Preferences
```

No module should use another owner's revision as if it were its own lifecycle counter.

---

# 18. ReadingSource

`ReadingSource` identifies the logical source from which readable content originates.

Examples:

```text
BrowserDocument
ComicDocument
ImageCollection
PdfDocument
EpubDocument
ClipboardContent
ApplicationSurface
```

Conceptually:

```text
ReadingSource
├── sourceId
├── sourceKind
├── sourceIdentity
├── title?
├── locator?
└── metadata?
```

ReadingSource must remain independent from technical implementation handles.

---

# 19. ReadingSource Boundary

ReadingSource may identify:

```text
a browser document
```

but must not contain:

```text
DOM Node
browser process handle
native HWND
framework-specific object
```

Technical handles belong to adapters/platform integration.

---

# 20. ReadingTarget

`ReadingTarget` identifies the logical portion of the source currently relevant to reading.

Examples:

* comic page;
* chapter;
* PDF page;
* paragraph;
* selected image;
* logical content region;
* structured document location.

Conceptually:

```text
ReadingTarget
├── targetId
├── targetKind
├── sourceId
├── contentIdentity
├── logicalLocator?
└── metadata?
```

---

# 21. ReadingTarget vs PresentationTarget

These are different concepts.

```text
ReadingTarget
    → what content the user is reading

PresentationTarget
    → where Presentation should display output
```

Example:

```text
ReadingTarget
    = Comic Page 42

PresentationTarget
    = Companion Side Panel
```

Reading Session owns ReadingTarget.

Presentation/UI architecture owns PresentationTarget semantics.

---

# 22. ReadingPosition

Reading Session may maintain business-level reading position.

Examples:

```text
chapter 4
page 18
paragraph anchor
scroll-progress percentage
logical item index
```

ReadingPosition should not become a dump of raw UI geometry.

Technical pixel viewport belongs elsewhere.

---

# 23. Viewport Boundary

The previous model treated viewport as authoritative Reading Session state.

Runtime v2 requires a distinction.

Technical viewport information such as:

```text
pixel width
pixel height
screen transform
target revision
device scale
platform coordinate space
```

belongs to UI Adapter/platform integration.

Reading Session may store business-relevant reading position derived from UI interaction.

If normalized viewport state must participate in orchestration, it should be referenced through a stable external snapshot rather than re-owned by Reading Session.

---

# 24. SessionConfiguration

Reading Session owns session-specific reading decisions.

Conceptually:

```text
SessionConfiguration
├── sourceLanguageOverride?
├── targetLanguageOverride?
├── readingMode?
├── autoTranslate?
├── translationQualityPreference?
├── recognitionModePreference?
├── presentationModePreference?
└── sessionOverrides[]
```

This is session-scoped domain configuration.

---

# 25. Preferences Boundary

Persistent user defaults belong to:

```text
Preferences
```

Reading Session may consume a resolved configuration snapshot and apply session-specific overrides.

Therefore:

```text
Preferences
    owns persistent/default preference

Reading Session
    owns active session-specific selection/override
```

Reading Session MUST NOT become the persistence owner of global Preferences.

---

# 26. Provider Preference Boundary

A user may express a session preference such as:

```text
prefer high-quality translation
prefer local OCR
```

Reading Session may preserve that user choice.

Actual provider resolution and execution configuration belong to the appropriate orchestration/provider/runtime layer.

Reading Session MUST NOT instantiate or select provider implementations directly.

---

# 27. Session Lifecycle

Reading Session owns its business lifecycle.

Primary states:

```text
CREATED
INITIALIZING
ACTIVE
PAUSED
COMPLETING
COMPLETED
DISPOSED
```

Optional cancellation/termination semantics are defined precisely in `STATES.md`.

Processing failures do not automatically change Reading Session lifecycle.

---

# 28. Session Creation

Creation establishes:

```text
ReadingSessionId
initial ReadingSource
initial ReadingTarget?
initial SessionConfiguration
initial ReadingContextRevision
initial lifecycle state
```

Creation itself does not schedule processing.

---

# 29. Session Activation

A session becomes `ACTIVE` when it can accept reading-domain operations.

Activation does not imply:

```text
Capture completed
Recognition completed
Translation completed
Presentation displayed
```

Those are independent lifecycles.

---

# 30. Session Pause

Pause means business progression is temporarily suspended according to Reading Session semantics.

While paused:

* new reading-domain mutations may be rejected or buffered according to contract;
* no assumption is made about already-running Runtime work.

Reading Session does not cancel Runtime work itself.

---

# 31. Session Resume

Resume returns the Reading Session to active business operation.

If the underlying source changed while paused, Application/integration may submit a new reading-target update after resume.

Reading Session then commits a new ReadingContextRevision as required.

---

# 32. Session Completion

Completion indicates the reading activity naturally ended.

Examples:

* user leaves reader;
* document finished;
* reading mode explicitly ended.

Completion does not directly dispose Runtime/UI resources.

Other owners respond through their own lifecycle contracts.

---

# 33. Session Disposal

Disposal means Reading Session business state is no longer active.

Disposed sessions cannot return to Active.

Physical persistence cleanup and Runtime resource disposal remain outside Reading Session.

---

# 34. Reading Context Mutation

A meaningful domain change follows:

```text
Current ReadingContext
        ↓
Domain Command
        ↓
Validate
        ↓
Build Candidate ReadingContext
        ↓
Commit
        ↓
ReadingContextRevision + 1
        ↓
Publish Reading Domain Fact
```

Reading Session mutation is independent from downstream processing completion.

---

# 35. Revision Creation Rules

A new ReadingContextRevision is created only when committed reading-domain state changes.

Typical examples:

```text
ReadingTarget changed
ReadingSource replaced
target language changed
source language override changed
session reading mode changed
session-level processing preference changed
reading position changed when business-significant
```

Not every raw UI event creates a revision.

---

# 36. Revision No-Op Rule

Equivalent domain state should not create an unnecessary revision.

Examples:

```text
same target selected again
same language selected again
same session mode applied again
equivalent source identity received
```

A no-op preserves the current ReadingContextRevision.

---

# 37. High-Frequency UI Changes

Raw scrolling or mouse movement may occur at high frequency.

Reading Session should not create one ReadingContextRevision for every technical event.

Instead:

```text
UI Adapter / Application
        ↓
normalize / coalesce
        ↓
business-significant reading change
        ↓
Reading Session update
```

This protects Runtime and cache efficiency.

---

# 38. Reading Domain Facts

Reading Session publishes facts describing committed reading-domain state changes.

Typical examples may include:

```text
ReadingSessionCreated
ReadingSessionActivated
ReadingContextChanged
ReadingTargetChanged
ReadingConfigurationChanged
ReadingSessionPaused
ReadingSessionResumed
ReadingSessionCompleted
ReadingSessionDisposed
```

Exact names belong to `EVENTS.md`.

---

# 39. Domain Events Are Not Pipeline Commands

A Reading Session event means:

```text
this reading-domain fact occurred
```

It does not mean:

```text
execute OCR now
translate now
capture now
```

Business Pipeline Orchestration consumes relevant facts/state and decides what processing is required.

---

# 40. Removal of `ProcessingIntent` Ownership

The previous module owned:

```text
Capture Required
OCR Required
Text Processing Required
Translation Required
Presentation Refresh Required
```

as `ProcessingIntent`.

That ownership is removed from Reading Session v3.

Reason:

Those decisions require pipeline dependency, reuse, Artifact compatibility, capability availability, and processing rules.

Those responsibilities belong to:

```text
Business Pipeline Orchestration
```

Reading Session should not duplicate them.

---

# 41. Business Pipeline Orchestration

Business Pipeline Orchestration may compare:

```text
previous ReadingContextSnapshot
new ReadingContextSnapshot
available accepted Artifacts
capabilities
pipeline policy
```

and determine:

```text
Capture required?
Recognition reusable?
Text Processing required?
Translation reusable?
Presentation update required?
```

Reading Session only provides trustworthy domain state.

---

# 42. Example — Target Language Change

Reading Session responsibility:

```text
Current target language = Vietnamese
        ↓
User selects English
        ↓
Validate
        ↓
Commit new ReadingContext
        ↓
ReadingContextRevision 25
        ↓
ReadingConfigurationChanged
```

Then separately:

```text
Business Pipeline Orchestration
        ↓
determines Translation needs reevaluation
        ↓
Runtime creates execution work
```

Reading Session never creates Translation WorkItems.

---

# 43. Example — Comic Page Change

```text
ReadingTarget = Page 10
        ↓
User moves to Page 11
        ↓
Reading Session commits:
ReadingTarget = Page 11
ReadingContextRevision = N+1
        ↓
ReadingTargetChanged
```

Pipeline Orchestration then decides whether Page 11 requires:

* new capture;
* Recognition;
* Artifact reuse;
* Translation;
* Presentation.

---

# 44. Example — Viewport Movement

Raw viewport movement does not automatically imply:

```text
new ReadingContextRevision
```

Possible flow:

```text
UI viewport changes rapidly
        ↓
UI Adapter normalizes/coalesces
        ↓
Application determines reading target/position materially changed
        ↓
Reading Session command
        ↓
new ReadingContextRevision if domain state changed
```

Presentation-only reflow may require no Reading Session revision at all.

---

# 45. Example — Presentation Mode Change

A session-specific user preference may change from:

```text
SidePanel
```

to:

```text
Overlay
```

Reading Session may commit the preference change if Presentation mode is considered reading-domain configuration.

However:

```text
Does existing Presentation need rebuild?
Can Overlay run?
Is geometry sufficient?
```

belongs outside Reading Session.

---

# 46. Runtime Revision Creation

Reading Session does not create RuntimeRevisionId.

A new committed ReadingContext may cause Business Pipeline Orchestration/Runtime integration to establish a new execution Revision.

Conceptually:

```text
ReadingContextRevision 30
        ↓
orchestration decision
        ↓
RuntimeRevisionId R100
```

The mapping need not be 1:1.

---

# 47. Reading Revision vs Runtime Revision

One ReadingContextRevision may require several Runtime changes.

One Runtime Revision may also process work derived from one stable Reading Context.

No architecture rule requires:

```text
ReadingContextRevision == RuntimeRevisionId
```

or:

```text
one ReadingContextRevision
=
exactly one Runtime Revision
```

They are independent identity domains.

---

# 48. Runtime Authority

Runtime owns whether asynchronous work may commit/publish.

Reading Session does not “revoke” Runtime work authority directly.

Instead:

```text
Reading domain changes
        ↓
new ReadingContextRevision committed
        ↓
Orchestration/Runtime receives new intent
        ↓
Runtime establishes new authority
        ↓
old Runtime work may become superseded
```

The Runtime owns the supersession decision.

---

# 49. Cancellation Boundary

The previous architecture described business cancellation as Reading Session revoking older ContentRevision authority.

That model is removed.

Reading Session may emit a domain fact that makes previous processing less relevant.

Runtime then determines:

* cancellation;
* supersession;
* queue removal;
* cooperative stop;
* result rejection.

Reading Session does not own Runtime cancellation.

---

# 50. Late Results

Late processing results are handled by Runtime authority checks.

Reading Session does not receive raw module completion in order to decide whether it is stale.

Required pattern:

```text
Attempt completes physically
        ↓
Runtime validates authority
        ↓
accepted or rejected
```

Only accepted Artifacts may reach downstream Presentation workflows.

---

# 51. Artifact Boundary

Reading Session does not own processing Artifacts.

Examples:

```text
CapturedFrameArtifact
RecognitionArtifact
SourceDocumentArtifact
TranslationArtifact
```

Reading Session may reference Artifact provenance when useful, but it must not mutate or publish those Artifacts.

---

# 52. Presentation Boundary

Reading Session does not construct:

```text
PresentationSnapshot
RenderPlan
PresentationItem
```

Presentation owns those concepts.

Reading Session describes:

```text
what is being read
```

Presentation describes:

```text
how accepted reading information should be represented to the user
```

---

# 53. Reading Progress

Reading Session may own domain-level reading progress when useful.

Examples:

```text
current chapter
current page
logical reading position
chapter completed
```

It should not own infrastructure metrics such as:

```text
OCR count
Translation latency
GPU duration
WorkItem duration
```

Those belong to telemetry/diagnostics/runtime observability.

---

# 54. ReadingMetadata

ReadingMetadata contains descriptive domain metadata.

Examples:

```text
createdAt
lastReadingActivityAt
sourceKind
document title?
chapter label?
```

Metadata should not contain:

* provider credentials;
* Runtime worker state;
* native handles;
* complete page content.

---

# 55. Concurrency

Reading Session business mutations for one ReadingSession must be logically serialized.

Concurrent commands may be accepted physically, but committed ReadingContext revisions must be deterministic.

Example:

```text
Current = Revision 20

Command A expects 20
Command B expects 20

B commits Revision 21

A reaches commit
    ↓
expected 20 != current 21
    ↓
reject / retry with latest domain state
```

This is Reading Session optimistic concurrency.

It is not Runtime execution authority.

---

# 56. Candidate Reading Context

Reading Session should prepare state changes before committing them.

Conceptually:

```text
CandidateReadingContext
├── basedOnRevision
├── candidateRevision
├── source
├── target
├── position
├── configuration
└── changeSet
```

Candidate state is not externally authoritative until committed.

---

# 57. Atomic Reading Commit

Commit should atomically update:

```text
ReadingContextRevision
+
ReadingContextSnapshot
+
Session-owned indexes/references
```

Domain facts publish only after commit.

---

# 58. Session State vs Reading Context Revision

Lifecycle state and context revision are distinct.

Example:

```text
SessionState = ACTIVE
ReadingContextRevision = 52
```

A page change may produce:

```text
ACTIVE / 53
```

without changing lifecycle state.

Pause may change lifecycle state and may or may not require a ReadingContextRevision depending on the final contract.

`STATES.md` will define the exact rule.

---

# 59. Session Lifecycle vs Runtime Lifecycle

These must never be equated.

```text
Reading Session ACTIVE
```

does not mean:

```text
Runtime WorkItem RUNNING
```

Likewise:

```text
Runtime Attempt FAILED
```

does not mean:

```text
Reading Session FAILED
```

Processing failure normally leaves the reading activity active.

---

# 60. Error Ownership

Reading Session owns only reading-domain errors.

Examples:

* invalid ReadingSessionId;
* invalid lifecycle transition;
* invalid ReadingTarget;
* invalid session configuration;
* ReadingContextRevision conflict;
* disposed session mutation;
* candidate context invariant violation.

It does not own:

* OCR failure;
* Translation failure;
* Runtime timeout;
* retry exhaustion;
* Artifact publication failure;
* Presentation failure;
* UI apply failure.

---

# 61. Dependency Rules

Reading Session may depend on stable contracts for:

```text
core identifiers
reading-domain primitives
resolved preference/configuration values
normalized source identity
normalized user/application intents
diagnostics abstraction
```

Reading Session must not directly depend on:

```text
Capture implementation
Recognition implementation
Translation implementation
Presentation implementation
Scheduler implementation
Event Bus implementation
Storage backend
UI framework
browser APIs
operating-system APIs
provider SDKs
```

---

# 62. Direct Processing Calls Are Forbidden

Invalid:

```text
Reading Session
    ↓
Recognition.execute()
```

Invalid:

```text
Reading Session
    ↓
Translation.translate()
```

Invalid:

```text
Reading Session
    ↓
Presentation.build()
```

Correct:

```text
Reading Session domain change
        ↓
Business Pipeline Orchestration
        ↓
Runtime Control
        ↓
processing contract
```

---

# 63. Event Bus Boundary

Reading Session may publish reading-domain facts.

It must not rely on Event Bus as hidden pipeline orchestration.

For example:

```text
ReadingTargetChanged
```

may be observed by orchestration.

But Reading Session does not require:

```text
ReadingTargetChanged
    → Recognition automatically starts
```

as a module invariant.

---

# 64. Persistence Boundary

Reading Session state may require persistence for:

* session restoration;
* reading continuation;
* reading position;
* session configuration.

Reading Session owns persistence semantics.

Storage owns persistence implementation.

Reading Session MUST NOT directly depend on:

```text
SQLite
PostgreSQL
filesystem
browser storage
cloud database
```

---

# 65. Session Restoration

A restored session record is not automatically active.

Conceptually:

```text
Stored Reading Session
        ↓
load through Storage contract
        ↓
validate domain state
        ↓
Candidate restored ReadingSession
        ↓
commit restoration
```

Invalid persisted state must not become authoritative Reading Context.

---

# 66. Privacy

Reading Session may contain sensitive reading-domain metadata.

Normal diagnostics should avoid:

* page content;
* source text;
* translated text;
* screenshots;
* browser HTML;
* private URLs where unnecessary.

Prefer:

```text
ReadingSessionId
source kind
target kind
revision
state
bounded identifiers
```

---

# 67. Performance

Reading Session is not responsible for processing latency.

Its performance goals are domain-state goals:

```text
fast domain mutation
bounded revision creation
low memory for long sessions
deterministic concurrency
minimal unnecessary context revisions
efficient immutable snapshots
```

Avoid generating ReadingContextRevision for meaningless technical noise.

---

# 68. Multiple Reading Sessions

Architecture should permit multiple independent ReadingSessions.

Example:

```text
Session A
    → comic

Session B
    → novel

Session C
    → PDF
```

Each owns:

```text
its own lifecycle
its own ReadingContext
its own ReadingContextRevision sequence
```

Runtime may execute work from multiple sessions concurrently.

---

# 69. Multi-Surface Reading

A single Reading Session may eventually support several logical Presentation/reading surfaces.

This must not require Reading Session to own native UI surfaces.

Future models may add:

```text
ReadingView
ReadingPane
LogicalSurfaceAssociation
```

only if needed by reading-domain semantics.

---

# 70. Reading Context Invariants

1. One ReadingSession has at most one current committed ReadingContext.

2. Every committed ReadingContext belongs to exactly one ReadingSession.

3. ReadingContextSnapshot is immutable.

4. ReadingContextRevision is monotonic within the ReadingSession.

5. Only Reading Session creates ReadingContextRevision.

6. Candidate ReadingContext is not current state.

7. Reading Session mutation is logically serialized.

8. No-op domain changes do not require a new revision.

---

# 71. Ownership Invariants

1. Reading Session owns reading-domain state.

2. Reading Session does not own pipeline orchestration.

3. Business Pipeline Orchestration owns processing-requirement decisions.

4. Runtime owns RuntimeRevisionId.

5. Runtime owns WorkItem.

6. Runtime owns Attempt.

7. Runtime owns execution authority.

8. Runtime owns cancellation and retry execution.

9. Artifact Store owns accepted Artifact lifecycle.

10. Presentation owns PresentationRevision.

11. UI Adapter owns technical viewport/surface lifecycle.

12. Preferences owns persistent user preference state.

---

# 72. Processing Invariants

1. Reading Session never performs Capture.

2. Reading Session never performs Recognition.

3. Reading Session never performs Text Processing.

4. Reading Session never performs Translation.

5. Reading Session never constructs Presentation.

6. Reading Session never invokes processing implementations directly.

7. Reading Session never schedules workers.

8. Reading Session never decides queue ordering.

9. Reading Session never determines Runtime retry.

10. Reading Session never accepts/rejects Runtime Attempt completion.

---

# 73. Revision Invariants

1. `ContentRevision` is deprecated as an overloaded execution-authority term.

2. `ReadingContextRevision` identifies reading-domain state.

3. `RuntimeRevisionId` identifies Runtime execution authority.

4. `PresentationRevision` identifies committed Presentation state.

5. Artifact identities/versions remain Artifact-owned.

6. Revision identities from different domains are never numerically compared for authority.

7. A ReadingContextRevision does not automatically invalidate Runtime work itself.

8. Runtime decides supersession through Runtime authority rules.

---

# 74. Lifecycle Invariants

1. Session lifecycle is independent from processing lifecycle.

2. Processing failure does not automatically terminate Reading Session.

3. Session pause does not directly mutate Runtime Attempt state.

4. Session completion does not directly destroy UI/native resources.

5. Session disposal is irreversible.

6. Restored persisted state must be validated before activation.

---

# 75. Example — Normal Reading Flow

```text
User opens comic
        ↓
CreateReadingSession
        ↓
ReadingSession CREATED
        ↓
Activate
        ↓
ReadingSession ACTIVE
        ↓
ReadingTarget = Page 1
ReadingContextRevision = 1
        ↓
Reading domain fact
        ↓
Business Pipeline Orchestration
        ↓
Runtime Revision established
        ↓
Capture / Recognition / Translation
        ↓
accepted Artifacts
        ↓
Presentation
```

---

# 76. Example — Page Change During OCR

```text
ReadingContextRevision 10
ReadingTarget = Page 4
        ↓
Runtime work running
        ↓
User selects Page 5
        ↓
Reading Session commits:
ReadingContextRevision 11
ReadingTarget = Page 5
        ↓
Business Pipeline Orchestration reacts
        ↓
Runtime establishes newer execution authority
        ↓
old OCR physically completes
        ↓
Runtime rejects obsolete completion
```

Reading Session does not inspect the OCR result.

---

# 77. Example — Presentation-Only Reflow

```text
ReadingTarget unchanged
ReadingContext unchanged
        ↓
window resized
        ↓
UI Adapter emits normalized viewport information
        ↓
Application requests Presentation reflow
        ↓
PresentationRevision changes
```

ReadingContextRevision may remain unchanged.

This is why Presentation and Reading revisions must remain separate.

---

# 78. Example — Translation Provider Preference Change

```text
User changes session translation preference
        ↓
Reading Session validates session override
        ↓
ReadingContextRevision N+1
        ↓
ReadingConfigurationChanged
        ↓
Business Pipeline Orchestration evaluates impact
        ↓
Runtime may schedule new Translation work
```

Reading Session does not instantiate the new provider.

---

# 79. Example — Pause

```text
ACTIVE
  ↓
PauseReadingSession
  ↓
PAUSED
```

Reading Session publishes a domain fact.

Runtime/Application policy separately determines whether existing processing:

```text
continues
is deprioritized
is canceled
```

Reading Session does not perform that execution action directly.

---

# 80. Example — Session Completion

```text
ACTIVE
  ↓
CompleteReadingSession
  ↓
COMPLETING
  ↓
COMPLETED
```

Completion means the business reading activity ended.

Runtime work, Presentation clearing, persistence, and UI disposal follow their respective owners' policies.

---

# 81. Conceptual Internal Components

Reading Session may internally contain responsibilities such as:

```text
Reading Session
├── ReadingSession Aggregate
├── Session Lifecycle Policy
├── Reading Context Manager
├── Reading Context Revision Manager
├── Reading Source Validator
├── Reading Target Validator
├── Session Configuration Resolver
├── Reading Position Policy
├── Candidate Context Builder
├── Domain Commit Coordinator
├── Domain Event Builder
└── Diagnostics
```

These are logical responsibilities, not mandatory code folders.

---

# 82. Testing Strategy

Reading Session must be testable without:

```text
OCR provider
Translation provider
Capture implementation
Scheduler
GPU
native window
browser DOM
Storage backend
```

---

# 83. Unit Tests

Test:

* session creation;
* lifecycle validation;
* context mutation;
* ReadingContextRevision monotonicity;
* no-op behavior;
* target validation;
* source replacement;
* configuration changes;
* optimistic concurrency;
* candidate isolation;
* domain event-after-commit behavior;
* disposal irreversibility.

---

# 84. Ownership Tests

Verify Reading Session never:

* creates RuntimeRevisionId;
* creates WorkItem;
* creates Attempt;
* changes Runtime authority;
* performs retry;
* calls processing modules;
* publishes processing-completion facts;
* builds PresentationSnapshot;
* owns UI viewport lifecycle.

---

# 85. Integration Tests

Verify:

```text
Reading Session domain mutation
        ↓
domain fact/state
        ↓
Business Pipeline Orchestration
```

without requiring Reading Session to know processing topology.

Also verify Runtime may change execution state without mutating ReadingSession.

---

# 86. Concurrency Tests

Test:

* two target changes from same expected ReadingContextRevision;
* configuration change racing target change;
* pause racing domain update;
* completion racing update;
* stale domain command after newer revision;
* no duplicate revision for equivalent update.

---

# 87. Architecture Decisions

## 87.1 Reading Session Is a Domain Module

It is no longer classified as:

```text
Business Orchestration
```

Preferred classification:

```text
Core Reading Domain
```

---

## 87.2 Pipeline Decisions Are External

Reading Session does not own `ProcessingIntent`.

Business Pipeline Orchestration owns pipeline requirement evaluation.

---

## 87.3 Runtime Authority Is External

Reading Session does not revoke execution authority.

Runtime Control owns that lifecycle.

---

## 87.4 `ContentRevision` Is Replaced

The ambiguous `ContentRevision` term should migrate toward:

```text
ReadingContextRevision
```

for business state.

---

## 87.5 Technical Viewport Is External

Reading Session owns reading position/target semantics.

UI Adapter owns technical viewport/surface state.

---

## 87.6 Persistent Preferences Are External

Preferences owns durable user defaults.

Reading Session owns session-specific resolved choices and overrides.

---

# 88. Architecture Invariants

1. Reading Session is the aggregate owner of one reading activity.

2. Reading Session owns ReadingContext.

3. Reading Session owns ReadingContextRevision.

4. ReadingContextRevision is immutable once committed.

5. Reading Session does not own RuntimeRevisionId.

6. Reading Session does not own Runtime execution authority.

7. Reading Session does not own WorkItem or Attempt.

8. Reading Session does not own processing retry.

9. Reading Session does not directly cancel Runtime work.

10. Reading Session does not own pipeline dependency evaluation.

11. Reading Session does not own `ProcessingIntent` in v3.

12. Business Pipeline Orchestration determines required processing.

13. Processing modules never mutate Reading Session.

14. Reading Session never directly calls processing implementations.

15. Reading Session does not publish processing completion facts.

16. Reading Session does not accept/reject processing results.

17. Reading Session does not own accepted Artifact lifecycle.

18. Reading Session does not own Presentation state.

19. Reading Session does not own native rendering.

20. ReadingTarget and PresentationTarget remain distinct.

21. Reading position and technical viewport remain distinct.

22. Persistent preferences and session overrides remain distinct.

23. Reading lifecycle and Runtime lifecycle remain distinct.

24. Processing failure does not automatically end Reading Session.

25. No-op domain changes do not create unnecessary revisions.

26. Domain state mutation is atomic.

27. Candidate ReadingContext is not authoritative before commit.

28. Domain facts are published only after business-state commit.

29. Reading Session does not use Event Bus as a hidden workflow engine.

30. Session disposal is irreversible.

31. Diagnostics remain privacy-safe.

32. Domain contracts remain platform-independent.

---

# 89. Related Documents

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULE_ROLE.md
.meta/WORKFLOW.md
.meta/CHANGE_RULE.md

doc/01-architecture/core/CAPABILITY_MAP.md
doc/01-architecture/core/DATA_FLOW.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
doc/01-architecture/core/STATE_MACHINE.md

doc/01-architecture/modules/MODULE_DEPENDENCY.md
doc/01-architecture/modules/MODULE_MAP.md
doc/01-architecture/modules/OWNERSHIP_MAP.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/MEMORY_MODEL.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/02-modules/reading-session/CONTRACT.md
doc/02-modules/reading-session/STATES.md
doc/02-modules/reading-session/EVENTS.md
doc/02-modules/reading-session/ERRORS.md
doc/02-modules/reading-session/README.md

doc/02-modules/preferences/MODULE.md
doc/02-modules/presentation/MODULE.md
```

---

# 90. Documentation Ownership

This file defines:

* Reading Session module identity;
* reading-domain ownership;
* aggregate boundary;
* ReadingContext;
* ReadingContextRevision;
* ReadingSource;
* ReadingTarget;
* lifecycle ownership;
* Runtime boundary;
* Business Pipeline Orchestration boundary;
* Presentation boundary;
* Preferences boundary;
* architecture invariants.

Detailed public contracts belong to:

```text
CONTRACT.md
```

Detailed lifecycle transitions belong to:

```text
STATES.md
```

Detailed Reading Session-owned facts belong to:

```text
EVENTS.md
```

Detailed domain error semantics belong to:

```text
ERRORS.md
```

---

# 91. Completion Criteria

The Reading Session module is architecturally usable when:

* ReadingSession aggregate ownership is explicit;
* ReadingContext is immutable after commit;
* ReadingContextRevision is monotonic and module-owned;
* `ContentRevision` execution-authority ambiguity is removed;
* RuntimeRevisionId is clearly external;
* processing pipeline decisions are outside Reading Session;
* `ProcessingIntent` ownership has moved to Business Pipeline Orchestration;
* session lifecycle is independent from Runtime lifecycle;
* target/position semantics are separate from technical viewport;
* session configuration is separate from persistent Preferences;
* no processing module directly depends on Reading Session internals;
* Reading Session never directly invokes processing;
* domain facts publish after commit;
* candidate domain state is isolated;
* concurrency is deterministic;
* tests can run without Runtime workers or providers.

---

# 92. Summary

Reading Session v3 is CRAI's reading-domain state authority.

Its core flow is:

```text
User / Application Reading Intent
        ↓
Reading Session
        ↓
Candidate Reading Context
        ↓
Domain Validation
        ↓
Atomic Reading Commit
        ↓
ReadingContextRevision
        +
ReadingContextSnapshot
        ↓
Reading Domain Facts
        ↓
Business Pipeline Orchestration
        ↓
Runtime Control
        ↓
Processing
```

The critical ownership model is:

```text
Reading Session
    owns what the user is reading

Business Pipeline Orchestration
    owns what processing is required

Runtime Control
    owns which execution is authoritative

Processing Modules
    own module-specific processing semantics

Presentation
    owns committed user-visible presentation

UI Adapter
    owns actual platform rendering
```

The central invariant is:

```text
ReadingContextRevision describes
the reading world.

RuntimeRevisionId describes
the execution world.

They must never be treated
as the same authority.
```
