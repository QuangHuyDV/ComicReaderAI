# Reading Session Module

> **Project:** CRAI
> **Module:** `reading-session`
> **Path:** `doc/02-modules/reading-session/README.md`
> **Version:** 3.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-08

---

# 1. Overview

The Reading Session Module is the CRAI domain authority for an active reading activity.

Its primary responsibility is to maintain a consistent business understanding of:

```text
what the user is reading
+
where the user is reading
+
which session-specific reading configuration is active
```

Reading Session owns:

```text
ReadingSession
ReadingContext
ReadingContextRevision
ReadingSource
ReadingTarget
ReadingPosition
SessionConfiguration
Reading lifecycle
```

Reading Session does not own:

```text
pipeline orchestration
Runtime execution
Runtime authority
WorkItem
Attempt
processing retry
Artifact publication
Presentation state
native UI rendering
```

The central question answered by Reading Session is:

> **What is the current state of the user's reading activity?**

---

# 2. Architecture Position

Reading Session sits at the reading-domain boundary.

```text
User / Application Intent
        ↓
Reading Session
        ↓
Committed ReadingContext
        +
ReadingContextRevision
        ↓
Reading-domain facts
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

Ownership is deliberately separated:

```text
Reading Session
    → reading-domain state

Business Pipeline Orchestration
    → required processing decisions

Runtime Control
    → execution authority and lifecycle

Processing Modules
    → processing semantics/results

Presentation
    → committed Presentation state

UI Adapter
    → actual platform rendering
```

---

# 3. Why Reading Session Exists

CRAI supports many reading environments:

```text
browser
comic reader
image collection
novel
EPUB
PDF
application capture
future document surfaces
```

The technical source may differ.

The business concept remains:

```text
the user is reading something
```

Reading Session provides one stable domain model for that activity.

This prevents reading-domain semantics from being scattered across:

* OCR;
* Translation;
* Runtime;
* Presentation;
* UI frameworks;
* browser integrations;
* storage implementations.

---

# 4. Primary Responsibilities

Reading Session is responsible for:

```text
creating Reading Sessions
maintaining reading lifecycle
maintaining ReadingContext
maintaining ReadingContextRevision
validating ReadingSource
validating ReadingTarget
maintaining ReadingPosition
maintaining session-specific configuration
performing domain concurrency checks
committing immutable context snapshots
publishing reading-domain facts
supporting domain recovery/restoration
```

---

# 5. Explicit Non-Responsibilities

Reading Session MUST NOT:

* execute Capture;
* execute Recognition;
* execute Text Processing;
* execute Translation;
* construct PresentationSnapshot;
* construct RenderPlan;
* determine processing dependencies;
* decide whether OCR is required;
* decide whether Translation is required;
* create Runtime WorkItems;
* create Runtime Attempts;
* create RuntimeRevisionId;
* own Runtime cancellation;
* own Runtime retry;
* decide execution authority;
* publish processing completion facts;
* manipulate UI framework objects;
* own native viewport lifecycle;
* access persistence implementation directly.

---

# 6. Reading-First Domain Model

CRAI models user activity first.

```text
User reads content
        ↓
ReadingSession
        ↓
ReadingContext
```

It does not begin with:

```text
Run OCR
Run Translation
Run Presentation
```

Those are processing consequences of reading-domain state.

This distinction keeps the architecture aligned with the product experience rather than implementation details.

---

# 7. Core Domain Concepts

Reading Session owns:

```text
Reading Session

