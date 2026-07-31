# Reading Session Events

- Module: Reading Session
- Identifier: reading-session
- Layer: Business Orchestration
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

This document defines all business events published and consumed by the Reading Session Module.

Unlike STATES.md, which defines business lifecycle,

EVENTS.md defines business facts.

An event represents something that has already happened.

Events never describe future work.

Events never describe execution progress.

This specification defines:

- business event ownership
- event categories
- event contracts
- event ordering
- event versioning
- event delivery guarantees
- event invariants

---

# 2. Event Philosophy

Reading Session follows an event-driven architecture.

Business changes are communicated exclusively through immutable events.

Every event represents a completed business fact.

Examples:

✔ Reading Context changed

✔ New Content Revision exists

✔ Session paused

Examples that are NOT business events:

✘ OCR started

✘ Translation running

✘ Worker queued

✘ GPU selected

Those belong to Runtime.

---

## 2.1 Events Describe Facts

Events never describe commands.

Incorrect:

```text
StartTranslation
```

Correct:

```text
ProcessingIntentPublished
```

---

## 2.2 Events Are Immutable

Once published,

an event never changes.

Corrections always generate new events.

Existing events remain historically valid.

---

## 2.3 Events Never Own State

An event reports that state changed.

The event itself never becomes the source of truth.

Source of truth always belongs to:

```text
ReadingSession

ReadingContext

ContentRevision

ProcessingIntent
```

---

# 3. Event Ownership

Reading Session owns only business events.

---

## Reading Session Publishes

```text
ReadingSession*

ReadingContext*

ContentRevision*

ProcessingIntent*

Configuration*
```

---

## Reading Session Never Publishes

```text
Capture*

Recognition*

Translation*

Presentation*

Worker*

Scheduler*

Queue*

Provider*
```

Those belong to other modules.

---

# 4. Event Categories

Business events are grouped by ownership.

```text
Reading Session

├── Session Lifecycle Events
│
├── Reading Context Events
│
├── Content Revision Events
│
├── Processing Intent Events
│
└── Configuration Events
```

Each category corresponds to exactly one business object.

---

# 5. Event Naming Rules

All event names follow the same convention.

```text
BusinessObject + Past Tense
```

Examples:

```text
ReadingSessionCreated

ReadingContextUpdated

ContentRevisionSuperseded

ProcessingIntentPublished
```

Avoid imperative names.

Incorrect:

```text
CreateRevision

StartTranslation

RunOCR

RestartPipeline
```

Those represent commands rather than facts.

---

# 6. Event Identity

Every business event has its own immutable identity.

Conceptually:

```text
BusinessEvent

├── EventId
├── EventType
├── AggregateId
├── AggregateVersion
├── OccurredAt
├── CorrelationId
├── CausationId
└── Payload
```

Event identity never changes.

---

# 7. Event Versioning

Business events evolve over time.

Older versions remain understandable.

Breaking changes require new event versions.

Example:

```text
ReadingContextUpdated v1

↓

ReadingContextUpdated v2
```

Existing published events are never rewritten.

---

# 8. Event Categories Overview

The Reading Session module defines five categories.

```text
ReadingSessionEvents

ReadingContextEvents

ContentRevisionEvents

ProcessingIntentEvents

ConfigurationEvents
```

Each category is described separately below.

---

# 9. Session Lifecycle Events

Session Lifecycle events describe changes to the Reading Session itself.

They never describe processing progress.

Defined events:

```text
ReadingSessionCreated

ReadingSessionActivated

ReadingSessionPaused

ReadingSessionResumed

ReadingSessionCompleted

ReadingSessionCancelled

ReadingSessionDisposed
```

Each event corresponds to a transition defined in STATES.md.

No additional lifecycle events should exist outside this list.

---

# 10. ReadingSessionCreated

Published when a new Reading Session is successfully created.

Meaning:

- Session identity exists.
- Business lifecycle begins.
- Configuration has been accepted.

Typical payload:

```text
SessionId

CreatedAt

ConfigurationVersion
```

This is always the first event in a session history.

---

# 11. ReadingSessionActivated

Published when the Session becomes Active.

Typical causes:

- initialization completed
- initial context established

Business meaning:

The Reading Session is now allowed to produce Reading Context updates and Processing Intents.

This event does not imply Runtime execution has begun.

