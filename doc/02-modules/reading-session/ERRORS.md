# Reading Session Errors

- Module: Reading Session
- Identifier: reading-session
- Layer: Business Orchestration
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

This document defines all business error contracts owned by the Reading Session Module.

Unlike Runtime errors,

Reading Session errors describe failures in business consistency,

business validation,

or business lifecycle progression.

This specification defines:

- business error ownership
- error categories
- stable error codes
- severity levels
- recovery policies
- logging requirements
- architectural guarantees

---

# 2. Error Philosophy

Reading Session owns only business errors.

It never reports implementation failures occurring inside Runtime.

Examples of Reading Session errors:

✔ Invalid Session

✔ Invalid Reading Context

✔ Invalid Revision

✔ Invalid Business Transition

✔ ProcessingIntent cannot be created

Examples that are NOT Reading Session errors:

✘ OCR timeout

✘ Translation provider failure

✘ Worker crashed

✘ Queue unavailable

✘ GPU memory exhausted

Those belong to Runtime.

---

## 2.1 Errors Represent Business Failures

Errors describe why a requested business operation cannot be accepted.

They never expose infrastructure details.

---

## 2.2 Stable Error Contracts

Consumers depend on stable ErrorCode values.

Consumers must never depend on:

- exception names
- stack traces
- implementation messages

Only ErrorCode is part of the public contract.

---

## 2.3 Errors Never Corrupt Business State

When an error occurs,

Reading Session guarantees that:

- current business state remains valid
- previous revisions remain immutable
- business history remains consistent

Errors never partially update business state.

---

## 2.4 Runtime Isolation

Failures occurring inside Runtime remain Runtime responsibilities.

Reading Session may observe that:

```text
Business outcome unavailable
```

It never reports:

```text
Translation provider timeout

OCR crashed

GPU unavailable
```

---

# 3. Error Ownership

Reading Session owns only business errors.

---

## Reading Session Owns

```text
Business Validation

Session Lifecycle

Reading Context

Content Revision

Processing Intent

Configuration

Business Consistency
```

---

## Reading Session Does Not Own

```text
Worker Errors

Queue Errors

Scheduler Errors

Recognition Errors

Translation Errors

Presentation Errors

Provider Errors

Storage Errors
```

---

# 4. Error Categories

Business errors are grouped according to business ownership.

```text
Reading Session Errors

├── Validation
├── Session
├── Reading Context
├── Content Revision
├── Processing Intent
├── Configuration
├── Consistency
└── Internal
```

Each category represents one business responsibility.

---

# 5. Error Code Convention

Every business error follows a stable identifier.

```text
SES-<CATEGORY>-<NUMBER>
```

Examples:

```text
SES-VALIDATION-001

SES-SESSION-002

SES-CONTEXT-001

SES-REVISION-004

SES-INTENT-002

SES-CONFIG-001

SES-CONSISTENCY-001

SES-INTERNAL-001
```

Error codes never change after publication.

---

# 6. Severity Levels

Reading Session classifies errors into four levels.

| Severity | Meaning |
|----------|---------|
| Info | Expected business condition |
| Warning | Invalid business request |
| Error | Business operation failed |
| Critical | Business invariant violated |

Severity never determines recovery policy.

Recovery is defined independently.

---

# 7. Recovery Policies

Each error defines one recovery recommendation.

| Policy | Meaning |
|---------|---------|
| Never | Retry will never help |
| AfterCorrection | Retry after correcting business input |
| Reevaluate | Recompute business state |
| RestartSession | Create a new Reading Session |
| ManualIntervention | Human action required |

Recovery policies describe business recovery,

not Runtime retry behavior.

---

# 8. Validation Errors

Validation errors occur before Reading Session modifies any business state.

They indicate that the requested operation cannot be evaluated because the supplied business input is invalid.

Validation errors never modify:

- Session
- Reading Context
- Content Revision
- Processing Intent

---

## 8.1 SES-VALIDATION-001 MissingSessionIdentifier

Meaning

The requested operation does not specify a SessionId.

Severity

Warning

Recovery

AfterCorrection

Business State

Unchanged

---

## 8.2 SES-VALIDATION-002 InvalidSessionIdentifier

Meaning

The supplied SessionId is syntactically or semantically invalid.

Severity

