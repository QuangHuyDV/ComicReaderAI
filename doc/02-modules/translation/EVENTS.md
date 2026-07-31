# Translation Events

> **Project:** CRAI
> **Module:** Translation
> **Document:** Integration Events
> **Path:** `modules/translation/EVENTS.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-25
> **Source of Truth:**
>
> * `modules/translation/MODULE.md`
> * `modules/translation/CONTRACT.md`

---

## 1. Purpose

This document defines the integration events published by the Translation module.

These events communicate facts that have already occurred, including:

* translation job creation;
* job scheduling and execution;
* translation attempts;
* batch execution;
* progressive translation output;
* completed translation results;
* failures;
* cancellation;
* supersession;
* invalidation;
* translation variant changes;
* user corrections.

Events allow other modules to react without directly coupling themselves to Translation internals.

This document does not define:

* commands;
* query contracts;
* provider-specific callbacks;
* internal method calls;
* complete state transition tables;
* error implementation details;
* database records.

---

## 2. Event Principles

Translation events must follow these principles.

### 2.1 Events represent facts

Event names use past-tense semantics.

Correct:

```text
TranslationJobCreated
TranslationAttemptStarted
TranslationCompleted
TranslationCancelled
```

Incorrect:

```text
StartTranslation
TranslateDocument
CancelTranslation
RetryTranslation
```

The incorrect examples express commands, not events.

---

### 2.2 Events are immutable

After publication, an event must never be modified.

Corrections or later changes require new events.

---

### 2.3 Events are provider-neutral

Public events must not expose:

* provider-native request bodies;
* provider-native response bodies;
* raw prompts;
* SDK models;
* API keys;
* access tokens;
* authorization headers.

Normalized provider identifiers and execution metadata may be included when operationally necessary.

---

### 2.4 Events preserve traceability

Every job-related event must contain enough identity information to associate it with:

```text
TranslationJob
PreparedDocument
ContentRevision
```

Attempt and batch events must additionally include their parent identities.

---

### 2.5 Events are not full entity snapshots by default

Events should contain:

* stable identifiers;
* the state change;
* compact state information;
* references to larger results;
* information needed by expected consumers.

Large documents and full result bodies should not be copied into every event.

---

### 2.6 Events may be delivered more than once

Consumers must assume at-least-once delivery unless the Event Bus contract states otherwise.

Every consumer must be idempotent.

---

### 2.7 Event order cannot be globally assumed

Consumers may rely only on explicit stream ordering guarantees.

They must not assume that events from unrelated jobs arrive in chronological order.

---

### 2.8 Stale events must not reactivate stale work

A consumer must verify revision and authority information before displaying or activating translation results.

---

## 3. Event Categories

Translation publishes five primary event categories:

```text
Job Events
Attempt Events
Batch Events
Result Events
Variant and Correction Events
```

Additional operational events may be kept internal.

---

## 4. Public Event Set

The initial public event set is:

```text
TranslationJobCreated
TranslationJobQueued
TranslationJobStarted
TranslationProgressUpdated

TranslationAttemptStarted
TranslationAttemptFailed
TranslationAttemptCompleted

TranslationBatchStarted
TranslationBatchCompleted
TranslationBatchFailed

TranslationPartialResultAvailable
TranslationSegmentCompleted
TranslationCompleted
TranslationCompletedWithWarnings
TranslationFailed

TranslationCancellationRequested
TranslationCancelled
TranslationSuperseded
TranslationInvalidated

TranslationRetryScheduled
TranslationProviderFallbackSelected

TranslationVariantCreated
TranslationVariantActivated
TranslationVariantInvalidated