---

# 12. ReadingSessionPaused

Published when business progression is paused.

Business meaning:

No further ProcessingIntent objects should be generated until resumed.

Previously published intents remain historically valid.

---

# 13. ReadingSessionResumed

Published when a paused session becomes Active again.

Business evaluation resumes from the current Reading Context.

Historical revisions remain unchanged.

---

# 14. ReadingSessionCompleted

Published when the reading activity finishes naturally.

Examples:

- end of reading session
- user exits normally
- source completed

After this event,

no new ContentRevision may become Current.

---

# 15. ReadingSessionCancelled

Published when the Session ends abnormally.

Typical causes:

- explicit cancellation
- source unavailable
- unrecoverable business condition

Cancellation never rewrites previous business history.

---

# 16. ReadingSessionDisposed

Published when all business resources have been released.

This is the final lifecycle event.

No further Session Lifecycle events may follow.

---

# 17. Reading Context Events

Reading Context events describe changes to the current business understanding of what the user is reading.

These events are produced whenever the ReadingContext changes.

They do not describe how the change was detected.

Detection belongs to Runtime.

Reading Session only publishes the resulting business fact.

Defined events:

```text
ReadingContextLoading

ReadingContextReady

ReadingContextUpdated

ReadingContextInvalidated

ReadingContextDisposed
```

---

# 18. ReadingContextLoading

Published when Reading Session begins constructing a new Reading Context.

Typical causes:

- session activation
- source change
- chapter navigation
- page navigation
- viewport reconstruction

Business meaning:

A new Reading Context is being established.

This event does not indicate that the context is already usable.

---

# 19. ReadingContextReady

Published when the Reading Context becomes authoritative.

Business meaning:

The current reading world has been successfully identified.

After this event,

Reading Session may create a new ContentRevision.

Typical payload:

```text
SessionId

ContextId

SourceId

PageId

ChapterId

ContextVersion
```

Only one ReadingContext may be Ready at any given moment.

---

# 20. ReadingContextUpdated

Published whenever the business understanding of the reading world changes.

Examples:

- page changed
- chapter changed
- reading direction changed
- viewport changed
- active source changed

Business meaning:

The previously accepted Reading Context is no longer current.

A new business evaluation is required.

This event commonly leads to:

```text
ContentRevisionCreated
```

---

# 21. ReadingContextInvalidated

Published when the current Reading Context can no longer represent reality.

Examples:

- source removed
- unsupported document
- inconsistent metadata
- invalid reading position

Business meaning:

ProcessingIntent generation must stop until a valid context exists.

No Runtime behavior is implied.

---

# 22. ReadingContextDisposed

Published when the Reading Context is permanently released.

Typical causes:

- session disposal
- context replacement
- resource cleanup

Disposed contexts never become Ready again.

---

# 23. Content Revision Events

Content Revision events describe changes to immutable snapshots of the reading world.

Unlike Reading Context,

a ContentRevision never changes after creation.

Instead,

new revisions replace older business authority.

Defined events:

```text
ContentRevisionCreated

ContentRevisionActivated

ContentRevisionSuperseded

ContentRevisionArchived

ContentRevisionDiscarded
```

---

# 24. ContentRevisionCreated

Published whenever a new immutable revision is created.

Typical causes:

- Reading Context updated
- configuration changed
- language changed
- reading mode changed

Business meaning:

A new snapshot of the reading world now exists.

The revision is immutable from this moment onward.

Typical payload:

```text
RevisionId

SessionId

RevisionNumber

CreatedAt
```

Creation does not automatically make the revision authoritative.

---

# 25. ContentRevisionActivated

Published when a revision becomes the current business authority.

Business meaning:

This revision replaces the previously active revision.

Only one revision may be active.

Immediately after activation,

future ProcessingIntent objects reference this revision.

---

# 26. ContentRevisionSuperseded

Published when a newer revision replaces an existing one.

Business meaning:

The previous revision remains historically valid,

but it is no longer authoritative.

Superseded revisions never become active again.

Typical consumers:

- history storage
- analytics
- diagnostics
- audit timeline

---

# 27. ContentRevisionArchived

Published when a historical revision moves into long-term storage.

Business meaning:

The revision remains available for historical inspection.

It no longer participates in active business evaluation.

