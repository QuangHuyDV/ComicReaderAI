# Reading Session Contract

> **Project:** CRAI
> **Module:** `reading-session`
> **Path:** `doc/02-modules/reading-session/CONTRACT.md`
> **Contract Version:** 3.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-08

---

# 1. Purpose

This document defines the public contract boundary of the Reading Session module.

Reading Session represents and maintains the business state of a user's reading activity.

Its public contract exposes:

```text
ReadingSession
ReadingContext
ReadingContextSnapshot
ReadingContextRevision
ReadingSource
ReadingTarget
ReadingPosition
SessionConfiguration
Session lifecycle commands
Reading-domain queries
Reading-domain facts
```

Reading Session does not expose:

```text
ProcessingIntent
RuntimeRevisionId ownership
WorkItem
Attempt
Scheduler state
Runtime cancellation state
Runtime retry state
Processing results
Presentation state
Native UI state
```

This contract exists so that other parts of CRAI can observe or mutate reading-domain state without accessing Reading Session internals.

---

# 2. Contract Scope

This file defines:

* public identifiers;
* public commands;
* public queries;
* ReadingSession contract;
* ReadingContext contract;
* ReadingContextSnapshot;
* ReadingContextRevision;
* ReadingSource;
* ReadingTarget;
* ReadingPosition;
* SessionConfiguration;
* command result semantics;
* optimistic concurrency;
* ownership boundaries;
* compatibility rules.

This file does not define:

* Runtime execution;
* Business Pipeline Orchestration rules;
* processing topology;
* WorkItem/Attempt lifecycle;
* Runtime cancellation;
* Runtime retry;
* Artifact publication;
* Presentation lifecycle;
* Event Bus implementation;
* storage implementation.

Those concerns belong elsewhere.

---

# 3. Architectural Boundary

The intended interaction is:

```text
User / Application Intent
        ↓
Reading Session Command
        ↓
Reading Session
        ↓
Committed ReadingContext
        +
ReadingContextRevision
        ↓
Reading-domain fact / queryable state
        ↓
Business Pipeline Orchestration
        ↓
Runtime Control
```

Reading Session ends at committed reading-domain state.

It does not cross into execution planning.

---

# 4. Contract Principles

## 4.1 Reading-Domain Only

Every public object owned by this module must represent reading-domain semantics.

A Reading Session contract must not contain mutable:

* Runtime state;
* scheduler state;
* processing-provider state;
* native UI state;
* storage implementation details.

---

## 4.2 Immutable Published State

Committed snapshots are immutable.

A state change creates:

```text
new ReadingContextSnapshot
+
new ReadingContextRevision
```

Existing committed snapshots are never mutated.

---

## 4.3 Explicit Ownership

Reading Session owns:

```text
ReadingSession
ReadingContext
ReadingContextSnapshot
ReadingContextRevision
ReadingSource
ReadingTarget
ReadingPosition
SessionConfiguration
```

Reading Session does not own:

```text
RuntimeRevisionId
PresentationRevision
ViewportRevision
Artifact identity
Preference persistence
```

---

## 4.4 Runtime Independence

Reading Session commands never specify:

* queue priority;
* worker selection;
* execution order;
* batching;
* retry timing;
* timeout;
* GPU/CPU selection.

---

## 4.5 No Hidden Pipeline Contract

This contract does not expose:

```text
CaptureRequired
RecognitionRequired
TranslationRequired
PresentationRefreshRequired
```

Those are pipeline decisions owned by Business Pipeline Orchestration.

---

# 5. Naming Convention

Public types use PascalCase.

Fields are shown in camelCase.

Examples:

```text
CreateReadingSession
ReadingContextSnapshot
readingSessionId
expectedReadingContextRevision
```

---

# 6. ReadingSessionId

```text
ReadingSessionId
- value
```

Identifies one logical reading activity.

Rules:

* immutable after creation;
* globally unique within the required application identity domain;
* not derived from a browser/native handle;
* not equal to Runtime WorkItem/Attempt identity.

---