TranslationCorrectionSubmitted
TranslationCorrectionApplied
```

Not every consumer should subscribe to every event.

---

# Part I — Event Envelope

## 5. TranslationEventEnvelope

Every public Translation event must use the common event envelope defined by the CRAI Event Bus architecture.

Conceptual shape:

```text
TranslationEventEnvelope<TPayload> {
    eventId
    eventType
    eventVersion

    occurredAt
    publishedAt

    producer
    subject

    correlationId
    causationId
    traceContext

    partitionKey
    sequence

    payload
}
```

---

## 6. eventId

Uniquely identifies one event instance.

```text
eventId
```

Consumers use it for:

* deduplication;
* audit tracing;
* replay safety;
* diagnostic correlation.

An event retry must reuse the same `eventId`.

A new domain fact must use a new `eventId`.

---

## 7. eventType

Uses the canonical event name.

Example:

```text
translation.job.created
```

Recommended naming convention:

```text
<module>.<entity>.<fact>
```

Examples:

```text
translation.job.created
translation.attempt.started
translation.batch.completed
translation.result.partial-available
translation.variant.activated
```

---

## 8. eventVersion

Identifies the payload schema version.

Example:

```text
1
```

Schema versioning must not depend on provider versions.

A backward-incompatible payload change requires a new event version.

---

## 9. occurredAt

The time when the domain fact occurred.

This is not necessarily the same as publication time.

---

## 10. publishedAt

The time when the event was submitted to the Event Bus.

---

## 11. producer

Identifies the publishing module.

```text
producer {
    module = "translation"
    instanceId
}
```

The instance identifier is optional outside operational environments.

---

## 12. subject

Identifies the primary domain entity represented by the event.

Example:

```text
subject {
    type = "translation-job"
    id = TranslationJobId
}
```

For batch events:

```text
subject {
    type = "translation-batch"
    id = TranslationBatchId
}
```

---

## 13. correlationId

Connects events and commands belonging to one larger application flow.

Examples:

* translating one visible comic page;
* translating one novel viewport;
* retranslating after a glossary update;
* cancelling work after navigation.

---

## 14. causationId

Identifies the command or event that directly caused this event.

Examples:

```text
StartTranslationCommand.commandId
RetryTranslationCommand.commandId
TranslationAttemptFailed.eventId
```

---

## 15. traceContext

Carries distributed tracing information.

It must not contain business secrets or provider credentials.

---

## 16. partitionKey

Translation events should normally be partitioned by:

```text
TranslationJobId
```

This enables ordering within one job stream.

Batch and attempt events should use their parent `TranslationJobId` as the partition key.

---

## 17. sequence

Monotonically increasing sequence within the event stream of one translation job.

```text
sequence = 1, 2, 3, ...
```

The sequence supports:

* out-of-order detection;
* replay;
* read-model reconstruction;
* duplicate handling.

Sequence numbering is per job, not global.

---

# Part II — Common Payload Identity

## 18. TranslationEventIdentity

Most Translation event payloads include:

```text
TranslationEventIdentity {
    translationJobId

    readingSessionId

    preparedDocumentId
    contentRevision

    targetLanguage

    activeVariantId
}
```

Only fields relevant to the event are required.

---

## 19. Revision Requirements

Events that may affect displayed content must include:

```text
preparedDocumentId
contentRevision
translationJobId
```

Where applicable, they must also include:

```text
translationResultId
resultRevision
translationVariantId
targetLanguage
```

This protects consumers from stale event application.

---

## 20. Compact Progress Contract

Events may use:

```text
TranslationProgressSummary {
    totalSegmentCount
    completedSegmentCount
    failedSegmentCount
    pendingSegmentCount

    totalBatchCount
    completedBatchCount
    failedBatchCount
}
```

Progress events should not carry complete translated text.

---

## 21. Compact Failure Contract

Failure-bearing events may include:

```text
TranslationFailureSummary {
    code
    category
    retryable

    scope

    affectedBatchIds[]
    affectedPreparedSegmentIds[]
}
```

Human-readable details are optional.

The complete failure contract belongs in `ERRORS.md`.

---

## 22. Compact Warning Contract

Warning-bearing events may include:

```text
TranslationWarningSummary {
    code
    category
    severity

    affectedPreparedSegmentIds[]
}
```

Warnings should avoid copying full source or translated text.

---

# Part III — Job Events

## 23. TranslationJobCreated

Published after a new logical translation job has been accepted and persisted.

Event type:

```text
translation.job.created
```

Payload:

```text
TranslationJobCreatedPayload {
    translationJobId

    readingSessionId

    preparedDocumentId
    contentRevision
    selectedPreparedSegmentIds[]

    sourceLanguage
    targetLanguage
    translationProfileId

    priority
    publicationMode

    createdAt
}
```

### Meaning

The job exists but may not yet have started execution.

### Expected consumers

* Reading Session;
* translation progress UI;
* observability;
* job scheduler;
* audit components.

### Must not include

* full source document;
* provider request;
* provider credentials;
* raw glossary content.

---

## 24. TranslationJobQueued

Published when a created job enters an execution queue.

Event type:

```text
translation.job.queued
```

Payload:

```text
TranslationJobQueuedPayload {
    translationJobId

    priority
    queueClass

    queuedAt
}
```

Possible queue classes:

```text
INTERACTIVE
VISIBLE
PREFETCH
BACKGROUND
```

This event may be internal if consumers do not need queue visibility.

---

## 25. TranslationJobStarted

Published when the job begins active execution.

Event type:

```text
translation.job.started
```

Payload:

```text
TranslationJobStartedPayload {
    translationJobId

    translationAttemptId

    startedAt

    totalSegmentCount
    plannedBatchCount
}
```

### Meaning

At least one attempt is now active or is being prepared.

It does not mean that a provider request has already been sent.

---

## 26. TranslationProgressUpdated

Published when meaningful job progress changes.

Event type:

```text
translation.job.progress-updated
```

Payload:

```text
TranslationProgressUpdatedPayload {
    translationJobId

    activeAttemptId

    progress

    partialResultAvailable

    updatedAt
}
```

### Publication rules

Do not publish one event for every provider token.

Publish only when:

* a batch completes;
* one or more aligned segments become available;
* a batch fails;
* progress changes meaningfully;
* a configured progress interval is reached.

### Coalescing

Implementations may coalesce rapid progress updates.

---

# Part IV — Attempt Events

## 27. TranslationAttemptStarted

Published when an execution attempt begins.

Event type:

```text
translation.attempt.started
```

Payload:

```text
TranslationAttemptStartedPayload {
    translationJobId
    translationAttemptId

    attemptNumber
    reason

    providerId
    modelIdentifier

    plannedBatchCount

    startedAt
}
```

Possible reasons:

```text
INITIAL
AUTOMATIC_RETRY
PROVIDER_FALLBACK
BATCH_RETRY
VALIDATION_RETRY
MANUAL_RETRY
```

Provider information is normalized and optional when selection has not yet completed.

---

## 28. TranslationAttemptCompleted

Published when an attempt reaches successful completion for its assigned work.

Event type:

```text
translation.attempt.completed
```

Payload:

```text
TranslationAttemptCompletedPayload {
    translationJobId
    translationAttemptId

    completedBatchCount
    translatedSegmentCount

    usageSummary

    completedAt
}
```

An attempt may complete without the whole job completing when:

* the attempt processed only failed batches;
* another attempt already contributed successful batches;
* result assembly remains pending.

---

## 29. TranslationAttemptFailed

Published when an attempt cannot continue.

Event type:

```text
translation.attempt.failed
```

Payload:

```text
TranslationAttemptFailedPayload {
    translationJobId
    translationAttemptId

    failure

    completedBatchIds[]
    failedBatchIds[]

    partialOutputAvailable

    failedAt
}
```

### Meaning

The attempt failed.

The job may still:

* retry;
* select fallback;
* complete partially;
* fail finally.

Consumers must not treat this event alone as final job failure.

---

# Part V — Batch Events

## 30. TranslationBatchStarted

Published when one translation batch starts provider execution.

Event type:

```text
translation.batch.started
```

Payload:

```text
TranslationBatchStartedPayload {
    translationJobId
    translationAttemptId
    translationBatchId

    batchSequence

    preparedSegmentIds[]

    providerId
    modelIdentifier

    startedAt
}
```

### Visibility

This event may be public for:

* detailed progress;
* observability;
* debugging;
* distributed workers.

For simple deployments, it may remain internal.

---

## 31. TranslationBatchCompleted

Published after a batch response has been received, structurally validated, and accepted.

Event type:

```text
translation.batch.completed
```

Payload:

```text
TranslationBatchCompletedPayload {
    translationJobId
    translationAttemptId
    translationBatchId

    batchSequence

    completedPreparedSegmentIds[]
    translatedSegmentIds[]

    warnings[]

    providerExecutionSummary
    usageSummary

    completedAt
}
```

### Important rule

This event must not be published immediately after receiving an unvalidated provider response.

It is published only after:

* response parsing;
* identifier validation;
* alignment validation;
* minimum output validation.

---

## 32. TranslationBatchFailed

Published when one batch cannot produce an accepted result in the current attempt.

Event type:

```text
translation.batch.failed
```

Payload:

```text
TranslationBatchFailedPayload {
    translationJobId
    translationAttemptId
    translationBatchId

    preparedSegmentIds[]

    failure
    retryable

    partialOutputAccepted

    failedAt
}
```

### Meaning

The batch failed in this attempt.

The job may retry the batch later.

---

# Part VI — Segment and Partial Result Events

## 33. TranslationSegmentCompleted

Published when one validated translated segment becomes available for progressive consumption.

Event type:

```text
translation.segment.completed
```

Payload:

```text
TranslationSegmentCompletedPayload {
    translationJobId
    translationAttemptId
    translationBatchId

    translationResultId
    resultRevision

    translationVariantId

    preparedDocumentId
    contentRevision

    preparedSegmentId
    translatedSegmentId
    sourceSequence

    targetLanguage

    completion
    warnings[]

    translatedTextReference

    completedAt
}
```

---

## 34. translatedTextReference

The event should prefer a result reference:

```text
translatedTextReference {
    translationResultId
    resultRevision
    translatedSegmentId
}
```

Embedding `translatedText` directly may be allowed for low-latency local delivery when:

* payload size is bounded;
* privacy policy permits it;
* Event Bus transport is trusted;
* consumers require immediate display.

The architecture should not require text embedding.

---

## 35. Segment Event Granularity

`TranslationSegmentCompleted` is useful for:

* progressive comic bubble overlays;
* progressive novel rendering;
* low-latency interactive reading.

However, publishing one event per segment may be excessive for large documents.

Implementations may instead publish grouped segment events.

---

## 36. TranslationSegmentsCompleted

Optional grouped event:

```text
translation.segments.completed
```

Payload:

```text
TranslationSegmentsCompletedPayload {
    translationJobId
    translationAttemptId
    translationBatchId

    translationResultId
    resultRevision
    translationVariantId

    preparedDocumentId
    contentRevision

    segments[] {
        preparedSegmentId
        translatedSegmentId
        sourceSequence
        completion
        warningCodes[]
    }

    completedAt
}
```

The grouped event is preferred when many segments complete together.

A deployment should avoid publishing both individual and grouped events for the same facts unless consumers explicitly require both.

---

## 37. TranslationPartialResultAvailable

Published when a validated partial result becomes available.

Event type:

```text
translation.result.partial-available
```

Payload:

```text
TranslationPartialResultAvailablePayload {
    translationJobId

    translationResultId
    resultRevision
    translationVariantId

    preparedDocumentId
    contentRevision

    progress

    completedPreparedSegmentIds[]
    missingPreparedSegmentIds[]
    failedPreparedSegmentIds[]

    warnings[]

    authoritative
    publicationAllowed

    availableAt
}
```

### authoritative

Normally `false` until publication policy permits activation.

For progressive mode, a partial result may be authoritative for only the completed segment subset.

---

## 38. Partial Result Consumer Rule

Consumers must not assume:

```text
partial result = final result
```

Consumers must preserve:

* result revision;
* segment identity;
* completion state;
* job identity.

Later result revisions may add or replace non-authoritative partial information.

---

# Part VII — Final Result Events

## 39. TranslationCompleted

Published when the selected source set has been translated successfully and the final result is available.

Event type:

```text
translation.completed
```

Payload:

```text
TranslationCompletedPayload {
    translationJobId

    translationResultId
    resultRevision
    translationVariantId

    preparedDocumentId
    contentRevision

    sourceLanguage
    targetLanguage

    translatedSegmentCount

    resultReference

    statistics

    authoritative
    activated

    completedAt
}
```

### Publication condition

This event is published only after:

* result assembly;
* alignment validation;
* stale-result verification;
* cancellation check;
* supersession check;
* final publication decision.

---

## 40. TranslationCompletedWithWarnings

Published when translation completes but contains warnings significant enough for consumers.

Event type:

```text
translation.completed-with-warnings
```

Payload:

```text
TranslationCompletedWithWarningsPayload {
    translationJobId

    translationResultId
    resultRevision
    translationVariantId

    preparedDocumentId
    contentRevision

    translatedSegmentCount

    warnings[]

    resultReference

    authoritative
    activated

    completedAt
}
```

### Relationship with TranslationCompleted

A single completion should normally publish either:

```text
TranslationCompleted
```

or:

```text
TranslationCompletedWithWarnings
```

It should not publish both for the same completion transition.

---

## 41. TranslationFailed

Published when a job reaches final failure and no further automatic execution is planned.

Event type:

```text
translation.failed
```

Payload:

```text
TranslationFailedPayload {
    translationJobId

    preparedDocumentId
    contentRevision

    finalAttemptId

    failure

    completedPreparedSegmentIds[]
    missingPreparedSegmentIds[]
    failedPreparedSegmentIds[]

    partialResultId
    partialResultRevision

    retryAllowed

    failedAt
}
```

### Important distinction

`TranslationAttemptFailed` means one attempt failed.

`TranslationFailed` means the logical job reached final failure.

---

# Part VIII — Retry and Fallback Events

## 42. TranslationRetryScheduled

Published when another attempt has been scheduled for the same job.

Event type:

```text
translation.retry.scheduled
```

Payload:

```text
TranslationRetryScheduledPayload {
    translationJobId

    failedAttemptId
    nextAttemptNumber

    retryScope
    retryReason

    affectedBatchIds[]

    scheduledAt
    notBefore
}
```

Possible scopes:

```text
FAILED_BATCHES
ACTIVE_ATTEMPT
ENTIRE_JOB
```

---

## 43. TranslationProviderFallbackSelected

Published when the module selects another provider after a failure or policy decision.

Event type:

```text
translation.provider.fallback-selected
```

Payload:

```text
TranslationProviderFallbackSelectedPayload {
    translationJobId

    previousAttemptId
    previousProviderId

    nextProviderId

    fallbackIndex
    reason

    selectedAt
}
```

This event must not expose:

* credentials;
* raw provider errors;
* provider request bodies.

---

# Part IX — Cancellation Events

## 44. TranslationCancellationRequested

Published after a valid cancellation request has been accepted.

Event type:

```text
translation.cancellation.requested
```

Payload:

```text
TranslationCancellationRequestedPayload {
    translationJobId

    scope
    reason

    requestedBy
    requestedAt
}
```

### Meaning

Logical cancellation has been requested.

Physical provider execution may still be stopping.

---

## 45. TranslationCancelled

Published when the Translation job has entered its terminal cancelled state.

Event type:

```text
translation.cancelled
```

Payload:

```text
TranslationCancelledPayload {
    translationJobId

    activeAttemptId

    reason

    completedPreparedSegmentIds[]
    discardedPreparedSegmentIds[]

    partialResultId
    partialResultRetained

    cancelledAt
}
```

### Authority rule

Any result arriving after this event must remain non-authoritative.

---

# Part X — Supersession and Invalidation Events

## 46. TranslationSuperseded

Published when newer work replaces the authority of an existing job.

Event type:

```text
translation.superseded
```

Payload:

```text
TranslationSupersededPayload {
    translationJobId

    supersededByTranslationJobId

    preparedDocumentId
    contentRevision

    reason

    supersededAt
}
```

Possible reasons:

```text
NEW_SOURCE_REVISION
NEW_TRANSLATION_JOB
TARGET_LANGUAGE_CHANGED
PROFILE_CHANGED
READING_CONTEXT_CHANGED
MANUAL_RETRANSLATION
```

A superseded result may be retained historically.

It must not become active later without an explicit new decision.

---

## 47. TranslationInvalidated

Published when previously usable translation data is marked invalid.

Event type:

```text
translation.invalidated
```

Payload:

```text
TranslationInvalidatedPayload {
    translationJobId

    translationVariantId
    translationResultId

    scope

    invalidatedPreparedSegmentIds[]

    reason

    historicalDataRetained
    cacheEntryInvalidated

    invalidatedAt
}
```

Possible scopes:

```text
JOB
RESULT
VARIANT
SEGMENTS
CACHE_ENTRY
```

---

## 48. Supersession Versus Invalidation

Supersession means:

```text
newer work replaced older work
```

Invalidation means:

```text
existing work is no longer considered valid
```

Examples:

```text
New translation for revision 8 replaces revision 7
    → supersession