Warning

Recovery

AfterCorrection

Business State

Unchanged

---

## 8.3 SES-VALIDATION-003 UnsupportedOperation

Meaning

The requested business operation is not supported by Reading Session.

Severity

Warning

Recovery

Never

Business State

Unchanged

---

## 8.4 SES-VALIDATION-004 MissingConfiguration

Meaning

The required SessionConfiguration is unavailable.

Severity

Warning

Recovery

AfterCorrection

Business State

Unchanged

---

## 8.5 SES-VALIDATION-005 InvalidConfiguration

Meaning

SessionConfiguration violates business validation rules.

Examples:

- unsupported language combination
- invalid reading mode
- incompatible business options

Severity

Warning

Recovery

AfterCorrection

Business State

Unchanged

---

# 9. Session Errors

Session errors describe failures related to Reading Session lifecycle management.

---

## 9.1 SES-SESSION-001 SessionNotFound

Meaning

The requested Reading Session does not exist.

Severity

Warning

Recovery

AfterCorrection

Business State

Unchanged

---

## 9.2 SES-SESSION-002 SessionAlreadyExists

Meaning

A Reading Session with the same identity already exists.

Severity

Warning

Recovery

Never

Business State

Unchanged

---

## 9.3 SES-SESSION-003 SessionAlreadyActive

Meaning

The Session is already Active.

The requested activation has no business effect.

Severity

Info

Recovery

Never

Business State

Unchanged

---

## 9.4 SES-SESSION-004 SessionAlreadyCompleted

Meaning

The Session has already reached the Completed lifecycle state.

No further business progression is allowed.

Severity

Info

Recovery

RestartSession

Business State

Completed

---

## 9.5 SES-SESSION-005 SessionAlreadyCancelled

Meaning

The Session has already been cancelled.

Business progression cannot resume.

Severity

Info

Recovery

RestartSession

Business State

Cancelled

---

## 9.6 SES-SESSION-006 SessionDisposed

Meaning

The Session has already been disposed.

Disposed Sessions cannot receive new business operations.

Severity

Info

Recovery

RestartSession

Business State

Disposed

---

# 10. Reading Context Errors

Reading Context errors occur when the current business understanding of the reading world cannot support the requested operation.

---

## 10.1 SES-CONTEXT-001 ReadingContextUnavailable

Meaning

No Reading Context currently exists.

Severity

Warning

Recovery

Reevaluate

Business State

Unchanged

---

## 10.2 SES-CONTEXT-002 ReadingContextInvalid

Meaning

The current Reading Context is invalid.

Business evaluation cannot continue.

Severity

Error

Recovery

Reevaluate

Business State

Unchanged

---

## 10.3 SES-CONTEXT-003 ReadingContextDisposed

Meaning

The requested Reading Context has already been disposed.

Severity

Warning

Recovery

Reevaluate

Business State

Disposed

---

## 10.4 SES-CONTEXT-004 ReadingContextMismatch

Meaning

The supplied Reading Context does not match the active Reading Session.

Severity

Warning

Recovery

AfterCorrection

Business State

Unchanged

---

# 11. Content Revision Errors

Content Revision errors describe failures related to immutable business snapshots.

---

## 11.1 SES-REVISION-001 RevisionNotFound

Meaning

The requested ContentRevision does not exist.

Severity

Warning

Recovery

AfterCorrection

Business State

Unchanged

---

## 11.2 SES-REVISION-002 RevisionAlreadyCurrent

Meaning

The requested revision is already the current business authority.

Severity

Info

Recovery

Never

Business State

Unchanged

---

## 11.3 SES-REVISION-003 RevisionSuperseded

Meaning

The revision has already been replaced by a newer revision.

It remains historically valid,

but may no longer participate in active business evaluation.

Severity

Info

Recovery

Reevaluate

Business State

Unchanged

---

## 11.4 SES-REVISION-004 RevisionArchived

Meaning

The revision has already been archived.

Archived revisions cannot become active again.

Severity

Info

Recovery

Never

Business State

Archived

---

## 11.5 SES-REVISION-005 RevisionDiscarded

Meaning

The revision has been permanently discarded.

Severity

Warning

Recovery

Never

Business State

Discarded

---

## 11.6 SES-REVISION-006 DuplicateRevision

Meaning