├── ReadingSession
├── ReadingContext
├── ReadingContextRevision
├── ReadingSource
├── ReadingTarget
├── ReadingPosition
└── SessionConfiguration
```

These concepts are described in detail in `MODULE.md` and `CONTRACT.md`.

---

# 8. ReadingSession

`ReadingSession` represents one logical reading activity.

Examples:

```text
reading one comic chapter
reading one novel
reading one PDF
reading content from a browser document
```

It has:

* identity;
* lifecycle;
* current ReadingContext;
* session-specific configuration;
* business metadata.

It is not a Runtime execution object.

---

# 9. ReadingContext

`ReadingContext` describes the current committed reading-domain state.

Conceptually:

```text
ReadingContext
├── ReadingSource
├── ReadingTarget
├── ReadingPosition
├── source language
├── target language
├── SessionConfiguration
└── ReadingContextRevision
```

ReadingContext contains no OCR, Translation, Runtime, or Presentation execution state.

---

# 10. ReadingContextSnapshot

A committed ReadingContext is exposed through an immutable:

```text
ReadingContextSnapshot
```

A snapshot may be used by:

* queries;
* Business Pipeline Orchestration;
* persistence;
* diagnostics;
* history.

Snapshots are immutable after commit.

---

# 11. ReadingContextRevision

Reading Session owns:

```text
ReadingContextRevision
```

It represents the version of committed reading-domain state.

Example:

```text
Revision 20
Target = Page 4

        ↓

user moves to Page 5

        ↓

Revision 21
Target = Page 5
```

ReadingContextRevision is not Runtime execution authority.

---

# 12. Removed `ContentRevision`

The previous Reading Session architecture used:

```text
ContentRevision
```

for two different purposes:

```text
reading-domain state version
+
processing authority
```

That model is removed.

The new separation is:

```text
ReadingContextRevision
    → reading-domain version

RuntimeRevisionId
    → execution authority
```

This prevents Reading Session from accidentally controlling Runtime execution.

---

# 13. Revision Domains

CRAI contains several independent revision/version domains.

```text
ReadingContextRevision
    owner: Reading Session

RuntimeRevisionId
    owner: Runtime Control

PresentationRevision
    owner: Presentation

ViewportRevision
    owner: UI Adapter / viewport owner

Artifact identity/version
    owner: Artifact contracts/store

Preference/Profile version
    owner: Preferences
```

These revisions must never be treated as interchangeable.

---

# 14. ReadingSource

ReadingSource identifies the logical reading origin.

Examples:

```text
BrowserDocument
ComicDocument
ImageCollection
PdfDocument
EpubDocument
StructuredText
ClipboardContent
ApplicationSurface
```

ReadingSource must remain independent from technical handles.

It must not contain:

```text
DOM nodes
native HWND
framework views
browser process handles
```

---

# 15. ReadingTarget

ReadingTarget identifies the logical content currently being read.

Examples:

```text
document
chapter
page
panel
paragraph
selection
logical content region
```

ReadingTarget answers:

> **What content is the user focused on?**

---

# 16. ReadingTarget vs PresentationTarget

These are separate concepts.

```text
ReadingTarget
    → what is being read

PresentationTarget
    → where translated output should appear
```

Example:

```text
ReadingTarget
    = Comic Page 18

PresentationTarget
    = Companion Side Panel
```

Reading Session owns only ReadingTarget.

---

# 17. ReadingPosition

Reading Session may maintain business-level reading progress.

Examples:

```text
chapter
page
paragraph anchor
logical item index
reading progress
```

Raw pixel coordinates or platform viewport transforms do not belong here.

---

# 18. Viewport Boundary

Technical viewport state belongs to UI Adapter/platform integration.

Examples:

```text
pixel width
pixel height
screen coordinates
device scale
window transform
viewport revision
```

Reading Session should only receive a normalized domain update when that technical change actually modifies:

```text
ReadingTarget
or
ReadingPosition
```

Presentation-only reflow may completely bypass Reading Session.

---

# 19. SessionConfiguration

Reading Session owns active session-specific reading configuration.

Examples:

```text
source language override
target language override
reading mode
auto-translate preference
translation quality preference
recognition preference
presentation preference
```

Persistent defaults remain owned by Preferences.

---

# 20. Preferences Boundary

Preferred flow:

```text
Preferences
    ↓
resolved user defaults
    ↓
Application
    ↓
Reading Session
    ↓