# 7. ReadingContextRevision

```text
ReadingContextRevision
- value
```

Represents one committed version of reading-domain state.

Rules:

1. scoped to one ReadingSession;
2. monotonic;
3. immutable once committed;
4. only Reading Session creates it;
5. does not itself grant Runtime authority;
6. is distinct from RuntimeRevisionId;
7. is distinct from PresentationRevision.

---

# 8. Deprecation of ContentRevision

The previous contract exposed:

```text
ContentRevision
```

as both:

```text
reading state version
+
downstream processing authority
```

That overloaded meaning is removed.

The replacement is:

```text
ReadingContextRevision
```

for Reading Session state.

Runtime execution authority belongs to:

```text
RuntimeRevisionId
```

owned by Runtime Control.

---

# 9. Revision Separation

The contract recognizes multiple revision domains.

```text
ReadingContextRevision
    owner: Reading Session

RuntimeRevisionId
    owner: Runtime Control

PresentationRevision
    owner: Presentation

ViewportRevision
    owner: UI Adapter / viewport owner

Preference/Profile version
    owner: Preferences
```

Values from different domains MUST NOT be numerically compared as if they shared authority semantics.

---

# 10. ReadingSource

```text
ReadingSource
├── sourceId
├── sourceKind
├── sourceIdentity
├── title?
├── locator?
└── metadata?
```

Possible kinds:

```text
BrowserDocument
ComicDocument
ImageCollection
PdfDocument
EpubDocument
StructuredText
ClipboardContent
ApplicationSurface
Unknown
```

ReadingSource identifies logical reading origin.

It contains no native handles or framework objects.

---

# 11. ReadingTarget

```text
ReadingTarget
├── targetId
├── targetKind
├── sourceId
├── contentIdentity
├── logicalLocator?
└── metadata?
```

Possible target kinds:

```text
Document
Chapter
Page
Panel
Paragraph
Selection
ImageRegion
LogicalViewportTarget
Unknown
```

ReadingTarget identifies what logical content is currently being read.

---

# 12. ReadingTarget vs PresentationTarget

These contracts are intentionally separate.

```text
ReadingTarget
    → what content is being read

PresentationTarget
    → where translated/presented output should appear
```

Reading Session never creates PresentationTarget.

---

# 13. ReadingPosition

```text
ReadingPosition
├── chapterId?
├── pageId?
├── paragraphAnchor?
├── logicalIndex?
├── progress?
└── customLocator?
```

ReadingPosition captures business-relevant progress.

Raw technical viewport geometry must not be embedded by default.

---

# 14. Technical Viewport Boundary

Technical viewport information such as:

```text
pixel width
pixel height
device scale
screen coordinates
native transforms
surface revision
```

belongs outside Reading Session.

If business reading position changes because of viewport movement, Application may translate that interaction into a Reading Session command.

Not every viewport event creates a ReadingContextRevision.

---

# 15. SessionConfiguration

```text
SessionConfiguration
├── sourceLanguageOverride?
├── targetLanguageOverride?
├── readingMode?
├── autoTranslate?
├── translationQualityPreference?
├── recognitionPreference?
├── presentationPreference?
└── sessionOverrides[]
```

This configuration is session-scoped.

Persistent defaults belong to Preferences.

---

# 16. ReadingContext

```text
ReadingContext
├── readingSessionId
├── readingSource
├── readingTarget?
├── readingPosition?
├── sourceLanguage?
├── targetLanguage?
├── sessionConfiguration
└── readingContextRevision
```

ReadingContext is the current committed business state.

It contains no processing result.

---

# 17. ReadingContextSnapshot

```text
ReadingContextSnapshot
├── readingSessionId
├── readingContextRevision
├── readingSource
├── readingTarget?
├── readingPosition?
├── sourceLanguage?
├── targetLanguage?
├── sessionConfiguration
├── contentIdentity?
├── createdAt
└── changeReason?
```

Rules:

* immutable;
* belongs to one ReadingSession;
* belongs to one ReadingContextRevision;
* safe for orchestration/runtime provenance;
* contains no mutable provider/runtime objects.