Archiving never changes revision contents.

---

# 28. ContentRevisionDiscarded

Published when a revision is permanently removed.

Business meaning:

The revision is no longer retained by the system.

Discarded revisions cannot be restored through business operations.

---

# 29. Revision Event Ordering

The typical lifecycle of a revision is:

```text
ContentRevisionCreated

↓

ContentRevisionActivated

↓

ContentRevisionSuperseded

↓

ContentRevisionArchived

↓

ContentRevisionDiscarded
```

Not every revision reaches every stage.

For example,

a revision may be discarded directly after creation if it becomes invalid before activation.

---

# 30. Revision Event Rules

The following guarantees always apply.

1. Every revision produces exactly one `ContentRevisionCreated` event.

2. A revision may produce at most one `ContentRevisionActivated` event.

3. Only one revision is active within a Reading Session.

4. Superseded revisions remain immutable.

5. Archived revisions never become active.

6. Discarded revisions never generate additional events.

---

# 31. Processing Intent Events

Processing Intent events describe the lifecycle of business intentions generated from a ContentRevision.

These events do not describe execution.

They describe only what business outcomes are required.

Runtime is responsible for deciding how those outcomes are achieved.

Defined events:

```text
ProcessingIntentCreated

ProcessingIntentPublished

ProcessingIntentAccepted

ProcessingIntentFulfilled

ProcessingIntentObsoleted

ProcessingIntentDiscarded
```

---

# 32. ProcessingIntentCreated

Published when Reading Session generates a new ProcessingIntent.

Typical causes:

- new ContentRevision activated
- business configuration changed
- language preference changed
- reading mode changed

Business meaning:

A new business requirement has been identified.

The ProcessingIntent is still private to Reading Session.

Typical payload:

```text
IntentId

RevisionId

IntentType

CreatedAt
```

Creation does not imply Runtime awareness.

---

# 33. ProcessingIntentPublished

Published when the ProcessingIntent becomes visible outside Reading Session.

Business meaning:

Reading Session has declared what business outcome is required.

Responsibility for execution may now be assumed by Runtime.

Typical payload:

```text
IntentId

RevisionId

IntentVersion

PublishedAt
```

Publishing never guarantees execution.

It only guarantees visibility.

---

# 34. ProcessingIntentAccepted

Published when Runtime acknowledges responsibility for the ProcessingIntent.

Business meaning:

The business request has been accepted for execution.

Reading Session still does not know:

- execution order
- scheduling policy
- worker allocation
- provider selection

Those remain Runtime responsibilities.

This event exists solely to establish business acknowledgement.

---

# 35. ProcessingIntentFulfilled

Published when the requested business outcome has been achieved.

Business meaning:

The ProcessingIntent no longer requires additional business action.

Examples:

- translated content is available
- presentation artifact exists
- requested reading result has been produced

Reading Session evaluates only the existence of the business outcome.

It does not evaluate how that outcome was produced.

Fulfilled ProcessingIntent objects remain historical facts.

---

# 36. ProcessingIntentObsoleted

Published when an existing ProcessingIntent is no longer relevant.

Typical causes:

- newer ContentRevision activated
- Reading Context changed
- Session completed
- Session cancelled
- configuration replaced

Business meaning:

The original business objective should no longer be considered current.

Obsolete intents never become Fulfilled afterward.

---

# 37. ProcessingIntentDiscarded

Published when a ProcessingIntent is permanently removed.

Business meaning:

The intent is no longer retained by Reading Session.

Discarded intents never produce additional business events.

---

# 38. Processing Intent Event Ordering

The normal lifecycle is:

```text
ProcessingIntentCreated

↓

ProcessingIntentPublished

↓

ProcessingIntentAccepted

↓

ProcessingIntentFulfilled
```

Replacement path:

```text
ProcessingIntentCreated

↓

ProcessingIntentPublished

↓

ProcessingIntentObsoleted

↓

ProcessingIntentDiscarded
```

Replacement may occur at any point before fulfillment.

---

# 39. Processing Intent Rules

The following guarantees always apply.

1. Every ProcessingIntent produces exactly one `ProcessingIntentCreated` event.

2. Every published ProcessingIntent references exactly one ContentRevision.

3. Fulfilled intents never become obsolete.

4. Obsolete intents never become fulfilled.