session-specific configuration
```

Reading Session does not become the persistence owner of global preferences.

---

# 21. Business Pipeline Orchestration Boundary

Reading Session does not decide processing topology.

It does not calculate:

```text
Capture Required
Recognition Required
Text Processing Required
Translation Required
Presentation Refresh Required
```

Those decisions belong to:

```text
Business Pipeline Orchestration
```

---

# 22. Removed `ProcessingIntent`

The previous architecture exposed:

```text
ProcessingIntent
```

as a Reading Session-owned business object.

That concept is removed from Reading Session v3.

Reason:

determining required processing requires knowledge of:

```text
pipeline dependencies
accepted Artifacts
Artifact reuse
capabilities
provider/runtime constraints
presentation requirements
```

Those belong outside the Reading Session domain.

---

# 23. Reading Session vs Business Pipeline Orchestration

Reading Session produces:

```text
ReadingContextSnapshot
ReadingContextChangeSet
Reading-domain facts
```

Business Pipeline Orchestration determines:

```text
what processing is required
what can be reused
what may be skipped
what downstream capabilities must run
```

This is a strict ownership boundary.

---

# 24. Reading Session vs Runtime Control

Runtime owns:

```text
RuntimeRevisionId
WorkItem
Attempt
authority
cancellation
retry
completion acceptance
supersession
```

Reading Session owns:

```text
ReadingSession
ReadingContext
ReadingContextRevision
reading lifecycle
```

Reading Session does not revoke Runtime authority directly.

---

# 25. Runtime Supersession

When reading-domain state changes:

```text
ReadingContextRevision N
        ↓
domain update
        ↓
ReadingContextRevision N+1
```

Reading Session only commits the new reading state.

Then:

```text
Business Pipeline Orchestration
        ↓
Runtime Control
        ↓
new execution authority
```

Runtime decides whether older execution becomes obsolete.

---

# 26. Cancellation

`CancelReadingSession` means:

```text
cancel the reading activity
```

It does not mean:

```text
cancel a Runtime Attempt directly
```

Reading Session publishes the reading-domain fact.

Runtime/Application independently applies execution cancellation policy.

---

# 27. Processing Failure Independence

The following failures do not automatically terminate or invalidate Reading Session:

```text
OCR failed
Translation failed
Runtime timed out
Presentation rejected
UI apply failed
```

A ReadingSession may remain:

```text
ACTIVE
```

with:

```text
ReadingContext READY
```

while processing is temporarily unavailable.

---

# 28. Session Lifecycle

Reading Session lifecycle is defined in `STATES.md`.

Full conceptual states:

```text
CREATED
INITIALIZING
ACTIVE
PAUSED
COMPLETING
COMPLETED
CANCELLED
DISPOSED
```

Primary normal path:

```text
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

Cancellation provides a separate terminal path.

---

# 29. Reading Context Lifecycle

ReadingContext has a separate lifecycle:

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

Session lifecycle and context lifecycle are related but distinct.

---

# 30. Candidate Reading Context

Reading Session uses Candidate isolation for mutations.

```text
Current Context N
        +
Candidate Context N+1
        ↓
validate
        ↓
commit?
    ├── yes → N+1 current
    └── no  → discard Candidate
              N remains current
```

This prevents partial domain updates.

---

# 31. Atomic Domain Commit

A successful Reading Context mutation commits:

```text
ReadingContextRevision
+
ReadingContextSnapshot
+
current context reference
```

as one logical operation.

Consumers must never observe partial domain state.

---

# 32. Optimistic Concurrency

Mutating commands may carry:

```text
expectedReadingContextRevision
```

Normal rule:

```text
expected
==
current
```

If not:

```text
ReadingContextRevisionConflict
```

The command is rejected without changing committed domain state.

---

# 33. No-Op Semantics

Equivalent domain updates should not create new revisions.

Examples:

```text
same ReadingTarget
same ReadingPosition
same target language
same SessionConfiguration
same ReadingSource identity
```

No-op:

```text
does not increment ReadingContextRevision
does not publish ReadingContextChanged
```

---

# 34. Commands

Typical Reading Session commands include:

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

Full contracts belong to:

```text
CONTRACT.md
```

---

# 35. Queries

Typical queries include:

```text
GetReadingSession
GetReadingContext
GetReadingContextRevision
GetSessionConfiguration
GetSessionState
ListActiveSessions
```

Queries return immutable domain state.

They never trigger processing.

---

# 36. Events

Reading Session publishes committed reading-domain facts.

Core events include:

```text
ReadingSessionCreated
ReadingSessionActivated
ReadingSessionPaused
ReadingSessionResumed
ReadingSessionCompleted
ReadingSessionCancelled
ReadingSessionDisposed

ReadingContextPrepared
ReadingContextChanged
ReadingContextInvalidated
ReadingContextDisposed
```

Optional specialized facts may include:

```text
ReadingTargetChanged
ReadingSourceChanged
ReadingPositionChanged
ReadingConfigurationChanged
ReadingLanguageChanged
ReadingModeChanged
```

---

# 37. Events Are Facts

Reading Session event:

```text
ReadingContextChanged
```

means:

```text
a new ReadingContext was committed
```

It does not mean:

```text
run OCR
run Translation
rebuild Presentation
```

Business Pipeline Orchestration determines processing consequences.

---

# 38. No ProcessingIntent Events

Reading Session v3 does not publish:

```text
ProcessingIntentCreated
ProcessingIntentPublished
ProcessingIntentAccepted
ProcessingIntentFulfilled
ProcessingIntentObsoleted
```

These concepts no longer belong to Reading Session.

---

# 39. No ContentRevision Lifecycle Events

Reading Session v3 does not publish:

```text
ContentRevisionCreated
ContentRevisionActivated
ContentRevisionSuperseded
ContentRevisionArchived
ContentRevisionDiscarded
```

`ReadingContextRevision` is carried by committed ReadingContext facts.

---

# 40. Event Bus Is Not the Workflow Engine

Invalid:

```text
ReadingContextChanged
    ↓
Translation directly starts itself
```

Preferred:

```text
ReadingContextChanged
        ↓
Business Pipeline Orchestration
        ↓
Runtime Control
        ↓
required processing
```

---

# 41. Event Publication Timing

Success facts occur only after domain state commit.

```text
validate
    ↓
commit
    ↓
publish event
```

If event publication fails:

```text
committed domain state remains committed
```

Do not rerun the domain command merely to regenerate the event.

---

# 42. Error Model

Reading Session errors describe reading-domain failures only.

Categories include:

```text
Validation
Session Lifecycle
Reading Context
ReadingContextRevision
Configuration
Consistency
Recovery
Event Publication
Internal
```

Detailed error semantics belong to:

```text
ERRORS.md
```

---

# 43. Failure Ownership

| Failure                         | Owner                           |
| ------------------------------- | ------------------------------- |
| Invalid ReadingTarget           | Reading Session                 |
| Invalid ReadingSource           | Reading Session                 |
| ReadingContextRevision conflict | Reading Session                 |
| Invalid Session transition      | Reading Session                 |
| Runtime Revision stale          | Runtime                         |
| WorkItem canceled               | Runtime                         |
| OCR failed                      | Recognition                     |
| Translation failed              | Translation                     |
| Artifact publication failed     | Artifact/Runtime infrastructure |
| Presentation failed             | Presentation                    |
| UI apply failed                 | UI Adapter                      |

---

# 44. ReadingContext Invalidity

`ReadingContextInvalid` means Reading Session cannot trust its domain understanding of:

```text
source
target
position
configuration
identity
```

It does not mean downstream processing failed.

This distinction is mandatory.

---

# 45. Persistence

Reading Session owns persistence semantics for reading-domain state.

Potential persisted values include:

```text
ReadingSession
ReadingContextSnapshot
ReadingContextRevision
ReadingPosition
SessionConfiguration
ReadingMetadata
```

Storage owns the persistence implementation.

---

# 46. Restoration

Stored state must be validated before becoming authoritative.

```text
persisted state
    ↓
load
    ↓
domain validation
    ↓
Candidate restored session
    ↓
commit
```

Restoration rebuilds reading-domain state only.

It does not restore Runtime execution authority.

---

# 47. Privacy

Reading Session should primarily store and expose:

```text
opaque IDs
reading-domain metadata
language values
target/position metadata
configuration summaries
revision values
```

Normal diagnostics must avoid:

```text
screenshots
full source text
full translated text
raw HTML
provider prompts
credentials
cookies
native handles
```

---

# 48. Platform Independence

Reading Session contracts must not depend on:

```text
DOM
Electron
Qt
Flutter
Android View
SwiftUI
WinUI
native window handles
browser-specific events
```