An identical immutable revision already exists.

Severity

Warning

Recovery

Never

Business State

Unchanged

---

# 12. Processing Intent Errors

Processing Intent errors occur when Reading Session cannot correctly create, publish, or evaluate a business intention.

These errors describe business failures only.

They never describe Runtime execution failures.

---

## 12.1 SES-INTENT-001 ProcessingIntentNotFound

Meaning

The requested ProcessingIntent does not exist.

Severity

Warning

Recovery

AfterCorrection

Business State

Unchanged

---

## 12.2 SES-INTENT-002 ProcessingIntentAlreadyPublished

Meaning

The ProcessingIntent has already been published.

Publishing the same business intent again would violate business consistency.

Severity

Info

Recovery

Never

Business State

Unchanged

---

## 12.3 SES-INTENT-003 ProcessingIntentObsolete

Meaning

The ProcessingIntent has already become obsolete.

A newer business intention has replaced it.

Severity

Info

Recovery

Reevaluate

Business State

Unchanged

---

## 12.4 SES-INTENT-004 ProcessingIntentFulfilled

Meaning

The requested ProcessingIntent has already been fulfilled.

No additional business action is required.

Severity

Info

Recovery

Never

Business State

Fulfilled

---

## 12.5 SES-INTENT-005 ProcessingIntentDiscarded

Meaning

The ProcessingIntent has already been discarded.

Discarded intents cannot become active again.

Severity

Warning

Recovery

Never

Business State

Discarded

---

## 12.6 SES-INTENT-006 ProcessingIntentCannotBeCreated

Meaning

Reading Session cannot generate a valid ProcessingIntent because business preconditions are not satisfied.

Typical causes include:

- ReadingContext is invalid
- no active ContentRevision
- Session is not Active

Severity

Error

Recovery

Reevaluate

Business State

Unchanged

---

# 13. Configuration Errors

Configuration errors occur when business configuration conflicts with Reading Session requirements.

---

## 13.1 SES-CONFIG-001 UnsupportedLanguage

Meaning

The requested language is not supported by the current Reading Session configuration.

Severity

Warning

Recovery

AfterCorrection

Business State

Unchanged

---

## 13.2 SES-CONFIG-002 UnsupportedReadingMode

Meaning

The requested reading mode is unsupported.

Severity

Warning

Recovery

AfterCorrection

Business State

Unchanged

---

## 13.3 SES-CONFIG-003 InvalidConfigurationCombination

Meaning

The supplied business configuration contains incompatible options.

Examples:

- mutually exclusive translation strategies
- conflicting reading preferences
- unsupported configuration combinations

Severity

Warning

Recovery

AfterCorrection

Business State

Unchanged

---

## 13.4 SES-CONFIG-004 ConfigurationVersionMismatch

Meaning

The supplied configuration version does not match the active SessionConfiguration.

Severity

Warning

Recovery

Reevaluate

Business State

Unchanged

---

# 14. Consistency Errors

Consistency errors indicate that one or more architectural invariants have been violated.

These errors should be extremely rare.

Most indicate a defect in implementation rather than user input.

---

## 14.1 SES-CONSISTENCY-001 MultipleCurrentRevisions

Meaning

More than one ContentRevision is marked as Current.

This violates the Reading Session state model.

Severity

Critical

Recovery

ManualIntervention

Business State

Unknown

---

## 14.2 SES-CONSISTENCY-002 MultipleActiveContexts

Meaning

More than one ReadingContext is simultaneously Ready.

Exactly one active ReadingContext is permitted.

Severity

Critical

Recovery

ManualIntervention

Business State

Unknown

---

## 14.3 SES-CONSISTENCY-003 InvalidLifecycleTransition

Meaning

A lifecycle transition violates the transition rules defined in `STATES.md`.

Examples:

```text
Completed

↓

Active
```

or

```text
Disposed

↓

Ready
```

Severity

Critical

Recovery

ManualIntervention

Business State

Unchanged

---

## 14.4 SES-CONSISTENCY-004 BusinessHistoryCorrupted

Meaning

Business history can no longer be reconstructed deterministically.

Examples include:

- missing revisions
- duplicate revision sequence
- invalid event ordering

Severity

Critical

Recovery

ManualIntervention

Business State

Unknown

---