Alignment bug found in revision 7 result
    → invalidation
```

---

# Part XI — Variant Events

## 49. TranslationVariantCreated

Published after a new immutable translation variant has been created.

Event type:

```text
translation.variant.created
```

Payload:

```text
TranslationVariantCreatedPayload {
    translationJobId
    translationVariantId

    parentVariantId
    variantType

    targetLanguage
    translationProfileId

    translatedSegmentCount

    createdBy
    createdAt
}
```

Possible variant types:

```text
PROVIDER_GENERATED
RETRANSLATED
LITERAL
NATURAL
USER_CORRECTED
SYSTEM_CORRECTED
IMPORTED
```

---

## 50. TranslationVariantActivated

Published when a variant becomes active for a reading context.

Event type:

```text
translation.variant.activated
```

Payload:

```text
TranslationVariantActivatedPayload {
    translationJobId
    translationVariantId

    readingSessionId

    preparedDocumentId
    contentRevision

    targetLanguage

    previousActiveVariantId

    activatedBy
    activatedAt
}
```

Presentation and Reading Session may use this event to refresh displayed translation.

---

## 51. TranslationVariantInvalidated

Published when one immutable variant is no longer eligible for activation.

Event type:

```text
translation.variant.invalidated
```

Payload:

```text
TranslationVariantInvalidatedPayload {
    translationJobId
    translationVariantId

    reason

    wasActive
    replacementVariantId

    invalidatedAt
}
```

Invalidation does not delete the variant unless retention policy separately requires deletion.

---

# Part XII — Correction Events

## 52. TranslationCorrectionSubmitted

Published after a correction request has been accepted.

Event type:

```text
translation.correction.submitted
```

Payload:

```text
TranslationCorrectionSubmittedPayload {
    translationJobId

    baseVariantId

    correctedPreparedSegmentIds[]

    submittedBy
    submittedAt

    knowledgeProposalRequested
}
```

Corrected text should generally not be embedded in the event.

---

## 53. TranslationCorrectionApplied

Published after a corrected immutable variant has been created.

Event type:

```text
translation.correction.applied
```

Payload:

```text
TranslationCorrectionAppliedPayload {
    translationJobId

    baseVariantId
    correctedVariantId

    correctedPreparedSegmentIds[]

    activated

    knowledgeProposalId

    appliedAt
}
```

Translation does not imply that the correction has updated the global Knowledge module.

---

# Part XIII — Cache Events

## 54. Public Cache Events

Cache details are primarily internal.

The following event may be exposed for observability when needed:

```text
TranslationCacheResultReused
```

Event type:

```text
translation.cache.result-reused
```

Payload:

```text
TranslationCacheResultReusedPayload {
    translationJobId

    translationResultId
    translationVariantId

    cacheScope

    sourceContentHash
    targetLanguage
    translationProfileId

    reusedAt
}
```

Possible cache scopes:

```text
SAME_SESSION
CROSS_SESSION
PROVIDER_SPECIFIC
PROVIDER_INDEPENDENT
```

This event should not expose the cache key if it contains sensitive or implementation-specific data.

---

# Part XIV — Events Consumed by Translation

## 55. Upstream Events

Translation may consume events from other modules.

Expected upstream events include:

```text
PreparedDocumentCompleted
PreparedDocumentRevised
PreparedDocumentInvalidated