Adapters normalize platform-specific input into Reading Session domain commands.

---

# 49. Module Dependencies

Reading Session may depend on stable domain abstractions such as:

```text
core identifiers
language identifiers
reading-domain primitives
resolved configuration values
diagnostics abstractions
```

It must not depend directly on:

```text
Recognition implementation
Translation implementation
Presentation implementation
Scheduler implementation
Work Queue implementation
Storage backend
UI framework
browser API
operating-system API
provider SDK
```

---

# 50. Public Document Set

```text
02-modules/
└── reading-session/
    ├── README.md
    ├── MODULE.md
    ├── CONTRACT.md
    ├── STATES.md
    ├── EVENTS.md
    └── ERRORS.md
```

---

# 51. Document Responsibilities

## README.md

Provides the module overview.

Answers:

```text
What is Reading Session?
What does it own?
Where does it sit?
What should I read next?
```

## MODULE.md

Defines:

```text
module identity
ownership
domain boundaries
dependencies
architecture invariants
```

## CONTRACT.md

Defines:

```text
public commands
queries
domain models
ReadingContextSnapshot
ReadingContextRevision
concurrency contracts
```

## STATES.md

Defines:

```text
ReadingSession lifecycle
ReadingContext lifecycle
candidate behavior
revision behavior
recovery
```

## EVENTS.md

Defines:

```text
Reading Session-owned facts
event contracts
ordering
publication semantics
```

## ERRORS.md

Defines:

```text
error ownership
stable ErrorCodes
recovery
consistency failures
publication errors
```

---

# 52. Recommended Reading Order

For a new contributor:

```text
1. README.md
2. MODULE.md
3. CONTRACT.md
4. STATES.md
5. EVENTS.md
6. ERRORS.md
```

This gives:

```text
overview
→ ownership
→ API
→ lifecycle
→ facts
→ failures
```

---

# 53. Implementation Reading Order

For implementation work:

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

This ensures implementation starts with ownership and legal state before event publication.

---

# 54. Common Architecture Mistakes

## Mistake 1 — Treating Reading Session as Pipeline Orchestrator

Wrong:

```text
Reading Session
    ↓
decides OCR required
    ↓
starts OCR
```

Correct:

```text
Reading Session
    ↓
committed ReadingContext
    ↓
Business Pipeline Orchestration
    ↓
Runtime
```

---

## Mistake 2 — Reintroducing ProcessingIntent

Wrong:

```text
Reading Session creates:
TranslationRequired
PresentationRefreshRequired
```

Correct:

```text
Reading Session publishes domain facts/state
Business Pipeline Orchestration computes requirements
```

---

## Mistake 3 — Using ReadingContextRevision as Runtime Authority

Wrong:

```text
ReadingContextRevision changed
    ↓
Reading Session rejects Runtime result
```

Correct:

```text
ReadingContextRevision changed
    ↓
Runtime receives newer execution intent
    ↓
Runtime decides supersession
```

---

## Mistake 4 — Treating Runtime Cancellation as Session Cancellation

Wrong:

```text
Attempt canceled
    ↓
ReadingSession = CANCELLED
```

Correct:

```text
Attempt canceled
    → Runtime state

ReadingSessionCancelled
    → reading activity state
```

---

## Mistake 5 — Treating Viewport as Reading Context Automatically

Wrong:

```text
every pixel scroll
    ↓
new ReadingContextRevision
```

Correct:

```text
raw UI changes
    ↓
normalize / coalesce
    ↓
business-significant ReadingPosition/Target change
    ↓
Reading Session update
```

---

## Mistake 6 — Treating Processing Failure as Context Invalidity

Wrong:

```text
Translation failed
    ↓
ReadingContext INVALID
```

Correct:

```text
Translation failed
    → Translation/Runtime concern

ReadingContext remains valid
unless reading-domain state itself is invalid
```

---

# 55. Architecture Invariants

1. Reading Session is a reading-domain module.

2. Reading Session is not the Business Pipeline Orchestrator.

3. Reading Session owns ReadingSession.

4. Reading Session owns ReadingContext.

5. Reading Session owns ReadingContextRevision.