5. ProcessingIntent events are append-only.

6. Runtime never changes business ownership.

---

# 40. Configuration Events

Configuration events describe changes to business configuration affecting the reading experience.

These events represent business facts.

They never instruct Runtime to perform a particular implementation.

Defined events:

```text
ConfigurationUpdated

LanguageChanged

ReadingModeChanged

TranslationModeChanged

SourceChanged
```

---

# 41. ConfigurationUpdated

Published whenever SessionConfiguration changes.

Examples:

- OCR preference updated
- translation preference updated
- presentation preference updated
- quality profile changed

Business meaning:

Future ContentRevision evaluation uses the new configuration.

Existing historical revisions remain unchanged.

---

# 42. LanguageChanged

Published when the preferred reading language changes.

Examples:

```text
Chinese

↓

Vietnamese
```

or

```text
Japanese

↓

English
```

Business meaning:

Future ProcessingIntent objects must reflect the new language preference.

Previously fulfilled ProcessingIntent objects remain historically correct.

---

# 43. ReadingModeChanged

Published when the reading mode changes.

Examples:

```text
Comic

↓

Novel
```

or

```text
Vertical

↓

Horizontal
```

Business meaning:

The business interpretation of the reading experience has changed.

Reading Session may create a new ContentRevision.

---

# 44. TranslationModeChanged

Published when the preferred translation strategy changes.

Examples:

```text
Image Overlay

↓

Text View
```

or

```text
Automatic

↓

Manual
```

Business meaning:

Future ProcessingIntent generation follows the new translation strategy.

Existing revisions remain immutable.

---

# 45. SourceChanged

Published when the active reading source changes.

Examples:

- browser tab switched
- different chapter opened
- different document selected
- different manga loaded

Business meaning:

The current Reading Context is no longer authoritative.

SourceChanged commonly leads to the following event sequence:

```text
SourceChanged

↓

ReadingContextUpdated

↓

ContentRevisionCreated

↓

ContentRevisionActivated

↓

ProcessingIntentCreated

↓

ProcessingIntentPublished
```

SourceChanged never implies a Runtime restart.

Only the business world has changed.

---

# 46. Event Ordering

Reading Session guarantees deterministic ordering of business events.

Consumers must observe events in the same logical order in which business facts occur.

Ordering is defined by business causality,

not by transport implementation.

---

## 46.1 Ordering Principles

Business events are ordered according to cause and effect.

For example,

a ContentRevision cannot become active before it exists.

Likewise,

a ProcessingIntent cannot be published before it has been created.

Ordering must therefore reflect business reality.

---

## 46.2 Typical Session Startup

A newly created Reading Session typically produces the following sequence.

```text
ReadingSessionCreated

↓

ReadingSessionActivated

↓

ReadingContextLoading

↓

ReadingContextReady

↓

ContentRevisionCreated

↓

ContentRevisionActivated

↓

ProcessingIntentCreated

↓

ProcessingIntentPublished
```

Every event represents a completed business fact.

---

## 46.3 Page Navigation

When the user changes page,

the business ordering is:

```text
ReadingContextUpdated

↓

ContentRevisionCreated

↓

ContentRevisionActivated

↓

ContentRevisionSuperseded

↓

ProcessingIntentCreated

↓

ProcessingIntentPublished

↓

ProcessingIntentObsoleted
```

The exact number of obsolete ProcessingIntent objects depends on business history.

---

## 46.4 Language Change

Changing the preferred language typically produces:

```text
LanguageChanged

↓

ContentRevisionCreated

↓

ContentRevisionActivated

↓

ProcessingIntentCreated

↓

ProcessingIntentPublished
```

Previously fulfilled business history remains unchanged.

---

## 46.5 Session Completion

A normal reading session may end with:

```text
ReadingSessionCompleted

↓

ReadingSessionDisposed
```

No additional business events are produced afterward.

---

# 47. Event Delivery

Reading Session defines semantic delivery requirements.

Transport technology is implementation dependent.

Examples include:

- Event Bus
- Message Queue
- Local Dispatcher
- In-Process Event Publisher

The architecture defines guarantees,

not infrastructure.

---

## 47.1 Delivery Guarantees

Business events should satisfy the following properties.

- immutable
- ordered
- durable when required
- uniquely identifiable
- replayable where supported

---