---

# 18. ReadingSession

```text
ReadingSession
├── readingSessionId
├── lifecycleState
├── currentContext
├── currentReadingContextRevision
├── configuration
├── metadata?
└── createdAt
```

The public object is immutable.

Mutation occurs only through commands.

---

# 19. Command Envelope

Reading Session commands should carry:

```text
requestId
contractVersion
issuedAt
readingSessionId?
expectedReadingContextRevision?
correlationId?
causationId?
```

Commands targeting existing state SHOULD use optimistic concurrency where appropriate.

---

# 20. CreateReadingSession

```text
CreateReadingSession
├── requestId
├── contractVersion
├── initialSource
├── initialTarget?
├── initialPosition?
├── initialConfiguration
└── metadata?
```

Result:

```text
CreateReadingSessionResult
├── readingSessionId
├── lifecycleState
├── readingContextRevision
└── readingContextSnapshot
```

Creation does not start processing execution.

---

# 21. ActivateReadingSession

```text
ActivateReadingSession
├── requestId
├── readingSessionId
└── expectedLifecycleState?
```

Result:

```text
ActivateReadingSessionResult
├── readingSessionId
├── lifecycleState
└── readingContextRevision
```

Activation means the reading activity is ready for domain interaction.

It does not create ProcessingIntent.

---

# 22. UpdateReadingTarget

Preferred explicit command:

```text
UpdateReadingTarget
├── requestId
├── readingSessionId
├── expectedReadingContextRevision
├── readingTarget
├── readingPosition?
└── reason
```

Possible reasons:

```text
PageChanged
ChapterChanged
SelectionChanged
SourceNavigation
UserSelectedTarget
RecoveredPosition
```

If state changes:

```text
ReadingContextRevision + 1
```

If equivalent:

```text
NoOp
```

---

# 23. ReplaceReadingSource

```text
ReplaceReadingSource
├── requestId
├── readingSessionId
├── expectedReadingContextRevision
├── readingSource
├── initialTarget?
├── initialPosition?
└── reason
```

A source replacement normally creates a new ReadingContextRevision.

It does not directly cancel Runtime work.

---

# 24. UpdateReadingPosition

```text
UpdateReadingPosition
├── requestId
├── readingSessionId
├── expectedReadingContextRevision
├── readingPosition
└── reason
```

Applications SHOULD coalesce high-frequency technical input before issuing this command.

Equivalent position changes may resolve as no-op.

---

# 25. UpdateSessionConfiguration

```text
UpdateSessionConfiguration
├── requestId
├── readingSessionId
├── expectedReadingContextRevision
├── configurationPatch
└── reason
```

Examples:

* target language;
* source language override;
* reading mode;
* session presentation preference;
* recognition preference;
* auto-translate choice.

Reading Session commits the domain preference change.

It does not decide pipeline consequences.

---

# 26. UpdateReadingContext

A broader convenience command MAY exist:

```text
UpdateReadingContext
├── requestId
├── readingSessionId
├── expectedReadingContextRevision
├── source?
├── target?
├── position?
├── configurationPatch?
└── reason
```

Implementations should prefer more explicit commands when domain semantics differ.

Atomicity applies to the full context mutation.

---

# 27. PauseReadingSession

```text
PauseReadingSession
├── requestId
├── readingSessionId
└── reason?
```

Result:

```text
PauseReadingSessionResult
├── readingSessionId
└── lifecycleState = PAUSED
```

Pause does not directly cancel Runtime work.

---

# 28. ResumeReadingSession

```text
ResumeReadingSession
├── requestId
├── readingSessionId
└── reason?
```

Result:

```text
ResumeReadingSessionResult
├── readingSessionId
└── lifecycleState
```

Resume does not automatically create a new ReadingContextRevision unless domain state also changes.

---

# 29. CompleteReadingSession

```text
CompleteReadingSession
├── requestId
├── readingSessionId
└── reason
```

Completion ends the normal reading activity.

It does not directly:

* cancel WorkItems;
* clear Presentation;
* destroy UI surfaces.

Those belong to their respective owners.

---

# 30. CancelReadingSession

If business cancellation remains part of the Reading Session lifecycle:

```text
CancelReadingSession
├── requestId
├── readingSessionId
└── reason
```

Meaning:

```text
the reading activity itself is canceled
```

It does **not** mean:

```text
Reading Session revokes Runtime authority directly
```

Runtime/Application observes the lifecycle fact and applies its own cancellation/supersession policy.

---

# 31. DisposeReadingSession

```text
DisposeReadingSession
├── requestId
└── readingSessionId
```

Rules:

* irreversible;
* only valid from allowed terminal states;
* releases Reading Session-owned business references;
* does not directly destroy external Runtime/UI/Storage resources.

---

# 32. Command Result

Generic conceptual result:

```text
ReadingSessionCommandResult
├── requestId
├── status
├── readingSessionId?
├── previousReadingContextRevision?
├── readingContextRevision?
├── lifecycleState?
├── contextSnapshot?
├── rejection?
└── occurredAt
```

Possible status:

```text
Committed
NoOp
Rejected
```

Runtime execution status does not appear here.

---

# 33. Optimistic Concurrency

Commands mutating ReadingContext SHOULD carry:

```text
expectedReadingContextRevision
```

Normal guard:

```text
expectedReadingContextRevision
==
currentReadingContextRevision
```

If not equal:

```text
ReadingContextRevisionConflict
```

No automatic merge is required for MVP.

---

# 34. No-Op Semantics

A valid command that produces semantically equivalent domain state SHOULD return:

```text
NoOp
```

without incrementing ReadingContextRevision.

Examples:

* same target;
* same language;
* same configuration;
* same logical position.

---

# 35. Candidate Reading Context

Before commit, Reading Session may construct:

```text
CandidateReadingContext
├── basedOnReadingContextRevision
├── candidateReadingContextRevision
├── readingSource
├── readingTarget?
├── readingPosition?
├── sessionConfiguration
└── changeSet
```

Candidate state is private and not authoritative.

---

# 36. Atomic Context Commit

A domain mutation commits atomically:

```text
ReadingContextRevision
+
ReadingContextSnapshot
+
current context reference
```

Consumers must never observe a partially changed ReadingContext.

---

# 37. Query — GetReadingSession

```text
GetReadingSession
- readingSessionId
```

Result:

```text
GetReadingSessionResult
├── found
└── readingSession?
```

---

# 38. Query — GetReadingContext

```text
GetReadingContext
- readingSessionId
```

Result:

```text
GetReadingContextResult
├── found
├── readingContextRevision?
└── contextSnapshot?
```

---

# 39. Query — GetReadingContextRevision

```text
GetReadingContextRevision
- readingSessionId
```

Result:

```text
GetReadingContextRevisionResult
├── found
└── readingContextRevision?
```

This replaces the old:

```text
GetActiveRevision
```

whose `ContentRevision` semantics were ambiguous.

---

# 40. Query — GetSessionConfiguration

```text
GetSessionConfiguration
- readingSessionId
```

Returns the current session-scoped effective configuration.

---

# 41. Query — GetSessionState

```text
GetSessionState
- readingSessionId
```

Returns Reading Session lifecycle state only.

It does not expose Runtime lifecycle.

---

# 42. Query — ListActiveSessions

```text
ListActiveSessions
```

Returns immutable session summaries.

Conceptually:

```text
ReadingSessionSummary
├── readingSessionId
├── lifecycleState
├── readingContextRevision
├── sourceKind
├── targetKind?
└── lastActivityAt?
```

---

# 43. Query — GetReadingContextAtRevision

Optional diagnostic/history query:

```text
GetReadingContextAtRevision
├── readingSessionId
└── readingContextRevision
```

Availability depends on retention policy.

Reading Session contract does not guarantee indefinite revision history.

---

# 44. Reading Context ChangeSet