ReadingSessionNavigated
ReadingSessionClosed
VisibleContentChanged

KnowledgeSnapshotUpdated
GlossaryRevisionCreated

ProviderAvailabilityChanged
ProviderRateLimitChanged
```

Exact event names depend on the owning modules.

Translation must not redefine those events.

---

## 56. Prepared Document Completed

Translation may react to an upstream prepared-document completion event when:

* automatic translation is enabled;
* visible content requires translation;
* prefetch policy allows it.

Automatic reaction must still create a valid `StartTranslation` command internally.

An upstream event must not bypass command validation.

---

## 57. Prepared Document Revised

When source content revision changes, Translation may:

* cancel active work for older revisions;
* mark older jobs superseded;
* invalidate incompatible cache entries;
* start new translation when policy permits.

The older job must not publish authoritative output after supersession.

---

## 58. Reading Session Navigation

When the user navigates away, Translation may:

* lower job priority;
* cancel visible-only work;
* retain useful prefetch work;
* mark a job superseded;
* discard non-authoritative partial output.

The chosen behavior depends on reading and cache policy.

---

## 59. Reading Session Closed

Closing a reading session may trigger cancellation of session-bound jobs.

Cross-session cached results may be retained when privacy and cache policies permit.

---

## 60. Knowledge Snapshot Updated

A Knowledge update does not automatically mutate existing translations.

Depending on policy, Translation may:

* leave completed translations unchanged;
* invalidate affected variants;
* offer retranslation;
* schedule background retranslation;
* invalidate cache entries.

A new translation must reference the new Knowledge revision.

---

## 61. Provider Availability Changed

Provider availability events may affect:

* provider selection;
* queued attempts;
* fallback decisions;
* retry timing.

They must not change the semantic translation configuration of an existing job.

---

# Part XV — Event Ordering

## 62. Job Stream Ordering

Events for one Translation job should be published using:

```text
partitionKey = TranslationJobId
```

Within that stream, the `sequence` must increase monotonically.

Example:

```text
1  TranslationJobCreated
2  TranslationJobQueued
3  TranslationAttemptStarted
4  TranslationBatchStarted
5  TranslationBatchCompleted
6  TranslationPartialResultAvailable
7  TranslationCompleted
8  TranslationVariantActivated
```

---

## 63. Concurrent Batch Ordering

Multiple batches may run concurrently.

Therefore, valid arrival order may be:

```text
Batch 1 started
Batch 2 started
Batch 2 completed
Batch 1 completed
```

Consumers must reconstruct segment order using:

```text
sourceSequence
preparedSegmentId
```

not event arrival order.

---

## 64. Final Event Ordering Rule

Terminal events for one job include:

```text
TranslationCompleted
TranslationCompletedWithWarnings
TranslationFailed
TranslationCancelled
TranslationSuperseded
```

A job must not transition into more than one incompatible terminal outcome.

Later administrative invalidation remains possible after completion.

---

## 65. Late Provider Response

A provider response may arrive after:

* cancellation;
* supersession;
* timeout;
* a newer attempt;
* final job failure.

In that case:

* the response may be logged internally;
* usage may be recorded;
* diagnostic metadata may be retained;
* authoritative result events must not be emitted.

An optional internal event may record discarded output, but it should not normally be public.

---

# Part XVI — Event Delivery and Idempotency

## 66. At-Least-Once Delivery

Consumers must handle duplicate delivery.

Recommended deduplication key:

```text
eventId
```

Where a consumer writes aggregate state, it should also track:

```text
TranslationJobId
sequence
```

---

## 67. Duplicate Event Handling

Receiving the same event twice must not:

* render duplicate overlays;
* increment progress twice;
* create duplicate translation variants;
* repeat notifications;
* reactivate already active variants;
* trigger duplicate retries.

---

## 68. Out-of-Order Handling

When an event arrives with a lower sequence than already processed:

* ignore it if already applied;
* retain it for audit if required;
* do not roll back newer aggregate state.

When a sequence gap is detected, consumers may:

* wait briefly for missing events;
* query Translation for current state;
* rebuild from the authoritative query model.

---

## 69. Consumer Recovery

Events provide notification and integration.

They are not necessarily the sole source for UI reconstruction.

A consumer recovering after downtime should be able to call:

```text
GetTranslationJob
GetTranslationResult
GetActiveTranslation
```

to retrieve authoritative current state.

---

# Part XVII — Event Payload Size

## 70. Payload Size Principle

Translation events should remain compact.

Large data should be referenced using:

```text
translationResultId
resultRevision
translationVariantId
translatedSegmentId
```

rather than embedded repeatedly.

---

## 71. When Text May Be Embedded

Translated text may be embedded only when all of these are satisfied:

* event purpose requires immediate low-latency consumption;
* text size is bounded;
* privacy policy allows transport;
* the Event Bus supports the payload size;
* consumers are trusted;
* retention behavior is understood.

For large chapters or documents, use references.

---

## 72. Prohibited Payload Content

Events must not contain:

* raw provider prompts;
* full raw provider responses;
* provider credentials;
* authentication headers;
* unrelated page content;
* arbitrary browser state;
* entire Knowledge databases;
* large source images;
* raw OCR image data.

---

# Part XVIII — Privacy and Security

## 73. Content Minimization

Events should avoid source and translated text unless necessary.

Preferred event information:

```text
identifiers
revisions
counts
statuses
warning codes
failure codes
durations
usage summaries
```

---

## 74. Sensitive Reading Content

Reading content may be private.

Event retention policy must consider:

* whether translated text is stored in events;
* whether events are persisted;
* who may consume them;
* whether remote telemetry receives them;
* whether deletion requests must affect event projections.

---

## 75. Credential Safety

No event may contain:

```text
API keys
access tokens
refresh tokens
authorization headers
credential file paths
secret environment values
```

---

## 76. Prompt Injection Safety

Provider-produced text must not control event routing, event type, or metadata.

All event metadata must be created by trusted Translation code.

Provider output is always treated as data.

---

# Part XIX — Event Versioning

## 77. Backward-Compatible Changes

Examples:

* adding an optional field;
* adding an optional warning code;
* adding a new enum value when consumers tolerate unknown values;
* adding optional metadata.

These changes may retain the current event version when governance permits.

---

## 78. Breaking Changes

Examples:

* removing a required field;
* changing field meaning;
* changing identifier ownership;
* changing one event into another semantic fact;
* changing required ordering guarantees;
* replacing a reference with incompatible embedded data.

These require a new event version.

---

## 79. Unknown Fields and Enum Values

Consumers should ignore unknown optional fields.

Consumers must handle unknown enum values safely by:

* preserving them where possible;
* mapping them to `UNKNOWN`;
* not failing the whole event stream.

---

# Part XX — Event Subscription Guidance

## 80. Presentation Module

Presentation should primarily consume:

```text
TranslationPartialResultAvailable
TranslationSegmentsCompleted
TranslationCompleted
TranslationCompletedWithWarnings
TranslationVariantActivated
TranslationCancelled
TranslationSuperseded
TranslationInvalidated
```

Presentation should query the authoritative result when necessary.

---

## 81. Reading Session Module

Reading Session may consume:

```text
TranslationJobCreated
TranslationProgressUpdated
TranslationCompleted
TranslationFailed
TranslationCancelled
TranslationSuperseded
TranslationVariantActivated
```

Reading Session must not use Translation events to change source revision ownership.

---

## 82. Knowledge Module

Knowledge may consume:

```text
TranslationCorrectionSubmitted
TranslationCorrectionApplied
```

It may use them to initiate a separate review or terminology proposal workflow.

It must not automatically accept every correction as global truth.

---

## 83. Observability

Observability may consume:

```text
TranslationJobCreated
TranslationAttemptStarted
TranslationAttemptFailed
TranslationBatchCompleted
TranslationBatchFailed
TranslationCompleted
TranslationFailed
TranslationCancelled
TranslationProviderFallbackSelected
TranslationCacheResultReused
```

Observability should prefer identifiers and metrics over content.

---

## 84. Provider Management

Provider Management may consume normalized operational events such as:

```text
TranslationAttemptFailed
TranslationProviderFallbackSelected
```

It must not infer permanent provider health from one job failure alone.

---

# Part XXI — Event Publication Matrix

## 85. Required Events

The following events are required for the initial Translation implementation:

```text
TranslationJobCreated
TranslationJobStarted