## 47.2 At-Least-Once Delivery

Consumers should assume events may be delivered more than once.

Business consumers must therefore be idempotent.

Duplicate delivery must never produce duplicate business state.

---

## 47.3 Ordering Scope

Ordering is guaranteed only within a single Reading Session.

For example:

```text
Session A

ReadingContextUpdated

↓

ContentRevisionCreated
```

must remain ordered.

However,

events from different Reading Sessions may interleave freely.

---

## 47.4 Event Replay

Previously published events may be replayed for:

- recovery
- diagnostics
- analytics
- projection rebuilding
- audit

Replay must never change historical event contents.

---

# 48. Event Correlation

Business events frequently participate in larger business flows.

Correlation metadata allows consumers to reconstruct these flows.

Typical metadata includes:

```text
CorrelationId

CausationId

SessionId

RevisionId

IntentId
```

Correlation metadata does not affect event semantics.

It only improves traceability.

---

## 48.1 CorrelationId

CorrelationId groups events belonging to the same logical business activity.

Example:

```text
PageChanged

↓

ReadingContextUpdated

↓

ContentRevisionCreated

↓

ProcessingIntentPublished
```

All events above may share one CorrelationId.

---

## 48.2 CausationId

CausationId identifies the event that directly caused another event.

Example:

```text
ReadingContextUpdated

↓

ContentRevisionCreated
```

The second event references the first as its causal predecessor.

This enables complete business event chains.

---

# 49. Event Consumers

Reading Session publishes events for other modules.

It does not dictate how those modules react.

Potential consumers include:

```text
Runtime

Recognition

Translation

Presentation

Analytics

History

Logging

Monitoring
```

Each consumer decides independently how to interpret business events.

Reading Session never assumes downstream behavior.

---

## 49.1 Consumer Independence

Business events are published without knowledge of subscribers.

Reading Session does not require:

- subscriber existence
- subscriber success
- subscriber implementation details

This preserves module independence.

---

## 49.2 Business Contract Stability

Consumers depend only on the published event contract.

Internal implementation changes within Reading Session must not alter event meaning.

Stable event contracts enable independent module evolution.

---

# 50. Event Invariants

The following guarantees always remain true.

---

## 50.1 Business Facts Never Change

Published events are immutable.

Corrections generate new events.

Existing events remain unchanged forever.

---

## 50.2 Event Ordering Is Deterministic

Given identical business history,

the same sequence of business events must always be produced.

---

## 50.3 Event Ownership Is Unique

Every event belongs to exactly one business module.

Reading Session never publishes Runtime events.

Runtime never publishes Reading Session events.

Ownership is unambiguous.

---

## 50.4 Events Never Execute Work

Business events describe completed facts.

They never request execution.

Incorrect:

```text
RunTranslation
```

Correct:

```text
ProcessingIntentPublished
```

---

## 50.5 Events Never Replace State

Events explain what happened.

State explains what currently exists.

Both are required,

but they serve different architectural purposes.

---

## 50.6 Historical Events Remain Valid

Older events remain historically correct,

even when newer revisions replace previous business authority.

Historical truth is never rewritten.

---

# 51. Related Documents

This specification works together with the remaining Reading Session documentation.

```text
README.md

MODULE.md

CONTRACT.md

STATES.md

ERRORS.md
```

Responsibilities are divided as follows.

| Document | Responsibility |
|----------|----------------|
| README | Module overview |
| MODULE | Responsibilities and ownership |
| CONTRACT | Public APIs and contracts |
| STATES | Business lifecycle |
| EVENTS | Business facts and event contracts |
| ERRORS | Business error model |

Each document owns a separate architectural concern.

---

# 52. Summary

Reading Session communicates exclusively through immutable business events.

The module defines five event categories.

```text
Reading Session Events

├── Session Lifecycle Events
├── Reading Context Events
├── Content Revision Events
├── Processing Intent Events
└── Configuration Events
```

These events describe **what has happened** in the business domain.

They never describe execution,

worker activity,

processing pipelines,

or infrastructure behavior.

Together with `STATES.md`, this document establishes a clear separation between:

- **Business State** (what currently exists)
- **Business Events** (what has happened)

This separation enables deterministic behavior, append-only business history, independent module evolution, and complete decoupling from Runtime execution.

---

# End of Document