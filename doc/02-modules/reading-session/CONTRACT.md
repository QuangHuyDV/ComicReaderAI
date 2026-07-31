# Reading Session Contract

- Module: Reading Session
- Identifier: reading-session
- Layer: Business Orchestration
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

This document defines the public contract exposed by the Reading Session Module.

Unlike MODULE.md, which describes architectural responsibilities and business boundaries, this document specifies the interfaces that other modules are allowed to depend upon.

Every interaction with Reading Session must occur through the contracts defined here.

No module may access internal Reading Session state directly.

The purpose of this contract is to provide:

- stable communication
- implementation independence
- version compatibility
- explicit ownership
- deterministic interaction

This document intentionally avoids describing runtime implementation.

Execution details belong to Runtime Architecture.

---

# 2. Contract Philosophy

The Reading Session Contract follows several architectural principles.

## 2.1 Contract Before Implementation

Modules communicate through contracts.

They never communicate through implementation.

A module consuming Reading Session must not require knowledge about:

- internal storage
- internal state machine
- runtime scheduler
- worker topology
- implementation language

Only the public contract is visible.

---

## 2.2 Stable Business Interface

The public interface should evolve significantly slower than implementation.

Internal algorithms may change.

Internal optimization may change.

Runtime behavior may change.

The public contract should remain stable whenever possible.

---

## 2.3 Explicit Ownership

A contract exposes only concepts owned by Reading Session.

Objects owned by Runtime or Processing Modules must never appear as mutable business objects inside this contract.

---

## 2.4 Immutable Communication

Every request and every published event represents an immutable business fact.

Consumers must never assume that previously received objects may change.

If business state changes, a new object is produced.

---

## 2.5 Runtime Independence

The contract never specifies:

- execution order
- execution priority
- scheduling policy
- retry strategy
- batching strategy
- worker assignment

These belong exclusively to Runtime Architecture.

---

# 3. Public Interface

Reading Session exposes two categories of public operations.

```text
Public Interface

├── Commands
└── Queries
```

Commands modify business state.

Queries observe business state.

Neither category exposes implementation details.

---

# 4. Command Contracts

Commands express business intentions.

A command requests Reading Session to evaluate or modify the reading domain.

Commands never request processing execution directly.

---

## 4.1 CreateReadingSession

### Purpose

Create a new Reading Session.

### Required Data

- Reading Source
- Source Type
- Initial Configuration

### Result

Returns:

- Session Identifier
- Initial ContentRevision
- Initial Session State

### Notes

The command creates a business session.

It does not guarantee that processing begins immediately.

---

## 4.2 ActivateReadingSession

### Purpose

Activate a previously created Reading Session.

### Result

The session becomes eligible to produce Processing Intent.

No runtime execution is implied.

---

## 4.3 UpdateReadingContext

### Purpose

Update the current reading context.

Typical causes include:

- page navigation
- viewport movement
- chapter change
- source replacement
- user selection
- configuration update

### Result

Reading Session evaluates whether a new ContentRevision is required.

If business state changes,

a new immutable ContentRevision is produced.

---

## 4.4 UpdateConfiguration

### Purpose

Modify session configuration.

Examples include:

- source language
- target language
- translation mode
- OCR preference
- reading mode

### Result

Configuration is updated.

Reading Session determines whether downstream business intent changes.

---

## 4.5 PauseReadingSession

### Purpose

Suspend business progression.

### Result

No additional Processing Intent is produced while the session remains paused.

Already executing work is handled by Runtime.

---

## 4.6 ResumeReadingSession

### Purpose

Resume a paused Reading Session.

### Result

Business evaluation resumes.

A new ContentRevision may be generated if the reading context has changed.

---

## 4.7 CompleteReadingSession

### Purpose

Finish a Reading Session normally.

### Result

The session enters its completion lifecycle.

No additional business intent is produced.

---

## 4.8 CancelReadingSession

### Purpose

Cancel an active Reading Session.

### Result

Current business authority is revoked.

Runtime receives cancellation requests through its own contracts.

Reading Session itself performs no execution cancellation.

---

## 4.9 DisposeReadingSession

### Purpose

Release business resources belonging to a completed session.

### Result

The session becomes permanently disposed.

Disposed sessions cannot be resumed.

---

# 5. Query Contracts

Queries expose immutable business information.

Queries never mutate state.

---

## 5.1 GetReadingSession

Returns:

- Session Identifier
- Session State
- Active ContentRevision
- Active Reading Source
- Active Reading Target

---

## 5.2 GetReadingContext

Returns the current Reading Context.

The returned object represents the latest accepted business state.

---

## 5.3 GetSessionConfiguration

Returns the effective configuration currently associated with the session.

---

## 5.4 GetActiveRevision