TranslationAttemptStarted
TranslationAttemptFailed

TranslationPartialResultAvailable

TranslationCompleted
TranslationCompletedWithWarnings
TranslationFailed

TranslationCancelled
TranslationSuperseded
TranslationInvalidated

TranslationVariantCreated
TranslationVariantActivated
```

---

## 86. Recommended Events

```text
TranslationProgressUpdated
TranslationBatchCompleted
TranslationBatchFailed
TranslationRetryScheduled
TranslationProviderFallbackSelected
TranslationCorrectionApplied
```

---

## 87. Optional Operational Events

```text
TranslationJobQueued
TranslationBatchStarted
TranslationAttemptCompleted
TranslationSegmentCompleted
TranslationSegmentsCompleted
TranslationCacheResultReused
TranslationCancellationRequested
TranslationCorrectionSubmitted
```

A deployment should not publish high-volume optional events without a concrete consumer.

---

# Part XXII — Event Flow Examples

## 88. Successful Comic Translation

```text
TranslationJobCreated
        ↓
TranslationJobQueued
        ↓
TranslationJobStarted
        ↓
TranslationAttemptStarted
        ↓
TranslationBatchStarted
        ↓
TranslationBatchCompleted
        ↓
TranslationPartialResultAvailable
        ↓
TranslationBatchCompleted
        ↓