```text
ReadingContextChangeSet
├── sourceChanged
├── targetChanged
├── positionChanged
├── sourceLanguageChanged
├── targetLanguageChanged
├── configurationChanged
└── lifecycleRelevantChange?
```

This describes committed domain change.

It does not tell Runtime what pipeline must execute.

---

# 45. Removed ProcessingIntent Contract

The following public contract is removed:

```text
ProcessingIntent
```

and values such as:

```text
CaptureRequired
RecognitionRequired
TextProcessingRequired
TranslationRequired
PresentationRefreshRequired
```

Reason:

pipeline requirement evaluation belongs to:

```text
Business Pipeline Orchestration
```

Reading Session exposes state/facts, not processing plans.

---

# 46. Reading-Domain Facts

Reading Session may publish immutable facts after domain commit.

Typical examples:

```text
ReadingSessionCreated
ReadingSessionActivated
ReadingContextChanged
ReadingTargetChanged
ReadingConfigurationChanged
ReadingSessionPaused
ReadingSessionResumed
ReadingSessionCompleted
ReadingSessionCancelled
ReadingSessionDisposed
```

Exact payloads belong to `EVENTS.md`.

---

# 47. No ProcessingIntentPublished

The old event:

```text
ProcessingIntentPublished
```

is removed from Reading Session ownership.

A pipeline orchestration layer may publish its own plan/decision facts if architecture requires them.

---

# 48. Events Are Not Commands

Reading Session event:

```text
ReadingTargetChanged
```

means:

```text
the committed reading target changed
```

It does not mean:

```text
run Capture
run OCR
run Translation
```

Consumers decide their own reactions according to ownership.

---

# 49. External Input Events

Reading Session does not require direct Event Bus subscriptions as its correctness mechanism.

External interactions such as:

```text
browser navigation
user selection
settings changes
session recovery
```

should normally be translated by Application/Adapters into explicit Reading Session commands.

This keeps the domain API deterministic and testable.

---

# 50. UI/Input Boundary

Reading Session commands accept normalized domain values.

They MUST NOT accept:

* DOM nodes;
* browser events;
* mouse events;
* keyboard events;
* HWND;
* native view objects.

Adapters convert those into ReadingSource/Target/Position semantics.

---

# 51. Preferences Boundary

Persistent user preference objects must not become mutable Reading Session state.

Preferred flow:

```text
Preferences
    ↓
resolved defaults
    ↓
Application
    ↓
Create/Update Reading Session
    ↓
session-specific effective configuration
```

---

# 52. Runtime Boundary

Reading Session public contracts contain no mandatory:

```text
RuntimeRevisionId
WorkItemId
AttemptId
SchedulerId
WorkerId
```

Runtime may correlate ReadingContextSnapshot externally.

Reading Session remains execution-independent.

---

# 53. Orchestration Boundary

Business Pipeline Orchestration may consume:

```text
ReadingContextSnapshot
ReadingContextChangeSet
accepted Artifact availability
capability state
pipeline policy
```

and decide required processing.

Those decisions do not mutate Reading Session unless a separate reading-domain command occurs.

---

# 54. Artifact Boundary

Reading Session does not expose processing result types as mutable state.

It does not own:

```text
CapturedFrameArtifact
RecognitionArtifact
SourceDocumentArtifact
TranslationArtifact
PresentationSnapshot
```

Artifact references may appear only where clearly useful for provenance/history and must remain externally owned.

---

# 55. Lifecycle States

Reading Session exposes lifecycle values such as:

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

Exact legal transitions belong to `STATES.md`.

---

# 56. Lifecycle vs Context Revision

A lifecycle transition and ReadingContextRevision change are separate.

Example:

```text
ACTIVE
ReadingContextRevision = 20
```

may become:

```text
PAUSED
ReadingContextRevision = 20
```

if pause does not change ReadingContext semantics.

The exact rule is defined in `STATES.md`.

---

# 57. Reading Context Authority

Reading Session is authoritative only for:

```text
current committed ReadingContext
```

This authority does not extend to:

```text
Runtime execution authority
Artifact publication authority
Presentation commit authority
```