Returns the current active ContentRevision.

If no session exists,

no revision is returned.

---

## 5.5 GetSessionState

Returns the current lifecycle state.

Possible values are defined by STATES.md.

---

## 5.6 ListActiveSessions

Returns every active Reading Session.

Current implementations may expose a single session.

Future versions may support multiple concurrent sessions without changing this contract.

---

# 6. Data Contracts

The Reading Session Module exposes several immutable business objects.

No consumer may modify them.

---

## 6.1 ReadingSession

Represents an active reading activity.

Contains:

- SessionId
- LifecycleState
- ActiveContext
- ActiveRevision
- Configuration
- Metadata

The object never exposes Runtime information.

---

## 6.2 ReadingContext

Represents the current business understanding of what the user is reading.

Contains:

- ReadingSource
- ReadingTarget
- CurrentPage
- CurrentChapter
- SourceLanguage
- TargetLanguage
- ReadingMode
- Viewport
- ActiveRevision

ReadingContext is the authoritative business snapshot.

---

## 6.3 ContentRevision

Represents an immutable revision of ReadingContext.

Contains:

- RevisionId
- SessionId
- ParentRevision
- CreatedAt
- RevisionReason

A ContentRevision never changes after creation.

---

## 6.4 SessionConfiguration

Contains all runtime-independent business configuration.

Examples include:

- translation preference
- OCR preference
- reading direction
- presentation preference
- provider preference

Configuration changes may create new ContentRevisions.

---

## 6.5 ProcessingIntent

ProcessingIntent expresses business requirements.

Typical values include:

- Capture Required
- Recognition Required
- Text Processing Required
- Translation Required
- Presentation Refresh Required

ProcessingIntent is not executable work.

It is business intent consumed by Runtime.

---

# 7. Command Validation Contract

Every command must be validated before modifying business state.

Validation occurs before any ContentRevision is created.

Typical validation includes:

- session existence
- lifecycle validity
- configuration validity
- source validity
- ownership validation
- state transition validation

Invalid commands produce no business changes.

---

# 8. Event Contracts

Events are immutable business facts published by the Reading Session Module.

Unlike commands, events cannot be rejected or modified after publication.

Events describe business state changes.

They never describe execution progress.

---

## 8.1 Event Philosophy

Reading Session publishes only events representing changes within the reading domain.

Examples include:

- session lifecycle changes
- reading context changes
- configuration changes
- ContentRevision creation
- ProcessingIntent publication

Execution events remain outside this module.

---

## 8.2 Published Events

The Reading Session Module may publish the following events.

| Event | Description |
|---------|-------------|
| ReadingSessionCreated | A new session has been created |
| ReadingSessionActivated | Session becomes active |
| ReadingSessionUpdated | Reading context changed |
| ReadingSessionPaused | Session paused |
| ReadingSessionResumed | Session resumed |
| ReadingSessionCompleted | Session completed normally |
| ReadingSessionCancelled | Session cancelled |
| ReadingSessionDisposed | Session permanently disposed |
| ReadingContextChanged | Reading context changed |
| ContentRevisionCreated | New immutable revision created |
| ConfigurationUpdated | Configuration changed |
| ProcessingIntentPublished | New business intent produced |

Additional events may be introduced in future versions without breaking compatibility.

---

## 8.3 Consumed Events

Reading Session consumes business events originating from upstream systems.

Typical examples include:

| Event | Origin |
|---------|--------|
| UserStartedReading | UI |
| UserStoppedReading | UI |
| BrowserNavigated | Browser Adapter |
| ViewportChanged | Browser Adapter |
| ChapterChanged | Browser Adapter |
| SourceChanged | Browser Adapter |
| ConfigurationChanged | Settings |
| SessionRecovered | Session Recovery |

Reading Session does not consume worker execution events directly.

Execution completion belongs to Runtime contracts.

---

## 8.4 Event Ordering

Events are published in business order.

Example:

```text
ReadingSessionCreated

↓

ReadingSessionActivated

↓

ReadingContextChanged

↓

ContentRevisionCreated

↓

ProcessingIntentPublished
```

Consumers must never assume execution completion based solely on event order.

---

## 8.5 Event Immutability

Every published event is immutable.

If business state changes,

a new event is published.

Previously published events remain historically valid.

---

# 9. State Contracts

The Reading Session Contract exposes only business lifecycle states.

Execution state remains internal to Runtime.

---

## 9.1 Session States

Possible lifecycle states include:

```text
Created

Initializing

Active

Paused

Completing

Completed

Cancelled

Disposed
```

The semantics of these states are defined by STATES.md.

---

## 9.2 Context State

A Reading Session always exposes exactly one active ReadingContext.