TranslationCompleted
        ↓
TranslationVariantCreated
        ↓
TranslationVariantActivated
```

Depending on implementation, variant creation may occur before the final completion event.

The chosen order must remain consistent.

Recommended order:

```text
Result assembled
      ↓
Variant created
      ↓
Job completed
      ↓
Variant activated
```

---

## 89. Retry After Timeout

```text
TranslationJobCreated
        ↓
TranslationAttemptStarted
        ↓
TranslationAttemptFailed
        ↓
TranslationRetryScheduled
        ↓
TranslationAttemptStarted
        ↓
TranslationCompleted
```

The same `TranslationJobId` is preserved.

A new `TranslationAttemptId` is created.

---

## 90. Provider Fallback

```text
TranslationAttemptStarted
        ↓
TranslationAttemptFailed
        ↓
TranslationProviderFallbackSelected
        ↓
TranslationRetryScheduled
        ↓
TranslationAttemptStarted
        ↓
TranslationCompletedWithWarnings
```

The completion warning may indicate that fallback was used.

---

## 91. Partial Success Then Final Failure

```text
TranslationBatchCompleted
        ↓
TranslationPartialResultAvailable
        ↓
TranslationBatchFailed
        ↓
TranslationAttemptFailed
        ↓
TranslationFailed
```

The final failure event references the retained partial result when policy permits.

---

## 92. User Navigates Away

```text
TranslationJobStarted
        ↓