---

# 58. Security Contract

Public Reading Session contracts MUST NOT expose:

* browser cookies;
* auth tokens;
* provider secrets;
* Runtime credentials;
* worker topology;
* native handles;
* storage credentials;
* raw private page content unless domain semantics explicitly require bounded content.

---

# 59. Privacy Contract

Reading Session contract values should prefer:

```text
logical source identifiers
target identifiers
reading position
language
bounded metadata
```

over raw content.

Normal error/diagnostic payloads must not include:

* full page text;
* screenshots;
* HTML;
* translation content.

---

# 60. Error Contract

Reading Session errors represent reading-domain failures.

Typical categories:

```text
Validation
Context
ReadingContextRevision
Lifecycle
Configuration
Ownership
Recovery
Internal
```

They do not represent:

```text
Runtime timeout
Translation failure
OCR failure
UI rendering failure
Artifact publication failure
```

Detailed codes belong to `ERRORS.md`.

---

# 61. ReadingContextRevision Conflict

Conceptual rejection:

```text
ReadingContextRevisionConflict
├── readingSessionId
├── expectedReadingContextRevision
├── currentReadingContextRevision
└── retryHint?
```

It is normal optimistic concurrency behavior.

It does not terminate the Reading Session.

---

# 62. Session Disposal Contract

After `DISPOSED`:

```text
no mutation command may succeed
```

Queries may return:

* tombstone summary;
* not found;
* retained immutable metadata;

depending on retention policy.

A disposed session never becomes ACTIVE again.

---

# 63. Restoration Contract

Stored state may be reconstructed through explicit restoration/application flow.

A persisted record must not become authoritative automatically.

Conceptually:

```text
stored session data
    ↓
validate
    ↓
Candidate ReadingSession
    ↓
commit restored state
```

Storage implementation remains external.

---

# 64. Compatibility

Semantic versioning:

```text
MAJOR.MINOR.PATCH
```

Major version required for:

* changing ReadingContextRevision meaning;
* removing ProcessingIntent;
* changing ownership;
* changing lifecycle semantics;
* changing required public fields incompatibly.

This migration from v2 to v3 is a major version change.

---

# 65. Unknown Fields

Unknown optional fields should be ignored when safe.

Unknown required enum values must:

* reject;
* or use explicitly documented compatibility fallback.

---

# 66. Stable Identifiers

Stable public identifiers include:

```text
ReadingSessionId
ReadingContextRevision
ReadingSourceId
ReadingTargetId
```

They remain immutable after creation.

---

# 67. Architecture Invariants

1. Commands modify Reading Session business state only.

2. Queries never mutate state.

3. ReadingSessionId is stable.

4. ReadingContextSnapshot is immutable.

5. ReadingContextRevision is immutable once committed.

6. ReadingContextRevision is monotonic within one session.

7. Reading Session owns only ReadingContextRevision.

8. ContentRevision is deprecated as an overloaded authority term.

9. RuntimeRevisionId is externally owned.

10. Reading Session does not expose ProcessingIntent.

11. Reading Session does not decide processing topology.

12. Business Pipeline Orchestration decides processing requirements.

13. Reading Session does not schedule work.

14. Reading Session does not retry work.

15. Reading Session does not cancel Runtime Attempts directly.

16. Reading Session does not accept/reject processing completion.

17. Reading Session does not own processing Artifacts.

18. Reading Session does not own Presentation state.

19. Reading Session does not own native UI state.

20. Technical viewport is not authoritative Reading Session state by default.

21. Session-specific configuration is separate from persistent Preferences.

22. Candidate ReadingContext is not committed state.

23. Context mutation is atomic.

24. No-op mutation does not create unnecessary revision.

25. Optimistic concurrency uses ReadingContextRevision only.

26. Runtime revision and ReadingContextRevision are never treated as the same authority.

27. Domain events publish after committed domain state.

28. Reading Session does not require Event Bus subscriptions for correctness.

29. External input is normalized into commands.

30. Session disposal is irreversible.

---