6. ContentRevision is removed as overloaded authority terminology.

7. ProcessingIntent is removed from Reading Session ownership.

8. Business Pipeline Orchestration decides required processing.

9. Runtime owns RuntimeRevisionId.

10. Runtime owns WorkItem.

11. Runtime owns Attempt.

12. Runtime owns execution authority.

13. Runtime owns cancellation and retry execution.

14. Reading Session does not directly invoke processing modules.

15. Reading Session does not accept/reject Runtime results.

16. ReadingContextRevision is not Runtime execution authority.

17. ReadingTarget and PresentationTarget are distinct.

18. ReadingPosition and technical viewport are distinct.

19. Session configuration and persistent Preferences are distinct.

20. Reading Session lifecycle and Runtime lifecycle are distinct.

21. Processing failures do not automatically mutate Reading Session lifecycle.

22. Candidate ReadingContext is not current state.

23. Context commit is atomic.

24. ReadingContextSnapshot is immutable.

25. No-op updates do not create unnecessary revisions.

26. Reading Session events describe committed domain facts.

27. Reading Session events do not request pipeline execution.

28. Event Bus is not the workflow engine.

29. Event publication failure does not roll back valid committed state.

30. Storage implementation remains external.

31. UI framework state remains external.

32. Diagnostics remain privacy-safe.

---

# 56. Related Architecture

```text
doc/01-architecture/core/
├── STATE_MACHINE.md
├── EVENT_BUS.md
└── EVENT_CONVENTION.md

doc/01-architecture/modules/
├── OWNERSHIP_MAP.md
├── MODULE_DEPENDENCY.md
└── MODULE_MAP.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── CANCELLATION.md
├── RETRY_POLICY.md
├── RESOURCE_LIFECYCLE.md
└── RUNTIME_OBSERVABILITY.md
```

Relevant neighboring modules:

```text
preferences
recognition
text-processing
translation
presentation
ui-adapter
storage
```

---

# 57. Completion Checklist

The Reading Session module is synchronized when:

* [ ] Reading Session is classified as Core Reading Domain;
* [ ] `ContentRevision` has been replaced by `ReadingContextRevision`;
* [ ] `ProcessingIntent` has been removed;
* [ ] Business Pipeline Orchestration owns processing decisions;
* [ ] Runtime owns execution authority;
* [ ] ReadingSession lifecycle is Runtime-independent;
* [ ] ReadingContext lifecycle is explicit;
* [ ] Candidate ReadingContext isolation is implemented;
* [ ] ReadingContextSnapshot is immutable;
* [ ] ReadingContextRevision is monotonic;
* [ ] optimistic concurrency is deterministic;
* [ ] no-op semantics are explicit;
* [ ] ReadingTarget is separate from PresentationTarget;
* [ ] ReadingPosition is separate from technical viewport;
* [ ] session configuration is separate from persistent Preferences;
* [ ] Reading Session never directly invokes processing modules;
* [ ] Reading Session events contain only domain facts;
* [ ] processing failures remain externally owned;
* [ ] event publication failure preserves committed domain state;
* [ ] persistence restores domain state only;
* [ ] all six Reading Session documents use the same ownership model.

---

# 58. Summary

Reading Session v3 is the CRAI authority for the user's reading-domain state.

Its core flow is:

```text
User / Application Reading Intent
        ↓
Reading Session Command
        ↓
Reading Session
        ↓
Candidate Reading Context
        ↓
Domain Validation
        ↓
Atomic Commit
        ↓
ReadingContextRevision
        +
ReadingContextSnapshot
        ↓
Reading-domain facts
        ↓
Business Pipeline Orchestration
        ↓
Runtime Control
        ↓
Processing
```

The ownership model is:

```text
Reading Session
    owns what the user is reading

Business Pipeline Orchestration
    owns what processing is required

Runtime Control
    owns which execution is authoritative

Processing Modules
    own processing semantics

Presentation
    owns committed presentation state

UI Adapter
    owns actual rendering
```

The central invariant is:

```text
ReadingContextRevision describes
the reading world.

RuntimeRevisionId describes
the execution world.

Those domains must remain separate.
```