ReadingSessionNavigated
        ↓
TranslationCancellationRequested
        ↓
TranslationCancelled
```

A later provider response must not produce `TranslationCompleted`.

---

## 93. Source Revision Changes

```text
TranslationJob A
    source revision = 7
        ↓
PreparedDocumentRevised
    source revision = 8
        ↓
TranslationJob A superseded
        ↓
TranslationJob B created
```

If Job A completes late, its result remains non-authoritative.

---

## 94. User Correction

```text
TranslationCorrectionSubmitted
        ↓
Corrected variant created
        ↓
TranslationCorrectionApplied
        ↓
TranslationVariantActivated
```

A separate Knowledge proposal may be created.

---

# Part XXIII — State Consistency Rules

## 95. Event and State Relationship

An event is published only after the associated state change has been committed or made durable enough for the architecture’s consistency model.

Avoid:

```text
publish event
    ↓
state update fails
```

Preferred patterns include:

* transactional outbox;
* durable event log;
* atomic state-and-event persistence;
* equivalent reliable publication mechanism.

---

## 96. No Phantom Completion

`TranslationCompleted` must not be published unless the result can subsequently be retrieved through the Translation query contract.

---

## 97. No Phantom Variant

`TranslationVariantCreated` must not be published unless the referenced variant exists and is retrievable.

---

## 98. No Premature Batch Completion

`TranslationBatchCompleted` requires accepted validated output.

Receiving an HTTP success response is not sufficient.

---

## 99. No Premature Cancellation Completion

`TranslationCancelled` should indicate the logical terminal state has been recorded.

Physical provider cancellation may still be pending, but no authoritative result may be published.

---

# Part XXIV — Core Event Invariants

## 100. Invariant 1 — Facts only

Events describe completed domain facts, never requested actions.

## 101. Invariant 2 — Immutable events

Published events are never edited.

## 102. Invariant 3 — Stable job identity

Retry events preserve `TranslationJobId`.

## 103. Invariant 4 — New attempt identity

Each retry execution receives a new `TranslationAttemptId`.

## 104. Invariant 5 — Stable source traceability

Display-affecting events include prepared document and revision identity.

## 105. Invariant 6 — Batch completion follows validation

Unvalidated provider output never produces a completed batch event.

## 106. Invariant 7 — No stale authority

Cancelled or superseded work never produces an authoritative completion event.

## 107. Invariant 8 — Missing content is explicit

Partial and failed events identify missing or failed segment identities.

## 108. Invariant 9 — Provider isolation

Provider-native payloads never appear in public events.

## 109. Invariant 10 — Credential isolation

Credentials never appear in any event.

## 110. Invariant 11 — Event consumers are idempotent

Duplicate event delivery must not duplicate business effects.

## 111. Invariant 12 — Arrival order is not segment order

Consumers use source sequence and identifiers, not event arrival order.

## 112. Invariant 13 — Result references remain retrievable

Published result and variant references resolve through query contracts.

## 113. Invariant 14 — Terminal outcomes are consistent

One job cannot simultaneously be completed, failed, and cancelled.

Administrative invalidation may occur after a completed state.

---

# Part XXV — Open Decisions

## 114. Event Granularity

The project must later decide whether the default progressive event is:

```text
TranslationSegmentCompleted
```

or:

```text
TranslationSegmentsCompleted
```

Recommended default:

```text
TranslationSegmentsCompleted
```

because batch-level grouping reduces Event Bus traffic while retaining progressive presentation.

---

## 115. Embedded Text

The project must decide whether translated text is embedded in progressive events.

Recommended approach:

```text
local in-process event
    → may embed bounded translated text