# 68. Example — Create Session

```text
CreateReadingSession
    ↓
validate source/configuration
    ↓
Candidate ReadingSession
    ↓
commit
    ↓
ReadingContextRevision = 1
    ↓
ReadingSessionCreated
```

No processing starts automatically.

---

# 69. Example — Page Change

```text
Current:
ReadingContextRevision = 10
Target = Page 4

UpdateReadingTarget(Page 5)
expected = 10
    ↓
validate
    ↓
commit
    ↓
ReadingContextRevision = 11
    ↓
ReadingTargetChanged
```

Pipeline consequences are evaluated outside Reading Session.

---

# 70. Example — Concurrent Update

```text
Current = Revision 20

Command A expects 20
Command B expects 20

B commits Revision 21

A reaches commit
    ↓
expected 20 != current 21
    ↓
ReadingContextRevisionConflict
```

No Runtime state is mutated.

---

# 71. Example — Raw Viewport Change

```text
Browser/UI emits many viewport changes
    ↓
Adapter/Application normalizes/coalesces
    ↓
business-significant reading target/position changed?
    ├── no → no Reading Session command
    └── yes
          ↓
      UpdateReadingPosition / Target
```

Presentation-only reflow may bypass Reading Session entirely.

---

# 72. Example — Target Language Change

```text
UpdateSessionConfiguration
targetLanguage = EN
    ↓
Reading Session commit
    ↓
ReadingContextRevision N+1
    ↓
ReadingConfigurationChanged
```

Then:

```text
Business Pipeline Orchestration
    ↓
decides whether Translation work is required
```

---

# 73. Example — Cancel Reading Session

```text
CancelReadingSession
    ↓
Reading Session lifecycle becomes CANCELLED
    ↓
ReadingSessionCancelled
```

Then separately:

```text
Application / Runtime
    ↓
decides what execution cancellation/supersession is required
```

Reading Session does not revoke Runtime authority directly.

---

# 74. Example — Completion

```text
CompleteReadingSession
    ↓
COMPLETING
    ↓
COMPLETED
    ↓
ReadingSessionCompleted
```

Runtime, Presentation, UI, and persistence owners respond through their own contracts.

---

# 75. Related Documents

```text
doc/02-modules/reading-session/MODULE.md
doc/02-modules/reading-session/STATES.md
doc/02-modules/reading-session/EVENTS.md
doc/02-modules/reading-session/ERRORS.md
doc/02-modules/reading-session/README.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md

doc/02-modules/preferences/CONTRACT.md
doc/02-modules/presentation/CONTRACT.md
```

---

# 76. Completion Criteria

This contract is synchronized when:

* public Reading Session contracts contain only reading-domain concepts;
* `ContentRevision` has been replaced by `ReadingContextRevision`;
* `ProcessingIntent` has been removed;
* pipeline decisions are absent from Reading Session API;
* Runtime authority concepts are absent from Reading Session ownership;
* commands use ReadingContextRevision optimistic concurrency;
* raw viewport/framework objects are absent;
* ReadingContextSnapshot is immutable;
* domain mutation is atomic;
* no-op semantics are explicit;
* lifecycle and context revision remain separate;
* events describe committed reading facts only;
* processing consequences remain external;
* contracts remain serializable and implementation-independent.

---

# 77. Summary

The Reading Session contract is:

```text
Application Reading Intent
        ↓
Reading Session Command
        ↓
Reading Session Domain Validation
        ↓
Candidate Reading Context
        ↓
Atomic Commit
        ↓
ReadingContextRevision
        +
ReadingContextSnapshot
        ↓
Reading Domain Fact / Query
```

Ownership remains:

```text
Reading Session
    → reading-domain state

Business Pipeline Orchestration
    → processing requirement decisions

Runtime Control
    → execution revision and authority

Processing Modules
    → processing semantics/results

Presentation
    → committed presentation state
```

The central contract rule is:

```text
ReadingContextRevision tells CRAI
which reading-domain state is current.

It does not tell Runtime
which asynchronous execution may commit.
```