## 14.5 SES-CONSISTENCY-005 AggregateVersionConflict

Meaning

The aggregate version supplied for a business operation is inconsistent with the current aggregate state.

Severity

Error

Recovery

Reevaluate

Business State

Unchanged

---

# 15. Internal Errors

Internal errors represent failures inside Reading Session itself.

They do not describe Runtime failures.

Internal errors indicate that Reading Session cannot safely continue normal business operation.

---

## 15.1 SES-INTERNAL-001 InternalFailure

Meaning

An unexpected internal failure occurred.

The exact implementation detail is intentionally hidden.

Severity

Critical

Recovery

ManualIntervention

Business State

Unknown

---

## 15.2 SES-INTERNAL-002 InvariantViolation

Meaning

One or more architectural invariants have been violated.

Reading Session can no longer guarantee deterministic behavior.

Severity

Critical

Recovery

ManualIntervention

Business State

Unknown

---

## 15.3 SES-INTERNAL-003 AtomicCommitFailed

Meaning

A business state transition could not be committed atomically.

No partial business update is allowed.

Severity

Critical

Recovery

ManualIntervention

Business State

Previous consistent state retained

---

## 15.4 SES-INTERNAL-004 EventPublicationFailed

Meaning

Reading Session failed to publish a required business event.

Business state remains unchanged until publication consistency can be guaranteed.

Severity

Critical

Recovery

ManualIntervention

Business State

Unchanged

---

# 16. Error Mapping

Every Reading Session error maps to a deterministic business outcome.

The same error must always produce the same business behavior.

---

## 16.1 Validation Errors

| Error Category | Business Result |
|----------------|-----------------|
| MissingSessionIdentifier | Reject request |
| InvalidSessionIdentifier | Reject request |
| UnsupportedOperation | Reject request |
| MissingConfiguration | Reject request |
| InvalidConfiguration | Reject request |

Validation errors never modify business state.

---

## 16.2 Session Errors

| Error | Business Result |
|--------|-----------------|
| SessionNotFound | Reject operation |
| SessionAlreadyExists | Reject creation |
| SessionAlreadyActive | Ignore request |
| SessionAlreadyCompleted | Reject operation |
| SessionAlreadyCancelled | Reject operation |
| SessionDisposed | Reject operation |

The Session lifecycle remains unchanged.

---

## 16.3 Reading Context Errors

| Error | Business Result |
|--------|-----------------|
| ReadingContextUnavailable | Stop business evaluation |
| ReadingContextInvalid | Stop business evaluation |
| ReadingContextDisposed | Reject operation |
| ReadingContextMismatch | Reject operation |

No new ContentRevision should be created until the Reading Context becomes valid.

---

## 16.4 Revision Errors

| Error | Business Result |
|--------|-----------------|
| RevisionNotFound | Reject operation |
| RevisionAlreadyCurrent | Ignore request |
| RevisionSuperseded | Ignore obsolete operation |
| RevisionArchived | Reject activation |
| RevisionDiscarded | Reject operation |
| DuplicateRevision | Ignore duplicate |

Revision history remains immutable.

---

## 16.5 Processing Intent Errors

| Error | Business Result |
|--------|-----------------|
| ProcessingIntentNotFound | Reject operation |
| ProcessingIntentAlreadyPublished | Ignore request |
| ProcessingIntentObsolete | Ignore obsolete operation |
| ProcessingIntentFulfilled | Ignore request |
| ProcessingIntentDiscarded | Reject operation |
| ProcessingIntentCannotBeCreated | Abort business evaluation |

ProcessingIntent history remains append-only.

---

## 16.6 Configuration Errors

| Error | Business Result |
|--------|-----------------|
| UnsupportedLanguage | Reject configuration |
| UnsupportedReadingMode | Reject configuration |
| InvalidConfigurationCombination | Reject configuration |
| ConfigurationVersionMismatch | Reevaluate Session |

Historical revisions remain unchanged.

---

## 16.7 Consistency Errors

Consistency errors always take precedence over operational errors.

Whenever a business invariant is violated,

Reading Session must stop producing new business objects until consistency has been restored.

---

## 16.8 Internal Errors

Internal errors indicate Reading Session can no longer guarantee deterministic business behavior.

Consumers should assume that:

- the current request failed;
- no partial business update occurred;
- previously committed business history remains valid.

---

# 17. Logging

Business logging exists to explain business decisions,

not implementation details.

Logs should make it possible to reconstruct the complete business timeline.

---

## 17.1 Required Fields

Every business error log should include:

```text
ErrorCode

Severity

SessionId

ContextId

RevisionId

IntentId

CorrelationId

OccurredAt
```

Additional identifiers may be recorded,

provided they remain implementation-independent.

---

## 17.2 Optional Fields

Depending on the business operation,

logs may also include:

```text
AggregateVersion

ConfigurationVersion

OperationName

BusinessAction

EventId
```

---

## 17.3 Sensitive Information

Business logs must never expose:

```text
Captured Image

OCR Result

Translated Text

Prompt

Authentication Token

Secret Key

Provider Credential

User Personal Content
```

Reading Session records business metadata only.

---

## 17.4 Logging Principles

Business logs should satisfy the following principles.

- deterministic
- structured
- privacy-preserving
- append-only
- traceable
- implementation-independent

---

# 18. Metrics

Reading Session exposes business-oriented metrics.

Infrastructure metrics belong to Runtime.

---

## 18.1 Recommended Metrics

```text
reading_session_created_total

reading_session_completed_total

reading_session_cancelled_total

reading_context_invalid_total

content_revision_created_total

content_revision_superseded_total

processing_intent_created_total

processing_intent_published_total

processing_intent_obsolete_total

business_error_total

business_consistency_error_total
```

These metrics describe business behavior,

not execution performance.

---

## 18.2 Metrics Principles

Business metrics should answer questions such as:

- How many reading sessions were created?
- How many sessions completed successfully?
- How often do Reading Contexts become invalid?
- How many Content Revisions are generated?
- How many ProcessingIntent objects become obsolete?

They should never answer questions such as:

- OCR latency
- GPU utilization
- Translation provider response time
- Worker queue length

Those belong to Runtime monitoring.

---

# 19. Error Invariants

The following guarantees always remain true.

---

## 19.1 Business State Safety

Errors never partially modify business state.

Either the requested business operation succeeds completely,

or the previous business state remains unchanged.

---

## 19.2 Immutable History

Errors never rewrite:

- ReadingContext history
- ContentRevision history
- ProcessingIntent history
- Session history

Historical business facts remain valid.

---

## 19.3 Stable Error Codes

Published ErrorCode values remain stable across versions.

Internal implementation may change,

but business contracts must remain compatible.

---

## 19.4 Runtime Isolation

Reading Session never exposes Runtime implementation failures.

Consumers receive only business error contracts.

---

## 19.5 Deterministic Recovery

Given identical business state,

the same recovery policy must always be recommended.

Recovery advice is therefore deterministic.

---

## 19.6 Business Ownership

Every business error belongs to exactly one module.

Reading Session never reports errors owned by:

- Runtime
- Recognition
- Translation
- Presentation
- Scheduler
- Provider

Likewise,

those modules never publish Reading Session business errors.

---

# 20. Related Documents

This specification complements the remaining Reading Session architecture documents.

```text
README.md

MODULE.md

CONTRACT.md

STATES.md

EVENTS.md
```

Responsibilities are divided as follows.

| Document | Responsibility |
|----------|----------------|
| README | Module overview |
| MODULE | Ownership and responsibilities |
| CONTRACT | Public contracts |
| STATES | Business lifecycle |
| EVENTS | Business events |
| ERRORS | Business failure model |

Each document owns one architectural concern.

---

# 21. Summary

Reading Session defines business error contracts independently from Runtime.

Business errors describe why a business operation cannot continue,

while Runtime errors describe how execution failed.

The module defines eight error categories.

```text
Reading Session Errors

├── Validation
├── Session
├── Reading Context
├── Content Revision
├── Processing Intent
├── Configuration
├── Consistency
└── Internal
```

The architecture guarantees that:

- business errors are deterministic;
- ErrorCode values remain stable;
- business history is never corrupted by failures;
- Runtime failures remain isolated;
- recovery policies are predictable;
- business ownership is explicit.

Together with `MODULE.md`, `CONTRACT.md`, `STATES.md`, and `EVENTS.md`, this document completes the core behavioral specification of the Reading Session module.

---

# End of Document