Consumers must never observe multiple active contexts simultaneously.

---

## 9.3 Revision State

Exactly one ContentRevision is considered active.

Previous revisions remain immutable but lose authority.

---

## 9.4 Configuration State

Configuration always represents the latest accepted business configuration.

Configuration never exists in a partially updated state.

---

# 10. Revision Contract

ContentRevision is the synchronization mechanism between business state and downstream processing.

Every ProcessingIntent references exactly one ContentRevision.

---

## 10.1 Revision Rules

The following rules are mandatory.

- revisions are immutable
- revisions are monotonically increasing
- revisions belong to one ReadingSession
- revisions cannot be modified
- revisions cannot be reused
- revisions cannot change ownership

---

## 10.2 Active Revision

Only one ContentRevision is active.

When a new revision is accepted,

all previous revisions become obsolete.

Obsolete revisions remain historically valid.

---

## 10.3 Revision Visibility

Consumers should always operate using the latest available revision.

Older revisions may still exist,

but should not be treated as authoritative.

---

## 10.4 Revision Compatibility

Future revisions may extend metadata.

Existing fields must preserve their semantics whenever possible.

---

# 11. Ownership Contract

Ownership defines authority.

Authority determines which module may modify business concepts.

---

## 11.1 Reading Session Authority

Reading Session owns:

- ReadingSession
- ReadingContext
- ContentRevision
- SessionConfiguration
- ProcessingIntent

No external module may mutate these objects.

---

## 11.2 Runtime Authority

Runtime owns:

- execution queue
- scheduler
- execution lifecycle
- retry
- timeout
- cancellation propagation
- worker lifecycle

These concepts are intentionally absent from this contract.

---

## 11.3 Processing Authority

Processing modules own only processing results.

Examples include:

Capture

- CaptureResult

Recognition

- OCRResult

Translation

- TranslationResult

Presentation

- PresentationSnapshot

These objects never become mutable Reading Session state.

---

# 12. Compatibility Contract

The Reading Session Contract is intended to remain stable across multiple architectural versions.

---

## 12.1 Backward Compatibility

Minor releases should preserve compatibility whenever possible.

Breaking changes require a new contract version.

---

## 12.2 Forward Compatibility

Consumers should ignore unknown optional fields.

Future versions may extend contracts without invalidating older implementations.

---

## 12.3 Stable Identifiers

The following identifiers are stable.

- SessionId
- RevisionId

Identifiers never change after creation.

---

# 13. Error Contract

Errors returned by Reading Session represent business failures.

Execution failures belong to Runtime.

---

## 13.1 Validation Errors

Examples:

- invalid source
- invalid configuration
- invalid language
- invalid reading mode

No business state changes occur.

---

## 13.2 State Errors

Examples:

- session already disposed
- session already active
- invalid lifecycle transition
- operation not permitted

---

## 13.3 Revision Errors

Examples:

- obsolete revision
- missing revision
- invalid revision reference

---

## 13.4 Ownership Errors

Examples:

- unauthorized mutation
- illegal context modification
- invalid ownership violation

---

## 13.5 Internal Errors

Unexpected failures inside Reading Session.

Consumers should treat these as non-deterministic failures.

Detailed classifications are defined in ERRORS.md.

---

# 14. Security Contract

The Reading Session Module must not expose implementation-sensitive information.

The public contract must never contain:

- browser cookies
- authentication tokens
- runtime credentials
- provider secrets
- execution topology
- worker identifiers
- internal scheduler metadata

Only business information may cross the module boundary.

---

# 15. Architecture Constraints

The following constraints are mandatory for every implementation of this contract.

1. Commands modify business state only.

2. Queries never modify business state.

3. Every published event is immutable.

4. Every ProcessingIntent references one ContentRevision.

5. Reading Session never exposes Runtime internals.

6. Reading Session never exposes Processing Module internals.

7. Runtime never mutates ReadingContext.

8. Processing modules never mutate ReadingContext.

9. Reading Session never performs processing execution.

10. Reading Session never performs scheduling.

11. Runtime never owns business state.

12. Business communication occurs exclusively through contracts.

13. Every contract object has exactly one owner.

14. Obsolete ContentRevisions never regain authority.

15. Session disposal is irreversible.

---

# 16. Related Documents

This contract is defined in conjunction with the remaining Reading Session specifications.

- MODULE.md
- STATES.md
- EVENTS.md
- ERRORS.md
- README.md

The documents are complementary.

MODULE.md defines architectural responsibilities.

CONTRACT.md defines public interfaces.

STATES.md defines lifecycle semantics.

EVENTS.md defines business event specifications.

ERRORS.md defines business failure semantics.

README.md provides module overview and navigation.

No document may redefine concepts owned by another document.

---

# End of Document