persistent or distributed event
    → use result references
```

---

## 116. Batch Event Visibility

Batch events may remain internal for the MVP unless:

* distributed workers are introduced;
* detailed progress is required;
* provider diagnostics require them.

---

## 117. Progress Throttling

The exact throttling policy remains open.

Recommended baseline:

* publish when a batch completes;
* publish when completion status changes materially;
* do not publish provider-token progress.

---

## 118. Completion and Variant Event Order

Recommended order:

```text
TranslationVariantCreated
        ↓
TranslationCompleted
        ↓
TranslationVariantActivated
```

This ensures completion events reference an existing variant.

The exact transactional implementation must preserve retrievability.

---

## 119. Event Retention

Retention remains to be defined for:

* job lifecycle events;
* events containing text;
* correction events;
* provider usage metadata;
* privacy-sensitive reading history.

---

# Part XXVI — Related Documents

```text
modules/translation/MODULE.md
modules/translation/CONTRACT.md
modules/translation/ERRORS.md
modules/translation/STATES.md
modules/translation/README.md
```

Architecture references:

```text
docs/architecture/EVENT_BUS.md
docs/architecture/STATE_MACHINE.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
```

Upstream module references:

```text
modules/text-processing/MODULE.md
modules/text-processing/CONTRACTS.md
modules/text-processing/EVENTS.md
```

Future integration references:

```text
modules/reading-session/EVENTS.md
modules/presentation/EVENTS.md
modules/knowledge/EVENTS.md
modules/provider-management/EVENTS.md
```

---

# 120. Summary

The Translation module publishes events for five main concerns:

```text
Translation job lifecycle
Translation execution attempts
Translation batches
Translation results
Translation variants and corrections
```

The core event flow is:

```text
TranslationJobCreated
        ↓
TranslationAttemptStarted
        ↓
TranslationBatchCompleted
        ↓
TranslationPartialResultAvailable
        ↓
TranslationVariantCreated
        ↓
TranslationCompleted
        ↓
TranslationVariantActivated
```

Failure and replacement flows include:

```text
TranslationAttemptFailed
TranslationRetryScheduled
TranslationProviderFallbackSelected
TranslationFailed
TranslationCancelled
TranslationSuperseded
TranslationInvalidated
```

Every public event must remain:

* immutable;
* provider-neutral;
* revision-aware;
* idempotently consumable;
* safe for duplicate delivery;
* compact by default;
* free from credentials;
* traceable to its translation job and prepared source revision.

Events notify consumers that translation state changed.

Queries remain the authoritative way to retrieve current Translation state and complete result data.
